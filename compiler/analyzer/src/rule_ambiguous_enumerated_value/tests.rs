//! A bare value name that more than one enumeration declares is refused where
//! it is used, in every place a value can stand.

use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use ironplc_parser::parse_program;
use rstest::rstest;

use crate::stages::analyze;

/// Two enumerations that share `U1`; `U2` and `U3` are declared once.
const TYPES: &str = "TYPE E1 : (U2, U1); E2 : (U1, U3); END_TYPE";

const AMBIGUOUS: &str = "P2042";

/// One diagnostic of the analysis: its code, the text its primary label points
/// at, and the text each secondary label points at with the label's message.
#[derive(Debug, PartialEq)]
struct Found {
    code: String,
    at: String,
    declared: Vec<(String, String)>,
}

fn analyzed(source: &str, options: &CompilerOptions) -> Vec<Found> {
    let library = parse_program(source, &FileId::default(), options).unwrap();
    let (_library, context) = analyze(&[&library], options).unwrap();
    let text = |label: &ironplc_dsl::diagnostic::Label| {
        source[label.location.start..label.location.end].to_string()
    };
    context
        .diagnostics()
        .iter()
        .map(|d| Found {
            code: d.code.clone(),
            at: text(&d.primary),
            declared: d
                .secondary
                .iter()
                .map(|label| (text(label), label.message.clone()))
                .collect(),
        })
        .collect()
}

/// The codes the analysis reports for `source`, in the order reported.
fn codes(source: &str) -> Vec<String> {
    analyzed(source, &CompilerOptions::default())
        .into_iter()
        .map(|found| found.code)
        .collect()
}

/// The diagnostics of the ambiguous value rule only: the programs of a table
/// row may be refused by other rules, which the row is not about.
fn ambiguous(source: &str, options: &CompilerOptions) -> Vec<Found> {
    analyzed(source, options)
        .into_iter()
        .filter(|found| found.code == AMBIGUOUS)
        .collect()
}

fn default_options() -> CompilerOptions {
    CompilerOptions::default()
}

fn with_methods() -> CompilerOptions {
    CompilerOptions {
        allow_fb_inheritance: true,
        ..CompilerOptions::default()
    }
}

/// Every place the language lets an enumeration value stand names the value
/// `U1` of `E2`, which `E1` declares as well: one diagnostic, at the name.
#[rstest]
// Initial value of a declaration with a type of its own.
#[case::initial_value_of_a_program_variable(
    default_options(),
    "PROGRAM main VAR w : E2 := U1; END_VAR END_PROGRAM"
)]
#[case::initial_value_of_a_function_block_input(
    default_options(),
    "FUNCTION_BLOCK FB VAR_INPUT w : E2 := U1; END_VAR END_FUNCTION_BLOCK
     PROGRAM main END_PROGRAM"
)]
#[case::initial_value_of_a_function_variable(
    default_options(),
    "FUNCTION F : BOOL VAR w : E2 := U1; END_VAR F := TRUE; END_FUNCTION
     PROGRAM main END_PROGRAM"
)]
#[case::initial_value_of_a_constant(
    default_options(),
    "PROGRAM main VAR CONSTANT w : E2 := U1; END_VAR END_PROGRAM"
)]
#[case::initial_value_of_a_global_variable(
    default_options(),
    "CONFIGURATION c VAR_GLOBAL g : E2 := U1; END_VAR
       RESOURCE r ON PLC
         TASK t(INTERVAL := T#10ms, PRIORITY := 1);
         PROGRAM pi WITH t : main;
       END_RESOURCE
     END_CONFIGURATION
     PROGRAM main END_PROGRAM"
)]
#[case::initial_value_of_a_resource_global_variable(
    default_options(),
    "CONFIGURATION c RESOURCE r ON PLC VAR_GLOBAL g : E2 := U1; END_VAR
         TASK t(INTERVAL := T#10ms, PRIORITY := 1);
         PROGRAM pi WITH t : main;
       END_RESOURCE
     END_CONFIGURATION
     PROGRAM main END_PROGRAM"
)]
// Initial value of a structure member or of a function block input.
#[case::initial_value_of_a_structure_member_declaration(
    default_options(),
    "TYPE S : STRUCT f : E2 := U1; END_STRUCT; END_TYPE
     PROGRAM main VAR s : S; END_VAR END_PROGRAM"
)]
#[case::member_of_a_structure_initializer(
    default_options(),
    "TYPE S : STRUCT f : E2; END_STRUCT; END_TYPE
     PROGRAM main VAR s : S := (f := U1); END_VAR END_PROGRAM"
)]
#[case::member_of_a_structure_type_default(
    default_options(),
    "TYPE S : STRUCT f : E2; END_STRUCT; END_TYPE
     TYPE T : S := (f := U1); END_TYPE
     PROGRAM main VAR s : T; END_VAR END_PROGRAM"
)]
#[case::input_of_a_function_block_instance(
    default_options(),
    "FUNCTION_BLOCK FB VAR_INPUT i : E2; END_VAR END_FUNCTION_BLOCK
     PROGRAM main VAR f : FB := (i := U1); END_VAR END_PROGRAM"
)]
// Initial value of an array element.
#[case::element_of_an_array_initializer(
    default_options(),
    "PROGRAM main VAR arr : ARRAY[1..2] OF E2 := [U1, U3]; END_VAR END_PROGRAM"
)]
#[case::repeated_element_of_an_array_initializer(
    default_options(),
    "PROGRAM main VAR arr : ARRAY[1..2] OF E2 := [2(U1)]; END_VAR END_PROGRAM"
)]
#[case::element_of_an_array_type_default(
    default_options(),
    "TYPE A : ARRAY[1..2] OF E2 := [U1, U3]; END_TYPE
     PROGRAM main END_PROGRAM"
)]
// A statement.
#[case::assignment(
    default_options(),
    "PROGRAM main VAR y : E2; END_VAR y := U1; END_PROGRAM"
)]
#[case::comparison_in_a_condition(
    default_options(),
    "PROGRAM main VAR y : E2; r : BOOL; END_VAR
     IF y = U1 THEN r := TRUE; END_IF; END_PROGRAM"
)]
#[case::comparison_with_the_value_first(
    default_options(),
    "PROGRAM main VAR y : E2; r : BOOL; END_VAR r := (U1 = y) AND TRUE; END_PROGRAM"
)]
#[case::comparison_in_a_loop_condition(
    default_options(),
    "PROGRAM main VAR y : E2; END_VAR WHILE y <> U1 DO y := U3; END_WHILE; END_PROGRAM"
)]
#[case::case_label(
    default_options(),
    "PROGRAM main VAR y : E2; END_VAR CASE y OF U1: y := U3; END_CASE; END_PROGRAM"
)]
#[case::value_returned_from_a_function(
    default_options(),
    "FUNCTION F : E2 F := U1; END_FUNCTION PROGRAM main END_PROGRAM"
)]
// An argument of a call.
#[case::positional_argument_of_a_function(
    default_options(),
    "FUNCTION F : BOOL VAR_INPUT i : E2; END_VAR F := TRUE; END_FUNCTION
     PROGRAM main VAR r : BOOL; END_VAR r := F(U1); END_PROGRAM"
)]
#[case::named_argument_of_a_function(
    default_options(),
    "FUNCTION F : BOOL VAR_INPUT i : E2; END_VAR F := TRUE; END_FUNCTION
     PROGRAM main VAR r : BOOL; END_VAR r := F(i := U1); END_PROGRAM"
)]
#[case::argument_of_a_function_block_call(
    default_options(),
    "FUNCTION_BLOCK FB VAR_INPUT i : E2; END_VAR END_FUNCTION_BLOCK
     PROGRAM main VAR f : FB; END_VAR f(i := U1); END_PROGRAM"
)]
#[case::argument_of_a_method_call(
    with_methods(),
    "FUNCTION_BLOCK FB
       METHOD M : BOOL VAR_INPUT i : E2; END_VAR M := TRUE; END_METHOD
     END_FUNCTION_BLOCK
     PROGRAM main VAR f : FB; r : BOOL; END_VAR r := f.M(i := U1); END_PROGRAM"
)]
// The default of a type declaration that names another enumeration.
#[case::default_of_an_enumeration_alias(
    default_options(),
    "TYPE E3 : E2 := U1; END_TYPE PROGRAM main END_PROGRAM"
)]
fn apply_when_shared_value_name_used_then_one_ambiguous_value_at_the_name(
    #[case] options: CompilerOptions,
    #[case] body: &str,
) {
    let found = ambiguous(&format!("{TYPES} {body}"), &options);

    assert_eq!(found.len(), 1, "{found:?}");
    assert_eq!(found[0].at, "U1");
}

#[test]
fn apply_when_shared_value_name_used_then_each_declaring_enumeration_is_named() {
    let source = format!("{TYPES} PROGRAM main VAR y : E2; END_VAR y := U1; END_PROGRAM");

    let found = ambiguous(&source, &default_options());

    // The labels point at the value where each enumeration declares it.
    assert_eq!(
        found[0].declared,
        vec![
            ("U1".to_string(), "Value of enumeration E1".to_string()),
            ("U1".to_string(), "Value of enumeration E2".to_string()),
        ]
    );
}

#[test]
fn apply_when_three_enumerations_share_a_value_name_then_all_three_are_named() {
    let source = "TYPE E1 : (U1, U2); E2 : (U3, U1); E3 : (U1, U4); END_TYPE
         PROGRAM main VAR y : E3; END_VAR y := U1; END_PROGRAM";

    let found = ambiguous(source, &default_options());

    let named: Vec<&str> = found[0]
        .declared
        .iter()
        .map(|(_, message)| message.as_str())
        .collect();
    assert_eq!(
        named,
        [
            "Value of enumeration E1",
            "Value of enumeration E2",
            "Value of enumeration E3"
        ]
    );
}

#[test]
fn apply_when_shared_value_name_used_twice_then_each_use_is_reported() {
    let source =
        format!("{TYPES} PROGRAM main VAR x : E1; y : E2; END_VAR x := U1; y := U1; END_PROGRAM");

    let found = ambiguous(&source, &default_options());

    assert_eq!(found.len(), 2);
}

#[test]
fn apply_when_shared_value_name_is_declared_and_not_used_then_no_diagnostic() {
    // U2 and U3 are declared once and are used bare, as before.
    let source = format!(
        "{TYPES} PROGRAM main VAR x : E1 := U2; y : E2 := U3; END_VAR
         x := U2; y := U3; IF x = U2 THEN y := U3; END_IF; END_PROGRAM"
    );

    assert_eq!(codes(&source), Vec::<String>::new());
}

#[test]
fn apply_when_value_name_is_declared_by_one_enumeration_then_no_diagnostic() {
    let source = "TYPE E1 : (A, B); E2 : (C, D); END_TYPE
         PROGRAM main VAR x : E1 := A; y : E2; END_VAR y := D; x := B; END_PROGRAM";

    assert_eq!(codes(source), Vec::<String>::new());
}

#[test]
fn apply_when_alias_of_an_enumeration_then_its_values_are_not_ambiguous() {
    let source = "TYPE E1 : (U1, U2); EA : E1; END_TYPE
         PROGRAM main VAR x : EA := U1; y : E1; END_VAR y := U1; x := U2; END_PROGRAM";

    assert_eq!(codes(source), Vec::<String>::new());
}

#[test]
fn apply_when_alias_of_an_alias_of_an_enumeration_then_its_values_are_not_ambiguous() {
    let source = "TYPE E1 : (U1, U2); EA : E1; EB : EA; END_TYPE
         PROGRAM main VAR x : EB := U1; END_VAR x := U2; END_PROGRAM";

    assert_eq!(codes(source), Vec::<String>::new());
}

#[test]
fn apply_when_alias_of_one_of_two_enumerations_then_the_alias_is_not_a_third_owner() {
    let source = "TYPE E1 : (U1, U2); E2 : (U1, U3); EA : E1; END_TYPE
         PROGRAM main VAR x : E2; END_VAR x := U1; END_PROGRAM";

    let found = ambiguous(source, &default_options());

    assert_eq!(found.len(), 1);
    assert_eq!(found[0].declared.len(), 2);
}

#[test]
fn apply_when_two_enumerations_list_the_same_values_then_they_are_two_enumerations() {
    let source = "TYPE E1 : (U1, U2); E2 : (U1, U2); END_TYPE
         PROGRAM main VAR x : E1; END_VAR x := U2; END_PROGRAM";

    let found = ambiguous(source, &default_options());

    assert_eq!(found.len(), 1);
    assert_eq!(found[0].declared.len(), 2);
}

#[test]
fn apply_when_name_differs_only_in_case_then_it_is_the_same_name() {
    let source = "TYPE E1 : (Run, Stop); E2 : (RUN, Idle); END_TYPE
         PROGRAM main VAR x : E1; END_VAR x := run; END_PROGRAM";

    assert_eq!(ambiguous(source, &default_options()).len(), 1);
}

/// A value written with the name of its enumeration is never ambiguous, in
/// every place the language accepts that form today.
#[rstest]
#[case::initial_value_of_a_variable("PROGRAM main VAR w : E2 := E2#U1; END_VAR END_PROGRAM")]
#[case::initial_value_of_a_constant(
    "PROGRAM main VAR CONSTANT w : E2 := E2#U1; END_VAR END_PROGRAM"
)]
#[case::initial_value_of_a_structure_member_declaration(
    "TYPE S : STRUCT f : E2 := E2#U1; END_STRUCT; END_TYPE
     PROGRAM main VAR s : S; END_VAR END_PROGRAM"
)]
#[case::member_of_a_structure_initializer(
    "TYPE S : STRUCT f : E2; END_STRUCT; END_TYPE
     PROGRAM main VAR s : S := (f := E2#U1); END_VAR END_PROGRAM"
)]
#[case::input_of_a_function_block_instance(
    "FUNCTION_BLOCK FB VAR_INPUT i : E2; END_VAR END_FUNCTION_BLOCK
     PROGRAM main VAR f : FB := (i := E2#U1); END_VAR END_PROGRAM"
)]
#[case::element_of_an_array_initializer(
    "PROGRAM main VAR arr : ARRAY[1..2] OF E2 := [E2#U1, E2#U3]; END_VAR END_PROGRAM"
)]
#[case::default_of_an_enumeration_alias("TYPE E3 : E2 := E2#U1; END_TYPE PROGRAM main END_PROGRAM")]
#[case::the_first_of_the_sharing_enumerations(
    "PROGRAM main VAR v : E1 := E1#U1; END_VAR END_PROGRAM"
)]
fn apply_when_shared_value_name_is_qualified_then_no_diagnostic(#[case] body: &str) {
    assert_eq!(codes(&format!("{TYPES} {body}")), Vec::<String>::new());
}

/// The list of values of a declaration declares them, and the default beside
/// the list is one of them: neither is a use of a bare name.
#[rstest]
#[case::default_of_an_enumeration("TYPE E3 : (U1, U5) := U1; END_TYPE PROGRAM main END_PROGRAM")]
#[case::default_of_the_second_enumeration(
    "TYPE E3 : (U5, U1) := U1; E4 : (U1, U6) := U1; END_TYPE PROGRAM main END_PROGRAM"
)]
#[case::inline_enumeration_of_a_variable(
    "PROGRAM main VAR a : (U1, U7) := U1; END_VAR END_PROGRAM"
)]
fn apply_when_declaration_lists_a_shared_value_name_then_no_diagnostic(#[case] body: &str) {
    assert_eq!(codes(&format!("{TYPES} {body}")), Vec::<String>::new());
}

/// An enumeration whose declaration is not valid declares no value, and the
/// declaration is where the problem is reported: the use adds no second one.
#[rstest]
#[case::repeated_declaration_is_dropped(
    "TYPE E1 : (U1, U2); E1 : (U1, U3); END_TYPE
     PROGRAM main VAR x : E1; END_VAR x := U1; END_PROGRAM",
    &["P2007"]
)]
#[case::enumeration_of_an_undeclared_type(
    "TYPE E1 : (U1, U2); E3 : E_NOWHERE := U1; END_TYPE
     PROGRAM main VAR x : E1; END_VAR x := U1; END_PROGRAM",
    &["P2009"]
)]
#[case::variable_of_an_undeclared_type(
    "TYPE E1 : (U1, U2); END_TYPE
     PROGRAM main VAR x : E_NOWHERE := U1; END_VAR END_PROGRAM",
    &["P2008"]
)]
fn apply_when_declaration_has_an_error_then_no_second_diagnostic_at_the_use(
    #[case] source: &str,
    #[case] expected: &[&str],
) {
    let expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();

    assert_eq!(codes(source), expected);
}

/// The analysis gives the same report on every run: the enumerations that
/// declare a name come in the order they were declared.
#[test]
fn apply_when_run_many_times_then_the_report_is_the_same() {
    let source = "TYPE E1 : (U1, U2); E2 : (U1, U3); E3 : (U4, U1); E4 : (U1); END_TYPE
         PROGRAM main VAR x : E1; y : E2; END_VAR x := U1; y := U1; END_PROGRAM";

    let first = analyzed(source, &default_options());
    for _ in 0..24 {
        assert_eq!(analyzed(source, &default_options()), first);
    }
}
