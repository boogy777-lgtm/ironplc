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
use ironplc_dsl::construct::{calendar_date, ClockField, DateField};
use rowan::TextRange;

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
            if let Err(message) = crate::interval_text::validate(&text) {
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
    // The colon before the seconds belongs to the literal only when it touches
    // the minute: a gap ends the literal at the minute, and what follows it is
    // not a part of it (no whitespace is allowed inside a literal).
    if !p.at(K::Colon) || !p.touches_previous() || minute.is_none() {
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
    use crate::parse_expression;
    use crate::parser::options::ParseOptions;
    use ironplc_dsl::construct::DurationUnit;

    fn messages(source: &str) -> Vec<String> {
        parse_expression(source, &ParseOptions::all())
            .errors
            .into_iter()
            .map(|error| error.message)
            .collect()
    }

    #[test]
    fn parse_expression_when_part_of_a_temporal_literal_is_wrong_then_the_message_names_the_literal(
    ) {
        for (source, message) in [
            ("TOD#25:00:00", "the hour is out of range: 'TOD#25:00:00'"),
            ("TOD#10:61:00", "the minute is out of range: 'TOD#10:61:00'"),
            ("D#2020-13-01", "the month is out of range: 'D#2020-13-01'"),
            (
                "T#5s1m",
                "duration units must be in descending order: 'T#5s1m'",
            ),
            (
                "DT#2020-01-01-25:00:00",
                "the hour is out of range: 'DT#2020-01-01-25:00:00'",
            ),
        ] {
            assert_eq!(messages(source), vec![message.to_string()], "{source}");
        }
    }

    #[test]
    fn parse_expression_when_duration_unit_unknown_then_the_units_come_from_the_one_table() {
        let found = messages("T#5x");
        let expected = format!("expected a {}: 'T#5x'", DurationUnit::expectation());
        assert_eq!(found, vec![expected]);
        for (spelling, _) in DurationUnit::UNITS {
            assert!(found[0].contains(spelling), "{spelling}");
        }
    }

    #[test]
    fn parse_statements_when_colon_before_the_seconds_is_separated_by_a_gap_then_the_literal_ends_at_the_minute(
    ) {
        use crate::parse_statements;
        let options = ParseOptions::all();
        // The gap ends the literal, so the rest is not part of it.
        let split = parse_statements("x := TOD#14:30 :20;", &options);
        assert!(!split.errors.is_empty());
        let seconds = parse_statements("x := TOD#14:30:20;", &options);
        assert!(seconds.errors.is_empty(), "{:?}", seconds.errors);
    }

    #[test]
    fn parse_expression_when_literal_is_not_temporal_then_the_message_does_not_quote_it() {
        assert_eq!(
            messages("INT# 5"),
            vec!["no whitespace is allowed inside a literal".to_string()]
        );
    }
}
