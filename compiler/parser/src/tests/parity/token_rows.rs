//! The documented differences between the token view of the lossless tree and
//! the tokens of the legacy pipeline (`token_view.rs` holds the comparison):
//! where each is found, why it exists and how often the corpus shows it.
//!
//! A row is one kind of difference with its reason. The first row that fits a
//! difference explains it, so a row is as narrow as the reason it states.

use super::differences::{MARKER_IN_STRING, SEVERAL_PAIRS};
use super::{Class, Reason};
use crate::token::TokenType;

/// One token as the comparison reads it. Two tuples are equal when the type,
/// the bytes, the position and the text are; `region` only says that the token
/// of the tree is a region, which a row about regions needs to know.
#[derive(Debug, Clone)]
pub struct Tuple {
    pub token_type: TokenType,
    pub start: usize,
    pub end: usize,
    pub line: usize,
    pub col: usize,
    pub text: String,
    pub region: bool,
}

impl PartialEq for Tuple {
    fn eq(&self, other: &Self) -> bool {
        self.token_type == other.token_type
            && self.start == other.start
            && self.end == other.end
            && self.line == other.line
            && self.col == other.col
            && self.text == other.text
    }
}

/// A stretch where the two token sequences differ: the tokens each side has in
/// it, and the tokens of the tree just before it, nearest first.
pub struct Hunk {
    pub legacy: Vec<Tuple>,
    pub cst: Vec<Tuple>,
    pub before: Vec<Tuple>,
}

/// A diagnostic as the comparison reads it: its code and its byte range.
pub type Reported = (String, usize, usize);

/// The input a hunk or a diagnostic is found in: its name in the corpus, its
/// text, and where each side found errors and the tree found pragmas.
pub struct Input<'a> {
    pub key: &'a str,
    pub text: &'a str,
    /// The byte ranges of the diagnostics of the legacy pipeline.
    pub legacy_errors: &'a [(usize, usize)],
    /// The byte ranges of the diagnostics of the tree.
    pub errors: &'a [(usize, usize)],
    /// The byte ranges of the pragma tokens of the tree.
    pub pragmas: &'a [(usize, usize)],
}

/// A kind of difference between the two token sequences.
pub struct TokenException {
    pub name: &'static str,
    pub reason: Reason,
    /// True when the row explains `hunk`, found in `input`.
    pub covers: fn(&Hunk, &Input) -> bool,
    /// How many hunks of the corpus the row explains.
    pub expected: usize,
}

/// One difference between the two diagnostic sequences.
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Item {
    /// The legacy pipeline reports it and the tree does not.
    LegacyOnly(Reported),
    /// The tree reports it and the legacy pipeline does not.
    CstOnly(Reported),
    /// Both report the same code from the same byte, over different ranges.
    Resized { legacy: Reported, cst: Reported },
    /// Both report the same diagnostics, in another order.
    Order,
}

/// A kind of difference between the two diagnostic sequences.
pub struct DiagnosticException {
    pub name: &'static str,
    pub reason: Reason,
    pub covers: fn(&Item, &Input) -> bool,
    pub expected: usize,
}

const T1: Reason = Reason::new(Class::OwnerDecided, "T1: a duration literal is not split into its parts: the legacy pipeline cuts the identifier `m30s` of `T#1m30s` into `m`, `30` and `s` so that its grammar sees one number and one unit at a time; the tree keeps the lexeme and the lowering reads the units from it");
const T2: Reason = Reason::new(Class::OwnerDecided, "T2: no synthetic empty `;` tokens: the legacy pipeline inserts a zero-width `;` after a keyword statement that has none (`allow_missing_semicolon`); the tree keeps the text as written and the grammar accepts the missing `;`");
const T3: Reason = Reason::new(Class::OwnerDecided, "T3: a region the grammar does not read (the stretch between an OSCAT marker pair, an untaken `{IF}` branch) is one comment token covering its bytes; the legacy pipeline blanks the first or removes the second and leaves tokens for the blank");
const T4: Reason = Reason::new(Class::OwnerDecided, "T4: a lone carriage return is a line break; the legacy lexer rejects it outside a comment or a string and counts it as a column inside one");
const LABEL: Reason = Reason::new(Class::OwnerDecided, "a statement label is an identifier in the token view: the legacy pipeline re-types the name of a label (`name :` in statement position) as `Label` so that its grammar can tell it from a name; whether a name is a label is decided by the grammar of the tree, which tokenizing does not run");
const BRACES: Reason = Reason::new(Class::OwnerDecided, "owner decision (pragma content is not examined): in a dialect without pragmas the tree keeps a `{ ... }` as one pragma token and reports it; the legacy lexer reads the braces and the text between them as the tokens of code");
const PRAGMA_TEXT: Reason = Reason::new(Class::LegacyDefect, "the legacy pipeline joins the texts of the tokens its lexer could read to make the text of a collapsed pragma, so the bytes its lexer rejected are missing from the text, and from the columns after it on the line; the token of the view is the source slice");
const TIME_BESIDE_COMMENT: Reason = Reason::new(Class::LegacyDefect, "the legacy pipeline looks past spaces and line breaks only for the neighbour that makes `TIME` a name (`TIME (`, `TIME :=`, `FUNCTION TIME`), so a comment between the two leaves it a keyword and the call is a syntax error; the tree and its grammar look past every comment, as they do everywhere else");
const ERROR_TOKEN: Reason = Reason::new(Class::AcceptedOnPurpose, "a comment, pragma or string that is never closed is one error token of the tree, reported over all of it and not a token of the view; the legacy lexer has no token for it and reads the text inside as code");
const ERROR_COLUMN: Reason = Reason::new(Class::LegacyDefect, "the legacy lexer does not count the bytes of text it rejects when it advances the column, so every token after one on its line is positioned too far left; the view counts them");
const STRING_RECOVERY: Reason = Reason::new(Class::AcceptedOnPurpose, "an unterminated string is an error token up to the end of its line in the tree, so a stray quote does not swallow the rest of the file; the legacy lexer reports everything from the quote to the end of the input and makes no token of it");
const PRAGMA_GATE: Reason = Reason::new(Class::AcceptedOnPurpose, "a pragma in a dialect without pragmas is reported by the tokenizer of the tree, as a form the dialect has not enabled; the legacy pipeline reports it from its grammar, after tokenizing");
const PRAGMA_CONTENT: Reason = Reason::new(Class::OwnerDecided, "owner decision (pragma content is not examined): the legacy lexer reads the inside of a pragma and rejects a character it has no token for; the tree keeps the pragma as one token and does not look inside");
const LONE_CR: Reason = Reason::new(
    Class::OwnerDecided,
    "T4: a lone carriage return is a line break in the tree; the legacy lexer rejects it",
);
const WHOLE_TOKEN: Reason = Reason::new(Class::AcceptedOnPurpose, "a comment or pragma that is never closed is one error token of the tree and is reported over all of it by the tokenizer; the legacy lexer has no token for it and fails in its grammar, after tokenizing");
const STRING_END: Reason = Reason::new(Class::AcceptedOnPurpose, "an unterminated string is an error up to the end of its line in the tree; the legacy lexer reports everything from the quote to the end of the input");
const INTERRUPTED_FORM: Reason = Reason::new(Class::AcceptedOnPurpose, "text the lexer rejects between the tokens of a gated form (`ARRAY [ * ]`, `STRING ( 10 )`) interrupts the form in the tree, so the form is not reported and the rejected text is; the legacy token rules do not see the rejected text and report the form as well");
const SOURCE_ORDER: Reason = Reason::new(Class::AcceptedOnPurpose, "the tokenizer of the tree reports its diagnostics in source order; the legacy pipeline reports those of its lexer first, then those of its token rules rule by rule, then those of its conditional pragmas");

fn lone_cr(text: &str) -> bool {
    let bytes = text.as_bytes();
    bytes
        .iter()
        .enumerate()
        .any(|(at, byte)| *byte == b'\r' && bytes.get(at + 1) != Some(&b'\n'))
}

fn joined(tokens: &[Tuple]) -> String {
    tokens.iter().map(|t| t.text.as_str()).collect()
}

/// True when two tokens are the same token read from the same bytes: type,
/// bytes and text, whatever their position.
fn same_token(legacy: &Tuple, cst: &Tuple) -> bool {
    legacy.token_type == cst.token_type
        && legacy.start == cst.start
        && legacy.end == cst.end
        && legacy.text == cst.text
}

/// True when two tokens have the same bytes and the same text.
fn same_text_and_bytes(legacy: &Tuple, cst: &Tuple) -> bool {
    legacy.start == cst.start && legacy.end == cst.end && legacy.text == cst.text
}

fn same_but_position(hunk: &Hunk) -> bool {
    hunk.legacy.len() == hunk.cst.len()
        && hunk
            .legacy
            .iter()
            .zip(&hunk.cst)
            .all(|(l, c)| same_token(l, c))
}

/// True when the range `start..end` is inside one of `ranges`.
fn inside(ranges: &[(usize, usize)], start: usize, end: usize) -> bool {
    ranges.iter().any(|(from, to)| *from <= start && end <= *to)
}

/// Reads the hunk as the legacy tokens of the pragmas the tree found, and of
/// nothing else, differing from the view: a `{` ... `}` run that the tree keeps
/// as one pragma token (a dialect without pragmas), or a collapsed pragma whose
/// text lacks the bytes the legacy lexer rejected. Where the positions differ
/// they are not compared. `Some(true)` when a run was collapsed, `Some(false)`
/// when only the text of a pragma differs, `None` when the hunk is anything
/// else.
fn pragmas_agree(hunk: &Hunk, input: &Input) -> Option<bool> {
    let mut collapsed = 0;
    let mut text_differs = 0;
    let mut legacy = hunk.legacy.iter().peekable();
    let mut cst = hunk.cst.iter();
    while let Some(token) = legacy.next() {
        let expected = cst.next()?;
        if token.token_type == TokenType::LeftBrace
            && input.pragmas.contains(&(token.start, expected.end))
            && expected.token_type == TokenType::Pragma
        {
            // The run ends at the brace that ends the pragma.
            for inner in legacy.by_ref() {
                if inner.end == expected.end {
                    break;
                }
            }
            collapsed += 1;
            continue;
        }
        if !(token.token_type == expected.token_type
            && token.start == expected.start
            && token.end == expected.end)
        {
            return None;
        }
        if token.text != expected.text {
            if token.token_type != TokenType::Pragma {
                return None;
            }
            text_differs += 1;
        }
    }
    if cst.next().is_some() {
        return None;
    }
    match (collapsed, text_differs) {
        (0, 0) => None,
        (0, _) => Some(false),
        _ => Some(true),
    }
}

pub const TOKEN_EXCEPTIONS: &[TokenException] = &[
    TokenException {
        name: "duration lexeme not split",
        reason: T1,
        covers: |hunk, _| {
            let [token] = hunk.cst.as_slice() else {
                return false;
            };
            token.token_type == TokenType::Identifier
                && hunk.legacy.len() >= 2
                && hunk.legacy.first().is_some_and(|l| l.start == token.start)
                && hunk.legacy.last().is_some_and(|l| l.end == token.end)
                && joined(&hunk.legacy) == token.text
                && hunk
                    .before
                    .iter()
                    .take(3)
                    .any(|t| t.token_type == TokenType::Hash)
        },
        expected: 126,
    },
    TokenException {
        name: "no synthetic semicolon",
        reason: T2,
        covers: |hunk, _| {
            hunk.cst.is_empty()
                && matches!(
                    hunk.legacy.as_slice(),
                    [token] if token.token_type == TokenType::Semicolon
                        && token.start == token.end
                        && token.text.is_empty()
                )
        },
        expected: 380,
    },
    TokenException {
        name: "ranged comment is one comment",
        reason: T3,
        covers: |hunk, _| {
            let [token] = hunk.cst.as_slice() else {
                return false;
            };
            token.region
                && token.text.starts_with("(*@KEY@:")
                && hunk.legacy.first().is_some_and(|l| l.start == token.start)
                && hunk.legacy.last().is_some_and(|l| l.end == token.end)
        },
        expected: 54,
    },
    TokenException {
        name: "untaken branch is one comment",
        reason: T3,
        covers: |hunk, _| {
            matches!(hunk.cst.as_slice(), [token] if token.region && !token.text.starts_with("(*@KEY@:"))
                && hunk.legacy.is_empty()
        },
        expected: 66,
    },
    TokenException {
        name: "lone carriage return is a line break",
        reason: T4,
        covers: |hunk, input| {
            let kept: Vec<&Tuple> = hunk.cst.iter().filter(|t| t.text != "\r").collect();
            lone_cr(input.text)
                && kept.len() == hunk.legacy.len()
                && hunk.legacy.iter().zip(kept).all(|(l, c)| same_token(l, c))
        },
        expected: 24,
    },
    TokenException {
        name: "statement label is an identifier",
        reason: LABEL,
        covers: |hunk, _| {
            matches!(
                (hunk.legacy.as_slice(), hunk.cst.as_slice()),
                ([label], [name]) if label.token_type == TokenType::Label
                    && label.start == name.start
                    && label.end == name.end
                    && label.text == name.text
            )
        },
        expected: 183,
    },
    TokenException {
        name: "TIME is a name next to a comment",
        reason: TIME_BESIDE_COMMENT,
        covers: |hunk, _| {
            matches!(
                (hunk.legacy.as_slice(), hunk.cst.as_slice()),
                ([time], [name]) if time.token_type == TokenType::Time
                    && name.token_type == TokenType::Identifier
                    && same_text_and_bytes(time, name)
            )
        },
        expected: 4,
    },
    TokenException {
        name: "pragma of a dialect without pragmas is one token",
        reason: BRACES,
        covers: |hunk, input| pragmas_agree(hunk, input) == Some(true),
        expected: 302,
    },
    TokenException {
        name: "pragma text without the rejected bytes",
        reason: PRAGMA_TEXT,
        covers: |hunk, input| pragmas_agree(hunk, input) == Some(false),
        expected: 8,
    },
    TokenException {
        name: "unterminated construct is an error token",
        reason: ERROR_TOKEN,
        covers: |hunk, input| {
            hunk.cst.is_empty()
                && !hunk.legacy.is_empty()
                && hunk
                    .legacy
                    .iter()
                    .all(|token| inside(input.errors, token.start, token.end))
        },
        expected: 48,
    },
    TokenException {
        name: "tokens after an unterminated string",
        reason: STRING_RECOVERY,
        covers: |hunk, input| {
            hunk.legacy.is_empty()
                && !hunk.cst.is_empty()
                && hunk
                    .cst
                    .iter()
                    .all(|token| inside(input.legacy_errors, token.start, token.end))
        },
        expected: 6,
    },
    TokenException {
        name: "position after an error token",
        reason: ERROR_COLUMN,
        covers: |hunk, input| {
            same_but_position(hunk)
                && hunk
                    .cst
                    .first()
                    .is_some_and(|first| input.errors.iter().any(|(_, end)| *end <= first.start))
        },
        expected: 204,
    },
    TokenException {
        name: "marker text inside a string",
        reason: MARKER_IN_STRING,
        covers: |_, input| {
            input
                .key
                .starts_with("tests/fixtures/lexical/oscat_marker_in_string.st")
        },
        expected: 18,
    },
    TokenException {
        name: "several marker pairs",
        reason: SEVERAL_PAIRS,
        covers: |_, input| {
            input
                .key
                .starts_with("tests/fixtures/lexical/oscat_several_pairs.st")
        },
        expected: 18,
    },
];

fn is_pragma_range(input: &Input, diagnostic: &Reported) -> bool {
    input.pragmas.contains(&(diagnostic.1, diagnostic.2))
}

pub const DIAGNOSTIC_EXCEPTIONS: &[DiagnosticException] = &[
    DiagnosticException {
        name: "pragma of a dialect without pragmas",
        reason: PRAGMA_GATE,
        covers: |item, input| matches!(item, Item::CstOnly(d) if d.0 == "P0002" && is_pragma_range(input, d)),
        expected: 306,
    },
    DiagnosticException {
        name: "rejected character inside a pragma",
        reason: PRAGMA_CONTENT,
        covers: |item, input| matches!(item, Item::LegacyOnly(d) if d.0 == "P0003" && inside(input.pragmas, d.1, d.2)),
        expected: 12,
    },
    DiagnosticException {
        name: "lone carriage return",
        reason: LONE_CR,
        covers: |item, input| {
            matches!(item, Item::LegacyOnly(d) if d.0 == "P0003"
                && input.text.get(d.1..d.2) == Some("\r"))
        },
        expected: 30,
    },
    DiagnosticException {
        name: "unterminated block comment or pragma",
        reason: WHOLE_TOKEN,
        covers: |item, input| {
            matches!(item, Item::CstOnly(d) if d.0 == "P0002"
            && d.2 == input.text.len()
            && input.text.get(d.1..d.2).is_some_and(|text| {
                text.starts_with("(*") || text.starts_with("/*") || text.starts_with('{')
            }))
        },
        expected: 48,
    },
    DiagnosticException {
        name: "several marker pairs",
        reason: SEVERAL_PAIRS,
        covers: |_, input| {
            input
                .key
                .starts_with("tests/fixtures/lexical/oscat_several_pairs.st")
        },
        expected: 18,
    },
    DiagnosticException {
        name: "unterminated string ends at its line",
        reason: STRING_END,
        covers: |item, _| {
            matches!(item, Item::Resized { legacy, cst } if legacy.0 == "P0003"
                && cst.0 == "P0003"
                && cst.2 < legacy.2)
        },
        expected: 6,
    },
    DiagnosticException {
        name: "gated form interrupted by rejected text",
        reason: INTERRUPTED_FORM,
        covers: |item, input| {
            matches!(item, Item::LegacyOnly(d) if (d.0 == "P4070" || d.0 == "P4042")
                && !input.errors.is_empty())
        },
        expected: 5,
    },
    DiagnosticException {
        name: "diagnostics in source order",
        reason: SOURCE_ORDER,
        covers: |item, _| matches!(item, Item::Order),
        expected: 10,
    },
];
