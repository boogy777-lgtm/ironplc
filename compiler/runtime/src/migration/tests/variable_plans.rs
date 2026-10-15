//! Tests of how the planner classifies each variable: copy by UID, region copies, and the structural rejects.

use super::*;

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
