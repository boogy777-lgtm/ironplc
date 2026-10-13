//! Spec conformance tests for the attributes of declarations
//! (analyzer-owned requirements `REQ-ATTR-analyzer-NNN`): the field survives
//! the analysis.
//!
//! See `specs/design/attributes.md`.

use crate::stages::analyze;
use ironplc_dsl::attribute::census::{declared, Declared};
use ironplc_dsl::common::Library;
use ironplc_dsl::core::FileId;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_parser::parse_program;
use ironplc_test::fixtures::{ATTRIBUTE_PROGRAM, ATTRIBUTE_PROGRAM_ATTRIBUTES};
use spec_test_macro::spec_test;

fn library_of(source: &str, options: &CompilerOptions) -> Library {
    parse_program(source, &FileId::default(), options).expect("the source parses")
}

/// The attribute names the declaration named `name` carries, if there is one.
fn attributes_named(found: &[Declared], name: &str) -> Option<Vec<String>> {
    found
        .iter()
        .find(|d| d.name.eq_ignore_ascii_case(name) && !d.attributes.is_empty())
        .map(|d| d.attributes.iter().map(|a| a.name.clone()).collect())
}

#[spec_test(REQ_ATTR_analyzer_001)]
fn analyze_when_attribute_before_each_kind_of_target_then_the_analysed_library_holds_it() {
    let options = CompilerOptions::from_dialect(Dialect::Codesys);
    let library = library_of(ATTRIBUTE_PROGRAM, &options);
    let parsed = declared(&library);

    let (analysed, _) = analyze(&[&library], &options).expect("the program is analysed");
    let found = declared(&analysed);

    // The parse holds one attribute for each kind of target.
    let kinds: std::collections::HashSet<_> = parsed
        .iter()
        .filter(|d| !d.attributes.is_empty())
        .map(|d| d.kind)
        .collect();
    assert_eq!(
        kinds.len(),
        ironplc_dsl::attribute::census::TargetKind::ALL.len()
    );
    let lost: Vec<&str> = ATTRIBUTE_PROGRAM_ATTRIBUTES
        .iter()
        .filter(|(attribute, name)| {
            attributes_named(&found, name) != Some(vec![attribute.to_string()])
        })
        .map(|(_, name)| *name)
        .collect();
    assert!(
        lost.is_empty(),
        "the analysed library lost the attributes of {lost:?}"
    );
}

/// A variable whose declaration the analysis rewrites keeps its attribute:
/// a type the lowering could not classify (late-bound), a value that is a
/// constant expression, a function block instance and a standard function
/// block.
#[spec_test(REQ_ATTR_analyzer_001)]
fn analyze_when_attribute_before_a_variable_the_analysis_rewrites_then_the_variable_keeps_it() {
    let source = "
TYPE
    Shape : STRUCT size : INT; END_STRUCT;
    Color : (Red, Green);
    Limits : ARRAY [0..1] OF INT;
END_TYPE
FUNCTION_BLOCK Motor
END_FUNCTION_BLOCK
PROGRAM Main
VAR CONSTANT
    {attribute 'constant'} width : INT := 4;
END_VAR
VAR
    {attribute 'structure'} shape : Shape := (size := 2);
    {attribute 'enumeration'} color : Color := Green;
    {attribute 'array'} limits : Limits;
    {attribute 'function_block'} motor : Motor;
    {attribute 'standard'} timer : TON;
    {attribute 'expression'} area : INT := width * 2;
    {attribute 'bool'} flag : BOOL := 1;
    {attribute 'plain'} count : INT;
END_VAR
END_PROGRAM";
    let options = CompilerOptions::from_dialect(Dialect::Codesys);
    let library = library_of(source, &options);

    let (analysed, _) = analyze(&[&library], &options).expect("the program is analysed");

    let found = declared(&analysed);
    for (attribute, name) in [
        ("constant", "width"),
        ("structure", "shape"),
        ("enumeration", "color"),
        ("array", "limits"),
        ("function_block", "motor"),
        ("standard", "timer"),
        ("expression", "area"),
        ("bool", "flag"),
        ("plain", "count"),
    ] {
        assert_eq!(
            attributes_named(&found, name),
            Some(vec![attribute.to_string()]),
            "the analysed library lost the attribute of {name}"
        );
    }
}
