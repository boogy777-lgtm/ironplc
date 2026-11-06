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

/// REQ-VC-vm-cli-042: `serve` runs the scan by itself, not only when a command
/// asks for a round: the rounds counter that `getStatus` reports advances while
/// no command is sent. The test asserts the state (it advanced), not a count,
/// because the number of rounds between two commands depends on timing.
#[spec_test(REQ_VC_vm_cli_042)]
#[test]
fn serve_when_idle_between_commands_then_the_scan_keeps_running(
) -> Result<(), Box<dyn std::error::Error>> {
    use std::io::{BufRead, BufReader, Write};
    use std::process::Stdio;
    use std::time::Duration;

    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, crate::COUNTER_PROGRAM);
    let mut child = std::process::Command::new(cargo::cargo_bin!("ironplcvm"))
        .arg("serve")
        .arg(&container_path)
        .stdin(Stdio::piped())
        .stdout(Stdio::piped())
        .spawn()?;
    let mut stdin = child.stdin.take().unwrap();
    let mut stdout = BufReader::new(child.stdout.take().unwrap());
    let mut rounds = || -> Result<u64, Box<dyn std::error::Error>> {
        stdin.write_all(b"{\"command\":\"getStatus\"}\n")?;
        stdin.flush()?;
        let mut line = String::new();
        stdout.read_line(&mut line)?;
        let status: serde_json::Value = serde_json::from_str(&line)?;
        Ok(status["rounds"].as_u64().unwrap())
    };

    let first = rounds()?;
    std::thread::sleep(Duration::from_millis(300));
    let second = rounds()?;

    assert!(second > first, "{first} then {second}");
    drop(rounds);
    drop(stdin);
    assert!(child.wait()?.success());
    Ok(())
}
