//! Acceptance tests for the hot-edit command layer.
//!
//! The scenarios mirror the runtime acceptance matrix (P0 online change)
//! over the line-delimited protocol: accept / test / untest / assemble /
//! cancel, untest blocked after a schema edit, and the status payload
//! contents. Every command enters as a JSON line and every response is
//! checked as rendered JSON, so the wire format is under test too.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

mod common;

use common::{compile_with_ids, container_bytes, counter_host, counter_program};
use ironplc_runtime::{
    execute, parse_command, Command, HostMode, Response, RuntimeHost, StatusPayload,
};

/// Runs one command line through the codec and the command layer, returning
/// the rendered response as a JSON value.
fn run_line(host: &mut RuntimeHost, line: &str) -> serde_json::Value {
    let command = parse_command(line).unwrap();
    let rendered = ironplc_runtime::render_response(&execute(command, host)).unwrap();
    serde_json::from_str(&rendered).unwrap()
}

/// The `AcceptEdits` line for a compiled source snippet without stable
/// variable IDs.
fn accept_line(source: &str) -> String {
    let bytes = container_bytes(&common::compile_source(source));
    serde_json::json!({"command": "acceptEdits", "program": bytes}).to_string()
}

/// The `AcceptEdits` line for a compiled source snippet with stable variable
/// IDs.
fn accept_line_with_ids(source: &str, ids: &[(&str, u64)]) -> String {
    let bytes = container_bytes(&compile_with_ids(source, ids));
    serde_json::json!({"command": "acceptEdits", "program": bytes}).to_string()
}

#[test]
fn accept_edits_when_compiled_container_then_ack_and_status_lists_candidate() {
    let (mut host, _counter) = counter_host(1);
    host.run(5, || 0).unwrap();

    let response = run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 10;")),
    );
    assert_eq!(response["response"], "ack");

    let response = run_line(&mut host, r#"{"command":"getStatus"}"#);
    assert_eq!(
        response,
        serde_json::json!({
            "response": "status",
            "mode": "normal",
            "active": 1,
            "normal": 1,
            "candidate": 2,
            "application": 1,
            "migration": false,
            "rounds": 5,
        })
    );
}

#[test]
fn test_edits_when_candidate_staged_then_ack_and_candidate_executes() {
    let (mut host, counter) = counter_host(1);
    host.run(5, || 0).unwrap();
    run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 10;")),
    );

    let response = run_line(&mut host, r#"{"command":"testEdits"}"#);
    assert_eq!(response["response"], "ack");

    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 15);
}

#[test]
fn untest_edits_when_logic_only_test_then_ack_and_code_reverts_state_stays() {
    let (mut host, counter) = counter_host(1);
    host.run(100, || 0).unwrap();
    run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 10;")),
    );
    run_line(&mut host, r#"{"command":"testEdits"}"#);
    host.run(3, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 130);

    let response = run_line(&mut host, r#"{"command":"untestEdits"}"#);
    assert_eq!(response["response"], "ack");

    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 131);
    assert!(host.status().candidate.is_some());
}

#[test]
fn assemble_edits_when_candidate_staged_then_candidate_becomes_normal() {
    let (mut host, counter) = counter_host(1);
    host.run(5, || 0).unwrap();
    run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 10;")),
    );

    let response = run_line(&mut host, r#"{"command":"assembleEdits"}"#);
    assert_eq!(response["response"], "ack");

    let response = run_line(&mut host, r#"{"command":"getStatus"}"#);
    assert_eq!(response["candidate"], serde_json::Value::Null);
    assert_eq!(response["normal"], 2);
    assert_eq!(response["application"], 2);

    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 15);
}

#[test]
fn cancel_edits_when_candidate_staged_then_candidate_discarded_and_original_runs() {
    let (mut host, counter) = counter_host(1);
    host.run(5, || 0).unwrap();
    run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 10;")),
    );

    let response = run_line(&mut host, r#"{"command":"cancelEdits"}"#);
    assert_eq!(response["response"], "ack");

    let response = run_line(&mut host, r#"{"command":"getStatus"}"#);
    assert_eq!(response["candidate"], serde_json::Value::Null);
    assert_eq!(response["normal"], 1);

    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 6);
}

#[test]
fn untest_edits_when_schema_edit_tested_then_v4011_and_candidate_keeps_running() {
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

    // Adding a variable is a declaration edit: the candidate carries a state
    // migration plan, so untest has no reverse mapping (ADR-0054).
    let accept = accept_line_with_ids(
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
    let response = run_line(&mut host, &accept);
    assert_eq!(response["response"], "ack");
    assert!(host.status().migration);

    let response = run_line(&mut host, r#"{"command":"testEdits"}"#);
    assert_eq!(response["response"], "ack");
    host.run(1, || 0).unwrap();

    let response = run_line(&mut host, r#"{"command":"untestEdits"}"#);
    assert_eq!(response["response"], "error");
    assert_eq!(response["vCode"], "V4011");

    // The refusal leaves the candidate active and the state untouched.
    host.run(1, || 0).unwrap();
    assert!(host.status().candidate.is_some());
}

#[test]
fn get_status_when_fresh_host_then_normal_generation_one_and_no_candidate() {
    let (mut host, _counter) = counter_host(1);

    let response = execute(Command::GetStatus, &mut host);

    assert_eq!(
        response,
        Response::Status(StatusPayload {
            mode: HostMode::Normal,
            active: 1,
            normal: 1,
            candidate: None,
            application: 1,
            migration: false,
            rounds: 0,
        })
    );
}

#[test]
fn status_payload_when_test_applied_then_mode_testing_and_generations_move() {
    let (mut host, _counter) = counter_host(1);
    run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 10;")),
    );
    run_line(&mut host, r#"{"command":"testEdits"}"#);
    host.run(3, || 0).unwrap();

    let response = execute(Command::GetStatus, &mut host);

    assert_eq!(
        response,
        Response::Status(StatusPayload {
            mode: HostMode::Testing,
            active: 2,
            normal: 1,
            candidate: Some(2),
            application: 1,
            migration: false,
            rounds: 3,
        })
    );
}

#[test]
fn accept_edits_when_payload_is_not_a_container_then_v4016() {
    let (mut host, _counter) = counter_host(1);

    let response = run_line(&mut host, r#"{"command":"acceptEdits","program":[1,2,3]}"#);

    assert_eq!(response["response"], "error");
    assert_eq!(response["vCode"], "V4016");
    assert!(host.status().candidate.is_none());
}

#[test]
fn accept_edits_when_candidate_already_staged_then_v4013() {
    let (mut host, _counter) = counter_host(1);
    run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 10;")),
    );

    let response = run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 20;")),
    );

    assert_eq!(response["response"], "error");
    assert_eq!(response["vCode"], "V4013");
}

#[test]
fn test_edits_when_no_candidate_staged_then_v4012() {
    let (mut host, _counter) = counter_host(1);

    let response = run_line(&mut host, r#"{"command":"testEdits"}"#);

    assert_eq!(response["response"], "error");
    assert_eq!(response["vCode"], "V4012");
}

#[test]
fn untest_edits_when_no_test_in_progress_then_v4014() {
    let (mut host, _counter) = counter_host(1);
    run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 10;")),
    );

    let response = run_line(&mut host, r#"{"command":"untestEdits"}"#);

    assert_eq!(response["response"], "error");
    assert_eq!(response["vCode"], "V4014");
}

#[test]
fn cancel_edits_while_testing_then_v4015() {
    let (mut host, _counter) = counter_host(1);
    run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 10;")),
    );
    run_line(&mut host, r#"{"command":"testEdits"}"#);
    host.run(1, || 0).unwrap();

    let response = run_line(&mut host, r#"{"command":"cancelEdits"}"#);

    assert_eq!(response["response"], "error");
    assert_eq!(response["vCode"], "V4015");
}
