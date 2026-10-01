//! The syntax-layer error type.
//!
//! A syntax error is a message plus the byte range it covers. Ranges are over
//! the original source text (design: parse-tree architecture, section 3.1), so
//! they are complete even when the tree contains error tokens. Each error has
//! a kind, and the kind decides the problem code the error is reported under
//! (see the `diagnostic` module).

use core::fmt;
use rowan::TextRange;

/// What kind of problem a [`SyntaxError`] is, which decides its problem code.
///
/// Every dialect-gated form has its own kind because the legacy token check
/// rules report each one under its own code.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum ErrorKind {
    /// The text does not follow the grammar.
    Syntax,
    /// Bytes that match no token.
    UnexpectedCharacter,
    /// `//` or `/* */` comment without the flag.
    CStyleComment,
    /// `VAR` block holding no declaration.
    EmptyVarBlock,
    /// `$` escape that the standard does not define.
    InvalidStringEscape,
    /// `.%Xn` selector without the flag.
    PartialAccessSyntaxDisabled,
    /// `STRING(n)` without the flag.
    ParenStringLengthNotAllowed,
    /// `ARRAY [*]` without the flag.
    IncompleteArrayNotAllowed,
    /// Backtick identifier without the flag.
    EscapedIdentifierNotAllowed,
    /// Identifier with letters outside ASCII without the flag.
    UnicodeIdentifierNotAllowed,
    /// Identifier with consecutive underscores without the flag.
    MultipleUnderscoresNotAllowed,
}

/// A problem found while lexing or parsing, anchored to a byte range.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct SyntaxError {
    /// What kind of problem this is.
    pub kind: ErrorKind,
    /// Human-readable description of what is wrong.
    pub message: String,
    /// The bytes of the original source this error covers.
    pub range: TextRange,
}

impl SyntaxError {
    /// Creates a grammar error covering `range`.
    pub fn new(message: impl Into<String>, range: TextRange) -> Self {
        SyntaxError {
            kind: ErrorKind::Syntax,
            message: message.into(),
            range,
        }
    }

    /// Returns the error with its kind replaced.
    pub fn with_kind(mut self, kind: ErrorKind) -> Self {
        self.kind = kind;
        self
    }
}

impl fmt::Display for SyntaxError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "{} at bytes {}..{}",
            self.message,
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
            "unexpected character",
            TextRange::new(TextSize::from(3), TextSize::from(4)),
        );
        assert_eq!(error.to_string(), "unexpected character at bytes 3..4");
    }
}
