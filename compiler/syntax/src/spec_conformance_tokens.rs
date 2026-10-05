//! Spec conformance tests for the token stage and the line index (syntax-owned
//! requirements `REQ-PT-syntax-010` and `REQ-PT-syntax-011`).
//!
//! The completeness check is in `spec_conformance.rs`. The inputs are the
//! corpus the repository shares (`ironplc_test::corpus`): every `.st` file in
//! three spellings, read in place.
//!
//! See `specs/design/parse-tree-architecture.md`.

use crate::lexer::lex;
use crate::line_index::{break_len, LineIndex, LINE_BREAKS};
use crate::{parse_source_file, tokenize, ParseOptions, SyntaxKind};
use spec_test_macro::spec_test;

fn option_sets() -> [ParseOptions; 2] {
    [ParseOptions::default(), ParseOptions::all()]
}

/// REQ-PT-syntax-010: `tokenize` is the first stage of a parse and nothing
/// besides it: its tokens tile the text, and its errors are errors of the parse
/// of the same text, with the same kind, message and range, in source order.
#[spec_test(REQ_PT_syntax_010)]
fn tokenize_spec_req_pt_010_tokens_tile_the_text_and_errors_are_those_of_the_parse() {
    let mut with_errors = 0;
    for (name, text) in ironplc_test::corpus::variants() {
        for options in option_sets() {
            let (tokens, errors) = tokenize(&text, &options);
            let joined: String = tokens.iter().map(|token| token.text).collect();
            assert_eq!(joined, text, "{name}");

            let starts: Vec<_> = errors
                .iter()
                .map(|e| (e.range.start(), e.range.end()))
                .collect();
            let mut sorted = starts.clone();
            sorted.sort();
            assert_eq!(starts, sorted, "{name}: not in source order");

            let parse = parse_source_file(&text, &options);
            for error in &errors {
                assert!(parse.errors.contains(error), "{name}: {error}");
            }
            with_errors += usize::from(!errors.is_empty());
        }
    }
    assert!(
        with_errors > 0,
        "no input of the corpus has a tokenizer error"
    );
}

/// The tokens a parse puts in its tree are the tokens of `tokenize` (a parse
/// that began from a second lexing sequence would not be caught by the
/// comparison above), and tokenizing needs no thread of its own: it completes on
/// the smallest stack a caller has.
#[spec_test(REQ_PT_syntax_010)]
fn tokenize_spec_req_pt_010_a_parse_reads_the_tokens_of_tokenize_and_completes_on_a_small_stack() {
    let text = "PROGRAM p {IF TRUE} x := 1; {END_IF} (*@KEY@:A*) y (*@KEY@:END_A*) END_PROGRAM";
    let options = ParseOptions::all();
    let (tokens, _) = tokenize(text, &options);
    let parse = parse_source_file(text, &options);
    let in_tree: Vec<(SyntaxKind, String)> = parse
        .root
        .descendants_with_tokens()
        .filter_map(|element| element.into_token())
        .map(|token| (token.kind(), token.text().to_string()))
        .collect();
    let from_tokenize: Vec<(SyntaxKind, String)> = tokens
        .iter()
        .map(|token| (token.kind, token.text.to_string()))
        .collect();
    assert_eq!(in_tree, from_tokenize);

    // No thread: input nested far past the depth limit completes on the stack
    // of a thread of 1 MiB, the smallest a caller has.
    let nested = format!("{}1{}", "(".repeat(20_000), ")".repeat(20_000));
    let handle = std::thread::Builder::new()
        .stack_size(1024 * 1024)
        .spawn(move || tokenize(&nested, &ParseOptions::all()).0.len());
    assert!(handle.is_ok_and(|handle| handle.join().is_ok_and(|count| count > 40_000)));
}

/// REQ-PT-syntax-011: a position is counted by one mapping over one table of
/// line breaks: `\r\n` is one break, a lone `\r` and a form feed are breaks, a
/// column counts UTF-16 code units, and the column offset of a text embedded in
/// a document applies to its first line only. The lexer reads a newline token
/// with the same table.
#[spec_test(REQ_PT_syntax_011)]
fn line_index_spec_req_pt_011_one_table_of_breaks_and_utf16_columns() {
    // The table, and the lexer's newline token: every newline token of the
    // corpus is one spelling of the table and ends a line.
    let mut newlines = 0;
    for (name, text) in ironplc_test::corpus::variants() {
        let index = LineIndex::new(&text, 0, 0);
        let (tokens, _) = lex(&text);
        for token in tokens.iter().filter(|t| t.kind == SyntaxKind::Newline) {
            assert!(
                LINE_BREAKS.contains(&token.text),
                "{name}: {:?}",
                token.text
            );
            let start = usize::from(token.range.start());
            let end = usize::from(token.range.end());
            let (line, _) = index.position(start);
            assert_eq!(index.position(end), (line + 1, 0), "{name} at {start}");
            newlines += 1;
        }
    }
    assert!(newlines > 1000);

    // The spellings.
    let at = |text: &str, offset: usize| LineIndex::new(text, 0, 0).position(offset);
    assert_eq!(at("a\r\nb", 3), (1, 0));
    assert_eq!(at("a\rb", 2), (1, 0));
    assert_eq!(at("a\u{c}b", 2), (1, 0));
    assert_eq!(break_len("\r\nx"), Some(2));
    // UTF-16 columns: one unit for `é`, two for a character outside the plane.
    assert_eq!(at("é\u{1F600}x", "é\u{1F600}".len()), (0, 3));
    // The column offset applies to the first line only.
    let embedded = LineIndex::new("ab\ncd", 4, 9);
    assert_eq!(embedded.position(1), (4, 10));
    assert_eq!(embedded.position(4), (5, 1));
}
