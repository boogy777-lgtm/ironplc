//! Spec conformance tests for the CODESYS special operators
//! (analyzer-owned requirements): typing of `__NEW`, `__DELETE`, `__TYPEOF`
//! and `__XADD`, the operand diagnostics, the recognised-but-unsupported
//! names, and dialect gating.
//!
//! Each test is annotated with `#[spec_test(REQ_CS_analyzer_NNN)]`, which adds
//! `#[test]` and references a build-script-generated constant so the test fails
//! to compile if the requirement is removed from the spec.
//!
//! See `specs/design/codesys-st-surface-syntax.md`.

use ironplc_dsl::core::FileId;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_parser::parse_program;
use ironplc_problems::Problem;
use rstest::rstest;
use spec_test_macro::spec_test;

use crate::stages::analyze;

fn codesys() -> CompilerOptions {
    CompilerOptions::from_dialect(Dialect::Codesys)
}

/// Analyzes a function block with `declarations` and `body`, returning the
/// problem codes.
fn analyze_codes(declarations: &str, body: &str, options: &CompilerOptions) -> Vec<String> {
    let program = format!(
        "
TYPE S : STRUCT a : INT; END_STRUCT; END_TYPE

FUNCTION_BLOCK FB_Example
VAR
{declarations}
END_VAR
{body}
END_FUNCTION_BLOCK"
    );
    let library = parse_program(&program, &FileId::default(), options).unwrap();
    let (_library, context) = analyze(&[&library], options).unwrap();
    context
        .diagnostics()
        .iter()
        .map(|d| d.code.clone())
        .collect()
}

fn code(problem: Problem) -> String {
    problem.code().to_string()
}

/// REQ-CS-analyzer-003: `__NEW(T)` and `__NEW(T, n)` have the type
/// `POINTER TO T`.
#[spec_test(REQ_CS_analyzer_003)]
fn analyzer_spec_req_cs_003_new_is_pointer_to_its_type() {
    let declarations = "    p : POINTER TO INT;\n    q : POINTER TO S;";
    for body in ["p := __NEW(INT);", "p := __NEW(INT, 4);", "q := __NEW(S);"] {
        let codes = analyze_codes(declarations, body, &codesys());
        assert!(codes.is_empty(), "{body}: {codes:?}");
    }
}

#[rstest]
#[case::not_a_reference("n : INT;", "n := __NEW(INT);")]
#[case::other_pointee_when_punning_off("p : POINTER TO DINT;", "p := __NEW(INT);")]
fn analyze_when_new_assigned_to_mismatched_target_then_reference_mismatch(
    #[case] declarations: &str,
    #[case] body: &str,
) {
    let options = CompilerOptions {
        allow_ref_type_punning: false,
        ..codesys()
    };
    let codes = analyze_codes(declarations, body, &options);
    assert_eq!(codes, vec![code(Problem::ReferenceTypeMismatch)], "{body}");
}

/// REQ-CS-analyzer-004: `__NEW` operand and position errors are P4073.
#[spec_test(REQ_CS_analyzer_004)]
#[rstest]
#[case::operand_is_a_variable("x : INT;\n    p : POINTER TO INT;", "p := __NEW(x);")]
#[case::operand_is_unknown("p : POINTER TO INT;", "p := __NEW(Nope);")]
#[case::count_is_real("p : POINTER TO INT;", "p := __NEW(INT, 2.5);")]
#[case::count_on_user_type("q : POINTER TO S;", "q := __NEW(S, 3);")]
#[case::result_not_assigned("p : POINTER TO INT;\n    b : BOOL;", "b := p = __NEW(INT);")]
fn analyzer_spec_req_cs_004_new_operand_errors_are_p4073(
    #[case] declarations: &str,
    #[case] body: &str,
) {
    let codes = analyze_codes(declarations, body, &codesys());
    assert_eq!(
        codes,
        vec![code(Problem::SpecialOperatorOperandInvalid)],
        "{body}"
    );
}

/// REQ-CS-analyzer-005: `__DELETE(p)` is `BOOL` and needs a pointer.
#[spec_test(REQ_CS_analyzer_005)]
fn analyzer_spec_req_cs_005_delete_is_bool_and_needs_a_pointer() {
    let declarations = "    p : POINTER TO INT;\n    ok : BOOL;\n    n : INT;";
    let accepted = analyze_codes(declarations, "ok := __DELETE(p);", &codesys());
    assert!(accepted.is_empty(), "{accepted:?}");

    let rejected = analyze_codes(declarations, "ok := __DELETE(n);", &codesys());
    assert_eq!(rejected, vec![code(Problem::SpecialOperatorOperandInvalid)]);

    // BOOL: it is not an INT.
    let typed = analyze_codes(declarations, "n := __DELETE(p);", &codesys());
    assert_eq!(typed, vec![code(Problem::FunctionCallReturnTypeMismatch)]);
}

/// REQ-CS-analyzer-006: `__TYPEOF(x)` is `INT` and takes a type or an
/// expression.
#[spec_test(REQ_CS_analyzer_006)]
fn analyzer_spec_req_cs_006_typeof_is_int_and_takes_a_type_or_expression() {
    let declarations = "    info : INT;\n    r : REAL;\n    inst : S;\n    ok : BOOL;";
    for body in [
        "info := __TYPEOF(INT);",
        "info := __TYPEOF(S);",
        "info := __TYPEOF(r);",
        "info := __TYPEOF(inst);",
    ] {
        let codes = analyze_codes(declarations, body, &codesys());
        assert!(codes.is_empty(), "{body}: {codes:?}");
    }

    let typed = analyze_codes(declarations, "ok := __TYPEOF(INT);", &codesys());
    assert_eq!(typed, vec![code(Problem::FunctionCallReturnTypeMismatch)]);
}

#[test]
fn analyze_when_typeof_operand_is_undeclared_then_undefined_variable() {
    let codes = analyze_codes("    info : INT;", "info := __TYPEOF(zzz);", &codesys());
    assert_eq!(codes, vec![code(Problem::VariableUndefined)]);
}

/// REQ-CS-analyzer-007: `__XADD(p, v)` is `DINT` and needs a
/// `POINTER TO DINT`.
#[spec_test(REQ_CS_analyzer_007)]
fn analyzer_spec_req_cs_007_xadd_is_dint_and_needs_pointer_to_dint() {
    let declarations = "    p : POINTER TO DINT;\n    r : DINT;\n    i : POINTER TO INT;\n    d : DINT;\n    n : INT;";
    for body in [
        "r := __XADD(p, 1);",
        "r := __XADD(p, d);",
        "r := __XADD(p, n);",
    ] {
        let codes = analyze_codes(declarations, body, &codesys());
        assert!(codes.is_empty(), "{body}: {codes:?}");
    }
    for body in ["r := __XADD(i, 1);", "r := __XADD(d, 1);"] {
        let codes = analyze_codes(declarations, body, &codesys());
        assert_eq!(
            codes,
            vec![code(Problem::SpecialOperatorOperandInvalid)],
            "{body}"
        );
    }
    // The value must be an integer (checked by the argument type rule).
    let real = analyze_codes(declarations, "r := __XADD(p, 1.5);", &codesys());
    assert_eq!(real, vec![code(Problem::FunctionCallArgTypeMismatch)]);
    // DINT: not an INT.
    let typed = analyze_codes(declarations, "n := __XADD(p, 1);", &codesys());
    assert_eq!(typed, vec![code(Problem::FunctionCallReturnTypeMismatch)]);
}

/// REQ-CS-analyzer-008: the names whose types come from the target's system
/// library are reported as unsupported, not as undeclared.
#[spec_test(REQ_CS_analyzer_008)]
#[rstest]
#[case::currenttask("t := __CURRENTTASK;")]
#[case::currenttask_member("t := __CURRENTTASK.TaskIndex;")]
#[case::lower_case("t := __currenttask;")]
#[case::system("t := __SYSTEM.some_global;")]
#[case::pool("t := __POOL.other;")]
fn analyzer_spec_req_cs_008_recognised_but_unsupported_names_are_p4074(#[case] body: &str) {
    let codes = analyze_codes("    t : INT;", body, &codesys());
    assert_eq!(
        codes,
        vec![code(Problem::SpecialOperatorNotSupported)],
        "{body}"
    );
}

#[test]
fn analyze_when_system_name_is_declared_then_a_variable() {
    let codes = analyze_codes(
        "    __SYSTEM : S;\n    t : INT;",
        "t := __SYSTEM.a;",
        &codesys(),
    );
    assert!(codes.is_empty(), "{codes:?}");
}

/// REQ-CS-analyzer-009: with the flag off nothing is recognised.
#[spec_test(REQ_CS_analyzer_009)]
fn analyzer_spec_req_cs_009_operators_are_undeclared_without_the_flag() {
    let options = CompilerOptions {
        allow_special_operators: false,
        ..codesys()
    };
    let declarations = "    p : POINTER TO DINT;\n    r : DINT;\n    t : INT;";
    for body in [
        "p := __NEW(DINT);",
        "r := __DELETE(p);",
        "t := __TYPEOF(r);",
        "r := __XADD(p, 1);",
    ] {
        let codes = analyze_codes(declarations, body, &options);
        assert!(
            codes.contains(&code(Problem::FunctionCallUndeclared)),
            "{body}: {codes:?}"
        );
        assert!(
            !codes.contains(&code(Problem::SpecialOperatorOperandInvalid)),
            "{body}: {codes:?}"
        );
    }
    for body in ["t := __CURRENTTASK;", "t := __SYSTEM.x;", "t := __POOL.x;"] {
        let codes = analyze_codes(declarations, body, &options);
        assert_eq!(codes, vec![code(Problem::VariableUndefined)], "{body}");
    }
}

#[rstest]
#[case::ed2(Dialect::Iec61131_3Ed2)]
#[case::ed3(Dialect::Iec61131_3Ed3)]
#[case::rusty(Dialect::Rusty)]
#[case::twincat(Dialect::TwinCat)]
fn analyze_when_dialect_does_not_enable_operators_then_xadd_is_undeclared(
    #[case] dialect: Dialect,
) {
    let options = CompilerOptions::from_dialect(dialect);
    let codes = analyze_codes(
        "    r : DINT;\n    d : DINT;",
        "r := __XADD(d, 1);",
        &options,
    );
    assert!(
        codes.contains(&code(Problem::FunctionCallUndeclared)),
        "{codes:?}"
    );
}

#[test]
fn analyze_when_any_parameter_given_structure_variable_then_accepted() {
    // `ANY` is not limited to elementary types, so a function that takes it
    // (`SIZEOF`, `__TYPEOF`) takes a structure variable.
    let declarations = "    inst : S;\n    n : DINT;";
    for body in ["n := SIZEOF(inst);", "n := __TYPEOF(inst);"] {
        let codes = analyze_codes(declarations, body, &codesys());
        assert!(codes.is_empty(), "{body}: {codes:?}");
    }
}
