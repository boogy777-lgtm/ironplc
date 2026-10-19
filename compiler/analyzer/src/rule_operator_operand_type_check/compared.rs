//! The operands of a comparison that are enumerations of different types.
//!
//! A comparison sets two values against each other, and two values of two
//! different enumerations have nothing in common but their numbers. The
//! program is accepted whether the enumerations are strict or not (a
//! comparison only holds its operands, see `value_type::ValueUse`), and CODESYS
//! reports it as the warning C0354 `Wrn_EnumComparison`
//! (`TypeCheckerVisitor.cs`, the comparison operators and `MIN` and `MAX`):
//! P4083.
//!
//! The places that compare are the ones that declare it: the relational
//! operators ([`ValueUse::of_compare`]), their function forms and the
//! functions whose row says `comparing` (`MIN`, `MAX`). Both ask the one
//! [`ValueUse::Compares`], so a new place that compares is a new row and not a
//! new branch here.

use ironplc_dsl::{
    core::{Located, SourceSpan},
    diagnostic::Diagnostic,
    textual::{CompareExpr, Expr, ExprType, Function, ParamAssignmentKind},
    type_id::TypeId,
};
use ironplc_problems::text;

use super::RuleOperatorOperandTypeCheck;
use crate::value_type::{describe, ValueUse};

impl RuleOperatorOperandTypeCheck<'_> {
    /// The enumeration an operand is a value of, when it is one: the
    /// declaration of the enumeration (the one an alias names) and the type
    /// as the operand is declared with, for the message. A reference to an
    /// enumeration, an integer and an untyped literal are not.
    fn enumeration_of(&self, operand: &Expr) -> Option<(TypeId, TypeId)> {
        let Some(ExprType::Concrete(declared)) = operand.expr_type else {
            return None;
        };
        let declaration = self.types.enumeration_declaration(declared)?;
        Some((declaration, declared))
    }

    /// Reports P4083 at `span` when `left` and `right` are values of two
    /// different enumerations.
    fn check_compared_pair(&mut self, span: SourceSpan, left: &Expr, right: &Expr) {
        let (Some((left_declaration, left_declared)), Some((right_declaration, right_declared))) =
            (self.enumeration_of(left), self.enumeration_of(right))
        else {
            return;
        };
        if left_declaration == right_declaration {
            return;
        }
        self.diagnostics.push(Diagnostic::text(
            span,
            text::enumeration_types_compared(
                describe(self.types, left_declared),
                describe(self.types, right_declared),
            ),
        ));
    }

    /// The comparison operators: `=`, `<>`, `<`, `<=`, `>` and `>=`.
    pub(super) fn check_compared_operator(&mut self, expr: &Expr, compare: &CompareExpr) {
        if ValueUse::of_compare(&compare.op) == ValueUse::Compares {
            self.check_compared_pair(expr.span(), &compare.left, &compare.right);
        }
    }

    /// A call to a function that compares its two inputs: the function forms
    /// of the comparison operators, `MIN` and `MAX`.
    pub(super) fn check_compared_call(&mut self, expr: &Expr, function: &Function) {
        let compares = self
            .functions
            .get(&function.name)
            .is_some_and(|signature| signature.value_use == ValueUse::Compares);
        if !compares {
            return;
        }
        let inputs: Vec<&Expr> = function
            .param_assignment
            .iter()
            .filter_map(|param| match param {
                ParamAssignmentKind::PositionalInput(input) => Some(&input.expr),
                ParamAssignmentKind::NamedInput(_) | ParamAssignmentKind::Output(_) => None,
            })
            .collect();
        if let [left, right] = inputs[..] {
            self.check_compared_pair(expr.span(), left, right);
        }
    }
}
