//! The summary lines of `check` and `compile`, the line of a limit and the form of
//! a message without a code for a person (`REQ-DS-cli-006` to `REQ-DS-cli-010`).

use ironplc_parser::options::CompilerOptions;
use spec_test_macro::spec_test;

use super::tests::TWO_ENUMERATIONS_COMPARED;
use super::{check, compile, Output};

use codespan_reporting::term::termcolor::NoColor;
use ironplc_dsl::diagnostic::Diagnostic;

/// A source file named `name` in a directory of its own.
fn source_named(name: &str, text: &str) -> (tempfile::TempDir, std::path::PathBuf) {
    let dir = tempfile::TempDir::new().unwrap();
    let path = dir.path().join(name);
    std::fs::write(&path, text).unwrap();
    (dir, path)
}

const UNDECLARED_VARIABLE: &str = "PROGRAM main
VAR y : INT; END_VAR
  y := x + 1;
END_PROGRAM
";

const CLEAN_PROGRAM: &str = "PROGRAM main
VAR y : INT; END_VAR
  y := 1;
END_PROGRAM
";

/// What the terminal shows for `diagnostics`, one string with the lines in order.
fn person_form(
    diagnostics: &[Diagnostic],
    project: Option<&ironplc_project::FileBackedProject>,
) -> String {
    let mut writer = NoColor::new(Vec::new());
    super::write_for_person(&mut writer, diagnostics, project);
    String::from_utf8(writer.into_inner()).unwrap()
}

/// The lines of a text that have no leading space, which are the headers and
/// plain lines and not the lines of a source snippet.
fn headed_lines(shown: &str) -> Vec<&str> {
    shown
        .lines()
        .filter(|line| !line.is_empty() && !line.starts_with(' '))
        .collect()
}

#[spec_test(REQ_DS_cli_006)]
fn check_when_counts_then_the_last_line_is_compile_complete_with_the_counts() {
    for (text, expected, ok) in [
        (
            CLEAN_PROGRAM,
            "Compile complete -- 0 errors, 0 warnings",
            true,
        ),
        (
            UNDECLARED_VARIABLE,
            "Compile complete -- 1 errors, 0 warnings",
            false,
        ),
        (
            TWO_ENUMERATIONS_COMPARED,
            "Compile complete -- 0 errors, 1 warnings",
            true,
        ),
    ] {
        let (_dir, path) = source_named("main.st", text);
        let paths = vec![path];

        let (project, diagnostics) = super::run_check(&paths, CompilerOptions::default(), &[]);
        let shown = person_form(&diagnostics, Some(&project));
        let result = check(&paths, CompilerOptions::default(), &[], Output::Silent);

        assert_eq!(headed_lines(&shown).last().copied(), Some(expected));
        assert_eq!(result.is_ok(), ok, "{expected}");
        // A text has no header and no code.
        assert!(!shown.contains("]: Compile complete"));
    }
}

#[spec_test(REQ_DS_cli_007)]
fn compile_when_only_a_warning_then_build_started_first_and_ready_for_download_last() {
    let (dir, path) = source_named("Main.st", TWO_ENUMERATIONS_COMPARED);
    let container = dir.path().join("out.iplc");

    let (project, diagnostics) = super::run_compile(
        std::slice::from_ref(&path),
        &container,
        CompilerOptions::default(),
        &[],
    )
    .unwrap();
    let shown = person_form(&diagnostics, Some(&project));

    let headed = headed_lines(&shown);
    assert_eq!(
        headed.first().copied(),
        Some("------ Build started: Application: Main -------")
    );
    assert!(
        headed[1].starts_with("Warning[P4083] (CODESYS C0354)"),
        "{shown}"
    );
    assert_eq!(
        headed.last().copied(),
        Some("Build complete -- 0 errors, 1 warnings : Ready for download")
    );
    assert!(std::fs::metadata(&container).unwrap().len() > 0);
    let result = compile(
        std::slice::from_ref(&path),
        &container,
        CompilerOptions::default(),
        &[],
        true,
    );
    assert!(result.is_ok());
}

#[spec_test(REQ_DS_cli_007)]
fn compile_when_a_directory_is_given_then_the_application_is_the_name_of_the_directory() {
    let dir = tempfile::Builder::new().prefix("plant").tempdir().unwrap();
    std::fs::write(dir.path().join("a.st"), CLEAN_PROGRAM).unwrap();
    let container = tempfile::NamedTempFile::new().unwrap();
    let name = dir
        .path()
        .file_name()
        .unwrap()
        .to_string_lossy()
        .into_owned();

    let (project, diagnostics) = super::run_compile(
        &[dir.path().to_path_buf()],
        container.path(),
        CompilerOptions::default(),
        &[],
    )
    .unwrap();
    let shown = person_form(&diagnostics, Some(&project));

    assert_eq!(
        headed_lines(&shown).first().copied(),
        Some(format!("------ Build started: Application: {name} -------").as_str())
    );
}

#[spec_test(REQ_DS_cli_008)]
fn compile_when_an_error_then_no_download_possible_and_no_container_and_failure() {
    let (dir, path) = source_named("main.st", UNDECLARED_VARIABLE);
    let container = dir.path().join("out.iplc");

    let (project, diagnostics) = super::run_compile(
        std::slice::from_ref(&path),
        &container,
        CompilerOptions::default(),
        &[],
    )
    .unwrap();
    let shown = person_form(&diagnostics, Some(&project));
    let result = compile(
        std::slice::from_ref(&path),
        &container,
        CompilerOptions::default(),
        &[],
        true,
    );

    assert_eq!(
        headed_lines(&shown).last().copied(),
        Some("Build complete -- 1 errors, 0 warnings : No download possible")
    );
    assert!(!container.exists());
    assert_eq!(result, Err("Compile failed with 1 problem(s)".to_string()));
}

#[spec_test(REQ_DS_cli_009)]
fn check_when_json_then_the_closing_line_is_the_last_message_a_text_without_a_code() {
    for (text, count) in [(CLEAN_PROGRAM, 1), (UNDECLARED_VARIABLE, 2)] {
        let (_dir, path) = source_named("main.st", text);
        let (project, diagnostics) = super::run_check(&[path], CompilerOptions::default(), &[]);

        let json = super::json_of(&diagnostics, Some(&project)).unwrap();

        let messages: serde_json::Value = serde_json::from_str(&json).unwrap();
        let messages = messages.as_array().unwrap();
        assert_eq!(messages.len(), count);
        let last = &messages[messages.len() - 1];
        assert_eq!(last["severity"], "Text");
        assert_eq!(last["severity_value"], 16);
        assert!(last["prefix"].is_null() && last["number"].is_null());
        assert!(last["text"]
            .as_str()
            .unwrap()
            .starts_with("Compile complete -- "));
    }
}

/// A program with `count` statements that each read an undeclared variable.
fn program_with_errors(count: usize) -> String {
    let mut source = String::from("PROGRAM main\nVAR y : INT; END_VAR\n");
    for at in 0..count {
        source.push_str(&format!("y := undeclared_{at};\n"));
    }
    source.push_str("END_PROGRAM\n");
    source
}

#[spec_test(REQ_DS_cli_010)]
fn check_when_501_errors_then_500_reports_the_limit_line_on_one_line_and_the_counts() {
    let (_dir, path) = source_named("main.st", &program_with_errors(501));
    let paths = vec![path];

    let (project, diagnostics) = super::run_check(&paths, CompilerOptions::default(), &[]);
    let shown = person_form(&diagnostics, Some(&project));
    let result = check(&paths, CompilerOptions::default(), &[], Output::Silent);

    let headed = headed_lines(&shown);
    assert_eq!(
        headed
            .iter()
            .filter(|line| line.starts_with("Error[P4007]"))
            .count(),
        500
    );
    let limit_line = "Error: More than 500 errors occured: Skipping all further error messages";
    assert_eq!(headed.iter().filter(|line| **line == limit_line).count(), 1);
    assert_eq!(
        headed.last().copied(),
        Some("Compile complete -- 501 errors, 0 warnings")
    );
    // The line of the limit has no snippet and no link under it.
    let at = shown.find(limit_line).unwrap();
    assert!(shown[at + limit_line.len()..].starts_with("\nCompile complete"));
    assert_eq!(result, Err("Check failed with 501 problem(s)".to_string()));
}

#[spec_test(REQ_DS_cli_010)]
fn write_for_person_when_warning_without_a_code_then_the_word_and_the_text_on_one_line() {
    let diagnostics = vec![Diagnostic::codeless(
        ironplc_problems::codeless::warning_limit_exceeded(500),
    )];

    let shown = person_form(&diagnostics, None);

    assert_eq!(
        shown,
        "Warning: More than 500 warnings occured: Skipping all further warning messages\n"
    );
}

/// A program with two configurations, `Zeta` written before `Alpha`.
const TWO_CONFIGURATIONS: &str = "PROGRAM main
VAR y : INT; END_VAR
  y := 1;
END_PROGRAM

CONFIGURATION Zeta
  RESOURCE r ON PLC
    TASK t(INTERVAL := T#10ms, PRIORITY := 1);
    PROGRAM i1 WITH t : main;
  END_RESOURCE
END_CONFIGURATION

CONFIGURATION Alpha
  RESOURCE r ON PLC
    TASK t(INTERVAL := T#10ms, PRIORITY := 1);
    PROGRAM i1 WITH t : main;
  END_RESOURCE
END_CONFIGURATION
";

#[spec_test(REQ_DS_cli_014)]
fn compile_when_configuration_then_the_application_is_its_name_not_the_file() {
    let (dir, path) = source_named(
        "Plant.st",
        &TWO_CONFIGURATIONS[..TWO_CONFIGURATIONS.find("CONFIGURATION Alpha").unwrap()],
    );
    let container = dir.path().join("out.iplc");

    let (project, diagnostics) = super::run_compile(
        std::slice::from_ref(&path),
        &container,
        CompilerOptions::default(),
        &[],
    )
    .unwrap();
    let shown = person_form(&diagnostics, Some(&project));

    assert_eq!(
        headed_lines(&shown).first().copied(),
        Some("------ Build started: Application: Zeta -------")
    );
}

#[spec_test(REQ_DS_cli_014)]
fn compile_when_several_configurations_then_the_application_is_the_first_written() {
    let (dir, path) = source_named("Plant.st", TWO_CONFIGURATIONS);
    let container = dir.path().join("out.iplc");

    let (project, diagnostics) = super::run_compile(
        std::slice::from_ref(&path),
        &container,
        CompilerOptions::default(),
        &[],
    )
    .unwrap();
    let shown = person_form(&diagnostics, Some(&project));

    assert_eq!(
        headed_lines(&shown).first().copied(),
        Some("------ Build started: Application: Zeta -------")
    );
}

#[spec_test(REQ_DS_cli_014)]
fn compile_when_syntax_error_then_the_application_is_the_name_of_the_file() {
    // No library is produced, so there is no configuration to name.
    let (dir, path) = source_named("Plant.st", "PROGRAM END_PROGRAM");
    let container = dir.path().join("out.iplc");

    let (project, diagnostics) = super::run_compile(
        std::slice::from_ref(&path),
        &container,
        CompilerOptions::default(),
        &[],
    )
    .unwrap();
    let shown = person_form(&diagnostics, Some(&project));

    assert_eq!(
        headed_lines(&shown).first().copied(),
        Some("------ Build started: Application: Plant -------")
    );
}
