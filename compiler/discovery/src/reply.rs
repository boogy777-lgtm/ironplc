//! The reply: the public identity of a device, and its encoding.

use crate::error::{DecodeError, EncodeError};
use crate::{KIND_REPLY, MAGIC, MAX_REPLY_LEN, REQUEST_LEN, WIRE_VERSION};

/// Bit 0 of the flags byte: the device requires credentials.
const FLAG_PROTECTED: u8 = 0b0000_0001;

/// Length of the device id on the wire: 32 hexadecimal digits are 16 bytes.
const DEVICE_ID_LEN: usize = 16;
/// Header, session protocol (`u32`), session port (`u16`) and device id.
const FIXED_LEN: usize = REQUEST_LEN + 4 + 2 + DEVICE_ID_LEN;
/// Bytes of the `u16` length in front of each string.
const STRING_PREFIX_LEN: usize = 2;

/// What a device says about itself to anyone on the network.
///
/// Public identity only: nothing about the application, nothing about whether
/// the session is busy, no secret (ADR-0084). The strings are borrowed (from
/// the received datagram when decoding, from the device's own settings when
/// encoding), so the crate needs no allocator.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Reply<'a> {
    /// Version of the session protocol the device speaks.
    pub protocol: u32,
    /// TCP port of the session.
    pub session_port: u16,
    /// The device id as 16 raw bytes (the 32 hexadecimal digits of the id).
    pub device_id: [u8; DEVICE_ID_LEN],
    /// Whether the session requires credentials.
    pub protected: bool,
    /// The device name.
    pub name: &'a str,
    /// The device model.
    pub model: &'a str,
    /// The runtime version; public for a protected device too.
    pub runtime_version: &'a str,
}

impl Reply<'_> {
    /// The length of the encoded reply.
    fn encoded_len(&self) -> usize {
        [self.name, self.model, self.runtime_version]
            .iter()
            .fold(FIXED_LEN, |len, s| {
                len.saturating_add(STRING_PREFIX_LEN)
                    .saturating_add(s.len())
            })
    }
}

/// Writes `reply` to the start of `out` and returns the number of bytes
/// written.
///
/// A reply that would exceed [`MAX_REPLY_LEN`] is [`EncodeError::TooLong`]
/// whatever the buffer, so a device cannot send a datagram that may fragment.
pub fn encode_reply(reply: &Reply<'_>, out: &mut [u8]) -> Result<usize, EncodeError> {
    let len = reply.encoded_len();
    if len > MAX_REPLY_LEN {
        return Err(EncodeError::TooLong { len });
    }
    let dest = out
        .get_mut(..len)
        .ok_or(EncodeError::BufferTooSmall { needed: len })?;

    let mut w = Writer { dest, pos: 0 };
    w.put(&MAGIC)?;
    w.put(&[WIRE_VERSION, KIND_REPLY])?;
    w.put(&[if reply.protected { FLAG_PROTECTED } else { 0 }, 0])?;
    w.put(&reply.protocol.to_le_bytes())?;
    w.put(&reply.session_port.to_le_bytes())?;
    w.put(&reply.device_id)?;
    for s in [reply.name, reply.model, reply.runtime_version] {
        w.put_str(s)?;
    }
    Ok(w.pos)
}

/// Reads a reply from one datagram.
///
/// Strict: any input that is not exactly a version-1 reply is an error naming
/// the first thing found wrong. The flag bits other than the protected bit and
/// the reserved byte are ignored, so a later version can define them without
/// breaking a version-1 reader.
pub fn decode_reply(bytes: &[u8]) -> Result<Reply<'_>, DecodeError> {
    if bytes.len() > MAX_REPLY_LEN {
        return Err(DecodeError::TooLong { len: bytes.len() });
    }

    // The one place the header is checked. The magic is compared on whatever
    // is there, so that a short datagram of something else is "not IronPLC"
    // and not "truncated".
    let magic = bytes.get(..MAGIC.len()).unwrap_or(bytes);
    if !MAGIC.starts_with(magic) {
        return Err(DecodeError::BadMagic);
    }
    let mut r = Reader { rest: bytes };
    r.take(MAGIC.len())?;
    let version = r.u8()?;
    if version != WIRE_VERSION {
        return Err(DecodeError::UnsupportedVersion(version));
    }
    let kind = r.u8()?;
    if kind != KIND_REPLY {
        return Err(DecodeError::UnexpectedKind(kind));
    }
    let flags = r.u8()?;
    r.u8()?; // reserved, ignored

    let protocol = u32::from_le_bytes(r.array()?);
    let session_port = u16::from_le_bytes(r.array()?);
    let device_id = r.array()?;
    let name = r.string()?;
    let model = r.string()?;
    let runtime_version = r.string()?;
    if !r.rest.is_empty() {
        return Err(DecodeError::TrailingBytes);
    }

    Ok(Reply {
        protocol,
        session_port,
        device_id,
        protected: flags & FLAG_PROTECTED != 0,
        name,
        model,
        runtime_version,
    })
}

/// Cursor over the output buffer, already cut to the encoded length.
struct Writer<'o> {
    dest: &'o mut [u8],
    pos: usize,
}

impl Writer<'_> {
    fn put(&mut self, bytes: &[u8]) -> Result<(), EncodeError> {
        let end = self.pos.saturating_add(bytes.len());
        let needed = self.dest.len();
        self.dest
            .get_mut(self.pos..end)
            .ok_or(EncodeError::BufferTooSmall { needed })?
            .copy_from_slice(bytes);
        self.pos = end;
        Ok(())
    }

    fn put_str(&mut self, s: &str) -> Result<(), EncodeError> {
        let len = u16::try_from(s.len()).map_err(|_| EncodeError::TooLong { len: s.len() })?;
        self.put(&len.to_le_bytes())?;
        self.put(s.as_bytes())
    }
}

/// Cursor over the input.
struct Reader<'a> {
    rest: &'a [u8],
}

impl<'a> Reader<'a> {
    fn take(&mut self, n: usize) -> Result<&'a [u8], DecodeError> {
        let (head, tail) = self
            .rest
            .split_at_checked(n)
            .ok_or(DecodeError::Truncated)?;
        self.rest = tail;
        Ok(head)
    }

    fn u8(&mut self) -> Result<u8, DecodeError> {
        Ok(u8::from_le_bytes(self.array()?))
    }

    fn array<const N: usize>(&mut self) -> Result<[u8; N], DecodeError> {
        self.take(N)?.try_into().map_err(|_| DecodeError::Truncated)
    }

    fn string(&mut self) -> Result<&'a str, DecodeError> {
        let len = usize::from(u16::from_le_bytes(self.array()?));
        let (head, tail) = self
            .rest
            .split_at_checked(len)
            .ok_or(DecodeError::StringPastEnd)?;
        self.rest = tail;
        core::str::from_utf8(head).map_err(|_| DecodeError::InvalidUtf8)
    }
}
