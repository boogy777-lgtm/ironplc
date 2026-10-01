//! Tiling and reconstruction guarantees over the shared corpus.
//!
//! The tree must reproduce the source for any input, so the same corpus and
//! the same truncations go through every parser entry point and every
//! dialect extreme.

mod common;

use common::{corpus, lex_exact};
use ironplc_syntax::lexer::{check_coverage, lex};
use ironplc_syntax::{
    parse_expression, parse_source_file, parse_statements, Parse, ParseOptions, SyntaxKind,
};

type EntryPoint = fn(&str, &ParseOptions) -> Parse;

const ENTRY_POINTS: [(&str, EntryPoint); 3] = [
    ("parse_source_file", parse_source_file),
    ("parse_statements", parse_statements),
    ("parse_expression", parse_expression),
];

fn option_sets() -> [ParseOptions; 2] {
    [ParseOptions::default(), ParseOptions::all()]
}

#[test]
fn lex_when_corpus_file_then_tokens_tile_source_exactly() {
    for (path, source) in corpus() {
        let (tokens, _) = lex(&source);
        let coverage = check_coverage(&source, &tokens);
        assert!(coverage.is_exact(), "{}: {coverage:?}", path.display());

        let mut offset = 0usize;
        for token in &tokens {
            let start = usize::from(token.range.start());
            let end = usize::from(token.range.end());
            assert_eq!(start, offset, "gap or overlap in {}", path.display());
            assert_eq!(&source[start..end], token.text);
            offset = end;
        }
        assert_eq!(offset, source.len(), "{}", path.display());
        let joined: String = tokens.iter().map(|token| token.text).collect();
        assert_eq!(joined, source, "{}", path.display());
    }
}

#[test]
fn parse_when_corpus_file_then_root_text_equals_source_for_every_entry_point() {
    for (path, source) in corpus() {
        for (name, parse) in ENTRY_POINTS {
            for options in option_sets() {
                let parsed = parse(&source, &options);
                assert_eq!(parsed.root.kind(), SyntaxKind::SourceFile);
                assert_eq!(
                    parsed.root.text().to_string(),
                    source,
                    "{name} on {}",
                    path.display()
                );
            }
        }
    }
}

#[test]
fn parse_source_file_when_corpus_file_then_every_error_lies_inside_the_source() {
    for (path, source) in corpus() {
        for (name, parse) in ENTRY_POINTS {
            let parsed = parse(&source, &ParseOptions::all());
            for error in &parsed.errors {
                assert!(
                    usize::from(error.range.end()) <= source.len(),
                    "{name} on {}: {error}",
                    path.display()
                );
            }
        }
    }
}

#[test]
fn lex_when_corpus_file_then_every_error_token_has_exactly_one_error() {
    for (path, source) in corpus() {
        let (tokens, errors) = lex(&source);
        let error_ranges: Vec<_> = tokens
            .iter()
            .filter(|token| token.kind == SyntaxKind::ErrorToken)
            .map(|token| token.range)
            .collect();
        let reported: Vec<_> = errors.iter().map(|error| error.range).collect();
        assert_eq!(error_ranges, reported, "{}", path.display());
    }
}

/// A sample with every lexical category, mid-sized for the prefix sweep.
const SAMPLE: &str = "PROGRAM Mixer\r\nVAR_INPUT\r\n\tspeed : REAL; (* target (* nested *) speed *)\r\nEND_VAR\r\n{attribute 'strict'}\r\n// line\r\n/// doc\r\ns := 'Wärmebild $'q$' ok';\r\nw := \"wide\";\r\nb := 16#FF + 2#1010 + 8#77 + 1.5E-3 + 3.25;\r\nt := T#1m30s;\r\nx AT %IX0.1 : BOOL; y := x.%X3 ? 1;\r\n/* c */ z := a <> b AND c <= d OR NOT e; `esc ident`\r";

#[test]
fn lex_when_every_prefix_of_sample_then_terminates_and_tiles() {
    // Prefixes may cut a multi-byte character; only char boundaries are
    // valid `&str` input.
    let mut count = 0;
    for end in 0..=SAMPLE.len() {
        if !SAMPLE.is_char_boundary(end) {
            continue;
        }
        lex_exact(&SAMPLE[..end]);
        count += 1;
    }
    assert!(count > 300, "swept only {count} prefixes");
}

#[test]
fn parse_when_every_prefix_of_sample_then_terminates_and_text_equals_prefix() {
    let mut count = 0;
    for end in 0..=SAMPLE.len() {
        if !SAMPLE.is_char_boundary(end) {
            continue;
        }
        let prefix = &SAMPLE[..end];
        for (name, parse) in ENTRY_POINTS {
            for options in option_sets() {
                let parsed = parse(prefix, &options);
                assert_eq!(parsed.root.text().to_string(), prefix, "{name} at {end}");
            }
        }
        count += 1;
    }
    assert!(count > 300, "swept only {count} prefixes");
}

#[test]
fn lex_when_every_suffix_of_sample_then_terminates_and_tiles() {
    for start in 0..=SAMPLE.len() {
        if SAMPLE.is_char_boundary(start) {
            lex_exact(&SAMPLE[start..]);
        }
    }
}

#[test]
fn parse_when_every_suffix_of_sample_then_text_equals_suffix() {
    for start in 0..=SAMPLE.len() {
        if SAMPLE.is_char_boundary(start) {
            let suffix = &SAMPLE[start..];
            for (name, parse) in ENTRY_POINTS {
                let parsed = parse(suffix, &ParseOptions::all());
                assert_eq!(parsed.root.text().to_string(), suffix, "{name} at {start}");
            }
        }
    }
}
