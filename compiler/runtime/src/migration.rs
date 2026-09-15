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
//! | UID only in the candidate | None: the candidate's init image has already initialized it |
//! | UID only in the base | None: the value belongs to a dropped entity |
//! | UID in both, different entry | [`MigrationError::IncompatibleEntry`] |
//!
//! ## Data-region payloads
//!
//! A STRING/WSTRING or aggregate variable's 8-byte slot holds the byte offset
//! of its data-region region. The plan sizes the copy from the container's
//! type data, never from the runtime header:
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
//! ## Function-block instances (POC safety rule)
//!
//! An FB instance's slot holds the offset of its field region, but the field
//! values themselves are not UID-covered yet: they are addressed through the
//! type section's FB descriptors. The plan therefore copies an FB instance
//! like a scalar only when the layout after the program prefix is
//! index-identical: the program prefix size
//! (`task_table.shared_globals_size`), the user FB descriptors and the FB type
//! descriptors must match. A candidate that changes any of them while either
//! container has an FB instance in its stable variable IDs is rejected with
//! [`MigrationError::FbLayoutUnsupported`] rather than risking a slot that
//! points at another instance's fields.
//!
//! ## Applying a plan
//!
//! [`StateMigrationPlan::apply`] copies from the active [`VmBuffers`] into a
//! candidate buffer set that has already run the candidate's init image, so
//! entities the plan does not mention keep their initialized candidate
//! values. The host runs the copy at a scan boundary.

use core::fmt;
use std::collections::HashMap;
use std::vec::Vec;

use ironplc_container::{
    string_region_size, CharWidth, Container, FbTypeDescriptor, FieldType, StableVarEntry,
    TypeSection, UserFbDescriptor, VarEntry, VarIndex, VAR_FLAG_IS_ARRAY,
};
use ironplc_vm::VmBuffers;

/// Byte offset of the `cur_length` field of a data-region string header.
///
/// The full header is `[max_length: u16][cur_length: u16][char_width: u16]`
/// (`ironplc_container::string_layout`); only the current length is read
/// here, to check it against the candidate's maximum length.
const STRING_CUR_LENGTH_OFFSET: usize = 2;

/// One copy the plan performs at the scan boundary.
///
/// `Slot` covers scalars and, under the FB safety rule, FB instances. The
/// other variants carry the data-region region size computed at build time.
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
}

/// Why a state migration could not be built or applied.
///
/// The variants are deliberately structural: the host surfaces the reason
/// through [`OnlineChangeError::MigrationUnsupported`](crate::OnlineChangeError)
/// and never guesses a migration it cannot justify.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum MigrationError {
    /// A UID present in both containers is bound to a different [`VarEntry`].
    IncompatibleEntry { uid: u64, reason: &'static str },
    /// The active string value does not fit the candidate's maximum length.
    StringShrink {
        current_length: u16,
        candidate_max_length: u16,
    },
    /// The array descriptor backing the UID changed, or is missing.
    ArrayDescriptorMismatch { uid: u64 },
    /// An FB instance's tail layout changed; per-field migration is not
    /// supported yet.
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
                "function-block instance layout changed; only rename or reorder edits are supported for FB instances"
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
/// Build the plan with [`StateMigrationPlan::build`] while the normal
/// artifact is running; apply it with [`StateMigrationPlan::apply`] at the
/// scan boundary to the candidate's freshly initialized buffers.
#[derive(Debug)]
pub struct StateMigrationPlan {
    actions: Vec<MigrationAction>,
}

impl StateMigrationPlan {
    /// Diffs the active (`base`) and staged (`candidate`) containers by UID.
    ///
    /// Returns the plan when every UID the two containers share names the
    /// same variable type and layout; otherwise the edit cannot preserve
    /// those values and is rejected with a typed error. UIDs unique to either
    /// side produce no action: candidate-only entities are initialized by the
    /// candidate's init image and base-only entities are discarded with the
    /// old buffers.
    pub fn build(base: &Container, candidate: &Container) -> Result<Self, MigrationError> {
        let base_section = base.type_section.as_ref();
        let candidate_section = candidate.type_section.as_ref();

        let base_stable = stable_vars(base_section);
        let candidate_stable = stable_vars(candidate_section);
        let base_variables = variable_table(base_section);
        let candidate_variables = variable_table(candidate_section);

        let base_by_uid = index_by_uid(base_stable)?;
        index_by_uid(candidate_stable)?;

        let mut actions = Vec::new();
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
                return Err(MigrationError::IncompatibleEntry {
                    uid: entry.uid,
                    reason: entry_difference(base_var, candidate_var),
                });
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

        check_fb_safety(base, candidate, base_stable, candidate_stable)?;

        Ok(StateMigrationPlan { actions })
    }

    /// Copies every planned value from `base` into `candidate`.
    ///
    /// `candidate` must be the candidate container's freshly initialized
    /// buffers; the plan overwrites only the entities it migrates, leaving
    /// the init image of every other entity in place.
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
    let base_descriptor =
        base_section.and_then(|section| section.array_descriptors.get(descriptor_index));
    let candidate_descriptor =
        candidate_section.and_then(|section| section.array_descriptors.get(descriptor_index));
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

/// Enforces the FB instance safety rule (see the module documentation).
fn check_fb_safety(
    base: &Container,
    candidate: &Container,
    base_stable: &[StableVarEntry],
    candidate_stable: &[StableVarEntry],
) -> Result<(), MigrationError> {
    let base_section = base.type_section.as_ref();
    let candidate_section = candidate.type_section.as_ref();

    let base_has_instance = has_fb_instance(base_stable, variable_table(base_section))?;
    let candidate_has_instance =
        has_fb_instance(candidate_stable, variable_table(candidate_section))?;
    if !base_has_instance && !candidate_has_instance {
        return Ok(());
    }

    if base.task_table.shared_globals_size != candidate.task_table.shared_globals_size
        || !same_user_fb_descriptors(
            user_fb_descriptors(base_section),
            user_fb_descriptors(candidate_section),
        )
        || !same_fb_descriptors(
            fb_descriptors(base_section),
            fb_descriptors(candidate_section),
        )
    {
        return Err(MigrationError::FbLayoutUnsupported);
    }
    Ok(())
}

/// Whether any stable variable ID names an FB instance.
fn has_fb_instance(
    stable: &[StableVarEntry],
    variables: &[VarEntry],
) -> Result<bool, MigrationError> {
    for entry in stable {
        if variable_at(variables, entry.var_index)?.var_type == FieldType::FbInstance {
            return Ok(true);
        }
    }
    Ok(false)
}

/// The user FB descriptors, or an empty slice without a type section.
fn user_fb_descriptors(section: Option<&TypeSection>) -> &[UserFbDescriptor] {
    section.map_or(&[], |section| section.user_fb_types.as_slice())
}

/// The FB type descriptors, or an empty slice without a type section.
fn fb_descriptors(section: Option<&TypeSection>) -> &[FbTypeDescriptor] {
    section.map_or(&[], |section| section.fb_types.as_slice())
}

/// Compares user FB descriptors as a set keyed by type ID.
///
/// Codegen emits the descriptors from a hash map, so their order is not part
/// of the container contract; only the descriptor contents are.
fn same_user_fb_descriptors(a: &[UserFbDescriptor], b: &[UserFbDescriptor]) -> bool {
    if a.len() != b.len() {
        return false;
    }
    let mut a = a.to_vec();
    let mut b = b.to_vec();
    a.sort_by_key(|descriptor| descriptor.type_id.raw());
    b.sort_by_key(|descriptor| descriptor.type_id.raw());
    a == b
}

/// Compares FB type descriptors as a set keyed by type ID.
fn same_fb_descriptors(a: &[FbTypeDescriptor], b: &[FbTypeDescriptor]) -> bool {
    if a.len() != b.len() {
        return false;
    }
    let mut a = a.to_vec();
    let mut b = b.to_vec();
    a.sort_by_key(|descriptor| descriptor.type_id.raw());
    b.sort_by_key(|descriptor| descriptor.type_id.raw());
    a == b
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

#[cfg(test)]
mod tests;
