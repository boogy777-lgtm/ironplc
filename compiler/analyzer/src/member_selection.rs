//! The scope a rule walks to resolve `owner.member`: what the declared
//! variables are, which function block the code belongs to, and the type of
//! what stands before a dot.
//!
//! A rule about the selection of a member has to know, at each `a.b.c`, the
//! type of `a.b`: the declarations in scope, the members of the types
//! ([`TypeMembers`]) and the inherited variables a function block names without
//! a qualifier. That state and its walk are the same for every such rule, so
//! they are here and the rules (`rule_member_declared`, `rule_member_access`)
//! hold one [`Selections`] each and feed it the scopes and the declarations
//! they walk.
//!
//! This is not `variable_type::of`: that answers the type of the variable a bit
//! access applies to and does not look through a reference, and the members of
//! a function block it knows are those the type environment holds, which leave
//! out a variable of a string, array or structure type and every inherited one.

use ironplc_dsl::{
    common::{Library, TypeName, VarDecl},
    scope::ScopeNode,
    textual::SymbolicVariableKind,
};

use crate::{
    callee_resolution::FunctionBlocks,
    intermediate_type::IntermediateType,
    type_environment::TypeEnvironment,
    type_members::{Membership, TypeMembers},
    variable_type::{self, Declarations, Declared},
};

/// The declarations in scope at the point of a walk, and what they select.
pub(crate) struct Selections<'a> {
    types: &'a TypeEnvironment,
    members: TypeMembers<'a>,
    /// The declared type of every variable in scope.
    declarations: Declarations<'a>,
    /// For each scope the walk is inside, the function block the code of that
    /// scope belongs to: a function block's own scope, and the scope of each of
    /// its methods, which are the block's code too. A function and a program
    /// belong to none.
    blocks: Vec<Option<TypeName>>,
}

impl<'a> Selections<'a> {
    pub(crate) fn new(lib: &'a Library, types: &'a TypeEnvironment) -> Self {
        Selections {
            types,
            members: TypeMembers::new(FunctionBlocks::from_library(lib), types),
            // `Declarations::new` opens the base scope, where declarations
            // made outside any POU land.
            declarations: Declarations::new(),
            blocks: Vec::new(),
        }
    }

    pub(crate) fn types(&self) -> &'a TypeEnvironment {
        self.types
    }

    pub(crate) fn members(&self) -> &TypeMembers<'a> {
        &self.members
    }

    /// The function block whose code the walk is in: the block itself, one of
    /// its methods, `None` in a function or a program.
    pub(crate) fn enclosing_block(&self) -> Option<&TypeName> {
        self.blocks.last().and_then(Option::as_ref)
    }

    /// Opens the scope of a declaration. A function block's scope also holds
    /// the variables it inherits through `EXTENDS`, which its body names
    /// without a qualifier.
    pub(crate) fn enter_scope(&mut self, node: ScopeNode<'_>) {
        let block = match &node {
            ScopeNode::FunctionBlock(block) => Some(block.name.clone()),
            // A method is code of the block it is declared in.
            ScopeNode::Method(_) => self.enclosing_block().cloned(),
            ScopeNode::Function(_) | ScopeNode::Program(_) => None,
        };
        variable_type::enter_declaration_scope(&mut self.declarations, node);
        if let Some(name) = &block {
            for decl in self.members.inherited_variables(name) {
                self.declarations
                    .add_if(decl.identifier.symbolic_id(), Declared::of(decl));
            }
        }
        self.blocks.push(block);
    }

    pub(crate) fn exit_scope(&mut self) {
        self.declarations.exit();
        self.blocks.pop();
    }

    /// Records a declaration of the scope the walk is in.
    pub(crate) fn declare(&mut self, node: &VarDecl) {
        self.declarations
            .add_if(node.identifier.symbolic_id(), Declared::of(node));
    }

    /// The type of the value that `kind` selects, when it is known.
    pub(crate) fn selected_type(&self, kind: &SymbolicVariableKind) -> Option<IntermediateType> {
        match kind {
            // The id the analyzer resolved the declaration to names every kind
            // of type a variable can have, a reference among them; the
            // initializer is the fallback for a declaration without one.
            SymbolicVariableKind::Named(named) => self
                .declarations
                .find(&named.name)?
                .type_id(self.types)
                .and_then(|id| self.types.get_by_id(id))
                .map(|attributes| attributes.representation.clone())
                .or_else(|| {
                    variable_type::declared_type(&named.name, &self.declarations, self.types)
                }),
            SymbolicVariableKind::Structured(structured) => {
                let owner = self.selected_type(&structured.record)?;
                match self.members.of(&owner) {
                    Membership::Members(members) => {
                        self.members.type_of(&members.find(&structured.field)?)
                    }
                    Membership::NoMembers | Membership::Unresolved => None,
                }
            }
            SymbolicVariableKind::Array(array) => {
                match self.selected_type(&array.subscripted_variable)? {
                    IntermediateType::Array { element_type, .. } => Some(*element_type),
                    _ => None,
                }
            }
            SymbolicVariableKind::Deref(deref) => match self.selected_type(&deref.variable)? {
                IntermediateType::Reference { target_type } => Some(*target_type),
                _ => None,
            },
            // A bit or a part of an integer has no members, and `THIS^` and
            // `SUPER^` are reported as not implemented where they are
            // written.
            SymbolicVariableKind::BitAccess(_)
            | SymbolicVariableKind::PartialAccess(_)
            | SymbolicVariableKind::SelfRef(_) => None,
        }
    }
}
