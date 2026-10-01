//! Spec conformance tests for numeric and boolean literal types
//! (analyzer-owned requirements): what a `BIT` declaration is.
//!
//! Each test is annotated with `#[spec_test(REQ_NL_analyzer_NNN)]`, which adds
//! `#[test]` and references a build-script-generated constant so the test
//! fails to compile if the requirement is removed from the spec. The
//! `all_spec_requirements_have_tests` meta-test in `spec_conformance` asserts
//! every analyzer-owned requirement has a test.
//!
//! See `specs/design/numeric-literals.md`.

use ironplc_dsl::common::TypeName;
use ironplc_parser::options::CompilerOptions;
use spec_test_macro::spec_test;

use crate::test_helpers::parse_and_resolve_types_with_options;

/// REQ-NL-analyzer-020: a `BIT` declaration is an alias of `BOOL` -- the same
/// representation, taking the same literals.
#[spec_test(REQ_NL_analyzer_020)]
fn analyzer_spec_req_nl_020_bit_is_an_alias_of_bool() {
    let program = "PROGRAM main
VAR
    b : BIT;
    x : BOOL;
END_VAR
    b := BIT#1;
    x := b;
END_PROGRAM";
    let options = CompilerOptions {
        allow_bit_type: true,
        ..CompilerOptions::default()
    };
    let (_library, context) = parse_and_resolve_types_with_options(program, &options);

    assert!(
        context.diagnostics().is_empty(),
        "a BIT declaration and its literals are accepted: {:?}",
        context.diagnostics()
    );
    assert_eq!(
        context
            .types()
            .resolve_elementary_type_name(&TypeName::from("BIT")),
        context
            .types()
            .resolve_elementary_type_name(&TypeName::from("BOOL")),
        "BIT resolves to the same elementary type as BOOL"
    );
}
