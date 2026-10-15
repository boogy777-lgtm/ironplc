//! Unit tests for the state migration planner.
//!
//! The cases pin the per-UID classification, each reject reason, the
//! data-region copies, and the ADR-0059 FB layout rules; the type-change
//! policy and the ADR-0061 decision vocabulary live in [`decisions`]. The
//! end-to-end host behavior lives in `tests/migration_acceptance.rs`.

mod apply;
mod decisions;
mod error_display;
mod fb_layout;
mod identity;
mod instance_arrays;
mod variable_plans;

use std::sync::atomic::{AtomicU64, Ordering};

use super::*;
use ironplc_container::{
    Container, ContainerBuilder, FbFieldUidEntry, FbTypeDescriptor, FbTypeId, FieldEntry,
    FieldType, FunctionId, UserFbDescriptor,
};
use ironplc_vm::Slot;

/// The source of the IDs [`Identified::identified`] gives out. They start far
/// above the IDs the tests write by hand and never repeat, so an ID given to one
/// container is never shared with another and no copy follows from it.
static NEXT_GENERATED_UID: AtomicU64 = AtomicU64::new(1_000_000);

/// Completes the identity of a container the tests build by hand.
///
/// The tests of the child modules pin the rows of the planner under the identity rule
/// (ADR-0074). A hand-built container names a few IDs, the ones the test is
/// about, and leaves the other persistent variables and function block fields
/// without one; the rule would refuse those before the row is reached. The
/// rule has tests of its own in [`identity`].
trait Identified {
    /// The container, with a fresh ID for each persistent variable and each
    /// function block field that had none.
    fn identified(self) -> Self;
}

impl Identified for Container {
    fn identified(mut self) -> Self {
        let extents = self.persistent_extents();
        let Some(section) = self.type_section.as_mut() else {
            return self;
        };
        for extent in extents {
            let first = extent.var_start;
            for raw in first..first + extent.var_count {
                let index = VarIndex::new(raw);
                if !section
                    .stable_vars
                    .iter()
                    .any(|entry| entry.var_index == index)
                {
                    section.stable_vars.push(StableVarEntry {
                        var_index: index,
                        uid: NEXT_GENERATED_UID.fetch_add(1, Ordering::Relaxed),
                    });
                }
            }
        }
        section
            .stable_vars
            .sort_by_key(|entry| entry.var_index.raw());
        for descriptor in section.fb_types.clone() {
            for ordinal in 0..descriptor.fields.len() as u8 {
                let named = section.fb_field_uids.iter().any(|entry| {
                    entry.fb_type_id == descriptor.type_id && entry.field_index == ordinal
                });
                if !named {
                    section.fb_field_uids.push(FbFieldUidEntry {
                        fb_type_id: descriptor.type_id,
                        field_index: ordinal,
                        uid: NEXT_GENERATED_UID.fetch_add(1, Ordering::Relaxed),
                    });
                }
            }
        }
        section
            .fb_field_uids
            .sort_by_key(|entry| (entry.fb_type_id.raw(), entry.field_index));
        self
    }
}

/// An I32 variable-table entry.
fn i32_entry() -> VarEntry {
    VarEntry {
        var_type: FieldType::I32,
        flags: 0,
        extra: 0,
    }
}

/// A STRING variable-table entry with the given maximum length.
fn string_entry(max_length: u16) -> VarEntry {
    VarEntry {
        var_type: FieldType::String,
        flags: 0,
        extra: max_length,
    }
}

/// An array variable-table entry referencing `descriptor`.
fn array_entry(descriptor: u16) -> VarEntry {
    VarEntry {
        var_type: FieldType::I32,
        flags: VAR_FLAG_IS_ARRAY,
        extra: descriptor,
    }
}

/// An FB instance variable-table entry for `type_id`.
fn fb_entry(type_id: u16) -> VarEntry {
    VarEntry {
        var_type: FieldType::FbInstance,
        flags: 0,
        extra: type_id,
    }
}

/// A user FB descriptor for `type_id` with `fields` fields.
fn user_fb(type_id: u16, fields: u8) -> UserFbDescriptor {
    UserFbDescriptor {
        type_id: FbTypeId::new(type_id),
        function_id: FunctionId::new(2),
        var_offset: 0,
        num_fields: fields,
    }
}

/// A user FB descriptor whose fields map at `var_offset`.
fn user_fb_at(type_id: u16, var_offset: u16, fields: u8) -> UserFbDescriptor {
    UserFbDescriptor {
        var_offset,
        ..user_fb(type_id, fields)
    }
}

/// An FB field UID entry.
fn field_uid(type_id: u16, field_index: u8, uid: u64) -> FbFieldUidEntry {
    FbFieldUidEntry {
        fb_type_id: FbTypeId::new(type_id),
        field_index,
        uid,
    }
}

/// Builds a container whose type section carries `variables` and the given
/// `(var_index, uid)` stable variable IDs.
fn container(variables: &[VarEntry], stable: &[(u16, u64)]) -> Container {
    let mut builder = ContainerBuilder::new();
    for entry in variables {
        builder = builder.add_var_entry(entry.clone());
    }
    for (index, uid) in stable {
        builder = builder.add_stable_var(StableVarEntry {
            var_index: VarIndex::new(*index),
            uid: *uid,
        });
    }
    builder
        .num_variables(variables.len() as u16)
        .build()
        .identified()
}

/// Builds a container with one stable I32 array variable sized so its data
/// region can hold all `elements`.
fn array_container(elements: u32, data_region_bytes: u32) -> Container {
    let mut builder = ContainerBuilder::new();
    builder.add_array_descriptor(FieldType::I32 as u8, elements, 0);
    builder
        .add_var_entry(array_entry(0))
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .data_region_bytes(data_region_bytes)
        .build()
        .identified()
}

/// A buffer set with the given slots and data region, for direct applications.
fn buffers(vars: Vec<Slot>, data_region_len: usize) -> VmBuffers {
    VmBuffers {
        stack: Vec::new(),
        vars,
        data_region: vec![0u8; data_region_len],
        temp_buf: Vec::new(),
        tasks: Vec::new(),
        programs: Vec::new(),
        ready: Vec::new(),
        frames: Vec::new(),
    }
}
/// A container with one stable array variable of `element` with `elements`
/// elements, sized so its data region holds every element.
fn typed_array_container(element: FieldType, elements: u32, data_region_bytes: u32) -> Container {
    let mut builder = ContainerBuilder::new();
    builder.add_array_descriptor(element as u8, elements, 0);
    builder
        .add_var_entry(VarEntry {
            var_type: element,
            flags: VAR_FLAG_IS_ARRAY,
            extra: 0,
        })
        .add_stable_var(StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 7,
        })
        .num_variables(1)
        .data_region_bytes(data_region_bytes)
        .build()
        .identified()
}

/// An FB type descriptor for `type_id` with `fields` I32 fields.
fn fb_type(type_id: u16, fields: usize) -> FbTypeDescriptor {
    FbTypeDescriptor {
        type_id: FbTypeId::new(type_id),
        fields: (0..fields)
            .map(|_| FieldEntry {
                field_type: FieldType::I32,
                field_extra: 0,
            })
            .collect(),
    }
}
