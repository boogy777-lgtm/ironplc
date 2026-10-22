//! One engineering session with a device: open, the identity handshake, then
//! one command at a time.
//!
//! The session is the client half of `specs/design/engineering-connection.md`.
//! It sends the [`Command`] and reads the [`Response`] of
//! `ironplc_runtime`, the one definition of the protocol, and adds nothing to
//! it. Whatever transport carries the lines is a connection method
//! ([`Endpoint`]).
//!
//! A session has no state machine of its own beyond "open or not". It does not
//! retry and does not keep a heartbeat: a caller that holds a session for long
//! decides how to detect a lost device, and one that opens a session for one
//! command (a tool call) has no idle time to guard.

use std::time::Duration;

use ironplc_runtime::frame::{CONNECT_TIMEOUT, RESPONSE_TIMEOUT};
use ironplc_runtime::{
    parse_response, render_command, Command, IdentityPayload, Response, SESSION_PROTOCOL_VERSION,
};
use serde::Deserialize;

use crate::channel::{Channel, ChannelError};
use crate::endpoint::Endpoint;
use crate::error::{Phase, SessionError};

/// The limits of a session. The default is the timeout policy of the
/// engineering connection; a caller may change them, as a profile may.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct Timeouts {
    /// The limit of opening the channel, and again of the identity round trip.
    pub connect: Duration,
    /// The limit of the answer to a command.
    pub response: Duration,
}

impl Default for Timeouts {
    fn default() -> Self {
        Timeouts {
            connect: CONNECT_TIMEOUT,
            response: RESPONSE_TIMEOUT,
        }
    }
}

/// An open session with a device.
pub struct Session {
    channel: Box<dyn Channel>,
    timeouts: Timeouts,
    identity: Option<IdentityPayload>,
}

impl Session {
    /// Opens a session with the timeouts of the policy. See
    /// [`connect_with`](Self::connect_with).
    pub fn connect(endpoint: &Endpoint) -> Result<Session, SessionError> {
        Session::connect_with(endpoint, Timeouts::default())
    }

    /// Opens the channel of `endpoint` and sends `identity`, the first command
    /// on every opened channel.
    ///
    /// A device that already serves a session answers with its refusal instead
    /// (V6014), and that is returned as [`SessionError::Device`]. A device that
    /// predates `identity` answers it with the error that has no code, which is
    /// the version negotiation of the protocol: the session opens, speaks the
    /// hot-edit commands only, and [`identity`](Self::identity) is `None`. A
    /// device that speaks a newer protocol than this client reads is refused
    /// ([`SessionError::ProtocolTooNew`]).
    pub fn connect_with(endpoint: &Endpoint, timeouts: Timeouts) -> Result<Session, SessionError> {
        Session::start(endpoint.open_channel(timeouts.connect)?, timeouts)
    }

    /// Starts a session over a channel that is open: the handshake.
    fn start(channel: Box<dyn Channel>, timeouts: Timeouts) -> Result<Session, SessionError> {
        let mut session = Session {
            channel,
            timeouts,
            identity: None,
        };
        session.identity = session.handshake()?;
        Ok(session)
    }

    /// What the device said about itself and its application when the session
    /// opened; `None` for a device that predates the handshake.
    pub fn identity(&self) -> Option<&IdentityPayload> {
        self.identity.as_ref()
    }

    /// Sends one command and returns its answer.
    ///
    /// An answer that is an error is not returned as a `Response`: a coded error
    /// is [`SessionError::Device`] and the error without a code is
    /// [`SessionError::Codec`], so an `Ok` is an acknowledgement, a status or an
    /// identity.
    pub fn send(&mut self, command: &Command) -> Result<Response, SessionError> {
        self.exchange(command, self.timeouts.response, Phase::Response)
    }

    /// The identity round trip.
    fn handshake(&mut self) -> Result<Option<IdentityPayload>, SessionError> {
        match self.exchange(&Command::Identity, self.timeouts.connect, Phase::Identity) {
            Ok(Response::Identity(payload)) => {
                if payload.protocol > SESSION_PROTOCOL_VERSION {
                    return Err(SessionError::ProtocolTooNew {
                        device: payload.protocol,
                        supported: SESSION_PROTOCOL_VERSION,
                    });
                }
                Ok(Some(*payload))
            }
            Ok(other) => Err(SessionError::Protocol {
                message: format!("the device answered identity with {}", describe(&other)),
            }),
            // A device without `identity` does not know the command.
            Err(SessionError::Codec { .. }) => Ok(None),
            Err(err) => Err(err),
        }
    }

    /// Sends `command` and reads the answer within `limit`.
    fn exchange(
        &mut self,
        command: &Command,
        limit: Duration,
        phase: Phase,
    ) -> Result<Response, SessionError> {
        let line = render_command(command).map_err(|err| SessionError::Protocol {
            message: format!("unable to write the command: {err}"),
        })?;
        if let Err(sent) = self.channel.send(&line) {
            // A device that refuses the connection answers and closes before it
            // reads anything, so the refusal may be waiting even though the
            // write failed.
            return match self.channel.receive(limit) {
                Ok(answer) => decode(&answer),
                Err(_) => Err(to_session_error(sent, phase, limit)),
            };
        }
        match self.channel.receive(limit) {
            Ok(answer) => decode(&answer),
            Err(err) => Err(to_session_error(err, phase, limit)),
        }
    }
}

/// The name of a kind of response, for a message.
fn describe(response: &Response) -> &'static str {
    match response {
        Response::Identity(_) => "an identity",
        Response::Status(_) => "a status",
        Response::Ack => "an acknowledgement",
        Response::Error(_) => "an error",
    }
}

fn to_session_error(err: ChannelError, phase: Phase, limit: Duration) -> SessionError {
    match err {
        ChannelError::TimedOut => SessionError::TimedOut { phase, limit },
        ChannelError::Closed(detail) => SessionError::Closed { detail },
        ChannelError::Broken(message) => SessionError::Protocol { message },
    }
}

/// The error line that has no code, which a device answers to a line it cannot
/// read as a command (ADR-0055). Writers: `codec_error_line` in
/// `compiler/vm-cli/src/serve.rs` and the shell of IronLink, with `lCode`.
#[derive(Deserialize)]
struct CodelessError {
    response: String,
    #[serde(rename = "vCode")]
    v_code: Option<String>,
    message: String,
}

/// Reads one answer line.
fn decode(line: &str) -> Result<Response, SessionError> {
    match parse_response(line) {
        Ok(Response::Error(error)) => Err(SessionError::Device(error)),
        Ok(response) => Ok(response),
        Err(err) => match serde_json::from_str::<CodelessError>(line) {
            Ok(codeless) if codeless.response == "error" && codeless.v_code.is_none() => {
                Err(SessionError::Codec {
                    message: codeless.message,
                })
            }
            _ => Err(SessionError::Protocol {
                message: format!("unable to read the answer of the device: {err}"),
            }),
        },
    }
}

#[cfg(test)]
mod tests;
