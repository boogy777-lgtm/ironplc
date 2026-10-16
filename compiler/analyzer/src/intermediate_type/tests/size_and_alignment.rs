//! Tests for the byte size, alignment and explicit-size answers of primitive and aggregate types.

use super::*;

#[test]
fn intermediate_type_size_in_bytes_returns_bytes() {
    // Test sized types
    assert_eq!(IntermediateType::Bool.size_in_bytes(), Some(1));
    assert_eq!(
        IntermediateType::Int {
            size: ByteSized::B16
        }
        .size_in_bytes(),
        Some(2)
    );
    assert_eq!(
        IntermediateType::UInt {
            size: ByteSized::B32
        }
        .size_in_bytes(),
        Some(4)
    );
    assert_eq!(
        IntermediateType::Real {
            size: ByteSized::B64
        }
        .size_in_bytes(),
        Some(8)
    );
    assert_eq!(
        IntermediateType::Bytes {
            size: ByteSized::B8
        }
        .size_in_bytes(),
        Some(1)
    );

    // Test time and date types
    assert_eq!(
        IntermediateType::Time {
            size: ByteSized::B32
        }
        .size_in_bytes(),
        Some(4)
    );
    assert_eq!(
        IntermediateType::Time {
            size: ByteSized::B64
        }
        .size_in_bytes(),
        Some(8)
    );
    assert_eq!(
        IntermediateType::Date {
            size: ByteSized::B32
        }
        .size_in_bytes(),
        Some(4)
    );
    assert_eq!(
        IntermediateType::TimeOfDay {
            size: ByteSized::B32
        }
        .size_in_bytes(),
        Some(4)
    );
    assert_eq!(
        IntermediateType::DateAndTime {
            size: ByteSized::B64
        }
        .size_in_bytes(),
        Some(8)
    );

    // Test string types
    assert_eq!(
        IntermediateType::String {
            max_len: Some(10),
            char_width: CharWidth::Narrow,
        }
        .size_in_bytes(),
        Some(10)
    );
    assert_eq!(
        IntermediateType::String {
            max_len: None,
            char_width: CharWidth::Narrow,
        }
        .size_in_bytes(),
        None
    );
    // WSTRING payload is two bytes per code unit.
    assert_eq!(
        IntermediateType::String {
            max_len: Some(10),
            char_width: CharWidth::Wide,
        }
        .size_in_bytes(),
        Some(20)
    );

    // Test subrange inherits base type size
    let subrange = IntermediateType::Subrange {
        base_type: Box::new(IntermediateType::Int {
            size: ByteSized::B16,
        }),
        min_value: 1,
        max_value: 100,
    };
    assert_eq!(subrange.size_in_bytes(), Some(2));

    // Test enumeration inherits underlying type size
    let enumeration = IntermediateType::Enumeration {
        underlying_type: Box::new(IntermediateType::Int {
            size: ByteSized::B8,
        }),
        base_type: Box::new(IntermediateType::Int {
            size: crate::intermediate_type::ByteSized::B16,
        }),
        numbers: vec![],
        strict: true,
    };
    assert_eq!(enumeration.size_in_bytes(), Some(1));

    // Test fixed-size array
    let array = IntermediateType::Array {
        element_type: Box::new(IntermediateType::Int {
            size: ByteSized::B32,
        }),
        dimensions: vec![ArrayDimension { lower: 1, upper: 5 }],
    };
    assert_eq!(array.size_in_bytes(), Some(20)); // 4 bytes * 5 elements

    // Test dynamic array (empty dimensions)
    let dynamic_array = IntermediateType::Array {
        element_type: Box::new(IntermediateType::Int {
            size: ByteSized::B32,
        }),
        dimensions: vec![],
    };
    assert_eq!(dynamic_array.size_in_bytes(), None);

    // Test reference types (always 8 bytes)
    let ref_type = IntermediateType::Reference {
        target_type: Box::new(IntermediateType::Int {
            size: ByteSized::B16,
        }),
    };
    assert_eq!(ref_type.size_in_bytes(), Some(8));
}

#[test]
fn intermediate_type_alignment_bytes_returns_alignment() {
    // Test primitive types
    assert_eq!(IntermediateType::Bool.alignment_bytes(), 1);
    assert_eq!(
        IntermediateType::Int {
            size: ByteSized::B16
        }
        .alignment_bytes(),
        2
    );
    assert_eq!(
        IntermediateType::UInt {
            size: ByteSized::B32
        }
        .alignment_bytes(),
        4
    );
    assert_eq!(
        IntermediateType::Real {
            size: ByteSized::B64
        }
        .alignment_bytes(),
        8
    );

    // Test time and date types
    assert_eq!(
        IntermediateType::Time {
            size: ByteSized::B32
        }
        .alignment_bytes(),
        4
    );
    assert_eq!(
        IntermediateType::Time {
            size: ByteSized::B64
        }
        .alignment_bytes(),
        8
    );
    assert_eq!(
        IntermediateType::Date {
            size: ByteSized::B32
        }
        .alignment_bytes(),
        4
    );
    assert_eq!(
        IntermediateType::DateAndTime {
            size: ByteSized::B64
        }
        .alignment_bytes(),
        8
    );

    // Test string types (byte-aligned)
    assert_eq!(
        IntermediateType::String {
            max_len: Some(10),
            char_width: CharWidth::Narrow,
        }
        .alignment_bytes(),
        1
    );

    // Test derived types inherit alignment
    let subrange = IntermediateType::Subrange {
        base_type: Box::new(IntermediateType::Int {
            size: ByteSized::B32,
        }),
        min_value: 1,
        max_value: 100,
    };
    assert_eq!(subrange.alignment_bytes(), 4);

    // Test array inherits element alignment
    let array = IntermediateType::Array {
        element_type: Box::new(IntermediateType::Real {
            size: ByteSized::B64,
        }),
        dimensions: vec![ArrayDimension { lower: 1, upper: 3 }],
    };
    assert_eq!(array.alignment_bytes(), 8);

    // Test reference type alignment (always 8 bytes)
    let ref_type = IntermediateType::Reference {
        target_type: Box::new(IntermediateType::Bool),
    };
    assert_eq!(ref_type.alignment_bytes(), 8);
}

#[test]
fn intermediate_type_has_explicit_size_returns_correct_value() {
    // Test types with explicit size
    assert!(IntermediateType::Bool.has_explicit_size());
    assert!(IntermediateType::Int {
        size: ByteSized::B16
    }
    .has_explicit_size());
    assert!(IntermediateType::Time {
        size: ByteSized::B32
    }
    .has_explicit_size());
    assert!(IntermediateType::Date {
        size: ByteSized::B32
    }
    .has_explicit_size());

    // Test string types
    assert!(IntermediateType::String {
        max_len: Some(10),
        char_width: CharWidth::Narrow,
    }
    .has_explicit_size());
    assert!(!IntermediateType::String {
        max_len: None,
        char_width: CharWidth::Narrow,
    }
    .has_explicit_size());

    // Test derived types
    let subrange = IntermediateType::Subrange {
        base_type: Box::new(IntermediateType::Int {
            size: ByteSized::B16,
        }),
        min_value: 1,
        max_value: 100,
    };
    assert!(subrange.has_explicit_size());

    // Test arrays
    let fixed_array = IntermediateType::Array {
        element_type: Box::new(IntermediateType::Bool),
        dimensions: vec![ArrayDimension { lower: 0, upper: 9 }],
    };
    assert!(fixed_array.has_explicit_size());

    let dynamic_array = IntermediateType::Array {
        element_type: Box::new(IntermediateType::Bool),
        dimensions: vec![],
    };
    assert!(!dynamic_array.has_explicit_size());
}

#[test]
fn array_size_in_bytes_when_elements_are_structures_then_each_element_occupies_its_stride() {
    // LWORD at 0 and BYTE at 8: the byte image of one structure is 9 bytes, its
    // stride is 16, so three elements are 48 bytes.
    let element = IntermediateType::Structure {
        fields: vec![
            IntermediateStructField {
                name: Id::from("a"),
                field_type: IntermediateType::Bytes {
                    size: ByteSized::B64,
                },
                offset: 0,
                var_type: None,
                initial_value: None,
            },
            IntermediateStructField {
                name: Id::from("b"),
                field_type: IntermediateType::Bytes {
                    size: ByteSized::B8,
                },
                offset: 8,
                var_type: None,
                initial_value: None,
            },
        ],
        bound: AlignmentBound::Natural,
    };
    let array = IntermediateType::Array {
        element_type: Box::new(element.clone()),
        dimensions: vec![ArrayDimension { lower: 1, upper: 3 }],
    };

    assert_eq!(element.size_in_bytes(), Some(9));
    assert_eq!(array.size_in_bytes(), Some(48));
}
