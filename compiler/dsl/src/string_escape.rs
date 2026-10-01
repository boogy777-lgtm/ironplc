//! The `$` escapes of character string literals (IEC 61131-3, B.1.2.2).
//!
//! This is the one escape table: the parser decodes a literal's source text
//! with [`decode`], and anything that writes a literal back as source uses
//! [`encode`]. See `specs/design/string-literals.md`.
//!
//! Both widths accept `$$`, `$L`, `$N`, `$P`, `$R` and `$T`, in either case.
//! A single-byte (`STRING`) literal spells its delimiter `$'` and any
//! character as `$` and two hex digits; a double-byte (`WSTRING`) literal
//! spells its delimiter `$"` and any character as `$` and four hex digits.
//! The other width's delimiter may be escaped too and stands for itself.
//! `$N` (newline) is a line feed, the same character as `$L`: the standard
//! leaves the newline character to the implementation.
//!
//! A numeric escape whose value is in `0x80..=0xFF` denotes the character
//! **Windows-1252** gives that byte, not the Latin-1 one: `$80` is `€`
//! (U+20AC), the way CODESYS decodes its local codepoints. `$U` and exactly
//! eight hex digits is a Unicode escape, valid in either width, for a code
//! point no numeric escape can spell (`$U000020AC` is the same `€`); it is
//! spelled with a capital `U`, as CODESYS spells it.

use core::ops::Range;

use crate::common::StringType;

/// The characters of a literal, and where its source text has an escape the
/// standard does not define.
#[derive(Debug, PartialEq)]
pub struct Decoded {
    /// The characters the literal denotes. An invalid escape is kept as
    /// written.
    pub chars: Vec<char>,
    /// The byte range, in the text given to [`decode`], of each invalid
    /// escape: the `$` and the characters after it that were read with it.
    pub invalid: Vec<Range<usize>>,
}

/// Decodes the text between a literal's delimiters.
pub fn decode(text: &str, width: &StringType) -> Decoded {
    let digits = hex_digit_count(width);
    let mut chars = Vec::with_capacity(text.len());
    let mut invalid = Vec::new();
    let mut rest = text.char_indices().peekable();

    while let Some((start, ch)) = rest.next() {
        if ch != '$' {
            chars.push(ch);
            continue;
        }
        let Some(&(_, next)) = rest.peek() else {
            invalid.push(start..text.len());
            chars.push('$');
            break;
        };
        if let Some(named) = named_escape(next) {
            rest.next();
            chars.push(named);
            continue;
        }
        if next == 'U' {
            match unicode_escape(text, start) {
                Some(decoded) => {
                    rest.next();
                    for _ in 0..UNICODE_DIGITS {
                        rest.next();
                    }
                    chars.push(decoded);
                }
                None => {
                    invalid.push(unicode_escape_range(text, start));
                    chars.push('$');
                }
            }
            continue;
        }
        let hex: String = text[start + 1..].chars().take(digits).collect();
        let code = (hex.len() == digits && hex.chars().all(|c| c.is_ascii_hexdigit()))
            .then(|| u32::from_str_radix(&hex, 16).ok())
            .flatten()
            .and_then(local_codepoint);
        match code {
            Some(decoded) => {
                for _ in 0..digits {
                    rest.next();
                }
                chars.push(decoded);
            }
            None => {
                invalid.push(start..start + 1 + next.len_utf8());
                chars.push('$');
            }
        }
    }

    Decoded { chars, invalid }
}

/// How many hex digits follow the `U` of a Unicode escape.
const UNICODE_DIGITS: usize = 8;

/// The character a Unicode escape `$Uxxxxxxxx` at `start` denotes.
///
/// `None` when fewer than eight hex digits follow the `U`, when one of them
/// is not a hex digit, or when the value is not a Unicode scalar (a
/// surrogate).
fn unicode_escape(text: &str, start: usize) -> Option<char> {
    let hex: String = text[start + 2..].chars().take(UNICODE_DIGITS).collect();
    (hex.len() == UNICODE_DIGITS)
        .then(|| u32::from_str_radix(&hex, 16).ok())
        .flatten()
        .and_then(char::from_u32)
}

/// The range of the text that an undecodable `$U` escape covers: the `$`,
/// the `U`, and the hex digits read with it (at most eight, and none past the
/// first character that is not one).
fn unicode_escape_range(text: &str, start: usize) -> core::ops::Range<usize> {
    let end = text[start + 2..]
        .char_indices()
        .take(UNICODE_DIGITS)
        .take_while(|(_, c)| c.is_ascii_hexdigit())
        .last()
        .map(|(offset, c)| start + 2 + offset + c.len_utf8())
        .unwrap_or(start + 2);
    start..end
}

/// The character a local codepoint (a numeric escape's value) denotes.
///
/// Values `0x80..=0xFF` are decoded through Windows-1252, which is how
/// CODESYS reads a local codepoint; the band `0x80..=0x9F` is the part of
/// that range where Windows-1252 and Latin-1 disagree. Everything else is
/// the code point itself.
fn local_codepoint(value: u32) -> Option<char> {
    match value {
        0x80..=0x9F => Some(CP1252[(value - 0x80) as usize]),
        _ => char::from_u32(value),
    }
}

/// Windows-1252's `0x80..=0x9F` band. The bytes Windows-1252 leaves
/// undefined (0x81, 0x8D, 0x8F, 0x90, 0x9D) decode to the code point of
/// their own value, as .NET's `GetEncoding(1252)` does.
const CP1252: [char; 32] = [
    '\u{20AC}', '\u{81}', '\u{201A}', '\u{0192}', '\u{201E}', '\u{2026}', '\u{2020}', '\u{2021}',
    '\u{02C6}', '\u{2030}', '\u{0160}', '\u{2039}', '\u{0152}', '\u{8D}', '\u{017D}', '\u{8F}',
    '\u{90}', '\u{2018}', '\u{2019}', '\u{201C}', '\u{201D}', '\u{2022}', '\u{2013}', '\u{2014}',
    '\u{02DC}', '\u{2122}', '\u{0161}', '\u{203A}', '\u{0153}', '\u{9D}', '\u{017E}', '\u{0178}',
];

/// Encodes characters as the text between a literal's delimiters, so that
/// [`decode`] gives the same characters back.
pub fn encode(chars: &[char], width: &StringType) -> String {
    let mut text = String::with_capacity(chars.len());
    for &ch in chars {
        match ch {
            '$' => text.push_str("$$"),
            '\n' => text.push_str("$L"),
            '\r' => text.push_str("$R"),
            '\u{0C}' => text.push_str("$P"),
            '\t' => text.push_str("$T"),
            _ if ch == width.delimiter() => {
                text.push('$');
                text.push(ch);
            }
            // A control character the literal's own width cannot spell with a
            // numeric escape (the Windows-1252 band `0x80..=0x9F` is held by
            // the printable characters of that encoding) is written as a
            // Unicode escape, which [`decode`] reads back to the same
            // character.
            _ if ch.is_control() => {
                let local = u32::from(ch);
                if local <= 0xFF && local_codepoint(local) == Some(ch) {
                    match width {
                        StringType::String => text.push_str(&format!("${local:02X}")),
                        StringType::WString => text.push_str(&format!("${local:04X}")),
                    }
                } else {
                    text.push_str(&format!("$U{:08X}", u32::from(ch)));
                }
            }
            _ => text.push(ch),
        }
    }
    text
}

/// The number of hex digits that follow `$` in a numeric escape.
fn hex_digit_count(width: &StringType) -> usize {
    match width {
        StringType::String => 2,
        StringType::WString => 4,
    }
}

/// The character a named escape `$c` stands for.
fn named_escape(c: char) -> Option<char> {
    match c {
        '$' => Some('$'),
        '\'' => Some('\''),
        '"' => Some('"'),
        'L' | 'l' | 'N' | 'n' => Some('\n'),
        'P' | 'p' => Some('\u{0C}'),
        'R' | 'r' => Some('\r'),
        'T' | 't' => Some('\t'),
        _ => None,
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use rstest::rstest;

    const NARROW: StringType = StringType::String;
    const WIDE: StringType = StringType::WString;

    fn chars(s: &str) -> Vec<char> {
        s.chars().collect()
    }

    #[rstest]
    #[case::plain("abc", "abc")]
    #[case::dollar("costs $$5", "costs $5")]
    #[case::quote("it$'s", "it's")]
    #[case::double_quote("say $\"hi$\"", "say \"hi\"")]
    #[case::line_feed("a$Lb", "a\nb")]
    #[case::newline_is_line_feed("a$nb", "a\nb")]
    #[case::page("$P", "\u{0C}")]
    #[case::carriage_return("$r", "\r")]
    #[case::tab("$T", "\t")]
    #[case::hex("$41$42", "AB")]
    #[case::hex_lower_case("$e9", "é")]
    #[case::raw_double_quote("\"", "\"")]
    #[case::cp1252_euro("costs $805", "costs €5")]
    #[case::cp1252_upper_band("$FF", "ÿ")]
    #[case::cp1252_undefined_byte("$81", "\u{81}")]
    #[case::unicode("$U000020AC", "€")]
    #[case::unicode_ascii("a$U00000041b", "aAb")]
    #[case::unicode_hex_after_escape("$U000000421", "B1")]
    fn decode_when_narrow_then_denoted_characters(#[case] text: &str, #[case] expected: &str) {
        let decoded = decode(text, &NARROW);
        assert!(decoded.invalid.is_empty(), "{decoded:?}");
        assert_eq!(chars(expected), decoded.chars);
    }

    #[rstest]
    #[case::hex("$0041$20AC", "A€")]
    #[case::quote("say $\"hi$\"", "say \"hi\"")]
    #[case::single_quote_escaped("it$'s", "it's")]
    #[case::named("$L$T$$", "\n\t$")]
    #[case::cp1252_euro("$0080", "€")]
    #[case::cp1252_upper_band("$00FF", "ÿ")]
    #[case::unicode("$U000020AC", "€")]
    fn decode_when_wide_then_denoted_characters(#[case] text: &str, #[case] expected: &str) {
        let decoded = decode(text, &WIDE);
        assert!(decoded.invalid.is_empty(), "{decoded:?}");
        assert_eq!(chars(expected), decoded.chars);
    }

    #[rstest]
    #[case::unknown_letter("a$qb", NARROW, 1..3, "a$qb")]
    #[case::one_hex_digit("$4", NARROW, 0..2, "$4")]
    #[case::two_digits_in_wide("$41", WIDE, 0..2, "$41")]
    #[case::trailing_dollar("ab$", NARROW, 2..3, "ab$")]
    #[case::surrogate("$D800", WIDE, 0..2, "$D800")]
    // The range covers `$U` and the hex-digit run read with it, here
    // `0041b`, which is five hex digits and so cannot be the eight.
    #[case::unicode_four_digits("a$U0041b", NARROW, 1..8, "a$U0041b")]
    #[case::unicode_surrogate("$U0000D800", WIDE, 0..10, "$U0000D800")]
    #[case::unicode_non_hex("$U0000ZZZZ", NARROW, 0..6, "$U0000ZZZZ")]
    #[case::unicode_at_end("ab$U", NARROW, 2..4, "ab$U")]
    fn decode_when_invalid_escape_then_reported_and_kept(
        #[case] text: &str,
        #[case] width: StringType,
        #[case] range: Range<usize>,
        #[case] kept: &str,
    ) {
        let decoded = decode(text, &width);
        assert_eq!(vec![range], decoded.invalid);
        assert_eq!(chars(kept), decoded.chars);
    }

    #[test]
    fn decode_when_escape_follows_non_ascii_then_range_is_in_bytes() {
        let decoded = decode("é$q", &NARROW);
        assert_eq!(vec![2..4], decoded.invalid);
    }

    #[rstest]
    #[case::dollar("costs $5", NARROW, "costs $$5")]
    #[case::own_delimiter("it's", NARROW, "it$'s")]
    #[case::other_delimiter_is_plain("say \"hi\"", NARROW, "say \"hi\"")]
    #[case::wide_delimiter("say \"hi\"", WIDE, "say $\"hi$\"")]
    #[case::wide_other_delimiter_is_plain("it's", WIDE, "it's")]
    #[case::named_controls("\n\r\u{0C}\t", NARROW, "$L$R$P$T")]
    #[case::other_control_narrow("\u{01}", NARROW, "$01")]
    #[case::other_control_wide("\u{01}", WIDE, "$0001")]
    // A control character of the Windows-1252 band has no numeric escape
    // that reads back as itself: `$80` means `€`.
    #[case::cp1252_band_control_narrow("\u{80}", NARROW, "$U00000080")]
    #[case::cp1252_band_control_wide("\u{9F}", WIDE, "$U0000009F")]
    #[case::cp1252_printable_as_itself("€ÿ", NARROW, "€ÿ")]
    #[case::non_ascii_as_itself("é€", WIDE, "é€")]
    fn encode_when_characters_then_source_text(
        #[case] value: &str,
        #[case] width: StringType,
        #[case] expected: &str,
    ) {
        assert_eq!(expected, encode(&chars(value), &width));
    }

    #[rstest]
    #[case(NARROW)]
    #[case(WIDE)]
    fn encode_then_decode_when_any_character_then_same_characters(#[case] width: StringType) {
        let value: Vec<char> = (0u32..=0x1FF).filter_map(char::from_u32).collect();
        let decoded = decode(&encode(&value, &width), &width);
        assert!(decoded.invalid.is_empty());
        assert_eq!(value, decoded.chars);
    }
}
