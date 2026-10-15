//! Emission of the typed opcode that matches an operand width and signedness.

use ironplc_container::{opcode, VarIndex};
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::{CompareOp, Expr, Operator};
use paste::paste;

use super::storage_bits;
use crate::compile::{CompileContext, OpType, OpWidth, Signedness, VarTypeInfo};
use crate::emit::Emitter;

// --- Typed opcode emission helpers ---
//
// Each helper selects the correct opcode based on the operation type
// (width and/or signedness).

pub(crate) fn emit_truncation(emitter: &mut Emitter, type_info: VarTypeInfo) {
    match (
        type_info.op_width,
        type_info.signedness,
        type_info.storage_bits,
    ) {
        (OpWidth::W32, Signedness::Signed, 8) => emitter.emit_trunc_i8(),
        (OpWidth::W32, Signedness::Signed, 16) => emitter.emit_trunc_i16(),
        (OpWidth::W32, Signedness::Unsigned, 8) => emitter.emit_trunc_u8(),
        (OpWidth::W32, Signedness::Unsigned, 16) => emitter.emit_trunc_u16(),
        // 32-bit and 64-bit types fill their native width; no truncation needed.
        _ => {}
    }
}

pub(crate) fn emit_load_var(emitter: &mut Emitter, var_index: VarIndex, op_type: OpType) {
    match op_type.0 {
        OpWidth::W32 => emitter.emit_load_var_i32(var_index),
        OpWidth::W64 => emitter.emit_load_var_i64(var_index),
        OpWidth::F32 => emitter.emit_load_var_f32(var_index),
        OpWidth::F64 => emitter.emit_load_var_f64(var_index),
    }
}

pub(crate) fn emit_store_var(emitter: &mut Emitter, var_index: VarIndex, op_type: OpType) {
    match op_type.0 {
        OpWidth::W32 => emitter.emit_store_var_i32(var_index),
        OpWidth::W64 => emitter.emit_store_var_i64(var_index),
        OpWidth::F32 => emitter.emit_store_var_f32(var_index),
        OpWidth::F64 => emitter.emit_store_var_f64(var_index),
    }
}

// The `emit_<op>` dispatch functions below map an operand `OpType` to the
// matching typed `Emitter` method. They share a handful of identical shapes,
// so the three `macro_rules!` generators fold each shape into a single
// definition. Every generated function keeps the same `pub(crate)` name and
// `(emitter, op_type)` signature that existing call sites and function-pointer
// uses in `compile_call.rs` / `compile_stmt.rs` rely on.

/// Generates a width-only dispatch `emit_<stem>` mapping the operand width to
/// `emit_<stem>_{i32,i64,f32,f64}`. Used for operators whose signedness does
/// not change the opcode.
macro_rules! emit_width_op {
    ($stem:ident) => {
        paste! {
            pub(crate) fn [<emit_ $stem>](emitter: &mut Emitter, op_type: OpType) {
                match op_type.0 {
                    OpWidth::W32 => emitter.[<emit_ $stem _i32>](),
                    OpWidth::W64 => emitter.[<emit_ $stem _i64>](),
                    OpWidth::F32 => emitter.[<emit_ $stem _f32>](),
                    OpWidth::F64 => emitter.[<emit_ $stem _f64>](),
                }
            }
        }
    };
}

/// Generates a width+signedness dispatch `emit_<stem>` mapping
/// `(OpWidth, Signedness)` to `emit_<stem>_{i32,u32,i64,u64,f32,f64}`. Integer
/// widths pick signed/unsigned variants; float widths ignore signedness.
macro_rules! emit_signed_op {
    ($stem:ident) => {
        paste! {
            pub(crate) fn [<emit_ $stem>](emitter: &mut Emitter, op_type: OpType) {
                match op_type {
                    (OpWidth::W32, Signedness::Signed) => emitter.[<emit_ $stem _i32>](),
                    (OpWidth::W32, Signedness::Unsigned) => emitter.[<emit_ $stem _u32>](),
                    (OpWidth::W64, Signedness::Signed) => emitter.[<emit_ $stem _i64>](),
                    (OpWidth::W64, Signedness::Unsigned) => emitter.[<emit_ $stem _u64>](),
                    (OpWidth::F32, _) => emitter.[<emit_ $stem _f32>](),
                    (OpWidth::F64, _) => emitter.[<emit_ $stem _f64>](),
                }
            }
        }
    };
}

/// Generates a logical/bitwise dispatch `emit_<stem>` for AND/OR/XOR. Unsigned
/// integer operands emit the bitwise opcode (`emit_bit_<stem>_32/_64`); every
/// other type (BOOL and signed integers) emits `emit_bool_<stem>`.
macro_rules! emit_logical_op {
    ($stem:ident) => {
        paste! {
            pub(crate) fn [<emit_ $stem>](emitter: &mut Emitter, op_type: OpType) {
                match op_type {
                    (OpWidth::W32, Signedness::Unsigned) => emitter.[<emit_bit_ $stem _32>](),
                    (OpWidth::W64, Signedness::Unsigned) => emitter.[<emit_bit_ $stem _64>](),
                    _ => emitter.[<emit_bool_ $stem>](),
                }
            }
        }
    };
}

emit_width_op!(add);
emit_width_op!(sub);
emit_width_op!(mul);
emit_width_op!(neg);
emit_width_op!(eq);
emit_width_op!(ne);

emit_signed_op!(div);
emit_signed_op!(lt);
emit_signed_op!(le);
emit_signed_op!(gt);
emit_signed_op!(ge);

emit_logical_op!(and);
emit_logical_op!(or);
emit_logical_op!(xor);

/// Emits the opcode of a binary arithmetic operator for operands of `op_type`.
///
/// The operator expression and the function form of the operator (`ADD`,
/// `SUB`, ...) both come through here, so the two spellings cannot emit
/// differently.
pub(crate) fn emit_arithmetic_op(emitter: &mut Emitter, op: &Operator, op_type: OpType) {
    match op {
        Operator::Add => emit_add(emitter, op_type),
        Operator::Sub => emit_sub(emitter, op_type),
        Operator::Mul => emit_mul(emitter, op_type),
        Operator::Div => emit_div(emitter, op_type),
        Operator::Mod => emit_mod(emitter, op_type),
        Operator::Pow => emit_pow(emitter, op_type),
    }
}

/// Emits the opcode of a comparison, logical or bitwise operator for
/// operands of `op_type`.
///
/// The operator expression and the function form of the operator (`GT`,
/// `AND`, ...) both come through here, so the two spellings cannot emit
/// differently.
pub(crate) fn emit_compare_op(emitter: &mut Emitter, op: &CompareOp, op_type: OpType) {
    match op {
        CompareOp::Eq => emit_eq(emitter, op_type),
        CompareOp::Ne => emit_ne(emitter, op_type),
        CompareOp::Lt => emit_lt(emitter, op_type),
        CompareOp::Gt => emit_gt(emitter, op_type),
        CompareOp::LtEq => emit_le(emitter, op_type),
        CompareOp::GtEq => emit_ge(emitter, op_type),
        CompareOp::And => emit_and(emitter, op_type),
        CompareOp::Or => emit_or(emitter, op_type),
        CompareOp::Xor => emit_xor(emitter, op_type),
        // Only reached for non-BOOL operands, where there is no boolean to
        // short-circuit on and the operator degenerates to its eager
        // counterpart. See `ShortCircuitOp::for_expr`.
        CompareOp::AndThen => emit_and(emitter, op_type),
        CompareOp::OrElse => emit_or(emitter, op_type),
    }
}

/// Emits the complement of the value of `term`, already on the stack.
///
/// The unsigned operation types are the bit strings, so they take the
/// bitwise complement, truncated back to `term`'s storage width where the
/// 32-bit complement would widen a BYTE or WORD. Every other type takes the
/// boolean complement. The `NOT` operator and the `NOT` function form both
/// come through here, so the two spellings cannot emit differently.
pub(crate) fn emit_not(
    emitter: &mut Emitter,
    ctx: &CompileContext,
    op_type: OpType,
    term: &Expr,
) -> Result<(), Diagnostic> {
    match op_type {
        (OpWidth::W32, Signedness::Unsigned) => {
            emitter.emit_bit_not_32();
            match storage_bits(ctx, term)? {
                8 => emitter.emit_trunc_u8(),
                16 => emitter.emit_trunc_u16(),
                _ => {}
            }
        }
        (OpWidth::W64, Signedness::Unsigned) => emitter.emit_bit_not_64(),
        _ => emitter.emit_bool_not(),
    }
    Ok(())
}

// Hand-written one-offs that do not fit the generated shapes above:

/// MOD dispatch. Fits the width+signedness shape for integers, but IEC 61131-3
/// MOD is integer-only, so the float arms are a no-op rather than a call to a
/// (nonexistent) `emit_mod_f32`. The analyzer rejects a float MOD before
/// codegen: the function form through the `MOD` row of the operator-form
/// table (P4026), the operator through `rule_operator_operand_type_check`
/// (P4049).
pub(crate) fn emit_mod(emitter: &mut Emitter, op_type: OpType) {
    match op_type {
        (OpWidth::W32, Signedness::Signed) => emitter.emit_mod_i32(),
        (OpWidth::W32, Signedness::Unsigned) => emitter.emit_mod_u32(),
        (OpWidth::W64, Signedness::Signed) => emitter.emit_mod_i64(),
        (OpWidth::W64, Signedness::Unsigned) => emitter.emit_mod_u64(),
        // Unreachable from source: the analyzer has already rejected a float
        // MOD (see above). Emitting nothing leaves the operand stack
        // unbalanced, which the bytecode verifier reports as an internal
        // error, so a gap in the analyzer cannot produce a silent miscompile.
        (OpWidth::F32, _) | (OpWidth::F64, _) => {}
    }
}

/// POW dispatch. Width-only shape, but emits `EXPT_*` builtins rather than
/// dedicated `emit_pow_*` opcodes, so it stays hand-written.
pub(crate) fn emit_pow(emitter: &mut Emitter, op_type: OpType) {
    match op_type.0 {
        OpWidth::W32 => emitter.emit_builtin(opcode::builtin::EXPT_I32),
        OpWidth::W64 => emitter.emit_builtin(opcode::builtin::EXPT_I64),
        OpWidth::F32 => emitter.emit_builtin(opcode::builtin::EXPT_F32),
        OpWidth::F64 => emitter.emit_builtin(opcode::builtin::EXPT_F64),
    }
}
