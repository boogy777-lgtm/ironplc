//! Tests for the argument and return types of function calls, user-defined and standard-library.

use super::apply;
use crate::test_helpers::parse_and_resolve_types_with_context;
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::Problem;
use rstest::rstest;

rule_ctx_ok!(
    apply_when_matching_types_then_ok,
    "
FUNCTION ADD_INTS : INT
VAR_INPUT
    A : INT;
    B : INT;
END_VAR
    ADD_INTS := A + B;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    a : INT;
    b : INT;
END_VAR
    result := ADD_INTS(a, b);
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_int_arg_to_real_param_lossless_then_ok,
    "
FUNCTION DOUBLE_REAL : REAL
VAR_INPUT
    A : REAL;
END_VAR
    DOUBLE_REAL := A;
END_FUNCTION

PROGRAM main
VAR
    result : REAL;
    x : INT;
END_VAR
    result := DOUBLE_REAL(x);
END_PROGRAM"
);

rule_ctx_err1!(
    apply_when_dint_arg_to_real_param_lossy_then_error,
    "
FUNCTION DOUBLE_REAL : REAL
VAR_INPUT
    A : REAL;
END_VAR
    DOUBLE_REAL := A;
END_FUNCTION

PROGRAM main
VAR
    result : REAL;
    x : DINT;
END_VAR
    result := DOUBLE_REAL(x);
END_PROGRAM",
    Problem::FunctionCallArgTypeMismatch
);

rule_ctx_ok!(
    apply_when_stdlib_arg_matches_param_then_ok,
    "
PROGRAM main
VAR
    result : REAL;
    x : INT;
END_VAR
    result := INT_TO_REAL(x);
END_PROGRAM"
);

// A standard-library return type is checked against the assignment
// target like a user-defined one: INT_TO_REAL yields REAL, which does
// not fit an INT.
rule_ctx_err1!(
    apply_when_stdlib_return_type_mismatches_target_then_error,
    "
PROGRAM main
VAR
    result : INT;
    x : INT;
END_VAR
    result := INT_TO_REAL(x);
END_PROGRAM",
    Problem::FunctionCallReturnTypeMismatch
);

// Integer widening still applies to a standard-library return
// (ADR-0029, ADR-0031): INT_TO_DINT yields DINT, which fits a LINT.
rule_ctx_ok!(
    apply_when_stdlib_return_widens_to_target_then_ok,
    "
PROGRAM main
VAR
    result : LINT;
    x : INT;
END_VAR
    result := INT_TO_DINT(x);
END_PROGRAM"
);

// A generic return type is narrowed by `xform_resolve_expr_types` before
// this rule runs, so ADD over INT arguments is an INT return and not a
// false P4027 against the ANY_NUM the signature declares.
rule_ctx_ok!(
    apply_when_stdlib_generic_return_resolves_to_target_then_ok,
    "
PROGRAM main
VAR
    result : INT;
    a : INT;
    b : INT;
END_VAR
    result := ADD(a, b);
END_PROGRAM"
);

/// The function forms of the bitwise boolean operators accept every
/// ANY_BIT type, as the operators do (#1567).
#[rstest]
#[case::and_bool("AND", "BOOL")]
#[case::and_byte("AND", "BYTE")]
#[case::and_word("AND", "WORD")]
#[case::and_dword("AND", "DWORD")]
#[case::and_lword("AND", "LWORD")]
#[case::or_word("OR", "WORD")]
#[case::xor_word("XOR", "WORD")]
fn apply_when_bitwise_function_form_on_bit_string_then_ok(
    #[case] function: &str,
    #[case] type_name: &str,
) {
    let program = format!(
        "
PROGRAM main
VAR
    a : {type_name};
    b : {type_name};
    result : {type_name};
END_VAR
    result := {function}(a, b);
END_PROGRAM"
    );
    let (library, context) = parse_and_resolve_types_with_context(&program);
    let result = apply(&library, &context, &CompilerOptions::default());
    assert!(result.is_ok(), "{result:?}");
}

rule_ctx_errn!(
    apply_when_bitwise_function_form_on_int_then_error_per_argument,
    "
PROGRAM main
VAR
    a : INT;
    b : INT;
    result : INT;
END_VAR
    result := AND(a, b);
END_PROGRAM",
    2,
    Problem::FunctionCallArgTypeMismatch
);

// An extensible call is checked past its declared parameters, so the
// third input of ADD is checked like the first two (#1618).
rule_ctx_ok!(
    apply_when_extensible_call_third_arg_matches_then_ok,
    "
PROGRAM main
VAR
    a : DINT;
    b : DINT;
    c : DINT;
    result : DINT;
END_VAR
    result := ADD(a, b, c);
END_PROGRAM"
);

rule_ctx_err1!(
    apply_when_extensible_call_third_arg_mismatch_then_error,
    "
PROGRAM main
VAR
    a : WORD;
    b : WORD;
    c : STRING;
    result : WORD;
END_VAR
    result := AND(a, b, c);
END_PROGRAM",
    Problem::FunctionCallArgTypeMismatch
);

rule_ctx_err1!(
    apply_when_mux_fourth_input_mismatch_then_error,
    "
PROGRAM main
VAR
    a : DINT;
    s : STRING;
    result : DINT;
END_VAR
    result := MUX(0, a, a, a, s);
END_PROGRAM",
    Problem::FunctionCallArgTypeMismatch
);

// A VAR_IN_OUT declared before a VAR_INPUT takes the first positional
// argument, so a mismatch on the second names the VAR_INPUT (#1658).
#[test]
fn apply_when_in_out_before_input_mismatch_then_names_input_parameter() {
    let program = "
FUNCTION Scale : INT
VAR_IN_OUT acc : INT; END_VAR
VAR_INPUT factor : INT; END_VAR
    acc := acc * factor;
    Scale := acc;
END_FUNCTION

PROGRAM main
VAR
    total : INT;
    s : STRING;
    result : INT;
END_VAR
    result := Scale(total, s);
END_PROGRAM";
    let (library, context) = parse_and_resolve_types_with_context(program);
    let errors = apply(&library, &context, &CompilerOptions::default()).unwrap_err();
    assert_eq!(errors.len(), 1, "{errors:?}");
    assert_eq!(errors[0].code, Problem::FunctionCallArgTypeMismatch.code());
    assert!(
        errors[0].described.contains(&"parameter=factor".to_owned()),
        "{:?}",
        errors[0].described
    );
}

// NOT(x) parses as the unary operator; the named-argument spelling is the
// one that reaches the function signature.
rule_ctx_ok!(
    apply_when_not_function_form_on_word_then_ok,
    "
PROGRAM main
VAR
    a : WORD;
    result : WORD;
END_VAR
    result := NOT(IN := a);
END_PROGRAM"
);

rule_ctx_err1!(
    apply_when_multiple_args_one_mismatch_then_one_error,
    "
FUNCTION MY_FUNC : INT
VAR_INPUT
    A : INT;
    B : SINT;
END_VAR
    MY_FUNC := A;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    x : INT;
END_VAR
    result := MY_FUNC(x, x);
END_PROGRAM",
    Problem::FunctionCallArgTypeMismatch
);

rule_ctx_err1!(
    apply_when_return_type_mismatch_then_error,
    "
FUNCTION GET_VALUE : REAL
VAR_INPUT
    A : REAL;
END_VAR
    GET_VALUE := A;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    x : REAL;
END_VAR
    result := GET_VALUE(x);
END_PROGRAM",
    Problem::FunctionCallReturnTypeMismatch
);

rule_ctx_ok!(
    apply_when_nested_function_call_types_match_then_ok,
    "
FUNCTION DOUBLE : INT
VAR_INPUT
    A : INT;
END_VAR
    DOUBLE := A + A;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    x : INT;
END_VAR
    result := DOUBLE(DOUBLE(x));
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_all_args_match_then_ok,
    "
FUNCTION ADD3 : DINT
VAR_INPUT
    A : DINT;
    B : DINT;
    C : DINT;
END_VAR
    ADD3 := A + B + C;
END_FUNCTION

PROGRAM main
VAR
    result : DINT;
    a : DINT;
    b : DINT;
    c : DINT;
END_VAR
    result := ADD3(a, b, c);
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_return_type_matches_then_ok,
    "
FUNCTION GET_REAL : REAL
VAR_INPUT
    A : REAL;
END_VAR
    GET_REAL := A;
END_FUNCTION

PROGRAM main
VAR
    result : REAL;
    x : REAL;
END_VAR
    result := GET_REAL(x);
END_PROGRAM"
);
