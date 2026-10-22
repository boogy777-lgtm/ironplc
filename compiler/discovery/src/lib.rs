//! The wire format of PLC discovery, shared by the device that answers and the
//! workstation service that asks (ADR-0084; the format is specified in
//! `specs/design/plc-discovery.md`).
//!
//! The workstation sends a fixed 8-byte request in a UDP datagram to the
//! session port; the device answers with a reply that holds its public
//! identity. This crate is only the encoding and decoding of those two
//! messages, with bounds checks. It opens no socket.
//!
//! The crate is `#![no_std]` without `alloc` and has no dependencies, so an
//! embedded device can use it (ADR-0010). It has no `std` feature: the normal
//! build is the `no_std` build.
//!
//! - **Class:** the messages of the discovery wire (request, reply), in
//!   `lib.rs` and `reply.rs`. A near case outside it: the session protocol
//!   (JSON over stdio or TCP), which this crate does not touch.
//! - **Inv:** `decode_reply(encode_reply(r))` is `r` for every valid reply;
//!   every strict prefix of an encoded reply fails to decode; no byte string
//!   makes a decoder panic; `is_request` accepts exactly one byte string.
//!   Checked by the tests in `tests.rs`.
//! - **N+1:** a new reply field is a new wire version (for a boolean, a new bit
//!   of the flags byte); a new message kind is one more `kind` value with its
//!   own encode and decode; the version check is in `decode_reply`, in one
//!   place.

#![no_std]

mod error;
mod reply;

pub use error::{DecodeError, EncodeError};
pub use reply::{decode_reply, encode_reply, Reply};

/// The first four bytes of every discovery message.
pub const MAGIC: [u8; 4] = *b"IPLD";

/// The wire version this crate encodes and decodes.
pub const WIRE_VERSION: u8 = 1;

/// The length of a request, which is also the length of the header common to
/// every message.
pub const REQUEST_LEN: usize = 8;

/// The longest reply, in bytes. A reply must fit one UDP datagram on any link
/// without fragmentation, so the encoder refuses a longer one and the decoder
/// does not parse a longer input. The device name alone may be 64 characters,
/// up to 256 UTF-8 bytes, hence the `u16` string lengths.
pub const MAX_REPLY_LEN: usize = 1024;

const KIND_REQUEST: u8 = 1;
const KIND_REPLY: u8 = 2;

/// The request: the header with kind 1 and two zero bytes.
pub const fn request() -> [u8; REQUEST_LEN] {
    let [m0, m1, m2, m3] = MAGIC;
    [m0, m1, m2, m3, WIRE_VERSION, KIND_REQUEST, 0, 0]
}

/// Whether `bytes` is exactly the version-1 request.
///
/// The device answers nothing else: no reply and no error to anything
/// unexpected, because an answer to a stray datagram is an amplification and a
/// crash surface (ADR-0084 cites the Beckhoff advisory about one empty packet).
pub fn is_request(bytes: &[u8]) -> bool {
    bytes == request().as_slice()
}

#[cfg(test)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}

#[cfg(test)]
mod tests;
