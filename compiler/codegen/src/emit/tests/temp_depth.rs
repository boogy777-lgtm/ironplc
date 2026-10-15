//! Tests for tracking the temporary string buffer depth.

use super::*;

#[test]
fn emitter_when_no_string_ops_then_zero_temp_depth() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_store_var_i32(VarIndex::new(0));
    em.emit_builtin(opcode::builtin::EXPT_I32);

    assert_eq!(em.max_temp_depth(), 0);
}

#[test]
fn emitter_when_each_allocating_string_op_then_raises_temp_depth() {
    let mut em = Emitter::new();
    em.emit_load_const_str(0); // 1
    em.emit_str_load_var(0); // 2
    em.emit_str_load_array_elem(VarIndex::new(0), 0); // 3
    em.emit_replace_str(0, 0); // 4
    em.emit_insert_str(0, 0); // 5
    em.emit_delete_str(0); // 6
    em.emit_left_str(0); // 7
    em.emit_right_str(0); // 8
    em.emit_mid_str(0); // 9
    em.emit_concat_str(0, 0); // 10
    em.emit_builtin(opcode::builtin::CONV_I32_TO_STR); // 11

    assert_eq!(em.max_temp_depth(), 11);
}

#[test]
fn emitter_when_string_consuming_ops_then_no_temp_depth() {
    let mut em = Emitter::new();
    em.emit_len_str(0);
    em.emit_find_str(0, 0);
    em.emit_builtin(opcode::builtin::CMP_STR);
    em.emit_builtin(opcode::builtin::CONV_STR_TO_I32);

    assert_eq!(em.max_temp_depth(), 0);
}

/// The shape a string statement compiles to: allocate a buffer, spill
/// it into the data region, and be back where we started. Repeating it
/// -- what a loop body does at run time -- must not raise the peak,
/// because the VM hands the same slot back each time.
#[test]
fn emitter_when_alloc_and_consume_repeated_then_temp_depth_stays_one() {
    let mut em = Emitter::new();
    em.emit_concat_str(0, 0);
    em.emit_str_store_var(0);
    em.emit_concat_str(0, 0);
    em.emit_str_store_var(0);
    em.emit_concat_str(0, 0);
    em.emit_str_store_var(0);

    assert_eq!(em.max_temp_depth(), 1);
}

/// `STR_STORE_ARRAY_ELEM` is the other consuming opcode, so it has to
/// release too: the later CONCAT reuses the slot rather than stacking
/// on top of it.
#[test]
fn emitter_when_string_array_element_stored_then_temp_depth_stays_one() {
    let mut em = Emitter::new();
    em.emit_str_load_var(0);
    em.emit_load_const_i32(0);
    em.emit_str_store_array_elem(VarIndex::new(0), 0);
    em.emit_concat_str(0, 0);
    em.emit_str_store_var(0);

    assert_eq!(em.max_temp_depth(), 1);
}
