//! Tests of the retained file of `ironplcvm serve` as a process: the write at
//! the end of the input, the report of the next start, and the exit code of a
//! write that fails (REQ-VC-vm-cli-037 through -040).

use std::path::Path;

use assert_cmd::cargo;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use crate::{compiled_bytes, write_compiled_container};

/// A program with one retained variable.
const RETAINED_PROGRAM: &str = "
PROGRAM main
  VAR RETAIN
    Kept : DINT;
  END_VAR
  Kept := Kept + 1;
END_PROGRAM
";

/// The same declarations, other code: another application.
const OTHER_PROGRAM: &str = "
PROGRAM main
  VAR RETAIN
    Kept : DINT;
  END_VAR
  Kept := Kept + 2;
END_PROGRAM
";

const IDENTITY: &str = "{\"command\":\"identity\"}\n";

/// One `ironplcvm serve` process fed `stdin`: its output and the response
/// lines it wrote.
fn serve_lines(path: &Path, stdin: &str) -> (std::process::Output, Vec<serde_json::Value>) {
    let output = assert_cmd::Command::new(cargo::cargo_bin!("ironplcvm"))
        .arg("serve")
        .arg(path)
        .write_stdin(stdin)
        .output()
        .unwrap();
    let lines = String::from_utf8(output.stdout.clone())
        .unwrap()
        .lines()
        .map(|line| serde_json::from_str(line).unwrap())
        .collect();
    (output, lines)
}

fn served(dir: &TempDir, source: &str) -> std::path::PathBuf {
    let path = dir.path().join("app.iplc");
    write_compiled_container(&path, source);
    path
}

fn retained_file(dir: &TempDir) -> std::path::PathBuf {
    dir.path().join("app.iplc.retained")
}

/// A session that runs the application for two driven rounds without leaving
/// a candidate under test: accept the same program, test it, untest it.
fn run_two_rounds() -> String {
    let accept = serde_json::json!({
        "command": "acceptEdits",
        "program": compiled_bytes(RETAINED_PROGRAM),
    })
    .to_string();
    format!("{accept}\n{{\"command\":\"testEdits\"}}\n{{\"command\":\"untestEdits\"}}\n")
}

/// REQ-VC-vm-cli-037, REQ-VC-vm-cli-038: the end of the input writes the
/// retained file, and the next start applies it and says so.
#[spec_test(REQ_VC_vm_cli_037)]
#[spec_test(REQ_VC_vm_cli_038)]
#[test]
fn serve_when_input_ends_then_the_retained_file_is_written_and_the_next_start_applies_it() {
    let dir = TempDir::new().unwrap();
    let path = served(&dir, RETAINED_PROGRAM);
    let (first_start, _) = serve_lines(&path, &run_two_rounds());
    assert!(first_start.status.success());
    assert!(retained_file(&dir).exists());

    let (_, second) = serve_lines(&path, IDENTITY);

    assert_eq!(
        second[0]["retained"],
        serde_json::json!({"result": "applied"})
    );
}

/// REQ-VC-vm-cli-037: a device with no file reports it.
#[spec_test(REQ_VC_vm_cli_037)]
#[test]
fn serve_when_there_is_no_retained_file_then_identity_reports_absent() {
    let dir = TempDir::new().unwrap();

    let (_, lines) = serve_lines(&served(&dir, RETAINED_PROGRAM), IDENTITY);

    assert_eq!(
        lines[0]["retained"],
        serde_json::json!({"result": "absent"})
    );
}

/// REQ-VC-vm-cli-037, REQ-VC-vm-cli-040: a file written for another program
/// is discarded and the identity says why.
#[spec_test(REQ_VC_vm_cli_037)]
#[spec_test(REQ_VC_vm_cli_040)]
#[test]
fn serve_when_the_retained_file_is_of_another_program_then_identity_reports_it_discarded() {
    let dir = TempDir::new().unwrap();
    let other = dir.path().join("other.iplc");
    write_compiled_container(&other, OTHER_PROGRAM);
    serve_lines(&other, &run_two_rounds());
    std::fs::rename(dir.path().join("other.iplc.retained"), retained_file(&dir)).unwrap();

    let (_, lines) = serve_lines(&served(&dir, RETAINED_PROGRAM), IDENTITY);

    assert_eq!(
        lines[0]["retained"],
        serde_json::json!({"result": "discarded", "reason": "otherApplication"})
    );
}

/// REQ-VC-vm-cli-039: a write that fails at the end of the input exits with 2
/// and V6018, and the answers of the session were not affected.
#[spec_test(REQ_VC_vm_cli_039)]
#[test]
fn serve_when_the_retained_write_fails_at_the_end_then_exit_2_and_v6018() {
    let dir = TempDir::new().unwrap();
    let path = served(&dir, RETAINED_PROGRAM);
    std::fs::create_dir(dir.path().join("app.iplc.retained.tmp")).unwrap();

    let (output, lines) = serve_lines(&path, IDENTITY);

    assert_eq!(output.status.code(), Some(2));
    assert!(String::from_utf8_lossy(&output.stderr).contains("V6018"));
    assert_eq!(lines[0]["response"], "identity");
}
