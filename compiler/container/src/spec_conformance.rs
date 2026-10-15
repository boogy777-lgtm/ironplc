//! Spec conformance tests for the bytecode container format and instruction set.
//!
//! Each test is annotated with `#[spec_test(REQ_XX_NNN)]` which:
//! 1. Adds `#[test]`
//! 2. References a build-script-generated constant — compilation fails if the
//!    requirement was removed from the spec markdown.
//!
//! The `all_spec_requirements_have_tests` meta-test ensures every requirement
//! in the spec has at least one test here.
//!
//! See `specs/design/spec-conformance-testing.md` for full design.

// ---------------------------------------------------------------------------
// Meta-test: completeness check
// ---------------------------------------------------------------------------

#[test]
fn all_spec_requirements_have_tests() {
    // UNTESTED is computed by build.rs by scanning all .rs files under src/
    // for #[spec_test(REQ_...)] attributes.
    assert!(
        crate::spec_requirements::UNTESTED.is_empty(),
        "Requirements in spec with no conformance test: {:?}",
        crate::spec_requirements::UNTESTED
    );
}

mod code_and_constants;
mod content_hash;
mod enum_def_payload;
mod file_header;
mod file_layout;
mod layout_hash;
mod loading;
mod type_section;
