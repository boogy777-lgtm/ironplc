//! Shared helpers for the runtime integration tests.
//!
//! Fixtures are compiled with the real project pipeline and round-tripped
//! through the container wire format, so `header.layout_hash` is the value a
//! deployed artifact carries.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here. Each
// integration test target uses a different subset of these helpers, so some
// are dead in any single target — the allow keeps that from warning.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    dead_code,
    reason = "integration test target: panicking helpers are sanctioned in tests and each target uses a subset of the shared helpers"
)]

use std::io::Cursor;

use ironplc_codegen::EmptyLookup;
use ironplc_container::{Container, VarIndex};
use ironplc_dsl::core::{FileId, Id};
use ironplc_parser::options::CompilerOptions;
use ironplc_project::{compile, MemoryBackedProject};
use ironplc_runtime::RuntimeHost;

/// Compiles `source` and round-trips the container through the wire format.
pub fn compile_source(source: &str) -> Container {
    compile_container(source, None)
}

/// Compiles `source` with the given engineering-side `(name, uid)` table and
/// round-trips the container through the wire format.
pub fn compile_with_ids(source: &str, ids: &[(&str, u64)]) -> Container {
    compile_container(source, Some(ids))
}

/// Compiles `source`, assigning stable variable IDs when `ids` is present,
/// and round-trips the container through the wire format.
fn compile_container(source: &str, ids: Option<&[(&str, u64)]>) -> Container {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    project.add_source(FileId::from_string("main.st"), source.to_owned());
    if let Some(ids) = ids {
        project.set_stable_var_ids(
            ids.iter()
                .map(|(name, uid)| (Id::from(name), *uid))
                .collect(),
        );
    }

    let output = compile(
        &mut project,
        &CompilerOptions::default(),
        &EmptyLookup,
        vec![],
    );
    assert!(
        output.diagnostics.is_empty(),
        "fixture must compile cleanly: {:?}",
        output.diagnostics
    );

    let container = output.container.unwrap();
    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    Container::read_from(&mut Cursor::new(&bytes)).unwrap()
}

/// Serializes a container to its wire-format bytes, the form an
/// `AcceptEdits` command carries.
pub fn container_bytes(container: &Container) -> Vec<u8> {
    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    bytes
}

/// Finds the variable table index of a named variable via the debug section.
pub fn variable_index(container: &Container, name: &str) -> VarIndex {
    container
        .debug_section
        .as_ref()
        .unwrap()
        .var_names
        .iter()
        .find(|entry| entry.name.eq_ignore_ascii_case(name))
        .unwrap()
        .var_index
}

/// A `PROGRAM main` with one DINT `Counter` and the given body.
pub fn counter_program(body: &str) -> String {
    format!(
        "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  {body}
END_PROGRAM
"
    )
}

/// A host whose program increments `Counter` by one per scan.
pub fn counter_host(step: i32) -> (RuntimeHost, VarIndex) {
    let base = compile_source(&counter_program(&format!("Counter := Counter + {step};")));
    let counter = variable_index(&base, "Counter");
    (RuntimeHost::new(base).unwrap(), counter)
}
