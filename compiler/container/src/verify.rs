//! Operand-stack discipline verification for emitted bytecode.
//!
//! This module implements the stack-discipline subset of the bytecode
//! verifier specified in `specs/design/bytecode-verifier-rules.md`:
//!
//! | Rule | Meaning | Variant |
//! |------|---------|---------|
//! | R0200 | Stack depth agrees at every control-flow merge point | [`StackImbalance::MergeConflict`] |
//! | R0202 | No instruction pops from an empty stack | [`StackImbalance::Underflow`] |
//! | R0203 | Depth never exceeds the declared `max_stack_depth` | [`StackImbalance::ExceedsDeclaredMax`] |
//!
//! plus the *balance* rule that motivates the pass: every path that leaves
//! a function must leave the operand stack at exactly the depth the calling
//! convention promises ([`StackImbalance::UnbalancedReturn`]) — 1 slot for
//! `RET` (the return value), 0 slots for `RET_VOID` and for falling off the
//! end of a body.
//!
//! # Why abstract interpretation
//!
//! The emitter keeps a running `current_stack_depth` while it appends
//! instructions, but that counter walks the buffer in *emission* order, not
//! in execution order. It therefore cannot answer the question this pass
//! answers: it sums both arms of an `IF` as if they ran back to back, it
//! keeps counting past an early `RETURN` that in reality leaves the
//! function, and it never compares the depths that different predecessors
//! deliver to the same instruction. Asserting that counter is zero at the
//! end of a function is neither sound (two opposite errors cancel) nor
//! complete (a value-returning early `RETURN` leaves it non-zero on
//! perfectly valid code).
//!
//! Walking the control-flow graph instead makes every one of those cases
//! exact, and — the point of the exercise — derives the answer from the
//! *bytecode that ships*, independently of the bookkeeping the emitter did
//! on the way there.
//!
//! # Model
//!
//! Each function is verified on its own with an entry depth of 0. That is
//! sound because the calling convention isolates frames: `CALL` pops the
//! callee's arguments *before* the callee's frame is pushed, so a callee
//! never observes — and must never touch — slots belonging to its caller.
//! `FB_CALL` likewise leaves the caller's `fb_ref` in place and runs the
//! body as its own frame.

use std::collections::VecDeque;
use std::fmt;
use std::vec;
use std::vec::Vec;

use crate::cfg::{self, CfgError, Flow};
use crate::code_section::CodeSection;
use crate::id_types::FunctionId;
use crate::opcode::{self, Opcode};

/// Depth the operand stack must have when a `RET` executes: the single
/// return value the caller's `CALL` accounts for.
const RET_DEPTH: u16 = 1;

/// Depth the operand stack must have when a `RET_VOID` executes, or when
/// control falls off the end of a body (which the VM treats as `RET_VOID`).
const RET_VOID_DEPTH: u16 = 0;

/// A violation of operand-stack discipline found in emitted bytecode.
///
/// Every variant names the function and the byte offset within that
/// function's body, so a failure points at the instruction responsible
/// rather than at a downstream symptom.
#[derive(Clone, Debug, PartialEq, Eq)]
pub enum StackImbalance {
    /// R0202: an instruction pops more values than the stack holds.
    Underflow {
        function_id: FunctionId,
        offset: usize,
        opcode: Opcode,
        /// Slots the instruction pops.
        needs: u16,
        /// Slots actually on the stack.
        has: u16,
    },
    /// R0200: two control-flow paths reach the same instruction with
    /// different stack depths.
    MergeConflict {
        function_id: FunctionId,
        offset: usize,
        /// Depth recorded by the path that reached this offset first.
        existing: u16,
        /// Depth delivered by the conflicting path.
        incoming: u16,
    },
    /// A path leaves the function with the wrong number of values on the
    /// stack — the leak (or over-pop) this pass exists to catch.
    UnbalancedReturn {
        function_id: FunctionId,
        offset: usize,
        /// Depth the calling convention requires at this exit.
        expected: u16,
        /// Depth the path actually delivers.
        actual: u16,
    },
    /// R0203: the stack grows past the depth declared in the function
    /// directory, which is what sizes the VM's operand-stack buffer.
    ExceedsDeclaredMax {
        function_id: FunctionId,
        offset: usize,
        /// Peak depth reached on some path.
        depth: u16,
        /// Depth declared in the function directory.
        declared: u16,
    },
    /// A branch targets a byte that is not the start of an instruction, or
    /// lies outside the function body.
    InvalidJumpTarget {
        function_id: FunctionId,
        offset: usize,
        target: isize,
    },
    /// A byte in an instruction position is not an assigned opcode.
    UnknownOpcode {
        function_id: FunctionId,
        offset: usize,
        byte: u8,
    },
    /// An instruction's operand bytes run past the end of the body.
    TruncatedInstruction {
        function_id: FunctionId,
        offset: usize,
        opcode: Opcode,
    },
    /// A `BUILTIN` names a function ID with no known argument count, so its
    /// stack effect cannot be determined.
    UnknownBuiltin {
        function_id: FunctionId,
        offset: usize,
        builtin_id: u16,
    },
    /// A `CALL` or `METHOD_CALL` names a function that is not in the function
    /// directory, so its parameter count — and therefore its stack effect —
    /// is unknown.
    UnknownCallee {
        function_id: FunctionId,
        offset: usize,
        callee: FunctionId,
    },
}

impl StackImbalance {
    /// The function whose body contains the violation.
    pub fn function_id(&self) -> FunctionId {
        match self {
            StackImbalance::Underflow { function_id, .. }
            | StackImbalance::MergeConflict { function_id, .. }
            | StackImbalance::UnbalancedReturn { function_id, .. }
            | StackImbalance::ExceedsDeclaredMax { function_id, .. }
            | StackImbalance::InvalidJumpTarget { function_id, .. }
            | StackImbalance::UnknownOpcode { function_id, .. }
            | StackImbalance::TruncatedInstruction { function_id, .. }
            | StackImbalance::UnknownBuiltin { function_id, .. }
            | StackImbalance::UnknownCallee { function_id, .. } => *function_id,
        }
    }

    /// Byte offset within that function's body.
    pub fn offset(&self) -> usize {
        match self {
            StackImbalance::Underflow { offset, .. }
            | StackImbalance::MergeConflict { offset, .. }
            | StackImbalance::UnbalancedReturn { offset, .. }
            | StackImbalance::ExceedsDeclaredMax { offset, .. }
            | StackImbalance::InvalidJumpTarget { offset, .. }
            | StackImbalance::UnknownOpcode { offset, .. }
            | StackImbalance::TruncatedInstruction { offset, .. }
            | StackImbalance::UnknownBuiltin { offset, .. }
            | StackImbalance::UnknownCallee { offset, .. } => *offset,
        }
    }

    /// The `R####` rule code from `bytecode-verifier-rules.md` this
    /// violation corresponds to, when one applies.
    pub fn rule(&self) -> Option<&'static str> {
        match self {
            StackImbalance::MergeConflict { .. } => Some("R0200"),
            StackImbalance::Underflow { .. } => Some("R0202"),
            StackImbalance::ExceedsDeclaredMax { .. } => Some("R0203"),
            StackImbalance::InvalidJumpTarget { .. } => Some("R0400"),
            StackImbalance::UnknownOpcode { .. } => Some("R0001"),
            StackImbalance::UnknownBuiltin { .. } => Some("R0510"),
            StackImbalance::UnknownCallee { .. } => Some("R0002"),
            StackImbalance::UnbalancedReturn { .. }
            | StackImbalance::TruncatedInstruction { .. } => None,
        }
    }
}

impl fmt::Display for StackImbalance {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let func = self.function_id();
        let at = self.offset();
        match self {
            StackImbalance::Underflow {
                opcode, needs, has, ..
            } => write!(
                f,
                "function {func} offset {at}: opcode 0x{opcode:02X} pops {needs} value(s) \
                 but the operand stack holds {has}"
            ),
            StackImbalance::MergeConflict {
                existing, incoming, ..
            } => write!(
                f,
                "function {func} offset {at}: control-flow paths reach this instruction with \
                 different operand-stack depths ({existing} and {incoming})"
            ),
            StackImbalance::UnbalancedReturn {
                expected, actual, ..
            } => write!(
                f,
                "function {func} offset {at}: this exit leaves {actual} value(s) on the operand \
                 stack but the calling convention requires {expected}"
            ),
            StackImbalance::ExceedsDeclaredMax {
                depth, declared, ..
            } => write!(
                f,
                "function {func} offset {at}: operand stack reaches depth {depth}, past the \
                 declared max_stack_depth of {declared}"
            ),
            StackImbalance::InvalidJumpTarget { target, .. } => write!(
                f,
                "function {func} offset {at}: branch target {target} is outside the function \
                 body or does not start an instruction"
            ),
            StackImbalance::UnknownOpcode { byte, .. } => write!(
                f,
                "function {func} offset {at}: 0x{byte:02X} is not an assigned opcode"
            ),
            StackImbalance::TruncatedInstruction { opcode, .. } => write!(
                f,
                "function {func} offset {at}: opcode 0x{opcode:02X} operands run past the end \
                 of the function body"
            ),
            StackImbalance::UnknownBuiltin { builtin_id, .. } => write!(
                f,
                "function {func} offset {at}: BUILTIN 0x{builtin_id:04X} has no known argument \
                 count, so its stack effect is undefined"
            ),
            StackImbalance::UnknownCallee { callee, .. } => write!(
                f,
                "function {func} offset {at}: call targets function {callee}, which is not in \
                 the function directory"
            ),
        }
    }
}

/// How many values an instruction pops and pushes.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
struct Effect {
    pops: u16,
    pushes: u16,
}

impl Effect {
    const NONE: Effect = Effect { pops: 0, pushes: 0 };

    const fn new(pops: u16, pushes: u16) -> Self {
        Effect { pops, pushes }
    }
}

/// Turns a [`CfgError`] from the shared walk into the matching
/// [`StackImbalance`] variant, adding the function this pass is verifying.
fn from_cfg(function_id: FunctionId, err: CfgError) -> StackImbalance {
    match err {
        CfgError::UnknownOpcode { offset, byte } => StackImbalance::UnknownOpcode {
            function_id,
            offset,
            byte,
        },
        CfgError::TruncatedInstruction { offset, opcode } => StackImbalance::TruncatedInstruction {
            function_id,
            offset,
            opcode,
        },
        CfgError::InvalidJumpTarget { offset, target } => StackImbalance::InvalidJumpTarget {
            function_id,
            offset,
            target,
        },
    }
}

/// Verifies operand-stack discipline for every function in `code`.
///
/// Returns the first violation found, scanning functions in directory
/// order and each body from its entry point outwards. Unreachable
/// instructions are not verified — nothing can execute them, so they
/// cannot unbalance the stack.
pub fn verify_stack_balance(code: &CodeSection) -> Result<(), StackImbalance> {
    for entry in &code.functions {
        let bytecode = code
            .get_function_bytecode(entry.function_id)
            .unwrap_or_default();
        verify_function(code, entry.function_id, bytecode, entry.max_stack_depth)?;
    }
    Ok(())
}

/// Verifies one function body by abstract interpretation over its CFG.
///
/// `declared_max` is the function directory's `max_stack_depth`, which
/// sizes the VM's operand-stack buffer (R0203).
fn verify_function(
    code: &CodeSection,
    function_id: FunctionId,
    bytecode: &[u8],
    declared_max: u16,
) -> Result<(), StackImbalance> {
    let len = bytecode.len();

    // Phase 1: instruction boundaries. Bodies are emitted as a contiguous
    // instruction stream with no interleaved data, so a linear decode from
    // offset 0 enumerates exactly the valid branch targets.
    let boundaries = cfg::instruction_boundaries(bytecode).map_err(|e| from_cfg(function_id, e))?;

    // Phase 2: abstract interpretation. `depth_at[pc]` is the operand-stack
    // depth on entry to the instruction at `pc`, once some path has reached
    // it. Index `len` represents falling off the end of the body.
    let mut depth_at: Vec<Option<u16>> = vec![None; len + 1];
    let mut work: VecDeque<usize> = VecDeque::new();
    depth_at[0] = Some(0);
    work.push_back(0);

    while let Some(pc) = work.pop_front() {
        let Some(depth) = depth_at[pc] else {
            continue;
        };

        if pc == len {
            // Fell off the end of the body; the VM treats this as RET_VOID
            // (ADR-0044), so hold it to the same operand-stack contract.
            return_check(function_id, pc, RET_VOID_DEPTH, depth)?;
            continue;
        }

        // `pc` is always an instruction boundary: phase 1 rejected every
        // unassigned byte and every instruction whose operands run past the
        // end, and this loop only ever enqueues offsets it validated against
        // `boundaries` (or `pc + size`, which is a boundary by construction).
        // Re-checking here would be unreachable code.
        debug_assert!(
            boundaries[pc],
            "phase 2 reached offset {pc}, which phase 1 did not mark as an instruction boundary"
        );
        let op = bytecode[pc];
        let size = opcode::instruction_size(op);

        let operands = &bytecode[pc + 1..pc + size];
        let effect = effect_of(code, function_id, pc, op, operands)?;

        if depth < effect.pops {
            return Err(StackImbalance::Underflow {
                function_id,
                offset: pc,
                opcode: op,
                needs: effect.pops,
                has: depth,
            });
        }
        let out = depth - effect.pops + effect.pushes;

        // R0203: the peak inside this instruction is whichever of the two
        // endpoints is higher — pops always precede pushes at runtime.
        let peak = depth.max(out);
        if peak > declared_max {
            return Err(StackImbalance::ExceedsDeclaredMax {
                function_id,
                offset: pc,
                depth: peak,
                declared: declared_max,
            });
        }

        match cfg::flow_of(op, operands) {
            Flow::Next => {
                propagate(function_id, &mut depth_at, &mut work, pc + size, out)?;
            }
            Flow::Jump(target) => {
                let target = resolve(function_id, pc, size, target, &boundaries, len)?;
                propagate(function_id, &mut depth_at, &mut work, target, out)?;
            }
            Flow::Branch(target) => {
                let resolved = resolve(function_id, pc, size, target, &boundaries, len)?;
                propagate(function_id, &mut depth_at, &mut work, resolved, out)?;
                propagate(function_id, &mut depth_at, &mut work, pc + size, out)?;
            }
            Flow::Return => {
                // `flow_of` yields Return only for RET and RET_VOID, so the
                // depth each owes follows from the opcode.
                let expected = if op == opcode::RET {
                    RET_DEPTH
                } else {
                    RET_VOID_DEPTH
                };
                return_check(function_id, pc, expected, out)?;
            }
        }
    }

    Ok(())
}

/// Resolves a branch operand, tagging any failure with this function.
fn resolve(
    function_id: FunctionId,
    pc: usize,
    size: usize,
    relative: isize,
    boundaries: &[bool],
    len: usize,
) -> Result<usize, StackImbalance> {
    cfg::branch_target(pc, size, relative, boundaries, len).map_err(|e| from_cfg(function_id, e))
}

/// Records `depth` as the entry depth of the instruction at `target`, or
/// reports R0200 if a previously recorded depth disagrees.
fn propagate(
    function_id: FunctionId,
    depth_at: &mut [Option<u16>],
    work: &mut VecDeque<usize>,
    target: usize,
    depth: u16,
) -> Result<(), StackImbalance> {
    match depth_at[target] {
        None => {
            depth_at[target] = Some(depth);
            work.push_back(target);
        }
        Some(existing) if existing != depth => {
            return Err(StackImbalance::MergeConflict {
                function_id,
                offset: target,
                existing,
                incoming: depth,
            });
        }
        // Already visited at this depth; the abstract state has converged
        // and re-walking would not change any successor.
        Some(_) => {}
    }
    Ok(())
}

/// Checks that a path leaving the function delivers the depth the calling
/// convention requires.
fn return_check(
    function_id: FunctionId,
    offset: usize,
    expected: u16,
    actual: u16,
) -> Result<(), StackImbalance> {
    if actual != expected {
        return Err(StackImbalance::UnbalancedReturn {
            function_id,
            offset,
            expected,
            actual,
        });
    }
    Ok(())
}

/// Resolves a branch's relative operand into an absolute offset, checking
/// that it lands on an instruction boundary inside the body.
///
/// Branch offsets are relative to the byte *after* the i16 operand, which
/// is the end of the instruction.
/// The operand-stack depth a `METHOD_CALL` callee leaves for its caller:
/// [`RET_DEPTH`] when the method has a return value, [`RET_VOID_DEPTH`]
/// when it is void.
///
/// A METHOD_CALL instruction encodes only addressing (`function_id`, the
/// field and param scratch offsets), so whether the method returns a value
/// has to come from the body it calls. Decoding the callee linearly is
/// enough: a body that returns a value ends every path in `RET`, and a void
/// body has no `RET` at all. A callee whose bytes do not decode is left at
/// [`RET_VOID_DEPTH`] here; verifying that function in its own right is what
/// reports the malformed body.
fn method_return_depth(code: &CodeSection, callee: FunctionId) -> u16 {
    let bytecode = code.get_function_bytecode(callee).unwrap_or_default();
    let decodes_to_ret = opcode::decode_body(bytecode)
        .map_while(Result::ok)
        .any(|instruction| instruction.opcode == opcode::RET);

    if decodes_to_ret {
        RET_DEPTH
    } else {
        RET_VOID_DEPTH
    }
}

/// The operand-stack effect of one instruction.
///
/// This match is exhaustive over the assigned opcode space — there is no
/// catch-all for assigned opcodes — so adding an opcode to
/// [`opcode::instruction_size`] without giving it a stack effect here is
/// caught by `stack_effect_when_opcode_assigned_then_effect_defined`
/// rather than silently verified as a no-op.
fn effect_of(
    code: &CodeSection,
    function_id: FunctionId,
    offset: usize,
    op: Opcode,
    operands: &[u8],
) -> Result<Effect, StackImbalance> {
    use opcode::*;

    let effect = match op {
        // --- Push one, pop nothing ---
        LOAD_CONST_I32 | LOAD_CONST_I64 | LOAD_CONST_F32 | LOAD_CONST_F64 | LOAD_CONST_STR
        | LOAD_VAR_I32 | LOAD_VAR_I64 | LOAD_VAR_F32 | LOAD_VAR_F64 | LOAD_TRUE | LOAD_FALSE
        | DUP | FB_LOAD_INSTANCE | STR_LOAD_VAR | LEN_STR | FIND_STR | CONCAT_STR => {
            Effect::new(0, 1)
        }

        // FB_LOAD_PARAM reads a field through the fb_ref it leaves in place.
        FB_LOAD_PARAM => Effect::new(0, 1),

        // --- Pop one, push nothing ---
        STORE_VAR_I32 | STORE_VAR_I64 | STORE_VAR_F32 | STORE_VAR_F64 | POP | JMP_IF_NOT
        | STR_STORE_VAR => Effect::new(1, 0),

        // FB_STORE_PARAM consumes the value; the fb_ref below it survives.
        FB_STORE_PARAM => Effect::new(1, 0),

        // COPY_REGION consumes the source data offset. The destination and
        // both sizes are immediates, so nothing else is taken from the stack.
        COPY_REGION => Effect::new(1, 0),

        // --- Pop two, push nothing ---
        STORE_INDIRECT | STORE_ARRAY | STORE_ARRAY_DEREF | STR_STORE_ARRAY_ELEM => {
            Effect::new(2, 0)
        }

        // --- Pop one, push one (net zero) ---
        NEG_I32 | NEG_I64 | NEG_F32 | NEG_F64 | BOOL_NOT | BIT_NOT_32 | BIT_NOT_64 | TRUNC_I8
        | TRUNC_U8 | TRUNC_I16 | TRUNC_U16 | LOAD_INDIRECT | LOAD_ARRAY | LOAD_ARRAY_DEREF
        | STR_LOAD_ARRAY_ELEM | INSERT_STR | LEFT_STR | RIGHT_STR => Effect::new(1, 1),

        // --- Pop two, push one (net pop one) ---
        ADD_I32 | SUB_I32 | MUL_I32 | DIV_I32 | MOD_I32 | ADD_I64 | SUB_I64 | MUL_I64 | DIV_I64
        | MOD_I64 | DIV_U32 | MOD_U32 | DIV_U64 | MOD_U64 | ADD_F32 | SUB_F32 | MUL_F32
        | DIV_F32 | ADD_F64 | SUB_F64 | MUL_F64 | DIV_F64 | EQ_I32 | NE_I32 | LT_I32 | LE_I32
        | GT_I32 | GE_I32 | EQ_I64 | NE_I64 | LT_I64 | LE_I64 | GT_I64 | GE_I64 | LT_U32
        | LE_U32 | GT_U32 | GE_U32 | LT_U64 | LE_U64 | GT_U64 | GE_U64 | EQ_F32 | NE_F32
        | LT_F32 | LE_F32 | GT_F32 | GE_F32 | EQ_F64 | NE_F64 | LT_F64 | LE_F64 | GT_F64
        | GE_F64 | BOOL_AND | BOOL_OR | BOOL_XOR | BIT_AND_32 | BIT_OR_32 | BIT_XOR_32
        | BIT_AND_64 | BIT_OR_64 | BIT_XOR_64 | REPLACE_STR | DELETE_STR | MID_STR => {
            Effect::new(2, 1)
        }

        // --- No stack effect ---
        //
        // SWAP reorders the top two slots; it needs two present, which the
        // pop/push pair below models without changing the depth. JMP,
        // CMP_BR, STR_INIT, STR_INIT_ARRAY and the returns move control or
        // touch only memory. FB_CALL runs the body as its own frame and
        // leaves the caller's fb_ref where it was.
        SWAP => Effect::new(2, 2),
        JMP | CMP_BR_I32 | CMP_BR_I64 | STR_INIT | STR_INIT_ARRAY | FB_CALL | RET | RET_VOID => {
            Effect::NONE
        }

        // --- Variable-effect instructions ---
        BUILTIN => {
            let builtin_id = cfg::u16_at(operands, 0);
            let args = opcode::builtin::arg_count_opt(builtin_id).ok_or(
                StackImbalance::UnknownBuiltin {
                    function_id,
                    offset,
                    builtin_id,
                },
            )?;
            // Every builtin consumes its arguments and produces one result.
            Effect::new(args, 1)
        }
        CALL => {
            let callee = FunctionId::new(cfg::u16_at(operands, 0));
            let entry = code
                .get_function(callee)
                .ok_or(StackImbalance::UnknownCallee {
                    function_id,
                    offset,
                    callee,
                })?;
            // Arguments are popped into the callee's parameter slots; the
            // callee's RET leaves exactly one value behind.
            Effect::new(entry.num_params, RET_DEPTH)
        }
        METHOD_CALL => {
            let callee = FunctionId::new(cfg::u16_at(operands, 0));
            let entry = code
                .get_function(callee)
                .ok_or(StackImbalance::UnknownCallee {
                    function_id,
                    offset,
                    callee,
                })?;
            // Arguments are popped into the method's own parameter slots,
            // exactly as for CALL. The fb_ref underneath them stays where it
            // is (as with FB_CALL). Unlike a FUNCTION, a METHOD may be void,
            // and the calling convention carries no flag saying which it is,
            // so the callee's own returns are the record.
            Effect::new(entry.num_params, method_return_depth(code, callee))
        }

        _ => {
            return Err(StackImbalance::UnknownOpcode {
                function_id,
                offset,
                byte: op,
            })
        }
    };
    Ok(effect)
}

#[cfg(test)]
mod tests;
