//! State migration of arrays of function block instances.
//!
//! An array of instances is a flat run of slots, so the planner copies it as
//! one region. That keeps each instance's state only when the block is laid out
//! alike on both sides; anything else fails closed.

use super::*;

/// The descriptor index of the array of instances in every container below.
const ARRAY: u16 = 0;
const BLOCK: u16 = 0x1000;

/// The variable-table entry of an array of instances: the flat slot array its
/// descriptor spans.
fn instance_array_entry() -> VarEntry {
    VarEntry {
        var_type: FieldType::Slot,
        flags: VAR_FLAG_IS_ARRAY,
        extra: ARRAY,
    }
}

/// A container holding one stable array of `slots` slots of instances of the
/// block `BLOCK`, whose fields are the entries `fields` after the array's own.
fn container_with_instances(
    slots: u32,
    user_block: Option<UserFbDescriptor>,
    fields: &[VarEntry],
    field_uids: &[FbFieldUidEntry],
) -> Container {
    let mut builder = ContainerBuilder::new();
    builder.add_array_descriptor(FieldType::FbInstance as u8, slots, BLOCK);
    builder = builder.add_var_entry(instance_array_entry());
    for field in fields {
        builder = builder.add_var_entry(field.clone());
    }
    if let Some(descriptor) = user_block {
        builder = builder.add_user_fb_type(descriptor);
    }
    for uid in field_uids {
        builder = builder.add_fb_field_uid(*uid);
    }
    builder
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1 + fields.len() as u16)
        .data_region_bytes(slots * 8)
        .build()
}

#[test]
fn build_when_standard_block_array_unchanged_then_emits_region_copy() {
    let base = container_with_instances(12, None, &[], &[]);
    let candidate = container_with_instances(12, None, &[], &[]);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::Array {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            byte_size: 96,
        }]
    );
}

#[test]
fn build_when_user_block_array_layout_identical_then_emits_region_copy() {
    let fields = [i32_entry(), i32_entry()];
    let uids = [field_uid(BLOCK, 0, 11), field_uid(BLOCK, 1, 12)];
    let base = container_with_instances(6, Some(user_fb_at(BLOCK, 1, 2)), &fields, &uids);
    let candidate = container_with_instances(6, Some(user_fb_at(BLOCK, 1, 2)), &fields, &uids);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::Array {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            byte_size: 48,
        }]
    );
}

#[test]
fn build_when_user_block_field_changes_storage_class_at_same_size_then_fb_layout_unsupported() {
    // The descriptors are equal and so are the slot counts: only the field
    // entries say the same slots no longer hold the same values.
    let base_fields = [i32_entry(), i32_entry()];
    let candidate_fields = [
        i32_entry(),
        VarEntry {
            var_type: FieldType::F32,
            flags: 0,
            extra: 0,
        },
    ];
    let base = container_with_instances(6, Some(user_fb_at(BLOCK, 1, 2)), &base_fields, &[]);
    let candidate =
        container_with_instances(6, Some(user_fb_at(BLOCK, 1, 2)), &candidate_fields, &[]);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(result.unwrap_err(), MigrationError::FbLayoutUnsupported);
}

#[test]
fn build_when_user_block_fields_change_uids_then_fb_layout_unsupported() {
    let fields = [i32_entry(), i32_entry()];
    let base_uids = [field_uid(BLOCK, 0, 11), field_uid(BLOCK, 1, 12)];
    let candidate_uids = [field_uid(BLOCK, 0, 12), field_uid(BLOCK, 1, 11)];
    let base = container_with_instances(6, Some(user_fb_at(BLOCK, 1, 2)), &fields, &base_uids);
    let candidate =
        container_with_instances(6, Some(user_fb_at(BLOCK, 1, 2)), &fields, &candidate_uids);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(result.unwrap_err(), MigrationError::FbLayoutUnsupported);
}

#[test]
fn build_when_user_block_described_on_one_side_only_then_fb_layout_unsupported() {
    let fields = [i32_entry(), i32_entry()];
    let base = container_with_instances(6, Some(user_fb_at(BLOCK, 1, 2)), &fields, &[]);
    let candidate = container_with_instances(6, None, &fields, &[]);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(result.unwrap_err(), MigrationError::FbLayoutUnsupported);
}

#[test]
fn build_when_block_array_changes_block_or_size_then_array_descriptor_mismatch() {
    let base = container_with_instances(12, None, &[], &[]);
    let resized = container_with_instances(18, None, &[], &[]);

    let result = StateMigrationPlan::build(&base, &resized);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::ArrayDescriptorMismatch { uid: 7 }
    );
}
