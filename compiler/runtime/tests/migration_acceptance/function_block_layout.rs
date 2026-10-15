//! Acceptance tests for hot-edit migration when the field layout of a function block changes.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.

use crate::common::{compile_with_ids, compile_with_uid_keys, variable_index};
use ironplc_runtime::{MigrationError, OnlineChangeError, RuntimeHost};

#[test]
fn stage_when_fb_field_added_without_field_uids_then_refused_naming_the_fields_and_application_runs(
) {
    // The fields of `Accumulator` carry no UID on either side: the identity rule
    // (ADR-0074) refuses the candidate before the planner reads the layout.
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
    host.permit_execution();
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

    let error = result.unwrap_err();
    assert!(matches!(
        error,
        OnlineChangeError::MigrationUnsupported(MigrationError::UnidentifiedVariables { .. })
    ));
    let text = error.to_string();
    assert_eq!(
        text,
        "candidate state cannot be migrated: persistent variables without a stable ID: \
         active application: ACCUMULATOR.step, ACCUMULATOR.total; \
         candidate: ACCUMULATOR.step, ACCUMULATOR.total, ACCUMULATOR.extra"
    );
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
    host.permit_execution();
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
fn stage_when_fb_layout_changes_and_field_uid_missing_then_refused_naming_the_field() {
    // The candidate drops `total`'s UID while changing the layout: the
    // value's identity is unprovable, so the candidate is refused and the
    // field named rather than guessed (ADR 0059's fail-closed rule, which is the
    // identity rule of ADR-0074 for the fields of a function block).
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
    host.permit_execution();
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

    let error = result.unwrap_err();
    assert!(matches!(
        error,
        OnlineChangeError::MigrationUnsupported(MigrationError::UnidentifiedVariables { .. })
    ));
    assert!(error.to_string().ends_with("candidate: ACCUMULATOR.total"));
    // The rejection leaves the running application untouched.
    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(total).unwrap(), 3);
}
