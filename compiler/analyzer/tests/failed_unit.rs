//! A unit that cannot be analyzed leaves every other unit analyzed in full.
//!
//! Each row is a program of one unit with an error in a pass of the type
//! resolution (a kind of pass failure) and the same program with that unit
//! replaced by a correct one of the same name. The other units are a type
//! file, a function block, a function, a correct program and a program with
//! errors of its own, each in a file of its own. Every message about the other
//! units must be the same, in the same order, whichever of the two is in the
//! project, and with the unit first or last.

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

fn analysis(unit: &str, unit_first: bool) -> Reported {
    let mut libraries: Vec<Library> = OTHERS
        .iter()
        .map(|(file, text)| parsed(file, text))
        .collect();
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
    let with_unit = analysis(unit, unit_first);
    let with_correct = analysis(correct, unit_first);

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
