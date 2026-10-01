//! Scanners for trivia: whitespace, line breaks, comments and pragmas.
//!
//! Every scanner returns the kind of the token it consumed and, for a
//! malformed construct, the message of the syntax error that goes with the
//! resulting error token.

use super::cursor::Cursor;
use super::{error, Scan};
use crate::syntax_kind::SyntaxKind;

pub(super) fn scan_whitespace(cursor: &mut Cursor<'_>) -> Scan {
    cursor.eat_while(|byte| byte == b' ' || byte == b'\t');
    (SyntaxKind::Whitespace, None)
}

/// One line break, keeping its spelling: `\r\n`, `\n`, a lone `\r`, or a
/// form feed (which the legacy lexer also treats as a line break).
pub(super) fn scan_newline(cursor: &mut Cursor<'_>) -> Scan {
    let first = cursor.peek();
    cursor.bump();
    if first == Some(b'\r') && cursor.peek() == Some(b'\n') {
        cursor.bump();
    }
    (SyntaxKind::Newline, None)
}

/// `//` or `///` up to, not including, the line break.
pub(super) fn scan_line_comment(cursor: &mut Cursor<'_>) -> Scan {
    let kind = if cursor.starts_with("///") {
        SyntaxKind::DocComment
    } else {
        SyntaxKind::LineComment
    };
    cursor.eat_while(|byte| byte != b'\r' && byte != b'\n');
    (kind, None)
}

/// A construct between `open` and `close`. With `nested`, an inner `open`
/// needs its own `close`. A nested construct that never balances ends at its
/// first `close` instead, as the legacy pipeline reads it, so one unbalanced
/// inner opener does not swallow the rest of the source. Unterminated input
/// becomes an error token covering the rest of the source.
pub(super) fn scan_delimited(
    cursor: &mut Cursor<'_>,
    kind: SyntaxKind,
    (open, close): (&str, &str),
    nested: bool,
    unterminated: &'static str,
) -> Scan {
    let start = cursor.pos();
    cursor.bump_n(open.len());
    let mut depth = 1usize;
    let mut inner_opened = false;
    while !cursor.is_at_end() {
        if nested && cursor.starts_with(open) {
            cursor.bump_n(open.len());
            depth += 1;
            inner_opened = true;
        } else if cursor.starts_with(close) {
            cursor.bump_n(close.len());
            depth -= 1;
            if depth == 0 {
                return (kind, None);
            }
        } else {
            cursor.bump();
        }
    }
    if nested && inner_opened {
        cursor.set_pos(start);
        return scan_delimited(cursor, kind, (open, close), false, unterminated);
    }
    error(unterminated)
}
