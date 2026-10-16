//! Semantic rule that references to enumerations use enumeration values
//! that are part of the enumeration declaration.
//!
//! ## Passes
//!
//! ```ignore
//! TYPE
//!    LEVEL : (CRITICAL) := CRITICAL;
//! END_TYPE
//!
//! FUNCTION_BLOCK LOGGER
//!    VAR_INPUT
//!       LEVEL : LEVEL := CRITICAL;
//!    END_VAR
//! END_FUNCTION_BLOCK
//! ```
//!
//! ## Fails
//!
//! ```ignore
//! TYPE
//!    LEVEL : (INFO) := INFO;
//! END_TYPE
//!
//! FUNCTION_BLOCK LOGGER
//!    VAR_INPUT
//!       LEVEL : LEVEL := CRITICAL;
//!    END_VAR
//! END_FUNCTION_BLOCK
//! ```
//!
//! ## A value written with its type
//!
//! `Type#Value` names the enumeration it belongs to, wherever it is written: an
//! initial value, an element of an array or a structure, a `CASE` label, an
//! operand of an expression, an argument of a call. The type before the `#` must
//! be declared (P2008), must be an enumeration (P2004) and must declare the value
//! (P2006). The rule looks at the value and not at the place it stands, as
//! `rule_ambiguous_enumerated_value` does.
//!
//! ## A value where an enumeration is expected
//!
//! The target of an assignment, a parameter, the selector of a `CASE` and the
//! other operand of a comparison each expect an enumeration, and the value
//! written there is one of it (see the `expected` module). A name that no
//! enumeration lists is no value, and is reported once wherever it stands (P4007,
//! as for a variable that is not declared). A name of another enumeration is
//! P2006, and a value written with the type of another enumeration is P4035
//! (P4026 for an argument, which types its operands).
//!
//! ```ignore
//! TYPE LEVEL : (INFO, WARN); END_TYPE
//!
//! PROGRAM main
//!    VAR l : LEVEL; END_VAR
//!    l := LEVEL#INFO;     (* passes *)
//!    l := LEVEL#BLAH;     (* P2006: BLAH is not a value of LEVEL *)
//!    l := NOWHERE#INFO;   (* P2008: NOWHERE is not declared *)
//! END_PROGRAM
//! ```
use ironplc_dsl::{
    common::*,
    core::{Id, Located},
    diagnostic::Diagnostic,
    scope::ScopeNode,
    textual::{Assignment, Case, CaseSelectionKind, CompareExpr, CompareOp, Function},
    visitor::Visitor,
};
use ironplc_problems::text;
use std::collections::HashSet;
use std::convert::Infallible;

mod expected;
mod position;

use self::position::Position;
use crate::{
    resolution::Resolved,
    result::SemanticResult,
    rule_support::{run_rule, DiagnosticVisitor},
    semantic_context::SemanticContext,
    variable_type::{enter_declaration_scope, Declarations, Declared},
};
use ironplc_parser::options::CompilerOptions;

pub fn apply(
    lib: &Library,
    context: &SemanticContext,
    _options: &CompilerOptions,
) -> SemanticResult {
    // Walk the library to find all references to enumerations
    // checking that all references use an enumeration value
    // that is part of the enumeration
    run_rule(RuleDeclaredEnumeratedValues::new(context, lib), lib)
}

struct RuleDeclaredEnumeratedValues<'a> {
    context: &'a SemanticContext,
    diagnostics: Vec<Diagnostic>,
    /// The values the enumerations of the library list.
    listed_values: HashSet<Id>,
    /// The declared type of every variable in scope.
    declarations: Declarations<'static>,
}

impl<'a> RuleDeclaredEnumeratedValues<'a> {
    fn new(context: &'a SemanticContext, lib: &Library) -> Self {
        let mut listed = ListedValues::default();
        let Ok(()) = listed.walk(lib);
        RuleDeclaredEnumeratedValues {
            context,
            diagnostics: Vec::new(),
            listed_values: listed.0,
            declarations: Declarations::new(),
        }
    }

    /// Returns the values of the enumeration a type name names.
    ///
    /// The type environment holds the values with the type, so an alias of an
    /// enumeration answers the values of the enumeration it names.
    ///
    /// # Errors
    ///
    /// Returns a diagnostic if the type name does not name an enumeration.
    fn find_enum_declaration_values(&self, type_name: &TypeName) -> Result<&'a [Id], Diagnostic> {
        self.context
            .types()
            .enumerated_values(type_name)
            .ok_or_else(|| {
                Diagnostic::text(
                    type_name.span(),
                    text::enum_not_declared_type_is_not_an_enumeration(),
                )
            })
    }

    /// Reports what is wrong with a value written `Type#Value`: the type is
    /// not declared, is not an enumeration, or does not declare the value.
    /// Returns whether it reported. A value written without its type, and a
    /// type that is declared with an error (reported where it is declared),
    /// have nothing to check here.
    fn report_qualified_value(&mut self, value: &EnumeratedValue) -> bool {
        let Some(type_name) = &value.type_name else {
            return false;
        };
        match self.context.types().lookup(type_name) {
            Resolved::Failed => return false,
            Resolved::Absent => {
                self.diagnostics.push(Diagnostic::text(
                    type_name.span(),
                    text::undeclared_unknown_type_enumeration_value_type(),
                ));
                return true;
            }
            Resolved::Valid(_) => {}
        }
        match self.find_enum_declaration_values(type_name) {
            Err(diagnostic) => {
                self.diagnostics.push(diagnostic);
                true
            }
            Ok(defined_values) if !defined_values.contains(&value.value) => {
                self.diagnostics.push(self.value_not_defined(value));
                true
            }
            Ok(_) => false,
        }
    }

    fn value_not_defined(&self, value: &EnumeratedValue) -> Diagnostic {
        Diagnostic::text(
            value.span(),
            text::enum_value_not_defined_expected_value_in_enumeration(),
        )
        .with_context_id("value", &value.value)
    }
}

/// The values the enumerations of a library list, whether the type
/// environment holds the enumeration or not: one spelled out where a variable
/// is declared has no name there, and one whose declaration has an error is
/// reported where it is declared.
#[derive(Default)]
struct ListedValues(HashSet<Id>);

impl Visitor<Infallible> for ListedValues {
    type Value = ();

    fn visit_enumerated_specification_values(
        &mut self,
        node: &EnumeratedSpecificationValues,
    ) -> Result<(), Infallible> {
        self.0
            .extend(node.values.iter().map(|value| value.value.clone()));
        Ok(())
    }

    fn visit_enumerated_values_initializer(
        &mut self,
        node: &EnumeratedValuesInitializer,
    ) -> Result<(), Infallible> {
        self.0
            .extend(node.values.iter().map(|value| value.value.clone()));
        Ok(())
    }
}

impl DiagnosticVisitor for RuleDeclaredEnumeratedValues<'_> {
    fn into_diagnostics(self) -> Vec<Diagnostic> {
        self.diagnostics
    }
}

impl Visitor<Infallible> for RuleDeclaredEnumeratedValues<'_> {
    type Value = ();

    fn visit_enumerated_initial_value_assignment(
        &mut self,
        init: &EnumeratedInitialValueAssignment,
    ) -> Result<Self::Value, Infallible> {
        // A declaration with an error has no values to check this one
        // against, and is reported where it is declared.
        if self.context.types().lookup(&init.type_name).is_failed() {
            return Ok(());
        }
        let defined_values = match self.find_enum_declaration_values(&init.type_name) {
            Ok(values) => values,
            Err(diagnostic) => {
                // The type is not an enumeration, so there is nothing to check
                // this initializer's value against. Report that and carry on to
                // the next declaration.
                self.diagnostics.push(diagnostic);
                return Ok(());
            }
        };
        if let Some(value) = &init.initial_value {
            // The walk does not go on into the value, so a value written with
            // its type is checked here. Its own enumeration is reported once,
            // and is not checked again against the type of the variable.
            if self.report_qualified_value(value) {
                return Ok(());
            }
            // Check if the value is in the list of defined enumeration values
            if value.type_name.is_some() {
                self.check_at(Position::InitialValue(&init.type_name), value);
            } else if !defined_values.contains(&value.value) {
                self.diagnostics.push(self.value_not_defined(value));
            }
        }

        Ok(())
    }

    fn visit_enumerated_value(&mut self, node: &EnumeratedValue) -> Result<(), Infallible> {
        if !self.report_qualified_value(node) {
            self.report_unknown_value(node);
        }
        Ok(())
    }

    /// The default of an enumeration is a value of the enumeration it is
    /// declared with, and its declaration is where a fault in it is reported.
    /// Only the type written with it is checked here.
    fn visit_enumerated_default(&mut self, node: &EnumeratedDefault) -> Result<(), Infallible> {
        if let Some(value) = node.as_value() {
            self.report_qualified_value(value);
        }
        Ok(())
    }

    fn enter_scope(&mut self, node: ScopeNode<'_>) -> Result<(), Infallible> {
        enter_declaration_scope(&mut self.declarations, node);
        Ok(())
    }

    fn exit_scope(&mut self) {
        self.declarations.exit();
    }

    fn visit_var_decl(&mut self, node: &VarDecl) -> Result<(), Infallible> {
        if let VariableIdentifier::Symbol(id) = &node.identifier {
            self.declarations.add(id, Declared::of(node));
        }
        node.recurse_visit(self)
    }

    /// The target of an assignment expects its own enumeration.
    fn visit_assignment(&mut self, node: &Assignment) -> Result<Self::Value, Infallible> {
        self.check_expression_at(Position::AssignmentTarget(&node.target), &node.value.kind);
        node.recurse_visit(self)
    }

    /// The parameter of a function expects its own enumeration.
    fn visit_function(&mut self, node: &Function) -> Result<Self::Value, Infallible> {
        if let Some(signature) = self.context.functions.get(&node.name) {
            for (parameter, argument) in signature.bind_inputs(&node.param_assignment) {
                self.check_expression_at(
                    Position::FunctionInput(&parameter.param_type),
                    &argument.kind,
                );
            }
        }
        node.recurse_visit(self)
    }

    /// A label of a `CASE` is a value of the enumeration of the selector.
    fn visit_case(&mut self, node: &Case) -> Result<(), Infallible> {
        for selector in node
            .statement_groups
            .iter()
            .flat_map(|group| &group.selectors)
        {
            if let CaseSelectionKind::EnumeratedValue(value) = selector {
                self.check_at(Position::CaseLabel(&node.selector), value);
            }
        }
        node.recurse_visit(self)
    }

    /// A value compared with an enumeration is a value of it.
    fn visit_compare_expr(&mut self, node: &CompareExpr) -> Result<(), Infallible> {
        if matches!(
            node.op,
            CompareOp::Eq
                | CompareOp::Ne
                | CompareOp::Lt
                | CompareOp::Gt
                | CompareOp::LtEq
                | CompareOp::GtEq
        ) {
            for (value, other) in [(&node.left, &node.right), (&node.right, &node.left)] {
                self.check_expression_at(Position::ComparedWith(other), &value.kind);
            }
        }
        node.recurse_visit(self)
    }
}

#[cfg(test)]
mod tests;
