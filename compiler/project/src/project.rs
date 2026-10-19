//! Implements a project consisting of one or more files. A project
//! responds to messages (that is, the language server protocol).
//!
//! The trait enables easy testing of the language server protocol integration.

use std::path::Path;

use ironplc_analyzer::{stages::analyze, SemanticContext};
use ironplc_dsl::{
    common::Library,
    core::{FileId, SourceSpan},
    diagnostic::{kept, Diagnostic},
};
use ironplc_parser::{options::CompilerOptions, token::Token, tokenize_program};
use ironplc_problems::text;
use ironplc_sources::{FileType, LibraryName, Source, SourceProject};
use log::{debug, trace};

use crate::sidecar::{sidecar_path_for, Sidecar, SidecarKey};

/// Runs semantic analysis on the given source project and compiler options.
///
/// This is the shared implementation used by both [`FileBackedProject`] and
/// [`MemoryBackedProject`]. It parses each source into a library, merges them,
/// runs the analyzer, and returns the diagnostics the run keeps (empty when the
/// project is clean) plus the semantic context (when type resolution
/// succeeds). What a run keeps -- no repeats, at most 500 errors and 500
/// warnings -- is decided by [`kept`], so every consumer of the analysis (the
/// command line, the language server, the MCP tools) gets the same messages.
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
    let (diagnostics, context, library) =
        analyze_sources(source_project, compiler_options, preparsed_libraries);
    (kept(diagnostics), context, library)
}

/// Parses and analyzes the sources and returns every diagnostic they produce.
fn analyze_sources(
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

    // The project yields its sources ascending by file ID, a function of the
    // set of files alone, so the combined Library's declaration order -- and
    // with it the outcome of analysis -- is the same on every run.
    let mut any_source_failed_to_parse = false;
    for source in source_project.sources_mut() {
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
            _ => vec![Diagnostic::text(
                SourceSpan::default(),
                text::no_content_multiple_directories_unsupported(),
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
                vec![Diagnostic::text(
                    SourceSpan::default(),
                    text::no_content_no_documents_to_tokenize(),
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
        vec![Diagnostic::text(
            SourceSpan::default(),
            text::no_content_directory_initialization_unsupported(),
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
                vec![Diagnostic::text(
                    SourceSpan::default(),
                    text::no_content_no_documents_to_tokenize(),
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
mod tests;
