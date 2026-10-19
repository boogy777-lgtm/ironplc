use crate::stages::analyze;
use ironplc_dsl::core::FileId;
use ironplc_parser::{options::CompilerOptions, parse_program};
use rstest::rstest;

mod positions;

#[test]
fn apply_when_two_undefined_enum_values_then_reports_both() {
    let program = "
TYPE
LEVEL : (INFO, WARN) := INFO;
END_TYPE

FUNCTION_BLOCK LOGGER
VAR_INPUT
A : LEVEL := CRITICAL;
B : LEVEL := FATAL;
END_VAR
END_FUNCTION_BLOCK";

    let library = parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
    let (_library, context) = analyze(&[&library], &CompilerOptions::default()).unwrap();

    let reported: Vec<&String> = context
        .diagnostics()
        .iter()
        .flat_map(|d| &d.described)
        .collect();
    assert!(
        reported.iter().any(|d| d.as_str() == "value=CRITICAL"),
        "expected CRITICAL, got {reported:?}"
    );
    assert!(
        reported.iter().any(|d| d.as_str() == "value=FATAL"),
        "expected FATAL, got {reported:?}"
    );
}

#[test]
fn apply_when_multiple_enum_values_with_one_undefined_then_error() {
    let program = "
TYPE
LEVEL : (INFO, WARN) := INFO;
END_TYPE

FUNCTION_BLOCK LOGGER
VAR_INPUT
A : LEVEL := INFO;
B : LEVEL := CRITICAL;
END_VAR
END_FUNCTION_BLOCK";

    let library = parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
    let result = analyze(&[&library], &CompilerOptions::default());

    let (_library, context) = result.unwrap();
    assert!(context.has_diagnostics());
}

#[test]
fn apply_when_var_init_undefined_enum_value_then_error() {
    let program = "
TYPE
LEVEL : (INFO) := INFO;
END_TYPE
    
FUNCTION_BLOCK LOGGER
VAR_INPUT
LEVEL : LEVEL := CRITICAL;
END_VAR
END_FUNCTION_BLOCK";

    let library = parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
    let result = analyze(&[&library], &CompilerOptions::default());

    let (_library, context) = result.unwrap();
    assert!(context.has_diagnostics());
}

#[test]
fn apply_when_var_init_valid_enum_value_then_ok() {
    let program = "
TYPE
LEVEL : (CRITICAL) := CRITICAL;
END_TYPE

FUNCTION_BLOCK LOGGER
VAR_INPUT
LEVEL : LEVEL := CRITICAL;
END_VAR
END_FUNCTION_BLOCK";

    let library = parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
    let result = analyze(&[&library], &CompilerOptions::default());

    assert!(result.is_ok());
}

#[test]
fn apply_when_var_init_valid_enum_value_through_alias_then_ok() {
    let program = "
TYPE
LEVEL : (CRITICAL) := CRITICAL;
LEVEL_ALIAS : LEVEL;
END_TYPE

FUNCTION_BLOCK LOGGER
VAR_INPUT
NAME : LEVEL_ALIAS := CRITICAL;
END_VAR

END_FUNCTION_BLOCK";

    let library = parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
    let result = analyze(&[&library], &CompilerOptions::default());

    assert!(result.is_ok());
}

/// How many times a program is analyzed to see the result of every
/// analysis. Each analysis makes containers of its own, so a result that
/// depends on the order of a hash container varies within one process.
const RUNS: usize = 24;

/// The distinct results of analyzing `program` `RUNS` times, each result
/// the sorted codes of the diagnostics reported.
fn distinct_results(program: &str) -> Vec<Vec<String>> {
    distinct_results_with(program, &CompilerOptions::default())
}

/// [`distinct_results`] under `options`.
pub(super) fn distinct_results_with(program: &str, options: &CompilerOptions) -> Vec<Vec<String>> {
    let library = parse_program(program, &FileId::default(), options).unwrap();
    let mut results: Vec<Vec<String>> = (0..RUNS)
        .map(|_| {
            let (_library, context) = analyze(&[&library], options).unwrap();
            let mut codes: Vec<String> = context
                .diagnostics()
                .iter()
                .map(|d| d.code.clone())
                .collect();
            codes.sort();
            codes
        })
        .collect();
    results.sort();
    results.dedup();
    results
}

/// A program with the two variables `declarations` declare, against the
/// enumerations and aliases `types` declare.
fn program_of(types: &str, declarations: &str) -> String {
    format!("TYPE {types} END_TYPE PROGRAM main VAR {declarations} END_VAR END_PROGRAM")
}

const NONE: &[&str] = &[];
const P2006: &[&str] = &["P2006"];
/// A bare value name that two enumerations declare is refused where it is
/// used (`rule_ambiguous_enumerated_value`), whatever its type expects.
const P2042: &[&str] = &["P2042"];

#[rstest]
// Enumerations that share no value name.
#[case::own_value("E1 : (A, B); E2 : (C, D);", "v : E1 := A;", NONE)]
#[case::other_enumerations_value("E1 : (A, B); E2 : (C, D);", "v : E1 := C;", P2006)]
#[case::own_value_of_second("E1 : (A, B); E2 : (C, D);", "v : E2 := C;", NONE)]
// Enumerations that share a value name: the name is a value of each.
#[case::shared_value_of_first("E1 : (U1, U2); E2 : (U1, U3);", "v : E1 := U1;", P2042)]
#[case::shared_value_of_second("E1 : (U1, U2); E2 : (U1, U3);", "w : E2 := U1;", P2042)]
#[case::shared_value_not_of_second("E1 : (U1, U2); E2 : (U1, U3);", "w : E2 := U2;", P2006)]
#[case::shared_value_not_of_first("E1 : (U1, U2); E2 : (U1, U3);", "v : E1 := U3;", P2006)]
// An alias has the values of the enumeration its declaration names.
#[case::alias_own_value("E1 : (A, B); E2 : (C, D); EA : E1;", "v : EA := A;", NONE)]
#[case::alias_other_enumerations_value("E1 : (A, B); E2 : (C, D); EA : E1;", "v : EA := C;", P2006)]
#[case::alias_of_second_enumeration("E1 : (A, B); E2 : (C, D); EA : E2;", "v : EA := C;", NONE)]
#[case::alias_of_second_not_first_value(
    "E1 : (A, B); E2 : (C, D); EA : E2;",
    "v : EA := A;",
    P2006
)]
#[case::alias_of_alias_own_value(
    "E1 : (A, B); E2 : (C, D); EA : E1; EB : EA;",
    "v : EB := B;",
    NONE
)]
#[case::alias_of_alias_other_value(
    "E1 : (A, B); E2 : (C, D); EA : E1; EB : EA;",
    "v : EB := D;",
    P2006
)]
#[case::alias_with_shared_value_name(
    "E1 : (U1, U2); E2 : (U1, U3); EA : E2;",
    "v : EA := U1;",
    P2042
)]
#[case::alias_with_shared_value_name_not_of_base(
    "E1 : (U1, U2); E2 : (U1, U3); EA : E2;",
    "v : EA := U2;",
    P2006
)]
#[case::aliases_of_two_enumerations(
    "E1 : (U1, U2); E2 : (U1, U3); EA : E1; EB : E2;",
    "v : EA := U2; w : EB := U3;",
    NONE
)]
fn apply_when_enumerations_declared_then_value_is_checked_against_its_type_in_every_run(
    #[case] types: &str,
    #[case] declarations: &str,
    #[case] expected: &[&str],
) {
    let expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();

    let results = distinct_results(&program_of(types, declarations));

    assert_eq!(results, vec![expected]);
}

/// A value name that two enumerations declare is refused where it is used
/// (`rule_ambiguous_enumerated_value`), whatever the place expects. A name
/// that one enumeration declares is a value of the enumeration the place
/// expects or it is not (`y := U2` below). The report is the same in every
/// run.
#[rstest]
#[case::compared_with_first_enumeration("IF x = U1 THEN r := TRUE; END_IF;", P2042)]
#[case::compared_with_second_enumeration("IF y = U1 THEN r := TRUE; END_IF;", P2042)]
#[case::assigned_to_each_enumeration("x := U1; y := U1;", &["P2042", "P2042"])]
#[case::case_labels_of_the_selectors_enumeration(
    "CASE x OF U1: r := TRUE; U2: r := FALSE; END_CASE;",
    P2042
)]
#[case::value_of_the_other_enumeration_assigned("y := U2;", P2006)]
fn apply_when_value_name_in_expression_then_a_shared_name_or_a_value_of_another_enumeration_is_reported_in_every_run(
    #[case] body: &str,
    #[case] expected: &[&str],
) {
    let program = format!(
        "TYPE E1 : (U1, U2); E2 : (U1, U3); END_TYPE              PROGRAM main VAR x : E1; y : E2; r : BOOL; END_VAR {body} END_PROGRAM"
    );
    let expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();

    assert_eq!(distinct_results(&program), vec![expected]);
}

const P2004: &[&str] = &["P2004"];
const P2008: &[&str] = &["P2008"];

/// The enumerations a value written with its type is checked against:
/// `E_State`, two that declare the same value name, and a type that is
/// no enumeration.
const QUALIFIED_TYPES: &str = "E_State : (Idle, Run, Stop); E1 : (A, Bx); E2 : (A, C); R : INT;         S : STRUCT m : E_State := E_State#Run; END_STRUCT;";

/// A program of the variables used by the cases below, and a function that
/// takes an enumeration.
fn qualified_program(body: &str) -> String {
    format!(
        "TYPE {QUALIFIED_TYPES} END_TYPE              FUNCTION is_run : BOOL VAR_INPUT v : E_State; END_VAR is_run := v = E_State#Run; END_FUNCTION              PROGRAM main VAR s : E_State; b : BOOL; x : E1; y : E2; END_VAR {body} END_PROGRAM"
    )
}

#[rstest]
// Written with a declared enumeration that declares the value.
#[case::assigned("s := E_State#Run;", NONE)]
#[case::compared("b := s = E_State#Stop;", NONE)]
#[case::compared_on_the_left("b := E_State#Idle <> s;", NONE)]
#[case::argument("b := is_run(E_State#Run);", NONE)]
#[case::named_argument("b := is_run(v := E_State#Run);", NONE)]
#[case::case_label("CASE s OF E_State#Run: b := TRUE; Idle: b := FALSE; END_CASE;", NONE)]
#[case::value_name_of_two_enumerations_in_the_first("x := E1#A;", NONE)]
#[case::value_name_of_two_enumerations_in_the_second("y := E2#A;", NONE)]
#[case::value_of_the_second_enumeration_only("y := E2#C;", NONE)]
// The type before the # is not declared.
#[case::undeclared_assigned("s := E_Nope#Run;", P2008)]
#[case::undeclared_compared("b := s = E_Nope#Run;", P2008)]
#[case::undeclared_argument("b := is_run(E_Nope#Run);", P2008)]
#[case::undeclared_case_label("CASE s OF E_Nope#Run: b := TRUE; END_CASE;", P2008)]
// The type before the # is not an enumeration.
#[case::not_an_enumeration_assigned("s := R#Run;", P2004)]
#[case::not_an_enumeration_case_label("CASE s OF R#Run: b := TRUE; END_CASE;", P2004)]
#[case::structure_is_not_an_enumeration("s := S#Run;", P2004)]
// The enumeration does not declare the value.
#[case::not_a_value_assigned("s := E_State#Nope;", P2006)]
#[case::not_a_value_compared("b := s = E_State#Nope;", P2006)]
#[case::not_a_value_argument("b := is_run(E_State#Nope);", P2006)]
#[case::not_a_value_case_label("CASE s OF E_State#Nope: b := TRUE; END_CASE;", P2006)]
#[case::value_of_another_enumeration("x := E1#C;", P2006)]
#[case::each_fault_once("s := E_Nope#Run; s := E_State#Nope;", &["P2006", "P2008"])]
fn apply_when_value_written_with_its_type_then_the_type_is_declared_an_enumeration_of_the_value(
    #[case] body: &str,
    #[case] expected: &[&str],
) {
    let expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();

    let results = distinct_results(&qualified_program(body));

    assert_eq!(results, vec![expected]);
}

const P4007: &[&str] = &["P4007"];
const P4026: &[&str] = &["P4026"];
const P4035: &[&str] = &["P4035"];
/// The error of the value and the warning of the comparison of two enumerations.
const P4035_P4083: &[&str] = &["P4035", "P4083"];

/// A name that no enumeration declares is no value: it is a name that is not
/// declared (P4007), as it is in an assignment to an integer. A name that
/// another enumeration declares is not a value of the expected one (P2006),
/// as it is in an initial value. A value written with the type of another
/// enumeration is a mismatch of types (P4035, or P4026 for an argument).
/// Every place that expects an enumeration reports each of them, so none
/// reaches code generation, which has no ordinal for a name that is not
/// declared.
#[rstest]
// An assignment.
#[case::assigned_name_of_no_enumeration("s := Nope;", P4007)]
#[case::assigned_value_of_another_enumeration("x := C; y := Bx;", &["P2006", "P2006"])]
#[case::assigned_own_value("x := Bx; y := C; s := Run;", NONE)]
#[case::assigned_qualified_value_of_another_enumeration("x := E2#C;", P4035)]
#[case::assigned_to_an_integer_name_of_no_enumeration("b := Nope;", P4007)]
// A comparison.
#[case::compared_name_of_no_enumeration("b := x = Nope;", P4007)]
#[case::compared_on_the_left_name_of_no_enumeration("b := Nope = x;", P4007)]
#[case::compared_value_of_another_enumeration("b := x = C;", P2006)]
#[case::compared_on_the_left_value_of_another_enumeration("b := C <> x;", P2006)]
#[case::compared_with_own_value("b := x = Bx; b := Bx = x;", NONE)]
#[case::compared_with_qualified_value_of_another_enumeration("b := x = E2#C;", P4035_P4083)]
#[case::compared_with_own_qualified_value("b := x = E1#Bx;", NONE)]
// An argument.
#[case::argument_name_of_no_enumeration("b := is_run(Nope);", P4007)]
#[case::argument_value_of_another_enumeration("b := is_run(Bx);", P2006)]
#[case::argument_own_value("b := is_run(Run);", NONE)]
#[case::argument_qualified_value_of_another_enumeration("b := is_run(E1#Bx);", P4026)]
// A label of a CASE.
#[case::label_name_of_no_enumeration("CASE s OF Nope: b := TRUE; END_CASE;", P4007)]
#[case::label_value_of_another_enumeration("CASE x OF C: b := TRUE; END_CASE;", P2006)]
#[case::label_own_values("CASE x OF Bx, E1#A: b := TRUE; END_CASE;", NONE)]
#[case::label_qualified_value_of_another_enumeration("CASE x OF E2#C: b := TRUE; END_CASE;", P4035)]
#[case::label_qualified_value_of_another_enumeration_with_a_name_of_both(
    "CASE x OF E2#A: b := TRUE; END_CASE;",
    P4035
)]
#[case::label_own_qualified_value("CASE x OF E1#Bx: b := TRUE; END_CASE;", NONE)]
#[case::label_each_fault_once(
    "CASE x OF Nope: b := TRUE; C: b := FALSE; E2#C: b := TRUE; END_CASE;",
    &["P2006", "P4007", "P4035"]
)]
fn apply_when_value_is_written_where_an_enumeration_is_expected_then_it_is_a_value_of_it(
    #[case] body: &str,
    #[case] expected: &[&str],
) {
    let expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();

    let results = distinct_results(&qualified_program(body));

    assert_eq!(results, vec![expected]);
}

#[rstest]
#[case::initial_value_name_of_no_enumeration("v : E1 := Nope;", P2006)]
#[case::initial_value_of_another_enumeration("v : E1 := C;", P2006)]
#[case::initial_qualified_value_of_another_enumeration("v : E1 := E2#C;", P4035)]
#[case::initial_qualified_value_of_another_enumeration_with_a_name_of_both(
    "v : E1 := E2#A;",
    P4035
)]
#[case::initial_own_qualified_value("v : E1 := E1#A;", NONE)]
#[case::initial_qualified_value_through_an_alias("v : EA := E1#A;", NONE)]
fn apply_when_initial_value_is_written_with_another_enumeration_then_reported_once(
    #[case] declarations: &str,
    #[case] expected: &[&str],
) {
    let expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();

    let results = distinct_results(&program_of(
        &format!("{QUALIFIED_TYPES} EA : E1;"),
        declarations,
    ));

    assert_eq!(results, vec![expected]);
}

#[test]
fn apply_when_value_of_an_enumeration_spelled_out_in_a_declaration_then_a_value() {
    let program = "PROGRAM main VAR c : (Red, Green); END_VAR c := Green; END_PROGRAM";

    assert_eq!(distinct_results(program), vec![Vec::<String>::new()]);
}

#[rstest]
// The initial value of a variable, of an element of an array, and the default
// of a member of a structure.
#[case::initial_value("v : E_State := E_State#Run;", NONE)]
#[case::initial_value_shared_name("v : E1 := E1#A; w : E2 := E2#A;", NONE)]
#[case::initial_value_undeclared("v : E_State := E_Nope#Run;", P2008)]
#[case::initial_value_not_an_enumeration("v : E_State := R#Run;", P2004)]
#[case::initial_value_not_a_value("v : E_State := E_State#Nope;", P2006)]
#[case::array_element("v : ARRAY[1..2] OF E_State := [E_State#Run, E_State#Stop];", NONE)]
#[case::array_element_undeclared(
    "v : ARRAY[1..2] OF E_State := [E_State#Run, E_Nope#Stop];",
    P2008
)]
#[case::array_element_not_a_value(
    "v : ARRAY[1..2] OF E_State := [E_State#Run, E_State#Nope];",
    P2006
)]
#[case::structure_member("v : S := (m := E_State#Stop);", NONE)]
#[case::structure_member_undeclared("v : S := (m := E_Nope#Stop);", P2008)]
#[case::structure_member_not_a_value("v : S := (m := E_State#Nope);", P2006)]
fn apply_when_initial_value_written_with_its_type_then_checked_like_one_in_a_statement(
    #[case] declarations: &str,
    #[case] expected: &[&str],
) {
    let expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();

    let results = distinct_results(&program_of(QUALIFIED_TYPES, declarations));

    assert_eq!(results, vec![expected]);
}

rule_ctx_ok!(
    apply_when_value_is_written_with_a_type_declared_with_an_error_then_ok,
    "
TYPE E : E_NOWHERE := A1; END_TYPE
PROGRAM main
VAR v : E := E#A1; w : E; END_VAR
w := E#A1;
END_PROGRAM"
);

rule_ctx_ok!(
    apply_when_enumeration_is_declared_with_an_error_then_ok,
    "
TYPE E : E_NOWHERE := A1; END_TYPE
PROGRAM main
VAR v : E; END_VAR
END_PROGRAM"
);
