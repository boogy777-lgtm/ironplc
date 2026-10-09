//! The points where the analysis reports what it is doing.
//!
//! Every step of the analysis crosses one of two points: a pass (a transform of
//! the library, and each of the other steps that make the library or an
//! environment) crosses [`Observer::observe`] through the functions that run
//! it in `stages`, and a semantic rule crosses the same method through the one
//! loop that runs the rules in `semantic_rules`. A caller that wants to know
//! what a step cost (a benchmark, a profile) hands [`stages::analyze_observed`]
//! an observer; [`stages::analyze`] hands it [`Unobserved`].
//!
//! The observer is a type parameter, not a trait object, so that the call of
//! a caller that observes nothing is the step itself: [`Unobserved`] does
//! nothing, the call is inlined away, and the analysis does the same work and
//! gives the same result with and without an observer.
//!
//! A step is named by the identifier that runs it (`xform_resolve_adr`,
//! `rule_jump_target`): the macros below take the name from the tokens of the
//! call, so that a pass or a rule that is added is reported under its own name
//! with no line written for the observer.
//!
//! [`stages::analyze_observed`]: crate::stages::analyze_observed
//! [`stages::analyze`]: crate::stages::analyze

/// What a step is, so that a report can group steps without reading names.
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub enum Kind {
    /// A step that makes an input of the passes: the merged library, the
    /// standard environments.
    Setup,
    /// A transform of the library, or another analysis over it.
    Pass,
    /// A semantic rule.
    Rule,
}

/// One step of the analysis: what it is and what it is called.
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct Step {
    pub kind: Kind,
    pub name: &'static str,
}

/// Told about every step of the analysis, around the call that does it.
///
/// A step may run on any thread (the analysis runs on the stack budget), so an
/// observer is `Sync`.
pub trait Observer: Sync {
    /// Runs `run` and returns what it returns. An observer measures or counts
    /// around the call; it must not change what the call does or returns.
    #[inline(always)]
    fn observe<R>(&self, _step: Step, run: impl FnOnce() -> R) -> R {
        run()
    }
}

/// The observer of a caller that observes nothing.
#[derive(Clone, Copy, Debug, Default)]
pub struct Unobserved;

impl Observer for Unobserved {}

/// The step that makes an input of the passes. The name says what is made.
pub(crate) const fn setup(name: &'static str) -> Step {
    Step {
        kind: Kind::Setup,
        name,
    }
}

/// The step of a pass, named `name`.
pub(crate) const fn pass(name: &'static str) -> Step {
    Step {
        kind: Kind::Pass,
        name,
    }
}

/// The step of a semantic rule, named `name`.
pub(crate) const fn rule(name: &'static str) -> Step {
    Step {
        kind: Kind::Rule,
        name,
    }
}

/// An observer that keeps every step it is told about, in order.
#[cfg(test)]
#[derive(Default)]
pub(crate) struct Recorder {
    steps: std::sync::Mutex<Vec<Step>>,
}

#[cfg(test)]
impl Recorder {
    /// The names of the steps of `kind` told so far, in order, with repeats.
    pub(crate) fn names(&self, kind: Kind) -> Vec<&'static str> {
        self.steps
            .lock()
            .map(|steps| {
                steps
                    .iter()
                    .filter(|step| step.kind == kind)
                    .map(|step| step.name)
                    .collect()
            })
            .unwrap_or_default()
    }
}

#[cfg(test)]
impl Observer for Recorder {
    fn observe<R>(&self, step: Step, run: impl FnOnce() -> R) -> R {
        if let Ok(mut steps) = self.steps.lock() {
            steps.push(step);
        }
        run()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::stages::{analyze, analyze_observed};
    use ironplc_dsl::core::FileId;
    use ironplc_parser::options::CompilerOptions;
    use ironplc_parser::parse_program;
    use ironplc_test::read_shared_resource;
    use std::collections::BTreeSet;
    use std::path::Path;

    /// The names of the files `src/<prefix>*.rs` of this crate, without the
    /// extension, except `excluded`.
    fn modules_named(prefix: &str, excluded: &[&str]) -> BTreeSet<String> {
        let source = Path::new(env!("CARGO_MANIFEST_DIR")).join("src");
        std::fs::read_dir(source)
            .unwrap()
            .filter_map(|entry| entry.ok())
            .filter_map(|entry| entry.file_name().into_string().ok())
            .filter_map(|file| file.strip_suffix(".rs").map(str::to_string))
            .filter(|name| name.starts_with(prefix) && !excluded.contains(&name.as_str()))
            .collect()
    }

    fn observed_analysis(options: &CompilerOptions) -> Recorder {
        let source = read_shared_resource("first_steps.st");
        let library = parse_program(&source, &FileId::default(), options).unwrap();
        let recorder = Recorder::default();
        analyze_observed(&[&library], options, &recorder).unwrap();
        recorder
    }

    #[test]
    fn observe_when_unobserved_then_returns_what_the_call_returns() {
        let value = Unobserved.observe(pass("p"), || 7);

        assert_eq!(7, value);
    }

    #[test]
    fn observe_when_recorder_then_step_is_told_and_call_runs_once() {
        let recorder = Recorder::default();
        let mut runs = 0;

        let value = recorder.observe(rule("r"), || {
            runs += 1;
            "done"
        });

        assert_eq!("done", value);
        assert_eq!(1, runs);
        assert_eq!(vec!["r"], recorder.names(Kind::Rule));
    }

    #[test]
    fn analyze_observed_when_analyzing_then_every_rule_module_is_reported() {
        let recorder = observed_analysis(&CompilerOptions::default());

        let reported: BTreeSet<String> = recorder
            .names(Kind::Rule)
            .into_iter()
            .map(str::to_string)
            .collect();

        assert_eq!(modules_named("rule_", &["rule_support"]), reported);
    }

    #[test]
    fn analyze_observed_when_analyzing_then_every_rule_is_reported_once() {
        let recorder = observed_analysis(&CompilerOptions::default());

        let names = recorder.names(Kind::Rule);
        let distinct: BTreeSet<&str> = names.iter().copied().collect();

        assert_eq!(distinct.len(), names.len());
    }

    #[test]
    fn analyze_observed_when_analyzing_then_every_xform_module_is_reported_as_a_pass() {
        let recorder = observed_analysis(&CompilerOptions::default());

        let reported: BTreeSet<String> = recorder
            .names(Kind::Pass)
            .into_iter()
            .map(str::to_string)
            .collect();

        assert_eq!(modules_named("xform_", &[]), reported_xforms(&reported));
    }

    /// The passes of the report that are transforms of the library.
    fn reported_xforms(reported: &BTreeSet<String>) -> BTreeSet<String> {
        reported
            .iter()
            .filter(|name| name.starts_with("xform_"))
            .cloned()
            .collect()
    }

    #[test]
    fn analyze_observed_when_analyzing_then_result_is_the_one_analyze_gives() {
        let options = CompilerOptions::default();
        let source = read_shared_resource("first_steps.st");
        let library = parse_program(&source, &FileId::default(), &options).unwrap();

        let (plain_library, plain_context) = analyze(&[&library], &options).unwrap();
        let (seen_library, seen_context) =
            analyze_observed(&[&library], &options, &Recorder::default()).unwrap();

        assert_eq!(plain_library, seen_library);
        assert_eq!(
            plain_context.diagnostics().len(),
            seen_context.diagnostics().len()
        );
    }
}
