//! Compiles standard library functions that take positional arguments and emit builtin opcodes.

use ironplc_container::opcode;
use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::{Expr, ExprKind, Function, SymbolicVariableKind, Variable};

use super::builtin_lookup::lookup_builtin;
use super::collect_positional_args;
use super::type_conversion::emit_conversion_opcode;
use crate::compile::{CompileContext, OpType, OpWidth, Signedness, VarTypeInfo, DEFAULT_OP_TYPE};
use crate::compile_expr::{compile_expr, op_type, storage_bits};
use crate::emit::Emitter;

/// Compiles a generic builtin function call via `lookup_builtin`.
///
/// All arguments are compiled with the same `op_type` and the function ID
/// is looked up by name.
pub(super) fn compile_generic_builtin(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    op_type: OpType,
) -> Result<(), Diagnostic> {
    let func_name = func.name.original().to_uppercase();

    let func_id = lookup_builtin(&func_name, op_type.0, op_type.1)
        .ok_or_else(|| Diagnostic::todo_with_span(func.name.span()))?;

    let expected_args = opcode::builtin::arg_count(func_id) as usize;

    let args = collect_positional_args(func);

    if args.len() != expected_args {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    let is_sel = func_name == "SEL";
    for (i, arg) in args.iter().enumerate() {
        // SEL's first argument (G) is always a BOOL/integer selector,
        // even when the remaining arguments are float.
        let arg_op_type = if is_sel && i == 0 {
            DEFAULT_OP_TYPE
        } else {
            op_type
        };
        compile_expr(emitter, ctx, arg, arg_op_type)?;
    }

    emitter.emit_builtin(func_id);
    Ok(())
}

/// Compiles the MOVE function form.
///
/// MOVE(IN) is equivalent to assignment. Takes a single argument and returns
/// it unchanged. No opcode is needed since the value is already on the stack.
pub(super) fn compile_move(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    op_type: OpType,
) -> Result<(), Diagnostic> {
    let args = collect_positional_args(func);

    if args.len() != 1 {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    compile_expr(emitter, ctx, args[0], op_type)?;
    // No additional opcode needed - the value is already on the stack

    Ok(())
}

/// Compiles TRUNC(IN) — truncates a real value toward zero.
///
/// The argument is compiled using its own (float) op_type derived from the
/// argument's resolved type. The result is converted to the target integer
/// type using the existing conversion opcodes.
pub(super) fn compile_trunc(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    target_op_type: OpType,
) -> Result<(), Diagnostic> {
    let args = collect_positional_args(func);

    if args.len() != 1 {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    // Determine the argument's float type from its resolved type.
    let arg_op_type = op_type(ctx, args[0])?;
    compile_expr(emitter, ctx, args[0], arg_op_type)?;

    // Build VarTypeInfo for source (float) and target (integer) to reuse
    // the existing conversion opcode emission.
    let source = VarTypeInfo {
        op_width: arg_op_type.0,
        signedness: arg_op_type.1,
        storage_bits: match arg_op_type.0 {
            OpWidth::F32 => 32,
            OpWidth::F64 => 64,
            _ => 32,
        },
    };
    let target = VarTypeInfo {
        op_width: target_op_type.0,
        signedness: target_op_type.1,
        storage_bits: match target_op_type.0 {
            OpWidth::W32 => 32,
            OpWidth::W64 => 64,
            _ => 32,
        },
    };
    emit_conversion_opcode(emitter, &source, &target);

    Ok(())
}

/// Compiles SIZEOF(IN) — returns the size in bytes of the argument's type.
///
/// SIZEOF is a compile-time constant: the argument is never evaluated at runtime.
/// For elementary types, the size is derived from the storage bit width.
/// For array variables, the total byte count is computed from element size × element count.
pub(super) fn compile_sizeof(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
) -> Result<(), Diagnostic> {
    let args = collect_positional_args(func);

    if args.len() != 1 {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    // Check if the argument is a variable that maps to an array.
    let size: u32 =
        if let ExprKind::Variable(Variable::Symbolic(SymbolicVariableKind::Named(ref named))) =
            args[0].kind
        {
            if let Some(array_info) = ctx.array_vars.get(&named.name) {
                let elem_bytes = array_info.element_var_type_info.storage_bits as u32 / 8;
                array_info.total_elements * elem_bytes
            } else {
                sizeof_from_expr_type(ctx, args[0])?
            }
        } else {
            sizeof_from_expr_type(ctx, args[0])?
        };

    let pool_index = ctx.add_i32_constant(size as i32);
    emitter.emit_load_const_i32(pool_index);
    Ok(())
}

/// Returns the size in bytes of an expression's value, from its `expr_type`.
fn sizeof_from_expr_type(ctx: &CompileContext, expr: &Expr) -> Result<u32, Diagnostic> {
    let bits = storage_bits(ctx, expr)?;
    // Ceiling division: types like BOOL (1 bit) still occupy 1 byte.
    Ok((bits as u32).div_ceil(8))
}

/// Compiles BCD_TO_INT(IN) — converts a BCD-encoded bit string to an integer.
///
/// The argument is compiled using its own (bit-string) op_type. The BCD
/// decoding opcode is selected based on the argument's storage bit width.
pub(super) fn compile_bcd_to_int(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    _target_op_type: OpType,
) -> Result<(), Diagnostic> {
    let args = collect_positional_args(func);

    if args.len() != 1 {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    let arg_op_type = op_type(ctx, args[0])?;
    let bits = storage_bits(ctx, args[0])?;
    compile_expr(emitter, ctx, args[0], arg_op_type)?;

    let func_id = match bits {
        8 => opcode::builtin::BCD_TO_INT_8,
        16 => opcode::builtin::BCD_TO_INT_16,
        32 => opcode::builtin::BCD_TO_INT_32,
        64 => opcode::builtin::BCD_TO_INT_64,
        _ => return Err(Diagnostic::todo_with_span(func.name.span())),
    };
    emitter.emit_builtin(func_id);
    Ok(())
}

/// Compiles INT_TO_BCD(IN) — converts an integer to a BCD-encoded bit string.
///
/// The argument is compiled using its own (integer) op_type. The BCD
/// encoding opcode is selected based on the target's operation width.
pub(super) fn compile_int_to_bcd(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    target_op_type: OpType,
) -> Result<(), Diagnostic> {
    let args = collect_positional_args(func);

    if args.len() != 1 {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    let arg_op_type = op_type(ctx, args[0])?;
    let bits = storage_bits(ctx, args[0])?;
    compile_expr(emitter, ctx, args[0], arg_op_type)?;

    let func_id = match (arg_op_type.0, bits) {
        (OpWidth::W32, 8) => opcode::builtin::INT_TO_BCD_8,
        (OpWidth::W32, 16) => opcode::builtin::INT_TO_BCD_16,
        (OpWidth::W32, 32) => opcode::builtin::INT_TO_BCD_32,
        (OpWidth::W64, 64) => opcode::builtin::INT_TO_BCD_64,
        _ => {
            // For wider target than source, select based on target width
            match target_op_type.0 {
                OpWidth::W32 => opcode::builtin::INT_TO_BCD_32,
                OpWidth::W64 => opcode::builtin::INT_TO_BCD_64,
                _ => return Err(Diagnostic::todo_with_span(func.name.span())),
            }
        }
    };
    emitter.emit_builtin(func_id);
    Ok(())
}

/// Compiles a MUX (multiplexer) function call.
///
/// MUX(K, IN0, IN1, ..., INn) selects one of the IN values based on the
/// integer selector K. The first argument K is always compiled as I32
/// (integer selector), while the remaining IN arguments use the caller's op_type.
///
/// The opcode encodes the number of IN arguments: `MUX_<WIDTH>_BASE + num_inputs`.
pub(super) fn compile_mux(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    op_type: OpType,
) -> Result<(), Diagnostic> {
    let args = collect_positional_args(func);

    // Must have at least 3 args (K + 2 IN values)
    if args.len() < 3 {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    let num_inputs = (args.len() - 1) as u16; // subtract K

    if num_inputs > opcode::builtin::MUX_MAX_INPUTS {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    let base = match op_type.0 {
        OpWidth::W32 => opcode::builtin::MUX_I32_BASE,
        OpWidth::W64 => opcode::builtin::MUX_I64_BASE,
        OpWidth::F32 => opcode::builtin::MUX_F32_BASE,
        OpWidth::F64 => opcode::builtin::MUX_F64_BASE,
    };
    let func_id = base + num_inputs;

    // Compile K (first arg) as integer
    compile_expr(emitter, ctx, args[0], DEFAULT_OP_TYPE)?;

    // Compile IN0..INn with the caller's op_type
    for arg in &args[1..] {
        compile_expr(emitter, ctx, arg, op_type)?;
    }

    emitter.emit_builtin(func_id);
    Ok(())
}

/// Compiles a bit shift or rotate function call (SHL, SHR, ROL, ROR).
///
/// Expects two positional arguments: IN (value) and N (shift count).
/// Emits the appropriate BUILTIN opcode based on function name and operand width.
/// For ROL/ROR on narrow types (BYTE, WORD), emits width-specific builtins
/// to ensure bits wrap correctly within the narrow type.
pub(super) fn compile_shift_rotate(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    op_type: OpType,
    name: &str,
) -> Result<(), Diagnostic> {
    let args = collect_positional_args(func);

    if args.len() != 2 {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    // Compile IN (value) with the inferred op_type
    compile_expr(emitter, ctx, args[0], op_type)?;
    // Compile N (shift count) — always as i32 for W32, i64 for W64
    let n_op_type = match op_type.0 {
        OpWidth::W64 => (OpWidth::W64, Signedness::Signed),
        _ => DEFAULT_OP_TYPE,
    };
    compile_expr(emitter, ctx, args[1], n_op_type)?;

    // Determine storage bits for narrow-type ROL/ROR selection
    let bits = storage_bits(ctx, args[0])?;

    let func_id = match (name, op_type.0) {
        ("shl", OpWidth::W64) => opcode::builtin::SHL_I64,
        ("shl", _) => opcode::builtin::SHL_I32,
        ("shr", OpWidth::W64) => opcode::builtin::SHR_I64,
        ("shr", _) => opcode::builtin::SHR_I32,
        ("rol", OpWidth::W64) => opcode::builtin::ROL_I64,
        ("rol", _) => match bits {
            8 => opcode::builtin::ROL_U8,
            16 => opcode::builtin::ROL_U16,
            _ => opcode::builtin::ROL_I32,
        },
        ("ror", OpWidth::W64) => opcode::builtin::ROR_I64,
        ("ror", _) => match bits {
            8 => opcode::builtin::ROR_U8,
            16 => opcode::builtin::ROR_U16,
            _ => opcode::builtin::ROR_I32,
        },
        _ => return Err(Diagnostic::todo_with_span(func.name.span())),
    };

    emitter.emit_builtin(func_id);
    Ok(())
}
