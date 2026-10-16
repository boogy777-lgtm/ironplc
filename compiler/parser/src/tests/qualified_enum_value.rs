//! `Type#Value` in an expression: the AST shape, in every dialect.
//!
//! IEC 61131-3 writes `enumerated_value ::= [enumerated_type_name '#']
//! identifier` as a value wherever a value may stand, so no dialect flag gates
//! it. The value is the same `EnumeratedValue` that an initial value holds.

use super::common::*;

const TYPES: &str = "TYPE E_State : (Idle, Run, Stop); END_TYPE";

fn body_of(statements: &str) -> Vec<StmtKind> {
    let source =
        format!("{TYPES} PROGRAM main VAR s : E_State; b : BOOL; END_VAR {statements} END_PROGRAM");
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default())
        .expect("the program parses");
    let program = library
        .elements
        .iter()
        .find_map(|element| match element {
            LibraryElementKind::ProgramDeclaration(program) => Some(program),
            _ => None,
        })
        .expect("a program");
    cast!(&program.body, FunctionBlockBodyKind::Statements)
        .body
        .clone()
}

fn is_qualified(expr: &Expr, value: &str) -> bool {
    matches!(
        &expr.kind,
        ExprKind::EnumeratedValue(found)
            if found.type_name == Some(TypeName::from("E_State"))
                && found.value == Id::from(value)
    )
}

#[test]
fn parse_when_qualified_value_assigned_then_enumerated_value_with_its_type() {
    let body = body_of("s := E_State#Run;");

    let assignment = cast!(&body[0], StmtKind::Assignment);
    assert!(is_qualified(&assignment.value, "Run"));
}

#[test]
fn parse_when_qualified_value_compared_then_enumerated_value_operand() {
    let body = body_of("b := s = E_State#Stop;");

    let assignment = cast!(&body[0], StmtKind::Assignment);
    let compare = cast!(&assignment.value.kind, ExprKind::Compare);
    assert_eq!(compare.op, CompareOp::Eq);
    assert!(is_qualified(&compare.right, "Stop"));
}

#[test]
fn parse_when_qualified_value_is_an_argument_then_enumerated_value_argument() {
    let body = body_of("b := f(E_State#Run);");

    let assignment = cast!(&body[0], StmtKind::Assignment);
    let function = cast!(&assignment.value.kind, ExprKind::Function);
    let argument = cast!(
        &function.param_assignment[0],
        ParamAssignmentKind::PositionalInput
    );
    assert!(is_qualified(&argument.expr, "Run"));
}

#[test]
fn parse_when_qualified_value_is_a_case_label_then_the_value_of_the_same_type() {
    let body = body_of("CASE s OF E_State#Run: b := TRUE; Idle: b := FALSE; END_CASE;");

    let case = cast!(&body[0], StmtKind::Case);
    let selector = &case.statement_groups[0].selectors[0];
    assert!(matches!(
        selector,
        CaseSelectionKind::EnumeratedValue(value)
            if value.type_name == Some(TypeName::from("E_State"))
                && value.value == Id::from("Run")
    ));
}

#[test]
fn parse_when_duration_prefix_names_an_enumeration_then_the_value_and_not_a_duration() {
    let source =
        "TYPE T : (Run, Stop); END_TYPE PROGRAM main VAR s : T; END_VAR s := T#Run; END_PROGRAM";

    let library = parse_program(source, &FileId::default(), &CompilerOptions::default())
        .expect("the program parses");
    let program = cast!(&library.elements[1], LibraryElementKind::ProgramDeclaration);
    let body = cast!(&program.body, FunctionBlockBodyKind::Statements);
    let assignment = cast!(&body.body[0], StmtKind::Assignment);
    assert!(matches!(
        &assignment.value.kind,
        ExprKind::EnumeratedValue(value) if value.type_name == Some(TypeName::from("T"))
    ));
}

#[test]
fn parse_when_typed_duration_in_the_same_position_then_still_a_duration() {
    let body = body_of("s := E_State#Run; b := T#5s = T#5s;");

    let assignment = cast!(&body[1], StmtKind::Assignment);
    let compare = cast!(&assignment.value.kind, ExprKind::Compare);
    assert!(matches!(
        compare.left.kind,
        ExprKind::Const(ConstantKind::Duration(_))
    ));
}
