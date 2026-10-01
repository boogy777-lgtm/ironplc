//! Parity with the legacy PEG parser on statements and expressions,
//! test-only.
//!
//! For a table of statement and expression snippets under every dialect
//! preset, the new parser must report "no syntax errors" exactly when the
//! legacy statement-fragment parser accepts. A difference must be listed in
//! the named exception table with its reason; an unlisted difference fails,
//! and so does a listed one that no longer differs. Declarations and whole
//! files are compared in `parity_declarations.rs` and `parity_files.rs`.

mod parity;

use parity::compare::{assert_clean, compare, summarize, Item};
use parity::legacy::presets;
use parity::tables::{
    BODY_EXCEPTIONS, EXPRESSIONS, EXPRESSION_EXCEPTIONS, STATEMENTS, STATEMENT_EXCEPTIONS,
};
use parity::{extract, Kind, Oracle};

fn items<'a>(snippets: &[&'a str]) -> Vec<Item<'a>> {
    snippets.iter().map(|text| Item::snippet(text)).collect()
}

#[test]
fn convert_when_legacy_options_then_every_new_flag_has_a_legacy_counterpart() {
    let legacy_keys: Vec<&str> = ironplc_parser::options::CompilerOptions::FEATURE_DESCRIPTORS
        .iter()
        .map(|descriptor| descriptor.option_key)
        .collect();
    for key in ironplc_syntax::ParseOptions::FLAG_KEYS {
        assert!(legacy_keys.contains(key), "{key} has no legacy flag");
    }
}

#[test]
fn convert_when_dialect_preset_then_flags_match_the_legacy_preset() {
    let presets = presets();
    let codesys = presets.iter().find(|preset| preset.name == "codesys");
    let strict = presets
        .iter()
        .find(|preset| preset.name == "iec61131-3-ed2");
    assert!(codesys.is_some_and(|preset| preset.new.allow_try_catch));
    assert!(strict.is_some_and(|preset| !preset.new.allow_try_catch));
    assert_eq!(presets.len(), 6);
}

#[test]
fn parity_when_statement_table_then_differences_are_exactly_the_exceptions() {
    let report = compare(
        Kind::Statements,
        Oracle::Fragment,
        &items(STATEMENTS),
        &presets(),
        STATEMENT_EXCEPTIONS,
    );
    summarize("statements", STATEMENTS.len(), &report);
    assert_clean(&report, 100, 20);
}

#[test]
fn parity_when_expression_table_then_differences_are_exactly_the_exceptions() {
    let report = compare(
        Kind::Expression,
        Oracle::Fragment,
        &items(EXPRESSIONS),
        &presets(),
        EXPRESSION_EXCEPTIONS,
    );
    summarize("expressions", EXPRESSIONS.len(), &report);
    assert_clean(&report, 100, 20);
}

#[test]
fn parity_when_legacy_test_bodies_then_differences_are_exactly_the_exceptions() {
    let bodies = extract::legacy_test_bodies();
    assert!(bodies.len() > 100, "only {} bodies extracted", bodies.len());
    let snippets: Vec<&str> = bodies.iter().map(String::as_str).collect();
    let report = compare(
        Kind::Statements,
        Oracle::Program,
        &items(&snippets),
        &presets(),
        BODY_EXCEPTIONS,
    );
    summarize("legacy test bodies", snippets.len(), &report);
    assert_clean(&report, 100, 20);
}
