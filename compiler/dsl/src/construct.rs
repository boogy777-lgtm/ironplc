//! Builders that turn the pieces of a literal or an initializer into DSL
//! values.
//!
//! Each rule here decides one thing about a literal: which duration parts may
//! follow one another, which clock and calendar values exist, and how an
//! initializer expression takes its shape. (Which `$` escapes decode is
//! decided by [`crate::string_escape`].) The parsers only recognise the
//! pieces; they call these builders, so a rule is written once whichever
//! grammar found the literal.

use std::sync::LazyLock;

use time::{Date, Month, Time};

use crate::common::{
    ConstantKind, EnumeratedInitialValueAssignment, EnumeratedValue, FixedPoint,
    InitialValueAssignmentKind, LateResolvedInitialValue, LateResolvedInitializer,
    SimpleExprInitializer, SimpleInitializer, StringType, StructureInitializationDeclaration,
    TypeName,
};
use crate::core::{Id, SourceSpan};
use crate::textual::{
    Expr, ExprKind, Function, NamedVariable, ParamAssignmentKind, PositionalInput,
    SymbolicVariableKind, UnaryOp, Variable,
};
use crate::time::DurationLiteral;

/// The characters a character-string token denotes: the text between its two
/// delimiting quotes with its `$` escapes decoded. An invalid escape is kept
/// as written; the escape check reports it.
pub fn unquote(text: &str, width: &StringType) -> Vec<char> {
    let inner = text
        .get(1..text.len().saturating_sub(1))
        .unwrap_or_default();
    crate::string_escape::decode(inner, width).chars
}

/// A unit of a duration literal part, smallest first so that the derived
/// order is the order of magnitude.
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord)]
pub enum DurationUnit {
    Nanoseconds,
    Microseconds,
    Milliseconds,
    Seconds,
    Minutes,
    Hours,
    Days,
}

impl DurationUnit {
    /// Every unit with its spelling, largest first. This is the one table of
    /// unit spellings: [`DurationUnit::from_word`] looks a word up in it and
    /// [`DurationUnit::expectation`] lists it, so a unit added here is
    /// recognised and reported without any other change.
    pub const UNITS: [(&'static str, DurationUnit); 7] = [
        ("d", DurationUnit::Days),
        ("h", DurationUnit::Hours),
        ("m", DurationUnit::Minutes),
        ("s", DurationUnit::Seconds),
        ("ms", DurationUnit::Milliseconds),
        ("us", DurationUnit::Microseconds),
        ("ns", DurationUnit::Nanoseconds),
    ];

    /// The unit a spelling names, in either case: `d`, `h`, `m`, `s`, `ms`,
    /// `us` or `ns`.
    pub fn from_word(word: &str) -> Option<DurationUnit> {
        Self::UNITS
            .iter()
            .find(|(spelling, _)| spelling.eq_ignore_ascii_case(word))
            .map(|(_, unit)| *unit)
    }

    /// The expected-input text for a word that is not a unit, naming every
    /// spelling in [`DurationUnit::UNITS`] in table order.
    pub fn expectation() -> &'static str {
        static TEXT: LazyLock<String> = LazyLock::new(|| {
            let spellings: Vec<&str> = DurationUnit::UNITS
                .iter()
                .map(|(spelling, _)| *spelling)
                .collect();
            format!("duration unit ({})", spellings.join(", "))
        });
        TEXT.as_str()
    }
}

/// Why the parts of a duration literal do not form one.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum IntervalError {
    /// A unit is not smaller than the one before it, which also rules out a
    /// repeated unit.
    UnitOrder,
    /// A part before the last has a fractional value.
    FractionBeforeLast,
}

/// Checks the parts of a duration literal, each given as its unit and
/// whether its value has a non-zero fraction (REQ-TL-021): the units must be
/// in strictly descending magnitude and only the last part may be
/// fractional. The first part to break a rule decides the error.
pub fn check_interval_parts(
    parts: impl IntoIterator<Item = (DurationUnit, bool)>,
) -> Result<(), IntervalError> {
    let mut parts = parts.into_iter().peekable();
    let mut previous: Option<DurationUnit> = None;
    while let Some((unit, fractional)) = parts.next() {
        if previous.is_some_and(|p| unit >= p) {
            return Err(IntervalError::UnitOrder);
        }
        if fractional && parts.peek().is_some() {
            return Err(IntervalError::FractionBeforeLast);
        }
        previous = Some(unit);
    }
    Ok(())
}

/// Sums the parts of a duration literal once [`check_interval_parts`]
/// accepts them.
pub fn combine_interval_parts(
    first: (FixedPoint, DurationUnit),
    rest: Vec<(FixedPoint, DurationUnit)>,
) -> Result<DurationLiteral, IntervalError> {
    check_interval_parts(
        std::iter::once(&first)
            .chain(&rest)
            .map(|(value, unit)| (*unit, value.femptos != 0)),
    )?;
    let (value, unit) = first;
    let total = rest
        .into_iter()
        .fold(duration_part(value, unit), |sum, (value, unit)| {
            sum.plus(duration_part(value, unit))
        });
    Ok(total)
}

fn duration_part(value: FixedPoint, unit: DurationUnit) -> DurationLiteral {
    match unit {
        DurationUnit::Nanoseconds => DurationLiteral::nanoseconds(value),
        DurationUnit::Microseconds => DurationLiteral::microseconds(value),
        DurationUnit::Days => DurationLiteral::days(value),
        DurationUnit::Hours => DurationLiteral::hours(value),
        DurationUnit::Minutes => DurationLiteral::minutes(value),
        DurationUnit::Seconds => DurationLiteral::seconds(value),
        DurationUnit::Milliseconds => DurationLiteral::milliseconds(value),
    }
}

/// A field of a time of day.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum ClockField {
    Hour,
    Minute,
    Second,
}

impl ClockField {
    /// Whether `value` is one this field can have: hours 0 to 23, and
    /// minutes and whole seconds 0 to 59.
    pub fn accepts(self, value: u128) -> bool {
        let max = match self {
            ClockField::Hour => 23,
            ClockField::Minute | ClockField::Second => 59,
        };
        value <= max
    }
}

/// A time of day, or the first field that is out of range.
///
/// `nanoseconds` is the fraction of the second; the stored count truncates
/// further, to the type's own unit (ADR-0025).
pub fn time_of_day(
    hour: u128,
    minute: u128,
    second: u128,
    nanoseconds: u32,
) -> Result<Time, ClockField> {
    for (field, value) in [
        (ClockField::Hour, hour),
        (ClockField::Minute, minute),
        (ClockField::Second, second),
    ] {
        if !field.accepts(value) {
            return Err(field);
        }
    }
    // Every field is in range, so only a fraction of a second or more can
    // still fail.
    Time::from_hms_nano(hour as u8, minute as u8, second as u8, nanoseconds)
        .map_err(|_| ClockField::Second)
}

/// The part of a calendar date that is out of range.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum DateField {
    Year,
    Month,
    /// The day is zero or past the end of its month.
    Day,
}

/// The latest year a date holds.
const MAX_YEAR: u128 = 9999;

/// A calendar date, or the first part that does not exist: the year, then
/// the month, then the day within that month.
pub fn calendar_date(year: u128, month: u128, day: u128) -> Result<Date, DateField> {
    if year > MAX_YEAR {
        return Err(DateField::Year);
    }
    let month = u8::try_from(month)
        .ok()
        .and_then(|month| Month::try_from(month).ok())
        .ok_or(DateField::Month)?;
    let day = u8::try_from(day).map_err(|_| DateField::Day)?;
    // The year is at most 9999, so it fits.
    Date::from_calendar_date(year as i32, month, day).map_err(|_| DateField::Day)
}

/// Negates a literal constant, for the small set of literal kinds that
/// `constant()` itself already supports with a leading sign (integer, real,
/// duration). Used to collapse `ExprKind::UnaryOp(Neg, Const(c))` — the
/// shape produced when a negative literal reaches the parser through
/// `expression()`'s unary-operator handling rather than directly through
/// `constant()` — back to the same `Const` shape `constant()` alone would
/// have produced, so that e.g. `x : INT := -123;` continues to parse as a
/// plain literal and does not require `allow_constant_initializer_expressions`.
/// Returns `Err(c)` (giving the original value back) for literal kinds that
/// have no natural negation (e.g. booleans, strings), which fall through to
/// `SimpleExpr` instead.
pub fn negate_literal_constant(c: ConstantKind) -> Result<ConstantKind, ConstantKind> {
    match c {
        ConstantKind::IntegerLiteral(mut lit) => {
            lit.value.is_neg = !lit.value.is_neg;
            Ok(ConstantKind::IntegerLiteral(lit))
        }
        ConstantKind::RealLiteral(mut lit) => {
            lit.value = -lit.value;
            Ok(ConstantKind::RealLiteral(lit))
        }
        ConstantKind::Duration(mut lit) => {
            lit.interval = -lit.interval;
            Ok(ConstantKind::Duration(lit))
        }
        other => Err(other),
    }
}

/// A member list written against a user type name: `T := (a := 1)`. The
/// type may be a structure or a function block; the resolver decides.
pub fn late_resolved_members(
    init: StructureInitializationDeclaration,
) -> InitialValueAssignmentKind {
    InitialValueAssignmentKind::LateResolvedType(LateResolvedInitializer {
        type_name: init.type_name,
        initial_value: Some(LateResolvedInitialValue::Members(init.elements_init)),
    })
}

/// A value written against a user type name: `T := Red`. A qualified value
/// (`T := T#Red`) names an enumeration and is settled here; a bare
/// identifier may be an enumeration value or a named constant of any other
/// type, and the resolver decides.
pub fn late_resolved_or_enumerated(
    type_name: TypeName,
    value: EnumeratedValue,
) -> InitialValueAssignmentKind {
    if value.type_name.is_some() {
        return InitialValueAssignmentKind::EnumeratedType(EnumeratedInitialValueAssignment {
            type_name,
            initial_value: Some(value),
        });
    }
    InitialValueAssignmentKind::LateResolvedType(LateResolvedInitializer {
        type_name,
        initial_value: Some(LateResolvedInitialValue::Value(value.value)),
    })
}

/// Returns the literal constant that an initializer expression denotes, if
/// it denotes one: a literal, or a literal with one leading unary minus
/// (e.g. `-123`, the shape `expression()` produces for a negative literal
/// because it routes the sign through its own unary-operator handling).
///
/// Returns `None` for everything else, including a negation that has no
/// natural literal form (`-TRUE`).
pub fn literal_value_of(e: &Expr) -> Option<ConstantKind> {
    match &e.kind {
        ExprKind::Const(c) => Some(c.clone()),
        ExprKind::UnaryOp(u) if u.op == UnaryOp::Neg => match &u.term.kind {
            ExprKind::Const(c) => negate_literal_constant(c.clone()).ok(),
            _ => None,
        },
        _ => None,
    }
}

/// Collapses an initializer expression to `Simple` when it is exactly a
/// literal (optionally with one leading unary minus, e.g. `-123`), and
/// otherwise keeps it as `SimpleExpr` (the constant-expression extension,
/// folded by `xform_fold_initializer_expressions`).
pub fn resolve_initializer_expr(type_name: TypeName, e: Expr) -> InitialValueAssignmentKind {
    match literal_value_of(&e) {
        Some(initial_value) => InitialValueAssignmentKind::Simple(SimpleInitializer {
            type_name,
            initial_value: Some(initial_value),
        }),
        None => InitialValueAssignmentKind::SimpleExpr(SimpleExprInitializer {
            type_name,
            initial_value: e,
        }),
    }
}

/// The call node for `__NEW(T[, n])` or `__TYPEOF(T)`: the type name is
/// recorded as a variable reference, followed by the element count when there
/// is one. `operator` is the operator's name as written and `span` covers the
/// whole call.
pub fn special_operator_type_call(
    operator: Id,
    type_name: Id,
    count: Option<Expr>,
    span: SourceSpan,
) -> Expr {
    let mut param_assignment = vec![ParamAssignmentKind::positional(ExprKind::Variable(
        Variable::Symbolic(SymbolicVariableKind::Named(NamedVariable {
            name: type_name,
        })),
    ))];
    if let Some(count) = count {
        param_assignment.push(ParamAssignmentKind::PositionalInput(PositionalInput {
            expr: count,
        }));
    }
    Expr::new(ExprKind::Function(Function {
        name: operator,
        param_assignment,
    }))
    .with_span(span)
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::common::{Boolean, BooleanLiteral};

    fn fixed(whole: u64, femptos: u64) -> FixedPoint {
        FixedPoint {
            span: SourceSpan::default(),
            whole,
            femptos,
        }
    }

    fn int_const(value: &str) -> ConstantKind {
        ConstantKind::integer_literal(value).unwrap()
    }

    fn variable(name: &str) -> Expr {
        Expr::new(ExprKind::Variable(Variable::Symbolic(
            SymbolicVariableKind::Named(NamedVariable {
                name: Id::from(name),
            }),
        )))
    }

    #[test]
    fn unquote_when_escapes_then_decoded_without_delimiters() {
        assert_eq!(
            unquote("'a$$b$41'", &StringType::String),
            vec!['a', '$', 'b', 'A']
        );
        assert_eq!(unquote("\"$0041\"", &StringType::WString), vec!['A']);
    }

    #[test]
    fn unquote_when_invalid_escape_then_kept_as_written() {
        assert_eq!(unquote("'$Q'", &StringType::String), vec!['$', 'Q']);
    }

    #[test]
    fn unquote_when_text_shorter_than_delimiters_then_empty() {
        assert!(unquote("", &StringType::String).is_empty());
        assert!(unquote("'", &StringType::String).is_empty());
    }

    #[test]
    fn from_word_when_unit_spelling_then_unit_in_either_case() {
        assert_eq!(
            DurationUnit::from_word("ms"),
            Some(DurationUnit::Milliseconds)
        );
        assert_eq!(
            DurationUnit::from_word("MS"),
            Some(DurationUnit::Milliseconds)
        );
        assert_eq!(DurationUnit::from_word("d"), Some(DurationUnit::Days));
        assert_eq!(
            DurationUnit::from_word("ns"),
            Some(DurationUnit::Nanoseconds)
        );
    }

    #[test]
    fn from_word_when_not_a_unit_then_none() {
        assert_eq!(DurationUnit::from_word(""), None);
        assert_eq!(DurationUnit::from_word("x"), None);
        assert_eq!(DurationUnit::from_word("sec"), None);
    }

    #[test]
    fn from_word_when_any_table_spelling_then_its_unit() {
        for (spelling, unit) in DurationUnit::UNITS {
            assert_eq!(DurationUnit::from_word(spelling), Some(unit));
        }
    }

    #[test]
    fn expectation_when_listed_then_names_every_table_spelling_largest_first() {
        let spellings: Vec<&str> = DurationUnit::UNITS.iter().map(|(s, _)| *s).collect();
        assert!(
            DurationUnit::expectation().contains(&spellings.join(", ")),
            "{}",
            DurationUnit::expectation()
        );
    }

    #[test]
    fn check_interval_parts_when_descending_then_ok() {
        let parts = [
            (DurationUnit::Hours, false),
            (DurationUnit::Minutes, false),
            (DurationUnit::Seconds, true),
        ];
        assert_eq!(check_interval_parts(parts), Ok(()));
    }

    #[test]
    fn check_interval_parts_when_unit_repeated_or_ascending_then_unit_order() {
        for parts in [
            [
                (DurationUnit::Seconds, false),
                (DurationUnit::Seconds, false),
            ],
            [
                (DurationUnit::Seconds, false),
                (DurationUnit::Minutes, false),
            ],
        ] {
            assert_eq!(check_interval_parts(parts), Err(IntervalError::UnitOrder));
        }
    }

    #[test]
    fn check_interval_parts_when_fraction_before_last_then_fraction_error() {
        let parts = [
            (DurationUnit::Minutes, true),
            (DurationUnit::Seconds, false),
        ];
        assert_eq!(
            check_interval_parts(parts),
            Err(IntervalError::FractionBeforeLast)
        );
    }

    #[test]
    fn check_interval_parts_when_both_rules_broken_then_first_part_decides() {
        let parts = [(DurationUnit::Minutes, true), (DurationUnit::Hours, false)];
        assert_eq!(
            check_interval_parts(parts),
            Err(IntervalError::FractionBeforeLast)
        );
    }

    #[test]
    fn check_interval_parts_when_no_parts_then_ok() {
        assert_eq!(check_interval_parts([]), Ok(()));
    }

    #[test]
    fn combine_interval_parts_when_descending_then_sum() {
        let total = combine_interval_parts(
            (fixed(1, 0), DurationUnit::Minutes),
            vec![(fixed(30, 0), DurationUnit::Seconds)],
        )
        .unwrap();
        assert_eq!(total.interval, time::Duration::seconds(90));
    }

    #[test]
    fn combine_interval_parts_when_ascending_then_unit_order() {
        let result = combine_interval_parts(
            (fixed(30, 0), DurationUnit::Seconds),
            vec![(fixed(1, 0), DurationUnit::Minutes)],
        );
        assert_eq!(result, Err(IntervalError::UnitOrder));
    }

    #[test]
    fn combine_interval_parts_when_fraction_before_last_then_fraction_error() {
        let result = combine_interval_parts(
            (
                fixed(1, FixedPoint::FRACTIONAL_UNITS / 2),
                DurationUnit::Minutes,
            ),
            vec![(fixed(30, 0), DurationUnit::Seconds)],
        );
        assert_eq!(result, Err(IntervalError::FractionBeforeLast));
    }

    #[test]
    fn accepts_when_at_and_past_limit_then_boundary() {
        assert!(ClockField::Hour.accepts(23));
        assert!(!ClockField::Hour.accepts(24));
        assert!(ClockField::Minute.accepts(59));
        assert!(!ClockField::Minute.accepts(60));
        assert!(ClockField::Second.accepts(59));
        assert!(!ClockField::Second.accepts(60));
    }

    #[test]
    fn time_of_day_when_in_range_then_time_with_fraction() {
        let time = time_of_day(10, 30, 15, 250_000_000).unwrap();
        assert_eq!(time, Time::from_hms_nano(10, 30, 15, 250_000_000).unwrap());
    }

    #[test]
    fn time_of_day_when_field_out_of_range_then_first_bad_field() {
        assert_eq!(time_of_day(24, 0, 0, 0), Err(ClockField::Hour));
        assert_eq!(time_of_day(0, 60, 0, 0), Err(ClockField::Minute));
        assert_eq!(time_of_day(0, 0, 60, 0), Err(ClockField::Second));
        assert_eq!(time_of_day(24, 60, 60, 0), Err(ClockField::Hour));
        assert_eq!(time_of_day(u128::MAX, 0, 0, 0), Err(ClockField::Hour));
    }

    #[test]
    fn time_of_day_when_fraction_past_one_second_then_second() {
        assert_eq!(time_of_day(0, 0, 0, 1_000_000_000), Err(ClockField::Second));
    }

    #[test]
    fn calendar_date_when_real_date_then_date() {
        assert_eq!(
            calendar_date(2024, 2, 29),
            Ok(Date::from_calendar_date(2024, Month::February, 29).unwrap())
        );
    }

    #[test]
    fn calendar_date_when_february_then_follows_leap_years() {
        assert!(calendar_date(2024, 2, 29).is_ok());
        assert_eq!(calendar_date(1900, 2, 29), Err(DateField::Day));
        assert!(calendar_date(2000, 2, 29).is_ok());
        assert_eq!(calendar_date(2023, 4, 31), Err(DateField::Day));
        assert!(calendar_date(2023, 12, 31).is_ok());
    }

    #[test]
    fn calendar_date_when_part_out_of_range_then_that_part() {
        assert_eq!(calendar_date(10000, 1, 1), Err(DateField::Year));
        assert_eq!(calendar_date(2023, 0, 1), Err(DateField::Month));
        assert_eq!(calendar_date(2023, 13, 1), Err(DateField::Month));
        assert_eq!(calendar_date(2023, 1, 0), Err(DateField::Day));
        assert_eq!(calendar_date(2023, 1, 300), Err(DateField::Day));
        assert_eq!(calendar_date(u128::MAX, 1, 1), Err(DateField::Year));
    }

    #[test]
    fn calendar_date_when_several_parts_bad_then_year_before_month_before_day() {
        assert_eq!(calendar_date(10000, 13, 40), Err(DateField::Year));
        assert_eq!(calendar_date(2023, 13, 40), Err(DateField::Month));
    }

    #[test]
    fn negate_literal_constant_when_integer_then_sign_flipped() {
        let negated = negate_literal_constant(int_const("5"));
        assert!(matches!(
            negated,
            Ok(ConstantKind::IntegerLiteral(ref lit)) if lit.value.is_neg
        ));
    }

    #[test]
    fn negate_literal_constant_when_boolean_then_given_back() {
        let original = ConstantKind::Boolean(BooleanLiteral::new(Boolean::True));
        assert_eq!(negate_literal_constant(original.clone()), Err(original));
    }

    #[test]
    fn literal_value_of_when_literal_or_negated_literal_then_constant() {
        let plain = Expr::new(ExprKind::Const(int_const("7")));
        assert_eq!(literal_value_of(&plain), Some(int_const("7")));
        let negated = Expr::unary(UnaryOp::Neg, plain);
        assert!(literal_value_of(&negated).is_some());
    }

    #[test]
    fn literal_value_of_when_not_a_literal_then_none() {
        assert_eq!(literal_value_of(&variable("x")), None);
    }

    #[test]
    fn resolve_initializer_expr_when_literal_then_simple() {
        let kind = resolve_initializer_expr(
            TypeName::from("INT"),
            Expr::new(ExprKind::Const(int_const("1"))),
        );
        assert!(matches!(kind, InitialValueAssignmentKind::Simple(_)));
    }

    #[test]
    fn resolve_initializer_expr_when_expression_then_simple_expr() {
        let kind = resolve_initializer_expr(TypeName::from("INT"), variable("x"));
        assert!(matches!(kind, InitialValueAssignmentKind::SimpleExpr(_)));
    }

    #[test]
    fn late_resolved_or_enumerated_when_qualified_then_enumerated() {
        let mut value = EnumeratedValue::new("Red");
        value.type_name = Some(TypeName::from("Color"));
        let kind = late_resolved_or_enumerated(TypeName::from("Color"), value);
        assert!(matches!(
            kind,
            InitialValueAssignmentKind::EnumeratedType(_)
        ));
    }

    #[test]
    fn late_resolved_or_enumerated_when_bare_then_late_resolved_value() {
        let kind =
            late_resolved_or_enumerated(TypeName::from("Color"), EnumeratedValue::new("Red"));
        assert!(matches!(
            kind,
            InitialValueAssignmentKind::LateResolvedType(LateResolvedInitializer {
                initial_value: Some(LateResolvedInitialValue::Value(_)),
                ..
            })
        ));
    }

    #[test]
    fn special_operator_type_call_when_count_then_two_parameters() {
        let call = special_operator_type_call(
            Id::from("__NEW"),
            Id::from("T"),
            Some(Expr::new(ExprKind::Const(int_const("3")))),
            SourceSpan::default(),
        );
        assert!(matches!(
            call.kind,
            ExprKind::Function(ref function)
                if function.name == Id::from("__NEW") && function.param_assignment.len() == 2
        ));
    }

    #[test]
    fn special_operator_type_call_when_no_count_then_one_parameter() {
        let call = special_operator_type_call(
            Id::from("__TYPEOF"),
            Id::from("T"),
            None,
            SourceSpan::default(),
        );
        assert!(matches!(
            call.kind,
            ExprKind::Function(ref function) if function.param_assignment.len() == 1
        ));
    }
}
