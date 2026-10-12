//! The steps of a project, a set of files: parse them, analyze them, generate
//! code. The rows of the table of paths (`paths`) over the inputs that are
//! projects are made of these steps, so that a row measures one step and
//! prepares the steps before it outside the measurement.

use crate::corpus::CorpusFile;
use ironplc_analyzer::{stages::analyze, SemanticContext};
use ironplc_codegen::{compile, CodegenOptions, EmptyLookup};
use ironplc_container::Container;
use ironplc_dsl::{common::Library, core::FileId, diagnostic::Diagnostic};
use ironplc_parser::{options::CompilerOptions, parse_program};

/// The result of analyzing a project: the library and what was found about it.
pub type Analyzed = (Library, SemanticContext);

/// Parses every file of a project, each under its own file id. Fails on the
/// first file that does not parse.
pub fn parse_files(
    files: &[CorpusFile],
    options: &CompilerOptions,
) -> Result<Vec<Library>, Diagnostic> {
    files
        .iter()
        .map(|file| parse_program(&file.source, &FileId::from_string(&file.name), options))
        .collect()
}

/// Analyzes parsed files as one project.
pub fn analyze_files(
    libraries: &[Library],
    options: &CompilerOptions,
) -> Result<Analyzed, Vec<Diagnostic>> {
    let references: Vec<&Library> = libraries.iter().collect();
    analyze(&references, options)
}

/// Parses and analyzes a project: what code generation takes.
pub fn analyzed(
    files: &[CorpusFile],
    options: &CompilerOptions,
) -> Result<Analyzed, Vec<Diagnostic>> {
    let libraries = parse_files(files, options).map_err(|diagnostic| vec![diagnostic])?;
    analyze_files(&libraries, options)
}

/// Generates the container of an analyzed project.
pub fn generate_code(analyzed: &Analyzed) -> Result<Container, Diagnostic> {
    compile(
        &analyzed.0,
        &analyzed.1,
        &CodegenOptions::default(),
        &EmptyLookup,
    )
}

/// Parse, analyze and generate code, as the compiler does for a project.
pub fn build(
    files: &[CorpusFile],
    options: &CompilerOptions,
) -> Result<Container, Vec<Diagnostic>> {
    let analyzed = analyzed(files, options)?;
    generate_code(&analyzed).map_err(|diagnostic| vec![diagnostic])
}

/// What a step made of a project, in a few words.
pub fn describe_analysis(result: &Result<Analyzed, Vec<Diagnostic>>) -> String {
    match result {
        Ok((library, context)) => format!(
            "ok, {} elements, {} diagnostics",
            library.elements.len(),
            context.diagnostics().len()
        ),
        Err(diagnostics) => format!("err, {} diagnostics", diagnostics.len()),
    }
}

/// What code generation made of a project, in a few words.
pub fn describe_code(result: &Result<Container, Diagnostic>) -> String {
    match result {
        Ok(container) => format!(
            "ok, {} functions, {} code bytes, {} variables",
            container.code.functions.len(),
            container.code.section_size(),
            container.header.num_variables
        ),
        Err(diagnostic) => format!("err[{}]", diagnostic.code),
    }
}

/// What the pipeline made of a project, in a few words.
pub fn describe_build(result: &Result<Container, Vec<Diagnostic>>) -> String {
    match result {
        Ok(container) => format!("ok, {} functions", container.code.functions.len()),
        Err(diagnostics) => format!("err, {} diagnostics", diagnostics.len()),
    }
}
