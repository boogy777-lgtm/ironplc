//! The text of a duration value: `1m30s`, `1.5s`, `1h_30m`.
//!
//! The lexer cuts such a value into a number token and an identifier that
//! holds the units (`m30s`), and the tokens are kept as lexed. The parser
//! checks the joined text with [`validate`], and the lowering reads its parts
//! with [`parse`]; both go through the one scanner here, so what counts as a
//! number, a unit and a well-formed sequence of parts is decided once. The
//! ordering rule itself is `ironplc_dsl::construct::check_interval_parts`.

use ironplc_dsl::construct::{check_interval_parts, DurationUnit, IntervalError};

/// One `number unit` part: the text of its number, its unit and whether the
/// number has a non-zero fraction.
#[derive(Debug, PartialEq, Eq)]
pub(crate) struct Part {
    pub(crate) number: String,
    pub(crate) unit: DurationUnit,
    pub(crate) fractional: bool,
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
    fn number(&mut self) -> Result<(String, bool), &'static str> {
        let mut text = self.take_while(|c| c.is_ascii_digit() || c == '_');
        if !text.chars().any(|c| c.is_ascii_digit()) {
            return Err("expected a number in the duration");
        }
        if self.peek() != Some('.') {
            return Ok((text, false));
        }
        self.at += 1;
        let fraction = self.take_while(|c| c.is_ascii_digit() || c == '_');
        if !fraction.chars().any(|c| c.is_ascii_digit()) {
            return Err("expected digits after the decimal point in the duration");
        }
        text.push('.');
        text.push_str(&fraction);
        Ok((
            text,
            fraction.chars().any(|c| c.is_ascii_digit() && c != '0'),
        ))
    }

    fn part(&mut self) -> Result<Part, &'static str> {
        let (number, fractional) = self.number()?;
        let word = self.take_while(char::is_alphabetic);
        let unit = DurationUnit::from_word(&word).ok_or(DurationUnit::expectation())?;
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

/// Why the parts do not form a duration, as the message a diagnostic gives.
fn interval_message(error: IntervalError) -> &'static str {
    match error {
        IntervalError::UnitOrder => "duration units must be in descending order",
        IntervalError::FractionBeforeLast => "only the last duration part may have a fraction",
    }
}

/// The parts of a duration value, in order, when they form one: at least one
/// part, units in descending order and only the last part fractional.
pub(crate) fn parse(text: &str) -> Result<Vec<Part>, &'static str> {
    let chars: Vec<char> = text.chars().collect();
    let mut scanner = Scanner {
        chars: &chars,
        at: 0,
    };
    let mut parts = Vec::new();
    while scanner.peek().is_some() {
        parts.push(scanner.part()?);
    }
    if parts.is_empty() {
        return Err("expected a duration value");
    }
    check_interval_parts(parts.iter().map(|part| (part.unit, part.fractional)))
        .map_err(interval_message)?;
    Ok(parts)
}

/// Checks the text of a duration value (`1m30s`, `1.5s`, `1h_30m`).
pub(crate) fn validate(text: &str) -> Result<(), &'static str> {
    parse(text).map(|_| ())
}

#[cfg(test)]
mod tests {
    use super::*;

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
        assert_eq!(parse("5x"), Err(DurationUnit::expectation()));
    }

    #[test]
    fn parse_when_units_out_of_order_then_ordering_message() {
        assert_eq!(
            parse("30s1m"),
            Err("duration units must be in descending order")
        );
        assert_eq!(
            parse("1.5m30s"),
            Err("only the last duration part may have a fraction")
        );
    }
}
