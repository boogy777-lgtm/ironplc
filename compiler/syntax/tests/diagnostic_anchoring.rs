//! A fault is anchored at the token that is wrong or that stands where
//! something is missing, never at a token before it that is right: a `.` that
//! has no member after it is reported at the token after the `.`, a list of
//! names that one name is allowed is reported at its first `,`, a path that
//! stops short is reported where it stops.

use ironplc_syntax::{parse_source_file, ParseOptions};
use rstest::rstest;

/// The message of the primary error of `source` and the source text it covers.
fn anchored(source: &str) -> (String, String) {
    let parse = parse_source_file(source, &ParseOptions::all());
    let error = parse.primary_error();
    let covered = error
        .map(|error| &source[usize::from(error.range.start())..usize::from(error.range.end())]);
    (
        error
            .map(|error| error.message().to_string())
            .unwrap_or_default(),
        covered.unwrap_or_default().to_string(),
    )
}

#[rstest]
#[case::assignment(
    "PROGRAM p\nx := a.;\nEND_PROGRAM",
    "found `;`, expected a member name",
    ";"
)]
#[case::after_a_member(
    "PROGRAM p\nx := a.b.;\nEND_PROGRAM",
    "found `;`, expected a member name",
    ";"
)]
#[case::after_a_dereference(
    "PROGRAM p\nx := a^.;\nEND_PROGRAM",
    "found `;`, expected a member name",
    ";"
)]
#[case::in_a_condition(
    "PROGRAM p\nIF a. THEN x := 1; END_IF;\nEND_PROGRAM",
    "found `THEN`, expected a member name",
    "THEN"
)]
#[case::as_a_target(
    "PROGRAM p\na. := 1;\nEND_PROGRAM",
    "found `:=`, expected a member name",
    ":="
)]
#[case::variable_block_of_one_name(
    "PROGRAM p VAR_EXTERNAL x, y : INT; END_VAR END_PROGRAM",
    "this variable block declares one variable at a time",
    ","
)]
#[case::path_of_one_name(
    "CONFIGURATION c RESOURCE r ON t PROGRAM p : q; END_RESOURCE VAR_CONFIG p.x : INT := 1; END_VAR END_CONFIGURATION",
    "found `:`, expected the path `resource.program.variable`",
    ":"
)]
#[case::path_of_two_names(
    "CONFIGURATION c RESOURCE r ON t PROGRAM p : q; END_RESOURCE VAR_CONFIG r.p : INT := 1; END_VAR END_CONFIGURATION",
    "found `:`, expected the path `resource.program.variable`",
    ":"
)]
#[case::path_that_is_not_a_name(
    "CONFIGURATION c RESOURCE r ON t PROGRAM p : q; END_RESOURCE VAR_CONFIG 1 : INT := 1; END_VAR END_CONFIGURATION",
    "found `1`, expected the path `resource.program.variable`",
    "1"
)]
fn primary_error_when_a_token_before_the_fault_is_right_then_anchored_at_the_fault(
    #[case] source: &str,
    #[case] message: &str,
    #[case] covered: &str,
) {
    assert_eq!(anchored(source), (message.to_string(), covered.to_string()));
}

#[test]
fn parse_when_a_path_is_complete_then_no_error() {
    let source = "CONFIGURATION c RESOURCE r ON t PROGRAM p : q; END_RESOURCE VAR_CONFIG r.p.x : INT := 1; END_VAR END_CONFIGURATION";
    assert!(parse_source_file(source, &ParseOptions::all()).is_ok());
}
