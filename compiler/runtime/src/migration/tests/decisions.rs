//! Type-change and migration-decision tests for the state migration planner.
//!
//! Split from the parent test module because the ADR-0060 conversion policy
//! and the ADR-0061 decision vocabulary form their own area. The parent
//! module keeps the copy paths, the structural rejects, and the FB layout
//! rules; this module pins the conversion table, the collected-pair payload,
//! and every decision outcome.

use std::collections::BTreeMap;

use super::*;
use crate::conversion::{policy, ValueConversion, NUMERIC_CLASSES};
use ironplc_container::debug_section::{function_id, iec_type_tag, var_section, VarNameEntry};
use ironplc_container::{Container, ContainerBuilder, StableVarEntry, VarEntry, VarIndex};
use ironplc_vm::VmBuffers;
use rstest::rstest;

/// A scalar variable-table entry of the given storage class.
fn scalar_entry(var_type: FieldType) -> VarEntry {
    VarEntry {
        var_type,
        flags: 0,
        extra: 0,
    }
}

/// A container with one user FB instance (uid 7) whose type maps `fields`
/// fields at `var_offset` 1, carrying the given field UIDs.
fn fb_container(fields: &[FieldType], field_uids: &[(u8, u64)]) -> Container {
    let mut builder = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, fields.len() as u8))
        .add_var_entry(fb_entry(0x1000));
    for field_type in fields {
        builder = builder.add_var_entry(VarEntry {
            var_type: *field_type,
            flags: 0,
            extra: 0,
        });
    }
    for (index, uid) in field_uids {
        builder = builder.add_fb_field_uid(field_uid(0x1000, *index, *uid));
    }
    builder
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables((1 + fields.len()) as u16)
        .build()
}

/// A container like [`container`] whose variables also carry debug names.
fn named_container(
    variables: &[VarEntry],
    stable: &[(u16, u64)],
    names: &[(u16, &str)],
) -> Container {
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
    for (index, name) in names {
        builder = builder.add_var_name(VarNameEntry {
            var_index: VarIndex::new(*index),
            function_id: function_id::GLOBAL_SCOPE,
            var_section: var_section::VAR,
            iec_type_tag: iec_type_tag::OTHER,
            name: (*name).to_string(),
            type_name: String::new(),
        });
    }
    builder.num_variables(variables.len() as u16).build()
}

/// A decision map from `(uid, decision)` pairs.
fn decisions(entries: &[(u64, MigrationDecision)]) -> BTreeMap<u64, MigrationDecision> {
    entries.iter().copied().collect()
}

/// The independent width-family specification: `I32`/`U32`/`F32` are 32-bit,
/// `I64`/`U64`/`F64` are 64-bit, every other class is outside the families a
/// preserve decision may copy.
fn same_width_family(from: FieldType, to: FieldType) -> bool {
    let family = |field_type: FieldType| match field_type {
        FieldType::I32 | FieldType::U32 | FieldType::F32 => Some(32u8),
        FieldType::I64 | FieldType::U64 | FieldType::F64 => Some(64u8),
        _ => None,
    };
    family(from).is_some() && family(from) == family(to)
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
#[case::int_to_time(FieldType::I32, FieldType::Time, false)]
#[case::int_to_string(FieldType::I32, FieldType::String, false)]
#[case::string_width(FieldType::String, FieldType::WString, false)]
#[case::fb_instance(FieldType::FbInstance, FieldType::I32, false)]
#[case::int_to_udint(FieldType::I32, FieldType::U32, true)]
#[case::udint_to_dint(FieldType::U32, FieldType::I32, true)]
#[case::long_to_ulint(FieldType::I64, FieldType::U64, true)]
fn build_when_pair_outside_policy_then_all_pairs_reported_named(
    #[case] base_type: FieldType,
    #[case] candidate_type: FieldType,
    #[case] size_equal: bool,
) {
    let base = container(&[scalar_entry(base_type)], &[(0, 7)]);
    let candidate = container(&[scalar_entry(candidate_type)], &[(0, 7)]);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::TypeChangeUnsupported {
            pairs: vec![TypeChangePair {
                uid: 7,
                name: None,
                from: base_type,
                to: candidate_type,
                size_equal,
            }],
        }
    );
}

/// Runs the exhaustive numeric reject matrix: every cross-class pair the
/// policy does not admit must reject with every offending pair named.
/// `size_equal` is the independent width-family specification, so the pair
/// payload is checked against the decision legality, not the implementation.
fn assert_cross_class_pairs_rejected(
    build: fn(FieldType, FieldType) -> (Container, Container),
    size_equal: fn(FieldType, FieldType) -> bool,
) {
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
                    pairs: vec![TypeChangePair {
                        uid: 7,
                        name: None,
                        from: base_type,
                        to: candidate_type,
                        size_equal: size_equal(base_type, candidate_type),
                    }],
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
fn build_when_every_cross_class_numeric_pair_outside_policy_then_all_pairs_reported() {
    assert_cross_class_pairs_rejected(
        |base_type, candidate_type| {
            (
                container(&[scalar_entry(base_type)], &[(0, 7)]),
                container(&[scalar_entry(candidate_type)], &[(0, 7)]),
            )
        },
        same_width_family,
    );
}

#[test]
fn build_when_every_numeric_array_pair_outside_policy_then_all_pairs_reported() {
    assert_cross_class_pairs_rejected(
        |base_type, candidate_type| {
            (
                typed_array_container(base_type, 2, 16),
                typed_array_container(candidate_type, 2, 16),
            )
        },
        same_width_family,
    );
}

#[test]
fn build_when_two_pairs_outside_policy_then_error_carries_both_with_names() {
    let base = named_container(
        &[i32_entry(), i32_entry()],
        &[(0, 7), (1, 9)],
        &[(0, "Counter"), (1, "Flag")],
    );
    let mut candidate = named_container(
        &[i32_entry(), i32_entry()],
        &[(0, 7), (1, 9)],
        &[(0, "Counter"), (1, "Flag")],
    );
    let table = &mut candidate.type_section.as_mut().unwrap().variable_table;
    table[0].var_type = FieldType::U32;
    table[1].var_type = FieldType::String;
    table[1].extra = 10;

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::TypeChangeUnsupported {
            pairs: vec![
                TypeChangePair {
                    uid: 7,
                    name: Some("Counter".into()),
                    from: FieldType::I32,
                    to: FieldType::U32,
                    size_equal: true,
                },
                TypeChangePair {
                    uid: 9,
                    name: Some("Flag".into()),
                    from: FieldType::I32,
                    to: FieldType::String,
                    size_equal: false,
                },
            ],
        }
    );
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
fn build_when_array_element_to_string_then_pair_reported_named() {
    let base = typed_array_container(FieldType::I32, 2, 16);
    let candidate = typed_array_container(FieldType::String, 2, 16);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::TypeChangeUnsupported {
            pairs: vec![TypeChangePair {
                uid: 7,
                name: None,
                from: FieldType::I32,
                to: FieldType::String,
                size_equal: false,
            }],
        }
    );
}

#[test]
fn build_when_string_width_changes_then_pair_reported_named() {
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
            pairs: vec![TypeChangePair {
                uid: 7,
                name: None,
                from: FieldType::String,
                to: FieldType::WString,
                size_equal: false,
            }],
        }
    );
}

#[rstest]
#[case::init(MigrationDecision::Init, vec![])]
#[case::preserve(
    MigrationDecision::Preserve,
    vec![MigrationAction::Slot {
        from_index: VarIndex::new(0),
        to_index: VarIndex::new(0),
    }]
)]
fn build_with_decisions_when_decision_overrides_convertible_pair_then_decision_wins(
    #[case] decision: MigrationDecision,
    #[case] expected: Vec<MigrationAction>,
) {
    let base = container(&[scalar_entry(FieldType::I32)], &[(0, 7)]);
    let candidate = container(&[scalar_entry(FieldType::F32)], &[(0, 7)]);
    let decisions = decisions(&[(7, decision)]);

    let plan = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions).unwrap();

    assert_eq!(plan.actions, expected);
}

#[test]
fn build_with_decisions_when_init_on_out_of_policy_pair_then_no_action_for_it() {
    let base = container(&[i32_entry(), i32_entry()], &[(0, 7), (1, 9)]);
    let mut candidate = container(&[i32_entry(), i32_entry()], &[(0, 7), (1, 9)]);
    candidate.type_section.as_mut().unwrap().variable_table[0].var_type = FieldType::U32;
    let decisions = decisions(&[(7, MigrationDecision::Init)]);

    let plan = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions).unwrap();

    // Only the untouched UID 9 copies; UID 7's candidate init image stands.
    assert_eq!(
        plan.actions,
        vec![MigrationAction::Slot {
            from_index: VarIndex::new(1),
            to_index: VarIndex::new(1),
        }]
    );
}

#[test]
fn build_with_decisions_when_preserve_on_size_equal_pair_then_plain_copy() {
    let base = container(&[scalar_entry(FieldType::I32)], &[(0, 7)]);
    let candidate = container(&[scalar_entry(FieldType::F32)], &[(0, 7)]);
    let decisions = decisions(&[(7, MigrationDecision::Preserve)]);

    let plan = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::Slot {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
        }]
    );
}

#[test]
fn build_with_decisions_when_preserve_on_size_mismatch_then_error() {
    let base = container(&[scalar_entry(FieldType::I32)], &[(0, 7)]);
    let candidate = container(&[scalar_entry(FieldType::I64)], &[(0, 7)]);
    let decisions = decisions(&[(7, MigrationDecision::Preserve)]);

    let result = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::PreserveSizeMismatch { uid: 7 }
    );
}

#[test]
fn build_with_decisions_when_uid_has_no_change_then_unknown_uid() {
    let base = container(&[i32_entry()], &[(0, 7)]);
    let candidate = container(&[i32_entry()], &[(0, 7)]);
    let decisions = decisions(&[(99, MigrationDecision::Init)]);

    let result = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::UnknownDecisionUid { uid: 99 }
    );
}

#[test]
fn build_with_decisions_when_numeric_pair_init_then_no_actions() {
    for base_type in NUMERIC_CLASSES {
        for candidate_type in NUMERIC_CLASSES {
            if base_type == candidate_type {
                continue;
            }
            let base = container(&[scalar_entry(base_type)], &[(0, 7)]);
            let candidate = container(&[scalar_entry(candidate_type)], &[(0, 7)]);
            let decisions = decisions(&[(7, MigrationDecision::Init)]);

            let plan =
                StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions).unwrap();

            assert!(
                plan.actions.is_empty(),
                "pair {base_type:?} -> {candidate_type:?}"
            );
        }
    }
}

#[test]
fn build_with_decisions_when_numeric_pair_preserve_then_matches_width_families() {
    for base_type in NUMERIC_CLASSES {
        for candidate_type in NUMERIC_CLASSES {
            if base_type == candidate_type {
                continue;
            }
            let base = container(&[scalar_entry(base_type)], &[(0, 7)]);
            let candidate = container(&[scalar_entry(candidate_type)], &[(0, 7)]);
            let decisions = decisions(&[(7, MigrationDecision::Preserve)]);

            let result = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions);

            if same_width_family(base_type, candidate_type) {
                assert_eq!(
                    result.unwrap().actions,
                    vec![MigrationAction::Slot {
                        from_index: VarIndex::new(0),
                        to_index: VarIndex::new(0),
                    }],
                    "pair {base_type:?} -> {candidate_type:?}"
                );
            } else {
                assert_eq!(
                    result.unwrap_err(),
                    MigrationError::PreserveSizeMismatch { uid: 7 },
                    "pair {base_type:?} -> {candidate_type:?}"
                );
            }
        }
    }
}

#[test]
fn build_with_decisions_when_init_overrides_convertible_array_then_no_actions() {
    let base = typed_array_container(FieldType::I32, 2, 16);
    let candidate = typed_array_container(FieldType::I64, 2, 16);
    let decisions = decisions(&[(7, MigrationDecision::Init)]);

    let plan = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions).unwrap();

    assert!(plan.actions.is_empty());
}

#[test]
fn build_with_decisions_when_preserve_overrides_convertible_array_then_region_copy() {
    let base = typed_array_container(FieldType::I32, 2, 16);
    let candidate = typed_array_container(FieldType::F32, 2, 16);
    let decisions = decisions(&[(7, MigrationDecision::Preserve)]);

    let plan = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::Array {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            byte_size: 16,
        }]
    );
}

#[test]
fn build_with_decisions_when_preserve_array_length_changes_then_error() {
    let base = typed_array_container(FieldType::I32, 2, 16);
    let candidate = typed_array_container(FieldType::F32, 3, 24);
    let decisions = decisions(&[(7, MigrationDecision::Preserve)]);

    let result = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::PreserveSizeMismatch { uid: 7 }
    );
}

#[test]
fn build_with_decisions_when_preserve_array_element_widths_differ_then_error() {
    let base = typed_array_container(FieldType::I32, 2, 16);
    let candidate = typed_array_container(FieldType::I64, 2, 16);
    let decisions = decisions(&[(7, MigrationDecision::Preserve)]);

    let result = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::PreserveSizeMismatch { uid: 7 }
    );
}

#[test]
fn build_when_fb_field_retyped_to_convertible_pair_then_field_converts() {
    let base = fb_container(&[FieldType::I32], &[(0, 101)]);
    let candidate = fb_container(&[FieldType::F64, FieldType::I32], &[(0, 101), (1, 103)]);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::FbField {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            from_field: 0,
            to_field: 0,
            conversion: Some(ValueConversion::SignedToLReal),
        }]
    );
}

#[test]
fn apply_when_fb_field_retyped_to_convertible_pair_then_field_value_converts() {
    let base = fb_container(&[FieldType::I32], &[(0, 101)]);
    let candidate = fb_container(&[FieldType::F64, FieldType::I32], &[(0, 101), (1, 103)]);
    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    let mut base_buffers = buffers(vec![Slot::from_i32(0), Slot::from_i32(5)], 16);
    base_buffers.data_region[0..8].copy_from_slice(&Slot::from_i32(5).as_u64().to_le_bytes());
    let mut candidate_buffers =
        buffers(vec![Slot::default(), Slot::default(), Slot::default()], 16);

    plan.apply(&base_buffers, &mut candidate_buffers).unwrap();

    let bits = u64::from_le_bytes(candidate_buffers.data_region[0..8].try_into().unwrap());
    assert_eq!(Slot::from_u64(bits), Slot::from_f64(5.0));
}

#[test]
fn build_with_decisions_when_fb_field_init_then_no_field_action() {
    let base = fb_container(&[FieldType::I32], &[(0, 101)]);
    let candidate = fb_container(&[FieldType::F64, FieldType::I32], &[(0, 101), (1, 103)]);
    let decisions = decisions(&[(101, MigrationDecision::Init)]);

    let plan = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions).unwrap();

    assert!(plan.actions.is_empty());
}

#[test]
fn build_with_decisions_when_fb_field_preserve_then_field_slot_copy() {
    let base = fb_container(&[FieldType::I32], &[(0, 101)]);
    let candidate = fb_container(&[FieldType::U32, FieldType::I32], &[(0, 101), (1, 103)]);
    let decisions = decisions(&[(101, MigrationDecision::Preserve)]);

    let plan = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::FbField {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
            from_field: 0,
            to_field: 0,
            conversion: None,
        }]
    );
}

#[test]
fn build_when_fb_field_retyped_outside_policy_then_pair_reported_named() {
    let base = fb_container(&[FieldType::I32], &[(0, 101)]);
    let candidate = fb_container(&[FieldType::U32, FieldType::I32], &[(0, 101), (1, 103)]);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::TypeChangeUnsupported {
            pairs: vec![TypeChangePair {
                uid: 101,
                name: None,
                from: FieldType::I32,
                to: FieldType::U32,
                size_equal: true,
            }],
        }
    );
}

#[test]
fn build_with_decisions_when_dint_to_real_init_then_candidate_init_image_stands() {
    let base = container(&[scalar_entry(FieldType::I32)], &[(0, 7)]);
    let candidate = container(&[scalar_entry(FieldType::F32)], &[(0, 7)]);
    let decisions = decisions(&[(7, MigrationDecision::Init)]);
    let plan = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions).unwrap();

    let mut base_buffers = VmBuffers::from_container(&base);
    base_buffers.vars[0] = Slot::from_i32(123);
    let mut candidate_buffers = VmBuffers::from_container(&candidate);
    candidate_buffers.vars[0] = Slot::from_f32(5.0);

    plan.apply(&base_buffers, &mut candidate_buffers).unwrap();

    assert_eq!(candidate_buffers.vars[0], Slot::from_f32(5.0));
}

#[test]
fn build_with_decisions_when_dint_to_real_preserve_then_old_bits_are_reinterpreted() {
    let base = container(&[scalar_entry(FieldType::I32)], &[(0, 7)]);
    let candidate = container(&[scalar_entry(FieldType::F32)], &[(0, 7)]);
    let decisions = decisions(&[(7, MigrationDecision::Preserve)]);
    let plan = StateMigrationPlan::build_with_decisions(&base, &candidate, &decisions).unwrap();

    let mut base_buffers = VmBuffers::from_container(&base);
    base_buffers.vars[0] = Slot::from_i32(123);
    let mut candidate_buffers = VmBuffers::from_container(&candidate);

    plan.apply(&base_buffers, &mut candidate_buffers).unwrap();

    // 123 = 0x0000_007B: preserved verbatim, it reads as f32 ~1.72e-43.
    let bits = candidate_buffers.vars[0].as_u64() as u32;
    assert_eq!(bits, 123);
    assert!((1.7e-43..1.8e-43).contains(&f32::from_bits(bits)));
}
