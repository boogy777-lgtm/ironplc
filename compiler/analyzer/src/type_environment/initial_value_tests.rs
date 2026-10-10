//! The value a variable of a type starts at when its own declaration states
//! none: one answer for every kind of type, through every alias.

use crate::type_environment::{TypeEnvironment, TypeEnvironmentBuilder};
use crate::xform_resolve_type_decl_environment::apply;
use ironplc_dsl::common::{
    ArrayInitialElementKind, ArrayInitialValueAssignment, ConstantKind,
    EnumeratedInitialValueAssignment, InitialValueAssignmentKind, LateResolvedInitializer,
    SimpleInitializer, SpecificationKind, StructInitialValueAssignmentKind, TypeName,
};
use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use rstest::rstest;

/// The environment the declarations of the `TYPE` section `types` make.
fn environment_of(types: &str) -> TypeEnvironment {
    let source = format!("TYPE {types} END_TYPE");
    let library =
        ironplc_parser::parse_program(&source, &FileId::default(), &CompilerOptions::default())
            .unwrap();
    let mut environment = TypeEnvironmentBuilder::new()
        .with_elementary_types()
        .build()
        .unwrap();
    let _ = apply(library, &mut environment);
    environment
}

/// The value as a short text.
fn rendered(value: Option<StructInitialValueAssignmentKind>) -> String {
    match value {
        None => "none".to_string(),
        Some(StructInitialValueAssignmentKind::Constant(ConstantKind::IntegerLiteral(literal))) => {
            literal.to_string()
        }
        Some(StructInitialValueAssignmentKind::Constant(ConstantKind::CharacterString(
            literal,
        ))) => literal.value.iter().collect(),
        Some(StructInitialValueAssignmentKind::EnumeratedValue(value)) => {
            let owner = value.type_name.map(|name| name.to_string());
            format!("{}#{}", owner.unwrap_or_default(), value.value)
        }
        Some(StructInitialValueAssignmentKind::Array(elements)) => {
            format!("array of {}", elements.len())
        }
        Some(other) => format!("{other:?}"),
    }
}

/// How a declaration is written against a type.
#[derive(Clone, Copy, Debug)]
enum Written {
    /// `v : T`, before anything knows what `T` is.
    Bare,
    /// `v : T` once `T` is known to be an enumeration.
    Enumeration,
    /// `v : T` once `T` is known to be an array.
    Array,
    /// `v : T` once `T` is known to be an elementary or string type.
    Simple,
}

impl Written {
    fn against(self, type_name: &str) -> InitialValueAssignmentKind {
        let type_name = TypeName::from(type_name);
        match self {
            Written::Bare => InitialValueAssignmentKind::LateResolvedType(
                LateResolvedInitializer::bare(type_name),
            ),
            Written::Enumeration => {
                InitialValueAssignmentKind::EnumeratedType(EnumeratedInitialValueAssignment {
                    type_name,
                    initial_value: None,
                })
            }
            Written::Array => InitialValueAssignmentKind::Array(ArrayInitialValueAssignment {
                spec: SpecificationKind::Named(type_name),
                initial_values: vec![],
            }),
            Written::Simple => InitialValueAssignmentKind::Simple(SimpleInitializer {
                type_name,
                initial_value: None,
            }),
        }
    }
}

#[rstest]
#[case::elementary("T1 : INT := 5;", Written::Simple, "T1", "5")]
#[case::elementary_states_none("T1 : INT;", Written::Simple, "T1", "none")]
#[case::alias("T1 : INT := 5; T2 : T1;", Written::Bare, "T2", "5")]
#[case::alias_of_alias("T1 : INT := 5; T2 : T1; T3 : T2;", Written::Bare, "T3", "5")]
#[case::nearest_declaration_wins("T1 : INT := 5; T2 : T1 := 9; T3 : T2;", Written::Bare, "T3", "9")]
#[case::enumeration("E1 : (A, B, C) := B;", Written::Enumeration, "E1", "E1#B")]
#[case::enumeration_states_none("E1 : (A, B, C);", Written::Enumeration, "E1", "none")]
#[case::enumeration_alias_keeps_the_value_of_its_enumeration(
    "E1 : (A, B, C) := B; EA : E1; EB : EA;",
    Written::Enumeration,
    "EB",
    "E1#B"
)]
#[case::enumeration_alias_that_states_a_value(
    "E1 : (A, B, C) := B; EA : E1 := C; EB : EA;",
    Written::Bare,
    "EB",
    "EA#C"
)]
#[case::array(
    "A1 : ARRAY [0..2] OF INT := [1, 2, 3];",
    Written::Array,
    "A1",
    "array of 3"
)]
#[case::array_states_none("A1 : ARRAY [0..2] OF INT;", Written::Array, "A1", "none")]
#[case::array_alias(
    "A1 : ARRAY [0..2] OF INT := [1, 2, 3]; A2 : A1; A3 : A2;",
    Written::Array,
    "A3",
    "array of 3"
)]
#[case::string("S1 : STRING := 'abc';", Written::Simple, "S1", "abc")]
#[case::string_with_length("S1 : STRING[10] := 'abc';", Written::Simple, "S1", "abc")]
#[case::string_alias("S1 : STRING[10] := 'abc'; S2 : S1;", Written::Simple, "S2", "abc")]
#[case::string_alias_that_states_a_value(
    "S1 : STRING[10] := 'abc'; S2 : S1 := 'def'; S3 : S2;",
    Written::Simple,
    "S3",
    "def"
)]
#[case::string_states_none("S1 : STRING[10];", Written::Simple, "S1", "none")]
fn initial_value_of_when_declaration_states_none_then_the_value_the_type_declares(
    #[case] types: &str,
    #[case] written: Written,
    #[case] type_name: &str,
    #[case] expected: &str,
) {
    let environment = environment_of(types);

    let value = environment.initial_value_of(&written.against(type_name));

    assert_eq!(rendered(value), expected);
}

#[test]
fn initial_value_of_when_array_declaration_states_elements_then_they_win() {
    let environment = environment_of("A1 : ARRAY [0..2] OF INT := [1, 2, 3];");
    let element = |value: &str| {
        ArrayInitialElementKind::Constant(ConstantKind::integer_literal(value).unwrap())
    };
    let declaration = InitialValueAssignmentKind::Array(ArrayInitialValueAssignment {
        spec: SpecificationKind::Named(TypeName::from("A1")),
        initial_values: vec![element("4"), element("5")],
    });

    let value = environment.initial_value_of(&declaration);

    assert_eq!(rendered(value), "array of 2");
}
