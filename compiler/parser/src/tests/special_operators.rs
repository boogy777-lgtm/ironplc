//! The ST-visible CODESYS special operators (`tables/special_operators.csv`).
//!
//! These are ordinary identifiers to the lexer and are promoted to operator
//! tokens by `xform_promote_special_operators` when their whole text matches,
//! which is what keeps `__NEW_ITEM` a valid name. The internal `__*`
//! operators are out of scope.

use super::common::*;
use spec_test_macro::spec_test;

/// The single expression assigned to `result` in a one-statement program.
fn parse_assigned_expression(source: &'static str) -> Expr {
    let lib = parse_text(source);
    extract_assignment_value(&lib)
}

/// REQ-CS-parser-008: The ST-visible special operators parse into the
/// call/named-variable shapes, and a name that merely contains one stays an
/// identifier.
#[spec_test(REQ_CS_parser_008)]
fn parse_when_special_new_with_type_then_function_call_named_new() {
    let value = parse_assigned_expression(
        "
FUNCTION_BLOCK FB_Example
VAR
    obj : INT;
END_VAR
obj := __NEW(INT);
END_FUNCTION_BLOCK",
    );
    let function = cast!(&value.kind, ExprKind::Function);
    assert_eq!(function.name, Id::from("__NEW"));
    assert_eq!(function.param_assignment.len(), 1);
    let positional = cast!(
        &function.param_assignment[0],
        ParamAssignmentKind::PositionalInput
    );
    let argument = cast!(&positional.expr.kind, ExprKind::Variable);
    let name = cast!(argument, Variable::Symbolic);
    let named = cast!(name, SymbolicVariableKind::Named);
    assert_eq!(named.name, Id::from("INT"));
}

#[test]
fn parse_when_special_typeof_with_type_then_function_call_named_typeof() {
    let value = parse_assigned_expression(
        "
FUNCTION_BLOCK FB_Example
VAR
    info : DINT;
END_VAR
info := __TYPEOF(INT);
END_FUNCTION_BLOCK",
    );
    let function = cast!(&value.kind, ExprKind::Function);
    assert_eq!(function.name, Id::from("__TYPEOF"));
    assert_eq!(function.param_assignment.len(), 1);
}

#[test]
fn parse_when_special_delete_then_function_call_named_delete() {
    let value = parse_assigned_expression(
        "
FUNCTION_BLOCK FB_Example
VAR
    result : INT;
    obj : INT;
END_VAR
result := __DELETE(obj);
END_FUNCTION_BLOCK",
    );
    let function = cast!(&value.kind, ExprKind::Function);
    assert_eq!(function.name, Id::from("__DELETE"));
    assert_eq!(function.param_assignment.len(), 1);
}

#[test]
fn parse_when_special_isvalidref_then_function_call_named_isvalidref() {
    let value = parse_assigned_expression(
        "
FUNCTION_BLOCK FB_Example
VAR
    result : BOOL;
    obj : INT;
END_VAR
result := __ISVALIDREF(obj);
END_FUNCTION_BLOCK",
    );
    let function = cast!(&value.kind, ExprKind::Function);
    assert_eq!(function.name, Id::from("__ISVALIDREF"));
}

#[test]
fn parse_when_special_xadd_with_two_arguments_then_function_call() {
    let value = parse_assigned_expression(
        "
FUNCTION_BLOCK FB_Example
VAR
    result : INT;
END_VAR
result := __XADD(1, 2);
END_FUNCTION_BLOCK",
    );
    let function = cast!(&value.kind, ExprKind::Function);
    assert_eq!(function.name, Id::from("__XADD"));
    assert_eq!(function.param_assignment.len(), 2);
}

#[test]
fn parse_when_special_currenttask_then_named_variable() {
    let value = parse_assigned_expression(
        "
FUNCTION_BLOCK FB_Example
VAR
    current : INT;
END_VAR
current := __CURRENTTASK;
END_FUNCTION_BLOCK",
    );
    let variable = cast!(&value.kind, ExprKind::Variable);
    let symbolic = cast!(variable, Variable::Symbolic);
    let named = cast!(symbolic, SymbolicVariableKind::Named);
    assert_eq!(named.name, Id::from("__CURRENTTASK"));
}

#[test]
fn parse_when_scope_prefix_names_then_structured_variables() {
    // `__SYSTEM` and `__POOL` are scope prefixes, not operators: the
    // qualified name is the ordinary structured-variable chain.
    let source = "
FUNCTION_BLOCK FB_Example
VAR
    a : INT;
    b : INT;
END_VAR
a := __SYSTEM.some_global;
b := __POOL.other;
END_FUNCTION_BLOCK";
    let lib = parse_text(source);
    let prog = cast!(
        lib.elements
            .iter()
            .find(|e| matches!(e, LibraryElementKind::FunctionBlockDeclaration(_)))
            .unwrap(),
        LibraryElementKind::FunctionBlockDeclaration
    );
    let stmts = cast!(&prog.body, FunctionBlockBodyKind::Statements);
    let second = cast!(&stmts.body[1], StmtKind::Assignment);
    let variable = cast!(&second.value.kind, ExprKind::Variable);
    let structured = cast!(variable, Variable::Symbolic);
    let record = cast!(structured, SymbolicVariableKind::Structured);
    assert_eq!(record.field, Id::from("other"));
    let record_record = cast!(record.record.as_ref(), SymbolicVariableKind::Named);
    assert_eq!(record_record.name, Id::from("__POOL"));
}

#[test]
fn parse_when_identifier_merely_contains_operator_name_then_left_alone() {
    // The promotion matches the whole text: a longer name is not split, so
    // these stay ordinary variables.
    let lib = parse_text(
        "
PROGRAM main
VAR
    __NEW_ITEM : INT;
    __XADD2 : INT;
END_VAR
END_PROGRAM",
    );
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(
        prog.variables[0].identifier.symbolic_id(),
        Some(&Id::from("__NEW_ITEM"))
    );
    assert_eq!(
        prog.variables[1].identifier.symbolic_id(),
        Some(&Id::from("__XADD2"))
    );
}

#[test]
fn parse_when_special_operator_lower_case_then_same_ast_as_upper_case() {
    let upper = "
FUNCTION_BLOCK FB_Example
VAR
    result : INT;
END_VAR
result := __XADD(1, 2);
END_FUNCTION_BLOCK";
    let lower = "
FUNCTION_BLOCK FB_Example
VAR
    result : INT;
END_VAR
result := __xadd(1, 2);
END_FUNCTION_BLOCK";
    let options = CompilerOptions::default();
    let upper_lib = parse_program(upper, &FileId::default(), &options).unwrap();
    let lower_lib = parse_program(lower, &FileId::default(), &options).unwrap();
    assert_eq!(upper_lib, lower_lib);
}
