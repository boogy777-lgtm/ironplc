//! The `ironplcvm serve` command: a newline-delimited JSON command session
//! over the runtime host's hot-edit protocol.
//!
//! [`serve`] loads and starts a compiled program through the same container
//! path as `run` (see [`crate::cli::load_container`]), then answers one line
//! of JSON per line of stdin until EOF. Stdout is the protocol channel:
//! startup prints nothing, every response line is flushed as written, and
//! anything diagnostic goes to stderr through the logger. The TCP transport
//! of the engineering connection (ADR-0063) is the same session loop over a
//! length-prefixed frame — see [`crate::tcp`].
//!
//! The FSM-advancing commands (`testEdits`, `untestEdits`, `assembleEdits`)
//! only *request* a swap; the host applies it at the next scan boundary
//! ([`RuntimeHost::run`]). After one of them is acknowledged, the session
//! drives one scan round at a constant zero uptime — the convention of the
//! runtime acceptance tests — so a scripted client reads back state that has
//! actually switched. A trap in the driven round is the running
//! application's own fault: it is logged to stderr with the trap's V-code
//! and the session continues.
//!
//! `assembleEdits` is also the single commit point (ADR-0064 amendment): the
//! session persists the host's committed wire bytes through the A/B
//! [`SlotStore`] after the driven boundary round, before the response line
//! is rendered. A persistence failure answers the wire with V6012 instead of
//! the ack — the RAM promotion stands and the client learns the commit is
//! live but not durable. Boot adopts the newest verifiable generation from
//! the store, so a reboot after an acknowledged assemble boots the committed
//! artifact.

use std::io::{self, BufRead, Write};
use std::path::Path;

use ironplc_container::Container;
use ironplc_runtime::{
    execute, parse_command, render_response, Command, DeviceIdentity, Response, RuntimeError,
    RuntimeHost,
};

use crate::error::{self, VmError};
use crate::slot_store::SlotStore;

/// The device panel `vm-cli` answers the `identity` handshake with
/// (ADR-0063): the served process is a soft device, so it reports its binary
/// name and its own version — the one composition point for the
/// application-supplied block the runtime host does not own.
pub(crate) fn device_identity() -> DeviceIdentity {
    DeviceIdentity {
        name: "ironplcvm".into(),
        model: "IronPLC SoftPLC".into(),
        modification: "vm-cli".into(),
        firmware_version: env!("CARGO_PKG_VERSION").to_string(),
    }
}

/// Boots the committed artifact from the A/B slot store beside `path`
/// (seeding the store from the file on first serve), starts it on the
/// runtime host, and serves commands until stdin reaches EOF.
pub fn serve(path: &Path) -> Result<(), VmError> {
    let mut store = SlotStore::beside(path);
    let container = store.boot()?;
    let mut host = start_host(container)?;

    let stdin = io::stdin();
    let stdout = io::stdout();
    let device = device_identity();
    serve_session(
        &mut host,
        stdin.lock(),
        stdout.lock(),
        Some(&mut store),
        &device,
    )
    .map_err(|err| {
        VmError::io(
            error::SESSION_IO,
            format!("unable to read or write the command session: {err}"),
        )
    })
}

/// Creates the runtime host for `container`, mapping init traps to the trap's
/// V-code exactly like `run` does.
///
/// The host boots without the execution permit (the HA redundancy
/// architecture, "Minimal Seams" 1); `serve` is a standalone shell, so it
/// grants the permit immediately — today's standalone behavior, the trivial
/// grant policy.
pub(crate) fn start_host(container: Container) -> Result<RuntimeHost, VmError> {
    let mut host = RuntimeHost::new(container).map_err(|err| match err {
        RuntimeError::Trap(context) => {
            VmError::from_trap(&context.trap, context.task_id, context.instance_id)
        }
        RuntimeError::NotPermitted => VmError::io(
            error::SESSION_IO,
            "runtime host refused to start: no execution permit".to_string(),
        ),
        RuntimeError::Internal { reason } => VmError::io(
            error::SESSION_IO,
            format!("runtime host failed to start: {reason}"),
        ),
    })?;
    host.permit_execution();
    Ok(host)
}

/// Serves one command session: reads one line per command from `reader`,
/// writes one line per response to `writer`, flushing after every line.
///
/// A command that advances the hot-edit FSM (`testEdits`, `untestEdits`,
/// `assembleEdits`) drives one scan round after its acknowledgment, before
/// the response line is written (see [`drive_scan_round`]). An acknowledged
/// `assembleEdits` with a composed `store` also persists the committed wire
/// bytes before the response line is written (see [`persist_commit`]); a
/// `None` store serves the honestly RAM-only session (no persistence).
/// `device` is the application-composed device panel every command mapping
/// carries (ADR-0063): it answers the `identity` handshake, the first command
/// on every transport.
///
/// Returns when the reader reaches EOF, or with the first I/O failure.
pub fn serve_session(
    host: &mut RuntimeHost,
    reader: impl BufRead,
    mut writer: impl Write,
    mut store: Option<&mut SlotStore>,
    device: &DeviceIdentity,
) -> io::Result<()> {
    for line in reader.lines() {
        let line = line?;
        let response = match parse_command(&line) {
            Ok(command) => {
                let commits = matches!(command, Command::AssembleEdits);
                let advances = advances_state(&command);
                let response = execute(command, host, device);
                if advances && matches!(response, Response::Ack) {
                    if let Some(err) = drive_scan_round(host) {
                        log::error!("driven scan round trapped: {err}");
                    }
                }
                if commits && matches!(response, Response::Ack) {
                    // ADR-0064 amendment: persist before the line renders, so
                    // a failure answers V6012 instead of an ack.
                    match store.as_deref_mut() {
                        Some(store) => match persist_commit(host, store) {
                            Ok(()) => render_line(&response)?,
                            Err(err) => persist_error_line(&err),
                        },
                        None => render_line(&response)?,
                    }
                } else {
                    render_line(&response)?
                }
            }
            Err(err) => {
                // A malformed line is a codec error, not an online change
                // refusal: it has no V-code (ADR-0055). The transport answers
                // with one error line so the wire stays aligned — one line
                // in, one line out — and the session continues.
                log::warn!("ignoring malformed command line: {err}");
                codec_error_line(&err)
            }
        };
        writeln!(writer, "{response}")?;
        writer.flush()?;
    }
    Ok(())
}

/// Renders one typed response as the session's single output line.
fn render_line(response: &Response) -> io::Result<String> {
    render_response(response).map_err(|err| io::Error::new(io::ErrorKind::InvalidData, err))
}

/// Persists the just-committed wire bytes through the slot store: drains the
/// host's committed latch (set only by `assemble`) and commits the bytes.
/// An empty drain is an internal error, never a silent skip — through
/// `serve` every accepted candidate carries its wire bytes, so an assemble
/// acknowledgment always has bytes to commit. The error surfaces as V6012 on
/// the wire.
fn persist_commit(host: &mut RuntimeHost, store: &mut SlotStore) -> Result<(), VmError> {
    let wire = host.take_committed_wire().ok_or_else(|| {
        log::error!("assemble acknowledged but the host holds no committed wire bytes");
        VmError::io(
            error::SLOT_COMMIT_PERSIST,
            "unable to persist the assembled commit to the slot store: \
             assemble committed no wire bytes to persist"
                .to_string(),
        )
    })?;
    store.commit(&wire)
}

/// Renders the V6012 answer that replaces the assemble acknowledgment when
/// the commit could not be persisted (ADR-0064 amendment). Built directly:
/// V6012 is a shell code, not one of the runtime command layer's V40xx
/// codes, so the typed `Response` cannot carry it.
fn persist_error_line(err: &VmError) -> String {
    serde_json::json!({
        "response": "error",
        "vCode": error::SLOT_COMMIT_PERSIST,
        "message": err.to_string(),
    })
    .to_string()
}

/// Whether acknowledging `command` advances the hot-edit FSM at a scan
/// boundary. `testEdits`/`untestEdits`/`assembleEdits` only *request* a swap;
/// `acceptEdits` and `cancelEdits` take effect without one.
fn advances_state(command: &Command) -> bool {
    matches!(
        command,
        Command::TestEdits | Command::UntestEdits | Command::AssembleEdits
    )
}

/// Drives the one scan round that applies a swap acknowledged on the wire,
/// at the constant-zero clock of the runtime acceptance tests (every task is
/// ready immediately after a (re)load, so the boundary round runs each task
/// exactly once).
///
/// Returns the trap as a [`VmError`] — its V-code is the trap's own — so the
/// caller can surface it; a trapped round does not end the session. A
/// violated host invariant keeps no V-code of its own (ADR-0055): it is
/// logged here and reported as `None`.
fn drive_scan_round(host: &mut RuntimeHost) -> Option<VmError> {
    match host.run_with_commit(
        1,
        || 0,
        |commit| {
            // The standalone shell's scan-commit composition (the HA redundancy
            // architecture, "Minimal Seams" 2): observe the boundary identity;
            // a redundancy shell mints the epoch here instead.
            log::debug!("scan boundary committed: {commit:?}");
        },
    ) {
        Ok(()) => None,
        Err(RuntimeError::Trap(context)) => Some(VmError::from_trap(
            &context.trap,
            context.task_id,
            context.instance_id,
        )),
        Err(RuntimeError::NotPermitted) => {
            // `serve` grants at startup, so a refusal here means the
            // composition is broken, not the user's program: log it like an
            // invariant violation; the acknowledgment already stands.
            log::error!("driven scan round refused: the host holds no execution permit");
            None
        }
        Err(RuntimeError::Internal { reason }) => {
            log::error!("runtime host invariant violated during a driven scan round: {reason}");
            None
        }
    }
}

/// Renders the transport-level error for a line that did not parse as a
/// command. Codec errors carry no V-code, so `vCode` is null.
fn codec_error_line(err: &serde_json::Error) -> String {
    serde_json::json!({
        "response": "error",
        "vCode": null,
        "message": format!("invalid command line: {err}"),
    })
    .to_string()
}

#[cfg(test)]
mod tests {
    use super::*;
    use spec_test_macro::spec_test;

    /// A `PROGRAM main` with one DINT `Counter` incremented by `step` per
    /// scan; same declarations for every step, so two compiles differ only
    /// in scan logic and share a layout.
    fn counter_source(step: i32) -> String {
        format!(
            "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + {step};
END_PROGRAM
"
        )
    }

    /// Compiles `source` through the real pipeline and round-trips the
    /// container through the wire format, mirroring
    /// `tests/cli.rs::write_compiled_container` and the runtime test fixtures,
    /// so `header.layout_hash` is the value a deployed artifact carries.
    fn compile_container(source: &str) -> Container {
        let options = ironplc_parser::options::CompilerOptions::default();
        let library =
            ironplc_parser::parse_program(source, &ironplc_dsl::core::FileId::default(), &options)
                .unwrap();
        let (analyzed, context) =
            ironplc_analyzer::stages::resolve_types(&[&library], &options).unwrap();
        let container = ironplc_codegen::compile(
            &analyzed,
            &context,
            &ironplc_codegen::CodegenOptions::default(),
            &ironplc_codegen::EmptyLookup,
        )
        .unwrap();

        let mut bytes = Vec::new();
        container.write_to(&mut bytes).unwrap();
        Container::read_from(&mut io::Cursor::new(&bytes)).unwrap()
    }

    /// The wire-format bytes an `acceptEdits` command carries for `source`.
    fn container_bytes(source: &str) -> Vec<u8> {
        let mut bytes = Vec::new();
        compile_container(source).write_to(&mut bytes).unwrap();
        bytes
    }

    /// [`compile_container`] for a source compiled with engineering-side
    /// stable variable IDs (ADR 0053), so a declaration edit stages as a
    /// migration candidate.
    fn compile_container_with_ids(source: &str, ids: &[(&str, u64)]) -> Container {
        let options = ironplc_parser::options::CompilerOptions::default();
        let library =
            ironplc_parser::parse_program(source, &ironplc_dsl::core::FileId::default(), &options)
                .unwrap();
        let (analyzed, context) =
            ironplc_analyzer::stages::resolve_types(&[&library], &options).unwrap();
        let codegen_options = ironplc_codegen::CodegenOptions {
            stable_var_ids: ids
                .iter()
                .map(|(name, uid)| (ironplc_dsl::core::Id::from(name), *uid))
                .collect(),
            ..ironplc_codegen::CodegenOptions::default()
        };
        let container = ironplc_codegen::compile(
            &analyzed,
            &context,
            &codegen_options,
            &ironplc_codegen::EmptyLookup,
        )
        .unwrap();

        let mut bytes = Vec::new();
        container.write_to(&mut bytes).unwrap();
        Container::read_from(&mut io::Cursor::new(&bytes)).unwrap()
    }

    /// The wire-format bytes an `acceptEdits` command carries for `source`
    /// compiled with stable variable IDs.
    fn container_bytes_with_ids(source: &str, ids: &[(&str, u64)]) -> Vec<u8> {
        let mut bytes = Vec::new();
        compile_container_with_ids(source, ids)
            .write_to(&mut bytes)
            .unwrap();
        bytes
    }

    /// A `PROGRAM main` whose single `Counter` carries `ty`, incremented by
    /// one per scan (the retype target of the migration wire tests).
    fn retyped_counter_source(ty: &str) -> String {
        format!(
            "PROGRAM main
  VAR
    Counter : {ty};
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
"
        )
    }

    /// The wire-format bytes of a candidate whose scan divides by the
    /// zero-initialized `Counter`, so the first driven round traps.
    fn trapping_container_bytes() -> Vec<u8> {
        container_bytes(
            "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := 1 / Counter;
END_PROGRAM
",
        )
    }

    /// Runs `lines` through one session without a slot store, returning the
    /// parsed response per input line.
    fn run_session(host: &mut RuntimeHost, lines: &[&str]) -> Vec<serde_json::Value> {
        run_session_with_store(host, lines, None)
    }

    /// Runs `lines` through one session, optionally persisting assembles
    /// through a slot store, returning the parsed response per input line.
    fn run_session_with_store(
        host: &mut RuntimeHost,
        lines: &[&str],
        store: Option<&mut SlotStore>,
    ) -> Vec<serde_json::Value> {
        let mut input = lines.join("\n");
        input.push('\n');
        let mut output = Vec::new();
        serve_session(
            host,
            io::Cursor::new(input.into_bytes()),
            &mut output,
            store,
            &device_identity(),
        )
        .unwrap();
        let text = String::from_utf8(output).unwrap();
        text.lines()
            .map(|line| serde_json::from_str(line).unwrap())
            .collect()
    }

    fn counter_host() -> RuntimeHost {
        // Test hosts model the standalone shell: granted at startup.
        let mut host = RuntimeHost::new(compile_container(&counter_source(1))).unwrap();
        host.permit_execution();
        host
    }

    /// REQ-VC-vm-cli-019: every command line gets exactly one response line,
    /// so a scripted session stays aligned line for line.
    #[spec_test(REQ_VC_vm_cli_019)]
    #[test]
    fn serve_session_when_scripted_sequence_then_one_response_line_per_command() {
        let edit = container_bytes(&counter_source(10));
        let accept = serde_json::json!({"command": "acceptEdits", "program": edit}).to_string();
        let lines = [
            r#"{"command":"getStatus"}"#,
            &accept,
            r#"{"command":"testEdits"}"#,
            r#"{"command":"assembleEdits"}"#,
            r#"{"command":"cancelEdits"}"#,
            r#"{"command":"getStatus"}"#,
        ];
        let mut host = counter_host();

        let responses = run_session(&mut host, &lines);

        assert_eq!(responses.len(), lines.len());
        assert_eq!(responses[0]["response"], "status");
        assert_eq!(responses[0]["mode"], "normal");
        assert_eq!(responses[0]["candidate"], serde_json::Value::Null);
        assert_eq!(responses[1]["response"], "ack");
        // Each FSM-advancing command drives one scan round at the boundary,
        // so the test swap is applied — and the assemble promotion likewise
        // — by the time its acknowledgment is written: the whole scripted
        // path is ack. ADR-0064: assemble follows the test, never Accepted.
        assert_eq!(responses[2]["response"], "ack");
        assert_eq!(responses[3]["response"], "ack");
        // Assembling left nothing staged, so the final cancel is refused
        // with the protocol's usual V-code.
        assert_eq!(responses[4]["response"], "error");
        assert_eq!(responses[4]["vCode"], "V4012");
        // Two rounds were driven (test, assemble): the candidate is the
        // promoted, running application.
        assert_eq!(responses[5]["response"], "status");
        assert_eq!(responses[5]["mode"], "normal");
        assert_eq!(responses[5]["normal"], 2);
        assert_eq!(responses[5]["application"], 2);
        assert_eq!(responses[5]["candidate"], serde_json::Value::Null);
        assert_eq!(responses[5]["rounds"], 2);
    }

    /// The `identity` handshake (ADR-0063) answers over the stdio session too:
    /// the device block is the `vm-cli` composition, the application block is
    /// the same status payload `getStatus` answers with, and no redundancy
    /// block means standalone.
    #[test]
    fn serve_session_when_identity_then_device_panel_and_status_snapshot() {
        let mut host = counter_host();

        let responses = run_session(
            &mut host,
            &[r#"{"command":"identity"}"#, r#"{"command":"getStatus"}"#],
        );

        assert_eq!(responses.len(), 2);
        assert_eq!(responses[0]["response"], "identity");
        assert_eq!(responses[0]["protocol"], 1);
        assert_eq!(responses[0]["device"]["name"], "ironplcvm");
        assert_eq!(responses[0]["device"]["model"], "IronPLC SoftPLC");
        assert_eq!(responses[0]["device"]["modification"], "vm-cli");
        // The firmware version is the binary's own crate version — the one
        // composition point for the device panel (ADR-0063).
        assert_eq!(
            responses[0]["device"]["firmwareVersion"],
            env!("CARGO_PKG_VERSION")
        );
        // The application block is the StatusPayload vocabulary verbatim —
        // the same fields getStatus answers with, minus the response tag.
        let mut status = responses[1].clone();
        status.as_object_mut().unwrap().remove("response");
        assert_eq!(responses[0]["application"], status);
        assert!(responses[0].get("redundancy").is_none());
    }

    /// REQ-VC-vm-cli-023: a trap in a driven round does not change the
    /// protocol answer (the swap was recorded, so the wire carries the ack);
    /// the trap surfaces with its V-code and the session keeps serving.
    #[spec_test(REQ_VC_vm_cli_023)]
    #[test]
    fn serve_session_when_driven_round_traps_then_ack_on_wire_and_session_continues() {
        let accept =
            serde_json::json!({"command": "acceptEdits", "program": trapping_container_bytes()})
                .to_string();
        let lines = [
            &accept,
            r#"{"command":"testEdits"}"#,
            r#"{"command":"getStatus"}"#,
        ];
        let mut host = counter_host();

        let responses = run_session(&mut host, &lines);

        assert_eq!(responses.len(), lines.len());
        assert_eq!(responses[0]["response"], "ack");
        // The driven boundary round trapped (divide by zero, V4001, logged
        // to stderr); the acknowledgment still stands and the session
        // answers the next command.
        assert_eq!(responses[1]["response"], "ack");
        assert_eq!(responses[2]["response"], "status");
    }

    /// A slot store booted in a fresh temp dir beside a written counter
    /// file (the first boot seeds slot A and the marker).
    fn booted_store(dir: &tempfile::TempDir) -> SlotStore {
        let file = dir.path().join("app.iplc");
        std::fs::write(&file, container_bytes(&counter_source(1))).unwrap();
        let store = SlotStore::beside(&file);
        store.boot().unwrap();
        store
    }

    /// The assemble session: accept a candidate, test it, assemble it.
    fn accept_test_assemble_lines(edit: Vec<u8>) -> Vec<String> {
        let accept = serde_json::json!({"command": "acceptEdits", "program": edit}).to_string();
        vec![
            accept,
            r#"{"command":"testEdits"}"#.to_string(),
            r#"{"command":"assembleEdits"}"#.to_string(),
        ]
    }

    #[test]
    fn serve_session_when_assemble_from_accepted_then_v4017_on_wire() {
        let mut host = counter_host();
        let edit = container_bytes(&counter_source(10));
        let accept = serde_json::json!({"command": "acceptEdits", "program": edit}).to_string();
        let lines = [&accept, r#"{"command":"assembleEdits"}"#];

        // Assemble before the candidate ever ran under Test refuses with the
        // ADR-0064 V-code; test+assemble would ack.
        let responses = run_session(&mut host, &lines);

        assert_eq!(responses[0]["response"], "ack");
        assert_eq!(responses[1]["response"], "error");
        assert_eq!(responses[1]["vCode"], "V4017");
        assert!(host.status().candidate.is_some());
    }

    #[test]
    fn serve_session_when_assemble_with_store_then_committed_bytes_land_in_inactive_slot() {
        let dir = tempfile::TempDir::new().unwrap();
        let mut store = booted_store(&dir);
        let edit = container_bytes(&counter_source(10));
        let lines = accept_test_assemble_lines(edit.clone());
        let lines: Vec<&str> = lines.iter().map(String::as_str).collect();
        let mut host = counter_host();

        let responses = run_session_with_store(&mut host, &lines, Some(&mut store));

        assert_eq!(responses[2]["response"], "ack");
        // Slot contents are exactly the accepted wire bytes — never a
        // re-serialization (ADR-0064 amendment).
        let slot_b = std::fs::read(dir.path().join("app.iplc.slot-b")).unwrap();
        assert_eq!(slot_b, edit);
        // The marker flipped to the inactive slot at seq 2.
        let marker = std::fs::read_to_string(dir.path().join("app.iplc.marker")).unwrap();
        let marker: serde_json::Value = serde_json::from_str(&marker).unwrap();
        assert_eq!(marker["slot"], "b");
        assert_eq!(marker["seq"], 2);
    }

    #[test]
    fn serve_session_when_persist_fails_then_v6012_on_wire_and_ram_promotion_stands() {
        let dir = tempfile::TempDir::new().unwrap();
        let mut store = booted_store(&dir);
        // Block the commit's tmp write: a directory where the tmp file lands.
        std::fs::create_dir(dir.path().join("app.iplc.tmp")).unwrap();
        let lines = accept_test_assemble_lines(container_bytes(&counter_source(10)));
        let lines: Vec<&str> = lines.iter().map(String::as_str).collect();
        let mut host = counter_host();

        let responses = run_session_with_store(&mut host, &lines, Some(&mut store));

        // The acknowledgment became a V6012 error line (ADR-0064 amendment);
        // the host promotion still stands, so a reboot would boot the
        // previous committed generation.
        assert_eq!(responses[2]["response"], "error");
        assert_eq!(responses[2]["vCode"], "V6012");
        assert_eq!(host.status().normal.raw(), 2);
        let marker = std::fs::read_to_string(dir.path().join("app.iplc.marker")).unwrap();
        assert!(marker.contains("\"seq\":1"));
    }

    #[test]
    fn serve_session_when_type_change_without_decision_then_v4010_with_pairs() {
        let base = compile_container_with_ids(&counter_source(1), &[("Counter", 1)]);
        let mut host = RuntimeHost::new(base).unwrap();
        host.permit_execution();
        host.run(3, || 0).unwrap();

        let candidate =
            container_bytes_with_ids(&retyped_counter_source("UDINT"), &[("Counter", 1)]);
        let accept =
            serde_json::json!({"command": "acceptEdits", "program": candidate}).to_string();

        let responses = run_session(&mut host, &[&accept]);

        assert_eq!(responses.len(), 1);
        assert_eq!(responses[0]["response"], "error");
        assert_eq!(responses[0]["vCode"], "V4010");
        assert_eq!(
            responses[0]["pairs"],
            serde_json::json!([{
                "uid": 1,
                "name": "Counter",
                "from": "I32",
                "to": "U32",
                "sizeEqual": true,
            }])
        );
        // The refusal staged nothing; the engineer can resubmit the same
        // container bytes with a decision.
        assert_eq!(host.status().candidate, None);
    }

    #[test]
    fn serve_session_when_preserve_decision_then_old_bits_survive_the_retype() {
        let base = compile_container_with_ids(&counter_source(1), &[("Counter", 1)]);
        let mut host = RuntimeHost::new(base).unwrap();
        host.permit_execution();
        host.run(3, || 0).unwrap();

        let candidate =
            container_bytes_with_ids(&retyped_counter_source("UDINT"), &[("Counter", 1)]);
        let accept = serde_json::json!({
            "command": "acceptEdits",
            "program": candidate,
            "migration": {"1": "preserve"},
        })
        .to_string();
        let lines = [&accept, r#"{"command":"testEdits"}"#];

        let responses = run_session(&mut host, &lines);

        assert_eq!(responses[0]["response"], "ack");
        assert!(host.status().migration);
        // The testEdits acknowledgment drove one boundary round: the DINT 3
        // (0x3) was preserved bit for bit and the UDINT body added one.
        assert_eq!(responses[1]["response"], "ack");
        assert_eq!(
            host.read_variable(ironplc_container::VarIndex::new(0))
                .unwrap(),
            4
        );
    }

    #[test]
    fn serve_session_when_init_decision_then_candidate_initial_value_stands() {
        let base = compile_container_with_ids(&counter_source(1), &[("Counter", 1)]);
        let mut host = RuntimeHost::new(base).unwrap();
        host.permit_execution();
        host.run(3, || 0).unwrap();

        let source = "PROGRAM main
  VAR
    Counter : UDINT := 100;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
";
        let candidate = container_bytes_with_ids(source, &[("Counter", 1)]);
        let accept = serde_json::json!({
            "command": "acceptEdits",
            "program": candidate,
            "migration": {"1": "init"},
        })
        .to_string();
        let lines = [&accept, r#"{"command":"testEdits"}"#];

        let responses = run_session(&mut host, &lines);

        assert_eq!(responses[0]["response"], "ack");
        // The old 3 was discarded; the candidate's declared 100 initialized
        // the variable before the candidate body ran.
        assert_eq!(responses[1]["response"], "ack");
        assert_eq!(
            host.read_variable(ironplc_container::VarIndex::new(0))
                .unwrap(),
            101
        );
    }

    #[test]
    fn drive_scan_round_when_round_traps_then_trap_v_code() {
        let mut host = counter_host();
        let device = device_identity();
        let accept = parse_command(
            &serde_json::json!({"command": "acceptEdits", "program": trapping_container_bytes()})
                .to_string(),
        )
        .unwrap();
        assert!(matches!(execute(accept, &mut host, &device), Response::Ack));
        assert!(matches!(
            execute(Command::TestEdits, &mut host, &device),
            Response::Ack
        ));

        let err = drive_scan_round(&mut host).unwrap();

        assert!(err
            .to_string()
            .starts_with("V4001 - runtime error: divide by zero"));
    }

    /// REQ-VC-vm-cli-020: a malformed line is a codec error — no V-code — so
    /// the answer carries a null vCode and the session keeps serving.
    #[spec_test(REQ_VC_vm_cli_020)]
    #[test]
    fn serve_session_when_malformed_line_then_null_vcode_error_and_session_continues() {
        let mut host = counter_host();

        let responses = run_session(&mut host, &["not json", r#"{"command":"getStatus"}"#]);

        assert_eq!(responses.len(), 2);
        assert_eq!(responses[0]["response"], "error");
        assert_eq!(responses[0]["vCode"], serde_json::Value::Null);
        assert!(responses[0]["message"]
            .as_str()
            .unwrap()
            .starts_with("invalid command line:"));
        assert_eq!(responses[1]["response"], "status");
    }

    /// REQ-VC-vm-cli-021: EOF ends the session cleanly, with no output when
    /// no commands were sent.
    #[spec_test(REQ_VC_vm_cli_021)]
    #[test]
    fn serve_session_when_eof_immediately_then_ok_and_silent() {
        let mut host = counter_host();
        let mut output = Vec::new();

        serve_session(
            &mut host,
            io::Cursor::new(Vec::new()),
            &mut output,
            None,
            &device_identity(),
        )
        .unwrap();

        assert!(output.is_empty());
    }

    /// A BufRead whose first read fails.
    struct FailingReader;

    impl io::Read for FailingReader {
        fn read(&mut self, _buf: &mut [u8]) -> io::Result<usize> {
            Err(io::Error::other("simulated read failure"))
        }
    }

    impl BufRead for FailingReader {
        fn fill_buf(&mut self) -> io::Result<&[u8]> {
            Err(io::Error::other("simulated read failure"))
        }

        fn consume(&mut self, _amt: usize) {}
    }

    /// REQ-VC-vm-cli-022: a stdin read failure surfaces as the session's I/O
    /// error, which the command maps to V6011.
    #[spec_test(REQ_VC_vm_cli_022)]
    #[test]
    fn serve_session_when_stdin_read_fails_then_io_error() {
        let mut host = counter_host();
        let mut output = Vec::new();

        let err = serve_session(
            &mut host,
            FailingReader,
            &mut output,
            None,
            &device_identity(),
        )
        .unwrap_err();

        assert_eq!(err.kind(), io::ErrorKind::Other);
        assert!(output.is_empty());
    }

    /// A Write impl that always fails, used to cover the write-error path.
    struct FailingWriter;

    impl Write for FailingWriter {
        fn write(&mut self, _buf: &[u8]) -> io::Result<usize> {
            Err(io::Error::other("simulated write failure"))
        }

        fn flush(&mut self) -> io::Result<()> {
            Ok(())
        }
    }

    /// REQ-VC-vm-cli-022: a stdout write failure surfaces as the session's
    /// I/O error, which the command maps to V6011.
    #[spec_test(REQ_VC_vm_cli_022)]
    #[test]
    fn serve_session_when_stdout_write_fails_then_io_error() {
        let mut host = counter_host();

        let err = serve_session(
            &mut host,
            io::Cursor::new(b"{\"command\":\"getStatus\"}\n".to_vec()),
            FailingWriter,
            None,
            &device_identity(),
        )
        .unwrap_err();

        assert_eq!(err.kind(), io::ErrorKind::Other);
    }
}
