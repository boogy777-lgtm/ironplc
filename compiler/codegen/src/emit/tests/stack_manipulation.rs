//! Tests for emitting DUP and SWAP.

use super::*;

#[test]
fn emitter_when_emit_dup_then_emits_dup_opcode_and_increments_stack() {
    let mut em = Emitter::new();
    em.emit_dup();

    assert_eq!(em.bytecode(), &[opcode::DUP]);
    assert_eq!(em.max_stack_depth(), 1);
}

#[test]
fn emitter_when_emit_swap_then_emits_swap_opcode_and_preserves_stack() {
    let mut em = Emitter::new();
    em.emit_swap();

    assert_eq!(em.bytecode(), &[opcode::SWAP]);
    assert_eq!(em.max_stack_depth(), 0);
}
