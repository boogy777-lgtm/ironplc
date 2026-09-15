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
    let options = CompilerOptions::default();
    let library = ironplc_parser::parse_program(source, &FileId::default(), &options).unwrap();
    let (analyzed, ctx) = ironplc_analyzer::stages::resolve_types(&[&library], &options).unwrap();
    let container = crate::compile(
        &analyzed,
        &ctx,
        &crate::CodegenOptions::from(&options),
        &crate::EmptyLookup,
    )
    .unwrap();
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
