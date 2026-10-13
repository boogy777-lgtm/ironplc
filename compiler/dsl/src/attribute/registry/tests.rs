//! Tests of the rows and of the readers.

use rstest::rstest;
use spec_test_macro::spec_test;

use super::test_support::{
    allows_missing, attribute, attributes, cases, invalid_values, spellings, valid_value,
};
use super::*;
use crate::attribute::census::TargetKind;
use crate::attribute::Attributes;

#[spec_test(REQ_ATTR_dsl_010)]
fn all_when_registry_then_every_row_has_a_name_a_summary_and_a_target() {
    let incomplete: Vec<&str> = ALL
        .iter()
        .filter(|row| {
            row.name.is_empty()
                || row.summary.is_empty()
                || TargetKind::ALL
                    .iter()
                    .all(|kind| !row.targets.contains(*kind))
        })
        .map(|row| row.name)
        .collect();

    assert!(incomplete.is_empty(), "incomplete rows: {incomplete:?}");
}

#[rstest]
#[case::flag(PACK_MODE.info())]
#[case::optional_one_of(SYMBOL.info())]
#[case::number(ESTIMATED_STACK_USAGE.info())]
#[case::with_aliases(NOINIT.info())]
#[spec_test(REQ_ATTR_dsl_010)]
fn all_when_typed_constant_then_the_list_holds_its_row(#[case] row: Info) {
    assert!(ALL.contains(&row));
}

#[spec_test(REQ_ATTR_dsl_011)]
fn all_when_registry_then_names_and_aliases_are_unique_without_case() {
    let mut spelled: Vec<String> = ALL
        .iter()
        .flat_map(spellings)
        .map(|spelling| spelling.to_ascii_lowercase())
        .collect();
    spelled.sort();
    let duplicates: Vec<&String> = spelled
        .iter()
        .zip(spelled.iter().skip(1))
        .filter(|(left, right)| left == right)
        .map(|(left, _)| left)
        .collect();

    assert!(
        duplicates.is_empty(),
        "spelled by more than one row: {duplicates:?}"
    );
}

#[spec_test(REQ_ATTR_dsl_011)]
fn find_when_name_or_alias_of_any_row_in_any_case_then_that_row() {
    let missed: Vec<String> = ALL
        .iter()
        .flat_map(|row| {
            spellings(row)
                .into_iter()
                .flat_map(cases)
                .filter(|spelling| find(spelling) != Some(row))
                .collect::<Vec<String>>()
        })
        .collect();

    assert!(missed.is_empty(), "not found as their row: {missed:?}");
}

#[rstest]
#[case::user_defined("my_company_tag")]
#[case::empty("")]
#[case::dash_for_underscore("pack-mode")]
#[case::prefix("hid")]
#[case::suffix("hidden")]
#[spec_test(REQ_ATTR_dsl_011)]
fn find_when_name_is_not_a_row_then_none(#[case] name: &str) {
    assert!(find(name).is_none());
}

#[spec_test(REQ_ATTR_dsl_012)]
fn types_when_registry_then_exactly_the_kinds_labelled_as_a_type() {
    let by_label: Vec<TargetKind> = TargetKind::ALL
        .iter()
        .copied()
        .filter(|kind| kind.label().ends_with(" type"))
        .collect();
    let in_types: Vec<TargetKind> = TYPES.kinds().collect();

    assert_eq!(by_label, in_types);
}

#[spec_test(REQ_ATTR_dsl_012)]
fn targets_when_every_kind_then_a_set_holds_each_and_the_kinds_fit_the_set() {
    let kinds: Vec<TargetKind> = EVERY.kinds().collect();

    assert_eq!(kinds, TargetKind::ALL);
    assert!(TargetKind::ALL.len() <= 32);
}

#[rstest]
#[case::every(EVERY, "every declaration")]
#[case::types_alone(TYPES, "any type declaration")]
#[case::kinds(Targets::of(&[TargetKind::Enumeration, TargetKind::Variable]), "enumeration type, variable")]
#[case::types_and_more(TYPES.and(&[TargetKind::Method]), "any type declaration, method")]
#[spec_test(REQ_ATTR_dsl_012)]
fn describe_when_set_of_kinds_then_the_labels_with_types_said_as_one(
    #[case] targets: Targets,
    #[case] expected: &str,
) {
    assert_eq!(targets.describe(), expected);
}

#[rstest]
#[case::empty(attributes(vec![]))]
#[case::other_attribute(attributes(vec![attribute("hide", None, 0)]))]
#[spec_test(REQ_ATTR_dsl_020)]
fn of_when_attribute_is_absent_then_none(#[case] present: Attributes) {
    assert_eq!(PACK_MODE.of(&present), None);
}

#[rstest]
#[case::name("noinit")]
#[case::alias_with_underscore("no_init")]
#[case::alias_with_dash("no-init")]
#[case::upper("NOINIT")]
#[case::alias_upper("NO-INIT")]
#[case::mixed("NoInit")]
#[spec_test(REQ_ATTR_dsl_020)]
fn of_when_name_or_alias_in_any_case_then_the_value(#[case] written: &str) {
    let present = attributes(vec![attribute(written, None, 0)]);

    assert_eq!(NOINIT.of(&present), Some(Flag));
}

#[rstest]
#[case("0", PackMode::Aligned)]
#[case("1", PackMode::Unaligned)]
#[case("2", PackMode::Bytes2)]
#[case("4", PackMode::Bytes4)]
#[case(" 8 ", PackMode::Bytes8)]
#[spec_test(REQ_ATTR_dsl_020)]
fn of_when_pack_mode_has_a_valid_value_then_the_typed_value(
    #[case] value: &str,
    #[case] expected: PackMode,
) {
    let present = attributes(vec![attribute("Pack_Mode", Some(value), 0)]);

    assert_eq!(PACK_MODE.of(&present), Some(expected));
}

#[rstest]
#[case::not_listed(Some("3"))]
#[case::empty(Some(""))]
#[case::missing(None)]
#[spec_test(REQ_ATTR_dsl_020)]
fn of_when_value_is_not_valid_then_none(#[case] value: Option<&str>) {
    let present = attributes(vec![attribute("pack_mode", value, 0)]);

    assert_eq!(PACK_MODE.of(&present), None);
}

#[spec_test(REQ_ATTR_dsl_020)]
fn of_when_attribute_is_written_twice_then_the_value_of_the_first() {
    let present = attributes(vec![
        attribute("pack_mode", Some("2"), 0),
        attribute("PACK_MODE", Some("8"), 1),
    ]);

    assert_eq!(PACK_MODE.of(&present), Some(PackMode::Bytes2));
}

#[spec_test(REQ_ATTR_dsl_020)]
fn of_when_number_text_and_optional_text_rows_then_each_gives_its_type() {
    let number = attributes(vec![attribute("estimated-stack-usage", Some("64"), 0)]);
    let text = attributes(vec![attribute("obsolete", Some("use b"), 0)]);
    let given = attributes(vec![attribute("conditionalshow", Some("x"), 0)]);
    let missing = attributes(vec![attribute("conditionalshow", None, 0)]);

    assert_eq!(ESTIMATED_STACK_USAGE.of(&number), Some(Number(64)));
    assert_eq!(OBSOLETE.of(&text), Some(Text("use b".to_string())));
    assert_eq!(
        CONDITIONALSHOW.of(&given),
        Some(OptionalText(Some("x".to_string())))
    );
    assert_eq!(CONDITIONALSHOW.of(&missing), Some(OptionalText(None)));
}

#[rstest]
#[case::read("read", Some(Optional(Some(SymbolAccess::Read))))]
#[case::readwrite_in_other_case("ReadWrite", Some(Optional(Some(SymbolAccess::ReadWrite))))]
#[case::not_a_choice("rw", None)]
#[spec_test(REQ_ATTR_dsl_020)]
fn of_when_symbol_has_a_value_then_the_access(
    #[case] value: &str,
    #[case] expected: Option<Optional<SymbolAccess>>,
) {
    let present = attributes(vec![attribute("symbol", Some(value), 0)]);

    assert_eq!(SYMBOL.of(&present), expected);
}

#[spec_test(REQ_ATTR_dsl_020)]
fn of_when_symbol_has_no_value_then_present_without_access() {
    let present = attributes(vec![attribute("symbol", None, 0)]);

    assert_eq!(SYMBOL.of(&present), Some(Optional(None)));
}

#[spec_test(REQ_ATTR_dsl_021)]
fn accepts_when_every_row_then_a_valid_value_is_read_and_an_invalid_one_is_not() {
    let wrong: Vec<String> = ALL
        .iter()
        .flat_map(|row| {
            let valid = (!row.accepts(valid_value(row.value)))
                .then(|| format!("{} refuses {:?}", row.name, valid_value(row.value)));
            let invalid = invalid_values(row.value)
                .iter()
                .filter(|value| row.accepts(**value))
                .map(|value| format!("{} accepts {value:?}", row.name));
            valid.into_iter().chain(invalid).collect::<Vec<String>>()
        })
        .collect();

    assert!(wrong.is_empty(), "{wrong:?}");
}

#[spec_test(REQ_ATTR_dsl_021)]
fn accepts_when_row_takes_one_of_texts_then_each_text_in_any_case_and_with_spaces() {
    let refused: Vec<String> = ALL
        .iter()
        .flat_map(|row| {
            let texts: &[&str] = match row.value {
                ValueKind::OneOf(texts) | ValueKind::OptionalOneOf(texts) => texts,
                ValueKind::Flag | ValueKind::Number | ValueKind::Text | ValueKind::OptionalText => {
                    &[]
                }
            };
            texts
                .iter()
                .flat_map(|text| cases(text))
                .map(|text| format!("  {text} "))
                .filter(|text| !row.accepts(Some(text)))
                .map(|text| format!("{} refuses {text:?}", row.name))
                .collect::<Vec<String>>()
        })
        .collect();

    assert!(refused.is_empty(), "{refused:?}");
}

#[spec_test(REQ_ATTR_dsl_021)]
fn accepts_when_row_takes_an_optional_value_then_a_missing_value_is_read() {
    let refused: Vec<&str> = ALL
        .iter()
        .filter(|row| allows_missing(row.value))
        .filter(|row| !row.accepts(None))
        .map(|row| row.name)
        .collect();

    assert!(refused.is_empty(), "{refused:?}");
}
