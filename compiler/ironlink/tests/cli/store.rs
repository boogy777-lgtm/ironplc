//! Tests of the commands that keep the workstation store (`route`, `address`) and
//! of the browse that uses it, started as processes. The helpers are those of the
//! parent module.

use std::path::{Path, PathBuf};
use std::process::Stdio;

use serde_json::json;
use spec_test_macro::spec_test;

use crate::{json_of, run, silent_address, Device, Process, Shell, StoreDir};

/// A fast identify for the commands that send one.
const FAST: [&str; 4] = ["--timeout-ms", "1500", "--attempts", "1"];

fn route_set_args<'a>(id: &'a str, address: &'a str) -> Vec<&'a str> {
    let mut args = vec!["route", "set", id, address];
    args.extend(FAST);
    args
}

#[spec_test(REQ_IL_ironlink_078)]
#[test]
fn route_set_when_the_device_answers_then_a_row_is_printed_and_routes_json_holds_the_route() {
    let device = Device::start(5, "Press");
    let address = device.addr.to_string();
    let store = StoreDir::new();

    let finished = store.run(&route_set_args(&Device::id(5), &address));

    assert!(finished.status.success(), "{}", finished.stderr);
    let row = finished.stdout.trim();
    assert!(
        row.starts_with(&format!(
            "stored route {}  127.0.0.1:{}  address list  verified ",
            Device::id(5),
            device.addr.port()
        )),
        "{row}"
    );
    let file = json_of(&store.read("routes.json"));
    let route = &file["routes"][Device::id(5)];
    assert_eq!(file["version"], 1);
    assert_eq!(route["address"], "127.0.0.1");
    assert_eq!(route["driver"], "addressList");
    assert_eq!(route["sessionPort"], device.addr.port());
    assert!(route["verifiedAt"].is_u64());
}

#[spec_test(REQ_IL_ironlink_078)]
#[test]
fn route_list_and_remove_when_a_route_is_stored_then_listed_removed_and_removing_again_is_no_error()
{
    let device = Device::start(5, "Press");
    let store = StoreDir::new();
    store.run(&route_set_args(&Device::id(5), &device.addr.to_string()));

    let listed = store.run(&["route", "list"]);
    let removed = store.run(&["route", "remove", &Device::id(5)]);
    let again = store.run(&["route", "remove", &Device::id(5)]);
    let empty = store.run(&["route", "list"]);

    assert!(
        listed.stdout.starts_with(&Device::id(5)),
        "{}",
        listed.stdout
    );
    assert_eq!(
        removed.stdout.trim(),
        format!("removed the route to {}", Device::id(5))
    );
    assert!(again.status.success(), "{}", again.stderr);
    assert_eq!(
        again.stdout.trim(),
        format!("no route to {}", Device::id(5))
    );
    assert_eq!(empty.stdout.trim(), "no routes");
}

#[spec_test(REQ_IL_ironlink_078)]
#[test]
fn route_list_when_json_then_it_is_the_response_the_shell_gives_for_the_same_command() {
    let device = Device::start(5, "Press");
    let store = StoreDir::new();
    store.run(&route_set_args(&Device::id(5), &device.addr.to_string()));
    let from_command_line = json_of(store.run(&["route", "list", "--json"]).stdout.trim());
    let mut shell = store.shell();

    let from_shell = json_of(&shell.ask(r#"{"command":"routeList"}"#));

    assert_eq!(from_command_line["response"], "routeList");
    assert_eq!(from_command_line, from_shell);
}

#[spec_test(REQ_IL_ironlink_078)]
#[test]
fn route_set_when_another_device_answers_then_exit_2_with_l6008_and_no_file() {
    let device = Device::start(5, "Press");
    let store = StoreDir::new();

    let finished = store.run(&route_set_args(&Device::id(6), &device.addr.to_string()));

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6008"), "{}", finished.stderr);
    assert!(
        finished.stderr.contains(&Device::id(5)),
        "{}",
        finished.stderr
    );
    assert!(
        finished.stderr.contains(&Device::id(6)),
        "{}",
        finished.stderr
    );
    assert!(finished.stdout.is_empty());
    assert!(!store.path().exists());
}

#[spec_test(REQ_IL_ironlink_078)]
#[test]
fn route_set_when_nothing_answers_then_exit_2_with_l6003_and_no_file() {
    let (_socket, address) = silent_address();
    let address = address.to_string();
    let id = Device::id(5);
    let store = StoreDir::new();

    let finished = store.run(&[
        "route",
        "set",
        &id,
        &address,
        "--timeout-ms",
        "300",
        "--attempts",
        "1",
    ]);

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6003"), "{}", finished.stderr);
    assert!(!store.path().exists());
}

#[spec_test(REQ_IL_ironlink_078)]
#[test]
fn route_set_when_the_device_id_is_not_valid_then_exit_2_with_l6004() {
    let finished = run(&["route", "set", "PRESS", "127.0.0.1:9"]);

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6004"), "{}", finished.stderr);
}

#[spec_test(REQ_IL_ironlink_078)]
#[test]
fn address_add_remove_and_list_when_run_then_the_list_is_kept_unique_and_sorted() {
    let store = StoreDir::new();

    let first = store.run(&["address", "add", "10.0.0.2:7000"]);
    store.run(&["address", "add", "10.0.0.1"]);
    let again = store.run(&["address", "add", "10.0.0.1"]);
    let listed = store.run(&["address", "list"]);
    let removed = store.run(&["address", "remove", "10.0.0.2:7000"]);
    let absent = store.run(&["address", "remove", "10.0.0.2:7000"]);
    let json = store.run(&["address", "list", "--json"]);

    assert_eq!(first.stdout.trim(), "added 10.0.0.2:7000");
    assert!(again.status.success());
    assert_eq!(
        again.stdout.trim(),
        "10.0.0.1:49152 was in the list already"
    );
    assert_eq!(listed.stdout.trim(), "10.0.0.1:49152\n10.0.0.2:7000");
    assert_eq!(removed.stdout.trim(), "removed 10.0.0.2:7000");
    assert!(absent.status.success());
    assert_eq!(absent.stdout.trim(), "10.0.0.2:7000 was not in the list");
    assert_eq!(
        json_of(json.stdout.trim()),
        json!({"response": "addressList", "addresses": ["10.0.0.1:49152"]})
    );
    assert_eq!(
        json_of(&store.read("addresses.json")),
        json!({"version": 1, "addresses": ["10.0.0.1:49152"]})
    );
}

#[spec_test(REQ_IL_ironlink_078)]
#[test]
fn address_add_when_not_an_address_then_exit_2_with_l6004() {
    let finished = run(&["address", "add", "printer"]);

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6004"), "{}", finished.stderr);
}

#[spec_test(REQ_IL_ironlink_074)]
#[test]
fn browse_when_an_address_is_stored_then_the_device_is_found_without_naming_it() {
    let named = Device::start(7, "Lathe");
    let stored = Device::start(8, "Mill");
    let store = StoreDir::new();
    store.run(&["address", "add", &stored.addr.to_string()]);

    let finished = store.run(&[
        "browse",
        "--address",
        &named.addr.to_string(),
        "--timeout-ms",
        "500",
        "--attempts",
        "1",
    ]);

    assert!(finished.status.success(), "{}", finished.stderr);
    let lines: Vec<&str> = finished.stdout.lines().collect();
    assert_eq!(lines.len(), 4, "{}", finished.stdout);
    assert_eq!(
        lines[1],
        format!("  address list: {}, {}", named.addr, stored.addr)
    );
    assert!(lines[2].trim_start().starts_with("Lathe"), "{}", lines[2]);
    assert!(lines[3].trim_start().starts_with("Mill"), "{}", lines[3]);
}

#[spec_test(REQ_IL_ironlink_075)]
#[test]
fn browse_when_a_route_is_stored_then_its_row_has_the_route_marker() {
    let routed = Device::start(7, "Lathe");
    let plain = Device::start(8, "Mill");
    let store = StoreDir::new();
    store.run(&route_set_args(&Device::id(7), &routed.addr.to_string()));

    let finished = store.run(&[
        "browse",
        "--address",
        &routed.addr.to_string(),
        "--address",
        &plain.addr.to_string(),
        "--timeout-ms",
        "500",
        "--attempts",
        "1",
    ]);

    assert!(finished.status.success(), "{}", finished.stderr);
    let rows: Vec<&str> = finished.stdout.lines().skip(2).collect();
    assert_eq!(rows.len(), 2, "{}", finished.stdout);
    assert!(
        rows[0].contains("Lathe") && rows[0].ends_with("  route"),
        "{}",
        rows[0]
    );
    assert!(
        rows[1].contains("Mill") && !rows[1].ends_with("route"),
        "{}",
        rows[1]
    );
}

#[spec_test(REQ_IL_ironlink_076)]
#[test]
fn browse_when_json_and_a_stored_route_did_not_answer_then_routes_not_seen_lists_it() {
    let seen = Device::start(7, "Lathe");
    let (_socket, away) = silent_address();
    let store = StoreDir::new();
    store.run(&route_set_args(&Device::id(7), &seen.addr.to_string()));
    // A route to a device that is away, in the file as IronLink wrote the first.
    let file = json_of(&store.read("routes.json"));
    let mut routes = file["routes"].clone();
    routes[Device::id(9)] = json!({
        "address": away.ip().to_string(), "driver": "addressList",
        "sessionPort": away.port(), "verifiedAt": 1
    });
    std::fs::write(
        store.path().join("routes.json"),
        json!({"version": 1, "routes": routes}).to_string(),
    )
    .unwrap();

    let finished = store.run(&[
        "browse",
        "--address",
        &seen.addr.to_string(),
        "--timeout-ms",
        "500",
        "--attempts",
        "1",
        "--json",
    ]);

    let response = json_of(finished.stdout.trim());
    assert_eq!(response["routesNotSeen"].as_array().unwrap().len(), 1);
    assert_eq!(response["routesNotSeen"][0]["deviceId"], Device::id(9));
    assert_eq!(
        response["drivers"][0]["devices"][0]["variants"][0]["routes"][0]["routed"],
        true
    );
}

#[spec_test(REQ_IL_ironlink_061)]
#[test]
fn store_option_when_the_variable_is_also_set_then_the_option_wins() {
    let by_variable = StoreDir::new();
    let by_option = StoreDir::new();
    let option_dir = by_option.path().to_string_lossy().into_owned();

    let finished = Process::spawn_with(
        &["address", "add", "10.0.0.1:7000", "--store", &option_dir],
        Stdio::null(),
        |command| {
            command.env("IRONLINK_STORE", by_variable.path());
        },
    )
    .finish();

    assert!(finished.status.success(), "{}", finished.stderr);
    assert!(by_option.path().join("addresses.json").exists());
    assert!(!by_variable.path().exists());
}

#[spec_test(REQ_IL_ironlink_060)]
#[test]
fn store_when_no_option_and_no_variable_then_the_directory_of_the_user_under_the_environment() {
    // Every root the program may read is a new temporary directory: the program
    // finds its default directory there, and never in the home of the user.
    let home = tempfile::tempdir().unwrap();
    let root = home.path().to_path_buf();

    let finished = Process::spawn_with(
        &["address", "add", "10.0.0.1:7000"],
        Stdio::null(),
        |command| {
            command
                .env_remove("IRONLINK_STORE")
                .env("APPDATA", &root)
                .env("HOME", &root)
                .env_remove("XDG_CONFIG_HOME");
        },
    )
    .finish();

    assert!(finished.status.success(), "{}", finished.stderr);
    let expected: PathBuf = if cfg!(windows) {
        root.join("IronPLC").join("ironlink")
    } else if cfg!(target_os = "macos") {
        root.join("Library")
            .join("Application Support")
            .join("IronPLC")
            .join("ironlink")
    } else {
        root.join(".config").join("ironplc").join("ironlink")
    };
    assert!(
        expected.join("addresses.json").exists(),
        "{}",
        expected.display()
    );
}

#[spec_test(REQ_IL_ironlink_064)]
#[test]
fn route_list_when_the_route_record_is_unreadable_then_exit_2_with_l6006_and_the_file_is_untouched()
{
    let store = StoreDir::new();
    std::fs::create_dir_all(store.path()).unwrap();
    std::fs::write(store.path().join("routes.json"), "{\"version\": 2}").unwrap();

    let routes = store.run(&["route", "list"]);
    let remove = store.run(&["route", "remove", &Device::id(1)]);
    let addresses = store.run(&["address", "list"]);

    for finished in [&routes, &remove] {
        assert_eq!(finished.status.code(), Some(2));
        assert!(finished.stderr.contains("L6006"), "{}", finished.stderr);
        assert!(
            finished.stderr.contains("routes.json"),
            "{}",
            finished.stderr
        );
        assert!(finished.stdout.is_empty());
    }
    assert!(addresses.status.success());
    assert_eq!(store.read("routes.json"), "{\"version\": 2}");
}

#[spec_test(REQ_IL_ironlink_065)]
#[test]
fn address_add_when_the_store_directory_cannot_be_created_then_exit_2_with_l6007() {
    let scratch = tempfile::tempdir().unwrap();
    let blocker: &Path = &scratch.path().join("blocker");
    std::fs::write(blocker, "a file").unwrap();

    let finished = Process::spawn_with(
        &["address", "add", "10.0.0.1:7000"],
        Stdio::null(),
        |command| {
            command.env("IRONLINK_STORE", blocker.join("ironlink"));
        },
    )
    .finish();

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6007"), "{}", finished.stderr);
}

#[spec_test(REQ_IL_ironlink_077)]
#[test]
fn serve_when_store_commands_are_piped_then_they_share_the_store_of_the_option() {
    let store = StoreDir::new();
    let option_dir = store.path().to_string_lossy().into_owned();
    let mut shell = Shell::start(&["--store", &option_dir]);

    let added = json_of(&shell.ask(r#"{"command":"addressAdd","address":"10.0.0.1:7000"}"#));
    let listed = json_of(&shell.ask(r#"{"command":"addressList"}"#));
    let (status, _) = shell.close();

    assert_eq!(added["added"], true);
    assert_eq!(listed["addresses"], json!(["10.0.0.1:7000"]));
    assert!(status.success());
    assert!(store.path().join("addresses.json").exists());
}
