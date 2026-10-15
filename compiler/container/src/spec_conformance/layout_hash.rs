//! Conformance test for what the layout hash covers.

use std::vec;

use spec_test_macro::spec_test;

use crate::builder::ContainerBuilder;
use crate::id_types::{FbTypeId, FunctionId, InstanceId};
use crate::type_section::FieldType;
use crate::Container;

/// REQ-CF-container-039: The layout hash covers the persistent extent, the
/// variables in it, the arrays they name (by content) and the FB type
/// descriptors, and nothing else: working slots, arrays no persistent
/// variable names, the count of variables of the whole table and the user FB
/// descriptors do not change it.
#[spec_test(REQ_CF_container_039)]
fn container_spec_req_cf_039_layout_hash_covers_the_persistent_part_only() {
    use crate::persistent_extent::PersistentExtent;
    use crate::type_section::{
        FbTypeDescriptor, FieldEntry, UserFbDescriptor, VarEntry, VAR_FLAG_IS_ARRAY,
    };

    let build = || {
        let mut builder = ContainerBuilder::new();
        builder.add_array_descriptor(FieldType::F64 as u8, 9, 0);
        builder.add_array_descriptor(FieldType::I32 as u8, 4, 0);
        builder
            .num_variables(3)
            .data_region_bytes(64)
            .add_var_entry(VarEntry {
                var_type: FieldType::I32,
                flags: VAR_FLAG_IS_ARRAY,
                extra: 1,
            })
            .add_var_entry(VarEntry {
                var_type: FieldType::String,
                flags: 0,
                extra: 10,
            })
            .add_var_entry(VarEntry {
                var_type: FieldType::F32,
                flags: 0,
                extra: 0,
            })
            .add_fb_type(FbTypeDescriptor {
                type_id: FbTypeId::new(0x1000),
                fields: vec![FieldEntry {
                    field_type: FieldType::I32,
                    field_extra: 0,
                }],
            })
            .add_user_fb_type(UserFbDescriptor {
                type_id: FbTypeId::new(0x1000),
                function_id: FunctionId::new(2),
                var_offset: 2,
                num_fields: 1,
            })
            .add_persistent_extent(PersistentExtent {
                instance_id: InstanceId::DEFAULT,
                var_start: 0,
                var_count: 2,
                data_start: 0,
                data_len: 48,
            })
            .add_function(FunctionId::INIT, &[0x8C], 0, 3, 0)
            .build()
    };
    let hash_of = |edit: &dyn Fn(&mut Container)| {
        let mut container = build();
        edit(&mut container);
        container.compute_layout_hash()
    };
    let base = build().compute_layout_hash();

    // Outside the layout.
    let working_slot =
        hash_of(&|c| c.type_section.as_mut().unwrap().variable_table[2].var_type = FieldType::U64);
    let whole_table = hash_of(&|c| c.header.num_variables = 9);
    let unnamed_array =
        hash_of(&|c| c.type_section.as_mut().unwrap().array_descriptors[0].total_elements = 99);
    let user_descriptor =
        hash_of(&|c| c.type_section.as_mut().unwrap().user_fb_types[0].var_offset = 7);
    assert_eq!(base, working_slot);
    assert_eq!(base, whole_table);
    assert_eq!(base, unnamed_array);
    assert_eq!(base, user_descriptor);

    // Inside the layout.
    let persistent_variable =
        hash_of(&|c| c.type_section.as_mut().unwrap().variable_table[1].extra = 11);
    let named_array =
        hash_of(&|c| c.type_section.as_mut().unwrap().array_descriptors[1].total_elements = 5);
    let extent = hash_of(&|c| c.type_section.as_mut().unwrap().persistent_extents[0].data_len = 56);
    let fb_field = hash_of(&|c| {
        c.type_section.as_mut().unwrap().fb_types[0].fields[0].field_type = FieldType::F32
    });
    assert_ne!(base, persistent_variable);
    assert_ne!(base, named_array);
    assert_ne!(base, extent);
    assert_ne!(base, fb_field);
}
