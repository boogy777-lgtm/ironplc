use std::net::TcpListener;
use std::sync::Mutex;

use ironplc_project::online_comparison::{ActionKind, EditStage, NoteKind, Row, RunState};
use ironplc_session_client::{Endpoint, Session, TcpEndpoint};
use ironplc_test::device::SoftDevice;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use super::*;
use crate::tools::test_support::{ed2_options, source, COUNTER_PROGRAM};

fn cache() -> Mutex<ContainerCache> {
    Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024))
}

fn tcp_device(port: u16) -> ResolvedDevice {
    ResolvedDevice {
        endpoint: Endpoint::Tcp(TcpEndpoint {
            address: "127.0.0.1".to_string(),
            port,
        }),
        requested_id: None,
    }
}

/// `COUNTER_PROGRAM` with another scan body: the same declarations.
fn counter_variant(body: &str) -> String {
    format!("PROGRAM Main\nVAR\n  Counter : INT;\nEND_VAR\n  {body}\nEND_PROGRAM")
}

/// A device that runs `COUNTER_PROGRAM`, and the directory that holds its file.
fn counter_device() -> (SoftDevice, ResolvedDevice, TempDir) {
    let candidate = crate::tools::hot_edit::prepare_candidate(
        &source(COUNTER_PROGRAM),
        &ed2_options(),
        &cache(),
    )
    .map_err(|response| response.diagnostics)
    .unwrap();
    let dir = TempDir::new().unwrap();
    let file = dir.path().join("counter.iplc");
    std::fs::write(&file, candidate.program).unwrap();
    let device = SoftDevice::start(&file);
    let resolved = tcp_device(device.address().port());
    (device, resolved, dir)
}

fn prepared(text: &str) -> Prepared {
    prepare_project(Some(&source(text)), Some(&ed2_options()), &cache())
}

fn enabled(comparison: &Comparison) -> Vec<ActionKind> {
    comparison
        .actions
        .iter()
        .filter(|offer| offer.enabled)
        .map(|offer| offer.action)
        .collect()
}

#[spec_test(REQ_TOL_mcp_179)]
#[test]
fn build_response_when_device_runs_the_compiled_sources_then_up_to_date_and_hot_edit_allowed() {
    let (_device, resolved, _dir) = counter_device();

    let response = build_response(&prepared(COUNTER_PROGRAM), &resolved);

    assert!(response.ok, "{:?}", response.diagnostics);
    let comparison = response.comparison.unwrap();
    assert_eq!(comparison.row, Row::UpToDate);
    assert!(comparison.connected);
    assert!(comparison.hot_edit.allowed);
    assert_eq!(comparison.controller.unwrap().device_id.unwrap().len(), 32);
    assert_eq!(comparison.project.application.as_deref(), Some("main"));
}

#[spec_test(REQ_TOL_mcp_180)]
#[test]
fn build_response_when_device_runs_other_code_then_other_application_and_download_only() {
    let (_device, resolved, _dir) = counter_device();
    let edit = counter_variant("Counter := Counter + 10;");

    let response = build_response(&prepared(&edit), &resolved);

    let comparison = response.comparison.unwrap();
    assert_eq!(comparison.row, Row::OtherApplication);
    assert!(!comparison.hot_edit.allowed);
    assert_eq!(
        enabled(&comparison),
        vec![
            ActionKind::Monitor,
            ActionKind::Download,
            ActionKind::Search,
            ActionKind::ChooseAnother
        ]
    );
    assert!(comparison.last_download.is_none());
}

#[spec_test(REQ_TOL_mcp_180)]
#[test]
fn build_response_when_no_sources_then_the_controller_is_compared_with_no_project() {
    let (_device, resolved, _dir) = counter_device();
    let prepared = prepare_project(None, None, &cache());

    let response = build_response(&prepared, &resolved);

    let comparison = response.comparison.unwrap();
    let value = serde_json::to_value(&comparison).unwrap();
    assert_eq!(value["project"]["status"], "neverBuilt");
    assert_eq!(value["project"]["contentHash"], serde_json::Value::Null);
    assert_eq!(value["controller"]["holds"]["kind"], "application");
    assert!(response.diagnostics.is_empty());
}

#[spec_test(REQ_TOL_mcp_183)]
#[test]
fn build_response_when_sources_do_not_build_then_build_fails_and_the_diagnostics_come_back() {
    let (_device, resolved, _dir) = counter_device();

    let response = build_response(
        &prepared("PROGRAM Main\nCounter := ;\nEND_PROGRAM"),
        &resolved,
    );

    assert!(response.ok);
    assert!(!response.diagnostics.is_empty());
    let comparison = response.comparison.unwrap();
    let kinds: Vec<NoteKind> = comparison.information.iter().map(|row| row.kind).collect();
    assert_eq!(kinds, vec![NoteKind::BuildFails]);
    assert!(!comparison.hot_edit.allowed);
    assert!(comparison.actions[0].enabled, "Monitor connects anyway");
}

#[spec_test(REQ_TOL_mcp_181)]
#[test]
fn build_response_when_nothing_listens_then_unreachable_is_a_comparison() {
    let port = TcpListener::bind("127.0.0.1:0")
        .unwrap()
        .local_addr()
        .unwrap()
        .port();

    let response = build_response(&prepared(COUNTER_PROGRAM), &tcp_device(port));

    assert!(response.ok);
    let comparison = response.comparison.unwrap();
    assert_eq!(comparison.row, Row::Unreachable);
    assert!(!comparison.connected);
    assert!(comparison.controller.is_none());
}

#[spec_test(REQ_TOL_mcp_181)]
#[test]
fn build_response_when_device_is_busy_then_busy_is_a_comparison() {
    let (_device, resolved, _dir) = counter_device();
    let _other_session = Session::connect(&resolved.endpoint).unwrap();

    let response = build_response(&prepared(COUNTER_PROGRAM), &resolved);

    assert!(response.ok);
    assert_eq!(response.comparison.unwrap().row, Row::Busy);
}

#[spec_test(REQ_TOL_mcp_179)]
#[test]
fn build_response_when_called_back_to_back_then_no_call_finds_the_device_busy() {
    let (_device, resolved, _dir) = counter_device();
    let prepared = prepared(COUNTER_PROGRAM);

    let rows: Vec<Row> = (0..10)
        .map(|_| build_response(&prepared, &resolved).comparison.unwrap().row)
        .collect();

    assert_eq!(rows, vec![Row::UpToDate; 10]);
}

#[spec_test(REQ_TOL_mcp_182)]
#[test]
fn unresolved_when_device_is_both_address_and_id_then_p8001_and_no_comparison() {
    let fault = DeviceFault::Invalid(text::mcp_input_validation_device_selector_invalid());

    let response = unresolved(&prepared(COUNTER_PROGRAM), fault);

    assert!(!response.ok);
    assert!(response.comparison.is_none());
    let last = response.diagnostics.last().unwrap();
    assert_eq!(last["number"], 8001);
}

#[spec_test(REQ_TOL_mcp_179)]
#[test]
fn build_response_when_serialized_then_the_comparison_has_the_fields_of_the_command_line() {
    let (_device, resolved, _dir) = counter_device();

    let response = build_response(&prepared(COUNTER_PROGRAM), &resolved);

    let value = serde_json::to_value(&response).unwrap();
    assert_eq!(value["ok"], true);
    assert_eq!(value["comparison"]["row"], "upToDate");
    assert_eq!(value["comparison"]["rowNumber"], 5);
    assert_eq!(value["comparison"]["hotEdit"]["allowed"], true);
    assert_eq!(value["comparison"]["actions"][0]["action"], "monitor");
    assert!(value.get("vCode").is_none());
}

/// Maps every stage of the runtime to the stage the comparison reads. The match
/// has no wildcard: a stage that the runtime adds does not compile here until the
/// comparison's reader knows it.
fn mirrored(stage: ironplc_runtime::EditStage) -> EditStage {
    match stage {
        ironplc_runtime::EditStage::Idle => EditStage::Idle,
        ironplc_runtime::EditStage::Accepted => EditStage::Accepted,
        ironplc_runtime::EditStage::Testing => EditStage::Testing,
        ironplc_runtime::EditStage::TestingMigration => EditStage::TestingMigration,
        ironplc_runtime::EditStage::TestingReplace => EditStage::TestingReplace,
        ironplc_runtime::EditStage::SwapPending => EditStage::SwapPending,
    }
}

fn mirrored_run_state(state: ironplc_runtime::RunState) -> RunState {
    match state {
        ironplc_runtime::RunState::Running => RunState::Running,
        ironplc_runtime::RunState::Stopped => RunState::Stopped,
    }
}

#[test]
fn facts_of_when_the_identity_of_a_device_then_every_field_the_comparison_reads_is_there() {
    let (_device, resolved, _dir) = counter_device();
    let session = Session::connect(&resolved.endpoint).unwrap();
    let identity = session.identity().unwrap();

    let facts = facts_of(identity).unwrap();

    assert_eq!(facts.device_id.unwrap().to_string().len(), 32);
    assert!(facts.content_hash.is_some());
    assert!(facts.layout_hash.is_some());
    assert_eq!(
        facts.container_format,
        Some(ironplc_container::FORMAT_VERSION)
    );
    assert_eq!(facts.edit.unwrap().stage, EditStage::Idle);
    assert_eq!(facts.run_state, RunState::Running);
}

#[test]
fn facts_of_when_every_stage_and_run_state_of_the_runtime_then_the_comparison_reads_the_same() {
    let (_device, resolved, _dir) = counter_device();
    let session = Session::connect(&resolved.endpoint).unwrap();
    let identity = session.identity().unwrap().clone();
    let stages = [
        ironplc_runtime::EditStage::Idle,
        ironplc_runtime::EditStage::Accepted,
        ironplc_runtime::EditStage::Testing,
        ironplc_runtime::EditStage::TestingMigration,
        ironplc_runtime::EditStage::TestingReplace,
        ironplc_runtime::EditStage::SwapPending,
    ];
    let states = [
        ironplc_runtime::RunState::Running,
        ironplc_runtime::RunState::Stopped,
    ];

    let read_stages: Vec<EditStage> = stages
        .iter()
        .map(|stage| {
            let mut identity = identity.clone();
            identity.application.edit.as_mut().unwrap().stage = *stage;
            facts_of(&identity).unwrap().edit.unwrap().stage
        })
        .collect();
    let read_states: Vec<RunState> = states
        .iter()
        .map(|state| {
            let mut identity = identity.clone();
            identity.run_state = *state;
            facts_of(&identity).unwrap().run_state
        })
        .collect();

    assert_eq!(
        read_stages,
        stages
            .iter()
            .map(|stage| mirrored(*stage))
            .collect::<Vec<_>>()
    );
    assert_eq!(
        read_states,
        states
            .iter()
            .map(|state| mirrored_run_state(*state))
            .collect::<Vec<_>>()
    );
}

#[test]
fn facts_of_when_the_runtime_refuses_accept_then_the_reason_is_the_runtimes_own() {
    let (_device, resolved, _dir) = counter_device();
    let cache = cache();
    let edit = counter_variant("Counter := Counter + 10;");
    crate::tools::hot_edit::build_accept_response(
        &source(&edit),
        &ed2_options(),
        &cache,
        &resolved,
    );

    // The session of the call above has ended; a new one reads the edit view.
    let response = build_response(&prepared(COUNTER_PROGRAM), &resolved);

    let comparison = response.comparison.unwrap();
    assert_eq!(comparison.row, Row::UpToDate);
    assert!(!comparison.hot_edit.allowed);
    let value = serde_json::to_value(&comparison.hot_edit).unwrap();
    assert_eq!(value["reason"]["code"], "V4013");
    assert_eq!(
        comparison
            .information
            .iter()
            .map(|row| row.kind)
            .collect::<Vec<_>>(),
        vec![NoteKind::EditInProgress]
    );
    assert_eq!(
        enabled(&comparison),
        vec![
            ActionKind::Monitor,
            ActionKind::Search,
            ActionKind::ChooseAnother,
            ActionKind::Resolve
        ]
    );
}
