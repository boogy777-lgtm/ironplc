//! Tests for emitting comparison instructions.

use super::*;

#[test]
fn emitter_when_eq_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_eq_i32();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::EQ_I32
        ]
    );
}

#[test]
fn emitter_when_eq_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := x = 5
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(0); // stack: 2
    em.emit_eq_i32(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_ne_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_ne_i32();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::NE_I32
        ]
    );
}

#[test]
fn emitter_when_ne_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := x <> 5
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(0); // stack: 2
    em.emit_ne_i32(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_lt_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_lt_i32();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::LT_I32
        ]
    );
}

#[test]
fn emitter_when_lt_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := x < 5
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(0); // stack: 2
    em.emit_lt_i32(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_le_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_le_i32();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::LE_I32
        ]
    );
}

#[test]
fn emitter_when_le_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := x <= 5
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(0); // stack: 2
    em.emit_le_i32(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_gt_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_gt_i32();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::GT_I32
        ]
    );
}

#[test]
fn emitter_when_gt_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := x > 5
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(0); // stack: 2
    em.emit_gt_i32(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_ge_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_ge_i32();

    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::GE_I32
        ]
    );
}

#[test]
fn emitter_when_ge_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    // y := x >= 5
    em.emit_load_var_i32(VarIndex::new(0)); // stack: 1
    em.emit_load_const_i32(0); // stack: 2
    em.emit_ge_i32(); // stack: 1
    em.emit_store_var_i32(VarIndex::new(1)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}
