//! Emits instructions that reach memory through references, array descriptors and region copies.

use ironplc_container::opcode;
use ironplc_container::VarIndex;

use super::Emitter;

impl Emitter {
    // --- Reference (indirect) ops ---
    emit_unaryop!(emit_load_indirect, opcode::LOAD_INDIRECT);

    /// Emits STORE_INDIRECT.
    /// Pops 2 (value and reference). Net: -2.
    pub fn emit_store_indirect(&mut self) {
        self.emit_opcode(opcode::STORE_INDIRECT);
        self.pop_stack(2);
    }

    /// Emits LOAD_ARRAY with var_index and desc_index operands.
    /// Pops 1 (flat index already on stack), pushes 1 (element value). Net: 0.
    pub fn emit_load_array(&mut self, var_index: VarIndex, desc_index: u16) {
        self.emit_opcode(opcode::LOAD_ARRAY);
        self.bytecode.extend_from_slice(&var_index.to_le_bytes());
        self.bytecode.extend_from_slice(&desc_index.to_le_bytes());
        // Pop index, push value = no net change
        self.pop_stack(1);
        self.push_stack(1);
    }

    /// Emits STORE_ARRAY with var_index and desc_index operands.
    /// Pops 2 (value and flat index). Net: -2.
    pub fn emit_store_array(&mut self, var_index: VarIndex, desc_index: u16) {
        self.emit_opcode(opcode::STORE_ARRAY);
        self.bytecode.extend_from_slice(&var_index.to_le_bytes());
        self.bytecode.extend_from_slice(&desc_index.to_le_bytes());
        self.pop_stack(2);
    }

    /// Emits LOAD_ARRAY_DEREF with ref_var_index and desc_index operands.
    /// Pops 1 (flat index), pushes 1 (element value). Net: 0.
    pub fn emit_load_array_deref(&mut self, ref_var_index: VarIndex, desc_index: u16) {
        self.emit_opcode(opcode::LOAD_ARRAY_DEREF);
        self.bytecode
            .extend_from_slice(&ref_var_index.to_le_bytes());
        self.bytecode.extend_from_slice(&desc_index.to_le_bytes());
        self.pop_stack(1);
        self.push_stack(1);
    }

    /// Emits STORE_ARRAY_DEREF with ref_var_index and desc_index operands.
    /// Pops 2 (value and flat index). Net: -2.
    pub fn emit_store_array_deref(&mut self, ref_var_index: VarIndex, desc_index: u16) {
        self.emit_opcode(opcode::STORE_ARRAY_DEREF);
        self.bytecode
            .extend_from_slice(&ref_var_index.to_le_bytes());
        self.bytecode.extend_from_slice(&desc_index.to_le_bytes());
        self.pop_stack(2);
    }

    /// Emits COPY_REGION with dst_var, dst_desc and src_desc operands.
    ///
    /// The source's data-region offset must already be on the stack. The copy
    /// length is not emitted — the VM derives it from the two descriptors and
    /// traps if they disagree. Pops 1 (source offset). Net: -1.
    pub fn emit_copy_region(&mut self, dst_var: VarIndex, dst_desc: u16, src_desc: u16) {
        self.emit_opcode(opcode::COPY_REGION);
        self.bytecode.extend_from_slice(&dst_var.to_le_bytes());
        self.bytecode.extend_from_slice(&dst_desc.to_le_bytes());
        self.bytecode.extend_from_slice(&src_desc.to_le_bytes());
        self.pop_stack(1);
    }

    /// Emits STR_INIT_ARRAY with var_index and desc_index operands.
    /// Initializes all string headers in an array. No stack effect.
    pub fn emit_str_init_array(&mut self, var_index: VarIndex, desc_index: u16) {
        self.emit_opcode(opcode::STR_INIT_ARRAY);
        self.bytecode.extend_from_slice(&var_index.to_le_bytes());
        self.bytecode.extend_from_slice(&desc_index.to_le_bytes());
    }

    /// Emits STR_LOAD_ARRAY_ELEM with var_index and desc_index operands.
    /// Pops flat_index, pushes buf_idx. Net: 0.
    pub fn emit_str_load_array_elem(&mut self, var_index: VarIndex, desc_index: u16) {
        self.emit_opcode(opcode::STR_LOAD_ARRAY_ELEM);
        self.bytecode.extend_from_slice(&var_index.to_le_bytes());
        self.bytecode.extend_from_slice(&desc_index.to_le_bytes());
        self.pop_stack(1);
        self.push_stack(1);
        self.alloc_temp_buf();
    }

    /// Emits STR_STORE_ARRAY_ELEM with var_index and desc_index operands.
    /// Pops flat_index and buf_idx. Net: -2.
    pub fn emit_str_store_array_elem(&mut self, var_index: VarIndex, desc_index: u16) {
        self.emit_opcode(opcode::STR_STORE_ARRAY_ELEM);
        self.bytecode.extend_from_slice(&var_index.to_le_bytes());
        self.bytecode.extend_from_slice(&desc_index.to_le_bytes());
        self.pop_stack(2);
        self.release_temp_buf();
    }
}
