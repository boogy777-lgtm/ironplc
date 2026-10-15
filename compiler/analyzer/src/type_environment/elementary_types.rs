//! The table of elementary types and the lookup of an elementary type by name.

use ironplc_container::CharWidth;
use ironplc_dsl::common::{ElementaryTypeName, TypeName};

use crate::intermediate_type::{ByteSized, IntermediateType};

pub(super) static ELEMENTARY_TYPES_LOWER_CASE: [(&str, ElementaryTypeName, IntermediateType); 30] = [
    // signed_integer_type_name
    (
        "sint",
        ElementaryTypeName::SINT,
        IntermediateType::Int {
            size: ByteSized::B8,
        },
    ),
    (
        "int",
        ElementaryTypeName::INT,
        IntermediateType::Int {
            size: ByteSized::B16,
        },
    ),
    (
        "dint",
        ElementaryTypeName::DINT,
        IntermediateType::Int {
            size: ByteSized::B32,
        },
    ),
    (
        "lint",
        ElementaryTypeName::LINT,
        IntermediateType::Int {
            size: ByteSized::B64,
        },
    ),
    // unsigned_integer_type_name
    (
        "usint",
        ElementaryTypeName::USINT,
        IntermediateType::UInt {
            size: ByteSized::B8,
        },
    ),
    (
        "uint",
        ElementaryTypeName::UINT,
        IntermediateType::UInt {
            size: ByteSized::B16,
        },
    ),
    (
        "udint",
        ElementaryTypeName::UDINT,
        IntermediateType::UInt {
            size: ByteSized::B32,
        },
    ),
    (
        "ulint",
        ElementaryTypeName::ULINT,
        IntermediateType::UInt {
            size: ByteSized::B64,
        },
    ),
    // real_type_name
    (
        "real",
        ElementaryTypeName::REAL,
        IntermediateType::Real {
            size: ByteSized::B32,
        },
    ),
    (
        "lreal",
        ElementaryTypeName::LREAL,
        IntermediateType::Real {
            size: ByteSized::B64,
        },
    ),
    // time_type_name — TIME must appear before TIME_OF_DAY so that
    // resolve_elementary_type_name() finds the canonical name first.
    (
        "time",
        ElementaryTypeName::TIME,
        IntermediateType::Time {
            size: ByteSized::B32,
        },
    ),
    // LTIME is 64-bit (milliseconds) — IEC 61131-3 Edition 3 (2013)
    (
        "ltime",
        ElementaryTypeName::LTIME,
        IntermediateType::Time {
            size: ByteSized::B64,
        },
    ),
    // date_type_name — DATE must appear before DATE_AND_TIME for the
    // same reason.
    (
        "date",
        ElementaryTypeName::DATE,
        IntermediateType::Date {
            size: ByteSized::B32,
        },
    ),
    (
        "time_of_day",
        ElementaryTypeName::TimeOfDay,
        IntermediateType::TimeOfDay {
            size: ByteSized::B32,
        },
    ),
    (
        "tod",
        ElementaryTypeName::TimeOfDay,
        IntermediateType::TimeOfDay {
            size: ByteSized::B32,
        },
    ),
    (
        "date_and_time",
        ElementaryTypeName::DateAndTime,
        IntermediateType::DateAndTime {
            size: ByteSized::B32,
        },
    ),
    (
        "dt",
        ElementaryTypeName::DateAndTime,
        IntermediateType::DateAndTime {
            size: ByteSized::B32,
        },
    ),
    // IEC 61131-3 Edition 3 (2013) long date/time types
    (
        "ldate",
        ElementaryTypeName::LDATE,
        IntermediateType::Date {
            size: ByteSized::B64,
        },
    ),
    (
        "ltime_of_day",
        ElementaryTypeName::LTimeOfDay,
        IntermediateType::TimeOfDay {
            size: ByteSized::B64,
        },
    ),
    (
        "ltod",
        ElementaryTypeName::LTimeOfDay,
        IntermediateType::TimeOfDay {
            size: ByteSized::B64,
        },
    ),
    (
        "ldate_and_time",
        ElementaryTypeName::LDateAndTime,
        IntermediateType::DateAndTime {
            size: ByteSized::B64,
        },
    ),
    (
        "ldt",
        ElementaryTypeName::LDateAndTime,
        IntermediateType::DateAndTime {
            size: ByteSized::B64,
        },
    ),
    // bit_string_type_name
    ("bool", ElementaryTypeName::BOOL, IntermediateType::Bool),
    // The one-bit type the vendor dialects add. IronPLC has no one-bit
    // storage, so BIT is an alias of BOOL: it gets BOOL's type id and
    // representation (see `type_id::elementary`). It follows BOOL in this
    // table so that BOOL stays the canonical name of the representation.
    ("bit", ElementaryTypeName::BIT, IntermediateType::Bool),
    (
        "byte",
        ElementaryTypeName::BYTE,
        IntermediateType::Bytes {
            size: ByteSized::B8,
        },
    ),
    (
        "word",
        ElementaryTypeName::WORD,
        IntermediateType::Bytes {
            size: ByteSized::B16,
        },
    ),
    (
        "dword",
        ElementaryTypeName::DWORD,
        IntermediateType::Bytes {
            size: ByteSized::B32,
        },
    ),
    (
        "lword",
        ElementaryTypeName::LWORD,
        IntermediateType::Bytes {
            size: ByteSized::B64,
        },
    ),
    // remaining elementary_type_name
    (
        "string",
        ElementaryTypeName::STRING,
        IntermediateType::String {
            max_len: None,
            char_width: CharWidth::Narrow,
        },
    ),
    (
        "wstring",
        ElementaryTypeName::WSTRING,
        IntermediateType::String {
            max_len: None,
            char_width: CharWidth::Wide,
        },
    ),
];

/// Returns what an elementary type is, by name.
///
/// The elementary type table above is the compiler's statement of what each
/// elementary type is -- `INT` is a signed 16-bit integer. This reads that
/// statement without building a [`TypeEnvironment`], so that code outside the
/// analyzer works from it rather than from a copy of it.
///
/// Returns `None` for any name that is not an elementary type.
pub fn elementary_type(type_name: &TypeName) -> Option<&'static IntermediateType> {
    ELEMENTARY_TYPES_LOWER_CASE
        .iter()
        .find(|(name, _, _)| type_name.name.lower_case == *name)
        .map(|(_, _, elem_type)| elem_type)
}
