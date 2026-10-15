//! Implements the command line behavior.

use codespan_reporting::{
    diagnostic::{Diagnostic as CodeSpanDiagnostic, Label as CodeSpanLabel, LabelStyle, Severity},
    files::SimpleFiles,
    term::{
        self,
        termcolor::{ColorChoice, StandardStream},
    },
};
use ironplc_dsl::{
    core::FileId,
    diagnostic::{Diagnostic, Label},
};
use ironplc_plc2plc::write_to_string;
use ironplc_problems::Problem;
use log::{error, trace};
use std::{
    collections::{BTreeSet, HashMap},
    fs::{canonicalize, metadata},
    ops::Range,
    path::{Path, PathBuf},
};

use ironplc_parser::options::CompilerOptions;
use ironplc_project::sidecar::{declared_var_keys, sidecar_path_for, Sidecar, SidecarKey};
use ironplc_sources::discovery::DiscoveredProject;
use ironplc_sources::LibraryName;

use ironplc_project::tokenizer;
use ironplc_project::{FileBackedProject, Project};

// Checks specified files.
pub fn check(
    paths: &[PathBuf],
    compiler_options: CompilerOptions,
    libraries: &[LibraryName],
    suppress_output: bool,
) -> Result<(), String> {
    let (mut project, mut diagnostics) = create_project(paths, compiler_options, libraries);

    // Analyze the set
    diagnostics.extend(project.semantic());

    finish("Check", diagnostics, Some(&project), suppress_output)
}

pub fn echo(
    paths: &[PathBuf],
    compiler_options: CompilerOptions,
    suppress_output: bool,
) -> Result<(), String> {
    // Echo renders parsed source; it runs no analysis, so no library activation.
    let (mut project, mut diagnostics) = create_project(paths, compiler_options, &[]);

    // Collect the results and output after because getting the results may change
    // the project itself
    let mut results = vec![];
    for src in project.sources_mut() {
        results.push(src.library());
    }

    for result in results {
        match result {
            Ok(library) => match write_to_string(library) {
                Ok(output) => print!("{output}"),
                Err(errs) => diagnostics.extend(errs),
            },
            Err(errs) => diagnostics.extend(errs),
        }
    }

    finish("Echo", diagnostics, Some(&project), suppress_output)
}

pub fn tokenize(
    paths: &[PathBuf],
    compiler_options: CompilerOptions,
    suppress_output: bool,
) -> Result<(), String> {
    // Tokenize only lexes each source; library activation is irrelevant here.
    let (project, mut diagnostics) = create_project(paths, compiler_options, &[]);

    for src in project.sources() {
        diagnostics.extend(tokenizer::tokenize_source(src));
    }

    finish("Tokenize", diagnostics, Some(&project), suppress_output)
}

/// Compiles source files into a bytecode container (.iplc) file.
///
/// Parses the source files, runs full analysis (type resolution + semantic
/// checks), and generates bytecode.
pub fn compile(
    paths: &[PathBuf],
    output: &Path,
    compiler_options: CompilerOptions,
    libraries: &[LibraryName],
    suppress_output: bool,
) -> Result<(), String> {
    let (mut project, mut diagnostics) = create_project(paths, compiler_options, libraries);

    // Refuse to write the container over a loaded source file. `File::create`
    // truncates immediately, so this must be diagnosed before codegen runs:
    // a non-empty diagnostic set below means no output is ever opened.
    if output_conflicts_with_source(&project, output) {
        diagnostics.extend(diagnostic(
            Problem::OutputPathConflictsWithInput,
            output,
            String::from("Choose an output path that is not an input source file"),
        ));
    }

    // Build a SourceLookup that hands codegen the exact bytes the parser saw
    // for each FileId. The container's debug section SOURCE_FILE_TABLE (tag 6)
    // records a BLAKE3 hash over these so a debugger can detect drift between
    // the .iplc and the user's working copy.
    let source_lookup = HashMapSourceLookup::from_project(&project);

    // The pipeline (parse, analysis, codegen) is owned by `ironplc-project`.
    // Handing it the diagnostics collected so far is what makes a
    // discovery-time problem suppress the container while still letting
    // analysis run against the files that did resolve.
    let output_result =
        ironplc_project::compile(&mut project, &compiler_options, &source_lookup, diagnostics);

    if let Some(container) = output_result.container {
        // Write the container to the output file
        let mut out_file = std::fs::File::create(output)
            .map_err(|e| format!("Failed to create output file: {e}"))?;
        container
            .write_to(&mut out_file)
            .map_err(|e| format!("Failed to write output file: {e}"))?;
    }

    finish(
        "Compile",
        output_result.diagnostics,
        Some(&project),
        suppress_output,
    )
}

/// Reconciles the stable variable UID sidecar with the project's
/// declarations and prints the sync report.
///
/// The report lists preserved, assigned, and removed keys plus rename and
/// swap candidates; the candidates are heuristics the user resolves
/// explicitly with [`map_uid`]. A clean analysis is required: a broken
/// project must not rewrite the sidecar. A report with unresolved candidates
/// must not rewrite it either: saving would drop the removed keys, leaving
/// nothing for `map-uid` to move, so the sidecar is persisted only when the
/// reconciliation is unambiguous.
pub fn sync_uids(
    paths: &[PathBuf],
    compiler_options: CompilerOptions,
    suppress_output: bool,
) -> Result<(), String> {
    let Some(project_path) = paths.first() else {
        return Err(String::from("Sync UIDs requires a project path"));
    };
    let Some(sidecar_path) = sidecar_path_for(project_path) else {
        return Err(format!(
            "Cannot derive a sidecar path from {}",
            project_path.display()
        ));
    };

    let (mut project, mut diagnostics) = create_project(paths, compiler_options, &[]);
    diagnostics.extend(project.semantic());
    if !diagnostics.is_empty() {
        return finish("Sync UIDs", diagnostics, Some(&project), suppress_output);
    }
    let Some(library) = project.analyzed_library() else {
        // A clean analysis always caches its artifacts, so this is a
        // compiler defect rather than a problem with the input.
        diagnostics.push(Diagnostic::internal_error());
        return finish("Sync UIDs", diagnostics, Some(&project), suppress_output);
    };

    let mut sidecar = match Sidecar::load(&sidecar_path) {
        Ok(sidecar) => sidecar,
        Err(err) => {
            diagnostics.push(err);
            return finish("Sync UIDs", diagnostics, Some(&project), suppress_output);
        }
    };
    let report = sidecar.sync(&declared_var_keys(library, &compiler_options));
    let has_candidates = !report.rename_candidates.is_empty() || !report.swap_candidates.is_empty();
    if !has_candidates {
        if let Err(err) = sidecar.save(&sidecar_path) {
            diagnostics.push(err);
            return finish("Sync UIDs", diagnostics, Some(&project), suppress_output);
        }
    }

    print!("{report}");
    finish("Sync UIDs", diagnostics, Some(&project), suppress_output)
}

/// Records an explicit rename or swap resolution in the project's stable
/// variable UID sidecar by moving the old key's UID to the new key.
pub fn map_uid(
    project_path: &Path,
    old_scope: &str,
    old_name: &str,
    new_scope: &str,
    new_name: &str,
) -> Result<(), String> {
    let Some(sidecar_path) = sidecar_path_for(project_path) else {
        return Err(format!(
            "Cannot derive a sidecar path from {}",
            project_path.display()
        ));
    };
    let mut sidecar = Sidecar::load(&sidecar_path).map_err(|err| diagnostic_message(&err))?;
    sidecar
        .map_uid(
            &SidecarKey::new(old_scope, old_name),
            &SidecarKey::new(new_scope, new_name),
        )
        .map_err(|err| err.to_string())?;
    sidecar
        .save(&sidecar_path)
        .map_err(|err| diagnostic_message(&err))?;
    Ok(())
}

/// Flattens a diagnostic into the command's `Result` error string, the way
/// operational (non-diagnostic-collection) failures are reported.
fn diagnostic_message(diagnostic: &Diagnostic) -> String {
    format!(
        "{}: {}: {}",
        diagnostic.code,
        diagnostic.description(),
        diagnostic.primary.message
    )
}

/// Codegen [`SourceLookup`](ironplc_codegen::SourceLookup) backed by an
/// in-memory map populated from the project's loaded sources. The map
/// owns the bytes so the lookup can outlive any borrow on the project.
struct HashMapSourceLookup(std::collections::HashMap<ironplc_dsl::core::FileId, Vec<u8>>);

impl HashMapSourceLookup {
    fn from_project(project: &FileBackedProject) -> Self {
        HashMapSourceLookup(
            project
                .sources()
                .iter()
                .map(|src| (src.file_id().clone(), src.as_string().as_bytes().to_vec()))
                .collect(),
        )
    }
}

impl ironplc_codegen::SourceLookup for HashMapSourceLookup {
    fn source_bytes(&self, file_id: &ironplc_dsl::core::FileId) -> Option<&[u8]> {
        self.0.get(file_id).map(Vec::as_slice)
    }
}

/// Prints every diagnostic a command collected and derives the command's
/// final result from that same collection: `Ok` when there are none,
/// otherwise `Err` naming the command and the problem count.
///
/// This is the single fold point from diagnostics (data) to exit status
/// (control flow). Commands accumulate diagnostics from every stage —
/// discovery, parsing, analysis, codegen — and must not decide
/// success/failure anywhere else, so that a problem in one stage never
/// prevents a later stage from reporting its own.
fn finish(
    command: &str,
    diagnostics: Vec<Diagnostic>,
    project: Option<&FileBackedProject>,
    suppress_output: bool,
) -> Result<(), String> {
    if diagnostics.is_empty() {
        return Ok(());
    }
    trace!("Errors {diagnostics:?}");
    handle_diagnostics(&diagnostics, project, suppress_output);
    Err(format!(
        "{command} failed with {} problem(s)",
        diagnostics.len()
    ))
}

/// Builds a project from `paths`, running discovery and loading every
/// resolvable file.
///
/// Returns the project together with every diagnostic discovery produced
/// (an unresolvable `.plcproj` entry, a referenced-but-unbundled
/// compatibility library, or an unreadable source file). Nothing is
/// printed here and nothing aborts: the project contains every file that
/// DID resolve, so callers run their normal work (analysis, codegen, ...)
/// against it — real diagnostics in the resolvable files must not be
/// hidden behind a discovery-time problem — and fold the returned
/// diagnostics into their final result via [`finish`].
fn create_project(
    paths: &[PathBuf],
    compiler_options: CompilerOptions,
    libraries: &[LibraryName],
) -> (FileBackedProject, Vec<Diagnostic>) {
    trace!("Reading paths {paths:?}");
    let mut files: Vec<PathBuf> = vec![];
    // Explicit `--library` activation, plus any libraries discovered project
    // files reference. The explicit set is applied first so it takes
    // precedence in ordering; discovered libraries are appended, deduplicated.
    let mut activated_libraries: Vec<LibraryName> = libraries.to_vec();
    let mut diagnostics: Vec<Diagnostic> = vec![];

    for path in paths {
        let (mut resolved, discovered_libraries, path_diagnostics) = enumerate_files(path);
        files.append(&mut resolved);
        for library in discovered_libraries {
            if !activated_libraries.contains(&library) {
                activated_libraries.push(library);
            }
        }
        diagnostics.extend(path_diagnostics);
    }

    // Create the project
    let mut project = FileBackedProject::with_options(compiler_options);
    project.set_activated_libraries(activated_libraries);

    for file_path in files {
        if let Err(err) = project.push(FileId::from_path(&file_path)) {
            diagnostics.push(err);
        }
    }

    // Load the UID sidecar derived from the first path so the compile
    // pipeline injects the project's stable variable IDs (ADR 0053). A
    // missing or malformed sidecar is an empty table, not an error.
    if let Some(sidecar_path) = paths.first().and_then(|path| sidecar_path_for(path)) {
        if let Err(err) = project.load_uid_sidecar(&sidecar_path) {
            diagnostics.push(err);
        }
    }

    (project, diagnostics)
}

/// Enumerates all files at the path.
///
/// Project discovery decides what the path means -- a `.sln`/`.plcproj`
/// or the folder holding one is a TwinCAT project, a `plc.xml` or its
/// folder is a Beremiz project, and anything else is enumerated as loose
/// source files. Naming a manifest is how a user says which project they
/// mean when a folder holds more than one.
///
/// Discovery problems that shouldn't stop the rest of the project from
/// being enumerated (e.g. a `.plcproj` `<Compile Include="...">` entry
/// that doesn't resolve to a real file) are returned alongside whatever
/// files DID resolve, rather than aborting enumeration entirely -- but
/// they are still genuine errors: the caller must still fail the overall
/// command if this returns any diagnostics.
fn enumerate_files(path: &PathBuf) -> (Vec<PathBuf>, Vec<LibraryName>, Vec<Diagnostic>) {
    // Get the canonical path so that error messages are unambiguous
    let path = match canonicalize(path) {
        Ok(path) => path,
        Err(e) => {
            return (
                vec![],
                vec![],
                diagnostic(
                    Problem::CannotCanonicalizePath,
                    path,
                    format!("{}, {}", path.display(), e),
                ),
            );
        }
    };

    // Determine what kind of path we have.
    let metadata = match metadata(&path) {
        Ok(metadata) => metadata,
        Err(e) => {
            return (
                vec![],
                vec![],
                diagnostic(Problem::CannotReadMetadata, &path, e.to_string()),
            );
        }
    };
    if metadata.is_symlink() {
        return (
            vec![],
            vec![],
            diagnostic(Problem::SymlinkUnsupported, &path, String::from("")),
        );
    }

    enumerate_project(ironplc_sources::discovery::discover(&path))
}

/// Flattens a discovered project into the files, libraries, and problems
/// [`enumerate_files`] reports.
///
/// Auto-activates the libraries a discovered project file references,
/// alongside any files it declares. Referenced but unshipped libraries
/// contribute a diagnostic naming them.
fn enumerate_project(
    discovered: Result<DiscoveredProject, Diagnostic>,
) -> (Vec<PathBuf>, Vec<LibraryName>, Vec<Diagnostic>) {
    match discovered {
        Ok(project) => {
            let (libraries, library_diagnostics) =
                ironplc_sources::libraries::LibraryRegistry::bundled()
                    .resolve_references(&project.library_references);
            let mut diagnostics = project.errors;
            diagnostics.extend(library_diagnostics);
            (project.files, libraries, diagnostics)
        }
        Err(e) => (vec![], vec![], vec![e]),
    }
}

/// Converts an IronPLC diagnostic into the
fn handle_diagnostics(
    diagnostics: &[Diagnostic],
    project: Option<&FileBackedProject>,
    suppress_output: bool,
) {
    if !suppress_output {
        let writer = StandardStream::stderr(ColorChoice::Always);
        let config = codespan_reporting::term::Config::default();

        let mut files: SimpleFiles<String, &str> = SimpleFiles::new();

        let mut unique_files: BTreeSet<&FileId> = BTreeSet::new();
        for diagnostic in diagnostics {
            for file_id in diagnostic.file_ids() {
                unique_files.insert(file_id);
            }
        }

        let mut files_to_ids: HashMap<&FileId, usize> = HashMap::new();
        let empty_source = &"".to_owned();
        match project {
            Some(set) => {
                for file_id in unique_files {
                    if let Some(content) = set.get(file_id) {
                        let id = files.add(file_id.to_string(), content.as_string());
                        files_to_ids.insert(file_id, id);
                    } else {
                        let id = files.add(file_id.to_string(), empty_source);
                        files_to_ids.insert(file_id, id);
                    }
                }
            }
            None => {
                for file_id in unique_files {
                    let id = files.add(file_id.to_string(), empty_source);
                    files_to_ids.insert(file_id, id);
                }
            }
        }

        diagnostics.iter().for_each(|d| {
            let diagnostic = map_diagnostic(d, &files_to_ids);

            if let Err(err) =
                term::emit_to_write_style(&mut writer.lock(), &config, &files, &diagnostic)
            {
                error!("Failed writing to terminal: {err}");
            }
        });
    }
}

/// Builds the documentation URL for a diagnostic surfaced on the command line.
///
/// `channel=cli` marks the origin; everything else about the URL — the section
/// the code documents into, the version, the Rust source location — is
/// [`Diagnostic::help_url`]'s business, so the CLI cannot drift from the other
/// channels.
fn problem_help_url(diagnostic: &Diagnostic) -> String {
    diagnostic.help_url(env!("CARGO_PKG_VERSION"), "cli")
}

fn map_diagnostic(
    diagnostic: &Diagnostic,
    file_to_id: &HashMap<&FileId, usize>,
) -> CodeSpanDiagnostic<usize> {
    let description = diagnostic.description();

    // Set the primary labels
    let mut labels = vec![map_label(
        &diagnostic.primary,
        LabelStyle::Primary,
        file_to_id,
    )];

    // Add any secondary labels
    labels.extend(
        diagnostic
            .secondary
            .iter()
            .map(|lbl| map_label(lbl, LabelStyle::Secondary, file_to_id)),
    );

    // Existing help notes, then a trailing link to the problem-code docs so a
    // CLI user can follow the same reference page the editor and playground link
    // to (and so that follow-through is attributable to the CLI in analytics).
    let mut notes = diagnostic.help().to_vec();
    notes.push(format!("Learn more: {}", problem_help_url(diagnostic)));

    CodeSpanDiagnostic::new(Severity::Error)
        .with_code(diagnostic.code.clone())
        .with_message(description)
        .with_labels(labels)
        .with_notes(notes)
}

fn map_label(
    label: &Label,
    style: LabelStyle,
    file_to_id: &HashMap<&FileId, usize>,
) -> CodeSpanLabel<usize> {
    let range = Range {
        start: label.location.start,
        end: label.location.end,
    };
    let id = file_to_id.get(&label.file_id);
    CodeSpanLabel::new(style, *id.unwrap_or(&0), range).with_message(&label.message)
}

/// Returns `true` when `output` refers to the same file as any loaded source.
///
/// Paths are canonicalized before comparison so relative-vs-absolute
/// differences and symbolic links resolve to the same target. When `output`
/// does not yet exist its canonicalization fails, so there is no conflict.
///
/// Hard-link aliasing is NOT caught: two names for one inode are genuinely
/// distinct paths with nothing for `canonicalize` to resolve, so writing
/// through one destroys a source reached by the other. Comparing device and
/// inode instead of canonical paths would close that hole.
fn output_conflicts_with_source(project: &FileBackedProject, output: &Path) -> bool {
    let Ok(output) = canonicalize(output) else {
        return false;
    };
    project.sources().iter().any(|source| {
        canonicalize(PathBuf::from(source.file_id().to_string()))
            .map(|resolved| resolved == output)
            .unwrap_or(false)
    })
}

fn diagnostic(problem: Problem, path: &Path, message: String) -> Vec<Diagnostic> {
    vec![Diagnostic::problem(
        problem,
        Label::file(FileId::from_path(path), message),
    )]
}

#[cfg(test)]
mod tests;
