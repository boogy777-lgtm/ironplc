//! The operands of an arithmetic operator that are enumerations.
//!
//! The overloads are judged by the names of the operand types
//! ([`resolve_arithmetic_overload`]), and an enumeration has none that they
//! judge, so a pair that holds one is left unchecked there. An enumeration is
//! no number to an operator all the same: it is accepted where an integer is
//! stored or passed (see `value_type::check`), but its values are named, and
//! an operation on one has no meaning (CODESYS reports "arithmetic operations
//! with enumeration members" for an enumeration declared `strict`).

use ironplc_dsl::{
    common::TypeName,
    core::Located,
    diagnostic::{Diagnostic, Label},
    textual::{BinaryExpr, Expr, Function, ParamAssignmentKind, UnaryExpr, UnaryOp},
};
use ironplc_problems::Problem;

use super::RuleOperatorOperandTypeCheck;
use crate::intermediates::operator_function_form::{
    form_of_operator, FormOf, OperatorFunctionForm,
};

impl RuleOperatorOperandTypeCheck<'_> {
    /// Reports P4049 for each of `operands` that is an enumeration, where the
    /// operator named `operator` requires the category `required`.
    fn check_enumerated_operands(
        &mut self,
        operator: &str,
        required: &TypeName,
        operands: &[&Expr],
    ) {
        for operand in operands {
            let Some(representation) = self.types.representation_of_expr(operand) else {
                continue;
            };
            if !representation.is_enumeration() {
                continue;
            }
            let actual = self
                .operand_name(operand)
                .map_or_else(|| "an enumeration".to_owned(), |name| name.to_string());
            self.diagnostics.push(
                Diagnostic::problem(
                    Problem::OperatorOperandTypeMismatch,
                    Label::span(operand.span(), "Operand"),
                )
                .with_context("operator", &operator.to_string())
                .with_context("expected", &required.to_string())
                .with_context("actual", &actual),
            );
        }
    }

    /// The operands of an arithmetic operator expression.
    pub(super) fn check_enumerated_binary(&mut self, binary: &BinaryExpr) {
        let required = form_of_operator(&FormOf::Arithmetic(binary.op.clone()))
            .map_or_else(|| TypeName::from("ANY_NUM"), |form| form.operand_type());
        self.check_enumerated_operands(
            &binary.op.to_string(),
            &required,
            &[&binary.left, &binary.right],
        );
    }

    /// The inputs of a call to an arithmetic function (`ADD`, `SUB`, ...).
    pub(super) fn check_enumerated_inputs(
        &mut self,
        function: &Function,
        form: &OperatorFunctionForm,
    ) {
        let operands: Vec<&Expr> = function
            .param_assignment
            .iter()
            .filter_map(|p| match p {
                ParamAssignmentKind::PositionalInput(input) => Some(&input.expr),
                ParamAssignmentKind::NamedInput(_) | ParamAssignmentKind::Output(_) => None,
            })
            .collect();
        self.check_enumerated_operands(
            &function.name.original().to_string(),
            &form.operand_type(),
            &operands,
        );
    }

    /// The operand of a negation, which has no function form to hold it to but
    /// is an arithmetic operator for the numbers of `ANY_NUM`.
    pub(super) fn check_enumerated_negation(&mut self, unary: &UnaryExpr) {
        if unary.op == UnaryOp::Neg {
            self.check_enumerated_operands("-", &TypeName::from("ANY_NUM"), &[&unary.term]);
        }
    }
}
