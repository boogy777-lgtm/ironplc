//! Tests of the faults: the kinds of declaration a row allows, the values it
//! reads, the attributes written twice and the attributes the compiler does
//! not know.

use rstest::rstest;
use spec_test_macro::spec_test;

use super::*;
use crate::attribute::registry::test_support::{
    attribute, attributes, cases, invalid_values, place, spellings, valid_value,
};
use crate::attribute::registry::{Family, State, ALL, NAMING};

/// The faults of one attribute written before a declaration of `kind`.
fn faults_of(kind: TargetKind, name: &str, value: Option<&str>) -> Vec<Fault> {
    faults(kind, &attributes(vec![attribute(name, value, 3)]))
}

#[spec_test(REQ_ATTR_dsl_030)]
fn faults_when_every_row_and_kind_then_not_allowed_here_exactly_where_the_row_does_not_stand() {
    let wrong: Vec<String> = ALL
        .iter()
        .flat_map(|row| {
            TargetKind::ALL.iter().flat_map(move |kind| {
                let found = faults_of(*kind, row.name, valid_value(row.value));
                let allowed = row.targets.contains(*kind);
                let right = match found.as_slice() {
                    [] => allowed,
                    [Fault {
                        kind:
                            FaultKind::NotAllowedHere {
                                row: named,
                                kind: at,
                            },
                        span,
                        ..
                    }] => !allowed && *named == row && at == kind && place(span) == (3, 4),
                    _ => false,
                };
                (!right).then(|| format!("{} before {:?}: {found:?}", row.name, kind))
            })
        })
        .collect();

    assert!(wrong.is_empty(), "{wrong:?}");
}

#[spec_test(REQ_ATTR_dsl_030)]
fn faults_when_name_or_alias_in_any_case_then_the_attribute_is_found_as_its_row() {
    let mut wrong = Vec::new();
    for row in ALL {
        for spelling in spellings(row).into_iter().flat_map(cases) {
            for kind in TargetKind::ALL {
                let found = faults_of(*kind, &spelling, valid_value(row.value));
                let expected = usize::from(!row.targets.contains(*kind));
                if found.len() != expected {
                    wrong.push(format!("{spelling} before {kind:?}: {found:?}"));
                }
            }
        }
    }

    assert!(wrong.is_empty(), "{wrong:?}");
}

#[spec_test(REQ_ATTR_dsl_031)]
fn faults_when_every_row_and_invalid_value_then_value_where_the_row_stands() {
    let wrong: Vec<String> = ALL
        .iter()
        .flat_map(|row| {
            invalid_values(row.value).iter().flat_map(move |value| {
                row.targets.kinds().filter_map(move |kind| {
                    let found = faults_of(kind, row.name, *value);
                    let right = matches!(
                        found.as_slice(),
                        [Fault { kind: FaultKind::Value { row: named, found: seen }, .. }]
                            if *named == row && seen.as_deref() == *value
                    );
                    (!right)
                        .then(|| format!("{} := {value:?} before {kind:?}: {found:?}", row.name))
                })
            })
        })
        .collect();

    assert!(wrong.is_empty(), "{wrong:?}");
}

#[spec_test(REQ_ATTR_dsl_031)]
fn faults_when_every_row_and_valid_value_then_none_where_the_row_stands() {
    let wrong: Vec<String> = ALL
        .iter()
        .flat_map(|row| {
            row.targets.kinds().filter_map(move |kind| {
                let found = faults_of(kind, row.name, valid_value(row.value));
                (!found.is_empty()).then(|| format!("{} before {kind:?}: {found:?}", row.name))
            })
        })
        .collect();

    assert!(wrong.is_empty(), "{wrong:?}");
}

#[spec_test(REQ_ATTR_dsl_031)]
fn faults_when_value_is_wrong_and_the_row_does_not_stand_there_then_only_not_allowed_here() {
    let found = faults_of(TargetKind::Variable, "pack_mode", Some("3"));

    assert_eq!(found.len(), 1);
    assert!(matches!(found[0].kind, FaultKind::NotAllowedHere { .. }));
}

#[rstest]
#[case::flag_with_value("hide", Some("yes"))]
#[case::value_missing("pack_mode", None)]
#[case::value_not_listed("pack_mode", Some("3"))]
#[case::not_an_integer("estimated-stack-usage", Some("big"))]
#[case::empty_text("obsolete", Some(""))]
#[spec_test(REQ_ATTR_dsl_031)]
fn faults_when_value_is_wrong_then_value_with_what_was_found(
    #[case] name: &str,
    #[case] value: Option<&str>,
) {
    let kind = find(name)
        .and_then(|row| row.targets.kinds().next())
        .expect("the row stands before a kind");

    let found = faults_of(kind, name, value);

    assert_eq!(found.len(), 1);
    assert_eq!(found[0].name, name);
    assert!(
        matches!(&found[0].kind, FaultKind::Value { found: seen, .. } if seen.as_deref() == value)
    );
}

#[rstest]
#[case::same_name(TargetKind::Variable, ("hide", None), ("hide", None))]
#[case::other_case(TargetKind::Variable, ("hide", None), ("HIDE", None))]
#[case::alias_and_name(TargetKind::Variable, ("noinit", None), ("no-init", None))]
#[case::two_aliases(TargetKind::Member, ("no_init", None), ("NO-INIT", None))]
#[case::value_differs(TargetKind::Structure, ("pack_mode", Some("2")), ("pack_mode", Some("4")))]
#[case::user_defined(TargetKind::Program, ("my_tag", Some("a")), ("my_tag", Some("b")))]
#[case::user_defined_other_case(TargetKind::Program, ("my_tag", None), ("My_Tag", None))]
#[spec_test(REQ_ATTR_dsl_032)]
fn faults_when_same_attribute_twice_then_repeated_at_the_second_with_the_span_of_the_first(
    #[case] kind: TargetKind,
    #[case] first: (&str, Option<&str>),
    #[case] second: (&str, Option<&str>),
) {
    let found = faults(
        kind,
        &attributes(vec![
            attribute(first.0, first.1, 10),
            attribute(second.0, second.1, 20),
        ]),
    );

    let places: Vec<(usize, usize)> = found.iter().map(|fault| place(&fault.span)).collect();
    assert_eq!(places, vec![(20, 21)]);
    assert!(matches!(&found[0].kind, FaultKind::Repeated { first } if place(first) == (10, 11)));
    assert_eq!(found[0].name, second.0);
}

#[spec_test(REQ_ATTR_dsl_032)]
fn faults_when_attribute_of_a_group_and_the_same_of_the_declaration_then_repeated_at_the_own() {
    // The attributes before a group come first, then the declaration's own.
    let found = faults(
        TargetKind::Variable,
        &attributes(vec![
            attribute("hide", None, 1),
            attribute("other", None, 2),
            attribute("hide", None, 3),
        ]),
    );

    assert_eq!(found.len(), 1);
    assert_eq!(place(&found[0].span), (3, 4));
    assert!(matches!(&found[0].kind, FaultKind::Repeated { first } if place(first) == (1, 2)));
}

#[spec_test(REQ_ATTR_dsl_032)]
fn faults_when_attribute_written_three_times_then_each_later_one_is_repeated_of_the_first() {
    let found = faults(
        TargetKind::Variable,
        &attributes(vec![
            attribute("hide", None, 1),
            attribute("hide", None, 2),
            attribute("hide", None, 3),
        ]),
    );

    let firsts: Vec<(usize, usize)> = found
        .iter()
        .filter_map(|fault| match &fault.kind {
            FaultKind::Repeated { first } => Some(place(first)),
            FaultKind::NotAllowedHere { .. }
            | FaultKind::Value { .. }
            | FaultKind::ForbidsBlock { .. } => None,
        })
        .collect();
    assert_eq!(firsts, vec![(1, 2), (1, 2)]);
}

#[spec_test(REQ_ATTR_dsl_032)]
fn faults_when_repeated_attribute_is_also_wrong_then_only_repeated() {
    let found = faults(
        TargetKind::Variable,
        &attributes(vec![
            attribute("hide", None, 1),
            attribute("hide", Some("yes"), 2),
            attribute("pack_mode", Some("3"), 3),
            attribute("pack_mode", Some("3"), 4),
        ]),
    );

    let summary: Vec<(usize, &str)> = found
        .iter()
        .map(|fault| {
            let what = match &fault.kind {
                FaultKind::NotAllowedHere { .. } => "not allowed here",
                FaultKind::Value { .. } => "value",
                FaultKind::Repeated { .. } => "repeated",
                FaultKind::ForbidsBlock { .. } => "forbids block",
            };
            (fault.span.start, what)
        })
        .collect();
    assert_eq!(
        summary,
        vec![(2, "repeated"), (3, "not allowed here"), (4, "repeated")]
    );
}

#[spec_test(REQ_ATTR_dsl_032)]
fn faults_when_different_attributes_then_none_repeated() {
    let found = faults(
        TargetKind::Variable,
        &attributes(vec![
            attribute("hide", None, 1),
            attribute("noinit", None, 2),
            attribute("my_tag", None, 3),
            attribute("my_other_tag", None, 4),
        ]),
    );

    assert!(found.is_empty(), "{found:?}");
}

#[rstest]
#[case::no_value(None)]
#[case::value(Some("x"))]
#[case::empty_value(Some(""))]
#[spec_test(REQ_ATTR_dsl_033)]
fn faults_when_attribute_is_user_defined_then_none_before_any_kind(#[case] value: Option<&str>) {
    let found: Vec<Fault> = TargetKind::ALL
        .iter()
        .flat_map(|kind| faults_of(*kind, "my_company_tag", value))
        .collect();

    assert!(found.is_empty(), "{found:?}");
}

#[spec_test(REQ_ATTR_dsl_033)]
fn faults_when_no_attributes_then_none() {
    assert!(faults(TargetKind::Program, &attributes(vec![])).is_empty());
}

/// A declaration of `kind` with the attribute at 3 and variables of the blocks
/// named, the one at index `i` at `10 + i`.
fn declaration(
    kind: TargetKind,
    name: &str,
    value: Option<&str>,
    blocks: &[VariableType],
) -> Declared {
    Declared {
        kind,
        name: "d".to_string(),
        attributes: attributes(vec![attribute(name, value, 3)]),
        holds: blocks
            .iter()
            .enumerate()
            .map(|(index, block)| Held {
                block: block.clone(),
                span: SourceSpan::range(10 + index, 11 + index),
            })
            .collect(),
    }
}

#[spec_test(REQ_ATTR_dsl_034)]
fn declared_faults_when_every_row_forbids_a_block_then_a_declaration_holding_it_has_the_fault_at_the_first_such_variable(
) {
    let mut checked = 0;
    let mut wrong = Vec::new();
    for row in ALL.iter().filter(|row| !row.forbids.is_empty()) {
        for kind in TargetKind::ALL.iter().filter(|k| row.targets.contains(**k)) {
            for block in row.forbids {
                checked += 1;
                let held = [VariableType::Var, block.clone(), block.clone()];
                let found =
                    declared_faults(&declaration(*kind, row.name, valid_value(row.value), &held));
                let right = matches!(
                    found.as_slice(),
                    [Fault {
                        kind: FaultKind::ForbidsBlock { row: named, block: forbidden, attribute },
                        span,
                        ..
                    }] if *named == row
                        && forbidden == block
                        && place(attribute) == (3, 4)
                        && place(span) == (11, 12)
                );
                if !right {
                    wrong.push(format!(
                        "{} in {kind:?} with {block:?}: {found:?}",
                        row.name
                    ));
                }
            }
        }
    }

    assert!(checked > 0, "no row forbids a block");
    assert!(wrong.is_empty(), "{wrong:?}");
}

#[spec_test(REQ_ATTR_dsl_034)]
fn declared_faults_when_declaration_holds_no_forbidden_block_then_no_fault() {
    for row in ALL.iter().filter(|row| !row.forbids.is_empty()) {
        for kind in TargetKind::ALL.iter().filter(|k| row.targets.contains(**k)) {
            let found = declared_faults(&declaration(
                *kind,
                row.name,
                valid_value(row.value),
                &[VariableType::Var, VariableType::Input],
            ));

            assert!(found.is_empty(), "{} in {kind:?}: {found:?}", row.name);
        }
    }
}

#[spec_test(REQ_ATTR_dsl_034)]
fn declared_faults_when_attribute_does_not_stand_before_the_kind_then_only_not_allowed_here() {
    // `subsequent` stands before a program and a global variable list; before
    // a function it is not allowed, and it forbids nothing there.
    let found = declared_faults(&declaration(
        TargetKind::Function,
        "subsequent",
        None,
        &[VariableType::VarTemp],
    ));

    assert!(
        matches!(
            found.as_slice(),
            [Fault {
                kind: FaultKind::NotAllowedHere { .. },
                ..
            }]
        ),
        "{found:?}"
    );
}

#[spec_test(REQ_ATTR_dsl_034)]
fn declared_faults_when_forbidding_attribute_is_written_twice_then_one_forbids_block_fault() {
    let mut declared = declaration(
        TargetKind::Program,
        "subsequent",
        None,
        &[VariableType::VarTemp],
    );
    declared.attributes = attributes(vec![
        attribute("subsequent", None, 1),
        attribute("SUBSEQUENT", None, 2),
    ]);

    let found = declared_faults(&declared);

    let blocks: Vec<&FaultKind> = found
        .iter()
        .map(|fault| &fault.kind)
        .filter(|kind| matches!(kind, FaultKind::ForbidsBlock { .. }))
        .collect();
    assert_eq!(blocks.len(), 1, "{found:?}");
    assert!(matches!(
        blocks[0],
        FaultKind::ForbidsBlock { attribute, .. } if place(attribute) == (1, 2)
    ));
}

#[spec_test(REQ_ATTR_dsl_050)]
fn region_when_registry_then_only_naming_marks_a_region_and_it_reads_off_on_and_omit() {
    let regions: Vec<&str> = ALL
        .iter()
        .filter(|row| row.region)
        .map(|row| row.name)
        .collect();

    assert_eq!(regions, ["naming"]);
    for value in ["off", "on", "omit", "OFF", " On "] {
        assert!(NAMING.info().accepts(Some(value)), "{value}");
    }
    for value in [None, Some(""), Some("maybe")] {
        assert!(!NAMING.info().accepts(value), "{value:?}");
    }
    assert_eq!(NAMING.info().state, State::Waiting);
    assert_eq!(NAMING.info().family, Family::StaticAnalysis);
}

#[spec_test(REQ_ATTR_dsl_051)]
fn region_fault_when_region_attribute_has_an_unread_value_then_the_value_fault_at_its_span() {
    let fault = region_fault(&attribute("Naming", Some("maybe"), 7));

    assert!(matches!(
        fault,
        Some(Fault {
            span: _,
            kind: FaultKind::Value { found: Some(_), .. },
            ..
        })
    ));
    assert_eq!(fault.map(|fault| place(&fault.span)), Some((7, 8)));
}

#[rstest]
#[case::valid_value("naming", Some("off"))]
#[case::not_a_region_attribute("pack_mode", Some("not-a-choice"))]
#[case::user_defined("my_own", Some("anything"))]
#[spec_test(REQ_ATTR_dsl_051)]
fn region_fault_when_no_fault_of_a_region_then_none(
    #[case] name: &str,
    #[case] value: Option<&str>,
) {
    assert!(region_fault(&attribute(name, value, 3)).is_none());
}

#[spec_test(REQ_ATTR_dsl_051)]
fn diagnostic_when_value_fault_then_p4077_naming_the_value_and_what_the_row_takes() {
    let fault = region_fault(&attribute("naming", Some("maybe"), 3));
    let diagnostic = fault.map(Fault::diagnostic);

    assert_eq!(diagnostic.as_ref().map(|d| d.code.as_str()), Some("P4077"));
    assert_eq!(
        diagnostic.map(|d| d.primary.message),
        Some(
            "The attribute 'naming' takes one of off, on, omit, but its value is 'maybe'"
                .to_string()
        )
    );
}
