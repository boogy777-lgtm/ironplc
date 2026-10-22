//! Tests of the text rows.

use std::net::IpAddr;

use rstest::rstest;
use spec_test_macro::spec_test;

use super::*;
use crate::driver::DriverKind;
use crate::observation::{group, Method, Observation, OtherVersion, Route};
use crate::service::{VersionInfo, Workstation};

const NOW: u64 = 1_700_000_100_000;

fn observation(id_byte: u8, name: &str, from: &str, seen_ago_ms: u64) -> Observation {
    let address: IpAddr = from.parse().unwrap();
    Observation {
        device_id: crate::observation::device_id_hex(&[id_byte; 16]),
        name: name.to_string(),
        model: "IronPLC SoftPLC".to_string(),
        runtime_version: "0.247.0".to_string(),
        protocol: 1,
        session_port: 49152,
        protected: false,
        route: Route {
            driver: DriverKind::AddressList,
            address,
            session_port: 49152,
            method: Method::Tcp,
        },
        seen_at: NOW - seen_ago_ms,
    }
}

fn driver_with(
    observations: Vec<Observation>,
    other_versions: Vec<OtherVersion>,
    undecodable: u32,
) -> DriverReport {
    DriverReport {
        kind: DriverKind::AddressList,
        targets: vec!["192.168.1.50:49152".to_string()],
        devices: group(observations),
        other_versions,
        undecodable,
    }
}

fn browse_response(drivers: Vec<DriverReport>) -> Response {
    Response::Browse(BrowseResult {
        workstation: Workstation {
            name: "PC-ONE".to_string(),
        },
        drivers,
    })
}

fn lines_of(response: &Response) -> Vec<String> {
    text(response, NOW).lines().map(str::to_string).collect()
}

#[spec_test(REQ_IL_ironlink_050)]
#[test]
fn text_when_one_device_then_workstation_driver_and_a_row_with_the_fields() {
    let response = browse_response(vec![driver_with(
        vec![observation(1, "Press", "192.168.1.50", 3000)],
        Vec::new(),
        0,
    )]);

    let lines = lines_of(&response);

    assert_eq!(lines.len(), 3);
    assert_eq!(lines[0], "PC-ONE");
    assert_eq!(lines[1], "  address list: 192.168.1.50:49152");
    assert_eq!(
        lines[2],
        "    Press  IronPLC SoftPLC  runtime 0.247.0  id 01010101010101010101010101010101  \
         192.168.1.50:49152  seen 3s ago"
    );
}

#[spec_test(REQ_IL_ironlink_050)]
#[test]
fn text_when_protected_then_the_row_ends_with_the_protected_marker() {
    let mut protected = observation(1, "Press", "192.168.1.50", 0);
    protected.protected = true;
    let response = browse_response(vec![driver_with(vec![protected], Vec::new(), 0)]);

    let lines = lines_of(&response);

    assert!(lines[2].ends_with("seen 0s ago  protected"), "{}", lines[2]);
}

#[spec_test(REQ_IL_ironlink_050)]
#[test]
fn text_when_device_has_two_routes_then_the_second_is_indented_under_the_row() {
    let response = browse_response(vec![driver_with(
        vec![
            observation(1, "Press", "192.168.1.50", 3000),
            observation(1, "Press", "192.168.2.50", 61_000),
        ],
        Vec::new(),
        0,
    )]);

    let lines = lines_of(&response);

    assert_eq!(lines.len(), 4);
    assert!(lines[2].contains("192.168.1.50:49152"));
    assert_eq!(lines[3], "      also 192.168.2.50:49152  seen 1m ago");
}

#[spec_test(REQ_IL_ironlink_050)]
#[test]
fn text_when_identity_conflict_then_a_conflict_marker_and_each_answer() {
    let response = browse_response(vec![driver_with(
        vec![
            observation(1, "Press", "192.168.1.50", 0),
            observation(1, "Press clone", "192.168.1.51", 0),
        ],
        Vec::new(),
        0,
    )]);

    let lines = lines_of(&response);

    assert_eq!(lines.len(), 5);
    assert_eq!(
        lines[2],
        "    id 01010101010101010101010101010101  CONFLICT: 2 different answers under this id"
    );
    assert!(lines[3].starts_with("      Press  "));
    assert!(lines[4].starts_with("      Press clone  "));
    assert!(!lines[3].contains("id 0101"));
}

#[spec_test(REQ_IL_ironlink_050)]
#[test]
fn text_when_nothing_answered_then_the_driver_says_so() {
    let response = browse_response(vec![driver_with(Vec::new(), Vec::new(), 0)]);

    let lines = lines_of(&response);

    assert_eq!(lines[2], "    no device answered");
}

#[spec_test(REQ_IL_ironlink_050)]
#[test]
fn text_when_other_version_and_undecodable_then_they_are_reported_for_the_driver() {
    let other = OtherVersion {
        address: "192.168.1.60".parse().unwrap(),
        wire_version: 2,
        seen_at: NOW - 5000,
    };
    let response = browse_response(vec![driver_with(Vec::new(), vec![other], 3)]);

    let lines = lines_of(&response);

    assert_eq!(lines.len(), 4);
    assert_eq!(
        lines[2],
        "    192.168.1.60  speaks discovery version 2; update IronLink to see it  seen 5s ago"
    );
    assert_eq!(lines[3], "    3 datagram(s) were not discovery replies");
}

#[spec_test(REQ_IL_ironlink_050)]
#[test]
fn text_when_identify_finds_a_device_then_one_row_without_indent() {
    let result = IdentifyResult {
        address: "192.168.1.50:49152".to_string(),
        timeout_ms: 1000,
        answer: Answer::Device {
            observation: observation(1, "Press", "192.168.1.50", 2000),
        },
    };

    let rendered = text(&Response::Identify(result), NOW);

    assert_eq!(
        rendered,
        "Press  IronPLC SoftPLC  runtime 0.247.0  id 01010101010101010101010101010101  \
         192.168.1.50:49152  seen 2s ago"
    );
}

#[test]
fn text_when_identify_has_no_answer_then_says_where_and_how_long() {
    let result = IdentifyResult {
        address: "192.168.1.50:49152".to_string(),
        timeout_ms: 1000,
        answer: Answer::None,
    };

    assert_eq!(
        text(&Response::Identify(result), NOW),
        "no answer from 192.168.1.50:49152 within 1000 ms"
    );
}

#[test]
fn text_when_identify_hears_another_version_then_tells_to_update() {
    let result = IdentifyResult {
        address: "192.168.1.50:49152".to_string(),
        timeout_ms: 1000,
        answer: Answer::OtherVersion {
            other_version: OtherVersion {
                address: "192.168.1.50".parse().unwrap(),
                wire_version: 3,
                seen_at: NOW,
            },
        },
    };

    assert!(text(&Response::Identify(result), NOW).contains("speaks discovery version 3"));
}

#[test]
fn text_when_version_then_both_versions() {
    let response = Response::Version(VersionInfo {
        ironlink: "0.247.0",
        discovery_wire_version: 1,
    });

    assert_eq!(
        text(&response, NOW),
        "ironlink version 0.247.0 (discovery wire version 1)"
    );
}

#[rstest]
#[case(Some("L6003"), "no answer", "L6003 - no answer")]
#[case(None, "invalid command line", "invalid command line")]
fn text_when_error_then_code_dash_message_or_message_alone(
    #[case] l_code: Option<&'static str>,
    #[case] message: &str,
    #[case] expected: &str,
) {
    let response = Response::Error(crate::shell::ErrorResponse {
        l_code,
        message: message.to_string(),
    });

    assert_eq!(text(&response, NOW), expected);
}

#[rstest]
#[case(0, "0s")]
#[case(999, "0s")]
#[case(1000, "1s")]
#[case(59_999, "59s")]
#[case(60_000, "1m")]
#[case(3_599_000, "59m")]
#[case(3_600_000, "1h")]
#[case(90_000_000, "25h")]
fn age_when_milliseconds_ago_then_the_largest_whole_unit(
    #[case] ago_ms: u64,
    #[case] expected: &str,
) {
    assert_eq!(age(NOW, NOW - ago_ms), expected);
}

#[test]
fn age_when_seen_in_the_future_then_zero_seconds() {
    assert_eq!(age(NOW, NOW + 5000), "0s");
}
