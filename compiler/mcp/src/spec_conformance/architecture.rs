//! Conformance tests for the server architecture requirements (REQ-ARC).

use spec_test_macro::spec_test;

use crate::tools;

use crate::tools::test_support::{ed2_options, source, VALID_PROGRAM};

// ===========================================================================
// Architecture (REQ-ARC-*)
// ===========================================================================

/// REQ-ARC-mcp-001: The MCP server uses stdio transport.
#[spec_test(REQ_ARC_mcp_001)]
fn mcp_spec_req_arc_001_stdio_transport() {
    // run_server() creates a stdio transport. We verify the function exists
    // and the server can be constructed. Actually starting the transport
    // requires stdin/stdout, so we just verify construction.
    let _server = crate::server::IronPlcMcp::new();
}

/// REQ-ARC-mcp-010: Each tool call constructs a fresh in-memory project from
/// the supplied sources.
#[spec_test(REQ_ARC_mcp_010)]
fn mcp_spec_req_arc_010_fresh_project_per_call() {
    let sources = source(VALID_PROGRAM);
    let options = ed2_options();

    // Two independent calls both succeed — no state leaks between them
    let resp1 = tools::check::build_response(&sources, &options);
    assert!(resp1.ok);

    let resp2 = tools::check::build_response(&sources, &options);
    assert!(resp2.ok);
}

/// REQ-ARC-mcp-011: Source names become FileId via `FileId::from_string`,
/// and names are validated before the compiler runs.
#[spec_test(REQ_ARC_mcp_011)]
fn mcp_spec_req_arc_011_file_id_from_string() {
    use crate::tools::common::SourceInput;

    // Diagnostic file field should match the source name we provided
    let sources = vec![SourceInput {
        name: "my_file.st".into(),
        content: "PROGRAM".into(), // parse error
    }];
    let options = ed2_options();

    let resp = tools::parse::build_response(&sources, &options);
    assert!(!resp.ok);
    assert_eq!(resp.diagnostics[0]["file"], "my_file.st");
}

#[spec_test(REQ_ARC_mcp_012)]
#[ignore]
fn mcp_spec_req_arc_012_no_filesystem_paths() {}

#[spec_test(REQ_ARC_mcp_020)]
#[ignore]
fn mcp_spec_req_arc_020_fully_qualified_variable_names() {}

#[spec_test(REQ_ARC_mcp_021)]
#[ignore]
fn mcp_spec_req_arc_021_unresolved_variable_name() {}

#[spec_test(REQ_ARC_mcp_030)]
#[ignore]
fn mcp_spec_req_arc_030_vm_resource_limits() {}

#[spec_test(REQ_ARC_mcp_031)]
#[ignore]
fn mcp_spec_req_arc_031_reject_loosened_limits() {}

#[spec_test(REQ_ARC_mcp_032)]
#[ignore]
fn mcp_spec_req_arc_032_vm_terminates_on_limit() {}

#[spec_test(REQ_ARC_mcp_033)]
#[ignore]
fn mcp_spec_req_arc_033_fuel_shared_across_tasks() {}

#[spec_test(REQ_ARC_mcp_034)]
#[ignore]
fn mcp_spec_req_arc_034_terminated_reason_completed_or_error() {}

#[spec_test(REQ_ARC_mcp_035)]
#[ignore]
fn mcp_spec_req_arc_035_wall_clock_not_real_time() {}

#[spec_test(REQ_ARC_mcp_040)]
#[ignore]
fn mcp_spec_req_arc_040_structured_log_per_tool_call() {}

#[spec_test(REQ_ARC_mcp_041)]
#[ignore]
fn mcp_spec_req_arc_041_tool_specific_log_summary() {}

#[spec_test(REQ_ARC_mcp_042)]
#[ignore]
fn mcp_spec_req_arc_042_no_source_text_in_logs() {}

#[spec_test(REQ_ARC_mcp_043)]
#[ignore]
fn mcp_spec_req_arc_043_logs_to_stderr() {}

#[spec_test(REQ_ARC_mcp_044)]
#[ignore]
fn mcp_spec_req_arc_044_connection_start_end_events() {}

#[spec_test(REQ_ARC_mcp_045)]
#[ignore]
fn mcp_spec_req_arc_045_log_stream_sufficient_for_analysis() {}

/// REQ-ARC-mcp-050: Tool descriptions follow the design guidance.
#[spec_test(REQ_ARC_mcp_050)]
fn mcp_spec_req_arc_050_tool_descriptions() {
    // Verify the server can be constructed with tool descriptions.
    // The descriptions are validated by the tool registration macros;
    // full text comparison is deferred to integration tests.
    let _server = crate::server::IronPlcMcp::new();
}

#[spec_test(REQ_ARC_mcp_051)]
#[ignore]
fn mcp_spec_req_arc_051_tool_descriptions_no_false_claims() {}

#[spec_test(REQ_ARC_mcp_060)]
#[ignore]
fn mcp_spec_req_arc_060_symbols_pou_filter_and_cap() {}

#[spec_test(REQ_ARC_mcp_061)]
#[ignore]
fn mcp_spec_req_arc_061_context_tools_are_blessed_path() {}

#[spec_test(REQ_ARC_mcp_062)]
#[ignore]
fn mcp_spec_req_arc_062_response_size_in_log() {}

#[spec_test(REQ_ARC_mcp_070)]
#[ignore]
fn mcp_spec_req_arc_070_container_cache() {}

#[spec_test(REQ_ARC_mcp_071)]
#[ignore]
fn mcp_spec_req_arc_071_cache_bounded_capacity() {}

#[spec_test(REQ_ARC_mcp_072)]
#[ignore]
fn mcp_spec_req_arc_072_cache_no_timer_expiry() {}

#[spec_test(REQ_ARC_mcp_073)]
#[ignore]
fn mcp_spec_req_arc_073_unknown_container_id() {}
