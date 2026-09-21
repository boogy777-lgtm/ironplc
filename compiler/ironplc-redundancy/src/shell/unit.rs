//! One unit of the simulated pair: its charts, its liveness exchange end,
//! and its fencing identity — the per-unit machinery the
//! [`super::Shell`] drives on both sides of the loopback link.
//!
//! The unit consumes peer packets under the anti-stale epoch rule and
//! drives its own SYNC chart from the observations: a peer-restart report
//! or an epoch regression against the observed high-water is the
//! epoch-discontinuity input; a revival re-pairs the chart; one healthy
//! tick completes the replication. Transmitting answers with the next
//! PING (and the owed PONG), feeding the calibration profile the shell
//! shares.

use std::collections::VecDeque;

use crate::calibration::{Calibration, Direction};
use crate::config::{ConfiguredRole, PairId, RedundancyConfig};
use crate::epoch::Epoch;
use crate::fencing::OwnerId;
use crate::hal::NicPort;
use crate::lease::OwnerLease;
use crate::liveness::{Liveness, LivenessEvent, Packet, PairRole};
use crate::loopback::LoopbackPort;
use crate::statechart::{ControlChart, ControlEvent, ControlState, SyncChart, SyncEvent};

/// Which of the two units an operation addresses: the served unit
/// ([`Side::Local`]) or its simulated peer.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Side {
    /// The unit this process serves — the one the session's host runs.
    Local,
    /// The simulated peer of the demo binding.
    Peer,
}

impl Side {
    /// The other side of the pair.
    pub(crate) const fn other(self) -> Self {
        match self {
            Side::Local => Side::Peer,
            Side::Peer => Side::Local,
        }
    }
}

/// One unit of the pair — the local unit or the simulated peer. Owns
/// its charts, its liveness exchange end, and its fencing identity.
pub(crate) struct Unit {
    /// The configured role: static configuration, exchanged only by a
    /// completed commanded swap.
    pub(crate) role: ConfiguredRole,
    /// The permanent ControllerId of this unit.
    pub(crate) owner: OwnerId,
    /// The SYNC chart of this unit.
    pub(crate) sync: SyncChart,
    /// The CONTROL chart of this unit.
    pub(crate) control: ControlChart,
    /// This unit's end of the ping/pong exchange.
    pub(crate) liveness: Liveness,
    /// This unit's end of the loopback pair link.
    pub(crate) port: LoopbackPort,
    /// The unit's epoch (adopted from peer packets under the
    /// anti-stale rule; minted at scan commit by the owner).
    pub(crate) epoch: Epoch,
    /// The OwnerLease minted when this unit passed the barrier.
    pub(crate) lease: Option<OwnerLease>,
    /// Send ticks of unanswered PINGs, oldest first: the RTT samples.
    pub(crate) in_flight: VecDeque<u64>,
    /// The highest PONG sequence observed from the peer.
    pub(crate) last_pong_seq: u64,
    /// The tick of the last confirmed PONG — the "last owner packet"
    /// the timeout event names.
    pub(crate) last_confirm_tick: u64,
    /// The peer's application generation as last observed on the wire.
    pub(crate) observed_generation: Option<u32>,
    /// The highest epoch this unit has observed from the peer: the
    /// restart evidence is a regression below this high-water (a
    /// rebooted peer lost its epoch memory), not an in-flight frame
    /// from before a coordinated swap.
    pub(crate) observed_epoch: Epoch,
    /// Whether the exchange counted the peer dead at the last
    /// observation (revival detection).
    pub(crate) was_dead: bool,
    /// Whether the chart is waiting for one healthy tick to record
    /// replication complete after re-pairing.
    pub(crate) replication_pending: bool,
}

impl Unit {
    pub(crate) fn new(
        config: &RedundancyConfig,
        role: ConfiguredRole,
        owner: OwnerId,
        port: LoopbackPort,
    ) -> Self {
        Self {
            role,
            owner,
            sync: SyncChart::new(),
            control: ControlChart::new(),
            liveness: Liveness::new(config),
            port,
            // The demo binding's boot epoch: any live peer epoch adopts
            // from here (Epoch(0) is never used on the wire).
            epoch: Epoch::new(1),
            lease: None,
            in_flight: VecDeque::new(),
            last_pong_seq: 0,
            last_confirm_tick: 0,
            observed_generation: None,
            observed_epoch: Epoch::new(0),
            was_dead: false,
            replication_pending: false,
        }
    }

    /// The pair role this unit presents on the wire: the configured
    /// role (in this slice the admitted verdict always matches it —
    /// only a completed commanded swap changes it).
    pub(crate) fn pair_role(&self) -> PairRole {
        match self.role {
            ConfiguredRole::Primary => PairRole::Primary,
            ConfiguredRole::Secondary => PairRole::Secondary,
        }
    }

    /// Receives one validated peer packet: feeds the exchange and the
    /// calibration profile, adopts the peer epoch under the anti-stale
    /// rule, and drives the SYNC chart from the observations.
    pub(crate) fn observe(
        &mut self,
        packet: &Packet,
        now: u64,
        calibration: &mut Calibration,
        direction: Direction,
    ) {
        if let Some(LivenessEvent::PeerRestarted) = self.liveness.note_received(packet) {
            // A restarted peer lost its epoch memory: the liveness
            // exchange's restart report is the chart's
            // epoch-discontinuity input (T13, external FSM review).
            self.sync.apply(SyncEvent::EpochDiscontinuity);
        }
        if packet.pong_seq > self.last_pong_seq {
            self.last_pong_seq = packet.pong_seq;
            self.last_confirm_tick = now;
            calibration.note_pong(direction);
            if let Some(sent) = self.in_flight.pop_front() {
                calibration.record_rtt(direction, now - sent);
            }
        }
        self.observed_generation = Some(packet.generation);
        // The restart evidence is an epoch regression against the
        // observed high-water — a rebooted peer lost its epoch memory —
        // never an in-flight frame from before a coordinated swap or
        // claim (an ordinary stale observation drops silently under
        // the anti-stale rule, ADR-0062).
        if packet.epoch < self.observed_epoch {
            self.sync.apply(SyncEvent::EpochDiscontinuity);
        } else {
            self.observed_epoch = packet.epoch;
        }
        let _ = self.epoch.adopt(packet.epoch);
        if self.was_dead && !self.liveness.is_dead() {
            // The peer revived: re-pair; one healthy tick completes
            // the (instantaneous, on a loopback) replication.
            self.was_dead = false;
            self.sync.apply(SyncEvent::Paired);
            self.replication_pending = true;
        }
    }

    /// Advances one exchange: sends the next PING (piggybacking the
    /// owed PONG), feeds the calibration profile, and reports the
    /// peer-death confirmation when the missed threshold crosses.
    pub(crate) fn transmit(
        &mut self,
        now: u64,
        pair_id: PairId,
        generation: u32,
        calibration: &mut Calibration,
        direction: Direction,
    ) -> Option<LivenessEvent> {
        let missed_before = self.liveness.missed();
        let (ping_seq, pong_seq, event) = self.liveness.begin_exchange();
        for _ in missed_before..self.liveness.missed() {
            calibration.note_miss(direction);
        }
        calibration.note_ping(direction);
        self.in_flight.push_back(now);
        let packet = Packet {
            pair_id,
            role: self.pair_role(),
            epoch: self.epoch,
            generation,
            ping_seq,
            pong_seq,
        };
        // The loopback binding's send is infallible (its
        // `PortError::Closed` is never constructed); a frame the link
        // drops is the partition model, not an error to act on.
        let _ = self.port.send(&packet.encode());
        event
    }
}

/// Applies the fencing-loss observation to one unit's CONTROL chart.
pub(crate) fn apply_loss(unit: &mut Unit, required: usize, held: usize) {
    match unit.control.state() {
        ControlState::Active => {
            if held == 0 {
                unit.control.apply(ControlEvent::IoLossFull);
            } else if held < required {
                unit.control.apply(ControlEvent::IoLossPartial);
            }
        }
        ControlState::ActiveDegraded if held == 0 => {
            unit.control.apply(ControlEvent::IoLossFull);
        }
        _ => {}
    }
}
