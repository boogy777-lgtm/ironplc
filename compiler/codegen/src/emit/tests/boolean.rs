//! Tests for emitting boolean instructions and boolean constants.

use super::*;

#[test]
fn emitter_when_bool_and_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_bool_and();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::BOOL_AND
        ]
    );
}

#[test]
fn emitter_when_bool_and_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_var_i32(VarIndex::new(1)); // stack: 2
    em.emit_bool_and(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(2)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_bool_or_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_bool_or();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::BOOL_OR
        ]
    );
}

#[test]
fn emitter_when_bool_or_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_var_i32(VarIndex::new(1)); // stack: 2
    em.emit_bool_or(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(2)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_bool_xor_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_bool_xor();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::BOOL_XOR
        ]
    );
}

#[test]
fn emitter_when_bool_xor_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_var_i32(VarIndex::new(1)); // stack: 2
    em.emit_bool_xor(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(2)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_bool_not_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_var_i32(VarIndex::new(0));
    em.emit_bool_not();

    assert_eq!(
        em.bytecode(),
        &[opcode::LOAD_VAR_I32, 0x00, 0x00, opcode::BOOL_NOT]
    );
}

#[test]
fn emitter_when_bool_not_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := NOT x
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_bool_not(); // stack: 1 (pop 1, push 1)
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 1);
}

#[test]
fn emitter_when_load_true_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_true();

    assert_eq!(em.bytecode(), &[opcode::LOAD_TRUE]);
}

#[test]
fn emitter_when_load_true_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    em.emit_load_true(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(0)); // stack: 0

    assert_eq!(em.max_stack_depth(), 1);
}

#[test]
fn emitter_when_load_false_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_false();

    assert_eq!(em.bytecode(), &[opcode::LOAD_FALSE]);
}

#[test]
fn emitter_when_load_false_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    em.emit_load_false(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(0)); // stack: 0

    assert_eq!(em.max_stack_depth(), 1);
}
