use super::*;
use crate::admission::AdmissionVerdict;
use crate::calibration::{BudgetVerdict, CalibrationState};
use crate::config::{ConfiguredRole, PairId, RedundancyConfig};
use crate::fencing::ModuleId;
use crate::shell::EVENT_RING_CAPACITY;
use crate::simulator::ModuleRegistry;
use crate::statechart::{ControlState, SyncState};
use ironplc_runtime::ScanCommit;

#[cfg(test)]
mod tests {
    use super::*;

    /// The demo pair configuration: the local unit is Primary-configured.
    fn pair_config() -> RedundancyConfig {
        RedundancyConfig::pair(PairId::new(7), ConfiguredRole::Primary)
            .with_confirmation_exchanges(2)
            .with_missed_exchanges(2)
            .with_lease_ttl(4)
    }

    /// The demo registry: two modules with firmware-reported delays,
    /// one of them dominant.
    fn demo_registry() -> ModuleRegistry {
        ModuleRegistry::new(2)
            .with_connection_timeout(3)
            .with_module_timing(
                ModuleId::new(0),
                crate::simulator::ModuleTiming {
                    claim_ticks: 2,
                    arm_ticks: 1,
                    output_apply_ticks: 1,
                },
            )
            .with_module_timing(
                ModuleId::new(1),
                crate::simulator::ModuleTiming {
                    claim_ticks: 5,
                    arm_ticks: 2,
                    output_apply_ticks: 1,
                },
            )
    }

    fn required() -> Vec<ModuleId> {
        vec![ModuleId::new(0), ModuleId::new(1)]
    }

    /// A brought-up simulated shell: admission, sync, claim, calibration.
    fn brought_up() -> Shell {
        let mut shell = Shell::simulated(pair_config(), demo_registry(), required());
        assert_eq!(shell.start_up(), AdmissionVerdict::Primary);
        shell
    }

    #[test]
    fn standalone_when_started_then_standalone_verdict_and_no_pair() {
        let mut shell = Shell::standalone();

        assert_eq!(shell.start_up(), AdmissionVerdict::Standalone);
        assert!(!shell.has_pair());
        assert_eq!(shell.local_verdict(), AdmissionVerdict::Standalone);
        assert!(shell.unit_view(Side::Peer).is_none());
        let io = shell.io_ready();
        assert!(!io.all());
        assert_eq!(io.failing, Some("standalone"));
        assert!(!shell.calibration_status().takeover_ready());
    }

    #[test]
    fn standalone_when_action_then_refused() {
        let mut shell = Shell::standalone();
        shell.start_up();

        assert_eq!(shell.commanded_swap(), Err(Refusal::Standalone));
        assert_eq!(shell.run_calibration(), Err(Refusal::Standalone));
        assert_eq!(shell.set_timing_budget(2, 100), Err(Refusal::Standalone));
    }

    #[test]
    fn bring_up_when_simulated_pair_then_active_calibrated_ready() {
        let shell = brought_up();

        let status = shell.calibration_status();
        assert_eq!(status.state(), CalibrationState::Calibrated);
        assert!(status.link_valid());
        assert!(status.takeover_ready());
        assert_eq!(shell.local_verdict(), AdmissionVerdict::Primary);
        let local = shell.unit_view(Side::Local).unwrap();
        assert_eq!(local.control, ControlState::Active);
        assert_eq!(local.sync, SyncState::SyncReady);
        let peer = shell.unit_view(Side::Peer).unwrap();
        assert_eq!(peer.control, ControlState::Idle);
        assert_eq!(peer.role, ConfiguredRole::Secondary);
        assert_eq!(peer.sync, SyncState::SyncReady);
        assert!(shell.io_ready().all());
        assert_eq!(status.limiting_device(), Some(ModuleId::new(1)));
        let (_, events) = shell.events();
        assert!(events.iter().any(|e| e.kind == HaEventKind::OwnerAccepted));
        assert!(events
            .iter()
            .any(|e| e.kind == HaEventKind::CalibrationCompleted));
    }

    #[test]
    fn tick_when_healthy_then_pair_exchanges_and_feeds_profiles() {
        let mut shell = brought_up();
        for _ in 0..4 {
            shell.tick();
        }

        let status = shell.calibration_status();
        assert_eq!(status.rtt_ab().rtt().max(), 2);
        assert_eq!(status.rtt_ba().rtt().max(), 2);
        assert_eq!(status.rtt_ab().loss_rate_percent(), 0);
        assert!(shell.io_ready().all());
        assert!(shell.io_ready().configs_match);
        assert!(shell.io_ready().epochs_valid);
    }

    #[test]
    fn swap_when_sync_ready_then_completes_and_exchanges_roles() {
        let mut shell = brought_up();

        assert_eq!(shell.commanded_swap(), Ok(SwapOutcome::Completed));

        let local = shell.unit_view(Side::Local).unwrap();
        assert_eq!(local.control, ControlState::Idle);
        assert_eq!(local.role, ConfiguredRole::Secondary);
        let peer = shell.unit_view(Side::Peer).unwrap();
        assert_eq!(peer.control, ControlState::Active);
        assert_eq!(peer.role, ConfiguredRole::Primary);
        assert_eq!(shell.local_verdict(), AdmissionVerdict::Secondary);
        // The new owner holds every required module.
        assert!(shell
            .ownership()
            .iter()
            .all(|entry| entry.state.owner() == Some(OwnerId::new(2))));
        let (_, events) = shell.events();
        assert!(events.iter().any(|e| e.kind == HaEventKind::SwapCommanded));
        assert!(events.iter().any(|e| e.kind == HaEventKind::SwapCompleted));
    }

    #[test]
    fn swap_when_partitioned_then_refused_with_v4108_semantics() {
        let mut shell = brought_up();
        shell.set_link_partitioned(true);
        for _ in 0..6 {
            shell.tick();
        }

        assert_eq!(shell.commanded_swap(), Err(Refusal::NotSyncReady));

        // The refusal latched the swap alarm; the pair fell to deSYNC.
        let local = shell.unit_view(Side::Local).unwrap();
        assert_eq!(local.sync, SyncState::DeSync);
        assert_eq!(local.alarm, Some(ControlAlarm::SwapRefused));
        let (_, events) = shell.events();
        assert!(events.iter().any(|e| e.kind == HaEventKind::SwapRefused));
        assert!(events
            .iter()
            .any(|e| e.kind == HaEventKind::TimeoutDetected));
    }

    #[test]
    fn partition_when_healed_then_pair_resyncs_through_syncing() {
        let mut shell = brought_up();
        shell.set_link_partitioned(true);
        for _ in 0..6 {
            shell.tick();
        }
        assert_eq!(
            shell.unit_view(Side::Local).unwrap().sync,
            SyncState::DeSync
        );
        shell.set_link_partitioned(false);
        for _ in 0..6 {
            shell.tick();
        }

        let local = shell.unit_view(Side::Local).unwrap();
        assert_eq!(local.sync, SyncState::SyncReady);
        assert_eq!(local.control, ControlState::Active);
        // The pair re-converged: IO_READY and TakeoverReady hold again.
        assert!(shell.io_ready().all());
        assert!(shell.calibration_status().takeover_ready());
    }

    #[test]
    fn swap_when_module_faulted_then_barrier_fails_into_redundancy_lost() {
        let mut shell = brought_up();
        shell.fault_module(ModuleId::new(1));

        assert_eq!(shell.commanded_swap(), Ok(SwapOutcome::BarrierFailed));

        let peer = shell.unit_view(Side::Peer).unwrap();
        assert_eq!(peer.control, ControlState::RedundancyLost);
        assert_eq!(peer.alarm, Some(ControlAlarm::OwnershipBarrierFailed));
        // The configured roles did not exchange; the local unit still
        // owns (the fault rejected the claim, the release already done
        // is the controlled handoff's cost — the pair asks a human).
        let local = shell.unit_view(Side::Local).unwrap();
        assert_eq!(local.role, ConfiguredRole::Primary);
        let (_, events) = shell.events();
        assert!(events.iter().any(|e| e.kind == HaEventKind::SafeCommanded));
        assert!(events.iter().any(|e| e.kind == HaEventKind::SafeApplied));
        assert!(shell.alarm_flags().2);
    }

    #[test]
    fn fencing_loss_when_active_module_faulted_then_degraded_then_lost() {
        let mut shell = brought_up();
        // One of two modules faults: partial loss within policy.
        shell.fault_module(ModuleId::new(1));
        shell.tick();
        assert_eq!(
            shell.unit_view(Side::Local).unwrap().control,
            ControlState::ActiveDegraded
        );
        // Both modules gone: full loss.
        shell.fault_module(ModuleId::new(0));
        shell.tick();
        assert_eq!(
            shell.unit_view(Side::Local).unwrap().control,
            ControlState::RedundancyLost
        );
    }

    #[test]
    fn set_timing_budget_when_below_demonstrated_then_reported_not_applied() {
        let mut shell = brought_up();
        // The demo vector's worst case is 4 + 7 + 2 + 1 + 1 = 15 ticks
        // (detect 4, claim 2+5, arm 2, scan 1, output 1): a 10-tick
        // budget cannot be honored.
        let verdict = shell.set_timing_budget(2, 10);

        assert_eq!(verdict, Ok(BudgetVerdict::MinimumDemonstrated(15)));
        // The configured budget stands unchanged.
        assert_eq!(
            shell
                .calibration_status()
                .budget()
                .copied()
                .unwrap()
                .configured_budget(),
            100
        );
    }

    #[test]
    fn set_timing_budget_when_qualified_then_applied() {
        let mut shell = brought_up();

        let verdict = shell.set_timing_budget(4, 1000);

        assert_eq!(verdict, Ok(BudgetVerdict::Qualified));
        assert_eq!(shell.peer_failure_confirmation(), 4);
        assert_eq!(
            shell
                .calibration_status()
                .budget()
                .copied()
                .unwrap()
                .configured_budget(),
            1000
        );
        let (_, events) = shell.events();
        assert!(events.iter().any(|e| e.kind == HaEventKind::BudgetUpdated));
    }

    #[test]
    fn set_timing_budget_when_reality_outgrows_it_then_lost_until_recalibrated() {
        let mut shell = brought_up();
        // A tight but demonstrable budget applies (the check is 13 <=
        // 13 against the commissioning bounds).
        let verdict = shell.set_timing_budget(2, 13);
        assert_eq!(verdict, Ok(BudgetVerdict::Qualified));

        // Scans slow down: two committed boundaries 20 ticks apart grow
        // the scan term past the budget — the guarantee is lost while
        // the configuration stands (ADR-0062: alarms, not silent
        // adjustment).
        for _ in 0..20 {
            shell.tick();
        }
        shell.on_scan_commit(commit_at(1, 1));
        for _ in 0..20 {
            shell.tick();
        }
        shell.on_scan_commit(commit_at(2, 1));

        assert!(shell.alarm_flags().1);
        let (_, events) = shell.events();
        assert!(events.iter().any(|e| e.kind == HaEventKind::GuaranteeLost));

        // A qualifying setting alone does not clear the latch; only a
        // new calibration run does.
        let _ = shell.set_timing_budget(2, 1000);
        assert!(shell.alarm_flags().1);
        let _ = shell.run_calibration();
        assert!(!shell.alarm_flags().1);
    }

    #[test]
    fn scan_commit_when_committed_then_mints_epoch_and_advances_generations() {
        let mut shell = brought_up();
        shell.note_application_generation(3);
        let before = shell.epoch();

        shell.on_scan_commit(commit_at(5, 3));
        shell.tick();

        assert_eq!(shell.epoch(), before.next());
        assert_eq!(shell.state_generation(), 5);
        assert_eq!(shell.application_generation(), 3);
        assert_eq!(shell.scan_phase(), 1);
    }

    #[test]
    fn events_when_ticks_pass_then_no_steady_state_flood() {
        let mut shell = brought_up();
        let (count_at_bring_up, _) = shell.events();
        for _ in 0..80 {
            shell.tick();
        }

        // Healthy ticks record nothing: the ring holds transitions and
        // actions, not per-tick noise — a post-incident review reads
        // the ledger.
        let (count, events) = shell.events();
        assert_eq!(count, count_at_bring_up);
        assert!(events.len() <= EVENT_RING_CAPACITY);
    }

    fn commit_at(rounds: u64, application: u32) -> ScanCommit {
        ScanCommit {
            rounds,
            mode: ironplc_runtime::HostMode::Normal,
            generation: ironplc_runtime::LogicGeneration::new(1),
            application: ironplc_runtime::ApplicationGeneration::new(application),
        }
    }
}
