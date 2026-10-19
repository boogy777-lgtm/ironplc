//! The area the language server writes: the `source` of a diagnostic, what the
//! editor holds of a run, and which messages reach it (`REQ-DS-cli-012`,
//! `REQ-DS-cli-013`).

use std::str::FromStr;

use ironplc_dsl::core::{FileId, SourceSpan};
use ironplc_dsl::diagnostic::{Area, Diagnostic};
use ironplc_problems::text;
use lsp_types::Uri;
use spec_test_macro::spec_test;

use super::scripted::Scripted;
use super::{LspProject, UriKey};

#[cfg(target_os = "windows")]
fn uri_of(name: &str) -> Uri {
    Uri::from_str(&format!("file:///C:/{name}")).unwrap()
}

#[cfg(not(target_os = "windows"))]
fn uri_of(name: &str) -> Uri {
    Uri::from_str(&format!("file:///localhost/{name}")).unwrap()
}

/// A project whose analysis reports `produced`, with the documents `files`
/// edited in this order.
fn project_with(files: &[&str], produced: Vec<Diagnostic>) -> LspProject {
    let mut project = LspProject::new(Box::new(Scripted(produced)));
    for name in files {
        project.change_text_document(&uri_of(name), "PROGRAM Main\nEND_PROGRAM".to_owned());
    }
    project
}

/// `count` different errors in the file `name`.
fn errors_in(name: &str, count: usize) -> Vec<Diagnostic> {
    let file_id = FileId::from_path(std::path::Path::new(uri_of(name).path().as_str()));
    (0..count)
        .map(|at| {
            Diagnostic::text(
                SourceSpan::range(at, at + 1).with_file_id(&file_id),
                text::variable_undefined(),
            )
        })
        .collect()
}

fn held_by(analysis: &super::Analysis, name: &str) -> usize {
    analysis
        .problems
        .get(&UriKey::from_uri(&uri_of(name)))
        .map_or(0, Vec::len)
}

#[spec_test(REQ_DS_cli_012)]
fn semantic_all_when_a_problem_then_the_source_is_that_of_the_precompile_area() {
    let mut project = project_with(&["a.st"], errors_in("a.st", 1));

    let analysis = project.semantic_all();

    let published: Vec<_> = analysis.problems.values().flatten().collect();
    assert_eq!(published.len(), 1);
    assert_eq!(
        published[0].source.as_deref(),
        Some(Area::Precompile.source())
    );
    assert_eq!(published[0].source.as_deref(), Some("IronPLC Precompile"));
}

#[spec_test(REQ_DS_cli_012)]
fn semantic_all_when_a_problem_without_a_usable_file_then_the_edited_file_has_it() {
    // A message about no file of the workspace: the built-in library.
    let diagnostic = Diagnostic::text(
        SourceSpan::range(0, 1).with_file_id(&FileId::BuiltIn),
        text::variable_undefined(),
    );
    let mut project = project_with(&["a.st", "b.st"], vec![diagnostic]);

    let analysis = project.semantic_all();

    assert_eq!(held_by(&analysis, "b.st"), 1);
    assert_eq!(held_by(&analysis, "a.st"), 0);
}

#[spec_test(REQ_DS_cli_013)]
fn semantic_all_when_951_problems_in_one_file_then_the_first_950_are_published() {
    let mut project = project_with(&["a.st"], errors_in("a.st", 951));

    let analysis = project.semantic_all();

    assert_eq!(held_by(&analysis, "a.st"), 950);
    let published = &analysis.problems[&UriKey::from_uri(&uri_of("a.st"))];
    assert_eq!(published[0].range.start.character, 0);
}

#[spec_test(REQ_DS_cli_013)]
fn semantic_all_when_more_than_950_in_two_files_then_the_oldest_written_file_loses_first() {
    // `b.st` is edited last, so it is written after `a.st`.
    let mut produced = errors_in("a.st", 600);
    produced.extend(errors_in("b.st", 600));
    let mut project = project_with(&["a.st", "b.st"], produced);

    let analysis = project.semantic_all();

    assert_eq!(held_by(&analysis, "a.st"), 350);
    assert_eq!(held_by(&analysis, "b.st"), 600);
}

#[spec_test(REQ_DS_cli_013)]
fn semantic_all_when_the_file_that_comes_first_is_edited_last_then_the_other_loses_first() {
    // `a.st` comes first in ascending order and is edited last, so it is the
    // newest and `b.st` is the oldest.
    let mut produced = errors_in("a.st", 600);
    produced.extend(errors_in("b.st", 600));
    let mut project = project_with(&["b.st", "a.st"], produced);

    let analysis = project.semantic_all();

    assert_eq!(held_by(&analysis, "a.st"), 600);
    assert_eq!(held_by(&analysis, "b.st"), 350);
}

#[spec_test(REQ_DS_cli_013)]
fn semantic_all_when_a_file_loses_all_its_messages_then_it_is_not_published() {
    let mut produced = errors_in("a.st", 400);
    produced.extend(errors_in("b.st", 950));
    let mut project = project_with(&["a.st", "b.st"], produced);

    let analysis = project.semantic_all();

    assert_eq!(held_by(&analysis, "b.st"), 950);
    assert!(!analysis
        .problems
        .contains_key(&UriKey::from_uri(&uri_of("a.st"))));
}
