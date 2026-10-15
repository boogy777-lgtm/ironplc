//! Compilation of assignments to a single bit of a variable, array element or structure field.

use ironplc_container::opcode;
use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::textual::{
    ArrayVariable, BitAccessVariable, Expr, StructuredVariable, SymbolicVariableKind,
};

use super::compile_expr;
use super::typed_opcode::{emit_load_var, emit_store_var, emit_truncation};
use super::variable_ref::resolve_symbolic_variable_name;
use crate::compile::{CompileContext, OpWidth, DEFAULT_OP_TYPE};
use crate::emit::Emitter;

/// Compiles a bit access assignment using read-modify-write.
///
/// `target.N := value` is compiled as:
///   1. Load the base variable
///   2. AND with clear mask (~(1 << N)) to clear the target bit
///   3. Compile the RHS value
///   4. AND with 1 to ensure it's 0 or 1
///   5. Left-shift by N
///   6. OR with the cleared variable to set the bit
///   7. Store back to the base variable
pub(crate) fn compile_bit_access_assignment(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    bit_access: &BitAccessVariable,
    value: &Expr,
) -> Result<(), Diagnostic> {
    // Array-element base: use the array read/write opcodes which take the
    // flat index on the stack. The index is emitted twice (once for the
    // load, once for the store) because no DUP opcode is available.
    if let SymbolicVariableKind::Array(array) = bit_access.variable.as_ref() {
        return compile_bit_access_assignment_on_array(emitter, ctx, array, bit_access, value);
    }

    // Struct-field base: `s.field.n := rhs;`. Uses LOAD_ARRAY/STORE_ARRAY on
    // the underlying struct-as-flat-slot-array with a compile-time slot index.
    if let SymbolicVariableKind::Structured(structured) = bit_access.variable.as_ref() {
        return compile_bit_access_assignment_on_struct_field(
            emitter, ctx, structured, bit_access, value,
        );
    }

    let base_name = resolve_symbolic_variable_name(&bit_access.variable)?;
    let var_index = ctx.var_index(base_name)?;
    let base_op_type = ctx.var_op_type(base_name);
    let bit_index = bit_access.index.value as u32;

    match base_op_type.0 {
        OpWidth::W64 => {
            let clear_mask = !(1i64 << bit_index);
            let clear_pool = ctx.add_i64_constant(clear_mask);

            // Load base var and clear the target bit.
            emit_load_var(emitter, var_index, base_op_type);
            emitter.emit_load_const_i64(clear_pool);
            emitter.emit_bit_and_64();

            // Compile the RHS, mask to 1 bit, shift into position.
            compile_expr(emitter, ctx, value, DEFAULT_OP_TYPE)?;
            let one_pool = ctx.add_i32_constant(1);
            emitter.emit_load_const_i32(one_pool);
            emitter.emit_bit_and_32();
            // Widen to 64-bit before shifting.
            emitter.emit_builtin(opcode::builtin::CONV_U32_TO_I64);
            let shift_pool = ctx.add_i32_constant(bit_index as i32);
            emitter.emit_load_const_i32(shift_pool);
            emitter.emit_builtin(opcode::builtin::SHL_I64);

            // OR the shifted bit into the cleared variable.
            emitter.emit_bit_or_64();
        }
        _ => {
            let clear_mask = !(1i32 << bit_index);
            let clear_pool = ctx.add_i32_constant(clear_mask);

            // Load base var and clear the target bit.
            emit_load_var(emitter, var_index, base_op_type);
            emitter.emit_load_const_i32(clear_pool);
            emitter.emit_bit_and_32();

            // Compile the RHS, mask to 1 bit, shift into position.
            compile_expr(emitter, ctx, value, DEFAULT_OP_TYPE)?;
            let one_pool = ctx.add_i32_constant(1);
            emitter.emit_load_const_i32(one_pool);
            emitter.emit_bit_and_32();
            let shift_pool = ctx.add_i32_constant(bit_index as i32);
            emitter.emit_load_const_i32(shift_pool);
            emitter.emit_builtin(opcode::builtin::SHL_I32);

            // OR the shifted bit into the cleared variable.
            emitter.emit_bit_or_32();
        }
    }

    // Truncate if needed and store back.
    if let Some(ti) = ctx.var_type_info(base_name) {
        emit_truncation(emitter, ti);
    }
    emit_store_var(emitter, var_index, base_op_type);
    Ok(())
}

/// Compiles a bit-access assignment where the base is an array element:
/// `arr[i].n := rhs;`. Uses LOAD_ARRAY/STORE_ARRAY; the flat index is emitted
/// twice (once for the read, once for the write) because no DUP opcode is
/// available. Supports both W32 element widths (BYTE/WORD/DWORD, INT/DINT)
/// and W64 element widths (LWORD/LINT/ULINT).
fn compile_bit_access_assignment_on_array(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    array: &ArrayVariable,
    bit_access: &BitAccessVariable,
    value: &Expr,
) -> Result<(), Diagnostic> {
    let bit_index = bit_access.index.value as u32;

    // If the array base is a struct field, delegate to the struct-field-array
    // handler which uses flat_index + field_slot_offset addressing.
    if let SymbolicVariableKind::Structured(structured) = array.subscripted_variable.as_ref() {
        return compile_bit_access_assignment_on_struct_field_array(
            emitter, ctx, structured, array, bit_access, value,
        );
    }

    let root_name = resolve_symbolic_variable_name(&array.subscripted_variable)?;
    let info = ctx.array_vars.get(root_name).ok_or_else(|| {
        Diagnostic::not_implemented(Label::span(
            bit_access.span(),
            "Bit access on non-trivial array base is not yet supported",
        ))
    })?;
    // Copy scalar fields out of the borrow so ctx can be used mutably.
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

    let span = bit_access.span();

    // 1. Compute flat index and load the element.
    crate::compile_array::emit_flat_index(emitter, ctx, &subscripts, &dim_info, &span)?;
    emitter.emit_load_array(arr_var_index, arr_desc_index);

    // 2/3. Clear the target bit, then OR in the shifted RHS bit. Width-
    //      dependent: LWORD/LINT elements use 64-bit ops; everything else
    //      (BYTE/WORD/DWORD/INT/DINT) uses 32-bit ops.
    if element_vti.op_width == OpWidth::W64 {
        let clear_mask = !(1i64 << bit_index);
        let clear_pool = ctx.add_i64_constant(clear_mask);
        emitter.emit_load_const_i64(clear_pool);
        emitter.emit_bit_and_64();

        compile_expr(emitter, ctx, value, DEFAULT_OP_TYPE)?;
        let one_pool = ctx.add_i32_constant(1);
        emitter.emit_load_const_i32(one_pool);
        emitter.emit_bit_and_32();
        // Widen to 64-bit before shifting into a high bit position.
        emitter.emit_builtin(opcode::builtin::CONV_U32_TO_I64);
        let shift_pool = ctx.add_i32_constant(bit_index as i32);
        emitter.emit_load_const_i32(shift_pool);
        emitter.emit_builtin(opcode::builtin::SHL_I64);
        emitter.emit_bit_or_64();
    } else {
        let clear_mask = !(1i32 << bit_index);
        let clear_pool = ctx.add_i32_constant(clear_mask);
        emitter.emit_load_const_i32(clear_pool);
        emitter.emit_bit_and_32();

        compile_expr(emitter, ctx, value, DEFAULT_OP_TYPE)?;
        let one_pool = ctx.add_i32_constant(1);
        emitter.emit_load_const_i32(one_pool);
        emitter.emit_bit_and_32();
        let shift_pool = ctx.add_i32_constant(bit_index as i32);
        emitter.emit_load_const_i32(shift_pool);
        emitter.emit_builtin(opcode::builtin::SHL_I32);
        emitter.emit_bit_or_32();
    }

    // 4. Truncate the new element value to fit its storage width.
    emit_truncation(emitter, element_vti);

    // 5. Recompute the flat index and STORE back.
    crate::compile_array::emit_flat_index(emitter, ctx, &subscripts, &dim_info, &span)?;
    emitter.emit_store_array(arr_var_index, arr_desc_index);

    Ok(())
}

/// Compiles a bit-access assignment where the base is a struct field:
/// `s.field.n := rhs;`. Uses LOAD_ARRAY/STORE_ARRAY on the struct's flat
/// slot-array with the field's compile-time slot offset as the index.
fn compile_bit_access_assignment_on_struct_field(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    structured: &StructuredVariable,
    bit_access: &BitAccessVariable,
    value: &Expr,
) -> Result<(), Diagnostic> {
    let bit_index = bit_access.index.value as u32;

    let (var_index, desc_index, slot_offset, _op_type, field_type) =
        crate::compile_struct::resolve_struct_field_access(ctx, structured)?;
    let field_vti =
        crate::compile_struct::var_type_info_for_field(&field_type).ok_or_else(|| {
            Diagnostic::not_implemented(Label::span(
                structured.field.span(),
                "Bit access on non-integer struct field is not supported",
            ))
        })?;

    // Index constant is the same for load and store; add once and reuse
    // across both emitter calls.
    let idx_const = ctx.add_i32_constant(slot_offset.raw() as i32);

    // 1. Load the current field value.
    emitter.emit_load_const_i32(idx_const);
    emitter.emit_load_array(var_index, desc_index);

    // 2/3. Clear target bit and OR in the shifted RHS bit.
    if field_vti.op_width == OpWidth::W64 {
        let clear_mask = !(1i64 << bit_index);
        let clear_pool = ctx.add_i64_constant(clear_mask);
        emitter.emit_load_const_i64(clear_pool);
        emitter.emit_bit_and_64();

        compile_expr(emitter, ctx, value, DEFAULT_OP_TYPE)?;
        let one_pool = ctx.add_i32_constant(1);
        emitter.emit_load_const_i32(one_pool);
        emitter.emit_bit_and_32();
        emitter.emit_builtin(opcode::builtin::CONV_U32_TO_I64);
        let shift_pool = ctx.add_i32_constant(bit_index as i32);
        emitter.emit_load_const_i32(shift_pool);
        emitter.emit_builtin(opcode::builtin::SHL_I64);
        emitter.emit_bit_or_64();
    } else {
        let clear_mask = !(1i32 << bit_index);
        let clear_pool = ctx.add_i32_constant(clear_mask);
        emitter.emit_load_const_i32(clear_pool);
        emitter.emit_bit_and_32();

        compile_expr(emitter, ctx, value, DEFAULT_OP_TYPE)?;
        let one_pool = ctx.add_i32_constant(1);
        emitter.emit_load_const_i32(one_pool);
        emitter.emit_bit_and_32();
        let shift_pool = ctx.add_i32_constant(bit_index as i32);
        emitter.emit_load_const_i32(shift_pool);
        emitter.emit_builtin(opcode::builtin::SHL_I32);
        emitter.emit_bit_or_32();
    }

    // 4. Truncate the new field value to fit its storage width (e.g., SINT
    //    stored in a W32 slot needs sign-extension/truncation).
    emit_truncation(emitter, field_vti);

    // 5. Re-emit the slot index and STORE back.
    emitter.emit_load_const_i32(idx_const);
    emitter.emit_store_array(var_index, desc_index);

    Ok(())
}

/// Compiles a bit-access assignment where the base is an array element
/// nested inside a struct field: `s.arr[i].n := rhs;`.
///
/// Combines the struct-field-array resolution (flat_index + slot_offset)
/// with the bit-level read-modify-write pattern.
fn compile_bit_access_assignment_on_struct_field_array(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    structured: &StructuredVariable,
    array: &ArrayVariable,
    bit_access: &BitAccessVariable,
    value: &Expr,
) -> Result<(), Diagnostic> {
    let bit_index = bit_access.index.value as u32;
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
            bit_access.span(),
            "Bit access on struct-field STRING array is not supported",
        )));
    };

    let element_vti =
        crate::compile_struct::var_type_info_for_field(&element_type).ok_or_else(|| {
            Diagnostic::not_implemented(Label::span(
                bit_access.span(),
                "Bit access on non-integer array element type",
            ))
        })?;

    let span = bit_access.span();

    // 1. Load the current element: flat_index + field_slot_offset → LOAD_ARRAY.
    crate::compile_array::emit_flat_index(emitter, ctx, &subscripts, dimensions, &span)?;
    let offset_const = ctx.add_i64_constant(field_slot_offset.raw() as i64);
    emitter.emit_load_const_i64(offset_const);
    emitter.emit_add_i64();
    emitter.emit_load_array(var_index, desc_index);

    // 2/3. Clear target bit, then OR in the shifted RHS bit.
    if element_vti.op_width == OpWidth::W64 {
        let clear_mask = !(1i64 << bit_index);
        let clear_pool = ctx.add_i64_constant(clear_mask);
        emitter.emit_load_const_i64(clear_pool);
        emitter.emit_bit_and_64();

        compile_expr(emitter, ctx, value, DEFAULT_OP_TYPE)?;
        let one_pool = ctx.add_i32_constant(1);
        emitter.emit_load_const_i32(one_pool);
        emitter.emit_bit_and_32();
        emitter.emit_builtin(opcode::builtin::CONV_U32_TO_I64);
        let shift_pool = ctx.add_i32_constant(bit_index as i32);
        emitter.emit_load_const_i32(shift_pool);
        emitter.emit_builtin(opcode::builtin::SHL_I64);
        emitter.emit_bit_or_64();
    } else {
        let clear_mask = !(1i32 << bit_index);
        let clear_pool = ctx.add_i32_constant(clear_mask);
        emitter.emit_load_const_i32(clear_pool);
        emitter.emit_bit_and_32();

        compile_expr(emitter, ctx, value, DEFAULT_OP_TYPE)?;
        let one_pool = ctx.add_i32_constant(1);
        emitter.emit_load_const_i32(one_pool);
        emitter.emit_bit_and_32();
        let shift_pool = ctx.add_i32_constant(bit_index as i32);
        emitter.emit_load_const_i32(shift_pool);
        emitter.emit_builtin(opcode::builtin::SHL_I32);
        emitter.emit_bit_or_32();
    }

    // 4. Truncate the new element value.
    emit_truncation(emitter, element_vti);

    // 5. Re-compute flat_index + field_slot_offset and STORE back.
    let subscripts_again: Vec<&Expr> = array.subscripts.iter().collect();
    crate::compile_array::emit_flat_index(emitter, ctx, &subscripts_again, dimensions, &span)?;
    emitter.emit_load_const_i64(offset_const);
    emitter.emit_add_i64();
    emitter.emit_store_array(var_index, desc_index);

    Ok(())
}
