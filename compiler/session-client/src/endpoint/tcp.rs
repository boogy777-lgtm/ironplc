//! The `tcp` connection method: a socket to the device at an address and a
//! port, one length-prefixed frame per message (`ironplc_runtime::frame`).
//!
//! The device is not started by this client, and a lost connection is not
//! retried here: a caller that wants to retry opens a new session.

use std::io::{self, ErrorKind};
use std::net::{TcpStream, ToSocketAddrs};
use std::time::Duration;

use ironplc_runtime::frame::{is_framing_error, read_frame, write_frame};

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
                    return Ok(Box::new(TcpChannel { stream }));
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
}
