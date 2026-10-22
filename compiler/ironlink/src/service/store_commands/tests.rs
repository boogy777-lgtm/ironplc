//! Tests of the commands that keep or use the workstation store. Every test has a
//! store in a temporary directory of its own and fake devices on loopback ports.

use std::fs;
use std::net::IpAddr;

use rstest::rstest;
use spec_test_macro::spec_test;

use ironplc_discovery::DeviceId;

use super::*;
use crate::driver::DriverKind;
use crate::observation::now_ms;
use crate::service::{browse, BrowseRequest};
use crate::test_support::{loopback, temp_store, Behaviour, FakeDevice, FakeIdentity};

/// The device id of a fake device made with `id_byte`.
fn id_of(id_byte: u8) -> String {
    format!("{id_byte:02x}").repeat(16)
}

/// The same device id as a value.
fn device_id_of(id_byte: u8) -> DeviceId {
    DeviceId::from_bytes([id_byte; 16])
}

fn set_request(device_id: &str, device: &FakeDevice) -> RouteSetRequest {
    RouteSetRequest {
        device_id: device_id.to_string(),
        address: device.addr().to_string(),
        timeout_ms: Some(1000),
        attempts: Some(1),
    }
}

fn silent_device(id_byte: u8) -> FakeDevice {
    FakeDevice::start(FakeIdentity::new(id_byte, "Quiet"), Behaviour::Silent)
}

fn browse_request(addresses: &[&FakeDevice]) -> BrowseRequest {
    BrowseRequest {
        addresses: addresses
            .iter()
            .map(|device| device.addr().to_string())
            .collect(),
        timeout_ms: Some(300),
        attempts: Some(1),
        ..BrowseRequest::default()
    }
}

fn route_to(device: &FakeDevice, verified_at: u64) -> RouteRecord {
    RouteRecord {
        address: loopback(),
        driver: DriverKind::AddressList,
        session_port: device.addr().port(),
        verified_at,
    }
}

#[spec_test(REQ_IL_ironlink_070)]
#[test]
fn route_set_when_the_device_answers_with_that_id_then_the_verified_route_is_stored() {
    let (_guard, store) = temp_store();
    let device = FakeDevice::answering(5, "Press");
    let before = now_ms();

    let result = route_set(&set_request(&id_of(5), &device), &store).unwrap();

    assert_eq!(result.device_id, device_id_of(5));
    assert!(!result.replaced);
    assert_eq!(result.route.address, loopback());
    assert_eq!(result.route.driver, DriverKind::AddressList);
    assert_eq!(result.route.session_port, device.addr().port());
    assert!(result.route.verified_at >= before, "{result:?}");
    assert!(result.route.verified_at <= now_ms(), "{result:?}");
    let stored = store.routes().unwrap();
    assert_eq!(stored.len(), 1);
    assert_eq!(stored[0].device_id, device_id_of(5));
    assert_eq!(stored[0].route, result.route);
}

#[spec_test(REQ_IL_ironlink_070)]
#[test]
fn route_set_when_a_route_exists_then_the_latest_replaces_it() {
    let (_guard, store) = temp_store();
    let first = FakeDevice::answering(5, "Press");
    let second = FakeDevice::answering(5, "Press copy");
    route_set(&set_request(&id_of(5), &first), &store).unwrap();

    let result = route_set(&set_request(&id_of(5), &second), &store).unwrap();

    assert!(result.replaced);
    let stored = store.routes().unwrap();
    assert_eq!(stored.len(), 1);
    assert_eq!(stored[0].route.session_port, second.addr().port());
}

#[spec_test(REQ_IL_ironlink_071)]
#[test]
fn route_set_when_another_device_id_answers_then_l6008_naming_both_and_nothing_is_stored() {
    let (_guard, store) = temp_store();
    let device = FakeDevice::answering(5, "Press");

    let err = route_set(&set_request(&id_of(6), &device), &store).unwrap_err();

    assert_eq!(err.l_code(), "L6008");
    assert_eq!(err.exit_code(), 2);
    assert!(err.message().contains(&id_of(5)), "{err}");
    assert!(err.message().contains(&id_of(6)), "{err}");
    assert!(err.message().contains(&device.addr().to_string()), "{err}");
    assert!(store.routes().unwrap().is_empty());
    assert!(!store.dir().unwrap().exists());
}

#[spec_test(REQ_IL_ironlink_071)]
#[test]
fn route_set_when_the_mismatch_would_replace_a_route_then_the_old_route_stays() {
    let (_guard, store) = temp_store();
    let known = FakeDevice::answering(5, "Press");
    let other = FakeDevice::answering(7, "Other");
    route_set(&set_request(&id_of(5), &known), &store).unwrap();

    let err = route_set(&set_request(&id_of(5), &other), &store).unwrap_err();

    assert_eq!(err.l_code(), "L6008");
    let stored = store.routes().unwrap();
    assert_eq!(stored[0].route.session_port, known.addr().port());
}

#[spec_test(REQ_IL_ironlink_071)]
#[test]
fn route_set_when_nothing_answers_then_l6003_and_nothing_is_stored() {
    let (_guard, store) = temp_store();
    let device = silent_device(5);
    let request = RouteSetRequest {
        timeout_ms: Some(300),
        ..set_request(&id_of(5), &device)
    };

    let err = route_set(&request, &store).unwrap_err();

    assert_eq!(err.l_code(), "L6003");
    assert!(store.routes().unwrap().is_empty());
    assert!(!store.dir().unwrap().exists());
}

#[spec_test(REQ_IL_ironlink_071)]
#[test]
fn route_set_when_the_device_speaks_another_discovery_version_then_l6003_and_nothing_is_stored() {
    let (_guard, store) = temp_store();
    let device = FakeDevice::start(FakeIdentity::new(5, "Newer"), Behaviour::WireVersion(2));

    let err = route_set(&set_request(&id_of(5), &device), &store).unwrap_err();

    assert_eq!(err.l_code(), "L6003");
    assert!(err.message().contains("update IronLink"), "{err}");
    assert!(store.routes().unwrap().is_empty());
}

#[spec_test(REQ_IL_ironlink_071)]
#[rstest]
#[case("")]
#[case("0505")]
#[case("0505050505050505050505050505050G")]
#[case("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
#[case("050505050505050505050505050505050")]
#[case(" 05050505050505050505050505050505")]
fn route_set_when_the_device_id_is_not_32_lowercase_hex_then_l6004_and_nothing_is_sent(
    #[case] device_id: &str,
) {
    let (_guard, store) = temp_store();
    let device = FakeDevice::answering(5, "Press");

    let err = route_set(&set_request(device_id, &device), &store).unwrap_err();

    assert_eq!(err.l_code(), "L6004");
    assert!(device.received(0).is_empty());
    assert!(store.routes().unwrap().is_empty());
}

#[spec_test(REQ_IL_ironlink_071)]
#[test]
fn route_set_when_the_address_is_not_an_address_then_l6004() {
    let (_guard, store) = temp_store();
    let request = RouteSetRequest {
        device_id: id_of(5),
        address: "controller".to_string(),
        timeout_ms: None,
        attempts: None,
    };

    assert_eq!(route_set(&request, &store).unwrap_err().l_code(), "L6004");
}

#[spec_test(REQ_IL_ironlink_064)]
#[test]
fn route_set_when_the_route_record_is_unreadable_then_l6006_and_the_file_is_not_changed() {
    let (_guard, store) = temp_store();
    let dir = store.dir().unwrap().to_path_buf();
    fs::create_dir_all(&dir).unwrap();
    fs::write(dir.join("routes.json"), "routes of the user, by hand").unwrap();
    let device = FakeDevice::answering(5, "Press");

    let err = route_set(&set_request(&id_of(5), &device), &store).unwrap_err();

    assert_eq!(err.l_code(), "L6006");
    assert_eq!(
        fs::read_to_string(dir.join("routes.json")).unwrap(),
        "routes of the user, by hand"
    );
}

#[spec_test(REQ_IL_ironlink_072)]
#[test]
fn route_remove_when_the_route_exists_then_removed_and_when_absent_then_not_removed_without_error()
{
    let (_guard, store) = temp_store();
    let device = FakeDevice::answering(5, "Press");
    route_set(&set_request(&id_of(5), &device), &store).unwrap();
    let request = RouteRemoveRequest {
        device_id: id_of(5),
    };

    let first = route_remove(&request, &store).unwrap();
    let second = route_remove(&request, &store).unwrap();

    assert_eq!(first.device_id, device_id_of(5));
    assert!(first.removed);
    assert!(!second.removed);
    assert!(route_list(&store).unwrap().routes.is_empty());
}

#[spec_test(REQ_IL_ironlink_072)]
#[test]
fn route_remove_when_the_device_id_is_not_valid_then_l6004() {
    let (_guard, store) = temp_store();
    let request = RouteRemoveRequest {
        device_id: "press".to_string(),
    };

    assert_eq!(
        route_remove(&request, &store).unwrap_err().l_code(),
        "L6004"
    );
}

#[spec_test(REQ_IL_ironlink_072)]
#[test]
fn route_list_when_routes_are_stored_then_ordered_by_device_id() {
    let (_guard, store) = temp_store();
    let high = FakeDevice::answering(9, "High");
    let low = FakeDevice::answering(2, "Low");
    route_set(&set_request(&id_of(9), &high), &store).unwrap();
    route_set(&set_request(&id_of(2), &low), &store).unwrap();

    let listed = route_list(&store).unwrap();

    let ids: Vec<DeviceId> = listed.routes.iter().map(|entry| entry.device_id).collect();
    assert_eq!(ids, [device_id_of(2), device_id_of(9)]);
    let json = serde_json::to_value(&listed).unwrap();
    assert_eq!(json["routes"][0]["deviceId"], id_of(2));
    assert_eq!(json["routes"][0]["address"], "127.0.0.1");
    assert_eq!(json["routes"][0]["driver"], "addressList");
    assert_eq!(json["routes"][0]["sessionPort"], low.addr().port());
    assert!(json["routes"][0]["verifiedAt"].is_u64());
}

fn address(text: &str) -> AddressRequest {
    AddressRequest {
        address: text.to_string(),
    }
}

#[spec_test(REQ_IL_ironlink_073)]
#[test]
fn address_add_when_added_twice_in_any_order_then_the_list_is_unique_and_sorted() {
    let (_guard, store) = temp_store();

    let first = address_add(&address("10.0.0.10:7000"), &store).unwrap();
    let second = address_add(&address("9.9.9.9"), &store).unwrap();
    let again = address_add(&address(" 10.0.0.10:7000 "), &store).unwrap();

    assert_eq!(first.address, "10.0.0.10:7000");
    assert!(first.added);
    assert_eq!(second.address, format!("9.9.9.9:{DEFAULT_SESSION_PORT}"));
    assert!(second.added);
    assert!(!again.added);
    assert_eq!(
        address_list(&store).unwrap().addresses,
        [
            format!("9.9.9.9:{DEFAULT_SESSION_PORT}"),
            "10.0.0.10:7000".to_string()
        ]
    );
}

#[spec_test(REQ_IL_ironlink_073)]
#[test]
fn address_remove_when_present_then_removed_and_when_absent_then_not_removed_without_error() {
    let (_guard, store) = temp_store();
    address_add(&address("10.0.0.1:7000"), &store).unwrap();

    let first = address_remove(&address("10.0.0.1:7000"), &store).unwrap();
    let second = address_remove(&address("10.0.0.1:7000"), &store).unwrap();

    assert!(first.removed);
    assert_eq!(first.address, "10.0.0.1:7000");
    assert!(!second.removed);
    assert!(address_list(&store).unwrap().addresses.is_empty());
}

#[spec_test(REQ_IL_ironlink_073)]
#[rstest]
#[case("printer")]
#[case("10.0.0.1:0")]
#[case("")]
fn address_add_when_not_an_address_then_l6004_and_nothing_is_stored(#[case] text: &str) {
    let (_guard, store) = temp_store();

    let add = address_add(&address(text), &store).unwrap_err();
    let remove = address_remove(&address(text), &store).unwrap_err();

    assert_eq!(add.l_code(), "L6004");
    assert_eq!(remove.l_code(), "L6004");
    assert!(!store.dir().unwrap().exists());
}

#[spec_test(REQ_IL_ironlink_074)]
#[test]
fn browse_when_the_address_list_is_stored_then_its_devices_are_found_with_the_named_ones() {
    let (_guard, store) = temp_store();
    let named = FakeDevice::answering(1, "Named");
    let stored = FakeDevice::answering(2, "Stored");
    let both = FakeDevice::answering(3, "Both");
    address_add(&address(&stored.addr().to_string()), &store).unwrap();
    address_add(&address(&both.addr().to_string()), &store).unwrap();

    let result = browse(&browse_request(&[&named, &both]), &store).unwrap();

    assert_eq!(result.drivers.len(), 1);
    let driver = &result.drivers[0];
    assert_eq!(driver.kind, DriverKind::AddressList);
    assert_eq!(
        driver.targets,
        [
            named.addr().to_string(),
            both.addr().to_string(),
            stored.addr().to_string()
        ]
    );
    let names: Vec<&str> = driver
        .devices
        .iter()
        .map(|device| device.variants[0].identity.name.as_str())
        .collect();
    assert_eq!(names, ["Both", "Named", "Stored"]);
}

#[spec_test(REQ_IL_ironlink_074)]
#[test]
fn selection_of_when_nothing_is_named_and_addresses_are_stored_then_they_are_remembered_not_named()
{
    let remembered = vec!["10.0.0.9:49152".parse().unwrap()];

    let selection = crate::service::selection_of(&BrowseRequest::default(), remembered).unwrap();

    assert!(selection.addresses.is_empty());
    assert!(selection.interfaces.is_empty());
    assert_eq!(selection.remembered.len(), 1);
}

#[spec_test(REQ_IL_ironlink_075)]
#[test]
fn browse_when_a_route_is_stored_then_that_route_is_marked_routed_and_others_are_not() {
    let (_guard, store) = temp_store();
    let routed = FakeDevice::answering(1, "Routed");
    let other = FakeDevice::answering(2, "Other");
    store
        .set_route(&device_id_of(1), route_to(&routed, 5))
        .unwrap();

    let result = browse(&browse_request(&[&routed, &other]), &store).unwrap();

    let devices = &result.drivers[0].devices;
    let marks: Vec<(&str, bool)> = devices
        .iter()
        .map(|device| {
            (
                device.variants[0].identity.name.as_str(),
                device.variants[0].routes[0].routed,
            )
        })
        .collect();
    assert_eq!(marks, [("Other", false), ("Routed", true)]);
    let json = serde_json::to_value(&result).unwrap();
    assert_eq!(
        json["drivers"][0]["devices"][1]["variants"][0]["routes"][0]["routed"],
        true
    );
    assert_eq!(
        json["drivers"][0]["devices"][0]["variants"][0]["routes"][0]["routed"],
        false
    );
}

#[spec_test(REQ_IL_ironlink_075)]
#[test]
fn browse_when_the_stored_route_has_another_address_or_port_then_the_route_is_not_marked() {
    let (_guard, store) = temp_store();
    let device = FakeDevice::answering(1, "Moved");
    let other_port = RouteRecord {
        session_port: device.addr().port().wrapping_add(1).max(1),
        ..route_to(&device, 5)
    };
    store.set_route(&device_id_of(1), other_port).unwrap();

    let result = browse(&browse_request(&[&device]), &store).unwrap();
    store
        .set_route(
            &device_id_of(1),
            RouteRecord {
                address: "192.0.2.1".parse::<IpAddr>().unwrap(),
                ..route_to(&device, 5)
            },
        )
        .unwrap();
    let other_address = browse(&browse_request(&[&device]), &store).unwrap();

    assert!(!result.drivers[0].devices[0].variants[0].routes[0].routed);
    assert!(!other_address.drivers[0].devices[0].variants[0].routes[0].routed);
}

#[spec_test(REQ_IL_ironlink_075)]
#[test]
fn browse_when_the_route_was_set_by_route_set_then_the_same_device_is_marked() {
    let (_guard, store) = temp_store();
    let device = FakeDevice::answering(4, "Press");
    route_set(&set_request(&id_of(4), &device), &store).unwrap();

    let result = browse(&browse_request(&[&device]), &store).unwrap();

    assert!(result.drivers[0].devices[0].variants[0].routes[0].routed);
}

#[spec_test(REQ_IL_ironlink_076)]
#[test]
fn browse_when_a_stored_route_is_of_a_device_not_seen_then_it_is_listed_apart_and_not_an_observation(
) {
    let (_guard, store) = temp_store();
    let seen = FakeDevice::answering(1, "Seen");
    let away = silent_device(2);
    store
        .set_route(&device_id_of(1), route_to(&seen, 5))
        .unwrap();
    store
        .set_route(&device_id_of(2), route_to(&away, 6))
        .unwrap();

    let result = browse(&browse_request(&[&seen]), &store).unwrap();

    let not_seen: Vec<DeviceId> = result
        .routes_not_seen
        .iter()
        .map(|entry| entry.device_id)
        .collect();
    assert_eq!(not_seen, [device_id_of(2)]);
    assert_eq!(result.drivers[0].devices.len(), 1);
    let json = serde_json::to_value(&result).unwrap();
    assert_eq!(json["routesNotSeen"][0]["deviceId"], id_of(2));
    assert_eq!(json["routesNotSeen"][0]["verifiedAt"], 6);
}

#[spec_test(REQ_IL_ironlink_076)]
#[test]
fn browse_when_no_route_is_stored_then_routes_not_seen_is_an_empty_list() {
    let (_guard, store) = temp_store();
    let device = FakeDevice::answering(1, "Seen");

    let result = browse(&browse_request(&[&device]), &store).unwrap();

    let json = serde_json::to_value(&result).unwrap();
    assert_eq!(json["routesNotSeen"], serde_json::json!([]));
}

#[spec_test(REQ_IL_ironlink_064)]
#[rstest]
#[case("routes.json")]
#[case("addresses.json")]
fn browse_when_a_store_file_is_unreadable_then_l6006_and_nothing_is_sent(#[case] name: &str) {
    let (_guard, store) = temp_store();
    let dir = store.dir().unwrap().to_path_buf();
    fs::create_dir_all(&dir).unwrap();
    fs::write(dir.join(name), "{").unwrap();
    let device = FakeDevice::answering(1, "Seen");

    let err = browse(&browse_request(&[&device]), &store).unwrap_err();

    assert_eq!(err.l_code(), "L6006");
    assert!(err.message().contains(name), "{err}");
    assert!(device.received(0).is_empty());
}
