//! A diagnostic whose span is empty (something is missing there) reaches the
//! editor as a range of one character; the span of the diagnostic is not
//! changed.

use std::str::FromStr;

use ironplc_dsl::core::{FileId, SourceSpan};
use ironplc_dsl::diagnostic::{Diagnostic, Label, Location};
use lsp_types::{Range, Uri};

use ironplc_project::FileBackedProject;

use super::{map_diagnostic, LspProject};

#[cfg(target_os = "macos")]
static FAKE_PATH: &str = "file:///localhost/missing.st";
#[cfg(target_os = "linux")]
static FAKE_PATH: &str = "file:///localhost/missing.st";
#[cfg(target_os = "windows")]
static FAKE_PATH: &str = "file:///C:/missing.st";

/// A project holding `content`, and the id of its file.
fn project_with(content: &str) -> (LspProject, FileId) {
    let mut project = LspProject::new(Box::new(FileBackedProject::new()));
    let url = Uri::from_str(FAKE_PATH).unwrap();
    project.change_text_document(&url, content.to_owned());
    let file_id = FileId::from_path(&std::path::PathBuf::from(url.path().as_str()));
    (project, file_id)
}

/// The range of the primary label of an empty span at `at` in `content`.
fn range_of_empty_span_at(content: &str, at: usize) -> Range {
    let (project, file_id) = project_with(content);
    let diagnostic = Diagnostic::text(
        SourceSpan::range(at, at).with_file_id(&file_id),
        ironplc_problems::text::syntax_error_a_task_needs_a_priority(),
    );
    map_diagnostic(diagnostic, project.wrapped.as_ref()).range
}

fn characters(range: Range) -> (u32, u32, u32, u32) {
    (
        range.start.line,
        range.start.character,
        range.end.line,
        range.end.character,
    )
}

const TWO_LINES: &str = "x := 1;\ny := 2;";

#[test]
fn map_diagnostic_when_primary_span_is_empty_mid_line_then_the_next_character() {
    let range = range_of_empty_span_at(TWO_LINES, 5);
    assert_eq!(characters(range), (0, 5, 0, 6));
}

#[test]
fn map_diagnostic_when_primary_span_is_empty_at_start_of_file_then_the_first_character() {
    let range = range_of_empty_span_at(TWO_LINES, 0);
    assert_eq!(characters(range), (0, 0, 0, 1));
}

#[test]
fn map_diagnostic_when_primary_span_is_empty_at_end_of_line_then_the_previous_character() {
    let range = range_of_empty_span_at(TWO_LINES, 7);
    assert_eq!(characters(range), (0, 6, 0, 7));
}

#[test]
fn map_diagnostic_when_primary_span_is_empty_at_end_of_input_then_the_previous_character() {
    let range = range_of_empty_span_at(TWO_LINES, 15);
    assert_eq!(characters(range), (1, 6, 1, 7));
}

#[test]
fn map_diagnostic_when_primary_span_is_empty_after_a_final_line_break_then_the_last_character() {
    let range = range_of_empty_span_at("x := 1;\n", 8);
    assert_eq!(characters(range), (0, 6, 0, 7));
}

#[test]
fn map_diagnostic_when_file_is_empty_then_the_range_stays_empty() {
    let range = range_of_empty_span_at("", 0);
    assert_eq!(characters(range), (0, 0, 0, 0));
}

#[test]
fn map_diagnostic_when_primary_span_has_bytes_then_the_range_is_the_span() {
    let (project, file_id) = project_with("x := 1;\ny := 2;");
    let diagnostic = Diagnostic::text(
        SourceSpan::range(5, 9).with_file_id(&file_id),
        ironplc_problems::text::syntax_error_a_task_needs_a_priority(),
    );
    let range = map_diagnostic(diagnostic, project.wrapped.as_ref()).range;
    assert_eq!(characters(range), (0, 5, 1, 1));
}

#[test]
fn map_diagnostic_when_secondary_span_is_empty_then_its_range_is_one_character() {
    let (project, file_id) = project_with("x := 1;\ny := 2;");
    let diagnostic = Diagnostic::text(
        SourceSpan::range(0, 1).with_file_id(&file_id),
        ironplc_problems::text::syntax_error_a_task_needs_a_priority(),
    )
    .with_secondary(Label {
        location: Location { start: 7, end: 7 },
        file_id,
        message: "here".to_string(),
    });
    let related = map_diagnostic(diagnostic, project.wrapped.as_ref())
        .related_information
        .unwrap();
    assert_eq!(characters(related[0].location.range), (0, 6, 0, 7));
}

#[test]
fn map_diagnostic_when_file_is_unknown_then_the_range_is_the_start_of_the_file() {
    let (project, _) = project_with("x := 1;");
    let diagnostic = Diagnostic::text(
        SourceSpan::range(3, 3).with_file_id(&FileId::from_string("other.st")),
        ironplc_problems::text::syntax_error_a_task_needs_a_priority(),
    );
    let range = map_diagnostic(diagnostic, project.wrapped.as_ref()).range;
    assert_eq!(characters(range), (0, 0, 0, 0));
}
