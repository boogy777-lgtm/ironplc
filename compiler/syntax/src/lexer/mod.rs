//! Lossless lexer over the original source text.
//!
//! [`lex`] runs on the text exactly as written (no preprocessing) and returns
//! tokens that tile the source: every byte belongs to exactly one token, in
//! order, and the concatenation of the token texts is the source (design:
//! parse-tree architecture, section 3.1). Trivia is kept as tokens. Bytes that
//! match no rule, and unterminated comments, strings, pragmas and quoted
//! names, become [`SyntaxKind::ErrorToken`]s instead of being dropped; each
//! error token is accompanied by exactly one [`SyntaxError`].
//!
//! [`lex_regions`] is the same lexer followed by the region pass: text the
//! grammar must not see (OSCAT ranged comments, untaken conditional-pragma
//! branches) becomes a single trivia token, still byte-exact.
//!
//! Keywords are lexed uniformly and case-insensitively; dialect-dependent
//! reservation is the parser's concern.

mod cursor;
pub(crate) mod escapes;
mod literals;
mod regions;
mod trivia;

use crate::error::{ErrorKind, SyntaxError};
use crate::syntax_kind::SyntaxKind;
use cursor::Cursor;
use literals::{scan_number, scan_percent, scan_quoted, Quoted};
use rowan::{TextRange, TextSize};
use trivia::{scan_delimited, scan_line_comment, scan_newline, scan_whitespace};

pub use regions::lex_regions;

/// A token borrowing its exact source slice.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Token<'src> {
    pub kind: SyntaxKind,
    pub text: &'src str,
    pub range: TextRange,
}

/// What one scanner consumed: the token kind and, when the token is an error
/// token, the kind and message of its syntax error.
type Scan = (SyntaxKind, Option<(ErrorKind, &'static str)>);

/// An error token for a malformed construct.
fn error(message: &'static str) -> Scan {
    (SyntaxKind::ErrorToken, Some((ErrorKind::Syntax, message)))
}

/// An error token for bytes that match no token.
fn unexpected_character() -> Scan {
    (
        SyntaxKind::ErrorToken,
        Some((ErrorKind::UnexpectedCharacter, "unexpected character")),
    )
}

const SINGLE_QUOTED: Quoted = Quoted {
    quote: b'\'',
    kind: SyntaxKind::StringLit,
    escapes: true,
    single_line: false,
    unterminated: "unterminated string literal",
};

const DOUBLE_QUOTED: Quoted = Quoted {
    quote: b'"',
    kind: SyntaxKind::WStringLit,
    escapes: true,
    single_line: false,
    unterminated: "unterminated string literal",
};

const BACKTICK_QUOTED: Quoted = Quoted {
    quote: b'`',
    kind: SyntaxKind::EscapedIdent,
    escapes: false,
    single_line: true,
    unterminated: "unterminated escaped identifier",
};

/// The lexer settings that change which bytes a token covers.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct LexOptions {
    /// A `(* *)` comment ends at the `*)` that matches its own `(*`, so an
    /// inner `(*` needs its own `*)`. Without it the comment ends at the first
    /// `*)`. A `/* */` comment never nests.
    pub nested_comments: bool,
}

impl Default for LexOptions {
    fn default() -> Self {
        LexOptions {
            nested_comments: true,
        }
    }
}

/// Tokenizes `source` completely: the returned tokens tile `[0, len)`. Block
/// comments nest, which is the reading that never splits a comment.
pub fn lex(source: &str) -> (Vec<Token<'_>>, Vec<SyntaxError>) {
    lex_with(source, LexOptions::default())
}

/// Like [`lex`], with the dialect's lexical settings.
pub fn lex_with(source: &str, options: LexOptions) -> (Vec<Token<'_>>, Vec<SyntaxError>) {
    let mut cursor = Cursor::new(source);
    let mut tokens = Vec::new();
    let mut errors = Vec::new();

    while !cursor.is_at_end() {
        let (token, error) = next_token(&mut cursor, options);
        errors.extend(error);
        tokens.push(token);
    }

    (tokens, errors)
}

/// Scans the token at the cursor and advances past it. The error is the one
/// that goes with an error token.
fn next_token<'src>(
    cursor: &mut Cursor<'src>,
    options: LexOptions,
) -> (Token<'src>, Option<SyntaxError>) {
    let start = cursor.pos();
    let (mut kind, mut message) = scan_token(cursor, options);
    if cursor.pos() == start {
        // Unreachable by construction; guarantees progress regardless.
        cursor.bump_char();
        (kind, message) = unexpected_character();
    }
    let range = range_of(start, cursor.pos());
    let error =
        message.map(|(error_kind, message)| SyntaxError::new(message, range).with_kind(error_kind));
    let token = Token {
        kind,
        text: cursor.slice_from(start),
        range,
    };
    (token, error)
}

fn range_of(start: usize, end: usize) -> TextRange {
    let to_size = |offset: usize| TextSize::from(u32::try_from(offset).unwrap_or(u32::MAX));
    TextRange::new(to_size(start), to_size(end))
}

/// Scans exactly one token at the cursor.
fn scan_token(cursor: &mut Cursor<'_>, options: LexOptions) -> Scan {
    let Some(byte) = cursor.peek() else {
        return error("unexpected end of input");
    };
    match byte {
        b' ' | b'\t' => scan_whitespace(cursor),
        b'\r' | b'\n' | 0x0c => scan_newline(cursor),
        b'/' if cursor.peek_at(1) == Some(b'/') => scan_line_comment(cursor),
        b'/' if cursor.peek_at(1) == Some(b'*') => scan_delimited(
            cursor,
            SyntaxKind::BlockComment,
            ("/*", "*/"),
            false,
            "unterminated block comment",
        ),
        b'(' if cursor.peek_at(1) == Some(b'*') => scan_delimited(
            cursor,
            SyntaxKind::BlockComment,
            ("(*", "*)"),
            options.nested_comments,
            "unterminated block comment",
        ),
        b'{' => scan_delimited(
            cursor,
            SyntaxKind::Pragma,
            ("{", "}"),
            false,
            "unterminated pragma",
        ),
        b'\'' => scan_quoted(cursor, &SINGLE_QUOTED),
        b'"' => scan_quoted(cursor, &DOUBLE_QUOTED),
        b'`' => scan_quoted(cursor, &BACKTICK_QUOTED),
        b'0'..=b'9' => scan_number(cursor),
        b'%' => scan_percent(cursor),
        _ => scan_name_or_punctuation(cursor, byte),
    }
}

fn scan_name_or_punctuation(cursor: &mut Cursor<'_>, byte: u8) -> Scan {
    if byte.is_ascii_alphabetic() || byte == b'_' || byte >= 0x80 {
        return scan_identifier(cursor);
    }
    match punctuation(cursor) {
        Some((kind, len)) => {
            cursor.bump_n(len);
            (kind, None)
        }
        None => {
            cursor.bump();
            unexpected_character()
        }
    }
}

/// An identifier or keyword: `[\p{L}_][\p{L}\p{Nd}_]*`, approximated with the
/// standard library's Unicode `alphabetic` / `numeric` classes. A character
/// that cannot start an identifier becomes a one-character error token.
fn scan_identifier(cursor: &mut Cursor<'_>) -> Scan {
    let start = cursor.pos();
    let starts_name = cursor
        .peek_char()
        .is_some_and(|c| c.is_alphabetic() || c == '_');
    if !starts_name {
        cursor.bump_char();
        return unexpected_character();
    }
    while let Some(c) = cursor.peek_char() {
        if c.is_alphabetic() || c.is_numeric() || c == '_' {
            cursor.bump_char();
        } else {
            break;
        }
    }
    let kind = SyntaxKind::keyword_from_str(cursor.slice_from(start)).unwrap_or(SyntaxKind::Ident);
    (kind, None)
}

/// Operators and punctuation, longest match first: (kind, length in bytes).
fn punctuation(cursor: &Cursor<'_>) -> Option<(SyntaxKind, usize)> {
    const TWO_BYTE: [(&str, SyntaxKind); 7] = [
        ("..", SyntaxKind::Range),
        ("<>", SyntaxKind::NotEqual),
        ("<=", SyntaxKind::LessEqual),
        (">=", SyntaxKind::GreaterEqual),
        ("**", SyntaxKind::Power),
        (":=", SyntaxKind::Assignment),
        ("=>", SyntaxKind::RightArrow),
    ];
    const ONE_BYTE: [(&str, SyntaxKind); 20] = [
        ("(", SyntaxKind::LeftParen),
        (")", SyntaxKind::RightParen),
        ("}", SyntaxKind::RightBrace),
        ("[", SyntaxKind::LeftBracket),
        ("]", SyntaxKind::RightBracket),
        (",", SyntaxKind::Comma),
        (";", SyntaxKind::Semicolon),
        (":", SyntaxKind::Colon),
        (".", SyntaxKind::Period),
        ("#", SyntaxKind::Hash),
        ("=", SyntaxKind::Equal),
        ("<", SyntaxKind::Less),
        (">", SyntaxKind::Greater),
        ("/", SyntaxKind::Div),
        ("*", SyntaxKind::Star),
        ("+", SyntaxKind::Plus),
        ("-", SyntaxKind::Minus),
        ("^", SyntaxKind::Caret),
        // `|` and `&` are the symbol spellings of OR and AND.
        ("|", SyntaxKind::Or),
        ("&", SyntaxKind::And),
    ];
    TWO_BYTE
        .iter()
        .chain(ONE_BYTE.iter())
        .find(|(text, _)| cursor.starts_with(text))
        .map(|(text, kind)| (*kind, text.len()))
}

/// Where a token stream fails to tile its source.
#[derive(Debug, Default, Clone, PartialEq, Eq)]
pub struct Coverage {
    /// Ranges of source bytes no token covers, or that tokens overlap.
    pub gaps: Vec<TextRange>,
    /// Tokens whose `text` does not equal their source slice.
    pub mismatches: Vec<TextRange>,
}

impl Coverage {
    /// True when the tokens tile the source exactly.
    pub fn is_exact(&self) -> bool {
        self.gaps.is_empty() && self.mismatches.is_empty()
    }
}

/// Checks that `tokens` tile `source` with no gap, no overlap, and with each
/// token's text equal to the slice its range names.
pub fn check_coverage(source: &str, tokens: &[Token<'_>]) -> Coverage {
    let mut coverage = Coverage::default();
    let mut cursor = TextSize::from(0);
    for token in tokens {
        if token.range.start() != cursor {
            coverage.gaps.push(TextRange::new(
                cursor.min(token.range.start()),
                cursor.max(token.range.start()),
            ));
        }
        let slice = source
            .get(usize::from(token.range.start())..usize::from(token.range.end()))
            .unwrap_or_default();
        if token.text != slice {
            coverage.mismatches.push(token.range);
        }
        cursor = token.range.end();
    }
    let len = range_of(source.len(), source.len()).end();
    if cursor != len {
        coverage
            .gaps
            .push(TextRange::new(cursor.min(len), cursor.max(len)));
    }
    coverage
}
