//! Declaration parity with the legacy PEG parser, test-only.
//!
//! Two tables of whole-file snippets, each under every dialect preset: the
//! declarations the legacy parser's own tests contain, lifted from their
//! sources, and a table written for this slice with one row per declaration
//! form, accepted and rejected. The new parser must report no syntax errors
//! exactly when the legacy `parse_program` accepts the snippet. A difference
//! must be listed in the named exception table with its reason; an unlisted
//! difference fails, and so does a listed one that no longer differs.

mod parity;

use parity::compare::{assert_clean, compare, summarize, Item};
use parity::declaration_table::DECLARATIONS;
use parity::legacy::presets;
use parity::tables::{DECLARATION_EXCEPTIONS, LEGACY_DECLARATION_EXCEPTIONS};
use parity::{extract, Kind, Oracle};

#[test]
fn parity_when_declaration_table_then_differences_are_exactly_the_exceptions() {
    let items: Vec<Item> = DECLARATIONS
        .iter()
        .map(|text| Item::snippet(text))
        .collect();
    let report = compare(
        Kind::Declarations,
        Oracle::Program,
        &items,
        &presets(),
        DECLARATION_EXCEPTIONS,
    );
    summarize("declarations", DECLARATIONS.len(), &report);
    assert_clean(&report, 300, 300);
}

#[test]
fn parity_when_legacy_test_declarations_then_differences_are_exactly_the_exceptions() {
    let snippets = extract::legacy_declaration_snippets();
    assert!(
        snippets.len() > 200,
        "only {} declaration snippets extracted",
        snippets.len()
    );
    let items: Vec<Item> = snippets.iter().map(|text| Item::snippet(text)).collect();
    let report = compare(
        Kind::Declarations,
        Oracle::Program,
        &items,
        &presets(),
        LEGACY_DECLARATION_EXCEPTIONS,
    );
    summarize("legacy test declarations", snippets.len(), &report);
    assert_clean(&report, 300, 100);
}
