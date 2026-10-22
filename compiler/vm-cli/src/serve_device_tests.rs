//! The device settings through the serve session: the `identity` handshake
//! reports them, and `setDeviceSettings` changes and persists them
//! (REQ-VC-vm-cli-027 through -029). The TCP and HA pair paths are covered
//! by the process tests in `tests/`.

use std::io;

use ironplc_runtime::RuntimeHost;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use crate::device_settings::device::{Device, TEST_DEVICE_ID};
use crate::serve::serve_session;
use crate::serve_tests::{container_bytes, counter_host, counter_source};

/// Runs `lines` through one session over `device`, returning the parsed
/// response per input line.
fn run(host: &mut RuntimeHost, device: &mut Device, lines: &[&str]) -> Vec<serde_json::Value> {
    let mut input = lines.join("\n");
    input.push('\n');
    let mut output = Vec::new();
    serve_session(
        host,
        io::Cursor::new(input.into_bytes()),
        &mut output,
        None,
        device,
        None,
    )
    .unwrap();
    String::from_utf8(output)
        .unwrap()
        .lines()
        .map(|line| serde_json::from_str(line).unwrap())
        .collect()
}

/// REQ-VC-vm-cli-029: `identity` reports the id and the name of the settings.
#[spec_test(REQ_VC_vm_cli_029)]
#[test]
fn serve_session_when_identity_then_device_block_carries_the_device_id_and_name() {
    let mut host = counter_host();
    let mut device = Device::in_memory();

    let responses = run(&mut host, &mut device, &[r#"{"command":"identity"}"#]);

    assert_eq!(responses[0]["device"]["deviceId"], TEST_DEVICE_ID);
    assert_eq!(responses[0]["device"]["name"], "ironplcvm");
    assert_eq!(responses[0]["device"]["model"], "IronPLC SoftPLC");
}

/// REQ-VC-vm-cli-027: the command changes the named settings and nothing
/// else, and the next `identity` reports them.
#[spec_test(REQ_VC_vm_cli_027)]
#[test]
fn serve_session_when_set_device_settings_then_ack_and_identity_reports_the_change() {
    let mut host = counter_host();
    let mut device = Device::in_memory();

    let responses = run(
        &mut host,
        &mut device,
        &[
            r#"{"command":"setDeviceSettings","name":"cell 1"}"#,
            r#"{"command":"identity"}"#,
        ],
    );

    assert_eq!(responses[0], serde_json::json!({"response": "ack"}));
    assert_eq!(responses[1]["device"]["name"], "cell 1");
    assert_eq!(responses[1]["device"]["deviceId"], TEST_DEVICE_ID);
}

/// REQ-VC-vm-cli-030: the id is not changed over the session. The command
/// that names it is refused as a whole, and a command that names the name
/// alone still works afterwards.
#[spec_test(REQ_VC_vm_cli_030)]
#[test]
fn serve_session_when_set_device_settings_names_the_device_id_then_v6015_and_identity_unchanged() {
    let mut host = counter_host();
    let mut device = Device::in_memory();
    let new_id = "c".repeat(32);
    let both =
        serde_json::json!({"command": "setDeviceSettings", "name": "cell 1", "deviceId": new_id})
            .to_string();
    let id_alone =
        serde_json::json!({"command": "setDeviceSettings", "deviceId": new_id}).to_string();

    let responses = run(
        &mut host,
        &mut device,
        &[
            &both,
            &id_alone,
            r#"{"command":"identity"}"#,
            r#"{"command":"setDeviceSettings","name":"cell 2"}"#,
            r#"{"command":"identity"}"#,
        ],
    );

    for refused in &responses[0..2] {
        assert_eq!(refused["response"], "error");
        assert_eq!(refused["vCode"], "V6015");
        assert!(refused["message"].as_str().unwrap().ends_with(
            "the field \"deviceId\" can only be changed on the device itself (its settings file)"
        ));
    }
    assert_eq!(responses[2]["device"]["name"], "ironplcvm");
    assert_eq!(responses[2]["device"]["deviceId"], TEST_DEVICE_ID);
    assert_eq!(responses[3]["response"], "ack");
    assert_eq!(responses[4]["device"]["name"], "cell 2");
    assert_eq!(responses[4]["device"]["deviceId"], TEST_DEVICE_ID);
}

#[test]
fn serve_session_when_set_device_settings_persists_then_a_restarted_device_reports_it() {
    let dir = TempDir::new().unwrap();
    let served = dir.path().join("app.iplc");
    let mut host = counter_host();
    let mut device = Device::open(&served).unwrap();
    let before = device.identity().device_id.unwrap();

    run(
        &mut host,
        &mut device,
        &[r#"{"command":"setDeviceSettings","name":"cell 1"}"#],
    );
    let restarted = Device::open(&served).unwrap().identity();

    assert_eq!(restarted.name, "cell 1");
    assert_eq!(restarted.device_id, Some(before));
}

/// REQ-VC-vm-cli-028: a refused change is a coded error, and the device and
/// the next `identity` are as before.
#[spec_test(REQ_VC_vm_cli_028)]
#[test]
fn serve_session_when_set_device_settings_invalid_then_v6015_and_identity_unchanged() {
    let mut host = counter_host();
    let mut device = Device::in_memory();

    let responses = run(
        &mut host,
        &mut device,
        &[
            r#"{"command":"setDeviceSettings","name":""}"#,
            r#"{"command":"setDeviceSettings","speed":"fast"}"#,
            r#"{"command":"identity"}"#,
        ],
    );

    assert_eq!(responses[0]["response"], "error");
    assert_eq!(responses[0]["vCode"], "V6015");
    assert!(responses[0]["message"]
        .as_str()
        .unwrap()
        .contains("\"name\""));
    assert_eq!(responses[1]["vCode"], "V6015");
    assert!(responses[1]["message"].as_str().unwrap().contains("speed"));
    assert_eq!(responses[2]["device"]["name"], "ironplcvm");
    assert_eq!(responses[2]["device"]["deviceId"], TEST_DEVICE_ID);
}

#[test]
fn serve_session_when_set_device_settings_cannot_write_then_v6016_and_identity_unchanged() {
    let dir = TempDir::new().unwrap();
    let served = dir.path().join("app.iplc");
    let mut host = counter_host();
    let mut device = Device::open(&served).unwrap();
    std::fs::create_dir(dir.path().join("app.iplc.device.json.tmp")).unwrap();

    let responses = run(
        &mut host,
        &mut device,
        &[
            r#"{"command":"setDeviceSettings","name":"cell 1"}"#,
            r#"{"command":"identity"}"#,
        ],
    );

    assert_eq!(responses[0]["vCode"], "V6016");
    assert_eq!(responses[1]["device"]["name"], "ironplcvm");
}

/// REQ-VC-vm-cli-028: the settings are not the application, so a staged
/// candidate does not block the command, and the command does not disturb it.
#[spec_test(REQ_VC_vm_cli_028)]
#[test]
fn serve_session_when_candidate_staged_then_set_device_settings_still_allowed() {
    let mut host = counter_host();
    let mut device = Device::in_memory();
    let accept = serde_json::json!({
        "command": "acceptEdits",
        "program": container_bytes(&counter_source(2)),
    })
    .to_string();

    let responses = run(
        &mut host,
        &mut device,
        &[
            &accept,
            r#"{"command":"setDeviceSettings","name":"cell 1"}"#,
            r#"{"command":"getStatus"}"#,
        ],
    );

    assert_eq!(responses[0]["response"], "ack");
    assert_eq!(responses[1]["response"], "ack");
    assert_eq!(responses[2]["candidate"], 2);
}
