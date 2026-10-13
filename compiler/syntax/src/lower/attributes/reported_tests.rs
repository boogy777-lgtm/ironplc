//! Tests of what is reported: an attribute pragma that is attached to nothing
//! (P0043), the pragmas that are not attributes, the dialect and the text of
//! an `{IF}` branch that is not taken.

use super::tests::{elements_of, file};
use super::*;
use crate::lower::{lower_library, lower_statements};
use crate::{parse_source_file, parse_statements, ParseOptions};
use ironplc_dsl::attribute::census::declared;
use ironplc_dsl::common::Library;
use ironplc_test::fixtures::ATTRIBUTE_PROGRAM;
use rstest::rstest;
use spec_test_macro::spec_test;

/// The result of lowering `source` with the check for attributes attached to
/// nothing, as a compiler lowers a file.
fn lowered(source: &str) -> Result<Library, Diagnostic> {
    let parse = parse_source_file(source, &ParseOptions::all());
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    lower_library(&parse, &file())
}

fn reported(source: &str) -> Diagnostic {
    lowered(source).expect_err("the source is refused")
}

#[rstest]
#[case::before_end_var("PROGRAM p VAR x : INT; {attribute 'a'} END_VAR END_PROGRAM")]
#[case::before_end_type("TYPE A : INT; {attribute 'a'} END_TYPE")]
#[case::before_end_function_block(
    "FUNCTION_BLOCK f VAR x : INT; END_VAR {attribute 'a'} END_FUNCTION_BLOCK"
)]
#[case::before_end_program("PROGRAM p {attribute 'a'} END_PROGRAM")]
#[spec_test(REQ_ATTR_syntax_010)]
fn lower_library_when_attribute_before_a_closing_keyword_then_p0043(#[case] source: &str) {
    assert_eq!(reported(source).code, "P0043");
}

#[rstest]
#[case::before_statement("PROGRAM p VAR x : INT; END_VAR {attribute 'a'} x := 1; END_PROGRAM")]
#[case::inside_expression("PROGRAM p VAR x : INT; END_VAR x := 1 + {attribute 'a'} 2; END_PROGRAM")]
#[case::inside_statement(
    "PROGRAM p VAR x : INT; END_VAR IF TRUE THEN {attribute 'a'} x := 1; END_IF; END_PROGRAM"
)]
#[case::end_of_file("PROGRAM p END_PROGRAM {attribute 'a'}")]
#[case::only_pragma("{attribute 'a'}")]
#[spec_test(REQ_ATTR_syntax_011)]
fn lower_library_when_attribute_before_a_statement_or_inside_an_expression_or_at_the_end_then_p0043(
    #[case] source: &str,
) {
    assert_eq!(reported(source).code, "P0043");
}

#[rstest]
#[case::configuration(
    "{attribute 'a'} CONFIGURATION c RESOURCE r ON t PROGRAM p : q; END_RESOURCE END_CONFIGURATION"
)]
#[case::resource(
    "CONFIGURATION c {attribute 'a'} RESOURCE r ON t PROGRAM p : q; END_RESOURCE END_CONFIGURATION"
)]
#[case::task("CONFIGURATION c RESOURCE r ON t {attribute 'a'} TASK k (PRIORITY := 1); PROGRAM p WITH k : q; END_RESOURCE END_CONFIGURATION")]
#[case::program_configuration(
    "CONFIGURATION c RESOURCE r ON t {attribute 'a'} PROGRAM p : q; END_RESOURCE END_CONFIGURATION"
)]
#[case::namespace("{attribute 'a'} NAMESPACE n PROGRAM p END_PROGRAM END_NAMESPACE")]
#[case::type_of_a_declaration("PROGRAM p VAR x : {attribute 'a'} INT; END_VAR END_PROGRAM")]
#[case::name_of_a_unit("PROGRAM {attribute 'a'} p END_PROGRAM")]
#[case::accessor("FUNCTION_BLOCK f PROPERTY q : INT {attribute 'a'} GET q := 1; END_GET END_PROPERTY END_FUNCTION_BLOCK")]
#[spec_test(REQ_ATTR_syntax_012)]
fn lower_library_when_attribute_before_a_node_that_is_neither_target_nor_group_then_p0043(
    #[case] source: &str,
) {
    assert_eq!(reported(source).code, "P0043");
}

#[rstest]
#[case::access("PROGRAM p {attribute 'a'} VAR_ACCESS a : p.x : INT READ_ONLY; END_VAR END_PROGRAM")]
#[case::config("CONFIGURATION c RESOURCE r ON t PROGRAM p : q; END_RESOURCE {attribute 'a'} VAR_CONFIG r.p.x : INT; END_VAR END_CONFIGURATION")]
#[spec_test(REQ_ATTR_syntax_013)]
fn lower_library_when_attribute_before_a_block_without_variables_then_p0043(#[case] source: &str) {
    assert_eq!(reported(source).code, "P0043");
}

#[spec_test(REQ_ATTR_syntax_014)]
fn lower_library_when_attribute_attached_to_nothing_then_span_of_the_pragma_and_message_names_it() {
    let source =
        "PROGRAM p VAR x : INT; END_VAR\n  {attribute 'pack_mode' := '1'}\n  x := 1;\nEND_PROGRAM";
    let pragma = "{attribute 'pack_mode' := '1'}";
    let start = source.find(pragma).expect("the pragma");

    let diagnostic = reported(source);

    assert_eq!(
        (
            diagnostic.primary.location.start,
            diagnostic.primary.location.end
        ),
        (start, start + pragma.len())
    );
    assert_eq!(diagnostic.primary.file_id, file());
    assert!(
        diagnostic.primary.message.contains("'pack_mode'"),
        "{}",
        diagnostic.primary.message
    );
}

#[spec_test(REQ_ATTR_syntax_014)]
fn lower_library_when_several_attributes_attached_to_nothing_then_the_first_in_source_order() {
    let source = "PROGRAM p {attribute 'first'} {attribute 'second'} END_PROGRAM";

    assert!(reported(source).primary.message.contains("'first'"));
}

#[spec_test(REQ_ATTR_syntax_015)]
fn unattached_when_any_source_then_each_attribute_pragma_is_attached_or_reported_never_both() {
    let sources = [
        ATTRIBUTE_PROGRAM,
        "TYPE A : INT; {attribute 'a'} END_TYPE",
        "{attribute 'a'} TYPE {attribute 'b'} A : INT; B : INT; {attribute 'c'} END_TYPE",
        "PROGRAM p {attribute 'a'} VAR {attribute 'b'} x, y : INT; END_VAR {attribute 'c'} x := {attribute 'd'} 1; {attribute 'e'} END_PROGRAM {attribute 'f'}",
        "CONFIGURATION c {attribute 'a'} VAR_GLOBAL {attribute 'b'} g : INT; END_VAR RESOURCE r ON t PROGRAM p : q; END_RESOURCE END_CONFIGURATION",
        "PROGRAM p {attribute 'c'} VAR_ACCESS a : p.x : INT READ_ONLY; END_VAR END_PROGRAM",
        "{attribute 'a'} NAMESPACE n {attribute 'b'} PROGRAM p END_PROGRAM END_NAMESPACE",
        "{IF defined(X)} {attribute 'hidden'} {END_IF} {attribute 'a'} PROGRAM p END_PROGRAM",
        "{attribute 'a'} {attribute} {attribute ''} {info 'm'} PROGRAM p END_PROGRAM",
        "{attribute 'a'}",
        "",
    ];
    for source in sources {
        let (parse, library) = elements_of(source);
        let attached: Vec<TextRange> = declared(&library)
            .iter()
            .flat_map(|d| d.attributes.iter().map(|a| (a.span.start, a.span.end)))
            .map(|(start, end)| {
                TextRange::new(
                    u32::try_from(start).expect("offset").into(),
                    u32::try_from(end).expect("offset").into(),
                )
            })
            .collect();
        let reported: Vec<TextRange> = unattached(&parse.root)
            .iter()
            .map(|token| token.text_range())
            .collect();
        let pragmas: Vec<TextRange> = parse
            .root
            .descendants_with_tokens()
            .filter_map(|element| element.into_token())
            .filter(|token| attribute_of(token).is_some())
            .map(|token| token.text_range())
            .collect();

        for pragma in &pragmas {
            let is_attached = attached.contains(pragma);
            let is_reported = reported.contains(pragma);
            assert!(
                is_attached != is_reported,
                "{source}: {pragma:?} attached {is_attached}, reported {is_reported}"
            );
        }
        // Nothing is attached or reported that is not an attribute pragma.
        for range in attached.iter().chain(&reported) {
            assert!(pragmas.contains(range), "{source}: {range:?}");
        }
    }
}

#[spec_test(REQ_ATTR_syntax_016)]
fn lower_statements_when_attribute_in_the_list_then_p0043() {
    let parse = parse_statements("{attribute 'a'} x := 1;", &ParseOptions::all());
    assert!(parse.is_ok(), "{:?}", parse.errors);

    let error = lower_statements(&parse, &file()).expect_err("the list is refused");

    assert_eq!(error.code, "P0043");
    assert!(error.primary.message.contains("'a'"));
}

#[spec_test(REQ_ATTR_syntax_016)]
fn lower_statements_when_no_attribute_in_the_list_then_the_statements() {
    let parse = parse_statements("{info 'a'} x := 1;", &ParseOptions::all());

    let statements = lower_statements(&parse, &file()).expect("the list lowers");

    assert_eq!(statements.len(), 1);
}

#[rstest]
#[case::info_before_a_declaration("PROGRAM p VAR {info 'm'} x : INT; END_VAR END_PROGRAM")]
#[case::text_before_a_unit("{text 'm'} PROGRAM p END_PROGRAM")]
#[case::warning_before_a_statement(
    "PROGRAM p VAR x : INT; END_VAR {warning 'w'} x := 1; END_PROGRAM"
)]
#[case::error_pragma_at_the_end("PROGRAM p END_PROGRAM {error 'e'}")]
#[case::attribute_without_name("{attribute} PROGRAM p END_PROGRAM")]
#[case::attribute_with_empty_name("{attribute ''} PROGRAM p END_PROGRAM")]
#[case::unknown_pragma("PROGRAM p VAR x : INT; {foo bar} END_VAR END_PROGRAM")]
#[case::define_before_a_unit("{DEFINE X} PROGRAM p END_PROGRAM")]
#[case::branch_pragmas(
    "{IF defined(X)} PROGRAM q END_PROGRAM {ELSE} PROGRAM p END_PROGRAM {END_IF}"
)]
#[spec_test(REQ_ATTR_syntax_020)]
fn lower_library_when_pragma_is_not_an_attribute_then_trivia_attached_and_reported_nowhere(
    #[case] source: &str,
) {
    let library = lowered(source).expect("the source is accepted");

    for found in declared(&library) {
        assert!(found.attributes.is_empty(), "{source}: {}", found.name);
    }
}

#[spec_test(REQ_ATTR_syntax_030)]
fn parse_source_file_when_pragmas_not_allowed_then_a_brace_is_a_syntax_error_as_before() {
    let source = "{attribute 'a'} PROGRAM p END_PROGRAM";
    let without = parse_source_file(source, &ParseOptions::default());
    let with = parse_source_file(source, &ParseOptions::all());

    assert!(!without.is_ok());
    let error = lower_library(&without, &file()).expect_err("the source is refused");
    assert_ne!(error.code, "P0043");
    assert_eq!(
        Some(error.code),
        without.primary_diagnostic(&file()).map(|d| d.code)
    );
    assert!(with.is_ok());
    assert!(lower_library(&with, &file()).is_ok());
}

#[spec_test(REQ_ATTR_syntax_031)]
fn lower_library_when_attribute_in_a_branch_that_is_not_taken_then_hidden_attached_and_reported_nowhere(
) {
    let source = "PROGRAM p
{IF defined(X)}
{attribute 'hidden'}
VAR a : INT; END_VAR
{END_IF}
{attribute 'seen'}
VAR b : INT; END_VAR
{IF defined(X)}
{attribute 'dangling'}
{END_IF}
END_PROGRAM";

    let library = lowered(source).expect("a hidden pragma is not reported");

    let found = declared(&library);
    assert_eq!(found.len(), 2);
    let names: Vec<(String, Vec<String>)> = found
        .into_iter()
        .map(|d| {
            (
                d.name,
                d.attributes.iter().map(|a| a.name.clone()).collect(),
            )
        })
        .collect();
    assert_eq!(
        names,
        vec![
            ("p".to_string(), vec![]),
            ("b".to_string(), vec!["seen".to_string()])
        ]
    );
}

#[spec_test(REQ_ATTR_syntax_031)]
fn lower_library_when_attribute_in_a_branch_that_is_taken_then_attached() {
    let source = "{DEFINE X}
PROGRAM p
{IF defined(X)}
{attribute 'live'}
VAR a : INT; END_VAR
{END_IF}
END_PROGRAM";

    let library = lowered(source).expect("the source is accepted");

    let a = declared(&library)
        .into_iter()
        .find(|d| d.name == "a")
        .expect("a variable");
    assert_eq!(a.attributes.iter().count(), 1);
}
