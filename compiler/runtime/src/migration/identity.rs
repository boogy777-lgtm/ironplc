//! The identity rule of the migration planner (ADR-0074): a migration needs a
//! stable ID for every persistent variable of both containers.
//!
//! The planner copies a value by ID. A persistent variable with no ID is not
//! copied, so the candidate's init image would give it its initial value
//! without a word. The rule refuses the candidate before any plan is built.
//!
//! The persistent variables are read from the container's own declaration of
//! what is persistent (ADR-0073), and every kind is one row of one rule:
//!
//! | Persistent variable | Where its identity is |
//! |---|---|
//! | each slot of each persistent extent row (globals, program variables, system and hidden slots) | the stable variable table |
//! | each field of each function block type descriptor | the function block field UID table |
//!
//! UID 0 is reserved and never an identity. A variable that exists on one side
//! only is a variable like any other here: on the candidate side it needs the ID
//! the engineering side assigned it, and the active side does not carry that ID
//! (the planner treats it as new), which is not a refusal.

use std::collections::HashSet;
use std::string::String;
use std::vec::Vec;

use ironplc_container::{Container, FbTypeDescriptor, FbTypeId, UserFbDescriptor, VarIndex};

use super::decision::debug_name;
use super::MigrationError;

/// Which container a variable without an ID belongs to.
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord)]
pub enum IdentitySide {
    /// The running application.
    Active,
    /// The staged candidate.
    Candidate,
}

/// What lacks an ID.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum IdentitySubject {
    /// A slot of the persistent extent.
    Variable { index: VarIndex },
    /// A field of a function block type.
    FbField { type_id: FbTypeId, ordinal: u8 },
}

/// One persistent variable that carries no stable ID.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct UnidentifiedVariable {
    /// The container it belongs to.
    pub side: IdentitySide,
    /// The variable.
    pub subject: IdentitySubject,
    /// The name the container's debug section gives it, when it carries one:
    /// `name` for a variable, `block.field` for a function block field (`field`
    /// when the block has no name).
    pub name: Option<String>,
}

impl core::fmt::Display for UnidentifiedVariable {
    fn fmt(&self, f: &mut core::fmt::Formatter<'_>) -> core::fmt::Result {
        if let Some(name) = &self.name {
            return write!(f, "{name}");
        }
        match self.subject {
            IdentitySubject::Variable { index } => write!(f, "variable {index}"),
            IdentitySubject::FbField { type_id, ordinal } => write!(
                f,
                "field {ordinal} of function block type {}",
                type_id.raw()
            ),
        }
    }
}

/// Refuses the pair of containers when a persistent variable of either has no
/// stable ID (see the module documentation).
pub(super) fn require_identified(
    base: &Container,
    candidate: &Container,
) -> Result<(), MigrationError> {
    let mut variables = Vec::new();
    unidentified(IdentitySide::Active, base, &mut variables);
    unidentified(IdentitySide::Candidate, candidate, &mut variables);
    if variables.is_empty() {
        Ok(())
    } else {
        Err(MigrationError::UnidentifiedVariables { variables })
    }
}

/// Appends the persistent variables of `container` that carry no ID, in table
/// order.
fn unidentified(side: IdentitySide, container: &Container, found: &mut Vec<UnidentifiedVariable>) {
    let Some(section) = container.type_section.as_ref() else {
        // Without a type section there is no table to name an ID in; the host
        // has refused such a container before a plan is built.
        return;
    };

    let identified: HashSet<VarIndex> = section
        .stable_vars
        .iter()
        .filter(|entry| entry.uid != 0)
        .map(|entry| entry.var_index)
        .collect();
    for extent in container.persistent_extents() {
        let first = u32::from(extent.var_start);
        for raw in first..first + u32::from(extent.var_count) {
            let Ok(raw) = u16::try_from(raw) else { break };
            let index = VarIndex::new(raw);
            if !identified.contains(&index) {
                found.push(UnidentifiedVariable {
                    side,
                    subject: IdentitySubject::Variable { index },
                    name: debug_name(container, index),
                });
            }
        }
    }

    let identified: HashSet<(FbTypeId, u8)> = section
        .fb_field_uids
        .iter()
        .filter(|entry| entry.uid != 0)
        .map(|entry| (entry.fb_type_id, entry.field_index))
        .collect();
    for descriptor in &section.fb_types {
        for ordinal in 0..field_count(descriptor) {
            if !identified.contains(&(descriptor.type_id, ordinal)) {
                found.push(UnidentifiedVariable {
                    side,
                    subject: IdentitySubject::FbField {
                        type_id: descriptor.type_id,
                        ordinal,
                    },
                    name: field_name(container, descriptor, ordinal),
                });
            }
        }
    }
}

/// The number of fields a function block type has, as a field ordinal can
/// count them.
fn field_count(descriptor: &FbTypeDescriptor) -> u8 {
    u8::try_from(descriptor.fields.len()).unwrap_or(u8::MAX)
}

/// `block.field` for a field the debug section names, `field` when it does not
/// name the block, nothing when it does not name the field.
///
/// The field is looked up at its slot in the body of the block, which is where
/// the debug section records it; the name describes the field and does not
/// identify it.
fn field_name(container: &Container, fb: &FbTypeDescriptor, ordinal: u8) -> Option<String> {
    let user = user_descriptor(container, fb.type_id)?;
    let field = debug_name(
        container,
        VarIndex::new(user.var_offset.saturating_add(u16::from(ordinal))),
    )?;
    let block = container
        .debug_section
        .as_ref()?
        .func_names
        .iter()
        .find(|entry| entry.function_id == user.function_id);
    Some(match block {
        Some(block) => format!("{}.{field}", block.name),
        None => field,
    })
}

/// The user function block descriptor of `type_id`, which holds the block's
/// function ID and the position of its slots.
fn user_descriptor(container: &Container, type_id: FbTypeId) -> Option<&UserFbDescriptor> {
    container
        .type_section
        .as_ref()?
        .user_fb_types
        .iter()
        .find(|descriptor| descriptor.type_id == type_id)
}
