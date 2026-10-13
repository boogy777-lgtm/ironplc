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
