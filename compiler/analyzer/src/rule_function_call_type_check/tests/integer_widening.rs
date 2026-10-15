//! Tests for implicit integer widening of function-call arguments and returns (ADR-0029).

// --- Implicit integer widening tests (ADR-0029) ---

rule_ctx_ok!(
    apply_when_sint_arg_to_int_param_then_ok,
    "
FUNCTION TAKES_INT : INT
VAR_INPUT
    x : INT;
END_VAR
    TAKES_INT := x;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    y : SINT;
END_VAR
    result := TAKES_INT(y);
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_int_arg_to_dint_param_then_ok,
    "
FUNCTION TAKES_DINT : DINT
VAR_INPUT
    x : DINT;
END_VAR
    TAKES_DINT := x;
END_FUNCTION

PROGRAM main
VAR
    result : DINT;
    y : INT;
END_VAR
    result := TAKES_DINT(y);
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_sint_arg_to_lint_param_then_ok,
    "
FUNCTION TAKES_LINT : LINT
VAR_INPUT
    x : LINT;
END_VAR
    TAKES_LINT := x;
END_FUNCTION

PROGRAM main
VAR
    result : LINT;
    y : SINT;
END_VAR
    result := TAKES_LINT(y);
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_usint_arg_to_uint_param_then_ok,
    "
FUNCTION TAKES_UINT : UINT
VAR_INPUT
    x : UINT;
END_VAR
    TAKES_UINT := x;
END_FUNCTION

PROGRAM main
VAR
    result : UINT;
    y : USINT;
END_VAR
    result := TAKES_UINT(y);
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_usint_arg_to_int_param_then_ok,
    "
FUNCTION TAKES_INT : INT
VAR_INPUT
    x : INT;
END_VAR
    TAKES_INT := x;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    y : USINT;
END_VAR
    result := TAKES_INT(y);
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_uint_arg_to_dint_param_then_ok,
    "
FUNCTION TAKES_DINT : DINT
VAR_INPUT
    x : DINT;
END_VAR
    TAKES_DINT := x;
END_FUNCTION

PROGRAM main
VAR
    result : DINT;
    y : UINT;
END_VAR
    result := TAKES_DINT(y);
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_sint_return_to_dint_var_then_ok,
    "
FUNCTION GET_SINT : SINT
VAR_INPUT
    x : SINT;
END_VAR
    GET_SINT := x;
END_FUNCTION

PROGRAM main
VAR
    result : DINT;
    y : SINT;
END_VAR
    result := GET_SINT(y);
END_PROGRAM"
);

rule_ctx_err!(
    apply_when_dint_arg_to_int_param_then_error,
    "
FUNCTION TAKES_INT : INT
VAR_INPUT
    x : INT;
END_VAR
    TAKES_INT := x;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    y : DINT;
END_VAR
    result := TAKES_INT(y);
END_PROGRAM"
);

rule_ctx_err!(
    apply_when_int_arg_to_uint_param_then_error,
    "
FUNCTION TAKES_UINT : UINT
VAR_INPUT
    x : UINT;
END_VAR
    TAKES_UINT := x;
END_FUNCTION

PROGRAM main
VAR
    result : UINT;
    y : INT;
END_VAR
    result := TAKES_UINT(y);
END_PROGRAM"
);

rule_ctx_err!(
    apply_when_byte_arg_to_int_param_then_error,
    "
FUNCTION TAKES_INT : INT
VAR_INPUT
    x : INT;
END_VAR
    TAKES_INT := x;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    y : BYTE;
END_VAR
    result := TAKES_INT(y);
END_PROGRAM"
);

// --- Integration tests for new widening cases ---

rule_ctx_ok!(
    apply_when_int_arg_to_real_param_then_ok,
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
    y : INT;
END_VAR
    result := TAKES_REAL(y);
END_PROGRAM"
);

rule_ctx_err!(
    apply_when_dint_arg_to_real_param_then_error,
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
    y : DINT;
END_VAR
    result := TAKES_REAL(y);
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_byte_arg_to_word_param_then_ok,
    "
FUNCTION TAKES_WORD : WORD
VAR_INPUT
    x : WORD;
END_VAR
    TAKES_WORD := x;
END_FUNCTION

PROGRAM main
VAR
    result : WORD;
    y : BYTE;
END_VAR
    result := TAKES_WORD(y);
END_PROGRAM"
);

rule_ctx_err!(
    apply_when_word_arg_to_byte_param_then_error,
    "
FUNCTION TAKES_BYTE : BYTE
VAR_INPUT
    x : BYTE;
END_VAR
    TAKES_BYTE := x;
END_FUNCTION

PROGRAM main
VAR
    result : BYTE;
    y : WORD;
END_VAR
    result := TAKES_BYTE(y);
END_PROGRAM"
);

rule_ctx_err!(
    apply_when_real_arg_to_int_param_then_error,
    "
FUNCTION TAKES_INT : INT
VAR_INPUT
    x : INT;
END_VAR
    TAKES_INT := x;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    y : REAL;
END_VAR
    result := TAKES_INT(y);
END_PROGRAM"
);
