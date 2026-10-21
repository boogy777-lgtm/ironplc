//! Tests of the table of settings: every row is exercised through the same
//! three doors that a setting has, the file reader, the online command and the
//! identity, so a row that is added without a sample fails the build. The
//! loops over `DeviceSetting::ALL` are deliberate: each test states a rule of
//! every row, and a row added later is covered without an edit here.

use rstest::rstest;
use serde_json::json;
use spec_test_macro::spec_test;

use super::*;

/// A random source that fills the buffer with 1, 2, 3, ...
fn counting(buffer: &mut [u8]) -> Result<(), String> {
    for (byte, value) in buffer.iter_mut().zip(1u8..) {
        *byte = value;
    }
    Ok(())
}

/// What the tests know about one row: a value it accepts that is not its first
/// value, and a value it refuses. The match has no wildcard arm, so a new row
/// does not compile until it has its samples.
fn sample(setting: DeviceSetting) -> (&'static str, &'static str) {
    match setting {
        DeviceSetting::DeviceId => (
            "ffffffffffffffffffffffffffffffff",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
        ),
        DeviceSetting::Name => ("cell 1 \u{00e4}", "   "),
    }
}

fn entry(setting: DeviceSetting, value: &str) -> Entries {
    Entries::from([(setting.key().to_string(), json!(value))])
}

fn first_settings() -> DeviceSettings {
    DeviceSettings::resolve(&Entries::new(), &mut counting).unwrap()
}

/// The rows the `setDeviceSettings` command may change.
fn online_rows() -> impl Iterator<Item = &'static DeviceSetting> {
    DeviceSetting::ALL
        .iter()
        .filter(|setting| setting.changeable() == Changeable::OnlineAndFile)
}

/// The rows that are changed in the settings file only.
fn file_only_rows() -> impl Iterator<Item = &'static DeviceSetting> {
    DeviceSetting::ALL
        .iter()
        .filter(|setting| setting.changeable() == Changeable::FileOnly)
}

#[test]
fn all_when_listed_then_keys_are_unique_and_follow_the_discriminants() {
    let mut keys: Vec<&str> = DeviceSetting::ALL.iter().map(|s| s.key()).collect();
    keys.sort_unstable();
    keys.dedup();

    assert_eq!(keys.len(), DeviceSetting::ALL.len());
    for (index, setting) in DeviceSetting::ALL.iter().enumerate() {
        assert_eq!(*setting as usize, index);
    }
}

#[test]
fn resolve_when_no_entries_then_each_row_takes_its_first_value() {
    let settings = first_settings();

    assert_eq!(
        settings.get(DeviceSetting::DeviceId),
        "0102030405060708090a0b0c0d0e0f10"
    );
    assert_eq!(settings.get(DeviceSetting::Name), "ironplcvm");
}

#[test]
fn resolve_when_os_random_twice_then_the_ids_differ_and_pass_the_check() {
    let first = DeviceSettings::resolve(&Entries::new(), &mut os_random).unwrap();
    let second = DeviceSettings::resolve(&Entries::new(), &mut os_random).unwrap();

    assert_ne!(
        first.get(DeviceSetting::DeviceId),
        second.get(DeviceSetting::DeviceId)
    );
    assert!(check_device_id(first.get(DeviceSetting::DeviceId)).is_ok());
}

#[test]
fn resolve_when_the_random_source_fails_then_error_naming_the_setting() {
    let mut failing = |_: &mut [u8]| Err("no entropy".to_string());

    let err = DeviceSettings::resolve(&Entries::new(), &mut failing).unwrap_err();

    assert_eq!(
        err.to_string(),
        "the field \"deviceId\" could not be generated, the random source failed: no entropy"
    );
}

#[test]
fn every_row_when_first_value_then_passes_its_own_check() {
    let settings = first_settings();

    for setting in DeviceSetting::ALL {
        assert!(
            check_entries(&entry(*setting, settings.get(*setting))).is_ok(),
            "{}",
            setting.key()
        );
    }
}

#[test]
fn resolve_when_a_row_has_a_valid_value_then_the_value_is_kept() {
    for setting in DeviceSetting::ALL {
        let (valid, _) = sample(*setting);

        let settings = DeviceSettings::resolve(&entry(*setting, valid), &mut counting).unwrap();

        assert_eq!(settings.get(*setting), valid);
    }
}

#[test]
fn resolve_when_a_row_has_an_invalid_value_then_error_naming_the_key() {
    for setting in DeviceSetting::ALL {
        let (_, invalid) = sample(*setting);

        let err = DeviceSettings::resolve(&entry(*setting, invalid), &mut counting).unwrap_err();

        assert!(
            err.to_string()
                .starts_with(&format!("the field \"{}\" ", setting.key())),
            "{err}"
        );
    }
}

#[test]
fn apply_online_when_an_online_row_has_a_valid_value_then_only_that_row_changes() {
    for setting in online_rows() {
        let (valid, _) = sample(*setting);
        let mut settings = first_settings();
        let before = settings.clone();

        settings.apply_online(&entry(*setting, valid)).unwrap();

        assert_eq!(settings.get(*setting), valid);
        for other in DeviceSetting::ALL.iter().filter(|other| *other != setting) {
            assert_eq!(settings.get(*other), before.get(*other));
        }
    }
}

#[test]
fn apply_online_when_an_online_row_has_an_invalid_value_then_error_and_settings_unchanged() {
    for setting in online_rows() {
        let (_, invalid) = sample(*setting);
        let mut settings = first_settings();
        let before = settings.clone();

        let err = settings
            .apply_online(&entry(*setting, invalid))
            .unwrap_err();

        assert!(err.to_string().contains(setting.key()), "{err}");
        assert_eq!(settings, before);
    }
}

/// REQ-VC-vm-cli-030: a row that is changed in the file only is refused by the
/// online door, whatever the value, and the settings stay as they were.
#[spec_test(REQ_VC_vm_cli_030)]
#[test]
fn apply_online_when_a_file_only_row_is_named_then_refused_and_settings_unchanged() {
    for setting in file_only_rows() {
        let (valid, invalid) = sample(*setting);
        for value in [valid, invalid] {
            let mut settings = first_settings();
            let before = settings.clone();

            let err = settings.apply_online(&entry(*setting, value)).unwrap_err();

            assert_eq!(
                err.to_string(),
                format!(
                    "the field \"{}\" can only be changed on the device itself (its settings file)",
                    setting.key()
                )
            );
            assert_eq!(settings, before);
        }
    }
}

/// REQ-VC-vm-cli-030: the file door still takes the file-only rows.
#[spec_test(REQ_VC_vm_cli_030)]
#[test]
fn resolve_when_a_file_only_row_has_a_valid_value_then_the_value_is_kept() {
    for setting in file_only_rows() {
        let (valid, _) = sample(*setting);

        let settings = DeviceSettings::resolve(&entry(*setting, valid), &mut counting).unwrap();

        assert_eq!(settings.get(*setting), valid);
    }
}

#[test]
fn apply_online_when_a_file_only_row_is_named_with_an_online_row_then_neither_is_applied() {
    for file_only in file_only_rows() {
        for online in online_rows() {
            let mut settings = first_settings();
            let before = settings.clone();
            let mut entries = entry(*online, sample(*online).0);
            entries.extend(entry(*file_only, sample(*file_only).0));

            let err = settings.apply_online(&entries).unwrap_err();

            assert!(err.to_string().contains(file_only.key()), "{err}");
            assert_eq!(settings, before);
        }
    }
}

#[test]
fn apply_online_when_one_entry_is_invalid_then_no_entry_is_applied() {
    let mut settings = first_settings();
    let before = settings.clone();
    let entries = Entries::from([
        ("name".to_string(), json!("cell 1")),
        ("nmae".to_string(), json!("cell 2")),
    ]);

    let err = settings.apply_online(&entries).unwrap_err();

    assert!(err.to_string().starts_with("the field \"nmae\""));
    assert_eq!(settings, before);
}

#[rstest]
#[case::unknown_key("nmae", json!("x"), "the field \"nmae\" is not a device setting")]
#[case::number("name", json!(5), "the field \"name\" must be text")]
#[case::null("name", json!(null), "the field \"name\" must be text")]
#[case::empty_name("name", json!(""), "the field \"name\" must not be empty")]
#[case::long_name(
    "name",
    json!("n".repeat(65)),
    "the field \"name\" must have at most 64 characters"
)]
#[case::control_character(
    "name",
    json!("a\nb"),
    "the field \"name\" must not contain control characters"
)]
#[case::file_only_id(
    "deviceId",
    json!("ffffffffffffffffffffffffffffffff"),
    "the field \"deviceId\" can only be changed on the device itself (its settings file)"
)]
fn apply_online_when_entry_is_refused_then_message_says_why(
    #[case] key: &str,
    #[case] value: serde_json::Value,
    #[case] expected: &str,
) {
    let mut settings = first_settings();

    let err = settings
        .apply_online(&Entries::from([(key.to_string(), value)]))
        .unwrap_err();

    assert_eq!(err.to_string(), expected);
}

#[test]
fn name_when_64_characters_of_two_bytes_then_accepted() {
    let name = "\u{00e4}".repeat(64);

    assert!(check_name(&name).is_ok());
}

#[test]
fn entries_when_settings_then_one_text_entry_for_each_row() {
    let entries = first_settings().entries();

    assert_eq!(entries.len(), DeviceSetting::ALL.len());
    assert_eq!(entries["name"], json!("ironplcvm"));
    assert_eq!(
        entries["deviceId"],
        json!("0102030405060708090a0b0c0d0e0f10")
    );
}

#[test]
fn identity_when_settings_then_carries_the_id_the_name_and_the_build_constants() {
    let mut settings = first_settings();
    settings
        .apply_online(&entry(DeviceSetting::Name, "cell 1"))
        .unwrap();

    let identity = settings.identity();

    assert_eq!(
        identity.device_id.as_deref(),
        Some("0102030405060708090a0b0c0d0e0f10")
    );
    assert_eq!(identity.name, "cell 1");
    assert_eq!(identity.model, "IronPLC SoftPLC");
    assert_eq!(identity.modification, "vm-cli");
    assert_eq!(identity.firmware_version, env!("CARGO_PKG_VERSION"));
}
