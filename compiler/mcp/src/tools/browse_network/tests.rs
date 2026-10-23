use std::net::UdpSocket;
use std::path::PathBuf;

use ironplc_test::device::{workspace_binary, SoftDevice};
use spec_test_macro::spec_test;
use tempfile::TempDir;

use super::*;

/// The `ironlink` of this workspace with a store of its own.
fn ironlink(store: &TempDir) -> IronLink {
    IronLink::new(
        workspace_binary("ironlink"),
        Some(store.path().to_path_buf()),
    )
}

/// A UDP port nothing answers on.
fn silent_port() -> u16 {
    UdpSocket::bind("127.0.0.1:0")
        .unwrap()
        .local_addr()
        .unwrap()
        .port()
}

fn quick(addresses: Vec<String>) -> BrowseNetworkInput {
    BrowseNetworkInput {
        addresses,
        timeout_ms: Some(300),
        attempts: Some(1),
    }
}

#[spec_test(REQ_TOL_mcp_160)]
#[test]
fn browse_network_when_a_controller_answers_then_the_browse_result_names_it() {
    let dir = TempDir::new().unwrap();
    let container = dir.path().join("main.iplc");
    std::fs::write(
        &container,
        ironplc_container::test_support::container_bytes(
            &ironplc_container::test_support::steel_thread_debug_builder().build(),
        ),
    )
    .unwrap();
    let device = SoftDevice::start_discoverable(&container);
    let store = TempDir::new().unwrap();

    let response = build_response(
        &quick(vec![device.address().to_string()]),
        Ok(ironlink(&store)),
    );

    assert!(response.ok, "{response:?}");
    let browse = response.browse.unwrap();
    assert!(browse.get("response").is_none());
    let route = &browse["drivers"][0]["devices"][0]["variants"][0]["routes"][0];
    assert_eq!(route["sessionPort"], device.address().port());
    assert_eq!(route["address"], "127.0.0.1");
}

#[spec_test(REQ_TOL_mcp_160)]
#[test]
fn browse_network_when_nothing_answers_then_ok_with_no_devices() {
    let store = TempDir::new().unwrap();
    let input = quick(vec![format!("127.0.0.1:{}", silent_port())]);

    let response = build_response(&input, Ok(ironlink(&store)));

    assert!(response.ok, "{response:?}");
    let browse = response.browse.unwrap();
    assert!(browse["workstation"]["name"].is_string());
    assert_eq!(browse["drivers"][0]["devices"], serde_json::json!([]));
}

#[spec_test(REQ_TOL_mcp_162)]
#[test]
fn browse_network_when_ironlink_refuses_then_its_l_code_and_message() {
    let store = TempDir::new().unwrap();

    let response = build_response(&quick(vec!["not-an-address".into()]), Ok(ironlink(&store)));

    assert!(!response.ok);
    assert_eq!(response.l_code.as_deref(), Some("L6004"));
    assert!(response.message.is_some());
    assert!(response.browse.is_none());
}

#[spec_test(REQ_TOL_mcp_161)]
#[test]
fn browse_network_when_ironlink_is_not_found_then_p8002_diagnostic() {
    let response = build_response(
        &BrowseNetworkInput::default(),
        Err(IronLinkError::NotFound {
            searched: "the directory of the server and PATH".to_string(),
        }),
    );

    assert!(!response.ok);
    assert_eq!(response.diagnostics[0]["prefix"], "P");
    assert_eq!(response.diagnostics[0]["number"], 8002);
    assert!(response.diagnostics[0]["text"]
        .as_str()
        .unwrap()
        .contains("PATH"));
}

#[spec_test(REQ_TOL_mcp_163)]
#[test]
fn browse_network_when_ironlink_does_not_start_then_p8002_diagnostic() {
    let ironlink = IronLink::new(PathBuf::from("no-such-ironlink"), None);

    let response = build_response(&BrowseNetworkInput::default(), Ok(ironlink));

    assert!(!response.ok);
    assert_eq!(response.diagnostics[0]["number"], 8002);
}

#[test]
fn browse_command_when_only_some_fields_named_then_only_those_are_sent() {
    let command = browse_command(&BrowseNetworkInput {
        addresses: vec!["10.0.0.7".into()],
        timeout_ms: None,
        attempts: Some(3),
    });

    assert_eq!(
        command,
        serde_json::json!({"command": "browse", "addresses": ["10.0.0.7"], "attempts": 3})
    );
}
