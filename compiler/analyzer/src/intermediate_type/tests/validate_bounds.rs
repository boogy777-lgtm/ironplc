//! Tests for validating subrange bounds against the base type.

use super::*;

#[test]
fn validate_bounds_with_all_integer_types_then_validates_correctly() {
    use ironplc_dsl::common::TypeName;

    // Test all signed integer types
    let sint_type = IntermediateType::Int {
        size: ByteSized::B8,
    };
    assert!(sint_type
        .validate_bounds(-128, 127, &TypeName::from("TEST"))
        .is_ok());
    assert!(sint_type
        .validate_bounds(-129, 127, &TypeName::from("TEST"))
        .is_err());
    assert!(sint_type
        .validate_bounds(-128, 128, &TypeName::from("TEST"))
        .is_err());

    let int_type = IntermediateType::Int {
        size: ByteSized::B16,
    };
    assert!(int_type
        .validate_bounds(-32768, 32767, &TypeName::from("TEST"))
        .is_ok());
    assert!(int_type
        .validate_bounds(-32769, 32767, &TypeName::from("TEST"))
        .is_err());

    let dint_type = IntermediateType::Int {
        size: ByteSized::B32,
    };
    assert!(dint_type
        .validate_bounds(-2147483648, 2147483647, &TypeName::from("TEST"))
        .is_ok());
    assert!(dint_type
        .validate_bounds(-2147483649, 2147483647, &TypeName::from("TEST"))
        .is_err());

    let lint_type = IntermediateType::Int {
        size: ByteSized::B64,
    };
    assert!(lint_type
        .validate_bounds(i64::MIN as i128, i64::MAX as i128, &TypeName::from("TEST"))
        .is_ok());

    // Test all unsigned integer types
    let usint_type = IntermediateType::UInt {
        size: ByteSized::B8,
    };
    assert!(usint_type
        .validate_bounds(0, 255, &TypeName::from("TEST"))
        .is_ok());
    assert!(usint_type
        .validate_bounds(-1, 255, &TypeName::from("TEST"))
        .is_err());
    assert!(usint_type
        .validate_bounds(0, 256, &TypeName::from("TEST"))
        .is_err());

    let uint_type = IntermediateType::UInt {
        size: ByteSized::B16,
    };
    assert!(uint_type
        .validate_bounds(0, 65535, &TypeName::from("TEST"))
        .is_ok());
    assert!(uint_type
        .validate_bounds(-1, 65535, &TypeName::from("TEST"))
        .is_err());

    let udint_type = IntermediateType::UInt {
        size: ByteSized::B32,
    };
    assert!(udint_type
        .validate_bounds(0, 4294967295, &TypeName::from("TEST"))
        .is_ok());
    assert!(udint_type
        .validate_bounds(-1, 4294967295, &TypeName::from("TEST"))
        .is_err());

    let ulint_type = IntermediateType::UInt {
        size: ByteSized::B64,
    };
    assert!(ulint_type
        .validate_bounds(0, u64::MAX as i128, &TypeName::from("TEST"))
        .is_ok());
    assert!(ulint_type
        .validate_bounds(-1, u64::MAX as i128, &TypeName::from("TEST"))
        .is_err());
}

#[test]
fn validate_bounds_with_nested_subrange_then_validates_correctly() {
    use ironplc_dsl::common::TypeName;

    // Create a nested subrange: INT (10..100) -> SUBRANGE (20..80)
    let nested_subrange = IntermediateType::Subrange {
        base_type: Box::new(IntermediateType::Int {
            size: ByteSized::B16,
        }),
        min_value: 10,
        max_value: 100,
    };

    // Valid bounds within the nested subrange
    assert!(nested_subrange
        .validate_bounds(20, 80, &TypeName::from("TEST"))
        .is_ok());
    assert!(nested_subrange
        .validate_bounds(10, 100, &TypeName::from("TEST"))
        .is_ok());

    // Invalid bounds outside the nested subrange
    assert!(nested_subrange
        .validate_bounds(5, 80, &TypeName::from("TEST"))
        .is_err());
    assert!(nested_subrange
        .validate_bounds(20, 150, &TypeName::from("TEST"))
        .is_err());
    assert!(nested_subrange
        .validate_bounds(5, 150, &TypeName::from("TEST"))
        .is_err());
}

#[test]
fn validate_bounds_with_non_numeric_type_then_error() {
    use ironplc_dsl::common::TypeName;
    use ironplc_problems::Problem;

    let string_type = IntermediateType::String {
        max_len: Some(10),
        char_width: CharWidth::Narrow,
    };
    let result = string_type.validate_bounds(0, 10, &TypeName::from("TEST"));
    assert!(result.is_err());
    let error = result.unwrap_err();
    assert_eq!(error.code, Problem::SubrangeBaseTypeNotNumeric.code());

    let bool_type = IntermediateType::Bool;
    let result = bool_type.validate_bounds(0, 1, &TypeName::from("TEST"));
    assert!(result.is_err());
    let error = result.unwrap_err();
    assert_eq!(error.code, Problem::SubrangeBaseTypeNotNumeric.code());

    let array_type = IntermediateType::Array {
        element_type: Box::new(IntermediateType::Int {
            size: ByteSized::B16,
        }),
        dimensions: vec![ArrayDimension {
            lower: 1,
            upper: 10,
        }],
    };
    let result = array_type.validate_bounds(0, 9, &TypeName::from("TEST"));
    assert!(result.is_err());
    let error = result.unwrap_err();
    assert_eq!(error.code, Problem::SubrangeBaseTypeNotNumeric.code());
}
