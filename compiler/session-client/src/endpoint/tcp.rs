//! The `tcp` connection method: a socket to the device at an address and a
//! port, one length-prefixed frame per message (`ironplc_runtime::frame`).
//!
//! The device is not started by this client, and a lost connection is not
//! retried here: a caller that wants to retry opens a new session.

use std::io::{self, ErrorKind, Read};
use std::net::{Shutdown, TcpStream, ToSocketAddrs};
use std::time::{Duration, Instant};

use ironplc_runtime::frame::{is_framing_error, read_frame, write_frame, CLOSE_LINGER};

use super::Method;
use crate::channel::{Channel, ChannelError};
use crate::error::SessionError;

/// A device that listens on the network.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct TcpEndpoint {
    /// An IPv4 address, an IPv6 literal or a host name.
    pub address: String,
    /// The session port of the device (`ironplc_runtime::DEFAULT_SESSION_PORT`
    /// unless the device was told another).
    pub port: u16,
}

impl Method for TcpEndpoint {
    fn open(&self, connect_timeout: Duration) -> Result<Box<dyn Channel>, SessionError> {
        let open_failed = |message: String| SessionError::Open {
            method: "tcp",
            message,
        };
        // Resolving a name is not bounded by the connect timeout: the standard
        // library offers no way to bound it.
        let addresses = (self.address.as_str(), self.port)
            .to_socket_addrs()
            .map_err(|err| {
                open_failed(format!(
                    "unable to resolve {}:{}: {err}",
                    self.address, self.port
                ))
            })?;
        let mut last_failure = None;
        for address in addresses {
            match TcpStream::connect_timeout(&address, connect_timeout) {
                Ok(stream) => {
                    // A message is one small frame: do not hold it back for more.
                    stream.set_nodelay(true).map_err(|err| {
                        open_failed(format!("unable to configure the socket: {err}"))
                    })?;
                    return Ok(Box::new(TcpChannel {
                        stream,
                        is_closed: false,
                    }));
                }
                Err(err) if err.kind() == ErrorKind::TimedOut => {
                    return Err(SessionError::TimedOut {
                        phase: crate::error::Phase::Connect,
                        limit: connect_timeout,
                    });
                }
                Err(err) => last_failure = Some(format!("{address}: {err}")),
            }
        }
        Err(open_failed(last_failure.unwrap_or_else(|| {
            format!("{}:{} has no address", self.address, self.port)
        })))
    }
}

/// The connected socket.
struct TcpChannel {
    stream: TcpStream,
    is_closed: bool,
}

/// Whether the error says the other end is gone, as opposed to a stream that
/// carries something wrong.
fn is_connection_gone(err: &io::Error) -> bool {
    matches!(
        err.kind(),
        ErrorKind::ConnectionReset
            | ErrorKind::ConnectionAborted
            | ErrorKind::BrokenPipe
            | ErrorKind::NotConnected
            | ErrorKind::UnexpectedEof
    )
}

impl Channel for TcpChannel {
    fn send(&mut self, line: &str) -> Result<(), ChannelError> {
        write_frame(&mut self.stream, line).map_err(|err| {
            if is_connection_gone(&err) {
                ChannelError::Closed(None)
            } else {
                ChannelError::Broken(err.to_string())
            }
        })
    }

    fn receive(&mut self, timeout: Duration) -> Result<String, ChannelError> {
        self.stream.set_read_timeout(Some(timeout)).map_err(|err| {
            ChannelError::Broken(format!("unable to set the read timeout: {err}"))
        })?;
        match read_frame(&mut self.stream) {
            Ok(Some(line)) => Ok(line),
            Ok(None) => Err(ChannelError::Closed(None)),
            Err(err) if is_framing_error(&err) => Err(ChannelError::Broken(err.to_string())),
            Err(err) if matches!(err.kind(), ErrorKind::WouldBlock | ErrorKind::TimedOut) => {
                Err(ChannelError::TimedOut)
            }
            Err(err) if is_connection_gone(&err) => Err(ChannelError::Closed(None)),
            Err(err) => Err(ChannelError::Broken(err.to_string())),
        }
    }

    fn close(&mut self) {
        if self.is_closed {
            return;
        }
        self.is_closed = true;
        // INVARIANT (the other site: `serve_accepted` in
        // `compiler/vm-cli/src/tcp.rs`): the device releases its occupied flag
        // BEFORE it drops the socket, so the end of its output, read here,
        // means the slot is free and the next connection is not refused. A
        // device that never closes is left after `CLOSE_LINGER`, the same
        // bound the device puts on a client.
        if let Err(err) = self.stream.shutdown(Shutdown::Write) {
            log::debug!("unable to end the output of the session connection: {err}");
            return;
        }
        let deadline = Instant::now() + CLOSE_LINGER;
        let mut buffer = [0u8; 4096];
        loop {
            let remaining = deadline.saturating_duration_since(Instant::now());
            if remaining.is_zero() {
                log::debug!("the device did not close the session connection in time");
                return;
            }
            if self.stream.set_read_timeout(Some(remaining)).is_err() {
                return;
            }
            // Whatever the device still sends is discarded; the end of its
            // output (or an error: reset, timeout) ends the wait.
            match self.stream.read(&mut buffer) {
                Ok(0) | Err(_) => return,
                Ok(_) => {}
            }
        }
    }
}

impl Drop for TcpChannel {
    fn drop(&mut self) {
        self.close();
    }
}
