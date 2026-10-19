//! Tests for the comparison of values of two different enumerations. See
//! `compared`.

use crate::test_helpers::parse_and_resolve_types_with_options;
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::{Diagnostic, Severity};
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_parser::parse_program;
use ironplc_problems::Problem;
use rstest::rstest;
use spec_test_macro::spec_test;

/// Whether the declaration of an enumeration carries `{attribute 'strict'}`,
/// which makes it strict under the options of CODESYS.
#[derive(Clone, Copy, Debug)]
enum Strictness {
    Strict,
    NotStrict,
}

use Strictness::{NotStrict, Strict};

/// The program with `body`: it declares the enumerations `E1` and `E2`, the
/// alias `E3` of `E1`, a variable of each (`one`, `two`, `three`), the integer
/// `n`, the flag `g` and a reference `to_one` to `E1`.
fn program(first: Strictness, second: Strictness, body: &str) -> String {
    let pragma = |strictness: Strictness| match strictness {
        Strict => "{attribute 'strict'} ",
        NotStrict => "",
    };
    format!(
        "
{first_pragma}TYPE E1 : (A, B, C); END_TYPE
{second_pragma}TYPE E2 : (P, Q, R); END_TYPE
TYPE E3 : E1; END_TYPE

PROGRAM main
VAR
  one : E1;
  two : E2;
  three : E3;
  to_one : REF_TO E1;
  n : DINT;
  g : BOOL;
END_VAR
  {body}
END_PROGRAM",
        first_pragma = pragma(first),
        second_pragma = pragma(second),
    )
}

/// The diagnostics the rule reports for `body` under the options of CODESYS.
fn diagnostics(first: Strictness, second: Strictness, body: &str) -> Vec<Diagnostic> {
    let mut options = CompilerOptions::from_dialect(Dialect::Codesys);
    options.allow_ref_to = true;
    let (library, context) =
        parse_and_resolve_types_with_options(&program(first, second, body), &options);
    super::apply(&library, &context, &options)
        .err()
        .unwrap_or_default()
}

fn codes(first: Strictness, second: Strictness, body: &str) -> Vec<String> {
    diagnostics(first, second, body)
        .into_iter()
        .map(|d| d.code)
        .collect()
}

const P4083: &str = "P4083";

#[rstest]
#[case::equal("g := one = two;")]
#[case::not_equal("g := one <> two;")]
#[case::less("g := one < two;")]
#[case::less_or_equal("g := one <= two;")]
#[case::greater("g := one > two;")]
#[case::greater_or_equal("g := one >= two;")]
#[case::equal_function("g := EQ(one, two);")]
#[case::not_equal_function("g := NE(one, two);")]
#[case::less_function("g := LT(one, two);")]
#[case::less_or_equal_function("g := LE(one, two);")]
#[case::greater_function("g := GT(one, two);")]
#[case::greater_or_equal_function("g := GE(one, two);")]
#[case::min("n := MIN(one, two);")]
#[case::max("n := MAX(one, two);")]
#[spec_test(REQ_DS_analyzer_001)]
fn apply_when_values_of_two_enumerations_compared_then_p4083_once(
    #[case] body: &str,
    #[values(Strict, NotStrict)] first: Strictness,
    #[values(Strict, NotStrict)] second: Strictness,
) {
    assert_eq!(codes(first, second, body), vec![P4083]);
}

#[test]
#[spec_test(REQ_DS_analyzer_002)]
fn apply_when_enumerations_compared_then_a_warning_that_names_both_types_at_the_comparison() {
    let body = "g := one = two;";

    let found = diagnostics(Strict, Strict, body);

    assert_eq!(found.len(), 1);
    let warning = &found[0];
    assert_eq!(warning.code, Problem::EnumerationTypesCompared.code());
    assert_eq!(warning.severity(), Severity::Warning);
    assert!(!warning.stops_build());
    assert_eq!(warning.codesys(), Some("C0354"));
    assert_eq!(
        warning.primary.message,
        "Comparison of one enumeration type (E1) with another (E2)"
    );
    // The label is the whole comparison, `one = two`.
    let program_start = program(Strict, Strict, body)
        .find("one = two")
        .expect("the comparison is in the program");
    assert_eq!(warning.primary.location.start, program_start);
    assert_eq!(
        warning.primary.location.end,
        program_start + "one = two".len()
    );
}

#[rstest]
#[spec_test(REQ_DS_analyzer_004)]
#[case::same_type("g := one = one;")]
#[case::same_type_ordered("g := one < one;")]
#[case::same_type_function("g := EQ(one, one);")]
#[case::same_type_min("n := MIN(one, one);")]
#[case::value_of_the_same_type("g := one = B;")]
#[case::integer_literal("g := one = 1;")]
#[case::integer_literal_on_the_left("g := 1 < two;")]
#[case::integer_variable("g := one = n;")]
#[case::integer_in_a_function_form("g := GT(two, 1);")]
#[case::integer_in_max("n := MAX(one, 3);")]
#[case::alias_of_the_enumeration("g := one = three;")]
#[case::alias_in_a_function_form("g := NE(three, one);")]
#[case::alias_in_min("n := MIN(three, one);")]
#[case::reference_to_an_enumeration("g := to_one = two;")]
#[case::limit("n := LIMIT(one, two, one);")]
#[case::select("n := SEL(g, one, two);")]
#[case::multiplex("n := MUX(1, one, two);")]
#[case::logical_operator_of_flags("g := g AND g;")]
fn apply_when_no_two_enumerations_compared_then_no_warning(
    #[case] body: &str,
    #[values(Strict, NotStrict)] first: Strictness,
    #[values(Strict, NotStrict)] second: Strictness,
) {
    assert_eq!(codes(first, second, body), Vec::<String>::new());
}

#[rstest]
#[case::enumeration_alias_first("g := three = two;")]
#[case::enumeration_alias_last("g := two = three;")]
#[case::alias_in_max("n := MAX(three, two);")]
#[spec_test(REQ_DS_analyzer_003)]
fn apply_when_alias_of_one_enumeration_compared_with_another_then_p4083(#[case] body: &str) {
    let found = diagnostics(Strict, Strict, body);

    assert_eq!(
        found.iter().map(|d| d.code.as_str()).collect::<Vec<_>>(),
        vec![P4083]
    );
    // The type is named as the operand is declared: the alias, not E1.
    assert!(found[0].primary.message.contains("E3"), "{:?}", found[0]);
}

#[test]
#[spec_test(REQ_DS_analyzer_003)]
fn apply_when_alias_of_an_enumeration_compared_with_the_enumeration_then_no_warning() {
    for body in [
        "g := one = three;",
        "g := three <> one;",
        "n := MIN(one, three);",
    ] {
        assert_eq!(codes(Strict, Strict, body), Vec::<String>::new(), "{body}");
    }
}

/// The codes of the whole analysis of `body`, under the options of CODESYS.
fn analysis_codes(first: Strictness, second: Strictness, body: &str) -> Vec<String> {
    let options = CompilerOptions::from_dialect(Dialect::Codesys);
    let library =
        parse_program(&program(first, second, body), &FileId::default(), &options).unwrap();
    let (_, context) = crate::stages::analyze(&[&library], &options).unwrap();
    let mut codes: Vec<String> = context
        .diagnostics()
        .iter()
        .map(|d| d.code.clone())
        .collect();
    codes.sort();
    codes
}

#[test]
#[spec_test(REQ_DS_analyzer_005)]
fn analyze_when_enumerations_compared_then_the_warning_and_no_error_for_the_comparison() {
    for first in [Strict, NotStrict] {
        for second in [Strict, NotStrict] {
            assert_eq!(
                analysis_codes(first, second, "g := one = two;"),
                vec![P4083]
            );
        }
    }
}

#[test]
#[spec_test(REQ_DS_analyzer_005)]
fn analyze_when_enumerations_compared_beside_an_error_of_another_cause_then_both_reported() {
    // `one := n` stores a number into a strict enumeration (P4035); the
    // comparison is a cause of its own.
    assert_eq!(
        analysis_codes(Strict, Strict, "one := n; g := one = two;"),
        vec!["P4035", P4083]
    );
}

#[test]
fn apply_when_two_comparisons_of_enumerations_then_one_warning_for_each() {
    let found = codes(Strict, Strict, "g := (one = two) OR (two <> one);");

    assert_eq!(found, vec![P4083, P4083]);
}

/// The codes of the analysis of `source` under the default options.
fn default_analysis_codes(source: &str) -> Vec<String> {
    let options = CompilerOptions::default();
    let library = parse_program(source, &FileId::default(), &options).unwrap();
    let (_, context) = crate::stages::analyze(&[&library], &options).unwrap();
    context
        .diagnostics()
        .iter()
        .map(|d| d.code.clone())
        .collect()
}

/// The two examples of the page of P4083.
#[test]
fn analyze_when_the_examples_of_the_documentation_then_the_warning_and_then_none() {
    let warned = "
TYPE
    TrafficLight : (Red, Yellow, Green);
    Mode : (Auto, Manual);
END_TYPE

PROGRAM main
VAR
    light : TrafficLight := Red;
    current : Mode := Auto;
    same : BOOL;
END_VAR
    same := light = current;
END_PROGRAM";
    let fixed = "
TYPE
    TrafficLight : (Red, Yellow, Green);
    Mode : (Auto, Manual);
END_TYPE

PROGRAM main
VAR
    light : TrafficLight := Red;
    current : Mode := Auto;
    light_number : DINT;
    mode_number : DINT;
    same : BOOL;
END_VAR
    light_number := light;
    mode_number := current;
    same := light_number = mode_number;
END_PROGRAM";

    assert_eq!(default_analysis_codes(warned), vec![P4083]);
    assert_eq!(default_analysis_codes(fixed), Vec::<String>::new());
}
