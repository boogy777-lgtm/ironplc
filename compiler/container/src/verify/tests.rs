//! Tests for operand-stack discipline verification, one case per violation and per accepted shape.

use super::*;
use crate::code_section::FuncEntry;
use std::string::{String, ToString};

/// Builds a single-function code section around `bytecode`.
fn section(bytecode: Vec<u8>, max_stack_depth: u16) -> CodeSection {
    CodeSection {
        functions: vec![FuncEntry {
            function_id: FunctionId::new(0),
            code_offset: 0,
            code_length: bytecode.len() as u32,
            max_stack_depth,
            num_locals: 4,
            num_params: 0,
        }],
        bytecode,
    }
}

#[test]
fn verify_stack_balance_when_balanced_then_ok() {
    // LOAD_CONST_I32 0; STORE_VAR_I32 0; RET_VOID
    let code = section(
        vec![
            opcode::LOAD_CONST_I32,
            0,
            0,
            opcode::STORE_VAR_I32,
            0,
            0,
            opcode::RET_VOID,
        ],
        1,
    );

    assert_eq!(verify_stack_balance(&code), Ok(()));
}

#[test]
fn verify_stack_balance_when_leaked_slot_then_unbalanced_return() {
    // LOAD_CONST_I32 0; RET_VOID  -- the loaded value is never consumed.
    let code = section(vec![opcode::LOAD_CONST_I32, 0, 0, opcode::RET_VOID], 1);

    assert_eq!(
        verify_stack_balance(&code),
        Err(StackImbalance::UnbalancedReturn {
            function_id: FunctionId::new(0),
            offset: 3,
            expected: 0,
            actual: 1,
        })
    );
}

#[test]
fn verify_stack_balance_when_over_pop_then_underflow() {
    // STORE_VAR_I32 0 with nothing on the stack.
    let code = section(vec![opcode::STORE_VAR_I32, 0, 0, opcode::RET_VOID], 1);

    assert_eq!(
        verify_stack_balance(&code),
        Err(StackImbalance::Underflow {
            function_id: FunctionId::new(0),
            offset: 0,
            opcode: opcode::STORE_VAR_I32,
            needs: 1,
            has: 0,
        })
    );
}

#[test]
fn verify_stack_balance_when_branch_arms_disagree_then_merge_conflict() {
    // LOAD_TRUE; JMP_IF_NOT +3; LOAD_TRUE; RET_VOID
    //
    // The taken path arrives at RET_VOID with depth 0, the fall-through
    // path with depth 1.
    let code = section(
        vec![
            opcode::LOAD_TRUE,
            opcode::JMP_IF_NOT,
            0x01,
            0x00,
            opcode::LOAD_TRUE,
            opcode::RET_VOID,
        ],
        2,
    );

    assert_eq!(
        verify_stack_balance(&code),
        Err(StackImbalance::MergeConflict {
            function_id: FunctionId::new(0),
            offset: 5,
            existing: 0,
            incoming: 1,
        })
    );
}

#[test]
fn verify_stack_balance_when_branch_arms_agree_then_ok() {
    // LOAD_TRUE; JMP_IF_NOT +1; POP-free both arms; RET_VOID
    let code = section(
        vec![
            opcode::LOAD_TRUE,
            opcode::JMP_IF_NOT,
            0x00,
            0x00,
            opcode::RET_VOID,
        ],
        2,
    );

    assert_eq!(verify_stack_balance(&code), Ok(()));
}

#[test]
fn verify_stack_balance_when_early_ret_with_value_then_ok() {
    // A value-returning function with an early RETURN: both exits leave
    // exactly one value. The emitter's linear counter would read 2 here.
    //
    // LOAD_VAR_I32 0; RET; LOAD_VAR_I32 0; RET
    let code = section(
        vec![
            opcode::LOAD_VAR_I32,
            0,
            0,
            opcode::RET,
            opcode::LOAD_VAR_I32,
            0,
            0,
            opcode::RET,
        ],
        1,
    );

    assert_eq!(verify_stack_balance(&code), Ok(()));
}

#[test]
fn verify_stack_balance_when_ret_without_value_then_unbalanced_return() {
    let code = section(vec![opcode::RET], 0);

    assert_eq!(
        verify_stack_balance(&code),
        Err(StackImbalance::UnbalancedReturn {
            function_id: FunctionId::new(0),
            offset: 0,
            expected: 1,
            actual: 0,
        })
    );
}

#[test]
fn verify_stack_balance_when_falls_off_end_unbalanced_then_unbalanced_return() {
    // LOAD_TRUE with no return at all: the VM treats the end of the body
    // as RET_VOID, so the leaked slot is still a leak.
    let code = section(vec![opcode::LOAD_TRUE], 1);

    assert_eq!(
        verify_stack_balance(&code),
        Err(StackImbalance::UnbalancedReturn {
            function_id: FunctionId::new(0),
            offset: 1,
            expected: 0,
            actual: 1,
        })
    );
}

#[test]
fn verify_stack_balance_when_depth_exceeds_declared_then_exceeds_declared_max() {
    // Two pushes against a declared max of 1.
    let code = section(
        vec![opcode::LOAD_TRUE, opcode::LOAD_TRUE, opcode::RET_VOID],
        1,
    );

    assert_eq!(
        verify_stack_balance(&code),
        Err(StackImbalance::ExceedsDeclaredMax {
            function_id: FunctionId::new(0),
            offset: 1,
            depth: 2,
            declared: 1,
        })
    );
}

#[test]
fn verify_stack_balance_when_jump_target_mid_instruction_then_invalid_jump_target() {
    // JMP +1 lands on the second byte of the following LOAD_CONST_I32.
    let code = section(
        vec![
            opcode::JMP,
            0x01,
            0x00,
            opcode::LOAD_CONST_I32,
            0,
            0,
            opcode::POP,
            opcode::RET_VOID,
        ],
        1,
    );

    assert!(matches!(
        verify_stack_balance(&code),
        Err(StackImbalance::InvalidJumpTarget { .. })
    ));
}

#[test]
fn verify_stack_balance_when_unassigned_opcode_then_unknown_opcode() {
    let code = section(vec![0xFF], 0);

    assert!(matches!(
        verify_stack_balance(&code),
        Err(StackImbalance::UnknownOpcode { byte: 0xFF, .. })
    ));
}

#[test]
fn verify_stack_balance_when_instruction_truncated_then_truncated_instruction() {
    // LOAD_CONST_I32 needs two operand bytes but only one is present.
    let code = section(vec![opcode::LOAD_CONST_I32, 0], 1);

    assert!(matches!(
        verify_stack_balance(&code),
        Err(StackImbalance::TruncatedInstruction { .. })
    ));
}

#[test]
fn verify_stack_balance_when_unreachable_code_unbalanced_then_ok() {
    // The trailing LOAD_TRUE cannot execute, so it cannot unbalance
    // anything and is not verified.
    let code = section(vec![opcode::RET_VOID, opcode::LOAD_TRUE], 1);

    assert_eq!(verify_stack_balance(&code), Ok(()));
}

#[test]
fn verify_stack_balance_when_call_then_pops_params_and_pushes_result() {
    // Function 0 pushes one argument, calls function 1 (one parameter),
    // then discards the result.
    let caller = vec![
        opcode::LOAD_TRUE,
        opcode::CALL,
        0x01,
        0x00,
        0x00,
        0x00,
        opcode::POP,
        opcode::RET_VOID,
    ];
    let callee = vec![opcode::LOAD_TRUE, opcode::RET];
    let caller_len = caller.len();
    let mut bytecode = caller;
    bytecode.extend_from_slice(&callee);

    let code = CodeSection {
        functions: vec![
            FuncEntry {
                function_id: FunctionId::new(0),
                code_offset: 0,
                code_length: caller_len as u32,
                max_stack_depth: 2,
                num_locals: 1,
                num_params: 0,
            },
            FuncEntry {
                function_id: FunctionId::new(1),
                code_offset: caller_len as u32,
                code_length: 2,
                max_stack_depth: 1,
                num_locals: 1,
                num_params: 1,
            },
        ],
        bytecode,
    };

    assert_eq!(verify_stack_balance(&code), Ok(()));
}

/// Builds a two-function section: function 0 is the caller, function 1
/// the method body it invokes.
fn caller_and_method(caller: Vec<u8>, method: Vec<u8>, caller_max: u16) -> CodeSection {
    let caller_len = caller.len() as u32;
    let method_len = method.len() as u32;
    let mut bytecode = caller;
    bytecode.extend_from_slice(&method);

    CodeSection {
        functions: vec![
            FuncEntry {
                function_id: FunctionId::new(0),
                code_offset: 0,
                code_length: caller_len,
                max_stack_depth: caller_max,
                num_locals: 1,
                num_params: 0,
            },
            FuncEntry {
                function_id: FunctionId::new(1),
                code_offset: caller_len,
                code_length: method_len,
                max_stack_depth: 1,
                num_locals: 1,
                num_params: 1,
            },
        ],
        bytecode,
    }
}

/// METHOD_CALL of function 1: [op][func u16][field_var_off u16][num_fields u8][param_var_off u16].
fn method_call_bytes() -> [u8; 8] {
    [
        opcode::METHOD_CALL,
        0x01,
        0x00,
        0x00,
        0x00,
        0x00,
        0x00,
        0x00,
    ]
}

#[test]
fn verify_stack_balance_when_method_call_returns_value_then_pops_params_and_pushes_result() {
    // fb_ref, one argument, the call, then discard the result and fb_ref.
    let mut caller = vec![opcode::FB_LOAD_INSTANCE, 0x00, 0x00, opcode::LOAD_TRUE];
    caller.extend_from_slice(&method_call_bytes());
    caller.extend_from_slice(&[opcode::POP, opcode::POP, opcode::RET_VOID]);

    // The method body ends in RET, so it leaves a value for the caller.
    let method = vec![opcode::LOAD_TRUE, opcode::RET];

    assert_eq!(
        verify_stack_balance(&caller_and_method(caller, method, 3)),
        Ok(())
    );
}

#[test]
fn verify_stack_balance_when_method_call_is_void_then_pushes_nothing() {
    // Same call site, but only fb_ref is discarded: a void method
    // leaves no result behind.
    let mut caller = vec![opcode::FB_LOAD_INSTANCE, 0x00, 0x00, opcode::LOAD_TRUE];
    caller.extend_from_slice(&method_call_bytes());
    caller.extend_from_slice(&[opcode::POP, opcode::RET_VOID]);

    // No RET anywhere in the body: the method is void.
    let method = vec![opcode::RET_VOID];

    assert_eq!(
        verify_stack_balance(&caller_and_method(caller, method, 3)),
        Ok(())
    );
}

#[test]
fn verify_stack_balance_when_void_method_result_popped_then_stack_underflow() {
    // Popping a result a void method never pushed must not verify.
    let mut caller = vec![opcode::FB_LOAD_INSTANCE, 0x00, 0x00, opcode::LOAD_TRUE];
    caller.extend_from_slice(&method_call_bytes());
    caller.extend_from_slice(&[opcode::POP, opcode::POP, opcode::RET_VOID]);

    let method = vec![opcode::RET_VOID];

    assert!(matches!(
        verify_stack_balance(&caller_and_method(caller, method, 3)),
        Err(StackImbalance::Underflow { .. })
    ));
}

#[test]
fn verify_stack_balance_when_method_callee_missing_then_unknown_callee() {
    let mut caller = vec![opcode::FB_LOAD_INSTANCE, 0x00, 0x00, opcode::LOAD_TRUE];
    caller.extend_from_slice(&method_call_bytes());
    caller.extend_from_slice(&[opcode::POP, opcode::POP, opcode::RET_VOID]);

    // Only function 0 exists, so the call target is out of range.
    let code = section(caller, 3);

    assert!(matches!(
        verify_stack_balance(&code),
        Err(StackImbalance::UnknownCallee { .. })
    ));
}

#[test]
fn verify_stack_balance_when_loop_back_edge_then_ok() {
    // A back edge that returns to the loop head at the same depth.
    //
    // 0: LOAD_TRUE
    // 1: JMP_IF_NOT +3   -> 7 (RET_VOID)
    // 4: JMP -7          -> 0
    // 7: RET_VOID
    let code = section(
        vec![
            opcode::LOAD_TRUE,
            opcode::JMP_IF_NOT,
            0x03,
            0x00,
            opcode::JMP,
            0xF9,
            0xFF,
            opcode::RET_VOID,
        ],
        1,
    );

    assert_eq!(verify_stack_balance(&code), Ok(()));
}

#[test]
fn verify_stack_balance_when_jump_target_before_body_then_invalid_jump_target() {
    // JMP -32 from offset 0 lands well before the body.
    let code = section(vec![opcode::JMP, 0xE0, 0xFF, opcode::RET_VOID], 0);

    assert!(matches!(
        verify_stack_balance(&code),
        Err(StackImbalance::InvalidJumpTarget { .. })
    ));
}

#[test]
fn verify_stack_balance_when_jump_target_past_body_then_invalid_jump_target() {
    // JMP +64 lands past the end of a four-byte body.
    let code = section(vec![opcode::JMP, 0x40, 0x00, opcode::RET_VOID], 0);

    assert!(matches!(
        verify_stack_balance(&code),
        Err(StackImbalance::InvalidJumpTarget { .. })
    ));
}

#[test]
fn verify_stack_balance_when_builtin_id_unknown_then_unknown_builtin() {
    // 0xFFFF is not a defined built-in function ID, so the instruction's
    // stack effect cannot be determined.
    let code = section(vec![opcode::BUILTIN, 0xFF, 0xFF, opcode::RET_VOID], 2);

    assert!(matches!(
        verify_stack_balance(&code),
        Err(StackImbalance::UnknownBuiltin {
            builtin_id: 0xFFFF,
            ..
        })
    ));
}

#[test]
fn verify_stack_balance_when_call_target_missing_then_unknown_callee() {
    // Function 9 is not in the single-entry function directory.
    let code = section(
        vec![opcode::CALL, 0x09, 0x00, 0x00, 0x00, opcode::RET_VOID],
        2,
    );

    assert!(matches!(
        verify_stack_balance(&code),
        Err(StackImbalance::UnknownCallee { .. })
    ));
}

#[test]
fn stack_imbalance_when_any_variant_then_renders_function_offset_and_rule() {
    // Every variant must produce a usable diagnostic. A violation is
    // reported as a compiler internal error, so its Display text is the
    // only thing pointing at the instruction responsible.
    let func = FunctionId::new(3);
    let variants = vec![
        StackImbalance::Underflow {
            function_id: func,
            offset: 7,
            opcode: opcode::POP,
            needs: 1,
            has: 0,
        },
        StackImbalance::MergeConflict {
            function_id: func,
            offset: 7,
            existing: 0,
            incoming: 1,
        },
        StackImbalance::UnbalancedReturn {
            function_id: func,
            offset: 7,
            expected: 0,
            actual: 1,
        },
        StackImbalance::ExceedsDeclaredMax {
            function_id: func,
            offset: 7,
            depth: 4,
            declared: 2,
        },
        StackImbalance::InvalidJumpTarget {
            function_id: func,
            offset: 7,
            target: -3,
        },
        StackImbalance::UnknownOpcode {
            function_id: func,
            offset: 7,
            byte: 0xFF,
        },
        StackImbalance::TruncatedInstruction {
            function_id: func,
            offset: 7,
            opcode: opcode::CALL,
        },
        StackImbalance::UnknownBuiltin {
            function_id: func,
            offset: 7,
            builtin_id: 0xFFFF,
        },
        StackImbalance::UnknownCallee {
            function_id: func,
            offset: 7,
            callee: FunctionId::new(9),
        },
    ];

    let unusable: Vec<String> = variants
        .iter()
        .filter(|v| {
            v.function_id() != func
                || v.offset() != 7
                || !v.to_string().contains("offset 7")
                || v.rule().is_some_and(|r| !r.starts_with('R'))
        })
        .map(|v| v.to_string())
        .collect();

    assert_eq!(unusable, Vec::<String>::new());
}

#[test]
fn stack_effect_when_opcode_unassigned_then_unknown_opcode() {
    let code = section(vec![], 0);

    assert!(matches!(
        effect_of(&code, FunctionId::new(0), 0, 0xFF, &[0u8; 8]),
        Err(StackImbalance::UnknownOpcode { byte: 0xFF, .. })
    ));
}

#[test]
fn stack_effect_when_opcode_assigned_then_effect_defined() {
    // Every assigned opcode must have a stack effect. This is the guard
    // that keeps the invariant holding as opcodes are added: a new
    // opcode wired into `instruction_size` but not into `effect_of`
    // fails here instead of being silently treated as a no-op.
    let code = section(vec![], 0);
    let operands = [0u8; 8];
    let undefined: Vec<u8> = (0u16..=255)
        .map(|b| b as u8)
        .filter(|&op| opcode::is_assigned(op))
        // CALL and BUILTIN read their operands; give them ones that
        // resolve so this test only measures whether an arm exists.
        .filter(|&op| op != opcode::CALL && op != opcode::BUILTIN)
        .filter(|&op| effect_of(&code, FunctionId::new(0), 0, op, &operands).is_err())
        .collect();

    assert_eq!(undefined, Vec::<u8>::new());
}
