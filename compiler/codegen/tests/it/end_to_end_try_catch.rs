//! End-to-end tests for the CODESYS exception handling statements
//! (`__TRY`/`__CATCH`/`__FINALLY`/`__ENDTRY` and `__THROW`).
//!
//! A throw is lowered statically (see `compile_try_catch`): it stores its
//! value in the innermost enclosing `__CATCH (e)` variable and jumps to that
//! clause, so these tests run the whole pipeline and read the resulting
//! buffers.
//!
//! See `specs/design/codesys-try-catch.md`.

use ironplc_parser::options::{CompilerOptions, Dialect};

use crate::common::{parse_and_run, parse_and_try_run, try_parse_and_compile};
use spec_test_macro::spec_test;

fn codesys() -> CompilerOptions {
    CompilerOptions::from_dialect(Dialect::Codesys)
}

/// REQ-TC-codegen-010: a protected body that runs to completion runs the
/// finally body and skips the catch body. `x` ends at 1 + 10.
#[spec_test(REQ_TC_codegen_010)]
#[test]
fn end_to_end_when_try_completes_then_finally_runs_and_catch_does_not() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  __TRY
    x := 1;
  __CATCH (e)
    x := 100;
  __FINALLY
    x := x + 10;
  __ENDTRY;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 11);
}

/// REQ-TC-codegen-010: with no finally body a completed try leaves the body's
/// effect in place, and the catch body still does not run.
#[spec_test(REQ_TC_codegen_010)]
#[test]
fn end_to_end_when_try_completes_without_finally_then_only_body_runs() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  __TRY
    x := 5;
  __CATCH (e)
    x := 100;
  __ENDTRY;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 5);
}

/// REQ-TC-codegen-011: `__THROW(v)` stores `v` in the catch variable, skips
/// the rest of the protected body, runs the handler and then the finally
/// body. `x := 1` runs, `x := 2` is skipped, the handler copies `e` (= 7) and
/// the finally body adds 10: x = 17, e = 7.
#[spec_test(REQ_TC_codegen_011)]
#[test]
fn end_to_end_when_throw_then_handler_receives_value_and_finally_runs() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    e : DINT;
  END_VAR
  __TRY
    x := 1;
    __THROW(7);
    x := 2;
  __CATCH (e)
    x := e;
  __FINALLY
    x := x + 10;
  __ENDTRY;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 17, "x");
    assert_eq!(bufs.vars[1].as_i32(), 7, "e");
}

/// REQ-TC-codegen-011: a throw nested inside the control flow of the body
/// still reaches the handler, and only the statements it skips are skipped.
#[spec_test(REQ_TC_codegen_011)]
#[test]
fn end_to_end_when_throw_inside_if_then_handler_runs() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    e : DINT;
  END_VAR
  x := 1;
  __TRY
    IF x > 0 THEN
      __THROW(3);
    END_IF;
    x := 2;
  __CATCH (e)
    x := e + 1;
  __ENDTRY;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 4, "x");
    assert_eq!(bufs.vars[1].as_i32(), 3, "e");
}

/// REQ-TC-codegen-011: a bare `__THROW` runs the handler without storing a
/// value.
#[spec_test(REQ_TC_codegen_011)]
#[test]
fn end_to_end_when_bare_throw_then_handler_runs() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  __TRY
    x := 1;
    __THROW;
    x := 2;
  __CATCH
    x := x + 10;
  __ENDTRY;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 11, "x");
}

/// REQ-TC-codegen-011: the thrown value is narrowed to the catch variable's
/// type, exactly like an assignment to it. A SINT catch variable therefore
/// truncates 200 to -56.
#[spec_test(REQ_TC_codegen_011)]
#[test]
fn end_to_end_when_throw_value_wider_than_catch_variable_then_truncates() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    e : SINT;
  END_VAR
  __TRY
    __THROW(200);
  __CATCH (e)
    x := e;
  __ENDTRY;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), -56, "x");
}

/// REQ-TC-codegen-012: a throw inside a catch body belongs to the next
/// enclosing try: the inner handler skips its own remaining statements and
/// the outer handler runs with the new value.
#[spec_test(REQ_TC_codegen_012)]
#[test]
fn end_to_end_when_throw_inside_catch_then_outer_handler_runs() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    inner : DINT;
    outer : DINT;
  END_VAR
  __TRY
    __TRY
      __THROW(1);
    __CATCH (inner)
      __THROW(2);
      x := 100;
    __ENDTRY;
    x := 200;
  __CATCH (outer)
    x := outer;
  __ENDTRY;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 2, "x");
    assert_eq!(bufs.vars[1].as_i32(), 1, "inner");
    assert_eq!(bufs.vars[2].as_i32(), 2, "outer");
}

/// REQ-TC-codegen-012: a throw inside a finally body belongs to the next
/// enclosing try.
#[spec_test(REQ_TC_codegen_012)]
#[test]
fn end_to_end_when_throw_inside_finally_then_outer_handler_runs() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    e : DINT;
  END_VAR
  __TRY
    __TRY
      x := 1;
    __FINALLY
      __THROW(9);
    __ENDTRY;
    x := 100;
  __CATCH (e)
    x := e;
  __ENDTRY;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 9, "x");
    assert_eq!(bufs.vars[1].as_i32(), 9, "e");
}

/// REQ-TC-codegen-011: a run that never throws leaves the catch variable
/// untouched, and the try with a handler around it is a no-op.
#[spec_test(REQ_TC_codegen_011)]
#[test]
fn end_to_end_when_no_throw_then_catch_variable_unchanged() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    e : DINT := 42;
  END_VAR
  __TRY
    x := 1;
  __CATCH (e)
    x := 100;
  __ENDTRY;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 1, "x");
    assert_eq!(bufs.vars[1].as_i32(), 42, "e");
}

/// REQ-TC-codegen-001: a throw with no enclosing try that has a catch clause
/// is refused with P4066 — at program level and inside a try that only has a
/// finally body.
#[spec_test(REQ_TC_codegen_001)]
#[test]
fn end_to_end_when_throw_outside_catchable_try_then_problem() {
    let outside = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  __THROW(1);
END_PROGRAM
";
    let err = try_parse_and_compile(outside, &codesys())
        .expect_err("a throw with no handler must not compile");
    assert_eq!(err.code, "P4066");

    let finally_only = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  __TRY
    __THROW(1);
  __FINALLY
    x := 0;
  __ENDTRY;
END_PROGRAM
";
    let err = try_parse_and_compile(finally_only, &codesys())
        .expect_err("a throw in a finally-only try must not compile");
    assert_eq!(err.code, "P4066");
}

/// REQ-TC-codegen-001: the same throw inside a catchable try compiles and
/// runs (the check above would otherwise pass vacuously).
#[spec_test(REQ_TC_codegen_001)]
#[test]
fn end_to_end_when_throw_inside_catchable_try_then_runs() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  __TRY
    __THROW(1);
  __CATCH ()
    x := 2;
  __ENDTRY;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source, &codesys());

    assert_eq!(bufs.vars[0].as_i32(), 2, "x");
}

/// REQ-TC-codegen-010: a runtime trap is not an exception the handler sees —
/// the handlers only run for `__THROW` statements. This pins that a division
/// by zero still traps instead of being swallowed by the catch clause.
#[spec_test(REQ_TC_codegen_010)]
#[test]
fn end_to_end_when_trap_inside_try_then_trap_reaches_the_vm() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    zero : DINT;
  END_VAR
  __TRY
    x := 10 / zero;
  __CATCH (e)
    x := 1;
  __ENDTRY;
END_PROGRAM
";
    assert!(
        parse_and_try_run(source, &codesys()).is_err(),
        "the trap must reach the VM; it is not a catchable __THROW"
    );
}
