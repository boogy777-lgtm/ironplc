//! Pair liveness: the ping/pong packet codec and the per-channel exchange.
//!
//! One logical ping/pong packet is exchanged over the pair link (port 1;
//! the second channel through the I/O daisy-chain binds later). The
//! packet carries the field list of the FSM spec — pair id, role, epoch,
//! generations, per-channel ping/pong sequences, `io_owner_state`, and a
//! CRC — as a fixed 38-byte frame; the binary cyclic encoding is a codec
//! detail here, not a framework (the architecture doc, "Deliberately Not
//! Built").
//!
//! The wire `role` field is the sender's pair role *as admitted*: Primary
//! exactly when the sender's admission verdict is Primary (the initial
//! owner or a promoted unit). That is this slice's fold of the FSM spec's
//! "configured role + chart state": a booting unit needs the ownership
//! truth on the wire, and the codec is unchanged when the CONTROL chart
//! later becomes the field's producer.
//!
//! Silence detection is a missing expected increment, not a bare packet
//! timeout (FSM spec, "Ping/Pong Liveness"): `ping_seq` advances +1 per
//! PING, and a peer that fails to advance `pong_seq` beyond the
//! confirmation window has missed that exchange. The operational penalty
//! counter adds +1 per successful exchange and +1000 per missing one, so
//! silence dominates the indicator immediately; it is a diagnostics input
//! on the engineering HMI, never the arbiter of takeover. At the
//! configured missed threshold the exchange reports peer death — an input
//! to the SYNC chart, not a promotion by itself. A peer that reboots
//! regresses its PING sequence against the observed high-water; the
//! exchange reports that once as [`LivenessEvent::PeerRestarted`], the
//! SYNC chart's epoch-discontinuity input (a restarted peer lost its
//! epoch memory — T13, external FSM review).

use crate::config::{PairId, RedundancyConfig};
use crate::epoch::Epoch;

/// The sender's pair role as admitted, carried in every ping/pong packet.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum PairRole {
    /// The sender holds output rights (initial owner or promoted).
    Primary,
    /// The sender synchronizes from its peer (monitor mode).
    Secondary,
}

impl PairRole {
    /// The wire discriminant for this role.
    const fn as_byte(self) -> u8 {
        match self {
            PairRole::Primary => 0,
            PairRole::Secondary => 1,
        }
    }

    /// Parses the wire discriminant; `None` when the byte names no role.
    fn from_byte(byte: u8) -> Option<Self> {
        match byte {
            0 => Some(PairRole::Primary),
            1 => Some(PairRole::Secondary),
            _ => None,
        }
    }
}

/// One logical ping/pong packet over the pair link.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct Packet {
    /// The redundant pair (domain) identity; a frame naming another pair
    /// is not this unit's peer (the admission layer refuses it with
    /// V4101, never treats it as liveness evidence).
    pub pair_id: PairId,
    /// The sender's pair role as admitted (see the module documentation).
    pub role: PairRole,
    /// The sender's ownership/fencing epoch; anti-stale only, never an
    /// arbitration input (ADR-0062).
    pub epoch: Epoch,
    /// The sender's application generation (the committed manifest).
    pub generation: u32,
    /// Per-channel PING sequence: +1 per PING the sender emits.
    pub ping_seq: u64,
    /// Per-channel PONG sequence: +1 per PONG the sender emits in answer
    /// to a received PING.
    pub pong_seq: u64,
}

/// Wire size of one ping/pong frame, bytes.
pub const FRAME_LEN: usize = 38;

/// The `io_owner_state` byte is reserved until the CONTROL chart exists;
/// v1 pair links always carry 0 (CONTROL is not on the link yet).
const IO_OWNER_STATE_RESERVED: u8 = 0;

impl Packet {
    /// Encodes the packet as its fixed-size wire frame (big-endian fields,
    /// CRC32 over the preceding bytes).
    pub fn encode(&self) -> [u8; FRAME_LEN] {
        let mut frame = [0u8; FRAME_LEN];
        frame[0..8].copy_from_slice(&self.pair_id.raw().to_be_bytes());
        frame[8] = self.role.as_byte();
        frame[9..13].copy_from_slice(&self.epoch.raw().to_be_bytes());
        frame[13..17].copy_from_slice(&self.generation.to_be_bytes());
        frame[17..25].copy_from_slice(&self.ping_seq.to_be_bytes());
        frame[25..33].copy_from_slice(&self.pong_seq.to_be_bytes());
        frame[33] = IO_OWNER_STATE_RESERVED;
        let crc = crc32(&frame[..34]);
        frame[34..38].copy_from_slice(&crc.to_be_bytes());
        frame
    }

    /// Parses one wire frame; `None` for a wrong length, a failed CRC, or
    /// an unknown role byte — a corrupted or foreign frame is dropped,
    /// never acted on.
    pub fn decode(frame: &[u8]) -> Option<Self> {
        if frame.len() != FRAME_LEN {
            return None;
        }
        let crc = u32::from_be_bytes([frame[34], frame[35], frame[36], frame[37]]);
        if crc32(&frame[..34]) != crc {
            return None;
        }
        let role = PairRole::from_byte(frame[8])?;
        Some(Packet {
            pair_id: PairId::new(u64::from_be_bytes([
                frame[0], frame[1], frame[2], frame[3], frame[4], frame[5], frame[6], frame[7],
            ])),
            role,
            epoch: Epoch::new(u32::from_be_bytes([
                frame[9], frame[10], frame[11], frame[12],
            ])),
            generation: u32::from_be_bytes([frame[13], frame[14], frame[15], frame[16]]),
            ping_seq: u64::from_be_bytes([
                frame[17], frame[18], frame[19], frame[20], frame[21], frame[22], frame[23],
                frame[24],
            ]),
            pong_seq: u64::from_be_bytes([
                frame[25], frame[26], frame[27], frame[28], frame[29], frame[30], frame[31],
                frame[32],
            ]),
        })
    }
}

/// CRC-32 (IEEE 802.3, reflected, polynomial 0xEDB88320) over the frame
/// prefix — integrity over all fields, bitwise with no table. Shared
/// with the crossload codec: one pair-link integrity function, not two.
pub(crate) fn crc32(bytes: &[u8]) -> u32 {
    let mut crc = 0xFFFF_FFFFu32;
    for byte in bytes {
        crc ^= u32::from(*byte);
        for _ in 0..8 {
            let mask = (crc & 1).wrapping_neg();
            crc = (crc >> 1) ^ (0xEDB8_8320 & mask);
        }
    }
    !crc
}

/// Events the ping/pong exchange reports to the SYNC chart.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum LivenessEvent {
    /// Consecutive missed exchanges reached the configured threshold: the
    /// peer is dead for the pair (a deSYNC input; silence makes a
    /// claimant, never an owner).
    PeerDied,
    /// The peer's per-channel PING sequence regressed below the observed
    /// high-water: the peer restarted its exchange from a fresh base (a
    /// reboot — T13, external FSM review). A restarted peer has lost its
    /// epoch memory, so the SYNC chart reads this as an epoch
    /// discontinuity and drops to deSYNC. Reported once per restart: the
    /// restarted base becomes the new high-water.
    PeerRestarted,
}

/// The per-channel ping/pong exchange of one unit.
///
/// The driver feeds validated peer packets through
/// [`note_received`](Self::note_received) and advances one exchange per
/// cadence tick through [`begin_exchange`](Self::begin_exchange), which
/// returns the sequences for the outbound frame and any event. Silence is
/// counted in unconfirmed pings (pings sent since the peer's PONG last
/// incremented), not in absolute sequence values — the counters are
/// per-channel and either side may reboot with a fresh base, so only the
/// *increment* is evidence, never the number. A peer that reboots
/// restarts its per-channel PING sequence from a fresh base; the
/// regression against the observed high-water is the restart evidence
/// (T13, external FSM review: producer restart with `sequence=0`), and
/// the exchange reports it once per restart as
/// [`LivenessEvent::PeerRestarted`] — the SYNC chart's epoch-discontinuity
/// input, because a restarted peer has lost its epoch memory.
pub struct Liveness {
    confirmation_exchanges: u64,
    missed_exchanges: u64,
    ping_seq: u64,
    pong_seq: u64,
    answer_due: bool,
    observed_pong: u64,
    observed_ping: u64,
    unconfirmed: u64,
    missed: u64,
    penalty: u64,
    dead: bool,
}

impl Liveness {
    /// Creates the exchange for one unit of the configured pair, with the
    /// link-timing thresholds from the shell's configuration (open
    /// parameters, in exchanges).
    pub fn new(config: &RedundancyConfig) -> Self {
        Self {
            confirmation_exchanges: u64::from(config.confirmation_exchanges),
            missed_exchanges: u64::from(config.missed_exchanges),
            ping_seq: 0,
            pong_seq: 0,
            answer_due: false,
            observed_pong: 0,
            observed_ping: 0,
            unconfirmed: 0,
            missed: 0,
            penalty: 0,
            dead: false,
        }
    }

    /// Feeds one validated frame from this pair's peer (the admission
    /// layer has already matched the pair identity). A peer PING is
    /// answered by the next outbound frame; a peer PONG increment
    /// confirms the outstanding pings since its last increment (+1),
    /// ending any miss streak. Only the increment is evidence: a repeated
    /// PONG value credits nothing (that is what detects a silently
    /// repeating link). A PING sequence regression is restart evidence
    /// (the peer rebooted with a fresh base): the exchange latches the
    /// restarted base as the new high-water and reports
    /// [`LivenessEvent::PeerRestarted`] once.
    pub fn note_received(&mut self, packet: &Packet) -> Option<LivenessEvent> {
        let mut restarted = None;
        if packet.ping_seq < self.observed_ping {
            restarted = Some(LivenessEvent::PeerRestarted);
        }
        self.observed_ping = packet.ping_seq;
        if packet.pong_seq > self.observed_pong {
            self.observed_pong = packet.pong_seq;
            self.unconfirmed = 0;
            self.penalty += 1;
            self.missed = 0;
            self.dead = false;
        }
        self.answer_due = true;
        restarted
    }

    /// Advances one exchange: emits the next PING (+1), piggybacks the
    /// owed PONG (+1) when a peer PING arrived since the last send, and
    /// accounts silence — when the count of unconfirmed pings exceeds the
    /// confirmation window, that exchange is missing (+1000). Returns the
    /// sequences for the outbound frame and, at most once per death,
    /// [`LivenessEvent::PeerDied`].
    pub fn begin_exchange(&mut self) -> (u64, u64, Option<LivenessEvent>) {
        self.ping_seq += 1;
        self.unconfirmed += 1;
        if self.answer_due {
            self.pong_seq += 1;
            self.answer_due = false;
        }
        if self.unconfirmed > self.confirmation_exchanges {
            self.penalty += 1000;
            self.missed += 1;
            if !self.dead && self.missed >= self.missed_exchanges {
                self.dead = true;
                return (self.ping_seq, self.pong_seq, Some(LivenessEvent::PeerDied));
            }
        }
        (self.ping_seq, self.pong_seq, None)
    }

    /// The operational penalty counter: +1 per successful exchange, +1000
    /// per missing one. Diagnostics only, never arbitration.
    pub fn penalty(&self) -> u64 {
        self.penalty
    }

    /// Consecutive missed exchanges since the last confirmed one.
    pub fn missed(&self) -> u64 {
        self.missed
    }

    /// Whether the exchange currently counts the peer as dead.
    pub fn is_dead(&self) -> bool {
        self.dead
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::config::ConfiguredRole;

    fn packet(ping: u64, pong: u64) -> Packet {
        Packet {
            pair_id: PairId::new(9),
            role: PairRole::Primary,
            epoch: Epoch::new(4),
            generation: 2,
            ping_seq: ping,
            pong_seq: pong,
        }
    }

    fn liveness(confirmation: u32, missed: u32) -> Liveness {
        Liveness::new(
            &RedundancyConfig::pair(PairId::new(9), ConfiguredRole::Primary)
                .with_confirmation_exchanges(confirmation)
                .with_missed_exchanges(missed),
        )
    }

    #[test]
    fn crc32_when_check_vector_then_ieee_value() {
        // The standard CRC-32 check vector: "123456789" -> 0xCBF43926.
        assert_eq!(crc32(b"123456789"), 0xCBF4_3926);
        assert_eq!(crc32(&[]), 0);
    }

    #[test]
    fn encode_when_decoded_then_round_trips_every_field() {
        let packet = Packet {
            pair_id: PairId::new(0x0102_0304_0506_0708),
            role: PairRole::Secondary,
            epoch: Epoch::new(0x0A0B_0C0D),
            generation: 0x1121_3141,
            ping_seq: 0x0102_0304_0506_0708,
            pong_seq: 0x1112_1314_1516_1718,
        };

        let decoded = Packet::decode(&packet.encode());

        assert_eq!(decoded, Some(packet));
    }

    #[test]
    fn encode_when_pinned_then_exact_wire_bytes() {
        // Pins the wire format (field order, big-endian widths, reserved
        // io_owner_state byte, CRC position) exactly like the opcode pins:
        // a renumber breaks exactly this test.
        let frame = packet(0x0102_0304_0506_0708, 0x1112_1314_1516_1718).encode();

        assert_eq!(
            frame,
            [
                0, 0, 0, 0, 0, 0, 0, 9, // pair_id
                0, // role: Primary
                0, 0, 0, 4, // epoch
                0, 0, 0, 2, // generation
                1, 2, 3, 4, 5, 6, 7, 8, // ping_seq
                17, 18, 19, 20, 21, 22, 23, 24, // pong_seq
                0,  // io_owner_state (reserved)
                121, 119, 138, 3, // crc32
            ]
        );
    }

    #[test]
    fn decode_when_crc_corrupted_then_rejected() {
        let mut frame = packet(1, 1).encode();
        frame[9] ^= 0xFF;

        assert_eq!(Packet::decode(&frame), None);
    }

    #[test]
    fn decode_when_unknown_role_then_rejected() {
        let mut frame = packet(1, 1).encode();
        frame[8] = 7;

        assert_eq!(Packet::decode(&frame), None);
    }

    #[test]
    fn decode_when_wrong_length_then_rejected() {
        assert_eq!(Packet::decode(&[0; 10]), None);
        assert_eq!(Packet::decode(&[0; 39]), None);
    }

    #[test]
    fn exchange_when_both_units_step_then_ping_and_pong_advance_by_one() {
        let config = RedundancyConfig::pair(PairId::new(9), ConfiguredRole::Primary);
        let mut a = Liveness::new(&config);
        let mut b = Liveness::new(&config);

        let (a_ping, a_pong, _) = a.begin_exchange();
        b.note_received(&packet(a_ping, a_pong));
        let (b_ping, b_pong, _) = b.begin_exchange();
        a.note_received(&packet(b_ping, b_pong));
        let (a_ping2, a_pong2, _) = a.begin_exchange();
        b.note_received(&packet(a_ping2, a_pong2));
        let (b_ping2, b_pong2, _) = b.begin_exchange();

        assert_eq!((a_ping, a_pong), (1, 0));
        assert_eq!((b_ping, b_pong), (1, 1));
        assert_eq!((a_ping2, a_pong2), (2, 1));
        assert_eq!((b_ping2, b_pong2), (2, 2));
    }

    #[test]
    fn exchange_when_healthy_then_penalty_adds_one_per_exchange() {
        let mut a = liveness(1, 2);

        for peer_pong in 1..=3u64 {
            let (ping, _, event) = a.begin_exchange();
            assert_eq!(event, None);
            // The peer answers each PING with a fresh PONG (+1).
            a.note_received(&packet(ping, peer_pong));
        }

        assert_eq!(a.penalty(), 3);
        assert_eq!(a.missed(), 0);
        assert!(!a.is_dead());
    }

    #[test]
    fn exchange_when_peer_silent_then_penalty_adds_thousand_per_missing_exchange() {
        let mut a = liveness(1, 5);

        let (_, _, _) = a.begin_exchange();
        a.note_received(&packet(1, 1));
        assert_eq!(a.penalty(), 1);

        a.begin_exchange();
        a.begin_exchange();

        // Window 1: the first silent exchange is still tolerated; each
        // further one is missing at +1000, dominating the +1 immediately.
        assert_eq!(a.penalty(), 1001);
        assert_eq!(a.missed(), 1);
        assert!(!a.is_dead());
    }

    #[test]
    fn exchange_when_missed_threshold_reached_then_peer_died_once() {
        let mut a = liveness(1, 2);

        assert_eq!(a.begin_exchange().2, None);
        assert_eq!(a.begin_exchange().2, None);
        assert_eq!(a.begin_exchange().2, Some(LivenessEvent::PeerDied));
        assert!(a.is_dead());
        // Death latches: further misses keep the counter but re-report nothing.
        assert_eq!(a.begin_exchange().2, None);
        assert!(a.is_dead());
    }

    #[test]
    fn exchange_when_peer_revives_then_exchange_recovers() {
        let mut a = liveness(1, 2);
        a.begin_exchange();
        a.begin_exchange();
        a.begin_exchange();
        assert!(a.is_dead());

        // The peer answers from beyond the observed PONG: the revival
        // confirms the exchange (+1) and clears the death latch.
        let (ping, pong, _) = a.begin_exchange();
        a.note_received(&packet(ping, pong + 1));

        assert!(!a.is_dead());
        assert_eq!(a.missed(), 0);
        assert_eq!(a.penalty(), 3001);
    }

    #[test]
    fn exchange_when_peer_repeats_frames_then_no_success_credited() {
        let mut a = liveness(1, 10);

        let (ping, _, _) = a.begin_exchange();
        a.note_received(&packet(ping, 1));
        let (ping2, _, _) = a.begin_exchange();
        // A silently repeating link replays the old PONG: no increment, no
        // +1, and the unconfirmed exchanges start counting as missing.
        a.note_received(&packet(ping2, 1));
        a.begin_exchange();

        assert_eq!(a.penalty(), 1001);
        assert_eq!(a.missed(), 1);
    }

    #[test]
    fn exchange_when_peer_restarts_sequences_then_peer_restarted_once() {
        let mut a = liveness(1, 10);

        let (ping, _, _) = a.begin_exchange();
        assert_eq!(a.note_received(&packet(ping, 1)), None);
        let (ping2, _, _) = a.begin_exchange();
        assert_eq!(a.note_received(&packet(ping2, 2)), None);

        // The peer reboots: its PING sequence restarts from a fresh base,
        // below the observed high-water (T13: producer restart with
        // sequence=0).
        assert_eq!(
            a.note_received(&packet(1, 0)),
            Some(LivenessEvent::PeerRestarted)
        );
        // The restarted base is the new high-water: the exchange reports
        // the restart once, and the catch-up increments are ordinary frames.
        assert_eq!(a.note_received(&packet(2, 1)), None);
        assert_eq!(a.note_received(&packet(3, 2)), None);
    }

    #[test]
    fn exchange_when_peer_reboots_again_then_restart_reported_again() {
        let mut a = liveness(1, 10);

        let (ping, _, _) = a.begin_exchange();
        assert_eq!(a.note_received(&packet(ping, 1)), None);
        let (ping2, _, _) = a.begin_exchange();
        assert_eq!(a.note_received(&packet(ping2, 2)), None);
        assert_eq!(
            a.note_received(&packet(1, 0)),
            Some(LivenessEvent::PeerRestarted)
        );
        assert_eq!(a.note_received(&packet(2, 1)), None);
        assert_eq!(a.note_received(&packet(3, 2)), None);

        // A second reboot restarts the sequence below the new high-water:
        // a new restart, reported again.
        assert_eq!(
            a.note_received(&packet(1, 0)),
            Some(LivenessEvent::PeerRestarted)
        );
    }
}
