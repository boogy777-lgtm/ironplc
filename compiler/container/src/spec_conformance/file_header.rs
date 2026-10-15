//! Conformance tests for the file header: size, magic, version, byte order, field offsets and reserved bytes.

use std::vec::Vec;

use spec_test_macro::spec_test;

use crate::header::{FileHeader, FLAG_HAS_SYSTEM_UPTIME, FORMAT_VERSION, HEADER_SIZE, MAGIC};

// ---------------------------------------------------------------------------
// Container Format — File Header (REQ-CF-container-001 through REQ-CF-container-007)
// ---------------------------------------------------------------------------

/// REQ-CF-container-001: The file header is exactly 256 bytes.
#[spec_test(REQ_CF_container_001)]
fn container_spec_req_cf_001_header_size_is_256_bytes() {
    assert_eq!(core::mem::size_of::<FileHeader>(), HEADER_SIZE);
    assert_eq!(HEADER_SIZE, 256);
}

/// REQ-CF-container-002: Magic number is 0x49504C43 ("IPLC" in ASCII).
#[spec_test(REQ_CF_container_002)]
fn container_spec_req_cf_002_magic_is_iplc() {
    assert_eq!(MAGIC, 0x49504C43);
    // On disk (little-endian): bytes are [0x43, 0x4C, 0x50, 0x49].
    // The u32 value 0x49504C43 encodes "IPLC" MSB-first.
    let bytes = MAGIC.to_le_bytes();
    assert_eq!(bytes, [0x43, 0x4C, 0x50, 0x49]);
}

/// REQ-CF-container-003: Format version is 8.
#[spec_test(REQ_CF_container_003)]
fn container_spec_req_cf_003_format_version_is_8() {
    assert_eq!(FORMAT_VERSION, 8);
}

/// REQ-CF-container-004: All multi-byte values in the header are little-endian.
#[spec_test(REQ_CF_container_004)]
fn container_spec_req_cf_004_header_uses_little_endian() {
    let header = FileHeader::default();
    let mut buf = Vec::new();
    header.write_to(&mut buf).unwrap();

    // Magic at offset 0: 0x49504C43 in LE is [0x43, 0x4C, 0x50, 0x49]
    assert_eq!(&buf[0..4], &0x49504C43u32.to_le_bytes());

    // Format version at offset 4 (u16 LE)
    assert_eq!(&buf[4..6], &FORMAT_VERSION.to_le_bytes());
}

/// REQ-CF-container-005: Header field offsets match the spec table layout, totaling
/// 256 bytes with reserved at 218-255.
#[spec_test(REQ_CF_container_005)]
fn container_spec_req_cf_005_header_field_offsets() {
    // Write a header with distinctive values and verify byte offsets
    let header = FileHeader {
        num_variables: 0x1234,
        code_section_offset: 0xAABBCCDD,
        ..Default::default()
    };

    let mut buf = Vec::new();
    header.write_to(&mut buf).unwrap();
    assert_eq!(buf.len(), 256);

    // num_variables at offset 196 (u16 LE)
    assert_eq!(u16::from_le_bytes([buf[196], buf[197]]), 0x1234);

    // code_section_offset at offset 176 (u32 LE)
    assert_eq!(
        u32::from_le_bytes([buf[176], buf[177], buf[178], buf[179]]),
        0xAABBCCDD
    );

    // Bytes 40..72 are the reserved hash slot (formerly source_hash);
    // a default header must zero them.
    assert_eq!(&buf[40..72], &[0u8; 32]);
}

/// REQ-CF-container-006: Reserved bytes are 38 bytes at offsets 218-255.
#[spec_test(REQ_CF_container_006)]
fn container_spec_req_cf_006_reserved_is_38_bytes_at_offset_218() {
    let header = FileHeader::default();
    assert_eq!(header.reserved.len(), 38);

    let mut buf = Vec::new();
    header.write_to(&mut buf).unwrap();

    // Bytes 218..256 should all be zero (reserved)
    assert_eq!(&buf[218..256], &[0u8; 38]);
    // And that's exactly the end of the header
    assert_eq!(buf.len(), 256);
}

/// REQ-CF-container-007: Flags bit 0 is FLAG_HAS_SYSTEM_UPTIME (0x01).
#[spec_test(REQ_CF_container_007)]
fn container_spec_req_cf_007_flags_bit0_is_system_uptime() {
    assert_eq!(FLAG_HAS_SYSTEM_UPTIME, 0x01);

    // Verify the flag is at byte offset 7
    let header = FileHeader {
        flags: FLAG_HAS_SYSTEM_UPTIME,
        ..Default::default()
    };
    let mut buf = Vec::new();
    header.write_to(&mut buf).unwrap();
    assert_eq!(buf[7], 0x01);
}
