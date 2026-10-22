//! The answer of the device to discovery: with `--listen`, the device also
//! listens on UDP at the address and port of its TCP session, and answers the
//! fixed 8-byte request of the workstation with its public identity (ADR-0084;
//! the bytes are specified in `specs/design/plc-discovery.md` and encoded by
//! the `ironplc-discovery` crate, which opens no socket).
//!
//! - **Class:** what the device answers to the network outside a session. Today
//!   one member, the discovery reply. A near case outside it: the session
//!   protocol on TCP ([`crate::tcp`]), which needs a connection and has its own
//!   framing.
//! - **Inv:** the reply is built only here ([`encode_identity`]), from
//!   [`DeviceIdentity`] (the one function that builds it, the session `identity`
//!   answer uses the same) and the two session constants; only a datagram that
//!   [`is_request`] gets an answer, and everything else gets neither a reply nor
//!   an error; replies per source and in total never exceed the limits of
//!   [`rate`]. Checked by the tests in `discovery/tests.rs` and `rate.rs`.
//! - **N+1:** a later device setting "discovery on or off" or "discovery port"
//!   changes only the decision whether and where [`bind`] is called, in
//!   `serve_tcp`; a later source for the `protected` flag changes the one field
//!   in [`encode_identity`].
//!
//! The responder runs on its own thread and reads the identity per request from
//! an [`IdentityView`], never from a copy, so an online name change shows in the
//! next reply. It cannot read the `Device` itself: the session thread holds that
//! for as long as its session lasts, and a busy device must answer discovery too.

mod rate;
#[cfg(test)]
mod tests;

use std::fmt;
use std::net::{IpAddr, SocketAddr, UdpSocket};
use std::time::Instant;

use ironplc_discovery::{encode_reply, is_request, EncodeError, Reply, MAX_REPLY_LEN, REQUEST_LEN};
use ironplc_runtime::{DeviceIdentity, SESSION_PROTOCOL_VERSION};

use crate::device_settings::device::IdentityView;
use crate::device_settings::device_id_bytes;
use crate::error::{self, VmError};
use rate::RateLimiter;

/// Whether `serve --listen` answers discovery. Chosen in one place: the
/// `--no-discovery` flag today, a device setting later.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum Discovery {
    /// Answer discovery requests on the UDP port of the session.
    Answer,
    /// Do not open the discovery port.
    Off,
}

/// Binds the discovery socket to `addr`, the address and port of the TCP
/// session. V6017 when it cannot: a device that cannot be found without a word
/// is worse than one that says so, as with the TCP bind.
pub(crate) fn bind(addr: SocketAddr) -> Result<UdpSocket, VmError> {
    UdpSocket::bind(addr).map_err(|err| {
        VmError::io(
            error::DISCOVERY_BIND,
            format!(
                "unable to bind the discovery port {addr} (UDP): {err}. \
                 Free the port, or run without discovery with --no-discovery"
            ),
        )
    })
}

/// Answers discovery on `socket` from a thread of its own, for as long as the
/// process runs. The reply names `session_port`, the TCP port of the session.
pub(crate) fn spawn(
    socket: UdpSocket,
    identity: IdentityView,
    session_port: u16,
) -> Result<(), VmError> {
    let responder = Responder::new(identity, session_port);
    std::thread::Builder::new()
        .name("discovery".to_string())
        .spawn(move || answer_datagrams(&socket, responder))
        .map(|_| ())
        .map_err(|err| {
            VmError::io(
                error::SESSION_IO,
                format!("unable to spawn the discovery thread: {err}"),
            )
        })
}

/// Why no reply can be built from an identity.
#[derive(Debug, PartialEq, Eq)]
enum ReplyError {
    /// The identity has no device id. The device of `serve` always has one.
    NoDeviceId,
    /// The device id is not 32 lowercase hexadecimal digits. The row of the
    /// settings refuses such an id, so this cannot happen either.
    MalformedDeviceId,
    /// The encoder refused the reply.
    Encode(EncodeError),
}

impl fmt::Display for ReplyError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            ReplyError::NoDeviceId => f.write_str("the device has no device id"),
            ReplyError::MalformedDeviceId => {
                f.write_str("the device id is not 32 lowercase hexadecimal digits")
            }
            ReplyError::Encode(err) => write!(f, "the reply cannot be encoded: {err}"),
        }
    }
}

/// Encodes the reply of the device with `identity` into `out` and returns its
/// length: the one place the discovery reply is built.
///
/// The cases of [`ReplyError`] cannot happen for the identity of the `serve`
/// device (it always has a checked id, and its strings are short); they are
/// errors rather than panics or defaults so that a device that ever gets there
/// says so once and stays silent, instead of announcing an id it does not have.
fn encode_identity(
    identity: &DeviceIdentity,
    session_port: u16,
    out: &mut [u8],
) -> Result<usize, ReplyError> {
    let id = identity
        .device_id
        .as_deref()
        .ok_or(ReplyError::NoDeviceId)?;
    let device_id = device_id_bytes(id).ok_or(ReplyError::MalformedDeviceId)?;
    let reply = Reply {
        protocol: SESSION_PROTOCOL_VERSION,
        session_port,
        device_id,
        // No session credentials exist yet (ADR-0063 keeps wire authentication
        // deferred), so every device is open. When credentials land, this
        // field reads from them.
        protected: false,
        name: &identity.name,
        model: &identity.model,
        runtime_version: &identity.runtime_version,
    };
    encode_reply(&reply, out).map_err(ReplyError::Encode)
}

/// The decisions of the responder, apart from the socket: which datagram gets
/// which reply.
struct Responder {
    identity: IdentityView,
    session_port: u16,
    limiter: RateLimiter,
    /// Whether the failure to build a reply was logged already, so that a
    /// stream of requests cannot fill the log.
    reported_reply_failure: bool,
}

impl Responder {
    fn new(identity: IdentityView, session_port: u16) -> Self {
        Responder {
            identity,
            session_port,
            limiter: RateLimiter::new(),
            reported_reply_failure: false,
        }
    }

    /// The reply to `datagram` from `source` at `now`, encoded into `out`:
    /// its length, or `None` when the device stays silent (not a request, over
    /// the rate, or no reply can be built).
    fn answer(
        &mut self,
        datagram: &[u8],
        source: IpAddr,
        now: Instant,
        out: &mut [u8],
    ) -> Option<usize> {
        if !is_request(datagram) {
            log::debug!(
                "ignoring a datagram of {} bytes from {source}: not a discovery request",
                datagram.len()
            );
            return None;
        }
        if !self.limiter.allow(source, now) {
            log::debug!("ignoring a discovery request from {source}: over the rate limit");
            return None;
        }
        match encode_identity(&self.identity.identity(), self.session_port, out) {
            Ok(len) => Some(len),
            Err(err) => {
                if !self.reported_reply_failure {
                    self.reported_reply_failure = true;
                    log::warn!("discovery requests are not answered: {err}");
                }
                None
            }
        }
    }
}

/// The loop of the discovery thread: one datagram, at most one reply.
fn answer_datagrams(socket: &UdpSocket, mut responder: Responder) {
    // One byte more than a request, so that a longer datagram is seen as longer
    // and is not mistaken for a request.
    let mut datagram = [0u8; REQUEST_LEN + 1];
    let mut reply = [0u8; MAX_REPLY_LEN];
    loop {
        let (len, source) = match socket.recv_from(&mut datagram) {
            Ok(received) => received,
            // Whatever goes wrong with one datagram is that datagram's
            // problem: Windows reports a datagram longer than the buffer as an
            // error, and a reset (an earlier reply reached a closed port) also
            // arrives here.
            Err(err) => {
                log::debug!("ignoring a datagram that could not be received: {err}");
                continue;
            }
        };
        let Some(received) = datagram.get(..len) else {
            continue;
        };
        let Some(reply_len) = responder.answer(received, source.ip(), Instant::now(), &mut reply)
        else {
            continue;
        };
        let Some(bytes) = reply.get(..reply_len) else {
            continue;
        };
        if let Err(err) = socket.send_to(bytes, source) {
            log::debug!("unable to send the discovery reply to {source}: {err}");
        }
    }
}
