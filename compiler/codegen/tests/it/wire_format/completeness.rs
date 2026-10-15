//! Guards that the pinned opcode registry is complete against the set of assigned opcodes.

use ironplc_container::opcode;

// ---------------------------------------------------------------------------
// Completeness guard.
//
// The per-family tests in `opcode_bytes.rs` pin the byte value of every opcode by name. This
// test proves that set is *complete*: it compares the pinned registry below to
// the set of assigned opcodes reported by `opcode::is_assigned` (which is
// derived from `instruction_size`, the match every new opcode must update).
// Adding, removing, or renumbering an opcode without editing this file fails
// this test with a clear diff -- so no opcode can change or be introduced
// without a deliberate, reviewed change to the wire-format guard.
// ---------------------------------------------------------------------------

#[test]
fn opcode_pins_when_compared_to_assigned_set_then_complete() {
    // Every opcode byte pinned by the family tests in `opcode_bytes.rs`. Keep in sync with
    // `instruction_size` in `container/src/opcode.rs`.
    let pinned: &[u8] = &[
        opcode::ADD_I32,
        opcode::SUB_I32,
        opcode::MUL_I32,
        opcode::DIV_I32,
        opcode::MOD_I32,
        opcode::NEG_I32,
        opcode::ADD_I64,
        opcode::SUB_I64,
        opcode::MUL_I64,
        opcode::DIV_I64,
        opcode::MOD_I64,
        opcode::NEG_I64,
        opcode::DIV_U32,
        opcode::MOD_U32,
        opcode::DIV_U64,
        opcode::MOD_U64,
        opcode::ADD_F32,
        opcode::SUB_F32,
        opcode::MUL_F32,
        opcode::DIV_F32,
        opcode::NEG_F32,
        opcode::ADD_F64,
        opcode::SUB_F64,
        opcode::MUL_F64,
        opcode::DIV_F64,
        opcode::NEG_F64,
        opcode::EQ_I32,
        opcode::NE_I32,
        opcode::LT_I32,
        opcode::LE_I32,
        opcode::GT_I32,
        opcode::GE_I32,
        opcode::EQ_I64,
        opcode::NE_I64,
        opcode::LT_I64,
        opcode::LE_I64,
        opcode::GT_I64,
        opcode::GE_I64,
        opcode::LT_U32,
        opcode::LE_U32,
        opcode::GT_U32,
        opcode::GE_U32,
        opcode::LT_U64,
        opcode::LE_U64,
        opcode::GT_U64,
        opcode::GE_U64,
        opcode::EQ_F32,
        opcode::NE_F32,
        opcode::LT_F32,
        opcode::LE_F32,
        opcode::GT_F32,
        opcode::GE_F32,
        opcode::EQ_F64,
        opcode::NE_F64,
        opcode::LT_F64,
        opcode::LE_F64,
        opcode::GT_F64,
        opcode::GE_F64,
        opcode::BOOL_AND,
        opcode::BOOL_OR,
        opcode::BOOL_XOR,
        opcode::BOOL_NOT,
        opcode::BIT_AND_32,
        opcode::BIT_OR_32,
        opcode::BIT_XOR_32,
        opcode::BIT_NOT_32,
        opcode::BIT_AND_64,
        opcode::BIT_OR_64,
        opcode::BIT_XOR_64,
        opcode::BIT_NOT_64,
        opcode::TRUNC_I8,
        opcode::TRUNC_U8,
        opcode::TRUNC_I16,
        opcode::TRUNC_U16,
        opcode::LOAD_INDIRECT,
        opcode::STORE_INDIRECT,
        opcode::LOAD_TRUE,
        opcode::LOAD_FALSE,
        opcode::POP,
        opcode::DUP,
        opcode::SWAP,
        opcode::RET,
        opcode::RET_VOID,
        opcode::FB_STORE_PARAM,
        opcode::FB_LOAD_PARAM,
        opcode::LOAD_CONST_I32,
        opcode::LOAD_CONST_I64,
        opcode::LOAD_CONST_F32,
        opcode::LOAD_CONST_F64,
        opcode::LOAD_CONST_STR,
        opcode::LOAD_VAR_I32,
        opcode::LOAD_VAR_I64,
        opcode::LOAD_VAR_F32,
        opcode::LOAD_VAR_F64,
        opcode::STORE_VAR_I32,
        opcode::STORE_VAR_I64,
        opcode::STORE_VAR_F32,
        opcode::STORE_VAR_F64,
        opcode::FB_LOAD_INSTANCE,
        opcode::FB_CALL,
        opcode::JMP,
        opcode::JMP_IF_NOT,
        opcode::BUILTIN,
        opcode::CALL,
        opcode::LOAD_ARRAY,
        opcode::STORE_ARRAY,
        opcode::LOAD_ARRAY_DEREF,
        opcode::STORE_ARRAY_DEREF,
        opcode::STR_INIT_ARRAY,
        opcode::STR_LOAD_ARRAY_ELEM,
        opcode::STR_STORE_ARRAY_ELEM,
        opcode::STR_LOAD_VAR,
        opcode::STR_STORE_VAR,
        opcode::LEN_STR,
        opcode::DELETE_STR,
        opcode::LEFT_STR,
        opcode::RIGHT_STR,
        opcode::MID_STR,
        opcode::STR_INIT,
        opcode::CMP_BR_I32,
        opcode::CMP_BR_I64,
        opcode::COPY_REGION,
        opcode::FIND_STR,
        opcode::REPLACE_STR,
        opcode::INSERT_STR,
        opcode::CONCAT_STR,
        opcode::METHOD_CALL,
    ];

    let mut pinned_sorted = pinned.to_vec();
    pinned_sorted.sort_unstable();
    let mut deduped = pinned_sorted.clone();
    deduped.dedup();
    assert_eq!(
        pinned_sorted, deduped,
        "two opcodes share a byte value in the pinned registry"
    );

    let assigned: Vec<u8> = (0u8..=255).filter(|&b| opcode::is_assigned(b)).collect();
    assert_eq!(
        pinned_sorted, assigned,
        "pinned opcode registry != assigned opcode set: an opcode was added, \
         removed, or renumbered without updating wire_format.rs"
    );
}
