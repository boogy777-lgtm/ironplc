//! The stack budget of the language server: the thread that serves requests is
//! given the budget once, when the server starts, and a request costs no thread
//! after that.
//!
//! The project the server holds is called on the serving thread, so a project
//! that records what that thread is, at each call, shows what a request runs
//! on.

use std::sync::{Arc, Mutex};
use std::time::Duration;

use ironplc_dsl::{
    common::Library,
    core::FileId,
    diagnostic::Diagnostic,
    stack::{is_on_budget, spawns_by_current_thread},
};
use ironplc_parser::{options::CompilerOptions, token::Token};
use ironplc_project::{MemoryBackedProject, Project};
use ironplc_sources::Source;
use lsp_server::{Connection, Message, Request, RequestId};
use serde_json::json;

use crate::lsp::start_with_connection;
use crate::lsp_project::LspProject;

/// What the serving thread was, at one call of the project.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
struct Served {
    on_budget: bool,
    /// How many threads the serving thread has made, since it started.
    spawns: usize,
}

type Log = Arc<Mutex<Vec<Served>>>;

/// A project that does what a memory-backed project does and records, after
/// each analysis and each tokenization, what the calling thread is.
struct Recording {
    inner: MemoryBackedProject,
    log: Log,
}

impl Recording {
    fn record(&self) {
        if let Ok(mut log) = self.log.lock() {
            log.push(Served {
                on_budget: is_on_budget(),
                spawns: spawns_by_current_thread(),
            });
        }
    }
}

impl Project for Recording {
    fn initialize(&mut self, dir: &std::path::Path) -> Vec<Diagnostic> {
        self.inner.initialize(dir)
    }

    fn change_text_document(&mut self, file_id: &FileId, content: String) {
        self.inner.change_text_document(file_id, content);
    }

    fn tokenize(&self, file_id: &FileId) -> (Vec<Token>, Vec<Diagnostic>) {
        let tokens = self.inner.tokenize(file_id);
        self.record();
        tokens
    }

    fn semantic(&mut self) -> Vec<Diagnostic> {
        let diagnostics = self.inner.semantic();
        self.record();
        diagnostics
    }

    fn semantic_context(&self) -> Option<&ironplc_analyzer::SemanticContext> {
        self.inner.semantic_context()
    }

    fn analyzed_library(&self) -> Option<&Library> {
        self.inner.analyzed_library()
    }

    fn sources(&self) -> Vec<&Source> {
        self.inner.sources()
    }

    fn sources_mut(&mut self) -> Vec<&mut Source> {
        self.inner.sources_mut()
    }

    fn find(&self, file_id: &FileId) -> Option<&Source> {
        self.inner.find(file_id)
    }
}

const URI: &str = "file:///workspace/main.st";
const PROGRAM: &str = "PROGRAM main VAR x : INT; END_VAR x := 1; END_PROGRAM";

/// The client: what it sends and what it waits for.
struct Client {
    connection: Connection,
    next_id: i32,
}

impl Client {
    fn receive(&self) -> Message {
        self.connection
            .receiver
            .recv_timeout(Duration::from_secs(60))
            .expect("the server answers within a minute")
    }

    fn request(&mut self, method: &str, params: serde_json::Value) {
        self.next_id += 1;
        let request = Request {
            id: RequestId::from(self.next_id),
            method: method.to_string(),
            params,
        };
        self.connection
            .sender
            .send(Message::Request(request))
            .expect("the server is listening");
        // The response, ignoring any notification the request caused.
        loop {
            if let Message::Response(_) = self.receive() {
                return;
            }
        }
    }

    fn notify(&self, method: &str, params: serde_json::Value) {
        self.connection
            .sender
            .send(Message::Notification(lsp_server::Notification {
                method: method.to_string(),
                params,
            }))
            .expect("the server is listening");
    }

    /// An edit of the document, and the diagnostics the server publishes for it.
    fn edit(&self, version: i32) {
        self.notify(
            "textDocument/didChange",
            json!({
                "textDocument": {"uri": URI, "version": version},
                "contentChanges": [{"text": PROGRAM}],
            }),
        );
        loop {
            if let Message::Notification(_) = self.receive() {
                return;
            }
        }
    }
}

/// Starts a server over the recording project, makes three edits and one
/// request for the semantic tokens, and stops it. Returns what the project
/// recorded at each of its calls, and how many threads the thread that started
/// the server made.
fn serve_three_edits() -> (Vec<Served>, usize) {
    let log: Log = Arc::default();
    let project = LspProject::new(Box::new(Recording {
        inner: MemoryBackedProject::new(CompilerOptions::default()),
        log: log.clone(),
    }));
    let (server_connection, client_connection) = Connection::memory();
    let server = std::thread::spawn(move || {
        let started = start_with_connection(server_connection, Some(project));
        (started, spawns_by_current_thread())
    });

    let mut client = Client {
        connection: client_connection,
        next_id: 0,
    };
    client.request(
        "initialize",
        json!({"processId": null, "rootUri": null, "capabilities": {}}),
    );
    client.notify("initialized", json!({}));
    for version in 1..=3 {
        client.edit(version);
    }
    client.request(
        "textDocument/semanticTokens/full",
        json!({"textDocument": {"uri": URI}}),
    );
    client.request("shutdown", json!(null));
    client.notify("exit", json!(null));

    let (started, spawned_for_the_server) = server.join().expect("the server thread ends");
    assert_eq!(started, Ok(()));
    let served = log.lock().expect("the log is not poisoned").clone();
    (served, spawned_for_the_server)
}

#[test]
fn start_with_connection_when_edits_are_served_then_every_request_runs_on_the_budget() {
    let (served, _) = serve_three_edits();

    // An analysis for each of the three edits and one tokenization.
    assert_eq!(served.len(), 4, "{served:?}");
    assert!(
        served.iter().all(|call| call.on_budget),
        "a request ran on a thread without the budget: {served:?}"
    );
}

#[test]
fn start_with_connection_when_edits_are_served_then_no_request_makes_a_thread() {
    let (served, _) = serve_three_edits();

    assert!(
        served.iter().all(|call| call.spawns == 0),
        "a request made a thread: {served:?}"
    );
}

#[test]
fn start_with_connection_when_server_runs_then_it_makes_one_thread_for_its_whole_life() {
    let (_, spawned_for_the_server) = serve_three_edits();

    assert_eq!(spawned_for_the_server, 1);
}
