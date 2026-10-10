//! Which enumeration a value name belongs to.
//!
//! The values of an enumeration are data of its type, and two enumerations may
//! declare the same value name. A bare value name (`U1`) is spelled the same
//! whichever enumeration it is a value of, so the question "which enumeration
//! does this name denote" is answered here, once, for every stage that asks it
//! (a semantic rule, code generation): the name is declared by no enumeration,
//! by exactly one, or by several.
//!
//! An alias of an enumeration (`TYPE EA : E1;`) has the same values with the
//! same ordinals as the enumeration it names, so for this question it is that
//! enumeration and not another one. Two declarations that list the same values
//! are two enumerations.

use std::collections::HashMap;

use indexmap::IndexMap;
use ironplc_dsl::{
    common::TypeName,
    core::{Id, Located},
    type_id::TypeId,
};

use super::TypeEnvironment;
use crate::type_attributes::TypeAttributes;

/// The declarations of enumerations, by the value names they declare.
///
/// Filled by [`TypeEnvironment`] as each type is entered, which is the one
/// place a type enters, so no stage keeps a table of its own.
#[derive(Debug, Default)]
pub(super) struct EnumerationIndex {
    /// The declaration an alias names, for each alias of an enumeration. A
    /// declaration is not in it: it is its own declaration.
    renames: HashMap<TypeId, TypeId>,
    /// The declarations of enumerations that declare a value name, in the
    /// order they were entered. Names are compared without regard to case.
    declaring: IndexMap<Id, Vec<TypeId>>,
}

impl EnumerationIndex {
    /// Records the type `id` that the environment has entered with
    /// `attributes`. `renames` is the type `id` is another name for, when it
    /// is an alias. Does nothing for a type that is not an enumeration.
    pub(super) fn declare(
        &mut self,
        id: TypeId,
        renames: Option<TypeId>,
        attributes: &TypeAttributes,
    ) {
        if !attributes.representation.is_enumeration() {
            return;
        }
        let declaration = renames.map_or(id, |base| self.declaration_of(base));
        if declaration != id {
            self.renames.insert(id, declaration);
        }
        for value in &attributes.enumerated_values {
            let declarations = self.declaring.entry(value.clone()).or_default();
            if !declarations.contains(&declaration) {
                declarations.push(declaration);
            }
        }
    }

    /// The declaration that `id` is: itself, or the declaration an alias names.
    fn declaration_of(&self, id: TypeId) -> TypeId {
        self.renames.get(&id).copied().unwrap_or(id)
    }
}

/// An enumeration that declares a value name.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Owner<'a> {
    /// The name the enumeration was declared with.
    pub enumeration: &'a TypeName,
    /// The value, where the enumeration declares it.
    pub value: &'a Id,
}

/// The enumerations that declare a value name: the answer to which enumeration
/// a bare value name denotes.
#[derive(Debug, Clone, PartialEq)]
pub enum ValueOwners<'a> {
    /// No enumeration declares the name.
    None,
    /// Exactly one enumeration declares the name.
    One(Owner<'a>),
    /// More than one enumeration declares the name, in the order of their
    /// declarations in the source (by file, then by position). The name does
    /// not denote one of them.
    Several(Vec<Owner<'a>>),
}

impl TypeEnvironment {
    /// The enumerations that declare `value`, an alias counted as the
    /// enumeration it names. An enumeration whose declaration has an error
    /// declares nothing here: its declaration is where the error is reported.
    pub fn enumerations_declaring(&self, value: &Id) -> ValueOwners<'_> {
        let mut owners: Vec<Owner<'_>> = self
            .enumerations
            .declaring
            .get(value)
            .into_iter()
            .flatten()
            .filter_map(|id| {
                let entry = self.entries.get(id)?;
                Some(Owner {
                    enumeration: entry.name.as_ref()?,
                    value: entry
                        .valid()?
                        .enumerated_values
                        .iter()
                        .find(|declared| *declared == value)?,
                })
            })
            .collect();
        // The order the declarations are read in is the order of the analysis,
        // not the order of the source: name them where a reader finds them.
        owners.sort_by_cached_key(|owner| {
            let span = owner.enumeration.span();
            (span.file_id, span.start)
        });
        match owners.len() {
            0 => ValueOwners::None,
            1 => ValueOwners::One(owners.remove(0)),
            _ => ValueOwners::Several(owners),
        }
    }

    /// The name an enumeration was declared with, for any name that names it:
    /// its own, or that of an alias. `None` when `name` is not an enumeration.
    pub fn enumeration_declared_as(&self, name: &TypeName) -> Option<&TypeName> {
        if !self.is_enumeration(name) {
            return None;
        }
        let declaration = self.enumerations.declaration_of(self.id_of(name)?);
        self.entries.get(&declaration)?.name.as_ref()
    }
}

#[cfg(test)]
mod tests;
