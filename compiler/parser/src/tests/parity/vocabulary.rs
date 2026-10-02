//! Every keyword and operator spelling the legacy lexer recognises must get a
//! non-error kind here. The spellings are read from the legacy token
//! definitions (`token.rs`) rather than retyped.

use ironplc_syntax::lexer::{check_coverage, lex};
use ironplc_syntax::SyntaxKind;

/// The `(kind, text)` pairs of `source` without trivia, after checking the
/// token stream tiles the source exactly.
fn significant(source: &str) -> Vec<(SyntaxKind, &str)> {
    let (tokens, _) = lex(source);
    assert!(check_coverage(source, &tokens).is_exact(), "{source:?}");
    tokens
        .into_iter()
        .filter(|token| !token.kind.is_trivia())
        .map(|token| (token.kind, token.text))
        .collect()
}

const LEGACY_TOKENS: &str = include_str!("../../token.rs");

/// The string argument of every `#[token("...")]` attribute in the legacy
/// token definitions.
fn legacy_spellings() -> Vec<&'static str> {
    LEGACY_TOKENS
        .lines()
        .filter_map(|line| {
            let rest = line.trim_start().strip_prefix("#[token(\"")?;
            Some(&rest[..rest.find('"')?])
        })
        .collect()
}

#[test]
fn legacy_spellings_when_extracted_then_vocabulary_is_plausibly_complete() {
    let spellings = legacy_spellings();
    assert!(spellings.len() > 150, "found {}", spellings.len());
    for expected in ["END_PROGRAM", ":=", "|", "&", "TOD", "__TRY", "MOD"] {
        assert!(spellings.contains(&expected), "missing {expected}");
    }
}

#[test]
fn lex_when_legacy_token_spelling_then_single_non_error_token() {
    for spelling in legacy_spellings() {
        // A lone `{` always opens a pragma; unterminated, it is an error
        // token here (the legacy lexer pairs it with `}` in a later pass).
        if spelling == "{" {
            continue;
        }
        let tokens = significant(spelling);
        assert_eq!(tokens.len(), 1, "{spelling:?} -> {tokens:?}");
        assert_ne!(tokens[0].0, SyntaxKind::ErrorToken, "{spelling:?}");
        assert_eq!(tokens[0].1, spelling);
    }
}

#[test]
fn lex_when_legacy_word_spelling_then_keyword_in_every_case() {
    for spelling in legacy_spellings() {
        if !spelling.starts_with(|c: char| c.is_ascii_alphabetic() || c == '_') {
            continue;
        }
        let expected = significant(spelling)[0].0;
        assert!(expected.is_keyword(), "{spelling} is not a keyword kind");
        for variant in [spelling.to_ascii_lowercase(), spelling.to_ascii_uppercase()] {
            assert_eq!(significant(&variant)[0].0, expected, "{variant}");
        }
    }
}

#[test]
fn lex_when_keyword_table_then_every_spelling_lexes_to_its_kind() {
    for (spelling, kind) in SyntaxKind::KEYWORDS {
        assert_eq!(significant(spelling), vec![(*kind, *spelling)]);
    }
}

#[test]
fn lex_when_legacy_regex_token_examples_then_non_error_kinds() {
    // The legacy tokens defined by regex rather than by fixed spelling.
    let cases = [
        ("name", SyntaxKind::Ident),
        ("`name`", SyntaxKind::EscapedIdent),
        ("16#FF", SyntaxKind::HexLit),
        ("8#7", SyntaxKind::OctLit),
        ("2#1", SyntaxKind::BinLit),
        ("1.5e3", SyntaxKind::FloatingPointLit),
        ("1.5", SyntaxKind::FixedPointLit),
        ("15", SyntaxKind::IntegerLit),
        ("'s'", SyntaxKind::StringLit),
        ("\"s\"", SyntaxKind::WStringLit),
        ("%I*", SyntaxKind::DirectAddressIncomplete),
        ("%I0.0", SyntaxKind::DirectAddress),
        ("%X1", SyntaxKind::PartialAccess),
        ("%B1", SyntaxKind::PartialAccess),
        ("%W1", SyntaxKind::PartialAccess),
        ("%D1", SyntaxKind::PartialAccess),
        ("%L1", SyntaxKind::PartialAccess),
    ];
    for (text, kind) in cases {
        assert_eq!(significant(text), vec![(kind, text)], "{text}");
    }
}
