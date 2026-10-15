//! Compiles the function form of an operator (ADD, GT, NOT, ...) as the operator itself.

use ironplc_analyzer::FormOf;
use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::Function;

use super::collect_positional_args;
use crate::compile::{CompileContext, OpType};
use crate::compile_arith::compile_arith_fold;
use crate::compile_expr::{compile_expr, emit_compare_op, emit_not};
use crate::emit::Emitter;

/// Compiles the function form of an operator as the operator itself.
///
/// The arguments compile at the enclosing expression's operation type, as
/// every function argument does, and the opcode comes from the emitter the
/// operator expression uses, so `AND(a, b)` and `a AND b` cannot diverge.
///
/// A binary operator folds its arguments from the left, so `ADD(a, b, c)`
/// compiles as `(a + b) + c`. The analyzer has already enforced how many
/// arguments the form takes; the fold is the same code for the two of a
/// binary form and the two or more of an extensible one.
pub(super) fn compile_operator_form(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    op_type: OpType,
    operator: &FormOf,
) -> Result<(), Diagnostic> {
    match operator {
        FormOf::Arithmetic(op) => compile_arith_fold(emitter, ctx, func, op, op_type),
        FormOf::Compare(op) => {
            compile_left_fold(emitter, ctx, func, op_type, |emitter, op_type| {
                emit_compare_op(emitter, op, op_type)
            })
        }
        FormOf::Not => {
            let args = collect_positional_args(func);
            let [term] = args.as_slice() else {
                return Err(Diagnostic::todo_with_span(func.name.span()));
            };
            compile_expr(emitter, ctx, term, op_type)?;
            emit_not(emitter, ctx, op_type, term)
        }
    }
}

/// Compiles a call's two or more positional arguments, emitting the operator
/// after each argument but the first, so the arguments fold from the left.
pub(crate) fn compile_left_fold(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    op_type: OpType,
    emit_fn: impl Fn(&mut Emitter, OpType),
) -> Result<(), Diagnostic> {
    let args = collect_positional_args(func);
    let [first, rest @ ..] = args.as_slice() else {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    };
    if rest.is_empty() {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }
    compile_expr(emitter, ctx, first, op_type)?;
    for arg in rest {
        compile_expr(emitter, ctx, arg, op_type)?;
        emit_fn(emitter, op_type);
    }
    Ok(())
}
