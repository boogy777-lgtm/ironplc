//! Tests for emitting function calls and returns.

use super::*;

#[test]
fn emitter_when_call_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0); // arg 1
    em.emit_load_const_i32(1); // arg 2
    em.emit_call(FunctionId::new(2), 2, VarIndex::new(5), 0); // CALL function 2, 2 params, var_offset=5

    // LOAD_CONST pool:0, LOAD_CONST pool:1, CALL func:2 var_offset:5
    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::LOAD_CONST_I32,
            0x01,
            0x00,
            opcode::CALL,
            0x02,
            0x00,
            0x05,
            0x00
        ]
    );
}

#[test]
fn emitter_when_call_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0); // stack: 1
    em.emit_load_const_i32(1); // stack: 2
    em.emit_call(FunctionId::new(2), 2, VarIndex::new(5), 0); // pop 2 args, push 1 result = stack: 1
    em.emit_store_var_i32(VarIndex::new(0)); // stack: 0

    assert_eq!(em.max_stack_depth(), 2);
}

#[test]
fn emitter_when_ret_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_var_i32(VarIndex::new(0));
    em.emit_ret();

    assert_eq!(
        em.bytecode(),
        &[opcode::LOAD_VAR_I32, 0x00, 0x00, opcode::RET]
    );
}
