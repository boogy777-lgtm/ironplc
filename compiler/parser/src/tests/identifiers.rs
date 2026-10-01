//! Identifier spellings: escaped (backtick) identifiers, letters outside
//! ASCII, and the consecutive-underscore rule.

use super::common::*;
use spec_test_macro::spec_test;

fn opts_with_escaped_identifiers() -> CompilerOptions {
    CompilerOptions {
        allow_escaped_identifiers: true,
        ..CompilerOptions::default()
    }
}

fn opts_with_unicode_identifiers() -> CompilerOptions {
    CompilerOptions {
        allow_unicode_identifiers: true,
        ..CompilerOptions::default()
    }
}

fn opts_with_multiple_underscores() -> CompilerOptions {
    CompilerOptions {
        allow_multiple_underscores: true,
        ..CompilerOptions::default()
    }
}

/// Asserts the source is rejected with exactly `code`.
fn assert_problem_code(source: &str, options: &CompilerOptions, code: &str) {
    let result = parse_program(source, &FileId::default(), options);
    assert!(result.is_err(), "expected error {code}, got Ok");
    let d = result.unwrap_err();
    assert_eq!(d.code, code, "got {}: {}", d.code, d.description());
}

// ---------------------------------------------------------------------
// Escaped identifiers
// ---------------------------------------------------------------------

/// REQ-CS-parser-005: A backtick-delimited identifier parses as one
/// identifier whose name keeps the backticks, and produces P4067 without
/// the flag.
#[spec_test(REQ_CS_parser_005)]
fn parse_when_escaped_identifier_declared_then_name_keeps_its_spelling() {
    let source = "
PROGRAM main
VAR
    `my var` : INT;
END_VAR
END_PROGRAM";
    let lib = parse_program(source, &FileId::default(), &opts_with_escaped_identifiers()).unwrap();
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(prog.variables.len(), 1);
    // The name is the spelling as written, backticks included: the
    // declaration and every reference must agree character for character.
    assert_eq!(
        prog.variables[0].identifier.symbolic_id(),
        Some(&Id::from("`my var`"))
    );
}

#[test]
fn parse_when_escaped_identifier_referenced_then_same_name() {
    let source = "
PROGRAM main
VAR
    `my var` : INT;
END_VAR
    `my var` := 1;
END_PROGRAM";
    let lib = parse_program(source, &FileId::default(), &opts_with_escaped_identifiers()).unwrap();
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(prog.variables.len(), 1);
    let stmt = cast!(only_statement(&lib), StmtKind::Assignment);
    let target = cast!(&stmt.target, Variable::Symbolic);
    let name = cast!(target, SymbolicVariableKind::Named);
    assert_eq!(Some(&name.name), prog.variables[0].identifier.symbolic_id());
}

#[test]
fn parse_when_escaped_identifier_and_flag_off_then_requires_flag() {
    let source = "
PROGRAM main
VAR
    `my var` : INT;
END_VAR
END_PROGRAM";
    assert_problem_code(source, &CompilerOptions::default(), "P4067");
}

#[test]
fn parse_when_escaped_identifier_with_underscores_then_underscore_rule_skipped() {
    // Inside the escape everything may appear, so the consecutive-underscore
    // rule does not apply: the point of the escape is to admit spellings an
    // ordinary identifier cannot have.
    let source = "
PROGRAM main
VAR
    `my__var` : INT;
END_VAR
END_PROGRAM";
    let result = parse_program(source, &FileId::default(), &opts_with_escaped_identifiers());
    assert!(result.is_ok(), "parse failed: {:?}", result.err());
}

// ---------------------------------------------------------------------
// Unicode identifiers
// ---------------------------------------------------------------------

/// REQ-CS-parser-006: Letters outside ASCII are accepted in an
/// identifier, and produce P4068 when the flag is off.
#[spec_test(REQ_CS_parser_006)]
fn parse_when_unicode_identifier_declared_then_parses() {
    let source = "
PROGRAM main
VAR
    caf\u{e9} : INT;
END_VAR
END_PROGRAM";
    let lib = parse_program(source, &FileId::default(), &opts_with_unicode_identifiers()).unwrap();
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(
        prog.variables[0].identifier.symbolic_id(),
        Some(&Id::from("caf\u{e9}"))
    );
}

#[test]
fn parse_when_unicode_identifier_and_flag_off_then_requires_flag() {
    let source = "
PROGRAM main
VAR
    caf\u{e9} : INT;
END_VAR
END_PROGRAM";
    assert_problem_code(source, &CompilerOptions::default(), "P4068");
}

#[test]
fn parse_when_unicode_digit_in_identifier_then_lexes_as_one_name() {
    // Continuation characters may be non-ASCII digits (category Nd) too.
    let source = "
PROGRAM main
VAR
    x\u{0ba8} : INT;
END_VAR
END_PROGRAM";
    let lib = parse_program(source, &FileId::default(), &opts_with_unicode_identifiers()).unwrap();
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(
        prog.variables[0].identifier.symbolic_id(),
        Some(&Id::from("x\u{0ba8}"))
    );
}

// ---------------------------------------------------------------------
// Consecutive underscores
// ---------------------------------------------------------------------

/// REQ-CS-parser-007: An identifier containing `__` outside the leading
/// reserved prefix produces P4069 unless allowed; the leading `__` prefix
/// is accepted without the flag.
#[spec_test(REQ_CS_parser_007)]
fn parse_when_inner_double_underscore_and_flag_off_then_requires_flag() {
    let source = "
PROGRAM main
VAR
    my__var : INT;
END_VAR
END_PROGRAM";
    assert_problem_code(source, &CompilerOptions::default(), "P4069");
}

#[test]
fn parse_when_inner_double_underscore_and_flag_on_then_parses() {
    let source = "
PROGRAM main
VAR
    my__var : INT;
END_VAR
END_PROGRAM";
    let lib = parse_program(
        source,
        &FileId::default(),
        &opts_with_multiple_underscores(),
    )
    .unwrap();
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(
        prog.variables[0].identifier.symbolic_id(),
        Some(&Id::from("my__var"))
    );
}

#[test]
fn parse_when_reserved_double_underscore_prefix_then_parses_without_flag() {
    // `__` in the leading position is the compiler's reserved namespace: the
    // implicit `__SYSTEM_UP_TIME` global and the `__TRUNC` intrinsic both
    // live there, so it must not need an option.
    let source = "
PROGRAM main
VAR
    __internal : INT;
END_VAR
END_PROGRAM";
    let lib = parse_text(source);
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(
        prog.variables[0].identifier.symbolic_id(),
        Some(&Id::from("__internal"))
    );
}

#[test]
fn parse_when_reserved_prefix_and_later_pair_then_requires_flag() {
    let source = "
PROGRAM main
VAR
    __x__y : INT;
END_VAR
END_PROGRAM";
    assert_problem_code(source, &CompilerOptions::default(), "P4069");
}
