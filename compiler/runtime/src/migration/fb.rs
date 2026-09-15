//! Function-block instance planning for the state migration planner.
//!
//! An FB instance's slot holds the offset of its field region, and the field
//! values themselves are addressed through the type section's user FB
//! descriptors (ADR 0059). This module is the planner's ADR 0059 half: it
//! plans the copies for shared-UID instances of user FB types and enforces
//! the fallback rule for everything else. The per-variable machinery —
//! scalars, strings, arrays, and type-changing conversions (ADR 0060) —
//! lives in the parent module.

use std::collections::HashMap;
use std::vec::Vec;

use ironplc_container::{
    Container, FbFieldUidEntry, FbTypeDescriptor, FbTypeId, FieldType, StableVarEntry, TypeSection,
    UserFbDescriptor, VarEntry, VarIndex,
};

use super::{entry_difference, variable_at, variable_table, MigrationAction, MigrationError};

/// Plans the copies for shared-UID FB instances of user FB types and enforces
/// the fallback rule for everything else (see the module documentation).
///
/// `instances` holds `(base index, candidate index)` pairs for the
/// shared-UID instances whose type has a user FB descriptor on both sides;
/// every other FB instance named by either stable table (standard-library
/// instances, instances only one side has) makes the planner fall back to
/// the stage-2 rule: the layout after the program prefix must be identical.
pub(super) fn plan_fb_instances(
    actions: &mut Vec<MigrationAction>,
    base: &Container,
    candidate: &Container,
    base_stable: &[StableVarEntry],
    candidate_stable: &[StableVarEntry],
    instances: &[(VarIndex, VarIndex)],
) -> Result<(), MigrationError> {
    let base_section = base.type_section.as_ref();
    let candidate_section = candidate.type_section.as_ref();
    let base_variables = variable_table(base_section);
    let candidate_variables = variable_table(candidate_section);

    let mut handled: Vec<(VarIndex, VarIndex)> = Vec::with_capacity(instances.len());
    for &(from_index, to_index) in instances {
        let type_id = FbTypeId::new(variable_at(candidate_variables, to_index)?.extra);
        let (Some(base_descriptor), Some(candidate_descriptor)) = (
            user_fb_descriptor(base_section, type_id),
            user_fb_descriptor(candidate_section, type_id),
        ) else {
            return Err(MigrationError::FbLayoutUnsupported);
        };
        handled.push((from_index, to_index));

        if base_descriptor == candidate_descriptor {
            // Identical layout: carry the slot and the whole field region.
            // The descriptor comparison covers var_offset and num_fields, so
            // both sides agree on where the region lives and how big it is.
            actions.push(MigrationAction::FbInstance {
                from_index,
                to_index,
                byte_size: u32::from(candidate_descriptor.num_fields)
                    .checked_mul(ironplc_container::SLOT_BYTES)
                    .ok_or(MigrationError::FbLayoutUnsupported)?,
            });
            continue;
        }

        // The layout differs: match the type's fields by UID (ADR 0059).
        let base_fields = field_uid_indexes(base_section, type_id)?;
        let candidate_fields = field_uid_indexes(candidate_section, type_id)?;
        for field_index in 0..candidate_descriptor.num_fields {
            let Some(&field_uid) = candidate_fields.by_index.get(&field_index) else {
                // The field carries no UID (or the reserved UID 0) while
                // the layout differs: the value's identity is unprovable,
                // so the planner fails closed.
                return Err(MigrationError::FbLayoutUnsupported);
            };
            let Some(&from_field) = base_fields.by_uid.get(&field_uid) else {
                // A candidate-only field UID is a new entity; the
                // candidate's init image has already initialized it.
                continue;
            };
            let base_field = variable_at(
                base_variables,
                VarIndex::new(base_descriptor.var_offset + u16::from(from_field)),
            )?;
            let candidate_field = variable_at(
                candidate_variables,
                VarIndex::new(candidate_descriptor.var_offset + u16::from(field_index)),
            )?;
            if base_field != candidate_field {
                return Err(MigrationError::IncompatibleEntry {
                    uid: field_uid,
                    reason: entry_difference(base_field, candidate_field),
                });
            }
            actions.push(MigrationAction::FbField {
                from_index,
                to_index,
                from_field,
                to_field: field_index,
            });
        }
    }

    // Any shared instance the per-field path did not take over — a
    // standard-library FB, or one whose descriptor exists on only one side —
    // falls back to the stage-2 rule: nothing above can match its internals,
    // so the post-prefix layout must be identical for a copy to be safe.
    // Instances only one stable table names need no fallback: a base-only
    // instance is dropped with the old buffers and a candidate-only one is
    // initialized by the candidate's init image, so no copy references
    // either.
    if has_unhandled_shared_instance(base_stable, base_variables, candidate_stable, &handled)?
        && (base.task_table.shared_globals_size != candidate.task_table.shared_globals_size
            || !same_user_fb_descriptors(
                user_fb_descriptors(base_section),
                user_fb_descriptors(candidate_section),
            )
            || !same_fb_descriptors(
                fb_descriptors(base_section),
                fb_descriptors(candidate_section),
            ))
    {
        return Err(MigrationError::FbLayoutUnsupported);
    }
    Ok(())
}

/// Whether a UID both stable tables share names an FB instance the per-field
/// path did not take over (see [`plan_fb_instances`]). `handled` holds the
/// `(base index, candidate index)` pairs the per-field path planned; the
/// base side of each pair is enough to recognize a shared instance.
fn has_unhandled_shared_instance(
    base_stable: &[StableVarEntry],
    base_variables: &[VarEntry],
    candidate_stable: &[StableVarEntry],
    handled: &[(VarIndex, VarIndex)],
) -> Result<bool, MigrationError> {
    let candidate_uids: HashMap<u64, VarIndex> = candidate_stable
        .iter()
        .map(|entry| (entry.uid, entry.var_index))
        .collect();
    for entry in base_stable {
        let Some(&candidate_index) = candidate_uids.get(&entry.uid) else {
            continue;
        };
        if variable_at(base_variables, entry.var_index)?.var_type == FieldType::FbInstance
            && !handled
                .iter()
                .any(|(base, candidate)| *base == entry.var_index && *candidate == candidate_index)
        {
            return Ok(true);
        }
    }
    Ok(false)
}

/// UID → field index and field index → UID maps for one FB type's entries in
/// the type section's FB field UID table. UID 0 is reserved (the UID sidecar
/// never assigns it) and excluded: an entry carrying it is treated as no
/// UID. A UID bound twice is a corrupt table.
struct FieldUidIndexes {
    by_uid: HashMap<u64, u8>,
    by_index: HashMap<u8, u64>,
}

fn field_uid_indexes(
    section: Option<&TypeSection>,
    type_id: FbTypeId,
) -> Result<FieldUidIndexes, MigrationError> {
    let mut by_uid = HashMap::new();
    let mut by_index = HashMap::new();
    for entry in fb_field_uids(section) {
        if entry.fb_type_id == type_id && entry.uid != 0 {
            if by_uid.insert(entry.uid, entry.field_index).is_some() {
                return Err(MigrationError::DuplicateUid { uid: entry.uid });
            }
            by_index.insert(entry.field_index, entry.uid);
        }
    }
    Ok(FieldUidIndexes { by_uid, by_index })
}

/// The FB field UID table, or an empty slice without a type section.
fn fb_field_uids(section: Option<&TypeSection>) -> &[FbFieldUidEntry] {
    section.map_or(&[], |section| section.fb_field_uids.as_slice())
}

/// The user FB descriptor for `type_id`, if the type section carries one.
pub(super) fn user_fb_descriptor(
    section: Option<&TypeSection>,
    type_id: FbTypeId,
) -> Option<&UserFbDescriptor> {
    user_fb_descriptors(section)
        .iter()
        .find(|descriptor| descriptor.type_id == type_id)
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
