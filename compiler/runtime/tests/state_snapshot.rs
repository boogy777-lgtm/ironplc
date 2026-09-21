//! Acceptance tests for the state snapshot seams (HA crossload).
//!
//! Seam 3 (`RuntimeHost::state_snapshot`) exports the persistent regions
//! in the carry-over vocabulary of `swap_buffers`; seam 4
//! (`RuntimeHost::apply_state_snapshot`) writes a replicated image into an
//! idle host, fail-closed on every mismatch; `takeover_testing` and
//! `apply_pending_at_boundary` carry the ADR-0064(e) takeover and the
//! pair-mirror Untest. Fixtures are compiled with the real project
//! pipeline and round-tripped through the wire format, so the layout
//! hashes are the values a deployed artifact carries.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

mod common;

use common::{compile_source, compile_with_ids, counter_host, counter_program, variable_index};
use ironplc_runtime::{CommandError, HostMode, OnlineChangeError, RuntimeHost, StateSnapshot};

/// The migration fixture: adding a declared variable changes the layout
/// hash; the shared UID carries the value across (the
/// `migration_acceptance.rs` pattern).
const MIGRATION_BASE: &str = "PROGRAM main
  VAR
    A : DINT;
  END_VAR
  A := A + 1;
END_PROGRAM
";

const MIGRATION_CANDIDATE: &str = "PROGRAM main
  VAR
    A : DINT;
    B : DINT;
  END_VAR
  A := A + 1;
END_PROGRAM
";

/// Re-exports the host's persistent state: the round-trip integrity
/// witness used across these tests.
fn persistent_snapshot(host: &RuntimeHost) -> StateSnapshot {
    host.state_snapshot()
}

/// The shared A index of the migration fixtures (deterministic across
/// compiles of the same source).
fn migration_a_index() -> ironplc_container::VarIndex {
    variable_index(&compile_with_ids(MIGRATION_BASE, &[("A", 1)]), "A")
}

#[test]
fn snapshot_when_exported_and_applied_to_idle_peer_then_persistent_bytes_identical() {
    let (mut primary, counter) = counter_host(1);
    primary.run(3, || 0).unwrap();
    let snapshot = primary.state_snapshot();
    assert_eq!(primary.read_variable(counter).unwrap(), 3);

    let mut secondary =
        RuntimeHost::new(compile_source(&counter_program("Counter := Counter + 1;"))).unwrap();
    secondary.apply_state_snapshot(&snapshot).unwrap();

    assert_eq!(secondary.read_variable(counter).unwrap(), 3);
    assert_eq!(persistent_snapshot(&secondary), snapshot);
}

#[test]
fn snapshot_when_lengths_differ_from_declared_layout_then_refused_v4019_without_touching_state() {
    let (mut primary, counter) = counter_host(1);
    primary.run(2, || 0).unwrap();
    let mut corrupt = primary.state_snapshot();
    corrupt.vars.push(0);

    let mut secondary =
        RuntimeHost::new(compile_source(&counter_program("Counter := Counter + 1;"))).unwrap();
    let error = secondary.apply_state_snapshot(&corrupt).unwrap_err();

    assert!(matches!(error, OnlineChangeError::SnapshotCorrupt));
    assert_eq!(CommandError::from(error).v_code(), "V4019");
    // Fail-closed: not one byte was applied.
    assert_eq!(secondary.read_variable(counter).unwrap(), 0);
}

#[test]
fn snapshot_when_layout_unknown_to_host_then_refused_v4007() {
    let (mut primary, _) = counter_host(1);
    primary.run(2, || 0).unwrap();
    let snapshot = primary.state_snapshot();

    // A different declaration (more variables) names a layout the
    // receiver does not hold.
    let other = compile_source(
        "PROGRAM main
  VAR
    X : DINT;
    Y : DINT;
  END_VAR
  X := X + 1;
END_PROGRAM
",
    );
    let mut secondary = RuntimeHost::new(other).unwrap();

    let error = secondary.apply_state_snapshot(&snapshot).unwrap_err();

    assert!(matches!(error, OnlineChangeError::LayoutIncompatible));
    assert_eq!(CommandError::from(error).v_code(), "V4007");
}

#[test]
fn advance_when_migration_test_image_arrives_then_replicated_state_is_authoritative() {
    // Primary: three rounds under the original, then Accept → Test of a
    // migration candidate; the migrated image (A carried, B initialized)
    // is what the pair replicates.
    let a = migration_a_index();
    let mut primary = RuntimeHost::new(compile_with_ids(MIGRATION_BASE, &[("A", 1)])).unwrap();
    primary.permit_execution();
    primary.run(3, || 0).unwrap();
    primary
        .stage(compile_with_ids(MIGRATION_CANDIDATE, &[("A", 1), ("B", 2)]))
        .unwrap();
    let pre_test = primary.state_snapshot();
    primary.test().unwrap();
    primary.run(1, || 0).unwrap();
    let post_test = primary.state_snapshot();

    // Secondary S1 mirrors through replication only: replicate the
    // pre-test image, stage the same candidate, then advance on the
    // post-test (candidate-layout) image.
    let mut mirrored = RuntimeHost::new(compile_with_ids(MIGRATION_BASE, &[("A", 1)])).unwrap();
    mirrored.apply_state_snapshot(&pre_test).unwrap();
    mirrored
        .stage(compile_with_ids(MIGRATION_CANDIDATE, &[("A", 1), ("B", 2)]))
        .unwrap();
    mirrored.apply_state_snapshot(&post_test).unwrap();

    assert_eq!(mirrored.status().mode, HostMode::Testing);
    assert_eq!(mirrored.read_variable(a).unwrap(), 4);
    assert_eq!(persistent_snapshot(&mirrored), post_test);
    // The migration marker survived the advance: untest stays refused
    // (V4011), assemble or cancel while the original is active.
    assert!(matches!(
        mirrored.untest(),
        Err(OnlineChangeError::UntestUnsupported)
    ));
    assert!(matches!(
        mirrored.apply_state_snapshot(&pre_test),
        Err(OnlineChangeError::UntestUnsupported)
    ));
    assert!(mirrored.takeover_testing().is_ok());

    // Secondary S2 runs the local migration instead (test + boundary +
    // round) and lands on exactly the same persistent bytes: the
    // replicated image is equivalent to the local carry-over.
    let mut local = RuntimeHost::new(compile_with_ids(MIGRATION_BASE, &[("A", 1)])).unwrap();
    local.permit_execution();
    local.apply_state_snapshot(&pre_test).unwrap();
    local
        .stage(compile_with_ids(MIGRATION_CANDIDATE, &[("A", 1), ("B", 2)]))
        .unwrap();
    local.test().unwrap();
    local.run(1, || 0).unwrap();

    assert_eq!(local.status().mode, HostMode::Testing);
    assert_eq!(persistent_snapshot(&local), post_test);
}

#[test]
fn takeover_when_exact_match_candidate_staged_then_flips_and_runs_candidate_code() {
    // ADR-0064(e) for a layout-preserving candidate: the survivor flips
    // to the candidate without a swap and executes the candidate's code.
    let (mut primary, counter) = counter_host(1);
    primary.run(2, || 0).unwrap();
    primary
        .stage(compile_source(&counter_program("Counter := Counter + 10;")))
        .unwrap();
    primary.test().unwrap();
    primary.run(1, || 0).unwrap();
    let snapshot = primary.state_snapshot();

    let mut survivor =
        RuntimeHost::new(compile_source(&counter_program("Counter := Counter + 1;"))).unwrap();
    survivor.apply_state_snapshot(&snapshot).unwrap();
    survivor
        .stage(compile_source(&counter_program("Counter := Counter + 10;")))
        .unwrap();

    survivor.takeover_testing().unwrap();
    assert_eq!(survivor.status().mode, HostMode::Testing);
    // Idempotent: a repeated takeover changes nothing.
    assert!(survivor.takeover_testing().is_ok());

    survivor.permit_execution();
    survivor.run(1, || 0).unwrap();
    // 12 (replicated: 2 rounds of +1, 1 round of +10) + 10 (the
    // candidate's step) — never + 1 (the original's step).
    assert_eq!(survivor.read_variable(counter).unwrap(), 22);
}

#[test]
fn takeover_when_migration_candidate_not_advanced_then_refused_never_a_guess() {
    let mut survivor = RuntimeHost::new(compile_with_ids(MIGRATION_BASE, &[("A", 1)])).unwrap();
    survivor
        .stage(compile_with_ids(MIGRATION_CANDIDATE, &[("A", 1), ("B", 2)]))
        .unwrap();

    // The state has not moved under this host: executing the candidate
    // over unmigrated state would be a guess.
    assert!(matches!(
        survivor.takeover_testing(),
        Err(OnlineChangeError::NotAllowedInThisMode)
    ));
    assert_eq!(survivor.status().mode, HostMode::Normal);

    // The refused takeover left the host untouched: cancel is still the
    // exit (ADR-0064: commit or cancel only).
    assert!(survivor.cancel().is_ok());
}

#[test]
fn idle_boundary_when_untest_mirrored_then_swaps_without_rounds_or_permit() {
    // The monitor-mode mirror of the pair's Untest: an idle, unpermitted
    // host applies its pending untest at an idle boundary — no scans are
    // driven, so the permit (which this unit never holds) is not the
    // gate; the selector and the carried state move exactly as at a scan
    // boundary and the candidate is kept.
    let (mut primary, _) = counter_host(1);
    primary.run(2, || 0).unwrap();
    primary
        .stage(compile_source(&counter_program("Counter := Counter + 10;")))
        .unwrap();
    primary.test().unwrap();
    primary.run(1, || 0).unwrap();
    let snapshot = primary.state_snapshot();

    let mut secondary =
        RuntimeHost::new(compile_source(&counter_program("Counter := Counter + 1;"))).unwrap();
    secondary.apply_state_snapshot(&snapshot).unwrap();
    secondary
        .stage(compile_source(&counter_program("Counter := Counter + 10;")))
        .unwrap();

    secondary.takeover_testing().unwrap();
    assert_eq!(secondary.status().mode, HostMode::Testing);

    secondary.untest().unwrap();
    secondary.apply_pending_at_boundary().unwrap();

    // ADR-0064(f): the selector switched back and the candidate is kept;
    // the process state was carried, never rolled back.
    assert_eq!(secondary.status().mode, HostMode::Normal);
    assert!(secondary.status().candidate.is_some());
    let counter = variable_index(
        &compile_source(&counter_program("Counter := Counter + 1;")),
        "Counter",
    );
    assert_eq!(secondary.read_variable(counter).unwrap(), 12);
}
