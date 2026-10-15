//! Pins the exact byte sequences that small compiled programs encode to, per instruction shape.

use ironplc_container::opcode;
use ironplc_parser::options::CompilerOptions;

use crate::common::parse_and_compile;

// ---------------------------------------------------------------------------
// 3. Golden encoding tests — one per shape in `instruction_size`.
//
// Each test compiles a tiny program that exercises a specific
// encoding shape and asserts the **exact** byte sequence. These guard
// operand widths, little-endian operand encoding, and per-shape
// layout. A change to any of these would fail here with a
// byte-for-byte diff.
// ---------------------------------------------------------------------------

/// Returns the bytecode of the program's main entry function.
fn bytecode_of(source: &str) -> Vec<u8> {
    let container = parse_and_compile(source, &CompilerOptions::default());
    container
        .code
        .get_function_bytecode(ironplc_container::FunctionId::new(1))
        .unwrap()
        .to_vec()
}

#[test]
fn wire_when_one_byte_arith_then_no_operand_bytes() {
    // SUB_I32 is a 1-byte op (opcode only, no operand).
    let bc = bytecode_of(
        "
PROGRAM main
  VAR
    a : DINT;
    b : DINT;
  END_VAR
  a := a - b;
END_PROGRAM
",
    );
    // Last 5 bytes: SUB_I32 (1) + STORE_VAR_I32 (3) + RET_VOID (1).
    let len = bc.len();
    assert_eq!(bc[len - 5], 0x24, "SUB_I32 is one byte at expected offset");
    assert_eq!(
        &bc[len - 4..len - 1],
        &[0x10, 0x00, 0x00],
        "STORE_VAR_I32 var:0"
    );
    assert_eq!(bc[len - 1], 0x8C, "RET_VOID");
}

#[test]
fn wire_when_three_byte_load_var_then_le_u16_operand() {
    // LOAD_VAR_I32 var:1 — opcode + LE u16 operand 0x0001.
    let bc = bytecode_of(
        "
PROGRAM main
  VAR
    a : DINT;
    b : DINT;
  END_VAR
  a := b;
END_PROGRAM
",
    );
    // First 3 bytes: LOAD_VAR_I32 followed by LE u16.
    assert_eq!(
        &bc[0..3],
        &[0x0C, 0x01, 0x00],
        "LOAD_VAR_I32 var:1 (LE u16)"
    );
}

#[test]
fn wire_when_three_byte_load_const_then_le_u16_pool_index() {
    // LOAD_CONST_I32 pool:0 — opcode + LE u16.
    let bc = bytecode_of(
        "
PROGRAM main
  VAR
    a : DINT;
  END_VAR
  a := 42;
END_PROGRAM
",
    );
    assert_eq!(&bc[0..3], &[0x00, 0x00, 0x00], "LOAD_CONST_I32 pool:0");
}

#[test]
fn wire_when_jmp_then_le_i16_signed_offset() {
    // WHILE loop with a non-fusable boolean-variable condition (so it
    // falls back to the LOAD_VAR + JMP_IF_NOT shape rather than the
    // fused CMP_BR_I32 path) validates LE i16 encoding for both
    // JMP_IF_NOT (forward) and JMP (backward).
    let bc = bytecode_of(
        "
PROGRAM main
  VAR
    flag : BOOL;
    x : DINT;
  END_VAR
  WHILE flag DO
    x := x - 1;
  END_WHILE;
END_PROGRAM
",
    );
    // Layout:
    //   0: LOAD_VAR_I32 var:0 (flag)
    //   3: JMP_IF_NOT +13 -> 19
    //   6: LOAD_VAR_I32 var:1 (x)
    //   9: LOAD_CONST_I32 pool:0 (1)
    //  12: SUB_I32
    //  13: STORE_VAR_I32 var:1
    //  16: JMP -19 -> 0
    //  19: RET_VOID
    assert_eq!(bc[3], 0x80, "JMP_IF_NOT");
    assert_eq!(i16::from_le_bytes([bc[4], bc[5]]), 13, "forward LE i16");
    assert_eq!(bc[16], 0x7C, "JMP");
    assert_eq!(i16::from_le_bytes([bc[17], bc[18]]), -19, "backward LE i16");
}

#[test]
fn wire_when_seven_byte_copy_region_then_three_le_u16_operands() {
    // COPY_REGION emits opcode + LE u16 dst_var + LE u16 dst_desc + LE u16
    // src_desc. It is the only 7-byte shape.
    let bc = bytecode_of(
        "
PROGRAM main
  VAR
    x : ARRAY[1..2] OF DINT;
    y : ARRAY[1..2] OF DINT;
  END_VAR
  x := y;
END_PROGRAM
",
    );
    assert_eq!(opcode::instruction_size(opcode::COPY_REGION), 7);
    let pos = bc
        .iter()
        .position(|&b| b == opcode::COPY_REGION)
        .expect("whole-array assignment emits COPY_REGION");
    assert!(
        pos + 7 <= bc.len(),
        "COPY_REGION has 6 trailing operand bytes"
    );

    // x is var 0 and y is var 1. Both arrays are ARRAY[1..2] OF DINT, so
    // `add_array_descriptor` dedupes them onto one descriptor index.
    assert_eq!(
        u16::from_le_bytes([bc[pos + 1], bc[pos + 2]]),
        0,
        "dst_var is x"
    );
    let dst_desc = u16::from_le_bytes([bc[pos + 3], bc[pos + 4]]);
    let src_desc = u16::from_le_bytes([bc[pos + 5], bc[pos + 6]]);
    assert_eq!(dst_desc, src_desc, "identical types share a descriptor");

    // The source offset is delivered by a preceding LOAD_VAR_I32 of y.
    assert_eq!(bc[pos - 3], opcode::LOAD_VAR_I32);
    assert_eq!(
        u16::from_le_bytes([bc[pos - 2], bc[pos - 1]]),
        1,
        "src is y"
    );
}

#[test]
fn wire_when_five_byte_call_then_two_le_u16_operands() {
    // CALL emits opcode + LE u16 func_id + LE u16 var_offset.
    // The CALL appears in the program function (the caller), whose
    // FunctionId varies depending on how many functions are declared.
    let container = parse_and_compile(
        "
FUNCTION ADD_ONE : DINT
VAR_INPUT
    x : DINT;
END_VAR
    ADD_ONE := x + 1;
END_FUNCTION

PROGRAM main
VAR
    r : DINT;
END_VAR
    r := ADD_ONE(5);
END_PROGRAM
",
        &CompilerOptions::default(),
    );
    // Walk every function looking for CALL. Any function may contain
    // it; we just need one occurrence to validate the wire shape.
    let (call_bc, call_pos) = container
        .code
        .functions
        .iter()
        .find_map(|entry| {
            let bc = container.code.get_function_bytecode(entry.function_id)?;
            bc.iter().position(|&b| b == opcode::CALL).map(|p| (bc, p))
        })
        .unwrap();
    assert_eq!(opcode::instruction_size(opcode::CALL), 5);
    assert!(
        call_pos + 5 <= call_bc.len(),
        "CALL has 4 trailing operand bytes"
    );
    // Decode trailing operands: two LE u16 values. We don't assert
    // exact values (codegen may renumber functions/vars) but they
    // must decode without panic.
    let _func_id = u16::from_le_bytes([call_bc[call_pos + 1], call_bc[call_pos + 2]]);
    let _var_offset = u16::from_le_bytes([call_bc[call_pos + 3], call_bc[call_pos + 4]]);
}

#[test]
fn wire_when_eight_byte_str_init_then_u32_plus_u16_plus_u8_operands() {
    // STR_INIT is an 8-byte shape (op + u32 data_offset + u16 max_len + u8
    // char_width). It appears in the program-init function (FunctionId 0),
    // not main.
    let container = parse_and_compile(
        "
PROGRAM main
  VAR
    s : STRING[10];
  END_VAR
END_PROGRAM
",
        &CompilerOptions::default(),
    );
    let init_bc = container
        .code
        .get_function_bytecode(ironplc_container::FunctionId::new(0))
        .unwrap();
    // Find STR_INIT (0xB8). Validate the 7 trailing operand bytes
    // decode as LE u32 + LE u16 + u8.
    let pos = init_bc.iter().position(|&b| b == 0xB8).unwrap();
    assert_eq!(opcode::instruction_size(opcode::STR_INIT), 8);
    assert!(
        pos + 8 <= init_bc.len(),
        "STR_INIT has 7 trailing operand bytes"
    );
    let _data_offset = u32::from_le_bytes([
        init_bc[pos + 1],
        init_bc[pos + 2],
        init_bc[pos + 3],
        init_bc[pos + 4],
    ]);
    let max_len = u16::from_le_bytes([init_bc[pos + 5], init_bc[pos + 6]]);
    assert_eq!(max_len, 10, "STRING[10] declares max_len = 10");
    assert_eq!(
        init_bc[pos + 7],
        1,
        "STRING declares char_width = 1 (narrow)"
    );
}

#[test]
fn wire_when_five_byte_len_str_then_le_u32_operand() {
    // LEN_STR is a 5-byte shape (op + u32 data_offset).
    let bc = bytecode_of(
        "
PROGRAM main
  VAR
    s : STRING[10];
    n : DINT;
  END_VAR
  n := LEN(s);
END_PROGRAM
",
    );
    let pos = bc.iter().position(|&b| b == 0xC4).unwrap();
    assert_eq!(opcode::instruction_size(opcode::LEN_STR), 5);
    assert!(pos + 5 <= bc.len(), "LEN_STR has 4 trailing u32 bytes");
    let _data_offset = u32::from_le_bytes([bc[pos + 1], bc[pos + 2], bc[pos + 3], bc[pos + 4]]);
}

#[test]
fn wire_when_nine_byte_find_str_then_two_le_u32_operands() {
    // FIND_STR / REPLACE_STR / INSERT_STR / CONCAT_STR are 9-byte
    // shapes: opcode + two LE u32 data_offsets.
    let bc = bytecode_of(
        "
PROGRAM main
  VAR
    s1 : STRING[10];
    s2 : STRING[5];
    n : DINT;
  END_VAR
  n := FIND(s1, s2);
END_PROGRAM
",
    );
    let pos = bc.iter().position(|&b| b == 0xC8).unwrap();
    assert_eq!(opcode::instruction_size(opcode::FIND_STR), 9);
    assert!(pos + 9 <= bc.len(), "FIND_STR has 8 trailing u32 bytes");
    let _in1 = u32::from_le_bytes([bc[pos + 1], bc[pos + 2], bc[pos + 3], bc[pos + 4]]);
    let _in2 = u32::from_le_bytes([bc[pos + 5], bc[pos + 6], bc[pos + 7], bc[pos + 8]]);
}

#[test]
fn wire_when_ret_void_then_one_byte_ends_program() {
    // Every program ends with RET_VOID (0x8C). Trivial guard, but it
    // pins a wire-level invariant: programs are byte-terminated.
    let bc = bytecode_of(
        "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 0;
END_PROGRAM
",
    );
    assert_eq!(
        bc.last().copied(),
        Some(0x8C),
        "RET_VOID terminates program"
    );
}
