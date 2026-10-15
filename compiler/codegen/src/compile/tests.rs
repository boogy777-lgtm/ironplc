//! Tests for compiling a library into a container: string literals, task configuration and basic programs.

use ironplc_analyzer::SemanticContext;
use ironplc_container::{Container, TaskType};
use ironplc_dsl::common::Library;
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_parser::options::CompilerOptions;
use ironplc_parser::parse_program;
use ironplc_problems::Problem;

use super::{compile, encode_string_literal, CodegenOptions, NARROW_CHAR_WIDTH, WIDE_CHAR_WIDTH};

/// Helper to parse and analyze an IEC 61131-3 program string into a Library.
///
/// Runs the analyzer's type resolution pass so that `Expr.expr_type` is
/// populated, which codegen requires for control flow and bitwise operations.
fn parse(source: &str) -> (Library, SemanticContext) {
    let library = parse_program(source, &FileId::default(), &CompilerOptions::default()).unwrap();
    let (analyzed, ctx) =
        ironplc_analyzer::stages::resolve_types(&[&library], &CompilerOptions::default()).unwrap();
    (analyzed, ctx)
}

#[test]
fn encode_string_literal_when_narrow_then_one_byte_per_char() {
    let bytes = encode_string_literal(&['h', 'i'], NARROW_CHAR_WIDTH);
    assert_eq!(bytes, vec![b'h', b'i']);
}

#[test]
fn encode_string_literal_when_wide_then_utf16le_two_bytes_per_char() {
    let bytes = encode_string_literal(&['h', 'i'], WIDE_CHAR_WIDTH);
    assert_eq!(bytes, vec![0x68, 0x00, 0x69, 0x00]);
}

#[test]
fn encode_string_literal_when_wide_and_bmp_above_ascii_then_utf16le() {
    // U+00E9 LATIN SMALL LETTER E WITH ACUTE
    let bytes = encode_string_literal(&['é'], WIDE_CHAR_WIDTH);
    assert_eq!(bytes, vec![0xE9, 0x00]);
}

#[test]
fn encode_string_literal_when_empty_then_returns_empty() {
    assert!(encode_string_literal(&[], NARROW_CHAR_WIDTH).is_empty());
    assert!(encode_string_literal(&[], WIDE_CHAR_WIDTH).is_empty());
}

#[test]
fn compile_when_simple_assignment_then_produces_container() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 10;
END_PROGRAM
";
    let (library, context) = parse(source);
    let container = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    )
    .unwrap();

    assert_eq!(container.header.num_variables, 1);
    assert_eq!(container.header.num_functions, 2);
    assert_eq!(
        container
            .constant_pool
            .get_i32(ironplc_container::ConstantIndex::new(0))
            .unwrap(),
        10
    );

    // Function 0: init (RET_VOID only, no initial values)
    let init_bytecode = container
        .code
        .get_function_bytecode(ironplc_container::FunctionId::new(0))
        .unwrap();
    assert_eq!(init_bytecode, &[0x8C]);

    // Function 1: scan — LOAD_CONST_I32 pool:0, STORE_VAR_I32 var:0, RET_VOID
    let scan_bytecode = container
        .code
        .get_function_bytecode(ironplc_container::FunctionId::new(1))
        .unwrap();
    assert_eq!(scan_bytecode, &[0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x8C]);
}

#[test]
fn compile_when_no_program_then_p4020_error() {
    let source = "
FUNCTION_BLOCK MyBlock
  VAR
    x : INT;
  END_VAR
END_FUNCTION_BLOCK
";
    let (library, context) = parse(source);
    let result = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    );

    assert!(result.is_err());
    let diagnostic = result.unwrap_err();
    assert_eq!(diagnostic.code, Problem::NoProgramDeclaration.code());
}

/// Builds a program whose CONFIGURATION declares one task with the given
/// initialization parameters and binds the program to it.
fn program_with_task(task_init: &str) -> String {
    format!(
        "
PROGRAM main
  VAR
    x : INT;
  END_VAR
  x := 1;
END_PROGRAM

CONFIGURATION config
  RESOURCE resource1 ON PLC
    TASK task1({task_init});
    PROGRAM instance1 WITH task1 : main;
  END_RESOURCE
END_CONFIGURATION
"
    )
}

fn compile_source(source: &str) -> Result<Container, Diagnostic> {
    let (library, context) = parse(source);
    compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    )
}

#[test]
fn compile_when_task_has_interval_then_cyclic_task_with_interval_us() {
    let source = program_with_task("INTERVAL := T#100ms, PRIORITY := 3");
    let container = compile_source(&source).unwrap();

    let task = &container.task_table.tasks[0];
    assert_eq!(task.task_type, TaskType::Cyclic);
    assert_eq!(task.interval_us, 100_000);
    assert_eq!(task.priority, 3);
}

#[test]
fn compile_when_task_interval_is_sub_millisecond_then_interval_us_keeps_precision() {
    let source = program_with_task("INTERVAL := T#0.5ms, PRIORITY := 0");
    let container = compile_source(&source).unwrap();

    assert_eq!(container.task_table.tasks[0].interval_us, 500);
}

#[test]
fn compile_when_no_configuration_then_freewheeling_task() {
    let source = "
PROGRAM main
  VAR
    x : INT;
  END_VAR
  x := 1;
END_PROGRAM
";
    let container = compile_source(source).unwrap();

    let task = &container.task_table.tasks[0];
    assert_eq!(task.task_type, TaskType::Freewheeling);
    assert_eq!(task.interval_us, 0);
}

#[test]
fn compile_when_program_instance_has_no_task_then_freewheeling_task() {
    let source = "
PROGRAM main
  VAR
    x : INT;
  END_VAR
  x := 1;
END_PROGRAM

CONFIGURATION config
  RESOURCE resource1 ON PLC
    TASK task1(INTERVAL := T#100ms, PRIORITY := 1);
    PROGRAM instance1 : main;
  END_RESOURCE
END_CONFIGURATION
";
    let container = compile_source(source).unwrap();

    let task = &container.task_table.tasks[0];
    assert_eq!(task.task_type, TaskType::Freewheeling);
    assert_eq!(task.interval_us, 0);
}

#[test]
fn compile_when_task_interval_is_zero_then_freewheeling_task() {
    let source = program_with_task("INTERVAL := T#0ms, PRIORITY := 1");
    let container = compile_source(&source).unwrap();

    let task = &container.task_table.tasks[0];
    assert_eq!(task.task_type, TaskType::Freewheeling);
    assert_eq!(task.interval_us, 0);
}

#[test]
fn compile_when_task_has_single_then_p4047_error() {
    let source = "
PROGRAM main
  VAR
    x : INT;
  END_VAR
  x := 1;
END_PROGRAM

CONFIGURATION config
  VAR_GLOBAL
    Trigger : BOOL;
  END_VAR
  RESOURCE resource1 ON PLC
    TASK task1(SINGLE := Trigger, PRIORITY := 1);
    PROGRAM instance1 WITH task1 : main;
  END_RESOURCE
END_CONFIGURATION
";
    let result = compile_source(source);

    assert!(result.is_err());
    assert_eq!(
        result.unwrap_err().code,
        Problem::TaskSingleNotSupported.code()
    );
}

#[test]
fn compile_when_task_priority_exceeds_u16_then_p4048_error() {
    let source = program_with_task("INTERVAL := T#100ms, PRIORITY := 100000");
    let result = compile_source(&source);

    assert!(result.is_err());
    assert_eq!(
        result.unwrap_err().code,
        Problem::TaskParameterOutOfRange.code()
    );
}

#[test]
fn compile_when_empty_program_then_produces_ret_void() {
    let source = "
PROGRAM main
  VAR
    x : INT;
  END_VAR
END_PROGRAM
";
    let (library, context) = parse(source);
    let container = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    )
    .unwrap();

    // Should have two functions (init + scan), both just RET_VOID
    assert_eq!(container.header.num_functions, 2);
    let init_bytecode = container
        .code
        .get_function_bytecode(ironplc_container::FunctionId::new(0))
        .unwrap();
    assert_eq!(init_bytecode, &[0x8C]); // RET_VOID only
    let scan_bytecode = container
        .code
        .get_function_bytecode(ironplc_container::FunctionId::new(1))
        .unwrap();
    assert_eq!(scan_bytecode, &[0x8C]); // RET_VOID only
}

#[test]
fn compile_when_duplicate_constants_then_deduplicates() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    y : DINT;
  END_VAR
  x := 10;
  y := 10;
END_PROGRAM
";
    let (library, context) = parse(source);
    let container = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    )
    .unwrap();

    // Should only have one constant (10 is deduplicated)
    assert_eq!(container.constant_pool.len(), 1);
}

#[test]
fn compile_when_variable_to_variable_assignment_then_load_store() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    y : DINT;
  END_VAR
  x := 10;
  y := x;
END_PROGRAM
";
    let (library, context) = parse(source);
    let container = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    )
    .unwrap();

    assert_eq!(container.header.num_variables, 2);
    assert_eq!(container.header.num_functions, 2);

    // Function 1 (scan):
    // x := 10: LOAD_CONST_I32 pool:0
    // (store-load peephole): DUP, STORE_VAR_I32 var:0, NOP, NOP
    // y := x:  STORE_VAR_I32 var:1
    // RET_VOID
    let bytecode = container
        .code
        .get_function_bytecode(ironplc_container::FunctionId::new(1))
        .unwrap();
    assert_eq!(
        bytecode,
        &[
            0x00, 0x00, 0x00, // LOAD_CONST_I32 pool:0
            0x91, // DUP (store-load optimization)
            0x10, 0x00, 0x00, // STORE_VAR_I32 var:0
            0x10, 0x01, 0x00, // STORE_VAR_I32 var:1
            0x8C, // RET_VOID
        ]
    );
}

#[test]
fn compile_when_negative_constant_then_produces_container() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := -5;
END_PROGRAM
";
    let (library, context) = parse(source);
    let container = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    )
    .unwrap();

    assert_eq!(
        container
            .constant_pool
            .get_i32(ironplc_container::ConstantIndex::new(0))
            .unwrap(),
        -5
    );

    // Function 1 (scan): LOAD_CONST_I32 pool:0 (-5), STORE_VAR_I32 var:0, RET_VOID
    let bytecode = container
        .code
        .get_function_bytecode(ironplc_container::FunctionId::new(1))
        .unwrap();
    assert_eq!(bytecode, &[0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x8C]);
}

#[test]
fn compile_when_simple_if_then_succeeds() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  IF x > 0 THEN
    x := 1;
  END_IF;
END_PROGRAM
";
    let (library, context) = parse(source);
    let result = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    );

    assert!(result.is_ok());
}

#[test]
fn compile_when_exit_outside_loop_then_p4021_error() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 1;
  EXIT;
END_PROGRAM
";
    let (library, context) = parse(source);
    let result = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    );

    assert!(result.is_err());
    let diagnostic = result.unwrap_err();
    assert_eq!(diagnostic.code, Problem::ExitOutsideLoop.code());
}

#[test]
fn compile_when_for_non_constant_step_then_p9999_error() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    s : DINT;
  END_VAR
  FOR x := 1 TO 10 BY s DO
    x := x;
  END_FOR;
END_PROGRAM
";
    let (library, context) = parse(source);
    let result = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    );

    assert!(result.is_err());
    let diagnostic = result.unwrap_err();
    assert_eq!(diagnostic.code, "P9999");
}

#[test]
fn compile_when_byte_variable_then_produces_container() {
    let source = "
PROGRAM main
  VAR
    x : BYTE;
  END_VAR
  x := 42;
END_PROGRAM
";
    let (library, context) = parse(source);
    let container = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    )
    .unwrap();

    assert_eq!(container.header.num_variables, 1);
    assert_eq!(
        container
            .constant_pool
            .get_i32(ironplc_container::ConstantIndex::new(0))
            .unwrap(),
        42
    );
}

#[test]
fn compile_when_dword_bit_string_literal_then_loads_constant() {
    let source = "
PROGRAM main
  VAR
    x : DWORD;
  END_VAR
  x := DWORD#16#FF;
END_PROGRAM
";
    let (library, context) = parse(source);
    let container = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    )
    .unwrap();

    assert_eq!(container.header.num_variables, 1);
    assert_eq!(
        container
            .constant_pool
            .get_i32(ironplc_container::ConstantIndex::new(0))
            .unwrap(),
        255
    );
}

#[test]
fn compile_when_user_function_with_real_comparison_then_produces_container() {
    let source = "
FUNCTION SIGN_R : BOOL
VAR_INPUT
    in : REAL;
END_VAR
    SIGN_R := in < 0.0;
END_FUNCTION
PROGRAM main
VAR
    result : BOOL;
END_VAR
    result := SIGN_R(in := 2.5);
END_PROGRAM
";
    let (library, context) = parse(source);
    let container = compile(
        &library,
        &context,
        &CodegenOptions::default(),
        &crate::EmptyLookup,
    )
    .unwrap();

    // Should have 3 functions: init, scan, SIGN_R
    assert_eq!(container.header.num_functions, 3);
}
