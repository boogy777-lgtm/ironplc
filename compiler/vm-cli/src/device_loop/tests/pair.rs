//! Tests of the device loop composed with a pair link (ADR-0086, stage 3d): the
//! link is the loopback binding, so the pair runs in the test's process on the
//! fake clock. The unit under test is the loop; the peer is a second link that
//! the test ticks (and, as an owner, whose rounds it drives).

use ironplc_redundancy::{
    loopback_pair, permit_for, AdmissionVerdict, ConfiguredRole, LoopbackPort, NicPort, PairId,
    PairLink, RedundancyConfig,
};
use tempfile::TempDir;

use super::*;
use crate::serve::boot_host;
use crate::slot_store::Serving;

/// The pair configuration of both units of the tests: the exchanges are
/// confirmed in two ticks and a peer is dead after two missed ones, so a death
/// shows quickly.
fn pair_config(role: ConfiguredRole) -> RedundancyConfig {
    RedundancyConfig::pair(PairId::new(7), role)
        .with_confirmation_exchanges(2)
        .with_missed_exchanges(2)
}

/// The peer of the unit under test: its link, its host, and whether it is
/// alive (ticked and, as the owner, scanning).
struct Peer {
    link: PairLink<LoopbackPort>,
    host: RuntimeHost,
    alive: bool,
}

impl Peer {
    /// One tick of the peer, then, once admitted as the owner, one round that
    /// mints its next epoch.
    fn tick(&mut self) {
        if !self.alive {
            return;
        }
        self.link.tick(&mut self.host);
        let verdict = self.link.local_verdict();
        if let Some(verdict) = verdict {
            permit_for(&mut self.host, verdict);
            if verdict == AdmissionVerdict::Primary {
                let link = &mut self.link;
                self.host
                    .run_with_commit(1, || 0, |commit| link.on_scan_commit(commit))
                    .unwrap();
            }
        }
    }
}

/// A device loop composed with a pair link on a fake clock.
struct PairRig {
    device_loop: DeviceLoop<FakeClock, LoopbackPort>,
    clock: FakeClock,
    handle: LoopHandle,
    /// The far end of the unit's port when there is no peer link: its frames
    /// are the ticks of the unit.
    far_end: Option<LoopbackPort>,
    peer: Option<Peer>,
    _dir: TempDir,
}

impl PairRig {
    /// A unit of `role` over `source`, whose peer is nobody: the far end of
    /// its link only receives.
    fn alone(role: ConfiguredRole, source: &str) -> PairRig {
        let (port, far_end) = loopback_pair();
        PairRig::compose(role, source, port, Some(far_end), None)
    }

    /// A unit of `role` over `source`, with a peer link of the other role over
    /// the same program.
    fn with_peer(role: ConfiguredRole, source: &str) -> PairRig {
        let (port, peer_port) = loopback_pair();
        let peer_role = match role {
            ConfiguredRole::Primary => ConfiguredRole::Secondary,
            ConfiguredRole::Secondary => ConfiguredRole::Primary,
        };
        let peer = Peer {
            link: PairLink::new(pair_config(peer_role), peer_port),
            host: boot_host(compile_container(source)).unwrap(),
            alive: true,
        };
        PairRig::compose(role, source, port, None, Some(peer))
    }

    fn compose(
        role: ConfiguredRole,
        source: &str,
        port: LoopbackPort,
        far_end: Option<LoopbackPort>,
        peer: Option<Peer>,
    ) -> PairRig {
        let dir = TempDir::new().unwrap();
        let file = dir.path().join("app.iplc");
        std::fs::write(&file, container_bytes(source)).unwrap();
        let store = Serving::File(&file).store();
        let host = boot_host(store.boot().unwrap()).unwrap();
        let clock = FakeClock::default();
        let (handle, commands) = channel();
        let pacer = Pacer::with_clock(clock.clone(), Arc::new(AtomicBool::new(false)), None);
        PairRig {
            device_loop: DeviceLoop::paired(
                host,
                store,
                Device::in_memory(),
                PairLink::new(pair_config(role), port),
                pacer,
                commands,
            ),
            clock,
            handle,
            far_end,
            peer,
            _dir: dir,
        }
    }

    /// One iteration of the loop, then one tick of the peer.
    fn step(&mut self) {
        assert_eq!(self.device_loop.step(), None);
        if let Some(peer) = self.peer.as_mut() {
            peer.tick();
        }
    }

    fn steps(&mut self, count: usize) {
        for _ in 0..count {
            self.step();
        }
    }

    /// Iterations until the fake clock reaches `until_us`.
    fn run_until(&mut self, until_us: u64) {
        while self.clock.now_us() < until_us {
            self.step();
        }
    }

    /// The peer stops answering: it is no longer ticked and its owner no
    /// longer scans.
    fn kill_peer(&mut self) {
        self.peer.as_mut().unwrap().alive = false;
    }

    /// Iterations until the unit has been promoted, at most `limit`.
    fn steps_until_promoted(&mut self, limit: usize) {
        for _ in 0..limit {
            if self.verdict() == Some(AdmissionVerdict::Primary) {
                return;
            }
            self.step();
        }
    }

    /// Sends a command line and runs the iteration that executes it; returns
    /// the parsed answer.
    fn ask(&mut self, line: &str) -> serde_json::Value {
        let answer = self.handle.send(line.to_string()).unwrap();
        self.step();
        parsed(&answer)
    }

    fn rounds(&mut self) -> u64 {
        self.device_loop.core().host.status().rounds
    }

    /// The epoch of the unit's link.
    fn epoch(&mut self) -> u64 {
        match self.device_loop.core().ha {
            Some(Ha::Pair(link)) => link.epoch().raw(),
            _ => u64::MAX,
        }
    }

    /// The verdict of the unit's link.
    fn verdict(&mut self) -> Option<AdmissionVerdict> {
        match self.device_loop.core().ha {
            Some(Ha::Pair(link)) => link.local_verdict(),
            _ => None,
        }
    }

    /// The frames that reached the far end of the link since the last call: one
    /// liveness frame per tick of the unit.
    fn frames_at_far_end(&mut self) -> usize {
        let far_end = self.far_end.as_mut().unwrap();
        let mut frames = 0;
        while far_end.poll().is_some() {
            frames += 1;
        }
        frames
    }
}

#[spec_test(REQ_VC_vm_cli_051)]
#[test]
fn step_when_pair_unit_is_admitted_primary_then_it_scans_between_commands() {
    let mut rig = PairRig::alone(ConfiguredRole::Primary, &cyclic_source(1, "T#1ms"));

    rig.run_until(40_000);

    assert_eq!(rig.verdict(), Some(AdmissionVerdict::Primary));
    assert!(rig.rounds() >= 20);
}

#[spec_test(REQ_VC_vm_cli_051)]
#[test]
fn step_when_pair_unit_scans_then_each_round_commit_reaches_the_link() {
    let mut rig = PairRig::alone(ConfiguredRole::Primary, &cyclic_source(1, "T#1ms"));

    rig.run_until(40_000);

    let rounds = rig.rounds();
    assert!(rounds > 0);
    assert_eq!(rig.epoch(), rounds);
}

#[spec_test(REQ_VC_vm_cli_051)]
#[test]
fn step_when_pair_unit_is_the_secondary_then_it_runs_no_round() {
    let mut rig = PairRig::alone(ConfiguredRole::Secondary, &cyclic_source(1, "T#1ms"));

    rig.run_until(40_000);

    assert_eq!(rig.verdict(), Some(AdmissionVerdict::Secondary));
    assert_eq!(rig.rounds(), 0);
}

#[spec_test(REQ_VC_vm_cli_051)]
#[test]
fn step_when_pair_unit_is_idle_then_the_link_is_ticked_every_pump_period() {
    let mut rig = PairRig::alone(ConfiguredRole::Secondary, &cyclic_source(1, "T#1ms"));

    rig.steps(10);

    assert_eq!(rig.frames_at_far_end(), 10);
    assert_eq!(rig.clock.now_us(), 10 * PUMP_PERIOD_US);
}

/// The link counts exchanges, not time, so a unit whose rounds are shorter than
/// the pump period ticks once per period and not once per round.
#[spec_test(REQ_VC_vm_cli_051)]
#[test]
fn step_when_pair_unit_scans_then_the_link_is_ticked_once_per_pump_period_not_per_round() {
    let mut rig = PairRig::alone(ConfiguredRole::Primary, &cyclic_source(1, "T#1ms"));

    rig.run_until(40_000);

    let rounds = rig.rounds();
    assert!(rounds >= 20);
    assert_eq!(rig.frames_at_far_end(), 8);
}

#[spec_test(REQ_VC_vm_cli_051)]
#[test]
fn step_when_pair_unit_scans_a_slow_task_then_the_wait_is_capped_at_the_pump_period() {
    let mut rig = PairRig::alone(ConfiguredRole::Primary, &cyclic_source(1, "T#1s"));

    rig.run_until(100_000);

    assert_eq!(rig.frames_at_far_end(), 20);
}

#[spec_test(REQ_VC_vm_cli_052)]
#[test]
fn step_when_pair_primary_is_stopped_then_it_runs_no_round_and_the_link_keeps_ticking() {
    let mut rig = PairRig::alone(ConfiguredRole::Primary, &cyclic_source(1, "T#1ms"));
    rig.run_until(20_000);
    let stopped = rig.ask(STOP);
    let rounds = rig.rounds();
    let epoch = rig.epoch();
    rig.frames_at_far_end();
    let stopped_at_us = rig.clock.now_us();

    rig.run_until(stopped_at_us + 50_000);

    assert_eq!(stopped["runState"], "stopped");
    assert_eq!(rig.rounds(), rounds);
    assert_eq!(rig.epoch(), epoch);
    assert_eq!(rig.verdict(), Some(AdmissionVerdict::Primary));
    assert!(rig.frames_at_far_end() >= 9);
}

#[spec_test(REQ_VC_vm_cli_052)]
#[test]
fn step_when_pair_primary_is_started_after_a_stop_then_it_scans_again() {
    let mut rig = PairRig::alone(ConfiguredRole::Primary, &cyclic_source(1, "T#1ms"));
    rig.run_until(20_000);
    rig.ask(STOP);
    let rounds = rig.rounds();
    let stopped_at_us = rig.clock.now_us();
    rig.run_until(stopped_at_us + 50_000);

    let started = rig.ask(START);
    let restarted_at_us = rig.clock.now_us();
    rig.run_until(restarted_at_us + 10_000);

    assert_eq!(started["runState"], "running");
    assert!(rig.rounds() > rounds);
    assert_eq!(rig.epoch(), rig.rounds());
}

#[spec_test(REQ_VC_vm_cli_052)]
#[test]
fn step_when_pair_secondary_is_started_then_it_still_runs_no_round() {
    let mut rig = PairRig::alone(ConfiguredRole::Secondary, &cyclic_source(1, "T#1ms"));
    rig.run_until(20_000);

    let stopped = rig.ask(STOP);
    let started = rig.ask(START);
    rig.run_until(40_000);

    assert_eq!(stopped["runState"], "stopped");
    assert_eq!(started["runState"], "running");
    assert_eq!(rig.rounds(), 0);
}

/// The survivor of a confirmed peer death takes the permit and runs the
/// boundary round it owes the pair in the iteration that promoted it, which
/// mints the next epoch.
#[spec_test(REQ_VC_vm_cli_053)]
#[test]
fn step_when_the_peer_dies_then_the_promoted_unit_runs_the_takeover_round_at_once() {
    let mut rig = PairRig::with_peer(ConfiguredRole::Secondary, &cyclic_source(1, "T#1ms"));
    rig.steps(20);
    let rounds_before = rig.rounds();
    let epoch_before = rig.epoch();
    rig.kill_peer();

    rig.steps_until_promoted(20);

    assert_eq!(rounds_before, 0);
    assert!(epoch_before > 0);
    assert_eq!(rig.verdict(), Some(AdmissionVerdict::Primary));
    assert_eq!(rig.rounds(), 1);
    assert!(rig.epoch() > epoch_before);
}

/// An operator's stop stands over a promotion: the survivor holds the verdict
/// and the permit and runs no round until it is started.
#[spec_test(REQ_VC_vm_cli_053)]
#[test]
fn step_when_a_stopped_unit_is_promoted_then_it_runs_no_round_until_started() {
    let mut rig = PairRig::with_peer(ConfiguredRole::Secondary, &cyclic_source(1, "T#1ms"));
    rig.steps(20);
    rig.ask(STOP);
    rig.kill_peer();
    rig.steps(20);
    let rounds_stopped = rig.rounds();

    rig.ask(START);
    rig.steps(2);

    assert_eq!(rig.verdict(), Some(AdmissionVerdict::Primary));
    assert_eq!(rounds_stopped, 0);
    assert!(rig.rounds() >= 1);
}
