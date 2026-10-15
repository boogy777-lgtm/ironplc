//! Parses and compiles conversions between numeric values and STRING.

use ironplc_container::opcode;
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::textual::Function;

use super::collect_positional_args;
use crate::compile::{CompileContext, OpType, OpWidth, Signedness, VarTypeInfo, NARROW_CHAR_WIDTH};
use crate::compile_expr::compile_expr;
use crate::compile_string::resolve_string_arg;
use crate::emit::Emitter;
use crate::type_info::resolve_type_name;

/// Describes a string ↔ numeric conversion direction.
pub(crate) enum StringConversion {
    /// Numeric → STRING (e.g., INT_TO_STRING, DWORD_TO_STRING).
    NumToString { source: VarTypeInfo },
    /// STRING → Numeric (e.g., STRING_TO_INT, STRING_TO_REAL).
    StringToNum { target: VarTypeInfo },
}

/// Checks if a function name is a string conversion (e.g., "int_to_string").
///
/// Returns `Some(StringConversion)` if the name matches `*_TO_STRING` or
/// `STRING_TO_*` and the non-string part is a recognized type name.
pub(crate) fn parse_string_conversion(name: &str) -> Option<StringConversion> {
    let upper = name.to_uppercase();
    let parts: Vec<&str> = upper.splitn(2, "_TO_").collect();
    if parts.len() != 2 {
        return None;
    }
    if parts[1] == "STRING" {
        let source = resolve_type_name(&Id::from(parts[0]))?;
        Some(StringConversion::NumToString { source })
    } else if parts[0] == "STRING" {
        let target = resolve_type_name(&Id::from(parts[1]))?;
        Some(StringConversion::StringToNum { target })
    } else {
        None
    }
}

/// Compiles a string ↔ numeric conversion function call.
pub(super) fn compile_string_conversion(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    func: &Function,
    conv: StringConversion,
) -> Result<(), Diagnostic> {
    let args = collect_positional_args(func);
    if args.len() != 1 {
        return Err(Diagnostic::todo_with_span(func.name.span()));
    }

    match conv {
        StringConversion::NumToString { source } => {
            let source_op_type: OpType = (source.op_width, source.signedness);
            compile_expr(emitter, ctx, args[0], source_op_type)?;

            let func_id = match (source.op_width, source.signedness) {
                (OpWidth::W32, Signedness::Signed) => opcode::builtin::CONV_I32_TO_STR,
                (OpWidth::W32, Signedness::Unsigned) => opcode::builtin::CONV_U32_TO_STR,
                (OpWidth::F32, _) => opcode::builtin::CONV_F32_TO_STR,
                _ => {
                    return Err(Diagnostic::todo_with_span(func.name.span()));
                }
            };
            emitter.emit_builtin(func_id);
            Ok(())
        }
        StringConversion::StringToNum { target } => {
            // STRING_TO_* parses Latin-1 digits, so a WSTRING argument has no
            // conversion -- P4034 rather than an encoding-mismatch trap.
            let data_offset =
                resolve_string_arg(emitter, ctx, args[0], &func.name.span(), NARROW_CHAR_WIDTH)?;
            let pool_index = ctx.add_i32_constant(data_offset as i32);
            emitter.emit_load_const_i32(pool_index);

            use opcode::builtin::str_to_num::{func_id as block_func_id, Target};
            // Every target is a policy-bearing conversion (ADR-0049): the
            // func_id names the target and both selected policies, the VM
            // range-checks against the target's own bounds, and no
            // truncation follows. `STRING_TO_SINT('300')` fails; it never
            // wraps to 44. A bit-string type has the signedness and value
            // width of the unsigned integer it aliases, so it lands on that
            // integer's target.
            let block = |target: Target| {
                block_func_id(
                    target,
                    ctx.string_to_num.non_numeric,
                    ctx.string_to_num.failure,
                )
            };
            let func_id = match (target.op_width, target.signedness, target.storage_bits) {
                (OpWidth::W32, Signedness::Unsigned, 32) => block(Target::U32),
                (OpWidth::W32, Signedness::Signed, 32) => block(Target::I32),
                (OpWidth::W32, Signedness::Unsigned, 8) => block(Target::U8),
                (OpWidth::W32, Signedness::Signed, 8) => block(Target::I8),
                (OpWidth::W32, Signedness::Unsigned, 16) => block(Target::U16),
                (OpWidth::W32, Signedness::Signed, 16) => block(Target::I16),
                (OpWidth::W64, Signedness::Unsigned, 64) => block(Target::U64),
                (OpWidth::W64, Signedness::Signed, 64) => block(Target::I64),
                (OpWidth::F32, _, _) => block(Target::F32),
                (OpWidth::F64, _, _) => block(Target::F64),
                // A 64-bit slot holds only the 64-bit value width.
                (OpWidth::W64, _, _) => {
                    return Err(Diagnostic::internal_error_at(Label::span(
                        func.name.span(),
                        "STRING_TO_* 64-bit target has no conversion",
                    )));
                }
                // A 32-bit slot holds only the value widths above. The
                // analyzer offers no other STRING_TO_* with a 32-bit
                // target (there is no STRING_TO_BOOL), so this is a
                // compiler bug, not a program error.
                (OpWidth::W32, _, _) => {
                    return Err(Diagnostic::internal_error_at(Label::span(
                        func.name.span(),
                        "STRING_TO_* target has no conversion",
                    )));
                }
            };
            emitter.emit_builtin(func_id);
            Ok(())
        }
    }
}
