//! Tests for the number of data-region slots a type occupies.

use super::*;

#[test]
fn slot_count_when_primitive_then_returns_1() {
    assert_eq!(IntermediateType::Bool.slot_count(), Ok(1));
    assert_eq!(
        IntermediateType::Int {
            size: ByteSized::B16
        }
        .slot_count(),
        Ok(1)
    );
    assert_eq!(
        IntermediateType::UInt {
            size: ByteSized::B32
        }
        .slot_count(),
        Ok(1)
    );
    assert_eq!(
        IntermediateType::Real {
            size: ByteSized::B64
        }
        .slot_count(),
        Ok(1)
    );
    assert_eq!(
        IntermediateType::Bytes {
            size: ByteSized::B8
        }
        .slot_count(),
        Ok(1)
    );
    assert_eq!(
        IntermediateType::Time {
            size: ByteSized::B32
        }
        .slot_count(),
        Ok(1)
    );
    assert_eq!(
        IntermediateType::Date {
            size: ByteSized::B32
        }
        .slot_count(),
        Ok(1)
    );
    assert_eq!(
        IntermediateType::TimeOfDay {
            size: ByteSized::B32
        }
        .slot_count(),
        Ok(1)
    );
    assert_eq!(
        IntermediateType::DateAndTime {
            size: ByteSized::B64
        }
        .slot_count(),
        Ok(1)
    );
    assert_eq!(
        IntermediateType::Enumeration {
            underlying_type: Box::new(IntermediateType::Int {
                size: ByteSized::B8
            }),
            base_type: Box::new(IntermediateType::Int {
                size: crate::intermediate_type::ByteSized::B16,
            }),
            numbers: vec![],
        }
        .slot_count(),
        Ok(1)
    );
    assert_eq!(
        IntermediateType::Subrange {
            base_type: Box::new(IntermediateType::Int {
                size: ByteSized::B16
            }),
            min_value: 1,
            max_value: 100,
        }
        .slot_count(),
        Ok(1)
    );
}

#[test]
fn slot_count_when_structure_with_two_fields_then_returns_2() {
    let s = IntermediateType::Structure {
        fields: vec![
            IntermediateStructField {
                name: Id::from("a"),
                field_type: IntermediateType::Int {
                    size: ByteSized::B16,
                },
                offset: 0,
                var_type: None,
                initial_value: None,
            },
            IntermediateStructField {
                name: Id::from("b"),
                field_type: IntermediateType::Bool,
                offset: 2,
                var_type: None,
                initial_value: None,
            },
        ],
    };
    assert_eq!(s.slot_count(), Ok(2));
}

#[test]
fn slot_count_when_nested_structure_then_returns_sum() {
    let inner = IntermediateType::Structure {
        fields: vec![
            IntermediateStructField {
                name: Id::from("x"),
                field_type: IntermediateType::Int {
                    size: ByteSized::B32,
                },
                offset: 0,
                var_type: None,
                initial_value: None,
            },
            IntermediateStructField {
                name: Id::from("y"),
                field_type: IntermediateType::Int {
                    size: ByteSized::B32,
                },
                offset: 4,
                var_type: None,
                initial_value: None,
            },
        ],
    };
    let outer = IntermediateType::Structure {
        fields: vec![
            IntermediateStructField {
                name: Id::from("inner"),
                field_type: inner,
                offset: 0,
                var_type: None,
                initial_value: None,
            },
            IntermediateStructField {
                name: Id::from("z"),
                field_type: IntermediateType::Bool,
                offset: 8,
                var_type: None,
                initial_value: None,
            },
        ],
    };
    // inner occupies 2 slots + z occupies 1 slot = 3
    assert_eq!(outer.slot_count(), Ok(3));
}

#[test]
fn slot_count_when_array_of_primitives_then_returns_total_elements() {
    let arr = IntermediateType::Array {
        element_type: Box::new(IntermediateType::Int {
            size: ByteSized::B32,
        }),
        dimensions: vec![ArrayDimension {
            lower: 1,
            upper: 10,
        }],
    };
    assert_eq!(arr.slot_count(), Ok(10));
}

#[test]
fn slot_count_when_array_of_structures_then_returns_elements_times_struct_slots() {
    let s = IntermediateType::Structure {
        fields: vec![
            IntermediateStructField {
                name: Id::from("a"),
                field_type: IntermediateType::Int {
                    size: ByteSized::B32,
                },
                offset: 0,
                var_type: None,
                initial_value: None,
            },
            IntermediateStructField {
                name: Id::from("b"),
                field_type: IntermediateType::Int {
                    size: ByteSized::B32,
                },
                offset: 4,
                var_type: None,
                initial_value: None,
            },
        ],
    };
    let arr = IntermediateType::Array {
        element_type: Box::new(s),
        dimensions: vec![ArrayDimension { lower: 1, upper: 5 }],
    };
    // 5 elements * 2 slots each = 10
    assert_eq!(arr.slot_count(), Ok(10));
}

#[test]
fn slot_count_when_reference_field_then_returns_1() {
    let r = IntermediateType::Reference {
        target_type: Box::new(IntermediateType::Int {
            size: ByteSized::B32,
        }),
    };
    assert_eq!(r.slot_count(), Ok(1));
}

#[test]
fn slot_count_when_string_with_explicit_len_then_returns_ceil_slots() {
    // STRING[255]: ceil((6 + 255 * 1) / 8) = ceil(261/8) = 33 slots
    let s = IntermediateType::String {
        max_len: Some(255),
        char_width: CharWidth::Narrow,
    };
    assert_eq!(s.slot_count(), Ok(33));
}

#[test]
fn slot_count_when_string_with_default_len_then_returns_ceil_slots() {
    // STRING (default 254): ceil((6 + 254 * 1) / 8) = ceil(260/8) = 33 slots
    let s = IntermediateType::String {
        max_len: None,
        char_width: CharWidth::Narrow,
    };
    assert_eq!(s.slot_count(), Ok(33));
}

#[test]
fn slot_count_when_string_small_then_returns_2() {
    // STRING[4]: ceil((6 + 4 * 1) / 8) = ceil(10/8) = 2 slots.
    // The 6-byte header does not leave a 4-character string inside one slot.
    let s = IntermediateType::String {
        max_len: Some(4),
        char_width: CharWidth::Narrow,
    };
    assert_eq!(s.slot_count(), Ok(2));
}

#[test]
fn slot_count_when_wstring_with_explicit_len_then_scales_by_char_width() {
    // WSTRING[255]: ceil((6 + 255 * 2) / 8) = ceil(516/8) = 65 slots
    let s = IntermediateType::String {
        max_len: Some(255),
        char_width: CharWidth::Wide,
    };
    assert_eq!(s.slot_count(), Ok(65));
}

#[test]
fn slot_count_when_wstring_with_default_len_then_scales_by_char_width() {
    // WSTRING (default 254): ceil((6 + 254 * 2) / 8) = ceil(514/8) = 65 slots
    let s = IntermediateType::String {
        max_len: None,
        char_width: CharWidth::Wide,
    };
    assert_eq!(s.slot_count(), Ok(65));
}

#[test]
fn slot_count_when_wstring_then_returns_more_slots_than_same_length_string() {
    let narrow = IntermediateType::String {
        max_len: Some(64),
        char_width: CharWidth::Narrow,
    };
    let wide = IntermediateType::String {
        max_len: Some(64),
        char_width: CharWidth::Wide,
    };
    // ceil(70/8) = 9 vs ceil(134/8) = 17 — the wide payload is twice as big.
    assert_eq!(narrow.slot_count(), Ok(9));
    assert_eq!(wide.slot_count(), Ok(17));
}

#[test]
fn slot_count_when_string_matches_size_in_bytes_payload() {
    // The two sizing methods must agree about the payload: slot_count is
    // size_in_bytes plus the header, rounded up to whole slots.
    for char_width in [CharWidth::Narrow, CharWidth::Wide] {
        for max_len in [0u128, 1, 4, 80, 254, 255, 4096] {
            let s = IntermediateType::String {
                max_len: Some(max_len),
                char_width,
            };
            let payload = s.size_in_bytes().unwrap();
            let expected = (payload + STRING_HEADER_BYTES as u32).div_ceil(8);
            assert_eq!(
                s.slot_count(),
                Ok(expected),
                "{:?} [{}]",
                char_width,
                max_len
            );
        }
    }
}

#[test]
fn slot_count_when_string_len_exceeds_u16_then_returns_overflow() {
    // max_length is a u16 field in the data-region header, so a longer
    // declared length has no representable layout.
    let s = IntermediateType::String {
        max_len: Some(u16::MAX as u128 + 1),
        char_width: CharWidth::Narrow,
    };
    assert_eq!(s.slot_count(), Err(SlotCountError::Overflow));
}

#[test]
fn slot_count_when_nesting_exceeds_max_depth_then_returns_max_depth_exceeded() {
    // Build 34 levels of nesting (exceeds MAX_NESTING_DEPTH of 32)
    let mut t = IntermediateType::Bool;
    for i in 0..34 {
        t = IntermediateType::Structure {
            fields: vec![IntermediateStructField {
                name: Id::from(&format!("f{}", i)),
                field_type: t,
                offset: 0,
                var_type: None,
                initial_value: None,
            }],
        };
    }
    assert_eq!(t.slot_count(), Err(SlotCountError::MaxDepthExceeded));
}

#[test]
fn slot_count_when_total_overflows_u32_then_returns_overflow() {
    // Create a multi-dimensional array that overflows u32
    let arr = IntermediateType::Array {
        element_type: Box::new(IntermediateType::Int {
            size: ByteSized::B32,
        }),
        dimensions: vec![
            ArrayDimension {
                lower: 1,
                upper: 100_000,
            },
            ArrayDimension {
                lower: 1,
                upper: 100_000,
            },
        ],
    };
    assert_eq!(arr.slot_count(), Err(SlotCountError::Overflow));
}
