//! Unit tests for the state migration planner.
//!
//! The cases pin the per-UID classification, each reject reason, the
//! data-region copies, and the type-conversion policy (ADR 0060); the
//! end-to-end host behavior lives in `tests/migration_acceptance.rs`.

use super::*;
use crate::conversion::{policy, NUMERIC_CLASSES};
use ironplc_container::{
    Container, ContainerBuilder, FbFieldUidEntry, FbTypeDescriptor, FbTypeId, FieldEntry,
    FieldType, FunctionId, UserFbDescriptor,
};
use ironplc_vm::Slot;
use rstest::rstest;

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
fn build_when_variable_type_changed_outside_policy_then_type_change_unsupported() {
    let base = container(&[i32_entry()], &[(0, 7)]);
    let mut candidate = container(&[i32_entry()], &[(0, 7)]);
    candidate.type_section.as_mut().unwrap().variable_table[0].var_type = FieldType::U32;

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::TypeChangeUnsupported {
            uid: 7,
            from: FieldType::I32,
            to: FieldType::U32,
        }
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

/// A scalar variable-table entry of the given storage class.
fn scalar_entry(var_type: FieldType) -> VarEntry {
    VarEntry {
        var_type,
        flags: 0,
        extra: 0,
    }
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

#[rstest]
#[case::signed_widen(FieldType::I32, FieldType::I64, ValueConversion::SignedWiden32To64)]
#[case::unsigned_widen(FieldType::U32, FieldType::U64, ValueConversion::UnsignedWiden32To64)]
#[case::int_to_real(FieldType::I32, FieldType::F32, ValueConversion::SignedToReal)]
#[case::int_to_lreal(FieldType::I32, FieldType::F64, ValueConversion::SignedToLReal)]
#[case::long_to_lreal(FieldType::I64, FieldType::F64, ValueConversion::LongToLReal)]
#[case::real_to_lreal(FieldType::F32, FieldType::F64, ValueConversion::RealToLReal)]
fn build_when_policy_pair_then_emits_convert_action(
    #[case] base_type: FieldType,
    #[case] candidate_type: FieldType,
    #[case] expected: ValueConversion,
) {
    let base = container(&[scalar_entry(base_type)], &[(0, 7)]);
    let candidate = container(&[scalar_entry(candidate_type)], &[(0, 7)]);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::Convert {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            conversion: expected,
        }]
    );
}

#[rstest]
#[case::signed_widen(
    FieldType::I32,
    FieldType::I64,
    Slot::from_i32(-30_000),
    Slot::from_i64(-30_000)
)]
#[case::unsigned_widen(
    FieldType::U32,
    FieldType::U64,
    Slot::from_u64(4_000_000_000),
    Slot::from_u64(4_000_000_000)
)]
#[case::int_to_real(
    FieldType::I32,
    FieldType::F32,
    Slot::from_i32(21),
    Slot::from_f32(21.0)
)]
#[case::int_to_lreal(
    FieldType::I32,
    FieldType::F64,
    Slot::from_i32(-21),
    Slot::from_f64(-21.0)
)]
#[case::long_to_lreal(
    FieldType::I64,
    FieldType::F64,
    Slot::from_i64(1_000_000_000_000),
    Slot::from_f64(1_000_000_000_000.0)
)]
#[case::real_to_lreal(
    FieldType::F32,
    FieldType::F64,
    Slot::from_f32(2.5),
    Slot::from_f64(2.5)
)]
fn apply_when_policy_pair_then_candidate_carries_converted_value(
    #[case] base_type: FieldType,
    #[case] candidate_type: FieldType,
    #[case] value: Slot,
    #[case] expected: Slot,
) {
    let base = container(&[scalar_entry(base_type)], &[(0, 7)]);
    let candidate = container(&[scalar_entry(candidate_type)], &[(0, 7)]);
    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    let mut base_buffers = VmBuffers::from_container(&base);
    base_buffers.vars[0] = value;
    let mut candidate_buffers = VmBuffers::from_container(&candidate);

    plan.apply(&base_buffers, &mut candidate_buffers).unwrap();

    assert_eq!(candidate_buffers.vars[0], expected);
}

#[rstest]
#[case::int_to_time(FieldType::I32, FieldType::Time)]
#[case::int_to_string(FieldType::I32, FieldType::String)]
#[case::string_width(FieldType::String, FieldType::WString)]
#[case::fb_instance(FieldType::FbInstance, FieldType::I32)]
fn build_when_pair_outside_policy_then_type_change_unsupported_named(
    #[case] base_type: FieldType,
    #[case] candidate_type: FieldType,
) {
    let base = container(&[scalar_entry(base_type)], &[(0, 7)]);
    let candidate = container(&[scalar_entry(candidate_type)], &[(0, 7)]);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::TypeChangeUnsupported {
            uid: 7,
            from: base_type,
            to: candidate_type,
        }
    );
}

/// Runs the exhaustive numeric reject matrix: every cross-class pair the
/// policy does not admit must reject with the pair named. `build` makes the
/// container pair for a class pair, so the scalar and array matrices share
/// the assertions; same-class pairs never reach the policy (the equal-entry
/// copy path serves them) and the admitted pairs are covered separately.
fn assert_cross_class_pairs_rejected(build: fn(FieldType, FieldType) -> (Container, Container)) {
    let mut rejected = 0;
    for base_type in NUMERIC_CLASSES {
        for candidate_type in NUMERIC_CLASSES {
            if base_type == candidate_type || policy(base_type, candidate_type).is_some() {
                continue;
            }
            let (base, candidate) = build(base_type, candidate_type);

            let result = StateMigrationPlan::build(&base, &candidate);

            assert_eq!(
                result.unwrap_err(),
                MigrationError::TypeChangeUnsupported {
                    uid: 7,
                    from: base_type,
                    to: candidate_type,
                },
                "pair {base_type:?} -> {candidate_type:?}"
            );
            rejected += 1;
        }
    }

    // 36 numeric class pairs - 6 same-class - 6 admitted conversions.
    assert_eq!(rejected, 24);
}

#[test]
fn build_when_every_cross_class_numeric_pair_outside_policy_then_type_change_unsupported_named() {
    assert_cross_class_pairs_rejected(|base_type, candidate_type| {
        (
            container(&[scalar_entry(base_type)], &[(0, 7)]),
            container(&[scalar_entry(candidate_type)], &[(0, 7)]),
        )
    });
}

#[test]
fn build_when_array_elements_in_policy_and_length_equal_then_convert_array() {
    let base = typed_array_container(FieldType::I32, 2, 16);
    let candidate = typed_array_container(FieldType::I64, 2, 16);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::ConvertArray {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            conversion: ValueConversion::SignedWiden32To64,
            byte_size: 16,
        }]
    );
}

#[test]
fn apply_when_array_elements_in_policy_then_each_element_converts() {
    let base = typed_array_container(FieldType::I32, 2, 16);
    let candidate = typed_array_container(FieldType::I64, 2, 16);
    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    let mut base_buffers = VmBuffers::from_container(&base);
    base_buffers.vars[0] = Slot::from_i32(0);
    base_buffers.data_region[0..8].copy_from_slice(&Slot::from_i32(-11).as_u64().to_le_bytes());
    base_buffers.data_region[8..16].copy_from_slice(&Slot::from_i32(22).as_u64().to_le_bytes());
    let mut candidate_buffers = VmBuffers::from_container(&candidate);
    candidate_buffers.vars[0] = Slot::from_i32(0);

    plan.apply(&base_buffers, &mut candidate_buffers).unwrap();

    assert_eq!(
        candidate_buffers.data_region[0..8],
        Slot::from_i64(-11).as_u64().to_le_bytes()
    );
    assert_eq!(
        candidate_buffers.data_region[8..16],
        Slot::from_i64(22).as_u64().to_le_bytes()
    );
}

#[test]
fn build_when_array_length_changes_with_in_policy_elements_then_descriptor_mismatch() {
    // The policy admits the element pair, but an aggregate whose length
    // changed cannot carry values element-wise: the shapes differ.
    let base = typed_array_container(FieldType::I32, 2, 16);
    let candidate = typed_array_container(FieldType::I64, 3, 24);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::ArrayDescriptorMismatch { uid: 7 }
    );
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
fn build_when_array_element_to_string_then_type_change_unsupported_named() {
    let base = typed_array_container(FieldType::I32, 2, 16);
    let candidate = typed_array_container(FieldType::String, 2, 16);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::TypeChangeUnsupported {
            uid: 7,
            from: FieldType::I32,
            to: FieldType::String,
        }
    );
}

#[test]
fn build_when_every_numeric_array_pair_outside_policy_then_type_change_unsupported_named() {
    assert_cross_class_pairs_rejected(|base_type, candidate_type| {
        (
            typed_array_container(base_type, 2, 16),
            typed_array_container(candidate_type, 2, 16),
        )
    });
}

#[test]
fn build_when_string_width_changes_then_type_change_unsupported_named() {
    // STRING -> WSTRING is a type-tag change outside the policy: strings
    // migrate only within the same width and maximum length.
    let base = container(&[string_entry(10)], &[(0, 7)]);
    let candidate = container(
        &[VarEntry {
            var_type: FieldType::WString,
            flags: 0,
            extra: 10,
        }],
        &[(0, 7)],
    );

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::TypeChangeUnsupported {
            uid: 7,
            from: FieldType::String,
            to: FieldType::WString,
        }
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
            },
            MigrationAction::FbField {
                from_index: VarIndex::new(0),
                to_index: VarIndex::new(0),
                from_field: 1,
                to_field: 2,
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
fn build_when_shared_fb_field_retyped_then_incompatible_entry() {
    // A shared field UID whose variable-table entry changed rejects with the
    // same typed error as a retyped program variable. The candidate also
    // grows the type so the per-field path (not the identical-descriptor
    // whole-region path) classifies the field.
    let base = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 1))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_fb_field_uid(field_uid(0x1000, 0, 101))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(2)
        .build();
    let candidate = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(VarEntry {
            var_type: FieldType::F64,
            flags: 0,
            extra: 0,
        })
        .add_var_entry(i32_entry())
        .add_fb_field_uid(field_uid(0x1000, 0, 101))
        .add_fb_field_uid(field_uid(0x1000, 1, 103))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(3)
        .build();

    let result = StateMigrationPlan::build(&base, &candidate);

    assert!(matches!(
        result,
        Err(MigrationError::IncompatibleEntry { uid: 101, .. })
    ));
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
            uid: 7,
            from: FieldType::I32,
            to: FieldType::String,
        }
        .to_string(),
        "entity 7 cannot change type from I32 to STRING: the conversion is outside the migration policy"
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
