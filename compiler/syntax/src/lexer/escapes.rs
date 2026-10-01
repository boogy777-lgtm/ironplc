//! Validity of the `$` escapes in a character string literal (IEC 61131-3,
//! B.1.2.2).
//!
//! The lexer keeps a literal's text byte for byte, escapes included. This
//! module only says which escapes the standard does not define, with their
//! byte ranges inside the literal's text, so the parser can report them. It
//! mirrors the escape table of the legacy pipeline, which owns the decoded
//! values; no value is computed here.
//!
//! Both widths accept `$$`, `$'`, `$"`, `$L`, `$N`, `$P`, `$R` and `$T` in
//! either case. A numeric escape is `$` and two hex digits in a single-byte
//! literal, four in a double-byte one. `$U` and eight hex digits is a Unicode
//! escape in either width; its value must be a Unicode scalar. A numeric
//! escape in the surrogate range cannot be a character either.

use core::ops::Range;

/// How many hex digits a `$U` escape carries.
const UNICODE_DIGITS: usize = 8;

/// The byte ranges, in `text`, of the escapes that are not valid. `text` is
/// the literal without its delimiters; `wide` selects the double-byte form.
pub(crate) fn invalid_escapes(text: &str, wide: bool) -> Vec<Range<usize>> {
    let digits = if wide { 4 } else { 2 };
    let mut invalid = Vec::new();
    let mut chars = text.char_indices().peekable();
    while let Some((start, ch)) = chars.next() {
        if ch != '$' {
            continue;
        }
        let Some(&(_, next)) = chars.peek() else {
            invalid.push(start..text.len());
            break;
        };
        if is_named_escape(next) {
            chars.next();
        } else if next == 'U' {
            match hex_run(text, start + 2, UNICODE_DIGITS) {
                Some(value) if char::from_u32(value).is_some() => {
                    for _ in 0..=UNICODE_DIGITS {
                        chars.next();
                    }
                }
                _ => invalid.push(start..unicode_escape_end(text, start)),
            }
        } else {
            match hex_run(text, start + 1, digits) {
                Some(value) if is_scalar_or_local(value) => {
                    for _ in 0..digits {
                        chars.next();
                    }
                }
                _ => invalid.push(start..start + 1 + next.len_utf8()),
            }
        }
    }
    invalid
}

fn is_named_escape(c: char) -> bool {
    matches!(
        c,
        '$' | '\'' | '"' | 'L' | 'l' | 'N' | 'n' | 'P' | 'p' | 'R' | 'r' | 'T' | 't'
    )
}

/// The value of exactly `count` hex digits at `from`, or `None` when fewer
/// follow or one of them is not a hex digit.
fn hex_run(text: &str, from: usize, count: usize) -> Option<u32> {
    let run: String = text.get(from..)?.chars().take(count).collect();
    if run.chars().count() != count || !run.chars().all(|c| c.is_ascii_hexdigit()) {
        return None;
    }
    u32::from_str_radix(&run, 16).ok()
}

/// A numeric escape is a character unless it names a surrogate.
fn is_scalar_or_local(value: u32) -> bool {
    char::from_u32(value).is_some()
}

/// The end of an undecodable `$U` escape: the `$`, the `U`, and the hex
/// digits read with them (at most eight, none past the first non-digit).
fn unicode_escape_end(text: &str, start: usize) -> usize {
    let from = start + 2;
    text.get(from..)
        .map(|rest| {
            rest.char_indices()
                .take(UNICODE_DIGITS)
                .take_while(|(_, c)| c.is_ascii_hexdigit())
                .last()
                .map_or(from, |(offset, c)| from + offset + c.len_utf8())
        })
        .unwrap_or(from)
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
