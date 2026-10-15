//! The text of a duration value: `1m30s`, `1.5s`, `1h_30m`.
//!
//! The lexer cuts such a value into a number token and an identifier that
//! holds the units (`m30s`), and the tokens are kept as lexed. The parser
//! checks the joined text with [`validate`], and the lowering reads its parts
//! with [`parse`]; both go through the one scanner here, so what counts as a
//! number, a unit and a well-formed sequence of parts is decided once. The
//! ordering rule itself is `ironplc_dsl::construct::check_interval_parts`.

use ironplc_dsl::construct::{check_interval_parts, DurationUnit, IntervalError};
use ironplc_problems::{text, Text};

/// One `number unit` part: the text of its number, its unit and whether the
/// number has a non-zero fraction.
#[derive(Debug, PartialEq, Eq)]
pub(crate) struct Part {
    pub(crate) number: String,
    pub(crate) unit: DurationUnit,
    pub(crate) fractional: bool,
}

/// What a word that is not a unit is told: the units there are, from the one
/// table of them.
fn expected_unit() -> Text {
    text::syntax_error_expected_a_duration_unit(DurationUnit::expectation())
}

struct Scanner<'a> {
    chars: &'a [char],
    at: usize,
}

impl Scanner<'_> {
    fn peek(&self) -> Option<char> {
        self.chars.get(self.at).copied()
    }

    fn take_while(&mut self, accept: impl Fn(char) -> bool) -> String {
        let mut taken = String::new();
        while let Some(c) = self.peek().filter(|c| accept(*c)) {
            taken.push(c);
            self.at += 1;
        }
        taken
    }

    /// `digits [. digits]`: the text of the number and whether its fraction
    /// is non-zero.
    fn number(&mut self) -> Result<(String, bool), Text> {
        let mut text = self.take_while(|c| c.is_ascii_digit() || c == '_');
        if !text.chars().any(|c| c.is_ascii_digit()) {
            return Err(text::syntax_error_expected_a_number_in_the_duration());
        }
        if self.peek() != Some('.') {
            return Ok((text, false));
        }
        self.at += 1;
        let fraction = self.take_while(|c| c.is_ascii_digit() || c == '_');
        if !fraction.chars().any(|c| c.is_ascii_digit()) {
            return Err(
                text::syntax_error_expected_digits_after_the_decimal_point_in_the_duration(),
            );
        }
        text.push('.');
        text.push_str(&fraction);
        Ok((
            text,
            fraction.chars().any(|c| c.is_ascii_digit() && c != '0'),
        ))
    }

    fn part(&mut self) -> Result<Part, Text> {
        let (number, fractional) = self.number()?;
        let word = self.take_while(char::is_alphabetic);
        let unit = DurationUnit::from_word(&word).ok_or_else(expected_unit)?;
        if self.peek() == Some('_') {
            self.at += 1;
        }
        Ok(Part {
            number,
            unit,
            fractional,
        })
    }
}

/// Why the parts do not form a duration, as the text a diagnostic gives.
fn interval_text(error: IntervalError) -> Text {
    match error {
        IntervalError::UnitOrder => text::syntax_error_duration_units_must_be_in_descending_order(),
        IntervalError::FractionBeforeLast => {
            text::syntax_error_only_the_last_duration_part_may_have_a_fraction()
        }
        IntervalError::OutOfRange => {
            text::syntax_error_the_duration_is_outside_the_range_a_duration_can_represent()
        }
    }
}

/// The parts of a duration value, in order, when they form one: at least one
/// part, units in descending order and only the last part fractional.
pub(crate) fn parse(written: &str) -> Result<Vec<Part>, Text> {
    let chars: Vec<char> = written.chars().collect();
    let mut scanner = Scanner {
        chars: &chars,
        at: 0,
    };
    let mut parts = Vec::new();
    while scanner.peek().is_some() {
        parts.push(scanner.part()?);
    }
    if parts.is_empty() {
        return Err(text::syntax_error_expected_a_duration_value());
    }
    check_interval_parts(parts.iter().map(|part| (part.unit, part.fractional)))
        .map_err(interval_text)?;
    Ok(parts)
}

/// Checks the text of a duration value (`1m30s`, `1.5s`, `1h_30m`).
pub(crate) fn validate(written: &str) -> Result<(), Text> {
    parse(written).map(|_| ())
}

#[cfg(test)]
mod tests {
    use super::*;

    /// The parts, or the message of why the text is not a duration.
    fn why(written: &str) -> Result<Vec<Part>, String> {
        parse(written).map_err(|text| text.message().to_string())
    }

    #[test]
    fn validate_when_well_formed_then_ok() {
        for text in [
            "5s",
            "1m30s",
            "1.5s",
            "100ms",
            "1d2h3m4s5ms6us7ns",
            "1h_30m",
            "5S",
        ] {
            assert_eq!(validate(text), Ok(()), "{text}");
        }
    }

    #[test]
    fn validate_when_malformed_then_error() {
        for text in [
            "", "s", "5", "5x", "30s1m", "1s1s", "1.5m30s", "1.s", "5ms5ms",
        ] {
            assert!(validate(text).is_err(), "{text}");
        }
    }

    #[test]
    fn validate_when_zero_fraction_before_last_part_then_ok() {
        assert_eq!(validate("1.0m30s"), Ok(()));
    }

    #[test]
    fn parse_when_two_parts_then_number_text_unit_and_fraction_flag_of_each() {
        let parts = parse("1m30.5s");
        assert_eq!(
            parts,
            Ok(vec![
                Part {
                    number: "1".to_string(),
                    unit: DurationUnit::Minutes,
                    fractional: false,
                },
                Part {
                    number: "30.5".to_string(),
                    unit: DurationUnit::Seconds,
                    fractional: true,
                },
            ])
        );
    }

    #[test]
    fn parse_when_underscores_then_kept_in_the_number_text_and_skipped_between_parts() {
        let parts = parse("1_0h_30m").unwrap_or_default();
        let numbers: Vec<&str> = parts.iter().map(|part| part.number.as_str()).collect();
        assert_eq!(numbers, vec!["1_0", "30"]);
    }

    #[test]
    fn parse_when_unknown_unit_then_message_lists_the_valid_units() {
        let expected = format!("expected a {}", DurationUnit::expectation());
        assert_eq!(why("5x"), Err(expected));
    }

    #[test]
    fn parse_when_units_out_of_order_then_ordering_message() {
        assert_eq!(
            why("30s1m"),
            Err("duration units must be in descending order".to_string())
        );
        assert_eq!(
            why("1.5m30s"),
            Err("only the last duration part may have a fraction".to_string())
        );
    }
}
