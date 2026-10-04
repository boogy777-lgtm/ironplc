//! Edge inputs: `x : BOOL R_EDGE` and `x : BOOL F_EDGE`.
//!
//! An edge input is an ordinary input whose value the body reads as `TRUE`
//! only in the scan in which the actual input changed in the edge's direction.
//! The input itself keeps the actual value, whoever writes it: a call site, the
//! runtime, or a task for a program. What the body reads is derived from it at
//! the start of the body, from two hidden variables the unit owns beside its
//! declared ones:
//!
//! - `<x>$prev`, the actual value of the previous scan;
//! - `<x>$edge`, the value the body reads in place of `x`.
//!
//! The hidden variables are declarations like any other, so each kind of unit
//! stores them the way it stores the rest of its variables: a program as
//! variables of its own, a function block as fields of every instance (so two
//! instances never share a previous value). Their names cannot be written in
//! source, so they never clash with a declared name.
//!
//! A function has no instance to keep a previous value in, and a method
//! writes into the instance of the block it belongs to, so neither takes an
//! edge input.

use ironplc_dsl::common::{EdgeDirection, VarDecl};
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::{Diagnostic, Label};

use crate::compile::{CompileContext, DEFAULT_OP_TYPE};
use crate::compile_expr::{emit_load_var, emit_store_var};
use crate::emit::Emitter;

/// An edge input and the hidden variables that detect its edge.
pub(crate) struct EdgeInput {
    name: Id,
    direction: EdgeDirection,
    previous: Id,
    edge: Id,
}

/// The edge inputs among `variables`, in the order declared.
pub(crate) fn edge_inputs(variables: &[VarDecl]) -> Vec<EdgeInput> {
    variables
        .iter()
        .filter_map(|decl| {
            let (direction, name) = (decl.edge?, decl.identifier.symbolic_id()?);
            Some(EdgeInput {
                name: name.clone(),
                direction,
                previous: Id::from(&format!("{name}$prev")),
                edge: Id::from(&format!("{name}$edge")),
            })
        })
        .collect()
}

/// The hidden variables the edge inputs among `variables` need, as the
/// declarations a unit adds to its own.
pub(crate) fn hidden_variables(variables: &[VarDecl]) -> Vec<VarDecl> {
    edge_inputs(variables)
        .into_iter()
        .flat_map(|input| [input.previous, input.edge])
        .map(|name| VarDecl::simple(&name.to_string(), "BOOL"))
        .collect()
}

/// Starts the detection of the edge inputs `inputs`, once the unit's variables
/// have their places: the body that follows reads each edge input as its
/// hidden variable, and the code emitted here (the start of the body) derives
/// that variable from the actual input.
pub(crate) fn bind_edge_inputs(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    inputs: &[EdgeInput],
) -> Result<(), Diagnostic> {
    for input in inputs {
        let place = |ctx: &CompileContext, id: &Id| {
            ctx.variables
                .get(id)
                .copied()
                .ok_or_else(Diagnostic::internal_error)
        };
        let (actual, previous, edge) = (
            place(ctx, &input.name)?,
            place(ctx, &input.previous)?,
            place(ctx, &input.edge)?,
        );

        // Rising: actual AND NOT previous. Falling: NOT actual AND previous.
        match input.direction {
            EdgeDirection::Rising => {
                emit_load_var(emitter, actual, DEFAULT_OP_TYPE);
                emit_load_var(emitter, previous, DEFAULT_OP_TYPE);
                emitter.emit_bool_not();
            }
            EdgeDirection::Falling => {
                emit_load_var(emitter, actual, DEFAULT_OP_TYPE);
                emitter.emit_bool_not();
                emit_load_var(emitter, previous, DEFAULT_OP_TYPE);
            }
        }
        emitter.emit_bool_and();
        emit_store_var(emitter, edge, DEFAULT_OP_TYPE);

        emit_load_var(emitter, actual, DEFAULT_OP_TYPE);
        emit_store_var(emitter, previous, DEFAULT_OP_TYPE);

        ctx.variables.insert(input.name.clone(), edge);
    }
    Ok(())
}

/// Refuses an edge input on a unit that has nowhere to keep the previous
/// actual value: `unit` says which kind of unit it is.
pub(crate) fn reject_edge_inputs(variables: &[VarDecl], unit: &str) -> Result<(), Diagnostic> {
    match variables.iter().find(|decl| decl.edge.is_some()) {
        Some(decl) => Err(Diagnostic::not_supported(Label::span(
            decl.identifier.span(),
            format!("An edge input is not supported on {unit}; it needs a function block or a program to keep its previous value"),
        ))),
        None => Ok(()),
    }
}
