//! How a pass of the type resolution is run.
//!
//! Every transform of the library crosses one of the functions here, and each
//! reports itself to the observer ([`crate::observe`]) under the name of its
//! module. The two functions differ in one thing, what a failure keeps; the
//! macros take the name from the module a call names, so a pass is written once
//! and is reported under that name.

use ironplc_dsl::{common::Library, diagnostic::Diagnostic};

use crate::observe::{self, Observer};

/// Runs the transform of the module named, `<module>::apply(library, ...)`,
/// as the pass named after that module: `how` is the function that decides
/// what a failure keeps ([`run_best_effort`] or [`run_reverting_on_error`]),
/// and the arguments after the module are the arguments of `apply` that follow
/// the library. The name an observer is told is the identifier of the module,
/// taken from the call, so a pass added here is reported under its own name.
macro_rules! pass {
    ($how:ident, $observer:expr, $library:expr, $diagnostics:expr, $module:ident $(, $argument:expr)*) => {
        $how($observer, stringify!($module), $library, $diagnostics, |library| {
            $module::apply(library $(, $argument)*)
        })
    };
}

/// Runs `<module>::apply(...)` as the pass named after the module, for a pass
/// that has no fallback to keep: one whose failure is a hard failure, one that
/// cannot fail, or one that does not transform the library.
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

/// Runs a transform whose failure discards the whole library.
///
/// The pre-pass library is restored when the transform returns `Err`, so every
/// transformation it had already completed is thrown away with the one that
/// failed. Reserve this for a pass whose output is meaningless when any part
/// of it failed, and prefer [`run_best_effort`].
///
/// The reason the distinction exists is not one failing test. Corpus testing
/// showed what looked like merge-order or file-pairing sensitivity: a source
/// that analyzed cleanly alone failed once merged with unrelated code. The
/// cause was never ordering -- a transform that accumulates diagnostics and
/// then discards its whole result throws away every unrelated resolution it
/// had already completed. A pass that returns `Err` after a user-level
/// diagnostic reintroduces that, so a new pass reports per-declaration
/// problems through `run_best_effort` instead.
pub(crate) fn run_reverting_on_error<O: Observer>(
    observer: &O,
    name: &'static str,
    library: Library,
    diagnostics: &mut Vec<Diagnostic>,
    xform: impl FnOnce(Library) -> Result<Library, Vec<Diagnostic>>,
) -> Library {
    let fallback = observer.observe(observe::fallback(name), || library.clone());
    match observer.observe(observe::pass(name), || xform(library)) {
        Ok(result) => {
            observer.observe(observe::fallback(name), || drop(fallback));
            result
        }
        Err(errs) => {
            diagnostics.extend(errs);
            fallback
        }
    }
}

/// Runs a transform that reports per-declaration problems without discarding
/// the declarations it did transform.
///
/// `Ok((library, diagnostics))` means "here is the transformed library, and
/// here is what was wrong with parts of it": the transformed library is kept
/// and the diagnostics are collected alongside it. A best-effort pass reserves
/// `Err` for a failure that left it with no library to return at all, which
/// still reverts because there is nothing else to keep.
pub(crate) fn run_best_effort<O: Observer>(
    observer: &O,
    name: &'static str,
    library: Library,
    diagnostics: &mut Vec<Diagnostic>,
    xform: impl FnOnce(Library) -> Result<(Library, Vec<Diagnostic>), Vec<Diagnostic>>,
) -> Library {
    let fallback = observer.observe(observe::fallback(name), || library.clone());
    match observer.observe(observe::pass(name), || xform(library)) {
        Ok((result, errs)) => {
            diagnostics.extend(errs);
            observer.observe(observe::fallback(name), || drop(fallback));
            result
        }
        Err(errs) => {
            diagnostics.extend(errs);
            fallback
        }
    }
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
        assert!(recorder.names(Kind::Fallback).is_empty());
    }
}
