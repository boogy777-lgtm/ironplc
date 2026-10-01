//! ST-visible CODESYS special operators round-tripping.

use super::common::*;
use spec_test_macro::spec_test;

/// REQ-CS-plc2plc-004: The special operators render in the spelling they
/// were written with and re-parse to the same AST.
#[spec_test(REQ_CS_plc2plc_004)]
fn write_to_string_when_special_operators_then_round_trips_in_the_source_spelling() {
    let source = read_shared_resource("special_operators.st");
    let rendered = assert_round_trips(&source, &CompilerOptions::default());
    // `__NEW(T)`/`__TYPEOF(T)` take a type where an ordinary call takes an
    // expression; that the rendering re-parses to the same AST is what
    // proves the type argument was written back as a name. The assertions
    // only pin the spelling, which the AST does not record.
    for spelling in ["__NEW", "__XADD", "__DELETE", "__TYPEOF", "__ISVALIDREF"] {
        assert!(
            rendered.contains(spelling),
            "missing {spelling}:\n{rendered}"
        );
    }
}
