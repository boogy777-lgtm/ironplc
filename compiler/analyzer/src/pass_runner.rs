//! How a pass of the type resolution is run.
//!
//! Every transform of the library crosses [`run_pass`], and each reports itself
//! to the observer ([`crate::observe`]) under the name of its module. The
//! contract is one for every pass: a pass takes the library and gives back an
//! [`Outcome`], the library as far as it could transform it and what it found
//! wrong with the parts it could not. A problem in the user's program is not a
//! failure of the pass, so a pass that finds one keeps the node it was about to
//! transform as it was, and the runner has nothing to give back and keeps no
//! copy of the library. The macros take the name from the module a call names,
//! so a pass is written once and is reported under that name.

use ironplc_dsl::{common::Library, diagnostic::Diagnostic};

use crate::observe::{self, Observer};

/// Runs the transform of the module named, `<module>::apply(library, ...)`,
/// as the pass named after that module: the arguments after the module are the
/// arguments of `apply` that follow the library. The name an observer is told
/// is the identifier of the module, taken from the call, so a pass added here
/// is reported under its own name.
macro_rules! pass {
    ($observer:expr, $library:expr, $diagnostics:expr, $module:ident $(, $argument:expr)*) => {
        $crate::pass_runner::run_pass(
            $observer,
            stringify!($module),
            $library,
            $diagnostics,
            |library| $module::apply(library $(, $argument)*),
        )
    };
}

/// Runs `<module>::apply(...)` as the pass named after the module, for a step
/// that is not a transform with the contract of [`run_pass`]: one whose failure
/// is a hard failure (the library cannot be analyzed at all), one that cannot
/// fail, or one that does not transform the library.
macro_rules! direct {
    ($observer:expr, $module:ident ( $($argument:expr),* $(,)? )) => {
        $observer.observe($crate::observe::pass(stringify!($module)), || {
            $module::apply($($argument),*)
        })
    };
}

pub(crate) use direct;
pub(crate) use pass;

/// What every pass gives back: the library as far as the pass could transform
/// it, and what the pass found wrong with the parts it could not.
///
/// A problem in the user's program is never a failure of the pass. The place
/// that finds it holds the node it was about to transform, so it records the
/// diagnostic and keeps the node as it was; everything else is transformed.
/// There is nothing to give back from a copy, so no pass keeps one.
pub(crate) struct Outcome {
    pub library: Library,
    pub diagnostics: Vec<Diagnostic>,
}

impl Outcome {
    pub(crate) fn new(library: Library, diagnostics: Vec<Diagnostic>) -> Self {
        Self {
            library,
            diagnostics,
        }
    }
}

/// Runs a pass: hands it the library, keeps the library it returns and adds
/// what it found to `diagnostics`.
pub(crate) fn run_pass<O: Observer>(
    observer: &O,
    name: &'static str,
    library: Library,
    diagnostics: &mut Vec<Diagnostic>,
    xform: impl FnOnce(Library) -> Outcome,
) -> Library {
    let outcome = observer.observe(observe::pass(name), || xform(library));
    diagnostics.extend(outcome.diagnostics);
    outcome.library
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::observe::{Kind, Recorder, Unobserved};
    use ironplc_dsl::core::FileId;
    use ironplc_parser::{options::CompilerOptions, parse_program};
    use ironplc_problems::Problem;

    fn library_of(text: &str) -> Library {
        parse_program(text, &FileId::default(), &CompilerOptions::default()).unwrap()
    }

    fn a_diagnostic() -> Diagnostic {
        Diagnostic::problem(
            Problem::NoContent,
            ironplc_dsl::diagnostic::Label::span(ironplc_dsl::core::SourceSpan::default(), "x"),
        )
    }

    #[test]
    fn run_pass_when_pass_returns_library_and_findings_then_library_is_kept_and_findings_added() {
        let library = library_of("PROGRAM main VAR x : INT; END_VAR x := 1; END_PROGRAM");
        let expected = library.clone();
        let mut diagnostics = vec![a_diagnostic()];

        let result = run_pass(&Unobserved, "p", library, &mut diagnostics, |library| {
            Outcome::new(library, vec![a_diagnostic()])
        });

        assert_eq!(expected, result);
        assert_eq!(2, diagnostics.len());
    }

    #[test]
    fn run_pass_when_observed_then_the_pass_is_told_once_and_nothing_else() {
        let recorder = Recorder::default();
        let mut diagnostics = vec![];

        run_pass(
            &recorder,
            "p",
            Library::new(),
            &mut diagnostics,
            |library| Outcome::new(library, vec![]),
        );

        assert_eq!(vec!["p"], recorder.names(Kind::Pass));
        assert!(recorder.names(Kind::Setup).is_empty());
    }
}
