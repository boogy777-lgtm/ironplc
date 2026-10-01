//! The `|` spelling of logical OR round-tripping.

use super::common::*;
use spec_test_macro::spec_test;

/// REQ-CS-plc2plc-001: A source using `|` renders back as the canonical
/// `OR` spelling and re-parses to the same AST.
#[spec_test(REQ_CS_plc2plc_001)]
fn write_to_string_when_pipe_or_then_round_trips() {
    let source = read_shared_resource("pipe_or.st");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    // The AST records the operator (logical OR), not its spelling, so the
    // re-parse alone cannot tell `|` from `OR`. The renderer writes the
    // canonical keyword spelling -- assert that on top, as the guide asks
    // for what AST equality cannot see.
    assert!(rendered.contains("OR"));
    assert!(!rendered.contains('|'));
}
