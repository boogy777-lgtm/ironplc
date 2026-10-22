//! `ironplcc record-path` and `ironplcc record-path-path`, run as the editor
//! runs them.

// Test-target boundary: the workspace denies `unwrap` in production code; tests
// assert by panicking, so they are exempt here.
#![expect(
    clippy::unwrap_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use predicates::prelude::*;
use std::fs;
use std::path::{Path, PathBuf};
use std::process::Command;
use tempfile::TempDir;

const DEVICE: &str = "0123456789abcdef0123456789abcdef";
const OTHER_DEVICE: &str = "fedcba9876543210fedcba9876543210";

/// A directory and the path of an application container in it. The container
/// need not exist for a path record.
fn application() -> (TempDir, PathBuf) {
    let dir = TempDir::new().unwrap();
    let application = dir.path().join("app.iplc");
    (dir, application)
}

fn record_path(application: &Path, device_id: &str, address: &str) -> Command {
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));
    cmd.arg("record-path")
        .arg(application)
        .args(["--device-id", device_id, "--address", address])
        .args(["--port", "49152"]);
    cmd
}

fn record_of(dir: &TempDir) -> serde_json::Value {
    serde_json::from_str(&fs::read_to_string(dir.path().join("app.path.iplcinfo")).unwrap())
        .unwrap()
}

/// How many files are in `dir`.
fn files(dir: &TempDir) -> usize {
    fs::read_dir(dir.path()).unwrap().count()
}

#[test]
fn record_path_when_choice_then_prints_nothing_and_writes_record() {
    let (dir, application) = application();

    record_path(&application, DEVICE, "192.168.1.20")
        .assert()
        .success()
        .stdout(predicate::str::is_empty())
        .stderr(predicate::str::is_empty());

    let record = record_of(&dir);
    assert_eq!(record["version"], 1);
    assert_eq!(record["deviceId"], DEVICE);
    assert_eq!(
        record["routeHint"],
        serde_json::json!({"address": "192.168.1.20", "sessionPort": 49152})
    );
    assert!(record["chosenAt"].as_u64().unwrap() > 1_700_000_000_000);
}

#[test]
fn record_path_when_loopback_then_hint_is_null() {
    let (dir, application) = application();

    record_path(&application, DEVICE, "127.0.0.1")
        .assert()
        .success();

    let record = record_of(&dir);
    assert_eq!(record["deviceId"], DEVICE);
    assert_eq!(record["routeHint"], serde_json::Value::Null);
}

#[test]
fn record_path_when_loopback_after_lan_then_hint_of_same_device_is_kept() {
    let (dir, application) = application();
    record_path(&application, DEVICE, "plc1.local")
        .assert()
        .success();

    record_path(&application, DEVICE, "localhost")
        .assert()
        .success();

    assert_eq!(record_of(&dir)["routeHint"]["address"], "plc1.local");
}

#[test]
fn record_path_when_loopback_after_lan_of_another_device_then_hint_is_null() {
    let (dir, application) = application();
    record_path(&application, DEVICE, "10.0.0.1")
        .assert()
        .success();

    record_path(&application, OTHER_DEVICE, "::1")
        .assert()
        .success();

    let record = record_of(&dir);
    assert_eq!(record["deviceId"], OTHER_DEVICE);
    assert_eq!(record["routeHint"], serde_json::Value::Null);
}

#[test]
fn record_path_when_device_id_bad_then_usage_error_and_writes_nothing() {
    let (dir, application) = application();

    record_path(&application, "ABC", "10.0.0.1")
        .assert()
        .code(2)
        .stderr(predicate::str::contains("32 lowercase hexadecimal digits"));

    assert_eq!(files(&dir), 0);
}

#[test]
fn record_path_when_address_bad_then_usage_error_and_writes_nothing() {
    let (dir, application) = application();

    record_path(&application, DEVICE, "not an address")
        .assert()
        .code(2)
        .stderr(predicate::str::contains("IP address or a host name"));

    assert_eq!(files(&dir), 0);
}

#[test]
fn record_path_when_port_zero_then_usage_error_and_writes_nothing() {
    let (dir, application) = application();

    Command::new(cargo::cargo_bin!("ironplcc"))
        .arg("record-path")
        .arg(&application)
        .args(["--device-id", DEVICE, "--address", "10.0.0.1"])
        .args(["--port", "0"])
        .assert()
        .code(2);

    assert_eq!(files(&dir), 0);
}

#[test]
fn record_path_when_flags_missing_then_usage_error() {
    let (dir, application) = application();

    Command::new(cargo::cargo_bin!("ironplcc"))
        .arg("record-path")
        .arg(&application)
        .assert()
        .code(2)
        .stderr(predicate::str::contains("--device-id"));

    assert_eq!(files(&dir), 0);
}

#[test]
fn record_path_path_when_application_then_prints_the_path_of_the_record() {
    let (dir, application) = application();

    Command::new(cargo::cargo_bin!("ironplcc"))
        .arg("record-path-path")
        .arg(&application)
        .assert()
        .success()
        .stdout(format!(
            "{}\n",
            dir.path().join("app.path.iplcinfo").display()
        ));
}

#[test]
fn record_path_path_when_record_written_then_is_the_file_record_path_wrote() {
    let (dir, application) = application();
    record_path(&application, DEVICE, "10.0.0.1")
        .assert()
        .success();

    let output = Command::new(cargo::cargo_bin!("ironplcc"))
        .arg("record-path-path")
        .arg(&application)
        .output()
        .unwrap();

    let printed = PathBuf::from(String::from_utf8(output.stdout).unwrap().trim_end());
    assert!(printed.is_file());
    assert_eq!(printed.parent().unwrap(), dir.path());
}
