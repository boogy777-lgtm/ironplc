// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use std::io::{self, Read, Write};
use std::net::{TcpListener, TcpStream};
use std::path::{Path, PathBuf};
use std::process::Command;
use std::time::Duration;

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use ironplc_container::debug_section::{iec_type_tag, VarNameEntry};
use ironplc_container::test_support::{
    container_bytes, single_function_container, steel_thread_debug_builder,
};
use ironplc_container::{
    Container, ContainerBuilder, FunctionId, InstanceId, ProgramInstanceEntry, TaskEntry, TaskId,
    TaskType, VarIndex,
};
use predicates::prelude::*;
use spec_test_macro::spec_test;
use tempfile::TempDir;

/// Spec-conformance requirements generated from `specs/design/vm-cli.md`.
/// Referenced by `#[spec_test(REQ_VC_NNN)]`. See vm-cli/build.rs.
#[allow(dead_code)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}

/// Meta-test: every requirement in `specs/design/vm-cli.md` has a
/// `#[spec_test(REQ_VC_NNN)]` somewhere in src/ or tests/. The build script
/// populates `UNTESTED` from files it scans.
#[test]
fn all_spec_requirements_have_tests() {
    assert!(
        spec_requirements::UNTESTED.is_empty(),
        "Requirements in spec with no conformance test: {:?}",
        spec_requirements::UNTESTED
    );
}

/// One-time generator for golden test files. Run with:
/// cargo test -p ironplc-vm-cli --test cli generate_golden -- --ignored --nocapture
///
/// Both goldens are frozen artifacts that exercise the container reader
/// end-to-end, and both must be refreshed whenever `FORMAT_VERSION` bumps:
/// the reader only accepts the current version. Last refreshed for the
/// population of the header integrity hashes (`content_hash`, `debug_hash`,
/// `layout_hash`), so a golden load also exercises the ADR-0006 load-time
/// verifier (REQ-CF-container-029).
#[test]
#[ignore]
fn generate_golden_files() {
    let steel_thread_path = path_to_golden_resource("steel_thread.iplc");
    write_steel_thread_container(&steel_thread_path);
    eprintln!("Generated golden file: {}", steel_thread_path.display());

    let path = path_to_golden_resource("debug_source_file_table.iplc");
    write_debug_source_file_table_container(&path);
    eprintln!("Generated golden file: {}", path.display());
}

fn path_to_golden_resource(name: &str) -> PathBuf {
    let mut path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    path.push("resources");
    path.push("test");
    path.push(name);
    path
}

/// Serializes a container to the given path.
fn write_container(container: &Container, path: &Path) {
    std::fs::write(path, container_bytes(container)).unwrap();
}

/// Builds the steel thread container (x := 10; y := x + 32) and writes it to
/// the given path.
fn write_steel_thread_container(path: &Path) {
    write_container(&steel_thread_debug_builder().build(), path);
}

/// Builds a container exercising the debug section features described in
/// `specs/design/debugger-support.md` §"Tag Registry":
///
/// - `SOURCE_FILE_TABLE` (tag 6) with two entries (`main.st`, `lib.st`)
///   whose `content_hash` fields are real BLAKE3 digests of synthetic
///   source bytes.
/// - A `LINE_MAP` (tag 1) whose entries reference both files via
///   `file_id`, including one entry at the same `(line, column)` as
///   another but with a different `file_id` to exercise the post-PR
///   wire layout.
///
/// The bytecode itself is the same `x := 10; y := x + 32` steel thread,
/// so the example runs end-to-end under `ironplcvm`. Loading the file
/// also exercises the BLAKE3-vs-SHA-256 spec change (`header.source_hash`
/// is gone; bytes 40-71 must be zero), and the backwards-compat read
/// of the original `steel_thread.iplc` is verified by the other vm-cli
/// tests.
fn write_debug_source_file_table_container(path: &Path) {
    use ironplc_container::debug_section::{LineMapEntry, SourceFileEntry};
    use ironplc_container::id_types::{FunctionId, SourceColumn, SourceFileId, SourceLine};

    let main_source =
        b"PROGRAM main\nVAR x, y : DINT; END_VAR\nx := 10;\ny := lib_add(x, 32);\nEND_PROGRAM\n";
    let lib_source = b"FUNCTION lib_add : DINT\nVAR_INPUT a, b : DINT; END_VAR\nlib_add := a + b;\nEND_FUNCTION\n";

    // Same base as `write_steel_thread_container` — the bytecode, constants
    // and `x`/`y` debug names — plus the source-file table and line map this
    // fixture exists to exercise.
    let container = steel_thread_debug_builder()
        // file_id = 0 is the program file, file_id = 1 the library.
        .add_source_file(SourceFileEntry {
            path: "src/main.st".into(),
            content_hash: *blake3::hash(main_source).as_bytes(),
        })
        .add_source_file(SourceFileEntry {
            path: "src/lib.st".into(),
            content_hash: *blake3::hash(lib_source).as_bytes(),
        })
        // LineMap mixing file_ids: the assignment lines are in main.st,
        // the addition is "inlined" from lib.st. The exact line/column
        // numbers are illustrative.
        .add_line_map_entry(LineMapEntry {
            function_id: FunctionId::SCAN,
            bytecode_offset: 0,
            file_id: SourceFileId::new(0),
            source_line: SourceLine::new(3),
            source_column: SourceColumn::new(1),
        })
        .add_line_map_entry(LineMapEntry {
            function_id: FunctionId::SCAN,
            bytecode_offset: 9,
            file_id: SourceFileId::new(1),
            source_line: SourceLine::new(3),
            source_column: SourceColumn::new(1),
        })
        .add_line_map_entry(LineMapEntry {
            function_id: FunctionId::SCAN,
            bytecode_offset: 13,
            file_id: SourceFileId::new(0),
            source_line: SourceLine::new(4),
            source_column: SourceColumn::new(1),
        })
        .build();

    write_container(&container, path);
}

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

#[test]
fn run_when_golden_container_file_then_ok() -> Result<(), Box<dyn std::error::Error>> {
    let golden_path = path_to_golden_resource("steel_thread.iplc");
    let dir = TempDir::new()?;
    let dump_path = dir.path().join("vars.txt");

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&golden_path)
        .arg("--dump-vars")
        .arg(&dump_path)
        .arg("--scans")
        .arg("1");
    cmd.assert().success();

    let contents = std::fs::read_to_string(&dump_path)?;
    // The golden is a frozen artifact written by the current container
    // writer: loading it succeeds only because its nonzero integrity hashes
    // verify against its sections — the golden load is itself the
    // end-to-end check of the ADR-0006 load-time verifier. Containers with
    // zero hashes (written before the hashes were populated) stay loadable
    // and are covered by the container crate's legacy-accept tests.
    assert_eq!(contents, "x: 10\ny: 42\n");

    Ok(())
}

/// Loads the debug-source-file-table example container (generated by the
/// `generate_golden_files` ignored test) and asserts the new debug
/// section features round-trip through the reader.
#[test]
fn read_when_debug_source_file_table_golden_then_decodes_new_debug_fields() {
    use ironplc_container::id_types::FunctionId;
    use ironplc_container::Container;
    use std::io::Cursor;

    let bytes = std::fs::read(path_to_golden_resource("debug_source_file_table.iplc")).unwrap();
    let container = Container::read_from(&mut Cursor::new(&bytes)).unwrap();

    let debug = container.debug_section.as_ref().unwrap();

    // SOURCE_FILE_TABLE round-trip
    assert_eq!(debug.source_files.len(), 2);
    assert_eq!(debug.source_files[0].path, "src/main.st");
    assert_eq!(debug.source_files[1].path, "src/lib.st");
    let expected_main = *blake3::hash(
        b"PROGRAM main\nVAR x, y : DINT; END_VAR\nx := 10;\ny := lib_add(x, 32);\nEND_PROGRAM\n",
    )
    .as_bytes();
    let expected_lib =
        *blake3::hash(b"FUNCTION lib_add : DINT\nVAR_INPUT a, b : DINT; END_VAR\nlib_add := a + b;\nEND_FUNCTION\n")
            .as_bytes();
    assert_eq!(debug.source_files[0].content_hash, expected_main);
    assert_eq!(debug.source_files[1].content_hash, expected_lib);

    // LineMap mixes file_ids
    let lm = &debug.line_map;
    assert_eq!(lm.len(), 3);
    assert!(lm.iter().all(|e| e.function_id == FunctionId::SCAN));
    assert!(lm.iter().any(|e| e.file_id.raw() == 0));
    assert!(lm.iter().any(|e| e.file_id.raw() == 1));

    // The new `header.source_hash` is gone; the wire slot must be zero.
    assert_eq!(container.header.reserved_hash_slot, [0u8; 32]);
}

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

#[test]
fn version_then_ok() -> Result<(), Box<dyn std::error::Error>> {
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("version");
    cmd.assert()
        .success()
        .stdout(predicate::str::starts_with("ironplcvm version "));

    Ok(())
}

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

/// Builds a container with an explicit cyclic task at `interval_us`.
/// The program is a no-op (RET_VOID) so run_round is cheap.
fn write_cyclic_task_container(path: &Path, interval_us: u64) {
    #[rustfmt::skip]
    let bytecode: Vec<u8> = vec![
        0x8C,                   // RET_VOID
    ];

    let task = TaskEntry {
        task_id: TaskId::DEFAULT,
        priority: 0,
        task_type: TaskType::Cyclic,
        flags: 0x01, // enabled
        interval_us,
        single_var_index: VarIndex::NO_SINGLE_VAR,
        watchdog_us: 0,
        input_image_offset: 0,
        output_image_offset: 0,
        reserved: [0; 4],
    };
    let program = ProgramInstanceEntry {
        instance_id: InstanceId::DEFAULT,
        task_id: TaskId::DEFAULT,
        entry_function_id: FunctionId::new(0),
        var_table_offset: 0,
        var_table_count: 0,
        fb_instance_offset: 0,
        fb_instance_count: 0,
        init_function_id: FunctionId::new(0),
    };

    let container = ContainerBuilder::new()
        .num_variables(0)
        .add_function(FunctionId::new(0), &bytecode, 0, 0, 0)
        .add_task(task)
        .add_program_instance(program)
        .max_call_depth(1)
        .build();

    write_container(&container, path);
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

/// Compiles IEC 61131-3 source to its wire-format bytes.
fn compiled_bytes(source: &str) -> Vec<u8> {
    let options = ironplc_parser::options::CompilerOptions::default();
    let library =
        ironplc_parser::parse_program(source, &ironplc_dsl::core::FileId::default(), &options)
            .unwrap();
    let (analyzed, context) =
        ironplc_analyzer::stages::resolve_types(&[&library], &options).unwrap();
    let container = ironplc_codegen::compile(
        &analyzed,
        &context,
        &ironplc_codegen::CodegenOptions::default(),
        &ironplc_codegen::EmptyLookup,
    )
    .unwrap();

    let mut buf = Vec::new();
    container.write_to(&mut buf).unwrap();
    buf
}

/// Compiles IEC 61131-3 source and writes the container to `path`.
///
/// The hand-built containers above pin the CLI's own behaviour, but they only
/// ever carried `BOOL` and `DINT` variables — which is why a `STRING` printing
/// as its unused slot (issue #1558) went unnoticed. Rendering a real compiled
/// program is the leg that covers the type tags and the data-region layout
/// codegen actually emits.
fn write_compiled_container(path: &Path, source: &str) {
    std::fs::write(path, compiled_bytes(source)).unwrap();
}

/// A `PROGRAM main` with one DINT `Counter` counting up by one per scan.
const COUNTER_PROGRAM: &str = "
PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
";

/// A logic-only edit of [`COUNTER_PROGRAM`] that divides by zero on every
/// scan (`x - x` is always 0). Stages against the counter, so it can be
/// committed and then recognized after a reboot: the driven round of the
/// running trapper faults with V4001 on stderr.
const TRAPPER_PROGRAM: &str = "
PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := 1 / (Counter - Counter);
END_PROGRAM
";

/// One `ironplcvm serve` process fed `stdin`, asserted to exit 0.
fn serve_with_stdin(path: &Path, stdin: &str) -> std::process::Output {
    let mut cmd = assert_cmd::Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("serve").arg(path).write_stdin(stdin);
    cmd.assert().success().get_output().clone()
}

/// The marker JSON of the store file `name` beside `counter.iplc`.
fn marker_value(dir: &TempDir, name: &str) -> serde_json::Value {
    let text = std::fs::read_to_string(dir.path().join(format!("counter.iplc.{name}"))).unwrap();
    serde_json::from_str(&text).unwrap()
}

fn write_marker_value(dir: &TempDir, name: &str, value: serde_json::Value) {
    std::fs::write(
        dir.path().join(format!("counter.iplc.{name}")),
        value.to_string(),
    )
    .unwrap();
}

/// The accept/test/assemble lines that commit `edit` in one session.
fn commit_stdin(edit: &[u8]) -> String {
    let accept = serde_json::json!({"command": "acceptEdits", "program": edit}).to_string();
    format!("{accept}\n{{\"command\":\"testEdits\"}}\n{{\"command\":\"assembleEdits\"}}\n")
}

/// A probe session that reveals which artifact booted: accept the clean
/// counter edit, test it, then untest — the untest's driven round runs the
/// BOOTED artifact. When the committed trapper booted, that round faults
/// with V4001 on stderr; the clean counter rounds stay silent.
fn probe_stdin() -> String {
    let accept =
        serde_json::json!({"command": "acceptEdits", "program": compiled_bytes(COUNTER_PROGRAM)})
            .to_string();
    format!("{accept}\n{{\"command\":\"testEdits\"}}\n{{\"command\":\"untestEdits\"}}\n")
}

/// ADR-0064 amendment: accept → test → assemble commits the candidate's exact
/// wire bytes beside the served container, and a reboot (a new serve process)
/// boots the committed artifact from the store.
#[test]
fn serve_when_assemble_then_reboot_loads_the_committed_artifact(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);

    // Session 1: accept the trapper, run it under Test, assemble it. The
    // driven boundary rounds trap (the candidate divides by zero) — that is
    // the program's own fault, logged to stderr, and does not change the
    // wire answers or the commit.
    let trapper = compiled_bytes(TRAPPER_PROGRAM);
    let output = serve_with_stdin(&container_path, &commit_stdin(&trapper));
    let lines: Vec<&str> = std::str::from_utf8(&output.stdout)?.lines().collect();
    assert_eq!(lines.len(), 3);
    for line in lines {
        let response: serde_json::Value = serde_json::from_str(line)?;
        assert_eq!(response["response"], "ack");
    }

    // The commit landed: slot B holds the wire bytes verbatim (never a
    // re-serialization) and the marker flipped to the new slot.
    assert_eq!(
        std::fs::read(dir.path().join("counter.iplc.slot-b"))?,
        trapper
    );
    assert_eq!(
        marker_value(&dir, "marker"),
        serde_json::json!({"slot": "b", "seq": 2})
    );

    // Session 2, the simulated reboot: the probe's untest round runs the
    // booted artifact — the committed trapper faults, proving the identity.
    let output = serve_with_stdin(&container_path, &probe_stdin());
    let stderr = String::from_utf8(output.stderr)?;
    assert!(
        stderr.contains("V4001"),
        "expected the committed trapper to boot and fault the probe round, stderr:\n{stderr}"
    );
    Ok(())
}

/// No assemble, no commit: a reboot boots the originally served artifact.
#[test]
fn serve_without_assemble_then_reboot_loads_the_original_artifact(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);

    // Session 1 only seeds the store (first serve); nothing commits.
    serve_with_stdin(&container_path, "{\"command\":\"getStatus\"}\n");

    let output = serve_with_stdin(&container_path, &probe_stdin());
    let stderr = String::from_utf8(output.stderr)?;
    assert!(
        !stderr.contains("V4001"),
        "expected the clean counter to boot, stderr:\n{stderr}"
    );
    Ok(())
}

/// Crash residue of a tmp write (partial, unverified) with the marker and
/// the active slot intact: boot the marker's generation; discard the tmp.
#[test]
fn serve_when_tmp_residue_present_then_reboot_boots_the_marker_generation(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);

    // Session 1 seeds the store; then simulate a crash during a tmp write.
    serve_with_stdin(&container_path, "{\"command\":\"getStatus\"}\n");
    std::fs::write(dir.path().join("counter.iplc.tmp"), "partial write")?;

    let output = serve_with_stdin(&container_path, &probe_stdin());
    let stderr = String::from_utf8(output.stderr)?;
    assert!(
        !stderr.contains("V4001"),
        "expected the marker's generation (the clean counter), stderr:\n{stderr}"
    );
    assert!(!dir.path().join("counter.iplc.tmp").exists());
    Ok(())
}

/// Crash during the marker flip: the marker is missing but the residue names
/// a verified slot — boot it and heal the marker.
#[test]
fn serve_when_marker_missing_then_reboot_adopts_the_residue_and_heals(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);
    let trapper = compiled_bytes(TRAPPER_PROGRAM);
    serve_with_stdin(&container_path, &commit_stdin(&trapper));

    // Recreate the flip window: marker deleted, residue names the committed
    // slot.
    std::fs::remove_file(dir.path().join("counter.iplc.marker"))?;
    write_marker_value(
        &dir,
        "marker.tmp",
        serde_json::json!({"slot": "b", "seq": 2}),
    );

    let output = serve_with_stdin(&container_path, &probe_stdin());
    let stderr = String::from_utf8(output.stderr)?;
    assert!(
        stderr.contains("V4001"),
        "expected the residue-named trapper to boot, stderr:\n{stderr}"
    );
    assert_eq!(
        marker_value(&dir, "marker"),
        serde_json::json!({"slot": "b", "seq": 2})
    );
    assert!(!dir.path().join("counter.iplc.marker.tmp").exists());
    Ok(())
}

/// Stale marker: the old marker was restored while the residue names the
/// newer verified slot — the highest-seq verifiable record wins.
#[test]
fn serve_when_stale_marker_then_reboot_boots_the_newest_verifiable(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);
    let trapper = compiled_bytes(TRAPPER_PROGRAM);
    serve_with_stdin(&container_path, &commit_stdin(&trapper));

    // Stale state: the marker names the old active slot at the old seq while
    // the residue names the committed slot at the newer seq.
    write_marker_value(&dir, "marker", serde_json::json!({"slot": "a", "seq": 1}));
    write_marker_value(
        &dir,
        "marker.tmp",
        serde_json::json!({"slot": "b", "seq": 2}),
    );

    let output = serve_with_stdin(&container_path, &probe_stdin());
    let stderr = String::from_utf8(output.stderr)?;
    assert!(
        stderr.contains("V4001"),
        "expected the newest verifiable generation (the trapper) to boot, stderr:\n{stderr}"
    );
    assert_eq!(
        marker_value(&dir, "marker"),
        serde_json::json!({"slot": "b", "seq": 2})
    );
    Ok(())
}

/// ADR-0064 amendment wire honesty: a persistence failure answers the
/// assemble line with V6012 instead of an ack; the RAM promotion stands and
/// the store is unchanged, so a reboot would boot the previous generation.
#[test]
fn serve_when_persist_fails_then_assemble_answers_v6012() -> Result<(), Box<dyn std::error::Error>>
{
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);

    // Session 1 seeds the store; then block the commit's tmp write with a
    // directory where the tmp file must land.
    serve_with_stdin(&container_path, "{\"command\":\"getStatus\"}\n");
    std::fs::create_dir(dir.path().join("counter.iplc.tmp"))?;

    let trapper = compiled_bytes(TRAPPER_PROGRAM);
    let output = serve_with_stdin(&container_path, &commit_stdin(&trapper));
    let lines: Vec<&str> = std::str::from_utf8(&output.stdout)?.lines().collect();
    assert_eq!(lines.len(), 3);
    let assembled: serde_json::Value = serde_json::from_str(lines[2])?;
    assert_eq!(assembled["response"], "error");
    assert_eq!(assembled["vCode"], "V6012");

    // The store still names the seeded generation.
    assert_eq!(
        marker_value(&dir, "marker"),
        serde_json::json!({"slot": "a", "seq": 1})
    );
    assert!(!dir.path().join("counter.iplc.slot-b").exists());
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

/// One ADR-0063 frame: 4-byte little-endian length, then the line bytes.
fn tcp_frame(line: &str) -> Vec<u8> {
    let mut bytes = (line.len() as u32).to_le_bytes().to_vec();
    bytes.extend_from_slice(line.as_bytes());
    bytes
}

/// Reads exactly one frame from `stream`, returning the line bytes.
fn read_frame(stream: &mut TcpStream) -> io::Result<String> {
    let mut header = [0u8; 4];
    stream.read_exact(&mut header)?;
    let len = u32::from_le_bytes(header) as usize;
    let mut payload = vec![0u8; len];
    stream.read_exact(&mut payload)?;
    String::from_utf8(payload).map_err(|err| io::Error::new(io::ErrorKind::InvalidData, err))
}

/// Connects a frame client to `addr` with a read timeout, so a hang fails
/// instead of blocking the suite.
fn connect_frame_client(addr: std::net::SocketAddr) -> io::Result<TcpStream> {
    let stream = TcpStream::connect(addr)?;
    stream.set_read_timeout(Some(Duration::from_secs(15)))?;
    Ok(stream)
}

/// The `vCode` of the next frame answer, parsed.
fn next_frame_v_code(stream: &mut TcpStream) -> io::Result<String> {
    let line = read_frame(stream)?;
    let value: serde_json::Value = serde_json::from_str(&line)
        .map_err(|err| io::Error::new(io::ErrorKind::InvalidData, err))?;
    value["vCode"]
        .as_str()
        .map(str::to_string)
        .ok_or_else(|| io::Error::new(io::ErrorKind::InvalidData, "answer carries no vCode"))
}

/// ADR-0063/0065: the TCP transport serves the same session protocol one
/// frame per message — the `identity` handshake, the edit FSM at one session
/// at a time (a second session is refused with one V6014 line), a framing
/// violation answered with V6013 and dropped, and the listener surviving it
/// all for a fresh session.
#[test]
fn serve_tcp_when_scripted_then_identity_refusal_framing_drop_and_reconnect(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);

    // Reserve the port the way a dialing client would; the server binds it next.
    let probe = TcpListener::bind("127.0.0.1:0")?;
    let addr = probe.local_addr()?;
    drop(probe);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("serve")
        .arg("--listen")
        .arg(addr.to_string())
        .arg(&container_path);
    let mut server = cmd.spawn()?;

    // Session 1: the scripted flow, framed — identity first (ADR-0063), then
    // accept → test → assemble (ADR-0064), each answered in one frame.
    let mut client = connect_frame_client(addr)?;
    client.write_all(&tcp_frame(r#"{"command":"identity"}"#))?;
    let identity: serde_json::Value = serde_json::from_str(&read_frame(&mut client)?)?;
    assert_eq!(identity["response"], "identity");
    assert_eq!(identity["protocol"], 1);
    assert_eq!(identity["device"]["name"], "ironplcvm");
    assert!(identity["device"]["firmwareVersion"]
        .as_str()
        .unwrap()
        .split('.')
        .next()
        .unwrap()
        .parse::<u64>()
        .is_ok());
    assert_eq!(identity["application"]["mode"], "normal");
    assert_eq!(
        identity["application"]["candidate"],
        serde_json::Value::Null
    );
    assert!(identity.get("redundancy").is_none());

    let edit = compiled_bytes(COUNTER_PROGRAM);
    let accept = serde_json::json!({"command": "acceptEdits", "program": edit}).to_string();
    for command in [
        &accept,
        r#"{"command":"testEdits"}"#,
        r#"{"command":"assembleEdits"}"#,
    ] {
        client.write_all(&tcp_frame(command))?;
        let line = read_frame(&mut client)?;
        let value: serde_json::Value = serde_json::from_str(&line)?;
        assert_eq!(value["response"], "ack");
    }

    // A second session while the first holds the device: exactly one V6014
    // refusal line, then the socket closes — and the first session is
    // unaffected (ADR-0065).
    let mut second = connect_frame_client(addr)?;
    let refusal: serde_json::Value = serde_json::from_str(&read_frame(&mut second)?)?;
    assert_eq!(refusal["response"], "error");
    assert_eq!(refusal["vCode"], "V6014");
    assert!(refusal["message"]
        .as_str()
        .unwrap()
        .contains("one engineering session"));
    let mut leftover = Vec::new();
    second.read_to_end(&mut leftover)?;
    assert!(leftover.is_empty());

    client.write_all(&tcp_frame(r#"{"command":"getStatus"}"#))?;
    let status: serde_json::Value = serde_json::from_str(&read_frame(&mut client)?)?;
    assert_eq!(status["response"], "status");
    assert_eq!(status["normal"], 2);

    // Framing garbage on the active session: V6013 on the wire, then the
    // connection drops — the listener survives.
    client.write_all(&[0xff, 0xff, 0xff, 0xff])?;
    assert_eq!(next_frame_v_code(&mut client)?, "V6013");
    let mut leftover = Vec::new();
    client.read_to_end(&mut leftover)?;
    assert!(leftover.is_empty());

    // A reconnect lands a fresh session: the assembled promotion stands in
    // the host (normal generation 2).
    let mut third = connect_frame_client(addr)?;
    third.write_all(&tcp_frame(r#"{"command":"identity"}"#))?;
    let identity: serde_json::Value = serde_json::from_str(&read_frame(&mut third)?)?;
    assert_eq!(identity["response"], "identity");
    assert_eq!(identity["application"]["normal"], 2);

    drop(third);
    server.kill()?;

    Ok(())
}
