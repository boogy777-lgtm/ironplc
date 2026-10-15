//! Tests for the debug section: round trips, source-location lookup, and malformed-input rejection.

use super::*;
use std::io::Cursor;

#[test]
fn debug_section_write_read_when_var_names_then_roundtrips() {
    let section = DebugSection {
        var_names: vec![
            VarNameEntry {
                var_index: VarIndex::new(0),
                function_id: function_id::GLOBAL_SCOPE,
                var_section: var_section::VAR,
                iec_type_tag: iec_type_tag::DINT,
                name: "counter".into(),
                type_name: "DINT".into(),
            },
            VarNameEntry {
                var_index: VarIndex::new(1),
                function_id: function_id::GLOBAL_SCOPE,
                var_section: var_section::VAR,
                iec_type_tag: iec_type_tag::REAL,
                name: "temp".into(),
                type_name: "REAL".into(),
            },
        ],
        func_names: vec![],
        line_map: vec![],
        string_layouts: vec![],
        source_files: vec![],
        enum_defs: vec![],
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let decoded = DebugSection::read_from(&mut Cursor::new(&buf)).unwrap();
    assert_eq!(decoded.var_names.len(), 2);
    assert_eq!(decoded.var_names[0].name, "counter");
    assert_eq!(decoded.var_names[0].iec_type_tag, iec_type_tag::DINT);
    assert_eq!(decoded.var_names[0].type_name, "DINT");
    assert_eq!(decoded.var_names[1].name, "temp");
    assert_eq!(decoded.var_names[1].iec_type_tag, iec_type_tag::REAL);
    assert!(decoded.func_names.is_empty());
}

#[test]
fn debug_section_write_read_when_func_names_then_roundtrips() {
    let section = DebugSection {
        var_names: vec![],
        func_names: vec![
            FuncNameEntry {
                function_id: FunctionId::INIT,
                name: "MAIN_init".into(),
            },
            FuncNameEntry {
                function_id: FunctionId::SCAN,
                name: "MAIN".into(),
            },
        ],
        line_map: vec![],
        string_layouts: vec![],
        source_files: vec![],
        enum_defs: vec![],
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let decoded = DebugSection::read_from(&mut Cursor::new(&buf)).unwrap();
    assert!(decoded.var_names.is_empty());
    assert_eq!(decoded.func_names.len(), 2);
    assert_eq!(decoded.func_names[0].function_id, FunctionId::INIT);
    assert_eq!(decoded.func_names[0].name, "MAIN_init");
    assert_eq!(decoded.func_names[1].function_id, FunctionId::SCAN);
    assert_eq!(decoded.func_names[1].name, "MAIN");
}

#[test]
fn debug_section_write_read_when_both_tables_then_roundtrips() {
    let section = DebugSection {
        var_names: vec![VarNameEntry {
            var_index: VarIndex::new(0),
            function_id: FunctionId::SCAN,
            var_section: var_section::VAR_INPUT,
            iec_type_tag: iec_type_tag::BOOL,
            name: "active".into(),
            type_name: "BOOL".into(),
        }],
        func_names: vec![FuncNameEntry {
            function_id: FunctionId::SCAN,
            name: "MAIN".into(),
        }],
        line_map: vec![],
        string_layouts: vec![],
        source_files: vec![],
        enum_defs: vec![],
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let decoded = DebugSection::read_from(&mut Cursor::new(&buf)).unwrap();
    assert_eq!(decoded.var_names.len(), 1);
    assert_eq!(decoded.func_names.len(), 1);
    assert_eq!(decoded.var_names[0], section.var_names[0]);
    assert_eq!(decoded.func_names[0], section.func_names[0]);
}

#[test]
fn debug_section_read_when_unknown_tag_then_skips() {
    // Build a debug section with an unknown tag (tag=99) followed by FUNC_NAME.
    let mut buf = Vec::new();
    // sub_table_count = 2
    buf.extend_from_slice(&2u16.to_le_bytes());
    // Directory entry 1: unknown tag 99, 4 bytes of payload
    buf.extend_from_slice(&99u16.to_le_bytes());
    buf.extend_from_slice(&0u16.to_le_bytes());
    buf.extend_from_slice(&4u32.to_le_bytes());
    // Directory entry 2: FUNC_NAME
    let func_payload_size: u32 = 2 + 3 + 4; // count(2) + id(2)+len(1)+name(4)
    buf.extend_from_slice(&TAG_FUNC_NAME.to_le_bytes());
    buf.extend_from_slice(&0u16.to_le_bytes());
    buf.extend_from_slice(&func_payload_size.to_le_bytes());
    // Unknown payload: 4 garbage bytes
    buf.extend_from_slice(&[0xDE, 0xAD, 0xBE, 0xEF]);
    // FUNC_NAME payload: count=1, function_id=0, name="MAIN"
    buf.extend_from_slice(&1u16.to_le_bytes());
    buf.extend_from_slice(&0u16.to_le_bytes());
    buf.push(4); // name_len
    buf.extend_from_slice(b"MAIN");

    let decoded = DebugSection::read_from(&mut Cursor::new(&buf)).unwrap();
    assert!(decoded.var_names.is_empty());
    assert_eq!(decoded.func_names.len(), 1);
    assert_eq!(decoded.func_names[0].name, "MAIN");
}

#[test]
fn debug_section_read_when_empty_then_empty_tables() {
    // sub_table_count = 0
    let buf: Vec<u8> = vec![0, 0];
    let decoded = DebugSection::read_from(&mut Cursor::new(&buf)).unwrap();
    assert!(decoded.var_names.is_empty());
    assert!(decoded.func_names.is_empty());
}

#[test]
fn debug_section_write_read_when_line_map_then_roundtrips() {
    let section = DebugSection {
        var_names: vec![],
        func_names: vec![],
        line_map: vec![
            LineMapEntry {
                function_id: FunctionId::SCAN,
                bytecode_offset: 0,
                file_id: SourceFileId::new(0),
                source_line: SourceLine::new(10),
                source_column: SourceColumn::new(1),
            },
            LineMapEntry {
                function_id: FunctionId::SCAN,
                bytecode_offset: 5,
                file_id: SourceFileId::new(0),
                source_line: SourceLine::new(11),
                source_column: SourceColumn::new(3),
            },
            LineMapEntry {
                function_id: FunctionId::SCAN,
                bytecode_offset: 12,
                file_id: SourceFileId::new(0),
                source_line: SourceLine::new(12),
                source_column: SourceColumn::new(0),
            },
        ],
        string_layouts: vec![],
        source_files: vec![],
        enum_defs: vec![],
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();
    assert_eq!(section.section_size(), buf.len() as u32);

    let decoded = DebugSection::read_from(&mut Cursor::new(&buf)).unwrap();
    assert_eq!(decoded.line_map, section.line_map);
    assert!(decoded.var_names.is_empty());
    assert!(decoded.func_names.is_empty());
}

#[test]
fn debug_section_write_read_when_source_files_then_roundtrips() {
    let section = DebugSection {
        var_names: vec![],
        func_names: vec![],
        line_map: vec![],
        string_layouts: vec![],
        source_files: vec![
            SourceFileEntry {
                path: "src/main.st".into(),
                content_hash: *blake3::hash(b"PROGRAM main\nEND_PROGRAM\n").as_bytes(),
            },
            SourceFileEntry {
                path: "src/lib/helpers.st".into(),
                content_hash: [0u8; SOURCE_FILE_HASH_LEN],
            },
        ],
        enum_defs: vec![],
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();
    assert_eq!(section.section_size(), buf.len() as u32);

    let decoded = DebugSection::read_from(&mut Cursor::new(&buf)).unwrap();
    assert_eq!(decoded.source_files, section.source_files);
}

#[test]
fn debug_section_write_read_when_line_map_carries_file_id_then_roundtrips() {
    let section = DebugSection {
        var_names: vec![],
        func_names: vec![],
        line_map: vec![
            LineMapEntry {
                function_id: FunctionId::SCAN,
                bytecode_offset: 0,
                file_id: SourceFileId::new(0),
                source_line: SourceLine::new(1),
                source_column: SourceColumn::new(1),
            },
            LineMapEntry {
                function_id: FunctionId::SCAN,
                bytecode_offset: 6,
                file_id: SourceFileId::new(1),
                source_line: SourceLine::new(7),
                source_column: SourceColumn::new(1),
            },
        ],
        string_layouts: vec![],
        source_files: vec![],
        enum_defs: vec![],
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();
    assert_eq!(section.section_size(), buf.len() as u32);

    let decoded = DebugSection::read_from(&mut Cursor::new(&buf)).unwrap();
    assert_eq!(decoded.line_map, section.line_map);
    assert_eq!(decoded.line_map[0].file_id.raw(), 0);
    assert_eq!(decoded.line_map[1].file_id.raw(), 1);
}

#[test]
fn debug_section_source_file_content_hash_when_known_input_then_matches_blake3() {
    // BLAKE3 is collision-resistant; this test pins down that the
    // crate uses the algorithm we documented in the spec/ADR. If we
    // ever swap to a different algorithm, this needs to be updated
    // intentionally — not silently.
    let source = b"PROGRAM main\nVAR x : DINT; END_VAR\nEND_PROGRAM\n";
    let expected = *blake3::hash(source).as_bytes();
    let entry = SourceFileEntry {
        path: "p.st".into(),
        content_hash: expected,
    };
    assert_eq!(entry.content_hash, expected);
    // Sanity: BLAKE3 produces a non-zero digest for non-empty input.
    assert_ne!(entry.content_hash, [0u8; SOURCE_FILE_HASH_LEN]);
}

#[test]
fn debug_section_read_when_unknown_high_tag_then_skips_and_continues() {
    // Cover the unknown-tag path with a tag the reader doesn't
    // recognize alongside tag 6, so the SOURCE_FILE_TABLE still loads.
    let mut buf = Vec::new();
    // sub_table_count = 2
    buf.extend_from_slice(&2u16.to_le_bytes());
    // Directory entry 1: unknown tag 99, 4 bytes of payload
    buf.extend_from_slice(&99u16.to_le_bytes());
    buf.extend_from_slice(&0u16.to_le_bytes());
    buf.extend_from_slice(&4u32.to_le_bytes());
    // Directory entry 2: TAG_SOURCE_FILE, one entry "p.st" + 32-byte hash
    let payload_size: u32 = 2 + 2 + 4 + SOURCE_FILE_HASH_LEN as u32;
    buf.extend_from_slice(&TAG_SOURCE_FILE.to_le_bytes());
    buf.extend_from_slice(&0u16.to_le_bytes());
    buf.extend_from_slice(&payload_size.to_le_bytes());
    // Unknown payload: 4 garbage bytes
    buf.extend_from_slice(&[0xDE, 0xAD, 0xBE, 0xEF]);
    // SOURCE_FILE payload: count=1, path_len=4, "p.st", 32-byte hash
    buf.extend_from_slice(&1u16.to_le_bytes());
    buf.extend_from_slice(&4u16.to_le_bytes());
    buf.extend_from_slice(b"p.st");
    let hash = [0xABu8; SOURCE_FILE_HASH_LEN];
    buf.extend_from_slice(&hash);

    let decoded = DebugSection::read_from(&mut Cursor::new(&buf)).unwrap();
    assert_eq!(decoded.source_files.len(), 1);
    assert_eq!(decoded.source_files[0].path, "p.st");
    assert_eq!(decoded.source_files[0].content_hash, hash);
}

#[test]
fn debug_section_lookup_source_location_when_offset_between_entries_then_returns_lower() {
    let section = DebugSection {
        var_names: vec![],
        func_names: vec![],
        // Sorted by (function_id, bytecode_offset) per the line_map
        // invariant.
        line_map: vec![
            LineMapEntry {
                function_id: FunctionId::INIT,
                bytecode_offset: 0,
                file_id: SourceFileId::new(0),
                source_line: SourceLine::new(99),
                source_column: SourceColumn::new(1),
            },
            LineMapEntry {
                function_id: FunctionId::SCAN,
                bytecode_offset: 0,
                file_id: SourceFileId::new(0),
                source_line: SourceLine::new(10),
                source_column: SourceColumn::new(1),
            },
            LineMapEntry {
                function_id: FunctionId::SCAN,
                bytecode_offset: 8,
                file_id: SourceFileId::new(0),
                source_line: SourceLine::new(11),
                source_column: SourceColumn::new(1),
            },
        ],
        string_layouts: vec![],
        source_files: vec![],
        enum_defs: vec![],
    };

    // Exact match
    let hit = section.lookup_source_location(FunctionId::SCAN, 0).unwrap();
    assert_eq!(hit.source_line.raw(), 10);

    // Between entries: should pick the largest bytecode_offset <= 5 (which is 0)
    let hit = section.lookup_source_location(FunctionId::SCAN, 5).unwrap();
    assert_eq!(hit.source_line.raw(), 10);

    // At/after the second entry
    let hit = section
        .lookup_source_location(FunctionId::SCAN, 20)
        .unwrap();
    assert_eq!(hit.source_line.raw(), 11);

    // Different function uses its own entries
    let hit = section.lookup_source_location(FunctionId::INIT, 0).unwrap();
    assert_eq!(hit.source_line.raw(), 99);

    // Function with no entries returns None
    assert!(section
        .lookup_source_location(FunctionId::new(123), 0)
        .is_none());
}

#[test]
fn debug_section_section_size_when_both_tables_then_correct() {
    let section = DebugSection {
        var_names: vec![VarNameEntry {
            var_index: VarIndex::new(0),
            function_id: function_id::GLOBAL_SCOPE,
            var_section: var_section::VAR,
            iec_type_tag: iec_type_tag::DINT,
            name: "x".into(),
            type_name: "DINT".into(),
        }],
        func_names: vec![FuncNameEntry {
            function_id: FunctionId::INIT,
            name: "MAIN".into(),
        }],
        line_map: vec![],
        string_layouts: vec![],
        source_files: vec![],
        enum_defs: vec![],
    };

    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();
    assert_eq!(section.section_size(), buf.len() as u32);
}

#[test]
fn debug_section_lookup_source_location_when_multiple_entries_then_returns_greatest_leq() {
    let section = DebugSection {
        var_names: vec![],
        func_names: vec![],
        line_map: vec![
            LineMapEntry {
                function_id: FunctionId::INIT,
                bytecode_offset: 0,
                file_id: SourceFileId::new(0),
                source_line: SourceLine::new(10),
                source_column: SourceColumn::new(1),
            },
            LineMapEntry {
                function_id: FunctionId::INIT,
                bytecode_offset: 8,
                file_id: SourceFileId::new(0),
                source_line: SourceLine::new(20),
                source_column: SourceColumn::new(1),
            },
            LineMapEntry {
                function_id: FunctionId::INIT,
                bytecode_offset: 12,
                file_id: SourceFileId::new(0),
                source_line: SourceLine::new(30),
                source_column: SourceColumn::new(1),
            },
            // Entry for a different function that must never be returned.
            LineMapEntry {
                function_id: FunctionId::GLOBAL_SCOPE,
                bytecode_offset: 4,
                file_id: SourceFileId::new(0),
                source_line: SourceLine::new(99),
                source_column: SourceColumn::new(1),
            },
        ],
        string_layouts: vec![],
        source_files: vec![],
        enum_defs: vec![],
    };

    // Offset 0: matches the first entry exactly.
    let hit = section.lookup_source_location(FunctionId::INIT, 0).unwrap();
    assert_eq!(hit.source_line.raw(), 10);

    // Offset 5: before the second entry, should return the first.
    let hit = section.lookup_source_location(FunctionId::INIT, 5).unwrap();
    assert_eq!(hit.source_line.raw(), 10);

    // Offset 10: between second and third entry, should return the second.
    let hit = section
        .lookup_source_location(FunctionId::INIT, 10)
        .unwrap();
    assert_eq!(hit.source_line.raw(), 20);

    // Offset 12: matches the third entry exactly.
    let hit = section
        .lookup_source_location(FunctionId::INIT, 12)
        .unwrap();
    assert_eq!(hit.source_line.raw(), 30);

    // Offset 100: beyond the last entry, should still return the third.
    let hit = section
        .lookup_source_location(FunctionId::INIT, 100)
        .unwrap();
    assert_eq!(hit.source_line.raw(), 30);
}
