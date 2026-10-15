//! Spec conformance tests for the MCP server design.
//!
//! Each test is annotated with `#[spec_test(REQ_XX_NNN)]` which:
//! 1. Adds `#[test]`
//! 2. References a build-script-generated constant — compilation fails if the
//!    requirement was removed from the spec markdown.
//!
//! The `all_spec_requirements_have_tests` meta-test ensures every requirement
//! in the spec has at least one test, in this module or in its child modules,
//! one per area of the spec.
//!
//! See `specs/design/spec-conformance-testing.md` for full design.
//! See `specs/design/mcp-server.md` for the MCP server requirements.

// ---------------------------------------------------------------------------
// Meta-test: completeness check
// ---------------------------------------------------------------------------

#[test]
fn all_spec_requirements_have_tests() {
    assert!(
        crate::spec_requirements::UNTESTED.is_empty(),
        "Requirements in spec with no conformance test: {:?}",
        crate::spec_requirements::UNTESTED
    );
}

mod architecture;
mod compile_and_run_tools;
mod context_tools;
mod deferred_tools;
mod parse_and_check_tools;
mod reference_lookup_tools;
mod stateless_surface;
mod symbols_tool;
