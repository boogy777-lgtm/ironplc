//! Tests of the discovery wire format.
//!
//! The loops live in helpers that return what they found, so that each test is
//! a plain statement and an assertion.

extern crate std;

use std::string::String;
use std::vec::Vec;

use rstest::rstest;
use spec_test_macro::spec_test;

use crate::{
    decode_reply, encode_reply, is_request, request, DecodeError, EncodeError, Reply, MAGIC,
    MAX_REPLY_LEN, REQUEST_LEN, WIRE_VERSION,
};

const ID: [u8; 16] = [
    0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff,
];

/// Offsets in an encoded reply that are named by the specification.
const FLAGS_AT: usize = 6;
const RESERVED_AT: usize = 7;
const FIRST_STRING_AT: usize = 30;

fn typical() -> Reply<'static> {
    Reply {
        protocol: 1,
        session_port: 49152,
        device_id: ID,
        protected: false,
        name: "Press line 3",
        model: "IronPLC SoftPLC",
        runtime_version: "0.247.0",
    }
}

fn encode(reply: &Reply<'_>) -> Vec<u8> {
    let mut buf = std::vec![0u8; MAX_REPLY_LEN];
    let n = encode_reply(reply, &mut buf).unwrap();
    buf.truncate(n);
    buf
}

fn typical_bytes() -> Vec<u8> {
    encode(&typical())
}

/// The reply as the decoder returns it, encoded again with every ignored bit
/// of the input cleared.
fn canonical(bytes: &[u8]) -> Vec<u8> {
    let mut out = bytes.to_vec();
    out[FLAGS_AT] &= 1;
    out[RESERVED_AT] = 0;
    out
}

/// The length of the first strict prefix of `bytes` that decodes, if any.
fn first_prefix_that_decodes(bytes: &[u8]) -> Option<usize> {
    (0..bytes.len()).find(|&n| decode_reply(&bytes[..n]).is_ok())
}

/// The first position at which changing the byte to each of `values` leaves a
/// buffer that `is_request` still accepts.
fn first_mutation_accepted_as_request(values: &[u8]) -> Option<(usize, u8)> {
    let good = request();
    (0..REQUEST_LEN).find_map(|pos| {
        values.iter().find_map(|&v| {
            let mut bytes = good;
            bytes[pos] = v;
            (v != good[pos] && is_request(&bytes)).then_some((pos, v))
        })
    })
}

/// xorshift64: the same sequence on every run and every platform.
struct Rng(u64);

impl Rng {
    fn next_u64(&mut self) -> u64 {
        self.0 ^= self.0 << 13;
        self.0 ^= self.0 >> 7;
        self.0 ^= self.0 << 17;
        self.0
    }

    fn below(&mut self, n: usize) -> usize {
        (self.next_u64() % n as u64) as usize
    }

    fn bytes(&mut self, len: usize) -> Vec<u8> {
        (0..len).map(|_| self.next_u64() as u8).collect()
    }
}

/// Decodes `bytes` and, if it decodes, checks that it encodes again to the
/// canonical bytes. Returns the description of the first violation.
fn violation(bytes: &[u8]) -> Option<String> {
    let reply = decode_reply(bytes).ok()?;
    let mut buf = std::vec![0u8; MAX_REPLY_LEN];
    match encode_reply(&reply, &mut buf) {
        Ok(n) if buf[..n] == canonical(bytes)[..] => None,
        other => Some(std::format!(
            "{bytes:?} decoded to {reply:?}, encoded to {other:?}"
        )),
    }
}

/// Random byte strings of random lengths 0..1100, and mutated valid replies.
fn first_fuzz_violation(rounds: usize) -> Option<String> {
    let mut rng = Rng(0x9E37_79B9_7F4A_7C15);
    let valid = typical_bytes();
    (0..rounds).find_map(|_| {
        let len = rng.below(1100);
        let random = rng.bytes(len);
        let mut mutated = valid.clone();
        for _ in 0..=rng.below(4) {
            let at = rng.below(mutated.len());
            mutated[at] = rng.next_u64() as u8;
        }
        if rng.below(2) == 0 {
            let keep = rng.below(mutated.len() + 1);
            mutated.truncate(keep);
        } else {
            let extra_len = rng.below(8);
            mutated.extend(rng.bytes(extra_len));
        }
        violation(&random).or_else(|| violation(&mutated))
    })
}

// ---------------------------------------------------------------------------
// Header and request
// ---------------------------------------------------------------------------

#[spec_test(REQ_DW_discovery_001)]
fn magic_when_read_then_is_iplc_discovery_ascii() {
    assert_eq!(MAGIC, *b"IPLD");
    assert_eq!(&request()[..4], b"IPLD");
    assert_eq!(&typical_bytes()[..4], b"IPLD");
}

#[spec_test(REQ_DW_discovery_002)]
fn wire_version_when_encoding_then_is_one_at_byte_four() {
    assert_eq!(WIRE_VERSION, 1);
    assert_eq!(request()[4], 1);
    assert_eq!(typical_bytes()[4], 1);
}

#[spec_test(REQ_DW_discovery_003)]
fn kind_when_encoding_then_request_is_one_and_reply_is_two() {
    assert_eq!(request()[5], 1);
    assert_eq!(typical_bytes()[5], 2);
}

#[spec_test(REQ_DW_discovery_004)]
fn request_when_built_then_is_exactly_the_eight_header_bytes() {
    assert_eq!(request(), *b"IPLD\x01\x01\x00\x00");
    assert_eq!(REQUEST_LEN, 8);
}

#[spec_test(REQ_DW_discovery_005)]
fn is_request_when_exact_request_then_true() {
    assert!(is_request(&request()));
}

#[rstest]
#[case::empty(&[])]
#[case::one_byte(b"I")]
#[case::seven_bytes(b"IPLD\x01\x01\x00")]
#[case::nine_bytes(b"IPLD\x01\x01\x00\x00\x00")]
#[case::reply_header(b"IPLD\x01\x02\x00\x00")]
#[case::flags_in_reserved_bytes(b"IPLD\x01\x01\x01\x00")]
fn is_request_when_not_the_request_then_false(#[case] bytes: &[u8]) {
    assert!(!is_request(bytes));
}

#[test]
fn is_request_when_any_one_byte_changed_then_false() {
    let every_value: Vec<u8> = (0..=255).collect();
    assert_eq!(first_mutation_accepted_as_request(&every_value), None);
}

#[test]
fn is_request_when_a_valid_reply_then_false() {
    assert!(!is_request(&typical_bytes()));
}

#[test]
fn is_request_when_request_is_longer_or_repeated_then_false() {
    let mut twice = request().to_vec();
    twice.extend(request());
    assert!(!is_request(&twice));
}

// ---------------------------------------------------------------------------
// Reply: layout
// ---------------------------------------------------------------------------

#[spec_test(REQ_DW_discovery_010)]
fn encode_reply_when_protected_then_flags_bit_zero_is_set() {
    let protected = Reply {
        protected: true,
        ..typical()
    };
    assert_eq!(encode(&protected)[FLAGS_AT], 0b0000_0001);
    assert_eq!(typical_bytes()[FLAGS_AT], 0);
}

#[spec_test(REQ_DW_discovery_011)]
fn decode_reply_when_reserved_byte_is_set_then_it_is_ignored() {
    let mut bytes = typical_bytes();
    assert_eq!(bytes[RESERVED_AT], 0);
    bytes[RESERVED_AT] = 0xAB;
    assert_eq!(decode_reply(&bytes), Ok(typical()));
}

#[spec_test(REQ_DW_discovery_012)]
fn encode_reply_when_protocol_set_then_it_is_little_endian_at_offset_eight() {
    let reply = Reply {
        protocol: 0x0102_0304,
        ..typical()
    };
    assert_eq!(&encode(&reply)[8..12], &[0x04, 0x03, 0x02, 0x01]);
}

#[spec_test(REQ_DW_discovery_013)]
fn encode_reply_when_port_set_then_it_is_little_endian_at_offset_twelve() {
    let reply = Reply {
        session_port: 0xC000,
        ..typical()
    };
    assert_eq!(&encode(&reply)[12..14], &[0x00, 0xC0]);
}

#[spec_test(REQ_DW_discovery_014)]
fn encode_reply_when_device_id_set_then_its_sixteen_bytes_follow_the_port() {
    assert_eq!(&typical_bytes()[14..30], &ID);
}

#[spec_test(REQ_DW_discovery_015)]
fn encode_reply_when_strings_set_then_each_has_a_u16_length_in_order() {
    let bytes = encode(&Reply {
        name: "ab",
        model: "c",
        runtime_version: "def",
        ..typical()
    });
    assert_eq!(
        &bytes[FIRST_STRING_AT..],
        &[2, 0, b'a', b'b', 1, 0, b'c', 3, 0, b'd', b'e', b'f']
    );
}

#[spec_test(REQ_DW_discovery_016)]
fn decode_reply_when_unknown_flag_bits_are_set_then_they_are_ignored() {
    let mut bytes = typical_bytes();
    bytes[FLAGS_AT] = 0b1111_1110;
    assert_eq!(decode_reply(&bytes), Ok(typical()));
    bytes[FLAGS_AT] = 0b1111_1111;
    assert_eq!(
        decode_reply(&bytes),
        Ok(Reply {
            protected: true,
            ..typical()
        })
    );
}

#[test]
fn encode_reply_when_not_protected_then_unknown_flag_bits_are_zero() {
    let protected = Reply {
        protected: true,
        ..typical()
    };
    assert_eq!(encode(&protected)[FLAGS_AT] & !1, 0);
}

// ---------------------------------------------------------------------------
// Reply: round trip
// ---------------------------------------------------------------------------

#[spec_test(REQ_DW_discovery_041)]
fn decode_reply_when_typical_reply_encoded_then_round_trips() {
    let bytes = typical_bytes();
    assert_eq!(decode_reply(&bytes), Ok(typical()));
}

#[test]
fn decode_reply_when_runtime_version_of_protected_device_then_it_is_carried() {
    let protected = Reply {
        protected: true,
        ..typical()
    };
    let bytes = encode(&protected);
    assert_eq!(decode_reply(&bytes).unwrap().runtime_version, "0.247.0");
}

#[rstest]
#[case::protected(Reply { protected: true, ..typical() })]
#[case::empty_strings(Reply { name: "", model: "", runtime_version: "", ..typical() })]
#[case::max_port(Reply { session_port: u16::MAX, ..typical() })]
#[case::zero_port(Reply { session_port: 0, ..typical() })]
#[case::max_protocol(Reply { protocol: u32::MAX, ..typical() })]
#[case::zero_id(Reply { device_id: [0; 16], ..typical() })]
#[case::max_id(Reply { device_id: [0xFF; 16], ..typical() })]
#[case::multibyte_name(Reply { name: "Цех №3 — линия", model: "ПЛК", ..typical() })]
fn decode_reply_when_edge_values_encoded_then_round_trip(#[case] reply: Reply<'static>) {
    let bytes = encode(&reply);
    assert_eq!(decode_reply(&bytes), Ok(reply));
}

#[test]
fn decode_reply_when_name_is_sixty_four_multibyte_characters_then_round_trips() {
    // 64 characters of 4 UTF-8 bytes each: 256 bytes, the longest a name may be.
    let name: String = "\u{1F600}".repeat(64);
    assert_eq!(name.len(), 256);
    let reply = Reply {
        name: &name,
        ..typical()
    };
    let bytes = encode(&reply);
    assert_eq!(decode_reply(&bytes), Ok(reply));
}

#[test]
fn encode_reply_when_buffer_is_larger_then_returns_the_encoded_length() {
    let mut buf = [0xEEu8; 2000];
    let n = encode_reply(&typical(), &mut buf).unwrap();
    assert_eq!(n, typical_bytes().len());
    assert_eq!(buf[n], 0xEE);
}

// ---------------------------------------------------------------------------
// Reply: size
// ---------------------------------------------------------------------------

#[spec_test(REQ_DW_discovery_020)]
fn encode_reply_when_exactly_max_length_then_succeeds() {
    let fixed = typical_bytes().len() - typical().name.len();
    let name: String = "n".repeat(MAX_REPLY_LEN - fixed);
    let reply = Reply {
        name: &name,
        ..typical()
    };
    let bytes = encode(&reply);
    assert_eq!(bytes.len(), MAX_REPLY_LEN);
    assert_eq!(decode_reply(&bytes), Ok(reply));
}

#[spec_test(REQ_DW_discovery_021)]
fn encode_reply_when_one_byte_over_max_then_too_long_and_nothing_written() {
    let fixed = typical_bytes().len() - typical().name.len();
    let name: String = "n".repeat(MAX_REPLY_LEN - fixed + 1);
    let reply = Reply {
        name: &name,
        ..typical()
    };
    let mut buf = std::vec![0u8; 4096];
    assert_eq!(
        encode_reply(&reply, &mut buf),
        Err(EncodeError::TooLong {
            len: MAX_REPLY_LEN + 1
        })
    );
    assert!(buf.iter().all(|&b| b == 0));
}

#[test]
fn encode_reply_when_strings_together_are_too_long_then_too_long() {
    let long: String = "x".repeat(400);
    let reply = Reply {
        name: &long,
        model: &long,
        runtime_version: &long,
        ..typical()
    };
    let mut buf = std::vec![0u8; 4096];
    assert!(matches!(
        encode_reply(&reply, &mut buf),
        Err(EncodeError::TooLong { .. })
    ));
}

#[test]
fn encode_reply_when_a_string_exceeds_u16_then_too_long() {
    let huge: String = "x".repeat(70_000);
    let reply = Reply {
        name: &huge,
        ..typical()
    };
    let mut buf = std::vec![0u8; 16];
    assert!(matches!(
        encode_reply(&reply, &mut buf),
        Err(EncodeError::TooLong { len: 70_058 })
    ));
}

#[spec_test(REQ_DW_discovery_022)]
fn encode_reply_when_buffer_too_small_then_error_with_needed_length_and_no_write() {
    let needed = typical_bytes().len();
    let mut buf = std::vec![0u8; needed - 1];
    assert_eq!(
        encode_reply(&typical(), &mut buf),
        Err(EncodeError::BufferTooSmall { needed })
    );
    assert!(buf.iter().all(|&b| b == 0));
}

#[test]
fn encode_reply_when_buffer_is_empty_then_buffer_too_small() {
    assert!(matches!(
        encode_reply(&typical(), &mut []),
        Err(EncodeError::BufferTooSmall { .. })
    ));
}

#[test]
fn encode_reply_when_buffer_is_exactly_the_length_then_succeeds() {
    let needed = typical_bytes().len();
    let mut buf = std::vec![0u8; needed];
    assert_eq!(encode_reply(&typical(), &mut buf), Ok(needed));
}

#[spec_test(REQ_DW_discovery_023)]
fn decode_reply_when_input_is_longer_than_max_then_too_long() {
    // A valid reply padded past the limit: refused for its length alone.
    let mut bytes = typical_bytes();
    bytes.resize(MAX_REPLY_LEN + 1, 0);
    assert_eq!(
        decode_reply(&bytes),
        Err(DecodeError::TooLong {
            len: MAX_REPLY_LEN + 1
        })
    );
}

#[test]
fn decode_reply_when_input_is_longer_than_max_and_not_iplc_then_too_long_first() {
    let bytes = std::vec![0u8; MAX_REPLY_LEN + 1];
    assert!(matches!(
        decode_reply(&bytes),
        Err(DecodeError::TooLong { .. })
    ));
}

// ---------------------------------------------------------------------------
// Decoding is strict
// ---------------------------------------------------------------------------

#[spec_test(REQ_DW_discovery_030)]
fn decode_reply_when_magic_is_wrong_then_bad_magic() {
    let mut bytes = typical_bytes();
    bytes[3] = b'X';
    assert_eq!(decode_reply(&bytes), Err(DecodeError::BadMagic));
}

#[rstest]
#[case::empty(&[], DecodeError::Truncated)]
#[case::right_prefix_only(b"IPL", DecodeError::Truncated)]
#[case::wrong_first_byte(b"X", DecodeError::BadMagic)]
#[case::wrong_short(b"IPX", DecodeError::BadMagic)]
#[case::request_of_another_protocol(b"GET / HTTP/1.1\r\n", DecodeError::BadMagic)]
fn decode_reply_when_short_input_then_magic_is_checked_on_what_is_there(
    #[case] bytes: &[u8],
    #[case] expected: DecodeError,
) {
    assert_eq!(decode_reply(bytes), Err(expected));
}

#[spec_test(REQ_DW_discovery_031)]
fn decode_reply_when_wire_version_is_not_one_then_unsupported_version() {
    let mut bytes = typical_bytes();
    bytes[4] = 2;
    assert_eq!(
        decode_reply(&bytes),
        Err(DecodeError::UnsupportedVersion(2))
    );
    bytes[4] = 0;
    assert_eq!(
        decode_reply(&bytes),
        Err(DecodeError::UnsupportedVersion(0))
    );
}

#[test]
fn decode_reply_when_newer_version_has_a_different_body_then_version_is_reported() {
    // Only the header is read before the version is judged.
    let mut bytes = b"IPLD\x02\x02".to_vec();
    bytes.extend_from_slice(&[0xFF; 40]);
    assert_eq!(
        decode_reply(&bytes),
        Err(DecodeError::UnsupportedVersion(2))
    );
}

#[spec_test(REQ_DW_discovery_032)]
fn decode_reply_when_kind_is_not_reply_then_unexpected_kind() {
    assert_eq!(
        decode_reply(&request()),
        Err(DecodeError::UnexpectedKind(1))
    );
    let mut bytes = typical_bytes();
    bytes[5] = 9;
    assert_eq!(decode_reply(&bytes), Err(DecodeError::UnexpectedKind(9)));
}

#[spec_test(REQ_DW_discovery_033)]
fn decode_reply_when_cut_inside_the_fixed_part_then_truncated() {
    let bytes = typical_bytes();
    assert_eq!(decode_reply(&bytes[..7]), Err(DecodeError::Truncated));
    assert_eq!(decode_reply(&bytes[..29]), Err(DecodeError::Truncated));
    // Inside the length prefix of the first string.
    assert_eq!(
        decode_reply(&bytes[..FIRST_STRING_AT + 1]),
        Err(DecodeError::Truncated)
    );
}

#[spec_test(REQ_DW_discovery_034)]
fn decode_reply_when_string_length_runs_past_the_end_then_string_past_end() {
    let mut bytes = typical_bytes();
    bytes[FIRST_STRING_AT] = 0xFF;
    bytes[FIRST_STRING_AT + 1] = 0xFF;
    assert_eq!(decode_reply(&bytes), Err(DecodeError::StringPastEnd));
    let cut = bytes.len() - 1;
    assert_eq!(
        decode_reply(&typical_bytes()[..cut]),
        Err(DecodeError::StringPastEnd)
    );
}

#[spec_test(REQ_DW_discovery_035)]
#[rstest]
#[case::name(0)]
#[case::model(1)]
#[case::runtime_version(2)]
fn decode_reply_when_a_string_is_not_utf8_then_invalid_utf8(#[case] which: usize) {
    let mut strings = ["ab", "cd", "ef"];
    strings[which] = "xy";
    let reply = Reply {
        name: strings[0],
        model: strings[1],
        runtime_version: strings[2],
        ..typical()
    };
    let mut bytes = encode(&reply);
    // Strings are two bytes with a two-byte length each.
    let at = FIRST_STRING_AT + which * 4 + 2;
    bytes[at] = 0xFF;
    bytes[at + 1] = 0xC0;
    assert_eq!(decode_reply(&bytes), Err(DecodeError::InvalidUtf8));
}

#[test]
fn decode_reply_when_a_string_ends_inside_a_multibyte_character_then_invalid_utf8() {
    let reply = Reply {
        name: "\u{1F600}",
        ..typical()
    };
    let mut bytes = encode(&reply);
    // Shorten the declared length of the name by one byte, and drop that byte.
    bytes[FIRST_STRING_AT] = 3;
    bytes.remove(FIRST_STRING_AT + 2 + 3);
    assert_eq!(decode_reply(&bytes), Err(DecodeError::InvalidUtf8));
}

#[spec_test(REQ_DW_discovery_036)]
fn decode_reply_when_a_byte_follows_the_last_string_then_trailing_bytes() {
    let mut bytes = typical_bytes();
    bytes.push(0);
    assert_eq!(decode_reply(&bytes), Err(DecodeError::TrailingBytes));
}

#[spec_test(REQ_DW_discovery_037)]
fn decode_reply_when_any_strict_prefix_of_a_valid_reply_then_error() {
    assert_eq!(first_prefix_that_decodes(&typical_bytes()), None);
}

#[test]
fn decode_reply_when_any_strict_prefix_of_edge_replies_then_error() {
    let longest_name: String = "\u{1F600}".repeat(64);
    let edges = [
        Reply {
            name: "",
            model: "",
            runtime_version: "",
            ..typical()
        },
        Reply {
            name: &longest_name,
            protected: true,
            ..typical()
        },
    ];
    let found = edges
        .iter()
        .find_map(|reply| first_prefix_that_decodes(&encode(reply)));
    assert_eq!(found, None);
}

// ---------------------------------------------------------------------------
// Fuzz
// ---------------------------------------------------------------------------

#[spec_test(REQ_DW_discovery_040)]
fn decode_reply_when_random_and_mutated_input_then_never_panics_and_successes_re_encode() {
    assert_eq!(first_fuzz_violation(4000), None);
}

#[test]
fn decode_reply_when_only_the_ignored_bits_differ_then_both_re_encode_to_the_same_bytes() {
    let mut bytes = typical_bytes();
    bytes[FLAGS_AT] = 0b1010_1010;
    bytes[RESERVED_AT] = 0x55;
    assert_eq!(violation(&bytes), None);
    assert_eq!(decode_reply(&bytes).map(|r| r.protected), Ok(false));
}

#[test]
fn display_when_each_error_then_is_a_sentence_of_its_own() {
    let decode = [
        DecodeError::TooLong { len: 2000 },
        DecodeError::BadMagic,
        DecodeError::UnsupportedVersion(7),
        DecodeError::UnexpectedKind(1),
        DecodeError::Truncated,
        DecodeError::StringPastEnd,
        DecodeError::InvalidUtf8,
        DecodeError::TrailingBytes,
    ];
    let mut shown: Vec<String> = decode.iter().map(|e| std::format!("{e}")).collect();
    shown.push(std::format!("{}", EncodeError::TooLong { len: 2000 }));
    shown.push(std::format!(
        "{}",
        EncodeError::BufferTooSmall { needed: 40 }
    ));
    let before = shown.len();
    shown.sort();
    shown.dedup();
    assert_eq!(shown.len(), before);
    assert!(shown.iter().all(|s| !s.is_empty()));
}

#[test]
fn all_spec_requirements_have_tests() {
    assert!(
        crate::spec_requirements::UNTESTED.is_empty(),
        "Requirements in spec with no conformance test: {:?}",
        crate::spec_requirements::UNTESTED
    );
}
