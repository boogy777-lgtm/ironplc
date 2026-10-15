//! Tests for the alignment and size of function block types.

use super::*;

#[test]
fn function_block_alignment_bytes_with_empty_function_block_then_returns_one() {
    let fb_type = IntermediateType::FunctionBlock {
        name: "EmptyFB".to_string(),
        fields: vec![],
    };
    assert_eq!(fb_type.alignment_bytes(), 1);
}

#[test]
fn function_block_alignment_bytes_with_single_field_then_returns_field_alignment() {
    use super::IntermediateStructField;
    use ironplc_dsl::core::Id;

    let fields = vec![IntermediateStructField {
        name: Id::from("field1"),
        field_type: IntermediateType::Int {
            size: ByteSized::B32,
        },
        offset: 0,
        var_type: None,
        initial_value: None,
    }];

    let fb_type = IntermediateType::FunctionBlock {
        name: "TestFB".to_string(),
        fields,
    };
    assert_eq!(fb_type.alignment_bytes(), 4);
}

#[test]
fn function_block_alignment_bytes_with_multiple_fields_then_returns_max_alignment() {
    use super::IntermediateStructField;
    use ironplc_dsl::core::Id;

    let fields = vec![
        IntermediateStructField {
            name: Id::from("field1"),
            field_type: IntermediateType::Int {
                size: ByteSized::B8,
            },
            offset: 0,
            var_type: None,
            initial_value: None,
        },
        IntermediateStructField {
            name: Id::from("field2"),
            field_type: IntermediateType::Real {
                size: ByteSized::B64,
            },
            offset: 8,
            var_type: None,
            initial_value: None,
        },
        IntermediateStructField {
            name: Id::from("field3"),
            field_type: IntermediateType::Int {
                size: ByteSized::B16,
            },
            offset: 16,
            var_type: None,
            initial_value: None,
        },
    ];

    let fb_type = IntermediateType::FunctionBlock {
        name: "TestFB".to_string(),
        fields,
    };
    // Maximum alignment should be 8 (from the Real field)
    assert_eq!(fb_type.alignment_bytes(), 8);
}

#[test]
fn function_block_size_in_bytes_with_empty_function_block_then_returns_none() {
    let fb_type = IntermediateType::FunctionBlock {
        name: "EmptyFB".to_string(),
        fields: vec![],
    };
    assert_eq!(fb_type.size_in_bytes(), None);
}

#[test]
fn function_block_size_in_bytes_with_single_field_then_returns_field_size() {
    use super::IntermediateStructField;
    use ironplc_dsl::core::Id;

    let fields = vec![IntermediateStructField {
        name: Id::from("counter"),
        field_type: IntermediateType::Int {
            size: ByteSized::B32,
        },
        offset: 0,
        var_type: None,
        initial_value: None,
    }];

    let fb_type = IntermediateType::FunctionBlock {
        name: "CounterFB".to_string(),
        fields,
    };
    // Size should be 4 bytes (one DINT field)
    assert_eq!(fb_type.size_in_bytes(), Some(4));
}

#[test]
fn function_block_size_in_bytes_with_aligned_fields_then_returns_total_size() {
    use super::IntermediateStructField;
    use ironplc_dsl::core::Id;

    // Function block with two 4-byte aligned fields
    let fields = vec![
        IntermediateStructField {
            name: Id::from("input"),
            field_type: IntermediateType::Int {
                size: ByteSized::B32,
            },
            offset: 0,
            var_type: None,
            initial_value: None,
        },
        IntermediateStructField {
            name: Id::from("output"),
            field_type: IntermediateType::Int {
                size: ByteSized::B32,
            },
            offset: 4,
            var_type: None,
            initial_value: None,
        },
    ];

    let fb_type = IntermediateType::FunctionBlock {
        name: "ProcessFB".to_string(),
        fields,
    };
    // Size should be 8 bytes (two DINT fields)
    assert_eq!(fb_type.size_in_bytes(), Some(8));
}

#[test]
fn function_block_size_in_bytes_with_padding_then_returns_padded_size() {
    use super::IntermediateStructField;
    use ironplc_dsl::core::Id;

    // Function block: SINT (1 byte at offset 0), DINT (4 bytes at offset 4)
    // Total size should be 8 bytes (padded to 4-byte alignment)
    let fields = vec![
        IntermediateStructField {
            name: Id::from("flag"),
            field_type: IntermediateType::Int {
                size: ByteSized::B8,
            },
            offset: 0,
            var_type: None,
            initial_value: None,
        },
        IntermediateStructField {
            name: Id::from("value"),
            field_type: IntermediateType::Int {
                size: ByteSized::B32,
            },
            offset: 4,
            var_type: None,
            initial_value: None,
        },
    ];

    let fb_type = IntermediateType::FunctionBlock {
        name: "TestFB".to_string(),
        fields,
    };
    // Size should be 8 bytes (1 byte + 3 padding + 4 bytes)
    assert_eq!(fb_type.size_in_bytes(), Some(8));
}

#[test]
fn function_block_size_in_bytes_with_trailing_padding_then_byte_image_ends_with_last_field_and_stride_is_aligned(
) {
    use super::IntermediateStructField;
    use ironplc_dsl::core::Id;

    // Function block: LREAL (8 bytes at offset 0), SINT (1 byte at offset 8)
    // The byte image is 9 bytes; where the function block repeats or nests it
    // occupies 16 bytes (padded to 8-byte alignment)
    let fields = vec![
        IntermediateStructField {
            name: Id::from("timestamp"),
            field_type: IntermediateType::Real {
                size: ByteSized::B64,
            },
            offset: 0,
            var_type: None,
            initial_value: None,
        },
        IntermediateStructField {
            name: Id::from("status"),
            field_type: IntermediateType::Int {
                size: ByteSized::B8,
            },
            offset: 8,
            var_type: None,
            initial_value: None,
        },
    ];

    let fb_type = IntermediateType::FunctionBlock {
        name: "TimerFB".to_string(),
        fields,
    };
    assert_eq!(fb_type.size_in_bytes(), Some(9));
    assert_eq!(fb_type.stride_bytes(), Some(16));
}

#[test]
fn function_block_size_in_bytes_with_nested_structure_then_returns_correct_size() {
    use super::IntermediateStructField;
    use ironplc_dsl::core::Id;

    // Create inner structure (two INT fields = 4 bytes total)
    let inner_struct = IntermediateType::Structure {
        fields: vec![
            IntermediateStructField {
                name: Id::from("x"),
                field_type: IntermediateType::Int {
                    size: ByteSized::B16,
                },
                offset: 0,
                var_type: None,
                initial_value: None,
            },
            IntermediateStructField {
                name: Id::from("y"),
                field_type: IntermediateType::Int {
                    size: ByteSized::B16,
                },
                offset: 2,
                var_type: None,
                initial_value: None,
            },
        ],
        bound: AlignmentBound::Natural,
    };

    // Create function block containing the structure
    let fb_type = IntermediateType::FunctionBlock {
        name: "PositionFB".to_string(),
        fields: vec![
            IntermediateStructField {
                name: Id::from("position"),
                field_type: inner_struct,
                offset: 0,
                var_type: None,
                initial_value: None,
            },
            IntermediateStructField {
                name: Id::from("id"),
                field_type: IntermediateType::Int {
                    size: ByteSized::B32,
                },
                offset: 4,
                var_type: None,
                initial_value: None,
            },
        ],
    };

    // Size should be 8 bytes (4 bytes for inner struct + 4 bytes for id)
    assert_eq!(fb_type.size_in_bytes(), Some(8));
}

#[test]
fn function_block_size_in_bytes_with_unknown_field_size_then_returns_none() {
    use super::IntermediateStructField;
    use ironplc_dsl::core::Id;

    // Function block containing a field with unknown size (dynamic array)
    let fields = vec![
        IntermediateStructField {
            name: Id::from("counter"),
            field_type: IntermediateType::Int {
                size: ByteSized::B32,
            },
            offset: 0,
            var_type: None,
            initial_value: None,
        },
        IntermediateStructField {
            name: Id::from("buffer"),
            field_type: IntermediateType::Array {
                element_type: Box::new(IntermediateType::Bool),
                dimensions: vec![], // Dynamic size
            },
            offset: 4,
            var_type: None,
            initial_value: None,
        },
    ];

    let fb_type = IntermediateType::FunctionBlock {
        name: "BufferFB".to_string(),
        fields,
    };
    // Should return None because one field has unknown size
    assert_eq!(fb_type.size_in_bytes(), None);
}
