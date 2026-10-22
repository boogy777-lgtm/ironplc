//! Implements the command line behavior.

use codespan_reporting::{
    diagnostic::{
        Diagnostic as CodeSpanDiagnostic, Label as CodeSpanLabel, LabelStyle,
        Severity as CodeSpanSeverity,
    },
    files::SimpleFiles,
    term::{
        self,
        termcolor::{ColorChoice, ColorSpec, StandardStream, WriteColor},
    },
};
use ironplc_dsl::{
    core::FileId,
    diagnostic::{
        any_stops_build, stops_build_count, Area, Diagnostic, Label, ScriptMessage, Severity,
        CHECK_SUMMARY, COMPILE_SUMMARY,
    },
};
use ironplc_plc2plc::write_to_string;
use ironplc_problems::{text, Text};
use log::{error, trace};
use std::{
    collections::{BTreeSet, HashMap},
    fs::{canonicalize, metadata},
    io::{IsTerminal, Write},
    ops::Range,
    path::{Path, PathBuf},
};

use ironplc_parser::options::CompilerOptions;
use ironplc_project::build_record::BuildInputs;
use ironplc_project::sidecar::{declared_var_keys, Sidecar, SidecarKey};
use ironplc_project::RecordKind;
use ironplc_sources::discovery::DiscoveredProject;
use ironplc_sources::LibraryName;

use ironplc_project::tokenizer;
use ironplc_project::{FileBackedProject, Project};

/// Where the messages of a command go, and in which form.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Output {
    /// Nowhere: the caller reads the result.
    Silent,
    /// To the terminal, in the form for a person: the words of the CODESYS
    /// Messages view, with the source snippet.
    Terminal,
    /// To standard output, in the form for a program: a JSON array with one
    /// `ScriptMessage` for each message, suppressed messages included.
    Json,
}

/// The `suppress_output` flag of a command that has no JSON form: the terminal,
/// unless the caller asked for none.
impl From<bool> for Output {
    fn from(suppress_output: bool) -> Self {
        if suppress_output {
            Output::Silent
        } else {
            Output::Terminal
        }
    }
}

// Checks specified files.
pub fn check(
    paths: &[PathBuf],
    compiler_options: CompilerOptions,
    libraries: &[LibraryName],
    output: Output,
) -> Result<(), String> {
    let (project, diagnostics) = run_check(paths, compiler_options, libraries);

    finish("Check", diagnostics, Some(&project), output)
}

/// The project and the messages of a check: the problems of discovery and of the
/// analysis, and, as a check is a compile without code generation, the line
/// that ends it with its counts.
fn run_check(
    paths: &[PathBuf],
    compiler_options: CompilerOptions,
    libraries: &[LibraryName],
) -> (FileBackedProject, Vec<Diagnostic>) {
    let (mut project, mut diagnostics) = create_project(paths, compiler_options, libraries);

    // Analyze the set
    diagnostics.extend(project.semantic(Area::Build));

    let application = project.application_name(&project_name(paths));
    let diagnostics = CHECK_SUMMARY.close(&application, &compiler_options.messages, diagnostics);
    (project, diagnostics)
}

/// The name of the project a command is run for: the file name without its
/// extension of the first path, or the name of the directory. It is the name of
/// the application, which the line "Build started" gives, when the program has
/// no `CONFIGURATION` to name it ([`Project::application_name`]).
fn project_name(paths: &[PathBuf]) -> String {
    let Some(path) = paths.first() else {
        return String::new();
    };
    let path = canonicalize(path).unwrap_or_else(|_| path.clone());
    let name = if path.is_dir() {
        path.file_name()
    } else {
        path.file_stem()
    };
    name.map(|name| name.to_string_lossy().into_owned())
        .unwrap_or_default()
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
        diagnostics.extend(tokenizer::tokenize_source(src, &compiler_options));
    }

    finish("Tokenize", diagnostics, Some(&project), suppress_output)
}

/// Compiles source files into a bytecode container (.iplc) file.
///
/// Parses the source files, runs full analysis (type resolution + semantic
/// checks), and generates bytecode. A build that finds nothing changed since the
/// last one does none of it and says the application is up to date; `force`
/// builds anyway ([`up_to_date`]).
pub fn compile(
    paths: &[PathBuf],
    output: &Path,
    compiler_options: CompilerOptions,
    libraries: &[LibraryName],
    force: bool,
    suppress_output: bool,
) -> Result<(), String> {
    let (project, diagnostics) = run_compile(paths, output, compiler_options, libraries, force)?;

    finish("Compile", diagnostics, Some(&project), suppress_output)
}

/// Runs the build, writes the container when nothing stops it, and returns the
/// project with the messages of the build: the line that starts it, the problems
/// and the line that ends it with its counts and whether the container is ready
/// for download.
///
/// When the build record says the container on disk is what this build would
/// write, nothing runs and the messages are the line that starts the build and
/// the line that says the application is up to date. After a container is
/// written, the build record of it is written.
fn run_compile(
    paths: &[PathBuf],
    output: &Path,
    compiler_options: CompilerOptions,
    libraries: &[LibraryName],
    force: bool,
) -> Result<(FileBackedProject, Vec<Diagnostic>), String> {
    let (mut project, mut diagnostics) = create_project(paths, compiler_options, libraries);

    // Refuse to write the container over a loaded source file. `File::create`
    // truncates immediately, so this must be diagnosed before codegen runs:
    // a non-empty diagnostic set below means no output is ever opened.
    if output_conflicts_with_source(&project, output) {
        diagnostics.extend(diagnostic(output, text::output_path_conflicts_with_input()));
    }

    // The inputs are read from the project that the pipeline below receives.
    let project_name = project_name(paths);
    let inputs = BuildInputs::of(
        &project,
        &compiler_options,
        &project_name,
        env!("CARGO_PKG_VERSION"),
    );
    // A problem found so far stops the build, so there is no container to skip.
    let previous = up_to_date::previous(output, &inputs, !force && !any_stops_build(&diagnostics));
    if let Some(application) = previous.up_to_date {
        let opening = COMPILE_SUMMARY.opening(&application);
        let lines = opening.into_iter().chain([up_to_date::up_to_date_line()]);
        return Ok((project, lines.collect()));
    }
    diagnostics.extend(previous.diagnostics);

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
    let mut result_diagnostics = output_result.diagnostics;
    let application = project.application_name(&project_name);

    if let Some(container) = output_result.container {
        // The header of a container is derived while it is serialized, so the
        // bytes are made first and the record is taken from them.
        let mut bytes = Vec::new();
        container
            .write_to(&mut bytes)
            .map_err(|e| format!("Failed to write output file: {e}"))?;
        let mut out_file = std::fs::File::create(output)
            .map_err(|e| format!("Failed to create output file: {e}"))?;
        out_file
            .write_all(&bytes)
            .map_err(|e| format!("Failed to write output file: {e}"))?;
        drop(out_file);

        // Only a container that was written has a record. A failed build leaves
        // the record that is there: it still describes the container on disk.
        if let Err(problem) = up_to_date::write(output, inputs, &application, &bytes) {
            result_diagnostics.push(problem);
        }
    }

    let diagnostics =
        COMPILE_SUMMARY.close(&application, &compiler_options.messages, result_diagnostics);
    Ok((project, diagnostics))
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
    let Some(sidecar_path) = RecordKind::Uids.path(project_path) else {
        return Err(format!(
            "Cannot derive a sidecar path from {}",
            project_path.display()
        ));
    };

    let (mut project, mut diagnostics) = create_project(paths, compiler_options, &[]);
    diagnostics.extend(project.semantic(Area::Build));
    if any_stops_build(&diagnostics) {
        return finish("Sync UIDs", diagnostics, Some(&project), suppress_output);
    }
    let Some(library) = project.analyzed_library() else {
        // An analysis that found no error always caches its artifacts, so this is a
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
    let Some(sidecar_path) = RecordKind::Uids.path(project_path) else {
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
/// final result from that same collection: `Ok` when none is an error,
/// otherwise `Err` naming the command and the error count. A warning or an
/// information message is printed and does not fail the command.
///
/// This is the single fold point from diagnostics (data) to exit status
/// (control flow). Commands accumulate diagnostics from every stage --
/// discovery, parsing, analysis, codegen -- and must not decide
/// success/failure anywhere else, so that a problem in one stage never
/// prevents a later stage from reporting its own.
fn finish(
    command: &str,
    diagnostics: Vec<Diagnostic>,
    project: Option<&FileBackedProject>,
    output: impl Into<Output>,
) -> Result<(), String> {
    let output = output.into();
    // The JSON form is a list even when it is empty.
    if diagnostics.is_empty() && output != Output::Json {
        return Ok(());
    }
    trace!("Diagnostics {diagnostics:?}");
    handle_diagnostics(&diagnostics, project, output);
    if !any_stops_build(&diagnostics) {
        return Ok(());
    }
    Err(format!(
        "{command} failed with {} problem(s)",
        stops_build_count(&diagnostics)
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
    if let Some(sidecar_path) = paths.first().and_then(|path| RecordKind::Uids.path(path)) {
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
                diagnostic(path, text::cannot_canonicalize_path(path.display(), e)),
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
                diagnostic(&path, text::cannot_read_metadata(e)),
            );
        }
    };
    if metadata.is_symlink() {
        return (
            vec![],
            vec![],
            diagnostic(&path, text::symlink_unsupported()),
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

/// Shows the diagnostics in the form of `output`.
fn handle_diagnostics(
    diagnostics: &[Diagnostic],
    project: Option<&FileBackedProject>,
    output: Output,
) {
    match output {
        Output::Silent => {}
        Output::Terminal => print_for_person(diagnostics, project),
        Output::Json => print_for_program(diagnostics, project),
    }
}

/// Prints every diagnostic that is shown, in the form for a person.
fn print_for_person(diagnostics: &[Diagnostic], project: Option<&FileBackedProject>) {
    let choice = color_choice(
        std::io::stderr().is_terminal(),
        std::env::var_os("NO_COLOR").is_some(),
    );
    write_for_person(&mut StandardStream::stderr(choice), diagnostics, project);
}

/// Whether the messages are drawn in colors: only when standard error is a
/// terminal that did not opt out with `NO_COLOR`. A pipe, a file, a CI log and
/// an editor's output channel get plain text, as the escapes would be noise
/// there.
fn color_choice(stderr_is_terminal: bool, no_color_set: bool) -> ColorChoice {
    if stderr_is_terminal && !no_color_set {
        ColorChoice::Always
    } else {
        ColorChoice::Never
    }
}

/// The path of a file as a person reads it. `canonicalize` gives a Windows path
/// the verbatim prefix `\\?\` (`\\?\C:\x`, `\\?\UNC\server\x`), which no editor
/// turns into a link; the prefix is dropped here (`C:\x`, `\\server\x`). Only
/// the display changes, the file id that is compared elsewhere does not. A path
/// without the prefix, and a verbatim path of another kind (`\\?\Volume{..}`),
/// is returned as it is.
fn display_path(path: &str) -> String {
    if let Some(rest) = path.strip_prefix(r"\\?\UNC\") {
        return format!(r"\\{rest}");
    }
    if let Some(rest) = path.strip_prefix(r"\\?\") {
        let mut chars = rest.chars();
        let is_disk = matches!(
            (chars.next(), chars.next(), chars.next()),
            (Some(letter), Some(':'), None | Some('\\')) if letter.is_ascii_alphabetic()
        );
        if is_disk {
            return rest.to_owned();
        }
    }
    path.to_owned()
}

/// Writes every diagnostic that is shown, in the form for a person, to `writer`.
fn write_for_person(
    writer: &mut impl WriteColor,
    diagnostics: &[Diagnostic],
    project: Option<&FileBackedProject>,
) {
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
                    let id = files.add(display_path(&file_id.to_string()), content.as_string());
                    files_to_ids.insert(file_id, id);
                } else {
                    let id = files.add(display_path(&file_id.to_string()), empty_source);
                    files_to_ids.insert(file_id, id);
                }
            }
        }
        None => {
            for file_id in unique_files {
                let id = files.add(display_path(&file_id.to_string()), empty_source);
                files_to_ids.insert(file_id, id);
            }
        }
    }

    diagnostics
        .iter()
        .for_each(|d| match display_of(d.severity()) {
            Display::Hidden => {}
            Display::TextOnly => {
                if let Err(err) = writeln!(writer, "{}", d.person_text()) {
                    error!("Failed writing to terminal: {err}");
                }
            }
            Display::Report(severity) if !d.has_code() => {
                if let Err(err) = write_line(writer, d, severity) {
                    error!("Failed writing to terminal: {err}");
                }
            }
            Display::Report(severity) => {
                let report = map_diagnostic(d, severity, &files_to_ids);
                if let Err(err) = write_report(writer, &config, &files, d, severity, &report) {
                    error!("Failed writing to terminal: {err}");
                }
            }
        });
}

/// Prints every diagnostic, suppressed ones included, in the form for a
/// program.
fn print_for_program(diagnostics: &[Diagnostic], project: Option<&FileBackedProject>) {
    match json_of(diagnostics, project) {
        Ok(json) => println!("{json}"),
        Err(err) => error!("Failed writing the messages: {err}"),
    }
}

/// The form for a program: one JSON array with the `ScriptMessage` of each
/// diagnostic, in order. A diagnostic of any category is in it, a suppressed one
/// too, so a tool can see what was hidden.
fn json_of(
    diagnostics: &[Diagnostic],
    project: Option<&FileBackedProject>,
) -> Result<String, serde_json::Error> {
    let messages: Vec<ScriptMessage> = diagnostics
        .iter()
        .map(|d| {
            let source = project.and_then(|set| set.get(&d.primary.file_id));
            d.script_message(source.map(|content| content.as_string()))
        })
        .collect();
    serde_json::to_string(&messages)
}

/// Writes a message that has no code and so no place: its header word, which
/// the category declares, and its text, on one line. There is no source snippet
/// and no link to a page about the code.
fn write_line(
    out: &mut impl WriteColor,
    diagnostic: &Diagnostic,
    severity: CodeSpanSeverity,
) -> Result<(), Box<dyn std::error::Error>> {
    let Some(header) = diagnostic.person_header() else {
        // A category without a header word is a plain line (see `display_of`).
        writeln!(out, "{}", diagnostic.person_text())?;
        return Ok(());
    };
    let styles = term::Styles::default();
    out.set_color(styles.header(severity))?;
    write!(out, "{header}")?;
    out.set_color(styles.header_message())?;
    write!(out, ": {}", diagnostic.person_text())?;
    out.reset()?;
    writeln!(out)?;
    Ok(())
}

/// Writes a diagnostic that has a header: the header of the CODESYS Messages
/// view, then the source snippet that codespan draws.
///
/// codespan prints a header of its own (`error[P4007]: ...`), always in lower
/// case and with its own word, so the header is written here and the one
/// codespan writes is dropped (see [`BelowHeader`]).
fn write_report(
    out: &mut impl WriteColor,
    config: &term::Config,
    files: &SimpleFiles<String, &str>,
    diagnostic: &Diagnostic,
    severity: CodeSpanSeverity,
    report: &CodeSpanDiagnostic<usize>,
) -> Result<(), Box<dyn std::error::Error>> {
    let Some(header) = diagnostic.person_header() else {
        // A category without a header word is not drawn as a report (see
        // `display_of`); codespan draws its own header for it.
        return Ok(term::emit_to_write_style(out, config, files, report)?);
    };
    let styles = term::Styles::default();
    out.set_color(styles.header(severity))?;
    write!(out, "{header}")?;
    out.set_color(styles.header_message())?;
    write!(out, ": {}", diagnostic.description())?;
    out.reset()?;
    writeln!(out)?;
    Ok(term::emit_to_write_style(
        &mut BelowHeader::new(out),
        config,
        files,
        report,
    )?)
}

/// A writer that drops the first line written to it, the header codespan
/// renders, and passes on everything after it, with its colors.
struct BelowHeader<W> {
    inner: W,
    in_header: bool,
}

impl<W> BelowHeader<W> {
    fn new(inner: W) -> Self {
        BelowHeader {
            inner,
            in_header: true,
        }
    }
}

impl<W: Write> Write for BelowHeader<W> {
    fn write(&mut self, buf: &[u8]) -> std::io::Result<usize> {
        if !self.in_header {
            return self.inner.write(buf);
        }
        if let Some(end) = buf.iter().position(|byte| *byte == b'\n') {
            self.in_header = false;
            self.inner.write_all(&buf[end + 1..])?;
        }
        Ok(buf.len())
    }

    fn flush(&mut self) -> std::io::Result<()> {
        self.inner.flush()
    }
}

impl<W: WriteColor> WriteColor for BelowHeader<W> {
    fn supports_color(&self) -> bool {
        self.inner.supports_color()
    }

    fn set_color(&mut self, spec: &ColorSpec) -> std::io::Result<()> {
        if self.in_header {
            Ok(())
        } else {
            self.inner.set_color(spec)
        }
    }

    fn reset(&mut self) -> std::io::Result<()> {
        if self.in_header {
            Ok(())
        } else {
            self.inner.reset()
        }
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
    severity: CodeSpanSeverity,
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

    CodeSpanDiagnostic::new(severity)
        .with_code(diagnostic.code.clone())
        .with_message(description)
        .with_labels(labels)
        .with_notes(notes)
}

/// How the terminal shows a diagnostic of a category.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Display {
    /// With its header, code, labels and notes, in the style codespan has for
    /// the severity.
    Report(CodeSpanSeverity),
    /// As the text alone, without a code: a line about the build.
    TextOnly,
    /// Not at all: the diagnostic is suppressed.
    Hidden,
}

/// How the terminal shows a category. The header word is the one the category
/// declares (`Severity::person_word`); the style of the header is the one
/// codespan has for the severity, so a fatal error is drawn as an error and an
/// information message as a note. No arm is a wildcard: a category added to
/// `Severity` does not compile until it is shown, or hidden, here.
fn display_of(severity: Severity) -> Display {
    match severity {
        Severity::FatalError | Severity::Error => Display::Report(CodeSpanSeverity::Error),
        Severity::Warning => Display::Report(CodeSpanSeverity::Warning),
        Severity::Information => Display::Report(CodeSpanSeverity::Note),
        Severity::Text => Display::TextOnly,
        Severity::SuppressedWarning | Severity::SuppressedInformation => Display::Hidden,
    }
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

fn diagnostic(path: &Path, text: Text) -> Vec<Diagnostic> {
    vec![Diagnostic::text_in_file(FileId::from_path(path), text)]
}

mod download_record;
pub use download_record::{record_download, record_download_path, RecordDownload};

mod path_record;
pub use path_record::{record_path, record_path_path, RecordPath};

mod up_to_date;

#[cfg(test)]
mod summary_tests;

#[cfg(test)]
mod up_to_date_tests;

#[cfg(test)]
mod tests;

#[cfg(test)]
mod display_tests;
