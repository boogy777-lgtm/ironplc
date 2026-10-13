//! Transformation of the PLCopen XML SFC body to the DSL `Sfc`: steps,
//! transitions, actions and the associations between them.
//!
//! A child of [`super`], which owns the helpers it shares with the other
//! transforms (`make_id`, `file_span`, the embedded ST parsers).
use ironplc_dsl::sfc::{
    Action as SfcAction, ActionAssociation, ActionQualifier, ElementKind, Network, Sfc, Step,
    Transition,
};
use ironplc_dsl::textual::Expr;

use super::*;
use crate::xml::schema::SfcBody;

/// Transform SFC body from XML to DSL Sfc
pub(super) fn transform_sfc_body(
    sfc_body: &SfcBody,
    pou: &Pou,
    file_id: &FileId,
    compiler_options: &CompilerOptions,
) -> Result<Sfc, Diagnostic> {
    // Find the initial step
    let initial_step = sfc_body
        .steps
        .iter()
        .find(|s| s.initial_step)
        .ok_or_else(|| {
            Diagnostic::problem(
                Problem::SfcMissingInitialStep,
                Label::span(file_span(file_id), "SFC body"),
            )
        })?;

    // Build action associations map from action blocks
    let action_associations = build_action_associations(sfc_body, file_id);

    // Transform initial step
    let initial_step_dsl = transform_sfc_step(initial_step, &action_associations, file_id);

    // Transform other elements (non-initial steps, transitions, actions)
    let mut elements = Vec::new();

    // Add non-initial steps
    for step in &sfc_body.steps {
        if !step.initial_step {
            elements.push(ElementKind::Step(transform_sfc_step(
                step,
                &action_associations,
                file_id,
            )));
        }
    }

    // Add transitions
    for transition in &sfc_body.transitions {
        elements.push(ElementKind::Transition(transform_sfc_transition(
            transition, file_id,
        )?));
    }

    // Add actions from POU-level actions container
    if let Some(ref actions) = pou.actions {
        for action in &actions.action {
            elements.push(ElementKind::Action(transform_sfc_action(
                action,
                file_id,
                compiler_options,
            )?));
        }
    }

    Ok(Sfc {
        networks: vec![Network {
            initial_step: initial_step_dsl,
            elements,
        }],
    })
}

/// Build a map of step names to their action associations
fn build_action_associations(
    sfc_body: &SfcBody,
    file_id: &FileId,
) -> std::collections::HashMap<String, Vec<ActionAssociation>> {
    let mut map = std::collections::HashMap::new();

    for action_block in &sfc_body.action_blocks {
        let associations: Vec<ActionAssociation> = action_block
            .actions
            .iter()
            .map(|a| ActionAssociation {
                name: make_id(&a.action_name, file_id),
                qualifier: a.qualifier.as_ref().and_then(|q| parse_action_qualifier(q)),
                indicators: vec![],
            })
            .collect();

        map.insert(action_block.step_name.value.clone(), associations);
    }

    map
}

/// Transform SFC step
fn transform_sfc_step(
    step: &crate::xml::schema::SfcStep,
    action_associations: &std::collections::HashMap<String, Vec<ActionAssociation>>,
    file_id: &FileId,
) -> Step {
    let associations = action_associations
        .get(&step.name.value)
        .cloned()
        .unwrap_or_default();

    Step {
        name: make_id(&step.name, file_id),
        action_associations: associations,
    }
}

/// Transform SFC transition
fn transform_sfc_transition(
    transition: &crate::xml::schema::SfcTransition,
    file_id: &FileId,
) -> Result<Transition, Diagnostic> {
    // Parse the condition
    let condition = if let Some(ref st_body) = transition.condition_st {
        // Parse inline ST condition as an expression
        Expr::new(parse_st_condition(
            &st_body.text,
            file_id,
            st_body.line_offset,
            st_body.col_offset,
        )?)
    } else if let Some(ref ref_name) = transition.condition_reference {
        // Reference to a named transition - use the name as a variable reference
        Expr::new(ExprKind::named_variable(ref_name))
    } else {
        // Default to TRUE if no condition specified
        Expr::new(ExprKind::Const(ConstantKind::Boolean(BooleanLiteral::new(
            Boolean::True,
        ))))
    };

    Ok(Transition {
        name: transition.name.as_ref().map(|n| make_id(n, file_id)),
        priority: transition.priority,
        from: transition
            .from_steps
            .iter()
            .map(|s| make_id(s, file_id))
            .collect(),
        to: transition
            .to_steps
            .iter()
            .map(|s| make_id(s, file_id))
            .collect(),
        condition,
    })
}

/// Transform SFC action from POU-level action
fn transform_sfc_action(
    action: &crate::xml::schema::Action,
    file_id: &FileId,
    compiler_options: &CompilerOptions,
) -> Result<SfcAction, Diagnostic> {
    let name = make_id(&action.name, file_id);

    // Parse the action body
    let body = if let Some(st_body) = action.body.st_body() {
        let stmts = parse_st_body(
            &st_body.text,
            file_id,
            st_body.line_offset,
            st_body.col_offset,
            compiler_options,
        )?;
        FunctionBlockBodyKind::Statements(Statements { body: stmts })
    } else {
        FunctionBlockBodyKind::Empty
    };

    Ok(SfcAction { name, body })
}

/// Parse an action qualifier string to ActionQualifier
fn parse_action_qualifier(qualifier: &str) -> Option<ActionQualifier> {
    match qualifier.to_uppercase().as_str() {
        "N" => Some(ActionQualifier::N),
        "R" => Some(ActionQualifier::R),
        "S" => Some(ActionQualifier::S),
        "L" => Some(ActionQualifier::L),
        "D" => Some(ActionQualifier::D),
        "P" => Some(ActionQualifier::P),
        // Timed qualifiers would need duration parsing
        _ => None,
    }
}
