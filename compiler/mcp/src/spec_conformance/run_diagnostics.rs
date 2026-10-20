//! The diagnostics of a check run in every tool that returns them
//! (`specs/design/diagnostic-severity.md`, `REQ-DS-mcp-013`).
//!
//! The recurrence guard: the tools are taken from the registry of the server
//! (those whose input has `sources` and `options`), each must be classified
//! below, and each tool that returns the diagnostics of a check run is called
//! on a clean program, a program with an error, invalid sources and invalid
//! options, and must end with the closing line of the check.

use serde_json::Value;
use spec_test_macro::spec_test;

use crate::server::IronPlcMcp;
use crate::tools;
use crate::tools::common::SourceInput;
use crate::tools::test_support::{
    ed2_options, source, unnamed_source, SEMANTIC_ERROR_PROGRAM, VALID_PROGRAM,
};

/// What a tool that takes `sources` and `options` does with the diagnostics.
enum Framing {
    /// It returns the diagnostics of a check run, framed as `check` frames them;
    /// the function gives the `diagnostics` of its response.
    CheckRun(fn(&[SourceInput], &Value) -> Vec<Value>),
    /// It returns no diagnostics of a check run, and says why.
    Other(&'static str),
}

/// The POU the tools that name one are asked for, declared by `VALID_PROGRAM`.
const POU: &str = "p";

/// Every registered tool that takes `sources` and `options`, classified. A tool
/// that is registered and not listed fails `every_tool_is_classified`; a tool
/// listed here and not registered fails it too.
fn classified() -> Vec<(&'static str, Framing)> {
    vec![
        (
            "check",
            Framing::CheckRun(|s, o| tools::check::build_response(s, o).diagnostics),
        ),
        (
            "symbols",
            Framing::CheckRun(|s, o| tools::symbols::build_response(s, o, None).diagnostics),
        ),
        (
            "project_manifest",
            Framing::CheckRun(|s, o| tools::project_manifest::build_response(s, o).diagnostics),
        ),
        (
            "project_io",
            Framing::CheckRun(|s, o| tools::project_io::build_response(s, o).diagnostics),
        ),
        (
            "types_all",
            Framing::CheckRun(|s, o| tools::types_all::build_response(s, o).diagnostics),
        ),
        (
            "pou_scope",
            Framing::CheckRun(|s, o| tools::pou_scope::build_response(s, o, POU).diagnostics),
        ),
        (
            "pou_lineage",
            Framing::CheckRun(|s, o| tools::pou_lineage::build_response(s, o, POU).diagnostics),
        ),
        (
            "compile",
            Framing::Other("frames its diagnostics with the lines of a build (REQ-DS-mcp-011)"),
        ),
        (
            "parse",
            Framing::Other("the syntax stage alone: there is no run to close"),
        ),
        (
            "get_message_objects",
            Framing::Other("returns the messages of a check, framed by check (REQ-DS-mcp-005)"),
        ),
        (
            "get_messages",
            Framing::Other("returns the messages of a check, framed by check (REQ-DS-mcp-006)"),
        ),
        (
            "hot_edit_accept",
            Framing::Other("returns the diagnostics of compile when the compile fails"),
        ),
    ]
}

fn texts(diagnostics: &[Value]) -> Vec<&str> {
    diagnostics
        .iter()
        .map(|d| d["text"].as_str().unwrap_or_default())
        .collect()
}

/// The count of the messages that stop the build, which the closing line gives.
fn errors_in(diagnostics: &[Value]) -> usize {
    diagnostics
        .iter()
        .filter(|d| matches!(d["severity"].as_str(), Some("Error" | "FatalError")))
        .count()
}

#[spec_test(REQ_DS_mcp_013)]
fn every_tool_taking_sources_and_options_is_classified() {
    let registered = IronPlcMcp::tools_taking_sources_and_options();
    let mut classified: Vec<&str> = classified().iter().map(|(name, _)| *name).collect();
    classified.sort_unstable();

    // A tool that is not framed says why.
    for (name, framing) in self::classified() {
        if let Framing::Other(reason) = framing {
            assert!(!reason.is_empty(), "{name} gives no reason");
        }
    }
    assert_eq!(
        registered, classified,
        "a tool that takes sources and options must be classified in run_diagnostics.rs: \
         either it returns the diagnostics of a check run (CheckRun) and is framed, or it \
         says why it does not (Other)"
    );
}

#[spec_test(REQ_DS_mcp_013)]
fn every_tool_that_returns_the_diagnostics_of_a_run_ends_with_the_closing_line() {
    for (name, framing) in classified() {
        let Framing::CheckRun(call) = framing else {
            continue;
        };

        // A clean program has the closing line alone.
        let clean = call(&source(VALID_PROGRAM), &ed2_options());
        assert_eq!(
            texts(&clean),
            ["Compile complete -- 0 errors, 0 warnings"],
            "{name}: a clean program"
        );
        assert_eq!(clean[0]["severity"], "Text", "{name}");

        // Every other input ends with the line, whatever ended the run, and the
        // line counts the errors before it.
        let inputs: [(&str, Vec<SourceInput>, Value); 3] = [
            ("an error", source(SEMANTIC_ERROR_PROGRAM), ed2_options()),
            ("invalid sources", unnamed_source(), ed2_options()),
            (
                "invalid options",
                source(VALID_PROGRAM),
                serde_json::json!({}),
            ),
        ];
        for (what, sources, options) in inputs {
            let diagnostics = call(&sources, &options);
            let errors = errors_in(&diagnostics);

            assert!(errors > 0, "{name}: {what} has no error: {diagnostics:?}");
            assert_eq!(
                texts(&diagnostics).last().copied(),
                Some(format!("Compile complete -- {errors} errors, 0 warnings").as_str()),
                "{name}: {what}"
            );
            assert_eq!(
                texts(&diagnostics)
                    .iter()
                    .filter(|text| text.starts_with("Compile complete"))
                    .count(),
                1,
                "{name}: {what}: the line is there once"
            );
        }
    }
}

#[spec_test(REQ_DS_mcp_013)]
fn the_tools_that_name_a_pou_count_a_pou_that_is_not_declared_in_the_closing_line() {
    let missing = "no_such_pou";
    let scope = tools::pou_scope::build_response(&source(VALID_PROGRAM), &ed2_options(), missing);
    let lineage =
        tools::pou_lineage::build_response(&source(VALID_PROGRAM), &ed2_options(), missing);
    let symbols =
        tools::symbols::build_response(&source(VALID_PROGRAM), &ed2_options(), Some(missing));

    for (name, ok, found, diagnostics) in [
        ("pou_scope", scope.ok, Some(scope.found), scope.diagnostics),
        (
            "pou_lineage",
            lineage.ok,
            Some(lineage.found),
            lineage.diagnostics,
        ),
        ("symbols", symbols.ok, symbols.found, symbols.diagnostics),
    ] {
        assert!(!ok, "{name}");
        assert_eq!(found, Some(false), "{name}");
        assert_eq!(diagnostics.len(), 2, "{name}: {diagnostics:?}");
        assert_eq!(diagnostics[0]["number"], 8001, "{name}");
        assert_eq!(
            texts(&diagnostics)[1],
            "Compile complete -- 1 errors, 0 warnings",
            "{name}"
        );
    }
}

#[spec_test(REQ_DS_mcp_013)]
fn the_tools_that_return_the_diagnostics_of_a_run_apply_the_settings_of_the_run() {
    // A warning reported as an error is an error in the closing line, and a
    // disabled one is not counted: the settings reach the framing of every tool.
    let program = "TYPE E1 : (a1, a2); E2 : (b1, b2); END_TYPE
PROGRAM p
VAR e1 : E1; e2 : E2; r : BOOL; END_VAR
  r := e1 = e2;
END_PROGRAM
";
    let mut raised = ed2_options();
    raised["warnings_as_errors"] = serde_json::json!(["P4083"]);
    let mut disabled = ed2_options();
    disabled["disabled_warnings"] = serde_json::json!(["P4083"]);

    for (name, framing) in classified() {
        let Framing::CheckRun(call) = framing else {
            continue;
        };

        let plain = call(&source(program), &ed2_options());
        let with_error = call(&source(program), &raised);
        let without = call(&source(program), &disabled);

        assert_eq!(
            texts(&plain).last().copied(),
            Some("Compile complete -- 0 errors, 1 warnings"),
            "{name}"
        );
        assert_eq!(
            texts(&with_error).last().copied(),
            Some("Compile complete -- 1 errors, 0 warnings"),
            "{name}"
        );
        assert_eq!(
            texts(&without),
            ["Compile complete -- 0 errors, 0 warnings"],
            "{name}"
        );
    }
}
