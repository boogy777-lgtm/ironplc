//! Structure type processing
//!
//! This module handles creating structure types from structure declarations,
//! including field validation, offset calculation, and memory layout.

use crate::intermediate_type::{IntermediateStructField, IntermediateType};
use crate::intermediates::array::element_type_id;
use crate::intermediates::enumeration::try_from_values;
use crate::intermediates::layout::{layout, AlignmentBound};
use crate::intermediates::subrange::IntermediateResult;
use crate::resolution::Failure;
use crate::type_attributes::{PartType, Parts};
use crate::type_environment::{TypeAttributes, TypeEnvironment};
use ironplc_dsl::attribute::registry::PACK_MODE;
use ironplc_dsl::attribute::Attributes;
use ironplc_dsl::common::*;
use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::*;
use ironplc_problems::text;

/// Try to create the intermediate type information from the structure specification.
pub fn try_from(
    node_name: &TypeName,
    spec: &StructureDeclaration,
    type_environment: &TypeEnvironment,
) -> Result<TypeAttributes, Failure> {
    from_elements(
        node_name,
        &spec.elements,
        &spec.attributes,
        type_environment,
    )
}

/// Same member processing for a union's members. A union is resolved as a
/// structure for now: its members are not overlaid at offset 0 yet, so it
/// takes the size of the sum of its members instead of the largest (P1 in
/// `Codesys/LEXER-GAP-ANALYSIS.md`).
pub fn from_union(
    node_name: &TypeName,
    spec: &UnionDeclaration,
    type_environment: &TypeEnvironment,
) -> Result<TypeAttributes, Failure> {
    from_elements(
        node_name,
        &spec.elements,
        &spec.attributes,
        type_environment,
    )
}

/// Builds the field list and memory layout shared by structures and unions.
///
/// This is the one place that reads `pack_mode`: the bound it gives travels in
/// the type, and the offsets, the alignment and the size derive from it.
fn from_elements(
    node_name: &TypeName,
    elements: &[StructureElementDeclaration],
    attributes: &Attributes,
    type_environment: &TypeEnvironment,
) -> Result<TypeAttributes, Failure> {
    // Note: Field name uniqueness is validated by semantic rules, not here

    let bound = PACK_MODE
        .of(attributes)
        .map_or(AlignmentBound::Natural, AlignmentBound::from);

    // Resolve the type of each field
    let field_types = elements
        .iter()
        .map(|element| resolve_field_type(element, type_environment))
        .collect::<Result<Vec<IntermediateType>, Failure>>()?;

    let laid_out = layout(&field_types, bound);

    let fields = elements
        .iter()
        .zip(field_types)
        .zip(laid_out.offsets)
        .map(|((element, field_type), offset)| IntermediateStructField {
            name: element.name.clone(),
            field_type,
            offset,
            var_type: None, // Structure fields don't have input/output distinction
            // The value the member declares for itself, if any
            initial_value: type_environment.initial_value_of(&element.init),
        })
        .collect();

    let parts = Parts::Members(
        elements
            .iter()
            .filter_map(|element| {
                Some((element.name.clone(), part_type(element, type_environment)?))
            })
            .collect(),
    );

    Ok(TypeAttributes::new(
        node_name.span(),
        IntermediateType::Structure { fields, bound },
    )
    .with_parts(parts))
}

/// The identity of the type of a member, when the member names its type or is
/// an array of elements of a named type. The representation of a field states
/// what the member looks like and not which declared type it is; see
/// [`Parts`].
fn part_type(
    element: &StructureElementDeclaration,
    type_environment: &TypeEnvironment,
) -> Option<PartType> {
    match &element.init {
        InitialValueAssignmentKind::Array(array) => match &array.spec {
            SpecificationKind::Named(name) => type_environment.id_of(name).map(PartType::Of),
            spec @ SpecificationKind::Inline(subranges) => element_type_id(spec, type_environment)
                .map(|element| PartType::ArrayOf {
                    element,
                    dimensions: subranges.ranges().len().max(1),
                }),
        },
        init => match init.type_reference() {
            TypeReference::Named(name) => type_environment.id_of(&name).map(PartType::Of),
            TypeReference::Inline | TypeReference::Unspecified => None,
        },
    }
}

/// Resolves the field type from the field's initial value assignment
fn resolve_field_type(
    element: &StructureElementDeclaration,
    type_environment: &TypeEnvironment,
) -> Result<IntermediateType, Failure> {
    match &element.init {
        // A value that is an expression of constants is not folded yet when the
        // environment is first derived; the type of the field is the same.
        InitialValueAssignmentKind::Simple(SimpleInitializer { type_name, .. })
        | InitialValueAssignmentKind::SimpleExpr(SimpleExprInitializer { type_name, .. }) => {
            // Handle simple field types like BOOL, INT, etc.
            let type_attrs = type_environment.lookup(type_name).or_failure(|| {
                Diagnostic::text(
                    type_name.span(),
                    text::struct_field_type_not_declared_field_type(),
                )
            })?;
            Ok(type_attrs.representation.clone())
        }
        InitialValueAssignmentKind::LateResolvedType(LateResolvedInitializer {
            type_name, ..
        }) => {
            // LateResolvedType may appear when the field references a user-defined type.
            // Since types are processed in topological order, the referenced type should
            // already be in the environment.
            let type_attrs = type_environment.lookup(type_name).or_failure(|| {
                Diagnostic::text(
                    type_name.span(),
                    text::struct_field_type_not_declared_field_type(),
                )
            })?;
            Ok(type_attrs.representation.clone())
        }
        InitialValueAssignmentKind::Subrange(subrange_spec) => {
            // Handle subrange field types
            // TODO: Replace magic string with proper type-safe approach for anonymous subranges
            // Consider: Option<&TypeName> or dedicated enum for synthetic/anonymous type names
            // Current approach relies on underscore prefix convention which isn't type-safe
            let subrange_result = crate::intermediates::subrange::try_from(
                &TypeName::from("_field_subrange"),
                &subrange_spec.spec,
                type_environment,
            )?;

            match subrange_result {
                IntermediateResult::Type(attrs) => Ok(attrs.representation),
                IntermediateResult::Alias(base_name) => {
                    let base_attrs = type_environment.lookup(&base_name).or_failure(|| {
                        Diagnostic::text(
                            base_name.span(),
                            text::struct_field_type_not_declared_base_type(),
                        )
                    })?;
                    Ok(base_attrs.representation.clone())
                }
            }
        }
        InitialValueAssignmentKind::EnumeratedValues(values) => {
            // Handle enumerated field types with values
            let strict = type_environment.enumeration_is_strict(&Attributes::default());
            let enum_attrs = try_from_values(values, None, strict)?;
            Ok(enum_attrs.representation)
        }
        InitialValueAssignmentKind::EnumeratedType(enum_assignment) => {
            // Handle enumerated field types with type reference
            let type_attrs = type_environment
                .lookup(&enum_assignment.type_name)
                .or_failure(|| {
                    Diagnostic::text(
                        enum_assignment.type_name.span(),
                        text::struct_field_type_not_declared_enumeration_type(),
                    )
                })?;
            Ok(type_attrs.representation.clone())
        }
        InitialValueAssignmentKind::Structure(structure_init) => {
            // Handle nested structure field types
            let type_attrs = type_environment
                .lookup(&structure_init.type_name)
                .or_failure(|| {
                    Diagnostic::text(
                        structure_init.type_name.span(),
                        text::struct_field_type_not_declared_structure_type(),
                    )
                })?;
            Ok(type_attrs.representation.clone())
        }
        InitialValueAssignmentKind::Array(array_init) => {
            // Handle array field types
            let array_result = crate::intermediates::array::try_from(
                &TypeName::from("_field_array"),
                &array_init.spec,
                type_environment,
            )?;

            match array_result {
                crate::intermediates::array::IntermediateResult::Type(attrs) => {
                    Ok(attrs.representation)
                }
                crate::intermediates::array::IntermediateResult::Alias(base_name) => {
                    let base_attrs = type_environment.lookup(&base_name).or_failure(|| {
                        Diagnostic::text(
                            base_name.span(),
                            text::struct_field_type_not_declared_base_type(),
                        )
                    })?;
                    Ok(base_attrs.representation.clone())
                }
            }
        }
        InitialValueAssignmentKind::String(string_init) => {
            let string_attrs = crate::intermediates::string::from(string_init);
            Ok(string_attrs.representation)
        }
        _other => {
            // Other types are not yet supported
            Err(Diagnostic::not_implemented(Label::span(
                element.name.span(),
                "Structure field with an unsupported type",
            ))
            .into())
        }
    }
}

#[cfg(test)]
mod tests {
    use crate::intermediate_type::IntermediateType;
    use crate::type_environment::{TypeEnvironment, TypeEnvironmentBuilder};
    use crate::xform_resolve_type_decl_environment::apply;
    use ironplc_dsl::common::TypeName;
    use ironplc_dsl::core::{FileId, Id};
    use ironplc_parser::options::{CompilerOptions, Dialect};
    use ironplc_problems::Problem;
    use ironplc_test::cast_struct;
    use rstest::rstest;
    use spec_test_macro::spec_test;

    /// Helper function to parse a program and apply type resolution
    /// Returns the type environment with resolved types
    fn parse_and_apply(program: &str) -> TypeEnvironment {
        parse_and_apply_with(program, &CompilerOptions::default())
    }

    /// Same as [`parse_and_apply`] with the options of the CODESYS dialect, which
    /// reads the attribute pragmas and the union type.
    fn parse_and_apply_codesys(program: &str) -> TypeEnvironment {
        parse_and_apply_with(program, &CompilerOptions::from_dialect(Dialect::Codesys))
    }

    fn parse_and_apply_with(program: &str, options: &CompilerOptions) -> TypeEnvironment {
        let input = ironplc_parser::parse_program(program, &FileId::default(), options).unwrap();
        let mut env = TypeEnvironmentBuilder::new()
            .with_elementary_types()
            .build()
            .unwrap();
        let result = apply(input, &mut env);
        assert!(
            result.diagnostics.is_empty(),
            "Expected no diagnostics, got: {:?}",
            result.diagnostics
        );
        env
    }

    /// Helper function to parse a program and expect an error
    /// Returns the error diagnostics
    fn parse_and_expect_error(program: &str) -> Vec<ironplc_dsl::diagnostic::Diagnostic> {
        let input =
            ironplc_parser::parse_program(program, &FileId::default(), &CompilerOptions::default())
                .unwrap();
        let mut env = TypeEnvironmentBuilder::new()
            .with_elementary_types()
            .build()
            .unwrap();
        let result = apply(input, &mut env);
        assert!(!result.diagnostics.is_empty());
        result.diagnostics
    }

    // Parser-based tests (more readable and maintainable)

    #[test]
    fn parse_simple_structure_then_creates_structure_type() {
        let program = "
TYPE
    MyStruct : STRUCT
        x : INT := 0;
        y : BOOL := FALSE;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        // Check that the structure type was created
        let struct_type = env.get(&TypeName::from("MyStruct")).unwrap();
        assert!(struct_type.representation.is_structure());

        let fields = cast_struct!(
            &struct_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 2);
        assert_eq!(fields[0].name, Id::from("x"));
        assert_eq!(fields[1].name, Id::from("y"));

        // Check field types
        assert!(matches!(fields[0].field_type, IntermediateType::Int { .. }));
        assert!(matches!(fields[1].field_type, IntermediateType::Bool));

        // Check field offsets (INT at 0, BOOL at 2 due to alignment)
        assert_eq!(fields[0].offset, 0);
        assert_eq!(fields[1].offset, 2);
    }

    #[test]
    fn parse_structure_with_subrange_field_then_creates_structure_with_subrange() {
        let program = "
TYPE
    MyStruct : STRUCT
        range_field : INT (1..100) := 50;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let struct_type = env.get(&TypeName::from("MyStruct")).unwrap();
        assert!(struct_type.representation.is_structure());

        let fields = cast_struct!(
            &struct_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 1);
        assert_eq!(fields[0].name, Id::from("range_field"));
        assert!(matches!(
            fields[0].field_type,
            IntermediateType::Subrange { .. }
        ));
    }

    #[test]
    fn parse_structure_with_enumerated_values_field_then_creates_structure_with_enum() {
        let program = "
TYPE
    MyStruct : STRUCT
        color_field : (RED, GREEN, BLUE) := RED;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let struct_type = env.get(&TypeName::from("MyStruct")).unwrap();
        assert!(struct_type.representation.is_structure());

        let fields = cast_struct!(
            &struct_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 1);
        assert_eq!(fields[0].name, Id::from("color_field"));
        assert!(matches!(
            fields[0].field_type,
            IntermediateType::Enumeration { .. }
        ));
    }

    #[test]
    fn parse_structure_with_enumerated_type_field_then_creates_structure_with_enum_ref() {
        let program = "
TYPE
    ColorEnum : (RED, GREEN, BLUE) := RED;
    MyStruct : STRUCT
        color_field : ColorEnum := RED;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let struct_type = env.get(&TypeName::from("MyStruct")).unwrap();
        assert!(struct_type.representation.is_structure());

        let fields = cast_struct!(
            &struct_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 1);
        assert_eq!(fields[0].name, Id::from("color_field"));
        assert!(matches!(
            fields[0].field_type,
            IntermediateType::Enumeration { .. }
        ));
    }

    #[test]
    fn parse_structure_with_custom_type_field_then_creates_structure_with_custom_type() {
        let program = "
TYPE
    CustomType : INT := 0;
    MyStruct : STRUCT
        custom_field : CustomType := 42;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let struct_type = env.get(&TypeName::from("MyStruct")).unwrap();
        assert!(struct_type.representation.is_structure());

        let fields = cast_struct!(
            &struct_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 1);
        assert_eq!(fields[0].name, Id::from("custom_field"));
        assert!(matches!(fields[0].field_type, IntermediateType::Int { .. }));
    }

    #[test]
    fn parse_structure_with_missing_field_type_then_error() {
        let program = "
TYPE
    MyStruct : STRUCT
        x : MISSING_TYPE := 0;
    END_STRUCT;
END_TYPE
        ";
        let errors = parse_and_expect_error(program);

        assert!(!errors.is_empty());
        assert_eq!(Problem::StructFieldTypeNotDeclared.code(), errors[0].code);
    }

    // Nested structure tests

    #[test]
    fn parse_structure_with_nested_structure_then_creates_structure_with_nested_type() {
        let program = "
TYPE
    Point : STRUCT
        x : INT := 0;
        y : INT := 0;
    END_STRUCT;
    Rectangle : STRUCT
        topLeft : Point;
        bottomRight : Point;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let rect_type = env.get(&TypeName::from("Rectangle")).unwrap();
        assert!(rect_type.representation.is_structure());

        let fields = cast_struct!(
            &rect_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 2);
        assert_eq!(fields[0].name, Id::from("topLeft"));
        assert_eq!(fields[1].name, Id::from("bottomRight"));
        assert!(fields[0].field_type.is_structure());
        assert!(fields[1].field_type.is_structure());
    }

    #[test]
    fn parse_structure_with_deeply_nested_structures_then_creates_correct_types() {
        let program = "
TYPE
    Inner : STRUCT
        value : INT := 0;
    END_STRUCT;
    Middle : STRUCT
        inner : Inner;
    END_STRUCT;
    Outer : STRUCT
        middle : Middle;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let outer_type = env.get(&TypeName::from("Outer")).unwrap();
        assert!(outer_type.representation.is_structure());

        let fields = cast_struct!(
            &outer_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 1);
        assert_eq!(fields[0].name, Id::from("middle"));
        assert!(fields[0].field_type.is_structure());
    }

    #[test]
    fn parse_structure_with_undeclared_nested_type_then_error() {
        let program = "
TYPE
    MyStruct : STRUCT
        nested : UndeclaredType;
    END_STRUCT;
END_TYPE
        ";
        let errors = parse_and_expect_error(program);

        assert!(!errors.is_empty());
        assert_eq!(Problem::StructFieldTypeNotDeclared.code(), errors[0].code);
    }

    #[test]
    fn parse_structure_with_nested_structure_then_calculates_correct_offsets() {
        let program = "
TYPE
    Point : STRUCT
        x : INT := 0;
        y : INT := 0;
    END_STRUCT;
    Line : STRUCT
        start : Point;
        endPt : Point;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let line_type = env.get(&TypeName::from("Line")).unwrap();
        let fields = cast_struct!(
            &line_type.representation,
            IntermediateType::Structure { fields }
        );

        // Point is 4 bytes (2x INT with 2-byte alignment), so:
        // - start at offset 0
        // - endPt at offset 4
        assert_eq!(fields[0].offset, 0);
        assert_eq!(fields[1].offset, 4);
    }

    #[test]
    fn parse_structure_with_mixed_nested_and_primitive_fields_then_correct_layout() {
        let program = "
TYPE
    Point : STRUCT
        x : INT := 0;
        y : INT := 0;
    END_STRUCT;
    Entity : STRUCT
        id : DINT := 0;
        position : Point;
        active : BOOL := FALSE;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let entity_type = env.get(&TypeName::from("Entity")).unwrap();
        let fields = cast_struct!(
            &entity_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 3);
        // id (DINT) at offset 0 (4 bytes)
        // position (Point) at offset 4 (4 bytes, 2-byte alignment)
        // active (BOOL) at offset 8 (1 byte)
        assert_eq!(fields[0].offset, 0);
        assert_eq!(fields[1].offset, 4);
        assert_eq!(fields[2].offset, 8);
    }

    // Array field tests

    #[test]
    fn parse_structure_with_array_field_then_creates_structure_with_array() {
        let program = "
TYPE
    MyStruct : STRUCT
        values : ARRAY [1..10] OF INT;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let struct_type = env.get(&TypeName::from("MyStruct")).unwrap();
        assert!(struct_type.representation.is_structure());

        let fields = cast_struct!(
            &struct_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 1);
        assert_eq!(fields[0].name, Id::from("values"));
        assert!(matches!(
            fields[0].field_type,
            IntermediateType::Array { .. }
        ));
    }

    #[test]
    fn parse_structure_with_multidimensional_array_field_then_creates_structure_with_array() {
        let program = "
TYPE
    Matrix : STRUCT
        data : ARRAY [1..3, 1..4] OF REAL;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let struct_type = env.get(&TypeName::from("Matrix")).unwrap();
        assert!(struct_type.representation.is_structure());

        let fields = cast_struct!(
            &struct_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 1);
        assert_eq!(fields[0].name, Id::from("data"));
        let (element_type, dimensions) = cast_struct!(
            &fields[0].field_type,
            IntermediateType::Array {
                element_type,
                dimensions
            }
        );
        let _ = element_type;
        assert_eq!(dimensions.len(), 2);
        // Total elements: (3-1+1) * (4-1+1) = 3 * 4 = 12
        assert_eq!(fields[0].field_type.array_total_elements(), Some(12));
    }

    #[test]
    fn parse_structure_with_array_of_struct_field_then_creates_structure_with_nested_array() {
        let program = "
TYPE
    Point : STRUCT
        x : INT := 0;
        y : INT := 0;
    END_STRUCT;
    Polygon : STRUCT
        vertices : ARRAY [1..4] OF Point;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let polygon_type = env.get(&TypeName::from("Polygon")).unwrap();
        assert!(polygon_type.representation.is_structure());

        let fields = cast_struct!(
            &polygon_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 1);
        assert_eq!(fields[0].name, Id::from("vertices"));
        let (element_type, dimensions) = cast_struct!(
            &fields[0].field_type,
            IntermediateType::Array {
                element_type,
                dimensions
            }
        );
        assert!(element_type.is_structure());
        assert_eq!(dimensions.len(), 1);
        assert_eq!(fields[0].field_type.array_total_elements(), Some(4));
    }

    #[test]
    fn parse_structure_with_mixed_array_and_primitive_fields_then_correct_types() {
        let program = "
TYPE
    DataRecord : STRUCT
        id : DINT := 0;
        values : ARRAY [1..5] OF INT;
        active : BOOL := FALSE;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let struct_type = env.get(&TypeName::from("DataRecord")).unwrap();
        let fields = cast_struct!(
            &struct_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 3);
        assert!(matches!(fields[0].field_type, IntermediateType::Int { .. }));
        assert!(matches!(
            fields[1].field_type,
            IntermediateType::Array { .. }
        ));
        assert!(matches!(fields[2].field_type, IntermediateType::Bool));
    }

    // String field tests

    #[test]
    fn parse_structure_with_string_field_then_creates_structure_with_string() {
        let program = "
TYPE
    MyStruct : STRUCT
        name : STRING;
        value : INT := 0;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let struct_type = env.get(&TypeName::from("MyStruct")).unwrap();
        assert!(struct_type.representation.is_structure());

        let fields = cast_struct!(
            &struct_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 2);
        assert_eq!(fields[0].name, Id::from("name"));
        assert!(matches!(
            fields[0].field_type,
            IntermediateType::String { max_len: None, .. }
        ));
        assert_eq!(fields[1].name, Id::from("value"));
        assert!(matches!(fields[1].field_type, IntermediateType::Int { .. }));
    }

    #[test]
    fn parse_structure_with_sized_string_field_then_creates_structure_with_sized_string() {
        let program = "
TYPE
    MyStruct : STRUCT
        name : STRING[30];
        value : INT := 0;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let struct_type = env.get(&TypeName::from("MyStruct")).unwrap();
        let fields = cast_struct!(
            &struct_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 2);
        assert_eq!(fields[0].name, Id::from("name"));
        assert!(matches!(
            fields[0].field_type,
            IntermediateType::String {
                max_len: Some(30),
                ..
            }
        ));
    }

    #[test]
    fn parse_structure_with_mixed_string_and_numeric_fields_then_correct_types() {
        let program = "
TYPE
    MyData : STRUCT
        label : STRING;
        count : DINT := 0;
        name : STRING[50];
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        let struct_type = env.get(&TypeName::from("MyData")).unwrap();
        let fields = cast_struct!(
            &struct_type.representation,
            IntermediateType::Structure { fields }
        );

        assert_eq!(fields.len(), 3);
        assert!(matches!(
            fields[0].field_type,
            IntermediateType::String { max_len: None, .. }
        ));
        assert!(matches!(fields[1].field_type, IntermediateType::Int { .. }));
        assert!(matches!(
            fields[2].field_type,
            IntermediateType::String {
                max_len: Some(50),
                ..
            }
        ));
    }

    #[test]
    fn parse_nested_structure_with_defaults_then_outer_field_has_default_true() {
        let program = "
TYPE
    Inner : STRUCT
        a : INT := 0;
        b : INT := 0;
    END_STRUCT;
    Outer : STRUCT
        inner : Inner;
    END_STRUCT;
END_TYPE
        ";
        let env = parse_and_apply(program);

        // Check Inner type
        let inner_type = env.get(&TypeName::from("Inner")).unwrap();
        let inner_fields = cast_struct!(
            &inner_type.representation,
            IntermediateType::Structure { fields }
        );
        assert!(inner_fields[0].has_default(), "Inner.a should have default");
        assert!(inner_fields[1].has_default(), "Inner.b should have default");

        // Check Outer type - the inner field should have has_default=true
        // because Inner has all defaults
        let outer_type = env.get(&TypeName::from("Outer")).unwrap();
        let outer_fields = cast_struct!(
            &outer_type.representation,
            IntermediateType::Structure { fields }
        );

        assert!(
            outer_fields[0].has_default(),
            "Outer.inner should have has_default=true because Inner has all defaults"
        );
    }

    /// The third example of the CODESYS help for `pack_mode`, with `pragma`
    /// before the declaration.
    fn example_3_declared_with(pragma: &str) -> String {
        format!(
            "{pragma}
TYPE Example : STRUCT
    a : BYTE; b : LWORD; c : BYTE; d : BYTE; e : DWORD; f : BYTE; g : BYTE; h : WORD;
END_STRUCT END_TYPE"
        )
    }

    #[rstest]
    #[case::without_attribute("", vec![0, 8, 16, 17, 20, 24, 25, 26], 28)]
    #[case::mode_0("{attribute 'pack_mode' := '0'}", vec![0, 1, 9, 10, 11, 15, 16, 17], 19)]
    #[case::mode_1("{attribute 'pack_mode' := '1'}", vec![0, 1, 9, 10, 11, 15, 16, 17], 19)]
    #[case::mode_2("{attribute 'pack_mode' := '2'}", vec![0, 2, 10, 11, 12, 16, 17, 18], 20)]
    #[case::mode_4("{attribute 'pack_mode' := '4'}", vec![0, 4, 12, 13, 16, 20, 21, 22], 24)]
    #[case::mode_8("{attribute 'pack_mode' := '8'}", vec![0, 8, 16, 17, 20, 24, 25, 26], 28)]
    #[case::mode_in_other_case("{attribute 'PACK_MODE' := '1'}", vec![0, 1, 9, 10, 11, 15, 16, 17], 19)]
    #[spec_test(REQ_ATTR_analyzer_020)]
    fn try_from_when_structure_has_pack_mode_then_fields_stand_at_the_offsets_of_the_mode(
        #[case] pragma: &str,
        #[case] offsets: Vec<u32>,
        #[case] size: u32,
    ) {
        let env = parse_and_apply_codesys(&example_3_declared_with(pragma));

        let example = env.get(&TypeName::from("Example")).unwrap();
        let fields = cast_struct!(
            &example.representation,
            IntermediateType::Structure { fields }
        );
        let found: Vec<u32> = fields.iter().map(|field| field.offset).collect();
        assert_eq!(found, offsets);
        assert_eq!(example.representation.size_in_bytes(), Some(size));
    }

    #[test]
    #[spec_test(REQ_ATTR_analyzer_020)]
    fn from_union_when_union_has_pack_mode_then_members_stand_at_the_offsets_of_the_mode() {
        let program = "{attribute 'pack_mode' := '1'}
TYPE U : UNION a : BYTE; b : LWORD; END_UNION; END_TYPE";
        let env = parse_and_apply_codesys(program);

        let union = env.get(&TypeName::from("U")).unwrap();
        let fields = cast_struct!(
            &union.representation,
            IntermediateType::Structure { fields }
        );
        assert_eq!(fields[1].offset, 1);
    }

    #[test]
    fn try_from_when_pack_mode_is_not_a_known_value_then_natural_alignment() {
        let program = "{attribute 'pack_mode' := '3'}
TYPE S : STRUCT a : BYTE; b : LWORD; END_STRUCT END_TYPE";
        let input = ironplc_parser::parse_program(
            program,
            &FileId::default(),
            &CompilerOptions::from_dialect(Dialect::Codesys),
        )
        .unwrap();
        let mut env = TypeEnvironmentBuilder::new()
            .with_elementary_types()
            .build()
            .unwrap();
        apply(input, &mut env);

        // The value is reported by the attribute check; the layout ignores it.
        let s = env.get(&TypeName::from("S")).unwrap();
        let fields = cast_struct!(&s.representation, IntermediateType::Structure { fields });
        assert_eq!(fields[1].offset, 8);
    }

    #[test]
    fn try_from_when_structure_nests_in_a_packed_structure_then_it_keeps_its_own_layout() {
        // The inner structure is not packed, so its LWORD stays at 8 and the
        // inner structure is 16 bytes where it nests; the outer structure only
        // bounds where the inner one starts.
        let program = "TYPE Inner : STRUCT a : BYTE; b : LWORD; END_STRUCT END_TYPE
{attribute 'pack_mode' := '1'}
TYPE Outer : STRUCT x : BYTE; inner : Inner; y : BYTE; END_STRUCT END_TYPE";
        let env = parse_and_apply_codesys(program);

        let inner = env.get(&TypeName::from("Inner")).unwrap();
        let inner_fields = cast_struct!(
            &inner.representation,
            IntermediateType::Structure { fields }
        );
        let outer = env.get(&TypeName::from("Outer")).unwrap();
        let outer_fields = cast_struct!(
            &outer.representation,
            IntermediateType::Structure { fields }
        );
        assert_eq!(inner_fields[1].offset, 8);
        assert_eq!(outer_fields[1].offset, 1);
        assert_eq!(outer_fields[2].offset, 17);
        assert_eq!(outer.representation.size_in_bytes(), Some(18));
    }
}
