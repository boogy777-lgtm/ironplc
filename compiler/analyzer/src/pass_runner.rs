//! How a pass of the type resolution is run.
//!
//! Every transform of the library crosses one of the functions here, and each
//! reports itself to the observer ([`crate::observe`]) under the name of its
//! module. The two functions differ in one thing, what a failure keeps; the
//! macros take the name from the module a call names, so a pass is written once
//! and is reported under that name.

use ironplc_dsl::{common::Library, diagnostic::Diagnostic};

use crate::experiment::NO_FALLBACK;
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

/// The copy a pass keeps so that a failure can give the library back.
///
/// Prototype B1: with the switch on, no copy is kept and a failed pass gives
/// back an empty library. That is the cost of the contract "a failed pass
/// returns the library it was given" when the contract needs no copy; the
/// result is the same whenever no pass fails.
fn keep_copy<O: Observer>(observer: &O, name: &'static str, library: &Library) -> Library {
    if NO_FALLBACK.level() == 1 {
        return Library::new();
    }
    observer.observe(observe::fallback(name), || library.clone())
}

fn release_copy<O: Observer>(observer: &O, name: &'static str, copy: Library) {
    if NO_FALLBACK.level() == 1 {
        return;
    }
    observer.observe(observe::fallback(name), || drop(copy));
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
    let fallback = keep_copy(observer, name, &library);
    match observer.observe(observe::pass(name), || xform(library)) {
        Ok(result) => {
            release_copy(observer, name, fallback);
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
    let fallback = keep_copy(observer, name, &library);
    match observer.observe(observe::pass(name), || xform(library)) {
        Ok((result, errs)) => {
            diagnostics.extend(errs);
            release_copy(observer, name, fallback);
            result
        }
        Err(errs) => {
            diagnostics.extend(errs);
            fallback
        }
    }
}
