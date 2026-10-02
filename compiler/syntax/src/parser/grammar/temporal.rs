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
