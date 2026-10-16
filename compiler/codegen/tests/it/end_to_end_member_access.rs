//! End-to-end tests for what code outside a function block may do with the
//! variables of an instance.
//!
//! The check of the program refuses a write to an output or an internal
//! variable (P4081) and any access to a `VAR_IN_OUT` (P4082), as `ironplcc
//! check` and `ironplcc compile` do, so such a program never reaches code
//! generation. A program that reads outputs and internal variables, and writes
//! inputs, runs.

use crate::common::checked::try_check_and_run;
use ironplc_parser::options::{CompilerOptions, Dialect};
use rstest::rstest;

const DECLARATIONS: &str = "
FUNCTION_BLOCK FB1
  VAR_INPUT a : DINT; END_VAR
  VAR_OUTPUT y : DINT; END_VAR
  VAR_IN_OUT io : DINT; END_VAR
  VAR localVar : DINT; END_VAR
  y := localVar + a;
END_FUNCTION_BLOCK
";

fn first_code(statements: &str) -> String {
    let source = format!(
        "{DECLARATIONS}
PROGRAM main
  VAR f : FB1; x : DINT; z : DINT; END_VAR
  {statements}
END_PROGRAM"
    );
    let options = CompilerOptions::from_dialect(Dialect::Codesys);
    try_check_and_run(&source, &options)
        .err()
        .map_or_else(|| "none".to_owned(), |diagnostic| diagnostic.code)
}

#[rstest]
#[case::internal_variable_written("f.localVar := 10; f(io := x);", "P4081")]
#[case::output_written("f.y := 10; f(io := x);", "P4081")]
#[case::output_bound_to_an_output("f(io := x, y => f.localVar);", "P4081")]
#[case::in_out_read("f(io := x); z := f.io;", "P4082")]
#[case::in_out_written("f.io := 1;", "P4082")]
fn check_when_member_is_accessed_from_a_program_then_what_codesys_refuses_is_refused(
    #[case] statements: &str,
    #[case] expected: &str,
) {
    assert_eq!(first_code(statements), expected, "{statements}");
}

/// A function block with outputs and an internal variable, and no
/// `VAR_IN_OUT`, which code generation does not support yet.
const RUNNABLE: &str = "
FUNCTION_BLOCK Counter
  VAR_INPUT inc : DINT; END_VAR
  VAR_OUTPUT total : DINT; END_VAR
  VAR hidden : DINT; END_VAR
  hidden := hidden + inc;
  total := hidden * 2;
END_FUNCTION_BLOCK
";

#[test]
fn run_when_program_writes_an_input_and_reads_an_output_and_an_internal_variable_then_values_are_read(
) {
    let source = format!(
        "{RUNNABLE}
PROGRAM main
  VAR c : Counter; read_total : DINT; read_hidden : DINT; END_VAR
  c.inc := 7;
  c();
  read_total := c.total;
  read_hidden := c.hidden;
END_PROGRAM"
    );
    let options = CompilerOptions::from_dialect(Dialect::Codesys);

    let (_container, bufs) = try_check_and_run(&source, &options).expect("the program runs");

    let values: Vec<i32> = bufs.vars.iter().map(|slot| slot.as_i32()).collect();
    assert!(
        values.contains(&14),
        "total (hidden * 2) was read: {values:?}"
    );
    assert!(
        values.iter().filter(|value| **value == 7).count() >= 2,
        "the input and the internal variable were read: {values:?}"
    );
}

#[test]
fn check_when_program_assigns_an_output_of_the_runnable_block_then_p4081() {
    let source = format!(
        "{RUNNABLE}
PROGRAM main
  VAR c : Counter; x : DINT; END_VAR
  c.total := 0;
END_PROGRAM"
    );
    let options = CompilerOptions::from_dialect(Dialect::Codesys);

    let refused = try_check_and_run(&source, &options);

    assert_eq!(refused.err().map(|d| d.code), Some("P4081".to_owned()));
}
