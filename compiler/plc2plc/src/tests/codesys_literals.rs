//! The CODESYS literal forms in statement bodies and declarations: the
//! base-10 based integer, the typed boolean digits, and the one-bit `BIT`
//! type. See `specs/design/numeric-literals.md`.

use super::common::*;

/// The base and the typed digits are spellings of an ordinary integer or
/// boolean, so the rendering is the plain spelling of the same value and
/// re-parses to the same AST.
#[test]
fn write_to_string_when_based_literals_then_renders_the_plain_value() {
    let source = read_shared_resource("based_literals.st");
    let options = CompilerOptions {
        allow_bit_type: true,
        ..CompilerOptions::default()
    };
    let rendered = assert_round_trips(&source, &options);

    assert!(rendered.contains("x := 123"), "rendered:\n{rendered}");
    // A boolean literal renders as the typed `BOOL#` spelling in a statement
    // body, whatever prefix the source used.
    assert!(rendered.contains("b := BOOL#TRUE"), "rendered:\n{rendered}");
    assert!(
        rendered.contains("one := BOOL#TRUE"),
        "rendered:\n{rendered}"
    );
    // The BIT declaration keeps its type name; it is not normalized to BOOL.
    assert!(rendered.contains("one : BIT"), "rendered:\n{rendered}");
}
