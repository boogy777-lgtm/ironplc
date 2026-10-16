//! Unit tests for the playground WASM entry points.

use super::*;

#[test]
fn compile_when_valid_source_then_returns_bytecode() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 42;
END_PROGRAM
";
    let result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    assert!(result.ok);
    assert!(result.bytecode.is_some());
    assert!(result.diagnostics.is_empty());
}

#[test]
fn compile_when_syntax_error_then_returns_diagnostics() {
    let source = "PROGRAM main INVALID END_PROGRAM";
    let result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    assert!(!result.ok);
    assert!(result.bytecode.is_none());
    assert!(!result.diagnostics.is_empty());
    assert!(!result.diagnostics[0].label.is_empty());
}

#[test]
fn compile_when_error_on_later_line_then_diagnostic_has_line_and_column() {
    // Line numbers are 1-based; the error is after the first line.
    let source = "PROGRAM main\nVAR\nEND_VAR\nINVALID\nEND_PROGRAM";
    let result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    assert!(!result.ok);
    assert!(!result.diagnostics.is_empty());
    let diag = &result.diagnostics[0];
    assert!(
        diag.start_line >= 2,
        "expected error on line 2 or later, got {}",
        diag.start_line
    );
    assert!(diag.start_column >= 1, "expected 1-based column");
    assert!(diag.end_line >= diag.start_line);
}

#[test]
fn run_when_valid_bytecode_then_returns_variables() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 42;
END_PROGRAM
";
    let compile_result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    let bytecode = compile_result.bytecode.unwrap();

    let result: RunResult = serde_json::from_str(&run(&bytecode, 1)).unwrap();
    assert!(result.ok);
    assert_eq!(result.scans_completed, 1);
    assert!(!result.variables.is_empty());
    assert_eq!(result.variables[2].value, "42"); // indices 0-1 are system globals
}

#[test]
fn run_when_program_traps_then_error_has_v_code() {
    // Divide-by-zero traps with v-code V4001. The structured code must
    // survive the WASM boundary in the error object's `code` member, not be
    // flattened into the message.
    let source = "
PROGRAM main
  VAR
    x : DINT;
    y : DINT;
  END_VAR
  y := 0;
  x := 1 / y;
END_PROGRAM
";
    let compile_result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    let bytecode = compile_result.bytecode.unwrap();

    let json = run(&bytecode, 1);
    let result: RunResult = serde_json::from_str(&json).unwrap();
    assert!(!result.ok);
    let error = result.error.unwrap();
    assert_eq!(error.code.as_deref(), Some("V4001"));
    // The JSON payload carries the code as a member of the error object.
    assert!(json.contains("\"code\":\"V4001\""));
}

#[test]
fn run_when_invalid_base64_then_error_is_internal_with_location() {
    let json = run("not-valid-base64!!!", 1);
    let result: RunResult = serde_json::from_str(&json).unwrap();
    assert!(!result.ok);
    let error = result.error.unwrap();
    // Host illegal states share the internal-error code and are told apart
    // by the recorded call-site location, not by a bespoke per-error code.
    assert_eq!(error.code.as_deref(), Some("P9998"));
    assert!(error.compiler_file.ends_with("lib.rs"));
    assert!(error.compiler_line > 0);
    assert!(json.contains("\"code\":\"P9998\""));
}

#[test]
fn run_when_invalid_container_then_error_is_internal() {
    let bytes = BASE64.encode(b"not a container");
    let result: RunResult = serde_json::from_str(&run(&bytes, 1)).unwrap();
    assert!(!result.ok);
    let error = result.error.unwrap();
    assert_eq!(error.code.as_deref(), Some("P9998"));
    assert!(error.compiler_line > 0);
}

#[test]
fn run_source_when_steel_thread_then_returns_values() {
    let source = "
PROGRAM main
  VAR
    x : INT;
    y : INT;
  END_VAR
  x := 10;
  y := x + 32;
END_PROGRAM
";
    let result: RunSourceResult = serde_json::from_str(&run_source(source, 1, "", "", "")).unwrap();
    assert!(result.ok);
    assert!(result.diagnostics.is_empty());
    assert!(result.error.is_none());
    assert_eq!(result.scans_completed, 1);
    assert!(result.variables.len() >= 2);
    assert_eq!(result.variables[2].value, "10"); // indices 0-1 are system globals
    assert_eq!(result.variables[3].value, "42"); // indices 0-1 are system globals
}

#[test]
fn run_source_when_syntax_error_then_returns_diagnostics() {
    let source = "PROGRAM main INVALID END_PROGRAM";
    let result: RunSourceResult = serde_json::from_str(&run_source(source, 1, "", "", "")).unwrap();
    assert!(!result.ok);
    assert!(!result.diagnostics.is_empty());
    assert_eq!(result.scans_completed, 0);
}

#[test]
fn run_source_when_multiple_scans_then_correct_count() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 99;
END_PROGRAM
";
    let result: RunSourceResult = serde_json::from_str(&run_source(source, 5, "", "", "")).unwrap();
    assert!(result.ok);
    assert_eq!(result.scans_completed, 5);
    assert_eq!(result.variables[2].value, "99"); // indices 0-1 are system globals
}

#[test]
fn compile_when_valid_source_then_bytecode_is_valid_base64() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 1;
END_PROGRAM
";
    let result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    let bytecode = result.bytecode.unwrap();
    let decoded = BASE64.decode(&bytecode);
    assert!(decoded.is_ok());
    assert!(!decoded.unwrap().is_empty());
}

#[test]
fn run_when_zero_scans_then_returns_zero_variables() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 42;
END_PROGRAM
";
    let compile_result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    let bytecode = compile_result.bytecode.unwrap();

    let result: RunResult = serde_json::from_str(&run(&bytecode, 0)).unwrap();
    assert!(result.ok);
    assert_eq!(result.scans_completed, 0);
}

// --- Stepping tests ---

#[test]
fn load_program_when_valid_source_then_creates_session() {
    reset_session();
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 42;
END_PROGRAM
";
    let result: StepResult =
        serde_json::from_str(&load_program(source, 100_000, "", "", "")).unwrap();
    assert!(result.ok);
    assert_eq!(result.total_scans, 0);
    assert!(result.diagnostics.is_empty());
    assert!(result.error.is_none());
}

#[test]
fn load_program_when_syntax_error_then_returns_diagnostics() {
    reset_session();
    let source = "PROGRAM main INVALID END_PROGRAM";
    let result: StepResult =
        serde_json::from_str(&load_program(source, 100_000, "", "", "")).unwrap();
    assert!(!result.ok);
    assert!(!result.diagnostics.is_empty());
}

#[test]
fn step_when_no_session_then_returns_error() {
    reset_session();
    let result: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert!(!result.ok);
    let error = result.error.unwrap();
    assert!(error.message.contains("No program loaded"));
    assert_eq!(error.code.as_deref(), Some("P9998"));
    assert!(error.compiler_line > 0);
}

#[test]
fn step_when_session_loaded_then_returns_variables() {
    reset_session();
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 42;
END_PROGRAM
";
    load_program(source, 100_000, "", "", "");
    let result: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert!(result.ok);
    assert_eq!(result.total_scans, 1);
    assert!(!result.variables.is_empty());
    assert_eq!(result.variables[2].value, "42"); // indices 0-1 are system globals
}

#[test]
fn step_when_called_twice_then_variables_persist() {
    reset_session();
    let source = "
PROGRAM main
  VAR
    count : DINT;
  END_VAR
  count := count + 1;
END_PROGRAM
";
    load_program(source, 100_000, "", "", "");

    let r1: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert!(r1.ok);
    assert_eq!(r1.variables[2].value, "1"); // indices 0-1 are system globals

    let r2: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert!(r2.ok);
    assert_eq!(r2.variables[2].value, "2"); // indices 0-1 are system globals
}

#[test]
fn step_when_called_twice_then_total_scans_accumulate() {
    reset_session();
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 1;
END_PROGRAM
";
    load_program(source, 100_000, "", "", "");

    let r1: StepResult = serde_json::from_str(&step(3)).unwrap();
    assert_eq!(r1.total_scans, 3);

    let r2: StepResult = serde_json::from_str(&step(2)).unwrap();
    assert_eq!(r2.total_scans, 5);
}

#[test]
fn step_when_session_faulted_then_returns_error() {
    reset_session();
    let source = "
PROGRAM main
  VAR
    x : DINT;
    y : DINT;
  END_VAR
  y := 0;
  x := 1 / y;
END_PROGRAM
";
    load_program(source, 100_000, "", "", "");

    // First step should fault (divide by zero)
    let r1: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert!(!r1.ok);
    assert!(r1.error.as_ref().unwrap().message.contains("VM trap"));

    // Subsequent step should report faulted session
    let r2: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert!(!r2.ok);
    let error = r2.error.unwrap();
    assert!(error.message.contains("faulted"));
    assert_eq!(error.code.as_deref(), Some("P9998"));
}

#[test]
fn reset_session_when_session_exists_then_clears_it() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 1;
END_PROGRAM
";
    load_program(source, 100_000, "", "", "");
    step(1);

    reset_session();

    // After reset, step should fail with no session
    let result: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert!(!result.ok);
    assert!(result.error.unwrap().message.contains("No program loaded"));
}

#[test]
fn compile_when_valid_xml_then_returns_bytecode() {
    let source = r#"<?xml version="1.0" encoding="utf-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
  <types>
    <dataTypes/>
    <pous>
      <pou name="main" pouType="program">
        <interface>
          <localVars>
            <variable name="bSwitch">
              <type><BOOL/></type>
            </variable>
          </localVars>
        </interface>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">
bSwitch := TRUE;
            </xhtml>
          </ST>
        </body>
      </pou>
    </pous>
  </types>
</project>"#;
    let result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    assert!(
        result.ok,
        "Expected ok but got diagnostics: {:?}",
        result.diagnostics
    );
    assert!(result.bytecode.is_some());
}

#[test]
fn compile_when_twincat_xml_then_returns_bytecode() {
    let source = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="main" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[PROGRAM main
VAR
    x : DINT;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[x := 42;]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>"#;
    let result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    assert!(
        result.ok,
        "Expected ok but got diagnostics: {:?}",
        result.diagnostics
    );
    assert!(result.bytecode.is_some());
}

#[test]
fn compile_when_malformed_xml_then_returns_diagnostics() {
    let source = "<?xml version=\"1.0\"?><project><invalid";
    let result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    assert!(!result.ok);
    assert!(!result.diagnostics.is_empty());
}

#[test]
fn load_program_when_called_twice_then_replaces_session() {
    reset_session();
    let source_a = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 10;
END_PROGRAM
";
    load_program(source_a, 100_000, "", "", "");
    let r1: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert_eq!(r1.variables[2].value, "10"); // indices 0-1 are system globals

    let source_b = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 20;
END_PROGRAM
";
    load_program(source_b, 100_000, "", "", "");
    let r2: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert_eq!(r2.variables[2].value, "20"); // indices 0-1 are system globals
    assert_eq!(r2.total_scans, 1);
}

#[test]
fn step_when_variable_has_initial_value_then_persists_across_steps() {
    reset_session();
    let source = "
PROGRAM main
  VAR
    exponentially : INT := 1;
  END_VAR
  exponentially := exponentially * 2;
END_PROGRAM
";
    load_program(source, 100_000, "", "", "");

    let r1: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert!(r1.ok);
    assert_eq!(r1.total_scans, 1);
    assert_eq!(r1.variables[2].value, "2"); // 1 * 2; indices 0-1 are system globals

    let r2: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert!(r2.ok);
    assert_eq!(r2.total_scans, 2);
    assert_eq!(r2.variables[2].value, "4"); // 2 * 2; indices 0-1 are system globals

    let r3: StepResult = serde_json::from_str(&step(1)).unwrap();
    assert!(r3.ok);
    assert_eq!(r3.total_scans, 3);
    assert_eq!(r3.variables[2].value, "8"); // 4 * 2; indices 0-1 are system globals
}

#[test]
fn run_source_when_bcd_to_int_with_literal_then_returns_value() {
    let source = "
PROGRAM main
  VAR
    int_val : USINT;
  END_VAR
  int_val := BCD_TO_INT(BYTE#16#42);
END_PROGRAM
";
    let result: RunSourceResult = serde_json::from_str(&run_source(source, 1, "", "", "")).unwrap();
    assert!(result.ok, "Expected ok but got error: {:?}", result.error);
    assert_eq!(result.variables[2].value, "42"); // indices 0-1 are system globals
}

#[test]
fn run_source_when_int_to_bcd_with_literal_then_returns_value() {
    let source = "
PROGRAM main
  VAR
    bcd_val : BYTE;
  END_VAR
  bcd_val := INT_TO_BCD(USINT#42);
END_PROGRAM
";
    let result: RunSourceResult = serde_json::from_str(&run_source(source, 1, "", "", "")).unwrap();
    assert!(result.ok, "Expected ok but got error: {:?}", result.error);
    assert_eq!(result.variables[2].value, "16#42"); // indices 0-1 are system globals
}

#[test]
fn compile_when_p9999_then_diagnostic_has_compiler_file_and_line() {
    // A direct hardware-address write is not supported by codegen and
    // produces P9999. The diagnostic must carry the compiler file/line so
    // the playground can report the location without the program source.
    let source = "
PROGRAM main
  VAR
    x : BOOL;
  END_VAR
  %QX0.0 := TRUE;
END_PROGRAM
";
    let result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    assert!(!result.ok);
    let diag = result
        .diagnostics
        .iter()
        .find(|d| d.code == "P9999")
        .unwrap();
    assert!(
        diag.compiler_file.ends_with(".rs"),
        "expected a compiler .rs file, got {:?}",
        diag.compiler_file
    );
    assert!(diag.compiler_line > 0, "expected a non-zero compiler line");
}

// A semantic error failing the compile is owned by
// `ironplc_project::compile::compile_when_semantic_error_then_no_container`;
// that a failed compile becomes `ok: false` with diagnostics attached is
// this binding's own contract, asserted by
// `compile_when_syntax_error_then_returns_diagnostics` above.

#[test]
fn step_when_ton_then_q_transitions_to_true() {
    reset_session();
    // PT = T#5s = 5000 ms. With cycle_time_us = 100_000 (100ms per step),
    // Q should become TRUE after 50 steps (50 * 100ms = 5s).
    let source = "
PROGRAM main
  VAR
    myTimer : TON;
    start : BOOL := TRUE;
    done : BOOL;
    elapsed : TIME;
  END_VAR
  myTimer(IN := start, PT := T#5s, Q => done, ET => elapsed);
END_PROGRAM
";
    let load: StepResult =
        serde_json::from_str(&load_program(source, 100_000, "", "", "")).unwrap();
    assert!(
        load.ok,
        "load failed: error={:?}, diagnostics={:?}",
        load.error, load.diagnostics
    );

    // After 10 steps (1s elapsed), Q should still be FALSE
    let r1: StepResult = serde_json::from_str(&step(10)).unwrap();
    assert!(r1.ok, "step(10) failed: {:?}", r1.error);
    let done_var = r1.variables.iter().find(|v| v.name == "done").unwrap();
    assert_eq!(done_var.value, "FALSE");

    // After 50 total steps (5s elapsed), Q should be TRUE
    let r2: StepResult = serde_json::from_str(&step(41)).unwrap();
    assert!(r2.ok, "step(41) failed: {:?}", r2.error);
    let done_var = r2.variables.iter().find(|v| v.name == "done").unwrap();
    assert_eq!(done_var.value, "TRUE");

    // Verify TIME variable displays correct type name
    let elapsed_var = r2.variables.iter().find(|v| v.name == "elapsed").unwrap();
    assert_eq!(
        elapsed_var.type_name, "TIME",
        "TIME variable should display as TIME, not TIME_OF_DAY"
    );
}

#[test]
fn step_when_tof_then_q_transitions_to_false() {
    reset_session();
    // PT = T#5s = 5000 ms. With cycle_time_us = 100_000 (100ms per step),
    // Q should become FALSE after 50 steps of IN=FALSE (50 * 100ms = 5s).
    let source = "
PROGRAM main
  VAR
    myTimer : TOF;
    run : BOOL := TRUE;
    active : BOOL;
    elapsed : TIME;
  END_VAR
  myTimer(IN := run, PT := T#5s, Q => active, ET => elapsed);
END_PROGRAM
";
    let load: StepResult =
        serde_json::from_str(&load_program(source, 100_000, "", "", "")).unwrap();
    assert!(
        load.ok,
        "load failed: error={:?}, diagnostics={:?}",
        load.error, load.diagnostics
    );

    // After 10 steps with IN=TRUE, Q should be TRUE
    let r1: StepResult = serde_json::from_str(&step(10)).unwrap();
    assert!(r1.ok, "step(10) failed: {:?}", r1.error);
    let active_var = r1.variables.iter().find(|v| v.name == "active").unwrap();
    assert_eq!(active_var.value, "TRUE");
}

#[test]
fn run_source_when_string_assignment_then_value_displays() {
    let source = "
PROGRAM main
  VAR
    s : STRING := 'hello';
  END_VAR
END_PROGRAM
";
    let result: RunSourceResult = serde_json::from_str(&run_source(source, 1, "", "", "")).unwrap();
    assert!(
        result.ok,
        "Expected ok but got diagnostics: {:?}, error: {:?}",
        result.diagnostics, result.error
    );
    let s = result.variables.iter().find(|v| v.name == "s").unwrap();
    assert_eq!(s.value, "'hello'");
    assert!(s.valid, "expected s.valid == true for a real STRING value");
}

/// The playground used to render durations as `T#1.5s` while the CLI dump
/// rendered the same variable as `T#1500ms`. Both now come from the shared
/// renderer, so this asserts the playground shows the shared form.
#[test]
fn run_source_when_duration_and_date_then_shared_rendering() {
    let source = "
PROGRAM main
  VAR
    t : TIME := T#1500ms;
    d : DATE := D#2024-01-15;
    clock : TIME_OF_DAY := TOD#14:30:00;
  END_VAR
END_PROGRAM
";
    let result: RunSourceResult = serde_json::from_str(&run_source(source, 1, "", "", "")).unwrap();
    assert!(
        result.ok,
        "Expected ok but got diagnostics: {:?}, error: {:?}",
        result.diagnostics, result.error
    );
    let value = |name: &str| {
        result
            .variables
            .iter()
            .find(|v| v.name == name)
            .unwrap()
            .value
            .clone()
    };
    assert_eq!(value("t"), "T#1500ms");
    assert_eq!(value("d"), "D#2024-01-15");
    assert_eq!(value("clock"), "TOD#14:30:00");
}

// Whether a given dialect or `--allow-` flag accepts a construct is owned
// by `parser/src/tests/dialect_flags.rs` (and re-verified behaviorally
// across the whole flag set by mcp's feature_flag_conformance). What is
// playground-owned is the string plumbing: that a dialect name and a
// comma-separated allows list arriving from JS are resolved and actually
// reach the compiler. Each pair below is kept as an off/on contrast for
// exactly that reason — a single positive case would also pass if the
// strings were ignored and the baseline were simply permissive.
#[test]
fn compile_when_dialect_2013_then_accepts_ltime() {
    let source = "
PROGRAM main
  VAR
    duration : LTIME;
  END_VAR
  duration := LTIME#100ms;
END_PROGRAM
";
    let result: CompileResult =
        serde_json::from_str(&compile(source, "iec61131-3-ed3", "", "")).unwrap();
    assert!(
        result.ok,
        "Expected ok but got diagnostics: {:?}",
        result.diagnostics
    );
    assert!(result.bytecode.is_some());
}

#[test]
fn compile_when_default_dialect_then_rejects_ltime_as_type() {
    let source = "
PROGRAM main
  VAR
    duration : LTIME;
  END_VAR
  duration := LTIME#100ms;
END_PROGRAM
";
    let result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    assert!(!result.ok);
    assert!(!result.diagnostics.is_empty());
}

#[test]
fn compile_when_dialect_2013_without_allow_sizeof_then_rejects_sizeof() {
    let source = "
PROGRAM main
  VAR
    x : INT;
    s : DINT;
  END_VAR
  s := SIZEOF(x);
END_PROGRAM
";
    let result: CompileResult =
        serde_json::from_str(&compile(source, "iec61131-3-ed3", "", "")).unwrap();
    assert!(!result.ok);
    assert!(!result.diagnostics.is_empty());
}

#[test]
fn compile_when_dialect_2013_with_allow_sizeof_then_accepts_sizeof() {
    let source = "
PROGRAM main
  VAR
    x : INT;
    s : DINT;
  END_VAR
  s := SIZEOF(x);
END_PROGRAM
";
    let result: CompileResult =
        serde_json::from_str(&compile(source, "iec61131-3-ed3", "sizeof", "")).unwrap();
    assert!(
        result.ok,
        "Expected ok but got diagnostics: {:?}",
        result.diagnostics
    );
    assert!(result.bytecode.is_some());
}

#[test]
fn compile_when_allows_has_unknown_name_then_ignored() {
    let source = "
PROGRAM main
  VAR
    x : INT;
  END_VAR
  x := 1;
END_PROGRAM
";
    let result: CompileResult = serde_json::from_str(&compile(
        source,
        "iec61131-3-ed3",
        "not-a-real-flag,sizeof",
        "",
    ))
    .unwrap();
    assert!(result.ok);
}

#[test]
fn compile_when_allows_has_multiple_then_each_applied() {
    // Use Ed3 baseline + two allows. Just verify it compiles successfully
    // (the program itself doesn't exercise the flags — we're checking the
    // allows parser handles whitespace and commas).
    let source = "
PROGRAM main
  VAR
    x : INT;
  END_VAR
  x := 1;
END_PROGRAM
";
    let result: CompileResult = serde_json::from_str(&compile(
        source,
        "iec61131-3-ed3",
        " sizeof , c-style-comments ",
        "",
    ))
    .unwrap();
    assert!(result.ok, "Expected ok but got: {:?}", result.diagnostics);
}

#[test]
fn dialect_from_when_canonical_names_then_resolves() {
    assert_eq!(dialect_from("iec61131-3-ed2"), Dialect::Iec61131_3Ed2);
    assert_eq!(dialect_from("iec61131-3-ed3"), Dialect::Iec61131_3Ed3);
    assert_eq!(dialect_from("rusty"), Dialect::Rusty);
    assert_eq!(dialect_from("codesys"), Dialect::Codesys);
}

#[test]
fn dialect_from_when_empty_or_unknown_then_defaults_to_rusty() {
    assert_eq!(dialect_from(""), Dialect::Rusty);
    assert_eq!(dialect_from("not-a-dialect"), Dialect::Rusty);
    // Year-based aliases are no longer recognized.
    assert_eq!(dialect_from("2013"), Dialect::Rusty);
}

#[test]
fn dialects_when_called_then_lists_all_dialects_with_one_default() {
    let options: Vec<DialectOption> = serde_json::from_str(&dialects()).unwrap();
    assert_eq!(options.len(), Dialect::ALL.len());
    // Every listed value round-trips back to a real dialect.
    for opt in &options {
        assert!(opt.value.parse::<Dialect>().is_ok());
        assert!(!opt.label.is_empty());
    }
    // Exactly one default, and it is the compiler's default dialect.
    let defaults: Vec<&DialectOption> = options.iter().filter(|o| o.is_default).collect();
    assert_eq!(defaults.len(), 1);
    assert_eq!(defaults[0].value, Dialect::default().cli_name());
}

#[test]
fn doc_sections_when_called_then_agrees_with_docs_section_for_every_family() {
    use ironplc_dsl::diagnostic::docs_section;

    let map: std::collections::BTreeMap<String, String> =
        serde_json::from_str(&doc_sections()).unwrap();
    assert_eq!(map.len(), DOCS_SECTIONS.len());

    // The served map must resolve a code exactly as the compiler's own
    // `docs_section` does; the front end builds documentation links from it.
    for (prefix, section) in &map {
        assert_eq!(docs_section(&format!("{prefix}0001")), section);
    }
}

#[test]
fn compile_when_ed2_and_cstyle_comment_then_error_carries_help() {
    let source = "
PROGRAM main
  VAR
    x : INT;
  END_VAR
  // C-style comment
  x := 1;
END_PROGRAM
";
    let result: CompileResult =
        serde_json::from_str(&compile(source, "iec61131-3-ed2", "", "")).unwrap();
    assert!(!result.ok);
    let cstyle = result
        .diagnostics
        .iter()
        .find(|d| d.code == "P0004")
        .unwrap();
    assert!(!cstyle.help.is_empty());
}

#[test]
fn compile_when_codesys_and_cstyle_comment_then_ok() {
    let source = "
PROGRAM main
  VAR
    x : INT;
  END_VAR
  // C-style comment
  x := 1;
END_PROGRAM
";
    let result: CompileResult = serde_json::from_str(&compile(source, "codesys", "", "")).unwrap();
    assert!(result.ok, "Expected ok but got: {:?}", result.diagnostics);
}

#[test]
fn compile_when_something_is_missing_at_end_of_line_then_the_range_is_the_last_character() {
    // END_PROGRAM is missing: the span of the diagnostic is empty, after the
    // `;` of line 3 (columns 1-based). The editor gets that `;`, since there is
    // no character after it on the line to draw the marker on.
    let source = "PROGRAM main\nVAR x : INT; END_VAR\nx := 1;\n";
    let result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    assert!(!result.ok);
    let diag = &result.diagnostics[0];
    assert_eq!(
        (
            diag.start_line,
            diag.start_column,
            diag.end_line,
            diag.end_column
        ),
        (3, 7, 3, 8)
    );
}

#[test]
fn compile_when_something_is_missing_mid_line_then_the_range_is_the_next_character() {
    // The `.` has no member after it: the error is at the `;`, one character.
    let source = "PROGRAM main\nVAR x : INT; a : INT; END_VAR\nx := a.;\nEND_PROGRAM";
    let result: CompileResult = serde_json::from_str(&compile(source, "", "", "")).unwrap();
    assert!(!result.ok);
    let diag = &result.diagnostics[0];
    assert_eq!(
        (
            diag.start_line,
            diag.start_column,
            diag.end_line,
            diag.end_column
        ),
        (3, 8, 3, 9)
    );
}
