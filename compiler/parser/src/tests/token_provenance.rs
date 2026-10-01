//! Provenance guard for the legacy token stream.
//!
//! The legacy pipeline (`tokenize_program`) rewrites the token stream in
//! place, and the only provenance the parser sees is each token's span plus
//! its copied text. These tests check that provenance against the original
//! source, so a transform that loses or invents bytes fails loudly while the
//! PEG pipeline still runs (S0 audit section 7 item 5, S0 experiment 4.3).
//!
//! For every case the stream is checked for:
//! * `TextMismatch`: a real token whose text differs from `source[span]`;
//! * `Gap`: source bytes (including a tail) that no real token covers;
//! * `Overlap`: a real token that starts before the previous real token ends;
//! * `Order`: any token (real or synthetic) that starts before its predecessor;
//! * `NonZeroSynthetic`: a synthetic terminator (empty text) with a width.
//!
//! A "real" token is one with non-empty text; synthetic terminators carry
//! empty text and own no source bytes.
//!
//! What does not hold today is pinned in `KNOWN_VIOLATIONS`: an explicit,
//! named list of cases with the exact offending ranges, the audit finding and
//! the reason. A case is compared against its pins exactly, so fixing a
//! defect means deleting its entry, and a new violation anywhere else (or a
//! pinned one that moves) fails the test.

use super::common::*;
use crate::token::{Token, TokenType};

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord)]
enum Rule {
    TextMismatch,
    Gap,
    Overlap,
    Order,
    NonZeroSynthetic,
}

/// One broken provenance expectation: the rule and the source range involved
/// (the token span for token rules, the uncovered bytes for `Gap`).
#[derive(Debug, Clone, PartialEq, Eq, PartialOrd, Ord)]
struct Violation {
    rule: Rule,
    start: usize,
    end: usize,
}

/// Checks `tokens` against `source` and returns every violation, in stream
/// order. An empty result means the real tokens tile the source exactly and
/// every provenance expectation holds.
fn check_provenance(source: &str, tokens: &[Token]) -> Vec<Violation> {
    let mut violations = vec![];
    let mut push = |rule, start, end| violations.push(Violation { rule, start, end });

    let mut cursor = 0usize;
    let mut previous_start = 0usize;
    for token in tokens {
        let (start, end) = (token.span.start, token.span.end);
        if start < previous_start {
            push(Rule::Order, start, end);
        }
        previous_start = start;

        if token.text.is_empty() {
            if start != end {
                push(Rule::NonZeroSynthetic, start, end);
            }
            continue;
        }

        if source.get(start..end) != Some(token.text.as_str()) {
            push(Rule::TextMismatch, start, end);
        }
        if start > cursor {
            push(Rule::Gap, cursor, start);
        }
        if start < cursor {
            push(Rule::Overlap, start, cursor.min(end));
        }
        cursor = cursor.max(end);
    }
    if cursor < source.len() {
        push(Rule::Gap, cursor, source.len());
    }
    violations
}

/// A pinned exception: `case` is known to violate `rule` over exactly
/// `ranges`. `when_pragmas` limits the pin to option sets with
/// `allow_pragmas` equal to the value (`None` = every option set).
struct KnownViolation {
    case: &'static str,
    rule: Rule,
    ranges: &'static [(usize, usize)],
    when_pragmas: Option<bool>,
    finding: &'static str,
    reason: &'static str,
}

const KNOWN_VIOLATIONS: &[KnownViolation] = &[
    KnownViolation {
        case: "oscat.st",
        rule: Rule::TextMismatch,
        ranges: &[(132, 156), (157, 175), (176, 192), (194, 210)],
        when_pragmas: None,
        finding: "F1",
        reason: "the preprocessor blanks the OSCAT ranged-comment body to spaces before \
                 lexing, so the whitespace tokens carry spaces where the file holds text",
    },
    KnownViolation {
        case: "oscat_ascii",
        rule: Rule::TextMismatch,
        ranges: &[(22, 31)],
        when_pragmas: None,
        finding: "F1",
        reason: "OSCAT body blanked to spaces; the whitespace token text differs from the source",
    },
    KnownViolation {
        case: "oscat_two",
        rule: Rule::TextMismatch,
        ranges: &[(12, 17)],
        when_pragmas: None,
        finding: "F1",
        reason: "only the first OSCAT pair is blanked (the second body is lexed as \
                 identifiers and tiles correctly), so just the first region mismatches",
    },
    KnownViolation {
        case: "oscat_unicode",
        rule: Rule::TextMismatch,
        ranges: &[
            (22, 31),
            (31, 32),
            (32, 57),
            (57, 58),
            (58, 62),
            (62, 63),
            (63, 64),
            (64, 65),
            (65, 66),
            (66, 67),
            (67, 68),
            (68, 71),
            (71, 72),
            (72, 73),
            (73, 81),
            (81, 82),
        ],
        when_pragmas: None,
        finding: "F1",
        reason: "blanking is per char, not per byte: the 2-byte a-umlaut becomes one \
                 space, so every later token is one byte off the original source",
    },
    KnownViolation {
        case: "oscat_unicode",
        rule: Rule::Gap,
        ranges: &[(82, 83)],
        when_pragmas: None,
        finding: "F1",
        reason: "the byte shift from the multi-byte character leaves the last source \
                 byte uncovered",
    },
    KnownViolation {
        case: "lexer_error",
        rule: Rule::Gap,
        ranges: &[(7, 8)],
        when_pragmas: None,
        finding: "F2",
        reason: "the lexer emits a diagnostic for the unmatched `?` but no token or \
                 error region",
    },
    KnownViolation {
        case: "lexer_error_multibyte",
        rule: Rule::Gap,
        ranges: &[(7, 9)],
        when_pragmas: None,
        finding: "F2",
        reason: "the 2-byte unmatched currency sign has a diagnostic but no token",
    },
    KnownViolation {
        case: "lone_carriage_return",
        rule: Rule::Gap,
        ranges: &[(9, 10), (30, 31), (38, 39), (50, 51)],
        when_pragmas: None,
        finding: "not in the audit (F2 family)",
        reason: "a carriage return not followed by a line feed matches no lexer rule: \
                 it is dropped as an unmatched byte, like the F2 error bytes",
    },
    KnownViolation {
        case: "pragma_with_lexer_error",
        rule: Rule::Gap,
        ranges: &[(8, 9)],
        when_pragmas: Some(false),
        finding: "F2",
        reason: "without pragma collapsing the `?` inside the braces is an unmatched byte",
    },
    KnownViolation {
        case: "pragma_with_lexer_error",
        rule: Rule::TextMismatch,
        ranges: &[(5, 12)],
        when_pragmas: Some(true),
        finding: "F4",
        reason: "the collapsed Pragma text is the concatenation of the inner token \
                 texts, so it diverges from the source slice where the lexer dropped \
                 a byte",
    },
];

/// Source cases that are not files in the shared corpus. Each is checked
/// under every dialect's options unless a pin narrows it.
const SNIPPETS: &[(&str, &str)] = &[
    (
        "unicode",
        "(* W\u{e4}rmebild \u{65e5}\u{672c} \u{1f600} *)\nPROGRAM p\nVAR s : STRING := 'W\u{e4}rmebild \u{1f600}'; END_VAR\ns := '\u{e9}';\nEND_PROGRAM\n",
    ),
    (
        "crlf",
        "PROGRAM p\r\nVAR\r\n\tx : INT;\r\nEND_VAR\r\nx := 1;\r\nEND_PROGRAM\r\n",
    ),
    (
        "lone_carriage_return",
        "PROGRAM p\rVAR x : INT; END_VAR\rx := 1;\rEND_PROGRAM\r",
    ),
    (
        "pragma",
        "{attribute 'strict'}\nPROGRAM p\n{attribute 'x' := 'y'}\nVAR x : INT; END_VAR\nx := 1;\nEND_PROGRAM\n",
    ),
    (
        "pragma_unicode",
        "{attribute 'W\u{e4}rme'}\nPROGRAM p\nEND_PROGRAM\n",
    ),
    ("pragma_unclosed", "TYPE {a b\n T : INT;\nEND_TYPE\n"),
    (
        "pragma_with_lexer_error",
        "TYPE {a ? b}\n T : INT;\nEND_TYPE\n",
    ),
    ("lexer_error", "a := 1 ? 2;"),
    ("lexer_error_multibyte", "a := 1 \u{a4} 2;"),
    (
        "oscat_ascii",
        "(*@KEY@:DESCRIPTION*)\nbody text\n(*@KEY@:END_DESCRIPTION*)\nTYPE\n T : INT;\nEND_TYPE\n",
    ),
    (
        "oscat_unicode",
        "(*@KEY@:DESCRIPTION*)\nW\u{e4}rmebild\n(*@KEY@:END_DESCRIPTION*)\nTYPE\n T : INT;\nEND_TYPE\n",
    ),
    (
        "oscat_two",
        "(*@KEY@:A*)\nfirst\n(*@KEY@:END_A*)\n(*@KEY@:B*)\nsecond body\n(*@KEY@:END_B*)\nTYPE\n T : INT;\nEND_TYPE\n",
    ),
    (
        "duration",
        "PROGRAM p\nVAR t : TIME; END_VAR\nt := T#1m30s;\nt := TIME#-2.5h;\nt := T#1d2h3m4s5ms;\nEND_PROGRAM\n",
    ),
    (
        "end_if_without_semicolon",
        "PROGRAM p\nVAR x : INT; END_VAR\nIF x > 0 THEN\nx := 1;\nEND_IF\nx := 2;\nEND_PROGRAM\n",
    ),
    (
        "end_if_comment_then_semicolon",
        "PROGRAM p\nVAR x : INT; END_VAR\nIF x > 0 THEN x := 1; END_IF (* c *)\n;\nx := 2;\nEND_PROGRAM\n",
    ),
    (
        "end_if_crlf",
        "PROGRAM p\r\nVAR x : INT; END_VAR\r\nIF x > 0 THEN\r\nx := 1;\r\nEND_IF\r\nx := 2;\r\nEND_PROGRAM\r\n",
    ),
    (
        "case_empty_branches",
        "PROGRAM p\nVAR x : INT; y : INT; END_VAR\nCASE x OF\n1:\n2: y := 1;\n3, 4:\nELSE\n5:\nEND_CASE\ny := 0;\nEND_PROGRAM\n",
    ),
    (
        "keyword_statements_nested",
        "PROGRAM p\nVAR x : INT; END_VAR\nWHILE x < 3 DO\nFOR x := 0 TO 2 DO\nx := x + 1;\nEND_FOR\nEND_WHILE\nREPEAT x := 1; UNTIL x > 0 END_REPEAT\nEND_PROGRAM\n",
    ),
    ("empty_source", ""),
    ("whitespace_only", "  \n\t\n"),
];

/// Violations a case is pinned to under `options`, sorted like the actual list.
fn pinned_violations(case: &str, options: &CompilerOptions) -> Vec<Violation> {
    let mut pinned: Vec<Violation> = KNOWN_VIOLATIONS
        .iter()
        .filter(|known| known.case == case)
        .filter(|known| {
            known
                .when_pragmas
                .is_none_or(|pragmas| pragmas == options.allow_pragmas)
        })
        .flat_map(|known| {
            known.ranges.iter().map(|&(start, end)| Violation {
                rule: known.rule,
                start,
                end,
            })
        })
        .collect();
    pinned.sort();
    pinned
}

fn tokens_of(source: &str, options: &CompilerOptions) -> Vec<Token> {
    crate::tokenize_program(source, &FileId::default(), options, 0, 0).0
}

/// Tokenizes `source` under every dialect's options and asserts the actual
/// violations equal the pinned ones exactly.
fn assert_provenance_for_all_dialects(case: &str, source: &str) {
    for dialect in Dialect::ALL {
        let options = CompilerOptions::from_dialect(*dialect);
        let mut actual = check_provenance(source, &tokens_of(source, &options));
        actual.sort();
        assert_eq!(
            actual,
            pinned_violations(case, &options),
            "provenance violations for `{case}` under {dialect:?} differ from the pinned list"
        );
    }
}

fn corpus_files() -> Vec<(String, String)> {
    let dir = ironplc_test::shared_resource_path("");
    let mut files: Vec<(String, String)> = std::fs::read_dir(dir)
        .expect("corpus directory is readable")
        .map(|entry| entry.expect("corpus entry is readable").path())
        .filter(|path| path.extension().is_some_and(|ext| ext == "st"))
        .map(|path| {
            let name = path.file_name().unwrap().to_string_lossy().into_owned();
            let source = std::fs::read_to_string(&path).expect("corpus file is UTF-8");
            (name, source)
        })
        .collect();
    files.sort();
    files
}

fn snippet(case: &str) -> &'static str {
    SNIPPETS
        .iter()
        .find(|(name, _)| *name == case)
        .expect("snippet name is listed in SNIPPETS")
        .1
}

#[test]
fn tokenize_program_when_corpus_file_then_provenance_matches_pinned_exceptions() {
    let files = corpus_files();
    assert!(files.len() > 40, "the corpus should not silently shrink");
    for (name, source) in files {
        assert_provenance_for_all_dialects(&name, &source);
    }
}

#[test]
fn tokenize_program_when_snippet_then_provenance_matches_pinned_exceptions() {
    for (name, source) in SNIPPETS {
        assert_provenance_for_all_dialects(name, source);
    }
}

#[test]
fn known_violations_when_listed_then_each_names_an_existing_case() {
    let corpus = corpus_files();
    for known in KNOWN_VIOLATIONS {
        let exists = SNIPPETS.iter().any(|(name, _)| *name == known.case)
            || corpus.iter().any(|(name, _)| name == known.case);
        assert!(exists, "pinned case `{}` does not exist", known.case);
        assert!(!known.ranges.is_empty());
        assert!(known.finding.starts_with('F') || known.finding.starts_with("not in the audit"));
        assert!(!known.reason.is_empty());
    }
}

#[test]
fn check_provenance_when_text_differs_from_slice_then_reports_mismatch() {
    let mut tokens = tokens_of("x := 1;", &CompilerOptions::default());
    tokens[0].text = "y".to_owned();
    assert_eq!(
        check_provenance("x := 1;", &tokens),
        vec![Violation {
            rule: Rule::TextMismatch,
            start: 0,
            end: 1
        }]
    );
}

#[test]
fn check_provenance_when_token_removed_then_reports_gap() {
    let mut tokens = tokens_of("x := 1;", &CompilerOptions::default());
    tokens.remove(0);
    assert_eq!(
        check_provenance("x := 1;", &tokens),
        vec![Violation {
            rule: Rule::Gap,
            start: 0,
            end: 1
        }]
    );
}

#[test]
fn check_provenance_when_tokens_swapped_then_reports_order_and_overlap() {
    let mut tokens = tokens_of("x := 1;", &CompilerOptions::default());
    tokens.swap(0, 2);
    let rules: Vec<Rule> = check_provenance("x := 1;", &tokens)
        .into_iter()
        .map(|violation| violation.rule)
        .collect();
    assert!(rules.contains(&Rule::Order));
    assert!(rules.contains(&Rule::Overlap));
}

#[test]
fn check_provenance_when_synthetic_token_has_width_then_reports_non_zero_synthetic() {
    let source = snippet("end_if_without_semicolon");
    let mut tokens = tokens_of(source, &CompilerOptions::from_dialect(Dialect::Codesys));
    let synthetic = tokens
        .iter_mut()
        .find(|token| token.text.is_empty())
        .expect("a terminator is inserted after END_IF");
    synthetic.span.end += 1;
    assert!(check_provenance(source, &tokens)
        .iter()
        .any(|violation| violation.rule == Rule::NonZeroSynthetic));
}

#[test]
fn tokenize_program_when_missing_semicolon_allowed_then_terminators_are_inserted_zero_width() {
    // Guards against the zero-width assertions above passing vacuously.
    let options = CompilerOptions::from_dialect(Dialect::Codesys);
    for case in [
        "end_if_without_semicolon",
        "end_if_comment_then_semicolon",
        "end_if_crlf",
        "case_empty_branches",
        "keyword_statements_nested",
    ] {
        let tokens = tokens_of(snippet(case), &options);
        let synthetic: Vec<_> = tokens.iter().filter(|t| t.text.is_empty()).collect();
        assert!(!synthetic.is_empty(), "`{case}` inserts no terminator");
        for token in synthetic {
            assert_eq!(token.span.start, token.span.end, "`{case}`");
        }
    }
}

#[test]
fn tokenize_program_when_duration_literal_then_parts_tile_original_lexeme() {
    // F5: the lexer's single `m30s` token is split into `m` `30` `s`. The
    // split is byte-lossless, so the tiling guard cannot see the lost lexeme
    // identity; this pins the split shape instead, and the parts must still
    // tile the original lexeme exactly.
    let source = "t := T#1m30s;";
    let tokens = tokens_of(source, &CompilerOptions::default());
    let texts: Vec<&str> = tokens
        .iter()
        .filter(|token| token.token_type != TokenType::Whitespace)
        .map(|token| token.text.as_str())
        .collect();
    assert_eq!(
        texts,
        vec!["t", ":=", "T", "#", "1", "m", "30", "s", ";"],
        "F5: duration unit run is split into synthetic parts"
    );
    assert_eq!(check_provenance(source, &tokens), vec![]);
}
