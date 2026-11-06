//! Acceptance tests of the `replace` mode of an accept (a Download, ADR-0088):
//! the candidate takes the place of the running application, its state is built
//! fresh, and only the `PERSISTENT` variables whose declaration is equal keep
//! their value. Every scenario compiles two programs with different layouts and
//! drives a real host through the command layer, so the wire is under test too.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![expect(
    clippy::unwrap_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

mod common;

use common::{compile_codesys_with_ids, container_bytes, variable_index};
use ironplc_container::Container;
use ironplc_runtime::{
    execute, parse_command, DeviceIdentity, HostMode, Response, RuntimeHost, StatusPayload,
};

/// The stable IDs of the fixtures, by variable name.
const IDS: [(&str, u64); 7] = [
    ("Kept", 1),
    ("Retyped", 2),
    ("Gone", 3),
    ("Retained", 4),
    ("Plain", 5),
    ("Fresh", 9),
    ("Newp", 10),
];

/// The running application: one variable of each kind of retention and one the
/// candidate removes. Every variable counts up once per scan.
const RUNNING: &str = "
PROGRAM main
  VAR PERSISTENT
    Kept : DINT;
    Retyped : DINT;
    Gone : DINT;
  END_VAR
  VAR RETAIN
    Retained : DINT;
  END_VAR
  VAR
    Plain : DINT;
  END_VAR
  Kept := Kept + 1;
  Retyped := Retyped + 1;
  Gone := Gone + 1;
  Retained := Retained + 1;
  Plain := Plain + 1;
END_PROGRAM
";

/// The replacement: another layout (a variable in front of the others), `Kept`
/// declared as before, `Retyped` as a LINT, `Gone` removed, `Retained` and
/// `Plain` as before, and two new variables with declared initial values.
const REPLACEMENT: &str = "
PROGRAM main
  VAR
    Fresh : DINT := 100;
  END_VAR
  VAR PERSISTENT
    Kept : DINT;
    Retyped : LINT;
    Newp : DINT := 77;
  END_VAR
  VAR RETAIN
    Retained : DINT;
  END_VAR
  VAR
    Plain : DINT;
  END_VAR
  Kept := Kept + 1;
  Retyped := Retyped + 1;
  Retained := Retained + 1;
  Plain := Plain + 1;
END_PROGRAM
";

/// The stage of the edit view of a status, as it is on the wire.
fn stage_of(status: &StatusPayload) -> String {
    serde_json::to_value(status).unwrap()["edit"]["stage"]
        .as_str()
        .unwrap()
        .to_string()
}

fn device() -> DeviceIdentity {
    DeviceIdentity {
        device_id: None,
        name: "ironplcvm".into(),
        model: "IronPLC SoftPLC".into(),
        modification: "vm-cli".into(),
        runtime_version: "0.13.0".into(),
    }
}

/// Runs one command line through the codec and the command layer.
fn run_line(host: &mut RuntimeHost, line: &str) -> Response {
    execute(parse_command(line).unwrap(), host, &device())
}

fn accept_line(container: &Container, mode: Option<&str>) -> String {
    let mut line = serde_json::json!({
        "command": "acceptEdits",
        "program": container_bytes(container),
    });
    if let Some(mode) = mode {
        line["mode"] = serde_json::json!(mode);
    }
    line.to_string()
}

/// A permitted host running `RUNNING` that has scanned `rounds` times.
fn running_host(rounds: u64) -> (RuntimeHost, Container) {
    let running = compile_codesys_with_ids(RUNNING, &IDS);
    let mut host = RuntimeHost::new(running.clone()).unwrap();
    host.permit_execution();
    host.run(rounds, || 0).unwrap();
    (host, running)
}

fn value(host: &RuntimeHost, container: &Container, name: &str) -> i32 {
    host.read_variable(variable_index(container, name)).unwrap()
}

/// Accepts `REPLACEMENT` in replace mode and returns the compiled candidate.
fn stage_replacement(host: &mut RuntimeHost) -> (Container, Response) {
    let replacement = compile_codesys_with_ids(REPLACEMENT, &IDS);
    let response = run_line(host, &accept_line(&replacement, Some("replace")));
    (replacement, response)
}

/// Tests the staged replacement and runs the boundary and one scan round.
fn test_and_scan(host: &mut RuntimeHost) {
    assert_eq!(run_line(host, r#"{"command":"testEdits"}"#), Response::Ack);
    host.run(1, || 0).unwrap();
}

#[test]
fn accept_when_replace_then_lists_the_persistent_variables_that_start_again() {
    let (mut host, _) = running_host(3);

    let (_, response) = stage_replacement(&mut host);

    let Response::Accepted { reinitialised } = response else {
        panic!("expected the accept of a replace, got {response:?}");
    };
    let listed: Vec<(Option<u64>, String)> = reinitialised
        .iter()
        .map(|line| (line.uid, format!("{:?}", line.reason)))
        .collect();
    assert_eq!(
        listed,
        vec![
            (Some(2), "TypeChanged".to_string()),
            (Some(3), "NotInCandidate".to_string())
        ]
    );
    assert_eq!(
        reinitialised[0].name.as_deref().map(str::to_lowercase),
        Some("retyped".to_string())
    );
}

#[test]
fn replace_when_swapped_then_equal_persistent_is_kept_and_everything_else_starts_again() {
    let (mut host, _) = running_host(3);
    let (replacement, _) = stage_replacement(&mut host);

    test_and_scan(&mut host);

    // The first scan of the replacement counts from the kept 3, or from the
    // initial value for the others.
    assert_eq!(value(&host, &replacement, "Kept"), 4);
    assert_eq!(value(&host, &replacement, "Retyped"), 1);
    assert_eq!(value(&host, &replacement, "Retained"), 1);
    assert_eq!(value(&host, &replacement, "Plain"), 1);
    assert_eq!(value(&host, &replacement, "Fresh"), 100);
    assert_eq!(value(&host, &replacement, "Newp"), 77);
}

#[test]
fn replace_when_assembled_then_the_replacement_is_the_normal_application() {
    let (mut host, _) = running_host(3);
    let (replacement, _) = stage_replacement(&mut host);
    test_and_scan(&mut host);

    assert_eq!(
        run_line(&mut host, r#"{"command":"assembleEdits"}"#),
        Response::Ack
    );

    let status = host.status();
    assert_eq!(status.candidate, None);
    assert_eq!(
        status.content_hash, replacement.header.content_hash,
        "the replacement is the application now"
    );
    host.run(1, || 0).unwrap();
    assert_eq!(value(&host, &replacement, "Kept"), 5);
    // Three PERSISTENT slots and the RETAIN one.
    assert_eq!(host.retained_image().vars.len(), 4);
}

#[test]
fn replace_when_staged_then_the_status_names_the_kind_and_untest_is_refused_under_test() {
    let (mut host, _) = running_host(1);
    stage_replacement(&mut host);
    let staged = StatusPayload::from(host.status());
    assert_eq!(
        serde_json::to_value(&staged).unwrap()["candidateMode"],
        "replace"
    );
    assert_eq!(stage_of(&staged), "accepted");

    test_and_scan(&mut host);

    let under_test = StatusPayload::from(host.status());
    assert_eq!(under_test.mode, HostMode::Testing);
    assert_eq!(stage_of(&under_test), "testingReplace");
    let refusal = run_line(&mut host, r#"{"command":"untestEdits"}"#);
    let Response::Error(error) = refusal else {
        panic!("untest of a replace must be refused");
    };
    assert_eq!(error.v_code(), "V4011");
    let refusal = run_line(&mut host, r#"{"command":"cancelEdits"}"#);
    let Response::Error(error) = refusal else {
        panic!("cancel of a replace under test must be refused");
    };
    assert_eq!(error.v_code(), "V4011");
}

#[test]
fn replace_when_cancelled_before_test_then_the_running_application_is_untouched() {
    let (mut host, running) = running_host(3);
    stage_replacement(&mut host);

    assert_eq!(
        run_line(&mut host, r#"{"command":"cancelEdits"}"#),
        Response::Ack
    );
    host.run(1, || 0).unwrap();

    assert_eq!(value(&host, &running, "Kept"), 4);
    assert_eq!(host.status().candidate, None);
}

#[test]
fn replace_when_a_candidate_is_staged_then_refused_with_v4013() {
    let (mut host, _) = running_host(1);
    stage_replacement(&mut host);

    let (_, response) = stage_replacement(&mut host);

    let Response::Error(error) = response else {
        panic!("a second accept must be refused");
    };
    assert_eq!(error.v_code(), "V4013");
}

#[test]
fn replace_when_the_running_application_has_no_stable_ids_then_every_persistent_variable_is_listed()
{
    let running = compile_codesys_with_ids(RUNNING, &[]);
    let mut host = RuntimeHost::new(running).unwrap();
    host.permit_execution();
    host.run(3, || 0).unwrap();
    let replacement = compile_codesys_with_ids(REPLACEMENT, &IDS);

    // As a change the candidate has nothing to be migrated by.
    let Response::Error(refusal) = run_line(&mut host, &accept_line(&replacement, None)) else {
        panic!("a change without stable IDs must be refused");
    };
    assert_eq!(refusal.v_code(), "V4007");

    let response = run_line(&mut host, &accept_line(&replacement, Some("replace")));

    let Response::Accepted { reinitialised } = response else {
        panic!("a replace needs no stable IDs to be accepted");
    };
    assert_eq!(reinitialised.len(), 3);
    assert!(reinitialised.iter().all(|line| line.uid.is_none()));
    test_and_scan(&mut host);
    assert_eq!(value(&host, &replacement, "Kept"), 1);
}
