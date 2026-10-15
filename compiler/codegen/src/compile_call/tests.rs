//! Tests for builtin opcode lookup.

use ironplc_container::opcode;

use super::builtin_lookup::lookup_builtin;
use crate::compile::{OpWidth, Signedness};

#[test]
fn lookup_builtin_when_all_width_numeric_then_selects_by_width() {
    // EXPT/ABS/SEL are defined for every op width, independent of sign.
    // An unsigned ABS never reaches code generation: the analyzer
    // removes it (`xform_remove_unsigned_abs`).
    assert_eq!(
        lookup_builtin("ABS", OpWidth::W32, Signedness::Signed),
        Some(opcode::builtin::ABS_I32)
    );
    assert_eq!(
        lookup_builtin("ABS", OpWidth::W64, Signedness::Signed),
        Some(opcode::builtin::ABS_I64)
    );
    assert_eq!(
        lookup_builtin("EXPT", OpWidth::F32, Signedness::Signed),
        Some(opcode::builtin::EXPT_F32)
    );
    assert_eq!(
        lookup_builtin("SEL", OpWidth::F64, Signedness::Signed),
        Some(opcode::builtin::SEL_F64)
    );
}

#[test]
fn lookup_builtin_when_signed_numeric_then_selects_by_width_and_sign() {
    assert_eq!(
        lookup_builtin("MIN", OpWidth::W32, Signedness::Signed),
        Some(opcode::builtin::MIN_I32)
    );
    assert_eq!(
        lookup_builtin("MIN", OpWidth::W32, Signedness::Unsigned),
        Some(opcode::builtin::MIN_U32)
    );
    assert_eq!(
        lookup_builtin("MAX", OpWidth::W64, Signedness::Unsigned),
        Some(opcode::builtin::MAX_U64)
    );
    // Float variants ignore signedness.
    assert_eq!(
        lookup_builtin("LIMIT", OpWidth::F32, Signedness::Unsigned),
        Some(opcode::builtin::LIMIT_F32)
    );
    assert_eq!(
        lookup_builtin("LIMIT", OpWidth::F64, Signedness::Signed),
        Some(opcode::builtin::LIMIT_F64)
    );
}

#[test]
fn lookup_builtin_when_float_only_and_float_width_then_selects_variant() {
    assert_eq!(
        lookup_builtin("SIN", OpWidth::F32, Signedness::Signed),
        Some(opcode::builtin::SIN_F32)
    );
    assert_eq!(
        lookup_builtin("ATAN2", OpWidth::F64, Signedness::Signed),
        Some(opcode::builtin::ATAN2_F64)
    );
}

#[test]
fn lookup_builtin_when_float_only_and_integer_width_then_none() {
    assert_eq!(
        lookup_builtin("SIN", OpWidth::W32, Signedness::Signed),
        None
    );
    assert_eq!(
        lookup_builtin("SQRT", OpWidth::W64, Signedness::Unsigned),
        None
    );
}

#[test]
fn lookup_builtin_when_case_insensitive_then_matches() {
    assert_eq!(
        lookup_builtin("sin", OpWidth::F32, Signedness::Signed),
        Some(opcode::builtin::SIN_F32)
    );
}

#[test]
fn lookup_builtin_when_unknown_name_then_none() {
    assert_eq!(
        lookup_builtin("NOT_A_BUILTIN", OpWidth::F32, Signedness::Signed),
        None
    );
}
