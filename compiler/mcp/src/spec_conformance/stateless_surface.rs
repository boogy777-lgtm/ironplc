//! Conformance tests for the stateless tool surface shared by every tool (REQ-STL).

use spec_test_macro::spec_test;

use crate::tools;

use crate::tools::test_support::{ed2_options, source, VALID_PROGRAM};

// ===========================================================================
// Stateless Tool Surface (REQ-STL-*)
// ===========================================================================

/// REQ-STL-mcp-001: Every analysis, context, and execution tool accepts a
/// required `sources` parameter.
#[spec_test(REQ_STL_mcp_001)]
fn mcp_spec_req_stl_001_tools_accept_sources_parameter() {
    let sources = source(VALID_PROGRAM);
    let options = ed2_options();

    // parse accepts sources
    let parse_resp = tools::parse::build_response(&sources, &options);
    assert!(parse_resp.ok);

    // check accepts sources
    let check_resp = tools::check::build_response(&sources, &options);
    assert!(check_resp.ok);
}

/// REQ-STL-mcp-002: Every analysis, context, and execution tool accepts a
/// required `options` object.
#[spec_test(REQ_STL_mcp_002)]
fn mcp_spec_req_stl_002_tools_accept_options_parameter() {
    let sources = source(VALID_PROGRAM);
    // Both parse and check accept an options object with dialect
    let options = serde_json::json!({"dialect": "rusty"});

    let parse_resp = tools::parse::build_response(&sources, &options);
    assert!(parse_resp.ok);

    let check_resp = tools::check::build_response(&sources, &options);
    assert!(check_resp.ok);
}

/// REQ-STL-mcp-003: The server holds no per-client state across tool calls.
#[spec_test(REQ_STL_mcp_003)]
#[ignore] // Requires multi-call integration test
fn mcp_spec_req_stl_003_no_per_client_state_across_calls() {}

/// REQ-STL-mcp-004: File name validation constraints.
#[spec_test(REQ_STL_mcp_004)]
fn mcp_spec_req_stl_004_source_name_validation() {
    use crate::tools::common::{validate_sources, SourceInput};

    // Empty name rejected
    let errs = validate_sources(&[SourceInput {
        name: String::new(),
        content: String::new(),
    }]);
    assert!(!errs.is_empty());

    // Name > 256 bytes rejected
    let errs = validate_sources(&[SourceInput {
        name: "a".repeat(257),
        content: String::new(),
    }]);
    assert!(!errs.is_empty());

    // Non-printable ASCII rejected (NUL)
    let errs = validate_sources(&[SourceInput {
        name: "f\0.st".into(),
        content: String::new(),
    }]);
    assert!(!errs.is_empty());

    // Non-printable ASCII rejected (tab)
    let errs = validate_sources(&[SourceInput {
        name: "a\tb".into(),
        content: String::new(),
    }]);
    assert!(!errs.is_empty());

    // Duplicates rejected
    let errs = validate_sources(&[
        SourceInput {
            name: "f.st".into(),
            content: String::new(),
        },
        SourceInput {
            name: "f.st".into(),
            content: String::new(),
        },
    ]);
    assert!(!errs.is_empty());
}

/// REQ-STL-mcp-005: Every tool response includes a top-level `ok: boolean` field.
#[spec_test(REQ_STL_mcp_005)]
fn mcp_spec_req_stl_005_response_includes_ok_field() {
    let sources = source(VALID_PROGRAM);
    let options = ed2_options();

    // parse response has ok field
    let parse_resp = tools::parse::build_response(&sources, &options);
    let json = serde_json::to_value(&parse_resp).unwrap();
    assert!(json.get("ok").is_some());

    // check response has ok field
    let check_resp = tools::check::build_response(&sources, &options);
    let json = serde_json::to_value(&check_resp).unwrap();
    assert!(json.get("ok").is_some());
}

/// REQ-STL-mcp-006: The server performs no disk I/O from any tool handler.
#[spec_test(REQ_STL_mcp_006)]
#[ignore] // Architectural constraint; verified by code review
fn mcp_spec_req_stl_006_no_disk_io() {}
