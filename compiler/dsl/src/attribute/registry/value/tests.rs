//! Tests of the types of value: the valid and the invalid values of each kind.

use rstest::rstest;
use spec_test_macro::spec_test;

use super::*;

#[rstest]
#[case::missing(None, true)]
#[case::empty(Some(""), false)]
#[case::given(Some("yes"), false)]
#[spec_test(REQ_ATTR_dsl_022)]
fn parse_when_flag_then_only_a_missing_value_is_read(
    #[case] text: Option<&str>,
    #[case] read: bool,
) {
    assert_eq!(Flag::parse(text).is_some(), read);
}

#[rstest]
#[case::zero("0", Some(0))]
#[case::plain("64", Some(64))]
#[case::with_spaces("  12 ", Some(12))]
#[case::largest("4294967295", Some(u32::MAX))]
#[case::too_large("4294967296", None)]
#[case::negative("-1", None)]
#[case::plus("+1", None)]
#[case::fraction("1.5", None)]
#[case::word("seven", None)]
#[case::empty("", None)]
#[case::only_spaces("   ", None)]
#[spec_test(REQ_ATTR_dsl_022)]
fn parse_when_number_then_an_unsigned_decimal_that_fits_u32(
    #[case] text: &str,
    #[case] expected: Option<u32>,
) {
    assert_eq!(Number::parse(Some(text)), expected.map(Number));
}

#[spec_test(REQ_ATTR_dsl_022)]
fn parse_when_number_is_missing_then_none() {
    assert_eq!(Number::parse(None), None);
}

#[rstest]
#[case::text("use b instead", Some("use b instead"))]
#[case::kept_as_written(" use b ", Some(" use b "))]
#[case::empty("", None)]
#[case::only_spaces("  ", None)]
#[spec_test(REQ_ATTR_dsl_022)]
fn parse_when_text_then_any_text_that_is_not_empty(
    #[case] text: &str,
    #[case] expected: Option<&str>,
) {
    assert_eq!(
        Text::parse(Some(text)),
        expected.map(|text| Text(text.to_string()))
    );
}

#[spec_test(REQ_ATTR_dsl_022)]
fn parse_when_text_is_missing_then_none() {
    assert_eq!(Text::parse(None), None);
}

#[rstest]
#[case::missing(None, None)]
#[case::empty(Some(""), Some(""))]
#[case::given(Some("x y"), Some("x y"))]
#[spec_test(REQ_ATTR_dsl_022)]
fn parse_when_optional_text_then_any_value_or_none(
    #[case] text: Option<&str>,
    #[case] expected: Option<&str>,
) {
    assert_eq!(
        OptionalText::parse(text),
        Some(OptionalText(expected.map(str::to_string)))
    );
}

#[rstest]
#[case("bin", DisplayMode::Binary)]
#[case("BINARY", DisplayMode::Binary)]
#[case("dec", DisplayMode::Decimal)]
#[case("Decimal", DisplayMode::Decimal)]
#[case(" hex ", DisplayMode::Hexadecimal)]
#[case("HEXADECIMAL", DisplayMode::Hexadecimal)]
#[spec_test(REQ_ATTR_dsl_022)]
fn parse_when_one_of_with_several_texts_for_a_choice_then_each_is_read(
    #[case] text: &str,
    #[case] expected: DisplayMode,
) {
    assert_eq!(DisplayMode::parse(Some(text)), Some(expected));
}

#[rstest]
#[case::missing(None)]
#[case::empty(Some(""))]
#[case::not_listed(Some("octal"))]
#[case::prefix(Some("he"))]
#[spec_test(REQ_ATTR_dsl_022)]
fn parse_when_one_of_and_the_value_is_not_listed_or_missing_then_none(#[case] text: Option<&str>) {
    assert_eq!(DisplayMode::parse(text), None);
}

#[rstest]
#[case("UTF8")]
#[case("utf-8")]
#[spec_test(REQ_ATTR_dsl_022)]
fn parse_when_one_of_with_one_choice_then_both_texts_are_that_choice(#[case] text: &str) {
    assert_eq!(
        MonitoringEncoding::parse(Some(text)),
        Some(MonitoringEncoding::Utf8)
    );
}

#[rstest]
#[case::missing(None, Some(None))]
#[case::none(Some("none"), Some(Some(SymbolAccess::NoAccess)))]
#[case::readwrite(Some("READWRITE"), Some(Some(SymbolAccess::ReadWrite)))]
#[case::empty(Some(""), None)]
#[case::not_listed(Some("all"), None)]
#[spec_test(REQ_ATTR_dsl_022)]
fn parse_when_optional_one_of_then_a_missing_value_or_one_of_the_texts(
    #[case] text: Option<&str>,
    #[case] expected: Option<Option<SymbolAccess>>,
) {
    assert_eq!(
        Optional::<SymbolAccess>::parse(text),
        expected.map(Optional)
    );
}

#[rstest]
#[case::flag(Flag::KIND, "no value")]
#[case::one_of(MonitoringMode::KIND, "one of variable, call")]
#[case::optional_one_of(Optional::<MonitoringMode>::KIND, "no value, or one of variable, call")]
#[case::number(Number::KIND, "an unsigned integer")]
#[case::text(Text::KIND, "a text that is not empty")]
#[case::optional_text(OptionalText::KIND, "any text, or no value")]
#[spec_test(REQ_ATTR_dsl_022)]
fn describe_when_value_kind_then_what_the_attribute_takes(
    #[case] kind: ValueKind,
    #[case] expected: &str,
) {
    assert_eq!(kind.describe(), expected);
}
