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

use common::{compile_source, counter_host, counter_program, variable_index};
use ironplc_container::VarIndex;
use ironplc_runtime::{HostMode, OnlineChangeError, RuntimeHost};

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
fn assemble_when_candidate_accepted_then_candidate_becomes_normal() {
    let (mut host, counter) = counter_host(1);
    host.run(5, || 0).unwrap();

    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    host.stage(candidate).unwrap();
    host.assemble().unwrap();

    let status = host.status();
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.candidate, None);
    assert_eq!(status.normal.raw(), 2);
    assert_eq!(status.active.raw(), 2);
    assert_eq!(status.application.raw(), 2);

    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 15);
    assert!(matches!(
        host.assemble(),
        Err(OnlineChangeError::NoCandidateStaged)
    ));
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
