//! Tests of `ironplcvm run --dump-vars`: where the dump goes and how each type renders.

use std::path::Path;
use std::process::Command;

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use ironplc_container::debug_section::{iec_type_tag, VarNameEntry};
use ironplc_container::test_support::single_function_container;
use ironplc_container::{ContainerBuilder, FunctionId, VarIndex};
use predicates::prelude::*;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use crate::{write_compiled_container, write_container, write_steel_thread_container};

/// REQ-VC-vm-cli-005: `run --dump-vars <PATH>` writes variable values to a file.
#[spec_test(REQ_VC_vm_cli_005)]
fn run_when_valid_container_file_and_dump_vars_then_writes_variables(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("test.iplc");
    let dump_path = dir.path().join("vars.txt");
    write_steel_thread_container(&container_path);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&container_path)
        .arg("--dump-vars")
        .arg(&dump_path)
        .arg("--scans")
        .arg("1");
    cmd.assert().success();

    let contents = std::fs::read_to_string(&dump_path)?;
    assert_eq!(contents, "x: 10\ny: 42\n");

    Ok(())
}

/// Builds a container with debug info: two BOOL variables named Button and Buzzer.
/// Program logic: Buzzer := NOT Button (Button defaults to 0/FALSE, so Buzzer = TRUE).
fn write_doorbell_container(path: &Path) {
    #[rustfmt::skip]
    let bytecode: Vec<u8> = vec![
        0x0C, 0x00, 0x00,       // LOAD_VAR_I32   var[0]   (push Button)
        0x7B,                   // BOOL_NOT                 (NOT Button)
        0x10, 0x01, 0x00,       // STORE_VAR_I32  var[1]   (Buzzer := result)
        0x8C,                   // RET_VOID
    ];

    let container = ContainerBuilder::new()
        .num_variables(2)
        .add_function(FunctionId::new(0), &bytecode, 1, 2, 0)
        .add_var_name(VarNameEntry {
            var_index: VarIndex::new(0),
            function_id: FunctionId::GLOBAL_SCOPE,
            var_section: 0,
            iec_type_tag: iec_type_tag::BOOL,
            name: "Button".into(),
            type_name: "BOOL".into(),
        })
        .add_var_name(VarNameEntry {
            var_index: VarIndex::new(1),
            function_id: FunctionId::GLOBAL_SCOPE,
            var_section: 0,
            iec_type_tag: iec_type_tag::BOOL,
            name: "Buzzer".into(),
            type_name: "BOOL".into(),
        })
        .max_call_depth(1)
        .build();

    write_container(&container, path);
}

/// REQ-VC-vm-cli-008: with debug info, the dump uses named variables.
#[spec_test(REQ_VC_vm_cli_008)]
fn run_when_debug_info_and_dump_vars_then_shows_named_variables(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("doorbell.iplc");
    let dump_path = dir.path().join("vars.txt");
    write_doorbell_container(&container_path);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&container_path)
        .arg("--dump-vars")
        .arg(&dump_path)
        .arg("--scans")
        .arg("1");
    cmd.assert().success();

    let contents = std::fs::read_to_string(&dump_path)?;
    assert_eq!(contents, "Button: FALSE\nBuzzer: TRUE\n");

    Ok(())
}

/// REQ-VC-vm-cli-006: `--dump-vars` without a path writes the dump to stdout.
#[spec_test(REQ_VC_vm_cli_006)]
fn run_when_dump_vars_without_path_then_prints_to_stdout() -> Result<(), Box<dyn std::error::Error>>
{
    let dir = TempDir::new()?;
    let container_path = dir.path().join("test.iplc");
    write_steel_thread_container(&container_path);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&container_path)
        .arg("--scans")
        .arg("1")
        .arg("--dump-vars");
    cmd.assert()
        .success()
        .stdout(predicate::str::contains("x: 10"))
        .stdout(predicate::str::contains("y: 42"));

    Ok(())
}

/// Builds a container with a no-op init function and a scan function that
/// assigns `x := 10` then divides by zero. The init runs cleanly under
/// `Vm::start()`; the fault happens inside `run_round`, so the pre-fault
/// variable state is observable via `--dump-vars`.
fn write_fault_with_vars_container(path: &Path) {
    #[rustfmt::skip]
    let scan_bytecode: Vec<u8> = vec![
        0x00, 0x00, 0x00,       // LOAD_CONST_I32 pool[0]  (10)
        0x10, 0x00, 0x00,       // STORE_VAR_I32  var[0]   (x := 10)
        0x00, 0x00, 0x00,       // LOAD_CONST_I32 pool[0]  (10)
        0x00, 0x01, 0x00,       // LOAD_CONST_I32 pool[1]  (0)
        0x30,                   // DIV_I32                  (10 / 0 → trap)
        0x10, 0x01, 0x00,       // STORE_VAR_I32  var[1]   (unreached)
        0x8C,                   // RET_VOID
    ];

    write_container(
        &single_function_container(&scan_bytecode, 2, &[10, 0]),
        path,
    );
}

/// REQ-VC-vm-cli-007: a runtime trap with `--dump-vars` writes the current variable
/// state before exiting with code 1.
#[spec_test(REQ_VC_vm_cli_007)]
fn run_when_fault_and_dump_vars_then_writes_variables_and_exits_1(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("fault.iplc");
    let dump_path = dir.path().join("vars.txt");
    write_fault_with_vars_container(&container_path);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&container_path)
        .arg("--dump-vars")
        .arg(&dump_path)
        .arg("--scans")
        .arg("1");
    cmd.assert()
        .code(1)
        .stderr(predicate::str::contains("V4001"));

    let contents = std::fs::read_to_string(&dump_path)?;
    // x was stored before the fault; y was never stored.
    assert_eq!(contents, "var[0]: 10\nvar[1]: 0\n");

    Ok(())
}

/// REQ-VC-vm-cli-010: an unreachable dump path returns V6004 with exit code 2.
#[spec_test(REQ_VC_vm_cli_010)]
fn run_when_dump_path_in_nonexistent_directory_then_exit_2_and_v6004(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("test.iplc");
    write_steel_thread_container(&container_path);

    // A parent directory that doesn't exist → File::create fails.
    let dump_path = dir.path().join("no_such_subdir").join("vars.txt");

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&container_path)
        .arg("--dump-vars")
        .arg(&dump_path)
        .arg("--scans")
        .arg("1");
    cmd.assert()
        .code(2)
        .stderr(predicate::str::contains("V6004"));

    Ok(())
}

/// REQ-VC-vm-cli-009: every declared type renders as its own IEC form. A
/// `STRING`'s variable slot is unused, so reading it prints a plausible `0`
/// rather than the string — the defect this covers.
#[spec_test(REQ_VC_vm_cli_009)]
fn run_when_dump_vars_and_every_type_then_renders_each_per_its_type(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("types.iplc");
    write_compiled_container(
        &container_path,
        "
PROGRAM main
  VAR
    msg   : STRING[20] := 'hello';
    flag  : BOOL := TRUE;
    n     : DINT := 42;
    ratio : REAL := 1.5;
    mask  : WORD := 16#ABCD;
    span  : TIME := T#1500ms;
    day   : DATE := D#2024-01-15;
    clock : TIME_OF_DAY := TOD#14:30:00;
    tick  : TIME_OF_DAY := TOD#14:30:00.250;
    stamp : DATE_AND_TIME := DT#2024-01-15-14:30:00;
  END_VAR
  flag := flag;
END_PROGRAM
",
    );

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&container_path)
        .arg("--scans")
        .arg("1")
        .arg("--dump-vars");
    let out = cmd.assert().success();
    let dump = String::from_utf8(out.get_output().stdout.clone())?;

    assert!(dump.contains("msg: 'hello'\n"), "dump was:\n{dump}");
    assert!(dump.contains("flag: TRUE\n"), "dump was:\n{dump}");
    assert!(dump.contains("n: 42\n"), "dump was:\n{dump}");
    assert!(dump.contains("ratio: 1.5\n"), "dump was:\n{dump}");
    assert!(dump.contains("mask: 16#ABCD\n"), "dump was:\n{dump}");
    assert!(dump.contains("span: T#1500ms\n"), "dump was:\n{dump}");
    assert!(dump.contains("day: D#2024-01-15\n"), "dump was:\n{dump}");
    assert!(dump.contains("clock: TOD#14:30:00\n"), "dump was:\n{dump}");
    assert!(
        dump.contains("tick: TOD#14:30:00.250\n"),
        "dump was:\n{dump}"
    );
    assert!(
        dump.contains("stamp: DT#2024-01-15-14:30:00\n"),
        "dump was:\n{dump}"
    );

    Ok(())
}

/// REQ-VC-vm-cli-009: a `WSTRING` renders its content as a double-quoted IEC
/// literal, from the same data-region reader as `STRING`.
#[spec_test(REQ_VC_vm_cli_009)]
fn run_when_dump_vars_and_wstring_then_renders_double_quoted_content(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("wide.iplc");
    write_compiled_container(
        &container_path,
        "
PROGRAM main
  VAR
    wide : WSTRING[20] := \"hello\";
  END_VAR
END_PROGRAM
",
    );

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&container_path)
        .arg("--scans")
        .arg("1")
        .arg("--dump-vars");
    let out = cmd.assert().success();
    let dump = String::from_utf8(out.get_output().stdout.clone())?;

    assert!(dump.contains("wide: \"hello\"\n"), "dump was:\n{dump}");

    Ok(())
}

/// REQ-VC-vm-cli-007: a dump taken after a trap reaches STRING content too —
/// the faulted VM keeps the data region its variables point into.
#[spec_test(REQ_VC_vm_cli_007)]
fn run_when_fault_and_dump_vars_then_string_content_still_rendered(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("fault.iplc");
    write_compiled_container(
        &container_path,
        "
PROGRAM main
  VAR
    msg   : STRING[20] := 'hello';
    zero  : DINT := 0;
    boom  : DINT;
  END_VAR
  boom := 10 / zero;
END_PROGRAM
",
    );

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&container_path)
        .arg("--scans")
        .arg("1")
        .arg("--dump-vars");
    let out = cmd.assert().code(1);
    let dump = String::from_utf8(out.get_output().stdout.clone())?;

    assert!(dump.contains("msg: 'hello'\n"), "dump was:\n{dump}");

    Ok(())
}

/// REQ-VC-vm-cli-009: a structure, array or function-block variable keeps its
/// contents in the data region, and its slot holds the byte offset of them.
/// Dumping the slot printed that offset as if it were the value — and a
/// convincing one, since it moves when an unrelated declaration changes size.
#[spec_test(REQ_VC_vm_cli_009)]
fn run_when_dump_vars_and_aggregates_then_names_type_instead_of_offset(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("agg.iplc");
    write_compiled_container(
        &container_path,
        "
TYPE Point : STRUCT
    X : DINT;
    Y : DINT;
END_STRUCT;
END_TYPE

PROGRAM main
VAR
    origin : Point;
    counts : ARRAY[1..3] OF DINT;
    timer  : TON;
    plain  : DINT := 7;
END_VAR
    origin.X := 11;
    counts[1] := 100;
END_PROGRAM
",
    );

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&container_path)
        .arg("--scans")
        .arg("1")
        .arg("--dump-vars");
    let out = cmd.assert().success();
    let dump = String::from_utf8(out.get_output().stdout.clone())?;

    assert!(dump.contains("origin: <POINT>\n"), "dump was:\n{dump}");
    assert!(
        dump.contains("counts: <ARRAY OF DINT>\n"),
        "dump was:\n{dump}"
    );
    assert!(dump.contains("timer: <TON>\n"), "dump was:\n{dump}");
    // The scalar alongside them still shows its value.
    assert!(dump.contains("plain: 7\n"), "dump was:\n{dump}");

    Ok(())
}

/// A named subrange also reaches the renderer without an elementary type tag,
/// but its slot *does* hold its value. It must keep showing it.
#[spec_test(REQ_VC_vm_cli_009)]
fn run_when_dump_vars_and_named_subrange_then_shows_value() -> Result<(), Box<dyn std::error::Error>>
{
    let dir = TempDir::new()?;
    let container_path = dir.path().join("sub.iplc");
    write_compiled_container(
        &container_path,
        "
TYPE Level : INT (0..100); END_TYPE

PROGRAM main
VAR
    lvl : Level := 75;
END_VAR
END_PROGRAM
",
    );

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&container_path)
        .arg("--scans")
        .arg("1")
        .arg("--dump-vars");
    let out = cmd.assert().success();
    let dump = String::from_utf8(out.get_output().stdout.clone())?;

    assert!(dump.contains("lvl: 75\n"), "dump was:\n{dump}");

    Ok(())
}
