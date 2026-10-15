//! Emits scalar loads, stores, arithmetic, comparison, logic and stack-manipulation instructions.

use ironplc_container::opcode;
use ironplc_container::VarIndex;

use super::Emitter;

impl Emitter {
    // --- Push ops (no operand, push 1) ---
    emit_push_op!(emit_load_true, opcode::LOAD_TRUE);
    emit_push_op!(emit_load_false, opcode::LOAD_FALSE);
    /// Emits DUP (duplicates top of stack). Net: +1.
    pub fn emit_dup(&mut self) {
        self.emit_opcode(opcode::DUP);
        self.push_stack(1);
    }

    // --- Load ops (u16 operand, push 1) ---
    emit_load_u16!(emit_load_const_i32, opcode::LOAD_CONST_I32);
    emit_load_u16!(emit_load_const_i64, opcode::LOAD_CONST_I64);
    emit_load_u16!(emit_load_const_f32, opcode::LOAD_CONST_F32);
    emit_load_u16!(emit_load_const_f64, opcode::LOAD_CONST_F64);
    emit_load_var_index!(emit_load_var_i32, opcode::LOAD_VAR_I32);
    emit_load_var_index!(emit_load_var_i64, opcode::LOAD_VAR_I64);
    emit_load_var_index!(emit_load_var_f32, opcode::LOAD_VAR_F32);
    emit_load_var_index!(emit_load_var_f64, opcode::LOAD_VAR_F64);

    // --- Store ops (VarIndex operand, pop 1) ---
    emit_store_var_index!(emit_store_var_i32, opcode::STORE_VAR_I32);
    emit_store_var_index!(emit_store_var_i64, opcode::STORE_VAR_I64);
    emit_store_var_index!(emit_store_var_f32, opcode::STORE_VAR_F32);
    emit_store_var_index!(emit_store_var_f64, opcode::STORE_VAR_F64);

    // --- Binary ops (pops 2, pushes 1 = net pop 1) ---
    emit_binop!(emit_add_i32, opcode::ADD_I32);
    emit_binop!(emit_sub_i32, opcode::SUB_I32);
    emit_binop!(emit_mul_i32, opcode::MUL_I32);
    emit_binop!(emit_div_i32, opcode::DIV_I32);
    emit_binop!(emit_mod_i32, opcode::MOD_I32);
    emit_binop!(emit_add_i64, opcode::ADD_I64);
    emit_binop!(emit_sub_i64, opcode::SUB_I64);
    emit_binop!(emit_mul_i64, opcode::MUL_I64);
    emit_binop!(emit_div_i64, opcode::DIV_I64);
    emit_binop!(emit_mod_i64, opcode::MOD_I64);
    emit_binop!(emit_div_u32, opcode::DIV_U32);
    emit_binop!(emit_mod_u32, opcode::MOD_U32);
    emit_binop!(emit_div_u64, opcode::DIV_U64);
    emit_binop!(emit_mod_u64, opcode::MOD_U64);
    emit_binop!(emit_add_f32, opcode::ADD_F32);
    emit_binop!(emit_sub_f32, opcode::SUB_F32);
    emit_binop!(emit_mul_f32, opcode::MUL_F32);
    emit_binop!(emit_div_f32, opcode::DIV_F32);
    emit_binop!(emit_add_f64, opcode::ADD_F64);
    emit_binop!(emit_sub_f64, opcode::SUB_F64);
    emit_binop!(emit_mul_f64, opcode::MUL_F64);
    emit_binop!(emit_div_f64, opcode::DIV_F64);
    emit_binop!(emit_eq_i32, opcode::EQ_I32);
    emit_binop!(emit_ne_i32, opcode::NE_I32);
    emit_binop!(emit_lt_i32, opcode::LT_I32);
    emit_binop!(emit_le_i32, opcode::LE_I32);
    emit_binop!(emit_gt_i32, opcode::GT_I32);
    emit_binop!(emit_ge_i32, opcode::GE_I32);
    emit_binop!(emit_eq_i64, opcode::EQ_I64);
    emit_binop!(emit_ne_i64, opcode::NE_I64);
    emit_binop!(emit_lt_i64, opcode::LT_I64);
    emit_binop!(emit_le_i64, opcode::LE_I64);
    emit_binop!(emit_gt_i64, opcode::GT_I64);
    emit_binop!(emit_ge_i64, opcode::GE_I64);
    emit_binop!(emit_lt_u32, opcode::LT_U32);
    emit_binop!(emit_le_u32, opcode::LE_U32);
    emit_binop!(emit_gt_u32, opcode::GT_U32);
    emit_binop!(emit_ge_u32, opcode::GE_U32);
    emit_binop!(emit_lt_u64, opcode::LT_U64);
    emit_binop!(emit_le_u64, opcode::LE_U64);
    emit_binop!(emit_gt_u64, opcode::GT_U64);
    emit_binop!(emit_ge_u64, opcode::GE_U64);
    emit_binop!(emit_eq_f32, opcode::EQ_F32);
    emit_binop!(emit_ne_f32, opcode::NE_F32);
    emit_binop!(emit_lt_f32, opcode::LT_F32);
    emit_binop!(emit_le_f32, opcode::LE_F32);
    emit_binop!(emit_gt_f32, opcode::GT_F32);
    emit_binop!(emit_ge_f32, opcode::GE_F32);
    emit_binop!(emit_eq_f64, opcode::EQ_F64);
    emit_binop!(emit_ne_f64, opcode::NE_F64);
    emit_binop!(emit_lt_f64, opcode::LT_F64);
    emit_binop!(emit_le_f64, opcode::LE_F64);
    emit_binop!(emit_gt_f64, opcode::GT_F64);
    emit_binop!(emit_ge_f64, opcode::GE_F64);
    emit_binop!(emit_bool_and, opcode::BOOL_AND);
    emit_binop!(emit_bool_or, opcode::BOOL_OR);
    emit_binop!(emit_bool_xor, opcode::BOOL_XOR);
    emit_binop!(emit_bit_and_32, opcode::BIT_AND_32);
    emit_binop!(emit_bit_or_32, opcode::BIT_OR_32);
    emit_binop!(emit_bit_xor_32, opcode::BIT_XOR_32);
    emit_binop!(emit_bit_and_64, opcode::BIT_AND_64);
    emit_binop!(emit_bit_or_64, opcode::BIT_OR_64);
    emit_binop!(emit_bit_xor_64, opcode::BIT_XOR_64);

    // --- Stack manipulation ops ---
    /// Emits SWAP (swaps top two values). Net: 0.
    pub fn emit_swap(&mut self) {
        self.emit_opcode(opcode::SWAP);
    }

    // --- Unary ops (pops 1, pushes 1 = no stack change) ---
    emit_unaryop!(emit_neg_i32, opcode::NEG_I32);
    emit_unaryop!(emit_neg_i64, opcode::NEG_I64);
    emit_unaryop!(emit_neg_f32, opcode::NEG_F32);
    emit_unaryop!(emit_neg_f64, opcode::NEG_F64);
    emit_unaryop!(emit_bool_not, opcode::BOOL_NOT);
    emit_unaryop!(emit_bit_not_32, opcode::BIT_NOT_32);
    emit_unaryop!(emit_bit_not_64, opcode::BIT_NOT_64);
    emit_unaryop!(emit_trunc_i8, opcode::TRUNC_I8);
    emit_unaryop!(emit_trunc_u8, opcode::TRUNC_U8);
    emit_unaryop!(emit_trunc_i16, opcode::TRUNC_I16);
    emit_unaryop!(emit_trunc_u16, opcode::TRUNC_U16);

    /// Emits POP (discards top of stack).
    pub fn emit_pop(&mut self) {
        self.emit_opcode(opcode::POP);
        self.pop_stack(1);
    }
}
