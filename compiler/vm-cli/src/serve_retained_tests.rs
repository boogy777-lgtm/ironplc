//! The retained image through the served device (ADR-0085): the file beside
//! the container, the boot result and what `identity` reports of it, and the
//! write points (a controlled stop, `assembleEdits`, the optional period).
//!
//! The tests drive the same pieces `serve` composes (the slot store, the
//! device, `serve_session`, `stop_device`) over a real temporary directory, so
//! a restart is a second boot over the same files. The stdin loop of `serve`
//! itself is covered by the process tests in `tests/`.

use std::fs;
use std::io;
use std::path::PathBuf;
use std::sync::atomic::{AtomicU64, Ordering};
use std::sync::Arc;

use ironplc_container::VarIndex;
use ironplc_runtime::{RetainedImage, RuntimeHost};
use rstest::rstest;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use crate::device_loop::scripted::serve_session;
use crate::device_settings::device::Device;
use crate::error;
use crate::serve::{compose_host, stop_device};
use crate::serve_tests::{compile_container, container_bytes};
use crate::slot_store::{suffix, SlotStore};

/// A program with one retained variable, `Kept`, and one that is not,
/// `Plain`, both incremented by `step` per scan. The declarations are the same
/// for every step, so two steps are two applications with one layout.
pub(crate) fn retained_source(step: i32) -> String {
    format!(
        "PROGRAM main
  VAR RETAIN
    Kept : DINT;
  END_VAR
  VAR
    Plain : DINT;
  END_VAR
  Kept := Kept + {step};
  Plain := Plain + {step};
END_PROGRAM
"
    )
}

/// The served container of a temporary directory, with a booted slot store.
pub(crate) struct Served {
    /// Keeps the directory alive for as long as the test uses it.
    _dir: TempDir,
    pub(crate) file: PathBuf,
}

impl Served {
    /// A directory holding the container of `retained_source(1)`.
    pub(crate) fn new() -> Self {
        let dir = TempDir::new().unwrap();
        let file = dir.path().join("app.iplc");
        fs::write(&file, container_bytes(&retained_source(1))).unwrap();
        Served { _dir: dir, file }
    }

    /// The path of the retained file.
    pub(crate) fn retained(&self) -> PathBuf {
        suffix(&self.file, "retained")
    }

    /// The path of the temporary file of the retained file.
    pub(crate) fn retained_tmp(&self) -> PathBuf {
        suffix(&self.file, "retained.tmp")
    }

    /// A boot of the device as `serve` does it: the slot store boots the
    /// application, the host is composed, the device opens its files and
    /// applies the retained file.
    pub(crate) fn boot(&self) -> (RuntimeHost, Device, SlotStore) {
        let store = SlotStore::beside(&self.file);
        let container = store.boot().unwrap();
        let (mut host, _shell) = compose_host(container, false).unwrap();
        let mut device = Device::open(&self.file).unwrap();
        device.boot_application(&mut host);
        (host, device, store)
    }
}

/// The value of `Kept`.
fn kept(host: &RuntimeHost) -> i32 {
    host.read_variable(VarIndex::new(0)).unwrap()
}

/// The value of `Plain`.
fn plain(host: &RuntimeHost) -> i32 {
    host.read_variable(VarIndex::new(1)).unwrap()
}

/// A host over `retained_source(step)` that ran `rounds` scans.
pub(crate) fn host_after(step: i32, rounds: u64) -> RuntimeHost {
    let (mut host, _shell) =
        compose_host(compile_container(&retained_source(step)), false).unwrap();
    host.run(rounds, || 0).unwrap();
    host
}

/// Runs `lines` through one session and returns the parsed response per line.
pub(crate) fn session(
    host: &mut RuntimeHost,
    device: &mut Device,
    store: Option<&mut SlotStore>,
    lines: &[&str],
) -> Vec<serde_json::Value> {
    let mut input = lines.join("\n");
    input.push('\n');
    let mut output = Vec::new();
    serve_session(
        host,
        io::Cursor::new(input.into_bytes()),
        &mut output,
        store,
        device,
        None,
    )
    .unwrap();
    String::from_utf8(output)
        .unwrap()
        .lines()
        .map(|line| serde_json::from_str(line).unwrap())
        .collect()
}

/// The `identity` answer of the device.
pub(crate) fn identity(host: &mut RuntimeHost, device: &mut Device) -> serde_json::Value {
    session(host, device, None, &[r#"{"command":"identity"}"#]).remove(0)
}

/// The lines that accept, test and assemble the container of
/// `retained_source(step)`.
fn assemble_lines(step: i32) -> Vec<String> {
    let accept = serde_json::json!({
        "command": "acceptEdits",
        "program": container_bytes(&retained_source(step)),
    })
    .to_string();
    vec![
        accept,
        r#"{"command":"testEdits"}"#.to_string(),
        r#"{"command":"assembleEdits"}"#.to_string(),
    ]
}

fn as_strs(lines: &[String]) -> Vec<&str> {
    lines.iter().map(String::as_str).collect()
}

fn hash_hex(bytes: &[u8]) -> String {
    let container = ironplc_container::Container::read_from(&mut io::Cursor::new(bytes)).unwrap();
    ironplc_container::integrity::hash_hex(&container.header.content_hash).to_string()
}

/// The file `served` keeps, decoded.
fn stored_image(served: &Served) -> RetainedImage {
    RetainedImage::decode(&fs::read(served.retained()).unwrap()).unwrap()
}

#[spec_test(REQ_VC_vm_cli_037)]
#[test]
fn boot_application_when_no_retained_file_then_absent_and_identity_reports_it() {
    let served = Served::new();

    let (mut host, mut device, _store) = served.boot();

    assert_eq!(kept(&host), 0);
    assert_eq!(
        identity(&mut host, &mut device)["retained"],
        serde_json::json!({"result": "absent"})
    );
}

#[spec_test(REQ_VC_vm_cli_037)]
#[test]
fn boot_application_when_stopped_with_values_then_a_restart_restores_the_retained_ones_only() {
    let served = Served::new();
    let (mut host, mut device, _store) = served.boot();
    host.run(3, || 0).unwrap();
    stop_device(&mut device, &host, Ok(())).unwrap();

    let (mut restarted, mut restarted_device, _store) = served.boot();

    assert_eq!(kept(&restarted), 3);
    assert_eq!(plain(&restarted), 0);
    assert_eq!(
        identity(&mut restarted, &mut restarted_device)["retained"],
        serde_json::json!({"result": "applied"})
    );
}

/// The retained file of the application, with the values of three scans.
fn write_image_of_three_scans(served: &Served) -> RetainedImage {
    let image = host_after(1, 3).retained_image();
    fs::write(served.retained(), image.encode()).unwrap();
    image
}

fn write_garbage(served: &Served) {
    fs::write(served.retained(), b"not a retained image").unwrap();
}

fn write_truncated(served: &Served) {
    let image = write_image_of_three_scans(served);
    fs::write(served.retained(), &image.encode()[..10]).unwrap();
}

fn write_image_of_another_application(served: &Served) {
    let image = host_after(2, 3).retained_image();
    fs::write(served.retained(), image.encode()).unwrap();
}

fn write_image_of_other_retained_layout(served: &Served) {
    let mut image = write_image_of_three_scans(served);
    image.tag.runs_hash[0] ^= 1;
    fs::write(served.retained(), image.encode()).unwrap();
}

fn write_image_with_a_missing_slot(served: &Served) {
    let mut image = write_image_of_three_scans(served);
    image.vars.clear();
    fs::write(served.retained(), image.encode()).unwrap();
}

#[spec_test(REQ_VC_vm_cli_037)]
#[rstest]
#[case::garbage(write_garbage, "damaged")]
#[case::truncated(write_truncated, "damaged")]
#[case::short_content(write_image_with_a_missing_slot, "damaged")]
#[case::another_application(write_image_of_another_application, "otherApplication")]
#[case::other_retained_layout(write_image_of_other_retained_layout, "otherRetainedLayout")]
fn boot_application_when_the_file_cannot_be_used_then_nothing_is_applied_and_identity_names_why(
    #[case] prepare: fn(&Served),
    #[case] reason: &str,
) {
    let served = Served::new();
    prepare(&served);

    let (mut host, mut device, _store) = served.boot();

    assert_eq!(kept(&host), 0);
    assert_eq!(
        identity(&mut host, &mut device)["retained"],
        serde_json::json!({"result": "discarded", "reason": reason})
    );
}

#[test]
fn boot_application_when_the_file_is_discarded_then_the_next_stop_replaces_it() {
    let served = Served::new();
    write_image_of_another_application(&served);
    let (mut host, mut device, _store) = served.boot();
    host.run(2, || 0).unwrap();

    stop_device(&mut device, &host, Ok(())).unwrap();

    let (restarted, _device, _store) = served.boot();
    assert_eq!(kept(&restarted), 2);
}

#[spec_test(REQ_VC_vm_cli_040)]
#[test]
fn identity_when_the_device_keeps_nothing_then_the_storage_fields_are_absent() {
    let mut host = host_after(1, 0);
    let mut device = Device::in_memory();

    let answer = identity(&mut host, &mut device);

    assert!(answer.get("retained").is_none());
    assert!(answer.get("persistedContentHash").is_none());
}

#[spec_test(REQ_VC_vm_cli_040)]
#[test]
fn identity_when_booted_then_the_persisted_content_hash_is_the_booted_applications() {
    let served = Served::new();

    let (mut host, mut device, _store) = served.boot();
    let answer = identity(&mut host, &mut device);

    let expected = hash_hex(&container_bytes(&retained_source(1)));
    assert_eq!(answer["persistedContentHash"], expected);
    assert_eq!(answer["application"]["contentHash"], expected);
}

#[spec_test(REQ_VC_vm_cli_038)]
#[test]
fn assemble_when_committed_then_the_retained_file_is_the_new_applications_and_identity_follows() {
    let served = Served::new();
    let (mut host, mut device, mut store) = served.boot();
    host.run(2, || 0).unwrap();
    let lines = assemble_lines(10);

    let responses = session(&mut host, &mut device, Some(&mut store), &as_strs(&lines));

    assert_eq!(responses[2]["response"], "ack");
    let expected = hash_hex(&container_bytes(&retained_source(10)));
    let image = stored_image(&served);
    assert_eq!(
        image.tag.content_hash,
        host.retained_image().tag.content_hash
    );
    assert_eq!(
        ironplc_container::integrity::hash_hex(&image.tag.content_hash).to_string(),
        expected
    );
    assert_eq!(
        identity(&mut host, &mut device)["persistedContentHash"],
        expected
    );
}

#[spec_test(REQ_VC_vm_cli_039)]
#[test]
fn assemble_when_the_retained_file_cannot_be_written_then_v6018_and_the_device_keeps_serving() {
    let served = Served::new();
    let (mut host, mut device, mut store) = served.boot();
    fs::create_dir(served.retained_tmp()).unwrap();
    let mut lines = assemble_lines(10);
    lines.push(r#"{"command":"getStatus"}"#.to_string());

    let responses = session(&mut host, &mut device, Some(&mut store), &as_strs(&lines));

    assert_eq!(responses[2]["response"], "error");
    assert_eq!(responses[2]["vCode"], "V6018");
    assert_eq!(responses[3]["response"], "status");
    assert_eq!(host.status().normal.raw(), 2);
    // The slot store committed before the retained file was written.
    assert_eq!(
        identity(&mut host, &mut device)["persistedContentHash"],
        hash_hex(&container_bytes(&retained_source(10)))
    );
}

#[spec_test(REQ_VC_vm_cli_040)]
#[test]
fn assemble_when_the_slot_store_fails_then_v6012_and_identity_shows_the_application_it_would_boot()
{
    let served = Served::new();
    let (mut host, mut device, mut store) = served.boot();
    fs::create_dir(suffix(&served.file, "tmp")).unwrap();
    let lines = assemble_lines(10);

    let responses = session(&mut host, &mut device, Some(&mut store), &as_strs(&lines));
    let answer = identity(&mut host, &mut device);

    assert_eq!(responses[2]["vCode"], "V6012");
    assert_eq!(
        answer["application"]["contentHash"],
        hash_hex(&container_bytes(&retained_source(10)))
    );
    assert_eq!(
        answer["persistedContentHash"],
        hash_hex(&container_bytes(&retained_source(1)))
    );
}

#[spec_test(REQ_VC_vm_cli_038)]
#[test]
fn stop_when_the_commit_did_not_reach_the_slot_store_then_the_new_applications_values_are_not_written(
) {
    let served = Served::new();
    let (mut host, mut device, mut store) = served.boot();
    fs::create_dir(suffix(&served.file, "tmp")).unwrap();
    let lines = assemble_lines(10);
    session(&mut host, &mut device, Some(&mut store), &as_strs(&lines));

    stop_device(&mut device, &host, Ok(())).unwrap();

    assert!(!served.retained().exists());
}

#[spec_test(REQ_VC_vm_cli_038)]
#[test]
fn stop_when_a_candidate_is_under_test_then_nothing_is_written() {
    let served = Served::new();
    let (mut host, mut device, mut store) = served.boot();
    let lines = assemble_lines(10);
    session(
        &mut host,
        &mut device,
        Some(&mut store),
        &as_strs(&lines[..2]),
    );

    stop_device(&mut device, &host, Ok(())).unwrap();

    assert!(!served.retained().exists());
}

#[spec_test(REQ_VC_vm_cli_039)]
#[test]
fn stop_when_the_retained_file_cannot_be_written_then_v6018() {
    let served = Served::new();
    let (host, mut device, _store) = served.boot();
    fs::create_dir(served.retained_tmp()).unwrap();

    let err = stop_device(&mut device, &host, Ok(())).unwrap_err();

    assert_eq!(err.v_code(), "V6018");
}

#[spec_test(REQ_VC_vm_cli_039)]
#[test]
fn stop_when_the_session_failed_then_the_session_error_is_the_one_reported() {
    let served = Served::new();
    let (host, mut device, _store) = served.boot();
    fs::create_dir(served.retained_tmp()).unwrap();
    let failed = Err(crate::error::VmError::io(
        error::SESSION_IO,
        "session".to_string(),
    ));

    let err = stop_device(&mut device, &host, failed).unwrap_err();

    assert_eq!(err.v_code(), "V6011");
}

/// A clock that a test advances, and the closure the retained file reads it
/// through.
fn test_clock() -> (Arc<AtomicU64>, Box<dyn Fn() -> u64 + Send>) {
    let now = Arc::new(AtomicU64::new(0));
    let reader = Arc::clone(&now);
    (now, Box::new(move || reader.load(Ordering::SeqCst)))
}

/// Sets the `retainedSavePeriod` of the device the way the command does.
fn set_period(device: &mut Device, seconds: u64) {
    device
        .change(&crate::device_settings::Entries::from([(
            "retainedSavePeriod".to_string(),
            serde_json::json!(seconds),
        )]))
        .unwrap();
}

#[spec_test(REQ_VC_vm_cli_038)]
#[test]
fn scan_boundary_when_the_period_is_on_then_a_changed_image_is_written_once_per_period() {
    let served = Served::new();
    let (mut host, mut device, _store) = served.boot();
    let (now, clock) = test_clock();
    device.use_retained_clock(clock);
    set_period(&mut device, 10);
    host.run(1, || 0).unwrap();

    // Before the period has passed nothing is written.
    now.store(9_999, Ordering::SeqCst);
    device.scan_boundary(&host);
    assert!(!served.retained().exists());

    // The period has passed and the image changed: one write.
    now.store(10_000, Ordering::SeqCst);
    device.scan_boundary(&host);
    assert_eq!(stored_image(&served).vars, host.retained_image().vars);

    // Another change inside the next period is not written yet.
    host.run(1, || 0).unwrap();
    now.store(19_999, Ordering::SeqCst);
    device.scan_boundary(&host);
    assert_eq!(stored_image(&served).vars[0], 1);

    // And is written when that period has passed.
    now.store(20_000, Ordering::SeqCst);
    device.scan_boundary(&host);
    assert_eq!(stored_image(&served).vars[0], 2);
}

#[test]
fn scan_boundary_when_the_period_has_passed_and_the_image_is_unchanged_then_nothing_is_written() {
    let served = Served::new();
    let (mut host, mut device, _store) = served.boot();
    let (now, clock) = test_clock();
    device.use_retained_clock(clock);
    set_period(&mut device, 10);
    host.run(1, || 0).unwrap();
    now.store(10_000, Ordering::SeqCst);
    device.scan_boundary(&host);
    fs::remove_file(served.retained()).unwrap();

    now.store(60_000, Ordering::SeqCst);
    device.scan_boundary(&host);

    assert!(!served.retained().exists());
}

#[spec_test(REQ_VC_vm_cli_038)]
#[test]
fn scan_boundary_when_the_period_is_off_then_nothing_is_ever_written() {
    let served = Served::new();
    let (mut host, mut device, _store) = served.boot();
    let (now, clock) = test_clock();
    device.use_retained_clock(clock);
    host.run(1, || 0).unwrap();

    now.store(86_400_000, Ordering::SeqCst);
    device.scan_boundary(&host);

    assert!(!served.retained().exists());
}

#[spec_test(REQ_VC_vm_cli_039)]
#[test]
fn scan_boundary_when_the_write_fails_then_the_device_goes_on_and_the_next_period_writes() {
    let served = Served::new();
    let (mut host, mut device, _store) = served.boot();
    let (now, clock) = test_clock();
    device.use_retained_clock(clock);
    set_period(&mut device, 10);
    host.run(1, || 0).unwrap();
    fs::create_dir(served.retained_tmp()).unwrap();
    now.store(10_000, Ordering::SeqCst);

    device.scan_boundary(&host);
    assert!(!served.retained().exists());

    fs::remove_dir(served.retained_tmp()).unwrap();
    now.store(10_001, Ordering::SeqCst);
    device.scan_boundary(&host);
    assert!(!served.retained().exists());
    now.store(20_000, Ordering::SeqCst);
    device.scan_boundary(&host);
    assert!(served.retained().exists());
}

#[test]
fn set_device_settings_when_the_period_is_set_then_it_applies_to_the_next_boundary() {
    let served = Served::new();
    let (mut host, mut device, _store) = served.boot();
    let (now, clock) = test_clock();
    device.use_retained_clock(clock);
    host.run(1, || 0).unwrap();
    let responses = session(
        &mut host,
        &mut device,
        None,
        &[r#"{"command":"setDeviceSettings","retainedSavePeriod":5}"#],
    );

    now.store(5_000, Ordering::SeqCst);
    device.scan_boundary(&host);

    assert_eq!(responses[0]["response"], "ack");
    assert!(served.retained().exists());
}
