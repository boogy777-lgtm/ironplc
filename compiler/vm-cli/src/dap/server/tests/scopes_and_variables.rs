//! Tests for the Variables and Runtime scopes and the variables they expose.

use super::*;

#[test]
fn serve_when_runtime_scope_at_entry_then_reports_no_completed_scans() {
    // `scan_count` counts *completed* cycles, so the entry stop -- which
    // happens before the first cycle runs -- reports 0. A falsy check
    // somewhere in the chain would render this as absent instead of zero.
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "stopOnEntry": true}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 5, "type": "request", "command": "disconnect"}),
    ]);

    let vars = responses(&out, "variables");
    assert_eq!(vars.len(), 1);
    assert_eq!(vars[0]["body"]["variables"][0]["name"], "scanCount");
    assert_eq!(vars[0]["body"]["variables"][0]["value"], "0");
}

#[test]
fn serve_when_scopes_requested_then_offers_variables_and_runtime() {
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "stopOnEntry": true}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "scopes",
                   "arguments": {"frameId": 0}}),
        json!({"seq": 5, "type": "request", "command": "disconnect"}),
    ]);

    let scopes = &responses(&out, "scopes")[0]["body"]["scopes"];
    // Not "Variables" -- that would render as Variables > Variables inside
    // the client's variables pane.
    assert_eq!(scopes[0]["name"], "Program");
    assert_eq!(scopes[1]["name"], "Runtime");
    // The two scopes must be addressable independently.
    assert_ne!(
        scopes[0]["variablesReference"],
        scopes[1]["variablesReference"]
    );
}

#[test]
fn serve_when_runtime_scope_expanded_then_shows_scan_count_advancing() {
    // The Runtime scope is what replaces the old "show scan count" button:
    // the client re-reads it at each stop, so the value must track cycles.
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
        json!({"seq": 3, "type": "request", "command": "setBreakpoints",
                   "arguments": {"source": {"path": "demo.st"},
                                 "breakpoints": [{"line": 11}]}}),
        json!({"seq": 4, "type": "request", "command": "configurationDone"}),
        json!({"seq": 5, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 6, "type": "request", "command": "continue",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 7, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 8, "type": "request", "command": "disconnect"}),
    ]);

    let vars = responses(&out, "variables");
    for scope in &vars {
        assert_eq!(scope["body"]["variables"][0]["name"], "scanCount");
        assert_eq!(scope["body"]["variables"][0]["type"], "ULINT");
    }
    let first: u64 = vars[0]["body"]["variables"][0]["value"]
        .as_str()
        .unwrap()
        .parse()
        .unwrap();
    let second: u64 = vars[1]["body"]["variables"][0]["value"]
        .as_str()
        .unwrap()
        .parse()
        .unwrap();
    assert_eq!(second, first + 1, "runtime scan count must advance a cycle");
}

#[test]
fn serve_when_runtime_scope_at_entry_then_reports_zero_uptime() {
    // The container under test declares no uptime globals, so the program
    // itself cannot read the clock -- the VM tracks it regardless, which is
    // the whole point of the entry. At the entry stop no scan has started,
    // so it is 0.
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "stopOnEntry": true}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 5, "type": "request", "command": "disconnect"}),
    ]);

    let uptime = &responses(&out, "variables")[0]["body"]["variables"][1];
    assert_eq!(uptime["name"], "systemUptime");
    assert_eq!(uptime["type"], "LINT");
    assert_eq!(uptime["value"], "0");
}

#[test]
fn serve_when_runtime_scope_expanded_then_shows_system_uptime_advancing() {
    // Time must be observable from one stop to the next; how *fast* it
    // advances is the debug driver's business (issue #1397), so this only
    // asserts that it moves forward.
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
        json!({"seq": 3, "type": "request", "command": "setBreakpoints",
                   "arguments": {"source": {"path": "demo.st"},
                                 "breakpoints": [{"line": 11}]}}),
        json!({"seq": 4, "type": "request", "command": "configurationDone"}),
        json!({"seq": 5, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 6, "type": "request", "command": "continue",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 7, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 8, "type": "request", "command": "disconnect"}),
    ]);

    let vars = responses(&out, "variables");
    let uptime_of = |scope: &Value| -> i64 {
        let entry = &scope["body"]["variables"][1];
        assert_eq!(entry["name"], "systemUptime");
        assert_eq!(entry["type"], "LINT");
        entry["value"].as_str().unwrap().parse().unwrap()
    };
    assert!(
        uptime_of(vars[1]) > uptime_of(vars[0]),
        "runtime uptime must advance between stops"
    );
}

/// Drives the incrementing container on a cyclic task at `interval_us`,
/// with a breakpoint on `line` and `body` between `configurationDone` and
/// `disconnect`.
///
/// The container's scan maps offset 0 to line 10 and its `RET_VOID` to line
/// 11, so a breakpoint on line 11 puts each stop one whole scan after the
/// last, and one on line 10 stops before the cycle's work.
fn timed_session(interval_us: u64, line: i64, body: &[Value]) -> Vec<Value> {
    let (_file, path) = incrementing_scan_container_with_task(TaskEntry {
        task_type: TaskType::Cyclic,
        interval_us,
        ..a_task(TaskId::new(0))
    });
    let mut msgs = vec![
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
        json!({"seq": 3, "type": "request", "command": "setBreakpoints",
                   "arguments": {"source": {"path": "demo.st"},
                                 "breakpoints": [{"line": line}]}}),
        json!({"seq": 4, "type": "request", "command": "configurationDone"}),
    ];
    msgs.extend(body.iter().cloned());
    msgs.push(json!({"seq": 90, "type": "request", "command": "disconnect"}));
    run_server(&msgs)
}

/// A `continue` request, then a read of the Runtime scope at the next stop.
fn resume_and_read_uptime(seq: i64) -> [Value; 2] {
    [
        json!({"seq": seq, "type": "request", "command": "continue",
                   "arguments": {"threadId": 1}}),
        json!({"seq": seq + 1, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
    ]
}

/// The `systemUptime` value, in milliseconds, from each `variables`
/// response in order.
fn uptimes(out: &[Value]) -> Vec<i64> {
    responses(out, "variables")
        .iter()
        .map(|scope| {
            let entry = &scope["body"]["variables"][1];
            assert_eq!(entry["name"], "systemUptime");
            entry["value"].as_str().unwrap().parse().unwrap()
        })
        .collect()
}

#[test]
fn serve_when_task_declares_interval_then_uptime_advances_by_that_interval() {
    // Issue #1397: program time advanced a flat 1 ms per scan whatever the
    // task declared, so a timer elapsed after a scan count unrelated to the
    // configuration. The rate is the task's, which is also the rate `run`
    // works from, so the two agree on when a timer elapses.
    let mut body = vec![json!({"seq": 5, "type": "request", "command": "variables",
                                   "arguments": {"variablesReference": 2}})];
    body.extend(resume_and_read_uptime(6));
    let out = timed_session(100_000, 11, &body);

    assert_eq!(
        uptimes(&out),
        vec![0, 100],
        "the first scan runs at time zero and each scan is one 100 ms cycle"
    );
}

#[test]
fn serve_when_scan_pauses_midway_then_next_scan_advances_one_interval() {
    // A round that paused mid-scan ran no cycle, so it must not move the
    // clock: otherwise a timer would depend on how many times the user
    // stopped, and the scan after a breakpoint would start late. The stop
    // is on the scan's first line and the step lands on its second, so no
    // cycle finishes between the two readings.
    let mut body = vec![
        json!({"seq": 5, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 6, "type": "request", "command": "next",
                   "arguments": {"threadId": 1}}),
    ];
    body.extend(resume_and_read_uptime(7));
    let out = timed_session(100_000, 10, &body);

    assert_eq!(
        uptimes(&out),
        vec![0, 100],
        "one cycle ran, so program time moved one interval; the breakpoint \
             and the step in between moved it none"
    );
}

#[test]
fn serve_when_task_interval_is_zero_then_falls_back_to_the_assumed_rate() {
    // A task table that names no positive rate says nothing about how fast
    // to scan, which is the freewheeling situation under another name. A
    // frozen clock would stop every timer without saying why.
    let out = timed_session(0, 11, &resume_and_read_uptime(5));

    assert_eq!(
        uptimes(&out),
        vec![DEFAULT_FREEWHEELING_INTERVAL.as_millis() as i64]
    );
}

#[test]
fn serve_when_variables_reference_unknown_then_returns_no_variables() {
    // A handle the server never issued must not fall back to the program
    // variables, which is what the pre-Runtime-scope code did.
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "stopOnEntry": true}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 99}}),
        json!({"seq": 5, "type": "request", "command": "disconnect"}),
    ]);

    let vars = responses(&out, "variables");
    assert_eq!(vars[0]["success"], true);
    assert_eq!(vars[0]["body"]["variables"].as_array().unwrap().len(), 0);
}
