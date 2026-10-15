//! Emits built-in, function block, method and function call instructions, and returns.

use ironplc_container::opcode;
use ironplc_container::{FunctionId, VarIndex};

use super::Emitter;

impl Emitter {
    /// Emits BUILTIN with a function ID.
    /// All builtins pop `arg_count` values and push one result.
    /// The arg count is looked up from `opcode::builtin::arg_count()`.
    pub fn emit_builtin(&mut self, func_id: u16) {
        self.emit_opcode(opcode::BUILTIN);
        self.bytecode.extend_from_slice(&func_id.to_le_bytes());
        // Net effect: pop arg_count, push 1 = pop (arg_count - 1)
        let arg_count = opcode::builtin::arg_count(func_id);
        if arg_count > 1 {
            self.pop_stack(arg_count - 1);
        }
        if opcode::builtin::allocates_temp_buf(func_id) {
            self.alloc_temp_buf();
        }
    }

    /// Emits FB_LOAD_INSTANCE with a variable index.
    pub fn emit_fb_load_instance(&mut self, var_index: VarIndex) {
        self.emit_opcode(opcode::FB_LOAD_INSTANCE);
        self.bytecode.extend_from_slice(&var_index.to_le_bytes());
        self.push_stack(1);
    }

    /// Emits FB_STORE_PARAM with a field index.
    /// Pops one value (the parameter); fb_ref remains on stack.
    pub fn emit_fb_store_param(&mut self, field: u8) {
        self.emit_opcode(opcode::FB_STORE_PARAM);
        self.bytecode.push(field);
        self.pop_stack(1);
    }

    /// Emits FB_LOAD_PARAM with a field index.
    /// Pushes one value (the output parameter); fb_ref remains on stack.
    pub fn emit_fb_load_param(&mut self, field: u8) {
        self.emit_opcode(opcode::FB_LOAD_PARAM);
        self.bytecode.push(field);
        self.push_stack(1);
    }

    /// Emits FB_CALL with a type_id.
    /// Net stack effect: 0 (fb_ref stays on stack).
    pub fn emit_fb_call(&mut self, type_id: u16) {
        self.emit_opcode(opcode::FB_CALL);
        self.bytecode.extend_from_slice(&type_id.to_le_bytes());
    }

    /// Emits METHOD_CALL (OOP extension, ADR-0041 Phase 1 static dispatch).
    ///
    /// `fb_ref` (pushed by a preceding [`Emitter::emit_fb_load_instance`])
    /// remains on the stack, matching [`Emitter::emit_fb_call`]. Pops
    /// `num_params` arguments from above `fb_ref`, then -- if the method
    /// has a return type -- pushes the return value on top of `fb_ref`
    /// (the caller is responsible for discarding both, in that order,
    /// once done with the call).
    #[expect(
        clippy::too_many_arguments,
        reason = "it encodes the METHOD_CALL instruction operand for operand; the operands are the instruction"
    )]
    pub fn emit_method_call(
        &mut self,
        function_id: FunctionId,
        field_var_off: VarIndex,
        num_fields: u8,
        param_var_off: VarIndex,
        num_params: u16,
        has_return_value: bool,
        callee_max_stack: u16,
    ) {
        self.emit_opcode(opcode::METHOD_CALL);
        self.bytecode.extend_from_slice(&function_id.to_le_bytes());
        self.bytecode
            .extend_from_slice(&field_var_off.to_le_bytes());
        self.bytecode.push(num_fields);
        self.bytecode
            .extend_from_slice(&param_var_off.to_le_bytes());
        if num_params > 0 {
            self.pop_stack(num_params);
        }
        // Account for the callee's stack usage on the shared stack.
        if callee_max_stack > 0 {
            self.push_stack(callee_max_stack);
            self.pop_stack(callee_max_stack);
        }
        if has_return_value {
            self.push_stack(1);
        }
    }

    /// Emits CALL with a function ID and variable offset.
    /// Pops `num_params` arguments and pushes one return value.
    /// `var_offset` is the absolute variable table index where the
    /// function's parameters start.
    pub fn emit_call(
        &mut self,
        function_id: FunctionId,
        num_params: u16,
        var_offset: VarIndex,
        callee_max_stack: u16,
    ) {
        self.emit_opcode(opcode::CALL);
        self.bytecode.extend_from_slice(&function_id.to_le_bytes());
        self.bytecode.extend_from_slice(&var_offset.to_le_bytes());
        if num_params > 0 {
            self.pop_stack(num_params);
        }
        // Account for the callee's stack usage on the shared stack.
        // The callee will use up to callee_max_stack slots on top of the
        // current depth, then leave exactly one return value.
        if callee_max_stack > 0 {
            self.push_stack(callee_max_stack);
            self.pop_stack(callee_max_stack);
        }
        self.push_stack(1);
    }

    /// Emits RET (return with value on stack).
    pub fn emit_ret(&mut self) {
        self.emit_opcode(opcode::RET);
    }

    /// Emits RET_VOID.
    pub fn emit_ret_void(&mut self) {
        self.emit_opcode(opcode::RET_VOID);
    }
}
