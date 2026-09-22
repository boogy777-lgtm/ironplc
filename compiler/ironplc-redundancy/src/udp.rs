//! The real pair-link binding: one `NicPort` over a `std::net::UdpSocket`.
//!
//! The architecture doc deferred "the real pair-link driver over two
//! processes" to the binding layer: the loopback simulator binding
//! ([`crate::loopback`]) stays the test vehicle, and this binding carries
//! the same framed messages — one datagram per frame, the ping/pong
//! liveness frames and the CRC-guarded crossload frames of
//! [`crate::liveness`] / [`crate::crossload`] unchanged — between two
//! `ironplcvm serve` processes (loopback demos today; a dedicated link
//! network later). The socket is connected to the single peer address the
//! composition root configures, so `send` needs no destination and
//! `poll` accepts no foreign datagrams; delivery stays UDP-honest — a
//! frame may be lost, duplicated, or delayed, and the liveness exchange's
//! missed-exchange counting (not this binding) is the authority on what
//! silence means.
//!
//! The socket is non-blocking: `poll` drains at most one datagram per
//! call (the [`NicPort`] contract — "the next received frame, if any"),
//! and `WouldBlock` is the empty-queue answer, never an error. A receive
//! error — a connected peer's port closed answers ICMP port unreachable —
//! counts as a dropped frame and yields `None`, so a dead peer surfaces
//! as silence, the partition model, not as a session failure. The
//! timestamp is `IngressTimestamp::NONE`: a software socket has no
//! hardware clock, the same honest default the loopback binding records.

use std::io;
use std::net::{SocketAddr, UdpSocket};

use crate::hal::{IngressTimestamp, NicPort, PhyCounters, PortCapabilities, PortError};

/// The largest frame this binding carries: one full UDP datagram
/// (65,507 bytes payload on IPv4). The pair-link codec has no
/// segmentation yet — the same envelope the loopback binding carries in
/// memory — so a larger crossload payload is the segmentation follow-up,
/// not a silent truncation.
const MAX_DATAGRAM: usize = u16::MAX as usize - 8 - 20;

/// One pair-link port over a connected UDP socket.
pub struct UdpPort {
    socket: UdpSocket,
    counters: PhyCounters,
}

impl UdpPort {
    /// Binds `bind` and connects the socket to the single peer `peer`.
    /// The socket is non-blocking from construction: the pair pump polls
    /// it on the session's cadence.
    pub fn bind(bind: SocketAddr, peer: SocketAddr) -> io::Result<Self> {
        let socket = UdpSocket::bind(bind)?;
        socket.connect(peer)?;
        socket.set_nonblocking(true)?;
        Ok(Self {
            socket,
            counters: PhyCounters::default(),
        })
    }

    /// The local address the socket bound to (`127.0.0.1:0` becomes the
    /// ephemeral port) — the composition root logs it, tests assert it.
    pub fn local_addr(&self) -> io::Result<SocketAddr> {
        self.socket.local_addr()
    }
}

impl NicPort for UdpPort {
    fn capabilities(&self) -> PortCapabilities {
        // A software socket proves no hardware guarantees: the binding
        // records the guarantee level it can honestly prove.
        PortCapabilities::default()
    }

    fn counters(&self) -> PhyCounters {
        self.counters
    }

    fn send(&mut self, frame: &[u8]) -> Result<(), PortError> {
        // A connected UDP send fails only when the link is unusable (the
        // OS queue is full, the socket erred) — the `Closed` shape names
        // that honestly. Delivery is still UDP-best-effort: the frame may
        // never arrive, and the exchange's miss counting owns that truth.
        match self.socket.send(frame) {
            Ok(_) => {
                self.counters.frames_sent += 1;
                Ok(())
            }
            Err(_) => Err(PortError::Closed),
        }
    }

    fn poll(&mut self) -> Option<(IngressTimestamp, Vec<u8>)> {
        let mut frame = vec![0u8; MAX_DATAGRAM];
        match self.socket.recv(&mut frame) {
            Ok(len) => {
                self.counters.frames_received += 1;
                frame.truncate(len);
                // No hardware clock on a software socket.
                Some((IngressTimestamp::NONE, frame))
            }
            Err(err) if err.kind() == io::ErrorKind::WouldBlock => None,
            Err(_) => {
                // A receive error — the connected peer's port closed
                // answers ICMP port unreachable — is the link's silence,
                // not a protocol input: count the loss and yield nothing.
                self.counters.frames_dropped += 1;
                None
            }
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::hal::NicPort;

    /// Reserves one ephemeral loopback address (bind, read, release).
    fn ephemeral_addr() -> SocketAddr {
        let socket = UdpSocket::bind("127.0.0.1:0").unwrap();
        let addr = socket.local_addr().unwrap();
        drop(socket);
        addr
    }

    /// Binds a connected pair of ports over reserved ephemeral addresses
    /// (the reserve-release pattern; on loopback the window is benign).
    fn udp_pair() -> (UdpPort, UdpPort) {
        let a_addr = ephemeral_addr();
        let b_addr = ephemeral_addr();
        (
            UdpPort::bind(a_addr, b_addr).unwrap(),
            UdpPort::bind(b_addr, a_addr).unwrap(),
        )
    }

    /// Drains every pending inbound frame of `port`, returning the payloads.
    fn drain(port: &mut UdpPort) -> Vec<Vec<u8>> {
        let mut frames = Vec::new();
        while let Some((_, frame)) = port.poll() {
            frames.push(frame);
        }
        frames
    }

    #[test]
    fn udp_when_frame_sent_then_peer_receives_it_byte_identical() {
        let (mut a, mut b) = udp_pair();

        a.send(&[1, 2, 3]).unwrap();
        // Give the datagram one scheduling quantum; UDP on loopback is
        // not instantaneous across threads.
        std::thread::sleep(std::time::Duration::from_millis(50));

        assert_eq!(drain(&mut b), vec![vec![1, 2, 3]]);
        assert_eq!(a.counters().frames_sent, 1);
        assert_eq!(b.counters().frames_received, 1);
    }

    #[test]
    fn udp_when_queried_then_no_hardware_capabilities() {
        let (a, _) = udp_pair();

        let capabilities = a.capabilities();

        assert!(!capabilities.hardware_timestamp);
        assert!(!capabilities.irq);
        assert!(!capabilities.dma);
    }

    #[test]
    fn udp_when_no_datagram_pending_then_poll_returns_none() {
        let (_, mut b) = udp_pair();

        assert!(b.poll().is_none());
    }

    #[test]
    fn udp_when_peer_silent_then_poll_returns_none_and_link_survives() {
        // A peer that never answers — nothing bound at its address — is
        // the loss model: the first send succeeds (UDP never blocks on
        // delivery), inbound stays silent, and once the ICMP refusal
        // latches a send may fail — the pump treats that as the
        // partition model, never a session failure.
        let peer_addr = ephemeral_addr();
        let mut silent = UdpPort::bind(ephemeral_addr(), peer_addr).unwrap();

        assert!(silent.send(&[1]).is_ok());
        for _ in 0..8 {
            std::thread::sleep(std::time::Duration::from_millis(10));
            assert!(silent.poll().is_none());
        }
    }

    #[test]
    fn udp_when_peer_rebinds_after_restart_then_frames_flow_again() {
        // The process-restart model at the binding: the peer's socket
        // dies and a fresh one (a restarted process's bind) takes its
        // address; once the address answers again, the link carries
        // frames.
        let (mut a, mut b) = udp_pair();
        let a_addr = a.local_addr().unwrap();
        drop(a);

        a = UdpPort::bind(a_addr, b.local_addr().unwrap()).unwrap();
        a.send(&[2]).unwrap();
        std::thread::sleep(std::time::Duration::from_millis(50));

        assert_eq!(drain(&mut b), vec![vec![2]]);
    }

    #[test]
    fn udp_when_large_frame_within_datagram_limit_then_round_trips() {
        let (mut a, mut b) = udp_pair();
        let frame = vec![0xAB; MAX_DATAGRAM];

        a.send(&frame).unwrap();
        std::thread::sleep(std::time::Duration::from_millis(50));

        assert_eq!(drain(&mut b), vec![frame]);
    }
}
