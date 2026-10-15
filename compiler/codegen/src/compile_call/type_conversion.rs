//! Compiles numeric type conversion functions (INT_TO_REAL, ...) and emits their conversion opcodes.

use ironplc_container::opcode;
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::Function;

use super::collect_positional_args;
use crate::compile::{CompileContext, OpType, OpWidth, Signedness, VarTypeInfo};
use crate::compile_expr::{compile_expr, emit_truncation};
use crate::emit::Emitter;
use crate::type_info::resolve_type_name;

/// Compiles a type conversion function call (e.g., INT_TO_REAL).
///
/// Unlike generic builtins, conversion functions have different source and
/// target types. The argument is compiled with the source type's OpType,
/// then a conversion opcode (if needed) transforms the value to the target
/// representation.
pub(super) fn compile_type_conversion(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    source: VarTypeInfo,
    target: VarTypeInfo,
) -> Result<(), Diagnostic> {
    let source_op_type: OpType = (source.op_width, source.signedness);

    let args = collect_positional_args(func);

    if args.len() != 1 {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    compile_expr(emitter, ctx, args[0], source_op_type)?;

    // Integer-to-boolean needs a dedicated opcode (non-zero → 1, zero → 0)
    // rather than the generic conversion + truncation path, because
    // truncation would only keep the lowest bit instead of testing for zero.
    if target.storage_bits == 1 {
        match source.op_width {
            OpWidth::W32 => emitter.emit_builtin(opcode::builtin::CONV_I32_TO_BOOL),
            OpWidth::W64 => emitter.emit_builtin(opcode::builtin::CONV_I64_TO_BOOL),
            _ => {
                return Err(Diagnostic::todo_with_span(func.name.span()));
            }
        }
    } else {
        emit_conversion_opcode(emitter, &source, &target);
        emit_truncation(emitter, target);
    }

    Ok(())
}

/// Emits the appropriate conversion BUILTIN opcode for the source->target
/// type transition. Does nothing for same-domain integer conversions that
/// are handled by the Slot's sign-extension and truncation.
pub(crate) fn emit_conversion_opcode(
    emitter: &mut Emitter,
    source: &VarTypeInfo,
    target: &VarTypeInfo,
) {
    use OpWidth::*;
    use Signedness::*;

    match (
        source.op_width,
        source.signedness,
        target.op_width,
        target.signedness,
    ) {
        // Same OpWidth: no conversion needed (truncation handles sub-width)
        (W32, _, W32, _) | (W64, _, W64, _) => {}

        // W32 signed -> W64: sign extension already in Slot, no-op
        (W32, Signed, W64, _) => {}

        // W32 unsigned -> W64: need zero-extension
        (W32, Unsigned, W64, _) => {
            emitter.emit_builtin(opcode::builtin::CONV_U32_TO_I64);
        }

        // W64 -> W32: as_i32() truncation at store time, no-op
        (W64, _, W32, _) => {}

        // Integer -> Float
        (W32, Signed, F32, _) => emitter.emit_builtin(opcode::builtin::CONV_I32_TO_F32),
        (W32, Signed, F64, _) => emitter.emit_builtin(opcode::builtin::CONV_I32_TO_F64),
        (W64, Signed, F32, _) => emitter.emit_builtin(opcode::builtin::CONV_I64_TO_F32),
        (W64, Signed, F64, _) => emitter.emit_builtin(opcode::builtin::CONV_I64_TO_F64),
        (W32, Unsigned, F32, _) => emitter.emit_builtin(opcode::builtin::CONV_U32_TO_F32),
        (W32, Unsigned, F64, _) => emitter.emit_builtin(opcode::builtin::CONV_U32_TO_F64),
        (W64, Unsigned, F32, _) => emitter.emit_builtin(opcode::builtin::CONV_U64_TO_F32),
        (W64, Unsigned, F64, _) => emitter.emit_builtin(opcode::builtin::CONV_U64_TO_F64),

        // Float -> Integer
        (F32, _, W32, Signed) => emitter.emit_builtin(opcode::builtin::CONV_F32_TO_I32),
        (F32, _, W64, Signed) => emitter.emit_builtin(opcode::builtin::CONV_F32_TO_I64),
        (F64, _, W32, Signed) => emitter.emit_builtin(opcode::builtin::CONV_F64_TO_I32),
        (F64, _, W64, Signed) => emitter.emit_builtin(opcode::builtin::CONV_F64_TO_I64),
        (F32, _, W32, Unsigned) => emitter.emit_builtin(opcode::builtin::CONV_F32_TO_U32),
        (F32, _, W64, Unsigned) => emitter.emit_builtin(opcode::builtin::CONV_F32_TO_U64),
        (F64, _, W32, Unsigned) => emitter.emit_builtin(opcode::builtin::CONV_F64_TO_U32),
        (F64, _, W64, Unsigned) => emitter.emit_builtin(opcode::builtin::CONV_F64_TO_U64),

        // Float -> Float
        (F32, _, F64, _) => emitter.emit_builtin(opcode::builtin::CONV_F32_TO_F64),
        (F64, _, F32, _) => emitter.emit_builtin(opcode::builtin::CONV_F64_TO_F32),

        // Same float width (shouldn't happen, but handle gracefully)
        (F32, _, F32, _) | (F64, _, F64, _) => {}
    }
}

/// Checks if a function name is a type conversion (e.g., "int_to_real").
pub(super) fn parse_type_conversion(name: &str) -> Option<(VarTypeInfo, VarTypeInfo)> {
    let upper = name.to_uppercase();
    let parts: Vec<&str> = upper.splitn(2, "_TO_").collect();
    if parts.len() != 2 {
        return None;
    }
    let source = resolve_type_name(&Id::from(parts[0]))?;
    let target = resolve_type_name(&Id::from(parts[1]))?;
    Some((source, target))
}
