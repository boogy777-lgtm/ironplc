//! Tests for the type environment.

use super::*;
use crate::intermediates::layout::AlignmentBound;
use crate::{
    intermediate_type::{ArrayDimension, ByteSized, IntermediateType},
    type_attributes::TypeAttributes,
    type_category::TypeCategory,
};
use ironplc_container::CharWidth;
use ironplc_dsl::common::{
    ArrayBounds, ArrayElementType, ArraySubranges, Integer, SignedInteger, SignedIntegerRef,
    Subrange,
};
use ironplc_dsl::core::{Id, SourceSpan};
use ironplc_problems::Problem;

#[test]
fn insert_type_when_type_already_exists_then_p2007_and_first_kept() {
    let mut env = TypeEnvironment::new();
    let first = SourceSpan::range(0, 4);
    env.insert_type(
        &TypeName::from("TYPE"),
        TypeAttributes::new(first.clone(), IntermediateType::Bool),
    );
    env.insert_type(
        &TypeName::from("TYPE"),
        TypeAttributes::new(SourceSpan::range(10, 14), IntermediateType::Bool),
    );

    let duplicates = env.take_duplicates();
    assert_eq!(duplicates.len(), 1);
    assert_eq!(duplicates[0].code, Problem::TypeDeclNameDuplicated.code());
    assert_eq!(env.get(&TypeName::from("TYPE")).unwrap().span(), first);
    assert!(env.take_duplicates().is_empty());
}

#[test]
fn insert_type_when_function_block_repeated_then_p4013() {
    let mut env = TypeEnvironment::new();
    let block = || {
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::FunctionBlock {
                name: "FB".to_string(),
                fields: vec![],
            },
        )
    };
    env.insert_type(&TypeName::from("FB"), block());
    env.insert_type(&TypeName::from("FB"), block());

    let duplicates = env.take_duplicates();
    assert_eq!(duplicates.len(), 1);
    assert_eq!(duplicates[0].code, Problem::PouDeclNameDuplicated.code());
}

#[test]
fn insert_type_when_function_block_repeats_type_then_p4013() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("Shared"),
        TypeAttributes::new(SourceSpan::default(), IntermediateType::Bool),
    );
    env.insert_type(
        &TypeName::from("Shared"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::FunctionBlock {
                name: "Shared".to_string(),
                fields: vec![],
            },
        ),
    );

    assert_eq!(
        env.take_duplicates()[0].code,
        Problem::PouDeclNameDuplicated.code()
    );
}

#[test]
fn insert_alias_when_type_already_exists_then_ok() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("TYPE"),
        TypeAttributes::new(SourceSpan::default(), IntermediateType::Bool),
    );
    assert!(env
        .insert_alias(&TypeName::from("TYPE_ALIAS"), &TypeName::from("TYPE"))
        .is_ok());
}

/// An alias inherits everything its base declares, whatever kind of type
/// the base is: the alias is entered with the base's attributes in full,
/// so a kind of type needs no code of its own to be inherited.
#[rstest::rstest]
#[case::bool(attributes(IntermediateType::Bool))]
#[case::enumeration(
        attributes(IntermediateType::Enumeration {
            underlying_type: Box::new(IntermediateType::Int { size: ByteSized::B8 }),
 base_type: Box::new(IntermediateType::Int {
 size: crate::intermediate_type::ByteSized::B16,
 }),
 numbers: vec![],
})
        .with_enumerated_values(vec![Id::from("A"), Id::from("B")])
    )]
#[case::structure(attributes(IntermediateType::Structure { fields: vec![], bound: AlignmentBound::Natural }))]
#[case::array(attributes(IntermediateType::Array {
        element_type: Box::new(IntermediateType::Bool),
        dimensions: vec![ArrayDimension { lower: 1, upper: 3 }],
    }))]
#[case::subrange(attributes(IntermediateType::Subrange {
        base_type: Box::new(IntermediateType::Int { size: ByteSized::B16 }),
        min_value: 1,
        max_value: 9,
    }))]
#[case::function_block(attributes(IntermediateType::FunctionBlock {
        name: "FB".to_string(),
        fields: vec![],
    }))]
fn insert_alias_when_base_declared_then_alias_has_the_attributes_of_base(
    #[case] base: TypeAttributes,
) {
    let mut env = TypeEnvironment::new();
    env.insert_type(&TypeName::from("BASE"), base.clone());

    env.insert_alias(&TypeName::from("ALIAS"), &TypeName::from("BASE"))
        .unwrap();

    assert_eq!(env.get(&TypeName::from("ALIAS")), Some(&base));
}

fn attributes(representation: IntermediateType) -> TypeAttributes {
    TypeAttributes::new(SourceSpan::default(), representation)
}

#[test]
fn insert_alias_when_type_doesnt_exist_then_error() {
    let mut env = TypeEnvironment::new();
    assert!(env
        .insert_alias(&TypeName::from("TYPE_ALIAS"), &TypeName::from("TYPE"))
        .is_err());
}

#[test]
fn intermediate_type_helper_methods_work_correctly() {
    // Test primitive types
    assert!(IntermediateType::Bool.is_primitive());
    assert!(IntermediateType::Int {
        size: ByteSized::B16
    }
    .is_primitive());
    assert!(IntermediateType::UInt {
        size: ByteSized::B32
    }
    .is_primitive());
    assert!(IntermediateType::Real {
        size: ByteSized::B64
    }
    .is_primitive());
    assert!(IntermediateType::String {
        max_len: Some(10),
        char_width: CharWidth::Narrow,
    }
    .is_primitive());
    assert!(IntermediateType::Time {
        size: ByteSized::B32
    }
    .is_primitive());
    assert!(IntermediateType::Date {
        size: ByteSized::B32
    }
    .is_primitive());

    // Test non-primitive types
    assert!(!IntermediateType::Enumeration {
        underlying_type: Box::new(IntermediateType::Int {
            size: ByteSized::B8
        }),
        base_type: Box::new(IntermediateType::Int {
            size: crate::intermediate_type::ByteSized::B16,
        }),
        numbers: vec![],
    }
    .is_primitive());
    assert!(!IntermediateType::Structure {
        fields: vec![],
        bound: AlignmentBound::Natural
    }
    .is_primitive());
    assert!(!IntermediateType::Array {
        element_type: Box::new(IntermediateType::Int {
            size: ByteSized::B16
        }),
        dimensions: vec![ArrayDimension {
            lower: 1,
            upper: 10
        }]
    }
    .is_primitive());

    // Test numeric types
    assert!(IntermediateType::Int {
        size: ByteSized::B16
    }
    .is_numeric());
    assert!(IntermediateType::UInt {
        size: ByteSized::B32
    }
    .is_numeric());
    assert!(IntermediateType::Real {
        size: ByteSized::B64
    }
    .is_numeric());
    assert!(!IntermediateType::Bool.is_numeric());
    assert!(!IntermediateType::String {
        max_len: Some(10),
        char_width: CharWidth::Narrow,
    }
    .is_numeric());

    // Test integer types
    assert!(IntermediateType::Int {
        size: ByteSized::B16
    }
    .is_integer());
    assert!(IntermediateType::UInt {
        size: ByteSized::B32
    }
    .is_integer());
    assert!(!IntermediateType::Real {
        size: ByteSized::B64
    }
    .is_integer());
    assert!(!IntermediateType::Bool.is_integer());

    // Test subrange types
    let subrange = IntermediateType::Subrange {
        base_type: Box::new(IntermediateType::Int {
            size: ByteSized::B16,
        }),
        min_value: 1,
        max_value: 100,
    };
    assert!(subrange.is_subrange());
    assert!(!subrange.is_primitive());

    // Test function block types
    let fb_type = IntermediateType::FunctionBlock {
        name: "MyFB".to_string(),
        fields: vec![],
    };
    assert!(fb_type.is_function_block());
    assert!(!fb_type.is_primitive());

    // Test function types
    let func_type = IntermediateType::Function {
        return_type: Some(Box::new(IntermediateType::Int {
            size: ByteSized::B16,
        })),
        parameters: vec![],
    };
    assert!(func_type.is_function());
    assert!(!func_type.is_primitive());
}

#[test]
fn type_environment_builder_with_elementary_types() {
    let env = TypeEnvironmentBuilder::new()
        .with_elementary_types()
        .build()
        .unwrap();

    // Check that elementary types are present
    assert!(env.get(&TypeName::from("bool")).is_some());
    assert!(env.get(&TypeName::from("int")).is_some());
    assert!(env.get(&TypeName::from("real")).is_some());
    assert!(env.get(&TypeName::from("string")).is_some());
    assert!(env.get(&TypeName::from("time")).is_some());
    assert!(env.get(&TypeName::from("date")).is_some());

    // Check specific type representations
    let int_type = env.get(&TypeName::from("int")).unwrap();
    assert!(matches!(
        &int_type.representation,
        IntermediateType::Int {
            size: ByteSized::B16
        }
    ));

    let bool_type = env.get(&TypeName::from("bool")).unwrap();
    assert!(matches!(&bool_type.representation, IntermediateType::Bool));
}

#[test]
fn type_environment_is_enumeration_helper() {
    let mut env = TypeEnvironment::new();

    // Add an enumeration type
    env.insert_type(
        &TypeName::from("MY_ENUM"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Enumeration {
                underlying_type: Box::new(IntermediateType::Int {
                    size: ByteSized::B8,
                }),
                base_type: Box::new(IntermediateType::Int {
                    size: crate::intermediate_type::ByteSized::B16,
                }),
                numbers: vec![],
            },
        ),
    );

    // Add a non-enumeration type
    env.insert_type(
        &TypeName::from("MY_INT"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Int {
                size: ByteSized::B16,
            },
        ),
    );

    // Test the helper method
    assert!(env.is_enumeration(&TypeName::from("MY_ENUM")));
    assert!(!env.is_enumeration(&TypeName::from("MY_INT")));
    assert!(!env.is_enumeration(&TypeName::from("NONEXISTENT")));
}

#[test]
fn type_environment_get_memory_size() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("MY_INT"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Int {
                size: ByteSized::B32,
            },
        ),
    );

    // Test successful memory size retrieval
    assert_eq!(
        env.get_memory_size(&TypeName::from("MY_INT")).unwrap(),
        Some(4)
    );

    // Test error for non-existent type
    assert!(env.get_memory_size(&TypeName::from("NONEXISTENT")).is_err());
}

#[test]
fn type_environment_get_all_types_by_category() {
    let mut env = TypeEnvironment::new();

    // Add types of different categories
    env.insert_type(
        &TypeName::from("MY_BOOL"),
        TypeAttributes::new(SourceSpan::default(), IntermediateType::Bool),
    );

    env.insert_type(
        &TypeName::from("MY_ENUM"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Enumeration {
                underlying_type: Box::new(IntermediateType::Int {
                    size: ByteSized::B8,
                }),
                base_type: Box::new(IntermediateType::Int {
                    size: crate::intermediate_type::ByteSized::B16,
                }),
                numbers: vec![],
            },
        ),
    );

    env.insert_type(
        &TypeName::from("MY_SUBRANGE"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Subrange {
                base_type: Box::new(IntermediateType::Int {
                    size: ByteSized::B16,
                }),
                min_value: 1,
                max_value: 100,
            },
        ),
    );

    let categories = env.get_all_types_by_category();

    // Check that types are correctly categorized
    assert!(categories
        .get(&TypeCategory::Elementary)
        .unwrap()
        .contains(&&TypeName::from("MY_BOOL")));
    assert!(categories
        .get(&TypeCategory::UserDefined)
        .unwrap()
        .contains(&&TypeName::from("MY_ENUM")));
    assert!(categories
        .get(&TypeCategory::Derived)
        .unwrap()
        .contains(&&TypeName::from("MY_SUBRANGE")));
}

#[test]
fn validate_type_usage_when_type_exists_and_context_valid_then_ok() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("MY_INT"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Int {
                size: ByteSized::B32,
            },
        ),
    );

    // Test valid usage contexts
    assert!(env
        .validate_type_usage(
            &TypeName::from("MY_INT"),
            &UsageContext::VariableDeclaration
        )
        .is_ok());
    assert!(env
        .validate_type_usage(&TypeName::from("MY_INT"), &UsageContext::SubrangeBase)
        .is_ok());
    assert!(env
        .validate_type_usage(
            &TypeName::from("MY_INT"),
            &UsageContext::EnumerationUnderlying
        )
        .is_ok());
}

#[test]
fn validate_type_usage_when_type_not_found_then_error() {
    let env = TypeEnvironment::new();

    assert!(env
        .validate_type_usage(&TypeName::from("NONEXISTENT"), &UsageContext::General)
        .is_err());
}

#[test]
fn validate_type_usage_when_non_numeric_subrange_base_then_error() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("MY_BOOL"),
        TypeAttributes::new(SourceSpan::default(), IntermediateType::Bool),
    );

    // Boolean is not numeric, should fail for subrange base
    assert!(env
        .validate_type_usage(&TypeName::from("MY_BOOL"), &UsageContext::SubrangeBase)
        .is_err());
}

#[test]
fn validate_type_usage_when_non_integer_enumeration_underlying_then_error() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("MY_REAL"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Real {
                size: ByteSized::B32,
            },
        ),
    );

    // Real is numeric but not integer, should fail for enumeration underlying
    assert!(env
        .validate_type_usage(
            &TypeName::from("MY_REAL"),
            &UsageContext::EnumerationUnderlying
        )
        .is_err());
}

#[test]
fn validate_type_usage_when_function_block_array_element_then_error() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("MY_FB"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::FunctionBlock {
                name: "MyFB".to_string(),
                fields: vec![],
            },
        ),
    );

    // Function blocks cannot be array elements
    assert!(env
        .validate_type_usage(&TypeName::from("MY_FB"), &UsageContext::ArrayElement)
        .is_err());
}

#[test]
fn validate_type_usage_when_function_block_return_type_then_error() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("MY_FB"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::FunctionBlock {
                name: "MyFB".to_string(),
                fields: vec![],
            },
        ),
    );

    // Function blocks cannot be return types
    assert!(env
        .validate_type_usage(&TypeName::from("MY_FB"), &UsageContext::FunctionReturn)
        .is_err());
}

#[test]
fn resolve_array_type_when_array_then_returns_type() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("MY_ARRAY"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Array {
                element_type: Box::new(IntermediateType::Int {
                    size: ByteSized::B16,
                }),
                dimensions: vec![ArrayDimension {
                    lower: 1,
                    upper: 10,
                }],
            },
        ),
    );

    let result = env.resolve_array_type(&TypeName::from("MY_ARRAY"));
    assert!(result.is_some());
    assert!(matches!(result.unwrap(), IntermediateType::Array { .. }));
}

#[test]
fn resolve_array_type_when_not_array_then_returns_none() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("MY_INT"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Int {
                size: ByteSized::B16,
            },
        ),
    );

    assert!(env.resolve_array_type(&TypeName::from("MY_INT")).is_none());
}

#[test]
fn resolve_array_type_when_not_found_then_returns_none() {
    let env = TypeEnvironment::new();
    assert!(env
        .resolve_array_type(&TypeName::from("NONEXISTENT"))
        .is_none());
}

#[test]
fn type_environment_builder_with_stdlib_function_blocks_then_contains_all() {
    let env = TypeEnvironmentBuilder::new()
        .with_elementary_types()
        .with_stdlib_function_blocks()
        .build()
        .unwrap();

    // Check that all 10 stdlib function blocks are present
    assert!(env.get(&TypeName::from("ton")).is_some());
    assert!(env.get(&TypeName::from("tof")).is_some());
    assert!(env.get(&TypeName::from("tp")).is_some());
    assert!(env.get(&TypeName::from("ctu")).is_some());
    assert!(env.get(&TypeName::from("ctd")).is_some());
    assert!(env.get(&TypeName::from("ctud")).is_some());
    assert!(env.get(&TypeName::from("r_trig")).is_some());
    assert!(env.get(&TypeName::from("f_trig")).is_some());
    assert!(env.get(&TypeName::from("sr")).is_some());
    assert!(env.get(&TypeName::from("rs")).is_some());

    // Check that a stdlib FB has the correct type
    let ton = env.get(&TypeName::from("ton")).unwrap();
    assert!(ton.representation.is_function_block());

    // Check that the span is marked as builtin
    assert!(ton.span.is_builtin());
}

#[test]
fn resolve_struct_type_when_structure_then_returns_type() {
    let mut env = TypeEnvironment::new();
    let struct_type = IntermediateType::Structure {
        fields: vec![],
        bound: AlignmentBound::Natural,
    };
    env.insert_type(
        &TypeName::from("MY_STRUCT"),
        TypeAttributes::new(SourceSpan::default(), struct_type.clone()),
    );

    let result = env.resolve_struct_type(&TypeName::from("MY_STRUCT"));
    assert!(result.is_some());
    assert_eq!(result.unwrap(), &struct_type);
}

#[test]
fn resolve_struct_type_when_not_structure_then_returns_none() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("MY_INT"),
        TypeAttributes::new(SourceSpan::default(), IntermediateType::Bool),
    );

    assert!(env.resolve_struct_type(&TypeName::from("MY_INT")).is_none());
    assert!(env
        .resolve_struct_type(&TypeName::from("NOT_FOUND"))
        .is_none());
}

#[test]
fn resolve_member_access_type_when_function_block_then_returns_type() {
    // Unlike resolve_struct_type, member access must also see function
    // block instances so `timer.Q` resolves.
    let mut env = TypeEnvironment::new();
    let fb_type = IntermediateType::FunctionBlock {
        name: "MyFB".to_string(),
        fields: vec![],
    };
    env.insert_type(
        &TypeName::from("MY_FB"),
        TypeAttributes::new(SourceSpan::default(), fb_type.clone()),
    );

    assert!(env.resolve_struct_type(&TypeName::from("MY_FB")).is_none());
    assert_eq!(
        env.resolve_member_access_type(&TypeName::from("MY_FB")),
        Some(&fb_type)
    );
}

#[test]
fn resolve_member_access_type_when_structure_then_returns_type() {
    let mut env = TypeEnvironment::new();
    let struct_type = IntermediateType::Structure {
        fields: vec![],
        bound: AlignmentBound::Natural,
    };
    env.insert_type(
        &TypeName::from("MY_STRUCT"),
        TypeAttributes::new(SourceSpan::default(), struct_type.clone()),
    );

    assert_eq!(
        env.resolve_member_access_type(&TypeName::from("MY_STRUCT")),
        Some(&struct_type)
    );
}

#[test]
fn resolve_member_access_type_when_no_members_then_returns_none() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("MY_BOOL"),
        TypeAttributes::new(SourceSpan::default(), IntermediateType::Bool),
    );

    assert!(env
        .resolve_member_access_type(&TypeName::from("MY_BOOL"))
        .is_none());
    assert!(env
        .resolve_member_access_type(&TypeName::from("NOT_FOUND"))
        .is_none());
}
fn subranges(element: &str) -> ArraySubranges {
    ArraySubranges {
        bounds: ArrayBounds::Ranges(vec![Subrange {
            start: SignedIntegerRef::Literal(SignedInteger {
                value: Integer {
                    span: SourceSpan::default(),
                    value: 0,
                },
                is_neg: false,
            }),
            end: SignedIntegerRef::Literal(SignedInteger {
                value: Integer {
                    span: SourceSpan::default(),
                    value: 3,
                },
                is_neg: false,
            }),
        }]),
        type_name: ArrayElementType::Named(TypeName::from(element)),
        ref_to: None,
    }
}

#[test]
fn resolve_reference_target_when_named_elementary_then_returns_elementary_type() {
    let env = TypeEnvironmentBuilder::new()
        .with_elementary_types()
        .build()
        .unwrap();

    let result = env.resolve_reference_target(
        &TypeName::from("INT_REF"),
        &ReferenceTarget::Named(TypeName::from("INT")),
    );

    assert!(matches!(
        result,
        Ok(IntermediateType::Int {
            size: ByteSized::B16
        })
    ));
}

#[test]
fn resolve_reference_target_when_named_array_then_returns_array_type() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &TypeName::from("ARR4"),
        TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Array {
                element_type: Box::new(IntermediateType::Int {
                    size: ByteSized::B16,
                }),
                dimensions: vec![ArrayDimension { lower: 0, upper: 3 }],
            },
        ),
    );

    let result = env.resolve_reference_target(
        &TypeName::from("ARR_REF"),
        &ReferenceTarget::Named(TypeName::from("ARR4")),
    );

    assert!(matches!(result, Ok(IntermediateType::Array { .. })));
}

#[test]
fn resolve_reference_target_when_inline_array_then_returns_array_type() {
    let env = TypeEnvironmentBuilder::new()
        .with_elementary_types()
        .build()
        .unwrap();

    let result = env.resolve_reference_target(
        &TypeName::from("ARR_REF"),
        &ReferenceTarget::Array(subranges("INT")),
    );

    assert!(result.is_ok());
    let resolved = result.unwrap();
    assert!(matches!(resolved, IntermediateType::Array { .. }));
    assert_eq!(resolved.array_total_elements(), Some(4));
}

#[test]
fn resolve_reference_target_when_named_target_undeclared_then_parent_type_not_declared() {
    let env = TypeEnvironment::new();

    let result = env.resolve_reference_target(
        &TypeName::from("BAD_REF"),
        &ReferenceTarget::Named(TypeName::from("MISSING")),
    );

    assert!(result.is_err());
    assert_eq!(
        result.unwrap_err().code,
        Problem::ParentTypeNotDeclared.code()
    );
}

#[test]
fn resolve_reference_target_when_inline_array_element_undeclared_then_error() {
    let env = TypeEnvironment::new();

    let result = env.resolve_reference_target(
        &TypeName::from("BAD_REF"),
        &ReferenceTarget::Array(subranges("MISSING")),
    );

    assert!(result.is_err());
}
