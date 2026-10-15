//! Low-level bytecode emitter.
//!
//! Provides a builder that appends opcodes and operands to a byte buffer.

use ironplc_container::opcode;
use ironplc_container::{SourceColumn, SourceFileId, SourceLine};

/// A bytecode-offset → source-location entry recorded by the [`Emitter`].
///
/// The Emitter does not know its own `FunctionId`; the codegen driver pairs
/// each entry with the appropriate `FunctionId` when handing them to the
/// container builder to produce
/// [`ironplc_container::LineMapEntry`](ironplc_container::LineMapEntry)
/// values.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct EmittedLineMapEntry {
    /// Offset within the emitted bytecode of the instruction this entry
    /// describes.
    pub bytecode_offset: u16,
    /// Index into the SOURCE_FILE_TABLE that the codegen driver passes
    /// to the container builder. The driver is responsible for assigning
    /// stable indices; the emitter just records what it's told.
    pub file_id: SourceFileId,
    /// Source line number.
    pub source_line: SourceLine,
    /// Source column number.
    pub source_column: SourceColumn,
}

/// An opaque forward reference to a bytecode position, used for jump targets.
#[derive(Clone, Copy)]
pub struct Label(usize);

/// A pending jump that needs to be patched once the target label is bound.
struct PendingPatch {
    /// Start of the instruction carrying the jump operand. Held rather than
    /// the operand's own position because only instruction starts survive
    /// the optimizer's old→new offset map.
    instruction_offset: usize,
    /// Where the i16 operand sits within that instruction.
    operand_delta: usize,
    /// The label this jump targets.
    label: Label,
}

impl PendingPatch {
    /// Position of the i16 operand in the bytecode buffer.
    fn patch_offset(&self) -> usize {
        self.instruction_offset + self.operand_delta
    }
}

/// The emitter's bytecode before its jump operands have been patched,
/// together with the positions those jumps will land on.
///
/// This is what the peephole optimizer consumes. Handing it the offsets
/// symbolically — rather than making it recover them from encoded branch
/// operands — is what keeps knowledge of the branch instruction encoding out
/// of `optimize/` entirely.
pub(crate) struct UnpatchedCode<'a> {
    /// The emitted bytes. Every branch operand is still a placeholder.
    pub(crate) bytecode: &'a [u8],
    /// Bound label positions referenced by at least one pending jump. These
    /// are the offsets the optimizer must not remove or rewrite.
    pub(crate) jump_targets: crate::optimize::OffsetSet,
}

/// Accumulates bytecode instructions.
pub struct Emitter {
    bytecode: Vec<u8>,
    max_stack_depth: u16,
    current_stack_depth: u16,
    /// Temporary string buffers live at the current point of emission,
    /// and the most that were ever live at once. Tracked here, beside the
    /// opcode that allocates or consumes one, so the container header's
    /// pool size cannot drift from the bytecode that draws on the pool.
    ///
    /// A buffer is allocated by the opcodes that push a `buf_idx`
    /// (`LOAD_CONST_STR`, `STR_LOAD_VAR`, the string functions that
    /// produce a result) and released by the opcodes that consume one
    /// (`STR_STORE_VAR`, `STR_STORE_ARRAY_ELEM`), matching the VM's
    /// allocator. So this is the count of buffers simultaneously live,
    /// not the number of string operations: a string operation in a loop
    /// allocates and releases one buffer per iteration and contributes 1.
    max_temp_depth: u16,
    current_temp_depth: u16,
    /// Bound positions for each label (None if not yet bound).
    labels: Vec<Option<usize>>,
    /// Jump operands that need backpatching.
    patches: Vec<PendingPatch>,
    /// Tracks the last emitted load for consecutive-load DUP optimization.
    last_load: Option<LastLoad>,
    /// Tracks the last emitted store for the store-load DUP optimization.
    last_store: Option<LastStore>,
    /// Source position to associate with subsequently emitted opcodes. When
    /// `Some`, the next opcode that adds bytes to `bytecode` records a
    /// line_map entry at its offset (subject to dedup against the last
    /// recorded position).
    current_position: Option<(SourceFileId, SourceLine, SourceColumn)>,
    /// Recorded `(bytecode_offset, file_id, source_line, source_column)`
    /// entries in emission order — therefore monotonically non-decreasing
    /// in offset.
    line_map: Vec<EmittedLineMapEntry>,
}

/// Records the last emitted load instruction for DUP optimization.
/// When two consecutive identical loads are emitted, the second is
/// replaced with a cheaper DUP instruction (1 byte vs 3 bytes).
#[derive(Clone, PartialEq)]
struct LastLoad {
    /// The opcode byte (e.g. LOAD_VAR_I32, LOAD_CONST_I32).
    opcode: u8,
    /// The 2-byte operand (little-endian).
    operand: [u8; 2],
}

/// Records the last emitted store-variable instruction for the store-load
/// DUP optimization. When the next emission is a matching LOAD_VAR, the
/// emitter retroactively inserts a DUP before the STORE and skips the LOAD.
#[derive(Clone)]
struct LastStore {
    /// Byte offset of the STORE_VAR opcode in the bytecode buffer.
    position: usize,
    /// The STORE_VAR opcode byte (e.g. STORE_VAR_I32).
    opcode: u8,
    /// The 2-byte operand (little-endian).
    operand: [u8; 2],
}

/// Emit a no-operand instruction that pushes one value.
macro_rules! emit_push_op {
    ($name:ident, $opcode:expr) => {
        pub fn $name(&mut self) {
            self.emit_opcode($opcode);
            self.push_stack(1);
        }
    };
}

/// Emit an instruction with a u16 operand that pushes one value.
/// Uses DUP optimization when the same load is emitted consecutively.
macro_rules! emit_load_u16 {
    ($name:ident, $opcode:expr) => {
        pub fn $name(&mut self, index: u16) {
            self.emit_load_with_dup_check($opcode, index.to_le_bytes());
        }
    };
}

/// Emit a variable load instruction with a VarIndex operand that pushes one value.
/// Uses DUP optimization when the same load is emitted consecutively.
macro_rules! emit_load_var_index {
    ($name:ident, $opcode:expr) => {
        pub fn $name(&mut self, index: VarIndex) {
            self.emit_load_with_dup_check($opcode, index.to_le_bytes());
        }
    };
}

/// Emit a variable store instruction with a VarIndex operand that pops one value.
/// Records the store in `last_store` so a following matching LOAD_VAR can be
/// optimized into a DUP inserted before the STORE.
macro_rules! emit_store_var_index {
    ($name:ident, $opcode:expr) => {
        pub fn $name(&mut self, index: VarIndex) {
            self.emit_store_with_tracking($opcode, index.to_le_bytes());
        }
    };
}

/// Emit a no-operand binary op (pops two, pushes one = net pop 1).
macro_rules! emit_binop {
    ($name:ident, $opcode:expr) => {
        pub fn $name(&mut self) {
            self.emit_opcode($opcode);
            self.pop_stack(1);
        }
    };
}

/// Emit a no-operand unary op (pops one, pushes one = no stack change).
macro_rules! emit_unaryop {
    ($name:ident, $opcode:expr) => {
        pub fn $name(&mut self) {
            self.emit_opcode($opcode);
        }
    };
}

mod call;
mod jump;
mod memory;
mod scalar;
mod string;

impl Emitter {
    pub fn new() -> Self {
        Emitter {
            bytecode: Vec::new(),
            max_stack_depth: 0,
            current_stack_depth: 0,
            max_temp_depth: 0,
            current_temp_depth: 0,
            labels: Vec::new(),
            patches: Vec::new(),
            last_load: None,
            last_store: None,
            current_position: None,
            line_map: Vec::new(),
        }
    }

    // Source-map recording. `compile_stmt` sets a position per statement,
    // `compile.rs` carries the entries through the optimizer's offset
    // remapping into the container's LINE_MAP (tag 1), and
    // `codegen/tests/it/end_to_end_debug_line_map.rs` asserts the result
    // against real source. See `specs/design/debugger-support.md`
    // §"Source Position Tracking".

    /// Sets the source position to associate with subsequently emitted
    /// opcodes. Each new opcode that actually pushes bytes records an
    /// entry at its offset, deduplicated against the previous entry so
    /// runs of instructions sharing a position produce a single entry.
    ///
    /// `file_id` indexes the SOURCE_FILE_TABLE that the codegen driver
    /// supplies to the container builder. Lines and columns follow the
    /// [`ironplc_container::LineMapEntry`] convention: 1-based, with
    /// `0` reserved for "unknown" column.
    pub fn set_source_position(
        &mut self,
        file_id: SourceFileId,
        line: SourceLine,
        column: SourceColumn,
    ) {
        self.current_position = Some((file_id, line, column));
    }

    /// Clears the current source position. Subsequent opcodes will not
    /// produce line_map entries until [`Self::set_source_position`] is
    /// called again.
    #[cfg_attr(
        not(test),
        expect(dead_code, reason = "only the tests clear the position")
    )]
    pub fn clear_source_position(&mut self) {
        self.current_position = None;
    }

    /// Takes ownership of the recorded line_map entries, leaving the
    /// Emitter's internal vec empty. Entries are returned in emission
    /// order (which is sorted by `bytecode_offset`).
    pub fn take_line_map(&mut self) -> Vec<EmittedLineMapEntry> {
        core::mem::take(&mut self.line_map)
    }

    /// Records a line_map entry for the next opcode about to be appended
    /// to `bytecode`, when a current source position is set and differs
    /// from the previously recorded entry's `(line, column)`. Call this
    /// immediately before pushing the opcode byte; the recorded offset is
    /// the current `bytecode.len()`.
    fn record_position_for_next_opcode(&mut self) {
        let Some((file_id, line, column)) = self.current_position else {
            return;
        };
        if let Some(last) = self.line_map.last() {
            if last.file_id == file_id && last.source_line == line && last.source_column == column {
                return;
            }
        }
        self.line_map.push(EmittedLineMapEntry {
            bytecode_offset: self.bytecode.len() as u16,
            file_id,
            source_line: line,
            source_column: column,
        });
    }

    /// Pushes an opcode byte and invalidates both DUP trackers.
    ///
    /// Every non-load, non-store emission goes through this method, so the
    /// peephole trackers are automatically cleared without needing explicit
    /// calls at each site. The load path (`emit_load_with_dup_check`) and
    /// the store path (`emit_store_with_tracking`) bypass this to manage
    /// the trackers themselves.
    fn emit_opcode(&mut self, op: u8) {
        self.record_position_for_next_opcode();
        self.bytecode.push(op);
        self.last_load = None;
        self.last_store = None;
    }

    /// Checks peephole opportunities before emitting a load:
    ///
    /// 1. **Consecutive identical load:** if the previous instruction was
    ///    an identical load, emit a 1-byte DUP instead of the 3-byte load.
    /// 2. **Store-load pair:** if the previous instruction was a matching
    ///    STORE_VAR (same width, same operand), retroactively insert a DUP
    ///    before the STORE and skip this load entirely. This eliminates
    ///    the redundant round-trip through the variable table.
    ///
    /// If neither applies, the load is emitted normally and recorded for
    /// future checks.
    fn emit_load_with_dup_check(&mut self, op: u8, operand: [u8; 2]) {
        // Case 1: consecutive identical load.
        let candidate = LastLoad {
            opcode: op,
            operand,
        };
        if self.last_load.as_ref() == Some(&candidate) {
            self.emit_opcode(opcode::DUP);
            self.push_stack(1);
            self.last_load = None;
            return;
        }

        // Case 2: store-load pair (STORE_VAR N; LOAD_VAR N of same width).
        if let Some(store) = &self.last_store {
            if Self::load_matches_store(op, store.opcode) && store.operand == operand {
                // Retroactively insert DUP before the STORE. This is safe
                // because `last_store` is cleared by any intervening
                // emission or label bind, so the STORE is guaranteed to be
                // the most recently emitted bytes and no patches or labels
                // reference positions at or after the STORE.
                let store_pos = store.position;
                self.bytecode.insert(store_pos, opcode::DUP);
                // The DUP;STORE sequence has the same net stack effect as
                // the original LOAD (+1), so advance current_stack_depth
                // as if the LOAD had been emitted. The runtime peak at the
                // DUP is one slot above that value.
                self.push_stack(1);
                let dup_peak = self.current_stack_depth.saturating_add(1);
                if dup_peak > self.max_stack_depth {
                    self.max_stack_depth = dup_peak;
                }
                self.last_load = None;
                self.last_store = None;
                return;
            }
        }

        // Default: emit the load normally.
        self.record_position_for_next_opcode();
        self.bytecode.push(op);
        self.bytecode.extend_from_slice(&operand);
        self.push_stack(1);
        self.last_load = Some(candidate);
        self.last_store = None;
    }

    /// Emits a STORE_VAR instruction and records it for the store-load DUP
    /// optimization. Bypasses `emit_opcode` so `last_store` is set rather
    /// than cleared.
    fn emit_store_with_tracking(&mut self, op: u8, operand: [u8; 2]) {
        self.record_position_for_next_opcode();
        let position = self.bytecode.len();
        self.bytecode.push(op);
        self.bytecode.extend_from_slice(&operand);
        self.pop_stack(1);
        self.last_load = None;
        self.last_store = Some(LastStore {
            position,
            opcode: op,
            operand,
        });
    }

    /// Returns true if `load_op` is the LOAD_VAR counterpart of `store_op`
    /// (same type width).
    fn load_matches_store(load_op: u8, store_op: u8) -> bool {
        matches!(
            (store_op, load_op),
            (opcode::STORE_VAR_I32, opcode::LOAD_VAR_I32)
                | (opcode::STORE_VAR_I64, opcode::LOAD_VAR_I64)
                | (opcode::STORE_VAR_F32, opcode::LOAD_VAR_F32)
                | (opcode::STORE_VAR_F64, opcode::LOAD_VAR_F64)
        )
    }

    /// Returns the maximum stack depth reached during emission.
    pub fn max_stack_depth(&self) -> u16 {
        self.max_stack_depth
    }

    /// Returns the most temporary string buffers this function ever holds
    /// live at one time.
    pub fn max_temp_depth(&self) -> u16 {
        self.max_temp_depth
    }

    /// Returns the operand-stack depth the emitter is currently tracking.
    ///
    /// Pair with [`Self::reset_stack_depth`] around the arms of a branch.
    pub fn stack_depth(&self) -> u16 {
        self.current_stack_depth
    }

    /// Restores the tracked operand-stack depth to `depth`.
    ///
    /// The emitter counts depth in *emission* order, so it sums the arms of a
    /// branch as if they ran back to back. That is harmless for statement
    /// bodies, which are stack-neutral, but an expression arm leaves a value
    /// behind: without this, every conditionally-evaluated expression would
    /// inflate `max_stack_depth` by a slot. Record the depth before the first
    /// arm and reset to it before emitting the second, so the depth after the
    /// merge is the depth one arm actually delivers.
    ///
    /// This only corrects the emitter's own bookkeeping. What ships is checked
    /// against the real control-flow graph by
    /// [`ironplc_container::verify_stack_balance`].
    pub fn reset_stack_depth(&mut self, depth: u16) {
        self.current_stack_depth = depth;
    }

    fn push_stack(&mut self, count: u16) {
        self.current_stack_depth += count;
        if self.current_stack_depth > self.max_stack_depth {
            self.max_stack_depth = self.current_stack_depth;
        }
    }

    fn pop_stack(&mut self, count: u16) {
        self.current_stack_depth = self.current_stack_depth.saturating_sub(count);
    }

    /// Records that the instruction just emitted allocates a temp buffer.
    fn alloc_temp_buf(&mut self) {
        self.current_temp_depth = self.current_temp_depth.saturating_add(1);
        if self.current_temp_depth > self.max_temp_depth {
            self.max_temp_depth = self.current_temp_depth;
        }
    }

    /// Records that the instruction just emitted consumes a temp buffer.
    ///
    /// Saturates at zero, mirroring [`Self::pop_stack`]: a function whose
    /// body consumes a buffer its caller produced (a `STRING`-returning
    /// call) legitimately releases more than it allocated.
    fn release_temp_buf(&mut self) {
        self.current_temp_depth = self.current_temp_depth.saturating_sub(1);
    }
}

impl Default for Emitter {
    fn default() -> Self {
        Self::new()
    }
}

#[cfg(test)]
mod tests;
