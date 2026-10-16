//! The places that expect an enumeration, and the one question each is asked.
//!
//! A value written where the language fixes a type has to be a value of that
//! type. [`Position`] lists those places, and [`RuleDeclaredEnumeratedValues::place_of`]
//! answers for each the type it fixes. The answer is one `match` over the
//! places with no wildcard arm, so a place added to the list does not compile
//! until it is classified, and every visitor of the rule asks that one match
//! and no other code for the type it expects.
//!
//! The places that are not in the list have no type the language fixes: an
//! operand of arithmetic, an argument of a function that accepts any type.
//! A value written there is not checked against an enumeration.

use ironplc_dsl::{
    common::{EnumeratedValue, TypeName},
    textual::{Expr, ExprKind, ExprType, Variable},
    type_id::TypeId,
};

use super::expected::OtherEnumeration;
use super::RuleDeclaredEnumeratedValues;

/// A place that expects a value of some type.
#[derive(Clone, Copy)]
pub(super) enum Position<'p> {
    /// The variable an assignment stores into.
    AssignmentTarget(&'p Variable),
    /// A parameter of a function, as the signature declares its type.
    FunctionInput(&'p TypeName),
    /// The initial value of a declaration of the type.
    InitialValue(&'p TypeName),
    /// A label of a `CASE`, whose selector is the expression.
    CaseLabel(&'p Expr),
    /// One operand of a comparison, the other being the expression.
    ComparedWith(&'p Expr),
}

/// The type a place fixes, and who compares a value with it.
#[derive(Clone, Copy)]
pub(super) struct Place {
    pub(super) type_id: TypeId,
    /// Whether the check of the types of the operands reports a value of
    /// another enumeration at this place, or this rule does.
    pub(super) other: OtherEnumeration,
}

impl RuleDeclaredEnumeratedValues<'_> {
    /// The type the language fixes at `position`, or `None` when it fixes none
    /// that is known: a declaration with an error, a name not declared.
    pub(super) fn place_of(&self, position: Position<'_>) -> Option<Place> {
        let types = self.context.types();
        match position {
            // The check of the assignment compares the type of the value with
            // the target's.
            Position::AssignmentTarget(target) => self.target_type(target).map(|type_id| Place {
                type_id,
                other: OtherEnumeration::ReportedByTypes,
            }),
            // The check of the arguments of a call types its operands.
            Position::FunctionInput(type_name) => types.id_of(type_name).map(|type_id| Place {
                type_id,
                other: OtherEnumeration::ReportedByTypes,
            }),
            Position::InitialValue(type_name) => types.id_of(type_name).map(|type_id| Place {
                type_id,
                other: OtherEnumeration::Report,
            }),
            Position::CaseLabel(selector) => type_of_expr(selector).map(|type_id| Place {
                type_id,
                other: OtherEnumeration::Report,
            }),
            Position::ComparedWith(other) => type_of_expr(other).map(|type_id| Place {
                type_id,
                other: OtherEnumeration::Report,
            }),
        }
    }

    /// Checks that `value`, written at `position`, is a value of the
    /// enumeration the position expects. A position that expects no
    /// enumeration expects nothing of it.
    pub(super) fn check_at(&mut self, position: Position<'_>, value: &EnumeratedValue) {
        let Some(place) = self.place_of(position) else {
            return;
        };
        if let Some(expected) = self.expected_of(place.type_id) {
            self.check_value(expected, value, place.other);
        }
    }

    /// [`Self::check_at`] for an expression that is a value of an
    /// enumeration; any other expression is not checked here.
    pub(super) fn check_expression_at(&mut self, position: Position<'_>, expr: &ExprKind) {
        if let ExprKind::EnumeratedValue(value) = expr {
            self.check_at(position, value);
        }
    }

    /// The type of the variable an assignment stores into.
    fn target_type(&self, target: &Variable) -> Option<TypeId> {
        use ironplc_dsl::textual::SymbolicVariableKind;
        let Variable::Symbolic(SymbolicVariableKind::Named(named)) = target else {
            return None;
        };
        self.declarations
            .find(&named.name)?
            .type_id(self.context.types())
    }
}

/// The type an expression of the type `expr` has, when it has a concrete one.
fn type_of_expr(expr: &Expr) -> Option<TypeId> {
    match expr.expr_type.as_ref()? {
        ExprType::Concrete(id) => Some(*id),
        ExprType::Literal(_) | ExprType::Null => None,
    }
}
