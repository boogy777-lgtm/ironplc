//! What a run keeps of the messages it produces, and the lines that close it.
//!
//! CODESYS stores the messages of a build in the order they are produced,
//! drops a message that repeats an earlier one, keeps at most a fixed number of
//! errors and of warnings, and ends a build or a compile with a line that gives
//! the counts. The rules are written once, here:
//!
//! - [`kept`] is the one place that decides which messages of a run stay. It is
//!   applied where the messages of a run are collected (the analysis of a
//!   project, the compile pipeline and the summary of a command), never by the
//!   consumer that shows them.
//! - [`counts`] is the one place that counts the messages of a run.
//! - [`Summary`] is how a command frames its messages: each command declares its
//!   lines in one constant.
//!
//! A new kind of counted message is one entry of `KINDS`; a new command is one
//! constant of type [`Summary`].

use std::collections::HashSet;

use ironplc_problems::{codeless, CodelessText};

use super::{stop_at_fatal, Diagnostic, Severity};

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

/// Whether the counts of a summary include the limit lines. CODESYS reads the
/// counts of its summary from the stored messages, and it stores the limit line
/// as an `Error` or a `Warning`, so the code says they are included (a run that
/// passes the error limit reports 501 errors). Whether a running CODESYS agrees
/// is the open item MSG-075; this is the one place that changes when it answers.
const COUNTS_INCLUDE_LIMIT_LINES: bool = true;

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
pub fn kept(diagnostics: Vec<Diagnostic>) -> Vec<Diagnostic> {
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
pub fn counts(diagnostics: &[Diagnostic]) -> Counts {
    let limit_keys = limit_keys();
    let [errors, warnings] = KINDS.map(|kind| {
        diagnostics
            .iter()
            .filter(|d| (kind.counts)(d.severity()))
            .filter(|d| {
                COUNTS_INCLUDE_LIMIT_LINES
                    || !d
                        .codeless_key()
                        .is_some_and(|key| limit_keys.contains(&key))
            })
            .count()
    });
    Counts { errors, warnings }
}

/// The lines a command puts around the messages of its run, as CODESYS puts them
/// in its Messages view. Each is a message of category `Text` without a code.
pub struct Summary {
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
    started: None,
    complete_ok: |c| codeless::compile_complete_ok(c.errors, c.warnings),
    complete_errors: |c| codeless::compile_complete_errors(c.errors, c.warnings),
};

/// `compile`, which is a build: it starts with "Build started" and ends with
/// "Build complete", "Ready for download" or, when an error stops the build,
/// "No download possible" (`CompilerPhaseControllerGenerateCode.cs:656-686`).
pub const COMPILE_SUMMARY: Summary = Summary {
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
    /// ([`kept`]), with the opening line before and the closing line after. The
    /// closing line has the counts of what the run kept, so it shows the state
    /// of the pipeline as it ends.
    pub fn close(&self, application: &str, diagnostics: Vec<Diagnostic>) -> Vec<Diagnostic> {
        let kept = kept(diagnostics);
        let closing = self.closing(&kept);
        let mut closed: Vec<Diagnostic> = self.opening(application).into_iter().collect();
        closed.extend(kept);
        closed.push(closing);
        closed
    }
}

#[cfg(test)]
mod tests;
