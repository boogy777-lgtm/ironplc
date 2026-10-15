//! Load-time container verification (ADR-0006).
//!
//! [`verify_stack_balance`](crate::verify_stack_balance) checks the
//! stack-discipline rules a *compiler* must satisfy; this module is the other
//! half of the load-time verifier: it checks the *container metadata* a
//! crafted or corrupted file could get wrong, before the VM ever sees it.
//! ADR-0006 requires verification at load time; the checks here cover the
//! type-section invariants the format spec defines
//! (`specs/design/bytecode-container-format.md`):
//!
//! | Invariant | Variant |
//! |-----------|---------|
//! | Variable table count matches `header.num_variables` | [`LoadViolation::VariableTableCountMismatch`] |
//! | FB type descriptor count matches `header.num_fb_types` | [`LoadViolation::FbTypeCountMismatch`] |
//! | Variable entry flags use no reserved bits | [`LoadViolation::ReservedVariableFlags`] |
//! | Array variables reference an existing array descriptor | [`LoadViolation::ArrayDescriptorOutOfBounds`] |
//! | FB type IDs are distinct (both descriptor tables) | [`LoadViolation::DuplicateFbTypeId`] / [`LoadViolation::DuplicateUserFbTypeId`] |
//! | Array element type tags are defined | [`LoadViolation::InvalidArrayElementType`] |
//! | Stable variable IDs are in bounds and ascending | [`LoadViolation::StableVarIdOutOfBounds`] / [`LoadViolation::StableVarIdsOutOfOrder`] |
//! | User FB descriptors reference an existing function and a field range inside the variable table | [`LoadViolation::UserFbFunctionOutOfBounds`] / [`LoadViolation::UserFbVarsOutOfBounds`] |
//! | A user FB descriptor and the FB type descriptor of the same type ID agree on the number of fields | [`LoadViolation::UserFbFieldCountMismatch`] |
//! | FB field UIDs name an existing user FB type and a field inside it, ascending | [`LoadViolation::FbFieldUidUnknownType`] / [`LoadViolation::FbFieldUidFieldOutOfBounds`] / [`LoadViolation::FbFieldUidsOutOfOrder`] |
//! | Persistent extent rows lie inside the variable table and the data region, ascend without overlap, and name distinct program instances of the task table | [`LoadViolation::PersistentExtentOutOfBounds`] / [`LoadViolation::PersistentExtentsOutOfOrder`] / [`LoadViolation::PersistentExtentInstance`] |
//! | `layout_hash` recomputes over the type section | [`LoadViolation::LayoutHashMismatch`] |
//!
//! Note what is deliberately *not* checked: an `FbInstance` variable's
//! `extra` type ID is not matched against the descriptor tables, because the
//! ID may name a standard-library FB (TON, TOF, ...) whose descriptor the
//! container does not carry.
//!
//! Integrity hashes are *not* this module's job: [`crate::integrity`]
//! (ADR-0007) defines and checks `content_hash` and `debug_hash` against the
//! raw bytes in both the `std` reader ([`Container::read_from`]) and the
//! `no_std` reader ([`crate::ContainerRef`]). This module starts from an
//! already-parsed container.

use std::vec::Vec;

use crate::id_types::{FbTypeId, FunctionId, InstanceId, VarIndex};
use crate::type_section::{FieldType, TypeSection, VAR_FLAG_IS_ARRAY};
use crate::Container;

/// A violation of the load-time container invariants (ADR-0006).
///
/// Every variant names the offending item (variable index, type ID,
/// descriptor index) so a rejection points at the data responsible rather
/// than at a downstream symptom.
#[derive(Clone, Debug, PartialEq, Eq)]
pub enum LoadViolation {
    /// `header.content_hash` is nonzero but does not equal BLAKE3 over the
    /// type, constant and code sections.
    ContentHashMismatch,
    /// `header.debug_hash` is nonzero but does not equal BLAKE3 over the
    /// debug section. Non-fatal at load: the debug section is discarded.
    DebugHashMismatch,
    /// `header.layout_hash` is nonzero but does not recompute over the
    /// variable table, FB type descriptors and array descriptors.
    LayoutHashMismatch,
    /// The type section's variable table length differs from
    /// `header.num_variables`.
    VariableTableCountMismatch {
        /// Variable count claimed by the header.
        header: u16,
        /// Variable table entries actually present.
        table: usize,
    },
    /// The type section's FB type descriptor count differs from
    /// `header.num_fb_types`.
    FbTypeCountMismatch {
        /// Descriptor count claimed by the header.
        header: u16,
        /// FB type descriptors actually present.
        table: usize,
    },
    /// A variable entry sets flag bits other than `VAR_FLAG_IS_ARRAY`.
    ReservedVariableFlags {
        /// Index of the offending variable entry.
        var_index: VarIndex,
        /// The flags byte as read.
        flags: u8,
    },
    /// An array variable's `extra` names an array descriptor index that does
    /// not exist.
    ArrayDescriptorOutOfBounds {
        /// Index of the offending variable entry.
        var_index: VarIndex,
        /// The descriptor index carried in `extra`.
        index: u16,
        /// Number of array descriptors in the type section.
        count: usize,
    },
    /// Two FB type descriptors claim the same type ID.
    DuplicateFbTypeId {
        /// The duplicated type ID.
        type_id: FbTypeId,
    },
    /// An array descriptor's `element_type` is not a defined type tag.
    InvalidArrayElementType {
        /// Index of the offending array descriptor.
        descriptor_index: usize,
        /// The element type byte as read.
        element_type: u8,
    },
    /// A stable variable ID entry names a variable index outside the
    /// variable table.
    StableVarIdOutOfBounds {
        /// The variable index carried in the entry.
        var_index: u16,
        /// The header's `num_variables`.
        num_variables: u16,
    },
    /// Stable variable ID entries are not in strictly ascending
    /// `var_index` order (a duplicate or unsorted entry).
    StableVarIdsOutOfOrder {
        /// The out-of-order variable index.
        var_index: u16,
        /// The index that preceded it.
        previous: u16,
    },
    /// Two user FB descriptors claim the same type ID.
    DuplicateUserFbTypeId {
        /// The duplicated type ID.
        type_id: FbTypeId,
    },
    /// A user FB descriptor names a function ID that is not in the code
    /// section's function directory.
    UserFbFunctionOutOfBounds {
        /// The FB type ID owning the descriptor.
        type_id: FbTypeId,
        /// The function ID carried in the descriptor.
        function_id: FunctionId,
        /// Number of functions in the code section (`header.num_functions`).
        num_functions: u16,
    },
    /// A user FB descriptor's field range (`var_offset` + `num_fields`) runs
    /// past the end of the variable table.
    UserFbVarsOutOfBounds {
        /// The FB type ID owning the descriptor.
        type_id: FbTypeId,
        /// Variable table offset of the instance's fields.
        var_offset: u16,
        /// Number of data-region fields in an instance.
        num_fields: u8,
        /// The header's `num_variables`.
        num_variables: u16,
    },
    /// A user FB descriptor and the FB type descriptor of the same type ID
    /// disagree on the number of fields of an instance.
    UserFbFieldCountMismatch {
        /// The FB type ID both descriptors carry.
        type_id: FbTypeId,
        /// `num_fields` of the user FB descriptor.
        user_fields: u8,
        /// Number of fields the FB type descriptor lists.
        type_fields: usize,
    },
    /// A persistent extent row runs past the end of the variable table or of
    /// the data region.
    PersistentExtentOutOfBounds {
        /// The program instance owning the row.
        instance_id: InstanceId,
        /// The end (exclusive) of the row's variable run.
        var_end: u32,
        /// The header's `num_variables`.
        num_variables: u16,
        /// The end (exclusive) of the row's data run.
        data_end: u64,
        /// The header's `data_region_bytes`.
        data_region_bytes: u32,
    },
    /// Persistent extent rows are not in ascending order or overlap in the
    /// variable table or in the data region.
    PersistentExtentsOutOfOrder {
        /// The program instance owning the offending row.
        instance_id: InstanceId,
    },
    /// A persistent extent row names a program instance that the task table
    /// does not have, or that another row already names.
    PersistentExtentInstance {
        /// The program instance the row names.
        instance_id: InstanceId,
    },
    /// An FB field UID entry names a type ID that has no user FB descriptor.
    /// Standard-library FBs own their layouts and never carry entries, so an
    /// entry for one (or for a nonexistent type) can only be corrupt.
    FbFieldUidUnknownType {
        /// The type ID carried in the entry.
        type_id: FbTypeId,
    },
    /// An FB field UID entry names a field ordinal at or past the type's
    /// `num_fields`.
    FbFieldUidFieldOutOfBounds {
        /// The FB type ID owning the entry.
        type_id: FbTypeId,
        /// The field ordinal carried in the entry.
        field_index: u8,
        /// Number of data-region fields the descriptor declares.
        num_fields: u8,
    },
    /// FB field UID entries are not in strictly ascending
    /// `(fb_type_id, field_index)` order (a duplicate or unsorted entry).
    FbFieldUidsOutOfOrder {
        /// The out-of-order entry's type ID.
        type_id: FbTypeId,
        /// The out-of-order entry's field ordinal.
        field_index: u8,
        /// The type ID that preceded it.
        previous_type_id: FbTypeId,
        /// The field ordinal that preceded it.
        previous_field_index: u8,
    },
}

impl core::fmt::Display for LoadViolation {
    fn fmt(&self, f: &mut core::fmt::Formatter<'_>) -> core::fmt::Result {
        match self {
            LoadViolation::ContentHashMismatch => {
                write!(f, "content hash does not match the type, constant and code sections")
            }
            LoadViolation::DebugHashMismatch => {
                write!(f, "debug hash does not match the debug section")
            }
            LoadViolation::LayoutHashMismatch => {
                write!(f, "layout hash does not recompute over the type section")
            }
            LoadViolation::VariableTableCountMismatch { header, table } => write!(
                f,
                "variable table has {table} entries but the header declares {header}"
            ),
            LoadViolation::FbTypeCountMismatch { header, table } => write!(
                f,
                "FB type section has {table} descriptors but the header declares {header}"
            ),
            LoadViolation::ReservedVariableFlags { var_index, flags } => write!(
                f,
                "variable entry {var_index} sets reserved flag bits: 0x{flags:02X}"
            ),
            LoadViolation::ArrayDescriptorOutOfBounds {
                var_index,
                index,
                count,
            } => write!(
                f,
                "array variable {var_index} references descriptor {index} of {count}"
            ),
            LoadViolation::DuplicateFbTypeId { type_id } => {
                write!(f, "duplicate FB type descriptor ID {type_id}")
            }
            LoadViolation::InvalidArrayElementType {
                descriptor_index,
                element_type,
            } => write!(
                f,
                "array descriptor {descriptor_index} has undefined element type {element_type}"
            ),
            LoadViolation::StableVarIdOutOfBounds {
                var_index,
                num_variables,
            } => write!(
                f,
                "stable variable ID references variable {var_index} of {num_variables}"
            ),
            LoadViolation::StableVarIdsOutOfOrder {
                var_index,
                previous,
            } => write!(
                f,
                "stable variable IDs are not ascending: {var_index} follows {previous}"
            ),
            LoadViolation::DuplicateUserFbTypeId { type_id } => {
                write!(f, "duplicate user FB descriptor ID {type_id}")
            }
            LoadViolation::UserFbFunctionOutOfBounds {
                type_id,
                function_id,
                num_functions,
            } => write!(
                f,
                "user FB {type_id} references function {function_id} of {num_functions}"
            ),
            LoadViolation::UserFbVarsOutOfBounds {
                type_id,
                var_offset,
                num_fields,
                num_variables,
            } => write!(
                f,
                "user FB {type_id} fields at variable offset {var_offset} ({} fields) exceed {num_variables} variables",
                u32::from(*num_fields)
            ),
            LoadViolation::UserFbFieldCountMismatch {
                type_id,
                user_fields,
                type_fields,
            } => write!(
                f,
                "user FB {type_id} declares {user_fields} fields but its FB type descriptor lists {type_fields}"
            ),
            LoadViolation::PersistentExtentOutOfBounds {
                instance_id,
                var_end,
                num_variables,
                data_end,
                data_region_bytes,
            } => write!(
                f,
                "persistent extent of instance {instance_id} ends at variable {var_end} of {num_variables} and at data byte {data_end} of {data_region_bytes}"
            ),
            LoadViolation::PersistentExtentsOutOfOrder { instance_id } => write!(
                f,
                "persistent extent of instance {instance_id} does not follow the previous row"
            ),
            LoadViolation::PersistentExtentInstance { instance_id } => write!(
                f,
                "persistent extent names instance {instance_id}, which is not a distinct program instance"
            ),
            LoadViolation::FbFieldUidUnknownType { type_id } => {
                write!(f, "FB field UID references user FB {type_id}, which has no descriptor")
            }
            LoadViolation::FbFieldUidFieldOutOfBounds {
                type_id,
                field_index,
                num_fields,
            } => write!(
                f,
                "FB field UID for {type_id} references field {field_index} of {num_fields}"
            ),
            LoadViolation::FbFieldUidsOutOfOrder {
                type_id,
                field_index,
                previous_type_id,
                previous_field_index,
            } => write!(
                f,
                "FB field UIDs are not ascending: ({type_id}, {field_index}) follows ({previous_type_id}, {previous_field_index})"
            ),
        }
    }
}

/// Verifies the load-time container invariants (ADR-0006) on a parsed
/// container: type-section table consistency and, when the header carries a
/// nonzero `layout_hash`, that the hash recomputes.
///
/// A header with all-zero `layout_hash` was built but never serialized
/// (ADR-0052's hash contract) and skips the recompute. Containers without a
/// type section predate table-based verification and are checked only for the
/// layout hash.
pub fn verify_load(container: &Container) -> Result<(), LoadViolation> {
    if let Some(type_section) = &container.type_section {
        verify_type_section(container, type_section)?;
    }
    if container.header.layout_hash != [0u8; 32]
        && container.compute_layout_hash() != container.header.layout_hash
    {
        return Err(LoadViolation::LayoutHashMismatch);
    }
    Ok(())
}

/// Verifies the type section's sub-tables against the header and each other.
fn verify_type_section(
    container: &Container,
    type_section: &TypeSection,
) -> Result<(), LoadViolation> {
    let header = &container.header;

    if type_section.variable_table.len() as u16 != header.num_variables {
        return Err(LoadViolation::VariableTableCountMismatch {
            header: header.num_variables,
            table: type_section.variable_table.len(),
        });
    }
    if type_section.fb_types.len() as u16 != header.num_fb_types {
        return Err(LoadViolation::FbTypeCountMismatch {
            header: header.num_fb_types,
            table: type_section.fb_types.len(),
        });
    }

    for (index, entry) in type_section.variable_table.iter().enumerate() {
        let var_index = VarIndex::new(index as u16);
        if entry.flags & !VAR_FLAG_IS_ARRAY != 0 {
            return Err(LoadViolation::ReservedVariableFlags {
                var_index,
                flags: entry.flags,
            });
        }
        if entry.flags & VAR_FLAG_IS_ARRAY != 0
            && usize::from(entry.extra) >= type_section.array_descriptors.len()
        {
            return Err(LoadViolation::ArrayDescriptorOutOfBounds {
                var_index,
                index: entry.extra,
                count: type_section.array_descriptors.len(),
            });
        }
    }

    let mut fb_type_ids: Vec<FbTypeId> = Vec::with_capacity(type_section.fb_types.len());
    for descriptor in &type_section.fb_types {
        if fb_type_ids.contains(&descriptor.type_id) {
            return Err(LoadViolation::DuplicateFbTypeId {
                type_id: descriptor.type_id,
            });
        }
        fb_type_ids.push(descriptor.type_id);
    }

    for (descriptor_index, descriptor) in type_section.array_descriptors.iter().enumerate() {
        if FieldType::from_u8(descriptor.element_type).is_err() {
            return Err(LoadViolation::InvalidArrayElementType {
                descriptor_index,
                element_type: descriptor.element_type,
            });
        }
    }

    let mut previous: Option<u16> = None;
    for entry in &type_section.stable_vars {
        let raw = entry.var_index.raw();
        if raw >= header.num_variables {
            return Err(LoadViolation::StableVarIdOutOfBounds {
                var_index: raw,
                num_variables: header.num_variables,
            });
        }
        if let Some(prev) = previous {
            if raw <= prev {
                return Err(LoadViolation::StableVarIdsOutOfOrder {
                    var_index: raw,
                    previous: prev,
                });
            }
        }
        previous = Some(raw);
    }

    let mut user_fb_type_ids: Vec<FbTypeId> = Vec::with_capacity(type_section.user_fb_types.len());
    for descriptor in &type_section.user_fb_types {
        if user_fb_type_ids.contains(&descriptor.type_id) {
            return Err(LoadViolation::DuplicateUserFbTypeId {
                type_id: descriptor.type_id,
            });
        }
        user_fb_type_ids.push(descriptor.type_id);
        if descriptor.function_id.raw() >= header.num_functions {
            return Err(LoadViolation::UserFbFunctionOutOfBounds {
                type_id: descriptor.type_id,
                function_id: descriptor.function_id,
                num_functions: header.num_functions,
            });
        }
        if u32::from(descriptor.var_offset) + u32::from(descriptor.num_fields)
            > u32::from(header.num_variables)
        {
            return Err(LoadViolation::UserFbVarsOutOfBounds {
                type_id: descriptor.type_id,
                var_offset: descriptor.var_offset,
                num_fields: descriptor.num_fields,
                num_variables: header.num_variables,
            });
        }
    }

    for descriptor in &type_section.user_fb_types {
        let listed = type_section
            .fb_types
            .iter()
            .find(|fb_type| fb_type.type_id == descriptor.type_id);
        if let Some(fb_type) =
            listed.filter(|t| t.fields.len() != usize::from(descriptor.num_fields))
        {
            return Err(LoadViolation::UserFbFieldCountMismatch {
                type_id: descriptor.type_id,
                user_fields: descriptor.num_fields,
                type_fields: fb_type.fields.len(),
            });
        }
    }

    // FB field UIDs reference a user FB descriptor's field range, so the
    // descriptor table must be validated first (as it is above).
    let mut previous_field: Option<(FbTypeId, u8)> = None;
    for entry in &type_section.fb_field_uids {
        let field = (entry.fb_type_id, entry.field_index);
        if let Some((previous_type_id, previous_field_index)) = previous_field {
            let out_of_order = field.0.raw() < previous_type_id.raw()
                || (field.0 == previous_type_id && field.1 <= previous_field_index);
            if out_of_order {
                return Err(LoadViolation::FbFieldUidsOutOfOrder {
                    type_id: entry.fb_type_id,
                    field_index: entry.field_index,
                    previous_type_id,
                    previous_field_index,
                });
            }
        }
        previous_field = Some(field);

        let Some(descriptor) = type_section
            .user_fb_types
            .iter()
            .find(|d| d.type_id == entry.fb_type_id)
        else {
            return Err(LoadViolation::FbFieldUidUnknownType {
                type_id: entry.fb_type_id,
            });
        };
        if entry.field_index >= descriptor.num_fields {
            return Err(LoadViolation::FbFieldUidFieldOutOfBounds {
                type_id: entry.fb_type_id,
                field_index: entry.field_index,
                num_fields: descriptor.num_fields,
            });
        }
    }

    verify_persistent_extents(container, type_section)
}

/// Verifies the persistent extent rows against the tables they index and
/// against the task table. A container that declares no row has the default
/// extent (the whole tables), which holds by construction.
fn verify_persistent_extents(
    container: &Container,
    type_section: &TypeSection,
) -> Result<(), LoadViolation> {
    let header = &container.header;
    let mut var_end: u32 = 0;
    let mut data_end: u64 = 0;
    let mut instances: Vec<InstanceId> = Vec::with_capacity(type_section.persistent_extents.len());
    for row in &type_section.persistent_extents {
        let row_var_end = u32::from(row.var_start) + u32::from(row.var_count);
        let row_data_end = u64::from(row.data_start) + u64::from(row.data_len);
        if row_var_end > u32::from(header.num_variables)
            || row_data_end > u64::from(header.data_region_bytes)
        {
            return Err(LoadViolation::PersistentExtentOutOfBounds {
                instance_id: row.instance_id,
                var_end: row_var_end,
                num_variables: header.num_variables,
                data_end: row_data_end,
                data_region_bytes: header.data_region_bytes,
            });
        }
        if u32::from(row.var_start) < var_end || u64::from(row.data_start) < data_end {
            return Err(LoadViolation::PersistentExtentsOutOfOrder {
                instance_id: row.instance_id,
            });
        }
        var_end = row_var_end;
        data_end = row_data_end;

        let known = container
            .task_table
            .programs
            .iter()
            .any(|program| program.instance_id == row.instance_id);
        if !known || instances.contains(&row.instance_id) {
            return Err(LoadViolation::PersistentExtentInstance {
                instance_id: row.instance_id,
            });
        }
        instances.push(row.instance_id);
    }
    Ok(())
}

#[cfg(test)]
mod tests;
