//! The query vocabulary of the HA engineering surface: the timestamped
//! event ring, the action refusals and outcomes, and the read-only views
//! the status and IO_READY payloads render — the types
//! [`super::Shell`] answers with and [`crate::commands`] serializes.

use std::fmt;

use crate::config::ConfiguredRole;
use crate::epoch::Epoch;
use crate::fencing::OwnerId;
use crate::statechart::{ControlAlarm, ControlState, DeSyncReason, SyncState};

/// One timestamped entry of the HA event ring (ha-engineering-ui.md,
/// `haEvents`): the kinds of ADR-0062's event list plus the calibration
/// and swap lifecycle events, each stamped with the simulation tick.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct HaEvent {
    /// The simulation tick the event was recorded at.
    pub tick: u64,
    /// What happened.
    pub kind: HaEventKind,
    /// The guard-relevant detail (the module, the unit, the bound).
    pub detail: Option<String>,
}

/// The vocabulary of the event ring — ADR-0062's timestamped events
/// (ForwardOpen received, owner accepted, ARM received, output
/// committed, timeout detected, safe commanded, safe applied) plus the
/// calibration events and the two timing-health alarms.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum HaEventKind {
    /// A claim (ForwardOpen) reached the fencing target for a module.
    ForwardOpenReceived,
    /// The OWNERSHIP_BARRIER passed: the unit owns every required output.
    OwnerAccepted,
    /// The barrier armed every module in one participation.
    ArmReceived,
    /// The newly active unit committed outputs for the first time.
    OutputCommitted,
    /// The exchange confirmed peer death (silence past the missed
    /// threshold); the detail names the last confirmed owner packet.
    TimeoutDetected,
    /// A barrier failure (or fencing loss) commanded the safe state:
    /// everything acquired is being released.
    SafeCommanded,
    /// The safe retreat completed: the acquired modules are released.
    SafeApplied,
    /// A calibration run began (commissioning or recalibration).
    CalibrationBegan,
    /// A calibration run completed; the detail names the new state.
    CalibrationCompleted,
    /// A timing-health alarm latched: reality left the calibrated
    /// envelope (HA_PERFORMANCE_DEGRADED, V4110).
    PerformanceDegraded,
    /// A timing-health alarm latched: the recovery budget can no longer
    /// be met (HA_TIMING_GUARANTEE_LOST, V4111).
    GuaranteeLost,
    /// The engineer's commanded swap passed its guards and was recorded.
    SwapCommanded,
    /// The commanded swap completed: the configured roles exchanged.
    SwapCompleted,
    /// The commanded swap was refused; the detail names the guard.
    SwapRefused,
    /// The engineer's timing parameters were applied; the detail names
    /// the new recovery budget.
    BudgetUpdated,
}

impl HaEventKind {
    /// The wire discriminant of the events view, camelCase.
    pub const fn as_str(self) -> &'static str {
        match self {
            HaEventKind::ForwardOpenReceived => "forwardOpenReceived",
            HaEventKind::OwnerAccepted => "ownerAccepted",
            HaEventKind::ArmReceived => "armReceived",
            HaEventKind::OutputCommitted => "outputCommitted",
            HaEventKind::TimeoutDetected => "timeoutDetected",
            HaEventKind::SafeCommanded => "safeCommanded",
            HaEventKind::SafeApplied => "safeApplied",
            HaEventKind::CalibrationBegan => "calibrationBegan",
            HaEventKind::CalibrationCompleted => "calibrationCompleted",
            HaEventKind::PerformanceDegraded => "performanceDegraded",
            HaEventKind::GuaranteeLost => "guaranteeLost",
            HaEventKind::SwapCommanded => "swapCommanded",
            HaEventKind::SwapCompleted => "swapCompleted",
            HaEventKind::SwapRefused => "swapRefused",
            HaEventKind::BudgetUpdated => "budgetUpdated",
        }
    }
}

/// Why an engineer action was refused — the shell's side of the coded
/// refusals ([`crate::commands`] maps these onto the V-codes).
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Refusal {
    /// The action requires a configured pair; this unit is standalone
    /// (V4112).
    Standalone,
    /// The commanded swap's guards did not hold: the claiming side is
    /// not a Secondary-configured unit in SYNC_READY with an Active
    /// peer and a live link (V4108).
    NotSyncReady,
}

impl fmt::Display for Refusal {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Refusal::Standalone => write!(f, "the unit is standalone: no pair is configured"),
            Refusal::NotSyncReady => {
                write!(f, "the pair is not in SYNC_READY with an active owner")
            }
        }
    }
}

/// The outcome of a commanded swap that passed its guards.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum SwapOutcome {
    /// The releasing side released and the claiming side passed the
    /// barrier; the configured roles exchanged.
    Completed,
    /// The claiming side's barrier failed: the pair is in
    /// REDUNDANCY_LOST (the retreat already happened), the configured
    /// roles did not exchange.
    BarrierFailed,
}

/// The read-only view of one unit's chart state the status payload
/// renders.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct UnitView {
    /// The configured role (exchanged by a completed commanded swap).
    pub role: ConfiguredRole,
    /// The permanent ControllerId (the fencing originator identity).
    pub owner: OwnerId,
    /// The SYNC substate.
    pub sync: SyncState,
    /// The guard-relevant reason of the last deSYNC entry.
    pub sync_reason: DeSyncReason,
    /// The CONTROL substate.
    pub control: ControlState,
    /// The alarm latched on the CONTROL chart, if any.
    pub alarm: Option<ControlAlarm>,
    /// The unit's current epoch.
    pub epoch: Epoch,
}

/// The `IO_READY` breakdown of ADR-0062: each input its own boolean,
/// the failing item identified when false.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct IoReadyView {
    /// Every required I/O module's inputs are observable.
    pub required_inputs: bool,
    /// The standby unit's observer connections are valid.
    pub standby_connections: bool,
    /// Both units hold one application generation (the admission
    /// contract).
    pub configs_match: bool,
    /// The pair's epochs are aligned.
    pub epochs_valid: bool,
    /// The first failing item, when any input is false.
    pub failing: Option<&'static str>,
}

impl IoReadyView {
    /// The single `IO_READY` verdict the TakeoverReady formula consumes.
    pub const fn all(&self) -> bool {
        self.required_inputs && self.standby_connections && self.configs_match && self.epochs_valid
    }
}
