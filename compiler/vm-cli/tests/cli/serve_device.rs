//! Tests of the device settings of `ironplcvm serve`: the id a device
//! generates, keeps across restarts and lets its operator change, and the
//! refusal to start on a settings file that cannot be used.

use std::path::Path;

use assert_cmd::cargo;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use crate::{write_compiled_container, COUNTER_PROGRAM};

/// One `ironplcvm serve` process fed `stdin`: its exit status and the
/// response lines it wrote.
fn serve_lines(path: &Path, stdin: &str) -> (std::process::Output, Vec<serde_json::Value>) {
    let output = assert_cmd::Command::new(cargo::cargo_bin!("ironplcvm"))
        .arg("serve")
        .arg(path)
        .write_stdin(stdin)
        .output()
        .unwrap();
    let lines = String::from_utf8(output.stdout.clone())
        .unwrap()
        .lines()
        .map(|line| serde_json::from_str(line).unwrap())
        .collect();
    (output, lines)
}

/// The container `counter.iplc` in a fresh directory.
fn served(dir: &TempDir) -> std::path::PathBuf {
    let path = dir.path().join("counter.iplc");
    write_compiled_container(&path, COUNTER_PROGRAM);
    path
}

fn settings_file(dir: &TempDir) -> std::path::PathBuf {
    dir.path().join("counter.iplc.device.json")
}

const IDENTITY: &str = "{\"command\":\"identity\"}\n";

/// REQ-VC-vm-cli-025: the second start of the same device reports the id the
/// first start generated, and a second device reports another.
#[spec_test(REQ_VC_vm_cli_025)]
#[test]
fn serve_when_started_twice_then_the_same_device_id_and_another_device_has_another() {
    let dir = TempDir::new().unwrap();
    let other = TempDir::new().unwrap();

    let (_, first) = serve_lines(&served(&dir), IDENTITY);
    let (_, second) = serve_lines(&served(&dir), IDENTITY);
    let (_, other_device) = serve_lines(&served(&other), IDENTITY);

    let id = first[0]["device"]["deviceId"].as_str().unwrap();
    assert_eq!(id.len(), 32);
    assert_eq!(second[0]["device"]["deviceId"], id);
    assert_ne!(other_device[0]["device"]["deviceId"], id);
    let file: serde_json::Value =
        serde_json::from_str(&std::fs::read_to_string(settings_file(&dir)).unwrap()).unwrap();
    assert_eq!(file["deviceId"], id);
}

/// REQ-VC-vm-cli-027: a change made through the session is in the file and
/// is the device after a restart.
#[spec_test(REQ_VC_vm_cli_027)]
#[test]
fn serve_when_settings_changed_through_the_session_then_the_restart_reports_them() {
    let dir = TempDir::new().unwrap();
    let path = served(&dir);
    let new_id = "d".repeat(32);
    let set =
        serde_json::json!({"command": "setDeviceSettings", "name": "cell 1", "deviceId": new_id});

    let (_, first) = serve_lines(&path, &format!("{set}\n{IDENTITY}"));
    let (_, second) = serve_lines(&path, IDENTITY);

    assert_eq!(first[0]["response"], "ack");
    assert_eq!(first[1]["device"]["deviceId"], new_id.as_str());
    assert_eq!(second[0]["device"]["deviceId"], new_id.as_str());
    assert_eq!(second[0]["device"]["name"], "cell 1");
}

/// REQ-VC-vm-cli-026: a damaged settings file stops the start with exit 2 and
/// V6015, and the file stays as it was.
#[spec_test(REQ_VC_vm_cli_026)]
#[test]
fn serve_when_settings_file_is_corrupt_then_exit_2_v6015_and_file_untouched() {
    let dir = TempDir::new().unwrap();
    let path = served(&dir);
    serve_lines(&path, IDENTITY);
    std::fs::write(settings_file(&dir), "{ \"version\": 1, \"deviceId\": ").unwrap();

    let (output, lines) = serve_lines(&path, IDENTITY);

    assert_eq!(output.status.code(), Some(2));
    assert!(lines.is_empty());
    let stderr = String::from_utf8(output.stderr).unwrap();
    assert!(stderr.contains("V6015"), "{stderr}");
    assert!(stderr.contains("counter.iplc.device.json"), "{stderr}");
    assert_eq!(
        std::fs::read_to_string(settings_file(&dir)).unwrap(),
        "{ \"version\": 1, \"deviceId\": "
    );
}

#[test]
fn serve_when_settings_file_has_a_bad_id_then_exit_2_v6015_naming_the_field() {
    let dir = TempDir::new().unwrap();
    let path = served(&dir);
    std::fs::write(settings_file(&dir), "{\"version\":1,\"deviceId\":\"ABC\"}").unwrap();

    let (output, _) = serve_lines(&path, IDENTITY);

    assert_eq!(output.status.code(), Some(2));
    let stderr = String::from_utf8(output.stderr).unwrap();
    assert!(stderr.contains("V6015"), "{stderr}");
    assert!(stderr.contains("\"deviceId\""), "{stderr}");
}
