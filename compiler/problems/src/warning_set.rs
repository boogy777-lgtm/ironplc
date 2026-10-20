//! The set of warnings a setting lists, and the one function that reads a code
//! written in a setting.
//!
//! A setting that disables a warning, or reports it as an error, lists codes
//! (`DisabledWarningIds` and `WarningAsErrorIds` of CODESYS). The rules of such a
//! list are written once, here:
//!
//! - **Only a warning can be listed.** The set holds [`WarningProblem`]s, a type
//!   with one value for each problem that declares the category Warning, so a
//!   code that is not a warning has no value to be listed under. The one way to
//!   make a set from text, [`WarningSet::from_code`], refuses everything else
//!   with a message of the problem `P0013`.
//! - **A code is written as the dictionary writes it, or as CODESYS does.** The
//!   IronPLC code (`P4083`) and the CODESYS code of the `Codesys` column
//!   (`C0354`) are accepted, with the case exact as the CSV writes it. A CODESYS
//!   code names a condition, which more than one IronPLC problem may report, so
//!   it gives a set. The pragma `{warning disable Cnnnn}` reads its code through
//!   this function too.
//! - **It is a bit set.** The set is `Copy` and its size is fixed by the number
//!   of warnings in the CSV, so it travels with the compiler options and a new
//!   warning needs no edit here.

use crate::{text, Problem, Severity, Text, WarningProblem};

/// The number of 64-bit words that hold one bit for each warning problem.
const WORDS: usize = WarningProblem::ALL.len().div_ceil(64);

/// A set of warnings. The empty set is the default.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct WarningSet([u64; WORDS]);

impl Default for WarningSet {
    fn default() -> Self {
        WarningSet([0; WORDS])
    }
}

impl WarningSet {
    /// Whether the set holds the warning.
    pub fn contains(&self, warning: WarningProblem) -> bool {
        let index = warning.index();
        self.0
            .get(index / 64)
            .is_some_and(|word| word & (1 << (index % 64)) != 0)
    }

    /// The set with the warning added.
    pub fn with(mut self, warning: WarningProblem) -> Self {
        let index = warning.index();
        if let Some(word) = self.0.get_mut(index / 64) {
            *word |= 1 << (index % 64);
        }
        self
    }

    /// The warnings of both sets.
    pub fn union(mut self, other: WarningSet) -> Self {
        for (word, other) in self.0.iter_mut().zip(other.0) {
            *word |= other;
        }
        self
    }

    /// Whether the set holds no warning.
    pub fn is_empty(&self) -> bool {
        self.0.iter().all(|word| *word == 0)
    }

    /// The warnings of the set, in the order of [`WarningProblem::ALL`].
    pub fn iter(&self) -> impl Iterator<Item = WarningProblem> + '_ {
        WarningProblem::ALL
            .into_iter()
            .filter(|warning| self.contains(*warning))
    }

    /// The warnings a code written in a setting names, or the message that says
    /// why the code is not accepted.
    ///
    /// - An IronPLC code (`P4083`) names the one warning that has it.
    /// - A CODESYS code (`C0354`) names every warning that reports the same
    ///   condition (the `Codesys` column), which is one or more.
    /// - The case is exact, as in the CSV: `p4083` and `c0354` are unknown. The
    ///   owner decided that case is handled by a separate rewrite of the text
    ///   later, not by this function.
    /// - A code that names only problems that are not warnings is refused with
    ///   the category it has: an error cannot be disabled or raised.
    /// - Any other text is unknown.
    ///
    /// The message is a row of `problem-texts.csv` of the problem `P0013`, and it
    /// names the code and lists the codes that are accepted.
    pub fn from_code(code: &str) -> Result<WarningSet, Text> {
        if let Some(problem) = Problem::from_code(code) {
            return match WarningProblem::of(problem) {
                Some(warning) => Ok(WarningSet::default().with(warning)),
                None => Err(not_a_warning(code, problem.severity())),
            };
        }

        let reporting: Vec<Problem> = Problem::ALL
            .into_iter()
            .filter(|problem| problem.codesys() == Some(code))
            .collect();
        let Some(first) = reporting.first() else {
            return Err(text::warning_code_invalid_unknown(code, accepted_codes()));
        };
        let warnings = reporting
            .iter()
            .filter_map(|problem| WarningProblem::of(*problem))
            .fold(WarningSet::default(), WarningSet::with);
        if warnings.is_empty() {
            Err(not_a_warning(code, first.severity()))
        } else {
            Ok(warnings)
        }
    }
}

impl WarningProblem {
    /// The warning that a problem is, or `None` when the problem is not a
    /// warning.
    pub fn of(problem: Problem) -> Option<WarningProblem> {
        WarningProblem::ALL
            .into_iter()
            .find(|warning| warning.problem() == problem)
    }

    /// The warning that has the IronPLC code, written as the CSV writes it.
    pub fn from_code(code: &str) -> Option<WarningProblem> {
        Problem::from_code(code).and_then(WarningProblem::of)
    }
}

fn not_a_warning(code: &str, severity: Severity) -> Text {
    text::warning_code_invalid_not_a_warning(code, severity.script_name(), accepted_codes())
}

/// The codes `from_code` accepts, for the message that refuses a code: the
/// IronPLC code of each warning and, where it has one, its CODESYS code.
fn accepted_codes() -> String {
    let mut codes: Vec<String> = Vec::new();
    for warning in WarningProblem::ALL {
        let problem = warning.problem();
        codes.push(problem.code().to_string());
        if let Some(codesys) = problem.codesys() {
            if !codes.iter().any(|code| code == codesys) {
                codes.push(codesys.to_string());
            }
        }
    }
    codes.join(", ")
}

#[cfg(test)]
mod tests;
