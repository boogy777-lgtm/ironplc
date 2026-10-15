//! Tests for emitting scalar load, store, arithmetic and return instructions.

use super::*;

#[test]
fn emitter_when_load_const_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);

    assert_eq!(em.bytecode(), &[opcode::LOAD_CONST_I32, 0x00, 0x00]);
}

#[test]
fn emitter_when_load_var_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_var_i32(VarIndex::new(1));

    assert_eq!(em.bytecode(), &[opcode::LOAD_VAR_I32, 0x01, 0x00]);
}

#[test]
fn emitter_when_store_var_then_correct_bytecode() {
    let mut em = Emitter::new();
    // Need something on the stack first
    em.emit_load_const_i32(0);
    em.emit_store_var_i32(VarIndex::new(0));

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::STORE_VAR_I32,
            0x00,
            0x00
        ]
    );
}

#[test]
fn emitter_when_add_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_add_i32();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::ADD_I32
        ]
    );
}

#[test]
fn emitter_when_sub_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_sub_i32();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::SUB_I32
        ]
    );
}

#[test]
fn emitter_when_sub_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := x - 5
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(0); // stack: 2
    em.emit_sub_i32(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_mul_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_mul_i32();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::MUL_I32
        ]
    );
}

#[test]
fn emitter_when_mul_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := x * 5
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(0); // stack: 2
    em.emit_mul_i32(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_div_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_div_i32();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::DIV_I32
        ]
    );
}

#[test]
fn emitter_when_div_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := x / 5
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(0); // stack: 2
    em.emit_div_i32(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_mod_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_mod_i32();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::MOD_I32
        ]
    );
}

#[test]
fn emitter_when_mod_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := x MOD 5
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(0); // stack: 2
    em.emit_mod_i32(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_neg_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_var_i32(VarIndex::new(0));
    em.emit_neg_i32();

    assert_eq!(
        em.bytecode(),
        &[opcode::LOAD_VAR_I32, 0x00, 0x00, opcode::NEG_I32]
    );
}

#[test]
fn emitter_when_neg_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := -x
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_neg_i32(); // stack: 1 (pop 1, push 1)
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 1);
}

#[test]
fn emitter_when_ret_void_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_ret_void();

    assert_eq!(em.bytecode(), &[opcode::RET_VOID]);
}

#[test]
fn emitter_when_steel_thread_then_tracks_max_stack_depth() {
    let mut em = Emitter::new();
    // x := 10
    em.emit_load_const_i32(0); // stack: 1
    em.emit_store_var_i32(VarIndex::new(0)); // stack: 0
                                             // y := x + 32
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(1); // stack: 2
    em.emit_add_i32(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0
    em.emit_ret_void();

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_large_pool_index_then_little_endian() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(256);

    // 256 in little-endian u16 is [0x00, 0x01]
    assert_eq!(em.bytecode(), &[opcode::LOAD_CONST_I32, 0x00, 0x01]);
}
