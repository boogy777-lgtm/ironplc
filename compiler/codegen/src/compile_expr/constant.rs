//! Compilation of constant literals (integer, real, boolean, string, time and bit-string) to constant loads.

use ironplc_dsl::common::{Boolean, ConstantKind, SignedInteger};
use ironplc_dsl::core::{Located, SourceSpan};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_problems::text;

use crate::compile::{
    encode_string_literal, CompileContext, OpType, OpWidth, Signedness, NARROW_CHAR_WIDTH,
};
use crate::emit::Emitter;

/// Compiles the integer count a time-like literal stores, pushing it onto the
/// stack: milliseconds for a duration or a time of day, seconds since
/// 1970-01-01 for a date or a date-and-time.
///
/// The count is held to the range the operation type names, which is the
/// storage it is about to go into: signed for a duration (ADR-0021 -- a
/// duration can be negative), unsigned for the calendar types (ADR-0025), at
/// whichever of the two widths the type uses. A count outside it is reported
/// as `problem` rather than truncated, because truncating leaves a value that
/// is not the one the program wrote -- `T#30d` kept its low 32 bits and became
/// -1,702,967,296 ms, a *negative* 19.7 days, with nothing said about it.
///
/// `count` is an `i128` so that the check happens before any narrowing: every
/// storage this can target, up to `u64::MAX`, and every value a literal can
/// carry, up to a duration's own `i128` millisecond count, fit it.
///
/// Every one of these is a count rather than a measurement, so no
/// floating-point type holds one and the analyzer rejects the assignment that
/// would ask for it (P4035). Reaching a float width here is a broken
/// invariant rather than a missing capability, and saying so beats what the
/// catch-all these arms shared used to do: emit an integer load, leaving the
/// count's bit pattern in a float slot to be read back as a number unrelated
/// to the literal.
fn compile_time_count(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    count: i128,
    literal: &str,
    span: &SourceSpan,
    op_type: OpType,
) -> Result<(), Diagnostic> {
    // Each arm states the range it stores, so a width added to `OpWidth`
    // has to say what a count means at that width rather than inheriting an
    // answer from a catch-all.
    match op_type.0 {
        OpWidth::W32 => {
            let value = within_storage(count, 32, op_type.1, literal, span)?;
            let pool_index = ctx.add_i32_constant(value as i32);
            emitter.emit_load_const_i32(pool_index);
        }
        OpWidth::W64 => {
            let value = within_storage(count, 64, op_type.1, literal, span)?;
            let pool_index = ctx.add_i64_constant(value as i64);
            emitter.emit_load_const_i64(pool_index);
        }
        // A count is not a measurement, so no floating-point type holds one,
        // and the analyzer rejects the assignment that would ask for it
        // (P4035). Reaching here means analysis was skipped, which is a
        // broken invariant rather than a missing capability: the catch-all
        // this replaced emitted an integer load, leaving the count's bit
        // pattern in a float slot to be read back as a number unrelated to
        // the literal.
        OpWidth::F32 | OpWidth::F64 => {
            return Err(Diagnostic::internal_error_at(Label::span(
                span.clone(),
                format!("{literal} compiled at a floating-point operation width"),
            )))
        }
    }
    Ok(())
}

/// Returns `count`, or an internal error when a `bits`-wide integer of
/// `signedness` cannot hold it.
///
/// `rule_temporal_literal_range` holds every temporal literal to the range its
/// own type gives it, and a literal only reaches storage at least that wide --
/// narrowing one into the shorter type is rejected too. So by the time a count
/// arrives here it fits, and a count that does not is a broken invariant
/// rather than something to report against the program: the diagnostic names
/// the compiler, not the source.
///
/// The check remains because being wrong here is silent. Truncating a count
/// emits a different value than the program wrote -- `T#30d` became a
/// *negative* 19.7 days -- which no test of the program's behaviour would
/// attribute to codegen.
fn within_storage(
    count: i128,
    bits: u32,
    signedness: Signedness,
    literal: &str,
    span: &SourceSpan,
) -> Result<i128, Diagnostic> {
    let signed = signedness == Signedness::Signed;
    if !ironplc_analyzer::value_range::fits(count, bits, signed) {
        let (minimum, maximum) = ironplc_analyzer::value_range::for_integer(bits, signed);
        return Err(Diagnostic::internal_error_at(Label::span(
            span.clone(),
            format!(
                "{literal} holds {count}, outside the range {minimum} to {maximum} its type stores"
            ),
        )));
    }
    Ok(count)
}

/// Compiles a constant literal, pushing it onto the stack.
pub(crate) fn compile_constant(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    constant: &ConstantKind,
    op_type: OpType,
) -> Result<(), Diagnostic> {
    match constant {
        ConstantKind::IntegerLiteral(lit) => {
            let span = lit.value.value.span();
            match op_type {
                (OpWidth::W32, Signedness::Signed) => {
                    let value = if lit.value.is_neg {
                        let unsigned = lit.value.value.value as i128;
                        let signed = -unsigned;
                        i32::try_from(signed).map_err(|_| {
                            Diagnostic::text(
                                span.clone(),
                                text::constant_overflow_integer_literal(),
                            )
                            .with_context("value", &signed.to_string())
                        })?
                    } else {
                        i32::try_from(lit.value.value.value).map_err(|_| {
                            Diagnostic::text(
                                span.clone(),
                                text::constant_overflow_integer_literal(),
                            )
                            .with_context("value", &lit.value.value.value.to_string())
                        })?
                    };
                    let pool_index = ctx.add_i32_constant(value);
                    emitter.emit_load_const_i32(pool_index);
                }
                (OpWidth::W32, Signedness::Unsigned) => {
                    // Unsigned 32-bit: values up to u32::MAX are valid.
                    // Store the bit-pattern as i32.
                    let value = if lit.value.is_neg {
                        return Err(Diagnostic::text(
                            span.clone(),
                            text::constant_overflow_integer_literal(),
                        )
                        .with_context("value", &format!("-{}", lit.value.value.value)));
                    } else {
                        u32::try_from(lit.value.value.value).map_err(|_| {
                            Diagnostic::text(
                                span.clone(),
                                text::constant_overflow_integer_literal(),
                            )
                            .with_context("value", &lit.value.value.value.to_string())
                        })? as i32
                    };
                    let pool_index = ctx.add_i32_constant(value);
                    emitter.emit_load_const_i32(pool_index);
                }
                (OpWidth::W64, Signedness::Signed) => {
                    let value = if lit.value.is_neg {
                        let unsigned = lit.value.value.value as i128;
                        let signed = -unsigned;
                        i64::try_from(signed).map_err(|_| {
                            Diagnostic::text(
                                span.clone(),
                                text::constant_overflow_integer_literal(),
                            )
                            .with_context("value", &signed.to_string())
                        })?
                    } else {
                        i64::try_from(lit.value.value.value).map_err(|_| {
                            Diagnostic::text(
                                span.clone(),
                                text::constant_overflow_integer_literal(),
                            )
                            .with_context("value", &lit.value.value.value.to_string())
                        })?
                    };
                    let pool_index = ctx.add_i64_constant(value);
                    emitter.emit_load_const_i64(pool_index);
                }
                (OpWidth::W64, Signedness::Unsigned) => {
                    // Unsigned 64-bit: values up to u64::MAX are valid.
                    // Store the bit-pattern as i64.
                    let value = if lit.value.is_neg {
                        return Err(Diagnostic::text(
                            span.clone(),
                            text::constant_overflow_integer_literal(),
                        )
                        .with_context("value", &format!("-{}", lit.value.value.value)));
                    } else {
                        lit.value.value.value as i64
                    };
                    let pool_index = ctx.add_i64_constant(value);
                    emitter.emit_load_const_i64(pool_index);
                }
                (OpWidth::F32, _) => {
                    // Integer literal in float context: convert to f32.
                    let int_val = if lit.value.is_neg {
                        -(lit.value.value.value as f32)
                    } else {
                        lit.value.value.value as f32
                    };
                    let pool_index = ctx.add_f32_constant(int_val);
                    emitter.emit_load_const_f32(pool_index);
                }
                (OpWidth::F64, _) => {
                    // Integer literal in float context: convert to f64.
                    let int_val = if lit.value.is_neg {
                        -(lit.value.value.value as f64)
                    } else {
                        lit.value.value.value as f64
                    };
                    let pool_index = ctx.add_f64_constant(int_val);
                    emitter.emit_load_const_f64(pool_index);
                }
            }
            Ok(())
        }
        ConstantKind::RealLiteral(lit) => match op_type.0 {
            OpWidth::F32 => {
                let value = lit.value as f32;
                let pool_index = ctx.add_f32_constant(value);
                emitter.emit_load_const_f32(pool_index);
                Ok(())
            }
            OpWidth::F64 => {
                let pool_index = ctx.add_f64_constant(lit.value);
                emitter.emit_load_const_f64(pool_index);
                Ok(())
            }
            _ => Err(Diagnostic::not_implemented(Label::span(
                lit.span.clone(),
                "Real literal in a non-floating-point context",
            ))),
        },
        ConstantKind::Boolean(lit) => {
            match lit.value {
                Boolean::True => emitter.emit_load_true(),
                Boolean::False => emitter.emit_load_false(),
            }
            Ok(())
        }
        ConstantKind::CharacterString(lit) => {
            // Load the string literal into a temp buffer, leaving buf_idx on the stack.
            // The caller (e.g., string assignment path) will consume the buf_idx via
            // emit_str_store_var to copy the value into the target data region.
            let bytes = encode_string_literal(&lit.value, NARROW_CHAR_WIDTH);
            let pool_index = ctx.add_str_constant(bytes);
            emitter.emit_load_const_str(pool_index);
            Ok(())
        }
        ConstantKind::Duration(lit) => compile_time_count(
            emitter,
            ctx,
            lit.interval.whole_milliseconds(),
            "Duration literal",
            &lit.span,
            op_type,
        ),
        // A time of day cannot leave its range: `whole_milliseconds` is
        // bounded by 86,399,999 by construction, which every width holds. The
        // check below is therefore vacuous, and the code names the problem it
        // would be if the bound ever stopped holding.
        ConstantKind::TimeOfDay(lit) => compile_time_count(
            emitter,
            ctx,
            i128::from(lit.whole_milliseconds()),
            "Time-of-day literal",
            &lit.span,
            op_type,
        ),
        ConstantKind::Date(lit) => compile_time_count(
            emitter,
            ctx,
            i128::from(lit.seconds_since_epoch()),
            "Date literal",
            &lit.span,
            op_type,
        ),
        ConstantKind::DateAndTime(lit) => compile_time_count(
            emitter,
            ctx,
            i128::from(lit.seconds_since_epoch()),
            "Date literal",
            &lit.span,
            op_type,
        ),
        ConstantKind::BitStringLiteral(lit) => {
            let span = lit.value.span();
            match op_type {
                (OpWidth::W32, _) => {
                    let value = u32::try_from(lit.value.value).map_err(|_| {
                        Diagnostic::text(span.clone(), text::constant_overflow_bit_string_literal())
                            .with_context("value", &lit.value.value.to_string())
                    })? as i32;
                    let pool_index = ctx.add_i32_constant(value);
                    emitter.emit_load_const_i32(pool_index);
                }
                (OpWidth::W64, _) => {
                    let value = u64::try_from(lit.value.value).map_err(|_| {
                        Diagnostic::text(span.clone(), text::constant_overflow_bit_string_literal())
                            .with_context("value", &lit.value.value.to_string())
                    })? as i64;
                    let pool_index = ctx.add_i64_constant(value);
                    emitter.emit_load_const_i64(pool_index);
                }
                (OpWidth::F32, _) => {
                    let value = lit.value.value as f32;
                    let pool_index = ctx.add_f32_constant(value);
                    emitter.emit_load_const_f32(pool_index);
                }
                (OpWidth::F64, _) => {
                    let value = lit.value.value as f64;
                    let pool_index = ctx.add_f64_constant(value);
                    emitter.emit_load_const_f64(pool_index);
                }
            }
            Ok(())
        }
    }
}

/// Converts a `SignedInteger` AST node to an `i64` value.
pub(crate) fn signed_integer_to_i64(si: &SignedInteger) -> Result<i64, Diagnostic> {
    if si.is_neg {
        let unsigned = si.value.value as i128;
        let signed = -unsigned;
        i64::try_from(signed).map_err(|_| {
            Diagnostic::text(si.value.span(), text::constant_overflow_integer_literal())
                .with_context("value", &signed.to_string())
        })
    } else {
        i64::try_from(si.value.value).map_err(|_| {
            Diagnostic::text(si.value.span(), text::constant_overflow_integer_literal())
                .with_context("value", &si.value.value.to_string())
        })
    }
}
