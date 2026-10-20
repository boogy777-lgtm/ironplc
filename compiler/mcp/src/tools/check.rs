//! The `check` MCP tool.
//!
//! Runs parse and full semantic analysis, returning structured diagnostics.

use ironplc_analyzer::SemanticContext;
use ironplc_dsl::common::Library;
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::{any_stops_build, Area, Diagnostic, MessageSettings, ScriptMessage};
use ironplc_project::project::{MemoryBackedProject, Project};
use serde::Serialize;

use super::common::{
    parse_options, project_name, script_message, serialize_message, validate_sources, SourceInput,
};

/// Response returned by the `check` tool.
///
/// `diagnostics` holds the messages of the run as the command line frames them:
/// the problems and then the line "Compile complete -- N errors, M warnings".
/// The program with no problem has that line alone, so the list is not empty
/// for a clean program; `ok` says whether a problem stops the build.
#[derive(Debug, Serialize)]
pub struct CheckResponse {
    pub ok: bool,
    pub diagnostics: Vec<serde_json::Value>,
}

/// What a check finds: whether no message stops the build, and every message,
/// of every category, suppressed ones included. The tools `check`,
/// `get_message_objects` and `get_messages` read it.
pub struct Checked {
    pub ok: bool,
    /// The messages of the run as the area frames them: for the Build area the
    /// problems and then the line that closes the check, "Compile complete",
    /// which CODESYS stores among the messages of a compile as a `Text`
    /// (`Summary::close`, the rule the command line uses). An area that has no
    /// closing line (`Area::check_summary`) holds the problems alone.
    pub messages: Vec<ScriptMessage>,
}

impl Checked {
    /// The messages as the JSON the tools return in their `diagnostics`.
    pub fn diagnostics(&self) -> Vec<serde_json::Value> {
        self.messages.iter().map(serialize_message).collect()
    }
}

/// A run of the checker over the sources of a request: validate the sources,
/// read the options, build the project, parse and analyze.
///
/// This is the one prelude of every tool that returns the diagnostics of a run
/// (`check`, `symbols`, `project_manifest`, `project_io`, `types_all`,
/// `pou_scope`, `pou_lineage`, and the queries of the messages), and the one
/// place that closes it: [`CheckRun::finish`] frames the messages as the command
/// line does (`Summary::close`: the closing line "Compile complete", the
/// application named by `Project::application_name`, the settings of the run), on
/// every path, including an invalid input. A tool keeps only its own payload and
/// asks the project of the run for it; none calls `project.semantic` and
/// serializes the messages itself, so none can return them unframed.
pub struct CheckRun<'a> {
    /// The project after the run, or `None` when the input was invalid and
    /// nothing ran.
    project: Option<MemoryBackedProject>,
    /// The messages the run kept, or the problems of the invalid input.
    diagnostics: Vec<Diagnostic>,
    area: Area,
    application: String,
    settings: MessageSettings,
    /// The sources, to place a message in them; empty for an invalid input.
    sources: &'a [SourceInput],
}

impl<'a> CheckRun<'a> {
    /// Validates the inputs, then parses and analyzes the sources. The run
    /// writes `area`: the area keeps its messages and decides the line that
    /// closes it.
    pub fn start(
        sources: &'a [SourceInput],
        options_value: &serde_json::Value,
        area: Area,
    ) -> Self {
        let fallback = project_name(sources);
        let invalid = |diagnostics: Vec<Diagnostic>| CheckRun {
            project: None,
            diagnostics,
            area,
            application: fallback.clone(),
            settings: MessageSettings::default(),
            sources: &[],
        };

        // Validate sources
        let source_errors = validate_sources(sources);
        if !source_errors.is_empty() {
            return invalid(source_errors);
        }

        // Parse options
        let options = match parse_options(options_value) {
            Ok(opts) => opts,
            Err(errs) => return invalid(errs),
        };

        // Construct a fresh in-memory project (REQ-ARC-mcp-010)
        let mut project = MemoryBackedProject::new(options);

        // Load sources (REQ-ARC-mcp-011)
        for src in sources {
            let file_id = FileId::from_string(&src.name);
            project.add_source(file_id, src.content.clone());
        }

        // Run parse + full semantic analysis
        let diagnostics = project.semantic(area);
        let application = project.application_name(&fallback);
        CheckRun {
            project: Some(project),
            diagnostics,
            area,
            application,
            settings: options.messages,
            sources,
        }
    }

    /// Whether the input was invalid (sources or options), so that nothing ran.
    pub fn is_invalid_input(&self) -> bool {
        self.project.is_none()
    }

    /// The semantic context of the run: `None` when the input was invalid or when
    /// type resolution failed (`Project::semantic_context`).
    pub fn semantic_context(&self) -> Option<&SemanticContext> {
        self.project
            .as_ref()
            .and_then(|project| project.semantic_context())
    }

    /// The analyzed library of the run, under the same conditions.
    pub fn analyzed_library(&self) -> Option<&Library> {
        self.project
            .as_ref()
            .and_then(|project| project.analyzed_library())
    }

    /// The messages of the run, framed.
    pub fn finish(&self) -> Checked {
        self.finish_with(vec![])
    }

    /// The messages of the run and then `extra`, which the tool found after the
    /// run (a POU that is not declared), framed: the closing line counts them, and
    /// `ok` is false when one stops the build.
    pub fn finish_with(&self, extra: Vec<Diagnostic>) -> Checked {
        let mut diagnostics = self.diagnostics.clone();
        diagnostics.extend(extra);
        let framed = match self.area.check_summary() {
            Some(summary) => summary.close(&self.application, &self.settings, diagnostics),
            None => diagnostics,
        };
        Checked {
            ok: !any_stops_build(&framed),
            messages: framed
                .iter()
                .map(|diagnostic| script_message(diagnostic, self.sources))
                .collect(),
        }
    }
}

/// Validates the inputs, then parses and analyzes the sources, and frames the
/// messages ([`CheckRun`]).
pub fn check(sources: &[SourceInput], options_value: &serde_json::Value, area: Area) -> Checked {
    CheckRun::start(sources, options_value, area).finish()
}

/// Builds the check response from raw inputs.
pub fn build_response(sources: &[SourceInput], options_value: &serde_json::Value) -> CheckResponse {
    let checked = check(sources, options_value, Area::Build);
    CheckResponse {
        ok: checked.ok,
        diagnostics: checked.diagnostics(),
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::tools::test_support::{
        ed2_options, source, unnamed_source, SEMANTIC_ERROR_PROGRAM, SYNTAX_ERROR_PROGRAM,
        VALID_PROGRAM,
    };

    // Parse and analyze outcomes are owned by the parser and analyzer crates;
    // these tests only prove the tool wires the pipeline and validation in.

    #[test]
    fn build_response_when_valid_program_then_ok_true() {
        let resp = build_response(&source(VALID_PROGRAM), &ed2_options());
        assert!(resp.ok);
        assert_eq!(resp.diagnostics.len(), 1);
    }

    #[test]
    fn build_response_when_syntax_error_then_ok_false() {
        let resp = build_response(&source(SYNTAX_ERROR_PROGRAM), &ed2_options());
        assert!(!resp.ok);
        assert!(!resp.diagnostics.is_empty());
    }

    #[test]
    fn build_response_when_semantic_error_then_ok_false() {
        let resp = build_response(&source(SEMANTIC_ERROR_PROGRAM), &ed2_options());
        assert!(!resp.ok);
        assert!(resp.diagnostics.iter().any(|d| d["number"].is_u64()));
    }

    #[test]
    fn build_response_when_invalid_sources_then_error_diagnostic() {
        let resp = build_response(&unnamed_source(), &ed2_options());
        assert!(!resp.ok);
        assert!(!resp.diagnostics.is_empty());
    }

    #[test]
    fn build_response_when_invalid_options_then_error_diagnostic() {
        let resp = build_response(&source(VALID_PROGRAM), &serde_json::json!({}));
        assert!(!resp.ok);
        assert!(!resp.diagnostics.is_empty());
    }

    #[test]
    fn build_response_when_parse_error_in_one_source_then_still_reports() {
        let sources = vec![
            SourceInput {
                name: "good.st".into(),
                content: "PROGRAM good\nEND_PROGRAM".into(),
            },
            SourceInput {
                name: "bad.st".into(),
                content: "PROGRAM".into(),
            },
        ];
        let resp = build_response(&sources, &ed2_options());
        assert!(!resp.ok);
        assert!(!resp.diagnostics.is_empty());
    }

    #[test]
    fn build_response_when_error_then_diagnostics_have_byte_offsets() {
        let resp = build_response(&source(SEMANTIC_ERROR_PROGRAM), &ed2_options());
        assert!(!resp.ok);
        let d = &resp.diagnostics[0];
        // Verify byte offset fields exist
        assert!(d.get("position").is_some());
        assert!(d.get("length").is_some());
    }
}
