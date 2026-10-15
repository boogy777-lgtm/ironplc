//! Tests for recording the bytecode-offset to source-position line map.

use super::*;

#[test]
fn emitter_line_map_when_no_position_set_then_no_entries_recorded() {
    let mut em = Emitter::new();
    em.emit_load_const_i32(0);
    em.emit_ret_void();
    assert!(em.take_line_map().is_empty());
}

#[test]
fn emitter_line_map_when_position_set_then_records_entry_at_next_opcode_offset() {
    let mut em = Emitter::new();
    em.set_source_position(
        SourceFileId::new(0),
        SourceLine::new(10),
        SourceColumn::new(5),
    );
    em.emit_ret_void();
    let entries = em.take_line_map();
    assert_eq!(
        entries,
        vec![EmittedLineMapEntry {
            bytecode_offset: 0,
            file_id: SourceFileId::new(0),
            source_line: SourceLine::new(10),
            source_column: SourceColumn::new(5),
        }],
    );
}

#[test]
fn emitter_line_map_when_consecutive_opcodes_share_position_then_dedupes_to_one_entry() {
    let mut em = Emitter::new();
    em.set_source_position(
        SourceFileId::new(0),
        SourceLine::new(7),
        SourceColumn::new(1),
    );
    em.emit_load_const_i32(0);
    em.emit_load_const_i32(1);
    em.emit_add_i32();
    em.emit_ret_void();
    let entries = em.take_line_map();
    assert_eq!(entries.len(), 1);
    assert_eq!(entries[0].source_line.raw(), 7);
    assert_eq!(entries[0].bytecode_offset, 0);
}

#[test]
fn emitter_line_map_when_position_changes_then_records_new_entry_at_boundary() {
    let mut em = Emitter::new();
    em.set_source_position(
        SourceFileId::new(0),
        SourceLine::new(1),
        SourceColumn::new(1),
    );
    em.emit_load_const_i32(0); // 3 bytes at offset 0
    em.set_source_position(
        SourceFileId::new(0),
        SourceLine::new(2),
        SourceColumn::new(1),
    );
    em.emit_load_const_i32(1); // 3 bytes at offset 3
    let entries = em.take_line_map();
    assert_eq!(entries.len(), 2);
    assert_eq!(entries[0].bytecode_offset, 0);
    assert_eq!(entries[0].source_line.raw(), 1);
    assert_eq!(entries[1].bytecode_offset, 3);
    assert_eq!(entries[1].source_line.raw(), 2);
}

#[test]
fn emitter_line_map_when_file_changes_then_records_new_entry_even_at_same_line_column() {
    let mut em = Emitter::new();
    em.set_source_position(
        SourceFileId::new(0),
        SourceLine::new(1),
        SourceColumn::new(1),
    );
    em.emit_load_const_i32(0);
    // Same line/column, different file — must record a new entry.
    em.set_source_position(
        SourceFileId::new(1),
        SourceLine::new(1),
        SourceColumn::new(1),
    );
    em.emit_load_const_i32(1);
    let entries = em.take_line_map();
    assert_eq!(entries.len(), 2);
    assert_eq!(entries[0].file_id.raw(), 0);
    assert_eq!(entries[1].file_id.raw(), 1);
    assert_eq!(entries[1].bytecode_offset, 3);
}

#[test]
fn emitter_line_map_when_clear_position_then_subsequent_opcodes_omit_entries() {
    let mut em = Emitter::new();
    em.set_source_position(
        SourceFileId::new(0),
        SourceLine::new(1),
        SourceColumn::new(1),
    );
    em.emit_load_const_i32(0);
    em.clear_source_position();
    em.emit_load_const_i32(1);
    em.emit_ret_void();
    let entries = em.take_line_map();
    assert_eq!(entries.len(), 1);
    assert_eq!(entries[0].source_line.raw(), 1);
}

#[test]
fn emitter_line_map_when_store_then_load_dup_optimized_then_no_entry_for_elided_load() {
    // The store-load peephole inserts DUP before the STORE and elides
    // the LOAD entirely. A line_map entry would have nowhere to point,
    // so we expect the LOAD's source position not to produce a new
    // entry. The STORE, recorded under its own position, must remain.
    let mut em = Emitter::new();
    em.set_source_position(
        SourceFileId::new(0),
        SourceLine::new(1),
        SourceColumn::new(1),
    );
    em.emit_load_const_i32(0);
    em.emit_store_var_i32(VarIndex::new(0));
    // Same source position for the LOAD that should be elided.
    em.set_source_position(
        SourceFileId::new(0),
        SourceLine::new(2),
        SourceColumn::new(1),
    );
    em.emit_load_var_i32(VarIndex::new(0));
    let entries = em.take_line_map();
    // Only the LOAD_CONST entry at line 1 is present; the LOAD_VAR at
    // line 2 was elided by the peephole so no entry exists for it.
    assert_eq!(entries.len(), 1);
    assert_eq!(entries[0].source_line.raw(), 1);
}

#[test]
fn emitter_line_map_when_take_called_then_subsequent_emissions_start_fresh() {
    let mut em = Emitter::new();
    em.set_source_position(
        SourceFileId::new(0),
        SourceLine::new(1),
        SourceColumn::new(1),
    );
    em.emit_ret_void();
    let first = em.take_line_map();
    assert_eq!(first.len(), 1);

    em.set_source_position(
        SourceFileId::new(0),
        SourceLine::new(2),
        SourceColumn::new(2),
    );
    em.emit_ret_void();
    let second = em.take_line_map();
    assert_eq!(second.len(), 1);
    assert_eq!(second[0].source_line.raw(), 2);
}
