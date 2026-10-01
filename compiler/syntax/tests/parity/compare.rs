//! Comparing a table of inputs against the legacy parser and reporting.
//!
//! The same comparison serves every table: statement snippets, expression
//! snippets, declaration snippets and whole files. A difference must be in the
//! exception table with its reason; an unlisted difference fails, and so does
//! a listed one that no longer differs.

use super::legacy::Preset;
use super::{verdict, Exception, Kind, Oracle, Verdict};
use std::collections::BTreeMap;

/// One input: the key an exception names it by, and its text. For a snippet
/// the two are the same; for a file the key is its name.
#[derive(Clone, Copy)]
pub struct Item<'a> {
    pub key: &'a str,
    pub text: &'a str,
}

impl<'a> Item<'a> {
    pub fn snippet(text: &'a str) -> Self {
        Item { key: text, text }
    }
}

/// The outcome of comparing a table.
pub struct Report {
    pub compared: usize,
    pub agreements: usize,
    pub both_accept: usize,
    pub both_reject: usize,
    pub unexplained: Vec<String>,
    /// Each listed difference with the number of presets it occurred under.
    pub explained: BTreeMap<String, usize>,
    pub stale: Vec<String>,
}

pub fn compare(
    kind: Kind,
    oracle: Oracle,
    items: &[Item],
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
    for item in items {
        for preset in presets {
            let verdict = verdict(kind, item.text, preset);
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
                    exception.covers(kind, item.key, &preset.name, oracle, &verdict)
                })
                .map(|(index, _)| index)
                .collect();
            for index in &covering {
                used[*index] = true;
                let exception = &exceptions[*index];
                if !exception.basis_holds(&verdict) {
                    report.unexplained.push(format!(
                        "{} -- blamed on the fragment entry, but the program-wrapped legacy parser differs from the new parser",
                        describe(kind, item.key, &preset.name, &verdict)
                    ));
                }
                let key = format!("{kind:?} {:?} -- {}", item.key, exception.reason);
                *report.explained.entry(key).or_insert(0) += 1;
            }
            if covering.is_empty() {
                report
                    .unexplained
                    .push(describe(kind, item.key, &preset.name, &verdict));
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

fn describe(kind: Kind, key: &str, preset: &str, verdict: &Verdict) -> String {
    format!(
        "{kind:?} {key:?} under {preset}: legacy {} program {} new {}",
        verdict.legacy, verdict.program, verdict.new
    )
}

pub fn summarize(name: &str, inputs: usize, report: &Report) {
    println!(
        "{name}: {inputs} inputs, {} comparisons, {} agree ({} accept, {} reject), {} listed differences",
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

/// Fails unless every difference is listed and every listed difference
/// exists. `min_accept` and `min_reject` guard against a table on which the
/// two parsers agree only because both reject (or both accept) everything.
pub fn assert_clean(report: &Report, min_accept: usize, min_reject: usize) {
    assert!(
        report.both_accept >= min_accept,
        "{} agreed accepts, expected at least {min_accept}",
        report.both_accept
    );
    assert!(
        report.both_reject >= min_reject,
        "{} agreed rejects, expected at least {min_reject}",
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
