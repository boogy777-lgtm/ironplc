//! Tests of the settings file: a new device, a restart, a damaged file and a
//! change, against real files in a temporary directory.

use std::fs;
use std::path::{Path, PathBuf};

use rstest::rstest;
use serde_json::json;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use super::device::{settings_path, Device};
use super::*;
use crate::slot_store::suffix;

/// A random source that fills the buffer with 1, 2, 3, ...
fn counting(buffer: &mut [u8]) -> Result<(), String> {
    for (byte, value) in buffer.iter_mut().zip(1u8..) {
        *byte = value;
    }
    Ok(())
}

/// The id [`counting`] generates.
const COUNTED_ID: &str = "0102030405060708090a0b0c0d0e0f10";

/// The served container of a temporary directory; the file itself does not
/// have to exist, the settings file sits beside its name.
fn served(dir: &TempDir) -> PathBuf {
    dir.path().join("app.iplc")
}

fn read_settings_file(served: &Path) -> serde_json::Value {
    serde_json::from_str(&fs::read_to_string(settings_path(served)).unwrap()).unwrap()
}

fn open_counting(served: &Path) -> Device {
    Device::open_with(served, &mut counting).unwrap()
}

fn name_entry(name: &str) -> Entries {
    Entries::from([("name".to_string(), json!(name))])
}

/// REQ-VC-vm-cli-025: a device without a settings file generates its id,
/// writes the file, and keeps the id when it starts again.
#[spec_test(REQ_VC_vm_cli_025)]
#[test]
fn open_when_settings_file_missing_then_generated_and_persisted_and_second_start_keeps_it() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);

    let first = Device::open(&served).unwrap();
    let written = read_settings_file(&served);
    let second = Device::open(&served).unwrap();

    let id = first.identity().device_id.unwrap();
    assert_eq!(id.len(), 32);
    assert_eq!(written["version"], 1);
    assert_eq!(written["deviceId"], id.as_str());
    assert_eq!(written["name"], "ironplcvm");
    assert_eq!(second.identity().device_id, Some(id));
    assert_eq!(read_settings_file(&served), written);
}

#[test]
fn open_when_two_fresh_devices_then_the_ids_differ() {
    let first_dir = TempDir::new().unwrap();
    let second_dir = TempDir::new().unwrap();

    let first = Device::open(&served(&first_dir)).unwrap();
    let second = Device::open(&served(&second_dir)).unwrap();

    assert_ne!(first.identity().device_id, second.identity().device_id);
}

#[test]
fn open_when_the_settings_file_is_deleted_then_the_device_is_a_new_one() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    let first = Device::open(&served).unwrap().identity().device_id;
    fs::remove_file(settings_path(&served)).unwrap();

    let second = Device::open(&served).unwrap().identity().device_id;

    assert_ne!(first, second);
}

#[test]
fn open_when_new_device_then_file_text_is_the_documented_shape() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);

    open_counting(&served);

    assert_eq!(
        fs::read_to_string(settings_path(&served)).unwrap(),
        format!("{{\n  \"version\": 1,\n  \"deviceId\": \"{COUNTED_ID}\",\n  \"name\": \"ironplcvm\"\n}}\n")
    );
}

#[test]
fn open_when_the_file_is_edited_offline_then_the_edited_values_are_the_device() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    open_counting(&served);
    fs::write(
        settings_path(&served),
        format!(
            "{{\"version\":1,\"deviceId\":\"{}\",\"name\":\"replacement\"}}",
            "a".repeat(32)
        ),
    )
    .unwrap();

    let identity = open_counting(&served).identity();

    assert_eq!(identity.device_id, Some("a".repeat(32)));
    assert_eq!(identity.name, "replacement");
}

#[test]
fn open_when_the_file_starts_with_a_byte_order_mark_then_it_is_read() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    let mut bytes = b"\xEF\xBB\xBF".to_vec();
    bytes.extend_from_slice(
        format!("{{\"version\":1,\"deviceId\":\"{COUNTED_ID}\",\"name\":\"x\"}}").as_bytes(),
    );
    fs::write(settings_path(&served), bytes).unwrap();

    let identity = open_counting(&served).identity();

    assert_eq!(identity.name, "x");
}

#[test]
fn open_when_the_file_lacks_a_setting_then_it_takes_the_first_value_and_the_file_is_completed() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    fs::write(
        settings_path(&served),
        format!("{{\"version\":1,\"deviceId\":\"{COUNTED_ID}\"}}"),
    )
    .unwrap();

    let identity = open_counting(&served).identity();

    assert_eq!(identity.device_id.as_deref(), Some(COUNTED_ID));
    assert_eq!(identity.name, "ironplcvm");
    assert_eq!(read_settings_file(&served)["name"], "ironplcvm");
}

/// REQ-VC-vm-cli-026: a settings file that cannot be used stops the start
/// with V6015, names the file and the field, and is left as it is.
#[spec_test(REQ_VC_vm_cli_026)]
#[rstest]
#[case::not_json("this is not json", "the content is not a JSON object")]
#[case::not_an_object("[1,2]", "the content is not a JSON object")]
#[case::empty_file("", "the content is not a JSON object")]
#[case::version_missing("{\"deviceId\":\"x\"}", "the field \"version\" is missing")]
#[case::other_version(
    "{\"version\":2,\"deviceId\":\"0102030405060708090a0b0c0d0e0f10\",\"name\":\"x\"}",
    "the field \"version\" must be 1"
)]
#[case::version_as_text(
    "{\"version\":\"1\",\"name\":\"x\"}",
    "the field \"version\" must be 1"
)]
#[case::bad_id(
    "{\"version\":1,\"deviceId\":\"0102\",\"name\":\"x\"}",
    "the field \"deviceId\" must be 32 lowercase hexadecimal digits"
)]
#[case::upper_case_id(
    "{\"version\":1,\"deviceId\":\"0102030405060708090A0B0C0D0E0F10\",\"name\":\"x\"}",
    "the field \"deviceId\" must be 32 lowercase hexadecimal digits"
)]
#[case::empty_name(
    "{\"version\":1,\"deviceId\":\"0102030405060708090a0b0c0d0e0f10\",\"name\":\"\"}",
    "the field \"name\" must not be empty"
)]
#[case::name_is_a_number("{\"version\":1,\"name\":7}", "the field \"name\" must be text")]
#[case::unknown_setting(
    "{\"version\":1,\"deviceID\":\"0102030405060708090a0b0c0d0e0f10\"}",
    "the field \"deviceID\" is not a device setting"
)]
fn open_when_the_file_is_not_valid_then_v6015_naming_the_file_and_the_field(
    #[case] content: &str,
    #[case] expected: &str,
) {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    fs::write(settings_path(&served), content).unwrap();

    let err = Device::open(&served).err().unwrap();

    assert_eq!(err.v_code(), "V6015");
    assert!(err.message().contains("app.iplc.device.json"), "{err}");
    assert!(err.message().contains(expected), "{err}");
    assert_eq!(fs::read_to_string(settings_path(&served)).unwrap(), content);
}

#[test]
fn open_when_the_settings_path_cannot_be_read_then_v6001() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    fs::create_dir(settings_path(&served)).unwrap();

    let err = Device::open(&served).err().unwrap();

    assert_eq!(err.v_code(), "V6001");
    assert!(err.message().contains("app.iplc.device.json"), "{err}");
}

#[test]
fn open_when_the_file_cannot_be_written_then_v6016_naming_the_file() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    // The temporary file's name is taken by a directory.
    fs::create_dir(suffix(&settings_path(&served), "tmp")).unwrap();

    let err = Device::open(&served).err().unwrap();

    assert_eq!(err.v_code(), "V6016");
    assert!(err.message().contains("app.iplc.device.json"), "{err}");
    assert!(!settings_path(&served).exists());
}

/// REQ-VC-vm-cli-027: a change that passes the checks is kept and written, and
/// the next start finds it.
#[spec_test(REQ_VC_vm_cli_027)]
#[test]
fn change_when_valid_then_the_device_and_the_file_hold_it_after_a_restart() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    let mut device = open_counting(&served);
    let entries = Entries::from([
        ("name".to_string(), json!("cell 1")),
        ("deviceId".to_string(), json!("b".repeat(32))),
    ]);

    device.change(&entries).unwrap();
    let restarted = open_counting(&served);

    assert_eq!(device.identity().name, "cell 1");
    assert_eq!(device.identity().device_id, Some("b".repeat(32)));
    assert_eq!(restarted.identity(), device.identity());
}

#[test]
fn change_when_twice_then_the_second_replaces_the_first() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    let mut device = open_counting(&served);

    device.change(&name_entry("one")).unwrap();
    device.change(&name_entry("two")).unwrap();

    assert_eq!(read_settings_file(&served)["name"], "two");
    assert!(!suffix(&settings_path(&served), "tmp").exists());
}

#[test]
fn change_when_the_values_are_the_current_ones_then_ok_and_nothing_is_written() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    let mut device = open_counting(&served);
    // A directory in the way of the temporary file would fail any write.
    fs::create_dir(suffix(&settings_path(&served), "tmp")).unwrap();

    let result = device.change(&name_entry("ironplcvm"));

    assert!(result.is_ok());
}

/// REQ-VC-vm-cli-028: a change that fails a check refuses with V6015 and
/// changes neither the file nor the running device.
#[spec_test(REQ_VC_vm_cli_028)]
#[test]
fn change_when_invalid_then_v6015_and_file_and_device_unchanged() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    let mut device = open_counting(&served);
    let before_file = fs::read(settings_path(&served)).unwrap();
    let before_identity = device.identity();
    let entries = Entries::from([
        ("name".to_string(), json!("cell 1")),
        ("deviceId".to_string(), json!("short")),
    ]);

    let err = device.change(&entries).unwrap_err();

    assert_eq!(err.v_code(), "V6015");
    assert!(err.message().contains("\"deviceId\""), "{err}");
    assert_eq!(fs::read(settings_path(&served)).unwrap(), before_file);
    assert_eq!(device.identity(), before_identity);
}

#[test]
fn change_when_the_file_cannot_be_written_then_v6016_and_file_and_device_unchanged() {
    let dir = TempDir::new().unwrap();
    let served = served(&dir);
    let mut device = open_counting(&served);
    let before_file = fs::read(settings_path(&served)).unwrap();
    let before_identity = device.identity();
    fs::create_dir(suffix(&settings_path(&served), "tmp")).unwrap();

    let err = device.change(&name_entry("cell 1")).unwrap_err();

    assert_eq!(err.v_code(), "V6016");
    assert_eq!(fs::read(settings_path(&served)).unwrap(), before_file);
    assert_eq!(device.identity(), before_identity);
}
