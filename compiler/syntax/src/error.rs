//! The syntax-layer error type.
//!
//! A syntax error is a text plus the byte range it covers. Ranges are over
//! the original source text (design: parse-tree architecture, section 3.1), so
//! they are complete even when the tree contains error tokens. The text is a
//! row of `problem-texts.csv`: it carries the problem the error is reported
//! under, so no second fact decides the code (see the `diagnostic` module).

use crate::parser::MAX_DEPTH;
use crate::syntax_kind::SyntaxKind;
use core::fmt;
use ironplc_problems::{text, Problem, Text};
use rowan::TextRange;

/// What is said of nesting beyond [`MAX_DEPTH`], wherever it is found.
pub(crate) fn nesting_too_deep() -> Text {
    text::max_nesting_depth_exceeded_over_limit(MAX_DEPTH)
}

/// What is said when the token `kind` was expected and is not next: its name
/// is the spelling the declaration of the kind gives it.
pub(crate) fn expected_token(kind: SyntaxKind) -> Text {
    let spelling = kind.spelling();
    debug_assert!(spelling.is_some(), "{kind:?} has no spelling to expect");
    text::syntax_error_expected_what(format_args!("`{}`", spelling.unwrap_or_default()))
}

/// A problem found while lexing or parsing, anchored to a byte range.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct SyntaxError {
    /// What is wrong, and the problem it is reported under.
    pub text: Text,
    /// The bytes of the original source this error covers.
    pub range: TextRange,
    /// What to do about it, for a problem whose fix is the same every time
    /// (a form the dialect has not enabled, an undefined escape).
    pub help: Option<&'static str>,
}

impl SyntaxError {
    /// Creates an error covering `range`.
    pub fn new(text: Text, range: TextRange) -> Self {
        SyntaxError {
            text,
            range,
            help: None,
        }
    }

    /// Returns the error with advice on how to fix it.
    pub fn with_help(mut self, help: &'static str) -> Self {
        self.help = Some(help);
        self
    }

    /// The problem the error is reported as.
    pub fn problem(&self) -> Problem {
        *self.text.problem()
    }

    /// Human-readable description of what is wrong.
    pub fn message(&self) -> &str {
        self.text.message()
    }
}

impl fmt::Display for SyntaxError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "{} at bytes {}..{}",
            self.message(),
            u32::from(self.range.start()),
            u32::from(self.range.end())
        )
    }
}

impl std::error::Error for SyntaxError {}

#[cfg(test)]
mod tests {
    use super::*;
    use rowan::TextSize;

    #[test]
    fn display_when_error_then_message_and_byte_range() {
        let error = SyntaxError::new(
            text::unexpected_token_unexpected_character(),
            TextRange::new(TextSize::from(3), TextSize::from(4)),
        );
        assert_eq!(error.to_string(), "unexpected character at bytes 3..4");
    }
}
