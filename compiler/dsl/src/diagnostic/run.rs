//! What a run keeps of the messages it produces, and the lines that close it.
//!
//! CODESYS stores the messages of a build in the order they are produced,
//! drops a message that repeats an earlier one, keeps at most a fixed number of
//! errors and of warnings, and ends a build or a compile with a line that gives
//! the counts. The rules are written once, here:
//!
//! - [`Area::keep`] is the one place that decides which messages of a run stay:
//!   the run names its area, and the area's rule is applied where the messages
//!   of the run are collected (the compile pipeline, the summary of a command,
//!   the analysis of the language server), never by the consumer that shows
//!   them. [`kept`] is the rule of the Build area and [`kept_for_precompile`]
//!   the rule of the Precompile area.
//! - [`counts`] is the one place that counts the messages of a run.
//! - [`Summary`] is how a command frames its messages: each command declares its
//!   lines in one constant.
//!
//! A new kind of counted message is one entry of `KINDS`; a new command is one
//! constant of type [`Summary`].

use std::collections::HashSet;

use ironplc_problems::{codeless, CodelessText};

use super::{stop_at_fatal, Area, Diagnostic, MessageSettings, Severity};

/// A kind of message a run counts and limits, with the limit and the line that
/// is kept when the limit is passed. The limit and the line are declared
/// together so that neither can change without the other.
struct Kind {
    /// Whether a message of the category is of this kind.
    counts: fn(Severity) -> bool,
    /// How many messages of the kind a run keeps.
    limit: usize,
    /// The line kept at the first message over the limit, made from the limit.
    line: fn(usize) -> CodelessText,
    /// Whether the related positions of the messages that follow are dropped
    /// once the limit is passed.
    drops_related_positions: bool,
}

/// The errors of a run, `FatalError` and `Error`: the messages that stop the
/// build. CODESYS keeps 500 (`-.352.cs:206-215`), and drops the related
/// positions after them (`-.352.cs:165`).
const ERRORS: Kind = Kind {
    counts: |severity| severity.stops_build(),
    limit: 500,
    line: |limit| codeless::error_limit_exceeded(limit),
    drops_related_positions: true,
};

/// The warnings of a run. CODESYS keeps 100 by default; IronPLC keeps 500, the
/// same as for errors, by the owner's decision.
const WARNINGS: Kind = Kind {
    counts: |severity| severity == Severity::Warning,
    limit: 500,
    line: |limit| codeless::warning_limit_exceeded(limit),
    drops_related_positions: false,
};

const KINDS: [Kind; 2] = [ERRORS, WARNINGS];

/// What identifies a message, to decide that it repeats an earlier one: its code,
/// category, place and text, as CODESYS compares id, severity, position, text
/// and object.
type Identity = (
    String,
    Option<&'static str>,
    Severity,
    String,
    usize,
    usize,
    String,
);

fn identity(diagnostic: &Diagnostic) -> Identity {
    (
        diagnostic.code.clone(),
        diagnostic.codeless_key(),
        diagnostic.severity(),
        diagnostic.primary.file_id.to_string(),
        diagnostic.primary.location.start,
        diagnostic.primary.location.end,
        format!(
            "{}\n{}",
            diagnostic.description(),
            diagnostic.primary.message
        ),
    )
}

/// The keys of the limit lines, which a run neither counts nor drops.
fn limit_keys() -> [&'static str; KINDS.len()] {
    KINDS.map(|kind| (kind.line)(kind.limit).key())
}

/// The messages of a run that stay.
///
/// - A fatal error ends the run: only the fatal errors stay ([`stop_at_fatal`]).
/// - The order the messages were produced in is kept; nothing is sorted.
/// - A message that repeats an earlier one (same code, category, place and text)
///   is dropped, and the first stays.
/// - At most the limit of each kind stays. At the first message over the limit
///   the line of the kind is kept in its place, and the messages of the kind
///   after it are dropped.
/// - Once the error limit is passed, the related positions (the secondary labels)
///   of the messages that follow are dropped.
///
/// Applying it again to its own result changes nothing, so the layers of a
/// pipeline can each apply it: a limit line is not counted and is not repeated.
pub(super) fn kept(diagnostics: Vec<Diagnostic>) -> Vec<Diagnostic> {
    let limit_keys = limit_keys();
    let mut seen: HashSet<Identity> = HashSet::new();
    let mut counted = [0usize; KINDS.len()];
    let mut stays = Vec::new();

    for mut diagnostic in stop_at_fatal(diagnostics) {
        if !seen.insert(identity(&diagnostic)) {
            continue;
        }
        if diagnostic
            .codeless_key()
            .is_some_and(|key| limit_keys.contains(&key))
        {
            stays.push(diagnostic);
            continue;
        }

        let mut within = true;
        for (kind, counted) in KINDS.iter().zip(counted.iter_mut()) {
            if !(kind.counts)(diagnostic.severity()) {
                continue;
            }
            *counted += 1;
            if *counted == kind.limit + 1 {
                let line = Diagnostic::codeless((kind.line)(kind.limit));
                if seen.insert(identity(&line)) {
                    stays.push(line);
                }
            }
            within &= *counted <= kind.limit;
        }
        if !within {
            continue;
        }
        let related_positions_stay = KINDS
            .iter()
            .zip(counted.iter())
            .all(|(kind, counted)| !(kind.drops_related_positions && *counted > kind.limit));
        if !related_positions_stay {
            diagnostic.secondary.clear();
        }
        stays.push(diagnostic);
    }
    stays
}

/// The messages of a run that the Precompile area keeps: those shown in it that
/// are not suppressed, in the order they were produced, without repeats.
///
/// - A message without a position is not shown in the area
///   ([`Diagnostic::shown_in`], MSG-022), and a suppressed message is not added
///   to it (`PreCompileContext.cs:2446`).
/// - A message that repeats an earlier one (same code, category, place and text)
///   is dropped, and the first stays, as in the Build area.
/// - There are no limit lines and a fatal error does not end the run: the
///   precompile of CODESYS has neither (`PreCompileContext.cs:2440-2525`). Its
///   cap of 950 is on what the area stores, see [`Area::store`].
///
/// Applying it again to its own result changes nothing.
pub(super) fn kept_for_precompile(diagnostics: Vec<Diagnostic>) -> Vec<Diagnostic> {
    let mut seen: HashSet<Identity> = HashSet::new();
    diagnostics
        .into_iter()
        .filter(|diagnostic| diagnostic.shown_in(Area::Precompile))
        .filter(|diagnostic| diagnostic.severity().is_shown())
        .filter(|diagnostic| seen.insert(identity(diagnostic)))
        .collect()
}

/// The counts a summary line reports.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Counts {
    /// The messages that stop the build: `FatalError` and `Error`.
    pub errors: usize,
    /// The messages of category `Warning`; a suppressed warning is not one.
    pub warnings: usize,
}

/// The counts of the messages of a run. This is the one counting function: every
/// summary line takes its numbers from here.
///
/// The counts include the limit lines: a run that passes the error limit reports
/// 501 errors, the 500 it kept and the line "More than 500 errors occured". This
/// is the owner's decision, as CODESYS counts what it stores, and it stores the
/// limit line as an `Error` or a `Warning`.
pub fn counts(diagnostics: &[Diagnostic]) -> Counts {
    let [errors, warnings] = KINDS.map(|kind| {
        diagnostics
            .iter()
            .filter(|d| (kind.counts)(d.severity()))
            .count()
    });
    Counts { errors, warnings }
}

/// The lines a command puts around the messages of its run, as CODESYS puts them
/// in its Messages view. Each is a message of category `Text` without a code.
pub struct Summary {
    /// The area the command writes: the one whose rule decides what the run
    /// keeps.
    area: Area,
    /// The line that opens the run, made from the name of the application, or
    /// none.
    started: Option<fn(&str) -> CodelessText>,
    /// The line that closes a run without an error, made from the counts.
    complete_ok: fn(Counts) -> CodelessText,
    /// The line that closes a run with an error.
    complete_errors: fn(Counts) -> CodelessText,
}

/// `check`, which is a compile without code generation: it ends with the line
/// "Compile complete" (`LanguageModelManagerConsolidated.cs:1972-1989`).
pub const CHECK_SUMMARY: Summary = Summary {
    area: Area::Build,
    started: None,
    complete_ok: |c| codeless::compile_complete_ok(c.errors, c.warnings),
    complete_errors: |c| codeless::compile_complete_errors(c.errors, c.warnings),
};

/// `compile`, which is a build: it starts with "Build started" and ends with
/// "Build complete", "Ready for download" or, when an error stops the build,
/// "No download possible" (`CompilerPhaseControllerGenerateCode.cs:656-686`).
pub const COMPILE_SUMMARY: Summary = Summary {
    area: Area::Build,
    started: Some(|application| codeless::build_started(application)),
    complete_ok: |c| codeless::build_complete_ok(c.errors, c.warnings),
    complete_errors: |c| codeless::build_complete_errors(c.errors, c.warnings),
};

impl Summary {
    /// The line that opens the run, if the command has one.
    pub fn opening(&self, application: &str) -> Option<Diagnostic> {
        self.started
            .map(|started| Diagnostic::codeless(started(application)))
    }

    /// The line that closes a run, with the counts of the messages it kept. It
    /// says the run has an error when one stops the build.
    pub fn closing(&self, kept: &[Diagnostic]) -> Diagnostic {
        let counts = counts(kept);
        let complete = if counts.errors > 0 {
            self.complete_errors
        } else {
            self.complete_ok
        };
        Diagnostic::codeless(complete(counts))
    }

    /// The messages of a run as the command shows them: what the run keeps
    /// ([`Area::keep`] of the area of the command, with the `settings` of the
    /// run), with the opening line before and the closing line after. The
    /// closing line has the counts of what the run kept, so it shows the state
    /// of the pipeline as it ends.
    pub fn close(
        &self,
        application: &str,
        settings: &MessageSettings,
        diagnostics: Vec<Diagnostic>,
    ) -> Vec<Diagnostic> {
        let kept = self.area.keep(settings, diagnostics);
        let closing = self.closing(&kept);
        let mut closed: Vec<Diagnostic> = self.opening(application).into_iter().collect();
        closed.extend(kept);
        closed.push(closing);
        closed
    }
}

#[cfg(test)]
mod tests;
