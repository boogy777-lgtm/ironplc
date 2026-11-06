//! The note of an unfinished edit through the served device: the file beside the
//! container, its life cycle with the edit, and what `identity` reports of it
//! after a restart that lost the candidate.
//!
//! The tests drive the same pieces `serve` composes over a real temporary
//! directory, so a restart is a second boot over the same files, as in
//! [`crate::serve_retained_tests`].

use std::fs;
use std::path::PathBuf;

use ironplc_runtime::RuntimeHost;
use spec_test_macro::spec_test;

use crate::device_settings::device::Device;
use crate::serve_retained_tests::{identity, retained_source, session, Served};
use crate::serve_tests::container_bytes;
use crate::slot_store::{suffix, SlotStore};

const TEST: &str = r#"{"command":"testEdits"}"#;
const UNTEST: &str = r#"{"command":"untestEdits"}"#;
const CANCEL: &str = r#"{"command":"cancelEdits"}"#;
const ASSEMBLE: &str = r#"{"command":"assembleEdits"}"#;
const STATUS: &str = r#"{"command":"getStatus"}"#;

/// The note file of a served container.
fn marker_path(served: &Served) -> PathBuf {
    suffix(&served.file, "edit")
}

/// The note file, decoded, or `None` when it is absent.
fn marker(served: &Served) -> Option<serde_json::Value> {
    fs::read(marker_path(served))
        .ok()
        .map(|bytes| serde_json::from_slice(&bytes).unwrap())
}

/// An accept of the container of `retained_source(step)` that names the edit.
fn accept(step: i32) -> String {
    serde_json::json!({
        "command": "acceptEdits",
        "program": container_bytes(&retained_source(step)),
        "edit": {"name": "fix pump", "origin": "studio on WS-12"},
    })
    .to_string()
}

/// One session of the booted device over `lines`.
fn run(booted: &mut (RuntimeHost, Device, SlotStore), lines: &[&str]) -> Vec<serde_json::Value> {
    let (host, device, store) = booted;
    session(host, device, Some(store), lines)
}

#[spec_test(REQ_VC_vm_cli_056)]
#[test]
fn accept_when_the_edit_is_named_then_the_note_has_the_name_origin_stage_and_candidate_hash() {
    let served = Served::new();
    let mut device = served.boot();

    let responses = run(&mut device, &[&accept(10)]);

    assert_eq!(responses[0]["response"], "ack");
    let note = marker(&served).unwrap();
    assert_eq!(note["version"], 1);
    assert_eq!(note["name"], "fix pump");
    assert_eq!(note["origin"], "studio on WS-12");
    assert_eq!(note["stage"], "accepted");
    let candidate = ironplc_container::Container::read_from(&mut std::io::Cursor::new(
        container_bytes(&retained_source(10)),
    ))
    .unwrap();
    assert_eq!(
        note["candidateContentHash"],
        ironplc_container::integrity::hash_hex(&candidate.header.content_hash).to_string()
    );
}

#[spec_test(REQ_VC_vm_cli_056)]
#[test]
fn test_and_untest_when_the_stage_changes_then_the_note_follows_it() {
    let served = Served::new();
    let mut device = served.boot();
    run(&mut device, &[&accept(10)]);

    run(&mut device, &[TEST]);
    let testing = marker(&served).unwrap()["stage"].clone();
    run(&mut device, &[UNTEST]);
    let untested = marker(&served).unwrap()["stage"].clone();

    assert_eq!(testing, "testing");
    assert_eq!(untested, "accepted");
}

#[spec_test(REQ_VC_vm_cli_056)]
#[test]
fn cancel_when_the_candidate_is_staged_then_the_note_is_removed() {
    let served = Served::new();
    let mut device = served.boot();
    run(&mut device, &[&accept(10)]);

    run(&mut device, &[CANCEL]);

    assert!(marker(&served).is_none());
}

#[spec_test(REQ_VC_vm_cli_056)]
#[test]
fn cancel_when_the_candidate_is_under_test_then_the_note_is_removed_after_the_one_command() {
    let served = Served::new();
    let mut device = served.boot();
    run(&mut device, &[&accept(10), TEST]);
    assert_eq!(marker(&served).unwrap()["stage"], "testing");

    let responses = run(&mut device, &[CANCEL, STATUS]);

    assert_eq!(responses[0]["response"], "ack");
    assert_eq!(responses[1]["edit"]["stage"], "idle");
    assert!(marker(&served).is_none());
}

#[spec_test(REQ_VC_vm_cli_056)]
#[test]
fn assemble_when_the_edit_is_committed_then_the_note_is_removed() {
    let served = Served::new();
    let mut device = served.boot();

    let responses = run(&mut device, &[&accept(10), TEST, ASSEMBLE]);

    assert_eq!(responses[2]["response"], "ack");
    assert!(marker(&served).is_none());
}

#[spec_test(REQ_VC_vm_cli_056)]
#[test]
fn boot_when_a_candidate_was_under_test_then_identity_reports_it_lost_and_the_note_is_gone() {
    let served = Served::new();
    {
        let mut first = served.boot();
        run(&mut first, &[&accept(10), TEST]);
    }

    let (mut host, mut device, _store) = served.boot();
    let answer = identity(&mut host, &mut device);

    assert_eq!(
        answer["candidateLost"],
        serde_json::json!({"name": "fix pump", "origin": "studio on WS-12", "stage": "testing"})
    );
    assert!(marker(&served).is_none());
    // The device never restores the candidate: it boots the assembled
    // application, with no edit.
    assert_eq!(answer["application"]["mode"], "normal");
    assert_eq!(answer["application"]["candidate"], serde_json::Value::Null);
    assert_eq!(answer["application"]["edit"]["stage"], "idle");
}

#[spec_test(REQ_VC_vm_cli_056)]
#[test]
fn boot_when_a_candidate_was_staged_then_identity_reports_the_stage_accepted() {
    let served = Served::new();
    {
        let mut first = served.boot();
        run(&mut first, &[&accept(10)]);
    }

    let (mut host, mut device, _store) = served.boot();

    assert_eq!(
        identity(&mut host, &mut device)["candidateLost"]["stage"],
        "accepted"
    );
}

#[spec_test(REQ_VC_vm_cli_056)]
#[test]
fn boot_when_the_edit_was_finished_then_identity_reports_nothing_lost() {
    let served = Served::new();
    {
        let mut first = served.boot();
        run(&mut first, &[&accept(10), TEST, ASSEMBLE]);
    }

    let (mut host, mut device, _store) = served.boot();

    assert!(identity(&mut host, &mut device)
        .get("candidateLost")
        .is_none());
}

#[spec_test(REQ_VC_vm_cli_056)]
#[test]
fn boot_when_the_lost_candidate_was_already_reported_then_the_next_boot_reports_nothing() {
    let served = Served::new();
    {
        let mut first = served.boot();
        run(&mut first, &[&accept(10)]);
    }
    drop(served.boot());

    let (mut host, mut device, _store) = served.boot();

    assert!(identity(&mut host, &mut device)
        .get("candidateLost")
        .is_none());
}

#[spec_test(REQ_VC_vm_cli_056)]
#[test]
fn boot_when_the_note_is_damaged_then_identity_reports_a_lost_edit_without_details() {
    let served = Served::new();
    fs::write(marker_path(&served), b"not a note").unwrap();

    let (mut host, mut device, _store) = served.boot();

    assert_eq!(
        identity(&mut host, &mut device)["candidateLost"],
        serde_json::json!({})
    );
    assert!(marker(&served).is_none());
}

#[spec_test(REQ_VC_vm_cli_056)]
#[test]
fn accept_when_the_note_cannot_be_written_then_the_accept_is_still_acknowledged() {
    let served = Served::new();
    let mut device = served.boot();
    fs::create_dir(suffix(&served.file, "edit.tmp")).unwrap();

    let responses = run(&mut device, &[&accept(10), STATUS]);

    // The candidate is staged; the note is advisory, so its failure is logged
    // and does not turn the acknowledgment into an error.
    assert_eq!(responses[0]["response"], "ack");
    assert_eq!(responses[1]["edit"]["stage"], "accepted");
    assert!(marker(&served).is_none());
}

#[spec_test(REQ_VC_vm_cli_057)]
#[test]
fn status_when_a_candidate_is_staged_then_the_edit_warns_that_a_restart_loses_it() {
    let served = Served::new();
    let mut device = served.boot();

    let idle = run(&mut device, &[STATUS]);
    let staged = run(&mut device, &[&accept(10), STATUS]);
    let tested = run(&mut device, &[TEST, STATUS]);
    let ended = run(&mut device, &[CANCEL, STATUS]);

    assert!(idle[0]["edit"].get("warning").is_none());
    assert_eq!(
        staged[1]["edit"]["warning"],
        "a restart of the device loses this candidate"
    );
    assert_eq!(
        tested[1]["edit"]["warning"],
        "a restart of the device loses this candidate"
    );
    assert!(ended[1]["edit"].get("warning").is_none());
}
