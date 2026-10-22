//! The ways a discovery message is refused, on either side of the wire.

use core::fmt;

use crate::MAX_REPLY_LEN;

/// Why a byte string is not a version-1 discovery reply.
///
/// Every refusal is its own variant, so that a caller can tell "not IronPLC"
/// ([`DecodeError::BadMagic`]) from "a newer discovery"
/// ([`DecodeError::UnsupportedVersion`]) from a damaged datagram.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum DecodeError {
    /// The input is longer than [`MAX_REPLY_LEN`]; it is refused before it is
    /// parsed.
    TooLong { len: usize },
    /// The input does not begin with the magic: it is not an IronPLC message.
    BadMagic,
    /// The wire version is not the one this crate speaks. The device speaks
    /// another discovery, perhaps a newer one.
    UnsupportedVersion(u8),
    /// The message kind is not a reply (a request, or a kind this version does
    /// not define).
    UnexpectedKind(u8),
    /// The input ends inside the header, a fixed field or a length prefix.
    Truncated,
    /// A string length runs past the end of the input.
    StringPastEnd,
    /// A string is not UTF-8.
    InvalidUtf8,
    /// Bytes follow the last string.
    TrailingBytes,
}

impl fmt::Display for DecodeError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            DecodeError::TooLong { len } => write!(
                f,
                "discovery reply of {len} bytes is longer than the maximum of {MAX_REPLY_LEN}"
            ),
            DecodeError::BadMagic => f.write_str("not an IronPLC discovery message"),
            DecodeError::UnsupportedVersion(v) => {
                write!(f, "unsupported discovery wire version {v}")
            }
            DecodeError::UnexpectedKind(k) => {
                write!(f, "discovery message kind {k} is not a reply")
            }
            DecodeError::Truncated => f.write_str("discovery reply is truncated"),
            DecodeError::StringPastEnd => {
                f.write_str("a string length in the discovery reply runs past its end")
            }
            DecodeError::InvalidUtf8 => {
                f.write_str("a string in the discovery reply is not valid UTF-8")
            }
            DecodeError::TrailingBytes => {
                f.write_str("discovery reply has bytes after its last field")
            }
        }
    }
}

impl core::error::Error for DecodeError {}

/// Why a reply cannot be written to the wire.
///
/// The encoder never truncates: a reply that does not fit is an error.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum EncodeError {
    /// The encoded reply would be longer than [`MAX_REPLY_LEN`], the most that
    /// is guaranteed to fit one datagram.
    TooLong { len: usize },
    /// The output buffer holds fewer bytes than the encoded reply needs.
    BufferTooSmall { needed: usize },
}

impl fmt::Display for EncodeError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            EncodeError::TooLong { len } => write!(
                f,
                "discovery reply would be {len} bytes, more than the maximum of {MAX_REPLY_LEN}"
            ),
            EncodeError::BufferTooSmall { needed } => {
                write!(
                    f,
                    "buffer too small for the discovery reply: {needed} bytes needed"
                )
            }
        }
    }
}

impl core::error::Error for EncodeError {}
