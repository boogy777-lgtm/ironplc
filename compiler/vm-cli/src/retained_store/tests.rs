//! Tests of the retained file: what it reads, how it writes, and when the
//! periodic write is due. The boot and the write points of the device are in
//! `serve_retained_tests`.

use std::fs;
use std::sync::atomic::{AtomicU64, Ordering};
use std::sync::Arc;

use tempfile::TempDir;

use super::*;
use crate::serve_retained_tests::host_after;

/// A store in a temporary directory whose clock the test sets.
fn store_with_clock(dir: &TempDir) -> (RetainedStore, Arc<AtomicU64>) {
    let now = Arc::new(AtomicU64::new(0));
    let reader = Arc::clone(&now);
    let store = RetainedStore::beside(&dir.path().join("app.iplc"))
        .with_clock(Box::new(move || reader.load(Ordering::SeqCst)));
    (store, now)
}

fn store_in(dir: &TempDir) -> RetainedStore {
    store_with_clock(dir).0
}

#[test]
fn beside_when_served_file_then_names_follow_the_suffix_helper() {
    let store = RetainedStore::beside(std::path::Path::new("dir/app.iplc"));

    assert_eq!(store.path, std::path::Path::new("dir/app.iplc.retained"));
    assert_eq!(
        store.temporary,
        std::path::Path::new("dir/app.iplc.retained.tmp")
    );
}

#[test]
fn save_when_image_then_load_returns_the_same_image_and_no_temporary_remains() {
    let dir = TempDir::new().unwrap();
    let mut store = store_in(&dir);
    let image = host_after(1, 3).retained_image();

    store.save(&image).unwrap();

    assert_eq!(store.load(), StoredItem::Found(image));
    assert!(!store.temporary.exists());
}

#[test]
fn load_when_no_file_then_absent() {
    let dir = TempDir::new().unwrap();

    assert_eq!(store_in(&dir).load(), StoredItem::Absent);
}

#[test]
fn load_when_the_bytes_are_not_an_image_then_damaged() {
    let dir = TempDir::new().unwrap();
    let store = store_in(&dir);
    fs::write(&store.path, b"IPRX").unwrap();

    assert_eq!(store.load(), StoredItem::Damaged);
}

#[test]
fn load_when_the_path_cannot_be_read_then_damaged() {
    let dir = TempDir::new().unwrap();
    let store = store_in(&dir);
    fs::create_dir(&store.path).unwrap();

    assert_eq!(store.load(), StoredItem::Damaged);
}

#[test]
fn save_when_the_temporary_path_is_blocked_then_v6018_and_the_old_image_stays() {
    let dir = TempDir::new().unwrap();
    let mut store = store_in(&dir);
    let old = host_after(1, 1).retained_image();
    store.save(&old).unwrap();
    fs::create_dir(&store.temporary).unwrap();

    let err = store.save(&host_after(1, 2).retained_image()).unwrap_err();

    assert_eq!(err.v_code(), "V6018");
    assert_eq!(store.load(), StoredItem::Found(old));
}

#[test]
fn save_host_when_the_file_holds_an_equal_image_then_it_does_not_write() {
    let dir = TempDir::new().unwrap();
    let mut store = store_in(&dir);
    let host = host_after(1, 2);
    store.save_host(&host).unwrap();
    fs::remove_file(&store.path).unwrap();

    let wrote = store.save_host(&host).unwrap();

    assert!(!wrote);
    assert!(!store.path.exists());
}

#[test]
fn save_host_when_the_image_changed_then_it_writes() {
    let dir = TempDir::new().unwrap();
    let mut store = store_in(&dir);
    store.save_host(&host_after(1, 2)).unwrap();

    let wrote = store.save_host(&host_after(1, 3)).unwrap();

    assert!(wrote);
}

#[test]
fn save_when_due_when_the_period_is_zero_then_never_writes() {
    let dir = TempDir::new().unwrap();
    let (mut store, now) = store_with_clock(&dir);
    now.store(u64::MAX, Ordering::SeqCst);

    let wrote = store.save_when_due(&host_after(1, 1), 0).unwrap();

    assert!(!wrote);
    assert!(!store.path.exists());
}

#[test]
fn save_when_due_when_the_period_has_not_passed_then_does_not_write() {
    let dir = TempDir::new().unwrap();
    let (mut store, now) = store_with_clock(&dir);
    now.store(59_999, Ordering::SeqCst);

    let wrote = store.save_when_due(&host_after(1, 1), 60).unwrap();

    assert!(!wrote);
}

#[test]
fn save_when_due_when_the_period_has_passed_then_writes_and_restarts_the_period() {
    let dir = TempDir::new().unwrap();
    let (mut store, now) = store_with_clock(&dir);
    now.store(60_000, Ordering::SeqCst);

    let first = store.save_when_due(&host_after(1, 1), 60).unwrap();
    now.store(119_999, Ordering::SeqCst);
    let second = store.save_when_due(&host_after(1, 2), 60).unwrap();

    assert!(first);
    assert!(!second);
}

#[test]
fn save_when_due_when_the_write_fails_then_v6018_and_the_period_restarts() {
    let dir = TempDir::new().unwrap();
    let (mut store, now) = store_with_clock(&dir);
    fs::create_dir(&store.temporary).unwrap();
    now.store(60_000, Ordering::SeqCst);
    let host = host_after(1, 1);

    let err = store.save_when_due(&host, 60).unwrap_err();
    now.store(60_001, Ordering::SeqCst);
    let retried = store.save_when_due(&host, 60).unwrap();

    assert_eq!(err.v_code(), "V6018");
    assert!(!retried);
}

#[test]
fn discard_reason_when_each_refusal_then_the_reason_of_the_class() {
    assert_eq!(
        discard_reason(RetainedApplyError::AfterFirstRound),
        DiscardReason::NotAtBoot
    );
    assert_eq!(
        discard_reason(RetainedApplyError::Mismatch(RetainedMismatch::ContentHash)),
        DiscardReason::OtherApplication
    );
    assert_eq!(
        discard_reason(RetainedApplyError::Mismatch(RetainedMismatch::Runs)),
        DiscardReason::OtherRetainedLayout
    );
    assert_eq!(
        discard_reason(RetainedApplyError::Mismatch(RetainedMismatch::Length)),
        DiscardReason::Damaged
    );
}

#[test]
fn boot_when_the_host_has_run_a_round_then_not_at_boot() {
    let dir = TempDir::new().unwrap();
    let mut store = store_in(&dir);
    let mut host = host_after(1, 1);
    store.save(&host.retained_image()).unwrap();

    let result = store.boot(&mut host);

    assert_eq!(result, BootResult::Discarded(DiscardReason::NotAtBoot));
}
