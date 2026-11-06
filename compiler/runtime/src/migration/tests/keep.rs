//! Tests of the keep rule of a replace (ADR-0088): which `PERSISTENT` values
//! survive into the application that replaces the running one, and the list of
//! those that do not.

use ironplc_container::{RetainKind, RetainedRun};

use super::*;

/// One variable of a hand-built container: its table entry, its stable ID (if
/// it has one) and how it is declared (if it is retained at all).
struct Declared {
    entry: VarEntry,
    uid: Option<u64>,
    kind: Option<RetainKind>,
}

fn persistent(entry: VarEntry, uid: u64) -> Declared {
    Declared {
        entry,
        uid: Some(uid),
        kind: Some(RetainKind::Persistent),
    }
}

/// A container of the given variables, 32 data bytes, with the stable IDs and the
/// retained slot runs the declarations give.
fn declared(variables: &[Declared]) -> Container {
    let mut builder = ContainerBuilder::new();
    for (index, variable) in variables.iter().enumerate() {
        let slot = index as u16;
        builder = builder.add_var_entry(variable.entry.clone());
        if let Some(uid) = variable.uid {
            builder = builder.add_stable_var(StableVarEntry {
                var_index: VarIndex::new(slot),
                uid,
            });
        }
        if let Some(kind) = variable.kind {
            builder = builder.add_retained_run(RetainedRun::slots(kind, u32::from(slot), 1));
        }
    }
    builder
        .num_variables(variables.len() as u16)
        .data_region_bytes(32)
        .build()
}

/// The values of the slots of a buffer set, as `i32`.
fn values(buffers: &VmBuffers) -> Vec<i32> {
    buffers.vars.iter().map(|slot| slot.as_i32()).collect()
}

/// A buffer set for `container` whose slots hold `slots`.
fn filled(container: &Container, slots: &[i32]) -> VmBuffers {
    let mut buffers = VmBuffers::from_container(container);
    for (slot, value) in buffers.vars.iter_mut().zip(slots) {
        *slot = Slot::from_i32(*value);
    }
    buffers
}

fn i64_entry() -> VarEntry {
    VarEntry {
        var_type: FieldType::I64,
        flags: 0,
        extra: 0,
    }
}

#[test]
fn replace_keep_plan_when_persistent_variable_is_equal_then_kept_whatever_its_slot() {
    let running = declared(&[persistent(i32_entry(), 7)]);
    // The candidate declares a new variable first, so the variable moves to
    // slot 1.
    let candidate = declared(&[persistent(i32_entry(), 9), persistent(i32_entry(), 7)]);

    let keep = replace_keep_plan(&running, &candidate).unwrap();
    let mut fresh = filled(&candidate, &[5, 0]);
    keep.apply(&filled(&running, &[42]), &mut fresh).unwrap();

    assert_eq!(keep.kept(), [7]);
    assert!(keep.reinitialised().is_empty());
    // The added variable keeps the value its init image gave it.
    assert_eq!(values(&fresh), vec![5, 42]);
}

#[test]
fn replace_keep_plan_when_type_changed_then_reinitialised_as_type_changed() {
    let running = declared(&[persistent(i32_entry(), 7)]);
    let candidate = declared(&[persistent(i64_entry(), 7)]);

    let keep = replace_keep_plan(&running, &candidate).unwrap();
    let mut fresh = filled(&candidate, &[3]);
    keep.apply(&filled(&running, &[42]), &mut fresh).unwrap();

    assert!(keep.kept().is_empty());
    assert_eq!(
        keep.reinitialised(),
        [Reinitialised {
            uid: Some(7),
            name: None,
            reason: ReinitReason::TypeChanged
        }]
    );
    assert_eq!(values(&fresh), vec![3]);
}

#[test]
fn replace_keep_plan_when_string_length_changed_then_reinitialised_as_type_changed() {
    let running = declared(&[persistent(string_entry(10), 7)]);
    let candidate = declared(&[persistent(string_entry(20), 7)]);

    let keep = replace_keep_plan(&running, &candidate).unwrap();

    assert_eq!(keep.reinitialised()[0].reason, ReinitReason::TypeChanged);
}

#[test]
fn replace_keep_plan_when_variable_has_no_stable_id_then_reinitialised_as_no_stable_id() {
    let running = declared(&[Declared {
        entry: i32_entry(),
        uid: None,
        kind: Some(RetainKind::Persistent),
    }]);
    let candidate = declared(&[persistent(i32_entry(), 7)]);

    let keep = replace_keep_plan(&running, &candidate).unwrap();

    assert!(keep.kept().is_empty());
    assert_eq!(
        keep.reinitialised(),
        [Reinitialised {
            uid: None,
            name: None,
            reason: ReinitReason::NoStableId
        }]
    );
}

#[test]
fn replace_keep_plan_when_candidate_has_no_stable_ids_then_reinitialised_as_no_stable_id() {
    let running = declared(&[persistent(i32_entry(), 7)]);
    let candidate = declared(&[Declared {
        entry: i32_entry(),
        uid: None,
        kind: Some(RetainKind::Persistent),
    }]);

    let keep = replace_keep_plan(&running, &candidate).unwrap();

    assert_eq!(keep.reinitialised()[0].reason, ReinitReason::NoStableId);
    assert_eq!(keep.reinitialised()[0].uid, Some(7));
}

#[test]
fn replace_keep_plan_when_variable_is_removed_then_reinitialised_as_not_in_candidate() {
    let running = declared(&[persistent(i32_entry(), 7), persistent(i32_entry(), 8)]);
    let candidate = declared(&[persistent(i32_entry(), 8)]);

    let keep = replace_keep_plan(&running, &candidate).unwrap();

    assert_eq!(keep.kept(), [8]);
    assert_eq!(
        keep.reinitialised(),
        [Reinitialised {
            uid: Some(7),
            name: None,
            reason: ReinitReason::NotInCandidate
        }]
    );
}

#[test]
fn replace_keep_plan_when_variable_is_retain_then_not_kept_and_not_listed() {
    let retain = |uid| Declared {
        entry: i32_entry(),
        uid: Some(uid),
        kind: Some(RetainKind::Retain),
    };
    let running = declared(&[retain(7)]);
    let candidate = declared(&[retain(7)]);

    let keep = replace_keep_plan(&running, &candidate).unwrap();
    let mut fresh = filled(&candidate, &[3]);
    keep.apply(&filled(&running, &[42]), &mut fresh).unwrap();

    assert!(keep.kept().is_empty());
    assert!(keep.reinitialised().is_empty());
    assert_eq!(values(&fresh), vec![3]);
}

#[test]
fn replace_keep_plan_when_candidate_drops_persistent_then_reinitialised_as_qualifier_changed() {
    let running = declared(&[persistent(i32_entry(), 7)]);
    let candidate = declared(&[Declared {
        entry: i32_entry(),
        uid: Some(7),
        kind: Some(RetainKind::Retain),
    }]);

    let keep = replace_keep_plan(&running, &candidate).unwrap();

    assert_eq!(
        keep.reinitialised()[0].reason,
        ReinitReason::QualifierChanged
    );
}

#[test]
fn replace_keep_plan_when_string_region_moved_then_value_follows_and_slot_stays() {
    // The string is the first variable in the running application and sits
    // after eight bytes of another variable in the candidate.
    let running = declared(&[persistent(string_entry(4), 7)]);
    let candidate = declared(&[persistent(i32_entry(), 9), persistent(string_entry(4), 7)]);
    let mut running_buffers = filled(&running, &[0]);
    running_buffers.data_region[0..10].copy_from_slice(b"\x04\x00\x02\x00\x01\x00WXYZ");
    let mut fresh = filled(&candidate, &[0, 8]);

    let keep = replace_keep_plan(&running, &candidate).unwrap();
    keep.apply(&running_buffers, &mut fresh).unwrap();

    assert_eq!(keep.kept(), [7]);
    assert_eq!(fresh.vars[1], Slot::from_i32(8));
    assert_eq!(&fresh.data_region[8..18], b"\x04\x00\x02\x00\x01\x00WXYZ");
}

#[test]
fn replace_keep_plan_when_array_descriptor_differs_then_reinitialised_as_type_changed() {
    let build = |elements: u32| {
        let mut builder = ContainerBuilder::new();
        builder.add_array_descriptor(FieldType::I32 as u8, elements, 0);
        builder
            .add_var_entry(array_entry(0))
            .add_stable_var(StableVarEntry {
                var_index: VarIndex::new(0),
                uid: 7,
            })
            .add_retained_run(RetainedRun::slots(RetainKind::Persistent, 0, 1))
            .num_variables(1)
            .data_region_bytes(64)
            .build()
    };

    let keep = replace_keep_plan(&build(2), &build(3)).unwrap();

    assert_eq!(keep.reinitialised()[0].reason, ReinitReason::TypeChanged);
}

#[test]
fn replace_keep_plan_when_array_descriptor_is_equal_at_another_index_then_kept() {
    // Two builds number their descriptors alike only by chance: the array of
    // eight elements is descriptor 0 in the running application and 1 in the
    // candidate.
    let mut running_builder = ContainerBuilder::new();
    running_builder.add_array_descriptor(FieldType::I32 as u8, 2, 0);
    let running = running_builder
        .add_var_entry(array_entry(0))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .add_retained_run(RetainedRun::slots(RetainKind::Persistent, 0, 1))
        .num_variables(1)
        .data_region_bytes(16)
        .build();
    let mut candidate_builder = ContainerBuilder::new();
    candidate_builder.add_array_descriptor(FieldType::I32 as u8, 5, 0);
    candidate_builder.add_array_descriptor(FieldType::I32 as u8, 2, 0);
    let candidate = candidate_builder
        .add_var_entry(array_entry(1))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .add_retained_run(RetainedRun::slots(RetainKind::Persistent, 0, 1))
        .num_variables(1)
        .data_region_bytes(16)
        .build();

    let keep = replace_keep_plan(&running, &candidate).unwrap();

    assert_eq!(keep.kept(), [7]);
    assert!(keep.reinitialised().is_empty());
}

#[test]
fn replace_keep_plan_when_standard_function_block_instance_then_reinitialised_as_unsupported() {
    let running = declared(&[persistent(fb_entry(3), 7)]);
    let candidate = declared(&[persistent(fb_entry(3), 7)]);

    let keep = replace_keep_plan(&running, &candidate).unwrap();

    assert_eq!(
        keep.reinitialised()[0].reason,
        ReinitReason::UnsupportedType
    );
}

#[test]
fn replace_keep_plan_when_stable_id_is_bound_twice_then_corrupt() {
    let running = declared(&[persistent(i32_entry(), 7), persistent(i32_entry(), 7)]);
    let candidate = declared(&[persistent(i32_entry(), 7)]);

    let result = replace_keep_plan(&running, &candidate);

    assert_eq!(result.unwrap_err(), MigrationError::DuplicateUid { uid: 7 });
}

#[test]
fn replace_keep_plan_when_nothing_is_persistent_then_keeps_and_lists_nothing() {
    let plain = || Declared {
        entry: i32_entry(),
        uid: Some(7),
        kind: None,
    };
    let running = declared(&[plain()]);
    let candidate = declared(&[plain()]);

    let keep = replace_keep_plan(&running, &candidate).unwrap();

    assert!(keep.kept().is_empty());
    assert!(keep.reinitialised().is_empty());
}

/// A container whose variable 0 is a `PERSISTENT` instance of the user-defined
/// block `0x1000` with `fields` I32 fields (field UIDs 101, 102, ...).
fn with_instance(fields: usize) -> Container {
    let mut builder = ContainerBuilder::new()
        .add_user_fb_type(user_fb(0x1000, fields as u8))
        .add_fb_type(fb_type(0x1000, fields))
        .add_var_entry(fb_entry(0x1000))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .add_retained_run(RetainedRun::slots(RetainKind::Persistent, 0, 1));
    for ordinal in 0..fields {
        builder = builder.add_fb_field_uid(field_uid(0x1000, ordinal as u8, 101 + ordinal as u64));
    }
    builder.num_variables(1).data_region_bytes(32).build()
}

#[test]
fn replace_keep_plan_when_user_instance_has_equal_fields_then_its_region_is_kept() {
    let running = with_instance(2);
    let candidate = with_instance(2);
    let mut running_buffers = VmBuffers::from_container(&running);
    running_buffers.vars[0] = Slot::from_i32(8);
    running_buffers.data_region[8..16].copy_from_slice(&11i64.to_le_bytes());
    running_buffers.data_region[16..24].copy_from_slice(&22i64.to_le_bytes());
    let mut fresh = VmBuffers::from_container(&candidate);
    fresh.vars[0] = Slot::from_i32(8);

    let keep = replace_keep_plan(&running, &candidate).unwrap();
    keep.apply(&running_buffers, &mut fresh).unwrap();

    assert_eq!(keep.kept(), [7]);
    assert_eq!(&fresh.data_region[8..16], &11i64.to_le_bytes());
    assert_eq!(&fresh.data_region[16..24], &22i64.to_le_bytes());
}

#[test]
fn replace_keep_plan_when_user_instance_gains_a_field_then_reinitialised_as_type_changed() {
    let keep = replace_keep_plan(&with_instance(2), &with_instance(3)).unwrap();

    assert_eq!(keep.reinitialised()[0].reason, ReinitReason::TypeChanged);
}
