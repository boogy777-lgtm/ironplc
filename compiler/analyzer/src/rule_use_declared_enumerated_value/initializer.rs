//! The values an initializer gives to the members of a structure or a function
//! block and to the elements of an array.
//!
//! Each value an initializer writes for a part is checked at the position of
//! the part ([`Position::Member`], [`Position::ListedElement`]), and an
//! initializer nested in one is followed to the part it initializes. The
//! matches over the kinds of value have no wildcard arm, so a kind of value
//! added to the language is not compiled until this module says what it does.

use ironplc_dsl::{
    common::{
        ArrayInitialElementKind, ArraySpecificationKind, SpecificationKind,
        StructInitialValueAssignmentKind, StructureElementInit,
    },
    core::Id,
    type_id::TypeId,
};

use super::position::Position;
use super::RuleDeclaredEnumeratedValues;
use crate::{intermediates::array::element_type_id, type_attributes::PartType};

impl RuleDeclaredEnumeratedValues<'_> {
    /// Checks the values given to the members of the structure or function
    /// block `owner`.
    pub(super) fn check_members(&mut self, owner: TypeId, elements: &[StructureElementInit]) {
        for element in elements {
            self.check_member_value(owner, &element.name, &element.init);
        }
    }

    fn check_member_value(
        &mut self,
        owner: TypeId,
        name: &Id,
        value: &StructInitialValueAssignmentKind,
    ) {
        let position = Position::Member { owner, name };
        match value {
            StructInitialValueAssignmentKind::EnumeratedValue(value) => {
                self.check_at(position, value);
            }
            StructInitialValueAssignmentKind::Expression(expr) => {
                self.check_expression_at(position, &expr.kind);
            }
            StructInitialValueAssignmentKind::Structure(elements) => {
                if let Some(PartType::Of(member)) = self.member_part(owner, name) {
                    self.check_members(member, elements);
                }
            }
            StructInitialValueAssignmentKind::Array(elements) => {
                if let Some(element) = self
                    .member_part(owner, name)
                    .and_then(|part| self.context.types().element_of_part(part))
                {
                    self.check_listed(element, elements);
                }
            }
            // A number is stored into an enumeration when it is the number of
            // one of its values, which `rule_constant_range` checks.
            StructInitialValueAssignmentKind::Constant(_) => {}
            // `xform_resolve_late_bound_expr_kind` replaces it before any rule
            // runs.
            StructInitialValueAssignmentKind::LateBound(_) => {}
        }
    }

    /// Checks the values an array initializer lists for an array whose
    /// elements are of the type `element`, each of a repeated value once.
    pub(super) fn check_listed(&mut self, element: TypeId, listed: &[ArrayInitialElementKind]) {
        for value in listed {
            match value {
                ArrayInitialElementKind::EnumValue(value) => {
                    self.check_at(Position::ListedElement(element), value);
                }
                ArrayInitialElementKind::Repeated(repeated) => {
                    if let Some(inner) = repeated.init.as_ref() {
                        self.check_listed(element, std::slice::from_ref(inner));
                    }
                }
                // See `check_member_value`.
                ArrayInitialElementKind::Constant(_) => {}
            }
        }
    }

    /// The type of the elements of the array a declaration specifies.
    pub(super) fn elements_of_spec(&self, spec: &ArraySpecificationKind) -> Option<TypeId> {
        let types = self.context.types();
        match spec {
            SpecificationKind::Named(name) => types.element_type(types.id_of(name)?),
            SpecificationKind::Inline(_) => element_type_id(spec, types),
        }
    }
}
