//! Tests for statement and scan stepping.

use super::*;

/// A scan of four single-byte-operand statements at the same call depth,
/// then `RET_VOID`, needing no constant pool: `LOAD_VAR var[0]; STORE_VAR
/// var[0]; LOAD_VAR var[0]; STORE_VAR var[0]; RET_VOID`. Statement starts
/// land at offsets 0, 3, 6, 9, 12 — so a step advances the paused pc by one
/// statement each time.
const MULTI_STATEMENT_SCAN: [u8; 13] = [
    0x0C, 0x00, 0x00, // LOAD_VAR_I32  var[0]  (offset 0)
    0x10, 0x00, 0x00, // STORE_VAR_I32 var[0]  (offset 3)
    0x0C, 0x00, 0x00, // LOAD_VAR_I32  var[0]  (offset 6)
    0x10, 0x00, 0x00, // STORE_VAR_I32 var[0]  (offset 9)
    0x8C, // RET_VOID                          (offset 12)
];

/// Line map for [`MULTI_STATEMENT_SCAN`]: one source line per statement,
/// lines 10–14 of `demo.st`.
const MULTI_STATEMENT_LINES: [(u16, u16); 5] = [(0, 10), (3, 11), (6, 12), (9, 13), (12, 14)];

#[test]
fn serve_when_step_over_then_advances_paused_pc_statement_by_statement() {
    // From a breakpoint on the first statement, each `next` lands on the
    // next statement (no CALL to step over, so step-over lands on the
    // immediately-following instruction). The paused source line is read
    // back via `stackTrace`.
    let (_file, path) = scan_container_file(&MULTI_STATEMENT_SCAN, 1, &MULTI_STATEMENT_LINES);
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path}}),
        json!({"seq": 3, "type": "request", "command": "setBreakpoints",
                   "arguments": {"source": {"path": "demo.st"},
                                 "breakpoints": [{"line": 10}]}}),
        json!({"seq": 4, "type": "request", "command": "configurationDone"}),
        json!({"seq": 5, "type": "request", "command": "stackTrace",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 6, "type": "request", "command": "next",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 7, "type": "request", "command": "stackTrace",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 8, "type": "request", "command": "next",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 9, "type": "request", "command": "stackTrace",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 10, "type": "request", "command": "disconnect"}),
    ]);

    // Breakpoint stop, then a step stop for each `next`.
    let stopped = events(&out, "stopped");
    assert_eq!(stopped.len(), 3);
    assert_eq!(stopped[0]["body"]["reason"], "breakpoint");
    assert_eq!(stopped[1]["body"]["reason"], "step");
    assert_eq!(stopped[2]["body"]["reason"], "step");

    // The paused line advances one statement per step: 10 → 11 → 12.
    let st = responses(&out, "stackTrace");
    assert_eq!(st[0]["body"]["stackFrames"][0]["line"], 10);
    assert_eq!(st[1]["body"]["stackFrames"][0]["line"], 11);
    assert_eq!(st[2]["body"]["stackFrames"][0]["line"], 12);

    for n in responses(&out, "next") {
        assert_eq!(n["success"], true);
    }
}

#[test]
fn serve_when_step_in_and_step_out_then_wired_and_resume() {
    // Cover the `stepIn` and `stepOut` handlers. With no callee: `stepIn`
    // lands on the next instruction; `stepOut` from the sole (entry) frame
    // has no shallower frame to reach, so it runs the scan to completion.
    // `scanLimit: 1` bounds the run so completing that scan terminates the
    // session (rather than continuing into the next scan).
    let (_file, path) = scan_container_file(&MULTI_STATEMENT_SCAN, 1, &MULTI_STATEMENT_LINES);
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "scanLimit": 1}}),
        json!({"seq": 3, "type": "request", "command": "setBreakpoints",
                   "arguments": {"source": {"path": "demo.st"},
                                 "breakpoints": [{"line": 10}]}}),
        json!({"seq": 4, "type": "request", "command": "configurationDone"}),
        json!({"seq": 5, "type": "request", "command": "stepIn",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 6, "type": "request", "command": "stackTrace",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 7, "type": "request", "command": "stepOut",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 8, "type": "request", "command": "disconnect"}),
    ]);

    // Breakpoint stop, then a step stop from `stepIn` landing on the
    // second statement (line 11).
    let stopped = events(&out, "stopped");
    assert_eq!(stopped.len(), 2);
    assert_eq!(stopped[1]["body"]["reason"], "step");
    assert_eq!(
        responses(&out, "stackTrace")[0]["body"]["stackFrames"][0]["line"],
        11
    );

    // stepOut from the top frame runs the scan to completion.
    assert_eq!(events(&out, "terminated").len(), 1);
    assert_eq!(responses(&out, "stepIn")[0]["success"], true);
    assert_eq!(responses(&out, "stepOut")[0]["success"], true);
}

// -- scan stepping (`ironplc/stepScan`) ---------------------------------

#[test]
fn serve_when_step_scan_then_stops_at_start_of_next_scan_with_cycle_complete() {
    // The whole point of the command: one press runs the rest of the
    // current cycle and stops at the top of the next, with the finished
    // cycle's values on screen.
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "stopOnEntry": true}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "ironplc/stepScan",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 5, "type": "request", "command": "stackTrace",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 6, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 1}}),
        json!({"seq": 7, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 8, "type": "request", "command": "disconnect"}),
    ]);

    assert_eq!(responses(&out, "ironplc/stepScan")[0]["success"], true);

    // The entry stop, then the scan step's landing -- reported as a step.
    let stopped = events(&out, "stopped");
    assert_eq!(stopped.len(), 2);
    assert_eq!(stopped[0]["body"]["reason"], "entry");
    assert_eq!(stopped[1]["body"]["reason"], "step");
    assert!(events(&out, "terminated").is_empty());

    // The stop has a live frame at the first statement of the new scan --
    // not the frame-less scan boundary, where a client would show no call
    // stack and no variables.
    let st = responses(&out, "stackTrace");
    assert_eq!(st[0]["body"]["stackFrames"].as_array().unwrap().len(), 1);
    assert_eq!(st[0]["body"]["stackFrames"][0]["name"], "MAIN");
    assert_eq!(st[0]["body"]["stackFrames"][0]["line"], 10);

    // One full cycle ran: the increment landed and the count advanced.
    let vars = responses(&out, "variables");
    assert_eq!(vars[0]["body"]["variables"][0]["name"], "x");
    assert_eq!(vars[0]["body"]["variables"][0]["value"], "1");
    assert_eq!(vars[1]["body"]["variables"][0]["name"], "scanCount");
    assert_eq!(vars[1]["body"]["variables"][0]["value"], "1");
}

#[test]
fn serve_when_step_scan_repeatedly_then_advances_exactly_one_cycle_each_time() {
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "stopOnEntry": true}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "ironplc/stepScan",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 5, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 6, "type": "request", "command": "ironplc/stepScan",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 7, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 8, "type": "request", "command": "ironplc/stepScan",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 9, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 10, "type": "request", "command": "disconnect"}),
    ]);

    // Entry stop plus one landing per press.
    assert_eq!(events(&out, "stopped").len(), 4);
    let counts: Vec<&str> = responses(&out, "variables")
        .iter()
        .map(|v| v["body"]["variables"][0]["value"].as_str().unwrap())
        .collect();
    assert_eq!(counts, ["1", "2", "3"], "one cycle per press, never two");
}

#[test]
fn serve_when_step_scan_lands_then_a_following_step_advances_one_statement() {
    // The landing must leave a usable pause position. Stopping at the
    // frame-less scan boundary instead would seed the next step from
    // `(depth 0, offset 0)` -- the first statement -- so `next` would skip
    // it and land on the second.
    let (_file, path) = scan_container_file(&MULTI_STATEMENT_SCAN, 1, &MULTI_STATEMENT_LINES);
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "stopOnEntry": true}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "ironplc/stepScan",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 5, "type": "request", "command": "stackTrace",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 6, "type": "request", "command": "next",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 7, "type": "request", "command": "stackTrace",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 8, "type": "request", "command": "disconnect"}),
    ]);

    let st = responses(&out, "stackTrace");
    assert_eq!(st[0]["body"]["stackFrames"][0]["line"], 10);
    assert_eq!(st[1]["body"]["stackFrames"][0]["line"], 11);
}

#[test]
fn serve_when_breakpoint_inside_stepped_scan_then_stops_at_the_breakpoint() {
    // A breakpoint reached while running the cycle out wins and abandons
    // the scan step, the same way one reached mid-`next` does.
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "stopOnEntry": true}}),
        // Line 11 is the RET_VOID, after the increment statement.
        json!({"seq": 3, "type": "request", "command": "setBreakpoints",
                   "arguments": {"source": {"path": "demo.st"},
                                 "breakpoints": [{"line": 11}]}}),
        json!({"seq": 4, "type": "request", "command": "configurationDone"}),
        json!({"seq": 5, "type": "request", "command": "ironplc/stepScan",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 6, "type": "request", "command": "stackTrace",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 7, "type": "request", "command": "variables",
                   "arguments": {"variablesReference": 2}}),
        json!({"seq": 8, "type": "request", "command": "disconnect"}),
    ]);

    let stopped = events(&out, "stopped");
    assert_eq!(stopped.len(), 2);
    assert_eq!(stopped[1]["body"]["reason"], "breakpoint");
    assert_eq!(
        responses(&out, "stackTrace")[0]["body"]["stackFrames"][0]["line"],
        11
    );
    // The cycle never finished, so no scan has completed.
    assert_eq!(
        responses(&out, "variables")[0]["body"]["variables"][0]["value"],
        "0"
    );
}

#[test]
fn serve_when_step_scan_reaches_scan_limit_then_terminates_instead_of_landing() {
    // With one scan left in the bound, the stepped cycle is the last one:
    // there is no next scan to land in, so the session ends.
    let (_file, path) = incrementing_scan_container_file();
    let out = run_server(&[
        json!({"seq": 1, "type": "request", "command": "initialize"}),
        json!({"seq": 2, "type": "request", "command": "launch",
                   "arguments": {"program": path, "stopOnEntry": true,
                                 "scanLimit": 1}}),
        json!({"seq": 3, "type": "request", "command": "configurationDone"}),
        json!({"seq": 4, "type": "request", "command": "ironplc/stepScan",
                   "arguments": {"threadId": 1}}),
        json!({"seq": 5, "type": "request", "command": "disconnect"}),
    ]);

    // Only the entry stop: the scan step terminated rather than landing.
    let stopped = events(&out, "stopped");
    assert_eq!(stopped.len(), 1);
    assert_eq!(stopped[0]["body"]["reason"], "entry");
    assert_eq!(events(&out, "terminated").len(), 1);
}
