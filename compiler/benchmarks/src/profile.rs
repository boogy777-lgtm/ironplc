//! What each step of the analysis costs: the observer of the analysis
//! ([`ironplc_analyzer::observe`]) that times a step and counts the allocations
//! it makes.
//!
//! The analysis tells an observer about every pass, every semantic rule and the
//! steps that make their inputs, around the call that does the step. [`Profile`]
//! is the one observer of the benchmarks: it asks a [`Meter`] what the process
//! has used before and after the call and adds the difference to the step. The
//! meter is the caller's, because counting allocations needs the global
//! allocator of the benchmark binary (`stats_alloc`), which a library cannot
//! install.

use ironplc_analyzer::observe::{Observer, Step};
use std::sync::Mutex;

/// What the process has used at one moment.
#[derive(Debug, Clone, Copy, Default, PartialEq, Eq)]
pub struct Sample {
    /// Nanoseconds since the meter was made.
    pub nanos: u64,
    /// Allocations made so far.
    pub allocations: usize,
    /// Bytes allocated so far.
    pub bytes: usize,
    /// Bytes allocated and not yet freed.
    pub live: usize,
}

/// Reads what the process has used. A step may run on any thread, so a meter is
/// `Sync`.
pub trait Meter: Sync {
    fn sample(&self) -> Sample;
}

/// What a step cost, added over its calls.
#[derive(Debug, Clone, Copy, Default, PartialEq, Eq)]
pub struct Cost {
    pub calls: usize,
    pub nanos: u64,
    pub allocations: usize,
    pub bytes: usize,
    /// The most bytes live at the end of any call: what the step leaves behind
    /// at the highest, which is a floor under the peak of the process.
    pub live_after: usize,
}

impl Cost {
    /// The cost of a call between two samples.
    pub fn between(before: Sample, after: Sample) -> Self {
        Self {
            calls: 1,
            nanos: after.nanos.saturating_sub(before.nanos),
            allocations: after.allocations.saturating_sub(before.allocations),
            bytes: after.bytes.saturating_sub(before.bytes),
            live_after: after.live,
        }
    }

    fn add(&mut self, other: Cost) {
        self.calls += other.calls;
        self.nanos += other.nanos;
        self.allocations += other.allocations;
        self.bytes += other.bytes;
        self.live_after = self.live_after.max(other.live_after);
    }
}

/// The observer that adds up what each step costs, in the order the steps are
/// first told.
pub struct Profile<'m, M: Meter> {
    meter: &'m M,
    steps: Mutex<Vec<(Step, Cost)>>,
}

impl<'m, M: Meter> Profile<'m, M> {
    pub fn new(meter: &'m M) -> Self {
        Self {
            meter,
            steps: Mutex::new(Vec::new()),
        }
    }

    /// The cost of every step told so far, in the order they were first told.
    pub fn into_steps(self) -> Vec<(Step, Cost)> {
        self.steps.into_inner().unwrap_or_default()
    }
}

impl<M: Meter> Observer for Profile<'_, M> {
    fn observe<R>(&self, step: Step, run: impl FnOnce() -> R) -> R {
        let before = self.meter.sample();
        let result = run();
        let cost = Cost::between(before, self.meter.sample());
        if let Ok(mut steps) = self.steps.lock() {
            match steps.iter_mut().find(|(known, _)| *known == step) {
                Some((_, total)) => total.add(cost),
                None => steps.push((step, cost)),
            }
        }
        result
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use ironplc_analyzer::observe::{Kind, Step};
    use std::sync::atomic::{AtomicUsize, Ordering};

    /// A meter that advances by one tick, one allocation and ten bytes at
    /// each reading.
    struct Ticks(AtomicUsize);

    impl Meter for Ticks {
        fn sample(&self) -> Sample {
            let tick = self.0.fetch_add(1, Ordering::SeqCst);
            Sample {
                nanos: tick as u64,
                allocations: tick,
                bytes: tick * 10,
                live: tick * 10,
            }
        }
    }

    const STEP: Step = Step {
        kind: Kind::Pass,
        name: "p",
    };

    #[test]
    fn observe_when_a_step_is_told_twice_then_its_calls_are_added() {
        let meter = Ticks(AtomicUsize::new(0));
        let profile = Profile::new(&meter);

        profile.observe(STEP, || ());
        profile.observe(STEP, || ());

        let steps = profile.into_steps();
        assert_eq!(1, steps.len());
        assert_eq!(2, steps[0].1.calls);
        assert_eq!(2, steps[0].1.allocations);
        assert_eq!(20, steps[0].1.bytes);
    }

    #[test]
    fn observe_when_steps_differ_then_they_keep_the_order_they_were_first_told() {
        let meter = Ticks(AtomicUsize::new(0));
        let profile = Profile::new(&meter);
        let other = Step {
            kind: Kind::Rule,
            name: "r",
        };

        profile.observe(other, || ());
        profile.observe(STEP, || ());
        profile.observe(other, || ());

        let names: Vec<&str> = profile.into_steps().iter().map(|s| s.0.name).collect();
        assert_eq!(vec!["r", "p"], names);
    }

    #[test]
    fn observe_when_the_call_returns_a_value_then_it_is_returned_unchanged() {
        let meter = Ticks(AtomicUsize::new(0));
        let profile = Profile::new(&meter);

        assert_eq!(7, profile.observe(STEP, || 7));
    }
}
