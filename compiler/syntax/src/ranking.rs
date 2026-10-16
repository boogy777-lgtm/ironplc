//! Which of the errors of a parse is the one reported.
//!
//! A parse reports every error it finds, in source order. A consumer that
//! reports one problem per file picks the *primary* error, and the choice is
//! the same for every consumer: errors are ranked by the stage that finds them,
//! the earliest stage first, and errors of one stage by position. The stages
//! are the order in which the checks of the compiler's established behaviour
//! run: bytes that are not tokens, then one gated form after another, then the
//! conditional pragmas, and last the grammar. A form that is a mistake in any
//! case ranks above a mistake in how the text is put together, because the
//! second is often the consequence of the first.
//!
//! The order is the data in [`STAGES`]. A new problem is one row, or one more
//! problem in an existing row when it is another spelling of what that stage
//! checks.

use crate::error::SyntaxError;
use crate::parser::Parse;
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_problems::Problem;

/// One stage of checking and the problems it reports. Errors of the problems
/// of one stage are ordered by position.
pub struct Stage {
    /// What the stage looks at.
    pub name: &'static str,
    pub problems: &'static [Problem],
}

/// The stages in rank order, the first stage first. A problem is in at most
/// one stage; an error of a problem that is in none ranks after every stage.
///
/// Lowering follows the last stage: it runs only on a tree that has no error,
/// so a problem it finds is never ranked against these.
pub const STAGES: &[Stage] = &[
    Stage {
        name: "bytes that match no token",
        problems: &[Problem::UnexpectedToken],
    },
    Stage {
        name: "C-style comments",
        problems: &[Problem::CStyleComment],
    },
    Stage {
        name: "empty variable blocks",
        problems: &[Problem::EmptyVarBlock],
    },
    Stage {
        name: "partial-access syntax",
        problems: &[Problem::PartialAccessSyntaxDisabled],
    },
    Stage {
        name: "parenthesis string length",
        problems: &[Problem::ParenStringLengthNotAllowed],
    },
    Stage {
        name: "incomplete arrays",
        problems: &[Problem::IncompleteArrayNotAllowed],
    },
    Stage {
        name: "identifier spelling",
        problems: &[
            Problem::EscapedIdentifierNotAllowed,
            Problem::UnicodeIdentifierNotAllowed,
            Problem::MultipleUnderscoresNotAllowed,
        ],
    },
    Stage {
        name: "string escapes",
        problems: &[Problem::InvalidStringEscape],
    },
    Stage {
        name: "conditional pragmas",
        problems: &[Problem::PragmaIfUnmatched, Problem::PragmaValueExpected],
    },
    Stage {
        // The nesting limit is a limit of the grammar: the text that is too
        // deep is reported where it is, among the other faults of how the text
        // is put together, not ahead of them.
        name: "grammar",
        problems: &[Problem::SyntaxError, Problem::MaxNestingDepthExceeded],
    },
];

/// The rank of the stage that reports `problem`: lower ranks first. A problem
/// that is in no stage ranks after every stage.
pub(crate) fn rank(problem: Problem) -> usize {
    STAGES
        .iter()
        .position(|stage| stage.problems.contains(&problem))
        .unwrap_or(STAGES.len())
}

impl Parse {
    /// The error to report when one is reported: the lowest rank, and the
    /// earliest of that rank. Of errors at one place the first found is
    /// reported, and a construct is closed, so reports its missing closer,
    /// before the construct around it: input that ends inside several open
    /// constructs is reported by the innermost one's closer. `None` when the
    /// parse found no error.
    pub fn primary_error(&self) -> Option<&SyntaxError> {
        self.errors.iter().min_by_key(|error| {
            (
                rank(error.problem()),
                error.range.start(),
                error.range.end(),
            )
        })
    }

    /// The diagnostic of [`Parse::primary_error`], for a parse of the text of
    /// `file_id`.
    pub fn primary_diagnostic(&self, file_id: &FileId) -> Option<Diagnostic> {
        self.primary_error()
            .map(|error| error.to_diagnostic(file_id))
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::{parse_source_file, ParseOptions, MAX_DEPTH};

    #[test]
    fn rank_when_problem_listed_then_it_is_in_exactly_one_stage() {
        let listed: Vec<&str> = STAGES
            .iter()
            .flat_map(|stage| stage.problems.iter().map(Problem::code))
            .collect();
        for code in &listed {
            assert_eq!(listed.iter().filter(|other| *other == code).count(), 1);
        }
        for problem in STAGES.iter().flat_map(|stage| stage.problems) {
            assert!(rank(*problem) < STAGES.len(), "{}", problem.code());
        }
    }

    #[test]
    fn primary_error_when_no_error_then_none() {
        let parse = parse_source_file("PROGRAM p END_PROGRAM", &ParseOptions::all());
        assert!(parse.primary_error().is_none());
        assert!(parse.primary_diagnostic(&FileId::default()).is_none());
    }

    #[test]
    fn primary_error_when_grammar_error_precedes_a_gated_form_then_the_gated_form() {
        // The grammar error is first in the file; the C-style comment ranks
        // above it.
        let source = "PROGRAM p x := ; // note\nEND_PROGRAM";
        let parse = parse_source_file(source, &ParseOptions::default());
        assert_eq!(parse.errors[0].problem(), Problem::SyntaxError);
        let primary = parse.primary_error().map(SyntaxError::problem);
        assert_eq!(primary, Some(Problem::CStyleComment));
    }

    #[test]
    fn primary_error_when_two_of_one_stage_then_the_earlier_position() {
        let source = "PROGRAM p\n// one\n// two\nEND_PROGRAM";
        let parse = parse_source_file(source, &ParseOptions::default());
        let primary = parse
            .primary_error()
            .map(|error| usize::from(error.range.start()));
        assert_eq!(primary, source.find("// one"));
    }

    fn deep() -> String {
        format!(
            "{}1{}",
            "(".repeat(2 * MAX_DEPTH),
            ")".repeat(2 * MAX_DEPTH)
        )
    }

    #[test]
    fn primary_error_when_nesting_is_too_deep_after_a_grammar_error_then_the_grammar_error() {
        let source = format!("PROGRAM p\nx := ;\ny := {};\nEND_PROGRAM", deep());
        let parse = parse_source_file(&source, &ParseOptions::default());
        assert!(parse
            .errors
            .iter()
            .any(|error| error.problem() == Problem::MaxNestingDepthExceeded));
        let primary = parse.primary_error().map(SyntaxError::problem);
        assert_eq!(primary, Some(Problem::SyntaxError));
    }

    #[test]
    fn primary_error_when_nesting_is_too_deep_before_a_grammar_error_then_the_nesting_error() {
        let source = format!("PROGRAM p\ny := {};\nx := ;\nEND_PROGRAM", deep());
        let parse = parse_source_file(&source, &ParseOptions::default());
        let primary = parse.primary_error().map(SyntaxError::problem);
        assert_eq!(primary, Some(Problem::MaxNestingDepthExceeded));
    }

    #[test]
    fn primary_error_when_nesting_is_too_deep_before_a_conditional_pragma_error_then_the_pragma() {
        let source = format!("PROGRAM p\ny := {};\n{{END_IF}}\nEND_PROGRAM", deep());
        let parse = parse_source_file(&source, &ParseOptions::all());
        let primary = parse.primary_error().map(SyntaxError::problem);
        assert_eq!(primary, Some(Problem::PragmaIfUnmatched));
    }
}
