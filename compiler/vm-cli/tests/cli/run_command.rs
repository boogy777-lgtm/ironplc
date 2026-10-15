//! Tests of `ironplcvm run`: loading, exit codes, traps, scan pacing and shutdown.

use std::path::Path;
use std::process::Command;

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use ironplc_container::test_support::{container_bytes, steel_thread_debug_builder};
use ironplc_container::{Container, ContainerBuilder, FunctionId};
use predicates::prelude::*;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use crate::{
    write_compiled_container, write_container, write_cyclic_task_container,
    write_steel_thread_container,
};

/// REQ-VC-vm-cli-003: `run --scans N` runs exactly N rounds then exits 0.
#[spec_test(REQ_VC_vm_cli_003)]
fn run_when_valid_container_file_then_ok() -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("test.iplc");
    write_steel_thread_container(&container_path);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run").arg(&container_path).arg("--scans").arg("1");
    cmd.assert().success().stdout(predicate::str::is_empty());

    Ok(())
}

/// REQ-VC-vm-cli-001: a missing container file yields V6001 exit 2.
#[spec_test(REQ_VC_vm_cli_001)]
fn run_when_file_not_found_then_exit_2_and_v6001() -> Result<(), Box<dyn std::error::Error>> {
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run").arg("test/file/doesnt/exist.iplc");
    cmd.assert()
        .code(2)
        .stderr(predicate::str::contains("V6001"));

    Ok(())
}

/// REQ-VC-vm-cli-002: a malformed container yields V6002 exit 2.
#[spec_test(REQ_VC_vm_cli_002)]
fn run_when_invalid_file_then_exit_2_and_v6002() -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let bad_path = dir.path().join("bad.iplc");
    std::fs::write(&bad_path, "this is not a container file")?;

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run").arg(&bad_path);
    cmd.assert()
        .code(2)
        .stderr(predicate::str::contains("V6002"));

    Ok(())
}

/// A container modified after compilation fails its content hash check and
/// is reported under the container-read code, before anything executes.
#[test]
fn run_when_code_byte_modified_then_exit_2_and_v6002() -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("modified.iplc");
    let mut bytes = container_bytes(&steel_thread_debug_builder().build());
    let container = Container::read_from(&mut std::io::Cursor::new(&bytes))?;
    let last_code_byte =
        (container.header.code_section_offset + container.header.code_section_size - 1) as usize;
    bytes[last_code_byte] ^= 0xFF;
    std::fs::write(&container_path, &bytes)?;

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run").arg(&container_path).arg("--scans").arg("1");
    cmd.assert()
        .failure()
        .code(2)
        .stderr(predicate::str::contains("V6002"))
        .stderr(predicate::str::contains("content hash mismatch"));

    Ok(())
}

/// Builds a container whose program divides by zero: 10 / 0.
fn write_divide_by_zero_container(path: &Path) {
    #[rustfmt::skip]
    let bytecode: Vec<u8> = vec![
        0x00, 0x00, 0x00,       // LOAD_CONST_I32 pool[0]  (10)
        0x00, 0x01, 0x00,       // LOAD_CONST_I32 pool[1]  (0)
        0x30,                   // DIV_I32                  (10 / 0 → trap)
        0x8C,                   // RET_VOID
    ];

    // Deliberately a single function: init *is* the entry point, so the trap
    // happens inside `start()`. `write_scan_divide_by_zero_container` below is
    // the init/scan-split counterpart that traps inside `run_round`.
    let container = ContainerBuilder::new()
        .num_variables(0)
        .add_i32_constant(10)
        .add_i32_constant(0)
        .add_function(FunctionId::new(0), &bytecode, 2, 0, 0)
        .max_call_depth(1)
        .build();

    write_container(&container, path);
}

/// REQ-VC-vm-cli-004: a runtime trap exits 1 with the trap's V-code on stderr.
#[spec_test(REQ_VC_vm_cli_004)]
fn run_when_divide_by_zero_then_exit_1_and_v4001() -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("div_zero.iplc");
    write_divide_by_zero_container(&container_path);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run").arg(&container_path).arg("--scans").arg("1");
    cmd.assert()
        .code(1)
        .stderr(predicate::str::contains("V4001"))
        .stderr(predicate::str::contains("divide by zero"));

    Ok(())
}

/// REQ-VC-vm-cli-012: `run` sleeps between rounds for a cyclic task — two rounds with
/// a 20 ms interval must take at least one interval of wall-clock time.
#[spec_test(REQ_VC_vm_cli_012)]
fn run_when_cyclic_task_then_sleeps_between_rounds() -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("cyclic.iplc");
    let interval_us: u64 = 20_000; // 20 ms
    write_cyclic_task_container(&container_path, interval_us);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run").arg(&container_path).arg("--scans").arg("2");
    let start = std::time::Instant::now();
    cmd.assert().success();
    let elapsed = start.elapsed();

    // Spawning a cargo binary has non-trivial overhead, so the exact wall-clock
    // depends on the host. We just assert the run took at least a single
    // interval — proof that `next_due_us`-driven sleep was exercised at least
    // once. A busy-loop would finish in microseconds.
    let interval = std::time::Duration::from_micros(interval_us);
    assert!(
        elapsed >= interval,
        "expected at least one cyclic interval ({interval:?}) of wall-clock, got {elapsed:?}"
    );

    Ok(())
}

/// REQ-VC-vm-cli-011: without `--scans`, `run` loops until SIGINT then exits 0.
/// Unix-only: we send SIGINT via `kill(2)` after giving the child time to
/// install the ctrlc handler and enter the main loop.
#[cfg(unix)]
#[spec_test(REQ_VC_vm_cli_011)]
fn run_without_scans_then_stops_on_sigint() -> Result<(), Box<dyn std::error::Error>> {
    use std::process::{Command as ProcessCommand, Stdio};
    use std::time::Duration;

    let dir = TempDir::new()?;
    let container_path = dir.path().join("test.iplc");
    // Use a cyclic container so the loop sleeps between rounds — that gives the
    // kill(2) call a deterministic window to be observed.
    write_cyclic_task_container(&container_path, 10_000);

    let mut child = ProcessCommand::new(cargo::cargo_bin!("ironplcvm"))
        .arg("run")
        .arg(&container_path)
        .stdout(Stdio::null())
        .stderr(Stdio::null())
        .spawn()?;

    // Give the child time to install its ctrlc handler and enter the scan loop.
    std::thread::sleep(Duration::from_millis(300));

    let pid = child.id();
    let kill_status = ProcessCommand::new("kill")
        .arg("-INT")
        .arg(pid.to_string())
        .status()?;
    assert!(kill_status.success(), "failed to signal child");

    let status = child.wait()?;
    assert!(
        status.success(),
        "expected clean exit 0 after SIGINT, got {status:?}"
    );

    Ok(())
}

/// A `STRING_TO_UDINT` compiled under the strict default (reject, trap)
/// halts the program with V4006 at the first non-convertible input, and the
/// message names the offending string (ADR-0049).
#[test]
fn run_when_string_not_convertible_then_exit_1_and_v4006() -> Result<(), Box<dyn std::error::Error>>
{
    let dir = TempDir::new()?;
    let container_path = dir.path().join("s2u.iplc");
    write_compiled_container(
        &container_path,
        "
PROGRAM main
  VAR
    s : STRING := '12abc';
    x : UDINT;
  END_VAR
  x := STRING_TO_UDINT(s);
END_PROGRAM
",
    );

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run").arg(&container_path).arg("--scans").arg("1");
    cmd.assert()
        .code(1)
        .stderr(predicate::str::contains("V4006"))
        .stderr(predicate::str::contains(
            "string '12abc' is not convertible to UDINT",
        ));

    Ok(())
}
