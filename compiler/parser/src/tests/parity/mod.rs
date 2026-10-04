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
mod combinations;
mod declarations;
mod diagnostics_codes;
mod differences;
mod files;
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
use std::path::Path;

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
    pub reason: &'static str,
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
/// (`name (CRLF)`, `name (tabs)`), read in place: the shared test resources
/// (`../resources/test/oop.st`), the syntax crate's fixtures
/// (`tests/fixtures/codesys/x.st`), and the other `.st` sources of the
/// repository (the plc2plc round-trip files, the bundled libraries and the
/// source-discovery fixtures, the CLI resources, the end-to-end library files
/// and the examples). A file that is not valid UTF-8 reads as empty.
pub fn file_variants() -> Vec<(String, String)> {
    let manifest = Path::new(env!("CARGO_MANIFEST_DIR"));
    let roots = [
        (manifest.join("../resources/test"), "../resources/test"),
        (manifest.join("../syntax/tests/fixtures"), "tests/fixtures"),
        (
            manifest.join("../plc2plc/resources/test"),
            "../plc2plc/resources/test",
        ),
        (
            manifest.join("../sources/resources/libs"),
            "../sources/resources/libs",
        ),
        (
            manifest.join("../sources/resources/test"),
            "../sources/resources/test",
        ),
        (
            manifest.join("../ironplc-cli/resources/test"),
            "../ironplc-cli/resources/test",
        ),
        (
            manifest.join("../../tests/e2e/library"),
            "../../tests/e2e/library",
        ),
        (manifest.join("../../examples"), "../../examples"),
    ];
    let mut files = Vec::new();
    for (root, label) in roots {
        for path in ironplc_test::st_files(&root) {
            let text = std::fs::read_to_string(&path).unwrap_or_default();
            let relative = path
                .strip_prefix(&root)
                .unwrap_or(&path)
                .to_string_lossy()
                .replace('\\', "/");
            let name = format!("{label}/{relative}");
            files.push((format!("{name} (CRLF)"), text.replace('\n', "\r\n")));
            files.push((format!("{name} (tabs)"), text.replace("    ", "\t")));
            files.push((name, text));
        }
    }
    files
}
