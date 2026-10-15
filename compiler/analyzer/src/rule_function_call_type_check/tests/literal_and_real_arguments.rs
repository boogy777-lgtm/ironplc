//! Tests for literal arguments and real-number arguments passed to function parameters.

use ironplc_problems::Problem;

rule_ctx_ok!(
    apply_when_bare_literal_arg_to_int_param_then_ok,
    "
FUNCTION ADD_ONE : INT
VAR_INPUT
    x : INT;
END_VAR
    ADD_ONE := x + 1;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
END_VAR
    result := ADD_ONE(5);
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_bare_literal_arg_to_sint_param_then_ok,
    "
FUNCTION INC : SINT
VAR_INPUT
    x : SINT;
END_VAR
    INC := x;
END_FUNCTION

PROGRAM main
VAR
    result : SINT;
END_VAR
    result := INC(5);
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_bare_real_literal_arg_to_lreal_param_then_ok,
    "
FUNCTION DBL : LREAL
VAR_INPUT
    x : LREAL;
END_VAR
    DBL := x;
END_FUNCTION

PROGRAM main
VAR
    result : LREAL;
END_VAR
    result := DBL(3.14);
END_PROGRAM"
);

// REAL -> LREAL is lossless, standard widening (unlike the bare
// literal case above, this argument is a typed REAL variable, not
// an untyped ANY_REAL literal -- a separate code path through
// ElementaryTypeName::can_widen_to()).
rule_ctx_ok!(
    apply_when_typed_real_var_arg_to_lreal_param_then_ok,
    "
FUNCTION DBL : LREAL
VAR_INPUT
    x : LREAL;
END_VAR
    DBL := x;
END_FUNCTION

PROGRAM main
VAR
    input : REAL;
    result : LREAL;
END_VAR
    result := DBL(input);
END_PROGRAM"
);

// The reverse direction (LREAL -> REAL) is narrowing and must
// remain an error -- guards against accidentally allowing both
// directions.
rule_ctx_err!(
    apply_when_typed_lreal_var_arg_to_real_param_then_error,
    "
FUNCTION SNGL : REAL
VAR_INPUT
    x : REAL;
END_VAR
    SNGL := x;
END_FUNCTION

PROGRAM main
VAR
    input : LREAL;
    result : REAL;
END_VAR
    result := SNGL(input);
END_PROGRAM"
);

rule_ctx_err1!(
    apply_when_typed_dint_literal_arg_to_int_param_then_error,
    "
FUNCTION ADD_ONE : INT
VAR_INPUT
    x : INT;
END_VAR
    ADD_ONE := x;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
END_VAR
    result := ADD_ONE(DINT#5);
END_PROGRAM",
    Problem::FunctionCallArgTypeMismatch
);

rule_ctx_err1!(
    apply_when_dint_var_arg_to_int_param_then_error,
    "
FUNCTION ADD_ONE : INT
VAR_INPUT
    x : INT;
END_VAR
    ADD_ONE := x;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    y : DINT;
END_VAR
    result := ADD_ONE(y);
END_PROGRAM",
    Problem::FunctionCallArgTypeMismatch
);

rule_ctx_ok!(
    apply_when_bare_int_literal_arg_to_real_param_then_ok,
    "
FUNCTION TAKES_REAL : REAL
VAR_INPUT
    x : REAL;
END_VAR
    TAKES_REAL := x;
END_FUNCTION

PROGRAM main
VAR
    result : REAL;
END_VAR
    result := TAKES_REAL(0);
END_PROGRAM
"
);

rule_ctx_ok!(
    apply_when_bare_int_literal_arg_to_lreal_param_then_ok,
    "
FUNCTION TAKES_LREAL : LREAL
VAR_INPUT
    x : LREAL;
END_VAR
    TAKES_LREAL := x;
END_FUNCTION

PROGRAM main
VAR
    result : LREAL;
END_VAR
    result := TAKES_LREAL(42);
END_PROGRAM
"
);
