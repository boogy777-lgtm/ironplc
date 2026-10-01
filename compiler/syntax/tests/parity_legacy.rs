//! Parity with the legacy PEG parser, test-only.
//!
//! For a table of statement and expression snippets under every dialect
//! preset, the new parser must report "no syntax errors" exactly when the
//! legacy statement-fragment parser accepts. A difference must be listed in
//! the named exception table with its reason; an unlisted difference fails,
//! and so does a listed one that no longer differs.

mod parity;

use parity::legacy::{presets, Preset};
use parity::tables::{
    BODY_EXCEPTIONS, EXPRESSIONS, EXPRESSION_EXCEPTIONS, STATEMENTS, STATEMENT_EXCEPTIONS,
};
use parity::{extract, verdict, Exception, Kind, Oracle, Verdict};
use std::collections::BTreeMap;

/// The outcome of comparing a table of snippets.
struct Report {
    compared: usize,
    agreements: usize,
    both_accept: usize,
    both_reject: usize,
    unexplained: Vec<String>,
    /// Each listed difference with the number of presets it occurred under.
    explained: BTreeMap<String, usize>,
    stale: Vec<String>,
}

fn compare(
    kind: Kind,
    oracle: Oracle,
    snippets: &[&str],
    presets: &[Preset],
    exceptions: &[Exception],
) -> Report {
    let mut report = Report {
        compared: 0,
        agreements: 0,
        both_accept: 0,
        both_reject: 0,
        unexplained: Vec::new(),
        explained: BTreeMap::new(),
        stale: Vec::new(),
    };
    let mut used = vec![false; exceptions.len()];
    for snippet in snippets {
        for preset in presets {
            let verdict = verdict(kind, snippet, preset);
            report.compared += 1;
            if verdict.legacy_for(oracle) == verdict.new {
                report.agreements += 1;
                if verdict.new {
                    report.both_accept += 1;
                } else {
                    report.both_reject += 1;
                }
                continue;
            }
            let covering: Vec<usize> = exceptions
                .iter()
                .enumerate()
                .filter(|(_, exception)| {
                    exception.covers(kind, snippet, &preset.name, oracle, &verdict)
                })
                .map(|(index, _)| index)
                .collect();
            for index in &covering {
                used[*index] = true;
                let exception = &exceptions[*index];
                if !exception.basis_holds(&verdict) {
                    report.unexplained.push(format!(
                        "{} -- blamed on the fragment entry, but the program-wrapped legacy parser differs from the new parser",
                        describe(kind, snippet, &preset.name, &verdict)
                    ));
                }
                let key = format!("{kind:?} {snippet:?} -- {}", exception.reason);
                *report.explained.entry(key).or_insert(0) += 1;
            }
            if covering.is_empty() {
                report
                    .unexplained
                    .push(describe(kind, snippet, &preset.name, &verdict));
            }
        }
    }
    for (index, exception) in exceptions.iter().enumerate() {
        if exception.kind == kind && !used[index] {
            report
                .stale
                .push(format!("{:?} {:?}", exception.kind, exception.snippet));
        }
    }
    report
}

fn describe(kind: Kind, snippet: &str, preset: &str, verdict: &Verdict) -> String {
    format!(
        "{kind:?} {snippet:?} under {preset}: legacy {} program {} new {}",
        verdict.legacy, verdict.program, verdict.new
    )
}

fn summarize(name: &str, snippets: usize, report: &Report) {
    println!(
        "{name}: {snippets} snippets, {} comparisons, {} agree ({} accept, {} reject), {} listed differences",
        report.compared,
        report.agreements,
        report.both_accept,
        report.both_reject,
        report.explained.len()
    );
    for (difference, presets) in &report.explained {
        println!("  [{presets} presets] {difference}");
    }
}

fn assert_clean(report: &Report) {
    // A table in which both parsers reject everything, or accept everything,
    // would agree trivially.
    assert!(
        report.both_accept > 100,
        "{} agreed accepts",
        report.both_accept
    );
    assert!(
        report.both_reject > 20,
        "{} agreed rejects",
        report.both_reject
    );
    assert!(
        report.unexplained.is_empty(),
        "{} unexplained differences of {} comparisons:\n{}",
        report.unexplained.len(),
        report.compared,
        report.unexplained.join("\n")
    );
    assert!(
        report.stale.is_empty(),
        "exceptions that no longer differ:\n{}",
        report.stale.join("\n")
    );
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
        STATEMENTS,
        &presets(),
        STATEMENT_EXCEPTIONS,
    );
    summarize("statements", STATEMENTS.len(), &report);
    assert_clean(&report);
}

#[test]
fn parity_when_expression_table_then_differences_are_exactly_the_exceptions() {
    let report = compare(
        Kind::Expression,
        Oracle::Fragment,
        EXPRESSIONS,
        &presets(),
        EXPRESSION_EXCEPTIONS,
    );
    summarize("expressions", EXPRESSIONS.len(), &report);
    assert_clean(&report);
}

#[test]
fn parity_when_legacy_test_bodies_then_differences_are_exactly_the_exceptions() {
    let bodies = extract::legacy_test_bodies();
    assert!(bodies.len() > 100, "only {} bodies extracted", bodies.len());
    let snippets: Vec<&str> = bodies.iter().map(String::as_str).collect();
    let report = compare(
        Kind::Statements,
        Oracle::Program,
        &snippets,
        &presets(),
        BODY_EXCEPTIONS,
    );
    summarize("legacy test bodies", snippets.len(), &report);
    assert_clean(&report);
}
