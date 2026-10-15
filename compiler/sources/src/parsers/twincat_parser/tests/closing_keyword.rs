//! Tests that the closing keyword appended to a declaration is derived from its POU keyword.

use super::*;

#[test]
fn closing_keyword_when_program_then_returns_end_program() {
    assert_eq!(closing_keyword("PROGRAM MAIN\nVAR\nEND_VAR"), "END_PROGRAM");
}

#[test]
fn closing_keyword_when_function_block_then_returns_end_function_block() {
    assert_eq!(
        closing_keyword("FUNCTION_BLOCK FB_Test\nVAR\nEND_VAR"),
        "END_FUNCTION_BLOCK"
    );
}

#[test]
fn closing_keyword_when_function_then_returns_end_function() {
    assert_eq!(
        closing_keyword("FUNCTION MyFunc : INT\nVAR\nEND_VAR"),
        "END_FUNCTION"
    );
}

#[test]
fn closing_keyword_when_interface_then_returns_end_interface() {
    assert_eq!(closing_keyword("INTERFACE I_Drivable"), "END_INTERFACE");
}

#[test]
fn closing_keyword_when_unknown_keyword_then_returns_empty() {
    // Unknown POU type — fallback returns empty string
    assert_eq!(closing_keyword("UNKNOWN_TYPE Something"), "");
}

#[test]
fn closing_keyword_when_leading_whitespace_then_still_detects() {
    assert_eq!(
        closing_keyword("  PROGRAM MAIN\nVAR\nEND_VAR"),
        "END_PROGRAM"
    );
}

#[test]
fn closing_keyword_when_leading_line_comment_then_still_detects() {
    assert_eq!(
        closing_keyword("// header\n//\nFUNCTION_BLOCK FB_Test\nVAR\nEND_VAR"),
        "END_FUNCTION_BLOCK"
    );
}

#[test]
fn closing_keyword_when_leading_block_comments_then_still_detects() {
    assert_eq!(
        closing_keyword("(* header\n  *)\n/* more */ FUNCTION MyFunc : INT"),
        "END_FUNCTION"
    );
}

#[test]
fn closing_keyword_when_leading_pragma_then_still_detects() {
    assert_eq!(
        closing_keyword("{attribute 'hide'}\n// comment\nPROGRAM MAIN"),
        "END_PROGRAM"
    );
}

#[test]
fn closing_keyword_when_comment_contains_keyword_then_detects_keyword_after_it() {
    assert_eq!(
        closing_keyword("(* FUNCTION_BLOCK *) INTERFACE I_Drivable"),
        "END_INTERFACE"
    );
}

#[test]
fn closing_keyword_when_unterminated_comment_then_returns_empty() {
    assert_eq!(closing_keyword("(* never closed FUNCTION_BLOCK FB"), "");
}

#[test]
fn closing_keyword_when_only_line_comment_then_returns_empty() {
    assert_eq!(closing_keyword("// FUNCTION_BLOCK FB"), "");
}
