//! The two forms of a message at the command line: the words of the CODESYS
//! Messages view for a person, and the fields of the CODESYS `ScriptMessage`
//! for a program (`check --format json`).

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use ironplc_test::shared_resource_path;
use std::process::Command;
use tempfile::NamedTempFile;

fn st_source(text: &str) -> Result<NamedTempFile, Box<dyn std::error::Error>> {
    use std::io::Write;

    let mut source = tempfile::Builder::new().suffix(".st").tempfile()?;
    source.write_all(text.as_bytes())?;
    Ok(source)
}

/// An undeclared variable: P4007, whose CODESYS counterpart is C0046.
const UNDECLARED_VARIABLE: &str = "PROGRAM main
VAR y : INT; END_VAR
  y := x + 1;
END_PROGRAM
";

/// The text without the escapes of the colors the terminal output is drawn in.
fn without_colors(text: &str) -> String {
    let mut plain = String::new();
    let mut chars = text.chars();
    while let Some(c) = chars.next() {
        if c == '\u{1b}' {
            for end in chars.by_ref() {
                if end == 'm' {
                    break;
                }
            }
        } else {
            plain.push(c);
        }
    }
    plain
}

/// The terminal form uses the word of the CODESYS Messages view, then the
/// code, then the CODESYS code of the same condition.
#[test]
fn check_when_undeclared_variable_then_header_has_word_code_and_codesys_code(
) -> Result<(), Box<dyn std::error::Error>> {
    let source = st_source(UNDECLARED_VARIABLE)?;
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));

    cmd.arg("check").arg(source.path());
    let output = cmd.assert().failure().get_output().clone();

    let stderr = String::from_utf8_lossy(&output.stderr);
    let first = stderr.lines().next().unwrap_or_default();
    // A pipe gets plain text, a terminal gets colors; the escapes are not asserted.
    assert!(
        first.contains("Error[P4007] (CODESYS C0046)") && first.contains("Variable not defined"),
        "first line: {first}"
    );
    // codespan's own header is not written beside ours.
    assert!(!stderr.contains("error[P4007]"), "{stderr}");
    assert!(stderr.contains(":3:8"), "{stderr}");

    Ok(())
}

/// The program form is a JSON array of the fields of the CODESYS
/// `ScriptMessage`, on standard output.
#[test]
fn check_when_format_json_then_stdout_is_an_array_of_script_messages(
) -> Result<(), Box<dyn std::error::Error>> {
    let source = st_source(UNDECLARED_VARIABLE)?;
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));

    cmd.arg("check")
        .arg("--format")
        .arg("json")
        .arg(source.path());
    let output = cmd.assert().failure().get_output().clone();

    let messages: serde_json::Value = serde_json::from_slice(&output.stdout)?;
    let first = &messages[0];
    // The error, and the line that closes the check.
    assert_eq!(messages.as_array().map(Vec::len), Some(2));
    assert_eq!(messages[1]["severity"], "Text");
    assert_eq!(first["severity"], "Error");
    assert_eq!(first["severity_value"], 2);
    assert_eq!(first["prefix"], "P");
    assert_eq!(first["number"], 4007);
    assert_eq!(first["codesys"], "C0046");
    assert!(first["text"].as_str().unwrap_or_default().contains("x"));
    assert!(first["position_text"]
        .as_str()
        .unwrap_or_default()
        .ends_with(":3:8"));
    assert!(first["object"].as_str().is_some());
    assert!(first["position"].is_u64() && first["length"].is_u64());

    Ok(())
}

#[test]
fn check_when_format_json_and_no_problem_then_stdout_is_the_closing_line_alone(
) -> Result<(), Box<dyn std::error::Error>> {
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));

    cmd.arg("check")
        .arg("--format")
        .arg("json")
        .arg(shared_resource_path("first_steps.st"));

    let output = cmd.assert().success().get_output().clone();

    let messages: serde_json::Value = serde_json::from_slice(&output.stdout)?;
    assert_eq!(messages.as_array().map(Vec::len), Some(1));
    assert_eq!(messages[0]["severity"], "Text");
    assert_eq!(
        messages[0]["text"],
        "Compile complete -- 0 errors, 0 warnings"
    );
    assert!(messages[0]["prefix"].is_null() && messages[0]["number"].is_null());

    Ok(())
}

/// A comparison of values of two different enumerations: the warning P4083,
/// and no error.
const TWO_ENUMERATIONS_COMPARED: &str = "TYPE
  TrafficLight : (Red, Yellow, Green);
  Mode : (Auto, Manual);
END_TYPE

PROGRAM main
VAR
  light : TrafficLight := Red;
  current : Mode := Auto;
  same : BOOL;
  result : DINT;
END_VAR
  same := light = current;
  IF same THEN result := 7; END_IF;
END_PROGRAM
";

/// A warning is written with its word, its code and the CODESYS code of the
/// same condition, and does not make the command fail.
#[test]
fn check_when_only_a_warning_then_header_names_it_and_the_exit_status_is_zero(
) -> Result<(), Box<dyn std::error::Error>> {
    let source = st_source(TWO_ENUMERATIONS_COMPARED)?;
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));

    cmd.arg("check").arg(source.path());
    let output = cmd.assert().success().get_output().clone();

    let stderr = String::from_utf8_lossy(&output.stderr);
    assert!(
        stderr.contains("Warning[P4083] (CODESYS C0354)")
            && stderr
                .contains("Comparison of one enumeration type (TrafficLight) with another (Mode)"),
        "{stderr}"
    );
    assert!(!stderr.contains("Error["), "{stderr}");

    Ok(())
}

/// The program form gives the category `Warning` (4) of the CODESYS scripting
/// model, and the command still succeeds.
#[test]
fn check_when_only_a_warning_and_format_json_then_the_message_is_a_warning(
) -> Result<(), Box<dyn std::error::Error>> {
    let source = st_source(TWO_ENUMERATIONS_COMPARED)?;
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));

    cmd.arg("check")
        .arg("--format")
        .arg("json")
        .arg(source.path());
    let output = cmd.assert().success().get_output().clone();

    let messages: serde_json::Value = serde_json::from_slice(&output.stdout)?;
    // The warning, and the line that closes the check.
    assert_eq!(messages.as_array().map(Vec::len), Some(2));
    assert_eq!(
        messages[1]["text"],
        "Compile complete -- 0 errors, 1 warnings"
    );
    assert_eq!(messages[0]["severity"], "Warning");
    assert_eq!(messages[0]["severity_value"], 4);
    assert_eq!(messages[0]["number"], 4083);
    assert_eq!(messages[0]["codesys"], "C0354");

    Ok(())
}

/// `compile` prints the warning, exits with status 0 and writes the container.
#[test]
fn compile_when_only_a_warning_then_the_container_is_written_and_the_exit_status_is_zero(
) -> Result<(), Box<dyn std::error::Error>> {
    let source = st_source(TWO_ENUMERATIONS_COMPARED)?;
    let output_file = tempfile::Builder::new().suffix(".iplc").tempfile()?;
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));

    cmd.arg("compile")
        .arg(source.path())
        .arg("--output")
        .arg(output_file.path());
    let output = cmd.assert().success().get_output().clone();

    let stderr = String::from_utf8_lossy(&output.stderr);
    assert!(
        stderr.contains("Warning[P4083] (CODESYS C0354)"),
        "{stderr}"
    );
    assert!(std::fs::metadata(output_file.path())?.len() > 0);

    Ok(())
}

/// `compile` writes the lines of a build around the problems, and the container
/// is ready for download when only a warning is found.
#[test]
fn compile_when_only_a_warning_then_the_build_lines_surround_it(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = tempfile::tempdir()?;
    let source = dir.path().join("Plant.st");
    std::fs::write(&source, TWO_ENUMERATIONS_COMPARED)?;
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));

    cmd.arg("compile")
        .arg(&source)
        .arg("--output")
        .arg(dir.path().join("out.iplc"));
    let output = cmd.assert().success().get_output().clone();

    let stderr = String::from_utf8_lossy(&output.stderr);
    let lines: Vec<&str> = stderr.lines().collect();
    assert_eq!(
        lines.first().copied(),
        Some("------ Build started: Application: Plant -------")
    );
    assert_eq!(
        lines.last().copied(),
        Some("Build complete -- 0 errors, 1 warnings : Ready for download")
    );

    Ok(())
}

/// An error stops the build: no container, and the last line says so.
#[test]
fn compile_when_an_error_then_no_download_possible_and_no_container(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = tempfile::tempdir()?;
    let source = dir.path().join("Plant.st");
    std::fs::write(&source, UNDECLARED_VARIABLE)?;
    let container = dir.path().join("out.iplc");
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));

    cmd.arg("compile")
        .arg(&source)
        .arg("--output")
        .arg(&container);
    let output = cmd.assert().failure().get_output().clone();

    let stderr = String::from_utf8_lossy(&output.stderr);
    assert!(
        stderr.contains("Build complete -- 1 errors, 0 warnings : No download possible"),
        "{stderr}"
    );
    assert!(!container.exists());

    Ok(())
}

/// More than 500 errors: the line of the limit, as one line, and the counts.
#[test]
fn check_when_more_than_500_errors_then_the_line_of_the_limit_and_the_counts(
) -> Result<(), Box<dyn std::error::Error>> {
    let mut text = String::from(
        "PROGRAM main
VAR y : INT; END_VAR
",
    );
    for at in 0..501 {
        text.push_str(&format!(
            "y := undeclared_{at};
"
        ));
    }
    text.push_str(
        "END_PROGRAM
",
    );
    let source = st_source(&text)?;
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));

    cmd.arg("check").arg(source.path());
    let output = cmd.assert().failure().get_output().clone();

    let stderr = without_colors(&String::from_utf8_lossy(&output.stderr));
    assert_eq!(stderr.matches("Error[P4007]").count(), 500);
    assert!(
        stderr.contains("Error: More than 500 errors occured: Skipping all further error messages"),
        "{stderr}"
    );
    assert!(
        stderr.contains("Compile complete -- 501 errors, 0 warnings"),
        "{stderr}"
    );

    Ok(())
}
