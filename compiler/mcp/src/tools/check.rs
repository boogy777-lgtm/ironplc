//! The `check` MCP tool.
//!
//! Runs parse and full semantic analysis, returning structured diagnostics.

use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::{any_stops_build, ScriptMessage, CHECK_SUMMARY};
use ironplc_project::project::{MemoryBackedProject, Project};
use serde::Serialize;

use super::common::{
    parse_options, script_message, serialize_message, validate_sources, SourceInput,
};

/// Response returned by the `check` tool.
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
    pub messages: Vec<ScriptMessage>,
    /// The line that closes the check, "Compile complete", which CODESYS stores
    /// among the messages of a compile as a `Text`. The two queries of the
    /// messages return it after the others; the `diagnostics` of `check` are
    /// the problems alone, so that an empty list still means a clean program.
    /// It is `None` for an input that is not checked.
    pub summary: Option<ScriptMessage>,
}

/// Validates the inputs, then parses and analyzes the sources.
pub fn check(sources: &[SourceInput], options_value: &serde_json::Value) -> Checked {
    // Validate sources
    let source_errors = validate_sources(sources);
    if !source_errors.is_empty() {
        return Checked {
            ok: false,
            messages: messages_of(&source_errors, &[]),
            summary: None,
        };
    }

    // Parse options
    let options = match parse_options(options_value) {
        Ok(opts) => opts,
        Err(errs) => {
            return Checked {
                ok: false,
                messages: messages_of(&errs, &[]),
                summary: None,
            };
        }
    };

    // Construct a fresh in-memory project (REQ-ARC-mcp-010)
    let mut project = MemoryBackedProject::new(options);

    // Load sources (REQ-ARC-mcp-011)
    for src in sources {
        let file_id = FileId::from_string(&src.name);
        project.add_source(file_id, src.content.clone());
    }

    // Run parse + full semantic analysis
    let diagnostics = project.semantic();
    Checked {
        ok: !any_stops_build(&diagnostics),
        messages: messages_of(&diagnostics, sources),
        summary: Some(script_message(&CHECK_SUMMARY.closing(&diagnostics), &[])),
    }
}

fn messages_of(
    diagnostics: &[ironplc_dsl::diagnostic::Diagnostic],
    sources: &[SourceInput],
) -> Vec<ScriptMessage> {
    diagnostics
        .iter()
        .map(|diagnostic| script_message(diagnostic, sources))
        .collect()
}

/// Builds the check response from raw inputs.
pub fn build_response(sources: &[SourceInput], options_value: &serde_json::Value) -> CheckResponse {
    let checked = check(sources, options_value);
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
        assert!(resp.diagnostics.is_empty());
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
