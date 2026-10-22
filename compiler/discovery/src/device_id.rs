//! The id of a device: 128 bits, written as 32 lowercase hexadecimal digits that
//! the device generates once and keeps (ADR-0084).
//!
//! [`DeviceId`] is the one definition of a well-formed id. It is the 16 raw
//! bytes the discovery wire carries, and it is what everything else that names
//! a device holds: the only way to make one from text is [`DeviceId::parse`], so
//! a value of the type is an id a controller can report. The device, the
//! workstation service and the records of a project all use it, each mapping
//! [`DeviceIdError`] to its own code and message.
//!
//! why: the text form has exactly one spelling (lowercase, no separators), so
//! that two spellings of one id never name two devices and an id is safe as part
//! of a file name (digits and `a-f` only). The type needs no allocator, so the
//! device can use it too (ADR-0010).
//!
//! - **Class:** a device id, in its two forms, bytes and 32 digits. A near case
//!   outside it: the device name, which is text a person chooses and has no
//!   format of this kind.
//! - **Inv:** `parse(id.to_string())` is `id` for every id, and `parse` accepts
//!   the 32-digit lowercase text and nothing else. Checked by the tests in
//!   `device_id/tests.rs`.
//! - **N+1:** another spelling of the id (for example a serde format) is one
//!   more impl on this type; the parsing and the formatting are not repeated.

use core::fmt::{self, Write as _};
use core::str::FromStr;

/// The number of bytes of a device id (128 bits), see [`DeviceId::LEN`].
const BYTES: usize = 16;

/// The number of hexadecimal digits of a device id.
const DIGITS: usize = BYTES * 2;

/// A device id: 128 bits, whose text is 32 lowercase hexadecimal digits.
#[derive(Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord)]
pub struct DeviceId([u8; BYTES]);

impl DeviceId {
    /// The number of bytes of a device id (128 bits).
    pub const LEN: usize = BYTES;

    /// The id with these 16 bytes.
    pub const fn from_bytes(bytes: [u8; BYTES]) -> DeviceId {
        DeviceId(bytes)
    }

    /// The 16 bytes of the id, as the discovery wire carries them.
    pub const fn as_bytes(&self) -> &[u8; BYTES] {
        &self.0
    }

    /// The id written by `text`, or why `text` is not one.
    ///
    /// Accepts exactly 32 lowercase hexadecimal digits: not uppercase, not
    /// another length, not separators, not surrounding space.
    pub fn parse(text: &str) -> Result<DeviceId, DeviceIdError> {
        if text.len() != DIGITS {
            return Err(DeviceIdError);
        }
        let mut bytes = [0u8; BYTES];
        // The length is checked, so there are 16 pairs and no remainder.
        let (pairs, _) = text.as_bytes().as_chunks::<2>();
        for (byte, [high, low]) in bytes.iter_mut().zip(pairs.iter().copied()) {
            *byte = (hex_value(high)? << 4) | hex_value(low)?;
        }
        Ok(DeviceId(bytes))
    }
}

/// The value of one lowercase hexadecimal digit.
fn hex_value(digit: u8) -> Result<u8, DeviceIdError> {
    match digit {
        b'0'..=b'9' => Ok(digit - b'0'),
        b'a'..=b'f' => Ok(digit - b'a' + 10),
        _ => Err(DeviceIdError),
    }
}

/// The lowercase hexadecimal digit of a value below 16.
fn hex_digit(value: u8) -> char {
    char::from(match value {
        0..=9 => b'0' + value,
        _ => b'a' + (value - 10),
    })
}

impl fmt::Display for DeviceId {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        for byte in self.0 {
            f.write_char(hex_digit(byte >> 4))?;
            f.write_char(hex_digit(byte & 0x0f))?;
        }
        Ok(())
    }
}

impl fmt::Debug for DeviceId {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "DeviceId({self})")
    }
}

impl FromStr for DeviceId {
    type Err = DeviceIdError;

    fn from_str(text: &str) -> Result<DeviceId, DeviceIdError> {
        DeviceId::parse(text)
    }
}

/// The text is not a device id: it is not exactly 32 lowercase hexadecimal
/// digits.
///
/// It carries nothing, so that the crate needs no allocator; the caller has the
/// text and says it in its own message.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct DeviceIdError;

impl fmt::Display for DeviceIdError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.write_str("expected 32 lowercase hexadecimal digits")
    }
}

impl core::error::Error for DeviceIdError {}

/// The id as its text, for the formats that have one (JSON).
#[cfg(feature = "serde")]
impl serde::Serialize for DeviceId {
    fn serialize<S: serde::Serializer>(&self, serializer: S) -> Result<S::Ok, S::Error> {
        serializer.collect_str(self)
    }
}

#[cfg(feature = "serde")]
impl<'de> serde::Deserialize<'de> for DeviceId {
    fn deserialize<D: serde::Deserializer<'de>>(deserializer: D) -> Result<DeviceId, D::Error> {
        struct Text;

        impl serde::de::Visitor<'_> for Text {
            type Value = DeviceId;

            fn expecting(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
                f.write_str("a device id")
            }

            fn visit_str<E: serde::de::Error>(self, text: &str) -> Result<DeviceId, E> {
                DeviceId::parse(text)
                    .map_err(|err| E::custom(format_args!("'{text}' is not a device id: {err}")))
            }
        }

        deserializer.deserialize_str(Text)
    }
}

#[cfg(test)]
mod tests;
