//! Tests that the body of every kind of POU that declares a `VAR_TEMP` sees it.

use ironplc_parser::options::CompilerOptions;

use super::*;

/// The body of every POU that declares a `VAR_TEMP` sees it. The rule keeps no
/// list of the kinds of POU: a declaration enters the scope of its POU whatever
/// its section, so a new POU kind that admits `VAR_TEMP` is a row.
#[rstest::rstest]
#[case::program("PROGRAM main VAR_TEMP t : INT; END_VAR t := t + 1; END_PROGRAM")]
#[case::function("FUNCTION f : INT VAR_TEMP t : INT; END_VAR t := 1; f := t; END_FUNCTION")]
#[case::function_block(
    "FUNCTION_BLOCK fb VAR_OUTPUT y : INT; END_VAR VAR_TEMP t : INT; END_VAR t := 1; y := t; END_FUNCTION_BLOCK"
)]
#[case::method(
    "FUNCTION_BLOCK fb METHOD m : INT VAR_TEMP t : INT; END_VAR t := 1; m := t; END_METHOD END_FUNCTION_BLOCK"
)]
fn apply_when_body_uses_its_var_temp_then_declared(#[case] program: &str) {
    let options = CompilerOptions {
        allow_fb_inheritance: true,
        ..CompilerOptions::default()
    };
    let (library, context) =
        crate::test_helpers::parse_and_resolve_types_with_options(program, &options);

    let result = apply(&library, &context, &options);

    assert!(result.is_ok(), "unexpected errors: {result:?}");
}
