//! Parity of the reported diagnostic: the one diagnostic the legacy parser
//! reports against the primary diagnostic of the new parser.
//!
//! The legacy parser reports the first problem it finds: its code, where it is
//! and what it says. The new parser reports every problem and ranks them
//! (`Parse::primary_diagnostic`). For every input both reject, the two
//! diagnostics must have the same code and the same byte range, or the
//! difference must be a row of `CODE_EXCEPTIONS` with its class and reason.
//! What they say is compared separately (`diagnostics_messages.rs`).
//!
//! A row describes a kind of difference, not an input: the two codes, how the
//! new range lies against the legacy one (`Relation`) and, when the reason is
//! about one place of the grammar, what the new diagnostic says there (its
//! `site`). The rows are counted: an unlisted difference fails, and so does a
//! row whose count is not the number observed.

use super::legacy::{rejection, Preset};
use super::literals::{corpus, Case};
use super::{Kind, Reason};
use ironplc_dsl::core::FileId;
use ironplc_syntax::lower::lower_library;
use ironplc_syntax::parse_source_file;

/// One diagnostic as the comparison reads it.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Reported {
    pub code: String,
    pub start: usize,
    pub end: usize,
    pub message: String,
}

impl Reported {
    fn of(diagnostic: ironplc_dsl::diagnostic::Diagnostic) -> Self {
        Reported {
            code: diagnostic.code.clone(),
            start: diagnostic.primary.location.start,
            end: diagnostic.primary.location.end,
            message: diagnostic.primary.message,
        }
    }
}

/// The diagnostic the legacy parser reports for the file `text`.
pub fn legacy_reported(text: &str, preset: &Preset) -> Option<Reported> {
    rejection(text, &preset.legacy).map(|(code, start, end)| Reported {
        code,
        start,
        end,
        message: legacy_message(text, preset),
    })
}

fn legacy_message(text: &str, preset: &Preset) -> String {
    crate::legacy::parse_program(text, &FileId::default(), &preset.legacy)
        .err()
        .map(|diagnostic| diagnostic.primary.message)
        .unwrap_or_default()
}

/// The diagnostic the new parser and the lowering report for the file `text`:
/// the primary diagnostic of the parse, or the first of the lowering when the
/// parse has none.
pub fn new_reported(text: &str, preset: &Preset) -> Option<Reported> {
    let parse = parse_source_file(text, &preset.new);
    lower_library(&parse, &FileId::default())
        .err()
        .map(Reported::of)
}

/// How the range of the new diagnostic lies against the legacy one.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Relation {
    /// The same bytes.
    Same,
    /// The new range is empty: what is missing is reported where it belongs.
    Empty,
    /// The new range starts where the legacy one does and goes further.
    Wider,
    /// The new range starts where the legacy one does and stops sooner.
    Narrower,
    /// The new range starts before the legacy one.
    Earlier,
    /// The new range starts after the legacy one.
    Later,
}

impl Relation {
    pub fn between(legacy: &Reported, new: &Reported) -> Relation {
        if (legacy.start, legacy.end) == (new.start, new.end) {
            Relation::Same
        } else if new.start == new.end {
            Relation::Empty
        } else if new.start == legacy.start {
            if new.end > legacy.end {
                Relation::Wider
            } else {
                Relation::Narrower
            }
        } else if new.start < legacy.start {
            Relation::Earlier
        } else {
            Relation::Later
        }
    }
}

/// What a diagnostic says at a place of the grammar, with the source text it
/// quotes left out: text in backticks or in single quotes becomes `_`, so
/// ``expected `END_VAR` `` and ``expected `;` `` are one site, and so are the
/// messages that name two different literals.
pub fn site(message: &str) -> String {
    let mut site = String::new();
    let mut closing = None;
    for c in message.chars() {
        match (closing, c) {
            (None, '`' | '\'') => {
                closing = Some(c);
                site.push(c);
                site.push('_');
                site.push(c);
            }
            (None, _) => site.push(c),
            (Some(delimiter), _) if c == delimiter => closing = None,
            (Some(_), _) => {}
        }
    }
    site
}

/// A kind of difference between the legacy diagnostic and the primary
/// diagnostic of the new parser, with the reason it exists and how often the
/// corpus shows it. A difference is covered by the first row that fits it.
pub struct CodeException {
    pub legacy_code: &'static str,
    pub new_code: &'static str,
    /// How the range lies, or any range when `None`.
    pub relation: Option<Relation>,
    /// What the new diagnostic says (see [`site`]) at one of these places of
    /// the grammar, or anywhere when the list is empty.
    pub sites: &'static [&'static str],
    pub reason: Reason,
    pub expected: usize,
}

impl CodeException {
    fn covers(&self, legacy: &Reported, new: &Reported) -> bool {
        self.legacy_code == legacy.code
            && self.new_code == new.code
            && self
                .relation
                .is_none_or(|relation| relation == Relation::between(legacy, new))
            && (self.sites.is_empty() || self.sites.contains(&site(&new.message).as_str()))
    }
}

/// The files the comparison runs over: every whole input of the corpus. A
/// statement list or an expression is put in a `PROGRAM`, as the legacy parser
/// reads it there, and the key is the snippet.
pub fn whole_inputs() -> Vec<(String, String)> {
    let mut seen = std::collections::HashSet::new();
    corpus()
        .into_iter()
        .map(|case| (case.key.clone(), whole(&case)))
        .filter(|(_, text)| seen.insert(text.clone()))
        .collect()
}

fn whole(case: &Case) -> String {
    match case.kind {
        Kind::Statements => format!("PROGRAM p\n{}\nEND_PROGRAM\n", case.text),
        Kind::Expression => format!("PROGRAM p\nx := {};\nEND_PROGRAM\n", case.text),
        Kind::Declarations | Kind::File => case.text.clone(),
    }
}

/// The outcome of comparing the diagnostics over inputs.
#[derive(Default)]
pub struct DiagnosticReport {
    /// Inputs both parsers reject, under each preset.
    pub both_reject: usize,
    /// Those with the same code and the same range.
    pub equal: usize,
    /// Differences covered by a row, by row.
    pub excepted: Vec<usize>,
    /// Differences no row covers: what was found, and one example.
    pub unexplained: Vec<String>,
    /// Rows whose count is not the number observed.
    pub miscounted: Vec<String>,
}

/// Compares the legacy diagnostic with the new primary diagnostic for every
/// input both reject.
pub fn compare_diagnostics(
    inputs: &[(String, String)],
    presets: &[Preset],
    exceptions: &[CodeException],
) -> DiagnosticReport {
    let mut report = DiagnosticReport {
        excepted: vec![0; exceptions.len()],
        ..DiagnosticReport::default()
    };
    let mut classes: Vec<(String, usize, String)> = Vec::new();
    for (key, text) in inputs {
        for preset in presets {
            let Some(legacy) = legacy_reported(text, preset) else {
                continue;
            };
            let Some(new) = new_reported(text, preset) else {
                continue;
            };
            report.both_reject += 1;
            if (&legacy.code, legacy.start, legacy.end) == (&new.code, new.start, new.end) {
                report.equal += 1;
                continue;
            }
            match exceptions.iter().position(|row| row.covers(&legacy, &new)) {
                Some(index) => report.excepted[index] += 1,
                None => {
                    let class = format!(
                        "legacy {} new {} range {:?} site {:?}",
                        legacy.code,
                        new.code,
                        Relation::between(&legacy, &new),
                        site(&new.message)
                    );
                    match classes.iter_mut().find(|(seen, _, _)| *seen == class) {
                        Some((_, count, _)) => *count += 1,
                        None => classes.push((
                            class,
                            1,
                            format!(
                                "{key:?} under {}: legacy {:?} new {:?}",
                                preset.name, legacy, new
                            ),
                        )),
                    }
                }
            }
        }
    }
    report.unexplained = classes
        .into_iter()
        .map(|(class, count, example)| format!("{class} x{count}, first {example}"))
        .collect();
    for (row, seen) in exceptions.iter().zip(&report.excepted) {
        if *seen != row.expected {
            report.miscounted.push(format!(
                "legacy {} new {} {:?} {:?}: recorded {}, observed {seen} ({})",
                row.legacy_code, row.new_code, row.relation, row.sites, row.expected, row.reason
            ));
        }
    }
    report
}
