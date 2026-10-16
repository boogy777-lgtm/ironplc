//! Adapts data types between what is required by the compiler
//! and the language server protocol.
use std::collections::BTreeMap;
use std::path::{Path, PathBuf};
use std::str::FromStr;

use ironplc_analyzer::extractors::TypeSymbolKind;
use ironplc_dsl::core::{FileId, Located};
use ironplc_dsl::diagnostic::LineColumn;
use log::error;
use lsp_types::{
    CodeDescription, DiagnosticRelatedInformation, DiagnosticSeverity, DocumentSymbol,
    DocumentSymbolResponse, Location, NumberOrString, SymbolKind, WorkspaceFolder,
};
use lsp_types::{SemanticToken, Uri};

use crate::lsp_runner::{RunResult, VmRunner};
use crate::semantic_tokens::to_semantic_tokens;
use ironplc_project::Project;

fn to_path_buf(uri: &Uri) -> Result<PathBuf, ()> {
    Ok(PathBuf::from(uri.path().as_str()))
}

/// A hashable, equality-comparable key derived from an LSP `Uri`.
///
/// `lsp_types::Uri` wraps `fluent_uri::Uri`, which contains interior
/// `Cell`s for caching parse results. Although the `Hash` and `Eq`
/// impls today only read the immutable string content, using such a
/// type as a `HashMap`/`HashSet` key is brittle: a future change in
/// the upstream crate could silently break lookups. Convert to this
/// newtype at the boundary so the collections store nothing more
/// than the canonical URI string.
#[derive(Clone, Debug, Hash, Eq, PartialEq, Ord, PartialOrd)]
pub(crate) struct UriKey(String);

impl UriKey {
    pub(crate) fn from_uri(uri: &Uri) -> Self {
        Self(uri.as_str().to_string())
    }

    /// Build a `UriKey` from a compiler `FileId`.
    ///
    /// Returns `None` for `FileId::BuiltIn`, for `FileId::File` with an
    /// empty path (which the analyzer uses as a placeholder for
    /// whole-project diagnostics like "no source files"), and when the
    /// path cannot be turned into a valid URI. Paths from
    /// `FileId::from_path` use OS-native separators; this normalises
    /// them and validates the result through `Uri::from_str` before
    /// returning the key.
    pub(crate) fn from_file_id(file_id: &FileId) -> Option<Self> {
        match file_id {
            FileId::File(path) => {
                let s = path.as_ref();
                if s.is_empty() {
                    return None;
                }
                let normalised = s.replace('\\', "/");
                let uri_str = if normalised.starts_with('/') {
                    format!("file://{normalised}")
                } else {
                    format!("file:///{normalised}")
                };
                let parsed = Uri::from_str(&uri_str).ok()?;
                Some(Self::from_uri(&parsed))
            }
            FileId::BuiltIn => None,
        }
    }

    #[cfg(test)]
    pub(crate) fn as_str(&self) -> &str {
        &self.0
    }

    /// Reconstruct an LSP `Uri` from this key for outbound LSP
    /// notifications. The key only ever holds a string that was
    /// previously produced by a valid `Uri`, so parsing must
    /// succeed.
    #[expect(
        clippy::expect_used,
        reason = "UriKey was constructed from a valid Uri, so parsing must succeed"
    )]
    pub(crate) fn to_uri(&self) -> Uri {
        Uri::from_str(&self.0).expect("UriKey was constructed from a valid Uri")
    }
}

/// The LSP project provides a view onto a project that accepts
/// and returns LSP types.
pub struct LspProject {
    wrapped: Box<dyn Project + Send>,
    /// Active VM runner session for step-through execution.
    runner: Option<VmRunner>,
    /// Compiler options used for compilation (cached for runner).
    compiler_options: ironplc_parser::options::CompilerOptions,
    /// Key for the most recently changed text document. Used as a
    /// fallback so that diagnostics whose `FileId` cannot be mapped
    /// back to a URI (e.g. `BuiltIn`) still surface in the editor
    /// instead of being silently dropped.
    last_changed_uri_key: Option<UriKey>,
}

impl LspProject {
    pub fn new(project: Box<dyn Project + Send>) -> Self {
        Self {
            wrapped: project,
            runner: None,
            compiler_options: ironplc_parser::options::CompilerOptions::default(),
            last_changed_uri_key: None,
        }
    }

    pub fn with_options(
        project: Box<dyn Project + Send>,
        options: ironplc_parser::options::CompilerOptions,
    ) -> Self {
        Self {
            wrapped: project,
            runner: None,
            compiler_options: options,
            last_changed_uri_key: None,
        }
    }

    /// Initializes the project from every given workspace folder, merged
    /// into one compilation unit -- see `Project::initialize_many`.
    pub(crate) fn initialize_many(&mut self, folders: &[WorkspaceFolder]) {
        let paths: Vec<PathBuf> = folders
            .iter()
            .filter_map(|folder| match to_path_buf(&folder.uri) {
                Ok(path) => Some(path),
                Err(_) => {
                    error!(
                        "URL must be convertible to a file path {}",
                        folder.uri.as_str()
                    );
                    None
                }
            })
            .collect();
        let refs: Vec<&Path> = paths.iter().map(|p| p.as_path()).collect();
        self.wrapped.initialize_many(&refs);
    }

    pub(crate) fn change_text_document(&mut self, uri: &Uri, content: String) {
        let path = to_path_buf(uri);
        if let Ok(path) = path {
            let file_id = FileId::from_path(&path);
            self.wrapped.change_text_document(&file_id, content);
            self.last_changed_uri_key = Some(UriKey::from_uri(uri));
        } else {
            error!("URL must be convertible to a file path {}", uri.as_str());
        }
    }

    /// The semantic tokens of the document, whatever the tokenizer reports.
    ///
    /// The token stream covers every byte of the text, so an unterminated
    /// comment, a pragma in a dialect without pragmas or any other error of the
    /// tokenizer leaves the rest of the document highlighted, as an editor
    /// expects while the text is being typed. The tokenizer's diagnostics are
    /// not returned here: they reach the editor through [`Self::semantic_all`].
    /// A document the project does not hold has no tokens.
    pub(crate) fn tokenize(&self, uri: &Uri) -> Vec<SemanticToken> {
        let Ok(path) = to_path_buf(uri) else {
            error!("URL must be convertible to a file path {}", uri.as_str());
            return vec![];
        };
        let (tokens, _diagnostics) = self.wrapped.tokenize(&FileId::from_path(&path));
        to_semantic_tokens(tokens)
    }

    /// Run semantic analysis on the whole workspace and return all
    /// resulting diagnostics grouped by the URI key that should
    /// display each one.
    ///
    /// Every diagnostic appears under each URI that its primary or
    /// secondary labels reference, so a cross-file diagnostic shows
    /// up in both files. Diagnostics whose `FileId` cannot be mapped
    /// to a URI (e.g. `BuiltIn`) are attributed to the most recently
    /// edited URI so they do not silently disappear.
    ///
    /// The map is ascending by URI, so the notifications go out in the same
    /// order on every run.
    ///
    /// Callers are responsible for emitting `PublishDiagnostics`
    /// notifications for every URI in the returned map and for
    /// emitting empty notifications to clear URIs that previously
    /// had diagnostics but no longer do.
    pub(crate) fn semantic_all(&mut self) -> BTreeMap<UriKey, Vec<lsp_types::Diagnostic>> {
        let diagnostics = self.wrapped.semantic();
        if diagnostics.is_empty() {
            return BTreeMap::new();
        }

        let mut by_key: BTreeMap<UriKey, Vec<lsp_types::Diagnostic>> = BTreeMap::new();
        for diagnostic in diagnostics {
            // Resolve every file id this diagnostic touches into a key.
            // De-duplicate so a diagnostic with primary and secondary
            // labels in the same file is published once for that file.
            let mut keys: Vec<UriKey> = Vec::new();
            for file_id in diagnostic.file_ids() {
                if let Some(key) = UriKey::from_file_id(file_id) {
                    if !keys.contains(&key) {
                        keys.push(key);
                    }
                }
            }
            if keys.is_empty() {
                // No usable URI on the diagnostic itself — fall back to
                // the last edited document so we never silently drop it.
                if let Some(fallback) = self.last_changed_uri_key.clone() {
                    keys.push(fallback);
                } else {
                    continue;
                }
            }

            let mapped = map_diagnostic(diagnostic, self.wrapped.as_ref());
            for key in keys {
                by_key.entry(key).or_default().push(mapped.clone());
            }
        }

        by_key
    }

    /// Run semantic analysis and return only the diagnostics that
    /// should be shown for `uri`. Retained as a thin wrapper over
    /// `semantic_all` for tests and callers that only need a single
    /// file's diagnostics.
    #[cfg_attr(
        not(test),
        expect(
            dead_code,
            reason = "only the tests ask for the diagnostics of one file"
        )
    )]
    pub(crate) fn semantic(&mut self, uri: &Uri) -> Vec<lsp_types::Diagnostic> {
        let mut by_key = self.semantic_all();
        by_key.remove(&UriKey::from_uri(uri)).unwrap_or_default()
    }

    /// Returns document symbols for the given URI.
    ///
    /// This provides an outline of types defined in the document for
    /// VS Code's Outline panel and "Go to Symbol" feature.
    pub(crate) fn document_symbols(&self, uri: &Uri) -> DocumentSymbolResponse {
        let path = match to_path_buf(uri) {
            Ok(path) => path,
            Err(_) => {
                error!("URL must be convertible to a file path {}", uri.as_str());
                return DocumentSymbolResponse::Nested(vec![]);
            }
        };

        let file_id = FileId::from_path(&path);

        let context = match self.wrapped.semantic_context() {
            Some(ctx) => ctx,
            None => return DocumentSymbolResponse::Nested(vec![]),
        };

        let source = match self.wrapped.find(&file_id) {
            Some(src) => src,
            None => return DocumentSymbolResponse::Nested(vec![]),
        };

        let contents = source.as_string();

        let mut symbols = Vec::new();

        // User-defined types (structures, enumerations, arrays, ...)
        // The extractor already excludes elementary types and the
        // function-block / function entries that types() carries
        // alongside the per-POU registry, so we don't need to filter
        // those again here.
        for view in context.user_defined_types() {
            if view.attributes.span().file_id != file_id {
                continue;
            }
            let range = range_of(
                contents,
                view.attributes.span().start,
                view.attributes.span().end,
            );
            symbols.push(document_symbol(
                view.name.to_string(),
                Some(format!("{:?}", view.attributes.type_category)),
                type_kind_to_symbol_kind(view.kind),
                range,
            ));
        }

        // Function blocks (declared by the user; not surfaced via
        // user-defined types since the function-block name is registered
        // separately in the symbol environment).
        for view in context.function_blocks() {
            if view.info.span.file_id != file_id {
                continue;
            }
            let range = range_of(contents, view.info.span.start, view.info.span.end);
            symbols.push(document_symbol(
                view.name.to_string(),
                None,
                SymbolKind::CLASS,
                range,
            ));
        }

        // User-defined functions
        for view in context.user_defined_functions() {
            if view.signature.span.file_id != file_id {
                continue;
            }
            let range = range_of(contents, view.signature.span.start, view.signature.span.end);
            symbols.push(document_symbol(
                view.signature.name.to_string(),
                view.signature
                    .return_type
                    .as_ref()
                    .map(|t| format!("{:?}", t)),
                SymbolKind::FUNCTION,
                range,
            ));
        }

        DocumentSymbolResponse::Nested(symbols)
    }

    /// Compile source and start a VM execution session.
    ///
    /// Returns initial metadata. Subsequent calls to `step()` execute
    /// scan cycles and return variable values.
    pub(crate) fn run_load(&mut self, source: &str, cycle_time_us: u64) -> RunResult {
        // Tear down any existing session
        self.runner = None;

        match VmRunner::load(source, cycle_time_us, &self.compiler_options) {
            Ok((runner, result)) => {
                self.runner = Some(runner);
                result
            }
            Err(err) => err,
        }
    }

    /// Execute N scan cycles in the current session.
    pub(crate) fn run_step(&mut self, scans: u32) -> RunResult {
        match self.runner.as_mut() {
            Some(runner) => runner.step(scans),
            None => RunResult {
                ok: false,
                variables: vec![],
                total_scans: 0,
                error: Some("No program loaded. Send ironplc/run first.".to_string()),
            },
        }
    }

    /// Stop the current execution session and release resources.
    pub(crate) fn run_stop(&mut self) -> RunResult {
        self.runner = None;
        RunResult {
            ok: true,
            variables: vec![],
            total_scans: 0,
            error: None,
        }
    }
}

/// A document symbol that spans `range` and has no children.
#[expect(deprecated, reason = "the struct has a deprecated field to set")]
fn document_symbol(
    name: String,
    detail: Option<String>,
    kind: SymbolKind,
    range: lsp_types::Range,
) -> DocumentSymbol {
    DocumentSymbol {
        name,
        detail,
        kind,
        tags: None,
        deprecated: None,
        range,
        selection_range: range,
        children: None,
    }
}

/// Map the analyzer's neutral `TypeSymbolKind` to LSP's `SymbolKind`.
fn type_kind_to_symbol_kind(kind: TypeSymbolKind) -> SymbolKind {
    match kind {
        TypeSymbolKind::Structure => SymbolKind::STRUCT,
        TypeSymbolKind::Enumeration => SymbolKind::ENUM,
        TypeSymbolKind::FunctionBlock => SymbolKind::CLASS,
        TypeSymbolKind::Function => SymbolKind::FUNCTION,
        TypeSymbolKind::Array => SymbolKind::ARRAY,
        TypeSymbolKind::Subrange => SymbolKind::TYPE_PARAMETER,
        TypeSymbolKind::String | TypeSymbolKind::Reference | TypeSymbolKind::Alias => {
            SymbolKind::VARIABLE
        }
    }
}

/// Convert the bytes `start..end` of `contents` to an LSP Range, the one place
/// a range is converted for the editor. An empty range (where something is
/// missing) is widened to a character the editor can draw, see
/// [`LineColumn::editor_range`]; the span of the diagnostic stays empty.
fn range_of(contents: &str, start: usize, end: usize) -> lsp_types::Range {
    let (start, end) = LineColumn::editor_range(contents, start, end);
    lsp_types::Range::new(
        lsp_types::Position::new(start.line, start.column),
        lsp_types::Position::new(end.line, end.column),
    )
}

/// Convert diagnostic type into the LSP diagnostic type.
fn map_diagnostic(
    diagnostic: ironplc_dsl::diagnostic::Diagnostic,
    project: &dyn Project,
) -> lsp_types::Diagnostic {
    let description = diagnostic.description();
    let range = map_label(&diagnostic.primary, project);

    // `channel=extension` attributes the arrival to the editor integration (the
    // language server surfaces these diagnostics; we do not assume the editor is
    // VS Code). The shared builder supplies the rest, including the reference
    // section: this used to be written out as `compiler`, which linked any
    // non-`P` diagnostic to a page that does not exist.
    let url_string = diagnostic.help_url(env!("CARGO_PKG_VERSION"), "extension");
    let code_description = match Uri::from_str(&url_string) {
        Ok(url) => Some(CodeDescription { href: url }),
        Err(_) => None,
    };

    let related_information = if diagnostic.secondary.is_empty() {
        None
    } else {
        Some(
            diagnostic
                .secondary
                .iter()
                .filter_map(|label| {
                    let uri_str = format!("file://{}", label.file_id);
                    let uri = Uri::from_str(&uri_str).ok()?;
                    let range = map_label(label, project);
                    Some(DiagnosticRelatedInformation {
                        location: Location { uri, range },
                        message: label.message.clone(),
                    })
                })
                .collect(),
        )
    };

    let mut message = format!("{description}: {} ", diagnostic.primary.message);
    for note in diagnostic.help() {
        message.push_str(&format!("\n{note}"));
    }

    lsp_types::Diagnostic {
        range,
        severity: Some(DiagnosticSeverity::ERROR),
        code: Some(NumberOrString::String(diagnostic.code)),
        code_description,
        source: Some("ironplc".into()),
        message,
        related_information,
        tags: None,
        data: None,
    }
}

/// Convert the diagnostic label into the LSP range type.
fn map_label(label: &ironplc_dsl::diagnostic::Label, project: &dyn Project) -> lsp_types::Range {
    let file_id = &label.file_id;
    let contents = project.find(file_id);

    if let Some(contents) = contents {
        return range_of(
            contents.as_string(),
            label.location.start,
            label.location.end,
        );
    }
    lsp_types::Range::new(
        lsp_types::Position::new(0, 0),
        lsp_types::Position::new(0, 0),
    )
}

#[cfg(test)]
mod tests;
