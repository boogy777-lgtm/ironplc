//! The recurrence guard of the renderer: every source of the repository's
//! shared corpus (`ironplc_test::corpus`) that parses must render to text
//! that parses to the same AST.
//!
//! `corpus.rs` pins the layout of a few sources against golden files; this
//! checks all of them, under every dialect that accepts them, without a
//! golden file, so a renderer that drops or rewrites a node fails here for the
//! first corpus source that contains it -- no test of its own needed. A source
//! that does not round trip must be listed in `KNOWN`, with the reason; an
//! unlisted one fails, and so does a listed one that has started to round trip.

use super::common::*;
use ironplc_test::corpus::{CRLF, TABS};
use std::collections::BTreeMap;

/// What became of one source under one dialect.
#[derive(Debug, Clone, Copy, PartialEq)]
enum Outcome {
    /// The rendering parses to the same AST.
    Same,
    /// The rendering parses to another AST, and rendering that gives the same
    /// text again: a normalization.
    Normalized,
    /// The rendering parses to another AST that renders differently.
    Changed,
    /// The rendering does not parse under the dialect of its source.
    DoesNotParse,
}

enum Known {
    /// The AST changes only by a normalization the renderer chooses.
    Normalizes,
    /// A renderer defect, kept here until it is fixed.
    Defect,
}

/// The sources that do not round trip: name, kind and reason.
const KNOWN: &[(&str, Known, &str)] = &[
    (
        "tests/fixtures/codesys/types_and_variables.st",
        Known::Normalizes,
        "a VAR_GLOBAL block of several variables renders as one block per variable",
    ),
    (
        "tests/fixtures/codesys/configuration.st",
        Known::Defect,
        "the PROGRAM parameter list of a configuration (task of a function block, \
         inputs and outputs) is not rendered",
    ),
    (
        "../resources/test/time_literal_extensions.st",
        Known::Defect,
        "a long date renders as `LDATE#`, which the dialects without the LDATE \
         keyword do not parse (the source spelled it `LD#`)",
    ),
];

fn round_trip(text: &str, options: &CompilerOptions) -> Option<Outcome> {
    let original = parse_program(text, &FileId::default(), options).ok()?;
    let rendered = write_to_string(&original).expect("a parsed library renders");
    let Ok(reparsed) = parse_program(&rendered, &FileId::default(), options) else {
        return Some(Outcome::DoesNotParse);
    };
    if reparsed == original {
        return Some(Outcome::Same);
    }
    let again = write_to_string(&reparsed).expect("a parsed library renders");
    Some(if again == rendered {
        Outcome::Normalized
    } else {
        Outcome::Changed
    })
}

#[test]
fn write_to_string_when_corpus_source_parses_then_rendering_parses_to_the_same_ast() {
    // The worst outcome of each source over the dialects that parse it.
    let mut worst: BTreeMap<String, (Outcome, Dialect)> = BTreeMap::new();
    let mut parsed = 0;
    for (name, text) in ironplc_test::corpus::variants() {
        // The other spellings only differ in white space and cannot change what
        // the renderer sees.
        if name.ends_with(CRLF) || name.ends_with(TABS) {
            continue;
        }
        let mut parses = false;
        for dialect in Dialect::ALL {
            let options = CompilerOptions::from_dialect(*dialect);
            let Some(outcome) = round_trip(&text, &options) else {
                continue;
            };
            parses = true;
            let rank = |o: Outcome| match o {
                Outcome::Same => 0,
                Outcome::Normalized => 1,
                Outcome::Changed => 2,
                Outcome::DoesNotParse => 3,
            };
            let entry = worst.entry(name.clone()).or_insert((outcome, *dialect));
            if rank(outcome) > rank(entry.0) {
                *entry = (outcome, *dialect);
            }
        }
        parsed += usize::from(parses);
    }
    assert!(parsed > 100, "only {parsed} corpus sources parse");

    let mut problems = Vec::new();
    for (name, (outcome, dialect)) in &worst {
        let known = KNOWN.iter().find(|(known, _, _)| name.ends_with(known));
        match (outcome, known) {
            (Outcome::Same, None) => {}
            (Outcome::Same, Some((_, _, reason))) => problems.push(format!(
                "{name}: listed in KNOWN but now round trips ({reason})"
            )),
            (_, None) => problems.push(format!("{name}: {outcome:?} under {dialect:?}")),
            (Outcome::Normalized, Some((_, Known::Normalizes, _))) => {}
            (_, Some((_, Known::Defect, _))) => {}
            (_, Some((_, Known::Normalizes, _))) => problems.push(format!(
                "{name}: listed as a normalization but {outcome:?} under {dialect:?}"
            )),
        }
    }
    for (known, _, _) in KNOWN {
        if !worst.keys().any(|name| name.ends_with(known)) {
            problems.push(format!("{known}: listed in KNOWN but not in the corpus"));
        }
    }
    assert!(
        problems.is_empty(),
        "the renderer changes the AST of:\n{}",
        problems.join("\n")
    );
}
