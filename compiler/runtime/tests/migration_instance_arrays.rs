//! Acceptance tests for hot-edit state migration of arrays of function block
//! instances.
//!
//! An array of instances is migrated as one region of slots, which keeps every
//! instance's state when the block is laid out alike on both sides and is
//! refused otherwise: a value is never carried into a slot that no longer means
//! the same field.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

mod common;

use common::{compile_codesys_with_ids, variable_index};
use ironplc_runtime::{MigrationError, OnlineChangeError, RuntimeHost};

fn program(total_type: &str, extra: &str) -> String {
    format!(
        "FUNCTION_BLOCK Accumulator
  VAR_INPUT
    step : {total_type};
  END_VAR
  VAR_OUTPUT
    total : {total_type};
  END_VAR
  total := total + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    accs : ARRAY[0..1] OF Accumulator;
    seen : {total_type};
{extra}  END_VAR
  accs[0](step := 1);
  accs[1](step := 10);
  seen := accs[1].total;
END_PROGRAM
"
    )
}

const IDS: &[(&str, u64)] = &[("accs", 1), ("seen", 2)];

#[test]
fn run_when_unrelated_variable_added_then_every_instance_keeps_its_state() {
    let base = compile_codesys_with_ids(&program("DINT", ""), IDS);
    let mut host = RuntimeHost::new(base.clone()).unwrap();
    host.permit_execution();
    host.run(3, || 0).unwrap();
    assert_eq!(
        host.read_variable(variable_index(&base, "seen")).unwrap(),
        30
    );

    let candidate = compile_codesys_with_ids(&program("DINT", "    added : DINT;\n"), IDS);
    let seen = variable_index(&candidate, "seen");

    host.stage(candidate).unwrap();
    assert!(host.status().migration);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    // The second instance had accumulated 30 and kept going.
    assert_eq!(host.read_variable(seen).unwrap(), 40);
}

#[test]
fn stage_when_block_field_changes_type_at_same_size_then_fb_layout_unsupported_and_application_runs(
) {
    let base = compile_codesys_with_ids(&program("DINT", ""), IDS);
    let seen = variable_index(&base, "seen");
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();
    host.run(2, || 0).unwrap();

    // DINT to LINT keeps the number of slots, so only the field entries say
    // that the instances' slots no longer hold the same values.
    let candidate = compile_codesys_with_ids(&program("LINT", ""), IDS);

    let result = host.stage(candidate);

    assert!(matches!(
        result,
        Err(OnlineChangeError::MigrationUnsupported(
            MigrationError::FbLayoutUnsupported
        ))
    ));
    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(seen).unwrap(), 30);
}
