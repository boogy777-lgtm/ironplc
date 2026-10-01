//! End-to-end execution of the CODESYS `PARAMS(n) OF T` type.
//!
//! A PARAMS list is indexed like an array, and it is lowered to one
//! (`ARRAY[0 .. n-1] OF T`) before code generation, so these tests prove the
//! lowering: the elements are read and written through the ordinary array
//! subscript code path.

use ironplc_parser::options::CompilerOptions;
use spec_test_macro::spec_test;

/// REQ-CS-codegen-001: A PARAMS list is laid out as the array it lowers to,
/// and its elements read and write through the array subscript path.
#[spec_test(REQ_CS_codegen_001)]
#[test]
fn end_to_end_when_params_indexed_then_lowers_to_array() {
    // The list occupies one variable slot (`args` is 0, `x` is 1).
    let source = "
PROGRAM main
  VAR
    args : PARAMS(3) OF INT;
    x : INT;
  END_VAR
  args[0] := 7;
  args[2] := 9;
  x := args[0] + args[2];
END_PROGRAM
";
    let options = CompilerOptions {
        allow_params_of: true,
        ..CompilerOptions::default()
    };
    crate::common::assert_run_i32_with(source, &options, &[(1, 16)]);
}

#[test]
fn end_to_end_when_named_params_type_then_lowers_to_array() {
    // `params` is a named PARAMS type; the declaration keeps the name and
    // resolves to the same list.
    let source = "
TYPE WordList : PARAMS(2) OF INT; END_TYPE
PROGRAM main
  VAR
    args : WordList;
    x : INT;
  END_VAR
  args[1] := 5;
  x := args[1] * 2;
END_PROGRAM
";
    let options = CompilerOptions {
        allow_params_of: true,
        ..CompilerOptions::default()
    };
    crate::common::assert_run_i32_with(source, &options, &[(1, 10)]);
}
