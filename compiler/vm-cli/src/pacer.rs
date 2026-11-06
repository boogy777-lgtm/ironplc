//! Pacing of a free-running scan: the one place that decides what uptime a
//! round sees, how long to wait for the next deadline, and when to stop.
//!
//! A driver of a free-running scan supplies only "run one round at this
//! uptime" and "the next due time". The policy lives here, once:
//!
//! - **Uptime** is microseconds since the pacer started, read from the
//!   [`Clock`] immediately before each round.
//! - **Waiting** is until the next due time, saturating: a deadline that has
//!   already passed means no wait at all. There is no catch-up beyond what the
//!   scheduler itself does with the late uptime it is handed; the pacer never
//!   shortens or skips rounds.
//! - **No deadline** (`None`, a freewheeling-only program) means no wait: the
//!   next round starts immediately.
//! - **Stopping** is a stop flag (set from a signal handler) or a round limit.
//!
//! Drivers that do not pace do not use this: the benchmark runs rounds as
//! fast as possible, the MCP runner uses simulated time, and the debug adapter
//! steps on request.

use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Arc;
use std::time::{Duration, Instant};

/// The time source of a [`Pacer`]; small enough to fake in tests.
pub trait Clock {
    /// Microseconds elapsed since the clock started.
    fn uptime_us(&self) -> u64;

    /// Blocks until [`uptime_us`](Clock::uptime_us) reaches `target_us`;
    /// returns at once when it already has.
    fn sleep_until_us(&mut self, target_us: u64);
}

/// The wall clock: [`Instant`] for the uptime and a thread sleep for the wait.
pub struct SystemClock {
    start: Instant,
}

impl SystemClock {
    /// Starts a clock whose uptime is zero now.
    pub fn start() -> Self {
        Self {
            start: Instant::now(),
        }
    }
}

impl Clock for SystemClock {
    fn uptime_us(&self) -> u64 {
        self.start.elapsed().as_micros() as u64
    }

    fn sleep_until_us(&mut self, target_us: u64) {
        let sleep_us = target_us.saturating_sub(self.uptime_us());
        if sleep_us > 0 {
            std::thread::sleep(Duration::from_micros(sleep_us));
        }
    }
}

/// Paces a free-running scan on a [`Clock`] until a stop flag is set or a
/// round limit is reached.
pub struct Pacer<C: Clock = SystemClock> {
    clock: C,
    stop: Arc<AtomicBool>,
    max_rounds: Option<u64>,
}

impl Pacer {
    /// A pacer on the wall clock, starting now. `max_rounds` of `None` runs
    /// until `stop` is set.
    pub fn start(stop: Arc<AtomicBool>, max_rounds: Option<u64>) -> Self {
        Self::with_clock(SystemClock::start(), stop, max_rounds)
    }
}

impl<C: Clock> Pacer<C> {
    /// A pacer on the given clock.
    pub fn with_clock(clock: C, stop: Arc<AtomicBool>, max_rounds: Option<u64>) -> Self {
        Self {
            clock,
            stop,
            max_rounds,
        }
    }

    /// The uptime to hand to the next round.
    pub fn uptime_us(&self) -> u64 {
        self.clock.uptime_us()
    }

    /// True when the stop flag is set or `rounds` rounds have run and that
    /// reaches the limit.
    pub fn should_stop(&self, rounds: u64) -> bool {
        self.stop.load(Ordering::Relaxed) || self.max_rounds.is_some_and(|max| rounds >= max)
    }

    /// Waits until `next_due_us`, the uptime of the next cyclic deadline.
    /// Returns at once for `None` (freewheeling) or a deadline that has passed.
    pub fn wait_for(&mut self, next_due_us: Option<u64>) {
        if let Some(due_us) = next_due_us {
            if due_us > self.clock.uptime_us() {
                self.clock.sleep_until_us(due_us);
            }
        }
    }
}

#[cfg(test)]
mod tests;
