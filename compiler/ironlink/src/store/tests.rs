//! Tests of the store: its location, the files and their errors. Every test works
//! in a directory of its own under the system's temporary directory; none reads
//! or writes the directory of the user.

use std::fs;
use std::net::{IpAddr, SocketAddr};
use std::path::{Path, PathBuf};

use rstest::rstest;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use super::addresses::AddressesFile;
use super::routes::RouteRecord;
use super::*;
use crate::driver::DriverKind;
use crate::test_support::temp_store;

const ID_A: &str = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
const ID_B: &str = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

fn dir_of(store: &Store) -> PathBuf {
    store.dir().unwrap().to_path_buf()
}

fn route(address: &str, session_port: u16, verified_at: u64) -> RouteRecord {
    RouteRecord {
        address: address.parse::<IpAddr>().unwrap(),
        driver: DriverKind::AddressList,
        session_port,
        verified_at,
    }
}

fn socket(text: &str) -> SocketAddr {
    text.parse().unwrap()
}

fn env<'a>(pairs: &'a [(&'a str, &'a str)]) -> impl Fn(&str) -> Option<String> + 'a {
    move |key| {
        pairs
            .iter()
            .find(|(name, _)| *name == key)
            .map(|(_, value)| (*value).to_string())
    }
}

fn names_in(dir: &Path) -> Vec<String> {
    let mut names: Vec<String> = fs::read_dir(dir)
        .unwrap()
        .map(|entry| entry.unwrap().file_name().to_string_lossy().into_owned())
        .collect();
    names.sort();
    names
}

#[spec_test(REQ_IL_ironlink_060)]
#[rstest]
#[case(
    Platform::Windows,
    &[("APPDATA", "C:\\Users\\Ann\\AppData\\Roaming"), ("HOME", "/ignored")],
    Some(PathBuf::from("C:\\Users\\Ann\\AppData\\Roaming").join("IronPLC").join("ironlink"))
)]
#[case(
    Platform::MacOs,
    &[("HOME", "/Users/ann"), ("XDG_CONFIG_HOME", "/ignored")],
    Some(PathBuf::from("/Users/ann/Library/Application Support/IronPLC/ironlink"))
)]
#[case(
    Platform::Other,
    &[("XDG_CONFIG_HOME", "/home/ann/cfg"), ("HOME", "/home/ann")],
    Some(PathBuf::from("/home/ann/cfg/ironplc/ironlink"))
)]
#[case(
    Platform::Other,
    &[("HOME", "/home/ann")],
    Some(PathBuf::from("/home/ann/.config/ironplc/ironlink"))
)]
#[case(
    Platform::Other,
    &[("XDG_CONFIG_HOME", ""), ("HOME", "/home/ann")],
    Some(PathBuf::from("/home/ann/.config/ironplc/ironlink"))
)]
#[case(Platform::Windows, &[("HOME", "/home/ann")], None)]
#[case(Platform::MacOs, &[], None)]
#[case(Platform::Other, &[("APPDATA", "x")], None)]
fn default_dir_when_platform_and_environment_given_then_the_directory_of_the_user(
    #[case] platform: Platform,
    #[case] variables: &[(&str, &str)],
    #[case] expected: Option<PathBuf>,
) {
    assert_eq!(default_dir(platform, env(variables)), expected);
}

#[spec_test(REQ_IL_ironlink_061)]
#[test]
fn resolve_when_option_and_variable_and_default_then_the_option_wins_then_the_variable() {
    let variables = [
        (STORE_ENV, "/from/variable"),
        ("XDG_CONFIG_HOME", "/home/ann/cfg"),
    ];

    let by_option = Store::resolve(
        Some(Path::new("/from/option")),
        Platform::Other,
        env(&variables),
    );
    let by_variable = Store::resolve(None, Platform::Other, env(&variables));
    let by_default = Store::resolve(None, Platform::Other, env(&variables[1..]));

    assert_eq!(by_option, Store::at("/from/option"));
    assert_eq!(by_variable, Store::at("/from/variable"));
    assert_eq!(by_default, Store::at("/home/ann/cfg/ironplc/ironlink"));
}

#[spec_test(REQ_IL_ironlink_061)]
#[test]
fn resolve_when_the_variable_is_empty_then_it_is_not_set() {
    let variables = [(STORE_ENV, ""), ("HOME", "/home/ann")];

    let store = Store::resolve(None, Platform::Other, env(&variables));

    assert_eq!(store, Store::at("/home/ann/.config/ironplc/ironlink"));
}

#[test]
fn resolve_when_nothing_names_a_directory_then_the_commands_that_need_it_fail_with_l6004() {
    let store = Store::resolve(None, Platform::Other, env(&[]));

    assert_eq!(store.routes().unwrap_err().l_code(), "L6004");
    assert_eq!(
        store
            .add_address(socket("10.0.0.1:1"))
            .unwrap_err()
            .l_code(),
        "L6004"
    );
}

#[spec_test(REQ_IL_ironlink_063)]
#[test]
fn read_when_the_files_and_the_directory_do_not_exist_then_empty() {
    let (_guard, store) = temp_store();

    assert!(store.routes().unwrap().is_empty());
    assert!(store.addresses().unwrap().is_empty());
    assert!(!dir_of(&store).exists());
}

#[spec_test(REQ_IL_ironlink_062)]
#[test]
fn write_when_first_write_then_the_directory_is_created_and_only_the_file_is_in_it() {
    let (_guard, store) = temp_store();

    store.add_address(socket("10.0.0.1:49152")).unwrap();

    assert_eq!(names_in(&dir_of(&store)), ["addresses.json"]);
}

#[spec_test(REQ_IL_ironlink_062)]
#[test]
fn write_when_replacing_a_file_then_no_temporary_file_is_left() {
    let (_guard, store) = temp_store();
    store.set_route(ID_A, route("10.0.0.1", 49152, 1)).unwrap();

    store.set_route(ID_A, route("10.0.0.2", 49152, 2)).unwrap();
    store.set_route(ID_B, route("10.0.0.3", 49152, 3)).unwrap();

    assert_eq!(names_in(&dir_of(&store)), ["routes.json"]);
}

#[spec_test(REQ_IL_ironlink_062)]
#[test]
fn write_when_routes_stored_then_versioned_pretty_json_with_sorted_keys() {
    let (_guard, store) = temp_store();
    store.set_route(ID_B, route("10.0.0.2", 7000, 20)).unwrap();
    store.set_route(ID_A, route("10.0.0.1", 49152, 10)).unwrap();

    let text = fs::read_to_string(dir_of(&store).join("routes.json")).unwrap();

    let expected = r#"{
  "routes": {
    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa": {
      "address": "10.0.0.1",
      "driver": "addressList",
      "sessionPort": 49152,
      "verifiedAt": 10
    },
    "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb": {
      "address": "10.0.0.2",
      "driver": "addressList",
      "sessionPort": 7000,
      "verifiedAt": 20
    }
  },
  "version": 1
}
"#;
    assert_eq!(text, expected);
}

#[spec_test(REQ_IL_ironlink_062)]
#[test]
fn write_when_addresses_stored_then_versioned_pretty_json() {
    let (_guard, store) = temp_store();
    store.add_address(socket("10.0.0.2:7000")).unwrap();

    let text = fs::read_to_string(dir_of(&store).join("addresses.json")).unwrap();

    assert_eq!(
        text,
        "{\n  \"addresses\": [\n    \"10.0.0.2:7000\"\n  ],\n  \"version\": 1\n}\n"
    );
}

#[spec_test(REQ_IL_ironlink_066)]
#[test]
fn routes_when_routes_stored_then_read_back_in_the_order_of_the_device_ids() {
    let (_guard, store) = temp_store();
    store.set_route(ID_B, route("10.0.0.2", 7000, 20)).unwrap();
    store.set_route(ID_A, route("fe80::1", 49152, 10)).unwrap();

    let entries = store.routes().unwrap();

    assert_eq!(entries.len(), 2);
    assert_eq!(entries[0].device_id, ID_A);
    assert_eq!(entries[0].route, route("fe80::1", 49152, 10));
    assert_eq!(entries[1].device_id, ID_B);
    assert_eq!(entries[1].route, route("10.0.0.2", 7000, 20));
}

#[spec_test(REQ_IL_ironlink_066)]
#[test]
fn set_route_when_the_device_has_a_route_then_the_latest_replaces_it_and_the_old_one_is_returned() {
    let (_guard, store) = temp_store();
    let first = store.set_route(ID_A, route("10.0.0.1", 49152, 1)).unwrap();

    let second = store.set_route(ID_A, route("10.0.0.9", 49153, 2)).unwrap();

    assert_eq!(first, None);
    assert_eq!(second, Some(route("10.0.0.1", 49152, 1)));
    let entries = store.routes().unwrap();
    assert_eq!(entries.len(), 1);
    assert_eq!(entries[0].route, route("10.0.0.9", 49153, 2));
}

#[spec_test(REQ_IL_ironlink_066)]
#[test]
fn remove_route_when_present_then_removed_and_when_absent_then_none_and_no_file() {
    let (_guard, store) = temp_store();

    let absent = store.remove_route(ID_A).unwrap();
    store.set_route(ID_A, route("10.0.0.1", 49152, 1)).unwrap();
    let removed = store.remove_route(ID_A).unwrap();
    let again = store.remove_route(ID_A).unwrap();

    assert_eq!(absent, None);
    assert_eq!(removed, Some(route("10.0.0.1", 49152, 1)));
    assert_eq!(again, None);
    assert!(store.routes().unwrap().is_empty());
}

#[test]
fn remove_route_when_absent_from_a_missing_store_then_nothing_is_created() {
    let (_guard, store) = temp_store();

    store.remove_route(ID_A).unwrap();

    assert!(!dir_of(&store).exists());
}

#[spec_test(REQ_IL_ironlink_067)]
#[test]
fn add_address_when_added_in_any_order_twice_then_unique_and_sorted_by_number() {
    let (_guard, store) = temp_store();

    let first = store.add_address(socket("10.0.0.10:49152")).unwrap();
    store.add_address(socket("9.0.0.1:49152")).unwrap();
    store.add_address(socket("10.0.0.10:7000")).unwrap();
    store.add_address(socket("[::1]:49152")).unwrap();
    let again = store.add_address(socket("10.0.0.10:49152")).unwrap();

    assert!(first);
    assert!(!again);
    let listed: Vec<String> = store
        .addresses()
        .unwrap()
        .iter()
        .map(ToString::to_string)
        .collect();
    assert_eq!(
        listed,
        [
            "9.0.0.1:49152",
            "10.0.0.10:7000",
            "10.0.0.10:49152",
            "[::1]:49152"
        ]
    );
}

#[spec_test(REQ_IL_ironlink_067)]
#[test]
fn remove_address_when_present_then_removed_and_when_absent_then_false() {
    let (_guard, store) = temp_store();
    store.add_address(socket("10.0.0.1:49152")).unwrap();
    store.add_address(socket("10.0.0.2:49152")).unwrap();

    let removed = store.remove_address(socket("10.0.0.1:49152")).unwrap();
    let absent = store.remove_address(socket("10.0.0.1:49152")).unwrap();

    assert!(removed);
    assert!(!absent);
    assert_eq!(store.addresses().unwrap(), [socket("10.0.0.2:49152")]);
}

/// Writes `text` as the file `name` of a new store and returns the store.
fn store_with(name: &str, text: &str) -> (TempDir, Store) {
    let (guard, store) = temp_store();
    fs::create_dir_all(dir_of(&store)).unwrap();
    fs::write(dir_of(&store).join(name), text).unwrap();
    (guard, store)
}

#[spec_test(REQ_IL_ironlink_064)]
#[rstest]
#[case::not_json("this is not json", "is not JSON")]
#[case::empty("", "is not JSON")]
#[case::not_an_object("[1, 2]", "is not a JSON object")]
#[case::no_version(r#"{"routes": {}}"#, "has no version")]
#[case::other_version(r#"{"version": 2, "routes": {}}"#, "has the version 2")]
#[case::version_text(r#"{"version": "1", "routes": {}}"#, "has the version \"1\"")]
#[case::no_routes(r#"{"version": 1}"#, "expected shape")]
#[case::another_shape(r#"{"version": 1, "routes": []}"#, "expected shape")]
#[case::extra_key(r#"{"version": 1, "routes": {}, "extra": 1}"#, "expected shape")]
#[case::bad_route(
    r#"{"version": 1, "routes": {"aa": {"address": "10.0.0.1"}}}"#,
    "expected shape"
)]
#[case::unknown_driver(
    r#"{"version": 1, "routes": {"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa": {"address": "10.0.0.1", "driver": "usb", "sessionPort": 1, "verifiedAt": 1}}}"#,
    "expected shape"
)]
#[case::key_not_a_device_id(
    r#"{"version": 1, "routes": {"AAAA": {"address": "10.0.0.1", "driver": "addressList", "sessionPort": 1, "verifiedAt": 1}}}"#,
    "not a device id"
)]
#[case::port_zero(
    r#"{"version": 1, "routes": {"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa": {"address": "10.0.0.1", "driver": "addressList", "sessionPort": 0, "verifiedAt": 1}}}"#,
    "session port 0"
)]
fn routes_when_the_file_cannot_be_read_as_this_version_then_l6006_naming_the_file_and_it_is_untouched(
    #[case] text: &str,
    #[case] reason: &str,
) {
    let (_guard, store) = store_with("routes.json", text);

    let read = store.routes().unwrap_err();
    let write = store
        .set_route(ID_A, route("10.0.0.1", 49152, 1))
        .unwrap_err();
    let remove = store.remove_route(ID_A).unwrap_err();

    for err in [&read, &write, &remove] {
        assert_eq!(err.l_code(), "L6006", "{err}");
        assert_eq!(err.exit_code(), 2);
        assert!(err.message().contains("routes.json"), "{err}");
        assert!(err.message().contains(reason), "{err}");
    }
    assert_eq!(
        fs::read_to_string(dir_of(&store).join("routes.json")).unwrap(),
        text
    );
    assert_eq!(names_in(&dir_of(&store)), ["routes.json"]);
}

#[spec_test(REQ_IL_ironlink_064)]
#[rstest]
#[case::not_json("{")]
#[case::other_version(r#"{"version": 9, "addresses": []}"#)]
#[case::a_string_is_not_an_address(r#"{"version": 1, "addresses": ["printer"]}"#)]
#[case::an_address_without_a_port(r#"{"version": 1, "addresses": ["10.0.0.1"]}"#)]
#[case::port_zero(r#"{"version": 1, "addresses": ["10.0.0.1:0"]}"#)]
#[case::not_a_list(r#"{"version": 1, "addresses": {}}"#)]
fn addresses_when_the_file_cannot_be_read_as_this_version_then_l6006_and_it_is_untouched(
    #[case] text: &str,
) {
    let (_guard, store) = store_with("addresses.json", text);

    let read = store.addresses().unwrap_err();
    let add = store.add_address(socket("10.0.0.1:49152")).unwrap_err();
    let remove = store.remove_address(socket("10.0.0.1:49152")).unwrap_err();

    for err in [&read, &add, &remove] {
        assert_eq!(err.l_code(), "L6006", "{err}");
        assert!(err.message().contains("addresses.json"), "{err}");
    }
    assert_eq!(
        fs::read_to_string(dir_of(&store).join("addresses.json")).unwrap(),
        text
    );
}

#[spec_test(REQ_IL_ironlink_064)]
#[test]
fn routes_when_the_other_file_is_unreadable_then_a_file_is_read_alone() {
    let (_guard, store) = store_with("addresses.json", "garbage");

    let routes = store.routes().unwrap();
    store.set_route(ID_A, route("10.0.0.1", 49152, 1)).unwrap();

    assert!(routes.is_empty());
    assert_eq!(store.routes().unwrap().len(), 1);
}

#[spec_test(REQ_IL_ironlink_064)]
#[test]
fn routes_when_the_file_is_a_directory_then_l6006() {
    let (_guard, store) = temp_store();
    fs::create_dir_all(dir_of(&store).join("routes.json")).unwrap();

    let err = store.routes().unwrap_err();

    assert_eq!(err.l_code(), "L6006");
}

#[spec_test(REQ_IL_ironlink_065)]
#[test]
fn write_when_the_directory_cannot_be_created_then_l6007_and_nothing_is_left() {
    let (guard, _) = temp_store();
    // A file where the directory should be.
    let blocker = guard.path().join("blocker");
    fs::write(&blocker, "a file").unwrap();
    let store = Store::at(blocker.join("ironlink"));

    let err = store.add_address(socket("10.0.0.1:49152")).unwrap_err();

    assert_eq!(err.l_code(), "L6007");
    assert_eq!(err.exit_code(), 2);
    assert!(err.message().contains("addresses.json"), "{err}");
    assert_eq!(fs::read_to_string(&blocker).unwrap(), "a file");
}

#[spec_test(REQ_IL_ironlink_065)]
#[test]
fn write_when_the_file_cannot_be_replaced_then_l6007_and_the_temporary_file_is_removed() {
    let (_guard, store) = temp_store();
    // A directory in the place of the file: the rename cannot replace it.
    fs::create_dir_all(dir_of(&store).join("addresses.json").join("inside")).unwrap();
    let file = AddressesFile {
        addresses: vec![socket("10.0.0.1:49152")],
    };

    let err = store.write(&file).unwrap_err();

    assert_eq!(err.l_code(), "L6007");
    assert_eq!(names_in(&dir_of(&store)), ["addresses.json"]);
}
