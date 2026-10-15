//! Compilation of assignments to a bit slice (partial access) of a variable, array element or structure field.

use ironplc_container::opcode;
use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::textual::{
    ArrayVariable, Expr, PartialAccessVariable, StructuredVariable, SymbolicVariableKind,
};

use super::compile_expr;
use super::typed_opcode::{emit_load_var, emit_store_var, emit_truncation};
use super::variable_ref::resolve_symbolic_variable_name;
use crate::compile::{CompileContext, OpWidth, Signedness};
use crate::emit::Emitter;

/// Compiles a partial-access assignment using read-modify-write.
///
/// `target.%Bn := value` clears the byte at position n and ORs in the RHS.
/// Supports named scalars; array elements and struct fields dispatch the
/// same logic used by bit access.
pub(crate) fn compile_partial_access_assignment(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    pa: &PartialAccessVariable,
    value: &Expr,
) -> Result<(), Diagnostic> {
    // Delegate to sub-handlers for array and struct bases, mirroring the
    // bit-access dispatch structure.
    if let SymbolicVariableKind::Array(array) = pa.variable.as_ref() {
        if let SymbolicVariableKind::Structured(structured) = array.subscripted_variable.as_ref() {
            return compile_partial_access_assignment_on_struct_field_array(
                emitter, ctx, structured, array, pa, value,
            );
        }
        return compile_partial_access_assignment_on_array(emitter, ctx, array, pa, value);
    }
    if let SymbolicVariableKind::Structured(structured) = pa.variable.as_ref() {
        return compile_partial_access_assignment_on_struct_field(
            emitter, ctx, structured, pa, value,
        );
    }

    let base_name = resolve_symbolic_variable_name(&pa.variable)?;
    let var_index = ctx.var_index(base_name)?;
    let base_op_type = ctx.var_op_type(base_name);

    emit_load_var(emitter, var_index, base_op_type);
    emit_partial_access_read_modify_write(emitter, ctx, base_op_type.0, pa, value)?;

    if let Some(ti) = ctx.var_type_info(base_name) {
        emit_truncation(emitter, ti);
    }
    emit_store_var(emitter, var_index, base_op_type);
    Ok(())
}

fn compile_partial_access_assignment_on_array(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    array: &ArrayVariable,
    pa: &PartialAccessVariable,
    value: &Expr,
) -> Result<(), Diagnostic> {
    let root_name = resolve_symbolic_variable_name(&array.subscripted_variable)?;
    let info = ctx.array_vars.get(root_name).ok_or_else(|| {
        Diagnostic::not_implemented(Label::span(
            pa.span(),
            "Partial access on non-trivial array base",
        ))
    })?;
    let arr_var_index = info.var_index;
    let arr_desc_index = info.desc_index;
    let element_vti = info.element_var_type_info;
    let dim_info: Vec<crate::compile_array::DimensionInfo> = info
        .dimensions
        .iter()
        .map(|d| crate::compile_array::DimensionInfo {
            lower_bound: d.lower_bound,
            size: d.size,
            stride: d.stride,
        })
        .collect();
    let subscripts: Vec<&Expr> = array.subscripts.iter().collect();
    let span = pa.span();

    crate::compile_array::emit_flat_index(emitter, ctx, &subscripts, &dim_info, &span)?;
    emitter.emit_load_array(arr_var_index, arr_desc_index);

    emit_partial_access_read_modify_write(emitter, ctx, element_vti.op_width, pa, value)?;
    emit_truncation(emitter, element_vti);

    crate::compile_array::emit_flat_index(emitter, ctx, &subscripts, &dim_info, &span)?;
    emitter.emit_store_array(arr_var_index, arr_desc_index);
    Ok(())
}

fn compile_partial_access_assignment_on_struct_field(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    structured: &StructuredVariable,
    pa: &PartialAccessVariable,
    value: &Expr,
) -> Result<(), Diagnostic> {
    let (var_index, desc_index, slot_offset, _op_type, field_type) =
        crate::compile_struct::resolve_struct_field_access(ctx, structured)?;
    let field_vti =
        crate::compile_struct::var_type_info_for_field(&field_type).ok_or_else(|| {
            Diagnostic::not_implemented(Label::span(
                structured.field.span(),
                "Partial access on non-integer struct field",
            ))
        })?;

    let idx_const = ctx.add_i32_constant(slot_offset.raw() as i32);

    emitter.emit_load_const_i32(idx_const);
    emitter.emit_load_array(var_index, desc_index);

    emit_partial_access_read_modify_write(emitter, ctx, field_vti.op_width, pa, value)?;
    emit_truncation(emitter, field_vti);

    emitter.emit_load_const_i32(idx_const);
    emitter.emit_store_array(var_index, desc_index);
    Ok(())
}

fn compile_partial_access_assignment_on_struct_field_array(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    structured: &StructuredVariable,
    array: &ArrayVariable,
    pa: &PartialAccessVariable,
    value: &Expr,
) -> Result<(), Diagnostic> {
    let subscripts: Vec<&Expr> = array.subscripts.iter().collect();

    let access = crate::compile_array::resolve_struct_field_array(ctx, structured, subscripts)?;
    let crate::compile_array::ResolvedAccess::StructFieldArrayElement {
        var_index,
        desc_index,
        field_slot_offset,
        ref dimensions,
        subscripts,
        element_type,
        ..
    } = access
    else {
        return Err(Diagnostic::not_implemented(Label::span(
            pa.span(),
            "Partial access on struct-field STRING array",
        )));
    };

    let element_vti =
        crate::compile_struct::var_type_info_for_field(&element_type).ok_or_else(|| {
            Diagnostic::not_implemented(Label::span(
                pa.span(),
                "Partial access on non-integer array element",
            ))
        })?;
    let span = pa.span();

    crate::compile_array::emit_flat_index(emitter, ctx, &subscripts, dimensions, &span)?;
    let offset_const = ctx.add_i64_constant(field_slot_offset.raw() as i64);
    emitter.emit_load_const_i64(offset_const);
    emitter.emit_add_i64();
    emitter.emit_load_array(var_index, desc_index);

    emit_partial_access_read_modify_write(emitter, ctx, element_vti.op_width, pa, value)?;
    emit_truncation(emitter, element_vti);

    let subscripts_again: Vec<&Expr> = array.subscripts.iter().collect();
    crate::compile_array::emit_flat_index(emitter, ctx, &subscripts_again, dimensions, &span)?;
    emitter.emit_load_const_i64(offset_const);
    emitter.emit_add_i64();
    emitter.emit_store_array(var_index, desc_index);
    Ok(())
}

/// A mask with the low `bits` bits set.
///
/// Built at 128 bits so that a slice exactly as wide as its operand -- `%D`
/// on a `DWORD`, `%L` on an `LWORD` -- does not overflow the shift. Callers
/// narrow the result to the operand width they AND it with.
fn low_bits_mask(bits: u32) -> u128 {
    (1u128 << bits) - 1
}

/// A partial access whose base is a `REAL` or `LREAL`.
///
/// A slice is taken from an integer or bit-string value; the analyzer is
/// expected to reject any other base before codegen sees it.
pub(super) fn partial_access_on_float(pa: &PartialAccessVariable) -> Diagnostic {
    Diagnostic::internal_error_at(Label::span(
        pa.span(),
        "Partial access on a floating-point variable",
    ))
}

/// Emits the read-modify-write sequence for partial access: clear the
/// target region and OR in the shifted RHS value.
///
/// Assumes the current base value is already on the stack.
fn emit_partial_access_read_modify_write(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    base_width: OpWidth,
    pa: &PartialAccessVariable,
    value: &Expr,
) -> Result<(), Diagnostic> {
    let access_bits = pa.size.bit_width();
    let bit_offset = pa.index.value as u32 * access_bits;

    // The slice is a bit string of its own width, so the value written is
    // compiled as one: an `%L` slice takes a 64-bit right-hand side, every
    // narrower slice a 32-bit one.
    let wide_value = access_bits > 32;
    let value_width = if wide_value {
        OpWidth::W64
    } else {
        OpWidth::W32
    };
    let value_op_type = (value_width, Signedness::Unsigned);

    match base_width {
        OpWidth::W64 => {
            let clear_mask = !((low_bits_mask(access_bits) << bit_offset) as u64) as i64;
            let clear_pool = ctx.add_i64_constant(clear_mask);
            emitter.emit_load_const_i64(clear_pool);
            emitter.emit_bit_and_64();

            compile_expr(emitter, ctx, value, value_op_type)?;
            if wide_value {
                let slice_mask = low_bits_mask(access_bits) as u64 as i64;
                let mask_pool = ctx.add_i64_constant(slice_mask);
                emitter.emit_load_const_i64(mask_pool);
                emitter.emit_bit_and_64();
            } else {
                let slice_mask = low_bits_mask(access_bits) as u32 as i32;
                let mask_pool = ctx.add_i32_constant(slice_mask);
                emitter.emit_load_const_i32(mask_pool);
                emitter.emit_bit_and_32();
                emitter.emit_builtin(opcode::builtin::CONV_U32_TO_I64);
            }
            if bit_offset > 0 {
                let shift_pool = ctx.add_i32_constant(bit_offset as i32);
                emitter.emit_load_const_i32(shift_pool);
                emitter.emit_builtin(opcode::builtin::SHL_I64);
            }
            emitter.emit_bit_or_64();
        }
        OpWidth::W32 => {
            let clear_mask = !((low_bits_mask(access_bits) << bit_offset) as u32) as i32;
            let clear_pool = ctx.add_i32_constant(clear_mask);
            emitter.emit_load_const_i32(clear_pool);
            emitter.emit_bit_and_32();

            compile_expr(emitter, ctx, value, value_op_type)?;
            let slice_mask = low_bits_mask(access_bits) as u32 as i32;
            let mask_pool = ctx.add_i32_constant(slice_mask);
            emitter.emit_load_const_i32(mask_pool);
            emitter.emit_bit_and_32();
            if bit_offset > 0 {
                let shift_pool = ctx.add_i32_constant(bit_offset as i32);
                emitter.emit_load_const_i32(shift_pool);
                emitter.emit_builtin(opcode::builtin::SHL_I32);
            }
            emitter.emit_bit_or_32();
        }
        OpWidth::F32 | OpWidth::F64 => return Err(partial_access_on_float(pa)),
    }
    Ok(())
}
