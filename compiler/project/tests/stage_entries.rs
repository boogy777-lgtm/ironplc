//! The public entries of the stages, and the budget each of them gets.
//!
//! A caller that is not a process entry (a test, a benchmark, a program that
//! embeds the compiler) holds no budget, so the entry of a stage must give it
//! one: [`ENTRIES`] is every public entry of a stage that recurses as deep as
//! the tree, one row each, and the two tests below hold every row to the same
//! two facts. Off the budget, an entry makes exactly one thread, the budget it
//! runs on. On the budget, which is where a process entry puts the run, it
//! makes none.
//!
//! A stage is added to the pipeline by adding its entry here; an entry that
//! runs on the caller's stack makes no thread and fails the first test.

use ironplc_analyzer::stages::{analyze, resolve_types};
use ironplc_analyzer::SemanticContext;
use ironplc_codegen::{CodegenOptions, EmptyLookup};
use ironplc_dsl::common::Library;
use ironplc_dsl::core::FileId;
use ironplc_dsl::stack::{spawns_by_current_thread, within_stack_budget};
use ironplc_parser::options::CompilerOptions;
use ironplc_parser::{parse_program, parse_st_statements};
use ironplc_plc2plc::write_to_string;
use ironplc_syntax::{parse_expression, parse_source_file, parse_statements, ParseOptions};

const PROGRAM: &str = "PROGRAM main VAR x : INT; END_VAR x := 1; END_PROGRAM";

/// What the entries are called on: the program, read and analyzed once.
struct Input {
    options: CompilerOptions,
    library: Library,
    analyzed: (Library, SemanticContext),
}

type Entry = fn(&Input);

/// Every public entry of a stage, by name.
const ENTRIES: &[(&str, Entry)] = &[
    ("parse_program", |input| {
        let _ = parse_program(PROGRAM, &FileId::default(), &input.options);
    }),
    ("parse_st_statements", |input| {
        let _ = parse_st_statements("x := 1;", &FileId::default(), &input.options, 0, 0);
    }),
    ("syntax::parse_source_file", |_| {
        let _ = parse_source_file(PROGRAM, &ParseOptions::default());
    }),
    ("syntax::parse_statements", |_| {
        let _ = parse_statements("x := 1;", &ParseOptions::default());
    }),
    ("syntax::parse_expression", |_| {
        let _ = parse_expression("1 + 2", &ParseOptions::default());
    }),
    ("analyze", |input| {
        let _ = analyze(&[&input.library], &input.options);
    }),
    ("resolve_types", |input| {
        let _ = resolve_types(&[&input.library], &input.options);
    }),
    ("codegen::compile", |input| {
        let (library, context) = &input.analyzed;
        let _ = ironplc_codegen::compile(
            library,
            context,
            &CodegenOptions::from(&input.options),
            &EmptyLookup,
        );
    }),
    ("write_to_string", |input| {
        let _ = write_to_string(&input.library);
    }),
];

fn input() -> Input {
    let options = CompilerOptions::default();
    let library = parse_program(PROGRAM, &FileId::default(), &options).expect("the program parses");
    let analyzed = analyze(&[&library], &options).expect("the program analyzes");
    Input {
        options,
        library,
        analyzed,
    }
}

/// How many threads `entry` makes on behalf of the calling thread.
fn spawned_by(entry: Entry, input: &Input) -> usize {
    let before = spawns_by_current_thread();
    entry(input);
    spawns_by_current_thread() - before
}

#[test]
fn entries_when_caller_has_no_budget_then_each_makes_one_thread() {
    let input = input();
    let without: Vec<&str> = ENTRIES
        .iter()
        .filter(|(_, entry)| spawned_by(*entry, &input) != 1)
        .map(|(name, _)| *name)
        .collect();

    assert_eq!(without, Vec::<&str>::new());
}

#[test]
fn entries_when_caller_has_the_budget_then_none_makes_a_thread() {
    let spawning: Vec<&str> = within_stack_budget(|| {
        let input = input();
        ENTRIES
            .iter()
            .filter(|(_, entry)| spawned_by(*entry, &input) != 0)
            .map(|(name, _)| *name)
            .collect()
    });

    assert_eq!(spawning, Vec::<&str>::new());
}
