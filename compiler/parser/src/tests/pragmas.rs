//! CODESYS/TwinCAT pragma skipping, and the conditional-compilation
//! `{IF}` family (`--allow-pragma-if`).
//!
//! See `specs/design/codesys-jump-and-pragmas.md`.

use super::common::*;
use spec_test_macro::spec_test;

/// The assignment statements of the first program element, as
/// `(target, value)` pairs, so a test can tell which `{IF}` branch survived.
fn assignments(library: &Library) -> Vec<(String, String)> {
    let prog = cast!(&library.elements[0], LibraryElementKind::ProgramDeclaration);
    let body = cast!(&prog.body, FunctionBlockBodyKind::Statements);
    body.body
        .iter()
        .map(|stmt| match stmt {
            StmtKind::Assignment(assignment) => (
                assignment.target.to_string(),
                assignment.value.to_string(),
            ),
            other => panic!("expected an assignment, got {other:?}"),
        })
        .collect()
}

fn parse_with_pragma_if(source: &str) -> Library {
    match parse_program(source, &FileId::default(), &opts_with_pragma_if()) {
        Ok(library) => library,
        Err(e) => panic!("parse failed: {e:?}"),
    }
}

/// REQ-JMP-parser-030: only the taken branch reaches the parser. Without a
/// define, `{IF defined(X)}` is false, so the `{ELSE}` branch is the one that
/// ends up in the AST.
#[spec_test(REQ_JMP_parser_030)]
fn pragma_if_when_condition_false_then_else_branch_survives() {
    let library = parse_with_pragma_if(
        "PROGRAM main
VAR x : INT; END_VAR
{IF defined(X)}
x := 1;
{ELSE}
x := 2;
{END_IF}
END_PROGRAM",
    );

    assert_eq!(assignments(&library), [("x".to_string(), "2".to_string())]);
}

/// REQ-JMP-parser-030: `{DEFINE}` makes the name visible to the conditions
/// that follow it, so the `{IF}` branch is the one that survives.
#[spec_test(REQ_JMP_parser_030)]
fn pragma_if_when_define_holds_then_if_branch_survives() {
    let library = parse_with_pragma_if(
        "PROGRAM main
VAR x : INT; END_VAR
{DEFINE X}
{IF defined(X)}
x := 1;
{ELSE}
x := 2;
{END_IF}
END_PROGRAM",
    );

    assert_eq!(assignments(&library), [("x".to_string(), "1".to_string())]);
}

/// REQ-JMP-parser-030: `{UNDEFINE}` removes a define again, and names match
/// case-insensitively.
#[spec_test(REQ_JMP_parser_030)]
fn pragma_if_when_undefine_then_condition_false_again() {
    let library = parse_with_pragma_if(
        "PROGRAM main
VAR x : INT; END_VAR
{DEFINE X}
{UNDEFINE x}
{IF defined(X)}
x := 1;
{ELSE}
x := 2;
{END_IF}
END_PROGRAM",
    );

    assert_eq!(assignments(&library), [("x".to_string(), "2".to_string())]);
}

/// REQ-JMP-parser-030: `{ELSIF}` is evaluated only while no earlier branch
/// was taken.
#[spec_test(REQ_JMP_parser_030)]
fn pragma_if_when_elsif_holds_then_elsif_branch_survives() {
    let library = parse_with_pragma_if(
        "PROGRAM main
VAR x : INT; END_VAR
{DEFINE B}
{IF defined(A)}
x := 1;
{ELSIF defined(B)}
x := 2;
{ELSE}
x := 3;
{END_IF}
END_PROGRAM",
    );

    assert_eq!(assignments(&library), [("x".to_string(), "2".to_string())]);
}

/// REQ-JMP-parser-030: the first condition that holds wins, even when a later
/// one would hold too.
#[spec_test(REQ_JMP_parser_030)]
fn pragma_if_when_two_conditions_hold_then_first_branch_survives() {
    let library = parse_with_pragma_if(
        "PROGRAM main
VAR x : INT; END_VAR
{DEFINE B}
{IF defined(A) OR defined(B)}
x := 1;
{ELSIF defined(B)}
x := 2;
{END_IF}
END_PROGRAM",
    );

    assert_eq!(assignments(&library), [("x".to_string(), "1".to_string())]);
}

/// REQ-JMP-parser-030: `NOT`, parentheses and `AND` combine operands; a
/// condition may use `TRUE`/`FALSE` literals.
#[spec_test(REQ_JMP_parser_030)]
fn pragma_if_when_compound_condition_then_evaluated() {
    let library = parse_with_pragma_if(
        "PROGRAM main
VAR x : INT; END_VAR
{DEFINE A}
{IF NOT defined(B) AND (defined(A) OR FALSE)}
x := 1;
{ELSE}
x := 2;
{END_IF}
END_PROGRAM",
    );

    assert_eq!(assignments(&library), [("x".to_string(), "1".to_string())]);
}

/// REQ-JMP-parser-030: pragmas nest, and an inner `{IF}` inside a taken outer
/// branch is still evaluated.
#[spec_test(REQ_JMP_parser_030)]
fn pragma_if_when_nested_then_inner_condition_applies() {
    let library = parse_with_pragma_if(
        "PROGRAM main
VAR x : INT; END_VAR
{IF TRUE}
{IF FALSE}
x := 1;
{ELSE}
x := 2;
{END_IF}
{ELSE}
x := 3;
{END_IF}
END_PROGRAM",
    );

    assert_eq!(assignments(&library), [("x".to_string(), "2".to_string())]);
}

/// REQ-JMP-parser-030: a non-branch pragma inside a taken branch stays trivia.
#[spec_test(REQ_JMP_parser_030)]
fn pragma_if_when_attribute_inside_branch_then_it_is_trivia() {
    let library = parse_with_pragma_if(
        "PROGRAM main
VAR x : INT; END_VAR
{IF TRUE}
{attribute 'qualified_only'}
x := 1;
{END_IF}
END_PROGRAM",
    );

    assert_eq!(assignments(&library), [("x".to_string(), "1".to_string())]);
}

/// REQ-JMP-parser-030: without the flag the pragmas stay trivia, so both
/// branches reach the parser -- the behavior before this feature existed.
#[spec_test(REQ_JMP_parser_030)]
fn pragma_if_when_flag_off_then_both_branches_parse() {
    let options = CompilerOptions {
        allow_pragmas: true,
        ..CompilerOptions::default()
    };
    let library = parse_program(
        "PROGRAM main
VAR x : INT; END_VAR
{IF defined(X)}
x := 1;
{ELSE}
x := 2;
{END_IF}
END_PROGRAM",
        &FileId::default(),
        &options,
    )
    .expect("pragmas stay trivia");

    assert_eq!(
        assignments(&library),
        [
            ("x".to_string(), "1".to_string()),
            ("x".to_string(), "2".to_string())
        ]
    );
}

/// REQ-JMP-parser-031: an `{IF}` with no `{END_IF}` reports P0023.
#[spec_test(REQ_JMP_parser_031)]
fn pragma_if_when_unclosed_then_problem() {
    let err = parse_program(
        "PROGRAM main VAR x : INT; END_VAR {IF TRUE} x := 1; END_PROGRAM",
        &FileId::default(),
        &opts_with_pragma_if(),
    )
    .expect_err("an unclosed {IF} is an error");

    assert_eq!(err.code, "P0023");
}

/// REQ-JMP-parser-031: a stray `{END_IF}` reports P0023.
#[spec_test(REQ_JMP_parser_031)]
fn pragma_if_when_stray_end_if_then_problem() {
    let err = parse_program(
        "PROGRAM main VAR x : INT; END_VAR {END_IF} x := 1; END_PROGRAM",
        &FileId::default(),
        &opts_with_pragma_if(),
    )
    .expect_err("a stray {END_IF} is an error");

    assert_eq!(err.code, "P0023");
}

/// REQ-JMP-parser-031: a condition the compiler does not understand reports
/// P0024. `COMPILERVERSION` needs the project's compiler version, which this
/// compiler does not model.
#[spec_test(REQ_JMP_parser_031)]
fn pragma_if_when_unknown_condition_then_problem() {
    let err = parse_program(
        "PROGRAM main VAR x : INT; END_VAR {IF COMPILERVERSION >= 3.5} x := 1; {END_IF} END_PROGRAM",
        &FileId::default(),
        &opts_with_pragma_if(),
    )
    .expect_err("an unsupported condition is an error");

    assert_eq!(err.code, "P0024");
}

/// REQ-JMP-parser-031: an `{IF}` with no condition reports P0024.
#[spec_test(REQ_JMP_parser_031)]
fn pragma_if_when_condition_missing_then_problem() {
    let err = parse_program(
        "PROGRAM main VAR x : INT; END_VAR {IF} x := 1; {END_IF} END_PROGRAM",
        &FileId::default(),
        &opts_with_pragma_if(),
    )
    .expect_err("a condition is required");

    assert_eq!(err.code, "P0024");
}

#[test]
fn parse_program_when_pragma_header_and_codesys_dialect_then_ok() {
    let source = enum_with_pragma_header();
    let options = CompilerOptions::from_dialect(Dialect::Codesys);

    let result = parse_program(&source, &FileId::default(), &options);

    assert!(result.is_ok(), "parse failed: {:?}", result.err());
}

#[test]
fn parse_program_when_pragma_header_and_default_dialect_then_err() {
    let source = enum_with_pragma_header();

    let result = parse_program(&source, &FileId::default(), &CompilerOptions::default());

    assert!(
        result.is_err(),
        "pragmas should still be unrecognized syntax without allow_pragmas"
    );
}

#[test]
fn parse_program_when_pragma_between_declarations_then_ok() {
    let source = "
        TYPE E_Color :
            (Red, Green, Blue);
        END_TYPE
        {attribute 'qualified_only'}
        FUNCTION_BLOCK FB_Example
        VAR
            x : INT;
        END_VAR
        END_FUNCTION_BLOCK";
    let options = CompilerOptions::from_dialect(Dialect::Codesys);

    let result = parse_program(source, &FileId::default(), &options);

    assert!(result.is_ok(), "parse failed: {:?}", result.err());
}

#[test]
fn parse_program_when_unclosed_pragma_and_codesys_dialect_then_err() {
    let source = "
        {attribute 'qualified_only'
        TYPE E_Color :
            (Red, Green, Blue);
        END_TYPE";
    let options = CompilerOptions::from_dialect(Dialect::Codesys);

    let result = parse_program(source, &FileId::default(), &options);

    assert!(result.is_err());
}
