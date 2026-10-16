//! Semantic rule that code outside a function block assigns only the inputs of
//! its instances and touches none of their `VAR_IN_OUT`.
//!
//! From outside a function block, a program may write the inputs of an
//! instance, may read its inputs, outputs and internal variables, and may not
//! write its outputs or internal variables (P4081) nor read or write its
//! `VAR_IN_OUT` (P4082). The call `f(io := x)` still passes a variable to
//! the `VAR_IN_OUT`: that is the way to give one to the block. Code of the
//! block itself -- its body, its methods, and the body of a block that extends
//! it -- is not outside. This follows CODESYS, the reference (C0037 and C0178).
//!
//! Which accesses are allowed is decided by `member_access::check`, from the
//! variable kind of the member and the `EXTENDS` lineage of the code that
//! accesses it; this rule only finds the accesses and which of them write. A
//! write is an assignment target, the target of an output binding
//! (`g(q => f.local)`), or an argument bound to a `VAR_IN_OUT` of a function, a
//! function block or a method (`g(io := f.local)`). Writing into a part of a
//! variable (`f.list[1] := 1`, `f.rec.m := 1`) is a write to the variable of the
//! instance. A `FOR` counter is a plain name, so it selects no member.
//!
//! The members that a type does not declare are P4080 (`rule_member_declared`),
//! so a selection reported there is not reported here. A `VAR_TEMP` is not a
//! member, so it is P4080 only. The initializer of an instance
//! (`f : FB := (y := 1)`) sets members from outside the block too, and is not
//! checked here.
//!
//! ## Passes
//!
//! ```ignore
//! FUNCTION_BLOCK Counter
//!    VAR_INPUT step : INT; END_VAR
//!    VAR_OUTPUT total : INT; END_VAR
//! END_FUNCTION_BLOCK
//!
//! PROGRAM main
//!    VAR c : Counter; x : INT; END_VAR
//!    c.step := 2;
//!    x := c.total;
//! END_PROGRAM
//! ```
//!
//! ## Fails
//!
//! ```ignore
//! PROGRAM main
//!    VAR c : Counter; END_VAR
//!    c.total := 0;
//! END_PROGRAM
//! ```
use std::{collections::HashSet, convert::Infallible};

use ironplc_dsl::{
    common::*,
    core::{FileId, Id, Located},
    diagnostic::Diagnostic,
    scope::ScopeNode,
    textual::*,
    visitor::Visitor,
};
use ironplc_problems::text;

use crate::{
    call_assignment_check::bind_inputs,
    callee_resolution::FunctionBlocks,
    intermediate_type::IntermediateType,
    member_access::{self, Access, Refusal},
    member_selection::Selections,
    result::SemanticResult,
    rule_support::{run_rule, DiagnosticVisitor},
    semantic_context::SemanticContext,
    type_members::Membership,
};
use ironplc_parser::options::CompilerOptions;

pub fn apply(
    lib: &Library,
    context: &SemanticContext,
    _options: &CompilerOptions,
) -> SemanticResult {
    run_rule(
        RuleMemberAccess {
            context,
            function_blocks: FunctionBlocks::from_library(lib),
            selections: Selections::new(lib, context.types()),
            written: HashSet::new(),
            diagnostics: Vec::new(),
        },
        lib,
    )
}

struct RuleMemberAccess<'a> {
    context: &'a SemanticContext,
    function_blocks: FunctionBlocks<'a>,
    selections: Selections<'a>,
    /// The places of the member names that a write reaches, found where the
    /// write is (the statement or the call) before its variable is visited.
    written: HashSet<Place>,
    diagnostics: Vec<Diagnostic>,
}

/// Where a name is written: a name has no identity but its position.
type Place = (FileId, usize, usize);

fn place(name: &Id) -> Place {
    let span = name.span();
    (span.file_id, span.start, span.end)
}

impl DiagnosticVisitor for RuleMemberAccess<'_> {
    fn into_diagnostics(self) -> Vec<Diagnostic> {
        self.diagnostics
    }
}

impl RuleMemberAccess<'_> {
    /// The member selection from a function block instance that writing
    /// `kind` writes, if there is one.
    ///
    /// Writing part of a variable writes the variable: through an array
    /// subscript, a bit or part access, and the elements of a structure or a
    /// union. The first selection from an instance on the way to the root is the
    /// member that is written; what stands before it is only read. A write
    /// through a reference writes no member of an instance by name.
    fn written_member<'v>(&self, kind: &'v SymbolicVariableKind) -> Option<&'v Id> {
        match kind {
            SymbolicVariableKind::Structured(structured) => {
                match self.selections.selected_type(&structured.record) {
                    Some(IntermediateType::FunctionBlock { .. }) => Some(&structured.field),
                    Some(_) => self.written_member(&structured.record),
                    None => None,
                }
            }
            SymbolicVariableKind::Array(array) => self.written_member(&array.subscripted_variable),
            SymbolicVariableKind::BitAccess(bit) => self.written_member(&bit.variable),
            SymbolicVariableKind::PartialAccess(partial) => self.written_member(&partial.variable),
            SymbolicVariableKind::Named(_)
            | SymbolicVariableKind::Deref(_)
            | SymbolicVariableKind::SelfRef(_) => None,
        }
    }

    /// Records that `variable` is written.
    fn write(&mut self, variable: &Variable) {
        let name = match variable {
            Variable::Symbolic(kind) => self.written_member(kind).map(place),
            Variable::Direct(_) => None,
        };
        self.written.extend(name);
    }

    /// Records the arguments of a call that `owner` binds to a `VAR_IN_OUT`.
    fn write_in_out_arguments(&mut self, owner: &dyn HasVariables, params: &[ParamAssignmentKind]) {
        for (param, declared) in bind_inputs(owner, params) {
            let in_out = declared.is_some_and(|decl| decl.var_type == VariableType::InOut);
            if let (true, Some(ExprKind::Variable(variable))) =
                (in_out, param.input_expr().map(|expr| &expr.kind))
            {
                self.write(variable);
            }
        }
    }

    fn check_selection(&mut self, node: &StructuredVariable) {
        let Some(owner) = self.selections.selected_type(&node.record) else {
            return;
        };
        let Membership::Members(members) = self.selections.members().of(&owner) else {
            return;
        };
        let Some(member) = members.find(&node.field) else {
            return;
        };
        let access = if self.written.contains(&place(&node.field)) {
            Access::Write
        } else {
            Access::Read
        };
        let refused = member_access::check(
            self.selections.members(),
            self.selections.enclosing_block(),
            &member,
            access,
        );
        if let Err(refusal) = refused {
            let owner_text = node.record.to_string();
            self.diagnostics
                .push(refused_access(refusal, &owner_text, &node.field));
        }
    }
}

/// The diagnostic for an access that the block does not allow, at the name of
/// the member. The match has no wildcard: a new reason to refuse must say how
/// it is reported.
fn refused_access(refusal: Refusal, owner: &str, member: &Id) -> Diagnostic {
    let text = match refusal {
        Refusal::OutputWritten => text::member_not_assignable_outside_output(member, owner),
        Refusal::InternalWritten => text::member_not_assignable_outside_internal(member, owner),
        Refusal::InOutTouched => text::in_out_member_accessed_outside_access(member, owner),
    };
    Diagnostic::text(member.span(), text)
        .with_context("owner", &owner.to_owned())
        .with_context_id("member", member)
}

impl Visitor<Infallible> for RuleMemberAccess<'_> {
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

    fn visit_assignment(&mut self, node: &Assignment) -> Result<(), Infallible> {
        self.write(&node.target);
        node.recurse_visit(self)
    }

    fn visit_output(&mut self, node: &Output) -> Result<(), Infallible> {
        self.write(&node.tgt);
        node.recurse_visit(self)
    }

    fn visit_function(&mut self, node: &Function) -> Result<(), Infallible> {
        if let Some(signature) = self.context.functions.get(&node.name) {
            for (param, arg) in signature.bind_inputs(&node.param_assignment) {
                if let (true, ExprKind::Variable(variable)) = (param.is_inout, &arg.kind) {
                    self.write(variable);
                }
            }
        }
        node.recurse_visit(self)
    }

    fn visit_fb_call(&mut self, node: &FbCall) -> Result<(), Infallible> {
        if let Variable::Symbolic(callee) = &node.callee {
            if let Some(IntermediateType::FunctionBlock { name, .. }) =
                self.selections.selected_type(callee)
            {
                if let Some(block) = self.function_blocks.get(&TypeName::from(name.as_str())) {
                    self.write_in_out_arguments(block, &node.params);
                }
            }
        }
        node.recurse_visit(self)
    }

    fn visit_method_call(&mut self, node: &MethodCall) -> Result<(), Infallible> {
        if let MethodReceiver::Instance(instance) = &node.receiver {
            if let Some(IntermediateType::FunctionBlock { name, .. }) =
                self.selections.declared_type(instance)
            {
                let resolved = self
                    .function_blocks
                    .resolve_method(&TypeName::from(name.as_str()), &node.method);
                if let Some((_, method)) = resolved {
                    self.write_in_out_arguments(method, &node.params);
                }
            }
        }
        node.recurse_visit(self)
    }

    fn visit_structured_variable(&mut self, node: &StructuredVariable) -> Result<(), Infallible> {
        self.check_selection(node);
        node.recurse_visit(self)
    }
}

#[cfg(test)]
mod tests;
