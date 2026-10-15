//! Tests for constructing an emitter.

use super::*;

#[test]
fn emitter_default_when_constructed_then_matches_new() {
    let mut default_em: Emitter = Default::default();
    let mut new_em = Emitter::new();

    assert_eq!(default_em.bytecode(), new_em.bytecode());
    assert_eq!(default_em.max_stack_depth(), new_em.max_stack_depth());
}
