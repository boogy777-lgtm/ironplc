//! Two-instance loopback scenarios for the crossload pipeline, over the
//! production [`PairLink`].
//!
//! Both units of one pair run their per-unit composition — the pair link
//! over the loopback simulator binding beside a real [`RuntimeHost`] —
//! the same composition `ironplcvm serve` runs in pair mode, minus the
//! session. The test driver owns the hosts, drives the owner's scan
//! rounds through the scan-commit seam, and enqueues the pair-lifecycle
//! notices the session enqueues in production (offer at Accept,
//! state update at Test, cancel/untest/assemble notices).
//!
//! The scenarios are the ADR-0064 pair pipeline: Accept crossloads the
//! candidate and the state snapshot to the Secondary, which reaches
//! SYNC_READY with the identical candidate generation and snapshot
//! bytes; a takeover during Testing executes the CANDIDATE (never a
//! revert to Original — that is an untest, which migration candidates
//! forbid, V4011); Assemble is the one commit transaction across the
//! pair; a garbled transfer keeps the Secondary unsynchronized with a
//! latched alarm while the Primary learns the coded refusal; cancel and
//! untest mirror across the pair.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use std::collections::BTreeMap;
use std::io::Cursor;

use ironplc_codegen::EmptyLookup;
use ironplc_container::{Container, VarIndex};
use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use ironplc_project::{compile, MemoryBackedProject, SidecarKey};
use ironplc_redundancy::{
    encode, loopback_pair, package_offer, permit_for, AdmissionVerdict, ConfiguredRole,
    CrossloadMessage, CrossloadReadiness, CrossloadRefusal, LoopbackPort, NicPort, PairId,
    PairLink, RedundancyConfig, SyncState,
};
use ironplc_runtime::{AcceptedEdit, HostMode, OnlineChangeError, RuntimeHost};

/// Compiles `source` and round-trips the container through the wire
/// format, so the hosts boot from the same bytes a deployed unit gets
/// (the runtime test-support convention).
fn compile_source(source: &str) -> Container {
    compile_container(source, &[])
}

/// Compiles `source` with the engineering-side program-variable
/// `(name, uid)` table (the migration fixture convention).
fn compile_with_ids(source: &str, ids: &[(&str, u64)]) -> Container {
    let keys: Vec<(SidecarKey, u64)> = ids
        .iter()
        .map(|(name, uid)| (SidecarKey::new("main", name), *uid))
        .collect();
    compile_container(source, &keys)
}

fn compile_container(source: &str, ids: &[(SidecarKey, u64)]) -> Container {
    let mut project = MemoryBackedProject::new(CompilerOptions::default());
    project.add_source(FileId::from_string("main.st"), source.to_owned());
    project.set_stable_var_ids(ids.to_vec());

    let output = compile(
        &mut project,
        &CompilerOptions::default(),
        &EmptyLookup,
        vec![],
    );
    assert!(
        output.diagnostics.is_empty(),
        "fixture must compile cleanly: {:?}",
        output.diagnostics
    );

    let container = output.container.unwrap();
    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    Container::read_from(&mut Cursor::new(&bytes)).unwrap()
}

/// Serializes a container to its wire-format bytes, the form an
/// `acceptEdits` command and a crossload offer carry.
fn container_bytes(container: &Container) -> Vec<u8> {
    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    bytes
}

/// Finds the variable table index of a named variable via the debug
/// section.
fn variable_index(container: &Container, name: &str) -> VarIndex {
    container
        .debug_section
        .as_ref()
        .unwrap()
        .var_names
        .iter()
        .find(|entry| entry.name.eq_ignore_ascii_case(name))
        .unwrap()
        .var_index
}

/// A `PROGRAM main` with one DINT `Counter` and the given body.
fn counter_program(body: &str) -> String {
    format!(
        "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  {body}
END_PROGRAM
"
    )
}

/// The migration fixture: adding a declared variable changes the layout
/// hash; the shared UID carries the value across.
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

/// One unit of a pair under test: the production pair link over the
/// loopback binding beside its runtime host. Admission is discovered
/// over the wire; the driver owns the host and its scan rounds.
struct Unit {
    link: PairLink<LoopbackPort>,
    host: RuntimeHost,
    /// Whether the scan driver runs this unit's rounds — pausing the
    /// Primary freezes its state while a replication image is in flight,
    /// so the scenarios assert byte equality against a stable producer.
    drive_scans: bool,
}

impl Unit {
    /// Boots one unit of the configured pair over its link port.
    fn boot(config: RedundancyConfig, port: LoopbackPort, host: RuntimeHost) -> Self {
        Self {
            link: PairLink::new(config, port),
            host,
            drive_scans: true,
        }
    }

    fn pair_id(&self) -> PairId {
        self.link.pair_id()
    }

    /// Whether the unit promoted itself (the takeover policy fired on a
    /// confirmed peer death).
    fn promoted(&self) -> bool {
        self.link.local_verdict() == Some(AdmissionVerdict::Primary)
            && self.link.config().role == ConfiguredRole::Secondary
    }
}

/// Reconciles the host's permit with the link's verdict and drives one
/// owner round through the scan-commit seam — exactly what the serve
/// session does after each pair tick and command.
fn drive_owner_round(unit: &mut Unit) {
    if let Some(verdict) = unit.link.local_verdict() {
        permit_for(&mut unit.host, verdict);
        if verdict.permits_execution() && unit.drive_scans {
            unit.host
                .run_with_commit(1, || 0, |commit| unit.link.on_scan_commit(commit))
                .unwrap();
        }
    }
}

/// Steps both units with their receive phases together, then drives the
/// owners' rounds.
fn step_pair(a: &mut Unit, b: &mut Unit) {
    a.link.tick(&mut a.host);
    b.link.tick(&mut b.host);
    drive_owner_round(a);
    drive_owner_round(b);
}

/// Steps both units without driving any scan round (the driver pauses
/// both producers while a replication image is in flight).
fn step_pair_idle(a: &mut Unit, b: &mut Unit) {
    let a_drive = a.drive_scans;
    let b_drive = b.drive_scans;
    a.drive_scans = false;
    b.drive_scans = false;
    step_pair(a, b);
    a.drive_scans = a_drive;
    b.drive_scans = b_drive;
}

/// Steps until the predicate holds or the step budget runs out (peer
/// death confirmation and promotion detection are exchange-count
/// events; the budget bounds them).
fn step_pair_until(a: &mut Unit, b: &mut Unit, mut predicate: impl FnMut(&Unit, &Unit) -> bool) {
    let mut satisfied = predicate(a, b);
    for _ in 0..12 {
        if satisfied {
            return;
        }
        step_pair_idle(a, b);
        satisfied = predicate(a, b);
    }
    assert!(satisfied, "predicate did not hold within the step budget");
}

fn pair_config(role: ConfiguredRole) -> RedundancyConfig {
    RedundancyConfig::pair(PairId::new(1), role)
        .with_confirmation_exchanges(2)
        .with_missed_exchanges(2)
}

/// Boots a pair over a loopback link, both over the same base
/// application, and converges it: the owner's boot replication stream
/// carries the initial image, and both units reach SYNC_READY — the
/// owner after four driven rounds.
fn boot_pair(base: &Container) -> (Unit, Unit) {
    let boot_host = || {
        let mut bytes = Vec::new();
        base.write_to(&mut bytes).unwrap();
        RuntimeHost::new(Container::read_from(&mut Cursor::new(&bytes)).unwrap()).unwrap()
    };
    let (port_a, port_b) = loopback_pair();
    let mut a = Unit::boot(pair_config(ConfiguredRole::Primary), port_a, boot_host());
    let mut b = Unit::boot(pair_config(ConfiguredRole::Secondary), port_b, boot_host());
    // Six exchanges: the verdict lands on the third and the owner drives
    // four rounds (steps 3..=6); the boot replication image has crossed
    // and both units are SYNC_READY.
    for _ in 0..6 {
        step_pair(&mut a, &mut b);
    }
    (a, b)
}

/// Stages `candidate` on the Primary and crossloads it to the Secondary,
/// driving the pair until the acceptance answer returns. Returns the
/// offered generation.
fn crossload_candidate(a: &mut Unit, b: &mut Unit, candidate: Container) -> u32 {
    let wire = container_bytes(&candidate);
    a.host
        .stage_with_decisions(
            candidate,
            &BTreeMap::new(),
            Some(AcceptedEdit {
                wire: wire.clone(),
                name: None,
                origin: None,
            }),
        )
        .unwrap();
    let offer = package_offer(a.pair_id(), a.link.epoch(), &a.host, wire).unwrap();
    let generation = offer.generation.raw();
    a.link.enqueue(CrossloadMessage::Offer(offer));
    for _ in 0..3 {
        step_pair(a, b);
    }
    generation
}

#[test]
fn crossload_when_primary_accepts_then_secondary_holds_identical_generation_and_state() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let (mut a, mut b) = boot_pair(&base);
    // The boot replication is real: the Secondary applied the owner's
    // initial image and both units report SYNC_READY. (The monitor's
    // image is necessarily one exchange behind the executing owner; the
    // byte-equality contract is asserted on the frozen offer below.)
    assert_eq!(b.link.chart().state(), SyncState::SyncReady);
    assert_eq!(b.link.receiver().readiness(), CrossloadReadiness::Complete);

    // Accept on the Primary: the offer carries the candidate wire
    // bytes, the snapshot, the candidate generation, and the epoch.
    a.drive_scans = false;
    let candidate = compile_source(&counter_program("Counter := Counter + 2;"));
    let wire = container_bytes(&candidate);
    a.host
        .stage_with_decisions(
            candidate,
            &BTreeMap::new(),
            Some(AcceptedEdit {
                wire: wire.clone(),
                name: None,
                origin: None,
            }),
        )
        .unwrap();
    let offer = package_offer(a.pair_id(), a.link.epoch(), &a.host, wire).unwrap();
    let generation = offer.generation;
    a.link.enqueue(CrossloadMessage::Offer(offer.clone()));
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }

    // ADR-0064(c): the same candidate generation on both units, the
    // replicated snapshot applied byte-identically, and the pair
    // reports SYNC — the Secondary is redundancy-ready.
    assert_eq!(b.link.receiver().readiness(), CrossloadReadiness::Complete);
    assert_eq!(b.link.receiver().alarm(), None);
    assert_eq!(b.link.chart().state(), SyncState::SyncReady);
    assert_eq!(b.host.status().candidate, Some(generation));
    assert_eq!(b.host.state_snapshot(), offer.snapshot);
}

#[test]
fn crossload_when_takeover_during_testing_of_migration_candidate_then_survivor_executes_candidate()
{
    // ADR-0064(e) with a schema-changing candidate: the Primary's Test
    // moves the state under the candidate; the replicated advance moves
    // it on the Secondary too; the takeover executes the CANDIDATE and
    // untest stays refused (V4011).
    let base = compile_with_ids(MIGRATION_BASE, &[("A", 1)]);
    let candidate = compile_with_ids(MIGRATION_CANDIDATE, &[("A", 1), ("B", 2)]);
    let a_index = variable_index(&base, "A");
    let (mut a, mut b) = boot_pair(&base);
    // Freeze the producer's state so the replicated image is asserted
    // byte-for-byte.
    a.drive_scans = false;

    crossload_candidate(&mut a, &mut b, candidate);
    assert_eq!(b.link.chart().state(), SyncState::SyncReady);
    assert_eq!(b.host.status().mode, HostMode::Normal);
    assert_eq!(b.host.read_variable(a_index).unwrap(), 4);

    // The Primary tests: the migration moves the state under the
    // candidate (A carried, B initialized), and the replication stream
    // carries the candidate-layout image to the Secondary.
    a.host.test().unwrap();
    a.host.run(1, || 0).unwrap();
    a.link
        .enqueue(CrossloadMessage::StateUpdate(a.host.state_snapshot()));
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(a.host.status().mode, HostMode::Testing);
    assert_eq!(b.host.status().mode, HostMode::Testing);
    assert_eq!(b.host.state_snapshot(), a.host.state_snapshot());
    assert_eq!(b.host.read_variable(a_index).unwrap(), 5);
    assert!(matches!(
        b.host.untest(),
        Err(OnlineChangeError::UntestUnsupported)
    ));

    // The Primary dies; the survivor detects the death and takes over
    // executing the CANDIDATE, already under Test.
    a.link.port_mut().set_partitioned(true);
    step_pair_until(&mut a, &mut b, |_, b| {
        b.link.liveness().is_dead() && b.promoted()
    });
    assert_eq!(b.link.chart().state(), SyncState::DeSync);

    // The promoted survivor executes the candidate: one driven round
    // advances A once more under the candidate layout.
    drive_owner_round(&mut b);
    assert_eq!(b.host.status().mode, HostMode::Testing);
    assert_eq!(b.host.read_variable(a_index).unwrap(), 6);
    // The pair remains in Testing until assemble or cancel: the
    // candidate is kept and untest stays refused.
    assert!(b.host.status().candidate.is_some());
    assert!(matches!(
        b.host.untest(),
        Err(OnlineChangeError::UntestUnsupported)
    ));
}

#[test]
fn crossload_when_takeover_during_testing_of_exact_match_candidate_then_survivor_flips_and_runs() {
    // ADR-0064(e) with a layout-preserving candidate: both layouts hash
    // identically, so the replicated stream never shows the Test — the
    // survivor's takeover flips the selector without a swap and runs the
    // candidate's code.
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let counter = variable_index(&base, "Counter");
    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    let (mut a, mut b) = boot_pair(&base);
    a.drive_scans = false;

    crossload_candidate(&mut a, &mut b, candidate);
    assert_eq!(b.host.read_variable(counter).unwrap(), 4);
    a.host.test().unwrap();
    a.host.run(1, || 0).unwrap();
    a.link
        .enqueue(CrossloadMessage::StateUpdate(a.host.state_snapshot()));
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(a.host.read_variable(counter).unwrap(), 14);
    assert_eq!(b.host.read_variable(counter).unwrap(), 14);
    assert_eq!(b.host.status().mode, HostMode::Normal);

    a.link.port_mut().set_partitioned(true);
    step_pair_until(&mut a, &mut b, |_, b| b.promoted());
    // The takeover flipped the selector to the CANDIDATE without a
    // swap: the state has moved under the candidate on the pair, and
    // the survivor's replicated image is current.
    assert_eq!(b.host.status().mode, HostMode::Testing);
    drive_owner_round(&mut b);
    // 14 (replicated) + 10 (the candidate's step), never + 1.
    assert_eq!(b.host.read_variable(counter).unwrap(), 24);
}

#[test]
fn crossload_when_pair_assembles_then_both_promote_one_generation() {
    // ADR-0064(h): Assemble is one transaction across the pair — the
    // owner's commit carries the AssembleCandidate notice and the
    // standby promotes the candidate in the same generation step, so
    // neither unit holds a different canonical generation.
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let candidate = compile_source(&counter_program("Counter := Counter + 2;"));
    let (mut a, mut b) = boot_pair(&base);
    a.drive_scans = false;

    crossload_candidate(&mut a, &mut b, candidate);
    assert_eq!(b.host.status().candidate, a.host.status().candidate);

    // The pair's Test: the owner runs the candidate under Test. The
    // layout-preserving Test is invisible on the replication stream, so
    // the Secondary's selector stays Original until the commit notice.
    a.host.test().unwrap();
    a.host.run(1, || 0).unwrap();
    a.link
        .enqueue(CrossloadMessage::StateUpdate(a.host.state_snapshot()));
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(a.host.status().mode, HostMode::Testing);
    assert_eq!(b.host.status().mode, HostMode::Normal);

    // The commit: the owner assembles, then the notice promotes the
    // standby. Both hold the same canonical application generation, the
    // staged candidate is gone, and the pair stays synchronized.
    a.host.assemble().unwrap();
    a.link.enqueue(CrossloadMessage::AssembleCandidate);
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(a.host.status().mode, HostMode::Normal);
    assert_eq!(b.host.status().mode, HostMode::Normal);
    assert!(a.host.status().candidate.is_none());
    assert!(b.host.status().candidate.is_none());
    assert_eq!(a.host.status().application, b.host.status().application);
    assert_eq!(b.link.receiver().readiness(), CrossloadReadiness::Complete);
    assert_eq!(b.link.receiver().alarm(), None);
    assert_eq!(b.link.chart().state(), SyncState::SyncReady);
    // The owner learned nothing refused: the notice was accepted.
    assert_eq!(a.link.notified(), None);
}

#[test]
fn crossload_when_transfer_garbled_then_secondary_stays_unsynced_and_primary_notified() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let (mut a, mut b) = boot_pair(&base);
    // One edit synchronizes normally: the pair is redundancy-ready.
    a.drive_scans = false;
    let first = compile_source(&counter_program("Counter := Counter + 2;"));
    crossload_candidate(&mut a, &mut b, first);
    assert_eq!(b.link.chart().state(), SyncState::SyncReady);

    // The next edit's offer crosses garbled: the CRC fails mid-payload.
    // Cancel the first candidate so the Primary may stage the next.
    a.host.cancel().unwrap();
    a.link.enqueue(CrossloadMessage::CancelCandidate);
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(b.link.chart().state(), SyncState::SyncReady);

    let second = compile_source(&counter_program("Counter := Counter + 3;"));
    let wire = container_bytes(&second);
    a.host
        .stage_with_decisions(
            second,
            &BTreeMap::new(),
            Some(AcceptedEdit {
                wire: wire.clone(),
                name: None,
                origin: None,
            }),
        )
        .unwrap();
    let offer = package_offer(a.pair_id(), a.link.epoch(), &a.host, wire).unwrap();
    let mut frame = encode(&CrossloadMessage::Offer(offer));
    let middle = frame.len() / 2;
    frame[middle] ^= 0xFF;
    a.link.port_mut().send(&frame).unwrap();
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }

    // The Secondary never pretends redundancy-ready: the readiness stays
    // down, the chart drops out of SYNC_READY and re-enters SYNCING per
    // the readiness policy, the alarm latches with the coded refusal,
    // and the pair link answers it — the Primary is notified.
    assert_eq!(
        b.link.receiver().readiness(),
        CrossloadReadiness::InProgress
    );
    assert_eq!(
        b.link.receiver().alarm(),
        Some(CrossloadRefusal::Interrupted)
    );
    assert_eq!(b.link.receiver().alarm().unwrap().v_code(), "V4104");
    assert_eq!(b.link.chart().state(), SyncState::Syncing);
    assert!(b.host.status().candidate.is_none());
    assert_eq!(a.link.notified(), Some(CrossloadRefusal::Interrupted));
}

#[test]
fn crossload_when_pair_cancels_then_both_drop_the_candidate() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let (mut a, mut b) = boot_pair(&base);
    a.drive_scans = false;

    let candidate = compile_source(&counter_program("Counter := Counter + 2;"));
    crossload_candidate(&mut a, &mut b, candidate);
    assert!(a.host.status().candidate.is_some());
    assert!(b.host.status().candidate.is_some());

    // ADR-0064(g): Cancel drops the candidate on both units from
    // exec = Original.
    a.host.cancel().unwrap();
    a.link.enqueue(CrossloadMessage::CancelCandidate);
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }

    assert!(a.host.status().candidate.is_none());
    assert!(b.host.status().candidate.is_none());
    assert_eq!(b.link.receiver().alarm(), None);
}

#[test]
fn crossload_when_pair_untests_then_selector_returns_and_candidate_is_kept() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let counter = variable_index(&base, "Counter");
    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    let (mut a, mut b) = boot_pair(&base);
    a.drive_scans = false;

    crossload_candidate(&mut a, &mut b, candidate);
    assert_eq!(b.host.read_variable(counter).unwrap(), 4);

    // The Primary tests; the layout-preserving Test is invisible on the
    // replication stream, so the Secondary's selector stays Original.
    a.host.test().unwrap();
    a.host.run(1, || 0).unwrap();
    a.link
        .enqueue(CrossloadMessage::StateUpdate(a.host.state_snapshot()));
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(a.host.status().mode, HostMode::Testing);
    assert_eq!(b.host.status().mode, HostMode::Normal);
    assert_eq!(b.host.read_variable(counter).unwrap(), 14);

    // ADR-0064(f): Untest switches the selector back on both units; the
    // candidate is kept. The Secondary never mirrored the Test, so the
    // notice is a no-op there — but it answers, and the pair agrees.
    a.link.enqueue(CrossloadMessage::UntestCandidate);
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }
    a.host.untest().unwrap();
    a.host.run(1, || 0).unwrap();
    assert_eq!(a.host.status().mode, HostMode::Normal);
    assert_eq!(b.host.status().mode, HostMode::Normal);
    assert!(a.host.status().candidate.is_some());
    assert!(b.host.status().candidate.is_some());
    // The state was carried, never rolled back: the Secondary holds the
    // last replicated image; the Primary advanced one round under the
    // original after the boundary.
    assert_eq!(b.host.read_variable(counter).unwrap(), 14);
    assert_eq!(a.host.read_variable(counter).unwrap(), 15);
    assert_eq!(b.link.receiver().alarm(), None);
}
