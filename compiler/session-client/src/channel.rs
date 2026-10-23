//! The channel a connection method opens: lines to the device and lines back.
//!
//! A channel knows nothing of the protocol. It carries one JSON line out and
//! brings one JSON line in, within a time, and says in three words why it
//! cannot: nothing came in time, the connection ended, or the stream is broken.
//! The envelope of the line (a newline, or a frame) is the business of the
//! connection method that opened it. The channel is public because the
//! transport is not the session protocol: a client of another protocol that is
//! also JSON lines, such as the shell of IronLink, opens the same `stdio`
//! channel and speaks its own commands on it.

use std::time::Duration;

/// Why a channel could not send or receive a line.
#[derive(Debug, PartialEq, Eq)]
pub enum ChannelError {
    /// Nothing arrived within the time given.
    TimedOut,
    /// The connection ended. The detail is what the other end said last, when
    /// the method can know it.
    Closed(Option<String>),
    /// The stream is broken or carries something that is not a line.
    Broken(String),
}

/// One open connection to a device.
pub trait Channel: Send {
    /// Sends one JSON line, which has no newline in it.
    fn send(&mut self, line: &str) -> Result<(), ChannelError>;

    /// Receives the next JSON line, without its newline, waiting at most
    /// `timeout`.
    fn receive(&mut self, timeout: Duration) -> Result<String, ChannelError>;
}
