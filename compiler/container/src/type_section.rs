use std::io::{Read, Write};
use std::vec::Vec;

use crate::id_types::{FbTypeId, FunctionId};
use crate::ContainerError;

/// Type tags for FB field entries.
///
/// These match the `var_type` encoding used in the variable table
/// (see the bytecode container format spec).
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
#[repr(u8)]
pub enum FieldType {
    I32 = 0,
    U32 = 1,
    I64 = 2,
    U64 = 3,
    F32 = 4,
    F64 = 5,
    String = 6,
    WString = 7,
    FbInstance = 8,
    Time = 9,
    /// Heterogeneous structure field slot. Used as the element type in array
    /// descriptors that back structure variables (which are treated as flat
    /// arrays of 8-byte slots). The VM does not check this value at runtime.
    Slot = 10,
}

impl FieldType {
    /// Converts a raw `u8` to a `FieldType`, returning an error for unknown tags.
    pub fn from_u8(v: u8) -> Result<Self, ContainerError> {
        match v {
            0 => Ok(FieldType::I32),
            1 => Ok(FieldType::U32),
            2 => Ok(FieldType::I64),
            3 => Ok(FieldType::U64),
            4 => Ok(FieldType::F32),
            5 => Ok(FieldType::F64),
            6 => Ok(FieldType::String),
            7 => Ok(FieldType::WString),
            8 => Ok(FieldType::FbInstance),
            9 => Ok(FieldType::Time),
            10 => Ok(FieldType::Slot),
            _ => Err(ContainerError::InvalidFieldType(v)),
        }
    }
}

/// A single field entry within an FB type descriptor.
///
/// On disk this is 4 bytes: field_type (u8), reserved (u8), field_extra (u16 LE).
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct FieldEntry {
    pub field_type: FieldType,
    pub field_extra: u16,
}

/// Size of a single field entry on disk in bytes.
const FIELD_ENTRY_SIZE: usize = 4;

/// An FB type descriptor in the type section.
///
/// On disk: type_id (u16 LE), num_fields (u8), reserved (u8), then field entries.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct FbTypeDescriptor {
    pub type_id: FbTypeId,
    pub fields: Vec<FieldEntry>,
}

/// An array descriptor in the type section.
///
/// Describes the element type and total number of elements for a single
/// array shape. Descriptors are deduplicated: multiple variables with
/// the same element type and size share one descriptor.
///
/// On disk this is 8 bytes:
/// `[element_type: u8] [reserved: u8] [total_elements: u32 LE] [element_extra: u16 LE]`
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct ArrayDescriptor {
    /// Element type tag using the same encoding as [`FieldType`]
    /// (I32=0, U32=1, I64=2, U64=3, F32=4, F64=5, etc.).
    pub element_type: u8,
    /// Total number of elements across all dimensions.
    pub total_elements: u32,
    /// Extra type-specific data. For STRING arrays, this holds the
    /// max string length (element stride = STRING_HEADER_BYTES + element_extra).
    /// Zero for primitive element types.
    pub element_extra: u16,
}

impl ArrayDescriptor {
    /// Returns the per-code-unit [`CharWidth`] for a STRING/WSTRING element
    /// array, derived from `element_type`. Wide for [`FieldType::WString`],
    /// narrow otherwise. The VM uses this to size the element stride and to
    /// write element headers (ADR-0035). Non-string arrays return
    /// [`CharWidth::Narrow`]; callers only consult this for string elements.
    pub fn element_char_width(&self) -> crate::CharWidth {
        if self.element_type == FieldType::WString as u8 {
            crate::CharWidth::Wide
        } else {
            crate::CharWidth::Narrow
        }
    }

    /// Returns the byte stride of one element in the data region.
    ///
    /// STRING/WSTRING elements are variable-length regions laid out as
    /// `[max_length: u16][cur_length: u16][encoding: u16][data]` (ADR-0015,
    /// ADR-0035), so their stride depends on `element_extra` (the max length in
    /// code units) and the per-code-unit width. Every other element type
    /// occupies exactly one 8-byte slot.
    pub fn element_stride(&self) -> u32 {
        if self.element_type == FieldType::String as u8
            || self.element_type == FieldType::WString as u8
        {
            crate::STRING_HEADER_BYTES as u32
                + (self.element_extra as u32) * (self.element_char_width().byte_width() as u32)
        } else {
            SLOT_BYTES
        }
    }

    /// Returns the total size in bytes of the data-region span this descriptor
    /// covers, or `None` on overflow.
    ///
    /// This is the single definition of an aggregate's byte size, shared by
    /// codegen (when allocating the region) and the VM (when bounds-checking a
    /// [`crate::opcode::COPY_REGION`]). Structure variables are described as a
    /// flat array of [`FieldType::Slot`] elements, so they are covered too.
    pub fn byte_size(&self) -> Option<u32> {
        self.total_elements.checked_mul(self.element_stride())
    }
}

/// Bytes occupied by a single data-region slot.
pub const SLOT_BYTES: u32 = 8;

/// Size of a single array descriptor on disk in bytes.
const ARRAY_DESCRIPTOR_SIZE: usize = 8;

/// A user-defined function block descriptor in the type section.
///
/// Maps a user-defined FB type ID to the compiled function that implements
/// its body, the variable table offset where its fields are mapped, and
/// the number of data-region fields in the instance.
///
/// On disk: type_id (u16 LE), function_id (u16 LE), var_offset (u16 LE),
/// num_fields (u8), reserved (u8).
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct UserFbDescriptor {
    pub type_id: FbTypeId,
    pub function_id: FunctionId,
    pub var_offset: u16,
    pub num_fields: u8,
}

/// Size of a single user FB descriptor on disk in bytes.
const USER_FB_DESCRIPTOR_SIZE: usize = 8;

/// `VarEntry.flags` bit 0: the variable is an array, and `extra` is the
/// index of its descriptor in the array-descriptor sub-table.
pub const VAR_FLAG_IS_ARRAY: u8 = 0x01;

/// A single variable table entry in the type section.
///
/// On disk this is 4 bytes: var_type (u8), flags (u8), extra (u16 LE).
/// `extra` carries the STRING/WSTRING maximum length, the FB_INSTANCE
/// `fb_type_id`, or the array descriptor index, depending on `var_type`.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct VarEntry {
    pub var_type: FieldType,
    pub flags: u8,
    pub extra: u16,
}

/// Size of a single variable table entry on disk in bytes.
const VAR_ENTRY_SIZE: usize = 4;

/// The type section of a bytecode container.
///
/// Contains FB type descriptors, array descriptors, user FB descriptors and
/// the variable table used by the verifier and VM for type safety checking.
#[derive(Clone, Debug, Default)]
pub struct TypeSection {
    pub fb_types: Vec<FbTypeDescriptor>,
    pub array_descriptors: Vec<ArrayDescriptor>,
    pub user_fb_types: Vec<UserFbDescriptor>,
    pub variable_table: Vec<VarEntry>,
}

impl TypeSection {
    /// Returns the total serialized size of this section in bytes.
    pub fn section_size(&self) -> u32 {
        // FB types: count(2) + sum of (header(4) + fields * 4)
        let mut size: u32 = 2;
        for desc in &self.fb_types {
            size += 4 + desc.fields.len() as u32 * FIELD_ENTRY_SIZE as u32;
        }
        // Array descriptors: count(2) + descriptors * 8
        size += 2 + self.array_descriptors.len() as u32 * ARRAY_DESCRIPTOR_SIZE as u32;
        // User FB descriptors: count(2) + descriptors * 8
        size += 2 + self.user_fb_types.len() as u32 * USER_FB_DESCRIPTOR_SIZE as u32;
        // Variable table: count(2) + entries * 4
        size += 2 + self.variable_table.len() as u32 * VAR_ENTRY_SIZE as u32;
        size
    }

    /// Writes the type section to the given writer.
    ///
    /// Format: FB count (u16 LE), FB descriptors, array count (u16 LE), array
    /// descriptors, user FB count (u16 LE), user FB descriptors, variable
    /// table count (u16 LE), variable entries.
    pub fn write_to(&self, w: &mut impl Write) -> Result<(), ContainerError> {
        // FB type descriptors
        w.write_all(&(self.fb_types.len() as u16).to_le_bytes())?;
        for desc in &self.fb_types {
            w.write_all(&desc.type_id.to_le_bytes())?;
            w.write_all(&[desc.fields.len() as u8])?;
            w.write_all(&[0u8])?; // reserved
            for field in &desc.fields {
                w.write_all(&[field.field_type as u8])?;
                w.write_all(&[0u8])?; // reserved
                w.write_all(&field.field_extra.to_le_bytes())?;
            }
        }

        // Array descriptors
        w.write_all(&(self.array_descriptors.len() as u16).to_le_bytes())?;
        for desc in &self.array_descriptors {
            w.write_all(&[desc.element_type])?;
            w.write_all(&[0u8])?; // reserved
            w.write_all(&desc.total_elements.to_le_bytes())?;
            w.write_all(&desc.element_extra.to_le_bytes())?;
        }

        // User FB descriptors
        w.write_all(&(self.user_fb_types.len() as u16).to_le_bytes())?;
        for desc in &self.user_fb_types {
            w.write_all(&desc.type_id.to_le_bytes())?;
            w.write_all(&desc.function_id.to_le_bytes())?;
            w.write_all(&desc.var_offset.to_le_bytes())?;
            w.write_all(&[desc.num_fields])?;
            w.write_all(&[0u8])?; // reserved
        }

        // Variable table (fourth and last sub-table)
        w.write_all(&(self.variable_table.len() as u16).to_le_bytes())?;
        for entry in &self.variable_table {
            w.write_all(&[entry.var_type as u8, entry.flags])?;
            w.write_all(&entry.extra.to_le_bytes())?;
        }
        Ok(())
    }

    /// Reads a type section from the given reader.
    pub fn read_from(r: &mut impl Read) -> Result<Self, ContainerError> {
        let mut buf2 = [0u8; 2];
        r.read_exact(&mut buf2)?;
        let count = u16::from_le_bytes(buf2) as usize;

        let mut fb_types = Vec::with_capacity(count);
        for _ in 0..count {
            let mut hdr = [0u8; 4];
            r.read_exact(&mut hdr)?;
            let type_id = FbTypeId::new(u16::from_le_bytes([hdr[0], hdr[1]]));
            let num_fields = hdr[2] as usize;
            // hdr[3] is reserved

            let mut fields = Vec::with_capacity(num_fields);
            for _ in 0..num_fields {
                let mut entry_buf = [0u8; FIELD_ENTRY_SIZE];
                r.read_exact(&mut entry_buf)?;
                let field_type = FieldType::from_u8(entry_buf[0])?;
                // entry_buf[1] is reserved
                let field_extra = u16::from_le_bytes([entry_buf[2], entry_buf[3]]);
                fields.push(FieldEntry {
                    field_type,
                    field_extra,
                });
            }

            fb_types.push(FbTypeDescriptor { type_id, fields });
        }

        // Array descriptors
        let mut buf2 = [0u8; 2];
        r.read_exact(&mut buf2)?;
        let array_count = u16::from_le_bytes(buf2) as usize;

        let mut array_descriptors = Vec::with_capacity(array_count);
        for _ in 0..array_count {
            let mut desc_buf = [0u8; ARRAY_DESCRIPTOR_SIZE];
            r.read_exact(&mut desc_buf)?;
            let element_type = desc_buf[0];
            // desc_buf[1] is reserved
            let total_elements =
                u32::from_le_bytes([desc_buf[2], desc_buf[3], desc_buf[4], desc_buf[5]]);
            let element_extra = u16::from_le_bytes([desc_buf[6], desc_buf[7]]);
            array_descriptors.push(ArrayDescriptor {
                element_type,
                total_elements,
                element_extra,
            });
        }

        // User FB descriptors
        let mut buf2 = [0u8; 2];
        let user_fb_count = if r.read_exact(&mut buf2).is_ok() {
            u16::from_le_bytes(buf2) as usize
        } else {
            0
        };

        let mut user_fb_types = Vec::with_capacity(user_fb_count);
        for _ in 0..user_fb_count {
            let mut desc_buf = [0u8; USER_FB_DESCRIPTOR_SIZE];
            r.read_exact(&mut desc_buf)?;
            let type_id = FbTypeId::new(u16::from_le_bytes([desc_buf[0], desc_buf[1]]));
            let function_id = FunctionId::new(u16::from_le_bytes([desc_buf[2], desc_buf[3]]));
            let var_offset = u16::from_le_bytes([desc_buf[4], desc_buf[5]]);
            let num_fields = desc_buf[6];
            // desc_buf[7] is reserved
            user_fb_types.push(UserFbDescriptor {
                type_id,
                function_id,
                var_offset,
                num_fields,
            });
        }

        // Variable table (fourth and last sub-table). A type section that
        // ends before it (a container written before format v4) reads as
        // zero entries, matching the user-FB handling above.
        let mut buf2 = [0u8; 2];
        let var_count = if r.read_exact(&mut buf2).is_ok() {
            u16::from_le_bytes(buf2) as usize
        } else {
            0
        };

        let mut variable_table = Vec::with_capacity(var_count);
        for _ in 0..var_count {
            let mut entry_buf = [0u8; VAR_ENTRY_SIZE];
            r.read_exact(&mut entry_buf)?;
            let var_type = FieldType::from_u8(entry_buf[0])?;
            let extra = u16::from_le_bytes([entry_buf[2], entry_buf[3]]);
            variable_table.push(VarEntry {
                var_type,
                flags: entry_buf[1],
                extra,
            });
        }

        Ok(TypeSection {
            fb_types,
            array_descriptors,
            user_fb_types,
            variable_table,
        })
    }
}

#[cfg(test)]
mod tests {
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
                ArrayDescriptor {
                    element_type: FieldType::I32 as u8,
                    total_elements: 10,
                    element_extra: 0,
                },
                ArrayDescriptor {
                    element_type: FieldType::F64 as u8,
                    total_elements: 32768,
                    element_extra: 0,
                },
            ],
            user_fb_types: vec![],
            variable_table: vec![],
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
            array_descriptors: vec![ArrayDescriptor {
                element_type: FieldType::U32 as u8,
                total_elements: 100,
                element_extra: 0,
            }],
            user_fb_types: vec![],
            variable_table: vec![],
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
        // 2 bytes for each of the four sub-table counts
        assert_eq!(section.section_size(), 8);
    }

    #[test]
    fn section_size_when_array_descriptors_then_includes_descriptor_bytes() {
        let section = TypeSection {
            fb_types: vec![],
            array_descriptors: vec![
                ArrayDescriptor {
                    element_type: 0,
                    total_elements: 10,
                    element_extra: 0,
                },
                ArrayDescriptor {
                    element_type: 4,
                    total_elements: 20,
                    element_extra: 0,
                },
            ],
            user_fb_types: vec![],
            variable_table: vec![],
        };
        // 4 counts(8) + 2 * 8 (descriptors) = 24
        assert_eq!(section.section_size(), 24);
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
        // 4 counts(8) + 2 entries * 4 = 16
        assert_eq!(section.section_size(), 16);
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
        };

        // Header: 4 counts * 2 = 8
        // Per descriptor: 4 (header) + 3 fields * 4 = 16
        // Total: 8 + 16 = 24
        assert_eq!(section.section_size(), 24);

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
}
