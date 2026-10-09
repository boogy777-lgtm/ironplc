//! No step of the analysis keeps a copy of the library.
//!
//! The guard reads the count of library elements cloned in the process
//! (`ironplc_dsl::common::library_element_clones`, built with the feature
//! `count-library-clones` of the dsl crate, which this crate enables for its
//! tests) around every step the
//! analysis tells its observer about: a pass, a rule, or the making of an input.
//! A copy of a library, however a step makes it (`library.clone()`, a clone of
//! its elements, a clone of each element), is a clone of its elements, so a
//! pass that is added later, or a change to one that exists, is held to this
//! without an edit here. The only clones are the merge of the inputs, which
//! clones each input once.
//!
//! The count is global to the process, so this file holds one test: a second
//! test running at the same time would clone elements of its own.

#![allow(
    clippy::unwrap_used,
    reason = "test target: panicking helpers are sanctioned in tests"
)]

use ironplc_analyzer::observe::{Observer, Step};
use ironplc_analyzer::stages::analyze_observed;
use ironplc_dsl::common::{library_element_clones, Library};
use ironplc_dsl::core::FileId;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_parser::parse_program;
use ironplc_test::read_shared_resource;
use std::sync::Mutex;

/// Records, for each step, how many library elements it cloned.
#[derive(Default)]
struct CloneCounter {
    steps: Mutex<Vec<(Step, usize)>>,
}

impl Observer for CloneCounter {
    fn observe<R>(&self, step: Step, run: impl FnOnce() -> R) -> R {
        let before = library_element_clones();
        let result = run();
        let cloned = library_element_clones() - before;
        self.steps.lock().unwrap().push((step, cloned));
        result
    }
}

fn options() -> CompilerOptions {
    CompilerOptions {
        allow_fb_inheritance: true,
        allow_top_level_var_global: true,
        ..CompilerOptions::from_dialect(Dialect::Rusty)
    }
}

/// Programs with no problem, and one with a problem for each pass that reports
/// one.
fn programs() -> Vec<String> {
    let mut programs = vec![
        read_shared_resource("first_steps.st"),
        read_shared_resource("main.st"),
        read_shared_resource("first_steps_semantic_error.st"),
    ];
    programs.extend(
        [
            "TYPE T_BAD : T_NOWHERE; END_TYPE",
            "TYPE T_BAD : ARRAY[3..1] OF INT; END_TYPE",
            "FUNCTION_BLOCK FB VAR count : INT; END_VAR METHOD Run THIS^.count := 1; END_METHOD END_FUNCTION_BLOCK",
            "PROGRAM p VAR z : INT; END_VAR z := 1 / 0; END_PROGRAM",
        ]
        .map(String::from),
    );
    programs
}

/// The elements cloned by each step of the analysis of `text`, and the number of
/// elements the analysis was given.
fn clones_of(text: &str) -> (Vec<(Step, usize)>, usize) {
    let options = options();
    let library = parse_program(text, &FileId::default(), &options).unwrap();
    let given = library.elements.len();
    let counter = CloneCounter::default();
    let references: Vec<&Library> = vec![&library];
    analyze_observed(&references, &options, &counter).unwrap();
    (counter.steps.into_inner().unwrap(), given)
}

#[test]
fn analyze_when_any_program_then_only_the_merge_of_the_inputs_clones_elements() {
    let outcomes: Vec<(Vec<(Step, usize)>, usize)> =
        programs().iter().map(|text| clones_of(text)).collect();

    let others: Vec<&str> = outcomes
        .iter()
        .flat_map(|(steps, _)| steps)
        .filter(|(step, cloned)| step.name != "merge sources" && *cloned != 0)
        .map(|(step, _)| step.name)
        .collect();
    assert!(others.is_empty(), "steps that copied: {others:?}");

    let merge_is_one_copy_of_the_inputs = outcomes.iter().all(|(steps, given)| {
        steps
            .iter()
            .filter(|(step, _)| step.name == "merge sources")
            .map(|(_, cloned)| *cloned)
            .sum::<usize>()
            == *given
    });
    assert!(merge_is_one_copy_of_the_inputs);
}
