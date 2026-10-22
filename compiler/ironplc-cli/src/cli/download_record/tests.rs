//! The download record commands as functions: the record is written beside the
//! application, and every failure is the error string with nothing written.

use std::fs;
use std::path::PathBuf;

use ironplc_container::FileHeader;
use ironplc_project::download_record::DeviceGenerations;
use ironplc_project::DeviceId;
use ironplc_project::RecordKind;
use tempfile::TempDir;

use super::{record_download, record_download_path, RecordDownload};

/// A directory with a file `app.iplc` that starts with a valid container header.
fn application() -> (TempDir, PathBuf) {
    let dir = TempDir::new().unwrap();
    let path = dir.path().join("app.iplc");
    let mut bytes = Vec::new();
    FileHeader {
        content_hash: [1; 32],
        ..FileHeader::default()
    }
    .write_to(&mut bytes)
    .unwrap();
    fs::write(&path, bytes).unwrap();
    (dir, path)
}

const DEVICE_TEXT: &str = "0123456789abcdef0123456789abcdef";

fn device() -> DeviceId {
    DeviceId::parse(DEVICE_TEXT).unwrap()
}

fn args<'a>(application: &'a PathBuf, device_id: &'a DeviceId) -> RecordDownload<'a> {
    RecordDownload {
        application,
        container: None,
        device_id,
        device_content_hash: None,
        device: None,
    }
}

#[test]
fn record_download_when_container_then_ok_and_record_beside_it() {
    let (dir, application) = application();

    record_download(&args(&application, &device())).unwrap();

    assert!(dir
        .path()
        .join(format!("app.{DEVICE_TEXT}.download.iplcinfo"))
        .is_file());
}

#[test]
fn record_download_when_generations_given_then_ok() {
    let (_dir, application) = application();
    let id = device();
    let mut request = args(&application, &id);
    request.device = Some(DeviceGenerations {
        active: 2,
        normal: 2,
        application: 2,
    });

    record_download(&request).unwrap();

    let path = RecordKind::Download { device_id: &id }
        .path(&application)
        .unwrap();
    assert!(fs::read_to_string(path).unwrap().contains("\"active\": 2"));
}

#[test]
fn record_download_when_container_missing_then_error_and_no_record() {
    let dir = TempDir::new().unwrap();
    let application = dir.path().join("app.iplc");

    let err = record_download(&args(&application, &device())).unwrap_err();

    assert!(err.contains("app.iplc"));
    assert_eq!(fs::read_dir(dir.path()).unwrap().count(), 0);
}

#[test]
fn record_download_path_when_device_then_path_of_the_record() {
    let (dir, application) = application();

    let path = record_download_path(&application, &device()).unwrap();

    assert_eq!(
        path,
        dir.path()
            .join(format!("app.{DEVICE_TEXT}.download.iplcinfo"))
    );
}

#[test]
fn record_download_path_when_anchor_has_no_file_name_then_error() {
    let root = PathBuf::from(std::path::MAIN_SEPARATOR_STR);

    let err = record_download_path(&root, &device()).unwrap_err();

    assert!(err.contains("needs a file name"), "{err}");
}
