//! Lowering against the legacy parser, test-only: literals, expressions,
//! variables, statements, declarations and whole libraries.
//!
//! Two comparisons, both strict (`ast.rs`):
//!
//! - **Sites.** Every site of every input the new parser accepts (`sites.rs`:
//!   a literal, an operand, an argument, a condition, a place, a statement, a
//!   list of statements, a unit, a method, a property) is lowered and compared
//!   to what the legacy rule for it (`constant`, `expression`, `variable`,
//!   `statement_list`, `library`, `function_block_member`) builds from the
//!   legacy tokens of the same bytes, as the body of a program, where a word
//!   that closes the body follows. The legacy rule is applied to the tokens,
//!   not looked up in the legacy tree, because the legacy tree holds some
//!   literal positions as bare integers (a subrange bound, a string length)
//!   that are not constants.
//! - **Whole inputs.** Every input the new parser accepts is lowered as a
//!   library and compared to the legacy `parse_program` result.
//!
//! The inputs are the statement, expression and declaration tables, the
//! declarations and bodies lifted from the legacy tests, and the file corpus
//! with its CRLF and tab spellings, each under every dialect preset.
//!
//! A difference must be listed in `DIFFERENCES` with its reason. An
//! unlisted difference fails; so does a listed one whose count is not the
//! number observed, which is how a stale entry (fewer) and a growing one
//! (more) are both caught.
//!
//! A difference is counted where it begins. An expression that differs from the
//! legacy one only because an operand inside it differs is not a second
//! difference (it is tallied as inherited): the entry that explains the operand
//! explains it, and a difference of the enclosing expression itself, with no
//! operand differing, is still found.

use super::ast::{
    compare, explain, member_without_inner_parts, without_inner_chart_parts, without_inner_parts,
    Component, Subject,
};
use super::blocks::judge_block;
use super::declaration_table::DECLARATIONS;
use super::differences::DIFFERENCES;
use super::legacy::{presets, Preset};
use super::sites::{sites, Site, Unit};
use super::tables::{EXPRESSIONS, STATEMENTS};
use super::type_table::TYPES;
use super::{extract, file_variants, new_parse, Kind};
use crate::legacy::{parse_program, tokenize_program};
use crate::parser::{
    parse_chart, parse_constant, parse_declarations, parse_expression, parse_member,
    parse_statement_list, parse_type_declaration, parse_variable, parse_variable_initial,
};
use crate::token::{Token, TokenType};
use crate::xform_assign_file_id;
use ironplc_dsl::common::{Library, LibraryElementKind};
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::sfc::Network;
use ironplc_syntax::lower::{
    disposition,
    expressions::lower_expr,
    initializers::lower_initial_value,
    literals::lower_constant,
    lower_element, lower_library,
    oop::{lower_member, Member},
    sfc::lower_chart,
    statements::{lower_statement, lower_statement_list},
    types::lower_type_declaration,
    variables::lower_variable,
    Area, Disposition, LowerCx,
};
use ironplc_syntax::{Parse, SyntaxKind, SyntaxNode};

/// The problem codes of a failure inside the compiler: an internal error and a
/// capability that is not implemented. The lowering reports neither for text the
/// parser accepts.
const INTERNAL_CODES: [&str; 2] = ["P9998", "P9999"];

fn is_internal(diagnostic: &Diagnostic) -> bool {
    INTERNAL_CODES.contains(&diagnostic.code.as_str())
}

/// The file every comparison is made in, so that the file of every span is
/// compared and not only its offsets.
pub fn file() -> FileId {
    FileId::from_string("parity.st")
}

/// What was compared at the sites of one unit.
#[derive(Default, Clone, Copy)]
pub struct Counts {
    /// Sites lowered and compared.
    pub compared: usize,
    /// Those equal to the legacy object, in every part.
    pub equal: usize,
    /// Those both sides reject.
    pub both_reject: usize,
}

impl Counts {
    fn add(&mut self, other: Counts) {
        self.compared += other.compared;
        self.equal += other.equal;
        self.both_reject += other.both_reject;
    }

    fn record(&mut self, outcome: &Outcome) {
        self.compared += 1;
        match outcome {
            Outcome::Equal => self.equal += 1,
            Outcome::BothReject => self.both_reject += 1,
            Outcome::Internal(_) => {}
            Outcome::Differs { .. } => {}
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
    /// What was compared, by unit.
    pub units: [Counts; Unit::ALL.len()],
    /// Of the lists of statements, the bodies of declarations: the lists held by
    /// a declaration, not by a statement or by the file.
    pub bodies: Counts,
    /// Of the equal literals, the ones in an input that holds nothing but
    /// literals.
    pub literal_only_equal: usize,
    /// Differences that begin at a site inside the one that differs.
    pub inherited: usize,
    /// Whole inputs lowered and compared, and those equal.
    pub whole_compared: usize,
    pub whole_equal: usize,
    /// How many differences each entry of `DIFFERENCES` explained.
    pub excepted: Vec<usize>,
    /// Differences no entry covers.
    pub unexplained: Vec<(String, String)>,
    /// Inputs the new parser accepts whose lowering failed inside (an internal
    /// error or a capability that is not implemented), which is never a
    /// difference to list: lowering is total over what the parser accepts.
    pub internal: Vec<String>,
}

impl Tally {
    fn new() -> Self {
        Tally {
            excepted: vec![0; DIFFERENCES.len()],
            ..Tally::default()
        }
    }

    pub fn counts(&self, unit: Unit) -> Counts {
        self.units[unit as usize]
    }

    fn counts_mut(&mut self, unit: Unit) -> &mut Counts {
        &mut self.units[unit as usize]
    }

    /// Records a difference at `key`, under a node of kind `node` whose parent
    /// has kind `parent`, in `parts` of the comparison, against the entries of
    /// `DIFFERENCES`.
    fn difference(
        &mut self,
        key: &str,
        parent: SyntaxKind,
        node: SyntaxKind,
        parts: &[Component],
        what: String,
    ) {
        let covering = DIFFERENCES
            .iter()
            .position(|entry| entry.scope.covers(key, node, parts));
        match covering {
            Some(index) => self.excepted[index] += 1,
            None => self.unexplained.push((
                format!("{parent:?}/{node:?} {parts:?}"),
                format!("{key:?}: {what}"),
            )),
        }
    }

    /// Adds the tally of another part of the corpus to this one.
    fn add(&mut self, other: Tally) {
        self.inputs += other.inputs;
        self.rejected += other.rejected;
        for (unit, counts) in self.units.iter_mut().zip(other.units) {
            unit.add(counts);
        }
        self.bodies.add(other.bodies);
        self.literal_only_equal += other.literal_only_equal;
        self.inherited += other.inherited;
        self.whole_compared += other.whole_compared;
        self.whole_equal += other.whole_equal;
        for (seen, more) in self.excepted.iter_mut().zip(other.excepted) {
            *seen += more;
        }
        self.unexplained.extend(other.unexplained);
        self.internal.extend(other.internal);
    }

    pub fn excepted_total(&self) -> usize {
        self.excepted.iter().sum()
    }
}

/// The legacy tokens of `text` as the body of a `POU`: the word that closes the
/// body follows it, as it does in a program. The legacy pipeline inserts the `;`
/// that a block statement may omit before the next token, so a block statement
/// that ends the text has none to be inserted before; the fragment entry
/// rejects it where the program does not. The offsets of `text` are those of the
/// tokens.
pub fn body_tokens(text: &str, preset: &Preset) -> Vec<Token> {
    tokenize_program(
        &format!("{text} END_PROGRAM"),
        &file(),
        &preset.legacy,
        0,
        0,
    )
    .0
}

/// The legacy tokens that make up the construct at `range`: the slice from the
/// first to the last token inside it. Whitespace and the zero-width
/// terminators the legacy pipeline inserts are not part of a construct, except
/// that a construct that is read with its terminator (`terminated`: a
/// statement) takes the one inserted where it ends.
pub fn literal_tokens(tokens: &[Token], range: (usize, usize), terminated: bool) -> &[Token] {
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
    // The terminator is inserted before the first token that is not a space or
    // a comment after the keyword that ends the statement.
    let inserted_after = |last: usize| {
        let after = &tokens[last + 1..];
        let gap = after
            .iter()
            .take_while(|token| {
                matches!(
                    token.token_type,
                    TokenType::Whitespace | TokenType::Comment | TokenType::DocComment
                )
            })
            .count();
        match after.get(gap) {
            Some(token)
                if token.token_type == TokenType::Semicolon
                    && token.span.start == token.span.end =>
            {
                gap + 1
            }
            _ => 0,
        }
    };
    match (first, last) {
        (Some(first), Some(last)) => {
            let end = last + if terminated { inserted_after(last) } else { 0 };
            &tokens[first..=end]
        }
        _ => &[],
    }
}

pub fn range_of(node: &SyntaxNode) -> (usize, usize) {
    (
        usize::from(node.text_range().start()),
        usize::from(node.text_range().end()),
    )
}

/// What the two sides say about one site.
pub enum Outcome {
    Equal,
    BothReject,
    /// The lowering failed inside (see `Tally::internal`).
    Internal(String),
    /// They differ, in these parts of the comparison (none when one side
    /// rejects what the other builds).
    Differs {
        parts: Vec<Component>,
        what: String,
    },
}

/// Compares what the legacy rule and the lowering make of the text `written`.
pub fn settle<T: Subject>(
    written: &str,
    legacy: Result<T, Diagnostic>,
    lowered: Result<T, Diagnostic>,
) -> Outcome {
    let differs = |parts, what| Outcome::Differs { parts, what };
    if let Err(error) = &lowered {
        if is_internal(error) {
            return Outcome::Internal(format!("{written} lowering fails with {}", error.code));
        }
    }
    match (legacy, lowered) {
        (Ok(legacy), Ok(lowered)) => {
            let parts = compare(&legacy, &lowered);
            if parts.is_empty() {
                Outcome::Equal
            } else {
                let what = format!("{written} {}", explain(&legacy, &lowered));
                differs(parts, what)
            }
        }
        (Err(legacy), Err(error)) if legacy.code == error.code => Outcome::BothReject,
        (Err(legacy), Err(error)) => differs(
            vec![],
            format!(
                "{written} legacy rejects {} where lowering rejects {}",
                legacy.code, error.code
            ),
        ),
        (Ok(legacy), Err(error)) => differs(
            vec![],
            format!(
                "{written} lowering rejects {} where legacy builds {legacy:?}",
                error.code
            ),
        ),
        (Err(error), Ok(_)) => differs(
            vec![],
            format!(
                "{written} legacy rejects {}: {}",
                error.code, error.primary.message
            ),
        ),
    }
}

/// Compares the lowering of the node at a site to the legacy rule for its
/// unit.
fn judge(site: &Site, tokens: &[Token], cx: &LowerCx, written: &str) -> Outcome {
    match site.unit {
        Unit::Literal => settle(
            written,
            parse_constant(tokens),
            lower_constant(cx, &site.node),
        ),
        Unit::Expression => settle(
            written,
            parse_expression(tokens),
            lower_expr(cx, &site.node),
        ),
        Unit::Variable => settle(
            written,
            parse_variable(tokens),
            lower_variable(cx, &site.node),
        ),
        Unit::Statement => settle(
            written,
            parse_statement_list(tokens),
            lower_statement(cx, &site.node)
                .map(Option::into_iter)
                .map(Iterator::collect),
        ),
        Unit::Statements => settle(
            written,
            parse_statement_list(tokens),
            lower_statement_list(cx, &site.node),
        ),
        Unit::TypeDeclaration => settle(
            written,
            parse_type_declaration(tokens),
            lower_type_declaration(cx, &site.node),
        ),
        Unit::VariableInitial => settle(
            written,
            parse_variable_initial(tokens),
            lower_initial_value(cx, &site.node),
        ),
        Unit::VariableBlock | Unit::VariableBlockFacts => judge_block(site, tokens, cx, written),
        Unit::Pou | Unit::PouFacts | Unit::Configuration | Unit::ConfigurationFacts => {
            let kept = |elements: Vec<LibraryElementKind>| Library {
                elements: match site.unit {
                    Unit::PouFacts | Unit::ConfigurationFacts => without_inner_parts(elements),
                    _ => elements,
                },
            };
            // The last step of the legacy pipeline stamps the file on every position
            // the grammar left unset (the empty name of a function block
            // initialisation is one); the rule alone has not run it.
            let stamped = |library: Library| xform_assign_file_id::apply(library, &file());
            settle(
                written,
                parse_declarations(tokens).map(kept).and_then(stamped),
                lower_element(cx, &site.node).map(kept).and_then(stamped),
            )
        }
        Unit::Chart | Unit::ChartFacts => {
            let kept = |networks: Vec<Network>| match site.unit {
                Unit::ChartFacts => without_inner_chart_parts(networks),
                _ => networks,
            };
            settle(
                written,
                parse_chart(tokens).map(kept),
                lower_chart(cx, &site.node).map(kept),
            )
        }
        Unit::Member | Unit::MemberFacts => {
            let kept = |member: Member| match site.unit {
                Unit::MemberFacts => member_without_inner_parts(member),
                _ => member,
            };
            settle(
                written,
                parse_member(tokens).map(kept),
                lower_member(cx, &site.node).map(kept),
            )
        }
    }
}

/// True when a list of statements held by a node of kind `parent` is the body
/// of a declaration: it is held by neither a statement nor the file.
fn is_declaration_body(parent: SyntaxKind) -> bool {
    parent != SyntaxKind::SourceFile && disposition(parent) != Disposition::Lowered(Area::Statement)
}

/// Lowers every site of an accepted parse and compares it to the legacy rule
/// for it.
fn compare_sites(tally: &mut Tally, key: &str, text: &str, parse: &Parse, preset: &Preset) -> bool {
    let cx = LowerCx::new(file()).with_options(preset.new);
    let tokens = body_tokens(text, preset);
    let only_literals = parse
        .root
        .descendants()
        .skip(1)
        .all(|node| disposition(node.kind()) == Disposition::Lowered(Area::Literal));
    let mut differing: Vec<(&Site, Vec<Component>, String)> = Vec::new();
    let all = sites(&parse.root);
    for site in &all {
        let range = range_of(&site.node);
        let written = &text[range.0..range.1];
        let outcome = judge(
            site,
            literal_tokens(&tokens, range, site.unit.ends_in_terminator()),
            &cx,
            written,
        );
        tally.counts_mut(site.unit).record(&outcome);
        if site.unit == Unit::Statements && is_declaration_body(site.parent) {
            tally.bodies.record(&outcome);
        }
        match outcome {
            Outcome::Equal => {
                tally.literal_only_equal +=
                    usize::from(only_literals && site.unit == Unit::Literal);
            }
            Outcome::BothReject => {}
            Outcome::Internal(what) => tally.internal.push(format!("{key:?}: {what}")),
            Outcome::Differs { parts, what } => differing.push((site, parts, what)),
        }
    }
    // Sites come outer before inner, so a site contains the differing ones
    // that follow it until one starts at or after its end. A view of a node
    // (a unit that compares part of what another unit compares of the same
    // node) is not inside the node, so it neither inherits a difference nor
    // passes one on.
    for (index, (site, parts, what)) in differing.iter().enumerate() {
        let end = range_of(&site.node).1;
        let inherited = !site.unit.is_view()
            && differing[index + 1..]
                .iter()
                .take_while(|(next, _, _)| range_of(&next.node).0 < end)
                .any(|(next, _, _)| !next.unit.is_view());
        if inherited {
            tally.inherited += 1;
        } else {
            tally.difference(key, site.parent, site.node.kind(), parts, what.clone());
        }
    }
    !differing.is_empty()
}

/// Lowers a whole accepted input as a library and compares it to the legacy
/// parse.
fn compare_library(
    tally: &mut Tally,
    key: &str,
    text: &str,
    parse: &Parse,
    preset: &Preset,
    sites_differ: bool,
) {
    tally.whole_compared += 1;
    let legacy = parse_program(text, &file(), &preset.legacy);
    let lowered = lower_library(parse, &file());
    if let Err(error) = &lowered {
        if is_internal(error) {
            tally
                .internal
                .push(format!("{key:?}: the library fails with {}", error.code));
        }
    }
    match (legacy, lowered) {
        (Ok(legacy), Ok(lowered)) => {
            let parts = compare(&legacy, &lowered);
            if parts.is_empty() {
                tally.whole_equal += 1;
            } else if sites_differ {
                // The difference began at a site inside the input, and is
                // counted there.
                tally.inherited += 1;
            } else {
                let what = explain(&legacy, &lowered);
                tally.difference(
                    key,
                    SyntaxKind::SourceFile,
                    SyntaxKind::SourceFile,
                    &parts,
                    what,
                );
            }
        }
        (legacy, lowered) => tally.difference(
            key,
            SyntaxKind::SourceFile,
            SyntaxKind::SourceFile,
            &[],
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
    cases.extend(TYPES.iter().map(|text| case(Kind::Declarations, text)));
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

/// Runs both comparisons over `cases` under every preset. The inputs are
/// independent, so they are dealt out to one worker for each processor and the
/// tallies are added: the run is as long as the busiest worker, not the sum.
pub fn run(cases: &[Case], presets: &[Preset]) -> Tally {
    let workers = std::thread::available_parallelism()
        .map_or(1, |count| count.get())
        .min(cases.len().max(1));
    std::thread::scope(|scope| {
        let handles: Vec<_> = (0..workers)
            .map(|first| {
                scope.spawn(move || run_inputs(cases.iter().skip(first).step_by(workers), presets))
            })
            .collect();
        let mut tally = Tally::new();
        for handle in handles {
            match handle.join() {
                Ok(part) => tally.add(part),
                Err(panic) => std::panic::resume_unwind(panic),
            }
        }
        tally
    })
}

fn run_inputs<'a>(cases: impl Iterator<Item = &'a Case>, presets: &[Preset]) -> Tally {
    let mut tally = Tally::new();
    for case in cases {
        for preset in presets {
            tally.inputs += 1;
            let parse = new_parse(case.kind, &case.text, &preset.new);
            if !parse.is_ok() {
                tally.rejected += 1;
                continue;
            }
            let sites_differ = compare_sites(&mut tally, &case.key, &case.text, &parse, preset);
            if matches!(case.kind, Kind::Declarations | Kind::File) {
                compare_library(
                    &mut tally,
                    &case.key,
                    &case.text,
                    &parse,
                    preset,
                    sites_differ,
                );
            }
        }
    }
    tally
}

/// What the run must reach at least: the floors never fall. The sites equal to the legacy ones, by unit, those
/// literals among them in inputs that are nothing but a literal, and whole
/// inputs lowered as libraries.
const MIN_EQUAL: [(Unit, usize); 17] = [
    (Unit::Literal, 18_000),
    (Unit::Expression, 30_000),
    (Unit::Variable, 9_500),
    (Unit::Statement, 12_000),
    (Unit::Statements, 7_500),
    (Unit::TypeDeclaration, 1_500),
    (Unit::VariableInitial, 2_700),
    (Unit::VariableBlock, 2_000),
    (Unit::VariableBlockFacts, 7_500),
    (Unit::Pou, 1_900),
    (Unit::Member, 270),
    (Unit::PouFacts, 5_400),
    (Unit::MemberFacts, 470),
    (Unit::Chart, 400),
    (Unit::ChartFacts, 405),
    (Unit::Configuration, 310),
    (Unit::ConfigurationFacts, 510),
];
const MIN_BODIES_EQUAL: usize = 4_500;
const MIN_LITERAL_ONLY_EQUAL: usize = 400;
const MIN_WHOLE_COMPARED: usize = 6_750;

/// The differences no entry covers, one problem for each kind of difference
/// (the node, and the parts in which it differs) with how often it was found
/// and the first example.
fn unexplained_by_class(tally: &Tally) -> Vec<String> {
    let mut classes: Vec<(&str, usize, &str)> = Vec::new();
    for (class, example) in &tally.unexplained {
        match classes.iter_mut().find(|(seen, _, _)| seen == class) {
            Some((_, count, _)) => *count += 1,
            None => classes.push((class, 1, example)),
        }
    }
    classes
        .into_iter()
        .map(|(class, count, example)| format!("{class} x{count}, first {example}"))
        .collect()
}

/// Checks a tally against the table: no unexplained difference, every
/// entry's count is the one recorded, and the floors are met.
pub fn check(tally: &Tally) -> Vec<String> {
    let mut problems = unexplained_by_class(tally);
    problems.extend(
        tally
            .internal
            .iter()
            .map(|what| format!("lowering is not total: {what}")),
    );
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
    for (unit, floor) in MIN_EQUAL {
        let seen = tally.counts(unit).equal;
        if seen < floor {
            problems.push(format!("equal {}: {seen}, floor {floor}", unit.name()));
        }
    }
    for (name, seen, floor) in [
        (
            "equal literals in literal-only inputs",
            tally.literal_only_equal,
            MIN_LITERAL_ONLY_EQUAL,
        ),
        (
            "equal bodies of declarations",
            tally.bodies.equal,
            MIN_BODIES_EQUAL,
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
        "lowering: {} inputs ({} rejected by the new parser, not lowered)",
        tally.inputs, tally.rejected
    );
    for unit in Unit::ALL {
        let counts = tally.counts(unit);
        println!(
            "  {}: {} compared, {} equal, {} rejected by both",
            unit.name(),
            counts.compared,
            counts.equal,
            counts.both_reject
        );
    }
    println!(
        "  of the statement lists, the bodies of declarations: {} compared, {} equal",
        tally.bodies.compared, tally.bodies.equal
    );
    println!(
        "  differences: {} excepted, {} inherited from a site inside, {} unexplained",
        tally.excepted_total(),
        tally.inherited,
        tally.unexplained.len()
    );
    println!(
        "  whole inputs: {} compared ({} equal)",
        tally.whole_compared, tally.whole_equal
    );
    for (entry, seen) in DIFFERENCES.iter().zip(&tally.excepted) {
        println!("  [{seen}] {:?}: {}", entry.scope, entry.reason);
    }
}

#[test]
fn parity_when_sites_and_libraries_lowered_then_differences_are_exactly_the_exceptions() {
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
        &[Component::Dump],
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
fn check_when_a_floor_is_not_met_then_reported() {
    let mut tally = Tally::new();
    tally.excepted = DIFFERENCES.iter().map(|entry| entry.expected).collect();
    let problems = check(&tally);
    assert!(problems
        .iter()
        .any(|problem| problem.contains("equal literals")));
    assert!(problems
        .iter()
        .any(|problem| problem.contains("whole inputs compared")));
}

#[test]
fn difference_when_listed_scope_then_counted_against_its_entry_and_not_unexplained() {
    let mut tally = Tally::new();
    tally.difference(
        "tests/fixtures/lexical/oscat_marker_in_string.st (CRLF)",
        SyntaxKind::AssignStmt,
        SyntaxKind::StringLiteral,
        &[],
        String::new(),
    );
    tally.difference(
        "STEP;",
        SyntaxKind::StatementList,
        SyntaxKind::NameRef,
        &[Component::Dump],
        String::new(),
    );
    assert!(tally.unexplained.is_empty());
    // The two oscat files, the unnamed located global, then the bare name.
    assert_eq!(&tally.excepted[..4], &[1, 0, 0, 1]);
}

#[test]
pub fn literal_tokens_when_range_given_then_the_tokens_inside_without_whitespace_or_terminators() {
    let source = "x := T#5s ;";
    let tokens = tokenize_program(source, &file(), &presets()[0].legacy, 0, 0).0;
    let inside = literal_tokens(&tokens, (5, 9), false);
    let text: Vec<&str> = inside.iter().map(|token| token.text.as_str()).collect();
    assert_eq!(text, vec!["T", "#", "5", "s"]);
    assert!(literal_tokens(&tokens, (20, 25), false).is_empty());
}

#[test]
fn compare_sites_when_lowered_with_a_wrong_file_then_reported_as_a_difference() {
    // The strict comparison is what the run relies on: an object that equals
    // the legacy one except in where it is must be reported. A negated name
    // lowered for another file is such an object.
    let preset = &presets()[0];
    let text = "x := -a;";
    let parse = new_parse(Kind::Statements, text, &preset.new);
    let tokens = tokenize_program(text, &file(), &preset.legacy, 0, 0).0;
    let node = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::UnaryExpr);
    let legacy = parse_expression(literal_tokens(&tokens, (5, 7), false));
    let parts = |file: FileId| {
        legacy
            .as_ref()
            .ok()
            .zip(
                node.as_ref()
                    .and_then(|node| lower_expr(&LowerCx::new(file), node).ok()),
            )
            .map(|(legacy, lowered)| compare(legacy, &lowered))
    };
    assert_eq!(parts(file()), Some(vec![]));
    assert_eq!(
        parts(FileId::from_string("other.st")),
        Some(vec![Component::Spans])
    );
}

#[test]
fn compare_sites_when_a_statement_is_lowered_for_another_file_then_reported_as_a_difference() {
    // The same holds for a statement: its target, its operator and its value
    // are positioned in a file, and a statement lowered for another one differs
    // from the legacy statement in position and in nothing else.
    let preset = &presets()[0];
    let text = "IF a THEN x[1] := b; END_IF;";
    let parse = new_parse(Kind::Statements, text, &preset.new);
    let tokens = body_tokens(text, preset);
    let node = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::IfStmt)
        .expect("an IF");
    let legacy = parse_statement_list(literal_tokens(&tokens, range_of(&node), true));
    let parts = |file: FileId| {
        legacy
            .as_ref()
            .ok()
            .zip(
                lower_statement(&LowerCx::new(file), &node)
                    .ok()
                    .map(|statement| statement.into_iter().collect::<Vec<_>>()),
            )
            .map(|(legacy, lowered)| compare(legacy, &lowered))
    };
    assert_eq!(parts(file()), Some(vec![]));
    assert_eq!(
        parts(FileId::from_string("other.st")),
        Some(vec![Component::Spans])
    );
}

#[test]
pub fn literal_tokens_when_block_statement_omits_its_terminator_then_a_statement_takes_the_inserted_one(
) {
    let preset = presets()
        .into_iter()
        .find(|preset| preset.name == "all-flags")
        .expect("the preset with every flag");
    let text = "IF a THEN b := 1; END_IF (* c *)\nx := 2;";
    let tokens = body_tokens(text, &preset);
    let range = (0, "IF a THEN b := 1; END_IF".len());
    let last = |terminated: bool| {
        literal_tokens(&tokens, range, terminated)
            .last()
            .map(|token| (token.token_type.clone(), token.text.clone()))
    };
    assert_eq!(last(false), Some((TokenType::EndIf, "END_IF".to_string())));
    assert_eq!(last(true), Some((TokenType::Semicolon, String::new())));
    // A `;` that is there is the last token, and nothing is added after it.
    let written = "x := 2;";
    let tokens = body_tokens(written, &preset);
    let at = literal_tokens(&tokens, (0, written.len()), true);
    assert_eq!(at.last().map(|token| token.text.as_str()), Some(";"));
}
