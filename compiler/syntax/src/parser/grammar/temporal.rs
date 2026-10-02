//! The value part of duration, time-of-day, date and date-and-time literals.
//!
//! The lexer cuts `T#1m30s` into `T`, `#`, `1` and the single identifier
//! `m30s`. The legacy pipeline rewrites that identifier into one token per
//! run of letters and digits before parsing. Here the tokens stay as lexed:
//! the parser consumes the adjacent pieces into the literal node and checks
//! their combined text, so the tree keeps the source tokens and no token is
//! split or rewritten.
//!
//! The rules are those of `ironplc_dsl::construct`, the same ones that build
//! the literal values: units in descending order with only the last part
//! fractional, and calendar and clock values in range. This module chooses
//! which source range each failure is reported on.

use super::literals::piece;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;
use ironplc_dsl::construct::{
    calendar_date, check_interval_parts, ClockField, DateField, DurationUnit, IntervalError,
};
use rowan::TextRange;

/// One `number unit` part: its unit and whether the number has a non-zero
/// fraction.
struct Part {
    unit: DurationUnit,
    fractional: bool,
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

    /// `digits [. digits]`; whether the fraction is non-zero.
    fn number(&mut self) -> Result<bool, &'static str> {
        let whole = self.take_while(|c| c.is_ascii_digit() || c == '_');
        if !whole.chars().any(|c| c.is_ascii_digit()) {
            return Err("expected a number in the duration");
        }
        if self.peek() != Some('.') {
            return Ok(false);
        }
        self.at += 1;
        let fraction = self.take_while(|c| c.is_ascii_digit() || c == '_');
        if !fraction.chars().any(|c| c.is_ascii_digit()) {
            return Err("expected digits after the decimal point in the duration");
        }
        Ok(fraction.chars().any(|c| c.is_ascii_digit() && c != '0'))
    }

    fn part(&mut self) -> Result<Part, &'static str> {
        let fractional = self.number()?;
        let word = self.take_while(char::is_alphabetic);
        let unit = DurationUnit::from_word(&word).ok_or("unknown duration unit")?;
        if self.peek() == Some('_') {
            self.at += 1;
        }
        Ok(Part { unit, fractional })
    }
}

/// Checks the text of a duration value (`1m30s`, `1.5s`, `1h_30m`).
fn validate_interval(text: &str) -> Result<(), &'static str> {
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
    check_interval_parts(parts.iter().map(|part| (part.unit, part.fractional))).map_err(|error| {
        match error {
            IntervalError::UnitOrder => "duration units must be in descending order",
            IntervalError::FractionBeforeLast => "only the last duration part may have a fraction",
        }
    })
}

/// The value of a duration literal: an optional `-` and the adjacent number
/// and unit tokens.
pub(super) fn duration_value(p: &mut Parser) {
    if p.at(K::Minus) {
        piece(p, K::Minus, "`-`");
    }
    let mut text = String::new();
    let mut range: Option<TextRange> = None;
    while matches!(
        p.nth(0),
        Some(K::IntegerLit | K::FixedPointLit | K::Ident | K::Period)
    ) && (range.is_none() || p.touches_previous())
    {
        let token = p.nth_range(0);
        text.push_str(p.nth_text(0));
        range = Some(range.map_or(token, |seen| seen.cover(token)));
        p.bump();
    }
    match range {
        None => p.error("expected a duration value"),
        Some(range) => {
            if let Err(message) = validate_interval(&text) {
                p.error_at(range, message);
            }
        }
    }
}

/// A number token of a clock or calendar value: its value and range.
fn number(p: &mut Parser, what: &str) -> Option<(u64, TextRange)> {
    let range = p.nth_range(0);
    let value = p.nth_number(0);
    piece(p, K::IntegerLit, what).then_some(())?;
    Some((value.unwrap_or(u64::MAX), range))
}

fn check_field(p: &mut Parser, value: Option<(u64, TextRange)>, field: ClockField, what: &str) {
    if let Some((value, range)) = value {
        if !field.accepts(u128::from(value)) {
            p.error_at(range, &format!("{what} is out of range"));
        }
    }
}

/// `hour : minute [: second]`, with the seconds optionally fractional.
pub(super) fn daytime_value(p: &mut Parser) {
    let hour = number(p, "an hour");
    check_field(p, hour, ClockField::Hour, "the hour");
    if !piece(p, K::Colon, "`:`") {
        return;
    }
    let minute = number(p, "a minute");
    check_field(p, minute, ClockField::Minute, "the minute");
    if !p.at(K::Colon) || minute.is_none() {
        return;
    }
    p.bump();
    let range = p.nth_range(0);
    let whole = p
        .nth_text(0)
        .split('.')
        .next()
        .and_then(|digits| digits.replace('_', "").parse::<u64>().ok());
    if p.at(K::FixedPointLit) {
        p.bump();
    } else if !piece(p, K::IntegerLit, "a second") {
        return;
    }
    check_field(
        p,
        whole.map(|value| (value, range)),
        ClockField::Second,
        "the second",
    );
}

/// `year - month - day`, a real calendar date.
pub(super) fn date_value(p: &mut Parser) {
    let year = number(p, "a year");
    if !piece(p, K::Minus, "`-`") {
        return;
    }
    let month = number(p, "a month");
    if !piece(p, K::Minus, "`-`") {
        return;
    }
    let day = number(p, "a day");
    if let (Some((year, year_range)), Some((month, _)), Some((day, day_range))) = (year, month, day)
    {
        match calendar_date(year.into(), month.into(), day.into()) {
            Ok(_) => {}
            Err(DateField::Year) => p.error_at(year_range, "the year is out of range"),
            Err(DateField::Month) => {
                p.error_at(year_range.cover(day_range), "the month is out of range")
            }
            Err(DateField::Day) => p.error_at(day_range, "the day is out of range for the month"),
        }
    }
}

/// A date and a time of day joined by `-`.
pub(super) fn date_and_time_value(p: &mut Parser) {
    date_value(p);
    if piece(p, K::Minus, "`-`") {
        daytime_value(p);
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn validate_interval_when_well_formed_then_ok() {
        for text in [
            "5s",
            "1m30s",
            "1.5s",
            "100ms",
            "1d2h3m4s5ms6us7ns",
            "1h_30m",
            "5S",
        ] {
            assert_eq!(validate_interval(text), Ok(()), "{text}");
        }
    }

    #[test]
    fn validate_interval_when_malformed_then_error() {
        for text in [
            "", "s", "5", "5x", "30s1m", "1s1s", "1.5m30s", "1.s", "5ms5ms",
        ] {
            assert!(validate_interval(text).is_err(), "{text}");
        }
    }

    #[test]
    fn validate_interval_when_zero_fraction_before_last_part_then_ok() {
        assert_eq!(validate_interval("1.0m30s"), Ok(()));
    }
}
