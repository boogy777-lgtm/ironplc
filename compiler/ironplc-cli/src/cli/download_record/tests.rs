//! The download record commands as functions: the record is written beside the
//! application, and every failure is the error string with nothing written.

use std::fs;
use std::path::PathBuf;

use ironplc_container::FileHeader;
use ironplc_project::download_record::DeviceGenerations;
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

fn args<'a>(application: &'a PathBuf, connection: &'a str) -> RecordDownload<'a> {
    RecordDownload {
        application,
        container: None,
        connection,
        device_content_hash: None,
        device: None,
    }
}

#[test]
fn record_download_when_container_then_ok_and_record_beside_it() {
    let (dir, application) = application();

    record_download(&args(&application, "My PLC")).unwrap();

    assert!(dir.path().join("app.My%20PLC.download.iplcinfo").is_file());
}

#[test]
fn record_download_when_generations_given_then_ok() {
    let (_dir, application) = application();
    let mut request = args(&application, "plc");
    request.device = Some(DeviceGenerations {
        active: 2,
        normal: 2,
        application: 2,
    });

    record_download(&request).unwrap();

    let path = RecordKind::Download { connection: "plc" }
        .path(&application)
        .unwrap();
    assert!(fs::read_to_string(path).unwrap().contains("\"active\": 2"));
}

#[test]
fn record_download_when_container_missing_then_error_and_no_record() {
    let dir = TempDir::new().unwrap();
    let application = dir.path().join("app.iplc");

    let err = record_download(&args(&application, "plc")).unwrap_err();

    assert!(err.contains("app.iplc"));
    assert_eq!(fs::read_dir(dir.path()).unwrap().count(), 0);
}

#[test]
fn record_download_when_connection_empty_then_error_and_no_record() {
    let (dir, application) = application();

    let err = record_download(&args(&application, "")).unwrap_err();

    assert!(err.contains("must not be empty"));
    assert_eq!(fs::read_dir(dir.path()).unwrap().count(), 1);
}

#[test]
fn record_download_path_when_connection_then_path_of_the_record() {
    let (dir, application) = application();

    let path = record_download_path(&application, "a.build").unwrap();

    assert_eq!(path, dir.path().join("app.a%2Ebuild.download.iplcinfo"));
}

#[test]
fn record_download_path_when_connection_empty_then_error() {
    let (_dir, application) = application();

    assert!(record_download_path(&application, "").is_err());
}
