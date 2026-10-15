//! Semantic rule that validates function call argument types match parameter
//! types, function return types match assignment destinations, and assignment
//! statement values match their target variable types.
//!
//! Both user-defined and standard-library function calls are checked. Standard
//! library parameters use the IEC 61131-3 generic categories (ANY_REAL, ANY_NUM,
//! etc.) or concrete types (for the `<SOURCE>_TO_<TARGET>` conversion functions);
//! [`are_types_compatible`] handles both.
//!
//! ## Passes
//!
//! ```ignore
//! FUNCTION ADD_INTS : INT
//! VAR_INPUT
//!     A : INT;
//!     B : INT;
//! END_VAR
//!     ADD_INTS := A + B;
//! END_FUNCTION
//!
//! PROGRAM main
//! VAR
//!     result : INT;
//! END_VAR
//!     result := ADD_INTS(1, 2);
//! END_PROGRAM
//! ```
//!
//! ## Fails (Argument Type Mismatch)
//!
//! ```ignore
//! FUNCTION ADD_REALS : REAL
//! VAR_INPUT
//!     A : REAL;
//! END_VAR
//!     ADD_REALS := A;
//! END_FUNCTION
//!
//! PROGRAM main
//! VAR
//!     result : REAL;
//!     x : DINT;
//! END_VAR
//!     result := ADD_REALS(x);
//! END_PROGRAM
//! ```

use ironplc_dsl::{
    common::*,
    core::{Id, Located},
    diagnostic::{Diagnostic, Label},
    scope::ScopeNode,
    textual::*,
    visitor::Visitor,
};
use ironplc_problems::Problem;
use std::convert::Infallible;

use crate::{
    intermediates::operator_function_form::operator_function_form,
    result::SemanticResult,
    rule_support::{run_rule, DiagnosticVisitor},
    semantic_context::SemanticContext,
    type_compat::is_checkable_type,
    value_type::{self, ValueType},
    variable_type::{Declarations, Declared},
};
use ironplc_parser::options::CompilerOptions;
pub fn apply(
    lib: &Library,
    context: &SemanticContext,
    options: &CompilerOptions,
) -> SemanticResult {
    run_rule(
        RuleFunctionCallTypeCheck {
            context,
            options,
            diagnostics: vec![],
            declarations: Declarations::new(),
        },
        lib,
    )
}

struct RuleFunctionCallTypeCheck<'a> {
    context: &'a SemanticContext,
    options: &'a CompilerOptions,
    diagnostics: Vec<Diagnostic>,
    /// Declared type of every variable in scope.
    ///
    /// Each declaration the traversal enters pushes a frame, so a
    /// method's locals do not outlive the method and a local shadows a
    /// field of the same name only within its own body.
    declarations: Declarations<'static>,
}

impl DiagnosticVisitor for RuleFunctionCallTypeCheck<'_> {
    fn into_diagnostics(self) -> Vec<Diagnostic> {
        self.diagnostics
    }
}

impl RuleFunctionCallTypeCheck<'_> {
    /// The type name a variable in scope was declared with, or `None` for
    /// one declared with an inline type or not declared at all.
    fn declared_type_name(&self, id: &Id) -> Option<TypeName> {
        match self.declarations.find(id)?.type_reference() {
            TypeReference::Named(type_name) => Some(type_name),
            TypeReference::Inline | TypeReference::Unspecified => None,
        }
    }

    /// Checks whether a function call expression assigned to a variable has a
    /// matching return type. Emits P4027 if there is a mismatch.
    ///
    /// Standard-library calls are checked like any other. Their declared
    /// return type may be a generic category, but `xform_resolve_expr_types`
    /// has already narrowed it to the concrete type of the argument the
    /// category binds to; where it could not, `expr_type` is `None` and
    /// the call is skipped below. A call naming a function the environment
    /// does not hold resolves to `None` the same way, so the signature
    /// itself is never needed here.
    fn check_return_type(&mut self, target: &Variable, value: &Expr) {
        let ExprKind::Function(ref func_call) = value.kind else {
            return;
        };
        let Variable::Symbolic(SymbolicVariableKind::Named(ref nv)) = target else {
            return;
        };
        let Some(target_type) = self.declared_type_name(&nv.name) else {
            return;
        };

        if let Err(mismatch) =
            value_type::check(self.context.types(), &target_type, value, self.options)
        {
            self.diagnostics.push(
                Diagnostic::problem(
                    Problem::FunctionCallReturnTypeMismatch,
                    Label::span(func_call.name.span(), "Function call return type"),
                )
                .with_context("function", &func_call.name.original().to_string())
                .with_context("return_type", &mismatch.actual)
                .with_context("target_type", &target_type.to_string()),
            );
        }
    }

    /// Checks whether the value assigned in an assignment statement is
    /// type-compatible with the target variable. Emits P4035 on a mismatch.
    ///
    /// This complements [`Self::check_return_type`], which handles the case where
    /// the right-hand side is a user-function call. Here we handle every other
    /// right-hand side (arithmetic, variables, literals, stdlib calls) by
    /// comparing the target's declared type against the resolved expression type.
    /// Only simple named targets of a type the relation judges
    /// ([`value_type::judges`]: an elementary type or an enumeration) are
    /// checked; the other user-defined targets (structures, arrays, function
    /// blocks) are skipped to avoid false positives.
    fn check_assignment_type(&mut self, target: &Variable, value: &Expr) {
        // Function-call right-hand sides are validated by `check_return_type`.
        if matches!(value.kind, ExprKind::Function(_)) {
            return;
        }

        let Variable::Symbolic(SymbolicVariableKind::Named(nv)) = target else {
            return;
        };
        let Some(declared) = self.declared_type_name(&nv.name) else {
            return;
        };
        // Resolve aliases/subranges to the underlying elementary type so the
        // comparison matches the already-resolved right-hand side type.
        let target_type = self
            .context
            .types()
            .resolve_elementary_type_name(&declared)
            .unwrap_or(declared);
        if !value_type::judges(self.context.types(), &target_type) {
            return;
        }

        // A scalar value the compatibility relation cannot judge (a
        // reference, a sized string) is skipped; a composite one is never
        // assignable to an elementary target.
        let types = self.context.types();
        let checkable = match value_type::of(types, value) {
            None => false,
            Some(ValueType::Scalar(value_type)) => is_checkable_type(&value_type),
            Some(ValueType::Composite(_)) => true,
        };
        if !checkable {
            return;
        }

        if let Err(mismatch) = value_type::check(types, &target_type, value, self.options) {
            self.diagnostics.push(
                Diagnostic::problem(
                    Problem::AssignmentTypeMismatch,
                    Label::span(value.span(), "Assignment value"),
                )
                .with_context("target", &nv.name.original().to_string())
                .with_context("target_type", &target_type.to_string())
                .with_context("value_type", &mismatch.actual),
            );
        }
    }
}

impl Visitor<Infallible> for RuleFunctionCallTypeCheck<'_> {
    type Value = ();

    /// Opens a declaration's scope.
    ///
    /// Replaces the per-POU `clear()` this rule used to do, which could
    /// not express a method: clearing at a method boundary would discard
    /// the enclosing function block's fields, and not clearing left a
    /// method's locals shadowing those fields for every later method.
    fn enter_scope(&mut self, node: ScopeNode<'_>) -> Result<(), Infallible> {
        self.declarations.enter();

        // A declaration's own name is its result variable, so assigning
        // it is an assignment with a target type like any other. Without
        // this the target lookup missed and the check returned early,
        // leaving `Foo := <wrong type>` unreported -- for a FUNCTION as
        // much as for a METHOD.
        match node {
            ScopeNode::Function(node) => {
                self.declarations
                    .add(&node.name, Declared::Typed(node.return_type.to_type_name()));
            }
            // Only a method that declares a return type has a result to
            // assign; `rule_use_declared_symbolic_var` rejects the
            // assignment outright for one that does not.
            ScopeNode::Method(node) => {
                if let Some(return_type) = &node.return_type {
                    self.declarations
                        .add(&node.name, Declared::Typed(return_type.to_type_name()));
                }
            }
            // Neither has a result variable.
            ScopeNode::FunctionBlock(_) | ScopeNode::Program(_) => {}
        }

        Ok(())
    }

    fn exit_scope(&mut self) {
        self.declarations.exit();
    }

    fn visit_var_decl(&mut self, node: &VarDecl) -> Result<Self::Value, Infallible> {
        if let VariableIdentifier::Symbol(ref id) = node.identifier {
            self.declarations.add(id, Declared::of(node));
        }
        node.recurse_visit(self)
    }

    fn visit_assignment(&mut self, node: &Assignment) -> Result<Self::Value, Infallible> {
        self.check_return_type(&node.target, &node.value);
        self.check_assignment_type(&node.target, &node.value);
        node.recurse_visit(self)
    }

    fn visit_function(&mut self, node: &Function) -> Result<Self::Value, Infallible> {
        let func_sig = self.context.functions.get(&node.name);

        if let Some(signature) = func_sig {
            // Emit NotImplemented for output arguments on user-defined functions.
            // Standard-library functions do not take output arguments.
            if !signature.is_stdlib() {
                for p in &node.param_assignment {
                    if let ParamAssignmentKind::Output(_) = p {
                        self.diagnostics
                            .push(Diagnostic::not_implemented(Label::span(
                                node.name.span(),
                                "Function call with output argument",
                            )));
                    }
                }
            }

            // Check each positional argument type against the parameter type.
            // Standard-library functions are checked too: their parameters use
            // generic ANY_* categories (or concrete types for the conversion
            // functions), all handled by `are_types_compatible`. The parameter
            // list continues past the declared ones for an extensible
            // function, so every input of `AND(a, b, c)` is checked. A
            // VAR_IN_OUT argument must match exactly, not just be compatible;
            // `rule_function_call_in_out_argument` checks it.
            //
            // `ADD`, `SUB`, `MUL` and `DIV` are the exception. Their inputs
            // are checked against every overload (the numeric one and the
            // typed ones on the time and date types) by the operator rule,
            // which reports a mismatch as P4049; their `ANY_NUM` signature
            // states only the numeric overload.
            let overloaded = operator_function_form(&node.name.to_string())
                .is_some_and(|form| !form.typed_overloads().is_empty());
            let inputs = signature
                .bind_inputs(&node.param_assignment)
                .filter(|_| !overloaded);
            for (param, arg_expr) in inputs {
                if param.is_inout {
                    continue;
                }
                if let Err(mismatch) = value_type::check(
                    self.context.types(),
                    &param.param_type,
                    arg_expr,
                    self.options,
                ) {
                    self.diagnostics.push(
                        Diagnostic::problem(
                            Problem::FunctionCallArgTypeMismatch,
                            Label::span(node.name.span(), "Function call"),
                        )
                        .with_context("function", &node.name.original().to_string())
                        .with_context("parameter", &param.name.original().to_string())
                        .with_context("expected", &param.param_type.to_string())
                        .with_context("actual", &mismatch.actual),
                    );
                }
            }
        }

        node.recurse_visit(self)
    }
}

#[cfg(test)]
mod composite_tests;

#[cfg(test)]
mod tests;
