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

use std::collections::BTreeMap;
use std::io::Cursor;
use std::sync::Mutex;

use ironplc_container::Container;
use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_problems::{text, Text};
use ironplc_runtime::{
    execute, Command, CommandError, DeviceIdentity, MigrationDecisionSpec, Response, RuntimeError,
    RuntimeHost, StatusPayload, TypeChangeDetail,
};
use schemars::JsonSchema;
use serde::{Deserialize, Serialize};

use super::common::{serialize_diagnostic, serialize_diagnostics, SourceInput};
use crate::cache::ContainerCache;
use crate::tools::device::{DeviceInput, ResolvedDevice};

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

/// Combined input accepted by `hot_edit_accept`: the shared sources/options
/// pair every analysis tool takes, plus optional per-UID migration decisions
/// (ADR 0061).
#[derive(Debug, Deserialize, JsonSchema)]
pub struct HotEditAcceptInput {
    /// One or more source files.
    pub sources: Vec<SourceInput>,
    /// Compiler options (dialect + optional feature-flag overrides).
    #[schemars(schema_with = "super::common::options_schema")]
    pub options: serde_json::Value,
    /// Engineer decisions for out-of-policy type changes, keyed by stable
    /// variable UID: `"init"` (the default when omitted) discards the old
    /// value, `"preserve"` keeps the old storage bytes under the candidate
    /// type (legal only when both sides' sizes match). An empty map fails
    /// closed with a V4010 naming every problematic pair.
    #[serde(default)]
    #[schemars(schema_with = "migration_schema")]
    pub migration: BTreeMap<u64, MigrationDecisionSpec>,
    /// The device to stage the candidate on (see [`DeviceInput`]). Without it
    /// the candidate goes to the server's own session.
    #[serde(default)]
    pub device: Option<DeviceInput>,
}

/// Input of the `hot_edit_*` tools that take no sources: where to act.
#[derive(Debug, Default, Deserialize, JsonSchema)]
pub struct HotEditDeviceInput {
    /// The device to act on (see [`DeviceInput`]). Without it the tool acts on
    /// the server's own session.
    #[serde(default)]
    pub device: Option<DeviceInput>,
}

/// schemars schema function for the UID-keyed `migration` map.
///
/// [`MigrationDecisionSpec`] comes from the runtime crate and does not
/// implement `JsonSchema`; this explicit object schema also keeps the field
/// from rendering as a boolean schema, which some MCP clients reject. The
/// `additionalProperties` enum names the two decision values so clients can
/// discover them from the tool schema.
pub fn migration_schema(_generator: &mut schemars::SchemaGenerator) -> schemars::Schema {
    schemars::json_schema!({
        "type": "object",
        "description": "Engineer decisions for out-of-policy type changes (ADR 0061): stable variable UID -> \"init\" | \"preserve\".",
        "additionalProperties": { "type": "string", "enum": ["init", "preserve"] }
    })
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
    /// missing session, a candidate the workstation did not send because the
    /// stable variable IDs are out of sync, an internal invariant).
    pub v_code: Option<String>,
    /// The failure message accompanying `v_code`.
    pub message: Option<String>,
    /// The V4010 type changes to decide, one entry per problematic entity
    /// (ADR 0061). Null on success and on every other refusal.
    pub pairs: Option<Vec<TypeChangeDetail>>,
    /// The device that answered, when the tool acted on a device (the `device`
    /// parameter): its id, name, model, modification and runtime version, as its
    /// `identity` reported them. Absent for the in-process host. Compare the id
    /// with the one asked for: the tool does not refuse a device for being
    /// another one.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub device: Option<DeviceIdentity>,
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
            pairs: None,
            device: None,
            diagnostics: vec![],
        }
    }

    /// Builds the failure response that carries diagnostics alone.
    fn with_diagnostics(diagnostics: Vec<serde_json::Value>) -> Self {
        HotEditResponse {
            ok: false,
            result: None,
            status: None,
            v_code: None,
            message: None,
            pairs: None,
            device: None,
            diagnostics,
        }
    }

    /// Builds the failure response for a candidate the workstation did not
    /// send: the command layer never saw it, so there is no V-code, and the
    /// message says what to do.
    fn not_sent(message: String) -> Self {
        HotEditResponse {
            ok: false,
            result: None,
            status: None,
            v_code: None,
            message: Some(message),
            pairs: None,
            device: None,
            diagnostics: vec![],
        }
    }

    /// Builds the failure response for a command-layer refusal. The V-code
    /// and message are the command layer's whole contract (ADR-0055), so the
    /// tool forwards them verbatim and adds nothing of its own; a V4010
    /// refusal additionally surfaces its pair list for the client to decide.
    fn command_error(error: &CommandError) -> Self {
        HotEditResponse {
            ok: false,
            result: None,
            status: None,
            v_code: Some(error.v_code().to_string()),
            message: Some(error.message().to_string()),
            pairs: error.pairs().map(<[TypeChangeDetail]>::to_vec),
            device: None,
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
                pairs: None,
                device: None,
                diagnostics: vec![],
            },
            RuntimeError::Internal { reason } => {
                internal_failure(format!("runtime host invariant violated: {reason}"))
            }
            RuntimeError::NotPermitted => {
                // The session grants at establish, so a refusal here means
                // a broken composition, not the user's program: surface the
                // stable code and the host's message.
                let error = RuntimeError::NotPermitted;
                HotEditResponse {
                    ok: false,
                    result: None,
                    status: None,
                    v_code: error.v_code().map(str::to_string),
                    message: Some(error.to_string()),
                    pairs: None,
                    device: None,
                    diagnostics: vec![],
                }
            }
        }
    }
}

/// Builds the failure response for a P8001 input/session validation error,
/// the same convention the `run` tool uses for unknown container handles.
fn validation_failure(text: Text) -> HotEditResponse {
    HotEditResponse::with_diagnostics(serialize_diagnostics(
        &[Diagnostic::text(SourceSpan::default(), text)],
        &[],
    ))
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
        pairs: None,
        device: None,
        diagnostics: vec![serialize_diagnostic(
            &Diagnostic::internal_error_at(Label::span(SourceSpan::default(), message)),
            &[],
        )],
    }
}

/// Where a hot-edit command is carried out: the server's own host, or a device
/// reached through the session protocol. The tools build the same [`Command`]
/// and the same response for both.
#[derive(Clone, Copy)]
pub enum Target<'a> {
    /// The in-process host of the server's single session.
    InProcess(&'a Mutex<HotEditSession>),
    /// A device, connected to for the command.
    Device(&'a ResolvedDevice),
}

impl<'a> From<&'a Mutex<HotEditSession>> for Target<'a> {
    fn from(session: &'a Mutex<HotEditSession>) -> Self {
        Target::InProcess(session)
    }
}

impl<'a> From<&'a ResolvedDevice> for Target<'a> {
    fn from(device: &'a ResolvedDevice) -> Self {
        Target::Device(device)
    }
}

/// Carries out one command on the target. `drive_scan` is for the in-process
/// host (see [`dispatch`]); a device drives its own scan round.
fn run(command: Command, target: Target<'_>, drive_scan: bool) -> HotEditResponse {
    match target {
        Target::InProcess(session) => {
            let mut guard = session.lock().unwrap_or_else(|e| e.into_inner());
            dispatch(command, &mut guard, drive_scan)
        }
        Target::Device(device) => device::exchange(device, command),
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
        return validation_failure(text::mcp_input_validation_hot_edit_session_missing());
    };

    match execute(command, host, &mcp_device_identity()) {
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
        // Unreachable from the MCP tools (they never send `identity`, the
        // engineering-connection handshake of ADR-0063): the session has no
        // device panel to answer with.
        Response::Identity(_) => {
            internal_failure("identity is not available on the MCP hot edit session".to_string())
        }
    }
}

/// The device panel the MCP server answers protocol handshakes with: the MCP
/// host is a soft device of its own, so it reports its own binary identity
/// the way `vm-cli` does.
fn mcp_device_identity() -> DeviceIdentity {
    DeviceIdentity {
        device_id: None,
        name: "ironplcmcp".into(),
        model: "IronPLC SoftPLC".into(),
        modification: "mcp".into(),
        runtime_version: env!("CARGO_PKG_VERSION").to_string(),
    }
}

/// Builds the `hot_edit_status` response: the session's status payload.
pub fn build_status_response<'a>(target: impl Into<Target<'a>>) -> HotEditResponse {
    run(Command::GetStatus, target.into(), false)
}

/// Builds the `hot_edit_accept` response without migration decisions.
///
/// Equivalent to
/// [`build_accept_response_with_decisions`](Self::build_accept_response_with_decisions)
/// with an empty decision map: an out-of-policy type change is refused with a
/// V4010 that names every problematic pair.
pub fn build_accept_response<'a>(
    sources: &[SourceInput],
    options_value: &serde_json::Value,
    cache: &Mutex<ContainerCache>,
    target: impl Into<Target<'a>>,
) -> HotEditResponse {
    build_accept_response_with_decisions(sources, options_value, &BTreeMap::new(), cache, target)
}

/// Builds the `hot_edit_accept` response, resolving out-of-policy type
/// changes with `migration` (ADR 0061).
///
/// Compiles `sources` through the same pipeline as the `compile` tool, then:
///
/// - no session yet: the compiled container becomes the running application
///   (the session is established and the result is `"established"`); or
/// - session running: the compiled container is staged through the command
///   layer's [`Command::AcceptEdits`], so every refusal (V4007 layout, V4008
///   schedule, V4010 type changes, V4013 already staged, ...) comes back with
///   its stable V-code. A V4010 refusal carries its pair list, and the caller
///   resubmits the identical sources with the decisions map.
pub fn build_accept_response_with_decisions<'a>(
    sources: &[SourceInput],
    options_value: &serde_json::Value,
    migration: &BTreeMap<u64, MigrationDecisionSpec>,
    cache: &Mutex<ContainerCache>,
    target: impl Into<Target<'a>>,
) -> HotEditResponse {
    match prepare_candidate(sources, options_value, cache) {
        Ok(candidate) => send_candidate(
            candidate.program,
            candidate.ids_out_of_sync,
            migration,
            target,
        ),
        Err(response) => response,
    }
}

/// A compiled program ready to be sent as an edit candidate.
pub struct Candidate {
    /// The compiled container in its wire format.
    pub program: Vec<u8>,
    /// The workstation's verdict on the stable variable IDs, when they are out
    /// of sync with the declarations (see [`send_candidate`]).
    pub ids_out_of_sync: Option<String>,
}

/// Compiles `sources` for `hot_edit_accept`. Compiling is separate from sending
/// because it runs on the thread that serves the request, on the stack budget,
/// while sending to a device waits on the network.
pub fn prepare_candidate(
    sources: &[SourceInput],
    options_value: &serde_json::Value,
    cache: &Mutex<ContainerCache>,
) -> Result<Candidate, HotEditResponse> {
    // The compile tool owns the pipeline; calling it keeps `hot_edit_accept`
    // on exactly the compile path the `compile` tool serves, diagnostics and
    // container cache included, so the two can never drift apart.
    let compiled = crate::tools::compile::build_response(sources, options_value, false, cache);
    if !compiled.ok {
        return Err(HotEditResponse::with_diagnostics(compiled.diagnostics));
    }
    let Some(container_id) = compiled.container_id else {
        return Err(internal_failure(
            "compile succeeded without a container id".to_string(),
        ));
    };
    let cached = {
        let mut guard = cache.lock().unwrap_or_else(|e| e.into_inner());
        guard
            .get(&container_id)
            .map(|cached| (cached.iplc_bytes.clone(), cached.ids_out_of_sync.clone()))
    };
    let Some((program, ids_out_of_sync)) = cached else {
        return Err(internal_failure(format!(
            "compiled container '{container_id}' was evicted before it could be staged"
        )));
    };
    Ok(Candidate {
        program,
        ids_out_of_sync,
    })
}

/// Sends a compiled candidate to the session, unless the workstation knows the
/// controller would refuse it (ADR-0074).
///
/// `ids_out_of_sync` is the verdict of the project's comparison of its stable
/// variable IDs with its declared persistent variables, rendered for the
/// engineer. While it is set the candidate is not sent: the controller refuses a
/// candidate that goes through a migration while a persistent variable has no ID
/// (V4020), and says so after the candidate has been transferred.
pub fn send_candidate<'a>(
    program: Vec<u8>,
    ids_out_of_sync: Option<String>,
    migration: &BTreeMap<u64, MigrationDecisionSpec>,
    target: impl Into<Target<'a>>,
) -> HotEditResponse {
    if let Some(difference) = ids_out_of_sync {
        return HotEditResponse::not_sent(difference);
    }

    let accept = |program: Vec<u8>| Command::AcceptEdits {
        program,
        edit: None,
        migration: migration.clone(),
    };
    match target.into() {
        Target::InProcess(session) => {
            let mut guard = session.lock().unwrap_or_else(|e| e.into_inner());
            if guard.host.is_none() {
                // The first accept has nothing to stage against: it establishes.
                return establish(program, &mut guard);
            }
            dispatch(accept(program), &mut guard, false)
        }
        // A device runs a program already: the candidate is staged against it.
        Target::Device(device) => device::exchange(device, accept(program)),
    }
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
        Ok(mut host) => {
            // Standalone RAM-only session: the MCP server is the
            // composition root and grants the execution permit at startup
            // (the HA redundancy architecture, "Minimal Seams" 1).
            host.permit_execution();
            host
        }
        Err(error) => return HotEditResponse::runtime_error(error),
    };
    let status = StatusPayload::from(host.status());
    session.host = Some(host);
    HotEditResponse::ok("established", status)
}

/// Builds the `hot_edit_test` response: activates the staged candidate at the
/// next scan boundary and drives one scan round so the candidate is
/// executing when the call returns.
pub fn build_test_response<'a>(target: impl Into<Target<'a>>) -> HotEditResponse {
    run(Command::TestEdits, target.into(), true)
}

/// Builds the `hot_edit_untest` response: reverts to the original artifact at
/// the next scan boundary (process state is preserved) and drives one scan
/// round so the revert is executing when the call returns.
pub fn build_untest_response<'a>(target: impl Into<Target<'a>>) -> HotEditResponse {
    run(Command::UntestEdits, target.into(), true)
}

/// Builds the `hot_edit_assemble` response: promotes the staged candidate to
/// the running application and drives one scan round. Allowed from both the
/// staged and the testing phase.
pub fn build_assemble_response<'a>(target: impl Into<Target<'a>>) -> HotEditResponse {
    run(Command::AssembleEdits, target.into(), true)
}

/// Builds the `hot_edit_cancel` response: discards the staged candidate while
/// the original artifact keeps running. Takes effect immediately, so no scan
/// round is driven.
pub fn build_cancel_response<'a>(target: impl Into<Target<'a>>) -> HotEditResponse {
    run(Command::CancelEdits, target.into(), false)
}

mod device;
pub use device::unresolved;

#[cfg(test)]
mod tests;
