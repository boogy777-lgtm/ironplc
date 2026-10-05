//! What the diagnostics say, legacy against new.
//!
//! The text of a diagnostic is not a contract of the compiler, but it is what
//! a person reads, so a change of wording is listed and counted, not made
//! silently. For every input both parsers reject with the same problem code,
//! the two messages are equal or are a row of `MESSAGE_DIFFERENCES`; the
//! codes that differ are `CODE_EXCEPTIONS` (`diagnostics.rs`).

use super::diagnostics::{legacy_reported, new_reported, whole_inputs};
use super::legacy::presets;
use super::{Class, Reason};

/// A difference between what the legacy parser says and what the new parser
/// says for one problem code, how often the corpus shows it, and why.
struct MessageDifference {
    code: &'static str,
    /// The exact legacy message, or any when `None`.
    legacy: Option<&'static str>,
    /// The exact new message, or any when `None`.
    new: Option<&'static str>,
    reason: Reason,
    expected: usize,
}

impl MessageDifference {
    fn covers(&self, code: &str, legacy: &str, new: &str) -> bool {
        self.code == code
            && self.legacy.is_none_or(|text| text == legacy)
            && self.new.is_none_or(|text| text == new)
    }
}

const PEG_EXPECTED_SET: Reason = Reason::new(Class::LegacyDefect, "legacy defect not ported: the legacy message is the set of tokens its ordered choice would have taken, spelled as the lexer's own patterns (`%[IQM]([XBWDL])?...`, whitespace and comment tokens) and the token it found; the new message says what the grammar needs at that place (`expected `;``, `the hour is out of range: 'TOD#25:00:00'`)");
const LEXER_SLICE: Reason = Reason::new(Class::AcceptedOnPurpose, "the legacy message quotes the text the lexer could not read, which runs to the end of the input for a string never closed, with a line and a column the label's own location repeats; the new message says what is wrong with the text");
const GATE_LABEL: Reason = Reason::new(Class::AcceptedOnPurpose, "the legacy label names the form (`Comment`, `identifier`) and leaves the dialect to the problem's description; the new label says the form is not enabled in this dialect, which is what a person has to change");
const ESCAPE_LABEL: Reason = Reason::new(Class::AcceptedOnPurpose, "the legacy label is the word `Escape`; the new label says which escape is not defined for character strings");

const fn gate(
    code: &'static str,
    legacy: &'static str,
    new: &'static str,
    expected: usize,
) -> MessageDifference {
    MessageDifference {
        code,
        legacy: Some(legacy),
        new: Some(new),
        reason: GATE_LABEL,
        expected,
    }
}

const MESSAGE_DIFFERENCES: &[MessageDifference] = &[
    MessageDifference {
        code: "P0002",
        legacy: None,
        new: None,
        reason: PEG_EXPECTED_SET,
        expected: 5021,
    },
    MessageDifference {
        code: "P0003",
        legacy: None,
        new: Some("unexpected character"),
        reason: LEXER_SLICE,
        expected: 192,
    },
    MessageDifference {
        code: "P0003",
        legacy: None,
        new: Some("unterminated string literal"),
        reason: LEXER_SLICE,
        expected: 30,
    },
    gate(
        "P0004",
        "Comment",
        "C-style comments are not enabled in this dialect",
        42,
    ),
    gate(
        "P0011",
        "Empty variable block",
        "empty variable blocks are not enabled in this dialect",
        46,
    ),
    MessageDifference {
        code: "P0012",
        legacy: Some("Escape"),
        new: Some("this `$` escape is not defined for character strings"),
        reason: ESCAPE_LABEL,
        expected: 66,
    },
    gate(
        "P4033",
        "partial-access selector",
        "partial-access syntax is not enabled in this dialect",
        24,
    ),
    gate(
        "P4042",
        "parenthesis length delimiter",
        "a string length in parentheses is not enabled in this dialect",
        54,
    ),
    gate(
        "P4067",
        "escaped identifier",
        "escaped identifiers are not enabled in this dialect",
        39,
    ),
    gate(
        "P4068",
        "identifier",
        "identifiers with letters outside ASCII are not enabled in this dialect",
        26,
    ),
    gate(
        "P4069",
        "identifier",
        "consecutive underscores in an identifier are not enabled in this dialect",
        12,
    ),
    gate(
        "P4070",
        "incomplete array bounds",
        "incomplete array bounds are not enabled in this dialect",
        39,
    ),
];

#[test]
fn messages_when_corpus_rejected_by_both_with_one_code_then_equal_or_a_listed_difference() {
    let mut observed = vec![0usize; MESSAGE_DIFFERENCES.len()];
    let (mut same, mut unlisted) = (0usize, Vec::new());
    for (key, text) in whole_inputs() {
        for preset in presets() {
            let (Some(legacy), Some(new)) = (
                legacy_reported(&text, &preset),
                new_reported(&text, &preset),
            ) else {
                continue;
            };
            if legacy.code != new.code {
                continue;
            }
            if legacy.message == new.message {
                same += 1;
                continue;
            }
            match MESSAGE_DIFFERENCES
                .iter()
                .position(|row| row.covers(&legacy.code, &legacy.message, &new.message))
            {
                Some(index) => observed[index] += 1,
                None => unlisted.push(format!(
                    "{key:?} under {}: {} legacy {:?} new {:?}",
                    preset.name, legacy.code, legacy.message, new.message
                )),
            }
        }
    }
    println!(
        "{same} messages equal, {} differ",
        observed.iter().sum::<usize>()
    );
    for (row, seen) in MESSAGE_DIFFERENCES.iter().zip(&observed) {
        println!(
            "  [{seen}] {} {:?} -> {:?}: {}",
            row.code, row.legacy, row.new, row.reason
        );
    }
    assert!(unlisted.is_empty(), "unlisted:\n{}", unlisted.join("\n"));
    let miscounted: Vec<String> = MESSAGE_DIFFERENCES
        .iter()
        .zip(&observed)
        .filter(|(row, seen)| row.expected != **seen)
        .map(|(row, seen)| {
            format!(
                "{} {:?}: recorded {}, observed {seen}",
                row.code, row.new, row.expected
            )
        })
        .collect();
    assert!(miscounted.is_empty(), "{}", miscounted.join("\n"));
    assert!(same >= 20, "only {same} equal messages");
}

#[test]
fn messages_when_every_row_then_it_has_a_reason() {
    for row in MESSAGE_DIFFERENCES {
        assert!(!row.reason.text.is_empty(), "{}", row.code);
    }
}
