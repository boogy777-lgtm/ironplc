//! Duration and date literal spec-conformance tests.

use super::common::*;
use spec_test_macro::spec_test;

#[spec_test(REQ_TL_parser_002)]
#[rstest]
#[case::lower_t_lower("t#5s")]
#[case::upper_t_lower("T#5s")]
#[case::keyword_upper("TIME#5s")]
#[case::keyword_lower("time#5s")]
#[case::keyword_mixed("Time#5s")]
fn duration_spec_req_tl_002_prefix_case_insensitive(#[case] literal: &str) {
    // REQ-TL-002: prefix is recognized case-insensitively.
    let source = duration_program(literal);
    let result = parse_program(&source, &FileId::default(), &CompilerOptions::default());
    assert!(
        result.is_ok(),
        "parse failed for {literal}: {:?}",
        result.err()
    );
}

/// REQ-TL-parser-010: the units are d, h, m, s, ms, us and ns, with any
/// other name rejected.
#[spec_test(REQ_TL_parser_010)]
fn duration_spec_req_tl_010_supported_units() {
    for literal in ["T#5d", "T#5h", "T#5m", "T#5s", "T#5ms", "T#5us", "T#5ns"] {
        let source = duration_program(literal);
        let result = parse_program(&source, &FileId::default(), &CompilerOptions::default());
        assert!(result.is_ok(), "{literal} should parse: {:?}", result.err());
    }

    let source = duration_program("T#5min");
    let result = parse_program(&source, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_err(), "expected parse error for T#5min");
}

#[spec_test(REQ_TL_parser_011)]
#[rstest]
#[case::s("T#5S", Duration::seconds(5))]
#[case::ms("T#100MS", Duration::milliseconds(100))]
#[case::h("T#1H", Duration::hours(1))]
#[case::d("T#1D", Duration::days(1))]
#[case::m("T#30M", Duration::minutes(30))]
fn duration_spec_req_tl_011_unit_suffix_uppercase_accepted(
    #[case] literal: &str,
    #[case] expected: Duration,
) {
    // REQ-TL-011: unit suffixes are case-insensitive.
    let source = duration_program(literal);
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default()).unwrap();
    assert_eq!(extract_duration(&library).interval, expected);
}

#[rstest]
#[case::ms_capital_m("T#500Ms", Duration::milliseconds(500))]
#[case::ms_capital_s("T#500mS", Duration::milliseconds(500))]
fn duration_spec_req_tl_011_unit_suffix_mixed_case_accepted(
    #[case] literal: &str,
    #[case] expected: Duration,
) {
    // REQ-TL-011: mixed-case unit suffixes parse identically to lowercase.
    let source = duration_program(literal);
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default()).unwrap();
    assert_eq!(extract_duration(&library).interval, expected);
}

/// REQ-TL-parser-012: a longer unit name is matched before a shorter one it
/// begins with, so `100ms` is milliseconds.
#[spec_test(REQ_TL_parser_012)]
fn duration_spec_req_tl_012_ms_matched_before_m() {
    let source = duration_program("T#100ms");
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default()).unwrap();
    assert_eq!(
        extract_duration(&library).interval,
        Duration::milliseconds(100)
    );

    // `us` and `ns` conflict with no other unit.
    let source = duration_program("T#100us");
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default()).unwrap();
    assert_eq!(
        extract_duration(&library).interval,
        Duration::microseconds(100)
    );
}

#[spec_test(REQ_TL_parser_021)]
#[test]
fn duration_spec_req_tl_021_compound_interval() {
    // REQ-TL-021: compound interval with parts in descending magnitude.
    let source = duration_program("T#1d2h30m15s500ms");
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default()).unwrap();
    let expected = Duration::days(1)
        + Duration::hours(2)
        + Duration::minutes(30)
        + Duration::seconds(15)
        + Duration::milliseconds(500);
    assert_eq!(extract_duration(&library).interval, expected);
}

#[spec_test(REQ_TL_parser_022)]
#[rstest]
#[case::lower("T#1d_2h_30m_5s_100ms")]
#[case::upper("T#1D_2H_30M_5S_100MS")]
fn duration_spec_req_tl_022_compound_with_underscore(#[case] literal: &str) {
    // REQ-TL-022: optional `_` separator in compound intervals.
    let source = duration_program(literal);
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default()).unwrap();
    let expected = Duration::days(1)
        + Duration::hours(2)
        + Duration::minutes(30)
        + Duration::seconds(5)
        + Duration::milliseconds(100);
    assert_eq!(extract_duration(&library).interval, expected);
}

#[spec_test(REQ_TL_parser_023)]
#[test]
fn duration_spec_req_tl_023_negative_duration() {
    // REQ-TL-023: optional leading `-` negates the interval.
    let source = duration_program("T#-5s");
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default()).unwrap();
    assert_eq!(extract_duration(&library).interval, Duration::seconds(-5));
}

#[rstest]
#[case("D#2026-01-01")]
#[case("d#2026-01-01")]
fn date_prefix_case_insensitive(#[case] literal: &str) {
    // `contextual_keyword("D")` matches the prefix in any case.
    let source = format!(
        "FUNCTION fun:DATE\nVAR\n    dv : DATE := {literal};\nEND_VAR\nfun := dv;\nEND_FUNCTION"
    );
    let result = parse_program(&source, &FileId::default(), &CompilerOptions::default());
    assert!(
        result.is_ok(),
        "parse failed for {literal}: {:?}",
        result.err()
    );
}

// The literals of #1814, and the forms the compound grammar allows: skipped
// units, a fixed-point last part, and `_` between parts.
#[rstest]
#[case::minutes_seconds("T#1m30s", Duration::minutes(1) + Duration::seconds(30))]
#[case::hours_minutes("T#2h3m", Duration::hours(2) + Duration::minutes(3))]
#[case::seconds_millis("T#4s5ms", Duration::seconds(4) + Duration::milliseconds(5))]
#[case::days_hours("T#1d2h", Duration::days(1) + Duration::hours(2))]
#[case::four_parts(
    "T#1h2m3s4ms",
    Duration::hours(1) + Duration::minutes(2) + Duration::seconds(3) + Duration::milliseconds(4)
)]
#[case::underscore("T#1h_30m", Duration::hours(1) + Duration::minutes(30))]
#[case::structured_text_basics("T#1h30m", Duration::hours(1) + Duration::minutes(30))]
#[case::skipped_units("T#1d30m", Duration::days(1) + Duration::minutes(30))]
#[case::fixed_point_last("T#1m1.5s", Duration::minutes(1) + Duration::milliseconds(1500))]
#[case::long_prefix("TIME#1m30s", Duration::minutes(1) + Duration::seconds(30))]
#[case::negative("T#-1m30s", -(Duration::minutes(1) + Duration::seconds(30)))]
fn parse_when_compound_duration_then_sum_of_parts(
    #[case] literal: &str,
    #[case] expected: Duration,
) {
    let source = duration_program(literal);
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default());
    assert!(library.is_ok(), "{literal}: {:?}", library.err());
    assert_eq!(extract_duration(&library.unwrap()).interval, expected);
}

#[rstest]
#[case::ascending_units("T#30m1h")]
#[case::repeated_unit("T#1m1m")]
#[case::fixed_point_before_last("T#1.5h30m")]
fn parse_when_compound_duration_malformed_then_error(#[case] literal: &str) {
    let source = duration_program(literal);
    let result = parse_program(&source, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_err(), "{literal} parsed");
}

/// REQ-TL-parser-001: a duration literal begins with `T`, `TIME` or `LTIME`
/// and `#`.
#[spec_test(REQ_TL_parser_001)]
#[rstest]
#[case::t("T#5s")]
#[case::time("TIME#5s")]
fn duration_spec_req_tl_001_prefixes(#[case] literal: &str) {
    let source = duration_program(literal);
    let result = parse_program(&source, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_ok(), "{literal} should parse: {:?}", result.err());
}

/// REQ-TL-parser-003: `LTIME#` is only accepted with `allow_long_time_types`.
#[spec_test(REQ_TL_parser_003)]
#[test]
fn duration_spec_req_tl_003_ltime_requires_flag() {
    let source = duration_program("LTIME#5s");
    let rejected = parse_program(&source, &FileId::default(), &CompilerOptions::default());
    assert!(rejected.is_err(), "LTIME# is gated by allow_long_time_types");

    let options = CompilerOptions::from_dialect(Dialect::Iec61131_3Ed3);
    let accepted = parse_program(&source, &FileId::default(), &options);
    assert!(accepted.is_ok(), "{:?}", accepted.err());
}

/// REQ-TL-parser-004: `LT#` is the abbreviated `LTIME#`, and is an `LTIME`
/// literal. The abbreviation is accepted in every dialect, unlike `LTIME`
/// (REQ-TL-parser-003).
#[spec_test(REQ_TL_parser_004)]
#[test]
fn duration_spec_req_tl_004_lt_abbreviation() {
    let source = duration_program("LT#5s");
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default()).unwrap();
    let literal = extract_duration(&library);
    assert_eq!(literal.interval, Duration::seconds(5));
    assert_eq!(literal.width, TemporalWidth::Long);

    // The abbreviation takes a compound interval too.
    let source = duration_program("LT#1m30s");
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default()).unwrap();
    assert_eq!(
        extract_duration(&library).interval,
        Duration::minutes(1) + Duration::seconds(30)
    );
}

/// A `LT` that is not immediately before `#` is an ordinary identifier, so a
/// type named `LT` keeps working: `LT#Red` is an enumeration value, not a
/// duration literal.
#[test]
fn parse_when_type_named_lt_then_enumeration_value() {
    let source = "TYPE LT : (Red, Green); END_TYPE
PROGRAM main
VAR
    x : LT := LT#Red;
END_VAR
END_PROGRAM";
    let result = parse_program(source, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_ok(), "{:?}", result.err());
}

/// REQ-TL-parser-005: `LD#` is the abbreviated `LDATE#`.
#[spec_test(REQ_TL_parser_005)]
#[test]
fn date_spec_req_tl_005_ld_abbreviation() {
    let source = "FUNCTION fun:DATE
VAR
    dv : DATE := LD#2024-01-20;
END_VAR
fun := dv;
END_FUNCTION";
    let options = CompilerOptions::from_dialect(Dialect::Iec61131_3Ed3);
    let library = parse_program(source, &FileId::default(), &options).unwrap();
    let func = cast!(
        &library.elements[0],
        LibraryElementKind::FunctionDeclaration
    );
    let simple = cast!(
        &func.variables[0].initializer,
        InitialValueAssignmentKind::Simple
    );
    let constant = simple.initial_value.as_ref().unwrap();
    let literal = cast!(constant, ConstantKind::Date);
    assert_eq!(literal.width, TemporalWidth::Long);
}

/// The initial value of a one-variable program whose declaration is
/// `declaration`.
fn initial_value(declaration: &str) -> ConstantKind {
    let program = format!("PROGRAM main
VAR
{declaration}
END_VAR
END_PROGRAM");
    let lib = parse_text_edition3(&program);
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    let init = cast!(
        &prog.variables[0].initializer,
        InitialValueAssignmentKind::Simple
    );
    init.initial_value.clone().unwrap()
}

/// REQ-TL-parser-024: the seconds of a time of day are optional and default
/// to zero, wherever a daytime occurs.
#[spec_test(REQ_TL_parser_024)]
#[rstest]
#[case::tod("t : TIME_OF_DAY := TOD#10:00;")]
#[case::time_of_day("t : TIME_OF_DAY := TIME_OF_DAY#10:00;")]
#[case::ltod("t : LTIME_OF_DAY := LTOD#10:00;")]
fn duration_spec_req_tl_024_seconds_optional(#[case] declaration: &str) {
    let constant = initial_value(declaration);
    let literal = cast!(constant, ConstantKind::TimeOfDay);
    assert_eq!(literal.whole_milliseconds(), 36_000_000);
}

#[rstest]
#[case::dt("d : DATE_AND_TIME := DT#2024-01-02-10:00;")]
#[case::ldt("d : LDATE_AND_TIME := LDT#2024-01-02-10:00;")]
fn parse_program_when_date_and_time_omits_seconds_then_zero(
    #[case] declaration: &str,
) {
    let constant = initial_value(declaration);
    let literal = cast!(constant, ConstantKind::DateAndTime);
    assert_eq!(literal.hmsm(), (10, 0, 0, 0));
}

/// REQ-TL-parser-025: the minutes of a time of day are required.
#[spec_test(REQ_TL_parser_025)]
#[rstest]
#[case("TOD#10")]
#[case("TOD#10:")]
fn duration_spec_req_tl_025_minutes_required(#[case] literal: &str) {
    let source = duration_program(literal);
    let result = parse_program(&source, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_err(), "expected parse error for {literal}");
}

/// REQ-TL-parser-030: a duration literal keeps its full value, down to the
/// nanosecond.
#[spec_test(REQ_TL_parser_030)]
#[rstest]
#[case::microseconds("T#1us", Duration::microseconds(1))]
#[case::nanoseconds("T#500ns", Duration::nanoseconds(500))]
#[case::compound("T#1ms500us", Duration::milliseconds(1) + Duration::microseconds(500))]
fn duration_spec_req_tl_030_submillisecond_value_preserved(
    #[case] literal: &str,
    #[case] expected: Duration,
) {
    let source = duration_program(literal);
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default()).unwrap();
    assert_eq!(extract_duration(&library).interval, expected);
}
