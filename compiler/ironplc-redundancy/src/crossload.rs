//! Crossload: the online-change payload pipeline of the pair link
//! (ADR-0064(c)–(h)).
//!
//! At Accept the Primary's supervisor packages
//! `{candidate wire bytes + state snapshot + CandidateGenerationId +
//! epoch}` into a [`CrossloadOffer`]; the Secondary validates the wire
//! bytes with the same load-time verifier boot uses
//! ([`Container::read_from`](ironplc_container::Container::read_from)),
//! stages through the existing `stage_with_decisions` (one candidate
//! authority, unchanged), verifies the generation contract — the same
//! candidate generation on both units is the admission contract of
//! ADR-0064(c) — and applies the replicated image while idle through the
//! runtime's seam 4. A unit that cannot accept latches an alarm and
//! answers `Refused`: it never pretends redundancy-ready.
//!
//! The wire is one CRC-guarded frame per message, demuxed from the
//! fixed 38-byte ping/pong frames by magic and length — a codec detail
//! of this module, not a framework (the architecture doc, "Deliberately
//! Not Built"). A frame that fails any check decodes as `None`: a
//! garbled transfer is dropped, never acted on; the session reports the
//! interruption through [`CrossloadRefusal::Interrupted`] and the unit
//! stays deSYNC.
//!
//! The per-unit driver that pumps these frames through the link lives in
//! the integration tests as test support (the Slice 2 precedent): the
//! real shell replaces it when a binary embeds the layer.

use std::collections::BTreeMap;

use ironplc_container::Container;
use ironplc_runtime::{AcceptedEdit, HostMode, LogicGeneration, RuntimeHost, StateSnapshot};

use crate::config::PairId;
use crate::epoch::Epoch;
use crate::liveness::crc32;
use crate::problem_codes;
use crate::statechart::CrossloadReadiness;

/// The two magic bytes every crossload frame starts with; they demux the
/// variable-length crossload frames from the fixed 38-byte ping/pong
/// frames on the same port.
pub const FRAME_MAGIC: [u8; 2] = [0xC7, 0x1C];

const MAGIC: [u8; 2] = FRAME_MAGIC;

/// The frame format version this codec encodes.
const VERSION: u8 = 1;

/// Wire discriminant of a [`CrossloadMessage`].
const KIND_OFFER: u8 = 0;
const KIND_ACCEPTED: u8 = 1;
const KIND_REFUSED: u8 = 2;
const KIND_CANCEL: u8 = 3;
const KIND_UNTEST: u8 = 4;
const KIND_STATE_UPDATE: u8 = 5;
const KIND_ASSEMBLE: u8 = 6;

/// One crossload message on the pair link: the offer/response pipeline
/// plus the pair-lifecycle notices that carry no payload.
#[derive(Clone, Debug, PartialEq, Eq)]
pub enum CrossloadMessage {
    /// The ADR-0064(c) Accept package: the candidate's exact wire bytes,
    /// the Primary's persistent-state snapshot, the staged candidate
    /// generation, and the pair epoch — one transaction unit.
    Offer(CrossloadOffer),
    /// The peer accepted: the candidate is staged, the generation
    /// contract holds, and the snapshot was applied while idle.
    Accepted,
    /// The peer refused; the carried refusal is the alarm code the
    /// receiver latched (the Primary is notified by this frame).
    Refused(CrossloadRefusal),
    /// ADR-0064(g) pair notice: drop the staged candidate (valid only
    /// from exec = Original).
    CancelCandidate,
    /// ADR-0064(f) pair notice: switch the execution selector back at a
    /// boundary, keeping the candidate.
    UntestCandidate,
    /// ADR-0064(h) pair notice: the pair's one commit transaction —
    /// the candidate becomes canonical on the owner and the notice
    /// promotes it on the standby (the owner already persisted before
    /// the acknowledgment rendered; the standby persists on apply).
    AssembleCandidate,
    /// The steady-state replication of a monitoring peer (ADR-0064(d)
    /// "keeps replicating"): one state image, no candidate payload — the
    /// candidate rides an [`CrossloadMessage::Offer`]. A monitoring peer
    /// applies each image while idle; a candidate-layout image is the
    /// pair's Test of a migration candidate arriving over replication.
    StateUpdate(StateSnapshot),
}

/// The ADR-0064(c) Accept package of one candidate generation.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct CrossloadOffer {
    /// The pair the offer belongs to; a foreign value is refused with
    /// V4101, never staged.
    pub pair_id: PairId,
    /// The owning unit's epoch at Accept (the anti-stale ordering input).
    pub epoch: Epoch,
    /// The CandidateGenerationId staged on the Primary; the Secondary
    /// verifies its own staged generation matches — the pair holds one
    /// application generation or the offer is refused.
    pub generation: LogicGeneration,
    /// The candidate's exact wire bytes, exactly as the client sent
    /// them to the Primary (ADR-0064 amendment: no re-serialization).
    pub candidate_wire: Vec<u8>,
    /// The Primary's persistent-state snapshot at Accept.
    pub snapshot: StateSnapshot,
}

/// Why a crossload was refused — the crate's alarm vocabulary (the
/// crate-local V41xx CSV). A refusal is terminal for the transfer: the
/// unit stays deSYNC and readiness is re-established per policy.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum CrossloadRefusal {
    /// The offer names a different pair identity (V4101).
    ForeignPair,
    /// The candidate wire bytes failed the load-time verification or
    /// staging validation on the peer (V4102).
    CandidateRejected,
    /// The replicated snapshot was refused by the peer's apply — layout
    /// incompatible or internally corrupt, fail-closed (V4103).
    SnapshotRejected,
    /// The staged generation does not match the offered generation; the
    /// pair does not hold one application generation (V4105).
    GenerationMismatch,
    /// The transfer was interrupted or carried garbled frames; nothing
    /// was applied (V4104).
    Interrupted,
}

impl CrossloadRefusal {
    /// The stable V-code surfacing this refusal on the HA command
    /// surface (the crate-local CSV).
    pub fn v_code(&self) -> &'static str {
        match self {
            CrossloadRefusal::ForeignPair => problem_codes::PAIR_IDENTITY_MISMATCH,
            CrossloadRefusal::CandidateRejected => problem_codes::CROSSLOAD_CANDIDATE_REJECTED,
            CrossloadRefusal::SnapshotRejected => problem_codes::CROSSLOAD_SNAPSHOT_REJECTED,
            CrossloadRefusal::GenerationMismatch => problem_codes::CROSSLOAD_GENERATION_MISMATCH,
            CrossloadRefusal::Interrupted => problem_codes::CROSSLOAD_INTERRUPTED,
        }
    }

    /// The wire discriminant for this refusal.
    const fn as_u16(self) -> u16 {
        match self {
            CrossloadRefusal::ForeignPair => 0,
            CrossloadRefusal::CandidateRejected => 1,
            CrossloadRefusal::SnapshotRejected => 2,
            CrossloadRefusal::GenerationMismatch => 3,
            CrossloadRefusal::Interrupted => 4,
        }
    }

    /// Parses the wire discriminant; `None` names no refusal (the frame
    /// is dropped, never acted on).
    fn from_u16(raw: u16) -> Option<Self> {
        match raw {
            0 => Some(CrossloadRefusal::ForeignPair),
            1 => Some(CrossloadRefusal::CandidateRejected),
            2 => Some(CrossloadRefusal::SnapshotRejected),
            3 => Some(CrossloadRefusal::GenerationMismatch),
            4 => Some(CrossloadRefusal::Interrupted),
            _ => None,
        }
    }
}

/// Appends the snapshot's fixed prefix fields: `num_variables`,
/// `data_region_bytes`, `layout_hash`.
fn push_snapshot_prefix(frame: &mut Vec<u8>, snapshot: &StateSnapshot) {
    frame.extend_from_slice(&snapshot.num_variables.to_be_bytes());
    frame.extend_from_slice(&snapshot.data_region_bytes.to_be_bytes());
    frame.extend_from_slice(&snapshot.layout_hash);
}

/// Appends the snapshot's byte blobs: the raw slot bytes, then the data
/// region.
fn push_snapshot_blobs(frame: &mut Vec<u8>, snapshot: &StateSnapshot) {
    for slot in &snapshot.vars {
        frame.extend_from_slice(&slot.to_be_bytes());
    }
    frame.extend_from_slice(&snapshot.data_region);
}

/// The snapshot prefix before the byte blobs: `num_variables`,
/// `data_region_bytes`, `layout_hash` — 38 bytes.
const SNAPSHOT_PREFIX_LEN: usize = 38;

/// Reads one snapshot from `prefix` + `payload`, where `payload` holds
/// the slot blob followed by the data region. `None` when the payload is
/// internally inconsistent — a garbled frame is dropped, never acted on.
fn take_snapshot(prefix: &[u8], payload: &[u8]) -> Option<StateSnapshot> {
    let num_variables = u16::from_be_bytes(prefix.get(..2)?.try_into().ok()?);
    let data_region_bytes = u32::from_be_bytes(prefix.get(2..6)?.try_into().ok()?);
    let mut layout_hash = [0u8; 32];
    layout_hash.copy_from_slice(prefix.get(6..SNAPSHOT_PREFIX_LEN)?);
    // `num_variables` pins the slot count, so the slot blob is the
    // leading `num_variables * 8` bytes and the data region takes the
    // rest; either length mismatching the declarations is garbled.
    let (vars, data_region) = payload.as_chunks::<8>();
    if vars.len() != usize::from(num_variables) || data_region.len() != data_region_bytes as usize {
        return None;
    }
    Some(StateSnapshot {
        layout_hash,
        num_variables,
        data_region_bytes,
        vars: vars
            .iter()
            .map(|chunk| u64::from_be_bytes(*chunk))
            .collect(),
        data_region: data_region.to_vec(),
    })
}

/// Encodes one message as its CRC-guarded wire frame.
pub fn encode(message: &CrossloadMessage) -> Vec<u8> {
    let mut frame = Vec::new();
    frame.extend_from_slice(&MAGIC);
    frame.push(VERSION);
    match message {
        CrossloadMessage::Offer(offer) => {
            frame.push(KIND_OFFER);
            frame.extend_from_slice(&offer.pair_id.raw().to_be_bytes());
            frame.extend_from_slice(&offer.epoch.raw().to_be_bytes());
            frame.extend_from_slice(&offer.generation.raw().to_be_bytes());
            frame.extend_from_slice(&(offer.candidate_wire.len() as u32).to_be_bytes());
            push_snapshot_prefix(&mut frame, &offer.snapshot);
            frame.extend_from_slice(&offer.candidate_wire);
            push_snapshot_blobs(&mut frame, &offer.snapshot);
        }
        CrossloadMessage::Accepted => frame.push(KIND_ACCEPTED),
        CrossloadMessage::Refused(refusal) => {
            frame.push(KIND_REFUSED);
            frame.extend_from_slice(&refusal.as_u16().to_be_bytes());
        }
        CrossloadMessage::CancelCandidate => frame.push(KIND_CANCEL),
        CrossloadMessage::UntestCandidate => frame.push(KIND_UNTEST),
        CrossloadMessage::AssembleCandidate => frame.push(KIND_ASSEMBLE),
        CrossloadMessage::StateUpdate(snapshot) => {
            frame.push(KIND_STATE_UPDATE);
            push_snapshot_prefix(&mut frame, snapshot);
            push_snapshot_blobs(&mut frame, snapshot);
        }
    }
    let crc = crc32(&frame);
    frame.extend_from_slice(&crc.to_be_bytes());
    frame
}

/// Decodes one wire frame; `None` for a wrong magic, version, length,
/// payload inconsistency, unknown discriminant, or failed CRC — a
/// garbled frame is dropped, never acted on.
pub fn decode(frame: &[u8]) -> Option<CrossloadMessage> {
    let (prefix, crc) = frame.split_last_chunk::<4>()?;
    if crc32(prefix) != u32::from_be_bytes(*crc) {
        return None;
    }
    let body = prefix.get(4..)?;
    if prefix.first_chunk::<2>()? != &MAGIC || prefix.get(2)? != &VERSION {
        return None;
    }
    match *prefix.get(3)? {
        KIND_OFFER => decode_offer(body),
        KIND_ACCEPTED if body.is_empty() => Some(CrossloadMessage::Accepted),
        KIND_REFUSED => {
            let raw: [u8; 2] = body.try_into().ok()?;
            Some(CrossloadMessage::Refused(CrossloadRefusal::from_u16(
                u16::from_be_bytes(raw),
            )?))
        }
        KIND_CANCEL if body.is_empty() => Some(CrossloadMessage::CancelCandidate),
        KIND_UNTEST if body.is_empty() => Some(CrossloadMessage::UntestCandidate),
        KIND_ASSEMBLE if body.is_empty() => Some(CrossloadMessage::AssembleCandidate),
        KIND_STATE_UPDATE => decode_state_update(body),
        _ => None,
    }
}

/// The offer payload after the 4-byte header: the 16-byte identity
/// block (pair id, epoch, generation), the wire length, the shared
/// snapshot block, then the candidate wire bytes.
const OFFER_PREFIX_LEN: usize = 20 + SNAPSHOT_PREFIX_LEN;

fn decode_offer(body: &[u8]) -> Option<CrossloadMessage> {
    let prefix = body.get(..OFFER_PREFIX_LEN)?;
    let wire_len = u32::from_be_bytes(prefix.get(16..20)?.try_into().ok()?) as usize;
    let snapshot_prefix = prefix.get(20..OFFER_PREFIX_LEN)?;
    let payload = body.get(OFFER_PREFIX_LEN..)?;
    // The snapshot prefix already pins the slot byte length; the
    // remaining payload after the wire bytes must be exactly the slot
    // blob plus the declared data region.
    let snapshot = take_snapshot(snapshot_prefix, payload.get(wire_len..)?)?;
    if payload.len() != wire_len + snapshot.vars.len() * 8 + snapshot.data_region.len() {
        return None;
    }
    Some(CrossloadMessage::Offer(CrossloadOffer {
        pair_id: PairId::new(u64::from_be_bytes(prefix.get(..8)?.try_into().ok()?)),
        epoch: Epoch::new(u32::from_be_bytes(prefix.get(8..12)?.try_into().ok()?)),
        generation: LogicGeneration::new(u32::from_be_bytes(prefix.get(12..16)?.try_into().ok()?)),
        candidate_wire: payload[..wire_len].to_vec(),
        snapshot,
    }))
}

/// The state-update payload after the 4-byte header: the shared snapshot
/// block, then the slot blob and the data region.
fn decode_state_update(body: &[u8]) -> Option<CrossloadMessage> {
    let prefix = body.get(..SNAPSHOT_PREFIX_LEN)?;
    let payload = body.get(SNAPSHOT_PREFIX_LEN..)?;
    Some(CrossloadMessage::StateUpdate(take_snapshot(
        prefix, payload,
    )?))
}

/// Packages the Primary's staged candidate for the pair link: the
/// staged candidate generation, the current persistent-state snapshot,
/// and the caller's wire bytes — `None` when no candidate is staged (a
/// packaging request with nothing staged is the caller's sequencing
/// error, not an offer of nothing).
pub fn package_offer(
    pair_id: PairId,
    epoch: Epoch,
    host: &RuntimeHost,
    candidate_wire: Vec<u8>,
) -> Option<CrossloadOffer> {
    let generation = host.status().candidate?;
    Some(CrossloadOffer {
        pair_id,
        epoch,
        generation,
        candidate_wire,
        snapshot: host.state_snapshot(),
    })
}

/// The Secondary's Accept mirror (ADR-0064(c)), reusing the runtime's
/// staging and apply unchanged: pair identity, load-verify, stage, the
/// generation contract, then the idle apply. Any refusal rolls the
/// half-staged candidate back to the clean pre-offer base and is coded —
/// a mismatched offer is refused, never guessed.
pub fn accept_offer(
    local: PairId,
    host: &mut RuntimeHost,
    offer: &CrossloadOffer,
) -> Result<(), CrossloadRefusal> {
    if offer.pair_id != local {
        return Err(CrossloadRefusal::ForeignPair);
    }
    let candidate = Container::read_from(&mut &offer.candidate_wire[..])
        .map_err(|_| CrossloadRefusal::CandidateRejected)?;
    host.stage_with_decisions(
        candidate,
        &BTreeMap::new(),
        Some(AcceptedEdit {
            wire: offer.candidate_wire.clone(),
            name: None,
            origin: None,
        }),
    )
    .map_err(|_| CrossloadRefusal::CandidateRejected)?;
    if host.status().candidate != Some(offer.generation) {
        rollback(host);
        return Err(CrossloadRefusal::GenerationMismatch);
    }
    host.apply_state_snapshot(&offer.snapshot).map_err(|_| {
        rollback(host);
        CrossloadRefusal::SnapshotRejected
    })
}

/// Drops a half-staged offer back to the clean pre-offer base. Best
/// effort by construction: the caller's sequencing keeps the host from
/// advancing between the refusal and this call.
fn rollback(host: &mut RuntimeHost) {
    let _ = host.cancel();
}

/// The Secondary's crossload latch: owns the readiness signal the SYNC
/// chart consumes and the alarm the engineering surface raises. The
/// mechanism behind "a secondary that cannot accept must NOT pretend
/// redundancy-ready" — readiness is `Complete` only through this latch,
/// and every refusal answers the Primary with the coded refusal.
#[derive(Clone, Debug, Default)]
pub struct CrossloadReceiver {
    readiness: CrossloadReadiness,
    alarm: Option<CrossloadRefusal>,
}

impl CrossloadReceiver {
    /// Creates the receiver at `InProgress` with no alarm — the state
    /// of a unit that has not replicated yet.
    pub fn new() -> Self {
        Self::default()
    }

    /// The signal the SYNC chart's replication guard consumes.
    pub fn readiness(&self) -> CrossloadReadiness {
        self.readiness
    }

    /// The last refusal this unit latched, if any — the diagnostics and
    /// alarm surface (the crate's V-code). Cleared by the next
    /// successful acceptance.
    pub fn alarm(&self) -> Option<CrossloadRefusal> {
        self.alarm
    }

    /// Applies one validated offer: stage, verify, apply. On success the
    /// readiness signal rises; on refusal the alarm latches, the offer
    /// rolls back, and the coded refusal answers the Primary.
    pub fn accept(
        &mut self,
        local: PairId,
        host: &mut RuntimeHost,
        offer: &CrossloadOffer,
    ) -> CrossloadMessage {
        match accept_offer(local, host, offer) {
            Ok(()) => {
                self.readiness = CrossloadReadiness::Complete;
                self.alarm = None;
                CrossloadMessage::Accepted
            }
            Err(refusal) => {
                self.readiness = CrossloadReadiness::InProgress;
                self.alarm = Some(refusal);
                CrossloadMessage::Refused(refusal)
            }
        }
    }

    /// Records an interrupted or garbled transfer: nothing decoded, so
    /// nothing was applied; the unit stays deSYNC and answers the coded
    /// refusal so the Primary learns the pair is not redundancy-ready.
    pub fn note_interrupted(&mut self) -> CrossloadMessage {
        self.readiness = CrossloadReadiness::InProgress;
        self.alarm = Some(CrossloadRefusal::Interrupted);
        CrossloadMessage::Refused(CrossloadRefusal::Interrupted)
    }

    /// Applies one steady-state replication image (ADR-0064(d) "keeps
    /// replicating"): the snapshot is the whole payload; the runtime's
    /// seam 4 decides whether it is a like-layout copy or the
    /// candidate-layout advance of the pair's Test of a migration
    /// candidate. A refused image latches the alarm and answers the
    /// coded refusal.
    pub fn apply_update(
        &mut self,
        host: &mut RuntimeHost,
        snapshot: &StateSnapshot,
    ) -> CrossloadMessage {
        match host.apply_state_snapshot(snapshot) {
            Ok(()) => {
                self.readiness = CrossloadReadiness::Complete;
                self.alarm = None;
                CrossloadMessage::Accepted
            }
            Err(_) => self.refused(CrossloadRefusal::SnapshotRejected),
        }
    }

    /// Mirrors the pair's Cancel (ADR-0064(g)): drop the staged
    /// candidate; the refusal answers when the peer's notice diverged
    /// from this unit's lifecycle.
    pub fn cancel_candidate(&mut self, host: &mut RuntimeHost) -> CrossloadMessage {
        match host.cancel() {
            Ok(()) => CrossloadMessage::Accepted,
            Err(_) => self.refused(CrossloadRefusal::CandidateRejected),
        }
    }

    /// Mirrors the pair's Untest (ADR-0064(f)): the selector switches
    /// back at a boundary and the candidate is kept. When this unit never
    /// mirrored the Test — a layout-preserving Test is invisible on the
    /// replication stream, so the selector never left Original — the
    /// mirror is a no-op, but the candidate must still be staged: the
    /// pair keeps it.
    pub fn untest_candidate(&mut self, host: &mut RuntimeHost) -> CrossloadMessage {
        match host.status().mode {
            HostMode::Normal => {
                if host.status().candidate.is_some() {
                    CrossloadMessage::Accepted
                } else {
                    self.refused(CrossloadRefusal::CandidateRejected)
                }
            }
            HostMode::Testing => match host.untest() {
                Err(_) => self.refused(CrossloadRefusal::CandidateRejected),
                Ok(()) => match host.apply_pending_at_boundary() {
                    Ok(()) => CrossloadMessage::Accepted,
                    Err(_) => self.refused(CrossloadRefusal::CandidateRejected),
                },
            },
        }
    }

    /// Mirrors the pair's Assemble (ADR-0064(h)): the candidate becomes
    /// canonical — the pair's one commit transaction, never one unit
    /// holding a different canonical generation. A migration candidate
    /// already moved the standby's state under Test (the selector is on
    /// the candidate); a layout-preserving Test never appeared on the
    /// replication stream, so the mirror flips the selector first — the
    /// same [`RuntimeHost::takeover_testing`] path the takeover policy
    /// uses, certified by the pair's Test on the owner. A candidate whose
    /// state never moved is refused, never promoted over unmigrated
    /// state.
    pub fn assemble_candidate(&mut self, host: &mut RuntimeHost) -> CrossloadMessage {
        match host.status().mode {
            HostMode::Testing => {}
            HostMode::Normal if !host.status().migration => {
                if host.takeover_testing().is_err() {
                    return self.refused(CrossloadRefusal::CandidateRejected);
                }
            }
            HostMode::Normal => return self.refused(CrossloadRefusal::CandidateRejected),
        }
        match host.assemble() {
            Ok(()) => {
                self.readiness = CrossloadReadiness::Complete;
                self.alarm = None;
                CrossloadMessage::Accepted
            }
            Err(_) => self.refused(CrossloadRefusal::CandidateRejected),
        }
    }

    fn refused(&mut self, refusal: CrossloadRefusal) -> CrossloadMessage {
        self.readiness = CrossloadReadiness::InProgress;
        self.alarm = Some(refusal);
        CrossloadMessage::Refused(refusal)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use ironplc_container::{Container, ContainerBuilder, FunctionId};

    /// A host over an empty program, mirroring the admission tests: the
    /// crossload tests exercise the protocol, not the application. The
    /// container round-trips through the wire format first, exactly as a
    /// deployed unit boots — `ContainerBuilder` leaves the integrity
    /// hashes to `write_to`, so an in-memory container would not match a
    /// wire-parsed candidate's populated hashes.
    fn shell_host() -> RuntimeHost {
        RuntimeHost::new(shell_container()).unwrap()
    }

    /// The shell container with populated wire hashes, the form a booted
    /// unit holds.
    fn shell_container() -> Container {
        Container::read_from(&mut &shell_wire()[..]).unwrap()
    }

    /// The shell container as accept-staged wire bytes.
    fn shell_wire() -> Vec<u8> {
        let mut bytes = Vec::new();
        ContainerBuilder::new()
            .add_function(FunctionId::INIT, &[], 0, 0, 0)
            .max_call_depth(1)
            .build()
            .write_to(&mut bytes)
            .unwrap();
        bytes
    }

    fn offer() -> CrossloadOffer {
        CrossloadOffer {
            pair_id: PairId::new(7),
            epoch: Epoch::new(3),
            generation: LogicGeneration::new(2),
            candidate_wire: vec![1, 2, 3, 4],
            snapshot: StateSnapshot {
                layout_hash: [9; 32],
                num_variables: 2,
                data_region_bytes: 4,
                vars: vec![0x0102_0304_0506_0708, 0x1112_1314_1516_1718],
                data_region: vec![0xAA, 0xBB, 0xCC, 0xDD],
            },
        }
    }

    fn assert_round_trip(message: &CrossloadMessage) {
        assert_eq!(decode(&encode(message)).as_ref(), Some(message));
    }

    #[test]
    fn encode_when_round_tripped_then_every_message_survives() {
        assert_round_trip(&CrossloadMessage::Offer(offer()));
        assert_round_trip(&CrossloadMessage::Accepted);
        for refusal in [
            CrossloadRefusal::ForeignPair,
            CrossloadRefusal::CandidateRejected,
            CrossloadRefusal::SnapshotRejected,
            CrossloadRefusal::GenerationMismatch,
            CrossloadRefusal::Interrupted,
        ] {
            assert_round_trip(&CrossloadMessage::Refused(refusal));
        }
        assert_round_trip(&CrossloadMessage::CancelCandidate);
        assert_round_trip(&CrossloadMessage::UntestCandidate);
        assert_round_trip(&CrossloadMessage::AssembleCandidate);
        assert_round_trip(&CrossloadMessage::StateUpdate(offer().snapshot));
    }

    #[test]
    fn decode_when_crc_corrupted_then_rejected() {
        let mut frame = encode(&CrossloadMessage::Offer(offer()));
        let last = frame.len() - 1;
        frame[last] ^= 0xFF;

        assert_eq!(decode(&frame), None);
    }

    /// Re-seals a frame after a test mutation: the CRC covers every byte
    /// before the trailing 4.
    fn reseal(frame: &mut [u8]) {
        let split = frame.len() - 4;
        let crc = crc32(&frame[..split]).to_be_bytes();
        frame[split..].copy_from_slice(&crc);
    }

    #[test]
    fn decode_when_garbled_then_rejected() {
        // Wrong magic, unknown kind, unknown refusal, and truncation all
        // drop the frame — a garbled transfer is never acted on.
        let mut frame = encode(&CrossloadMessage::Accepted);
        frame[0] = 0x00;
        assert_eq!(decode(&frame), None);

        let mut frame = encode(&CrossloadMessage::Accepted);
        frame[3] = 0x7F;
        assert_eq!(decode(&frame), None);

        let mut garbled = encode(&CrossloadMessage::Refused(CrossloadRefusal::Interrupted));
        garbled[4] = 0x7F;
        reseal(&mut garbled);
        assert_eq!(decode(&garbled), None);

        assert_eq!(decode(&garbled[..garbled.len() - 1]), None);
    }

    #[test]
    fn decode_when_snapshot_lengths_inconsistent_then_rejected() {
        let mut message = encode(&CrossloadMessage::Offer(offer()));
        // Declare three variables while carrying two slots, and re-seal
        // the frame so the CRC check cannot catch it first: the slot
        // blob length is pinned to the variable count, so the remaining
        // bytes cannot split into the declared data region.
        message[24] = 0x00;
        message[25] = 0x03;
        reseal(&mut message);

        assert_eq!(decode(&message), None);
    }

    #[test]
    fn refusal_v_code_when_surfaced_then_stable_codes() {
        assert_eq!(
            CrossloadRefusal::ForeignPair.v_code(),
            problem_codes::PAIR_IDENTITY_MISMATCH
        );
        assert_eq!(
            CrossloadRefusal::CandidateRejected.v_code(),
            problem_codes::CROSSLOAD_CANDIDATE_REJECTED
        );
        assert_eq!(
            CrossloadRefusal::SnapshotRejected.v_code(),
            problem_codes::CROSSLOAD_SNAPSHOT_REJECTED
        );
        assert_eq!(
            CrossloadRefusal::GenerationMismatch.v_code(),
            problem_codes::CROSSLOAD_GENERATION_MISMATCH
        );
        assert_eq!(
            CrossloadRefusal::Interrupted.v_code(),
            problem_codes::CROSSLOAD_INTERRUPTED
        );
    }

    #[test]
    fn receiver_when_offer_accepted_then_ready_and_no_alarm() {
        let mut receiver = CrossloadReceiver::new();

        assert_eq!(receiver.readiness(), CrossloadReadiness::InProgress);
        assert_eq!(receiver.alarm(), None);

        // The staged candidate of a fresh shell host is generation 2;
        // the offer carries the host's own snapshot so the apply copies
        // the identical layout.
        let mut host = shell_host();
        let offer = CrossloadOffer {
            candidate_wire: shell_wire(),
            snapshot: host.state_snapshot(),
            ..offer()
        };
        let response = receiver.accept(PairId::new(7), &mut host, &offer);

        assert_eq!(response, CrossloadMessage::Accepted);
        assert_eq!(receiver.readiness(), CrossloadReadiness::Complete);
        assert_eq!(receiver.alarm(), None);
        // The candidate is staged with the wire bytes retained.
        assert!(host.status().candidate.is_some());
    }

    #[test]
    fn receiver_when_offer_foreign_then_alarm_latched_and_refused() {
        let mut receiver = CrossloadReceiver::new();
        let mut host = shell_host();
        let offer = offer();

        let response = receiver.accept(PairId::new(8), &mut host, &offer);

        assert_eq!(
            response,
            CrossloadMessage::Refused(CrossloadRefusal::ForeignPair)
        );
        assert_eq!(receiver.readiness(), CrossloadReadiness::InProgress);
        assert_eq!(receiver.alarm(), Some(CrossloadRefusal::ForeignPair));
        // Nothing staged: the refusal left the host at its clean base.
        assert!(host.status().candidate.is_none());
    }

    #[test]
    fn receiver_when_interrupted_then_stays_in_progress_and_answers_coded() {
        let mut receiver = CrossloadReceiver::new();

        let response = receiver.note_interrupted();

        assert_eq!(
            response,
            CrossloadMessage::Refused(CrossloadRefusal::Interrupted)
        );
        assert_eq!(receiver.readiness(), CrossloadReadiness::InProgress);
        assert_eq!(receiver.alarm(), Some(CrossloadRefusal::Interrupted));
    }

    #[test]
    fn receiver_when_assemble_under_testing_then_promotes_and_answers_accepted() {
        let mut receiver = CrossloadReceiver::new();
        let mut host = shell_host();
        let offer = CrossloadOffer {
            candidate_wire: shell_wire(),
            snapshot: host.state_snapshot(),
            ..offer()
        };
        assert_eq!(
            receiver.accept(PairId::new(7), &mut host, &offer),
            CrossloadMessage::Accepted
        );
        // The pair's Test of this like-layout candidate is invisible on
        // the replication stream, so the mirror flips the selector (the
        // `takeover_testing` path) and assembles.
        assert_eq!(
            receiver.assemble_candidate(&mut host),
            CrossloadMessage::Accepted
        );
        assert_eq!(host.status().mode, HostMode::Normal);
        assert!(host.status().candidate.is_none());
        assert_eq!(host.status().application.raw(), 2);
        assert_eq!(receiver.readiness(), CrossloadReadiness::Complete);
        assert_eq!(receiver.alarm(), None);
    }

    #[test]
    fn receiver_when_assemble_without_candidate_then_refused() {
        let mut receiver = CrossloadReceiver::new();
        let mut host = shell_host();

        let response = receiver.assemble_candidate(&mut host);

        assert_eq!(
            response,
            CrossloadMessage::Refused(CrossloadRefusal::CandidateRejected)
        );
        assert_eq!(receiver.readiness(), CrossloadReadiness::InProgress);
        assert_eq!(receiver.alarm(), Some(CrossloadRefusal::CandidateRejected));
    }
}
