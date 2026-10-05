//! The path table is the one place that lists what the parse benchmarks
//! measure, so a mistake in it must fail a test and not skew a report: a
//! baseline that names no path, two paths with one name, a path that panics.

use ironplc_benchmarks::corpus::{corpus_dir, load_corpus, statement_bodies};
use ironplc_benchmarks::paths::{path_named, selected_frontend, timed, Ctx, Over, Probe, PATHS};
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
fn paths_when_baseline_named_then_a_path_over_the_same_inputs_has_that_name() {
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
        assert_eq!(other.over, path.over, "{}", path.name);
        assert_ne!(other.name, path.name);
    }
}

#[test]
fn paths_when_run_on_a_tiny_input_then_one_call_is_bracketed_and_described() {
    let ctx = Ctx::default();
    for path in PATHS {
        for (_, source) in path.over.probes() {
            let mut counter = Counter::default();
            (path.run)(&ctx, source, &mut counter);
            assert_eq!((counter.starts, counter.stops), (1, 1), "{}", path.name);
            assert!(!(path.describe)(&ctx, source).is_empty(), "{}", path.name);
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
    for over in Over::ALL {
        assert!(!over.probes().is_empty(), "{}", over.label());
    }
}

#[test]
fn selected_frontend_when_asked_then_one_of_the_two_front_ends() {
    let selected = selected_frontend(&Ctx::default());

    assert!(selected == "legacy" || selected == "cst", "{selected}");
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
