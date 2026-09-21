//! Two-instance loopback scenarios for the crossload pipeline.
//!
//! Both units of one pair run their per-unit composition over the
//! loopback simulator binding: link port, ping/pong liveness, SYNC
//! chart, epoch, the runtime host, and the crossload receiver. The node
//! is test support — it composes the crate's modules the way the
//! redundancy shell will, and the real shell replaces it when a binary
//! embeds the layer.
//!
//! The scenarios are the ADR-0064 pair pipeline: Accept crossloads the
//! candidate and the state snapshot to the Secondary, which reaches
//! SYNC_READY with the identical candidate generation and snapshot
//! bytes; a takeover during Testing executes the CANDIDATE (never a
//! revert to Original — that is an untest, which migration candidates
//! forbid, V4011); a garbled transfer keeps the Secondary unsynchronized
//! with a latched alarm while the Primary learns the coded refusal;
//! cancel and untest mirror across the pair.

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
    admit, decode, encode, loopback_pair, package_offer, permit_for, AdmissionVerdict,
    ConfiguredRole, CrossloadMessage, CrossloadReadiness, CrossloadReceiver, CrossloadRefusal,
    Discovery, Epoch, Liveness, LivenessEvent, LoopbackPort, NicPort, Packet, PairId, PairRole,
    RedundancyConfig, SyncChart, SyncEvent, SyncState, FRAME_LEN, FRAME_MAGIC,
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
/// `AcceptEdits` command and a crossload offer carry.
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

/// One unit of a pair under test: the per-unit composition of link
/// port, liveness exchange, epoch, SYNC chart, runtime host, and the
/// crossload receiver. Admission is fixed from the configured role — the
/// scenarios start from admitted units; discovery is the pair-link
/// suite's territory.
struct Node {
    config: RedundancyConfig,
    port: LoopbackPort,
    liveness: Liveness,
    chart: SyncChart,
    epoch: Epoch,
    host: RuntimeHost,
    receiver: CrossloadReceiver,
    /// Crossload messages awaiting the next transmit.
    outbound: Vec<CrossloadMessage>,
    /// The refusal this unit was notified with, if any (the Primary's
    /// "peer refused" surface).
    notified: Option<CrossloadRefusal>,
    /// A valid peer frame has been observed since the last peer death.
    paired: bool,
    /// The peer epoch is agreed while synchronizing.
    epoch_agreed: bool,
    /// Whether this unit drives scans (the Primary verdict).
    permitted: bool,
    /// Whether the scan driver runs this round — pausing the Primary
    /// freezes its state while a replication image is in flight, so the
    /// scenarios assert byte equality against a stable producer.
    drive_scans: bool,
}

impl Node {
    /// Boots one admitted unit over its link port: deSYNC with the boot
    /// reason, the host permitted exactly when the role is Primary.
    fn boot(config: RedundancyConfig, port: LoopbackPort, host: RuntimeHost) -> Self {
        let permitted = matches!(
            admit(config.role, Discovery::NoPeer),
            Ok(AdmissionVerdict::Primary)
        );
        let mut host = host;
        if permitted {
            permit_for(&mut host, AdmissionVerdict::Primary);
        }
        Self {
            liveness: Liveness::new(&config),
            port,
            config,
            chart: SyncChart::new(),
            epoch: Epoch::new(0),
            host,
            receiver: CrossloadReceiver::new(),
            outbound: Vec::new(),
            notified: None,
            paired: false,
            epoch_agreed: false,
            permitted,
            drive_scans: true,
        }
    }

    fn pair_id(&self) -> PairId {
        self.config.pair_id.unwrap()
    }

    /// The pair role this unit presents on the wire: the admitted
    /// ownership truth.
    fn pair_role(&self) -> PairRole {
        if self.permitted {
            PairRole::Primary
        } else {
            PairRole::Secondary
        }
    }

    fn generation(&self) -> u32 {
        self.host.status().application.raw()
    }

    /// Queues one crossload message for the next transmit.
    fn send(&mut self, message: CrossloadMessage) {
        self.outbound.push(message);
    }

    /// Models the promotion verdict of a takeover (the promotion
    /// mechanism — fencing, barrier — is a later slice): the survivor
    /// gains the permit through the same policy call admission uses.
    fn promote(&mut self) {
        self.permitted = true;
        permit_for(&mut self.host, AdmissionVerdict::Primary);
    }

    /// Drains the inbound link: crossload frames demux by magic, the
    /// fixed-size ping/pong frames feed liveness and the epoch.
    fn receive(&mut self) {
        while let Some((_, frame)) = self.port.poll() {
            if frame.starts_with(&FRAME_MAGIC) {
                self.receive_crossload(&frame);
                continue;
            }
            if frame.len() != FRAME_LEN {
                continue;
            }
            let Some(packet) = Packet::decode(&frame) else {
                continue;
            };
            self.paired = true;
            if let Some(LivenessEvent::PeerRestarted) = self.liveness.note_received(&packet) {
                // The peer rebooted and lost its epoch memory: an epoch
                // discontinuity (T13), the chart drops to deSYNC.
                self.chart.apply(SyncEvent::EpochDiscontinuity);
                self.epoch_agreed = false;
            } else {
                self.epoch.adopt(packet.epoch);
                if self.chart.state() == SyncState::Syncing {
                    self.epoch_agreed = true;
                }
            }
        }
    }

    /// The supervisor's deterministic reaction to one decoded crossload
    /// message; the coded answer is queued for the peer and a refused
    /// transfer drops the SYNC chart (readiness lost ⇒ deSYNC, the FSM
    /// spec's "SYNC_READY lost ⇒ deSYNC").
    fn receive_crossload(&mut self, frame: &[u8]) {
        let Some(message) = decode(frame) else {
            // A garbled transfer is dropped, never acted on; the latch
            // records the interruption and answers the coded refusal.
            let response = self.receiver.note_interrupted();
            self.chart.apply(SyncEvent::SyncLoss);
            self.outbound.push(response);
            return;
        };
        let pair_id = self.pair_id();
        let response = match message {
            CrossloadMessage::Offer(offer) => self.receiver.accept(pair_id, &mut self.host, &offer),
            CrossloadMessage::StateUpdate(snapshot) => {
                self.receiver.apply_update(&mut self.host, &snapshot)
            }
            CrossloadMessage::CancelCandidate => self.receiver.cancel_candidate(&mut self.host),
            CrossloadMessage::UntestCandidate => self.receiver.untest_candidate(&mut self.host),
            CrossloadMessage::Refused(refusal) => {
                self.notified = Some(refusal);
                return;
            }
            CrossloadMessage::Accepted => return,
        };
        if matches!(response, CrossloadMessage::Refused(_)) {
            self.chart.apply(SyncEvent::SyncLoss);
        }
        self.outbound.push(response);
    }

    /// Advances one ping/pong exchange and sends the outbound frames.
    fn transmit(&mut self) {
        let (ping_seq, pong_seq, event) = self.liveness.begin_exchange();
        if let Some(LivenessEvent::PeerDied) = event {
            self.chart.apply(SyncEvent::PeerDied);
            self.paired = false;
            self.epoch_agreed = false;
        }
        let packet = Packet {
            pair_id: self.pair_id(),
            role: self.pair_role(),
            epoch: self.epoch,
            generation: self.generation(),
            ping_seq,
            pong_seq,
        };
        self.port.send(&packet.encode()).unwrap();
        for message in self.outbound.drain(..) {
            self.port.send(&encode(&message)).unwrap();
        }
    }

    /// Feeds the chart's guards and — when the verdict permits and the
    /// driver is not paused — drives one scan round whose committed
    /// boundary mints the next epoch.
    fn advance(&mut self) {
        if self.chart.state() == SyncState::DeSync && self.paired {
            self.chart.apply(SyncEvent::Paired);
        }
        if self.chart.state() == SyncState::Syncing
            && self.epoch_agreed
            && self.receiver.readiness() == CrossloadReadiness::Complete
        {
            self.chart.apply(SyncEvent::ReplicationComplete);
        }
        if self.permitted && self.drive_scans {
            let mut epoch = self.epoch;
            self.host
                .run_with_commit(
                    1,
                    || 0,
                    |_| {
                        epoch = epoch.next();
                    },
                )
                .unwrap();
            self.epoch = epoch;
        }
    }
}

fn pair_config(role: ConfiguredRole) -> RedundancyConfig {
    RedundancyConfig::pair(PairId::new(1), role)
        .with_confirmation_exchanges(2)
        .with_missed_exchanges(2)
}

/// Boots an admitted pair over a loopback link, both over the same base
/// application.
fn boot_pair(base: &Container) -> (Node, Node) {
    let boot_host = || {
        let mut bytes = Vec::new();
        base.write_to(&mut bytes).unwrap();
        RuntimeHost::new(Container::read_from(&mut Cursor::new(&bytes)).unwrap()).unwrap()
    };
    let (port_a, port_b) = loopback_pair();
    let a = Node::boot(pair_config(ConfiguredRole::Primary), port_a, boot_host());
    let b = Node::boot(pair_config(ConfiguredRole::Secondary), port_b, boot_host());
    (a, b)
}

/// Steps both units with their receive phases together, so each round
/// both see the previous round's frames — symmetric exchanges.
fn step_pair(a: &mut Node, b: &mut Node) {
    a.receive();
    b.receive();
    a.transmit();
    b.transmit();
    a.advance();
    b.advance();
}

/// Stages `candidate` on the Primary and crossloads it to the Secondary,
/// driving the pair until the acceptance answer returns. Returns the
/// offered generation.
fn crossload_candidate(a: &mut Node, b: &mut Node, candidate: Container) -> u32 {
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
    let offer = package_offer(a.pair_id(), a.epoch, &a.host, wire).unwrap();
    let generation = offer.generation.raw();
    a.send(CrossloadMessage::Offer(offer));
    for _ in 0..3 {
        step_pair(a, b);
    }
    generation
}

#[test]
fn crossload_when_primary_accepts_then_secondary_holds_identical_generation_and_state() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let (mut a, mut b) = boot_pair(&base);
    // The Secondary pairs and synchronizes but cannot report ready: no
    // replication has completed.
    for _ in 0..4 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(b.chart.state(), SyncState::Syncing);
    assert_eq!(b.receiver.readiness(), CrossloadReadiness::InProgress);

    // Accept on the Primary: the offer carries the candidate wire
    // bytes, the snapshot, the candidate generation, and the epoch.
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
    let offer = package_offer(a.pair_id(), a.epoch, &a.host, wire).unwrap();
    let generation = offer.generation;
    a.send(CrossloadMessage::Offer(offer.clone()));
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }

    // ADR-0064(c): the same candidate generation on both units, the
    // replicated snapshot applied byte-identically, and the pair
    // reports SYNC — the Secondary is redundancy-ready.
    assert_eq!(b.receiver.readiness(), CrossloadReadiness::Complete);
    assert_eq!(b.receiver.alarm(), None);
    assert_eq!(b.chart.state(), SyncState::SyncReady);
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
    for _ in 0..4 {
        step_pair(&mut a, &mut b);
    }
    // Freeze the producer's state so the replicated image is asserted
    // byte-for-byte.
    a.drive_scans = false;

    crossload_candidate(&mut a, &mut b, candidate);
    assert_eq!(b.chart.state(), SyncState::SyncReady);
    assert_eq!(b.host.status().mode, HostMode::Normal);
    assert_eq!(b.host.read_variable(a_index).unwrap(), 4);

    // The Primary tests: the migration moves the state under the
    // candidate (A carried, B initialized), and the replication stream
    // carries the candidate-layout image to the Secondary.
    a.host.test().unwrap();
    a.host.run(1, || 0).unwrap();
    a.send(CrossloadMessage::StateUpdate(a.host.state_snapshot()));
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

    // The Primary dies; the survivor promotes (modeled at the
    // verdict/policy layer — fencing is a later slice) and takes over
    // executing the CANDIDATE, already under Test.
    a.port.set_partitioned(true);
    for _ in 0..4 {
        step_pair(&mut a, &mut b);
    }
    assert!(b.liveness.is_dead());
    assert!(matches!(
        b.chart.state(),
        SyncState::DeSync | SyncState::Syncing
    ));

    b.promote();
    assert_eq!(b.host.status().mode, HostMode::Testing);
    b.host.run(1, || 0).unwrap();
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
    for _ in 0..4 {
        step_pair(&mut a, &mut b);
    }
    a.drive_scans = false;

    crossload_candidate(&mut a, &mut b, candidate);
    assert_eq!(b.host.read_variable(counter).unwrap(), 4);
    a.host.test().unwrap();
    a.host.run(1, || 0).unwrap();
    a.send(CrossloadMessage::StateUpdate(a.host.state_snapshot()));
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(a.host.read_variable(counter).unwrap(), 14);
    assert_eq!(b.host.read_variable(counter).unwrap(), 14);
    assert_eq!(b.host.status().mode, HostMode::Normal);

    a.port.set_partitioned(true);
    for _ in 0..4 {
        step_pair(&mut a, &mut b);
    }
    assert!(b.liveness.is_dead());

    b.promote();
    // The takeover flips the selector to the CANDIDATE without a swap:
    // the state has moved under the candidate on the pair, and the
    // survivor's replicated image is current.
    b.host.takeover_testing().unwrap();
    assert_eq!(b.host.status().mode, HostMode::Testing);
    b.host.run(1, || 0).unwrap();
    // 14 (replicated) + 10 (the candidate's step), never + 1.
    assert_eq!(b.host.read_variable(counter).unwrap(), 24);
}

#[test]
fn crossload_when_transfer_garbled_then_secondary_stays_unsynced_and_primary_notified() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let (mut a, mut b) = boot_pair(&base);
    for _ in 0..4 {
        step_pair(&mut a, &mut b);
    }
    // One edit synchronizes normally: the pair is redundancy-ready.
    let first = compile_source(&counter_program("Counter := Counter + 2;"));
    crossload_candidate(&mut a, &mut b, first);
    assert_eq!(b.chart.state(), SyncState::SyncReady);
    a.drive_scans = false;

    // The next edit's offer crosses garbled: the CRC fails mid-payload.
    // Cancel the first candidate so the Primary may stage the next.
    a.host.cancel().unwrap();
    a.send(CrossloadMessage::CancelCandidate);
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(b.chart.state(), SyncState::SyncReady);

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
    let offer = package_offer(a.pair_id(), a.epoch, &a.host, wire).unwrap();
    let mut frame = encode(&CrossloadMessage::Offer(offer));
    let middle = frame.len() / 2;
    frame[middle] ^= 0xFF;
    a.port.send(&frame).unwrap();
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }

    // The Secondary never pretends redundancy-ready: the readiness stays
    // down, the chart drops out of SYNC_READY and re-enters SYNCING per
    // the readiness policy, the alarm latches with the coded refusal,
    // and the pair link answers it — the Primary is notified.
    assert_eq!(b.receiver.readiness(), CrossloadReadiness::InProgress);
    assert_eq!(b.receiver.alarm(), Some(CrossloadRefusal::Interrupted));
    assert_eq!(b.receiver.alarm().unwrap().v_code(), "V4104");
    assert_eq!(b.chart.state(), SyncState::Syncing);
    assert!(b.host.status().candidate.is_none());
    assert_eq!(a.notified, Some(CrossloadRefusal::Interrupted));
}

#[test]
fn crossload_when_pair_cancels_then_both_drop_the_candidate() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let (mut a, mut b) = boot_pair(&base);
    for _ in 0..4 {
        step_pair(&mut a, &mut b);
    }
    a.drive_scans = false;

    let candidate = compile_source(&counter_program("Counter := Counter + 2;"));
    crossload_candidate(&mut a, &mut b, candidate);
    assert!(a.host.status().candidate.is_some());
    assert!(b.host.status().candidate.is_some());

    // ADR-0064(g): Cancel drops the candidate on both units from
    // exec = Original.
    a.host.cancel().unwrap();
    a.send(CrossloadMessage::CancelCandidate);
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }

    assert!(a.host.status().candidate.is_none());
    assert!(b.host.status().candidate.is_none());
    assert_eq!(b.receiver.alarm(), None);
}

#[test]
fn crossload_when_pair_untests_then_selector_returns_and_candidate_is_kept() {
    let base = compile_source(&counter_program("Counter := Counter + 1;"));
    let counter = variable_index(&base, "Counter");
    let candidate = compile_source(&counter_program("Counter := Counter + 10;"));
    let (mut a, mut b) = boot_pair(&base);
    for _ in 0..4 {
        step_pair(&mut a, &mut b);
    }
    a.drive_scans = false;

    crossload_candidate(&mut a, &mut b, candidate);
    assert_eq!(b.host.read_variable(counter).unwrap(), 4);

    // The Primary tests; the layout-preserving Test is invisible on the
    // replication stream, so the Secondary's selector stays Original.
    a.host.test().unwrap();
    a.host.run(1, || 0).unwrap();
    a.send(CrossloadMessage::StateUpdate(a.host.state_snapshot()));
    for _ in 0..3 {
        step_pair(&mut a, &mut b);
    }
    assert_eq!(a.host.status().mode, HostMode::Testing);
    assert_eq!(b.host.status().mode, HostMode::Normal);
    assert_eq!(b.host.read_variable(counter).unwrap(), 14);

    // ADR-0064(f): Untest switches the selector back on both units; the
    // candidate is kept. The Secondary never mirrored the Test, so the
    // notice is a no-op there — but it answers, and the pair agrees.
    a.send(CrossloadMessage::UntestCandidate);
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
    assert_eq!(b.receiver.alarm(), None);
}
