//! CODESYS jump statements: `JMP` with `label:` statement labels, `CALC` and
//! `__WAIT`.
//!
//! See `specs/design/codesys-jump-and-pragmas.md`.

use super::common::*;


use spec_test_macro::spec_test;

/// `allow_jump_statement` on, the gate for `JMP` and statement labels.
pub(crate) fn opts_with_jump() -> CompilerOptions {
    CompilerOptions {
        allow_jump_statement: true,
        ..CompilerOptions::default()
    }
}

/// `allow_calc_statement` on, the gate for `CALC`.
fn opts_with_calc() -> CompilerOptions {
    CompilerOptions {
        allow_calc_statement: true,
        ..CompilerOptions::default()
    }
}

/// `allow_wait_statement` on, the gate for `__WAIT`.
fn opts_with_wait() -> CompilerOptions {
    CompilerOptions {
        allow_wait_statement: true,
        ..CompilerOptions::default()
    }
}

fn parse_with(source: &str, options: &CompilerOptions) -> Library {
    let result = parse_program(source, &FileId::default(), options);
    assert!(result.is_ok(), "Parse failed: {:?}", result.err());
    result.unwrap()
}

/// The statements of the first program element.
fn statements(library: &Library) -> &[StmtKind] {
    let prog = cast!(&library.elements[0], LibraryElementKind::ProgramDeclaration);
    let body = cast!(&prog.body, FunctionBlockBodyKind::Statements);
    &body.body
}

/// REQ-JMP-parser-001: the three words lex as their own token types with the
/// gates on, and as identifiers with them off.
#[spec_test(REQ_JMP_parser_001)]
fn lexer_spec_req_jmp_parser_001_words_are_keywords_only_behind_their_gates() {
    use crate::token::TokenType;

    let all_on = CompilerOptions {
        allow_jump_statement: true,
        allow_calc_statement: true,
        allow_wait_statement: true,
        ..CompilerOptions::default()
    };
    let (tokens, diagnostics) =
        crate::tokenize_program("JMP calc __wait", &FileId::default(), &all_on, 0, 0);
    assert!(diagnostics.is_empty(), "{diagnostics:?}");
    let types: Vec<TokenType> = tokens
        .iter()
        .filter(|t| t.token_type != TokenType::Whitespace)
        .map(|t| t.token_type.clone())
        .collect();
    assert_eq!(types, [TokenType::Jmp, TokenType::Calc, TokenType::Wait]);

    // Case-insensitively, like every other keyword.
    let (tokens, _) =
        crate::tokenize_program("jmp calc __wait", &FileId::default(), &all_on, 0, 0);
    let types: Vec<TokenType> = tokens
        .iter()
        .filter(|t| t.token_type != TokenType::Whitespace)
        .map(|t| t.token_type.clone())
        .collect();
    assert_eq!(types, [TokenType::Jmp, TokenType::Calc, TokenType::Wait]);

    // Without their flags they are ordinary identifiers.
    let (tokens, _) = crate::tokenize_program(
        "JMP calc __wait",
        &FileId::default(),
        &CompilerOptions::default(),
        0,
        0,
    );
    assert!(
        tokens
            .iter()
            .filter(|t| t.token_type != TokenType::Whitespace)
            .all(|t| t.token_type == TokenType::Identifier),
        "tokens = {tokens:?}"
    );
}

/// REQ-JMP-parser-002: `JMP label;` jumps unconditionally, `JMP (cond) label;`
/// only when the condition holds.
#[spec_test(REQ_JMP_parser_002)]
fn jump_when_plain_then_no_condition() {
    let library = parse_with(
        "PROGRAM main
VAR x : INT; END_VAR
x := 1;
JMP done;
done:
x := 2;
END_PROGRAM",
        &opts_with_jump(),
    );

    let stmts = statements(&library);
    match &stmts[1] {
        StmtKind::Jump(jump) => {
            assert!(jump.condition.is_none());
            assert_eq!(jump.label.to_string(), "done");
        }
        other => panic!("expected a jump statement, got {other:?}"),
    }
    assert!(
        matches!(stmts[2], StmtKind::Label(_)),
        "stmts = {stmts:?}"
    );
}

/// REQ-JMP-parser-002: the conditional form carries the condition.
#[spec_test(REQ_JMP_parser_002)]
fn jump_when_condition_then_statement_carries_it() {
    let library = parse_with(
        "PROGRAM main
VAR x : BOOL; END_VAR
x := TRUE;
JMP (x) done;
done:
x := FALSE;
END_PROGRAM",
        &opts_with_jump(),
    );

    match &statements(&library)[1] {
        StmtKind::Jump(jump) => assert!(jump.condition.is_some(), "{jump:?}"),
        other => panic!("expected a jump statement, got {other:?}"),
    }
}

/// REQ-JMP-parser-003: a label is `name:` in statement position, and the
/// statement list may hold it without a terminator of its own.
#[spec_test(REQ_JMP_parser_003)]
fn label_when_in_statement_position_then_label_statement() {
    let library = parse_with(
        "PROGRAM main
VAR x : INT; END_VAR
start:
x := 1;
END_PROGRAM",
        &opts_with_jump(),
    );

    let stmts = statements(&library);
    assert_eq!(stmts.len(), 2, "{stmts:?}");
    match &stmts[0] {
        StmtKind::Label(label) => assert_eq!(label.name.to_string(), "start"),
        other => panic!("expected a label statement, got {other:?}"),
    }
}

/// REQ-JMP-parser-003: a label nested in a statement body (`IF`, loop, TRY)
/// is a label too.
#[spec_test(REQ_JMP_parser_003)]
fn label_when_nested_in_body_then_label_statement() {
    let library = parse_with(
        "PROGRAM main
VAR x : BOOL; END_VAR
IF x THEN
inner:
x := FALSE;
END_IF;
END_PROGRAM",
        &opts_with_jump(),
    );

    let stmts = statements(&library);
    match &stmts[0] {
        StmtKind::If(if_stmt) => match &if_stmt.body[0] {
            StmtKind::Label(label) => assert_eq!(label.name.to_string(), "inner"),
            other => panic!("expected a label statement, got {other:?}"),
        },
        other => panic!("expected an IF statement, got {other:?}"),
    }
}

/// REQ-JMP-parser-003: declarations, POU headers and `CASE` selectors keep
/// their meaning; only statement position makes a label. The negative side is
/// what the token transform is for, so nothing here may parse as a label:
/// each row still parses, and no `Label` statement appears.
#[spec_test(REQ_JMP_parser_003)]
fn label_when_not_in_statement_position_then_not_a_label() {
    let source = "TYPE
    T : STRUCT field : INT; END_STRUCT;
END_TYPE

FUNCTION f : INT
VAR
    x : INT;
    y : INT;
END_VAR
    f := 1;
END_FUNCTION

PROGRAM main
VAR
    e : (Red, Blue) := Red;
END_VAR
    CASE e OF
        Red: e := Blue;
        Blue: e := Red;
    END_CASE;
END_PROGRAM";
    let library = parse_with(source, &opts_with_jump());

    // The program's only statement is the CASE, with two selector groups.
    let prog = library
        .elements
        .iter()
        .find_map(|element| match element {
            LibraryElementKind::ProgramDeclaration(prog) => Some(prog),
            _ => None,
        })
        .expect("program element");
    let body = cast!(&prog.body, FunctionBlockBodyKind::Statements);
    match &body.body[0] {
        StmtKind::Case(case) => assert_eq!(
            case.statement_groups.len(),
            2,
            "the case selectors must not become labels: {case:?}"
        ),
        other => panic!("expected a CASE statement, got {other:?}"),
    }
}

/// REQ-JMP-parser-003: without the gate a `name:` statement is still a syntax
/// error, and the words stay usable as identifiers.
#[spec_test(REQ_JMP_parser_003)]
fn label_without_gate_then_syntax_error() {
    let source = "PROGRAM main VAR x : INT; END_VAR start: x := 1; END_PROGRAM";
    let result = parse_program(source, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_err());

    // `JMP` as a variable name: accepted in a standard program.
    let ok = parse_program(
        "PROGRAM main VAR jmp : INT; END_VAR jmp := 1; END_PROGRAM",
        &FileId::default(),
        &CompilerOptions::default(),
    );
    assert!(ok.is_ok(), "{:?}", ok.err());
}

/// REQ-JMP-parser-004: `CALC(condition, call)` carries the call.
#[spec_test(REQ_JMP_parser_004)]
fn calc_when_condition_and_call_then_statement_carries_both() {
    let library = parse_with(
        "PROGRAM main
VAR b : BOOL; END_VAR
b := TRUE;
CALC(b, Fb(a := 1));
END_PROGRAM",
        &opts_with_calc(),
    );

    match &statements(&library)[1] {
        StmtKind::ConditionalCall(call) => {
            assert_eq!(call.call.var_name.to_string(), "Fb");
            assert_eq!(call.call.params.len(), 1);
        }
        other => panic!("expected a conditional call, got {other:?}"),
    }
}

/// REQ-JMP-parser-004: with the gate off `CALC(...)` is an ordinary function
/// block invocation of a POU named `CALC` -- the two spellings are the same
/// text, so only the token type tells them apart.
#[spec_test(REQ_JMP_parser_004)]
fn calc_without_gate_then_plain_fb_call() {
    let library = parse_with(
        "PROGRAM main
VAR b : BOOL; END_VAR
CALC(b, Fb(a := 1));
END_PROGRAM",
        &CompilerOptions::default(),
    );

    match &statements(&library)[0] {
        StmtKind::FbCall(call) => assert_eq!(call.var_name.to_string(), "CALC"),
        other => panic!("expected an fb call, got {other:?}"),
    }
}

/// REQ-JMP-parser-005: `__WAIT;` and `__WAIT(condition);` both parse.
#[spec_test(REQ_JMP_parser_005)]
fn wait_when_condition_optional_then_both_forms_parse() {
    let library = parse_with(
        "PROGRAM main
VAR b : BOOL; END_VAR
__WAIT;
__WAIT(b);
END_PROGRAM",
        &opts_with_wait(),
    );

    let stmts = statements(&library);
    match (&stmts[0], &stmts[1]) {
        (StmtKind::Wait(first), StmtKind::Wait(second)) => {
            assert!(first.condition.is_none());
            assert!(second.condition.is_some());
        }
        other => panic!("expected wait statements, got {other:?}"),
    }
}

/// REQ-JMP-parser-001: with the gates off the statements are syntax errors,
/// so a program that uses the words as names still parses.
#[spec_test(REQ_JMP_parser_001)]
fn statements_without_gates_then_syntax_error() {
    let source = "PROGRAM main VAR x : INT; END_VAR JMP x; END_PROGRAM";
    assert!(parse_program(source, &FileId::default(), &CompilerOptions::default()).is_err());

    let source = "PROGRAM main VAR x : INT; END_VAR __WAIT; END_PROGRAM";
    assert!(parse_program(source, &FileId::default(), &CompilerOptions::default()).is_err());

}
