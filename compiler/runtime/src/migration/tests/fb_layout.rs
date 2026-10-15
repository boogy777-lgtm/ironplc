//! Tests of the function block layout rules (ADR-0059): instance copies, per-field copies, and layout refusals.

use super::*;

#[test]
fn build_when_fb_tail_identical_then_copy_slot_and_region() {
    let base = ContainerBuilder::new()
        .shared_globals_size(2)
        .add_user_fb_type(user_fb(0x1000, 2))
        .add_fb_type(fb_type(0x1000, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(2)
        .build()
        .identified();
    let candidate = ContainerBuilder::new()
        .shared_globals_size(2)
        .add_user_fb_type(user_fb(0x1000, 2))
        .add_fb_type(fb_type(0x1000, 2))
        .add_var_entry(i32_entry())
        .add_var_entry(fb_entry(0x1000))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(1),
            uid: 7,
        })
        .num_variables(2)
        .build()
        .identified();

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
fn build_when_fb_working_slots_move_but_fields_equal_then_copy_slot_and_region() {
    // The candidate adds a function that sits before the block's working slots
    // in the variable table, so the descriptor's `var_offset` and the body's
    // function ID differ. Neither is the layout of an instance: the lists of
    // fields are equal, so the instance is carried as it is, with no field
    // UIDs involved.
    let base = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 2))
        .add_fb_type(fb_type(0x1000, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(3)
        .build()
        .identified();
    let candidate = ContainerBuilder::new()
        .add_user_fb_type(UserFbDescriptor {
            function_id: FunctionId::new(3),
            ..user_fb_at(0x1000, 4, 2)
        })
        .add_fb_type(fb_type(0x1000, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(6)
        .build()
        .identified();

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
fn build_when_fb_has_string_field_then_fb_layout_unsupported() {
    // A STRING field's slot holds the offset of the string inside the
    // instance; a slot carried over from the other container would point into
    // that container's layout, so the instance is not migrated.
    let container = || {
        ContainerBuilder::new()
            .shared_globals_size(1)
            .add_user_fb_type(user_fb_at(0x1000, 1, 1))
            .add_fb_type(FbTypeDescriptor {
                type_id: FbTypeId::new(0x1000),
                fields: vec![FieldEntry {
                    field_type: FieldType::String,
                    field_extra: 10,
                }],
            })
            .add_var_entry(fb_entry(0x1000))
            .add_var_entry(string_entry(10))
            .add_stable_var(StableVarEntry {
                var_index: VarIndex::new(0),
                uid: 7,
            })
            .num_variables(2)
            .build()
            .identified()
    };

    let result = StateMigrationPlan::build(&container(), &container());

    assert_eq!(result.unwrap_err(), MigrationError::FbLayoutUnsupported);
}

#[test]
fn build_when_standard_fb_instance_and_layout_after_prefix_changes_then_fb_layout_unsupported() {
    // A standard-library block has no user descriptor, so its fields are not
    // identified by UID and the layout after the program prefix must be the
    // same for the instance to be carried.
    let base = ContainerBuilder::new()
        .add_var_entry(fb_entry(0x0010))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build()
        .identified();
    let candidate = ContainerBuilder::new()
        .add_var_entry(fb_entry(0x0010))
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(2)
        .build()
        .identified();

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(result.unwrap_err(), MigrationError::FbLayoutUnsupported);
}

#[test]
fn build_when_no_fb_instance_then_tail_change_is_ignored() {
    let base = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_user_fb_type(user_fb(0x1000, 2))
        .add_fb_type(fb_type(0x1000, 2))
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build()
        .identified();
    let candidate = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_user_fb_type(user_fb(0x1000, 3))
        .add_fb_type(fb_type(0x1000, 3))
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build()
        .identified();

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
        .add_fb_type(fb_type(0x1000, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build()
        .identified();
    let candidate = ContainerBuilder::new()
        .shared_globals_size(2)
        .add_user_fb_type(user_fb(0x1000, 2))
        .add_fb_type(fb_type(0x1000, 2))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(2)
        .build()
        .identified();

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
        .add_fb_type(fb_type(0x1000, 2))
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
        .build()
        .identified();
    let candidate = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 3))
        .add_fb_type(fb_type(0x1000, 3))
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
        .build()
        .identified();

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
fn build_when_fb_field_uid_unknown_and_layout_differs_then_refused_as_unidentified() {
    // The candidate's fields 1 and 2 carry no UID while the layout differs
    // underneath them: the value's identity is unprovable, so the candidate is
    // refused (ADR 0059's fail-closed rule, which is the identity rule of
    // ADR-0074 for the fields of a function block).
    let base = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 2))
        .add_fb_type(fb_type(0x1000, 2))
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
        .build()
        .identified();
    let candidate = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 3))
        .add_fb_type(fb_type(0x1000, 3))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_var_entry(i32_entry())
        .add_fb_field_uid(field_uid(0x1000, 0, 101))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(1),
            uid: 8,
        })
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(2),
            uid: 9,
        })
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(3),
            uid: 10,
        })
        .num_variables(4)
        .build();

    let result = StateMigrationPlan::build(&base, &candidate);

    let fields: Vec<IdentitySubject> = match result {
        Err(MigrationError::UnidentifiedVariables { variables }) => {
            variables.iter().map(|variable| variable.subject).collect()
        }
        _ => Vec::new(),
    };
    assert_eq!(
        fields,
        vec![
            IdentitySubject::FbField {
                type_id: FbTypeId::new(0x1000),
                ordinal: 1
            },
            IdentitySubject::FbField {
                type_id: FbTypeId::new(0x1000),
                ordinal: 2
            },
        ]
    );
}

#[test]
fn build_when_fb_field_type_details_change_then_incompatible_entry() {
    // A nested block field keeps its type tag while the block it holds
    // changes: that is a structural change of the field, not a storage-class
    // change the policy could convert, so the planner rejects it.
    let fb_type_with = |nested: u16| FbTypeDescriptor {
        type_id: FbTypeId::new(0x1000),
        fields: vec![FieldEntry {
            field_type: FieldType::FbInstance,
            field_extra: nested,
        }],
    };
    let base = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 1))
        .add_fb_type(fb_type_with(0x1001))
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(fb_entry(0x1001))
        .add_fb_field_uid(field_uid(0x1000, 0, 101))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(2)
        .build()
        .identified();
    let candidate = ContainerBuilder::new()
        .add_user_fb_type(user_fb_at(0x1000, 1, 2))
        .add_fb_type(FbTypeDescriptor {
            type_id: FbTypeId::new(0x1000),
            fields: vec![
                FieldEntry {
                    field_type: FieldType::FbInstance,
                    field_extra: 0x1002,
                },
                FieldEntry {
                    field_type: FieldType::I32,
                    field_extra: 0,
                },
            ],
        })
        .add_var_entry(fb_entry(0x1000))
        .add_var_entry(fb_entry(0x1002))
        .add_var_entry(i32_entry())
        .add_fb_field_uid(field_uid(0x1000, 0, 101))
        .add_fb_field_uid(field_uid(0x1000, 1, 102))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(3)
        .build()
        .identified();

    let result = StateMigrationPlan::build(&base, &candidate);

    assert_eq!(
        result.unwrap_err(),
        MigrationError::IncompatibleEntry {
            uid: 101,
            reason: "the variable's type details changed",
        }
    );
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
        .build()
        .identified();
    let candidate = ContainerBuilder::new()
        .shared_globals_size(1)
        .add_fb_type(fb_type(0x0010, 3))
        .add_var_entry(fb_entry(0x0010))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .build()
        .identified();

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
        .build()
        .identified();
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
        .build()
        .identified();

    let plan = StateMigrationPlan::build(&base, &candidate).unwrap();

    assert_eq!(
        plan.actions,
        vec![MigrationAction::Slot {
            from_index: VarIndex::new(0),
            to_index: VarIndex::new(0),
        }]
    );
}
