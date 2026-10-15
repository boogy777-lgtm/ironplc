//! Resolution of variable references to names, spans, table indices and access-target kinds.

use ironplc_container::VarIndex;
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::{
    BitAccessVariable, PartialAccessVariable, SymbolicVariableKind, Variable,
};

use crate::compile::CompileContext;
use crate::emit::Emitter;

/// Resolves the name of the innermost named variable from a symbolic variable kind.
pub(super) fn resolve_symbolic_variable_name(
    kind: &SymbolicVariableKind,
) -> Result<&Id, Diagnostic> {
    match kind {
        SymbolicVariableKind::Named(named) => Ok(&named.name),
        SymbolicVariableKind::BitAccess(bit_access) => {
            resolve_symbolic_variable_name(&bit_access.variable)
        }
        SymbolicVariableKind::PartialAccess(partial) => {
            resolve_symbolic_variable_name(&partial.variable)
        }
        SymbolicVariableKind::Array(array) => {
            resolve_symbolic_variable_name(&array.subscripted_variable)
        }
        SymbolicVariableKind::Structured(structured) => {
            resolve_symbolic_variable_name(&structured.record)
        }
        SymbolicVariableKind::Deref(deref) => resolve_symbolic_variable_name(&deref.variable),
        // THIS^/SUPER^ names the executing instance, not a variable in the
        // table. Giving it a receiver-pointer parameter is the codegen
        // slice that also owns method-call codegen; until then this is an
        // error, never a guess at some enclosing name.
        SymbolicVariableKind::SelfRef(self_ref) => Err(Diagnostic::todo_with_span(self_ref.span())),
    }
}

/// Returns the slot holding the reference when `variable` names a
/// `VAR_IN_OUT` parameter of the function being compiled.
pub(crate) fn in_out_ref_slot(ctx: &CompileContext, variable: &Variable) -> Option<VarIndex> {
    match variable {
        Variable::Symbolic(SymbolicVariableKind::Named(named)) => ctx.in_out_ref_slot(&named.name),
        _ => None,
    }
}

/// Loads the value of the variable a `VAR_IN_OUT` parameter refers to.
pub(super) fn emit_load_in_out(emitter: &mut Emitter, ref_slot: VarIndex) {
    emitter.emit_load_var_i64(ref_slot);
    emitter.emit_load_indirect();
}

/// Resolves a variable reference to its variable table index.
pub(crate) fn resolve_variable(
    ctx: &CompileContext,
    variable: &Variable,
) -> Result<VarIndex, Diagnostic> {
    match variable {
        Variable::Symbolic(symbolic) => match symbolic {
            SymbolicVariableKind::Named(named) => ctx.var_index(&named.name),
            SymbolicVariableKind::Array(array) => Err(Diagnostic::todo_with_span(array.span())),
            SymbolicVariableKind::Structured(structured) => {
                Err(Diagnostic::todo_with_span(structured.span()))
            }
            SymbolicVariableKind::BitAccess(bit_access) => {
                Err(Diagnostic::todo_with_span(bit_access.span()))
            }
            SymbolicVariableKind::PartialAccess(pa) => Err(Diagnostic::todo_with_span(pa.span())),
            SymbolicVariableKind::Deref(deref) => Err(Diagnostic::todo_with_span(deref.span())),
            SymbolicVariableKind::SelfRef(self_ref) => {
                Err(Diagnostic::todo_with_span(self_ref.span()))
            }
        },
        Variable::Direct(direct) => Err(Diagnostic::todo_with_span(direct.position.clone())),
    }
}

/// Extracts the variable name `Id` from a variable reference, if it is a named symbolic variable.
pub(crate) fn resolve_variable_name(variable: &Variable) -> Option<&Id> {
    match variable {
        Variable::Symbolic(SymbolicVariableKind::Named(named)) => Some(&named.name),
        _ => None,
    }
}

/// Extracts a SourceSpan from a Variable for diagnostic messages.
pub(crate) fn variable_span(variable: &Variable) -> ironplc_dsl::core::SourceSpan {
    match variable {
        Variable::Symbolic(kind) => kind.span(),
        Variable::Direct(addr) => addr.position.clone(),
    }
}

/// Extracts a `BitAccessVariable` from an assignment target, if it is a bit access.
pub(crate) fn extract_bit_access_target(variable: &Variable) -> Option<&BitAccessVariable> {
    match variable {
        Variable::Symbolic(SymbolicVariableKind::BitAccess(bit_access)) => Some(bit_access),
        _ => None,
    }
}

/// Extracts a `PartialAccessVariable` from an assignment target.
pub(crate) fn extract_partial_access_target(variable: &Variable) -> Option<&PartialAccessVariable> {
    match variable {
        Variable::Symbolic(SymbolicVariableKind::PartialAccess(pa)) => Some(pa),
        _ => None,
    }
}
