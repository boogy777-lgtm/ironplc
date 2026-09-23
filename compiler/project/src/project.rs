//! Implements a project consisting of one or more files. A project
//! responds to messages (that is, the language server protocol).
//!
//! The trait enables easy testing of the language server protocol integration.

use std::path::Path;

use ironplc_analyzer::{stages::analyze, SemanticContext};
use ironplc_dsl::{
    common::Library,
    core::{FileId, SourceSpan},
    diagnostic::{Diagnostic, Label},
};
use ironplc_parser::{options::CompilerOptions, token::Token, tokenize_program};
use ironplc_problems::Problem;
use ironplc_sources::{FileType, LibraryName, Source, SourceProject};
use log::{debug, trace};

use crate::sidecar::{sidecar_path_for, Sidecar, SidecarKey};

/// Runs semantic analysis on the given source project and compiler options.
///
/// This is the shared implementation used by both [`FileBackedProject`] and
/// [`MemoryBackedProject`]. It parses each source into a library, merges them,
/// runs the analyzer, and returns the collected diagnostics (empty when the
/// project is clean) plus the semantic context (when type resolution
/// succeeds).
///
/// `preparsed_libraries` are compatibility libraries the caller already parsed,
/// injected alongside the ones the bundled registry loads. They exist for hosts
/// that cannot read the registry from disk -- the playground fetches its
/// library text over HTTP -- and are treated identically once loaded.
fn run_semantic_analysis(
    source_project: &mut SourceProject,
    compiler_options: &CompilerOptions,
    preparsed_libraries: &[Library],
) -> (Vec<Diagnostic>, Option<SemanticContext>, Option<Library>) {
    let mut all_libraries = vec![];
    let mut all_diagnostics: Vec<Diagnostic> = vec![];

    // Load the activated compatibility libraries first. Any that fail to load
    // (an unshipped name or malformed manifest) contribute a diagnostic but do
    // not prevent the rest of analysis. These declarations are injected ahead
    // of user source (base stdlib -> library -> user) and merge as ordinary
    // source: a user declaration of the same name in the same scope is a
    // duplicate, not an override (`REQ-CL-analyzer-007`).
    let (mut compat_libraries, compat_diagnostics) = source_project.load_activated_libraries();
    compat_libraries.extend(preparsed_libraries.iter().cloned());
    all_diagnostics.extend(compat_diagnostics);

    // Sources are backed by a HashMap, so iteration order is randomized
    // per-process by its hasher's random seed. Merging them in that order
    // made the combined Library's declaration order -- and therefore
    // semantic analysis's outcome for a given multi-file project -- vary
    // from run to run of the identical binary and identical input. Sort by
    // FileId first so the merged order (and every downstream result) is
    // deterministic.
    let mut sources = source_project.sources_mut();
    sources.sort_by_key(|source| source.file_id().to_string());

    let mut any_source_failed_to_parse = false;
    for source in sources {
        match source.library() {
            Ok(library) => {
                all_libraries.push(library);
            }
            Err(diagnostics) => {
                any_source_failed_to_parse = true;
                for diagnostic in diagnostics {
                    all_diagnostics.push(diagnostic.clone());
                }
            }
        }
    }

    // Nothing parsed, and parsing is why. Analysis of an empty set reports
    // `NoContent` (P9002), which is true but useless here: the syntax error
    // already collected says exactly what is wrong, and stacking an
    // internal-flavored code on top of it only adds noise. A project with no
    // sources at all still falls through to analysis, which is the case
    // `NoContent` exists to report. Partial failure is unaffected -- analysis
    // runs on whatever did parse.
    if all_libraries.is_empty() && any_source_failed_to_parse {
        return (all_diagnostics, None, None);
    }

    // Activation order: the compatibility libraries precede user source in the
    // merge (the base stdlib is seeded inside `analyze`).
    let analyze_input: Vec<&Library> = compat_libraries
        .iter()
        .chain(all_libraries.iter().copied())
        .collect();

    match analyze(&analyze_input, compiler_options) {
        Ok((library, context)) => {
            debug!("Semantic analysis completed {context:?}");
            all_diagnostics.extend(context.diagnostics().iter().cloned());
            (all_diagnostics, Some(context), Some(library))
        }
        Err(diagnostics) => {
            debug!("Semantic analysis errored {diagnostics:?}");
            all_diagnostics.extend(diagnostics);
            (all_diagnostics, None, None)
        }
    }
}

/// A project consisting of one or more files.
///
/// The project acts is akin to an interface for interacting with the compiler
/// for one or more files.
pub trait Project {
    /// Initialize
    fn initialize(&mut self, dir: &Path) -> Vec<Diagnostic>;

    /// Initialize from multiple directories, merging all discovered files
    /// into one compilation unit (unlike calling `initialize` once per
    /// directory, which would discard the previous directory's files).
    ///
    /// Default implementation delegates to `initialize` for a single
    /// directory, or does nothing for zero directories -- sufficient for
    /// implementors (e.g. `MemoryBackedProject`) that don't have a
    /// meaningful multi-directory story.
    fn initialize_many(&mut self, dirs: &[&Path]) -> Vec<Diagnostic> {
        match dirs {
            [] => vec![],
            [dir] => self.initialize(dir),
            _ => vec![Diagnostic::problem(
                Problem::NoContent,
                Label::span(
                    SourceSpan::default(),
                    "This project implementation does not support multiple directories",
                ),
            )],
        }
    }

    /// Updates the text for a document.
    fn change_text_document(&mut self, file_id: &FileId, content: String);

    /// Requests tokens for the file.
    fn tokenize(&self, file_id: &FileId) -> (Vec<Token>, Vec<Diagnostic>);

    /// Requests semantic analysis for the project.
    ///
    /// Analysis goes as far as it can and returns every diagnostic that
    /// parsing and analysis produced — an empty vector means the project is
    /// clean. The analyzed artifacts are cached and available through
    /// `semantic_context()` and `analyzed_library()`.
    fn semantic(&mut self) -> Vec<Diagnostic>;

    /// Engineering-side stable variable IDs, mapping a persistent
    /// declaration's or FB field's `(scope, name)` to its entity UID (ADR
    /// 0053; an FB field's scope is its qualified FB type name, ADR 0059),
    /// or an empty slice when the caller has none.
    /// [`crate::compile::compile`] splits the table with the library's
    /// declarations and passes both parts through to codegen, which emits
    /// the container's `stable_vars` and `fb_field_uids` tables.
    fn stable_var_ids(&self) -> &[(SidecarKey, u64)] {
        &[]
    }

    /// Gets the semantic context from the last analysis.
    ///
    /// Returns `Some` when the last call to `semantic()` succeeded in building
    /// type, function, and symbol environments — even if analysis reported
    /// validation diagnostics. Returns `None` only if `semantic()`
    /// has not been called or if foundational type resolution failed.
    fn semantic_context(&self) -> Option<&SemanticContext>;

    /// Gets the analyzed library from the last semantic analysis.
    ///
    /// Returns `Some` when the analyzer's type resolution phase succeeded,
    /// producing a merged and resolved library. Returns `None` if `semantic()`
    /// has not been called or if foundational type resolution failed.
    fn analyzed_library(&self) -> Option<&Library>;

    /// Gets the sources that are the project.
    fn sources(&self) -> Vec<&Source>;

    fn sources_mut(&mut self) -> Vec<&mut Source>;

    fn find(&self, file_id: &FileId) -> Option<&Source>;
}

/// A project is a collection of files used together as a single unit.
pub struct FileBackedProject {
    /// The underlying source project
    source_project: SourceProject,
    /// Parse options for this project
    compiler_options: CompilerOptions,
    /// Cached semantic context from the last successful analysis
    semantic_context: Option<SemanticContext>,
    /// Cached analyzed library from the last successful analysis
    analyzed_library: Option<Library>,
    /// Engineering-side stable variable IDs (ADR 0053); the keys also cover
    /// FB fields, whose scope is the qualified FB type name (ADR 0059).
    stable_var_ids: Vec<(SidecarKey, u64)>,
    /// The UID sidecar auto-loaded at initialization, when the
    /// initialization path yields one.
    sidecar_path: Option<std::path::PathBuf>,
}

impl Default for FileBackedProject {
    fn default() -> Self {
        Self::new()
    }
}

impl FileBackedProject {
    pub fn new() -> Self {
        FileBackedProject {
            source_project: SourceProject::new(),
            compiler_options: CompilerOptions::default(),
            semantic_context: None,
            analyzed_library: None,
            stable_var_ids: Vec::new(),
            sidecar_path: None,
        }
    }

    pub fn with_options(compiler_options: CompilerOptions) -> Self {
        FileBackedProject {
            source_project: SourceProject::with_options(compiler_options),
            compiler_options,
            semantic_context: None,
            analyzed_library: None,
            stable_var_ids: Vec::new(),
            sidecar_path: None,
        }
    }

    pub fn push(&mut self, file_id: FileId) -> Result<(), Diagnostic> {
        self.source_project.add_file(file_id)
    }

    pub fn get(&self, file_id: &FileId) -> Option<&Source> {
        self.source_project.get_source(file_id)
    }

    /// Activate the named compatibility libraries (replacing any current set).
    ///
    /// Activation is out of band — it never modifies source — and comes only
    /// from an explicit channel such as a `--library` request.
    pub fn set_activated_libraries(&mut self, names: Vec<LibraryName>) {
        self.source_project.set_activated_libraries(names);
    }

    /// Load the activated compatibility libraries from the bundled registry.
    ///
    /// Returns the parsed declarations to inject ahead of user source, plus one
    /// diagnostic per library that could not be loaded.
    pub fn load_activated_libraries(&self) -> (Vec<Library>, Vec<Diagnostic>) {
        self.source_project.load_activated_libraries()
    }

    /// Sets the engineering-side stable variable IDs, mapping each
    /// persistent declaration's or FB field's `(scope, name)` to its entity
    /// UID (ADR 0053, ADR 0059). Replaces any previous table.
    pub fn set_stable_var_ids(&mut self, stable_var_ids: Vec<(SidecarKey, u64)>) {
        self.stable_var_ids = stable_var_ids;
    }

    /// The UID sidecar auto-loaded at initialization, when the
    /// initialization path yielded one.
    pub fn sidecar_path(&self) -> Option<&Path> {
        self.sidecar_path.as_deref()
    }

    /// Loads the stable variable UID sidecar at `path` into the project,
    /// replacing the current stable variable IDs; an empty or malformed
    /// sidecar clears them. [`Project::initialize`] calls this automatically
    /// for the sidecar derived from the initialization path; callers that
    /// build the project by pushing individual files (the CLI) call this
    /// explicitly.
    pub fn load_uid_sidecar(&mut self, path: &Path) -> Result<(), Diagnostic> {
        let sidecar = Sidecar::load(path)?;
        self.sidecar_path = Some(path.to_path_buf());
        self.set_stable_var_ids(sidecar.keyed_entries());
        Ok(())
    }

    /// Loads the UID sidecar derived from an initialization path. A missing
    /// or malformed sidecar is an empty table, so only a genuine I/O failure
    /// surfaces as a diagnostic.
    fn auto_load_uid_sidecar(&mut self, project_path: &Path) -> Vec<Diagnostic> {
        let Some(path) = sidecar_path_for(project_path) else {
            return vec![];
        };
        match self.load_uid_sidecar(&path) {
            Ok(()) => vec![],
            Err(diagnostic) => vec![diagnostic],
        }
    }
}

impl Project for FileBackedProject {
    /// Create a new project from the files in the specified directory.
    fn initialize(&mut self, dir: &Path) -> Vec<Diagnostic> {
        let mut diagnostics = self.source_project.initialize_from_directory(dir);
        diagnostics.extend(self.auto_load_uid_sidecar(dir));
        diagnostics
    }

    /// Create a new project from the files in multiple directories,
    /// merged into one compilation unit.
    fn initialize_many(&mut self, dirs: &[&Path]) -> Vec<Diagnostic> {
        let mut diagnostics = self.source_project.initialize_from_directories(dirs);
        if let Some(first) = dirs.first() {
            diagnostics.extend(self.auto_load_uid_sidecar(first));
        }
        diagnostics
    }

    fn change_text_document(&mut self, file_id: &FileId, content: String) {
        trace!(
            "Change text document sources initial length is {}",
            self.source_project.len()
        );

        self.source_project.add_source(file_id.clone(), content);

        trace!(
            "Change text document sources new length is {}",
            self.source_project.len()
        );
    }

    fn tokenize(&self, file_id: &FileId) -> (Vec<Token>, Vec<Diagnostic>) {
        let source = self.source_project.get_source(file_id);

        match source {
            Some(src) => tokenize_program(src.as_string(), file_id, &self.compiler_options, 0, 0),
            None => (
                vec![],
                vec![Diagnostic::problem(
                    Problem::NoContent,
                    Label::span(SourceSpan::default(), "No documents to tokenize"),
                )],
            ),
        }
    }

    fn semantic(&mut self) -> Vec<Diagnostic> {
        self.semantic_context = None;
        self.analyzed_library = None;
        let (diagnostics, context, library) =
            run_semantic_analysis(&mut self.source_project, &self.compiler_options, &[]);
        self.semantic_context = context;
        self.analyzed_library = library;
        diagnostics
    }

    fn semantic_context(&self) -> Option<&SemanticContext> {
        self.semantic_context.as_ref()
    }

    fn analyzed_library(&self) -> Option<&Library> {
        self.analyzed_library.as_ref()
    }

    fn stable_var_ids(&self) -> &[(SidecarKey, u64)] {
        &self.stable_var_ids
    }

    fn sources(&self) -> Vec<&Source> {
        self.source_project.sources()
    }

    fn sources_mut(&mut self) -> Vec<&mut Source> {
        self.source_project.sources_mut()
    }

    fn find(&self, file_id: &FileId) -> Option<&Source> {
        self.source_project.get_source(file_id)
    }
}

/// An in-memory project that never touches the filesystem.
///
/// This is the project implementation for the MCP server and other contexts
/// where source text is supplied directly rather than read from disk.
pub struct MemoryBackedProject {
    /// The underlying source project
    source_project: SourceProject,
    /// Parse options for this project
    compiler_options: CompilerOptions,
    /// Cached semantic context from the last successful analysis
    semantic_context: Option<SemanticContext>,
    /// Cached analyzed library from the last successful analysis
    analyzed_library: Option<Library>,
    /// Compatibility libraries the caller parsed itself, injected ahead of
    /// user source alongside any the bundled registry loads.
    preparsed_libraries: Vec<Library>,
    /// Engineering-side stable variable IDs (ADR 0053).
    stable_var_ids: Vec<(SidecarKey, u64)>,
}

impl MemoryBackedProject {
    /// Creates a new empty in-memory project with the given compiler options.
    pub fn new(compiler_options: CompilerOptions) -> Self {
        MemoryBackedProject {
            source_project: SourceProject::with_options(compiler_options),
            compiler_options,
            semantic_context: None,
            analyzed_library: None,
            preparsed_libraries: Vec::new(),
            stable_var_ids: Vec::new(),
        }
    }

    /// Adds a source to the project by name and content.
    ///
    /// The `file_id` identifies the source in diagnostics. If a source with
    /// the same `file_id` already exists, it is replaced.
    pub fn add_source(&mut self, file_id: FileId, content: String) {
        self.source_project.add_source(file_id, content);
    }

    /// Adds a source whose file type is supplied rather than derived from the
    /// `file_id`'s extension.
    ///
    /// For content that never had a filename -- the playground editor's buffer
    /// -- where the caller detects the type from the content itself (see
    /// [`ironplc_sources::FileType::from_content`]).
    pub fn add_source_with_file_type(
        &mut self,
        file_id: FileId,
        content: String,
        file_type: FileType,
    ) {
        self.source_project
            .add_source_with_file_type(file_id, content, file_type);
    }

    /// Activate the named compatibility libraries (replacing any current set).
    pub fn set_activated_libraries(&mut self, names: Vec<LibraryName>) {
        self.source_project.set_activated_libraries(names);
    }

    /// Supply compatibility libraries the caller already parsed (replacing any
    /// current set).
    ///
    /// [`set_activated_libraries`](Self::set_activated_libraries) loads library
    /// text from the bundled registry on disk, which a wasm host cannot reach.
    /// The playground fetches that text over HTTP and parses it itself
    /// (`REQ-CL-playground-001`); the result arrives here. Both sets are
    /// injected ahead of user source, registry-loaded first.
    pub fn set_preparsed_libraries(&mut self, libraries: Vec<Library>) {
        self.preparsed_libraries = libraries;
    }

    /// Sets the engineering-side stable variable IDs, mapping each
    /// persistent declaration's or FB field's `(scope, name)` to its entity
    /// UID (ADR 0053, ADR 0059). Replaces any previous table.
    pub fn set_stable_var_ids(&mut self, stable_var_ids: Vec<(SidecarKey, u64)>) {
        self.stable_var_ids = stable_var_ids;
    }
}

impl Project for MemoryBackedProject {
    fn initialize(&mut self, _dir: &Path) -> Vec<Diagnostic> {
        vec![Diagnostic::problem(
            Problem::NoContent,
            Label::span(
                SourceSpan::default(),
                "MemoryBackedProject does not support directory initialization",
            ),
        )]
    }

    fn change_text_document(&mut self, file_id: &FileId, content: String) {
        self.source_project.add_source(file_id.clone(), content);
    }

    fn tokenize(&self, file_id: &FileId) -> (Vec<Token>, Vec<Diagnostic>) {
        let source = self.source_project.get_source(file_id);

        match source {
            Some(src) => tokenize_program(src.as_string(), file_id, &self.compiler_options, 0, 0),
            None => (
                vec![],
                vec![Diagnostic::problem(
                    Problem::NoContent,
                    Label::span(SourceSpan::default(), "No documents to tokenize"),
                )],
            ),
        }
    }

    fn semantic(&mut self) -> Vec<Diagnostic> {
        self.semantic_context = None;
        self.analyzed_library = None;
        let (diagnostics, context, library) = run_semantic_analysis(
            &mut self.source_project,
            &self.compiler_options,
            &self.preparsed_libraries,
        );
        self.semantic_context = context;
        self.analyzed_library = library;
        diagnostics
    }

    fn semantic_context(&self) -> Option<&SemanticContext> {
        self.semantic_context.as_ref()
    }

    fn analyzed_library(&self) -> Option<&Library> {
        self.analyzed_library.as_ref()
    }

    fn stable_var_ids(&self) -> &[(SidecarKey, u64)] {
        &self.stable_var_ids
    }

    fn sources(&self) -> Vec<&Source> {
        self.source_project.sources()
    }

    fn sources_mut(&mut self) -> Vec<&mut Source> {
        self.source_project.sources_mut()
    }

    fn find(&self, file_id: &FileId) -> Option<&Source> {
        self.source_project.get_source(file_id)
    }
}

#[cfg(test)]
mod test {
    use ironplc_dsl::core::FileId;
    use ironplc_parser::options::{CompilerOptions, Dialect};
    use std::path::Path;

    use super::{FileBackedProject, LibraryName, MemoryBackedProject, Problem, Project};

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
        let result = project.semantic();
        assert!(!result.is_empty());
    }

    #[test]
    fn semantic_when_tc2_system_activated_then_pi_resolves() {
        let mut project = MemoryBackedProject::new(library_options());
        project.set_activated_libraries(vec![LibraryName::from("Tc2_System")]);
        project.add_source(FileId::from_string("main.st"), PI_PROGRAM.to_owned());

        // Activating Tc2_System injects the global PI, so the initializer folds.
        let result = project.semantic();
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
        let result = project.semantic();
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

        let result = project.semantic();
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
        let result = project.semantic();
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
        let result = project.semantic();
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

        let result = project.semantic();
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
        let result = project.semantic();
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

        let result = project.semantic();
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

        let result = project.semantic();
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

        let result = project.semantic();
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

        let result = project.semantic();
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

        let result = project.semantic();
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

        let result = project.semantic();
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

        let result = project.semantic();
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

        let result = project.semantic();
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

        let result = project.semantic();

        assert!(!result.is_empty());
    }

    #[test]
    fn semantic_when_validation_error_then_context_cached() {
        let mut project = FileBackedProject::default();
        // Valid type declaration with an inverted subrange (semantic error)
        let content = "TYPE\nINVALID_RANGE : INT(10..-10);\nEND_TYPE";
        let file_id = FileId::from_string("test.st");
        project.change_text_document(&file_id, content.to_owned());

        let result = project.semantic();

        assert!(!result.is_empty());
        assert!(project.semantic_context().is_some());
    }

    // Regression test for a real non-determinism bug (see the GitHub issue
    // this fixes): sources used to be merged in HashMap iteration order,
    // which is randomized per-process, so the same multi-file project could
    // produce different (spuriously failing) semantic analysis results on
    // different runs of the identical binary. Sources are now sorted by
    // FileId before merging, so the combined library's element order --
    // and therefore the analysis result -- no longer depends on either
    // insertion order or hash-seed randomness.
    #[test]
    fn run_semantic_analysis_when_sources_inserted_out_of_order_then_merges_in_sorted_order() {
        use ironplc_dsl::common::LibraryElementKind;
        use ironplc_sources::SourceProject;

        fn element_names(source_project: &mut SourceProject) -> Vec<String> {
            let (_, _, library) =
                super::run_semantic_analysis(source_project, &CompilerOptions::default(), &[]);
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

        let result = project.semantic();
        assert!(result.is_empty());
    }

    #[test]
    fn memory_semantic_when_syntax_error_then_diagnostics() {
        let mut project = MemoryBackedProject::new(CompilerOptions::default());
        project.add_source(
            FileId::from_string("bad.st"),
            "PROGRAM END_PROGRAM".to_owned(),
        );

        let result = project.semantic();
        assert!(!result.is_empty());
    }

    #[test]
    fn memory_semantic_when_semantic_error_then_diagnostics_with_context() {
        let mut project = MemoryBackedProject::new(CompilerOptions::default());
        let content = "TYPE\nINVALID_RANGE : INT(10..-10);\nEND_TYPE";
        project.add_source(FileId::from_string("test.st"), content.to_owned());

        let result = project.semantic();
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
        let diagnostics =
            project.initialize_many(&[Path::new("/some/dir"), Path::new("/other/dir")]);

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
            crate::sidecar::sidecar_path_for(&dir).unwrap(),
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
        let sidecar_path = crate::sidecar::sidecar_path_for(&dir).unwrap();
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
            crate::sidecar::sidecar_path_for(&dir).unwrap(),
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

        let result = project.semantic();
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
        let result = project.semantic();
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

        let result = project.semantic();
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

        let result = project.semantic();
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

        let result = project.semantic();
        assert!(!result.is_empty());
        // Type resolution succeeds even with semantic diagnostics, so
        // the analyzed library should be available.
        assert!(project.analyzed_library().is_some());
    }
}
