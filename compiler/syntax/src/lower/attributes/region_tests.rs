//! Tests of the attributes that mark the edge of a region instead of
//! describing a declaration (`naming`): they are attached to nothing, need no
//! declaration after them, and have their value checked where they stand.

use super::tests::{elements_of, file};
use crate::lower::{lower_library, lower_statements};
use crate::{parse_source_file, parse_statements, ParseOptions};
use ironplc_dsl::attribute::census::declared;
use ironplc_dsl::diagnostic::Diagnostic;
use rstest::rstest;
use spec_test_macro::spec_test;

fn lowered(source: &str) -> Result<(), Diagnostic> {
    let parse = parse_source_file(source, &ParseOptions::all());
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    lower_library(&parse, &file()).map(|_| ())
}

#[rstest]
#[case::around_variables(
    "PROGRAM p VAR {attribute 'naming' := 'off'} x : INT; {attribute 'naming' := 'on'} END_VAR END_PROGRAM"
)]
#[case::around_a_block(
    "PROGRAM p {attribute 'naming' := 'off'} VAR x : INT; END_VAR {attribute 'naming' := 'on'} END_PROGRAM"
)]
#[case::before_end_struct(
    "TYPE S : STRUCT {attribute 'naming' := 'off'} a : INT; {attribute 'naming' := 'on'} END_STRUCT; END_TYPE"
)]
#[case::before_end_type(
    "TYPE {attribute 'naming' := 'off'} A : INT; {attribute 'naming' := 'on'} END_TYPE"
)]
#[case::omit_before_a_declaration(
    "PROGRAM p VAR {attribute 'naming' := 'omit'} x : INT; END_VAR END_PROGRAM"
)]
#[case::never_closed("PROGRAM p VAR {attribute 'naming' := 'off'} x : INT; END_VAR END_PROGRAM")]
#[case::at_the_end_of_the_file("PROGRAM p END_PROGRAM {attribute 'naming' := 'on'}")]
#[case::in_any_case("PROGRAM p VAR x : INT; {attribute 'NAMING' := 'OFF'} END_VAR END_PROGRAM")]
#[spec_test(REQ_ATTR_syntax_040)]
fn lower_library_when_region_marker_then_no_declaration_is_needed_after_it(#[case] source: &str) {
    assert!(lowered(source).is_ok(), "{source}");
}

#[spec_test(REQ_ATTR_syntax_041)]
fn lower_library_when_region_marker_before_a_declaration_then_it_is_not_an_attribute_of_it() {
    let source = "PROGRAM p VAR {attribute 'naming' := 'off'} {attribute 'hide'} x : INT; {attribute 'naming' := 'on'} END_VAR END_PROGRAM";
    let (_, library) = elements_of(source);
    let names: Vec<String> = declared(&library)
        .iter()
        .flat_map(|declaration| declaration.attributes.iter().map(|a| a.name.clone()))
        .collect();
    assert_eq!(names, ["hide"]);
}

#[spec_test(REQ_ATTR_syntax_042)]
fn lower_library_when_region_marker_has_a_value_the_row_does_not_read_then_p4077_at_the_pragma() {
    let source = "PROGRAM p VAR {attribute 'naming' := 'maybe'} x : INT; END_VAR END_PROGRAM";
    let error = lowered(source).expect_err("the value is refused");
    let start = source.find('{').expect("a pragma");
    assert_eq!(error.code, "P4077");
    assert_eq!(
        (error.primary.location.start, error.primary.location.end),
        (start, source.find("} x").expect("the end") + 1)
    );
    assert_eq!(
        error.primary.message,
        "The attribute 'naming' takes one of off, on, omit, but its value is 'maybe'"
    );
}

#[spec_test(REQ_ATTR_syntax_042)]
fn lower_library_when_region_marker_has_no_value_then_p4077() {
    let source = "PROGRAM p VAR x : INT; {attribute 'naming'} END_VAR END_PROGRAM";
    assert_eq!(lowered(source).expect_err("refused").code, "P4077");
}

#[spec_test(REQ_ATTR_syntax_043)]
fn lower_library_when_a_misvalued_marker_and_an_unattached_attribute_then_the_earlier() {
    let marker_first = "PROGRAM p VAR {attribute 'naming' := 'maybe'} x : INT; {attribute 'a'} END_VAR END_PROGRAM";
    let attribute_first = "PROGRAM p VAR {attribute 'a'} x : INT; END_VAR {attribute 'a'} {attribute 'naming' := 'maybe'} END_PROGRAM";
    assert_eq!(lowered(marker_first).expect_err("refused").code, "P4077");
    assert_eq!(lowered(attribute_first).expect_err("refused").code, "P0043");
}

#[spec_test(REQ_ATTR_syntax_043)]
fn lower_library_when_region_markers_and_an_attribute_before_no_declaration_then_p0043() {
    let source =
        "PROGRAM p VAR x : INT; {attribute 'naming' := 'off'} {attribute 'a'} END_VAR END_PROGRAM";
    assert_eq!(lowered(source).expect_err("refused").code, "P0043");
}

#[spec_test(REQ_ATTR_syntax_043)]
fn lower_statements_when_region_marker_then_checked_like_a_file() {
    let parse = parse_statements(
        "{attribute 'naming' := 'off'} x := 1;",
        &ParseOptions::all(),
    );
    assert!(lower_statements(&parse, &file()).is_ok());
    let parse = parse_statements("{attribute 'naming' := 'x'} x := 1;", &ParseOptions::all());
    let error = lower_statements(&parse, &file()).expect_err("refused");
    assert_eq!(error.code, "P4077");
}
