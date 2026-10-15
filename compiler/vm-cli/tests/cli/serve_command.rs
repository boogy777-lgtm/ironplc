//! Tests of `ironplcvm serve`: argument handling, container loading and the status reply.

use std::process::Command;

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use predicates::prelude::*;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use crate::write_compiled_container;

#[test]
fn serve_help_then_ok() -> Result<(), Box<dyn std::error::Error>> {
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("serve").arg("--help");
    cmd.assert()
        .success()
        .stdout(predicate::str::contains("Usage"))
        .stdout(predicate::str::contains(".iplc"));

    Ok(())
}

/// REQ-VC-vm-cli-018: `serve` loads the container exactly like `run` — a
/// missing file exits 2 with V6001.
#[spec_test(REQ_VC_vm_cli_018)]
#[test]
fn serve_when_file_not_found_then_exit_2_and_v6001() -> Result<(), Box<dyn std::error::Error>> {
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("serve").arg("test/file/doesnt/exist.iplc");
    cmd.assert()
        .code(2)
        .stderr(predicate::str::contains("V6001"));

    Ok(())
}

/// REQ-VC-vm-cli-018: bytes that are not a container exit 2 with V6002.
#[spec_test(REQ_VC_vm_cli_018)]
#[test]
fn serve_when_invalid_file_then_exit_2_and_v6002() -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let bad_path = dir.path().join("bad.iplc");
    std::fs::write(&bad_path, "this is not a container file")?;

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("serve").arg(&bad_path);
    cmd.assert()
        .code(2)
        .stderr(predicate::str::contains("V6002"));

    Ok(())
}

/// REQ-VC-vm-cli-019: one command line on stdin yields exactly one flushed
/// response line on stdout, and EOF ends the session with exit 0.
#[spec_test(REQ_VC_vm_cli_019)]
#[test]
fn serve_when_get_status_then_one_status_line_and_exit_0() -> Result<(), Box<dyn std::error::Error>>
{
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(
        &container_path,
        "
PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
    );

    let mut cmd = assert_cmd::Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("serve").arg(&container_path);
    cmd.write_stdin("{\"command\":\"getStatus\"}\n");
    let output = cmd.assert().success().get_output().stdout.clone();

    let text = String::from_utf8(output)?;
    let lines: Vec<&str> = text.lines().collect();
    assert_eq!(lines.len(), 1);
    let response: serde_json::Value = serde_json::from_str(lines[0])?;
    assert_eq!(response["response"], "status");
    assert_eq!(response["mode"], "normal");
    assert_eq!(response["candidate"], serde_json::Value::Null);

    Ok(())
}
