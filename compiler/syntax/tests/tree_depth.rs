//! The tree depth invariant: no tree is deeper than `MAX_DEPTH`, whatever the
//! construct that nests, whether it nests by recursion (parentheses, calls,
//! statements, declarations) or by a chain that grows leftwards (`a + b + c`,
//! `a.b.c`, `a[1][2]`, `a^^`). Each construct is parsed as deep as the limit
//! allows, one step past it, and far past it, on a stack of 1 MiB, the
//! smallest the compiler runs on. The tree keeps every byte and parsing ends
//! at every depth.

use ironplc_syntax::{
    parse_expression, parse_source_file, parse_statements, ErrorKind, Parse, ParseOptions,
    SyntaxNode, MAX_DEPTH,
};

/// What a construct is parsed as.
#[derive(Clone, Copy)]
enum Entry {
    Expression,
    Statements,
    File,
}

/// A construct that nests, and what its nesting makes of the tree.
struct Construct {
    name: &'static str,
    entry: Entry,
    /// The source nested `n` times.
    build: fn(usize) -> String,
    /// True when each nesting adds exactly one node on the deepest path, so
    /// the tree is exactly `MAX_DEPTH` deep where it is as deep as it may be.
    one_node_a_level: bool,
}

const CONSTRUCTS: &[Construct] = &[
    Construct {
        name: "dereferences",
        entry: Entry::Expression,
        build: |n| format!("a{}", "^".repeat(n)),
        one_node_a_level: true,
    },
    Construct {
        name: "members",
        entry: Entry::Expression,
        build: |n| format!("a{}", ".b".repeat(n)),
        one_node_a_level: true,
    },
    Construct {
        name: "subscripts",
        entry: Entry::Expression,
        build: |n| format!("a{}", "[1]".repeat(n)),
        one_node_a_level: true,
    },
    Construct {
        name: "sum",
        entry: Entry::Expression,
        build: |n| vec!["a"; n].join("+"),
        one_node_a_level: true,
    },
    Construct {
        name: "parentheses",
        entry: Entry::Expression,
        build: |n| format!("{}1{}", "(".repeat(n), ")".repeat(n)),
        one_node_a_level: false,
    },
    Construct {
        name: "calls",
        entry: Entry::Expression,
        build: |n| format!("{}1{}", "f(".repeat(n), ")".repeat(n)),
        one_node_a_level: false,
    },
    Construct {
        name: "if statements",
        entry: Entry::Statements,
        build: |n| format!("{}x := 1;{}", "IF a THEN ".repeat(n), " END_IF;".repeat(n)),
        one_node_a_level: false,
    },
    Construct {
        name: "case statements",
        entry: Entry::Statements,
        build: |n| {
            format!(
                "{}x := 1;{}",
                "CASE a OF 1: ".repeat(n),
                " END_CASE;".repeat(n)
            )
        },
        one_node_a_level: false,
    },
    Construct {
        name: "namespaces",
        entry: Entry::File,
        build: |n| format!("{}{}", "NAMESPACE n ".repeat(n), "END_NAMESPACE ".repeat(n)),
        one_node_a_level: false,
    },
];

/// Runs `check` on a thread with a stack of 1 MiB, so that a recursion the
/// limit fails to bound overflows here instead of passing on a roomy stack.
fn on_small_stack(check: impl FnOnce() + Send + 'static) {
    let handle = std::thread::Builder::new()
        .stack_size(1024 * 1024)
        .spawn(check);
    assert!(handle.is_ok_and(|handle| handle.join().is_ok()));
}

fn parse(construct: &Construct, source: &str) -> Parse {
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

fn nesting_errors(parsed: &Parse) -> usize {
    parsed
        .errors
        .iter()
        .filter(|error| error.kind == ErrorKind::NestingTooDeep)
        .count()
}

/// The most the construct nests before the parse reports the depth.
fn deepest_allowed(construct: &Construct) -> usize {
    (1..)
        .find(|n| nesting_errors(&parse(construct, &(construct.build)(*n))) > 0)
        .map_or(0, |first_too_deep| first_too_deep - 1)
}

#[test]
fn parse_when_nesting_is_as_deep_as_allowed_then_no_error_and_tree_within_the_limit() {
    on_small_stack(|| {
        for construct in CONSTRUCTS {
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
        for construct in CONSTRUCTS {
            let source = (construct.build)(deepest_allowed(construct) + 1);
            let parsed = parse(construct, &source);
            assert_well_formed(&source, &parsed);
            assert_eq!(nesting_errors(&parsed), 1, "{}", construct.name);
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
        for construct in CONSTRUCTS {
            let source = (construct.build)(20_000);
            let parsed = parse(construct, &source);
            assert_well_formed(&source, &parsed);
            assert!(nesting_errors(&parsed) > 0, "{}", construct.name);
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
