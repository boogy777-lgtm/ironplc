//! Tests for argument types checked against standard-library function signatures.

use ironplc_problems::Problem;

// --- Standard-library argument type checks ---

rule_ctx_err_code!(
    apply_when_stdlib_sin_arg_is_bool_then_arg_type_error,
    "
PROGRAM main
VAR
    b : BOOL;
    r : REAL;
END_VAR
    r := SIN(b);
END_PROGRAM",
    Problem::FunctionCallArgTypeMismatch
);

rule_ctx_ok!(
    apply_when_stdlib_sin_arg_is_real_then_ok,
    "
PROGRAM main
VAR
    x : REAL;
    r : REAL;
END_VAR
    r := SIN(x);
END_PROGRAM"
);

// UINT_TO_REAL expects UINT, but the argument is UDINT.
rule_ctx_err1!(
    apply_when_wrong_conversion_function_arg_then_arg_type_error,
    "
PROGRAM main
VAR
    u : UDINT;
    r : REAL;
END_VAR
    r := UINT_TO_REAL(u);
END_PROGRAM",
    Problem::FunctionCallArgTypeMismatch
);

rule_ctx_ok!(
    apply_when_correct_conversion_function_arg_then_ok,
    "
PROGRAM main
VAR
    u : UDINT;
    r : REAL;
END_VAR
    r := UDINT_TO_REAL(u);
END_PROGRAM"
);

// ABS accepts ANY_NUM; a bare integer literal is accepted.
rule_ctx_ok!(
    apply_when_stdlib_int_literal_arg_to_real_param_then_ok,
    "
PROGRAM main
VAR
    r : REAL;
END_VAR
    r := SQRT(2.0);
END_PROGRAM"
);

// Call arguments go through the same resolved type, so a wide literal
// reaches a WSTRING parameter and a narrow one does not.
rule_ctx_ok!(
    apply_when_wstring_parameter_given_wide_literal_then_ok,
    "
FUNCTION wide_len : INT
VAR_INPUT
    w : WSTRING[10];
END_VAR
    wide_len := LEN(w);
END_FUNCTION

PROGRAM main
VAR
    n : INT;
END_VAR
    n := wide_len(\"abc\");
END_PROGRAM"
);

rule_ctx_err1!(
    apply_when_wstring_parameter_given_narrow_literal_then_error,
    "
FUNCTION wide_len : INT
VAR_INPUT
    w : WSTRING[10];
END_VAR
    wide_len := LEN(w);
END_FUNCTION

PROGRAM main
VAR
    n : INT;
END_VAR
    n := wide_len('abc');
END_PROGRAM",
    Problem::FunctionCallArgTypeMismatch
);
