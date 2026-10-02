//! Every expression records the source text it was written as.
//!
//! An expression is written with tokens that none of its children hold: the
//! operator of `NOT c`, the caret of `p^`, the parentheses of `(a + b)` and
//! of `MAX(a, b)`. `Located for ExprKind` can only join the spans of the
//! children, so a diagnostic labelling any of those underlined too little
//! until the parser started recording the span on `Expr` itself. See
//! https://github.com/ironplc/ironplc/issues/1662.

use super::common::*;
use dsl::core::Located;

/// Parses `FUNCTION_BLOCK fb ... a := <expression>;` and returns both the
/// source and the assigned expression, so a test can slice the source with
/// the span it is checking.
fn parse_assigned_expression(expression: &str) -> (String, Expr) {
    let source = format!(
        "FUNCTION_BLOCK fb
VAR
    a : INT;
    b : INT;
    c : BOOL;
    p : REF_TO INT;
END_VAR
a := {expression};
END_FUNCTION_BLOCK"
    );
    let options = CompilerOptions {
        allow_ref_to: true,
        allow_partial_access_syntax: true,
        ..CompilerOptions::default()
    };
    let library = parse_program(&source, &FileId::default(), &options);
    assert!(library.is_ok(), "Parse failed: {:?}", library.err());

    let value = extract_assignment_value(&library.unwrap());
    (source, value)
}

#[rstest]
#[case::variable("a")]
#[case::literal("42")]
#[case::late_bound("undeclared")]
#[case::negation("-a")]
#[case::negated_literal("-42")]
#[case::logical_negation("NOT c")]
#[case::binary("a + b")]
#[case::comparison("a < b")]
#[case::group("(a + b)")]
#[case::group_of_unary("(NOT c)")]
#[case::binary_of_group("(a + b) * 2")]
#[case::binary_of_unary("a + -b")]
#[case::dereference("p^")]
#[case::negated_dereference("-p^")]
#[case::reference("REF(a)")]
#[case::call("MAX(a, b)")]
#[case::negated_call("-MAX(a, b)")]
#[case::null("NULL")]
#[case::member("a.b")]
#[case::subscript("a[b]")]
#[case::subscript_of_two("a[b, 2]")]
#[case::member_of_subscript("a[b].c[d]")]
#[case::bit("a.3")]
#[case::partial_access("a.%X3")]
#[case::dereference_of_member("p^.b")]
#[case::dereference_of_subscript("p^[1]")]
#[case::negated_subscript("-a[b]")]
#[case::binary_of_subscripts("a[1] + b.c")]
#[case::new_operator("__NEW(INT)")]
fn parse_when_expression_then_span_covers_the_expression(#[case] expression: &str) {
    let (source, value) = parse_assigned_expression(expression);

    let span = value.span();
    assert_eq!(
        expression,
        &source[span.start..span.end],
        "span of {value:?} should cover the expression as written"
    );
}

/// The span belongs to each node, not only to the outermost one: a
/// diagnostic about an operand has to underline that operand.
#[test]
fn parse_when_unary_operand_of_binary_then_operand_span_covers_the_operator() {
    let (source, value) = parse_assigned_expression("a + NOT c");

    let binary = cast!(&value.kind, ExprKind::BinaryOp);
    let span = binary.right.span();
    assert_eq!("NOT c", &source[span.start..span.end]);
}

/// A diagnostic about a call's argument underlines the argument, not the
/// whole call.
#[test]
fn parse_when_negated_call_argument_then_argument_span_covers_the_operator() {
    let (source, value) = parse_assigned_expression("MAX(a, -b)");

    let function = cast!(&value.kind, ExprKind::Function);
    let argument = cast!(
        &function.param_assignment[1],
        ParamAssignmentKind::PositionalInput
    );
    let span = argument.expr.span();
    assert_eq!("-b", &source[span.start..span.end]);
}

/// An operand that is a variable is positioned at all the tokens it was
/// written with, so a diagnostic about it underlines the `]` and the member
/// name too.
#[test]
fn parse_when_subscripted_operand_of_binary_then_operand_span_covers_the_subscript() {
    let (source, value) = parse_assigned_expression("a[b].c + d");

    let binary = cast!(&value.kind, ExprKind::BinaryOp);
    let span = binary.left.span();
    assert_eq!("a[b].c", &source[span.start..span.end]);
}

/// The index of a partial access is positioned at its digits, not left
/// without a position.
#[test]
fn parse_when_partial_access_then_index_span_covers_the_digits() {
    let (source, value) = parse_assigned_expression("a.%W12");

    let variable = cast!(&value.kind, ExprKind::Variable);
    let symbolic = cast!(variable, Variable::Symbolic);
    let partial = cast!(symbolic, SymbolicVariableKind::PartialAccess);
    let span = partial.index.span();
    assert_eq!("12", &source[span.start..span.end]);
}

/// A variable named like a SFC keyword is positioned at its token.
#[rstest]
#[case::step("STEP")]
#[case::on("ON")]
#[case::rising_edge("R_EDGE")]
#[case::falling_edge("F_EDGE")]
fn parse_when_variable_named_like_a_keyword_then_name_span_covers_the_name(#[case] name: &str) {
    let (source, value) = parse_assigned_expression(&format!("{name}.b"));

    let span = value.span();
    assert_eq!(format!("{name}.b"), source[span.start..span.end]);
    let variable = cast!(&value.kind, ExprKind::Variable);
    let symbolic = cast!(variable, Variable::Symbolic);
    let structured = cast!(symbolic, SymbolicVariableKind::Structured);
    let record = cast!(structured.record.as_ref(), SymbolicVariableKind::Named);
    let span = record.name.span();
    assert_eq!(name, &source[span.start..span.end]);
}

/// The type a special operator takes is positioned at its keyword.
#[test]
fn parse_when_special_operator_type_is_keyword_then_type_span_covers_the_keyword() {
    let (source, value) = parse_assigned_expression("__NEW(INT)");

    let function = cast!(&value.kind, ExprKind::Function);
    let argument = cast!(
        &function.param_assignment[0],
        ParamAssignmentKind::PositionalInput
    );
    let span = argument.expr.span();
    assert_eq!("INT", &source[span.start..span.end]);
}
