//! The typed T-contributions of the ADR-0062 timing model: the
//! per-term trackers the calibration engine and the engineering surface
//! consume.
//!
//! ADR-0062 ("Decision") fixes the tracking shape: every term of the
//! failover formula — per-channel ping/pong latencies, per-module
//! claim/ARM latencies, scan safe-point, output apply — is tracked as
//! **current / EMA10 / EMA100 / max / count**, and the HA link profile
//! ("Variables for the engineering UI") adds the RTT min / max jitter
//! envelope, the loss rate, and the max consecutive loss per direction.
//! Nothing here is an assumed constant: every value is a measurement
//! fed through [`TermStats::record`] (the measured-terms philosophy of
//! ADR-0062, shared with the exchange-counted windows of
//! [`crate::liveness`] and the abstract clock of [`crate::lease`]).
//!
//! The EMAs are exact exponential moving averages with alpha =
//! 2/(N+1), computed in fixed-point milli-units (no floats, so the
//! values are bit-deterministic across platforms and hand-computable in
//! tests), seeded with the first sample: an EMA that started at zero
//! would under-report the first samples of a commissioning run. Every
//! tracker also records the running minimum beside the maximum: the
//! jitter envelopes ADR-0062's link profile reports (RTT min / max,
//! scan jitter) are the measured range, never a derived constant.

use crate::fencing::ModuleId;

/// One timing term's tracker: current / min / EMA10 / EMA100 / max /
/// count (ADR-0062, "Decision" names the current / EMA10 / EMA100 /
/// max / count; the min completes the jitter envelope the link
/// profile reports). All values are abstract clock ticks supplied
/// by the composition root.
#[derive(Clone, Debug, Default, PartialEq, Eq)]
pub struct TermStats {
    current: u64,
    min: u64,
    ema10_milli: u64,
    ema100_milli: u64,
    max: u64,
    count: u64,
}

impl TermStats {
    /// The EMA periods ADR-0062 names: EMA10 (fast) and EMA100 (slow).
    const EMA10_PERIOD: u64 = 10;
    const EMA100_PERIOD: u64 = 100;

    /// Creates an empty tracker (no samples yet).
    pub const fn new() -> Self {
        Self {
            current: 0,
            min: 0,
            ema10_milli: 0,
            ema100_milli: 0,
            max: 0,
            count: 0,
        }
    }

    /// Records one sample: updates current, the running min and max,
    /// the count, and both EMAs.
    pub fn record(&mut self, sample: u64) {
        self.count += 1;
        self.current = sample;
        if self.count == 1 {
            self.min = sample;
            self.max = sample;
            self.ema10_milli = sample.saturating_mul(1000);
            self.ema100_milli = self.ema10_milli;
            return;
        }
        self.min = self.min.min(sample);
        self.max = self.max.max(sample);
        self.ema10_milli = Self::ema_next(self.ema10_milli, sample, Self::EMA10_PERIOD);
        self.ema100_milli = Self::ema_next(self.ema100_milli, sample, Self::EMA100_PERIOD);
    }

    /// One EMA step in fixed-point milli-units with alpha = 2/(period+1),
    /// truncating division (deterministic on every platform).
    fn ema_next(ema_milli: u64, sample: u64, period: u64) -> u64 {
        let sample_milli = sample.saturating_mul(1000);
        if sample_milli >= ema_milli {
            let delta = sample_milli - ema_milli;
            ema_milli.saturating_add(delta.saturating_mul(2) / (period + 1))
        } else {
            let delta = ema_milli - sample_milli;
            ema_milli.saturating_sub(delta.saturating_mul(2) / (period + 1))
        }
    }

    /// The most recent sample (0 before the first).
    pub const fn current(&self) -> u64 {
        self.current
    }

    /// The smallest observed sample — the low edge of the jitter
    /// envelope (0 before the first sample).
    pub const fn min(&self) -> u64 {
        self.min
    }

    /// The fast EMA (EMA10), rounded to ticks.
    pub fn ema10(&self) -> u64 {
        self.ema10_milli.saturating_add(500) / 1000
    }

    /// The slow EMA (EMA100), rounded to ticks.
    pub fn ema100(&self) -> u64 {
        self.ema100_milli.saturating_add(500) / 1000
    }

    /// The running maximum — the qualification bound ADR-0062's budget
    /// check consumes (`MaxQualified`).
    pub const fn max(&self) -> u64 {
        self.max
    }

    /// How many samples were recorded.
    pub const fn count(&self) -> u64 {
        self.count
    }
}

/// The per-direction link profile of ADR-0062's HA link profile
/// ("Variables for the engineering UI"): ping/pong round-trip latency as
/// current / EMA10 / EMA100 / max / count, the per-side processing
/// latency, the RTT min / max jitter envelope, the loss rate, and the
/// max consecutive loss — per direction (A→B→A and B→A→B), because
/// scheduler behavior and NIC queues differ per direction and a
/// single-direction number is not evidence (ADR-0062, "Paradigm
/// change").
#[derive(Clone, Debug, Default, PartialEq, Eq)]
pub struct DirectionProfile {
    rtt: TermStats,
    /// The measuring side's processing latency: ticks from the
    /// confirming PONG to the next emitted PING (the firmware/IRQ
    /// delay a real binding reports; measured, 0 on the in-process
    /// loopback binding whose receive and transmit share a step).
    processing: TermStats,
    pings_sent: u64,
    pongs_received: u64,
    misses: u64,
    consecutive_loss: u64,
    max_consecutive_loss: u64,
}

impl DirectionProfile {
    /// Creates an empty profile (no exchanges observed yet).
    pub const fn new() -> Self {
        Self {
            rtt: TermStats::new(),
            processing: TermStats::new(),
            pings_sent: 0,
            pongs_received: 0,
            misses: 0,
            consecutive_loss: 0,
            max_consecutive_loss: 0,
        }
    }

    /// Records one round-trip sample (ticks from the PING send to the
    /// confirming PONG increment).
    pub fn record_rtt(&mut self, sample: u64) {
        self.rtt.record(sample);
    }

    /// Records one per-side processing sample (ticks from the
    /// confirming PONG to the next emitted PING).
    pub fn record_processing(&mut self, sample: u64) {
        self.processing.record(sample);
    }

    /// Notes one PING emitted on this direction.
    pub fn note_ping(&mut self) {
        self.pings_sent += 1;
    }

    /// Notes one PONG increment received on this direction: the exchange
    /// answered, ending any loss streak.
    pub fn note_pong(&mut self) {
        self.pongs_received += 1;
        self.consecutive_loss = 0;
    }

    /// Notes one exchange the confirmation window closed unanswered
    /// (the liveness exchange's miss — ADR-0062's missing-increment
    /// silence detection, not a bare packet timeout). A PING counts as
    /// lost exactly here: PINGs merely in flight are not loss.
    pub fn note_miss(&mut self) {
        self.misses += 1;
        self.consecutive_loss += 1;
        self.max_consecutive_loss = self.max_consecutive_loss.max(self.consecutive_loss);
    }

    /// The round-trip tracker (current / EMA10 / EMA100 / max / count).
    pub const fn rtt(&self) -> &TermStats {
        &self.rtt
    }

    /// The per-side processing latency tracker (ticks from the
    /// confirming PONG to the next emitted PING).
    pub const fn processing(&self) -> &TermStats {
        &self.processing
    }

    /// The smallest observed round trip — the low edge of the jitter
    /// envelope (delegates to the tracker's minimum).
    pub const fn rtt_min(&self) -> u64 {
        self.rtt.min()
    }

    /// The jitter envelope: smallest..=largest observed round trip.
    pub fn jitter_envelope(&self) -> core::ops::RangeInclusive<u64> {
        self.rtt.min()..=self.rtt.max()
    }

    /// PINGs emitted on this direction.
    pub const fn pings_sent(&self) -> u64 {
        self.pings_sent
    }

    /// PONG increments received on this direction.
    pub const fn pongs_received(&self) -> u64 {
        self.pongs_received
    }

    /// Exchanges the confirmation window closed unanswered.
    pub const fn misses(&self) -> u64 {
        self.misses
    }

    /// The loss rate in whole percent: exchanges the confirmation
    /// window closed unanswered over PINGs sent.
    pub fn loss_rate_percent(&self) -> u64 {
        if self.pings_sent == 0 {
            return 0;
        }
        self.misses * 100 / self.pings_sent
    }

    /// The longest run of unanswered exchanges observed.
    pub const fn max_consecutive_loss(&self) -> u64 {
        self.max_consecutive_loss
    }
}

/// One required I/O module's T-contribution to the takeover time
/// (ADR-0062's ownership-barrier view: "per-module profile … and its T
/// contribution"): the claim, ARM, and output-apply latencies, each
/// tracked as current / EMA10 / EMA100 / max / count. On a real target
/// the I/O firmware instruments and reports these delays upward
/// (ADR-0062: "Firmware MUST timestamp each stage and compute the
/// per-module metrics locally, reporting them upward"); the simulator
/// binding reports them from its module registry
/// ([`crate::simulator::ModuleTiming`]).
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct ModuleContribution {
    module: ModuleId,
    claim: TermStats,
    arm: TermStats,
    output_apply: TermStats,
}

impl ModuleContribution {
    /// Creates the contribution tracker for one required module.
    pub const fn new(module: ModuleId) -> Self {
        Self {
            module,
            claim: TermStats::new(),
            arm: TermStats::new(),
            output_apply: TermStats::new(),
        }
    }

    /// Records one sample of the module's firmware-reported delays.
    pub fn record(&mut self, claim: u64, arm: u64, output_apply: u64) {
        self.claim.record(claim);
        self.arm.record(arm);
        self.output_apply.record(output_apply);
    }

    /// The module this contribution belongs to.
    pub const fn module(&self) -> ModuleId {
        self.module
    }

    /// The claim latency tracker (`T_claim,i`; the barrier claims
    /// sequentially, so the pair's claim term is the sum — ADR-0062,
    /// "Timing formulas").
    pub const fn claim(&self) -> &TermStats {
        &self.claim
    }

    /// The ARM latency tracker (`T_arm,i`).
    pub const fn arm(&self) -> &TermStats {
        &self.arm
    }

    /// The output-apply latency tracker (`T_output-apply,i`).
    pub const fn output_apply(&self) -> &TermStats {
        &self.output_apply
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn record_when_first_sample_then_seeds_both_emas() {
        let mut stats = TermStats::new();

        stats.record(10);

        assert_eq!(stats.current(), 10);
        assert_eq!(stats.min(), 10);
        assert_eq!(stats.ema10(), 10);
        assert_eq!(stats.ema100(), 10);
        assert_eq!(stats.max(), 10);
        assert_eq!(stats.count(), 1);
    }

    #[test]
    fn record_when_series_then_hand_computed_emas() {
        // alpha = 2/(N+1) in fixed-point milli-units, truncating:
        // 10 -> 20: ema10 = 10000 + 10000*2/11 = 11818 -> 12
        //           ema100 = 10000 + 10000*2/101 = 10198 -> 10
        // 20 -> 30: ema10 = 11818 + 18182*2/11 = 15123 -> 15
        //           ema100 = 10198 + 19802*2/101 = 10590 -> 11
        let mut stats = TermStats::new();
        stats.record(10);

        stats.record(20);

        assert_eq!(stats.ema10(), 12);
        assert_eq!(stats.ema100(), 10);
        assert_eq!(stats.current(), 20);
        assert_eq!(stats.max(), 20);
        assert_eq!(stats.count(), 2);

        stats.record(30);

        assert_eq!(stats.ema10(), 15);
        assert_eq!(stats.ema100(), 11);
        assert_eq!(stats.min(), 10);
        assert_eq!(stats.max(), 30);
        assert_eq!(stats.count(), 3);
    }

    #[test]
    fn record_when_sample_falls_then_ema_decreases() {
        let mut stats = TermStats::new();
        stats.record(100);
        stats.record(0);

        // 100000 - 100000*2/11 = 81818 -> 82 (EMA10 lags the drop).
        assert_eq!(stats.ema10(), 82);
        assert_eq!(stats.current(), 0);
        assert_eq!(stats.min(), 0);
        assert_eq!(stats.max(), 100);
    }

    #[test]
    fn record_when_no_samples_then_all_zero() {
        let stats = TermStats::new();

        assert_eq!(stats.current(), 0);
        assert_eq!(stats.min(), 0);
        assert_eq!(stats.ema10(), 0);
        assert_eq!(stats.ema100(), 0);
        assert_eq!(stats.max(), 0);
        assert_eq!(stats.count(), 0);
    }

    #[test]
    fn direction_profile_when_processing_sampled_then_tracks_per_side_latency() {
        let mut profile = DirectionProfile::new();

        for _ in 0..3 {
            profile.record_processing(0);
        }

        assert_eq!(profile.processing().count(), 3);
        assert_eq!(profile.processing().current(), 0);
        assert_eq!(profile.processing().max(), 0);
    }

    #[test]
    fn direction_profile_when_steady_then_zero_loss_and_jitter_envelope() {
        let mut profile = DirectionProfile::new();
        for _ in 0..4 {
            profile.note_ping();
            profile.note_pong();
            profile.record_rtt(2);
        }
        profile.record_rtt(3);

        assert_eq!(profile.pings_sent(), 4);
        assert_eq!(profile.pongs_received(), 4);
        assert_eq!(profile.loss_rate_percent(), 0);
        assert_eq!(profile.max_consecutive_loss(), 0);
        assert_eq!(profile.rtt_min(), 2);
        assert_eq!(profile.jitter_envelope(), 2..=3);
        assert_eq!(profile.rtt().ema10(), 2);
        assert_eq!(profile.rtt().count(), 5);
    }

    #[test]
    fn direction_profile_when_partition_then_loss_rate_and_streak() {
        let mut profile = DirectionProfile::new();
        profile.note_ping();
        profile.note_pong();
        profile.note_ping();
        profile.note_miss();
        profile.note_ping();
        profile.note_miss();

        // 3 PINGs, 2 closed unanswered by the window: 66% loss, streak 2.
        assert_eq!(profile.pings_sent(), 3);
        assert_eq!(profile.pongs_received(), 1);
        assert_eq!(profile.misses(), 2);
        assert_eq!(profile.loss_rate_percent(), 66);
        assert_eq!(profile.max_consecutive_loss(), 2);

        // An answer ends the streak; the max stays the historical high.
        profile.note_pong();
        profile.note_ping();
        profile.note_miss();

        assert_eq!(profile.max_consecutive_loss(), 2);
        assert_eq!(profile.loss_rate_percent(), 75);
    }

    #[test]
    fn module_contribution_when_recorded_then_tracks_all_three_terms() {
        let mut contribution = ModuleContribution::new(ModuleId::new(7));

        contribution.record(2, 1, 1);
        contribution.record(4, 3, 2);

        assert_eq!(contribution.module(), ModuleId::new(7));
        assert_eq!(contribution.claim().count(), 2);
        assert_eq!(contribution.claim().max(), 4);
        assert_eq!(contribution.arm().max(), 3);
        assert_eq!(contribution.output_apply().max(), 2);
    }
}
