//! Tests of the registry of discovery drivers and of its two members.

use std::net::{IpAddr, SocketAddr};

use spec_test_macro::spec_test;

use super::address_list::AddressList;
use super::broadcast::Broadcast;
use super::*;
use crate::test_support::{devices_of, loopback, window, Behaviour, FakeDevice, FakeIdentity};

const PORT: u16 = 49152;

fn selection(interfaces: &[&str], addresses: &[&str]) -> Selection {
    Selection {
        interfaces: interfaces
            .iter()
            .map(|text| text.parse().unwrap())
            .collect(),
        addresses: addresses.iter().map(|text| text.parse().unwrap()).collect(),
        remembered: Vec::new(),
        port: PORT,
    }
}

/// The kinds that build a driver for `selection`, and the kind of each driver.
fn built(selection: &Selection) -> Vec<(DriverKind, DriverKind)> {
    DriverKind::ALL
        .iter()
        .filter_map(|kind| {
            kind.build(selection)
                .unwrap()
                .map(|driver| (*kind, driver.kind()))
        })
        .collect()
}

fn targets_of(selection: &Selection, kind: DriverKind) -> Vec<String> {
    kind.build(selection).unwrap().unwrap().targets()
}

#[spec_test(REQ_IL_ironlink_001)]
#[test]
fn all_when_every_kind_is_asked_to_build_then_each_builds_a_driver_of_its_own_kind() {
    let everything = selection(&["192.168.1.20"], &["10.0.0.5:49152"]);

    let pairs = built(&everything);

    assert_eq!(pairs.len(), DriverKind::ALL.len());
    assert!(pairs.iter().all(|(kind, built)| kind == built));
}

#[spec_test(REQ_IL_ironlink_001)]
#[test]
fn all_when_listed_then_kinds_have_distinct_labels_and_json_names() {
    let labels: Vec<&str> = DriverKind::ALL.iter().map(|kind| kind.label()).collect();
    let names: Vec<String> = DriverKind::ALL
        .iter()
        .map(|kind| serde_json::to_string(kind).unwrap())
        .collect();

    assert_eq!(labels, ["broadcast", "address list"]);
    assert_eq!(names, ["\"broadcast\"", "\"addressList\""]);
}

#[spec_test(REQ_IL_ironlink_005)]
#[test]
fn build_when_selection_names_nothing_then_broadcast_on_the_default_interface_only() {
    let nothing = selection(&[], &[]);

    assert_eq!(
        built(&nothing),
        [(DriverKind::Broadcast, DriverKind::Broadcast)]
    );
    assert_eq!(
        targets_of(&nothing, DriverKind::Broadcast),
        ["0.0.0.0 -> 255.255.255.255:49152"]
    );
}

#[spec_test(REQ_IL_ironlink_005)]
#[test]
fn build_when_selection_names_only_addresses_then_address_list_only() {
    let addresses_only = selection(&[], &["10.0.0.5:49152"]);

    assert_eq!(
        built(&addresses_only),
        [(DriverKind::AddressList, DriverKind::AddressList)]
    );
}

#[spec_test(REQ_IL_ironlink_005)]
#[test]
fn build_when_selection_names_only_interfaces_then_broadcast_only() {
    let interfaces_only = selection(&["192.168.1.20", "10.0.0.7"], &[]);

    assert_eq!(
        built(&interfaces_only),
        [(DriverKind::Broadcast, DriverKind::Broadcast)]
    );
    assert_eq!(
        targets_of(&interfaces_only, DriverKind::Broadcast),
        [
            "192.168.1.20 -> 255.255.255.255:49152",
            "10.0.0.7 -> 255.255.255.255:49152"
        ]
    );
}

/// `selection` with the stored address list `remembered`.
fn with_remembered(selection: Selection, remembered: &[&str]) -> Selection {
    Selection {
        remembered: remembered
            .iter()
            .map(|text| text.parse().unwrap())
            .collect(),
        ..selection
    }
}

#[spec_test(REQ_IL_ironlink_074)]
#[test]
fn build_when_nothing_is_named_and_an_address_is_stored_then_broadcast_on_the_default_and_the_stored_list(
) {
    let stored = with_remembered(selection(&[], &[]), &["10.0.0.9:49152"]);

    assert_eq!(
        built(&stored),
        [
            (DriverKind::Broadcast, DriverKind::Broadcast),
            (DriverKind::AddressList, DriverKind::AddressList)
        ]
    );
    assert_eq!(
        targets_of(&stored, DriverKind::Broadcast),
        ["0.0.0.0 -> 255.255.255.255:49152"]
    );
    assert_eq!(
        targets_of(&stored, DriverKind::AddressList),
        ["10.0.0.9:49152"]
    );
}

#[spec_test(REQ_IL_ironlink_074)]
#[test]
fn build_when_addresses_are_named_and_stored_then_the_union_named_first_each_once() {
    let both = with_remembered(
        selection(&[], &["10.0.0.5:49152", "10.0.0.9:49152"]),
        &["10.0.0.9:49152", "10.0.0.2:7000"],
    );

    assert_eq!(
        built(&both),
        [(DriverKind::AddressList, DriverKind::AddressList)]
    );
    assert_eq!(
        targets_of(&both, DriverKind::AddressList),
        ["10.0.0.5:49152", "10.0.0.9:49152", "10.0.0.2:7000"]
    );
}

#[spec_test(REQ_IL_ironlink_074)]
#[test]
fn build_when_interfaces_are_named_and_an_address_is_stored_then_both_drivers_run() {
    let both = with_remembered(selection(&["192.168.1.20"], &[]), &["10.0.0.9:49152"]);

    assert_eq!(built(&both).len(), 2);
    assert_eq!(
        targets_of(&both, DriverKind::Broadcast),
        ["192.168.1.20 -> 255.255.255.255:49152"]
    );
}

#[spec_test(REQ_IL_ironlink_002)]
#[test]
fn from_selection_when_port_given_then_broadcast_goes_to_the_limited_broadcast_address_on_it() {
    let custom = Selection {
        port: 5000,
        ..selection(&["192.168.1.20"], &[])
    };

    assert_eq!(
        targets_of(&custom, DriverKind::Broadcast),
        ["192.168.1.20 -> 255.255.255.255:5000"]
    );
}

#[spec_test(REQ_IL_ironlink_004)]
#[test]
fn from_selection_when_interface_is_ipv6_then_refused_with_l6004_and_a_clear_message() {
    let err = DriverKind::Broadcast
        .build(&selection(&["fe80::1"], &[]))
        .err()
        .unwrap();

    assert_eq!(err.l_code(), "L6004");
    assert!(err.message().contains("IPv6 has no broadcast"), "{err}");
    assert!(err.message().contains("fe80::1"), "{err}");
}

#[spec_test(REQ_IL_ironlink_004)]
#[test]
fn aimed_at_when_one_of_several_interfaces_is_ipv6_then_refused() {
    let interfaces: Vec<IpAddr> = vec!["192.168.1.20".parse().unwrap(), "::1".parse().unwrap()];
    let destination: SocketAddr = "127.0.0.1:9".parse().unwrap();

    let err = Broadcast::aimed_at(&interfaces, destination).err().unwrap();

    assert_eq!(err.l_code(), "L6004");
}

#[spec_test(REQ_IL_ironlink_003)]
#[test]
fn run_when_broadcast_is_aimed_at_a_loopback_device_then_it_is_found_from_the_chosen_interface() {
    let device = FakeDevice::answering(7, "Press line 3");
    let broadcast = Broadcast::aimed_at(&[loopback()], device.addr()).unwrap();

    let collected = broadcast.run(&window(400, 1)).unwrap();

    assert_eq!(collected.heard.len(), 1);
    let observation = &devices_of(&collected.heard)[0];
    assert_eq!(observation.name, "Press line 3");
    assert_eq!(observation.route.driver, DriverKind::Broadcast);
    assert_eq!(observation.route.address, loopback());
    // The request left from the socket bound to the chosen interface.
    assert_eq!(device.received(1)[0].from.ip(), loopback());
}

#[spec_test(REQ_IL_ironlink_003)]
#[test]
fn run_when_broadcast_has_two_interfaces_then_the_same_device_is_one_entry() {
    let device = FakeDevice::answering(7, "Press line 3");
    let broadcast =
        Broadcast::aimed_at(&[loopback(), "0.0.0.0".parse().unwrap()], device.addr()).unwrap();

    let collected = broadcast.run(&window(400, 1)).unwrap();

    // Both sockets get a reply from 127.0.0.1; the source is the same, so the
    // two replies are one observation.
    assert_eq!(collected.heard.len(), 1);
}

#[spec_test(REQ_IL_ironlink_003)]
#[test]
fn run_when_interface_is_not_an_address_of_this_pc_then_l6001() {
    // 203.0.113.0/24 is reserved for documentation (RFC 5737): no PC has it.
    let broadcast = Broadcast::aimed_at(
        &["203.0.113.9".parse().unwrap()],
        "127.0.0.1:9".parse().unwrap(),
    )
    .unwrap();

    let err = broadcast.run(&window(200, 1)).err().unwrap();

    assert_eq!(err.l_code(), "L6001");
    assert!(err.message().contains("203.0.113.9"), "{err}");
}

#[spec_test(REQ_IL_ironlink_006)]
#[test]
fn run_when_address_list_has_one_device_then_one_observation_by_address_list() {
    let device = FakeDevice::answering(1, "Cell 1");
    let list = AddressList::new(vec![device.addr()]);

    let collected = list.run(&window(400, 1)).unwrap();

    assert_eq!(collected.heard.len(), 1);
    let observation = &devices_of(&collected.heard)[0];
    assert_eq!(observation.route.driver, DriverKind::AddressList);
    assert_eq!(observation.route.session_port, device.addr().port());
}

#[spec_test(REQ_IL_ironlink_006)]
#[test]
fn run_when_address_list_has_two_devices_then_both_are_found() {
    let first = FakeDevice::answering(1, "Cell 1");
    let second = FakeDevice::start(FakeIdentity::new(2, "Cell 2"), Behaviour::Answer);
    let list = AddressList::new(vec![first.addr(), second.addr()]);

    let collected = list.run(&window(400, 1)).unwrap();

    assert_eq!(collected.heard.len(), 2);
}

#[spec_test(REQ_IL_ironlink_006)]
#[test]
fn run_when_one_target_is_silent_then_the_others_are_still_found() {
    let silent = FakeDevice::start(FakeIdentity::new(3, "Quiet"), Behaviour::Silent);
    let answering = FakeDevice::answering(1, "Cell 1");
    let list = AddressList::new(vec![silent.addr(), answering.addr()]);

    let collected = list.run(&window(400, 1)).unwrap();

    assert_eq!(collected.heard.len(), 1);
    assert_eq!(silent.received(1).len(), 1);
}

#[spec_test(REQ_IL_ironlink_006)]
#[test]
fn targets_when_address_list_then_each_address_with_its_port() {
    let list = AddressList::new(vec![
        "10.0.0.5:49152".parse().unwrap(),
        "[::1]:5000".parse().unwrap(),
    ]);

    assert_eq!(list.targets(), ["10.0.0.5:49152", "[::1]:5000"]);
    assert_eq!(list.kind(), DriverKind::AddressList);
}
