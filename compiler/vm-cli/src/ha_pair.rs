//! The `ironplcvm serve` pair mode: one served process as a unit of a
//! real two-process redundant pair over UDP.
//!
//! The composition (the redundancy architecture's "Shell, Not Runtime
//! +1", at the process edge): the A/B [`SlotStore`] boots the committed
//! artifact, the runtime host owns the hot-edit state, and the
//! redundancy crate's [`PairLink`] over the [`UdpPort`] binding owns the
//! pair link — the liveness exchange, admission discovery, the SYNC
//! chart, the epoch, the crossload receiver, and the ADR-0064 outbound
//! pipeline. The pair is a composition of the device loop
//! ([`DeviceLoop`](crate::device_loop::DeviceLoop), ADR-0086, stage 3d), as the
//! standalone and the simulated shells are: the loop owns the host and the
//! link, answers the session over a channel like stdio does, and runs the
//! scan rounds. So the Primary scans between commands, and the Secondary
//! synchronizes, replicates, and promotes with no client connected at all.
//!
//! The pump cadence is the composition's clock: the loop calls [`pump`] once
//! per [`PUMP_PERIOD_US`] and waits no longer than that, whatever the tasks'
//! periods are. A pump is one [`PairLink::tick`], the execution permit
//! reconciled from the link's verdict ([`apply_pair_permit`]; the
//! policy/mechanism split — a promoted unit is granted before it scans, so the
//! next round is the takeover round it owes the pair), and a peer-applied
//! assemble persists the drained commit bytes through this unit's slot store
//! (ADR-0064(h): both units persist inside the one commit transaction). A
//! confirmed peer death promotes the survivor per ADR-0064(e) (the takeover
//! policy, modeled at the verdict/policy layer — the fencing barrier is a later
//! slice).

use std::net::SocketAddr;

use ironplc_redundancy::{AdmissionVerdict, NicPort, PairLink, RedundancyConfig, UdpPort};
use ironplc_runtime::RuntimeHost;

use crate::device_loop::DeviceLoop;
use crate::device_settings::device::Device;
use crate::error::{self, VmError};
use crate::serve::{boot_host, commit_assembled, serve_stdio};
use crate::slot_store::{Serving, SlotStore};

/// The pump cadence in microseconds: how often the loop ticks the pair link,
/// and the longest it waits for anything before ticking it. Bounds the pair's
/// convergence and failover latency in this soft-device composition; a
/// controller daemon drives the link on a hardware cadence instead.
///
/// The link's liveness counts exchanges, not time, so this is also the least
/// time between two ticks: a link ticked faster than its peer (once per round
/// of a freewheeling program) would count a peer that answers at this cadence
/// as dead.
pub(crate) const PUMP_PERIOD_US: u64 = 5_000;

/// Boots the committed artifact from the A/B slot store of `serving`,
/// composes the host with the pair link over UDP (`bind`, connected to
/// the peer's `peer`), and serves commands on stdin and stdout while the device
/// loop scans, until stdin reaches EOF or Ctrl+C.
pub fn serve_pair(
    serving: Serving<'_>,
    config: RedundancyConfig,
    bind: SocketAddr,
    peer: SocketAddr,
) -> Result<(), VmError> {
    let store = serving.store();
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
    let link = PairLink::new(config, port);
    let mut device = Device::open(serving.anchor())?;
    device.boot_application(&mut host);

    serve_stdio(|pacer, commands| DeviceLoop::paired(host, store, device, link, pacer, commands))
}

/// One pump of the pair link, between rounds: ticks the link, reconciles the
/// permit with its verdict, and acts on the tick's outcomes.
///
/// A promotion (the survivor of a confirmed peer death, ADR-0064(e)) needs no
/// action here: the verdict made the permit granted, so the unit scans again,
/// and the first round it runs is the boundary round the survivor owes the
/// pair, which mints the next epoch through the scan-commit seam. If the
/// operator has stopped the device, no round runs until the start.
pub(crate) fn pump<P: NicPort>(
    link: &mut PairLink<P>,
    host: &mut RuntimeHost,
    store: Option<&mut SlotStore>,
    device: &mut Device,
) {
    let tick = link.tick(host);
    apply_pair_permit(host, link);
    if tick.promoted {
        log::info!("promoted after the peer's death; executing the candidate");
    }
    if tick.peer_committed {
        // ADR-0064(h): the peer's assemble applied on this unit; its commit
        // bytes drain through this unit's slot store, so both units persist
        // inside the one transaction.
        match store {
            Some(store) => {
                if let Err(err) = commit_assembled(host, store, device) {
                    log::error!("peer commit persistence failed: {err}");
                }
            }
            None => log::error!("the peer committed but this unit has no slot store"),
        }
    }
    // The tick mirrors the pair's edit on this host without a command, so the
    // note of the unfinished edit is made to say what the edit is now.
    device.edit_changed(host);
}

/// Applies the pair link's verdict to the host's execution permit (the
/// redundancy architecture's policy/mechanism split): the owner executes,
/// everything else — a standby, a unit whose admission is still
/// undecided — refuses at the latch (V4018) instead of executing.
fn apply_pair_permit<P: NicPort>(host: &mut RuntimeHost, link: &PairLink<P>) {
    match link.local_verdict() {
        Some(AdmissionVerdict::Primary) | Some(AdmissionVerdict::Standalone) => {
            host.permit_execution()
        }
        Some(AdmissionVerdict::Secondary) | None => host.revoke_execution_permit(),
    }
}
