//! `Type#Value`, an enumeration value written with its type, wherever a value
//! may stand.
//!
//! One grammar function reads it (`grammar/enum_value.rs`), so every position
//! produces the same node, and what follows the `#` tells it from a typed
//! literal.

mod common;

use common::{expression, statements};
use ironplc_syntax::{parse_expression, parse_statements, ParseOptions};
use rstest::rstest;

const VALUE: &str = "EnumValueRef(NameRef(E_State) # NameRef(Run))";

#[rstest]
#[case::operand_alone("E_State#Run", VALUE)]
#[case::comparison(
    "s = E_State#Run",
    "BinaryExpr(NameRef(s) = EnumValueRef(NameRef(E_State) # NameRef(Run)))"
)]
#[case::right_operand_of_an_operator_chain(
    "a OR s <> E_State#Run AND b",
    "BinaryExpr(NameRef(a) OR BinaryExpr(BinaryExpr(NameRef(s) <> EnumValueRef(NameRef(E_State) # NameRef(Run))) AND NameRef(b)))"
)]
#[case::in_parentheses(
    "(E_State#Run)",
    "ParenExpr(( EnumValueRef(NameRef(E_State) # NameRef(Run)) ))"
)]
#[case::positional_argument(
    "f(E_State#Run)",
    "CallExpr(NameRef(f) ArgList(( PositionalArg(EnumValueRef(NameRef(E_State) # NameRef(Run))) )))"
)]
#[case::named_argument(
    "f(x := E_State#Run)",
    "CallExpr(NameRef(f) ArgList(( NamedArg(NameRef(x) := EnumValueRef(NameRef(E_State) # NameRef(Run))) )))"
)]
#[case::array_index(
    "a[E_State#Run]",
    "IndexExpr(NameRef(a) [ EnumValueRef(NameRef(E_State) # NameRef(Run)) ])"
)]
fn parse_expression_when_qualified_enumeration_value_then_enum_value_node_in_every_operand_position(
    #[case] source: &str,
    #[case] expected: &str,
) {
    let options = ParseOptions::default();

    assert_eq!(expression(source, &options), expected);
    assert!(parse_expression(source, &options).is_ok());
}

#[rstest]
#[case::duration("T#5s", "DurationLiteral(")]
#[case::long_duration("LT#5s", "DurationLiteral(")]
#[case::integer("INT#5", "IntLiteral(")]
#[case::negative_integer("DINT#-5", "IntLiteral(")]
#[case::boolean("BOOL#TRUE", "BoolLiteral(")]
#[case::date("D#2024-01-20", "DateLiteral(")]
#[case::string("UTF8#'a'", "StringLiteral(")]
#[case::based_integer("16#FF", "IntLiteral(")]
fn parse_expression_when_typed_literal_then_a_literal_and_not_an_enumeration_value(
    #[case] source: &str,
    #[case] starts: &str,
) {
    let tree = expression(source, &ParseOptions::default());

    assert!(tree.starts_with(starts), "{tree}");
    assert!(!tree.contains("EnumValueRef"), "{tree}");
}

#[rstest]
#[case::duration_prefix_names_a_type("T#Run")]
#[case::date_prefix_names_a_type("D#Run")]
#[case::long_duration_prefix_names_a_type("LT#Run")]
fn parse_expression_when_literal_prefix_names_an_enumeration_then_enum_value_node(
    #[case] source: &str,
) {
    let tree = expression(source, &ParseOptions::default());

    assert!(tree.starts_with("EnumValueRef("), "{tree}");
}

#[rstest]
#[case::space_before_the_hash("E_State #Run")]
#[case::space_after_the_hash("E_State# Run")]
#[case::spaces_around_the_hash("E_State # Run")]
fn parse_expression_when_pieces_of_the_qualified_value_do_not_touch_then_error(
    #[case] source: &str,
) {
    assert!(!parse_expression(source, &ParseOptions::default()).is_ok());
}

#[rstest]
#[case::assignment(
    "s := E_State#Run;",
    "StatementList(AssignStmt(NameRef(s) AssignOp(:=) EnumValueRef(NameRef(E_State) # NameRef(Run)) ;))"
)]
#[case::comparison_in_a_condition(
    "IF s = E_State#Stop THEN b := TRUE; END_IF;",
    "StatementList(IfStmt(IF BinaryExpr(NameRef(s) = EnumValueRef(NameRef(E_State) # NameRef(Stop))) THEN StatementList(AssignStmt(NameRef(b) AssignOp(:=) BoolLiteral(TRUE) ;)) END_IF ;))"
)]
#[case::case_label(
    "CASE s OF E_State#Run: b := TRUE; END_CASE;",
    "StatementList(CaseStmt(CASE NameRef(s) OF CaseBranch(CaseLabel(EnumValueRef(NameRef(E_State) # NameRef(Run))) : StatementList(AssignStmt(NameRef(b) AssignOp(:=) BoolLiteral(TRUE) ;))) END_CASE ;))"
)]
#[case::case_label_among_names(
    "CASE s OF Idle, E_State#Run, Stop: ; END_CASE;",
    "StatementList(CaseStmt(CASE NameRef(s) OF CaseBranch(CaseLabel(NameRef(Idle)) , CaseLabel(EnumValueRef(NameRef(E_State) # NameRef(Run))) , CaseLabel(NameRef(Stop)) : StatementList(EmptyStmt(;))) END_CASE ;))"
)]
fn parse_statements_when_qualified_enumeration_value_then_the_same_node_as_in_an_expression(
    #[case] source: &str,
    #[case] expected: &str,
) {
    let options = ParseOptions::default();

    assert_eq!(statements(source, &options), expected);
    assert!(parse_statements(source, &options).is_ok());
}
