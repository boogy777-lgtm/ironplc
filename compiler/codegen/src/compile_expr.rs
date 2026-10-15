//! Expression compilation for IEC 61131-3 code generation.
//!
//! Contains expression dispatch and the operand-type queries. Constant
//! compilation, variable reads and references, partial and bit access
//! assignment, and typed opcode emission helpers live in the child modules.
//! Separated from compile.rs to keep module sizes within the 1000-line guideline.

mod bit_access_assignment;
mod constant;
mod partial_access_assignment;
mod typed_opcode;
mod variable_read;
mod variable_ref;

pub(crate) use bit_access_assignment::compile_bit_access_assignment;
pub(crate) use constant::{compile_constant, signed_integer_to_i64};
pub(crate) use partial_access_assignment::compile_partial_access_assignment;
pub(crate) use typed_opcode::{
    emit_add, emit_arithmetic_op, emit_compare_op, emit_div, emit_eq, emit_ge, emit_le,
    emit_load_var, emit_mod, emit_mul, emit_not, emit_store_var, emit_sub, emit_truncation,
};
pub(crate) use variable_ref::{
    extract_bit_access_target, extract_partial_access_target, in_out_ref_slot, resolve_variable,
    resolve_variable_name, variable_span,
};

use ironplc_analyzer::IntermediateType;
use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::textual::{CompareExpr, CompareOp, Expr, ExprKind, ExprType, UnaryOp};

use self::typed_opcode::emit_neg;
use self::variable_read::compile_variable_read;
use self::variable_ref::emit_load_in_out;
use super::compile::{CompileContext, OpType, OpWidth, Signedness, DEFAULT_OP_TYPE};
use super::compile_arith::compile_binary_arith;
use super::compile_call::compile_function_call;
use super::compile_method::compile_method_call_expression;
use super::compile_short_circuit::{compile_short_circuit, ShortCircuitOp};
use super::compile_string::compile_string_compare;
use super::type_info::{expr_operand_name, expr_own_op_type, expr_representation, expr_type_info};
use crate::emit::Emitter;
use crate::string_width::compare_is_string;

/// Returns the operation type of an expression's value, from its
/// `expr_type`.
///
/// The analyzer must have given the expression a type this backend operates
/// on. Anything else -- an array or a structure, say -- is a compiler bug.
pub(crate) fn op_type(ctx: &CompileContext, expr: &Expr) -> Result<OpType, Diagnostic> {
    let info = expr_type_info(ctx, expr).ok_or_else(|| unresolved_expr_type(expr))?;
    Ok((info.op_width, info.signedness))
}

/// Returns the operation type of an expression's value, if it has one.
///
/// Unlike [`op_type`] this returns `None` instead of an error when the
/// type is missing or not one this backend operates on, making it safe to
/// use as a best-effort fallback.
pub(crate) fn op_type_from_expr(ctx: &CompileContext, expr: &Expr) -> Option<OpType> {
    let info = expr_type_info(ctx, expr)?;
    Some((info.op_width, info.signedness))
}

/// Returns the operation type only when the expression has a concrete type,
/// not an untyped literal's generic category.
///
/// Generic types like `ANY_INT` map to a signed default (`DINT`) which is
/// wrong when the other operand is unsigned (e.g. `DWORD`). Returning
/// `None` for generic types lets callers prefer a concrete type from
/// another operand.
pub(crate) fn concrete_op_type_from_expr(ctx: &CompileContext, expr: &Expr) -> Option<OpType> {
    if !matches!(expr.expr_type, Some(ExprType::Concrete(_))) {
        return None;
    }
    op_type_from_expr(ctx, expr)
}

/// Returns `true` if the expression's value is a BOOL.
pub(crate) fn expr_is_bool(ctx: &CompileContext, expr: &Expr) -> bool {
    matches!(expr_representation(ctx, expr), Some(IntermediateType::Bool))
}

/// Returns the storage bit width of an expression's value.
///
/// The analyzer must have given the expression a type this backend operates
/// on. Anything else is a compiler bug.
pub(crate) fn storage_bits(ctx: &CompileContext, expr: &Expr) -> Result<u8, Diagnostic> {
    let info = expr_type_info(ctx, expr).ok_or_else(|| unresolved_expr_type(expr))?;
    Ok(info.storage_bits)
}

/// Builds the P9999 for an expression whose type the analyzer did not resolve
/// to one codegen knows, pointing at the expression.
///
/// The analyzer leaves `expr_type` empty for constructs it does not type
/// yet (a direct address such as `%QX0.0`, for example), so this is a gap in
/// the compiler rather than an invalid program.
#[track_caller]
pub(crate) fn unresolved_expr_type(expr: &Expr) -> Diagnostic {
    Diagnostic::not_implemented(Label::span(expr.span(), "Expression has no resolved type"))
}

/// Returns the operation type for compiling a condition expression.
///
/// For comparison operators (`>`, `<`, `=`, etc.), returns the type of the
/// left operand since the comparison's own resolved type is BOOL but we need
/// the operand type for correct signedness. For boolean combinations (AND,
/// OR, XOR), recurses into the first operand. For other expressions (bare
/// boolean variables, parenthesized expressions), returns the expression's
/// own resolved type.
pub(crate) fn condition_op_type(ctx: &CompileContext, expr: &Expr) -> Result<OpType, Diagnostic> {
    match &expr.kind {
        ExprKind::Compare(compare) => match compare.op {
            CompareOp::And
            | CompareOp::Or
            | CompareOp::Xor
            | CompareOp::AndThen
            | CompareOp::OrElse => condition_op_type(ctx, &compare.left),
            _ => {
                // String comparisons take a dedicated path in compile_expr
                // that emits an i32 boolean; the operand op_type is unused.
                if compare_is_string(ctx, compare) {
                    return Ok(DEFAULT_OP_TYPE);
                }
                op_type(ctx, &compare.left)
            }
        },
        ExprKind::UnaryOp(unary) if unary.op == UnaryOp::Not => condition_op_type(ctx, &unary.term),
        ExprKind::Expression(inner) => condition_op_type(ctx, inner),
        _ => op_type(ctx, expr),
    }
}
/// Compiles an expression, leaving the result on the stack.
///
/// The `op_type` determines which width (i32/i64) and signedness to use
/// for arithmetic, comparison, and load/store instructions.
pub(crate) fn compile_expr(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    expr: &Expr,
    op_type: OpType,
) -> Result<(), Diagnostic> {
    match &expr.kind {
        ExprKind::Const(constant) => compile_constant(emitter, ctx, constant, op_type),
        // A variable read at a different width is read at its own and
        // converted: loading an INT's slot as a REAL would reinterpret its
        // bits, and loading a UDINT's as a LINT would sign-extend it.
        ExprKind::Variable(variable) => match expr_own_op_type(ctx, expr) {
            Some(own) if own.0 != op_type.0 => {
                compile_variable_read(emitter, ctx, variable, own)?;
                crate::compile_arith::convert(emitter, own, op_type);
                Ok(())
            }
            _ => compile_variable_read(emitter, ctx, variable, op_type),
        },
        ExprKind::BinaryOp(binary) => {
            let result = expr_operand_name(ctx, expr);
            compile_binary_arith(emitter, ctx, binary, result.as_ref(), op_type)
        }
        ExprKind::UnaryOp(unary) => match unary.op {
            UnaryOp::Neg => {
                compile_expr(emitter, ctx, &unary.term, op_type)?;
                emit_neg(emitter, op_type);
                Ok(())
            }
            UnaryOp::Not => {
                compile_expr(emitter, ctx, &unary.term, op_type)?;
                emit_not(emitter, ctx, op_type, &unary.term)
            }
        },
        ExprKind::LateBound(late_bound) => {
            if let Some(ref_slot) = ctx.in_out_ref_slot(&late_bound.value) {
                emit_load_in_out(emitter, ref_slot);
                return Ok(());
            }
            let var_index = ctx.var_index(&late_bound.value)?;
            emit_load_var(emitter, var_index, op_type);
            Ok(())
        }
        ExprKind::Expression(inner) => compile_expr(emitter, ctx, inner, op_type),
        ExprKind::Compare(compare) => compile_compare(emitter, ctx, compare, op_type),
        ExprKind::EnumeratedValue(enum_val) => {
            // REQ-EN-codegen-030: Push the enum value's ordinal as an i32 constant.
            let ordinal = crate::compile_enum::resolve_enum_ordinal(&ctx.enum_map, enum_val)?;
            let pool_index = ctx.add_i32_constant(ordinal);
            emitter.emit_load_const_i32(pool_index);
            Ok(())
        }
        ExprKind::Function(func) => compile_function_call(emitter, ctx, func, op_type),
        ExprKind::MethodCall(call) => compile_method_call_expression(emitter, ctx, call),
        ExprKind::Ref(variable) => {
            // REF(param) of a VAR_IN_OUT parameter is the reference its slot
            // already holds: the caller's variable.
            if let Some(ref_slot) = in_out_ref_slot(ctx, variable) {
                emitter.emit_load_var_i64(ref_slot);
                return Ok(());
            }
            // REF(var) → push the variable's table index as a u64 constant.
            let var_index = resolve_variable(ctx, variable)?;
            let pool_index = ctx.add_i64_constant(var_index.into());
            emitter.emit_load_const_i64(pool_index);
            Ok(())
        }
        ExprKind::Deref(inner) => {
            // var^ → compile the reference expression (produces a var index),
            // then emit LOAD_INDIRECT to load the referenced variable's value.
            compile_expr(emitter, ctx, inner, (OpWidth::W64, Signedness::Unsigned))?;
            emitter.emit_load_indirect();
            Ok(())
        }
        ExprKind::Null(_) => {
            // NULL → push null sentinel (u64::MAX) as a u64 constant.
            let pool_index = ctx.add_i64_constant(u64::MAX as i64);
            emitter.emit_load_const_i64(pool_index);
            Ok(())
        }
    }
}

/// Compiles a comparison, logical, or bitwise binary expression, leaving the
/// result on the stack.
///
/// `op_type` is the type context the enclosing expression supplies; it is only
/// a fallback, because a comparison's own result is BOOL while its operands may
/// be any type.
fn compile_compare(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    compare: &CompareExpr,
    op_type: OpType,
) -> Result<(), Diagnostic> {
    // AND_THEN and OR_ELSE must not evaluate their right operand when the
    // left one already decides the answer, so they branch instead of
    // evaluating both operands into an eager bitwise op.
    if let Some(short_circuit) = ShortCircuitOp::for_expr(ctx, compare) {
        return compile_short_circuit(emitter, ctx, compare, short_circuit);
    }

    // String comparisons need a completely different code path because
    // strings live in the data region, not on the operand stack.
    if compare_is_string(ctx, compare) {
        return compile_string_compare(emitter, ctx, compare);
    }

    // A comparison's result is BOOL, but its operands may be a different
    // type (e.g. REAL for `in < 0.0`). Derive the operand type from a
    // concrete (non-generic) resolved type, preferring the left operand.
    // When one side is a literal (generic type like ANY_INT) and the other
    // is a typed variable (e.g. DWORD), we use the concrete type to ensure
    // correct signedness. This also applies to AND/OR/XOR which can be
    // either boolean (BOOL operands) or bitwise (e.g. DWORD operands).
    let operand_op_type = concrete_op_type_from_expr(ctx, &compare.left)
        .or_else(|| concrete_op_type_from_expr(ctx, &compare.right))
        .or_else(|| op_type_from_expr(ctx, &compare.left))
        .unwrap_or(op_type);
    compile_expr(emitter, ctx, &compare.left, operand_op_type)?;
    compile_expr(emitter, ctx, &compare.right, operand_op_type)?;
    emit_compare_op(emitter, &compare.op, operand_op_type);
    Ok(())
}
