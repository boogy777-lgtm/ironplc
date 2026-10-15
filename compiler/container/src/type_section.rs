#[cfg(feature = "std")]
use std::io::{Read, Write};
#[cfg(feature = "std")]
use std::vec::Vec;

#[cfg(feature = "std")]
use crate::id_types::VarIndex;
use crate::id_types::{FbTypeId, FunctionId};
#[cfg(feature = "std")]
use crate::persistent_extent::PersistentExtent;
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

impl FieldEntry {
    /// Serialized size of one field entry in bytes.
    pub const SIZE: usize = 4;
}

/// Serialized size of the fixed part of an FB type descriptor: `type_id`
/// (u16), `num_fields` (u8) and one reserved byte. Field entries follow.
#[cfg(feature = "std")]
pub(crate) const FB_TYPE_DESCRIPTOR_HEADER_SIZE: usize = 4;

/// An FB type descriptor in the type section.
///
/// On disk: type_id (u16 LE), num_fields (u8), reserved (u8), then field entries.
#[cfg(feature = "std")]
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct FbTypeDescriptor {
    pub type_id: FbTypeId,
    pub fields: Vec<FieldEntry>,
}

/// An array descriptor in the type section.
///
/// Describes the element type, total number of elements and element stride
/// for a single array shape. Descriptors are deduplicated: multiple variables
/// with the same shape share one descriptor.
///
/// On disk this is 12 bytes:
/// `[element_type: u8] [reserved: u8] [total_elements: u32 LE] [element_extra: u16 LE] [element_stride: u32 LE]`
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct ArrayDescriptor {
    /// Element type tag using the same encoding as [`FieldType`]
    /// (I32=0, U32=1, I64=2, U64=3, F32=4, F64=5, etc.).
    pub element_type: u8,
    /// Total number of elements across all dimensions.
    pub total_elements: u32,
    /// Extra type-specific data. For STRING arrays, this holds the
    /// max string length in code units. Zero for primitive element types.
    pub element_extra: u16,
    /// Byte distance between the starts of consecutive elements.
    ///
    /// Usually the element's own size ([`ArrayDescriptor::natural_stride`]).
    /// It is larger when the elements are fields of consecutive structures,
    /// such as the STRING field of each element of an array of structures,
    /// where it is the size of one structure (ADR-0069).
    pub element_stride: u32,
}

impl ArrayDescriptor {
    /// Creates a descriptor whose elements are packed back to back, at the
    /// element type's natural stride.
    pub fn new(element_type: u8, total_elements: u32, element_extra: u16) -> Self {
        Self {
            element_type,
            total_elements,
            element_extra,
            element_stride: Self::natural_stride(element_type, element_extra),
        }
    }

    /// Returns the size in bytes of one element of the given type.
    ///
    /// STRING/WSTRING elements are variable-length regions laid out as
    /// `[max_length: u16][cur_length: u16][encoding: u16][data]` (ADR-0015,
    /// ADR-0035), so their size depends on `element_extra` (the max length in
    /// code units) and the per-code-unit width. Every other element type
    /// occupies exactly one 8-byte slot.
    pub fn natural_stride(element_type: u8, element_extra: u16) -> u32 {
        if element_type == FieldType::String as u8 {
            crate::STRING_HEADER_BYTES as u32 + element_extra as u32
        } else if element_type == FieldType::WString as u8 {
            crate::STRING_HEADER_BYTES as u32
                + element_extra as u32 * crate::CharWidth::Wide.byte_width() as u32
        } else {
            SLOT_BYTES
        }
    }

    /// Serialized size of one descriptor in bytes.
    pub const SIZE: usize = 12;

    /// Decodes a descriptor from its serialized bytes. The element type is
    /// kept as the raw tag and the stride is not checked, so any 12 bytes
    /// decode; a reader that must reject an impossible stride calls
    /// [`validate`](Self::validate) on the result.
    pub fn from_bytes(buf: &[u8; Self::SIZE]) -> Self {
        ArrayDescriptor {
            element_type: buf[0],
            // buf[1] is reserved
            total_elements: u32::from_le_bytes([buf[2], buf[3], buf[4], buf[5]]),
            element_extra: u16::from_le_bytes([buf[6], buf[7]]),
            element_stride: u32::from_le_bytes([buf[8], buf[9], buf[10], buf[11]]),
        }
    }

    /// Encodes the descriptor into its serialized bytes.
    pub fn to_bytes(&self) -> [u8; Self::SIZE] {
        let mut buf = [0u8; Self::SIZE];
        buf[0] = self.element_type;
        buf[2..6].copy_from_slice(&self.total_elements.to_le_bytes());
        buf[6..8].copy_from_slice(&self.element_extra.to_le_bytes());
        buf[8..12].copy_from_slice(&self.element_stride.to_le_bytes());
        buf
    }

    /// Returns the per-code-unit [`CharWidth`] for a STRING/WSTRING element
    /// array, derived from `element_type`. Wide for [`FieldType::WString`],
    /// narrow otherwise. The VM uses this to write element headers
    /// (ADR-0035). Non-string arrays return [`CharWidth::Narrow`]; callers
    /// only consult this for string elements.
    pub fn element_char_width(&self) -> crate::CharWidth {
        if self.element_type == FieldType::WString as u8 {
            crate::CharWidth::Wide
        } else {
            crate::CharWidth::Narrow
        }
    }

    /// Returns the byte distance between the starts of consecutive elements.
    pub fn element_stride(&self) -> u32 {
        self.element_stride
    }

    /// Checks that the stride is one the VM can honour.
    ///
    /// A STRING/WSTRING stride may exceed the element's size but not fall
    /// below it, which would make elements overlap. Every other element type
    /// must use exactly one slot, because `LOAD_ARRAY` and `STORE_ARRAY`
    /// step by one slot regardless of the descriptor.
    pub fn validate(&self) -> Result<(), ContainerError> {
        let natural = Self::natural_stride(self.element_type, self.element_extra);
        let is_string = self.element_type == FieldType::String as u8
            || self.element_type == FieldType::WString as u8;
        let valid = if is_string {
            self.element_stride >= natural
        } else {
            self.element_stride == natural
        };
        if valid {
            Ok(())
        } else {
            Err(ContainerError::InvalidArrayStride {
                element_type: self.element_type,
                element_stride: self.element_stride,
            })
        }
    }

    /// Returns the total size in bytes of the data-region span this descriptor
    /// covers, or `None` on overflow.
    ///
    /// This is the single definition of an aggregate's byte size, shared by
    /// codegen (when allocating the region) and the VM (when bounds-checking a
    /// [`crate::opcode::COPY_REGION`]). Structure variables are described as a
    /// flat array of [`FieldType::Slot`] elements, so they are covered too.
    ///
    /// Only meaningful for a descriptor at its natural stride: a strided
    /// descriptor addresses one field of each of a run of structures, which
    /// is not a contiguous span of its own. Codegen never passes one to
    /// `COPY_REGION`.
    pub fn byte_size(&self) -> Option<u32> {
        self.total_elements.checked_mul(self.element_stride())
    }
}

/// Bytes occupied by a single data-region slot.
pub const SLOT_BYTES: u32 = 8;

/// A user-defined function block descriptor in the type section.
///
/// Maps a user-defined FB type ID to the compiled function that implements
/// its body, the variable table offset where its fields are mapped, and
/// the number of data-region fields in the instance.
///
/// On disk: type_id (u16 LE), function_id (u16 LE), var_offset (u16 LE),
/// num_fields (u8), reserved (u8).
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct UserFbDescriptor {
    pub type_id: FbTypeId,
    pub function_id: FunctionId,
    pub var_offset: u16,
    pub num_fields: u8,
}

impl UserFbDescriptor {
    /// Serialized size of one descriptor in bytes.
    pub const SIZE: usize = 8;

    /// Decodes a descriptor from its serialized bytes. Every field is a
    /// plain integer, so any 8 bytes decode.
    pub fn from_bytes(buf: &[u8; Self::SIZE]) -> Self {
        UserFbDescriptor {
            type_id: FbTypeId::new(u16::from_le_bytes([buf[0], buf[1]])),
            function_id: FunctionId::new(u16::from_le_bytes([buf[2], buf[3]])),
            var_offset: u16::from_le_bytes([buf[4], buf[5]]),
            num_fields: buf[6],
            // buf[7] is reserved
        }
    }

    /// Encodes the descriptor into its serialized bytes.
    pub fn to_bytes(&self) -> [u8; Self::SIZE] {
        let mut buf = [0u8; Self::SIZE];
        buf[0..2].copy_from_slice(&self.type_id.to_le_bytes());
        buf[2..4].copy_from_slice(&self.function_id.to_le_bytes());
        buf[4..6].copy_from_slice(&self.var_offset.to_le_bytes());
        buf[6] = self.num_fields;
        buf
    }
}

/// `VarEntry.flags` bit 0: the variable is an array, and `extra` is the
/// index of its descriptor in the array-descriptor sub-table.
#[cfg(feature = "std")]
pub const VAR_FLAG_IS_ARRAY: u8 = 0x01;

/// A single variable table entry in the type section.
///
/// On disk this is 4 bytes: var_type (u8), flags (u8), extra (u16 LE).
/// `extra` carries the STRING/WSTRING maximum length, the FB_INSTANCE
/// `fb_type_id`, or the array descriptor index, depending on `var_type`.
#[cfg(feature = "std")]
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct VarEntry {
    pub var_type: FieldType,
    pub flags: u8,
    pub extra: u16,
}

/// Size of a single variable table entry on disk in bytes.
#[cfg(feature = "std")]
const VAR_ENTRY_SIZE: usize = 4;

/// A stable variable ID entry in the type section.
///
/// Maps a persistent variable's compiler-assigned index to the
/// engineering-side entity UID. The UID identifies the declaration, not its
/// name: a rename keeps the UID, so the variable's type and value stay with
/// the entity. Transient slots (function locals, scratch) have no entry.
///
/// On disk this is 10 bytes: var_index (u16 LE), uid (u64 LE).
#[cfg(feature = "std")]
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct StableVarEntry {
    pub var_index: VarIndex,
    pub uid: u64,
}

/// Size of a single stable variable ID entry on disk in bytes.
#[cfg(feature = "std")]
const STABLE_VAR_ENTRY_SIZE: usize = 10;

/// A stable function-block field UID entry in the type section.
///
/// Maps one field of a user-defined FB type to the engineering-side entity
/// UID of the field declaration (ADR 0059). The UID identifies the field, not
/// its position: inserting a field into the FB type shifts the field
/// ordinals, but a field's UID (and therefore its value) stays with it. Only
/// user-defined FB types carry entries; standard-library FBs (TON, ...)
/// have fixed, VM-owned layouts and none.
///
/// On disk this is 11 bytes: fb_type_id (u16 LE), field_index (u8),
/// uid (u64 LE). Entries ascend by `(fb_type_id, field_index)`.
#[cfg(feature = "std")]
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct FbFieldUidEntry {
    pub fb_type_id: FbTypeId,
    /// Ordinal position of the field within the type's field list
    /// (the same ordering the user FB descriptor's `num_fields` counts).
    pub field_index: u8,
    pub uid: u64,
}

/// Size of a single FB field UID entry on disk in bytes.
#[cfg(feature = "std")]
const FB_FIELD_UID_ENTRY_SIZE: usize = 11;

/// The type section of a bytecode container.
///
/// Contains FB type descriptors, array descriptors, user FB descriptors, the
/// variable table used by the verifier and VM for type safety checking, the
/// stable variable ID table used by online change for per-variable
/// migration, and the FB field UID table used for per-field FB instance
/// migration.
#[cfg(feature = "std")]
#[derive(Clone, Debug, Default)]
pub struct TypeSection {
    pub fb_types: Vec<FbTypeDescriptor>,
    pub array_descriptors: Vec<ArrayDescriptor>,
    pub user_fb_types: Vec<UserFbDescriptor>,
    pub variable_table: Vec<VarEntry>,
    /// Stable variable IDs in ascending `var_index` order. Callers (codegen)
    /// own the ordering contract; the writer preserves the given order.
    pub stable_vars: Vec<StableVarEntry>,
    /// FB field UIDs in ascending `(fb_type_id, field_index)` order.
    /// Callers (codegen) own the ordering contract; the writer preserves
    /// the given order.
    pub fb_field_uids: Vec<FbFieldUidEntry>,
    /// The persistent extent, one row per program instance (format v8).
    /// Empty when the container declares none; read through
    /// [`Container::persistent_extents`](crate::Container::persistent_extents),
    /// which applies that default.
    pub persistent_extents: Vec<PersistentExtent>,
}

#[cfg(feature = "std")]
impl TypeSection {
    /// Returns the total serialized size of this section in bytes.
    pub fn section_size(&self) -> u32 {
        // FB types: count(2) + sum of (header(4) + fields * 4)
        let mut size: u32 = 2;
        for desc in &self.fb_types {
            size += FB_TYPE_DESCRIPTOR_HEADER_SIZE as u32
                + desc.fields.len() as u32 * FieldEntry::SIZE as u32;
        }
        // Array descriptors: count(2) + descriptors * 12
        size += 2 + self.array_descriptors.len() as u32 * ArrayDescriptor::SIZE as u32;
        // User FB descriptors: count(2) + descriptors * 8
        size += 2 + self.user_fb_types.len() as u32 * UserFbDescriptor::SIZE as u32;
        // Variable table: count(2) + entries * 4
        size += 2 + self.variable_table.len() as u32 * VAR_ENTRY_SIZE as u32;
        // Stable variable IDs: count(2) + entries * 10
        size += 2 + self.stable_vars.len() as u32 * STABLE_VAR_ENTRY_SIZE as u32;
        // FB field UIDs: count(2) + entries * 11
        size += 2 + self.fb_field_uids.len() as u32 * FB_FIELD_UID_ENTRY_SIZE as u32;
        // Persistent extents: count(2) + rows * 14
        size += 2 + self.persistent_extents.len() as u32 * PersistentExtent::SIZE as u32;
        size
    }

    /// Writes the type section to the given writer.
    ///
    /// Format: FB count (u16 LE), FB descriptors, array count (u16 LE), array
    /// descriptors, user FB count (u16 LE), user FB descriptors, variable
    /// table count (u16 LE), variable entries, stable variable ID count
    /// (u16 LE), stable variable ID entries, FB field UID count (u16 LE),
    /// FB field UID entries, persistent extent count (u16 LE), persistent
    /// extent rows.
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
            w.write_all(&desc.to_bytes())?;
        }

        // User FB descriptors
        w.write_all(&(self.user_fb_types.len() as u16).to_le_bytes())?;
        for desc in &self.user_fb_types {
            w.write_all(&desc.to_bytes())?;
        }

        // Variable table (fourth sub-table)
        w.write_all(&(self.variable_table.len() as u16).to_le_bytes())?;
        for entry in &self.variable_table {
            w.write_all(&[entry.var_type as u8, entry.flags])?;
            w.write_all(&entry.extra.to_le_bytes())?;
        }

        // Stable variable IDs (fifth sub-table), in the caller's
        // ascending var_index order.
        w.write_all(&(self.stable_vars.len() as u16).to_le_bytes())?;
        for entry in &self.stable_vars {
            w.write_all(&entry.var_index.to_le_bytes())?;
            w.write_all(&entry.uid.to_le_bytes())?;
        }

        // FB field UIDs (sixth sub-table), in the caller's ascending
        // (fb_type_id, field_index) order.
        w.write_all(&(self.fb_field_uids.len() as u16).to_le_bytes())?;
        for entry in &self.fb_field_uids {
            w.write_all(&entry.fb_type_id.to_le_bytes())?;
            w.write_all(&[entry.field_index])?;
            w.write_all(&entry.uid.to_le_bytes())?;
        }

        // Persistent extents (seventh and last sub-table), in the caller's
        // order.
        w.write_all(&(self.persistent_extents.len() as u16).to_le_bytes())?;
        for row in &self.persistent_extents {
            w.write_all(&row.to_bytes())?;
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
                let mut entry_buf = [0u8; FieldEntry::SIZE];
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
            let mut desc_buf = [0u8; ArrayDescriptor::SIZE];
            r.read_exact(&mut desc_buf)?;
            let desc = ArrayDescriptor::from_bytes(&desc_buf);
            desc.validate()?;
            array_descriptors.push(desc);
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
            let mut desc_buf = [0u8; UserFbDescriptor::SIZE];
            r.read_exact(&mut desc_buf)?;
            user_fb_types.push(UserFbDescriptor::from_bytes(&desc_buf));
        }

        // Variable table (fourth sub-table). A type section that ends
        // before it (a container written before format v4) reads as
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

        // Stable variable IDs (fifth and last sub-table). A type section
        // that ends before it (a container written before format v5) reads
        // as zero entries.
        let mut buf2 = [0u8; 2];
        let stable_var_count = if r.read_exact(&mut buf2).is_ok() {
            u16::from_le_bytes(buf2) as usize
        } else {
            0
        };

        let mut stable_vars = Vec::with_capacity(stable_var_count);
        for _ in 0..stable_var_count {
            let mut entry_buf = [0u8; STABLE_VAR_ENTRY_SIZE];
            r.read_exact(&mut entry_buf)?;
            stable_vars.push(StableVarEntry {
                var_index: VarIndex::new(u16::from_le_bytes([entry_buf[0], entry_buf[1]])),
                uid: u64::from_le_bytes([
                    entry_buf[2],
                    entry_buf[3],
                    entry_buf[4],
                    entry_buf[5],
                    entry_buf[6],
                    entry_buf[7],
                    entry_buf[8],
                    entry_buf[9],
                ]),
            });
        }

        // FB field UIDs (sixth sub-table). A type section that ends before
        // it (a container written before format v6) reads as zero entries.
        let mut buf2 = [0u8; 2];
        let fb_field_uid_count = if r.read_exact(&mut buf2).is_ok() {
            u16::from_le_bytes(buf2) as usize
        } else {
            0
        };

        let mut fb_field_uids = Vec::with_capacity(fb_field_uid_count);
        for _ in 0..fb_field_uid_count {
            let mut entry_buf = [0u8; FB_FIELD_UID_ENTRY_SIZE];
            r.read_exact(&mut entry_buf)?;
            fb_field_uids.push(FbFieldUidEntry {
                fb_type_id: FbTypeId::new(u16::from_le_bytes([entry_buf[0], entry_buf[1]])),
                field_index: entry_buf[2],
                uid: u64::from_le_bytes([
                    entry_buf[3],
                    entry_buf[4],
                    entry_buf[5],
                    entry_buf[6],
                    entry_buf[7],
                    entry_buf[8],
                    entry_buf[9],
                    entry_buf[10],
                ]),
            });
        }

        // Persistent extents (seventh and last sub-table). A type section
        // that ends before it (a container written before format v8) reads as
        // zero rows.
        let mut buf2 = [0u8; 2];
        let extent_count = if r.read_exact(&mut buf2).is_ok() {
            u16::from_le_bytes(buf2) as usize
        } else {
            0
        };

        let mut persistent_extents = Vec::with_capacity(extent_count);
        for _ in 0..extent_count {
            let mut row_buf = [0u8; PersistentExtent::SIZE];
            r.read_exact(&mut row_buf)?;
            persistent_extents.push(PersistentExtent::from_bytes(&row_buf));
        }

        Ok(TypeSection {
            fb_types,
            array_descriptors,
            user_fb_types,
            variable_table,
            stable_vars,
            fb_field_uids,
            persistent_extents,
        })
    }
}

#[cfg(test)]
mod tests;
