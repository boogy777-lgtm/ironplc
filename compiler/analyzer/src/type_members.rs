//! The members of a type: what `owner.name` can select.
//!
//! A reference such as `f.y`, `s.b.c` or `arr[1].m` selects a member of the
//! type of what stands before the dot. Which names a type has is a fact of
//! its own declaration, and this module is the one place that states it, so
//! that a rule asking "is this a member?" and a walk asking "what is the
//! type of this member?" cannot disagree:
//!
//! | Type | Its members |
//! |---|---|
//! | structure, union | the elements of the declaration |
//! | function block declared in the project | every `VAR_INPUT`, `VAR_OUTPUT`, `VAR_IN_OUT` and `VAR` (and `VAR_STAT`, `VAR_INST`) variable of the block **and of every block in its `EXTENDS` chain**, its properties and its methods |
//! | standard function block | the fields the type environment holds for it |
//! | any other type that is known | none |
//!
//! The type environment does not hold the members of a function block
//! declared in the project: its field list leaves out a variable whose type
//! it does not represent yet (strings, arrays, structures), the inherited
//! variables, the properties and the methods. The members of such a block are
//! therefore read from its declaration, through the same
//! `FunctionBlockVarType::of` that decides which variables the field list
//! holds, so the two cannot differ about which variable blocks are members.
//!
//! A type about which the analyzer cannot say is [`Membership::Unresolved`],
//! never "has no members": a rule that reports an absent member must be sure.
//! That is the case for a reference, a function, a function block whose
//! `EXTENDS` chain leaves the project's declarations for a name that is no
//! function block, and for the empty structure that stands for an `INTERFACE`
//! (see `xform_resolve_type_decl_environment`).
//!
//! A new kind of type is one more arm of the exhaustive match in
//! [`TypeMembers::of`]; a new kind of member is one more [`Member`] variant
//! and one more arm in [`Members::find`] and [`Member::value_type`].

use ironplc_dsl::common::{
    FunctionBlockDeclaration, MethodDeclaration, PropertyDeclaration, TypeName, VarDecl,
};
use ironplc_dsl::core::Id;

use crate::callee_resolution::FunctionBlocks;
use crate::intermediate_type::{FunctionBlockVarType, IntermediateStructField, IntermediateType};
use crate::type_environment::TypeEnvironment;
use crate::variable_type::resolve_initializer;

/// The members of the types of a project.
pub(crate) struct TypeMembers<'a> {
    function_blocks: FunctionBlocks<'a>,
    types: &'a TypeEnvironment,
}

/// What a type exposes to `owner.name`.
pub(crate) enum Membership<'a> {
    /// The type has members, which may be searched.
    Members(Members<'a>),
    /// The type is known and has none: an elementary type, an enumeration,
    /// a subrange, a string or an array.
    NoMembers,
    /// The analyzer cannot say what the members are; see the module
    /// documentation.
    Unresolved,
}

/// The members of one type.
pub(crate) struct Members<'a> {
    /// Where the members are declared, nearest declaration first.
    sources: Vec<Source<'a>>,
}

/// A place that declares members.
enum Source<'a> {
    /// The fields of a structure, a union or a standard function block.
    Fields(&'a [IntermediateStructField]),
    /// A function block declared in the project.
    Block(&'a FunctionBlockDeclaration),
}

/// A member of a type.
pub(crate) enum Member<'a> {
    /// A field of a structure, a union or a standard function block.
    Field(&'a IntermediateStructField),
    /// A variable of a function block declared in the project, with the block
    /// of the `EXTENDS` chain that declares it.
    Variable {
        decl: &'a VarDecl,
        block: &'a FunctionBlockDeclaration,
    },
    Property(&'a PropertyDeclaration),
    Method(&'a MethodDeclaration),
}

impl<'a> TypeMembers<'a> {
    pub(crate) fn new(
        function_blocks: FunctionBlocks<'a>,
        types: &'a TypeEnvironment,
    ) -> TypeMembers<'a> {
        TypeMembers {
            function_blocks,
            types,
        }
    }

    /// The members of the type `owner`.
    ///
    /// The match has no wildcard: a new kind of type must say what its
    /// members are.
    pub(crate) fn of<'o>(&'o self, owner: &'o IntermediateType) -> Membership<'o> {
        match owner {
            // An `INTERFACE` is entered as a structure without elements,
            // which says nothing about what an instance of a block that
            // implements it has.
            IntermediateType::Structure { fields, .. } if fields.is_empty() => {
                Membership::Unresolved
            }
            IntermediateType::Structure { fields, .. } => {
                Membership::Members(Members::of_fields(fields))
            }
            IntermediateType::FunctionBlock { name, fields } => {
                self.of_function_block(&TypeName::from(name.as_str()), fields)
            }
            // `r.name` on a reference is not a selection from the target
            // unless the reference is dereferenced first (`r^.name`), and
            // which spellings the dialects accept is not decided here.
            IntermediateType::Reference { .. } => Membership::Unresolved,
            IntermediateType::Function { .. } => Membership::Unresolved,
            IntermediateType::Bool
            | IntermediateType::Int { .. }
            | IntermediateType::UInt { .. }
            | IntermediateType::Real { .. }
            | IntermediateType::Bytes { .. }
            | IntermediateType::Time { .. }
            | IntermediateType::Date { .. }
            | IntermediateType::TimeOfDay { .. }
            | IntermediateType::DateAndTime { .. }
            | IntermediateType::String { .. }
            | IntermediateType::Enumeration { .. }
            | IntermediateType::Array { .. }
            | IntermediateType::Subrange { .. } => Membership::NoMembers,
        }
    }

    fn of_function_block<'o>(
        &'o self,
        name: &TypeName,
        fields: &'o [IntermediateStructField],
    ) -> Membership<'o> {
        // A standard function block is not declared in the project: the
        // fields of its type are its declaration.
        if !self.function_blocks.contains(name) {
            return Membership::Members(Members::of_fields(fields));
        }

        let lineage = self.function_blocks.lineage(name);
        let mut sources: Vec<Source<'o>> = lineage.blocks.into_iter().map(Source::Block).collect();
        if let Some(base) = lineage.open_base {
            match self.types.get(base).map(|base| &base.representation) {
                Some(IntermediateType::FunctionBlock { fields, .. }) => {
                    sources.push(Source::Fields(fields));
                }
                // The base is no function block, or is not declared: the
                // `EXTENDS` clause is reported where it is written, and the
                // members it would add are not known.
                _ => return Membership::Unresolved,
            }
        }
        Membership::Members(Members { sources })
    }

    /// The variables the function block `name` inherits that its own
    /// declarations can name without a qualifier: those of the blocks it
    /// extends that are members of an instance.
    pub(crate) fn inherited_variables(&self, name: &TypeName) -> Vec<&'a VarDecl> {
        self.function_blocks
            .lineage(name)
            .blocks
            .into_iter()
            .skip(1)
            .flat_map(instance_variables)
            .collect()
    }

    /// Whether the code of the function block `block` is code of `ancestor`:
    /// `block` is `ancestor` or has it in its `EXTENDS` chain.
    pub(crate) fn is_code_of(&self, block: &TypeName, ancestor: &FunctionBlockDeclaration) -> bool {
        self.function_blocks
            .lineage(block)
            .blocks
            .iter()
            .any(|declared| declared.name == ancestor.name)
    }

    /// The type of the member `member`.
    pub(crate) fn type_of(&self, member: &Member<'_>) -> Option<IntermediateType> {
        member.value_type(self.types)
    }
}

impl<'a> Members<'a> {
    fn of_fields(fields: &'a [IntermediateStructField]) -> Members<'a> {
        Members {
            sources: vec![Source::Fields(fields)],
        }
    }

    /// The member called `name`, in the nearest declaration that declares it.
    pub(crate) fn find(&self, name: &Id) -> Option<Member<'a>> {
        self.sources.iter().find_map(|source| match source {
            Source::Fields(fields) => fields
                .iter()
                .find(|field| &field.name == name)
                .map(Member::Field),
            Source::Block(block) => instance_variables(block)
                .find(|decl| decl.identifier.symbolic_id() == Some(name))
                .map(|decl| Member::Variable { decl, block })
                .or_else(|| {
                    block
                        .properties
                        .iter()
                        .find(|property| &property.name == name)
                        .map(Member::Property)
                })
                .or_else(|| {
                    block
                        .methods
                        .iter()
                        .find(|method| &method.name == name)
                        .map(Member::Method)
                }),
        })
    }

    /// The names that a selection may name -- the fields, variables and
    /// properties -- for a suggestion in a message.
    pub(crate) fn value_names(&self) -> impl Iterator<Item = &'a Id> + '_ {
        self.sources.iter().flat_map(|source| {
            let names: Box<dyn Iterator<Item = &'a Id> + '_> = match source {
                Source::Fields(fields) => Box::new(fields.iter().map(|field| &field.name)),
                Source::Block(block) => Box::new(
                    instance_variables(block)
                        .filter_map(|decl| decl.identifier.symbolic_id())
                        .chain(block.properties.iter().map(|property| &property.name)),
                ),
            };
            names
        })
    }
}

/// The kind of variable a member is, and where it is declared.
pub(crate) struct DeclaredVariable<'a> {
    pub(crate) kind: FunctionBlockVarType,
    /// The function block that declares it. `None` for a variable of a standard
    /// function block, which no code of the project is part of.
    pub(crate) block: Option<&'a FunctionBlockDeclaration>,
}

impl<'a> Member<'a> {
    /// The variable kind of the member, `None` for a member that is not a
    /// variable of a function block: an element of a structure or a union, a
    /// property, a method.
    ///
    /// The kind comes from the member's own declaration; it is what decides
    /// what code outside the block may do with the member (see `member_access`).
    pub(crate) fn declared_variable(&self) -> Option<DeclaredVariable<'a>> {
        match self {
            Member::Field(field) => field
                .var_type
                .map(|kind| DeclaredVariable { kind, block: None }),
            Member::Variable { decl, block } => {
                FunctionBlockVarType::of(&decl.var_type).map(|kind| DeclaredVariable {
                    kind,
                    block: Some(block),
                })
            }
            Member::Property(_) | Member::Method(_) => None,
        }
    }
}

impl Member<'_> {
    /// The type of a value read from, or written to, the member. `None` for a
    /// method, which has no value without a call, and for a member whose type
    /// the analyzer does not represent.
    pub(crate) fn value_type(&self, types: &TypeEnvironment) -> Option<IntermediateType> {
        match self {
            Member::Field(field) => Some(field.field_type.clone()),
            Member::Variable { decl, .. } => decl
                .type_id
                .and_then(|id| types.get_by_id(id))
                .map(|attributes| attributes.representation.clone())
                .or_else(|| resolve_initializer(&decl.initializer, types)),
            Member::Property(property) => types
                .get(&property.property_type.to_type_name())
                .map(|attributes| attributes.representation.clone()),
            Member::Method(_) => None,
        }
    }
}

/// The variables of `block` that are members of its instances.
fn instance_variables(block: &FunctionBlockDeclaration) -> impl Iterator<Item = &VarDecl> {
    block
        .variables
        .iter()
        .filter(|decl| FunctionBlockVarType::of(&decl.var_type).is_some())
}

#[cfg(test)]
mod tests;
