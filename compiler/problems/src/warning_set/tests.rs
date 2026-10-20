//! The set of warnings and the reading of a code written in a setting
//! (`REQ-DS-problems-012` to `-015`).

use spec_test_macro::spec_test;

use crate::{Problem, Severity, WarningProblem, WarningSet};

/// The only warning today: the comparison of two enumerations, P4083, whose
/// CODESYS counterpart is C0354. A test that needs another warning does not
/// exist yet; the ones below ask the table and not this name where they can.
const COMPARED: WarningProblem = WarningProblem::EnumerationTypesCompared;

#[spec_test(REQ_DS_problems_012)]
fn all_when_compared_with_the_problems_then_the_problems_that_declare_warning_in_csv_order() {
    let declared: Vec<Problem> = Problem::ALL
        .into_iter()
        .filter(|problem| problem.severity() == Severity::Warning)
        .collect();
    let listed: Vec<Problem> = WarningProblem::ALL
        .into_iter()
        .map(WarningProblem::problem)
        .collect();

    assert_eq!(listed, declared);
    assert!(!listed.is_empty());
}

#[spec_test(REQ_DS_problems_012)]
fn of_when_problem_then_a_warning_only_for_the_problems_that_declare_warning() {
    for problem in Problem::ALL {
        assert_eq!(
            WarningProblem::of(problem).is_some(),
            problem.severity() == Severity::Warning,
            "{}",
            problem.code()
        );
    }
    assert_eq!(WarningProblem::of(Problem::SyntaxError), None);
    assert_eq!(WarningProblem::of(Problem::VariableUndefined), None);
}

#[spec_test(REQ_DS_problems_012)]
fn index_when_each_warning_then_its_position_in_all() {
    for (position, warning) in WarningProblem::ALL.into_iter().enumerate() {
        assert_eq!(warning.index(), position);
    }
}

#[spec_test(REQ_DS_problems_012)]
fn from_code_when_code_of_a_warning_then_the_warning_and_when_of_an_error_then_none() {
    assert_eq!(WarningProblem::from_code("P4083"), Some(COMPARED));
    assert_eq!(WarningProblem::from_code("P4007"), None);
    assert_eq!(WarningProblem::from_code("p4083"), None);
}

#[test]
fn set_when_warnings_added_then_it_contains_exactly_those() {
    let empty = WarningSet::default();
    let one = empty.with(COMPARED);

    assert!(empty.is_empty());
    assert!(!empty.contains(COMPARED));
    assert!(!one.is_empty());
    assert!(one.contains(COMPARED));
    assert_eq!(one.iter().collect::<Vec<_>>(), vec![COMPARED]);
    assert_eq!(empty.iter().count(), 0);
}

#[test]
fn union_when_two_sets_then_the_warnings_of_both_and_each_unchanged() {
    let none = WarningSet::default();
    let one = WarningSet::default().with(COMPARED);

    assert_eq!(none.union(one), one);
    assert_eq!(one.union(none), one);
    assert_eq!(one.union(one), one);
    assert_eq!(none.union(none), none);
}

#[spec_test(REQ_DS_problems_014)]
fn from_code_when_ironplc_code_of_a_warning_then_that_warning() {
    let set = WarningSet::from_code("P4083").unwrap();

    assert_eq!(set.iter().collect::<Vec<_>>(), vec![COMPARED]);
}

#[spec_test(REQ_DS_problems_014)]
fn from_code_when_codesys_code_of_a_warning_then_every_warning_that_has_it() {
    let set = WarningSet::from_code("C0354").unwrap();

    let reporting: Vec<WarningProblem> = WarningProblem::ALL
        .into_iter()
        .filter(|warning| warning.problem().codesys() == Some("C0354"))
        .collect();
    assert!(!reporting.is_empty());
    assert_eq!(set.iter().collect::<Vec<_>>(), reporting);
    assert!(set.contains(COMPARED));
}

#[spec_test(REQ_DS_problems_014)]
fn from_code_when_each_warning_then_its_codes_give_a_set_that_holds_it() {
    for warning in WarningProblem::ALL {
        let problem = warning.problem();

        assert!(WarningSet::from_code(problem.code())
            .unwrap()
            .contains(warning));
        if let Some(codesys) = problem.codesys() {
            assert!(WarningSet::from_code(codesys).unwrap().contains(warning));
        }
    }
}

/// What the refusal of `code` says.
fn refusal(code: &str) -> (String, String) {
    let text = WarningSet::from_code(code).unwrap_err();
    (
        text.problem().code().to_string(),
        text.message().to_string(),
    )
}

#[spec_test(REQ_DS_problems_015)]
fn from_code_when_unknown_text_then_p0013_that_names_it_and_lists_the_accepted_codes() {
    for code in [
        "",
        "X",
        "P9",
        "P0000",
        "C0000",
        "P4083 ",
        " P4083",
        "4083",
        "Wrn_EnumComparison",
    ] {
        let (problem, message) = refusal(code);

        assert_eq!(problem, "P0013", "'{code}'");
        assert!(message.contains(&format!("'{code}'")), "{message}");
        assert!(message.contains("P4083, C0354"), "{message}");
    }
}

#[spec_test(REQ_DS_problems_015)]
fn from_code_when_other_case_then_unknown_because_case_is_exact() {
    for code in ["p4083", "c0354", "P4083x"] {
        let (problem, message) = refusal(code);

        assert_eq!(problem, "P0013", "'{code}'");
        assert!(message.contains("not the code of a warning"), "{message}");
        assert!(message.contains(&format!("'{code}'")), "{message}");
    }
}

#[spec_test(REQ_DS_problems_015)]
fn from_code_when_code_of_an_error_then_p0013_that_names_the_code_and_its_category() {
    // P4007 is an error, and its CODESYS counterpart C0046 reports an error too.
    for code in ["P4007", "C0046", "P0002"] {
        let (problem, message) = refusal(code);

        assert_eq!(problem, "P0013", "'{code}'");
        assert!(message.contains(&format!("'{code}'")), "{message}");
        assert!(message.contains("category Error"), "{message}");
        assert!(message.contains("only the code of a warning"), "{message}");
        assert!(message.contains("P4083, C0354"), "{message}");
    }
}

#[spec_test(REQ_DS_problems_015)]
fn from_code_when_code_of_a_compiler_located_error_then_refused_with_its_category() {
    // P9999 is declared `Error`: nothing in the CSV above `Warning` can be listed.
    let (problem, message) = refusal("P9999");

    assert_eq!(problem, "P0013");
    assert!(message.contains("category Error"), "{message}");
}

#[test]
fn display_when_text_then_the_code_and_the_message() {
    let text = WarningSet::from_code("X").unwrap_err();

    assert_eq!(text.to_string(), format!("P0013: {}", text.message()));
}
