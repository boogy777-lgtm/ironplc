//! Conformance tests for the severity of a diagnostic in a tool response
//! (`specs/design/diagnostic-severity.md`, `REQ-DS-mcp-*`).

use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::{Diagnostic, Severity};
use ironplc_problems::text;
use spec_test_macro::spec_test;

use crate::tools;
use crate::tools::common::{serialize_diagnostic, serialize_diagnostics};
use crate::tools::test_support::{ed2_options, source, SEMANTIC_ERROR_PROGRAM, VALID_PROGRAM};

#[spec_test(REQ_DS_mcp_001)]
fn mcp_spec_req_ds_001_severity_is_the_name_of_the_category_of_the_diagnostic() {
    let diagnostic = Diagnostic::text(
        SourceSpan::default(),
        text::syntax_error_a_task_needs_a_priority(),
    );
    assert_eq!(
        serialize_diagnostic(&diagnostic)["severity"].as_str(),
        Some("error")
    );

    for (severity, name) in [
        (Severity::FatalError, "fatal-error"),
        (Severity::Error, "error"),
        (Severity::Warning, "warning"),
        (Severity::Information, "information"),
        (Severity::Text, "text"),
        (Severity::SuppressedWarning, "suppressed-warning"),
        (Severity::SuppressedInformation, "suppressed-information"),
    ] {
        let json = serialize_diagnostic(&diagnostic.clone().with_severity_for_test(severity));

        assert_eq!(json["severity"].as_str(), Some(name));
        assert_eq!(json["severity"].as_str(), Some(severity.name()));
    }
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

#[spec_test(REQ_DS_mcp_003)]
fn mcp_spec_req_ds_003_a_suppressed_diagnostic_is_serialized_with_its_category() {
    let warning = Diagnostic::text(
        SourceSpan::default(),
        text::syntax_error_a_task_needs_a_priority(),
    )
    .with_severity_for_test(Severity::Warning);

    let json = serialize_diagnostics(&[warning.clone(), warning.suppress()]);

    assert_eq!(json.len(), 2);
    assert_eq!(json[0]["severity"].as_str(), Some("warning"));
    assert_eq!(json[1]["severity"].as_str(), Some("suppressed-warning"));
}
