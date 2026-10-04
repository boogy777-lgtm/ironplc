//! Sequential function charts: the networks of steps, transitions and actions a
//! body may be written as.
//!
//! A chart is a run of elements, and each `INITIAL_STEP` starts a network:
//! `PARTS` has a row for each kind of element, saying whether it begins a
//! network or belongs to the one before it. Another kind of element is another
//! row.
//!
//! A step lists the actions it is associated with, and each association may
//! carry a qualifier. `QUALIFIERS` has a row for each qualifier word, and says
//! whether the qualifier names a time (`SD, T#2s`) and which object it is.
//! A transition names the steps it leaves and the steps it enters (a list of
//! two or more is written in parentheses), and every step written is kept: the
//! legacy grammar drops all but the first two of a longer list.
//!
//! The body of an action is read by the rule that reads the body of a unit, so
//! an action holds statements or a chart of its own.

#[cfg(test)]
mod tests;

use super::expressions::lower_expr;
use super::literals::lower_constant;
use super::names::lower_name;
use super::pou::{declared_name, lower_body};
use super::tree::{child_of, children_of, significant_tokens};
use super::values::lower_priority;
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::common::ConstantKind;
use ironplc_dsl::core::Id;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::sfc::{
    Action, ActionAssociation, ActionQualifier, ActionTimeKind, ElementKind, Network, Step,
    Transition,
};

/// What a part of a chart is to the network it stands in.
enum Part {
    /// It is the initial step, and begins a network.
    Begins(fn(&LowerCx, &SyntaxNode) -> Result<Step, Diagnostic>),
    /// It is an element of the network that was begun last.
    Element(fn(&LowerCx, &SyntaxNode) -> Result<ElementKind, Diagnostic>),
}

/// The kinds of node a chart holds, and what each is.
const PARTS: &[(K, Part)] = &[
    (K::InitialStepDecl, Part::Begins(lower_step)),
    (K::StepDecl, Part::Element(step_element)),
    (K::TransitionDecl, Part::Element(transition_element)),
    (K::ActionDecl, Part::Element(action_element)),
];

fn step_element(cx: &LowerCx, node: &SyntaxNode) -> Result<ElementKind, Diagnostic> {
    lower_step(cx, node).map(ElementKind::Step)
}

fn transition_element(cx: &LowerCx, node: &SyntaxNode) -> Result<ElementKind, Diagnostic> {
    lower_transition(cx, node).map(ElementKind::Transition)
}

fn action_element(cx: &LowerCx, node: &SyntaxNode) -> Result<ElementKind, Diagnostic> {
    lower_action(cx, node).map(ElementKind::Action)
}

/// Lowers a chart to its networks, in the order written.
pub fn lower_chart(cx: &LowerCx, node: &SyntaxNode) -> Result<Vec<Network>, Diagnostic> {
    let mut networks: Vec<Network> = Vec::new();
    for child in node.children() {
        let (_, part) = PARTS
            .iter()
            .find(|(kind, _)| *kind == child.kind())
            .ok_or_else(|| cx.unsupported(&child))?;
        match part {
            Part::Begins(rule) => networks.push(Network {
                initial_step: rule(cx, &child)?,
                elements: vec![],
            }),
            Part::Element(rule) => networks
                .last_mut()
                .ok_or_else(|| cx.missing(node, "an initial step"))?
                .elements
                .push(rule(cx, &child)?),
        }
    }
    Ok(networks)
}

/// `STEP name : {action(qualifier, indicators);} END_STEP`, and `INITIAL_STEP`
/// the same.
pub fn lower_step(cx: &LowerCx, node: &SyntaxNode) -> Result<Step, Diagnostic> {
    Ok(Step {
        name: declared_name(cx, node)?,
        action_associations: children_of(node, K::ActionAssociation)
            .map(|association| lower_association(cx, &association))
            .collect::<Result<_, _>>()?,
    })
}

/// How a qualifier word is built: alone, or with the time it names.
#[derive(Clone)]
enum Form {
    Plain(ActionQualifier),
    Timed(fn(ActionTimeKind) -> ActionQualifier),
}

/// The qualifier words, spelled as the grammar matches them, and the qualifier
/// each is.
const QUALIFIERS: &[(&str, Form)] = &[
    ("N", Form::Plain(ActionQualifier::N)),
    ("R", Form::Plain(ActionQualifier::R)),
    ("S", Form::Plain(ActionQualifier::S)),
    ("L", Form::Plain(ActionQualifier::L)),
    ("D", Form::Plain(ActionQualifier::D)),
    ("P", Form::Plain(ActionQualifier::P)),
    ("SD", Form::Timed(ActionQualifier::SD)),
    ("DS", Form::Timed(ActionQualifier::DS)),
    ("SL", Form::Timed(ActionQualifier::SL)),
    ("P1", Form::Timed(ActionQualifier::PR)),
    ("P0", Form::Timed(ActionQualifier::PF)),
];

/// The time a qualifier names: a duration, or the name of a variable that
/// holds one.
fn lower_time(cx: &LowerCx, node: &SyntaxNode) -> Result<ActionTimeKind, Diagnostic> {
    match node.kind() {
        K::NameRef => lower_name(cx, node).map(ActionTimeKind::VariableName),
        K::DurationLiteral => match lower_constant(cx, node)? {
            ConstantKind::Duration(duration) => Ok(ActionTimeKind::Duration(duration)),
            _ => Err(cx.missing(node, "a duration")),
        },
        _ => Err(cx.syntax_error(
            node.text_range(),
            "expected a duration or the name of a variable",
        )),
    }
}

/// Lowers a qualifier, and the time it names.
pub fn lower_qualifier(cx: &LowerCx, node: &SyntaxNode) -> Result<ActionQualifier, Diagnostic> {
    let word = significant_tokens(node)
        .into_iter()
        .next()
        .ok_or_else(|| cx.missing(node, "a qualifier word"))?;
    let (_, form) = QUALIFIERS
        .iter()
        .find(|(spelling, _)| *spelling == word.text())
        .ok_or_else(|| cx.missing(node, "a qualifier word"))?;
    match form {
        Form::Plain(qualifier) => Ok(qualifier.clone()),
        Form::Timed(build) => {
            let time = node
                .first_child()
                .ok_or_else(|| cx.missing(node, "a time"))?;
            lower_time(cx, &time).map(build)
        }
    }
}

/// `action ( [qualifier] [, indicator {, indicator}] )`: the names after the
/// first are the indicators.
fn lower_association(cx: &LowerCx, node: &SyntaxNode) -> Result<ActionAssociation, Diagnostic> {
    let mut names = children_of(node, K::NameRef);
    let name = names
        .next()
        .ok_or_else(|| cx.missing(node, "the name of an action"))?;
    Ok(ActionAssociation {
        name: lower_name(cx, &name)?,
        qualifier: child_of(node, K::ActionQualifier)
            .map(|qualifier| lower_qualifier(cx, &qualifier))
            .transpose()?,
        indicators: names
            .map(|indicator| lower_name(cx, &indicator))
            .collect::<Result<_, _>>()?,
    })
}

/// The steps a transition names on one side: one, or the parenthesised list.
fn lower_steps(cx: &LowerCx, list: &SyntaxNode) -> Result<Vec<Id>, Diagnostic> {
    children_of(list, K::NameRef)
        .map(|step| lower_name(cx, &step))
        .collect()
}

/// `TRANSITION [name] [(PRIORITY := n)] FROM steps TO steps := condition ;
/// END_TRANSITION`
pub fn lower_transition(cx: &LowerCx, node: &SyntaxNode) -> Result<Transition, Diagnostic> {
    let mut lists = children_of(node, K::StepList);
    let (from, to) = match (lists.next(), lists.next()) {
        (Some(from), Some(to)) => (from, to),
        _ => return Err(cx.missing(node, "the steps it leaves and enters")),
    };
    let condition = child_of(node, K::TransitionCondition)
        .and_then(|condition| condition.first_child())
        .ok_or_else(|| cx.missing(node, "a condition"))?;
    let priority = child_of(node, K::TransitionPriority)
        .map(|priority| {
            priority
                .first_child()
                .ok_or_else(|| cx.missing(&priority, "a priority"))
                .and_then(|number| lower_priority(cx, &number))
        })
        .transpose()?;
    Ok(Transition {
        name: child_of(node, K::Name)
            .map(|name| lower_name(cx, &name))
            .transpose()?,
        priority,
        from: lower_steps(cx, &from)?,
        to: lower_steps(cx, &to)?,
        condition: lower_expr(cx, &condition)?,
    })
}

/// `ACTION name : body END_ACTION`
pub fn lower_action(cx: &LowerCx, node: &SyntaxNode) -> Result<Action, Diagnostic> {
    Ok(Action {
        name: declared_name(cx, node)?,
        body: lower_body(cx, node)?,
    })
}
