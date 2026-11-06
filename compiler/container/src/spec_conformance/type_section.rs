//! Conformance tests for the type section: sub-table order, table layouts, descriptor sizes and field type encodings.

use std::vec;
use std::vec::Vec;

use spec_test_macro::spec_test;

use crate::builder::ContainerBuilder;
use crate::id_types::{FbTypeId, FunctionId, InstanceId, VarIndex};
use crate::persistent_extent::PersistentExtent;
use crate::type_section::{
    ArrayDescriptor, FbFieldUidEntry, FbTypeDescriptor, FieldEntry, FieldType, StableVarEntry,
    TypeSection, UserFbDescriptor, VarEntry,
};

// ---------------------------------------------------------------------------
// Container Format — Type Section (REQ-CF-container-008 through REQ-CF-container-009,
// REQ-CF-container-018 through REQ-CF-container-021)
// ---------------------------------------------------------------------------

fn write_type_section(section: &TypeSection) -> Vec<u8> {
    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();
    buf
}

/// REQ-CF-container-018: The type section is FB type descriptors, then array
/// descriptors, then user FB descriptors, then the variable table, then the
/// stable variable IDs, then the FB field UIDs, then the persistent extent
/// rows, then the retained runs, each behind a u16 count.
#[spec_test(REQ_CF_container_018)]
fn container_spec_req_cf_018_type_section_sub_table_order() {
    let section = TypeSection {
        fb_types: vec![FbTypeDescriptor {
            type_id: FbTypeId::new(0x0A),
            fields: vec![FieldEntry {
                field_type: FieldType::I32,
                field_extra: 0,
            }],
        }],
        array_descriptors: vec![ArrayDescriptor::new(FieldType::F64 as u8, 3, 0)],
        user_fb_types: vec![UserFbDescriptor {
            type_id: FbTypeId::new(0x0B),
            function_id: FunctionId::new(2),
            var_offset: 5,
            num_fields: 1,
        }],
        variable_table: vec![VarEntry {
            var_type: FieldType::Time,
            flags: 0,
            extra: 0,
        }],
        stable_vars: vec![StableVarEntry {
            var_index: VarIndex::new(7),
            uid: 0x0102_0304_0506_0708,
        }],
        fb_field_uids: vec![FbFieldUidEntry {
            fb_type_id: FbTypeId::new(0x0B),
            field_index: 0,
            uid: 0x1112_1314_1516_1718,
        }],
        persistent_extents: vec![],
        retained_runs: vec![],
    };
    let buf = write_type_section(&section);
    // fb count(2) + fb header(4) + one field(4) = 10, then array count(2) +
    // descriptor(12) = 24, then user count(2) + descriptor(8) = 34, then
    // variable count(2) + entry(4) = 40, then stable var count(2) +
    // entry(10) = 52, then FB field UID count(2) + entry(11) = 65, then
    // persistent extent count(2) = 67, then retained run count(2) = 69.
    assert_eq!(buf.len(), 69);
    assert_eq!(&buf[0..2], &1u16.to_le_bytes());
    assert_eq!(&buf[2..4], &0x0Au16.to_le_bytes());
    assert_eq!(&buf[10..12], &1u16.to_le_bytes());
    assert_eq!(buf[12], FieldType::F64 as u8);
    assert_eq!(&buf[24..26], &1u16.to_le_bytes());
    assert_eq!(&buf[26..28], &0x0Bu16.to_le_bytes());
    assert_eq!(&buf[34..36], &1u16.to_le_bytes());
    assert_eq!(&buf[36..40], &[FieldType::Time as u8, 0, 0, 0]);
    assert_eq!(&buf[40..42], &1u16.to_le_bytes());
    assert_eq!(&buf[42..44], &7u16.to_le_bytes());
    assert_eq!(&buf[44..52], &0x0102_0304_0506_0708u64.to_le_bytes());
    assert_eq!(&buf[52..54], &1u16.to_le_bytes());
    assert_eq!(&buf[54..56], &0x0Bu16.to_le_bytes());
    assert_eq!(buf[56], 0);
    assert_eq!(
        &buf[57..65],
        &[0x18, 0x17, 0x16, 0x15, 0x14, 0x13, 0x12, 0x11]
    );
    assert_eq!(&buf[65..67], &0u16.to_le_bytes());
    assert_eq!(&buf[67..69], &0u16.to_le_bytes());
}

/// REQ-CF-container-038: The persistent extent is a u16 count followed by
/// 14-byte rows — instance_id, var_start, var_count (u16 LE each),
/// data_start, data_len (u32 LE each) — and a container that declares no row
/// has the whole variable table and the whole data region as its extent.
#[spec_test(REQ_CF_container_038)]
fn container_spec_req_cf_038_persistent_extent_layout() {
    let section = TypeSection {
        persistent_extents: vec![PersistentExtent {
            instance_id: InstanceId::new(0x0102),
            var_start: 0x0304,
            var_count: 0x0506,
            data_start: 0x0708_090A,
            data_len: 0x0B0C_0D0E,
        }],
        ..Default::default()
    };
    let buf = write_type_section(&section);

    // Six empty counts(12), then the extent count at 12..14 and the row at
    // 14..28, then the empty retained run count.
    assert_eq!(buf.len(), 30);
    assert_eq!(&buf[12..14], &1u16.to_le_bytes());
    assert_eq!(
        &buf[14..28],
        &[0x02, 0x01, 0x04, 0x03, 0x06, 0x05, 0x0A, 0x09, 0x08, 0x07, 0x0E, 0x0D, 0x0C, 0x0B]
    );

    let undeclared = ContainerBuilder::new()
        .num_variables(3)
        .data_region_bytes(40)
        .add_function(FunctionId::INIT, &[0x8C], 0, 3, 0)
        .build();
    assert_eq!(
        undeclared.persistent_extents(),
        vec![PersistentExtent {
            instance_id: InstanceId::DEFAULT,
            var_start: 0,
            var_count: 3,
            data_start: 0,
            data_len: 40,
        }]
    );
}

/// REQ-CF-container-028: The stable variable ID table is a u16 count followed
/// by 10-byte `var_index`/`uid` entries in ascending `var_index` order.
#[spec_test(REQ_CF_container_028)]
fn container_spec_req_cf_028_stable_var_table_layout() {
    let section = TypeSection {
        stable_vars: vec![
            StableVarEntry {
                var_index: VarIndex::new(0x0102),
                uid: 0x0304_0506_0708_090A,
            },
            StableVarEntry {
                var_index: VarIndex::new(0x0B0C),
                uid: 0x0D0E_0F10_1112_1314,
            },
        ],
        ..Default::default()
    };
    let buf = write_type_section(&section);

    // Eight counts(16) + 2 entries * 10 = 36. The stable var count is the
    // fifth count, at bytes 8..10; entries follow at 10..20 and 20..30.
    assert_eq!(buf.len(), 36);
    assert_eq!(&buf[8..10], &2u16.to_le_bytes());
    assert_eq!(
        &buf[10..20],
        &[0x02, 0x01, 0x0A, 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03]
    );
    assert_eq!(
        &buf[20..30],
        &[0x0C, 0x0B, 0x14, 0x13, 0x12, 0x11, 0x10, 0x0F, 0x0E, 0x0D]
    );
}

/// REQ-CF-container-030: The FB field UID table is a u16 count followed by
/// 11-byte entries — fb_type_id (u16 LE), field_index (u8), uid (u64 LE) —
/// in ascending `(fb_type_id, field_index)` order.
#[spec_test(REQ_CF_container_030)]
fn container_spec_req_cf_030_fb_field_uid_table_layout() {
    let section = TypeSection {
        fb_field_uids: vec![
            FbFieldUidEntry {
                fb_type_id: FbTypeId::new(0x0B0C),
                field_index: 0x0D,
                uid: 0x1112_1314_1516_1718,
            },
            FbFieldUidEntry {
                fb_type_id: FbTypeId::new(0x0B0C),
                field_index: 0x0E,
                uid: 0x191A_1B1C_1D1E_1F20,
            },
        ],
        ..Default::default()
    };
    let buf = write_type_section(&section);

    // Eight counts(16) + 2 entries * 11 = 38. The FB field UID count is the
    // sixth count, at bytes 10..12; entries follow at 12..23 and 23..34, and
    // the seventh and eighth counts (no persistent extent rows, no retained runs)
    // close the section.
    assert_eq!(buf.len(), 38);
    assert_eq!(&buf[10..12], &2u16.to_le_bytes());
    assert_eq!(
        &buf[12..23],
        &[0x0C, 0x0B, 0x0D, 0x18, 0x17, 0x16, 0x15, 0x14, 0x13, 0x12, 0x11]
    );
    assert_eq!(
        &buf[23..34],
        &[0x0C, 0x0B, 0x0E, 0x20, 0x1F, 0x1E, 0x1D, 0x1C, 0x1B, 0x1A, 0x19]
    );
}

/// REQ-CF-container-019: An ArrayDescriptor is element_type u8, reserved u8,
/// total_elements u32, element_extra u16, element_stride u32 — 12 bytes.
#[spec_test(REQ_CF_container_019)]
fn container_spec_req_cf_019_array_descriptor_is_12_bytes() {
    let section = TypeSection {
        array_descriptors: vec![ArrayDescriptor {
            element_type: FieldType::String as u8,
            total_elements: 0x0102_0304,
            element_extra: 0x0506,
            element_stride: 0x0708_090A,
        }],
        ..Default::default()
    };
    let buf = write_type_section(&section);
    // fb count(2) + array count(2) + descriptor(12) + user count(2)
    //   + variable count(2) + stable var count(2) + FB field UID count(2)
    // + persistent extent count(2) + retained run count(2)
    assert_eq!(buf.len(), 28);
    assert_eq!(
        &buf[4..16],
        &[
            FieldType::String as u8,
            0,
            0x04,
            0x03,
            0x02,
            0x01,
            0x06,
            0x05,
            0x0A,
            0x09,
            0x08,
            0x07
        ]
    );
}

/// REQ-CF-container-020: A UserFbDescriptor is type_id u16, function_id u16,
/// var_offset u16, num_fields u8, reserved u8 — 8 bytes.
#[spec_test(REQ_CF_container_020)]
fn container_spec_req_cf_020_user_fb_descriptor_is_8_bytes() {
    let section = TypeSection {
        user_fb_types: vec![UserFbDescriptor {
            type_id: FbTypeId::new(0x0102),
            function_id: FunctionId::new(0x0304),
            var_offset: 0x0506,
            num_fields: 7,
        }],
        ..Default::default()
    };
    let buf = write_type_section(&section);
    // fb count(2) + array count(2) + user count(2) + descriptor(8)
    //   + variable count(2) + stable var count(2) + FB field UID count(2)
    // + persistent extent count(2) + retained run count(2)
    assert_eq!(buf.len(), 24);
    assert_eq!(&buf[6..14], &[0x02, 0x01, 0x04, 0x03, 0x06, 0x05, 7, 0]);
}

/// REQ-CF-container-021: An FB type descriptor is a 4-byte header (type_id
/// u16, num_fields u8, reserved u8) followed by its FieldEntry records.
#[spec_test(REQ_CF_container_021)]
fn container_spec_req_cf_021_fb_type_descriptor_header_is_4_bytes() {
    let section = TypeSection {
        fb_types: vec![FbTypeDescriptor {
            type_id: FbTypeId::new(0x0102),
            fields: vec![
                FieldEntry {
                    field_type: FieldType::I32,
                    field_extra: 0,
                },
                FieldEntry {
                    field_type: FieldType::String,
                    field_extra: 0x0708,
                },
            ],
        }],
        ..Default::default()
    };
    let buf = write_type_section(&section);
    // fb count(2) + header(4) + 2 fields(8) + array count(2) + user count(2)
    //   + variable count(2) + stable var count(2) + FB field UID count(2)
    // + persistent extent count(2) + retained run count(2)
    assert_eq!(buf.len(), 28);
    assert_eq!(&buf[2..6], &[0x02, 0x01, 2, 0]);
    assert_eq!(&buf[6..10], &[FieldType::I32 as u8, 0, 0, 0]);
    assert_eq!(&buf[10..14], &[FieldType::String as u8, 0, 0x08, 0x07]);
}

/// REQ-CF-container-008: Each FieldEntry is 4 bytes.
#[spec_test(REQ_CF_container_008)]
fn container_spec_req_cf_008_field_entry_is_4_bytes() {
    let section = TypeSection {
        fb_types: vec![FbTypeDescriptor {
            type_id: FbTypeId::new(0),
            fields: vec![FieldEntry {
                field_type: FieldType::I32,
                field_extra: 0,
            }],
        }],
        ..Default::default()
    };
    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();
    // fb_count(2) + type_id(2) + num_fields(1) + reserved(1) + field(4)
    //   + array_count(2) + user_fb_count(2) + variable_count(2)
    //   + stable_var_count(2) + fb_field_uid_count(2) + persistent extent count(2)
    //   + retained run count(2) = 24
    // The single field entry occupies exactly 4 bytes (bytes 6..10).
    assert_eq!(buf.len(), 24);
}

/// REQ-CF-container-009: FieldType/var_type encoding values 0 through 10.
#[spec_test(REQ_CF_container_009)]
fn container_spec_req_cf_009_field_type_encoding_values() {
    assert_eq!(FieldType::I32 as u8, 0);
    assert_eq!(FieldType::U32 as u8, 1);
    assert_eq!(FieldType::I64 as u8, 2);
    assert_eq!(FieldType::U64 as u8, 3);
    assert_eq!(FieldType::F32 as u8, 4);
    assert_eq!(FieldType::F64 as u8, 5);
    assert_eq!(FieldType::String as u8, 6);
    assert_eq!(FieldType::WString as u8, 7);
    assert_eq!(FieldType::FbInstance as u8, 8);
    assert_eq!(FieldType::Time as u8, 9);
    assert_eq!(FieldType::Slot as u8, 10);
    // Values 0-10 are valid; 11 is invalid
    for tag in 0..=10u8 {
        assert!(FieldType::from_u8(tag).is_ok());
    }
    assert!(FieldType::from_u8(11).is_err());
}

/// REQ-CF-container-040: The retained runs are a u16 count (zero when nothing
/// is retained) followed by 10-byte rows — kind u8, space u8, start and len
/// (u32 LE each) — and a container that declares no run retains nothing.
#[spec_test(REQ_CF_container_040)]
fn container_spec_req_cf_040_retained_run_layout() {
    use crate::retained_extent::{RetainKind, RetainedRun};

    let section = TypeSection {
        retained_runs: vec![RetainedRun::bytes(
            RetainKind::Persistent,
            0x0102_0304,
            0x0506_0708,
        )],
        ..Default::default()
    };
    let buf = write_type_section(&section);

    // Seven empty counts(14), then the run count at 14..16 and the row at
    // 16..26.
    assert_eq!(buf.len(), 26);
    assert_eq!(&buf[14..16], &1u16.to_le_bytes());
    assert_eq!(
        &buf[16..26],
        &[0x02, 0x01, 0x04, 0x03, 0x02, 0x01, 0x08, 0x07, 0x06, 0x05]
    );

    // Nothing declared: the count is zero, the reader sees no run, and a type
    // section that ends before the sub-table reads as no run either.
    let empty = write_type_section(&TypeSection::default());
    assert_eq!(&empty[14..16], &0u16.to_le_bytes());
    let truncated = TypeSection::read_from(&mut std::io::Cursor::new(&empty[..14])).unwrap();
    assert!(truncated.retained_runs.is_empty());
    let undeclared = ContainerBuilder::new()
        .num_variables(3)
        .data_region_bytes(40)
        .add_function(FunctionId::INIT, &[0x8C], 0, 3, 0)
        .build();
    assert!(undeclared.retained_runs().is_empty());
}

/// REQ-CF-container-041: A retained run whose kind byte is not 1 or 2, or
/// whose space byte is not 0 or 1, is refused by the reader.
#[spec_test(REQ_CF_container_041)]
fn container_spec_req_cf_041_undefined_kind_or_space_is_refused() {
    use crate::retained_extent::{RetainKind, RetainedRun};
    use crate::ContainerError;

    let read_with = |kind: u8, space: u8| {
        let mut row = RetainedRun::slots(RetainKind::Retain, 0, 1).to_bytes();
        row[0] = kind;
        row[1] = space;
        let mut buf = Vec::new();
        for _ in 0..7 {
            buf.extend_from_slice(&0u16.to_le_bytes());
        }
        buf.extend_from_slice(&1u16.to_le_bytes());
        buf.extend_from_slice(&row);
        TypeSection::read_from(&mut std::io::Cursor::new(&buf))
    };

    assert!(read_with(1, 0).is_ok());
    assert!(read_with(2, 1).is_ok());
    assert!(matches!(
        read_with(0, 0),
        Err(ContainerError::InvalidRetainKind(0))
    ));
    assert!(matches!(
        read_with(3, 0),
        Err(ContainerError::InvalidRetainKind(3))
    ));
    assert!(matches!(
        read_with(1, 2),
        Err(ContainerError::InvalidRetainSpace(2))
    ));
}

/// REQ-CF-container-042: The writer stores the retained runs sorted by space
/// and start, with empty runs dropped and adjacent runs of one kind merged,
/// whatever the order and the split in which they were declared.
#[spec_test(REQ_CF_container_042)]
fn container_spec_req_cf_042_retained_runs_are_stored_canonical() {
    use crate::retained_extent::{RetainKind, RetainedRun};

    let declared_in_pieces = ContainerBuilder::new()
        .num_variables(4)
        .data_region_bytes(64)
        .add_function(FunctionId::INIT, &[0x8C], 0, 4, 0)
        .add_retained_run(RetainedRun::bytes(RetainKind::Retain, 16, 8))
        .add_retained_run(RetainedRun::slots(RetainKind::Persistent, 3, 1))
        .add_retained_run(RetainedRun::bytes(RetainKind::Retain, 8, 8))
        .add_retained_run(RetainedRun::slots(RetainKind::Retain, 1, 1))
        .add_retained_run(RetainedRun::slots(RetainKind::Retain, 2, 0))
        .add_retained_run(RetainedRun::slots(RetainKind::Retain, 0, 1))
        .build();
    let declared_whole = ContainerBuilder::new()
        .num_variables(4)
        .data_region_bytes(64)
        .add_function(FunctionId::INIT, &[0x8C], 0, 4, 0)
        .add_retained_run(RetainedRun::slots(RetainKind::Retain, 0, 2))
        .add_retained_run(RetainedRun::slots(RetainKind::Persistent, 3, 1))
        .add_retained_run(RetainedRun::bytes(RetainKind::Retain, 8, 16))
        .build();

    assert_eq!(
        declared_in_pieces.retained_runs(),
        vec![
            RetainedRun::slots(RetainKind::Retain, 0, 2),
            RetainedRun::slots(RetainKind::Persistent, 3, 1),
            RetainedRun::bytes(RetainKind::Retain, 8, 16),
        ]
    );
    assert_eq!(
        write_type_section(declared_in_pieces.type_section.as_ref().unwrap()),
        write_type_section(declared_whole.type_section.as_ref().unwrap())
    );
}
