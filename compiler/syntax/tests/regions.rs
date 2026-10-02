//! Regions the grammar skips: OSCAT ranged comments and the branches of
//! conditional pragmas that are not taken. Each is one trivia token in the
//! tree, the tree still reproduces the source, and the grammar never sees what
//! is inside.

mod common;

use ironplc_dsl::core::FileId;
use ironplc_syntax::{
    parse_source_file, parse_statements, ErrorKind, Parse, ParseOptions, SyntaxKind,
};

fn with_pragma_if() -> ParseOptions {
    ParseOptions {
        allow_pragma_if: true,
        ..ParseOptions::all()
    }
}

fn without_pragma_if() -> ParseOptions {
    ParseOptions {
        allow_pragma_if: false,
        ..ParseOptions::all()
    }
}

/// The text of every token of `kind` in the tree, in source order.
fn tokens_of(parsed: &Parse, kind: SyntaxKind) -> Vec<String> {
    parsed
        .root
        .descendants_with_tokens()
        .filter_map(|element| element.into_token())
        .filter(|token| token.kind() == kind)
        .map(|token| token.text().to_string())
        .collect()
}

fn assignments(parsed: &Parse) -> usize {
    parsed
        .root
        .descendants()
        .filter(|node| node.kind() == SyntaxKind::AssignStmt)
        .count()
}

fn codes(parsed: &Parse) -> Vec<String> {
    parsed
        .diagnostics(&FileId::default())
        .into_iter()
        .map(|diagnostic| diagnostic.code)
        .collect()
}

fn assert_lossless(source: &str, parsed: &Parse) {
    assert_eq!(parsed.root.text().to_string(), source);
    for error in &parsed.errors {
        assert!(
            usize::from(error.range.end()) <= source.len(),
            "{error} outside {source:?}"
        );
    }
}

const OSCAT_FILE: &str = "PROGRAM p\n(*@KEY@:DESCRIPTION*)\nfree text ' (* ? {\n(*@KEY@:END_DESCRIPTION*)\nVAR x : INT; END_VAR\nx := 1;\nEND_PROGRAM\n";

#[test]
fn parse_source_file_when_oscat_pair_then_one_ranged_comment_token_holds_markers_and_body() {
    let parsed = parse_source_file(OSCAT_FILE, &ParseOptions::default());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_lossless(OSCAT_FILE, &parsed);
    assert_eq!(
        tokens_of(&parsed, SyntaxKind::RangedComment),
        vec!["(*@KEY@:DESCRIPTION*)\nfree text ' (* ? {\n(*@KEY@:END_DESCRIPTION*)"]
    );
    assert!(SyntaxKind::RangedComment.is_trivia());
}

#[test]
fn parse_source_file_when_several_oscat_pairs_then_every_pair_is_a_region() {
    let source = "PROGRAM p\n(*@KEY@:A*)x ? y(*@KEY@:END_A*)\nVAR v : INT; END_VAR\n(*@KEY@:B*) ' (*@KEY@:END_B*)\nv := 1;\n(*@KEY@:C*)z(*@KEY@:END_C*)\nEND_PROGRAM\n";
    let parsed = parse_source_file(source, &ParseOptions::default());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_lossless(source, &parsed);
    assert_eq!(tokens_of(&parsed, SyntaxKind::RangedComment).len(), 3);
}

#[test]
fn parse_source_file_when_oscat_body_is_non_ascii_or_crlf_then_lossless() {
    let body = "PROGRAM p\r\n(*@KEY@:D*)\r\n\u{439}\u{446} \u{1F600} text\r\n(*@KEY@:END_D*)\r\nEND_PROGRAM\r\n";
    let parsed = parse_source_file(body, &ParseOptions::default());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_lossless(body, &parsed);
    assert_eq!(tokens_of(&parsed, SyntaxKind::RangedComment).len(), 1);
}

#[test]
fn parse_source_file_when_oscat_marker_has_no_closing_marker_then_plain_comment() {
    let source = "PROGRAM p\n(*@KEY@:D*)\nEND_PROGRAM\n";
    let parsed = parse_source_file(source, &ParseOptions::default());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert!(tokens_of(&parsed, SyntaxKind::RangedComment).is_empty());
    assert_eq!(tokens_of(&parsed, SyntaxKind::BlockComment).len(), 1);
}

#[test]
fn parse_source_file_when_oscat_markers_are_inside_a_comment_or_string_then_not_a_region() {
    let source = "PROGRAM p\nVAR s : STRING; END_VAR\n(* (*@KEY@:A*) x (*@KEY@:END_A*) *)\ns := '(*@KEY@:B*) (*@KEY@:END_B*)';\nEND_PROGRAM\n";
    let parsed = parse_source_file(source, &ParseOptions::all());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert!(tokens_of(&parsed, SyntaxKind::RangedComment).is_empty());
}

#[test]
fn parse_source_file_when_oscat_body_is_malformed_then_no_error_for_it() {
    let source = "PROGRAM p\n(*@KEY@:D*) ? ` ' \" {unterminated (*@KEY@:END_D*)\nEND_PROGRAM\n";
    let parsed = parse_source_file(source, &ParseOptions::default());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_lossless(source, &parsed);
}

#[test]
fn parse_statements_when_untaken_branch_then_inactive_region_between_the_directives() {
    let source = "{IF false} x := ; {ELSE} y := 1; {END_IF}";
    let parsed = parse_statements(source, &with_pragma_if());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_lossless(source, &parsed);
    assert_eq!(
        tokens_of(&parsed, SyntaxKind::InactiveRegion),
        vec![" x := ; "]
    );
    assert_eq!(tokens_of(&parsed, SyntaxKind::Pragma).len(), 3);
    assert_eq!(assignments(&parsed), 1);
    assert!(SyntaxKind::InactiveRegion.is_trivia());
}

#[test]
fn parse_statements_when_elsif_chain_then_each_stretch_that_stays_dead_is_one_region() {
    let source =
        "{IF false} a := 1; {ELSIF true} b := 2; {ELSIF true} c := 3; {ELSE} d := 4; {END_IF}";
    let parsed = parse_statements(source, &with_pragma_if());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_lossless(source, &parsed);
    assert_eq!(
        tokens_of(&parsed, SyntaxKind::InactiveRegion),
        vec![" a := 1; ", " c := 3; {ELSE} d := 4; "]
    );
    assert_eq!(assignments(&parsed), 1);
}

#[test]
fn parse_statements_when_nested_if_in_untaken_branch_then_one_region_for_the_whole_branch() {
    let source = "{IF true} a := 1; {IF false} b := 2; {IF true} c := 3; {END_IF} d := 4; {END_IF} e := 5; {END_IF}";
    let parsed = parse_statements(source, &with_pragma_if());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_lossless(source, &parsed);
    assert_eq!(
        tokens_of(&parsed, SyntaxKind::InactiveRegion),
        vec![" b := 2; {IF true} c := 3; {END_IF} d := 4; "]
    );
    assert_eq!(assignments(&parsed), 2);
}

#[test]
fn parse_statements_when_define_in_untaken_branch_then_ignored_and_in_live_branch_then_honoured() {
    let ignored = "{IF false} {DEFINE x} {END_IF} {IF defined(x)} a := ; {END_IF} b := 1;";
    let parsed = parse_statements(ignored, &with_pragma_if());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_lossless(ignored, &parsed);
    assert_eq!(tokens_of(&parsed, SyntaxKind::InactiveRegion).len(), 2);

    let honoured = "{DEFINE x} {IF defined(x)} a := 1; {ELSE} b := ; {END_IF}";
    let parsed = parse_statements(honoured, &with_pragma_if());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_eq!(
        tokens_of(&parsed, SyntaxKind::InactiveRegion),
        vec![" b := ; "]
    );
}

#[test]
fn parse_statements_when_other_pragma_in_untaken_branch_then_inside_the_region() {
    let source = "{IF false} {attribute 'x'} {END_IF} {attribute 'y'} a := 1;";
    let parsed = parse_statements(source, &with_pragma_if());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_eq!(
        tokens_of(&parsed, SyntaxKind::InactiveRegion),
        vec![" {attribute 'x'} "]
    );
    assert_eq!(tokens_of(&parsed, SyntaxKind::Pragma).len(), 3);
}

#[test]
fn parse_statements_when_ranged_comment_in_untaken_branch_then_one_inactive_region() {
    let source = "{IF false}(*@KEY@:A*) x := ; (*@KEY@:END_A*){END_IF} y := 1;";
    let parsed = parse_statements(source, &with_pragma_if());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_lossless(source, &parsed);
    assert_eq!(tokens_of(&parsed, SyntaxKind::InactiveRegion).len(), 1);
    assert!(tokens_of(&parsed, SyntaxKind::RangedComment).is_empty());
}

#[test]
fn parse_statements_when_pragma_if_flag_off_then_both_branches_reach_the_grammar() {
    let source = "{IF false} x := ; {END_IF} y := 1;";
    let off = parse_statements(source, &without_pragma_if());
    assert_lossless(source, &off);
    assert!(!off.is_ok());
    assert!(tokens_of(&off, SyntaxKind::InactiveRegion).is_empty());

    let on = parse_statements(source, &with_pragma_if());
    assert!(on.is_ok(), "{:?}", on.errors);
}

#[test]
fn parse_statements_when_pragma_if_flag_off_then_unmatched_directives_are_not_errors() {
    let parsed = parse_statements("{END_IF} {IF x} a := 1;", &without_pragma_if());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
}

#[test]
fn parse_statements_when_pragmas_not_enabled_then_directives_are_gated_like_any_pragma() {
    let options = ParseOptions {
        allow_pragmas: false,
        ..with_pragma_if()
    };
    let parsed = parse_statements("{IF true} a := 1; {END_IF}", &options);
    assert_eq!(parsed.errors.len(), 2);
    assert!(parsed
        .errors
        .iter()
        .all(|error| error.kind == ErrorKind::Syntax));
}

#[test]
fn parse_statements_when_if_never_closed_then_p0023_at_the_if_and_the_rest_is_a_region() {
    let source = "a := 1; {IF false} b := ; c := 2;";
    let parsed = parse_statements(source, &with_pragma_if());
    assert_lossless(source, &parsed);
    assert_eq!(parsed.errors.len(), 1);
    assert_eq!(parsed.errors[0].kind, ErrorKind::PragmaIfUnmatched);
    assert_eq!(codes(&parsed), vec!["P0023"]);
    let start = source.find("{IF").unwrap_or(0);
    assert_eq!(usize::from(parsed.errors[0].range.start()), start);
    assert_eq!(
        usize::from(parsed.errors[0].range.end()),
        start + "{IF false}".len()
    );
    assert_eq!(
        tokens_of(&parsed, SyntaxKind::InactiveRegion),
        vec![" b := ; c := 2;"]
    );
}

#[test]
fn parse_statements_when_live_if_never_closed_then_p0023_and_no_region() {
    let parsed = parse_statements("{IF true} a := 1;", &with_pragma_if());
    assert_eq!(codes(&parsed), vec!["P0023"]);
    assert!(tokens_of(&parsed, SyntaxKind::InactiveRegion).is_empty());
}

#[test]
fn parse_statements_when_stray_directive_then_p0023_at_it() {
    for directive in ["{END_IF}", "{ELSE}", "{ELSIF true}"] {
        let source = format!("{directive} a := 1;");
        let parsed = parse_statements(&source, &with_pragma_if());
        assert_lossless(&source, &parsed);
        assert_eq!(codes(&parsed), vec!["P0023"], "{directive}");
        assert_eq!(
            usize::from(parsed.errors[0].range.end()),
            directive.len(),
            "{directive}"
        );
        assert!(tokens_of(&parsed, SyntaxKind::InactiveRegion).is_empty());
    }
}

#[test]
fn parse_statements_when_condition_not_understood_then_p0024_and_the_branch_is_untaken() {
    for source in [
        "{IF hastype(x)} a := ; {END_IF}",
        "{IF} a := ; {END_IF}",
        "{IF COMPILERVERSION >= 3.5} a := ; {END_IF}",
    ] {
        let parsed = parse_statements(source, &with_pragma_if());
        assert_lossless(source, &parsed);
        assert_eq!(codes(&parsed), vec!["P0024"], "{source}");
        assert_eq!(parsed.errors[0].kind, ErrorKind::PragmaValueExpected);
        assert_eq!(tokens_of(&parsed, SyntaxKind::InactiveRegion).len(), 1);
    }
}

#[test]
fn parse_statements_when_define_has_no_name_then_p0024() {
    let parsed = parse_statements("{DEFINE} a := 1;", &with_pragma_if());
    assert_eq!(codes(&parsed), vec!["P0024"]);
}

#[test]
fn parse_statements_when_lexical_error_in_untaken_branch_then_still_reported() {
    let source = "{IF false} x := ?; {END_IF}";
    let parsed = parse_statements(source, &with_pragma_if());
    assert_lossless(source, &parsed);
    assert_eq!(codes(&parsed), vec!["P0003"]);
    let at = source.find('?').unwrap_or(0);
    assert_eq!(usize::from(parsed.errors[0].range.start()), at);
}

#[test]
fn parse_source_file_when_declaration_in_untaken_branch_then_not_parsed() {
    let source = "PROGRAM p\nVAR x : INT; END_VAR\n{IF false}\nVAR y : ; END_VAR\n{END_IF}\nx := 1;\nEND_PROGRAM\n";
    let parsed = parse_source_file(source, &with_pragma_if());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_lossless(source, &parsed);
}

#[test]
fn parse_source_file_when_crlf_then_regions_and_losslessness_hold() {
    let source = "PROGRAM p\n{IF false}\nx := ;\n{ELSE}\ny := 1;\n{END_IF}\n(*@KEY@:D*)\nbody\n(*@KEY@:END_D*)\nEND_PROGRAM\n"
        .replace('\n', "\r\n");
    let parsed = parse_source_file(&source, &with_pragma_if());
    assert!(parsed.is_ok(), "{:?}", parsed.errors);
    assert_lossless(&source, &parsed);
    assert_eq!(tokens_of(&parsed, SyntaxKind::InactiveRegion).len(), 1);
    assert_eq!(tokens_of(&parsed, SyntaxKind::RangedComment).len(), 1);
}

#[test]
fn parse_source_file_when_every_prefix_of_region_input_then_lossless_and_terminates() {
    let sample = "PROGRAM p\n{DEFINE a}\n{IF defined(a) AND NOT false}\n(*@KEY@:K*) \u{439}' {IF} (*@KEY@:END_K*)\n{ELSIF true} x := ;\n{ELSE}\n{IF bad} {END_IF}\n{END_IF}\n{END_IF}\n{IF false}\n(*@KEY@:Z*)\nEND_PROGRAM\n";
    for options in [
        with_pragma_if(),
        without_pragma_if(),
        ParseOptions::default(),
    ] {
        for end in (0..=sample.len()).filter(|end| sample.is_char_boundary(*end)) {
            let prefix = &sample[..end];
            let parsed = parse_source_file(prefix, &options);
            assert_lossless(prefix, &parsed);
            let statements = parse_statements(prefix, &options);
            assert_lossless(prefix, &statements);
        }
    }
}
