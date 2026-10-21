//! Tests for the download record: the writer, the reader and the format.

use std::fs;
use std::path::{Path, PathBuf};

use ironplc_container::FileHeader;
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::Problem;
use rstest::rstest;
use serde_json::{json, Value};
use tempfile::TempDir;

use super::*;
use crate::build_record::{BuildInputs, BuildRecord};
use crate::project::FileBackedProject;

const GENERATIONS: DeviceGenerations = DeviceGenerations {
    active: 3,
    normal: 2,
    application: 2,
};

/// Writes a file that is a container as far as the record is concerned: a valid
/// header with the given content hash, then a body.
fn write_container(path: &Path, content_hash: u8) {
    let header = FileHeader {
        content_hash: [content_hash; HASH_LEN],
        layout_hash: [2; HASH_LEN],
        debug_hash: [3; HASH_LEN],
        ..FileHeader::default()
    };
    let mut bytes = Vec::new();
    header.write_to(&mut bytes).unwrap();
    bytes.extend_from_slice(b"body");
    fs::write(path, bytes).unwrap();
}

/// A directory with the application container `app.iplc` whose content hash is
/// `[1; 32]`.
fn application() -> (TempDir, PathBuf) {
    let dir = TempDir::new().unwrap();
    let anchor = dir.path().join("app.iplc");
    write_container(&anchor, 1);
    (dir, anchor)
}

fn download<'a>(anchor: &'a Path, connection: &'a str) -> Download<'a> {
    Download {
        anchor,
        container: anchor,
        connection,
        device_content_hash: None,
        device: None,
    }
}

fn record_path(anchor: &Path, connection: &str) -> PathBuf {
    RecordKind::Download { connection }.path(anchor).unwrap()
}

fn read_present(anchor: &Path, connection: &str) -> DownloadRecord {
    let result = read(connection, &record_path(anchor, connection)).unwrap();
    let DownloadRecordRead::Present(record) = result else {
        panic!("expected a record, got {result:?}");
    };
    *record
}

#[test]
fn record_download_when_container_then_record_beside_it_with_its_identity() {
    let (dir, anchor) = application();

    record_download(&download(&anchor, "My PLC"), 0).unwrap();

    let path = dir.path().join("app.My%20PLC.download.iplcinfo");
    assert!(path.is_file());
    let record = read_present(&anchor, "My PLC");
    assert_eq!(record.connection, "My PLC");
    assert_eq!(record.application, "app");
    assert_eq!(
        record.container,
        ContainerIdentity::of_file(&anchor).unwrap()
    );
    assert_eq!(record.container.content_hash, [1; HASH_LEN]);
    assert_eq!(record.device, None);
    assert_eq!(record.time, "1970-01-01T00:00:00Z");
}

#[test]
fn record_download_when_generations_given_then_record_holds_them() {
    let (_dir, anchor) = application();
    let mut request = download(&anchor, "plc");
    request.device = Some(GENERATIONS);

    record_download(&request, 0).unwrap();

    assert_eq!(read_present(&anchor, "plc").device, Some(GENERATIONS));
}

#[test]
fn record_download_when_second_download_then_replaces_the_record() {
    let (_dir, anchor) = application();
    record_download(&download(&anchor, "plc"), 1_700_000_000).unwrap();
    write_container(&anchor, 9);

    record_download(&download(&anchor, "plc"), 1_700_000_060).unwrap();

    let record = read_present(&anchor, "plc");
    assert_eq!(record.container.content_hash, [9; HASH_LEN]);
    assert_eq!(record.time, "2023-11-14T22:14:20Z");
}

#[test]
fn record_download_when_two_connections_then_one_record_each() {
    let (_dir, anchor) = application();
    record_download(&download(&anchor, "first"), 0).unwrap();
    write_container(&anchor, 9);
    record_download(&download(&anchor, "second"), 0).unwrap();

    assert_eq!(
        read_present(&anchor, "first").container.content_hash,
        [1; HASH_LEN]
    );
    assert_eq!(
        read_present(&anchor, "second").container.content_hash,
        [9; HASH_LEN]
    );
}

#[test]
fn record_download_when_build_record_names_the_application_then_that_name() {
    let (_dir, anchor) = application();
    let project = FileBackedProject::new();
    let record = BuildRecord {
        application: "Plant".to_owned(),
        inputs: BuildInputs::of(&project, &CompilerOptions::default(), "plant", "1.0.0"),
        container: ContainerIdentity::of_file(&anchor).unwrap(),
    };
    crate::build_record::write(&RecordKind::Build.path(&anchor).unwrap(), &record).unwrap();

    record_download(&download(&anchor, "plc"), 0).unwrap();

    assert_eq!(read_present(&anchor, "plc").application, "Plant");
}

#[test]
fn record_download_when_build_record_malformed_then_stem_names_the_application() {
    let (_dir, anchor) = application();
    fs::write(RecordKind::Build.path(&anchor).unwrap(), "not json").unwrap();

    record_download(&download(&anchor, "plc"), 0).unwrap();

    assert_eq!(read_present(&anchor, "plc").application, "app");
}

#[test]
fn record_download_when_container_is_another_file_then_its_identity_beside_the_anchor() {
    let dir = TempDir::new().unwrap();
    let anchor = dir.path().join("app.iplc");
    let sent = dir.path().join("compiled-for-the-occasion.iplc");
    write_container(&sent, 7);
    let mut request = download(&anchor, "plc");
    request.container = &sent;

    record_download(&request, 0).unwrap();

    let record = read_present(&anchor, "plc");
    assert_eq!(record.container.content_hash, [7; HASH_LEN]);
    assert_eq!(record.application, "app");
    assert!(!record_path(&sent, "plc").exists());
}

#[test]
fn record_download_when_device_hash_is_that_of_the_container_then_written() {
    let (_dir, anchor) = application();
    let mut request = download(&anchor, "plc");
    request.device_content_hash = Some([1; HASH_LEN]);

    record_download(&request, 0).unwrap();

    assert!(record_path(&anchor, "plc").is_file());
}

#[test]
fn record_download_when_device_runs_another_container_then_error_and_no_record() {
    let (_dir, anchor) = application();
    let mut request = download(&anchor, "plc");
    request.device_content_hash = Some([5; HASH_LEN]);

    let err = record_download(&request, 0).unwrap_err();

    assert!(matches!(err, DownloadError::DeviceRunsOther { .. }));
    assert!(err.to_string().contains(&"05".repeat(32)));
    assert!(err.to_string().contains(&"01".repeat(32)));
    assert!(!record_path(&anchor, "plc").exists());
}

#[test]
fn record_download_when_connection_empty_then_error_and_no_file() {
    let (dir, anchor) = application();

    let err = record_download(&download(&anchor, ""), 0).unwrap_err();

    assert!(matches!(err, DownloadError::NoRecordPath { .. }));
    assert_eq!(fs::read_dir(dir.path()).unwrap().count(), 1);
}

#[test]
fn record_download_when_container_missing_then_not_a_container() {
    let dir = TempDir::new().unwrap();
    let anchor = dir.path().join("app.iplc");

    let err = record_download(&download(&anchor, "plc"), 0).unwrap_err();

    assert!(matches!(err, DownloadError::NotAContainer(_)));
    assert!(!record_path(&anchor, "plc").exists());
}

#[test]
fn record_download_when_file_is_not_a_container_then_not_a_container() {
    let dir = TempDir::new().unwrap();
    let anchor = dir.path().join("app.iplc");
    fs::write(&anchor, "this is not a container").unwrap();

    let err = record_download(&download(&anchor, "plc"), 0).unwrap_err();

    assert!(matches!(err, DownloadError::NotAContainer(_)));
}

#[test]
fn record_download_when_record_cannot_be_written_then_record_error() {
    let (_dir, anchor) = application();
    // A directory where the record would be.
    fs::create_dir(record_path(&anchor, "plc")).unwrap();

    let err = record_download(&download(&anchor, "plc"), 0).unwrap_err();

    assert!(
        matches!(&err, DownloadError::Record(problem) if problem.code == Problem::CannotWriteFile.code())
    );
}

/// Reads the record of `plc` after its JSON was edited by `edit`.
fn read_edited(edit: fn(&mut Value)) -> DownloadRecordRead {
    let (_dir, anchor) = application();
    record_download(&download(&anchor, "plc"), 0).unwrap();
    let path = record_path(&anchor, "plc");
    let mut value: Value = serde_json::from_str(&fs::read_to_string(&path).unwrap()).unwrap();
    edit(&mut value);
    fs::write(&path, value.to_string()).unwrap();
    read("plc", &path).unwrap()
}

#[test]
fn read_when_file_missing_then_missing() {
    let (_dir, anchor) = application();

    let result = read("plc", &record_path(&anchor, "plc")).unwrap();

    assert_eq!(result, DownloadRecordRead::Missing);
}

#[rstest]
#[case::other_version(|v: &mut Value| v["version"] = json!(2))]
#[case::no_version(|v: &mut Value| { v.as_object_mut().unwrap().remove("version"); })]
#[case::other_connection(|v: &mut Value| v["connection"] = json!("PLC"))]
#[case::no_container(|v: &mut Value| { v.as_object_mut().unwrap().remove("container"); })]
#[case::bad_hash(|v: &mut Value| v["container"]["contentHash"] = json!("abc"))]
#[case::no_time(|v: &mut Value| { v.as_object_mut().unwrap().remove("time"); })]
#[case::no_device_key(|v: &mut Value| { v.as_object_mut().unwrap().remove("device"); })]
#[case::partial_device(|v: &mut Value| v["device"] = json!({"active": 1}))]
#[case::negative_counter(|v: &mut Value| v["device"] = json!({"active": -1, "normal": 1, "application": 1}))]
fn read_when_shape_is_not_the_records_then_malformed(#[case] edit: fn(&mut Value)) {
    assert_eq!(read_edited(edit), DownloadRecordRead::Malformed);
}

#[test]
fn read_when_file_is_not_json_then_malformed() {
    let (_dir, anchor) = application();
    let path = record_path(&anchor, "plc");
    fs::write(&path, "{ not json").unwrap();

    assert_eq!(read("plc", &path).unwrap(), DownloadRecordRead::Malformed);
}

#[test]
fn read_when_unknown_extra_key_then_present() {
    let result = read_edited(|v| v["fieldOfALaterWriter"] = json!(true));

    assert!(matches!(result, DownloadRecordRead::Present(_)));
}

#[test]
fn write_when_record_then_json_keys_are_those_the_editor_reads() {
    let (_dir, anchor) = application();
    let mut request = download(&anchor, "plc");
    request.device = Some(GENERATIONS);
    record_download(&request, 1_700_000_000).unwrap();

    let text = fs::read_to_string(record_path(&anchor, "plc")).unwrap();
    let value: Value = serde_json::from_str(&text).unwrap();

    assert_eq!(value["version"], 1);
    assert_eq!(value["connection"], "plc");
    assert_eq!(value["application"], "app");
    assert_eq!(value["container"]["contentHash"], "01".repeat(32));
    assert_eq!(value["container"]["layoutHash"], "02".repeat(32));
    assert_eq!(value["container"]["debugHash"], "03".repeat(32));
    assert!(value["container"]["size"].as_u64().unwrap() > 0);
    assert_eq!(
        value["device"],
        json!({"active": 3, "normal": 2, "application": 2})
    );
    assert_eq!(value["time"], "2023-11-14T22:13:20Z");
}

#[rstest]
#[case::epoch(0, "1970-01-01T00:00:00Z")]
#[case::last_second_of_a_day(86_399, "1970-01-01T23:59:59Z")]
#[case::leap_day(951_782_400, "2000-02-29T00:00:00Z")]
#[case::after_leap_day(951_868_800, "2000-03-01T00:00:00Z")]
#[case::known_time(1_700_000_000, "2023-11-14T22:13:20Z")]
#[case::end_of_a_year(1_735_689_599, "2024-12-31T23:59:59Z")]
#[case::century_without_leap_day(4_107_542_400, "2100-03-01T00:00:00Z")]
fn rfc3339_utc_when_seconds_then_date_and_time(#[case] seconds: u64, #[case] expected: &str) {
    assert_eq!(rfc3339_utc(seconds), expected);
}
