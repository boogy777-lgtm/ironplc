//! The type-changing migration policy (ADR 0060): which value conversions
//! the migration planner may perform when a shared-UID variable's storage
//! class changed between the active and candidate containers, and how each
//! admitted conversion transforms the 8-byte slot.
//!
//! The policy is a fixed table of `(base, candidate)` storage-class pairs —
//! widening and same-family conversions only. A pair outside the table is
//! reported at stage time ([`MigrationError::TypeChangeUnsupported`](crate::MigrationError))
//! unless the caller resolved it with a `MigrationDecision` (ADR 0061);
//! an admitted conversion executes at the scan boundary in the migration
//! planner (ADR 0054), never at stage time.
//!
//! The variable table encodes IEC types at storage-class granularity, so the
//! table speaks in storage classes: `SINT`/`INT`/`DINT` all share `I32`,
//! which makes the within-class widenings (`INT` -> `DINT`, `UINT` ->
//! `UDINT`) invisible to the planner — their entries are identical and the
//! ordinary slot copy already carries the value. Only a class change can
//! require a conversion.

use ironplc_container::FieldType;
use ironplc_vm::Slot;

/// One value conversion the policy admits, with the slot transformation it
/// performs. Each variant names the source and target storage classes, so the
/// variant set and the policy table stay in one-to-one correspondence.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum ValueConversion {
    /// I32 -> I64: a signed integer widens into the 64-bit class
    /// (`SINT`/`INT`/`DINT` -> `LINT`).
    SignedWiden32To64,
    /// U32 -> U64: an unsigned integer widens into the 64-bit class
    /// (`USINT`/`UINT`/`UDINT` -> `ULINT`).
    UnsignedWiden32To64,
    /// I32 -> F32: a signed integer converts to `REAL` (`INT`/`DINT` ->
    /// `REAL`).
    SignedToReal,
    /// I32 -> F64: a signed integer converts to `LREAL` (`INT`/`DINT` ->
    /// `LREAL`).
    SignedToLReal,
    /// I64 -> F64: a 64-bit signed integer converts to `LREAL` (`LINT` ->
    /// `LREAL`).
    LongToLReal,
    /// F32 -> F64: `REAL` widens to `LREAL`.
    RealToLReal,
}

impl ValueConversion {
    /// Converts the slot's value. The slot carries the source value exactly
    /// as the base program's VM stored it: I32 sign-extended, U32
    /// zero-extended, F32 as its low-32-bit pattern. Each arm reads through
    /// the accessor that reproduces that value regardless of the remaining
    /// bits, then stores the target the same way the candidate's VM will
    /// store it.
    pub(crate) fn apply(self, slot: Slot) -> Slot {
        match self {
            Self::SignedWiden32To64 => Slot::from_i64(i64::from(slot.as_i32())),
            Self::UnsignedWiden32To64 => Slot::from_u64(u64::from(slot.as_u64() as u32)),
            Self::SignedToReal => Slot::from_f32(slot.as_i32() as f32),
            Self::SignedToLReal => Slot::from_f64(f64::from(slot.as_i32())),
            Self::LongToLReal => Slot::from_f64(slot.as_i64() as f64),
            Self::RealToLReal => Slot::from_f64(f64::from(slot.as_f32())),
        }
    }
}

/// The admitted `(base, candidate)` storage-class pairs and the conversion
/// each performs. Widening and same-family only: integer widening within the
/// signed and unsigned families, integer-to-real for signed integers, and
/// real widening. Narrowing, signedness changes, `TIME`, strings, and FB
/// instances are deliberately absent: their conversions can lose or redefine
/// a value, so the planner rejects them with the pair named instead.
const POLICY: &[(FieldType, FieldType, ValueConversion)] = &[
    (
        FieldType::I32,
        FieldType::I64,
        ValueConversion::SignedWiden32To64,
    ),
    (
        FieldType::U32,
        FieldType::U64,
        ValueConversion::UnsignedWiden32To64,
    ),
    (
        FieldType::I32,
        FieldType::F32,
        ValueConversion::SignedToReal,
    ),
    (
        FieldType::I32,
        FieldType::F64,
        ValueConversion::SignedToLReal,
    ),
    (FieldType::I64, FieldType::F64, ValueConversion::LongToLReal),
    (FieldType::F32, FieldType::F64, ValueConversion::RealToLReal),
];

/// Returns the conversion the policy admits for a `(base, candidate)` type
/// pair, or `None` when the pair is outside the policy.
pub(crate) fn policy(base: FieldType, candidate: FieldType) -> Option<ValueConversion> {
    POLICY
        .iter()
        .find(|(from, to, _)| *from == base && *to == candidate)
        .map(|(_, _, conversion)| *conversion)
}

/// The name the migration diagnostic uses for a storage class. The variable
/// table cannot distinguish `SINT` from `DINT` (both are [`FieldType::I32`]),
/// so the name is the class name of the container format spec rather than an
/// IEC type name.
pub(crate) fn type_name(field_type: FieldType) -> &'static str {
    match field_type {
        FieldType::I32 => "I32",
        FieldType::U32 => "U32",
        FieldType::I64 => "I64",
        FieldType::U64 => "U64",
        FieldType::F32 => "F32",
        FieldType::F64 => "F64",
        FieldType::String => "STRING",
        FieldType::WString => "WSTRING",
        FieldType::FbInstance => "FB_INSTANCE",
        FieldType::Time => "TIME",
        FieldType::Slot => "SLOT",
    }
}

/// The six numeric storage classes the conversion policy is defined over, in
/// the container format's tag order. Test-only: production code never
/// iterates the classes, and the policy table names each pair it admits.
#[cfg(test)]
pub(crate) const NUMERIC_CLASSES: [FieldType; 6] = [
    FieldType::I32,
    FieldType::U32,
    FieldType::I64,
    FieldType::U64,
    FieldType::F32,
    FieldType::F64,
];

#[cfg(test)]
mod tests {
    use super::*;
    use rstest::rstest;
    use ValueConversion::{
        LongToLReal, RealToLReal, SignedToLReal, SignedToReal, SignedWiden32To64,
        UnsignedWiden32To64,
    };

    #[rstest]
    #[case::signed_widen(FieldType::I32, FieldType::I64, ValueConversion::SignedWiden32To64)]
    #[case::unsigned_widen(FieldType::U32, FieldType::U64, ValueConversion::UnsignedWiden32To64)]
    #[case::int_to_real(FieldType::I32, FieldType::F32, ValueConversion::SignedToReal)]
    #[case::int_to_lreal(FieldType::I32, FieldType::F64, ValueConversion::SignedToLReal)]
    #[case::long_to_lreal(FieldType::I64, FieldType::F64, ValueConversion::LongToLReal)]
    #[case::real_to_lreal(FieldType::F32, FieldType::F64, ValueConversion::RealToLReal)]
    fn policy_when_pair_admitted_then_returns_conversion(
        #[case] base: FieldType,
        #[case] candidate: FieldType,
        #[case] expected: ValueConversion,
    ) {
        assert_eq!(policy(base, candidate), Some(expected));
    }

    #[rstest]
    #[case::same_type(FieldType::I32, FieldType::I32)]
    #[case::narrowing(FieldType::I64, FieldType::I32)]
    #[case::real_to_int(FieldType::F64, FieldType::I32)]
    #[case::real_narrowing(FieldType::F64, FieldType::F32)]
    #[case::signedness_change(FieldType::I32, FieldType::U32)]
    #[case::unsigned_to_real(FieldType::U32, FieldType::F32)]
    #[case::unsigned_to_lreal(FieldType::U32, FieldType::F64)]
    #[case::ulint_to_lreal(FieldType::U64, FieldType::F64)]
    #[case::time(FieldType::Time, FieldType::I64)]
    #[case::int_to_time(FieldType::I32, FieldType::Time)]
    #[case::string(FieldType::String, FieldType::WString)]
    #[case::int_to_string(FieldType::I32, FieldType::String)]
    #[case::fb_instance(FieldType::FbInstance, FieldType::I32)]
    fn policy_when_pair_outside_policy_then_none(
        #[case] base: FieldType,
        #[case] candidate: FieldType,
    ) {
        assert_eq!(policy(base, candidate), None);
    }

    /// The independent specification of the policy as an explicit 6x6
    /// matrix over [`NUMERIC_CLASSES`]: rows are the base class, columns the
    /// candidate class. `None` everywhere except the six admitted
    /// conversions; the same-class diagonal is `None` too, because those
    /// cells take the ordinary copy path and the policy is not consulted.
    /// Deliberately duplicates `POLICY`, so any policy change fails the
    /// truth-table test below.
    const EXPECTED: [[Option<ValueConversion>; 6]; 6] = [
        [
            None,
            None,
            Some(SignedWiden32To64),
            None,
            Some(SignedToReal),
            Some(SignedToLReal),
        ],
        [None, None, None, Some(UnsignedWiden32To64), None, None],
        [None, None, None, None, None, Some(LongToLReal)],
        [None, None, None, None, None, None],
        [None, None, None, None, None, Some(RealToLReal)],
        [None, None, None, None, None, None],
    ];

    #[test]
    fn policy_when_numeric_class_pair_then_matches_the_full_truth_table() {
        let mut actual = [[None; 6]; 6];
        for (row, base) in NUMERIC_CLASSES.iter().enumerate() {
            for (column, candidate) in NUMERIC_CLASSES.iter().enumerate() {
                actual[row][column] = policy(*base, *candidate);
            }
        }

        assert_eq!(actual, EXPECTED);
    }

    #[rstest]
    #[case::signed_widen(
        ValueConversion::SignedWiden32To64,
        Slot::from_i32(-7),
        Slot::from_i64(-7)
    )]
    #[case::unsigned_widen(
        ValueConversion::UnsignedWiden32To64,
        Slot::from_u64(4_000_000_000),
        Slot::from_u64(4_000_000_000)
    )]
    #[case::int_to_real(ValueConversion::SignedToReal, Slot::from_i32(3), Slot::from_f32(3.0))]
    #[case::int_to_lreal(
        ValueConversion::SignedToLReal,
        Slot::from_i32(-9),
        Slot::from_f64(-9.0)
    )]
    #[case::long_to_lreal(
        ValueConversion::LongToLReal,
        Slot::from_i64(1_000_000_000_000),
        Slot::from_f64(1_000_000_000_000.0)
    )]
    #[case::real_to_lreal(ValueConversion::RealToLReal, Slot::from_f32(0.5), Slot::from_f64(0.5))]
    fn apply_when_conversion_then_slot_carries_converted_value(
        #[case] conversion: ValueConversion,
        #[case] from: Slot,
        #[case] expected: Slot,
    ) {
        assert_eq!(conversion.apply(from), expected);
    }

    #[test]
    fn apply_when_signed_widen_with_high_bits_set_then_sign_extended() {
        // The conversion reads through as_i32, so even a slot whose high
        // bits do not match the base VM's sign extension widens correctly.
        let slot = Slot::from_u64(0xDEAD_BEEF_0000_002A);

        assert_eq!(
            ValueConversion::SignedWiden32To64.apply(slot),
            Slot::from_i64(42)
        );
    }

    #[rstest]
    #[case::i32(FieldType::I32, "I32")]
    #[case::u64(FieldType::U64, "U64")]
    #[case::f32(FieldType::F32, "F32")]
    #[case::string(FieldType::String, "STRING")]
    #[case::fb_instance(FieldType::FbInstance, "FB_INSTANCE")]
    #[case::time(FieldType::Time, "TIME")]
    fn type_name_when_storage_class_then_spec_name(
        #[case] field_type: FieldType,
        #[case] expected: &'static str,
    ) {
        assert_eq!(type_name(field_type), expected);
    }
}
