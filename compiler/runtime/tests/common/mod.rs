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
use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use ironplc_project::{compile, MemoryBackedProject, SidecarKey};
use ironplc_runtime::RuntimeHost;

/// Compiles `source` and round-trips the container through the wire format.
pub fn compile_source(source: &str) -> Container {
    compile_container(source, &[])
}

/// Compiles `source` with the given engineering-side program-variable
/// `(name, uid)` table and round-trips the container through the wire
/// format. The scope is the conventional program name of these fixtures;
/// codegen matches program variables by name, so it does not participate.
pub fn compile_with_ids(source: &str, ids: &[(&str, u64)]) -> Container {
    let keys: Vec<(SidecarKey, u64)> = ids
        .iter()
        .map(|(name, uid)| (SidecarKey::new("main", name), *uid))
        .collect();
    compile_container(source, &keys)
}

/// Compiles `source` with the given engineering-side keyed UID table —
/// `(scope, name, uid)` triples where an FB field's scope is its FB type
/// name (ADR 0059) — and round-trips the container through the wire format.
pub fn compile_with_uid_keys(source: &str, keys: &[(&str, &str, u64)]) -> Container {
    let keyed: Vec<(SidecarKey, u64)> = keys
        .iter()
        .map(|(scope, name, uid)| (SidecarKey::new(scope, name), *uid))
        .collect();
    compile_container(source, &keyed)
}

/// Compiles `source`, assigning the given stable variable IDs, and
/// round-trips the container through the wire format.
fn compile_container(source: &str, ids: &[(SidecarKey, u64)]) -> Container {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    project.add_source(FileId::from_string("main.st"), source.to_owned());
    project.set_stable_var_ids(ids.to_vec());

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
///
/// The host is granted the execution permit: fixtures model the standalone
/// composition roots, which grant at startup (the HA redundancy
/// architecture, "Minimal Seams" 1).
pub fn counter_host(step: i32) -> (RuntimeHost, VarIndex) {
    let base = compile_source(&counter_program(&format!("Counter := Counter + {step};")));
    let counter = variable_index(&base, "Counter");
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();
    (host, counter)
}
