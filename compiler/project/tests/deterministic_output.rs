//! Guard: the same source files and options give the same output on every run.
//!
//! The workstation compares a project with the controller by the hash of the
//! bytes of the container, and checks an online change by "a selective build
//! equals a full build". Neither works while a full build is not equal to
//! itself. Every collection a result is made of has an order that is a rule
//! (declaration order, a sort that names its key); this guard holds the whole
//! pipeline to it from the outside.
//!
//! One process makes each hash container with its own random seed, so the
//! same program compiled several times in one test is as good as several runs
//! of the tool: a result that depends on the order of a hash container shows
//! as two different results among the compiles.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use ironplc_codegen::EmptyLookup;
use ironplc_container::{Container, PersistentExtent};
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_parser::options::CompilerOptions;
use ironplc_project::sidecar::declared_var_keys;
use ironplc_project::{compile, MemoryBackedProject, Project, Sidecar};
use ironplc_test::edit_classes::edit_classes;
use ironplc_test::{shared_resource_path, st_files};

/// How many times a program is compiled. Two compiles of a container with two
/// entries in a hash container differ half the time; ten make a miss
/// vanishingly rare for a defect that has more than two orders.
const RUNS: usize = 10;

/// A program made for this guard: three enumerations, three function block
/// types with instances (declared in an order that is not the order of their
/// names), three structures, arrays, strings, globals of a configuration and a
/// function with inputs and outputs.
const RICH: &str = "TYPE
  color : (red, green, blue);
  mode : (idle, running, stopped);
  level : (low, mid, high);
END_TYPE

TYPE
  point : STRUCT x : DINT; y : DINT; END_STRUCT;
  label : STRUCT n : INT; text : STRING[8]; END_STRUCT;
  sample : STRUCT t : DINT; ok : BOOL; END_STRUCT;
END_TYPE

FUNCTION scale : DINT
  VAR_INPUT a : DINT; b : DINT; END_VAR
  VAR_OUTPUT res : DINT; END_VAR
  scale := a * b;
END_FUNCTION

FUNCTION_BLOCK zeta_counter
  VAR_INPUT up : BOOL; END_VAR
  VAR_OUTPUT q : DINT; END_VAR
  VAR n : DINT; name : STRING[6]; END_VAR
  IF up THEN n := n + 1; END_IF;
  q := n;
END_FUNCTION_BLOCK

FUNCTION_BLOCK alpha_latch
  VAR_INPUT set : BOOL; END_VAR
  VAR_OUTPUT q : BOOL; END_VAR
  VAR held : BOOL; END_VAR
  IF set THEN held := TRUE; END_IF;
  q := held;
END_FUNCTION_BLOCK

FUNCTION_BLOCK mid_sum
  VAR_INPUT v : DINT; END_VAR
  VAR_OUTPUT total : DINT; END_VAR
  total := total + v;
END_FUNCTION_BLOCK

CONFIGURATION plc
  VAR_GLOBAL g : DINT; flag : BOOL; END_VAR
  RESOURCE res ON PROCESSOR
    TASK scan(PRIORITY := 1);
    PROGRAM instance WITH scan : main;
  END_RESOURCE
END_CONFIGURATION

PROGRAM main
  VAR_EXTERNAL g : DINT; flag : BOOL; END_VAR
  VAR
    c : color := green;
    m : mode := idle;
    l : level := high;
    p : point;
    lb : label;
    sm : sample;
    arr : ARRAY[1..4] OF INT;
    names : ARRAY[1..2] OF STRING[4];
    s : STRING[10];
    z : zeta_counter;
    a : alpha_latch;
    d : mid_sum;
    r : DINT;
    extra : DINT;
  END_VAR
  c := blue;
  m := running;
  l := low;
  p.x := 1;
  p.y := 2;
  lb.n := 3;
  lb.text := 'hi';
  sm.t := 4;
  arr[2] := 5;
  names[1] := 'ab';
  s := 'text';
  z(up := TRUE);
  a(set := flag);
  d(v := p.x);
  r := scale(a := p.x, b := p.y);
  extra := r;
  g := g + r + z.q + d.total;
END_PROGRAM";

/// Three independent errors, one in each unit: an undeclared variable, a
/// type that does not exist, and an assignment of a value of the wrong type.
const THREE_ERRORS: &str = "FUNCTION_BLOCK first
  VAR x : INT; END_VAR
  undeclared_one := 1;
END_FUNCTION_BLOCK

FUNCTION_BLOCK second
  VAR y : no_such_type; END_VAR
END_FUNCTION_BLOCK

FUNCTION_BLOCK third
  VAR z : INT; b : BOOL; END_VAR
  z := b;
END_FUNCTION_BLOCK

PROGRAM main
  VAR i : first; j : second; k : third; undeclared_two : INT; END_VAR
  undeclared_three := 1;
END_PROGRAM";

/// Programs made for this guard, with the name that reports them.
fn made_programs() -> Vec<(String, String)> {
    vec![
        ("rich program".to_owned(), RICH.to_owned()),
        (
            "three independent errors".to_owned(),
            THREE_ERRORS.to_owned(),
        ),
    ]
}

/// The sources of the compiler's test programs (the shared resources and both
/// sources of every class of edit) and the programs made for this guard.
fn programs() -> Vec<(String, String)> {
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
    sources.extend(made_programs());
    sources
}

fn options() -> CompilerOptions {
    CompilerOptions {
        allow_top_level_var_global: true,
        ..CompilerOptions::default()
    }
}

/// Everything one compile of a project gives that a user or the controller
/// sees. Not `Debug`: a failed comparison would print the container.
#[derive(PartialEq)]
struct Outcome {
    /// The diagnostics, in order: code, position and messages.
    diagnostics: Vec<String>,
    /// The bytes of the container, when there is one.
    bytes: Option<Vec<u8>>,
    /// The layout hash, when there is a container.
    layout_hash: Option<[u8; 32]>,
    /// The persistent extent, when there is a container.
    extents: Option<Vec<PersistentExtent>>,
}

fn describe(diagnostic: &Diagnostic) -> String {
    let secondary: Vec<String> = diagnostic
        .secondary
        .iter()
        .map(|label| format!("{:?} {} {}", label.location, label.file_id, label.message))
        .collect();
    format!(
        "{} {:?} {} {} {:?}",
        diagnostic.code,
        diagnostic.primary.location,
        diagnostic.primary.file_id,
        diagnostic.primary.message,
        secondary
    )
}

fn container_bytes(container: &Container) -> Vec<u8> {
    let mut bytes = Vec::new();
    container.write_to(&mut bytes).expect("container writes");
    bytes
}

/// Compiles the files, added to a new project in the given order. With
/// `keyed`, every key the sidecar would hold gets a stable variable ID first.
fn build(files: &[(&str, &str)], keyed: bool) -> Outcome {
    let options = options();
    let mut project = MemoryBackedProject::new(options);
    for (name, source) in files {
        project.add_source(FileId::from_string(name), (*source).to_owned());
    }
    if keyed {
        project.semantic();
        if let Some(library) = project.analyzed_library() {
            let mut sidecar = Sidecar::new();
            sidecar.sync(&declared_var_keys(library, &options));
            project.set_stable_var_ids(sidecar.keyed_entries());
        }
    }
    let output = compile(&mut project, &options, &EmptyLookup, vec![]);
    Outcome {
        diagnostics: output.diagnostics.iter().map(describe).collect(),
        bytes: output.container.as_ref().map(container_bytes),
        layout_hash: output
            .container
            .as_ref()
            .map(Container::compute_layout_hash),
        extents: output.container.as_ref().map(Container::persistent_extents),
    }
}

/// The names of the programs for which `key` of the outcomes of `RUNS`
/// compiles is not the same on every compile.
fn unstable<K: PartialEq>(
    programs: &[(String, String)],
    keyed: bool,
    key: impl Fn(&Outcome) -> K,
) -> Vec<String> {
    programs
        .iter()
        .filter(|(_, source)| {
            let files = [("main.st", source.as_str())];
            let first = key(&build(&files, keyed));
            (1..RUNS).any(|_| key(&build(&files, keyed)) != first)
        })
        .map(|(name, _)| name.clone())
        .collect()
}

#[test]
fn compile_when_made_program_then_container_is_built() {
    let outcome = build(&[("main.st", RICH)], false);

    assert_eq!(outcome.diagnostics, Vec::<String>::new());
    assert!(outcome.bytes.is_some());
}

#[test]
fn compile_when_corpus_programs_repeated_then_container_bytes_equal() {
    assert_eq!(
        unstable(&programs(), false, |outcome| outcome.bytes.clone()),
        Vec::<String>::new()
    );
}

#[test]
fn compile_when_corpus_programs_repeated_with_stable_ids_then_container_bytes_equal() {
    assert_eq!(
        unstable(&programs(), true, |outcome| outcome.bytes.clone()),
        Vec::<String>::new()
    );
}

#[test]
fn analyze_when_corpus_programs_repeated_then_diagnostics_equal() {
    assert_eq!(
        unstable(&programs(), false, |outcome| outcome.diagnostics.clone()),
        Vec::<String>::new()
    );
}

#[test]
fn analyze_when_three_independent_errors_then_three_or_more_diagnostics() {
    let outcome = build(&[("main.st", THREE_ERRORS)], false);

    assert!(
        outcome.diagnostics.len() >= 3,
        "expected three independent errors, got {:?}",
        outcome.diagnostics
    );
}

#[test]
fn compile_when_corpus_programs_repeated_then_layout_hash_and_persistent_extent_equal() {
    assert_eq!(
        unstable(&programs(), false, |outcome| (
            outcome.layout_hash,
            outcome.extents.clone()
        )),
        Vec::<String>::new()
    );
}

/// A project of three files that depend on each other, each with its own
/// declarations, so that the order the files are merged in is visible in the
/// container.
const PROJECT: [(&str, &str); 3] = [
    (
        "types.st",
        "TYPE
  color : (red, green, blue);
  mode : (idle, running, stopped);
  point : STRUCT x : DINT; y : DINT; END_STRUCT;
END_TYPE",
    ),
    (
        "blocks.st",
        "FUNCTION_BLOCK zeta_counter
  VAR_INPUT up : BOOL; END_VAR
  VAR_OUTPUT q : DINT; END_VAR
  VAR n : DINT; END_VAR
  IF up THEN n := n + 1; END_IF;
  q := n;
END_FUNCTION_BLOCK

FUNCTION_BLOCK alpha_latch
  VAR_INPUT set : BOOL; END_VAR
  VAR_OUTPUT q : BOOL; END_VAR
  VAR held : BOOL; END_VAR
  IF set THEN held := TRUE; END_IF;
  q := held;
END_FUNCTION_BLOCK",
    ),
    (
        "main.st",
        "PROGRAM main
  VAR c : color := green; m : mode; p : point; z : zeta_counter; a : alpha_latch; END_VAR
  c := blue;
  m := running;
  p.x := 1;
  z(up := TRUE);
  a(set := TRUE);
END_PROGRAM",
    ),
];

/// The same project with an independent error in each file.
const PROJECT_WITH_ERRORS: [(&str, &str); 3] = [
    (
        "a.st",
        "FUNCTION_BLOCK one VAR x : INT; END_VAR undeclared_one := 1; END_FUNCTION_BLOCK",
    ),
    (
        "b.st",
        "FUNCTION_BLOCK two VAR y : no_such_type; END_VAR END_FUNCTION_BLOCK",
    ),
    (
        "c.st",
        "PROGRAM main VAR i : INT; END_VAR undeclared_three := 1; END_PROGRAM",
    ),
];

/// The files in the order given by `order`, which indexes into `files`.
fn in_order<'a>(files: &[(&'a str, &'a str)], order: [usize; 3]) -> Vec<(&'a str, &'a str)> {
    order.iter().map(|&index| files[index]).collect()
}

#[test]
fn compile_when_project_files_given_in_either_order_then_bytes_and_diagnostics_equal() {
    let forward = build(&in_order(&PROJECT, [0, 1, 2]), false);

    assert!(forward.bytes.is_some());
    assert!(build(&in_order(&PROJECT, [2, 1, 0]), false) == forward);
    assert!(build(&in_order(&PROJECT, [1, 2, 0]), false) == forward);
}

#[test]
fn compile_when_project_files_repeated_in_one_order_then_bytes_and_diagnostics_equal() {
    let files = in_order(&PROJECT, [0, 1, 2]);
    let first = build(&files, false);

    assert!((1..RUNS).all(|_| build(&files, false) == first));
}

#[test]
fn analyze_when_project_files_with_errors_given_in_either_order_then_diagnostics_equal() {
    let forward = build(&in_order(&PROJECT_WITH_ERRORS, [0, 1, 2]), false);

    assert!(forward.diagnostics.len() >= 3, "{:?}", forward.diagnostics);
    assert!(build(&in_order(&PROJECT_WITH_ERRORS, [2, 1, 0]), false) == forward);
    assert!(build(&in_order(&PROJECT_WITH_ERRORS, [1, 2, 0]), false) == forward);
}

#[test]
fn analyze_when_project_files_with_errors_repeated_then_diagnostics_equal() {
    let files = in_order(&PROJECT_WITH_ERRORS, [0, 1, 2]);
    let first = build(&files, false);

    assert!((1..RUNS).all(|_| build(&files, false) == first));
}
