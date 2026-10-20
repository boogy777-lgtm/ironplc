//! `ironplcc record-download` and `ironplcc record-download-path`, run as the
//! editor runs them.

// Test-target boundary: the workspace denies `unwrap` in production code; tests
// assert by panicking, so they are exempt here.
#![expect(
    clippy::unwrap_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use ironplc_container::FileHeader;
use predicates::prelude::*;
use std::fs;
use std::path::{Path, PathBuf};
use std::process::Command;
use tempfile::TempDir;

const PROGRAM: &str = "PROGRAM main
VAR y : INT; END_VAR
  y := 1;
END_PROGRAM
";

/// Compiles a small program to `app.iplc` in a new directory.
fn compiled_application() -> (TempDir, PathBuf) {
    let dir = TempDir::new().unwrap();
    let source = dir.path().join("main.st");
    fs::write(&source, PROGRAM).unwrap();
    let application = dir.path().join("app.iplc");
    Command::new(cargo::cargo_bin!("ironplcc"))
        .arg("compile")
        .arg(&source)
        .arg("-o")
        .arg(&application)
        .assert()
        .success();
    (dir, application)
}

fn record_download(application: &Path) -> Command {
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));
    cmd.arg("record-download").arg(application);
    cmd
}

/// How many download records are in `dir`.
fn download_records(dir: &TempDir) -> usize {
    fs::read_dir(dir.path())
        .unwrap()
        .filter(|entry| {
            let name = entry.as_ref().unwrap().file_name();
            name.to_string_lossy().ends_with(".download.iplcinfo")
        })
        .count()
}

fn record_of(dir: &TempDir, name: &str) -> serde_json::Value {
    serde_json::from_str(&fs::read_to_string(dir.path().join(name)).unwrap()).unwrap()
}

#[test]
fn record_download_when_compiled_container_then_prints_nothing_and_writes_record() {
    let (dir, application) = compiled_application();

    record_download(&application)
        .args(["--connection", "My PLC"])
        .assert()
        .success()
        .stdout(predicate::str::is_empty())
        .stderr(predicate::str::is_empty());

    let record = record_of(&dir, "app.My%20PLC.download.iplcinfo");
    assert_eq!(record["version"], 1);
    assert_eq!(record["connection"], "My PLC");
    // The application is the one the build record of the compile names.
    let build = record_of(&dir, "app.build.iplcinfo");
    assert_eq!(record["application"], build["application"]);
    assert_eq!(record["container"], build["container"]);
    assert_eq!(
        record["container"]["contentHash"].as_str().unwrap().len(),
        64
    );
}

#[test]
fn record_download_when_generations_and_matching_device_hash_then_record_holds_them() {
    let (dir, application) = compiled_application();
    record_download(&application)
        .args(["--connection", "plc"])
        .assert()
        .success();
    let hash = record_of(&dir, "app.plc.download.iplcinfo")["container"]["contentHash"]
        .as_str()
        .unwrap()
        .to_owned();

    record_download(&application)
        .args(["--connection", "plc", "--device-content-hash", &hash])
        .args(["--active", "3", "--normal", "2", "--application", "2"])
        .assert()
        .success()
        .stdout(predicate::str::is_empty());

    let record = record_of(&dir, "app.plc.download.iplcinfo");
    assert_eq!(
        record["device"],
        serde_json::json!({"active": 3, "normal": 2, "application": 2})
    );
}

#[test]
fn record_download_when_device_hash_differs_then_fails_and_writes_nothing() {
    let (dir, application) = compiled_application();

    record_download(&application)
        .args([
            "--connection",
            "plc",
            "--device-content-hash",
            &"ab".repeat(32),
        ])
        .assert()
        .failure()
        .stderr(predicate::str::contains("did not receive this container"));

    assert!(!dir.path().join("app.plc.download.iplcinfo").exists());
}

#[test]
fn record_download_when_container_flag_then_identity_of_that_file() {
    let (dir, application) = compiled_application();
    let other = dir.path().join("sent.iplc");
    let mut bytes = Vec::new();
    FileHeader {
        content_hash: [7; 32],
        ..FileHeader::default()
    }
    .write_to(&mut bytes)
    .unwrap();
    fs::write(&other, bytes).unwrap();

    record_download(&application)
        .args(["--connection", "plc", "--container"])
        .arg(&other)
        .assert()
        .success();

    let record = record_of(&dir, "app.plc.download.iplcinfo");
    assert_eq!(record["container"]["contentHash"], "07".repeat(32));
}

#[test]
fn record_download_when_file_missing_then_fails_with_message() {
    let dir = TempDir::new().unwrap();

    record_download(&dir.path().join("missing.iplc"))
        .args(["--connection", "plc"])
        .assert()
        .failure()
        .stderr(predicate::str::contains("missing.iplc"));

    assert_eq!(download_records(&dir), 0);
}

#[test]
fn record_download_when_file_is_not_a_container_then_fails() {
    let dir = TempDir::new().unwrap();
    let application = dir.path().join("app.iplc");
    fs::write(&application, "not a container").unwrap();

    record_download(&application)
        .args(["--connection", "plc"])
        .assert()
        .failure()
        .stderr(predicate::str::contains("valid container header"));
}

#[test]
fn record_download_when_connection_empty_then_fails_and_writes_nothing() {
    let (dir, application) = compiled_application();

    record_download(&application)
        .args(["--connection", ""])
        .assert()
        .failure()
        .stderr(predicate::str::contains("must not be empty"));

    assert_eq!(download_records(&dir), 0);
}

#[test]
fn record_download_when_connection_missing_then_usage_error() {
    let (_dir, application) = compiled_application();

    record_download(&application)
        .assert()
        .failure()
        .stderr(predicate::str::contains("--connection"));
}

#[test]
fn record_download_when_only_some_generations_then_usage_error() {
    let (dir, application) = compiled_application();

    record_download(&application)
        .args(["--connection", "plc", "--active", "1"])
        .assert()
        .failure();

    assert!(!dir.path().join("app.plc.download.iplcinfo").exists());
}

#[test]
fn record_download_when_device_hash_not_hex_then_usage_error() {
    let (_dir, application) = compiled_application();

    record_download(&application)
        .args(["--connection", "plc", "--device-content-hash", "xyz"])
        .assert()
        .failure()
        .stderr(predicate::str::contains("64 hexadecimal digits"));
}

#[test]
fn record_download_path_when_connection_then_prints_the_path_of_the_record() {
    let dir = TempDir::new().unwrap();
    let application = dir.path().join("app.iplc");

    Command::new(cargo::cargo_bin!("ironplcc"))
        .arg("record-download-path")
        .arg(&application)
        .args(["--connection", "My PLC:1"])
        .assert()
        .success()
        .stdout(format!(
            "{}\n",
            dir.path()
                .join("app.My%20PLC%3A1.download.iplcinfo")
                .display()
        ));
}

#[test]
fn record_download_path_when_record_written_then_is_the_file_record_download_wrote() {
    let (dir, application) = compiled_application();
    record_download(&application)
        .args(["--connection", "My PLC:1"])
        .assert()
        .success();

    let output = Command::new(cargo::cargo_bin!("ironplcc"))
        .arg("record-download-path")
        .arg(&application)
        .args(["--connection", "My PLC:1"])
        .output()
        .unwrap();

    let printed = PathBuf::from(String::from_utf8(output.stdout).unwrap().trim_end());
    assert!(printed.is_file());
    assert_eq!(printed.parent().unwrap(), dir.path());
}

#[test]
fn record_download_path_when_connection_empty_then_fails() {
    let dir = TempDir::new().unwrap();

    Command::new(cargo::cargo_bin!("ironplcc"))
        .arg("record-download-path")
        .arg(dir.path().join("app.iplc"))
        .args(["--connection", ""])
        .assert()
        .failure()
        .stdout(predicate::str::is_empty());
}
