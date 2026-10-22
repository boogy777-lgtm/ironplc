use std::path::PathBuf;

use spec_test_macro::spec_test;

use super::*;

fn stdio() -> Endpoint {
    Endpoint::Stdio(StdioEndpoint::serving(
        PathBuf::from("ironplcvm"),
        PathBuf::from("main.iplc"),
    ))
}

fn tcp() -> Endpoint {
    Endpoint::Tcp(TcpEndpoint {
        address: "127.0.0.1".to_string(),
        port: 49152,
    })
}

#[spec_test(REQ_SC_session_client_001)]
#[test]
fn endpoint_methods_when_declared_then_stdio_and_tcp_in_the_order_of_the_table() {
    assert_eq!(Endpoint::METHODS, &["stdio", "tcp"]);
}

#[spec_test(REQ_SC_session_client_001)]
#[test]
fn endpoint_method_when_each_member_then_its_name_is_in_the_table() {
    let members = [stdio(), tcp()];

    let names: Vec<&str> = members.iter().map(Endpoint::method).collect();

    assert_eq!(names, Endpoint::METHODS);
}

/// The extension of the editor has the same class: the `transport` of a
/// connection profile is one of its connection methods, and the manifest lists
/// them. The two lists name the same methods, or one side has a method the
/// other cannot reach.
#[spec_test(REQ_SC_session_client_001)]
#[test]
fn endpoint_methods_when_compared_with_the_editor_settings_then_the_same_names() {
    let manifest = std::fs::read_to_string(concat!(
        env!("CARGO_MANIFEST_DIR"),
        "/../../integrations/vscode/package.json"
    ))
    .unwrap();
    let manifest: serde_json::Value = serde_json::from_str(&manifest).unwrap();

    let transports = &manifest["contributes"]["configuration"]["properties"]["ironplc.connections"]
        ["items"]["properties"]["transport"]["enum"];

    assert_eq!(transports, &serde_json::json!(Endpoint::METHODS));
}
