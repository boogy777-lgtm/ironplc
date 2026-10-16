//! Type attributes for the IronPLC compiler.
//!
//! This module provides the TypeAttributes struct that combines type representation
//! with categorization information. Memory layout information is accessed directly
//! through IntermediateType methods.

use ironplc_dsl::common::StructInitialValueAssignmentKind;
use ironplc_dsl::core::{Id, Located, SourceSpan};
use ironplc_dsl::type_id::TypeId;

use crate::{intermediate_type::IntermediateType, type_category::TypeCategory};

/// The type of one part of a type that has parts.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum PartType {
    /// A type of the environment, by identity: what the representation of
    /// the part cannot say, as two enumerations that list the same number of
    /// values have the same representation.
    Of(TypeId),
    /// An array spelled out in place (`a : ARRAY[1..2] OF E`), which has no
    /// type of its own in the environment, with the type of its elements.
    ArrayOf(TypeId),
}

/// The parts of a type, with the identity of the type of each. The
/// representation of a type states what its parts look like and not which
/// declared types they are, which is what a check of the values written for
/// them needs.
#[derive(Debug, Clone, PartialEq, Default)]
pub enum Parts {
    /// A type that has no parts, or whose parts are not recorded.
    #[default]
    None,
    /// The members of a structure that name their type.
    Members(Vec<(Id, PartType)>),
    /// The type of the elements of an array that names it.
    Elements(TypeId),
}

/// Attributes associated with a type in the type environment.
#[derive(Debug, Clone, PartialEq)]
pub struct TypeAttributes {
    /// The location in source code that defined the type.
    /// TODO this should be unnecessary since the TypeName already has a span.
    pub span: SourceSpan,
    /// The intermediate representation of the type
    pub representation: IntermediateType,
    /// Category of the type (elementary, user-defined, or derived)
    pub type_category: TypeCategory,
    /// The value a declaration of this type starts at when it states none
    /// (`TYPE Level : INT := 5; END_TYPE`). `None` when the type declares no
    /// value of its own, which leaves the type's default: zero, or the lower
    /// bound of a subrange.
    pub initial_value: Option<Box<StructInitialValueAssignmentKind>>,
    /// The values an enumeration declares, in declaration order; empty for a
    /// type that is not an enumeration.
    ///
    /// The names are part of the type and not of its representation: two
    /// enumerations with the same underlying type are different types. An
    /// alias is entered with the attributes of the type its declaration names
    /// (`TypeEnvironment::insert_alias`), so it has the same values as that
    /// type and a name that two enumerations share belongs to both.
    pub enumerated_values: Vec<Id>,
    /// The identity of the types of the parts of a structure or an array. An
    /// alias is entered with the attributes of the type it names, so it has
    /// the parts of that type.
    pub parts: Parts,
}

impl TypeAttributes {
    /// Creates new TypeAttributes with calculated type category
    pub fn new(span: SourceSpan, representation: IntermediateType) -> Self {
        let type_category = TypeCategory::for_type(&representation);
        Self {
            span,
            representation,
            type_category,
            initial_value: None,
            enumerated_values: Vec::new(),
            parts: Parts::None,
        }
    }

    /// Sets the identity of the types of the parts.
    pub fn with_parts(mut self, parts: Parts) -> Self {
        self.parts = parts;
        self
    }

    /// Sets the values an enumeration declares.
    pub fn with_enumerated_values(mut self, values: Vec<Id>) -> Self {
        self.enumerated_values = values;
        self
    }

    /// Creates new TypeAttributes for elementary types
    pub fn elementary(representation: IntermediateType) -> Self {
        Self::new(SourceSpan::default(), representation)
    }

    /// Gets the size in bytes for this type (delegates to IntermediateType)
    ///
    /// Returns `None` if the size cannot be determined (e.g., dynamic arrays,
    /// variable-length strings, or types with unknown field sizes).
    pub fn size_bytes(&self) -> Option<u32> {
        self.representation.size_in_bytes()
    }

    /// Gets the alignment requirement in bytes for this type (delegates to IntermediateType)
    pub fn alignment_bytes(&self) -> u32 {
        self.representation.alignment_bytes() as u32
    }

    /// Returns whether this type has an explicitly specified size (delegates to IntermediateType)
    pub fn has_explicit_size(&self) -> bool {
        self.representation.has_explicit_size()
    }
}

impl Located for TypeAttributes {
    fn span(&self) -> SourceSpan {
        self.span.clone()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::intermediate_type::{ByteSized, IntermediateType};

    #[test]
    fn type_attributes_constructor_sets_correct_fields() {
        let attrs = TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Int {
                size: ByteSized::B32,
            },
        );

        assert_eq!(attrs.size_bytes(), Some(4));
        assert_eq!(attrs.alignment_bytes(), 4);
        assert!(attrs.has_explicit_size());
        assert_eq!(attrs.type_category, TypeCategory::Elementary);
    }

    #[test]
    fn type_attributes_elementary_constructor() {
        let attrs = TypeAttributes::elementary(IntermediateType::Bool);

        assert_eq!(attrs.size_bytes(), Some(1));
        assert_eq!(attrs.alignment_bytes(), 1);
        assert!(attrs.has_explicit_size());
        assert_eq!(attrs.type_category, TypeCategory::Elementary);
        assert_eq!(attrs.span, SourceSpan::default());
    }

    #[test]
    fn type_attributes_convenience_methods_delegate_to_intermediate_type() {
        let attrs = TypeAttributes::new(
            SourceSpan::default(),
            IntermediateType::Real {
                size: ByteSized::B64,
            },
        );

        // Test that convenience methods return the same values as IntermediateType methods
        assert_eq!(attrs.size_bytes(), attrs.representation.size_in_bytes());
        assert_eq!(
            attrs.alignment_bytes(),
            attrs.representation.alignment_bytes() as u32
        );
        assert_eq!(
            attrs.has_explicit_size(),
            attrs.representation.has_explicit_size()
        );
    }
}
