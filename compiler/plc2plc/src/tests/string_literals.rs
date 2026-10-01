//! Character string literals in statement bodies.
//!
//! A declaration spells its own width (`STRING` / `WSTRING`), so the renderer
//! can pick the delimiter from the declaration. A literal in a statement body
//! has no such keyword — the delimiter *is* the width — so the literal has to
//! carry it. See issue #1550.

use super::common::*;
use rstest::rstest;

fn assignment_program(declaration: &str, literal: &str) -> String {
    format!("PROGRAM main\nVAR\n    v : {declaration};\nEND_VAR\nv := {literal};\nEND_PROGRAM\n")
}

#[test]
fn write_to_string_when_narrow_literal_in_body_then_single_quoted() {
    let source = assignment_program("STRING[10]", "'abc'");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    assert!(rendered.contains("v := 'abc'"), "rendered:\n{rendered}");
}

#[test]
fn write_to_string_when_wide_literal_in_body_then_double_quoted() {
    let source = assignment_program("WSTRING[10]", "\"abc\"");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    assert!(rendered.contains("v := \"abc\""), "rendered:\n{rendered}");
}

#[test]
fn write_to_string_when_wide_literal_contains_single_quote_then_not_escaped() {
    // Only the delimiter in force needs a `$` escape. Escaping a single quote
    // inside a WSTRING would change the value, because nothing unescapes it
    // on the way back in.
    let source = assignment_program("WSTRING[10]", "\"it's\"");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    assert!(rendered.contains("v := \"it's\""), "rendered:\n{rendered}");
}

#[test]
fn write_to_string_when_narrow_literal_contains_double_quote_then_not_escaped() {
    let source = assignment_program("STRING[10]", "'say \"hi\"'");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    assert!(
        rendered.contains("v := 'say \"hi\"'"),
        "rendered:\n{rendered}"
    );
}

#[test]
fn write_to_string_when_literal_in_function_call_argument_then_keeps_width() {
    let source = "PROGRAM main
VAR
    a : WSTRING[10];
    c : WSTRING[20];
END_VAR
c := CONCAT(a, \"tail\");
END_PROGRAM
";
    let rendered = assert_round_trips(source, &CompilerOptions::default());
    assert!(rendered.contains("\"tail\""), "rendered:\n{rendered}");
}

// A typed string literal's prefix (`UTF8#`, `UCHAR#`, `__XSTRING#`) names an
// encoding, not a different set of characters, so the AST keeps only the
// decoded characters and the width; the rendering is the untyped spelling.
#[test]
fn write_to_string_when_typed_string_literals_then_prefix_is_not_rendered() {
    let source = read_shared_resource("typed_string_literals.st");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());

    assert!(rendered.contains("narrow := 'aAb'"), "rendered:\n{rendered}");
    assert!(
        rendered.contains("wide := \"aAb\""),
        "rendered:\n{rendered}"
    );
    assert!(rendered.contains("code := 'A'"), "rendered:\n{rendered}");
}

// A literal's `value` holds the decoded characters, so rendering has to
// escape them again, exactly once. An earlier renderer re-escaped undecoded
// source text, which compounded on each pass (`$L`, `$$L`, `$$$$L`).
// `assert_round_trips` compares the decoded ASTs, so it catches either
// mistake.

#[test]
fn write_to_string_when_literal_contains_escape_then_escape_is_not_re_escaped() {
    // `$L` is one line feed. Rendering it as `$$L` would make it two
    // characters: a literal dollar and an `L`.
    let source = assignment_program("STRING[20]", "'a$Lb'");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    assert!(rendered.contains("v := 'a$Lb'"), "rendered:\n{rendered}");
}

#[test]
fn write_to_string_when_literal_contains_escaped_dollar_then_stays_one_dollar() {
    // `$$` is one dollar sign. It must not become `$$$$`.
    let source = assignment_program("STRING[20]", "'costs $$5'");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    assert!(
        rendered.contains("v := 'costs $$5'"),
        "rendered:\n{rendered}"
    );
}

#[test]
fn write_to_string_when_wide_literal_contains_escape_then_escape_is_preserved() {
    let source = assignment_program("WSTRING[20]", "\"tab$There\"");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    assert!(
        rendered.contains("v := \"tab$There\""),
        "rendered:\n{rendered}"
    );
}

#[test]
fn write_to_string_when_literal_rendered_twice_then_escapes_are_stable() {
    // The defect compounded: each pass added another `$`. Rendering the
    // re-parsed library must reproduce the same text.
    let source = assignment_program("STRING[20]", "'a$Lb$$c'");
    let rendered = assert_round_trips_idempotently(&source, &CompilerOptions::default());
    assert!(rendered.contains("v := 'a$Lb$$c'"), "rendered:\n{rendered}");
}

#[test]
fn write_to_string_when_literal_contains_raw_control_char_then_escaped() {
    // The lexer admits a raw tab inside a literal. It is the same character
    // as `$T`, and renders as the escape.
    let source = assignment_program("STRING[20]", "'a\tb'");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    assert!(rendered.contains("v := 'a$Tb'"), "rendered:\n{rendered:?}");
}

#[test]
fn write_to_string_when_narrow_literal_contains_escaped_quote_then_round_trips() {
    // `$'` is the single quote inside a single-quoted literal (#1818).
    let source = assignment_program("STRING[20]", "'it$'s'");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    assert!(rendered.contains("v := 'it$'s'"), "rendered:\n{rendered}");
}

#[test]
fn write_to_string_when_wide_literal_contains_escaped_quote_then_round_trips() {
    // `$"` is the double quote inside a double-quoted literal.
    let source = assignment_program("WSTRING[20]", "\"say $\"hi$\"\"");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    assert!(
        rendered.contains("v := \"say $\"hi$\"\""),
        "rendered:\n{rendered}"
    );
}

#[rstest]
#[case::hex_narrow("STRING[20]", "'$41$42'", "v := 'AB'")]
#[case::hex_wide("WSTRING[20]", "\"$00E9t$00E9\"", "v := \"été\"")]
#[case::lower_case_named("STRING[20]", "'a$nb'", "v := 'a$Lb'")]
#[case::other_delimiter_escaped("STRING[20]", "'say $\"hi$\"'", "v := 'say \"hi\"'")]
#[case::control_narrow("STRING[20]", "'$01'", "v := '$01'")]
#[case::control_wide("WSTRING[20]", "\"$0001\"", "v := \"$0001\"")]
// `$80` is the Windows-1252 euro sign, and it renders as itself.
#[case::cp1252_euro_narrow("STRING[20]", "'a$80b'", "v := 'a€b'")]
#[case::cp1252_euro_wide("WSTRING[20]", "\"a$0080b\"", "v := \"a€b\"")]
// 0x81 is undefined in Windows-1252, so `$81` is U+0081 and renders as the
// same escape.
#[case::cp1252_undefined("STRING[20]", "'$81'", "v := '$81'")]
#[case::cp1252_undefined_wide("WSTRING[20]", "\"$0081\"", "v := \"$0081\"")]
// U+0082 is a control character whose byte 0x82 Windows-1252 gives to `‚`,
// so no numeric escape denotes it and it renders as a Unicode escape.
#[case::cp1252_control("STRING[20]", "'$U00000082'", "v := '$U00000082'")]
#[case::cp1252_control_wide("WSTRING[20]", "\"$U00000082\"", "v := \"$U00000082\"")]
#[case::unicode_escape_narrow("STRING[20]", "'$U000020AC'", "v := '€'")]
#[case::unicode_escape_wide("WSTRING[20]", "\"$U000020AC\"", "v := \"€\"")]
fn write_to_string_when_literal_has_escape_then_renders_canonical_spelling(
    #[case] declaration: &str,
    #[case] literal: &str,
    #[case] expected: &str,
) {
    // The rendering is the canonical spelling of the same characters, so it
    // re-parses to the same AST even where the text differs.
    let source = assignment_program(declaration, literal);
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    assert!(rendered.contains(expected), "rendered:\n{rendered}");
}
