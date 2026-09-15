use std::io::{Cursor, Read, Write};
use std::vec::Vec;

use crate::code_section::CodeSection;
use crate::constant_pool::ConstantPool;
use crate::debug_section::DebugSection;
use crate::header::{FileHeader, FLAG_HAS_DEBUG_SECTION, FLAG_HAS_TYPE_SECTION, HEADER_SIZE};
use crate::load_verify::{verify_content_hash, verify_debug_hash, verify_load};
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
    /// Computes the layout hash for online change: BLAKE3 over the variable
    /// table, FB type descriptors and array descriptors, as defined by the
    /// Layout Hash and Online Change formula
    /// (`specs/design/bytecode-container-format.md`). Code, constants and
    /// debug info are excluded, so a logic-only edit yields the same hash
    /// and can be swapped in without restarting.
    ///
    /// [`write_to`](Self::write_to) stores this value in
    /// `header.layout_hash`, along with `content_hash` and `debug_hash`;
    /// the in-memory header keeps zeros until serialized (ADR-0052's hash
    /// contract).
    pub fn compute_layout_hash(&self) -> [u8; 32] {
        let mut hasher = blake3::Hasher::new();
        hasher.update(&self.header.num_variables.to_le_bytes());

        let empty_type_section = TypeSection::default();
        let type_section = self.type_section.as_ref().unwrap_or(&empty_type_section);

        for entry in &type_section.variable_table {
            hasher.update(&[entry.var_type as u8, entry.flags]);
            hasher.update(&entry.extra.to_le_bytes());
        }

        hasher.update(&(type_section.fb_types.len() as u16).to_le_bytes());
        for desc in &type_section.fb_types {
            hasher.update(&[desc.fields.len() as u8]);
            for field in &desc.fields {
                hasher.update(&[field.field_type as u8]);
                hasher.update(&field.field_extra.to_le_bytes());
            }
        }

        hasher.update(&(type_section.array_descriptors.len() as u16).to_le_bytes());
        for desc in &type_section.array_descriptors {
            hasher.update(&[desc.element_type]);
            hasher.update(&desc.total_elements.to_le_bytes());
            hasher.update(&desc.element_extra.to_le_bytes());
        }

        *hasher.finalize().as_bytes()
    }

    /// Writes the container to the given writer.
    ///
    /// Computes section offsets and fills the header before writing sections
    /// in file-layout order. The header carries three integrity hashes, all
    /// computed over the exact bytes written:
    ///
    /// * `content_hash` — BLAKE3 over `type_section || constant_pool ||
    ///   code_section` (the Content Hash Scope; the header, signature
    ///   sections and debug section are excluded).
    /// * `debug_hash` — BLAKE3 over the debug section, or zero when no debug
    ///   section is present.
    /// * `layout_hash` — see [`compute_layout_hash`](Self::compute_layout_hash).
    pub fn write_to(&self, w: &mut impl Write) -> Result<(), ContainerError> {
        // Serialize each section once so the header hashes cover exactly the
        // bytes that follow the header. The sections are small (the header's
        // resource summary bounds their total), so buffering them whole is
        // not a regression over streaming.
        let mut task_bytes = Vec::new();
        self.task_table.write_to(&mut task_bytes)?;

        let type_bytes = match &self.type_section {
            Some(type_section) => {
                let mut bytes = Vec::new();
                type_section.write_to(&mut bytes)?;
                Some(bytes)
            }
            None => None,
        };

        let mut const_bytes = Vec::new();
        self.constant_pool.write_to(&mut const_bytes)?;

        let mut code_bytes = Vec::new();
        self.code.write_to(&mut code_bytes)?;

        let debug_bytes = match &self.debug_section {
            Some(debug) => {
                let mut bytes = Vec::new();
                debug.write_to(&mut bytes)?;
                Some(bytes)
            }
            None => None,
        };

        let task_section_offset = HEADER_SIZE as u32;
        let task_section_size = task_bytes.len() as u32;

        let mut next_offset = task_section_offset + task_section_size;

        let mut header = self.header.clone();
        header.task_section_offset = task_section_offset;
        header.task_section_size = task_section_size;

        // Type section (optional, between task table and constant pool)
        if let Some(bytes) = &type_bytes {
            header.type_section_offset = next_offset;
            header.type_section_size = bytes.len() as u32;
            header.flags |= FLAG_HAS_TYPE_SECTION;
            next_offset += bytes.len() as u32;
        }

        let const_section_offset = next_offset;
        let const_section_size = const_bytes.len() as u32;
        header.const_section_offset = const_section_offset;
        header.const_section_size = const_section_size;
        next_offset = const_section_offset + const_section_size;

        let code_section_offset = next_offset;
        let code_section_size = code_bytes.len() as u32;
        header.code_section_offset = code_section_offset;
        header.code_section_size = code_section_size;
        header.num_functions = self.code.functions.len() as u16;
        next_offset = code_section_offset + code_section_size;

        if let Some(bytes) = &debug_bytes {
            header.debug_section_offset = next_offset;
            header.debug_section_size = bytes.len() as u32;
            header.flags |= FLAG_HAS_DEBUG_SECTION;
        }

        // Content Hash Scope: type || constant || code, in file order.
        let mut content_hasher = blake3::Hasher::new();
        if let Some(bytes) = &type_bytes {
            content_hasher.update(bytes);
        }
        content_hasher.update(&const_bytes);
        content_hasher.update(&code_bytes);
        header.content_hash = *content_hasher.finalize().as_bytes();

        header.debug_hash = match &debug_bytes {
            Some(bytes) => *blake3::hash(bytes).as_bytes(),
            None => [0u8; 32],
        };

        header.layout_hash = self.compute_layout_hash();

        header.write_to(w)?;
        w.write_all(&task_bytes)?;
        if let Some(bytes) = &type_bytes {
            w.write_all(bytes)?;
        }
        w.write_all(&const_bytes)?;
        w.write_all(&code_bytes)?;
        if let Some(bytes) = &debug_bytes {
            w.write_all(bytes)?;
        }

        Ok(())
    }

    /// Reads a container from the given reader.
    ///
    /// Loads only a container that passes the ADR-0006 load-time checks
    /// (see [`verify_load`](crate::verify_load)): when `content_hash` is
    /// nonzero it must match the type, constant and code sections, and the
    /// type section's tables must be internally consistent; a zero hash is a
    /// legacy container and is accepted. When `debug_hash` is nonzero but
    /// does not match, the debug section is discarded (non-fatal), matching
    /// the loading sequence's step 13.
    pub fn read_from(r: &mut impl Read) -> Result<Self, ContainerError> {
        let header = FileHeader::read_from(r)?;

        // Read remaining bytes after the header so we can seek to
        // section offsets within them.
        let mut rest = Vec::new();
        r.read_to_end(&mut rest)?;

        verify_content_hash(&header, &rest)?;

        let base = HEADER_SIZE as u32;

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

        // Parse debug section if present (non-fatal on error).
        let mut debug_section = if header.debug_section_size > 0 {
            let debug_start = (header.debug_section_offset - base) as usize;
            let debug_end = debug_start + header.debug_section_size as usize;
            if debug_end <= rest.len() {
                DebugSection::read_from(&mut Cursor::new(&rest[debug_start..debug_end])).ok()
            } else {
                None
            }
        } else {
            None
        };

        // A debug hash that does not match discards the debug section rather
        // than the container (loading sequence step 13: invalid debug info
        // is non-fatal).
        if verify_debug_hash(&header, &rest).is_err() {
            debug_section = None;
        }

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

    /// Rebuilds the byte image with the header rewritten by `f`. The body
    /// bytes (including every section and its hashes) are left untouched.
    fn with_tampered_header(bytes: &[u8], f: impl FnOnce(&mut FileHeader)) -> Vec<u8> {
        let mut header = FileHeader::read_from(&mut Cursor::new(&bytes[..HEADER_SIZE])).unwrap();
        f(&mut header);

        let mut tampered = Vec::with_capacity(bytes.len());
        header.write_to(&mut tampered).unwrap();
        tampered.extend_from_slice(&bytes[HEADER_SIZE..]);
        tampered
    }

    /// A tampered type-section size makes the directory inconsistent with
    /// the file: the type section no longer parses, so the layout hash no
    /// longer recomputes over the declared variable table and load-time
    /// verification rejects the container.
    #[test]
    fn container_read_from_when_type_section_size_tampered_then_rejected() {
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

        // Inflate the declared type_section_size so the section no longer
        // fits the file and the reader treats it as absent.
        let tampered = with_tampered_header(&buf, |h| {
            h.type_section_size = buf.len() as u32 * 2;
        });

        let result = Container::read_from(&mut Cursor::new(&tampered));
        assert!(matches!(
            result,
            Err(ContainerError::VerificationFailed(
                crate::LoadViolation::LayoutHashMismatch
            ))
        ));
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

        // Inflate the declared debug_section_size past the end of the buffer.
        let tampered = with_tampered_header(&buf, |h| {
            h.debug_section_size = buf.len() as u32 * 2;
        });

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
    fn compute_layout_hash_when_array_descriptor_changes_then_differs() {
        let first = layout_hash_container();
        let mut second = layout_hash_container();
        second.type_section.as_mut().unwrap().array_descriptors[0].total_elements = 5;

        assert_ne!(first.compute_layout_hash(), second.compute_layout_hash());
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
        assert!(matches!(
            result,
            Err(ContainerError::VerificationFailed(
                crate::LoadViolation::ContentHashMismatch
            ))
        ));
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
    /// online-change comparison.
    #[test]
    fn read_from_when_layout_hash_tampered_then_verification_failed() {
        let container = layout_hash_container();
        let buf = container_bytes(&container);
        let tampered = with_tampered_header(&buf, |h| h.layout_hash = [0xFF; 32]);

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
