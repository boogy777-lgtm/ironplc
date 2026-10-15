//! The calibration engine: measured timing terms, the commissioning
//! calibration state, the takeover budget, and the guarantee monitors —
//! ADR-0062's "measure → calibrate → qualify → configure a budget →
//! continuously verify reality" paradigm.
//!
//! The engine owns:
//!
//! - **The timing terms** ([`crate::timing`]): peer-detection measured
//!   from the ping/pong exchange, per-module claim / ARM / output-apply
//!   contributions, and the scan term feeding the safe-point — each as
//!   current / EMA10 / EMA100 / max / count.
//! - **The readiness chain** (ADR-0062, "Decision"): UNQUALIFIED →
//!   CALIBRATING → CALIBRATED. A secondary has no right to be
//!   takeover-ready until the pair's commissioning calibration has
//!   produced a valid link profile, so [`Calibration::begin`]
//!   invalidates any previous guarantee and the link is valid only in
//!   `Calibrated` with no guarantee-lost alarm. A significant change
//!   (NIC or medium replaced, link speed changed, topology changed,
//!   runtime or HA protocol version changed — the ADR's own list)
//!   invalidates the qualification through [`Calibration::invalidate`]
//!   and requires a new calibration.
//! - **The budget** (ADR-0062, "Timing formulas"): the claim-start is
//!   `max(T_plc-peer-detection, T_io-owner-lease-expiry)` — both
//!   independent proofs must agree before a claim may begin, so the
//!   budget's detection term is the claim-start. `T_claim` is the
//!   sequential v1 sum `sum_i(T_claim,i)`; `T_arm` and
//!   `T_output-apply` are parallel terms bounded by the slowest device
//!   (the barrier arms every module in one all-or-nothing
//!   participation). The recovery worst case is the ADR's recovery
//!   formula with the qualification bounds (`T_scan,max` for the
//!   safe-point): detection plus claim plus ARM plus the scan bound
//!   plus output apply. The budget check is the ADR's inequality —
//!   detect plus `T_claim,max` plus `T_scan,max` plus `T_output,max`
//!   against the configured recovery budget. On failure the verdict
//!   reports the minimum demonstrated budget instead of silently
//!   accepting a setting the installation cannot guarantee.
//! - **The guarantee monitors** (ADR-0062, "Continuous verification"):
//!   a degraded channel (a current sample or the EMA10 above the frozen
//!   commissioning envelope) raises `HA_PERFORMANCE_DEGRADED` (V4110);
//!   a recovery budget that can no longer be met raises
//!   `HA_TIMING_GUARANTEE_LOST` (V4111). Both are latched flags the
//!   engineering surface reads (`haStatus` renders them independently);
//!   failover thresholds are never automatically retuned, so only a new
//!   calibration clears them.
//!
//! The verdict is the calibration gate's output: `TakeoverReady =
//! SYNC_READY && IO_READY && RedundancyLinkValid` (ADR-0062). The
//! engine consumes the SYNC and IO booleans from the driver and owns
//! the link-validity input; wiring the verdict into the promotion guard
//! is the shell composition's policy, not this module's.

use std::collections::BTreeMap;

use crate::fencing::ModuleId;
use crate::problem_codes;
use crate::timing::{DirectionProfile, ModuleContribution, TermStats};

/// The commissioning calibration state — the ADR-0062 readiness chain
/// (UNQUALIFIED → CALIBRATING → CALIBRATED) that gates takeover
/// readiness.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub enum CalibrationState {
    /// No valid link profile: the pair has never completed a
    /// commissioning calibration, or a significant change invalidated
    /// the qualification.
    #[default]
    Unqualified,
    /// A calibration run is in progress; the previous guarantee (if
    /// any) is invalid until the run completes.
    Calibrating,
    /// The commissioning calibration produced a valid link profile.
    Calibrated,
}

impl CalibrationState {
    /// The wire discriminant of the engineering surface
    /// (ha-engineering-ui.md): the calibration run state in camelCase.
    pub const fn as_str(self) -> &'static str {
        match self {
            CalibrationState::Unqualified => "unqualified",
            CalibrationState::Calibrating => "calibrating",
            CalibrationState::Calibrated => "calibrated",
        }
    }
}

/// The direction of one ping/pong measurement. ADR-0062 ("Paradigm
/// change") requires calibration in both directions: scheduler
/// behavior, IRQ affinity, NIC queues, and CPU load differ per
/// direction, so a single-direction number is not evidence.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Direction {
    /// The A→B→A round trip.
    AToB,
    /// The B→A→B round trip.
    BToA,
}

/// Why the qualification was invalidated — ADR-0062's significant-change
/// list ("NIC or medium replaced, link speed changed, topology changed,
/// runtime or HA protocol version changed"), recorded for the
/// engineering surface (the Calibration tab's "last recalibration
/// trigger").
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum InvalidationReason {
    /// The NIC or the medium was replaced, or the link speed changed.
    LinkChanged,
    /// The network topology changed.
    TopologyChanged,
    /// The runtime or the HA protocol version changed.
    ProtocolVersionChanged,
}

impl InvalidationReason {
    /// The wire discriminant of the engineering surface: the last
    /// recalibration trigger in camelCase (the Calibration tab's
    /// invalidation reason).
    pub const fn as_str(self) -> &'static str {
        match self {
            InvalidationReason::LinkChanged => "linkChanged",
            InvalidationReason::TopologyChanged => "topologyChanged",
            InvalidationReason::ProtocolVersionChanged => "protocolVersionChanged",
        }
    }
}

/// The timing-health alarms of ADR-0062 ("Continuous verification"),
/// surfaced as stable V-codes on the HA command surface.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum TimingAlarm {
    /// Reality left the calibrated envelope: `HA_PERFORMANCE_DEGRADED`
    /// (V4110). Redundancy still works; the alarm surfaces the drift
    /// while it is recoverable.
    PerformanceDegraded,
    /// The recovery budget can no longer be met:
    /// `HA_TIMING_GUARANTEE_LOST` (V4111). The link profile no longer
    /// supports the configured takeover budget.
    GuaranteeLost,
}

impl TimingAlarm {
    /// The stable V-code surfacing this alarm (the crate-local CSV).
    pub const fn v_code(self) -> &'static str {
        match self {
            TimingAlarm::PerformanceDegraded => problem_codes::PERFORMANCE_DEGRADED,
            TimingAlarm::GuaranteeLost => problem_codes::TIMING_GUARANTEE_LOST,
        }
    }
}

/// The budget-check verdict (ADR-0062, "Budget validation, not silent
/// acceptance"): the engineering UI refuses to silently accept a setting
/// it cannot guarantee and reports the minimum demonstrated budget
/// instead.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum BudgetVerdict {
    /// The configured recovery budget meets the ADR's inequality
    /// `T_detect + T_claim,max + T_scan,max + T_output,max <=
    /// T_recovery-budget`.
    Qualified,
    /// The configured budget cannot be honored; carries the minimum
    /// demonstrated budget — the calculated worst-case recovery time the
    /// installation can honestly demonstrate.
    MinimumDemonstrated(u64),
}

/// The takeover budget with every term a measured number (the
/// haTimingBudget payload of the engineering UI contract): the
/// configured engineer parameter, the qualification bounds per term,
/// the calculated worst-case recovery, and the verdict.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct TakeoverBudget {
    configured_budget: u64,
    peer_detect: u64,
    claim: u64,
    arm: u64,
    scan_max: u64,
    output_apply: u64,
    calculated_worst_case: u64,
    verdict: BudgetVerdict,
}

impl TakeoverBudget {
    /// Computes the budget from the qualification bounds, exactly per
    /// ADR-0062's formulas: the worst case is `T_peer-detect + T_claim +
    /// T_arm + T_scan,max + T_output-apply` (the recovery formula with
    /// the safe-point at its `T_scan,max` bound), and the verdict is the
    /// ADR's budget inequality `T_detect + T_claim,max + T_scan,max +
    /// T_output,max <= T_recovery-budget` (the check the ADR states
    /// verbatim; the ARM term is reported in the worst case).
    pub fn calculate(
        configured_budget: u64,
        peer_detect: u64,
        claim: u64,
        arm: u64,
        scan_max: u64,
        output_apply: u64,
    ) -> Self {
        let calculated_worst_case = peer_detect
            .saturating_add(claim)
            .saturating_add(arm)
            .saturating_add(scan_max)
            .saturating_add(output_apply);
        let check = peer_detect
            .saturating_add(claim)
            .saturating_add(scan_max)
            .saturating_add(output_apply);
        let verdict = if check <= configured_budget {
            BudgetVerdict::Qualified
        } else {
            BudgetVerdict::MinimumDemonstrated(calculated_worst_case)
        };
        Self {
            configured_budget,
            peer_detect,
            claim,
            arm,
            scan_max,
            output_apply,
            calculated_worst_case,
            verdict,
        }
    }

    /// The engineer-configured maximum process-recovery budget (ticks).
    pub const fn configured_budget(&self) -> u64 {
        self.configured_budget
    }

    /// The detection term: the claim-start `max(T_plc-peer-detection,
    /// T_io-owner-lease-expiry)` — uncontrolled failover starts at both
    /// independent proofs agreeing (ADR-0062).
    pub const fn peer_detect(&self) -> u64 {
        self.peer_detect
    }

    /// The qualification bound of the sequential claim term,
    /// `sum_i(MaxQualified_claim,i)`.
    pub const fn claim(&self) -> u64 {
        self.claim
    }

    /// The ARM term: the slowest device's ARM latency (the barrier arms
    /// every module in one participation).
    pub const fn arm(&self) -> u64 {
        self.arm
    }

    /// The scan safe-point at its `T_scan,max` bound.
    pub const fn scan_max(&self) -> u64 {
        self.scan_max
    }

    /// The output-apply term: the slowest device's output latency.
    pub const fn output_apply(&self) -> u64 {
        self.output_apply
    }

    /// `T_recovery` with the qualification bounds — the calculated worst
    /// case, and the minimum demonstrated budget on a failing check.
    pub const fn calculated_worst_case(&self) -> u64 {
        self.calculated_worst_case
    }

    /// The budget-check verdict.
    pub const fn verdict(&self) -> BudgetVerdict {
        self.verdict
    }
}

/// The queryable calibration/budget status — the backend values the
/// engineering UI renders (haCalibration, haTimingBudget, haBarrier,
/// and the TakeoverReady verdict of haStatus). A point-in-time snapshot;
/// the engine's live state keeps moving.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct CalibrationStatus {
    state: CalibrationState,
    link_valid: bool,
    takeover_ready: bool,
    rtt_ab: DirectionProfile,
    rtt_ba: DirectionProfile,
    peer_detect: TermStats,
    lease_expiry: u64,
    claim_start: u64,
    scan: TermStats,
    modules: Vec<ModuleContribution>,
    limiting_device: Option<ModuleId>,
    budget: Option<TakeoverBudget>,
    /// The frozen commissioning envelope: the RTT max at completion,
    /// the degradation reference (the Calibration tab's
    /// "commissioning baseline"). `None` until the first completion
    /// and after any `begin`/`invalidate`.
    envelope_rtt_max: Option<u64>,
    degraded: bool,
    guarantee_lost: bool,
    recalibrations: u64,
    last_invalidation: Option<InvalidationReason>,
}

impl CalibrationStatus {
    /// The calibration run state (UNQUALIFIED / CALIBRATING /
    /// CALIBRATED).
    pub const fn state(&self) -> CalibrationState {
        self.state
    }

    /// Whether the link profile is valid: calibrated with no
    /// guarantee-lost alarm — the `RedundancyLinkValid` input of the
    /// TakeoverReady formula (ADR-0062).
    pub const fn link_valid(&self) -> bool {
        self.link_valid
    }

    /// The TakeoverReady verdict: `SYNC_READY && IO_READY &&
    /// RedundancyLinkValid` (ADR-0062), computed from the driver's
    /// SYNC/IO inputs and this engine's link validity.
    pub const fn takeover_ready(&self) -> bool {
        self.takeover_ready
    }

    /// The A→B→A link profile (RTT stats, jitter envelope, loss).
    pub const fn rtt_ab(&self) -> &DirectionProfile {
        &self.rtt_ab
    }

    /// The B→A→B link profile (RTT stats, jitter envelope, loss).
    pub const fn rtt_ba(&self) -> &DirectionProfile {
        &self.rtt_ba
    }

    /// The peer-detection term as measured by the ping/pong drill.
    pub const fn peer_detect(&self) -> &TermStats {
        &self.peer_detect
    }

    /// The I/O owner-lease expiry (the target's old-connection timeout
    /// or the configured lease TTL).
    pub const fn lease_expiry(&self) -> u64 {
        self.lease_expiry
    }

    /// `T_claim-start = max(T_plc-peer-detection,
    /// T_io-owner-lease-expiry)` — both independent proofs must agree.
    pub const fn claim_start(&self) -> u64 {
        self.claim_start
    }

    /// The scan term feeding the phase-aware safe-point estimate.
    pub const fn scan(&self) -> &TermStats {
        &self.scan
    }

    /// The per-module T-contributions, in module-identity order.
    pub fn modules(&self) -> &[ModuleContribution] {
        &self.modules
    }

    /// The limiting device: the module whose qualification-bound claim
    /// contribution dominates the barrier (the device the takeover
    /// estimate names, ADR-0062 "Consequences"). `None` when no module
    /// was measured.
    pub const fn limiting_device(&self) -> Option<ModuleId> {
        self.limiting_device
    }

    /// The computed budget: present while calibrated — the live
    /// computation from the current qualification bounds, so the
    /// verdict always matches the alarm flags — and absent until the
    /// first completion and after any `begin`/`invalidate`.
    pub const fn budget(&self) -> Option<&TakeoverBudget> {
        self.budget.as_ref()
    }

    /// The commissioning baseline: the RTT max frozen at completion,
    /// the reference the degradation monitor compares live reality
    /// against. `None` while no valid commissioning calibration
    /// exists.
    pub const fn baseline_rtt_max(&self) -> Option<u64> {
        self.envelope_rtt_max
    }

    /// The `HA_PERFORMANCE_DEGRADED` flag (V4110): reality left the
    /// calibrated envelope.
    pub const fn degraded(&self) -> bool {
        self.degraded
    }

    /// The `HA_TIMING_GUARANTEE_LOST` flag (V4111): the configured
    /// recovery budget can no longer be met.
    pub const fn guarantee_lost(&self) -> bool {
        self.guarantee_lost
    }

    /// How many recalibration runs followed the first commissioning
    /// calibration.
    pub const fn recalibrations(&self) -> u64 {
        self.recalibrations
    }

    /// The last invalidation reason (the Calibration tab's "last
    /// recalibration trigger").
    pub const fn last_invalidation(&self) -> Option<InvalidationReason> {
        self.last_invalidation
    }
}

/// The calibration engine of one unit: the measured terms, the
/// readiness state, the budget, and the guarantee monitors. The shell
/// feeds measurements through the `record_*`/`note_*` APIs — during the
/// commissioning run and live afterwards — and reads the engineering
/// surface through [`status`](Self::status).
#[derive(Clone, Debug)]
pub struct Calibration {
    state: CalibrationState,
    configured_budget: u64,
    /// The frozen commissioning envelope (the RTT max at completion):
    /// the degradation reference. `None` until the first completion.
    envelope_rtt_max: Option<u64>,
    rtt_ab: DirectionProfile,
    rtt_ba: DirectionProfile,
    peer_detect: TermStats,
    lease_expiry: u64,
    scan: TermStats,
    modules: BTreeMap<ModuleId, ModuleContribution>,
    degraded: bool,
    guarantee_lost: bool,
    recalibrations: u64,
    last_invalidation: Option<InvalidationReason>,
}

impl Calibration {
    /// Creates the engine unqualified, with the engineer-configured
    /// recovery budget (ticks) the verdicts check against.
    pub const fn new(configured_budget: u64) -> Self {
        Self {
            state: CalibrationState::Unqualified,
            configured_budget,
            envelope_rtt_max: None,
            rtt_ab: DirectionProfile::new(),
            rtt_ba: DirectionProfile::new(),
            peer_detect: TermStats::new(),
            lease_expiry: 0,
            scan: TermStats::new(),
            modules: BTreeMap::new(),
            degraded: false,
            guarantee_lost: false,
            recalibrations: 0,
            last_invalidation: None,
        }
    }

    /// The calibration run state.
    pub const fn state(&self) -> CalibrationState {
        self.state
    }

    /// The engineer-configured maximum process-recovery budget (ticks)
    /// the verdicts check against.
    pub const fn configured_budget(&self) -> u64 {
        self.configured_budget
    }

    /// The `HA_PERFORMANCE_DEGRADED` latch without the verdict inputs —
    /// the flag the shell's alarm-transition watch consumes.
    pub const fn degraded(&self) -> bool {
        self.degraded
    }

    /// The `HA_TIMING_GUARANTEE_LOST` latch without the verdict inputs —
    /// the flag the shell's alarm-transition watch consumes.
    pub const fn guarantee_lost(&self) -> bool {
        self.guarantee_lost
    }

    /// Begins a calibration run. Any previous guarantee is invalid the
    /// moment a recalibration starts (ADR-0062: readiness is
    /// calibration-gated) — the link profile stays invalid until
    /// [`complete`](Self::complete) produces a fresh one. The measured
    /// statistics carry over: they are the pair's running reality, and
    /// the next completion re-freezes the envelope from them.
    pub fn begin(&mut self) {
        if self.state == CalibrationState::Calibrated {
            self.recalibrations += 1;
        }
        self.state = CalibrationState::Calibrating;
        self.degraded = false;
        self.guarantee_lost = false;
    }

    /// Records that a completed calibration run was a recalibration:
    /// the commissioning run constructs a fresh engine (count 0), and
    /// the shell's recalibration path — which replaces the engine with
    /// a new commissioning run — notes that this run followed an
    /// earlier baseline. The counter is the engine's own; this is the
    /// one write the run-replacement composition needs.
    pub fn note_recalibration(&mut self) {
        self.recalibrations += 1;
    }

    /// Invalidates the qualification: a significant change (ADR-0062's
    /// list, recorded as the reason) requires a new calibration before
    /// the pair is takeover-ready again.
    pub fn invalidate(&mut self, reason: InvalidationReason) {
        self.state = CalibrationState::Unqualified;
        self.envelope_rtt_max = None;
        self.degraded = false;
        self.guarantee_lost = false;
        self.last_invalidation = Some(reason);
    }

    /// Completes the calibration run: freezes the commissioning
    /// envelope and evaluates the guarantee — a commissioning whose
    /// measured reality cannot honor the configured budget never
    /// becomes valid (the guarantee is lost at birth, honestly reported
    /// rather than silently accepted). The budget itself is computed
    /// live in [`status`](Self::status) from the qualification bounds.
    pub fn complete(&mut self) {
        self.state = CalibrationState::Calibrated;
        self.envelope_rtt_max = Some(self.rtt_ab.rtt().max().max(self.rtt_ba.rtt().max()));
        self.evaluate();
    }

    /// Records the engineer-configured maximum process-recovery budget
    /// (ticks) the verdicts check against. Re-evaluates the guarantee
    /// monitors immediately: the engineer parameter the budget
    /// inequality consumes changed, so a setting the live measurements
    /// cannot honor surfaces at once (ADR-0062: validated, never
    /// silently accepted).
    pub fn set_budget(&mut self, configured_budget: u64) {
        self.configured_budget = configured_budget;
        self.evaluate();
    }

    /// Records one ping/pong round-trip sample on a direction.
    pub fn record_rtt(&mut self, direction: Direction, sample: u64) {
        self.profile_mut(direction).record_rtt(sample);
        self.evaluate();
    }

    /// Records one per-side processing sample on a direction (ticks
    /// from the confirming PONG to the next emitted PING — ADR-0062's
    /// "per-side processing latency").
    pub fn record_processing(&mut self, direction: Direction, sample: u64) {
        self.profile_mut(direction).record_processing(sample);
    }

    /// Notes one PING emitted on a direction.
    pub fn note_ping(&mut self, direction: Direction) {
        self.profile_mut(direction).note_ping();
    }

    /// Notes one PONG increment received on a direction (the exchange
    /// answered; any loss streak ends).
    pub fn note_pong(&mut self, direction: Direction) {
        self.profile_mut(direction).note_pong();
    }

    /// Notes one exchange the confirmation window closed unanswered on
    /// a direction.
    pub fn note_miss(&mut self, direction: Direction) {
        self.profile_mut(direction).note_miss();
    }

    /// Records one measured peer-detection sample (ticks from the last
    /// confirmed PONG to the confirmed peer death).
    pub fn record_peer_detect(&mut self, sample: u64) {
        self.peer_detect.record(sample);
        self.evaluate();
    }

    /// Records the I/O owner-lease expiry (the target's old-connection
    /// timeout, or the configured lease TTL when the target sets none).
    pub fn record_lease_expiry(&mut self, ticks: u64) {
        self.lease_expiry = ticks;
        self.evaluate();
    }

    /// Records one measured scan interval (ticks between committed scan
    /// boundaries) feeding the safe-point term.
    pub fn record_scan(&mut self, sample: u64) {
        self.scan.record(sample);
        self.evaluate();
    }

    /// Records one sample of a module's firmware-reported claim / ARM /
    /// output-apply delays (ADR-0062: the I/O firmware reports its own
    /// delays upward).
    pub fn record_module_timing(
        &mut self,
        module: ModuleId,
        claim: u64,
        arm: u64,
        output_apply: u64,
    ) {
        self.modules
            .entry(module)
            .or_insert_with(|| ModuleContribution::new(module))
            .record(claim, arm, output_apply);
        self.evaluate();
    }

    /// The predicted-if-now recovery estimate (ADR-0062: the online
    /// estimator shows two numbers — the predicted recovery if the
    /// failover happened now, and the calibrated worst case): the
    /// EMA-based terms plus the phase-aware safe-point `T_next-commit -
    /// t_takeover`, bounded `0 <= T_safepoint <= T_scan,max`.
    /// `scan_phase_elapsed` is the ticks the current scan has already
    /// run. `None` until calibrated.
    pub fn predicted_if_now(&self, scan_phase_elapsed: u64) -> Option<u64> {
        if self.state != CalibrationState::Calibrated {
            return None;
        }
        let safepoint = self.scan.ema10().saturating_sub(scan_phase_elapsed);
        let detect = self.peer_detect.ema10().max(self.lease_expiry);
        let claim = self.sum_max(|module| module.claim().ema10());
        let arm = self.max_of(|module| module.arm().ema10());
        let output_apply = self.max_of(|module| module.output_apply().ema10());
        Some(
            detect
                .saturating_add(claim)
                .saturating_add(arm)
                .saturating_add(safepoint)
                .saturating_add(output_apply),
        )
    }

    /// The engineering surface: a point-in-time snapshot with the
    /// TakeoverReady verdict computed from the driver's SYNC and IO
    /// inputs (ADR-0062: `TakeoverReady = SYNC_READY && IO_READY &&
    /// RedundancyLinkValid`).
    pub fn status(&self, sync_ready: bool, io_ready: bool) -> CalibrationStatus {
        let link_valid = self.state == CalibrationState::Calibrated && !self.guarantee_lost;
        CalibrationStatus {
            state: self.state,
            link_valid,
            takeover_ready: sync_ready && io_ready && link_valid,
            rtt_ab: self.rtt_ab.clone(),
            rtt_ba: self.rtt_ba.clone(),
            peer_detect: self.peer_detect.clone(),
            lease_expiry: self.lease_expiry,
            claim_start: self.claim_start(),
            scan: self.scan.clone(),
            modules: self.modules.values().cloned().collect(),
            limiting_device: self.limiting_device(),
            budget: if self.state == CalibrationState::Calibrated {
                Some(self.current_budget())
            } else {
                None
            },
            envelope_rtt_max: self.envelope_rtt_max,
            degraded: self.degraded,
            guarantee_lost: self.guarantee_lost,
            recalibrations: self.recalibrations,
            last_invalidation: self.last_invalidation,
        }
    }

    /// The guarantee evaluation, run inside every recording call — the
    /// engine is the one authority on the alarms, never a
    /// caller-must-check convention. Degradation compares the live
    /// channel against the frozen commissioning envelope; the guarantee
    /// recomputes the ADR's budget inequality from the live term
    /// maxima. Only a calibrated pair is monitored: an unqualified or
    /// calibrating pair has no envelope to leave and no budget to lose.
    fn evaluate(&mut self) {
        if self.state != CalibrationState::Calibrated {
            return;
        }
        let envelope = self.envelope_rtt_max.unwrap_or(0);
        if self.rtt_ab.rtt().current() > envelope
            || self.rtt_ab.rtt().ema10() > envelope
            || self.rtt_ba.rtt().current() > envelope
            || self.rtt_ba.rtt().ema10() > envelope
        {
            self.degraded = true;
        }
        if self.current_budget().verdict() != BudgetVerdict::Qualified {
            self.guarantee_lost = true;
        }
    }

    fn profile_mut(&mut self, direction: Direction) -> &mut DirectionProfile {
        match direction {
            Direction::AToB => &mut self.rtt_ab,
            Direction::BToA => &mut self.rtt_ba,
        }
    }

    /// `T_claim-start = max(T_plc-peer-detection,
    /// T_io-owner-lease-expiry)`.
    fn claim_start(&self) -> u64 {
        self.peer_detect.max().max(self.lease_expiry)
    }

    /// The budget from the live qualification bounds.
    fn current_budget(&self) -> TakeoverBudget {
        TakeoverBudget::calculate(
            self.configured_budget,
            self.claim_start(),
            self.sum_max(|module| module.claim().max()),
            self.max_of(|module| module.arm().max()),
            self.scan.max(),
            self.max_of(|module| module.output_apply().max()),
        )
    }

    /// The sequential-claim aggregate: `sum_i` over the modules (ADR-0062,
    /// "Timing formulas": v1 claims sequentially).
    fn sum_max(&self, term: impl Fn(&ModuleContribution) -> u64) -> u64 {
        self.modules.values().map(term).fold(0, u64::saturating_add)
    }

    /// A parallel term's bound: the slowest device.
    fn max_of(&self, term: impl Fn(&ModuleContribution) -> u64) -> u64 {
        self.modules.values().map(term).max().unwrap_or(0)
    }

    /// The limiting device: the module whose qualification-bound claim
    /// contribution dominates the barrier; the first in identity order
    /// on a tie.
    fn limiting_device(&self) -> Option<ModuleId> {
        let mut best: Option<&ModuleContribution> = None;
        for module in self.modules.values() {
            let keep = best.is_some_and(|current| current.claim().max() >= module.claim().max());
            if !keep {
                best = Some(module);
            }
        }
        best.map(ModuleContribution::module)
    }
}

#[cfg(test)]
mod tests;
