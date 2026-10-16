//! `Type#Value` in an expression renders back as written, in every position a
//! value stands.

use super::common::*;

const SOURCE: &str = "
TYPE
E_State : (Idle, Run, Stop);
END_TYPE
FUNCTION is_run : BOOL
VAR_INPUT
    v : E_State;
END_VAR
    is_run := v = E_State#Run;
END_FUNCTION
PROGRAM main
VAR
    s : E_State;
    b : BOOL;
END_VAR
    s := E_State#Run;
    b := s = E_State#Stop;
    b := is_run(E_State#Run);
    b := is_run(v := E_State#Idle);
    CASE s OF
        E_State#Run:
            b := TRUE;
        Idle:
            b := FALSE;
        E_State#Stop:
            b := FALSE;
    END_CASE;
END_PROGRAM
";

#[test]
fn write_to_string_when_qualified_value_in_expressions_and_case_labels_then_round_trips() {
    assert_round_trips(SOURCE, &CompilerOptions::default());
}

#[test]
fn write_to_string_when_qualified_value_in_a_codesys_program_then_round_trips() {
    assert_round_trips(SOURCE, &CompilerOptions::from_dialect(Dialect::Codesys));
}
