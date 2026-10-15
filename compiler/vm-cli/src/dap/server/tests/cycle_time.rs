//! Tests for the cycle time a session assumes or takes from the task.

use super::*;

#[test]
fn serve_when_freewheeling_program_then_reports_assumed_cycle_time() {
    // A program with no INTERVAL has no cycle time of its own, so the
    // session assumes one — and has to say so, because every timer in the
    // program elapses on that assumption.
    let (_file, path) = scan_container_file(&[0x8C], 0, &[(0, 10)]);
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "scanLimit": 1}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "disconnect"}),
    ]);

    let lines = console_lines(&out);
    assert_eq!(lines.len(), 1, "console: {lines:?}");
    assert!(lines[0].contains("100 ms"), "console: {lines:?}");
    assert!(
        lines[0].contains("launch configuration"),
        "the notice says where to change it: {lines:?}"
    );
}

#[test]
fn serve_when_freewheeling_interval_supplied_then_reports_that_cycle_time() {
    let (_file, path) = scan_container_file(&[0x8C], 0, &[(0, 10)]);
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "scanLimit": 1,
                                 "freewheelingIntervalMs": 5}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "disconnect"}),
    ]);

    let lines = console_lines(&out);
    assert!(lines[0].contains("5 ms"), "console: {lines:?}");
}

#[test]
fn serve_when_freewheeling_interval_out_of_range_then_reports_the_default() {
    // A typo in a launch configuration should not stop the session from
    // starting, but the notice still reports what was actually used.
    let (_file, path) = scan_container_file(&[0x8C], 0, &[(0, 10)]);
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "scanLimit": 1,
                                 "freewheelingIntervalMs": 0}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "disconnect"}),
    ]);

    assert_eq!(responses(&out, "launch")[0]["success"], true);
    let lines = console_lines(&out);
    assert!(lines[0].contains("100 ms"), "console: {lines:?}");
}

#[test]
fn serve_when_cyclic_program_then_assumes_nothing() {
    let (_file, path) = cyclic_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "scanLimit": 1}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "disconnect"}),
    ]);

    assert!(console_lines(&out).is_empty());
}
