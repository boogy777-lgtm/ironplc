//! State migration planner for declaration-level online change.
//!
//! Stage 1 online change accepts a candidate only when its `layout_hash`
//! equals the active artifact's, so it can carry the persistent buffers over
//! wholesale. A declaration edit changes the layout hash, but not necessarily
//! the entity that owns each value: ADR 0053 gives every persistent variable a
//! stable UID that survives renames, so the runtime can decide per variable
//! what to copy. [`StateMigrationPlan`] is that decision:
//!
//! | Case | Action |
//! |---|---|
//! | UID in both, identical [`VarEntry`] | Copy the slot, and the data-region region it points to for aggregates |
//! | UID in both, entry differs only by an admitted type change | Convert the value at the swap per the policy table (ADR 0060) |
//! | UID only in the candidate | None: the candidate's init image has already initialized it |
//! | UID only in the base | None: the value belongs to a dropped entity |
//! | UID in both, entry differs by a storage-class change | Convert per policy, or resolve with a [`MigrationDecision`] (ADR 0061); structural differences reject with [`MigrationError::IncompatibleEntry`] |
//!
//! ## Data-region payloads
//!
//! A STRING/WSTRING or aggregate variable's 8-byte slot holds the byte offset
//! of its data-region region. The migration plan sizes the copy from the
//! container's type data, never from the runtime header:
//!
//! - STRING/WSTRING: `STRING_HEADER_BYTES + max_length * char_width`, with
//!   the width taken from the type tag (STRING narrow, WSTRING wide). The
//!   copy also checks that the active value's `cur_length` (the second u16 of
//!   the region header, as the container format and VM define it) fits the
//!   candidate's `max_length`.
//! - Arrays and structures: [`ArrayDescriptor::byte_size`](ironplc_container::ArrayDescriptor::byte_size)
//!   of the candidate's descriptor at `VarEntry::extra`; the base descriptor
//!   for the same UID must be identical (`element_type`, `total_elements`,
//!   `element_extra`). Structures are flat arrays of [`FieldType::Slot`] and
//!   take this path too.
//!
//! ## Function-block instances (ADR 0059)
//!
//! An FB instance's slot holds the offset of its field region, and the field
//! values themselves are addressed through the type section's user FB
//! descriptors. The type section's FB field UID table gives each field of a
//! user-defined FB type a stable UID, so the planner no longer needs the
//! stage-2 POC's global "identical tail layout" rule (ADR 0054) for
//! user-defined instances. For every shared-UID instance of a user FB type
//! (a descriptor exists on both sides):
//!
//! | Layout | Action |
//! |---|---|
//! | Descriptors identical | Copy the slot and the whole field region (`num_fields * 8` bytes) |
//! | Descriptors differ | Match fields by UID; copy each shared-UID field's 8-byte slot, initialise candidate-only UIDs, drop base-only UIDs |
//!
//! A candidate field whose UID is unknown (no entry, or the reserved UID 0)
//! while the layout differs rejects the whole candidate with
//! [`MigrationError::FbLayoutUnsupported`] — the value's identity is
//! unprovable, so the planner fails closed rather than guess. Standard-
//! library FB instances (TON, ...), which have no user FB descriptor, keep
//! the stage-2 rule: when any unhandled instance exists on either side, the
//! layout after the program prefix (program prefix size, user FB descriptors,
//! FB type descriptors) must be identical.
//!
//! ## Type-changing variables (ADR 0060, ADR 0061)
//!
//! A shared UID whose entry differs only in `var_type` is a storage-class
//! change. The planner classifies it against the conversion policy in
//! [`conversion`]: an admitted `(base, candidate)` pair plans a
//! [`MigrationAction::Convert`] (scalar) or [`MigrationAction::ConvertArray`]
//! (an aggregate whose element pair is admitted, with equal element count
//! and element size), executed at the scan boundary.
//!
//! Every out-of-policy pair is collected instead of rejecting on the first
//! offender; without a decision the build fails with
//! [`MigrationError::TypeChangeUnsupported`] carrying all of them as
//! [`TypeChangePair`] values. The caller — the staged edit's engineering
//! client — answers per UID with a [`MigrationDecision`]:
//! [`MigrationDecision::Init`] plans no action, so the candidate's init
//! image initializes the value, and [`MigrationDecision::Preserve`] plans a
//! plain byte copy when both sides' storage sizes match (scalars: the same
//! numeric width family; arrays: equal element width and element count). A
//! decision may also override an admitted pair (init or preserve instead of
//! convert); the automatic conversion stays the default. Structural
//! differences (flags, or `extra` with the type tag unchanged — a STRING
//! maximum length or an FB type ID) keep the stage-2 rejection. FB instance
//! *fields* follow the same path via their field UIDs (ADR 0059).
//!
//! ## Applying a plan
//!
//! [`StateMigrationPlan::apply`] copies from the active [`VmBuffers`] into a
//! candidate buffer set that has already run the candidate's init image, so
//! entities the migration plan does not mention keep their initialized
//! candidate values. The host runs the copy at a scan boundary.

use core::fmt;
use std::collections::{BTreeMap, HashMap};
use std::vec::Vec;

use ironplc_container::{
    string_region_size, ArrayDescriptor, CharWidth, Container, FbTypeId, FieldType, StableVarEntry,
    TypeSection, VarEntry, VarIndex, VAR_FLAG_IS_ARRAY,
};
use ironplc_vm::{Slot, VmBuffers};

use crate::conversion::{type_name, ValueConversion};

/// Byte offset of the `cur_length` field of a data-region string header.
///
/// The full header is `[max_length: u16][cur_length: u16][char_width: u16]`
/// (`ironplc_container::string_layout`); only the current length is read
/// here, to check it against the candidate's maximum length.
const STRING_CUR_LENGTH_OFFSET: usize = 2;

/// One copy the migration plan performs at the scan boundary.
///
/// `Slot` covers scalars. The other variants carry the data-region sizes and
/// offsets computed at build time; an FB instance takes the whole-region
/// path only when its type's layout is identical on both sides, and the
/// per-field path when field UIDs cover the change.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum MigrationAction {
    /// Copy the 8-byte variable slot.
    Slot {
        from_index: VarIndex,
        to_index: VarIndex,
    },
    /// Copy the slot and the STRING/WSTRING region it points to.
    String {
        from_index: VarIndex,
        to_index: VarIndex,
        byte_size: u32,
        candidate_max_length: u16,
    },
    /// Copy the slot and the array/struct region it points to.
    Array {
        from_index: VarIndex,
        to_index: VarIndex,
        byte_size: u32,
    },
    /// Copy the slot and the instance's whole field region; used only when
    /// the instance's type layout is identical on both sides.
    FbInstance {
        from_index: VarIndex,
        to_index: VarIndex,
        byte_size: u32,
    },
    /// Copy one field's 8-byte slot inside the instance's field region,
    /// base field `from_field` to candidate field `to_field`. The instance's
    /// own slot is not copied: the candidate's init image already aimed it
    /// at the candidate's field region. `conversion` transforms the field
    /// value when the rebuilt FB type retyped it to an admitted pair
    /// (ADR 0060); `None` copies the slot unchanged.
    FbField {
        from_index: VarIndex,
        to_index: VarIndex,
        from_field: u8,
        to_field: u8,
        conversion: Option<ValueConversion>,
    },
    /// Convert the slot's value from the base variable's type to the
    /// candidate's per an admitted policy pair (ADR 0060).
    Convert {
        from_index: VarIndex,
        to_index: VarIndex,
        conversion: ValueConversion,
    },
    /// Convert each 8-byte element of the array/struct region the slot
    /// points to; used only when the descriptors' element pair is admitted
    /// by the policy and the element count and size match.
    ConvertArray {
        from_index: VarIndex,
        to_index: VarIndex,
        conversion: ValueConversion,
        byte_size: u32,
    },
}

/// Why a state migration could not be built or applied.
///
/// The variants are deliberately structural: the host surfaces the reason
/// through [`OnlineChangeError::MigrationUnsupported`](crate::OnlineChangeError)
/// and never guesses a migration it cannot justify.
#[derive(Clone, Debug, PartialEq, Eq)]
pub enum MigrationError {
    /// A UID present in both containers is bound to a different [`VarEntry`].
    IncompatibleEntry { uid: u64, reason: &'static str },
    /// Shared UIDs whose variable type changed to pairs the conversion policy
    /// does not admit (ADR 0060) and that no [`MigrationDecision`] resolved
    /// (ADR 0061); carries every offender.
    TypeChangeUnsupported { pairs: Vec<TypeChangePair> },
    /// A `preserve` decision is illegal for this pair: the base and candidate
    /// storage sizes differ, so the old bytes cannot be reinterpreted.
    PreserveSizeMismatch { uid: u64 },
    /// A decision map entry names a UID that is not a shared variable or
    /// field whose storage class changed, so there is nothing for it to
    /// decide.
    UnknownDecisionUid { uid: u64 },
    /// The active string value does not fit the candidate's maximum length.
    StringShrink {
        current_length: u16,
        candidate_max_length: u16,
    },
    /// The array descriptor backing the UID changed, or is missing.
    ArrayDescriptorMismatch { uid: u64 },
    /// An FB instance's tail layout changed and field UIDs cannot justify the
    /// change (a field carries no UID, or the instance is a standard-library
    /// FB whose layout is not UID-covered).
    FbLayoutUnsupported,
    /// A stable variable ID entry references an index outside the variable
    /// table.
    IndexOutOfRange { index: VarIndex },
    /// A variable's data-region region lies outside the buffers.
    RegionOutOfRange { index: VarIndex },
    /// Two stable variable ID entries share one UID.
    DuplicateUid { uid: u64 },
}

impl fmt::Display for MigrationError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            MigrationError::IncompatibleEntry { uid, reason } => {
                write!(f, "entity {uid} cannot migrate: {reason}")
            }
            MigrationError::TypeChangeUnsupported { pairs } => {
                write!(f, "type changes are outside the migration policy:")?;
                for (index, pair) in pairs.iter().enumerate() {
                    if index > 0 {
                        write!(f, ",")?;
                    }
                    write!(f, " entity {}", pair.uid)?;
                    if let Some(name) = &pair.name {
                        write!(f, " ({name})")?;
                    }
                    write!(f, " {} -> {}", type_name(pair.from), type_name(pair.to))?;
                }
                Ok(())
            }
            MigrationError::PreserveSizeMismatch { uid } => write!(
                f,
                "entity {uid} cannot preserve its storage: the base and candidate sizes differ"
            ),
            MigrationError::UnknownDecisionUid { uid } => write!(
                f,
                "migration decision names uid {uid}, which is not a shared entity whose type changed"
            ),
            MigrationError::StringShrink {
                current_length,
                candidate_max_length,
            } => write!(
                f,
                "string value of length {current_length} does not fit the candidate maximum {candidate_max_length}"
            ),
            MigrationError::ArrayDescriptorMismatch { uid } => {
                write!(f, "entity {uid} has a changed array descriptor")
            }
            MigrationError::FbLayoutUnsupported => write!(
                f,
                "function-block instance layout changed and field UIDs cannot justify the change"
            ),
            MigrationError::IndexOutOfRange { index } => {
                write!(f, "variable index {index} is out of range")
            }
            MigrationError::RegionOutOfRange { index } => {
                write!(
                    f,
                    "the data-region region for variable {index} is out of range"
                )
            }
            MigrationError::DuplicateUid { uid } => {
                write!(f, "stable variable uid {uid} is bound more than once")
            }
        }
    }
}

/// The per-variable state copies an online change performs at the boundary.
///
/// Build a migration plan with [`StateMigrationPlan::build`] while the
/// normal artifact is running; apply it with [`StateMigrationPlan::apply`]
/// at the scan boundary to the candidate's freshly initialized buffers.
#[derive(Debug)]
pub struct StateMigrationPlan {
    actions: Vec<MigrationAction>,
}

impl StateMigrationPlan {
    /// Diffs the active (`base`) and staged (`candidate`) containers by UID.
    ///
    /// Equivalent to [`build_with_decisions`](Self::build_with_decisions)
    /// with an empty decision map: every out-of-policy type change rejects,
    /// with all offending pairs carried in
    /// [`MigrationError::TypeChangeUnsupported`].
    pub fn build(base: &Container, candidate: &Container) -> Result<Self, MigrationError> {
        Self::build_with_decisions(base, candidate, &BTreeMap::new())
    }

    /// Diffs the active (`base`) and staged (`candidate`) containers by UID,
    /// resolving storage-class changes with the engineer's `decisions`.
    ///
    /// Returns a migration plan when every UID the two containers share names
    /// the same variable type and layout, or when every storage-class change
    /// is either admitted by the conversion policy (ADR 0060) or resolved by
    /// a [`MigrationDecision`] (ADR 0061). `decisions` maps a shared UID to
    /// its decision; empty behaves exactly like [`build`](Self::build). An
    /// unknown UID, or `preserve` on a size-mismatched pair, is rejected.
    /// UIDs unique to either side produce no action: candidate-only entities
    /// are initialized by the candidate's init image and base-only entities
    /// are discarded with the old buffers.
    pub fn build_with_decisions(
        base: &Container,
        candidate: &Container,
        decisions: &BTreeMap<u64, MigrationDecision>,
    ) -> Result<Self, MigrationError> {
        let base_section = base.type_section.as_ref();
        let candidate_section = candidate.type_section.as_ref();

        let base_stable = stable_vars(base_section);
        let candidate_stable = stable_vars(candidate_section);
        let base_variables = variable_table(base_section);
        let candidate_variables = variable_table(candidate_section);

        let base_by_uid = index_by_uid(base_stable)?;
        index_by_uid(candidate_stable)?;

        let mut decisions = decision::Decisions::new(decisions);
        let mut actions = Vec::new();
        // Shared-UID instances of user FB types are planned per type below,
        // not by the per-variable classification: their slot's entry can be
        // identical while the field layout behind it changed.
        let mut fb_instances: Vec<(VarIndex, VarIndex)> = Vec::new();
        for entry in candidate_stable {
            let to_index = entry.var_index;
            let candidate_var = variable_at(candidate_variables, to_index)?;
            let Some(&from_index) = base_by_uid.get(&entry.uid) else {
                // A candidate-only UID names a new entity; its initial value
                // comes from the candidate's init image.
                continue;
            };
            let base_var = variable_at(base_variables, from_index)?;
            if base_var != candidate_var {
                // The entries differ: reconcile the difference against the
                // conversion policy (ADR 0060) and the engineer's decisions
                // (ADR 0061), or reject with the typed error.
                decision::plan_type_change(
                    &mut actions,
                    &mut decisions,
                    decision::TypeChange {
                        uid: entry.uid,
                        name: decision::debug_name(candidate, to_index),
                        from_index,
                        to_index,
                        base_var,
                        candidate_var,
                        base_section,
                        candidate_section,
                    },
                )?;
                continue;
            }
            if candidate_var.var_type == FieldType::FbInstance
                && fb::user_fb_descriptor(base_section, FbTypeId::new(candidate_var.extra))
                    .is_some()
                && fb::user_fb_descriptor(candidate_section, FbTypeId::new(candidate_var.extra))
                    .is_some()
            {
                fb_instances.push((from_index, to_index));
                continue;
            }
            actions.push(copy_action(
                entry.uid,
                from_index,
                to_index,
                candidate_var,
                base_section,
                candidate_section,
            )?);
        }

        fb::plan_fb_instances(
            &mut actions,
            &mut decisions,
            base,
            candidate,
            base_stable,
            candidate_stable,
            &fb_instances,
        )?;
        decisions.finish()?;

        Ok(StateMigrationPlan { actions })
    }

    /// Copies every planned value from `base` into `candidate`.
    ///
    /// `candidate` must be the candidate container's freshly initialized
    /// buffers; the migration plan overwrites only the entities it migrates,
    /// leaving the init image of every other entity in place.
    pub fn apply(&self, base: &VmBuffers, candidate: &mut VmBuffers) -> Result<(), MigrationError> {
        for action in &self.actions {
            match *action {
                MigrationAction::Slot {
                    from_index,
                    to_index,
                } => {
                    copy_slot(base, candidate, from_index, to_index)?;
                }
                MigrationAction::String {
                    from_index,
                    to_index,
                    byte_size,
                    candidate_max_length,
                } => {
                    copy_data_region(
                        base,
                        candidate,
                        from_index,
                        to_index,
                        byte_size,
                        Some(candidate_max_length),
                    )?;
                }
                MigrationAction::Array {
                    from_index,
                    to_index,
                    byte_size,
                } => {
                    copy_data_region(base, candidate, from_index, to_index, byte_size, None)?;
                }
                MigrationAction::FbInstance {
                    from_index,
                    to_index,
                    byte_size,
                } => {
                    // Identical field layout: the region copy carries every
                    // field value, and the slot copy carries the region
                    // offset (equal on both sides, so a self-copy when the
                    // offsets match).
                    copy_data_region(base, candidate, from_index, to_index, byte_size, None)?;
                }
                MigrationAction::FbField {
                    from_index,
                    to_index,
                    from_field,
                    to_field,
                    conversion,
                } => {
                    copy_fb_field(
                        base, candidate, from_index, to_index, from_field, to_field, conversion,
                    )?;
                }
                MigrationAction::Convert {
                    from_index,
                    to_index,
                    conversion,
                } => {
                    convert_slot(base, candidate, from_index, to_index, conversion)?;
                }
                MigrationAction::ConvertArray {
                    from_index,
                    to_index,
                    conversion,
                    byte_size,
                } => {
                    convert_region(base, candidate, from_index, to_index, conversion, byte_size)?;
                }
            }
        }
        Ok(())
    }
}

/// Builds the UID -> variable index map, rejecting a UID bound twice.
fn index_by_uid(stable: &[StableVarEntry]) -> Result<HashMap<u64, VarIndex>, MigrationError> {
    let mut by_uid = HashMap::with_capacity(stable.len());
    for entry in stable {
        if by_uid.insert(entry.uid, entry.var_index).is_some() {
            return Err(MigrationError::DuplicateUid { uid: entry.uid });
        }
    }
    Ok(by_uid)
}

/// The stable variable ID table, or an empty slice without a type section.
fn stable_vars(section: Option<&TypeSection>) -> &[StableVarEntry] {
    section.map_or(&[], |section| section.stable_vars.as_slice())
}

/// The variable table, or an empty slice without a type section.
fn variable_table(section: Option<&TypeSection>) -> &[VarEntry] {
    section.map_or(&[], |section| section.variable_table.as_slice())
}

/// The variable table entry at `index`.
fn variable_at(variables: &[VarEntry], index: VarIndex) -> Result<&VarEntry, MigrationError> {
    variables
        .get(usize::from(index.raw()))
        .ok_or(MigrationError::IndexOutOfRange { index })
}

/// Names what changed between two entries for the diagnostic.
fn entry_difference(base: &VarEntry, candidate: &VarEntry) -> &'static str {
    if base.var_type != candidate.var_type {
        "the variable type changed"
    } else if base.flags != candidate.flags {
        "the variable's layout class changed"
    } else {
        "the variable's type details changed"
    }
}

/// Classifies one shared UID's copy from its (identical) candidate entry.
fn copy_action(
    uid: u64,
    from_index: VarIndex,
    to_index: VarIndex,
    entry: &VarEntry,
    base_section: Option<&TypeSection>,
    candidate_section: Option<&TypeSection>,
) -> Result<MigrationAction, MigrationError> {
    if entry.flags & VAR_FLAG_IS_ARRAY != 0 {
        return array_action(
            uid,
            from_index,
            to_index,
            entry,
            base_section,
            candidate_section,
        );
    }
    match entry.var_type {
        FieldType::String | FieldType::WString => Ok(string_action(from_index, to_index, entry)),
        _ => Ok(MigrationAction::Slot {
            from_index,
            to_index,
        }),
    }
}

/// Sizes a STRING/WSTRING copy from the entry's maximum length.
///
/// The shared entry guarantees the base and candidate regions are the same
/// size, so `byte_size` is their common size (the minimum the specification
/// asks for).
fn string_action(from_index: VarIndex, to_index: VarIndex, entry: &VarEntry) -> MigrationAction {
    let char_width = if entry.var_type == FieldType::WString {
        CharWidth::Wide
    } else {
        CharWidth::Narrow
    };
    MigrationAction::String {
        from_index,
        to_index,
        byte_size: string_region_size(entry.extra, char_width),
        candidate_max_length: entry.extra,
    }
}

/// Sizes an array or structure copy from the descriptor both containers share
/// at the entry's descriptor index.
fn array_action(
    uid: u64,
    from_index: VarIndex,
    to_index: VarIndex,
    entry: &VarEntry,
    base_section: Option<&TypeSection>,
    candidate_section: Option<&TypeSection>,
) -> Result<MigrationAction, MigrationError> {
    let descriptor_index = usize::from(entry.extra);
    let base_descriptor = array_descriptor(base_section, descriptor_index);
    let candidate_descriptor = array_descriptor(candidate_section, descriptor_index);
    let (Some(base_descriptor), Some(candidate_descriptor)) =
        (base_descriptor, candidate_descriptor)
    else {
        return Err(MigrationError::ArrayDescriptorMismatch { uid });
    };
    if base_descriptor != candidate_descriptor {
        return Err(MigrationError::ArrayDescriptorMismatch { uid });
    }
    let byte_size = candidate_descriptor
        .byte_size()
        .ok_or(MigrationError::IncompatibleEntry {
            uid,
            reason: "the array's byte size overflows a u32",
        })?;
    Ok(MigrationAction::Array {
        from_index,
        to_index,
        byte_size,
    })
}

/// The array descriptor at `descriptor_index`, if the section carries one.
fn array_descriptor(
    section: Option<&TypeSection>,
    descriptor_index: usize,
) -> Option<&ArrayDescriptor> {
    section.and_then(|section| section.array_descriptors.get(descriptor_index))
}

/// Copies the 8-byte variable slot at `to_index` from `from_index`.
fn copy_slot(
    base: &VmBuffers,
    candidate: &mut VmBuffers,
    from_index: VarIndex,
    to_index: VarIndex,
) -> Result<(), MigrationError> {
    let value = *base
        .vars
        .get(usize::from(from_index.raw()))
        .ok_or(MigrationError::IndexOutOfRange { index: from_index })?;
    let target = candidate
        .vars
        .get_mut(usize::from(to_index.raw()))
        .ok_or(MigrationError::IndexOutOfRange { index: to_index })?;
    *target = value;
    Ok(())
}

/// Converts the base slot's value and writes it to the candidate slot.
///
/// The candidate slot's prior value (its init image) is overwritten, exactly
/// like a copy; only the transformation differs (ADR 0060).
fn convert_slot(
    base: &VmBuffers,
    candidate: &mut VmBuffers,
    from_index: VarIndex,
    to_index: VarIndex,
    conversion: ValueConversion,
) -> Result<(), MigrationError> {
    let value = *base
        .vars
        .get(usize::from(from_index.raw()))
        .ok_or(MigrationError::IndexOutOfRange { index: from_index })?;
    let target = candidate
        .vars
        .get_mut(usize::from(to_index.raw()))
        .ok_or(MigrationError::IndexOutOfRange { index: to_index })?;
    *target = conversion.apply(value);
    Ok(())
}

/// Converts each 8-byte element of the base region into the candidate region.
///
/// Both slots hold their own region's byte offset; the candidate's slot is
/// left alone (its init image already aimed it at the candidate's region).
/// Admitted element pairs are non-string, so both regions are runs of 8-byte
/// slots and `byte_size` is a multiple of [`ironplc_container::SLOT_BYTES`].
fn convert_region(
    base: &VmBuffers,
    candidate: &mut VmBuffers,
    from_index: VarIndex,
    to_index: VarIndex,
    conversion: ValueConversion,
    byte_size: u32,
) -> Result<(), MigrationError> {
    let source_span = region_span(base, from_index, byte_size)?;
    let target_span = region_span(candidate, to_index, byte_size)?;
    let source = &base.data_region[source_span];
    let target = &mut candidate.data_region[target_span];

    // The descriptor sized the copy to whole 8-byte elements, so neither
    // remainder can be nonempty; every chunk pair is one element.
    let (source_chunks, _) = source.as_chunks::<{ ironplc_container::SLOT_BYTES as usize }>();
    let (target_chunks, _) = target.as_chunks_mut::<{ ironplc_container::SLOT_BYTES as usize }>();
    for (from, to) in source_chunks.iter().zip(target_chunks.iter_mut()) {
        let converted = conversion.apply(Slot::from_u64(u64::from_le_bytes(*from)));
        *to = converted.as_u64().to_le_bytes();
    }
    Ok(())
}

/// Copies a variable's data-region region from `base` to `candidate`.
///
/// Both slots hold the region's byte offset, so the copy is
/// offset-to-offset: a region that moved with its variable migrates, and one
/// that stayed in place is copied onto itself. The optional maximum length
/// applies the string-fit check before any byte moves.
fn copy_data_region(
    base: &VmBuffers,
    candidate: &mut VmBuffers,
    from_index: VarIndex,
    to_index: VarIndex,
    byte_size: u32,
    candidate_max_length: Option<u16>,
) -> Result<(), MigrationError> {
    let source_span = region_span(base, from_index, byte_size)?;
    let target_span = region_span(candidate, to_index, byte_size)?;

    if let Some(max_length) = candidate_max_length {
        let current_length = string_current_length(&base.data_region[source_span.clone()]);
        if current_length > max_length {
            return Err(MigrationError::StringShrink {
                current_length,
                candidate_max_length: max_length,
            });
        }
    }

    copy_slot(base, candidate, from_index, to_index)?;
    candidate.data_region[target_span].copy_from_slice(&base.data_region[source_span]);
    Ok(())
}

/// Copies one 8-byte field between the instances' field regions.
///
/// Each instance's slot holds its field region's byte offset, so the copy
/// resolves both sides through their own slot and moves the single field
/// the planner matched by UID. The instance slots themselves are left alone:
/// the candidate's init image already aimed its slot at the candidate's
/// field region, which is where the copied bytes land. `conversion` applies
/// when the rebuilt FB type retyped the field to an admitted pair (ADR
/// 0060); `None` copies the slot unchanged.
fn copy_fb_field(
    base: &VmBuffers,
    candidate: &mut VmBuffers,
    from_index: VarIndex,
    to_index: VarIndex,
    from_field: u8,
    to_field: u8,
    conversion: Option<ValueConversion>,
) -> Result<(), MigrationError> {
    let source = field_span(base, from_index, from_field)?;
    let target = field_span(candidate, to_index, to_field)?;
    match conversion {
        Some(conversion) => {
            let mut bytes = [0u8; ironplc_container::SLOT_BYTES as usize];
            bytes.copy_from_slice(&base.data_region[source]);
            let converted = conversion.apply(Slot::from_u64(u64::from_le_bytes(bytes)));
            candidate.data_region[target].copy_from_slice(&converted.as_u64().to_le_bytes());
        }
        None => candidate.data_region[target].copy_from_slice(&base.data_region[source]),
    }
    Ok(())
}

/// Resolves the 8-byte span one field occupies inside an instance's field
/// region, through the instance's slot.
fn field_span(
    buffers: &VmBuffers,
    index: VarIndex,
    field: u8,
) -> Result<core::ops::Range<usize>, MigrationError> {
    let slot = buffers
        .vars
        .get(usize::from(index.raw()))
        .ok_or(MigrationError::IndexOutOfRange { index })?;
    // The slot carries the region offset as an i32 (codegen stores
    // `data_offset as i32` and checks it fits); anything else is corrupt.
    let start =
        u32::try_from(slot.as_i64()).map_err(|_| MigrationError::RegionOutOfRange { index })?;
    let start = start as usize + usize::from(field) * ironplc_container::SLOT_BYTES as usize;
    let end = start
        .checked_add(ironplc_container::SLOT_BYTES as usize)
        .ok_or(MigrationError::RegionOutOfRange { index })?;
    if end > buffers.data_region.len() {
        return Err(MigrationError::RegionOutOfRange { index });
    }
    Ok(start..end)
}

/// Resolves the data-region region a variable's slot points to.
fn region_span(
    buffers: &VmBuffers,
    index: VarIndex,
    byte_size: u32,
) -> Result<core::ops::Range<usize>, MigrationError> {
    let slot = buffers
        .vars
        .get(usize::from(index.raw()))
        .ok_or(MigrationError::IndexOutOfRange { index })?;
    // The slot carries the region offset as an i32 (codegen stores
    // `data_offset as i32` and checks it fits); anything else is corrupt.
    let start =
        u32::try_from(slot.as_i64()).map_err(|_| MigrationError::RegionOutOfRange { index })?;
    let start = start as usize;
    let end = start
        .checked_add(byte_size as usize)
        .ok_or(MigrationError::RegionOutOfRange { index })?;
    if end > buffers.data_region.len() {
        return Err(MigrationError::RegionOutOfRange { index });
    }
    Ok(start..end)
}

/// Reads the `cur_length` field of a string region header.
fn string_current_length(region: &[u8]) -> u16 {
    let low = region
        .get(STRING_CUR_LENGTH_OFFSET)
        .copied()
        .unwrap_or_default();
    let high = region
        .get(STRING_CUR_LENGTH_OFFSET + 1)
        .copied()
        .unwrap_or_default();
    u16::from_le_bytes([low, high])
}

mod decision;
mod fb;

pub use decision::{MigrationDecision, TypeChangePair};

#[cfg(test)]
mod tests;
