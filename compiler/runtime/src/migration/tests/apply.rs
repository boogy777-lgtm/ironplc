//! Tests of applying a migration plan to the VM buffers.

use super::*;

#[test]
fn apply_when_scalar_copy_then_slot_moves() {
    let base = container(&[i32_entry()], &[(0, 7)]);
    let candidate = container(&[i32_entry(), i32_entry()], &[(1, 7)]);
    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    let mut base_buffers = VmBuffers::from_container(&base);
    base_buffers.vars[0] = Slot::from_i32(9);
    let mut candidate_buffers = VmBuffers::from_container(&candidate);

    plan.apply(&base_buffers, &mut candidate_buffers).unwrap();

    assert_eq!(candidate_buffers.vars[1], Slot::from_i32(9));
    assert_eq!(candidate_buffers.vars[0], Slot::default());
}

#[test]
fn apply_when_string_copy_then_region_and_slot_move() {
    let base = ContainerBuilder::new()
        .add_var_entry(string_entry(10))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .data_region_bytes(16)
        .build()
        .identified();
    let candidate = ContainerBuilder::new()
        .add_var_entry(i32_entry())
        .add_var_entry(string_entry(10))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(1),
            uid: 7,
        })
        .num_variables(2)
        .data_region_bytes(16)
        .build()
        .identified();
    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    let mut base_buffers = VmBuffers::from_container(&base);
    base_buffers.vars[0] = Slot::from_i32(0);
    base_buffers.data_region[0..2].copy_from_slice(&10u16.to_le_bytes());
    base_buffers.data_region[2..4].copy_from_slice(&4u16.to_le_bytes());
    base_buffers.data_region[4..6].copy_from_slice(&1u16.to_le_bytes());
    base_buffers.data_region[6..10].copy_from_slice(b"WXYZ");
    let mut candidate_buffers = VmBuffers::from_container(&candidate);
    candidate_buffers.vars[1] = Slot::from_i32(0);

    plan.apply(&base_buffers, &mut candidate_buffers).unwrap();

    assert_eq!(candidate_buffers.vars[1], Slot::from_i32(0));
    assert_eq!(
        &candidate_buffers.data_region[0..10],
        b"\x0A\x00\x04\x00\x01\x00WXYZ"
    );
}

#[test]
fn apply_when_array_copy_then_region_and_slot_move() {
    let base = array_container(2, 16);
    let mut builder = ContainerBuilder::new();
    builder.add_array_descriptor(FieldType::I32 as u8, 2, 0);
    let candidate = builder
        .add_var_entry(i32_entry())
        .add_var_entry(array_entry(0))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(1),
            uid: 7,
        })
        .num_variables(2)
        .data_region_bytes(16)
        .build()
        .identified();
    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    let mut base_buffers = VmBuffers::from_container(&base);
    base_buffers.vars[0] = Slot::from_i32(0);
    base_buffers.data_region[0..8].copy_from_slice(&11i64.to_le_bytes());
    base_buffers.data_region[8..16].copy_from_slice(&22i64.to_le_bytes());
    let mut candidate_buffers = VmBuffers::from_container(&candidate);
    candidate_buffers.vars[1] = Slot::from_i32(0);

    plan.apply(&base_buffers, &mut candidate_buffers).unwrap();

    assert_eq!(candidate_buffers.vars[1], Slot::from_i32(0));
    assert_eq!(&candidate_buffers.data_region[0..8], &11i64.to_le_bytes());
    assert_eq!(&candidate_buffers.data_region[8..16], &22i64.to_le_bytes());
}

#[test]
fn apply_when_string_current_length_exceeds_candidate_max_then_error() {
    let plan = StateMigrationPlan {
        actions: vec![MigrationAction::String {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            byte_size: 16,
            candidate_max_length: 4,
        }],
    };
    let mut base_buffers = buffers(vec![Slot::from_i32(0)], 16);
    base_buffers.data_region[2..4].copy_from_slice(&5u16.to_le_bytes());
    let mut candidate_buffers = buffers(vec![Slot::from_i32(0)], 16);

    let result = plan.apply(&base_buffers, &mut candidate_buffers);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::StringShrink {
            current_length: 5,
            candidate_max_length: 4,
        }
    );
}

#[test]
fn apply_when_variable_index_out_of_range_then_error() {
    let plan = StateMigrationPlan {
        actions: vec![MigrationAction::Slot {
            from_index: VarIndex::new(9),
            to_index: VarIndex::new(0),
        }],
    };
    let base_buffers = buffers(vec![Slot::default()], 0);
    let mut candidate_buffers = buffers(vec![Slot::default()], 0);

    let result = plan.apply(&base_buffers, &mut candidate_buffers);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::IndexOutOfRange {
            index: VarIndex::new(9)
        }
    );
}

#[test]
fn apply_when_region_offset_out_of_range_then_error() {
    let plan = StateMigrationPlan {
        actions: vec![MigrationAction::Array {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            byte_size: 32,
        }],
    };
    let base_buffers = buffers(vec![Slot::from_i32(8)], 16);
    let mut candidate_buffers = buffers(vec![Slot::from_i32(8)], 16);

    let result = plan.apply(&base_buffers, &mut candidate_buffers);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::RegionOutOfRange {
            index: VarIndex::new(0)
        }
    );
}

#[test]
fn apply_when_region_offset_negative_then_error() {
    let plan = StateMigrationPlan {
        actions: vec![MigrationAction::Array {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            byte_size: 32,
        }],
    };
    let base_buffers = buffers(vec![Slot::from_i32(-1)], 16);
    let mut candidate_buffers = buffers(vec![Slot::from_i32(0)], 16);

    let result = plan.apply(&base_buffers, &mut candidate_buffers);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::RegionOutOfRange {
            index: VarIndex::new(0)
        }
    );
}
