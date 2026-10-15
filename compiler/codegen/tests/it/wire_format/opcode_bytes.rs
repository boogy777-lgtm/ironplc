//! Pins the byte value of every opcode constant, one test per op-class family.

use ironplc_container::opcode;

// ---------------------------------------------------------------------------
// 1. Opcode-byte pinning.
//
// One test per op-class family. Each asserts the **literal byte value**
// of every constant in that family. Grouping by family keeps the diff
// small when a single family is renumbered.
// ---------------------------------------------------------------------------

#[test]
fn opcode_constants_when_load_const_family_then_pinned_bytes() {
    assert_eq!(opcode::LOAD_CONST_I32, 0x00);
    assert_eq!(opcode::LOAD_CONST_I64, 0x01);
    assert_eq!(opcode::LOAD_CONST_F32, 0x02);
    assert_eq!(opcode::LOAD_CONST_F64, 0x03);
}

#[test]
fn opcode_constants_when_load_bool_family_then_pinned_bytes() {
    assert_eq!(opcode::LOAD_FALSE, 0x04);
    assert_eq!(opcode::LOAD_TRUE, 0x05);
}

#[test]
fn opcode_constants_when_load_const_str_then_pinned_byte() {
    assert_eq!(opcode::LOAD_CONST_STR, 0x08);
}

#[test]
fn opcode_constants_when_load_var_family_then_pinned_bytes() {
    assert_eq!(opcode::LOAD_VAR_I32, 0x0C);
    assert_eq!(opcode::LOAD_VAR_I64, 0x0D);
    assert_eq!(opcode::LOAD_VAR_F32, 0x0E);
    assert_eq!(opcode::LOAD_VAR_F64, 0x0F);
}

#[test]
fn opcode_constants_when_store_var_family_then_pinned_bytes() {
    assert_eq!(opcode::STORE_VAR_I32, 0x10);
    assert_eq!(opcode::STORE_VAR_I64, 0x11);
    assert_eq!(opcode::STORE_VAR_F32, 0x12);
    assert_eq!(opcode::STORE_VAR_F64, 0x13);
}

#[test]
fn opcode_constants_when_indirect_then_pinned_bytes() {
    assert_eq!(opcode::LOAD_INDIRECT, 0x14);
    assert_eq!(opcode::STORE_INDIRECT, 0x18);
}

#[test]
fn opcode_constants_when_trunc_family_then_pinned_bytes() {
    assert_eq!(opcode::TRUNC_I8, 0x1C);
    assert_eq!(opcode::TRUNC_U8, 0x1D);
    assert_eq!(opcode::TRUNC_I16, 0x1E);
    assert_eq!(opcode::TRUNC_U16, 0x1F);
}

#[test]
fn opcode_constants_when_arith_family_then_pinned_bytes() {
    // ADD (op_class 0x08).
    assert_eq!(opcode::ADD_I32, 0x20);
    assert_eq!(opcode::ADD_I64, 0x21);
    assert_eq!(opcode::ADD_F32, 0x22);
    assert_eq!(opcode::ADD_F64, 0x23);
    // SUB (0x09).
    assert_eq!(opcode::SUB_I32, 0x24);
    assert_eq!(opcode::SUB_I64, 0x25);
    assert_eq!(opcode::SUB_F32, 0x26);
    assert_eq!(opcode::SUB_F64, 0x27);
    // MUL (0x0A).
    assert_eq!(opcode::MUL_I32, 0x28);
    assert_eq!(opcode::MUL_I64, 0x29);
    assert_eq!(opcode::MUL_F32, 0x2A);
    assert_eq!(opcode::MUL_F64, 0x2B);
    // NEG (0x0B).
    assert_eq!(opcode::NEG_I32, 0x2C);
    assert_eq!(opcode::NEG_I64, 0x2D);
    assert_eq!(opcode::NEG_F32, 0x2E);
    assert_eq!(opcode::NEG_F64, 0x2F);
    // DIV signed (0x0C).
    assert_eq!(opcode::DIV_I32, 0x30);
    assert_eq!(opcode::DIV_I64, 0x31);
    assert_eq!(opcode::DIV_F32, 0x32);
    assert_eq!(opcode::DIV_F64, 0x33);
    // DIV unsigned (0x0D, only int variants).
    assert_eq!(opcode::DIV_U32, 0x34);
    assert_eq!(opcode::DIV_U64, 0x35);
    // MOD signed (0x0E, int only).
    assert_eq!(opcode::MOD_I32, 0x38);
    assert_eq!(opcode::MOD_I64, 0x39);
    // MOD unsigned (0x0F, int only).
    assert_eq!(opcode::MOD_U32, 0x3C);
    assert_eq!(opcode::MOD_U64, 0x3D);
}

#[test]
fn opcode_constants_when_cmp_family_then_pinned_bytes() {
    // EQ (0x10).
    assert_eq!(opcode::EQ_I32, 0x40);
    assert_eq!(opcode::EQ_I64, 0x41);
    assert_eq!(opcode::EQ_F32, 0x42);
    assert_eq!(opcode::EQ_F64, 0x43);
    // NE (0x11).
    assert_eq!(opcode::NE_I32, 0x44);
    assert_eq!(opcode::NE_I64, 0x45);
    assert_eq!(opcode::NE_F32, 0x46);
    assert_eq!(opcode::NE_F64, 0x47);
    // LT signed (0x12).
    assert_eq!(opcode::LT_I32, 0x48);
    assert_eq!(opcode::LT_I64, 0x49);
    assert_eq!(opcode::LT_F32, 0x4A);
    assert_eq!(opcode::LT_F64, 0x4B);
    // LE signed (0x13).
    assert_eq!(opcode::LE_I32, 0x4C);
    assert_eq!(opcode::LE_I64, 0x4D);
    assert_eq!(opcode::LE_F32, 0x4E);
    assert_eq!(opcode::LE_F64, 0x4F);
    // GT signed (0x14).
    assert_eq!(opcode::GT_I32, 0x50);
    assert_eq!(opcode::GT_I64, 0x51);
    assert_eq!(opcode::GT_F32, 0x52);
    assert_eq!(opcode::GT_F64, 0x53);
    // GE signed (0x15).
    assert_eq!(opcode::GE_I32, 0x54);
    assert_eq!(opcode::GE_I64, 0x55);
    assert_eq!(opcode::GE_F32, 0x56);
    assert_eq!(opcode::GE_F64, 0x57);
    // Unsigned LT/LE/GT/GE (0x16..0x19, int only).
    assert_eq!(opcode::LT_U32, 0x58);
    assert_eq!(opcode::LT_U64, 0x59);
    assert_eq!(opcode::LE_U32, 0x5C);
    assert_eq!(opcode::LE_U64, 0x5D);
    assert_eq!(opcode::GT_U32, 0x60);
    assert_eq!(opcode::GT_U64, 0x61);
    assert_eq!(opcode::GE_U32, 0x64);
    assert_eq!(opcode::GE_U64, 0x65);
}

#[test]
fn opcode_constants_when_bitwise_family_then_pinned_bytes() {
    assert_eq!(opcode::BIT_AND_32, 0x68);
    assert_eq!(opcode::BIT_AND_64, 0x69);
    assert_eq!(opcode::BIT_OR_32, 0x6C);
    assert_eq!(opcode::BIT_OR_64, 0x6D);
    assert_eq!(opcode::BIT_XOR_32, 0x70);
    assert_eq!(opcode::BIT_XOR_64, 0x71);
    assert_eq!(opcode::BIT_NOT_32, 0x74);
    assert_eq!(opcode::BIT_NOT_64, 0x75);
}

#[test]
fn opcode_constants_when_bool_family_then_pinned_bytes() {
    // BOOL_OP consolidates AND/OR/XOR/NOT in the type-tag slots
    // (0..3) of op-class 0x1E.
    assert_eq!(opcode::BOOL_AND, 0x78);
    assert_eq!(opcode::BOOL_OR, 0x79);
    assert_eq!(opcode::BOOL_XOR, 0x7A);
    assert_eq!(opcode::BOOL_NOT, 0x7B);
}

#[test]
fn opcode_constants_when_control_flow_then_pinned_bytes() {
    assert_eq!(opcode::JMP, 0x7C);
    assert_eq!(opcode::JMP_IF_NOT, 0x80);
    assert_eq!(opcode::CALL, 0x84);
    assert_eq!(opcode::RET, 0x88);
    assert_eq!(opcode::RET_VOID, 0x8C);
}

#[test]
fn opcode_constants_when_stack_op_family_then_pinned_bytes() {
    // STACK_OP consolidates POP/DUP/SWAP in type-tag slots 0..2.
    assert_eq!(opcode::POP, 0x90);
    assert_eq!(opcode::DUP, 0x91);
    assert_eq!(opcode::SWAP, 0x92);
}

#[test]
fn opcode_constants_when_builtin_then_pinned_byte() {
    assert_eq!(opcode::BUILTIN, 0x94);
}

#[test]
fn opcode_constants_when_fb_family_then_pinned_bytes() {
    assert_eq!(opcode::FB_LOAD_INSTANCE, 0x98);
    assert_eq!(opcode::FB_STORE_PARAM, 0x9C);
    assert_eq!(opcode::FB_LOAD_PARAM, 0xA0);
    assert_eq!(opcode::FB_CALL, 0xA4);
}

#[test]
fn opcode_constants_when_method_call_then_pinned_byte() {
    assert_eq!(opcode::METHOD_CALL, 0xF8);
}

#[test]
fn opcode_constants_when_array_family_then_pinned_bytes() {
    assert_eq!(opcode::LOAD_ARRAY, 0xA8);
    assert_eq!(opcode::STORE_ARRAY, 0xAC);
    assert_eq!(opcode::LOAD_ARRAY_DEREF, 0xB0);
    assert_eq!(opcode::STORE_ARRAY_DEREF, 0xB4);
    // COPY_REGION is STORE_ARRAY at type tag 1 -- the same op class, one
    // granularity coarser -- rather than an op class of its own.
    assert_eq!(opcode::COPY_REGION, 0xAD);
}

#[test]
fn opcode_constants_when_string_family_then_pinned_bytes() {
    assert_eq!(opcode::STR_INIT, 0xB8);
    assert_eq!(opcode::STR_LOAD_VAR, 0xBC);
    assert_eq!(opcode::STR_STORE_VAR, 0xC0);
    assert_eq!(opcode::LEN_STR, 0xC4);
    assert_eq!(opcode::FIND_STR, 0xC8);
    assert_eq!(opcode::REPLACE_STR, 0xCC);
    assert_eq!(opcode::INSERT_STR, 0xD0);
    assert_eq!(opcode::DELETE_STR, 0xD4);
    assert_eq!(opcode::LEFT_STR, 0xD8);
    assert_eq!(opcode::RIGHT_STR, 0xDC);
    assert_eq!(opcode::MID_STR, 0xE0);
    assert_eq!(opcode::CONCAT_STR, 0xE4);
}

#[test]
fn opcode_constants_when_string_array_family_then_pinned_bytes() {
    assert_eq!(opcode::STR_INIT_ARRAY, 0xE8);
    assert_eq!(opcode::STR_LOAD_ARRAY_ELEM, 0xEC);
    assert_eq!(opcode::STR_STORE_ARRAY_ELEM, 0xF0);
}
