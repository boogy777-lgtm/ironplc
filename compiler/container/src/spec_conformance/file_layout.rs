//! Conformance tests for the file layout: section order, contiguity, absent sections, flag bits and the signature directory.

use std::io::Cursor;
use std::vec::Vec;

use spec_test_macro::spec_test;

use crate::builder::ContainerBuilder;
use crate::debug_section::FuncNameEntry;
use crate::header::{FileHeader, FLAG_HAS_DEBUG_SECTION, FLAG_HAS_TYPE_SECTION, HEADER_SIZE};
use crate::id_types::FunctionId;
use crate::opcode;
use crate::type_section::{FieldType, VarEntry};

// ---------------------------------------------------------------------------
// Container Format — File Layout (REQ-CF-container-010 through REQ-CF-container-016)
// ---------------------------------------------------------------------------

/// Serializes `container` and parses the header back out of the bytes, so
/// every assertion below is about what is on disk, not what the builder
/// held in memory.
fn write_and_read_header(container: &crate::Container) -> (Vec<u8>, FileHeader) {
    let mut buf = Vec::new();
    container.write_to(&mut buf).unwrap();
    let header = FileHeader::read_from(&mut Cursor::new(&buf[..HEADER_SIZE])).unwrap();
    (buf, header)
}

/// A container carrying every section, both optional ones included.
pub(super) fn full_container_bytes() -> (Vec<u8>, FileHeader) {
    let mut builder = ContainerBuilder::new();
    builder.add_array_descriptor(0, 4, 0);
    let container = builder
        .num_variables(1)
        .add_var_entry(VarEntry {
            var_type: FieldType::I32,
            flags: 0,
            extra: 0,
        })
        .add_i32_constant(1)
        .add_function(FunctionId::INIT, &[opcode::RET_VOID], 1, 1, 0)
        .add_func_name(FuncNameEntry {
            function_id: FunctionId::INIT,
            name: "MAIN".into(),
        })
        .build();
    write_and_read_header(&container)
}

/// A container carrying only the mandatory sections.
pub(super) fn minimal_container_bytes() -> (Vec<u8>, FileHeader) {
    let container = ContainerBuilder::new()
        .num_variables(1)
        .add_i32_constant(1)
        .add_function(FunctionId::INIT, &[opcode::RET_VOID], 1, 1, 0)
        .build();
    write_and_read_header(&container)
}

/// REQ-CF-container-010: Sections appear in the order header, task table,
/// type section, constant pool, code section, debug section. The signature
/// sections that precede the task table are planned and not yet emitted.
#[spec_test(REQ_CF_container_010)]
fn container_spec_req_cf_010_sections_appear_in_fixed_order() {
    let (_, h) = full_container_bytes();
    assert_eq!(h.task_section_offset, HEADER_SIZE as u32);
    assert!(h.task_section_offset < h.type_section_offset);
    assert!(h.type_section_offset < h.const_section_offset);
    assert!(h.const_section_offset < h.code_section_offset);
    assert!(h.code_section_offset < h.debug_section_offset);
}

/// REQ-CF-container-011: Every section starts where the previous present one
/// ends, with no padding; with no signature sections emitted the task table
/// therefore starts at 256.
#[spec_test(REQ_CF_container_011)]
fn container_spec_req_cf_011_sections_are_contiguous() {
    let (buf, h) = full_container_bytes();
    assert_eq!(h.task_section_offset, 256);
    assert_eq!(
        h.type_section_offset,
        h.task_section_offset + h.task_section_size
    );
    assert_eq!(
        h.const_section_offset,
        h.type_section_offset + h.type_section_size
    );
    assert_eq!(
        h.code_section_offset,
        h.const_section_offset + h.const_section_size
    );
    assert_eq!(
        h.debug_section_offset,
        h.code_section_offset + h.code_section_size
    );
    assert_eq!(
        buf.len() as u32,
        h.debug_section_offset + h.debug_section_size
    );

    // With the optional sections absent the constant pool follows the task
    // table directly and the file ends with the code section.
    let (buf, h) = minimal_container_bytes();
    assert_eq!(
        h.const_section_offset,
        h.task_section_offset + h.task_section_size
    );
    assert_eq!(
        buf.len() as u32,
        h.code_section_offset + h.code_section_size
    );
}

/// REQ-CF-container-012: An absent optional section has offset 0 and size 0.
#[spec_test(REQ_CF_container_012)]
fn container_spec_req_cf_012_absent_sections_have_zero_offset_and_size() {
    let (_, h) = minimal_container_bytes();
    assert_eq!(h.type_section_offset, 0);
    assert_eq!(h.type_section_size, 0);
    assert_eq!(h.debug_section_offset, 0);
    assert_eq!(h.debug_section_size, 0);
}

/// REQ-CF-container-013: Flag bit 1 (0x02) is set iff a debug section is present.
#[spec_test(REQ_CF_container_013)]
fn container_spec_req_cf_013_flag_bit1_iff_debug_section() {
    assert_eq!(FLAG_HAS_DEBUG_SECTION, 0x02);
    let (buf, h) = full_container_bytes();
    assert_ne!(h.flags & FLAG_HAS_DEBUG_SECTION, 0);
    assert_ne!(buf[7] & 0x02, 0);
    let (_, h) = minimal_container_bytes();
    assert_eq!(h.flags & FLAG_HAS_DEBUG_SECTION, 0);
}

/// REQ-CF-container-014: Flag bit 2 (0x04) is set iff a type section is present.
#[spec_test(REQ_CF_container_014)]
fn container_spec_req_cf_014_flag_bit2_iff_type_section() {
    assert_eq!(FLAG_HAS_TYPE_SECTION, 0x04);
    let (buf, h) = full_container_bytes();
    assert_ne!(h.flags & FLAG_HAS_TYPE_SECTION, 0);
    assert_ne!(buf[7] & 0x04, 0);
    let (_, h) = minimal_container_bytes();
    assert_eq!(h.flags & FLAG_HAS_TYPE_SECTION, 0);
}

/// REQ-CF-container-015: Bits 3–7 are written as zero and no bit marks a
/// signature section: a container with every section sets exactly the debug
/// and type bits.
#[spec_test(REQ_CF_container_015)]
fn container_spec_req_cf_015_no_other_flag_bits_are_written() {
    let (_, h) = full_container_bytes();
    assert_eq!(h.flags, FLAG_HAS_DEBUG_SECTION | FLAG_HAS_TYPE_SECTION);
    assert_eq!(h.flags & 0xF8, 0);
    let (_, h) = minimal_container_bytes();
    assert_eq!(h.flags, 0);
}

/// REQ-CF-container-016: Without signature sections all four signature
/// directory entries are zero.
#[spec_test(REQ_CF_container_016)]
fn container_spec_req_cf_016_signature_directory_entries_are_zero() {
    let (_, h) = full_container_bytes();
    assert_eq!(h.sig_section_offset, 0);
    assert_eq!(h.sig_section_size, 0);
    assert_eq!(h.debug_sig_offset, 0);
    assert_eq!(h.debug_sig_size, 0);
}
