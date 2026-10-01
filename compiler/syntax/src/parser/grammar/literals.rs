//! Literals assembled from the lexer's pieces.
//!
//! The lexer has no typed-literal tokens: `T#1m30s`, `INT#-5`, `16#FF` and
//! `BOOL#TRUE` are several adjacent tokens. A literal node here owns exactly
//! those tokens. The pieces of a literal must touch (a space in `T# 5s` is an
//! error), as in the legacy grammar, which matches them without skipping
//! trivia.
//!
//! One table, [`Class`], names every typed-literal family: the prefix words
//! that select it, the node it produces, and which value parser reads what
//! follows the `#`. Adding a family is adding its prefixes and arm there.

use super::temporal::{date_and_time_value, date_value, daytime_value, duration_value};
use crate::parser::event::CompletedMarker;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// The families of typed literal.
#[derive(Clone, Copy, PartialEq, Eq)]
enum Class {
    Int,
    Real,
    BitString,
    /// A boolean; the flag says whether `TRUE`/`FALSE` are accepted (they
    /// are after `BOOL#`, not after `BIT#`).
    Bool(bool),
    String {
        wide: bool,
    },
    Duration,
    TimeOfDay,
    Date,
    DateTime,
}

impl Class {
    fn node(self) -> K {
        match self {
            Class::Int => K::IntLiteral,
            Class::Real => K::RealLiteral,
            Class::BitString => K::BitStringLiteral,
            Class::Bool(_) => K::BoolLiteral,
            Class::String { .. } => K::StringLiteral,
            Class::Duration => K::DurationLiteral,
            Class::TimeOfDay => K::TimeOfDayLiteral,
            Class::Date => K::DateLiteral,
            Class::DateTime => K::DateTimeLiteral,
        }
    }
}

/// Prefixes that are keywords.
fn keyword_class(kind: K) -> Option<Class> {
    Some(match kind {
        K::Sint | K::Int | K::Dint | K::Lint | K::Usint | K::Uint | K::Udint | K::Ulint => {
            Class::Int
        }
        K::Real | K::Lreal => Class::Real,
        K::Byte | K::Word | K::Dword | K::Lword => Class::BitString,
        K::Bool => Class::Bool(true),
        K::Bit => Class::Bool(false),
        K::String => Class::String { wide: false },
        K::WString => Class::String { wide: true },
        K::Time | K::Ltime => Class::Duration,
        K::TimeOfDay | K::Ltod => Class::TimeOfDay,
        K::Date | K::Ldate => Class::Date,
        K::DateAndTime | K::Ldt => Class::DateTime,
        _ => return None,
    })
}

/// Prefixes that are ordinary names: the vendor abbreviations and the
/// Unicode string prefixes.
const WORD_PREFIXES: &[(&str, Class)] = &[
    ("T", Class::Duration),
    ("LT", Class::Duration),
    ("D", Class::Date),
    ("LD", Class::Date),
    ("UTF8", Class::String { wide: false }),
    ("UCHAR", Class::String { wide: false }),
    ("__XSTRING", Class::String { wide: true }),
];

/// The class of the typed literal that starts at the next token, if the next
/// two tokens are a prefix and a touching `#`.
fn typed_class(p: &Parser) -> Option<Class> {
    let kind = p.nth(0)?;
    if !(p.nth_at(1, K::Hash) && p.adjacent(0)) {
        return None;
    }
    if kind == K::Ident {
        return WORD_PREFIXES
            .iter()
            .find(|(word, _)| p.nth_is_word(0, word))
            .map(|(_, class)| *class);
    }
    keyword_class(kind).filter(|_| p.at(kind))
}

/// Consumes `kind` as the next piece of a literal, which must touch the
/// piece before it.
pub(super) fn piece(p: &mut Parser, kind: K, what: &str) -> bool {
    if !p.at(kind) {
        p.error(&format!("expected {what}"));
        return false;
    }
    if !p.touches_previous() {
        p.error("no whitespace is allowed inside a literal");
    }
    p.bump();
    true
}

/// Consumes one of `kinds` as the next piece of a literal.
fn piece_any(p: &mut Parser, kinds: &[K], what: &str) -> bool {
    match kinds.iter().find(|kind| p.at(**kind)) {
        Some(kind) => piece(p, *kind, what),
        None => {
            p.error(&format!("expected {what}"));
            false
        }
    }
}

/// An optional sign that touches the number after it.
fn sign(p: &mut Parser, numbers: &[K]) {
    if p.at_any(&[K::Plus, K::Minus]) {
        if p.adjacent(0) && numbers.iter().any(|kind| p.nth_at(1, *kind)) {
            p.bump();
        } else {
            p.error("a sign must touch the number after it");
            p.bump();
        }
    }
}

/// The literal that starts at the next token, or `None` (consuming nothing)
/// when the next token does not start one.
pub(super) fn literal(p: &mut Parser) -> Option<CompletedMarker> {
    if let Some(class) = typed_class(p) {
        return Some(typed(p, class));
    }
    let numeric_with_sign = p.at_any(&[K::Plus, K::Minus])
        && p.adjacent(0)
        && matches!(
            p.nth(1),
            Some(K::IntegerLit | K::FixedPointLit | K::FloatingPointLit)
        );
    let kind = p.nth(0)?;
    let node = match kind {
        K::True | K::False => K::BoolLiteral,
        K::StringLit | K::WStringLit => K::StringLiteral,
        K::FixedPointLit | K::FloatingPointLit => K::RealLiteral,
        K::IntegerLit | K::HexLit | K::OctLit | K::BinLit => K::IntLiteral,
        K::Plus | K::Minus if numeric_with_sign => {
            if matches!(p.nth(1), Some(K::IntegerLit)) {
                K::IntLiteral
            } else {
                K::RealLiteral
            }
        }
        _ => return None,
    };
    let marker = p.start();
    if numeric_with_sign {
        p.bump();
    }
    if !numeric_with_sign && p.at(K::IntegerLit) && is_based_decimal(p) {
        p.bump_n(2);
        piece(p, K::IntegerLit, "digits after `10#`");
    } else {
        p.bump();
    }
    Some(p.complete(marker, node))
}

/// `10#123`: the one based literal the lexer does not make a single token.
fn is_based_decimal(p: &Parser) -> bool {
    p.nth_text(0) == "10" && p.nth_at(1, K::Hash) && p.adjacent(0)
}

fn typed(p: &mut Parser, class: Class) -> CompletedMarker {
    let marker = p.start();
    p.bump_n(2);
    if !p.touches_previous() {
        p.error("no whitespace is allowed inside a literal");
    }
    match class {
        Class::Int => int_value(p),
        Class::Real => {
            sign(p, &[K::FixedPointLit, K::FloatingPointLit]);
            piece_any(p, &[K::FixedPointLit, K::FloatingPointLit], "a real value");
        }
        Class::BitString => {
            piece_any(
                p,
                &[K::BinLit, K::OctLit, K::HexLit, K::IntegerLit],
                "a bit-string value",
            );
        }
        Class::Bool(words) => bool_value(p, words),
        Class::String { wide } => {
            let (kind, what) = if wide {
                (K::WStringLit, "a double-quoted string")
            } else {
                (K::StringLit, "a single-quoted string")
            };
            piece(p, kind, what);
        }
        Class::Duration => duration_value(p),
        Class::TimeOfDay => daytime_value(p),
        Class::Date => date_value(p),
        Class::DateTime => date_and_time_value(p),
    }
    p.complete(marker, class.node())
}

fn int_value(p: &mut Parser) {
    if p.at_any(&[K::HexLit, K::OctLit, K::BinLit]) {
        p.bump();
    } else if p.at(K::IntegerLit) && is_based_decimal(p) {
        p.bump_n(2);
        piece(p, K::IntegerLit, "digits after `10#`");
    } else {
        sign(p, &[K::IntegerLit]);
        piece(p, K::IntegerLit, "an integer value");
    }
}

fn bool_value(p: &mut Parser, words: bool) {
    let is_bit_digit = p.at(K::IntegerLit) && matches!(p.nth_text(0), "0" | "1");
    if is_bit_digit || (words && p.at_any(&[K::True, K::False])) {
        p.bump();
    } else {
        p.error("expected 0, 1 or a boolean word");
    }
}
