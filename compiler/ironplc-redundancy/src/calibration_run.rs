//! The deterministic commissioning calibration run (ADR-0062): the
//! procedure that measures the pair's timing model over the loopback /
//! simulator bindings and produces the link profile the takeover budget
//! qualifies against.
//!
//! The run is a single driver stepping an abstract clock (ticks), the
//! same measured-terms domain as the liveness exchange and the
//! OwnerLease: one tick is one driver step, every value is an integer,
//! and two runs over equal inputs produce identical results (pinned by
//! test). Three phases:
//!
//! 1. **Link sampling** ([`CALIBRATION_SAMPLES`] steady exchanges per
//!    direction): the ping/pong round trip is stamped per PING and
//!    recorded on the confirming PONG increment, per direction
//!    (A→B→A and B→A→B — ADR-0062: a single-direction number is not
//!    evidence); the scan cadence is measured from the driven
//!    round-per-tick commits. On the served-VM target the shell feeds
//!    the same `record_*` APIs from the host's scan-commit seam; the
//!    simulator's composition root is the clock authority here.
//! 2. **Detection drills** ([`DETECTION_DRILLS`] repetitions): the
//!    measuring side's port is partitioned (silence, the
//!    missing-increment model of the FSM spec) and the exchange must
//!    confirm peer death within the configured confirmation time —
//!    `confirmation_exchanges + missed_exchanges` ticks at the driver's
//!    one-exchange-per-tick cadence, which is ADR-0062's detection term
//!    ("detection (the configured confirmation time)"): a configured
//!    threshold validated against the measurement, never assumed. The
//!    drill's self-inflicted partition losses are *not* fed into the
//!    link profile: they are the procedure's own construction, not link
//!    evidence.
//! 3. **Firmware-reported terms**: the per-module claim / ARM /
//!    output-apply delays are sampled from the registry (ADR-0062: "I/O
//!    firmware instruments its own delays and reports them; the PLC
//!    measures peer-detection"), and the I/O owner-lease expiry is the
//!    target's old-connection timeout (falling back to the configured
//!    lease TTL when the target sets none).
//!
//! A standalone configuration (no pair) calibrates nothing: the engine
//! is returned unqualified, honestly reporting that no link profile
//! exists.

use std::collections::VecDeque;

use crate::calibration::{Calibration, Direction};
use crate::config::{PairId, RedundancyConfig};
use crate::epoch::Epoch;
use crate::fencing::ModuleId;
use crate::hal::NicPort;
use crate::liveness::{Liveness, Packet, PairRole};
use crate::loopback::{loopback_pair, LoopbackPort};
use crate::simulator::ModuleRegistry;

/// Steady-state exchanges per direction in the link-sampling phase.
const CALIBRATION_SAMPLES: u64 = 12;

/// Repetitions of the partition-and-measure detection drill.
const DETECTION_DRILLS: u64 = 3;

/// Samples taken of each firmware-reported module delay.
const MODULE_SAMPLES: u64 = 3;

/// Steady exchanges run after each drill so the exchange revives before
/// the next partition (the revival is the PONG increment clearing the
/// death latch).
const RECONVERGE_TICKS: u64 = 4;

/// One side of the drill's ping/pong exchange: the liveness exchange,
/// its port, and the FIFO of unanswered PING send-ticks the RTT samples
/// are measured from.
struct Exchange {
    liveness: Liveness,
    port: LoopbackPort,
    pair_id: PairId,
    role: PairRole,
    epoch: Epoch,
    /// Send ticks of unanswered PINGs, oldest first. Delivery is
    /// in-order and one PONG increment answers one PING, so the oldest
    /// unanswered PING is the one a PONG increment confirms.
    in_flight: VecDeque<u64>,
    last_pong_seq: u64,
    last_pong_tick: u64,
}

impl Exchange {
    fn new(config: &RedundancyConfig, port: LoopbackPort, pair_id: PairId, role: PairRole) -> Self {
        Self {
            liveness: Liveness::new(config),
            port,
            pair_id,
            role,
            epoch: Epoch::new(1),
            in_flight: VecDeque::new(),
            last_pong_seq: 0,
            last_pong_tick: 0,
        }
    }

    /// Receives every pending peer frame: a PONG increment confirms the
    /// oldest unanswered PING (ending any loss streak and, when the
    /// profile is fed, recording the round trip).
    fn receive(
        &mut self,
        now: u64,
        calibration: &mut Calibration,
        direction: Direction,
        feed: bool,
    ) {
        while let Some((_, frame)) = self.port.poll() {
            let Some(packet) = Packet::decode(&frame) else {
                continue;
            };
            if packet.pair_id != self.pair_id {
                continue;
            }
            if packet.pong_seq > self.last_pong_seq {
                self.last_pong_seq = packet.pong_seq;
                self.last_pong_tick = now;
                if feed {
                    calibration.note_pong(direction);
                    if let Some(sent) = self.in_flight.pop_front() {
                        calibration.record_rtt(direction, now - sent);
                    }
                }
            }
            self.liveness.note_received(&packet);
        }
    }

    /// Advances one exchange and sends the outbound frame. The
    /// loopback binding's `send` is infallible (its `PortError::Closed`
    /// is never constructed); a frame the link drops is the partition
    /// drill's own model, not an error to act on.
    fn transmit(
        &mut self,
        now: u64,
        calibration: &mut Calibration,
        direction: Direction,
        feed: bool,
    ) {
        let missed_before = self.liveness.missed();
        let (ping_seq, pong_seq, _) = self.liveness.begin_exchange();
        if feed {
            for _ in missed_before..self.liveness.missed() {
                calibration.note_miss(direction);
            }
            calibration.note_ping(direction);
            // Only PINGs whose answers feed the profile join the FIFO:
            // a drill-era PING is never answered (the partition drops
            // it), and keeping it would inflate the post-revival
            // samples matched against it.
            self.in_flight.push_back(now);
        }
        let packet = Packet {
            pair_id: self.pair_id,
            role: self.role,
            epoch: self.epoch,
            generation: 0,
            ping_seq,
            pong_seq,
        };
        let _ = self.port.send(&packet.encode());
    }
}

/// The scan cadence the run drives: one committed scan per tick. The
/// interval between consecutive commits is the scan sample feeding the
/// safe-point term.
struct ScanCadence {
    last_commit: Option<u64>,
}

impl ScanCadence {
    const fn new() -> Self {
        Self { last_commit: None }
    }

    fn tick(&mut self, now: u64, calibration: &mut Calibration) {
        if let Some(last) = self.last_commit {
            calibration.record_scan(now - last);
        }
        self.last_commit = Some(now);
    }
}

/// One driver step: both sides receive and transmit, and one scan
/// commits. `feed` is whether the exchanges feed the link profile —
/// false during the drills, whose self-inflicted partition losses are
/// not link evidence.
fn step(
    now: u64,
    a: &mut Exchange,
    b: &mut Exchange,
    calibration: &mut Calibration,
    scan: &mut ScanCadence,
    feed: bool,
) {
    a.receive(now, calibration, Direction::AToB, feed);
    b.receive(now, calibration, Direction::BToA, feed);
    a.transmit(now, calibration, Direction::AToB, feed);
    b.transmit(now, calibration, Direction::BToA, feed);
    scan.tick(now, calibration);
}

/// Runs the commissioning calibration over the loopback pair and the
/// simulator registry, returning the engine in `Calibrated` state with
/// the measured link profile, the per-module contributions, and the
/// computed budget (whose verdict is the TakeoverReady gate's input).
///
/// `required` is the application's required I/O set, in the fixed claim
/// order — the modules whose T-contributions the barrier view and the
/// limiting-device selection cover.
pub fn run_calibration(
    config: &RedundancyConfig,
    configured_budget: u64,
    registry: &ModuleRegistry,
    required: &[ModuleId],
) -> Calibration {
    let mut calibration = Calibration::new(configured_budget);
    // A standalone unit has no pair link to calibrate: the engine stays
    // honestly unqualified rather than producing a profile of nothing.
    let Some(pair_id) = config.pair_id else {
        return calibration;
    };
    calibration.begin();

    let (port_a, port_b) = loopback_pair();
    let mut a = Exchange::new(config, port_a, pair_id, PairRole::Primary);
    let mut b = Exchange::new(config, port_b, pair_id, PairRole::Secondary);
    let mut scan = ScanCadence::new();
    let mut now = 0u64;

    // Phase 1: steady-state link sampling, both directions.
    for _ in 0..CALIBRATION_SAMPLES {
        now += 1;
        step(now, &mut a, &mut b, &mut calibration, &mut scan, true);
    }

    // Phase 2: detection drills — partition the measuring side and
    // verify the exchange confirms peer death within the configured
    // confirmation time. ADR-0062's detection term is that configured
    // time ("detection (the configured confirmation time)"), recorded
    // per drill that validates it; a drill that never confirms death
    // records nothing, honestly reporting a threshold the exchange does
    // not honor.
    let configured_detect =
        u64::from(config.confirmation_exchanges) + u64::from(config.missed_exchanges);
    let max_drill_ticks = configured_detect + 4;
    for _ in 0..DETECTION_DRILLS {
        let detected = drill(
            &mut now,
            &mut a,
            &mut b,
            &mut calibration,
            &mut scan,
            max_drill_ticks,
        );
        if detected.is_some_and(|latency| latency <= configured_detect) {
            calibration.record_peer_detect(configured_detect);
        }
    }

    // Phase 3: firmware-reported terms — the I/O owner-lease expiry and
    // the per-module claim / ARM / output-apply delays.
    let lease_expiry = match registry.connection_timeout() {
        u64::MAX => config.lease_ttl,
        timeout => timeout,
    };
    calibration.record_lease_expiry(lease_expiry);
    for _ in 0..MODULE_SAMPLES {
        for &module in required {
            if let Some(timing) = registry.module_timing(module) {
                calibration.record_module_timing(
                    module,
                    timing.claim_ticks,
                    timing.arm_ticks,
                    timing.output_apply_ticks,
                );
            }
        }
    }

    calibration.complete();
    calibration
}

/// One detection drill: partition the measuring side's port, run until
/// the exchange confirms peer death, then restore the link and
/// re-converge. Returns the measured detection latency (ticks from the
/// last confirmed PONG to the confirmed death), or `None` when the
/// exchange never confirmed death within `max_ticks`. The in-flight
/// PINGs of both sides die with the partition (the link drops them), so
/// the FIFOs are cleared — the post-revival PONG increments confirm
/// only post-revival PINGs.
fn drill(
    now: &mut u64,
    a: &mut Exchange,
    b: &mut Exchange,
    calibration: &mut Calibration,
    scan: &mut ScanCadence,
    max_ticks: u64,
) -> Option<u64> {
    a.port.set_partitioned(true);
    a.in_flight.clear();
    b.in_flight.clear();
    let confirmed_at = a.last_pong_tick;
    let mut detected = None;
    for _ in 0..max_ticks {
        *now += 1;
        step(*now, a, b, calibration, scan, false);
        if a.liveness.is_dead() {
            detected = Some(*now - confirmed_at);
            break;
        }
    }
    a.port.set_partitioned(false);
    for _ in 0..RECONVERGE_TICKS {
        *now += 1;
        step(*now, a, b, calibration, scan, true);
    }
    detected
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::config::ConfiguredRole;

    fn pair_config() -> RedundancyConfig {
        RedundancyConfig::pair(PairId::new(1), ConfiguredRole::Primary)
            .with_confirmation_exchanges(2)
            .with_missed_exchanges(2)
            .with_lease_ttl(4)
    }

    fn registry() -> ModuleRegistry {
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

    #[test]
    fn run_when_simulator_pair_then_measured_terms_match_the_drill() {
        let config = pair_config();
        let registry = registry();
        let required = [ModuleId::new(0), ModuleId::new(1)];

        let calibration = run_calibration(&config, 100, &registry, &required);

        let status = calibration.status(true, true);
        assert_eq!(status.state(), crate::CalibrationState::Calibrated);
        // Loopback delivers in one tick per hop: the round trip is 2.
        assert_eq!(status.rtt_ab().rtt().max(), 2);
        assert_eq!(status.rtt_ba().rtt().max(), 2);
        assert_eq!(status.rtt_ab().loss_rate_percent(), 0);
        // The detection term is the configured confirmation time
        // (confirmation 2 + missed 2 = 4 ticks), recorded once per
        // drill that validated it; the exchange's one-PING-deep
        // pipeline actually confirms death in 3 ticks, within the bound.
        assert_eq!(status.peer_detect().max(), 4);
        assert_eq!(status.peer_detect().count(), DETECTION_DRILLS);
        // The registry's old-connection timeout is the lease expiry.
        assert_eq!(status.lease_expiry(), 3);
        assert_eq!(status.claim_start(), 4);
        // The scan cadence is one committed round per tick.
        assert_eq!(status.scan().max(), 1);
        // Module 1's claim (5) dominates module 0's (2).
        assert_eq!(status.limiting_device(), Some(ModuleId::new(1)));
        assert!(status.link_valid());
        assert!(status.takeover_ready());
    }

    #[test]
    fn run_when_repeated_then_reproducible() {
        let config = pair_config();
        let required = [ModuleId::new(0), ModuleId::new(1)];

        let first = run_calibration(&config, 100, &registry(), &required);
        let second = run_calibration(&config, 100, &registry(), &required);

        assert_eq!(first.status(true, true), second.status(true, true));
    }

    #[test]
    fn run_when_standalone_then_stays_unqualified() {
        let config = RedundancyConfig::standalone();
        let registry = ModuleRegistry::new(1);

        let calibration = run_calibration(&config, 100, &registry, &[ModuleId::new(0)]);

        let status = calibration.status(true, true);
        assert_eq!(status.state(), crate::CalibrationState::Unqualified);
        assert_eq!(status.budget(), None);
        assert!(!status.link_valid());
    }
}
