//! Which declared type each part of a structure or an array is.
//!
//! The representation of a type says what its parts look like, and two
//! enumerations that list the same number of values look the same. A check of
//! the values written for the parts of a structure or the elements of an array
//! needs to know which declared type each part is, so the environment holds
//! that with the type (see [`Parts`]) and answers here.

use ironplc_dsl::{core::Id, type_id::TypeId};

use super::TypeEnvironment;
use crate::{
    intermediate_type::IntermediateType,
    type_attributes::{PartType, Parts},
};

impl TypeEnvironment {
    /// The type of the member `name` of the structure `owner`, or `None` when
    /// `owner` is not a structure, has no such member, or the member does not
    /// name its type.
    pub fn member_part(&self, owner: TypeId, name: &Id) -> Option<PartType> {
        let Parts::Members(members) = &self.get_by_id(owner)?.parts else {
            return None;
        };
        members
            .iter()
            .find_map(|(member, part)| (member == name).then_some(*part))
    }

    /// The type of the elements of the array `array`, or `None` when `array`
    /// is not an array or its elements do not name their type.
    pub fn element_type(&self, array: TypeId) -> Option<TypeId> {
        match self.get_by_id(array)?.parts {
            Parts::Elements(element) => Some(element),
            Parts::None | Parts::Members(_) => None,
        }
    }

    /// The type of the values an array initializer lists for `element`, the
    /// type of the elements of an array: an initializer lists the elements flat
    /// whatever the shape of the array, so an array of arrays is listed by its
    /// innermost elements.
    pub fn listed_element(&self, mut element: TypeId) -> TypeId {
        while self.is_array(element) {
            let Some(inner) = self.element_type(element) else {
                break;
            };
            element = inner;
        }
        element
    }

    /// The type of the element of the array `array` that `subscripts`
    /// subscripts select, or `None` when they do not select one element: too
    /// few subscripts select an array and not a value, and `array` is no
    /// array.
    pub fn subscripted(&self, array: TypeId, subscripts: usize) -> Option<TypeId> {
        let mut current = array;
        let mut remaining = subscripts;
        while remaining > 0 {
            let IntermediateType::Array { dimensions, .. } =
                &self.get_by_id(current)?.representation
            else {
                return None;
            };
            remaining = remaining.checked_sub(dimensions.len().max(1))?;
            current = self.element_type(current)?;
        }
        Some(current)
    }

    /// [`Self::subscripted`] for a part: the type of the element that
    /// `subscripts` subscripts select from an array that is the part.
    pub fn subscripted_part(&self, part: PartType, subscripts: usize) -> Option<TypeId> {
        match part {
            PartType::Of(array) => self.subscripted(array, subscripts),
            PartType::ArrayOf {
                element,
                dimensions,
            } => self.subscripted(element, subscripts.checked_sub(dimensions)?),
        }
    }

    /// The type of the elements of the array that is the part, or `None` when
    /// the part is no array.
    pub fn element_of_part(&self, part: PartType) -> Option<TypeId> {
        match part {
            PartType::Of(array) => self.element_type(array),
            PartType::ArrayOf { element, .. } => Some(element),
        }
    }

    fn is_array(&self, id: TypeId) -> bool {
        matches!(
            self.get_by_id(id)
                .map(|attributes| &attributes.representation),
            Some(IntermediateType::Array { .. })
        )
    }
}

#[cfg(test)]
mod tests {
    use ironplc_dsl::common::TypeName;
    use ironplc_dsl::core::{FileId, Id};
    use ironplc_parser::{options::CompilerOptions, parse_program};

    use crate::stages::analyze;
    use crate::type_attributes::PartType;
    use crate::{SemanticContext, TypeEnvironment};

    /// The environment of an analysis of `source`, whose declarations are
    /// valid.
    fn analysed(source: &str) -> SemanticContext {
        let options = CompilerOptions::default();
        let library = parse_program(source, &FileId::default(), &options).unwrap();
        let (_library, context) = analyze(&[&library], &options).unwrap();
        assert!(!context.has_diagnostics(), "{:?}", context.diagnostics());
        context
    }

    const DECLARATIONS: &str = "TYPE E1 : (A, B); E2 : (C, D); EA : E1; \
         S : STRUCT m : E1; n : INT; arr : ARRAY[0..1] OF E2; inner : EA; END_STRUCT; \
         S2 : S; Row : ARRAY[0..1] OF E1; Grid : ARRAY[0..1] OF Row; Mat : ARRAY[0..1, 0..2] OF E2; END_TYPE";

    fn id(env: &TypeEnvironment, name: &str) -> ironplc_dsl::type_id::TypeId {
        env.id_of(&TypeName::from(name)).unwrap()
    }

    #[test]
    fn member_part_when_member_names_its_type_then_the_declared_type() {
        let context = analysed(DECLARATIONS);
        let env = context.types();

        let part = env.member_part(id(env, "S"), &Id::from("m"));

        assert_eq!(part, Some(PartType::Of(id(env, "E1"))));
    }

    #[test]
    fn member_part_when_member_names_an_alias_then_the_alias_itself() {
        let context = analysed(DECLARATIONS);
        let env = context.types();

        let part = env.member_part(id(env, "S"), &Id::from("inner"));

        assert_eq!(part, Some(PartType::Of(id(env, "EA"))));
    }

    #[test]
    fn member_part_when_member_is_an_array_spelled_in_place_then_its_elements_type() {
        let context = analysed(DECLARATIONS);
        let env = context.types();

        let part = env.member_part(id(env, "S"), &Id::from("arr"));

        assert_eq!(
            part,
            Some(PartType::ArrayOf {
                element: id(env, "E2"),
                dimensions: 1
            })
        );
    }

    #[test]
    fn member_part_when_alias_of_a_structure_then_the_members_of_the_structure() {
        let context = analysed(DECLARATIONS);
        let env = context.types();

        let part = env.member_part(id(env, "S2"), &Id::from("m"));

        assert_eq!(part, Some(PartType::Of(id(env, "E1"))));
    }

    #[test]
    fn member_part_when_no_such_member_or_not_a_structure_then_none() {
        let context = analysed(DECLARATIONS);
        let env = context.types();

        assert_eq!(env.member_part(id(env, "S"), &Id::from("nope")), None);
        assert_eq!(env.member_part(id(env, "E1"), &Id::from("m")), None);
    }

    #[test]
    fn element_type_when_array_of_named_type_then_that_type() {
        let context = analysed(DECLARATIONS);
        let env = context.types();

        assert_eq!(env.element_type(id(env, "Row")), Some(id(env, "E1")));
        assert_eq!(env.element_type(id(env, "E1")), None);
    }

    #[test]
    fn listed_element_when_array_of_arrays_then_the_innermost_elements() {
        let context = analysed(DECLARATIONS);
        let env = context.types();

        let element = env.element_type(id(env, "Grid")).unwrap();

        assert_eq!(element, id(env, "Row"));
        assert_eq!(env.listed_element(element), id(env, "E1"));
    }

    #[test]
    fn subscripted_when_subscripts_select_one_element_then_its_type() {
        let context = analysed(DECLARATIONS);
        let env = context.types();

        assert_eq!(env.subscripted(id(env, "Row"), 1), Some(id(env, "E1")));
        assert_eq!(env.subscripted(id(env, "Mat"), 2), Some(id(env, "E2")));
        assert_eq!(env.subscripted(id(env, "Grid"), 2), Some(id(env, "E1")));
    }

    #[test]
    fn subscripted_part_when_array_spelled_in_place_then_the_element_after_its_dimensions() {
        let context = analysed(DECLARATIONS);
        let env = context.types();
        let part = env.member_part(id(env, "S"), &Id::from("arr")).unwrap();

        assert_eq!(env.subscripted_part(part, 1), Some(id(env, "E2")));
        assert_eq!(env.subscripted_part(part, 0), None);
        assert_eq!(env.element_of_part(part), Some(id(env, "E2")));
    }

    #[test]
    fn subscripted_when_too_few_subscripts_or_no_array_then_none() {
        let context = analysed(DECLARATIONS);
        let env = context.types();

        assert_eq!(env.subscripted(id(env, "Mat"), 1), None);
        assert_eq!(env.subscripted(id(env, "E1"), 1), None);
    }
}
