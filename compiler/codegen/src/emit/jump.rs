//! Emits jump and branch instructions, binds labels, and resolves jump offsets when the bytecode is finalised.

use ironplc_container::opcode;
use ironplc_container::VarIndex;

use super::{Emitter, Label, PendingPatch, UnpatchedCode};
use crate::optimize::OffsetMap;

/// Byte position of the branch operand within a `JMP`/`JMP_IF_NOT`
/// instruction: straight after the opcode.
const JMP_OPERAND_DELTA: usize = 1;

/// Byte position of the branch operand within a `CMP_BR_*` instruction:
/// after the opcode, the cmp_op byte, the variable index and the pool index.
const CMP_BR_OPERAND_DELTA: usize = 6;

impl Emitter {
    /// Creates a new unbound label for use as a jump target.
    pub fn create_label(&mut self) -> Label {
        let index = self.labels.len();
        self.labels.push(None);
        Label(index)
    }

    /// Binds a label to the current bytecode position.
    /// Binds a label to the current bytecode position.
    /// Clears the peephole trackers because a jump may land here.
    pub fn bind_label(&mut self, label: Label) {
        self.labels[label.0] = Some(self.bytecode.len());
        self.last_load = None;
        self.last_store = None;
    }

    /// Emits JMP with a placeholder offset targeting the given label.
    pub fn emit_jmp(&mut self, label: Label) {
        let instruction_offset = self.bytecode.len();
        self.emit_opcode(opcode::JMP);
        self.bytecode.extend_from_slice(&0i16.to_le_bytes());
        self.patches.push(PendingPatch {
            instruction_offset,
            operand_delta: JMP_OPERAND_DELTA,
            label,
        });
    }

    /// Emits JMP_IF_NOT with a placeholder offset targeting the given label.
    /// Pops the condition value from the stack.
    pub fn emit_jmp_if_not(&mut self, label: Label) {
        let instruction_offset = self.bytecode.len();
        self.emit_opcode(opcode::JMP_IF_NOT);
        self.bytecode.extend_from_slice(&0i16.to_le_bytes());
        self.patches.push(PendingPatch {
            instruction_offset,
            operand_delta: JMP_OPERAND_DELTA,
            label,
        });
        self.pop_stack(1);
    }

    /// Emits `CMP_BR_I32` (fused compare-and-branch on a 32-bit signed
    /// integer variable against a constant pool entry). Branches to `target`
    /// when `cmp_op(vars[var_index], const_pool[const_idx])` is true.
    /// Stack effect: 0.
    pub fn emit_cmp_br_i32(
        &mut self,
        cmp_op_byte: u8,
        var_index: VarIndex,
        const_idx: u16,
        target: Label,
    ) {
        self.emit_cmp_br(
            opcode::CMP_BR_I32,
            cmp_op_byte,
            var_index,
            const_idx,
            target,
        );
    }

    /// Emits `CMP_BR_I64` (fused compare-and-branch on a 64-bit signed
    /// integer variable against a constant pool entry). See `emit_cmp_br_i32`.
    pub fn emit_cmp_br_i64(
        &mut self,
        cmp_op_byte: u8,
        var_index: VarIndex,
        const_idx: u16,
        target: Label,
    ) {
        self.emit_cmp_br(
            opcode::CMP_BR_I64,
            cmp_op_byte,
            var_index,
            const_idx,
            target,
        );
    }

    fn emit_cmp_br(
        &mut self,
        op: u8,
        cmp_op_byte: u8,
        var_index: VarIndex,
        const_idx: u16,
        target: Label,
    ) {
        debug_assert!(
            opcode::cmp_op::is_valid(cmp_op_byte),
            "emit_cmp_br requires a valid cmp_op byte"
        );
        let instruction_offset = self.bytecode.len();
        self.emit_opcode(op);
        self.bytecode.push(cmp_op_byte);
        self.bytecode.extend_from_slice(&var_index.to_le_bytes());
        self.bytecode.extend_from_slice(&const_idx.to_le_bytes());
        debug_assert_eq!(
            self.bytecode.len() - instruction_offset,
            CMP_BR_OPERAND_DELTA,
            "CMP_BR_OPERAND_DELTA must name where the branch offset is written"
        );
        self.bytecode.extend_from_slice(&0i16.to_le_bytes());
        self.patches.push(PendingPatch {
            instruction_offset,
            operand_delta: CMP_BR_OPERAND_DELTA,
            label: target,
        });
        // Stack effect: 0 (no pushes, no pops).
    }

    /// Returns the accumulated bytecode with all pending jump patches resolved.
    ///
    /// Peephole optimizations (consecutive load → DUP, store-load → insert
    /// DUP before STORE) are applied inline during emission. The pass-based
    /// optimizer runs on the un-patched bytes, so it must be given
    /// [`Self::unpatched_code`] and its result handed back through
    /// [`Self::apply_optimized`] before this is called.
    pub fn bytecode(&mut self) -> &[u8] {
        self.patch_jumps();
        &self.bytecode
    }

    /// Returns the emitted bytes with their jump operands still unresolved,
    /// alongside the offsets those jumps target.
    ///
    /// The target set is derived from the pending patches rather than from
    /// every bound label: a label that is bound but never jumped to
    /// constrains nothing, and protecting its position would needlessly
    /// block a peephole there.
    #[expect(
        clippy::expect_used,
        reason = "labels are bound before the optimizer sees the emitter; this is the pipeline contract"
    )]
    pub(crate) fn unpatched_code(&self) -> UnpatchedCode<'_> {
        let jump_targets = self
            .patches
            .iter()
            .map(|patch| self.labels[patch.label.0].expect("label must be bound before optimizing"))
            .collect();
        UnpatchedCode {
            bytecode: &self.bytecode,
            jump_targets,
        }
    }

    /// Replaces the emitted bytes with the optimizer's output, moving every
    /// label and pending patch to where `offset_map` says its instruction now
    /// starts.
    ///
    /// The jump operands are still placeholders afterwards;
    /// [`Self::bytecode`] resolves them against the new positions.
    pub(crate) fn apply_optimized(&mut self, bytecode: Vec<u8>, offset_map: &OffsetMap) {
        // The optimizer never removes a branch instruction — no pass matches
        // a pair containing one — so a patch's instruction start always maps
        // to the same instruction rather than snapping forward to the next.
        let opcodes: Vec<u8> = self
            .patches
            .iter()
            .map(|patch| self.bytecode[patch.instruction_offset])
            .collect();

        self.bytecode = bytecode;
        for (patch, opcode) in self.patches.iter_mut().zip(opcodes) {
            patch.instruction_offset = offset_map[&patch.instruction_offset];
            debug_assert_eq!(
                self.bytecode[patch.instruction_offset], opcode,
                "the optimizer moved a branch instruction away from its patch"
            );
        }
        for label in self.labels.iter_mut().flatten() {
            *label = offset_map[label];
        }
    }

    /// Resolves all pending jump patches by computing relative offsets.
    #[expect(
        clippy::expect_used,
        reason = "labels are bound before patching; this is the pipeline contract"
    )]
    fn patch_jumps(&mut self) {
        for patch in self.patches.drain(..) {
            let label_pos =
                self.labels[patch.label.0].expect("label must be bound before patching");
            // Offset is relative to the byte after the i16 operand
            let patch_offset = patch.patch_offset();
            let next_pc = patch_offset + 2;
            let offset = (label_pos as isize - next_pc as isize) as i16;
            let bytes = offset.to_le_bytes();
            self.bytecode[patch_offset] = bytes[0];
            self.bytecode[patch_offset + 1] = bytes[1];
        }
    }
}
