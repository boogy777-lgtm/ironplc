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
//! - **A wait that something may end**: the device loop waits for its next
//!   deadline for a command to arrive, not for the deadline alone. Such a wait
//!   is at most [`STOP_POLL_US`] long, so the stop flag is looked at at least
//!   that often whatever the period of the tasks is.
//!
//! Drivers that do not pace do not use this: the benchmark runs rounds as
//! fast as possible, the MCP runner uses simulated time, and the debug adapter
//! steps on request.

use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::mpsc::{Receiver, RecvTimeoutError, TryRecvError};
use std::sync::Arc;
use std::time::{Duration, Instant};

/// The longest a wait that something may end lasts, in microseconds: the
/// interval at which a waiting driver looks at the stop flag.
pub const STOP_POLL_US: u64 = 50_000;

/// The time source of a [`Pacer`]; small enough to fake in tests.
pub trait Clock {
    /// Microseconds elapsed since the clock started.
    fn uptime_us(&self) -> u64;

    /// Blocks until [`uptime_us`](Clock::uptime_us) reaches `target_us`;
    /// returns at once when it already has.
    fn sleep_until_us(&mut self, target_us: u64);

    /// Waits for a message on `rx` until [`uptime_us`](Clock::uptime_us)
    /// reaches `target_us`: the message when one arrives first, else
    /// [`RecvTimeoutError::Timeout`] at the time, or `Disconnected` when every
    /// sender is gone. Returns at once when the time has already passed.
    fn recv_until_us<T>(&mut self, target_us: u64, rx: &Receiver<T>)
        -> Result<T, RecvTimeoutError>;
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

    fn recv_until_us<T>(
        &mut self,
        target_us: u64,
        rx: &Receiver<T>,
    ) -> Result<T, RecvTimeoutError> {
        let wait_us = target_us.saturating_sub(self.uptime_us());
        rx.recv_timeout(Duration::from_micros(wait_us))
    }
}

/// Whether a round is due at `uptime_us`: always for no deadline (`None`, a
/// freewheeling-only program), else when the deadline has been reached.
pub fn is_due(uptime_us: u64, next_due_us: Option<u64>) -> bool {
    next_due_us.is_none_or(|due_us| uptime_us >= due_us)
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

    /// Waits until `next_due_us` like [`wait_for`](Self::wait_for), but ends
    /// the wait early with the message when one arrives on `rx`.
    ///
    /// `Err(Timeout)` means the wait is over without a message: the deadline
    /// came, or the stop poll interval passed ([`STOP_POLL_US`]) and the
    /// caller looks at its stop flag and asks again. No deadline (`None`) and
    /// a deadline that has passed do not wait: a message that is already there
    /// is returned, else `Timeout`.
    pub fn wait_for_message<T>(
        &mut self,
        next_due_us: Option<u64>,
        rx: &Receiver<T>,
    ) -> Result<T, RecvTimeoutError> {
        let now_us = self.clock.uptime_us();
        match next_due_us {
            Some(due_us) if due_us > now_us => self
                .clock
                .recv_until_us(due_us.min(now_us + STOP_POLL_US), rx),
            _ => rx.try_recv().map_err(|err| match err {
                TryRecvError::Empty => RecvTimeoutError::Timeout,
                TryRecvError::Disconnected => RecvTimeoutError::Disconnected,
            }),
        }
    }

    /// Waits for a message while there is no round to wait for (the device is
    /// not permitted to scan, is stopped or is faulted): at most the stop poll
    /// interval, or until `wake_us` when the caller has something of its own to
    /// do then (the pump of a pair link) and that comes sooner.
    pub fn wait_idle<T>(
        &mut self,
        wake_us: Option<u64>,
        rx: &Receiver<T>,
    ) -> Result<T, RecvTimeoutError> {
        let now_us = self.clock.uptime_us();
        let end_us = now_us + STOP_POLL_US;
        self.clock
            .recv_until_us(wake_us.map_or(end_us, |wake_us| wake_us.min(end_us)), rx)
    }
}

#[cfg(test)]
pub(crate) mod test_clock;
#[cfg(test)]
mod tests;
