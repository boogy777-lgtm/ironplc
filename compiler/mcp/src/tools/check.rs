//! The `check` MCP tool.
//!
//! Runs parse and full semantic analysis, returning structured diagnostics.

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

/// The messages of a run written to `area`, framed as the area frames them, and
/// whether none of them stops the build. This is the one place that closes a
/// check, so every path out of `check` (invalid input too) ends the same way.
fn framed(
    area: Area,
    application: &str,
    settings: &MessageSettings,
    diagnostics: Vec<Diagnostic>,
    sources: &[SourceInput],
) -> Checked {
    let framed = match area.check_summary() {
        Some(summary) => summary.close(application, settings, diagnostics),
        None => diagnostics,
    };
    Checked {
        ok: !any_stops_build(&framed),
        messages: framed
            .iter()
            .map(|diagnostic| script_message(diagnostic, sources))
            .collect(),
    }
}

/// Validates the inputs, then parses and analyzes the sources. The check is a
/// run that writes `area`: the area keeps its messages and decides the line that
/// closes it.
pub fn check(sources: &[SourceInput], options_value: &serde_json::Value, area: Area) -> Checked {
    let fallback = project_name(sources);

    // Validate sources
    let source_errors = validate_sources(sources);
    if !source_errors.is_empty() {
        return framed(
            area,
            &fallback,
            &MessageSettings::default(),
            source_errors,
            &[],
        );
    }

    // Parse options
    let options = match parse_options(options_value) {
        Ok(opts) => opts,
        Err(errs) => return framed(area, &fallback, &MessageSettings::default(), errs, &[]),
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
    framed(area, &application, &options.messages, diagnostics, sources)
}

/// Builds the check response from raw inputs.
pub fn build_response(sources: &[SourceInput], options_value: &serde_json::Value) -> CheckResponse {
    let checked = check(sources, options_value, Area::Build);
    CheckResponse {
        ok: checked.ok,
        diagnostics: checked.messages.iter().map(serialize_message).collect(),
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
