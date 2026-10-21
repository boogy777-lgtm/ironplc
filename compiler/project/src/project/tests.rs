//! Tests for the file-backed and memory-backed projects: document changes, compatibility-library activation and analysis results.

use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::Area;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_problems::Problem;
use spec_test_macro::spec_test;
use std::path::Path;

use super::{FileBackedProject, LibraryName, MemoryBackedProject, Project};

#[test]
fn change_text_document_when_overwrite_then_one_file() {
    let mut project = FileBackedProject::default();
    project.change_text_document(&FileId::default(), "AAA".to_owned());
    project.change_text_document(&FileId::default(), "BBB".to_owned());
    assert_eq!(1, project.sources().len());
}

#[test]
fn compilation_set_when_empty_then_ok() {
    let project = FileBackedProject::default();
    assert_eq!(0, project.sources().len());
}

// -----------------------------------------------------------------
// Compatibility-library activation.
// -----------------------------------------------------------------

fn library_options() -> CompilerOptions {
    CompilerOptions {
        allow_top_level_var_global: true,
        allow_constant_initializer_expressions: true,
        ..CompilerOptions::default()
    }
}

const PI_PROGRAM: &str =
    "FUNCTION_BLOCK FB_Angle VAR d2r : LREAL := PI/180.0; END_VAR END_FUNCTION_BLOCK";

#[test]
fn semantic_when_library_not_activated_then_pi_undefined() {
    let mut project = MemoryBackedProject::new(library_options());
    project.add_source(FileId::from_string("main.st"), PI_PROGRAM.to_owned());

    // Dormant by default: PI does not resolve without activation.
    let result = project.semantic(Area::Build);
    assert!(!result.is_empty());
}

#[test]
fn semantic_when_tc2_system_activated_then_pi_resolves() {
    let mut project = MemoryBackedProject::new(library_options());
    project.set_activated_libraries(vec![LibraryName::from("Tc2_System")]);
    project.add_source(FileId::from_string("main.st"), PI_PROGRAM.to_owned());

    // Activating Tc2_System injects the global PI, so the initializer folds.
    let result = project.semantic(Area::Build);
    assert!(
        result.is_empty(),
        "expected clean analysis, got: {:?}",
        result
    );
}

/// End-to-end (Phase 2): activation comes *only* from a discovered
/// `.plcproj`'s library reference -- no `--library` flag and no source-level
/// directive -- yet `PI` resolves and the initializer folds.
#[test]
fn semantic_when_plcproj_references_tc2_system_then_pi_resolves() {
    use std::fs;
    use tempfile::TempDir;

    let dir = TempDir::new().unwrap();
    fs::write(dir.path().join("main.st"), PI_PROGRAM).unwrap();
    fs::write(
        dir.path().join("proj.plcproj"),
        r#"<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Compile Include="main.st" />
    <PlaceholderReference Include="Tc2_System">
      <DefaultResolution>Tc2_System, * (Beckhoff Automation GmbH)</DefaultResolution>
      <Namespace>Tc2_System</Namespace>
    </PlaceholderReference>
  </ItemGroup>
</Project>"#,
    )
    .unwrap();

    let mut project = FileBackedProject::with_options(library_options());
    let errors = project.initialize(dir.path());
    assert!(errors.is_empty(), "unexpected discovery errors: {errors:?}");

    // The .plcproj reference alone activated Tc2_System.
    let result = project.semantic(Area::Build);
    assert!(
        result.is_empty(),
        "expected clean analysis, got: {:?}",
        result
    );
}

const BOOL_TO_STRING_PROGRAM: &str =
    "PROGRAM main VAR s : STRING; END_VAR s := BOOL_TO_STRING(TRUE); END_PROGRAM";

/// Implicit activation (`REQ-CL-sources-008`): a `.plcproj` with **no**
/// library references still activates the implicit `Tc2_BuiltIns`, so
/// `BOOL_TO_STRING` resolves -- mirroring TwinCAT, where the built-in
/// conversion operators exist in every project.
#[test]
fn semantic_when_plcproj_has_no_references_then_bool_to_string_resolves() {
    use std::fs;
    use tempfile::TempDir;

    let dir = TempDir::new().unwrap();
    fs::write(dir.path().join("main.st"), BOOL_TO_STRING_PROGRAM).unwrap();
    fs::write(
        dir.path().join("proj.plcproj"),
        r#"<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Compile Include="main.st" />
  </ItemGroup>
</Project>"#,
    )
    .unwrap();

    let mut project = FileBackedProject::with_options(library_options());
    let errors = project.initialize(dir.path());
    assert!(errors.is_empty(), "unexpected discovery errors: {errors:?}");

    let result = project.semantic(Area::Build);
    assert!(
        result.is_empty(),
        "expected clean analysis, got: {:?}",
        result
    );
}

#[test]
fn semantic_when_bare_source_then_bool_to_string_undefined() {
    let mut project = MemoryBackedProject::new(library_options());
    project.add_source(
        FileId::from_string("main.st"),
        BOOL_TO_STRING_PROGRAM.to_owned(),
    );

    // Dormant by default: no project context, no implicit activation.
    let result = project.semantic(Area::Build);
    assert!(!result.is_empty());
}

#[test]
fn semantic_when_tc2_builtins_activated_explicitly_then_bool_to_string_resolves() {
    let mut project = MemoryBackedProject::new(library_options());
    project.set_activated_libraries(vec![LibraryName::from("Tc2_BuiltIns")]);
    project.add_source(
        FileId::from_string("main.st"),
        BOOL_TO_STRING_PROGRAM.to_owned(),
    );

    // The CLI `--library Tc2_BuiltIns` path: explicit activation works for
    // source with no project context.
    let result = project.semantic(Area::Build);
    assert!(
        result.is_empty(),
        "expected clean analysis, got: {:?}",
        result
    );
}

const TC2_MATH_PROGRAM: &str =
    "PROGRAM main VAR a : LREAL; b : LREAL; c : LREAL; d : LREAL; END_VAR \
         a := LTRUNC(2.8); b := LMOD(400.56, 360.0); c := MODABS(-400.56, 360.0); d := FRAC(2.8); \
         END_PROGRAM";

#[test]
fn semantic_when_tc2_math_activated_then_all_four_functions_resolve() {
    let mut project = MemoryBackedProject::new(library_options());
    project.set_activated_libraries(vec![LibraryName::from("Tc2_Math")]);
    project.add_source(FileId::from_string("main.st"), TC2_MATH_PROGRAM.to_owned());

    let result = project.semantic(Area::Build);
    assert!(
        result.is_empty(),
        "expected clean analysis, got: {:?}",
        result
    );
}

#[test]
fn semantic_when_bare_source_then_tc2_math_functions_undefined() {
    let mut project = MemoryBackedProject::new(library_options());
    project.add_source(FileId::from_string("main.st"), TC2_MATH_PROGRAM.to_owned());

    // Reference-activated only: dormant with no project context and no
    // explicit activation.
    let result = project.semantic(Area::Build);
    assert!(!result.is_empty());
}

/// `REQ-CL-analyzer-007`: a user-defined function named `LTRUNC` is a
/// duplicate of the activated library's, not an override of it.
#[test]
fn semantic_when_user_function_named_like_library_function_then_duplicate() {
    let mut project = MemoryBackedProject::new(library_options());
    project.set_activated_libraries(vec![LibraryName::from("Tc2_Math")]);
    project.add_source(
        FileId::from_string("main.st"),
        "FUNCTION LTRUNC : LREAL VAR_INPUT IN : LREAL; END_VAR LTRUNC := 123.0; END_FUNCTION \
             PROGRAM main VAR a : LREAL; END_VAR a := LTRUNC(2.8); END_PROGRAM"
            .to_owned(),
    );

    let result = project.semantic(Area::Build);
    assert!(
        result
            .iter()
            .any(|d| d.code == Problem::FunctionDeclNameDuplicated.code()),
        "expected P4016 for the duplicated library function, got: {result:?}"
    );
}

#[test]
fn semantic_when_unshipped_library_activated_then_diagnostic() {
    let mut project = MemoryBackedProject::new(library_options());
    project.set_activated_libraries(vec![LibraryName::from("DoesNotExist")]);
    project.add_source(
        FileId::from_string("main.st"),
        "FUNCTION_BLOCK FB VAR x : INT; END_VAR END_FUNCTION_BLOCK".to_owned(),
    );

    let result = project.semantic(Area::Build);
    let diagnostics = result;
    assert!(
        diagnostics.iter().any(|d| d.code == "P6011"),
        "expected P6011 naming the missing library, got: {diagnostics:?}"
    );
}

// -----------------------------------------------------------------
// Pre-parsed compatibility libraries (the playground's path: library
// text arrives over HTTP, so the on-disk registry is unreachable).
// -----------------------------------------------------------------

const LIB_FUNCTION: &str =
    "FUNCTION LIB_DOUBLE : LREAL VAR_INPUT IN : LREAL; END_VAR LIB_DOUBLE := IN * 2.0; \
         END_FUNCTION";
const CALLS_LIB_FUNCTION: &str =
    "PROGRAM main VAR a : LREAL; END_VAR a := LIB_DOUBLE(2.0); END_PROGRAM";

fn parse_library(source: &str) -> ironplc_dsl::common::Library {
    ironplc_sources::parse_source(
        ironplc_sources::FileType::StructuredText,
        source,
        &FileId::from_string("lib.st"),
        &CompilerOptions::default(),
    )
    .expect("fixture library must parse")
}

#[test]
fn semantic_when_preparsed_library_supplied_then_its_function_resolves() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    project.set_preparsed_libraries(vec![parse_library(LIB_FUNCTION)]);
    project.add_source(
        FileId::from_string("main.st"),
        CALLS_LIB_FUNCTION.to_owned(),
    );

    let result = project.semantic(Area::Build);
    assert!(
        result.is_empty(),
        "expected clean analysis, got: {:?}",
        result
    );
}

#[test]
fn semantic_when_no_preparsed_library_then_its_function_undefined() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    project.add_source(
        FileId::from_string("main.st"),
        CALLS_LIB_FUNCTION.to_owned(),
    );

    let result = project.semantic(Area::Build);
    assert!(!result.is_empty());
}

/// `REQ-CL-analyzer-007` holds for pre-parsed libraries too: the
/// playground's path merges them exactly as the CLI merges bundled ones,
/// so a redeclared library function is a duplicate in both.
#[test]
fn semantic_when_user_function_named_like_preparsed_library_function_then_duplicate() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    project.set_preparsed_libraries(vec![parse_library(LIB_FUNCTION)]);
    project.add_source(
        FileId::from_string("main.st"),
        format!("{LIB_FUNCTION} {CALLS_LIB_FUNCTION}"),
    );

    let result = project.semantic(Area::Build);
    assert!(
        result
            .iter()
            .any(|d| d.code == Problem::FunctionDeclNameDuplicated.code()),
        "expected P4016 for the duplicated library function, got: {result:?}"
    );
}

// -----------------------------------------------------------------
// What analysis does when parsing produced nothing to analyze.
// -----------------------------------------------------------------

/// A syntax error is a complete explanation on its own. Analysis of the
/// empty set would add `NoContent` (P9002) on top of it -- an
/// internal-flavored code that tells the user nothing they don't already
/// know from the syntax error -- so analysis is skipped instead.
#[test]
fn semantic_when_every_source_fails_to_parse_then_no_content_not_reported() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    project.add_source(
        FileId::from_string("bad.st"),
        "PROGRAM END_PROGRAM".to_owned(),
    );

    let result = project.semantic(Area::Build);
    assert!(!result.is_empty());
    assert!(
        !result.iter().any(|d| d.code == "P9002"),
        "the syntax error explains itself; P9002 is noise: {result:?}"
    );
}

/// The case P9002 exists for: nothing to analyze, and no parse failure to
/// explain why.
#[test]
fn semantic_when_no_sources_then_no_content_reported() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());

    let result = project.semantic(Area::Build);
    assert!(
        result.iter().any(|d| d.code == "P9002"),
        "expected P9002 for an empty project, got: {result:?}"
    );
}

/// Partial failure is unaffected: whatever parsed is still analyzed, so a
/// real problem in the good file surfaces alongside the syntax error.
#[test]
fn semantic_when_one_of_two_sources_fails_to_parse_then_other_still_analyzed() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    project.add_source(
        FileId::from_string("bad.st"),
        "PROGRAM END_PROGRAM".to_owned(),
    );
    project.add_source(
        FileId::from_string("good.st"),
        "PROGRAM main VAR x : INT; END_VAR x := undeclared_var; END_PROGRAM".to_owned(),
    );

    let result = project.semantic(Area::Build);
    assert!(
        result.len() >= 2,
        "expected the syntax error AND the semantic error, got: {result:?}"
    );
    assert!(project.analyzed_library().is_some());
}

#[test]
fn tokenize_when_has_other_file_then_error() {
    let mut project = FileBackedProject::default();
    project.change_text_document(&FileId::default(), "AAA".to_owned());
    let res = project.tokenize(&FileId::from_string("abc"));
    assert!(!res.1.is_empty());
}

#[test]
fn semantic_when_source_is_not_valid_then_returns_diagnostics() {
    let mut project = FileBackedProject::default();
    project.change_text_document(&FileId::default(), "AAA".to_owned());

    let result = project.semantic(Area::Build);

    assert!(!result.is_empty());
}

#[test]
fn semantic_when_validation_error_then_context_cached() {
    let mut project = FileBackedProject::default();
    // Valid type declaration with an inverted subrange (semantic error)
    let content = "TYPE\nINVALID_RANGE : INT(10..-10);\nEND_TYPE";
    let file_id = FileId::from_string("test.st");
    project.change_text_document(&file_id, content.to_owned());

    let result = project.semantic(Area::Build);

    assert!(!result.is_empty());
    assert!(project.semantic_context().is_some());
}

// Regression test for a real non-determinism bug (see the GitHub issue
// this fixes): sources used to be merged in HashMap iteration order,
// which is randomized per-process, so the same multi-file project could
// produce different (spuriously failing) semantic analysis results on
// different runs of the identical binary. The project now yields its
// sources ascending by FileId, so the combined library's element order --
// and therefore the analysis result -- no longer depends on either
// insertion order or hash-seed randomness.
#[test]
fn run_semantic_analysis_when_sources_inserted_out_of_order_then_merges_in_sorted_order() {
    use ironplc_dsl::common::LibraryElementKind;
    use ironplc_sources::SourceProject;

    fn element_names(source_project: &mut SourceProject) -> Vec<String> {
        let (_, _, library) = super::run_semantic_analysis(
            source_project,
            &CompilerOptions::default(),
            &[],
            Area::Build,
        );
        let library = library.unwrap();
        library
            .elements
            .iter()
            .map(|element| {
                assert!(
                    matches!(element, LibraryElementKind::FunctionBlockDeclaration(_)),
                    "unexpected element: {element:?}"
                );
                let LibraryElementKind::FunctionBlockDeclaration(fb) = element else {
                    return String::new();
                };
                fb.name.to_string()
            })
            .collect()
    }

    // Two otherwise-identical projects, differing only in the order the
    // same three files were inserted. Independent declarations (no
    // reference between them) have no required relative order, but the
    // merge must still be a deterministic function of FileId, not of
    // insertion order or (pre-fix) HashMap hash-seed randomness -- so
    // both must produce the exact same result.
    let mut forward = SourceProject::new();
    forward.add_source(
        FileId::from_string("a_file.st"),
        "FUNCTION_BLOCK FB_A\nEND_FUNCTION_BLOCK".to_owned(),
    );
    forward.add_source(
        FileId::from_string("m_file.st"),
        "FUNCTION_BLOCK FB_M\nEND_FUNCTION_BLOCK".to_owned(),
    );
    forward.add_source(
        FileId::from_string("z_file.st"),
        "FUNCTION_BLOCK FB_Z\nEND_FUNCTION_BLOCK".to_owned(),
    );

    let mut reverse = SourceProject::new();
    reverse.add_source(
        FileId::from_string("z_file.st"),
        "FUNCTION_BLOCK FB_Z\nEND_FUNCTION_BLOCK".to_owned(),
    );
    reverse.add_source(
        FileId::from_string("m_file.st"),
        "FUNCTION_BLOCK FB_M\nEND_FUNCTION_BLOCK".to_owned(),
    );
    reverse.add_source(
        FileId::from_string("a_file.st"),
        "FUNCTION_BLOCK FB_A\nEND_FUNCTION_BLOCK".to_owned(),
    );

    assert_eq!(element_names(&mut forward), element_names(&mut reverse));
}

// XML source handling (empty-library, parse errors) is owned and tested by
// `ironplc_sources::source`; this crate only routes file content there.

// MemoryBackedProject tests

#[test]
fn memory_add_source_when_valid_then_source_available() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    let file_id = FileId::from_string("main.st");
    project.add_source(file_id.clone(), "PROGRAM Main END_PROGRAM".to_owned());

    assert_eq!(1, project.sources().len());
    assert!(project.find(&file_id).is_some());
}

#[test]
fn memory_add_source_when_overwrite_then_one_source() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    let file_id = FileId::from_string("main.st");
    project.add_source(file_id.clone(), "AAA".to_owned());
    project.add_source(file_id, "BBB".to_owned());

    assert_eq!(1, project.sources().len());
}

#[test]
fn memory_semantic_when_valid_program_then_ok() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    let content = r#"
PROGRAM Main
VAR
  x : INT;
END_VAR
  x := 1;
END_PROGRAM

CONFIGURATION config
  RESOURCE resource1 ON PLC
    TASK plc_task(INTERVAL := T#100ms, PRIORITY := 1);
    PROGRAM program1 WITH plc_task : Main;
  END_RESOURCE
END_CONFIGURATION
"#;
    project.add_source(FileId::from_string("main.st"), content.to_owned());

    let result = project.semantic(Area::Build);
    assert!(result.is_empty());
}

#[test]
fn memory_semantic_when_syntax_error_then_diagnostics() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    project.add_source(
        FileId::from_string("bad.st"),
        "PROGRAM END_PROGRAM".to_owned(),
    );

    let result = project.semantic(Area::Build);
    assert!(!result.is_empty());
}

#[test]
fn memory_semantic_when_semantic_error_then_diagnostics_with_context() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    let content = "TYPE\nINVALID_RANGE : INT(10..-10);\nEND_TYPE";
    project.add_source(FileId::from_string("test.st"), content.to_owned());

    let result = project.semantic(Area::Build);
    assert!(!result.is_empty());
    assert!(project.semantic_context().is_some());
}

#[test]
fn memory_tokenize_when_valid_then_tokens() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    let file_id = FileId::from_string("main.st");
    project.add_source(file_id.clone(), "PROGRAM Main END_PROGRAM".to_owned());

    let (tokens, diagnostics) = project.tokenize(&file_id);
    assert!(!tokens.is_empty());
    assert!(diagnostics.is_empty());
}

#[test]
fn memory_tokenize_when_missing_file_then_error() {
    let project = MemoryBackedProject::new(CompilerOptions::default());
    let (tokens, diagnostics) = project.tokenize(&FileId::from_string("missing.st"));

    assert!(tokens.is_empty());
    assert!(!diagnostics.is_empty());
}

#[test]
fn memory_initialize_when_called_then_returns_error() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    let diagnostics = project.initialize(Path::new("/some/dir"));

    assert!(!diagnostics.is_empty());
}

// -----------------------------------------------------------------
// Multi-directory initialization.
// -----------------------------------------------------------------

// Multi-directory merge semantics (clearing once, per-directory failures,
// zero directories) are owned and tested by
// `ironplc_sources::project::initialize_from_directories`; this test only
// proves `initialize_many` wires through to it.
#[test]
fn file_backed_initialize_many_when_two_directories_then_merges_both() {
    use std::fs;
    use tempfile::TempDir;

    let dir1 = TempDir::new().unwrap();
    fs::write(dir1.path().join("a.st"), "PROGRAM A\nEND_PROGRAM").unwrap();

    let dir2 = TempDir::new().unwrap();
    fs::write(dir2.path().join("b.st"), "PROGRAM B\nEND_PROGRAM").unwrap();

    let mut project = FileBackedProject::default();
    let diagnostics = project.initialize_many(&[dir1.path(), dir2.path()]);

    assert!(diagnostics.is_empty());
    assert_eq!(project.sources().len(), 2);
}

#[test]
fn memory_initialize_many_when_multiple_directories_then_returns_error() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    let diagnostics = project.initialize_many(&[Path::new("/some/dir"), Path::new("/other/dir")]);

    assert!(!diagnostics.is_empty());
}

// -----------------------------------------------------------------
// UID sidecar auto-load (ADR 0053).
// -----------------------------------------------------------------

/// A program with two variables, used by the sidecar auto-load tests.
const TWO_VAR_PROGRAM: &str = "PROGRAM main VAR x : INT; y : INT; END_VAR END_PROGRAM";

/// A sidecar holding UIDs for the two variables of TWO_VAR_PROGRAM.
const TWO_VAR_SIDECAR: &str = r#"{
  "version": 1,
  "variables": [
    { "scope": "main", "name": "x", "uid": 42 },
    { "scope": "main", "name": "y", "uid": 43 }
  ]
}"#;

/// Creates `name` as a subdirectory of a temporary directory holding
/// `TWO_VAR_PROGRAM` as main.st, returning both.
fn project_dir_with_program(temp: &tempfile::TempDir, name: &str) -> std::path::PathBuf {
    let dir = temp.path().join(name);
    std::fs::create_dir(&dir).unwrap();
    std::fs::write(dir.join("main.st"), TWO_VAR_PROGRAM).unwrap();
    dir
}

#[test]
fn initialize_when_sidecar_present_then_stable_var_ids_loaded() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = project_dir_with_program(&temp, "proj");
    std::fs::write(
        crate::records::RecordKind::Uids.path(&dir).unwrap(),
        TWO_VAR_SIDECAR,
    )
    .unwrap();

    let mut project = FileBackedProject::default();
    let diagnostics = project.initialize(&dir);

    assert!(
        diagnostics.is_empty(),
        "unexpected diagnostics: {diagnostics:?}"
    );
    assert_eq!(
        project.stable_var_ids(),
        [
            (crate::sidecar::SidecarKey::new("main", "x"), 42),
            (crate::sidecar::SidecarKey::new("main", "y"), 43)
        ]
    );
}

#[test]
fn initialize_when_sidecar_present_then_sidecar_path_exposed() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = project_dir_with_program(&temp, "proj");
    let sidecar_path = crate::records::RecordKind::Uids.path(&dir).unwrap();
    std::fs::write(&sidecar_path, TWO_VAR_SIDECAR).unwrap();

    let mut project = FileBackedProject::default();
    project.initialize(&dir);

    assert_eq!(project.sidecar_path(), Some(sidecar_path.as_path()));
}

#[test]
fn initialize_when_sidecar_missing_then_stable_var_ids_empty() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = project_dir_with_program(&temp, "proj");

    let mut project = FileBackedProject::default();
    let diagnostics = project.initialize(&dir);

    assert!(
        diagnostics.is_empty(),
        "unexpected diagnostics: {diagnostics:?}"
    );
    assert!(project.stable_var_ids().is_empty());
}

#[test]
fn initialize_when_sidecar_malformed_then_stable_var_ids_empty() {
    let temp = tempfile::TempDir::new().unwrap();
    let dir = project_dir_with_program(&temp, "proj");
    std::fs::write(
        crate::records::RecordKind::Uids.path(&dir).unwrap(),
        "this is not a sidecar",
    )
    .unwrap();

    let mut project = FileBackedProject::default();
    let diagnostics = project.initialize(&dir);

    assert!(
        diagnostics.is_empty(),
        "unexpected diagnostics: {diagnostics:?}"
    );
    assert!(project.stable_var_ids().is_empty());
}

#[test]
fn memory_change_text_document_when_called_then_adds_source() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    let file_id = FileId::from_string("main.st");
    project.change_text_document(&file_id, "PROGRAM Main END_PROGRAM".to_owned());

    assert_eq!(1, project.sources().len());
}

#[test]
fn memory_semantic_when_with_dialect_then_uses_options() {
    let options = CompilerOptions::from_dialect(Dialect::Rusty);
    let mut project = MemoryBackedProject::new(options);
    let content = r#"
// C-style comment (allowed in Rusty dialect)
PROGRAM Main
VAR
  x : INT;
END_VAR
  x := 1;
END_PROGRAM

CONFIGURATION config
  RESOURCE resource1 ON PLC
    TASK plc_task(INTERVAL := T#100ms, PRIORITY := 1);
    PROGRAM program1 WITH plc_task : Main;
  END_RESOURCE
END_CONFIGURATION
"#;
    project.add_source(FileId::from_string("main.st"), content.to_owned());

    let result = project.semantic(Area::Build);
    assert!(result.is_empty());
}

#[test]
fn memory_semantic_when_multiple_sources_then_analyzes_together() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());

    let fb_content = r#"
FUNCTION_BLOCK Counter
VAR
  count : INT;
END_VAR
  count := count + 1;
END_FUNCTION_BLOCK
"#;
    let program_content = r#"
PROGRAM Main
VAR
  c : Counter;
END_VAR
END_PROGRAM

CONFIGURATION config
  RESOURCE resource1 ON PLC
    TASK plc_task(INTERVAL := T#100ms, PRIORITY := 1);
    PROGRAM program1 WITH plc_task : Main;
  END_RESOURCE
END_CONFIGURATION
"#;
    project.add_source(FileId::from_string("counter.st"), fb_content.to_owned());
    project.add_source(FileId::from_string("main.st"), program_content.to_owned());

    // Counter FB from counter.st should be visible to main.st
    let result = project.semantic(Area::Build);
    assert!(
        result.is_empty(),
        "Expected no diagnostics, got: {:?}",
        result
    );
}

#[test]
fn memory_semantic_context_when_no_analysis_then_none() {
    let project = MemoryBackedProject::new(CompilerOptions::default());
    assert!(project.semantic_context().is_none());
}

#[test]
fn memory_analyzed_library_when_no_analysis_then_none() {
    let project = MemoryBackedProject::new(CompilerOptions::default());
    assert!(project.analyzed_library().is_none());
}

#[test]
fn memory_semantic_when_valid_program_then_analyzed_library_available() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    let content = r#"
PROGRAM Main
VAR
  x : INT;
END_VAR
  x := 1;
END_PROGRAM

CONFIGURATION config
  RESOURCE resource1 ON PLC
    TASK plc_task(INTERVAL := T#100ms, PRIORITY := 1);
    PROGRAM program1 WITH plc_task : Main;
  END_RESOURCE
END_CONFIGURATION
"#;
    project.add_source(FileId::from_string("main.st"), content.to_owned());

    let result = project.semantic(Area::Build);
    assert!(result.is_empty());
    assert!(project.analyzed_library().is_some());
}

#[test]
fn memory_semantic_when_syntax_error_then_analyzed_library_none() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    project.add_source(
        FileId::from_string("bad.st"),
        "PROGRAM END_PROGRAM".to_owned(),
    );

    let result = project.semantic(Area::Build);
    assert!(!result.is_empty());
    // Foundational analysis fails when parse produces no valid library elements,
    // so analyzed_library should be None.
    assert!(project.analyzed_library().is_none());
}

#[test]
fn memory_semantic_when_semantic_error_then_analyzed_library_available() {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    let content = "TYPE\nINVALID_RANGE : INT(10..-10);\nEND_TYPE";
    project.add_source(FileId::from_string("test.st"), content.to_owned());

    let result = project.semantic(Area::Build);
    assert!(!result.is_empty());
    // Type resolution succeeds even with semantic diagnostics, so
    // the analyzed library should be available.
    assert!(project.analyzed_library().is_some());
}

// -----------------------------------------------------------------
// The name of the application a build is for (REQ-DS-project-005).
// -----------------------------------------------------------------

/// A program and two configurations, `Zeta` written before `Alpha`.
const TWO_CONFIGURATIONS: &str = "
PROGRAM main
  VAR x : INT; END_VAR
  x := 1;
END_PROGRAM

CONFIGURATION Zeta
  RESOURCE r ON PLC
    TASK t(INTERVAL := T#10ms, PRIORITY := 1);
    PROGRAM i1 WITH t : main;
  END_RESOURCE
END_CONFIGURATION

CONFIGURATION Alpha
  RESOURCE r ON PLC
    TASK t(INTERVAL := T#10ms, PRIORITY := 1);
    PROGRAM i1 WITH t : main;
  END_RESOURCE
END_CONFIGURATION";

fn analyzed(text: &str) -> MemoryBackedProject {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    project.add_source(FileId::from_string("plant.st"), text.to_owned());
    project.semantic(Area::Build);
    project
}

#[spec_test(REQ_DS_project_005)]
fn application_name_when_one_configuration_then_its_name_not_the_fallback() {
    let project =
        analyzed(&TWO_CONFIGURATIONS[..TWO_CONFIGURATIONS.find("CONFIGURATION Alpha").unwrap()]);

    assert_eq!(project.application_name("plant"), "Zeta");
}

#[spec_test(REQ_DS_project_005)]
fn application_name_when_several_configurations_then_the_first_written() {
    let project = analyzed(TWO_CONFIGURATIONS);

    assert_eq!(project.application_name("plant"), "Zeta");
}

#[spec_test(REQ_DS_project_005)]
fn application_name_when_no_configuration_then_the_fallback() {
    let project = analyzed("PROGRAM main END_PROGRAM");

    assert_eq!(project.application_name("plant"), "plant");
}

#[spec_test(REQ_DS_project_005)]
fn application_name_when_no_library_then_the_fallback() {
    let project = analyzed("PROGRAM END_PROGRAM");

    assert!(project.analyzed_library().is_none());
    assert_eq!(project.application_name("plant"), "plant");
}
