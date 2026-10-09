//! Tests that the suggestion of a name does not depend on the order a
//! container yields its entries.

use crate::semantic_context::SemanticContextBuilder;
use crate::test_helpers::parse_and_resolve_types;
use ironplc_parser::options::CompilerOptions;

use super::*;

/// The suggestions made for `abd` over several runs, which each make a
/// table of their own: a suggestion that depended on the order of a hash
/// container would differ between runs of one process.
fn suggestions_for_abd(declarations: &str) -> Vec<String> {
    let program = format!("PROGRAM main VAR {declarations} END_VAR abd := 1; END_PROGRAM");
    let library = parse_and_resolve_types(&program);
    let context = SemanticContextBuilder::new().build().unwrap();
    let mut suggestions: Vec<String> = (0..24)
        .map(|_| {
            let errors = apply(&library, &context, &CompilerOptions::default()).unwrap_err();
            errors[0]
                .described
                .iter()
                .find(|d| d.starts_with("did you mean"))
                .cloned()
                .unwrap_or_default()
        })
        .collect();
    suggestions.sort();
    suggestions.dedup();
    suggestions
}

#[rstest::rstest]
#[case::first_declared_is_alphabetically_first("abc : INT; abe : INT;", "did you mean=abc")]
#[case::first_declared_is_alphabetically_last("abe : INT; abc : INT;", "did you mean=abe")]
#[case::three_of_equal_distance("abx : INT; abe : INT; abc : INT;", "did you mean=abx")]
#[case::closer_candidate_wins("abxy : INT; abc : INT;", "did you mean=abc")]
fn apply_when_candidates_of_equal_distance_then_suggests_the_one_declared_first(
    #[case] declarations: &str,
    #[case] expected: &str,
) {
    assert_eq!(suggestions_for_abd(declarations), vec![expected.to_owned()]);
}
