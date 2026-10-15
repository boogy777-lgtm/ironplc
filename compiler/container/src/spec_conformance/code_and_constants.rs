//! Conformance tests for the code section directory and bodies and the constant pool entry encoding.

use std::vec;
use std::vec::Vec;

use spec_test_macro::spec_test;

use crate::code_section::{CodeSection, FuncEntry};
use crate::constant_pool::{ConstEntry, ConstantPool};
use crate::id_types::FunctionId;
use crate::{opcode, ConstType};

// ---------------------------------------------------------------------------
// Container Format — Constant Pool and Code Section (REQ-CF-container-022
// through REQ-CF-container-024)
// ---------------------------------------------------------------------------

fn write_code_section(section: &CodeSection) -> Vec<u8> {
    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();
    buf
}

/// REQ-CF-container-022: A FuncEntry is 16 bytes with num_params at offset 14.
#[spec_test(REQ_CF_container_022)]
fn container_spec_req_cf_022_func_entry_is_16_bytes() {
    let section = CodeSection {
        functions: vec![FuncEntry {
            function_id: FunctionId::new(0x0102),
            code_offset: 0x0304_0506,
            code_length: 1,
            max_stack_depth: 0x0708,
            num_locals: 0x090A,
            num_params: 0x0B0C,
        }],
        bytecode: vec![opcode::RET_VOID],
    };
    assert_eq!(section.section_size(), 17);
    let buf = write_code_section(&section);
    assert_eq!(buf.len(), 17);
    assert_eq!(&buf[0..2], &[0x02, 0x01]);
    assert_eq!(&buf[2..6], &[0x06, 0x05, 0x04, 0x03]);
    assert_eq!(&buf[6..10], &[1, 0, 0, 0]);
    assert_eq!(&buf[10..12], &[0x08, 0x07]);
    assert_eq!(&buf[12..14], &[0x0A, 0x09]);
    assert_eq!(&buf[14..16], &[0x0C, 0x0B]);
}

/// REQ-CF-container-023: Bodies follow the directory immediately; a body is
/// at 16 × num_functions + code_offset within the section.
#[spec_test(REQ_CF_container_023)]
fn container_spec_req_cf_023_bodies_follow_directory() {
    let section = CodeSection {
        functions: vec![
            FuncEntry {
                function_id: FunctionId::new(0),
                code_offset: 0,
                code_length: 1,
                max_stack_depth: 0,
                num_locals: 0,
                num_params: 0,
            },
            FuncEntry {
                function_id: FunctionId::new(1),
                code_offset: 1,
                code_length: 2,
                max_stack_depth: 0,
                num_locals: 0,
                num_params: 0,
            },
        ],
        bytecode: vec![0xAA, 0xBB, 0xCC],
    };
    let buf = write_code_section(&section);
    assert_eq!(buf.len(), 2 * 16 + 3);
    assert_eq!(&buf[32..35], &[0xAA, 0xBB, 0xCC]);
    assert_eq!(&buf[32 + 1..32 + 3], &[0xBB, 0xCC]);
}

/// REQ-CF-container-024: A ConstEntry is const_type u8, char_width u8,
/// size u16, then exactly `size` value bytes; strings carry no length prefix.
#[spec_test(REQ_CF_container_024)]
fn container_spec_req_cf_024_const_entry_header_and_value() {
    let mut pool = ConstantPool::default();
    pool.push(ConstEntry::primitive_le(
        ConstType::I32,
        &0x0102_0304i32.to_le_bytes(),
    ));
    pool.push(ConstEntry::string(b"hi".to_vec()));
    let mut buf = Vec::new();
    pool.write_to(&mut buf).unwrap();
    assert_eq!(
        buf,
        vec![
            2,
            0, // count
            ConstType::I32 as u8,
            0,
            4,
            0,
            0x04,
            0x03,
            0x02,
            0x01, // i32
            ConstType::Str as u8,
            1,
            2,
            0,
            b'h',
            b'i', // STRING "hi"
        ]
    );
}
