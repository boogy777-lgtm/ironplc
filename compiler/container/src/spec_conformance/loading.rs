//! Conformance tests for the loading sequence: magic and version rejection and load-time verification.

use std::io::Cursor;
use std::vec::Vec;

use spec_test_macro::spec_test;

use crate::header::{FileHeader, FORMAT_VERSION, HEADER_SIZE, MAGIC};
use crate::ContainerError;

// ---------------------------------------------------------------------------
// Container Format — Loading Sequence (REQ-CF-container-026 through
// REQ-CF-container-027)
// ---------------------------------------------------------------------------

/// REQ-CF-container-026: A wrong magic is rejected with InvalidMagic.
#[spec_test(REQ_CF_container_026)]
fn container_spec_req_cf_026_wrong_magic_is_rejected() {
    let mut bytes = [0u8; HEADER_SIZE];
    bytes[0..4].copy_from_slice(&(MAGIC + 1).to_le_bytes());
    bytes[4..6].copy_from_slice(&FORMAT_VERSION.to_le_bytes());
    assert!(matches!(
        FileHeader::from_bytes(&bytes),
        Err(ContainerError::InvalidMagic)
    ));
}

/// REQ-CF-container-027: A format_version other than FORMAT_VERSION is
/// rejected with UnsupportedVersion.
#[spec_test(REQ_CF_container_027)]
fn container_spec_req_cf_027_unsupported_version_is_rejected() {
    let mut bytes = [0u8; HEADER_SIZE];
    bytes[0..4].copy_from_slice(&MAGIC.to_le_bytes());
    bytes[4..6].copy_from_slice(&(FORMAT_VERSION + 1).to_le_bytes());
    assert!(matches!(
        FileHeader::from_bytes(&bytes),
        Err(ContainerError::UnsupportedVersion)
    ));
}

// ---------------------------------------------------------------------------
// Container Format — Load-Time Verification (REQ-CF-container-029)
// ---------------------------------------------------------------------------

/// REQ-CF-container-029: The reader verifies a container at load time: a
/// nonzero `content_hash` must match the type, constant and code sections,
/// the type section's tables must be internally consistent, and a zero hash
/// (a legacy container) is accepted. A violated invariant is rejected with
/// `ContainerError::VerificationFailed`.
#[spec_test(REQ_CF_container_029)]
fn container_spec_req_cf_029_load_time_verification() {
    use crate::verify_load;

    // A consistent container round-trips and verifies.
    let consistent = crate::test_support::steel_thread_single_function_container();
    let mut buf = Vec::new();
    consistent.write_to(&mut buf).unwrap();
    let decoded = crate::Container::read_from(&mut Cursor::new(&buf)).unwrap();
    assert_eq!(verify_load(&decoded), Ok(()));

    // A corrupted code byte fails the content hash.
    let code_end = {
        let header = FileHeader::read_from(&mut Cursor::new(&buf[..HEADER_SIZE])).unwrap();
        (header.code_section_offset + header.code_section_size) as usize
    };
    buf[code_end - 1] = buf[code_end - 1].wrapping_add(1);
    assert!(matches!(
        crate::Container::read_from(&mut Cursor::new(&buf)),
        Err(ContainerError::ContentHashMismatch)
    ));

    // A zeroed content hash marks the bytes as legacy and is accepted.
    let mut header = FileHeader::read_from(&mut Cursor::new(&buf[..HEADER_SIZE])).unwrap();
    header.content_hash = [0u8; 32];
    let mut legacy = Vec::new();
    header.write_to(&mut legacy).unwrap();
    legacy.extend_from_slice(&buf[HEADER_SIZE..]);
    assert!(crate::Container::read_from(&mut Cursor::new(&legacy)).is_ok());
}
