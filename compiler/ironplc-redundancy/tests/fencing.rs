//! Two-instance loopback + simulator-registry scenarios for the CONTROL
//! chart and the OWNERSHIP_BARRIER.
//!
//! Both units of one pair run their per-unit composition over the
//! loopback simulator binding (pair link) and the module registry (the
//! fenced I/O network): ping/pong liveness, the SYNC chart, the CONTROL
//! chart, epoch and OwnerLease minting through a real [`RuntimeHost`]
//! scan-commit callback, and the fencing client driving the barrier. The
//! node is test support — it composes the crate's modules the way the
//! redundancy shell will, and the real shell replaces it when a binary
//! embeds the layer.
//!
//! Admission is fixed from the configured role (the Slice-3 precedent):
//! the Primary-configured unit boot-claims through the guard table's boot
//! barrier (its no-peer admission), the Secondary-configured unit stays
//! in monitor mode; discovery and the zombie fence are the pair-link
//! suite's territory. Crossload readiness is pre-set — replication is
//! instantaneous here; the crossload pipeline is Slice 3's tested
//! territory. Takeover during Testing stays Slice 3's
//! `takeover_testing` (ADR-0064(e)): the fencing slice composes only the
//! permit seam around promotion, never the candidate machinery, so no
//! candidate scenarios repeat here.
//!
//! Timing model: `now` is an abstract step counter both nodes and the
//! registry share. The liveness exchange counts exchanges (Slice 2); the
//! OwnerLease TTL ([`RedundancyConfig::lease_ttl`]) and the `I` signal's
//! outputs-change window ([`CHANGE_WINDOW`]) count ticks; the registry
//! ages an owner's connections out after [`CONNECTION_TIMEOUT`] ticks
//! without commits (the target's old-connection timeout, the failover
//! formula's `T_old-connection-timeout` term). A unit dies by going
//! silent: no frames, no commits — its lease stops renewing, its outputs
//! freeze, and the target eventually ages its connections out.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use ironplc_container::{ContainerBuilder, FunctionId};
use ironplc_redundancy::{
    ClaimBarrier, ConfiguredRole, ControlAlarm, ControlChart, ControlEvent, ControlState,
    DeSyncReason, DetectionAction, Epoch, Liveness, LivenessEvent, LoopbackPort, ModuleId,
    ModuleOwnership, ModuleRegistry, ModuleState, NicPort, OwnerId, OwnerLease, Packet, PairId,
    PairRole,     RedundancyConfig, RegistryClient, SyncChart, SyncEvent, SyncState, claim_barrier, detect,
    loopback_pair, ownership_barrier, release_all, FencingClient,
};
use ironplc_runtime::{RuntimeError, RuntimeHost};

/// How long an armed module's outputs count as "changing" after the last
/// commit — the `I` signal's freshness window (ticks).
const CHANGE_WINDOW: u64 = 1;

/// The registry's connection timeout: an armed owner silent this many
/// ticks loses its connections at the target (ticks).
const CONNECTION_TIMEOUT: u64 = 2;

/// One unit of a pair under test: the per-unit composition of link port,
/// liveness exchange, SYNC chart, CONTROL chart, epoch, OwnerLease, the
/// runtime host, and the fencing client.
struct Node {
    config: RedundancyConfig,
    port: LoopbackPort,
    client: RegistryClient,
    liveness: Liveness,
    sync: SyncChart,
    control: ControlChart,
    epoch: Epoch,
    lease: Option<OwnerLease>,
    host: RuntimeHost,
    owner: OwnerId,
    /// The modules the application requires, in the fixed claim order.
    required: Vec<ModuleId>,
    /// The modules this unit currently holds (armed once the barrier
    /// passed; empty again after a release or a failed barrier).
    owned: Vec<ModuleId>,
    /// Whether the host holds the execution permit.
    permitted: bool,
    /// Whether the process is alive: a dead unit transmits no frames and
    /// drives no scans (a crash, not a partition).
    alive: bool,
    /// Primary-configured units boot-claim once, from their no-peer
    /// admission (the guard table's boot barrier).
    boot_pending: bool,
    /// Whether the exchange's confirmed peer death has been handled
    /// (the takeover gate evaluated, or the SYNC chart dropped).
    death_handled: bool,
    /// Whether this unit's Input-Only view of the I/O chain is
    /// reachable: a partition that cuts the daisy-chain on this unit's
    /// side kills the peer's ping/pong AND its input data while the
    /// target still sees a live owner.
    input_path_ok: bool,
    /// The peer's epoch high-water: the newest epoch ever presented on
    /// the wire (the promotion bump must dominate it).
    peer_epoch_latest: Epoch,
    /// The registry clock tick of the last valid peer frame — the start
    /// of the peer's OwnerLease authority window.
    peer_last_seen_at: u64,
    /// Whether the peer epoch was agreed while synchronizing (the SYNC
    /// chart's replication guard; replication itself is instant here).
    epoch_agreed: bool,
}

/// What one unit observes about its peer this step.
#[derive(Clone, Copy)]
struct PeerView {
    live: bool,
    owner: OwnerId,
}

/// A host over an empty program: the fencing scenarios exercise the
/// seams, not the application.
fn shell_host() -> RuntimeHost {
    let container = ContainerBuilder::new()
        .add_function(FunctionId::INIT, &[], 0, 0, 0)
        .max_call_depth(1)
        .build();
    RuntimeHost::new(container).unwrap()
}

impl Node {
    /// Boots one admitted unit over its link port and the shared
    /// registry: deSYNC with the boot reason, no ownership, no permit.
    fn boot(
        config: RedundancyConfig,
        port: LoopbackPort,
        client: RegistryClient,
        owner: OwnerId,
    ) -> Self {
        let required = [ModuleId::new(0), ModuleId::new(1)].to_vec();
        Self {
            liveness: Liveness::new(&config),
            port,
            client,
            config,
            sync: SyncChart::new(),
            control: ControlChart::new(),
            epoch: Epoch::new(0),
            lease: None,
            host: shell_host(),
            owner,
            required,
            owned: Vec::new(),
            permitted: false,
            alive: true,
            boot_pending: false,
            death_handled: false,
            input_path_ok: true,
            peer_epoch_latest: Epoch::new(0),
            peer_last_seen_at: 0,
            epoch_agreed: false,
        }
    }

    /// Boots the Primary-configured unit: its admission verdict is the
    /// initial owner, so the guard table's boot barrier runs on the
    /// first advance.
    fn boot_primary(
        config: RedundancyConfig,
        port: LoopbackPort,
        client: RegistryClient,
        owner: OwnerId,
    ) -> Self {
        let mut node = Self::boot(config, port, client, owner);
        node.boot_pending = true;
        node
    }

    fn pair_id(&self) -> PairId {
        self.config.pair_id.unwrap()
    }

    fn host_rounds(&self) -> u64 {
        self.host.status().rounds
    }

    /// Grants the execution permit: the promotion/boot policy call
    /// admission owns (Slice 1).
    fn grant_permit(&mut self) {
        self.permitted = true;
        self.host.permit_execution();
    }

    /// The pair role this unit presents on the wire: the ownership truth
    /// is the permit (only an output-controlling unit presents Primary).
    fn pair_role(&self) -> PairRole {
        if self.permitted {
            PairRole::Primary
        } else {
            PairRole::Secondary
        }
    }

    fn generation(&self) -> u32 {
        self.host.status().application.raw()
    }

    /// Drains the inbound link and feeds every valid peer frame to the
    /// exchange and the epoch.
    fn receive(&mut self, now: u64) {
        while let Some((_, frame)) = self.port.poll() {
            let Some(packet) = Packet::decode(&frame) else {
                continue;
            };
            if packet.pair_id != self.pair_id() {
                continue;
            }
            self.peer_last_seen_at = now;
            self.peer_epoch_latest = self.peer_epoch_latest.max(packet.epoch);
            if let Some(LivenessEvent::PeerRestarted) = self.liveness.note_received(&packet) {
                // A restarted peer lost its epoch memory: epoch
                // discontinuity, drop to deSYNC.
                self.sync.apply(SyncEvent::EpochDiscontinuity);
                self.epoch_agreed = false;
            } else {
                self.epoch.adopt(packet.epoch);
                if self.sync.state() == SyncState::Syncing {
                    self.epoch_agreed = true;
                }
            }
        }
    }

    /// Advances one ping/pong exchange and sends the outbound frame.
    fn transmit(&mut self) {
        if !self.alive {
            return;
        }
        let (ping_seq, pong_seq, _) = self.liveness.begin_exchange();
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

    /// The CONTROL claim: the promotion mint bumps the epoch first (the
    /// barrier runs and the outputs are armed under the fresh epoch),
    /// then the ordered all-or-nothing barrier; on pass the lease is
    /// minted and the permit granted, on failure everything acquired was
    /// already released by the barrier helper and the coded alarm names
    /// the cause.
    fn attempt_claim(&mut self, now: u64) {
        self.epoch = self.epoch.next();
        self.control.apply(ControlEvent::ClaimGuarded);
        match ownership_barrier(&mut self.client, self.owner, &self.required, self.epoch) {
            Ok(()) => {
                self.control.apply(ControlEvent::BarrierPassed);
                self.owned = self.required.clone();
                self.lease = Some(OwnerLease::mint(self.epoch, now, self.config.lease_ttl));
                self.grant_permit();
            }
            Err(error) => {
                self.control.apply(ControlEvent::BarrierFailed);
                self.control.raise_alarm(ControlAlarm::from(error));
                self.owned.clear();
            }
        }
    }

    /// The ACTIVE unit's fencing observation: compare the held set
    /// against the registry truth. Partial loss degrades; loss grown to
    /// full drops everything and stops executing.
    fn observe_fencing_loss(&mut self) {
        let owners = self.client.owners();
        let held = self
            .owned
            .iter()
            .filter(|module| {
                owners.iter().any(|entry: &ModuleOwnership| {
                    entry.module == **module && entry.state.owner() == Some(self.owner)
                })
            })
            .count();
        if held == 0 && !self.owned.is_empty() {
            release_all(&mut self.client, self.owner, &self.owned);
            self.owned.clear();
            self.control.apply(ControlEvent::IoLossFull);
            self.host.revoke_execution_permit();
            self.permitted = false;
        } else if held < self.owned.len() {
            self.control.apply(ControlEvent::IoLossPartial);
        }
    }

    /// The confirmed-death handling: the detection case table decides the
    /// action, the guard table decides permission. The SYNC chart's
    /// SYNC_READY is the takeover-eligibility latch: it is evaluated
    /// before the death is fed to the chart, because a confirmed-silent
    /// peer with frozen outputs and lapsed authority is exactly the
    /// promotion candidacy row — the claim itself is still fenced by the
    /// target's exclusivity.
    fn handle_peer_death(&mut self, now: u64, peer: PeerView, registry: &mut ModuleRegistry) {
        let io_evidence =
            self.input_path_ok && registry.outputs_changing(peer.owner, CHANGE_WINDOW);
        let self_ready = self.sync.state() == SyncState::SyncReady;
        match detect(false, io_evidence, self_ready) {
            DetectionAction::DegradedChannel => {
                self.control.raise_alarm(ControlAlarm::DegradedChannel);
                self.sync.apply(SyncEvent::PeerDied);
                self.death_handled = true;
            }
            DetectionAction::PromotionCandidate => {
                let authority_expired =
                    now.saturating_sub(self.peer_last_seen_at) >= self.config.lease_ttl;
                if authority_expired {
                    self.death_handled = true;
                    self.attempt_claim(now);
                }
                // else: silence is not proof yet — the OwnerLease
                // authority window must close first; keep evaluating.
            }
            DetectionAction::NotReady => {
                self.sync.apply(SyncEvent::PeerDied);
                self.death_handled = true;
            }
            DetectionAction::None => {}
        }
    }

    /// One step of the per-unit driver: SYNC chart guards, fencing
    /// observation, death handling, and — when permitted — one scan
    /// round whose committed boundary mints the epoch and the OwnerLease
    /// and commits the armed outputs at the registry.
    fn advance(&mut self, now: u64, peer: PeerView, registry: &mut ModuleRegistry) {
        if !self.alive {
            return;
        }
        if self.boot_pending {
            self.boot_pending = false;
            self.attempt_claim(now);
        }
        if self.sync.state() == SyncState::DeSync && peer.live {
            self.sync.apply(SyncEvent::Paired);
        }
        if self.sync.state() == SyncState::Syncing && self.epoch_agreed {
            self.sync.apply(SyncEvent::ReplicationComplete);
        }
        if matches!(
            self.control.state(),
            ControlState::Active | ControlState::ActiveDegraded
        ) {
            self.observe_fencing_loss();
        }
        if self.liveness.is_dead() && !self.death_handled {
            match self.control.state() {
                ControlState::Idle => self.handle_peer_death(now, peer, registry),
                // ACTIVE realtime isolation: the owner keeps executing;
                // only the SYNC chart records the dead replication
                // pipeline.
                _ => {
                    self.sync.apply(SyncEvent::PeerDied);
                    self.death_handled = true;
                }
            }
        }
        if self.permitted {
            let mut epoch = self.epoch;
            self.host.run_with_commit(1, || now, |commit| {
                epoch = epoch.next();
                let _ = commit;
            }).unwrap();
            self.epoch = epoch;
            self.lease = Some(OwnerLease::mint(self.epoch, now, self.config.lease_ttl));
            registry.commit_outputs(self.owner);
        }
    }
}

/// What one unit presents to its peer.
fn view(node: &Node) -> PeerView {
    PeerView {
        live: !node.liveness.is_dead(),
        owner: node.owner,
    }
}

/// Steps both units with their receive phases together (symmetric
/// exchanges), then advances each against the peer's pre-advance view.
fn step(now: u64, a: &mut Node, b: &mut Node, registry: &mut ModuleRegistry) {
    registry.tick(now);
    a.receive(now);
    b.receive(now);
    a.transmit();
    b.transmit();
    let view_b = view(b);
    a.advance(now, view_b, registry);
    let view_a = view(a);
    b.advance(now, view_a, registry);
}

fn pair_config(role: ConfiguredRole) -> RedundancyConfig {
    RedundancyConfig::pair(PairId::new(1), role)
        .with_confirmation_exchanges(2)
        .with_missed_exchanges(2)
        .with_lease_ttl(4)
}

/// Boots the pair and converges: the Primary boot-claims and drives
/// scans, the Secondary replicates and reaches SYNC_READY in monitor
/// mode. Returns the converged units at step 8.
fn converged_pair() -> (Node, Node, ModuleRegistry) {
    let mut registry = ModuleRegistry::new(2).with_connection_timeout(CONNECTION_TIMEOUT);
    let (port_a, port_b) = loopback_pair();
    let mut a = Node::boot_primary(
        pair_config(ConfiguredRole::Primary),
        port_a,
        registry.client(),
        OwnerId::new(1),
    );
    let mut b = Node::boot(
        pair_config(ConfiguredRole::Secondary),
        port_b,
        registry.client(),
        OwnerId::new(2),
    );
    for now in 1..=8 {
        step(now, &mut a, &mut b, &mut registry);
    }
    (a, b, registry)
}

/// The commanded role swap (promotion case a): refused unless the pair is
/// in SYNC_READY; the old owner releases everything and stops executing,
/// the new Primary runs the barrier and — on pass — takes the epoch, the
/// lease, and the permit.
fn swap_pair(now: u64, a: &mut Node, b: &mut Node) -> Result<(), ControlAlarm> {
    if a.sync.state() != SyncState::SyncReady || b.sync.state() != SyncState::SyncReady {
        b.control.raise_alarm(ControlAlarm::SwapRefused);
        return Err(ControlAlarm::SwapRefused);
    }
    // The releasing side: drop all ownership, stop executing, and
    // re-establish readiness as the new Secondary.
    release_all(&mut a.client, a.owner, &a.owned);
    a.owned.clear();
    a.control.apply(ControlEvent::ReleaseOwner);
    a.host.revoke_execution_permit();
    a.permitted = false;
    a.sync.apply(SyncEvent::SyncLoss);
    // The claiming side.
    b.attempt_claim(now);
    Ok(())
}

#[test]
fn fencing_when_pair_boots_then_primary_boot_claims_and_secondary_stays_idle() {
    let (a, b, registry) = converged_pair();

    assert_eq!(a.control.state(), ControlState::Active);
    assert_eq!(a.sync.state(), SyncState::SyncReady);
    assert!(a.permitted);
    assert!(a.host_rounds() > 0);
    assert_eq!(a.lease.unwrap().epoch(), a.epoch);
    assert!(!a.lease.unwrap().is_expired(8));
    for entry in registry.owners() {
        assert!(
            matches!(entry.state, ModuleState::Armed { owner, .. } if owner == a.owner),
            "the boot owner must hold every module armed"
        );
    }
    // The guard table: the Secondary never claims outside the two
    // promotion cases — P is observed, so the detection table says none.
    assert_eq!(b.control.state(), ControlState::Idle);
    assert_eq!(b.sync.state(), SyncState::SyncReady);
    assert!(!b.permitted);
    assert_eq!(b.host_rounds(), 0);
    assert_eq!(b.lease, None);
    assert!(b.owned.is_empty());
    assert!(registry
        .owners()
        .iter()
        .all(|entry| entry.state.owner() != Some(b.owner)));
    assert!(b.epoch > Epoch::new(0));
    assert!(b.epoch <= a.epoch);
}

#[test]
fn fencing_when_swap_commanded_at_sync_ready_then_ownership_exchanges() {
    let (mut a, mut b, mut registry) = converged_pair();
    let epoch_b_before = b.epoch;
    let rounds_before = a.host_rounds();
    let owner_a = a.owner;
    let now = 9;
    registry.tick(now);

    swap_pair(now, &mut a, &mut b).unwrap();

    // RELEASE: the old owner owns nothing, is idle, and has stopped.
    assert!(registry
        .owners()
        .iter()
        .all(|entry| entry.state.owner() != Some(owner_a)));
    assert_eq!(a.control.state(), ControlState::Idle);
    assert!(matches!(a.host.run(1, || now), Err(RuntimeError::NotPermitted)));
    assert!(!a.permitted);
    // CLAIM → CLAIMED_DISARMED → BARRIER → ARM: the new Primary holds
    // every module, armed under the promotion epoch.
    for entry in registry.owners() {
        assert_eq!(
            entry.state,
            ModuleState::Armed {
                owner: b.owner,
                epoch: b.epoch
            }
        );
    }
    // EPOCH: the promotion bumped the pair epoch past the old owner's
    // last presented epoch, and the lease carries it.
    assert!(b.epoch > epoch_b_before);
    assert!(b.epoch > b.peer_epoch_latest);
    assert_eq!(b.lease.unwrap().epoch(), b.epoch);
    // PERMIT: the new Primary executes.
    assert!(b.permitted);
    // RE-SYNC: the old Primary rejoins as the new Secondary.
    let b_rounds_after_swap = b.host_rounds();
    for i in 1..=6 {
        step(now + i, &mut a, &mut b, &mut registry);
    }
    assert_eq!(a.sync.state(), SyncState::SyncReady);
    assert_eq!(a.control.state(), ControlState::Idle);
    assert!(!a.permitted);
    assert_eq!(a.host_rounds(), rounds_before);
    assert!(b.host_rounds() > b_rounds_after_swap);
    assert!(a.epoch <= b.epoch);
    assert!(registry
        .owners()
        .iter()
        .all(|entry| entry.state.owner() != Some(owner_a)));
}

#[test]
fn fencing_when_primary_proven_dead_then_survivor_takes_over_via_lease_expiry() {
    let (mut a, mut b, mut registry) = converged_pair();
    let epoch_b_before = b.epoch;
    assert_eq!(b.control.state(), ControlState::Idle);
    assert_eq!(b.sync.state(), SyncState::SyncReady);
    assert_eq!(b.lease, None);

    // The Primary dies: the link goes silent and no scan commits renew
    // the OwnerLease or move the outputs.
    a.port.set_partitioned(true);
    a.alive = false;
    for now in 9..=14 {
        step(now, &mut a, &mut b, &mut registry);
    }

    // Proven death: peer-detection and lease-expiry agree (the takeover
    // gate only fired after both), the outputs froze (no I/O evidence),
    // and the target aged the dead owner's connections out — the
    // surviving claimant acquired and armed everything.
    assert!(b.liveness.is_dead());
    assert_eq!(b.control.state(), ControlState::Active);
    assert!(b.permitted);
    assert!(b.host_rounds() > 0);
    assert!(b.epoch > epoch_b_before);
    assert!(b.epoch > b.peer_epoch_latest);
    assert_eq!(b.lease.unwrap().epoch(), b.epoch);
    assert!(!b.lease.unwrap().is_expired(14));
    // Armed under the promotion epoch — the stamp the takeover claimed
    // in (after adopting the owner's last in-flight frame), while the
    // owner's mints move on.
    for entry in registry.owners() {
        assert!(
            matches!(entry.state, ModuleState::Armed { owner, epoch }
                if owner == b.owner && epoch > epoch_b_before && epoch <= b.epoch),
            "the takeover must arm every module under the promotion epoch"
        );
    }
}

#[test]
fn fencing_when_partition_hides_live_primary_then_claim_rejected_and_redundancy_lost() {
    let (mut a, mut b, mut registry) = converged_pair();
    let owner_a = a.owner;
    // A stays alive — scanning, owning, committing outputs — but the
    // partition silences both pair-link channels AND this unit's
    // Input-Only view of the I/O chain (a mid-chain cut): from here P=0
    // and I=0, while the target still sees A's live ownership.
    a.port.set_partitioned(true);
    b.input_path_ok = false;
    for now in 9..=14 {
        step(now, &mut a, &mut b, &mut registry);
    }

    // The candidacy row fired once the authority window closed, and the
    // target's exclusivity decided: a live owner holds the modules.
    assert!(b.liveness.is_dead());
    assert_eq!(b.control.state(), ControlState::RedundancyLost);
    assert_eq!(b.control.alarm(), Some(ControlAlarm::OwnerConflict));
    assert_eq!(b.control.alarm().unwrap().v_code(), "V4106");
    assert!(!b.permitted);
    // Fail-closed, zero partial ownership: the claimant holds nothing…
    assert!(registry
        .owners()
        .iter()
        .all(|entry| entry.state.owner() != Some(b.owner)));
    // …and the live Primary is untouched: still ACTIVE, still owning,
    // still executing.
    assert!(registry
        .owners()
        .iter()
        .all(|entry| matches!(entry.state, ModuleState::Armed { owner, .. } if owner == owner_a)));
    assert_eq!(a.control.state(), ControlState::Active);
    assert!(a.permitted);
}

#[test]
fn fencing_when_silent_peer_still_owns_io_then_degraded_channel_and_never_claim() {
    let (mut a, mut b, mut registry) = converged_pair();
    let owner_b = b.owner;
    // The peer stays alive and owning; only the pair link is cut: P=0
    // but I=1 — the degraded-channel row, never a claim.
    a.port.set_partitioned(true);
    for now in 9..=14 {
        step(now, &mut a, &mut b, &mut registry);
    }

    assert!(b.liveness.is_dead());
    assert_eq!(b.control.state(), ControlState::Idle);
    assert_eq!(b.control.alarm(), Some(ControlAlarm::DegradedChannel));
    assert_eq!(b.control.alarm().unwrap().v_code(), "V4109");
    assert_eq!(b.sync.state(), SyncState::DeSync);
    assert_eq!(b.sync.reason(), DeSyncReason::PeerDeath);
    // Never claim: the peer demonstrably owns the I/O.
    assert!(registry
        .owners()
        .iter()
        .all(|entry| entry.state.owner() != Some(owner_b)));
    assert!(registry
        .owners()
        .iter()
        .all(|entry| matches!(entry.state, ModuleState::Armed { owner, .. } if owner == a.owner)));
}

#[test]
fn fencing_when_partial_io_loss_while_peer_live_then_degraded_owner_and_no_promotion() {
    let (mut a, mut b, mut registry) = converged_pair();
    let rounds_before = a.host_rounds();
    // P=1 (the peer is live on the link); one module faults: partial
    // fencing loss within policy degrades the owner, which keeps
    // executing; the observing peer never claims while P=1.
    registry.yank(ModuleId::new(1));
    for now in 9..=11 {
        step(now, &mut a, &mut b, &mut registry);
    }

    assert_eq!(a.control.state(), ControlState::ActiveDegraded);
    assert!(a.permitted);
    assert!(a.host_rounds() > rounds_before);
    assert!(registry
        .owners()
        .iter()
        .all(|entry| entry.state.owner() != Some(b.owner)));

    // The loss grows to full: the owner drops everything and lands
    // terminal — redundancy asks a human.
    registry.yank(ModuleId::new(0));
    for now in 12..=13 {
        step(now, &mut a, &mut b, &mut registry);
    }

    assert_eq!(a.control.state(), ControlState::RedundancyLost);
    assert_eq!(
        a.control.alarm(),
        Some(ControlAlarm::OwnershipBarrierFailed)
    );
    assert!(!a.permitted);
    assert!(matches!(a.host.run(1, || 13), Err(RuntimeError::NotPermitted)));
}

#[test]
fn fencing_when_takeover_meets_foreign_owner_then_safe_retreat_without_partial_ownership() {
    let (mut a, mut b, mut registry) = converged_pair();
    // A foreign originator grabs one module after the target aged the
    // dead owner's connections out (the step-11 tick) but before the
    // survivor's claim — a misconfigured exclusive owner that the
    // ordered claim meets mid-promotion. The claim acquires the free
    // module, then conflicts, and the barrier rollback releases
    // everything acquired.
    let foreign = OwnerId::new(99);
    a.port.set_partitioned(true);
    a.alive = false;
    for now in 9..=14 {
        if now == 12 {
            registry
                .client()
                .claim(ModuleId::new(1), foreign, Epoch::new(1))
                .unwrap();
        }
        step(now, &mut a, &mut b, &mut registry);
    }

    assert_eq!(b.control.state(), ControlState::RedundancyLost);
    assert_eq!(b.control.alarm(), Some(ControlAlarm::OwnerConflict));
    // Safe retreat mid-promotion: the acquired module was released, the
    // conflicting owner is untouched, the claimant holds nothing.
    assert_eq!(registry.owners()[0].state, ModuleState::Unowned);
    assert_eq!(
        registry.owners()[1].state,
        ModuleState::ClaimedDisarmed {
            owner: foreign,
            epoch: Epoch::new(1)
        }
    );
    assert!(!b.permitted);
}

#[test]
fn fencing_when_swap_commanded_before_sync_ready_then_refused_with_v4108() {
    let mut registry = ModuleRegistry::new(2).with_connection_timeout(CONNECTION_TIMEOUT);
    let (port_a, port_b) = loopback_pair();
    let mut a = Node::boot_primary(
        pair_config(ConfiguredRole::Primary),
        port_a,
        registry.client(),
        OwnerId::new(1),
    );
    let mut b = Node::boot(
        pair_config(ConfiguredRole::Secondary),
        port_b,
        registry.client(),
        OwnerId::new(2),
    );
    // One step: A boot-claims (ACTIVE); B has only just paired — still
    // SYNCING, so the pair is not in SYNC_READY.
    step(1, &mut a, &mut b, &mut registry);
    assert_eq!(a.control.state(), ControlState::Active);
    assert_eq!(b.sync.state(), SyncState::Syncing);

    let refusal = swap_pair(1, &mut a, &mut b).unwrap_err();

    assert_eq!(refusal, ControlAlarm::SwapRefused);
    assert_eq!(b.control.alarm(), Some(ControlAlarm::SwapRefused));
    assert_eq!(b.control.alarm().unwrap().v_code(), "V4108");
    // Nothing moved: the owner still owns and executes, the Secondary
    // never claimed, the swap is never half-run.
    assert_eq!(a.control.state(), ControlState::Active);
    assert!(a.permitted);
    assert!(registry
        .owners()
        .iter()
        .all(|entry| matches!(entry.state, ModuleState::Armed { owner, .. } if owner == a.owner)));
    assert!(registry
        .owners()
        .iter()
        .all(|entry| entry.state.owner() != Some(b.owner)));
    assert_eq!(b.control.state(), ControlState::Idle);
}

#[test]
fn fencing_when_guard_table_evaluated_then_only_the_two_promotion_cases() {
    // The node drives the guard table at its trigger points; this pins
    // the composition the scenarios rely on: the boot claim is the
    // Primary-configured admission path, and the takeover gate refuses
    // every not-quite-proven-death shape.
    assert_eq!(
        claim_barrier(
            ConfiguredRole::Primary,
            SyncState::DeSync,
            false,
            false,
            false,
            false,
            true
        ),
        Some(ClaimBarrier::Boot)
    );
    assert_eq!(
        claim_barrier(
            ConfiguredRole::Secondary,
            SyncState::SyncReady,
            false,
            false,
            true,
            false,
            false
        ),
        Some(ClaimBarrier::Takeover)
    );
    // Alive peer: no barrier, no matter the rest.
    assert_eq!(
        claim_barrier(
            ConfiguredRole::Secondary,
            SyncState::SyncReady,
            true,
            false,
            true,
            false,
            false
        ),
        None
    );
}
