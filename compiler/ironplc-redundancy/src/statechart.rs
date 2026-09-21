//! The SYNC subchart: is this unit's state aligned with its peer?
//!
//! One runtime statechart per unit, orthogonal to the configured role
//! (`specs/design/ha-redundancy-fsm.md`). This module carries the SYNC
//! superstate only — `deSYNC → SYNCING → SYNC_READY` — as a pure
//! table-driven chart: events in, transitions per the spec's table, no
//! I/O. The CONTROL chart (IDLE → CLAIMING → ACTIVE, REDUNDANCY_LOST) is a
//! later slice; nothing here names ownership.
//!
//! The chart's guards per the spec:
//!
//! - `deSYNC → SYNCING`: the peer is reachable (paired) and the readiness
//!   policy permits — this slice models the automatic policy.
//! - `SYNCING → SYNC_READY`: state replication is complete and the peer
//!   epoch is agreed. State replication is not built yet: the driver
//!   composes that guard from [`CrossloadReadiness`] (the typed
//!   placeholder seam the future crossload module feeds — the signal, not
//!   the payload) and the epoch-adoption outcome.
//! - any state → `deSYNC`: sync loss, epoch discontinuity, or peer death
//!   (`SYNC_READY` lost ⇒ `deSYNC`, re-enter via `SYNCING`).
//!
//! `deSYNC` covers both the never-synced and the sync-lapsed conditions; a
//! reason is recorded for diagnostics (the spec's "a reason is recorded").
//! Restart, pair loss, epoch discontinuity, or unclean shutdown always
//! lands here — the zombie re-entry rule is enforced by the admission
//! verdict, and the chart never offers a path back to a running state on
//! its own.

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

/// The typed readiness signal of the future crossload pipeline: the
/// placeholder seam for "state replication complete". The crossload
/// module (a later slice) produces this; the SYNC chart consumes the
/// signal, never the payload.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub enum CrossloadReadiness {
    /// Replication has not completed (or has not started).
    #[default]
    InProgress,
    /// The replicated state is complete and current.
    Complete,
}

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
