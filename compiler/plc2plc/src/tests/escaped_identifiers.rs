//! Escaped (backtick) identifier round-tripping.

use super::common::*;

#[test]
fn write_to_string_when_escaped_identifiers_then_round_trips_with_backticks() {
    let options = CompilerOptions {
        allow_escaped_identifiers: true,
        ..CompilerOptions::default()
    };
    let source = read_shared_resource("escaped_identifiers.st");
    let rendered = assert_round_trips(&source, &options);
    // The name keeps its backticks in the AST, so the rendering must carry
    // them: without them the re-parse would be a different (and failing)
    // program. The re-parse alone proves the rendering parses; this proves
    // the escape survived.
    assert!(
        rendered.contains("`counter value`"),
        "rendered:\n{rendered}"
    );
    assert!(rendered.contains("`CAFÉ`"), "rendered:\n{rendered}");
}
