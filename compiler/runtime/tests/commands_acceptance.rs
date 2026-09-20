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

use common::{compile_with_ids, container_bytes, counter_host, counter_program, variable_index};
use ironplc_runtime::{
    execute, parse_command, Command, DeviceIdentity, HostMode, IdentityPayload, Response,
    RuntimeHost, StatusPayload, SESSION_PROTOCOL_VERSION,
};

/// The device block composed for the acceptance tests' soft device.
fn test_device() -> DeviceIdentity {
    DeviceIdentity {
        name: "ironplcvm".into(),
        model: "IronPLC SoftPLC".into(),
        modification: "vm-cli".into(),
        firmware_version: "0.13.0".into(),
    }
}

/// Runs one command line through the codec and the command layer, returning
/// the rendered response as a JSON value.
fn run_line(host: &mut RuntimeHost, line: &str) -> serde_json::Value {
    let command = parse_command(line).unwrap();
    let rendered =
        ironplc_runtime::render_response(&execute(command, host, &test_device())).unwrap();
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

/// The `AcceptEdits` line for a compiled source snippet with stable variable
/// IDs and a per-UID migration decision map (ADR 0061).
fn accept_line_with_decisions(
    source: &str,
    ids: &[(&str, u64)],
    migration: serde_json::Value,
) -> String {
    let bytes = container_bytes(&compile_with_ids(source, ids));
    serde_json::json!({"command": "acceptEdits", "program": bytes, "migration": migration})
        .to_string()
}

/// A `PROGRAM main` whose single `Counter` is retyped and incremented by one.
fn retyped_counter_program(ty: &str) -> String {
    format!(
        "PROGRAM main
  VAR
    Counter : {ty};
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
"
    )
}

/// A host whose `Counter : DINT` (UID 1) has counted up to 3.
fn retype_base_host() -> (RuntimeHost, ironplc_container::VarIndex) {
    let base = compile_with_ids(
        &counter_program("Counter := Counter + 1;"),
        &[("Counter", 1)],
    );
    let counter = variable_index(&base, "Counter");
    let mut host = RuntimeHost::new(base).unwrap();
    host.run(3, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 3);
    (host, counter)
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

    // The staged candidate carries the pending-edit record (ADR-0064): the
    // accept named no edit, so the block is present but unnamed; its
    // acceptedAt timestamp is device-side and asserted only for presence.
    let response = run_line(&mut host, r#"{"command":"getStatus"}"#);
    assert_eq!(response["response"], "status");
    assert_eq!(response["mode"], "normal");
    assert_eq!(response["active"], 1);
    assert_eq!(response["normal"], 1);
    assert_eq!(response["candidate"], 2);
    assert_eq!(response["application"], 1);
    assert_eq!(response["migration"], false);
    assert_eq!(response["rounds"], 5);
    let pending = &response["pendingEdit"];
    assert_eq!(pending["name"], serde_json::Value::Null);
    assert_eq!(pending["origin"], serde_json::Value::Null);
    assert!(pending["acceptedAt"].as_u64().unwrap() > 0);
    assert_eq!(pending["baseline"]["normalGeneration"], 1);
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
fn assemble_edits_when_candidate_accepted_then_v4017_and_candidate_kept() {
    let (mut host, counter) = counter_host(1);
    host.run(5, || 0).unwrap();
    run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 10;")),
    );

    // ADR-0064: assemble straight from Accepted refuses; the wire carries
    // the stable V-code and the candidate stays staged.
    let response = run_line(&mut host, r#"{"command":"assembleEdits"}"#);
    assert_eq!(response["response"], "error");
    assert_eq!(response["vCode"], "V4017");
    assert!(host.status().candidate.is_some());

    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 6);
}

#[test]
fn assemble_edits_when_candidate_staged_then_candidate_becomes_normal() {
    let (mut host, counter) = counter_host(1);
    host.run(5, || 0).unwrap();
    run_line(
        &mut host,
        &accept_line(&counter_program("Counter := Counter + 10;")),
    );

    // ADR-0064: the candidate must execute under Test before it assembles.
    run_line(&mut host, r#"{"command":"testEdits"}"#);
    host.run(1, || 0).unwrap();
    let response = run_line(&mut host, r#"{"command":"assembleEdits"}"#);
    assert_eq!(response["response"], "ack");

    let response = run_line(&mut host, r#"{"command":"getStatus"}"#);
    assert_eq!(response["candidate"], serde_json::Value::Null);
    assert_eq!(response["normal"], 2);
    assert_eq!(response["application"], 2);

    // The test round ran the candidate once (5 -> 15); the round after the
    // assemble runs the promoted application once more (15 -> 25).
    host.run(1, || 0).unwrap();
    assert_eq!(host.read_variable(counter).unwrap(), 25);
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
fn accept_edits_when_type_change_without_decision_then_v4010_and_pairs() {
    let (mut host, _counter) = retype_base_host();

    let response = run_line(
        &mut host,
        &accept_line_with_ids(&retyped_counter_program("UDINT"), &[("Counter", 1)]),
    );

    assert_eq!(response["response"], "error");
    assert_eq!(response["vCode"], "V4010");
    assert_eq!(
        response["pairs"],
        serde_json::json!([{
            "uid": 1,
            "name": "Counter",
            "from": "I32",
            "to": "U32",
            "sizeEqual": true,
        }])
    );
    // Nothing was staged: the engineer can resubmit the same edit with a
    // decision map.
    assert!(host.status().candidate.is_none());
    assert_eq!(host.status().mode, HostMode::Normal);
}

#[test]
fn accept_edits_when_preserve_decision_then_old_bits_run_under_the_new_type() {
    let (mut host, _counter) = retype_base_host();
    let candidate = compile_with_ids(&retyped_counter_program("UDINT"), &[("Counter", 1)]);
    let counter = variable_index(&candidate, "Counter");

    let response = run_line(
        &mut host,
        &accept_line_with_decisions(
            &retyped_counter_program("UDINT"),
            &[("Counter", 1)],
            serde_json::json!({"1": "preserve"}),
        ),
    );
    assert_eq!(response["response"], "ack");
    assert!(host.status().migration);

    host.test().unwrap();
    host.run(1, || 0).unwrap();

    // The DINT 3 (0x3) was preserved bit for bit; the UDINT body added one.
    assert_eq!(host.read_variable(counter).unwrap(), 4);
}

#[test]
fn accept_edits_when_init_decision_then_candidate_initial_value_stands() {
    let (mut host, _counter) = retype_base_host();
    let source = "PROGRAM main
  VAR
    Counter : UDINT := 100;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
";
    let candidate = compile_with_ids(source, &[("Counter", 1)]);
    let counter = variable_index(&candidate, "Counter");

    let response = run_line(
        &mut host,
        &accept_line_with_decisions(source, &[("Counter", 1)], serde_json::json!({"1": "init"})),
    );
    assert_eq!(response["response"], "ack");

    host.test().unwrap();
    host.run(1, || 0).unwrap();

    // The old 3 was discarded; the candidate's declared 100 initialized the
    // variable before the candidate body ran.
    assert_eq!(host.read_variable(counter).unwrap(), 101);
}

#[test]
fn accept_edits_when_decision_uid_is_unknown_then_v4010_without_pairs() {
    let (mut host, _counter) = retype_base_host();

    let response = run_line(
        &mut host,
        &accept_line_with_decisions(
            &retyped_counter_program("UDINT"),
            &[("Counter", 1)],
            serde_json::json!({"99": "init"}),
        ),
    );

    assert_eq!(response["response"], "error");
    assert_eq!(response["vCode"], "V4010");
    assert_eq!(response["pairs"], serde_json::Value::Null);
    assert!(host.status().candidate.is_none());
}

#[test]
fn accept_edits_with_edit_then_status_carries_pending_edit_until_assemble() {
    let (mut host, _counter) = counter_host(1);
    host.run(2, || 0).unwrap();
    let bytes = container_bytes(&common::compile_source(&counter_program(
        "Counter := Counter + 10;",
    )));
    let accept = serde_json::json!({
        "command": "acceptEdits",
        "program": bytes,
        "edit": {"name": "hotfix", "origin": "bench-2"},
    })
    .to_string();

    let response = run_line(&mut host, &accept);
    assert_eq!(response["response"], "ack");

    let response = run_line(&mut host, r#"{"command":"getStatus"}"#);
    let pending = &response["pendingEdit"];
    assert_eq!(pending["name"], "hotfix");
    assert_eq!(pending["origin"], "bench-2");
    assert!(pending["acceptedAt"].as_u64().unwrap() > 0);
    assert_eq!(pending["baseline"]["normalGeneration"], 1);
    assert_eq!(
        pending["baseline"]["contentHash"].as_array().unwrap().len(),
        32
    );

    run_line(&mut host, r#"{"command":"testEdits"}"#);
    host.run(1, || 0).unwrap();
    let response = run_line(&mut host, r#"{"command":"assembleEdits"}"#);
    assert_eq!(response["response"], "ack");

    // Assemble cleared the record with the candidate: the block is absent.
    let response = run_line(&mut host, r#"{"command":"getStatus"}"#);
    assert_eq!(response["pendingEdit"], serde_json::Value::Null);
    assert_eq!(response["candidate"], serde_json::Value::Null);
}

#[test]
fn cancel_edits_when_candidate_staged_then_pending_edit_block_gone() {
    let (mut host, _counter) = counter_host(1);
    let bytes = container_bytes(&common::compile_source(&counter_program(
        "Counter := Counter + 10;",
    )));
    let accept = serde_json::json!({
        "command": "acceptEdits",
        "program": bytes,
        "edit": {"name": "hotfix"},
    })
    .to_string();
    run_line(&mut host, &accept);
    let response = run_line(&mut host, r#"{"command":"getStatus"}"#);
    assert_eq!(response["pendingEdit"]["name"], "hotfix");

    let response = run_line(&mut host, r#"{"command":"cancelEdits"}"#);
    assert_eq!(response["response"], "ack");

    let response = run_line(&mut host, r#"{"command":"getStatus"}"#);
    assert_eq!(response["pendingEdit"], serde_json::Value::Null);
}

#[test]
fn get_status_when_fresh_host_then_normal_generation_one_and_no_candidate() {
    let (mut host, _counter) = counter_host(1);

    let response = execute(Command::GetStatus, &mut host, &test_device());

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
            pending_edit: None,
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

    let response = execute(Command::GetStatus, &mut host, &test_device());

    assert!(
        matches!(response, Response::Status(_)),
        "expected a status response: {response:?}"
    );
    if let Response::Status(status) = response {
        assert_eq!(status.mode, HostMode::Testing);
        assert_eq!(status.active, 2);
        assert_eq!(status.normal, 1);
        assert_eq!(status.candidate, Some(2));
        assert_eq!(status.application, 1);
        assert!(!status.migration);
        assert_eq!(status.rounds, 3);
    }
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

#[test]
fn identity_when_fresh_host_then_device_panel_and_live_status_snapshot() {
    let (mut host, _counter) = counter_host(1);

    let response = execute(Command::Identity, &mut host, &test_device());

    assert_eq!(
        response,
        Response::Identity(Box::new(IdentityPayload {
            protocol: SESSION_PROTOCOL_VERSION,
            device: test_device(),
            application: StatusPayload {
                mode: HostMode::Normal,
                active: 1,
                normal: 1,
                candidate: None,
                application: 1,
                migration: false,
                rounds: 0,
                pending_edit: None,
            },
            redundancy: None,
        }))
    );
}
