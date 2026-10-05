//! The tree depth invariant: no tree is deeper than `MAX_DEPTH`, whatever the
//! construct that nests, whether it nests by recursion (parentheses, calls,
//! statements, declarations) or by a chain that grows leftwards (`a + b + c`,
//! `a.b.c`, `a[1][2]`, `a^^`). Each construct is parsed as deep as the limit
//! allows, one step past it, and far past it, from a thread of 1 MiB: the
//! parse owns the stack it needs, so what the caller has is enough. The tree
//! keeps every byte and parsing ends at every depth. What the limit is for,
//! that every stage of a compile survives a tree as deep as it on the stack
//! budget, is proved by `ironplc-project`'s `tests/stack_budget.rs`.

use ironplc_dsl::core::FileId;
use ironplc_dsl::fold::Fold;
use ironplc_dsl::stack::within_stack_budget;
use ironplc_dsl::visitor::Visitor;
use ironplc_syntax::lower::expressions::lower_expr;
use ironplc_syntax::lower::LowerCx;
use ironplc_syntax::{
    parse_expression, parse_source_file, parse_statements, ErrorKind, Parse, ParseOptions,
    SyntaxNode, MAX_DEPTH,
};
use ironplc_test::nesting::{Entry, Nesting, NESTINGS};
use std::convert::Infallible;

/// The smallest stack a caller has: the main thread on Windows.
const SMALLEST_STACK: usize = 1024 * 1024;

/// Runs `check` on a thread with the smallest stack, so that what the parse
/// leaves on the caller's stack, the walk and the drop of the tree, is
/// bounded by the limit too.
fn on_small_stack(check: impl FnOnce() + Send + 'static) {
    let handle = std::thread::Builder::new()
        .stack_size(SMALLEST_STACK)
        .spawn(check);
    assert!(handle.is_ok_and(|handle| handle.join().is_ok()));
}

fn parse(construct: &Nesting, source: &str) -> Parse {
    let options = ParseOptions::all();
    match construct.entry {
        Entry::Expression => parse_expression(source, &options),
        Entry::Statements => parse_statements(source, &options),
        Entry::File => parse_source_file(source, &options),
    }
}

/// How many nodes the longest path from `node` down holds.
fn depth_of(node: &SyntaxNode) -> usize {
    node.descendants()
        .map(|descendant| descendant.ancestors().count())
        .max()
        .unwrap_or(0)
}

fn assert_well_formed(source: &str, parsed: &Parse) {
    assert_eq!(parsed.root.text().to_string(), source);
    for error in &parsed.errors {
        assert!(usize::from(error.range.end()) <= source.len(), "{error}");
    }
    assert!(depth_of(&parsed.root) <= MAX_DEPTH);
}

/// True when the parse reports the depth as the problem `P0019`, the way a
/// compile shows it.
fn reported_as_p0019(parsed: &Parse) -> bool {
    parsed
        .diagnostics(&FileId::from_string("t.st"))
        .iter()
        .any(|diagnostic| diagnostic.code == "P0019")
}

fn nesting_errors(parsed: &Parse) -> usize {
    parsed
        .errors
        .iter()
        .filter(|error| error.kind == ErrorKind::NestingTooDeep)
        .count()
}

/// The most the construct nests before the parse reports the depth.
fn deepest_allowed(construct: &Nesting) -> usize {
    construct.deepest(MAX_DEPTH, |source| {
        nesting_errors(&parse(construct, source)) > 0
    })
}

#[test]
fn parse_when_nesting_is_as_deep_as_allowed_then_no_error_and_tree_within_the_limit() {
    on_small_stack(|| {
        for construct in NESTINGS {
            let deepest = deepest_allowed(construct);
            assert!(deepest > 10, "{}: {deepest}", construct.name);
            let source = (construct.build)(deepest);
            let parsed = parse(construct, &source);
            assert_well_formed(&source, &parsed);
            assert!(parsed.errors.is_empty(), "{}", construct.name);
            let depth = depth_of(&parsed.root);
            if construct.one_node_a_level {
                assert_eq!(depth, MAX_DEPTH, "{}", construct.name);
            } else {
                // A rule that recurses leaves one level for the error node it
                // would degrade to, and opens a few nodes for each step.
                assert!(depth + 8 >= MAX_DEPTH, "{}: {depth}", construct.name);
            }
        }
    });
}

#[test]
fn parse_when_nesting_is_one_step_past_the_limit_then_one_depth_error_and_tree_within_the_limit() {
    on_small_stack(|| {
        for construct in NESTINGS {
            let source = (construct.build)(deepest_allowed(construct) + 1);
            let parsed = parse(construct, &source);
            assert_well_formed(&source, &parsed);
            assert_eq!(nesting_errors(&parsed), 1, "{}", construct.name);
            assert!(reported_as_p0019(&parsed), "{}", construct.name);
            if construct.one_node_a_level {
                assert_eq!(parsed.errors.len(), 1, "{}", construct.name);
                assert_eq!(depth_of(&parsed.root), MAX_DEPTH, "{}", construct.name);
            }
        }
    });
}

#[test]
fn parse_when_nesting_is_far_past_the_limit_then_it_ends_with_the_text_kept_and_a_depth_error() {
    on_small_stack(|| {
        for construct in NESTINGS {
            let source = (construct.build)(20_000);
            let parsed = parse(construct, &source);
            assert_well_formed(&source, &parsed);
            assert!(nesting_errors(&parsed) > 0, "{}", construct.name);
            assert!(reported_as_p0019(&parsed), "{}", construct.name);
        }
    });
}

#[test]
fn parse_when_chain_is_past_the_limit_then_the_links_beyond_it_join_the_last_node_and_one_error_is_reported(
) {
    let source = format!("a{}", "^".repeat(MAX_DEPTH + 10));
    let parsed = parse_expression(&source, &ParseOptions::all());
    assert_well_formed(&source, &parsed);
    assert_eq!(parsed.errors.len(), 1);
    assert_eq!(parsed.errors[0].kind, ErrorKind::NestingTooDeep);
    assert!(parsed.errors[0].message.contains(&MAX_DEPTH.to_string()));
}

#[test]
fn parse_when_pragma_condition_nests_past_the_limit_then_depth_error_instead_of_a_stack_overflow() {
    on_small_stack(|| {
        let open = "(".repeat(20_000);
        let close = ")".repeat(20_000);
        let negations = "NOT ".repeat(20_000);
        for condition in [format!("{open}TRUE{close}"), format!("{negations}TRUE")] {
            let source = format!("{{IF {condition}}} x := 1; {{END_IF}}");
            let parsed = parse_statements(&source, &ParseOptions::all());
            assert_eq!(parsed.root.text().to_string(), source);
            assert_eq!(nesting_errors(&parsed), 1);
        }
    });
}

struct Walk;
impl Visitor<Infallible> for Walk {
    type Value = ();
}

struct Identity;
impl Fold<Infallible> for Identity {}

#[test]
fn compile_when_nesting_is_as_deep_as_allowed_then_every_stage_fits_the_stack_budget() {
    // The limit is derived from the stack budget, and this is the proof: every
    // stage of the pipeline that exists for this parser, over every nesting at
    // the limit, on a thread of exactly the budget, in whichever profile the
    // test is built. The stages are the parse and the tree walk, lowering,
    // `Visitor`, `Fold` and the drop of the object tree, and the drop of the
    // syntax tree. Only expressions lower to objects today, so statements and
    // declarations stop at the syntax tree. The stages of the legacy parser's
    // pipeline are in `ironplc-project`, `tests/stack_budget.rs`.
    within_stack_budget(|| {
        for construct in NESTINGS {
            let source = (construct.build)(deepest_allowed(construct));
            let parsed = parse(construct, &source);
            assert!(parsed.errors.is_empty(), "{}", construct.name);
            assert!(
                depth_of(&parsed.root) + 8 >= MAX_DEPTH,
                "{}",
                construct.name
            );
            let node = parsed.root.first_child();
            if let (Entry::Expression, Some(node)) = (&construct.entry, node) {
                let cx = LowerCx::new(FileId::from_string("t.st"));
                let lowered = lower_expr(&cx, &node);
                assert!(lowered.is_ok(), "{}", construct.name);
                if let Ok(expr) = lowered {
                    assert!(Walk.visit_expr(&expr).is_ok(), "{}", construct.name);
                    assert!(Identity.fold_expr(expr).is_ok(), "{}", construct.name);
                }
            }
        }
    });
}

#[test]
fn tokenize_when_input_nests_far_past_the_limit_then_it_ends_on_the_callers_small_stack() {
    // Tokenizing runs on the caller's stack and spawns no thread, so it must
    // not recurse as deep as the input does: comments, strings, pragma
    // conditions and `{IF}` nesting, each 20 000 levels deep, from a thread of
    // 1 MiB.
    on_small_stack(|| {
        let options = ParseOptions::all();
        let open = "(".repeat(20_000);
        let close = ")".repeat(20_000);
        let sources = [
            format!("{}x{}", "(* ".repeat(20_000), " *)".repeat(20_000)),
            format!("{{IF {open}TRUE{close}}} x := 1; {{END_IF}}"),
            format!("{{IF {}TRUE}} x := 1; {{END_IF}}", "NOT ".repeat(20_000)),
            format!(
                "{}x := 1;{}",
                "{IF TRUE} ".repeat(20_000),
                "{END_IF} ".repeat(20_000)
            ),
            format!("{open}1{close}"),
        ];
        for source in sources {
            let (tokens, _) = ironplc_syntax::tokenize(&source, &options);
            let text: String = tokens.iter().map(|token| token.text).collect();
            assert_eq!(text, source);
        }
    });
}
