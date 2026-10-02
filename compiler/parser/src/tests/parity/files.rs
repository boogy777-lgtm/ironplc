//! Whole-file parity with the legacy PEG parser, test-only.
//!
//! For every file of the corpus (`file_variants`),
//! and for a CRLF and a tab-indented spelling of each, under every dialect
//! preset, the new parser must report no syntax errors exactly when the legacy
//! `parse_program` accepts the file. A difference must be listed in
//! `FILE_EXCEPTIONS` with its reason; an unlisted difference fails, and so
//! does a listed one that no longer differs.

use super::compare::{assert_clean, compare, summarize, Item};
use super::file_variants;
use super::legacy::{presets, Preset};
use super::tables::FILE_EXCEPTIONS;
use super::{Kind, Oracle};
use std::path::Path;

#[test]
fn parity_when_whole_files_then_differences_are_exactly_the_exceptions() {
    let files = file_variants();
    assert!(files.len() > 180, "only {} files", files.len());
    let items: Vec<Item> = files
        .iter()
        .map(|(name, text)| Item { key: name, text })
        .collect();
    let report = compare(
        Kind::File,
        Oracle::Program,
        &items,
        &presets(),
        FILE_EXCEPTIONS,
    );
    summarize("whole files", files.len(), &report);
    assert_clean(&report, 300, 150);
}

#[test]
fn parity_when_oscat_file_then_ranged_comments_are_regions_and_both_parsers_accept() {
    // The legacy preprocessor blanks the text between a ranged-comment marker
    // pair; the new parser keeps it as one trivia token and skips it.
    let source = std::fs::read_to_string(
        Path::new(env!("CARGO_MANIFEST_DIR")).join("../resources/test/oscat.st"),
    )
    .unwrap_or_default();
    assert!(
        source.contains("(*@KEY@:DESCRIPTION*)"),
        "oscat.st lost its ranged comment"
    );
    for preset in presets() {
        let parsed = ironplc_syntax::parse_source_file(&source, &preset.new);
        let legacy = super::legacy::accepts_file(&source, &preset.legacy);
        assert!(legacy, "{}: legacy rejects oscat.st", preset.name);
        assert!(parsed.is_ok(), "{}: {:?}", preset.name, parsed.errors);
        assert_eq!(parsed.root.text().to_string(), source, "{}", preset.name);
        let regions = parsed
            .root
            .descendants_with_tokens()
            .filter(|element| element.kind() == ironplc_syntax::SyntaxKind::RangedComment)
            .count();
        assert!(regions > 0, "{}: no ranged comment region", preset.name);
    }
}

fn lexical_fixture(name: &str) -> String {
    std::fs::read_to_string(
        Path::new(env!("CARGO_MANIFEST_DIR"))
            .join("../syntax/tests/fixtures/lexical")
            .join(name),
    )
    .unwrap_or_default()
}

fn ranged_comments(parsed: &ironplc_syntax::Parse) -> usize {
    parsed
        .root
        .descendants_with_tokens()
        .filter(|element| element.kind() == ironplc_syntax::SyntaxKind::RangedComment)
        .count()
}

/// The statement count of the first program of the library the legacy parser
/// builds for `source`.
fn legacy_program_statements(source: &str, preset: &Preset) -> Option<usize> {
    use ironplc_dsl::common::{FunctionBlockBodyKind, LibraryElementKind};
    let library = crate::legacy::parse_program(
        source,
        &ironplc_dsl::core::FileId::default(),
        &preset.legacy,
    )
    .ok()?;
    library.elements.iter().find_map(|element| match element {
        LibraryElementKind::ProgramDeclaration(program) => match &program.body {
            FunctionBlockBodyKind::Statements(statements) => Some(statements.body.len()),
            _ => None,
        },
        _ => None,
    })
}

#[test]
fn parity_when_several_oscat_pairs_then_every_pair_is_a_region_and_legacy_blanks_only_the_first() {
    // Deliberate difference: the legacy pre-pass blanks the first marker pair
    // only, so the body of the second pair reaches the legacy lexer and is
    // rejected; the new parser makes each of the three pairs a region. The
    // difference is listed in `FILE_EXCEPTIONS` for the file and its CRLF and
    // tab spellings.
    let source = lexical_fixture("oscat_several_pairs.st");
    assert_eq!(source.matches("(*@KEY@:END_").count(), 3);
    for preset in presets() {
        let parsed = ironplc_syntax::parse_source_file(&source, &preset.new);
        assert!(parsed.is_ok(), "{}: {:?}", preset.name, parsed.errors);
        assert_eq!(parsed.root.text().to_string(), source, "{}", preset.name);
        assert_eq!(ranged_comments(&parsed), 3, "{}", preset.name);
        let rejection = super::legacy::rejection(&source, &preset.legacy);
        assert_eq!(
            rejection.map(|(code, _, _)| code),
            Some("P0003".to_string()),
            "{}: legacy should reject the unblanked second body",
            preset.name
        );
    }
}

#[test]
fn parity_when_oscat_marker_inside_string_then_both_accept_but_only_the_new_parser_keeps_the_code()
{
    // The marker text sits inside a string literal. The legacy pre-pass
    // matches raw text, takes the first string's `(*@KEY@:X*)` as the opening
    // marker and the second string's `(*@KEY@:END_X*)` as the closing one, and
    // blanks the live statement between them: it accepts the file (the
    // blanked text stays inside one string) with one statement. The new
    // parser lexes the strings first, finds no marker and keeps all three
    // statements. The two agree on accept/reject, so the difference is
    // asserted here, not listed.
    let source = lexical_fixture("oscat_marker_in_string.st");
    for preset in presets() {
        let parsed = ironplc_syntax::parse_source_file(&source, &preset.new);
        assert!(parsed.is_ok(), "{}: {:?}", preset.name, parsed.errors);
        assert_eq!(parsed.root.text().to_string(), source, "{}", preset.name);
        assert_eq!(ranged_comments(&parsed), 0, "{}", preset.name);
        let assignments = parsed
            .root
            .descendants()
            .filter(|node| node.kind() == ironplc_syntax::SyntaxKind::AssignStmt)
            .count();
        assert_eq!(assignments, 3, "{}", preset.name);
        assert_eq!(
            legacy_program_statements(&source, &preset),
            Some(1),
            "{}: legacy blanks the statement between the strings",
            preset.name
        );
    }
}
