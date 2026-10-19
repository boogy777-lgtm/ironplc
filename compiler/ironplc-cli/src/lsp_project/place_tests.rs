//! Where the editor shows a message of each category: the Problems panel, the
//! Output channel, or nowhere (`REQ-DS-cli-004`).

use std::str::FromStr;

use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::Diagnostic;
use lsp_types::Uri;
use spec_test_macro::spec_test;

use ironplc_project::FileBackedProject;

use super::LspProject;

#[cfg(target_os = "macos")]
static FAKE_PATH: &str = "file:///localhost/missing.st";
#[cfg(target_os = "linux")]
static FAKE_PATH: &str = "file:///localhost/missing.st";
#[cfg(target_os = "windows")]
static FAKE_PATH: &str = "file:///C:/missing.st";

fn new_empty_project() -> LspProject {
    LspProject::new(Box::new(FileBackedProject::new()))
}

#[spec_test(REQ_DS_cli_004)]
fn place_of_when_each_category_then_where_the_editor_shows_it() {
    use super::Place;
    use ironplc_dsl::diagnostic::Severity;
    use lsp_types::DiagnosticSeverity;

    for (severity, expected) in [
        (
            Severity::FatalError,
            Place::Problems(DiagnosticSeverity::ERROR),
        ),
        (Severity::Error, Place::Problems(DiagnosticSeverity::ERROR)),
        (
            Severity::Warning,
            Place::Problems(DiagnosticSeverity::WARNING),
        ),
        (
            Severity::Information,
            Place::Problems(DiagnosticSeverity::INFORMATION),
        ),
        (Severity::Text, Place::Output),
        (Severity::SuppressedWarning, Place::Nowhere),
        (Severity::SuppressedInformation, Place::Nowhere),
    ] {
        assert_eq!(super::place_of(severity), expected, "{severity:?}");
        // What is shown is what the category says is shown.
        assert_eq!(
            super::place_of(severity) != Place::Nowhere,
            severity.is_shown(),
            "{severity:?}"
        );
    }
}

#[spec_test(REQ_DS_cli_004)]
fn analysis_of_when_each_category_then_problems_log_or_nothing() {
    use ironplc_dsl::diagnostic::Severity;
    use lsp_types::DiagnosticSeverity;

    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    proj.change_text_document(
        &url,
        "PROGRAM Main
END_PROGRAM"
            .to_owned(),
    );
    let file = ironplc_dsl::core::FileId::from_path(std::path::Path::new(FAKE_PATH));
    let made = |severity| {
        Diagnostic::text(
            SourceSpan {
                start: 0,
                end: 1,
                file_id: file.clone(),
            },
            ironplc_problems::text::syntax_error_a_task_needs_a_priority(),
        )
        .with_severity_for_test(severity)
    };

    let analysis = proj.analysis_of(vec![
        made(Severity::FatalError),
        made(Severity::Error),
        made(Severity::Warning),
        made(Severity::Information),
        made(Severity::Text),
        made(Severity::SuppressedWarning),
        made(Severity::SuppressedInformation),
    ]);

    let published: Vec<_> = analysis.problems.values().flatten().collect();
    let severities: Vec<_> = published.iter().filter_map(|d| d.severity).collect();
    assert_eq!(
        severities,
        vec![
            DiagnosticSeverity::ERROR,
            DiagnosticSeverity::ERROR,
            DiagnosticSeverity::WARNING,
            DiagnosticSeverity::INFORMATION
        ]
    );
    // The text is not a diagnostic of the Problems panel; it is one line of the log.
    assert_eq!(published.len(), 4);
    assert_eq!(analysis.log, vec!["a task needs a `PRIORITY`".to_string()]);
}

#[spec_test(REQ_DS_cli_011)]
fn analysis_of_when_messages_without_a_position_then_neither_published_nor_logged() {
    use ironplc_problems::codeless;

    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    proj.change_text_document(
        &url,
        "PROGRAM Main
END_PROGRAM"
            .to_owned(),
    );

    // The limit lines and the closing line are made without a position, so they
    // are not shown in the Precompile area the language server writes.
    let analysis = proj.analysis_of(vec![
        Diagnostic::codeless(codeless::error_limit_exceeded(500)),
        Diagnostic::codeless(codeless::warning_limit_exceeded(500)),
        Diagnostic::codeless(codeless::compile_complete_errors(501, 501)),
    ]);

    assert!(analysis.problems.is_empty());
    assert!(analysis.log.is_empty());
}
