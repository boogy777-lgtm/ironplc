//! The CODESYS `PARAMS(n) OF T` parameter-list type round-tripping.

use super::common::*;
use spec_test_macro::spec_test;

/// REQ-CS-plc2plc-003: A PARAMS declaration renders with the
/// `PARAMS(n) OF T` spelling and re-parses to the same AST.
#[spec_test(REQ_CS_plc2plc_003)]
fn write_to_string_when_params_of_then_round_trips_with_params_spelling() {
    let options = CompilerOptions {
        allow_params_of: true,
        ..CompilerOptions::default()
    };
    let source = read_shared_resource("params_of.st");
    let rendered = assert_round_trips(&source, &options);
    // The AST keeps the spelling; the array the list lowers to is an
    // analyzer decision. A rendering as `ARRAY[0..2] OF INT` would re-parse
    // to a different AST, so the round trip proves the spelling survives --
    // these assertions pin what the round trip cannot show on its own.
    assert!(
        rendered.contains("PARAMS ( 3 ) OF INT"),
        "rendered:\n{rendered}"
    );
    assert!(
        rendered.contains("PARAMS ( 2 ) OF REAL"),
        "rendered:\n{rendered}"
    );
}
