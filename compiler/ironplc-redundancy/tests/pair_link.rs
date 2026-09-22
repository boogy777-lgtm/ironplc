//! Two-instance loopback scenarios for the production pair link
//! ([`PairLink`]) and the SYNC chart.
//!
//! Two units of one pair (or a foreign pair intruding) run their full
//! per-unit composition — the pair link over the loopback simulator
//! binding beside a real [`RuntimeHost`] — the same composition
//! `ironplcvm serve` runs in pair mode, minus the session. The test
//! driver owns the hosts and drives the owners' scan rounds through the
//! scan-commit seam (the serve session drives them through commands).
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
    loopback_pair, permit_for, AdmissionRefusal, AdmissionVerdict, ConfiguredRole, DeSyncReason,
    Epoch, LoopbackPort, PairId, PairLink, RedundancyConfig, SyncState,
};
use ironplc_runtime::{RuntimeError, RuntimeHost};

/// One unit under test: the production pair link over the loopback
/// binding beside its runtime host — the per-unit composition the serve
/// process runs, minus the session.
struct Unit {
    link: PairLink<LoopbackPort>,
    host: RuntimeHost,
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

impl Unit {
    /// Boots one pair unit: deSYNC with the boot reason, an undecided
    /// admission, and an unpermitted host.
    fn boot(config: RedundancyConfig, port: LoopbackPort) -> Self {
        Self {
            link: PairLink::new(config, port),
            host: shell_host(),
        }
    }

    /// Repeats the boot lifecycle on the same link: the unit restarted
    /// and lost its epoch memory (the zombie and stale-epoch scenarios
    /// resurrect a unit mid-scenario).
    fn resurrect(&mut self) {
        self.link.restart();
        self.host = shell_host();
    }

    fn chart_state(&self) -> SyncState {
        self.link.chart().state()
    }

    fn reason(&self) -> DeSyncReason {
        self.link.chart().reason()
    }

    fn epoch(&self) -> Epoch {
        self.link.epoch()
    }

    /// The host's rounds, proving whether the permit was granted and
    /// scans executed.
    fn host_rounds(&self) -> u64 {
        self.host.status().rounds
    }

    /// Driving scans without the permit: the refusal the unadmitted unit
    /// keeps answering (V4018).
    fn run_refused_code(&mut self) -> Option<&'static str> {
        let error = self.host.run(1, || 0).unwrap_err();
        assert!(matches!(error, RuntimeError::NotPermitted));
        error.v_code()
    }
}

/// Reconciles the host's permit with the link's verdict and drives one
/// owner round whose committed boundary mints the next epoch (the
/// scan-commit seam) — exactly what the serve session does after each
/// pair tick and command.
fn drive_owner_round(unit: &mut Unit) {
    if let Some(verdict) = unit.link.local_verdict() {
        permit_for(&mut unit.host, verdict);
        if verdict.permits_execution() {
            unit.host
                .run_with_commit(1, || 0, |commit| unit.link.on_scan_commit(commit))
                .unwrap();
        }
    }
}

/// Steps both units with their receive phases together, so each round
/// both see the previous round's frames — symmetric exchanges — then
/// drives the owners' rounds.
fn step_pair(a: &mut Unit, b: &mut Unit) {
    a.link.tick(&mut a.host);
    b.link.tick(&mut b.host);
    drive_owner_round(a);
    drive_owner_round(b);
}

fn pair_config(role: ConfiguredRole) -> RedundancyConfig {
    // Window 2 covers the two-exchange answer pipeline (a PING is answered
    // by the peer's next frame and read the round after that); the missed
    // threshold of 2 confirms death quickly in scenarios.
    RedundancyConfig::pair(PairId::new(1), role)
        .with_confirmation_exchanges(2)
        .with_missed_exchanges(2)
}

/// Boots an admitted, converged pair: both units at SYNC_READY through
/// the real boot replication (the owner's sync stream), the owner
/// permitted and executing, the standby in monitor mode.
fn converged_pair() -> (Unit, Unit) {
    let (port_a, port_b) = loopback_pair();
    let mut a = Unit::boot(pair_config(ConfiguredRole::Primary), port_a);
    let mut b = Unit::boot(pair_config(ConfiguredRole::Secondary), port_b);
    for _ in 0..8 {
        step_pair(&mut a, &mut b);
    }
    (a, b)
}

#[test]
fn pair_link_when_primary_and_secondary_boot_then_both_converge_to_sync_ready() {
    // A/Primary and B/Secondary discover each other, admit (the owner is
    // the initial owner; the Secondary defers to the live Primary), sync
    // through the owner's replication stream and the epochs its scan
    // commits mint, and reach SYNC_READY — the Secondary in monitor mode:
    // never permitted, never executing.
    let (a, b) = converged_pair();

    assert_eq!(a.link.local_verdict(), Some(AdmissionVerdict::Primary));
    assert_eq!(b.link.local_verdict(), Some(AdmissionVerdict::Secondary));
    assert_eq!(a.chart_state(), SyncState::SyncReady);
    assert_eq!(b.chart_state(), SyncState::SyncReady);
    // The verdict lands on the third exchange and drives a round in the
    // same step; every committed round minted exactly one epoch through
    // the scan-commit callback.
    assert_eq!(a.host_rounds(), 6);
    assert_eq!(a.epoch().raw(), a.host_rounds() as u32);
    // The peer tracks the owner but is never ahead of it (anti-stale).
    assert!(b.epoch().raw() > 0);
    assert!(b.epoch().raw() < a.epoch().raw());
    assert_eq!(b.host_rounds(), 0);
}

#[test]
fn pair_link_when_exchanges_healthy_then_penalty_adds_one_per_exchange() {
    let (port_a, port_b) = loopback_pair();
    let mut a = Unit::boot(pair_config(ConfiguredRole::Primary), port_a);
    let mut b = Unit::boot(pair_config(ConfiguredRole::Secondary), port_b);

    for _ in 0..6 {
        step_pair(&mut a, &mut b);
    }

    // The pump polls the peer's datagram as soon as it arrives (the
    // production interleaving: each unit's receive sees the peer's
    // same-step frame), so six ticks credit five successful exchanges at
    // +1 each — and none is missing.
    assert_eq!(a.link.liveness().penalty(), 5);
    assert_eq!(b.link.liveness().penalty(), 5);
    assert_eq!(a.link.liveness().missed(), 0);
    assert_eq!(b.link.liveness().missed(), 0);
}

#[test]
fn pair_link_when_peer_silent_then_penalty_adds_thousand_and_survivor_enters_de_sync() {
    // The +1000 step dominates the indicator immediately, and the
    // survivor's SYNC chart drops to deSYNC on confirmed peer death —
    // silence is a state input, never a promotion (the survivor here
    // never paired, so it promotes no one).
    let (port_a, port_b) = loopback_pair();
    let mut a = Unit::boot(pair_config(ConfiguredRole::Primary), port_a);
    let mut b = Unit::boot(pair_config(ConfiguredRole::Secondary), port_b);

    a.link.port_mut().set_partitioned(true);
    for _ in 0..5 {
        step_pair(&mut a, &mut b);
    }

    // Three unconfirmed pings close the window each silent exchange: the
    // first two misses confirm death, the rest keep the +1000 counter —
    // silence dominates the indicator immediately.
    assert_eq!(b.link.liveness().penalty(), 3000);
    assert!(b.link.liveness().is_dead());
    assert_eq!(b.chart_state(), SyncState::DeSync);
    assert_eq!(b.reason(), DeSyncReason::PeerDeath);
    assert_eq!(b.link.local_verdict(), Some(AdmissionVerdict::Secondary));
    // The partitioned owner sees the same silence and lands in deSYNC too.
    assert_eq!(a.chart_state(), SyncState::DeSync);
    assert_eq!(a.reason(), DeSyncReason::PeerDeath);
}

#[test]
fn pair_link_when_primary_dies_and_revives_after_promotion_then_rejoins_as_secondary() {
    // The zombie rule end to end: the Primary dies; the survivor detects
    // the death and promotes itself (the takeover policy, modeled at the
    // verdict/policy layer — the fencing slice owns the mechanism); the
    // revived ex-Primary discovers the live Primary and rejoins as the
    // Secondary, never re-entering as Primary.
    let (mut a, mut b) = converged_pair();
    let epoch_before_death = a.epoch();
    assert!(epoch_before_death.raw() > 0);

    a.link.port_mut().set_partitioned(true);
    for _ in 0..5 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(b.chart_state(), SyncState::DeSync);
    assert_eq!(b.reason(), DeSyncReason::PeerDeath);
    // The survivor promoted itself on the confirmed death.
    assert_eq!(b.link.local_verdict(), Some(AdmissionVerdict::Primary));

    a.link.port_mut().set_partitioned(false);
    a.resurrect();
    for _ in 0..8 {
        step_pair(&mut a, &mut b);
    }

    // The zombie fence: a Primary-configured unit admitted as Secondary.
    assert_eq!(a.link.config().role, ConfiguredRole::Primary);
    assert_eq!(a.link.local_verdict(), Some(AdmissionVerdict::Secondary));
    // Monitor mode: the rejoined unit holds no permit even at SYNC_READY.
    assert_eq!(a.chart_state(), SyncState::SyncReady);
    assert_eq!(a.host_rounds(), 0);
    assert_eq!(a.run_refused_code(), Some("V4018"));
    // The promoted owner minted forward past the dead owner's epoch, and
    // the rejoined unit tracks it without ever leading.
    assert_eq!(b.link.local_verdict(), Some(AdmissionVerdict::Primary));
    assert!(b.epoch().raw() > epoch_before_death.raw());
    assert_eq!(b.chart_state(), SyncState::SyncReady);
    assert!(a.epoch().raw() > 0);
    assert!(a.epoch().raw() <= b.epoch().raw());
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
    let committed = b.epoch();
    assert!(committed.raw() >= 3);

    a.resurrect();
    // The pump polls the peer's datagram as soon as it arrives, so the
    // rebooted unit's fresh PING base lands in the very next tick (the
    // pre-reboot in-flight frame is consumed by the resurrect itself).
    step_pair(&mut a, &mut b);

    // The restarted sequence is the discontinuity: the chart dropped to
    // deSYNC and re-entered via SYNCING in the same tick; the recorded
    // reason (set only on deSYNC entry) names the discontinuity.
    assert!(matches!(
        b.chart_state(),
        SyncState::DeSync | SyncState::Syncing
    ));
    assert_eq!(b.reason(), DeSyncReason::EpochDiscontinuity);
    // The restarted peer's presentations never moved the survivor's epoch
    // backwards (the pre-reboot in-flight frame may still advance it).
    assert!(b.epoch() >= committed);

    for _ in 0..8 {
        step_pair(&mut a, &mut b);
    }

    assert_eq!(a.chart_state(), SyncState::SyncReady);
    assert_eq!(b.chart_state(), SyncState::SyncReady);
    // The owner's commits carried the epoch past the committed value and
    // the survivor caught up to it.
    assert!(a.epoch().raw() > committed.raw());
    assert!(b.epoch().raw() > committed.raw());
}

#[test]
fn pair_link_when_foreign_pair_answers_then_admission_refused_with_v4101() {
    // A live controller on the pair link that belongs to a different
    // pair is not a neighbor: admission refuses with the HA surface's
    // V-code and neither unit grants itself a permit.
    let (port_a, port_c) = loopback_pair();
    let mut a = Unit::boot(pair_config(ConfiguredRole::Primary), port_a);
    let mut c = Unit::boot(
        RedundancyConfig::pair(PairId::new(99), ConfiguredRole::Primary)
            .with_confirmation_exchanges(1)
            .with_missed_exchanges(2),
        port_c,
    );

    for _ in 0..4 {
        step_pair(&mut a, &mut c);
    }

    assert_eq!(a.link.refusal(), Some(AdmissionRefusal::ForeignPairOnLink));
    assert_eq!(a.link.local_verdict(), None);
    assert_eq!(a.run_refused_code(), Some("V4018"));
    assert_eq!(a.host_rounds(), 0);
    assert_eq!(c.link.refusal(), Some(AdmissionRefusal::ForeignPairOnLink));
    assert_eq!(c.host_rounds(), 0);
    assert_eq!(c.run_refused_code(), Some("V4018"));
}

#[test]
fn pair_link_when_admission_pending_then_permit_held_and_granted_on_verdict() {
    // The shell decides admission before the application starts: scans
    // refuse while discovery runs and execute once the verdict grants;
    // the Secondary's permit stays withheld.
    let (port_a, port_b) = loopback_pair();
    let mut a = Unit::boot(pair_config(ConfiguredRole::Primary), port_a);
    let mut b = Unit::boot(pair_config(ConfiguredRole::Secondary), port_b);

    step_pair(&mut a, &mut b);
    assert_eq!(a.link.local_verdict(), None);
    assert_eq!(a.host_rounds(), 0);
    assert_eq!(a.run_refused_code(), Some("V4018"));

    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }

    assert_eq!(a.link.local_verdict(), Some(AdmissionVerdict::Primary));
    assert!(a.host_rounds() > 0);
    assert_eq!(b.link.local_verdict(), Some(AdmissionVerdict::Secondary));
    assert_eq!(b.host_rounds(), 0);
    assert_eq!(b.run_refused_code(), Some("V4018"));
}
