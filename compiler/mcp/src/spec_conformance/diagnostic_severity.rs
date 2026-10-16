//! Conformance tests for the severity of a diagnostic in a tool response
//! (`specs/design/diagnostic-severity.md`, `REQ-DS-mcp-*`).

use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::{Diagnostic, Severity};
use ironplc_problems::text;
use spec_test_macro::spec_test;

use crate::tools;
use crate::tools::common::{serialize_diagnostic, severity_name};
use crate::tools::test_support::{ed2_options, source, SEMANTIC_ERROR_PROGRAM, VALID_PROGRAM};

#[spec_test(REQ_DS_mcp_001)]
fn mcp_spec_req_ds_001_severity_is_named_by_the_severity_of_the_diagnostic() {
    assert_eq!(severity_name(Severity::Error), "error");
    assert_eq!(severity_name(Severity::Warning), "warning");
    assert_eq!(severity_name(Severity::Info), "info");

    let diagnostic = Diagnostic::text(
        SourceSpan::default(),
        text::syntax_error_a_task_needs_a_priority(),
    );
    let json = serialize_diagnostic(&diagnostic);
    assert_eq!(
        json["severity"].as_str(),
        Some(severity_name(diagnostic.severity()))
    );
    assert_eq!(json["severity"].as_str(), Some("error"));
}

#[spec_test(REQ_DS_mcp_002)]
fn mcp_spec_req_ds_002_check_is_ok_only_when_no_diagnostic_is_an_error() {
    let clean = tools::check::build_response(&source(VALID_PROGRAM), &ed2_options());
    assert!(clean.ok, "diagnostics: {:?}", clean.diagnostics);
    assert!(clean.diagnostics.is_empty());

    let broken = tools::check::build_response(&source(SEMANTIC_ERROR_PROGRAM), &ed2_options());
    assert!(!broken.ok);
    assert!(broken
        .diagnostics
        .iter()
        .all(|d| d["severity"].as_str() == Some("error")));
}
