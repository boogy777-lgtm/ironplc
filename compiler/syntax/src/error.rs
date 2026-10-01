//! The syntax-layer error type.
//!
//! A syntax error is a message plus the byte range it covers. Ranges are over
//! the original source text (design: parse-tree architecture, section 3.1), so
//! they are complete even when the tree contains error tokens. Rendering a
//! syntax error into a user-facing diagnostic with a problem code is the
//! consumer's job.

use core::fmt;
use rowan::TextRange;

/// A problem found while lexing or parsing, anchored to a byte range.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct SyntaxError {
    /// Human-readable description of what is wrong.
    pub message: String,
    /// The bytes of the original source this error covers.
    pub range: TextRange,
}

impl SyntaxError {
    /// Creates an error covering `range`.
    pub fn new(message: impl Into<String>, range: TextRange) -> Self {
        SyntaxError {
            message: message.into(),
            range,
        }
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
