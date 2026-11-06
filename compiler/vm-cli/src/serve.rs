//! The `ironplcvm serve` command: a newline-delimited JSON command session
//! over the runtime host's hot-edit protocol, composed with the HA
//! redundancy shell's engineering surface, while the device scans by itself.
//!
//! [`serve`] loads and starts a compiled program through the same container
//! path as `run` (see [`crate::cli::load_container`]), then answers one line
//! of JSON per line of stdin until EOF or Ctrl+C. Stdout is the protocol
//! channel: startup prints nothing, every response line is flushed as
//! written, and anything diagnostic goes to stderr through the logger. The TCP
//! transport of the engineering connection (ADR-0063) is the same session over
//! a length-prefixed frame — see [`crate::tcp`].
//!
//! The scan is run by the device loop ([`crate::device_loop`], ADR-0086), which
//! owns the host: a session thread relays each line to it and writes its
//! answer. This module is the loop's dispatch and composition: [`execute_line`]
//! executes a line, [`finish_line`] writes its response, and [`paced_round`]
//! runs a round; the loop calls them between rounds.
//!
//! The session composes both command layers (the HA engineering UI
//! contract, `ironplcvm serve`'s client mapping): every line first parses
//! against the runtime's hot-edit [`Command`], then against the
//! redundancy crate's [`HaCommand`] — disjoint vocabularies, so neither
//! layer's FSM nor the one-line-in/one-line-out ordering is disturbed. A
//! line that parses against neither is the codeless codec error, exactly
//! like a malformed hot-edit line today. In pair mode (the real UDP pair
//! link, [`crate::ha_pair`]) the HA layer answers the pair overview
//! ([`HaCommand::HaStatus`]) from the link's status; the simulator
//! binding's surfaces have no binding there and answer the codec error,
//! the same shape a vocabulary the session does not speak answers.
//!
//! The FSM-advancing commands (`testEdits`, `untestEdits`, `assembleEdits`)
//! only *request* a swap; the host applies it at the next scan boundary
//! ([`RuntimeHost::run_paced_round`]). The loop does not wait for the period
//! for it: the next round is the boundary, and the response line is written
//! after it ([`Executing::awaits_boundary`]), so the acknowledgment means the
//! swap has been applied and a client reads back state that has actually
//! switched. A trap in a round is the application's own fault: the device
//! scans no more, keeps answering, and reports the fault in `identity`.
//!
//! `assembleEdits` is also the single commit point (ADR-0064 amendment): the
//! session persists the host's committed wire bytes through the A/B
//! [`SlotStore`] after the boundary round, before the response line is
//! rendered. A persistence failure answers the wire with V6012 instead of
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
//! latch from the next line on). A round's scan-commit callback feeds the HA
//! backend's scan-commit seam, the supervisor's mint seam.
//!
//! The pair path ([`crate::ha_pair`]) is a composition of the same loop: the
//! loop owns the pair link, ticks it at the pump cadence, and runs the rounds
//! (ADR-0086, stage 3d). Every composition answers the runtime vocabulary, the
//! slot persistence, and — in pair mode — the ADR-0064 crossload hooks (the
//! offer at Accept, the state update at Test, the cancel/untest/assemble
//! notices) through the [`Ha`] backend enum.

use std::io;
use std::path::Path;
use std::sync::mpsc::Receiver;

use ironplc_container::Container;
use ironplc_redundancy::{
    execute_ha, package_offer, pair_link_status, parse_ha_command, permit_for, render_ha_response,
    AdmissionVerdict, ConfiguredRole, CrossloadMessage, HaCommand, HaResponse, ModuleId,
    ModuleRegistry, ModuleTiming, NicPort, PairId, PairLink, RedundancyConfig, Shell, Side,
    UdpPort,
};
use ironplc_runtime::{
    execute, parse_command, render_response, Command, RedundancyIdentity, Response, RuntimeError,
    RuntimeHost, ScanCommit,
};
use ironplc_vm::FaultContext;

use crate::device_loop::{self, relay_session, DeviceLoop, End, Envelope};
use crate::device_settings::device::Device;
use crate::device_settings::Entries;
use crate::error::{self, VmError};
use crate::pacer::{Pacer, SystemClock};
use crate::slot_store::SlotStore;

/// The HA backend one session serves: the demo binding's simulator shell,
/// or the real pair link over the UDP binding (the two-process pair).
/// Both own the permit *policy* the session reconciles with the host's
/// latch; the enum is the session's one dispatch shape, not a layer.
///
/// The pair link is generic over its port: the UDP binding of the two-process
/// pair is the default, and a hardware binding (EtherNet/IP) is another
/// [`NicPort`] behind the same link, with nothing here or in the loop to change.
pub(crate) enum Ha<'a, P: NicPort = UdpPort> {
    /// The simulated loopback pair (the `--ha-simulated-peer` demo).
    Shell(&'a mut Shell),
    /// The real pair link (the `--ha-peer-bind` composition).
    Pair(&'a mut PairLink<P>),
}

/// Boots the committed artifact from the A/B slot store beside `path`
/// (seeding the store from the file on first serve), composes the runtime
/// host with the HA shell (standalone by default; the simulated loopback
/// pair under `ha_simulated_peer`), and serves commands on stdin and stdout
/// while the device loop scans, until stdin reaches EOF or Ctrl+C.
pub fn serve(path: &Path, ha_simulated_peer: bool) -> Result<(), VmError> {
    let store = SlotStore::beside(path);
    let container = store.boot()?;
    let (mut host, shell) = compose_host(container, ha_simulated_peer)?;
    let mut device = Device::open(path)?;
    device.boot_application(&mut host);

    serve_stdio(|pacer, commands| {
        DeviceLoop::new(host, Some(store), device, shell, pacer, commands)
    })
}

/// Serves the command session of stdin and stdout through the device loop that
/// `compose` builds, and returns the outcome of the process: the session of
/// every stdio composition (standalone, simulated pair, real pair).
///
/// `compose` receives the pacer (stopped by Ctrl+C) and the receiving end of
/// the loop's command channel, so the composition decides only what the loop
/// owns.
pub(crate) fn serve_stdio<P: NicPort>(
    compose: impl FnOnce(Pacer, Receiver<Envelope>) -> DeviceLoop<SystemClock, P>,
) -> Result<(), VmError> {
    let stop = crate::cli::stop_on_ctrl_c()?;
    let (handle, commands) = device_loop::channel();
    // The session thread: the loop is the main thread, because it ends the
    // process in order (it writes the retained values), and a read of stdin
    // cannot be interrupted.
    let relay = std::thread::Builder::new()
        .name("serve-stdio".to_string())
        .spawn(move || relay_session(&handle, io::stdin().lock(), io::stdout().lock()))
        .map_err(|err| {
            VmError::io(
                error::SESSION_IO,
                format!("unable to spawn the session thread: {err}"),
            )
        })?;
    let mut device_loop = compose(Pacer::start(stop, None), commands);
    let session = match device_loop.run() {
        // The session thread ended: its outcome is the outcome of the session.
        End::Closed => relay
            .join()
            .unwrap_or_else(|_| Err(io::Error::other("the session thread panicked")))
            .map_err(|err| {
                VmError::io(
                    error::SESSION_IO,
                    format!("unable to read or write the command session: {err}"),
                )
            }),
        // Ctrl+C: the session thread may be blocked in a read of stdin that
        // only the end of the process interrupts.
        End::Stopped => Ok(()),
    };
    device_loop.stop(session)
}

/// The controlled stop of a served process: the end of its input, or Ctrl+C. Writes the
/// retained values (V6018 when it cannot), then returns the outcome of the
/// session. A failed session is the error to report; a failed write is the
/// error of a session that otherwise ended well, and is logged in both cases.
pub(crate) fn stop_device(
    device: &mut Device,
    host: &RuntimeHost,
    session: Result<(), VmError>,
) -> Result<(), VmError> {
    let stop = device.stopped(host);
    if let Err(err) = &stop {
        log::error!("retained values not written at the stop: {err}");
    }
    session.and(stop)
}

/// Boots the runtime host for `container`, mapping init traps to the
/// trap's V-code exactly like `run` does. The host boots without the
/// execution permit (the HA redundancy architecture, "Minimal Seams" 1);
/// the HA composition the root serves owns the grant policy.
pub(crate) fn boot_host(container: Container) -> Result<RuntimeHost, VmError> {
    RuntimeHost::new(container).map_err(|err| match err {
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
    })
}

/// Creates the runtime host for `container` and composes the HA shell,
/// mapping init traps to the trap's V-code exactly like `run` does.
///
/// The host boots without the execution permit; the shell's admission
/// bring-up owns the grant policy: the standalone shell admits immediately
/// (today's standalone behavior), the simulated pair admits on the pair
/// verdict — Primary grants, Secondary refuses. `permit_for` is the
/// existing verdict→permit mapping; the session reconciles the latch with
/// the shell's live verdict from then on (a commanded swap or a fencing
/// loss can demote the unit between commands).
pub(crate) fn compose_host(
    container: Container,
    ha_simulated_peer: bool,
) -> Result<(RuntimeHost, Shell), VmError> {
    let mut host = boot_host(container)?;
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

/// The pair-lifecycle notice one acknowledged hot-edit command owes the
/// pair link (ADR-0064): what the session enqueues after the local state
/// committed, so the peer mirrors the lifecycle.
enum PairNotice {
    /// `acceptEdits`: the offer carries the candidate's exact wire bytes.
    Accept(Vec<u8>),
    /// `testEdits`/`untestEdits`/`assembleEdits`/`cancelEdits`: notices
    /// without a payload; the state update snapshots the driven boundary.
    Test,
    Untest,
    Assemble,
    Cancel,
}

/// The notice `command` owes the pair link when acknowledged, if any.
fn pair_notice_for(command: &Command) -> Option<PairNotice> {
    match command {
        Command::AcceptEdits { program, .. } => Some(PairNotice::Accept(program.clone())),
        Command::TestEdits => Some(PairNotice::Test),
        Command::UntestEdits => Some(PairNotice::Untest),
        Command::AssembleEdits => Some(PairNotice::Assemble),
        Command::CancelEdits => Some(PairNotice::Cancel),
        _ => None,
    }
}

/// A command line after the runtime command layer executed it, before its
/// response line is written.
pub(crate) enum Executed {
    /// The line is answered: a settings change, an HA command, a malformed
    /// line. Nothing is left to do.
    Answer(String),
    /// A runtime command was executed; see [`Executing`] for what remains.
    Command(Executing),
}

/// A runtime command that was executed and whose response line is not written
/// yet. [`finish_line`] writes it, once [`awaits_boundary`](Self::awaits_boundary)
/// has been answered by a scan boundary.
pub(crate) struct Executing {
    response: Response,
    commits: bool,
    advances: bool,
    pair_notice: Option<PairNotice>,
}

impl Executing {
    /// Whether the response has to wait for a scan boundary: the command is an
    /// acknowledged request of a swap (`testEdits`, `untestEdits`,
    /// `assembleEdits`), and the acknowledgment means the swap has been
    /// applied, not merely recorded.
    pub(crate) fn awaits_boundary(&self) -> bool {
        self.advances && matches!(self.response, Response::Ack)
    }
}

/// Executes one command line through the runtime command layer (and the HA
/// layer for a line that is not a runtime command), up to the point where the
/// response depends on a scan boundary.
///
/// `device` holds the device's settings (see [`crate::device_settings`]): its
/// identity answers the `identity` handshake (ADR-0063), the first command on
/// every transport, and `setDeviceSettings` changes it. The runtime command
/// layer has no file system, so the settings command is answered here, the way
/// the slot store persists the commit of `assembleEdits` (see [`finish_line`]).
pub(crate) fn execute_line<P: NicPort>(
    host: &mut RuntimeHost,
    device: &mut Device,
    mut ha: Option<&mut Ha<P>>,
    line: &str,
) -> io::Result<Executed> {
    match parse_command(line) {
        Ok(Command::SetDeviceSettings { settings }) => {
            set_device_settings_line(device, &settings).map(Executed::Answer)
        }
        // The scan control is answered here, by the application that drives the
        // rounds: the device loop drives every composition of `serve`, so each
        // has a scan to start and stop. The runtime refuses it (V4022) only for
        // a host without a scan loop.
        Ok(command @ (Command::StartScan | Command::StopScan)) => {
            scan_control_line(host, device, &command).map(Executed::Answer)
        }
        Ok(command) => {
            let commits = matches!(command, Command::AssembleEdits);
            let advances = advances_state(&command);
            let pair_notice = pair_notice_for(&command);
            let mut response = execute(command, host, &device.identity());
            // The facts of the device (what it would boot, what it did with
            // the retained file at boot, whether its scan trapped): the
            // device owns them, the runtime layer leaves the fields empty.
            if let Response::Identity(payload) = &mut response {
                device.complete_identity(payload);
            }
            // The ADR-0063 identity handshake answers the redundancy
            // block from the composed HA backend: present exactly when a
            // pair is configured, absent standalone — the field the
            // runtime layer leaves for the server that composes one.
            if let (Some(backend), Response::Identity(payload)) = (ha.as_mut(), &mut response) {
                payload.redundancy = ha_identity_block(backend);
            }
            Ok(Executed::Command(Executing {
                response,
                commits,
                advances,
                pair_notice,
            }))
        }
        Err(command_err) => Ok(Executed::Answer(match parse_ha_command(line) {
            Ok(ha_command) => match ha.as_mut() {
                Some(backend) => match &mut **backend {
                    Ha::Shell(shell) => {
                        let response = execute_ha(ha_command, shell);
                        apply_ha_permit(host, shell);
                        render_ha_line(&response)?
                    }
                    // The pair-link session speaks the pair overview only; the
                    // simulator binding's surfaces (calibration, barrier, ...)
                    // have no binding here and answer the codec error — the
                    // same shape a vocabulary the session does not speak
                    // answers.
                    Ha::Pair(link) => match ha_command {
                        HaCommand::HaStatus => render_ha_line(&HaResponse::HaStatus(Box::new(
                            pair_link_status(&link.status()),
                        )))?,
                        other => pair_unavailable_line(&other),
                    },
                },
                // The session always composes an HA backend; a None here is a
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
                let err = if line_names_ha_command(line) {
                    ha_err
                } else {
                    command_err
                };
                log::warn!("ignoring malformed command line: {err}");
                codec_error_line(&err)
            }
        })),
    }
}

/// Writes the response line of an executed command, after the scan boundary
/// it awaited (if any) has passed.
///
/// An acknowledged `assembleEdits` with a `store` first persists the committed
/// wire bytes (see [`persist_commit`]); a `None` store serves the honestly
/// RAM-only session (no persistence). In pair mode, an acknowledged hot-edit
/// command also owes the pair its ADR-0064 notice (see [`PairNotice`]).
pub(crate) fn finish_line<P: NicPort>(
    host: &mut RuntimeHost,
    store: Option<&mut SlotStore>,
    device: &mut Device,
    ha: Option<&mut Ha<P>>,
    executing: Executing,
) -> io::Result<String> {
    let acked = matches!(executing.response, Response::Ack);
    let mut rendered: Option<String> = None;
    if acked && executing.commits {
        // ADR-0064 amendment: persist before the line renders, so
        // a failure answers V6012 instead of an ack.
        if let Some(store) = store {
            if let Err(err) = commit_assembled(host, store, device) {
                log::error!("assemble persistence failed: {err}");
                rendered = Some(coded_error_line(&err));
            }
        }
    }
    // The pair hears about a committed lifecycle only after the
    // local commit stands (a V6012 answer owes the peer nothing).
    if acked && rendered.is_none() {
        pair_post_command(ha, executing.pair_notice, host);
    }
    match rendered {
        Some(line) => Ok(line),
        None => render_line(&executing.response),
    }
}

/// Enqueues the pair-lifecycle notice an acknowledged command owes (the
/// ADR-0064 outbound pipeline): the offer packages the staged candidate
/// with the session's wire bytes at Accept; the Test snapshot rides the
/// boundary round that applied it; the remaining notices carry no payload.
fn pair_post_command<P: NicPort>(
    ha: Option<&mut Ha<P>>,
    notice: Option<PairNotice>,
    host: &mut RuntimeHost,
) {
    let (Some(Ha::Pair(link)), Some(notice)) = (ha, notice) else {
        return;
    };
    match notice {
        PairNotice::Accept(wire) => {
            // A packaging request with nothing staged is the caller's
            // sequencing error, not an offer of nothing — the host
            // acked the accept, so the candidate is there.
            match package_offer(link.pair_id(), link.epoch(), host, wire) {
                Some(offer) => link.enqueue(CrossloadMessage::Offer(offer)),
                None => {
                    log::error!("accept acknowledged but no candidate is staged for the pair offer")
                }
            }
        }
        PairNotice::Test => link.enqueue(CrossloadMessage::StateUpdate(host.state_snapshot())),
        PairNotice::Untest => link.enqueue(CrossloadMessage::UntestCandidate),
        PairNotice::Assemble => link.enqueue(CrossloadMessage::AssembleCandidate),
        PairNotice::Cancel => link.enqueue(CrossloadMessage::CancelCandidate),
    }
}

/// The ADR-0063 identity redundancy block, filled from the composed HA
/// backend: present exactly when a pair is configured (absent means
/// standalone), the same additive-block shape the runtime layer defines.
fn ha_identity_block<P: NicPort>(ha: &Ha<P>) -> Option<RedundancyIdentity> {
    match ha {
        Ha::Shell(shell) => identity_block(shell),
        Ha::Pair(link) => pair_identity_block(link),
    }
}

/// The identity redundancy block of the real pair link: the SYNC state
/// and the epoch are the link's own; the CONTROL substate is the
/// permit-based stand-in — `active` while this unit executes, `idle`
/// otherwise — until the fencing slice owns the CONTROL chart.
fn pair_identity_block<P: NicPort>(link: &PairLink<P>) -> Option<RedundancyIdentity> {
    let status = link.status();
    let control = match status.admitted {
        Some(AdmissionVerdict::Primary) | Some(AdmissionVerdict::Standalone) => "active",
        _ => "idle",
    };
    Some(RedundancyIdentity {
        pair_id: status.pair_id.to_string(),
        role: status.role.as_str().to_string(),
        epoch: status.epoch.raw(),
        sync: status.sync.as_str().to_string(),
        control: control.to_string(),
    })
}

/// The transport-level error for a valid HA command the pair-link
/// session does not speak. Codec errors carry no V-code, so `vCode` is
/// null — the same shape a malformed line answers.
fn pair_unavailable_line(command: &HaCommand) -> String {
    serde_json::json!({
        "response": "error",
        "vCode": null,
        "message": format!("the pair-link session does not serve {command:?}"),
    })
    .to_string()
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
pub(crate) fn persist_commit(host: &mut RuntimeHost, store: &mut SlotStore) -> Result<(), VmError> {
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

/// Persists an acknowledged assemble: the committed wire bytes through the slot
/// store (V6012 when it cannot), then, once the device would boot the new
/// application, the retained values of that application (V6018 when they cannot
/// be written). Either error replaces the acknowledgment on the wire; the
/// promotion in RAM stands in both cases.
pub(crate) fn commit_assembled(
    host: &mut RuntimeHost,
    store: &mut SlotStore,
    device: &mut Device,
) -> Result<(), VmError> {
    persist_commit(host, store)?;
    device.assembled(host)
}

/// Renders the answer that carries a shell code in place of the
/// acknowledgment: V6012 for the slot store and V6018 for the retained file
/// when the commit could not be persisted (ADR-0064 amendment), V6019 for a
/// start of a faulted device. Built directly: these are shell codes, not the
/// runtime command layer's V40xx codes, so the typed `Response` cannot carry
/// them.
fn coded_error_line(err: &VmError) -> String {
    serde_json::json!({
        "response": "error",
        "vCode": err.v_code(),
        "message": err.to_string(),
    })
    .to_string()
}

/// Answers `startScan` and `stopScan` (the scan control, REQ-VC-vm-cli-046):
/// the run state the device is in now, or the coded refusal (V6019 for a start
/// of a faulted device, V6018 when the retained values of a stop cannot be
/// written, with the device stopped all the same). Neither compiles anything or
/// touches the edit: the commands only change whether rounds run.
fn scan_control_line(
    host: &RuntimeHost,
    device: &mut Device,
    command: &Command,
) -> io::Result<String> {
    let changed = match command {
        Command::StartScan => device.start_scan(),
        _ => device.stop_scan(host),
    };
    match changed {
        Ok(run_state) => {
            log::info!("the scan is {run_state:?}");
            render_line(&Response::RunState { run_state })
        }
        Err(err) => {
            log::warn!("the run state is not as commanded: {err}");
            Ok(coded_error_line(&err))
        }
    }
}

/// Answers `setDeviceSettings`: the acknowledgment once the change is
/// checked and kept, else the coded refusal (V6015 for a value a setting
/// refuses, V6016 when the settings file cannot be written) with the device
/// unchanged. Built directly: the V60xx codes are shell codes that the typed
/// `Response` cannot carry.
fn set_device_settings_line(device: &mut Device, settings: &Entries) -> io::Result<String> {
    match device.change(settings) {
        Ok(()) => {
            log::info!("device settings changed");
            render_line(&Response::Ack)
        }
        Err(err) => {
            log::warn!("device settings not changed: {err}");
            Ok(serde_json::json!({
                "response": "error",
                "vCode": err.v_code(),
                "message": err.message(),
            })
            .to_string())
        }
    }
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

/// What one scan round came to.
pub(crate) enum RoundOutcome {
    /// The round ran and its boundary committed.
    Completed,
    /// The host holds no execution permit (the HA shell demoted this unit, or
    /// admission is not decided): nothing ran.
    Refused,
    /// The application trapped: the fault, with the task and the instance.
    Trapped(FaultContext),
    /// A host invariant was violated (logged; it has no V-code, ADR-0055).
    Invariant,
}

/// Reads the result of a call of the host that drove one round.
fn round_outcome(result: Result<(), RuntimeError>) -> RoundOutcome {
    match result {
        Ok(()) => RoundOutcome::Completed,
        Err(RuntimeError::Trap(context)) => RoundOutcome::Trapped(context),
        Err(RuntimeError::NotPermitted) => {
            // The HA shell demoted this unit (a commanded swap made it the
            // Secondary, or the barrier was lost): the boundary refuses at
            // the permit latch — the composition is working, not broken.
            // The acknowledgment already stands; the rounds counter the
            // client reads back simply stops advancing.
            log::error!("scan round refused: the host holds no execution permit");
            RoundOutcome::Refused
        }
        Err(RuntimeError::Internal { reason }) => {
            log::error!("runtime host invariant violated during a scan round: {reason}");
            RoundOutcome::Invariant
        }
    }
}

/// The scan-commit callback of a round: with a composed HA backend, the
/// boundary reports through the scan-commit seam — the epoch mint and the
/// generations are the supervisor's answer to the boundary. Without one, the
/// standalone composition only observes the boundary identity, as it always has.
fn notify_commit<P: NicPort>(ha: Option<&mut Ha<P>>, commit: ScanCommit) {
    match ha {
        Some(Ha::Shell(shell)) => shell.on_scan_commit(commit),
        Some(Ha::Pair(link)) => link.on_scan_commit(commit),
        None => log::debug!("scan boundary committed: {commit:?}"),
    }
}

/// Runs one round of the paced scan at `uptime_us`: the cyclic tasks keep the
/// deadlines the previous round set (see [`RuntimeHost::run_paced_round`]). The
/// round of the device loop.
pub(crate) fn paced_round<P: NicPort>(
    host: &mut RuntimeHost,
    mut ha: Option<&mut Ha<P>>,
    uptime_us: u64,
) -> RoundOutcome {
    round_outcome(
        host.run_paced_round(uptime_us, |commit| notify_commit(ha.as_deref_mut(), commit)),
    )
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
