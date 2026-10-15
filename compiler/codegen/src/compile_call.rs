//! Function call compilation for IEC 61131-3 code generation.
//!
//! Contains the dispatch of function calls. Standard library functions,
//! user-defined function calls, builtin lookup, type conversions, time functions
//! and shift/rotate operations live in the child modules.
//! Separated from compile.rs to keep module sizes within the 1000-line guideline.

mod builtin_lookup;
mod datetime;
mod fb_types;
mod operator_form;
mod standard_functions;
mod string_conversion;
mod type_conversion;
mod user_call;

#[cfg(test)]
mod tests;

pub(crate) use fb_types::resolve_fb_type;
pub(crate) use operator_form::compile_left_fold;
pub(crate) use string_conversion::{parse_string_conversion, StringConversion};
pub(crate) use type_conversion::emit_conversion_opcode;

use ironplc_analyzer::{operator_function_form, SpecialOperator};
use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::textual::{Expr, Function, ParamAssignmentKind};

use self::datetime::{compile_dt_to_date, compile_dt_to_tod};
use self::operator_form::compile_operator_form;
use self::standard_functions::{
    compile_bcd_to_int, compile_generic_builtin, compile_int_to_bcd, compile_move, compile_mux,
    compile_shift_rotate, compile_sizeof, compile_trunc,
};
use self::string_conversion::compile_string_conversion;
use self::type_conversion::{compile_type_conversion, parse_type_conversion};
use self::user_call::compile_user_function_call;
use super::compile::{CompileContext, OpType};
use super::compile_string::{
    compile_concat, compile_delete, compile_find, compile_insert, compile_left, compile_len,
    compile_mid, compile_replace, compile_right,
};
use super::compile_time_arith::{compile_time_arith, time_arith_for, Operand};
use crate::emit::Emitter;

/// Compiles a standard library function call.
///
/// Dispatches shift/rotate functions to a width-aware handler, and other
/// known builtins to the generic lookup path.
pub(crate) fn compile_function_call(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    op_type: OpType,
) -> Result<(), Diagnostic> {
    let name = func.name.lower_case();
    // A function form of an operator (ADD, GT, AND, NOT, ...) compiles as
    // the operator it is a form of; the analyzer's table says which.
    if let Some(form) = operator_function_form(name.as_str()) {
        return compile_operator_form(emitter, ctx, func, op_type, &form.operator);
    }
    // A typed time or date function (ADD_TIME, SUB_DATE_DATE, ...) compiles
    // as the instruction sequence for the units of its operands.
    if let Some((arith, width)) = time_arith_for(name.as_str()) {
        let (in1, in2) = extract_two_positional_args(func)?;
        return compile_time_arith(emitter, ctx, arith, width, Operand::Expr(in1), in2);
    }
    match name.as_str() {
        "shl" | "shr" | "rol" | "ror" => {
            compile_shift_rotate(emitter, ctx, func, op_type, name.as_str())
        }
        "mux" => compile_mux(emitter, ctx, func, op_type),
        // Assignment function (equivalent to := operator)
        "move" => compile_move(emitter, ctx, func, op_type),
        // Truncation function
        "trunc" => compile_trunc(emitter, ctx, func, op_type),
        // SIZEOF operator (extension)
        "sizeof" => compile_sizeof(emitter, ctx, func),
        // BCD conversion functions
        "bcd_to_int" => compile_bcd_to_int(emitter, ctx, func, op_type),
        "int_to_bcd" => compile_int_to_bcd(emitter, ctx, func, op_type),
        // String functions
        "len" => compile_len(emitter, ctx, func),
        "find" => compile_find(emitter, ctx, func),
        "replace" => compile_replace(emitter, ctx, func),
        "insert" => compile_insert(emitter, ctx, func),
        "delete" => compile_delete(emitter, ctx, func),
        "left" => compile_left(emitter, ctx, func),
        "right" => compile_right(emitter, ctx, func),
        "mid" => compile_mid(emitter, ctx, func),
        "concat" => compile_concat(emitter, ctx, func),
        // Time functions: datetime decomposition
        "dt_to_date" | "date_and_time_to_date" => compile_dt_to_date(emitter, ctx, func),
        "dt_to_tod" | "date_and_time_to_time_of_day" => compile_dt_to_tod(emitter, ctx, func),
        _ => {
            // Check user-defined functions first.
            if let Some(func_info) = ctx.user_functions.get(name.as_str()).cloned() {
                compile_user_function_call(emitter, ctx, func, &func_info)
            } else if let Some(operator) = SpecialOperator::named(&func.name) {
                // `__NEW`, `__DELETE`, `__TYPEOF` and `__XADD` are typed by
                // the analyzer but have no runtime behaviour here (heap
                // allocation, type classes, atomic memory access). Refuse
                // rather than emit code that only looks like them.
                Err(Diagnostic::not_implemented(Label::span(
                    func.name.span(),
                    "CODESYS special operator has no runtime behaviour",
                ))
                .with_context("operator", &operator.name().to_string()))
            } else if let Some(conv) = parse_string_conversion(name) {
                compile_string_conversion(emitter, ctx, func, conv)
            } else if let Some((source, target)) = parse_type_conversion(name) {
                compile_type_conversion(emitter, ctx, func, source, target)
            } else {
                compile_generic_builtin(emitter, ctx, func, op_type)
            }
        }
    }
}
/// Extracts two positional input arguments from a function call.
fn extract_two_positional_args(func: &Function) -> Result<(&Expr, &Expr), Diagnostic> {
    match collect_positional_args(func).as_slice() {
        [in1, in2] => Ok((in1, in2)),
        _ => Err(Diagnostic::todo_with_span(func.name.span())),
    }
}

/// Collects positional input arguments from a function call.
pub(crate) fn collect_positional_args(func: &Function) -> Vec<&Expr> {
    func.param_assignment
        .iter()
        .filter_map(|p| match p {
            ParamAssignmentKind::PositionalInput(pos) => Some(&pos.expr),
            _ => None,
        })
        .collect()
}
