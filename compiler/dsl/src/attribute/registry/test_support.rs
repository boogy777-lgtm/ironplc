//! What the tests of the registry share: attributes built by hand, the
//! spellings of a row, and a valid and invalid value for every value kind.
//! The matches over the value kind have no wildcard, so a kind added later does
//! not compile until its samples are here.

use super::{Info, ValueKind};
use crate::attribute::{Attribute, Attributes};
use crate::core::SourceSpan;

/// An attribute whose span is the one byte at `at`.
pub(super) fn attribute(name: &str, value: Option<&str>, at: usize) -> Attribute {
    Attribute {
        name: name.to_string(),
        value: value.map(str::to_string),
        span: SourceSpan::range(at, at + 1),
    }
}

pub(super) fn attributes(list: Vec<Attribute>) -> Attributes {
    Attributes::new(list)
}

/// Where a span starts and ends.
pub(super) fn place(span: &SourceSpan) -> (usize, usize) {
    (span.start, span.end)
}

/// The name of the row and its aliases.
pub(super) fn spellings(row: &Info) -> Vec<&'static str> {
    std::iter::once(row.name)
        .chain(row.aliases.iter().copied())
        .collect()
}

/// The text in lower case, in upper case, and with the case alternating.
pub(super) fn cases(text: &str) -> [String; 3] {
    let mixed = text
        .chars()
        .enumerate()
        .map(|(index, c)| {
            if index % 2 == 0 {
                c.to_ascii_uppercase()
            } else {
                c.to_ascii_lowercase()
            }
        })
        .collect();
    [text.to_ascii_lowercase(), text.to_ascii_uppercase(), mixed]
}

/// A value, as written, that a row of this kind reads.
pub(super) fn valid_value(kind: ValueKind) -> Option<&'static str> {
    match kind {
        ValueKind::Flag => None,
        ValueKind::OneOf(texts) | ValueKind::OptionalOneOf(texts) => texts.first().copied(),
        ValueKind::Number => Some("7"),
        ValueKind::Text => Some("a text"),
        ValueKind::OptionalText => Some("a text"),
        ValueKind::RuleList => Some("-33, -31"),
    }
}

/// The values, as written, that a row of this kind does not read.
pub(super) fn invalid_values(kind: ValueKind) -> &'static [Option<&'static str>] {
    match kind {
        ValueKind::Flag => &[Some("yes"), Some("")],
        ValueKind::OneOf(_) => &[None, Some(""), Some("not-a-choice")],
        ValueKind::OptionalOneOf(_) => &[Some(""), Some("not-a-choice")],
        ValueKind::Number => &[
            None,
            Some(""),
            Some("-1"),
            Some("+1"),
            Some("1.5"),
            Some("4294967296"),
            Some("seven"),
        ],
        ValueKind::Text => &[None, Some(""), Some("   ")],
        ValueKind::OptionalText => &[],
        ValueKind::RuleList => &[
            None,
            Some(""),
            Some("33"),
            Some("+33"),
            Some("-"),
            Some("-x"),
            Some("-33,"),
            Some("-33 -31"),
        ],
    }
}

/// Whether a row of this kind may have no value.
pub(super) fn allows_missing(kind: ValueKind) -> bool {
    match kind {
        ValueKind::Flag => true,
        ValueKind::OptionalOneOf(_) | ValueKind::OptionalText => true,
        ValueKind::OneOf(_) | ValueKind::Number | ValueKind::Text | ValueKind::RuleList => false,
    }
}
