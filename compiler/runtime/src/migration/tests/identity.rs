//! Tests of the identity rule (ADR-0074): a migration needs a stable ID for
//! every persistent variable of both containers.
//!
//! The containers here are built without [`Identified::identified`], because
//! the variables without an ID are the point.

use ironplc_container::debug_section::{
    function_id, iec_type_tag, var_section, FuncNameEntry, VarNameEntry,
};
use ironplc_container::{ContainerBuilder, PersistentExtent};

use super::*;

/// A variable-table container with the given `(var_index, uid)` IDs and
/// `(var_index, name)` debug names.
fn named(variables: usize, stable: &[(u16, u64)], names: &[(u16, &str)]) -> Container {
    let mut builder = ContainerBuilder::new();
    for _ in 0..variables {
        builder = builder.add_var_entry(i32_entry());
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
    builder.num_variables(variables as u16).build()
}

/// The variables the refusal lists, as `(side, name or position)`.
fn listed(result: Result<StateMigrationPlan, MigrationError>) -> Vec<(IdentitySide, String)> {
    match result {
        Err(MigrationError::UnidentifiedVariables { variables }) => variables
            .iter()
            .map(|variable| (variable.side, variable.to_string()))
            .collect(),
        _ => Vec::new(),
    }
}

#[test]
fn build_when_every_persistent_variable_has_an_id_then_plan_is_built() {
    let base = named(2, &[(0, 1), (1, 2)], &[]);
    let candidate = named(2, &[(0, 1), (1, 2)], &[]);

    assert!(StateMigrationPlan::build(&base, &candidate).is_ok());
}

#[test]
fn build_when_variable_has_no_id_on_either_side_then_refused_naming_it_on_both() {
    let base = named(2, &[(0, 1)], &[(0, "A"), (1, "B")]);
    let candidate = named(3, &[(0, 1)], &[(0, "A"), (1, "B"), (2, "C")]);

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        listed(result),
        vec![
            (IdentitySide::Active, "B".to_string()),
            (IdentitySide::Candidate, "B".to_string()),
            (IdentitySide::Candidate, "C".to_string()),
        ]
    );
}

#[test]
fn build_when_variable_has_no_id_only_in_active_then_refused() {
    let base = named(2, &[(0, 1)], &[(1, "Old")]);
    let candidate = named(1, &[(0, 1)], &[]);

    assert_eq!(
        listed(StateMigrationPlan::build(&base, &candidate)),
        vec![(IdentitySide::Active, "Old".to_string())]
    );
}

#[test]
fn build_when_variable_has_no_id_only_in_candidate_then_refused() {
    let base = named(1, &[(0, 1)], &[]);
    let candidate = named(2, &[(0, 1)], &[(1, "New")]);

    assert_eq!(
        listed(StateMigrationPlan::build(&base, &candidate)),
        vec![(IdentitySide::Candidate, "New".to_string())]
    );
}

#[test]
fn build_when_candidate_adds_a_variable_with_an_id_then_it_starts_from_its_initial_value() {
    // The new variable carries an ID the active side does not, which is what
    // makes it new: no copy, no refusal.
    let base = named(1, &[(0, 1)], &[]);
    let candidate = named(2, &[(0, 1), (1, 2)], &[]);

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(plan.actions.len(), 1);
}

#[test]
fn build_when_candidate_removes_a_variable_with_an_id_then_it_is_dropped() {
    let base = named(2, &[(0, 1), (1, 2)], &[]);
    let candidate = named(1, &[(0, 1)], &[]);

    assert!(StateMigrationPlan::build(&base, &candidate).is_ok());
}

#[test]
fn build_when_id_is_the_reserved_zero_then_the_variable_has_no_id() {
    let base = named(1, &[(0, 0)], &[(0, "Zero")]);
    let candidate = named(1, &[(0, 1)], &[]);

    assert_eq!(
        listed(StateMigrationPlan::build(&base, &candidate)),
        vec![(IdentitySide::Active, "Zero".to_string())]
    );
}

#[test]
fn build_when_container_has_no_names_then_variables_are_named_by_position() {
    let base = named(2, &[(0, 1)], &[]);
    let candidate = named(2, &[(0, 1)], &[]);

    let names = listed(StateMigrationPlan::build(&base, &candidate));

    assert_eq!(
        names,
        vec![
            (IdentitySide::Active, "variable 1".to_string()),
            (IdentitySide::Candidate, "variable 1".to_string()),
        ]
    );
}

#[test]
fn build_when_slot_is_outside_the_persistent_extent_then_it_needs_no_id() {
    // Slot 1 is a working slot of a function: it is not state.
    let extent = PersistentExtent {
        instance_id: ironplc_container::InstanceId::DEFAULT,
        var_start: 0,
        var_count: 1,
        data_start: 0,
        data_len: 0,
    };
    let working = |uid: u64| {
        ContainerBuilder::new()
            .add_var_entry(i32_entry())
            .add_var_entry(i32_entry())
            .add_stable_var(StableVarEntry {
                var_index: VarIndex::new(0),
                uid,
            })
            .add_persistent_extent(extent)
            .num_variables(2)
            .build()
    };

    assert!(StateMigrationPlan::build(&working(1), &working(1)).is_ok());
}

/// A container with one function block type of two fields; the first field has
/// the UID `first`, the second has `second`; the program holds the instance
/// (ID 7). The debug section names the block `holder` and its body slots
/// `count` and `limit` when `names` is set.
fn with_block(first: Option<u64>, second: Option<u64>, names: bool) -> Container {
    let mut builder = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 2))
        .add_fb_type(fb_type(0x1000, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .add_persistent_extent(PersistentExtent {
            instance_id: ironplc_container::InstanceId::DEFAULT,
            var_start: 0,
            var_count: 1,
            data_start: 0,
            data_len: 0,
        })
        .num_variables(3);
    for (ordinal, uid) in [first, second].into_iter().enumerate() {
        if let Some(uid) = uid {
            builder = builder.add_fb_field_uid(field_uid(0x1000, ordinal as u8, uid));
        }
    }
    if names {
        builder = builder.add_func_name(FuncNameEntry {
            function_id: FunctionId::new(2),
            name: "holder".to_string(),
        });
        for (index, name) in [(1u16, "count"), (2, "limit")] {
            builder = builder.add_var_name(VarNameEntry {
                var_index: VarIndex::new(index),
                function_id: FunctionId::new(2),
                var_section: var_section::VAR,
                iec_type_tag: iec_type_tag::OTHER,
                name: name.to_string(),
                type_name: String::new(),
            });
        }
    }
    builder.build()
}

#[test]
fn build_when_every_field_of_a_function_block_has_a_uid_then_plan_is_built() {
    let base = with_block(Some(1), Some(2), false);
    let candidate = with_block(Some(1), Some(2), false);

    assert!(StateMigrationPlan::build(&base, &candidate).is_ok());
}

#[test]
fn build_when_function_block_field_has_no_uid_then_refused_naming_block_and_field() {
    let base = with_block(Some(1), Some(2), true);
    let candidate = with_block(Some(1), None, true);

    assert_eq!(
        listed(StateMigrationPlan::build(&base, &candidate)),
        vec![(IdentitySide::Candidate, "holder.limit".to_string())]
    );
}

#[test]
fn build_when_function_block_field_has_no_uid_and_no_names_then_named_by_ordinal() {
    let base = with_block(None, Some(2), false);
    let candidate = with_block(Some(1), Some(2), false);

    assert_eq!(
        listed(StateMigrationPlan::build(&base, &candidate)),
        vec![(
            IdentitySide::Active,
            "field 0 of function block type 4096".to_string()
        )]
    );
}

#[test]
fn build_when_field_layout_is_identical_but_a_field_has_no_uid_then_refused() {
    // The same list of fields on both sides would copy the instance as a whole,
    // and the planner would not need the UID. The rule is the same for every
    // persistent variable: a field without an ID is refused.
    let base = with_block(None, None, false);
    let candidate = with_block(None, None, false);

    assert_eq!(
        listed(StateMigrationPlan::build(&base, &candidate)).len(),
        4
    );
}

#[test]
fn display_when_variables_on_both_sides_then_groups_them_by_side() {
    let base = named(2, &[(0, 1)], &[(1, "B")]);
    let candidate = named(3, &[(0, 1)], &[(1, "B"), (2, "C")]);

    let error = StateMigrationPlan::build(&base, &candidate).unwrap_err();

    assert_eq!(
        error.to_string(),
        "persistent variables without a stable ID: active application: B; candidate: B, C"
    );
}

#[test]
fn v_code_when_variables_are_unidentified_then_v4020_and_other_causes_v4010() {
    let base = named(1, &[], &[]);
    let candidate = named(1, &[(0, 1)], &[]);

    let unidentified = StateMigrationPlan::build(&base, &candidate).unwrap_err();

    assert_eq!(unidentified.v_code(), "V4020");
    assert_eq!(MigrationError::FbLayoutUnsupported.v_code(), "V4010");
}
