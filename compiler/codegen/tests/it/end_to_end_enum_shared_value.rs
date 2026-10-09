//! End-to-end tests for enumerations that declare the same value name.
//!
//! A bare value name that two enumerations declare is refused by the analyzer
//! (P2042), so code generation never has to choose between them. These tests
//! build the library as the analysis leaves it, without the semantic rules,
//! to hold code generation to that: it compiles a qualified value to the
//! ordinal of its own enumeration, and it does not pick an owner for a bare
//! name that has several.

use crate::common::try_parse_and_compile;
use ironplc_parser::options::CompilerOptions;

// E1 declares U1 as ordinal 1 and E2 declares U1 as ordinal 0.
const TYPES: &str = "TYPE E1 : (U2, U1); E2 : (U1, U3); END_TYPE";

// Each variable is a slot, in the order declared.
e2e_i32!(
    end_to_end_when_qualified_value_is_shared_then_ordinal_of_its_own_enumeration,
    "TYPE E1 : (U2, U1); E2 : (U1, U3); END_TYPE
     PROGRAM main VAR v : E1 := E1#U1; w : E2 := E2#U1; END_VAR END_PROGRAM",
    &[(0, 1), (1, 0)],
);

e2e_i32!(
    end_to_end_when_qualified_value_names_an_alias_then_ordinal_of_the_aliased_enumeration,
    "TYPE E1 : (U2, U1); E2 : (U1, U3); EA : E2; END_TYPE
     PROGRAM main VAR z : EA := EA#U1; y : EA := EA#U3; END_VAR END_PROGRAM",
    &[(0, 0), (1, 1)],
);

e2e_i32!(
    end_to_end_when_qualified_value_in_a_structure_member_then_ordinal_of_its_own_enumeration,
    "TYPE E1 : (U2, U1); E2 : (U1, U3); S : STRUCT f : E2 := E2#U1; g : E1 := E1#U1; END_STRUCT; END_TYPE
     PROGRAM main VAR s : S; a : DINT; b : DINT; END_VAR a := s.f; b := s.g; END_PROGRAM",
    &[(1, 0), (2, 1)],
);

e2e_i32!(
    end_to_end_when_names_are_declared_once_then_bare_names_resolve_as_before,
    "TYPE E1 : (U2, U1); E2 : (U1, U3); END_TYPE
     PROGRAM main VAR v : E1 := U2; w : E2 := U3; END_VAR
       IF v = U2 THEN w := U3; END_IF;
     END_PROGRAM",
    &[(0, 0), (1, 1)],
);

e2e_i32!(
    end_to_end_when_alias_of_an_enumeration_then_bare_value_is_not_ambiguous,
    "TYPE E1 : (A, B); EA : E1; END_TYPE
     PROGRAM main VAR x : EA := B; y : E1 := A; END_VAR END_PROGRAM",
    &[(0, 1), (1, 0)],
);

#[test]
fn end_to_end_when_bare_value_has_several_enumerations_then_internal_error_not_a_choice() {
    let source = format!("{TYPES} PROGRAM main VAR w : E2 := U1; END_VAR END_PROGRAM");

    let result = try_parse_and_compile(&source, &CompilerOptions::default());

    let diagnostic = result.err().expect("an ambiguous value does not compile");
    assert_eq!(diagnostic.code, "P9998");
}

#[test]
fn end_to_end_when_bare_value_has_several_enumerations_in_a_statement_then_internal_error() {
    let source = format!("{TYPES} PROGRAM main VAR y : E2; END_VAR y := U1; END_PROGRAM");

    let result = try_parse_and_compile(&source, &CompilerOptions::default());

    let diagnostic = result.err().expect("an ambiguous value does not compile");
    assert_eq!(diagnostic.code, "P9998");
}
