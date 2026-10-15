//! Tests for the type section: binary round-trips, section sizing and malformed-input rejection.

use super::*;
use std::io::Cursor;
use std::vec;
use std::vec::Vec;

#[test]
fn type_section_write_read_when_empty_then_roundtrips() {
    let section = TypeSection::default();

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert!(decoded.fb_types.is_empty());
    assert!(decoded.array_descriptors.is_empty());
    assert!(decoded.variable_table.is_empty());
    assert!(decoded.stable_vars.is_empty());
    assert!(decoded.fb_field_uids.is_empty());
}

#[test]
fn type_section_write_read_when_ton_descriptor_then_roundtrips() {
    let section = TypeSection {
        fb_types: vec![FbTypeDescriptor {
            type_id: FbTypeId::new(0x0010),
            fields: vec![
                FieldEntry {
                    field_type: FieldType::I32,
                    field_extra: 0,
                },
                FieldEntry {
                    field_type: FieldType::Time,
                    field_extra: 0,
                },
                FieldEntry {
                    field_type: FieldType::I32,
                    field_extra: 0,
                },
                FieldEntry {
                    field_type: FieldType::Time,
                    field_extra: 0,
                },
                FieldEntry {
                    field_type: FieldType::Time,
                    field_extra: 0,
                },
                FieldEntry {
                    field_type: FieldType::I32,
                    field_extra: 0,
                },
            ],
        }],
        array_descriptors: vec![],
        user_fb_types: vec![],
        variable_table: vec![],
        stable_vars: vec![],
        fb_field_uids: vec![],
        persistent_extents: vec![],
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert_eq!(decoded.fb_types.len(), 1);
    let desc = &decoded.fb_types[0];
    assert_eq!(desc.type_id, FbTypeId::new(0x0010));
    assert_eq!(desc.fields.len(), 6);
    assert_eq!(desc.fields[0].field_type, FieldType::I32);
    assert_eq!(desc.fields[1].field_type, FieldType::Time);
    assert_eq!(desc.fields[2].field_type, FieldType::I32);
    assert_eq!(desc.fields[3].field_type, FieldType::Time);
    assert_eq!(desc.fields[4].field_type, FieldType::Time);
    assert_eq!(desc.fields[5].field_type, FieldType::I32);
}

#[test]
fn type_section_write_read_when_array_descriptors_then_roundtrips() {
    let section = TypeSection {
        fb_types: vec![],
        array_descriptors: vec![
            ArrayDescriptor::new(FieldType::I32 as u8, 10, 0),
            ArrayDescriptor::new(FieldType::F64 as u8, 32768, 0),
        ],
        user_fb_types: vec![],
        variable_table: vec![],
        stable_vars: vec![],
        fb_field_uids: vec![],
        persistent_extents: vec![],
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert!(decoded.fb_types.is_empty());
    assert_eq!(decoded.array_descriptors.len(), 2);
    assert_eq!(
        decoded.array_descriptors[0].element_type,
        FieldType::I32 as u8
    );
    assert_eq!(decoded.array_descriptors[0].total_elements, 10);
    assert_eq!(
        decoded.array_descriptors[1].element_type,
        FieldType::F64 as u8
    );
    assert_eq!(decoded.array_descriptors[1].total_elements, 32768);
}

#[test]
fn type_section_write_read_when_fb_and_array_descriptors_then_roundtrips() {
    let section = TypeSection {
        fb_types: vec![FbTypeDescriptor {
            type_id: FbTypeId::new(1),
            fields: vec![FieldEntry {
                field_type: FieldType::I32,
                field_extra: 0,
            }],
        }],
        array_descriptors: vec![ArrayDescriptor::new(FieldType::U32 as u8, 100, 0)],
        user_fb_types: vec![],
        variable_table: vec![],
        stable_vars: vec![],
        fb_field_uids: vec![],
        persistent_extents: vec![],
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert_eq!(decoded.fb_types.len(), 1);
    assert_eq!(decoded.fb_types[0].type_id, FbTypeId::new(1));
    assert_eq!(decoded.array_descriptors.len(), 1);
    assert_eq!(
        decoded.array_descriptors[0].element_type,
        FieldType::U32 as u8
    );
    assert_eq!(decoded.array_descriptors[0].total_elements, 100);
}

#[test]
fn section_size_when_empty_then_returns_header_counts_only() {
    let section = TypeSection::default();
    // 2 bytes for each of the seven sub-table counts
    assert_eq!(section.section_size(), 14);
}

#[test]
fn section_size_when_array_descriptors_then_includes_descriptor_bytes() {
    let section = TypeSection {
        fb_types: vec![],
        array_descriptors: vec![
            ArrayDescriptor::new(0, 10, 0),
            ArrayDescriptor::new(4, 20, 0),
        ],
        user_fb_types: vec![],
        variable_table: vec![],
        stable_vars: vec![],
        fb_field_uids: vec![],
        persistent_extents: vec![],
    };
    // 7 counts(14) + 2 * 12 (descriptors) = 38
    assert_eq!(section.section_size(), 38);
}

fn read_single_descriptor(desc: ArrayDescriptor) -> Result<TypeSection, ContainerError> {
    let section = TypeSection {
        array_descriptors: vec![desc],
        ..Default::default()
    };
    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();
    TypeSection::read_from(&mut Cursor::new(&buf))
}

#[test]
fn array_descriptor_new_when_string_then_stride_spans_header_and_code_units() {
    let narrow = ArrayDescriptor::new(FieldType::String as u8, 3, 10);
    assert_eq!(
        narrow.element_stride(),
        crate::STRING_HEADER_BYTES as u32 + 10
    );
    let wide = ArrayDescriptor::new(FieldType::WString as u8, 3, 10);
    assert_eq!(
        wide.element_stride(),
        crate::STRING_HEADER_BYTES as u32 + 20
    );
}

#[test]
fn array_descriptor_new_when_primitive_then_stride_is_one_slot() {
    let desc = ArrayDescriptor::new(FieldType::I32 as u8, 3, 0);
    assert_eq!(desc.element_stride(), SLOT_BYTES);
}

#[test]
fn type_section_write_read_when_strided_string_descriptor_then_roundtrips() {
    let desc = ArrayDescriptor {
        element_type: FieldType::String as u8,
        total_elements: 6,
        element_extra: 50,
        element_stride: 80,
    };
    let decoded = read_single_descriptor(desc).unwrap();
    assert_eq!(decoded.array_descriptors, vec![desc]);
}

#[test]
fn type_section_read_when_string_stride_below_element_size_then_error() {
    let desc = ArrayDescriptor {
        element_type: FieldType::String as u8,
        total_elements: 6,
        element_extra: 50,
        element_stride: crate::STRING_HEADER_BYTES as u32 + 49,
    };
    assert!(matches!(
        read_single_descriptor(desc),
        Err(ContainerError::InvalidArrayStride {
            element_type: 6,
            ..
        })
    ));
}

#[test]
fn type_section_read_when_primitive_stride_not_one_slot_then_error() {
    let desc = ArrayDescriptor {
        element_type: FieldType::I32 as u8,
        total_elements: 6,
        element_extra: 0,
        element_stride: 16,
    };
    assert!(matches!(
        read_single_descriptor(desc),
        Err(ContainerError::InvalidArrayStride {
            element_type: 0,
            element_stride: 16,
        })
    ));
}

#[test]
fn type_section_write_read_when_user_fb_descriptors_then_roundtrips() {
    let section = TypeSection {
        fb_types: vec![],
        array_descriptors: vec![],
        user_fb_types: vec![
            UserFbDescriptor {
                type_id: FbTypeId::new(0x1000),
                function_id: FunctionId::new(2),
                var_offset: 4,
                num_fields: 3,
            },
            UserFbDescriptor {
                type_id: FbTypeId::new(0x1001),
                function_id: FunctionId::new(3),
                var_offset: 7,
                num_fields: 5,
            },
        ],
        variable_table: vec![],
        stable_vars: vec![],
        fb_field_uids: vec![],
        persistent_extents: vec![],
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert_eq!(decoded.user_fb_types.len(), 2);
    assert_eq!(decoded.user_fb_types[0].type_id, FbTypeId::new(0x1000));
    assert_eq!(decoded.user_fb_types[0].function_id, FunctionId::new(2));
    assert_eq!(decoded.user_fb_types[0].var_offset, 4);
    assert_eq!(decoded.user_fb_types[0].num_fields, 3);
    assert_eq!(decoded.user_fb_types[1].type_id, FbTypeId::new(0x1001));
    assert_eq!(decoded.user_fb_types[1].function_id, FunctionId::new(3));
    assert_eq!(decoded.user_fb_types[1].var_offset, 7);
    assert_eq!(decoded.user_fb_types[1].num_fields, 5);
}

#[test]
fn type_section_write_read_when_variable_table_then_roundtrips() {
    let entries = vec![
        VarEntry {
            var_type: FieldType::I32,
            flags: 0,
            extra: 0,
        },
        VarEntry {
            var_type: FieldType::String,
            flags: 0,
            extra: 80,
        },
        VarEntry {
            var_type: FieldType::FbInstance,
            flags: 0,
            extra: 7,
        },
        VarEntry {
            var_type: FieldType::F64,
            flags: VAR_FLAG_IS_ARRAY,
            extra: 2,
        },
    ];
    let section = TypeSection {
        variable_table: entries.clone(),
        ..Default::default()
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert_eq!(decoded.variable_table, entries);
}

#[test]
fn section_size_when_variable_table_then_includes_entry_bytes() {
    let section = TypeSection {
        variable_table: vec![
            VarEntry {
                var_type: FieldType::I32,
                flags: 0,
                extra: 0,
            },
            VarEntry {
                var_type: FieldType::F64,
                flags: VAR_FLAG_IS_ARRAY,
                extra: 3,
            },
        ],
        ..Default::default()
    };
    // 7 counts(14) + 2 entries * 4 = 22
    assert_eq!(section.section_size(), 22);
}

#[test]
fn type_section_write_read_when_stable_vars_then_roundtrips_in_order() {
    let entries = vec![
        StableVarEntry {
            var_index: VarIndex::new(0),
            uid: 0x0102_0304_0506_0708,
        },
        StableVarEntry {
            var_index: VarIndex::new(2),
            uid: 1,
        },
        StableVarEntry {
            var_index: VarIndex::new(7),
            uid: u64::MAX,
        },
    ];
    let section = TypeSection {
        stable_vars: entries.clone(),
        ..Default::default()
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert_eq!(decoded.stable_vars, entries);
    assert_eq!(
        decoded
            .stable_vars
            .iter()
            .map(|e| e.var_index)
            .collect::<Vec<_>>(),
        vec![VarIndex::new(0), VarIndex::new(2), VarIndex::new(7)]
    );
}

#[test]
fn section_size_when_stable_vars_then_includes_entry_bytes() {
    let section = TypeSection {
        stable_vars: vec![
            StableVarEntry {
                var_index: VarIndex::new(0),
                uid: 1,
            },
            StableVarEntry {
                var_index: VarIndex::new(1),
                uid: 2,
            },
        ],
        ..Default::default()
    };
    // 7 counts(14) + 2 entries * 10 = 34
    assert_eq!(section.section_size(), 34);
}

#[test]
fn type_section_read_from_when_no_stable_vars_then_empty() {
    // Build a type section payload that stops after the variable table,
    // simulating a container written before format v5. The reader should
    // treat the missing stable variable ID sub-table as zero entries.
    let mut buf = Vec::new();
    buf.extend_from_slice(&0u16.to_le_bytes()); // FB count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // array count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // user FB count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // variable count = 0

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert!(decoded.variable_table.is_empty());
    assert!(decoded.stable_vars.is_empty());
}

#[test]
fn type_section_read_from_when_no_fb_field_uids_then_empty() {
    // Build a type section payload that stops after the stable variable
    // IDs, simulating a container written before format v6. The reader
    // should treat the missing FB field UID sub-table as zero entries.
    let mut buf = Vec::new();
    buf.extend_from_slice(&0u16.to_le_bytes()); // FB count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // array count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // user FB count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // variable count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // stable var count = 0

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert!(decoded.stable_vars.is_empty());
    assert!(decoded.fb_field_uids.is_empty());
}

#[test]
fn type_section_write_read_when_fb_field_uids_then_roundtrips_in_order() {
    let entries = vec![
        FbFieldUidEntry {
            fb_type_id: FbTypeId::new(0x1000),
            field_index: 0,
            uid: 0x0102_0304_0506_0708,
        },
        FbFieldUidEntry {
            fb_type_id: FbTypeId::new(0x1000),
            field_index: 2,
            uid: 1,
        },
        FbFieldUidEntry {
            fb_type_id: FbTypeId::new(0x1001),
            field_index: 0,
            uid: u64::MAX,
        },
    ];
    let section = TypeSection {
        fb_field_uids: entries.clone(),
        persistent_extents: vec![],
        ..Default::default()
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert_eq!(decoded.fb_field_uids, entries);
}

#[test]
fn section_size_when_fb_field_uids_then_includes_entry_bytes() {
    let section = TypeSection {
        fb_field_uids: vec![
            FbFieldUidEntry {
                fb_type_id: FbTypeId::new(0x1000),
                field_index: 0,
                uid: 1,
            },
            FbFieldUidEntry {
                fb_type_id: FbTypeId::new(0x1000),
                field_index: 1,
                uid: 2,
            },
        ],
        ..Default::default()
    };
    // 7 counts(14) + 2 entries * 11 = 36
    assert_eq!(section.section_size(), 36);
}

#[test]
fn type_section_read_from_when_stable_var_entry_truncated_then_error() {
    // A count of one entry with no entry bytes must fail, not silently
    // read a partial entry.
    let mut buf = Vec::new();
    buf.extend_from_slice(&0u16.to_le_bytes()); // FB count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // array count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // user FB count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // variable count = 0
    buf.extend_from_slice(&1u16.to_le_bytes()); // stable var count = 1

    let mut cursor = Cursor::new(&buf);
    let result = TypeSection::read_from(&mut cursor);

    assert!(matches!(result, Err(ContainerError::Io(_))));
}

#[test]
fn type_section_read_from_when_no_variable_table_then_empty_variable_table() {
    // Build a type section payload that stops after the user FB count,
    // simulating a legacy container without the variable table sub-table.
    // The reader should treat the missing data as zero entries.
    let mut buf = Vec::new();
    buf.extend_from_slice(&0u16.to_le_bytes()); // FB count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // array count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // user FB count = 0

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert!(decoded.variable_table.is_empty());
}

#[test]
fn field_type_from_u8_when_invalid_then_returns_error() {
    assert!(matches!(
        FieldType::from_u8(42),
        Err(ContainerError::InvalidFieldType(42))
    ));
}

#[test]
fn type_section_section_size_when_fb_types_with_fields_then_includes_field_bytes() {
    let section = TypeSection {
        fb_types: vec![FbTypeDescriptor {
            type_id: FbTypeId::new(0x20),
            fields: vec![
                FieldEntry {
                    field_type: FieldType::I32,
                    field_extra: 0,
                },
                FieldEntry {
                    field_type: FieldType::F64,
                    field_extra: 0,
                },
                FieldEntry {
                    field_type: FieldType::String,
                    field_extra: 80,
                },
            ],
        }],
        array_descriptors: vec![],
        user_fb_types: vec![],
        variable_table: vec![],
        stable_vars: vec![],
        fb_field_uids: vec![],
        persistent_extents: vec![],
    };

    // Header: 7 counts * 2 = 14
    // Per descriptor: 4 (header) + 3 fields * 4 = 16
    // Total: 14 + 16 = 30
    assert_eq!(section.section_size(), 30);

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();
    assert_eq!(buf.len() as u32, section.section_size());
}

#[test]
fn type_section_read_from_when_no_user_fb_descriptor_then_empty_user_fb_types() {
    // Build a type section payload that stops after the array-count
    // field, simulating a legacy container without the user FB descriptor
    // sub-table. The reader should treat the missing data as zero
    // descriptors rather than erroring.
    let mut buf = Vec::new();
    buf.extend_from_slice(&0u16.to_le_bytes()); // FB count = 0
    buf.extend_from_slice(&0u16.to_le_bytes()); // array count = 0

    let mut cursor = Cursor::new(&buf);
    let decoded = TypeSection::read_from(&mut cursor).unwrap();

    assert!(decoded.fb_types.is_empty());
    assert!(decoded.array_descriptors.is_empty());
    assert!(decoded.user_fb_types.is_empty());
    assert!(decoded.variable_table.is_empty());
}

#[test]
fn type_section_write_read_when_persistent_extents_then_roundtrips() {
    let section = TypeSection {
        persistent_extents: vec![PersistentExtent {
            instance_id: crate::id_types::InstanceId::new(1),
            var_start: 2,
            var_count: 3,
            data_start: 16,
            data_len: 24,
        }],
        ..Default::default()
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();
    let decoded = TypeSection::read_from(&mut Cursor::new(&buf)).unwrap();

    assert_eq!(decoded.persistent_extents, section.persistent_extents);
    assert_eq!(buf.len() as u32, section.section_size());
}

#[test]
fn type_section_read_from_when_no_persistent_extents_then_no_rows() {
    // A type section that ends after the FB field UIDs has no extent
    // rows; the container then has the whole tables as its extent.
    let mut buf = Vec::new();
    for _ in 0..6 {
        buf.extend_from_slice(&0u16.to_le_bytes());
    }

    let decoded = TypeSection::read_from(&mut Cursor::new(&buf)).unwrap();

    assert!(decoded.persistent_extents.is_empty());
}
