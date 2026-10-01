//! The CODESYS special operators that are typed calls (`__NEW`, `__DELETE`,
//! `__TYPEOF`, `__XADD`) pass analysis but have no runtime behaviour: heap
//! allocation, type classes and atomic memory access are not implemented. Code
//! generation refuses them with P9999 at the operator, and never emits code
//! that only looks like them.

use crate::common::try_parse_and_compile;
use ironplc_parser::options::{CompilerOptions, Dialect};
use rstest::rstest;
use spec_test_macro::spec_test;

#[spec_test(REQ_CS_codegen_002)]
#[rstest]
#[case::new("obj := __NEW(INT);", "__NEW(INT)", "__NEW")]
#[case::new_with_count("obj := __NEW(INT, 4);", "__NEW(INT, 4)", "__NEW")]
#[case::delete("ok := __DELETE(obj);", "__DELETE(obj)", "__DELETE")]
#[case::typeof_type("n := __TYPEOF(INT);", "__TYPEOF(INT)", "__TYPEOF")]
#[case::xadd("r := __XADD(counter, 1);", "__XADD(counter, 1)", "__XADD")]
fn compile_when_special_operator_then_not_implemented_at_the_operator(
    #[case] statement: &str,
    #[case] anchor: &str,
    #[case] labelled: &str,
) {
    let source = format!(
        "
PROGRAM main
VAR
    obj : POINTER TO INT;
    counter : POINTER TO DINT;
    ok : BOOL;
    n : INT;
    r : DINT;
END_VAR
    {statement}
END_PROGRAM
"
    );
    let options = CompilerOptions::from_dialect(Dialect::Codesys);

    let result = try_parse_and_compile(&source, &options);

    let diagnostic = result.unwrap_err();
    assert_eq!(diagnostic.code, "P9999");
    let start = source.find(anchor).unwrap();
    assert_eq!(diagnostic.primary.location.start, start);
    assert_eq!(diagnostic.primary.location.end, start + labelled.len());
}
