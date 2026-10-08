//! The path table is the one place that lists what the parse benchmarks
//! measure, so a mistake in it must fail a test and not skew a report: a
//! baseline that names no path, two paths with one name, a path that panics.

use ironplc_benchmarks::corpus::{corpus_dir, load_corpus, plcopen_document, statement_bodies};
use ironplc_benchmarks::generated::SHAPES;
use ironplc_benchmarks::paths::{path_named, timed, Ctx, Input, Over, Probe, Stack, PATHS};
use std::collections::HashSet;

/// Counts the brackets a path makes.
#[derive(Default)]
struct Counter {
    starts: usize,
    stops: usize,
}

impl Probe for Counter {
    fn start(&mut self) {
        self.starts += 1;
    }
    fn stop(&mut self) {
        self.stops += 1;
    }
}

#[test]
fn paths_when_listed_then_names_and_groups_are_unique() {
    let names: HashSet<_> = PATHS.iter().map(|path| path.name).collect();
    let groups: HashSet<_> = PATHS.iter().map(|path| path.group).collect();
    assert_eq!(names.len(), PATHS.len());
    assert_eq!(groups.len(), PATHS.len());
}

#[test]
fn paths_when_baseline_named_then_a_path_over_at_least_the_same_inputs_has_that_name() {
    for path in PATHS {
        let Some(baseline) = path.baseline else {
            continue;
        };
        let other = path_named(baseline);
        assert!(
            other.is_some(),
            "{} names the missing {baseline}",
            path.name
        );
        let other = other.unwrap();
        for over in path.over {
            assert!(other.runs_over(*over), "{}", path.name);
        }
        assert_ne!(other.name, path.name);
    }
}

#[test]
fn paths_when_run_on_a_tiny_input_then_one_call_is_bracketed_and_described() {
    let ctx = Ctx::default();
    for path in PATHS {
        for (_, input) in path.over.iter().flat_map(|over| over.probes()) {
            let mut counter = Counter::default();
            path.call(&ctx, &input, &mut counter);
            assert_eq!((counter.starts, counter.stops), (1, 1), "{}", path.name);
            assert!(!(path.describe)(&ctx, &input).is_empty(), "{}", path.name);
        }
    }
}

#[test]
fn timed_when_called_then_the_call_runs_between_start_and_stop() {
    let mut counter = Counter::default();
    let mut ran = false;
    timed(&mut counter, || ran = true);
    assert!(ran);
    assert_eq!((counter.starts, counter.stops), (1, 1));
}

#[test]
fn probes_when_each_set_then_at_least_one_tiny_input() {
    for over in Over::all() {
        assert!(!over.probes().is_empty(), "{}", over.label());
    }
}

#[test]
fn paths_when_listed_then_every_set_of_inputs_a_path_names_exists() {
    let all = Over::all();
    for path in PATHS {
        assert!(!path.over.is_empty(), "{}", path.name);
        for over in path.over {
            assert!(all.contains(over), "{} names {}", path.name, over.label());
        }
    }
}

#[test]
fn paths_when_listed_then_every_shape_of_projects_has_a_path_over_it() {
    for shape in SHAPES {
        let over = Over::Generated(shape);
        assert!(
            PATHS.iter().any(|path| path.runs_over(over)),
            "no path runs over {}",
            shape.name
        );
    }
}

#[test]
fn statement_bodies_when_corpus_then_non_empty_and_named_after_their_file() {
    let files = load_corpus(&corpus_dir()).unwrap();
    let bodies = statement_bodies(&files);
    assert!(!bodies.is_empty());
    for body in &bodies {
        assert!(!body.source.is_empty());
        assert_eq!(body.source, body.source.trim());
        let file_name = body.name.rsplit_once('#').map(|(file, _)| file);
        assert!(files
            .iter()
            .any(|file| Some(file.name.as_str()) == file_name));
    }
}

/// Records whether the thread has the budget when the call starts.
#[derive(Default)]
struct OnBudget {
    at_start: Option<bool>,
}

impl Probe for OnBudget {
    fn start(&mut self) {
        self.at_start = Some(ironplc_dsl::stack::is_on_budget());
    }
    fn stop(&mut self) {}
}

#[test]
fn paths_when_stack_is_held_then_the_call_starts_on_the_budget_and_otherwise_does_not() {
    let ctx = Ctx::default();
    for path in PATHS {
        for (_, input) in path.over.iter().flat_map(|over| over.probes()) {
            let mut probe = OnBudget::default();
            path.call(&ctx, &input, &mut probe);
            assert_eq!(
                probe.at_start,
                Some(path.stack == Stack::Held),
                "{}",
                path.name
            );
        }
    }
}

#[test]
fn paths_when_held_then_a_path_over_the_same_inputs_measures_the_same_call_without_the_hold() {
    for path in PATHS.iter().filter(|path| path.stack == Stack::Held) {
        let baseline = path.baseline.and_then(path_named);
        assert!(
            baseline.is_some_and(|baseline| baseline.stack == Stack::Caller),
            "{}",
            path.name
        );
    }
}

#[test]
fn plcopen_document_when_corpus_then_the_document_is_read_with_a_unit_for_each_body() {
    let files = load_corpus(&corpus_dir()).unwrap();
    let document = plcopen_document(&statement_bodies(&files));
    let ctx = Ctx::default();
    let path = path_named("xml document").unwrap();

    let described = (path.describe)(&ctx, &Input::of(document.clone()));

    let units: usize = described
        .strip_prefix("ok, ")
        .and_then(|rest| rest.split(' ').next())
        .and_then(|count| count.parse().ok())
        .unwrap_or(0);
    assert!(units > 10, "{described}");
    assert!(
        document.name.contains(&units.to_string()),
        "{}",
        document.name
    );
}
