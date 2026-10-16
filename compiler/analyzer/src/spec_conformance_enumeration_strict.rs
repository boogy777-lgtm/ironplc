//! Spec conformance tests for the attribute `strict` before an enumeration
//! (analyzer-owned requirements `REQ-ATTR-analyzer-030` to `-034`): which
//! enumerations are strict, and what the places of the class do with one that
//! is and one that is not.
//!
//! See `specs/design/attributes.md`.

use crate::stages::analyze;
use ironplc_dsl::common::TypeName;
use ironplc_dsl::core::FileId;
use ironplc_parser::options::{CompilerOptions, Dialect, EnumerationStrictness};
use ironplc_parser::parse_program;
use rstest::rstest;
use spec_test_macro::spec_test;

/// How the enumerations of a program come to be strict: the options, and
/// whether `E1` and `E2` carry `{attribute 'strict'}`.
#[derive(Clone, Copy, Debug)]
struct Setup {
    dialect: Dialect,
    policy: Option<EnumerationStrictness>,
    attribute: bool,
}

impl Setup {
    fn options(self) -> CompilerOptions {
        let mut options = CompilerOptions::from_dialect(self.dialect);
        if let Some(policy) = self.policy {
            options.policy_enumeration_strictness = policy;
        }
        options
    }
}

const IEC: Setup = Setup {
    dialect: Dialect::Iec61131_3Ed2,
    policy: None,
    attribute: false,
};
const CODESYS_ATTRIBUTE: Setup = Setup {
    dialect: Dialect::Codesys,
    policy: None,
    attribute: true,
};
const CODESYS_PLAIN: Setup = Setup {
    dialect: Dialect::Codesys,
    policy: None,
    attribute: false,
};
const TWINCAT_PLAIN: Setup = Setup {
    dialect: Dialect::TwinCat,
    policy: None,
    attribute: false,
};
const TWINCAT_ATTRIBUTE: Setup = Setup {
    dialect: Dialect::TwinCat,
    policy: None,
    attribute: true,
};
const CODESYS_EVERY: Setup = Setup {
    dialect: Dialect::Codesys,
    policy: Some(EnumerationStrictness::EveryEnumeration),
    attribute: false,
};
const IEC_WITH_ATTRIBUTE_POLICY: Setup = Setup {
    dialect: Dialect::Iec61131_3Ed2,
    policy: Some(EnumerationStrictness::WithAttribute),
    attribute: false,
};

/// The program: `E1` and `E2` (strict by the setup), the alias `E3` of `E1`,
/// and a program that declares `x : E1`, `a : E3`, `z : E2`, `i : INT`,
/// `d : DINT` and an inline enumeration `w`, and runs `statements`.
fn source_of(setup: Setup, statements: &str) -> String {
    let pragma = if setup.attribute {
        "{attribute 'strict'} "
    } else {
        ""
    };
    format!(
        "{pragma}TYPE E1 : (A, B, C); END_TYPE {pragma}TYPE E2 : (P, Q, R); END_TYPE \
         TYPE E3 : E1; END_TYPE \
         PROGRAM main VAR x : E1 := C; y : E1; a : E3 := C; z : E2 := Q; i : INT; d : DINT; g : BOOL; \
         w : (U, V) := V; END_VAR {statements} END_PROGRAM"
    )
}

/// The code of the first diagnostic of `source`, or `ok`.
fn first_code(source: &str, options: &CompilerOptions) -> String {
    let library = parse_program(source, &FileId::default(), options).expect("the source parses");
    match analyze(&[&library], options) {
        Err(found) => found[0].code.clone(),
        Ok((_, context)) => context
            .diagnostics()
            .first()
            .map_or_else(|| "ok".to_owned(), |d| d.code.clone()),
    }
}

/// Whether the analysis of the program records `E1` as strict.
fn e1_is_strict(setup: Setup) -> bool {
    let options = setup.options();
    let source = source_of(setup, "");
    let library = parse_program(&source, &FileId::default(), &options).unwrap();
    let (_, context) = analyze(&[&library], &options).unwrap();
    context
        .types()
        .get(&TypeName::from("E1"))
        .unwrap()
        .representation
        .is_strict_enumeration()
}

#[rstest]
#[case::iec_without_attribute(IEC, true)]
#[case::codesys_with_attribute(CODESYS_ATTRIBUTE, true)]
#[case::codesys_without_attribute(CODESYS_PLAIN, false)]
#[case::twincat_without_attribute(TWINCAT_PLAIN, false)]
#[case::twincat_with_attribute(TWINCAT_ATTRIBUTE, true)]
#[case::codesys_every_enumeration(CODESYS_EVERY, true)]
#[case::iec_with_attribute_policy(IEC_WITH_ATTRIBUTE_POLICY, false)]
#[spec_test(REQ_ATTR_analyzer_030)]
fn analyze_when_enumeration_declared_then_strict_by_the_policy_and_the_attribute(
    #[case] setup: Setup,
    #[case] strict: bool,
) {
    assert_eq!(e1_is_strict(setup), strict);
}

/// A statement of the class, with the code that refuses it for a strict
/// enumeration (REQ-ATTR-analyzer-031 and -032).
const REFUSED: [(&str, &str); 18] = [
    ("y := x + 1;", "P4049"),
    ("i := x * 2;", "P4049"),
    ("i := -x;", "P4049"),
    ("i := ADD(x, 1);", "P4049"),
    ("i := ABS(x);", "P4026"),
    ("i := EXPT(x, 2);", "P4026"),
    ("FOR x := A TO C DO i := 1; END_FOR;", "P4049"),
    ("y := MAX(x, 1);", "P4027"),
    ("y := SEL(g, x, z);", "P4027"),
    ("y := MUX(1, x, z);", "P4027"),
    ("y := i;", "P4035"),
    ("y := z;", "P4035"),
    ("y := 7;", "P2006"),
    ("y := a + 1;", "P4049"),
    ("i := a MOD 2;", "P4049"),
    ("FOR a := A TO C DO i := 1; END_FOR;", "P4049"),
    ("a := i;", "P4035"),
    ("a := 7;", "P2006"),
];

#[rstest]
#[case::iec_without_attribute(IEC)]
#[case::codesys_with_attribute(CODESYS_ATTRIBUTE)]
#[case::twincat_with_attribute(TWINCAT_ATTRIBUTE)]
#[case::codesys_every_enumeration(CODESYS_EVERY)]
#[spec_test(REQ_ATTR_analyzer_031)]
#[spec_test(REQ_ATTR_analyzer_032)]
fn analyze_when_strict_enumeration_computed_with_or_assigned_a_number_then_refused(
    #[case] setup: Setup,
) {
    let options = setup.options();
    let mismatches: Vec<String> = REFUSED
        .iter()
        .filter_map(|(statements, expected)| {
            let got = first_code(&source_of(setup, statements), &options);
            (got != *expected).then(|| format!("{statements}: expected {expected}, got {got}"))
        })
        .collect();

    assert_eq!(mismatches, Vec::<String>::new());
}

#[rstest]
#[case::codesys_without_attribute(CODESYS_PLAIN)]
#[case::twincat_without_attribute(TWINCAT_PLAIN)]
#[case::iec_with_attribute_policy(IEC_WITH_ATTRIBUTE_POLICY)]
#[spec_test(REQ_ATTR_analyzer_033)]
fn analyze_when_enumeration_not_strict_then_every_place_of_the_class_accepts_it(
    #[case] setup: Setup,
) {
    let options = setup.options();
    let refused: Vec<String> = REFUSED
        .iter()
        .filter_map(|(statements, _)| {
            let got = first_code(&source_of(setup, statements), &options);
            (got != "ok").then(|| format!("{statements}: {got}"))
        })
        .collect();

    assert_eq!(refused, Vec::<String>::new());
}

/// The places that only hold an enumeration are the same whether it is strict
/// or not (the near cases of the class).
#[rstest]
#[case::iec_without_attribute(IEC)]
#[case::codesys_with_attribute(CODESYS_ATTRIBUTE)]
#[case::codesys_without_attribute(CODESYS_PLAIN)]
fn analyze_when_enumeration_only_held_or_compared_then_accepted_whether_strict_or_not(
    #[case] setup: Setup,
) {
    let options = setup.options();
    for statements in [
        "g := x = 2;",
        "g := x = z;",
        "i := x;",
        "y := x;",
        "y := a;",
        "y := SEL(g, x, A);",
        "y := MAX(x, y);",
        "g := GT(x, A);",
        "d := INT_TO_DINT(x);",
        "FOR i := A TO C DO g := TRUE; END_FOR;",
    ] {
        assert_eq!(
            first_code(&source_of(setup, statements), &options),
            "ok",
            "{statements}"
        );
    }
}

#[rstest]
#[case::attribute_policy_inline_not_strict(CODESYS_ATTRIBUTE, "i := w + 1;", "ok")]
#[case::iec_inline_strict(IEC, "i := w + 1;", "P4049")]
#[case::every_enumeration_inline_strict(CODESYS_EVERY, "i := w + 1;", "P4049")]
#[case::alias_of_strict_is_strict(CODESYS_ATTRIBUTE, "i := a + 1;", "P4049")]
#[case::alias_of_not_strict_is_not_strict(CODESYS_PLAIN, "i := a + 1;", "ok")]
#[spec_test(REQ_ATTR_analyzer_034)]
fn analyze_when_alias_or_inline_enumeration_then_strict_as_the_declaration_it_names_or_the_policy_says(
    #[case] setup: Setup,
    #[case] statements: &str,
    #[case] expected: &str,
) {
    let options = setup.options();

    assert_eq!(
        first_code(&source_of(setup, statements), &options),
        expected
    );
}
