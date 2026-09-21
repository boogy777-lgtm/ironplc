//! Calibration-engine scenarios over the loopback/simulator bindings:
//! the ADR-0062 budget math against hand-computed vectors, the
//! limiting-device selection, the commissioning run's reproducibility,
//! and the guarantee monitors' alarm crossings.
//!
//! The composition is the calibration slice's own: the deterministic
//! commissioning run (`run_calibration`) produces the calibrated engine,
//! and the test then feeds live samples the way the shell will. Timing
//! is the abstract tick domain of the crate (one tick = one driver
//! step). The hand-computed vector: confirmation 2 + missed 2 gives a
//! 4-tick peer detection; the registry's old-connection timeout (3) is
//! the lease expiry, so the claim-start is 4; modules claim 2 and 5
//! (sum 7), arm 1 and 2 (max 2), apply 1 and 1 (max 1); the scan
//! cadence is 1. The ADR inequality: 4 + 7 + 1 + 1 = 13; the recovery
//! worst case: 4 + 7 + 2 + 1 + 1 = 15.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use ironplc_redundancy::{
    run_calibration, BudgetVerdict, Calibration, CalibrationState, ConfiguredRole, Direction,
    InvalidationReason, ModuleId, ModuleRegistry, ModuleTiming, PairId, RedundancyConfig,
    TimingAlarm,
};

/// The commissioning composition under test: a (2,2)-threshold pair
/// link, a registry of two modules with firmware-reported delays, and
/// the target's old-connection timeout at 3 ticks.
fn commissioning() -> (RedundancyConfig, ModuleRegistry, [ModuleId; 2]) {
    let config = RedundancyConfig::pair(PairId::new(1), ConfiguredRole::Primary)
        .with_confirmation_exchanges(2)
        .with_missed_exchanges(2)
        .with_lease_ttl(4);
    let registry = ModuleRegistry::new(2)
        .with_connection_timeout(3)
        .with_module_timing(
            ModuleId::new(0),
            ModuleTiming {
                claim_ticks: 2,
                arm_ticks: 1,
                output_apply_ticks: 1,
            },
        )
        .with_module_timing(
            ModuleId::new(1),
            ModuleTiming {
                claim_ticks: 5,
                arm_ticks: 2,
                output_apply_ticks: 1,
            },
        );
    (config, registry, [ModuleId::new(0), ModuleId::new(1)])
}

/// Runs the commissioning calibration with the given configured budget.
fn calibrated(budget: u64) -> Calibration {
    let (config, registry, required) = commissioning();
    run_calibration(&config, budget, &registry, &required)
}

#[test]
fn calibration_when_commissioned_then_measured_terms_match_hand_vector() {
    let calibration = calibrated(100);

    let status = calibration.status(true, true);
    assert_eq!(status.state(), CalibrationState::Calibrated);
    // The link profile: 2-tick loopback round trips, no steady loss.
    assert_eq!(status.rtt_ab().rtt().max(), 2);
    assert_eq!(status.rtt_ba().rtt().max(), 2);
    assert_eq!(status.rtt_ab().loss_rate_percent(), 0);
    assert_eq!(status.rtt_ab().max_consecutive_loss(), 0);
    // Detection measured by the drill; lease expiry from the target.
    assert_eq!(status.peer_detect().max(), 4);
    assert_eq!(status.lease_expiry(), 3);
    assert_eq!(status.claim_start(), 4);
    assert_eq!(status.scan().max(), 1);
    // The limiting device is the module whose claim dominates.
    assert_eq!(status.limiting_device(), Some(ModuleId::new(1)));
    assert_eq!(status.modules().len(), 2);
    // TakeoverReady = SYNC_READY && IO_READY && RedundancyLinkValid.
    assert!(status.link_valid());
    assert!(status.takeover_ready());
    assert!(!calibration.status(false, true).takeover_ready());
    assert!(!calibration.status(true, false).takeover_ready());
}

#[test]
fn calibration_when_budget_at_adr_check_boundary_then_verdicts_exact() {
    // The ADR inequality: 4 + 7 + 1 + 1 = 13.
    let qualified = calibrated(13);
    let budget = qualified.status(true, true).budget().copied().unwrap();
    assert_eq!(budget.verdict(), BudgetVerdict::Qualified);
    assert_eq!(budget.calculated_worst_case(), 15);
    assert_eq!(budget.configured_budget(), 13);
    assert!(qualified.status(true, true).takeover_ready());

    // One tick below the check: the minimum demonstrated budget is the
    // calculated worst case, and the guarantee is lost at birth.
    let failing = calibrated(12);
    let budget = failing.status(true, true).budget().copied().unwrap();
    assert_eq!(budget.verdict(), BudgetVerdict::MinimumDemonstrated(15));
    let status = failing.status(true, true);
    assert!(status.guarantee_lost());
    assert!(!status.link_valid());
    assert!(!status.takeover_ready());
}

#[test]
fn calibration_when_run_twice_then_identical_status() {
    let first = calibrated(100);
    let second = calibrated(100);

    assert_eq!(first.status(true, true), second.status(true, true));
}

#[test]
fn calibration_when_rtt_leaves_envelope_then_degraded_latched() {
    let mut calibration = calibrated(100);
    assert!(!calibration.status(true, true).degraded());

    // The calibrated envelope is a 2-tick round trip; a 10-tick sample
    // is reality leaving it.
    calibration.record_rtt(Direction::AToB, 10);

    let status = calibration.status(true, true);
    assert!(status.degraded());
    assert!(!status.guarantee_lost());
    // Degradation alarms while redundancy still works: the link stays
    // valid and the pair stays takeover-ready.
    assert!(status.link_valid());
    assert!(status.takeover_ready());
    assert_eq!(TimingAlarm::PerformanceDegraded.v_code(), "V4110");
}

#[test]
fn calibration_when_terms_outgrow_budget_then_guarantee_lost_latched() {
    let mut calibration = calibrated(13);
    assert!(calibration.status(true, true).takeover_ready());

    // Scans slow from 1 to 20 ticks: the check becomes 4 + 7 + 20 + 1 =
    // 32 > 13; the worst case becomes 4 + 7 + 2 + 20 + 1 = 34.
    calibration.record_scan(20);

    let status = calibration.status(true, true);
    assert!(status.guarantee_lost());
    assert!(!status.degraded());
    assert!(!status.link_valid());
    assert!(!status.takeover_ready());
    assert_eq!(
        status.budget().copied().unwrap().verdict(),
        BudgetVerdict::MinimumDemonstrated(34)
    );
    assert_eq!(TimingAlarm::GuaranteeLost.v_code(), "V4111");
}

#[test]
fn calibration_when_recalibrated_then_previous_guarantee_invalid() {
    let mut calibration = calibrated(100);
    assert!(calibration.status(true, true).takeover_ready());

    // A new calibration run invalidates the guarantee the moment it
    // starts (ADR-0062: readiness is calibration-gated).
    calibration.begin();

    let status = calibration.status(true, true);
    assert_eq!(status.state(), CalibrationState::Calibrating);
    assert!(!status.link_valid());
    assert!(!status.takeover_ready());
    assert_eq!(status.budget(), None);
    assert_eq!(status.recalibrations(), 1);

    // Completing the run produces a fresh valid profile.
    calibration.complete();
    let status = calibration.status(true, true);
    assert_eq!(status.state(), CalibrationState::Calibrated);
    assert!(status.link_valid());
    assert!(status.takeover_ready());
}

#[test]
fn calibration_when_significant_change_then_invalidation_recorded() {
    let mut calibration = calibrated(100);

    calibration.invalidate(InvalidationReason::ProtocolVersionChanged);

    let status = calibration.status(true, true);
    assert_eq!(status.state(), CalibrationState::Unqualified);
    assert!(!status.link_valid());
    assert!(!status.takeover_ready());
    assert_eq!(status.budget(), None);
    assert_eq!(
        status.last_invalidation(),
        Some(InvalidationReason::ProtocolVersionChanged)
    );

    // Requalification: a fresh run restores readiness.
    calibration.begin();
    calibration.record_peer_detect(4);
    calibration.record_lease_expiry(3);
    calibration.record_scan(1);
    calibration.record_module_timing(ModuleId::new(0), 2, 1, 1);
    calibration.record_module_timing(ModuleId::new(1), 5, 2, 1);
    calibration.record_rtt(Direction::AToB, 2);
    calibration.record_rtt(Direction::BToA, 2);
    calibration.complete();

    let status = calibration.status(true, true);
    assert_eq!(status.state(), CalibrationState::Calibrated);
    assert!(status.takeover_ready());
    assert_eq!(status.recalibrations(), 0);
}
