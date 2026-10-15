//! Checks the structural invariants of the `[op_class:6][type:2]` encoding scheme.

use ironplc_container::opcode;

// ---------------------------------------------------------------------------
// 2. Encoding-scheme tests.
//
// These don't repeat the pinning in `opcode_bytes.rs`; they verify *structural*
// invariants of the `[op_class:6][type:2]` scheme. A renumbering that
// preserved every byte value but broke the scheme would be unusual,
// but a renumbering that broke a single family's invariants is
// plausible — these tests catch that.
// ---------------------------------------------------------------------------

#[test]
fn encoding_when_load_var_family_then_consistent_op_class() {
    // All LOAD_VAR_* share the same high 6 bits.
    let class = opcode::LOAD_VAR_I32 >> 2;
    assert_eq!(opcode::LOAD_VAR_I64 >> 2, class);
    assert_eq!(opcode::LOAD_VAR_F32 >> 2, class);
    assert_eq!(opcode::LOAD_VAR_F64 >> 2, class);
    // Low 2 bits map to type tag.
    assert_eq!(opcode::LOAD_VAR_I32 & 0b11, opcode::T_I32);
    assert_eq!(opcode::LOAD_VAR_I64 & 0b11, opcode::T_I64);
    assert_eq!(opcode::LOAD_VAR_F32 & 0b11, opcode::T_F32);
    assert_eq!(opcode::LOAD_VAR_F64 & 0b11, opcode::T_F64);
}

#[test]
fn encoding_when_store_var_family_then_consistent_op_class() {
    let class = opcode::STORE_VAR_I32 >> 2;
    assert_eq!(opcode::STORE_VAR_I64 >> 2, class);
    assert_eq!(opcode::STORE_VAR_F32 >> 2, class);
    assert_eq!(opcode::STORE_VAR_F64 >> 2, class);
    assert_eq!(opcode::STORE_VAR_I32 & 0b11, opcode::T_I32);
    assert_eq!(opcode::STORE_VAR_I64 & 0b11, opcode::T_I64);
    assert_eq!(opcode::STORE_VAR_F32 & 0b11, opcode::T_F32);
    assert_eq!(opcode::STORE_VAR_F64 & 0b11, opcode::T_F64);
}

#[test]
fn encoding_when_arithmetic_family_then_consistent_op_class() {
    for (i32_op, i64_op, f32_op, f64_op) in [
        (
            opcode::ADD_I32,
            opcode::ADD_I64,
            opcode::ADD_F32,
            opcode::ADD_F64,
        ),
        (
            opcode::SUB_I32,
            opcode::SUB_I64,
            opcode::SUB_F32,
            opcode::SUB_F64,
        ),
        (
            opcode::MUL_I32,
            opcode::MUL_I64,
            opcode::MUL_F32,
            opcode::MUL_F64,
        ),
        (
            opcode::NEG_I32,
            opcode::NEG_I64,
            opcode::NEG_F32,
            opcode::NEG_F64,
        ),
        (
            opcode::DIV_I32,
            opcode::DIV_I64,
            opcode::DIV_F32,
            opcode::DIV_F64,
        ),
    ] {
        let class = i32_op >> 2;
        assert_eq!(i64_op >> 2, class, "i64 vs i32 op_class mismatch");
        assert_eq!(f32_op >> 2, class, "f32 vs i32 op_class mismatch");
        assert_eq!(f64_op >> 2, class, "f64 vs i32 op_class mismatch");
        assert_eq!(i32_op & 0b11, opcode::T_I32);
        assert_eq!(i64_op & 0b11, opcode::T_I64);
        assert_eq!(f32_op & 0b11, opcode::T_F32);
        assert_eq!(f64_op & 0b11, opcode::T_F64);
    }
}

#[test]
fn encoding_when_comparison_family_then_consistent_op_class() {
    for (i32_op, i64_op, f32_op, f64_op) in [
        (
            opcode::EQ_I32,
            opcode::EQ_I64,
            opcode::EQ_F32,
            opcode::EQ_F64,
        ),
        (
            opcode::NE_I32,
            opcode::NE_I64,
            opcode::NE_F32,
            opcode::NE_F64,
        ),
        (
            opcode::LT_I32,
            opcode::LT_I64,
            opcode::LT_F32,
            opcode::LT_F64,
        ),
        (
            opcode::LE_I32,
            opcode::LE_I64,
            opcode::LE_F32,
            opcode::LE_F64,
        ),
        (
            opcode::GT_I32,
            opcode::GT_I64,
            opcode::GT_F32,
            opcode::GT_F64,
        ),
        (
            opcode::GE_I32,
            opcode::GE_I64,
            opcode::GE_F32,
            opcode::GE_F64,
        ),
    ] {
        let class = i32_op >> 2;
        assert_eq!(i64_op >> 2, class);
        assert_eq!(f32_op >> 2, class);
        assert_eq!(f64_op >> 2, class);
        assert_eq!(i32_op & 0b11, opcode::T_I32);
        assert_eq!(i64_op & 0b11, opcode::T_I64);
        assert_eq!(f32_op & 0b11, opcode::T_F32);
        assert_eq!(f64_op & 0b11, opcode::T_F64);
    }
}

#[test]
fn encoding_when_consolidated_op_class_then_type_tag_selects_member() {
    // BOOL_OP: op_class consolidates 4 family members in type-tag slots.
    let bool_class = opcode::BOOL_AND >> 2;
    assert_eq!(opcode::BOOL_OR >> 2, bool_class);
    assert_eq!(opcode::BOOL_XOR >> 2, bool_class);
    assert_eq!(opcode::BOOL_NOT >> 2, bool_class);
    assert_eq!(opcode::BOOL_AND & 0b11, 0);
    assert_eq!(opcode::BOOL_OR & 0b11, 1);
    assert_eq!(opcode::BOOL_XOR & 0b11, 2);
    assert_eq!(opcode::BOOL_NOT & 0b11, 3);

    // STACK_OP: similar consolidation for POP/DUP/SWAP.
    let stack_class = opcode::POP >> 2;
    assert_eq!(opcode::DUP >> 2, stack_class);
    assert_eq!(opcode::SWAP >> 2, stack_class);
    assert_eq!(opcode::POP & 0b11, 0);
    assert_eq!(opcode::DUP & 0b11, 1);
    assert_eq!(opcode::SWAP & 0b11, 2);
}

#[test]
fn encoding_when_op_class_census_taken_then_one_slot_free() {
    // The op-class field is 6 bits: 64 slots, and `encode_opcode` shifts
    // the class left by two, so a 65th class would alias another class's
    // bytes rather than fail. ADR-0033 planned on 23 free slots and the
    // family consolidations that would have bought them; only BOOL_OP and
    // STACK_OP were folded, so the real budget is one slot.
    //
    // This test is the tripwire that keeps the budget honest: spending
    // 0x3F fails here, and a 65th class fails to compile against the
    // assertion in `encode_opcode`.
    let mut classes: Vec<u8> = (0u8..=255)
        .filter(|&b| opcode::is_assigned(b))
        .map(|b| b >> 2)
        .collect();
    classes.sort_unstable();
    classes.dedup();

    let highest = *classes.last().expect("at least one op class is assigned");
    assert!(
        highest <= opcode::MAX_OP_CLASS,
        "op class 0x{highest:02X} does not fit the 6-bit field"
    );
    assert_eq!(classes.len(), 63, "op classes in use");
    assert_eq!(highest, 0x3E, "highest op class in use");
    assert_eq!(
        64 - classes.len(),
        1,
        "free op-class slots -- consolidate a family behind sub-opcode \
         dispatch before claiming another top-level operation"
    );
}

#[test]
fn encoding_when_decode_opcode_then_round_trips() {
    // The decode/encode helpers are part of the wire-format contract:
    // they let consumers (verifier, disassembler) interpret the byte
    // layout without per-opcode knowledge.
    for op in [
        opcode::LOAD_VAR_I32,
        opcode::STORE_VAR_F64,
        opcode::ADD_I64,
        opcode::EQ_F32,
        opcode::JMP,
        opcode::RET_VOID,
    ] {
        let (class, tag) = opcode::decode_opcode(op);
        assert_eq!(opcode::encode_opcode(class, tag), op);
        assert!(class < 64, "op_class must fit in 6 bits");
        assert!(tag < 4, "type_tag must fit in 2 bits");
    }
}
