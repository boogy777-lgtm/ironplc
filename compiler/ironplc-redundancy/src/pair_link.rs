//! The production per-unit pair link: the real two-process pair driver
//! over any [`NicPort`].
//!
//! The loopback scenarios of `tests/pair_link.rs` and `tests/crossload.rs`
//! composed the crate's modules through test-support nodes with the
//! explicit expectation that "the real shell replaces it when a binary
//! embeds the layer". This module is that replacement: one [`PairLink`]
//! owns a unit's end of the pair link — the liveness exchange, admission
//! discovery, the anti-stale epoch, the SYNC chart, the crossload
//! receiver, and the ADR-0064 outbound pipeline — and the composition
//! root ([`ironplcvm serve`](https://github.com/ironplc/ironplc) in pair
//! mode) drives it: one [`PairLink::tick`] per pump cadence, scan rounds
//! driven through the host's scan-commit seam, and the execution permit
//! reconciled from [`PairLink::local_verdict`] after every tick (the
//! architecture's policy/mechanism split: the link owns the policy, the
//! host owns the latch).
//!
//! Two bindings exist: the loopback simulator binding stays the test
//! vehicle (`PairLink<LoopbackPort>`), and the UDP binding
//! ([`crate::udp`]) carries the same frames between two real processes.
//! The driver never names its port's concrete type; a protocol swap
//! changes the binding, never this module (the architecture's "Protocol
//! Portability").
//!
//! What is deliberately not here (later slices, per the architecture):
//! the fencing/CONTROL machinery — the simulator shell owns that demo
//! surface — and the fencing-enforced promotion barrier. Promotion on
//! confirmed peer death is modeled at the verdict/policy layer exactly
//! like the loopback scenarios do (the takeover policy of ADR-0064(e));
//! the fencing mechanism replaces the model when it lands.

use ironplc_runtime::{HostMode, RuntimeHost, ScanCommit};

use crate::admission::{admit, classify, AdmissionVerdict, Discovery, Neighbor};
use crate::config::{ConfiguredRole, PairId, RedundancyConfig};
use crate::crossload::{
    decode, encode, CrossloadMessage, CrossloadReceiver, CrossloadRefusal, FRAME_MAGIC,
};
use crate::epoch::Epoch;
use crate::hal::NicPort;
use crate::liveness::{Liveness, LivenessEvent, Packet, PairRole, FRAME_LEN};
use crate::statechart::{CrossloadReadiness, SyncChart, SyncEvent, SyncState};

/// Exchanges in the discovery window before the unit decides admission
/// (the pair-link scenarios' window): long enough to see the peer's
/// role, short enough that a booting owner starts promptly.
const DISCOVERY_EXCHANGES: u32 = 3;

/// What one [`PairLink::tick`] reports to its driver: outcomes the
/// composition root acts on, never state it could read off the link.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub struct PairTick {
    /// The unit promoted itself after the peer's death was confirmed
    /// (the ADR-0064(e) takeover policy): the root grants the permit and
    /// drives the boundary round that mints the pair's next epoch.
    pub promoted: bool,
    /// The peer's Assemble notice was applied to this unit's host this
    /// tick: the root persists the drained commit bytes through its slot
    /// store (ADR-0064(h): both units persist inside the one commit).
    pub peer_committed: bool,
}

/// The production pair link of one unit, over the transport binding `P`.
pub struct PairLink<P: NicPort> {
    config: RedundancyConfig,
    port: P,
    liveness: Liveness,
    chart: SyncChart,
    epoch: Epoch,
    receiver: CrossloadReceiver,
    outbound: Vec<CrossloadMessage>,
    notified: Option<CrossloadRefusal>,
    discovery: Discovery,
    discovery_left: u32,
    verdict: Option<AdmissionVerdict>,
    refusal: Option<crate::admission::AdmissionRefusal>,
    paired: bool,
    /// A valid peer frame has been observed at least once since boot:
    /// the survivor of a confirmed death promotes itself; a unit that
    /// never paired promotes no one.
    peer_ever_paired: bool,
    /// Proof the owner was alive and executing after this unit paired:
    /// the owner's epoch advanced on the wire (one epoch per committed
    /// scan round — the OwnerLease evidence of ADR-0062). A standby that
    /// never saw the owner commit has no proven-live owner to take over
    /// from: it never promotes, so a slow owner boot cannot end in a
    /// spurious takeover.
    owner_proven_live: bool,
    epoch_agreed: bool,
    was_dead: bool,
    observed_peer_role: Option<PairRole>,
    observed_peer_epoch: Epoch,
    observed_peer_generation: Option<u32>,
    /// The peer revived this tick (a valid frame after a confirmed
    /// death): the replication source answers with a fresh image.
    peer_revived: bool,
    /// The owner-side readiness signal: the peer confirmed at least one
    /// replication unit (its `Accepted` answer). Cleared on an epoch
    /// discontinuity — a restarted peer has lost the replicated state.
    peer_confirmed: bool,
    application_generation: u32,
    state_generation: u64,
    now: u64,
}

impl<P: NicPort> PairLink<P> {
    /// Boots one pair unit over `port`: the chart at deSYNC (boot), an
    /// undecided admission, and a fresh crossload pipeline. The
    /// composition root reconciles the host's execution permit from
    /// [`Self::local_verdict`] as the discovery window decides.
    pub fn new(config: RedundancyConfig, port: P) -> Self {
        Self {
            liveness: Liveness::new(&config),
            port,
            config,
            chart: SyncChart::new(),
            epoch: Epoch::new(0),
            receiver: CrossloadReceiver::new(),
            outbound: Vec::new(),
            notified: None,
            discovery: Discovery::NoPeer,
            discovery_left: DISCOVERY_EXCHANGES,
            verdict: None,
            refusal: None,
            paired: false,
            peer_ever_paired: false,
            owner_proven_live: false,
            epoch_agreed: false,
            was_dead: false,
            observed_peer_role: None,
            observed_peer_epoch: Epoch::new(0),
            observed_peer_generation: None,
            peer_revived: false,
            peer_confirmed: false,
            application_generation: 0,
            state_generation: 0,
            now: 0,
        }
    }

    /// This unit's admission verdict, once the discovery window decides.
    /// `None` while discovering — the root holds the permit until then
    /// (fail closed).
    pub const fn local_verdict(&self) -> Option<AdmissionVerdict> {
        self.verdict
    }

    /// The refusal that pinned this unit out of the pair, if admission
    /// refused (a foreign pair on the link).
    pub const fn refusal(&self) -> Option<crate::admission::AdmissionRefusal> {
        self.refusal
    }

    /// The last crossload refusal the peer notified this unit of, if any
    /// (the Primary's "peer refused" surface).
    pub const fn notified(&self) -> Option<CrossloadRefusal> {
        self.notified
    }

    /// Queues one crossload message for the next transmit (the
    /// composition root packages offers from the host; the pair
    /// lifecycle notices are enqueued by the session's command handlers).
    pub fn enqueue(&mut self, message: CrossloadMessage) {
        self.outbound.push(message);
    }

    /// The scan-commit seam (the runtime's seam 2): the owner mints one
    /// epoch per committed round (ADR-0062); the generations are stamped
    /// from the boundary identity. The binding between host and link
    /// stays the root's — the link records, it never drives.
    pub fn on_scan_commit(&mut self, commit: ScanCommit) {
        self.state_generation = commit.rounds;
        self.application_generation = commit.application.raw();
        if self.verdict == Some(AdmissionVerdict::Primary) {
            self.epoch = self.epoch.next();
        }
    }

    /// Advances the pair link one pump tick: drains the inbound link
    /// (demuxing the liveness frames from the crossload frames by magic
    /// and length), feeds the charts, transmits the next exchange plus
    /// every queued crossload message, and applies the takeover policy
    /// when the exchange confirms the peer's death.
    pub fn tick(&mut self, host: &mut RuntimeHost) -> PairTick {
        self.now += 1;
        // The wire generation is the host's committed manifest — one
        // authority, read per tick (the link never caches host truth).
        self.application_generation = host.status().application.raw();

        let mut tick = PairTick {
            peer_committed: self.receive(host),
            ..PairTick::default()
        };
        self.update_chart();
        self.replicate(host);
        if self.transmit() {
            self.chart.apply(SyncEvent::PeerDied);
            self.paired = false;
            self.epoch_agreed = false;
            tick.promoted = self.consider_promotion(host);
        }
        tick
    }

    /// Repeats the boot lifecycle on the same port and configuration: the
    /// unit restarted and lost its epoch memory — a fresh exchange base,
    /// chart, and discovery window (the zombie/restart scenarios). The
    /// composition root reboots its host to match.
    pub fn restart(&mut self) {
        self.liveness = Liveness::new(&self.config);
        self.chart = SyncChart::new();
        self.epoch = Epoch::new(0);
        self.receiver = CrossloadReceiver::new();
        self.outbound.clear();
        self.discovery = Discovery::NoPeer;
        self.discovery_left = DISCOVERY_EXCHANGES;
        self.verdict = None;
        self.refusal = None;
        self.paired = false;
        self.peer_ever_paired = false;
        self.owner_proven_live = false;
        self.epoch_agreed = false;
        self.was_dead = false;
        self.observed_peer_role = None;
        self.observed_peer_epoch = Epoch::new(0);
        self.observed_peer_generation = None;
        self.peer_revived = false;
        self.peer_confirmed = false;
    }

    /// The replication source streams one state image per tick while the
    /// pair synchronizes (ADR-0064(d) "keeps replicating"): the boot sync
    /// is the owner's image applied by the standby, and a revived peer
    /// receives a fresh image after a confirmed death. The stream stops
    /// at SYNC_READY; the candidate-carrying pipeline (offers and the
    /// session-driven updates) takes over from there.
    fn replicate(&mut self, host: &mut RuntimeHost) {
        if !self.paired || !self.is_source() {
            self.peer_revived = false;
            return;
        }
        if self.chart.state() != SyncState::Syncing && !self.peer_revived {
            return;
        }
        self.peer_revived = false;
        self.enqueue(CrossloadMessage::StateUpdate(host.state_snapshot()));
    }

    /// Whether this unit is the replication source: the admitted owner,
    /// or a Primary-configured unit still discovering (its boot sync
    /// starts with the pair regardless of when the verdict lands).
    fn is_source(&self) -> bool {
        match self.verdict {
            Some(AdmissionVerdict::Primary) | Some(AdmissionVerdict::Standalone) => true,
            Some(AdmissionVerdict::Secondary) => false,
            None => self.config.role == ConfiguredRole::Primary,
        }
    }

    /// The read-only status view the engineering surface renders
    /// (`haStatus`, the identity redundancy block).
    pub fn status(&self) -> PairLinkStatus {
        PairLinkStatus {
            pair_id: self.pair_id(),
            role: self.pair_role(),
            configured_role: self.config.role,
            admitted: self.verdict,
            sync: self.chart.state(),
            sync_reason: self.chart.reason(),
            epoch: self.epoch,
            readiness: self.receiver.readiness(),
            alarm: self.receiver.alarm(),
            link_valid: self.paired && !self.liveness.is_dead(),
            peer: self.observed_peer_role.map(|role| PairPeerStatus {
                role,
                epoch: self.observed_peer_epoch,
                generation: self.observed_peer_generation.unwrap_or(0),
                live: self.paired && !self.liveness.is_dead(),
            }),
            application_generation: self.application_generation,
            state_generation: self.state_generation,
        }
    }

    /// The transport port, for the composition root's and the tests'
    /// binding-level controls (the loopback partition model).
    pub fn port_mut(&mut self) -> &mut P {
        &mut self.port
    }

    /// The SYNC chart (the integration scenarios assert transitions).
    pub const fn chart(&self) -> &SyncChart {
        &self.chart
    }

    /// The crossload receiver (readiness/alarm assertions).
    pub const fn receiver(&self) -> &CrossloadReceiver {
        &self.receiver
    }

    /// The liveness exchange (peer-death assertions).
    pub const fn liveness(&self) -> &Liveness {
        &self.liveness
    }

    /// The unit's epoch (anti-stale assertions).
    pub const fn epoch(&self) -> Epoch {
        self.epoch
    }

    /// The unit's configured pair identity.
    pub const fn pair_id(&self) -> PairId {
        match self.config.pair_id {
            Some(pair_id) => pair_id,
            None => PairId::new(0),
        }
    }

    /// The unit's static redundancy configuration.
    pub const fn config(&self) -> &RedundancyConfig {
        &self.config
    }

    /// The pair role this unit presents on the wire: the admitted
    /// ownership truth once the verdict exists, the configured role
    /// while discovering.
    fn pair_role(&self) -> PairRole {
        match self.verdict {
            Some(AdmissionVerdict::Primary) | Some(AdmissionVerdict::Standalone) => {
                PairRole::Primary
            }
            Some(AdmissionVerdict::Secondary) => PairRole::Secondary,
            None => match self.config.role {
                ConfiguredRole::Primary => PairRole::Primary,
                ConfiguredRole::Secondary => PairRole::Secondary,
            },
        }
    }

    /// Drains the inbound link; returns whether the peer's Assemble
    /// notice was applied this tick.
    fn receive(&mut self, host: &mut RuntimeHost) -> bool {
        let mut peer_committed = false;
        while let Some((_, frame)) = self.port.poll() {
            if frame.starts_with(&FRAME_MAGIC) {
                peer_committed |= self.receive_crossload(host, &frame);
                continue;
            }
            if frame.len() != FRAME_LEN {
                continue;
            }
            let Some(packet) = Packet::decode(&frame) else {
                continue;
            };
            if self.observe_packet(&packet) {
                self.epoch_agreed = false;
            }
        }
        peer_committed
    }

    /// Observes one validated liveness packet: discovery, the exchange,
    /// the epoch under the anti-stale rule, and the SYNC chart inputs.
    /// Returns whether an epoch discontinuity was recorded.
    fn observe_packet(&mut self, packet: &Packet) -> bool {
        match classify(self.pair_id(), packet) {
            Neighbor::ForeignPair => {
                if self.verdict.is_none() {
                    self.discovery = Discovery::ForeignPair;
                }
                return false;
            }
            Neighbor::Primary => self.observe_discovery(Discovery::LivePrimary),
            Neighbor::Secondary => self.observe_discovery(Discovery::LiveSecondary),
        }
        self.paired = true;
        self.peer_ever_paired = true;
        self.observed_peer_role = Some(packet.role);
        self.observed_peer_generation = Some(packet.generation);
        let mut discontinuity = false;
        if let Some(LivenessEvent::PeerRestarted) = self.liveness.note_received(packet) {
            // The peer rebooted and lost its epoch memory: the restart
            // report is the chart's epoch-discontinuity input (T13), and
            // the replication the pair had confirmed is void.
            self.chart.apply(SyncEvent::EpochDiscontinuity);
            self.peer_confirmed = false;
            discontinuity = true;
        } else {
            // Adopt the peer's epoch when it is not behind ours; a
            // monitoring peer trails the owner's mint without consequence.
            self.epoch.adopt(packet.epoch);
            if self.chart.state() == SyncState::Syncing {
                self.epoch_agreed = true;
            }
        }
        // The restart evidence is an epoch regression against the
        // observed high-water — never an in-flight frame from before a
        // coordinated swap (the anti-stale rule, ADR-0062).
        if packet.epoch < self.observed_peer_epoch {
            self.chart.apply(SyncEvent::EpochDiscontinuity);
            self.peer_confirmed = false;
            discontinuity = true;
        } else {
            // The owner's epoch advancing on the wire is its scan-commit
            // evidence: the owner is alive and executing (ADR-0062's
            // lease is born at scan commit, never in the network task).
            if packet.role == PairRole::Primary && packet.epoch > self.observed_peer_epoch {
                self.owner_proven_live = true;
            }
            self.observed_peer_epoch = packet.epoch;
        }
        if self.was_dead && !self.liveness.is_dead() {
            // The peer revived: re-pair; the replication source answers
            // with a fresh image, and one healthy exchange completes the
            // re-replication through the chart guards.
            self.was_dead = false;
            self.peer_revived = true;
            self.chart.apply(SyncEvent::Paired);
        }
        discontinuity
    }

    /// Records the strongest discovery observation of the window (a live
    /// Primary outranks a live Secondary; a foreign pair is assigned
    /// directly by the classifier).
    fn observe_discovery(&mut self, seen: Discovery) {
        if self.verdict.is_none() && self.refusal.is_none() {
            self.discovery = match (self.discovery, seen) {
                (Discovery::LivePrimary, _) | (_, Discovery::LivePrimary) => Discovery::LivePrimary,
                _ => Discovery::LiveSecondary,
            };
        }
    }

    /// The deterministic reaction to one decoded crossload message; the
    /// coded answer is queued for the peer and a refused transfer drops
    /// the SYNC chart (readiness lost ⇒ deSYNC, the FSM spec's policy).
    fn receive_crossload(&mut self, host: &mut RuntimeHost, frame: &[u8]) -> bool {
        let Some(message) = decode(frame) else {
            // A garbled transfer is dropped, never acted on; the latch
            // records the interruption and answers the coded refusal.
            let response = self.receiver.note_interrupted();
            self.chart.apply(SyncEvent::SyncLoss);
            self.outbound.push(response);
            return false;
        };
        let pair_id = self.pair_id();
        let mut peer_committed = false;
        let response = match message {
            CrossloadMessage::Offer(offer) => self.receiver.accept(pair_id, host, &offer),
            CrossloadMessage::StateUpdate(snapshot) => self.receiver.apply_update(host, &snapshot),
            CrossloadMessage::CancelCandidate => self.receiver.cancel_candidate(host),
            CrossloadMessage::UntestCandidate => self.receiver.untest_candidate(host),
            CrossloadMessage::AssembleCandidate => {
                peer_committed = true;
                self.receiver.assemble_candidate(host)
            }
            CrossloadMessage::Refused(refusal) => {
                self.notified = Some(refusal);
                return false;
            }
            CrossloadMessage::Accepted => {
                // The peer confirmed a replication unit: the owner's
                // side of "state replication complete" (the standby's
                // side is its receiver's readiness latch).
                self.peer_confirmed = true;
                return false;
            }
        };
        if matches!(response, CrossloadMessage::Refused(_)) {
            self.chart.apply(SyncEvent::SyncLoss);
        }
        self.outbound.push(response);
        peer_committed
    }

    /// Feeds the chart's guards and closes the discovery window.
    fn update_chart(&mut self) {
        if self.verdict.is_none() && self.refusal.is_none() && self.discovery_left > 0 {
            self.discovery_left -= 1;
            if self.discovery_left == 0 {
                self.decide();
            }
        }
        if self.chart.state() == SyncState::DeSync && self.paired {
            self.chart.apply(SyncEvent::Paired);
        }
        if self.chart.state() == SyncState::Syncing
            && self.epoch_agreed
            && (self.receiver.readiness() == CrossloadReadiness::Complete
                || (self.is_source() && self.peer_confirmed))
        {
            self.chart.apply(SyncEvent::ReplicationComplete);
        }
    }

    /// Transmits the next liveness exchange plus every queued crossload
    /// message; returns whether the exchange confirmed the peer's death.
    fn transmit(&mut self) -> bool {
        let (ping_seq, pong_seq, event) = self.liveness.begin_exchange();
        let died = matches!(event, Some(LivenessEvent::PeerDied));
        if died {
            self.was_dead = true;
        }
        let packet = Packet {
            pair_id: self.pair_id(),
            role: self.pair_role(),
            epoch: self.epoch,
            generation: self.application_generation,
            ping_seq,
            pong_seq,
        };
        // A frame the link drops is the partition/loss model, not an
        // error to act on; the exchange's miss counting owns the truth.
        let _ = self.port.send(&packet.encode());
        for message in self.outbound.drain(..) {
            let _ = self.port.send(&encode(&message));
        }
        died
    }

    /// Decides admission at the end of the discovery window. The permit
    /// itself is the composition root's (the policy/mechanism split).
    fn decide(&mut self) {
        match admit(self.config.role, self.discovery) {
            Ok(verdict) => self.verdict = Some(verdict),
            Err(refusal) => self.refusal = Some(refusal),
        }
    }

    /// The takeover policy of ADR-0064(e), modeled at the verdict/policy
    /// layer exactly like the loopback scenarios (the fencing barrier
    /// mechanism is a later slice): the survivor of a confirmed peer
    /// death — a unit that had paired and holds proof the owner was
    /// alive and executing (its epoch advanced on the wire, the
    /// scan-commit lease evidence of ADR-0062) — takes over executing
    /// the CANDIDATE, because reverting to Original equals an untest,
    /// which migration candidates forbid. A layout-preserving Test never
    /// reached this unit's selector over the replication stream, so the
    /// selector flips here; a migration candidate's replicated image
    /// already moved the state and the selector (the flip is then a
    /// no-op). A unit that never paired, and one that never saw the
    /// owner commit, promotes no one — a slow owner boot must not end in
    /// a spurious takeover — and the dead owner itself never re-promotes.
    fn consider_promotion(&mut self, host: &mut RuntimeHost) -> bool {
        if self.verdict != Some(AdmissionVerdict::Secondary)
            || !self.peer_ever_paired
            || !self.owner_proven_live
        {
            return false;
        }
        self.verdict = Some(AdmissionVerdict::Primary);
        if host.status().mode == HostMode::Normal && host.status().candidate.is_some() {
            let _ = host.takeover_testing();
        }
        true
    }
}

/// The read-only status of one [`PairLink`]: what the engineering
/// surface renders (the pair-mode `haStatus` and the identity redundancy
/// block) and nothing more.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct PairLinkStatus {
    /// The pair identity.
    pub pair_id: PairId,
    /// The role this unit presents on the wire right now.
    pub role: PairRole,
    /// The configured role (static configuration).
    pub configured_role: ConfiguredRole,
    /// The admission verdict, `None` while discovering.
    pub admitted: Option<AdmissionVerdict>,
    /// The SYNC substate.
    pub sync: SyncState,
    /// The reason of the last deSYNC entry.
    pub sync_reason: crate::statechart::DeSyncReason,
    /// The unit's epoch.
    pub epoch: Epoch,
    /// The crossload readiness signal.
    pub readiness: CrossloadReadiness,
    /// The latched crossload alarm, if any.
    pub alarm: Option<CrossloadRefusal>,
    /// Whether the pair link is valid (a live peer has been observed).
    pub link_valid: bool,
    /// The peer as observed on the wire; `None` before the first packet.
    pub peer: Option<PairPeerStatus>,
    /// The application generation (the committed manifest).
    pub application_generation: u32,
    /// The committed state generation (the host's boundary counter).
    pub state_generation: u64,
}

/// The peer unit as observed on the wire (the pair-mode status view):
/// what the link can honestly know — role, epoch, generation, liveness —
/// never the peer's internal chart states, which are not on the wire.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct PairPeerStatus {
    /// The role the peer's packets present.
    pub role: PairRole,
    /// The highest epoch observed from the peer.
    pub epoch: Epoch,
    /// The application generation last observed from the peer.
    pub generation: u32,
    /// Whether the peer is live right now.
    pub live: bool,
}
