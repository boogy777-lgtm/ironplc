//! The `ironplcvm serve` pair mode: one served process as a unit of a
//! real two-process redundant pair over UDP.
//!
//! The composition (the redundancy architecture's "Shell, Not Runtime
//! +1", at the process edge): the A/B [`SlotStore`] boots the committed
//! artifact, the runtime host owns the hot-edit state, and the
//! redundancy crate's [`PairLink`] over the [`UdpPort`] binding owns the
//! pair link — the liveness exchange, admission discovery, the SYNC
//! chart, the epoch, the crossload receiver, and the ADR-0064 outbound
//! pipeline. The session is the existing JSON session
//! ([`handle_line`](crate::serve::handle_line)); the pump is the only new
//! shape: a stdin-reader thread feeds lines over a channel, and the
//! session loop waits on the channel with a short timeout, so the pair
//! link keeps exchanging with the peer process while no command arrives
//! — the Secondary, in particular, synchronizes, replicates, and
//! promotes with no client connected at all.
//!
//! The pump cadence is the composition's clock: one [`PairLink::tick`]
//! per wait (per line, or per timeout), the execution permit reconciled
//! from the link's verdict after every tick (the policy/mechanism split
//! — a promoted unit is granted before its takeover round drives), a
//! confirmed peer death promotes the survivor per ADR-0064(e) (the
//! takeover policy, modeled at the verdict/policy layer — the fencing
//! barrier is a later slice), and a peer-applied assemble persists the
//! drained commit bytes through this unit's slot store (ADR-0064(h):
//! both units persist inside the one commit transaction).

use std::io::{self, BufRead, Write};
use std::net::SocketAddr;
use std::path::Path;
use std::sync::mpsc::{self, RecvTimeoutError};
use std::time::Duration;

use ironplc_redundancy::{AdmissionVerdict, PairLink, RedundancyConfig, UdpPort};
use ironplc_runtime::{DeviceIdentity, RuntimeHost};

use crate::error::{self, VmError};
use crate::serve::{boot_host, device_identity, drive_scan_round, handle_line, persist_commit, Ha};
use crate::slot_store::SlotStore;

/// The pump cadence: how long the session loop waits for a command line
/// before ticking the pair link anyway. Bounds the pair's convergence
/// and failover latency in this soft-device composition; a controller
/// daemon drives the link on a hardware cadence instead.
const PUMP_PERIOD: Duration = Duration::from_millis(5);

/// Boots the committed artifact from the A/B slot store beside `path`,
/// composes the host with the pair link over UDP (`bind`, connected to
/// the peer's `peer`), and serves commands until stdin reaches EOF.
pub fn serve_pair(
    path: &Path,
    config: RedundancyConfig,
    bind: SocketAddr,
    peer: SocketAddr,
) -> Result<(), VmError> {
    let mut store = SlotStore::beside(path);
    let container = store.boot()?;
    let mut host = boot_host(container)?;
    let port = UdpPort::bind(bind, peer).map_err(|err| {
        VmError::io(
            error::SESSION_IO,
            format!("unable to bind the HA pair link on {bind}: {err}"),
        )
    })?;
    let local = port.local_addr().map_err(|err| {
        VmError::io(
            error::SESSION_IO,
            format!("unable to read the HA pair link's local address: {err}"),
        )
    })?;
    log::info!("serving the HA pair link on {local} (peer {peer})");
    let mut link = PairLink::new(config, port);
    let device = device_identity();

    // The stdin reader thread: the session loop multiplexes commands and
    // the pump over the channel, so the pair link outlives idle periods
    // (the Secondary pumps forever with no client). EOF drops the sender
    // and ends the loop.
    let (line_tx, line_rx) = mpsc::channel::<io::Result<String>>();
    std::thread::spawn(move || {
        let stdin = io::stdin();
        for line in stdin.lock().lines() {
            if line_tx.send(line).is_err() {
                return;
            }
        }
    });

    let stdout = io::stdout();
    let mut out = stdout.lock();
    serve_pair_loop(
        &mut host, &mut store, &mut link, &device, &line_rx, &mut out,
    )
    .map_err(|err| {
        VmError::io(
            error::SESSION_IO,
            format!("unable to read or write the command session: {err}"),
        )
    })
}

/// The pair-mode session loop: waits for a command line (or the pump
/// timeout), ticks the pair link, reconciles the permit with the
/// verdict, acts on the tick's outcomes, and answers the line through
/// the shared [`handle_line`] dispatch.
fn serve_pair_loop(
    host: &mut RuntimeHost,
    store: &mut SlotStore,
    link: &mut PairLink<UdpPort>,
    device: &DeviceIdentity,
    line_rx: &mpsc::Receiver<io::Result<String>>,
    out: &mut impl Write,
) -> io::Result<()> {
    loop {
        let line = match line_rx.recv_timeout(PUMP_PERIOD) {
            Ok(line) => Some(line?),
            Err(RecvTimeoutError::Timeout) => None,
            Err(RecvTimeoutError::Disconnected) => return Ok(()),
        };
        let tick = link.tick(host);
        apply_pair_permit(host, link);
        if tick.promoted {
            // The takeover (ADR-0064(e)): the permit is granted, so the
            // boundary round the survivor owes the pair mints the next
            // epoch through the scan-commit seam.
            log::info!("promoted after the peer's death; executing the candidate");
            if let Some(err) = drive_scan_round(host, Some(&mut Ha::Pair(link))) {
                log::error!("takeover scan round trapped: {err}");
            }
        }
        if tick.peer_committed {
            // ADR-0064(h): the peer's assemble applied on this unit; its
            // commit bytes drain through this unit's slot store, so both
            // units persist inside the one transaction.
            if let Err(err) = persist_commit(host, store) {
                log::error!("peer commit persistence failed: {err}");
            }
        }
        if let Some(line) = line {
            let response =
                handle_line(host, Some(store), device, Some(&mut Ha::Pair(link)), &line)?;
            writeln!(out, "{response}")?;
            out.flush()?;
        }
    }
}

/// Applies the pair link's verdict to the host's execution permit (the
/// redundancy architecture's policy/mechanism split): the owner executes,
/// everything else — a standby, a unit whose admission is still
/// undecided — refuses at the latch (V4018) instead of executing.
fn apply_pair_permit(host: &mut RuntimeHost, link: &PairLink<UdpPort>) {
    match link.local_verdict() {
        Some(AdmissionVerdict::Primary) | Some(AdmissionVerdict::Standalone) => {
            host.permit_execution()
        }
        Some(AdmissionVerdict::Secondary) | None => host.revoke_execution_permit(),
    }
}
