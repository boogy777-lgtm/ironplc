//! Tests for looking up a field's offset and a type's member fields.

use super::*;

#[test]
fn get_field_offset_with_structure_then_returns_correct_offset() {
    use super::IntermediateStructField;
    use ironplc_dsl::core::Id;

    let fields = vec![
        IntermediateStructField {
            name: Id::from("field1"),
            field_type: IntermediateType::Int {
                size: ByteSized::B16,
            },
            offset: 0,
            var_type: None,
            initial_value: None,
        },
        IntermediateStructField {
            name: Id::from("field2"),
            field_type: IntermediateType::Int {
                size: ByteSized::B32,
            },
            offset: 4,
            var_type: None,
            initial_value: None,
        },
    ];

    let struct_type = IntermediateType::Structure {
        fields,
        bound: AlignmentBound::Natural,
    };

    let field1_id = Id::from("field1");
    let field2_id = Id::from("field2");
    let nonexistent_id = Id::from("nonexistent");

    assert_eq!(struct_type.get_field_offset(&field1_id), Some(0));
    assert_eq!(struct_type.get_field_offset(&field2_id), Some(4));
    assert_eq!(struct_type.get_field_offset(&nonexistent_id), None);
}

#[test]
fn get_field_offset_with_case_insensitive_field_name_then_returns_correct_offset() {
    use super::IntermediateStructField;
    use ironplc_dsl::core::Id;

    let fields = vec![IntermediateStructField {
        name: Id::from("MyField"),
        field_type: IntermediateType::Int {
            size: ByteSized::B16,
        },
        offset: 2,
        var_type: None,
        initial_value: None,
    }];

    let struct_type = IntermediateType::Structure {
        fields,
        bound: AlignmentBound::Natural,
    };

    // Test case-insensitive matching following IEC 61131-3 identifier rules
    let lowercase_id = Id::from("myfield");
    let uppercase_id = Id::from("MYFIELD");
    let mixed_case_id = Id::from("MyField");

    assert_eq!(struct_type.get_field_offset(&lowercase_id), Some(2));
    assert_eq!(struct_type.get_field_offset(&uppercase_id), Some(2));
    assert_eq!(struct_type.get_field_offset(&mixed_case_id), Some(2));
}

#[test]
fn get_field_offset_with_non_structure_then_returns_none() {
    use ironplc_dsl::core::Id;

    let int_type = IntermediateType::Int {
        size: ByteSized::B16,
    };
    let field_id = Id::from("any_field");
    assert_eq!(int_type.get_field_offset(&field_id), None);

    let array_type = IntermediateType::Array {
        element_type: Box::new(IntermediateType::Bool),
        dimensions: vec![ArrayDimension { lower: 0, upper: 9 }],
    };
    assert_eq!(array_type.get_field_offset(&field_id), None);
}

#[test]
fn member_fields_when_structure_then_returns_fields() {
    use super::IntermediateStructField;
    use ironplc_dsl::core::Id;

    let fields = vec![IntermediateStructField {
        name: Id::from("field1"),
        field_type: IntermediateType::Bool,
        offset: 0,
        var_type: None,
        initial_value: None,
    }];
    let struct_type = IntermediateType::Structure {
        fields: fields.clone(),
        bound: AlignmentBound::Natural,
    };

    assert!(struct_type.has_members());
    assert_eq!(struct_type.member_fields(), Some(&fields));
}

#[test]
fn member_fields_when_function_block_then_returns_fields() {
    use super::{FunctionBlockVarType, IntermediateStructField};
    use ironplc_dsl::core::Id;

    let fields = vec![IntermediateStructField {
        name: Id::from("Q"),
        field_type: IntermediateType::Bool,
        offset: 0,
        var_type: Some(FunctionBlockVarType::Output),
        initial_value: None,
    }];
    let fb_type = IntermediateType::FunctionBlock {
        name: "MyFB".to_string(),
        fields: fields.clone(),
    };

    assert!(fb_type.has_members());
    assert_eq!(fb_type.member_fields(), Some(&fields));
}

#[test]
fn member_fields_when_not_composite_then_returns_none() {
    assert!(!IntermediateType::Bool.has_members());
    assert_eq!(IntermediateType::Bool.member_fields(), None);

    let array_type = IntermediateType::Array {
        element_type: Box::new(IntermediateType::Bool),
        dimensions: vec![],
    };
    assert!(!array_type.has_members());
    assert_eq!(array_type.member_fields(), None);
}

#[rstest::rstest]
#[case::input(
    ironplc_dsl::common::VariableType::Input,
    Some(FunctionBlockVarType::Input)
)]
#[case::output(
    ironplc_dsl::common::VariableType::Output,
    Some(FunctionBlockVarType::Output)
)]
#[case::in_out(
    ironplc_dsl::common::VariableType::InOut,
    Some(FunctionBlockVarType::InOut)
)]
#[case::local(
    ironplc_dsl::common::VariableType::Var,
    Some(FunctionBlockVarType::Internal)
)]
#[case::static_local(
    ironplc_dsl::common::VariableType::VarStat,
    Some(FunctionBlockVarType::Internal)
)]
#[case::instance_local(
    ironplc_dsl::common::VariableType::VarInst,
    Some(FunctionBlockVarType::Internal)
)]
#[case::temporary(ironplc_dsl::common::VariableType::VarTemp, None)]
#[case::external(ironplc_dsl::common::VariableType::External, None)]
#[case::global(ironplc_dsl::common::VariableType::Global, None)]
fn function_block_var_type_of_when_variable_block_then_field_kind_or_none(
    #[case] variable_type: ironplc_dsl::common::VariableType,
    #[case] expected: Option<FunctionBlockVarType>,
) {
    assert_eq!(FunctionBlockVarType::of(&variable_type), expected);
}
