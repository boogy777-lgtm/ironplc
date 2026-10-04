//! Tests of the charts: a row of `PARTS` and of `QUALIFIERS` for each thing a
//! chart may hold, and what each part makes of what it is written with.

use super::*;
use crate::lower::{lower_library, INTERNAL_ERROR};
use crate::{parse_source_file, ParseOptions};
use ironplc_dsl::common::{FunctionBlockBodyKind, LibraryElementKind};
use ironplc_dsl::core::FileId;
use ironplc_problems::Problem;

type TimedBuild = fn(ActionTimeKind) -> ActionQualifier;

fn file() -> FileId {
    FileId::from_string("t.st")
}

/// The body of the program `source` is written as, lowered with every keyword
/// enabled.
fn lowered_body(source: &str) -> Result<FunctionBlockBodyKind, String> {
    let parse = parse_source_file(source, &ParseOptions::all());
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    let library = lower_library(&parse, &file()).map_err(|error| error.code)?;
    let body = match library.elements.into_iter().next() {
        Some(LibraryElementKind::ProgramDeclaration(program)) => Some(program.body),
        Some(LibraryElementKind::FunctionBlockDeclaration(block)) => Some(block.body),
        _ => None,
    };
    Ok(body.expect("a program or a function block"))
}

/// The networks of the chart a program is written with.
fn networks(source: &str) -> Vec<Network> {
    match lowered_body(&format!("PROGRAM p {source} END_PROGRAM")) {
        Ok(FunctionBlockBodyKind::Sfc(chart)) => Some(chart.networks),
        _ => None,
    }
    .expect("a chart")
}

/// The action at `at` among the elements of the first network.
fn action_at(chart: &[Network], at: usize) -> Action {
    match &chart[0].elements[at] {
        ElementKind::Action(action) => Some(action.clone()),
        _ => None,
    }
    .expect("an action")
}

/// The error code of the chart a program is written with.
fn error_code(source: &str) -> String {
    lowered_body(&format!("PROGRAM p {source} END_PROGRAM"))
        .err()
        .unwrap_or_default()
}

fn first_step(source: &str) -> Step {
    networks(source).remove(0).initial_step
}

fn first_transition(source: &str) -> Transition {
    let wrapped = format!("INITIAL_STEP s : END_STEP {source}");
    networks(&wrapped)
        .remove(0)
        .elements
        .into_iter()
        .find_map(|element| match element {
            ElementKind::Transition(transition) => Some(transition),
            _ => None,
        })
        .expect("a transition")
}

fn duration(text: &str) -> ActionTimeKind {
    let parse = parse_source_file(
        &format!("PROGRAM p INITIAL_STEP s : a(SD, {text}); END_STEP END_PROGRAM"),
        &ParseOptions::all(),
    );
    let node = parse
        .root
        .descendants()
        .find(|node| node.kind() == K::DurationLiteral)
        .expect("a duration");
    match lower_constant(&LowerCx::new(file()), &node) {
        Ok(ConstantKind::Duration(duration)) => Some(ActionTimeKind::Duration(duration)),
        _ => None,
    }
    .expect("a duration")
}

// ---- Networks.

#[test]
fn lower_chart_when_one_initial_step_then_one_network_with_no_elements() {
    let chart = networks("INITIAL_STEP s : END_STEP");
    assert_eq!(chart.len(), 1);
    assert_eq!(chart[0].initial_step.name, Id::from("s"));
    assert!(chart[0].elements.is_empty());
}

#[test]
fn lower_chart_when_each_initial_step_then_a_network_each_and_elements_stay_with_their_own() {
    let chart = networks(
        "INITIAL_STEP a : END_STEP STEP b : END_STEP
         INITIAL_STEP c : END_STEP ACTION x : END_ACTION STEP d : END_STEP",
    );
    assert_eq!(chart.len(), 2);
    assert_eq!(chart[0].initial_step.name, Id::from("a"));
    assert_eq!(chart[0].elements.len(), 1);
    assert_eq!(chart[1].initial_step.name, Id::from("c"));
    let kinds: Vec<&str> = chart[1]
        .elements
        .iter()
        .map(|element| match element {
            ElementKind::Step(_) => "step",
            ElementKind::Transition(_) => "transition",
            ElementKind::Action(_) => "action",
        })
        .collect();
    assert_eq!(kinds, ["action", "step"]);
}

#[test]
fn lower_chart_when_elements_of_every_kind_then_in_the_order_written() {
    let chart = networks(
        "INITIAL_STEP a : END_STEP
         TRANSITION FROM a TO b := TRUE; END_TRANSITION
         STEP b : END_STEP
         ACTION x : END_ACTION",
    );
    let kinds: Vec<&str> = chart[0]
        .elements
        .iter()
        .map(|element| match element {
            ElementKind::Step(_) => "step",
            ElementKind::Transition(_) => "transition",
            ElementKind::Action(_) => "action",
        })
        .collect();
    assert_eq!(kinds, ["transition", "step", "action"]);
}

#[test]
fn lower_chart_when_a_node_that_is_no_part_of_a_chart_then_internal_error() {
    let parse = parse_source_file(
        "PROGRAM p INITIAL_STEP s : END_STEP END_PROGRAM",
        &ParseOptions::all(),
    );
    let program = parse.root.first_child().expect("a program");
    let error = lower_chart(&LowerCx::new(file()), &program).expect_err("a program is no chart");
    assert_eq!(error.code, INTERNAL_ERROR);
}

#[test]
fn lower_chart_when_an_element_has_no_initial_step_before_it_then_internal_error() {
    // The grammar starts a chart at its initial step, so this is not a tree it
    // builds.
    let step = rowan::GreenNode::new(rowan::SyntaxKind(K::StepDecl as u16), std::iter::empty());
    let green = rowan::GreenNode::new(
        rowan::SyntaxKind(K::SfcBody as u16),
        [rowan::NodeOrToken::Node(step)],
    );
    let error = lower_chart(&LowerCx::new(file()), &SyntaxNode::new_root(green))
        .expect_err("no initial step");
    assert_eq!(error.code, INTERNAL_ERROR);
}

// ---- Steps.

#[test]
fn lower_step_when_associations_then_names_qualifiers_and_indicators_in_order() {
    let step = first_step("INITIAL_STEP s : a(N); b(S, lit, other); c(); d(, only); END_STEP");
    let written: Vec<(String, Option<ActionQualifier>, Vec<String>)> = step
        .action_associations
        .iter()
        .map(|association| {
            (
                association.name.original().to_string(),
                association.qualifier.clone(),
                association
                    .indicators
                    .iter()
                    .map(|indicator| indicator.original().to_string())
                    .collect(),
            )
        })
        .collect();
    assert_eq!(
        written,
        vec![
            ("a".to_string(), Some(ActionQualifier::N), vec![]),
            (
                "b".to_string(),
                Some(ActionQualifier::S),
                vec!["lit".to_string(), "other".to_string()]
            ),
            ("c".to_string(), None, vec![]),
            ("d".to_string(), None, vec!["only".to_string()]),
        ]
    );
}

#[test]
fn lower_step_when_no_association_then_an_empty_list_and_the_name_is_positioned() {
    let step = first_step("INITIAL_STEP  start : END_STEP");
    assert!(step.action_associations.is_empty());
    assert_eq!(step.name.original(), "start");
    assert_eq!((step.name.span.start, step.name.span.end), (24, 29));
    assert_eq!(step.name.span.file_id, file());
}

#[test]
fn lower_step_when_an_association_names_an_action_then_the_name_is_positioned_at_the_name() {
    let step = first_step("INITIAL_STEP s : act(N, ind); END_STEP");
    let association = &step.action_associations[0];
    assert_eq!(
        (association.name.span.start, association.name.span.end),
        (27, 30)
    );
    assert_eq!(
        (
            association.indicators[0].span.start,
            association.indicators[0].span.end
        ),
        (34, 37)
    );
}

// ---- Qualifiers.

#[test]
fn lower_qualifier_when_each_word_then_its_row() {
    let plain = [
        ("N", ActionQualifier::N),
        ("R", ActionQualifier::R),
        ("S", ActionQualifier::S),
        ("L", ActionQualifier::L),
        ("D", ActionQualifier::D),
        ("P", ActionQualifier::P),
    ];
    for (word, expected) in plain {
        let step = first_step(&format!("INITIAL_STEP s : a({word}); END_STEP"));
        assert_eq!(
            step.action_associations[0].qualifier,
            Some(expected),
            "{word}"
        );
    }
    let timed: [(&str, TimedBuild); 5] = [
        ("SD", ActionQualifier::SD),
        ("DS", ActionQualifier::DS),
        ("SL", ActionQualifier::SL),
        ("P1", ActionQualifier::PR),
        ("P0", ActionQualifier::PF),
    ];
    for (word, build) in timed {
        let step = first_step(&format!("INITIAL_STEP s : a({word}, T#1s); END_STEP"));
        assert_eq!(
            step.action_associations[0].qualifier,
            Some(build(duration("T#1s"))),
            "{word}"
        );
    }
}

#[test]
fn qualifiers_when_the_table_then_one_row_for_each_word_the_grammar_reads_and_no_other() {
    let words: Vec<&str> = QUALIFIERS.iter().map(|(word, _)| *word).collect();
    assert_eq!(
        words,
        ["N", "R", "S", "L", "D", "P", "SD", "DS", "SL", "P1", "P0"]
    );
    let timed = QUALIFIERS
        .iter()
        .filter(|(_, form)| matches!(form, Form::Timed(_)))
        .count();
    assert_eq!(timed, 5);
}

#[test]
fn lower_qualifier_when_the_time_is_a_variable_then_its_name() {
    let step = first_step("INITIAL_STEP s : a(SL, limit); END_STEP");
    assert_eq!(
        step.action_associations[0].qualifier,
        Some(ActionQualifier::SL(ActionTimeKind::VariableName(Id::from(
            "limit"
        ))))
    );
}

#[test]
fn lower_qualifier_when_the_duration_is_written_then_it_is_the_duration_literal() {
    let step = first_step("INITIAL_STEP s : a(SD, T#2s500ms); END_STEP");
    let literal = match step.action_associations[0].qualifier.clone() {
        Some(ActionQualifier::SD(ActionTimeKind::Duration(literal))) => Some(literal),
        _ => None,
    }
    .expect("a stored and delayed action with a duration");
    assert_eq!(
        ActionTimeKind::Duration(literal.clone()),
        duration("T#2s500ms")
    );
    assert_eq!((literal.span.start, literal.span.end), (33, 42));
}

#[test]
fn lower_qualifier_when_the_time_is_neither_a_duration_nor_a_name_then_syntax_error() {
    for time in ["1 + 2", "5", "f(x)", "'text'"] {
        assert_eq!(
            error_code(&format!("INITIAL_STEP s : a(SD, {time}); END_STEP")),
            Problem::SyntaxError.code(),
            "{time}"
        );
    }
}

#[test]
fn lower_qualifier_when_the_node_names_no_known_word_then_internal_error() {
    // The grammar only makes a qualifier of the words of the table.
    let parse = parse_source_file(
        "PROGRAM p INITIAL_STEP s : a(N); END_STEP END_PROGRAM",
        &ParseOptions::all(),
    );
    let qualifier = parse
        .root
        .descendants()
        .find(|node| node.kind() == K::ActionQualifier)
        .expect("a qualifier");
    assert!(lower_qualifier(&LowerCx::new(file()), &qualifier).is_ok());
    let green = rowan::GreenNode::new(
        rowan::SyntaxKind(K::ActionQualifier as u16),
        std::iter::empty(),
    );
    let error =
        lower_qualifier(&LowerCx::new(file()), &SyntaxNode::new_root(green)).expect_err("no word");
    assert_eq!(error.code, INTERNAL_ERROR);
}

// ---- Transitions.

#[test]
fn lower_transition_when_every_part_then_name_priority_steps_and_condition() {
    let transition =
        first_transition("TRANSITION go (PRIORITY := 3) FROM a TO b := x AND y; END_TRANSITION");
    assert_eq!(transition.name, Some(Id::from("go")));
    assert_eq!(transition.priority, Some(3));
    assert_eq!(transition.from, vec![Id::from("a")]);
    assert_eq!(transition.to, vec![Id::from("b")]);
    assert!(matches!(
        &transition.condition.kind,
        ironplc_dsl::textual::ExprKind::Compare(_)
    ));
}

#[test]
fn lower_transition_when_no_name_and_no_priority_then_none_of_either() {
    let transition = first_transition("TRANSITION FROM a TO b := TRUE; END_TRANSITION");
    assert_eq!(transition.name, None);
    assert_eq!(transition.priority, None);
}

#[test]
fn lower_transition_when_a_list_of_steps_then_every_step_written_is_kept() {
    // The legacy grammar keeps the first two of a list; the rest are steps the
    // transition leaves or enters as well.
    let transition =
        first_transition("TRANSITION FROM (a, b, c, d) TO (e, f, g) := TRUE; END_TRANSITION");
    let names = |steps: &[Id]| -> Vec<String> {
        steps
            .iter()
            .map(|step| step.original().to_string())
            .collect()
    };
    assert_eq!(names(&transition.from), ["a", "b", "c", "d"]);
    assert_eq!(names(&transition.to), ["e", "f", "g"]);
}

#[test]
fn lower_transition_when_two_steps_then_the_same_as_the_legacy_grammar_keeps() {
    let transition = first_transition("TRANSITION FROM (a, b) TO c := TRUE; END_TRANSITION");
    assert_eq!(transition.from, vec![Id::from("a"), Id::from("b")]);
}

#[test]
fn lower_transition_when_priority_is_beyond_32_bits_then_syntax_error_and_when_at_the_limit_then_kept(
) {
    assert_eq!(
        error_code(
            "INITIAL_STEP s : END_STEP TRANSITION (PRIORITY := 4294967296) FROM s TO s := TRUE; END_TRANSITION"
        ),
        Problem::SyntaxError.code()
    );
    let transition =
        first_transition("TRANSITION (PRIORITY := 4294967295) FROM s TO s := TRUE; END_TRANSITION");
    assert_eq!(transition.priority, Some(u32::MAX));
}

#[test]
fn lower_transition_when_a_part_is_missing_from_the_tree_then_internal_error() {
    let green = rowan::GreenNode::new(
        rowan::SyntaxKind(K::TransitionDecl as u16),
        std::iter::empty(),
    );
    let error = lower_transition(&LowerCx::new(file()), &SyntaxNode::new_root(green))
        .expect_err("no steps");
    assert_eq!(error.code, INTERNAL_ERROR);
}

#[test]
fn lower_transition_when_the_condition_is_written_then_it_is_positioned_at_the_expression() {
    let transition = first_transition("TRANSITION FROM a TO b := go; END_TRANSITION");
    assert_eq!(
        (
            transition.condition.span.start,
            transition.condition.span.end
        ),
        (62, 64)
    );
}

// ---- Actions.

#[test]
fn lower_action_when_statements_then_a_statement_body() {
    let chart = networks("INITIAL_STEP s : END_STEP ACTION a : x := 1; y := 2; END_ACTION");
    let action = action_at(&chart, 0);
    assert_eq!(action.name, Id::from("a"));
    assert!(
        matches!(&action.body, FunctionBlockBodyKind::Statements(list) if list.body.len() == 2)
    );
}

#[test]
fn lower_action_when_nothing_is_written_then_an_empty_body_as_for_a_unit_that_has_none() {
    let chart = networks("INITIAL_STEP s : END_STEP ACTION a : END_ACTION");
    let action = action_at(&chart, 0);
    assert_eq!(action.body, FunctionBlockBodyKind::Empty);
}

#[test]
fn lower_action_when_a_chart_is_written_then_a_chart_body_that_nests() {
    let chart = networks(
        "INITIAL_STEP s : END_STEP
         ACTION a : INITIAL_STEP t : END_STEP ACTION b : x := 1; END_ACTION END_ACTION",
    );
    let action = action_at(&chart, 0);
    let inner = match &action.body {
        FunctionBlockBodyKind::Sfc(inner) => Some(inner),
        _ => None,
    }
    .expect("a chart body");
    assert_eq!(inner.networks[0].initial_step.name, Id::from("t"));
    assert!(matches!(
        &inner.networks[0].elements[0],
        ElementKind::Action(nested) if nested.name == Id::from("b")
    ));
}

// ---- Whole charts.

#[test]
fn lower_chart_when_a_whole_chart_then_equal_to_the_one_built_by_hand() {
    let written = networks(
        "INITIAL_STEP idle : light(N); END_STEP
         TRANSITION start (PRIORITY := 1) FROM idle TO run := go; END_TRANSITION
         STEP run : light(N, lit); pulse(SD, T#2s); END_STEP
         ACTION light : lit := src; END_ACTION",
    );
    let expected = vec![Network {
        initial_step: Step {
            name: Id::from("idle"),
            action_associations: vec![ActionAssociation::new("light", Some(ActionQualifier::N))],
        },
        elements: vec![
            ElementKind::Transition(Transition {
                name: Some(Id::from("start")),
                priority: Some(1),
                from: vec![Id::from("idle")],
                to: vec![Id::from("run")],
                condition: ironplc_dsl::textual::Expr::new(
                    ironplc_dsl::textual::ExprKind::late_bound("go"),
                ),
            }),
            ElementKind::step(
                Id::from("run"),
                vec![
                    ActionAssociation {
                        name: Id::from("light"),
                        qualifier: Some(ActionQualifier::N),
                        indicators: vec![Id::from("lit")],
                    },
                    ActionAssociation::new("pulse", Some(ActionQualifier::SD(duration("T#2s")))),
                ],
            ),
            ElementKind::action(
                "light",
                vec![ironplc_dsl::textual::StmtKind::simple_assignment(
                    "lit", "src",
                )],
            ),
        ],
    }];
    assert_eq!(written, expected);
}
