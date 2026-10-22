//! The id of a device: 32 lowercase hexadecimal digits that the device
//! generates once and keeps (ADR 0084).
//!
//! [`DeviceId`] is the one definition of a well-formed id on the engineering
//! side: the only way to make one is [`DeviceId::parse`], so a value of the type
//! is an id that a controller can report. The records that name or hold a
//! device ([`crate::path_record`], [`crate::download_record`]) take the type
//! and not a text.
//!
//! why: the id is the identity of a controller, and a mixed-case or short text
//! would name a controller that no controller reports. The check of the device
//! itself lives in the `ironplcvm` binary crate, which this crate cannot use;
//! the format is the one that crate and the discovery protocol write. The id is
//! also a safe part of a file name (digits and `a-f` only), which is why a
//! record can be named after it without encoding.

use std::fmt;
use std::str::FromStr;

/// The number of hexadecimal digits of a device id (128 bits).
const DEVICE_ID_DIGITS: usize = 32;

/// A device id: exactly 32 lowercase hexadecimal digits.
#[derive(Clone, Debug, PartialEq, Eq, Hash)]
pub struct DeviceId(String);

impl DeviceId {
    /// The id `text`, or why it is not one.
    pub fn parse(text: &str) -> Result<DeviceId, String> {
        let well_formed = text.len() == DEVICE_ID_DIGITS
            && text
                .bytes()
                .all(|byte| byte.is_ascii_digit() || (b'a'..=b'f').contains(&byte));
        if well_formed {
            Ok(DeviceId(text.to_owned()))
        } else {
            Err(format!(
                "'{text}' is not a device id: expected {DEVICE_ID_DIGITS} lowercase hexadecimal digits"
            ))
        }
    }

    /// The 32 lowercase hexadecimal digits.
    pub fn as_str(&self) -> &str {
        &self.0
    }
}

impl FromStr for DeviceId {
    type Err = String;

    fn from_str(text: &str) -> Result<DeviceId, String> {
        DeviceId::parse(text)
    }
}

impl fmt::Display for DeviceId {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.write_str(&self.0)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use rstest::rstest;

    #[test]
    fn parse_when_32_lowercase_hex_digits_then_the_id() {
        let id = DeviceId::parse("0123456789abcdef0123456789abcdef").unwrap();

        assert_eq!(id.as_str(), "0123456789abcdef0123456789abcdef");
        assert_eq!(id.to_string(), id.as_str());
    }

    #[rstest]
    #[case::empty("")]
    #[case::short("0123456789abcdef")]
    #[case::long("0123456789abcdef0123456789abcdef0")]
    #[case::uppercase("0123456789ABCDEF0123456789abcdef")]
    #[case::not_hex("0123456789abcdef0123456789abcdeg")]
    #[case::percent("0123456789abcdef0123456789abcd%f")]
    #[case::non_ascii("0123456789abcdef0123456789abcdeü")]
    fn parse_when_not_32_lowercase_hex_digits_then_error_naming_the_text(#[case] text: &str) {
        let err = DeviceId::parse(text).unwrap_err();

        assert!(err.contains("is not a device id"), "{err}");
        assert!(err.contains(&format!("'{text}'")), "{err}");
    }
}
