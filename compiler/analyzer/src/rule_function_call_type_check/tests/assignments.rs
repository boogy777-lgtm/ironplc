//! Tests for the types of assignment statement values against their target variables (P4035).

use super::apply;
use crate::test_helpers::parse_and_resolve_types_with_context;
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::Problem;
use rstest::rstest;

// --- Assignment statement type checks (P4035) ---

// A character-string literal is typed by its delimiter (IEC 61131-3
// Table 5), so each spelling belongs to exactly one of the two targets.
rule_ctx_ok!(
    apply_when_wstring_target_assigned_wide_literal_then_ok,
    "
PROGRAM main
VAR
    w : WSTRING[10];
END_VAR
    w := \"abc\";
END_PROGRAM"
);

rule_ctx_err1!(
    apply_when_wstring_target_assigned_narrow_literal_then_error,
    "
PROGRAM main
VAR
    w : WSTRING[10];
END_VAR
    w := 'abc';
END_PROGRAM",
    Problem::AssignmentTypeMismatch
);

rule_ctx_ok!(
    apply_when_string_target_assigned_narrow_literal_then_ok,
    "
PROGRAM main
VAR
    s : STRING[10];
END_VAR
    s := 'abc';
END_PROGRAM"
);

rule_ctx_err1!(
    apply_when_string_target_assigned_wide_literal_then_error,
    "
PROGRAM main
VAR
    s : STRING[10];
END_VAR
    s := \"abc\";
END_PROGRAM",
    Problem::AssignmentTypeMismatch
);

rule_ctx_err1!(
    apply_when_bool_target_assigned_real_expr_then_error,
    "
PROGRAM main
VAR
    b : BOOL;
    x : REAL;
END_VAR
    b := x * 2.0;
END_PROGRAM",
    Problem::AssignmentTypeMismatch
);

rule_ctx_err1!(
    apply_when_int_target_assigned_real_var_then_error,
    "
PROGRAM main
VAR
    i : INT;
    r : REAL;
END_VAR
    i := r;
END_PROGRAM",
    Problem::AssignmentTypeMismatch
);

// INT widens losslessly to REAL, so this assignment is valid.
rule_ctx_ok!(
    apply_when_real_target_assigned_int_var_then_ok,
    "
PROGRAM main
VAR
    i : INT;
    r : REAL;
END_VAR
    r := i;
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_matching_assignment_then_ok,
    "
PROGRAM main
VAR
    i : INT;
    j : INT;
END_VAR
    i := j + 1;
END_PROGRAM"
);

/// Cross-family widening on assignment statements with
/// `--allow-cross-family-widening` enabled (ADR-0031), against a resolved
/// context. Each case resolves the program, applies the rule with the flag
/// on, and asserts the expected outcome; each row still runs as an
/// individually-named test.
#[rstest]
// UDINT -> DWORD is allowed even though the two are the same width, so it
// is a reinterpretation rather than a widening. Real TcXaeShell accepts it,
// which is why the rule is permissive here; ADR-0031 sets the cross-family
// policy but does not speak to the equal-width case.
#[case::dword_target_assigned_udint_var_ok(
    "
PROGRAM main
VAR
    dwFromUdint : DWORD;
    udValue : UDINT;
END_VAR
    dwFromUdint := udValue;
END_PROGRAM",
    true
)]
#[case::udint_target_assigned_dword_var_ok(
    "
PROGRAM main
VAR
    udFromDword : UDINT;
    dwValue : DWORD;
END_VAR
    udFromDword := dwValue;
END_PROGRAM",
    true
)]
// Signed integer, equal width -- not part of the verified exception, must
// stay rejected even with the flag on.
#[case::dword_target_assigned_dint_var_error(
    "
PROGRAM main
VAR
    dwFromDint : DWORD;
    diValue : DINT;
END_VAR
    dwFromDint := diValue;
END_PROGRAM",
    false
)]
fn apply_when_cross_family_flags_on_assignment_then_matches_expectation(
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

rule_ctx_err!(
    apply_when_dword_target_assigned_udint_var_without_flag_then_error,
    "
PROGRAM main
VAR
    dwFromUdint : DWORD;
    udValue : UDINT;
END_VAR
    dwFromUdint := udValue;
END_PROGRAM"
);

// Temporal short/long widths are treated as one family.
rule_ctx_ok!(
    apply_when_ltime_target_assigned_time_var_then_ok,
    "
PROGRAM main
VAR
    lt : LTIME;
    t : TIME;
END_VAR
    lt := t;
END_PROGRAM"
);
