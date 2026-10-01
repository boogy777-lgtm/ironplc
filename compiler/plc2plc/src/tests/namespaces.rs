//! `NAMESPACE ... END_NAMESPACE` and `__BEGIN_IMPLEMENTATION`
//! round-tripping. See specs/design/st-declaration-extensions.md.

use super::common::*;
use spec_test_macro::spec_test;

fn namespace_options() -> CompilerOptions {
    CompilerOptions {
        allow_namespace: true,
        allow_begin_implementation: true,
        ..CompilerOptions::default()
    }
}

/// REQ-STX-plc2plc-021: a namespace renders back with its nested
/// declarations in place, and the `__BEGIN_IMPLEMENTATION` marker keeps its
/// position in the statement list; the rendering re-parses to the same AST.
#[spec_test(REQ_STX_plc2plc_021)]
fn plc2plc_spec_req_stx_021_namespace_and_marker_round_trip() {
    let source = read_shared_resource("namespace.st");
    let rendered = assert_round_trips(&source, &namespace_options());

    // The nesting is what the AST records and the re-parse cannot see in the
    // rendering alone (indentation is not part of the AST): the namespace
    // keywords must appear, in order, around the nested program.
    let outer = rendered.find("NAMESPACE Motor").expect("outer namespace");
    let inner = rendered.find("NAMESPACE Control").expect("inner namespace");
    let marker = rendered
        .find("__BEGIN_IMPLEMENTATION")
        .expect("implementation marker");
    let inner_end = rendered.find("END_NAMESPACE").expect("inner end");
    assert!(outer < inner && inner < marker && marker < inner_end);
}
