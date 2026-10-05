//! Writes to the fields of a function block instance.
//!
//! Writing one field is a fixed four-instruction sequence — load the
//! instance, evaluate the value, store it into the named parameter slot,
//! drop the instance handle — wrapped in a field lookup that resolves the
//! field's name to its slot index and operand type.
//!
//! It lives here rather than inline in either caller because the sequence
//! belongs to the function block instance, not to what happens to be
//! setting the member. Two spellings set one:
//!
//! * an assignment statement (`timer.PT := T#100MS;`), and
//! * a declaration's member initializer (`timer : TON := (PT := T#100MS);`),
//!   emitted once as part of the setup block.
//!
//! One copy of the sequence is what makes those two observably the same
//! thing at runtime.

use ironplc_analyzer::TypeEnvironment;
use ironplc_dsl::common::{
    ConstantKind, InitialValueAssignmentKind, StructInitialValueAssignmentKind,
    StructureElementInit, VarDecl,
};
use ironplc_dsl::construct::merge_member_inits;
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::textual::{Expr, ExprKind, Variable};

use super::compile::{CompileContext, OpType, StringVarInfo, DEFAULT_OP_TYPE};
use super::compile_expr::{compile_expr, resolve_variable_name, variable_span};
use super::compile_fb_instance::ResolvedInstance;
use super::compile_initial_value::subrange_lower_bound;
use crate::emit::Emitter;
use crate::string_width::{compile_string_value, encoding_mismatch};

/// Resolves the operand type for a function block field.
///
/// A user-defined function block records the operand type of each of its
/// fields, so its own table answers first. A standard library block records
/// none — the intrinsic owns its layout, not codegen — so its fields take
/// the default slot type.
pub(crate) fn resolve_fb_field_op_type(
    ctx: &CompileContext,
    type_id: u16,
    field_name: &str,
) -> OpType {
    // Check user-defined FBs by type_id.
    for user_fb in ctx.user_fb_types.values() {
        if user_fb.type_id == type_id {
            if let Some(op_type) = user_fb.field_op_types.get(field_name) {
                return *op_type;
            }
        }
    }
    DEFAULT_OP_TYPE
}

/// The run of the STRING/WSTRING field `field` of the function block instance
/// `instance`, when the field keeps its characters in one: a fixed offset,
/// because the instance is at a known place.
pub(crate) fn instance_string_field(
    ctx: &CompileContext,
    instance: &Id,
    field: &Id,
) -> Option<StringVarInfo> {
    ctx.fb_instances
        .get(instance)?
        .strings
        .get(&field.to_string().to_lowercase())
        .cloned()
}

/// Emits a store of `value` into the string field whose run is `info`: the
/// value is produced at the field's encoding, then written into the run. The
/// instance reference, if one is on the stack, is left where it is.
pub(crate) fn compile_string_field_store(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    info: &StringVarInfo,
    value: &Expr,
) -> Result<(), Diagnostic> {
    compile_string_value(emitter, ctx, value, info.char_width)?;
    info.emit_store(emitter, ctx);
    Ok(())
}

/// Copies the string field whose run is `source` out of an instance into the
/// string variable `target`, which is what binding it to an output
/// (`name => target`) means.
pub(crate) fn compile_string_output(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    source: &StringVarInfo,
    target: &Variable,
) -> Result<(), Diagnostic> {
    let span = variable_span(target);
    let Some(destination) = resolve_variable_name(target)
        .and_then(|name| ctx.string_vars.get(name))
        .cloned()
    else {
        return Err(Diagnostic::not_implemented(Label::span(
            span,
            "A string output of a function block bound to something other than a string variable",
        )));
    };
    if destination.char_width != source.char_width {
        return Err(encoding_mismatch(
            destination.char_width,
            source.char_width,
            &span,
        ));
    }
    source.emit_load(emitter, ctx);
    destination.emit_store(emitter, ctx);
    Ok(())
}

/// Emits a store of `value` into `field` of `instance`.
pub(crate) fn compile_fb_field_store(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    instance: &ResolvedInstance,
    field: &Id,
    value: &Expr,
) -> Result<(), Diagnostic> {
    let field_name = field.to_string().to_lowercase();
    // A string keeps its characters in a run of the instance, not in the slot.
    if let Some(info) = instance.strings.get(&field_name) {
        return compile_string_field_store(emitter, ctx, info, value);
    }
    let field_idx = instance
        .field_indices
        .get(&field_name)
        .copied()
        .ok_or_else(|| {
            Diagnostic::not_implemented(Label::span(
                field.span(),
                format!(
                    "Unknown field '{field}' on a function block instance \
                     (writing a PROPERTY is not supported yet)"
                ),
            ))
        })?;

    let op_type = resolve_fb_field_op_type(ctx, instance.type_id, &field_name);
    instance.emit_reference(emitter, ctx)?;
    compile_expr(emitter, ctx, value, op_type)?;
    emitter.emit_fb_store_param(field_idx);
    emitter.emit_pop();
    Ok(())
}

/// Emits the member initializers of a function block instance declaration
/// (`timer : TON := (PT := T#100MS);`).
///
/// Each member is stored exactly as the equivalent assignment statement
/// would store it, so the instance is already initialized before the first
/// scan invokes it.
pub(crate) fn emit_fb_instance_member_initializers(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    instance: &ResolvedInstance,
    init: &[StructureElementInit],
) -> Result<(), Diagnostic> {
    for element in init {
        let value = match &element.init {
            StructInitialValueAssignmentKind::Constant(constant) => {
                Expr::new(ExprKind::Const(constant.clone()))
            }
            StructInitialValueAssignmentKind::EnumeratedValue(value) => {
                Expr::new(ExprKind::EnumeratedValue(value.clone()))
            }
            StructInitialValueAssignmentKind::Expression(expr) => expr.clone(),
            // `xform_resolve_late_bound_expr_kind` replaces every one of
            // these with an enumerated value or an expression, so reaching
            // codegen with one means that pass did not run.
            StructInitialValueAssignmentKind::LateBound(late_bound) => {
                return Err(Diagnostic::internal_error_at(Label::span(
                    late_bound.value.span(),
                    "Unresolved function block instance member initializer",
                )))
            }
            // An array or nested structure value initializes several slots
            // at once, which the single-slot FB_STORE_PARAM path cannot
            // express. Refuse rather than silently leave the member zeroed.
            StructInitialValueAssignmentKind::Array(_)
            | StructInitialValueAssignmentKind::Structure(_) => {
                return Err(Diagnostic::not_implemented(Label::span(
                    element.name.span(),
                    format!(
                        "Array or structure value initializing field '{}' of a function block instance",
                        element.name
                    ),
                )))
            }
        };
        compile_fb_field_store(emitter, ctx, instance, &element.name, &value)?;
    }
    Ok(())
}

/// The values the fields of a function block declare for themselves
/// (`VAR count : INT := 4; END_VAR`), as the member initializers an instance
/// starts from.
///
/// A field starts at the value its declaration states, else the value the type
/// it names declares, else, for a subrange, its lower bound: the one reading
/// of a declared value, so an instance and every other storage location agree.
/// Only the fields that take one slot are listed; a string, an array and a
/// structure are laid out by their own routines and keep what they have.
pub(crate) fn declared_field_values(
    types: &TypeEnvironment,
    fields: &[&VarDecl],
) -> Vec<StructureElementInit> {
    fields
        .iter()
        .filter_map(|decl| {
            let name = decl.identifier.symbolic_id()?.clone();
            let init =
                types
                    .initial_value_of(&decl.initializer)
                    .or_else(|| match &decl.initializer {
                        InitialValueAssignmentKind::Subrange(subrange) => {
                            let bound = subrange_lower_bound(types, &subrange.spec)?;
                            ConstantKind::integer_literal(&bound.to_string())
                                .ok()
                                .map(StructInitialValueAssignmentKind::Constant)
                        }
                        _ => None,
                    })?;
            match &init {
                StructInitialValueAssignmentKind::Constant(ConstantKind::CharacterString(_))
                | StructInitialValueAssignmentKind::Array(_)
                | StructInitialValueAssignmentKind::Structure(_) => None,
                _ => Some(StructureElementInit { name, init }),
            }
        })
        .collect()
}

/// The members a function block instance starts from: the values its type
/// declares for its fields, with those its own declaration states laid over
/// them.
pub(crate) fn instance_members(
    ctx: &CompileContext,
    type_id: u16,
    stated: &[StructureElementInit],
) -> Vec<StructureElementInit> {
    let declared = ctx
        .user_fb_types
        .values()
        .find(|user_fb| user_fb.type_id == type_id)
        .map(|user_fb| user_fb.field_defaults.as_slice())
        .unwrap_or_default();
    merge_member_inits(declared, stated)
}
