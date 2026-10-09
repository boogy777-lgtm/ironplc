//! Guard: every persistent variable that code generation lays out can get a
//! stable ID (ADR-0074).
//!
//! The engineering side keys the persistent declarations of a library
//! (`declared_var_keys`) and code generation lays them out. The two are one
//! definition read twice, and this guard is what holds them together: it keys
//! every declaration the sidecar would key, compiles with those IDs, and fails
//! when a slot of a persistent extent, or a field of a function block type, has
//! no ID. A kind of persistent declaration that is laid out but not keyed fails
//! it, whichever kind it is, because the corpus below is the test programs of
//! the whole compiler and not a list of the kinds known today.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use ironplc_codegen::EmptyLookup;
use ironplc_container::{Container, FbTypeId, VarIndex};
use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use ironplc_project::sidecar::declared_var_keys;
use ironplc_project::{compile, MemoryBackedProject, Project, Sidecar};
use ironplc_test::edit_classes::edit_classes;
use ironplc_test::{shared_resource_path, st_files};
use std::collections::HashSet;

/// Programs for the kinds of persistent declaration, one row per kind, beside
/// the corpus: the sources the kinds appear in are not all programs that
/// compile on their own.
const KINDS: &[(&str, &str)] = &[
    (
        "program variable of each section",
        "PROGRAM main
  VAR_INPUT i : DINT; END_VAR
  VAR_OUTPUT o : DINT; END_VAR
  VAR v : DINT; s : STRING[8]; a : ARRAY[1..3] OF INT; END_VAR
  o := i + v;
END_PROGRAM",
    ),
    (
        "top-level global",
        "VAR_GLOBAL g : DINT; END_VAR
PROGRAM main
  VAR_EXTERNAL g : DINT; END_VAR
  g := g + 1;
END_PROGRAM",
    ),
    (
        "global of the configuration",
        "CONFIGURATION plc
  VAR_GLOBAL g : DINT; h : BOOL; END_VAR
  RESOURCE res ON PROCESSOR
    TASK scan(PRIORITY := 1);
    PROGRAM instance WITH scan : main;
  END_RESOURCE
END_CONFIGURATION
PROGRAM main
  VAR_EXTERNAL g : DINT; END_VAR
  g := g + 1;
END_PROGRAM",
    ),
    (
        "function block field",
        "FUNCTION_BLOCK counter
  VAR_INPUT up : BOOL; END_VAR
  VAR_OUTPUT q : DINT; END_VAR
  VAR n : DINT; END_VAR
  IF up THEN n := n + 1; END_IF;
  q := n;
END_FUNCTION_BLOCK
PROGRAM main
  VAR c : counter; r : DINT; END_VAR
  c(up := TRUE);
  r := c.q;
END_PROGRAM",
    ),
    (
        "edge input of a program",
        "PROGRAM main
  VAR_INPUT go : BOOL R_EDGE; END_VAR
  VAR n : DINT; END_VAR
  IF go THEN n := n + 1; END_IF;
END_PROGRAM",
    ),
    (
        "edge input of a function block",
        "FUNCTION_BLOCK counter
  VAR_INPUT up : BOOL R_EDGE; END_VAR
  VAR_OUTPUT q : DINT; END_VAR
  IF up THEN q := q + 1; END_IF;
END_FUNCTION_BLOCK
PROGRAM main
  VAR c : counter; END_VAR
  c(up := TRUE);
END_PROGRAM",
    ),
];

/// Compiles `source` with a stable ID for every key the sidecar would hold, and
/// returns what code generation laid out as persistent without an ID, or `None`
/// when the source does not compile.
fn without_id(source: &str, options: CompilerOptions) -> Result<Vec<String>, String> {
    let mut project = MemoryBackedProject::new(options);
    project.add_source(FileId::from_string("main.st"), source.to_owned());
    let diagnostics = project.semantic();
    if !diagnostics.is_empty() {
        return Err(format!("{diagnostics:?}"));
    }
    let library = project.analyzed_library().ok_or("no analyzed library")?;
    let keys = declared_var_keys(library, &options);
    let mut sidecar = Sidecar::new();
    sidecar.sync(&keys);
    project.set_stable_var_ids(sidecar.keyed_entries());

    let output = compile(&mut project, &options, &EmptyLookup, vec![]);
    let container = output
        .container
        .ok_or_else(|| format!("{:?}", output.diagnostics))?;
    Ok(unidentified(&container))
}

/// The persistent slots and function block fields of `container` that carry no
/// stable ID, named by the debug section.
fn unidentified(container: &Container) -> Vec<String> {
    let section = container.type_section.as_ref().expect("type section");
    let names = |index: u16| {
        container
            .debug_section
            .as_ref()
            .and_then(|debug| {
                debug
                    .var_names
                    .iter()
                    .find(|entry| entry.var_index == VarIndex::new(index))
            })
            .map_or_else(|| format!("slot {index}"), |entry| entry.name.clone())
    };
    let identified: HashSet<u16> = section
        .stable_vars
        .iter()
        .map(|entry| entry.var_index.raw())
        .collect();
    let mut missing = Vec::new();
    for extent in container.persistent_extents() {
        for index in extent.var_start..extent.var_start + extent.var_count {
            if !identified.contains(&index) {
                missing.push(names(index));
            }
        }
    }
    let fields: HashSet<(u16, u8)> = section
        .fb_field_uids
        .iter()
        .map(|entry| (entry.fb_type_id.raw(), entry.field_index))
        .collect();
    for descriptor in &section.fb_types {
        for ordinal in 0..descriptor.fields.len() as u8 {
            if !fields.contains(&(descriptor.type_id.raw(), ordinal)) {
                missing.push(format!(
                    "field {ordinal} of function block type {}",
                    FbTypeId::raw(descriptor.type_id)
                ));
            }
        }
    }
    missing
}

fn both_options() -> [CompilerOptions; 2] {
    [
        CompilerOptions {
            allow_top_level_var_global: true,
            ..CompilerOptions::default()
        },
        CompilerOptions {
            allow_top_level_var_global: true,
            allow_system_uptime_global: true,
            ..CompilerOptions::default()
        },
    ]
}

#[test]
fn keys_when_each_kind_of_persistent_declaration_is_laid_out_then_it_has_an_id() {
    for (kind, source) in KINDS {
        for options in both_options() {
            let missing = without_id(source, options);
            assert_eq!(
                missing,
                Ok(Vec::new()),
                "{kind} (system uptime globals: {}) is laid out as persistent and has no key",
                options.allow_system_uptime_global
            );
        }
    }
}

/// The sources of the compiler's test programs: the shared resources, and both
/// sources of every class of edit (strings, arrays, structures, function blocks,
/// globals of a configuration, calls). A program that does not compile (a syntax
/// sample, a program without a body to run) is not a program to lay out.
fn corpus() -> Vec<(String, String)> {
    let mut sources: Vec<(String, String)> = st_files(&shared_resource_path(""))
        .into_iter()
        .map(|path| {
            let source = std::fs::read_to_string(&path).expect("test program");
            (path.display().to_string(), source)
        })
        .collect();
    for class in edit_classes() {
        sources.push((format!("{} (before)", class.name), class.before));
        sources.push((format!("{} (after)", class.name), class.after));
    }
    sources
}

#[test]
fn keys_when_a_test_program_compiles_then_every_persistent_variable_has_an_id() {
    let mut compiled = 0;
    let mut gaps = Vec::new();
    for (name, source) in corpus() {
        for options in both_options() {
            if let Ok(missing) = without_id(&source, options) {
                compiled += 1;
                if !missing.is_empty() {
                    gaps.push(format!("{name}: {missing:?}"));
                }
            }
        }
    }

    assert!(compiled > 100, "the corpus compiled only {compiled} times");
    assert_eq!(gaps, Vec::<String>::new());
}
