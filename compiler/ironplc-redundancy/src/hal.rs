//! The transport seam: one network port as the redundancy layer sees it.
//!
//! This is the `NicPort` interface declared by the architecture doc
//! ("Interfaces"): implementations are target-side drivers (the EtherNet/IP
//! binding later; the loopback simulator binding in
//! [`crate::loopback`]), and the redundancy layer never touches registers.
//! A protocol swap changes the binding and its capability descriptor,
//! never the FSM ("Protocol Portability").

use core::fmt;

/// One network port as the redundancy layer sees it.
///
/// Framed datagrams in, framed datagrams out: the ping/pong exchange and
/// the future crossload segments are codec details inside
/// [`crate::liveness`] / `crossload`, not part of this interface.
pub trait NicPort {
    /// Advertised per-port capabilities (the calibration pipeline of
    /// ADR-0062 consumes these; a binding records what it can prove).
    fn capabilities(&self) -> PortCapabilities;
    /// Uniform PHY/error counters, exposed for diagnostics.
    fn counters(&self) -> PhyCounters;
    /// Queues one frame for transmission on the port.
    fn send(&mut self, frame: &[u8]) -> Result<(), PortError>;
    /// Takes the next received frame, if any. The timestamp is the
    /// ingress time when the port has a clock; `IngressTimestamp::NONE`
    /// otherwise.
    fn poll(&mut self) -> Option<(IngressTimestamp, Vec<u8>)>;
}

/// Per-port capability advertisement (timestamp/IRQ/DMA), recorded per
/// binding so a protocol swap surfaces the guarantee level it can prove.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub struct PortCapabilities {
    /// The port stamps ingress frames with a hardware clock.
    pub hardware_timestamp: bool,
    /// The port raises an interrupt on frame arrival (vs. pure polling).
    pub irq: bool,
    /// The port transfers frames by DMA (vs. CPU copies).
    pub dma: bool,
}

/// Uniform PHY/error counters per port. Operational diagnostics only —
/// never an arbitration input (the detection case table and the fencing
/// chain arbitrate, per the FSM spec).
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub struct PhyCounters {
    /// Frames accepted for transmission.
    pub frames_sent: u64,
    /// Frames delivered to the receiver.
    pub frames_received: u64,
    /// Frames the port dropped (a partitioned link, a full queue).
    pub frames_dropped: u64,
}

/// Ingress timestamp in microseconds from the port's clock; `NONE` when
/// the port has no timestamping capability (the loopback binding).
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub struct IngressTimestamp(pub u64);

impl IngressTimestamp {
    /// The value a port without a clock reports.
    pub const NONE: Self = Self(0);
}

/// Why a frame could not be sent on a port.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum PortError {
    /// The port's link is closed; nothing can be transmitted.
    Closed,
}

impl fmt::Display for PortError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            PortError::Closed => write!(f, "the port's link is closed"),
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn port_error_display_when_closed_then_names_the_link() {
        assert_eq!(PortError::Closed.to_string(), "the port's link is closed");
    }

    #[test]
    fn ingress_timestamp_when_none_then_zero() {
        assert_eq!(IngressTimestamp::NONE, IngressTimestamp(0));
    }
}
