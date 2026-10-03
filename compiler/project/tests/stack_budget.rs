//! The stack budget of the compile pipeline: every stage that recurses as deep
//! as the tree it is given runs on a stack of `STACK_BUDGET`, and a tree as
//! deep as `MAX_DEPTH` fits it. The legacy parser is the one production
//! consumers run, and it has no limit of its own; its stages, analysis, code
//! generation and printing, are reached only through it, so the proof is here,
//! where they all are. The stages of the parser in `ironplc-syntax` are proved
//! in its own `tests/tree_depth.rs`.

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

/// A program that holds `construct` nested `n` times.
fn program(construct: &Nesting, n: usize) -> String {
    let body = (construct.build)(n);
    let head = "FUNCTION f : INT VAR_INPUT v : INT; END_VAR f := v; END_FUNCTION\n\
                PROGRAM main VAR a : INT; c : BOOL; x : INT; END_VAR\n";
    match construct.entry {
        Entry::Expression => format!("{head}x := {body};\nEND_PROGRAM"),
        Entry::Statements | Entry::File => format!("{head}{body}\nEND_PROGRAM"),
    }
}

/// The constructs the legacy grammar has: it has no nested namespaces.
fn legacy_constructs() -> impl Iterator<Item = &'static Nesting> {
    NESTINGS
        .iter()
        .filter(|construct| construct.entry != Entry::File)
}

#[test]
fn compile_when_nesting_is_as_deep_as_allowed_then_every_legacy_stage_fits_the_stack_budget() {
    // Parse, analysis, code generation and printing, over every nesting that
    // `MAX_DEPTH` allows, and the drop of everything they made, on a thread of
    // exactly the budget, in whichever profile the test is built. The inputs
    // nest `MAX_DEPTH` times, which for the legacy parser is the deepest it is
    // asked to go.
    within_stack_budget(|| {
        let options = CompilerOptions::default();
        for construct in legacy_constructs() {
            let source = program(construct, MAX_DEPTH);
            let parsed = parse_program(&source, &FileId::default(), &options);
            assert!(parsed.is_ok(), "{}", construct.name);
            let Ok(parsed) = parsed else { continue };

            let analysed = analyze(&[&parsed], &options);
            assert!(analysed.is_ok(), "{}", construct.name);
            let Ok((library, context)) = analysed else {
                continue;
            };

            if TYPED.contains(&construct.name) {
                assert!(!context.has_diagnostics(), "{}", construct.name);
                let container = ironplc_codegen::compile(
                    &library,
                    &context,
                    &CodegenOptions::from(&options),
                    &EmptyLookup,
                );
                assert!(container.is_ok(), "{}", construct.name);
            }

            assert!(write_to_string(&parsed).is_ok(), "{}", construct.name);
            assert!(write_to_string(&library).is_ok(), "{}", construct.name);
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
        project.add_source(
            FileId::from_string("deep.st"),
            program(construct, MAX_DEPTH / 2),
        );
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
