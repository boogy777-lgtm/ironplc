//! Tests for the calibration state machine, budget formulas and guarantee monitors.

use super::*;

/// The hand-computed vector the budget tests share: detection 4,
/// lease expiry 2, modules claiming 2/3/5, arming 1/1/2, applying
/// 1/2/1, scan 1.
fn vector(budget: u64) -> Calibration {
    let mut calibration = Calibration::new(budget);
    calibration.begin();
    calibration.record_peer_detect(4);
    calibration.record_lease_expiry(2);
    calibration.record_scan(1);
    calibration.record_module_timing(ModuleId::new(0), 2, 1, 1);
    calibration.record_module_timing(ModuleId::new(1), 3, 1, 2);
    calibration.record_module_timing(ModuleId::new(2), 5, 2, 1);
    calibration.record_rtt(Direction::AToB, 2);
    calibration.record_rtt(Direction::BToA, 2);
    calibration.complete();
    calibration
}

/// The configured budget of the engine's live status snapshot.
fn status_configured_budget(calibration: &Calibration) -> u64 {
    calibration
        .status(true, true)
        .budget()
        .copied()
        .unwrap()
        .configured_budget()
}

#[test]
fn budget_when_hand_vector_then_adr_formulas() {
    let calibration = vector(17);

    let budget = calibration.status(true, true).budget().copied().unwrap();
    // T_claim-start = max(4, 2) = 4; T_claim,max = 2+3+5 = 10;
    // T_arm = max(1,1,2) = 2; T_scan,max = 1; T_output,max = 2.
    assert_eq!(budget.peer_detect(), 4);
    assert_eq!(budget.claim(), 10);
    assert_eq!(budget.arm(), 2);
    assert_eq!(budget.scan_max(), 1);
    assert_eq!(budget.output_apply(), 2);
    // T_recovery = 4 + 10 + 2 + 1 + 2 = 19.
    assert_eq!(budget.calculated_worst_case(), 19);
    // The ADR inequality: 4 + 10 + 1 + 2 = 17 <= 17.
    assert_eq!(budget.verdict(), BudgetVerdict::Qualified);
    assert_eq!(budget.configured_budget(), 17);
}

#[test]
fn budget_when_budget_below_check_then_minimum_demonstrated() {
    // The check fails at 16 (17 <= 16 is false): the minimum
    // demonstrated budget is the calculated worst case, 19.
    let calibration = vector(16);

    let budget = calibration.status(true, true).budget().copied().unwrap();

    assert_eq!(budget.verdict(), BudgetVerdict::MinimumDemonstrated(19));
    assert_eq!(budget.calculated_worst_case(), 19);
}

#[test]
fn budget_when_never_completed_then_absent_and_not_ready() {
    let mut calibration = Calibration::new(100);
    calibration.begin();
    calibration.record_peer_detect(4);

    let status = calibration.status(true, true);

    assert_eq!(status.state(), CalibrationState::Calibrating);
    assert_eq!(status.budget(), None);
    assert!(!status.link_valid());
    assert!(!status.takeover_ready());
}

#[test]
fn status_when_verdict_inputs_then_takeover_ready_formula() {
    let calibration = vector(100);

    // TakeoverReady = SYNC_READY && IO_READY && RedundancyLinkValid.
    assert!(calibration.status(true, true).takeover_ready());
    assert!(!calibration.status(false, true).takeover_ready());
    assert!(!calibration.status(true, false).takeover_ready());
    assert!(calibration.status(true, true).link_valid());
    assert_eq!(calibration.state(), CalibrationState::Calibrated);
}

#[test]
fn status_when_limiting_device_then_dominant_claim_module() {
    let calibration = vector(100);

    let status = calibration.status(true, true);

    // Module 2's claim bound (5) dominates 2 and 3.
    assert_eq!(status.limiting_device(), Some(ModuleId::new(2)));
    assert_eq!(status.modules().len(), 3);
    assert_eq!(status.claim_start(), 4);
    assert_eq!(status.lease_expiry(), 2);
    assert_eq!(status.peer_detect().max(), 4);
}

#[test]
fn status_when_no_modules_then_no_limiting_device() {
    let mut calibration = Calibration::new(100);
    calibration.begin();
    calibration.record_peer_detect(4);
    calibration.complete();

    let status = calibration.status(true, true);

    assert_eq!(status.limiting_device(), None);
    assert_eq!(status.budget().copied().unwrap().claim(), 0);
}

#[test]
fn monitor_when_rtt_leaves_envelope_then_degraded_latched() {
    let mut calibration = vector(100);
    assert!(!calibration.status(true, true).degraded());

    // Reality leaves the calibrated envelope (2): a current sample
    // of 10 is the spike.
    calibration.record_rtt(Direction::AToB, 10);

    let status = calibration.status(true, true);
    assert!(status.degraded());
    assert!(!status.guarantee_lost());
    // Degradation does not invalidate the link: redundancy still
    // works, the alarm surfaces the drift.
    assert!(status.link_valid());
    assert!(status.takeover_ready());
    assert_eq!(TimingAlarm::PerformanceDegraded.v_code(), "V4110");
}

#[test]
fn monitor_when_terms_outgrow_budget_then_guarantee_lost_latched() {
    let mut calibration = vector(17);
    assert_eq!(
        calibration
            .status(true, true)
            .budget()
            .copied()
            .unwrap()
            .verdict(),
        BudgetVerdict::Qualified
    );

    // Scans slow to 20 ticks: the check becomes 4 + 10 + 20 + 2 = 36
    // > 17 — the recovery budget can no longer be met.
    calibration.record_scan(20);

    let status = calibration.status(true, true);
    assert!(status.guarantee_lost());
    assert!(!status.degraded());
    assert!(!status.link_valid());
    assert!(!status.takeover_ready());
    assert_eq!(
        status.budget().copied().unwrap().verdict(),
        BudgetVerdict::MinimumDemonstrated(38)
    );
    assert_eq!(TimingAlarm::GuaranteeLost.v_code(), "V4111");
}

#[test]
fn monitor_when_uncalibrated_then_no_alarms_latched() {
    let mut calibration = Calibration::new(1);
    calibration.begin();
    // Far past any envelope — but there is no envelope yet: an
    // uncalibrated pair reports no timing-health alarms.
    calibration.record_rtt(Direction::AToB, 1000);
    calibration.record_scan(1000);

    let status = calibration.status(true, true);
    assert!(!status.degraded());
    assert!(!status.guarantee_lost());
}

#[test]
fn begin_when_recalibrating_then_previous_guarantee_invalid() {
    let mut calibration = vector(100);
    assert!(calibration.status(true, true).takeover_ready());

    calibration.begin();

    let status = calibration.status(true, true);
    assert_eq!(status.state(), CalibrationState::Calibrating);
    assert!(!status.link_valid());
    assert!(!status.takeover_ready());
    assert_eq!(status.budget(), None);
    assert_eq!(status.recalibrations(), 1);
}

#[test]
fn begin_when_first_run_then_no_recalibration_counted() {
    let mut calibration = Calibration::new(100);

    calibration.begin();

    assert_eq!(calibration.status(true, true).recalibrations(), 0);
}

#[test]
fn invalidate_when_significant_change_then_unqualified_with_reason() {
    let mut calibration = vector(100);

    calibration.invalidate(InvalidationReason::TopologyChanged);

    let status = calibration.status(true, true);
    assert_eq!(status.state(), CalibrationState::Unqualified);
    assert!(!status.link_valid());
    assert!(!status.takeover_ready());
    assert_eq!(status.budget(), None);
    assert_eq!(
        status.last_invalidation(),
        Some(InvalidationReason::TopologyChanged)
    );
    assert_eq!(status.recalibrations(), 0);
}

#[test]
fn predicted_if_now_when_calibrated_then_ema_terms_plus_bounded_safepoint() {
    let calibration = vector(100);

    // detect max(ema10 4, lease 2) = 4; claim ema 10; arm ema 2;
    // output ema 2; safepoint = scan ema10 1 - phase.
    assert_eq!(calibration.predicted_if_now(0), Some(19));
    // A phase past the scan period floors the safe-point at 0.
    assert_eq!(calibration.predicted_if_now(5), Some(18));
}

#[test]
fn predicted_if_now_when_not_calibrated_then_absent() {
    let mut calibration = Calibration::new(100);
    calibration.begin();

    assert_eq!(calibration.predicted_if_now(0), None);
}

#[test]
fn timing_alarm_v_codes_when_surfaced_then_stable_codes() {
    assert_eq!(TimingAlarm::PerformanceDegraded.v_code(), "V4110");
    assert_eq!(TimingAlarm::GuaranteeLost.v_code(), "V4111");
}

#[test]
fn set_budget_when_raised_then_guarantee_latch_holds_until_recalibration() {
    let mut calibration = vector(16);
    assert!(calibration.status(true, true).guarantee_lost());

    calibration.set_budget(100);
    // The latch is designed behavior: only a completed
    // recalibration restores the guarantee, never a parameter
    // edit (ADR-0062: alarms, not silent adjustment).
    assert!(calibration.status(true, true).guarantee_lost());

    calibration.begin();
    calibration.complete();

    assert!(!calibration.status(true, true).guarantee_lost());
    assert_eq!(status_configured_budget(&calibration), 100);
}

#[test]
fn set_budget_when_lowered_then_guarantee_lost_at_once() {
    let mut calibration = vector(100);
    assert!(!calibration.status(true, true).guarantee_lost());

    // A setting the live measurements cannot honor surfaces
    // immediately, never at the next sample.
    calibration.set_budget(1);

    assert!(calibration.status(true, true).guarantee_lost());
}

#[test]
fn baseline_rtt_max_when_calibrated_then_frozen_envelope() {
    let calibration = vector(100);

    // The hand vector's link samples are 2 per direction: the
    // commissioning baseline is the frozen RTT max (2).
    assert_eq!(calibration.status(true, true).baseline_rtt_max(), Some(2));
}

#[test]
fn baseline_rtt_max_when_unqualified_then_absent() {
    let mut calibration = Calibration::new(100);
    calibration.begin();

    assert_eq!(calibration.status(true, true).baseline_rtt_max(), None);
}
