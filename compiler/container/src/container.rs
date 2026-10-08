use std::io::{Cursor, Read, Write};
use std::vec::Vec;

use crate::code_section::CodeSection;
use crate::constant_pool::ConstantPool;
use crate::debug_section::DebugSection;
use crate::header::{FileHeader, FLAG_HAS_DEBUG_SECTION, FLAG_HAS_TYPE_SECTION, HEADER_SIZE};
use crate::integrity;
use crate::load_verify::verify_load;
use crate::task_table::TaskTable;
use crate::type_section::TypeSection;
use crate::ContainerError;

/// A complete bytecode container, in file order: header, task table,
/// optional type section, constant pool, code section, optional debug
/// section.
#[derive(Clone, Debug)]
pub struct Container {
    pub header: FileHeader,
    pub task_table: TaskTable,
    pub type_section: Option<TypeSection>,
    pub constant_pool: ConstantPool,
    pub code: CodeSection,
    pub debug_section: Option<DebugSection>,
}

impl Container {
    /// Writes the container to the given writer.
    ///
    /// Each section is serialized to a buffer first, so the header is
    /// derived from the bytes that actually reach the file — the section
    /// directory from their lengths, `content_hash` and `debug_hash` from
    /// their contents, and `layout_hash` (see [`compute_layout_hash`](Self::compute_layout_hash))
    /// from the persistent part of the type section — before anything is written. Whatever
    /// `self.header` carries in those fields is replaced.
    pub fn write_to(&self, w: &mut impl Write) -> Result<(), ContainerError> {
        let task_bytes = serialize(|buf| self.task_table.write_to(buf))?;
        let type_bytes = match &self.type_section {
            Some(type_section) => serialize(|buf| type_section.write_to(buf))?,
            None => Vec::new(),
        };
        let const_bytes = serialize(|buf| self.constant_pool.write_to(buf))?;
        let code_bytes = serialize(|buf| self.code.write_to(buf))?;
        let debug_bytes = match &self.debug_section {
            Some(debug) => serialize(|buf| debug.write_to(buf))?,
            None => Vec::new(),
        };

        let mut header = self.header.clone();
        let mut next_offset = HEADER_SIZE as u32;

        header.task_section_offset = next_offset;
        header.task_section_size = task_bytes.len() as u32;
        next_offset += header.task_section_size;

        // Type section (optional, between task table and constant pool)
        if self.type_section.is_some() {
            header.type_section_offset = next_offset;
            header.type_section_size = type_bytes.len() as u32;
            header.flags |= FLAG_HAS_TYPE_SECTION;
            next_offset += header.type_section_size;
        }

        header.const_section_offset = next_offset;
        header.const_section_size = const_bytes.len() as u32;
        next_offset += header.const_section_size;

        header.code_section_offset = next_offset;
        header.code_section_size = code_bytes.len() as u32;
        header.num_functions = self.code.functions.len() as u16;
        next_offset += header.code_section_size;

        if self.debug_section.is_some() {
            header.debug_section_offset = next_offset;
            header.debug_section_size = debug_bytes.len() as u32;
            header.flags |= FLAG_HAS_DEBUG_SECTION;
        }

        header.debug_hash = if self.debug_section.is_some() {
            integrity::debug_hash(&debug_bytes)
        } else {
            integrity::NO_HASH
        };

        // The layout hash is computed from the type section before the
        // header is serialized for hashing: the content hash covers the
        // masked header, and the mask does not blank `layout_hash`.
        //
        // It is kept alongside the ADR-0007 integrity hashes on purpose: the
        // integrity scope covers *execution* (any byte that changes behavior),
        // while `layout_hash` is the *online-change gate* (ADR-0052/0058) and
        // deliberately narrower — code, constants and debug info are excluded
        // so a logic-only edit keeps the hash equal and can swap at a scan
        // boundary without a restart. Neither can do the other's job.
        header.layout_hash = self.compute_layout_hash();

        // The header is part of the hashed content (masked), so it is
        // serialized once to hash and again to write with the hash in it.
        header.content_hash = integrity::content_hash(&integrity::Content {
            header: &header_image(&header)?,
            task_table: &task_bytes,
            type_section: &type_bytes,
            const_section: &const_bytes,
            code_section: &code_bytes,
        });

        header.write_to(w)?;
        w.write_all(&task_bytes)?;
        w.write_all(&type_bytes)?;
        w.write_all(&const_bytes)?;
        w.write_all(&code_bytes)?;
        w.write_all(&debug_bytes)?;

        Ok(())
    }

    /// Reads a container from the given reader.
    ///
    /// Rejects the container with [`ContainerError::ContentHashMismatch`]
    /// when the header carries a content hash that the header, task table,
    /// type, constant and code sections do not reproduce; a zero hash is a
    /// legacy container and is accepted. A debug section whose bytes do not
    /// reproduce a carried `debug_hash` is discarded, not fatal, so a
    /// modified or stale debug section cannot stop a program from running —
    /// that is the separation ADR-0007 asks for. The structural load-time
    /// checks ([`verify_load`](crate::load_verify)) then validate the type
    /// section, including a recomputed `layout_hash`, per ADR-0006.
    pub fn read_from(r: &mut impl Read) -> Result<Self, ContainerError> {
        let mut header_bytes = [0u8; HEADER_SIZE];
        r.read_exact(&mut header_bytes)?;
        let header = FileHeader::from_bytes(&header_bytes)?;

        // Read remaining bytes after the header so we can seek to
        // section offsets within them.
        let mut rest = Vec::new();
        r.read_to_end(&mut rest)?;

        let base = HEADER_SIZE as u32;

        // Integrity first, so a modified file reports the modification
        // rather than whatever parse error the modification happens to
        // cause. An unhashed container skips this and is parsed as
        // leniently as before.
        if header.content_hash != integrity::NO_HASH {
            let h = &header;
            integrity::check_content_hash(
                &h.content_hash,
                &integrity::Content {
                    header: &header_bytes,
                    task_table: section_bytes(&rest, h.task_section_offset, h.task_section_size)?,
                    type_section: section_bytes(&rest, h.type_section_offset, h.type_section_size)?,
                    const_section: section_bytes(
                        &rest,
                        h.const_section_offset,
                        h.const_section_size,
                    )?,
                    code_section: section_bytes(&rest, h.code_section_offset, h.code_section_size)?,
                },
            )?;
        }

        let task_start = (header.task_section_offset - base) as usize;
        let task_end = task_start + header.task_section_size as usize;
        let task_table = TaskTable::read_from(&mut Cursor::new(&rest[task_start..task_end]))?;

        // Parse type section if present.
        let type_section =
            if (header.flags & FLAG_HAS_TYPE_SECTION) != 0 && header.type_section_size > 0 {
                let ts_start = (header.type_section_offset - base) as usize;
                let ts_end = ts_start + header.type_section_size as usize;
                if ts_end <= rest.len() {
                    Some(TypeSection::read_from(&mut Cursor::new(
                        &rest[ts_start..ts_end],
                    ))?)
                } else {
                    None
                }
            } else {
                None
            };

        let const_start = (header.const_section_offset - base) as usize;
        let const_end = const_start + header.const_section_size as usize;
        let constant_pool =
            ConstantPool::read_from(&mut Cursor::new(&rest[const_start..const_end]))?;

        let code_start = (header.code_section_offset - base) as usize;
        let code_end = code_start + header.code_section_size as usize;
        let code = CodeSection::read_from(
            &mut Cursor::new(&rest[code_start..code_end]),
            header.num_functions,
            header.code_section_size,
        )?;

        // Parse debug section if present (non-fatal on error or on a
        // debug hash mismatch).
        let debug_section = if header.debug_section_size > 0 {
            let debug_start = (header.debug_section_offset - base) as usize;
            let debug_end = debug_start + header.debug_section_size as usize;
            if debug_end <= rest.len()
                && integrity::debug_hash_matches(&header.debug_hash, &rest[debug_start..debug_end])
            {
                DebugSection::read_from(&mut Cursor::new(&rest[debug_start..debug_end])).ok()
            } else {
                None
            }
        } else {
            None
        };

        let container = Container {
            header,
            task_table,
            type_section,
            constant_pool,
            code,
            debug_section,
        };

        verify_load(&container).map_err(ContainerError::VerificationFailed)?;

        Ok(container)
    }
}

/// The bytes of one section, located by its directory entry, within the
/// bytes that follow the header. An absent section (size 0) is empty; an
/// entry that points outside `rest` is a [`ContainerError::SectionSizeMismatch`].
fn section_bytes(rest: &[u8], offset: u32, size: u32) -> Result<&[u8], ContainerError> {
    if size == 0 {
        return Ok(&[]);
    }
    let start = (offset as usize)
        .checked_sub(HEADER_SIZE)
        .ok_or(ContainerError::SectionSizeMismatch)?;
    rest.get(start..start + size as usize)
        .ok_or(ContainerError::SectionSizeMismatch)
}

/// Serializes a header to its 256 on-disk bytes.
fn header_image(header: &FileHeader) -> Result<[u8; HEADER_SIZE], ContainerError> {
    let bytes = serialize(|buf| header.write_to(buf))?;
    bytes
        .try_into()
        .map_err(|_| ContainerError::SectionSizeMismatch)
}

/// Runs a section writer against a fresh buffer and returns the bytes.
fn serialize(
    write: impl FnOnce(&mut Vec<u8>) -> Result<(), ContainerError>,
) -> Result<Vec<u8>, ContainerError> {
    let mut buf = Vec::new();
    write(&mut buf)?;
    Ok(buf)
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::vec;
    use std::vec::Vec;

    use crate::debug_section::{
        function_id, iec_type_tag, var_section, FuncNameEntry, VarNameEntry,
    };
    use crate::id_types::{ConstantIndex, FbTypeId, FunctionId, InstanceId, TaskId, VarIndex};
    use crate::test_support::{
        container_bytes, round_trip, steel_thread_bytecode, steel_thread_single_function_container,
        with_tampered_header,
    };
    use crate::type_section::{FbTypeDescriptor, FieldEntry, FieldType, StableVarEntry, VarEntry};
    use crate::ContainerBuilder;

    #[test]
    fn container_write_read_when_steel_thread_program_then_roundtrips() {
        // x := 10; y := x + 32;
        let bytecode = steel_thread_bytecode();
        let decoded = round_trip(&steel_thread_single_function_container());

        // Verify synthesized default task table
        assert_eq!(decoded.task_table.tasks.len(), 1);
        assert_eq!(decoded.task_table.tasks[0].task_id, TaskId::DEFAULT);
        assert_eq!(
            decoded.task_table.tasks[0].task_type,
            crate::TaskType::Freewheeling
        );
        assert_eq!(decoded.task_table.tasks[0].flags, 0x01);
        assert_eq!(decoded.task_table.programs.len(), 1);
        assert_eq!(
            decoded.task_table.programs[0].instance_id,
            InstanceId::DEFAULT
        );
        assert_eq!(decoded.task_table.programs[0].task_id, TaskId::DEFAULT);
        assert_eq!(decoded.task_table.programs[0].var_table_count, 2);

        assert_eq!(
            decoded
                .constant_pool
                .get_i32(ConstantIndex::new(0))
                .unwrap(),
            10
        );
        assert_eq!(
            decoded
                .constant_pool
                .get_i32(ConstantIndex::new(1))
                .unwrap(),
            32
        );
        assert_eq!(decoded.code.functions.len(), 1);
        assert_eq!(decoded.code.functions[0].function_id, FunctionId::INIT);

        let code = decoded
            .code
            .get_function_bytecode(FunctionId::INIT)
            .unwrap();
        assert_eq!(code, bytecode.as_slice());

        // No debug section in this container.
        assert!(decoded.debug_section.is_none());
    }

    #[test]
    fn container_write_read_when_debug_section_then_roundtrips() {
        #[rustfmt::skip]
        let bytecode: Vec<u8> = vec![
            0x00, 0x00, 0x00,       // LOAD_CONST_I32 pool[0]
            0x10, 0x00, 0x00,       // STORE_VAR_I32  var[0]
            0x8C,                   // RET_VOID
        ];

        let container = ContainerBuilder::new()
            .num_variables(1)
            .add_i32_constant(42)
            .add_function(FunctionId::INIT, &bytecode, 1, 1, 0)
            .add_var_name(VarNameEntry {
                var_index: VarIndex::new(0),
                function_id: function_id::GLOBAL_SCOPE,
                var_section: var_section::VAR,
                iec_type_tag: iec_type_tag::DINT,
                name: "x".into(),
                type_name: "DINT".into(),
            })
            .add_func_name(FuncNameEntry {
                function_id: FunctionId::INIT,
                name: "MAIN".into(),
            })
            .build();

        let mut buf = Vec::new();
        container.write_to(&mut buf).unwrap();

        let decoded = Container::read_from(&mut Cursor::new(&buf)).unwrap();

        // Verify debug section flag is set.
        assert_eq!(decoded.header.flags & 0x02, 0x02);

        let debug = decoded.debug_section.unwrap();
        assert_eq!(debug.var_names.len(), 1);
        assert_eq!(debug.var_names[0].name, "x");
        assert_eq!(debug.var_names[0].type_name, "DINT");
        assert_eq!(debug.var_names[0].iec_type_tag, iec_type_tag::DINT);
        assert_eq!(debug.func_names.len(), 1);
        assert_eq!(debug.func_names[0].name, "MAIN");
    }

    #[test]
    fn container_write_read_when_type_section_with_array_then_roundtrips() {
        #[rustfmt::skip]
        let bytecode: Vec<u8> = vec![
            0x00, 0x00, 0x00,       // LOAD_CONST_I32 pool[0]
            0x10, 0x00, 0x00,       // STORE_VAR_I32  var[0]
            0x8C,                   // RET_VOID
        ];

        let mut builder = ContainerBuilder::new();
        let desc_idx = builder.add_array_descriptor(0, 10, 0); // I32, 10 elements
        assert_eq!(desc_idx, 0);

        let container = builder
            .num_variables(1)
            .add_var_entry(VarEntry {
                var_type: FieldType::I32,
                flags: 0,
                extra: 0,
            })
            .add_i32_constant(42)
            .add_function(FunctionId::INIT, &bytecode, 1, 1, 0)
            .build();

        let mut buf = Vec::new();
        container.write_to(&mut buf).unwrap();

        let decoded = Container::read_from(&mut Cursor::new(&buf)).unwrap();

        // Verify type section flag is set.
        assert_eq!(decoded.header.flags & 0x04, 0x04);

        let ts = decoded.type_section.unwrap();
        assert!(ts.fb_types.is_empty());
        assert_eq!(ts.array_descriptors.len(), 1);
        assert_eq!(ts.array_descriptors[0].element_type, 0);
        assert_eq!(ts.array_descriptors[0].total_elements, 10);

        // Verify other sections still roundtrip correctly.
        assert_eq!(
            decoded
                .constant_pool
                .get_i32(ConstantIndex::new(0))
                .unwrap(),
            42
        );
        assert_eq!(decoded.code.functions.len(), 1);
    }

    /// An inflated type-section size makes the section fall outside the
    /// file, so the lenient parse treats it as absent. Clearing both hashes
    /// selects the legacy path: no integrity check and, per ADR-0052's hash
    /// contract, no layout-hash recompute.
    #[test]
    fn container_read_from_when_type_section_truncated_then_type_section_is_none() {
        #[rustfmt::skip]
        let bytecode: Vec<u8> = vec![
            0x01, 0x00, 0x00,
            0x18, 0x00, 0x00,
            0x8C,
        ];

        let mut builder = ContainerBuilder::new();
        builder.add_array_descriptor(0, 4, 0);
        let container = builder
            .num_variables(1)
            .add_i32_constant(1)
            .add_function(FunctionId::INIT, &bytecode, 1, 1, 0)
            .build();

        let mut buf = Vec::new();
        container.write_to(&mut buf).unwrap();

        // Inflate the declared type_section_size so ts_end exceeds available
        // bytes, forcing the bounds check in read_from to return None. The
        // lenient path is for unhashed containers, so drop the hashes too
        // (a zero layout_hash means "never serialized" per ADR-0052's hash
        // contract and skips the recompute).
        let n = buf.len() as u32;
        let tampered = with_tampered_header(&buf, |h| {
            h.type_section_size = n * 2;
            h.content_hash = integrity::NO_HASH;
            h.layout_hash = [0u8; 32];
        });

        let decoded = Container::read_from(&mut Cursor::new(&tampered)).unwrap();
        assert!(decoded.type_section.is_none());
    }

    /// An inflated debug-section size leaves no debug bytes to verify, so
    /// the debug section loads as absent — discarded, non-fatally, exactly
    /// like a malformed debug section.
    #[test]
    fn container_read_from_when_debug_section_truncated_then_debug_section_is_none() {
        #[rustfmt::skip]
        let bytecode: Vec<u8> = vec![
            0x01, 0x00, 0x00,
            0x18, 0x00, 0x00,
            0x8C,
        ];

        let container = ContainerBuilder::new()
            .num_variables(1)
            .add_i32_constant(1)
            .add_function(FunctionId::INIT, &bytecode, 1, 1, 0)
            .add_func_name(FuncNameEntry {
                function_id: FunctionId::INIT,
                name: "MAIN".into(),
            })
            .build();

        let mut buf = Vec::new();
        container.write_to(&mut buf).unwrap();

        // Inflate the declared debug_section_size past the end of the buffer,
        // triggering the bounds check in read_from that returns None.
        let n = buf.len() as u32;
        let tampered = with_tampered_header(&buf, |h| h.debug_section_size = n * 2);

        let decoded = Container::read_from(&mut Cursor::new(&tampered)).unwrap();
        assert!(decoded.debug_section.is_none());
    }

    /// A container exercising every input of the layout hash: a variable
    /// table, an FB type with two fields, an array descriptor and the stable
    /// variable IDs (which are deliberately not part of the hash).
    fn layout_hash_container() -> Container {
        let mut builder = ContainerBuilder::new();
        builder.add_array_descriptor(FieldType::I32 as u8, 4, 0);
        builder
            .num_variables(2)
            .add_var_entry(VarEntry {
                var_type: FieldType::I32,
                flags: 0,
                extra: 0,
            })
            .add_var_entry(VarEntry {
                var_type: FieldType::String,
                flags: 0,
                extra: 80,
            })
            .add_stable_var(StableVarEntry {
                var_index: VarIndex::new(0),
                uid: 0x1000,
            })
            .add_stable_var(StableVarEntry {
                var_index: VarIndex::new(1),
                uid: 0x2000,
            })
            .add_fb_type(FbTypeDescriptor {
                type_id: FbTypeId::new(0),
                fields: vec![
                    FieldEntry {
                        field_type: FieldType::I32,
                        field_extra: 0,
                    },
                    FieldEntry {
                        field_type: FieldType::Time,
                        field_extra: 0,
                    },
                ],
            })
            .add_function(FunctionId::INIT, &[0x8C], 0, 0, 0)
            .build()
    }

    #[test]
    fn container_write_read_when_variable_table_then_roundtrips() {
        let container = layout_hash_container();

        let decoded = round_trip(&container);

        let ts = decoded.type_section.unwrap();
        assert_eq!(ts.variable_table.len(), 2);
        assert_eq!(ts.variable_table[0].var_type, FieldType::I32);
        assert_eq!(ts.variable_table[0].flags, 0);
        assert_eq!(ts.variable_table[0].extra, 0);
        assert_eq!(ts.variable_table[1].var_type, FieldType::String);
        assert_eq!(ts.variable_table[1].extra, 80);
    }

    #[test]
    fn container_write_read_when_stable_vars_then_roundtrips() {
        let container = layout_hash_container();

        let decoded = round_trip(&container);

        let ts = decoded.type_section.unwrap();
        assert_eq!(
            ts.stable_vars,
            vec![
                StableVarEntry {
                    var_index: VarIndex::new(0),
                    uid: 0x1000,
                },
                StableVarEntry {
                    var_index: VarIndex::new(1),
                    uid: 0x2000,
                },
            ]
        );
    }

    #[test]
    fn compute_layout_hash_when_same_inputs_then_equal() {
        let first = layout_hash_container();
        let second = layout_hash_container();

        assert_eq!(first.compute_layout_hash(), second.compute_layout_hash());
    }

    #[test]
    fn compute_layout_hash_when_only_stable_var_uids_change_then_equal() {
        // A rename (a new UID binding for the same variable index, or new
        // UIDs for the same entities) must not look like a layout change:
        // migration compatibility is the migration planner's decision, not
        // this hash's.
        let first = layout_hash_container();
        let mut second = layout_hash_container();
        {
            let stable_vars = &mut second.type_section.as_mut().unwrap().stable_vars;
            stable_vars[0].uid = 0xFFFF_FFFF_FFFF_FFFF;
            stable_vars[1].uid = 0;
        }

        assert_eq!(first.compute_layout_hash(), second.compute_layout_hash());
    }

    #[test]
    fn compute_layout_hash_when_only_fb_field_uids_change_then_equal() {
        // FB field UIDs (ADR 0059) are identity, not layout: assigning or
        // changing them must not look like a layout change.
        let first = layout_hash_container();
        let mut second = layout_hash_container();
        second.type_section.as_mut().unwrap().fb_field_uids.push(
            crate::type_section::FbFieldUidEntry {
                fb_type_id: FbTypeId::new(0x1000),
                field_index: 0,
                uid: 0xBEEF,
            },
        );

        assert_eq!(first.compute_layout_hash(), second.compute_layout_hash());
    }

    #[test]
    fn compute_layout_hash_when_variable_entry_changes_then_differs() {
        let first = layout_hash_container();
        let mut second = layout_hash_container();
        second.type_section.as_mut().unwrap().variable_table[0].extra = 1;

        assert_ne!(first.compute_layout_hash(), second.compute_layout_hash());
    }

    #[test]
    fn compute_layout_hash_when_fb_field_changes_then_differs() {
        let first = layout_hash_container();
        let mut second = layout_hash_container();
        second.type_section.as_mut().unwrap().fb_types[0].fields[0].field_extra = 1;

        assert_ne!(first.compute_layout_hash(), second.compute_layout_hash());
    }

    #[test]
    fn container_read_from_when_version_7_file_then_unsupported_version() {
        // A container of the previous format version is refused when its
        // header is read; it is never read with the old meaning of the layout
        // hash or without the persistent extent.
        let mut buf = Vec::new();
        layout_hash_container().write_to(&mut buf).unwrap();
        let version_7 = with_tampered_header(&buf, |h| h.format_version = 7);

        let result = Container::read_from(&mut Cursor::new(&version_7));

        assert!(matches!(result, Err(ContainerError::UnsupportedVersion)));
    }

    #[test]
    fn write_to_when_called_then_header_hashes_match_computation() {
        let container = layout_hash_container();
        let mut buf = Vec::new();
        container.write_to(&mut buf).unwrap();

        let decoded = Container::read_from(&mut Cursor::new(&buf)).unwrap();

        assert_eq!(decoded.header.layout_hash, decoded.compute_layout_hash());
        assert_ne!(decoded.header.layout_hash, [0u8; 32]);
        // The integrity hashes are populated on the wire; the in-memory
        // header keeps zeros until serialized (ADR-0052's hash contract).
        // This container has no debug section, so `debug_hash` is zero on
        // the wire per the Content Hash Scope.
        assert_ne!(decoded.header.content_hash, [0u8; 32]);
        assert_eq!(decoded.header.debug_hash, [0u8; 32]);
        assert_eq!(container.header.content_hash, [0u8; 32]);
        assert_eq!(container.header.debug_hash, [0u8; 32]);
        assert_eq!(container.header.layout_hash, [0u8; 32]);
    }

    /// Corrupting a code byte invalidates the content hash, and the reader
    /// rejects the container before any section is used.
    #[test]
    fn read_from_when_code_byte_tampered_then_content_hash_rejected() {
        let container = layout_hash_container();
        let mut buf = Vec::new();
        container.write_to(&mut buf).unwrap();

        let header = FileHeader::read_from(&mut Cursor::new(&buf[..HEADER_SIZE])).unwrap();
        let code_end = (header.code_section_offset + header.code_section_size) as usize;
        buf[code_end - 1] = buf[code_end - 1].wrapping_add(1);

        let result = Container::read_from(&mut Cursor::new(&buf));
        assert!(matches!(result, Err(ContainerError::ContentHashMismatch)));
    }

    /// Zeroing the content hash marks the container as legacy, and the
    /// reader accepts it without checking — even when a type-section
    /// invariant is violated, which the structural verifier still catches.
    #[test]
    fn read_from_when_content_hash_zeroed_then_legacy_accept() {
        let container = layout_hash_container();
        let buf = container_bytes(&container);
        let tampered = with_tampered_header(&buf, |h| h.content_hash = [0u8; 32]);

        let decoded = Container::read_from(&mut Cursor::new(&tampered)).unwrap();
        assert_eq!(decoded.header.content_hash, [0u8; 32]);
    }

    /// The structural verifier runs even when the integrity hashes are zero:
    /// a reserved variable flag bit is rejected with the specific violation.
    #[test]
    fn read_from_when_reserved_var_flags_and_zero_hashes_then_verification_failed() {
        let mut container = layout_hash_container();
        container.type_section.as_mut().unwrap().variable_table[0].flags = 0x80;
        let buf = container_bytes(&container);
        let tampered = with_tampered_header(&buf, |h| {
            h.content_hash = [0u8; 32];
            h.debug_hash = [0u8; 32];
            h.layout_hash = [0u8; 32];
        });

        let result = Container::read_from(&mut Cursor::new(&tampered));
        assert!(matches!(
            result,
            Err(ContainerError::VerificationFailed(
                crate::LoadViolation::ReservedVariableFlags { flags: 0x80, .. }
            ))
        ));
    }

    /// A layout hash that does not recompute over the type section is
    /// rejected: a candidate declaring the wrong layout must not reach the
    /// online-change comparison. The content hash is cleared so the
    /// structural verifier, not the integrity check, is what fires (the
    /// masked header the content hash covers includes `layout_hash`).
    #[test]
    fn read_from_when_layout_hash_tampered_then_verification_failed() {
        let container = layout_hash_container();
        let buf = container_bytes(&container);
        let tampered = with_tampered_header(&buf, |h| {
            h.layout_hash = [0xFF; 32];
            h.content_hash = [0u8; 32];
        });

        let result = Container::read_from(&mut Cursor::new(&tampered));
        assert!(matches!(
            result,
            Err(ContainerError::VerificationFailed(
                crate::LoadViolation::LayoutHashMismatch
            ))
        ));
    }

    /// A debug byte corruption invalidates the debug hash; the debug
    /// section is discarded (non-fatal) and the container still loads.
    #[test]
    fn read_from_when_debug_byte_tampered_then_debug_section_discarded() {
        let mut container = layout_hash_container();
        container.debug_section = Some(crate::debug_section::DebugSection {
            var_names: vec![],
            func_names: vec![crate::debug_section::FuncNameEntry {
                function_id: FunctionId::INIT,
                name: "MAIN".into(),
            }],
            line_map: vec![],
            string_layouts: vec![],
            source_files: vec![],
            enum_defs: vec![],
        });
        let mut buf = Vec::new();
        container.write_to(&mut buf).unwrap();
        assert_ne!(buf.len(), 0);
        let last = buf.len() - 1;
        buf[last] = buf[last].wrapping_add(1);

        let decoded = Container::read_from(&mut Cursor::new(&buf)).unwrap();
        assert!(decoded.debug_section.is_none());
    }

    #[test]
    fn container_read_from_when_truncated_inside_code_section_then_section_size_mismatch() {
        let buf = container_bytes(&steel_thread_single_function_container());
        let truncated = &buf[..buf.len() - 1];
        assert!(matches!(
            Container::read_from(&mut Cursor::new(truncated)),
            Err(ContainerError::SectionSizeMismatch)
        ));
    }

    #[test]
    fn container_read_from_when_hashed_and_section_offset_inside_header_then_section_size_mismatch()
    {
        let buf = container_bytes(&steel_thread_single_function_container());
        let tampered = with_tampered_header(&buf, |h| h.code_section_offset = 0);
        assert!(matches!(
            Container::read_from(&mut Cursor::new(&tampered)),
            Err(ContainerError::SectionSizeMismatch)
        ));
    }

    #[test]
    fn container_read_from_when_no_debug_section_then_debug_section_is_none() {
        #[rustfmt::skip]
        let bytecode: Vec<u8> = vec![
            0x01, 0x00, 0x00,
            0x18, 0x00, 0x00,
            0x8C,
        ];

        let container = ContainerBuilder::new()
            .num_variables(1)
            .add_i32_constant(1)
            .add_function(FunctionId::INIT, &bytecode, 1, 1, 0)
            .build();

        let mut buf = Vec::new();
        container.write_to(&mut buf).unwrap();

        let decoded = Container::read_from(&mut Cursor::new(&buf)).unwrap();
        assert_eq!(decoded.header.debug_section_size, 0);
        assert!(decoded.debug_section.is_none());
    }
}
