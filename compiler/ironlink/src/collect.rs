//! The collection every driver shares: send the request, wait for replies until
//! the deadline, and turn each datagram into an outcome.
//!
//! This is the only place IronLink sends a datagram and the only place it reads
//! one, so the two rules of the tool are checked here once:
//!
//! - what is sent is `ironplc_discovery::request()` and nothing else (IronLink
//!   never writes to a controller, ADR-0084);
//! - a [`Heard`] outcome is made only from a datagram that `decode_reply`
//!   examined. A datagram that does not decode is counted and logged at debug
//!   level and is never an error, because anyone on the network can send one.
//!
//! The request is sent `attempts` times, the n-th at n times
//! [`ATTEMPT_SPACING`] after the first and only if that moment is before the
//! deadline. The spacing is longer than the 250 ms in which a device answers one
//! request per source (`ironplcvm`'s rate limit), so a datagram lost on the way is
//! asked again without the retry being rate-limited away. Replies are read until
//! the deadline, except that [`Stop::OnFirstOutcome`] ends the wait at the first
//! outcome (identify asks one route and wants one answer).

use std::io;
use std::net::{SocketAddr, UdpSocket};
use std::time::{Duration, Instant};

use ironplc_discovery::{decode_reply, request, DecodeError, MAX_REPLY_LEN};

use crate::driver::DriverKind;
use crate::error::IronLinkError;
use crate::observation::{merge, now_ms, Heard, Observation, OtherVersion};
use crate::parallel::map_in_parallel;

/// The shortest time between two sends of the request to the same destination.
pub(crate) const ATTEMPT_SPACING: Duration = Duration::from_millis(300);

/// The default time to wait for replies, in milliseconds.
pub(crate) const DEFAULT_TIMEOUT_MS: u64 = 1000;
/// The longest time to wait that a command may ask for, in milliseconds.
pub(crate) const MAX_TIMEOUT_MS: u64 = 60_000;
/// The default number of times the request is sent.
pub(crate) const DEFAULT_ATTEMPTS: u32 = 2;
/// The most times a command may ask the request to be sent.
pub(crate) const MAX_ATTEMPTS: u32 = 10;

/// When the wait for replies ends before the deadline, if it does.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum Stop {
    /// Wait until the deadline: a browse cannot know how many devices there are.
    AtDeadline,
    /// Also stop at the first outcome: an identify asks for one answer.
    OnFirstOutcome,
}

/// How long and how often to ask.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) struct Window {
    pub(crate) timeout: Duration,
    pub(crate) attempts: u32,
    pub(crate) stop: Stop,
}

impl Window {
    /// The window of a command, from its optional options; the defaults are
    /// [`DEFAULT_TIMEOUT_MS`] and [`DEFAULT_ATTEMPTS`].
    pub(crate) fn from_options(
        timeout_ms: Option<u64>,
        attempts: Option<u32>,
        stop: Stop,
    ) -> Result<Window, IronLinkError> {
        let timeout_ms = timeout_ms.unwrap_or(DEFAULT_TIMEOUT_MS);
        if !(1..=MAX_TIMEOUT_MS).contains(&timeout_ms) {
            return Err(IronLinkError::argument_invalid(format!(
                "the timeout must be between 1 and {MAX_TIMEOUT_MS} ms, not {timeout_ms}"
            )));
        }
        let attempts = attempts.unwrap_or(DEFAULT_ATTEMPTS);
        if !(1..=MAX_ATTEMPTS).contains(&attempts) {
            return Err(IronLinkError::argument_invalid(format!(
                "the number of attempts must be between 1 and {MAX_ATTEMPTS}, not {attempts}"
            )));
        }
        Ok(Window {
            timeout: Duration::from_millis(timeout_ms),
            attempts,
            stop,
        })
    }

    /// The timeout in milliseconds, as the user gave it.
    pub(crate) fn timeout_ms(&self) -> u64 {
        u64::try_from(self.timeout.as_millis()).unwrap_or(u64::MAX)
    }
}

/// A socket and the destinations the request is sent to from it.
pub(crate) struct Channel {
    pub(crate) socket: UdpSocket,
    pub(crate) destinations: Vec<SocketAddr>,
}

/// What the channels of a driver heard.
#[derive(Debug, Default, PartialEq, Eq)]
pub(crate) struct Collected {
    /// One outcome for each source, with the latest time.
    pub(crate) heard: Vec<Heard>,
    /// Datagrams that were not accepted as discovery replies, or could not be
    /// received.
    pub(crate) undecodable: u32,
}

impl Collected {
    fn absorb(&mut self, other: Collected) {
        for heard in other.heard {
            merge(&mut self.heard, heard);
        }
        self.undecodable = self.undecodable.saturating_add(other.undecodable);
    }
}

/// Sends the request on every channel and collects the outcomes within
/// `window`, for `driver`. The channels wait at the same time.
pub(crate) fn collect(
    driver: DriverKind,
    channels: &[Channel],
    window: &Window,
) -> Result<Collected, IronLinkError> {
    let per_channel = map_in_parallel(channels, |channel| listen(driver, channel, window))?;
    let mut all = Collected::default();
    for collected in per_channel {
        all.absorb(collected);
    }
    Ok(all)
}

/// Sends the request to every destination of `channel`.
fn send_request(channel: &Channel) -> Result<(), IronLinkError> {
    let request = request();
    for destination in &channel.destinations {
        channel
            .socket
            .send_to(&request, destination)
            .map_err(|err| IronLinkError::send_failed(*destination, &err))?;
    }
    Ok(())
}

/// The moment the `n`-th send is due, `n` counted from 0.
fn send_due(start: Instant, n: u32) -> Instant {
    start + ATTEMPT_SPACING * n
}

/// One channel: the sends and the reads until the deadline.
fn listen(
    driver: DriverKind,
    channel: &Channel,
    window: &Window,
) -> Result<Collected, IronLinkError> {
    let start = Instant::now();
    let deadline = start + window.timeout;
    let mut sent = 0;
    let mut collected = Collected::default();
    // One byte more than the longest reply, so that a longer datagram is seen as
    // longer and refused by `decode_reply`, and not cut to a valid length.
    let mut buffer = [0u8; MAX_REPLY_LEN + 1];

    loop {
        let now = Instant::now();
        while sent < window.attempts {
            let due = send_due(start, sent);
            if due > now || due >= deadline {
                break;
            }
            send_request(channel)?;
            sent += 1;
        }
        if now >= deadline {
            return Ok(collected);
        }
        let next_send = (sent < window.attempts)
            .then(|| send_due(start, sent))
            .filter(|due| *due < deadline);
        let wake = next_send.map_or(deadline, |due| due.min(deadline));
        // A read timeout of zero is an error; a millisecond is the shortest wait.
        let wait = wake
            .saturating_duration_since(now)
            .max(Duration::from_millis(1));
        if let Err(err) = channel.socket.set_read_timeout(Some(wait)) {
            return Err(IronLinkError::internal(format!(
                "unable to set the read timeout of a socket: {err}"
            )));
        }

        match channel.socket.recv_from(&mut buffer) {
            Ok((len, source)) => {
                let Some(datagram) = buffer.get(..len) else {
                    continue;
                };
                match decode_reply(datagram) {
                    Ok(reply) => merge(
                        &mut collected.heard,
                        Heard::Device(Observation::from_reply(
                            &reply,
                            driver,
                            source.ip(),
                            now_ms(),
                        )),
                    ),
                    Err(DecodeError::UnsupportedVersion(wire_version)) => merge(
                        &mut collected.heard,
                        Heard::OtherVersion(OtherVersion {
                            address: source.ip(),
                            wire_version,
                            seen_at: now_ms(),
                        }),
                    ),
                    Err(err) => {
                        log::debug!("ignoring a datagram of {len} bytes from {source}: {err}");
                        collected.undecodable = collected.undecodable.saturating_add(1);
                    }
                }
                if window.stop == Stop::OnFirstOutcome && !collected.heard.is_empty() {
                    return Ok(collected);
                }
            }
            Err(err) => match err.kind() {
                // The wait ended, or a signal ended it early: look at the clock.
                io::ErrorKind::WouldBlock
                | io::ErrorKind::TimedOut
                | io::ErrorKind::Interrupted => {}
                // Windows reports an earlier send to a closed port here. It is no
                // datagram, and nothing is lost by going on.
                io::ErrorKind::ConnectionReset => {
                    log::debug!("a destination refused an earlier datagram: {err}");
                }
                // Windows reports a datagram longer than the buffer as an error.
                _ => {
                    log::debug!("a datagram could not be received: {err}");
                    collected.undecodable = collected.undecodable.saturating_add(1);
                }
            },
        }
    }
}

#[cfg(test)]
mod tests;
