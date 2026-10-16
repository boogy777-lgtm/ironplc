//! Semantic rule that a member selected from a value is a member that the
//! type of the value declares.
//!
//! `f.y` selects the member `y` of what `f` is: an element of a structure or a
//! union, a variable, a property of a function block instance (the block's own
//! and those it inherits through `EXTENDS`). A name that the type does not
//! declare selects nothing, whether it is read (`x := f.nothere`) or written
//! (`f.nothere := 1`), and so does the name of an element in an initializer
//! (`s : S := (nothere := 1)`). `VAR_TEMP` belongs to one call of the block, so
//! it is no member of an instance.
//!
//! The members of a type are stated once, in `type_members`; this rule asks
//! that module and holds no list of its own.
//!
//! A selection from a value whose type is not known -- the variable is not
//! declared, or its type is not, or its declaration has an error -- is not
//! reported: that is reported where the variable or the type is. A selection
//! whose owner is itself a bad selection (`s.nothere.c`) is reported once, at
//! `nothere`.
//!
//! What this rule leaves to others: the call of a method (`f.M()`, P4046), the
//! bit and partial access of an integer (`x.3`, `x.%X3`, which are not
//! members), a value of an enumeration (`E.V`, P2006 and P4035), and `THIS^` and
//! `SUPER^`, which are not resolved yet.
//!
//! ## Passes
//!
//! ```ignore
//! TYPE S : STRUCT m : INT; END_STRUCT; END_TYPE
//!
//! FUNCTION_BLOCK FB1
//!    VAR_OUTPUT y : INT; END_VAR
//! END_FUNCTION_BLOCK
//!
//! PROGRAM main
//!    VAR f : FB1; s : S; x : INT; END_VAR
//!    x := f.y;
//!    s.m := x;
//! END_PROGRAM
//! ```
//!
//! ## Fails
//!
//! ```ignore
//! PROGRAM main
//!    VAR f : FB1; s : S; x : INT; END_VAR
//!    x := f.nothere;
//!    s.nothere := 1;
//! END_PROGRAM
//! ```
use std::convert::Infallible;

use ironplc_dsl::{
    common::*,
    core::{Id, Located},
    diagnostic::{Diagnostic, Label},
    scope::ScopeNode,
    textual::*,
    visitor::Visitor,
};
use ironplc_problems::text;

use crate::{
    intermediate_type::IntermediateType,
    member_selection::Selections,
    result::SemanticResult,
    rule_support::{run_rule, DiagnosticVisitor},
    semantic_context::SemanticContext,
    string_similarity::find_closest_match,
    type_members::{Member, Members, Membership},
};
use ironplc_parser::options::CompilerOptions;

pub fn apply(
    lib: &Library,
    context: &SemanticContext,
    _options: &CompilerOptions,
) -> SemanticResult {
    run_rule(
        RuleMemberDeclared {
            selections: Selections::new(lib, context.types()),
            diagnostics: Vec::new(),
        },
        lib,
    )
}

struct RuleMemberDeclared<'a> {
    selections: Selections<'a>,
    diagnostics: Vec<Diagnostic>,
}

impl DiagnosticVisitor for RuleMemberDeclared<'_> {
    fn into_diagnostics(self) -> Vec<Diagnostic> {
        self.diagnostics
    }
}

impl RuleMemberDeclared<'_> {
    fn check_selection(&mut self, node: &StructuredVariable) {
        let Some(owner) = self.selections.selected_type(&node.record) else {
            return;
        };
        let owner_text = node.record.to_string();
        let diagnostic = match self.selections.members().of(&owner) {
            Membership::Unresolved => None,
            Membership::NoMembers => Some(
                Diagnostic::text(
                    node.field.span(),
                    text::member_not_declared_type_has_no_members(&owner_text, &node.field),
                )
                .with_context("owner", &owner_text)
                .with_context_id("member", &node.field),
            ),
            Membership::Members(members) => match members.find(&node.field) {
                Some(Member::Method(method)) => Some(
                    Diagnostic::text(
                        node.field.span(),
                        text::member_not_declared_method_selected_as_variable(
                            &node.field,
                            &owner_text,
                        ),
                    )
                    .with_context("owner", &owner_text)
                    .with_context_id("member", &node.field)
                    .with_secondary(Label::span(method.name.span(), "Method declared here")),
                ),
                Some(_) => None,
                None => Some(not_a_member(&owner_text, &node.field, &members)),
            },
        };
        self.diagnostics.extend(diagnostic);
    }

    /// Checks the names an initializer gives to the elements of the type
    /// `owner`, and of the elements of those that are structures in turn.
    fn check_elements(
        &self,
        owner: &IntermediateType,
        owner_text: &str,
        elements: &[StructureElementInit],
    ) -> Vec<Diagnostic> {
        let Membership::Members(members) = self.selections.members().of(owner) else {
            return Vec::new();
        };
        let mut found = Vec::new();
        for element in elements {
            let Some(member) = members.find(&element.name) else {
                found.push(not_a_member(owner_text, &element.name, &members));
                continue;
            };
            if let StructInitialValueAssignmentKind::Structure(inner) = &element.init {
                if let Some(member_type) = self.selections.members().type_of(&member) {
                    found.extend(self.check_elements(
                        &member_type,
                        &element.name.to_string(),
                        inner,
                    ));
                }
            }
        }
        found
    }
}

/// The diagnostic for a name that the type does not declare, with the closest
/// name the type does declare when there is one.
fn not_a_member(owner_text: &str, name: &Id, members: &Members<'_>) -> Diagnostic {
    let mut diagnostic = Diagnostic::text(
        name.span(),
        text::member_not_declared_not_a_member(owner_text, name),
    )
    .with_context("owner", &owner_text.to_owned())
    .with_context_id("member", name);
    let candidates: Vec<&Id> = members.value_names().collect();
    if let Some(suggestion) = find_closest_match(
        name.original(),
        candidates
            .iter()
            .map(|candidate| candidate.original().as_str()),
    ) {
        diagnostic = diagnostic.with_context("did you mean", &suggestion);
    }
    diagnostic
}

impl Visitor<Infallible> for RuleMemberDeclared<'_> {
    type Value = ();

    fn enter_scope(&mut self, node: ScopeNode<'_>) -> Result<(), Infallible> {
        self.selections.enter_scope(node);
        Ok(())
    }

    fn exit_scope(&mut self) {
        self.selections.exit_scope();
    }

    fn visit_var_decl(&mut self, node: &VarDecl) -> Result<(), Infallible> {
        self.selections.declare(node);
        node.recurse_visit(self)
    }

    fn visit_structured_variable(&mut self, node: &StructuredVariable) -> Result<(), Infallible> {
        self.check_selection(node);
        node.recurse_visit(self)
    }

    fn visit_structure_initialization_declaration(
        &mut self,
        node: &StructureInitializationDeclaration,
    ) -> Result<(), Infallible> {
        if let Some(attributes) = self.selections.types().get(&node.type_name) {
            let found = self.check_elements(
                &attributes.representation,
                &node.type_name.to_string(),
                &node.elements_init,
            );
            self.diagnostics.extend(found);
        }
        node.recurse_visit(self)
    }

    fn visit_function_block_initial_value_assignment(
        &mut self,
        node: &FunctionBlockInitialValueAssignment,
    ) -> Result<(), Infallible> {
        if let Some(attributes) = self.selections.types().get(&node.type_name) {
            let found = self.check_elements(
                &attributes.representation,
                &node.type_name.to_string(),
                &node.init,
            );
            self.diagnostics.extend(found);
        }
        node.recurse_visit(self)
    }
}

#[cfg(test)]
mod tests;
