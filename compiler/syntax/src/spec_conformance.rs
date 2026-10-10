//! Spec conformance tests for the lossless tree and its lowering
//! (syntax-owned requirements `REQ-PT-syntax-NNN`).
//!
//! Each test is annotated with `#[spec_test(REQ_PT_syntax_NNN)]`, which adds
//! `#[test]` and references a build-script-generated constant, so the test
//! fails to compile if the requirement is removed from the design. The
//! `all_spec_requirements_have_tests` meta-test asserts every syntax-owned
//! requirement has a test here.
//!
//! The inputs are the corpus the repository shares (`ironplc_test::corpus`):
//! every `.st` file in three spellings, read in place.
//!
//! See `specs/design/parse-tree-architecture.md`.

use crate::lower::{disposition, lower_library, Disposition};
use crate::{parse_source_file, Parse, ParseOptions, SyntaxKind};
use ironplc_dsl::common::Library;
use ironplc_dsl::core::{FileId, SourceSpan};
use ironplc_dsl::stack::within_stack_budget;
use ironplc_dsl::visitor::Visitor;
use ironplc_test::corpus::{CRLF, TABS};
use ironplc_test::nesting::{Entry, NESTINGS};
use spec_test_macro::spec_test;
use std::collections::BTreeSet;
use std::convert::Infallible;

#[test]
fn all_spec_requirements_have_tests() {
    assert!(
        crate::spec_requirements::UNTESTED.is_empty(),
        "Requirements in spec with no conformance test: {:?}",
        crate::spec_requirements::UNTESTED
    );
}

/// The corpus in every spelling: name and text.
fn corpus() -> Vec<(String, String)> {
    ironplc_test::corpus::variants()
}

/// The corpus as written, without the CRLF and tab spellings.
fn corpus_as_written() -> Vec<(String, String)> {
    corpus()
        .into_iter()
        .filter(|(name, _)| !name.ends_with(CRLF) && !name.ends_with(TABS))
        .collect()
}

/// The options every check runs under: the defaults, and every flag on.
fn option_sets() -> [ParseOptions; 2] {
    [ParseOptions::default(), ParseOptions::all()]
}

fn file() -> FileId {
    FileId::from_string("corpus.st")
}

/// True for a problem that reports the compiler itself failing: `P9998`, an
/// internal error, and `P9999`, a capability that is not implemented.
fn is_internal(code: &str) -> bool {
    code == "P9998" || code == "P9999"
}

/// The text of the tokens of a tree that are not trivia, in order.
fn significant_text(parse: &Parse) -> Vec<String> {
    parse
        .root
        .descendants_with_tokens()
        .filter_map(|element| element.into_token())
        .filter(|token| !token.kind().is_trivia())
        .map(|token| token.text().to_string())
        .collect()
}

/// Every `SourceSpan` of an object, in the order they are visited.
#[derive(Default)]
struct Spans(Vec<SourceSpan>);

impl Visitor<Infallible> for Spans {
    type Value = ();

    fn visit_source_span(&mut self, node: &SourceSpan) -> Result<(), Infallible> {
        self.0.push(node.clone());
        Ok(())
    }
}

fn spans_of(library: &Library) -> Vec<SourceSpan> {
    let mut spans = Spans::default();
    let Ok(()) = spans.walk(library);
    spans.0
}

/// REQ-PT-syntax-001: For every file of the corpus, in LF, CRLF and
/// tab-indented spelling, under every option set, the text of the tree equals
/// the text parsed, whether or not the parse reports errors.
#[spec_test(REQ_PT_syntax_001)]
fn tree_spec_req_pt_001_text_of_the_tree_is_the_text_parsed() {
    for (name, text) in corpus() {
        for options in option_sets() {
            let parse = parse_source_file(&text, &options);
            assert_eq!(parse.root.text().to_string(), text, "{name}");
        }
    }
}

/// REQ-PT-syntax-002: An OSCAT ranged-comment pair, and with `allow_pragma_if`
/// each branch of an `{IF}` that is not taken, is one trivia token of the
/// tree holding the bytes it covers; the grammar does not read what is inside.
#[spec_test(REQ_PT_syntax_002)]
fn region_spec_req_pt_002_ranged_comment_and_untaken_branch_are_one_trivia_token() {
    let regions = |source: &str, options: &ParseOptions, kind: SyntaxKind| {
        let parse = parse_source_file(source, options);
        assert!(parse.is_ok(), "{source:?}: {:?}", parse.errors);
        assert_eq!(parse.root.text().to_string(), source);
        assert!(kind.is_trivia());
        parse
            .root
            .descendants_with_tokens()
            .filter_map(|element| element.into_token())
            .filter(|token| token.kind() == kind)
            .map(|token| token.text().to_string())
            .collect::<Vec<_>>()
    };
    let ranged = "PROGRAM p\n(*@KEY@:D*)\nnot ' code ? {\n(*@KEY@:END_D*)\nEND_PROGRAM\n";
    assert_eq!(
        regions(ranged, &ParseOptions::default(), SyntaxKind::RangedComment),
        vec!["(*@KEY@:D*)\nnot ' code ? {\n(*@KEY@:END_D*)"]
    );
    let conditional = ParseOptions {
        allow_pragma_if: true,
        ..ParseOptions::all()
    };
    let source = "PROGRAM p {IF false} x := ; {ELSE} y := 1; {END_IF} END_PROGRAM";
    assert_eq!(
        regions(source, &conditional, SyntaxKind::InactiveRegion),
        vec![" x := ; "]
    );
}

/// REQ-PT-syntax-003: Lowering is total over the text the parser accepts: for
/// every file of the corpus and every prefix of it, the parse and the lowering
/// return, and a lowering that fails reports a problem other than an internal
/// error or a capability that is not implemented.
#[spec_test(REQ_PT_syntax_003)]
fn lowering_spec_req_pt_003_total_over_accepted_text_for_files_and_prefixes() {
    let lower = |name: &str, text: &str, options: &ParseOptions| {
        let parse = parse_source_file(text, options);
        if let Err(diagnostic) = lower_library(&parse, &file()) {
            assert!(
                !is_internal(&diagnostic.code),
                "{name}: {}: {}",
                diagnostic.code,
                diagnostic.primary.message
            );
        }
    };
    for (name, text) in corpus() {
        for options in option_sets() {
            lower(&name, &text, &options);
        }
    }
    let every_flag = ParseOptions::all();
    for (name, text) in corpus_as_written() {
        // Thirty-one cuts spread over the text, each moved back to a
        // character boundary.
        for step in 1..32 {
            let mut end = text.len() * step / 32;
            while !text.is_char_boundary(end) {
                end -= 1;
            }
            lower(&format!("{name} up to {end}"), &text[..end], &every_flag);
        }
    }
}

/// REQ-PT-syntax-004: Lowering a parse that reported errors fails with its
/// primary error (REQ-PT-syntax-009), positioned in the file given, and builds
/// no object.
#[spec_test(REQ_PT_syntax_004)]
fn lowering_spec_req_pt_004_refuses_a_parse_with_errors_with_its_primary_error() {
    let named = FileId::from_string("bad.st");
    for source in [
        "PROGRAM p x END_PROGRAM",
        "PROGRAM p VAR x : INT; END_VAR x := ; END_PROGRAM",
        "PROGRAM p VAR x : INT; END_VAR x := ; // note\nEND_PROGRAM",
    ] {
        let parse = parse_source_file(source, &ParseOptions::default());
        assert!(!parse.is_ok(), "{source}");
        let refused = lower_library(&parse, &named).err();
        let primary = parse.primary_diagnostic(&named);
        assert!(refused.is_some(), "{source}");
        let describe = |diagnostic: Option<ironplc_dsl::diagnostic::Diagnostic>| {
            diagnostic.map(|d| {
                (
                    d.code,
                    d.primary.file_id,
                    d.primary.location.start,
                    d.primary.location.end,
                )
            })
        };
        assert_eq!(describe(refused), describe(primary), "{source}");
    }
}

/// REQ-PT-syntax-009: A parse reports every error it finds, and one of them is
/// primary: the error of the earliest stage that finds errors, and the earliest
/// in the text among the errors of that stage.
#[spec_test(REQ_PT_syntax_009)]
fn parse_spec_req_pt_009_the_primary_error_is_the_earliest_stage_then_the_earliest_position() {
    let check = |name: &str, text: &str, options: &ParseOptions| {
        let parse = parse_source_file(text, options);
        let Some(primary) = parse.primary_error() else {
            assert!(parse.errors.is_empty(), "{name}");
            return false;
        };
        for error in &parse.errors {
            assert!(
                (error.kind.rank(), error.range.start(), error.range.end())
                    >= (
                        primary.kind.rank(),
                        primary.range.start(),
                        primary.range.end()
                    ),
                "{name}: {error:?} precedes the primary {primary:?}"
            );
        }
        true
    };
    let mut with_errors = 0;
    for (name, text) in corpus() {
        for options in option_sets() {
            with_errors += usize::from(check(&name, &text, &options));
        }
    }
    // Cuts of the corpus add grammar errors to the gated forms of a file.
    for (name, text) in corpus_as_written() {
        for step in 1..16 {
            let mut end = text.len() * step / 16;
            while !text.is_char_boundary(end) {
                end -= 1;
            }
            for options in option_sets() {
                with_errors += usize::from(check(&name, &text[..end], &options));
            }
        }
    }
    assert!(with_errors > 100, "only {with_errors} parses had errors");
    // Every kind of error is in exactly one stage.
    let listed: usize = crate::STAGES.iter().map(|stage| stage.kinds.len()).sum();
    let distinct: BTreeSet<String> = crate::STAGES
        .iter()
        .flat_map(|stage| stage.kinds.iter().map(|kind| format!("{kind:?}")))
        .collect();
    assert_eq!(listed, distinct.len());
}

/// REQ-PT-syntax-005: Every kind of node that a lowering rule owns occurs in a
/// file of the corpus that lowers, so a construct added to the grammar has a
/// rule and a case before the corpus passes.
#[spec_test(REQ_PT_syntax_005)]
fn lowering_spec_req_pt_005_every_node_kind_with_a_rule_occurs_in_the_corpus() {
    let mut seen = BTreeSet::new();
    for (_, text) in corpus_as_written() {
        let parse = parse_source_file(&text, &ParseOptions::all());
        if parse.is_ok() && lower_library(&parse, &file()).is_ok() {
            seen.extend(parse.root.descendants().map(|node| node.kind() as u16));
        }
    }
    let missing: Vec<SyntaxKind> = SyntaxKind::ALL
        .iter()
        .filter(|kind| kind.node().is_some())
        .filter(|kind| matches!(disposition(**kind), Disposition::Lowered(_)))
        .filter(|kind| !seen.contains(&(**kind as u16)))
        .copied()
        .collect();
    assert!(missing.is_empty(), "no corpus file holds {missing:?}");
}

/// REQ-PT-syntax-006: Every span of a lowered library carries the file the
/// text came from and lies inside the text, on character boundaries.
#[spec_test(REQ_PT_syntax_006)]
fn lowering_spec_req_pt_006_every_span_is_in_the_file_and_inside_the_text() {
    let named = file();
    let mut checked = 0usize;
    for (name, text) in corpus() {
        let parse = parse_source_file(&text, &ParseOptions::all());
        let Ok(library) = lower_library(&parse, &named) else {
            continue;
        };
        for span in spans_of(&library) {
            assert_eq!(span.file_id, named, "{name}");
            assert!(span.start <= span.end && span.end <= text.len(), "{name}");
            assert!(
                text.is_char_boundary(span.start) && text.is_char_boundary(span.end),
                "{name}: {}..{}",
                span.start,
                span.end
            );
            checked += 1;
        }
    }
    assert!(checked > 10_000, "only {checked} spans checked");
}

/// REQ-PT-syntax-007: Spelling the trivia differently (CRLF line ends, tabs
/// for indentation) does not change what is lowered: a file and a spelling
/// of it with the same significant tokens lower to equal libraries.
#[spec_test(REQ_PT_syntax_007)]
fn lowering_spec_req_pt_007_trivia_spelling_does_not_change_the_library() {
    let files = corpus();
    let mut compared = 0usize;
    for (name, text) in files
        .iter()
        .filter(|(name, _)| !name.ends_with(CRLF) && !name.ends_with(TABS))
    {
        let options = ParseOptions::all();
        let plain = parse_source_file(text, &options);
        let Ok(library) = lower_library(&plain, &file()) else {
            continue;
        };
        for suffix in [CRLF, TABS] {
            let Some((_, spelling)) = files
                .iter()
                .find(|(key, _)| *key == format!("{name}{suffix}"))
            else {
                continue;
            };
            let parse = parse_source_file(spelling, &options);
            if significant_text(&parse) != significant_text(&plain) {
                // The spelling reached inside a literal (a string that holds a
                // line end or four spaces): it is not a trivia difference.
                continue;
            }
            let respelled = lower_library(&parse, &file());
            assert_eq!(respelled.as_ref().ok(), Some(&library), "{name}{suffix}");
            compared += 1;
        }
    }
    assert!(compared > 150, "only {compared} spellings compared");
}

/// REQ-PT-syntax-008: Lowering a tree as deep as the parser allows, for every
/// construct that nests, completes on the stack budget and reports no
/// internal error.
#[spec_test(REQ_PT_syntax_008)]
fn lowering_spec_req_pt_008_deepest_allowed_nesting_lowers_on_the_stack_budget() {
    within_stack_budget(|| {
        for construct in NESTINGS {
            let deepest = construct.deepest(crate::MAX_DEPTH, |source| {
                let parse = parse_for(construct.entry, source);
                parse
                    .errors
                    .iter()
                    .any(|error| error.kind == crate::ErrorKind::NestingTooDeep)
            });
            let source = (construct.build)(deepest);
            let parse = parse_for(construct.entry, &source);
            assert!(parse.is_ok(), "{}", construct.name);
            let lowered = match construct.entry {
                Entry::Expression => lower_expression(&parse),
                Entry::Statements => crate::lower::lower_statements(&parse, &file()).map(drop),
                Entry::File => lower_library(&parse, &file()).map(drop),
            };
            if let Err(diagnostic) = lowered {
                assert!(
                    !is_internal(&diagnostic.code),
                    "{}: {}",
                    construct.name,
                    diagnostic.primary.message
                );
            }
        }
    });
}

fn parse_for(entry: Entry, source: &str) -> Parse {
    let options = ParseOptions::all();
    match entry {
        Entry::Expression => crate::parse_expression(source, &options),
        Entry::Statements => crate::parse_statements(source, &options),
        Entry::File => parse_source_file(source, &options),
    }
}

fn lower_expression(parse: &Parse) -> Result<(), ironplc_dsl::diagnostic::Diagnostic> {
    match parse.root.first_child() {
        Some(node) => {
            let cx = crate::lower::LowerCx::new(file());
            crate::lower::expressions::lower_expr(&cx, &node).map(drop)
        }
        None => Ok(()),
    }
}
