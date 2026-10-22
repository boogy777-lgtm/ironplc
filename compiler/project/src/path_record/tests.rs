//! Tests for the path record: the writer, the reader, the loopback rule and the
//! format.

use std::fs;
use std::path::{Path, PathBuf};
use std::sync::LazyLock;

use ironplc_problems::Problem;
use rstest::rstest;
use serde_json::json;
use tempfile::TempDir;

use super::*;

const DEVICE_TEXT: &str = "0123456789abcdef0123456789abcdef";
const OTHER_DEVICE_TEXT: &str = "fedcba9876543210fedcba9876543210";
static DEVICE: LazyLock<DeviceId> = LazyLock::new(|| DeviceId::parse(DEVICE_TEXT).unwrap());
static OTHER_DEVICE: LazyLock<DeviceId> =
    LazyLock::new(|| DeviceId::parse(OTHER_DEVICE_TEXT).unwrap());
const PORT: u16 = 49152;

/// A directory with the application container name `app.iplc`; the container
/// itself is not needed by the path record.
fn application() -> (TempDir, PathBuf) {
    let dir = TempDir::new().unwrap();
    let anchor = dir.path().join("app.iplc");
    (dir, anchor)
}

fn choice<'a>(anchor: &'a Path, device_id: &'a DeviceId, address: &'a str) -> Choice<'a> {
    Choice {
        anchor,
        device_id,
        address,
        session_port: PORT,
    }
}

fn file_of(anchor: &Path) -> PathBuf {
    RecordKind::Path.path(anchor).unwrap()
}

fn read_present(anchor: &Path) -> PathRecord {
    let result = read(&file_of(anchor)).unwrap();
    let PathRecordRead::Present(record) = result else {
        panic!("expected a record, got {result:?}");
    };
    record
}

fn hint(address: &str, session_port: u16) -> Option<RouteHint> {
    Some(RouteHint {
        address: address.to_owned(),
        session_port,
    })
}

#[test]
fn record_choice_when_address_not_loopback_then_record_beside_container_with_hint() {
    let (dir, anchor) = application();

    record_choice(&choice(&anchor, &DEVICE, "192.168.1.20"), 1_761_148_800_000).unwrap();

    assert!(dir.path().join("app.path.iplcinfo").is_file());
    assert_eq!(
        read_present(&anchor),
        PathRecord {
            device_id: *DEVICE,
            route_hint: hint("192.168.1.20", PORT),
            chosen_at: 1_761_148_800_000,
        }
    );
}

#[test]
fn record_choice_then_bytes_are_the_documented_body() {
    let (_dir, anchor) = application();

    record_choice(&choice(&anchor, &DEVICE, "plc1.local"), 5).unwrap();

    let body: serde_json::Value =
        serde_json::from_str(&fs::read_to_string(file_of(&anchor)).unwrap()).unwrap();
    assert_eq!(
        body,
        json!({
            "version": 1,
            "deviceId": DEVICE_TEXT,
            "routeHint": {"address": "plc1.local", "sessionPort": PORT},
            "chosenAt": 5,
        })
    );
}

#[test]
fn record_choice_when_second_choice_then_replaces_the_record() {
    let (_dir, anchor) = application();
    record_choice(&choice(&anchor, &DEVICE, "10.0.0.1"), 1).unwrap();

    record_choice(&choice(&anchor, &OTHER_DEVICE, "10.0.0.2"), 2).unwrap();

    assert_eq!(
        read_present(&anchor),
        PathRecord {
            device_id: *OTHER_DEVICE,
            route_hint: hint("10.0.0.2", PORT),
            chosen_at: 2,
        }
    );
}

#[rstest]
fn record_choice_when_loopback_and_no_record_then_hint_is_null(
    #[values(
        "127.0.0.1",
        "127.255.0.9",
        "::1",
        "localhost",
        "LocalHost",
        "localhost."
    )]
    address: &str,
) {
    let (_dir, anchor) = application();

    record_choice(&choice(&anchor, &DEVICE, address), 7).unwrap();

    let record = read_present(&anchor);
    assert_eq!(record.device_id, *DEVICE);
    assert_eq!(record.route_hint, None);
    assert_eq!(record.chosen_at, 7);
}

#[test]
fn record_choice_when_loopback_then_previous_hint_of_the_same_device_is_kept() {
    let (_dir, anchor) = application();
    record_choice(&choice(&anchor, &DEVICE, "192.168.1.20"), 1).unwrap();

    record_choice(
        &Choice {
            session_port: 1234,
            ..choice(&anchor, &DEVICE, "127.0.0.1")
        },
        2,
    )
    .unwrap();

    assert_eq!(
        read_present(&anchor),
        PathRecord {
            device_id: *DEVICE,
            route_hint: hint("192.168.1.20", PORT),
            chosen_at: 2,
        }
    );
}

#[test]
fn record_choice_when_loopback_and_another_device_then_hint_is_null() {
    let (_dir, anchor) = application();
    record_choice(&choice(&anchor, &DEVICE, "192.168.1.20"), 1).unwrap();

    record_choice(&choice(&anchor, &OTHER_DEVICE, "::1"), 2).unwrap();

    assert_eq!(read_present(&anchor).route_hint, None);
}

#[test]
fn record_choice_when_loopback_and_previous_record_malformed_then_hint_is_null() {
    let (_dir, anchor) = application();
    fs::write(file_of(&anchor), "this is not json").unwrap();

    record_choice(&choice(&anchor, &DEVICE, "localhost"), 3).unwrap();

    assert_eq!(read_present(&anchor).route_hint, None);
}

#[rstest]
#[case::empty("")]
#[case::space("my plc")]
#[case::brackets("[::1]")]
#[case::with_port("10.0.0.1:49152")]
#[case::underscore("my_plc")]
#[case::leading_dash("-plc")]
#[case::empty_label("a..b")]
#[case::slash("10.0.0.1/8")]
fn record_choice_when_address_bad_then_refused_and_nothing_written(#[case] address: &str) {
    let (dir, anchor) = application();

    let err = record_choice(&choice(&anchor, &DEVICE, address), 1).unwrap_err();

    assert!(matches!(err, PathError::BadAddress(_)), "{err}");
    assert_eq!(fs::read_dir(dir.path()).unwrap().count(), 0);
}

#[test]
fn record_choice_when_port_zero_then_refused_and_nothing_written() {
    let (dir, anchor) = application();

    let err = record_choice(
        &Choice {
            session_port: 0,
            ..choice(&anchor, &DEVICE, "10.0.0.1")
        },
        1,
    )
    .unwrap_err();

    assert!(matches!(err, PathError::BadPort));
    assert_eq!(fs::read_dir(dir.path()).unwrap().count(), 0);
}

#[test]
fn record_choice_when_anchor_has_no_name_then_no_record_path() {
    let root = Path::new(std::path::MAIN_SEPARATOR_STR);

    let err = record_choice(&choice(root, &DEVICE, "10.0.0.1"), 1).unwrap_err();

    assert!(matches!(err, PathError::NoRecordPath(_)));
}

#[test]
fn record_choice_when_directory_unwritable_then_record_diagnostic() {
    let dir = TempDir::new().unwrap();
    // The record's own path is a directory, so it can be neither read as a
    // record nor written over.
    let anchor = dir.path().join("app.iplc");
    fs::create_dir(file_of(&anchor)).unwrap();

    let err = record_choice(&choice(&anchor, &DEVICE, "10.0.0.1"), 1).unwrap_err();

    let PathError::Record(problem) = err else {
        panic!("expected a record diagnostic");
    };
    assert_eq!(problem.code, Problem::CannotWriteFile.code());
}

#[rstest]
#[case::v4("127.0.0.1", true)]
#[case::v4_range("127.1.2.3", true)]
#[case::v6("::1", true)]
#[case::v6_long("0:0:0:0:0:0:0:1", true)]
#[case::v4_mapped_v6("::ffff:127.0.0.1", true)]
#[case::name("localhost", true)]
#[case::name_upper("LOCALHOST", true)]
#[case::name_root("localhost.", true)]
#[case::lan("192.168.1.20", false)]
#[case::wildcard("0.0.0.0", false)]
#[case::v4_next_to_range("128.0.0.1", false)]
#[case::v6_other("fe80::1", false)]
#[case::v4_mapped_other("::ffff:10.0.0.1", false)]
#[case::host("plc1.local", false)]
#[case::host_that_starts_with_localhost("localhost.example.com", false)]
#[case::host_that_ends_with_localhost("mylocalhost", false)]
fn is_loopback_when_address_then_this_pc_only_for_loopback(
    #[case] address: &str,
    #[case] expected: bool,
) {
    assert_eq!(is_loopback(address), expected);
}

#[test]
fn read_when_file_missing_then_missing() {
    let dir = TempDir::new().unwrap();

    let result = read(&dir.path().join("app.path.iplcinfo")).unwrap();

    assert_eq!(result, PathRecordRead::Missing);
}

#[rstest]
fn read_when_not_a_path_record_then_malformed(
    #[values(
        "this is not json",
        "[1]",
        r#"{"version": 2, "deviceId": "0123456789abcdef0123456789abcdef", "routeHint": null, "chosenAt": 1}"#,
        r#"{"deviceId": "0123456789abcdef0123456789abcdef", "routeHint": null, "chosenAt": 1}"#,
        r#"{"version": 1, "routeHint": null, "chosenAt": 1}"#,
        r#"{"version": 1, "deviceId": "ABCDEF", "routeHint": null, "chosenAt": 1}"#,
        r#"{"version": 1, "deviceId": 5, "routeHint": null, "chosenAt": 1}"#,
        r#"{"version": 1, "deviceId": "0123456789abcdef0123456789abcdef", "chosenAt": 1}"#,
        r#"{"version": 1, "deviceId": "0123456789abcdef0123456789abcdef", "routeHint": null}"#,
        r#"{"version": 1, "deviceId": "0123456789abcdef0123456789abcdef", "routeHint": null, "chosenAt": "now"}"#,
        r#"{"version": 1, "deviceId": "0123456789abcdef0123456789abcdef", "routeHint": null, "chosenAt": -1}"#,
        r#"{"version": 1, "deviceId": "0123456789abcdef0123456789abcdef", "routeHint": {"address": "10.0.0.1"}, "chosenAt": 1}"#,
        r#"{"version": 1, "deviceId": "0123456789abcdef0123456789abcdef", "routeHint": {"address": "10.0.0.1", "sessionPort": 70000}, "chosenAt": 1}"#,
        r#"{"version": 1, "deviceId": "0123456789abcdef0123456789abcdef", "routeHint": {"address": "10.0.0.1", "sessionPort": 0}, "chosenAt": 1}"#,
        r#"{"version": 1, "deviceId": "0123456789abcdef0123456789abcdef", "routeHint": {"address": "not an address", "sessionPort": 5}, "chosenAt": 1}"#,
        r#"{"version": 1, "deviceId": "0123456789abcdef0123456789abcdef", "routeHint": {"address": "127.0.0.1", "sessionPort": 5}, "chosenAt": 1}"#,
        r#"{"version": 1, "deviceId": "0123456789abcdef0123456789abcdef", "routeHint": "10.0.0.1", "chosenAt": 1}"#
    )]
    text: &str,
) {
    let dir = TempDir::new().unwrap();
    let path = dir.path().join("app.path.iplcinfo");
    fs::write(&path, text).unwrap();

    assert_eq!(read(&path).unwrap(), PathRecordRead::Malformed);
}

#[test]
fn read_when_hint_null_then_present_without_hint() {
    let dir = TempDir::new().unwrap();
    let path = dir.path().join("app.path.iplcinfo");
    fs::write(
        &path,
        format!(
            r#"{{"version": 1, "deviceId": "{DEVICE_TEXT}", "routeHint": null, "chosenAt": 9}}"#
        ),
    )
    .unwrap();

    assert_eq!(
        read(&path).unwrap(),
        PathRecordRead::Present(PathRecord {
            device_id: *DEVICE,
            route_hint: None,
            chosen_at: 9,
        })
    );
}

#[test]
fn read_when_path_is_directory_then_diagnostic() {
    let dir = TempDir::new().unwrap();

    let err = read(dir.path()).unwrap_err();

    assert_eq!(err.code, Problem::CannotReadFile.code());
}

#[test]
fn record_path_when_application_then_the_path_record_beside_it() {
    let path = record_path(&Path::new("some").join("dir").join("My.App.iplc")).unwrap();

    assert_eq!(
        path,
        Path::new("some").join("dir").join("My.App.path.iplcinfo")
    );
}

#[test]
fn record_path_when_root_then_error() {
    let root = Path::new(std::path::MAIN_SEPARATOR_STR);

    assert!(matches!(record_path(root), Err(PathError::NoRecordPath(_))));
}
