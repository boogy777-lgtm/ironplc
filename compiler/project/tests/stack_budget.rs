//! The stack budget of the compile pipeline: every stage that recurses as deep
//! as the tree it is given runs on a stack of `STACK_BUDGET`, and a tree as
//! deep as `MAX_DEPTH` fits it. The stages after the parse (analysis, code
//! generation and printing) are reached only through the front end, so the proof
//! is here, where they all are: the program is read by `ironplc_parser::
//! parse_program`, the one way the compiler reads text.
//!
//! The front end limits the depth of its *tree*, and a program around the
//! nesting adds levels of its own, so each construct is asked for the deepest
//! nesting the front end accepts, found by bisection, which is as deep as its
//! tree may be (`MAX_DEPTH` nodes). It parses the nestings of files too.
//!
//! The stages of the parser in `ironplc-syntax` before lowering are proved in
//! its own `tests/tree_depth.rs`.

use ironplc_analyzer::stages::analyze;
use ironplc_codegen::{CodegenOptions, EmptyLookup};
use ironplc_dsl::core::FileId;
use ironplc_dsl::stack::{within_stack_budget, MAX_DEPTH};
use ironplc_parser::options::CompilerOptions;
use ironplc_parser::parse_program;
use ironplc_plc2plc::write_to_string;
use ironplc_project::{compile, MemoryBackedProject};
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

/// The options the programs are read under: the defaults, with the one keyword
/// that the nesting of files needs.
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

/// True when the front end rejects `construct` nested `n` times as too deep
/// (P0019).
fn rejects_as_too_deep(construct: &Nesting, n: usize) -> bool {
    // The program around the nesting is the text that is parsed.
    let parsed = parse_program(&program(construct, n), &FileId::default(), &options());
    parsed.is_err_and(|diagnostic| diagnostic.code == "P0019")
}

/// How deep `construct` is nested for `limit`: the limit, or, where the limit
/// is more than the program around the nesting leaves the tree, the deepest
/// nesting the front end accepts.
fn depth_for(construct: &Nesting, limit: usize) -> usize {
    if !rejects_as_too_deep(construct, limit) {
        return limit;
    }
    construct.deepest_by(limit, |n| rejects_as_too_deep(construct, n))
}

#[test]
fn compile_when_nesting_is_as_deep_as_allowed_then_every_stage_fits_the_stack_budget() {
    // Parse, analysis, code generation and printing, over every nesting that
    // `MAX_DEPTH` allows, and the drop of everything they made, on a thread of
    // exactly the budget, in whichever profile the test is built.
    within_stack_budget(|| {
        let options = options();
        for construct in NESTINGS {
            let label = construct.name;
            let depth = depth_for(construct, MAX_DEPTH);
            println!("{label}: {depth} levels");
            // A level of a construct is at most four levels of the tree, so
            // the nesting reaches the depth the compiler is built for.
            assert!(depth * 4 >= MAX_DEPTH, "{label}: only {depth} levels");

            let parsed = parse_program(&program(construct, depth), &FileId::default(), &options);
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
        let Some(construct) = construct else {
            return false;
        };
        // Half the limit, or less where a statement is more than one level of
        // the tree.
        let depth = depth_for(construct, MAX_DEPTH / 2);
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
