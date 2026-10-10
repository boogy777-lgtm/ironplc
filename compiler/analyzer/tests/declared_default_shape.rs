//! A variable declared against a named type is the declaration its kind of
//! type implies, carrying the value the type declares: what `x : STRING[10] :=
//! 'abc'` is for `x : Name` where `Name` is a string type of that length with
//! that default, however many aliases lie between the two.

#![allow(
    clippy::unwrap_used,
    reason = "test target: panicking helpers are sanctioned in tests"
)]

use ironplc_analyzer::stages::analyze;
use ironplc_dsl::common::{InitialValueAssignmentKind, LibraryElementKind, StringType};
use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use ironplc_parser::parse_program;
use rstest::rstest;

/// The initializer of the first variable of the program `p`, after analysis,
/// and the codes of the diagnostics the analysis reports.
fn analysed(text: &str) -> (InitialValueAssignmentKind, Vec<String>) {
    let options = CompilerOptions::default();
    let library = parse_program(text, &FileId::default(), &options).unwrap();
    let (library, context) = analyze(&[&library], &options).unwrap();
    let codes = context
        .diagnostics()
        .iter()
        .map(|d| d.code.clone())
        .collect();
    let initializer = library
        .elements
        .iter()
        .find_map(|element| match element {
            LibraryElementKind::ProgramDeclaration(program) => {
                program.variables.first().map(|v| v.initializer.clone())
            }
            _ => None,
        })
        .unwrap();
    (initializer, codes)
}

/// A string initializer as `width length value`, or what else it is.
fn described(initializer: &InitialValueAssignmentKind) -> String {
    match initializer {
        InitialValueAssignmentKind::String(string) => {
            let width = match string.width {
                StringType::String => "STRING",
                StringType::WString => "WSTRING",
            };
            let length = string
                .length
                .as_ref()
                .and_then(|length| length.as_integer())
                .map(|length| length.value.to_string())
                .unwrap_or_default();
            let value: String = string
                .initial_value
                .iter()
                .flat_map(|literal| literal.value.iter())
                .collect();
            format!("{width}[{length}] '{value}'")
        }
        other => format!("{other:?}"),
    }
}

#[rstest]
#[case::with_length_and_value(
    "TYPE S1 : STRING[10] := 'abc'; END_TYPE PROGRAM p VAR v : S1; END_VAR END_PROGRAM",
    "STRING[10] 'abc'"
)]
#[case::without_length(
    "TYPE S1 : STRING := 'abc'; END_TYPE PROGRAM p VAR v : S1; END_VAR END_PROGRAM",
    "STRING[] 'abc'"
)]
#[case::without_value(
    "TYPE S1 : STRING[10]; END_TYPE PROGRAM p VAR v : S1; END_VAR END_PROGRAM",
    "STRING[10] ''"
)]
#[case::wide(
    "TYPE S1 : WSTRING[10] := \"abc\"; END_TYPE PROGRAM p VAR v : S1; END_VAR END_PROGRAM",
    "WSTRING[10] 'abc'"
)]
#[case::through_aliases(
    "TYPE S1 : STRING[10] := 'abc'; S2 : S1; S3 : S2; END_TYPE
     PROGRAM p VAR v : S3; END_VAR END_PROGRAM",
    "STRING[10] 'abc'"
)]
#[case::alias_that_states_a_value(
    "TYPE S1 : STRING[10] := 'abc'; S2 : S1 := 'def'; S3 : S2; END_TYPE
     PROGRAM p VAR v : S3; END_VAR END_PROGRAM",
    "STRING[10] 'def'"
)]
#[case::variable_that_states_a_value(
    "TYPE S1 : STRING[10] := 'abc'; END_TYPE
     PROGRAM p VAR v : S1 := 'ghi'; END_VAR END_PROGRAM",
    "STRING[10] 'ghi'"
)]
fn analyze_when_variable_of_a_string_type_then_declared_as_a_string_of_its_value(
    #[case] text: &str,
    #[case] expected: &str,
) {
    let (initializer, codes) = analysed(text);

    assert_eq!(codes, Vec::<String>::new());
    assert_eq!(described(&initializer), expected);
}

#[test]
fn analyze_when_string_type_length_is_more_than_a_string_holds_then_reported_once_at_the_type() {
    let (initializer, codes) =
        analysed("TYPE Long : STRING[70000]; END_TYPE PROGRAM p VAR v : Long; END_VAR END_PROGRAM");

    assert_eq!(codes, vec!["P2041".to_string()]);
    assert!(!matches!(
        initializer,
        InitialValueAssignmentKind::String(_)
    ));
}
