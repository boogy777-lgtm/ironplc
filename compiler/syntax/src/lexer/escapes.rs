//! Validity of the `$` escapes in a character string literal (IEC 61131-3,
//! B.1.2.2).
//!
//! The lexer keeps a literal's text byte for byte, escapes included. This
//! module only says which escapes the standard does not define, with their
//! byte ranges inside the literal's text, so the parser can report them. The
//! escape table itself is the one in `ironplc_dsl::string_escape`, which
//! also decodes the values; no value is computed here.

use core::ops::Range;

use ironplc_dsl::common::StringType;
use ironplc_dsl::string_escape::decode;

/// The byte ranges, in `text`, of the escapes that are not valid. `text` is
/// the literal without its delimiters; `wide` selects the double-byte form.
pub(crate) fn invalid_escapes(text: &str, wide: bool) -> Vec<Range<usize>> {
    let width = if wide {
        StringType::WString
    } else {
        StringType::String
    };
    decode(text, &width).invalid
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn invalid_escapes_when_all_defined_then_none() {
        assert!(invalid_escapes("a$$b$'c$L$N$P$R$T$l$41$U000020AC", false).is_empty());
        assert!(invalid_escapes("$\"$0041", true).is_empty());
    }

    #[test]
    fn invalid_escapes_when_unknown_letter_then_range_of_dollar_and_letter() {
        assert_eq!(invalid_escapes("ab$Qcd", false), vec![2..4]);
    }

    #[test]
    fn invalid_escapes_when_trailing_dollar_then_range_to_end() {
        assert_eq!(invalid_escapes("ab$", false), vec![2..3]);
    }

    #[test]
    fn invalid_escapes_when_hex_digits_missing_then_flagged() {
        assert_eq!(invalid_escapes("$4", false), vec![0..2]);
        assert_eq!(invalid_escapes("$004", true), vec![0..2]);
    }

    #[test]
    fn invalid_escapes_when_unicode_escape_short_or_surrogate_then_flagged_over_its_digits() {
        assert_eq!(invalid_escapes("$U0041", false), vec![0..6]);
        assert_eq!(invalid_escapes("$UD800ABCD", false), vec![0..2 + 8]);
    }

    #[test]
    fn invalid_escapes_when_wide_surrogate_then_flagged() {
        assert_eq!(invalid_escapes("$D800", true), vec![0..2]);
    }
}
