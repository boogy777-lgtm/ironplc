//! Tests of the pacing policy on a fake clock.

use std::cell::{Cell, RefCell};
use std::rc::Rc;

use super::*;

/// A clock that only moves when told to or when asked to sleep, and records
/// the deadlines it was asked to sleep until. Clones share the state, so a
/// test keeps one after the pacer takes the other.
#[derive(Clone, Default)]
struct FakeClock {
    now_us: Rc<Cell<u64>>,
    sleeps: Rc<RefCell<Vec<u64>>>,
}

impl FakeClock {
    fn advance(&self, us: u64) {
        self.now_us.set(self.now_us.get() + us);
    }

    fn sleeps(&self) -> Vec<u64> {
        self.sleeps.borrow().clone()
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
}

fn pacer(clock: &FakeClock, max_rounds: Option<u64>) -> (Pacer<FakeClock>, Arc<AtomicBool>) {
    let stop = Arc::new(AtomicBool::new(false));
    (
        Pacer::with_clock(clock.clone(), stop.clone(), max_rounds),
        stop,
    )
}

/// Drives the pacer the way `run` does: stop check, round at the pacer's
/// uptime, count, wait for the deadline `next_due_us` gives after that round.
/// Returns the uptime each round saw.
fn drive(
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

#[test]
fn wait_for_when_no_deadline_then_does_not_sleep() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);

    pacer.wait_for(None);

    assert!(clock.sleeps().is_empty());
}

#[test]
fn wait_for_when_deadline_in_future_then_sleeps_until_deadline() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);
    clock.advance(300);

    pacer.wait_for(Some(1_000));

    assert_eq!(clock.sleeps(), vec![1_000]);
    assert_eq!(pacer.uptime_us(), 1_000);
}

#[test]
fn wait_for_when_deadline_passed_then_does_not_sleep() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);
    clock.advance(2_000);

    pacer.wait_for(Some(1_000));

    assert!(clock.sleeps().is_empty());
    assert_eq!(pacer.uptime_us(), 2_000);
}

#[test]
fn wait_for_when_deadline_is_now_then_does_not_sleep() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);
    clock.advance(1_000);

    pacer.wait_for(Some(1_000));

    assert!(clock.sleeps().is_empty());
}

#[test]
fn uptime_us_when_clock_advances_then_follows_clock() {
    let clock = FakeClock::default();
    let (pacer, _) = pacer(&clock, None);
    assert_eq!(pacer.uptime_us(), 0);

    clock.advance(250);

    assert_eq!(pacer.uptime_us(), 250);
}

#[test]
fn should_stop_when_flag_set_then_true() {
    let clock = FakeClock::default();
    let (pacer, stop) = pacer(&clock, None);
    assert!(!pacer.should_stop(0));

    stop.store(true, Ordering::Relaxed);

    assert!(pacer.should_stop(0));
}

#[test]
fn should_stop_when_limit_reached_then_true() {
    let clock = FakeClock::default();
    let (pacer, _) = pacer(&clock, Some(3));

    assert!(!pacer.should_stop(2));
    assert!(pacer.should_stop(3));
    assert!(pacer.should_stop(4));
}

#[test]
fn should_stop_when_no_limit_and_no_flag_then_false() {
    let clock = FakeClock::default();
    let (pacer, _) = pacer(&clock, None);

    assert!(!pacer.should_stop(u64::MAX));
}

#[test]
fn drive_when_periodic_deadline_then_rounds_start_on_period() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, Some(4));

    // Rounds cost 100 us; the next deadline is the next multiple of 1000 us.
    let round_times = drive(&mut pacer, &clock, 100, |rounds| Some(rounds * 1_000));

    assert_eq!(round_times, vec![0, 1_000, 2_000, 3_000]);
    assert_eq!(clock.sleeps(), vec![1_000, 2_000, 3_000, 4_000]);
}

#[test]
fn drive_when_round_overruns_period_then_next_round_starts_late_without_sleep() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, Some(3));

    // Rounds cost 1500 us against a 1000 us period: every deadline is past
    // when the round ends, so the rounds run back to back.
    let round_times = drive(&mut pacer, &clock, 1_500, |rounds| Some(rounds * 1_000));

    assert_eq!(round_times, vec![0, 1_500, 3_000]);
    assert!(clock.sleeps().is_empty());
}

#[test]
fn drive_when_freewheeling_then_rounds_run_back_to_back_without_sleep() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, Some(3));

    let round_times = drive(&mut pacer, &clock, 100, |_| None);

    assert_eq!(round_times, vec![0, 100, 200]);
    assert!(clock.sleeps().is_empty());
}
