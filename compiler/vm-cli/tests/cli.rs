// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![expect(
    clippy::unwrap_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use std::path::Path;
use std::process::Command;

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use ironplc_container::test_support::{container_bytes, steel_thread_debug_builder};
use ironplc_container::{
    Container, ContainerBuilder, FunctionId, InstanceId, ProgramInstanceEntry, TaskEntry, TaskId,
    TaskType, VarIndex,
};
use predicates::prelude::*;

// Children by subject under test; the wrapper keeps their files under
// `tests/cli/`, since a test crate root resolves `mod` files next to itself,
// where cargo would build each as a separate test binary.
mod cli {
    mod benchmark_command;
    mod dump_vars;
    mod golden_files;
    mod run_command;
    mod serve_command;
    mod serve_device;
    mod serve_discovery;
    mod serve_store;
    mod serve_tcp;
}

/// Spec-conformance requirements generated from `specs/design/vm-cli.md`.
/// Referenced by `#[spec_test(REQ_VC_NNN)]`. See vm-cli/build.rs.
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

/// Serializes a container to the given path.
fn write_container(container: &Container, path: &Path) {
    std::fs::write(path, container_bytes(container)).unwrap();
}

/// Builds the steel thread container (x := 10; y := x + 32) and writes it to
/// the given path.
fn write_steel_thread_container(path: &Path) {
    write_container(&steel_thread_debug_builder().build(), path);
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
