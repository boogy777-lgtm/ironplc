//! Lowering against the legacy parser, test-only: literals, and whole
//! libraries as far as the lowering reaches.
//!
//! Two comparisons, both strict (`ast.rs`):
//!
//! - **Literals.** Every literal node of every input the new parser accepts is
//!   lowered and compared to what the legacy `constant` rule builds from the
//!   legacy tokens of the same bytes. The legacy rule is applied to the
//!   tokens, not looked up in the legacy tree, because the legacy tree holds
//!   some literal positions as bare integers (a subrange bound, a string
//!   length) that are not constants.
//! - **Whole inputs.** An input whose tree contains no node kind without a
//!   lowering rule is lowered as a library and compared to the legacy
//!   `parse_program` result. An input that does contain one is skipped and
//!   counted: the skipped count falls as rules are written, and the floor on
//!   the compared count rises with it.
//!
//! The inputs are the statement, expression and declaration tables, the
//! declarations and bodies lifted from the legacy tests, and the file corpus
//! with its CRLF and tab spellings, each under every dialect preset.
//!
//! A difference must be listed in `DIFFERENCES` with its reason. An
//! unlisted difference fails; so does a listed one whose count is not the
//! number observed, which is how a stale entry (fewer) and a growing one
//! (more) are both caught.

use super::ast::{compare, explain};
use super::declaration_table::DECLARATIONS;
use super::legacy::{presets, Preset};
use super::tables::{EXPRESSIONS, STATEMENTS};
use super::{extract, file_variants, new_parse, Kind};
use crate::legacy::{parse_program, tokenize_program};
use crate::parser::parse_constant;
use crate::token::{Token, TokenType};
use ironplc_dsl::core::FileId;
use ironplc_syntax::lower::{
    contains_pending, disposition, literals::lower_constant, lower_library, Area, Disposition,
    LowerCx,
};
use ironplc_syntax::{Parse, SyntaxKind, SyntaxNode};

/// The file every comparison is made in, so that the file of every span is
/// compared and not only its offsets.
fn file() -> FileId {
    FileId::from_string("parity.st")
}

/// Where a difference is found.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Scope {
    /// Every literal of the named input (a file, with or without its CRLF and
    /// tab spellings, or a snippet).
    Input(&'static str),
    /// Every literal node of kind `node` directly under a node of kind
    /// `parent`.
    Site {
        parent: SyntaxKind,
        node: SyntaxKind,
    },
}

/// A documented difference: where it is, why, and how often the corpus shows
/// it.
pub struct Difference {
    pub scope: Scope,
    pub reason: &'static str,
    pub expected: usize,
}

const CASE_SELECTOR: &str = "the legacy grammar reads a case selector with its own rule (`case_bit_string_literal`), which builds an untyped bit string; the `constant` rule the comparison applies reads the same tokens as an integer. The selector itself is compared where case labels are lowered";
const MARKER_IN_STRING: &str = "the legacy pre-pass takes marker text inside a string literal as a ranged-comment marker and blanks the statement between, so the legacy tokens are not those of the text; the new parser lexes the strings first and keeps the code (a deliberate difference, pinned by its own test)";
const SEVERAL_PAIRS: &str = "the legacy pre-pass blanks the first ranged-comment pair only, so the legacy lexer rejects the file at the second pair's body and produces no tokens after it; the new parser makes every pair a region (a deliberate difference, listed for the file)";

pub const DIFFERENCES: &[Difference] = &[
    Difference {
        scope: Scope::Site {
            parent: SyntaxKind::CaseLabel,
            node: SyntaxKind::BitStringLiteral,
        },
        reason: CASE_SELECTOR,
        expected: 54,
    },
    Difference {
        scope: Scope::Input("tests/fixtures/lexical/oscat_marker_in_string.st"),
        reason: MARKER_IN_STRING,
        expected: 54,
    },
    Difference {
        scope: Scope::Input("tests/fixtures/lexical/oscat_several_pairs.st"),
        reason: SEVERAL_PAIRS,
        expected: 18,
    },
];

impl Scope {
    fn covers(&self, key: &str, parent: SyntaxKind, node: SyntaxKind) -> bool {
        match *self {
            Scope::Input(name) => {
                key == name
                    || key
                        .strip_prefix(name)
                        .is_some_and(|rest| rest.starts_with(" ("))
            }
            Scope::Site {
                parent: wanted_parent,
                node: wanted_node,
            } => wanted_parent == parent && wanted_node == node,
        }
    }
}

/// The result of running the comparisons over a corpus.
#[derive(Default)]
pub struct Tally {
    /// Inputs under presets, whether or not the new parser accepted them.
    pub inputs: usize,
    /// Those the new parser rejected, which are not lowered.
    pub rejected: usize,
    /// Literal nodes lowered.
    pub literals: usize,
    /// Literals that are equal to the legacy ones, in every part.
    pub equal: usize,
    /// Of those, the ones in an input that holds nothing but literals.
    pub literal_only_equal: usize,
    /// Literals both sides reject.
    pub both_reject: usize,
    /// Whole inputs lowered and compared, and those equal.
    pub whole_compared: usize,
    pub whole_equal: usize,
    /// Whole inputs skipped for a node kind without a rule.
    pub whole_skipped: usize,
    /// How many differences each entry of `DIFFERENCES` explained.
    pub excepted: Vec<usize>,
    /// Differences no entry covers.
    pub unexplained: Vec<String>,
}

impl Tally {
    fn new() -> Self {
        Tally {
            excepted: vec![0; DIFFERENCES.len()],
            ..Tally::default()
        }
    }

    /// Records a difference at `key`, under a node of kind `node` whose parent
    /// has kind `parent`, against the entries of `DIFFERENCES`.
    fn difference(&mut self, key: &str, parent: SyntaxKind, node: SyntaxKind, what: String) {
        let covering = DIFFERENCES
            .iter()
            .position(|entry| entry.scope.covers(key, parent, node));
        match covering {
            Some(index) => self.excepted[index] += 1,
            None => self
                .unexplained
                .push(format!("{key:?} {parent:?}/{node:?}: {what}")),
        }
    }

    pub fn excepted_total(&self) -> usize {
        self.excepted.iter().sum()
    }
}

/// The legacy tokens that make up the literal at `range`: the slice from the
/// first to the last token inside it. Whitespace and the zero-width
/// terminators the legacy pipeline inserts are not part of a literal.
fn literal_tokens(tokens: &[Token], range: (usize, usize)) -> &[Token] {
    let counts = |token: &Token| {
        token.span.start < token.span.end
            && !matches!(token.token_type, TokenType::Whitespace | TokenType::Newline)
    };
    let inside = |token: &Token| token.span.start >= range.0 && token.span.end <= range.1;
    let first = tokens
        .iter()
        .position(|token| counts(token) && inside(token));
    let last = tokens
        .iter()
        .rposition(|token| counts(token) && inside(token));
    match (first, last) {
        (Some(first), Some(last)) => &tokens[first..=last],
        _ => &[],
    }
}

fn range_of(node: &SyntaxNode) -> (usize, usize) {
    (
        usize::from(node.text_range().start()),
        usize::from(node.text_range().end()),
    )
}

/// Lowers every literal of an accepted parse and compares it to the legacy
/// constant rule.
fn compare_literals(tally: &mut Tally, key: &str, text: &str, parse: &Parse, preset: &Preset) {
    let cx = LowerCx::new(file());
    let tokens = tokenize_program(text, &file(), &preset.legacy, 0, 0).0;
    let only_literals = parse
        .root
        .descendants()
        .skip(1)
        .all(|node| disposition(node.kind()) == Disposition::Lowered(Area::Literal));
    for node in parse.root.descendants() {
        if disposition(node.kind()) != Disposition::Lowered(Area::Literal) {
            continue;
        }
        tally.literals += 1;
        let parent = node
            .parent()
            .map_or(SyntaxKind::Unknown, |parent| parent.kind());
        let range = range_of(&node);
        let written = &text[range.0..range.1];
        match (
            parse_constant(literal_tokens(&tokens, range)),
            lower_constant(&cx, &node),
        ) {
            (Ok(legacy), Ok(lowered)) => {
                if compare(&legacy, &lowered).is_empty() {
                    tally.equal += 1;
                    tally.literal_only_equal += usize::from(only_literals);
                } else {
                    let what = format!("{written} {}", explain(&legacy, &lowered));
                    tally.difference(key, parent, node.kind(), what);
                }
            }
            (Err(legacy), Err(error)) if legacy.code == error.code => tally.both_reject += 1,
            (Err(legacy), Err(error)) => {
                let what = format!(
                    "{written} legacy rejects {} where lowering rejects {}",
                    legacy.code, error.code
                );
                tally.difference(key, parent, node.kind(), what);
            }
            (Ok(legacy), Err(error)) => {
                let what = format!(
                    "{written} lowering rejects {} where legacy builds {legacy:?}",
                    error.code
                );
                tally.difference(key, parent, node.kind(), what);
            }
            (Err(error), Ok(_)) => {
                let what = format!(
                    "{written} legacy rejects {}: {}",
                    error.code, error.primary.message
                );
                tally.difference(key, parent, node.kind(), what);
            }
        }
    }
}

/// Lowers a whole accepted input as a library and compares it to the legacy
/// parse, when every node kind of its tree has a lowering rule.
fn compare_library(tally: &mut Tally, key: &str, text: &str, parse: &Parse, preset: &Preset) {
    if contains_pending(&parse.root) {
        tally.whole_skipped += 1;
        return;
    }
    tally.whole_compared += 1;
    let legacy = parse_program(text, &file(), &preset.legacy);
    let lowered = lower_library(parse, &file());
    match (legacy, lowered) {
        (Ok(legacy), Ok(lowered)) => {
            if compare(&legacy, &lowered).is_empty() {
                tally.whole_equal += 1;
            } else {
                let what = explain(&legacy, &lowered);
                tally.difference(key, SyntaxKind::SourceFile, SyntaxKind::SourceFile, what);
            }
        }
        (legacy, lowered) => tally.difference(
            key,
            SyntaxKind::SourceFile,
            SyntaxKind::SourceFile,
            format!(
                "legacy {:?} lowered {:?}",
                legacy.map(|_| ()).map_err(|e| e.code),
                lowered.map(|_| ()).map_err(|e| e.code)
            ),
        ),
    }
}

/// One input: its key, its text and the entry point that parses it.
pub struct Case {
    pub kind: Kind,
    pub key: String,
    pub text: String,
}

/// Every input the comparisons run over.
pub fn corpus() -> Vec<Case> {
    let case = |kind: Kind, text: &str| Case {
        kind,
        key: text.to_string(),
        text: text.to_string(),
    };
    let mut cases: Vec<Case> = Vec::new();
    cases.extend(STATEMENTS.iter().map(|text| case(Kind::Statements, text)));
    cases.extend(EXPRESSIONS.iter().map(|text| case(Kind::Expression, text)));
    cases.extend(
        extract::legacy_test_bodies()
            .iter()
            .map(|text| case(Kind::Statements, text)),
    );
    cases.extend(
        DECLARATIONS
            .iter()
            .map(|text| case(Kind::Declarations, text)),
    );
    cases.extend(
        extract::legacy_declaration_snippets()
            .iter()
            .map(|text| case(Kind::Declarations, text)),
    );
    cases.extend(file_variants().into_iter().map(|(key, text)| Case {
        kind: Kind::File,
        key,
        text,
    }));
    cases
}

/// Runs both comparisons over `cases` under every preset.
pub fn run(cases: &[Case], presets: &[Preset]) -> Tally {
    let mut tally = Tally::new();
    for case in cases {
        for preset in presets {
            tally.inputs += 1;
            let parse = new_parse(case.kind, &case.text, &preset.new);
            if !parse.is_ok() {
                tally.rejected += 1;
                continue;
            }
            compare_literals(&mut tally, &case.key, &case.text, &parse, preset);
            if matches!(case.kind, Kind::Declarations | Kind::File) {
                compare_library(&mut tally, &case.key, &case.text, &parse, preset);
            }
        }
    }
    tally
}

/// What the run must reach at least: the floors rise as lowering rules are
/// written and never fall. Literals equal to the legacy ones, those among them
/// in inputs that are nothing but a literal, and whole inputs lowered as
/// libraries.
const MIN_LITERALS_EQUAL: usize = 18_000;
const MIN_LITERAL_ONLY_EQUAL: usize = 400;
const MIN_WHOLE_COMPARED: usize = 40;

/// Checks a tally against the table: no unexplained difference, every
/// entry's count is the one recorded, and the floors are met.
pub fn check(tally: &Tally) -> Vec<String> {
    let mut problems: Vec<String> = tally.unexplained.clone();
    for (entry, seen) in DIFFERENCES.iter().zip(&tally.excepted) {
        if *seen != entry.expected {
            let how = if *seen == 0 {
                "no longer differs (stale)"
            } else if *seen > entry.expected {
                "differs more often than recorded"
            } else {
                "differs less often than recorded"
            };
            problems.push(format!(
                "{:?} {how}: recorded {}, observed {seen} ({})",
                entry.scope, entry.expected, entry.reason
            ));
        }
    }
    for (name, seen, floor) in [
        ("equal literals", tally.equal, MIN_LITERALS_EQUAL),
        (
            "equal literals in literal-only inputs",
            tally.literal_only_equal,
            MIN_LITERAL_ONLY_EQUAL,
        ),
        (
            "whole inputs compared",
            tally.whole_compared,
            MIN_WHOLE_COMPARED,
        ),
    ] {
        if seen < floor {
            problems.push(format!("{name}: {seen}, floor {floor}"));
        }
    }
    problems
}

/// Prints the numbers of a run.
pub fn summarize(tally: &Tally) {
    println!(
        "lowering: {} inputs ({} rejected by the new parser, not lowered); \
         literals: {} compared, {} equal, {} excepted, {} rejected by both; \
         whole inputs: {} compared ({} equal), {} skipped for a kind without a rule",
        tally.inputs,
        tally.rejected,
        tally.literals,
        tally.equal,
        tally.excepted_total(),
        tally.both_reject,
        tally.whole_compared,
        tally.whole_equal,
        tally.whole_skipped
    );
    for (entry, seen) in DIFFERENCES.iter().zip(&tally.excepted) {
        println!("  [{seen}] {:?}: {}", entry.scope, entry.reason);
    }
}

#[test]
fn parity_when_literals_and_libraries_lowered_then_differences_are_exactly_the_exceptions() {
    let tally = run(&corpus(), &presets());
    summarize(&tally);
    let problems = check(&tally);
    assert!(
        problems.is_empty(),
        "{} problems:\n{}",
        problems.len(),
        problems.join("\n")
    );
}

#[test]
fn check_when_difference_is_unlisted_then_reported() {
    let mut tally = Tally::new();
    tally.difference(
        "x := 1;",
        SyntaxKind::AssignStmt,
        SyntaxKind::IntLiteral,
        "1 differs".to_string(),
    );
    assert_eq!(tally.unexplained.len(), 1);
    assert!(check(&tally)
        .iter()
        .any(|problem| problem.contains("1 differs")));
}

#[test]
fn check_when_entry_count_is_not_the_recorded_one_then_reported_both_ways() {
    let mut fewer = Tally::new();
    fewer.excepted = DIFFERENCES.iter().map(|entry| entry.expected).collect();
    fewer.excepted[0] -= 1;
    assert!(check(&fewer)
        .iter()
        .any(|problem| problem.contains("less often")));
    let mut more = Tally::new();
    more.excepted = DIFFERENCES.iter().map(|entry| entry.expected).collect();
    more.excepted[0] += 1;
    assert!(check(&more)
        .iter()
        .any(|problem| problem.contains("more often")));
    let stale = Tally::new();
    assert!(check(&stale)
        .iter()
        .any(|problem| problem.contains("stale")));
}

#[test]
fn difference_when_listed_scope_then_counted_against_its_entry_and_not_unexplained() {
    let mut tally = Tally::new();
    tally.difference(
        "tests/fixtures/lexical/oscat_marker_in_string.st (CRLF)",
        SyntaxKind::AssignStmt,
        SyntaxKind::StringLiteral,
        String::new(),
    );
    tally.difference(
        "x",
        SyntaxKind::CaseLabel,
        SyntaxKind::BitStringLiteral,
        String::new(),
    );
    assert!(tally.unexplained.is_empty());
    assert_eq!(tally.excepted, vec![1, 1, 0]);
}

#[test]
fn covers_when_input_scope_then_the_file_and_its_spellings_only() {
    let scope = Scope::Input("a/b.st");
    let any = SyntaxKind::Unknown;
    assert!(scope.covers("a/b.st", any, any));
    assert!(scope.covers("a/b.st (CRLF)", any, any));
    assert!(scope.covers("a/b.st (tabs)", any, any));
    assert!(!scope.covers("a/b.st.bak", any, any));
    assert!(!scope.covers("a/bb.st", any, any));
}

#[test]
fn literal_tokens_when_range_given_then_the_tokens_inside_without_whitespace_or_terminators() {
    let source = "x := T#5s ;";
    let tokens = tokenize_program(source, &file(), &presets()[0].legacy, 0, 0).0;
    let inside = literal_tokens(&tokens, (5, 9));
    let text: Vec<&str> = inside.iter().map(|token| token.text.as_str()).collect();
    assert_eq!(text, vec!["T", "#", "5", "s"]);
    assert!(literal_tokens(&tokens, (20, 25)).is_empty());
}

#[test]
fn compare_literals_when_literal_lowered_with_a_wrong_span_then_reported_as_unexplained() {
    // The strict comparison is what the run relies on: a literal that equals
    // the legacy one except in where it is must be reported.
    let preset = &presets()[0];
    let text = "x := 5;";
    let parse = new_parse(Kind::Statements, text, &preset.new);
    let tokens = tokenize_program(text, &file(), &preset.legacy, 0, 0).0;
    let legacy = parse_constant(literal_tokens(&tokens, (5, 6)));
    let moved = LowerCx::new(file());
    let node = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::IntLiteral);
    let lowered = node.map(|node| lower_constant(&moved, &node));
    assert_eq!(
        legacy
            .as_ref()
            .ok()
            .zip(lowered.as_ref().and_then(|l| l.as_ref().ok()))
            .map(|(a, b)| compare(a, b)),
        Some(vec![])
    );
    let shifted = legacy.ok().map(|constant| {
        constant.with_span(ironplc_dsl::core::SourceSpan::range(4, 5).with_file_id(&file()))
    });
    let both = shifted
        .as_ref()
        .zip(lowered.as_ref().and_then(|l| l.as_ref().ok()))
        .map(|(a, b)| compare(a, b).is_empty());
    assert_eq!(both, Some(false));
}

/// Durations at and past the longest one a duration holds, with the problem
/// both parsers must report for each (`None` when both accept it). The legacy
/// parser and the lowering build them through the same checked builder, so
/// each must give the same duration, or the same problem over the same bytes.
const DURATION_RANGE_EDGES: &[(&str, Option<&str>)] = &[
    ("T#106751991167300d", None),
    ("T#106751991167301d", Some("P2039")),
    ("T#2562047788015215h", None),
    ("T#2562047788015216h", Some("P2039")),
    ("T#153722867280912930m", None),
    ("T#153722867280912931m", Some("P2039")),
    ("T#9223372036854775807s", None),
    ("T#9223372036854775807.999999999s", None),
    ("T#9223372036854775807s999999999ns", None),
    ("T#9223372036854775807s1000000000ns", Some("P2039")),
    ("T#9223372036854775808s", Some("P2039")),
    ("T#18446744073709551615s", Some("P2039")),
    ("T#18446744073709551615ms", None),
    ("T#18446744073709551615us", None),
    ("T#18446744073709551615ns", None),
    ("T#9223372036854775807d", Some("P2039")),
    ("T#106751991167300d23h", Some("P2039")),
    ("T#-9223372036854775807s", None),
    ("T#-9223372036854775807d", Some("P2039")),
    ("LTIME#9223372036854775807d", Some("P2039")),
    ("LT#106751991167300d", None),
    // A whole part beyond `u64` is not a number the structure holds, whether
    // or not it has a decimal point; it was read as `0` without one.
    ("T#18446744073709551616s", Some("P0002")),
    ("T#18446744073709551617ms", Some("P0002")),
    ("T#99999999999999999999.5s", Some("P0002")),
];

#[test]
fn parity_when_duration_at_range_edge_then_same_duration_or_same_problem_and_range() {
    for preset in presets() {
        for (snippet, expected) in DURATION_RANGE_EDGES {
            let text = format!("x := {snippet};");
            let parse = new_parse(Kind::Statements, &text, &preset.new);
            if !parse.is_ok() {
                // `LTIME` and `LT` are keywords of the editions that have them.
                assert!(snippet.starts_with("LT"), "{snippet} under {}", preset.name);
                continue;
            }
            let node = parse
                .root
                .descendants()
                .find(|node| disposition(node.kind()) == Disposition::Lowered(Area::Literal))
                .expect("a literal");
            let range = range_of(&node);
            let tokens = tokenize_program(&text, &file(), &preset.legacy, 0, 0).0;
            let legacy = parse_constant(literal_tokens(&tokens, range));
            let lowered = lower_constant(&LowerCx::new(file()), &node);
            let code = |result: &Result<_, ironplc_dsl::diagnostic::Diagnostic>| {
                result.as_ref().err().map(|d| d.code.clone())
            };
            let expected_code = expected.map(str::to_string);
            assert_eq!(
                code(&legacy),
                expected_code,
                "{snippet} under {}: legacy",
                preset.name
            );
            assert_eq!(
                code(&lowered),
                expected_code,
                "{snippet} under {}: lowering",
                preset.name
            );
            match (&legacy, &lowered) {
                (Ok(legacy), Ok(lowered)) => {
                    assert_eq!(compare(legacy, lowered), vec![], "{snippet}");
                }
                // The wording of a syntax error differs by parser; the range
                // and the wording of a range problem do not.
                (Err(legacy), Err(lowered)) if legacy.code == "P2039" => {
                    assert_eq!(legacy.primary.message, lowered.primary.message);
                    assert_eq!(
                        (legacy.primary.location.start, legacy.primary.location.end),
                        (lowered.primary.location.start, lowered.primary.location.end),
                        "{snippet}"
                    );
                }
                _ => {}
            }
        }
    }
}
