//! Tests for the load-time container verifier, one case per invariant it checks.

use super::*;
use crate::id_types::{FbTypeId, FunctionId, InstanceId, VarIndex};
use crate::persistent_extent::PersistentExtent;
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
fn verify_load_when_user_fb_field_count_matches_type_descriptor_then_ok() {
    let mut container = consistent_container();
    container.type_section.as_mut().unwrap().fb_types[0].type_id = FbTypeId::new(0x1000);
    assert_eq!(verify_load(&container), Ok(()));
}

#[test]
fn verify_load_when_user_fb_field_count_differs_from_type_descriptor_then_violation() {
    let mut container = consistent_container();
    let section = container.type_section.as_mut().unwrap();
    section.fb_types[0].type_id = FbTypeId::new(0x1000);
    section.fb_types[0].fields.push(FieldEntry {
        field_type: FieldType::I32,
        field_extra: 0,
    });
    assert!(matches!(
        verify_load(&container),
        Err(LoadViolation::UserFbFieldCountMismatch {
            user_fields: 1,
            type_fields: 2,
            ..
        })
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

fn row(
    instance: u16,
    var_start: u16,
    var_count: u16,
    data_start: u32,
    data_len: u32,
) -> PersistentExtent {
    PersistentExtent {
        instance_id: InstanceId::new(instance),
        var_start,
        var_count,
        data_start,
        data_len,
    }
}

/// The consistent container (three variables) with a 64-byte data region
/// and the given extent rows.
fn container_with_extents(rows: &[PersistentExtent]) -> Container {
    let mut container = consistent_container();
    container.header.data_region_bytes = 64;
    container.type_section.as_mut().unwrap().persistent_extents = rows.to_vec();
    container
}

#[test]
fn verify_load_when_persistent_extent_inside_tables_then_ok() {
    let container = container_with_extents(&[row(0, 0, 2, 0, 40)]);
    assert_eq!(verify_load(&container), Ok(()));
}

#[test]
fn verify_load_when_persistent_extent_past_variable_table_then_violation() {
    let container = container_with_extents(&[row(0, 1, 3, 0, 8)]);
    assert!(matches!(
        verify_load(&container),
        Err(LoadViolation::PersistentExtentOutOfBounds { var_end: 4, .. })
    ));
}

#[test]
fn verify_load_when_persistent_extent_past_data_region_then_violation() {
    let container = container_with_extents(&[row(0, 0, 2, 32, 40)]);
    assert!(matches!(
        verify_load(&container),
        Err(LoadViolation::PersistentExtentOutOfBounds { data_end: 72, .. })
    ));
}

#[test]
fn verify_load_when_persistent_extent_data_end_overflows_u32_then_violation() {
    let container = container_with_extents(&[row(0, 0, 2, u32::MAX, u32::MAX)]);
    assert!(matches!(
        verify_load(&container),
        Err(LoadViolation::PersistentExtentOutOfBounds { .. })
    ));
}

#[test]
fn verify_load_when_persistent_extents_overlap_then_violation() {
    let mut container = container_with_extents(&[row(0, 0, 2, 0, 8), row(1, 1, 1, 8, 8)]);
    container
        .task_table
        .programs
        .push(container.task_table.programs[0]);
    container.task_table.programs[1].instance_id = InstanceId::new(1);
    assert!(matches!(
        verify_load(&container),
        Err(LoadViolation::PersistentExtentsOutOfOrder { instance_id }) if instance_id == InstanceId::new(1)
    ));
}

#[test]
fn verify_load_when_two_persistent_extents_follow_each_other_then_ok() {
    let mut container = container_with_extents(&[row(0, 0, 1, 0, 8), row(1, 1, 2, 8, 8)]);
    container
        .task_table
        .programs
        .push(container.task_table.programs[0]);
    container.task_table.programs[1].instance_id = InstanceId::new(1);
    assert_eq!(verify_load(&container), Ok(()));
}

#[test]
fn verify_load_when_persistent_extent_names_unknown_instance_then_violation() {
    let container = container_with_extents(&[row(7, 0, 2, 0, 8)]);
    assert!(matches!(
        verify_load(&container),
        Err(LoadViolation::PersistentExtentInstance { instance_id }) if instance_id == InstanceId::new(7)
    ));
}

#[test]
fn verify_load_when_two_persistent_extents_name_one_instance_then_violation() {
    let container = container_with_extents(&[row(0, 0, 1, 0, 8), row(0, 1, 1, 8, 8)]);
    assert!(matches!(
        verify_load(&container),
        Err(LoadViolation::PersistentExtentInstance { instance_id }) if instance_id == InstanceId::new(0)
    ));
}
