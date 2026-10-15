//! Tests for emitting BUILTIN calls.

use super::*;

#[test]
fn emitter_when_builtin_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_builtin(opcode::builtin::EXPT_I32);

    // LOAD_CONST pool:0, LOAD_CONST pool:1, BUILTIN 0x0340
    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::BUILTIN,
            0x40,
            0x03
        ]
    );
}

#[test]
fn emitter_when_builtin_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := x ** 5
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(0); // stack: 2
    em.emit_builtin(opcode::builtin::EXPT_I32); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_builtin_1_arg_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := ABS(x)
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_builtin(opcode::builtin::ABS_I32); // stack: 1 (pop 1, push 1)
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 1);
}

#[test]
fn emitter_when_builtin_3_arg_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := LIMIT(mn, x, mx)
    em.emit_load_const_i32(0); // stack: 1
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 2
    em.emit_load_const_i32(1); // stack: 3
    em.emit_builtin(opcode::builtin::LIMIT_I32); // stack: 1 (pop 3, push 1)
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 3);
}
