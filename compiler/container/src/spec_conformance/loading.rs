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
/// rejected with UnsupportedVersion, which names the version found and the
/// version supported.
#[spec_test(REQ_CF_container_027)]
fn container_spec_req_cf_027_unsupported_version_is_rejected() {
    let mut bytes = [0u8; HEADER_SIZE];
    bytes[0..4].copy_from_slice(&MAGIC.to_le_bytes());
    bytes[4..6].copy_from_slice(&(FORMAT_VERSION + 1).to_le_bytes());
    assert!(matches!(
        FileHeader::from_bytes(&bytes),
        Err(ContainerError::UnsupportedVersion { found, supported })
        if found == FORMAT_VERSION + 1 && supported == FORMAT_VERSION
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

/// REQ-CF-container-043: The load check refuses a retained run that does not
/// lie inside the persistent extent in its space, and retained runs that are
/// not in ascending order of space and start or that overlap.
#[spec_test(REQ_CF_container_043)]
fn container_spec_req_cf_043_retained_runs_lie_inside_the_persistent_extent() {
    use crate::builder::ContainerBuilder;
    use crate::id_types::FunctionId;
    use crate::load_verify::LoadViolation;
    use crate::retained_extent::{RetainKind, RetainedRun};
    use crate::type_section::{FieldType, VarEntry};
    use crate::verify_load;

    let entry = || VarEntry {
        var_type: FieldType::I32,
        flags: 0,
        extra: 0,
    };
    let with_runs = |runs: &[RetainedRun]| {
        let mut container = ContainerBuilder::new()
            .num_variables(2)
            .data_region_bytes(32)
            .add_function(FunctionId::INIT, &[0x8C], 0, 2, 0)
            .add_var_entry(entry())
            .add_var_entry(entry())
            .add_retained_run(RetainedRun::slots(RetainKind::Retain, 0, 1))
            .build();
        // Keep the runs as given, the way a crafted file carries them.
        container.type_section.as_mut().unwrap().retained_runs = runs.to_vec();
        container
    };

    assert_eq!(
        verify_load(&with_runs(&[
            RetainedRun::slots(RetainKind::Retain, 0, 2),
            RetainedRun::bytes(RetainKind::Persistent, 8, 24),
        ])),
        Ok(())
    );
    assert!(matches!(
        verify_load(&with_runs(&[RetainedRun::slots(RetainKind::Retain, 1, 2)])),
        Err(LoadViolation::RetainedRunOutOfExtent { .. })
    ));
    assert!(matches!(
        verify_load(&with_runs(&[RetainedRun::bytes(
            RetainKind::Retain,
            24,
            16
        )])),
        Err(LoadViolation::RetainedRunOutOfExtent { .. })
    ));
    assert!(matches!(
        verify_load(&with_runs(&[
            RetainedRun::bytes(RetainKind::Retain, 0, 16),
            RetainedRun::bytes(RetainKind::Retain, 8, 8),
        ])),
        Err(LoadViolation::RetainedRunsOutOfOrder { .. })
    ));
}
