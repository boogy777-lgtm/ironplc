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
//! | FB field UIDs name an existing user FB type and a field inside it, ascending | [`LoadViolation::FbFieldUidUnknownType`] / [`LoadViolation::FbFieldUidFieldOutOfBounds`] / [`LoadViolation::FbFieldUidsOutOfOrder`] |
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

use crate::id_types::{FbTypeId, FunctionId, VarIndex};
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

    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::id_types::{FbTypeId, FunctionId, VarIndex};
    use crate::test_support::round_trip;
    use crate::type_section::{
        FbTypeDescriptor, FieldEntry, StableVarEntry, UserFbDescriptor, VarEntry,
    };
    use crate::ContainerBuilder;
    use std::vec;

    /// A container whose type section exercises every check: two variables
    /// (one array), one FB type, one user FB and an ascending stable ID,
    /// plus a debug section so the debug hash is populated.
    fn consistent_container() -> Container {
        let mut builder = ContainerBuilder::new();
        builder.add_array_descriptor(FieldType::I32 as u8, 4, 0);
        builder
            .num_variables(3)
            .add_i32_constant(1)
            .add_function(FunctionId::INIT, &[0x8C], 1, 3, 0)
            .add_func_name(crate::debug_section::FuncNameEntry {
                function_id: FunctionId::INIT,
                name: "MAIN".into(),
            })
            .add_var_entry(VarEntry {
                var_type: FieldType::FbInstance,
                flags: 0,
                extra: 0x1000,
            })
            .add_var_entry(VarEntry {
                var_type: FieldType::I32,
                flags: VAR_FLAG_IS_ARRAY,
                extra: 0,
            })
            .add_var_entry(VarEntry {
                var_type: FieldType::I32,
                flags: 0,
                extra: 0,
            })
            .add_stable_var(StableVarEntry {
                var_index: VarIndex::new(1),
                uid: 7,
            })
            .add_stable_var(StableVarEntry {
                var_index: VarIndex::new(2),
                uid: 8,
            })
            .add_fb_type(FbTypeDescriptor {
                type_id: FbTypeId::new(0x0010),
                fields: vec![FieldEntry {
                    field_type: FieldType::I32,
                    field_extra: 0,
                }],
            })
            .add_user_fb_type(UserFbDescriptor {
                type_id: FbTypeId::new(0x1000),
                function_id: FunctionId::INIT,
                var_offset: 0,
                num_fields: 1,
            })
            .build()
    }

    #[test]
    fn verify_load_when_consistent_container_then_ok() {
        let container = round_trip(&consistent_container());
        assert_eq!(verify_load(&container), Ok(()));
    }

    #[test]
    fn verify_load_when_no_type_section_then_ok() {
        let container = crate::test_support::steel_thread_single_function_container();
        assert_eq!(verify_load(&container), Ok(()));
    }

    #[test]
    fn verify_load_when_in_memory_container_then_ok() {
        // A header that was built but never written carries zero hashes and
        // skips the layout-hash recompute (ADR-0052's hash contract).
        let container = consistent_container();
        assert_eq!(container.header.layout_hash, [0u8; 32]);
        assert_eq!(verify_load(&container), Ok(()));
    }

    #[test]
    fn verify_load_when_variable_table_count_mismatches_then_violation() {
        let mut container = consistent_container();
        container.header.num_variables = 2;
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::VariableTableCountMismatch {
                header: 2,
                table: 3
            })
        ));
    }

    #[test]
    fn verify_load_when_fb_type_count_mismatches_then_violation() {
        let mut container = consistent_container();
        container.header.num_fb_types = 0;
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::FbTypeCountMismatch {
                header: 0,
                table: 1
            })
        ));
    }

    #[test]
    fn verify_load_when_reserved_variable_flags_then_violation() {
        let mut container = consistent_container();
        container.type_section.as_mut().unwrap().variable_table[2].flags = 0x02;
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::ReservedVariableFlags { var_index, flags: 0x02 })
                if var_index == VarIndex::new(2)
        ));
    }

    #[test]
    fn verify_load_when_array_descriptor_out_of_bounds_then_violation() {
        let mut container = consistent_container();
        container.type_section.as_mut().unwrap().variable_table[1].extra = 1;
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::ArrayDescriptorOutOfBounds { var_index, index: 1, count: 1 })
                if var_index == VarIndex::new(1)
        ));
    }

    #[test]
    fn verify_load_when_duplicate_fb_type_id_then_violation() {
        let mut container = consistent_container();
        let ts = container.type_section.as_mut().unwrap();
        ts.fb_types.push(ts.fb_types[0].clone());
        container.header.num_fb_types = 2;
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::DuplicateFbTypeId { type_id }) if type_id == FbTypeId::new(0x0010)
        ));
    }

    #[test]
    fn verify_load_when_invalid_array_element_type_then_violation() {
        let mut container = consistent_container();
        container.type_section.as_mut().unwrap().array_descriptors[0].element_type = 42;
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::InvalidArrayElementType {
                descriptor_index: 0,
                element_type: 42
            })
        ));
    }

    #[test]
    fn verify_load_when_stable_var_id_out_of_bounds_then_violation() {
        let mut container = consistent_container();
        container.type_section.as_mut().unwrap().stable_vars[0].var_index = VarIndex::new(3);
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::StableVarIdOutOfBounds {
                var_index: 3,
                num_variables: 3
            })
        ));
    }

    #[test]
    fn verify_load_when_stable_var_ids_unsorted_then_violation() {
        let mut container = consistent_container();
        container.type_section.as_mut().unwrap().stable_vars[0].var_index = VarIndex::new(2);
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::StableVarIdsOutOfOrder {
                var_index: 2,
                previous: 2
            })
        ));
    }

    #[test]
    fn verify_load_when_duplicate_user_fb_type_id_then_violation() {
        let mut container = consistent_container();
        let ts = container.type_section.as_mut().unwrap();
        ts.user_fb_types.push(ts.user_fb_types[0]);
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::DuplicateUserFbTypeId { type_id }) if type_id == FbTypeId::new(0x1000)
        ));
    }

    #[test]
    fn verify_load_when_user_fb_function_out_of_bounds_then_violation() {
        let mut container = consistent_container();
        container.type_section.as_mut().unwrap().user_fb_types[0].function_id = FunctionId::new(99);
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::UserFbFunctionOutOfBounds {
                type_id,
                function_id,
                num_functions: 1
            }) if type_id == FbTypeId::new(0x1000) && function_id == FunctionId::new(99)
        ));
    }

    #[test]
    fn verify_load_when_user_fb_vars_out_of_bounds_then_violation() {
        let mut container = consistent_container();
        container.type_section.as_mut().unwrap().user_fb_types[0].var_offset = 3;
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::UserFbVarsOutOfBounds {
                type_id,
                var_offset: 3,
                num_fields: 1,
                num_variables: 3
            }) if type_id == FbTypeId::new(0x1000)
        ));
    }

    #[test]
    fn verify_load_when_fb_field_uids_consistent_then_ok() {
        let mut container = consistent_container();
        container.type_section.as_mut().unwrap().fb_field_uids.push(
            crate::type_section::FbFieldUidEntry {
                fb_type_id: FbTypeId::new(0x1000),
                field_index: 0,
                uid: 77,
            },
        );
        assert_eq!(verify_load(&container), Ok(()));
    }

    #[test]
    fn verify_load_when_fb_field_uid_unknown_type_then_violation() {
        let mut container = consistent_container();
        container.type_section.as_mut().unwrap().fb_field_uids.push(
            crate::type_section::FbFieldUidEntry {
                fb_type_id: FbTypeId::new(0x1001),
                field_index: 0,
                uid: 77,
            },
        );
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::FbFieldUidUnknownType { type_id })
                if type_id == FbTypeId::new(0x1001)
        ));
    }

    #[test]
    fn verify_load_when_fb_field_uid_field_out_of_bounds_then_violation() {
        let mut container = consistent_container();
        container.type_section.as_mut().unwrap().fb_field_uids.push(
            crate::type_section::FbFieldUidEntry {
                fb_type_id: FbTypeId::new(0x1000),
                field_index: 1,
                uid: 77,
            },
        );
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::FbFieldUidFieldOutOfBounds {
                type_id,
                field_index: 1,
                num_fields: 1
            }) if type_id == FbTypeId::new(0x1000)
        ));
    }

    #[test]
    fn verify_load_when_fb_field_uids_unsorted_then_violation() {
        let mut container = consistent_container();
        let uids = &mut container.type_section.as_mut().unwrap().fb_field_uids;
        uids.push(crate::type_section::FbFieldUidEntry {
            fb_type_id: FbTypeId::new(0x1000),
            field_index: 0,
            uid: 77,
        });
        uids.push(crate::type_section::FbFieldUidEntry {
            fb_type_id: FbTypeId::new(0x1000),
            field_index: 0,
            uid: 78,
        });
        let result = verify_load(&container);
        assert!(matches!(
            result,
            Err(LoadViolation::FbFieldUidsOutOfOrder {
                type_id,
                field_index: 0,
                previous_type_id,
                previous_field_index: 0
            }) if type_id == FbTypeId::new(0x1000) && previous_type_id == FbTypeId::new(0x1000)
        ));
    }

    #[test]
    fn verify_load_when_layout_hash_mismatches_then_violation() {
        let mut container = consistent_container();
        container.header.layout_hash = [0xFF; 32];
        let result = verify_load(&container);
        assert!(matches!(result, Err(LoadViolation::LayoutHashMismatch)));
    }
}
