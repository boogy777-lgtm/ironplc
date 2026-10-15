//! Conformance tests for the `symbols` tool.

use spec_test_macro::spec_test;

use crate::tools;

use crate::tools::test_support::{ed2_options, source, PROGRAM_WITH_VAR, SEMANTIC_ERROR_PROGRAM};

// ===========================================================================
// `symbols` tool (REQ-TOL-mcp-050..055) — Milestone 1 (later)
// ===========================================================================

#[spec_test(REQ_TOL_mcp_050)]
fn mcp_spec_req_tol_050_symbols_returns_declarations() {
    use crate::tools::common::SourceInput;

    let sources = vec![SourceInput {
        name: "main.st".into(),
        content: "TYPE\nMyEnum : (A, B);\nEND_TYPE\nFUNCTION_BLOCK fb\nEND_FUNCTION_BLOCK\nFUNCTION f : INT\nVAR_INPUT a : INT; END_VAR\nf := a;\nEND_FUNCTION\nPROGRAM p\nVAR inst : fb; END_VAR\nEND_PROGRAM".into(),
    }];
    let options = ed2_options();
    let resp = tools::symbols::build_response(&sources, &options, None);
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    assert!(!resp.programs.is_empty());
    assert!(!resp.functions.is_empty());
    assert!(!resp.function_blocks.is_empty());
    assert!(!resp.types.is_empty());
}

#[spec_test(REQ_TOL_mcp_051)]
fn mcp_spec_req_tol_051_program_variable_details() {
    let sources = source(PROGRAM_WITH_VAR);
    let options = ed2_options();
    let resp = tools::symbols::build_response(&sources, &options, None);
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    let prog = &resp.programs[0];
    let var = &prog.variables[0];
    assert_eq!(var.name, "x");
    assert_eq!(var.direction, "Local");
    assert!(!var.external);
}

#[spec_test(REQ_TOL_mcp_052)]
fn mcp_spec_req_tol_052_function_entry_details() {
    use crate::tools::common::SourceInput;

    let sources = vec![SourceInput {
        name: "main.st".into(),
        content: "FUNCTION f : INT\nVAR_INPUT a : INT; END_VAR\nf := a;\nEND_FUNCTION\nPROGRAM p\nVAR r : INT; END_VAR\nr := f(a := 1);\nEND_PROGRAM".into(),
    }];
    let options = ed2_options();
    let resp = tools::symbols::build_response(&sources, &options, None);
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    let func = &resp.functions[0];
    assert_eq!(func.name, "f");
    assert_eq!(func.return_type, "INT");
    assert!(!func.parameters.is_empty());
    assert_eq!(func.parameters[0].direction, "In");
}

#[spec_test(REQ_TOL_mcp_053)]
fn mcp_spec_req_tol_053_symbols_diagnostics_format() {
    let sources = source(SEMANTIC_ERROR_PROGRAM);
    let options = ed2_options();
    let resp = tools::symbols::build_response(&sources, &options, None);
    assert!(!resp.ok);
    assert!(!resp.diagnostics.is_empty());
    assert!(resp.diagnostics[0]["code"].as_str().is_some());
}

#[spec_test(REQ_TOL_mcp_054)]
fn mcp_spec_req_tol_054_symbols_pou_filter() {
    use crate::tools::common::SourceInput;

    let sources = vec![SourceInput {
        name: "main.st".into(),
        content: "PROGRAM a\nEND_PROGRAM\nPROGRAM b\nVAR x : INT; END_VAR\nEND_PROGRAM".into(),
    }];
    let options = ed2_options();

    let resp = tools::symbols::build_response(&sources, &options, Some("b"));
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    assert_eq!(resp.found, Some(true));
    assert_eq!(resp.programs.len(), 1);
    assert_eq!(resp.programs[0].name, "b");

    let resp_missing = tools::symbols::build_response(&sources, &options, Some("nonexistent"));
    assert!(!resp_missing.ok);
    assert_eq!(resp_missing.found, Some(false));
}

#[spec_test(REQ_TOL_mcp_055)]
#[ignore]
fn mcp_spec_req_tol_055_symbols_response_size_cap() {}
