//! Byte-exact ST tokenizer for the S0 spike.
//!
//! Unlike the production pipeline (`ironplc_parser::tokenize_program`), this
//! lexer runs on the **original** text, keeps every trivia byte, and turns
//! unmatched bytes into `ErrorToken`s so the token sequence tiles the source
//! (design section 3.1; audit findings F1/F2). It is small on purpose: it
//! covers what the spike grammar consumes, plus comments/pragmas/strings and
//! error regions.

use crate::syntax::SyntaxKind;
use rowan::{TextRange, TextSize};

/// Token categories produced by [`lex`].
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum TokenKind {
    Whitespace,
    Newline,
    LineComment,
    BlockComment,
    Pragma,
    Ident,
    IntegerLit,
    RealLit,
    StringLit,
    Operator,
    /// Bytes that matched no rule; kept so reconstruction stays exact.
    Error,
}

impl TokenKind {
    /// Trivia is retained by the tree but never drives parsing.
    pub fn is_trivia(self) -> bool {
        matches!(
            self,
            TokenKind::Whitespace
                | TokenKind::Newline
                | TokenKind::LineComment
                | TokenKind::BlockComment
                | TokenKind::Pragma
        )
    }

    /// The CST kind for this token category.
    pub fn syntax_kind(self) -> SyntaxKind {
        match self {
            TokenKind::Whitespace => SyntaxKind::Whitespace,
            TokenKind::Newline => SyntaxKind::Newline,
            TokenKind::LineComment => SyntaxKind::LineComment,
            TokenKind::BlockComment => SyntaxKind::BlockComment,
            TokenKind::Pragma => SyntaxKind::Pragma,
            TokenKind::Ident => SyntaxKind::Ident,
            TokenKind::IntegerLit => SyntaxKind::IntegerLit,
            TokenKind::RealLit => SyntaxKind::RealLit,
            TokenKind::StringLit => SyntaxKind::StringLit,
            TokenKind::Operator => SyntaxKind::Operator,
            TokenKind::Error => SyntaxKind::ErrorToken,
        }
    }
}

/// A token borrowing its exact source slice.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Token<'src> {
    pub kind: TokenKind,
    pub text: &'src str,
    pub range: TextRange,
}

/// A lexing problem; the offending bytes are still covered by a token.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct LexDiagnostic {
    pub message: &'static str,
    pub range: TextRange,
}

/// Tokenize `source` completely: the returned tokens tile `[0, len)` in order.
pub fn lex(source: &str) -> (Vec<Token<'_>>, Vec<LexDiagnostic>) {
    let bytes = source.as_bytes();
    let mut tokens = Vec::new();
    let mut diagnostics = Vec::new();
    let mut pos = 0usize;

    while pos < bytes.len() {
        let start = pos;
        let kind = scan_one(source, bytes, &mut pos, &mut diagnostics);
        tokens.push(Token {
            kind,
            text: &source[start..pos],
            range: range_of(start, pos),
        });
    }

    (tokens, diagnostics)
}

fn range_of(start: usize, end: usize) -> TextRange {
    let start = u32::try_from(start).unwrap_or(u32::MAX);
    let end = u32::try_from(end).unwrap_or(u32::MAX);
    TextRange::new(TextSize::from(start), TextSize::from(end))
}

/// Scans exactly one token starting at `*pos`, advancing `*pos` past it.
fn scan_one(
    source: &str,
    bytes: &[u8],
    pos: &mut usize,
    diagnostics: &mut Vec<LexDiagnostic>,
) -> TokenKind {
    let byte = bytes[*pos];
    match byte {
        b' ' | b'\t' | 0x0b | 0x0c => {
            *pos += 1;
            while *pos < bytes.len() && is_ascii_space(bytes[*pos]) {
                *pos += 1;
            }
            TokenKind::Whitespace
        }
        b'\r' => {
            *pos += 1;
            if *pos < bytes.len() && bytes[*pos] == b'\n' {
                *pos += 1;
            }
            TokenKind::Newline
        }
        b'\n' => {
            *pos += 1;
            TokenKind::Newline
        }
        b'/' if bytes.get(*pos + 1) == Some(&b'/') => {
            *pos += 2;
            while *pos < bytes.len() && bytes[*pos] != b'\n' && bytes[*pos] != b'\r' {
                *pos += 1;
            }
            TokenKind::LineComment
        }
        b'(' if bytes.get(*pos + 1) == Some(&b'*') => {
            let start = *pos;
            *pos += 2;
            let mut closed = false;
            while *pos < bytes.len() {
                if bytes[*pos] == b'*' && bytes.get(*pos + 1) == Some(&b')') {
                    *pos += 2;
                    closed = true;
                    break;
                }
                *pos += 1;
            }
            if !closed {
                diagnostics.push(LexDiagnostic {
                    message: "unterminated block comment",
                    range: range_of(start, *pos),
                });
            }
            TokenKind::BlockComment
        }
        b'{' => {
            let start = *pos;
            *pos += 1;
            let mut closed = false;
            while *pos < bytes.len() {
                if bytes[*pos] == b'}' {
                    *pos += 1;
                    closed = true;
                    break;
                }
                *pos += 1;
            }
            if !closed {
                diagnostics.push(LexDiagnostic {
                    message: "unterminated pragma",
                    range: range_of(start, *pos),
                });
            }
            TokenKind::Pragma
        }
        b'\'' => {
            let start = *pos;
            *pos += 1;
            let mut closed = false;
            while *pos < bytes.len() {
                match bytes[*pos] {
                    b'$' => *pos += 2,
                    b'\'' => {
                        *pos += 1;
                        closed = true;
                        break;
                    }
                    b'\n' | b'\r' => break,
                    _ => *pos += 1,
                }
            }
            if !closed {
                diagnostics.push(LexDiagnostic {
                    message: "unterminated string literal",
                    range: range_of(start, *pos),
                });
            }
            TokenKind::StringLit
        }
        b'A'..=b'Z' | b'a'..=b'z' | b'_' => {
            *pos += 1;
            while *pos < bytes.len() && (bytes[*pos].is_ascii_alphanumeric() || bytes[*pos] == b'_')
            {
                *pos += 1;
            }
            TokenKind::Ident
        }
        b'0'..=b'9' => scan_number(source, bytes, pos),
        _ => {
            if let Some(len) = operator_len(bytes, *pos) {
                *pos += len;
                TokenKind::Operator
            } else if byte >= 0x80 {
                // Consume one whole UTF-8 character so slicing stays valid;
                // the bytes are preserved as an error token either way.
                let len = source[*pos..].chars().next().map_or(1, char::len_utf8);
                *pos += len;
                TokenKind::Error
            } else {
                *pos += 1;
                TokenKind::Error
            }
        }
    }
}

fn is_ascii_space(byte: u8) -> bool {
    matches!(byte, b' ' | b'\t' | 0x0b | 0x0c)
}

fn scan_number(_source: &str, bytes: &[u8], pos: &mut usize) -> TokenKind {
    *pos += 1;
    while *pos < bytes.len() && (bytes[*pos].is_ascii_digit() || bytes[*pos] == b'_') {
        *pos += 1;
    }

    if bytes.get(*pos) == Some(&b'#') {
        // Based literal: 16#FF, 2#1010, 8#77.
        *pos += 1;
        while *pos < bytes.len() && (bytes[*pos].is_ascii_alphanumeric() || bytes[*pos] == b'_') {
            *pos += 1;
        }
        return TokenKind::IntegerLit;
    }

    let mut is_real = false;
    if bytes.get(*pos) == Some(&b'.')
        && bytes
            .get(*pos + 1)
            .is_some_and(|next| next.is_ascii_digit())
    {
        is_real = true;
        *pos += 1;
        while *pos < bytes.len() && (bytes[*pos].is_ascii_digit() || bytes[*pos] == b'_') {
            *pos += 1;
        }
    }

    let exponent_byte = bytes.get(*pos).copied().unwrap_or(0) | 0x20;
    if exponent_byte == b'e' {
        let mut look = *pos + 1;
        if bytes
            .get(look)
            .is_some_and(|next| *next == b'+' || *next == b'-')
        {
            look += 1;
        }
        if bytes.get(look).is_some_and(|next| next.is_ascii_digit()) {
            is_real = true;
            *pos = look + 1;
            while *pos < bytes.len() && (bytes[*pos].is_ascii_digit() || bytes[*pos] == b'_') {
                *pos += 1;
            }
        }
    }

    if is_real {
        TokenKind::RealLit
    } else {
        TokenKind::IntegerLit
    }
}

fn operator_len(bytes: &[u8], pos: usize) -> Option<usize> {
    const TWO_BYTE: [&[u8; 2]; 7] = [b":=", b"=>", b"..", b"<=", b">=", b"<>", b"**"];
    for op in TWO_BYTE {
        if bytes.len() >= pos + 2 && &bytes[pos..pos + 2] == op {
            return Some(2);
        }
    }
    let byte = bytes[pos];
    if matches!(
        byte,
        b'=' | b'<'
            | b'>'
            | b'+'
            | b'-'
            | b'*'
            | b'/'
            | b'('
            | b')'
            | b'['
            | b']'
            | b','
            | b';'
            | b':'
            | b'.'
            | b'^'
            | b'#'
            | b'%'
            | b'&'
            | b'|'
    ) {
        Some(1)
    } else {
        None
    }
}

/// Coverage check for the token stream: any byte not owned by exactly one
/// token, or any token whose text is not its source slice, is reported.
#[derive(Debug, Default, Clone, PartialEq, Eq)]
pub struct Coverage {
    /// Ranges of source bytes no token covers.
    pub gaps: Vec<TextRange>,
    /// Tokens whose `text` does not equal their source slice.
    pub mismatches: Vec<TextRange>,
}

/// Computes [`Coverage`] for `source` and its tokens.
pub fn check_coverage(source: &str, tokens: &[Token<'_>]) -> Coverage {
    let mut coverage = Coverage::default();
    let mut cursor = 0u32;
    for token in tokens {
        let start = u32::from(token.range.start());
        let end = u32::from(token.range.end());
        if start != cursor {
            coverage.gaps.push(TextRange::new(
                TextSize::from(cursor),
                TextSize::from(start),
            ));
        }
        if token.text != &source[token.range] {
            coverage.mismatches.push(token.range);
        }
        cursor = end;
    }
    let len = u32::try_from(source.len()).unwrap_or(u32::MAX);
    if cursor != len {
        coverage
            .gaps
            .push(TextRange::new(TextSize::from(cursor), TextSize::from(len)));
    }
    coverage
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn lex_when_plain_pou_then_tiles_source() {
        let source = "PROGRAM P\nVAR\n\tx : INT := 1;\nEND_VAR\nx := x + 1;\nEND_PROGRAM";
        let (tokens, diagnostics) = lex(source);
        assert!(diagnostics.is_empty());
        let coverage = check_coverage(source, &tokens);
        assert_eq!(coverage.gaps, vec![]);
        assert_eq!(coverage.mismatches, vec![]);
    }

    #[test]
    fn lex_when_unmatched_byte_then_error_token_covers_it() {
        let source = "a := 1 ? 2;";
        let (_tokens, diagnostics) = lex(source);
        assert!(
            diagnostics.is_empty(),
            "error bytes are tokens, not diagnostics"
        );
        let (tokens, _) = lex(source);
        let coverage = check_coverage(source, &tokens);
        assert_eq!(coverage.gaps, vec![]);
        let error = tokens
            .iter()
            .find(|token| token.kind == TokenKind::Error)
            .expect("expected one error token");
        assert_eq!(error.text, "?");
        assert_eq!(
            error.range,
            TextRange::new(TextSize::from(7), TextSize::from(8))
        );
    }

    #[test]
    fn lex_when_crlf_and_tabs_then_preserved() {
        let source = "PROGRAM P\r\n\tVAR\r\n\tEND_VAR\r\nEND_PROGRAM\r\n";
        let (tokens, _) = lex(source);
        let coverage = check_coverage(source, &tokens);
        assert_eq!(coverage.gaps, vec![]);
        let newlines: Vec<&str> = tokens
            .iter()
            .filter(|token| token.kind == TokenKind::Newline)
            .map(|token| token.text)
            .collect();
        assert_eq!(newlines, vec!["\r\n", "\r\n", "\r\n", "\r\n"]);
    }

    #[test]
    fn lex_when_unicode_comment_and_string_then_bytes_preserved() {
        let source = "(* Wärmebild *)\ns := 'Wärmebild';";
        let (tokens, diagnostics) = lex(source);
        assert!(diagnostics.is_empty());
        let coverage = check_coverage(source, &tokens);
        assert_eq!(coverage.gaps, vec![]);
        assert_eq!(coverage.mismatches, vec![]);
        assert!(tokens
            .iter()
            .any(|token| token.kind == TokenKind::BlockComment && token.text.contains('ä')));
    }

    #[test]
    fn lex_when_duration_and_based_literals_then_single_tokens() {
        let (tokens, diagnostics) = lex("t := T#1m30s; b := 16#D012; r := 1.5E-3;");
        assert!(diagnostics.is_empty());
        let kinds: Vec<(TokenKind, &str)> = tokens
            .iter()
            .filter(|token| !token.kind.is_trivia())
            .map(|token| (token.kind, token.text))
            .collect();
        assert!(kinds.contains(&(TokenKind::Ident, "T")));
        assert!(kinds.contains(&(TokenKind::IntegerLit, "16#D012")));
        assert!(kinds.contains(&(TokenKind::RealLit, "1.5E-3")));
        assert!(kinds.contains(&(TokenKind::Ident, "m30s")));
    }
}
