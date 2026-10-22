use ironplc_session_client::Session;
use ironplc_test::device::{workspace_binary, SoftDevice};
use rstest::rstest;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use super::*;

fn address(address: Option<&str>, port: Option<u16>, device_id: Option<&str>) -> DeviceInput {
    DeviceInput {
        address: address.map(str::to_string),
        port,
        device_id: device_id.map(str::to_string),
    }
}

fn no_ironlink() -> Result<IronLink, IronLinkError> {
    Err(IronLinkError::NotFound {
        searched: "nowhere".to_string(),
    })
}

/// The container every device of these tests serves.
fn container_bytes() -> Vec<u8> {
    ironplc_container::test_support::container_bytes(
        &ironplc_container::test_support::steel_thread_debug_builder().build(),
    )
}

fn tcp(address: &str, port: u16) -> Endpoint {
    Endpoint::Tcp(TcpEndpoint {
        address: address.to_string(),
        port,
    })
}

#[spec_test(REQ_TOL_mcp_174)]
#[test]
fn resolve_when_address_only_then_tcp_on_the_session_port() {
    let resolved = resolve(&address(Some("10.0.0.7"), None, None), no_ironlink).unwrap();

    assert_eq!(resolved.endpoint, tcp("10.0.0.7", DEFAULT_SESSION_PORT));
    assert_eq!(resolved.requested_id, None);
}

#[test]
fn resolve_when_address_and_port_then_tcp_on_that_port() {
    let resolved = resolve(&address(Some("plc-1"), Some(50000), None), no_ironlink).unwrap();

    assert_eq!(resolved.endpoint, tcp("plc-1", 50000));
    assert_eq!(resolved.describe(), "plc-1:50000");
}

#[spec_test(REQ_TOL_mcp_174)]
#[rstest]
#[case::neither(address(None, None, None))]
#[case::both(address(Some("10.0.0.7"), None, Some("00112233445566778899aabbccddeeff")))]
#[case::port_with_id(address(None, Some(50000), Some("00112233445566778899aabbccddeeff")))]
#[case::port_alone(address(None, Some(50000), None))]
fn resolve_when_the_selector_is_not_one_way_to_name_a_device_then_invalid(
    #[case] device: DeviceInput,
) {
    let result = resolve(&device, no_ironlink);

    assert_eq!(
        result,
        Err(DeviceFault::Invalid(
            text::mcp_input_validation_device_selector_invalid()
        ))
    );
}

#[spec_test(REQ_TOL_mcp_174)]
#[test]
fn resolve_when_id_and_ironlink_missing_then_the_ironlink_fault() {
    let result = resolve(
        &address(None, None, Some("00112233445566778899aabbccddeeff")),
        no_ironlink,
    );

    assert!(matches!(
        result,
        Err(DeviceFault::IronLink(IronLinkError::NotFound { .. }))
    ));
}

#[spec_test(REQ_TOL_mcp_174)]
#[test]
fn resolve_when_id_has_no_route_then_invalid_that_says_to_browse() {
    let store = TempDir::new().unwrap();
    let ironlink = IronLink::new(
        workspace_binary("ironlink"),
        Some(store.path().to_path_buf()),
    );
    let id = "00112233445566778899aabbccddeeff";

    let result = resolve(&address(None, None, Some(id)), || Ok(ironlink));

    assert_eq!(
        result,
        Err(DeviceFault::Invalid(
            text::mcp_input_validation_device_id_no_route(id.to_string())
        ))
    );
}

#[spec_test(REQ_TOL_mcp_174)]
#[test]
fn resolve_when_id_has_a_stored_route_then_that_route_whatever_the_case_of_the_id() {
    let dir = TempDir::new().unwrap();
    let container = dir.path().join("main.iplc");
    std::fs::write(&container, container_bytes()).unwrap();
    let device = SoftDevice::start_discoverable(&container);
    let id = {
        let session = Session::connect(&tcp("127.0.0.1", device.address().port())).unwrap();
        session
            .identity()
            .unwrap()
            .device
            .device_id
            .clone()
            .unwrap()
    };
    let store = TempDir::new().unwrap();
    let ironlink = IronLink::new(
        workspace_binary("ironlink"),
        Some(store.path().to_path_buf()),
    );
    ironlink
        .call(
            &serde_json::json!({
                "command": "routeSet",
                "deviceId": id,
                "address": device.address().to_string(),
                "timeoutMs": 500
            }),
            std::time::Duration::from_millis(500),
        )
        .unwrap();

    let resolved = resolve(&address(None, None, Some(&id.to_ascii_uppercase())), || {
        Ok(ironlink)
    })
    .unwrap();

    assert_eq!(resolved.endpoint, tcp("127.0.0.1", device.address().port()));
    assert_eq!(resolved.requested_id, Some(id));
}
