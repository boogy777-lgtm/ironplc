//! Tests for opcode encoding, decoding, instruction sizes and built-in argument counts.

use super::*;

#[test]
fn instruction_size_when_unknown_opcode_then_returns_one() {
    // 0xFE is not assigned; the default arm must return 1 so that
    // disassembly does not get stuck in an infinite loop.
    assert_eq!(instruction_size(0xFE), 1);
}

#[test]
fn decode_when_unassigned_byte_then_returns_none() {
    assert_eq!(Instruction::decode(0xFE), None);
}

#[test]
fn decode_when_assigned_opcode_then_mnemonic_is_its_name() {
    assert_eq!(Instruction::decode(ADD_I32).unwrap().mnemonic, "ADD_I32");
    assert_eq!(
        Instruction::decode(STORE_VAR_F64).unwrap().mnemonic,
        "STORE_VAR_F64"
    );
}

#[test]
fn decode_when_opcode_assigned_then_every_one_decodes() {
    // The table gives an opcode its name and operands together, so
    // `is_assigned` and `decode` cannot disagree about which bytes are
    // opcodes.
    for op in 0..=u8::MAX {
        assert_eq!(
            is_assigned(op),
            Instruction::decode(op).is_some(),
            "opcode 0x{op:02X}"
        );
    }
}

#[test]
fn instruction_size_when_opcode_assigned_then_matches_wire_format() {
    // One opcode per instruction shape, pinned to the byte counts the
    // wire format has always used. These are asserted independently of
    // the table so a mistyped operand layout is caught here.
    assert_eq!(instruction_size(ADD_I32), 1);
    assert_eq!(instruction_size(FB_STORE_PARAM), 2);
    assert_eq!(instruction_size(LOAD_VAR_I32), 3);
    assert_eq!(instruction_size(CALL), 5);
    assert_eq!(instruction_size(LEN_STR), 5);
    assert_eq!(instruction_size(STR_INIT), 8);
    assert_eq!(instruction_size(CMP_BR_I32), 8);
    assert_eq!(instruction_size(METHOD_CALL), 8);
    assert_eq!(instruction_size(FIND_STR), 9);
}

#[test]
fn decode_when_no_operand_opcode_then_operands_are_empty() {
    assert_eq!(Instruction::decode(ADD_I32).unwrap().operands, &[]);
}

#[test]
fn decode_when_multi_operand_opcode_then_operands_are_in_order() {
    assert_eq!(
        Instruction::decode(CMP_BR_I32).unwrap().operands,
        &[
            Operand::CmpOp,
            Operand::VarIndex,
            Operand::ConstIndex,
            Operand::JumpOffset
        ]
    );
}

#[test]
fn mux_info_when_valid_arity_then_returns_some_count() {
    assert_eq!(builtin::mux_info(builtin::MUX_I32_BASE + 3), Some(3));
    assert_eq!(builtin::mux_info(builtin::MUX_F64_BASE + 5), Some(5));
}

#[test]
fn mux_info_when_arity_below_two_then_returns_none() {
    assert_eq!(builtin::mux_info(builtin::MUX_I32_BASE), None);
    assert_eq!(builtin::mux_info(builtin::MUX_I32_BASE + 1), None);
}

#[test]
fn mux_info_when_not_a_mux_id_then_returns_none() {
    assert_eq!(builtin::mux_info(0x0001), None);
}

#[test]
fn arg_count_when_mux_id_then_returns_n_plus_one() {
    // MUX pops n IN values + 1 K selector.
    assert_eq!(builtin::arg_count(builtin::MUX_I32_BASE + 3), 4);
}

#[test]
#[should_panic(expected = "unknown builtin function ID")]
fn arg_count_when_unknown_function_id_then_panics() {
    let _ = builtin::arg_count(0xFFFF);
}

#[test]
fn arg_count_when_unnamed_arithmetic_builtins_then_counts_match_dispatch() {
    assert_eq!(builtin::arg_count(builtin::TRUNC_F64), 1);
    assert_eq!(builtin::arg_count(builtin::MOD_F64), 2);
    assert_eq!(builtin::arg_count(builtin::TRUNC_F32), 1);
    assert_eq!(builtin::arg_count(builtin::MOD_F32), 2);
}
