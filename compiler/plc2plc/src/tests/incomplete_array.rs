//! The incomplete array type `ARRAY[*] OF T` round-tripping.

use super::common::*;
use spec_test_macro::spec_test;

/// REQ-CS-plc2plc-002: An `ARRAY[*]` declaration renders with the star and
/// re-parses to the same AST.
#[spec_test(REQ_CS_plc2plc_002)]
fn write_to_string_when_incomplete_array_then_round_trips_with_star() {
    let options = CompilerOptions {
        allow_incomplete_array: true,
        ..CompilerOptions::default()
    };
    let source = read_shared_resource("incomplete_array.st");
    let rendered = assert_round_trips(&source, &options);
    // The AST records the bounds as incomplete, not as an empty range list,
    // so the star must survive the rendering; a `<no ranges>` rendering would
    // re-parse to the same AST but spell the source wrong.
    assert!(rendered.contains("ARRAY [* ]"), "rendered:\n{rendered}");
}
