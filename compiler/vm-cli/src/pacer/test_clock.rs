//! A clock for tests of anything that is paced: time moves only when told to
//! or when something waits.

use std::cell::{Cell, RefCell};
use std::rc::Rc;
use std::sync::mpsc::{Receiver, RecvTimeoutError, TryRecvError};

use super::{Clock, Pacer};

/// A clock that only moves when told to or when asked to wait, and records
/// the deadlines it was asked to sleep until and to wait for a message until.
/// Clones share the state, so a test keeps one after the pacer takes the other.
#[derive(Clone, Default)]
pub(crate) struct FakeClock {
    now_us: Rc<Cell<u64>>,
    sleeps: Rc<RefCell<Vec<u64>>>,
    waits: Rc<RefCell<Vec<u64>>>,
}

impl FakeClock {
    pub(crate) fn advance(&self, us: u64) {
        self.now_us.set(self.now_us.get() + us);
    }

    pub(crate) fn now_us(&self) -> u64 {
        self.now_us.get()
    }

    /// The deadlines of the sleeps, in order.
    pub(crate) fn sleeps(&self) -> Vec<u64> {
        self.sleeps.borrow().clone()
    }

    /// The deadlines of the waits for a message that found none, in order.
    pub(crate) fn waits(&self) -> Vec<u64> {
        self.waits.borrow().clone()
    }
}

impl Clock for FakeClock {
    fn uptime_us(&self) -> u64 {
        self.now_us.get()
    }

    fn sleep_until_us(&mut self, target_us: u64) {
        self.sleeps.borrow_mut().push(target_us);
        self.now_us.set(self.now_us.get().max(target_us));
    }

    /// A message that is there is returned at once and time does not move;
    /// else the wait runs to its end: time jumps to `target_us`.
    fn recv_until_us<T>(
        &mut self,
        target_us: u64,
        rx: &Receiver<T>,
    ) -> Result<T, RecvTimeoutError> {
        match rx.try_recv() {
            Ok(message) => Ok(message),
            Err(TryRecvError::Disconnected) => Err(RecvTimeoutError::Disconnected),
            Err(TryRecvError::Empty) => {
                self.waits.borrow_mut().push(target_us);
                self.now_us.set(self.now_us.get().max(target_us));
                Err(RecvTimeoutError::Timeout)
            }
        }
    }
}

/// Drives the pacer the way `run` does: stop check, round at the pacer's
/// uptime, count, wait for the deadline `next_due_us` gives after that round.
/// Returns the uptime each round saw.
pub(crate) fn drive(
    pacer: &mut Pacer<FakeClock>,
    clock: &FakeClock,
    round_cost_us: u64,
    next_due_us: impl Fn(u64) -> Option<u64>,
) -> Vec<u64> {
    let mut round_times = Vec::new();
    let mut rounds = 0u64;
    while !pacer.should_stop(rounds) {
        round_times.push(pacer.uptime_us());
        clock.advance(round_cost_us);
        rounds += 1;
        pacer.wait_for(next_due_us(rounds));
    }
    round_times
}
