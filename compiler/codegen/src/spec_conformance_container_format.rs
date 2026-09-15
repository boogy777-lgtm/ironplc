//! Spec conformance tests for what the compiler writes into the container
//! header (`REQ-CF-codegen-*`).
//!
//! The container crate owns the rest of `bytecode-container-format.md`; the
//! requirements here are claims about compiler output rather than about the
//! format, so they are verified where the output is produced.
//!
//! See `specs/design/spec-conformance-testing.md` for the mechanism.

use std::io::Cursor;

use ironplc_container::Container;
use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use spec_test_macro::spec_test;

/// Compiles `source` and parses the container back out of the serialized
/// bytes, so the assertions are about what reaches the file.
fn compiled_container(source: &str) -> Container {
    compiled_container_with_fb_uids(source, &[])
}

/// [`compiled_container`] with an engineering-side FB field UID table
/// (ADR 0059) supplied to codegen.
fn compiled_container_with_fb_uids(source: &str, fb_field_uids: &[(&str, &str, u64)]) -> Container {
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
    Container::read_from(&mut Cursor::new(&buf)).unwrap()
}

/// REQ-CF-codegen-025: `layout_hash`, `content_hash` and `debug_hash` are
/// computed when the container is written; the reader verifies the nonzero
/// hashes at load time, so a successful round-trip is itself proof the
/// hashes match the serialized sections.
#[spec_test(REQ_CF_codegen_025)]
fn container_spec_req_cf_025_header_hashes_are_computed() {
    let container = compiled_container(
        "PROGRAM main
         VAR
             x : DINT;
         END_VAR
             x := 1;
         END_PROGRAM",
    );
    assert_ne!(container.header.content_hash, [0u8; 32]);
    assert_ne!(container.header.layout_hash, [0u8; 32]);
    if container.header.debug_section_size > 0 {
        assert_ne!(container.header.debug_hash, [0u8; 32]);
    } else {
        assert_eq!(container.header.debug_hash, [0u8; 32]);
    }
    assert_eq!(
        container.header.layout_hash,
        container.compute_layout_hash()
    );
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
