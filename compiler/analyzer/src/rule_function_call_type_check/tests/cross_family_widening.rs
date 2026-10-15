//! Tests for the cross-family widening flags applied to function-call arguments and returns (ADR-0031).

use super::apply;
use crate::test_helpers::parse_and_resolve_types_with_context;
use ironplc_parser::options::CompilerOptions;
use rstest::rstest;

// --- Cross-family widening tests (ADR-0031, requires flag) ---

/// Cross-family widening on function-call arguments/returns with
/// `--allow-cross-family-widening` enabled (ADR-0031), against a resolved
/// context. Each case resolves the program, applies the rule with the flag
/// on, and asserts the expected outcome; each row still runs as an
/// individually-named test.
#[rstest]
#[case::byte_arg_to_int_param_ok(
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
END_PROGRAM",
    true
)]
#[case::literal_zero_to_byte_param_ok(
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
END_VAR
    result := TAKES_BYTE(0);
END_PROGRAM",
    true
)]
#[case::byte_return_to_int_var_ok(
    "
FUNCTION GET_BYTE : BYTE
VAR_INPUT
    x : BYTE;
END_VAR
    GET_BYTE := x;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    y : BYTE;
END_VAR
    result := GET_BYTE(y);
END_PROGRAM",
    true
)]
// Integer → bit-string is allowed only for the UDINT ↔ DWORD pair, which
// ElementaryTypeName::can_widen_cross_family_to carves out at equal width.
#[case::udint_arg_to_dword_param_ok(
    "
FUNCTION TAKES_DWORD : DWORD
VAR_INPUT
    x : DWORD;
END_VAR
    TAKES_DWORD := x;
END_FUNCTION

PROGRAM main
VAR
    result : DWORD;
    y : UDINT;
END_VAR
    result := TAKES_DWORD(y);
END_PROGRAM",
    true
)]
// INT → BYTE is not that pair, so it stays an error even with the flag.
#[case::int_arg_to_byte_param_error(
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
    y : INT;
END_VAR
    result := TAKES_BYTE(y);
END_PROGRAM",
    false
)]
fn apply_when_cross_family_flags_on_call_then_matches_expectation(
    #[case] program: &str,
    #[case] expect_ok: bool,
) {
    let (library, context) = parse_and_resolve_types_with_context(program);
    let opts = CompilerOptions {
        allow_cross_family_widening: true,
        allow_cross_family_conversion: true,
        allow_int_literal_to_bit_string: true,
        ..CompilerOptions::default()
    };
    let result = apply(&library, &context, &opts);
    assert_eq!(result.is_ok(), expect_ok);
}

/// One program per cross-family rule. Each is accepted under exactly one
/// of the three flags, which is what makes them three flags rather than
/// one: enabling any other flag leaves the program rejected.
const WIDENING_PROGRAM: &str = "
FUNCTION TAKES_INT : INT
VAR_INPUT
    x : INT;
END_VAR
    TAKES_INT := x;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    b : BYTE;
END_VAR
    result := TAKES_INT(b);
END_PROGRAM";

const CONVERSION_PROGRAM: &str = "
FUNCTION TAKES_DWORD : DWORD
VAR_INPUT
    x : DWORD;
END_VAR
    TAKES_DWORD := x;
END_FUNCTION

PROGRAM main
VAR
    result : DWORD;
    u : UDINT;
END_VAR
    result := TAKES_DWORD(u);
END_PROGRAM";

const LITERAL_PROGRAM: &str = "
FUNCTION TAKES_BYTE : BYTE
VAR_INPUT
    x : BYTE;
END_VAR
    TAKES_BYTE := x;
END_FUNCTION

PROGRAM main
VAR
    result : BYTE;
END_VAR
    result := TAKES_BYTE(0);
END_PROGRAM";

fn only(flag: &str) -> CompilerOptions {
    let mut opts = CompilerOptions::default();
    match flag {
        "widening" => opts.allow_cross_family_widening = true,
        "conversion" => opts.allow_cross_family_conversion = true,
        "literal" => opts.allow_int_literal_to_bit_string = true,
        "none" => {}
        other => panic!("unknown cross-family flag {other}"),
    }
    opts
}

#[rstest]
#[case::widening_under_widening(WIDENING_PROGRAM, "widening", true)]
#[case::widening_under_conversion(WIDENING_PROGRAM, "conversion", false)]
#[case::widening_under_literal(WIDENING_PROGRAM, "literal", false)]
#[case::conversion_under_widening(CONVERSION_PROGRAM, "widening", false)]
#[case::conversion_under_conversion(CONVERSION_PROGRAM, "conversion", true)]
#[case::conversion_under_literal(CONVERSION_PROGRAM, "literal", false)]
#[case::literal_under_widening(LITERAL_PROGRAM, "widening", false)]
#[case::literal_under_conversion(LITERAL_PROGRAM, "conversion", false)]
#[case::literal_under_literal(LITERAL_PROGRAM, "literal", true)]
#[case::widening_under_none(WIDENING_PROGRAM, "none", false)]
#[case::conversion_under_none(CONVERSION_PROGRAM, "none", false)]
#[case::literal_under_none(LITERAL_PROGRAM, "none", false)]
fn apply_when_one_cross_family_flag_on_then_only_its_rule_is_accepted(
    #[case] program: &str,
    #[case] flag: &str,
    #[case] expect_ok: bool,
) {
    let (library, context) = parse_and_resolve_types_with_context(program);
    let result = apply(&library, &context, &only(flag));
    assert_eq!(result.is_ok(), expect_ok, "program under flag {flag:?}");
}

rule_ctx_err!(
    apply_when_literal_zero_to_byte_param_without_flag_then_error,
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
END_VAR
    result := TAKES_BYTE(0);
END_PROGRAM"
);

rule_ctx_err!(
    apply_when_byte_return_to_int_var_without_flag_then_error,
    "
FUNCTION GET_BYTE : BYTE
VAR_INPUT
    x : BYTE;
END_VAR
    GET_BYTE := x;
END_FUNCTION

PROGRAM main
VAR
    result : INT;
    y : BYTE;
END_VAR
    result := GET_BYTE(y);
END_PROGRAM"
);
