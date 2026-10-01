//! CODESYS exception handling: `__TRY` / `__CATCH` / `__FINALLY` /
//! `__ENDTRY` / `__THROW`.
//!
//! See `specs/design/codesys-try-catch.md` and
//! `Codesys/grammar/ST_GRAMMAR.ebnf` (tryCatchStatement).

use super::common::*;
use spec_test_macro::spec_test;

fn parse_with_try_catch(source: &str) -> Library {
    let result = parse_program(source, &FileId::default(), &opts_with_try_catch());
    assert!(result.is_ok(), "Parse failed: {:?}", result.err());
    result.unwrap()
}

/// Wraps a statement body in a program whose `e` variable is the throw
/// target and the catch variable.
fn program_with_body(body: &str) -> String {
    format!("PROGRAM main\nVAR\n  e : DINT;\n  x : DINT;\nEND_VAR\n{body}\nEND_PROGRAM")
}

/// Returns the try/catch statement of a single-statement program body.
fn try_catch_of(source: &str) -> TryCatch {
    let library = parse_with_try_catch(&program_with_body(source));
    cast!(only_statement(&library), StmtKind::TryCatch).clone()
}

/// REQ-TC-parser-001: the five words lex as their own token types.
#[spec_test(REQ_TC_parser_001)]
fn lexer_spec_req_tc_parser_001_each_word_is_its_own_token() {
    use crate::token::TokenType;
    let (tokens, diagnostics) = crate::tokenize_program(
        "__TRY __ENDTRY __CATCH __FINALLY __THROW",
        &FileId::default(),
        &opts_with_try_catch(),
        0,
        0,
    );
    assert!(diagnostics.is_empty(), "{diagnostics:?}");

    let types: Vec<TokenType> = tokens
        .iter()
        .filter(|t| t.token_type != TokenType::Whitespace)
        .map(|t| t.token_type.clone())
        .collect();
    assert_eq!(
        types,
        [
            TokenType::Try,
            TokenType::EndTry,
            TokenType::Catch,
            TokenType::Finally,
            TokenType::Throw,
        ]
    );

    // Case-insensitive, like every other keyword.
    let (lower, diagnostics) = crate::tokenize_program(
        "__try __endtry __catch __finally __throw",
        &FileId::default(),
        &opts_with_try_catch(),
        0,
        0,
    );
    assert!(diagnostics.is_empty(), "{diagnostics:?}");
    assert!(lower.iter().any(|t| t.token_type == TokenType::EndTry));
}

/// REQ-TC-parser-002: with the flag off the words are ordinary identifiers,
/// so a program may use them as names; with it on they are keywords. A word
/// that merely starts with `__TRY` is never a keyword.
#[spec_test(REQ_TC_parser_002)]
fn lexer_spec_req_tc_parser_002_words_are_identifiers_without_the_gate() {
    use crate::token::TokenType;
    let (tokens, diagnostics) = crate::tokenize_program(
        "__TRY __ENDTRY __CATCH __FINALLY __THROW __TRYING",
        &FileId::default(),
        &CompilerOptions::default(),
        0,
        0,
    );
    assert!(diagnostics.is_empty(), "{diagnostics:?}");
    assert!(
        tokens
            .iter()
            .filter(|t| t.token_type != TokenType::Whitespace)
            .all(|t| t.token_type == TokenType::Identifier),
        "tokens = {tokens:?}"
    );

    // `__TRYING` is an identifier even when the gate is on.
    let (tokens, _) =
        crate::tokenize_program("__TRYING", &FileId::default(), &opts_with_try_catch(), 0, 0);
    assert_eq!(tokens[0].token_type, TokenType::Identifier);
    assert_eq!(tokens[0].text, "__TRYING");
}

/// REQ-TC-parser-010: a bare `__TRY … __ENDTRY` has no catch and no finally.
#[spec_test(REQ_TC_parser_010)]
fn try_catch_when_no_handlers_then_body_only() {
    let try_catch = try_catch_of("__TRY x := 1; __ENDTRY;");

    assert_eq!(try_catch.body.len(), 1);
    assert!(try_catch.catch.is_none());
    assert!(try_catch.finally_body.is_empty());
    assert!(
        try_catch.span.start < try_catch.span.end,
        "the statement's span covers `__TRY .. __ENDTRY`"
    );
}

/// REQ-TC-parser-010: an empty protected body is legal.
#[spec_test(REQ_TC_parser_010)]
fn try_catch_when_body_empty_then_empty_statements() {
    let try_catch = try_catch_of("__TRY __ENDTRY;");

    assert!(try_catch.body.is_empty());
    assert!(try_catch.catch.is_none());
}

/// REQ-TC-parser-011: `__CATCH (e)` names the exception variable, `__CATCH ()`
/// and a bare `__CATCH` do not.
#[spec_test(REQ_TC_parser_011)]
fn try_catch_when_catch_names_variable_then_catch_clause_has_it() {
    let try_catch = try_catch_of("__TRY x := 1; __CATCH (e) x := 2; __ENDTRY;");

    let catch = try_catch.catch.as_ref().expect("catch clause");
    assert_eq!(
        catch.exception.as_ref().map(|v| v.to_string()),
        Some("e".to_string())
    );
    assert_eq!(catch.body.len(), 1);
}

/// REQ-TC-parser-011: the empty-parenthesis spelling carries no variable and
/// still has a catch body.
#[spec_test(REQ_TC_parser_011)]
fn try_catch_when_catch_has_empty_parentheses_then_no_variable() {
    let try_catch = try_catch_of("__TRY x := 1; __CATCH () x := 2; __ENDTRY;");

    let catch = try_catch.catch.as_ref().expect("catch clause");
    assert!(catch.exception.is_none());
    assert_eq!(catch.body.len(), 1);
}

/// REQ-TC-parser-011: a bare `__CATCH` is a catch clause too.
#[spec_test(REQ_TC_parser_011)]
fn try_catch_when_catch_bare_then_no_variable() {
    let try_catch = try_catch_of("__TRY x := 1; __CATCH x := 2; __ENDTRY;");

    let catch = try_catch.catch.as_ref().expect("catch clause");
    assert!(catch.exception.is_none());
}

/// REQ-TC-parser-012: `__CATCH` and `__FINALLY` appear together, in order.
#[spec_test(REQ_TC_parser_012)]
fn try_catch_when_catch_and_finally_then_both_clauses() {
    let try_catch = try_catch_of("__TRY x := 1; __CATCH (e) x := 2; __FINALLY x := 3; __ENDTRY;");

    assert_eq!(try_catch.body.len(), 1);
    assert!(try_catch.catch.is_some());
    assert_eq!(try_catch.finally_body.len(), 1);
}

/// REQ-TC-parser-012: a finally clause without a catch clause is legal.
#[spec_test(REQ_TC_parser_012)]
fn try_catch_when_finally_only_then_no_catch_clause() {
    let try_catch = try_catch_of("__TRY x := 1; __FINALLY x := 3; __ENDTRY;");

    assert!(try_catch.catch.is_none());
    assert_eq!(try_catch.finally_body.len(), 1);
}

/// REQ-TC-parser-013: try statements nest.
#[spec_test(REQ_TC_parser_013)]
fn try_catch_when_nested_then_inner_statement_is_in_the_outer_body() {
    let try_catch = try_catch_of("__TRY __TRY x := 1; __ENDTRY; __CATCH (e) __THROW; __ENDTRY;");

    assert_eq!(try_catch.body.len(), 1);
    assert!(
        matches!(try_catch.body[0], StmtKind::TryCatch(_)),
        "body = {:?}",
        try_catch.body
    );
}

/// REQ-TC-parser-014: `__THROW(expr)` carries the expression, bare
/// `__THROW` carries none.
#[spec_test(REQ_TC_parser_014)]
fn throw_when_value_written_then_statement_carries_it() {
    let library = parse_with_try_catch(&program_with_body("__THROW(5);"));
    let throw = cast!(only_statement(&library), StmtKind::Throw);
    assert!(
        matches!(throw.value, Some(Expr { ref kind, .. }) if matches!(kind, ExprKind::Const(_)))
    );

    let library = parse_with_try_catch(&program_with_body("__THROW;"));
    let throw = cast!(only_statement(&library), StmtKind::Throw);
    assert!(throw.value.is_none());
}

/// REQ-TC-parser-020: the three clause vectors are reachable from the
/// statement and keep their statement counts.
#[spec_test(REQ_TC_parser_020)]
fn try_catch_when_all_clauses_then_each_clause_holds_its_statements() {
    let try_catch = try_catch_of(
        "__TRY x := 1; x := 2; __CATCH (e) x := 3; __FINALLY x := 4; x := 5; __ENDTRY;",
    );

    assert_eq!(try_catch.body.len(), 2);
    assert_eq!(try_catch.catch.as_ref().map(|c| c.body.len()), Some(1));
    assert_eq!(try_catch.finally_body.len(), 2);
}
