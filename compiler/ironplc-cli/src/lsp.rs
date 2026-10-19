//! Implements the language server protocol for integration with an IDE such
//! as Visual Studio Code.

use crossbeam_channel::{Receiver, Sender};
use ironplc_dsl::diagnostic::WarningList;
use ironplc_dsl::stack::within_stack_budget;
use ironplc_parser::options::{CompilerOptions, Dialect};
use log::{debug, trace, warn};
use lsp_server::{Connection, ExtractError, Message, RequestId};
use lsp_types::{
    notification::{self, LogMessage, Notification, PublishDiagnostics},
    request::{self, Request},
    InitializeParams, LogMessageParams, MessageType, OneOf, PublishDiagnosticsParams,
    SemanticTokens, SemanticTokensFullOptions, SemanticTokensLegend, SemanticTokensOptions,
    SemanticTokensResult, SemanticTokensServerCapabilities, ServerCapabilities,
    TextDocumentSyncCapability, TextDocumentSyncKind, Uri, WorkDoneProgressOptions,
    WorkspaceFoldersServerCapabilities, WorkspaceServerCapabilities,
};
use serde::{de::DeserializeOwned, Serialize};
use std::collections::BTreeSet;
use std::str::FromStr;

use crate::lsp_client::send_to_client;
use crate::lsp_project::{LspProject, UriKey};
use crate::semantic_tokens::TOKEN_TYPE_LEGEND;
use ironplc_project::disassemble;
use ironplc_project::FileBackedProject;

/// Start the LSP server.
///
/// The project is constructed after receiving `initializationOptions` from the
/// client so that parse options (e.g. the IEC 61131-3 standard version) can be
/// applied.
pub fn start() -> Result<(), String> {
    let (connection, io_threads) = Connection::stdio();
    let result = start_with_connection(connection, None);

    io_threads.join().map_err(|e| e.to_string())?;

    result
}

/// Converts a snake_case option key to the lowerCamelCase form used in LSP
/// `initializationOptions` (e.g. `allow_ref_to` -> `allowRefTo`,
/// `allow_c_style_comments` -> `allowCStyleComments`). Option keys are ASCII
/// snake_case, so ASCII-only casing is sufficient.
fn to_lower_camel_case(snake: &str) -> String {
    let mut out = String::with_capacity(snake.len());
    let mut capitalize = false;
    for ch in snake.chars() {
        if ch == '_' {
            capitalize = true;
        } else if capitalize {
            out.push(ch.to_ascii_uppercase());
            capitalize = false;
        } else {
            out.push(ch);
        }
    }
    out
}

/// Extract parse options from LSP initialization options.
///
/// Reads `"dialect"` to select the base preset, then overlays individual
/// `--allow-*` flags, `--policy-*` selections and the lists of warnings
/// (`disabledWarnings` and `warningsAsErrors`).  Recognised dialect values
/// come from [`Dialect::ALL`] via `FromStr`; unknown or missing values fall
/// back to the default ([`Dialect::Iec61131_3Ed2`]).
fn extract_compiler_options(initialize_params: &InitializeParams) -> CompilerOptions {
    if let Some(ref opts) = initialize_params.initialization_options {
        let dialect = opts
            .get("dialect")
            .and_then(|v| v.as_str())
            .and_then(|s| s.parse::<Dialect>().ok())
            .unwrap_or_default();
        debug!(
            "Using {} dialect from initializationOptions",
            dialect.display_name()
        );

        let mut options = CompilerOptions::from_dialect(dialect);

        // Overlay individual dialect flags (can only enable, never disable). The
        // LSP option key is the lowerCamelCase form of each descriptor's
        // `option_key` (e.g. `allow_ref_to` -> `allowRefTo`). Deriving from
        // `FEATURE_DESCRIPTORS` keeps this in lockstep with the compiler: a new
        // flag is wired in automatically, with no hand-maintained list to drift.
        for fd in CompilerOptions::FEATURE_DESCRIPTORS {
            let key = to_lower_camel_case(fd.option_key);
            if opts.get(&key).and_then(|v| v.as_bool()).unwrap_or(false) {
                options.set_flag_by_key(fd.option_key, true);
            }
        }

        // Overlay behavior policies (ADR-0049): a string naming one of the
        // policy's alternatives replaces the dialect's selection. An unknown
        // alternative is logged and ignored, like an unknown dialect.
        for pd in CompilerOptions::POLICY_DESCRIPTORS {
            let key = to_lower_camel_case(pd.option_key);
            if let Some(value) = opts.get(&key).and_then(|v| v.as_str()) {
                if options.set_policy_by_key(pd.option_key, value).is_err() {
                    debug!(
                        "Ignoring unknown alternative {value:?} for {key} in initializationOptions"
                    );
                }
            }
        }

        // Overlay the lists of warnings: an array of codes, read with the one
        // function every front end reads a code with. The key of each list is the
        // lowerCamelCase form of the key `WarningList` declares, so a list added
        // there is wired in with no edit here. A code that is not accepted is
        // logged with its message and ignored, like an unknown policy
        // alternative; the other codes of the list are used.
        for list in WarningList::ALL {
            let key = to_lower_camel_case(list.option_key());
            let Some(value) = opts.get(&key) else {
                continue;
            };
            let Some(entries) = value.as_array() else {
                warn!("Ignoring {key} in initializationOptions: it must be an array of codes");
                continue;
            };
            let codes: Vec<&str> = entries.iter().filter_map(|entry| entry.as_str()).collect();
            if codes.len() != entries.len() {
                warn!(
                    "Ignoring the entries of {key} in initializationOptions that are not strings"
                );
            }
            for refused in options.messages.add_codes(list, codes) {
                warn!("Ignoring a code of {key} in initializationOptions: {refused}");
            }
        }
        options
    } else {
        CompilerOptions::default()
    }
}

/// Start the LSP server using the connection for communication.
///
/// When `project_override` is `None`, the project is constructed from
/// `initializationOptions` received from the client. When `Some`, the
/// provided project is used directly (for testing).
///
/// The server is its loop, and the loop is a process entry: it runs on the
/// stack budget for as long as the server lives, so every request is served
/// on a thread that has the budget and the stages that a request reaches make
/// no thread of their own.
pub(crate) fn start_with_connection(
    connection: Connection,
    project_override: Option<LspProject>,
) -> Result<(), String> {
    within_stack_budget(|| serve(connection, project_override))
}

fn serve(connection: Connection, project_override: Option<LspProject>) -> Result<(), String> {
    // Declare what capabilities this server supports
    let server_capabilities =
        serde_json::to_value(LspServer::server_capabilities()).map_err(|e| e.to_string())?;

    // Send the capabilities to the client and receive back the initialization.
    let initialize_params = connection
        .initialize(server_capabilities)
        .map_err(|e| e.to_string())?;

    // Configure a project based on the initialize params
    let initialize_params: InitializeParams =
        serde_json::from_value(initialize_params).map_err(|e| e.to_string())?;

    // Build the project — use override if provided, otherwise construct from
    // initializationOptions
    let project = match project_override {
        Some(project) => project,
        None => {
            let compiler_options = extract_compiler_options(&initialize_params);
            LspProject::with_options(
                Box::new(FileBackedProject::with_options(compiler_options)),
                compiler_options,
            )
        }
    };

    let mut server = LspServer::new(&connection.sender, project);

    match initialize_params.workspace_folders {
        Some(folders) => {
            debug!("Initialize server with workspace folders {folders:?}");
            server.project.initialize_many(&folders);
        }
        None => {
            debug!("Initialize server without a workspace folder");
        }
    }

    match server.run(&connection.receiver) {
        Ok(shutdown_request) => connection
            .handle_shutdown(&shutdown_request)
            .map(|_v| ())
            .map_err(|err| err.to_string()),
        Err(err) => Err(err),
    }
}

struct LspServer<'a> {
    sender: &'a Sender<Message>,
    project: LspProject,
    /// URIs for which the server has an outstanding non-empty
    /// `publishDiagnostics`. Tracking these lets the server send an
    /// empty-diagnostics notification when previously-failing files
    /// no longer have errors, so stale squiggles get cleared instead
    /// of lingering forever. Stored as `UriKey` (a `String` newtype)
    /// rather than `Uri` so the set is independent of `lsp_types::Uri`'s
    /// interior `Cell`s.
    published_uris: BTreeSet<UriKey>,
}

impl<'a> LspServer<'a> {
    /// Returns the set of capabilities that this language server supports.
    ///
    /// This effectively declares to the other end of the channel what we can
    /// do.
    fn server_capabilities() -> ServerCapabilities {
        ServerCapabilities {
            text_document_sync: Some(TextDocumentSyncCapability::Kind(TextDocumentSyncKind::FULL)),
            semantic_tokens_provider: Some(
                SemanticTokensServerCapabilities::SemanticTokensOptions(SemanticTokensOptions {
                    // We don't report progress in generating tokens so
                    // there is no work to report on
                    work_done_progress_options: WorkDoneProgressOptions {
                        work_done_progress: None,
                    },
                    legend: SemanticTokensLegend {
                        token_types: TOKEN_TYPE_LEGEND.to_vec(),
                        token_modifiers: vec![],
                    },
                    range: None,
                    full: Some(SemanticTokensFullOptions::Bool(true)),
                }),
            ),
            workspace: Some(WorkspaceServerCapabilities {
                workspace_folders: Some(WorkspaceFoldersServerCapabilities {
                    supported: Some(true),
                    change_notifications: None,
                }),
                file_operations: None,
            }),
            document_symbol_provider: Some(OneOf::Left(true)),
            ..ServerCapabilities::default()
        }
    }

    fn new(sender: &'a Sender<Message>, project: LspProject) -> Self {
        Self {
            sender,
            project,
            published_uris: BTreeSet::new(),
        }
    }

    /// Run semantic analysis on the entire workspace and publish
    /// diagnostics for every affected file. Sends an empty-clear
    /// notification for every URI that previously had diagnostics
    /// but no longer does. The version field is attached only to the
    /// notification for `edited_uri` (the document that triggered
    /// this round of analysis); other URIs receive `version: None`,
    /// which the LSP spec permits and signals "out of band" to the
    /// client.
    fn publish_workspace_diagnostics(&mut self, edited_uri: &Uri, edited_version: Option<i32>) {
        let edited_key = UriKey::from_uri(edited_uri);
        let analysis = self.project.semantic_all();
        let by_key = analysis.problems;
        let mut new_published: BTreeSet<UriKey> = BTreeSet::new();

        for (key, diagnostics) in by_key {
            if diagnostics.is_empty() {
                continue;
            }
            let version = if key == edited_key {
                edited_version
            } else {
                None
            };
            let uri = key.to_uri();
            new_published.insert(key);
            self.send_notification::<PublishDiagnostics>(PublishDiagnosticsParams {
                uri,
                diagnostics,
                version,
            });
        }

        // Clear stale diagnostics: every URI we previously published
        // that did not appear in this round.
        let stale: Vec<UriKey> = self
            .published_uris
            .iter()
            .filter(|key| !new_published.contains(*key))
            .cloned()
            .collect();
        for key in stale {
            let version = if key == edited_key {
                edited_version
            } else {
                None
            };
            self.send_notification::<PublishDiagnostics>(PublishDiagnosticsParams {
                uri: key.to_uri(),
                diagnostics: vec![],
                version,
            });
        }

        // The edited URI must always receive a notification, even
        // when it is currently error-free and was never previously
        // published. Without this the editor would never see the
        // initial "no problems" state for a freshly-opened file and
        // the LSP test harness — which expects one notification per
        // edit — would block.
        if !new_published.contains(&edited_key) && !self.published_uris.contains(&edited_key) {
            self.send_notification::<PublishDiagnostics>(PublishDiagnosticsParams {
                uri: edited_uri.clone(),
                diagnostics: vec![],
                version: edited_version,
            });
        }

        self.published_uris = new_published;

        // A text is a line about the build, not a problem of a file: the
        // editor shows it in the Output channel. `INFO`, not `LOG`: the
        // language client writes a `LOG` message at the debug level, which the
        // channel hides by default.
        for message in analysis.log {
            self.send_notification::<LogMessage>(LogMessageParams {
                typ: MessageType::INFO,
                message,
            });
        }
    }

    /// The main event loop. The event loop receives messages from the other
    /// end of the channel.
    fn run(&mut self, receiver: &Receiver<Message>) -> Result<lsp_server::Request, String> {
        for msg in receiver {
            match msg {
                lsp_server::Message::Request(req) => {
                    if req.method == request::Shutdown::METHOD {
                        return Ok(req);
                    }
                    self.handle_request(req);
                }
                lsp_server::Message::Response(_) => {
                    // LSP responses are typically handled by the client, not the server
                    // For now, we just ignore them
                }
                lsp_server::Message::Notification(notification) => {
                    self.handle_notification(notification);
                }
            }
        }

        Err("terminated but no shutdown".to_owned())
    }

    fn handle_request(&mut self, req: lsp_server::Request) -> &'static str {
        let req_id = req.id.clone();
        let req = match Self::cast_request::<request::Shutdown>(req) {
            Ok(_params) => {
                return request::Shutdown::METHOD;
            }
            Err(req) => req,
        };
        let req = match Self::cast_request::<request::SemanticTokensFullRequest>(req) {
            Ok(params) => {
                let uri = params.text_document.uri;
                let tokens = self.project.tokenize(&uri);
                trace!("SemanticTokensFullRequest Response {tokens:?}");
                self.send_response::<request::SemanticTokensFullRequest>(
                    req_id,
                    Some(SemanticTokensResult::Tokens(SemanticTokens {
                        result_id: None,
                        data: tokens,
                    })),
                );

                return request::SemanticTokensFullRequest::METHOD;
            }
            Err(req) => req,
        };
        let _req = match Self::cast_request::<request::DocumentSymbolRequest>(req) {
            Ok(params) => {
                let uri = params.text_document.uri;
                let symbols = self.project.document_symbols(&uri);

                trace!("DocumentSymbolRequest Response {symbols:?}");
                self.send_response::<request::DocumentSymbolRequest>(req_id, Some(symbols));

                return request::DocumentSymbolRequest::METHOD;
            }
            Err(req) => req,
        };

        // Handle custom requests by method name
        if _req.method == "ironplc/disassemble" {
            let params: serde_json::Value = serde_json::from_value(_req.params).unwrap_or_default();
            let uri_str = params["uri"].as_str().unwrap_or("");

            let result = match lsp_types::Uri::from_str(uri_str) {
                Ok(uri) => {
                    let path_str = uri_to_file_path(&uri);
                    let path = std::path::Path::new(&path_str);
                    disassemble::disassemble_file(path)
                }
                Err(_) => serde_json::json!({"error": "Invalid URI"}),
            };

            let response = lsp_server::Response::new_ok(req_id, result);
            send_to_client(self.sender, lsp_server::Message::Response(response));
            return "ironplc/disassemble";
        }

        if _req.method == "ironplc/run" {
            let params: serde_json::Value = serde_json::from_value(_req.params).unwrap_or_default();
            let source = params["source"].as_str().unwrap_or("");
            let cycle_time_us = params["cycleTimeUs"].as_u64().unwrap_or(100_000);

            let result = self.project.run_load(source, cycle_time_us);
            let response = lsp_server::Response::new_ok(req_id, result);
            send_to_client(self.sender, lsp_server::Message::Response(response));
            return "ironplc/run";
        }

        if _req.method == "ironplc/step" {
            let params: serde_json::Value = serde_json::from_value(_req.params).unwrap_or_default();
            let scans = params["scans"].as_u64().unwrap_or(1) as u32;

            let result = self.project.run_step(scans);
            let response = lsp_server::Response::new_ok(req_id, result);
            send_to_client(self.sender, lsp_server::Message::Response(response));
            return "ironplc/step";
        }

        if _req.method == "ironplc/stop" {
            let result = self.project.run_stop();
            let response = lsp_server::Response::new_ok(req_id, result);
            send_to_client(self.sender, lsp_server::Message::Response(response));
            return "ironplc/stop";
        }

        ""
    }

    #[expect(
        clippy::panic,
        reason = "ExtractError::JsonError consumes the request, so it cannot be handed back; mirrors rust-analyzer's cast pattern"
    )]
    fn cast_request<T>(request: lsp_server::Request) -> Result<T::Params, lsp_server::Request>
    where
        T: lsp_types::request::Request,
        T::Params: DeserializeOwned,
    {
        request
            .extract(T::METHOD)
            .map(|val| val.1)
            .map_err(|e| match e {
                ExtractError::MethodMismatch(n) => n,
                err @ ExtractError::JsonError { .. } => panic!("Invalid request: {err:?}"),
            })
    }

    fn send_response<R>(&self, request_id: RequestId, params: R::Result)
    where
        R: lsp_types::request::Request,
        R::Result: Serialize,
    {
        trace!("Response for method {}", R::METHOD);
        let response = lsp_server::Response::new_ok(request_id, params);
        send_to_client(self.sender, Message::Response(response));
    }

    fn handle_notification(&mut self, notification: lsp_server::Notification) -> &'static str {
        let notification = match Self::cast_notification::<notification::Exit>(notification) {
            Ok(_params) => {
                return notification::Exit::METHOD;
            }
            Err(notification) => notification,
        };

        let notification =
            match Self::cast_notification::<notification::DidOpenTextDocument>(notification) {
                Ok(params) => {
                    trace!("DidOpenTextDocument {}", params.text_document.uri.as_str());
                    let contents = params.text_document.text;
                    let uri = params.text_document.uri;
                    let version = params.text_document.version;

                    self.project
                        .change_text_document(&uri, contents.as_str().to_string());
                    self.publish_workspace_diagnostics(&uri, Some(version));

                    return notification::DidOpenTextDocument::METHOD;
                }
                Err(notification) => notification,
            };

        let _notification =
            match Self::cast_notification::<notification::DidChangeTextDocument>(notification) {
                Ok(params) => {
                    trace!(
                        "DidChangeTextDocument {}",
                        params.text_document.uri.as_str()
                    );
                    let Some(change) = params.content_changes.into_iter().next() else {
                        // A DidChange with no changes carries nothing to apply.
                        return "";
                    };
                    let contents = change.text;
                    let uri = params.text_document.uri;
                    let version = params.text_document.version;

                    self.project
                        .change_text_document(&uri, contents.as_str().to_string());
                    self.publish_workspace_diagnostics(&uri, Some(version));

                    return notification::DidChangeTextDocument::METHOD;
                }
                Err(notification) => notification,
            };

        ""
    }

    #[expect(
        clippy::panic,
        reason = "ExtractError::JsonError consumes the notification, so it cannot be handed back; mirrors rust-analyzer's cast pattern"
    )]
    fn cast_notification<T>(
        notification: lsp_server::Notification,
    ) -> Result<T::Params, lsp_server::Notification>
    where
        T: lsp_types::notification::Notification,
        T::Params: DeserializeOwned,
    {
        notification.extract(T::METHOD).map_err(|e| match e {
            ExtractError::MethodMismatch(n) => n,
            err @ ExtractError::JsonError { .. } => panic!("Invalid notification: {err:?}"),
        })
    }

    fn send_notification<N>(&self, params: N::Params)
    where
        N: lsp_types::notification::Notification,
        N::Params: Serialize,
    {
        let notification = lsp_server::Notification::new(N::METHOD.to_string(), params);
        send_to_client(self.sender, Message::Notification(notification));
    }
}

/// Converts a `file:` URI to a filesystem path string.
///
/// On Windows, file URIs have the form `file:///C:/path` where the URI path
/// component is `/C:/path`. This function strips the leading `/` so the
/// result is a valid Windows path like `C:/path`.
fn uri_to_file_path(uri: &lsp_types::Uri) -> String {
    let path = uri.path().as_str();
    // On Windows, strip the leading / before the drive letter (e.g. /C:/foo -> C:/foo)
    #[cfg(windows)]
    if path.len() >= 3 && path.as_bytes()[0] == b'/' && path.as_bytes()[2] == b':' {
        return path[1..].to_string();
    }
    path.to_string()
}

#[cfg(test)]
mod tests;
