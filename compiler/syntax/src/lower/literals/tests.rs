use super::*;
use crate::lower::{disposition, Area, Disposition, INTERNAL_ERROR, NOT_IMPLEMENTED};
use crate::{parse_source_file, parse_statements, ParseOptions};
use ironplc_dsl::core::FileId;
use ironplc_problems::Problem;
use time::Duration;

fn file() -> FileId {
    FileId::from_string("t.st")
}

/// Every flag on, so that the dialect keywords (`LTIME`, `BIT`) are literal
/// prefixes.
fn all() -> ParseOptions {
    ParseOptions::all()
}

/// Where the literals are written: the default of a subrange type declaration, a
/// constant position, where a sign belongs to the literal (in an expression it
/// is an operator).
const CONTEXT: &str = "TYPE t : INT(1..2) := ";

/// The first literal node of `source` written in [`CONTEXT`], ignoring any
/// error the parse reported, so that malformed literals reach the rules.
fn literal_node(source: &str, options: &ParseOptions) -> Option<SyntaxNode> {
    parse_source_file(&format!("{CONTEXT}{source}; END_TYPE"), options)
        .root
        .descendants()
        .find(|node| node.kind() == crate::SyntaxKind::Initializer)?
        .descendants()
        .find(|node| disposition(node.kind()) == Disposition::Lowered(Area::Literal))
}

fn lower_with(source: &str, options: &ParseOptions) -> Result<ConstantKind, Diagnostic> {
    let node = literal_node(source, options).ok_or_else(Diagnostic::internal_error)?;
    lower_constant(&LowerCx::new(file()), &node)
}

fn lower(source: &str) -> ConstantKind {
    lower_with(source, &all()).expect("the literal lowers")
}

fn code(result: Result<ConstantKind, Diagnostic>) -> Option<String> {
    result.err().map(|diagnostic| diagnostic.code)
}

fn int(source: &str) -> Option<(i128, Option<IntegerTypeName>)> {
    match lower(source) {
        ConstantKind::IntegerLiteral(literal) => {
            Some((i128::try_from(literal.value).ok()?, literal.data_type))
        }
        _ => None,
    }
}

fn real(source: &str) -> Option<(f64, Option<RealTypeName>)> {
    match lower(source) {
        ConstantKind::RealLiteral(literal) => Some((literal.value, literal.data_type)),
        _ => None,
    }
}

fn text(source: &str) -> Option<(String, StringType)> {
    match lower(source) {
        ConstantKind::CharacterString(literal) => {
            Some((literal.value.iter().collect(), literal.width))
        }
        _ => None,
    }
}

/// The characters of a string literal.
fn chars(source: &str) -> Option<String> {
    text(source).map(|(chars, _)| chars)
}

fn duration_of(source: &str) -> Option<(Duration, TemporalWidth)> {
    match lower(source) {
        ConstantKind::Duration(literal) => Some((literal.interval, literal.width)),
        _ => None,
    }
}

#[test]
fn lower_constant_when_decimal_integer_then_value_and_no_type() {
    assert_eq!(int("42"), Some((42, None)));
    assert_eq!(int("1_000"), Some((1000, None)));
    assert_eq!(int("0"), Some((0, None)));
}

#[test]
fn lower_constant_when_signed_integer_then_sign_in_the_value() {
    assert_eq!(int("-7"), Some((-7, None)));
    assert_eq!(int("+7"), Some((7, None)));
}

#[test]
fn lower_constant_when_based_integer_then_value_of_the_base() {
    assert_eq!(int("16#FF"), Some((255, None)));
    assert_eq!(int("16#FF_FF"), Some((65535, None)));
    assert_eq!(int("8#17"), Some((15, None)));
    assert_eq!(int("2#1010_1010"), Some((170, None)));
    assert_eq!(int("10#123"), Some((123, None)));
}

#[test]
fn lower_constant_when_typed_integer_then_data_type_of_the_prefix() {
    let prefixes = [
        ("SINT", IntegerTypeName::SINT),
        ("INT", IntegerTypeName::INT),
        ("DINT", IntegerTypeName::DINT),
        ("LINT", IntegerTypeName::LINT),
        ("USINT", IntegerTypeName::USINT),
        ("UINT", IntegerTypeName::UINT),
        ("UDINT", IntegerTypeName::UDINT),
        ("ULINT", IntegerTypeName::ULINT),
    ];
    for (prefix, data_type) in prefixes {
        assert_eq!(
            int(&format!("{prefix}#5")),
            Some((5, Some(data_type.clone())))
        );
        assert_eq!(int(&format!("{prefix}#-5")), Some((-5, Some(data_type))));
    }
    assert_eq!(int("int#16#FF"), Some((255, Some(IntegerTypeName::INT))));
    assert_eq!(int("DINT#10#7"), Some((7, Some(IntegerTypeName::DINT))));
}

#[test]
fn lower_constant_when_largest_integer_then_value_kept_and_one_more_is_a_syntax_error() {
    let largest = u128::MAX.to_string();
    let literal = match lower(&largest) {
        ConstantKind::IntegerLiteral(literal) => Some(literal.value.value.value),
        _ => None,
    };
    assert_eq!(literal, Some(u128::MAX));
    let too_large = "340282366920938463463374607431768211456";
    assert_eq!(
        code(lower_with(too_large, &all())),
        Some(Problem::SyntaxError.code().to_string())
    );
}

#[test]
fn lower_constant_when_real_then_value_and_no_type() {
    assert_eq!(real("1.5"), Some((1.5, None)));
    assert_eq!(real("-1.5"), Some((-1.5, None)));
    assert_eq!(real("+2.25"), Some((2.25, None)));
    assert_eq!(real("1.5E-3"), Some((0.0015, None)));
    assert_eq!(real("2e3"), Some((2000.0, None)));
    assert_eq!(real("1_0.5"), Some((10.5, None)));
}

#[test]
fn lower_constant_when_typed_real_then_data_type_of_the_prefix() {
    assert_eq!(real("REAL#3.5"), Some((3.5, Some(RealTypeName::REAL))));
    assert_eq!(real("LREAL#-0.5"), Some((-0.5, Some(RealTypeName::LREAL))));
    assert_eq!(real("real#+1.0"), Some((1.0, Some(RealTypeName::REAL))));
}

#[test]
fn lower_constant_when_negative_zero_then_the_sign_is_kept() {
    let value = real("-0.0").map(|(value, _)| value);
    assert!(value.is_some_and(|value| value == 0.0 && value.is_sign_negative()));
}

#[test]
fn lower_constant_when_boolean_words_then_the_value() {
    for (source, expected) in [
        ("TRUE", Boolean::True),
        ("FALSE", Boolean::False),
        ("true", Boolean::True),
        ("False", Boolean::False),
    ] {
        assert_eq!(
            lower(source),
            ConstantKind::Boolean(BooleanLiteral::new(expected)),
            "{source}"
        );
    }
}

#[test]
fn lower_constant_when_typed_boolean_then_the_value() {
    for (source, expected) in [
        ("BOOL#1", Boolean::True),
        ("BOOL#0", Boolean::False),
        ("BOOL#TRUE", Boolean::True),
        ("BOOL#false", Boolean::False),
        ("BIT#1", Boolean::True),
        ("BIT#0", Boolean::False),
    ] {
        assert_eq!(
            lower(source),
            ConstantKind::Boolean(BooleanLiteral::new(expected)),
            "{source}"
        );
    }
}

#[test]
fn lower_constant_when_single_quoted_string_then_string_width_and_text() {
    assert_eq!(text("'abc'"), Some(("abc".to_string(), StringType::String)));
    assert_eq!(text("''"), Some((String::new(), StringType::String)));
}

#[test]
fn lower_constant_when_double_quoted_string_then_wstring_width() {
    assert_eq!(
        text("\"abc\""),
        Some(("abc".to_string(), StringType::WString))
    );
}

#[test]
fn lower_constant_when_escapes_then_decoded_by_the_shared_table() {
    assert_eq!(chars("'a$$b$41$L'"), Some("a$bA\n".to_string()));
    assert_eq!(chars("'it$'s'"), Some("it's".to_string()));
    assert_eq!(chars("'$80'"), Some("\u{20AC}".to_string()));
    assert_eq!(chars("'$U000020AC'"), Some("\u{20AC}".to_string()));
    assert_eq!(chars("\"$0041$\"\""), Some("A\"".to_string()));
    assert_eq!(chars("\"$U0001F600\""), Some("\u{1F600}".to_string()));
}

#[test]
fn lower_constant_when_typed_string_then_width_follows_the_delimiter() {
    assert_eq!(
        text("STRING#'x'"),
        Some(("x".to_string(), StringType::String))
    );
    assert_eq!(
        text("WSTRING#\"x\""),
        Some(("x".to_string(), StringType::WString))
    );
    assert_eq!(
        text("UTF8#'x'"),
        Some(("x".to_string(), StringType::String))
    );
    assert_eq!(
        text("utf8#'x'"),
        Some(("x".to_string(), StringType::String))
    );
    assert_eq!(
        text("UCHAR#'x'"),
        Some(("x".to_string(), StringType::String))
    );
    assert_eq!(
        text("__XSTRING#\"x\""),
        Some(("x".to_string(), StringType::WString))
    );
}

#[test]
fn lower_constant_when_duration_then_interval_and_short_width() {
    let cases = [
        ("T#5s", Duration::seconds(5)),
        ("t#5s", Duration::seconds(5)),
        ("TIME#100ms", Duration::milliseconds(100)),
        ("T#1m30s", Duration::seconds(90)),
        ("T#1.5s", Duration::milliseconds(1500)),
        (
            "T#1d2h3m4s5ms",
            Duration::days(1)
                + Duration::hours(2)
                + Duration::minutes(3)
                + Duration::seconds(4)
                + Duration::milliseconds(5),
        ),
        ("T#1h_30m", Duration::minutes(90)),
        ("T#5S", Duration::seconds(5)),
        ("T#250us", Duration::microseconds(250)),
        ("T#7ns", Duration::nanoseconds(7)),
        ("T#1_000ms", Duration::seconds(1)),
        ("T#-5s", Duration::seconds(-5)),
        ("T#-1m30s", Duration::seconds(-90)),
    ];
    for (source, expected) in cases {
        assert_eq!(
            duration_of(source),
            Some((expected, TemporalWidth::Short)),
            "{source}"
        );
    }
}

#[test]
fn lower_constant_when_long_duration_prefix_then_long_width() {
    for source in ["LTIME#5s", "LT#5s", "lt#5s", "ltime#5s"] {
        assert_eq!(
            duration_of(source),
            Some((Duration::seconds(5), TemporalWidth::Long)),
            "{source}"
        );
    }
}

#[test]
fn lower_constant_when_time_of_day_then_clock_and_width() {
    let clock = |source: &str| match lower(source) {
        ConstantKind::TimeOfDay(literal) => Some((literal.hmsm(), literal.width)),
        _ => None,
    };
    assert_eq!(
        clock("TOD#10:30"),
        Some(((10, 30, 0, 0), TemporalWidth::Short))
    );
    assert_eq!(
        clock("TIME_OF_DAY#23:59:59"),
        Some(((23, 59, 59, 0), TemporalWidth::Short))
    );
    assert_eq!(
        clock("TOD#10:00:00.250"),
        Some(((10, 0, 0, 250_000), TemporalWidth::Short))
    );
    assert_eq!(
        clock("LTOD#00:00:01"),
        Some(((0, 0, 1, 0), TemporalWidth::Long))
    );
    assert_eq!(
        clock("LTIME_OF_DAY#12:00"),
        Some(((12, 0, 0, 0), TemporalWidth::Long))
    );
}

#[test]
fn lower_constant_when_date_then_calendar_date_and_width() {
    let day = |source: &str| match lower(source) {
        ConstantKind::Date(literal) => Some((literal.ymd(), literal.width)),
        _ => None,
    };
    assert_eq!(
        day("D#2024-02-29"),
        Some(((2024, 2, 29), TemporalWidth::Short))
    );
    assert_eq!(
        day("DATE#1970-01-01"),
        Some(((1970, 1, 1), TemporalWidth::Short))
    );
    assert_eq!(
        day("LDATE#2024-12-31"),
        Some(((2024, 12, 31), TemporalWidth::Long))
    );
    assert_eq!(
        day("ld#2000-01-02"),
        Some(((2000, 1, 2), TemporalWidth::Long))
    );
}

#[test]
fn lower_constant_when_date_and_time_then_both_parts_and_width() {
    let stamp = |source: &str| match lower(source) {
        ConstantKind::DateAndTime(literal) => Some((literal.ymd(), literal.hmsm(), literal.width)),
        _ => None,
    };
    assert_eq!(
        stamp("DT#2024-01-15-10:30:00"),
        Some(((2024, 1, 15), (10, 30, 0, 0), TemporalWidth::Short))
    );
    assert_eq!(
        stamp("DATE_AND_TIME#2024-01-15-10:30"),
        Some(((2024, 1, 15), (10, 30, 0, 0), TemporalWidth::Short))
    );
    assert_eq!(
        stamp("LDT#2024-01-15-23:59:59.5"),
        Some(((2024, 1, 15), (23, 59, 59, 500_000), TemporalWidth::Long))
    );
    assert_eq!(
        stamp("LDATE_AND_TIME#2000-02-29-00:00:00"),
        Some(((2000, 2, 29), (0, 0, 0, 0), TemporalWidth::Long))
    );
}

#[test]
fn lower_constant_when_bit_string_then_prefix_and_value() {
    let bits = |source: &str| match lower(source) {
        ConstantKind::BitStringLiteral(literal) => Some((literal.value.value, literal.data_type)),
        _ => None,
    };
    assert_eq!(
        bits("BYTE#16#FF"),
        Some((255, Some(BitStringTypeName::BYTE)))
    );
    assert_eq!(
        bits("WORD#2#1010"),
        Some((10, Some(BitStringTypeName::WORD)))
    );
    assert_eq!(
        bits("DWORD#8#17"),
        Some((15, Some(BitStringTypeName::DWORD)))
    );
    assert_eq!(
        bits("LWORD#255"),
        Some((255, Some(BitStringTypeName::LWORD)))
    );
    assert_eq!(bits("byte#1_0"), Some((10, Some(BitStringTypeName::BYTE))));
}

#[test]
fn lower_constant_when_case_label_bit_string_then_no_type() {
    let parse = parse_statements(
        "CASE x OF 16#FF: y := 1; 2#11: y := 2; END_CASE;",
        &ParseOptions::default(),
    );
    assert!(parse.is_ok(), "{:?}", parse.errors);
    let cx = LowerCx::new(file());
    let values: Vec<Option<(u128, Option<BitStringTypeName>)>> = parse
        .root
        .descendants()
        .filter(|node| node.kind() == K::BitStringLiteral)
        .map(|node| match lower_constant(&cx, &node) {
            Ok(ConstantKind::BitStringLiteral(literal)) => {
                Some((literal.value.value, literal.data_type))
            }
            _ => None,
        })
        .collect();
    assert_eq!(values, vec![Some((255, None)), Some((3, None))]);
}

#[test]
fn lower_constant_when_literal_has_a_prefix_and_sign_then_span_covers_all_of_it() {
    let constant = lower("INT#-5");
    let span = ironplc_dsl::core::Located::span(&constant);
    assert_eq!((span.start, span.end), (CONTEXT.len(), CONTEXT.len() + 6));
    assert_eq!(span.file_id, file());
}

#[test]
fn lower_constant_when_every_family_then_the_span_is_the_node_range_in_the_file() {
    use ironplc_dsl::core::Located;
    let sources = [
        "42",
        "16#FF",
        "-1.5",
        "TRUE",
        "'abc'",
        "\"abc\"",
        "T#1m30s",
        "TOD#10:00",
        "D#2024-01-01",
        "DT#2024-01-01-10:00",
        "BYTE#16#FF",
        "STRING#'a'",
        "LTIME#5s",
    ];
    for source in sources {
        let span = lower(source).span();
        assert_eq!(
            (span.start, span.end),
            (CONTEXT.len(), CONTEXT.len() + source.len()),
            "{source}"
        );
        assert_eq!(span.file_id, file(), "{source}");
    }
}

#[test]
fn lower_constant_when_literal_inside_a_statement_then_offsets_are_into_the_whole_text() {
    let source = "x := T#5s;";
    let parse = parse_statements(source, &ParseOptions::default());
    let cx = LowerCx::new(file());
    let node = parse
        .root
        .descendants()
        .find(|node| node.kind() == K::DurationLiteral)
        .expect("a duration");
    let constant = lower_constant(&cx, &node).expect("lowers");
    let span = ironplc_dsl::core::Located::span(&constant);
    assert_eq!(&source[span.start..span.end], "T#5s");
}

#[test]
fn lower_constant_when_node_is_not_a_literal_then_not_implemented_or_internal_error() {
    let cx = LowerCx::new(file());
    let parse = parse_source_file("PROGRAM p\nEND_PROGRAM\n", &ParseOptions::default());
    let pending = parse
        .root
        .descendants()
        .find(|node| node.kind() == K::ProgramDecl)
        .expect("a declaration");
    assert_eq!(
        code(lower_constant(&cx, &pending)),
        Some(NOT_IMPLEMENTED.to_string())
    );
    assert_eq!(
        code(lower_constant(&cx, &parse.root)),
        Some(INTERNAL_ERROR.to_string())
    );
}

#[test]
fn lower_constant_when_duration_value_too_large_then_syntax_error_naming_the_literal() {
    let diagnostic = lower_with("T#99999999999999999999s", &all()).err();
    assert_eq!(
        diagnostic
            .as_ref()
            .map(|diagnostic| diagnostic.code.as_str()),
        Some(Problem::SyntaxError.code())
    );
    assert!(diagnostic
        .map(|diagnostic| diagnostic
            .primary
            .message
            .contains("T#99999999999999999999s"))
        .unwrap_or(false));
}

#[test]
fn lower_constant_when_duration_at_last_representable_then_that_duration() {
    assert_eq!(
        duration_of("T#9223372036854775807.999999999s"),
        Some((Duration::new(i64::MAX, 999_999_999), TemporalWidth::Short))
    );
    assert_eq!(
        duration_of("T#106751991167300d").map(|(interval, _)| interval.whole_seconds()),
        Some(106_751_991_167_300 * 86_400)
    );
    assert_eq!(
        duration_of("T#18446744073709551615ns").map(|(interval, _)| interval.whole_nanoseconds()),
        Some(i128::from(u64::MAX))
    );
}

#[test]
fn lower_constant_when_duration_past_last_representable_then_p2039_over_the_literal() {
    // The crash report's input, the whole part past `i64`, and parts that
    // sum past the longest duration.
    for (source, type_name) in [
        ("T#9223372036854775807d", "TIME"),
        ("LTIME#9223372036854775807d", "LTIME"),
        ("T#9223372036854775808s", "TIME"),
        ("T#18446744073709551615s", "TIME"),
        ("T#106751991167300d23h", "TIME"),
        ("T#-9223372036854775807d", "TIME"),
    ] {
        let diagnostic = lower_with(source, &all()).unwrap_err();
        assert_eq!(diagnostic.code, Problem::DurationLiteralOutOfRange.code());
        assert_eq!(
            diagnostic.primary.message,
            format!("Constant '{source}' is outside the range of type '{type_name}'"),
        );
        let start = CONTEXT.len();
        assert_eq!(
            (
                diagnostic.primary.location.start,
                diagnostic.primary.location.end
            ),
            (start, start + source.len()),
            "{source}"
        );
    }
}

#[test]
fn lower_constant_when_value_does_not_exist_then_syntax_error_over_the_literal() {
    // The parser reports each of these; the rule still answers with the same
    // problem code, so a tree that was not checked cannot yield a wrong value.
    for source in [
        "TOD#24:00",
        "TOD#10:60",
        "TOD#10:00:60",
        "D#2023-02-29",
        "D#2024-13-01",
        "D#10000-01-01",
        "DT#2024-01-15-25:00",
        "DT#2024-02-30-10:00",
        "T#30s1m",
        "T#1.5m30s",
        "T#5x",
    ] {
        let diagnostic = lower_with(source, &all()).err();
        assert_eq!(
            diagnostic
                .as_ref()
                .map(|diagnostic| diagnostic.code.as_str()),
            Some(Problem::SyntaxError.code()),
            "{source}"
        );
        let range = diagnostic.map(|diagnostic| {
            (
                diagnostic.primary.location.start,
                diagnostic.primary.location.end,
            )
        });
        assert_eq!(
            range,
            Some((CONTEXT.len(), CONTEXT.len() + source.len())),
            "{source}"
        );
    }
}

#[test]
fn lower_constant_when_malformed_literal_then_an_answer_and_no_panic() {
    for source in [
        "T#",
        "T# 5s",
        "T#-",
        "INT#",
        "INT#-",
        "REAL#",
        "BOOL#",
        "BOOL#2",
        "BIT#",
        "STRING#",
        "D#",
        "D#2024",
        "D#2024-01",
        "DT#2024-01-01",
        "DT#2024-01-01-",
        "TOD#",
        "TOD#10",
        "TOD#10:",
        "BYTE#",
        "10#",
        "16#",
        "'abc",
        "\"abc",
        "T#1.s",
        "+",
        "-",
        "1.",
        ".5",
        "T#5s5s",
    ] {
        for node in parse_source_file(&format!("{CONTEXT}{source}; END_TYPE"), &all())
            .root
            .descendants()
            .filter(|node| disposition(node.kind()) == Disposition::Lowered(Area::Literal))
        {
            let _ = lower_constant(&LowerCx::new(file()), &node);
        }
    }
}

#[test]
fn lower_constant_when_literal_of_each_kind_then_disposition_names_the_literal_area() {
    for source in [
        "42",
        "1.5",
        "TRUE",
        "'a'",
        "T#1s",
        "TOD#1:00",
        "D#2024-01-01",
        "DT#2024-01-01-10:00",
        "BYTE#1",
    ] {
        let node = literal_node(source, &all());
        assert!(node.is_some(), "{source}");
        assert!(lower_with(source, &all()).is_ok(), "{source}");
    }
}
