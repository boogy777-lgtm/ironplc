//! End-to-end tests for edge inputs (`x : BOOL R_EDGE` / `F_EDGE`): the body
//! reads the input as TRUE only in the scan in which the actual input changed
//! in the edge's direction. Each instance keeps its own previous value.

use ironplc_parser::options::CompilerOptions;
use rstest::rstest;

use crate::common::{drive_fb, try_parse_and_compile, FbStep, FbStep::*};

const RISING_BLOCK: &str = "
FUNCTION_BLOCK Pulse
  VAR_INPUT x : BOOL R_EDGE; END_VAR
  VAR_OUTPUT q : BOOL; END_VAR
  q := x;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    p : Pulse;
    v : BOOL;
    out : BOOL;
  END_VAR
  p(x := v, q => out);
END_PROGRAM
";

// vars: 0 = p, 1 = v, 2 = out.
#[rstest]
// The input is FALSE throughout: no edge.
#[case::held_low(&[Run(0), Expect(2, 0), Run(1), Expect(2, 0)])]
// FALSE, TRUE, TRUE, FALSE, TRUE: the block reads TRUE only on the two rises.
#[case::pulses_on_each_rise(&[
    Write(1, 0), Run(0), Expect(2, 0),
    Write(1, 1), Run(1), Expect(2, 1),
    Write(1, 1), Run(2), Expect(2, 0),
    Write(1, 0), Run(3), Expect(2, 0),
    Write(1, 1), Run(4), Expect(2, 1),
])]
// An input that is TRUE in the first scan is a rise from the initial FALSE.
#[case::high_at_first_scan(&[Write(1, 1), Run(0), Expect(2, 1), Run(1), Expect(2, 0)])]
fn end_to_end_edge_input_when_r_edge_then_true_only_on_rising_scan(#[case] steps: &[FbStep]) {
    drive_fb(RISING_BLOCK, &CompilerOptions::default(), steps);
}

#[rstest]
// FALSE, TRUE, TRUE, FALSE, TRUE, FALSE: TRUE only on the two falls.
#[case::pulses_on_each_fall(&[
    Write(1, 0), Run(0), Expect(2, 0),
    Write(1, 1), Run(1), Expect(2, 0),
    Write(1, 1), Run(2), Expect(2, 0),
    Write(1, 0), Run(3), Expect(2, 1),
    Write(1, 1), Run(4), Expect(2, 0),
    Write(1, 0), Run(5), Expect(2, 1),
    Run(6), Expect(2, 0),
])]
fn end_to_end_edge_input_when_f_edge_then_true_only_on_falling_scan(#[case] steps: &[FbStep]) {
    drive_fb(
        &RISING_BLOCK.replace("R_EDGE", "F_EDGE"),
        &CompilerOptions::default(),
        steps,
    );
}

// vars: 0 = a, 1 = b, 2 = va, 3 = vb, 4 = outa, 5 = outb.
const TWO_INSTANCES: &str = "
FUNCTION_BLOCK Pulse
  VAR_INPUT x : BOOL R_EDGE; END_VAR
  VAR_OUTPUT q : BOOL; END_VAR
  q := x;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    a : Pulse;
    b : Pulse;
    va : BOOL;
    vb : BOOL;
    outa : BOOL;
    outb : BOOL;
  END_VAR
  a(x := va, q => outa);
  b(x := vb, q => outb);
END_PROGRAM
";

#[test]
fn end_to_end_edge_input_when_two_instances_then_each_keeps_its_own_previous_value() {
    drive_fb(
        TWO_INSTANCES,
        &CompilerOptions::default(),
        &[
            // Only `a` rises.
            Write(2, 1),
            Run(0),
            Expect(4, 1),
            Expect(5, 0),
            // `b` rises while `a` stays high: only `b` pulses.
            Write(3, 1),
            Run(1),
            Expect(4, 0),
            Expect(5, 1),
            Run(2),
            Expect(4, 0),
            Expect(5, 0),
        ],
    );
}

// vars: 0 = p, 1 = connected, 2 = v, 3 = out.
const SOMETIMES_UNCONNECTED: &str = "
FUNCTION_BLOCK Pulse
  VAR_INPUT x : BOOL R_EDGE; END_VAR
  VAR_OUTPUT q : BOOL; END_VAR
  q := x;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    p : Pulse;
    connected : BOOL;
    v : BOOL;
    out : BOOL;
  END_VAR
  IF connected THEN
    p(x := v, q => out);
  ELSE
    p(q => out);
  END_IF;
END_PROGRAM
";

#[test]
fn end_to_end_edge_input_when_call_leaves_input_unassigned_then_no_edge_and_no_stale_pulse() {
    drive_fb(
        SOMETIMES_UNCONNECTED,
        &CompilerOptions::default(),
        &[
            Write(1, 1),
            Write(2, 1),
            Run(0),
            Expect(3, 1),
            // The input keeps its value when a call leaves it out: no new rise.
            Write(1, 0),
            Run(1),
            Expect(3, 0),
            Run(2),
            Expect(3, 0),
        ],
    );
}

// vars: 0 = x, 1 = q, then the program's hidden state.
const PROGRAM_INPUT: &str = "
PROGRAM main
  VAR_INPUT x : BOOL R_EDGE; END_VAR
  VAR q : BOOL; END_VAR
  q := x;
END_PROGRAM
";

#[rstest]
#[case::r_edge("R_EDGE", &[Run(0), Expect(1, 0), Write(0, 1), Run(1), Expect(1, 1), Run(2), Expect(1, 0)])]
#[case::f_edge("F_EDGE", &[Write(0, 1), Run(0), Expect(1, 0), Write(0, 0), Run(1), Expect(1, 1), Run(2), Expect(1, 0)])]
fn end_to_end_edge_input_when_program_input_then_edge_of_the_value_written(
    #[case] edge: &str,
    #[case] steps: &[FbStep],
) {
    drive_fb(
        &PROGRAM_INPUT.replace("R_EDGE", edge),
        &CompilerOptions::default(),
        steps,
    );
}

#[test]
fn compile_when_edge_input_on_function_then_not_supported() {
    let result = try_parse_and_compile(
        "
FUNCTION f : BOOL
  VAR_INPUT x : BOOL R_EDGE; END_VAR
  f := x;
END_FUNCTION

PROGRAM main
  VAR r : BOOL; END_VAR
  r := f(x := TRUE);
END_PROGRAM
",
        &CompilerOptions::default(),
    );
    assert!(result.is_err());
}
