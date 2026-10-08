//! Spec conformance tests for what the compiler writes into the container
//! header (`REQ-CF-codegen-*`).
//!
//! The container crate owns the rest of `bytecode-container-format.md`; the
//! requirements here are claims about compiler output rather than about the
//! format, so they are verified where the output is produced.
//!
//! See `specs/design/spec-conformance-testing.md` for the mechanism.

use std::io::Cursor;

use ironplc_container::{integrity, Container, FileHeader, HEADER_SIZE};
use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use spec_test_macro::spec_test;

/// Compiles `source` and returns the serialized bytes with the header parsed
/// back out of them, so every assertion is about what reaches the file.
fn compiled(source: &str) -> (Vec<u8>, FileHeader) {
    compiled_with_fb_uids(source, &[])
}

/// [`compiled`] with an engineering-side FB field UID table (ADR 0059)
/// supplied to codegen.
fn compiled_with_fb_uids(
    source: &str,
    fb_field_uids: &[(&str, &str, u64)],
) -> (Vec<u8>, FileHeader) {
    let options = CompilerOptions::default();
    let library = ironplc_parser::parse_program(source, &FileId::default(), &options).unwrap();
    let (analyzed, ctx) = ironplc_analyzer::stages::resolve_types(&[&library], &options).unwrap();
    let mut codegen_options = crate::CodegenOptions::from(&options);
    codegen_options.fb_field_uids = fb_field_uids
        .iter()
        .map(|(fb_type, field, uid)| (crate::FbFieldUidKey::new(fb_type, field), *uid))
        .collect();
    let container = crate::compile(&analyzed, &ctx, &codegen_options, &crate::EmptyLookup).unwrap();
    let mut buf = Vec::new();
    container.write_to(&mut buf).unwrap();
    let header = FileHeader::read_from(&mut Cursor::new(&buf[..HEADER_SIZE])).unwrap();
    (buf, header)
}

/// [`compiled_container`] with an engineering-side FB field UID table
/// (ADR 0059) supplied to codegen.
fn compiled_container_with_fb_uids(source: &str, fb_field_uids: &[(&str, &str, u64)]) -> Container {
    let (buf, _) = compiled_with_fb_uids(source, fb_field_uids);
    Container::read_from(&mut Cursor::new(&buf)).unwrap()
}

const ASSIGNMENT_PROGRAM: &str = "PROGRAM main
         VAR
             x : DINT;
         END_VAR
             x := 1;
         END_PROGRAM";

/// The bytes of one section, as the header's directory locates it.
fn section(buf: &[u8], offset: u32, size: u32) -> &[u8] {
    &buf[offset as usize..(offset + size) as usize]
}

/// REQ-CF-codegen-025: `layout_hash`, `content_hash` and `debug_hash` are
/// computed when the container is written; `content_hash` and `debug_hash`
/// reproduce from the written section bytes (the content hash covers the
/// masked header, task table, type, constant and code sections; see
/// [ADR-0007](../adrs/0007-dual-signature-integrity-model.md)). The reader
/// verifies the nonzero hashes at load time, so a successful round-trip is
/// itself proof the hashes match the serialized sections.
#[spec_test(REQ_CF_codegen_025)]
fn container_spec_req_cf_025_header_hashes_are_computed() {
    let (buf, header) = compiled(ASSIGNMENT_PROGRAM);

    assert_ne!(header.content_hash, integrity::NO_HASH);
    assert_ne!(header.layout_hash, [0u8; 32]);
    if header.debug_section_size > 0 {
        assert_ne!(header.debug_hash, integrity::NO_HASH);
    } else {
        assert_eq!(header.debug_hash, integrity::NO_HASH);
    }

    let expected_content = integrity::content_hash(&integrity::Content {
        header: buf[..HEADER_SIZE].try_into().unwrap(),
        task_table: section(&buf, header.task_section_offset, header.task_section_size),
        type_section: section(&buf, header.type_section_offset, header.type_section_size),
        const_section: section(&buf, header.const_section_offset, header.const_section_size),
        code_section: section(&buf, header.code_section_offset, header.code_section_size),
    });
    assert_eq!(header.content_hash, expected_content);

    let debug = section(&buf, header.debug_section_offset, header.debug_section_size);
    if header.debug_section_size > 0 {
        assert_eq!(header.debug_hash, integrity::debug_hash(debug));
    }

    let container = Container::read_from(&mut Cursor::new(&buf)).unwrap();
    assert_eq!(header.layout_hash, container.compute_layout_hash());
}

const FB_PROGRAM: &str = "FUNCTION_BLOCK Accumulator
  VAR_INPUT
    step : DINT;
  END_VAR
  VAR
    total : DINT;
  END_VAR
  total := total + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    acc : Accumulator;
  END_VAR
  acc(step := 1);
END_PROGRAM";

/// REQ-CF-codegen-026: the compiler records an FB field UID entry for each
/// user-defined FB field the engineering-side table names — (fb_type_id,
/// field ordinal) → uid, ascending — and no entry for fields it does not
/// name.
#[spec_test(REQ_CF_codegen_026)]
fn container_spec_req_cf_026_fb_field_uids_are_recorded() {
    let container = compiled_container_with_fb_uids(
        FB_PROGRAM,
        &[("Accumulator", "step", 101), ("Accumulator", "total", 102)],
    );
    let type_section = container.type_section.as_ref().unwrap();

    let user_fb = &type_section.user_fb_types[0];
    assert_eq!(
        type_section.fb_field_uids,
        vec![
            ironplc_container::FbFieldUidEntry {
                fb_type_id: user_fb.type_id,
                field_index: 0,
                uid: 101,
            },
            ironplc_container::FbFieldUidEntry {
                fb_type_id: user_fb.type_id,
                field_index: 1,
                uid: 102,
            },
        ]
    );

    // Fields the table does not name carry no entry.
    let unnamed = compiled_container_with_fb_uids(FB_PROGRAM, &[("Accumulator", "step", 101)]);
    assert_eq!(
        unnamed.type_section.as_ref().unwrap().fb_field_uids.len(),
        1
    );
}

/// REQ-CF-codegen-027: the compiler declares one persistent extent row per
/// program instance, taken at the point where it has assigned the last
/// persistent variable: the variable slots and data-region bytes reserved for
/// the globals and the program come inside it, and those reserved for
/// functions and function block bodies afterwards come outside it.
#[spec_test(REQ_CF_codegen_027)]
fn container_spec_req_cf_027_persistent_extent_ends_at_the_last_persistent_variable() {
    let container = compiled_container_with_fb_uids(FB_PROGRAM, &[]);

    let declared = &container.type_section.as_ref().unwrap().persistent_extents;

    // The program's `acc` is the only persistent variable; the function
    // block's two fields are working slots after it. The instance's two
    // 8-byte field slots are its persistent data.
    assert_eq!(declared.len(), 1);
    assert_eq!(
        declared[0].instance_id,
        ironplc_container::InstanceId::DEFAULT
    );
    assert_eq!((declared[0].var_start, declared[0].var_count), (0, 1));
    assert_eq!(container.header.num_variables, 3);
    assert_eq!((declared[0].data_start, declared[0].data_len), (0, 16));
}

/// REQ-CF-codegen-028: the compiler writes one FB type descriptor for every
/// user function block, in ascending type ID order, listing its fields in slot
/// order, and its field count equals the user FB descriptor's.
#[spec_test(REQ_CF_codegen_028)]
fn container_spec_req_cf_028_fb_type_descriptor_lists_the_fields_of_each_user_block() {
    let container = compiled_container_with_fb_uids(FB_PROGRAM, &[]);
    let type_section = container.type_section.as_ref().unwrap();

    assert_eq!(type_section.fb_types.len(), 1);
    assert_eq!(type_section.user_fb_types.len(), 1);
    let listed = &type_section.fb_types[0];
    assert_eq!(listed.type_id, type_section.user_fb_types[0].type_id);
    assert_eq!(
        listed.fields,
        vec![
            ironplc_container::FieldEntry {
                field_type: ironplc_container::FieldType::I32,
                field_extra: 0,
            },
            ironplc_container::FieldEntry {
                field_type: ironplc_container::FieldType::I32,
                field_extra: 0,
            },
        ]
    );
    assert_eq!(
        listed.fields.len(),
        usize::from(type_section.user_fb_types[0].num_fields)
    );
}
