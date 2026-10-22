//! Tests of the command protocol: lines fed to the serve loop over an in-memory
//! reader and writer.

use std::io::{self, Cursor, Write};

use rstest::rstest;
use serde_json::{json, Value};
use spec_test_macro::spec_test;

use super::*;
use crate::test_support::{temp_store, Behaviour, FakeDevice, FakeIdentity};

/// Feeds `lines` to the serve loop and returns the response lines.
fn session(lines: &[String]) -> Vec<Value> {
    let (_guard, store) = temp_store();
    session_in(lines, &store)
}

/// Feeds `lines` to the serve loop over `store` and returns the response lines.
fn session_in(lines: &[String], store: &Store) -> Vec<Value> {
    let input = lines.join("\n");
    let mut output = Vec::new();

    serve(Cursor::new(input), &mut output, store).unwrap();

    String::from_utf8(output)
        .unwrap()
        .lines()
        .map(|line| serde_json::from_str(line).unwrap())
        .collect()
}

fn browse_line(device: &FakeDevice) -> String {
    json!({
        "command": "browse",
        "addresses": [device.addr().to_string()],
        "timeoutMs": 300,
        "attempts": 1
    })
    .to_string()
}

fn identify_line(device: &FakeDevice) -> String {
    json!({
        "command": "identify",
        "address": device.addr().to_string(),
        "timeoutMs": 300,
        "attempts": 1
    })
    .to_string()
}

fn line(text: &str) -> String {
    text.to_string()
}

/// A writer that fails, like a closed pipe.
struct ClosedPipe;

impl Write for ClosedPipe {
    fn write(&mut self, _: &[u8]) -> io::Result<usize> {
        Err(io::Error::new(io::ErrorKind::BrokenPipe, "closed"))
    }

    fn flush(&mut self) -> io::Result<()> {
        Ok(())
    }
}

#[spec_test(REQ_IL_ironlink_040)]
#[test]
fn serve_when_version_then_one_line_with_the_versions() {
    let responses = session(&[line(r#"{"command":"version"}"#)]);

    assert_eq!(responses.len(), 1);
    assert_eq!(responses[0]["response"], "version");
    assert_eq!(responses[0]["ironlink"], env!("CARGO_PKG_VERSION"));
    assert_eq!(responses[0]["discoveryWireVersion"], 1);
}

#[spec_test(REQ_IL_ironlink_040)]
#[test]
fn serve_when_browse_with_an_address_list_then_the_tree_of_what_answered() {
    let device = FakeDevice::answering(1, "Press");

    let responses = session(&[browse_line(&device)]);

    assert_eq!(responses.len(), 1);
    assert_eq!(responses[0]["response"], "browse");
    assert!(responses[0]["workstation"]["name"].is_string());
    assert_eq!(responses[0]["drivers"][0]["kind"], "addressList");
    assert_eq!(
        responses[0]["drivers"][0]["devices"][0]["deviceId"],
        "01010101010101010101010101010101"
    );
}

#[spec_test(REQ_IL_ironlink_040)]
#[test]
fn serve_when_identify_then_the_observation() {
    let device = FakeDevice::answering(2, "Lathe");

    let responses = session(&[identify_line(&device)]);

    assert_eq!(responses[0]["response"], "identify");
    assert_eq!(responses[0]["answer"], "device");
    assert_eq!(responses[0]["observation"]["name"], "Lathe");
    assert_eq!(responses[0]["observation"]["route"]["method"], "tcp");
}

#[spec_test(REQ_IL_ironlink_031)]
#[test]
fn serve_when_identify_gets_no_answer_then_a_normal_response_and_the_loop_goes_on() {
    let silent = FakeDevice::start(FakeIdentity::new(3, "Quiet"), Behaviour::Silent);

    let responses = session(&[identify_line(&silent), line(r#"{"command":"version"}"#)]);

    assert_eq!(responses.len(), 2);
    assert_eq!(responses[0]["response"], "identify");
    assert_eq!(responses[0]["answer"], "none");
    assert_eq!(responses[1]["response"], "version");
}

#[spec_test(REQ_IL_ironlink_040)]
#[test]
fn serve_when_several_commands_then_one_response_line_for_each_in_order() {
    let device = FakeDevice::answering(1, "Press");

    let responses = session(&[
        line(r#"{"command":"version"}"#),
        browse_line(&device),
        identify_line(&device),
        line(r#"{"command":"version"}"#),
    ]);

    let kinds: Vec<&str> = responses
        .iter()
        .map(|response| response["response"].as_str().unwrap())
        .collect();
    assert_eq!(kinds, ["version", "browse", "identify", "version"]);
}

#[spec_test(REQ_IL_ironlink_041)]
#[test]
fn serve_when_line_is_not_json_then_codeless_codec_error_and_the_loop_goes_on() {
    let responses = session(&[line("this is not json"), line(r#"{"command":"version"}"#)]);

    assert_eq!(responses.len(), 2);
    assert_eq!(responses[0]["response"], "error");
    assert_eq!(responses[0]["lCode"], Value::Null);
    assert!(responses[0]["message"]
        .as_str()
        .unwrap()
        .starts_with("invalid command line"));
    assert_eq!(responses[1]["response"], "version");
}

#[spec_test(REQ_IL_ironlink_041)]
#[rstest]
#[case(r#"{"command":"frobnicate"}"#)]
#[case(r#"{"command":"identify"}"#)]
#[case(r#"{"command":7}"#)]
#[case(r#"{"nothing":"here"}"#)]
#[case(r#"[]"#)]
#[case("")]
fn serve_when_line_is_not_a_command_then_codeless_codec_error(#[case] input: &str) {
    let responses = session(&[line(input), line(r#"{"command":"version"}"#)]);

    assert_eq!(responses.len(), 2);
    assert_eq!(responses[0]["response"], "error");
    assert_eq!(responses[0]["lCode"], Value::Null);
}

#[spec_test(REQ_IL_ironlink_042)]
#[test]
fn serve_when_command_fails_then_error_response_with_its_l_code_and_the_loop_goes_on() {
    let responses = session(&[
        line(r#"{"command":"identify","address":"controller"}"#),
        line(r#"{"command":"browse","interfaces":["::1"]}"#),
        line(r#"{"command":"browse","addresses":["127.0.0.1:9"],"timeoutMs":0}"#),
        line(r#"{"command":"version"}"#),
    ]);

    assert_eq!(responses.len(), 4);
    assert_eq!(responses[0]["response"], "error");
    assert_eq!(responses[0]["lCode"], "L6004");
    assert_eq!(responses[1]["lCode"], "L6004");
    assert_eq!(responses[2]["lCode"], "L6004");
    assert_eq!(responses[3]["response"], "version");
}

#[spec_test(REQ_IL_ironlink_043)]
#[test]
fn serve_when_input_is_empty_then_no_output_and_ok() {
    let (_guard, store) = temp_store();
    let mut output = Vec::new();

    let result = serve(Cursor::new(""), &mut output, &store);

    assert!(result.is_ok());
    assert!(output.is_empty());
}

#[spec_test(REQ_IL_ironlink_043)]
#[test]
fn serve_when_input_ends_after_the_last_command_then_that_command_is_answered_and_it_ends() {
    let responses = session(&[line(r#"{"command":"version"}"#)]);

    assert_eq!(responses.len(), 1);
}

#[test]
fn serve_when_the_output_cannot_be_written_then_l6005() {
    let input = Cursor::new(r#"{"command":"version"}"#);

    let (_guard, store) = temp_store();

    let err = serve(input, ClosedPipe, &store).unwrap_err();

    assert_eq!(err.l_code(), "L6005");
}

#[test]
fn execute_when_version_then_the_version_response() {
    let (_guard, store) = temp_store();

    let response = execute(&Command::Version, &store).unwrap();

    assert!(matches!(response, Response::Version(_)));
}

#[test]
fn command_when_browse_has_no_fields_then_defaults() {
    let command: Command = serde_json::from_str(r#"{"command":"browse"}"#).unwrap();

    assert_eq!(command, Command::Browse(BrowseRequest::default()));
}

#[test]
fn command_when_a_field_is_unknown_then_it_is_ignored_like_the_session_protocol() {
    let command: Command =
        serde_json::from_str(r#"{"command":"version","from":"a later client"}"#).unwrap();

    assert_eq!(command, Command::Version);
}

fn route_set_line(device_id: &str, device: &FakeDevice) -> String {
    json!({
        "command": "routeSet",
        "deviceId": device_id,
        "address": device.addr().to_string(),
        "timeoutMs": 300,
        "attempts": 1
    })
    .to_string()
}

#[spec_test(REQ_IL_ironlink_077)]
#[test]
fn serve_when_routes_are_set_listed_and_removed_then_one_response_for_each_command() {
    let (_guard, store) = temp_store();
    let device = FakeDevice::answering(1, "Press");
    let id = "01".repeat(16);

    let responses = session_in(
        &[
            route_set_line(&id, &device),
            line(r#"{"command":"routeList"}"#),
            json!({"command": "routeRemove", "deviceId": id}).to_string(),
            json!({"command": "routeRemove", "deviceId": id}).to_string(),
            line(r#"{"command":"routeList"}"#),
        ],
        &store,
    );

    assert_eq!(responses.len(), 5);
    assert_eq!(responses[0]["response"], "routeSet");
    assert_eq!(responses[0]["deviceId"], id);
    assert_eq!(responses[0]["replaced"], false);
    assert_eq!(responses[0]["route"]["address"], "127.0.0.1");
    assert_eq!(responses[0]["route"]["driver"], "addressList");
    assert_eq!(responses[0]["route"]["sessionPort"], device.addr().port());
    assert!(responses[0]["route"]["verifiedAt"].is_u64());
    assert_eq!(responses[1]["response"], "routeList");
    assert_eq!(responses[1]["routes"][0]["deviceId"], id);
    assert_eq!(responses[2]["response"], "routeRemove");
    assert_eq!(responses[2]["removed"], true);
    assert_eq!(responses[3]["removed"], false);
    assert_eq!(responses[4]["routes"], json!([]));
}

#[spec_test(REQ_IL_ironlink_077)]
#[test]
fn serve_when_addresses_are_added_listed_and_removed_then_one_response_for_each_command() {
    let (_guard, store) = temp_store();

    let responses = session_in(
        &[
            line(r#"{"command":"addressAdd","address":"10.0.0.2:7000"}"#),
            line(r#"{"command":"addressAdd","address":"10.0.0.1"}"#),
            line(r#"{"command":"addressAdd","address":"10.0.0.1"}"#),
            line(r#"{"command":"addressList"}"#),
            line(r#"{"command":"addressRemove","address":"10.0.0.2:7000"}"#),
            line(r#"{"command":"addressList"}"#),
        ],
        &store,
    );

    assert_eq!(responses.len(), 6);
    assert_eq!(responses[0]["response"], "addressAdd");
    assert_eq!(responses[0]["address"], "10.0.0.2:7000");
    assert_eq!(responses[0]["added"], true);
    assert_eq!(responses[1]["address"], "10.0.0.1:49152");
    assert_eq!(responses[2]["added"], false);
    assert_eq!(responses[3]["response"], "addressList");
    assert_eq!(
        responses[3]["addresses"],
        json!(["10.0.0.1:49152", "10.0.0.2:7000"])
    );
    assert_eq!(responses[4]["response"], "addressRemove");
    assert_eq!(responses[4]["removed"], true);
    assert_eq!(responses[5]["addresses"], json!(["10.0.0.1:49152"]));
}

#[spec_test(REQ_IL_ironlink_077)]
#[test]
fn serve_when_a_store_command_fails_then_an_error_response_with_its_l_code_and_the_loop_goes_on() {
    let (_guard, store) = temp_store();
    let device = FakeDevice::answering(1, "Press");
    let quiet = FakeDevice::start(FakeIdentity::new(2, "Quiet"), Behaviour::Silent);

    let responses = session_in(
        &[
            route_set_line("not-an-id", &device),
            route_set_line(&"09".repeat(16), &device),
            route_set_line(&"02".repeat(16), &quiet),
            line(r#"{"command":"addressAdd","address":"printer"}"#),
            line(r#"{"command":"routeSet","deviceId":"01"}"#),
            line(r#"{"command":"routeList"}"#),
        ],
        &store,
    );

    assert_eq!(responses.len(), 6);
    let codes: Vec<&Value> = responses
        .iter()
        .map(|response| &response["lCode"])
        .collect();
    assert_eq!(codes[0], "L6004");
    assert_eq!(codes[1], "L6008");
    assert_eq!(codes[2], "L6003");
    assert_eq!(codes[3], "L6004");
    assert_eq!(responses[4]["response"], "error");
    assert_eq!(codes[4], &Value::Null);
    assert_eq!(responses[5]["response"], "routeList");
    assert_eq!(responses[5]["routes"], json!([]));
}

#[spec_test(REQ_IL_ironlink_077)]
#[test]
fn serve_when_the_store_is_shared_then_a_browse_uses_what_the_commands_stored() {
    let (_guard, store) = temp_store();
    let device = FakeDevice::answering(1, "Press");
    let id = "01".repeat(16);

    let responses = session_in(
        &[
            json!({"command": "addressAdd", "address": device.addr().to_string()}).to_string(),
            route_set_line(&id, &device),
            json!({
                "command": "browse",
                "addresses": [device.addr().to_string()],
                "timeoutMs": 300,
                "attempts": 1
            })
            .to_string(),
        ],
        &store,
    );

    let driver = &responses[2]["drivers"][0];
    assert_eq!(driver["kind"], "addressList");
    assert_eq!(driver["targets"], json!([device.addr().to_string()]));
    assert_eq!(
        driver["devices"][0]["variants"][0]["routes"][0]["routed"],
        true
    );
    assert_eq!(responses[2]["routesNotSeen"], json!([]));
}

#[test]
fn command_when_a_store_command_has_no_fields_then_it_is_parsed() {
    let list: Command = serde_json::from_str(r#"{"command":"routeList"}"#).unwrap();
    let addresses: Command = serde_json::from_str(r#"{"command":"addressList"}"#).unwrap();

    assert_eq!(list, Command::RouteList);
    assert_eq!(addresses, Command::AddressList);
}
