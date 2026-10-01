//! Whole-file parity with the legacy PEG parser, test-only.
//!
//! For every file of the shared test resources and of the crate's fixtures,
//! under every dialect preset, the new parser must report no syntax errors
//! exactly when the legacy `parse_program` accepts the file. A difference must
//! be listed in `FILE_EXCEPTIONS` with its reason; an unlisted difference
//! fails, and so does a listed one that no longer differs.

mod common;
mod parity;

use parity::compare::{assert_clean, compare, summarize, Item};
use parity::legacy::presets;
use parity::tables::FILE_EXCEPTIONS;
use parity::{Kind, Oracle};
use std::path::{Path, PathBuf};

/// The files compared, named by their path from the crate manifest's
/// directory (`../resources/test/oop.st`, `tests/fixtures/codesys/x.st`).
fn files() -> Vec<(String, String)> {
    let manifest = Path::new(env!("CARGO_MANIFEST_DIR"));
    let mut paths: Vec<PathBuf> = common::st_files("../resources/test");
    paths.extend(common::st_files("tests/fixtures"));
    paths
        .into_iter()
        .map(|path| {
            let text = std::fs::read_to_string(&path).unwrap_or_default();
            let name = path
                .strip_prefix(manifest)
                .unwrap_or(&path)
                .to_string_lossy()
                .replace('\\', "/");
            (name, text)
        })
        .collect()
}

#[test]
fn parity_when_whole_files_then_differences_are_exactly_the_exceptions() {
    let files = files();
    assert!(files.len() > 60, "only {} files", files.len());
    let items: Vec<Item> = files
        .iter()
        .map(|(name, text)| Item {
            key: name,
            text,
        })
        .collect();
    let report = compare(
        Kind::File,
        Oracle::Program,
        &items,
        &presets(),
        FILE_EXCEPTIONS,
    );
    summarize("whole files", files.len(), &report);
    assert_clean(&report, 100, 60);
}
