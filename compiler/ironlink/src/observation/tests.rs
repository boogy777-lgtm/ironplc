//! Tests of the observation model, its merging and its grouping.

use std::net::IpAddr;

use spec_test_macro::spec_test;

use super::*;

fn address(text: &str) -> IpAddr {
    text.parse().unwrap()
}

/// An observation of device `id_byte` named `name`, received from `from`.
fn observation(id_byte: u8, name: &str, from: &str, seen_at: u64) -> Observation {
    Observation {
        device_id: device_id_hex(&[id_byte; 16]),
        name: name.to_string(),
        model: "IronPLC SoftPLC".to_string(),
        runtime_version: "0.247.0".to_string(),
        protocol: 1,
        session_port: 49152,
        protected: false,
        route: Route {
            driver: DriverKind::AddressList,
            address: address(from),
            session_port: 49152,
            method: Method::Tcp,
        },
        seen_at,
    }
}

fn other_version(from: &str, wire_version: u8, seen_at: u64) -> OtherVersion {
    OtherVersion {
        address: address(from),
        wire_version,
        seen_at,
    }
}

fn devices(heard: &[Heard]) -> Vec<Observation> {
    heard
        .iter()
        .filter_map(|heard| match heard {
            Heard::Device(observation) => Some(observation.clone()),
            Heard::OtherVersion(_) => None,
        })
        .collect()
}

#[test]
fn device_id_hex_when_sixteen_bytes_then_32_lowercase_digits() {
    let mut bytes = [0u8; 16];
    bytes[1] = 0xab;
    bytes[15] = 0xff;

    let hex = device_id_hex(&bytes);

    assert_eq!(hex, "00ab00000000000000000000000000ff");
}

#[spec_test(REQ_IL_ironlink_020)]
#[test]
fn observation_when_serialized_then_camel_case_fields_with_route_and_time() {
    let observation = observation(0x0a, "Press line 3", "192.168.1.50", 1_700_000_000_123);

    let json = serde_json::to_value(&observation).unwrap();

    assert_eq!(json["deviceId"], "0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a");
    assert_eq!(json["name"], "Press line 3");
    assert_eq!(json["model"], "IronPLC SoftPLC");
    assert_eq!(json["runtimeVersion"], "0.247.0");
    assert_eq!(json["protocol"], 1);
    assert_eq!(json["sessionPort"], 49152);
    assert_eq!(json["protected"], false);
    assert_eq!(json["seenAt"], 1_700_000_000_123_u64);
    assert_eq!(json["route"]["driver"], "addressList");
    assert_eq!(json["route"]["address"], "192.168.1.50");
    assert_eq!(json["route"]["sessionPort"], 49152);
    assert_eq!(json["route"]["method"], "tcp");
}

#[spec_test(REQ_IL_ironlink_020)]
#[test]
fn from_reply_when_reply_decoded_then_fields_copied_and_id_formatted() {
    let reply = Reply {
        protocol: 1,
        session_port: 4000,
        device_id: [0x12; 16],
        protected: true,
        name: "Cell 7",
        model: "Model X",
        runtime_version: "1.2.3",
    };

    let observation =
        Observation::from_reply(&reply, DriverKind::Broadcast, address("10.0.0.9"), 5);

    assert_eq!(observation.device_id, "12121212121212121212121212121212");
    assert_eq!(observation.name, "Cell 7");
    assert!(observation.protected);
    assert_eq!(observation.route.driver, DriverKind::Broadcast);
    assert_eq!(observation.route.session_port, 4000);
    assert_eq!(observation.seen_at, 5);
}

#[spec_test(REQ_IL_ironlink_011)]
#[test]
fn merge_when_same_device_at_same_address_again_then_one_entry_with_latest_time() {
    let mut heard = Vec::new();

    merge(
        &mut heard,
        Heard::Device(observation(1, "A", "10.0.0.1", 100)),
    );
    merge(
        &mut heard,
        Heard::Device(observation(1, "A", "10.0.0.1", 400)),
    );
    merge(
        &mut heard,
        Heard::Device(observation(1, "A", "10.0.0.1", 250)),
    );

    let found = devices(&heard);
    assert_eq!(found.len(), 1);
    assert_eq!(found[0].seen_at, 400);
}

#[spec_test(REQ_IL_ironlink_011)]
#[test]
fn merge_when_same_device_at_another_address_then_both_kept() {
    let mut heard = Vec::new();

    merge(
        &mut heard,
        Heard::Device(observation(1, "A", "10.0.0.1", 100)),
    );
    merge(
        &mut heard,
        Heard::Device(observation(1, "A", "10.0.1.1", 100)),
    );

    assert_eq!(devices(&heard).len(), 2);
}

#[spec_test(REQ_IL_ironlink_011)]
#[test]
fn merge_when_same_address_other_device_then_both_kept() {
    let mut heard = Vec::new();

    merge(
        &mut heard,
        Heard::Device(observation(1, "A", "10.0.0.1", 100)),
    );
    merge(
        &mut heard,
        Heard::Device(observation(2, "B", "10.0.0.1", 100)),
    );

    assert_eq!(devices(&heard).len(), 2);
}

#[spec_test(REQ_IL_ironlink_011)]
#[test]
fn merge_when_same_address_speaks_the_same_other_version_again_then_one_entry() {
    let mut heard = Vec::new();

    merge(
        &mut heard,
        Heard::OtherVersion(other_version("10.0.0.1", 2, 100)),
    );
    merge(
        &mut heard,
        Heard::OtherVersion(other_version("10.0.0.1", 2, 300)),
    );
    merge(
        &mut heard,
        Heard::OtherVersion(other_version("10.0.0.1", 3, 300)),
    );

    assert_eq!(heard.len(), 2);
}

#[test]
fn merge_when_other_version_and_device_from_one_address_then_both_kept() {
    let mut heard = Vec::new();

    merge(
        &mut heard,
        Heard::Device(observation(1, "A", "10.0.0.1", 100)),
    );
    merge(
        &mut heard,
        Heard::OtherVersion(other_version("10.0.0.1", 2, 100)),
    );

    assert_eq!(heard.len(), 2);
}

#[spec_test(REQ_IL_ironlink_021)]
#[test]
fn group_when_same_id_and_equal_identity_at_two_addresses_then_one_device_with_two_routes() {
    let found = vec![
        observation(1, "Press", "192.168.2.50", 200),
        observation(1, "Press", "192.168.1.50", 100),
    ];

    let devices = group(found);

    assert_eq!(devices.len(), 1);
    assert!(!devices[0].conflict);
    assert_eq!(devices[0].variants.len(), 1);
    let routes = &devices[0].variants[0].routes;
    assert_eq!(routes.len(), 2);
    assert_eq!(routes[0].route.address, address("192.168.1.50"));
    assert_eq!(routes[1].route.address, address("192.168.2.50"));
    assert_eq!(routes[1].seen_at, 200);
}

#[spec_test(REQ_IL_ironlink_022)]
#[test]
fn group_when_same_id_and_different_identity_then_conflict_with_each_variant() {
    let found = vec![
        observation(1, "Press", "192.168.1.50", 100),
        observation(1, "Press copy", "192.168.1.51", 100),
    ];

    let devices = group(found);

    assert_eq!(devices.len(), 1);
    assert!(devices[0].conflict);
    assert_eq!(devices[0].variants.len(), 2);
    assert_eq!(devices[0].variants[0].identity.name, "Press");
    assert_eq!(devices[0].variants[1].identity.name, "Press copy");
}

#[spec_test(REQ_IL_ironlink_022)]
#[test]
fn group_when_same_id_and_only_runtime_version_differs_then_conflict() {
    let mut newer = observation(1, "Press", "192.168.1.51", 100);
    newer.runtime_version = "0.300.0".to_string();

    let devices = group(vec![observation(1, "Press", "192.168.1.50", 100), newer]);

    assert!(devices[0].conflict);
}

#[test]
fn group_when_session_port_differs_only_then_one_variant_not_a_conflict() {
    let mut other_port = observation(1, "Press", "192.168.1.51", 100);
    other_port.session_port = 50000;
    other_port.route.session_port = 50000;

    let devices = group(vec![
        observation(1, "Press", "192.168.1.50", 100),
        other_port,
    ]);

    assert!(!devices[0].conflict);
    assert_eq!(devices[0].variants[0].routes.len(), 2);
}

#[test]
fn group_when_different_ids_then_devices_ordered_by_name_whatever_the_arrival_order() {
    let forward = group(vec![
        observation(1, "Beta", "10.0.0.1", 1),
        observation(2, "Alpha", "10.0.0.2", 1),
    ]);
    let backward = group(vec![
        observation(2, "Alpha", "10.0.0.2", 1),
        observation(1, "Beta", "10.0.0.1", 1),
    ]);

    assert_eq!(forward, backward);
    assert_eq!(forward[0].variants[0].identity.name, "Alpha");
}

#[test]
fn group_when_no_observations_then_no_devices() {
    assert!(group(Vec::new()).is_empty());
}

#[test]
fn device_when_serialized_then_variants_carry_flattened_identity_and_routes() {
    let devices = group(vec![observation(1, "Press", "192.168.1.50", 100)]);

    let json = serde_json::to_value(&devices[0]).unwrap();

    assert_eq!(json["deviceId"], "01010101010101010101010101010101");
    assert_eq!(json["conflict"], false);
    assert_eq!(json["variants"][0]["name"], "Press");
    assert_eq!(json["variants"][0]["runtimeVersion"], "0.247.0");
    assert_eq!(json["variants"][0]["routes"][0]["address"], "192.168.1.50");
    assert_eq!(json["variants"][0]["routes"][0]["seenAt"], 100);
}

#[spec_test(REQ_IL_ironlink_025)]
#[test]
fn now_ms_when_called_then_a_time_after_the_year_2020() {
    // 2020-01-01T00:00:00Z in milliseconds since the Unix epoch.
    assert!(now_ms() > 1_577_836_800_000);
}
