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
//! The order is the data in [`STAGES`]. A new kind of error is one row, or one
//! more kind in an existing row when it is another spelling of what that
//! stage checks.

use crate::error::{ErrorKind, SyntaxError};
use crate::parser::Parse;
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::Diagnostic;

/// One stage of checking and the kinds of error it reports. Errors of the
/// kinds of one stage are ordered by position.
pub struct Stage {
    /// What the stage looks at.
    pub name: &'static str,
    pub kinds: &'static [ErrorKind],
}

/// The stages in rank order, the first stage first. Every [`ErrorKind`] is in
/// exactly one stage.
///
/// Lowering follows the last stage: it runs only on a tree that has no error,
/// so a problem it finds is never ranked against these.
pub const STAGES: &[Stage] = &[
    Stage {
        name: "bytes that match no token",
        kinds: &[ErrorKind::UnexpectedCharacter],
    },
    Stage {
        name: "C-style comments",
        kinds: &[ErrorKind::CStyleComment],
    },
    Stage {
        name: "empty variable blocks",
        kinds: &[ErrorKind::EmptyVarBlock],
    },
    Stage {
        name: "partial-access syntax",
        kinds: &[ErrorKind::PartialAccessSyntaxDisabled],
    },
    Stage {
        name: "parenthesis string length",
        kinds: &[ErrorKind::ParenStringLengthNotAllowed],
    },
    Stage {
        name: "incomplete arrays",
        kinds: &[ErrorKind::IncompleteArrayNotAllowed],
    },
    Stage {
        name: "identifier spelling",
        kinds: &[
            ErrorKind::EscapedIdentifierNotAllowed,
            ErrorKind::UnicodeIdentifierNotAllowed,
            ErrorKind::MultipleUnderscoresNotAllowed,
        ],
    },
    Stage {
        name: "string escapes",
        kinds: &[ErrorKind::InvalidStringEscape],
    },
    Stage {
        name: "conditional pragmas and the nesting limit",
        kinds: &[
            ErrorKind::PragmaIfUnmatched,
            ErrorKind::PragmaValueExpected,
            ErrorKind::NestingTooDeep,
        ],
    },
    Stage {
        name: "grammar",
        kinds: &[ErrorKind::Syntax],
    },
];

impl ErrorKind {
    /// The rank of the stage that reports errors of this kind: lower ranks
    /// first. A kind that is in no stage ranks after every stage.
    pub fn rank(self) -> usize {
        STAGES
            .iter()
            .position(|stage| stage.kinds.contains(&self))
            .unwrap_or(STAGES.len())
    }
}

impl Parse {
    /// The error to report when one is reported: the lowest rank, and the
    /// earliest of that rank. `None` when the parse found no error.
    pub fn primary_error(&self) -> Option<&SyntaxError> {
        self.errors
            .iter()
            .min_by_key(|error| (error.kind.rank(), error.range.start(), error.range.end()))
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
    use crate::{parse_source_file, ParseOptions};

    /// Every kind. The match has no wildcard arm, so a new kind fails to
    /// compile here until it is added to the list, and then to the stages.
    fn every_kind() -> Vec<ErrorKind> {
        let all = vec![
            ErrorKind::Syntax,
            ErrorKind::UnexpectedCharacter,
            ErrorKind::CStyleComment,
            ErrorKind::EmptyVarBlock,
            ErrorKind::InvalidStringEscape,
            ErrorKind::PartialAccessSyntaxDisabled,
            ErrorKind::ParenStringLengthNotAllowed,
            ErrorKind::IncompleteArrayNotAllowed,
            ErrorKind::EscapedIdentifierNotAllowed,
            ErrorKind::UnicodeIdentifierNotAllowed,
            ErrorKind::MultipleUnderscoresNotAllowed,
            ErrorKind::PragmaIfUnmatched,
            ErrorKind::PragmaValueExpected,
            ErrorKind::NestingTooDeep,
        ];
        for kind in &all {
            match kind {
                ErrorKind::Syntax
                | ErrorKind::UnexpectedCharacter
                | ErrorKind::CStyleComment
                | ErrorKind::EmptyVarBlock
                | ErrorKind::InvalidStringEscape
                | ErrorKind::PartialAccessSyntaxDisabled
                | ErrorKind::ParenStringLengthNotAllowed
                | ErrorKind::IncompleteArrayNotAllowed
                | ErrorKind::EscapedIdentifierNotAllowed
                | ErrorKind::UnicodeIdentifierNotAllowed
                | ErrorKind::MultipleUnderscoresNotAllowed
                | ErrorKind::PragmaIfUnmatched
                | ErrorKind::PragmaValueExpected
                | ErrorKind::NestingTooDeep => {}
            }
        }
        all
    }

    #[test]
    fn rank_when_every_kind_then_it_is_in_exactly_one_stage() {
        for kind in every_kind() {
            let stages = STAGES
                .iter()
                .filter(|stage| stage.kinds.contains(&kind))
                .count();
            assert_eq!(stages, 1, "{kind:?}");
        }
        let listed: usize = STAGES.iter().map(|stage| stage.kinds.len()).sum();
        assert_eq!(listed, every_kind().len());
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
        assert_eq!(parse.errors[0].kind, ErrorKind::Syntax);
        let primary = parse.primary_error().map(|error| error.kind);
        assert_eq!(primary, Some(ErrorKind::CStyleComment));
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
}
