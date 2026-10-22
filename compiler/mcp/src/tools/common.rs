//! Shared types and helpers for MCP tools that accept `sources` and `options`.
//!
//! Every source-accepting tool (parse, check, compile, run, …) reuses
//! the types and validation functions defined here.
//!
//! Validation errors produce standard `Diagnostic` values with problem code
//! `P8001` — the same diagnostic type the compiler uses everywhere.

use std::collections::HashSet;

use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::{Diagnostic, ScriptMessage, WarningList};
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_problems::{text, Text};
use schemars::JsonSchema;
use serde::{Deserialize, Serialize};

// ───────────────────────────────────────────────────────────────────
// Input types
// ───────────────────────────────────────────────────────────────────

/// A single source file supplied by the caller.
#[derive(Debug, Clone, Deserialize, JsonSchema)]
pub struct SourceInput {
    /// Logical file name (e.g. `"main.st"`). Must be non-empty, ≤ 256 bytes,
    /// contain only printable ASCII (0x20–0x7E), and be unique within the
    /// sources array.
    pub name: String,
    /// The full source text of the file.
    pub content: String,
}

/// Combined input accepted by `parse` and `check`.
#[derive(Debug, Deserialize, JsonSchema)]
pub struct ParseCheckInput {
    /// One or more source files.
    pub sources: Vec<SourceInput>,
    /// Compiler options (dialect + optional feature-flag overrides).
    #[schemars(schema_with = "options_schema")]
    pub options: serde_json::Value,
}

/// schemars schema function for the free-form `options` object.
///
/// The runtime type is `serde_json::Value`, which schemars would otherwise
/// render as the boolean schema `true`. Some MCP clients (e.g. OpenCode) reject
/// a boolean schema used as a `properties` value and fail to load the entire
/// tool list. Emit an explicit object schema so the field stays free-form but is
/// always a valid, non-boolean schema — independent of doc comments, which is
/// what previously (fragilely) kept the rendered schema from collapsing to
/// `true`. The `options` payload is validated at runtime by `parse_options`.
pub fn options_schema(_generator: &mut schemars::SchemaGenerator) -> schemars::Schema {
    schemars::json_schema!({
        "type": "object",
        "description": "Compiler options (dialect + optional feature-flag overrides)."
    })
}

// ───────────────────────────────────────────────────────────────────
// Output types (thin JSON serialisation layer)
// ───────────────────────────────────────────────────────────────────

/// A single entry in the `structure` array returned by the `parse` tool.
#[derive(Debug, Clone, Serialize)]
pub struct StructureEntry {
    pub kind: String,
    pub name: Option<String>,
    pub file: String,
    pub start: usize,
    pub end: usize,
}

// ───────────────────────────────────────────────────────────────────
// Source-name validation (REQ-STL-mcp-004)
// ───────────────────────────────────────────────────────────────────

/// Returns `true` when every byte in `name` is printable ASCII (0x20–0x7E).
fn is_printable_ascii(name: &str) -> bool {
    name.bytes().all(|b| (0x20..=0x7E).contains(&b))
}

/// Validates the `sources` array. Returns validation-error diagnostics
/// (empty on success).
pub fn validate_sources(sources: &[SourceInput]) -> Vec<Diagnostic> {
    let mut errors = Vec::new();
    let mut seen = HashSet::new();

    for src in sources {
        if src.name.is_empty() {
            errors.push(validation_diagnostic(
                text::mcp_input_validation_source_name_empty(),
            ));
            continue;
        }
        if src.name.len() > 256 {
            errors.push(validation_diagnostic(
                text::mcp_input_validation_source_name_too_long(truncate(&src.name, 40)),
            ));
        }
        if !is_printable_ascii(&src.name) {
            errors.push(validation_diagnostic(
                text::mcp_input_validation_source_name_not_printable(
                    src.name
                        .replace(|c: char| !c.is_ascii_graphic() && c != ' ', "?"),
                ),
            ));
        }
        if !seen.insert(&src.name) {
            errors.push(validation_diagnostic(
                text::mcp_input_validation_source_name_duplicate(&src.name),
            ));
        }
    }
    errors
}

// ───────────────────────────────────────────────────────────────────
// Options parsing (REQ-TOL-mcp-025, REQ-TOL-mcp-026)
// ───────────────────────────────────────────────────────────────────

/// Parses and validates the `options` JSON value into `CompilerOptions`.
pub fn parse_options(value: &serde_json::Value) -> Result<CompilerOptions, Vec<Diagnostic>> {
    let obj = value.as_object().ok_or_else(|| {
        vec![validation_diagnostic(
            text::mcp_input_validation_options_not_object(),
        )]
    })?;

    // `dialect` is required
    let dialect_str = obj.get("dialect").and_then(|v| v.as_str()).ok_or_else(|| {
        vec![validation_diagnostic(
            text::mcp_input_validation_options_dialect_missing(),
        )]
    })?;

    let dialect = resolve_dialect(dialect_str).ok_or_else(|| {
        let known: Vec<String> = Dialect::ALL.iter().map(|d| d.to_string()).collect();
        vec![validation_diagnostic(
            text::mcp_input_validation_options_dialect_unknown(dialect_str, known.join(", ")),
        )]
    })?;

    let mut options = CompilerOptions::from_dialect(dialect);
    let mut errors = Vec::new();

    for (key, val) in obj {
        if key == "dialect" {
            continue;
        }
        // A list of warnings (the settings that disable a warning or report it as
        // an error) is an array of codes. Its key is the one `WarningList`
        // declares, so a list added there is accepted here with no edit, and each
        // code is read by the one function every front end reads a code with.
        if let Some(list) = WarningList::ALL
            .into_iter()
            .find(|list| list.option_key() == key)
        {
            let codes: Option<Vec<&str>> = val
                .as_array()
                .and_then(|entries| entries.iter().map(|entry| entry.as_str()).collect());
            match codes {
                Some(codes) => {
                    for refused in options.messages.add_codes(list, codes) {
                        errors.push(validation_diagnostic(refused));
                    }
                }
                None => errors.push(validation_diagnostic(
                    text::mcp_input_validation_option_not_string_array(key),
                )),
            }
            continue;
        }
        let is_policy = CompilerOptions::POLICY_DESCRIPTORS
            .iter()
            .any(|pd| pd.option_key == key);
        if is_policy {
            // A behavior policy (ADR-0049) takes the CLI name of one of its
            // alternatives.
            match val.as_str() {
                Some(alt) => {
                    if options.set_policy_by_key(key, alt).is_err() {
                        let allowed = CompilerOptions::POLICY_DESCRIPTORS
                            .iter()
                            .find(|pd| pd.option_key == key)
                            .map(|pd| pd.alternatives.join(", "))
                            .unwrap_or_default();
                        errors.push(validation_diagnostic(
                            text::mcp_input_validation_option_policy_value_invalid(key, allowed),
                        ));
                    }
                }
                None => {
                    errors.push(validation_diagnostic(
                        text::mcp_input_validation_option_not_string(key),
                    ));
                }
            }
            continue;
        }
        match val.as_bool() {
            Some(b) => {
                if !options.set_flag_by_key(key, b) {
                    errors.push(validation_diagnostic(
                        text::mcp_input_validation_option_key_unknown(key),
                    ));
                }
            }
            None => {
                errors.push(validation_diagnostic(
                    text::mcp_input_validation_option_not_boolean(key),
                ));
            }
        }
    }

    if !errors.is_empty() {
        return Err(errors);
    }
    Ok(options)
}

/// Resolves a dialect string (e.g. `"iec61131-3-ed2"`) to a `Dialect`.
fn resolve_dialect(s: &str) -> Option<Dialect> {
    Dialect::ALL.iter().find(|d| d.to_string() == s).copied()
}

// ───────────────────────────────────────────────────────────────────
// Diagnostic serialisation (REQ-TOL-mcp-023, REQ-DS-mcp-001, REQ-DS-mcp-004)
// ───────────────────────────────────────────────────────────────────

/// The text of the source `name` among `sources`, for the line and column of a
/// message in that file.
fn source_text<'a>(sources: &'a [SourceInput], name: &str) -> Option<&'a str> {
    sources
        .iter()
        .find(|source| source.name == name)
        .map(|source| source.content.as_str())
}

/// Reads a compiler `Diagnostic` as a message for a program: the fields of the
/// CODESYS `ScriptMessage` and `codesys` (`specs/design/diagnostic-severity.md`).
///
/// `position` and `length` are byte offsets already stored in the diagnostic, so
/// no line/column conversion is applied to them. `sources` are the files the
/// caller supplied: the `position_text` of a message in one of them is
/// `file:line:column`, and it is `null` for a message in a file not among them.
pub fn script_message(diag: &Diagnostic, sources: &[SourceInput]) -> ScriptMessage {
    let source = source_text(sources, &diag.primary.file_id.to_string());
    diag.script_message(source)
}

/// The name of the project the sources are, which the line "Build started" gives
/// when the program has no `CONFIGURATION` to name it
/// (`Project::application_name`): the name of the first source without its
/// extension. The command line gives the same, the file name without its
/// extension. An input with no source has no name.
pub fn project_name(sources: &[SourceInput]) -> String {
    sources
        .first()
        .and_then(|source| std::path::Path::new(&source.name).file_stem())
        .map(|stem| stem.to_string_lossy().into_owned())
        .unwrap_or_default()
}

/// The JSON of a message: the one place a `ScriptMessage` becomes a tool result.
pub fn serialize_message(message: &ScriptMessage) -> serde_json::Value {
    serde_json::json!(message)
}

/// Serialises a compiler `Diagnostic` to the JSON of its `ScriptMessage`.
pub fn serialize_diagnostic(diag: &Diagnostic, sources: &[SourceInput]) -> serde_json::Value {
    serialize_message(&script_message(diag, sources))
}

/// Batch-serialises compiler diagnostics to a JSON array.
pub fn serialize_diagnostics(
    diags: &[Diagnostic],
    sources: &[SourceInput],
) -> Vec<serde_json::Value> {
    diags
        .iter()
        .map(|diag| serialize_diagnostic(diag, sources))
        .collect()
}

/// The diagnostic of the tools that name a POU, for a name no source declares.
/// A tool adds it to the messages of its run before they are framed
/// (`CheckRun::finish_with`).
pub fn pou_not_found_diagnostic(pou_name: &str) -> Diagnostic {
    validation_diagnostic(text::mcp_input_validation_pou_not_found(pou_name))
}

// ───────────────────────────────────────────────────────────────────
// Helpers
// ───────────────────────────────────────────────────────────────────

/// Creates a validation `Diagnostic` using the `P8001` problem code.
pub fn validation_diagnostic(text: Text) -> Diagnostic {
    Diagnostic::text(SourceSpan::default(), text)
}

/// Truncates a string to at most `max` characters, appending "…" if truncated.
fn truncate(s: &str, max: usize) -> String {
    if s.chars().count() <= max {
        s.to_string()
    } else {
        let truncated: String = s.chars().take(max).collect();
        format!("{truncated}…")
    }
}

// ───────────────────────────────────────────────────────────────────
// Tests
// ───────────────────────────────────────────────────────────────────

/// The P8002 diagnostics for why IronLink could not be used by a tool that needs
/// it (`browse_network`, and the `device_id` form of the `device` parameter).
pub fn ironlink_diagnostics(error: &crate::ironlink::IronLinkError) -> Vec<serde_json::Value> {
    use crate::ironlink::IronLinkError;
    let problem = match error {
        IronLinkError::NotFound { searched } => {
            text::mcp_iron_link_unavailable_not_found(searched.clone())
        }
        IronLinkError::Unavailable { message } => {
            text::mcp_iron_link_unavailable_failed(message.clone())
        }
        IronLinkError::Refused { l_code, message } => {
            text::mcp_iron_link_unavailable_failed(format!(
                "it refused the request: {} - {message}",
                l_code.as_deref().unwrap_or("no code")
            ))
        }
    };
    serialize_diagnostics(&[Diagnostic::text(SourceSpan::default(), problem)], &[])
}

#[cfg(test)]
mod tests {
    use super::*;

    // -- validate_sources tests --

    #[test]
    fn validate_sources_when_empty_name_then_error() {
        let sources = vec![SourceInput {
            name: String::new(),
            content: "PROGRAM p END_PROGRAM".into(),
        }];
        let errs = validate_sources(&sources);
        assert_eq!(errs.len(), 1);
    }

    #[test]
    fn validate_sources_when_name_too_long_then_error() {
        let sources = vec![SourceInput {
            name: "a".repeat(257),
            content: String::new(),
        }];
        let errs = validate_sources(&sources);
        assert_eq!(errs.len(), 1);
    }

    #[test]
    fn validate_sources_when_name_contains_non_printable_then_error() {
        // NUL character
        let sources = vec![SourceInput {
            name: "file\0.st".into(),
            content: String::new(),
        }];
        let errs = validate_sources(&sources);
        assert_eq!(errs.len(), 1);
    }

    #[test]
    fn validate_sources_when_name_contains_slash_then_accepted() {
        let sources = vec![SourceInput {
            name: "path/file.st".into(),
            content: String::new(),
        }];
        // A name is an opaque key, not a filesystem path, so a separator in
        // it means nothing to the server. '/' is 0x2F, inside the printable
        // ASCII range REQ-STL-mcp-004 allows, so it is accepted.
        let errs = validate_sources(&sources);
        assert!(errs.is_empty());
    }

    #[test]
    fn validate_sources_when_name_contains_tab_then_error() {
        let sources = vec![SourceInput {
            name: "file\t.st".into(),
            content: String::new(),
        }];
        let errs = validate_sources(&sources);
        assert_eq!(errs.len(), 1);
    }

    #[test]
    fn validate_sources_when_name_contains_bell_then_error() {
        let sources = vec![SourceInput {
            name: "file\x07.st".into(),
            content: String::new(),
        }];
        let errs = validate_sources(&sources);
        assert_eq!(errs.len(), 1);
    }

    #[test]
    fn validate_sources_when_name_contains_del_then_error() {
        // DEL is 0x7F, outside printable ASCII range
        let sources = vec![SourceInput {
            name: "file\x7F.st".into(),
            content: String::new(),
        }];
        let errs = validate_sources(&sources);
        assert_eq!(errs.len(), 1);
    }

    #[test]
    fn validate_sources_when_duplicate_names_then_error() {
        let sources = vec![
            SourceInput {
                name: "main.st".into(),
                content: String::new(),
            },
            SourceInput {
                name: "main.st".into(),
                content: String::new(),
            },
        ];
        let errs = validate_sources(&sources);
        assert_eq!(errs.len(), 1);
    }

    #[test]
    fn validate_sources_when_valid_then_ok() {
        let sources = vec![
            SourceInput {
                name: "main.st".into(),
                content: "PROGRAM p END_PROGRAM".into(),
            },
            SourceInput {
                name: "lib.st".into(),
                content: "FUNCTION f : INT END_FUNCTION".into(),
            },
        ];
        let errs = validate_sources(&sources);
        assert!(errs.is_empty());
    }

    #[test]
    fn validate_sources_when_p8001_code_then_correct() {
        let sources = vec![SourceInput {
            name: String::new(),
            content: String::new(),
        }];
        let errs = validate_sources(&sources);
        assert_eq!(errs[0].code, "P8001");
    }

    // -- parse_options tests --

    #[test]
    fn parse_options_when_missing_dialect_then_error() {
        let val = serde_json::json!({});
        let result = parse_options(&val);
        assert!(result.is_err());
    }

    #[test]
    fn parse_options_when_unknown_dialect_then_error() {
        let val = serde_json::json!({"dialect": "cobol"});
        let result = parse_options(&val);
        assert!(result.is_err());
    }

    #[test]
    fn parse_options_when_unknown_key_then_error() {
        let val = serde_json::json!({"dialect": "iec61131-3-ed2", "bogus": true});
        let result = parse_options(&val);
        assert!(result.is_err());
    }

    #[test]
    fn parse_options_when_ed2_dialect_then_default_options() {
        let val = serde_json::json!({"dialect": "iec61131-3-ed2"});
        let opts = parse_options(&val).unwrap();
        assert!(!opts.allow_long_time_types);
        assert!(!opts.allow_c_style_comments);
    }

    #[test]
    fn parse_options_when_ed3_dialect_then_edition3_enabled() {
        let val = serde_json::json!({"dialect": "iec61131-3-ed3"});
        let opts = parse_options(&val).unwrap();
        assert!(opts.allow_long_time_types);
        assert!(opts.allow_ref_to);
        assert!(!opts.allow_c_style_comments);
    }

    #[test]
    fn parse_options_when_rusty_dialect_then_dialect_flags_enabled() {
        let val = serde_json::json!({"dialect": "rusty"});
        let opts = parse_options(&val).unwrap();
        assert!(opts.allow_c_style_comments);
        assert!(opts.allow_missing_semicolon);
        assert!(opts.allow_ref_to);
    }

    #[test]
    fn parse_options_when_flag_override_then_applied() {
        let val = serde_json::json!({"dialect": "iec61131-3-ed2", "allow_c_style_comments": true});
        let opts = parse_options(&val).unwrap();
        assert!(opts.allow_c_style_comments);
    }

    #[test]
    fn parse_options_when_policy_override_then_replaces_dialect_selection() {
        let val = serde_json::json!({
            "dialect": "codesys",
            "policy_string_to_num_failure": "trap",
        });
        let opts = parse_options(&val).unwrap();
        assert_eq!(
            opts.get_policy_by_key("policy_string_to_num_failure"),
            Some("trap")
        );
        // The other policy keeps the dialect's selection.
        assert_eq!(
            opts.get_policy_by_key("policy_string_to_num_non_numeric"),
            Some("ignore-trailing")
        );
    }

    #[test]
    fn parse_options_when_strictness_policy_set_then_replaces_dialect_selection() {
        let val = serde_json::json!({
            "dialect": "codesys",
            "policy_enumeration_strictness": "every-enumeration",
        });
        let opts = parse_options(&val).unwrap();
        assert_eq!(
            opts.get_policy_by_key("policy_enumeration_strictness"),
            Some("every-enumeration")
        );
        let dialect_only = parse_options(&serde_json::json!({ "dialect": "codesys" })).unwrap();
        assert_eq!(
            dialect_only.get_policy_by_key("policy_enumeration_strictness"),
            Some("with-attribute")
        );
    }

    #[test]
    fn parse_options_when_policy_value_unknown_then_error_names_alternatives() {
        let val = serde_json::json!({
            "dialect": "iec61131-3-ed2",
            "policy_string_to_num_failure": "wrap",
        });
        let errors = parse_options(&val).unwrap_err();
        assert_eq!(errors.len(), 1);
        assert!(errors[0].primary.message.contains("trap, zero"));
    }

    #[test]
    fn parse_options_when_policy_value_not_a_string_then_error() {
        let val = serde_json::json!({
            "dialect": "iec61131-3-ed2",
            "policy_string_to_num_failure": true,
        });
        assert!(parse_options(&val).is_err());
    }

    // -- serialize_diagnostic tests --

    #[test]
    fn serialize_diagnostic_when_called_then_has_byte_offsets() {
        let diag = Diagnostic::text(
            SourceSpan::range(10, 15),
            text::syntax_error_a_task_needs_a_priority(),
        );
        let json = serialize_diagnostic(&diag, &[]);
        assert_eq!(json["position"], 10);
        assert_eq!(json["length"], 5);
    }

    #[test]
    fn serialize_diagnostic_when_called_then_has_code_and_text() {
        let diag = Diagnostic::text(
            SourceSpan::range(0, 1),
            text::syntax_error_a_task_needs_a_priority(),
        );
        let json = serialize_diagnostic(&diag, &[]);
        assert_eq!(json["prefix"], "P");
        assert_eq!(json["number"], 2);
        assert!(json["text"].as_str().unwrap().contains("Syntax error"));
    }

    #[test]
    fn serialize_diagnostic_when_source_supplied_then_position_text_has_line_and_column() {
        let mut span = SourceSpan::range(7, 8);
        span.file_id = ironplc_dsl::core::FileId::from_string("main.st");
        let diag = Diagnostic::text(span, text::syntax_error_a_task_needs_a_priority());
        let sources = vec![SourceInput {
            name: "main.st".into(),
            content: "a\nbb\nccc".into(),
        }];

        let json = serialize_diagnostic(&diag, &sources);

        assert_eq!(json["position_text"], "main.st:3:3");
        assert_eq!(json["object"], "main.st");
        assert!(serialize_diagnostic(&diag, &[])["position_text"].is_null());
    }

    #[test]
    fn pou_not_found_diagnostic_when_called_then_a_validation_message_naming_the_pou() {
        let json = serialize_diagnostic(&pou_not_found_diagnostic("Main"), &[]);

        assert_eq!(json["number"], 8001);
        assert!(json["text"]
            .as_str()
            .unwrap()
            .ends_with("No POU named 'Main' found."));
    }
}
