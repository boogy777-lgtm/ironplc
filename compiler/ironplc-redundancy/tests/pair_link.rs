//! Two-instance loopback scenarios for the pair link and the SYNC chart.
//!
//! Two units of one pair (or a foreign pair intruding) run their full
//! per-unit composition over the loopback simulator binding: link port,
//! ping/pong exchange, admission discovery, epoch adoption and minting
//! through a real [`RuntimeHost`] scan-commit callback, and the SYNC
//! chart. The node is test support — it composes the crate's modules the
//! way the redundancy shell will, and the real shell replaces it when a
//! binary embeds the layer.
//!
//! Epoch timing note: the owner mints one epoch per driven scan commit,
//! and the peer always sees the owner's *previous* frame, so the peer's
//! adopted epoch trails the owner's current one. Trailing is normal for a
//! monitoring peer and is not a discontinuity: an epoch discontinuity is
//! the *peer rebooting* — its per-channel PING sequence regresses against
//! the exchange's observed high-water (T13, external FSM review), because
//! a restarted peer lost its epoch memory. The scenarios assert "never
//! ahead" (anti-stale holds), "restart drops the survivor to deSYNC", and
//! "moved forward past the committed value", not exact equality at an
//! arbitrary cut.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use ironplc_container::{ContainerBuilder, FunctionId};
use ironplc_redundancy::{
    admit, classify, loopback_pair, permit_for, AdmissionRefusal, AdmissionVerdict, ConfiguredRole,
    CrossloadReadiness, DeSyncReason, Discovery, Epoch, Liveness, LivenessEvent, Neighbor, NicPort,
    Packet, PairId, PairRole, RedundancyConfig, SyncChart, SyncEvent, SyncState,
};
use ironplc_runtime::{RuntimeError, RuntimeHost};

/// Exchanges in the discovery window before the unit decides admission.
const DISCOVERY_EXCHANGES: u32 = 3;

/// One unit of a pair under test: the per-unit composition of link port,
/// liveness exchange, admission, epoch, SYNC chart, and the runtime host
/// whose scan commits mint the epoch.
struct TestNode {
    config: RedundancyConfig,
    port: ironplc_redundancy::LoopbackPort,
    liveness: Liveness,
    chart: SyncChart,
    epoch: Epoch,
    host: Option<RuntimeHost>,
    discovery: Discovery,
    discovery_left: u32,
    verdict: Option<AdmissionVerdict>,
    refusal: Option<AdmissionRefusal>,
    crossload: CrossloadReadiness,
    /// A valid peer frame has been observed since the last peer death.
    paired: bool,
    /// The peer epoch is agreed while synchronizing.
    epoch_agreed: bool,
}

/// A host over an empty program: the pair scenarios exercise the seams,
/// not the application.
fn shell_host() -> RuntimeHost {
    let container = ContainerBuilder::new()
        .add_function(FunctionId::INIT, &[], 0, 0, 0)
        .max_call_depth(1)
        .build();
    RuntimeHost::new(container).unwrap()
}

impl TestNode {
    /// Boots one unit of the configured pair: deSYNC with the boot
    /// reason, an unpermitted host, and an undecided admission.
    fn boot(config: RedundancyConfig, port: ironplc_redundancy::LoopbackPort) -> Self {
        assert!(config.pair_id.is_some(), "scenarios run pair units");
        Self {
            liveness: Liveness::new(&config),
            port,
            config,
            chart: SyncChart::new(),
            epoch: Epoch::new(0),
            host: Some(shell_host()),
            discovery: Discovery::NoPeer,
            discovery_left: DISCOVERY_EXCHANGES,
            verdict: None,
            refusal: None,
            crossload: CrossloadReadiness::InProgress,
            paired: false,
            epoch_agreed: false,
        }
    }

    /// Repeats the boot lifecycle on the same link port and
    /// configuration: the unit restarted (the zombie and stale-epoch
    /// scenarios resurrect a unit mid-scenario).
    fn resurrect(&mut self) {
        self.liveness = Liveness::new(&self.config);
        self.chart = SyncChart::new();
        self.epoch = Epoch::new(0);
        self.host = Some(shell_host());
        self.discovery = Discovery::NoPeer;
        self.discovery_left = DISCOVERY_EXCHANGES;
        self.verdict = None;
        self.refusal = None;
        self.crossload = CrossloadReadiness::InProgress;
        self.paired = false;
        self.epoch_agreed = false;
    }

    /// Models the outcome of a promotion (the promotion *mechanism* —
    /// fencing, barrier — is a later slice): the survivor's verdict
    /// becomes Primary through the same policy call admission uses.
    fn promote_to_primary(&mut self) {
        self.verdict = Some(AdmissionVerdict::Primary);
        permit_for(self.host.as_mut().unwrap(), AdmissionVerdict::Primary);
    }

    fn pair_id(&self) -> PairId {
        self.config.pair_id.unwrap()
    }

    /// The pair role this unit presents on the wire: the admitted
    /// ownership truth once the verdict exists, the configured role while
    /// discovering.
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

    fn generation(&self) -> u32 {
        self.host
            .as_ref()
            .map_or(0, |host| host.status().application.raw())
    }

    /// Drains the inbound link and feeds every valid frame to admission
    /// discovery, the exchange, and the epoch.
    fn receive(&mut self) {
        while let Some((_, frame)) = self.port.poll() {
            let Some(packet) = Packet::decode(&frame) else {
                continue;
            };
            match classify(self.pair_id(), &packet) {
                Neighbor::ForeignPair => {
                    if self.verdict.is_none() {
                        self.discovery = Discovery::ForeignPair;
                    }
                    continue;
                }
                Neighbor::Primary => self.observe(Discovery::LivePrimary),
                Neighbor::Secondary => self.observe(Discovery::LiveSecondary),
            }
            self.paired = true;
            if let Some(LivenessEvent::PeerRestarted) = self.liveness.note_received(&packet) {
                // The peer rebooted and lost its epoch memory: the chart
                // reads an epoch discontinuity (ADR-0062 anti-stale) and
                // drops to deSYNC; the peer's minted epoch catches up from
                // the restarted base.
                self.chart.apply(SyncEvent::EpochDiscontinuity);
                self.epoch_agreed = false;
            } else {
                // Adopt the peer's epoch when it is not behind ours. A
                // monitoring peer trails the owner's mint and adoption
                // rejects that without consequence — the local value never
                // moves backward.
                self.epoch.adopt(packet.epoch);
                if self.chart.state() == SyncState::Syncing {
                    self.epoch_agreed = true;
                }
            }
        }
    }

    /// Records the strongest discovery observation of the window.
    fn observe(&mut self, seen: Discovery) {
        if self.verdict.is_none() && self.refusal.is_none() {
            self.discovery = merge_discovery(self.discovery, seen);
        }
    }

    /// Advances one ping/pong exchange and sends the outbound frame.
    fn transmit(&mut self) {
        let (ping_seq, pong_seq, event) = self.liveness.begin_exchange();
        if let Some(LivenessEvent::PeerDied) = event {
            self.chart.apply(SyncEvent::PeerDied);
            self.paired = false;
            self.epoch_agreed = false;
        }
        let packet = Packet {
            pair_id: self.pair_id(),
            role: self.pair_role(),
            epoch: self.epoch,
            generation: self.generation(),
            ping_seq,
            pong_seq,
        };
        self.port.send(&packet.encode()).unwrap();
    }

    /// Closes the exchange: decides admission when the window ends, feeds
    /// the chart's re-pair and replication guards, and — when the verdict
    /// admits execution — drives one scan round whose committed boundary
    /// mints the next epoch (the runtime's scan-commit callback).
    fn advance(&mut self) {
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
            && self.crossload == CrossloadReadiness::Complete
        {
            self.chart.apply(SyncEvent::ReplicationComplete);
        }
        if self
            .verdict
            .map(|verdict| verdict.permits_execution())
            .unwrap_or(false)
        {
            if let Some(host) = self.host.as_mut() {
                let mut epoch = self.epoch;
                host.run_with_commit(
                    1,
                    || 0,
                    |_| {
                        epoch = epoch.next();
                    },
                )
                .unwrap();
                self.epoch = epoch;
            }
        }
    }

    fn decide(&mut self) {
        match admit(self.config.role, self.discovery) {
            Ok(verdict) => {
                if let Some(host) = self.host.as_mut() {
                    permit_for(host, verdict);
                }
                self.verdict = Some(verdict);
            }
            Err(refusal) => {
                self.refusal = Some(refusal);
            }
        }
    }

    fn chart_state(&self) -> SyncState {
        self.chart.state()
    }

    /// The host's rounds, proving whether the permit was granted.
    fn host_rounds(&self) -> u64 {
        self.host.as_ref().map_or(0, |host| host.status().rounds)
    }

    /// Driving scans without the permit: the refusal the unadmitted unit
    /// keeps answering (V4018).
    fn run_refused_code(&mut self) -> Option<&'static str> {
        let error = self.host.as_mut().unwrap().run(1, || 0).unwrap_err();
        assert!(matches!(error, RuntimeError::NotPermitted));
        error.v_code()
    }
}

/// Discovery precedence: a live Primary outranks a live Secondary — the
/// unit never admits on top of the strongest thing it saw. (A foreign
/// pair outranks everything and is assigned directly, not through this
/// merge.)
fn merge_discovery(current: Discovery, seen: Discovery) -> Discovery {
    use Discovery::*;
    match (current, seen) {
        (LivePrimary, _) | (_, LivePrimary) => LivePrimary,
        _ => LiveSecondary,
    }
}

/// Steps both units with their receive phases together, so each round
/// both see the previous round's frames — symmetric exchanges.
fn step_pair(a: &mut TestNode, b: &mut TestNode) {
    a.receive();
    b.receive();
    a.transmit();
    b.transmit();
    a.advance();
    b.advance();
}

fn pair_config(role: ConfiguredRole) -> RedundancyConfig {
    // Window 2 covers the two-exchange answer pipeline (a PING is answered
    // by the peer's next frame and read the round after that); the missed
    // threshold of 2 confirms death quickly in scenarios.
    RedundancyConfig::pair(PairId::new(1), role)
        .with_confirmation_exchanges(2)
        .with_missed_exchanges(2)
}

fn converged_pair() -> (TestNode, TestNode) {
    let (port_a, port_b) = loopback_pair();
    let mut a = TestNode::boot(pair_config(ConfiguredRole::Primary), port_a);
    let mut b = TestNode::boot(pair_config(ConfiguredRole::Secondary), port_b);
    a.crossload = CrossloadReadiness::Complete;
    b.crossload = CrossloadReadiness::Complete;
    for _ in 0..8 {
        step_pair(&mut a, &mut b);
    }
    (a, b)
}

#[test]
fn pair_link_when_primary_and_secondary_boot_then_both_converge_to_sync_ready() {
    // A/Primary and B/Secondary discover each other, admit (the owner is
    // the initial owner; the Secondary defers to the live Primary), sync
    // against the epochs the owner's scan commits mint, and reach
    // SYNC_READY — the Secondary in monitor mode: never permitted, never
    // executing.
    let (a, b) = converged_pair();

    assert_eq!(a.verdict, Some(AdmissionVerdict::Primary));
    assert_eq!(b.verdict, Some(AdmissionVerdict::Secondary));
    assert_eq!(a.chart_state(), SyncState::SyncReady);
    assert_eq!(b.chart_state(), SyncState::SyncReady);
    // The verdict lands on the third exchange and drives a round in the
    // same step; every committed round minted exactly one epoch through
    // the scan-commit callback.
    assert_eq!(a.host_rounds(), 6);
    assert_eq!(a.epoch.raw(), a.host_rounds() as u32);
    // The peer tracks the owner but is never ahead of it (anti-stale).
    assert!(b.epoch.raw() > 0);
    assert!(b.epoch.raw() < a.epoch.raw());
    assert_eq!(b.host_rounds(), 0);
}

#[test]
fn pair_link_when_exchanges_healthy_then_penalty_adds_one_per_exchange() {
    let (port_a, port_b) = loopback_pair();
    let mut a = TestNode::boot(pair_config(ConfiguredRole::Primary), port_a);
    let mut b = TestNode::boot(pair_config(ConfiguredRole::Secondary), port_b);

    for _ in 0..6 {
        step_pair(&mut a, &mut b);
    }

    // The first answerable PONG travels one round, so six rounds credit
    // four successful exchanges at +1 each — and none is missing.
    assert_eq!(a.liveness.penalty(), 4);
    assert_eq!(b.liveness.penalty(), 4);
    assert_eq!(a.liveness.missed(), 0);
    assert_eq!(b.liveness.missed(), 0);
}

#[test]
fn pair_link_when_peer_silent_then_penalty_adds_thousand_and_survivor_enters_de_sync() {
    // The +1000 step dominates the indicator immediately, and the
    // survivor's SYNC chart drops to deSYNC on confirmed peer death —
    // silence is a state input, never a promotion.
    let (port_a, port_b) = loopback_pair();
    let mut a = TestNode::boot(pair_config(ConfiguredRole::Primary), port_a);
    let mut b = TestNode::boot(pair_config(ConfiguredRole::Secondary), port_b);

    a.port.set_partitioned(true);
    for _ in 0..5 {
        step_pair(&mut a, &mut b);
    }

    // Three unconfirmed pings close the window each silent exchange: the
    // first two misses confirm death, the rest keep the +1000 counter —
    // silence dominates the indicator immediately.
    assert_eq!(b.liveness.penalty(), 3000);
    assert!(b.liveness.is_dead());
    assert_eq!(b.chart_state(), SyncState::DeSync);
    assert_eq!(b.chart.reason(), DeSyncReason::PeerDeath);
    assert_eq!(b.verdict, Some(AdmissionVerdict::Secondary));
    // The partitioned owner sees the same silence and lands in deSYNC too.
    assert_eq!(a.chart_state(), SyncState::DeSync);
    assert_eq!(a.chart.reason(), DeSyncReason::PeerDeath);
}

#[test]
fn pair_link_when_primary_dies_and_revives_after_promotion_then_rejoins_as_secondary() {
    // The zombie rule end to end: the Primary dies; the survivor is
    // promoted (modeled at the verdict/policy layer — the fencing slice
    // owns the mechanism); the revived ex-Primary discovers the live
    // Primary and rejoins as the Secondary, never re-entering as Primary.
    let (mut a, mut b) = converged_pair();
    let epoch_before_death = a.epoch;
    assert!(epoch_before_death.raw() > 0);

    a.port.set_partitioned(true);
    for _ in 0..5 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(b.chart_state(), SyncState::DeSync);
    assert_eq!(b.chart.reason(), DeSyncReason::PeerDeath);

    b.promote_to_primary();
    a.port.set_partitioned(false);
    a.resurrect();
    a.crossload = CrossloadReadiness::Complete;
    b.crossload = CrossloadReadiness::Complete;
    for _ in 0..8 {
        step_pair(&mut a, &mut b);
    }

    // The zombie fence: a Primary-configured unit admitted as Secondary.
    assert_eq!(a.config.role, ConfiguredRole::Primary);
    assert_eq!(a.verdict, Some(AdmissionVerdict::Secondary));
    // Monitor mode: the rejoined unit holds no permit even at SYNC_READY.
    assert_eq!(a.chart_state(), SyncState::SyncReady);
    assert_eq!(a.host_rounds(), 0);
    assert_eq!(a.run_refused_code(), Some("V4018"));
    // The promoted owner minted forward past the dead owner's epoch, and
    // the rejoined unit tracks it without ever leading.
    assert_eq!(b.verdict, Some(AdmissionVerdict::Primary));
    assert!(b.epoch.raw() > epoch_before_death.raw());
    assert_eq!(b.chart_state(), SyncState::SyncReady);
    assert!(a.epoch.raw() > 0);
    assert!(a.epoch.raw() <= b.epoch.raw());
}

#[test]
fn pair_link_when_peer_reboots_with_fresh_epoch_then_restart_drops_survivor_to_de_sync() {
    // Anti-stale only: the survivor committed the pair's epoch; a peer
    // that reboots lost its epoch memory — its per-channel PING sequence
    // regresses against the exchange's observed high-water (T13), and
    // the survivor reads that as an epoch discontinuity: it drops to
    // deSYNC, its own epoch never moves backwards, and it re-enters via
    // SYNCING. Once the rebooted owner's minted epochs move the pair
    // forward past the committed value, both units re-converge.
    let (mut a, mut b) = converged_pair();
    let committed = b.epoch;
    assert!(committed.raw() >= 3);

    a.resurrect();
    a.crossload = CrossloadReadiness::Complete;
    b.crossload = CrossloadReadiness::Complete;
    // The first step delivers the pre-reboot in-flight frame (a valid
    // continuation of the old counter); the rebooted unit's fresh PING
    // base arrives on the next frame.
    step_pair(&mut a, &mut b);
    step_pair(&mut a, &mut b);

    // The restarted sequence is the discontinuity: the chart dropped to
    // deSYNC (the recorded reason — only set on deSYNC entry) and has
    // already re-entered via SYNCING in the same step.
    assert!(matches!(
        b.chart_state(),
        SyncState::DeSync | SyncState::Syncing
    ));
    assert_eq!(b.chart.reason(), DeSyncReason::EpochDiscontinuity);
    // The restarted peer's presentations never moved the survivor's epoch
    // backwards (the pre-reboot in-flight frame may still advance it).
    assert!(b.epoch >= committed);

    for _ in 0..8 {
        step_pair(&mut a, &mut b);
    }

    assert_eq!(a.chart_state(), SyncState::SyncReady);
    assert_eq!(b.chart_state(), SyncState::SyncReady);
    // The owner's commits carried the epoch past the committed value and
    // the survivor caught up to it.
    assert!(a.epoch.raw() > committed.raw());
    assert!(b.epoch.raw() > committed.raw());
}

#[test]
fn pair_link_when_foreign_pair_answers_then_admission_refused_with_v4101() {
    // A live controller on the pair link that belongs to a different
    // pair is not a neighbor: admission refuses with the HA surface's
    // V-code and neither unit grants itself a permit.
    let (port_a, port_c) = loopback_pair();
    let mut a = TestNode::boot(pair_config(ConfiguredRole::Primary), port_a);
    let mut c = TestNode::boot(
        RedundancyConfig::pair(PairId::new(99), ConfiguredRole::Primary)
            .with_confirmation_exchanges(1)
            .with_missed_exchanges(2),
        port_c,
    );

    for _ in 0..4 {
        step_pair(&mut a, &mut c);
    }

    assert_eq!(a.refusal, Some(AdmissionRefusal::ForeignPairOnLink));
    assert_eq!(a.verdict, None);
    assert_eq!(a.run_refused_code(), Some("V4018"));
    assert_eq!(a.host_rounds(), 0);
    assert_eq!(c.refusal, Some(AdmissionRefusal::ForeignPairOnLink));
    assert_eq!(c.host_rounds(), 0);
    assert_eq!(c.run_refused_code(), Some("V4018"));
}

#[test]
fn pair_link_when_admission_pending_then_permit_held_and_granted_on_verdict() {
    // The shell decides admission before the application starts: scans
    // refuse while discovery runs and execute once the verdict grants;
    // the Secondary's permit stays withheld.
    let (port_a, port_b) = loopback_pair();
    let mut a = TestNode::boot(pair_config(ConfiguredRole::Primary), port_a);
    let mut b = TestNode::boot(pair_config(ConfiguredRole::Secondary), port_b);

    step_pair(&mut a, &mut b);
    assert_eq!(a.verdict, None);
    assert_eq!(a.host_rounds(), 0);
    assert_eq!(a.run_refused_code(), Some("V4018"));

    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }

    assert_eq!(a.verdict, Some(AdmissionVerdict::Primary));
    assert!(a.host_rounds() > 0);
    assert_eq!(b.verdict, Some(AdmissionVerdict::Secondary));
    assert_eq!(b.host_rounds(), 0);
    assert_eq!(b.run_refused_code(), Some("V4018"));
}
