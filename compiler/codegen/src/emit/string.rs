//! Emits the STRING and WSTRING instructions that move text between the data region and temporary buffers.

use ironplc_container::opcode;
use ironplc_container::CharWidth;

use super::Emitter;

impl Emitter {
    /// Emits STR_INIT with data_offset, max_length, and char_width operands.
    /// Initializes a STRING/WSTRING variable's header in the data region. The
    /// `char_width` (1 = narrow STRING, 2 = wide WSTRING) is written into the
    /// header so the VM can verify encoding and scale byte spans (ADR-0035).
    pub fn emit_str_init(&mut self, data_offset: u32, max_length: u16, char_width: CharWidth) {
        self.emit_opcode(opcode::STR_INIT);
        self.bytecode.extend_from_slice(&data_offset.to_le_bytes());
        self.bytecode.extend_from_slice(&max_length.to_le_bytes());
        self.bytecode.push(char_width.byte_width());
        // No stack effect.
    }

    /// Emits LOAD_CONST_STR with a constant pool index.
    /// Loads a string literal from the constant pool into a temp buffer and pushes buf_idx.
    pub fn emit_load_const_str(&mut self, pool_index: u16) {
        self.emit_opcode(opcode::LOAD_CONST_STR);
        self.bytecode.extend_from_slice(&pool_index.to_le_bytes());
        self.push_stack(1);
        self.alloc_temp_buf();
    }

    /// Emits STR_STORE_VAR with a data_offset operand.
    /// Pops buf_idx from the stack and copies the temp buffer contents to the data region.
    pub fn emit_str_store_var(&mut self, data_offset: u32) {
        self.emit_opcode(opcode::STR_STORE_VAR);
        self.bytecode.extend_from_slice(&data_offset.to_le_bytes());
        self.pop_stack(1);
        self.release_temp_buf();
    }

    /// Emits STR_LOAD_VAR with a data_offset operand.
    /// Copies a string from the data region into a temp buffer and pushes buf_idx.
    pub fn emit_str_load_var(&mut self, data_offset: u32) {
        self.emit_opcode(opcode::STR_LOAD_VAR);
        self.bytecode.extend_from_slice(&data_offset.to_le_bytes());
        self.push_stack(1);
        self.alloc_temp_buf();
    }

    /// Emits LEN_STR with a data_offset operand.
    /// Reads the current length of a STRING variable from the data region
    /// and pushes the result as an i32.
    pub fn emit_len_str(&mut self, data_offset: u32) {
        self.emit_opcode(opcode::LEN_STR);
        self.bytecode.extend_from_slice(&data_offset.to_le_bytes());
        self.push_stack(1);
    }

    /// Emits FIND_STR with two data_offset operands.
    /// Finds the first occurrence of IN2 within IN1 and pushes the
    /// 1-based position as an i32 (0 if not found).
    pub fn emit_find_str(&mut self, in1_data_offset: u32, in2_data_offset: u32) {
        self.emit_opcode(opcode::FIND_STR);
        self.bytecode
            .extend_from_slice(&in1_data_offset.to_le_bytes());
        self.bytecode
            .extend_from_slice(&in2_data_offset.to_le_bytes());
        self.push_stack(1);
    }

    /// Emits REPLACE_STR with two data_offset operands.
    /// Pops P (i32) then L (i32) from stack, replaces L characters at
    /// position P in IN1 with IN2, and pushes the result buf_idx.
    pub fn emit_replace_str(&mut self, in1_data_offset: u32, in2_data_offset: u32) {
        self.emit_opcode(opcode::REPLACE_STR);
        self.bytecode
            .extend_from_slice(&in1_data_offset.to_le_bytes());
        self.bytecode
            .extend_from_slice(&in2_data_offset.to_le_bytes());
        // Net effect: pop 2 (L, P), push 1 (buf_idx) = pop 1
        self.pop_stack(1);
        self.alloc_temp_buf();
    }

    /// Emits INSERT_STR with two data_offset operands.
    /// Pops P (i32) from stack, inserts IN2 into IN1 after position P,
    /// and pushes the result buf_idx.
    pub fn emit_insert_str(&mut self, in1_data_offset: u32, in2_data_offset: u32) {
        self.emit_opcode(opcode::INSERT_STR);
        self.bytecode
            .extend_from_slice(&in1_data_offset.to_le_bytes());
        self.bytecode
            .extend_from_slice(&in2_data_offset.to_le_bytes());
        // Net effect: pop 1 (P), push 1 (buf_idx) = 0
        self.alloc_temp_buf();
    }

    /// Emits DELETE_STR with a data_offset operand.
    /// Pops P (i32) then L (i32) from stack, deletes L characters from
    /// IN1 starting at position P, and pushes the result buf_idx.
    pub fn emit_delete_str(&mut self, in1_data_offset: u32) {
        self.emit_opcode(opcode::DELETE_STR);
        self.bytecode
            .extend_from_slice(&in1_data_offset.to_le_bytes());
        // Net effect: pop 2 (L, P), push 1 (buf_idx) = pop 1
        self.pop_stack(1);
        self.alloc_temp_buf();
    }

    /// Emits LEFT_STR with a data_offset operand.
    /// Pops L (i32) from stack, returns the leftmost L characters of IN,
    /// and pushes the result buf_idx.
    pub fn emit_left_str(&mut self, in_data_offset: u32) {
        self.emit_opcode(opcode::LEFT_STR);
        self.bytecode
            .extend_from_slice(&in_data_offset.to_le_bytes());
        // Net effect: pop 1 (L), push 1 (buf_idx) = 0
        self.alloc_temp_buf();
    }

    /// Emits RIGHT_STR with a data_offset operand.
    /// Pops L (i32) from stack, returns the rightmost L characters of IN,
    /// and pushes the result buf_idx.
    pub fn emit_right_str(&mut self, in_data_offset: u32) {
        self.emit_opcode(opcode::RIGHT_STR);
        self.bytecode
            .extend_from_slice(&in_data_offset.to_le_bytes());
        // Net effect: pop 1 (L), push 1 (buf_idx) = 0
        self.alloc_temp_buf();
    }

    /// Emits MID_STR with a data_offset operand.
    /// Pops P (i32) then L (i32) from stack, returns L characters from
    /// IN starting at position P, and pushes the result buf_idx.
    pub fn emit_mid_str(&mut self, in_data_offset: u32) {
        self.emit_opcode(opcode::MID_STR);
        self.bytecode
            .extend_from_slice(&in_data_offset.to_le_bytes());
        // Net effect: pop 2 (L, P), push 1 (buf_idx) = pop 1
        self.pop_stack(1);
        self.alloc_temp_buf();
    }

    /// Emits CONCAT_STR with two data_offset operands.
    /// Concatenates IN1 and IN2, pushes the result buf_idx.
    pub fn emit_concat_str(&mut self, in1_data_offset: u32, in2_data_offset: u32) {
        self.emit_opcode(opcode::CONCAT_STR);
        self.bytecode
            .extend_from_slice(&in1_data_offset.to_le_bytes());
        self.bytecode
            .extend_from_slice(&in2_data_offset.to_le_bytes());
        self.push_stack(1);
        self.alloc_temp_buf();
    }
}
