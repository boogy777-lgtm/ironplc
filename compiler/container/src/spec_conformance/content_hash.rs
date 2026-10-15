//! Conformance tests for the content hash and debug hash: what they cover, what they mask and how readers react to a mismatch.

use std::io::Cursor;
use std::vec;
use std::vec::Vec;

use spec_test_macro::spec_test;

use super::file_layout::{full_container_bytes, minimal_container_bytes};
use crate::header::{FileHeader, FLAG_HAS_DEBUG_SECTION, HEADER_SIZE};
use crate::test_support::with_tampered_header;
use crate::{integrity, Container, ContainerError, ContainerRef};

// ---------------------------------------------------------------------------
// Container Format — Content and Debug Hashes (REQ-CF-container-028 through
// REQ-CF-container-034)
// ---------------------------------------------------------------------------

/// The bytes of one section, as the header's directory locates it.
fn section(buf: &[u8], offset: u32, size: u32) -> &[u8] {
    &buf[offset as usize..(offset + size) as usize]
}

/// The file index of the last byte of a section.
fn last_byte_of(offset: u32, size: u32) -> usize {
    (offset + size - 1) as usize
}

/// Asserts that both readers reject `buf` with `ContentHashMismatch` once
/// the byte at `index` is flipped.
fn assert_flipped_byte_is_rejected(buf: &[u8], index: usize) {
    let mut tampered = buf.to_vec();
    tampered[index] ^= 0xFF;
    assert!(matches!(
        Container::read_from(&mut Cursor::new(&tampered)),
        Err(ContainerError::ContentHashMismatch)
    ));
    let mut offsets = vec![0u32; 4];
    assert!(matches!(
        ContainerRef::from_slice(&tampered, &mut offsets),
        Err(ContainerError::ContentHashMismatch)
    ));
}

/// The header image the content hash covers, recomputed from the spec's
/// description rather than from `integrity::masked_header`.
fn spec_masked_header(buf: &[u8]) -> Vec<u8> {
    let mut image = buf[..HEADER_SIZE].to_vec();
    image[7] &= !FLAG_HAS_DEBUG_SECTION;
    image[8..40].fill(0);
    image[72..104].fill(0);
    image[136..192].fill(0);
    image
}

/// BLAKE3 over the masked header followed by each section in file order,
/// each located by the header's directory.
fn spec_content_hash(buf: &[u8], h: &FileHeader) -> [u8; 32] {
    let mut hasher = blake3::Hasher::new();
    hasher.update(&spec_masked_header(buf));
    hasher.update(section(buf, h.task_section_offset, h.task_section_size));
    hasher.update(section(buf, h.type_section_offset, h.type_section_size));
    hasher.update(section(buf, h.const_section_offset, h.const_section_size));
    hasher.update(section(buf, h.code_section_offset, h.code_section_size));
    *hasher.finalize().as_bytes()
}

// REQ-CF-container-035 through -037 carry upstream's hash requirements under
// numbers upstream added as 028-030: the fork had already taken 028 (stable
// var layout), 029 (load-time verification) and 030 (FB field UID layout),
// so the merge renumbered the newer side. Keep the numbers in sync with
// `specs/design/bytecode-container-format.md`.

/// REQ-CF-container-035: content_hash is BLAKE3 over the masked header, task
/// table, type section, constant pool and code section in file order; an
/// absent type section contributes nothing.
#[spec_test(REQ_CF_container_035)]
fn container_spec_req_cf_035_content_hash_covers_header_task_type_const_and_code() {
    let (buf, h) = full_container_bytes();
    assert_eq!(h.content_hash, spec_content_hash(&buf, &h));
    assert_ne!(h.content_hash, integrity::NO_HASH);

    let (buf, h) = minimal_container_bytes();
    assert_eq!(h.type_section_size, 0);
    assert_eq!(h.content_hash, spec_content_hash(&buf, &h));
}

/// REQ-CF-container-034: the hashed header image zeroes exactly
/// content_hash, debug_hash and the section directory, and clears the debug
/// flag bit; every other header byte is covered.
#[spec_test(REQ_CF_container_034)]
fn container_spec_req_cf_034_header_image_masks_hashes_directory_and_debug_flag() {
    let (buf, _) = full_container_bytes();
    let header: [u8; HEADER_SIZE] = buf[..HEADER_SIZE].try_into().unwrap();
    let image = integrity::masked_header(&header);
    assert_eq!(image.to_vec(), spec_masked_header(&buf));

    // The masked bytes carry nothing; the rest carries the header verbatim.
    assert_eq!(image[7] & FLAG_HAS_DEBUG_SECTION, 0);
    assert_eq!(&image[8..40], &[0u8; 32]);
    assert_eq!(&image[72..104], &[0u8; 32]);
    assert_eq!(&image[136..192], &[0u8; 56]);
    assert_eq!(&image[0..7], &header[0..7]);
    assert_eq!(&image[40..72], &header[40..72]);
    assert_eq!(&image[104..136], &header[104..136]);
    assert_eq!(&image[192..256], &header[192..256]);
}

/// REQ-CF-container-036: debug_hash is BLAKE3 over the debug section bytes,
/// and all zeros when there is no debug section.
#[spec_test(REQ_CF_container_036)]
fn container_spec_req_cf_036_debug_hash_covers_debug_section() {
    let (buf, h) = full_container_bytes();
    let debug = section(&buf, h.debug_section_offset, h.debug_section_size);
    assert_eq!(h.debug_hash, *blake3::hash(debug).as_bytes());

    let (_, h) = minimal_container_bytes();
    assert_eq!(h.debug_hash, integrity::NO_HASH);
}

/// REQ-CF-container-037: a byte changed in the covered header bytes or in
/// any hashed section is rejected with ContentHashMismatch by both readers.
#[spec_test(REQ_CF_container_037)]
fn container_spec_req_cf_037_modified_hashed_content_is_rejected() {
    let (buf, h) = full_container_bytes();
    // Header: `profile` (byte 6) and `data_region_bytes` (bytes 198–201)
    // are covered and not otherwise validated by the reader.
    assert_flipped_byte_is_rejected(&buf, 6);
    assert_flipped_byte_is_rejected(&buf, 198);
    assert_flipped_byte_is_rejected(
        &buf,
        last_byte_of(h.task_section_offset, h.task_section_size),
    );
    assert_flipped_byte_is_rejected(
        &buf,
        last_byte_of(h.type_section_offset, h.type_section_size),
    );
    assert_flipped_byte_is_rejected(
        &buf,
        last_byte_of(h.const_section_offset, h.const_section_size),
    );
    assert_flipped_byte_is_rejected(
        &buf,
        last_byte_of(h.code_section_offset, h.code_section_size),
    );
}

/// REQ-CF-container-031: an all-zero content_hash is not checked, so a
/// container that would otherwise be rejected loads.
#[spec_test(REQ_CF_container_031)]
fn container_spec_req_cf_031_zero_content_hash_is_not_checked() {
    let (buf, h) = full_container_bytes();
    let mut unhashed = with_tampered_header(&buf, |h| h.content_hash = integrity::NO_HASH);
    unhashed[last_byte_of(h.code_section_offset, h.code_section_size)] ^= 0xFF;

    assert!(Container::read_from(&mut Cursor::new(&unhashed)).is_ok());
    let mut offsets = vec![0u32; 4];
    assert!(ContainerRef::from_slice(&unhashed, &mut offsets).is_ok());
}

/// REQ-CF-container-032: a debug section that does not reproduce a nonzero
/// debug_hash is discarded; the container still loads.
#[spec_test(REQ_CF_container_032)]
fn container_spec_req_cf_032_debug_hash_mismatch_discards_debug_section() {
    let (mut buf, h) = full_container_bytes();
    // Change a byte the parser accepts either way: the function name.
    let debug = section(&buf, h.debug_section_offset, h.debug_section_size);
    let name_at = debug.windows(4).position(|w| w == b"MAIN").unwrap();
    buf[h.debug_section_offset as usize + name_at] = b'X';

    let container = Container::read_from(&mut Cursor::new(&buf)).unwrap();
    assert!(container.debug_section.is_none());
    assert_eq!(container.code.functions.len(), 1);

    // The same bytes parse once the hash says nothing about them, so it was
    // the hash check that discarded the section, not the parser.
    let unhashed = with_tampered_header(&buf, |h| h.debug_hash = integrity::NO_HASH);
    let container = Container::read_from(&mut Cursor::new(&unhashed)).unwrap();
    assert!(container.debug_section.is_some());
}

/// REQ-CF-container-033: stripping the debug section leaves content_hash
/// valid, so the stripped container loads unchanged.
#[spec_test(REQ_CF_container_033)]
fn container_spec_req_cf_033_stripped_debug_section_keeps_content_hash_valid() {
    let (buf, h) = full_container_bytes();
    let stripped = with_tampered_header(&buf[..h.debug_section_offset as usize], |h| {
        h.debug_section_offset = 0;
        h.debug_section_size = 0;
        h.debug_hash = integrity::NO_HASH;
        h.flags &= !FLAG_HAS_DEBUG_SECTION;
    });

    let container = Container::read_from(&mut Cursor::new(&stripped)).unwrap();
    assert_eq!(container.header.content_hash, h.content_hash);
    assert!(container.debug_section.is_none());
    assert_eq!(container.code.functions.len(), 1);
}
