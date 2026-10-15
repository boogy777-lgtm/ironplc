//! Tests of `ironplcvm benchmark`: JSON statistics, per-task budgets and error exits.

use std::path::Path;
use std::process::Command;

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use ironplc_container::test_support::single_function_container;
use predicates::prelude::*;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use crate::{write_container, write_cyclic_task_container, write_steel_thread_container};

/// REQ-VC-vm-cli-013: `benchmark` prints a JSON object with `scan_us` stats and tasks.
#[spec_test(REQ_VC_vm_cli_013)]
fn benchmark_when_valid_container_then_outputs_json_with_scan_us(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("test.iplc");
    write_steel_thread_container(&container_path);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("benchmark")
        .arg(&container_path)
        .arg("--cycles")
        .arg("100")
        .arg("--warmup")
        .arg("10");
    cmd.assert()
        .success()
        .stdout(predicate::str::contains("scan_us"))
        .stdout(predicate::str::contains("mean"))
        .stdout(predicate::str::contains("stddev"))
        .stdout(predicate::str::contains("p99"))
        .stdout(predicate::str::contains("tasks"));

    Ok(())
}

/// REQ-VC-vm-cli-016: `benchmark` surfaces file-open errors as V6001 exit 2.
#[spec_test(REQ_VC_vm_cli_016)]
fn benchmark_when_file_not_found_then_exit_2_and_v6001() -> Result<(), Box<dyn std::error::Error>> {
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("benchmark").arg("nonexistent.iplc");
    cmd.assert()
        .code(2)
        .stderr(predicate::str::contains("V6001"));

    Ok(())
}

/// REQ-VC-vm-cli-016: `benchmark` surfaces malformed-container errors as V6002/exit 2.
#[spec_test(REQ_VC_vm_cli_016)]
fn benchmark_when_invalid_file_then_exit_2_and_v6002() -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let bad_path = dir.path().join("bad.iplc");
    std::fs::write(&bad_path, "this is not a container file")?;

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("benchmark").arg(&bad_path);
    cmd.assert()
        .code(2)
        .stderr(predicate::str::contains("V6002"));

    Ok(())
}

/// Builds a container whose init is a no-op but whose scan function divides
/// by zero. The fault therefore occurs inside `run_round`, not `start()`,
/// which is the path used by `benchmark`'s warmup and measured loops.
fn write_scan_divide_by_zero_container(path: &Path) {
    #[rustfmt::skip]
    let scan_bytecode: Vec<u8> = vec![
        0x00, 0x00, 0x00,       // LOAD_CONST_I32 pool[0]  (10)
        0x00, 0x01, 0x00,       // LOAD_CONST_I32 pool[1]  (0)
        0x30,                   // DIV_I32                  (10 / 0 → trap)
        0x8C,                   // RET_VOID
    ];

    write_container(
        &single_function_container(&scan_bytecode, 0, &[10, 0]),
        path,
    );
}

/// REQ-VC-vm-cli-017: a trap during the benchmark warmup phase exits 1 with the trap's V-code.
#[spec_test(REQ_VC_vm_cli_017)]
fn benchmark_when_fault_during_warmup_then_exit_1() -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("scan_div_zero.iplc");
    write_scan_divide_by_zero_container(&container_path);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("benchmark")
        .arg(&container_path)
        .arg("--warmup")
        .arg("5")
        .arg("--cycles")
        .arg("10");
    cmd.assert()
        .code(1)
        .stderr(predicate::str::contains("V4001"));

    Ok(())
}

/// REQ-VC-vm-cli-017: a trap during the measured phase (warmup=0) exits 1 with the trap's V-code.
#[spec_test(REQ_VC_vm_cli_017)]
fn benchmark_when_fault_during_measured_then_exit_1() -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("scan_div_zero.iplc");
    write_scan_divide_by_zero_container(&container_path);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("benchmark")
        .arg(&container_path)
        .arg("--warmup")
        .arg("0")
        .arg("--cycles")
        .arg("5");
    cmd.assert()
        .code(1)
        .stderr(predicate::str::contains("V4001"));

    Ok(())
}

/// REQ-VC-vm-cli-014: with `--cycles 0 --warmup 0`, `benchmark` still emits valid
/// JSON — `scan_us` stats are zero and no samples were measured.
#[spec_test(REQ_VC_vm_cli_014)]
fn benchmark_when_zero_cycles_then_outputs_zero_stats() -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("test.iplc");
    write_steel_thread_container(&container_path);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("benchmark")
        .arg(&container_path)
        .arg("--warmup")
        .arg("0")
        .arg("--cycles")
        .arg("0");
    let output = cmd.assert().success().get_output().stdout.clone();
    let json: serde_json::Value = serde_json::from_slice(&output)?;
    assert_eq!(json["cycles"], 0);
    assert_eq!(json["warmup"], 0);
    // With zero samples, max and p99 come from `unwrap_or(0.0)` / the empty
    // percentile guard. mean and stddev are NaN (serialised as null).
    assert_eq!(json["scan_us"]["p99"], 0.0);
    assert_eq!(json["scan_us"]["max"], 0.0);

    Ok(())
}

/// REQ-VC-vm-cli-015: `benchmark` emits per-cyclic-task `budget_pct` when the task's
/// interval is non-zero.
#[spec_test(REQ_VC_vm_cli_015)]
fn benchmark_when_cyclic_task_then_budget_pct_in_output() -> Result<(), Box<dyn std::error::Error>>
{
    let dir = TempDir::new()?;
    let container_path = dir.path().join("cyclic.iplc");
    // 10 ms interval — non-zero so the budget_pct branch fires.
    write_cyclic_task_container(&container_path, 10_000);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("benchmark")
        .arg(&container_path)
        .arg("--warmup")
        .arg("1")
        .arg("--cycles")
        .arg("5");
    let output = cmd.assert().success().get_output().stdout.clone();
    let json: serde_json::Value = serde_json::from_slice(&output)?;
    let tasks = json["tasks"].as_array().unwrap();
    assert!(!tasks.is_empty(), "expected at least one task entry");
    let task = &tasks[0];
    assert_eq!(task["task_type"], "Cyclic");
    let budget = &task["budget_pct"];
    assert!(budget.is_object(), "expected budget_pct object: {task}");
    assert!(budget["mean"].is_number());
    assert!(budget["p99"].is_number());
    assert!(budget["max"].is_number());

    Ok(())
}
