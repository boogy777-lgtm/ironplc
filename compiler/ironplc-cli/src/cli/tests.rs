//! Tests for the command-line commands: check, compile, tokenize, uid sync and mapping, and help-URL formatting.

use ironplc_test::shared_resource_path;

use ironplc_parser::options::CompilerOptions;

use crate::{
    cli::check, cli::compile, cli::map_uid, cli::sync_uids, cli::tokenize,
    test_helpers::resource_path,
};
use ironplc_project::sidecar::Sidecar;

// The plain valid/syntax-error/semantic-error outcomes of check, echo,
// tokenize, and compile against the shared fixtures are asserted by the
// subprocess suite in tests/cli.rs, which pins the exit-code and
// stdout/stderr contract as well. The tests here cover what that suite
// cannot reach: directory input, the diagnostic-accumulation contract
// (the `problem_count` cases below), XML tokenize, container round-trip,
// and help-URL formatting.

/// Extracts the problem count from a command's failure message
/// (`"<Command> failed with N problem(s)"`). The count is the observable
/// that proves how many stages contributed diagnostics: a
/// discovery-only failure reports 1, while discovery + a real
/// analysis finding reports 2 or more.
fn problem_count(err: &str) -> usize {
    let parsed = err
        .rsplit_once(" failed with ")
        .and_then(|(_, rest)| rest.strip_suffix(" problem(s)"))
        .and_then(|n| n.parse().ok());
    assert!(parsed.is_some(), "not a command failure message: {err}");
    parsed.unwrap()
}

#[test]
fn problem_help_url_when_diagnostic_then_url_tagged_for_cli() {
    use ironplc_dsl::core::SourceSpan;
    use ironplc_dsl::diagnostic::Diagnostic;
    use ironplc_problems::text;

    let diag = Diagnostic::text(
        SourceSpan::default(),
        text::syntax_error_a_task_needs_a_priority(),
    )
    .with_source("compiler/analyzer/src/rule_example.rs", 42);

    let url = super::problem_help_url(&diag);
    assert!(url.contains("/reference/compiler/problems/"));
    assert!(url.contains("?version="));
    assert!(url.contains("&channel=cli"));
    assert!(url.contains("&file=compiler/analyzer/src/rule_example.rs"));
    assert!(url.contains("&line=42"));
}

#[test]
fn check_first_steps_dir_when_valid_syntax_then_ok() {
    let paths = vec![resource_path("set")];
    let result = check(&paths, CompilerOptions::default(), &[], true);
    assert!(result.is_ok())
}

#[test]
fn check_when_plcproj_references_missing_file_then_error() {
    // Maintainer feedback on the PR that introduced discovery
    // continuing past one bad `.plcproj` entry: the missing
    // reference itself must still fail the overall command, even
    // though discovery no longer aborts because of it.
    let dir = tempfile::TempDir::new().unwrap();
    std::fs::write(
        dir.path().join("project.plcproj"),
        r#"<Project>
  <ItemGroup>
    <Compile Include="MISSING.TcPOU" />
  </ItemGroup>
</Project>"#,
    )
    .unwrap();

    let paths = vec![dir.path().to_path_buf()];
    let result = check(&paths, CompilerOptions::default(), &[], true);
    assert!(result.is_err())
}

#[test]
fn check_when_plcproj_named_as_file_argument_then_loads_its_sources() {
    // A manifest given by name resolves as a project rather than
    // being read as source text -- this is how a user says which
    // project they mean when a directory holds more than one.
    let dir = tempfile::TempDir::new().unwrap();
    std::fs::write(
        dir.path().join("A.st"),
        "PROGRAM A\nVAR\n    x : INT;\nEND_VAR\n    x := UNDECLARED_VAR;\nEND_PROGRAM",
    )
    .unwrap();
    std::fs::write(
        dir.path().join("project.plcproj"),
        r#"<Project>
  <ItemGroup>
    <Compile Include="A.st" />
  </ItemGroup>
</Project>"#,
    )
    .unwrap();

    let paths = vec![dir.path().join("project.plcproj")];
    let err = check(&paths, CompilerOptions::default(), &[], true).unwrap_err();

    // Exactly the semantic error in A.st: the .plcproj resolved to
    // its sources instead of being parsed as Structured Text, which
    // would have produced a syntax error against the XML itself.
    assert_eq!(problem_count(&err), 1, "{err}");
}

#[test]
fn check_when_ambiguous_plcproj_directory_then_naming_one_resolves_it() {
    // Two .plcproj in one folder name no single project, so the
    // folder is enumerated as loose sources -- MISSING.TcPOU is never
    // looked for, because no project file was followed. Naming one
    // resolves that project, and its missing entry then fails.
    let dir = tempfile::TempDir::new().unwrap();
    std::fs::write(dir.path().join("A.st"), "PROGRAM A\nEND_PROGRAM").unwrap();
    for name in ["AAA.plcproj", "ZZZ.plcproj"] {
        std::fs::write(
            dir.path().join(name),
            r#"<Project>
  <ItemGroup>
    <Compile Include="A.st" />
    <Compile Include="MISSING.TcPOU" />
  </ItemGroup>
</Project>"#,
        )
        .unwrap();
    }

    assert!(check(
        &[dir.path().to_path_buf()],
        CompilerOptions::default(),
        &[],
        true
    )
    .is_ok());

    let named = vec![dir.path().join("ZZZ.plcproj")];
    let err = check(&named, CompilerOptions::default(), &[], true).unwrap_err();
    assert_eq!(problem_count(&err), 1, "{err}");
}

#[test]
fn check_when_plcproj_has_valid_and_missing_entries_then_semantic_error_still_surfaces() {
    // The valid entry must still be loaded and checked -- not just
    // that the overall command fails (which a discovery-time-only
    // failure would also produce), but specifically that
    // project.semantic() actually ran against it. A.st here has a
    // real semantic error (an undeclared variable): if analysis
    // never ran, the failure message would report only the single
    // discovery problem -- a count of 2 or more is what
    // distinguishes "aborted before checking" from "checked, and
    // found a real bug".
    let dir = tempfile::TempDir::new().unwrap();
    std::fs::write(
        dir.path().join("A.st"),
        "PROGRAM A\nVAR\n    x : INT;\nEND_VAR\n    x := UNDECLARED_VAR;\nEND_PROGRAM",
    )
    .unwrap();
    std::fs::write(
        dir.path().join("project.plcproj"),
        r#"<Project>
  <ItemGroup>
    <Compile Include="A.st" />
    <Compile Include="MISSING.TcPOU" />
  </ItemGroup>
</Project>"#,
    )
    .unwrap();

    let paths = vec![dir.path().to_path_buf()];
    let result = check(&paths, CompilerOptions::default(), &[], true);
    let err = result.unwrap_err();
    assert!(
        problem_count(&err) >= 2,
        "expected the discovery problem AND a semantic problem, got: {err}"
    );
}

#[test]
fn check_when_plcproj_references_unbundled_library_then_still_runs_analysis() {
    // A project referencing a library IronPLC doesn't bundle (P6011)
    // must not prevent analysis of the project's own files -- same
    // reasoning as the missing-.plcproj-entry case above, for the
    // other kind of discovery-time diagnostic.
    let dir = tempfile::TempDir::new().unwrap();
    std::fs::write(
        dir.path().join("A.st"),
        "PROGRAM A\nVAR\n    x : INT;\nEND_VAR\n    x := UNDECLARED_VAR;\nEND_PROGRAM",
    )
    .unwrap();
    std::fs::write(
        dir.path().join("project.plcproj"),
        r#"<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Compile Include="A.st" />
    <PlaceholderReference Include="NotBundled" />
  </ItemGroup>
</Project>"#,
    )
    .unwrap();

    let paths = vec![dir.path().to_path_buf()];
    let result = check(&paths, CompilerOptions::default(), &[], true);
    let err = result.unwrap_err();
    assert!(
        problem_count(&err) >= 2,
        "expected the P6011 discovery problem AND a semantic problem, got: {err}"
    );
}

#[test]
fn tokenize_xml_when_valid_syntax_then_ok() {
    let paths = vec![resource_path("simple.xml")];
    let result = tokenize(&paths, CompilerOptions::default(), true);
    assert!(result.is_ok())
}

#[test]
fn compile_when_plcproj_references_unbundled_library_then_error_and_no_container() {
    // Codegen runs only on a project with no problems at all. A
    // discovery-time diagnostic (P6011, unbundled library) fails the
    // command AND suppresses the container: a failing command must
    // not leave behind a deployable artifact, even when the program
    // never calls anything from the unbundled library. (If P6011
    // should ever stop blocking compilation, the fix is diagnostic
    // severities -- make it a warning -- not a codegen exception.)
    let dir = tempfile::TempDir::new().unwrap();
    std::fs::write(
        dir.path().join("A.st"),
        "PROGRAM A\nVAR\n    x : INT;\nEND_VAR\n    x := 1;\nEND_PROGRAM",
    )
    .unwrap();
    std::fs::write(
        dir.path().join("project.plcproj"),
        r#"<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Compile Include="A.st" />
    <PlaceholderReference Include="NotBundled" />
  </ItemGroup>
</Project>"#,
    )
    .unwrap();

    let paths = vec![dir.path().to_path_buf()];
    let output = tempfile::NamedTempFile::new().unwrap();
    let result = compile(&paths, output.path(), CompilerOptions::default(), &[], true);

    assert!(result.is_err());
    assert_eq!(output.path().metadata().unwrap().len(), 0);
}

#[test]
fn compile_when_output_is_valid_container_then_roundtrips() {
    let paths = vec![shared_resource_path("steel_thread.st")];
    let output = tempfile::NamedTempFile::new().unwrap();
    compile(&paths, output.path(), CompilerOptions::default(), &[], true).unwrap();

    // Verify the output is a valid container by reading it back
    let mut file = std::fs::File::open(output.path()).unwrap();
    let container = ironplc_container::Container::read_from(&mut file).unwrap();
    assert_eq!(container.header.num_variables, 2);
    assert_eq!(container.header.num_functions, 2);
}

#[test]
fn compile_when_structured_variable_then_error() {
    use std::io::Write;

    let mut source = tempfile::NamedTempFile::new().unwrap();
    write!(
        source,
        "FUNCTION MY_FUNC : BOOL
  VAR_INPUT
      x : BYTE;
  END_VAR
      IF setup.EXTENDED_ASCII THEN
          MY_FUNC := TRUE;
      ELSE
          MY_FUNC := FALSE;
      END_IF;
  END_FUNCTION
  PROGRAM main
  VAR
      result : BOOL;
  END_VAR
      result := MY_FUNC(x := BYTE#65);
  END_PROGRAM"
    )
    .unwrap();

    let output = tempfile::NamedTempFile::new().unwrap();
    let result = compile(
        &[source.path().to_path_buf()],
        output.path(),
        CompilerOptions::default(),
        &[],
        true,
    );
    assert!(result.is_err());
}

// -----------------------------------------------------------------
// Refactor commands: UID sidecar maintenance (ADR 0053).
// -----------------------------------------------------------------

const COUNTER_PROGRAM: &str = "PROGRAM main VAR Counter : INT; END_VAR Counter := 1; END_PROGRAM";

/// Creates `name` as a subdirectory of a temporary directory holding
/// `COUNTER_PROGRAM` as main.st, returning both.
fn project_dir_with_counter(temp: &tempfile::TempDir, name: &str) -> std::path::PathBuf {
    let dir = temp.path().join(name);
    std::fs::create_dir(&dir).unwrap();
    std::fs::write(dir.join("main.st"), COUNTER_PROGRAM).unwrap();
    dir
}

#[test]
fn sync_uids_when_valid_project_then_creates_sidecar_and_reports() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = project_dir_with_counter(&temp, "proj");

    let result = sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true);

    assert!(result.is_ok(), "unexpected error: {:?}", result.err());
    let sidecar_path = ironplc_project::sidecar_path_for(&dir).unwrap();
    let text = std::fs::read_to_string(&sidecar_path).unwrap();
    assert!(text.contains("\"Counter\""));
}

#[test]
fn sync_uids_when_run_twice_then_key_preserved() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = project_dir_with_counter(&temp, "proj");
    sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true).unwrap();

    let sidecar_path = ironplc_project::sidecar_path_for(&dir).unwrap();
    let before = std::fs::read_to_string(&sidecar_path).unwrap();
    sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true).unwrap();
    let after = std::fs::read_to_string(&sidecar_path).unwrap();

    // Nothing changed between runs, so the deterministic serialization
    // is byte-identical.
    assert_eq!(before, after);
}

#[test]
fn sync_uids_when_project_has_error_then_no_sidecar() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = temp.path().join("proj");
    std::fs::create_dir(&dir).unwrap();
    std::fs::write(
        dir.join("main.st"),
        "PROGRAM main VAR x : INT; END_VAR x := undeclared_var; END_PROGRAM",
    )
    .unwrap();

    let result = sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true);

    assert!(result.is_err());
    assert!(!ironplc_project::sidecar_path_for(&dir).unwrap().exists());
}

#[test]
fn sync_uids_when_rename_candidate_then_sidecar_unchanged() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = project_dir_with_counter(&temp, "proj");
    sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true).unwrap();
    let sidecar_path = ironplc_project::sidecar_path_for(&dir).unwrap();
    let before = std::fs::read_to_string(&sidecar_path).unwrap();

    // Rename in the source: the next sync reports a rename candidate
    // but must not persist it — saving would drop the removed key and
    // leave nothing for `map-uid` to move.
    std::fs::write(
        dir.join("main.st"),
        "PROGRAM main VAR Ticks : INT; END_VAR Ticks := 1; END_PROGRAM",
    )
    .unwrap();
    let result = sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true);

    assert!(result.is_ok(), "unexpected error: {:?}", result.err());
    let after = std::fs::read_to_string(&sidecar_path).unwrap();
    assert_eq!(before, after);
}

#[test]
fn sync_uids_when_candidate_resolved_with_map_uid_then_uid_preserved() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = project_dir_with_counter(&temp, "proj");
    sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true).unwrap();

    // The full resolution flow: sync reports the candidate without
    // rewriting the sidecar, `map-uid` moves the UID, and the re-sync
    // persists the moved UID as preserved.
    std::fs::write(
        dir.join("main.st"),
        "PROGRAM main VAR Ticks : INT; END_VAR Ticks := 1; END_PROGRAM",
    )
    .unwrap();
    sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true).unwrap();
    map_uid(&dir, "main", "Counter", "main", "Ticks").unwrap();
    let result = sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true);

    assert!(result.is_ok(), "unexpected error: {:?}", result.err());
    let sidecar = Sidecar::load(&ironplc_project::sidecar_path_for(&dir).unwrap()).unwrap();
    let entries: Vec<(String, u64)> = sidecar
        .entries()
        .map(|(key, uid)| (key.name().to_string(), uid))
        .collect();
    assert_eq!(entries, vec![(String::from("Ticks"), 1)]);
}

#[test]
fn map_uid_when_old_key_known_then_moves_uid() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = project_dir_with_counter(&temp, "proj");
    sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true).unwrap();
    let sidecar_path = ironplc_project::sidecar_path_for(&dir).unwrap();
    let before = std::fs::read_to_string(&sidecar_path).unwrap();
    assert!(before.contains("\"Counter\""));

    let result = map_uid(&dir, "main", "Counter", "main", "Ticks");

    assert!(result.is_ok(), "unexpected error: {:?}", result.err());
    let after = std::fs::read_to_string(&sidecar_path).unwrap();
    assert!(after.contains("\"Ticks\""));
    assert!(!after.contains("\"Counter\""));
}

#[test]
fn map_uid_when_old_key_unknown_then_error_and_sidecar_unchanged() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = project_dir_with_counter(&temp, "proj");
    sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true).unwrap();
    let sidecar_path = ironplc_project::sidecar_path_for(&dir).unwrap();
    let before = std::fs::read_to_string(&sidecar_path).unwrap();

    let result = map_uid(&dir, "main", "Missing", "main", "Ticks");

    assert!(result.is_err());
    let after = std::fs::read_to_string(&sidecar_path).unwrap();
    assert_eq!(before, after);
}

#[test]
fn map_uid_when_new_key_already_mapped_then_error() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = temp.path().join("proj");
    std::fs::create_dir(&dir).unwrap();
    std::fs::write(
        dir.join("main.st"),
        "PROGRAM main VAR Counter : INT; Other : INT; END_VAR Counter := 1; END_PROGRAM",
    )
    .unwrap();
    sync_uids(std::slice::from_ref(&dir), CompilerOptions::default(), true).unwrap();

    let result = map_uid(&dir, "main", "Counter", "main", "Other");

    assert!(result.is_err());
}

#[test]
fn display_of_when_each_category_then_its_terminal_display() {
    use super::Display;
    use codespan_reporting::diagnostic::Severity as CodeSpanSeverity;
    use ironplc_dsl::diagnostic::Severity;

    for (severity, expected) in [
        (
            Severity::FatalError,
            Display::Report(CodeSpanSeverity::Error),
        ),
        (Severity::Error, Display::Report(CodeSpanSeverity::Error)),
        (
            Severity::Warning,
            Display::Report(CodeSpanSeverity::Warning),
        ),
        (
            Severity::Information,
            Display::Report(CodeSpanSeverity::Note),
        ),
        (Severity::Text, Display::TextOnly),
        (Severity::SuppressedWarning, Display::Hidden),
        (Severity::SuppressedInformation, Display::Hidden),
    ] {
        assert_eq!(super::display_of(severity), expected, "{severity:?}");
        // What is shown is what the category says is shown.
        assert_eq!(
            super::display_of(severity) != Display::Hidden,
            severity.is_shown(),
            "{severity:?}"
        );
    }
}

#[test]
fn text_of_when_label_then_the_label_and_otherwise_the_description() {
    use ironplc_dsl::core::SourceSpan;
    use ironplc_dsl::diagnostic::{Diagnostic, Label};
    use ironplc_problems::text;

    let labelled = Diagnostic::text(
        SourceSpan::default(),
        text::syntax_error_a_task_needs_a_priority(),
    );
    assert_eq!(
        super::text_of(&labelled),
        "a task needs a `PRIORITY`".to_string()
    );

    let mut unlabelled = labelled.clone();
    unlabelled.primary = Label::span(SourceSpan::default(), "");
    assert_eq!(super::text_of(&unlabelled), unlabelled.description());
}

#[test]
fn map_diagnostic_when_severity_given_then_codespan_diagnostic_has_it() {
    use codespan_reporting::diagnostic::Severity as CodeSpanSeverity;
    use ironplc_dsl::core::SourceSpan;
    use ironplc_dsl::diagnostic::Diagnostic;
    use ironplc_problems::text;

    let diag = Diagnostic::text(
        SourceSpan::default(),
        text::syntax_error_a_task_needs_a_priority(),
    );

    let mapped = super::map_diagnostic(
        &diag,
        CodeSpanSeverity::Warning,
        &std::collections::HashMap::new(),
    );

    assert_eq!(mapped.severity, CodeSpanSeverity::Warning);
    assert_eq!(mapped.code.as_deref(), Some("P0002"));
}

#[test]
fn finish_when_no_diagnostics_then_ok() {
    assert_eq!(super::finish("Check", vec![], None, true), Ok(()));
}

#[test]
fn finish_when_errors_then_failure_names_the_error_count() {
    use ironplc_dsl::core::SourceSpan;
    use ironplc_dsl::diagnostic::Diagnostic;
    use ironplc_problems::text;

    let diagnostics = vec![
        Diagnostic::text(
            SourceSpan::default(),
            text::syntax_error_a_task_needs_a_priority(),
        ),
        Diagnostic::text(
            SourceSpan::default(),
            text::syntax_error_a_task_needs_a_priority(),
        ),
    ];

    let result = super::finish("Check", diagnostics, None, true);

    assert_eq!(result, Err("Check failed with 2 problem(s)".to_string()));
}
