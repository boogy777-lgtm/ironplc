//! Rendering syntax errors as compiler diagnostics.
//!
//! The error kind picks the problem code and the byte range becomes the span
//! of the primary label, over the original source text, the same coordinates
//! the legacy parser reports. The caller supplies the [`FileId`] of the text
//! that was parsed.

use crate::error::{ErrorKind, SyntaxError};
use crate::parser::Parse;
use crate::pragma::Fault;
use ironplc_dsl::core::{FileId, SourceSpan};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_problems::Problem;

impl ErrorKind {
    /// The problem a syntax error of this kind is reported as.
    pub fn problem(self) -> Problem {
        match self {
            ErrorKind::Syntax => Problem::SyntaxError,
            ErrorKind::UnexpectedCharacter => Problem::UnexpectedToken,
            ErrorKind::CStyleComment => Problem::CStyleComment,
            ErrorKind::EmptyVarBlock => Problem::EmptyVarBlock,
            ErrorKind::InvalidStringEscape => Problem::InvalidStringEscape,
            ErrorKind::PartialAccessSyntaxDisabled => Problem::PartialAccessSyntaxDisabled,
            ErrorKind::ParenStringLengthNotAllowed => Problem::ParenStringLengthNotAllowed,
            ErrorKind::IncompleteArrayNotAllowed => Problem::IncompleteArrayNotAllowed,
            ErrorKind::EscapedIdentifierNotAllowed => Problem::EscapedIdentifierNotAllowed,
            ErrorKind::UnicodeIdentifierNotAllowed => Problem::UnicodeIdentifierNotAllowed,
            ErrorKind::MultipleUnderscoresNotAllowed => Problem::MultipleUnderscoresNotAllowed,
            ErrorKind::PragmaIfUnmatched => Fault::Unmatched.problem(),
            ErrorKind::PragmaValueExpected => Fault::UnexpectedValue.problem(),
        }
    }
}

impl SyntaxError {
    /// The diagnostic for this error in the file `file_id`.
    pub fn to_diagnostic(&self, file_id: &FileId) -> Diagnostic {
        let span = SourceSpan::range(
            usize::from(self.range.start()),
            usize::from(self.range.end()),
        )
        .with_file_id(file_id);
        Diagnostic::problem(self.kind.problem(), Label::span(span, self.message.clone()))
    }
}

impl Parse {
    /// The diagnostics for every error of the parse, in source order, for a
    /// parse of the text of `file_id`.
    pub fn diagnostics(&self, file_id: &FileId) -> Vec<Diagnostic> {
        self.errors
            .iter()
            .map(|error| error.to_diagnostic(file_id))
            .collect()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::parse_source_file;
    use crate::parser::options::ParseOptions;

    #[test]
    fn to_diagnostic_when_grammar_error_then_syntax_error_code_and_byte_span() {
        let parse = parse_source_file("PROGRAM p x END_PROGRAM", &ParseOptions::default());
        let file_id = FileId::from_string("a.st");
        let diagnostics = parse.diagnostics(&file_id);
        assert!(!diagnostics.is_empty());
        assert_eq!(diagnostics[0].code, Problem::SyntaxError.code());
        assert_eq!(diagnostics[0].primary.file_id, file_id);
        let range = parse.errors[0].range;
        assert_eq!(
            diagnostics[0].primary.location.start,
            usize::from(range.start())
        );
        assert_eq!(
            diagnostics[0].primary.location.end,
            usize::from(range.end())
        );
    }

    #[test]
    fn to_diagnostic_when_gated_form_then_the_rule_problem_code() {
        let parse = parse_source_file("PROGRAM p\n// note\nEND_PROGRAM", &ParseOptions::default());
        let diagnostics = parse.diagnostics(&FileId::default());
        assert_eq!(diagnostics[0].code, Problem::CStyleComment.code());
    }
}
