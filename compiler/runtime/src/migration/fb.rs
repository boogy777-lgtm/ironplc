//! Function-block instance planning for the state migration planner.
//!
//! An FB instance's slot holds the offset of its field region, and the field
//! values themselves are addressed through the type section's FB type
//! descriptors, which list the fields of each user function block (ADR 0059,
//! ADR-0073). The position of the block's working slots in the variable table
//! (the `var_offset` of the user FB descriptor) is an address for the VM and
//! is never read as the identity of the block. This module is the planner's
//! ADR 0059 half: it
//! plans the copies for shared-UID instances of user FB types and enforces
//! the fallback rule for everything else. The per-variable machinery —
//! scalars, strings, arrays, and type-changing conversions (ADR 0060) —
//! lives in the parent module.

use std::collections::HashMap;
use std::vec::Vec;

use ironplc_container::{
    Container, FbFieldUidEntry, FbTypeDescriptor, FbTypeId, FieldEntry, FieldType, StableVarEntry,
    TypeSection, UserFbDescriptor, VarEntry, VarIndex,
};

use super::decision::{self, Decisions, TypeChangeChoice};
use super::{variable_at, variable_table, MigrationAction, MigrationError};

/// Plans the copies for shared-UID FB instances of user FB types and enforces
/// the fallback rule for everything else (see the module documentation).
///
/// `instances` holds `(base index, candidate index)` pairs for the
/// shared-UID instances whose type lists its fields on both sides;
/// every other FB instance named by either stable table (standard-library
/// instances, instances only one side has) makes the planner fall back to
/// the stage-2 rule: the layout after the program prefix must be identical.
///
/// A shared field whose entry changed is a storage-class change: it converts
/// per the ADR-0060 policy, resolves through the engineer's decisions
/// (ADR 0061), or joins the collected
/// [`TypeChangeUnsupported`](MigrationError::TypeChangeUnsupported) pairs.
pub(super) fn plan_fb_instances(
    actions: &mut Vec<MigrationAction>,
    decisions: &mut Decisions<'_>,
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
        let (Some(base_fields), Some(candidate_fields)) = (
            fb_fields(base_section, type_id),
            fb_fields(candidate_section, type_id),
        ) else {
            return Err(MigrationError::FbLayoutUnsupported);
        };
        // A STRING field's slot holds the offset of the string, and that offset
        // is the instance's own: a slot copied from the other container would
        // point into its layout, not this one's. The per-field path copies one
        // slot per field and cannot carry the value, so such an instance fails
        // closed, as an array-field retype does.
        if has_string_field(base_fields) || has_string_field(candidate_fields) {
            return Err(MigrationError::FbLayoutUnsupported);
        }
        handled.push((from_index, to_index));

        if base_fields == candidate_fields {
            // Identical layout: carry the slot and the whole field region.
            // The lists of fields are equal, so both sides agree on how big
            // the region is and what each slot holds.
            actions.push(MigrationAction::FbInstance {
                from_index,
                to_index,
                byte_size: u32::try_from(candidate_fields.len())
                    .ok()
                    .and_then(|fields| fields.checked_mul(ironplc_container::SLOT_BYTES))
                    .ok_or(MigrationError::FbLayoutUnsupported)?,
            });
            continue;
        }

        // The layout differs: match the type's fields by UID (ADR 0059).
        let base_uids = field_uid_indexes(base_section, type_id)?;
        let candidate_uids = field_uid_indexes(candidate_section, type_id)?;
        for (field_index, candidate_field) in (0..).zip(candidate_fields) {
            let Some(&field_uid) = candidate_uids.by_index.get(&field_index) else {
                // The field carries no UID (or the reserved UID 0) while
                // the layout differs: the value's identity is unprovable,
                // so the planner fails closed.
                return Err(MigrationError::FbLayoutUnsupported);
            };
            let Some(&from_field) = base_uids.by_uid.get(&field_uid) else {
                // A candidate-only field UID is a new entity; the
                // candidate's init image has already initialized it.
                continue;
            };
            let Some(base_field) = base_fields.get(usize::from(from_field)) else {
                return Err(MigrationError::FbLayoutUnsupported);
            };
            if base_field != candidate_field {
                // The field entry differs: reconcile the storage-class
                // change against the policy (ADR 0060) and the engineer's
                // decisions (ADR 0061), or reject the structural difference
                // as before.
                if base_field.field_type == candidate_field.field_type {
                    // The type tag is the same and its details differ (a
                    // STRING length, a nested block's type): a structural
                    // change, not a storage-class change.
                    return Err(MigrationError::IncompatibleEntry {
                        uid: field_uid,
                        reason: "the variable's type details changed",
                    });
                }
                let from = base_field.field_type;
                let to = candidate_field.field_type;
                let name = field_debug_name(candidate_section, candidate, type_id, field_index);
                let size_equal = decision::scalar_size_equal(from, to);
                let conversion = match decisions.resolve(field_uid, name, from, to, size_equal)? {
                    TypeChangeChoice::Convert(conversion) => Some(conversion),
                    TypeChangeChoice::Preserve => None,
                    TypeChangeChoice::Init => continue,
                };
                actions.push(MigrationAction::FbField {
                    from_index,
                    to_index,
                    from_field,
                    to_field: field_index,
                    conversion,
                });
                continue;
            }
            actions.push(MigrationAction::FbField {
                from_index,
                to_index,
                from_field,
                to_field: field_index,
                conversion: None,
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
        && (base.persistent_extents() != candidate.persistent_extents()
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

/// Whether instances of the FB type `type_id` are laid out alike in both
/// containers: a standard-library block by its type ID alone, a user-defined one
/// by the same number of fields of the same storage classes under the same UIDs.
/// The slots of an array of instances can then be copied as they are; anything
/// less could attach a value to a different field.
pub(super) fn same_instance_layout(
    base: Option<&TypeSection>,
    candidate: Option<&TypeSection>,
    type_id: FbTypeId,
) -> bool {
    match (
        user_fb_descriptor(base, type_id),
        user_fb_descriptor(candidate, type_id),
    ) {
        (None, None) => true,
        (Some(_), Some(_)) => {
            fb_fields(base, type_id).is_some()
                && fb_fields(base, type_id) == fb_fields(candidate, type_id)
                && matches!(
                    (field_uid_indexes(base, type_id), field_uid_indexes(candidate, type_id)),
                    (Ok(a), Ok(b)) if a.by_uid == b.by_uid
                )
        }
        _ => false,
    }
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

/// The user FB descriptor for `type_id`, if the type section carries one. A
/// block that has none is a standard-library block, whose layout the VM owns.
pub(super) fn user_fb_descriptor(
    section: Option<&TypeSection>,
    type_id: FbTypeId,
) -> Option<&UserFbDescriptor> {
    section.and_then(|section| {
        section
            .user_fb_types
            .iter()
            .find(|descriptor| descriptor.type_id == type_id)
    })
}

/// The fields of the function block type `type_id`, as its FB type descriptor
/// lists them, if the type section carries one. This is the one place the
/// planner reads the layout of an instance from.
pub(super) fn fb_fields(section: Option<&TypeSection>, type_id: FbTypeId) -> Option<&[FieldEntry]> {
    fb_descriptors(section)
        .iter()
        .find(|descriptor| descriptor.type_id == type_id)
        .map(|descriptor| descriptor.fields.as_slice())
}

/// The name the debug section gives the field at ordinal `field_index` of the
/// type `type_id`, for the engineer's decision prompt. The name is looked up at
/// the field's slot in the body, which is where the debug section records it;
/// it names the field and does not identify it.
fn field_debug_name(
    section: Option<&TypeSection>,
    container: &Container,
    type_id: FbTypeId,
    field_index: u8,
) -> Option<String> {
    let descriptor = user_fb_descriptor(section, type_id)?;
    decision::debug_name(
        container,
        VarIndex::new(descriptor.var_offset + u16::from(field_index)),
    )
}

/// The FB type descriptors, or an empty slice without a type section.
fn fb_descriptors(section: Option<&TypeSection>) -> &[FbTypeDescriptor] {
    section.map_or(&[], |section| section.fb_types.as_slice())
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

/// Whether a field of an FB type is a STRING or a WSTRING, whose characters
/// live outside its slot, so that the slot holds where the value is rather
/// than the value.
fn has_string_field(fields: &[FieldEntry]) -> bool {
    fields
        .iter()
        .any(|field| matches!(field.field_type, FieldType::String | FieldType::WString))
}
