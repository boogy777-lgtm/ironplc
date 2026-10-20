//! Conformance tests for the `list_options` and `explain_diagnostic` reference-lookup tools.

use spec_test_macro::spec_test;

use crate::tools;

// ===========================================================================
// `list_options` tool (REQ-TOL-mcp-060..063) — Implemented
// ===========================================================================

/// REQ-TOL-mcp-060: The `list_options` tool takes no inputs.
#[spec_test(REQ_TOL_mcp_060)]
fn mcp_spec_req_tol_060_list_options_takes_no_inputs() {
    // list_options is called with no parameters — build_response() takes none.
    let response = tools::list_options::build_response();
    // If it compiles and returns, it takes no inputs.
    assert!(!response.dialects.is_empty());
}

/// REQ-TOL-mcp-061: The `list_options` tool returns a `dialects` array whose entries
/// each contain `id`, `display_name`, and `description`.
#[spec_test(REQ_TOL_mcp_061)]
fn mcp_spec_req_tol_061_list_options_returns_dialects() {
    use ironplc_parser::options::Dialect;
    let response = tools::list_options::build_response();
    assert_eq!(response.dialects.len(), Dialect::ALL.len());
    for dialect in &response.dialects {
        assert!(!dialect.id.is_empty(), "dialect id must be non-empty");
        assert!(
            !dialect.display_name.is_empty(),
            "dialect display_name must be non-empty"
        );
        assert!(
            !dialect.description.is_empty(),
            "dialect description must be non-empty"
        );
    }
}

/// REQ-TOL-mcp-062: The `list_options` tool returns a `flags` array whose entries
/// each contain `id`, `type`, `default`, and `description`.
#[spec_test(REQ_TOL_mcp_062)]
fn mcp_spec_req_tol_062_list_options_returns_flags() {
    let response = tools::list_options::build_response();
    assert!(!response.flags.is_empty());
    for flag in &response.flags {
        assert!(!flag.id.is_empty(), "flag id must be non-empty");
        match flag.flag_type.as_str() {
            "bool" | "string-list" => assert!(flag.allowed_values.is_none()),
            "enum" => assert!(
                flag.allowed_values.as_ref().is_some_and(|v| !v.is_empty()),
                "enum flag {} must list its allowed values",
                flag.id
            ),
            other => assert!(
                matches!(other, "bool" | "string-list" | "enum"),
                "flag {} has unexpected type {other}",
                flag.id
            ),
        }
        assert!(
            !flag.description.is_empty(),
            "flag {} has empty description",
            flag.id
        );
    }
}

/// REQ-TOL-mcp-063: The option `id` values returned by `list_options` are the exact
/// keys accepted in the `options` object of `parse`, `check`, and `compile`.
#[spec_test(REQ_TOL_mcp_063)]
fn mcp_spec_req_tol_063_option_ids_match_compiler_options_fields() {
    use ironplc_parser::options::CompilerOptions;

    let response = tools::list_options::build_response();
    let flag_ids: Vec<&str> = response.flags.iter().map(|f| f.id.as_str()).collect();

    // Every FEATURE_DESCRIPTORS entry must appear in the response
    for fd in CompilerOptions::FEATURE_DESCRIPTORS {
        assert!(
            flag_ids.contains(&fd.option_key),
            "FEATURE_DESCRIPTOR key '{}' missing from list_options response",
            fd.option_key
        );
    }
    // ... and every list of warnings, which `parse_options` accepts as an array.
    for list in ironplc_dsl::diagnostic::WarningList::ALL {
        assert!(
            flag_ids.contains(&list.option_key()),
            "list '{}' missing from list_options response",
            list.option_key()
        );
    }
    // ... and every behavior policy, which `parse_options` accepts as well.
    for pd in CompilerOptions::POLICY_DESCRIPTORS {
        assert!(
            flag_ids.contains(&pd.option_key),
            "POLICY_DESCRIPTOR key '{}' missing from list_options response",
            pd.option_key
        );
    }
}

// ===========================================================================
// `explain_diagnostic` tool (REQ-TOL-mcp-070..072)
// ===========================================================================

/// REQ-TOL-mcp-070: The `explain_diagnostic` tool accepts a `code` string and
/// returns `code`, `title`, `description`, and optionally `suggested_fix`.
#[spec_test(REQ_TOL_mcp_070)]
fn mcp_spec_req_tol_070_explain_diagnostic_returns_explanation() {
    let resp = tools::explain_diagnostic::build_response("P0001");
    assert!(resp.ok);
    assert!(resp.found);
    assert_eq!(resp.code, "P0001");
    assert!(resp.title.is_some());
    assert!(!resp.title.unwrap().is_empty());
    assert!(resp.description.is_some());
    assert!(!resp.description.unwrap().is_empty());
    assert!(resp.diagnostics.is_empty());
}

/// REQ-TOL-mcp-071: The `explain_diagnostic` tool returns `ok: false`,
/// `found: false`, and a populated `diagnostics` array when the code is
/// unknown.
#[spec_test(REQ_TOL_mcp_071)]
fn mcp_spec_req_tol_071_explain_diagnostic_unknown_code() {
    let resp = tools::explain_diagnostic::build_response("P9876");
    assert!(!resp.ok);
    assert!(!resp.found);
    assert!(!resp.diagnostics.is_empty());
}

/// REQ-TOL-mcp-072: The text is embedded at build time via `include_str!`.
/// If this test compiles and runs, the build-time embedding succeeded.
#[spec_test(REQ_TOL_mcp_072)]
fn mcp_spec_req_tol_072_explain_diagnostic_embedded_at_build_time() {
    // The lookup function is generated by build.rs using include_str!.
    // If the binary compiled, the embedding worked. Verify a known code
    // returns non-empty RST content.
    let resp = tools::explain_diagnostic::build_response("P0002");
    assert!(resp.ok);
    assert!(resp.found);
    let desc = resp.description.unwrap();
    assert!(desc.contains("Syntax error") || !desc.is_empty());
}
