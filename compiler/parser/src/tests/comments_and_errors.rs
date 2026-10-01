//! Comment handling and top-level parse errors.

use super::common::*;
use spec_test_macro::spec_test;

#[test]
fn parse_program_when_has_comment_then_ok() {
    let source = "
        TYPE
        (* A comment *)
            CUSTOM_STRUCT : STRUCT 
                NAME: BOOL;
            END_STRUCT;
        END_TYPE";

    let res = parse_text(source);
    assert_eq!(1, res.elements.len());
}

#[test]
fn parse_program_when_back_to_back_comments_then_ok() {
    let program = "
        TYPE
        (* A comment *)(* A comment *)
           CUSTOM_STRUCT : STRUCT 
             NAME: BOOL;
           END_STRUCT;
        END_TYPE";

    parse_text(program);
}

#[test]
fn parse_program_when_right_parent_in_comment_then_ok() {
    let program = "
        TYPE
        (* A comment) *)(* A comment *)
           CUSTOM_STRUCT : STRUCT 
             NAME: BOOL;
           END_STRUCT;
        END_TYPE";

    parse_text(program);
}

#[test]
fn parse_program_when_comment_not_closed_then_err() {
    let program = "
        TYPE
        (* A comment
            CUSTOM_STRUCT : STRUCT
                NAME: BOOL;
            END_STRUCT;
        END_TYPE";

    let res = parse_program(program, &FileId::default(), &CompilerOptions::default());
    assert!(res.is_err());
}

#[test]
fn parse_program_when_bad_name_then_err() {
    let program = "
        TYPE
            CUSTOM_STRUCT : STRUCT& 
                NAME: BOOL;
            END_STRUCT;
        END_TYPE";

    let res = parse_program(program, &FileId::default(), &CompilerOptions::default());
    assert!(res.is_err());

    let err = res.unwrap_err();
    assert_eq!("Syntax error".to_owned(), err.description());
    assert_eq!("Expected ' ' (space) | '\\t' (tab) | '(* ... *)' (comment) | '///' (documentation comment) | '\\n' (new line) | '{ ... }' (pragma) | (identifier). Found text '&' that matched token 'AND' | '&'".to_owned(), err.primary.message);
}

#[test]
fn parse_program_when_not_valid_top_item_then_err() {
    let program = "ACTION
        END_ACTION";

    let res = parse_program(program, &FileId::default(), &CompilerOptions::default());
    assert!(res.is_err());

    let err = res.unwrap_err();
    assert_eq!("Syntax error".to_owned(), err.description());
    assert_eq!("Expected ' ' (space) | '\\t' (tab) | '(* ... *)' (comment) | '///' (documentation comment) | 'CONFIGURATION' | 'FUNCTION' | 'FUNCTION_BLOCK' | 'INTERFACE' | 'PROGRAM' | 'TYPE' | 'VAR_GLOBAL' | '\\n' (new line) | '{ ... }' (pragma). Found text 'ACTION' that matched token 'ACTION'".to_owned(), err.primary.message);
}


// ---------------------------------------------------------------------
// CODESYS comments: nested `(* ... *)` and `///` documentation comments.
// See `specs/design/codesys-jump-and-pragmas.md`.
// ---------------------------------------------------------------------

/// REQ-JMP-parser-020: with `allow_nested_comments` the whole nested comment
/// is one comment, so the statement after it parses.
#[spec_test(REQ_JMP_parser_020)]
fn parse_program_when_nested_comment_inside_body_then_ok() {
    let program = "
        PROGRAM main
        VAR x : INT; END_VAR
        (* a (* b *) c *)
        x := 1;
        END_PROGRAM";

    let result = parse_program(program, &FileId::default(), &opts_with_nested_comments());
    assert!(result.is_ok(), "{:?}", result.err());
}

/// REQ-JMP-parser-020: without the flag the nested comment is still an error,
/// so the flag is what widens the language.
#[spec_test(REQ_JMP_parser_020)]
fn parse_program_when_nested_comment_and_flag_off_then_err() {
    let program = "PROGRAM main VAR x : INT; END_VAR (* a (* b *) c *) x := 1; END_PROGRAM";

    let result = parse_program(program, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_err());
}

/// REQ-JMP-parser-020: an unterminated nested comment is left as it was, so
/// the flag changes nothing: the parse ends up exactly where the flag-off
/// parse does instead of swallowing the rest of the file into a comment.
#[spec_test(REQ_JMP_parser_020)]
fn parse_program_when_nested_comment_unterminated_then_same_as_flag_off() {
    let program = "PROGRAM main (* a (* b *) x := 1; END_PROGRAM";

    let with_flag = parse_program(program, &FileId::default(), &opts_with_nested_comments());
    let without_flag = parse_program(program, &FileId::default(), &CompilerOptions::default());

    assert_eq!(with_flag.is_ok(), without_flag.is_ok());
}

/// REQ-JMP-parser-021: `///` is a `DocComment` token of its own, and the
/// parser skips it as trivia.
#[spec_test(REQ_JMP_parser_021)]
fn tokenize_when_doc_comment_then_doc_comment_token() {
    use crate::token::TokenType;

    let options = CompilerOptions {
        allow_c_style_comments: true,
        ..CompilerOptions::default()
    };
    let (tokens, diagnostics) = crate::tokenize_program(
        "PROGRAM main
/// documentation
END_PROGRAM",
        &FileId::default(),
        &options,
        0,
        0,
    );
    assert!(diagnostics.is_empty(), "{diagnostics:?}");
    let doc = tokens
        .iter()
        .find(|t| t.token_type == TokenType::DocComment)
        .expect("a DocComment token");
    assert_eq!(doc.text, "/// documentation");

    // A plain `(* ... *)` comment is still a `Comment`, not a doc comment.
    let (tokens, _) = crate::tokenize_program(
        "(* plain *)",
        &FileId::default(),
        &CompilerOptions::default(),
        0,
        0,
    );
    assert_eq!(tokens[0].token_type, TokenType::Comment);
}

/// REQ-JMP-parser-021: a `///` comment is trivia for the parser, and without
/// `allow_c_style_comments` it is rejected like a `//` comment (P0004).
#[spec_test(REQ_JMP_parser_021)]
fn parse_program_when_doc_comment_then_trivia() {
    let program = "PROGRAM main
VAR x : INT; END_VAR
/// documentation
x := 1;
END_PROGRAM";
    let options = CompilerOptions {
        allow_c_style_comments: true,
        ..CompilerOptions::default()
    };
    let result = parse_program(program, &FileId::default(), &options);
    assert!(result.is_ok(), "{:?}", result.err());

    let err = parse_program(program, &FileId::default(), &CompilerOptions::default())
        .expect_err("a doc comment is a C-style comment form");
    assert_eq!(err.code, "P0004");
}
