use std::net::TcpListener;
use std::sync::Mutex;

use ironplc_runtime::{HostMode, RunState};
use ironplc_session_client::Session;
use ironplc_test::device::SoftDevice;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use super::*;
use crate::cache::ContainerCache;
use crate::tools::hot_edit::{
    build_accept_response, build_assemble_response, build_cancel_response, build_start_response,
    build_status_response, build_stop_response, build_test_response, HotEditSession,
};
use crate::tools::test_support::{ed2_options, source, COUNTER_PROGRAM};
use ironplc_session_client::{Endpoint, TcpEndpoint};

/// `COUNTER_PROGRAM` with another scan body: the same declarations, so the
/// candidate stages as a logic edit.
fn counter_variant(body: &str) -> String {
    format!("PROGRAM Main\nVAR\n  Counter : INT;\nEND_VAR\n  {body}\nEND_PROGRAM")
}

fn cache() -> Mutex<ContainerCache> {
    Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024))
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

fn tcp_device(port: u16) -> ResolvedDevice {
    ResolvedDevice {
        endpoint: Endpoint::Tcp(TcpEndpoint {
            address: "127.0.0.1".to_string(),
            port,
        }),
        requested_id: None,
    }
}

#[spec_test(REQ_TOL_mcp_171)]
#[test]
fn build_status_response_when_device_then_the_status_of_the_device_and_who_answered() {
    let (_device, resolved, _dir) = counter_device();

    let response = build_status_response(&resolved);

    assert!(response.ok, "{:?}", response.diagnostics);
    assert_eq!(response.result.as_deref(), Some("ack"));
    assert_eq!(response.status.unwrap().mode, HostMode::Normal);
    let identity = response.device.unwrap();
    assert_eq!(identity.name, "ironplcvm");
    assert_eq!(identity.device_id.as_ref().map(String::len), Some(32));
}

#[spec_test(REQ_TOL_mcp_171)]
#[test]
fn build_flow_when_device_accept_test_assemble_then_the_candidate_becomes_the_application() {
    let (_device, resolved, _dir) = counter_device();
    let cache = cache();
    let edit = counter_variant("Counter := Counter + 10;");

    let staged = build_accept_response(&source(&edit), &ed2_options(), &cache, &resolved);
    let tested = build_test_response(&resolved);
    let assembled = build_assemble_response(&resolved);

    assert!(staged.ok, "{:?} {:?}", staged.v_code, staged.message);
    assert_eq!(staged.result.as_deref(), Some("ack"));
    assert!(staged.status.unwrap().candidate.is_some());
    assert_eq!(tested.status.unwrap().mode, HostMode::Testing);
    let status = assembled.status.unwrap();
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.candidate, None);
    assert_eq!(status.normal, 2);
}

#[spec_test(REQ_TOL_mcp_171)]
#[test]
fn build_test_response_when_device_has_no_candidate_then_the_v_code_of_the_device() {
    let (_device, resolved, _dir) = counter_device();

    let response = build_test_response(&resolved);

    assert!(!response.ok);
    assert_eq!(response.v_code.as_deref(), Some("V4012"));
    assert!(response.device.is_some());
    let cancel = build_cancel_response(&resolved);
    assert_eq!(cancel.v_code.as_deref(), Some("V4012"));
}

#[spec_test(REQ_TOL_mcp_171)]
#[test]
fn build_accept_response_when_device_refuses_then_the_message_is_the_reason_of_the_edit_view() {
    let (_device, resolved, _dir) = counter_device();
    let cache = cache();
    let edit = counter_variant("Counter := Counter + 10;");
    build_accept_response(&source(&edit), &ed2_options(), &cache, &resolved);

    let second = build_accept_response(&source(&edit), &ed2_options(), &cache, &resolved);

    assert!(!second.ok);
    let view = second.status.unwrap().edit.unwrap();
    let reason = view.commands.edit[&ironplc_runtime::EditCommand::Accept]
        .reason
        .clone()
        .unwrap();
    assert_eq!(second.v_code.as_deref(), Some("V4013"));
    assert_eq!(second.v_code, Some(reason.code));
    assert_eq!(second.message, Some(reason.text));
}

#[spec_test(REQ_TOL_mcp_171)]
#[test]
fn build_cancel_response_when_device_runs_the_candidate_under_test_then_one_call_ends_the_edit() {
    let (_device, resolved, _dir) = counter_device();
    let cache = cache();
    let edit = counter_variant("Counter := Counter + 10;");
    build_accept_response(&source(&edit), &ed2_options(), &cache, &resolved);
    let tested = build_test_response(&resolved);
    let view = tested.status.unwrap().edit.unwrap();
    assert_eq!(view.stage, ironplc_runtime::EditStage::Testing);
    assert!(view.commands.edit[&ironplc_runtime::EditCommand::Cancel].enabled);

    let cancelled = build_cancel_response(&resolved);

    assert!(
        cancelled.ok,
        "{:?} {:?}",
        cancelled.v_code, cancelled.message
    );
    let status = cancelled.status.unwrap();
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.candidate, None);
    assert_eq!(status.edit.unwrap().stage, ironplc_runtime::EditStage::Idle);
}

#[spec_test(REQ_TOL_mcp_175)]
#[test]
fn build_status_response_when_called_back_to_back_then_no_call_finds_the_device_busy() {
    let (_device, resolved, _dir) = counter_device();

    let responses: Vec<_> = (0..20).map(|_| build_status_response(&resolved)).collect();

    for (call, response) in responses.iter().enumerate() {
        assert!(
            response.ok,
            "call {call}: {:?} {:?}",
            response.v_code, response.message
        );
    }
}

#[spec_test(REQ_TOL_mcp_172)]
#[test]
fn build_status_response_when_device_is_busy_then_v6014() {
    let (_device, resolved, _dir) = counter_device();
    let _other_session = Session::connect(&resolved.endpoint).unwrap();

    let response = build_status_response(&resolved);

    assert!(!response.ok);
    assert_eq!(response.v_code.as_deref(), Some("V6014"));
}

#[spec_test(REQ_TOL_mcp_173)]
#[test]
fn build_status_response_when_nothing_listens_then_p8003_and_no_v_code() {
    let port = TcpListener::bind("127.0.0.1:0")
        .unwrap()
        .local_addr()
        .unwrap()
        .port();

    let response = build_status_response(&tcp_device(port));

    assert!(!response.ok);
    assert!(response.v_code.is_none());
    assert_eq!(response.diagnostics[0]["number"], 8003);
    assert!(response.diagnostics[0]["text"]
        .as_str()
        .unwrap()
        .contains(&port.to_string()));
}

#[spec_test(REQ_TOL_mcp_170)]
#[test]
fn build_status_response_when_in_process_then_the_shape_has_no_device() {
    let session = Mutex::new(HotEditSession::default());

    let response = build_status_response(&session);

    let json = serde_json::to_value(&response).unwrap();
    assert!(json.get("device").is_none());
    assert_eq!(json["diagnostics"][0]["number"], 8001);
}

/// A device that serves a program that traps in its first round.
fn trapping_device() -> (SoftDevice, ResolvedDevice, TempDir) {
    let candidate = crate::tools::hot_edit::prepare_candidate(
        &source(
            "PROGRAM Main
VAR
  Counter : DINT;
END_VAR
  Counter := 1 / Counter;
END_PROGRAM",
        ),
        &ed2_options(),
        &cache(),
    )
    .map_err(|response| response.diagnostics)
    .unwrap();
    let dir = TempDir::new().unwrap();
    let file = dir.path().join("trapping.iplc");
    std::fs::write(&file, candidate.program).unwrap();
    let device = SoftDevice::start(&file);
    let resolved = tcp_device(device.address().port());
    (device, resolved, dir)
}

#[spec_test(REQ_TOL_mcp_176)]
#[test]
fn build_stop_response_when_device_then_stopped_and_start_then_running_with_the_status() {
    let (_device, resolved, _dir) = counter_device();

    let stopped = build_stop_response(&resolved);
    let stopped_again = build_stop_response(&resolved);
    let started = build_start_response(&resolved);

    assert!(stopped.ok, "{:?} {:?}", stopped.v_code, stopped.message);
    assert_eq!(stopped.result.as_deref(), Some("ack"));
    assert_eq!(stopped.run_state, Some(RunState::Stopped));
    assert_eq!(stopped.status.unwrap().mode, HostMode::Normal);
    assert_eq!(stopped.device.unwrap().name, "ironplcvm");
    assert_eq!(stopped_again.run_state, Some(RunState::Stopped));
    assert_eq!(started.run_state, Some(RunState::Running));
    assert!(started.status.is_some());
}

#[spec_test(REQ_TOL_mcp_177)]
#[test]
fn build_stop_response_when_candidate_staged_then_the_edit_is_untouched() {
    let (_device, resolved, _dir) = counter_device();
    let cache = cache();
    let edit = counter_variant("Counter := Counter + 10;");
    build_accept_response(&source(&edit), &ed2_options(), &cache, &resolved);
    build_test_response(&resolved);

    let stopped = build_stop_response(&resolved);
    let started = build_start_response(&resolved);

    assert_eq!(stopped.status.unwrap().mode, HostMode::Testing);
    let status = started.status.unwrap();
    assert_eq!(status.mode, HostMode::Testing);
    assert!(status.candidate.is_some());
}

/// Starts the device until it refuses: the device traps in its first round,
/// which runs on its own thread, so a start that comes first is acknowledged.
fn start_until_refused(device: &ResolvedDevice) -> crate::tools::hot_edit::HotEditResponse {
    let mut response = build_start_response(device);
    for _ in 0..100 {
        if !response.ok {
            break;
        }
        std::thread::sleep(std::time::Duration::from_millis(20));
        response = build_start_response(device);
    }
    response
}

#[spec_test(REQ_TOL_mcp_178)]
#[test]
fn build_start_response_when_device_is_faulted_then_v6019_and_stop_is_accepted() {
    let (_device, resolved, _dir) = trapping_device();

    let started = start_until_refused(&resolved);
    let stopped = build_stop_response(&resolved);

    assert!(!started.ok);
    assert_eq!(started.v_code.as_deref(), Some("V6019"));
    assert_eq!(started.run_state, None);
    assert!(stopped.ok);
    assert_eq!(stopped.run_state, Some(RunState::Stopped));
}

#[spec_test(REQ_TOL_mcp_178)]
#[test]
fn build_start_response_when_in_process_then_v4022() {
    let session = Mutex::new(HotEditSession::default());
    let candidate = crate::tools::hot_edit::prepare_candidate(
        &source(COUNTER_PROGRAM),
        &ed2_options(),
        &cache(),
    )
    .map_err(|response| response.diagnostics)
    .unwrap();
    crate::tools::hot_edit::send_candidate(candidate.program, None, &Default::default(), &session);

    let started = build_start_response(&session);

    assert!(!started.ok);
    assert_eq!(started.v_code.as_deref(), Some("V4022"));
}
