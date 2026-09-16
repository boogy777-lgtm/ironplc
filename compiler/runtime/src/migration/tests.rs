//! Unit tests for the state migration planner.
//!
//! The cases pin the per-UID classification, each reject reason, the
//! data-region copies, and the ADR-0059 FB layout rules; the type-change
//! policy and the ADR-0061 decision vocabulary live in [`decisions`]. The
//! end-to-end host behavior lives in `tests/migration_acceptance.rs`.

mod decisions;

use super::*;
use ironplc_container::{
    Container, ContainerBuilder, FbFieldUidEntry, FbTypeDescriptor, FbTypeId, FieldEntry,
    FieldType, FunctionId, UserFbDescriptor,
};
use ironplc_vm::Slot;

/// An I32 variable-table entry.
fn i32_entry() -> VarEntry {
    VarEntry {
        var_type: FieldType::I32,
        flags: 0,
        extra: 0,
    }
}

/// A STRING variable-table entry with the given maximum length.
fn string_entry(max_length: u16) -> VarEntry {
    VarEntry {
        var_type: FieldType::String,
        flags: 0,
        extra: max_length,
    }
}

/// An array variable-table entry referencing `descriptor`.
fn array_entry(descriptor: u16) -> VarEntry {
    VarEntry {
        var_type: FieldType::I32,
        flags: VAR_FLAG_IS_ARRAY,
        extra: descriptor,
    }
}

/// An FB instance variable-table entry for `type_id`.
fn fb_entry(type_id: u16) -> VarEntry {
    VarEntry {
        var_type: FieldType::FbInstance,
        flags: 0,
        extra: type_id,
    }
}

/// A user FB descriptor for `type_id` with `fields` fields.
fn user_fb(type_id: u16, fields: u8) -> UserFbDescriptor {
    UserFbDescriptor {
        type_id: FbTypeId::new(type_id),
        function_id: FunctionId::new(2),
        var_offset: 0,
        num_fields: fields,
    }
}

/// A user FB descriptor whose fields map at `var_offset`.
fn user_fb_at(type_id: u16, var_offset: u16, fields: u8) -> UserFbDescriptor {
    UserFbDescriptor {
        var_offset,
        ..user_fb(type_id, fields)
    }
}

/// An FB field UID entry.
fn field_uid(type_id: u16, field_index: u8, uid: u64) -> FbFieldUidEntry {
    FbFieldUidEntry {
        fb_type_id: FbTypeId::new(type_id),
        field_index,
        uid,
    }
}

/// Builds a container whose type section carries `variables` and the given
/// `(var_index, uid)` stable variable IDs.
fn container(variables: &[VarEntry], stable: &[(u16, u64)]) -> Container {
    let mut builder = ContainerBuilder::new();
    for entry in variables {
        builder = builder.add_var_entry(entry.clone());
    }
    for (index, uid) in stable {
        builder = builder.add_stable_var(StableVarEntry {
            var_index: VarIndex::new(*index),
            uid: *uid,
        });
    }
    builder.num_variables(variables.len() as u16).build()
}

/// Builds a container with one stable I32 array variable sized so its data
/// region can hold all `elements`.
fn array_container(elements: u32, data_region_bytes: u32) -> Container {
    let mut builder = ContainerBuilder::new();
    builder.add_array_descriptor(FieldType::I32 as u8, elements, 0);
    builder
        .add_var_entry(array_entry(0))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .data_region_bytes(data_region_bytes)
        .build()
}

/// A buffer set with the given slots and data region, for direct applications.
fn buffers(vars: Vec<Slot>, data_region_len: usize) -> VmBuffers {
    VmBuffers {
        stack: Vec::new(),
        vars,
        data_region: vec![0u8; data_region_len],
        temp_buf: Vec::new(),
        tasks: Vec::new(),
        programs: Vec::new(),
        ready: Vec::new(),
        frames: Vec::new(),
    }
}

#[test]
fn build_when_uid_in_both_then_emits_copy_slot() {
    let base = container(&[i32_entry()], &[(0, 7)]);
    let candidate = container(&[i32_entry(), i32_entry()], &[(1, 7)]);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::Slot {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(1),
        }]
    );
}

#[test]
fn build_when_uid_only_in_candidate_then_no_action() {
    let base = container(&[i32_entry()], &[(0, 7)]);
    let candidate = container(&[i32_entry(), i32_entry()], &[(0, 7), (1, 9)]);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::Slot {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
        }]
    );
}

#[test]
fn build_when_uid_only_in_base_then_no_action() {
    let base = container(&[i32_entry(), i32_entry()], &[(0, 7), (1, 9)]);
    let candidate = container(&[i32_entry()], &[(0, 7)]);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::Slot {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
        }]
    );
}

#[test]
fn build_when_variable_flags_changed_then_incompatible_entry() {
    let base = container(&[i32_entry()], &[(0, 7)]);
    let mut candidate = container(&[i32_entry()], &[(0, 7)]);
    candidate.type_section.as_mut().unwrap().variable_table[0].flags = VAR_FLAG_IS_ARRAY;

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::IncompatibleEntry {
            uid: 7,
            reason: "the variable's layout class changed",
        }
    );
}

#[test]
fn build_when_string_max_length_changed_then_incompatible_entry() {
    let base = container(&[string_entry(80)], &[(0, 7)]);
    let candidate = container(&[string_entry(40)], &[(0, 7)]);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::IncompatibleEntry {
            uid: 7,
            reason: "the variable's type details changed",
        }
    );
}

#[test]
fn build_when_string_entry_equal_then_emits_header_sized_region_copy() {
    let base = container(&[string_entry(10)], &[(0, 7)]);
    let candidate = container(&[string_entry(10)], &[(0, 7)]);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::String {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            byte_size: 16,
            candidate_max_length: 10,
        }]
    );
}

/// A container with one stable array variable of `element` with `elements`
/// elements, sized so its data region holds every element.
fn typed_array_container(element: FieldType, elements: u32, data_region_bytes: u32) -> Container {
    let mut builder = ContainerBuilder::new();
    builder.add_array_descriptor(element as u8, elements, 0);
    builder
        .add_var_entry(VarEntry {
            var_type: element,
            flags: VAR_FLAG_IS_ARRAY,
            extra: 0,
        })
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .data_region_bytes(data_region_bytes)
        .build()
}

#[test]
fn build_when_array_descriptor_missing_then_array_descriptor_mismatch() {
    // The candidate's entry names a descriptor index the candidate does not
    // carry: the shape cannot be proven, so the planner fails closed.
    let base = typed_array_container(FieldType::I32, 2, 16);
    let mut candidate = typed_array_container(FieldType::I64, 2, 16);
    candidate.type_section.as_mut().unwrap().variable_table[0].extra = 9;

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::ArrayDescriptorMismatch { uid: 7 }
    );
}

#[test]
fn build_when_array_descriptor_unchanged_then_emits_region_copy() {
    let base = array_container(4, 32);
    let candidate = array_container(4, 32);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::Array {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            byte_size: 32,
        }]
    );
}

#[test]
fn build_when_array_descriptor_changed_then_array_descriptor_mismatch() {
    let base = array_container(4, 32);
    let candidate = array_container(8, 64);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::ArrayDescriptorMismatch { uid: 7 }
    );
}

#[test]
fn build_when_fb_tail_identical_then_copy_slot_and_region() {
    let base = ContainerBuilder::new()
        .shared_globals_size(2)
        .add_user_fb_type(user_fb(0x1000, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(2)
        .build();
    let candidate = ContainerBuilder::new()
        .shared_globals_size(2)
        .add_user_fb_type(user_fb(0x1000, 2))
        .add_var_entry(i32_entry())
        .add_var_entry(fb_entry(0x1000))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(1),
            uid: 7,
        })
        .num_variables(2)
        .build();

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    // The identical descriptor lets the planner carry the slot and the
    // whole field region, so the instance's values survive the reorder.
    assert_eq!(
        plan.actions,
        vec![MigrationAction::FbInstance {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(1),
            byte_size: 16,
        }]
    );
}

#[test]
fn build_when_fb_tail_changes_then_fb_layout_unsupported() {
    let base = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_user_fb_type(user_fb(0x1000, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build();
    let candidate = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_user_fb_type(user_fb(0x1000, 3))
        .add_var_entry(fb_entry(0x1000))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build();

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(result.unwrap_err(), MigrationError::FbLayoutUnsupported);
}

#[test]
fn build_when_no_fb_instance_then_tail_change_is_ignored() {
    let base = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_user_fb_type(user_fb(0x1000, 2))
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build();
    let candidate = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_user_fb_type(user_fb(0x1000, 3))
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build();

    assert!(StateMigrationPlan::build(&base, &candidate).is_ok());
}

#[test]
fn build_when_fb_instance_and_program_prefix_grows_then_copy_slot_and_region() {
    // The per-instance rule (ADR 0059) no longer keys on the program prefix
    // size: the type's own descriptor decides whether the instance's layout
    // is identical, and an added scalar program variable consumes no data
    // region, so the field region copy stays valid.
    let base = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_user_fb_type(user_fb(0x1000, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build();
    let candidate = ContainerBuilder::new()
        .shared_globals_size(2)
        .add_user_fb_type(user_fb(0x1000, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(2)
        .build();

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::FbInstance {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            byte_size: 16,
        }]
    );
}

#[test]
fn build_when_fb_field_uids_cover_layout_change_then_per_field_copy() {
    // ADR 0059: the candidate inserts a field at ordinal 1, shifting the
    // surviving field's ordinal to 2. The field UIDs — not the ordinals —
    // decide which value continues: uid 101 (base field 0) copies to
    // candidate field 0, uid 102 (base field 1) copies to candidate field 2,
    // and uid 103 is candidate-only (no action; the candidate's init image
    // initializes it).
    let base = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_fb_field_uid(field_uid(0x1000, 0, 101))
        .add_fb_field_uid(field_uid(0x1000, 1, 102))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(3)
        .build();
    let candidate = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 3))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_fb_field_uid(field_uid(0x1000, 0, 101))
        .add_fb_field_uid(field_uid(0x1000, 1, 103))
        .add_fb_field_uid(field_uid(0x1000, 2, 102))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(4)
        .build();

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![
            MigrationAction::FbField {
                from_index: VarIndex::new(0),
                to_index: VarIndex::new(0),
                from_field: 0,
                to_field: 0,
                conversion: None,
            },
            MigrationAction::FbField {
                from_index: VarIndex::new(0),
                to_index: VarIndex::new(0),
                from_field: 1,
                to_field: 2,
                conversion: None,
            },
        ]
    );
}

#[test]
fn build_when_fb_field_uid_unknown_and_layout_differs_then_fail_closed() {
    // The candidate's field 2 carries no UID while the layout differs
    // underneath it: the value's identity is unprovable, so the planner
    // rejects the whole candidate (ADR 0059's fail-closed rule).
    let base = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_fb_field_uid(field_uid(0x1000, 0, 101))
        .add_fb_field_uid(field_uid(0x1000, 1, 102))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(3)
        .build();
    let candidate = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 3))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_fb_field_uid(field_uid(0x1000, 0, 101))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(4)
        .build();

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(result.unwrap_err(), MigrationError::FbLayoutUnsupported);
}

#[test]
fn build_when_fb_array_field_retyped_then_incompatible_entry() {
    // The per-field path copies one slot per field, and an array field's
    // slot holds the region offset rather than the elements: a retyped
    // array field cannot migrate through it, so the planner fails closed
    // until array-field retype is supported.
    let mut base_builder = ContainerBuilder::new();
    base_builder.add_array_descriptor(FieldType::I32 as u8, 2, 0);
    let base = base_builder
        .add_user_fb_type(user_fb_at(0x1000, 1, 1))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(array_entry(0))
        .add_fb_field_uid(field_uid(0x1000, 0, 101))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(2)
        .build();
    let mut candidate_builder = ContainerBuilder::new();
    candidate_builder.add_array_descriptor(FieldType::U32 as u8, 2, 0);
    let candidate = candidate_builder
        .add_user_fb_type(user_fb_at(0x1000, 1, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(VarEntry {
            var_type: FieldType::U32,
            flags: VAR_FLAG_IS_ARRAY,
            extra: 0,
        })
        .add_var_entry(i32_entry())
        .add_fb_field_uid(field_uid(0x1000, 0, 101))
        .add_fb_field_uid(field_uid(0x1000, 1, 102))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(3)
        .build();

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::IncompatibleEntry {
            uid: 101,
            reason: "the variable type changed",
        }
    );
}

/// An FB type descriptor for `type_id` with `fields` I32 fields.
fn fb_type(type_id: u16, fields: usize) -> FbTypeDescriptor {
    FbTypeDescriptor {
        type_id: FbTypeId::new(type_id),
        fields: (0..fields)
            .map(|_| FieldEntry {
                field_type: FieldType::I32,
                field_extra: 0,
            })
            .collect(),
    }
}

#[test]
fn build_when_fb_type_descriptor_changed_then_fb_layout_unsupported() {
    let base = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_fb_type(fb_type(0x0010, 2))
        .add_var_entry(fb_entry(0x0010))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build();
    let candidate = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_fb_type(fb_type(0x0010, 3))
        .add_var_entry(fb_entry(0x0010))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build();

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(result.unwrap_err(), MigrationError::FbLayoutUnsupported);
}

#[test]
fn build_when_fb_type_descriptors_reordered_then_copy_slot() {
    // Descriptor order is not part of the contract, so the same descriptors
    // in a different order must not reject the migration.
    let base = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_fb_type(fb_type(0x0010, 2))
        .add_fb_type(fb_type(0x0040, 3))
        .add_var_entry(fb_entry(0x0010))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build();
    let candidate = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_fb_type(fb_type(0x0040, 3))
        .add_fb_type(fb_type(0x0010, 2))
        .add_var_entry(fb_entry(0x0010))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build();

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::Slot {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
        }]
    );
}

#[test]
fn build_when_wstring_entry_equal_then_emits_wide_region_copy() {
    let entry = VarEntry {
        var_type: FieldType::WString,
        flags: 0,
        extra: 10,
    };
    let base = container(std::slice::from_ref(&entry), &[(0, 7)]);
    let candidate = container(&[entry], &[(0, 7)]);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::String {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            byte_size: 26,
            candidate_max_length: 10,
        }]
    );
}

#[test]
fn build_when_stable_index_out_of_range_then_error() {
    let base = container(&[i32_entry()], &[(0, 7)]);
    let candidate = container(&[i32_entry()], &[(3, 7)]);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::IndexOutOfRange {
            index: VarIndex::new(3)
        }
    );
}

#[test]
fn build_when_duplicate_uid_then_error() {
    let base = container(&[i32_entry()], &[(0, 7)]);
    let candidate = container(&[i32_entry(), i32_entry()], &[(0, 7), (1, 7)]);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(result.unwrap_err(), MigrationError::DuplicateUid { uid: 7 });
}

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
        .build();
    let candidate = ContainerBuilder::new()
        .add_var_entry(i32_entry())
        .add_var_entry(string_entry(10))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(1),
            uid: 7,
        })
        .num_variables(2)
        .data_region_bytes(16)
        .build();
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
        .build();
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

#[test]
fn migration_error_display_when_variant_then_names_the_reason() {
    assert_eq!(
        MigrationError::IncompatibleEntry {
            uid: 7,
            reason: "the variable type changed",
        }
        .to_string(),
        "entity 7 cannot migrate: the variable type changed"
    );
    assert_eq!(
        MigrationError::StringShrink {
            current_length: 5,
            candidate_max_length: 4,
        }
        .to_string(),
        "string value of length 5 does not fit the candidate maximum 4"
    );
    assert_eq!(
        MigrationError::ArrayDescriptorMismatch { uid: 7 }.to_string(),
        "entity 7 has a changed array descriptor"
    );
    assert_eq!(
        MigrationError::TypeChangeUnsupported {
            pairs: vec![
                TypeChangePair {
                    uid: 7,
                    name: Some("Counter".into()),
                    from: FieldType::I32,
                    to: FieldType::String,
                    size_equal: false,
                },
                TypeChangePair {
                    uid: 9,
                    name: None,
                    from: FieldType::I32,
                    to: FieldType::U32,
                    size_equal: true,
                },
            ],
        }
        .to_string(),
        "type changes are outside the migration policy: entity 7 (Counter) I32 -> STRING, entity 9 I32 -> U32"
    );
    assert_eq!(
        MigrationError::PreserveSizeMismatch { uid: 7 }.to_string(),
        "entity 7 cannot preserve its storage: the base and candidate sizes differ"
    );
    assert_eq!(
        MigrationError::UnknownDecisionUid { uid: 7 }.to_string(),
        "migration decision names uid 7, which is not a shared entity whose type changed"
    );
    assert_eq!(
        MigrationError::FbLayoutUnsupported.to_string(),
        "function-block instance layout changed and field UIDs cannot justify the change"
    );
    assert_eq!(
        MigrationError::IndexOutOfRange {
            index: VarIndex::new(3)
        }
        .to_string(),
        "variable index 3 is out of range"
    );
    assert_eq!(
        MigrationError::RegionOutOfRange {
            index: VarIndex::new(3)
        }
        .to_string(),
        "the data-region region for variable 3 is out of range"
    );
    assert_eq!(
        MigrationError::DuplicateUid { uid: 7 }.to_string(),
        "stable variable uid 7 is bound more than once"
    );
}
