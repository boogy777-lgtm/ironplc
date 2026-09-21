//! The `ironplcvm serve` command: a newline-delimited JSON command session
//! over the runtime host's hot-edit protocol, composed with the HA
//! redundancy shell's engineering surface.
//!
//! [`serve`] loads and starts a compiled program through the same container
//! path as `run` (see [`crate::cli::load_container`]), then answers one line
//! of JSON per line of stdin until EOF. Stdout is the protocol channel:
//! startup prints nothing, every response line is flushed as written, and
//! anything diagnostic goes to stderr through the logger. The TCP transport
//! of the engineering connection (ADR-0063) is the same session loop over a
//! length-prefixed frame — see [`crate::tcp`].
//!
//! The session composes both command layers (the HA engineering UI
//! contract, `ironplcvm serve`'s client mapping): every line first parses
//! against the runtime's hot-edit [`Command`], then against the
//! redundancy crate's [`HaCommand`] — disjoint vocabularies, so neither
//! layer's FSM nor the one-line-in/one-line-out ordering is disturbed. A
//! line that parses against neither is the codeless codec error, exactly
//! like a malformed hot-edit line today.
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
//!
//! The HA composition (the redundancy architecture's "Shell, Not Runtime
//! +1"): [`compose_shell`] builds the shell the composition root serves —
//! the honest standalone shell by default, or the demo binding's simulated
//! loopback pair under `--ha-simulated-peer`, so every HA state is
//! exercisable end-to-end through the session. The host boots unpermitted;
//! the shell's admission bring-up decides the verdict and
//! [`permit_for`](ironplc_redundancy::permit_for) applies it to the host's
//! latch. In simulated mode the shell's [`Shell::tick`] runs once per
//! command line — the command cadence is the simulation clock — and the
//! session reconciles the permit latch with the shell's verdict after every
//! command (a commanded swap demotes this unit: its scans refuse at the
//! latch from the next line on). The driven round's scan-commit callback
//! feeds [`Shell::on_scan_commit`], the supervisor's mint seam.

use std::io::{self, BufRead, Write};
use std::path::Path;

use ironplc_container::Container;
use ironplc_redundancy::{
    execute_ha, parse_ha_command, permit_for, render_ha_response, AdmissionVerdict, ConfiguredRole,
    HaResponse, ModuleId, ModuleRegistry, ModuleTiming, PairId, RedundancyConfig, Shell, Side,
};
use ironplc_runtime::{
    execute, parse_command, render_response, Command, DeviceIdentity, RedundancyIdentity, Response,
    RuntimeError, RuntimeHost,
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
/// (seeding the store from the file on first serve), composes the runtime
/// host with the HA shell (standalone by default; the simulated loopback
/// pair under `ha_simulated_peer`), and serves commands until stdin reaches
/// EOF.
pub fn serve(path: &Path, ha_simulated_peer: bool) -> Result<(), VmError> {
    let mut store = SlotStore::beside(path);
    let container = store.boot()?;
    let (mut host, mut shell) = compose_host(container, ha_simulated_peer)?;

    let stdin = io::stdin();
    let stdout = io::stdout();
    let device = device_identity();
    serve_session(
        &mut host,
        stdin.lock(),
        stdout.lock(),
        Some(&mut store),
        &device,
        Some(&mut shell),
    )
    .map_err(|err| {
        VmError::io(
            error::SESSION_IO,
            format!("unable to read or write the command session: {err}"),
        )
    })
}

/// Creates the runtime host for `container` and composes the HA shell,
/// mapping init traps to the trap's V-code exactly like `run` does.
///
/// The host boots without the execution permit (the HA redundancy
/// architecture, "Minimal Seams" 1); the shell's admission bring-up owns the
/// grant policy: the standalone shell admits immediately (today's standalone
/// behavior), the simulated pair admits on the pair verdict — Primary grants,
/// Secondary refuses. `permit_for` is the existing verdict→permit mapping;
/// the session reconciles the latch with the shell's live verdict from then
/// on (a commanded swap or a fencing loss can demote the unit between
/// commands).
pub(crate) fn compose_host(
    container: Container,
    ha_simulated_peer: bool,
) -> Result<(RuntimeHost, Shell), VmError> {
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
    let mut shell = compose_shell(ha_simulated_peer);
    shell.note_application_generation(host.status().application.raw());
    let verdict = shell.start_up();
    permit_for(&mut host, verdict);
    Ok((host, shell))
}

/// Composes the shell of one served process: the honest standalone shell by
/// default, or the demo binding's simulated pair — the loopback link, the
/// simulator registry with two firmware-instrumented modules, and the fixed
/// required set — under `--ha-simulated-peer`. The demo values are the
/// composition root's stand-ins, exactly the class of the simulator
/// binding's device properties: they exist so every HA state is exercisable
/// end-to-end before the EtherNet/IP binding and the I/O configuration do.
fn compose_shell(ha_simulated_peer: bool) -> Shell {
    if !ha_simulated_peer {
        return Shell::standalone();
    }
    let config = RedundancyConfig::pair(PairId::new(7), ConfiguredRole::Primary);
    let registry = ModuleRegistry::new(2)
        .with_connection_timeout(3)
        .with_module_timing(
            ModuleId::new(0),
            ModuleTiming {
                claim_ticks: 2,
                arm_ticks: 1,
                output_apply_ticks: 1,
            },
        )
        .with_module_timing(
            ModuleId::new(1),
            ModuleTiming {
                claim_ticks: 5,
                arm_ticks: 2,
                output_apply_ticks: 1,
            },
        );
    Shell::simulated(config, registry, vec![ModuleId::new(0), ModuleId::new(1)])
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
/// `ha` is the composed redundancy shell (always `Some` from [`serve`]; a
/// `None` serves the honestly HA-free session the tests of the hot-edit path
/// use). In simulated mode the shell advances one tick per command line —
/// the command cadence is the simulation clock — and the permit latch is
/// reconciled with the shell's verdict after every command.
///
/// Returns when the reader reaches EOF, or with the first I/O failure.
pub fn serve_session(
    host: &mut RuntimeHost,
    reader: impl BufRead,
    mut writer: impl Write,
    mut store: Option<&mut SlotStore>,
    device: &DeviceIdentity,
    mut ha: Option<&mut Shell>,
) -> io::Result<()> {
    for line in reader.lines() {
        let line = line?;
        // The simulated pair's clock: one tick per command line, whatever
        // the command — the hot-edit keepalive advances the pair too.
        if let Some(shell) = ha.as_deref_mut() {
            if shell.has_pair() {
                shell.tick();
            }
        }
        let response = match parse_command(&line) {
            Ok(command) => {
                let commits = matches!(command, Command::AssembleEdits);
                let advances = advances_state(&command);
                let mut response = execute(command, host, device);
                // The ADR-0063 identity handshake answers the redundancy
                // block from the composed shell: present exactly when a
                // pair is configured, absent standalone — the field the
                // runtime layer leaves for the server that composes one.
                if let (Some(shell), Response::Identity(payload)) = (ha.as_deref(), &mut response) {
                    payload.redundancy = identity_block(shell);
                }
                if advances && matches!(response, Response::Ack) {
                    if let Some(err) = drive_scan_round(host, ha.as_deref_mut()) {
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
            Err(command_err) => match parse_ha_command(&line) {
                Ok(ha_command) => match ha.as_deref_mut() {
                    Some(shell) => {
                        let response = execute_ha(ha_command, shell);
                        apply_ha_permit(host, shell);
                        render_ha_line(&response)?
                    }
                    // The session always composes a shell; a None here is a
                    // test of the hot-edit path alone, which answers the HA
                    // vocabulary like the codec layer it shares: one error
                    // line, no V-code, and the session continues.
                    None => codec_error_line(&command_err),
                },
                Err(ha_err) => {
                    // A malformed line is a codec error, not a refusal: it
                    // has no V-code (ADR-0055). The transport answers with
                    // one error line so the wire stays aligned — one line
                    // in, one line out — and the session continues. The
                    // message comes from the vocabulary the line named.
                    let err = if line_names_ha_command(&line) {
                        ha_err
                    } else {
                        command_err
                    };
                    log::warn!("ignoring malformed command line: {err}");
                    codec_error_line(&err)
                }
            },
        };
        writeln!(writer, "{response}")?;
        writer.flush()?;
    }
    Ok(())
}

/// Whether a malformed line named the HA vocabulary on its `command` tag:
/// the error message then comes from the HA parser, so a scripted HA client
/// sees why its line failed.
fn line_names_ha_command(line: &str) -> bool {
    serde_json::from_str::<serde_json::Value>(line)
        .ok()
        .and_then(|value| {
            value
                .get("command")
                .and_then(|tag| tag.as_str())
                .map(str::to_owned)
        })
        .is_some_and(|tag| tag.starts_with("ha"))
}

/// The ADR-0063 identity redundancy block, filled from the composed
/// shell: present exactly when a pair is configured (absent means
/// standalone), the same additive-block shape the runtime layer defines.
fn identity_block(shell: &Shell) -> Option<RedundancyIdentity> {
    if !shell.has_pair() {
        return None;
    }
    let local = shell.unit_view(Side::Local)?;
    Some(RedundancyIdentity {
        pair_id: shell
            .config()
            .pair_id
            .map(|id| id.to_string())
            .unwrap_or_default(),
        role: local.role.as_str().to_string(),
        epoch: shell.epoch().raw(),
        sync: local.sync.as_str().to_string(),
        control: local.control.as_str().to_string(),
    })
}

/// Applies the shell's permit verdict to the host latch (the redundancy
/// architecture's policy/mechanism split): the shell owns the policy of
/// when this unit may execute, the host owns the enforcement. A standalone
/// or output-controlling unit is granted; anything else — a Secondary after
/// a commanded swap, a unit that lost the barrier — is revoked, so its next
/// driven round refuses at the latch (V4018) instead of executing.
fn apply_ha_permit(host: &mut RuntimeHost, shell: &Shell) {
    match shell.local_verdict() {
        AdmissionVerdict::Secondary => host.revoke_execution_permit(),
        AdmissionVerdict::Standalone | AdmissionVerdict::Primary => host.permit_execution(),
    }
}

/// Renders one typed response as the session's single output line.
fn render_line(response: &Response) -> io::Result<String> {
    render_response(response).map_err(|err| io::Error::new(io::ErrorKind::InvalidData, err))
}

/// Renders one typed HA response as the session's single output line.
fn render_ha_line(response: &HaResponse) -> io::Result<String> {
    render_ha_response(response).map_err(|err| io::Error::new(io::ErrorKind::InvalidData, err))
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
/// With a composed shell, the driven boundary reports through the
/// scan-commit seam ([`Shell::on_scan_commit`]): the epoch mint, the lease
/// renewal, and the output-commit stamping are the supervisor's answer to
/// the boundary. Without one, the standalone composition only observes the
/// boundary identity, as it always has.
///
/// Returns the trap as a [`VmError`] — its V-code is the trap's own — so the
/// caller can surface it; a trapped round does not end the session. A
/// violated host invariant keeps no V-code of its own (ADR-0055): it is
/// logged here and reported as `None`.
pub(crate) fn drive_scan_round(
    host: &mut RuntimeHost,
    mut ha: Option<&mut Shell>,
) -> Option<VmError> {
    match host.run_with_commit(
        1,
        || 0,
        |commit| match ha.as_deref_mut() {
            Some(shell) => shell.on_scan_commit(commit),
            None => log::debug!("scan boundary committed: {commit:?}"),
        },
    ) {
        Ok(()) => None,
        Err(RuntimeError::Trap(context)) => Some(VmError::from_trap(
            &context.trap,
            context.task_id,
            context.instance_id,
        )),
        Err(RuntimeError::NotPermitted) => {
            // The HA shell demoted this unit (a commanded swap made it the
            // Secondary, or the barrier was lost): the boundary refuses at
            // the permit latch — the composition is working, not broken.
            // The acknowledgment already stands; the rounds counter the
            // client reads back simply stops advancing.
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
