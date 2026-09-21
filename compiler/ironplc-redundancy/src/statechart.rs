//! The redundancy statecharts: is this unit's state aligned with its
//! peer, and does this unit command outputs?
//!
//! One runtime statechart per unit, orthogonal to the configured role
//! (`specs/design/ha-redundancy-fsm.md`). This module carries both
//! superstates as pure table-driven charts: events in, transitions per
//! the spec's tables, no I/O — the CONTROL chart's guards consume the
//! SYNC chart's substate and the configured role, the detection case
//! table's `P`/`I`/`S` signals, and the peer's lease authority, all fed
//! by the driver; the fencing operations themselves run against the
//! [`crate::fencing`] seam the driver holds.
//!
//! The CONTROL chart's guards per the spec:
//!
//! - `IDLE → CLAIMING` is guarded by the guard table — the only
//!   role-dependent transitions in the whole statechart: the boot
//!   barrier (Primary-configured, no live Primary neighbor), the
//!   takeover barrier (Secondary-configured, SYNC in SYNC_READY, `!P &&
//!   !I`, and the peer's OwnerLease authority expired — proven death),
//!   and the commanded-swap barrier (Secondary-configured, pair in
//!   SYNC_READY, swap commanded). Everything else is role-independent.
//! - `CLAIMING → ACTIVE` requires the all-or-nothing OWNERSHIP_BARRIER
//!   to have passed: the driver acquired Exclusive Owner on **all**
//!   required outputs in the fixed configured order, verified each, and
//!   ARMed. Any acquisition or verification failure releases everything
//!   acquired and lands in `REDUNDANCY_LOST` — partial ownership never
//!   means `ACTIVE`.
//! - `ACTIVE → ACTIVE_DEGRADED` on partial I/O loss within policy;
//!   `ACTIVE`/`ACTIVE_DEGRADED → REDUNDANCY_LOST` on full fencing loss;
//!   `REDUNDANCY_LOST` is terminal until manual repair.
//!
//! `deSYNC` covers both the never-synced and the sync-lapsed conditions;
//! a reason is recorded for diagnostics (the spec's "a reason is
//! recorded"). Restart, pair loss, epoch discontinuity, or unclean
//! shutdown always lands there — the zombie re-entry rule is enforced by
//! the admission verdict, and the chart never offers a path back to a
//! running state on its own.

/// The substates of the SYNC superstate.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub enum SyncState {
    /// Not synchronized (never synced, or sync lapsed). The unit executes
    /// no application logic; readiness is re-established per policy.
    #[default]
    DeSync,
    /// Obtains the peer's epoch, then replicates application state,
    /// runtime state, and I/O configuration from the peer. Never
    /// takeover-eligible.
    Syncing,
    /// State replication complete; state and epoch aligned with the peer.
    /// Takeover-eligible (the CONTROL chart's guard, in a later slice).
    SyncReady,
}

/// Why the chart is in `deSYNC`; recorded for diagnostics.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub enum DeSyncReason {
    /// Boot completed (any unit; any restart begins here too, via a fresh
    /// chart).
    #[default]
    Boot,
    /// Sync loss while synchronizing or synchronized.
    SyncLoss,
    /// The peer presented an older epoch than committed: an epoch
    /// discontinuity, rejected as stale (ADR-0062).
    EpochDiscontinuity,
    /// The peer stopped answering: peer death confirmed by the liveness
    /// exchange (pair loss).
    PeerDeath,
}

/// Events the driver feeds the chart, mapped from what the link and the
/// services observe. Events that name no transition for the current state
/// are ignored — a statechart consumes what it understands.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum SyncEvent {
    /// The peer is reachable (paired) and the readiness policy permits
    /// synchronizing.
    Paired,
    /// State replication is complete and the peer epoch is agreed.
    ReplicationComplete,
    /// The exchange reports peer death (pair loss).
    PeerDied,
    /// The peer's epoch went stale: epoch discontinuity.
    EpochDiscontinuity,
    /// State replication failed or the link degraded below usefulness.
    SyncLoss,
}

/// The typed readiness signal of the crossload pipeline: the seam for
/// "state replication complete". The crossload receiver produces this;
/// the SYNC chart consumes the signal, never the payload.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub enum CrossloadReadiness {
    /// Replication has not completed (or has not started).
    #[default]
    InProgress,
    /// The replicated state is complete and current.
    Complete,
}

use crate::config::ConfiguredRole;
use crate::fencing::FencingError;
use crate::problem_codes;

/// The SYNC chart of one unit: the substate plus the recorded `deSYNC`
/// reason.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub struct SyncChart {
    state: SyncState,
    reason: DeSyncReason,
}

impl SyncChart {
    /// Creates the chart at `deSYNC` with the boot reason — the state
    /// every (re)start, pair loss, and unclean shutdown lands in.
    pub fn new() -> Self {
        Self::default()
    }

    /// The current substate.
    pub fn state(&self) -> SyncState {
        self.state
    }

    /// Why the chart is in `deSYNC` (the reason of the last entry).
    pub fn reason(&self) -> DeSyncReason {
        self.reason
    }

    /// Applies one event per the transition table. Unknown combinations
    /// leave the chart unchanged.
    pub fn apply(&mut self, event: SyncEvent) {
        match (self.state, event) {
            (SyncState::DeSync, SyncEvent::Paired) => {
                self.state = SyncState::Syncing;
            }
            (SyncState::Syncing, SyncEvent::ReplicationComplete) => {
                self.state = SyncState::SyncReady;
            }
            (SyncState::Syncing, SyncEvent::PeerDied) => {
                self.state = SyncState::DeSync;
                self.reason = DeSyncReason::PeerDeath;
            }
            (SyncState::SyncReady, SyncEvent::PeerDied) => {
                self.state = SyncState::DeSync;
                self.reason = DeSyncReason::PeerDeath;
            }
            (SyncState::Syncing, SyncEvent::EpochDiscontinuity) => {
                self.state = SyncState::DeSync;
                self.reason = DeSyncReason::EpochDiscontinuity;
            }
            (SyncState::SyncReady, SyncEvent::EpochDiscontinuity) => {
                self.state = SyncState::DeSync;
                self.reason = DeSyncReason::EpochDiscontinuity;
            }
            (SyncState::Syncing, SyncEvent::SyncLoss) => {
                self.state = SyncState::DeSync;
                self.reason = DeSyncReason::SyncLoss;
            }
            (SyncState::SyncReady, SyncEvent::SyncLoss) => {
                self.state = SyncState::DeSync;
                self.reason = DeSyncReason::SyncLoss;
            }
            // Peer death before pairing keeps the chart in deSYNC but
            // records the fresher reason (diagnostics, per the spec's
            // "a reason is recorded").
            (SyncState::DeSync, SyncEvent::PeerDied) => {
                self.reason = DeSyncReason::PeerDeath;
            }
            // Every other combination names no transition: ignored.
            _ => {}
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn chart_when_booted_then_de_sync_with_boot_reason() {
        let chart = SyncChart::new();

        assert_eq!(chart.state(), SyncState::DeSync);
        assert_eq!(chart.reason(), DeSyncReason::Boot);
    }

    #[test]
    fn chart_when_paired_then_moves_de_sync_to_syncing() {
        let mut chart = SyncChart::new();

        chart.apply(SyncEvent::Paired);

        assert_eq!(chart.state(), SyncState::Syncing);
        assert_eq!(chart.reason(), DeSyncReason::Boot);
    }

    #[test]
    fn chart_when_replication_completes_then_sync_ready() {
        let mut chart = SyncChart::new();
        chart.apply(SyncEvent::Paired);

        chart.apply(SyncEvent::ReplicationComplete);

        assert_eq!(chart.state(), SyncState::SyncReady);
    }

    #[test]
    fn chart_when_peer_dies_then_any_sync_substate_returns_to_de_sync() {
        let mut syncing = SyncChart::new();
        syncing.apply(SyncEvent::Paired);
        syncing.apply(SyncEvent::PeerDied);
        assert_eq!(syncing.state(), SyncState::DeSync);
        assert_eq!(syncing.reason(), DeSyncReason::PeerDeath);

        let mut ready = SyncChart::new();
        ready.apply(SyncEvent::Paired);
        ready.apply(SyncEvent::ReplicationComplete);
        ready.apply(SyncEvent::PeerDied);
        assert_eq!(ready.state(), SyncState::DeSync);
        assert_eq!(ready.reason(), DeSyncReason::PeerDeath);
    }

    #[test]
    fn chart_when_epoch_discontinues_then_de_sync_with_discontinuity_reason() {
        let mut chart = SyncChart::new();
        chart.apply(SyncEvent::Paired);
        chart.apply(SyncEvent::ReplicationComplete);

        chart.apply(SyncEvent::EpochDiscontinuity);

        assert_eq!(chart.state(), SyncState::DeSync);
        assert_eq!(chart.reason(), DeSyncReason::EpochDiscontinuity);

        // The same discontinuity while synchronizing: same landing.
        let mut syncing = SyncChart::new();
        syncing.apply(SyncEvent::Paired);
        syncing.apply(SyncEvent::EpochDiscontinuity);
        assert_eq!(syncing.state(), SyncState::DeSync);
        assert_eq!(syncing.reason(), DeSyncReason::EpochDiscontinuity);
    }

    #[test]
    fn chart_when_sync_loses_then_de_sync_with_sync_loss_reason() {
        let mut chart = SyncChart::new();
        chart.apply(SyncEvent::Paired);

        chart.apply(SyncEvent::SyncLoss);

        assert_eq!(chart.state(), SyncState::DeSync);
        assert_eq!(chart.reason(), DeSyncReason::SyncLoss);
    }

    #[test]
    fn chart_when_sync_ready_lost_then_reenters_via_syncing() {
        // SYNC_READY lost ⇒ deSYNC, re-enter via SYNCING (the spec's
        // subchart diagram): paired again restarts the pipeline.
        let mut chart = SyncChart::new();
        chart.apply(SyncEvent::Paired);
        chart.apply(SyncEvent::ReplicationComplete);
        chart.apply(SyncEvent::PeerDied);

        chart.apply(SyncEvent::Paired);
        chart.apply(SyncEvent::ReplicationComplete);

        assert_eq!(chart.state(), SyncState::SyncReady);
    }

    #[test]
    fn chart_when_peer_dies_before_pairing_then_reason_refreshes() {
        // Death during discovery never leaves deSYNC, but the reason says
        // why the unit is still there.
        let mut chart = SyncChart::new();

        chart.apply(SyncEvent::PeerDied);

        assert_eq!(chart.state(), SyncState::DeSync);
        assert_eq!(chart.reason(), DeSyncReason::PeerDeath);
    }

    #[test]
    fn chart_when_event_names_no_transition_then_unchanged() {
        let mut chart = SyncChart::new();

        chart.apply(SyncEvent::ReplicationComplete);
        assert_eq!(chart.state(), SyncState::DeSync);

        chart.apply(SyncEvent::SyncLoss);
        assert_eq!(chart.state(), SyncState::DeSync);
        // Ignored events never overwrite the boot reason.
        assert_eq!(chart.reason(), DeSyncReason::Boot);

        chart.apply(SyncEvent::Paired);
        chart.apply(SyncEvent::Paired);
        assert_eq!(chart.state(), SyncState::Syncing);

        chart.apply(SyncEvent::ReplicationComplete);
        chart.apply(SyncEvent::ReplicationComplete);
        assert_eq!(chart.state(), SyncState::SyncReady);
    }
}

/// The substates of the CONTROL superstate.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub enum ControlState {
    /// No output control: the unit owns no outputs and observes I/O over
    /// Input Only connections.
    #[default]
    Idle,
    /// The OWNERSHIP_BARRIER in progress: the way is clear (peer gone, or
    /// ownership released in a commanded swap) and self is ready, but the
    /// unit must prove fencing by acquiring Exclusive Owner on **all**
    /// required outputs in the fixed configured order, verifying each.
    /// Not yet output-controlling.
    Claiming,
    /// The barrier passed: the unit owns all required I/O, ARMed, and
    /// commands outputs under the current epoch (`CAN_EXECUTE_OUTPUTS`).
    Active,
    /// An ACTIVE unit that lost a policy-defined subset of I/O ownership:
    /// it continues executing with the degraded policy; escalation to
    /// full fencing loss ends in `REDUNDANCY_LOST`.
    ActiveDegraded,
    /// Terminal until manual repair: entered on any failed barrier
    /// acquisition and on full fencing loss. The alarm is latched; on
    /// manual repair/requalification the unit returns to `Idle` with the
    /// SYNC chart at deSYNC.
    RedundancyLost,
}

/// The three ways into `CLAIMING` — the guard table's only role-dependent
/// transitions (the FSM spec, "Guard table").
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum ClaimBarrier {
    /// Clean boot with fencing applied at boot (Primary-configured only).
    Boot,
    /// Proven death of the Primary: the peer is silent on both channels
    /// with no I/O evidence and its OwnerLease authority has expired, and
    /// self is SYNC_READY (Secondary-configured only).
    Takeover,
    /// The engineer commanded a role swap while the pair is in SYNC_READY
    /// (the claiming side; the releasing side takes `ReleaseOwner`).
    CommandedSwap,
}

/// Events the driver feeds the CONTROL chart, mapped from the guard
/// table's evaluation, the fencing barrier's outcome, and the fencing
/// observation. Events that name no transition for the current state are
/// ignored — a statechart consumes what it understands.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum ControlEvent {
    /// A guard-table barrier passed (the driver verified the guard):
    /// begin the ordered claim.
    ClaimGuarded,
    /// The barrier passed: every required module is claimed and ARMed by
    /// self in the current epoch. The driver bumps the epoch, mints the
    /// OwnerLease, and grants the execution permit alongside this event.
    BarrierPassed,
    /// Any acquisition or verification failed; the driver has already
    /// released everything acquired. Latch the alarm, land terminal.
    BarrierFailed,
    /// Commanded role swap (the releasing side): drop all I/O ownership,
    /// stop executing; the SYNC chart re-establishes readiness as the new
    /// Secondary.
    ReleaseOwner,
    /// Partial I/O ownership loss within the degraded policy.
    IoLossPartial,
    /// The fencing loss grew to full.
    IoLossFull,
    /// Manual repair / requalification after `REDUNDANCY_LOST`.
    Repaired,
}

/// The CONTROL chart of one unit: the substate plus the latched alarm.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub struct ControlChart {
    state: ControlState,
    alarm: Option<ControlAlarm>,
}

impl ControlChart {
    /// Creates the chart at `Idle` — every boot begins owning nothing.
    pub fn new() -> Self {
        Self::default()
    }

    /// The current substate.
    pub fn state(&self) -> ControlState {
        self.state
    }

    /// The alarm latched on `REDUNDANCY_LOST` entry or the degraded
    /// observation — the coded surface the engineering UI raises.
    pub fn alarm(&self) -> Option<ControlAlarm> {
        self.alarm
    }

    /// Applies one event per the transition table. Unknown combinations
    /// leave the chart unchanged.
    pub fn apply(&mut self, event: ControlEvent) {
        match (self.state, event) {
            (ControlState::Idle, ControlEvent::ClaimGuarded) => {
                self.state = ControlState::Claiming;
            }
            (ControlState::Claiming, ControlEvent::BarrierPassed) => {
                self.state = ControlState::Active;
            }
            (ControlState::Claiming, ControlEvent::BarrierFailed) => {
                self.state = ControlState::RedundancyLost;
                self.alarm
                    .get_or_insert(ControlAlarm::OwnershipBarrierFailed);
            }
            (ControlState::Active, ControlEvent::ReleaseOwner) => {
                self.state = ControlState::Idle;
            }
            (ControlState::Active, ControlEvent::IoLossPartial) => {
                self.state = ControlState::ActiveDegraded;
            }
            (ControlState::Active, ControlEvent::IoLossFull)
            | (ControlState::ActiveDegraded, ControlEvent::IoLossFull) => {
                self.state = ControlState::RedundancyLost;
                self.alarm
                    .get_or_insert(ControlAlarm::OwnershipBarrierFailed);
            }
            (ControlState::RedundancyLost, ControlEvent::Repaired) => {
                self.state = ControlState::Idle;
            }
            // Every other combination names no transition: ignored.
            _ => {}
        }
    }

    /// Latches an alarm without a state change — the degraded-channel
    /// observation (`!P && I`) raises the alarm while the chart stays
    /// `Idle`, per the detection case table's "stay in IDLE; raise
    /// degraded-channel alarm; never claim".
    pub fn raise_alarm(&mut self, alarm: ControlAlarm) {
        self.alarm = Some(alarm);
    }
}

/// Why the CONTROL chart latched an alarm — the crate's CONTROL alarm
/// vocabulary (the crate-local V41xx CSV). A latched alarm is the coded
/// surface the engineering UI raises; `OwnershipBarrierFailed` on
/// `REDUNDANCY_LOST` entry, the others as raised.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum ControlAlarm {
    /// A claim was rejected: a live owner still holds the module's
    /// exclusive ownership — the partition evidence (V4106).
    OwnerConflict,
    /// The OWNERSHIP_BARRIER failed — an acquisition or verification
    /// failure released everything acquired; the pair lands in
    /// `REDUNDANCY_LOST` and asks a human (V4107).
    OwnershipBarrierFailed,
    /// The engineer's commanded role swap was refused: the pair is not in
    /// `SYNC_READY` (V4108).
    SwapRefused,
    /// Ping/pong is lost on both channels but the peer demonstrably owns
    /// I/O — degraded-channel alarm; the unit stays `Idle` and never
    /// claims (V4109).
    DegradedChannel,
}

impl ControlAlarm {
    /// The stable V-code surfacing this alarm on the HA command surface
    /// (the crate-local CSV).
    pub const fn v_code(self) -> &'static str {
        match self {
            ControlAlarm::OwnerConflict => problem_codes::OWNER_CONFLICT,
            ControlAlarm::OwnershipBarrierFailed => problem_codes::OWNERSHIP_BARRIER_FAILED,
            ControlAlarm::SwapRefused => problem_codes::SWAP_REFUSED,
            ControlAlarm::DegradedChannel => problem_codes::DEGRADED_CHANNEL,
        }
    }
}

impl From<FencingError> for ControlAlarm {
    /// A fencing refusal is the alarm the CONTROL chart latches: the
    /// owner conflict is the partition evidence, and every other barrier
    /// failure is the coded barrier alarm.
    fn from(error: FencingError) -> Self {
        match error {
            FencingError::OwnerConflict => ControlAlarm::OwnerConflict,
            FencingError::NotOwned | FencingError::Unavailable => {
                ControlAlarm::OwnershipBarrierFailed
            }
        }
    }
}

/// The guard table — the only role-dependent transitions in the whole
/// statechart. Evaluates whether `IDLE → CLAIMING` is allowed right now
/// and names the barrier; `None` means forbidden (every other case: a
/// Secondary never controls I/O outside the two promotion cases, and a
/// Primary-configured unit never claims through them).
///
/// * Boot barrier: clean boot, no live Primary neighbor,
///   Primary-configured.
/// * Takeover barrier: Secondary-configured, SYNC in `SYNC_READY`, the
///   peer silent on both channels with no I/O evidence (`!P && !I`), and
///   the peer's OwnerLease authority expired — proven death, both
///   independent proofs agreeing (`max(peer-detection, lease-expiry)`).
/// * Commanded-swap barrier: the engineer commanded the swap and the pair
///   is in `SYNC_READY` (the releasing side never evaluates this: it is
///   not `Idle`).
pub fn claim_barrier(
    configured: ConfiguredRole,
    sync: SyncState,
    peer_live: bool,
    io_evidence: bool,
    authority_expired: bool,
    swap_commanded: bool,
    boot: bool,
) -> Option<ClaimBarrier> {
    if swap_commanded {
        return match (configured, sync) {
            (ConfiguredRole::Secondary, SyncState::SyncReady) => Some(ClaimBarrier::CommandedSwap),
            _ => None,
        };
    }
    if boot && !peer_live && configured == ConfiguredRole::Primary {
        return Some(ClaimBarrier::Boot);
    }
    if !peer_live
        && !io_evidence
        && authority_expired
        && configured == ConfiguredRole::Secondary
        && sync == SyncState::SyncReady
    {
        return Some(ClaimBarrier::Takeover);
    }
    None
}

/// The action of the detection case table (the FSM spec, "Detection Case
/// Table") for a unit that is not output-controlling: what the observed
/// `P` (peer ping/pong), `I` (I/O evidence of a live peer), and `S` (self
/// ready) mean right now.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum DetectionAction {
    /// `P`: the peer is observed — nothing to do (normal, or the peer is
    /// alive and the SYNC chart handles own-sync-broken).
    None,
    /// `!P && I`: the peer demonstrably owns I/O while silent on both
    /// channels — stay `Idle`, raise the degraded-channel alarm, never
    /// claim.
    DegradedChannel,
    /// `!P && !I && S`: candidacy, not promotion — enter `CLAIMING` per
    /// the guard table and let the target's exclusivity arbitrate:
    /// silence makes a claimant, never an owner.
    PromotionCandidate,
    /// `!P && !S`: ambiguous and self not ready — the SYNC chart is in
    /// deSYNC; claim forbidden.
    NotReady,
}

/// The detection case table: `P` and `I` and `S` in, the decided action
/// out. Loss of only one ping/pong channel is degraded transport, not
/// redundancy loss — that case never reaches this table as `!P`.
pub fn detect(peer_live: bool, io_evidence: bool, self_ready: bool) -> DetectionAction {
    match (peer_live, io_evidence, self_ready) {
        (true, _, _) => DetectionAction::None,
        (false, true, true) => DetectionAction::DegradedChannel,
        (false, false, true) => DetectionAction::PromotionCandidate,
        (false, _, false) => DetectionAction::NotReady,
    }
}

#[cfg(test)]
mod control_tests {
    use super::*;

    #[test]
    fn control_when_booted_then_idle_without_alarm() {
        let chart = ControlChart::new();

        assert_eq!(chart.state(), ControlState::Idle);
        assert_eq!(chart.alarm(), None);
    }

    #[test]
    fn control_when_barrier_passes_then_active() {
        let mut chart = ControlChart::new();
        chart.apply(ControlEvent::ClaimGuarded);
        assert_eq!(chart.state(), ControlState::Claiming);

        chart.apply(ControlEvent::BarrierPassed);

        assert_eq!(chart.state(), ControlState::Active);
        assert_eq!(chart.alarm(), None);
    }

    #[test]
    fn control_when_barrier_fails_then_redundancy_lost_with_barrier_alarm() {
        let mut chart = ControlChart::new();
        chart.apply(ControlEvent::ClaimGuarded);

        chart.apply(ControlEvent::BarrierFailed);

        assert_eq!(chart.state(), ControlState::RedundancyLost);
        assert_eq!(chart.alarm(), Some(ControlAlarm::OwnershipBarrierFailed));
        assert_eq!(chart.alarm().unwrap().v_code(), "V4107");
    }

    #[test]
    fn control_when_swap_releases_then_active_to_idle() {
        let mut chart = ControlChart::new();
        chart.apply(ControlEvent::ClaimGuarded);
        chart.apply(ControlEvent::BarrierPassed);

        chart.apply(ControlEvent::ReleaseOwner);

        assert_eq!(chart.state(), ControlState::Idle);
    }

    #[test]
    fn control_when_partial_loss_then_degraded_and_full_loss_then_lost() {
        let mut chart = ControlChart::new();
        chart.apply(ControlEvent::ClaimGuarded);
        chart.apply(ControlEvent::BarrierPassed);

        chart.apply(ControlEvent::IoLossPartial);
        assert_eq!(chart.state(), ControlState::ActiveDegraded);

        chart.apply(ControlEvent::IoLossFull);
        assert_eq!(chart.state(), ControlState::RedundancyLost);
    }

    #[test]
    fn control_when_full_loss_from_active_then_lost() {
        let mut chart = ControlChart::new();
        chart.apply(ControlEvent::ClaimGuarded);
        chart.apply(ControlEvent::BarrierPassed);

        chart.apply(ControlEvent::IoLossFull);

        assert_eq!(chart.state(), ControlState::RedundancyLost);
    }

    #[test]
    fn control_when_repaired_then_back_to_idle() {
        let mut chart = ControlChart::new();
        chart.apply(ControlEvent::ClaimGuarded);
        chart.apply(ControlEvent::BarrierFailed);
        assert_eq!(chart.state(), ControlState::RedundancyLost);

        chart.apply(ControlEvent::Repaired);

        assert_eq!(chart.state(), ControlState::Idle);
        // The alarm is the diagnostics record; repair clears the state,
        // the history stays latched.
        assert_eq!(chart.alarm(), Some(ControlAlarm::OwnershipBarrierFailed));
    }

    #[test]
    fn control_when_event_names_no_transition_then_unchanged() {
        let mut chart = ControlChart::new();

        chart.apply(ControlEvent::BarrierPassed);
        assert_eq!(chart.state(), ControlState::Idle);
        chart.apply(ControlEvent::ReleaseOwner);
        assert_eq!(chart.state(), ControlState::Idle);
        chart.apply(ControlEvent::BarrierFailed);
        assert_eq!(chart.state(), ControlState::Idle);
        chart.apply(ControlEvent::Repaired);
        assert_eq!(chart.state(), ControlState::Idle);
    }

    #[test]
    fn control_when_alarm_raised_then_latched_without_state_change() {
        let mut chart = ControlChart::new();

        chart.raise_alarm(ControlAlarm::DegradedChannel);

        assert_eq!(chart.state(), ControlState::Idle);
        assert_eq!(chart.alarm(), Some(ControlAlarm::DegradedChannel));
        assert_eq!(chart.alarm().unwrap().v_code(), "V4109");
    }

    #[test]
    fn guard_when_boot_barrier_then_primary_only_without_live_peer() {
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
        // A live Primary neighbor forbids the boot claim (admission's
        // zombie fence decides first).
        assert_eq!(
            claim_barrier(
                ConfiguredRole::Primary,
                SyncState::DeSync,
                true,
                false,
                false,
                false,
                true
            ),
            None
        );
        // Secondary-configured may never boot-claim.
        assert_eq!(
            claim_barrier(
                ConfiguredRole::Secondary,
                SyncState::DeSync,
                false,
                false,
                false,
                false,
                true
            ),
            None
        );
    }

    #[test]
    fn guard_when_takeover_barrier_then_proven_death_only() {
        // Proven death: silent, no I/O evidence, authority expired, ready.
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
        // Not expired yet: silence alone is not proof.
        assert_eq!(
            claim_barrier(
                ConfiguredRole::Secondary,
                SyncState::SyncReady,
                false,
                false,
                false,
                false,
                false
            ),
            None
        );
        // I/O evidence of a live peer: never claim.
        assert_eq!(
            claim_barrier(
                ConfiguredRole::Secondary,
                SyncState::SyncReady,
                false,
                true,
                true,
                false,
                false
            ),
            None
        );
        // Not SYNC_READY: claim forbidden.
        assert_eq!(
            claim_barrier(
                ConfiguredRole::Secondary,
                SyncState::Syncing,
                false,
                false,
                true,
                false,
                false
            ),
            None
        );
        // Primary-configured never claims through the promotion path.
        assert_eq!(
            claim_barrier(
                ConfiguredRole::Primary,
                SyncState::SyncReady,
                false,
                false,
                true,
                false,
                false
            ),
            None
        );
    }

    #[test]
    fn guard_when_commanded_swap_then_sync_ready_pair_only() {
        assert_eq!(
            claim_barrier(
                ConfiguredRole::Secondary,
                SyncState::SyncReady,
                true,
                false,
                false,
                true,
                false
            ),
            Some(ClaimBarrier::CommandedSwap)
        );
        // Not SYNC_READY: the swap is refused, never half-run.
        assert_eq!(
            claim_barrier(
                ConfiguredRole::Secondary,
                SyncState::Syncing,
                true,
                false,
                false,
                true,
                false
            ),
            None
        );
        // The releasing side (Primary-configured) never evaluates the
        // claim: its half of the swap is the release.
        assert_eq!(
            claim_barrier(
                ConfiguredRole::Primary,
                SyncState::SyncReady,
                true,
                false,
                false,
                true,
                false
            ),
            None
        );
    }

    #[test]
    fn detect_when_peer_live_then_none_regardless_of_io_evidence() {
        assert_eq!(detect(true, false, true), DetectionAction::None);
        assert_eq!(detect(true, true, true), DetectionAction::None);
        assert_eq!(detect(true, false, false), DetectionAction::None);
    }

    #[test]
    fn detect_when_silent_with_io_evidence_and_ready_then_degraded_channel() {
        assert_eq!(detect(false, true, true), DetectionAction::DegradedChannel);
    }

    #[test]
    fn detect_when_silent_without_evidence_and_ready_then_candidacy() {
        assert_eq!(
            detect(false, false, true),
            DetectionAction::PromotionCandidate
        );
    }

    #[test]
    fn detect_when_silent_and_not_ready_then_claim_forbidden() {
        assert_eq!(detect(false, false, false), DetectionAction::NotReady);
        assert_eq!(detect(false, true, false), DetectionAction::NotReady);
    }

    #[test]
    fn alarm_v_codes_when_surfaced_then_stable_codes() {
        assert_eq!(ControlAlarm::OwnerConflict.v_code(), "V4106");
        assert_eq!(ControlAlarm::OwnershipBarrierFailed.v_code(), "V4107");
        assert_eq!(ControlAlarm::SwapRefused.v_code(), "V4108");
        assert_eq!(ControlAlarm::DegradedChannel.v_code(), "V4109");
    }

    #[test]
    fn alarm_from_fencing_error_then_refusal_mapping() {
        assert_eq!(
            ControlAlarm::from(FencingError::OwnerConflict),
            ControlAlarm::OwnerConflict
        );
        assert_eq!(
            ControlAlarm::from(FencingError::NotOwned),
            ControlAlarm::OwnershipBarrierFailed
        );
        assert_eq!(
            ControlAlarm::from(FencingError::Unavailable),
            ControlAlarm::OwnershipBarrierFailed
        );
    }
}
