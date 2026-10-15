//! Tests for breakpoints: stopping, refiring, entry stops and unresolved lines.

use super::*;

#[test]
fn serve_when_no_breakpoints_and_scan_limit_then_runs_to_bound_and_terminates() {
    // With no breakpoint, `scanLimit` is what bounds the run: the loop keeps
    // scanning until `scan_count` reaches it (without a bound this program
    // would scan forever, as the single-threaded loop has no `pause`).
    let (_file, path) = scan_container_file(&[0x8C], 0, &[(0, 10)]);
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "scanLimit": 1}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "disconnect"}),
    ]);
    assert_eq!(responses(&out, "configurationDone")[0]["success"], true);
    // The scan bound is reached → a single terminated event, then disconnect.
    assert_eq!(events(&out, "terminated").len(), 1);
    assert_eq!(responses(&out, "disconnect")[0]["success"], true);
    // terminated precedes the disconnect response.
    let terminated_at = index_of(&out, |m| m["event"] == "terminated");
    let disconnect_at = index_of(&out, |m| m["command"] == "disconnect");
    assert!(terminated_at < disconnect_at);
}

#[test]
fn serve_when_breakpoint_then_stops_inspects_continues_and_terminates() {
    let (_file, path) = scan_container_file(&[0x8C], 0, &[(0, 10)]);
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "scanLimit": 1}}),
        json!({"seq": 3, "type": "request", "command": "setBreakpoints",
                   "arguments": {"source": {"path": "demo.st"},
                                 "breakpoints": [{"line": 10}]}}),
        json!({"seq": 4, "type": "request", "command": "configurationDone"}),
        json!({"seq": 5, "type": "request", "command": "threads"}),
        json!({"seq": 6, "type": "request", "command": "stackTrace",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 7, "type": "request", "command": "scopes",
                   "arguments": {"frameId": 0}}),
        json!({"seq": 8, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 1}}),
        json!({"seq": 9, "type": "request", "command": "continue",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 10, "type": "request", "command": "disconnect"}),
    ]);

    // The breakpoint is verified back to the client at its bound line.
    let sbp = responses(&out, "setBreakpoints");
    assert_eq!(sbp[0]["body"]["breakpoints"][0]["verified"], true);
    assert_eq!(sbp[0]["body"]["breakpoints"][0]["line"], 10);

    // The VM stops at the breakpoint before running to completion.
    let stopped = events(&out, "stopped");
    assert_eq!(stopped.len(), 1);
    assert_eq!(stopped[0]["body"]["reason"], "breakpoint");
    assert_eq!(stopped[0]["body"]["threadId"], 1);

    // One synthetic thread.
    assert_eq!(responses(&out, "threads")[0]["body"]["threads"][0]["id"], 1);

    // The stack has the single scan frame, resolved to its POU name and
    // source location.
    let st = responses(&out, "stackTrace");
    assert_eq!(st[0]["body"]["stackFrames"].as_array().unwrap().len(), 1);
    assert_eq!(st[0]["body"]["stackFrames"][0]["name"], "MAIN");
    assert_eq!(st[0]["body"]["stackFrames"][0]["line"], 10);
    assert_eq!(st[0]["body"]["stackFrames"][0]["source"]["path"], "demo.st");

    // One scope, whose handle enumerates the variables.
    let sc = responses(&out, "scopes");
    assert_eq!(
        sc[0]["body"]["scopes"][0]["variablesReference"],
        PROGRAM_REF
    );

    // The single program variable is rendered with its source name and
    // declared type.
    let vars = responses(&out, "variables");
    assert_eq!(vars[0]["body"]["variables"][0]["name"], "x");
    assert_eq!(vars[0]["body"]["variables"][0]["type"], "DINT");

    // Continue resumes, past the breakpoint, to completion.
    assert_eq!(
        responses(&out, "continue")[0]["body"]["allThreadsContinued"],
        true
    );
    assert_eq!(events(&out, "terminated").len(), 1);

    // Ordering: stop before inspection, terminate after continue.
    let stopped_at = index_of(&out, |m| m["event"] == "stopped");
    let stacktrace_at = index_of(&out, |m| m["command"] == "stackTrace");
    let continue_at = index_of(&out, |m| m["command"] == "continue");
    let terminated_at = index_of(&out, |m| m["event"] == "terminated");
    assert!(stopped_at < stacktrace_at);
    assert!(continue_at < terminated_at);
}

#[test]
fn serve_when_breakpoint_then_refires_every_scan_and_variable_evolves() {
    // The core Phase 4c behavior: the loop keeps scanning, so a breakpoint
    // fires once per scan and `var[0]` grows across cycles (1, 2, 3, …).
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
        // Breakpoint on the RET_VOID line, after the increment statement.
        json!({"seq": 3, "type": "request", "command": "setBreakpoints",
                   "arguments": {"source": {"path": "demo.st"},
                                 "breakpoints": [{"line": 11}]}}),
        json!({"seq": 4, "type": "request", "command": "configurationDone"}),
        // Scan 1 paused: inspect, then continue to scan 2.
        json!({"seq": 5, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 1}}),
        json!({"seq": 6, "type": "request", "command": "continue",
                   "arguments": {"threadId": 1}}),
        // Scan 2 paused: inspect, then continue to scan 3.
        json!({"seq": 7, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 1}}),
        json!({"seq": 8, "type": "request", "command": "continue",
                   "arguments": {"threadId": 1}}),
        // Scan 3 paused: inspect, then tear down.
        json!({"seq": 9, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 1}}),
        json!({"seq": 10, "type": "request", "command": "disconnect"}),
    ]);

    // A breakpoint stop for each of the three scans (no `terminated`).
    assert_eq!(events(&out, "stopped").len(), 3);
    assert!(events(&out, "terminated").is_empty());
    for stopped in events(&out, "stopped") {
        assert_eq!(stopped["body"]["reason"], "breakpoint");
    }

    // The variable increments once per scan: 1, then 2, then 3.
    let vars = responses(&out, "variables");
    assert_eq!(vars.len(), 3);
    assert_eq!(vars[0]["body"]["variables"][0]["value"], "1");
    assert_eq!(vars[1]["body"]["variables"][0]["value"], "2");
    assert_eq!(vars[2]["body"]["variables"][0]["value"], "3");
}

#[test]
fn serve_when_stop_on_entry_then_pauses_before_first_instruction() {
    // `stopOnEntry` pauses before any logic runs, so the incrementing
    // scan has not yet touched `var[0]` (still 0) at the entry stop.
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "stopOnEntry": true}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 1}}),
        json!({"seq": 5, "type": "request", "command": "disconnect"}),
    ]);

    let stopped = events(&out, "stopped");
    assert_eq!(stopped.len(), 1);
    assert_eq!(stopped[0]["body"]["reason"], "entry");
    // No logic has run yet: the increment has not happened.
    let vars = responses(&out, "variables");
    assert_eq!(vars[0]["body"]["variables"][0]["value"], "0");
}

#[test]
fn serve_when_setbreakpoints_line_unresolvable_then_reports_unverified() {
    // A line past the last executable line has nothing to snap to.
    let (_file, path) = scan_container_file(&[0x8C], 0, &[(0, 10)]);
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
        json!({"seq": 3, "type": "request", "command": "setBreakpoints",
                   "arguments": {"source": {"path": "demo.st"},
                                 "breakpoints": [{"line": 9999}, {"line": 65546},
                                                 {"line": 10}]}}),
        json!({"seq": 4, "type": "request", "command": "disconnect"}),
    ]);
    let sbp = responses(&out, "setBreakpoints");
    let bps = &sbp[0]["body"]["breakpoints"];
    assert_eq!(bps[0]["verified"], false);
    assert!(bps[0]["message"].is_string());
    // A line beyond what the debug section can represent is rejected at
    // the serde boundary, and its line is omitted rather than truncated.
    assert_eq!(bps[1]["verified"], false);
    assert!(bps[1].get("line").is_none());
    // The malformed entries do not sink the valid one alongside them.
    assert_eq!(bps[2]["verified"], true);
    assert_eq!(bps[2]["line"], 10);
}
