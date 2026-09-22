//! The HA engineering command vocabulary: typed, serializable commands
//! over the line-delimited JSON codec, following the ADR-0055 pattern
//! of the runtime's command layer (`compiler/runtime/src/commands.rs`)
//! — one serde-tagged `HaCommand`/`HaResponse` enum, one `execute`
//! dispatch, one line in, one line out, and no protocol state of its
//! own (the shell's charts are the only HA state).
//!
//! This module is the wire form of the HA engineering UI contract
//! (`specs/design/ha-engineering-ui.md`): the six queries
//! (`haStatus`, `haCalibration`, `haBarrier`, `haIoReady`,
//! `haTimingBudget`, `haEvents`) and the engineer actions
//! (`haCommandedSwap`, `haRunCalibration`, `haSetTimingBudget`). Every
//! payload field names the contract's item; every value the shell
//! measures or computes — nothing here is an assumed constant
//! (ADR-0062). The session composes this layer beside the runtime's
//! hot-edit layer; a line parses against one or the other, never both
//! (the vocabularies are disjoint), so neither layer's FSM or
//! one-line-in/one-line-out ordering is disturbed.

use serde::{Deserialize, Serialize};

use crate::calibration::BudgetVerdict;
use crate::fencing::ModuleState;
use crate::pair_link::PairLinkStatus;
use crate::problem_codes;
use crate::shell::{HaEvent, Refusal, Shell, Side, SwapOutcome};
use crate::statechart::SyncState;
use crate::timing::{DirectionProfile, TermStats};

/// A command in the HA engineering protocol (ha-engineering-ui.md).
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(tag = "command", rename_all = "camelCase")]
pub enum HaCommand {
    /// The pair overview: both units' chart states, the epoch, the
    /// generation counters, the TakeoverReady verdict with its inputs,
    /// and the active alarm flags.
    HaStatus,
    /// The per-channel calibration state and the calibration run state.
    HaCalibration,
    /// The ownership barrier view: per-module profile, owner state, and
    /// T contribution, the limiting device, the worst ownership recovery.
    HaBarrier,
    /// The `IO_READY` breakdown, the failing item identified when false.
    HaIoReady,
    /// The failover formula with live terms and the budget verdict.
    HaTimingBudget,
    /// The timestamped event ring.
    HaEvents,
    /// The commanded Primary↔Secondary swap: refused outside SYNC_READY
    /// (V4108).
    HaCommandedSwap,
    /// Runs a commissioning/recalibration run (synchronous on the demo
    /// binding).
    HaRunCalibration,
    /// Sets the peer-failure confirmation time and the recovery budget;
    /// a budget the installation cannot honor is reported, never
    /// silently applied (V4111 names the refusal).
    #[serde(rename_all = "camelCase")]
    HaSetTimingBudget {
        /// The peer-failure confirmation time (ticks).
        peer_failure_confirmation: u64,
        /// The maximum process-recovery budget (ticks).
        recovery_budget: u64,
    },
}

/// The measured-term block every tracker renders: current / min /
/// EMA10 / EMA100 / max / count (ADR-0062, "Decision"; the min
/// completes the jitter envelope).
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct TermStatsPayload {
    /// The most recent sample.
    current: u64,
    /// The smallest observed sample.
    min: u64,
    /// The fast EMA (EMA10), rounded to ticks.
    ema10: u64,
    /// The slow EMA (EMA100), rounded to ticks.
    ema100: u64,
    /// The running maximum — the qualification bound.
    max: u64,
    /// How many samples were recorded.
    count: u64,
}

impl From<&TermStats> for TermStatsPayload {
    fn from(stats: &TermStats) -> Self {
        TermStatsPayload {
            current: stats.current(),
            min: stats.min(),
            ema10: stats.ema10(),
            ema100: stats.ema100(),
            max: stats.max(),
            count: stats.count(),
        }
    }
}

/// One unit's block of the pair overview: the configured role, the
/// ControllerId, both chart states with the guard-relevant reason, and
/// the latched alarm. Standalone units render every state `None` — the
/// statechart does not run.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct UnitPayload {
    /// The configured role (`primary` / `secondary`); `None` standalone.
    #[serde(skip_serializing_if = "Option::is_none")]
    role: Option<&'static str>,
    /// The permanent ControllerId (the fencing originator identity).
    controller_id: u64,
    /// The SYNC substate; `None` standalone.
    #[serde(skip_serializing_if = "Option::is_none")]
    sync: Option<&'static str>,
    /// The guard-relevant reason of the last deSYNC entry.
    #[serde(skip_serializing_if = "Option::is_none")]
    sync_reason: Option<&'static str>,
    /// The CONTROL substate; `None` standalone.
    #[serde(skip_serializing_if = "Option::is_none")]
    control: Option<&'static str>,
    /// The alarm latched on the CONTROL chart, if any.
    #[serde(skip_serializing_if = "Option::is_none")]
    alarm: Option<&'static str>,
    /// The unit's current epoch.
    epoch: u32,
}

/// The active alarm flags of the pair (ha-engineering-ui.md, haStatus).
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct AlarmFlagsPayload {
    /// `HA_PERFORMANCE_DEGRADED` (V4110): reality left the calibrated
    /// envelope.
    performance_degraded: bool,
    /// `HA_TIMING_GUARANTEE_LOST` (V4111): the recovery budget can no
    /// longer be met.
    timing_guarantee_lost: bool,
    /// `REDUNDANCY_LOST`: a CONTROL chart is in the terminal state.
    redundancy_lost: bool,
}

/// The payload of [`HaResponse::HaStatus`]: the pair overview.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct HaStatusPayload {
    /// Whether this unit runs standalone (no pair configured); the
    /// honest state every other field then renders empty.
    standalone: bool,
    /// The pair identity; `None` standalone.
    #[serde(skip_serializing_if = "Option::is_none")]
    pair_id: Option<String>,
    /// The served unit's state.
    local: UnitPayload,
    /// The peer unit's state; `None` standalone.
    #[serde(skip_serializing_if = "Option::is_none")]
    peer: Option<UnitPayload>,
    /// The pair's current epoch; `None` standalone.
    #[serde(skip_serializing_if = "Option::is_none")]
    epoch: Option<u32>,
    /// The application generation (the committed manifest).
    application_generation: u32,
    /// The committed state generation (the host's boundary counter).
    state_generation: u64,
    /// The `TakeoverReady` verdict with its three inputs broken out
    /// (ADR-0062).
    takeover_ready: bool,
    /// The `SYNC_READY` input.
    sync_ready: bool,
    /// The `IO_READY` input.
    io_ready: bool,
    /// The `RedundancyLinkValid` input.
    link_valid: bool,
    /// The active alarm flags.
    alarms: AlarmFlagsPayload,
}

/// The per-direction link profile of the calibration view.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct DirectionPayload {
    /// The ping/pong round-trip latency (current / min / EMA10 / EMA100
    /// / max / count); min..=max is the jitter envelope.
    rtt: TermStatsPayload,
    /// The per-side processing latency (the confirming PONG to the next
    /// emitted PING).
    processing: TermStatsPayload,
    /// The loss rate in whole percent.
    loss_rate_percent: u64,
    /// The longest run of unanswered exchanges.
    max_consecutive_loss: u64,
    /// PINGs emitted.
    pings_sent: u64,
    /// PONG increments received.
    pongs_received: u64,
}

impl From<&DirectionProfile> for DirectionPayload {
    fn from(profile: &DirectionProfile) -> Self {
        DirectionPayload {
            rtt: TermStatsPayload::from(profile.rtt()),
            processing: TermStatsPayload::from(profile.processing()),
            loss_rate_percent: profile.loss_rate_percent(),
            max_consecutive_loss: profile.max_consecutive_loss(),
            pings_sent: profile.pings_sent(),
            pongs_received: profile.pongs_received(),
        }
    }
}

/// The payload of [`HaResponse::HaCalibration`]: the per-channel
/// calibration state and the calibration run state.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct HaCalibrationPayload {
    /// The calibration run state (unqualified / calibrating /
    /// calibrated).
    state: &'static str,
    /// Whether the link profile is valid (the `RedundancyLinkValid`
    /// input).
    link_valid: bool,
    /// The TakeoverReady verdict computed from the live inputs.
    takeover_ready: bool,
    /// The A→B→A link profile.
    ab: DirectionPayload,
    /// The B→A→B link profile.
    ba: DirectionPayload,
    /// The peer-detection term (measured by the confirmation drills).
    peer_detect: TermStatsPayload,
    /// The I/O owner-lease expiry (the target's old-connection
    /// timeout).
    lease_expiry: u64,
    /// `T_claim-start = max(T_plc-peer-detection,
    /// T_io-owner-lease-expiry)`.
    claim_start: u64,
    /// The scan term feeding the phase-aware safe-point estimate.
    scan: TermStatsPayload,
    /// The commissioning baseline: the RTT max frozen at completion.
    #[serde(skip_serializing_if = "Option::is_none")]
    baseline_rtt_max: Option<u64>,
    /// How many recalibration runs followed the first commissioning.
    recalibrations: u64,
    /// The last invalidation reason (the recalibration trigger).
    #[serde(skip_serializing_if = "Option::is_none")]
    last_invalidation: Option<&'static str>,
}

/// One module's row of the barrier view: the profile, the ownership
/// truth, and the measured T contribution.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ModuleBarrierPayload {
    /// The module identity.
    module: u16,
    /// The connection profile of the takeover path: `reconnect` (a
    /// fresh exclusive connection per takeover) or `preconnected`.
    profile: &'static str,
    /// Whether the module is powered and communicating.
    online: bool,
    /// The ownership state: unowned / claimedDisarmed / armed.
    owner_state: &'static str,
    /// The owning ControllerId, if any originator holds the module.
    #[serde(skip_serializing_if = "Option::is_none")]
    owner: Option<u64>,
    /// The ownership epoch stamped on the module, if held.
    #[serde(skip_serializing_if = "Option::is_none")]
    owner_epoch: Option<u32>,
    /// Whether the holder commands the module's outputs.
    owner_armed: bool,
    /// The claim latency (`T_claim,i`; the barrier claims sequentially).
    claim: TermStatsPayload,
    /// The ARM latency (`T_arm,i`).
    arm: TermStatsPayload,
    /// The output-apply latency (`T_output-apply,i`).
    output_apply: TermStatsPayload,
}

/// The payload of [`HaResponse::HaBarrier`]: the fencing state of every
/// required I/O module and the cost of the barrier, device by device.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct HaBarrierPayload {
    /// Per-module rows, in the fixed configured claim order.
    modules: Vec<ModuleBarrierPayload>,
    /// The module whose qualification-bound claim dominates the
    /// barrier; the device the takeover estimate names.
    #[serde(skip_serializing_if = "Option::is_none")]
    limiting_device: Option<u16>,
    /// The calculated worst-case ownership recovery; `None` until the
    /// first calibration qualifies a budget.
    #[serde(skip_serializing_if = "Option::is_none")]
    worst_ownership_recovery: Option<u64>,
}

/// The payload of [`HaResponse::HaIoReady`]: the `IO_READY` breakdown.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct HaIoReadyPayload {
    /// Every required input is observable.
    required_inputs_observable: bool,
    /// The standby connections are valid.
    standby_connections_valid: bool,
    /// Both units hold one application generation.
    configs_match: bool,
    /// The pair's epochs are aligned.
    epochs_valid: bool,
    /// The first failing item, when any input is false.
    #[serde(skip_serializing_if = "Option::is_none")]
    failing_item: Option<&'static str>,
}

/// One failover-formula term as the contract renders it: the
/// qualification bound against the measured current / EMA.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct TermPayload {
    /// The qualification bound (`MaxQualified` — the budget check's
    /// input).
    bound: u64,
    /// The measured current value.
    current: u64,
    /// The measured fast EMA (EMA10).
    ema10: u64,
}

/// The phase-aware safe-point term: the scan statistics behind
/// `T_safepoint` plus the current scan phase (ADR-0062's
/// `T_safepoint = T_next-commit - t_takeover`, bounded
/// `0 <= T_safepoint <= T_scan,max`).
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ScanTermPayload {
    /// The qualification bound (`T_scan,max`).
    bound: u64,
    /// The measured current scan interval.
    current: u64,
    /// The measured fast EMA (EMA10).
    ema10: u64,
    /// The measured slow EMA (EMA100).
    ema100: u64,
    /// The smallest observed interval — the jitter envelope's low edge.
    min: u64,
    /// The largest observed interval — the jitter envelope's high edge.
    max: u64,
    /// The current scan phase (ticks since the last committed
    /// boundary; 0 before the first commit).
    phase: u64,
}

/// The budget-check verdict: qualified, or the minimum demonstrated
/// budget the installation can honestly honor.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct BudgetVerdictPayload {
    /// Whether the configured budget meets the ADR's inequality.
    qualified: bool,
    /// The minimum demonstrated budget (the calculated worst case);
    /// present exactly when `qualified` is false.
    #[serde(skip_serializing_if = "Option::is_none")]
    minimum_demonstrated: Option<u64>,
}

/// The payload of [`HaResponse::HaTimingBudget`]: the honest failover
/// estimate — every formula term as a measured number, the configured
/// budget, and the verdict.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct HaTimingBudgetPayload {
    /// The configured peer-failure confirmation time (ticks).
    peer_failure_confirmation: u64,
    /// The configured maximum process-recovery budget (ticks).
    recovery_budget: u64,
    /// `T_peer-detect` — the claim-start, both independent proofs
    /// agreeing.
    peer_detect: TermPayload,
    /// `T_claim` — the sequential sum over the modules.
    claim: TermPayload,
    /// `T_arm` — the slowest device's ARM latency.
    arm: TermPayload,
    /// `T_scan-safe-point` — the phase-aware safe-point term.
    scan: ScanTermPayload,
    /// `T_output-apply` — the slowest device's output latency.
    output_apply: TermPayload,
    /// `T_recovery` with the qualification bounds — the calculated
    /// worst case (the minimum demonstrated budget on a failing check).
    #[serde(skip_serializing_if = "Option::is_none")]
    calculated_worst_case: Option<u64>,
    /// The predicted recovery if the failover happened now (the online
    /// estimator); `None` while uncalibrated.
    #[serde(skip_serializing_if = "Option::is_none")]
    predicted_if_now: Option<u64>,
    /// The budget-check verdict; `None` until the first calibration
    /// qualifies a budget.
    #[serde(skip_serializing_if = "Option::is_none")]
    verdict: Option<BudgetVerdictPayload>,
}

/// One timestamped event of the ring.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct HaEventPayload {
    /// The simulation tick the event was recorded at.
    tick: u64,
    /// What happened (the event vocabulary, camelCase).
    kind: &'static str,
    /// The guard-relevant detail.
    #[serde(skip_serializing_if = "Option::is_none")]
    detail: Option<String>,
}

/// The payload of [`HaResponse::HaEvents`]: the bounded timestamped
/// ring, read on poll.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct HaEventsPayload {
    /// How many events were ever recorded: clients dedup polls by the
    /// count and read the ring appended, never rewritten.
    count: u64,
    /// The ring, oldest first, bounded.
    events: Vec<HaEventPayload>,
}

/// Why an HA command failed: a stable V-code from the crate's CSV plus
/// the shell's own description.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct HaCommandError {
    /// The stable V-code (e.g. `"V4108"`).
    #[serde(rename = "vCode")]
    v_code: &'static str,
    /// What failed, in the shell's vocabulary.
    message: String,
    /// The minimum demonstrated budget a V4111 budget refusal carries.
    #[serde(skip_serializing_if = "Option::is_none")]
    minimum_demonstrated: Option<u64>,
}

/// The answer to one [`HaCommand`].
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(tag = "response", rename_all = "camelCase")]
pub enum HaResponse {
    /// The pair overview (the answer to [`HaCommand::HaStatus`]).
    HaStatus(Box<HaStatusPayload>),
    /// The calibration state (the answer to [`HaCommand::HaCalibration`]).
    HaCalibration(Box<HaCalibrationPayload>),
    /// The barrier view (the answer to [`HaCommand::HaBarrier`]).
    HaBarrier(HaBarrierPayload),
    /// The `IO_READY` breakdown (the answer to [`HaCommand::HaIoReady`]).
    HaIoReady(HaIoReadyPayload),
    /// The timing budget (the answer to [`HaCommand::HaTimingBudget`]).
    HaTimingBudget(Box<HaTimingBudgetPayload>),
    /// The event ring (the answer to [`HaCommand::HaEvents`]).
    HaEvents(HaEventsPayload),
    /// The command succeeded; there is nothing further to report.
    Ack,
    /// The command failed; carries a stable V-code and the message.
    Error(HaCommandError),
}

/// Parses one line-delimited HA command: a single JSON value without
/// its trailing newline.
pub fn parse_ha_command(line: &str) -> Result<HaCommand, serde_json::Error> {
    serde_json::from_str(line)
}

/// Renders one HA response as a single line of JSON, without a trailing
/// newline.
pub fn render_ha_response(response: &HaResponse) -> Result<String, serde_json::Error> {
    serde_json::to_string(response)
}

/// Executes `command` against `shell` and returns the response to send.
///
/// This is the whole command mapping: each variant renders the matching
/// shell snapshot, and each action delegates to the one shell call —
/// the shell's charts and engine stay the only HA state.
pub fn execute(command: HaCommand, shell: &mut Shell) -> HaResponse {
    match command {
        HaCommand::HaStatus => HaResponse::HaStatus(Box::new(status_payload(shell))),
        HaCommand::HaCalibration => HaResponse::HaCalibration(Box::new(calibration_payload(shell))),
        HaCommand::HaBarrier => HaResponse::HaBarrier(barrier_payload(shell)),
        HaCommand::HaIoReady => HaResponse::HaIoReady(io_ready_payload(shell)),
        HaCommand::HaTimingBudget => {
            HaResponse::HaTimingBudget(Box::new(timing_budget_payload(shell)))
        }
        HaCommand::HaEvents => HaResponse::HaEvents(events_payload(shell)),
        HaCommand::HaCommandedSwap => match shell.commanded_swap() {
            Ok(SwapOutcome::Completed) => HaResponse::Ack,
            Ok(SwapOutcome::BarrierFailed) => HaResponse::Error(HaCommandError {
                v_code: problem_codes::OWNERSHIP_BARRIER_FAILED,
                message: "the commanded swap failed the OWNERSHIP_BARRIER: everything acquired \
                          was released and the pair is in REDUNDANCY_LOST"
                    .to_string(),
                minimum_demonstrated: None,
            }),
            Err(Refusal::Standalone) => HaResponse::Error(HaCommandError {
                v_code: problem_codes::PAIR_REQUIRED,
                message: "the commanded swap requires a configured redundant pair; this unit \
                          is standalone"
                    .to_string(),
                minimum_demonstrated: None,
            }),
            Err(Refusal::NotSyncReady) => HaResponse::Error(HaCommandError {
                v_code: problem_codes::SWAP_REFUSED,
                message: "the commanded role swap was refused: the pair is not in SYNC_READY"
                    .to_string(),
                minimum_demonstrated: None,
            }),
        },
        HaCommand::HaRunCalibration => match shell.run_calibration() {
            Ok(()) => HaResponse::Ack,
            Err(Refusal::Standalone) => HaResponse::Error(HaCommandError {
                v_code: problem_codes::PAIR_REQUIRED,
                message: "a calibration run requires a configured redundant pair; this unit \
                          is standalone"
                    .to_string(),
                minimum_demonstrated: None,
            }),
            Err(_) => HaResponse::Error(HaCommandError::internal()),
        },
        HaCommand::HaSetTimingBudget {
            peer_failure_confirmation,
            recovery_budget,
        } => match shell.set_timing_budget(peer_failure_confirmation, recovery_budget) {
            Ok(BudgetVerdict::Qualified) => HaResponse::Ack,
            Ok(BudgetVerdict::MinimumDemonstrated(minimum)) => HaResponse::Error(HaCommandError {
                v_code: problem_codes::TIMING_GUARANTEE_LOST,
                message: format!(
                    "the configured recovery budget cannot be honored: the minimum \
                         demonstrated budget is {minimum} ticks; the configuration was not \
                         applied"
                ),
                minimum_demonstrated: Some(minimum),
            }),
            Err(Refusal::Standalone) => HaResponse::Error(HaCommandError {
                v_code: problem_codes::PAIR_REQUIRED,
                message: "a timing budget requires a configured redundant pair; this unit is \
                          standalone"
                    .to_string(),
                minimum_demonstrated: None,
            }),
            Err(_) => HaResponse::Error(HaCommandError::internal()),
        },
    }
}

impl HaCommandError {
    /// The invariant refusal: a code the CSV does not carry, matching
    /// the runtime layer's shape (built directly, never on the wire in
    /// a correct composition).
    fn internal() -> Self {
        HaCommandError {
            v_code: "V9000",
            message: "internal error: the HA shell refused an action for no coded reason"
                .to_string(),
            minimum_demonstrated: None,
        }
    }
}

/// The pair overview payload from the shell's live state.
fn status_payload(shell: &Shell) -> HaStatusPayload {
    let standalone = !shell.has_pair();
    let calibration = shell.calibration_status();
    let io = shell.io_ready();
    let (performance_degraded, timing_guarantee_lost, redundancy_lost) = shell.alarm_flags();
    HaStatusPayload {
        standalone,
        pair_id: shell.config().pair_id.map(|id| id.to_string()),
        local: unit_payload(shell, Side::Local),
        peer: if standalone {
            None
        } else {
            Some(unit_payload(shell, Side::Peer))
        },
        epoch: if standalone {
            None
        } else {
            Some(shell.epoch().raw())
        },
        application_generation: shell.application_generation(),
        state_generation: shell.state_generation(),
        takeover_ready: calibration.takeover_ready(),
        sync_ready: shell
            .unit_view(Side::Local)
            .is_some_and(|u| u.sync == SyncState::SyncReady),
        io_ready: io.all(),
        link_valid: calibration.link_valid(),
        alarms: AlarmFlagsPayload {
            performance_degraded,
            timing_guarantee_lost,
            redundancy_lost,
        },
    }
}

/// The pair overview payload from the production pair link's status view
/// (the real two-process composition; the engineering session of
/// `ironplcvm serve` in pair mode renders this for `haStatus`).
///
/// What the link slice answers honestly: the SYNC chart, the epoch, the
/// wire generation counters, the peer as observed on the wire, and link
/// validity. What it does not model — the fencing/CONTROL charts and the
/// calibration/IO_READY chain are the simulator binding's surface — it
/// renders absent or `false`, never guessed: `takeoverReady` stays false
/// without the qualification machinery, and the peer's internal chart
/// states are not on the wire.
pub fn pair_link_status(status: &PairLinkStatus) -> HaStatusPayload {
    let peer = status.peer.map(|view| UnitPayload {
        role: Some(view.role.as_str()),
        controller_id: 0,
        sync: None,
        sync_reason: None,
        control: None,
        alarm: None,
        epoch: view.epoch.raw(),
    });
    HaStatusPayload {
        standalone: false,
        pair_id: Some(status.pair_id.to_string()),
        local: UnitPayload {
            role: Some(status.configured_role.as_str()),
            controller_id: 0,
            sync: Some(status.sync.as_str()),
            sync_reason: Some(status.sync_reason.as_str()),
            control: None,
            alarm: None,
            epoch: status.epoch.raw(),
        },
        peer,
        epoch: Some(status.epoch.raw()),
        application_generation: status.application_generation,
        state_generation: status.state_generation,
        takeover_ready: false,
        sync_ready: status.sync == SyncState::SyncReady,
        io_ready: false,
        link_valid: status.link_valid,
        alarms: AlarmFlagsPayload {
            performance_degraded: false,
            timing_guarantee_lost: false,
            redundancy_lost: false,
        },
    }
}

/// One unit's pair-overview block; standalone renders the statechart
/// fields absent.
fn unit_payload(shell: &Shell, side: Side) -> UnitPayload {
    let view = shell.unit_view(side);
    let standalone = !shell.has_pair();
    UnitPayload {
        role: if standalone {
            None
        } else {
            view.map(|u| u.role.as_str())
        },
        controller_id: view.map_or(0, |u| u.owner.raw()),
        sync: if standalone {
            None
        } else {
            view.map(|u| u.sync.as_str())
        },
        sync_reason: if standalone {
            None
        } else {
            view.map(|u| u.sync_reason.as_str())
        },
        control: if standalone {
            None
        } else {
            view.map(|u| u.control.as_str())
        },
        alarm: view.and_then(|u| u.alarm.map(|alarm| alarm.as_str())),
        epoch: view.map_or(0, |u| u.epoch.raw()),
    }
}

/// The calibration payload from the engine's live status snapshot.
fn calibration_payload(shell: &Shell) -> HaCalibrationPayload {
    let status = shell.calibration_status();
    HaCalibrationPayload {
        state: status.state().as_str(),
        link_valid: status.link_valid(),
        takeover_ready: status.takeover_ready(),
        ab: DirectionPayload::from(status.rtt_ab()),
        ba: DirectionPayload::from(status.rtt_ba()),
        peer_detect: TermStatsPayload::from(status.peer_detect()),
        lease_expiry: status.lease_expiry(),
        claim_start: status.claim_start(),
        scan: TermStatsPayload::from(status.scan()),
        baseline_rtt_max: status.baseline_rtt_max(),
        recalibrations: status.recalibrations(),
        last_invalidation: status.last_invalidation().map(|reason| reason.as_str()),
    }
}

/// The barrier payload: the registry's ownership truth joined with the
/// engine's per-module contributions, in the configured claim order.
fn barrier_payload(shell: &Shell) -> HaBarrierPayload {
    let status = shell.calibration_status();
    let ownership = shell.ownership();
    let profile = shell.ownership_mode().as_profile_str();
    let modules = shell
        .required_modules()
        .iter()
        .map(|module| {
            let state = ownership
                .iter()
                .find(|entry| entry.module == *module)
                .map_or(ModuleState::Unowned, |entry| entry.state);
            let contribution = status
                .modules()
                .iter()
                .find(|entry| entry.module() == *module);
            let term = |stats: Option<&TermStats>| {
                stats.map_or_else(
                    || TermStatsPayload::from(&TermStats::new()),
                    TermStatsPayload::from,
                )
            };
            ModuleBarrierPayload {
                module: module.raw(),
                profile,
                online: shell.module_online(*module),
                owner_state: state.as_str(),
                owner: state.owner().map(|owner| owner.raw()),
                owner_epoch: state.epoch().map(|epoch| epoch.raw()),
                owner_armed: state.is_armed(),
                claim: term(contribution.map(|c| c.claim())),
                arm: term(contribution.map(|c| c.arm())),
                output_apply: term(contribution.map(|c| c.output_apply())),
            }
        })
        .collect();
    HaBarrierPayload {
        modules,
        limiting_device: status.limiting_device().map(|module| module.raw()),
        worst_ownership_recovery: status.budget().map(|budget| budget.calculated_worst_case()),
    }
}

/// The `IO_READY` breakdown payload.
fn io_ready_payload(shell: &Shell) -> HaIoReadyPayload {
    let io = shell.io_ready();
    HaIoReadyPayload {
        required_inputs_observable: io.required_inputs,
        standby_connections_valid: io.standby_connections,
        configs_match: io.configs_match,
        epochs_valid: io.epochs_valid,
        failing_item: io.failing,
    }
}

/// The timing-budget payload: every formula term as the qualification
/// bound against the measured current / EMA, the configured engineer
/// parameters, and the budget-check verdict.
fn timing_budget_payload(shell: &Shell) -> HaTimingBudgetPayload {
    let status = shell.calibration_status();
    let modules = status.modules();
    let sum = |term: fn(&crate::timing::ModuleContribution) -> &TermStats,
               f: fn(&TermStats) -> u64| {
        modules
            .iter()
            .map(|module| f(term(module)))
            .fold(0u64, u64::saturating_add)
    };
    let max_of = |term: fn(&crate::timing::ModuleContribution) -> &TermStats,
                  f: fn(&TermStats) -> u64| {
        modules
            .iter()
            .map(|module| f(term(module)))
            .max()
            .unwrap_or(0)
    };
    let scan = status.scan();
    let budget = status.budget();
    HaTimingBudgetPayload {
        peer_failure_confirmation: shell.peer_failure_confirmation(),
        recovery_budget: shell.configured_budget(),
        peer_detect: TermPayload {
            bound: status.claim_start(),
            current: status.peer_detect().current().max(status.lease_expiry()),
            ema10: status.peer_detect().ema10().max(status.lease_expiry()),
        },
        claim: TermPayload {
            bound: sum(|m| m.claim(), TermStats::max),
            current: sum(|m| m.claim(), TermStats::current),
            ema10: sum(|m| m.claim(), TermStats::ema10),
        },
        arm: TermPayload {
            bound: max_of(|m| m.arm(), TermStats::max),
            current: max_of(|m| m.arm(), TermStats::current),
            ema10: max_of(|m| m.arm(), TermStats::ema10),
        },
        scan: ScanTermPayload {
            bound: scan.max(),
            current: scan.current(),
            ema10: scan.ema10(),
            ema100: scan.ema100(),
            min: scan.min(),
            max: scan.max(),
            phase: shell.scan_phase(),
        },
        output_apply: TermPayload {
            bound: max_of(|m| m.output_apply(), TermStats::max),
            current: max_of(|m| m.output_apply(), TermStats::current),
            ema10: max_of(|m| m.output_apply(), TermStats::ema10),
        },
        calculated_worst_case: budget.map(|budget| budget.calculated_worst_case()),
        predicted_if_now: shell.predicted_if_now(),
        verdict: budget.map(|budget| match budget.verdict() {
            BudgetVerdict::Qualified => BudgetVerdictPayload {
                qualified: true,
                minimum_demonstrated: None,
            },
            BudgetVerdict::MinimumDemonstrated(minimum) => BudgetVerdictPayload {
                qualified: false,
                minimum_demonstrated: Some(minimum),
            },
        }),
    }
}

/// The events payload: the bounded ring plus the ever-recorded count.
fn events_payload(shell: &Shell) -> HaEventsPayload {
    let (count, events) = shell.events();
    HaEventsPayload {
        count,
        events: events
            .iter()
            .map(|event: &HaEvent| HaEventPayload {
                tick: event.tick,
                kind: event.kind.as_str(),
                detail: event.detail.clone(),
            })
            .collect(),
    }
}
