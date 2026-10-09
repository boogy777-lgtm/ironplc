//! A unit that cannot be analyzed leaves every other unit analyzed in full.
//!
//! Each row is a program of one unit with an error in a pass of the type
//! resolution (a kind of pass failure) and the same program with that unit
//! replaced by a correct one of the same name. The other units are a type
//! file, a function block, a function, a correct program and a program with
//! errors of their own, each in a file of its own. Every message about the other
//! units must be the same, in the same order, whichever of the two is in the
//! project, and with the unit first or last.
//!
//! A unit that has an error shows the first cause only. The second table adds a
//! file that uses the unit in every way the language allows (a variable of it,
//! a parameter of a function and of a function block, a member of a structure,
//! an element of an array, an alias of it, a call, an instance) and has an
//! error of its own: with the unit failing, the messages about that file must
//! still be those it has with the unit correct, and the unit is reported once,
//! for its own problem. A recursive cycle is a failing unit like any other: its
//! rows are in the same table, and each runs under a time limit, so a step that
//! went round a cycle without end would fail the row rather than hang.

#![allow(
    clippy::unwrap_used,
    reason = "test target: panicking helpers are sanctioned in tests"
)]

use ironplc_analyzer::stages::analyze;
use ironplc_dsl::common::Library;
use ironplc_dsl::core::FileId;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_parser::parse_program;

const TYPES: &str = "
TYPE S_GOOD : STRUCT f : INT; g : BOOL; END_STRUCT; END_TYPE
TYPE E_GOOD : (E1, E2); END_TYPE
TYPE E_ALIAS : E_GOOD; END_TYPE
TYPE A_GOOD : ARRAY[1..3] OF INT; END_TYPE
";

const BLOCK: &str = "
FUNCTION_BLOCK FB_GOOD
VAR_INPUT x : INT; END_VAR
VAR y : S_GOOD; e : E_ALIAS; a : A_GOOD; END_VAR
  y.f := x;
  e := E1;
  a[1] := x;
END_FUNCTION_BLOCK
";

const FUNCTION: &str = "
FUNCTION F_GOOD : INT
VAR_INPUT a : INT; b : INT; END_VAR
  F_GOOD := a + b;
END_FUNCTION
";

const PROGRAM_OK: &str = "
PROGRAM p_ok
VAR i : FB_GOOD; r : INT; END_VAR
  i(x := 1);
  r := F_GOOD(b := 2, a := 1);
END_PROGRAM
";

/// A program with errors of its own: a member of the wrong type, an operand of
/// the wrong type, an undeclared variable, a named argument that is not a
/// parameter and a division by zero.
const PROGRAM_ERRORS: &str = "
PROGRAM p_err
VAR s : S_GOOD; c : INT; d : BOOL; k : E_ALIAS; r : INT; END_VAR
  s.f := TRUE;
  c := d + 1;
  r := UNDECLARED_R;
  k := E1;
  r := F_GOOD(zz := 1);
  r := 2 / 0;
END_PROGRAM
";

const OTHERS: [(&str, &str); 5] = [
    ("types.st", TYPES),
    ("block.st", BLOCK),
    ("function.st", FUNCTION),
    ("program_ok.st", PROGRAM_OK),
    ("program_errors.st", PROGRAM_ERRORS),
];

const UNIT_FILE: &str = "unit.st";
const USES_FILE: &str = "uses.st";

/// How long one analysis of these small projects may take. They take
/// milliseconds; a step that goes round a cycle without end takes forever.
const TIME_LIMIT: std::time::Duration = std::time::Duration::from_secs(60);

/// [`analysis`] on a thread of its own with a time limit.
fn analysis_within_limit(unit: &'static str, uses: Option<&'static str>, unit_first: bool) -> Reported {
    let (sender, receiver) = std::sync::mpsc::channel();
    std::thread::spawn(move || {
        let _ = sender.send(analysis(unit, uses, unit_first));
    });
    receiver
        .recv_timeout(TIME_LIMIT)
        .expect("the analysis finished within the time limit")
}

fn options() -> CompilerOptions {
    CompilerOptions {
        allow_fb_inheritance: true,
        allow_top_level_var_global: true,
        ..CompilerOptions::from_dialect(Dialect::Rusty)
    }
}

fn parsed(file: &str, text: &str) -> Library {
    parse_program(text, &FileId::from_string(file), &options()).unwrap()
}

/// What the analysis of the other units reports, and what it reports about the
/// unit: the messages of each as `file code message start..end`, in order.
struct Reported {
    others: Vec<String>,
    unit: Vec<String>,
}

fn analysis(unit: &str, uses: Option<&str>, unit_first: bool) -> Reported {
    let mut libraries: Vec<Library> = OTHERS
        .iter()
        .map(|(file, text)| parsed(file, text))
        .collect();
    libraries.extend(uses.map(|text| parsed(USES_FILE, text)));
    let unit_library = parsed(UNIT_FILE, unit);
    if unit_first {
        libraries.insert(0, unit_library);
    } else {
        libraries.push(unit_library);
    }
    let references: Vec<&Library> = libraries.iter().collect();
    let (_library, context) = analyze(&references, &options()).unwrap();

    let unit_id = format!("{:?}", FileId::from_string(UNIT_FILE));
    let mut reported = Reported {
        others: vec![],
        unit: vec![],
    };
    for diagnostic in context.diagnostics() {
        let file = format!("{:?}", diagnostic.primary.file_id);
        let line = format!(
            "{file} {} {} {}..{}",
            diagnostic.code,
            diagnostic.primary.message,
            diagnostic.primary.location.start,
            diagnostic.primary.location.end
        );
        if file == unit_id {
            reported.unit.push(diagnostic.code.clone());
        } else {
            reported.others.push(line);
        }
    }
    reported
}

/// One kind of pass failure: a unit that fails in that pass, the correct unit
/// of the same name, and the codes the failing unit is reported with.
#[rstest::rstest]
#[case::declared_type_alias_of_undeclared_type(
    "TYPE T_UNIT : T_NOWHERE; END_TYPE",
    "TYPE T_UNIT : INT; END_TYPE",
    &["P2011"]
)]
#[case::structure_field_of_undeclared_type(
    "TYPE T_UNIT : STRUCT a : T_NOWHERE; END_STRUCT; END_TYPE",
    "TYPE T_UNIT : STRUCT a : INT; END_STRUCT; END_TYPE",
    &["P2021"]
)]
#[case::array_of_undeclared_element_type(
    "TYPE T_UNIT : ARRAY[1..3] OF T_NOWHERE; END_TYPE",
    "TYPE T_UNIT : ARRAY[1..3] OF INT; END_TYPE",
    &["P2013"]
)]
#[case::array_with_bounds_in_the_wrong_order(
    "TYPE T_UNIT : ARRAY[3..1] OF INT; END_TYPE",
    "TYPE T_UNIT : ARRAY[1..3] OF INT; END_TYPE",
    &["P2024"]
)]
#[case::subrange_with_bounds_in_the_wrong_order(
    "TYPE T_UNIT : INT (5..1); END_TYPE",
    "TYPE T_UNIT : INT (1..5); END_TYPE",
    &["P2002"]
)]
#[case::enumeration_alias_of_undeclared_enumeration(
    "TYPE T_UNIT : E_NOWHERE := A1; END_TYPE",
    "TYPE T_UNIT : (A1, A2); END_TYPE",
    &["P2009"]
)]
#[case::reference_to_undeclared_type(
    "TYPE T_UNIT : REF_TO T_NOWHERE; END_TYPE",
    "TYPE T_UNIT : REF_TO INT; END_TYPE",
    &["P2011"]
)]
#[case::function_block_variable_of_undeclared_type_with_initializer(
    "FUNCTION_BLOCK T_UNIT VAR x : T_NOWHERE := 1; END_VAR END_FUNCTION_BLOCK",
    "FUNCTION_BLOCK T_UNIT VAR x : INT := 1; END_VAR END_FUNCTION_BLOCK",
    &["P2011"]
)]
#[case::self_reference_written(
    "FUNCTION_BLOCK T_UNIT VAR count : INT; END_VAR METHOD Run THIS^.count := 1; END_METHOD END_FUNCTION_BLOCK",
    "FUNCTION_BLOCK T_UNIT VAR count : INT; END_VAR METHOD Run count := 1; END_METHOD END_FUNCTION_BLOCK",
    &["P9999"]
)]
#[case::self_reference_read(
    "FUNCTION_BLOCK T_UNIT VAR count : INT; END_VAR METHOD Run count := THIS^.count; END_METHOD END_FUNCTION_BLOCK",
    "FUNCTION_BLOCK T_UNIT VAR count : INT; END_VAR METHOD Run count := count; END_METHOD END_FUNCTION_BLOCK",
    &["P9999"]
)]
#[case::constant_expression_divides_by_zero(
    "PROGRAM t_unit VAR z : INT; END_VAR z := 1 / 0; END_PROGRAM",
    "PROGRAM t_unit VAR z : INT; END_VAR z := 1 / 1; END_PROGRAM",
    &["P4039"]
)]
fn analyze_when_one_unit_fails_in_a_pass_then_messages_about_other_units_are_those_of_a_correct_unit(
    #[case] unit: &str,
    #[case] correct: &str,
    #[case] expected: &[&str],
    #[values(true, false)] unit_first: bool,
) {
    let with_unit = analysis(unit, None, unit_first);
    let with_correct = analysis(correct, None, unit_first);

    assert!(
        !with_correct.others.is_empty(),
        "the other units carry errors of their own"
    );
    assert_eq!(with_correct.others, with_unit.others);
    assert!(
        with_correct.unit.is_empty(),
        "the correct unit is not reported: {:?}",
        with_correct.unit
    );
    assert!(
        expected
            .iter()
            .all(|code| with_unit.unit.iter().any(|reported| reported == code)),
        "{expected:?} are not all among {:?}",
        with_unit.unit
    );
}

/// Uses of a unit named `T_UNIT` that is a type, in every way the language
/// allows, and an error of the using file's own (`k := 'text'`).
const TYPE_USES: &str = "
FUNCTION G : INT VAR_INPUT p : T_UNIT; END_VAR G := 1; END_FUNCTION
FUNCTION_BLOCK UB VAR_INPUT p : T_UNIT; END_VAR VAR q : T_UNIT; END_VAR q := p; END_FUNCTION_BLOCK
TYPE US : STRUCT m : T_UNIT; n : INT; END_STRUCT; END_TYPE
TYPE UA : ARRAY[1..2] OF T_UNIT; END_TYPE
TYPE UALIAS : T_UNIT; END_TYPE
PROGRAM p_use
VAR v : T_UNIT; w : T_UNIT; ub : UB; us : US; ua : UA; al : UALIAS; k : INT; arr : ARRAY[1..2] OF T_UNIT; END_VAR
  v := w; k := G(v); ub(p := v); ub.q := v; us.m := v; ua[1] := v; arr[1] := v; al := v;
  k := 'text';
END_PROGRAM
";

/// [`TYPE_USES`] for a type a number can be used as: the value is assigned a
/// number and is an operand.
const SCALAR_USES: &str = "
FUNCTION G : INT VAR_INPUT p : T_UNIT; END_VAR G := 1; END_FUNCTION
FUNCTION_BLOCK UB VAR_INPUT p : T_UNIT; END_VAR VAR q : T_UNIT; END_VAR q := p; END_FUNCTION_BLOCK
TYPE US : STRUCT m : T_UNIT; n : INT; END_STRUCT; END_TYPE
TYPE UA : ARRAY[1..2] OF T_UNIT; END_TYPE
TYPE UALIAS : T_UNIT; END_TYPE
PROGRAM p_use
VAR v : T_UNIT; w : T_UNIT; ub : UB; us : US; ua : UA; al : UALIAS; k : INT; b : BOOL; arr : ARRAY[1..2] OF T_UNIT; END_VAR
  v := w; k := G(v); ub(p := v); ub.q := v; us.m := v; ua[1] := v; arr[1] := v; al := v;
  v := 3; k := v + 1; b := v = 1; b := v < w;
  IF v > 1 THEN k := 2; END_IF;
  k := 'text';
END_PROGRAM
";

/// [`TYPE_USES`] for a reference type, which cannot be the element of an array.
const REFERENCE_USES: &str = "
FUNCTION G : INT VAR_INPUT p : T_UNIT; END_VAR G := 1; END_FUNCTION
FUNCTION_BLOCK UB VAR_INPUT p : T_UNIT; END_VAR VAR q : T_UNIT; END_VAR q := p; END_FUNCTION_BLOCK
TYPE US : STRUCT m : T_UNIT; n : INT; END_STRUCT; END_TYPE
PROGRAM p_use
VAR v : T_UNIT; w : T_UNIT; ub : UB; us : US; k : INT; END_VAR
  v := w; k := G(v); ub(p := v); ub.q := v; us.m := v;
  k := 'text';
END_PROGRAM
";

/// [`TYPE_USES`] for a structure with a member `a`, which is used too.
const STRUCTURE_USES: &str = "
FUNCTION G : INT VAR_INPUT p : T_UNIT; END_VAR G := 1; END_FUNCTION
FUNCTION_BLOCK UB VAR_INPUT p : T_UNIT; END_VAR VAR q : T_UNIT; END_VAR q := p; END_FUNCTION_BLOCK
TYPE US : STRUCT m : T_UNIT; n : INT; END_STRUCT; END_TYPE
TYPE UA : ARRAY[1..2] OF T_UNIT; END_TYPE
TYPE UALIAS : T_UNIT; END_TYPE
PROGRAM p_use
VAR v : T_UNIT; w : T_UNIT; ub : UB; us : US; ua : UA; al : UALIAS; k : INT; arr : ARRAY[1..2] OF T_UNIT; END_VAR
  v := w; k := G(v); ub(p := v); ub.q := v; us.m := v; ua[1] := v; arr[1] := v; al := v;
  v.a := 1; k := v.a; us.m.a := 2; arr[2].a := k;
  k := 'text';
END_PROGRAM
";

/// Uses of a unit named `T_UNIT` that is a function block with the input `p`
/// and the variable `y`.
const FUNCTION_BLOCK_USES: &str = "
FUNCTION_BLOCK UB VAR i : T_UNIT; END_VAR i(p := 1); END_FUNCTION_BLOCK
FUNCTION_BLOCK UC EXTENDS T_UNIT VAR z : INT; END_VAR z := 1; END_FUNCTION_BLOCK
PROGRAM p_use
VAR v : T_UNIT; ub : UB; uc : UC; k : INT; END_VAR
  v(p := 1); v(); k := v.y; v.y := 2; ub(); uc(p := 2); k := uc.y; k := uc.z;
  k := 'text';
END_PROGRAM
";

/// Uses of a unit named `T_UNIT` that is a function with the inputs `a` and `b`.
const FUNCTION_USES: &str = "
FUNCTION H : INT VAR_INPUT a : INT; END_VAR H := T_UNIT(a, 1); END_FUNCTION
FUNCTION_BLOCK UB VAR r : INT; END_VAR r := T_UNIT(1, 2); END_FUNCTION_BLOCK
PROGRAM p_use
VAR k : INT; s : STRING; END_VAR
  k := T_UNIT(1, 2); k := T_UNIT(a := 1, b := 2); k := T_UNIT(1); k := T_UNIT(1, 'b'); s := T_UNIT(1, 2);
  k := T_UNIT(1, 2) + 1; k := H(1);
  k := 'text';
END_PROGRAM
";

/// A unit that has an error, the correct unit of the same name, what uses
/// them, and the codes the unit is reported with.
#[rstest::rstest]
// A declaration that has an error of its own.
#[case::alias_of_undeclared_type(
    "TYPE T_UNIT : T_NOWHERE; END_TYPE",
    "TYPE T_UNIT : INT; END_TYPE",
    SCALAR_USES,
    &["P2011"]
)]
#[case::structure_with_member_of_undeclared_type(
    "TYPE T_UNIT : STRUCT a : INT; m : T_NOWHERE; END_STRUCT; END_TYPE",
    "TYPE T_UNIT : STRUCT a : INT; m : INT; END_STRUCT; END_TYPE",
    STRUCTURE_USES,
    &["P2008", "P2021"]
)]
#[case::enumeration_alias_of_undeclared_enumeration(
    "TYPE T_UNIT : E_NOWHERE := U1; END_TYPE",
    "TYPE T_UNIT : (U1, U2); END_TYPE",
    TYPE_USES,
    &["P2009"]
)]
#[case::subrange_with_bounds_in_the_wrong_order(
    "TYPE T_UNIT : INT (5..1); END_TYPE",
    "TYPE T_UNIT : INT (1..5); END_TYPE",
    SCALAR_USES,
    &["P2002", "P2002"]
)]
#[case::array_of_undeclared_element_type(
    "TYPE T_UNIT : ARRAY[1..3] OF T_NOWHERE; END_TYPE",
    "TYPE T_UNIT : ARRAY[1..3] OF INT; END_TYPE",
    TYPE_USES,
    &["P2013"]
)]
#[case::array_with_bounds_in_the_wrong_order(
    "TYPE T_UNIT : ARRAY[3..1] OF INT; END_TYPE",
    "TYPE T_UNIT : ARRAY[1..3] OF INT; END_TYPE",
    TYPE_USES,
    &["P2024", "P2024"]
)]
#[case::reference_to_undeclared_type(
    "TYPE T_UNIT : REF_TO T_NOWHERE; END_TYPE",
    "TYPE T_UNIT : REF_TO INT; END_TYPE",
    REFERENCE_USES,
    &["P2011"]
)]
#[case::function_block_with_variable_of_undeclared_type(
    "FUNCTION_BLOCK T_UNIT VAR_INPUT p : INT; END_VAR VAR y : INT; z : T_NOWHERE := 1; END_VAR END_FUNCTION_BLOCK",
    "FUNCTION_BLOCK T_UNIT VAR_INPUT p : INT; END_VAR VAR y : INT; z : INT := 1; END_VAR END_FUNCTION_BLOCK",
    FUNCTION_BLOCK_USES,
    &["P2011"]
)]
#[case::function_with_parameter_of_undeclared_type(
    "FUNCTION T_UNIT : INT VAR_INPUT a : T_NOWHERE; b : INT; END_VAR T_UNIT := b; END_FUNCTION",
    "FUNCTION T_UNIT : INT VAR_INPUT a : INT; b : INT; END_VAR T_UNIT := b; END_FUNCTION",
    FUNCTION_USES,
    &["P2008"]
)]
fn analyze_when_a_unit_with_an_error_is_used_then_only_its_first_cause_and_the_independent_errors_are_reported(
    #[case] unit: &'static str,
    #[case] correct: &'static str,
    #[case] uses: &'static str,
    #[case] expected: &[&str],
    #[values(true, false)] unit_first: bool,
) {
    let (with_unit, with_correct) = analysis_pair(unit, correct, uses, unit_first);

    assert_eq!(with_correct.others, with_unit.others);
    assert_first_cause_only(&with_unit, expected);
}

/// The analysis of the project with the unit that has an error and with the
/// correct unit, each under the time limit, and what the using file and the
/// other units must carry in both: errors of their own.
fn analysis_pair(
    unit: &'static str,
    correct: &'static str,
    uses: &'static str,
    unit_first: bool,
) -> (Reported, Reported) {
    let with_unit = analysis_within_limit(unit, Some(uses), unit_first);
    let with_correct = analysis_within_limit(correct, Some(uses), unit_first);
    assert!(
        with_correct.others.len() > 1,
        "the using file and the other units carry errors of their own: {:?}",
        with_correct.others
    );
    (with_unit, with_correct)
}

/// The unit is reported once for each problem of its own, and for nothing else.
fn assert_first_cause_only(with_unit: &Reported, expected: &[&str]) {
    let mut reported = with_unit.unit.clone();
    reported.sort();
    let mut expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();
    expected.sort();
    assert_eq!(expected, reported);
}

/// A recursive cycle: the unit is a member, and the correct unit breaks it.
/// The messages about the other units are the same as with the cycle broken,
/// but not necessarily in the same order: the cycle takes edges out of the
/// graph the declarations are ordered by, and the messages follow that order.
#[rstest::rstest]
#[case::cycle_of_two_types(
    "TYPE T_UNIT : STRUCT o : T_OTHER; END_STRUCT; END_TYPE TYPE T_OTHER : STRUCT u : T_UNIT; END_STRUCT; END_TYPE",
    "TYPE T_UNIT : STRUCT a : INT; END_STRUCT; END_TYPE TYPE T_OTHER : STRUCT u : T_UNIT; END_STRUCT; END_TYPE",
    STRUCTURE_USES,
    &["P4005"]
)]
#[case::cycle_of_three_types(
    "TYPE T_UNIT : STRUCT a : INT; o : T_B; END_STRUCT; END_TYPE TYPE T_B : STRUCT c : T_C; END_STRUCT; END_TYPE TYPE T_C : STRUCT u : T_UNIT; END_STRUCT; END_TYPE",
    "TYPE T_UNIT : STRUCT a : INT; o : T_B; END_STRUCT; END_TYPE TYPE T_B : STRUCT c : T_C; END_STRUCT; END_TYPE TYPE T_C : STRUCT u : INT; END_STRUCT; END_TYPE",
    STRUCTURE_USES,
    &["P4005"]
)]
#[case::type_that_holds_itself(
    "TYPE T_UNIT : STRUCT a : INT; u : T_UNIT; END_STRUCT; END_TYPE",
    "TYPE T_UNIT : STRUCT a : INT; u : INT; END_STRUCT; END_TYPE",
    STRUCTURE_USES,
    &["P4005"]
)]
#[case::cycle_of_two_aliases(
    "TYPE T_UNIT : T_OTHER; END_TYPE TYPE T_OTHER : T_UNIT; END_TYPE",
    "TYPE T_UNIT : T_OTHER; END_TYPE TYPE T_OTHER : INT; END_TYPE",
    SCALAR_USES,
    &["P4005"]
)]
#[case::array_that_holds_itself(
    "TYPE T_UNIT : ARRAY[1..2] OF T_UNIT; END_TYPE",
    "TYPE T_UNIT : ARRAY[1..2] OF INT; END_TYPE",
    TYPE_USES,
    &["P4005"]
)]
#[case::cycle_of_two_function_blocks(
    "FUNCTION_BLOCK T_UNIT VAR_INPUT p : INT; END_VAR VAR y : INT; o : T_OTHER; END_VAR END_FUNCTION_BLOCK FUNCTION_BLOCK T_OTHER VAR u : T_UNIT; END_VAR END_FUNCTION_BLOCK",
    "FUNCTION_BLOCK T_UNIT VAR_INPUT p : INT; END_VAR VAR y : INT; o : T_OTHER; END_VAR END_FUNCTION_BLOCK FUNCTION_BLOCK T_OTHER VAR u : INT; END_VAR END_FUNCTION_BLOCK",
    FUNCTION_BLOCK_USES,
    &["P4005"]
)]
#[case::function_block_that_holds_itself(
    "FUNCTION_BLOCK T_UNIT VAR_INPUT p : INT; END_VAR VAR y : INT; u : T_UNIT; END_VAR END_FUNCTION_BLOCK",
    "FUNCTION_BLOCK T_UNIT VAR_INPUT p : INT; END_VAR VAR y : INT; u : INT; END_VAR END_FUNCTION_BLOCK",
    FUNCTION_BLOCK_USES,
    &["P4005"]
)]
#[case::cycle_of_two_functions(
    "FUNCTION T_UNIT : INT VAR_INPUT a : INT; b : INT; END_VAR T_UNIT := T_OTHER(a); END_FUNCTION FUNCTION T_OTHER : INT VAR_INPUT a : INT; END_VAR T_OTHER := T_UNIT(a, a); END_FUNCTION",
    "FUNCTION T_UNIT : INT VAR_INPUT a : INT; b : INT; END_VAR T_UNIT := T_OTHER(a); END_FUNCTION FUNCTION T_OTHER : INT VAR_INPUT a : INT; END_VAR T_OTHER := a; END_FUNCTION",
    FUNCTION_USES,
    &["P4005"]
)]
#[case::function_that_calls_itself(
    "FUNCTION T_UNIT : INT VAR_INPUT a : INT; b : INT; END_VAR T_UNIT := T_UNIT(a, b); END_FUNCTION",
    "FUNCTION T_UNIT : INT VAR_INPUT a : INT; b : INT; END_VAR T_UNIT := b; END_FUNCTION",
    FUNCTION_USES,
    &["P4005"]
)]
fn analyze_when_a_unit_is_in_a_cycle_then_only_the_cycle_and_the_independent_errors_are_reported(
    #[case] unit: &'static str,
    #[case] correct: &'static str,
    #[case] uses: &'static str,
    #[case] expected: &[&str],
    #[values(true, false)] unit_first: bool,
) {
    let (with_unit, with_correct) = analysis_pair(unit, correct, uses, unit_first);

    let mut others_with_unit = with_unit.others.clone();
    others_with_unit.sort();
    let mut others_with_correct = with_correct.others.clone();
    others_with_correct.sort();
    assert_eq!(others_with_correct, others_with_unit);
    assert_first_cause_only(&with_unit, expected);
}
