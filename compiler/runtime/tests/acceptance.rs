//! Acceptance tests for the hot-edit P0 online change host.
//!
//! The scenarios mirror the POC acceptance test
//! (`docs/reference/Rnd_Rockwell/ironplc_hot_edit_poc_handoff.md` section 19)
//! and the four P0 tests of the baseline: code-body edits, FB instance
//! state, declaration-edit rejection, and untest-without-state-rollback.
//!
//! Fixtures are compiled with the real project pipeline and round-tripped
//! through the container wire format, so `header.layout_hash` is the value a
//! deployed artifact carries.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

mod common;

use std::collections::BTreeMap;

use common::{compile_source, container_bytes, counter_host, counter_program, variable_index};
use ironplc_container::VarIndex;
use ironplc_runtime::{AcceptedEdit, HostMode, OnlineChangeError, RuntimeError, RuntimeHost};

#[test]
fn run_when_no_execution_permit_then_refused_with_v4018_and_no_rounds_execute() {
    // A fresh host boots unpermitted (the HA redundancy architecture,
    // "Minimal Seams" 1): scans refuse until the composition root grants.
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let mut host = RuntimeHost::new(base).unwrap();

    let error = host.run(1, || 0).unwrap_err();

    assert!(matches!(error, RuntimeError::NotPermitted));
    assert_eq!(error.v_code(), Some("V4018"));
    assert_eq!(host.status().rounds, 0);
}

#[test]
fn run_when_permit_granted_then_rounds_execute() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let counter = variable_index(&base, "Counter");
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();

    host.run(3, || 0).unwrap();

    assert_eq!(host.status().rounds, 3);
    assert_eq!(host.read_variable(counter).unwrap(), 3);
}

#[test]
fn run_when_permit_revoked_before_boundary_then_pending_swap_cancelled_terminally() {
    // External FSM review, scenario T07: a permit revoked between the
    // request and the boundary cancels the operation with a terminal
    // result — the pending swap is consumed and never applied.
    let (mut host, counter) = counter_host(1);
    host.stage(compile_source(&counter_program("Counter := Counter + 10;")))
        .unwrap();
    host.test().unwrap();
    host.revoke_execution_permit();

    let error = host.run(1, || 0).unwrap_err();

    assert!(matches!(error, RuntimeError::NotPermitted));
    assert_eq!(error.v_code(), Some("V4018"));
    // Terminal: the pending swap is gone, the candidate stays staged, and
    // no round ran.
    let status = host.status();
    assert_eq!(status.mode, HostMode::Normal);
    assert!(status.candidate.is_some());
    assert_eq!(status.rounds, 0);

    // A re-grant drives scans but does not resurrect the cancelled swap.
    host.permit_execution();
    host.run(1, || 0).unwrap();
    assert_eq!(host.status().mode, HostMode::Normal);
    assert_eq!(host.read_variable(counter).unwrap(), 1);
}

#[test]
fn run_when_logic_only_edit_then_counter_continues_without_reset() {
    let (mut host, counter) = counter_host(1);
    host.run(12_537, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 12_537);

    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    host.stage(candidate).unwrap();
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    assert_eq!(host.read_variable(counter).unwrap(), 12_547);
}

#[test]
fn run_when_test_then_scan_count_continues_and_init_is_not_rerun() {
    let (mut host, counter) = counter_host(1);
    host.run(12_537, || 0).unwrap();
    assert_eq!(host.status().rounds, 12_537);
    assert_eq!(host.status().mode, HostMode::Normal);
    assert_eq!(host.status().active.raw(), 1);

    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    host.stage(candidate).unwrap();
    assert_eq!(host.status().candidate.unwrap().raw(), 2);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    let status = host.status();
    assert_eq!(status.rounds, 12_538);
    assert_eq!(status.mode, HostMode::Testing);
    assert_eq!(status.active.raw(), 2);
    assert_eq!(status.normal.raw(), 1);
    assert_eq!(host.read_variable(counter).unwrap(), 12_547);
}

#[test]
fn run_when_fb_body_edit_then_instance_state_continues() {
    let base = compile_source(
        "FUNCTION_BLOCK StepCounter
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
    fb : StepCounter;
    result : DINT;
  END_VAR
  fb(step := 1);
  result := fb.total;
END_PROGRAM
",
    );
    let total = variable_index(&base, "total");
    let result = variable_index(&base, "result");
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();

    host.run(5, || 0).unwrap();
    assert_eq!(host.read_variable(total).unwrap(), 5);
    assert_eq!(host.read_variable(result).unwrap(), 5);

    let edited = compile_source(
        "FUNCTION_BLOCK StepCounter
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
    fb : StepCounter;
    result : DINT;
  END_VAR
  fb(step := 10);
  result := fb.total;
END_PROGRAM
",
    );
    host.stage(edited).unwrap();
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    assert_eq!(host.read_variable(total).unwrap(), 15);
    assert_eq!(host.read_variable(result).unwrap(), 15);
}

#[test]
fn stage_when_declaration_changes_then_layout_incompatible_and_running_application_unaffected() {
    let (mut host, counter) = counter_host(1);
    host.run(10, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 10);

    let changed = compile_source(
        "PROGRAM main
  VAR
    Counter : DINT;
    Extra : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
    );
    let result = host.stage(changed);

    assert!(matches!(result, Err(OnlineChangeError::LayoutIncompatible)));
    let status = host.status();
    assert_eq!(status.candidate, None);
    assert_eq!(status.mode, HostMode::Normal);

    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 11);
}

#[test]
fn run_when_untest_then_code_reverts_but_state_stays_current() {
    let (mut host, counter) = counter_host(1);
    host.run(100, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 100);

    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    host.stage(candidate).unwrap();
    host.test().unwrap();
    host.run(3, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 130);
    assert_eq!(host.status().mode, HostMode::Testing);

    host.untest().unwrap();
    host.run(1, || 0).unwrap();

    assert_eq!(host.read_variable(counter).unwrap(), 131);
    assert_eq!(host.status().mode, HostMode::Normal);
}

#[test]
fn run_when_candidate_grows_data_region_then_string_survives_and_length_updates() {
    let base = compile_source(
        "PROGRAM main
  VAR
    Text : STRING[80] := 'HELLO';
    Length : DINT;
  END_VAR
  Length := LEN(Text);
END_PROGRAM
",
    );
    let length = variable_index(&base, "Length");
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();
    host.run(3, || 0).unwrap();
    assert_eq!(host.read_variable(length).unwrap(), 5);
    let region_bytes_before = host.data_region().len();

    let edited = compile_source(
        "PROGRAM main
  VAR
    Text : STRING[80] := 'HELLO';
    Length : DINT;
  END_VAR
  Length := LEN(Text) + LEN(CONCAT(Text, 'X'));
END_PROGRAM
",
    );
    host.stage(edited).unwrap();
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    assert_eq!(host.read_variable(length).unwrap(), 11);
    assert!(host.data_region().len() > region_bytes_before);
}

#[test]
fn assemble_when_candidate_accepted_then_assemble_without_test_error() {
    let (mut host, counter) = counter_host(1);
    host.run(5, || 0).unwrap();

    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    host.stage(candidate).unwrap();

    // ADR-0064: a candidate that never ran under Test cannot be promoted.
    assert!(matches!(
        host.assemble(),
        Err(OnlineChangeError::AssembleWithoutTest)
    ));

    // The refusal leaves the original running and the candidate staged.
    let status = host.status();
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.normal.raw(), 1);
    assert!(status.candidate.is_some());

    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 6);
}

#[test]
fn assemble_when_testing_then_candidate_becomes_normal() {
    let (mut host, counter) = counter_host(1);
    host.run(5, || 0).unwrap();

    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    host.stage(candidate).unwrap();
    host.test().unwrap();
    host.run(1, || 0).unwrap();
    assert_eq!(host.status().mode, HostMode::Testing);

    host.assemble().unwrap();
    assert_eq!(host.status().mode, HostMode::Normal);
    assert_eq!(host.status().candidate, None);
    assert_eq!(host.status().normal.raw(), 2);

    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 25);
}

#[test]
fn stage_with_decisions_when_edit_then_record_and_wire_follow_the_candidate() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let base_hash = base.header.content_hash;
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();

    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    let wire = container_bytes(&candidate);
    host.stage_with_decisions(
        candidate,
        &BTreeMap::new(),
        Some(AcceptedEdit {
            wire: wire.clone(),
            name: Some("edit-1".into()),
            origin: Some("bench".into()),
        }),
    )
    .unwrap();

    let record = host.status().pending_edit.unwrap();
    assert_eq!(record.name.as_deref(), Some("edit-1"));
    assert_eq!(record.origin.as_deref(), Some("bench"));
    assert!(record.accepted_at > 0);
    assert_eq!(record.baseline.normal_generation, 1);
    assert_eq!(record.baseline.content_hash, base_hash);

    // Untest keeps the candidate, the record, and the retained wire bytes.
    host.test().unwrap();
    host.run(1, || 0).unwrap();
    host.untest().unwrap();
    host.run(1, || 0).unwrap();
    assert!(host.status().pending_edit.is_some());

    // Assemble clears the record and moves the exact wire bytes to the
    // committed latch, drained once by the shell-side commit.
    host.test().unwrap();
    host.run(1, || 0).unwrap();
    host.assemble().unwrap();
    assert!(host.status().pending_edit.is_none());
    assert_eq!(host.take_committed_wire().unwrap(), wire);
    assert!(host.take_committed_wire().is_none());
}

#[test]
fn cancel_when_candidate_staged_then_record_and_wire_dropped() {
    let (mut host, _counter) = counter_host(1);
    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    let wire = container_bytes(&candidate);
    host.stage_with_decisions(
        candidate,
        &BTreeMap::new(),
        Some(AcceptedEdit {
            wire,
            name: None,
            origin: None,
        }),
    )
    .unwrap();
    assert!(host.status().pending_edit.is_some());

    host.cancel().unwrap();

    assert!(host.status().pending_edit.is_none());
    // Nothing committed: the latch stays empty.
    assert!(host.take_committed_wire().is_none());
}

#[test]
fn stage_without_edit_then_unnamed_record_written_and_no_wire_retained() {
    let (mut host, _counter) = counter_host(1);
    host.stage(compile_source(&counter_program("Counter := Counter + 10;")))
        .unwrap();

    // The record is written unnamed; without wire bytes nothing can commit.
    let record = host.status().pending_edit.unwrap();
    assert!(record.name.is_none());
    assert!(record.origin.is_none());
    assert!(record.accepted_at > 0);

    host.test().unwrap();
    host.run(1, || 0).unwrap();
    host.assemble().unwrap();
    assert!(host.take_committed_wire().is_none());
}

#[test]
fn cancel_when_candidate_accepted_then_candidate_discarded_and_original_runs() {
    let (mut host, counter) = counter_host(1);
    host.run(5, || 0).unwrap();

    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    host.stage(candidate).unwrap();
    host.cancel().unwrap();

    let status = host.status();
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.candidate, None);
    assert_eq!(status.normal.raw(), 1);

    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 6);
}

#[test]
fn stage_when_candidate_already_staged_then_candidate_already_staged_error() {
    let (mut host, _counter) = counter_host(1);
    host.stage(compile_source(&counter_program("Counter := Counter + 10;")))
        .unwrap();

    let result = host.stage(compile_source(&counter_program("Counter := Counter + 20;")));

    assert!(matches!(
        result,
        Err(OnlineChangeError::CandidateAlreadyStaged)
    ));
}

#[test]
fn test_when_no_candidate_staged_then_no_candidate_staged_error() {
    let (mut host, _counter) = counter_host(1);

    assert!(matches!(
        host.test(),
        Err(OnlineChangeError::NoCandidateStaged)
    ));
}

#[test]
fn untest_when_candidate_not_active_then_no_test_in_progress_error() {
    let (mut host, _counter) = counter_host(1);
    host.stage(compile_source(&counter_program("Counter := Counter + 10;")))
        .unwrap();

    assert!(matches!(
        host.untest(),
        Err(OnlineChangeError::NoTestInProgress)
    ));
}

#[test]
fn cancel_when_testing_then_not_allowed_in_this_mode_error() {
    let (mut host, _counter) = counter_host(1);
    host.stage(compile_source(&counter_program("Counter := Counter + 10;")))
        .unwrap();
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    assert!(matches!(
        host.cancel(),
        Err(OnlineChangeError::NotAllowedInThisMode)
    ));
}

#[test]
fn stage_when_io_image_sizes_change_then_io_incompatible_error() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let mut candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    candidate.header.input_image_bytes += 1;
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();

    assert!(matches!(
        host.stage(candidate),
        Err(OnlineChangeError::IoIncompatible)
    ));
}

#[test]
fn stage_when_task_table_changes_then_schedule_incompatible_error() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let mut candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    candidate.task_table.tasks[0].priority += 1;
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();

    assert!(matches!(
        host.stage(candidate),
        Err(OnlineChangeError::ScheduleIncompatible)
    ));
}

#[test]
fn stage_when_header_flags_change_then_layout_incompatible_error() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let mut candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    candidate.header.flags |= 0x80;
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();

    assert!(matches!(
        host.stage(candidate),
        Err(OnlineChangeError::LayoutIncompatible)
    ));
}

#[test]
fn status_when_fresh_host_then_normal_generation_one_and_zero_rounds() {
    let (host, _counter) = counter_host(1);

    let status = host.status();
    assert_eq!(status.active, status.normal);
    assert_eq!(status.normal.raw(), 1);
    assert_eq!(status.candidate, None);
    assert_eq!(status.application.raw(), 1);
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.rounds, 0);
}

#[test]
fn run_when_clock_advances_then_rounds_and_state_advance() {
    let (mut host, counter) = counter_host(1);
    let mut now = 0u64;

    host.run(4, || {
        now += 1_000;
        now
    })
    .unwrap();

    assert_eq!(host.status().rounds, 4);
    assert_eq!(host.read_variable(counter).unwrap(), 4);
}

#[test]
fn read_variable_when_index_out_of_range_then_internal_error() {
    let (host, _counter) = counter_host(1);

    assert!(host.read_variable(VarIndex::new(u16::MAX)).is_err());
}
