//! Engineer-decided type-change planning for the state migration planner.
//!
//! ADR 0060 admits a fixed table of storage-class conversions; every other
//! `(base, candidate)` pair used to reject the candidate on the first
//! offender. ADR 0061 replaces that hard reject with a per-UID decision:
//! [`build_with_decisions`](super::StateMigrationPlan::build_with_decisions)
//! reports every offending pair in one [`MigrationError::TypeChangeUnsupported`],
//! and the caller answers each UID with [`MigrationDecision::Init`] (the
//! fail-closed default) or [`MigrationDecision::Preserve`].
//!
//! This module owns that classification — shared by program variables and FB
//! instance fields — plus the decision bookkeeping:
//!
//! - an admitted policy pair converts (ADR 0060) unless a decision overrides
//!   it;
//! - `init` plans no action, leaving the candidate's init image in place;
//! - `preserve` plans a plain byte copy, legal only when both sides' storage
//!   sizes match (scalars: the same numeric width family; arrays: equal
//!   element width and element count; ADR 0061), and rejects otherwise;
//! - a decision for a UID that names no such change is rejected, so the
//!   planner never silently ignores a stale decision.

use std::collections::{BTreeMap, BTreeSet};

use ironplc_container::{ArrayDescriptor, Container, FieldType, TypeSection, VarEntry, VarIndex};

use super::{array_descriptor, entry_difference, MigrationAction, MigrationError};
use crate::conversion::{self, ValueConversion};

/// The engineer's answer for one shared-UID storage-class change (ADR 0061).
///
/// Decisions are per-edit and transient: they resolve the one staged edit and
/// are not recorded in the UID sidecar, which records identity only.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum MigrationDecision {
    /// Discard the old value: the candidate's init image stands. The
    /// fail-closed default for an unchecked control.
    Init,
    /// Keep the old storage bytes and reinterpret them under the candidate
    /// type (Rockwell semantics). Legal only when both sides' storage sizes
    /// are equal; the value may no longer be valid.
    Preserve,
}

/// One shared-UID variable or FB field whose `(base, candidate)` storage
/// class changed outside the ADR-0060 policy, carried by
/// [`MigrationError::TypeChangeUnsupported`] (ADR 0061).
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct TypeChangePair {
    /// The entity's stable UID.
    pub uid: u64,
    /// The entity's debug name, when the candidate carries a debug section.
    pub name: Option<String>,
    /// The active artifact's storage class.
    pub from: FieldType,
    /// The candidate's storage class.
    pub to: FieldType,
    /// Whether [`MigrationDecision::Preserve`] is legal for this pair: both
    /// sides' storage sizes match (ADR 0061).
    pub size_equal: bool,
}

/// What the planner does with one shared-UID storage-class change.
pub(super) enum TypeChangeChoice {
    /// Convert the value per the ADR-0060 policy (the default).
    Convert(ValueConversion),
    /// Discard the old value; no action is planned.
    Init,
    /// Keep the bytes; the caller plans a plain copy.
    Preserve,
}

/// The decision map, the pairs collected so far, and the consumed decisions.
pub(super) struct Decisions<'a> {
    decisions: &'a BTreeMap<u64, MigrationDecision>,
    consumed: BTreeSet<u64>,
    pairs: Vec<TypeChangePair>,
}

impl<'a> Decisions<'a> {
    /// Wraps the caller's decision map; nothing is consumed yet.
    pub(super) fn new(decisions: &'a BTreeMap<u64, MigrationDecision>) -> Self {
        Decisions {
            decisions,
            consumed: BTreeSet::new(),
            pairs: Vec::new(),
        }
    }

    /// Classifies one storage-class change against the decisions and the
    /// ADR-0060 policy.
    ///
    /// `size_equal` says whether a preserve copy is legal for the pair; the
    /// caller computes it because arrays resolve it through their
    /// descriptors. An out-of-policy pair without a decision is recorded and
    /// resolved as [`TypeChangeChoice::Init`];
    /// [`finish`](Self::finish) then fails the build naming every recorded
    /// pair.
    pub(super) fn resolve(
        &mut self,
        uid: u64,
        name: Option<String>,
        from: FieldType,
        to: FieldType,
        size_equal: bool,
    ) -> Result<TypeChangeChoice, MigrationError> {
        match self.decisions.get(&uid) {
            Some(MigrationDecision::Init) => {
                self.consumed.insert(uid);
                Ok(TypeChangeChoice::Init)
            }
            Some(MigrationDecision::Preserve) => {
                self.consumed.insert(uid);
                if size_equal {
                    Ok(TypeChangeChoice::Preserve)
                } else {
                    Err(MigrationError::PreserveSizeMismatch { uid })
                }
            }
            None => match conversion::policy(from, to) {
                Some(conversion) => Ok(TypeChangeChoice::Convert(conversion)),
                None => {
                    self.pairs.push(TypeChangePair {
                        uid,
                        name,
                        from,
                        to,
                        size_equal,
                    });
                    Ok(TypeChangeChoice::Init)
                }
            },
        }
    }

    /// Validates the decisions map against the changes the build saw.
    ///
    /// A decision for a UID that names no storage-class change rejects: it
    /// cannot be applied and is not silently ignored. Otherwise, collected
    /// out-of-policy pairs fail the build naming all of them (ADR 0061).
    pub(super) fn finish(self) -> Result<(), MigrationError> {
        if let Some(&uid) = self
            .decisions
            .keys()
            .find(|uid| !self.consumed.contains(uid))
        {
            return Err(MigrationError::UnknownDecisionUid { uid });
        }
        if self.pairs.is_empty() {
            Ok(())
        } else {
            Err(MigrationError::TypeChangeUnsupported { pairs: self.pairs })
        }
    }
}

/// One shared-UID program variable whose entry changed, threaded to
/// [`plan_type_change`].
pub(super) struct TypeChange<'a> {
    /// The shared stable UID.
    pub(super) uid: u64,
    /// The variable's candidate debug name, when known.
    pub(super) name: Option<String>,
    /// The base container's variable index.
    pub(super) from_index: VarIndex,
    /// The candidate container's variable index.
    pub(super) to_index: VarIndex,
    /// The active artifact's variable-table entry.
    pub(super) base_var: &'a VarEntry,
    /// The candidate's variable-table entry.
    pub(super) candidate_var: &'a VarEntry,
    /// The active artifact's type section.
    pub(super) base_section: Option<&'a TypeSection>,
    /// The candidate's type section.
    pub(super) candidate_section: Option<&'a TypeSection>,
}

/// Plans the action a shared-UID program variable's storage-class change
/// resolves to.
///
/// The structural differences (layout flags, or the type details with the
/// type tag unchanged) reject as before; a storage-class change goes through
/// [`Decisions::resolve`].
pub(super) fn plan_type_change(
    actions: &mut Vec<MigrationAction>,
    decisions: &mut Decisions<'_>,
    change: TypeChange<'_>,
) -> Result<(), MigrationError> {
    if change.base_var.flags != change.candidate_var.flags
        || change.base_var.var_type == change.candidate_var.var_type
    {
        return Err(MigrationError::IncompatibleEntry {
            uid: change.uid,
            reason: entry_difference(change.base_var, change.candidate_var),
        });
    }
    let from = change.base_var.var_type;
    let to = change.candidate_var.var_type;
    if change.base_var.flags & ironplc_container::VAR_FLAG_IS_ARRAY == 0 {
        return match decisions.resolve(
            change.uid,
            change.name,
            from,
            to,
            scalar_size_equal(from, to),
        )? {
            TypeChangeChoice::Convert(conversion) => {
                actions.push(MigrationAction::Convert {
                    from_index: change.from_index,
                    to_index: change.to_index,
                    conversion,
                });
                Ok(())
            }
            TypeChangeChoice::Preserve => {
                actions.push(MigrationAction::Slot {
                    from_index: change.from_index,
                    to_index: change.to_index,
                });
                Ok(())
            }
            TypeChangeChoice::Init => Ok(()),
        };
    }
    let base_descriptor = array_descriptor(change.base_section, usize::from(change.base_var.extra));
    let candidate_descriptor = array_descriptor(
        change.candidate_section,
        usize::from(change.candidate_var.extra),
    );
    let size_equal = array_size_equal(base_descriptor, candidate_descriptor);
    match decisions.resolve(change.uid, change.name, from, to, size_equal)? {
        TypeChangeChoice::Convert(conversion) => {
            let action = array_conversion_action(
                change.uid,
                change.from_index,
                change.to_index,
                conversion,
                base_descriptor,
                candidate_descriptor,
            )?;
            actions.push(action);
            Ok(())
        }
        TypeChangeChoice::Preserve => {
            let action = array_preserve_action(
                change.uid,
                change.from_index,
                change.to_index,
                candidate_descriptor,
            )?;
            actions.push(action);
            Ok(())
        }
        TypeChangeChoice::Init => Ok(()),
    }
}

/// Whether a scalar preserve is legal: both classes share one of the two
/// numeric width families ADR 0061 names. Strings, FB instances, `TIME`, and
/// heterogeneous slots are outside them.
pub(super) fn scalar_size_equal(from: FieldType, to: FieldType) -> bool {
    let family = width_family(from);
    family.is_some() && family == width_family(to)
}

/// Whether an array preserve is legal: same element width family and same
/// element count (ADR 0061's "element size and length equal").
pub(super) fn array_size_equal(
    base: Option<&ArrayDescriptor>,
    candidate: Option<&ArrayDescriptor>,
) -> bool {
    let (Some(base), Some(candidate)) = (base, candidate) else {
        return false;
    };
    base.total_elements == candidate.total_elements
        && element_family_matches(base.element_type, candidate.element_type)
}

/// The two numeric width families ADR 0061 allows a preserve decision to
/// copy between.
#[derive(Clone, Copy, PartialEq, Eq)]
enum WidthFamily {
    /// `I32`, `U32`, `F32`.
    Bits32,
    /// `I64`, `U64`, `F64`.
    Bits64,
}

/// The width family a storage class belongs to, or `None` when no preserve
/// can carry it.
fn width_family(field_type: FieldType) -> Option<WidthFamily> {
    match field_type {
        FieldType::I32 | FieldType::U32 | FieldType::F32 => Some(WidthFamily::Bits32),
        FieldType::I64 | FieldType::U64 | FieldType::F64 => Some(WidthFamily::Bits64),
        _ => None,
    }
}

/// Whether two raw array element tags name classes of one width family.
fn element_family_matches(base: u8, candidate: u8) -> bool {
    let (Ok(base), Ok(candidate)) = (FieldType::from_u8(base), FieldType::from_u8(candidate))
    else {
        return false;
    };
    scalar_size_equal(base, candidate)
}

/// The candidate variable's debug name, when the candidate carries a debug
/// section naming it.
pub(super) fn debug_name(candidate: &Container, index: VarIndex) -> Option<String> {
    candidate
        .debug_section
        .as_ref()?
        .var_names
        .iter()
        .find(|entry| entry.var_index == index)
        .map(|entry| entry.name.clone())
}

/// Sizes a per-element conversion for an array whose element types the
/// policy admits (ADR 0060). The element count and the per-element size must
/// match on both sides; the admitted element pairs are non-string, so every
/// element is one 8-byte slot in both regions.
fn array_conversion_action(
    uid: u64,
    from_index: VarIndex,
    to_index: VarIndex,
    conversion: ValueConversion,
    base_descriptor: Option<&ArrayDescriptor>,
    candidate_descriptor: Option<&ArrayDescriptor>,
) -> Result<MigrationAction, MigrationError> {
    let (Some(base_descriptor), Some(candidate_descriptor)) =
        (base_descriptor, candidate_descriptor)
    else {
        return Err(MigrationError::ArrayDescriptorMismatch { uid });
    };
    if base_descriptor.total_elements != candidate_descriptor.total_elements
        || base_descriptor.element_extra != candidate_descriptor.element_extra
    {
        return Err(MigrationError::ArrayDescriptorMismatch { uid });
    }
    let byte_size = candidate_descriptor
        .byte_size()
        .ok_or(MigrationError::ArrayDescriptorMismatch { uid })?;
    Ok(MigrationAction::ConvertArray {
        from_index,
        to_index,
        conversion,
        byte_size,
    })
}

/// Sizes a preserve copy from the candidate descriptor; the size equality
/// check already proved the base region is the same size.
fn array_preserve_action(
    uid: u64,
    from_index: VarIndex,
    to_index: VarIndex,
    candidate_descriptor: Option<&ArrayDescriptor>,
) -> Result<MigrationAction, MigrationError> {
    let Some(candidate_descriptor) = candidate_descriptor else {
        return Err(MigrationError::ArrayDescriptorMismatch { uid });
    };
    let byte_size = candidate_descriptor
        .byte_size()
        .ok_or(MigrationError::ArrayDescriptorMismatch { uid })?;
    Ok(MigrationAction::Array {
        from_index,
        to_index,
        byte_size,
    })
}
