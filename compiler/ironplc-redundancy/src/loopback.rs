//! The loopback simulator binding: two in-process `NicPort` ends over a
//! memory link.
//!
//! The architecture doc mandates this binding as a first-class early
//! deliverable ("Portability is unproven until a second binding exists"):
//! it gives the FSM, admission, and the ping/pong exchange CI coverage
//! before any field device is attached. The binding is single-threaded —
//! the two ends share their queues through `Rc`, matching the host's
//! single-threaded scan loop; a driver steps both ends in one thread.
//!
//! `set_partitioned` models a link cut: the partitioned end discards its
//! outbound frames and delivers no inbound frames, so its peer sees
//! silence — the peer-death and loss scenarios inject failure here.

use std::cell::RefCell;
use std::collections::VecDeque;
use std::rc::Rc;

use crate::hal::{IngressTimestamp, NicPort, PhyCounters, PortCapabilities, PortError};

/// Creates the two ends of one in-process pair link.
pub fn loopback_pair() -> (LoopbackPort, LoopbackPort) {
    let a_to_b: Rc<RefCell<VecDeque<Vec<u8>>>> = Rc::new(RefCell::new(VecDeque::new()));
    let b_to_a: Rc<RefCell<VecDeque<Vec<u8>>>> = Rc::new(RefCell::new(VecDeque::new()));
    (
        LoopbackPort {
            outbound: a_to_b.clone(),
            inbound: b_to_a.clone(),
            partitioned: false,
            counters: PhyCounters::default(),
        },
        LoopbackPort {
            outbound: b_to_a,
            inbound: a_to_b,
            partitioned: false,
            counters: PhyCounters::default(),
        },
    )
}

/// One end of an in-process pair link (the simulator's NIC port).
pub struct LoopbackPort {
    outbound: Rc<RefCell<VecDeque<Vec<u8>>>>,
    inbound: Rc<RefCell<VecDeque<Vec<u8>>>>,
    partitioned: bool,
    counters: PhyCounters,
}

impl LoopbackPort {
    /// Cuts or restores the link at this end. Partitioned, the port
    /// discards outbound frames and delivers no inbound frames, so the
    /// peer's link is silent in both directions.
    pub fn set_partitioned(&mut self, partitioned: bool) {
        self.partitioned = partitioned;
    }

    /// Whether the link at this end is currently cut.
    pub fn is_partitioned(&self) -> bool {
        self.partitioned
    }
}

impl NicPort for LoopbackPort {
    fn capabilities(&self) -> PortCapabilities {
        // A memory link has no hardware to advertise: the binding records
        // the guarantee level it can honestly prove.
        PortCapabilities::default()
    }

    fn counters(&self) -> PhyCounters {
        self.counters
    }

    fn send(&mut self, frame: &[u8]) -> Result<(), PortError> {
        if self.partitioned {
            self.counters.frames_dropped += 1;
            return Ok(());
        }
        self.counters.frames_sent += 1;
        self.outbound.borrow_mut().push_back(frame.to_vec());
        Ok(())
    }

    fn poll(&mut self) -> Option<(IngressTimestamp, Vec<u8>)> {
        if self.partitioned {
            self.inbound.borrow_mut().clear();
            return None;
        }
        let frame = self.inbound.borrow_mut().pop_front()?;
        self.counters.frames_received += 1;
        // No clock on a memory link.
        Some((IngressTimestamp::NONE, frame))
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::hal::NicPort;

    /// Drains every pending inbound frame of `port`, returning the payloads.
    fn drain(port: &mut LoopbackPort) -> Vec<Vec<u8>> {
        let mut frames = Vec::new();
        while let Some((_, frame)) = port.poll() {
            frames.push(frame);
        }
        frames
    }

    #[test]
    fn loopback_when_frame_sent_then_peer_receives_it() {
        let (mut a, mut b) = loopback_pair();

        a.send(&[1, 2, 3]).unwrap();
        let frames = drain(&mut b);

        assert_eq!(frames, vec![vec![1, 2, 3]]);
        assert_eq!(a.counters().frames_sent, 1);
        assert_eq!(b.counters().frames_received, 1);
    }

    #[test]
    fn loopback_when_queried_then_no_hardware_capabilities() {
        let (a, _) = loopback_pair();

        let capabilities = a.capabilities();

        assert!(!capabilities.hardware_timestamp);
        assert!(!capabilities.irq);
        assert!(!capabilities.dma);
    }

    #[test]
    fn loopback_when_partitioned_then_outbound_discarded_and_inbound_silent() {
        let (mut a, mut b) = loopback_pair();

        a.set_partitioned(true);
        a.send(&[1]).unwrap();
        assert_eq!(a.counters().frames_dropped, 1);
        assert!(drain(&mut b).is_empty());

        b.send(&[2]).unwrap();
        assert!(drain(&mut a).is_empty());
        assert_eq!(b.counters().frames_sent, 1);
    }

    #[test]
    fn loopback_when_reconnected_then_frames_flow_again() {
        let (mut a, mut b) = loopback_pair();
        a.set_partitioned(true);
        a.send(&[1]).unwrap();
        a.set_partitioned(false);

        a.send(&[2]).unwrap();

        assert_eq!(drain(&mut b), vec![vec![2]]);
    }
}
