//! Compilation of variable reads, including bit access, partial access, structure fields and array elements.

use ironplc_container::opcode;
use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::textual::{ArrayVariable, SymbolicVariableKind, Variable};

use super::partial_access_assignment::partial_access_on_float;
use super::typed_opcode::emit_load_var;
use super::variable_ref::{
    emit_load_in_out, resolve_symbolic_variable_name, resolve_variable_name, variable_span,
};
use crate::compile::{CompileContext, OpType, OpWidth, DEFAULT_OP_TYPE};
use crate::emit::Emitter;

/// Compiles a variable read expression, handling bit access.
///
/// For simple named variables, loads the variable value onto the stack.
/// For bit access (e.g., `a.0`), loads the base variable, shifts right
/// by the bit index, and masks with 1 to extract the single bit as a
/// BOOL (0 or 1).
pub(super) fn compile_variable_read(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    variable: &Variable,
    op_type: OpType,
) -> Result<(), Diagnostic> {
    match variable {
        Variable::Symbolic(SymbolicVariableKind::BitAccess(bit_access)) => {
            let bit_index = bit_access.index.value;

            // Determine the op_type of the inner integer value that we are
            // bit-accessing. For a named scalar, look it up in var_types. For
            // an array element, the element type is stored on ArrayVarInfo.
            // For a struct field, walk the struct chain and derive the op
            // type from the leaf field's IntermediateType.
            let base_op_type: OpType = match bit_access.variable.as_ref() {
                SymbolicVariableKind::Named(named) => ctx.var_op_type(&named.name),
                SymbolicVariableKind::Array(array) => {
                    let root_name = resolve_symbolic_variable_name(&array.subscripted_variable)?;
                    match ctx.array_vars.get(root_name) {
                        Some(info) => (
                            info.element_var_type_info.op_width,
                            info.element_var_type_info.signedness,
                        ),
                        None => resolve_struct_field_array_element_op_type(ctx, array)
                            .unwrap_or(DEFAULT_OP_TYPE),
                    }
                }
                SymbolicVariableKind::Structured(structured) => {
                    let (_root, _slot, field_type) = crate::compile_struct::walk_struct_chain(
                        ctx,
                        &structured.record,
                        &structured.field,
                        0,
                    )?;
                    crate::compile_struct::resolve_field_op_type(&field_type)
                        .unwrap_or(DEFAULT_OP_TYPE)
                }
                _ => DEFAULT_OP_TYPE,
            };

            // Compile the inner variable read. For a named variable this is
            // emit_load_var; for an array element it is emit_flat_index +
            // emit_load_array; etc. The existing compile_variable_read path
            // already handles each of these.
            let inner_variable: Variable = (*bit_access.variable.clone()).into();
            compile_variable_read(emitter, ctx, &inner_variable, base_op_type)?;

            // Load the bit index and shift right
            match base_op_type.0 {
                OpWidth::W64 => {
                    let pool_index = ctx.add_i64_constant(bit_index as i64);
                    emitter.emit_load_const_i64(pool_index);
                    emitter.emit_builtin(opcode::builtin::SHR_I64);
                    // AND with 1 to isolate the bit
                    let one_index = ctx.add_i64_constant(1);
                    emitter.emit_load_const_i64(one_index);
                    emitter.emit_bit_and_64();
                }
                _ => {
                    let pool_index = ctx.add_i32_constant(bit_index as i32);
                    emitter.emit_load_const_i32(pool_index);
                    emitter.emit_builtin(opcode::builtin::SHR_I32);
                    // AND with 1 to isolate the bit
                    let one_index = ctx.add_i32_constant(1);
                    emitter.emit_load_const_i32(one_index);
                    emitter.emit_bit_and_32();
                }
            }
            Ok(())
        }
        Variable::Symbolic(SymbolicVariableKind::PartialAccess(pa)) => {
            let access_bits = pa.size.bit_width();
            let bit_offset = pa.index.value as u32 * access_bits;

            let base_op_type: OpType = match pa.variable.as_ref() {
                SymbolicVariableKind::Named(named) => ctx.var_op_type(&named.name),
                SymbolicVariableKind::Array(array) => {
                    let root_name = resolve_symbolic_variable_name(&array.subscripted_variable)?;
                    match ctx.array_vars.get(root_name) {
                        Some(info) => (
                            info.element_var_type_info.op_width,
                            info.element_var_type_info.signedness,
                        ),
                        None => resolve_struct_field_array_element_op_type(ctx, array)
                            .unwrap_or(DEFAULT_OP_TYPE),
                    }
                }
                SymbolicVariableKind::Structured(structured) => {
                    let (_root, _slot, field_type) = crate::compile_struct::walk_struct_chain(
                        ctx,
                        &structured.record,
                        &structured.field,
                        0,
                    )?;
                    crate::compile_struct::resolve_field_op_type(&field_type)
                        .unwrap_or(DEFAULT_OP_TYPE)
                }
                _ => DEFAULT_OP_TYPE,
            };

            let inner_variable: Variable = (*pa.variable.clone()).into();
            compile_variable_read(emitter, ctx, &inner_variable, base_op_type)?;

            match base_op_type.0 {
                OpWidth::W64 => {
                    if bit_offset > 0 {
                        let shift_pool = ctx.add_i64_constant(bit_offset as i64);
                        emitter.emit_load_const_i64(shift_pool);
                        emitter.emit_builtin(opcode::builtin::SHR_I64);
                    }
                    if access_bits < 64 {
                        let mask = (1i64 << access_bits) - 1;
                        let mask_pool = ctx.add_i64_constant(mask);
                        emitter.emit_load_const_i64(mask_pool);
                        emitter.emit_bit_and_64();
                    }
                }
                OpWidth::W32 => {
                    if bit_offset > 0 {
                        let shift_pool = ctx.add_i32_constant(bit_offset as i32);
                        emitter.emit_load_const_i32(shift_pool);
                        emitter.emit_builtin(opcode::builtin::SHR_I32);
                    }
                    if access_bits < 32 {
                        let mask = (1i32 << access_bits) - 1;
                        let mask_pool = ctx.add_i32_constant(mask);
                        emitter.emit_load_const_i32(mask_pool);
                        emitter.emit_bit_and_32();
                    }
                }
                OpWidth::F32 | OpWidth::F64 => {
                    return Err(partial_access_on_float(pa));
                }
            }
            Ok(())
        }
        // A member of a function block instance (`timer.Q`, `timers[i].Q`):
        // the fields are stored in the data region addressed via FB_LOAD_PARAM.
        Variable::Symbolic(SymbolicVariableKind::Structured(structured))
            if crate::compile_fb_instance::names_instance(ctx, &structured.record) =>
        {
            crate::compile_fb_instance::compile_member_read(emitter, ctx, structured)
        }
        // The guard excludes `s.arr[i].field`, whose record is an array
        // element rather than a fixed-offset struct field. That shape falls
        // through to the generic `resolve_access` dispatch below.
        Variable::Symbolic(SymbolicVariableKind::Structured(structured))
            if !matches!(structured.record.as_ref(), SymbolicVariableKind::Array(_)) =>
        {
            // STRING fields are composite (multi-slot) and stored in the data
            // region, so we intercept before resolve_struct_field_access which
            // only supports single-slot (primitive/enum) fields.
            let (root_name, slot_offset, field_type) = crate::compile_struct::walk_struct_chain(
                ctx,
                &structured.record,
                &structured.field,
                0,
            )?;
            if matches!(
                &field_type,
                ironplc_analyzer::intermediate_type::IntermediateType::String { .. }
            ) {
                let struct_info = ctx.struct_vars.get(&root_name).ok_or_else(|| {
                    Diagnostic::not_implemented(Label::span(
                        structured.span(),
                        format!("Variable '{}' is not a structure", root_name),
                    ))
                })?;
                let byte_offset = struct_info.data_offset + slot_offset.raw() * 8;
                emitter.emit_str_load_var(byte_offset);
                return Ok(());
            }

            let (var_index, desc_index, slot_offset, _op_type, _field_type) =
                crate::compile_struct::resolve_struct_field_access(ctx, structured)?;
            let idx_const = ctx.add_i32_constant(slot_offset.raw() as i32);
            emitter.emit_load_const_i32(idx_const);
            emitter.emit_load_array(var_index, desc_index);
            Ok(())
        }
        _ => {
            // Check if this is a string variable (stored in data region).
            // String reads emit str_load_var to produce a buf_idx on the stack,
            // which is consumed by string assignment or string function args.
            if let Some(var_name) = resolve_variable_name(variable) {
                if let Some(info) = ctx.string_vars.get(var_name).cloned() {
                    info.emit_load(emitter, ctx);
                    return Ok(());
                }
            }

            match crate::compile_array::resolve_access(ctx, variable)? {
                crate::compile_array::ResolvedAccess::Scalar { var_index } => {
                    emit_load_var(emitter, var_index, op_type);
                }
                crate::compile_array::ResolvedAccess::InOut { ref_slot } => {
                    emit_load_in_out(emitter, ref_slot);
                }
                crate::compile_array::ResolvedAccess::ArrayElement { info, subscripts } => {
                    let arr_var_index = info.var_index;
                    let arr_desc_index = info.desc_index;
                    let is_string_elem = info.is_string_element;
                    let dim_info: Vec<_> = info
                        .dimensions
                        .iter()
                        .map(|d| crate::compile_array::DimensionInfo {
                            lower_bound: d.lower_bound,
                            size: d.size,
                            stride: d.stride,
                        })
                        .collect();
                    let span = variable_span(variable);
                    crate::compile_array::emit_flat_index(
                        emitter,
                        ctx,
                        &subscripts,
                        &dim_info,
                        &span,
                    )?;
                    if is_string_elem {
                        emitter.emit_str_load_array_elem(arr_var_index, arr_desc_index);
                    } else {
                        emitter.emit_load_array(arr_var_index, arr_desc_index);
                    }
                }
                crate::compile_array::ResolvedAccess::DerefArrayElement { info, subscripts } => {
                    let ref_var_index = info.var_index;
                    let arr_desc_index = info.desc_index;
                    let dim_info: Vec<_> = info
                        .dimensions
                        .iter()
                        .map(|d| crate::compile_array::DimensionInfo {
                            lower_bound: d.lower_bound,
                            size: d.size,
                            stride: d.stride,
                        })
                        .collect();
                    let span = variable_span(variable);
                    crate::compile_array::emit_flat_index(
                        emitter,
                        ctx,
                        &subscripts,
                        &dim_info,
                        &span,
                    )?;
                    emitter.emit_load_array_deref(ref_var_index, arr_desc_index);
                }
                crate::compile_array::ResolvedAccess::StructFieldArrayElement {
                    var_index,
                    desc_index,
                    field_slot_offset,
                    ref dimensions,
                    subscripts,
                    ..
                } => {
                    let span = variable_span(variable);
                    crate::compile_array::emit_flat_index(
                        emitter,
                        ctx,
                        &subscripts,
                        dimensions,
                        &span,
                    )?;
                    let offset_const = ctx.add_i64_constant(field_slot_offset.raw() as i64);
                    emitter.emit_load_const_i64(offset_const);
                    emitter.emit_add_i64();
                    emitter.emit_load_array(var_index, desc_index);
                }
                crate::compile_array::ResolvedAccess::StructFieldStringArrayElement(element) => {
                    element.emit_base_and_index(emitter, ctx, &variable_span(variable))?;
                    emitter.emit_str_load_array_elem(
                        element.scratch_var_index,
                        element.string_desc_index,
                    );
                }
            }
            Ok(())
        }
    }
}

/// Resolves the element op_type for an array that is a struct field.
///
/// Used by the bit-access read path when the bit-access base is
/// `Array(Structured(...))` — the array is not in `ctx.array_vars` so we
/// derive the element type from the struct field's `IntermediateType::Array`.
fn resolve_struct_field_array_element_op_type(
    ctx: &CompileContext,
    array: &ArrayVariable,
) -> Option<OpType> {
    if let SymbolicVariableKind::Structured(structured) = array.subscripted_variable.as_ref() {
        let (_root, _slot, field_type) =
            crate::compile_struct::walk_struct_chain(ctx, &structured.record, &structured.field, 0)
                .ok()?;
        if let ironplc_analyzer::intermediate_type::IntermediateType::Array {
            element_type, ..
        } = &field_type
        {
            return crate::compile_struct::resolve_field_op_type(element_type);
        }
    }
    None
}
