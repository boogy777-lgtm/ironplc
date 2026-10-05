//! A function block call whose callee is an array element (`fbs[i](...)`).

use super::common::*;

#[test]
fn write_to_string_when_array_element_called_then_round_trips() {
    let source = "
FUNCTION_BLOCK Acc
VAR_INPUT
    x : DINT;
END_VAR
VAR_OUTPUT
    total : DINT;
END_VAR
END_FUNCTION_BLOCK

PROGRAM main
VAR
    accs : ARRAY[0..2] OF Acc;
    grid : ARRAY[1..2, 0..1] OF Acc;
    i : DINT;
    a : DINT;
END_VAR
accs[i + 1](x := 1, total => a);
grid[2, 1](x := 2);
CALC(TRUE, accs[0](x := 3));
END_PROGRAM
";
    let options = CompilerOptions::from_dialect(Dialect::Codesys);

    // The re-parse proves the callee keeps its subscripts: a rendering that
    // dropped them would not parse back to the same calls.
    assert_round_trips(source, &options);
}
