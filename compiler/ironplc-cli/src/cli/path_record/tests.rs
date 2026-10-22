//! The path record commands as functions: the record is written beside the
//! application, and every failure is the error string with nothing written.

use std::fs;

use tempfile::TempDir;

use ironplc_project::DeviceId;

use super::{record_path, record_path_path, RecordPath};

const DEVICE_TEXT: &str = "0123456789abcdef0123456789abcdef";

fn device() -> DeviceId {
    DeviceId::parse(DEVICE_TEXT).unwrap()
}

#[test]
fn record_path_when_choice_then_ok_and_record_beside_application() {
    let dir = TempDir::new().unwrap();
    let application = dir.path().join("app.iplc");

    record_path(&RecordPath {
        application: &application,
        device_id: &device(),
        address: "192.168.1.20",
        session_port: 49152,
    })
    .unwrap();

    let text = fs::read_to_string(dir.path().join("app.path.iplcinfo")).unwrap();
    assert!(text.contains(DEVICE_TEXT));
    assert!(text.contains("192.168.1.20"));
}

#[test]
fn record_path_path_when_application_then_the_record_name() {
    let dir = TempDir::new().unwrap();

    let path = record_path_path(&dir.path().join("app.iplc")).unwrap();

    assert_eq!(path, dir.path().join("app.path.iplcinfo"));
}
