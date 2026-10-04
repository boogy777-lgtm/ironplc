//! A negated output assignment (`NOT out => target`) in a function block call.

use super::common::*;

#[test]
fn write_to_string_when_output_negated_then_round_trips_and_keeps_not() {
    let source = "
FUNCTION_BLOCK FLAG
VAR_OUTPUT
    q : BOOL;
END_VAR
END_FUNCTION_BLOCK

PROGRAM main
VAR
    fb : FLAG;
    a : BOOL;
    b : BOOL;
END_VAR
fb(q => a, NOT q => b);
END_PROGRAM
";
    let rendered = assert_round_trips(source, &CompilerOptions::default());
    assert!(
        rendered.contains("NOT q => b"),
        "the NOT of the output is rendered: {rendered}"
    );
    assert!(
        rendered.contains("q => a"),
        "the plain output stays plain: {rendered}"
    );
}
