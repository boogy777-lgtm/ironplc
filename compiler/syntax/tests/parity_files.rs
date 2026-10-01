//! Whole-file parity with the legacy PEG parser, test-only.
//!
//! For every file of the shared test resources and of the crate's fixtures,
//! and for a CRLF and a tab-indented spelling of each, under every dialect
//! preset, the new parser must report no syntax errors exactly when the legacy
//! `parse_program` accepts the file. A difference must be listed in
//! `FILE_EXCEPTIONS` with its reason; an unlisted difference fails, and so
//! does a listed one that no longer differs.

mod common;
mod parity;

use parity::compare::{assert_clean, compare, summarize, Item};
use parity::legacy::presets;
use parity::tables::FILE_EXCEPTIONS;
use parity::{Kind, Oracle};
use std::path::Path;

#[test]
fn parity_when_whole_files_then_differences_are_exactly_the_exceptions() {
    let files = common::file_variants();
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
fn parity_when_ranged_comment_is_blanked_then_the_new_parser_accepts_the_oscat_file() {
    // The legacy preprocessor blanks the text between a ranged-comment marker
    // pair. Doing the same here shows that nothing else in the file keeps the
    // new parser from accepting it.
    let source = std::fs::read_to_string(
        Path::new(env!("CARGO_MANIFEST_DIR")).join("../resources/test/oscat.st"),
    )
    .unwrap_or_default();
    let open = "(*@KEY@:DESCRIPTION*)";
    let close = "(*@KEY@:END_DESCRIPTION*)";
    let (start, end) = (source.find(open), source.find(close));
    assert!(
        start.is_some() && end.is_some(),
        "oscat.st lost its ranged comment"
    );
    let (start, end) = (start.unwrap_or(0), end.unwrap_or(0));
    let blanked: String = source
        .char_indices()
        .map(|(index, c)| {
            if index >= start + open.len() && index < end && c != '\n' {
                ' '
            } else {
                c
            }
        })
        .collect();
    for preset in presets() {
        let parsed = ironplc_syntax::parse_source_file(&blanked, &preset.new);
        let legacy = parity::legacy::accepts_file(&source, &preset.legacy);
        assert_eq!(
            parsed.is_ok(),
            legacy,
            "{}: {:?}",
            preset.name,
            parsed.errors
        );
    }
}
