//! Scanners for literals, quoted names and direct addresses.
//!
//! The recognised shapes follow the legacy lexer's token definitions
//! (`ironplc-parser`, `token.rs`) so that the two agree on token boundaries.
//! Typed literals such as `T#1m30s` or `INT#5` are not single tokens here, as
//! in the legacy lexer: the type keyword or identifier, the `#`, and the value
//! parts are separate tokens.

use super::cursor::Cursor;
use super::{error, unexpected_character, unmatched, Scan};
use crate::syntax_kind::SyntaxKind;

/// How a quoted token is delimited.
pub(super) struct Quoted {
    pub(super) quote: u8,
    pub(super) kind: SyntaxKind,
    /// `$` takes the next character with it (IEC 61131-3 B.1.2.2).
    pub(super) escapes: bool,
    /// The token may not contain a line break.
    pub(super) single_line: bool,
    pub(super) unterminated: &'static str,
}

fn is_line_break(byte: u8) -> bool {
    byte == b'\r' || byte == b'\n'
}

/// A quoted token starting at its opening quote. An unterminated multi-line
/// token (a string) becomes an error token for the rest of its first line,
/// so a stray quote does not swallow the remainder of the file.
pub(super) fn scan_quoted(cursor: &mut Cursor<'_>, quoted: &Quoted) -> Scan {
    cursor.bump();
    let mut first_break = None;
    loop {
        match cursor.peek() {
            None => {
                if let Some(pos) = first_break {
                    cursor.set_pos(pos);
                }
                return unmatched(quoted.unterminated);
            }
            Some(byte) if byte == quoted.quote => {
                cursor.bump();
                return (quoted.kind, None);
            }
            Some(byte) if is_line_break(byte) => {
                if quoted.single_line {
                    return unmatched(quoted.unterminated);
                }
                first_break.get_or_insert(cursor.pos());
                cursor.bump();
            }
            Some(b'$') if quoted.escapes => {
                cursor.bump();
                if cursor.peek().is_some_and(|next| !is_line_break(next)) {
                    cursor.bump_char();
                }
            }
            Some(_) => cursor.bump(),
        }
    }
}

fn is_digit_or_underscore(byte: u8) -> bool {
    byte.is_ascii_digit() || byte == b'_'
}

/// Based literals `16#`, `8#`, `2#`: (prefix, kind, first digit, later digits).
type BasedForm = (&'static str, SyntaxKind, fn(u8) -> bool, fn(u8) -> bool);

const BASED_FORMS: [BasedForm; 3] = [
    (
        "16#",
        SyntaxKind::HexLit,
        |byte| byte.is_ascii_digit() || (b'A'..=b'F').contains(&byte),
        |byte| byte.is_ascii_digit() || (b'A'..=b'F').contains(&byte) || byte == b'_',
    ),
    (
        "8#",
        SyntaxKind::OctLit,
        |byte| (b'0'..=b'7').contains(&byte),
        |byte| (b'0'..=b'7').contains(&byte) || byte == b'_',
    ),
    (
        "2#",
        SyntaxKind::BinLit,
        |byte| byte == b'0' || byte == b'1',
        |byte| byte == b'0' || byte == b'1' || byte == b'_',
    ),
];

/// A number starting at a decimal digit: based literal, integer, fixed point
/// or floating point. The sign is never part of the token.
pub(super) fn scan_number(cursor: &mut Cursor<'_>) -> Scan {
    for (prefix, kind, is_first, is_rest) in BASED_FORMS {
        if cursor.starts_with(prefix) && cursor.peek_at(prefix.len()).is_some_and(is_first) {
            cursor.bump_n(prefix.len() + 1);
            cursor.eat_while(is_rest);
            return (kind, None);
        }
    }

    cursor.eat_while(is_digit_or_underscore);
    let mut kind = SyntaxKind::IntegerLit;
    if cursor.peek() == Some(b'.') && cursor.peek_at(1).is_some_and(is_digit_or_underscore) {
        cursor.bump();
        cursor.eat_while(is_digit_or_underscore);
        kind = SyntaxKind::FixedPointLit;
    }
    if matches!(cursor.peek(), Some(b'e' | b'E')) {
        let sign = usize::from(matches!(cursor.peek_at(1), Some(b'+' | b'-')));
        if cursor.peek_at(1 + sign).is_some_and(is_digit_or_underscore) {
            cursor.bump_n(1 + sign);
            cursor.eat_while(is_digit_or_underscore);
            kind = SyntaxKind::FloatingPointLit;
        }
    }
    (kind, None)
}

/// A token starting at `%`: a direct address (`%IX0.1`, `%MW10`), an
/// incomplete one (`%I*`), or a partial-access selector (`%X3`, `%W1`).
/// A `%` followed by anything else is an error token of its own.
pub(super) fn scan_percent(cursor: &mut Cursor<'_>) -> Scan {
    cursor.bump();
    match cursor.peek().map(|byte| byte.to_ascii_uppercase()) {
        Some(b'I' | b'Q' | b'M') => {
            cursor.bump();
            if cursor.peek() == Some(b'*') {
                cursor.bump();
                return (SyntaxKind::DirectAddressIncomplete, None);
            }
            if matches!(
                cursor.peek().map(|byte| byte.to_ascii_uppercase()),
                Some(b'X' | b'B' | b'W' | b'D' | b'L')
            ) {
                cursor.bump();
            }
            if !cursor.peek().is_some_and(|byte| byte.is_ascii_digit()) {
                return error("incomplete direct address");
            }
            cursor.eat_while(|byte| byte.is_ascii_digit());
            while cursor.peek() == Some(b'.')
                && cursor.peek_at(1).is_some_and(|b| b.is_ascii_digit())
            {
                cursor.bump();
                cursor.eat_while(|byte| byte.is_ascii_digit());
            }
            (SyntaxKind::DirectAddress, None)
        }
        Some(b'X' | b'B' | b'W' | b'D' | b'L')
            if cursor.peek_at(1).is_some_and(|byte| byte.is_ascii_digit()) =>
        {
            cursor.bump();
            cursor.eat_while(|byte| byte.is_ascii_digit());
            (SyntaxKind::PartialAccess, None)
        }
        _ => unexpected_character(),
    }
}
