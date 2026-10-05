//! Parity of the token view: the tokens and the tokenizer diagnostics of the
//! lossless tree, as `tokenize_program` of the cst front end gives them,
//! against those of the legacy pipeline.
//!
//! Both sides are read as the sequence of `(type, start, end, line, col, text)`
//! of their tokens and the sequence of `(code, start, end)` of their
//! diagnostics. The token sequences are aligned by byte offset into the
//! stretches where they agree and the *hunks* where they do not; the
//! diagnostic sequences into the *items* only one side reports, the ones both
//! report over different ranges, and their order. Every hunk and every item is
//! covered by exactly one row of `TOKEN_EXCEPTIONS` or `DIAGNOSTIC_EXCEPTIONS`
//! with its reason and its count; one no row covers fails, and so does a row
//! whose count is not the number observed.
//!
//! The inputs are the whole inputs of the corpus (`diagnostics::whole_inputs`:
//! every file in LF, CRLF and tab spelling, and every snippet in a `PROGRAM`)
//! under every preset.

use super::diagnostics::whole_inputs;
use super::legacy::{presets, Preset};
use super::token_rows::{
    Hunk, Input, Item, Reported, Tuple, DIAGNOSTIC_EXCEPTIONS, TOKEN_EXCEPTIONS,
};
use crate::legacy;
use crate::options::CompilerOptions;
use crate::token::Token;
use crate::tokens::type_of;
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_syntax::{tokenize, SyntaxKind};
use spec_test_macro::spec_test;
use std::collections::BTreeMap;

/// The token view and the diagnostics of one front end for `text`.
pub struct View {
    pub tokens: Vec<Tuple>,
    pub diagnostics: Vec<Reported>,
}

/// The two tokenizers that are compared: the legacy pipeline, the oracle, and
/// the public function of the crate, which is the token view of the tree.
type Tokenize = fn(&str, &FileId, &CompilerOptions, usize, usize) -> (Vec<Token>, Vec<Diagnostic>);

const LEGACY: Tokenize = legacy::tokenize_program;
const CST: Tokenize = crate::tokenize_program;

fn view(tokenize: Tokenize, text: &str, preset: &Preset, (line, col): (usize, usize)) -> View {
    let (tokens, diagnostics) = tokenize(text, &FileId::default(), &preset.legacy, line, col);
    View {
        tokens: tokens
            .into_iter()
            .map(|token| Tuple {
                token_type: token.token_type,
                start: token.span.start,
                end: token.span.end,
                line: token.line,
                col: token.col,
                text: token.text,
                region: false,
            })
            .collect(),
        diagnostics: diagnostics
            .into_iter()
            .map(|d| (d.code, d.primary.location.start, d.primary.location.end))
            .collect(),
    }
}

/// What only the tree knows about its tokens: for each token of the view, the
/// token is a region, and the ranges of the pragmas.
fn tree_facts(text: &str, preset: &Preset) -> (Vec<bool>, Vec<(usize, usize)>) {
    let (tokens, _) = tokenize(text, &preset.new);
    let regions = (0..tokens.len())
        .filter(|index| type_of(&tokens, *index, &preset.new).is_some())
        .map(|index| {
            matches!(
                tokens[index].kind,
                SyntaxKind::RangedComment | SyntaxKind::InactiveRegion
            )
        })
        .collect();
    let pragmas = tokens
        .iter()
        .filter(|token| token.kind == SyntaxKind::Pragma)
        .map(|token| {
            (
                usize::from(token.range.start()),
                usize::from(token.range.end()),
            )
        })
        .collect();
    (regions, pragmas)
}

/// Splits two token sequences into the number of tokens that agree and the
/// hunks where they do not.
pub fn hunks(legacy: &[Tuple], cst: &[Tuple]) -> (usize, Vec<Hunk>) {
    let (mut i, mut j) = (0, 0);
    let mut equal = 0;
    let mut found = Vec::new();
    while i < legacy.len() || j < cst.len() {
        if let (Some(l), Some(c)) = (legacy.get(i), cst.get(j)) {
            if l == c {
                equal += 1;
                i += 1;
                j += 1;
                continue;
            }
        }
        let start = (i, j);
        loop {
            let grown = (i, j) != start;
            match (legacy.get(i), cst.get(j)) {
                (Some(l), Some(c)) if l == c => break,
                // A token with no width is a hunk of its own.
                (Some(l), _) if l.start == l.end => {
                    if !grown {
                        i += 1;
                    }
                    break;
                }
                (Some(l), Some(c)) => {
                    let legacy_first = l.start < c.start || (l.start == c.start && l.end < c.end);
                    if legacy_first {
                        i += 1;
                    } else if c.start < l.start || c.end < l.end {
                        j += 1;
                    } else {
                        i += 1;
                        j += 1;
                    }
                }
                (Some(_), None) => i += 1,
                (None, Some(_)) => j += 1,
                (None, None) => break,
            }
        }
        found.push(Hunk {
            legacy: legacy[start.0..i].to_vec(),
            cst: cst[start.1..j].to_vec(),
            before: cst[..start.1].iter().rev().take(4).cloned().collect(),
        });
    }
    (equal, found)
}

/// Splits two diagnostic sequences into what only one side reports, what both
/// report over different ranges, and whether the common ones come in another
/// order.
pub fn items(legacy: &[Reported], cst: &[Reported]) -> Vec<Item> {
    let mut matched = vec![false; cst.len()];
    let mut only_legacy = Vec::new();
    let mut common_legacy = Vec::new();
    for diagnostic in legacy {
        let found = cst
            .iter()
            .enumerate()
            .position(|(at, other)| !matched[at] && other == diagnostic);
        match found {
            Some(at) => {
                matched[at] = true;
                common_legacy.push(diagnostic.clone());
            }
            None => only_legacy.push(diagnostic.clone()),
        }
    }
    let common_cst: Vec<Reported> = cst
        .iter()
        .zip(&matched)
        .filter(|(_, matched)| **matched)
        .map(|(diagnostic, _)| diagnostic.clone())
        .collect();
    let mut only_cst: Vec<Reported> = cst
        .iter()
        .zip(&matched)
        .filter(|(_, matched)| !**matched)
        .map(|(diagnostic, _)| diagnostic.clone())
        .collect();

    let mut found = Vec::new();
    for diagnostic in only_legacy {
        let partner = only_cst
            .iter()
            .position(|other| other.0 == diagnostic.0 && other.1 == diagnostic.1);
        match partner {
            Some(at) => found.push(Item::Resized {
                legacy: diagnostic,
                cst: only_cst.remove(at),
            }),
            None => found.push(Item::LegacyOnly(diagnostic)),
        }
    }
    found.extend(only_cst.into_iter().map(Item::CstOnly));
    if common_legacy != common_cst {
        found.push(Item::Order);
    }
    found
}

/// What the comparison found.
#[derive(Default)]
pub struct Report {
    pub compared: usize,
    pub equal_inputs: usize,
    pub equal_tokens: usize,
    pub token_rows: Vec<usize>,
    pub diagnostic_rows: Vec<usize>,
    /// For each row of the two tables, the first input it explains.
    pub token_examples: Vec<String>,
    pub diagnostic_examples: Vec<String>,
    pub unexplained: BTreeMap<String, (usize, String)>,
}

fn note(report: &mut Report, class: String, example: String) {
    let entry = report.unexplained.entry(class).or_insert((0, example));
    entry.0 += 1;
}

fn signature(hunk: &Hunk) -> String {
    let types = |tokens: &[Tuple]| {
        tokens
            .iter()
            .take(8)
            .map(|t| format!("{:?}", t.token_type))
            .collect::<Vec<_>>()
            .join(" ")
    };
    format!(
        "legacy [{}] cst [{}]",
        types(&hunk.legacy),
        types(&hunk.cst)
    )
}

/// A few bytes of `text` around `at`, for an example.
fn around(text: &str, at: usize) -> String {
    let mut from = at.saturating_sub(40);
    while !text.is_char_boundary(from) {
        from -= 1;
    }
    let mut to = (at + 40).min(text.len());
    while !text.is_char_boundary(to) {
        to += 1;
    }
    format!("{:?}", &text[from..to])
}

/// Compares the two front ends over `inputs` under `presets`.
pub fn compare_views(
    inputs: &[(String, String)],
    presets: &[Preset],
    start: (usize, usize),
) -> Report {
    let mut report = Report {
        token_rows: vec![0; TOKEN_EXCEPTIONS.len()],
        diagnostic_rows: vec![0; DIAGNOSTIC_EXCEPTIONS.len()],
        token_examples: vec![String::new(); TOKEN_EXCEPTIONS.len()],
        diagnostic_examples: vec![String::new(); DIAGNOSTIC_EXCEPTIONS.len()],
        ..Report::default()
    };
    for (key, text) in inputs {
        for preset in presets {
            report.compared += 1;
            let legacy = view(LEGACY, text, preset, start);
            let mut cst = view(CST, text, preset, start);
            let (regions, pragmas) = tree_facts(text, preset);
            for (tuple, region) in cst.tokens.iter_mut().zip(regions) {
                tuple.region = region;
            }
            let ranges = |diagnostics: &[Reported]| -> Vec<(usize, usize)> {
                diagnostics.iter().map(|d| (d.1, d.2)).collect()
            };
            let (legacy_errors, errors) = (ranges(&legacy.diagnostics), ranges(&cst.diagnostics));
            let input = Input {
                key,
                text,
                legacy_errors: &legacy_errors,
                errors: &errors,
                pragmas: &pragmas,
            };

            let (equal, found) = hunks(&legacy.tokens, &cst.tokens);
            report.equal_tokens += equal;
            let mut clean = found.is_empty();
            for hunk in &found {
                let row = TOKEN_EXCEPTIONS
                    .iter()
                    .position(|row| (row.covers)(hunk, &input));
                match row {
                    Some(row) => {
                        report.token_rows[row] += 1;
                        if report.token_examples[row].is_empty() {
                            report.token_examples[row] = format!("{key:?} under {}", preset.name);
                        }
                    }
                    None => {
                        let at = hunk
                            .legacy
                            .first()
                            .or(hunk.cst.first())
                            .map_or(0, |t| t.start);
                        let example = format!(
                            "{key:?} under {} at {at}: {}\n    legacy {:?}\n    cst {:?}",
                            preset.name,
                            around(text, at),
                            hunk.legacy.iter().take(3).collect::<Vec<_>>(),
                            hunk.cst.iter().take(3).collect::<Vec<_>>()
                        );
                        note(&mut report, signature(hunk), example);
                    }
                }
            }

            let differing = items(&legacy.diagnostics, &cst.diagnostics);
            clean &= differing.is_empty();
            for item in &differing {
                let row = DIAGNOSTIC_EXCEPTIONS
                    .iter()
                    .position(|row| (row.covers)(item, &input));
                match row {
                    Some(row) => {
                        report.diagnostic_rows[row] += 1;
                        if report.diagnostic_examples[row].is_empty() {
                            report.diagnostic_examples[row] =
                                format!("{key:?} under {}", preset.name);
                        }
                    }
                    None => {
                        let example = format!(
                            "{key:?} under {}: {item:?}\n    legacy {:?}\n    cst {:?}",
                            preset.name, legacy.diagnostics, cst.diagnostics
                        );
                        let class = match item {
                            Item::LegacyOnly(d) => format!("diagnostic only legacy {}", d.0),
                            Item::CstOnly(d) => format!("diagnostic only cst {}", d.0),
                            Item::Resized { legacy, cst } => {
                                format!("diagnostic resized {} {}", legacy.0, cst.0)
                            }
                            Item::Order => "diagnostic order".to_string(),
                        };
                        note(&mut report, class, example);
                    }
                }
            }
            if clean {
                report.equal_inputs += 1;
            }
        }
    }
    report
}

/// Inputs that the corpus has too few of, written by hand: text that the legacy
/// lexer rejects, the line breaks other than `\n` and `\r\n`, and characters
/// outside ASCII, in the places where a column is counted.
const EXTRA_INPUTS: &[&str] = &[
    "PROGRAM p\n(* a\rb *)\nx := 1;\nEND_PROGRAM\n",
    "PROGRAM p\nx := 'a\rb';\nEND_PROGRAM\n",
    "PROGRAM p\nx := 1;\u{c}y := 2;\nEND_PROGRAM\n",
    "PROGRAM p\nx := 'é\u{1F600}' + é;\n(* \u{1F600} *) y := 2;\nEND_PROGRAM\n",
    "PROGRAM p\nx := 1 ? 2; y := 3;\nEND_PROGRAM\n",
    "PROGRAM p\nx := 1; \u{7} y := 3;\nEND_PROGRAM\n",
    "PROGRAM p (* a (* b *) c *) x := TIME(); y := __new(1) + __NEW_ITEM + a mod b; z := NOT d; END_PROGRAM",
    "FUNCTION TIME : TIME TIME := T#1s; END_FUNCTION FUNCTION g : INT x := time (* c *) (1); END_FUNCTION",
    "x := LT#1.5s; y := T#-1d2h3m4s5ms; z := TIME#1m30s;",
];

/// Every keyword spelling, in upper and in lower case, one after the other: the
/// dialect decides which of them are keywords, and the two cases differ for the
/// two operators the legacy lexer reads in upper case only.
fn every_keyword() -> String {
    SyntaxKind::KEYWORDS
        .iter()
        .flat_map(|(spelling, _)| [spelling.to_string(), spelling.to_lowercase()])
        .collect::<Vec<_>>()
        .join(" ")
}

/// The corpus and the hand-written inputs.
fn inputs() -> Vec<(String, String)> {
    let mut inputs = whole_inputs();
    inputs.push(("every keyword".to_string(), every_keyword()));
    inputs.extend(
        EXTRA_INPUTS
            .iter()
            .map(|text| (format!("extra {text:?}"), (*text).to_string())),
    );
    inputs
}

fn print(report: &Report, inputs: usize) {
    println!(
        "token view: {inputs} inputs, {} compared (input x preset), {} equal, {} tokens equal",
        report.compared, report.equal_inputs, report.equal_tokens
    );
    for (index, row) in TOKEN_EXCEPTIONS.iter().enumerate() {
        println!(
            "  token row {:?}: {} (e.g. {}) {}",
            row.name, report.token_rows[index], report.token_examples[index], row.reason
        );
    }
    for (index, row) in DIAGNOSTIC_EXCEPTIONS.iter().enumerate() {
        println!(
            "  diagnostic row {:?}: {} (e.g. {}) {}",
            row.name, report.diagnostic_rows[index], report.diagnostic_examples[index], row.reason
        );
    }
    println!("  unexplained: {}", report.unexplained.len());
    let mut unexplained: Vec<_> = report.unexplained.iter().collect();
    unexplained.sort_by_key(|(_, (count, _))| std::cmp::Reverse(*count));
    for (class, (count, example)) in &unexplained {
        println!("UNEXPLAINED x{count} {class}\n    e.g. {example}");
    }
}

/// The rows whose count is not the number recorded: a stale row (fewer) and a
/// growing one (more) are both caught.
fn miscounted(report: &Report) -> Vec<String> {
    TOKEN_EXCEPTIONS
        .iter()
        .zip(&report.token_rows)
        .map(|(row, seen)| (row.name, row.expected, *seen))
        .chain(
            DIAGNOSTIC_EXCEPTIONS
                .iter()
                .zip(&report.diagnostic_rows)
                .map(|(row, seen)| (row.name, row.expected, *seen)),
        )
        .filter(|(_, expected, seen)| expected != seen)
        .map(|(name, expected, seen)| format!("{name}: recorded {expected}, observed {seen}"))
        .collect()
}

/// REQ-PT-parser-012: the token view is the legacy token sequence except for the
/// differences that are rows of one table, each with its reason; a difference no
/// row names fails and so does a row that explains none.
#[spec_test(REQ_PT_parser_012)]
fn parity_when_token_view_then_differences_are_exactly_the_exceptions() {
    let inputs = inputs();
    let report = compare_views(&inputs, &presets(), (0, 0));
    print(&report, inputs.len());
    let miscounted = miscounted(&report);
    println!("  stale or miscounted: {}", miscounted.len());
    assert!(
        report.unexplained.is_empty(),
        "{} unexplained difference classes",
        report.unexplained.len()
    );
    // A row with no difference to explain is stale.
    for row in TOKEN_EXCEPTIONS {
        assert!(row.expected > 0, "{}", row.name);
    }
    for row in DIAGNOSTIC_EXCEPTIONS {
        assert!(row.expected > 0, "{}", row.name);
    }
    assert!(miscounted.is_empty(), "{miscounted:#?}");
}

#[test]
fn parity_when_text_starts_inside_a_document_then_the_same_rows_explain_the_same_differences() {
    // The offsets are where the text starts in its document (the body of a
    // PLCopen XML element): both front ends shift the first line's columns and
    // every line by the same amount, so the differences are the ones of a text
    // that starts at the beginning.
    let inputs = inputs();
    let at_start = compare_views(&inputs, &presets(), (0, 0));
    let embedded = compare_views(&inputs, &presets(), (7, 11));
    assert!(
        embedded.unexplained.is_empty(),
        "{:?}",
        embedded.unexplained
    );
    assert_eq!(embedded.token_rows, at_start.token_rows);
    assert_eq!(embedded.diagnostic_rows, at_start.diagnostic_rows);
    assert_eq!(embedded.equal_tokens, at_start.equal_tokens);
}

#[test]
fn tokenize_when_gated_form_then_the_diagnostic_carries_the_help_of_the_legacy_rule() {
    // The advice that goes with a problem is part of what a consumer shows, so
    // the tokenizer of the tree gives the same as the token rule it replaces.
    let sources = [
        "PROGRAM p\n// c\nEND_PROGRAM",
        "PROGRAM p\nx := 'a$Qb';\nEND_PROGRAM",
        "PROGRAM p\nx := \"a$Qb\";\nEND_PROGRAM",
    ];
    let preset = &presets()[0];
    for source in sources {
        let (_, legacy) = LEGACY(source, &FileId::default(), &preset.legacy, 0, 0);
        let (_, cst) = CST(source, &FileId::default(), &preset.legacy, 0, 0);
        assert_eq!(legacy.len(), 1, "{source:?}");
        assert_eq!(cst.len(), 1, "{source:?}");
        assert!(!legacy[0].help().is_empty(), "{source:?}");
        assert_eq!(cst[0].help(), legacy[0].help(), "{source:?}");
    }
}
