//! The `hot_edit_*` MCP tools.
//!
//! Six thin wrappers over the runtime host's hot-edit command layer
//! ([`ironplc_runtime`]): `hot_edit_status`, `hot_edit_accept`,
//! `hot_edit_test`, `hot_edit_untest`, `hot_edit_assemble`, and
//! `hot_edit_cancel`. The tools own no protocol logic of their own: every
//! call maps onto one [`Command`], and every refusal comes back as the
//! command layer's stable V-code plus the host's message. The server only
//! serializes.
//!
//! # Session state
//!
//! Unlike the analysis tools, the hot-edit tools are stateful: the protocol
//! operates on *the running application*. The server therefore keeps one
//! [`HotEditSession`] — a single [`RuntimeHost`] behind a mutex, next to the
//! container cache, following `cache.rs`'s `Arc<Mutex<..>>` convention.
//!
//! The lifecycle choice, deliberately minimal: at most one session exists per
//! server process, the first `hot_edit_accept` establishes it from the
//! compiled sources, and later accepts stage candidates against it. There is
//! no session keying, table, or eviction — restarting the server resets the
//! session, exactly as restarting `ironplcvm serve` does, which also hosts
//! exactly one running application per process. If a client ever needs
//! parallel sessions keyed by program identity, this type is the one place
//! to grow (ADR-0056).
//!
//! # Scan driving
//!
//! `test`/`untest` only *request* a swap; the host applies it at the next
//! scan boundary ([`RuntimeHost::run`]). Where the CLI `serve` session leaves
//! that to its caller, each MCP tool that advances the FSM (`hot_edit_test`,
//! `hot_edit_untest`, `hot_edit_assemble`) drives one scan round after an
//! acknowledgment, so the state an agent reads back has actually switched.
//! The round runs at a constant zero uptime — the convention of the runtime
//! acceptance tests — where every task is ready immediately after a (re)load,
//! so the boundary round executes each task exactly once. A trap in that
//! round surfaces the trap's own V-code.

use std::io::Cursor;
use std::sync::Mutex;

use ironplc_container::Container;
use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_problems::Problem;
use ironplc_runtime::{
    execute, Command, CommandError, Response, RuntimeError, RuntimeHost, StatusPayload,
};
use serde::Serialize;

use super::common::{serialize_diagnostic, serialize_diagnostics, SourceInput};
use crate::cache::ContainerCache;

/// The server's single hot-edit session: the running application the
/// protocol operates on.
///
/// See the module documentation for the lifecycle. `None` means no session
/// has been established yet; every tool except `hot_edit_accept` refuses with
/// a P8001 diagnostic in that state.
#[derive(Default)]
pub struct HotEditSession {
    host: Option<RuntimeHost>,
}

/// Top-level response shared by every `hot_edit_*` tool.
#[derive(Debug, Serialize)]
pub struct HotEditResponse {
    /// Whether the command succeeded.
    pub ok: bool,
    /// The acknowledgment word: `"established"` when `hot_edit_accept`
    /// started the session, `"ack"` for every other success. Null on failure.
    pub result: Option<String>,
    /// The host's hot-edit status after the command: mode, generation
    /// counters, migration flag, and completed scan rounds. Null on failure.
    pub status: Option<StatusPayload>,
    /// The stable V-code when the command layer refused the command (e.g.
    /// `"V4011"`) or the VM trapped during a driven scan round. Null when the
    /// failure never reached the command layer (compile diagnostics, a
    /// missing session, an internal invariant).
    pub v_code: Option<String>,
    /// The failure message accompanying `v_code`.
    pub message: Option<String>,
    /// Compiler diagnostics: compile failures on `hot_edit_accept`, and
    /// validation failures (such as calling a tool before any session
    /// exists) with code P8001.
    pub diagnostics: Vec<serde_json::Value>,
}

impl HotEditResponse {
    /// Builds the success response carrying the post-command status snapshot.
    fn ok(result: &str, status: StatusPayload) -> Self {
        HotEditResponse {
            ok: true,
            result: Some(result.to_string()),
            status: Some(status),
            v_code: None,
            message: None,
            diagnostics: vec![],
        }
    }

    /// Builds the failure response for a command-layer refusal. The V-code
    /// and message are the command layer's whole contract (ADR-0055), so the
    /// tool forwards them verbatim and adds nothing of its own.
    fn command_error(error: &CommandError) -> Self {
        HotEditResponse {
            ok: false,
            result: None,
            status: None,
            v_code: Some(error.v_code().to_string()),
            message: Some(error.message().to_string()),
            diagnostics: vec![],
        }
    }

    /// Builds the failure response for a host or VM failure: session
    /// creation (an init trap) or a driven scan round that trapped.
    fn runtime_error(error: RuntimeError) -> Self {
        match error {
            RuntimeError::Trap(context) => HotEditResponse {
                ok: false,
                result: None,
                status: None,
                v_code: Some(context.trap.v_code().to_string()),
                message: Some(format!(
                    "runtime error: {} (task {}, instance {})",
                    context.trap,
                    context.task_id.raw(),
                    context.instance_id.raw()
                )),
                diagnostics: vec![],
            },
            RuntimeError::Internal { reason } => {
                internal_failure(format!("runtime host invariant violated: {reason}"))
            }
        }
    }
}

/// Builds the failure response for a P8001 input/session validation error,
/// the same convention the `run` tool uses for unknown container handles.
fn validation_failure(message: &str) -> HotEditResponse {
    HotEditResponse {
        ok: false,
        result: None,
        status: None,
        v_code: None,
        message: None,
        diagnostics: serialize_diagnostics(&[Diagnostic::problem(
            Problem::McpInputValidation,
            Label::span(SourceSpan::default(), message),
        )]),
    }
}

/// Builds the failure response for a violated internal invariant — reachable
/// only through a bug, never through input, so it carries an internal
/// diagnostic rather than a user-facing code.
fn internal_failure(message: String) -> HotEditResponse {
    HotEditResponse {
        ok: false,
        result: None,
        status: None,
        v_code: None,
        message: None,
        diagnostics: vec![serialize_diagnostic(&Diagnostic::internal_error_at(
            Label::span(SourceSpan::default(), message),
        ))],
    }
}

/// Runs one command against the session host and shapes the response.
///
/// `drive_scan` is set for the FSM-advancing commands: their effect applies
/// at the next scan boundary, so after an acknowledgment one round is driven
/// through [`RuntimeHost::run`] before the status snapshot is taken (see the
/// module documentation for the clock convention).
fn dispatch(command: Command, session: &mut HotEditSession, drive_scan: bool) -> HotEditResponse {
    let Some(host) = session.host.as_mut() else {
        return validation_failure(
            "no hot edit session; call `hot_edit_accept` with a program to start one",
        );
    };

    match execute(command, host) {
        Response::Status(status) => HotEditResponse::ok("ack", status),
        Response::Ack => {
            if drive_scan {
                if let Err(error) = host.run(1, || 0) {
                    return HotEditResponse::runtime_error(error);
                }
            }
            HotEditResponse::ok("ack", StatusPayload::from(host.status()))
        }
        Response::Error(error) => HotEditResponse::command_error(&error),
    }
}

/// Builds the `hot_edit_status` response: the session's status payload.
pub fn build_status_response(session: &Mutex<HotEditSession>) -> HotEditResponse {
    let mut guard = session.lock().unwrap_or_else(|e| e.into_inner());
    dispatch(Command::GetStatus, &mut guard, false)
}

/// Builds the `hot_edit_accept` response.
///
/// Compiles `sources` through the same pipeline as the `compile` tool, then:
///
/// - no session yet: the compiled container becomes the running application
///   (the session is established and the result is `"established"`); or
/// - session running: the compiled container is staged through the command
///   layer's [`Command::AcceptEdits`], so every refusal (V4007 layout, V4008
///   schedule, V4013 already staged, ...) comes back with its stable V-code.
pub fn build_accept_response(
    sources: &[SourceInput],
    options_value: &serde_json::Value,
    cache: &Mutex<ContainerCache>,
    session: &Mutex<HotEditSession>,
) -> HotEditResponse {
    // The compile tool owns the pipeline; calling it keeps `hot_edit_accept`
    // on exactly the compile path the `compile` tool serves, diagnostics and
    // container cache included, so the two can never drift apart.
    let compiled = crate::tools::compile::build_response(sources, options_value, false, cache);
    if !compiled.ok {
        return HotEditResponse {
            ok: false,
            result: None,
            status: None,
            v_code: None,
            message: None,
            diagnostics: compiled.diagnostics,
        };
    }
    let Some(container_id) = compiled.container_id else {
        return internal_failure("compile succeeded without a container id".to_string());
    };
    let program = {
        let mut guard = cache.lock().unwrap_or_else(|e| e.into_inner());
        guard
            .get(&container_id)
            .map(|cached| cached.iplc_bytes.clone())
    };
    let Some(program) = program else {
        return internal_failure(format!(
            "compiled container '{container_id}' was evicted before it could be staged"
        ));
    };

    let mut guard = session.lock().unwrap_or_else(|e| e.into_inner());
    if guard.host.is_none() {
        return establish(program, &mut guard);
    }
    dispatch(Command::AcceptEdits { program }, &mut guard, false)
}

/// Creates the session from the compiled program's wire bytes.
///
/// The first accept has nothing to stage against: it loads the compiled
/// program as the running application, mirroring how `ironplcvm serve` loads
/// its program before serving commands. The command layer is not involved —
/// `AcceptEdits` only stages a candidate against a running artifact, and
/// here none exists yet. An init trap refuses the establishment: nothing
/// runs, the session stays empty, and the caller can fix the source and
/// retry.
fn establish(program: Vec<u8>, session: &mut HotEditSession) -> HotEditResponse {
    let container = match Container::read_from(&mut Cursor::new(&program)) {
        Ok(container) => container,
        // Unreachable in principle — the bytes come from this server's own
        // compiler. A refusal, not a panic, per the workspace no-panic rule.
        Err(error) => {
            return internal_failure(format!("compiled container failed to re-parse: {error}"));
        }
    };
    let host = match RuntimeHost::new(container) {
        Ok(host) => host,
        Err(error) => return HotEditResponse::runtime_error(error),
    };
    let status = StatusPayload::from(host.status());
    session.host = Some(host);
    HotEditResponse::ok("established", status)
}

/// Builds the `hot_edit_test` response: activates the staged candidate at the
/// next scan boundary and drives one scan round so the candidate is
/// executing when the call returns.
pub fn build_test_response(session: &Mutex<HotEditSession>) -> HotEditResponse {
    let mut guard = session.lock().unwrap_or_else(|e| e.into_inner());
    dispatch(Command::TestEdits, &mut guard, true)
}

/// Builds the `hot_edit_untest` response: reverts to the original artifact at
/// the next scan boundary (process state is preserved) and drives one scan
/// round so the revert is executing when the call returns.
pub fn build_untest_response(session: &Mutex<HotEditSession>) -> HotEditResponse {
    let mut guard = session.lock().unwrap_or_else(|e| e.into_inner());
    dispatch(Command::UntestEdits, &mut guard, true)
}

/// Builds the `hot_edit_assemble` response: promotes the staged candidate to
/// the running application and drives one scan round. Allowed from both the
/// staged and the testing phase.
pub fn build_assemble_response(session: &Mutex<HotEditSession>) -> HotEditResponse {
    let mut guard = session.lock().unwrap_or_else(|e| e.into_inner());
    dispatch(Command::AssembleEdits, &mut guard, true)
}

/// Builds the `hot_edit_cancel` response: discards the staged candidate while
/// the original artifact keeps running. Takes effect immediately, so no scan
/// round is driven.
pub fn build_cancel_response(session: &Mutex<HotEditSession>) -> HotEditResponse {
    let mut guard = session.lock().unwrap_or_else(|e| e.into_inner());
    dispatch(Command::CancelEdits, &mut guard, false)
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::tools::test_support::{ed2_options, source, COUNTER_PROGRAM, SYNTAX_ERROR_PROGRAM};
    use ironplc_container::VarIndex;
    use ironplc_runtime::HostMode;

    fn make_state() -> (Mutex<ContainerCache>, Mutex<HotEditSession>) {
        (
            Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024)),
            Mutex::new(HotEditSession::default()),
        )
    }

    /// `COUNTER_PROGRAM` with the scan body replaced — the declarations are
    /// untouched, so two compiles share a layout and differ only in logic.
    fn counter_variant(body: &str) -> String {
        format!(
            "PROGRAM Main
VAR
  Counter : INT;
END_VAR
  {body}
END_PROGRAM"
        )
    }

    /// Compiles `source` through the project pipeline and round-trips the
    /// wire format, mirroring `runtime/tests/common`.
    fn compile_container(source: &str) -> Container {
        compile_container_with_ids(source, &[])
    }

    /// [`compile_container`] with an engineering-side stable variable ID
    /// table (ADR-0053), so a declaration edit stages as a migration
    /// candidate. The tool surface has no way to supply IDs — they arrive
    /// with PLCopen UID storage (roadmap Phase 3+) — so the schema-edit
    /// scenario establishes its session directly instead of through
    /// `build_accept_response`.
    fn compile_container_with_ids(source: &str, ids: &[(&str, u64)]) -> Container {
        use ironplc_parser::options::CompilerOptions;
        use ironplc_project::{compile, MemoryBackedProject};

        let options = CompilerOptions::default();
        let mut project = MemoryBackedProject::new(options);
        project.add_source(
            ironplc_dsl::core::FileId::from_string("main.st"),
            source.to_owned(),
        );
        if !ids.is_empty() {
            project.set_stable_var_ids(
                ids.iter()
                    .map(|(name, uid)| (ironplc_project::SidecarKey::new("main", name), *uid))
                    .collect(),
            );
        }

        let output = compile(
            &mut project,
            &options,
            &ironplc_codegen::EmptyLookup,
            vec![],
        );
        assert!(
            output.diagnostics.is_empty(),
            "fixture must compile: {:?}",
            output.diagnostics
        );

        let container = output.container.unwrap();
        let mut bytes = Vec::new();
        container.write_to(&mut bytes).unwrap();
        Container::read_from(&mut Cursor::new(&bytes)).unwrap()
    }

    /// Finds a variable's table index through the debug section.
    fn variable_index(container: &Container, name: &str) -> VarIndex {
        container
            .debug_section
            .as_ref()
            .unwrap()
            .var_names
            .iter()
            .find(|entry| entry.name.eq_ignore_ascii_case(name))
            .unwrap()
            .var_index
    }

    /// Establishes a session from `COUNTER_PROGRAM`, leaving no candidate.
    fn establish_counter_session(cache: &Mutex<ContainerCache>, session: &Mutex<HotEditSession>) {
        let resp = build_accept_response(&source(COUNTER_PROGRAM), &ed2_options(), cache, session);
        assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
        assert_eq!(resp.result.as_deref(), Some("established"));
    }

    /// Establishes a session and stages `body` as a logic-only candidate.
    fn session_with_staged_candidate(
        cache: &Mutex<ContainerCache>,
        session: &Mutex<HotEditSession>,
        body: &str,
    ) {
        establish_counter_session(cache, session);
        let edit = counter_variant(body);
        let resp = build_accept_response(&source(&edit), &ed2_options(), cache, session);
        assert!(
            resp.ok,
            "v_code: {:?} message: {:?}",
            resp.v_code, resp.message
        );
    }

    #[test]
    fn build_accept_response_when_no_session_then_establishes_running_application() {
        let (cache, session) = make_state();

        let resp =
            build_accept_response(&source(COUNTER_PROGRAM), &ed2_options(), &cache, &session);

        assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
        assert_eq!(resp.result.as_deref(), Some("established"));
        let status = resp.status.unwrap();
        assert_eq!(status.mode, HostMode::Normal);
        assert_eq!(status.active, 1);
        assert_eq!(status.normal, 1);
        assert_eq!(status.candidate, None);
        assert_eq!(status.rounds, 0);
    }

    #[test]
    fn build_accept_response_when_session_running_then_stages_candidate() {
        let (cache, session) = make_state();
        establish_counter_session(&cache, &session);

        let edit = counter_variant("Counter := Counter + 10;");
        let resp = build_accept_response(&source(&edit), &ed2_options(), &cache, &session);

        assert!(resp.ok);
        assert_eq!(resp.result.as_deref(), Some("ack"));
        let status = resp.status.unwrap();
        assert_eq!(status.mode, HostMode::Normal);
        assert_eq!(status.candidate, Some(2));
    }

    #[test]
    fn build_accept_response_when_syntax_error_then_compile_diagnostics_and_no_session() {
        let (cache, session) = make_state();

        let resp = build_accept_response(
            &source(SYNTAX_ERROR_PROGRAM),
            &ed2_options(),
            &cache,
            &session,
        );

        assert!(!resp.ok);
        assert!(resp.v_code.is_none());
        assert!(!resp.diagnostics.is_empty());
        assert!(resp
            .diagnostics
            .iter()
            .all(|d| d["code"].as_str().is_some_and(|code| code.starts_with('P'))));
        // Nothing was established: the next status call still finds no session.
        let status = build_status_response(&session);
        assert!(!status.ok);
        assert!(status.diagnostics.iter().any(|d| d["code"] == "P8001"));
    }

    #[test]
    fn build_accept_response_when_layout_change_without_stable_ids_then_v4007() {
        let (cache, session) = make_state();
        establish_counter_session(&cache, &session);

        // Adding a variable is a declaration edit: without stable variable
        // IDs the host cannot justify carrying state over, so the stage is
        // refused (with IDs it would stage as a migration candidate).
        let schema_edit = "PROGRAM Main
VAR
  Counter : INT;
  Extra : INT;
END_VAR
  Counter := Counter + 1;
END_PROGRAM";
        let resp = build_accept_response(&source(schema_edit), &ed2_options(), &cache, &session);

        assert!(!resp.ok);
        assert_eq!(resp.v_code.as_deref(), Some("V4007"));
        assert!(resp.message.unwrap().contains("layout"));
    }

    #[test]
    fn build_accept_response_when_candidate_already_staged_then_v4013() {
        let (cache, session) = make_state();
        session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");

        let second = counter_variant("Counter := Counter + 20;");
        let resp = build_accept_response(&source(&second), &ed2_options(), &cache, &session);

        assert!(!resp.ok);
        assert_eq!(resp.v_code.as_deref(), Some("V4013"));
        // The first candidate is still the one that is staged.
        let status = build_status_response(&session);
        assert_eq!(status.status.unwrap().candidate, Some(2));
    }

    #[test]
    fn build_status_response_when_no_session_then_p8001_diagnostic() {
        let (_cache, session) = make_state();

        let resp = build_status_response(&session);

        assert!(!resp.ok);
        assert!(resp.diagnostics.iter().any(|d| d["code"] == "P8001"));
    }

    #[test]
    fn build_test_response_when_no_session_then_p8001_diagnostic() {
        let (_cache, session) = make_state();

        let resp = build_test_response(&session);

        assert!(!resp.ok);
        assert!(resp.diagnostics.iter().any(|d| d["code"] == "P8001"));
    }

    #[test]
    fn build_test_response_when_no_candidate_staged_then_v4012() {
        let (cache, session) = make_state();
        establish_counter_session(&cache, &session);

        let resp = build_test_response(&session);

        assert!(!resp.ok);
        assert_eq!(resp.v_code.as_deref(), Some("V4012"));
        assert_eq!(resp.message.unwrap(), "no candidate is staged");
    }

    #[test]
    fn build_test_response_when_candidate_staged_then_swap_applies_at_scan_boundary() {
        let (cache, session) = make_state();
        session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");

        let resp = build_test_response(&session);

        assert!(resp.ok);
        assert_eq!(resp.result.as_deref(), Some("ack"));
        let status = resp.status.unwrap();
        assert_eq!(status.mode, HostMode::Testing);
        assert_eq!(status.active, 2);
        assert_eq!(status.candidate, Some(2));
        // The tool drove exactly one scan round: the swap was applied and the
        // candidate's logic executed (Counter went 0 -> 10).
        assert_eq!(status.rounds, 1);
        let guard = session.lock().unwrap();
        let container = compile_container(&counter_variant("Counter := Counter + 10;"));
        let index = variable_index(&container, "Counter");
        assert_eq!(
            guard.host.as_ref().unwrap().read_variable(index).unwrap(),
            10
        );
    }

    #[test]
    fn build_untest_response_when_test_in_progress_then_reverts_code_and_keeps_state() {
        let (cache, session) = make_state();
        session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");
        let tested = build_test_response(&session);
        assert!(tested.ok);

        let resp = build_untest_response(&session);

        assert!(resp.ok);
        let status = resp.status.unwrap();
        assert_eq!(status.mode, HostMode::Normal);
        assert_eq!(status.active, 1);
        // The second driven round: untest swapped back and the base artifact
        // executed once more on top of the candidate's state (10 -> 11).
        assert_eq!(status.rounds, 2);
        let guard = session.lock().unwrap();
        let container = compile_container(COUNTER_PROGRAM);
        let index = variable_index(&container, "Counter");
        assert_eq!(
            guard.host.as_ref().unwrap().read_variable(index).unwrap(),
            11
        );
    }

    #[test]
    fn build_untest_response_when_no_test_in_progress_then_v4014() {
        let (cache, session) = make_state();
        session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");

        let resp = build_untest_response(&session);

        assert!(!resp.ok);
        assert_eq!(resp.v_code.as_deref(), Some("V4014"));
        assert_eq!(resp.message.unwrap(), "no test is in progress");
    }

    #[test]
    fn build_untest_response_when_schema_edit_tested_then_v4011_and_candidate_keeps_running() {
        // The tool surface cannot supply stable variable IDs (PLCopen UID
        // storage is roadmap Phase 3+), so this scenario establishes the
        // session and stages the migration candidate through the command
        // layer directly — the same calls `build_accept_response` makes
        // internally once the compiled bytes exist.
        let (_cache, session) = make_state();
        let base = compile_container_with_ids(
            "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
            &[("Counter", 1)],
        );
        let schema_edit = compile_container_with_ids(
            "PROGRAM main
  VAR
    Counter : DINT;
    Extra : DINT := 42;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
            &[("Counter", 1), ("Extra", 2)],
        );
        {
            let mut guard = session.lock().unwrap();
            guard.host = Some(RuntimeHost::new(base).unwrap());
        }
        let staged = {
            let mut guard = session.lock().unwrap();
            let mut bytes = Vec::new();
            schema_edit.write_to(&mut bytes).unwrap();
            execute(
                Command::AcceptEdits { program: bytes },
                guard.host.as_mut().unwrap(),
            )
        };
        assert!(matches!(staged, Response::Ack));
        let tested = build_test_response(&session);
        assert!(
            tested.ok,
            "v_code: {:?} message: {:?}",
            tested.v_code, tested.message
        );
        assert!(tested.status.unwrap().migration);

        let resp = build_untest_response(&session);

        assert!(!resp.ok);
        assert_eq!(resp.v_code.as_deref(), Some("V4011"));
        assert!(resp.message.unwrap().contains("schema"));
        // The refusal leaves the candidate active.
        let status = build_status_response(&session);
        let status = status.status.unwrap();
        assert_eq!(status.mode, HostMode::Testing);
        assert_eq!(status.candidate, Some(2));
    }

    #[test]
    fn build_assemble_response_when_candidate_staged_then_candidate_becomes_normal() {
        let (cache, session) = make_state();
        session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");

        let resp = build_assemble_response(&session);

        assert!(resp.ok);
        let status = resp.status.unwrap();
        assert_eq!(status.mode, HostMode::Normal);
        assert_eq!(status.normal, 2);
        assert_eq!(status.application, 2);
        assert_eq!(status.candidate, None);
        // The promoted candidate is what runs now (0 -> 10 in the driven round).
        let guard = session.lock().unwrap();
        let container = compile_container(&counter_variant("Counter := Counter + 10;"));
        let index = variable_index(&container, "Counter");
        assert_eq!(
            guard.host.as_ref().unwrap().read_variable(index).unwrap(),
            10
        );
    }

    #[test]
    fn build_assemble_response_when_no_candidate_staged_then_v4012() {
        let (cache, session) = make_state();
        establish_counter_session(&cache, &session);

        let resp = build_assemble_response(&session);

        assert!(!resp.ok);
        assert_eq!(resp.v_code.as_deref(), Some("V4012"));
    }

    #[test]
    fn build_cancel_response_when_candidate_staged_then_discards_candidate() {
        let (cache, session) = make_state();
        session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");

        let resp = build_cancel_response(&session);

        assert!(resp.ok);
        let status = resp.status.unwrap();
        assert_eq!(status.mode, HostMode::Normal);
        assert_eq!(status.normal, 1);
        assert_eq!(status.candidate, None);
    }

    #[test]
    fn build_cancel_response_while_testing_then_v4015() {
        let (cache, session) = make_state();
        session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");
        let tested = build_test_response(&session);
        assert!(tested.ok);

        let resp = build_cancel_response(&session);

        assert!(!resp.ok);
        assert_eq!(resp.v_code.as_deref(), Some("V4015"));
    }

    #[test]
    fn build_test_response_when_candidate_traps_on_first_scan_then_trap_v_code() {
        let (cache, session) = make_state();
        // `1 / Counter` divides by zero on the first candidate scan. The
        // driven boundary round traps, and the trap's own V-code comes back.
        session_with_staged_candidate(&cache, &session, "Counter := 1 / Counter;");

        let resp = build_test_response(&session);

        assert!(!resp.ok);
        assert_eq!(resp.v_code.as_deref(), Some("V4001"));
        assert!(resp.message.unwrap().contains("divide"));
    }

    #[test]
    fn build_flow_accept_test_assemble_then_candidate_is_the_running_application() {
        let (cache, session) = make_state();

        let established =
            build_accept_response(&source(COUNTER_PROGRAM), &ed2_options(), &cache, &session);
        assert_eq!(established.result.as_deref(), Some("established"));
        let edit = counter_variant("Counter := Counter + 10;");
        let staged = build_accept_response(&source(&edit), &ed2_options(), &cache, &session);
        assert!(staged.ok);
        let tested = build_test_response(&session);
        assert!(tested.ok);
        let assembled = build_assemble_response(&session);

        assert!(assembled.ok);
        let status = assembled.status.unwrap();
        assert_eq!(status.mode, HostMode::Normal);
        assert_eq!(status.normal, 2);
        assert_eq!(status.application, 2);
        assert_eq!(status.candidate, None);
    }

    #[test]
    fn dispatch_when_accept_payload_is_not_a_container_then_v4016() {
        // The accept tool's compile path can never produce invalid bytes, so
        // the command layer's invalid-payload refusal is reached through
        // `dispatch` directly — the same call `build_accept_response` makes
        // for a staged accept.
        let (_cache, session) = make_state();
        {
            let mut guard = session.lock().unwrap();
            guard.host = Some(RuntimeHost::new(compile_container(COUNTER_PROGRAM)).unwrap());
        }

        let mut guard = session.lock().unwrap();
        let resp = dispatch(
            Command::AcceptEdits {
                program: vec![1, 2, 3],
            },
            &mut guard,
            false,
        );

        assert!(!resp.ok);
        assert_eq!(resp.v_code.as_deref(), Some("V4016"));
    }
}
