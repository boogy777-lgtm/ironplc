//! Tests for method-local scoping and for a function or method result variable as an assignment target.

use ironplc_parser::options::CompilerOptions;
use ironplc_problems::Problem;

// ---------------------------------------------------------------------
// METHOD scoping.
// ---------------------------------------------------------------------

fn apply_with_methods(program: &str) -> crate::result::SemanticResult {
    let options = CompilerOptions {
        allow_fb_inheritance: true,
        ..CompilerOptions::default()
    };
    let (library, context) =
        crate::test_helpers::parse_and_resolve_types_with_options(program, &options);
    super::apply(&library, &context, &options)
}

/// A method's local belongs to the method. It used to be recorded
/// against the enclosing function block, so it overwrote a field of
/// the same name for every method compiled after it -- and the
/// mismatch below was accepted because `v` was still recorded as the
/// `REAL` from `A`.
#[test]
fn apply_when_method_local_shadows_field_then_sibling_method_uses_field_type() {
    let errors = apply_with_methods(
        "
FUNCTION_BLOCK FB_Motor
VAR
    v : INT;
END_VAR
METHOD A
VAR
    v : REAL;
END_VAR
    v := 1.5;
END_METHOD
METHOD B
    v := 2.5;
END_METHOD
END_FUNCTION_BLOCK",
    )
    .unwrap_err();

    assert!(
        errors
            .iter()
            .any(|d| d.code == Problem::AssignmentTypeMismatch.code()),
        "expected an assignment type mismatch on the INT field, got {errors:?}"
    );
}

/// A method's locals are still checked against their own declared
/// types once they live in the method's own scope.
#[test]
fn apply_when_method_local_assigned_wrong_type_then_error() {
    let errors = apply_with_methods(
        "
FUNCTION_BLOCK FB_Motor
METHOD A
VAR
    b : BOOL;
    i : INT;
END_VAR
    b := i;
END_METHOD
END_FUNCTION_BLOCK",
    )
    .unwrap_err();

    assert!(errors
        .iter()
        .any(|d| d.code == Problem::AssignmentTypeMismatch.code()));
}

/// A method reading the instance's field is not a mismatch: the
/// method scope nests inside the function block's.
#[test]
fn apply_when_method_assigns_field_from_matching_local_then_ok() {
    assert!(apply_with_methods(
        "
FUNCTION_BLOCK FB_Motor
VAR
    speed : INT;
END_VAR
METHOD SetSpeed
VAR_INPUT
    newSpeed : INT;
END_VAR
    speed := newSpeed;
END_METHOD
END_FUNCTION_BLOCK",
    )
    .is_ok());
}

// ---------------------------------------------------------------------
// Result variables. A declaration's own name is an assignment target.
// ---------------------------------------------------------------------

rule_ctx_err_code!(
    apply_when_function_result_assigned_wrong_type_then_error,
    "
FUNCTION GetFlag : BOOL
VAR
    n : INT;
END_VAR
    GetFlag := n;
END_FUNCTION",
    Problem::AssignmentTypeMismatch,
);

rule_ctx_ok!(
    apply_when_function_result_assigned_correct_type_then_ok,
    "
FUNCTION GetN : INT
VAR
    n : INT;
END_VAR
    GetN := n;
END_FUNCTION"
);

#[test]
fn apply_when_method_result_assigned_wrong_type_then_error() {
    let errors = apply_with_methods(
        "
FUNCTION_BLOCK FB_Motor
METHOD GetFlag : BOOL
VAR
    n : INT;
END_VAR
    GetFlag := n;
END_METHOD
END_FUNCTION_BLOCK",
    )
    .unwrap_err();

    assert!(errors
        .iter()
        .any(|d| d.code == Problem::AssignmentTypeMismatch.code()));
}

#[test]
fn apply_when_method_result_assigned_correct_type_then_ok() {
    assert!(apply_with_methods(
        "
FUNCTION_BLOCK FB_Motor
METHOD GetN : INT
VAR
    n : INT;
END_VAR
    GetN := n;
END_METHOD
END_FUNCTION_BLOCK",
    )
    .is_ok());
}

/// A method with no return type has no result variable, so its name
/// is not an assignment target here either. The assignment is
/// rejected earlier, by `rule_use_declared_symbolic_var`; this pins
/// that this rule adds no target type for it.
#[test]
fn apply_when_method_has_no_return_type_then_name_is_not_a_target() {
    assert!(apply_with_methods(
        "
FUNCTION_BLOCK FB_Motor
METHOD DoThing
VAR
    n : INT;
END_VAR
    DoThing := n;
END_METHOD
END_FUNCTION_BLOCK",
    )
    .is_ok());
}
