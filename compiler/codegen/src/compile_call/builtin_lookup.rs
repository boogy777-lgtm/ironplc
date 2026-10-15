//! Maps a standard library function name and operation width to its builtin opcode.

use ironplc_container::opcode;

use crate::compile::{OpWidth, Signedness};

/// Builds the opcode for a builtin defined across all four operation widths
/// (both integer widths and both float widths), independent of signedness.
///
/// Used for the identically-shaped EXPT/ABS/SEL arms of [`lookup_builtin`].
macro_rules! numeric_builtin {
    ($op_width:expr, $i32_op:path, $i64_op:path, $f32_op:path, $f64_op:path) => {
        Some(match $op_width {
            OpWidth::W32 => $i32_op,
            OpWidth::W64 => $i64_op,
            OpWidth::F32 => $f32_op,
            OpWidth::F64 => $f64_op,
        })
    };
}

/// Builds the opcode for a builtin whose integer variants distinguish
/// signedness but whose float variants do not.
///
/// Used for the identically-shaped MIN/MAX/LIMIT arms of [`lookup_builtin`].
macro_rules! signed_numeric_builtin {
    ($op_width:expr, $signedness:expr,
     $i32_op:path, $u32_op:path, $i64_op:path, $u64_op:path,
     $f32_op:path, $f64_op:path) => {
        Some(match ($op_width, $signedness) {
            (OpWidth::W32, Signedness::Signed) => $i32_op,
            (OpWidth::W32, Signedness::Unsigned) => $u32_op,
            (OpWidth::W64, Signedness::Signed) => $i64_op,
            (OpWidth::W64, Signedness::Unsigned) => $u64_op,
            (OpWidth::F32, _) => $f32_op,
            (OpWidth::F64, _) => $f64_op,
        })
    };
}

/// Builds the opcode for a float-only (transcendental) builtin. The integer
/// operation widths have no variant and yield `None`.
///
/// Used for the many identically-shaped SQRT/LN/.../ATAN2 arms of
/// [`lookup_builtin`].
macro_rules! float_builtin {
    ($op_width:expr, $f32_op:path, $f64_op:path) => {
        match $op_width {
            OpWidth::F32 => Some($f32_op),
            OpWidth::F64 => Some($f64_op),
            OpWidth::W32 | OpWidth::W64 => None,
        }
    };
}

/// Returns the builtin opcode for a named standard library function, if known.
///
/// The `op_width` selects the correct width variant and `signedness` selects
/// the signed/unsigned variant for functions that distinguish them.
///
/// The arms delegate to the `*_builtin!` macros above, which each capture one
/// recurring arm shape (all-widths, signed/unsigned, float-only). The opcode
/// identifiers are still written verbatim per arm — the workspace has no
/// `paste` crate to concatenate them, and spelling them out keeps every opcode
/// greppable — while the macros remove the ~55 lines of duplicated `match`
/// scaffolding the arms would otherwise repeat.
pub(super) fn lookup_builtin(name: &str, op_width: OpWidth, signedness: Signedness) -> Option<u16> {
    use opcode::builtin;
    match name.to_uppercase().as_str() {
        "EXPT" => numeric_builtin!(
            op_width,
            builtin::EXPT_I32,
            builtin::EXPT_I64,
            builtin::EXPT_F32,
            builtin::EXPT_F64
        ),
        "ABS" => numeric_builtin!(
            op_width,
            builtin::ABS_I32,
            builtin::ABS_I64,
            builtin::ABS_F32,
            builtin::ABS_F64
        ),
        "SEL" => numeric_builtin!(
            op_width,
            builtin::SEL_I32,
            builtin::SEL_I64,
            builtin::SEL_F32,
            builtin::SEL_F64
        ),
        "MIN" => signed_numeric_builtin!(
            op_width,
            signedness,
            builtin::MIN_I32,
            builtin::MIN_U32,
            builtin::MIN_I64,
            builtin::MIN_U64,
            builtin::MIN_F32,
            builtin::MIN_F64
        ),
        "MAX" => signed_numeric_builtin!(
            op_width,
            signedness,
            builtin::MAX_I32,
            builtin::MAX_U32,
            builtin::MAX_I64,
            builtin::MAX_U64,
            builtin::MAX_F32,
            builtin::MAX_F64
        ),
        "LIMIT" => signed_numeric_builtin!(
            op_width,
            signedness,
            builtin::LIMIT_I32,
            builtin::LIMIT_U32,
            builtin::LIMIT_I64,
            builtin::LIMIT_U64,
            builtin::LIMIT_F32,
            builtin::LIMIT_F64
        ),
        "SQRT" => float_builtin!(op_width, builtin::SQRT_F32, builtin::SQRT_F64),
        "LN" => float_builtin!(op_width, builtin::LN_F32, builtin::LN_F64),
        "LOG" => float_builtin!(op_width, builtin::LOG_F32, builtin::LOG_F64),
        "EXP" => float_builtin!(op_width, builtin::EXP_F32, builtin::EXP_F64),
        "SIN" => float_builtin!(op_width, builtin::SIN_F32, builtin::SIN_F64),
        "COS" => float_builtin!(op_width, builtin::COS_F32, builtin::COS_F64),
        "TAN" => float_builtin!(op_width, builtin::TAN_F32, builtin::TAN_F64),
        "ASIN" => float_builtin!(op_width, builtin::ASIN_F32, builtin::ASIN_F64),
        "ACOS" => float_builtin!(op_width, builtin::ACOS_F32, builtin::ACOS_F64),
        "ATAN" => float_builtin!(op_width, builtin::ATAN_F32, builtin::ATAN_F64),
        "ATAN2" => float_builtin!(op_width, builtin::ATAN2_F32, builtin::ATAN2_F64),
        // Compiler intrinsics (reserved `__` namespace): real-preserving
        // truncation and floating modulo, ANY_REAL with the width selecting
        // the F32/F64 builtin variant.
        "__TRUNC" => float_builtin!(op_width, builtin::TRUNC_F32, builtin::TRUNC_F64),
        "__MOD" => float_builtin!(op_width, builtin::MOD_F32, builtin::MOD_F64),
        _ => None,
    }
}
