//! Tests of the collection every driver shares, against fake devices.
//!
//! Timing assertions use tolerant bounds: the sockets of a loaded machine wake a
//! few milliseconds late, and the clock of Windows ticks at about 15 ms.

use std::net::{Ipv4Addr, UdpSocket};
use std::time::{Duration, Instant};

use ironplc_discovery::request;
use rstest::rstest;
use spec_test_macro::spec_test;

use super::*;
use crate::test_support::{
    devices_of, other_versions_of, window, Behaviour, FakeDevice, FakeIdentity,
};

/// A channel on a fresh loopback socket to `destinations`.
fn channel_to(destinations: &[SocketAddr]) -> Channel {
    Channel {
        socket: UdpSocket::bind((Ipv4Addr::LOCALHOST, 0)).unwrap(),
        destinations: destinations.to_vec(),
    }
}

fn silent_device() -> FakeDevice {
    FakeDevice::start(FakeIdentity::new(9, "Quiet"), Behaviour::Silent)
}

fn device_that(behaviour: Behaviour) -> FakeDevice {
    FakeDevice::start(FakeIdentity::new(5, "Cell 5"), behaviour)
}

/// Collects from one fake device and returns the result with the time it took.
fn collect_from(device: &FakeDevice, window: &Window) -> (Collected, Duration) {
    let started = Instant::now();
    let collected = collect(
        DriverKind::AddressList,
        &[channel_to(&[device.addr()])],
        window,
    )
    .unwrap();
    (collected, started.elapsed())
}

fn identify_window(timeout_ms: u64) -> Window {
    Window::from_options(Some(timeout_ms), Some(2), Stop::OnFirstOutcome).unwrap()
}

#[spec_test(REQ_IL_ironlink_008)]
#[test]
fn from_options_when_no_options_then_one_second_and_two_attempts() {
    let window = Window::from_options(None, None, Stop::AtDeadline).unwrap();

    assert_eq!(window.timeout, Duration::from_millis(1000));
    assert_eq!(window.attempts, 2);
    assert_eq!(window.timeout_ms(), 1000);
}

#[spec_test(REQ_IL_ironlink_008)]
#[rstest]
#[case(Some(0), None)]
#[case(Some(60_001), None)]
#[case(None, Some(0))]
#[case(None, Some(11))]
fn from_options_when_out_of_range_then_l6004(
    #[case] timeout_ms: Option<u64>,
    #[case] attempts: Option<u32>,
) {
    let err = Window::from_options(timeout_ms, attempts, Stop::AtDeadline).unwrap_err();

    assert_eq!(err.l_code(), "L6004");
}

#[spec_test(REQ_IL_ironlink_008)]
#[rstest]
#[case(Some(1), None)]
#[case(Some(60_000), None)]
#[case(None, Some(1))]
#[case(None, Some(10))]
fn from_options_when_at_the_limit_then_accepted(
    #[case] timeout_ms: Option<u64>,
    #[case] attempts: Option<u32>,
) {
    assert!(Window::from_options(timeout_ms, attempts, Stop::AtDeadline).is_ok());
}

#[spec_test(REQ_IL_ironlink_013)]
#[test]
fn collect_when_it_asks_then_every_datagram_sent_is_exactly_the_discovery_request() {
    let device = FakeDevice::answering(1, "Cell 1");

    collect_from(&device, &window(700, 3));

    let received = device.received(3);
    assert_eq!(received.len(), 3);
    assert!(received.iter().all(|datagram| datagram.bytes == request()));
}

#[spec_test(REQ_IL_ironlink_007)]
#[test]
fn collect_when_attempts_are_three_then_each_is_sent_at_least_300_ms_after_the_last() {
    let device = silent_device();

    collect_from(&device, &window(700, 3));

    let gaps = device.gaps(3);
    assert_eq!(gaps.len(), 2);
    assert!(
        gaps.iter().all(|gap| *gap >= Duration::from_millis(250)),
        "{gaps:?}"
    );
    assert!(
        gaps.iter().all(|gap| *gap < Duration::from_millis(2000)),
        "{gaps:?}"
    );
}

#[spec_test(REQ_IL_ironlink_007)]
#[test]
fn collect_when_the_next_attempt_would_fall_after_the_deadline_then_it_is_not_sent() {
    let device = silent_device();

    collect_from(&device, &window(250, 3));

    assert_eq!(device.received(1).len(), 1);
}

#[spec_test(REQ_IL_ironlink_007)]
#[test]
fn collect_when_one_attempt_then_one_request() {
    let device = silent_device();

    collect_from(&device, &window(400, 1));

    assert_eq!(device.received(1).len(), 1);
}

#[spec_test(REQ_IL_ironlink_007)]
#[test]
fn collect_when_one_of_two_targets_never_answers_then_the_other_answers_both_attempts() {
    let device = FakeDevice::answering(1, "Cell 1");
    let silent = silent_device();
    let channel = channel_to(&[silent.addr(), device.addr()]);

    let collected = collect(DriverKind::AddressList, &[channel], &window(700, 2)).unwrap();

    // The second request is 300 ms after the first, outside the 250 ms in which
    // a device answers one request per source, so both are answered, and the
    // two answers are one entry.
    assert_eq!(devices_of(&collected.heard).len(), 1);
    assert_eq!(device.received(2).len(), 2);
    assert_eq!(silent.received(2).len(), 2);
}

#[spec_test(REQ_IL_ironlink_008)]
#[test]
fn collect_when_the_target_is_silent_then_nothing_is_found_and_it_ends_at_the_deadline() {
    let device = silent_device();

    let (collected, took) = collect_from(&device, &window(400, 1));

    assert!(collected.heard.is_empty());
    assert_eq!(collected.undecodable, 0);
    assert!(took >= Duration::from_millis(390), "{took:?}");
    assert!(took < Duration::from_millis(1500), "{took:?}");
}

#[spec_test(REQ_IL_ironlink_008)]
#[test]
fn collect_when_it_waits_for_the_deadline_then_a_late_reply_inside_it_is_still_taken() {
    let device = FakeDevice::answering(1, "Cell 1");

    let (collected, took) = collect_from(&device, &window(400, 1));

    assert_eq!(devices_of(&collected.heard).len(), 1);
    // A browse cannot know how many devices there are, so it waits to the end.
    assert!(took >= Duration::from_millis(390), "{took:?}");
}

#[spec_test(REQ_IL_ironlink_012)]
#[test]
fn collect_when_stop_is_at_first_outcome_then_it_ends_at_the_first_reply() {
    let device = FakeDevice::answering(1, "Cell 1");

    let (collected, took) = collect_from(&device, &identify_window(5000));

    assert_eq!(devices_of(&collected.heard).len(), 1);
    assert!(took < Duration::from_millis(2000), "{took:?}");
}

#[spec_test(REQ_IL_ironlink_009)]
#[test]
fn collect_when_reply_is_garbage_then_counted_and_not_an_error() {
    let device = device_that(Behaviour::Garbage);

    let (collected, _) = collect_from(&device, &window(300, 1));

    assert!(collected.heard.is_empty());
    assert_eq!(collected.undecodable, 1);
}

#[spec_test(REQ_IL_ironlink_009)]
#[test]
fn collect_when_reply_is_longer_than_the_longest_then_seen_as_too_long_and_counted() {
    let device = device_that(Behaviour::Oversize);

    let (collected, _) = collect_from(&device, &window(300, 1));

    assert!(collected.heard.is_empty());
    assert_eq!(collected.undecodable, 1);
}

#[spec_test(REQ_IL_ironlink_009)]
#[test]
fn collect_when_the_garbage_comes_twice_then_counted_twice() {
    let device = device_that(Behaviour::Garbage);

    let (collected, _) = collect_from(&device, &window(700, 2));

    assert_eq!(collected.undecodable, 2);
}

#[spec_test(REQ_IL_ironlink_010)]
#[test]
fn collect_when_device_speaks_another_wire_version_then_kept_as_an_other_version_outcome() {
    let device = device_that(Behaviour::WireVersion(2));

    let (collected, _) = collect_from(&device, &window(300, 1));

    assert!(devices_of(&collected.heard).is_empty());
    let others = other_versions_of(&collected.heard);
    assert_eq!(others.len(), 1);
    assert_eq!(others[0].wire_version, 2);
    assert_eq!(others[0].address, Ipv4Addr::LOCALHOST);
    assert_eq!(collected.undecodable, 0);
}

#[spec_test(REQ_IL_ironlink_011)]
#[test]
fn collect_when_the_device_answers_twice_to_each_of_two_requests_then_one_observation() {
    let device = device_that(Behaviour::AnswerTwice);

    let (collected, _) = collect_from(&device, &window(700, 2));

    assert_eq!(collected.heard.len(), 1);
    assert_eq!(device.received(2).len(), 2);
}

#[spec_test(REQ_IL_ironlink_011)]
#[test]
fn collect_when_two_channels_hear_the_same_device_then_one_observation() {
    let device = FakeDevice::answering(1, "Cell 1");
    let channels = [channel_to(&[device.addr()]), channel_to(&[device.addr()])];

    let collected = collect(DriverKind::AddressList, &channels, &window(300, 1)).unwrap();

    assert_eq!(collected.heard.len(), 1);
}

#[spec_test(REQ_IL_ironlink_025)]
#[test]
fn collect_when_reply_arrives_then_the_observation_keeps_the_time_it_was_seen() {
    let device = FakeDevice::answering(1, "Cell 1");
    let before = now_ms();

    let (collected, _) = collect_from(&device, &window(300, 1));

    let after = now_ms();
    let seen_at = devices_of(&collected.heard)[0].seen_at;
    assert!(
        before <= seen_at && seen_at <= after,
        "{before} {seen_at} {after}"
    );
}

#[test]
fn collect_when_a_target_is_a_closed_port_then_it_is_not_an_error() {
    // On Windows the datagram to a closed port comes back as a connection reset
    // on the next read; that is no datagram and no failure.
    let closed = {
        let socket = UdpSocket::bind((Ipv4Addr::LOCALHOST, 0)).unwrap();
        socket.local_addr().unwrap()
    };
    let answering = FakeDevice::answering(1, "Cell 1");

    let collected = collect(
        DriverKind::AddressList,
        &[channel_to(&[closed, answering.addr()])],
        &window(500, 2),
    )
    .unwrap();

    assert_eq!(devices_of(&collected.heard).len(), 1);
    assert_eq!(collected.undecodable, 0);
}

#[test]
fn collect_when_destination_is_of_another_address_family_then_l6002() {
    let ipv6: SocketAddr = "[::1]:9".parse().unwrap();

    let err = collect(
        DriverKind::AddressList,
        &[channel_to(&[ipv6])],
        &window(300, 1),
    )
    .unwrap_err();

    assert_eq!(err.l_code(), "L6002");
    assert!(err.message().contains("[::1]:9"), "{err}");
}

#[test]
fn collect_when_there_are_no_channels_then_nothing_is_heard() {
    let collected = collect(DriverKind::AddressList, &[], &window(300, 1)).unwrap();

    assert_eq!(collected, Collected::default());
}
