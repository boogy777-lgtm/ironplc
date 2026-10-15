//! Tests for the TwinCAT XML parser.
//!
//! Split out of `twincat_parser.rs` to keep that module within the project's
//! 1000-line limit.

mod cdata_offsets;
mod closing_keyword;
mod element_kinds;
mod error_positions;
mod leading_comment;
mod malformed_documents;
mod methods;
mod unsupported_bodies;

use super::*;
use ironplc_dsl::common::{FunctionBlockDeclaration, LibraryElementKind, TypeName};
use ironplc_dsl::core::{FileId, Id, Located};
use ironplc_dsl::member_qualifier::AccessSpecifier;
use ironplc_parser::options::Dialect;
use ironplc_test::cast;

pub(super) fn test_file_id() -> FileId {
    FileId::from_string("test.TcPOU")
}

pub(super) fn opts_with_fb_inheritance() -> CompilerOptions {
    CompilerOptions {
        allow_fb_inheritance: true,
        ..CompilerOptions::default()
    }
}

/// Extract the single function block from a library, or panic describing
/// what was found instead.
pub(super) fn only_function_block(library: Library) -> FunctionBlockDeclaration {
    assert_eq!(library.elements.len(), 1);
    match library.elements.into_iter().next() {
        Some(LibraryElementKind::FunctionBlockDeclaration(decl)) => decl,
        other => panic!("expected FunctionBlockDeclaration, got {other:?}"),
    }
}
