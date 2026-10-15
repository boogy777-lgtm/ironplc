//! Tests for emitting array element loads and stores.

use super::*;

#[test]
fn emitter_when_load_array_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0); // push flat index
    em.emit_load_array(VarIndex::new(3), 7); // var_index=3, desc_index=7

    // LOAD_CONST pool:0, LOAD_ARRAY var:3 desc:7
    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_ARRAY,
            0x03,
            0x00,
            0x07,
            0x00
        ]
    );
}

#[test]
fn emitter_when_load_array_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0); // stack: 1 (flat index)
    em.emit_load_array(VarIndex::new(3), 7); // stack: 1 (pop index, push value)
    em.emit_store_var_i32(VarIndex::new(0)); // stack: 0

    assert_eq!(em.max_stack_depth(), 1);
}

#[test]
fn emitter_when_store_array_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0); // push value
    em.emit_load_const_i32(1); // push flat index
    em.emit_store_array(VarIndex::new(5), 2); // var_index=5, desc_index=2

    // LOAD_CONST pool:0, LOAD_CONST pool:1, STORE_ARRAY var:5 desc:2
    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::STORE_ARRAY,
            0x05,
            0x00,
            0x02,
            0x00
        ]
    );
}

#[test]
fn emitter_when_store_array_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0); // stack: 1 (value)
    em.emit_load_const_i32(1); // stack: 2 (flat index)
    em.emit_store_array(VarIndex::new(5), 2); // stack: 0 (pops 2)

    assert_eq!(em.max_stack_depth(), 2);
}
