//! Tests for the language server: initialization, diagnostics publishing, and document change handling.

use core::time::Duration;
use lsp_server::{Connection, Message, RequestId};
use lsp_server::{Notification, Response};
use lsp_types::DidChangeTextDocumentParams;
use lsp_types::Uri;
use lsp_types::VersionedTextDocumentIdentifier;
use lsp_types::{notification, WorkDoneProgressParams};
use lsp_types::{
    request, ClientCapabilities, InitializeParams, InitializeResult, InitializedParams,
    PublishDiagnosticsParams, TextDocumentContentChangeEvent,
};
use serde::de::DeserializeOwned;
use serde::Serialize;
use spec_test_macro::spec_test;
use std::collections::{BTreeMap, HashMap};
use std::str::FromStr;

use crate::lsp_project::{LspProject, UriKey};
use ironplc_project::{FileBackedProject, Project};

use super::start_with_connection;

struct TestServer {
    server_thread: Option<std::thread::JoinHandle<()>>,
    client_connection: Connection,
    request_id_counter: i32,

    responses: HashMap<RequestId, Response>,
    notifications: Vec<Notification>,
}

impl Drop for TestServer {
    fn drop(&mut self) {
        self.send_request::<request::Shutdown>(());
        self.send_notification::<notification::Exit>(());

        if let Some(server_thread) = self.server_thread.take() {
            server_thread.join().unwrap();
        }
    }
}

impl TestServer {
    fn new(project: Box<dyn Project + Send>) -> Self {
        let project = LspProject::new(project);
        let (server_connection, client_connection) = Connection::memory();

        let server_thread = std::thread::spawn(|| {
            start_with_connection(server_connection, Some(project)).unwrap();
        });

        let mut server = Self {
            server_thread: Some(server_thread),
            client_connection,
            request_id_counter: 0,
            responses: HashMap::new(),
            notifications: Vec::new(),
        };

        let init = params_with_init_options(None);

        let initialize_id = server.send_request::<request::Initialize>(init);
        server.receive_response::<InitializeResult>(initialize_id);

        server.send_notification::<notification::Initialized>(InitializedParams {});

        server
    }

    fn send_request<N>(&mut self, params: N::Params) -> RequestId
    where
        N: lsp_types::request::Request,
        N::Params: Serialize,
    {
        self.request_id_counter += 1;
        let message = lsp_server::Request::new(
            RequestId::from(self.request_id_counter),
            N::METHOD.to_string(),
            params,
        );
        self.client_connection
            .sender
            .send(Message::Request(message))
            .unwrap();
        RequestId::from(self.request_id_counter)
    }

    /// Sends a message from the client, such as VSCode, to the server.
    fn send_notification<N>(&mut self, params: N::Params)
    where
        N: lsp_types::notification::Notification,
        N::Params: Serialize,
    {
        let message = lsp_server::Notification::new(N::METHOD.to_string(), params);
        self.client_connection
            .sender
            .send(Message::Notification(message))
            .unwrap();
    }

    fn send_raw_request(&mut self, method: &str, params: serde_json::Value) -> RequestId {
        self.request_id_counter += 1;
        let message = lsp_server::Request {
            id: RequestId::from(self.request_id_counter),
            method: method.to_string(),
            params: serde_json::to_value(params).unwrap(),
        };
        self.client_connection
            .sender
            .send(Message::Request(message))
            .unwrap();
        RequestId::from(self.request_id_counter)
    }

    fn receive(&mut self) {
        let timeout = Duration::from_secs(60);
        let message = self
            .client_connection
            .receiver
            .recv_timeout(timeout)
            .unwrap();

        match message {
            Message::Request(request) => {
                // The test client never expects requests from the server.
                assert!(
                    request.method.is_empty(),
                    "unexpected request in test client: {}",
                    request.method
                );
            }
            Message::Response(response) => {
                let id = response.id.clone();
                self.responses.insert(id, response);
            }
            Message::Notification(notification) => {
                self.notifications.push(notification);
            }
        }
    }

    fn receive_response<T: DeserializeOwned>(&mut self, request_id: RequestId) -> T {
        self.receive();
        let response = self.responses.get(&request_id).unwrap();
        // `response_result` is `Ok(value)` for a successful response and
        // `Err(..)` for a failure, so `expect` asserts success here.
        let result = response.response_result.as_ref().unwrap().clone();
        serde_json::from_value::<T>(result).unwrap()
    }

    fn receive_notification<T: DeserializeOwned>(&mut self) -> T {
        self.receive();
        let notification = self.notifications.pop().unwrap();
        serde_json::from_value::<T>(notification.params).unwrap()
    }

    /// Receive `n` `publishDiagnostics` notifications from the
    /// server and return them keyed by URI. The server emits one
    /// notification per affected file plus one per stale URI it
    /// is clearing, so callers know exactly how many to expect.
    fn receive_publishes(&mut self, n: usize) -> BTreeMap<UriKey, PublishDiagnosticsParams> {
        let mut out = BTreeMap::new();
        for _ in 0..n {
            let p = self.receive_notification::<PublishDiagnosticsParams>();
            out.insert(UriKey::from_uri(&p.uri), p);
        }
        out
    }
}

#[test]
fn text_document_changed_then_returns_diagnostics() {
    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);
    server.send_notification::<notification::DidChangeTextDocument>(DidChangeTextDocumentParams {
        text_document: VersionedTextDocumentIdentifier {
            uri: Uri::from_str("file://example.net/a/b.html").unwrap(),
            version: 1,
        },
        content_changes: vec![TextDocumentContentChangeEvent {
            range: None,
            range_length: None,
            text: String::from("this is some text"),
        }],
    });

    server.receive_notification::<PublishDiagnosticsParams>();
}

#[test]
fn disassemble_request_when_valid_iplc_file_then_returns_json() {
    use ironplc_container::ContainerBuilder;
    use std::io::Write;

    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);

    // Build a small .iplc file
    #[rustfmt::skip]
        let bytecode: Vec<u8> = vec![
            0x00, 0x00, 0x00,  // LOAD_CONST_I32 pool[0]
            0x10, 0x00, 0x00,  // STORE_VAR_I32  var[0]
            0x8C,              // RET_VOID
        ];
    let container = ContainerBuilder::new()
        .num_variables(1)
        .add_i32_constant(42)
        .add_function(ironplc_container::FunctionId::new(0), &bytecode, 1, 1, 0)
        .build();
    let mut buf = Vec::new();
    container.write_to(&mut buf).unwrap();

    let mut tmp = tempfile::NamedTempFile::new().unwrap();
    tmp.write_all(&buf).unwrap();
    tmp.flush().unwrap();

    let path_str = tmp.path().display().to_string().replace('\\', "/");
    let uri = if path_str.starts_with('/') {
        format!("file://{path_str}")
    } else {
        format!("file:///{path_str}")
    };
    let params = serde_json::json!({"uri": uri});
    let req_id = server.send_raw_request("ironplc/disassemble", params);
    let result: serde_json::Value = server.receive_response(req_id);

    assert_eq!(result["header"]["numFunctions"], 1);
    assert_eq!(result["constants"][0]["value"], "42");
}

/// Builds `InitializeParams` carrying the given `initializationOptions`.
#[expect(
    deprecated,
    reason = "lsp_types requires the deprecated `root_path` field to be set"
)]
fn params_with_init_options(init: Option<serde_json::Value>) -> InitializeParams {
    InitializeParams {
        process_id: None,
        root_path: None,
        root_uri: None,
        initialization_options: init,
        capabilities: ClientCapabilities::default(),
        trace: None,
        workspace_folders: None,
        client_info: None,
        locale: None,
        work_done_progress_params: WorkDoneProgressParams {
            work_done_token: None,
        },
    }
}

/// The policy counterpart: every behavior policy must be settable to each
/// of its alternatives via its lowerCamelCase key with a string value.
#[test]
fn extract_compiler_options_when_each_policy_key_set_then_alternative_selected() {
    for pd in ironplc_parser::options::CompilerOptions::POLICY_DESCRIPTORS {
        for alt in pd.alternatives {
            let key = super::to_lower_camel_case(pd.option_key);
            let params = params_with_init_options(Some(
                serde_json::json!({ "dialect": "codesys", key.clone(): alt }),
            ));
            let options = super::extract_compiler_options(&params);
            assert_eq!(
                options.get_policy_by_key(pd.option_key),
                Some(*alt),
                "LSP key `{key}` = {alt:?} did not select it for CompilerOptions.{}",
                pd.option_key
            );
        }
    }
}

#[test]
fn extract_compiler_options_when_policy_value_unknown_then_dialect_selection_kept() {
    let params = params_with_init_options(Some(
        serde_json::json!({ "dialect": "codesys", "policyStringToNumFailure": "wrap" }),
    ));
    let options = super::extract_compiler_options(&params);
    assert_eq!(
        options.get_policy_by_key("policy_string_to_num_failure"),
        Some("zero")
    );
}

/// Guards the LSP option surface against drifting out of sync with the
/// compiler: every dialect flag must be enablable via its lowerCamelCase
/// `initializationOptions` key. `extract_compiler_options` derives these
/// from `FEATURE_DESCRIPTORS`, so this also pins the snake -> camelCase
/// contract the VS Code extension relies on.
#[test]
fn extract_compiler_options_when_each_dialect_flag_key_set_then_flag_enabled() {
    for fd in ironplc_parser::options::CompilerOptions::FEATURE_DESCRIPTORS {
        let key = super::to_lower_camel_case(fd.option_key);
        let params = params_with_init_options(Some(serde_json::json!({ key.clone(): true })));
        let options = super::extract_compiler_options(&params);
        assert_eq!(
            options.get_flag_by_key(fd.option_key),
            Some(true),
            "LSP key `{key}` did not enable CompilerOptions.{}",
            fd.option_key
        );
    }
}

#[test]
fn to_lower_camel_case_when_multi_segment_then_camel_cased() {
    assert_eq!(super::to_lower_camel_case("allow_ref_to"), "allowRefTo");
    assert_eq!(
        super::to_lower_camel_case("allow_c_style_comments"),
        "allowCStyleComments"
    );
    assert_eq!(super::to_lower_camel_case("allow_sizeof"), "allowSizeof");
}

#[test]
fn extract_compiler_options_when_ed3_dialect_then_enables_edition_3() {
    let params = params_with_init_options(Some(serde_json::json!({"dialect": "iec61131-3-ed3"})));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_long_time_types);
    assert!(options.allow_ref_to);
}

#[test]
fn extract_compiler_options_when_ed2_dialect_then_uses_default() {
    let params = params_with_init_options(Some(serde_json::json!({"dialect": "iec61131-3-ed2"})));

    let options = super::extract_compiler_options(&params);
    assert!(!options.allow_long_time_types);
}

#[test]
fn extract_compiler_options_when_rusty_dialect_then_enables_ref_to_and_dialect_flags() {
    let params = params_with_init_options(Some(serde_json::json!({"dialect": "rusty"})));

    let options = super::extract_compiler_options(&params);
    assert!(!options.allow_long_time_types);
    assert!(options.allow_ref_to);
    assert!(options.allow_c_style_comments);
    assert!(options.allow_missing_semicolon);
}

#[test]
fn extract_compiler_options_when_codesys_dialect_then_enables_ref_to_without_uptime_global() {
    let params = params_with_init_options(Some(serde_json::json!({"dialect": "codesys"})));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_long_time_types);
    assert!(options.allow_ref_to);
    assert!(options.allow_c_style_comments);
    assert!(options.allow_sizeof);
    // CODESYS dialect does not pre-bind the IronPLC uptime globals.
    assert!(!options.allow_system_uptime_global);
}

#[test]
fn extract_compiler_options_when_twincat_dialect() {
    let params = params_with_init_options(Some(serde_json::json!({"dialect": "twincat"})));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_long_time_types);
    assert!(options.allow_c_style_comments);
    assert!(options.allow_pragmas);
    assert!(options.allow_short_circuit_operators);
    // TwinCAT spells references/pointers REFERENCE TO / POINTER TO, not the
    // CODESYS REF_TO / REF() / NULL, so none of the REF_TO-family flags are
    // enabled.
    assert!(options.allow_reference_to);
    assert!(options.allow_pointer_to);
    assert!(!options.allow_ref_to);
    assert!(!options.allow_ref_arithmetic);
    assert!(!options.allow_ref_stack_variables);
    assert!(!options.allow_ref_type_punning);
    // TwinCAT, like CODESYS, does not pre-bind the IronPLC uptime globals.
    assert!(!options.allow_system_uptime_global);
}

#[test]
fn extract_compiler_options_when_no_options_then_uses_default() {
    let params = params_with_init_options(None);

    let options = super::extract_compiler_options(&params);
    assert!(!options.allow_long_time_types);
}

#[test]
fn extract_compiler_options_when_allow_missing_semicolon_then_enables_flag() {
    let params = params_with_init_options(Some(serde_json::json!({"allowMissingSemicolon": true})));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_missing_semicolon);
}

#[test]
fn extract_compiler_options_when_allow_empty_var_blocks_then_enables_flag() {
    let params = params_with_init_options(Some(serde_json::json!({"allowEmptyVarBlocks": true})));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_empty_var_blocks);
}

#[test]
fn extract_compiler_options_when_allow_sizeof_then_enables_flag() {
    let params = params_with_init_options(Some(serde_json::json!({"allowSizeof": true})));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_sizeof);
}

#[test]
fn extract_compiler_options_when_allow_pragmas_then_enables_flag() {
    let params = params_with_init_options(Some(serde_json::json!({"allowPragmas": true})));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_pragmas);
}

#[test]
fn extract_compiler_options_when_allow_mixed_located_var_declarations_then_enables_flag() {
    let params = params_with_init_options(Some(
        serde_json::json!({"allowMixedLocatedVarDeclarations": true}),
    ));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_mixed_located_var_declarations);
}

#[test]
fn extract_compiler_options_when_allow_constant_initializer_expressions_then_enables_flag() {
    let params = params_with_init_options(Some(
        serde_json::json!({"allowConstantInitializerExpressions": true}),
    ));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_constant_initializer_expressions);
}

#[test]
fn extract_compiler_options_when_allow_reference_to_then_enables_flag() {
    let params = params_with_init_options(Some(serde_json::json!({"allowReferenceTo": true})));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_reference_to);
}

#[test]
fn extract_compiler_options_when_allow_pointer_to_then_enables_flag() {
    let params = params_with_init_options(Some(serde_json::json!({"allowPointerTo": true})));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_pointer_to);
}

#[test]
fn extract_compiler_options_when_allow_adr_then_enables_flag() {
    let params = params_with_init_options(Some(serde_json::json!({"allowAdr": true})));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_adr);
}

#[test]
fn extract_compiler_options_when_allow_fb_inheritance_then_enables_flag() {
    let params = params_with_init_options(Some(serde_json::json!({"allowFbInheritance": true})));

    let options = super::extract_compiler_options(&params);
    assert!(options.allow_fb_inheritance);
}

#[test]
fn lsp_when_ed3_dialect_then_accepts_ltime() {
    use ironplc_parser::options::{CompilerOptions, Dialect};

    let compiler_options = CompilerOptions::from_dialect(Dialect::Iec61131_3Ed3);
    let proj = Box::new(FileBackedProject::with_options(compiler_options));
    let mut server = TestServer::new(proj);

    // Send a document with LTIME literal (Edition 3 feature)
    server.send_notification::<notification::DidChangeTextDocument>(DidChangeTextDocumentParams {
        text_document: VersionedTextDocumentIdentifier {
            uri: Uri::from_str("file://example.net/test.st").unwrap(),
            version: 1,
        },
        content_changes: vec![TextDocumentContentChangeEvent {
            range: None,
            range_length: None,
            text: String::from("PROGRAM Main\nVAR\nx : LTIME;\nEND_VAR\nEND_PROGRAM"),
        }],
    });

    let diagnostics = server.receive_notification::<PublishDiagnosticsParams>();
    // LTIME should be accepted with 2013 options — no parse errors
    assert!(
        diagnostics.diagnostics.is_empty(),
        "Expected no diagnostics for LTIME with 2013 options, got: {:?}",
        diagnostics.diagnostics
    );
}

#[test]
fn run_request_when_valid_source_then_returns_ok() {
    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);

    let params = serde_json::json!({
        "source": "PROGRAM main\nVAR\nx : DINT;\nEND_VAR\nx := 42;\nEND_PROGRAM",
        "cycleTimeUs": 100000
    });
    let req_id = server.send_raw_request("ironplc/run", params);
    let result: serde_json::Value = server.receive_response(req_id);

    assert_eq!(result["ok"], true);
    assert_eq!(result["total_scans"], 0);
}

#[test]
fn run_request_when_invalid_source_then_returns_error() {
    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);

    let params = serde_json::json!({
        "source": "INVALID CODE",
        "cycleTimeUs": 100000
    });
    let req_id = server.send_raw_request("ironplc/run", params);
    let result: serde_json::Value = server.receive_response(req_id);

    assert_eq!(result["ok"], false);
    assert!(result["error"].as_str().is_some());
}

#[test]
fn step_request_when_program_loaded_then_returns_variables() {
    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);

    // Load program first
    let params = serde_json::json!({
        "source": "PROGRAM main\nVAR\nx : DINT;\nEND_VAR\nx := 42;\nEND_PROGRAM",
        "cycleTimeUs": 100000
    });
    let req_id = server.send_raw_request("ironplc/run", params);
    let _: serde_json::Value = server.receive_response(req_id);

    // Step one scan
    let step_params = serde_json::json!({"scans": 1});
    let step_id = server.send_raw_request("ironplc/step", step_params);
    let result: serde_json::Value = server.receive_response(step_id);

    assert_eq!(result["ok"], true);
    assert_eq!(result["total_scans"], 1);
    let vars = result["variables"].as_array().unwrap();
    assert!(!vars.is_empty());
    assert_eq!(vars[0]["name"], "x");
    assert_eq!(vars[0]["value"], "42");
}

#[test]
fn step_request_when_no_program_then_returns_error() {
    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);

    let params = serde_json::json!({"scans": 1});
    let req_id = server.send_raw_request("ironplc/step", params);
    let result: serde_json::Value = server.receive_response(req_id);

    assert_eq!(result["ok"], false);
    assert!(result["error"]
        .as_str()
        .unwrap()
        .contains("No program loaded"));
}

#[test]
fn stop_request_when_program_running_then_clears_session() {
    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);

    // Load and step
    let params = serde_json::json!({
        "source": "PROGRAM main\nVAR\nx : DINT;\nEND_VAR\nx := 42;\nEND_PROGRAM",
        "cycleTimeUs": 100000
    });
    let req_id = server.send_raw_request("ironplc/run", params);
    let _: serde_json::Value = server.receive_response(req_id);

    // Stop
    let stop_id = server.send_raw_request("ironplc/stop", serde_json::json!({}));
    let result: serde_json::Value = server.receive_response(stop_id);
    assert_eq!(result["ok"], true);

    // Step should fail now
    let step_id = server.send_raw_request("ironplc/step", serde_json::json!({"scans": 1}));
    let result: serde_json::Value = server.receive_response(step_id);
    assert_eq!(result["ok"], false);
}

#[test]
fn lsp_when_default_options_then_demotes_ltime_to_identifier() {
    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);

    // Send a document with LTIME used as an identifier (Edition 3 keyword demoted)
    server.send_notification::<notification::DidChangeTextDocument>(DidChangeTextDocumentParams {
        text_document: VersionedTextDocumentIdentifier {
            uri: Uri::from_str("file://example.net/test.st").unwrap(),
            version: 1,
        },
        content_changes: vec![TextDocumentContentChangeEvent {
            range: None,
            range_length: None,
            text: String::from("PROGRAM Main\nVAR\nx : LTIME;\nEND_VAR\nEND_PROGRAM"),
        }],
    });

    let diagnostics = server.receive_notification::<PublishDiagnosticsParams>();
    // LTIME should be demoted to an identifier with default (2003) options,
    // so no tokenization-level diagnostics are produced
    assert!(
        diagnostics.diagnostics.is_empty(),
        "Expected no diagnostics for LTIME with default options (demoted to identifier)"
    );
}

fn change_doc(uri: &Uri, version: i32, text: &str) -> DidChangeTextDocumentParams {
    DidChangeTextDocumentParams {
        text_document: VersionedTextDocumentIdentifier {
            uri: uri.clone(),
            version,
        },
        content_changes: vec![TextDocumentContentChangeEvent {
            range: None,
            range_length: None,
            text: text.to_string(),
        }],
    }
}

#[test]
fn multi_file_when_two_broken_files_opened_then_publishes_for_each_uri() {
    // Reproduces the user-reported bug: with two files in the
    // workspace, both broken, the LSP must publish diagnostics for
    // each URI rather than dropping the unedited file's errors.
    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);

    let uri_a = Uri::from_str("file:///workspace/a.st").unwrap();
    let uri_b = Uri::from_str("file:///workspace/b.st").unwrap();
    let key_a = UriKey::from_uri(&uri_a);
    let key_b = UriKey::from_uri(&uri_b);

    // Open file A — broken.
    server.send_notification::<notification::DidChangeTextDocument>(change_doc(
        &uri_a,
        1,
        "this is not valid IEC 61131-3 in file a",
    ));
    // After this, only A is in the project so we expect one publish for A.
    let n = server.receive_publishes(1);
    assert!(n.contains_key(&key_a));
    assert!(!n[&key_a].diagnostics.is_empty());

    // Open file B — also broken. Server now re-analyses the
    // workspace and publishes per-file: one for A, one for B.
    server.send_notification::<notification::DidChangeTextDocument>(change_doc(
        &uri_b,
        1,
        "this is not valid IEC 61131-3 in file b",
    ));
    let n = server.receive_publishes(2);
    assert!(
        n.contains_key(&key_a),
        "expected publish for url_a, got {:?}",
        n.keys().collect::<Vec<_>>()
    );
    assert!(
        n.contains_key(&key_b),
        "expected publish for url_b, got {:?}",
        n.keys().collect::<Vec<_>>()
    );
    assert!(
        !n[&key_a].diagnostics.is_empty(),
        "url_a should still report its errors after url_b is opened"
    );
    assert!(!n[&key_b].diagnostics.is_empty());

    // Version is attached only to the URI we actually edited.
    assert_eq!(n[&key_b].version, Some(1));
    assert!(n[&key_a].version.is_none());
}

#[test]
fn multi_file_when_error_fixed_in_second_file_then_clearing_diagnostics_published() {
    // Open A (broken) and B (broken), then fix B. The server must
    // emit a clearing publish (empty diagnostics) for B so the IDE
    // erases the stale squiggle, while still re-publishing A's
    // outstanding errors.
    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);

    let uri_a = Uri::from_str("file:///workspace/a.st").unwrap();
    let uri_b = Uri::from_str("file:///workspace/b.st").unwrap();
    let key_a = UriKey::from_uri(&uri_a);
    let key_b = UriKey::from_uri(&uri_b);

    server.send_notification::<notification::DidChangeTextDocument>(change_doc(
        &uri_a,
        1,
        "this is not valid IEC 61131-3 a",
    ));
    let _ = server.receive_publishes(1);

    server.send_notification::<notification::DidChangeTextDocument>(change_doc(
        &uri_b,
        1,
        "this is not valid IEC 61131-3 b",
    ));
    let _ = server.receive_publishes(2);

    // Fix B by replacing it with a valid program.
    server.send_notification::<notification::DidChangeTextDocument>(change_doc(
        &uri_b,
        2,
        "PROGRAM Main\nVAR\n  x : BOOL;\nEND_VAR\nEND_PROGRAM",
    ));

    // Expect one publish for A (still broken) plus one clearing
    // publish for B.
    let n = server.receive_publishes(2);
    assert!(n.contains_key(&key_a), "expected publish for url_a");
    assert!(n.contains_key(&key_b), "expected clear for url_b");
    assert!(!n[&key_a].diagnostics.is_empty());
    assert!(
        n[&key_b].diagnostics.is_empty(),
        "url_b's clear notification must carry an empty diagnostics list, got {:?}",
        n[&key_b].diagnostics
    );
}

#[test]
fn single_file_when_error_introduced_then_cleared_then_clearing_notification_emitted() {
    // Regression guard for the same-file case: introducing then
    // fixing an error in one file should still produce a clearing
    // publish so the squiggle goes away.
    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);

    let uri = Uri::from_str("file:///workspace/only.st").unwrap();
    let key = UriKey::from_uri(&uri);

    // Broken first.
    server.send_notification::<notification::DidChangeTextDocument>(change_doc(
        &uri,
        1,
        "this is not valid IEC 61131-3 source",
    ));
    let n = server.receive_publishes(1);
    assert!(!n[&key].diagnostics.is_empty());

    // Fix it.
    server.send_notification::<notification::DidChangeTextDocument>(change_doc(
        &uri,
        2,
        "PROGRAM Main\nVAR\n  x : BOOL;\nEND_VAR\nEND_PROGRAM",
    ));
    let n = server.receive_publishes(1);
    assert!(
        n[&key].diagnostics.is_empty(),
        "fixing the only file should emit a clear publish, got {:?}",
        n[&key].diagnostics
    );
    // The clear is in response to the edit, so the version should
    // match the edit that produced it.
    assert_eq!(n[&key].version, Some(2));
}

#[test]
fn multi_file_when_clean_file_opened_after_broken_file_then_each_publish_targets_its_uri() {
    // After a broken file is open, opening a *clean* second file
    // must not silently inherit or hide the broken one. The clean
    // file gets an empty publish; the broken one gets a re-publish
    // of its errors.
    let proj = Box::new(FileBackedProject::default());
    let mut server = TestServer::new(proj);

    let broken = Uri::from_str("file:///workspace/broken.st").unwrap();
    let clean = Uri::from_str("file:///workspace/clean.st").unwrap();
    let broken_key = UriKey::from_uri(&broken);
    let clean_key = UriKey::from_uri(&clean);

    server.send_notification::<notification::DidChangeTextDocument>(change_doc(
        &broken,
        1,
        "this is not valid IEC 61131-3 syntax",
    ));
    let _ = server.receive_publishes(1);

    server.send_notification::<notification::DidChangeTextDocument>(change_doc(
        &clean,
        1,
        "PROGRAM Clean\nVAR\n  x : BOOL;\nEND_VAR\nEND_PROGRAM",
    ));

    // Expect one publish for the broken file (re-published), and
    // one empty publish for the clean file (so the editor knows
    // the clean file has no problems).
    let n = server.receive_publishes(2);
    assert!(n.contains_key(&broken_key));
    assert!(n.contains_key(&clean_key));
    assert!(!n[&broken_key].diagnostics.is_empty());
    assert!(n[&clean_key].diagnostics.is_empty());
}

/// A project that does what a memory-backed project does and adds a message of
/// category `Text` to every analysis: no problem code is a text yet, so the
/// language server cannot get one from the analyzer.
struct WithText {
    inner: ironplc_project::MemoryBackedProject,
}

impl Project for WithText {
    fn initialize(&mut self, dir: &std::path::Path) -> Vec<ironplc_dsl::diagnostic::Diagnostic> {
        self.inner.initialize(dir)
    }

    fn change_text_document(&mut self, file_id: &ironplc_dsl::core::FileId, content: String) {
        self.inner.change_text_document(file_id, content);
    }

    fn tokenize(
        &self,
        file_id: &ironplc_dsl::core::FileId,
    ) -> (
        Vec<ironplc_parser::token::Token>,
        Vec<ironplc_dsl::diagnostic::Diagnostic>,
    ) {
        self.inner.tokenize(file_id)
    }

    fn semantic(&mut self) -> Vec<ironplc_dsl::diagnostic::Diagnostic> {
        let mut diagnostics = self.inner.semantic();
        diagnostics.push(
            ironplc_dsl::diagnostic::Diagnostic::text(
                ironplc_dsl::core::SourceSpan::default(),
                ironplc_problems::text::syntax_error_a_task_needs_a_priority(),
            )
            .with_severity_for_test(ironplc_dsl::diagnostic::Severity::Text),
        );
        diagnostics
    }

    fn semantic_context(&self) -> Option<&ironplc_analyzer::SemanticContext> {
        self.inner.semantic_context()
    }

    fn analyzed_library(&self) -> Option<&ironplc_dsl::common::Library> {
        self.inner.analyzed_library()
    }

    fn sources(&self) -> Vec<&ironplc_sources::Source> {
        self.inner.sources()
    }

    fn sources_mut(&mut self) -> Vec<&mut ironplc_sources::Source> {
        self.inner.sources_mut()
    }

    fn find(&self, file_id: &ironplc_dsl::core::FileId) -> Option<&ironplc_sources::Source> {
        self.inner.find(file_id)
    }
}

#[spec_test(REQ_DS_cli_004)]
fn text_message_when_analysis_has_one_then_sent_as_log_message_and_not_as_a_diagnostic() {
    let proj = Box::new(WithText {
        inner: ironplc_project::MemoryBackedProject::new(
            ironplc_parser::options::CompilerOptions::default(),
        ),
    });
    let mut server = TestServer::new(proj);
    server.send_notification::<notification::DidChangeTextDocument>(change_doc(
        &Uri::from_str("file:///workspace/main.st").unwrap(),
        1,
        "PROGRAM main\nEND_PROGRAM\n",
    ));

    // The edited file gets its (empty) publication first, then the log.
    let published = server.receive_notification::<PublishDiagnosticsParams>();
    let logged = server.receive_notification::<lsp_types::LogMessageParams>();

    assert!(published.diagnostics.is_empty(), "{:?}", published);
    assert_eq!(logged.typ, lsp_types::MessageType::INFO);
    assert_eq!(logged.message, "a task needs a `PRIORITY`");
}
