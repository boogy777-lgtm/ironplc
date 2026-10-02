use std::fmt;
use time::{
    convert::{Day, Hour, Minute, Nanosecond, Second},
    Date, Duration, PrimitiveDateTime, Time,
};

use crate::{
    common::{ElementaryTypeName, FixedPoint},
    construct::DurationUnit,
    core::SourceSpan,
    diagnostic::Diagnostic,
};
use ironplc_problems::Problem;

const NANOSECOND_PER_SECOND: i128 = Nanosecond::per(Second) as i128;

/// A duration that no duration value can represent: its length does not fit
/// the count of seconds and nanoseconds a [`Duration`] holds.
///
/// This is the one failure of building a duration from a number and a unit.
/// It says only that the value cannot be built at all; whether a value that
/// can be built fits `TIME` or `LTIME` is the range rule's question, and the
/// two report through [`DurationOutOfRange::diagnostic`] and
/// [`StoredCount`] so that they cannot disagree on what is out of range.
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
pub struct DurationOutOfRange;

impl DurationOutOfRange {
    /// The diagnostic for the literal `text` of the type `width` names: the
    /// one the range rule gives a literal its type cannot hold, because this
    /// is a literal no type can hold.
    pub fn diagnostic(self, span: SourceSpan, text: &str, width: TemporalWidth) -> Diagnostic {
        Diagnostic::literal_out_of_range(
            Problem::DurationLiteralOutOfRange,
            span,
            text,
            &duration_type_name(width).to_string(),
        )
    }
}

fn duration_type_name(width: TemporalWidth) -> ElementaryTypeName {
    match width {
        TemporalWidth::Short => ElementaryTypeName::TIME,
        TemporalWidth::Long => ElementaryTypeName::LTIME,
    }
}

/// The count a temporal literal holds, together with the storage its own type
/// gives that count.
///
/// A temporal value is an integer count in a fixed unit — milliseconds for a
/// duration or a time of day, seconds since 1970-01-01 for a date or a
/// date-and-time — and the literal's type decides how many bits hold it and
/// whether they are signed. Answering all three together is what lets one
/// range check serve every family: the caller asks whether `count` fits
/// `bits` of the stated signedness and needs to know nothing else about dates
/// or durations.
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
pub struct StoredCount {
    /// The count, in the unit the type stores.
    ///
    /// Wider than any storage so that a value the storage cannot hold arrives
    /// intact to be judged, rather than having been truncated on the way.
    pub count: i128,
    /// How many bits hold it: 32 for the short member, 64 for the long one.
    pub bits: u32,
    /// Whether those bits are signed. A duration is signed because it can be
    /// negative (ADR-0021); the calendar types are unsigned counts from the
    /// epoch (ADR-0025).
    pub signed: bool,
}

impl TemporalWidth {
    /// How many bits this width holds.
    pub fn bits(&self) -> u32 {
        match self {
            TemporalWidth::Short => 32,
            TemporalWidth::Long => 64,
        }
    }
}

/// Which member of a temporal family a literal names: the 32-bit type or the
/// 64-bit one.
///
/// IEC 61131-3 pairs each temporal type with a wider one -- `TIME` with
/// `LTIME`, `DATE` with `LDATE`, `TIME_OF_DAY` with `LTIME_OF_DAY`,
/// `DATE_AND_TIME` with `LDATE_AND_TIME` -- and a literal's prefix says which
/// one it is: `T#1h` is a `TIME` and `LTIME#1h` an `LTIME`.
///
/// The width belongs on the literal and not only on the declaration it
/// initializes, for the reason [`CharacterStringLiteral::width`] gives: a
/// literal also appears in statement bodies, where there is no declaration to
/// borrow it from. Without it every temporal literal resolved to the 32-bit
/// type, which held a 64-bit literal to a 32-bit range (issue #1560).
///
/// [`CharacterStringLiteral::width`]: crate::common::CharacterStringLiteral::width
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
pub enum TemporalWidth {
    /// The 32-bit member: `TIME`, `DATE`, `TIME_OF_DAY`, `DATE_AND_TIME`.
    Short,
    /// The 64-bit member: `LTIME`, `LDATE`, `LTIME_OF_DAY`, `LDATE_AND_TIME`.
    Long,
}

impl FixedPoint {
    /// The fraction as a whole number of nanoseconds, when the number counts
    /// seconds, as the seconds field of a time of day does. A fraction finer
    /// than a nanosecond is truncated.
    ///
    /// ```rust
    /// use ironplc_dsl::common::FixedPoint;
    /// assert_eq!(FixedPoint::parse("1.25").unwrap().nanoseconds(), 250_000_000);
    /// assert_eq!(FixedPoint::parse("0.0000000019").unwrap().nanoseconds(), 1);
    /// ```
    pub fn nanoseconds(&self) -> u32 {
        // `femptos` is below 10^15, so the quotient is below 10^9.
        (self.femptos / 1_000_000) as u32
    }
}

// See section 2.2.2
#[derive(Debug, PartialEq, Clone)]
pub struct DurationLiteral {
    pub span: SourceSpan,
    pub interval: Duration,
    /// The width the source spelled, which is what selects the prefix:
    /// `TIME#`/`T#` for the 32-bit type, `LTIME#` for the 64-bit one.
    pub width: TemporalWidth,
}

impl DurationLiteral {
    /// Creates a literal spanning `span` and measuring `interval`.
    ///
    /// Every constructor funnels through here so that what a duration literal
    /// is made of is stated once. The width defaults to the 32-bit member of
    /// the family, as [`CharacterStringLiteral::new`] defaults to `STRING`;
    /// a caller that knows better says so with
    /// [`with_width`](Self::with_width).
    ///
    /// [`CharacterStringLiteral::new`]: crate::common::CharacterStringLiteral::new
    pub fn new(span: SourceSpan, interval: Duration) -> Self {
        Self {
            span,
            interval,
            width: TemporalWidth::Short,
        }
    }

    /// Returns the literal with `width` recorded as the member of the family
    /// its prefix named.
    pub fn with_width(mut self, width: TemporalWidth) -> Self {
        self.width = width;
        self
    }

    /// The IEC 61131-3 type this literal is: the duration type its prefix named.
    ///
    /// A literal states its own type, so it is checked against that type's
    /// range wherever it is written, the way a prefixed integer literal is
    /// (`INT#40000` is not an `INT` whatever it is stored into).
    pub fn type_name(&self) -> ElementaryTypeName {
        duration_type_name(self.width)
    }

    /// The millisecond count this literal holds and the storage its type gives
    /// it.
    ///
    /// A duration is signed: subtracting a later time from an earlier one
    /// gives a negative result (ADR-0021).
    pub fn stored_count(&self) -> StoredCount {
        StoredCount {
            count: self.interval.whole_milliseconds(),
            bits: self.width.bits(),
            signed: true,
        }
    }

    /// Creates a literal of `value` counted in `unit`, or says that no
    /// duration can be that long.
    ///
    /// This is the one point where a number and a unit become a duration, so
    /// it is the one point that checks the result fits. The whole part and
    /// the fraction are scaled into nanoseconds in `i128`, which holds the
    /// largest the grammar can write (`u64::MAX` days is about 1.6e33) with
    /// room to spare, and only the total is narrowed to the seconds and
    /// nanoseconds a [`Duration`] stores.
    ///
    /// A fraction finer than a nanosecond is truncated (ADR-0021).
    ///
    /// ```rust
    /// use ironplc_dsl::common::FixedPoint;
    /// use ironplc_dsl::construct::DurationUnit;
    /// use ironplc_dsl::time::DurationLiteral;
    /// use time::Duration;
    /// let hours = |text| DurationLiteral::from_unit(FixedPoint::parse(text).unwrap(), DurationUnit::Hours);
    /// assert_eq!(hours("1.5").unwrap().interval, Duration::minutes(90));
    /// assert!(hours("18446744073709551615").is_err());
    /// ```
    pub fn from_unit(value: FixedPoint, unit: DurationUnit) -> Result<Self, DurationOutOfRange> {
        let scale = i128::from(unit.nanoseconds());
        // `femptos` counts 10^-15 of the unit, so its nanoseconds are
        // `femptos * scale / 10^15`.
        let whole = i128::from(value.whole)
            .checked_mul(scale)
            .ok_or(DurationOutOfRange)?;
        let fraction = i128::from(value.femptos)
            .checked_mul(scale)
            .ok_or(DurationOutOfRange)?
            / i128::from(FixedPoint::FRACTIONAL_UNITS);
        let nanoseconds = whole.checked_add(fraction).ok_or(DurationOutOfRange)?;

        let seconds =
            i64::try_from(nanoseconds / NANOSECOND_PER_SECOND).map_err(|_| DurationOutOfRange)?;
        let subsecond =
            i32::try_from(nanoseconds % NANOSECOND_PER_SECOND).map_err(|_| DurationOutOfRange)?;
        Ok(Self::new(value.span, Duration::new(seconds, subsecond)))
    }

    /// The literal that measures `self` and `other` together, spanning both,
    /// or says that no duration can be that long.
    pub fn plus(&self, other: DurationLiteral) -> Result<Self, DurationOutOfRange> {
        let interval = self
            .interval
            .checked_add(other.interval)
            .ok_or(DurationOutOfRange)?;
        Ok(Self::new(
            SourceSpan::join(&self.span, &other.span),
            interval,
        ))
    }

    /// The `number unit` text of this literal, in the largest unit that
    /// loses no precision: milliseconds when the interval is a whole number
    /// of them, microseconds when whole microseconds, else nanoseconds.
    ///
    /// A literal keeps its value down to the nanosecond (REQ-TL-parser-030),
    /// so writing every interval in milliseconds would change a literal that
    /// has a sub-millisecond part: `TIME#1us` is not `TIME#0ms`.
    ///
    /// ```rust
    /// use ironplc_dsl::common::FixedPoint;
    /// use ironplc_dsl::construct::DurationUnit;
    /// use ironplc_dsl::time::DurationLiteral;
    /// let literal = DurationLiteral::from_unit(FixedPoint::parse("1.5").unwrap(), DurationUnit::Milliseconds);
    /// assert_eq!("1500us", literal.unwrap().unit_text());
    /// ```
    pub fn unit_text(&self) -> String {
        let nanoseconds = self.interval.whole_nanoseconds();
        if nanoseconds % 1_000_000 == 0 {
            format!("{}ms", self.interval.whole_milliseconds())
        } else if nanoseconds % 1_000 == 0 {
            format!("{}us", nanoseconds / 1_000)
        } else {
            format!("{nanoseconds}ns")
        }
    }
}

impl fmt::Display for DurationLiteral {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "TIME#{}ms", self.interval.whole_milliseconds())
    }
}

// See section 2.2.3
#[derive(Debug, PartialEq, Clone)]
pub struct TimeOfDayLiteral {
    value: Time,
    /// The literal's position in the source text.
    pub span: SourceSpan,
    /// The width the source spelled: `TIME_OF_DAY#`/`TOD#` for the 32-bit
    /// type, `LTIME_OF_DAY#`/`LTOD#` for the 64-bit one.
    pub width: TemporalWidth,
}

impl TimeOfDayLiteral {
    pub fn new(value: Time) -> Self {
        Self {
            value,
            span: SourceSpan::default(),
            width: TemporalWidth::Short,
        }
    }

    /// Returns the literal with `width` recorded as the member of the family
    /// its prefix named.
    pub fn with_width(mut self, width: TemporalWidth) -> Self {
        self.width = width;
        self
    }

    /// The IEC 61131-3 type this literal is: the time of day type its prefix named.
    ///
    /// A literal states its own type, so it is checked against that type's
    /// range wherever it is written, the way a prefixed integer literal is
    /// (`INT#40000` is not an `INT` whatever it is stored into).
    pub fn type_name(&self) -> ElementaryTypeName {
        match self.width {
            TemporalWidth::Short => ElementaryTypeName::TimeOfDay,
            TemporalWidth::Long => ElementaryTypeName::LTimeOfDay,
        }
    }

    /// The millisecond-since-midnight count this literal holds and the storage
    /// its type gives it.
    ///
    /// The count is unsigned and bounded by 86,399,999 by construction, so it
    /// fits either width; the range check is vacuous rather than absent, so
    /// that a bound which stopped holding would be reported rather than
    /// silently truncated.
    pub fn stored_count(&self) -> StoredCount {
        StoredCount {
            count: i128::from(self.whole_milliseconds()),
            bits: self.width.bits(),
            signed: false,
        }
    }

    /// Returns the hour, minute, second and microsecond from the literal.
    pub fn hmsm(&self) -> (u8, u8, u8, u32) {
        self.value.as_hms_micro()
    }

    /// The literal's time of day as source text, `hh:mm:ss` and its fraction.
    pub fn daytime_text(&self) -> String {
        daytime_text(&self.value)
    }

    /// Returns milliseconds since midnight as a u32.
    ///
    /// Maximum value is 86_399_999 (23:59:59.999).
    /// The fraction the literal carries below a millisecond is truncated, as
    /// [`DurationLiteral`] truncates a sub-millisecond duration (ADR-0021).
    pub fn whole_milliseconds(&self) -> u32 {
        let (h, m, s, micro) = self.hmsm();
        (h as u32) * 3_600_000 + (m as u32) * 60_000 + (s as u32) * 1_000 + micro / 1_000
    }
}

impl fmt::Display for TimeOfDayLiteral {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "TIME_OF_DAY#{}", self.daytime_text())
    }
}

/// The text of a time of day in a literal: `hh:mm:ss` and its fraction.
///
/// Shared by the time-of-day and date-and-time literals, whose daytime parts
/// are the same grammar and so are written the same way.
///
/// The fraction is written to the nanosecond the value holds, with its
/// trailing zeros removed, and not at all when it is zero, so that the text
/// reads back as the same value: `10:00:00.25`, `10:00:00.005`, `10:00:00`.
fn daytime_text(time: &Time) -> String {
    let (h, m, s, nano) = time.as_hms_nano();
    let whole = format!("{h:02}:{m:02}:{s:02}");
    if nano == 0 {
        return whole;
    }
    let fraction = format!("{nano:09}");
    format!("{whole}.{}", fraction.trim_end_matches('0'))
}

/// The number of seconds from the Unix epoch to midnight on `date`, negative
/// for a date before the epoch.
///
/// Shared by the two date literal types so that a date and a date-and-time
/// agree on where the epoch is and what a day is worth.
fn seconds_to_midnight(date: &Date) -> i64 {
    const UNIX_EPOCH_JULIAN_DAY: i32 = 2_440_588; // 1970-01-01
    let days = i64::from(date.to_julian_day() - UNIX_EPOCH_JULIAN_DAY);
    days * i64::from(Second::per(Day))
}

// See section 2.2.3
#[derive(Debug, PartialEq, Clone)]
pub struct DateLiteral {
    pub value: Date,
    /// The literal's position in the source text.
    pub span: SourceSpan,
    /// The width the source spelled: `DATE#`/`D#` for the 32-bit type,
    /// `LDATE#` for the 64-bit one.
    pub width: TemporalWidth,
}

impl DateLiteral {
    pub fn new(value: Date) -> Self {
        Self {
            value,
            span: SourceSpan::default(),
            width: TemporalWidth::Short,
        }
    }

    /// Returns the literal with `width` recorded as the member of the family
    /// its prefix named.
    pub fn with_width(mut self, width: TemporalWidth) -> Self {
        self.width = width;
        self
    }

    /// The IEC 61131-3 type this literal is: the date type its prefix named.
    ///
    /// A literal states its own type, so it is checked against that type's
    /// range wherever it is written, the way a prefixed integer literal is
    /// (`INT#40000` is not an `INT` whatever it is stored into).
    pub fn type_name(&self) -> ElementaryTypeName {
        match self.width {
            TemporalWidth::Short => ElementaryTypeName::DATE,
            TemporalWidth::Long => ElementaryTypeName::LDATE,
        }
    }

    /// The epoch-second count this literal holds and the storage its type
    /// gives it.
    ///
    /// The count is unsigned (ADR-0025), so a date before 1970-01-01 has
    /// nowhere to go at either width.
    pub fn stored_count(&self) -> StoredCount {
        StoredCount {
            count: i128::from(self.seconds_since_epoch()),
            bits: self.width.bits(),
            signed: false,
        }
    }

    /// Returns the year, month, day from the literal.
    pub fn ymd(&self) -> (i32, u8, u8) {
        let year = self.value.year();
        let month = self.value.month();
        let day = self.value.day();
        (year, month.into(), day)
    }

    /// Returns seconds since the Unix epoch (1970-01-01).
    ///
    /// The IEC 61131-3 DATE type is stored as a u32 count of seconds since
    /// 1970-01-01, matching the CODESYS/Beckhoff industry standard. The
    /// resolution is logically 1 day but the storage unit is seconds for
    /// compatibility with DATE_AND_TIME.
    ///
    /// The count returned is the literal's own, which is not always a value
    /// the storage can hold: it is negative for a date before the epoch and
    /// beyond `u32::MAX` for one after 2106-02-07. It is computed wider than
    /// the storage so that those dates arrive at the caller to be judged
    /// rather than trapping here.
    pub fn seconds_since_epoch(&self) -> i64 {
        seconds_to_midnight(&self.value)
    }
}

impl fmt::Display for DateLiteral {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let (y, m, d) = self.ymd();
        write!(f, "DATE#{}-{:02}-{:02}", y, m, d)
    }
}

// See section 2.2.3
#[derive(Debug, PartialEq, Clone)]
pub struct DateAndTimeLiteral {
    value: PrimitiveDateTime,
    /// The literal's position in the source text.
    pub span: SourceSpan,
    /// The width the source spelled: `DATE_AND_TIME#`/`DT#` for the 32-bit
    /// type, `LDATE_AND_TIME#`/`LDT#` for the 64-bit one.
    pub width: TemporalWidth,
}

impl DateAndTimeLiteral {
    pub fn new(value: PrimitiveDateTime) -> Self {
        Self {
            value,
            span: SourceSpan::default(),
            width: TemporalWidth::Short,
        }
    }

    /// Returns the literal with `width` recorded as the member of the family
    /// its prefix named.
    pub fn with_width(mut self, width: TemporalWidth) -> Self {
        self.width = width;
        self
    }

    /// The IEC 61131-3 type this literal is: the date and time type its prefix named.
    ///
    /// A literal states its own type, so it is checked against that type's
    /// range wherever it is written, the way a prefixed integer literal is
    /// (`INT#40000` is not an `INT` whatever it is stored into).
    pub fn type_name(&self) -> ElementaryTypeName {
        match self.width {
            TemporalWidth::Short => ElementaryTypeName::DateAndTime,
            TemporalWidth::Long => ElementaryTypeName::LDateAndTime,
        }
    }

    /// The epoch-second count this literal holds and the storage its type
    /// gives it.
    ///
    /// As with [`DateLiteral::stored_count`], the count is unsigned.
    pub fn stored_count(&self) -> StoredCount {
        StoredCount {
            count: i128::from(self.seconds_since_epoch()),
            bits: self.width.bits(),
            signed: false,
        }
    }

    /// Returns the year, month, day from the literal.
    pub fn ymd(&self) -> (i32, u8, u8) {
        let year = self.value.year();
        let month = self.value.month();
        let day = self.value.day();
        (year, month.into(), day)
    }

    /// Returns the hour, minute, second and microsecond from the literal.
    pub fn hmsm(&self) -> (u8, u8, u8, u32) {
        self.value.as_hms_micro()
    }

    /// The literal's time of day as source text, `hh:mm:ss` and its fraction.
    pub fn daytime_text(&self) -> String {
        daytime_text(&self.value.time())
    }

    /// Returns seconds since the Unix epoch (1970-01-01 00:00:00).
    ///
    /// The IEC 61131-3 DATE_AND_TIME type is stored as a u32 count of seconds
    /// since 1970-01-01, matching the CODESYS/Beckhoff industry standard.
    /// Resolution is 1 second: the fraction of the literal's seconds is
    /// truncated.
    ///
    /// As with [`DateLiteral::seconds_since_epoch`], the count is the
    /// literal's own and may lie outside what the storage holds.
    pub fn seconds_since_epoch(&self) -> i64 {
        let (h, m, s, _micro) = self.hmsm();
        let tod_secs = i64::from(h) * i64::from(Second::per(Hour))
            + i64::from(m) * i64::from(Second::per(Minute))
            + i64::from(s);
        seconds_to_midnight(&self.value.date()) + tod_secs
    }
}

impl fmt::Display for DateAndTimeLiteral {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let (y, m, d) = self.ymd();
        write!(
            f,
            "DATE_AND_TIME#{}-{:02}-{:02}-{}",
            y,
            m,
            d,
            self.daytime_text()
        )
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use time::{Date, Month, PrimitiveDateTime, Time};

    fn fixed(text: &str) -> FixedPoint {
        FixedPoint::parse(text).unwrap()
    }

    fn literal(text: &str, unit: DurationUnit) -> DurationLiteral {
        DurationLiteral::from_unit(fixed(text), unit).unwrap()
    }

    /// The longest duration a `Duration` holds, in nanoseconds.
    const MAX_NANOSECONDS: i128 = i64::MAX as i128 * NANOSECOND_PER_SECOND + 999_999_999;

    #[test]
    fn from_unit_when_one_day_then_correct_duration() {
        assert_eq!(literal("1", DurationUnit::Days).interval, Duration::days(1));
    }

    #[test]
    fn from_unit_when_one_hour_then_correct_duration() {
        assert_eq!(
            literal("1", DurationUnit::Hours).interval,
            Duration::hours(1)
        );
    }

    #[test]
    fn from_unit_when_one_minute_then_correct_duration() {
        assert_eq!(
            literal("1", DurationUnit::Minutes).interval,
            Duration::minutes(1)
        );
    }

    #[test]
    fn from_unit_when_fraction_of_each_unit_then_scaled_to_nanoseconds() {
        assert_eq!(
            literal("1.5", DurationUnit::Hours).interval,
            Duration::minutes(90)
        );
        assert_eq!(
            literal("1.001", DurationUnit::Seconds).interval,
            Duration::seconds(1) + Duration::milliseconds(1)
        );
        assert_eq!(
            literal("0.001", DurationUnit::Milliseconds).interval,
            Duration::microseconds(1)
        );
        assert_eq!(
            literal("1.5", DurationUnit::Microseconds).interval,
            Duration::microseconds(1) + Duration::nanoseconds(500)
        );
    }

    #[test]
    fn from_unit_when_fraction_finer_than_nanosecond_then_truncated() {
        assert_eq!(
            literal("500.9", DurationUnit::Nanoseconds).interval,
            Duration::nanoseconds(500)
        );
        assert_eq!(
            literal("0.0000000009", DurationUnit::Seconds).interval,
            Duration::ZERO
        );
    }

    #[test]
    fn from_unit_when_fraction_of_hour_finer_than_microsecond_then_keeps_nanoseconds() {
        // REQ-TL-parser-030: the value is preserved down to the nanosecond.
        // 1e-10 hours is 360 ns.
        assert_eq!(
            literal("0.0000000001", DurationUnit::Hours).interval,
            Duration::nanoseconds(360)
        );
    }

    #[test]
    fn from_unit_when_crash_report_input_then_out_of_range() {
        assert_eq!(
            DurationLiteral::from_unit(fixed("9223372036854775807"), DurationUnit::Days),
            Err(DurationOutOfRange)
        );
    }

    #[test]
    fn from_unit_when_whole_part_exceeds_i64_then_out_of_range_not_wrapped() {
        // These wrapped to a negative or small duration when the whole part
        // was cast to `i64`.
        for text in ["9223372036854775808", "18446744073709551615"] {
            assert_eq!(
                DurationLiteral::from_unit(fixed(text), DurationUnit::Seconds),
                Err(DurationOutOfRange),
                "{text}s"
            );
        }
    }

    #[test]
    fn from_unit_when_around_the_longest_duration_then_exact_or_out_of_range() {
        // Every unit, at and around the point where the total stops fitting,
        // and at the far end of what the grammar can write, judged against an
        // independent computation in `i128`. None may panic or wrap.
        let fractions = [0, 1, 500_000_000_000_000, 999_999_999_999_999];
        for (_, unit) in DurationUnit::UNITS {
            let scale = i128::from(unit.nanoseconds());
            let edge = MAX_NANOSECONDS / scale;
            let wholes = [0, 1, edge - 1, edge, edge + 1, edge * 2, u64::MAX.into()];
            for whole in wholes.into_iter().filter(|w| *w <= u64::MAX.into()) {
                for femptos in fractions {
                    let value = FixedPoint {
                        span: SourceSpan::default(),
                        whole: u64::try_from(whole).unwrap(),
                        femptos,
                    };
                    let expected = whole * scale + i128::from(femptos) * scale / 10_i128.pow(15);
                    let actual = DurationLiteral::from_unit(value, unit);
                    if expected <= MAX_NANOSECONDS {
                        assert_eq!(
                            actual.map(|l| l.interval.whole_nanoseconds()),
                            Ok(expected),
                            "{whole}.{femptos} {unit:?}"
                        );
                    } else {
                        assert_eq!(
                            actual,
                            Err(DurationOutOfRange),
                            "{whole}.{femptos} {unit:?}"
                        );
                    }
                }
            }
        }
    }

    #[test]
    fn from_unit_when_longest_duration_then_ok_and_negation_fits() {
        let longest = literal("9223372036854775807.999999999", DurationUnit::Seconds);
        assert_eq!(longest.interval.whole_nanoseconds(), MAX_NANOSECONDS);
        assert_eq!(-longest.interval, Duration::new(-i64::MAX, -999_999_999));
    }

    #[test]
    fn plus_when_two_durations_then_sum() {
        let a = literal("1", DurationUnit::Seconds);
        let b = literal("2", DurationUnit::Seconds);
        assert_eq!(a.plus(b).unwrap().interval, Duration::seconds(3));
    }

    #[test]
    fn plus_when_sum_is_longest_duration_then_ok() {
        let a = literal("9223372036854775807", DurationUnit::Seconds);
        let b = literal("999999999", DurationUnit::Nanoseconds);
        assert_eq!(
            a.plus(b).unwrap().interval.whole_nanoseconds(),
            MAX_NANOSECONDS
        );
    }

    #[test]
    fn plus_when_sum_exceeds_longest_duration_then_out_of_range() {
        let a = literal("9223372036854775807", DurationUnit::Seconds);
        let b = literal("1", DurationUnit::Seconds);
        assert_eq!(a.plus(b), Err(DurationOutOfRange));
    }

    #[test]
    fn plus_when_each_part_fits_but_sum_does_not_then_out_of_range() {
        // The multi-part grammar reaches this with every part valid: the
        // most days a duration holds, and a day short of a day more.
        let days = literal("106751991167300", DurationUnit::Days);
        let hours = literal("23", DurationUnit::Hours);
        assert_eq!(days.plus(hours), Err(DurationOutOfRange));
    }

    #[test]
    fn diagnostic_when_out_of_range_then_p2039_naming_literal_and_type() {
        let span = SourceSpan::default();
        let short = DurationOutOfRange.diagnostic(span.clone(), "T#1d", TemporalWidth::Short);
        assert_eq!(short.code, Problem::DurationLiteralOutOfRange.code());
        assert_eq!(
            short.primary.message,
            "Constant 'T#1d' is outside the range of type 'TIME'"
        );
        let long = DurationOutOfRange.diagnostic(span, "LTIME#1d", TemporalWidth::Long);
        assert!(long.primary.message.ends_with("of type 'LTIME'"));
    }

    #[test]
    fn display_when_duration_then_formats_as_time_ms() {
        let dur = literal("2", DurationUnit::Seconds);
        assert_eq!(format!("{dur}"), "TIME#2000ms");
    }

    #[test]
    fn display_when_time_of_day_then_formats_as_tod() {
        let tod = TimeOfDayLiteral::new(Time::from_hms(14, 30, 0).unwrap());
        assert_eq!(format!("{tod}"), "TIME_OF_DAY#14:30:00");
    }

    #[test]
    fn display_when_time_of_day_has_fraction_then_formats_fraction_without_trailing_zeros() {
        let tod = TimeOfDayLiteral::new(Time::from_hms_milli(14, 30, 0, 250).unwrap());
        assert_eq!(format!("{tod}"), "TIME_OF_DAY#14:30:00.25");
    }

    #[test]
    fn whole_milliseconds_when_fraction_below_millisecond_then_truncated() {
        let tod = TimeOfDayLiteral::new(Time::from_hms_nano(10, 0, 0, 250_999_999).unwrap());
        assert_eq!(tod.whole_milliseconds(), 36_000_250);
    }

    #[test]
    fn seconds_since_epoch_when_date_and_time_has_fraction_then_truncated() {
        let dt = DateAndTimeLiteral::new(PrimitiveDateTime::new(
            Date::from_calendar_date(1970, Month::January, 1).unwrap(),
            Time::from_hms_milli(0, 0, 1, 999).unwrap(),
        ));
        assert_eq!(dt.seconds_since_epoch(), 1);
    }

    #[test]
    fn display_when_date_and_time_has_fraction_then_formats_fraction() {
        let dt = DateAndTimeLiteral::new(PrimitiveDateTime::new(
            Date::from_calendar_date(2025, Month::January, 1).unwrap(),
            Time::from_hms_micro(12, 0, 0, 5_000).unwrap(),
        ));
        assert_eq!(format!("{dt}"), "DATE_AND_TIME#2025-01-01-12:00:00.005");
    }

    #[test]
    fn display_when_date_then_formats_as_date() {
        let date = DateLiteral::new(Date::from_calendar_date(2025, Month::March, 15).unwrap());
        assert_eq!(format!("{date}"), "DATE#2025-03-15");
    }

    #[test]
    fn display_when_date_and_time_then_formats_as_dt() {
        let dt = DateAndTimeLiteral::new(PrimitiveDateTime::new(
            Date::from_calendar_date(2025, Month::January, 1).unwrap(),
            Time::from_hms(12, 0, 0).unwrap(),
        ));
        assert_eq!(format!("{dt}"), "DATE_AND_TIME#2025-01-01-12:00:00");
    }
}
