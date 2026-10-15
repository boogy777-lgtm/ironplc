//! Tests for emitting jumps and patching their targets.

use super::*;

#[test]
fn emitter_when_jmp_then_correct_bytecode() {
    let mut em = Emitter::new();
    let label = em.create_label();
    em.emit_jmp(label);
    em.bind_label(label);

    // JMP with offset 0 (target is immediately after the instruction)
    assert_eq!(em.bytecode(), &[opcode::JMP, 0x00, 0x00]);
}

#[test]
fn emitter_when_jmp_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0); // stack: 1
    let label = em.create_label();
    em.emit_jmp(label);
    em.bind_label(label);
    em.emit_store_var_i32(VarIndex::new(0)); // stack: 0

    // JMP does not change stack depth
    assert_eq!(em.max_stack_depth(), 1);
}

#[test]
fn emitter_when_jmp_if_not_then_correct_bytecode() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0); // push condition
    let label = em.create_label();
    em.emit_jmp_if_not(label);
    em.bind_label(label);

    // LOAD_CONST_I32 pool:0, JMP_IF_NOT offset:0
    assert_eq!(
        em.bytecode(),
        &[
            opcode::LOAD_CONST_I32,
            0x00,
            0x00,
            opcode::JMP_IF_NOT,
            0x00,
            0x00
        ]
    );
}

#[test]
fn emitter_when_jmp_if_not_then_tracks_stack_depth() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0); // stack: 1
    let label = em.create_label();
    em.emit_jmp_if_not(label); // stack: 0 (pops condition)
    em.bind_label(label);

    assert_eq!(em.max_stack_depth(), 1);
}

#[test]
fn emitter_when_forward_jump_then_patches_correctly() {
    let mut em = Emitter::new();
    let label = em.create_label();
    em.emit_jmp(label);
    // Emit 4 bytes of filler (a LOAD_CONST_I32)
    em.emit_load_const_i32(0);
    em.bind_label(label);

    // JMP offset should be 3 (skip over the LOAD_CONST_I32 which is 3 bytes)
    assert_eq!(
        em.bytecode(),
        &[opcode::JMP, 0x03, 0x00, opcode::LOAD_CONST_I32, 0x00, 0x00]
    );
}
