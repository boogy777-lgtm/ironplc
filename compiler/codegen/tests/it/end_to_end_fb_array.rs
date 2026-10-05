//! End-to-end tests for arrays of function block instances: a call of an
//! element (`fbs[i](IN := x);`), a read of its member (`fbs[i].Q`), a write of
//! its member, and the binding of its outputs (`Q => y`).
//!
//! An element is selected like any array element, so it is checked against the
//! array's bounds, and each element is an instance of its own: its state and
//! its declared defaults are not shared with its neighbours.

use ironplc_parser::options::CompilerOptions;
use ironplc_vm::error::Trap;

use crate::common::{drive_fb, parse_and_try_run, try_parse_and_compile, FbStep::*};

fn fb_arrays() -> CompilerOptions {
    CompilerOptions {
        allow_fb_instance_arrays: true,
        ..CompilerOptions::default()
    }
}

/// A block that keeps a running sum, starting at the value its declaration
/// states, so that a skipped or shared initialization shows in the result.
const ACC: &str = "
FUNCTION_BLOCK Acc
  VAR_INPUT x : DINT; END_VAR
  VAR_OUTPUT total : DINT; END_VAR
  VAR sum : DINT := 10; END_VAR
  sum := sum + x;
  total := sum;
END_FUNCTION_BLOCK
";

/// `ACC` followed by a program built from `program`, which is run with the
/// instance arrays enabled and asserts `(variable, value)` pairs after one scan.
fn assert_acc_program(program: &str, asserts: &[(usize, i32)]) {
    let source = format!("{ACC}{program}");
    crate::common::assert_run_i32_with(&source, &fb_arrays(), asserts);
}

// timers=var0, q0=var1, q1=var2, q2=var3. Each element has its own preset and
// its own state, so only the elements whose preset elapsed report true; the
// output is read through `=>` for one element and through `.Q` for another.
const TIMER_ARRAY: &str = "
PROGRAM main
  VAR
    timers : ARRAY[0..2] OF TON;
    q0 : BOOL;
    q1 : BOOL;
    q2 : BOOL;
  END_VAR
  timers[0](IN := TRUE, PT := T#1s, Q => q0);
  timers[1](IN := TRUE, PT := T#3s);
  q1 := timers[1].Q;
  timers[2](IN := FALSE, PT := T#1s, Q => q2);
END_PROGRAM
";

#[test]
fn end_to_end_fb_array_when_standard_timers_called_by_index_then_each_elapses_on_its_own_preset() {
    drive_fb(
        TIMER_ARRAY,
        &fb_arrays(),
        &[
            Run(0),
            Expect(1, 0),
            Expect(2, 0),
            Expect(3, 0),
            Run(2_000_000),
            Expect(1, 1),
            Expect(2, 0),
            Expect(3, 0),
            Run(4_000_000),
            Expect(1, 1),
            Expect(2, 1),
            Expect(3, 0),
        ],
    );
}

// timers=var0, i=var1, done=var2. Every element is called in a loop and read in
// another, with the loop variable as the index.
#[test]
fn end_to_end_fb_array_when_called_and_read_in_loops_then_every_element_runs() {
    drive_fb(
        "
PROGRAM main
  VAR
    timers : ARRAY[1..3] OF TON;
    i : DINT;
    done : DINT;
  END_VAR
  FOR i := 1 TO 3 DO
    timers[i](IN := TRUE, PT := T#1s);
  END_FOR;
  done := 0;
  FOR i := 1 TO 3 DO
    IF timers[i].Q THEN
      done := done + 1;
    END_IF;
  END_FOR;
END_PROGRAM
",
        &fb_arrays(),
        &[Run(0), Expect(2, 0), Run(2_000_000), Expect(2, 3)],
    );
}

// accs=var0, a=var1, b=var2, c=var3. Each instance starts from the value the
// block declares, keeps its own state across scans and does not see another
// element's input.
#[test]
fn end_to_end_fb_array_when_user_block_called_each_scan_then_instances_keep_independent_state() {
    let source = format!(
        "{ACC}
PROGRAM main
  VAR
    accs : ARRAY[0..2] OF Acc;
    a : DINT;
    b : DINT;
    c : DINT;
  END_VAR
  accs[0](x := 1, total => a);
  accs[1](x := 5, total => b);
  accs[2](x := 0, total => c);
END_PROGRAM
"
    );
    drive_fb(
        &source,
        &fb_arrays(),
        &[
            Run(0),
            Expect(1, 11),
            Expect(2, 15),
            Expect(3, 10),
            Run(1),
            Expect(1, 12),
            Expect(2, 20),
            Expect(3, 10),
        ],
    );
}

// accs=var0, hit=var1, other=var2, member=var3. A two-dimensional array is
// indexed with one subscript per dimension, and the call and the member read
// select the same instance.
#[test]
fn end_to_end_fb_array_when_two_dimensional_then_subscripts_select_one_instance() {
    assert_acc_program(
        "
PROGRAM main
  VAR
    grid : ARRAY[1..2, 0..1] OF Acc;
    hit : DINT;
    other : DINT;
    member : DINT;
  END_VAR
  grid[2, 1](x := 7, total => hit);
  grid[1, 0](x := 0, total => other);
  member := grid[2, 1].total;
END_PROGRAM
",
        &[(1, 17), (2, 10), (3, 17)],
    );
}

// accs=var0, i=var1, b=var2, first=var3. The index is an expression.
#[test]
fn end_to_end_fb_array_when_index_is_an_expression_then_that_element_is_called() {
    assert_acc_program(
        "
PROGRAM main
  VAR
    accs : ARRAY[0..2] OF Acc;
    i : DINT;
    b : DINT;
    first : DINT;
  END_VAR
  i := 0;
  accs[i + 1](x := 4, total => b);
  first := accs[i].total;
END_PROGRAM
",
        &[(2, 14), (3, 0)],
    );
}

// accs=var0, member=var1, untouched=var2. A member written through an element
// is the member of that instance only.
#[test]
fn end_to_end_fb_array_when_member_assigned_through_element_then_only_that_instance_changes() {
    assert_acc_program(
        "
PROGRAM main
  VAR
    accs : ARRAY[0..2] OF Acc;
    member : DINT;
    untouched : DINT;
  END_VAR
  accs[1].x := 3;
  accs[1]();
  member := accs[1].total;
  accs[2]();
  untouched := accs[2].total;
END_PROGRAM
",
        &[(1, 13), (2, 10)],
    );
}

// A member that is not a whole number reads at its own type.
#[test]
fn end_to_end_fb_array_when_member_is_real_then_element_member_reads_as_real() {
    let source = "
FUNCTION_BLOCK Half
  VAR_INPUT x : REAL; END_VAR
  VAR_OUTPUT y : REAL; END_VAR
  y := x / 2.0;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    halves : ARRAY[0..1] OF Half;
    r : REAL;
  END_VAR
  halves[1](x := 5.0);
  r := halves[1].y;
END_PROGRAM
";
    let (_container, bufs) = parse_and_try_run(source, &fb_arrays()).unwrap();
    assert_eq!(bufs.vars[1].as_f32(), 2.5);
}

// shared=var0 (an array in the data region), result=var1. An array of
// instances declared as a global is reached through VAR_EXTERNAL.
#[test]
fn end_to_end_fb_array_when_declared_global_then_external_declaration_calls_elements() {
    assert_acc_program(
        "
CONFIGURATION config
  VAR_GLOBAL
    shared : ARRAY[0..1] OF Acc;
  END_VAR
  RESOURCE resource1 ON PLC
    TASK plc_task(INTERVAL := T#100ms, PRIORITY := 1);
    PROGRAM plc_task_instance WITH plc_task : main;
  END_RESOURCE
END_CONFIGURATION

PROGRAM main
  VAR_EXTERNAL
    shared : ARRAY[0..1] OF Acc;
  END_VAR
  VAR
    result : DINT;
  END_VAR
  shared[1](x := 6, total => result);
END_PROGRAM
",
        &[(1, 16)],
    );
}

// An index outside the array must not address the neighbouring memory: the
// access traps, as for any other array.
#[test]
fn end_to_end_fb_array_when_index_past_end_at_run_time_then_traps() {
    let source = "
PROGRAM main
  VAR
    timers : ARRAY[0..2] OF TON;
    i : DINT;
  END_VAR
  i := 3;
  timers[i](IN := TRUE, PT := T#1s);
END_PROGRAM
";
    let trap = parse_and_try_run(source, &fb_arrays())
        .expect_err("expected an out-of-bounds trap")
        .trap;
    assert!(
        matches!(trap, Trap::ArrayIndexOutOfBounds { .. }),
        "got {trap:?}"
    );
}

#[test]
fn end_to_end_fb_array_when_index_before_start_at_run_time_then_traps() {
    let source = "
PROGRAM main
  VAR
    timers : ARRAY[1..2] OF TON;
    i : DINT;
    q : BOOL;
  END_VAR
  i := 0;
  q := timers[i].Q;
END_PROGRAM
";
    let trap = parse_and_try_run(source, &fb_arrays())
        .expect_err("expected an out-of-bounds trap")
        .trap;
    assert!(
        matches!(trap, Trap::ArrayIndexOutOfBounds { .. }),
        "got {trap:?}"
    );
}

#[test]
fn compile_fb_array_when_constant_index_out_of_range_then_array_index_out_of_bounds() {
    let source = "
PROGRAM main
  VAR
    timers : ARRAY[0..2] OF TON;
  END_VAR
  timers[3](IN := TRUE, PT := T#1s);
END_PROGRAM
";
    let error = try_parse_and_compile(source, &fb_arrays()).expect_err("expected a compile error");
    assert_eq!(error.code, "P2027", "{error:?}");
}

// Starting values for the elements are not supported; refusing them is what
// keeps an element from silently starting at the default instead.
#[test]
fn compile_fb_array_when_initial_values_stated_then_not_implemented() {
    let source = "
PROGRAM main
  VAR
    timers : ARRAY[0..1] OF TON := [1, 2];
  END_VAR
END_PROGRAM
";
    assert!(try_parse_and_compile(source, &fb_arrays()).is_err());
}

// A character run belongs to one instance at a place known when compiling; an
// element selected at run time has none.
#[test]
fn compile_fb_array_when_block_has_string_field_then_not_implemented() {
    let source = "
FUNCTION_BLOCK Named
  VAR_INPUT n : STRING[8]; END_VAR
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    names : ARRAY[0..1] OF Named;
  END_VAR
END_PROGRAM
";
    let error = try_parse_and_compile(source, &fb_arrays()).expect_err("expected a compile error");
    assert_eq!(error.code, "P9999", "{error:?}");
}

// An instance inside a function block's own body has no storage of its own yet
// (a single instance fails the same way), so the call is refused rather than
// compiled against another unit's variables.
#[test]
fn compile_fb_array_when_declared_inside_function_block_then_not_implemented() {
    let source = "
FUNCTION_BLOCK Holder
  VAR ts : ARRAY[0..1] OF TON; END_VAR
  ts[0](IN := TRUE, PT := T#1s);
END_FUNCTION_BLOCK

PROGRAM main
  VAR h : Holder; END_VAR
  h();
END_PROGRAM
";
    let error = try_parse_and_compile(source, &fb_arrays()).expect_err("expected a compile error");
    assert_eq!(error.code, "P9999", "{error:?}");
}

// An array type that is declared by name is not laid out as an array of
// instances; it is refused, not compiled as an array of single slots.
#[test]
fn compile_fb_array_when_declared_through_a_type_name_then_not_implemented() {
    let source = "
TYPE Timers : ARRAY[0..1] OF TON; END_TYPE

PROGRAM main
  VAR ts : Timers; END_VAR
  ts[0](IN := TRUE, PT := T#1s);
END_PROGRAM
";
    let error = try_parse_and_compile(source, &fb_arrays()).expect_err("expected a compile error");
    assert_eq!(error.code, "P9999", "{error:?}");
}
