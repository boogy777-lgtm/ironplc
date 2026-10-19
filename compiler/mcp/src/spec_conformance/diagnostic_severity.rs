//! Conformance tests for the severity of a diagnostic in a tool response
//! (`specs/design/diagnostic-severity.md`, `REQ-DS-mcp-*`).

use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::{Area, Diagnostic, Severity};
use ironplc_problems::text;
use spec_test_macro::spec_test;

use crate::tools;
use crate::tools::common::{serialize_diagnostic, serialize_diagnostics, ParseCheckInput};
use crate::tools::message_areas;
use crate::tools::messages::{AreaName, MessageObjectsInput, MessagesInput};
use crate::tools::test_support::{ed2_options, source, SEMANTIC_ERROR_PROGRAM, VALID_PROGRAM};

/// A diagnostic in each of the seven categories, in the order of CODESYS, with
/// the scripting name and the flag value each has.
const CATEGORIES: [(Severity, &str, u64); 7] = [
    (Severity::FatalError, "FatalError", 1),
    (Severity::Error, "Error", 2),
    (Severity::Warning, "Warning", 4),
    (Severity::Information, "Information", 8),
    (Severity::Text, "Text", 16),
    (Severity::SuppressedWarning, "SuppressedWarning", 32),
    (Severity::SuppressedInformation, "SuppressedInformation", 64),
];

fn a_diagnostic() -> Diagnostic {
    Diagnostic::text(
        SourceSpan::default(),
        text::syntax_error_a_task_needs_a_priority(),
    )
}

#[spec_test(REQ_DS_mcp_001)]
fn mcp_spec_req_ds_001_severity_is_the_scripting_name_of_the_category_of_the_diagnostic() {
    let diagnostic = a_diagnostic();
    let json = serialize_diagnostic(&diagnostic, &[]);
    assert_eq!(json["severity"].as_str(), Some("Error"));
    assert_eq!(json["severity_value"].as_u64(), Some(2));

    for (severity, name, value) in CATEGORIES {
        let json = serialize_diagnostic(&diagnostic.clone().with_severity_for_test(severity), &[]);

        assert_eq!(json["severity"].as_str(), Some(name));
        assert_eq!(json["severity"].as_str(), Some(severity.script_name()));
        assert_eq!(json["severity_value"].as_u64(), Some(value));
        assert_eq!(
            json["severity_value"].as_u64(),
            Some(u64::from(severity.script_value()))
        );
    }
}

#[spec_test(REQ_DS_mcp_002)]
fn mcp_spec_req_ds_002_check_is_ok_only_when_no_diagnostic_is_an_error() {
    let clean = tools::check::build_response(&source(VALID_PROGRAM), &ed2_options());
    assert!(clean.ok, "diagnostics: {:?}", clean.diagnostics);
    assert!(clean.diagnostics.is_empty());

    let broken = tools::check::build_response(&source(SEMANTIC_ERROR_PROGRAM), &ed2_options());
    assert!(!broken.ok);
    assert!(broken
        .diagnostics
        .iter()
        .all(|d| d["severity"].as_str() == Some("Error")));
}

#[spec_test(REQ_DS_mcp_003)]
fn mcp_spec_req_ds_003_a_suppressed_diagnostic_is_serialized_with_its_category() {
    let warning = a_diagnostic().with_severity_for_test(Severity::Warning);

    let json = serialize_diagnostics(&[warning.clone(), warning.suppress()], &[]);

    assert_eq!(json.len(), 2);
    assert_eq!(json[0]["severity"].as_str(), Some("Warning"));
    assert_eq!(json[1]["severity"].as_str(), Some("SuppressedWarning"));
    assert_eq!(
        serialize_diagnostic(
            &a_diagnostic()
                .with_severity_for_test(Severity::Information)
                .suppress(),
            &[]
        )["severity"]
            .as_str(),
        Some("SuppressedInformation")
    );
}

#[spec_test(REQ_DS_mcp_004)]
fn mcp_spec_req_ds_004_a_diagnostic_has_the_fields_of_script_message_and_codesys() {
    let sources = source(SEMANTIC_ERROR_PROGRAM);
    let resp = tools::check::build_response(&sources, &ed2_options());
    let first = &resp.diagnostics[0];

    for field in [
        "severity",
        "severity_value",
        "prefix",
        "number",
        "text",
        "position",
        "length",
        "position_text",
        "object",
        "codesys",
    ] {
        assert!(first.get(field).is_some(), "no field {field}: {first}");
    }
    // The names the tools had before are gone.
    for field in ["code", "message", "file", "start", "end"] {
        assert!(first.get(field).is_none(), "old field {field}: {first}");
    }
    assert_eq!(first["prefix"], "P");
    assert!(first["number"].is_u64());
    assert_eq!(first["object"], sources[0].name.as_str());
    assert!(first["position_text"]
        .as_str()
        .unwrap()
        .starts_with(&format!("{}:", sources[0].name)));

    // `codesys` is the CODESYS code of the same condition, and null otherwise.
    let with = serialize_diagnostic(
        &Diagnostic::text(
            SourceSpan::default(),
            text::variable_undefined_undefined_variable(),
        ),
        &[],
    );
    assert_eq!(with["codesys"], "C0046");
    assert!(serialize_diagnostic(&a_diagnostic(), &[])["codesys"].is_null());
}

/// The objects of a program that produces an error, for a mask of categories.
fn objects_with(severities: Option<u32>) -> Vec<serde_json::Value> {
    let input = MessageObjectsInput {
        check: ParseCheckInput {
            sources: source(SEMANTIC_ERROR_PROGRAM),
            options: ed2_options(),
        },
        severities,
        area: None,
    };
    tools::messages::build_objects_response(&input).messages
}

fn texts_input(program: &str, area: Option<Area>) -> MessagesInput {
    MessagesInput {
        check: ParseCheckInput {
            sources: source(program),
            options: ed2_options(),
        },
        area: area.map(AreaName),
    }
}

/// A program of `count` statements, each of which reads a variable that is not
/// declared: `count` errors (P4007), each at its own place.
fn program_with_errors(count: usize) -> String {
    let mut program = String::from(
        "PROGRAM Main
VAR x : INT; END_VAR
",
    );
    for at in 0..count {
        program.push_str(&format!(
            "x := undeclared_{at};
"
        ));
    }
    program.push_str(
        "END_PROGRAM
",
    );
    program
}

#[spec_test(REQ_DS_mcp_005)]
fn mcp_spec_req_ds_005_get_message_objects_returns_the_objects_of_the_masked_categories() {
    let all = objects_with(None);
    let check = tools::check::build_response(&source(SEMANTIC_ERROR_PROGRAM), &ed2_options());

    // The default is every category: the objects of `check`, then the line that
    // closes the check.
    assert!(!check.diagnostics.is_empty());
    assert_eq!(all.len(), check.diagnostics.len() + 1);
    assert_eq!(all[..check.diagnostics.len()], check.diagnostics[..]);
    assert_eq!(all[check.diagnostics.len()]["severity"], "Text");
    assert_eq!(objects_with(Some(0xFFFF_FFFF)), all);
    // The mask selects by `severity_value`: errors are 2 and 1, warnings 4, a
    // text 16.
    assert_eq!(objects_with(Some(2)), check.diagnostics);
    assert_eq!(objects_with(Some(1 | 2)), check.diagnostics);
    assert_eq!(objects_with(Some(2 | 16)), all);
    assert_eq!(objects_with(Some(16)).len(), 1);
    assert!(objects_with(Some(4)).is_empty());
    assert!(objects_with(Some(0)).is_empty());
    assert!(objects_with(Some(64)).is_empty());
}

#[spec_test(REQ_DS_mcp_006)]
fn mcp_spec_req_ds_006_get_messages_returns_the_texts_of_the_objects() {
    let objects = objects_with(None);
    let texts = tools::messages::build_texts_response(&texts_input(SEMANTIC_ERROR_PROGRAM, None));

    let expected: Vec<&str> = objects
        .iter()
        .map(|object| object["text"].as_str().unwrap())
        .collect();
    assert!(expected.len() > 1);
    assert_eq!(texts.messages, expected);
    assert!(!texts.ok);

    let clean = tools::messages::build_texts_response(&texts_input(VALID_PROGRAM, None));
    assert!(clean.ok);
    assert_eq!(
        clean.messages,
        vec!["Compile complete -- 0 errors, 0 warnings"]
    );
}

#[spec_test(REQ_DS_mcp_007)]
fn mcp_spec_req_ds_007_a_message_without_a_code_has_null_prefix_number_and_codesys() {
    let closing = objects_with(Some(16));

    assert_eq!(closing.len(), 1);
    let line = &closing[0];
    assert_eq!(line["severity"], "Text");
    assert_eq!(line["severity_value"], 16);
    assert!(line["prefix"].is_null());
    assert!(line["number"].is_null());
    assert!(line["codesys"].is_null());
    assert!(line["text"]
        .as_str()
        .unwrap()
        .starts_with("Compile complete -- 1 errors, 0 warnings"));
    // The same for the line of a limit, which is an `Error` without a code.
    let limit = serialize_diagnostic(
        &Diagnostic::codeless(ironplc_problems::codeless::error_limit_exceeded(500)),
        &[],
    );
    assert_eq!(limit["severity"], "Error");
    assert!(limit["prefix"].is_null() && limit["number"].is_null());
}

#[spec_test(REQ_DS_mcp_008)]
fn mcp_spec_req_ds_008_the_area_selects_the_list_of_messages_and_the_default_is_build() {
    let program = program_with_errors(501);
    let objects = |area: Option<Area>| {
        tools::messages::build_objects_response(&MessageObjectsInput {
            check: ParseCheckInput {
                sources: source(&program),
                options: ed2_options(),
            },
            severities: None,
            area: area.map(AreaName),
        })
        .messages
    };

    let by_default = objects(None);
    let build = objects(Some(Area::Build));
    let precompile = objects(Some(Area::Precompile));

    // The default is the Build area: 500 errors, the line of the limit and the
    // line that closes the check.
    assert_eq!(by_default, build);
    assert_eq!(build.len(), 502);
    assert!(build[500]["prefix"].is_null());
    assert_eq!(build[501]["severity"], "Text");
    // The Precompile area holds the 501 errors that have a position, without the
    // line of the limit and without a closing line.
    assert_eq!(precompile.len(), 501);
    assert!(precompile.iter().all(|m| m["prefix"] == "P"));
    assert!(precompile.iter().all(|m| m["severity"] == "Error"));

    let texts = |area| tools::messages::build_texts_response(&texts_input(&program, area)).messages;
    assert_eq!(texts(None).len(), 502);
    assert_eq!(texts(Some(Area::Precompile)).len(), 501);
}

#[spec_test(REQ_DS_mcp_008)]
fn mcp_spec_req_ds_008_an_unknown_area_is_not_accepted_and_each_area_is_by_its_name() {
    let accepted = |name: &str| {
        serde_json::from_value::<MessagesInput>(serde_json::json!({
            "sources": [{"name": "main.st", "content": VALID_PROGRAM}],
            "options": ed2_options(),
            "area": name,
        }))
    };

    for area in Area::ALL {
        assert_eq!(accepted(area.name()).unwrap().area, Some(AreaName(area)));
    }
    let error = accepted("Static analysis").unwrap_err().to_string();
    assert!(error.contains("unknown area"), "{error}");
    assert!(error.contains("Build, Precompile"), "{error}");
}

#[spec_test(REQ_DS_mcp_009)]
fn mcp_spec_req_ds_009_get_message_areas_lists_each_area_with_its_description() {
    let response = message_areas::build_response();

    assert_eq!(response.areas.len(), Area::ALL.len());
    for (listed, area) in response.areas.iter().zip(Area::ALL) {
        assert_eq!(listed.name, area.name());
        assert_eq!(listed.source, area.source());
        assert_eq!(listed.description, area.description());
        assert!(!listed.description.is_empty());
    }
    let json = serde_json::to_value(&response).unwrap();
    assert_eq!(json["areas"][0]["name"], "Build");
    assert_eq!(json["areas"][1]["name"], "Precompile");
}
