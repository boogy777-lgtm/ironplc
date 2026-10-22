//! Tests of the device id.

extern crate std;

use std::string::{String, ToString};

use rstest::rstest;
use spec_test_macro::spec_test;

use crate::{DeviceId, DeviceIdError};

const TEXT: &str = "00112233445566778899aabbccddeeff";

const BYTES: [u8; 16] = [
    0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff,
];

#[spec_test(REQ_DW_discovery_050)]
#[test]
fn parse_when_32_lowercase_hex_digits_then_the_id_with_those_bytes() {
    let id = DeviceId::parse(TEXT).unwrap();

    assert_eq!(id.as_bytes(), &BYTES);
    assert_eq!(id, DeviceId::from_bytes(BYTES));
}

#[spec_test(REQ_DW_discovery_050)]
#[test]
fn parse_when_digits_are_counted_then_the_bytes_one_to_sixteen() {
    let id = DeviceId::parse("0102030405060708090a0b0c0d0e0f10").unwrap();

    let expected: [u8; 16] = core::array::from_fn(|i| u8::try_from(i + 1).unwrap());
    assert_eq!(id.as_bytes(), &expected);
}

#[spec_test(REQ_DW_discovery_050)]
#[rstest]
#[case::empty("")]
#[case::short("0123456789abcdef")]
#[case::one_short("0123456789abcdef0123456789abcde")]
#[case::long("0123456789abcdef0123456789abcdef0")]
#[case::uppercase("0123456789ABCDEF0123456789abcdef")]
#[case::all_uppercase("0123456789ABCDEF0123456789ABCDEF")]
#[case::not_hex("0123456789abcdef0123456789abcdeg")]
#[case::percent("0123456789abcdef0123456789abcd%f")]
#[case::dashes("01234567-89ab-cdef-0123-456789abcdef")]
#[case::leading_space(" 0123456789abcdef0123456789abcde")]
#[case::trailing_newline("0123456789abcdef0123456789abcde\n")]
#[case::hex_prefix("0x23456789abcdef0123456789abcdef")]
#[case::signed("+0112233445566778899aabbccddeeff")]
// Multibyte text of exactly 32 bytes: the length alone must not make it an id.
#[case::multibyte_utf8_in_32_bytes("0123456789abcdef0123456789abcd\u{e9}")]
#[case::multibyte_utf8_fills_the_length("\u{e9}23456789abcdef0123456789abcdef")]
#[case::four_byte_character_fills_the_length("0123456789abcdef0123456789ab\u{1f600}")]
fn parse_when_not_32_lowercase_hex_digits_then_error(#[case] text: &str) {
    assert_eq!(DeviceId::parse(text), Err(DeviceIdError));
}

#[spec_test(REQ_DW_discovery_051)]
#[test]
fn display_when_id_then_32_lowercase_digits() {
    let id = DeviceId::from_bytes(BYTES);

    assert_eq!(id.to_string(), TEXT);
}

#[test]
fn display_when_leading_zero_bytes_then_digits_are_not_dropped() {
    let mut bytes = [0u8; 16];
    bytes[1] = 0xab;
    bytes[15] = 0x0f;

    let expected = std::format!("00ab{}0f", "00".repeat(13));
    assert_eq!(DeviceId::from_bytes(bytes).to_string(), expected);
}

#[spec_test(REQ_DW_discovery_051)]
#[rstest]
#[case::zero([0u8; 16])]
#[case::max([0xffu8; 16])]
#[case::counted(BYTES)]
fn display_when_parsed_again_then_the_same_id(#[case] bytes: [u8; 16]) {
    let id = DeviceId::from_bytes(bytes);

    let text: String = id.to_string();

    assert_eq!(text.len(), 32);
    assert_eq!(DeviceId::parse(&text), Ok(id));
}

#[test]
fn from_str_when_valid_then_same_as_parse() {
    assert_eq!(TEXT.parse::<DeviceId>(), DeviceId::parse(TEXT));
    assert_eq!("nope".parse::<DeviceId>(), Err(DeviceIdError));
}

#[test]
fn ord_when_ids_then_the_order_of_their_text() {
    let low = DeviceId::parse("00000000000000000000000000000009").unwrap();
    let high = DeviceId::parse("0000000000000000000000000000000a").unwrap();
    let highest = DeviceId::parse("f0000000000000000000000000000000").unwrap();

    assert!(low < high);
    assert!(high < highest);
    assert!(low.to_string() < high.to_string());
    assert!(high.to_string() < highest.to_string());
}

#[test]
fn debug_when_id_then_names_the_type_and_the_digits() {
    let id = DeviceId::from_bytes(BYTES);

    assert_eq!(std::format!("{id:?}"), std::format!("DeviceId({TEXT})"));
}

#[test]
fn error_display_when_formatted_then_says_what_is_expected() {
    assert_eq!(
        DeviceIdError.to_string(),
        "expected 32 lowercase hexadecimal digits"
    );
}

#[test]
fn serialize_when_id_then_json_string_of_its_digits() {
    let id = DeviceId::from_bytes(BYTES);

    assert_eq!(
        serde_json::to_string(&id).unwrap(),
        std::format!("\"{TEXT}\"")
    );
}

#[test]
fn deserialize_when_json_string_of_digits_then_the_id() {
    let id: DeviceId = serde_json::from_str(&std::format!("\"{TEXT}\"")).unwrap();

    assert_eq!(id, DeviceId::from_bytes(BYTES));
}

#[rstest]
#[case::uppercase("\"00112233445566778899AABBCCDDEEFF\"")]
#[case::short("\"0011\"")]
#[case::number("17")]
fn deserialize_when_not_a_device_id_then_error(#[case] json: &str) {
    assert!(serde_json::from_str::<DeviceId>(json).is_err());
}

#[test]
fn deserialize_when_not_a_device_id_then_the_error_names_the_text() {
    let err = serde_json::from_str::<DeviceId>("\"AAAA\"").unwrap_err();

    assert!(
        err.to_string().contains("'AAAA' is not a device id"),
        "{err}"
    );
}

#[test]
fn serde_when_map_keyed_by_id_then_the_keys_are_the_digits() {
    let mut map = std::collections::BTreeMap::new();
    map.insert(DeviceId::from_bytes(BYTES), 1u8);

    let json = serde_json::to_string(&map).unwrap();
    let back: std::collections::BTreeMap<DeviceId, u8> = serde_json::from_str(&json).unwrap();

    assert_eq!(json, std::format!("{{\"{TEXT}\":1}}"));
    assert_eq!(back, map);
}
