//! Semantic rule that checks the operands of the CODESYS special operators
//! that are typed calls (`__NEW`, `__DELETE`, `__TYPEOF`, `__XADD`).
//!
//! The operators are registered as functions when `allow_special_operators`
//! is set, so the declared-call, argument-count and argument-type rules have
//! already seen them. This rule checks what a function signature cannot say:
//! a type where `__NEW` takes one, a pointer where `__DELETE` and `__XADD`
//! take one, and that the result of `__NEW` is assigned. A call that does not
//! fit is reported with P4073.
//!
//! ## Passes
//!
//! ```ignore
//! FUNCTION_BLOCK FB_Example
//! VAR
//!     p : POINTER TO INT;
//!     ok : BOOL;
//! END_VAR
//!     p := __NEW(INT);
//!     ok := __DELETE(p);
//! END_FUNCTION_BLOCK
//! ```
//!
//! ## Fails
//!
//! ```ignore
//! FUNCTION_BLOCK FB_Example
//! VAR
//!     n : INT;
//!     ok : BOOL;
//! END_VAR
//!     ok := __DELETE(n);
//! END_FUNCTION_BLOCK
//! ```
//!
//! `__CURRENTTASK`, `__SYSTEM` and `__POOL` are not calls; they are reported
//! as unsupported (P4074) where variable references are resolved, in
//! `rule_use_declared_symbolic_var`.

use std::convert::Infallible;

use ironplc_dsl::{
    common::*,
    core::{Located, SourceSpan},
    diagnostic::{Diagnostic, Label},
    textual::*,
    visitor::Visitor,
};
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::Problem;

use crate::{
    intermediates::special_operator::SpecialOperator,
    result::SemanticResult,
    rule_support::{run_rule, DiagnosticVisitor},
    semantic_context::SemanticContext,
    type_environment::{elementary_type, TypeEnvironment},
    value_type,
};

pub fn apply(
    lib: &Library,
    context: &SemanticContext,
    options: &CompilerOptions,
) -> SemanticResult {
    if !options.allow_special_operators {
        return Ok(());
    }
    run_rule(
        RuleSpecialOperator {
            types: context.types(),
            options: *options,
            assigned_new: Vec::new(),
            diagnostics: Vec::new(),
        },
        lib,
    )
}

struct RuleSpecialOperator<'a> {
    types: &'a TypeEnvironment,
    options: CompilerOptions,
    /// The names of the `__NEW` calls that are the value of an assignment.
    assigned_new: Vec<SourceSpan>,
    diagnostics: Vec<Diagnostic>,
}

impl RuleSpecialOperator<'_> {
    fn invalid(&mut self, operator: SpecialOperator, at: &impl Located, reason: &str) {
        self.diagnostics.push(
            Diagnostic::problem(
                Problem::SpecialOperatorOperandInvalid,
                Label::span(at.span(), reason.to_string()),
            )
            .with_context("operator", &operator.name().to_string()),
        );
    }

    /// The input arguments of `call`, in order.
    fn arguments(call: &Function) -> Vec<&Expr> {
        call.param_assignment
            .iter()
            .filter_map(|param| match param {
                ParamAssignmentKind::PositionalInput(input) => Some(&input.expr),
                ParamAssignmentKind::NamedInput(_) | ParamAssignmentKind::Output(_) => None,
            })
            .collect()
    }

    /// Whether `expr` has a resolved type that is a reference. `None` when
    /// the analyzer resolved no type, since another rule reports that.
    fn is_reference(&self, expr: &Expr) -> Option<bool> {
        match expr.expr_type.as_ref()? {
            ExprType::Concrete(_) => Some(
                self.types
                    .representation_of_expr(expr)
                    .is_some_and(|representation| representation.is_reference()),
            ),
            ExprType::Literal(_) | ExprType::Null => Some(false),
        }
    }

    fn check_new(&mut self, call: &Function, arguments: &[&Expr]) {
        let operator = SpecialOperator::New;
        if !self.assigned_new.contains(&call.name.span()) {
            self.invalid(operator, &call.name, "The result of __NEW must be assigned");
        }
        let Some(type_argument) = arguments.first() else {
            return;
        };
        let type_name = match &type_argument.kind {
            ExprKind::Variable(Variable::Symbolic(SymbolicVariableKind::Named(named))) => {
                Some(TypeName::from_id(&named.name))
            }
            _ => None,
        }
        .filter(|name| self.types.id_of(name).is_some());
        let Some(type_name) = type_name else {
            self.invalid(
                operator,
                *type_argument,
                "The first operand of __NEW must be a type",
            );
            return;
        };
        let Some(count) = arguments.get(1) else {
            return;
        };
        if elementary_type(&type_name).is_none() {
            self.invalid(
                operator,
                *count,
                "An array of a user-defined type cannot be created with __NEW",
            );
        }
        if value_type::check(self.types, &TypeName::from("ANY_INT"), count, &self.options).is_err()
        {
            self.invalid(operator, *count, "The element count must be an integer");
        }
    }

    fn check_delete(&mut self, arguments: &[&Expr]) {
        if let Some(operand) = arguments.first() {
            if self.is_reference(operand) == Some(false) {
                self.invalid(
                    SpecialOperator::Delete,
                    *operand,
                    "The operand of __DELETE must be a pointer",
                );
            }
        }
    }

    fn check_xadd(&mut self, arguments: &[&Expr]) {
        let Some(pointer) = arguments.first() else {
            return;
        };
        if pointer.expr_type.is_none() {
            return;
        }
        let dint = elementary_type(&TypeName::from("DINT"));
        let points_at_dint = self
            .types
            .representation_of_expr(pointer)
            .and_then(|representation| representation.referenced_type())
            .is_some_and(|target| Some(target) == dint);
        if !points_at_dint {
            self.invalid(
                SpecialOperator::XAdd,
                *pointer,
                "The first operand of __XADD must be a POINTER TO DINT",
            );
        }
    }
}

impl DiagnosticVisitor for RuleSpecialOperator<'_> {
    fn into_diagnostics(self) -> Vec<Diagnostic> {
        self.diagnostics
    }
}

impl Visitor<Infallible> for RuleSpecialOperator<'_> {
    type Value = ();

    fn visit_assignment(&mut self, node: &Assignment) -> Result<(), Infallible> {
        if let ExprKind::Function(call) = &node.value.kind {
            if SpecialOperator::of_call(&call.name, &self.options) == Some(SpecialOperator::New) {
                self.assigned_new.push(call.name.span());
            }
        }
        node.recurse_visit(self)
    }

    fn visit_function(&mut self, node: &Function) -> Result<(), Infallible> {
        if let Some(operator) = SpecialOperator::of_call(&node.name, &self.options) {
            let arguments = Self::arguments(node);
            match operator {
                SpecialOperator::New => self.check_new(node, &arguments),
                SpecialOperator::Delete => self.check_delete(&arguments),
                SpecialOperator::XAdd => self.check_xadd(&arguments),
                // Takes a type or an expression, and any is valid.
                SpecialOperator::TypeOf => {}
            }
        }
        node.recurse_visit(self)
    }
}
