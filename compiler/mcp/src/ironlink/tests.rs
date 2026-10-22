use std::ffi::OsString;
use std::fs;

use ironplc_test::device::workspace_binary;
use tempfile::TempDir;

use super::*;

/// A directory that holds a file named like the `ironlink` executable.
fn dir_with_ironlink() -> TempDir {
    let dir = TempDir::new().unwrap();
    fs::write(dir.path().join(executable_name()), b"").unwrap();
    dir
}

fn path_of(dirs: &[&Path]) -> OsString {
    env::join_paths(dirs).unwrap()
}

#[test]
fn locate_when_beside_the_server_and_on_path_then_the_one_beside_the_server() {
    let beside = dir_with_ironlink();
    let on_path = dir_with_ironlink();

    let found = locate(Some(beside.path()), Some(&path_of(&[on_path.path()]))).unwrap();

    assert_eq!(found, beside.path().join(executable_name()));
}

#[test]
fn locate_when_only_on_path_then_the_one_on_path() {
    let beside = TempDir::new().unwrap();
    let empty = TempDir::new().unwrap();
    let on_path = dir_with_ironlink();

    let found = locate(
        Some(beside.path()),
        Some(&path_of(&[empty.path(), on_path.path()])),
    )
    .unwrap();

    assert_eq!(found, on_path.path().join(executable_name()));
}

#[test]
fn locate_when_nowhere_then_not_found_naming_where_it_looked() {
    let beside = TempDir::new().unwrap();

    let result = locate(Some(beside.path()), None);

    assert!(matches!(
        result,
        Err(IronLinkError::NotFound { searched }) if searched.contains("PATH")
    ));
}

#[test]
fn call_when_version_then_the_answer_of_the_program() {
    let store = TempDir::new().unwrap();
    let ironlink = IronLink::new(
        workspace_binary("ironlink"),
        Some(store.path().to_path_buf()),
    );

    let answer = ironlink
        .call(
            &serde_json::json!({"command": "version"}),
            Duration::from_secs(1),
        )
        .unwrap();

    assert_eq!(answer["response"], "version");
}

#[test]
fn call_when_the_command_fails_then_refused_with_the_l_code() {
    let store = TempDir::new().unwrap();
    let ironlink = IronLink::new(
        workspace_binary("ironlink"),
        Some(store.path().to_path_buf()),
    );

    let result = ironlink.call(
        &serde_json::json!({"command": "identify", "address": "not-an-address"}),
        Duration::from_secs(1),
    );

    assert!(matches!(
        result,
        Err(IronLinkError::Refused { l_code: Some(code), .. }) if code == "L6004"
    ));
}

#[test]
fn call_when_the_line_is_not_a_command_then_refused_without_a_code() {
    let store = TempDir::new().unwrap();
    let ironlink = IronLink::new(
        workspace_binary("ironlink"),
        Some(store.path().to_path_buf()),
    );

    let result = ironlink.call(
        &serde_json::json!({"command": "no-such"}),
        Duration::from_secs(1),
    );

    assert!(matches!(
        result,
        Err(IronLinkError::Refused { l_code: None, .. })
    ));
}

#[test]
fn call_when_the_program_does_not_exist_then_unavailable() {
    let ironlink = IronLink::new(PathBuf::from("no-such-ironlink"), None);

    let result = ironlink.call(
        &serde_json::json!({"command": "version"}),
        Duration::from_secs(1),
    );

    assert!(matches!(result, Err(IronLinkError::Unavailable { .. })));
}
