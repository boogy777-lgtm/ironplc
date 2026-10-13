//! Tests for an enumeration used as the operand of an operator that computes,
//! or as the counter of a `FOR`. See `enumerated`.

use crate::test_helpers::parse_and_resolve_types_with_context;
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::Problem;
use rstest::rstest;

/// The problem codes the rule reports for `body` inside a program that
/// declares an enumeration `light` and the integers `n` and `i`.
fn problem_codes(body: &str) -> Vec<String> {
    let program = format!(
        "
TYPE
  COL : (RED, GREEN, BLUE);
END_TYPE

PROGRAM main
VAR
  light : COL;
  inline : (X, Y);
  n : DINT;
  i : INT;
END_VAR
  {body}
END_PROGRAM"
    );
    let (library, context) = parse_and_resolve_types_with_context(&program);
    match super::apply(&library, &context, &CompilerOptions::default()) {
        Ok(()) => vec![],
        Err(diagnostics) => diagnostics.into_iter().map(|d| d.code).collect(),
    }
}

#[rstest]
#[case::sum("n := light + 1;")]
#[case::negation("n := -light;")]
#[case::function_form("n := ADD(light, 1);")]
#[case::counter("FOR light := RED TO BLUE DO n := n + 1; END_FOR;")]
#[case::counter_of_inline_type("FOR inline := X TO Y DO n := n + 1; END_FOR;")]
fn apply_when_enumeration_computed_with_then_p4049(#[case] body: &str) {
    assert_eq!(
        problem_codes(body),
        vec![Problem::OperatorOperandTypeMismatch.code().to_string()]
    );
}

#[rstest]
#[case::counter_over_enumeration_bounds("FOR i := RED TO BLUE DO n := n + 1; END_FOR;")]
#[case::counter_to_enumeration_bound("FOR i := 1 TO light DO n := n + 1; END_FOR;")]
#[case::comparison("IF light = GREEN THEN n := 1; END_IF;")]
#[case::function_form_of_comparison("IF GT(light, RED) THEN n := 1; END_IF;")]
fn apply_when_enumeration_only_held_then_ok(#[case] body: &str) {
    assert_eq!(problem_codes(body), Vec::<String>::new());
}
