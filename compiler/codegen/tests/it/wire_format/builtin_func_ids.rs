//! Pins the permanent builtin function ids that containers encode directly.

use ironplc_container::opcode;

#[test]
fn builtin_func_ids_when_unnamed_arithmetic_builtins_then_pinned_values() {
    // The __TRUNC/__MOD lowering targets are permanent compiler/VM ABI:
    // containers encode these func_ids directly, so the values are
    // wire-format commitments and must never change.
    assert_eq!(opcode::builtin::TRUNC_F64, 0x03A3);
    assert_eq!(opcode::builtin::MOD_F64, 0x03A4);
    assert_eq!(opcode::builtin::TRUNC_F32, 0x03A5);
    assert_eq!(opcode::builtin::MOD_F32, 0x03A6);
}

#[test]
fn builtin_func_ids_when_string_to_u32_policy_block_then_pinned_values() {
    // The STRING_TO_<numeric> policy block (ADR-0049): each row is a
    // permanent compiler/VM commitment, laid out as
    // BASE + target * 8 + non_numeric * 2 + failure.
    assert_eq!(opcode::builtin::str_to_num::BASE, 0x0480);
    assert_eq!(opcode::builtin::str_to_num::END, 0x04FF);
    assert_eq!(opcode::builtin::CONV_STR_TO_U32_REJECT_TRAP, 0x0480);
    assert_eq!(opcode::builtin::CONV_STR_TO_U32_REJECT_ZERO, 0x0481);
    assert_eq!(
        opcode::builtin::CONV_STR_TO_U32_IGNORE_TRAILING_TRAP,
        0x0482
    );
    assert_eq!(
        opcode::builtin::CONV_STR_TO_U32_IGNORE_TRAILING_ZERO,
        0x0483
    );
    assert_eq!(
        opcode::builtin::CONV_STR_TO_U32_IGNORE_SURROUNDING_TRAP,
        0x0484
    );
    assert_eq!(
        opcode::builtin::CONV_STR_TO_U32_IGNORE_SURROUNDING_ZERO,
        0x0485
    );
}

#[test]
fn builtin_func_ids_when_string_to_int_policy_block_then_pinned_values() {
    // The other integer targets of the STRING_TO_<numeric> block, at the
    // positions the block reserves: even positions unsigned, the odd
    // position after each its signed counterpart, in width order 32, 8, 16,
    // 64.
    use opcode::builtin::*;
    assert_eq!(CONV_STR_TO_I32_REJECT_TRAP, 0x0488);
    assert_eq!(CONV_STR_TO_I32_REJECT_ZERO, 0x0489);
    assert_eq!(CONV_STR_TO_I32_IGNORE_TRAILING_TRAP, 0x048A);
    assert_eq!(CONV_STR_TO_I32_IGNORE_TRAILING_ZERO, 0x048B);
    assert_eq!(CONV_STR_TO_I32_IGNORE_SURROUNDING_TRAP, 0x048C);
    assert_eq!(CONV_STR_TO_I32_IGNORE_SURROUNDING_ZERO, 0x048D);

    assert_eq!(CONV_STR_TO_U8_REJECT_TRAP, 0x0490);
    assert_eq!(CONV_STR_TO_U8_REJECT_ZERO, 0x0491);
    assert_eq!(CONV_STR_TO_U8_IGNORE_TRAILING_TRAP, 0x0492);
    assert_eq!(CONV_STR_TO_U8_IGNORE_TRAILING_ZERO, 0x0493);
    assert_eq!(CONV_STR_TO_U8_IGNORE_SURROUNDING_TRAP, 0x0494);
    assert_eq!(CONV_STR_TO_U8_IGNORE_SURROUNDING_ZERO, 0x0495);

    assert_eq!(CONV_STR_TO_I8_REJECT_TRAP, 0x0498);
    assert_eq!(CONV_STR_TO_I8_REJECT_ZERO, 0x0499);
    assert_eq!(CONV_STR_TO_I8_IGNORE_TRAILING_TRAP, 0x049A);
    assert_eq!(CONV_STR_TO_I8_IGNORE_TRAILING_ZERO, 0x049B);
    assert_eq!(CONV_STR_TO_I8_IGNORE_SURROUNDING_TRAP, 0x049C);
    assert_eq!(CONV_STR_TO_I8_IGNORE_SURROUNDING_ZERO, 0x049D);

    assert_eq!(CONV_STR_TO_U16_REJECT_TRAP, 0x04A0);
    assert_eq!(CONV_STR_TO_U16_REJECT_ZERO, 0x04A1);
    assert_eq!(CONV_STR_TO_U16_IGNORE_TRAILING_TRAP, 0x04A2);
    assert_eq!(CONV_STR_TO_U16_IGNORE_TRAILING_ZERO, 0x04A3);
    assert_eq!(CONV_STR_TO_U16_IGNORE_SURROUNDING_TRAP, 0x04A4);
    assert_eq!(CONV_STR_TO_U16_IGNORE_SURROUNDING_ZERO, 0x04A5);

    assert_eq!(CONV_STR_TO_I16_REJECT_TRAP, 0x04A8);
    assert_eq!(CONV_STR_TO_I16_REJECT_ZERO, 0x04A9);
    assert_eq!(CONV_STR_TO_I16_IGNORE_TRAILING_TRAP, 0x04AA);
    assert_eq!(CONV_STR_TO_I16_IGNORE_TRAILING_ZERO, 0x04AB);
    assert_eq!(CONV_STR_TO_I16_IGNORE_SURROUNDING_TRAP, 0x04AC);
    assert_eq!(CONV_STR_TO_I16_IGNORE_SURROUNDING_ZERO, 0x04AD);

    assert_eq!(CONV_STR_TO_U64_REJECT_TRAP, 0x04B0);
    assert_eq!(CONV_STR_TO_U64_REJECT_ZERO, 0x04B1);
    assert_eq!(CONV_STR_TO_U64_IGNORE_TRAILING_TRAP, 0x04B2);
    assert_eq!(CONV_STR_TO_U64_IGNORE_TRAILING_ZERO, 0x04B3);
    assert_eq!(CONV_STR_TO_U64_IGNORE_SURROUNDING_TRAP, 0x04B4);
    assert_eq!(CONV_STR_TO_U64_IGNORE_SURROUNDING_ZERO, 0x04B5);

    assert_eq!(CONV_STR_TO_I64_REJECT_TRAP, 0x04B8);
    assert_eq!(CONV_STR_TO_I64_REJECT_ZERO, 0x04B9);
    assert_eq!(CONV_STR_TO_I64_IGNORE_TRAILING_TRAP, 0x04BA);
    assert_eq!(CONV_STR_TO_I64_IGNORE_TRAILING_ZERO, 0x04BB);
    assert_eq!(CONV_STR_TO_I64_IGNORE_SURROUNDING_TRAP, 0x04BC);
    assert_eq!(CONV_STR_TO_I64_IGNORE_SURROUNDING_ZERO, 0x04BD);
}

#[test]
fn builtin_func_ids_when_string_to_real_policy_block_then_pinned_values() {
    // The real targets of the STRING_TO_<numeric> block, after the integer
    // targets: REAL at position 8, LREAL at 9.
    use opcode::builtin::*;
    assert_eq!(CONV_STR_TO_F32_REJECT_TRAP, 0x04C0);
    assert_eq!(CONV_STR_TO_F32_REJECT_ZERO, 0x04C1);
    assert_eq!(CONV_STR_TO_F32_IGNORE_TRAILING_TRAP, 0x04C2);
    assert_eq!(CONV_STR_TO_F32_IGNORE_TRAILING_ZERO, 0x04C3);
    assert_eq!(CONV_STR_TO_F32_IGNORE_SURROUNDING_TRAP, 0x04C4);
    assert_eq!(CONV_STR_TO_F32_IGNORE_SURROUNDING_ZERO, 0x04C5);

    assert_eq!(CONV_STR_TO_F64_REJECT_TRAP, 0x04C8);
    assert_eq!(CONV_STR_TO_F64_REJECT_ZERO, 0x04C9);
    assert_eq!(CONV_STR_TO_F64_IGNORE_TRAILING_TRAP, 0x04CA);
    assert_eq!(CONV_STR_TO_F64_IGNORE_TRAILING_ZERO, 0x04CB);
    assert_eq!(CONV_STR_TO_F64_IGNORE_SURROUNDING_TRAP, 0x04CC);
    assert_eq!(CONV_STR_TO_F64_IGNORE_SURROUNDING_ZERO, 0x04CD);
}
