//! End-to-end tests for an enumeration value written with its type,
//! `E_State#Run`, in the places an expression stands: an assignment, a
//! comparison, an argument of a call and a `CASE` label.
//!
//! Each cell checks, runs one scan and reads the numbers the program leaves, as
//! `ironplcc compile` and `ironplcvm run` do. The value is the same ordinal
//! whether it is written with its type or without, in every dialect, and
//! whether the enumeration is strict (`{attribute 'strict'}`, or the policy)
//! or not: the qualified spelling changes which enumeration a name belongs
//! to, not what the enumeration is.

use crate::common::checked::try_check_and_run;
use ironplc_parser::options::{CompilerOptions, Dialect, EnumerationStrictness};
use rstest::rstest;

/// `E_State` is Idle = 0, Run = 1, Stop = 2. The variables are `s`, `t`, `a`,
/// `b`, `c` and `d` in this order.
const PROGRAM: &str = "
TYPE E_State : (Idle, Run, Stop); END_TYPE
FUNCTION pick : DINT
  VAR_INPUT v : E_State; END_VAR
  IF v = E_State#Stop THEN pick := 30; ELSE pick := 40; END_IF;
END_FUNCTION
PROGRAM main
  VAR s : E_State; t : E_State; a : DINT; b : DINT; c : DINT; d : DINT; END_VAR
  s := E_State#Run;
  t := E_State#Stop;
  IF s = E_State#Run THEN a := 1; END_IF;
  IF s = E_State#Stop THEN b := 5; ELSE b := 2; END_IF;
  c := pick(E_State#Stop);
  CASE s OF
    E_State#Idle: d := 10;
    E_State#Run: d := 20;
    E_State#Stop: d := 30;
  END_CASE;
END_PROGRAM
";

fn every_enumeration_strict(dialect: Dialect) -> CompilerOptions {
    CompilerOptions {
        policy_enumeration_strictness: EnumerationStrictness::EveryEnumeration,
        ..CompilerOptions::from_dialect(dialect)
    }
}

#[rstest]
#[case::iec_edition_2(CompilerOptions::from_dialect(Dialect::Iec61131_3Ed2), false)]
#[case::iec_edition_3(CompilerOptions::from_dialect(Dialect::Iec61131_3Ed3), false)]
#[case::codesys_with_the_attribute(CompilerOptions::from_dialect(Dialect::Codesys), true)]
#[case::codesys_without_the_attribute(CompilerOptions::from_dialect(Dialect::Codesys), false)]
#[case::twincat_with_the_attribute(CompilerOptions::from_dialect(Dialect::TwinCat), true)]
#[case::twincat_without_the_attribute(CompilerOptions::from_dialect(Dialect::TwinCat), false)]
#[case::codesys_every_enumeration_strict(every_enumeration_strict(Dialect::Codesys), false)]
fn end_to_end_when_qualified_value_in_every_expression_position_then_ordinal_of_the_value_in_every_dialect(
    #[case] options: CompilerOptions,
    #[case] attribute: bool,
) {
    let source = if attribute {
        PROGRAM.replace("TYPE E_State", "{attribute 'strict'} TYPE E_State")
    } else {
        PROGRAM.to_owned()
    };

    let (_container, bufs) = try_check_and_run(&source, &options).expect("the program runs");

    let numbers: Vec<i32> = (0..6).map(|slot| bufs.vars[slot].as_i32()).collect();
    assert_eq!(numbers, vec![1, 2, 1, 2, 30, 20]);
}

/// E1 declares U1 as ordinal 1 and E2 declares U1 as ordinal 0: the type before
/// the `#` says which, and a bare `U1` is refused (P2042) before it runs.
const SHARED: &str = "
TYPE E1 : (U2, U1); E2 : (U1, U3); END_TYPE
PROGRAM main
  VAR x : E1; y : E2; a : DINT; b : DINT; END_VAR
  x := E1#U1;
  y := E2#U1;
  IF x = E1#U1 THEN a := 1; END_IF;
  IF y = E2#U1 THEN b := 2; END_IF;
END_PROGRAM
";

#[rstest]
#[case::iec_edition_2(Dialect::Iec61131_3Ed2)]
#[case::codesys(Dialect::Codesys)]
fn end_to_end_when_value_name_is_declared_by_two_enumerations_then_the_qualified_value_is_the_ordinal_of_its_own(
    #[case] dialect: Dialect,
) {
    let options = CompilerOptions::from_dialect(dialect);

    let (_container, bufs) = try_check_and_run(SHARED, &options).expect("the program runs");

    let numbers: Vec<i32> = (0..4).map(|slot| bufs.vars[slot].as_i32()).collect();
    assert_eq!(numbers, vec![1, 0, 1, 2]);
}

#[test]
fn end_to_end_when_bare_value_name_is_declared_by_two_enumerations_then_refused_and_the_qualified_one_is_the_remedy(
) {
    let bare = SHARED.replace("x := E1#U1;", "x := U1;");

    let refused = try_check_and_run(&bare, &CompilerOptions::default());

    assert_eq!(
        refused.expect_err("a bare shared name is refused").code,
        "P2042"
    );
}

/// A name that is no value of the expected enumeration is refused by the check
/// of the program, and does not reach code generation, which has no ordinal for
/// it (it was P9999 "Unknown enum value" there). Each row is the first
/// diagnostic of the program, written with `E1 : (P, R)` and `E2 : (P, Q)`.
#[rstest]
#[case::assigned_name_of_no_enumeration("", "s := Nope;", "P4007")]
#[case::case_label_name_of_no_enumeration("", "CASE s OF Nope: b := TRUE; END_CASE;", "P4007")]
#[case::compared_name_of_no_enumeration("", "b := s = Nope;", "P4007")]
#[case::assigned_value_of_another_enumeration("", "s := Q;", "P2006")]
#[case::case_label_value_of_another_enumeration("", "CASE s OF Q: b := TRUE; END_CASE;", "P2006")]
#[case::initial_qualified_value_of_another_enumeration("t : E1 := E2#Q;", "", "P4035")]
#[case::assigned_qualified_value_of_another_enumeration("", "s := E2#Q;", "P4035")]
#[case::case_label_qualified_value_of_another_enumeration(
    "",
    "CASE s OF E2#Q: b := TRUE; END_CASE;",
    "P4035"
)]
#[case::compared_qualified_value_of_another_enumeration("", "b := s = E2#Q;", "P4035")]
fn end_to_end_when_value_is_not_of_the_expected_enumeration_then_the_check_refuses_it(
    #[case] declarations: &str,
    #[case] statements: &str,
    #[case] code: &str,
) {
    let source = [
        "TYPE E1 : (P, R); E2 : (P, Q); END_TYPE PROGRAM main VAR s : E1; b : BOOL;",
        declarations,
        "END_VAR",
        statements,
        "END_PROGRAM",
    ]
    .join(" ");

    let refused = try_check_and_run(&source, &CompilerOptions::default());

    assert_eq!(
        refused.expect_err("the program is refused").code,
        code,
        "{source}"
    );
}
