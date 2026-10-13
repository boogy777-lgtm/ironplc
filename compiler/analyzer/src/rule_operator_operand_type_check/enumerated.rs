//! The operands of an arithmetic operator that are enumerations.
//!
//! The overloads are judged by the names of the operand types
//! ([`resolve_arithmetic_overload`]), and an enumeration has none that they
//! judge, so a pair that holds one is left unchecked there. An enumeration is
//! no number to an operator all the same: it is accepted where an integer is
//! stored or passed (see `value_type::check`), but its values are named, and
//! an operation on one has no meaning (CODESYS reports "arithmetic operations
//! with enumeration members" for an enumeration declared `strict`).
//!
//! Whether an enumeration is a number to a place that computes is answered by
//! [`ValueUse::refuses`], the relation every computing place asks: the
//! numerical functions and the `FOR` counter ask it too, and report in their
//! own rule.

use ironplc_dsl::{
    common::TypeName,
    core::{Located, SourceSpan},
    diagnostic::Diagnostic,
    textual::{BinaryExpr, Expr, For, Function, ParamAssignmentKind, UnaryExpr, UnaryOp},
};
use ironplc_problems::{text, Text};

use super::RuleOperatorOperandTypeCheck;
use crate::intermediates::operator_function_form::{
    form_of_operator, FormOf, OperatorFunctionForm,
};
use crate::value_type::{describe, ValueUse};

impl RuleOperatorOperandTypeCheck<'_> {
    /// Reports P4049 for the value `actual` at `span`, an enumeration where
    /// the operator named `operator` requires the category `required`.
    fn report_enumerated(
        &mut self,
        span: SourceSpan,
        label: Text,
        operator: &str,
        required: &TypeName,
        actual: &str,
    ) {
        self.diagnostics.push(
            Diagnostic::text(span, label)
                .with_context("operator", &operator.to_owned())
                .with_context("expected", &required.to_string())
                .with_context("actual", &actual.to_owned()),
        );
    }

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
            if !ValueUse::Computes.refuses(representation) {
                continue;
            }
            let actual = self
                .operand_name(operand)
                .map_or_else(|| "an enumeration".to_owned(), |name| name.to_string());
            self.report_enumerated(
                operand.span(),
                text::operator_operand_type_mismatch_operand(),
                operator,
                required,
                &actual,
            );
        }
    }

    /// Reports P4049 when the control variable of a `FOR` is an enumeration:
    /// the statement adds the step to it on every pass, so it computes with it
    /// as an arithmetic operator does. The bounds and the step are only held
    /// by the statement, and are not checked here.
    pub(super) fn check_enumerated_counter(&mut self, statement: &For) {
        let control = &statement.control;
        let Some(id) = self
            .declarations
            .find(control)
            .and_then(|declared| declared.type_id(self.types))
        else {
            return;
        };
        let Some(attributes) = self.types.get_by_id(id) else {
            return;
        };
        if !ValueUse::Computes.refuses(&attributes.representation) {
            return;
        }
        let actual = describe(self.types, id);
        self.report_enumerated(
            control.span(),
            text::operator_operand_type_mismatch_control_variable(),
            "FOR",
            &TypeName::from("ANY_INT"),
            &actual,
        );
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
