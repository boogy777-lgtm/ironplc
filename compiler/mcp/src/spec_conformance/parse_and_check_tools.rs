//! Conformance tests for the `parse` and `check` analysis tools.

use spec_test_macro::spec_test;

use crate::tools;

use crate::tools::test_support::{
    ed2_options, source, with_program, SEMANTIC_ERROR_PROGRAM, SYNTAX_ERROR_PROGRAM, VALID_PROGRAM,
};

// ===========================================================================
// `parse` tool (REQ-TOL-mcp-010..013)
// ===========================================================================

/// REQ-TOL-mcp-010: The `parse` tool runs the parse stage only (no semantic analysis).
#[spec_test(REQ_TOL_mcp_010)]
fn mcp_spec_req_tol_010_parse_runs_parse_only() {
    // This program parses fine but has a semantic error (undeclared variable y).
    // parse should succeed because it doesn't run semantic analysis.
    let sources = source(SEMANTIC_ERROR_PROGRAM);
    let options = ed2_options();

    let resp = tools::parse::build_response(&sources, &options);
    assert!(resp.ok, "parse should not catch semantic errors");
}

/// REQ-TOL-mcp-011: The `parse` tool returns a `diagnostics` array using the same
/// format as `check`.
#[spec_test(REQ_TOL_mcp_011)]
fn mcp_spec_req_tol_011_parse_returns_diagnostics_array() {
    let sources = source(SYNTAX_ERROR_PROGRAM);
    let options = ed2_options();

    let resp = tools::parse::build_response(&sources, &options);
    let json = serde_json::to_value(&resp).unwrap();
    let diags = json["diagnostics"].as_array().unwrap();
    assert!(!diags.is_empty());
    // Verify diagnostic format matches check's format
    let d = &diags[0];
    assert!(d.get("prefix").is_some());
    assert!(d.get("number").is_some());
    assert!(d.get("text").is_some());
    assert!(d.get("object").is_some());
    assert!(d.get("severity").is_some());
}

/// REQ-TOL-mcp-012: The `parse` tool accepts the same `options` object as `check`.
#[spec_test(REQ_TOL_mcp_012)]
fn mcp_spec_req_tol_012_parse_accepts_same_options_as_check() {
    let sources = source(VALID_PROGRAM);
    // Same options accepted by both
    let options = serde_json::json!({"dialect": "rusty", "allow_c_style_comments": true});

    let parse_resp = tools::parse::build_response(&sources, &options);
    assert!(parse_resp.ok);

    let check_resp = tools::check::build_response(&sources, &options);
    assert!(check_resp.ok);
}

/// REQ-TOL-mcp-013: The `parse` tool returns a best-effort `structure` array.
#[spec_test(REQ_TOL_mcp_013)]
fn mcp_spec_req_tol_013_parse_returns_structure_array() {
    let sources = source(VALID_PROGRAM);
    let options = ed2_options();

    let resp = tools::parse::build_response(&sources, &options);
    let json = serde_json::to_value(&resp).unwrap();
    let structure = json["structure"].as_array().unwrap();
    assert_eq!(structure.len(), 1);
    let entry = &structure[0];
    assert_eq!(entry["kind"], "program");
    assert_eq!(entry["name"], "p");
    assert!(entry.get("file").is_some());
    assert!(entry.get("start").is_some());
    assert!(entry.get("end").is_some());
}

// ===========================================================================
// `check` tool (REQ-TOL-mcp-020..026)
// ===========================================================================

/// REQ-TOL-mcp-020: The `check` tool runs parse and full semantic analysis.
#[spec_test(REQ_TOL_mcp_020)]
fn mcp_spec_req_tol_020_check_runs_semantic_analysis() {
    // This program has a semantic error (undeclared variable y).
    let sources = source(SEMANTIC_ERROR_PROGRAM);
    let options = ed2_options();

    let resp = tools::check::build_response(&sources, &options);
    assert!(!resp.ok, "check should catch semantic errors");
    assert!(!resp.diagnostics.is_empty());
}

/// REQ-TOL-mcp-021: The `check` tool does not run code generation.
#[spec_test(REQ_TOL_mcp_021)]
fn mcp_spec_req_tol_021_check_no_codegen() {
    let sources = source(VALID_PROGRAM);
    let options = ed2_options();

    let resp = tools::check::build_response(&sources, &options);
    // Response has no container_id — check does not produce codegen output
    let json = serde_json::to_value(&resp).unwrap();
    assert!(json.get("container_id").is_none());
}

/// REQ-TOL-mcp-022: The `check` tool returns `diagnostics` and `ok`; the
/// diagnostics end with the line that closes the check, and `ok` ignores it.
#[spec_test(REQ_TOL_mcp_022)]
fn mcp_spec_req_tol_022_check_returns_diagnostics_and_ok() {
    // Valid program: ok = true, and the closing line alone
    let sources = source(VALID_PROGRAM);
    let options = ed2_options();

    let resp = tools::check::build_response(&sources, &options);
    let json = serde_json::to_value(&resp).unwrap();
    assert_eq!(json["ok"], true);
    let diagnostics = json["diagnostics"].as_array().unwrap();
    assert_eq!(diagnostics.len(), 1);
    assert_eq!(diagnostics[0]["severity"], "Text");
    assert_eq!(
        diagnostics[0]["text"],
        "Compile complete -- 0 errors, 0 warnings"
    );

    // Invalid program: ok = false, the errors and then the closing line with
    // their count
    let bad_sources = source(SEMANTIC_ERROR_PROGRAM);
    let resp = tools::check::build_response(&bad_sources, &options);
    let json = serde_json::to_value(&resp).unwrap();
    assert_eq!(json["ok"], false);
    let diagnostics = json["diagnostics"].as_array().unwrap();
    assert!(diagnostics.len() > 1);
    let last = &diagnostics[diagnostics.len() - 1];
    assert_eq!(last["severity"], "Text");
    assert_eq!(
        last["text"],
        format!(
            "Compile complete -- {} errors, 0 warnings",
            diagnostics.len() - 1
        )
    );
}

/// REQ-TOL-mcp-023: Diagnostic format with byte offsets.
#[spec_test(REQ_TOL_mcp_023)]
fn mcp_spec_req_tol_023_diagnostic_format() {
    let sources = source(SEMANTIC_ERROR_PROGRAM);
    let options = ed2_options();

    let resp = tools::check::build_response(&sources, &options);
    assert!(!resp.diagnostics.is_empty());

    let d = &resp.diagnostics[0];

    // All required fields are present
    assert!(d.get("prefix").is_some());
    assert!(d.get("number").is_some());
    assert!(d.get("text").is_some());
    assert!(d.get("object").is_some());
    assert!(d.get("position").is_some());
    assert!(d.get("length").is_some());
    assert!(d.get("severity").is_some());

    // position/length are 0-indexed byte offsets and a byte count (numeric)
    assert!(d["position"].is_number());
    assert!(d["length"].is_number());
}

/// REQ-TOL-mcp-024: The `check` tool never returns an MCP-level error for
/// compiler failures.
#[spec_test(REQ_TOL_mcp_024)]
fn mcp_spec_req_tol_024_no_mcp_error_for_compiler_failures() {
    // Syntax error — should produce diagnostics, not panic or MCP error
    let sources = source(SYNTAX_ERROR_PROGRAM);
    let options = ed2_options();

    // build_response returns a CheckResponse (not Err), even for broken input
    let resp = tools::check::build_response(&sources, &options);
    assert!(!resp.ok);
    assert!(!resp.diagnostics.is_empty());
}

/// REQ-TOL-mcp-025: The `check` tool rejects invalid `options`.
#[spec_test(REQ_TOL_mcp_025)]
fn mcp_spec_req_tol_025_rejects_invalid_options() {
    let sources = source(VALID_PROGRAM);

    // Missing dialect
    let resp = tools::check::build_response(&sources, &serde_json::json!({}));
    assert!(!resp.ok);

    // Unknown dialect
    let resp = tools::check::build_response(&sources, &serde_json::json!({"dialect": "cobol"}));
    assert!(!resp.ok);

    // Unknown key
    let resp = tools::check::build_response(
        &sources,
        &serde_json::json!({"dialect": "iec61131-3-ed2", "bogus_key": true}),
    );
    assert!(!resp.ok);
}

/// REQ-TOL-mcp-026: The `check` tool accepts individual feature flag overrides.
#[spec_test(REQ_TOL_mcp_026)]
fn mcp_spec_req_tol_026_accepts_flag_overrides() {
    // C-style comments are not allowed in ed2, but allowed with override
    let sources = source(&with_program("// C-style comment"));

    // Without override — should fail (ed2 doesn't allow C-style comments)
    let resp = tools::check::build_response(&sources, &ed2_options());
    assert!(!resp.ok, "ed2 should reject C-style comments");

    // With flag override — should succeed
    let resp = tools::check::build_response(
        &sources,
        &serde_json::json!({"dialect": "iec61131-3-ed2", "allow_c_style_comments": true}),
    );
    assert!(resp.ok, "flag override should enable C-style comments");
}
