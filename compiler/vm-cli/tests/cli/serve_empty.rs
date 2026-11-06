//! Tests of `ironplcvm serve` without a file: the empty application, where the
//! device keeps its files, and the container format its identity reports.

use std::path::Path;

use assert_cmd::cargo;
use ironplc_container::integrity::hash_hex;
use ironplc_container::{EMPTY_APPLICATION_HASH, FORMAT_VERSION};
use spec_test_macro::spec_test;
use tempfile::TempDir;

use crate::{compiled_bytes, COUNTER_PROGRAM};

const IDENTITY: &str = "{\"command\":\"identity\"}\n";

/// One `ironplcvm serve` process without a file, started in `dir`, fed
/// `stdin`: its exit status and the response lines it wrote.
fn serve_here(dir: &Path, stdin: &str) -> (std::process::Output, Vec<serde_json::Value>) {
    let output = assert_cmd::Command::new(cargo::cargo_bin!("ironplcvm"))
        .current_dir(dir)
        .arg("serve")
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

/// REQ-VC-vm-cli-058: without a file the device boots the empty application,
/// reports its constant hash, and keeps its files in the current directory.
#[spec_test(REQ_VC_vm_cli_058)]
#[test]
fn serve_when_no_file_and_no_store_then_the_empty_application_boots_and_its_files_are_here() {
    let dir = TempDir::new().unwrap();

    let (output, lines) = serve_here(dir.path(), IDENTITY);

    assert!(output.status.success());
    assert_eq!(
        lines[0]["application"]["contentHash"],
        hash_hex(&EMPTY_APPLICATION_HASH).to_string()
    );
    assert_eq!(lines[0]["application"]["mode"], "normal");
    assert_eq!(lines[0]["application"]["rounds"], 0);
    assert_eq!(
        lines[0]["persistedContentHash"],
        hash_hex(&EMPTY_APPLICATION_HASH).to_string()
    );
    for name in [
        "ironplcvm.slot-a",
        "ironplcvm.marker",
        "ironplcvm.device.json",
    ] {
        assert!(dir.path().join(name).exists(), "{name} is missing");
    }
}

/// The device is the same device on the next start in the same directory, and
/// still holds no application.
#[spec_test(REQ_VC_vm_cli_058)]
#[test]
fn serve_when_no_file_started_twice_then_the_same_device_holds_the_empty_application() {
    let dir = TempDir::new().unwrap();

    let (_, first) = serve_here(dir.path(), IDENTITY);
    let (_, second) = serve_here(dir.path(), IDENTITY);

    assert_eq!(
        first[0]["device"]["deviceId"],
        second[0]["device"]["deviceId"]
    );
    assert_eq!(
        second[0]["application"]["contentHash"],
        hash_hex(&EMPTY_APPLICATION_HASH).to_string()
    );
}

/// A store in the current directory is the application the device boots.
#[spec_test(REQ_VC_vm_cli_058)]
#[test]
fn serve_when_no_file_and_a_store_exists_then_the_committed_application_boots() {
    let dir = TempDir::new().unwrap();
    std::fs::write(
        dir.path().join("ironplcvm.slot-a"),
        compiled_bytes(COUNTER_PROGRAM),
    )
    .unwrap();
    std::fs::write(
        dir.path().join("ironplcvm.marker"),
        "{\"slot\":\"a\",\"seq\":1}",
    )
    .unwrap();

    let (output, lines) = serve_here(dir.path(), IDENTITY);

    assert!(output.status.success());
    let hash = lines[0]["application"]["contentHash"].as_str().unwrap();
    assert_ne!(hash, hash_hex(&EMPTY_APPLICATION_HASH).to_string());
}

/// REQ-VC-vm-cli-059: the identity carries the container format version.
#[spec_test(REQ_VC_vm_cli_059)]
#[test]
fn serve_when_identity_then_container_format_is_the_format_version() {
    let dir = TempDir::new().unwrap();

    let (_, lines) = serve_here(dir.path(), IDENTITY);

    assert_eq!(lines[0]["containerFormat"], u64::from(FORMAT_VERSION));
    assert_eq!(lines[0]["protocol"], 2);
}

/// The reference page states the hash of the empty application for the format
/// version of the build, so a change of the format that regenerates the
/// constant cannot leave the page behind.
#[test]
fn reference_page_when_format_version_changes_then_it_lists_the_empty_application_hash() {
    let page = std::fs::read_to_string(
        Path::new(env!("CARGO_MANIFEST_DIR")).join("../../docs/reference/runtime/ironplcvm.rst"),
    )
    .unwrap();

    assert!(page.contains(&hash_hex(&EMPTY_APPLICATION_HASH).to_string()));
    assert!(page.contains(&format!("   * - {FORMAT_VERSION}\n")));
}
