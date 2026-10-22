//! Tests of browse, identify and the checking of what the user typed.

use std::time::{Duration, Instant};

use rstest::rstest;
use spec_test_macro::spec_test;

use super::*;
use crate::test_support::{Behaviour, FakeDevice, FakeIdentity};

const DEFAULT_PORT: u16 = DEFAULT_SESSION_PORT;

fn browse_addresses(addresses: &[&FakeDevice]) -> BrowseRequest {
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

fn identify_request(device: &FakeDevice, timeout_ms: u64) -> IdentifyRequest {
    IdentifyRequest {
        address: device.addr().to_string(),
        timeout_ms: Some(timeout_ms),
        attempts: Some(1),
    }
}

#[rstest]
#[case("192.168.1.50", "192.168.1.50:49152")]
#[case("192.168.1.50:5000", "192.168.1.50:5000")]
#[case(" 10.0.0.1 ", "10.0.0.1:49152")]
#[case("::1", "[::1]:49152")]
#[case("[::1]:5000", "[::1]:5000")]
fn parse_target_when_valid_then_bare_ip_gets_the_default_port(
    #[case] text: &str,
    #[case] expected: &str,
) {
    let address = parse_target(text, 49152).unwrap();

    assert_eq!(address.to_string(), expected);
}

#[rstest]
#[case("")]
#[case("printer")]
#[case("192.168.1")]
#[case("192.168.1.50:")]
#[case("192.168.1.50:99999")]
#[case("192.168.1.50:0")]
#[case("[::1]")]
fn parse_target_when_not_an_address_then_l6004_naming_the_text(#[case] text: &str) {
    let err = parse_target(text, 49152).unwrap_err();

    assert_eq!(err.l_code(), "L6004");
    assert!(err.message().contains(text), "{err}");
}

#[rstest]
#[case("192.168.1.20", "192.168.1.20")]
#[case("0.0.0.0", "0.0.0.0")]
#[case("fe80::1", "fe80::1")]
fn parse_interface_when_an_ip_address_then_accepted(#[case] text: &str, #[case] expected: &str) {
    assert_eq!(parse_interface(text).unwrap().to_string(), expected);
}

#[rstest]
#[case("Ethernet")]
#[case("192.168.1.20:80")]
#[case("")]
fn parse_interface_when_not_an_ip_address_then_l6004(#[case] text: &str) {
    let err = parse_interface(text).unwrap_err();

    assert_eq!(err.l_code(), "L6004");
    assert!(err.message().contains("by its address"), "{err}");
}

#[test]
fn selection_of_when_port_given_then_it_is_the_port_of_addresses_without_one() {
    let request = BrowseRequest {
        addresses: vec!["10.0.0.1".to_string(), "10.0.0.2:7000".to_string()],
        port: Some(5000),
        ..BrowseRequest::default()
    };

    let selection = selection_of(&request).unwrap();

    assert_eq!(selection.port, 5000);
    assert_eq!(
        selection
            .addresses
            .iter()
            .map(ToString::to_string)
            .collect::<Vec<_>>(),
        ["10.0.0.1:5000", "10.0.0.2:7000"]
    );
}

#[test]
fn selection_of_when_no_port_then_the_session_port() {
    let selection = selection_of(&BrowseRequest::default()).unwrap();

    assert_eq!(selection.port, DEFAULT_PORT);
}

#[test]
fn selection_of_when_port_is_zero_then_l6004() {
    let request = BrowseRequest {
        port: Some(0),
        ..BrowseRequest::default()
    };

    assert_eq!(selection_of(&request).unwrap_err().l_code(), "L6004");
}

#[test]
fn selection_of_when_an_address_is_named_twice_then_it_is_asked_once() {
    let request = BrowseRequest {
        interfaces: vec!["192.168.1.20".to_string(), "192.168.1.20".to_string()],
        addresses: vec!["10.0.0.1".to_string(), "10.0.0.1:49152".to_string()],
        ..BrowseRequest::default()
    };

    let selection = selection_of(&request).unwrap();

    assert_eq!(selection.interfaces.len(), 1);
    assert_eq!(selection.addresses.len(), 1);
}

#[spec_test(REQ_IL_ironlink_024)]
#[rstest]
#[case(Some("PC-ONE"), Some("host-one"), "PC-ONE")]
#[case(None, Some("host-one"), "host-one")]
#[case(Some("  "), Some("host-one"), "host-one")]
#[case(None, None, "this workstation")]
fn workstation_name_when_environment_given_then_computername_then_hostname_then_default(
    #[case] computername: Option<&str>,
    #[case] hostname: Option<&str>,
    #[case] expected: &str,
) {
    let name = workstation_name(|key| match key {
        "COMPUTERNAME" => computername.map(str::to_string),
        "HOSTNAME" => hostname.map(str::to_string),
        _ => None,
    });

    assert_eq!(name, expected);
}

#[spec_test(REQ_IL_ironlink_023)]
#[test]
fn browse_when_two_devices_answer_then_tree_of_workstation_and_one_driver_with_two_devices() {
    let first = FakeDevice::answering(1, "Press");
    let second = FakeDevice::answering(2, "Lathe");

    let result = browse(&browse_addresses(&[&first, &second])).unwrap();

    assert!(!result.workstation.name.is_empty());
    assert_eq!(result.drivers.len(), 1);
    let driver = &result.drivers[0];
    assert_eq!(driver.kind, DriverKind::AddressList);
    assert_eq!(
        driver.targets,
        [first.addr().to_string(), second.addr().to_string()]
    );
    let names: Vec<&str> = driver
        .devices
        .iter()
        .map(|device| device.variants[0].identity.name.as_str())
        .collect();
    assert_eq!(names, ["Lathe", "Press"]);
    assert_eq!(driver.undecodable, 0);
    assert!(driver.other_versions.is_empty());
}

#[spec_test(REQ_IL_ironlink_023)]
#[test]
fn browse_when_result_serialized_then_json_has_workstation_drivers_and_counts() {
    let device = FakeDevice::answering(1, "Press");

    let result = browse(&browse_addresses(&[&device])).unwrap();

    let json = serde_json::to_value(&result).unwrap();
    assert!(json["workstation"]["name"].is_string());
    assert_eq!(json["drivers"][0]["kind"], "addressList");
    assert_eq!(json["drivers"][0]["targets"][0], device.addr().to_string());
    assert_eq!(json["drivers"][0]["undecodable"], 0);
    assert_eq!(json["drivers"][0]["otherVersions"], serde_json::json!([]));
    assert_eq!(
        json["drivers"][0]["devices"][0]["variants"][0]["name"],
        "Press"
    );
}

#[spec_test(REQ_IL_ironlink_023)]
#[test]
fn browse_when_garbage_and_other_version_answer_then_counts_are_reported_per_driver() {
    let garbage = FakeDevice::start(FakeIdentity::new(1, "Noise"), Behaviour::Garbage);
    let newer = FakeDevice::start(FakeIdentity::new(2, "Newer"), Behaviour::WireVersion(2));

    let result = browse(&browse_addresses(&[&garbage, &newer])).unwrap();

    let driver = &result.drivers[0];
    assert!(driver.devices.is_empty());
    assert_eq!(driver.undecodable, 1);
    assert_eq!(driver.other_versions.len(), 1);
    assert_eq!(driver.other_versions[0].wire_version, 2);
}

#[test]
fn browse_when_interface_is_ipv6_then_l6004_before_anything_is_sent() {
    let request = BrowseRequest {
        interfaces: vec!["::1".to_string()],
        ..BrowseRequest::default()
    };

    assert_eq!(browse(&request).unwrap_err().l_code(), "L6004");
}

#[test]
fn browse_when_timeout_is_zero_then_l6004() {
    let request = BrowseRequest {
        addresses: vec!["127.0.0.1:9".to_string()],
        timeout_ms: Some(0),
        ..BrowseRequest::default()
    };

    assert_eq!(browse(&request).unwrap_err().l_code(), "L6004");
}

#[spec_test(REQ_IL_ironlink_030)]
#[test]
fn identify_when_device_answers_then_its_observation() {
    let device = FakeDevice::answering(3, "Press");

    let result = identify(&identify_request(&device, 1000)).unwrap();

    assert_eq!(result.address, device.addr().to_string());
    let json = serde_json::to_value(&result).unwrap();
    assert_eq!(json["observation"]["name"], "Press");
    assert_eq!(
        json["observation"]["deviceId"],
        "03030303030303030303030303030303"
    );
    assert_eq!(json["observation"]["route"]["driver"], "addressList");
}

#[spec_test(REQ_IL_ironlink_030)]
#[test]
fn identify_when_device_answers_then_it_does_not_wait_for_the_whole_timeout() {
    let device = FakeDevice::answering(3, "Press");
    let started = Instant::now();

    identify(&identify_request(&device, 20_000)).unwrap();

    assert!(started.elapsed() < Duration::from_secs(5));
}

#[spec_test(REQ_IL_ironlink_031)]
#[test]
fn identify_when_nothing_answers_then_the_none_answer_with_the_time_waited() {
    let device = FakeDevice::start(FakeIdentity::new(3, "Quiet"), Behaviour::Silent);

    let result = identify(&identify_request(&device, 300)).unwrap();

    assert_eq!(result.answer, Answer::None);
    assert_eq!(result.timeout_ms, 300);
}

#[spec_test(REQ_IL_ironlink_031)]
#[test]
fn identify_when_answer_serialized_then_the_json_names_the_answer() {
    let device = FakeDevice::answering(3, "Press");
    let silent = FakeDevice::start(FakeIdentity::new(4, "Quiet"), Behaviour::Silent);

    let found = serde_json::to_value(identify(&identify_request(&device, 1000)).unwrap()).unwrap();
    let none = serde_json::to_value(identify(&identify_request(&silent, 300)).unwrap()).unwrap();

    assert_eq!(found["answer"], "device");
    assert_eq!(found["observation"]["name"], "Press");
    assert_eq!(found["address"], device.addr().to_string());
    assert_eq!(none["answer"], "none");
    assert_eq!(none["timeoutMs"], 300);
    assert_eq!(none["address"], silent.addr().to_string());
}

#[spec_test(REQ_IL_ironlink_030)]
#[test]
fn identify_when_device_speaks_another_wire_version_then_the_other_version_answer() {
    let device = FakeDevice::start(FakeIdentity::new(3, "Newer"), Behaviour::WireVersion(2));

    let result = identify(&identify_request(&device, 1000)).unwrap();

    let json = serde_json::to_value(&result).unwrap();
    assert_eq!(json["answer"], "otherVersion");
    assert_eq!(json["otherVersion"]["wireVersion"], 2);
}

#[test]
fn identify_when_address_is_not_an_address_then_l6004() {
    let request = IdentifyRequest {
        address: "controller".to_string(),
        timeout_ms: None,
        attempts: None,
    };

    assert_eq!(identify(&request).unwrap_err().l_code(), "L6004");
}

#[spec_test(REQ_IL_ironlink_040)]
#[test]
fn version_when_asked_then_crate_version_and_discovery_wire_version() {
    let info = version();

    assert_eq!(info.ironlink, env!("CARGO_PKG_VERSION"));
    assert_eq!(info.discovery_wire_version, 1);
}
