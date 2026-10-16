//! The places that expect an enumeration, and the one question each is asked.
//!
//! A value written where the language fixes a type has to be a value of that
//! type. [`Position`] lists those places, and
//! [`RuleDeclaredEnumeratedValues::place_of`] answers for each the type it
//! fixes. The answer is one `match` over the places with no wildcard arm, so a
//! place added to the list does not compile until it is classified, and every
//! visitor of the rule asks that one match and no other code for the type it
//! expects.
//!
//! The places that are not in the list have no type the language fixes: an
//! operand of arithmetic, an argument of a function that accepts any type.
//! A value written there is not checked against an enumeration.

use ironplc_dsl::{
    common::{EnumeratedValue, TypeName, VarDecl},
    core::Id,
    textual::{Expr, ExprKind, ExprType, SymbolicVariableKind, Variable},
    type_id::TypeId,
};

use super::expected::OtherEnumeration;
use super::RuleDeclaredEnumeratedValues;
use crate::type_attributes::PartType;

/// A place that expects a value of some type.
#[derive(Clone, Copy)]
pub(super) enum Position<'p> {
    /// The variable an assignment stores into.
    AssignmentTarget(&'p Variable),
    /// A parameter of a function, as the signature declares its type.
    FunctionInput(&'p TypeName),
    /// An input of a function block or a method, as the callee declares it.
    CallInput(&'p VarDecl),
    /// The initial value of a declaration of the type.
    InitialValue(&'p TypeName),
    /// The value a structure or function block initializer gives a member.
    Member { owner: TypeId, name: &'p Id },
    /// A value an array initializer lists, for an array whose elements are of
    /// the type.
    ListedElement(TypeId),
    /// A bound or the step of a `FOR`, whose control variable is named.
    ForBound(&'p Id),
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

impl Place {
    /// A place whose type this rule alone compares a value of another
    /// enumeration with.
    fn reported_here(type_id: TypeId) -> Self {
        Place {
            type_id,
            other: OtherEnumeration::Report,
        }
    }

    /// A place whose type the check of the types of the operands compares with
    /// the type of the value.
    fn reported_by_types(type_id: TypeId) -> Self {
        Place {
            type_id,
            other: OtherEnumeration::ReportedByTypes,
        }
    }
}

impl RuleDeclaredEnumeratedValues<'_> {
    /// The type the language fixes at `position`, or `None` when it fixes none
    /// that is known: a declaration with an error, a name not declared.
    pub(super) fn place_of(&self, position: Position<'_>) -> Option<Place> {
        let types = self.context.types();
        match position {
            // The check of an assignment to a variable compares the type of
            // the value with the variable's; it does not judge a member of a
            // structure or an element of an array.
            Position::AssignmentTarget(target) => {
                let type_id = self.target_type(target)?;
                Some(match target {
                    Variable::Symbolic(SymbolicVariableKind::Named(_)) => {
                        Place::reported_by_types(type_id)
                    }
                    _ => Place::reported_here(type_id),
                })
            }
            // The check of the arguments of a function types its operands.
            Position::FunctionInput(type_name) => {
                types.id_of(type_name).map(Place::reported_by_types)
            }
            // Nothing compares the type of an argument of a function block or
            // a method with its input.
            Position::CallInput(input) => input.type_id.map(Place::reported_here),
            Position::InitialValue(type_name) => types.id_of(type_name).map(Place::reported_here),
            Position::Member { owner, name } => match self.member_part(owner, name)? {
                PartType::Of(type_id) => Some(Place::reported_here(type_id)),
                // An array is no enumeration; its elements are its own
                // position.
                PartType::ArrayOf { .. } => None,
            },
            Position::ListedElement(element) => {
                Some(Place::reported_here(types.listed_element(element)))
            }
            Position::ForBound(control) => self
                .declarations
                .find(control)?
                .type_id(types)
                .map(Place::reported_here),
            Position::CaseLabel(selector) => type_of_expr(selector).map(Place::reported_here),
            Position::ComparedWith(other) => type_of_expr(other).map(Place::reported_here),
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
        match self.variable_part(target)? {
            PartType::Of(type_id) => Some(type_id),
            PartType::ArrayOf { .. } => None,
        }
    }

    /// The declared type of the part of a type that `owner` has under the
    /// name `name`: a member of a structure from the type environment, a
    /// variable of a function block from its declaration.
    pub(super) fn member_part(&self, owner: TypeId, name: &Id) -> Option<PartType> {
        let types = self.context.types();
        if let Some(part) = types.member_part(owner, name) {
            return Some(part);
        }
        let block = self
            .function_blocks
            .declaring_block(types.name_of(owner)?, name)?;
        block
            .variables
            .iter()
            .find(|declared| declared.identifier.symbolic_id() == Some(name))?
            .type_id
            .map(PartType::Of)
    }

    /// The declared type of what a variable names: the variable in scope, a
    /// member of a structure, an element of an array.
    pub(super) fn variable_part(&self, variable: &Variable) -> Option<PartType> {
        let Variable::Symbolic(kind) = variable else {
            return None;
        };
        self.symbolic_part(kind)
    }

    fn symbolic_part(&self, kind: &SymbolicVariableKind) -> Option<PartType> {
        let types = self.context.types();
        match kind {
            SymbolicVariableKind::Named(named) => self
                .declarations
                .find(&named.name)?
                .type_id(types)
                .map(PartType::Of),
            SymbolicVariableKind::Structured(structured) => {
                match self.symbolic_part(&structured.record)? {
                    PartType::Of(owner) => self.member_part(owner, &structured.field),
                    PartType::ArrayOf { .. } => None,
                }
            }
            SymbolicVariableKind::Array(array) => {
                let array_part = self.symbolic_part(&array.subscripted_variable)?;
                types
                    .subscripted_part(array_part, array.subscripts.len())
                    .map(PartType::Of)
            }
            // A bit or a part of a word is a number and not a value of an
            // enumeration; a reference is followed by no rule yet.
            SymbolicVariableKind::BitAccess(_)
            | SymbolicVariableKind::PartialAccess(_)
            | SymbolicVariableKind::SelfRef(_)
            | SymbolicVariableKind::Deref(_) => None,
        }
    }
}

/// The type an expression has, when it has a concrete one.
fn type_of_expr(expr: &Expr) -> Option<TypeId> {
    match expr.expr_type.as_ref()? {
        ExprType::Concrete(id) => Some(*id),
        ExprType::Literal(_) | ExprType::Null => None,
    }
}
