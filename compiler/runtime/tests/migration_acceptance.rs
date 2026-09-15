//! Acceptance tests for declaration-level state migration (hot-edit stage 2).
//!
//! Each test compiles the base and candidate fixtures with the real project
//! pipeline, assigns engineering-side stable variable IDs (ADR 0053), and
//! round-trips the containers through the wire format, so the migration
//! planner sees exactly what a deployed artifact carries.
//!
//! The matrix: rename, diff-type name swap through a swapped UID table,
//! declaration reorder, add, remove, delete-plus-rename, string content
//! migration, and the rejects (retype, string shrink, FB field add, untest
//! after a migration test).

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

mod common;

use common::{compile_with_ids, compile_with_uid_keys, variable_index};
use ironplc_container::FieldType;
use ironplc_runtime::{HostMode, MigrationError, OnlineChangeError, RuntimeHost};

#[test]
fn test_when_same_type_rename_then_value_continues_from_the_old_entity() {
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    A : DINT;
  END_VAR
  A := A + 1;
END_PROGRAM
",
        &[("A", 7)],
    );
    let a = variable_index(&base, "A");
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(9, || 0).unwrap();
    assert_eq!(host.read_variable(a).unwrap(), 9);

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    B : DINT;
  END_VAR
  B := B + 1;
END_PROGRAM
",
        &[("B", 7)],
    );
    let b = variable_index(&candidate, "B");

    host.stage(candidate).unwrap();
    // A rename keeps the layout hash, so this is not a migration candidate.
    assert!(!host.status().migration);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    assert_eq!(host.read_variable(b).unwrap(), 10);
}

#[test]
fn test_when_names_swap_with_types_through_swapped_table_then_entities_keep_value_and_role() {
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    A : DINT;
    B : REAL;
    Scaled : DINT;
  END_VAR
  A := A + 1;
  B := B + 0.5;
  Scaled := REAL_TO_DINT(B * 10.0);
END_PROGRAM
",
        &[("A", 1), ("B", 2), ("Scaled", 3)],
    );
    let a_dint = variable_index(&base, "A");
    let scaled_before = variable_index(&base, "Scaled");
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(4, || 0).unwrap();
    assert_eq!(host.read_variable(a_dint).unwrap(), 4);
    assert_eq!(host.read_variable(scaled_before).unwrap(), 20);

    // The names exchange places but each entity keeps its type: UID 1 (DINT)
    // is now `B`, UID 2 (REAL) is now `A`, UID 3 keeps `Scaled`.
    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    B : DINT;
    A : REAL;
    Scaled : DINT;
  END_VAR
  B := B + 1;
  A := A + 0.5;
  Scaled := REAL_TO_DINT(A * 10.0);
END_PROGRAM
",
        &[("B", 1), ("A", 2), ("Scaled", 3)],
    );
    let dint_entity = variable_index(&candidate, "B");
    let scaled = variable_index(&candidate, "Scaled");

    host.stage(candidate).unwrap();
    assert!(!host.status().migration);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    // The DINT entity continues 4 -> 5; the REAL entity continues
    // 2.0 -> 2.5 and Scaled reports it.
    assert_eq!(host.read_variable(dint_entity).unwrap(), 5);
    assert_eq!(host.read_variable(scaled).unwrap(), 25);
}

#[test]
fn test_when_declarations_reordered_then_values_follow_uids_to_new_indices() {
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    A : DINT;
    B : REAL;
    BView : DINT;
  END_VAR
  A := A + 1;
  B := B + 0.5;
  BView := REAL_TO_DINT(B * 10.0);
END_PROGRAM
",
        &[("A", 1), ("B", 2), ("BView", 3)],
    );
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(3, || 0).unwrap();

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    B : REAL;
    A : DINT;
    BView : DINT;
  END_VAR
  A := A + 1;
  B := B + 0.5;
  BView := REAL_TO_DINT(B * 10.0);
END_PROGRAM
",
        &[("A", 1), ("B", 2), ("BView", 3)],
    );
    let a = variable_index(&candidate, "A");
    let b_view = variable_index(&candidate, "BView");

    host.stage(candidate).unwrap();
    assert!(host.status().migration);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    // Both values followed their UIDs onto the swapped indices: the DINT
    // entity is 4, the REAL entity is 2.0, and BView reports it.
    assert_eq!(host.read_variable(a).unwrap(), 4);
    assert_eq!(host.read_variable(b_view).unwrap(), 20);
}

#[test]
fn run_when_variable_added_then_existing_value_survives_and_new_value_initializes() {
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let counter_before = variable_index(&base, "Counter");
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(5, || 0).unwrap();
    assert_eq!(host.read_variable(counter_before).unwrap(), 5);

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : DINT;
    Extra : DINT := 42;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let counter = variable_index(&candidate, "Counter");
    let extra = variable_index(&candidate, "Extra");

    host.stage(candidate).unwrap();
    assert!(host.status().migration);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    assert_eq!(host.read_variable(counter).unwrap(), 6);
    assert_eq!(host.read_variable(extra).unwrap(), 42);
}

#[test]
fn run_when_variable_removed_then_remaining_value_survives() {
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : DINT;
    Removed : DINT;
  END_VAR
  Counter := Counter + 1;
  Removed := Removed + 5;
END_PROGRAM
",
        &[("Counter", 1), ("Removed", 2)],
    );
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(4, || 0).unwrap();

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let counter = variable_index(&candidate, "Counter");

    host.stage(candidate).unwrap();
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    assert_eq!(host.read_variable(counter).unwrap(), 5);
}

#[test]
fn run_when_b_deleted_and_a_renamed_to_b_then_surviving_entity_continues() {
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    A : DINT;
    B : DINT;
  END_VAR
  A := A + 1;
  B := B + 100;
END_PROGRAM
",
        &[("A", 1), ("B", 2)],
    );
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(7, || 0).unwrap();

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    B : DINT;
  END_VAR
  B := B + 1;
END_PROGRAM
",
        // The surviving entity is A's UID renamed to `B`; UID 2 is gone.
        &[("B", 1)],
    );
    let b = variable_index(&candidate, "B");

    host.stage(candidate).unwrap();
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    // A's value (7) continues in B; the dropped UID 2's value (700) is gone.
    assert_eq!(host.read_variable(b).unwrap(), 8);
}

#[test]
fn run_when_string_added_variable_then_content_migrates_and_length_is_computed() {
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    Text : STRING[20] := 'HELLO';
    Length : DINT;
  END_VAR
  Text := 'ABCDEFGHIJ';
  Length := LEN(Text);
END_PROGRAM
",
        &[("Text", 1), ("Length", 2)],
    );
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(1, || 0).unwrap();

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    Text : STRING[20] := 'HELLO';
    Extra : DINT := 7;
    Length : DINT;
  END_VAR
  Length := LEN(Text);
END_PROGRAM
",
        &[("Text", 1), ("Length", 2)],
    );
    let length = variable_index(&candidate, "Length");
    let extra = variable_index(&candidate, "Extra");

    host.stage(candidate).unwrap();
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    // The 10-character running value, not the candidate's 5-character init
    // image, is what the candidate's LEN sees.
    assert_eq!(host.read_variable(length).unwrap(), 10);
    assert_eq!(host.read_variable(extra).unwrap(), 7);
}

#[test]
fn stage_when_variable_retyped_outside_policy_then_rejected_with_named_types() {
    // DINT -> STRING is outside the conversion policy (ADR 0060): the stage
    // rejects with the pair named, through the existing V4010 path, and the
    // running application is untouched.
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let counter = variable_index(&base, "Counter");
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(3, || 0).unwrap();

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : STRING[10];
  END_VAR
  Counter := 'AB';
END_PROGRAM
",
        &[("Counter", 1)],
    );

    let result = host.stage(candidate);

    assert!(matches!(
        result,
        Err(OnlineChangeError::MigrationUnsupported(
            MigrationError::TypeChangeUnsupported {
                uid: 1,
                from: FieldType::I32,
                to: FieldType::String,
            }
        ))
    ));
    let message = result.unwrap_err().to_string();
    assert!(message.contains("I32"));
    assert!(message.contains("STRING"));
    assert_eq!(host.status().candidate, None);
    assert_eq!(host.status().mode, HostMode::Normal);
    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 4);
}

#[test]
fn run_when_variable_changed_from_dint_to_real_then_value_converts_and_continues() {
    // ADR 0060: DINT -> REAL is an admitted pair. The running DINT value
    // converts at the swap, and the REAL entity continues from it.
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(4, || 0).unwrap();

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : REAL;
  END_VAR
  Counter := Counter + 1.0;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let counter = variable_index(&candidate, "Counter");

    host.stage(candidate).unwrap();
    assert!(host.status().migration);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    // The DINT value 4 converted to REAL at the swap, then the candidate's
    // body added one more: a REAL slot's low 32 bits are its f32 pattern.
    let bits = host.read_variable(counter).unwrap() as u32;
    assert_eq!(f32::from_bits(bits), 5.0);
}

#[test]
fn run_when_variable_widened_from_int_to_dint_then_value_continues_without_migration() {
    // INT and DINT share the container's I32 storage class, so the widening
    // changes no VarEntry: the candidate is not even a migration candidate,
    // and the ordinary online change carries the sign-extended slot value.
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : INT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(4, || 0).unwrap();

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let counter = variable_index(&candidate, "Counter");

    host.stage(candidate).unwrap();
    assert!(!host.status().migration);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    assert_eq!(host.read_variable(counter).unwrap(), 5);
}

#[test]
fn run_when_array_elements_widen_from_dint_to_lint_then_each_element_converts() {
    // ADR 0060: arrays migrate when the element pair is admitted and the
    // length matches; each element converts at the swap.
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    Vals : ARRAY[0..1] OF DINT;
  END_VAR
  Vals[0] := Vals[0] + 1;
  Vals[1] := Vals[1] + 2;
END_PROGRAM
",
        &[("Vals", 1)],
    );
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(1, || 0).unwrap();

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    Vals : ARRAY[0..1] OF LINT;
  END_VAR
  Vals[0] := Vals[0] + 1;
  Vals[1] := Vals[1] + 2;
END_PROGRAM
",
        &[("Vals", 1)],
    );
    let vals = variable_index(&candidate, "Vals");

    host.stage(candidate).unwrap();
    assert!(host.status().migration);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    // 1 -> 2 and 2 -> 4: both elements converted at the swap (DINT to LINT)
    // and kept accumulating under the candidate's body.
    let offset = host.read_variable(vals).unwrap() as usize;
    let region = host.data_region();
    let first = i64::from_le_bytes(region[offset..offset + 8].try_into().unwrap());
    let second = i64::from_le_bytes(region[offset + 8..offset + 16].try_into().unwrap());
    assert_eq!(first, 2);
    assert_eq!(second, 4);
}

#[test]
fn stage_when_array_length_changes_then_migration_unsupported_and_application_runs() {
    // ADR 0060: an aggregate whose element pair is admitted but whose length
    // changed cannot carry values element-wise; the stage rejects. `Peek`
    // reports `Vals[0]`: an array's own slot holds its region offset, not a
    // value.
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    Vals : ARRAY[0..1] OF DINT;
    Peek : DINT;
  END_VAR
  Vals[0] := Vals[0] + 1;
  Peek := Vals[0];
END_PROGRAM
",
        &[("Vals", 1), ("Peek", 2)],
    );
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(2, || 0).unwrap();

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    Vals : ARRAY[0..2] OF DINT;
    Peek : DINT;
  END_VAR
  Vals[0] := Vals[0] + 1;
  Peek := Vals[0];
END_PROGRAM
",
        &[("Vals", 1), ("Peek", 2)],
    );
    let peek = variable_index(&candidate, "Peek");

    let result = host.stage(candidate);

    assert!(matches!(
        result,
        Err(OnlineChangeError::MigrationUnsupported(
            MigrationError::ArrayDescriptorMismatch { uid: 1 }
        ))
    ));
    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(peek).unwrap(), 3);
}

#[test]
fn stage_when_string_max_length_shrinks_then_migration_unsupported() {
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    Text : STRING[80] := 'HELLO';
    Length : DINT;
  END_VAR
  Length := LEN(Text);
END_PROGRAM
",
        &[("Text", 1), ("Length", 2)],
    );
    let length = variable_index(&base, "Length");
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(1, || 0).unwrap();

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    Text : STRING[40] := 'HELLO';
    Length : DINT;
  END_VAR
  Length := LEN(Text);
END_PROGRAM
",
        &[("Text", 1), ("Length", 2)],
    );

    let result = host.stage(candidate);

    assert!(matches!(
        result,
        Err(OnlineChangeError::MigrationUnsupported(
            MigrationError::IncompatibleEntry { uid: 1, .. }
        ))
    ));
    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(length).unwrap(), 5);
}

#[test]
fn stage_when_fb_field_added_then_fb_layout_unsupported_and_application_runs() {
    let base = compile_with_ids(
        "FUNCTION_BLOCK Accumulator
  VAR_INPUT
    step : DINT;
  END_VAR
  VAR
    total : DINT;
  END_VAR
  total := total + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    acc : Accumulator;
  END_VAR
  acc(step := 1);
END_PROGRAM
",
        &[("acc", 1)],
    );
    let total = variable_index(&base, "total");
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(2, || 0).unwrap();

    let candidate = compile_with_ids(
        "FUNCTION_BLOCK Accumulator
  VAR_INPUT
    step : DINT;
  END_VAR
  VAR
    total : DINT;
    extra : DINT;
  END_VAR
  total := total + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    acc : Accumulator;
  END_VAR
  acc(step := 1);
END_PROGRAM
",
        &[("acc", 1)],
    );

    let result = host.stage(candidate);

    assert!(matches!(
        result,
        Err(OnlineChangeError::MigrationUnsupported(
            MigrationError::FbLayoutUnsupported
        ))
    ));
    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(total).unwrap(), 3);
}

#[test]
fn run_when_fb_field_inserted_in_middle_with_uids_then_existing_field_values_follow() {
    // ADR 0059: the field UIDs, not the field positions, decide which value
    // continues. `extra` is declared between `step` and `total`, so the
    // candidate's `total` sits at a new ordinal; its UID must carry the
    // running value there.
    let base = compile_with_uid_keys(
        "FUNCTION_BLOCK Accumulator
  VAR_INPUT
    step : DINT;
  END_VAR
  VAR
    total : DINT;
  END_VAR
  total := total + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    acc : Accumulator;
  END_VAR
  acc(step := 1);
END_PROGRAM
",
        &[
            ("main", "acc", 1),
            ("Accumulator", "step", 101),
            ("Accumulator", "total", 102),
        ],
    );
    let total = variable_index(&base, "total");
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(2, || 0).unwrap();
    assert_eq!(host.read_variable(total).unwrap(), 2);

    let candidate = compile_with_uid_keys(
        "FUNCTION_BLOCK Accumulator
  VAR_INPUT
    step : DINT;
  END_VAR
  VAR
    extra : DINT;
    total : DINT;
  END_VAR
  total := total + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    acc : Accumulator;
  END_VAR
  acc(step := 1);
END_PROGRAM
",
        // `extra` is a new entity with a fresh UID; the surviving fields
        // keep theirs.
        &[
            ("main", "acc", 1),
            ("Accumulator", "step", 101),
            ("Accumulator", "extra", 103),
            ("Accumulator", "total", 102),
        ],
    );
    let total = variable_index(&candidate, "total");
    let extra = variable_index(&candidate, "extra");

    host.stage(candidate).unwrap();
    assert!(host.status().migration);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    // total carried 2 across the ordinal shift and kept accumulating;
    // extra is a candidate-only entity, so the candidate's init image (a
    // zeroed field region) is what it starts from.
    assert_eq!(host.read_variable(total).unwrap(), 3);
    assert_eq!(host.read_variable(extra).unwrap(), 0);
}

#[test]
fn stage_when_fb_layout_changes_and_field_uid_missing_then_fail_closed() {
    // The candidate drops `total`'s UID while changing the layout: the
    // value's identity is unprovable, so the planner must reject rather
    // than guess (ADR 0059's fail-closed rule).
    let base = compile_with_uid_keys(
        "FUNCTION_BLOCK Accumulator
  VAR_INPUT
    step : DINT;
  END_VAR
  VAR
    total : DINT;
  END_VAR
  total := total + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    acc : Accumulator;
  END_VAR
  acc(step := 1);
END_PROGRAM
",
        &[
            ("main", "acc", 1),
            ("Accumulator", "step", 101),
            ("Accumulator", "total", 102),
        ],
    );
    let total = variable_index(&base, "total");
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(2, || 0).unwrap();

    let candidate = compile_with_uid_keys(
        "FUNCTION_BLOCK Accumulator
  VAR_INPUT
    step : DINT;
  END_VAR
  VAR
    extra : DINT;
    total : DINT;
  END_VAR
  total := total + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    acc : Accumulator;
  END_VAR
  acc(step := 1);
END_PROGRAM
",
        // `total` has no UID on the candidate side even though the layout
        // changed underneath it.
        &[
            ("main", "acc", 1),
            ("Accumulator", "step", 101),
            ("Accumulator", "extra", 103),
        ],
    );

    let result = host.stage(candidate);

    assert!(matches!(
        result,
        Err(OnlineChangeError::MigrationUnsupported(
            MigrationError::FbLayoutUnsupported
        ))
    ));
    // The rejection leaves the running application untouched.
    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(total).unwrap(), 3);
}

#[test]
fn untest_when_migration_test_then_untest_unsupported_and_application_keeps_running() {
    let base = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(5, || 0).unwrap();

    let candidate = compile_with_ids(
        "PROGRAM main
  VAR
    Counter : DINT;
    Extra : DINT := 42;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let counter = variable_index(&candidate, "Counter");

    host.stage(candidate).unwrap();
    host.test().unwrap();
    host.run(1, || 0).unwrap();
    assert_eq!(host.status().mode, HostMode::Testing);
    assert_eq!(host.read_variable(counter).unwrap(), 6);

    assert!(matches!(
        host.untest(),
        Err(OnlineChangeError::UntestUnsupported)
    ));

    // The refusal leaves the candidate active and the state untouched.
    host.run(1, || 0).unwrap();
    assert_eq!(host.status().mode, HostMode::Testing);
    assert!(host.status().candidate.is_some());
    assert_eq!(host.read_variable(counter).unwrap(), 7);

    // Assemble is the exit that commits the migration.
    host.assemble().unwrap();
    assert_eq!(host.status().mode, HostMode::Normal);
    assert!(!host.status().migration);
    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 8);
}
