//! The redundancy shell of one served process: the HA state the
//! engineering command surface reads and the composition root drives.
//!
//! Per the HA redundancy layer architecture ("Shell, Not Runtime +1")
//! the redundancy crate is the process's outer shell: its composition
//! root composes it, and the shell owns startup ordering and the policy
//! of *when* the application may execute. This module is that shell in
//! its first composable form: it holds the unit's SYNC/CONTROL charts,
//! the calibration engine, the pair liveness exchange over the loopback
//! binding, the fencing client, the epoch, and the timestamped event
//! ring the `haEvents` query renders — everything
//! [`crate::commands`] serializes.
//!
//! Two compositions exist today (`specs/design/ha-engineering-ui.md`,
//! the client mapping of `ironplcvm serve`):
//!
//! - **Standalone** ([`Shell::standalone`]): no pair is configured, the
//!   statechart does not run, and every query answers the honest
//!   standalone shape (nothing measured, nothing to render). Engineer
//!   actions refuse with V4112 rather than acknowledging a no-op.
//! - **Simulated peer** ([`Shell::simulated`]): the dev/demo binding the
//!   architecture mandates as a first-class deliverable — the local
//!   unit plus one simulated peer over the [`crate::loopback`] binding
//!   and the [`crate::simulator`] registry, so every HA state is
//!   exercisable end-to-end before any field device exists. The shell
//!   is the single-threaded driver: [`Shell::tick`] advances one pair
//!   exchange, drives the charts from the observations, and stamps the
//!   registry cadence — and the serve session's command cadence is the
//!   simulation clock.
//!
//! The shell never names the runtime host: the *verdict* is the shell's
//! ([`Shell::local_verdict`], the permit policy), the *latch* is the
//! host's — the composition root applies one to the other, exactly the
//! policy/mechanism split of the architecture. The scan-commit seam
//! ([`Shell::on_scan_commit`]) is where the host reports committed
//! boundaries in; the epoch mint, the lease renewal, and the
//! output-commit stamping answer out.
//!
//! What is deliberately not here (later slices): the real pair-link
//! driver over two processes, the crossload pipeline of the live sync,
//! and the promotion/takeover path — the standby never self-promotes in
//! this slice, and the liveness exchange's peer-death input lands the
//! charts in deSYNC, nowhere else.

use ironplc_runtime::ScanCommit;

use crate::admission::{admit, AdmissionVerdict, Discovery};
use crate::calibration::{
    BudgetVerdict, Calibration, CalibrationState, CalibrationStatus, Direction, TakeoverBudget,
};
use crate::config::{ConfiguredRole, RedundancyConfig};
use crate::epoch::Epoch;
use crate::fencing::{
    ownership_barrier, release_all, FencingClient, ModuleId, ModuleOwnership, OwnerId,
    OwnershipMode,
};
use crate::hal::NicPort;
use crate::lease::OwnerLease;
use crate::liveness::{Liveness, LivenessEvent, Packet};
use crate::loopback::loopback_pair;
use crate::simulator::{ModuleRegistry, RegistryClient};
use crate::statechart::{
    claim_barrier, ClaimBarrier, ControlAlarm, ControlEvent, ControlState, SyncEvent, SyncState,
};

mod unit;
mod views;

pub use unit::Side;
use unit::{apply_loss, Unit};
pub use views::{HaEvent, HaEventKind, IoReadyView, Refusal, SwapOutcome, UnitView};

/// The demo binding's ControllerIds: the permanent owner identities the
/// two simulated units present to the fencing target. A real binding
/// carries these from the controller's identity configuration; the demo
/// binding assigns them, exactly like its pair identity.
const DEMO_OWNER_LOCAL: u64 = 1;
const DEMO_OWNER_PEER: u64 = 2;

/// The recovery budget (ticks) the demo binding composes before the
/// engineer configures one: an open parameter placeholder of the same
/// class as `RedundancyConfig`'s timing placeholders. `haSetTimingBudget`
/// replaces it; the commissioning calibration validates against it.
const DEFAULT_RECOVERY_BUDGET_TICKS: u64 = 100;

/// The event ring capacity (ha-engineering-ui.md, `haEvents`: a bounded
/// ring, read on poll). The oldest entries drop once the ring is full.
pub(crate) const EVENT_RING_CAPACITY: usize = 64;

/// The redundancy shell of one served process — see the module
/// documentation.
pub struct Shell {
    config: RedundancyConfig,
    local: Unit,
    peer: Option<Unit>,
    registry: ModuleRegistry,
    client: RegistryClient,
    required: Vec<ModuleId>,
    calibration: Calibration,
    /// The engineer-configured peer-failure confirmation time (ticks).
    peer_failure_confirmation: u64,
    events: Vec<HaEvent>,
    event_count: u64,
    now: u64,
    application_generation: u32,
    state_generation: u64,
    last_commit_tick: Option<u64>,
}

impl Shell {
    /// The standalone shell: no pair configured, the statechart does
    /// not run, and queries answer the honest standalone shape.
    pub fn standalone() -> Self {
        let config = RedundancyConfig::standalone();
        let registry = ModuleRegistry::new(0);
        let (port, _unused) = loopback_pair();
        drop(_unused);
        let local = Unit::new(&config, config.role, OwnerId::new(DEMO_OWNER_LOCAL), port);
        Self::with_units(config, registry, Vec::new(), local, None)
    }

    /// The simulated-peer shell of the demo binding: the local unit of
    /// `config`'s pair plus one peer over the loopback binding and the
    /// simulator registry. The composition root supplies the registry
    /// (the demo device properties) and the required I/O set.
    pub fn simulated(
        config: RedundancyConfig,
        registry: ModuleRegistry,
        required: Vec<ModuleId>,
    ) -> Self {
        let peer_role = match config.role {
            ConfiguredRole::Primary => ConfiguredRole::Secondary,
            ConfiguredRole::Secondary => ConfiguredRole::Primary,
        };
        let (local_port, peer_port) = loopback_pair();
        let local = Unit::new(
            &config,
            config.role,
            OwnerId::new(DEMO_OWNER_LOCAL),
            local_port,
        );
        let peer = Unit::new(&config, peer_role, OwnerId::new(DEMO_OWNER_PEER), peer_port);
        Self::with_units(config, registry, required, local, Some(peer))
    }

    /// The shared construction over the two units.
    fn with_units(
        config: RedundancyConfig,
        registry: ModuleRegistry,
        required: Vec<ModuleId>,
        local: Unit,
        peer: Option<Unit>,
    ) -> Self {
        let client = registry.client();
        let peer_failure_confirmation = u64::from(config.confirmation_exchanges);
        Self {
            config,
            local,
            peer,
            registry,
            client,
            required,
            calibration: Calibration::new(DEFAULT_RECOVERY_BUDGET_TICKS),
            peer_failure_confirmation,
            events: Vec::new(),
            event_count: 0,
            now: 0,
            application_generation: 0,
            state_generation: 0,
            last_commit_tick: None,
        }
    }

    /// Whether a pair is configured: standalone shells answer every
    /// query with the honest standalone shape and refuse actions.
    pub const fn has_pair(&self) -> bool {
        self.config.pair_id.is_some()
    }

    /// The shell's static configuration (the pair identity when one is
    /// configured).
    pub const fn config(&self) -> &RedundancyConfig {
        &self.config
    }

    /// Notes the served host's application generation: what the
    /// outbound packets carry and the `configsMatch` input compares.
    /// The composition root wires it at boot; [`Self::on_scan_commit`]
    /// keeps it current.
    pub fn note_application_generation(&mut self, generation: u32) {
        self.application_generation = generation;
    }

    /// Brings the shell up: admission, sync, the boot claim, and the
    /// commissioning calibration. Returns the verdict the composition
    /// root applies to the host's execution permit.
    pub fn start_up(&mut self) -> AdmissionVerdict {
        if !self.has_pair() {
            return AdmissionVerdict::Standalone;
        }
        // Admission (the admission authority's verdict table): the demo
        // discovery finds the peer presenting the Secondary role.
        let Ok(verdict) = admit(self.config.role, Discovery::LiveSecondary) else {
            // Foreign traffic never appears in the simulated
            // composition; fail closed without the permit.
            return AdmissionVerdict::Secondary;
        };
        // The pair is connected by construction: both charts rise
        // through SYNCING to SYNC_READY (the loopback applies
        // replicated state in one step). The replication delivery is
        // what the sync pipeline produces: each unit holds the peer's
        // application generation from here, and the live exchange keeps
        // it fresh on the wire.
        self.local.sync.apply(SyncEvent::Paired);
        self.local.sync.apply(SyncEvent::ReplicationComplete);
        self.local.observed_generation = Some(self.application_generation);
        if let Some(peer) = self.peer.as_mut() {
            peer.sync.apply(SyncEvent::Paired);
            peer.sync.apply(SyncEvent::ReplicationComplete);
            peer.observed_generation = Some(self.application_generation);
        }
        // The admitted Primary claims outputs at boot through the guard
        // table's boot barrier (a live Secondary never blocks it).
        let claiming = match verdict {
            AdmissionVerdict::Primary => Side::Local,
            AdmissionVerdict::Standalone | AdmissionVerdict::Secondary => Side::Peer,
        };
        let Some(unit) = self.unit(claiming) else {
            return AdmissionVerdict::Secondary;
        };
        if claim_barrier(
            unit.role,
            unit.sync.state(),
            false,
            false,
            true,
            false,
            true,
        )
        .is_none()
        {
            return AdmissionVerdict::Secondary;
        }
        let _ = self.claim_active(claiming);
        // Commissioning calibration (ADR-0062): the readiness chain
        // UNQUALIFIED → CALIBRATING → CALIBRATED, gating TakeoverReady.
        let _ = self.run_calibration();
        verdict
    }

    /// Advances the simulated pair one tick: one ping/pong exchange on
    /// both sides, the SYNC charts from the observations, the fencing
    /// observation of the ACTIVE units, and the registry cadence. The
    /// serve session calls this once per command line in simulated
    /// mode — the command cadence is the simulation clock.
    pub fn tick(&mut self) {
        if !self.has_pair() {
            return;
        }
        self.now += 1;
        let now = self.now;
        let before = (
            self.calibration.degraded(),
            self.calibration.guarantee_lost(),
        );

        // 1. The pair link: receive every pending frame, then transmit
        //    the next exchange on both sides.
        self.exchange(now);

        // 2. Replication completes on the first healthy tick after a
        //    re-pair.
        if self.local.replication_pending && !self.local.liveness.is_dead() {
            self.local.sync.apply(SyncEvent::ReplicationComplete);
            self.local.replication_pending = false;
        }
        if let Some(peer) = self.peer.as_mut() {
            if peer.replication_pending && !peer.liveness.is_dead() {
                peer.sync.apply(SyncEvent::ReplicationComplete);
                peer.replication_pending = false;
            }
        }

        // 3. The fencing observation of the ACTIVE units: partial loss
        //    degrades, full loss ends in REDUNDANCY_LOST.
        self.observe_fencing();

        // 4. The registry cadence; the ACTIVE units' outputs commit —
        //    the `I` signal evidence.
        self.registry.tick(now);
        if self.local.control.state().is_output_controlling() {
            self.registry.commit_outputs(self.local.owner);
        }
        if let Some(peer) = self.peer.as_ref() {
            if peer.control.state().is_output_controlling() {
                self.registry.commit_outputs(peer.owner);
            }
        }

        self.watch_alarms(before);
    }

    /// The verdict the permit policy applies to the local unit right
    /// now: a standalone unit executes; a paired unit executes exactly
    /// while it is output-controlling (ACTIVE / ACTIVE_DEGRADED).
    pub fn local_verdict(&self) -> AdmissionVerdict {
        if !self.has_pair() {
            return AdmissionVerdict::Standalone;
        }
        if self.local.control.state().is_output_controlling() {
            AdmissionVerdict::Primary
        } else {
            AdmissionVerdict::Secondary
        }
    }

    /// The read-only view of one unit's chart state.
    pub fn unit_view(&self, side: Side) -> Option<UnitView> {
        let unit = match side {
            Side::Local => Some(&self.local),
            Side::Peer => self.peer.as_ref(),
        }?;
        Some(UnitView {
            role: unit.role,
            owner: unit.owner,
            sync: unit.sync.state(),
            sync_reason: unit.sync.reason(),
            control: unit.control.state(),
            alarm: unit.control.alarm(),
            epoch: unit.epoch,
        })
    }

    /// The current epoch of the local unit (the pair's minted epoch).
    pub const fn epoch(&self) -> Epoch {
        self.local.epoch
    }

    /// The application generation the host last committed.
    pub const fn application_generation(&self) -> u32 {
        self.application_generation
    }

    /// The committed state generation (the host's boundary counter).
    pub const fn state_generation(&self) -> u64 {
        self.state_generation
    }

    /// The engineer-configured peer-failure confirmation time (ticks).
    pub const fn peer_failure_confirmation(&self) -> u64 {
        self.peer_failure_confirmation
    }

    /// The engineer-configured maximum process-recovery budget (ticks).
    pub const fn configured_budget(&self) -> u64 {
        self.calibration.configured_budget()
    }

    /// Whether one module is powered and communicating (the barrier
    /// view's fault state; a faulted module owns nothing).
    pub fn module_online(&self, module: ModuleId) -> bool {
        self.registry.is_online(module)
    }

    /// The calibration/budget status snapshot with the live TakeoverReady
    /// inputs (the local SYNC state and the IO_READY breakdown).
    pub fn calibration_status(&self) -> CalibrationStatus {
        self.calibration.status(
            self.local.sync.state() == SyncState::SyncReady,
            self.io_ready().all(),
        )
    }

    /// The `IO_READY` breakdown (ADR-0062): required inputs observable,
    /// standby connections valid, configs match, epochs valid — the
    /// failing item identified when false.
    pub fn io_ready(&self) -> IoReadyView {
        if !self.has_pair() {
            return IoReadyView {
                required_inputs: false,
                standby_connections: false,
                configs_match: false,
                epochs_valid: false,
                failing: Some("standalone"),
            };
        }
        let required_inputs = self.required.iter().all(|m| self.registry.is_online(*m));
        let standby_connections = self
            .unit(Side::Peer)
            .is_some_and(|peer| !peer.liveness.is_dead());
        let configs_match = self
            .unit(Side::Local)
            .and_then(|local| local.observed_generation)
            .is_some_and(|observed| observed == self.application_generation);
        let epochs_valid = self
            .unit(Side::Peer)
            .is_some_and(|peer| peer.epoch == self.local.epoch);
        let failing = if !required_inputs {
            Some("requiredInputs")
        } else if !standby_connections {
            Some("standbyConnections")
        } else if !configs_match {
            Some("configsMatch")
        } else if !epochs_valid {
            Some("epochs")
        } else {
            None
        };
        IoReadyView {
            required_inputs,
            standby_connections,
            configs_match,
            epochs_valid,
            failing,
        }
    }

    /// The ownership truth of every registered module (the barrier view
    /// renders the required ones).
    pub fn ownership(&self) -> Vec<ModuleOwnership> {
        self.registry.owners()
    }

    /// The required I/O modules, in the fixed configured claim order.
    pub fn required_modules(&self) -> &[ModuleId] {
        &self.required
    }

    /// The binding's ownership mode: the per-module profile name of the
    /// barrier view (a protocol swap changes this descriptor, never the
    /// FSM).
    pub fn ownership_mode(&self) -> OwnershipMode {
        self.client.capabilities().ownership_mode
    }

    /// The phase-aware safe-point input: ticks since the host last
    /// committed a scan boundary (0 before the first commit).
    pub fn scan_phase(&self) -> u64 {
        self.last_commit_tick.map_or(0, |last| self.now - last)
    }

    /// The predicted-if-now recovery estimate (ADR-0062's online
    /// estimator), or `None` while the pair is not calibrated.
    pub fn predicted_if_now(&self) -> Option<u64> {
        self.calibration.predicted_if_now(self.scan_phase())
    }

    /// The active alarm flags of the pair (haStatus): the two
    /// timing-health alarms from the calibration engine and
    /// REDUNDANCY_LOST from either CONTROL chart.
    pub fn alarm_flags(&self) -> (bool, bool, bool) {
        let redundancy_lost = self.local.control.state() == ControlState::RedundancyLost
            || self
                .peer
                .as_ref()
                .is_some_and(|peer| peer.control.state() == ControlState::RedundancyLost);
        (
            self.calibration.degraded(),
            self.calibration.guarantee_lost(),
            redundancy_lost,
        )
    }

    /// The bounded event ring, oldest first, and the total number of
    /// events ever recorded (clients dedup polls by the count).
    pub fn events(&self) -> (u64, &[HaEvent]) {
        (self.event_count, &self.events)
    }

    /// The engineer's commanded swap (haCommandedSwap): refused outside
    /// SYNC_READY with V4108's guard semantics, otherwise performed
    /// synchronously — the release, the claim, and the configured-role
    /// exchange all complete before the acknowledgment renders.
    pub fn commanded_swap(&mut self) -> Result<SwapOutcome, Refusal> {
        if !self.has_pair() {
            self.record(
                HaEventKind::SwapRefused,
                Some(Refusal::Standalone.to_string()),
            );
            return Err(Refusal::Standalone);
        }
        // The claiming side is the Secondary-configured unit (the guard
        // table's commanded-swap barrier); the releasing side is the
        // Active unit.
        let claiming = match (self.local.role, self.peer.as_ref().map(|p| p.role)) {
            (ConfiguredRole::Secondary, _) => Side::Local,
            (ConfiguredRole::Primary, Some(ConfiguredRole::Secondary)) => Side::Peer,
            // The peer is configured in the constructor as the role
            // opposite to the local unit; any other shape is a
            // composition defect, refused like any other illegal swap.
            _ => {
                self.record(
                    HaEventKind::SwapRefused,
                    Some("the pair holds no Secondary-configured unit".to_string()),
                );
                return Err(Refusal::NotSyncReady);
            }
        };
        let releasing = claiming.other();
        let guards = self.unit(claiming).is_some_and(|unit| {
            claim_barrier(
                unit.role,
                unit.sync.state(),
                false,
                false,
                false,
                true,
                false,
            ) == Some(ClaimBarrier::CommandedSwap)
        }) && self
            .unit(releasing)
            .is_some_and(|unit| unit.control.state() == ControlState::Active)
            && !self.local.liveness.is_dead()
            && self.peer.as_ref().is_some_and(|p| !p.liveness.is_dead());
        if !guards {
            self.record(
                HaEventKind::SwapRefused,
                Some(Refusal::NotSyncReady.to_string()),
            );
            // The refusal is latched as the alarm on the refusing unit
            // (the V4108 contract).
            self.local.control.raise_alarm(ControlAlarm::SwapRefused);
            return Err(Refusal::NotSyncReady);
        }

        self.record(HaEventKind::SwapCommanded, None);
        // The controlled handoff (ADR-0062: zero old-owner timeout via
        // an explicit RELEASE_OWNER): the releasing side drops every
        // module, then the claiming side proves the barrier.
        let releasing_owner = self
            .unit(releasing)
            .map_or(OwnerId::new(0), |unit| unit.owner);
        release_all(&mut self.client, releasing_owner, &self.required);
        if let Some(unit) = self.unit_mut(releasing) {
            unit.control.apply(ControlEvent::ReleaseOwner);
            unit.lease = None;
        }
        match self.claim_active(claiming) {
            Ok(()) => {
                // A commanded role swap exchanges the pair's configured
                // roles (the config vocabulary): both units flip. The
                // swap is a coordinated transaction: the claiming unit's
                // epoch bump propagates to the releasing unit at
                // completion, so the pair is aligned the moment the
                // acknowledgment renders.
                self.local.role = match self.local.role {
                    ConfiguredRole::Primary => ConfiguredRole::Secondary,
                    ConfiguredRole::Secondary => ConfiguredRole::Primary,
                };
                if let Some(peer) = self.peer.as_mut() {
                    peer.role = match peer.role {
                        ConfiguredRole::Primary => ConfiguredRole::Secondary,
                        ConfiguredRole::Secondary => ConfiguredRole::Primary,
                    };
                }
                self.config.role = self.local.role;
                let new_primary = self.unit(claiming).map_or(0, |unit| unit.owner.raw());
                self.record(
                    HaEventKind::SwapCompleted,
                    Some(format!("unit {new_primary} now owns the outputs")),
                );
                Ok(SwapOutcome::Completed)
            }
            Err(_) => Ok(SwapOutcome::BarrierFailed),
        }
    }

    /// Runs a commissioning/recalibration run (haRunCalibration):
    /// synchronous over the demo binding, so the state it acknowledges
    /// (CALIBRATED or the honestly failed verdict) has changed before
    /// the response renders.
    pub fn run_calibration(&mut self) -> Result<(), Refusal> {
        if !self.has_pair() {
            return Err(Refusal::Standalone);
        }
        let before = (
            self.calibration.degraded(),
            self.calibration.guarantee_lost(),
        );
        let was_calibrated = self.calibration.state() == CalibrationState::Calibrated;
        self.record(HaEventKind::CalibrationBegan, None);
        let budget = self.calibration.configured_budget();
        self.calibration =
            crate::run_calibration(&self.config, budget, &self.registry, &self.required);
        if was_calibrated {
            // The run constructs a fresh commissioning engine; a
            // recalibration run carries the baseline counter forward.
            self.calibration.note_recalibration();
        }
        self.record(
            HaEventKind::CalibrationCompleted,
            Some(format!("state {}", self.calibration.state().as_str())),
        );
        self.watch_alarms(before);
        Ok(())
    }

    /// Sets the engineer's timing parameters (haSetTimingBudget): the
    /// peer-failure confirmation time and the maximum process-recovery
    /// budget. A budget the live qualification bounds cannot honor is
    /// reported, never silently applied (ADR-0062): the configured
    /// values stand unchanged and the would-be verdict is returned.
    pub fn set_timing_budget(
        &mut self,
        peer_failure_confirmation: u64,
        recovery_budget: u64,
    ) -> Result<BudgetVerdict, Refusal> {
        if !self.has_pair() {
            return Err(Refusal::Standalone);
        }
        let status = self.calibration_status();
        let candidate = TakeoverBudget::calculate(
            recovery_budget,
            status.claim_start(),
            status
                .modules()
                .iter()
                .fold(0u64, |sum, module| sum.saturating_add(module.claim().max())),
            status
                .modules()
                .iter()
                .map(|module| module.arm().max())
                .max()
                .unwrap_or(0),
            status.scan().max(),
            status
                .modules()
                .iter()
                .map(|module| module.output_apply().max())
                .max()
                .unwrap_or(0),
        );
        if candidate.verdict() != BudgetVerdict::Qualified {
            return Ok(candidate.verdict());
        }
        self.calibration.set_budget(recovery_budget);
        self.peer_failure_confirmation = peer_failure_confirmation;
        // The confirmation time is the supervision protocol parameter:
        // the exchange windows rebuild from it (the miss counting
        // restarts — harmless next to a parameter change).
        self.config = self.config.clone().with_confirmation_exchanges(
            u32::try_from(peer_failure_confirmation).unwrap_or(u32::MAX),
        );
        self.local.liveness = Liveness::new(&self.config);
        if let Some(peer) = self.peer.as_mut() {
            peer.liveness = Liveness::new(&self.config);
        }
        self.record(
            HaEventKind::BudgetUpdated,
            Some(format!("recovery budget {recovery_budget} ticks")),
        );
        Ok(candidate.verdict())
    }

    /// The scan-commit seam (the runtime's seam 2): the host reports a
    /// committed boundary; the shell stamps the epoch mint, renews the
    /// owner's lease, feeds the scan term, and notes the generations.
    pub fn on_scan_commit(&mut self, commit: ScanCommit) {
        self.state_generation = commit.rounds;
        self.application_generation = commit.application.raw();
        let phase_base = self.last_commit_tick;
        self.last_commit_tick = Some(self.now);
        if !self.has_pair() {
            return;
        }
        let before = (
            self.calibration.degraded(),
            self.calibration.guarantee_lost(),
        );
        // Only the HA supervisor mints, and only at scan commit
        // (ADR-0062): one epoch per committed round.
        self.local.epoch = self.local.epoch.next();
        if let Some(peer) = self.peer.as_mut() {
            let minted = self.local.epoch;
            let _ = peer.epoch.adopt(minted);
        }
        // The scan safe-point term: the interval between committed
        // boundaries, measured like every other term.
        if let Some(last) = phase_base {
            self.calibration.record_scan(self.now.saturating_sub(last));
        }
        self.watch_alarms(before);
    }

    /// Cuts or restores the local end of the pair link — the simulator
    /// binding's partition control (the loopback port's own model), so
    /// tests and the demo can exercise deSYNC and timeout detection.
    pub fn set_link_partitioned(&mut self, partitioned: bool) {
        self.local.port.set_partitioned(partitioned);
    }

    /// Faults a module offline — the simulator binding's fault control,
    /// the fencing verification failure path.
    pub fn fault_module(&mut self, module: ModuleId) {
        self.registry.yank(module);
    }

    /// The unit of one side, if it exists (the peer never exists
    /// standalone).
    fn unit(&self, side: Side) -> Option<&Unit> {
        match side {
            Side::Local => Some(&self.local),
            Side::Peer => self.peer.as_ref(),
        }
    }

    fn unit_mut(&mut self, side: Side) -> Option<&mut Unit> {
        match side {
            Side::Local => Some(&mut self.local),
            Side::Peer => self.peer.as_mut(),
        }
    }

    /// One pair-link exchange: both sides receive every pending frame,
    /// then both transmit (the calibration run's step ordering).
    fn exchange(&mut self, now: u64) {
        let Some(pair_id) = self.config.pair_id else {
            return;
        };
        let mut local_frames = Vec::new();
        while let Some((_, frame)) = self.local.port.poll() {
            local_frames.push(frame);
        }
        let mut peer_frames = Vec::new();
        if let Some(peer) = self.peer.as_mut() {
            while let Some((_, frame)) = peer.port.poll() {
                peer_frames.push(frame);
            }
        }
        for frame in &local_frames {
            let Some(packet) = Packet::decode(frame) else {
                continue;
            };
            if packet.pair_id != pair_id {
                continue;
            }
            self.local
                .observe(&packet, now, &mut self.calibration, Direction::AToB);
        }
        if let Some(peer) = self.peer.as_mut() {
            for frame in &peer_frames {
                let Some(packet) = Packet::decode(frame) else {
                    continue;
                };
                if packet.pair_id != pair_id {
                    continue;
                }
                peer.observe(&packet, now, &mut self.calibration, Direction::BToA);
            }
        }
        if let Some(event) = self.local.transmit(
            now,
            pair_id,
            self.application_generation,
            &mut self.calibration,
            Direction::AToB,
        ) {
            debug_assert!(event == LivenessEvent::PeerDied);
            self.local.was_dead = true;
            self.local.sync.apply(SyncEvent::PeerDied);
            let last = self.local.last_confirm_tick;
            self.record(
                HaEventKind::TimeoutDetected,
                Some(format!("last owner packet at tick {last}")),
            );
        }
        if let Some(peer) = self.peer.as_mut() {
            if let Some(event) = peer.transmit(
                now,
                pair_id,
                self.application_generation,
                &mut self.calibration,
                Direction::BToA,
            ) {
                debug_assert!(event == LivenessEvent::PeerDied);
                peer.was_dead = true;
                peer.sync.apply(SyncEvent::PeerDied);
            }
        }
    }

    /// The fencing observation of the ACTIVE units: a required module
    /// no longer held by its owner is partial loss within policy
    /// (ACTIVE_DEGRADED) or, once everything is gone, full loss
    /// (REDUNDANCY_LOST).
    fn observe_fencing(&mut self) {
        if self.peer.is_none() {
            return;
        }
        let owners = self.registry.owners();
        let held_count = |owner: OwnerId| {
            self.required
                .iter()
                .filter(|module| {
                    owners
                        .iter()
                        .any(|entry| entry.module == **module && entry.state.owner() == Some(owner))
                })
                .count()
        };
        let local_held = held_count(self.local.owner);
        apply_loss(&mut self.local, self.required.len(), local_held);
        if let Some(peer) = self.peer.as_mut() {
            let peer_held = held_count(peer.owner);
            apply_loss(peer, self.required.len(), peer_held);
        }
    }

    /// Drives one unit from `Idle` through the OWNERSHIP_BARRIER to
    /// `Active`, or to `REDUNDANCY_LOST` with the safe retreat on
    /// failure. The caller has verified the guard.
    fn claim_active(&mut self, side: Side) -> Result<(), crate::fencing::FencingError> {
        let lease_ttl = self.config.lease_ttl;
        let Some(unit) = self.unit_mut(side) else {
            return Err(crate::fencing::FencingError::Unavailable);
        };
        unit.control.apply(ControlEvent::ClaimGuarded);
        unit.epoch = unit.epoch.next();
        let owner = unit.owner;
        let epoch = unit.epoch;
        let now = self.now;
        match ownership_barrier(&mut self.client, owner, &self.required, epoch) {
            Ok(()) => {
                if let Some(unit) = self.unit_mut(side) {
                    unit.control.apply(ControlEvent::BarrierPassed);
                    unit.lease = Some(OwnerLease::mint(epoch, now, lease_ttl));
                }
                // The claim is pair-visible: the peer observes the new
                // ownership epoch as of the barrier passing, so the
                // pair converges at completion (the acknowledgment
                // renders pair-aligned state).
                if let Some(claiming_epoch) = self.unit(side).map(|unit| unit.epoch) {
                    if let Some(other) = self.unit_mut(side.other()) {
                        let _ = other.epoch.adopt(claiming_epoch);
                    }
                }
                self.record(
                    HaEventKind::ForwardOpenReceived,
                    Some(format!(
                        "unit {} claimed {} modules in epoch {}",
                        owner.raw(),
                        self.required.len(),
                        epoch.raw()
                    )),
                );
                self.record(
                    HaEventKind::ArmReceived,
                    Some(format!("unit {} armed every module", owner.raw())),
                );
                self.record(
                    HaEventKind::OwnerAccepted,
                    Some(format!("unit {} owns all required outputs", owner.raw())),
                );
                Ok(())
            }
            Err(error) => {
                // The barrier helper already released everything
                // acquired: the retreat is the safe command, and the
                // released modules are its application.
                self.record(
                    HaEventKind::SafeCommanded,
                    Some(format!(
                        "barrier failed with {}: releasing everything acquired",
                        error.v_code()
                    )),
                );
                if let Some(unit) = self.unit_mut(side) {
                    unit.control.apply(ControlEvent::BarrierFailed);
                    unit.lease = None;
                }
                self.record(
                    HaEventKind::SafeApplied,
                    Some(
                        "all acquired modules released; the pair is in REDUNDANCY_LOST".to_string(),
                    ),
                );
                Err(error)
            }
        }
    }

    /// Records a rising timing-health alarm as an event (the flags
    /// themselves live on the calibration engine).
    fn watch_alarms(&mut self, before: (bool, bool)) {
        if self.calibration.degraded() && !before.0 {
            self.record(
                HaEventKind::PerformanceDegraded,
                Some("reality left the calibrated envelope (V4110)".to_string()),
            );
        }
        if self.calibration.guarantee_lost() && !before.1 {
            self.record(
                HaEventKind::GuaranteeLost,
                Some("the recovery budget can no longer be met (V4111)".to_string()),
            );
        }
    }

    /// Appends one event to the bounded ring.
    fn record(&mut self, kind: HaEventKind, detail: Option<String>) {
        self.events.push(HaEvent {
            tick: self.now,
            kind,
            detail,
        });
        self.event_count += 1;
        while self.events.len() > EVENT_RING_CAPACITY {
            self.events.remove(0);
        }
    }
}
