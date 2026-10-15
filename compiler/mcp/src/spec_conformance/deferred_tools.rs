//! Conformance tests for the `format` and `container_drop` tools, which are not yet implemented.

use spec_test_macro::spec_test;

// ===========================================================================
// `format` tool (REQ-TOL-mcp-080..084) — Milestone 1 (later)
// ===========================================================================

#[spec_test(REQ_TOL_mcp_080)]
#[ignore]
fn mcp_spec_req_tol_080_format_returns_canonical_form() {}

#[spec_test(REQ_TOL_mcp_081)]
#[ignore]
fn mcp_spec_req_tol_081_format_preserves_unparseable_source() {}

#[spec_test(REQ_TOL_mcp_082)]
#[ignore]
fn mcp_spec_req_tol_082_format_is_idempotent() {}

#[spec_test(REQ_TOL_mcp_083)]
#[ignore]
fn mcp_spec_req_tol_083_format_matches_plc2plc_output() {}

#[spec_test(REQ_TOL_mcp_084)]
#[ignore]
fn mcp_spec_req_tol_084_format_is_pure() {}

// ===========================================================================
// `container_drop` tool (REQ-TOL-mcp-150..151) — Milestone 2
// ===========================================================================

#[spec_test(REQ_TOL_mcp_150)]
#[ignore]
fn mcp_spec_req_tol_150_container_drop_removes_entry() {}

#[spec_test(REQ_TOL_mcp_151)]
#[ignore]
fn mcp_spec_req_tol_151_container_drop_unknown_id() {}
