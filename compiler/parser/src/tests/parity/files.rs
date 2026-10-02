//! Whole-file parity with the legacy PEG parser, test-only.
//!
//! For every file of the shared test resources and of the crate's fixtures,
//! and for a CRLF and a tab-indented spelling of each, under every dialect
//! preset, the new parser must report no syntax errors exactly when the legacy
//! `parse_program` accepts the file. A difference must be listed in
//! `FILE_EXCEPTIONS` with its reason; an unlisted difference fails, and so
//! does a listed one that no longer differs.

use super::compare::{assert_clean, compare, summarize, Item};
use super::file_variants;
use super::legacy::presets;
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
