//! The stack budget of the compile pipeline: every stage that recurses as deep
//! as the tree it is given runs on a stack of `STACK_BUDGET`, and a tree as
//! deep as `MAX_DEPTH` fits it. The stages after the parse (analysis, code
//! generation and printing) are reached only through a front end, so the proof
//! is here, where they all are, once for each front end that is a way into
//! them: one table, [`FRONTENDS`], and one test body over its rows.
//!
//! - `production` is `ironplc_parser::parse_program`, the one production
//!   consumers run: the legacy parser, or the one on the lossless tree where
//!   the `cst-frontend` feature of `ironplc-parser` is on. The legacy parser
//!   has no limit of its own, so it is asked for `MAX_DEPTH` levels.
//! - `cst` is the parser of `ironplc-syntax` and its lowering, called
//!   directly, so that this proof does not depend on the feature. That parser
//!   limits the depth of its *tree*, and a program around the nesting adds
//!   levels of its own, so it is asked for the deepest nesting it accepts,
//!   found by bisection, which is as deep as its tree may be (`MAX_DEPTH`
//!   nodes). It parses the nestings of files too.
//!
//! The stages of the parser in `ironplc-syntax` before lowering are proved in
//! its own `tests/tree_depth.rs`.

// Allow large errors because this is a compiler - we expect large errors.
#![allow(clippy::result_large_err)]

use ironplc_analyzer::stages::analyze;
use ironplc_codegen::{CodegenOptions, EmptyLookup};
use ironplc_dsl::common::Library;
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::stack::{within_stack_budget, MAX_DEPTH};
use ironplc_parser::options::CompilerOptions;
use ironplc_parser::parse_program;
use ironplc_plc2plc::write_to_string;
use ironplc_project::{compile, MemoryBackedProject};
use ironplc_syntax::lower::lower_library;
use ironplc_syntax::{parse_source_file, ParseOptions};
use ironplc_test::nesting::{Entry, Nesting, NESTINGS};

/// The constructs the analyzer types without a diagnostic in the program that
/// [`program`] builds, and so the ones code generation runs for.
const TYPED: &[&str] = &[
    "sum",
    "parentheses",
    "calls",
    "if statements",
    "case statements",
];

/// A way into the stages after the parse.
struct Frontend {
    name: &'static str,
    /// Source text to a library, or the one diagnostic that says why not.
    parse: fn(&str, &CompilerOptions) -> Result<Library, Diagnostic>,
    /// True when the front end has the construct.
    supports: fn(&Nesting) -> bool,
    /// True when the front end limits how deep a tree may be, so that the
    /// depth it accepts has to be found.
    limits_depth: bool,
}

fn production(source: &str, options: &CompilerOptions) -> Result<Library, Diagnostic> {
    parse_program(source, &FileId::default(), options)
}

fn cst(source: &str, _options: &CompilerOptions) -> Result<Library, Diagnostic> {
    let parse = parse_source_file(source, &ParseOptions::all());
    lower_library(&parse, &FileId::default())
}

/// The front ends. A new way into the stages is one more row.
const FRONTENDS: &[Frontend] = &[
    Frontend {
        name: "production",
        parse: production,
        // The legacy grammar has no nested namespaces.
        supports: |construct| construct.entry != Entry::File,
        limits_depth: false,
    },
    Frontend {
        name: "cst",
        parse: cst,
        supports: |_| true,
        limits_depth: true,
    },
];

/// The options every front end is run under: the defaults, with the one
/// keyword that the nesting of files needs.
fn options() -> CompilerOptions {
    CompilerOptions {
        allow_namespace: true,
        ..CompilerOptions::default()
    }
}

/// A program that holds `construct` nested `n` times, or the file itself for a
/// construct that is a file.
fn program(construct: &Nesting, n: usize) -> String {
    let body = (construct.build)(n);
    let head = "FUNCTION f : INT VAR_INPUT v : INT; END_VAR f := v; END_FUNCTION\n\
                PROGRAM main VAR a : INT; c : BOOL; x : INT; END_VAR\n";
    match construct.entry {
        Entry::Expression => format!("{head}x := {body};\nEND_PROGRAM"),
        Entry::Statements => format!("{head}{body}\nEND_PROGRAM"),
        Entry::File => body,
    }
}

/// How deep `construct` is nested for `frontend` and `limit`: the limit, or
/// where the front end limits depth, the deepest nesting it accepts up to the
/// limit.
fn depth_for(frontend: &Frontend, construct: &Nesting, limit: usize) -> usize {
    if !frontend.limits_depth {
        return limit;
    }
    // The program around the nesting is the text that is parsed.
    let deepest = construct.deepest_by(limit, |n| {
        let parsed = (frontend.parse)(&program(construct, n), &options());
        parsed.is_err_and(|diagnostic| diagnostic.code == "P0019")
    });
    deepest.min(limit)
}

#[test]
fn compile_when_nesting_is_as_deep_as_allowed_then_every_stage_fits_the_stack_budget() {
    // Parse, analysis, code generation and printing, over every nesting that
    // `MAX_DEPTH` allows the front end, and the drop of everything they made,
    // on a thread of exactly the budget, in whichever profile the test is
    // built.
    within_stack_budget(|| {
        let options = options();
        for frontend in FRONTENDS {
            for construct in NESTINGS.iter().filter(|c| (frontend.supports)(c)) {
                let label = format!("{} / {}", frontend.name, construct.name);
                let depth = depth_for(frontend, construct, MAX_DEPTH);
                println!("{label}: {depth} levels");
                // A level of a construct is at most four levels of the tree, so
                // the nesting reaches the depth the compiler is built for.
                assert!(depth * 4 >= MAX_DEPTH, "{label}: only {depth} levels");

                let parsed = (frontend.parse)(&program(construct, depth), &options);
                assert!(parsed.is_ok(), "{label}: {:?}", parsed.err());
                let Ok(parsed) = parsed else { continue };

                let analysed = analyze(&[&parsed], &options);
                assert!(analysed.is_ok(), "{label}");
                let Ok((library, context)) = analysed else {
                    continue;
                };

                if TYPED.contains(&construct.name) {
                    assert!(!context.has_diagnostics(), "{label}");
                    let container = ironplc_codegen::compile(
                        &library,
                        &context,
                        &CodegenOptions::from(&options),
                        &EmptyLookup,
                    );
                    assert!(container.is_ok(), "{label}");
                }

                assert!(write_to_string(&parsed).is_ok(), "{label}");
                assert!(write_to_string(&library).is_ok(), "{label}");
            }
        }
    });
}

#[test]
fn compile_when_caller_has_a_small_stack_then_the_stages_still_run_on_the_budget() {
    // The budget is not something a caller has to bring: from a thread of 1
    // MiB, the smallest a caller has, the pipeline of the project runs a
    // nesting that needs several times that.
    const SMALLEST_STACK: usize = 1024 * 1024;
    let check = || {
        let mut project = MemoryBackedProject::new(CompilerOptions::default());
        let construct = NESTINGS
            .iter()
            .find(|construct| construct.name == "if statements");
        let (Some(construct), Some(production)) = (
            construct,
            FRONTENDS
                .iter()
                .find(|frontend| frontend.name == "production"),
        ) else {
            return false;
        };
        // Half the limit, or less where the front end limits the depth of its
        // tree and a statement is more than one level of it.
        let depth = depth_for(production, construct, MAX_DEPTH / 2);
        project.add_source(FileId::from_string("deep.st"), program(construct, depth));
        let output = compile(
            &mut project,
            &CompilerOptions::default(),
            &EmptyLookup,
            vec![],
        );
        output.diagnostics.is_empty() && output.container.is_some()
    };
    let handle = std::thread::Builder::new()
        .stack_size(SMALLEST_STACK)
        .spawn(check);
    assert!(handle.is_ok_and(|handle| handle.join().is_ok_and(|compiled| compiled)));
}
