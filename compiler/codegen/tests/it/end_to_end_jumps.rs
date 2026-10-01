//! End-to-end tests for the CODESYS jump statements: `JMP` with `label:`
//! statement labels, the `CALC` conditional call and `__WAIT`.
//!
//! See `specs/design/codesys-jump-and-pragmas.md`.

use ironplc_parser::options::{CompilerOptions, Dialect};
use spec_test_macro::spec_test;

use crate::common::{drive_fb, parse_and_run, FbStep};

fn codesys() -> CompilerOptions {
    CompilerOptions::from_dialect(Dialect::Codesys)
}

/// REQ-JMP-codegen-001: a backward jump runs the statements between the label
/// and the jump again, so the body doubles as a hand-written loop
/// (`i` reaches 5).
#[spec_test(REQ_JMP_codegen_001)]
#[test]
fn end_to_end_when_backward_jump_then_loops_until_condition_holds() {
    let source = "
PROGRAM main
  VAR
    i : DINT;
  END_VAR
  loop:
  i := i + 1;
  IF i < 5 THEN
    JMP loop;
  END_IF;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 5, "i");
}

/// REQ-JMP-codegen-001: a forward jump skips the statements between it and its
/// label (`x := 2` never runs).
#[spec_test(REQ_JMP_codegen_001)]
#[test]
fn end_to_end_when_forward_jump_then_skips_statements() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 1;
  JMP skip;
  x := 2;
  skip:
  x := x + 10;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 11, "x");
}

/// REQ-JMP-codegen-001: a jump nested in a loop body leaves the loop, and the
/// statements it skipped do not run (`i` counts 1 and 2 only, `sum` is 3).
#[spec_test(REQ_JMP_codegen_001)]
#[test]
fn end_to_end_when_jump_out_of_loop_then_loop_ends() {
    let source = "
PROGRAM main
  VAR
    i : DINT;
    sum : DINT;
  END_VAR
  FOR i := 1 TO 10 DO
    IF i > 2 THEN
      JMP done;
    END_IF;
    sum := sum + i;
  END_FOR;
  done:
  sum := sum + 100;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[1].as_i32(), 103, "sum");
}

/// REQ-JMP-codegen-001: `JMP (condition) label` jumps only when the condition
/// is TRUE; with a FALSE condition execution falls through.
#[spec_test(REQ_JMP_codegen_001)]
#[test]
fn end_to_end_when_conditional_jump_is_false_then_falls_through() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    go : BOOL;
  END_VAR
  x := 1;
  JMP (go) skip;
  x := 2;
  skip:
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 2, "x");
}

/// REQ-JMP-codegen-001: the same statement with the condition TRUE jumps.
#[spec_test(REQ_JMP_codegen_001)]
#[test]
fn end_to_end_when_conditional_jump_is_true_then_jumps() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    go : BOOL;
  END_VAR
  x := 1;
  go := TRUE;
  JMP (go) skip;
  x := 2;
  skip:
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 1, "x");
}

/// REQ-JMP-codegen-001: a label that no jump reaches is a no-op marker.
#[spec_test(REQ_JMP_codegen_001)]
#[test]
fn end_to_end_when_label_unused_then_program_still_runs() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 1;
  unused:
  x := x + 1;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 2, "x");
}

/// REQ-JMP-codegen-001: a jump to a name no label defines is refused with
/// P0027 rather than emitting a jump to nowhere.
#[spec_test(REQ_JMP_codegen_001)]
#[test]
fn end_to_end_when_jump_target_missing_then_problem() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 1;
  JMP nowhere;
END_PROGRAM
";
    let err = crate::common::try_parse_and_compile(source, &codesys())
        .expect_err("an undefined label must be reported");

    assert_eq!(err.code, "P0027");
}

/// REQ-JMP-codegen-002: `CALC(condition, call)` runs the call only when the
/// condition is TRUE. The `RS` block latches `Q1` when the call runs and
/// leaves it alone otherwise.
#[spec_test(REQ_JMP_codegen_002)]
#[test]
fn end_to_end_when_calc_condition_false_then_call_skipped() {
    let source = "
PROGRAM main
  VAR
    latch : RS;
    set_in : BOOL;
    run : BOOL;
    result : BOOL;
  END_VAR
  CALC(run, latch(S := set_in, Q1 => result));
END_PROGRAM
";
    drive_fb(
        source,
        &codesys(),
        &[
            // `set_in` is TRUE but the call is skipped, so `Q1` stays FALSE.
            FbStep::Write(1, 1),
            FbStep::Run(0),
            FbStep::Expect(3, 0),
            // The same inputs with `run` TRUE: the call runs and latches.
            FbStep::Write(2, 1),
            FbStep::Run(1),
            FbStep::Expect(3, 1),
        ],
    );
}

/// REQ-JMP-codegen-003: `__WAIT(condition)` goes on as soon as the condition
/// holds, so a satisfied condition falls straight through.
#[spec_test(REQ_JMP_codegen_003)]
#[test]
fn end_to_end_when_wait_condition_true_then_continues() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    ready : BOOL;
  END_VAR
  ready := TRUE;
  __WAIT(ready);
  x := 1;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 1, "x");
}

/// REQ-JMP-codegen-003: the bare `__WAIT;` form has no condition this VM can
/// wait for, so it is refused as not implemented (P9999).
#[spec_test(REQ_JMP_codegen_003)]
#[test]
fn end_to_end_when_wait_without_condition_then_not_implemented() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  __WAIT;
  x := 1;
END_PROGRAM
";
    let err = crate::common::try_parse_and_compile(source, &codesys())
        .expect_err("a condition-less wait has no lowering");

    assert_eq!(err.code, "P9999");
}
