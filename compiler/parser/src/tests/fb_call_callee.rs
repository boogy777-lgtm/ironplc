//! The callee of a function block call is the instance as a variable: a plain
//! name, or an element of an array of instances.

use super::common::*;

/// The statements of the first program element.
fn statements(library: &Library) -> &[StmtKind] {
    let prog = cast!(&library.elements[0], LibraryElementKind::ProgramDeclaration);
    let body = cast!(&prog.body, FunctionBlockBodyKind::Statements);
    &body.body
}

fn parse(source: &str) -> Library {
    let result = parse_program(source, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_ok(), "Parse failed: {:?}", result.err());
    result.unwrap()
}

/// The callee of the first statement, which must be a call.
fn callee(library: &Library) -> &SymbolicVariableKind {
    let call = cast!(&statements(library)[0], StmtKind::FbCall);
    cast!(&call.callee, Variable::Symbolic)
}

#[test]
fn parse_program_when_plain_instance_call_then_callee_is_the_named_variable() {
    let library = parse("PROGRAM main VAR t : TON; END_VAR t(IN := TRUE); END_PROGRAM");

    let named = cast!(callee(&library), SymbolicVariableKind::Named);
    assert_eq!(named.name.to_string(), "t");
}

#[test]
fn parse_program_when_array_element_call_then_callee_is_the_subscripted_variable() {
    let library = parse(
        "PROGRAM main VAR ts : ARRAY[0..2] OF TON; END_VAR ts[1](IN := TRUE, Q => q); END_PROGRAM",
    );

    let element = cast!(callee(&library), SymbolicVariableKind::Array);
    assert_eq!(element.subscripts.len(), 1);
    cast!(
        element.subscripted_variable.as_ref(),
        SymbolicVariableKind::Named
    );
    let call = cast!(&statements(&library)[0], StmtKind::FbCall);
    assert_eq!(call.params.len(), 2);
}

#[test]
fn parse_program_when_two_dimensional_element_call_then_one_array_variable_with_both_subscripts() {
    let library =
        parse("PROGRAM main VAR g : ARRAY[0..2, 0..2] OF TON; END_VAR g[1, i + 1](); END_PROGRAM");

    let element = cast!(callee(&library), SymbolicVariableKind::Array);
    assert_eq!(element.subscripts.len(), 2);
}

#[test]
fn parse_program_when_member_of_element_called_then_method_call_syntax_is_not_an_instance_callee() {
    let result = parse_program(
        "PROGRAM main VAR ts : ARRAY[0..2] OF TON; END_VAR ts[1].fb(); END_PROGRAM",
        &FileId::default(),
        &CompilerOptions::default(),
    );
    assert!(result.is_err());
}
