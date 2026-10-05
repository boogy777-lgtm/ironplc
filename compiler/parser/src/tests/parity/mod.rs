//! Test-only comparison of the new parser against the legacy PEG parser.
//!
//! The new parser is not wired into any production consumer; these tests
//! are how its accepted language is held to the legacy one until the legacy
//! path is replaced. The oracle is `parse_st_statements`, the fragment entry
//! the legacy crate exposes for ST bodies. That entry has artifacts of its
//! own (it rejects a trailing comment or pragma, and a block statement that
//! ends the input without a `;`), so each snippet is also run inside a
//! minimal `PROGRAM`, where the legacy parser behaves as it does on real
//! files; an exception that blames the fragment entry must be confirmed by
//! that second verdict.

mod blocks;
mod code_exceptions;
mod combinations;
mod declarations;
mod diagnostics_codes;
mod diagnostics_messages;
mod diagnostics_order;
mod differences;
mod edges;
mod files;
mod hand_built;
mod legacy_options;
mod literals;
mod sites;
mod vocabulary;

pub mod ast;
pub mod compare;
pub mod declaration_table;
pub mod diagnostics;
pub mod extract;
pub mod legacy;
pub mod tables;
pub mod type_table;

use ironplc_syntax::{parse_expression, parse_source_file, parse_statements, Parse, ParseOptions};
use legacy::Preset;

/// Which kind of snippet a table holds.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Kind {
    /// A statement list.
    Statements,
    /// One expression; the legacy side wraps it as `x := <expr>;`.
    Expression,
    /// Whole-file text holding declarations; the oracle is `parse_program`.
    Declarations,
    /// A source file, named by its path; the oracle is `parse_program`.
    File,
}

/// What each parser says about one snippet under one preset.
#[derive(Debug, PartialEq, Eq, Clone)]
pub struct Verdict {
    /// The legacy fragment entry.
    pub legacy: bool,
    /// The legacy parser on the snippet inside a `PROGRAM`.
    pub program: bool,
    /// The new parser.
    pub new: bool,
}

/// The new parser on `snippet`, through the entry point the kind names.
pub fn new_parse(kind: Kind, snippet: &str, options: &ParseOptions) -> Parse {
    match kind {
        Kind::Statements => parse_statements(snippet, options),
        Kind::Expression => parse_expression(snippet, options),
        Kind::Declarations | Kind::File => parse_source_file(snippet, options),
    }
}

pub fn new_accepts(kind: Kind, snippet: &str, options: &ParseOptions) -> bool {
    new_parse(kind, snippet, options).is_ok()
}

pub fn verdict(kind: Kind, snippet: &str, preset: &Preset) -> Verdict {
    let new = new_accepts(kind, snippet, &preset.new);
    if matches!(kind, Kind::Declarations | Kind::File) {
        let legacy = legacy::accepts_file(snippet, &preset.legacy);
        return Verdict {
            legacy,
            program: legacy,
            new,
        };
    }
    let body = match kind {
        Kind::Expression => format!("x := {snippet};"),
        _ => snippet.to_string(),
    };
    Verdict {
        legacy: legacy::accepts(&body, &preset.legacy),
        program: legacy::accepts_in_program(&body, &preset.legacy),
        new,
    }
}

/// Which legacy entry is the reference for a table.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Oracle {
    /// The legacy fragment entry, `parse_st_statements`.
    Fragment,
    /// The legacy parser on the snippet inside a `PROGRAM`.
    Program,
}

impl Verdict {
    /// The legacy verdict of the chosen oracle.
    pub fn legacy_for(&self, oracle: Oracle) -> bool {
        match oracle {
            Oracle::Fragment => self.legacy,
            Oracle::Program => self.program,
        }
    }
}

/// What kind of reason a listed difference has. Every difference from the
/// legacy parser is one of these three, so a difference that is none of them
/// is a defect of the new parser and is fixed at its cause, not listed.
#[derive(Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Debug)]
pub enum Class {
    /// The legacy parser is wrong and the new one does not port the defect.
    LegacyDefect,
    /// A behaviour change the owner decided, by name or by the rule that
    /// CODESYS is the reference.
    OwnerDecided,
    /// The new parser takes what the legacy one rejects (or lowers it to
    /// another shape), on purpose, and the reason says why.
    AcceptedOnPurpose,
}

impl Class {
    pub const ALL: [Class; 3] = [
        Class::LegacyDefect,
        Class::OwnerDecided,
        Class::AcceptedOnPurpose,
    ];

    pub fn name(self) -> &'static str {
        match self {
            Class::LegacyDefect => "legacy defect not ported",
            Class::OwnerDecided => "owner-decided change",
            Class::AcceptedOnPurpose => "accepted on purpose",
        }
    }
}

/// A reason for a difference, with its class. A reason is defined once and
/// named by every entry it explains, so one reason has one class.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub struct Reason {
    pub class: Class,
    pub text: &'static str,
}

impl Reason {
    pub const fn new(class: Class, text: &'static str) -> Self {
        Reason { class, text }
    }
}

impl std::fmt::Display for Reason {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "[{}] {}", self.class.name(), self.text)
    }
}

/// Why a difference exists.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Basis {
    /// The legacy fragment entry is wrong for the snippet: the legacy
    /// parser inside a `PROGRAM` agrees with the new parser.
    FragmentEntry,
    /// The new parser deliberately differs from the legacy one.
    Deliberate,
}

/// A documented difference: for `snippet`, under `preset` (or every preset
/// when `None`), the legacy fragment parser says `legacy` and the new one
/// says the opposite, for `reason`.
pub struct Exception {
    pub kind: Kind,
    pub snippet: &'static str,
    pub preset: Option<&'static str>,
    pub legacy: bool,
    pub basis: Basis,
    pub reason: Reason,
}

impl Exception {
    pub fn covers(
        &self,
        kind: Kind,
        snippet: &str,
        preset: &str,
        oracle: Oracle,
        verdict: &Verdict,
    ) -> bool {
        self.kind == kind
            && self.snippet == snippet
            && self.preset.is_none_or(|name| name == preset)
            && self.legacy == verdict.legacy_for(oracle)
            && self.legacy != verdict.new
    }

    /// True when the exception's claim about its basis holds for `verdict`.
    pub fn basis_holds(&self, verdict: &Verdict) -> bool {
        match self.basis {
            Basis::FragmentEntry => verdict.program == verdict.new,
            Basis::Deliberate => true,
        }
    }
}

/// The files compared, each with its CRLF and tab-indented spellings
/// (`name (CRLF)`, `name (tabs)`), read in place from the corpus the
/// repository shares (`ironplc_test::corpus`).
pub fn file_variants() -> Vec<(String, String)> {
    ironplc_test::corpus::variants()
}
