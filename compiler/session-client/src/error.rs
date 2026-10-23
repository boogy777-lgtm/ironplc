//! The ways a session fails, as types a caller matches on.
//!
//! A failure of the device's own, a refusal that carries a V-code, is
//! [`SessionError::Device`] and holds the [`CommandError`] of the protocol
//! unchanged, so the code, the message and the type-change pairs of a V4010
//! reach the caller as the device wrote them. The other variants are failures of
//! the connection, which the device cannot have coded: the channel did not open,
//! nothing arrived in time, the stream broke, or it ended.

use std::fmt;
use std::time::Duration;

use ironplc_runtime::CommandError;

/// The step of a session that a timeout cut short. The limit of each step is a
/// constant of the timeout policy (`ironplc_runtime::frame`).
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Phase {
    /// Opening the channel to the device.
    Connect,
    /// The `identity` round trip that follows the open.
    Identity,
    /// The answer to a command.
    Response,
}

impl fmt::Display for Phase {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.write_str(match self {
            Phase::Connect => "opening the connection",
            Phase::Identity => "the identity handshake",
            Phase::Response => "the answer to a command",
        })
    }
}

/// Why a session could not open or a command could not be answered.
#[derive(Debug)]
pub enum SessionError {
    /// The channel to the device did not open: the connection method that was
    /// tried and what went wrong (a program that does not start, a refused
    /// connection, an address that does not resolve).
    Open {
        /// The connection method (`stdio` or `tcp`).
        method: &'static str,
        /// What failed.
        message: String,
    },
    /// A step took longer than its limit.
    TimedOut {
        /// The step that was cut short.
        phase: Phase,
        /// The limit it exceeded.
        limit: Duration,
    },
    /// The device answered with an error that carries a V-code. This includes
    /// the refusals of the transport: V6014 for a device that already serves a
    /// session, and V6013 for a stream the device found malformed. A refusal is
    /// an answer, not a fault of the connection: the session stays usable
    /// unless the device closed it, as it does after V6013 and V6014.
    Device(CommandError),
    /// The device could not read the line as a command and answered with the
    /// error that has no code (ADR-0055). The session stays usable.
    Codec {
        /// What the device said.
        message: String,
    },
    /// The device answered with something that is not part of the protocol, or
    /// its stream is broken.
    Protocol {
        /// What is wrong.
        message: String,
    },
    /// The device speaks a newer session protocol than this client reads.
    ProtocolTooNew {
        /// The protocol version the device reported.
        device: u32,
        /// The highest version this client supports.
        supported: u32,
    },
    /// The connection ended: the device closed it, was stopped, or the network
    /// dropped it.
    Closed {
        /// What the device said last, when it said something (the error output
        /// of a program that exited at start).
        detail: Option<String>,
    },
}

impl SessionError {
    /// The V-code of the device's refusal, for the variant that has one.
    pub fn v_code(&self) -> Option<&str> {
        match self {
            SessionError::Device(error) => Some(error.v_code()),
            _ => None,
        }
    }
}

impl fmt::Display for SessionError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            SessionError::Open { method, message } => {
                write!(f, "unable to open the {method} connection: {message}")
            }
            SessionError::TimedOut { phase, limit } => {
                write!(f, "{phase} took longer than {} ms", limit.as_millis())
            }
            SessionError::Device(error) => write!(f, "{error}"),
            SessionError::Codec { message } => {
                write!(f, "the device could not read the command: {message}")
            }
            SessionError::Protocol { message } => write!(f, "protocol error: {message}"),
            SessionError::ProtocolTooNew { device, supported } => write!(
                f,
                "the device speaks session protocol {device}, this client supports up to {supported}"
            ),
            SessionError::Closed { detail: None } => f.write_str("the connection was closed"),
            SessionError::Closed {
                detail: Some(detail),
            } => write!(f, "the connection was closed: {detail}"),
        }
    }
}

impl std::error::Error for SessionError {}
