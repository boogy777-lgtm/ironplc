use super::*;
use crate::commands::execute;
use crate::config::{ConfiguredRole, RedundancyConfig};
use crate::simulator::{ModuleRegistry, ModuleTiming};
use crate::statechart::{ControlState, SyncState};

#[cfg(test)]
mod tests {
    use super::*;

    fn pair_config() -> RedundancyConfig {
        RedundancyConfig::pair(crate::config::PairId::new(7), ConfiguredRole::Primary)
            .with_confirmation_exchanges(2)
            .with_missed_exchanges(2)
            .with_lease_ttl(4)
    }

    fn demo_registry() -> ModuleRegistry {
        ModuleRegistry::new(2)
            .with_connection_timeout(3)
            .with_module_timing(
                crate::fencing::ModuleId::new(0),
                ModuleTiming {
                    claim_ticks: 2,
                    arm_ticks: 1,
                    output_apply_ticks: 1,
                },
            )
            .with_module_timing(
                crate::fencing::ModuleId::new(1),
                ModuleTiming {
                    claim_ticks: 5,
                    arm_ticks: 2,
                    output_apply_ticks: 1,
                },
            )
    }

    fn brought_up() -> Shell {
        let mut shell = Shell::simulated(
            pair_config(),
            demo_registry(),
            vec![
                crate::fencing::ModuleId::new(0),
                crate::fencing::ModuleId::new(1),
            ],
        );
        shell.note_application_generation(1);
        assert_eq!(
            shell.start_up(),
            crate::admission::AdmissionVerdict::Primary
        );
        shell
    }

    fn render(response: &HaResponse) -> serde_json::Value {
        serde_json::from_str(&render_ha_response(response).unwrap()).unwrap()
    }

    #[test]
    fn parse_when_status_query_then_matching_variant() {
        assert_eq!(
            parse_ha_command(r#"{"command":"haStatus"}"#).unwrap(),
            HaCommand::HaStatus
        );
        assert_eq!(
            parse_ha_command(r#"{"command":"haEvents"}"#).unwrap(),
            HaCommand::HaEvents
        );
        assert_eq!(
            parse_ha_command(r#"{"command":"haCommandedSwap"}"#).unwrap(),
            HaCommand::HaCommandedSwap
        );
        assert_eq!(
            parse_ha_command(r#"{"command":"haRunCalibration"}"#).unwrap(),
            HaCommand::HaRunCalibration
        );
    }

    #[test]
    fn parse_when_set_timing_budget_then_carries_parameters() {
        assert_eq!(
            parse_ha_command(
                r#"{"command":"haSetTimingBudget","peerFailureConfirmation":4,"recoveryBudget":100}"#
            )
            .unwrap(),
            HaCommand::HaSetTimingBudget {
                peer_failure_confirmation: 4,
                recovery_budget: 100
            }
        );
    }

    #[test]
    fn parse_when_line_is_not_json_or_unknown_then_error() {
        assert!(parse_ha_command("not json").is_err());
        assert!(parse_ha_command(r#"{"command":"frobinate"}"#).is_err());
        // A hot-edit command is not an HA command (disjoint vocabularies).
        assert!(parse_ha_command(r#"{"command":"getStatus"}"#).is_err());
    }

    #[test]
    fn execute_when_standalone_then_honest_status_and_refusals() {
        let mut shell = Shell::standalone();
        shell.start_up();

        let status = render(&execute(HaCommand::HaStatus, &mut shell));
        assert_eq!(status["response"], "haStatus");
        assert_eq!(status["standalone"], true);
        assert!(status.get("pairId").is_none());
        assert_eq!(status["local"]["sync"], serde_json::Value::Null);
        assert_eq!(status["takeoverReady"], false);
        assert_eq!(status["alarms"]["redundancyLost"], false);

        let swap = render(&execute(HaCommand::HaCommandedSwap, &mut shell));
        assert_eq!(swap["vCode"], "V4112");
        let run = render(&execute(HaCommand::HaRunCalibration, &mut shell));
        assert_eq!(run["vCode"], "V4112");
        let set = render(&execute(
            HaCommand::HaSetTimingBudget {
                peer_failure_confirmation: 2,
                recovery_budget: 100,
            },
            &mut shell,
        ));
        assert_eq!(set["vCode"], "V4112");
    }

    #[test]
    fn execute_when_simulated_pair_then_full_status_payload() {
        let mut shell = brought_up();

        let status = render(&execute(HaCommand::HaStatus, &mut shell));

        assert_eq!(status["response"], "haStatus");
        assert_eq!(status["standalone"], false);
        assert_eq!(status["pairId"], "7");
        assert_eq!(status["local"]["role"], "primary");
        assert_eq!(status["local"]["control"], "active");
        assert_eq!(status["local"]["sync"], "syncReady");
        assert_eq!(status["peer"]["role"], "secondary");
        assert_eq!(status["peer"]["control"], "idle");
        assert_eq!(status["epoch"], 2);
        assert_eq!(status["applicationGeneration"], 1);
        assert_eq!(status["takeoverReady"], true);
        assert_eq!(status["syncReady"], true);
        assert_eq!(status["ioReady"], true);
        assert_eq!(status["linkValid"], true);
    }

    #[test]
    fn execute_when_simulated_pair_then_calibration_and_barrier_payloads() {
        let mut shell = brought_up();

        let calibration = render(&execute(HaCommand::HaCalibration, &mut shell));
        assert_eq!(calibration["response"], "haCalibration");
        assert_eq!(calibration["state"], "calibrated");
        assert_eq!(calibration["ab"]["rtt"]["max"], 2);
        assert!(calibration["ab"]["processing"]["count"].as_u64().unwrap() >= 10);
        assert_eq!(calibration["ab"]["processing"]["current"], 0);
        assert_eq!(calibration["peerDetect"]["max"], 4);
        assert_eq!(calibration["leaseExpiry"], 3);
        assert_eq!(calibration["claimStart"], 4);
        assert_eq!(calibration["baselineRttMax"], 2);

        let barrier = render(&execute(HaCommand::HaBarrier, &mut shell));
        assert_eq!(barrier["response"], "haBarrier");
        assert_eq!(barrier["modules"].as_array().unwrap().len(), 2);
        assert_eq!(barrier["modules"][0]["profile"], "reconnect");
        assert_eq!(barrier["modules"][0]["ownerState"], "armed");
        assert_eq!(barrier["modules"][0]["owner"], 1);
        assert_eq!(barrier["modules"][0]["ownerArmed"], true);
        assert_eq!(barrier["modules"][1]["claim"]["max"], 5);
        assert_eq!(barrier["limitingDevice"], 1);
        assert_eq!(barrier["worstOwnershipRecovery"], 15);
    }

    #[test]
    fn execute_when_simulated_pair_then_io_ready_and_budget_payloads() {
        let mut shell = brought_up();

        let io = render(&execute(HaCommand::HaIoReady, &mut shell));
        assert_eq!(io["response"], "haIoReady");
        assert_eq!(io["requiredInputsObservable"], true);
        assert_eq!(io["standbyConnectionsValid"], true);
        assert_eq!(io["configsMatch"], true);
        assert_eq!(io["epochsValid"], true);
        assert!(io.get("failingItem").is_none());

        let budget = render(&execute(HaCommand::HaTimingBudget, &mut shell));
        assert_eq!(budget["response"], "haTimingBudget");
        assert_eq!(budget["peerFailureConfirmation"], 2);
        assert_eq!(budget["recoveryBudget"], 100);
        assert_eq!(budget["peerDetect"]["bound"], 4);
        assert_eq!(budget["claim"]["bound"], 7);
        assert_eq!(budget["arm"]["bound"], 2);
        assert_eq!(budget["scan"]["bound"], 1);
        assert_eq!(budget["outputApply"]["bound"], 1);
        assert_eq!(budget["calculatedWorstCase"], 15);
        assert_eq!(budget["verdict"]["qualified"], true);
        assert!(budget["verdict"].get("minimumDemonstrated").is_none());
    }

    #[test]
    fn execute_when_swap_then_ack_and_state_switches() {
        let mut shell = brought_up();

        let ack = render(&execute(HaCommand::HaCommandedSwap, &mut shell));
        assert_eq!(ack["response"], "ack");

        let status = render(&execute(HaCommand::HaStatus, &mut shell));
        assert_eq!(status["local"]["role"], "secondary");
        assert_eq!(status["local"]["control"], "idle");
        assert_eq!(status["peer"]["control"], "active");
        assert_eq!(status["takeoverReady"], true);
    }

    #[test]
    fn execute_when_swap_partitioned_then_v4108() {
        let mut shell = brought_up();
        shell.set_link_partitioned(true);
        for _ in 0..6 {
            shell.tick();
        }

        let refused = render(&execute(HaCommand::HaCommandedSwap, &mut shell));
        assert_eq!(refused["vCode"], "V4108");
        let status = render(&execute(HaCommand::HaStatus, &mut shell));
        assert_eq!(status["local"]["sync"], "deSync");
        assert_eq!(status["local"]["alarm"], "swapRefused");
    }

    #[test]
    fn execute_when_budget_below_demonstrated_then_v4111_with_minimum() {
        let mut shell = brought_up();

        let refused = render(&execute(
            HaCommand::HaSetTimingBudget {
                peer_failure_confirmation: 2,
                recovery_budget: 10,
            },
            &mut shell,
        ));

        assert_eq!(refused["vCode"], "V4111");
        assert_eq!(refused["minimumDemonstrated"], 15);
        // The configured budget stands unchanged.
        let budget = render(&execute(HaCommand::HaTimingBudget, &mut shell));
        assert_eq!(budget["recoveryBudget"], 100);
    }

    #[test]
    fn execute_when_run_calibration_then_ack_and_recalibrated() {
        let mut shell = brought_up();

        let ack = render(&execute(HaCommand::HaRunCalibration, &mut shell));
        assert_eq!(ack["response"], "ack");
        let calibration = render(&execute(HaCommand::HaCalibration, &mut shell));
        assert_eq!(calibration["state"], "calibrated");
        assert_eq!(calibration["recalibrations"], 1);
    }

    #[test]
    fn execute_when_events_then_ring_appended_with_kinds() {
        let mut shell = brought_up();

        let events = render(&execute(HaCommand::HaEvents, &mut shell));
        assert_eq!(events["response"], "haEvents");
        assert!(events["count"].as_u64().unwrap() >= 4);
        let kinds: Vec<&str> = events["events"]
            .as_array()
            .unwrap()
            .iter()
            .map(|event| event["kind"].as_str().unwrap())
            .collect();
        assert!(kinds.contains(&"forwardOpenReceived"));
        assert!(kinds.contains(&"ownerAccepted"));
        assert!(kinds.contains(&"armReceived"));
        assert!(kinds.contains(&"calibrationCompleted"));
    }

    #[test]
    fn execute_when_partition_then_timeout_event_and_io_ready_failing() {
        let mut shell = brought_up();
        shell.set_link_partitioned(true);
        for _ in 0..6 {
            shell.tick();
        }

        let events = render(&execute(HaCommand::HaEvents, &mut shell));
        let kinds: Vec<&str> = events["events"]
            .as_array()
            .unwrap()
            .iter()
            .map(|event| event["kind"].as_str().unwrap())
            .collect();
        assert!(kinds.contains(&"timeoutDetected"));

        let io = render(&execute(HaCommand::HaIoReady, &mut shell));
        assert_eq!(io["standbyConnectionsValid"], false);
        assert_eq!(io["failingItem"], "standbyConnections");
    }

    #[test]
    fn shell_when_brought_up_then_charts_in_expected_states() {
        let shell = brought_up();
        let local = shell.unit_view(Side::Local).unwrap();
        assert_eq!(local.sync, SyncState::SyncReady);
        assert_eq!(local.control, ControlState::Active);
        let peer = shell.unit_view(Side::Peer).unwrap();
        assert_eq!(peer.sync, SyncState::SyncReady);
        assert_eq!(peer.control, ControlState::Idle);
        assert_eq!(
            Refusal::Standalone.to_string(),
            "the unit is standalone: no pair is configured"
        );
    }
}
