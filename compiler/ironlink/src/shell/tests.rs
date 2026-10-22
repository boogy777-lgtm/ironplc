//! Tests of the command protocol: lines fed to the serve loop over an in-memory
//! reader and writer.

use std::io::{self, Cursor, Write};

use rstest::rstest;
use serde_json::{json, Value};
use spec_test_macro::spec_test;

use super::*;
use crate::test_support::{Behaviour, FakeDevice, FakeIdentity};

/// Feeds `lines` to the serve loop and returns the response lines.
fn session(lines: &[String]) -> Vec<Value> {
    let input = lines.join("\n");
    let mut output = Vec::new();

    serve(Cursor::new(input), &mut output).unwrap();

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
    let mut output = Vec::new();

    let result = serve(Cursor::new(""), &mut output);

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

    let err = serve(input, ClosedPipe).unwrap_err();

    assert_eq!(err.l_code(), "L6005");
}

#[test]
fn execute_when_version_then_the_version_response() {
    let response = execute(&Command::Version).unwrap();

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
