//! Provides definition for diagnostics, which are normally errors and warnings
//! associated with compilation.
//!
//! # Diagnostic Creation Guidelines
//!
//! When creating diagnostics:
//! 1. Make the diagnostic from a row of the message dictionary with
//!    [`Diagnostic::text`] or [`Diagnostic::text_in_file`]
//! 2. Primary label points to the main error location
//! 3. Secondary labels point to different, related locations
//! 4. Labels describe what's at each location, not how to fix issues
//!
//! There exist crates that make this easy, but we need different information
//! for different integrations and there is no one crate that does it all
//! (especially one that works for both command line and language server
//! protocol).
pub use ironplc_problems::Severity;
use ironplc_problems::{CodelessText, Problem, Text};

mod area;
mod editor_range;
mod forms;
mod handling;
mod run;
mod settings;

pub use area::{Area, ShownIn};
pub use forms::ScriptMessage;
pub use handling::MessageHandling;
pub use run::{counts, Counts, Summary, CHECK_SUMMARY, COMPILE_SUMMARY};
pub use settings::{MessageSettings, WarningList};

use crate::common::TypeName;
use crate::core::{FileId, Id, Located, SourceSpan};

/// A position marker that only has an offset in a file.
#[derive(Debug, Clone)]
pub struct Location {
    /// Byte offset from start of string (0-indexed)
    pub start: usize,
    /// Byte offset from end of string (0-indexed)
    pub end: usize,
}

/// A 0-indexed line and column within a source file.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct LineColumn {
    /// 0-indexed line number.
    pub line: u32,
    /// 0-indexed column number (code points, not bytes).
    pub column: u32,
}

impl LineColumn {
    /// Converts a byte offset in `source` to a 0-indexed line and column.
    ///
    /// The column is measured in code points. Offsets beyond the end of
    /// `source` are clamped to the end of `source`.
    pub fn from_offset(source: &str, offset: usize) -> Self {
        let clamped = offset.min(source.len());
        let mut line: u32 = 0;
        let mut column: u32 = 0;
        for ch in source[..clamped].chars() {
            if ch == '\n' {
                line += 1;
                column = 0;
            } else {
                column += 1;
            }
        }
        LineColumn { line, column }
    }
}

/// A label that refers to some range in a file and possibly associated
/// with a message related to that range.
///
/// Normally this indicates the location of an error or warning along with a
/// text message describing that position.
#[derive(Debug, Clone)]
pub struct Label {
    /// The position of label.
    pub location: Location,

    /// Identifier for the file.
    pub file_id: FileId,

    /// A message describing this label.
    pub message: String,
}

impl Label {
    /// Creates a label pointing to a source span with a descriptive message.
    ///
    /// The message should describe **what is at this location**, not provide
    /// explanations or fix guidance.
    ///
    /// # Examples
    /// ```text
    /// Label::span(name.span(), "Type declaration")     // ✅ What's there
    /// Label::span(base.span(), "Base type")           // ✅ What's there
    /// Label::span(name.span(), "Fix by adding type")  // ❌ Fix guidance
    /// ```
    pub fn span(span: SourceSpan, message: impl Into<String>) -> Self {
        Self {
            location: Location {
                start: span.start,
                end: span.end,
            },
            file_id: span.file_id,
            message: message.into(),
        }
    }

    /// A "position" that a file in it's entirety rather that a particular
    /// line number.
    pub fn file(file_id: impl Into<FileId>, message: impl Into<String>) -> Self {
        Self {
            location: Location { start: 0, end: 0 },
            file_id: file_id.into(),
            message: message.into(),
        }
    }
}

/// A diagnostic. Diagnostic have a code that is indicative of the category,
/// a primary location and possibly non-zero set of secondary location.
///
/// A message that has no code (a limit line, a summary line) is a diagnostic
/// too, made by [`Diagnostic::codeless`]: its `code` is empty and
/// [`Diagnostic::has_code`] is false. Both forms of a message and the editor
/// mapping treat it as they treat a coded one.
#[derive(Debug, Clone)]
pub struct Diagnostic {
    /// A normally unique value describing the type of diagnostic. It is empty
    /// for a message that has no code; ask [`Diagnostic::has_code`] before
    /// treating it as a code.
    pub code: String,

    /// The key of the row of `codeless-texts.csv` that a message without a
    /// code was made from, and `None` for a message with a code. It is the
    /// identity of such a message in place of a code.
    codeless: Option<&'static str>,

    /// The message category of the diagnostic. It is the severity of the problem
    /// code and is set only where the diagnostic is made, so a call site cannot
    /// decide it; `suppress` and `raise` are the two changes made afterwards.
    severity: Severity,

    /// The CODESYS code that reports the same condition as the problem, from
    /// its row of `problem-codes.csv`.
    codesys: Option<&'static str>,

    /// The areas the message is shown in. It is set where the message is made,
    /// from whether the message has a position, and never changes.
    shown_in: ShownIn,

    description: String,

    /// The primary or first diagnostic.
    pub primary: Label,

    /// Additional descriptions to the constant description.
    pub described: Vec<String>,

    /// Guidance on how to resolve the problem. Unlike labels (which describe
    /// *what* is at a location), help notes explain *how to fix* the problem.
    /// Rendered as trailing notes by the CLI and appended to the message by
    /// the language server.
    pub help: Vec<String>,

    /// Additional information about the diagnostic.
    pub secondary: Vec<Label>,

    /// Rust source file that produced this diagnostic (from `file!()` macro).
    pub source_file: Option<String>,

    /// Rust source line that produced this diagnostic (from `line!()` macro).
    pub source_line: Option<u32>,
}

/// How many of `diagnostics` stop the build.
///
/// This is the count a failure message reports: a warning or an information
/// message is shown and is not a problem the program is rejected for.
pub fn stops_build_count(diagnostics: &[Diagnostic]) -> usize {
    diagnostics.iter().filter(|d| d.stops_build()).count()
}

/// Whether any of `diagnostics` stops the build. This is the one test that
/// decides that a program is rejected -- code generation does not run, a
/// command exits with a failure, an editor request reports `ok: false` -- so a
/// place that gates on failure asks this and not whether `diagnostics` is empty.
pub fn any_stops_build(diagnostics: &[Diagnostic]) -> bool {
    diagnostics.iter().any(Diagnostic::stops_build)
}

/// The diagnostics of a stage, cut short as CODESYS cuts a build short at a
/// fatal error: when any is a `FatalError`, the diagnostics found so far are
/// dropped and only the fatal ones remain, and the caller runs no later stage.
/// Without a fatal error the diagnostics are returned as they are.
pub fn stop_at_fatal(diagnostics: Vec<Diagnostic>) -> Vec<Diagnostic> {
    if has_fatal(&diagnostics) {
        diagnostics
            .into_iter()
            .filter(|d| d.severity == Severity::FatalError)
            .collect()
    } else {
        diagnostics
    }
}

/// Whether any of `diagnostics` is a `FatalError`, after which no later stage
/// runs.
pub fn has_fatal(diagnostics: &[Diagnostic]) -> bool {
    diagnostics
        .iter()
        .any(|d| d.severity == Severity::FatalError)
}

/// Formats the label message for a "not implemented" diagnostic from the
/// compiler location that produced it.
fn not_implemented_message(caller: &std::panic::Location<'_>) -> String {
    format!("Not implemented at {}#L{}", caller.file(), caller.line())
}

impl Diagnostic {
    /// Creates a diagnostic from the problem code and with the specified label.
    ///
    /// This is the shared body of the public constructors and is not public
    /// itself: a label text that is not a row of the message dictionary
    /// (`problem-texts.csv`) cannot be paired with a problem from outside this
    /// module. Use [`Diagnostic::text`] or [`Diagnostic::text_in_file`] for a
    /// problem of the program, and [`Diagnostic::not_implemented`],
    /// [`Diagnostic::not_supported`], [`Diagnostic::internal_error_at`] or the
    /// `todo_with_*` constructors for a gap in the compiler.
    fn problem(problem: Problem, primary: Label) -> Self {
        Self {
            code: problem.code().to_string(),
            codeless: None,
            severity: problem.severity(),
            codesys: problem.codesys(),
            shown_in: ShownIn::EVERYWHERE,
            description: problem.message().to_string(),
            primary,
            described: vec![],
            help: vec![],
            secondary: vec![],
            source_file: None,
            source_line: None,
        }
    }

    /// Creates a message that has no code from a text of the dictionary
    /// (`ironplc_problems::codeless`): a limit line or a summary line.
    ///
    /// The row declares the category and the text, so the message takes both
    /// from one place and no call site writes either. The message is not about
    /// a place in a file: its primary label is empty and points nowhere, so it is
    /// shown in the Build area alone ([`ShownIn::NO_POSITION`], MSG-022). A
    /// message without a code that has a position -- the message of a pragma in
    /// the source -- is made by its own constructor, which sets
    /// [`ShownIn::EVERYWHERE`].
    pub fn codeless(text: CodelessText) -> Self {
        let (key, severity, message) = text.into_parts();
        Self {
            code: String::new(),
            codeless: Some(key),
            severity,
            codesys: None,
            shown_in: ShownIn::NO_POSITION,
            description: message,
            primary: Label::file(FileId::default(), ""),
            described: vec![],
            help: vec![],
            secondary: vec![],
            source_file: None,
            source_line: None,
        }
    }

    /// The key of the row of `codeless-texts.csv` a message without a code was
    /// made from, and `None` for a message with a code.
    pub fn codeless_key(&self) -> Option<&'static str> {
        self.codeless
    }

    /// Whether the diagnostic has a code. A message made by
    /// [`Diagnostic::codeless`] has none; every other diagnostic has.
    pub fn has_code(&self) -> bool {
        self.codeless.is_none()
    }

    /// Creates a diagnostic from a text of the dictionary
    /// (`ironplc_problems::text`) and the place it is about.
    ///
    /// The text carries its problem, so the code and the label text come from
    /// one row of `problem-texts.csv` and cannot be paired wrongly. The span
    /// is the place of the primary label.
    pub fn text(span: SourceSpan, text: Text) -> Self {
        let (problem, message) = text.into_parts();
        Self::problem(problem, Label::span(span, message))
    }

    /// Creates a diagnostic from a text of the dictionary
    /// (`ironplc_problems::text`) that is about a file in its entirety rather
    /// than a particular span of it.
    ///
    /// This is `Diagnostic::text` for a label made by `Label::file`, with the
    /// code and the label text from one row.
    pub fn text_in_file(file_id: impl Into<FileId>, text: Text) -> Self {
        let (problem, message) = text.into_parts();
        Self::problem(problem, Label::file(file_id, message))
    }

    /// Creates a "todo" diagnostic associated with a file and line in the Rust
    /// source code. Also provides a location in IEC 61131-3 associated with the
    /// todo (but is not necessarily the origin).
    ///
    /// Unlike other uses of problem, the location in this is related to the compiler
    /// rather than the IEC 61131-3 source.
    ///
    /// The location is captured via `#[track_caller]`, so no `file!()`/`line!()`
    /// need to be passed.
    #[track_caller]
    #[expect(
        deprecated,
        reason = "this is a sanctioned constructor of the deprecated compiler-located problem variants"
    )]
    pub fn todo_with_id(id: &Id) -> Self {
        let caller = std::panic::Location::caller();
        Diagnostic::problem(
            Problem::NotImplemented,
            Label::span(id.span(), not_implemented_message(caller)),
        )
        .with_source(caller.file(), caller.line())
    }

    /// Creates a "todo" diagnostic associated with a file and line in the Rust
    /// source code. Also provides a location in IEC 61131-3 associated with the
    /// todo (but is not necessarily the origin).
    ///
    /// Unlike other uses of problem, the location in this is related to the compiler
    /// rather than the IEC 61131-3 source.
    ///
    /// The location is captured via `#[track_caller]`, so no `file!()`/`line!()`
    /// need to be passed.
    #[track_caller]
    #[expect(
        deprecated,
        reason = "this is a sanctioned constructor of the deprecated compiler-located problem variants"
    )]
    pub fn todo_with_type(ty: &TypeName) -> Self {
        let caller = std::panic::Location::caller();
        Diagnostic::problem(
            Problem::NotImplemented,
            Label::span(ty.span(), not_implemented_message(caller)),
        )
        .with_source(caller.file(), caller.line())
    }

    /// Creates a "todo" diagnostic associated with a file and line in the Rust
    /// source code. Also provides a location in IEC 61131-3 associated with the
    /// todo (but is not necessarily the origin).
    ///
    /// Unlike other uses of problem, the location in this is related to the compiler
    /// rather than the IEC 61131-3 source.
    ///
    /// The location is captured via `#[track_caller]`, so no `file!()`/`line!()`
    /// need to be passed.
    #[track_caller]
    #[expect(
        deprecated,
        reason = "this is a sanctioned constructor of the deprecated compiler-located problem variants"
    )]
    pub fn todo_with_span(span: SourceSpan) -> Self {
        let caller = std::panic::Location::caller();
        Diagnostic::problem(
            Problem::NotImplemented,
            Label::span(span, not_implemented_message(caller)),
        )
        .with_source(caller.file(), caller.line())
    }

    /// Creates a P9999 (NotImplemented) diagnostic that automatically records
    /// the compiler `file#Lline` of the call site as its source location.
    ///
    /// Use this for P9999: the compiler location is what the telemetry
    /// dashboards rank to point maintainers at the unimplemented code. Unlike
    /// `todo*()`, the caller supplies the primary label, so a descriptive
    /// message and IEC 61131-3 span are preserved.
    ///
    /// The location is captured via `#[track_caller]`, so no `file!()`/`line!()`
    /// need to be passed.
    #[track_caller]
    #[expect(
        deprecated,
        reason = "this is a sanctioned constructor of the deprecated compiler-located problem variants"
    )]
    pub fn not_implemented(primary: Label) -> Self {
        let caller = std::panic::Location::caller();
        Diagnostic::problem(Problem::NotImplemented, primary)
            .with_source(caller.file(), caller.line())
    }

    /// Creates a P9997 (NotSupported) diagnostic that automatically records
    /// the compiler `file#Lline` of the call site as its source location.
    ///
    /// Use this — rather than [`Diagnostic::not_implemented`] — for a capability
    /// the compiler deliberately does not offer: a fixed limit of the bytecode
    /// format, or a construct that is not planned. P9999 promises "not yet";
    /// P9997 does not, so a program that hits it needs to change rather than
    /// wait for a later release.
    ///
    /// The location is captured via `#[track_caller]`, so no `file!()`/`line!()`
    /// need to be passed.
    #[track_caller]
    #[expect(
        deprecated,
        reason = "this is a sanctioned constructor of the deprecated compiler-located problem variants"
    )]
    pub fn not_supported(primary: Label) -> Self {
        let caller = std::panic::Location::caller();
        Diagnostic::problem(Problem::NotSupported, primary)
            .with_source(caller.file(), caller.line())
    }

    /// Creates an "internal error" diagnostic associated with a file and line in the Rust
    /// source code.
    ///
    /// Unlike other uses of problem, the location in this is related to the compiler
    /// rather than the IEC 61131-3 source.
    ///
    /// The location is captured via `#[track_caller]`, so no `file!()`/`line!()`
    /// need to be passed.
    #[track_caller]
    #[expect(
        deprecated,
        reason = "this is a sanctioned constructor of the deprecated compiler-located problem variants"
    )]
    pub fn internal_error() -> Self {
        let caller = std::panic::Location::caller();
        Diagnostic::problem(
            Problem::InternalError,
            Label::span(
                SourceSpan::default(),
                format!(
                    "Internal error at {}#L{} indicates a bug in the compiler",
                    caller.file(),
                    caller.line()
                ),
            ),
        )
        .with_source(caller.file(), caller.line())
    }

    /// Creates a P9998 (InternalError) diagnostic that automatically records
    /// the compiler `file#Lline` of the call site as its source location, while
    /// letting the caller supply a descriptive primary label.
    ///
    /// Use `internal_error()` instead when no custom label is needed.
    ///
    /// The location is captured via `#[track_caller]`, so no `file!()`/`line!()`
    /// need to be passed.
    #[track_caller]
    #[expect(
        deprecated,
        reason = "this is a sanctioned constructor of the deprecated compiler-located problem variants"
    )]
    pub fn internal_error_at(primary: Label) -> Self {
        let caller = std::panic::Location::caller();
        Diagnostic::problem(Problem::InternalError, primary)
            .with_source(caller.file(), caller.line())
    }

    /// Adds to the problem description (primary text) additional context
    /// about the problem.
    ///
    /// This is similar to adding primary and second items except that this
    /// forms part of the main description and does not need to be related to
    /// a position in a source file.
    pub fn with_context(mut self, description: &str, item: &String) -> Self {
        self.described.push(format!("{description}={item}"));
        self
    }

    pub fn with_context_id(mut self, description: &str, item: &Id) -> Self {
        self.described.push(format!("{description}={item}"));
        self
    }

    pub fn with_context_type(mut self, description: &str, item: &TypeName) -> Self {
        self.described.push(format!("{description}={item}"));
        self
    }

    /// Adds a secondary label pointing to a related location.
    ///
    /// Secondary labels must point to **different spans** than the primary label
    /// and should describe what is located at those spans.
    ///
    /// # Examples
    /// ```text
    /// // ✅ Correct: Different spans, describes locations
    /// Diagnostic::text(decl_span, text::parent_type_not_declared_type_alias())
    /// .with_secondary(Label::span(base_span, "Base type"))
    ///
    /// // ❌ Wrong: Same span or fix guidance
    /// .with_secondary(Label::span(decl_span, "Add missing type"))
    /// ```
    pub fn with_secondary(mut self, label: Label) -> Self {
        self.secondary.push(label);
        self
    }

    /// Adds a help note describing how to resolve the problem.
    ///
    /// Help notes are for fix guidance (e.g. "use `(* *)` comments"), which is
    /// intentionally kept out of labels. They are rendered as trailing notes by
    /// the command line and appended to the message by the language server.
    pub fn with_help(mut self, help: impl Into<String>) -> Self {
        self.help.push(help.into());
        self
    }

    /// Returns the help notes describing how to resolve the problem.
    pub fn help(&self) -> &[String] {
        &self.help
    }

    /// Sets the Rust source file and line that produced this diagnostic.
    ///
    /// This is typically called with `file!()` and `line!()` to record
    /// where in the compiler the diagnostic was generated.
    pub fn with_source(mut self, file: &str, line: u32) -> Self {
        self.source_file = Some(file.to_string());
        self.source_line = Some(line);
        self
    }

    /// The category the diagnostic's problem code declares, or the one it was
    /// suppressed to.
    pub fn severity(&self) -> Severity {
        self.severity
    }

    /// Whether the message is shown in the area. A message is written to an area
    /// only if it is shown there ([`Area::keep`]); the answer is fixed when the
    /// message is made and is not derived from its place.
    pub fn shown_in(&self, area: Area) -> bool {
        self.shown_in.contains(area)
    }

    /// Whether the diagnostic stops the build, which is the one question that
    /// decides whether a program is rejected: a warning or an information
    /// message is shown and does not stop it. Ask this, not whether there are
    /// any diagnostics.
    pub fn stops_build(&self) -> bool {
        self.severity.stops_build()
    }

    /// Suppresses the diagnostic: a warning becomes a suppressed warning and an
    /// information message a suppressed information message, which are not
    /// shown. A category that cannot be suppressed stays, as in CODESYS. This is
    /// the change of category that the handling `Suppress` makes
    /// ([`Diagnostic::handle`]); with [`Diagnostic::raise`] it is one of the two
    /// ways to change the category of a diagnostic after it is made.
    pub fn suppress(mut self) -> Self {
        self.severity = self.severity.suppressed();
        self
    }

    /// Reports a warning as an error: the category becomes `Error` and the code,
    /// the texts and the places stay (`SeverityExtensions.GetSeverity`). A
    /// diagnostic of any other category stays as it is, so this never lowers an
    /// error. It is made by a setting that lists the warning
    /// ([`MessageSettings`]) and by nothing else.
    pub fn raise(mut self) -> Self {
        self.severity = self.severity.raised();
        self
    }

    /// Gives the diagnostic a category no problem code declares yet, so that a
    /// test of a consumer can build a fatal error or a warning.
    #[cfg(feature = "test-support")]
    pub fn with_severity_for_test(mut self, severity: Severity) -> Self {
        self.severity = severity;
        self
    }

    /// Returns the description for the diagnostic. This may add in other
    /// data in addition that is part of the diagnostic.
    pub fn description(&self) -> String {
        if self.described.is_empty() {
            self.description.clone()
        } else {
            format!("{} ({})", self.description, self.described.join(", "))
        }
    }

    /// The files the diagnostic points into, each once: the primary label's
    /// file first, then the files of the secondary labels in the order the
    /// labels were added. The order is the order a person reads the diagnostic
    /// in, and the same on every run.
    pub fn file_ids(&self) -> Vec<&FileId> {
        // A message without a code is about no place, so it points into no file.
        let mut file_ids = if self.has_code() {
            vec![&self.primary.file_id]
        } else {
            vec![]
        };

        for secondary_item in self.secondary.iter() {
            if !file_ids.contains(&&secondary_item.file_id) {
                file_ids.push(&secondary_item.file_id);
            }
        }

        file_ids
    }

    /// The www.ironplc.com documentation URL for this diagnostic's problem code,
    /// tagged with the `channel` it is being surfaced through, and carrying the
    /// Rust source location when the diagnostic recorded one.
    ///
    /// See [`problem_help_url`], which this delegates to; the only thing added
    /// here is reading `source_file`/`source_line` off the diagnostic.
    pub fn help_url(&self, version: &str, channel: &str) -> String {
        problem_help_url(
            &self.code,
            version,
            channel,
            self.source_file.as_deref(),
            self.source_line,
        )
    }
}

/// The documented problem-code families: a code's leading letter paired with
/// the www.ironplc.com reference section its page lives in.
///
/// This is the table [`docs_section`] reads. It is a table rather than a `match`
/// arm so that it can also be handed to callers that cannot call into Rust — the
/// playground's front end asks the WASM crate for it instead of testing code
/// prefixes itself, which is how a fourth copy of this mapping was removed.
pub const DOCS_SECTIONS: &[(char, &str)] = &[
    ('P', "compiler"),
    ('V', "runtime"),
    ('E', "editor"),
    ('L', "ironlink"),
];

/// The section reported for a code whose family is not in [`DOCS_SECTIONS`].
pub const UNKNOWN_DOCS_SECTION: &str = "unknown";

/// The www.ironplc.com reference section that documents a problem code, derived
/// from its leading letter: `P####` → `compiler`, `V####` → `runtime`,
/// `E####` → `editor`, `L####` → `ironlink`.
///
/// Returns [`UNKNOWN_DOCS_SECTION`] for any unrecognized prefix rather than
/// guessing a section: a `…/reference/unknown/problems/…` URL 404s honestly
/// instead of confidently pointing at the wrong docs. The
/// `docs_section_covers_every_documented_code` test in this module walks the
/// docs tree and fails if any documented code's prefix is left unmapped, so
/// adding a new code family without updating [`DOCS_SECTIONS`] is caught at test
/// time rather than shipping a broken link.
pub fn docs_section(code: &str) -> &'static str {
    let Some(prefix) = code.chars().next() else {
        return UNKNOWN_DOCS_SECTION;
    };
    DOCS_SECTIONS
        .iter()
        .find(|(letter, _)| *letter == prefix)
        .map_or(UNKNOWN_DOCS_SECTION, |(_, section)| *section)
}

/// Builds the www.ironplc.com documentation URL for a problem `code`, tagged
/// with the `channel` it was surfaced through.
///
/// Every channel — CLI, LSP/editor, MCP — funnels through here rather than
/// formatting the URL itself, so the section can only ever come from
/// [`docs_section`]. The language server used to format its own URL with the
/// section written out as `compiler`, which sent any non-`P` diagnostic to a
/// page that does not exist; a single builder makes that class of bug
/// unrepresentable rather than merely fixed. The
/// `problem_help_url_is_the_only_place_rust_names_the_docs_host` test fails if
/// another Rust module starts minting these URLs again.
///
/// The URL is a working docs link regardless of the query string. `version`
/// carries the client version, which the out-of-date banner in
/// docs/_static/version-check.js reads, and PostHog captures `version` and
/// `channel` as breakdown dimensions via `custom_campaign_params` in
/// docs/_static/posthog-init.js, so we can see where and on which version
/// people reach these pages. `source_file`/`source_line` — the Rust source
/// location that raised the diagnostic — are appended when present so a
/// maintainer can see what a remote user hit.
pub fn problem_help_url(
    code: &str,
    version: &str,
    channel: &str,
    source_file: Option<&str>,
    source_line: Option<u32>,
) -> String {
    let mut url = format!(
        "https://www.ironplc.com/reference/{section}/problems/{code}.html?version={version}&channel={channel}",
        section = docs_section(code),
    );
    if let Some(file) = source_file {
        url.push_str(&format!("&file={file}"));
    }
    if let Some(line) = source_line {
        url.push_str(&format!("&line={line}"));
    }
    url
}

#[cfg(test)]
mod tests;
