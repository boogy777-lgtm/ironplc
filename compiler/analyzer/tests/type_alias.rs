//! A declaration made from an alias can use what a declaration made from the
//! type the alias names can use, whatever kind of type that is.

#![allow(
    clippy::unwrap_used,
    reason = "test target: panicking helpers are sanctioned in tests"
)]

use ironplc_analyzer::stages::analyze;
use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use ironplc_parser::parse_program;
use rstest::rstest;

/// The sorted codes of the diagnostics the analysis of `text` reports.
fn codes_of(text: &str) -> Vec<String> {
    let options = CompilerOptions::default();
    let library = parse_program(text, &FileId::default(), &options).unwrap();
    let (_library, context) = analyze(&[&library], &options).unwrap();
    let mut codes: Vec<String> = context
        .diagnostics()
        .iter()
        .map(|d| d.code.clone())
        .collect();
    codes.sort();
    codes
}

const TYPES: &str = "
TYPE
  S : STRUCT x : INT; y : BOOL; END_STRUCT;
  SA : S;
  SB : SA;
  A : ARRAY[1..3] OF INT;
  AA : A;
  AB : AA;
  E : (RED, GREEN);
  EA : E;
  EB : EA;
END_TYPE
";

#[rstest]
#[case::structure_alias_member_read("v : SA; r : INT;", "r := v.x;", &[])]
#[case::structure_alias_member_written("v : SA;", "v.y := TRUE;", &[])]
#[case::structure_alias_of_alias("v : SB; r : INT;", "r := v.x;", &[])]
#[case::array_alias_element("v : AA; r : INT;", "r := v[2];", &[])]
#[case::array_alias_element_written("v : AA;", "v[3] := 7;", &[])]
#[case::array_alias_of_alias("v : AB; r : INT;", "r := v[1];", &[])]
#[case::enumeration_alias_value("v : EA := GREEN;", "", &[])]
#[case::enumeration_alias_of_alias_value("v : EB := RED;", "", &[])]
#[case::enumeration_alias_other_value("v : EB := BLUE;", "", &["P2006"])]
fn analyze_when_declaration_made_from_alias_then_alias_is_the_type_it_names(
    #[case] declarations: &str,
    #[case] body: &str,
    #[case] expected: &[&str],
) {
    let program = format!("{TYPES} PROGRAM main VAR {declarations} END_VAR {body} END_PROGRAM");

    assert_eq!(codes_of(&program), expected);
}
