//! The replace mode of an accept through the served device (ADR-0088): a
//! Download that keeps the `PERSISTENT` variables with an equal declaration.
//!
//! The tests drive the pieces `serve` composes (the slot store, the device,
//! `serve_session`, `stop_device`) over a real temporary directory, so a
//! restart is a second boot over the same files, as in the tests of the retained
//! image.

use std::fs;
use std::io;

use ironplc_container::{Container, VarIndex};
use ironplc_dsl::core::Id;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_runtime::RuntimeHost;
use spec_test_macro::spec_test;

use crate::serve::stop_device;
use crate::serve_retained_tests::{identity, session, Served};

/// The stable IDs of the programs, by variable name.
const IDS: [(&str, u64); 7] = [
    ("Kept", 1),
    ("Retyped", 2),
    ("Gone", 3),
    ("Retained", 4),
    ("Plain", 5),
    ("Fresh", 9),
    ("Newp", 10),
];

/// The running application: one variable of each kind of retention and one the
/// replacement removes. Every variable counts up once per scan.
const RUNNING: &str = "
PROGRAM main
  VAR PERSISTENT
    Kept : DINT;
    Retyped : DINT;
    Gone : DINT;
  END_VAR
  VAR RETAIN
    Retained : DINT;
  END_VAR
  VAR
    Plain : DINT;
  END_VAR
  Kept := Kept + 1;
  Retyped := Retyped + 1;
  Gone := Gone + 1;
  Retained := Retained + 1;
  Plain := Plain + 1;
END_PROGRAM
";

/// The replacement: another layout (a variable in front of the others), `Kept`
/// as before, `Retyped` as a LINT, `Gone` removed, and two new variables with
/// declared initial values.
const REPLACEMENT: &str = "
PROGRAM main
  VAR
    Fresh : DINT := 100;
  END_VAR
  VAR PERSISTENT
    Kept : DINT;
    Retyped : LINT;
    Newp : DINT := 77;
  END_VAR
  VAR RETAIN
    Retained : DINT;
  END_VAR
  VAR
    Plain : DINT;
  END_VAR
  Kept := Kept + 1;
  Retyped := Retyped + 1;
  Retained := Retained + 1;
  Plain := Plain + 1;
END_PROGRAM
";

/// Compiles `source` under the CODESYS dialect (which allows `PERSISTENT`) with
/// the stable variable IDs of [`IDS`], through the wire format.
fn compile(source: &str) -> Container {
    let options = CompilerOptions::from_dialect(Dialect::Codesys);
    let library =
        ironplc_parser::parse_program(source, &ironplc_dsl::core::FileId::default(), &options)
            .unwrap();
    let (analyzed, context) =
        ironplc_analyzer::stages::resolve_types(&[&library], &options).unwrap();
    let codegen_options = ironplc_codegen::CodegenOptions {
        stable_var_ids: IDS
            .iter()
            .map(|(name, uid)| (Id::from(name), *uid))
            .collect(),
        ..ironplc_codegen::CodegenOptions::from(&options)
    };
    let container = ironplc_codegen::compile(
        &analyzed,
        &context,
        &codegen_options,
        &ironplc_codegen::EmptyLookup,
    )
    .unwrap();
    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    Container::read_from(&mut io::Cursor::new(&bytes)).unwrap()
}

fn wire(container: &Container) -> Vec<u8> {
    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    bytes
}

/// The value of the variable `name` of `container` in `host`.
fn value(host: &RuntimeHost, container: &Container, name: &str) -> i32 {
    let index: VarIndex = container
        .debug_section
        .as_ref()
        .unwrap()
        .var_names
        .iter()
        .find(|entry| entry.name.eq_ignore_ascii_case(name))
        .unwrap()
        .var_index;
    host.read_variable(index).unwrap()
}

/// The lines that accept `container` in the given mode (`None` is the default
/// mode), test it and assemble it.
fn download_lines(container: &Container, mode: Option<&str>) -> Vec<String> {
    let mut accept = serde_json::json!({
        "command": "acceptEdits",
        "program": wire(container),
    });
    if let Some(mode) = mode {
        accept["mode"] = serde_json::json!(mode);
    }
    vec![
        accept.to_string(),
        r#"{"command":"testEdits"}"#.to_string(),
        r#"{"command":"assembleEdits"}"#.to_string(),
    ]
}

fn as_strs(lines: &[String]) -> Vec<&str> {
    lines.iter().map(String::as_str).collect()
}

/// A served directory that holds `RUNNING`.
fn served_running() -> Served {
    let served = Served::new();
    fs::write(&served.file, wire(&compile(RUNNING))).unwrap();
    served
}

#[spec_test(REQ_VC_vm_cli_062)]
#[test]
fn download_when_replace_then_the_equal_persistent_value_is_kept_and_the_reply_lists_the_rest() {
    let served = served_running();
    let (mut host, mut device, mut store) = served.boot();
    host.run(3, || 0).unwrap();
    let replacement = compile(REPLACEMENT);
    let lines = download_lines(&replacement, Some("replace"));

    let responses = session(&mut host, &mut device, Some(&mut store), &as_strs(&lines));

    assert_eq!(responses[0]["response"], "accepted");
    assert_eq!(
        responses[0]["reinitialised"],
        serde_json::json!([
            {"uid": 2, "name": "Retyped", "reason": "typeChanged"},
            {"uid": 3, "name": "Gone", "reason": "notInCandidate"}
        ])
    );
    assert_eq!(responses[1]["response"], "ack");
    assert_eq!(responses[2]["response"], "ack");
    // The replacement counts on from the kept value; every other variable
    // started again, the new ones at their declared initial values.
    let kept = value(&host, &replacement, "Kept");
    assert!(kept >= 4, "kept {kept}");
    assert_eq!(value(&host, &replacement, "Newp"), 77);
    assert_eq!(value(&host, &replacement, "Fresh"), 100);
    assert!(value(&host, &replacement, "Retained") < kept);
    assert!(value(&host, &replacement, "Plain") < kept);
    assert!(value(&host, &replacement, "Retyped") < kept);
}

#[spec_test(REQ_VC_vm_cli_063)]
#[test]
fn download_when_replace_is_assembled_then_a_restart_keeps_the_persistent_value() {
    let served = served_running();
    let (mut host, mut device, mut store) = served.boot();
    host.run(3, || 0).unwrap();
    let replacement = compile(REPLACEMENT);
    let lines = download_lines(&replacement, Some("replace"));
    let responses = session(&mut host, &mut device, Some(&mut store), &as_strs(&lines));
    assert_eq!(responses[2]["response"], "ack");
    host.run(2, || 0).unwrap();
    let before = value(&host, &replacement, "Kept");
    stop_device(&mut device, &host, Ok(())).unwrap();

    let (restarted, _device, _store) = served.boot();

    assert_eq!(value(&restarted, &replacement, "Kept"), before);
    assert_eq!(value(&restarted, &replacement, "Newp"), 77);
}

#[spec_test(REQ_VC_vm_cli_062)]
#[test]
fn download_when_replace_is_assembled_without_a_restart_in_between_then_the_retained_file_is_written_at_the_assemble(
) {
    // A device that is killed after the Download, with no controlled stop,
    // still finds the values of the replacement in its retained file.
    let served = served_running();
    let (mut host, mut device, mut store) = served.boot();
    host.run(3, || 0).unwrap();
    let replacement = compile(REPLACEMENT);
    let lines = download_lines(&replacement, Some("replace"));
    let responses = session(&mut host, &mut device, Some(&mut store), &as_strs(&lines));
    assert_eq!(responses[2]["response"], "ack");
    let at_assemble = value(&host, &replacement, "Kept");

    let (restarted, _device, _store) = served.boot();

    assert!(value(&restarted, &replacement, "Kept") >= 4);
    assert!(value(&restarted, &replacement, "Kept") <= at_assemble);
}

#[spec_test(REQ_VC_vm_cli_064)]
#[test]
fn download_when_replace_is_under_test_then_untest_and_cancel_are_refused_with_v4011() {
    let served = served_running();
    let (mut host, mut device, mut store) = served.boot();
    host.run(3, || 0).unwrap();
    let replacement = compile(REPLACEMENT);
    let mut lines = download_lines(&replacement, Some("replace"));
    lines.truncate(2);
    lines.push(r#"{"command":"untestEdits"}"#.to_string());
    lines.push(r#"{"command":"cancelEdits"}"#.to_string());
    lines.push(r#"{"command":"getStatus"}"#.to_string());

    let responses = session(&mut host, &mut device, Some(&mut store), &as_strs(&lines));

    assert_eq!(responses[2]["vCode"], "V4011");
    assert_eq!(responses[3]["vCode"], "V4011");
    assert_eq!(responses[4]["edit"]["stage"], "testingReplace");
    assert_eq!(responses[4]["candidateMode"], "replace");
    assert_eq!(responses[4]["edit"]["next"]["command"], "assemble");
}

#[spec_test(REQ_VC_vm_cli_062)]
#[test]
fn download_when_the_device_holds_the_empty_application_then_the_program_replaces_it_and_runs() {
    // A device started with no file holds the empty application, which has no
    // task, no variable and no layout in common with any program.
    let served = Served::new();
    fs::write(
        &served.file,
        ironplc_container::empty_application_bytes().unwrap(),
    )
    .unwrap();
    let (mut host, mut device, mut store) = served.boot();
    assert!(!host.has_runnable_task());
    let replacement = compile(REPLACEMENT);
    let lines = download_lines(&replacement, Some("replace"));

    let responses = session(&mut host, &mut device, Some(&mut store), &as_strs(&lines));

    assert_eq!(responses[0]["response"], "accepted");
    assert_eq!(responses[0]["reinitialised"], serde_json::json!([]));
    assert_eq!(responses[1]["response"], "ack");
    assert_eq!(responses[2]["response"], "ack");
    assert!(host.has_runnable_task());
    host.run(2, || 0).unwrap();
    assert!(value(&host, &replacement, "Kept") >= 1);
    assert_eq!(value(&host, &replacement, "Newp"), 77);
    let answer = identity(&mut host, &mut device);
    let expected =
        ironplc_container::integrity::hash_hex(&replacement.header.content_hash).to_string();
    let empty = ironplc_container::integrity::hash_hex(&ironplc_container::EMPTY_APPLICATION_HASH)
        .to_string();
    assert_eq!(answer["application"]["contentHash"], expected);
    assert_ne!(answer["application"]["contentHash"], empty);
    assert_eq!(answer["persistedContentHash"], expected);
}
