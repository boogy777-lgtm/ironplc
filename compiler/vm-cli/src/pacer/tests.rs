//! Tests of the pacing policy on a fake clock.

use std::sync::mpsc;

use super::test_clock::{drive, FakeClock};
use super::*;

fn pacer(clock: &FakeClock, max_rounds: Option<u64>) -> (Pacer<FakeClock>, Arc<AtomicBool>) {
    let stop = Arc::new(AtomicBool::new(false));
    (
        Pacer::with_clock(clock.clone(), stop.clone(), max_rounds),
        stop,
    )
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

#[test]
fn is_due_when_no_deadline_then_always() {
    assert!(is_due(0, None));
}

#[test]
fn is_due_when_deadline_reached_then_true_and_before_then_false() {
    assert!(!is_due(999, Some(1_000)));
    assert!(is_due(1_000, Some(1_000)));
    assert!(is_due(1_001, Some(1_000)));
}

#[test]
fn wait_for_message_when_no_message_then_waits_for_the_deadline() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);
    let (_tx, rx) = mpsc::channel::<u8>();

    let result = pacer.wait_for_message(Some(1_000), &rx);

    assert_eq!(result, Err(RecvTimeoutError::Timeout));
    assert_eq!(clock.now_us(), 1_000);
}

#[test]
fn wait_for_message_when_message_waiting_then_returns_it_before_the_deadline() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);
    let (tx, rx) = mpsc::channel();
    tx.send(7u8).unwrap();

    let result = pacer.wait_for_message(Some(1_000), &rx);

    assert_eq!(result, Ok(7));
    assert_eq!(clock.now_us(), 0);
}

#[test]
fn wait_for_message_when_deadline_is_far_then_wait_ends_at_the_stop_poll() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);
    let (_tx, rx) = mpsc::channel::<u8>();

    let result = pacer.wait_for_message(Some(10_000_000), &rx);

    assert_eq!(result, Err(RecvTimeoutError::Timeout));
    assert_eq!(clock.now_us(), STOP_POLL_US);
}

#[test]
fn wait_for_message_when_no_deadline_then_does_not_wait() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);
    let (_tx, rx) = mpsc::channel::<u8>();

    let result = pacer.wait_for_message(None, &rx);

    assert_eq!(result, Err(RecvTimeoutError::Timeout));
    assert_eq!(clock.now_us(), 0);
    assert!(clock.waits().is_empty());
}

#[test]
fn wait_for_message_when_no_deadline_and_message_waiting_then_returns_it() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);
    let (tx, rx) = mpsc::channel();
    tx.send(7u8).unwrap();

    assert_eq!(pacer.wait_for_message(None, &rx), Ok(7));
}

#[test]
fn wait_for_message_when_deadline_passed_then_does_not_wait() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);
    let (_tx, rx) = mpsc::channel::<u8>();
    clock.advance(2_000);

    let result = pacer.wait_for_message(Some(1_000), &rx);

    assert_eq!(result, Err(RecvTimeoutError::Timeout));
    assert_eq!(clock.now_us(), 2_000);
}

#[test]
fn wait_for_message_when_senders_gone_then_disconnected() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);
    let (tx, rx) = mpsc::channel::<u8>();
    drop(tx);

    assert_eq!(
        pacer.wait_for_message(Some(1_000), &rx),
        Err(RecvTimeoutError::Disconnected)
    );
    assert_eq!(
        pacer.wait_for_message(None, &rx),
        Err(RecvTimeoutError::Disconnected)
    );
}

#[test]
fn wait_idle_when_no_message_then_waits_one_stop_poll() {
    let clock = FakeClock::default();
    let (mut pacer, _) = pacer(&clock, None);
    let (_tx, rx) = mpsc::channel::<u8>();

    let result = pacer.wait_idle(&rx);

    assert_eq!(result, Err(RecvTimeoutError::Timeout));
    assert_eq!(clock.now_us(), STOP_POLL_US);
}

#[test]
fn recv_until_us_when_system_clock_and_message_sent_then_returns_it() {
    let mut clock = SystemClock::start();
    let (tx, rx) = mpsc::channel();
    tx.send(5u8).unwrap();

    assert_eq!(clock.recv_until_us(10_000_000, &rx), Ok(5));
}

#[test]
fn recv_until_us_when_system_clock_and_target_passed_then_times_out_without_waiting() {
    let mut clock = SystemClock::start();
    let (_tx, rx) = mpsc::channel::<u8>();

    assert_eq!(clock.recv_until_us(0, &rx), Err(RecvTimeoutError::Timeout));
}
