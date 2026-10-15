//! Compiles the date and time-of-day extraction functions of a DATE_AND_TIME value.

use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::Function;

use super::collect_positional_args;
use crate::compile::{CompileContext, OpWidth, Signedness};
use crate::compile_expr::{compile_expr, emit_mod, emit_mul, emit_sub};
use crate::emit::Emitter;

/// Compiles DT_TO_DATE and DATE_AND_TIME_TO_DATE.
///
/// Extracts the date portion from a DATE_AND_TIME by stripping the
/// time-of-day: `IN - (IN % 86400)`. Both DT and DATE are in seconds
/// since 1970-01-01.
pub(super) fn compile_dt_to_date(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
) -> Result<(), Diagnostic> {
    let args = collect_positional_args(func);

    if args.len() != 1 {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    let op_type = (OpWidth::W32, Signedness::Unsigned);
    // Stack: IN
    compile_expr(emitter, ctx, args[0], op_type)?;
    // Stack: IN, IN
    compile_expr(emitter, ctx, args[0], op_type)?;
    // Stack: IN, IN, 86400
    let secs_per_day = ctx.add_i32_constant(86400);
    emitter.emit_load_const_i32(secs_per_day);
    // Stack: IN, (IN % 86400)
    emit_mod(emitter, op_type);
    // Stack: IN - (IN % 86400)
    emit_sub(emitter, op_type);
    Ok(())
}

/// Compiles DT_TO_TOD and DATE_AND_TIME_TO_TIME_OF_DAY.
///
/// Extracts the time-of-day from a DATE_AND_TIME: `(IN % 86400) * 1000`.
/// DT is in seconds since epoch; TOD is in milliseconds since midnight.
pub(super) fn compile_dt_to_tod(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
) -> Result<(), Diagnostic> {
    let args = collect_positional_args(func);

    if args.len() != 1 {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    let op_type = (OpWidth::W32, Signedness::Unsigned);
    // Stack: IN
    compile_expr(emitter, ctx, args[0], op_type)?;
    // Stack: IN, 86400
    let secs_per_day = ctx.add_i32_constant(86400);
    emitter.emit_load_const_i32(secs_per_day);
    // Stack: (IN % 86400)
    emit_mod(emitter, op_type);
    // Stack: (IN % 86400), 1000
    let ms_per_sec = ctx.add_i32_constant(1000);
    emitter.emit_load_const_i32(ms_per_sec);
    // Stack: (IN % 86400) * 1000
    emit_mul(emitter, op_type);
    Ok(())
}
