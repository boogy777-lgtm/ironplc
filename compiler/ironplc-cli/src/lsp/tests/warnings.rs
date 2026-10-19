//! The lists of warnings in the `initializationOptions` of the language server
//! (`REQ-DS-cli-016`): the keys that set them, what a server started with them
//! publishes, and what it does with a code it does not accept.

use super::*;

/// A program with one warning, P4083, and no error.
const ONE_WARNING: &str = "TYPE E1 : (a1, a2); E2 : (b1, b2); END_TYPE
PROGRAM Main
VAR e1 : E1; e2 : E2; r : BOOL; END_VAR
  r := e1 = e2;
END_PROGRAM
";

/// What a server started with `initialization_options` publishes for
/// `ONE_WARNING`: the code and the severity of each diagnostic.
fn published_for_one_warning(
    initialization_options: serde_json::Value,
) -> Vec<(String, Option<lsp_types::DiagnosticSeverity>)> {
    let mut server = TestServer::started_with(initialization_options);
    server.send_notification::<notification::DidChangeTextDocument>(DidChangeTextDocumentParams {
        text_document: VersionedTextDocumentIdentifier {
            uri: Uri::from_str("file:///plant/main.st").unwrap(),
            version: 1,
        },
        content_changes: vec![TextDocumentContentChangeEvent {
            range: None,
            range_length: None,
            text: String::from(ONE_WARNING),
        }],
    });

    let published = server.receive_notification::<PublishDiagnosticsParams>();
    published
        .diagnostics
        .into_iter()
        .map(|diagnostic| {
            let code = match diagnostic.code {
                Some(lsp_types::NumberOrString::String(code)) => code,
                other => format!("{other:?}"),
            };
            (code, diagnostic.severity)
        })
        .collect()
}

/// The server started by the editor reads the lists from its
/// `initializationOptions`: a disabled warning is not published, and a warning
/// reported as an error stays a warning in the Precompile area, which is what
/// CODESYS shows while editing.
#[spec_test(REQ_DS_cli_016)]
fn initialize_when_the_warning_is_disabled_then_it_is_not_published() {
    let none = published_for_one_warning(serde_json::json!({}));
    let disabled = published_for_one_warning(serde_json::json!({ "disabledWarnings": ["P4083"] }));
    let disabled_by_codesys_code =
        published_for_one_warning(serde_json::json!({ "disabledWarnings": ["C0354"] }));

    assert_eq!(
        none,
        vec![(
            "P4083".to_string(),
            Some(lsp_types::DiagnosticSeverity::WARNING)
        )]
    );
    assert!(disabled.is_empty(), "{disabled:?}");
    assert!(disabled_by_codesys_code.is_empty());
}

#[spec_test(REQ_DS_cli_016)]
fn initialize_when_the_warning_is_raised_then_the_precompile_area_still_shows_a_warning() {
    let raised = published_for_one_warning(serde_json::json!({ "warningsAsErrors": ["P4083"] }));
    let both = published_for_one_warning(
        serde_json::json!({ "warningsAsErrors": ["P4083"], "disabledWarnings": ["P4083"] }),
    );

    let warning = vec![(
        "P4083".to_string(),
        Some(lsp_types::DiagnosticSeverity::WARNING),
    )];
    assert_eq!(raised, warning);
    assert_eq!(both, warning);
}

/// Every list of warnings is set by the lowerCamelCase form of the key it
/// declares, with the codes in either form, and only that list.
#[spec_test(REQ_DS_cli_016)]
fn extract_compiler_options_when_each_warning_list_key_set_then_the_codes_are_in_that_list_only() {
    for list in ironplc_dsl::diagnostic::WarningList::ALL {
        let key = super::super::to_lower_camel_case(list.option_key());
        for code in ["P4083", "C0354"] {
            let params = params_with_init_options(Some(serde_json::json!({ key.clone(): [code] })));

            let options = super::super::extract_compiler_options(&params);

            for other in ironplc_dsl::diagnostic::WarningList::ALL {
                assert_eq!(
                    options
                        .messages
                        .list(other)
                        .contains(ironplc_problems::WarningProblem::EnumerationTypesCompared),
                    other == list,
                    "LSP key `{key}` = [{code:?}] and list {other:?}"
                );
            }
        }
    }
}

#[spec_test(REQ_DS_cli_016)]
fn extract_compiler_options_when_the_key_is_the_camel_case_of_the_declared_key_then_the_names_of_the_design(
) {
    let names: Vec<String> = ironplc_dsl::diagnostic::WarningList::ALL
        .iter()
        .map(|list| super::super::to_lower_camel_case(list.option_key()))
        .collect();

    assert_eq!(names, ["disabledWarnings", "warningsAsErrors"]);
}

#[spec_test(REQ_DS_cli_016)]
fn extract_compiler_options_when_a_code_is_not_accepted_then_it_is_ignored_and_the_others_kept() {
    let params = params_with_init_options(Some(serde_json::json!({
        "disabledWarnings": ["P4007", "C0354", "nonsense", 7]
    })));

    let options = super::super::extract_compiler_options(&params);

    assert!(options
        .messages
        .list(ironplc_dsl::diagnostic::WarningList::Disabled)
        .contains(ironplc_problems::WarningProblem::EnumerationTypesCompared));
    assert!(options
        .messages
        .list(ironplc_dsl::diagnostic::WarningList::AsError)
        .is_empty());
}

#[spec_test(REQ_DS_cli_016)]
fn extract_compiler_options_when_a_list_is_not_an_array_then_it_is_ignored() {
    let params = params_with_init_options(Some(serde_json::json!({
        "warningsAsErrors": "P4083"
    })));

    let options = super::super::extract_compiler_options(&params);

    for list in ironplc_dsl::diagnostic::WarningList::ALL {
        assert!(options.messages.list(list).is_empty());
    }
}
