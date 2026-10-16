//! End-to-end tests for a member that the type does not declare.
//!
//! The check of the program refuses the name (P4080), as `ironplcc check` and
//! `ironplcc compile` do, so it never reaches code generation, which has no
//! slot for it (it was P9999 "Unknown field" there, and a name in an
//! initializer was compiled to nothing). A member that the type declares and
//! this compiler does not keep in the instance yet is still P9999.

use crate::common::checked::try_check_and_run;
use ironplc_parser::options::{CompilerOptions, Dialect};
use rstest::rstest;

const DECLARATIONS: &str = "
TYPE S : STRUCT m : DINT; END_STRUCT; END_TYPE
TYPE A : STRUCT b : S; END_STRUCT; END_TYPE
FUNCTION_BLOCK FB1
  VAR_OUTPUT y : DINT; END_VAR
  VAR_TEMP t : DINT; END_VAR
  y := 1;
  PROPERTY Prop : DINT
  GET
    Prop := 1;
  END_GET
  END_PROPERTY
END_FUNCTION_BLOCK
FUNCTION_BLOCK Base
  VAR_OUTPUT inherited : DINT; END_VAR
  inherited := 5;
END_FUNCTION_BLOCK
FUNCTION_BLOCK Derived EXTENDS Base
  VAR_OUTPUT own : DINT; END_VAR
END_FUNCTION_BLOCK
";

fn first_code(statements: &str) -> String {
    let source = format!(
        "{DECLARATIONS}
PROGRAM main
  VAR f : FB1; d : Derived; s : S; a : A; arr : ARRAY[1..3] OF S; x : DINT; END_VAR
  {statements}
END_PROGRAM"
    );
    let options = CompilerOptions::from_dialect(Dialect::Codesys);
    try_check_and_run(&source, &options)
        .err()
        .map_or_else(|| "none".to_owned(), |diagnostic| diagnostic.code)
}

#[rstest]
#[case::block_member_read("x := f.nothere;", "P4080")]
#[case::block_member_write("f.nothere := 1;", "P4080")]
#[case::block_temporary_read("x := f.t;", "P4080")]
#[case::structure_member_read("x := s.nothere;", "P4080")]
#[case::structure_member_write("s.nothere := 1;", "P4080")]
#[case::nested_member_read("x := a.b.nothere;", "P4080")]
#[case::array_element_member_read("x := arr[1].nothere;", "P4080")]
#[case::array_element_member_write("arr[2].nothere := 1;", "P4080")]
#[case::block_member_read_declared("f(); x := f.y;", "none")]
#[case::structure_member_read_declared("s.m := 4; x := s.m;", "none")]
#[case::property_read_declared("x := f.Prop;", "P9999")]
#[case::property_write_declared("f.Prop := 1;", "P9999")]
#[case::inherited_variable_declared("d(); x := d.inherited;", "P9999")]
fn check_when_member_is_selected_then_only_a_name_the_type_lacks_is_p4080(
    #[case] statements: &str,
    #[case] expected: &str,
) {
    assert_eq!(first_code(statements), expected, "{statements}");
}

#[test]
fn check_when_initializer_names_an_element_the_structure_lacks_then_p4080() {
    let source = "
TYPE S : STRUCT m : DINT; END_STRUCT; END_TYPE
PROGRAM main
  VAR s : S := (nothere := 1); END_VAR
END_PROGRAM";

    let refused = try_check_and_run(source, &CompilerOptions::default());

    assert_eq!(refused.err().map(|d| d.code), Some("P4080".to_owned()));
}
