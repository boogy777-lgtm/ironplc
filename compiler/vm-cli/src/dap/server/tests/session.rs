//! Tests for the launch handshake, request gating and session teardown.

use super::*;

#[test]
fn serve_when_initialize_then_returns_capabilities_and_initialized_event() {
    let out = run_server(&[json!({"seq": 1, "type": "request", "command": "initialize"})]);
    assert_eq!(out.len(), 2);
    assert_eq!(out[0]["type"], "response");
    assert_eq!(out[0]["command"], "initialize");
    assert_eq!(out[0]["success"], true);
    assert_eq!(out[0]["body"]["supportsConfigurationDoneRequest"], true);
    // Only the one capability is advertised in this phase.
    assert_eq!(out[0]["body"].as_object().unwrap().len(), 1);
    assert_eq!(out[1]["type"], "event");
    assert_eq!(out[1]["event"], "initialized");
}

#[test]
fn serve_when_initialize_launch_disconnect_then_full_handshake_succeeds() {
    let (_file, path) = single_instance_debug_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
        json!({"seq": 3, "type": "request", "command": "disconnect"}),
    ]);
    // initialize response, initialized event, launch response, the
    // assumed-cycle-time notice (this container's task is freewheeling),
    // disconnect response.
    assert_eq!(out.len(), 5);
    let launch = responses(&out, "launch")[0];
    assert_eq!(launch["success"], true);
    assert_eq!(launch["request_seq"], 2);
    let disconnect = responses(&out, "disconnect")[0];
    assert_eq!(disconnect["success"], true);
}

#[test]
fn serve_when_launch_before_initialize_then_request_not_applicable() {
    let (_file, path) = single_instance_debug_container_file();
    let out = run_server(&[json!({"seq": 1, "type": "request", "command": "launch",
                                      "arguments": {"program": path}})]);
    assert_eq!(out.len(), 1);
    assert_eq!(out[0]["success"], false);
    assert_eq!(out[0]["message"], "requestNotApplicable");
}

#[test]
fn serve_when_launch_container_without_debug_then_no_debug_info_error() {
    let (_file, path) = no_debug_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
    ]);
    let launch = out.last().unwrap();
    assert_eq!(launch["command"], "launch");
    assert_eq!(launch["success"], false);
    // The message is V-coded (V6009) rather than a bare string.
    assert!(launch["message"].as_str().unwrap().starts_with("V6009 - "));
}

#[test]
fn serve_when_launch_multi_instance_then_multi_instance_error() {
    let (_file, path) = multi_instance_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
    ]);
    let launch = out.last().unwrap();
    assert_eq!(launch["success"], false);
    let message = launch["message"].as_str().unwrap();
    assert!(message.starts_with("V6010 - "));
    assert!(message.contains("MultiInstanceUnsupported"));
}

#[test]
fn serve_when_launch_vm_fails_to_start_then_launch_error() {
    // Preconditions pass, but the init function traps → launch fails.
    let (_file, path) = init_traps_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
    ]);
    let launch = out.last().unwrap();
    assert_eq!(launch["command"], "launch");
    assert_eq!(launch["success"], false);
    let message = launch["message"].as_str().unwrap();
    // The start-time trap surfaces its own V-code (divide by zero → V4001).
    assert!(message.starts_with("V4001 - "));
    assert!(message.contains("launch failed to start"));
}

#[test]
fn serve_when_launch_precondition_fails_then_session_continues_to_disconnect() {
    // A failed precondition leaves the pre-launch loop live: a subsequent
    // disconnect is still serviced.
    let (_file, path) = no_debug_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
        json!({"seq": 3, "type": "request", "command": "disconnect"}),
    ]);
    let disconnect = out.last().unwrap();
    assert_eq!(disconnect["command"], "disconnect");
    assert_eq!(disconnect["success"], true);
}

#[test]
fn serve_when_launch_missing_program_arg_then_error() {
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch", "arguments": {}}),
    ]);
    let launch = out.last().unwrap();
    assert_eq!(launch["success"], false);
    let message = launch["message"].as_str().unwrap();
    assert!(message.starts_with("V6008 - "));
    assert!(message.contains("'program'"));
}

#[test]
fn serve_when_launch_scan_limit_zero_then_error_instead_of_one_scan_session() {
    // Zero used to bound the run at the first completed scan -- the most
    // restrictive setting reachable, and the one the docs called unlimited
    // (#1515). It is now rejected before the VM starts, so no session runs.
    let (_file, path) = single_instance_debug_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "scanLimit": 0}}),
    ]);
    let launch = out.last().unwrap();
    assert_eq!(launch["command"], "launch");
    assert_eq!(launch["success"], false);
    let message = launch["message"].as_str().unwrap();
    // The same code as a missing `program`: one code for a bad argument,
    // with the message naming which one.
    assert!(message.starts_with("V6008 - "));
    assert!(message.contains("'scanLimit'"));
    assert!(message.contains("omit it"));
    // The launch never reached the VM, so there is no terminated event.
    assert!(events(&out, "terminated").is_empty());
}

#[test]
fn serve_when_launch_scan_limit_negative_then_scan_limit_error_not_missing_program() {
    // `-1` is signed-parsed precisely so the response names the argument
    // that is actually wrong rather than `program`.
    let (_file, path) = single_instance_debug_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "scanLimit": -1}}),
    ]);
    let launch = out.last().unwrap();
    assert_eq!(launch["success"], false);
    let message = launch["message"].as_str().unwrap();
    assert!(message.starts_with("V6008 - "));
    assert!(message.contains("'scanLimit'"));
}

#[test]
fn serve_when_launch_omits_scan_limit_then_runs_unbounded_until_disconnect() {
    // Absence is the only spelling of "no bound": the session keeps
    // scanning and ends on the client's disconnect, not on its own.
    let (_file, path) = single_instance_debug_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
        json!({"seq": 3, "type": "request", "command": "disconnect"}),
    ]);
    assert_eq!(responses(&out, "launch")[0]["success"], true);
    assert!(events(&out, "terminated").is_empty());
    assert_eq!(responses(&out, "disconnect")[0]["success"], true);
}

#[test]
fn serve_when_pause_after_initialize_then_request_not_applicable() {
    // `pause` is a modelled-but-cut request: always requestNotApplicable.
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "pause"}),
    ]);
    let pause = out.last().unwrap();
    assert_eq!(pause["command"], "pause");
    assert_eq!(pause["success"], false);
    assert_eq!(pause["message"], "requestNotApplicable");
}

#[test]
fn serve_when_unknown_command_then_request_not_applicable() {
    // A custom request in IronPLC's namespace that the server does not
    // implement (unlike `ironplc/stepScan`, which it does).
    let out = run_server(&[json!({"seq": 1, "type": "request",
                                      "command": "ironplc/forceVariable"})]);
    assert_eq!(out[0]["success"], false);
    assert_eq!(out[0]["message"], "requestNotApplicable");
}

#[test]
fn serve_when_step_scan_before_a_pause_then_request_not_applicable() {
    // Scan stepping is execution control: it needs a live pause to step
    // from, the same as `continue` or `next`.
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "ironplc/stepScan"}),
    ]);
    let step_scan = out.last().unwrap();
    assert_eq!(step_scan["command"], "ironplc/stepScan");
    assert_eq!(step_scan["success"], false);
    assert_eq!(step_scan["message"], "requestNotApplicable");
}

#[test]
fn serve_when_disconnect_after_launch_then_post_launch_loop_tears_down() {
    // Exercise the post-launch loop's requestNotApplicable branch, then
    // disconnect.
    let (_file, path) = single_instance_debug_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
        json!({"seq": 3, "type": "request", "command": "threads"}),
        json!({"seq": 4, "type": "request", "command": "disconnect"}),
    ]);
    // Post-launch `threads` is refused for now.
    let threads = responses(&out, "threads")[0];
    assert_eq!(threads["success"], false);
    assert_eq!(threads["message"], "requestNotApplicable");
    // Then disconnect is honored.
    let disconnect = responses(&out, "disconnect")[0];
    assert_eq!(disconnect["success"], true);
}

#[test]
fn serve_when_stream_ends_without_disconnect_then_returns_ok() {
    // No trailing disconnect: a clean EOF just ends the session.
    let out = run_server(&[json!({"seq": 1, "type": "request", "command": "initialize"})]);
    // Handshake still produced its two messages; the loop returned on EOF.
    assert_eq!(out.len(), 2);
}
