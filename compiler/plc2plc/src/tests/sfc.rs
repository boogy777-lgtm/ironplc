//! SFC networks: transitions and action associations.

use super::common::*;

const NETWORK: &str = "
FUNCTION_BLOCK Sequence
VAR_INPUT go : BOOL; END_VAR
INITIAL_STEP Idle:
    Light(N);
END_STEP
STEP Running:
    Light(N, lit);
END_STEP
TRANSITION FROM Idle TO Running := go; END_TRANSITION
TRANSITION FROM Running TO Idle := NOT go; END_TRANSITION
ACTION Light:
    lit := TRUE;
END_ACTION
END_FUNCTION_BLOCK
";

fn with_transition_header(header: &str) -> String {
    NETWORK.replacen(
        "TRANSITION FROM Idle",
        &format!("TRANSITION {header} FROM Idle"),
        1,
    )
}

#[test]
fn write_to_string_when_transition_has_name_then_keeps_it() {
    let rendered = assert_round_trips(&with_transition_header("Start"), &edition3());

    assert!(
        rendered.contains("TRANSITION Start FROM Idle"),
        "rendered:\n{rendered}"
    );
}

#[test]
fn write_to_string_when_transition_has_priority_then_keeps_it() {
    let rendered = assert_round_trips(&with_transition_header("(PRIORITY := 3)"), &edition3());

    assert!(
        rendered.contains("TRANSITION ( PRIORITY := 3 ) FROM Idle"),
        "rendered:\n{rendered}"
    );
}

#[test]
fn write_to_string_when_transition_has_name_and_priority_then_keeps_both() {
    let rendered = assert_round_trips(
        &with_transition_header("Start (PRIORITY := 1)"),
        &edition3(),
    );

    assert!(
        rendered.contains("TRANSITION Start ( PRIORITY := 1 ) FROM Idle"),
        "rendered:\n{rendered}"
    );
}

#[test]
fn write_to_string_when_qualifier_is_timed_then_keeps_the_time() {
    let source = "
FUNCTION_BLOCK Sequence
VAR timeout : TIME; END_VAR
INITIAL_STEP Idle:
    Light(N);
    Pulse(SD, T#2s);
    Hold(SL, timeout);
    Flash(P1, T#1s, lit);
    Edge(P0, T#1s);
END_STEP
ACTION Light: lit := TRUE; END_ACTION
END_FUNCTION_BLOCK
";
    let rendered = assert_round_trips(source, &edition3());

    assert!(
        rendered.contains("Pulse ( SD , TIME#2000ms );"),
        "rendered:\n{rendered}"
    );
}
