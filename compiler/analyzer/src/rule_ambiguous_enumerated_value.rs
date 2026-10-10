//! Semantic rule that a bare enumeration value name is a value of one
//! enumeration.
//!
//! Two enumerations may declare the same value name, and a bare value name
//! (`U1`) is spelled the same whichever enumeration it belongs to. Where the
//! name is used, nothing in the program says which enumeration is meant, so
//! code generation could only guess. This rule reports every use of a name
//! that more than one enumeration declares. Which enumerations declare a name
//! is the type environment's answer ([`TypeEnvironment::enumerations_declaring`]),
//! in which an alias of an enumeration is that enumeration.
//!
//! The rule looks at the value node and not at the place it stands, so one
//! visit covers every place the language lets a value stand: the initial value
//! of a variable, of a structure member, of an array element or of a function
//! block input, an assignment, a comparison or any other expression, a `CASE`
//! label, an argument of a call, and the default value of a type.
//!
//! A qualified value (`E2#U1`) names its enumeration and is never ambiguous.
//! The values listed by a declaration, and the default stated beside the list,
//! belong to the enumeration being declared, so they are not uses either.
//! Declaring a name that another enumeration also declares is not an error.
//!
//! See section 2.3.3.1.
//!
//! ## Passes
//!
//! ```ignore
//! TYPE
//!    E1 : (U2, U1);
//!    E2 : (U1, U3);
//! END_TYPE
//!
//! PROGRAM main
//!    VAR
//!       a : E1 := U2;   (* U2 is declared by E1 only *)
//!       b : E2 := E2#U1;
//!    END_VAR
//! END_PROGRAM
//! ```
//!
//! ## Fails
//!
//! ```ignore
//! TYPE
//!    E1 : (U2, U1);
//!    E2 : (U1, U3);
//! END_TYPE
//!
//! PROGRAM main
//!    VAR
//!       b : E2 := U1;   (* U1 is declared by E1 and by E2 *)
//!    END_VAR
//! END_PROGRAM
//! ```
use ironplc_dsl::{
    common::*,
    core::Located,
    diagnostic::{Diagnostic, Label},
    visitor::Visitor,
};
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::Problem;
use std::convert::Infallible;

use crate::{
    result::SemanticResult,
    rule_support::{run_rule, DiagnosticVisitor},
    semantic_context::SemanticContext,
    type_environment::{TypeEnvironment, ValueOwners},
};

pub fn apply(
    lib: &Library,
    context: &SemanticContext,
    _options: &CompilerOptions,
) -> SemanticResult {
    run_rule(
        RuleAmbiguousEnumeratedValue {
            types: context.types(),
            diagnostics: Vec::new(),
        },
        lib,
    )
}

struct RuleAmbiguousEnumeratedValue<'a> {
    types: &'a TypeEnvironment,
    diagnostics: Vec<Diagnostic>,
}

impl DiagnosticVisitor for RuleAmbiguousEnumeratedValue<'_> {
    fn into_diagnostics(self) -> Vec<Diagnostic> {
        self.diagnostics
    }
}

impl Visitor<Infallible> for RuleAmbiguousEnumeratedValue<'_> {
    type Value = ();

    fn visit_enumerated_value(&mut self, node: &EnumeratedValue) -> Result<(), Infallible> {
        if node.type_name.is_some() {
            return Ok(());
        }
        if let ValueOwners::Several(owners) = self.types.enumerations_declaring(&node.value) {
            let mut diagnostic = Diagnostic::problem(
                Problem::AmbiguousEnumeratedValue,
                Label::span(node.span(), "Value is declared by several enumerations"),
            )
            .with_context_id("value", &node.value)
            .with_help(
                "Rename the value so that one enumeration declares it, or write Enumeration#Value where that form is accepted",
            );
            for owner in owners {
                diagnostic = diagnostic.with_secondary(Label::span(
                    owner.value.span(),
                    format!("Value of enumeration {}", owner.enumeration),
                ));
            }
            self.diagnostics.push(diagnostic);
        }
        Ok(())
    }

    fn visit_enumeration_declaration(
        &mut self,
        node: &EnumerationDeclaration,
    ) -> Result<(), Infallible> {
        // A list of values declares them and its default is one of them. A
        // declaration that names another enumeration lists nothing, and its
        // default is a use of a name like any other.
        match &node.spec_init.spec {
            SpecificationKind::Inline(_) => Ok(()),
            SpecificationKind::Named(_) => node
                .spec_init
                .default
                .as_ref()
                .map_or(Ok(()), |value| self.visit_enumerated_default(value)),
        }
    }

    fn visit_enumerated_values_initializer(
        &mut self,
        _node: &EnumeratedValuesInitializer,
    ) -> Result<(), Infallible> {
        // The list of values of an enumeration spelled out where a variable is
        // declared, and its initial value: the list declares them.
        Ok(())
    }
}

#[cfg(test)]
mod tests;
