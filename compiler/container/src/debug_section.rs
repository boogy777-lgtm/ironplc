use std::io::{Read, Write};
use std::string::String;
use std::vec;
use std::vec::Vec;

use crate::id_types::{FunctionId, SourceColumn, SourceFileId, SourceLine, VarIndex};
use crate::ContainerError;

// Sub-table tag constants.
const TAG_LINE_MAP: u16 = 1;
const TAG_VAR_NAME: u16 = 2;
const TAG_FUNC_NAME: u16 = 3;
const TAG_STRING_LAYOUT: u16 = 4;
const TAG_SOURCE_FILE: u16 = 6;
const TAG_ENUM_DEF: u16 = 9;

/// Size of each StringLayoutEntry on disk: var_index(2) + data_offset(4) + max_length(2) = 8 bytes.
const STRING_LAYOUT_ENTRY_SIZE: u32 = 8;

/// Size of each LineMapEntry on disk: function_id(2) + bytecode_offset(2)
/// + file_id(2) + source_line(2) + source_column(2) = 10 bytes.
const LINE_MAP_ENTRY_SIZE: u32 = 10;

/// BLAKE3 digest size in bytes (32, default output length).
pub const SOURCE_FILE_HASH_LEN: usize = 32;

/// Size of each directory entry: tag(2) + reserved(2) + size(4) = 8 bytes.
const DIR_ENTRY_SIZE: u32 = 8;

/// IEC 61131-3 type tag for debug display interpretation.
///
/// See ADR-0019 for the full encoding table and rationale.
pub mod iec_type_tag {
    pub const BOOL: u8 = 0;
    pub const SINT: u8 = 1;
    pub const INT: u8 = 2;
    pub const DINT: u8 = 3;
    pub const LINT: u8 = 4;
    pub const USINT: u8 = 5;
    pub const UINT: u8 = 6;
    pub const UDINT: u8 = 7;
    pub const ULINT: u8 = 8;
    pub const REAL: u8 = 9;
    pub const LREAL: u8 = 10;
    pub const BYTE: u8 = 11;
    pub const WORD: u8 = 12;
    pub const DWORD: u8 = 13;
    pub const LWORD: u8 = 14;
    pub const STRING: u8 = 15;
    pub const WSTRING: u8 = 16;
    pub const TIME: u8 = 17;
    pub const LTIME: u8 = 18;
    pub const DATE: u8 = 19;
    pub const LDATE: u8 = 20;
    pub const TIME_OF_DAY: u8 = 21;
    pub const LTOD: u8 = 22;
    pub const DATE_AND_TIME: u8 = 23;
    pub const LDT: u8 = 24;
    /// A structure variable. Its slot holds the value's byte offset into the
    /// data region, not the value, so a renderer must not display the slot.
    pub const STRUCT: u8 = 25;
    /// An array variable, including an array of structures. As for
    /// [`STRUCT`], the slot holds a data-region offset.
    pub const ARRAY: u8 = 26;
    /// A function-block instance. As for [`STRUCT`], the slot holds a
    /// data-region offset.
    pub const FB_INSTANCE: u8 = 27;
    /// Anything else. The slot may or may not hold the value; `type_name`
    /// carries whatever the compiler knew. Named subrange types land here and
    /// *do* hold their value, which is why the aggregate tags above are
    /// distinct rather than folded into this one.
    pub const OTHER: u8 = 255;
}

/// Function ID constants for debug variable ownership.
pub mod function_id {
    use crate::id_types::FunctionId;
    /// Indicates a variable belongs to program/global scope (not a specific function).
    pub const GLOBAL_SCOPE: FunctionId = FunctionId::GLOBAL_SCOPE;
}

/// IEC 61131-3 variable section encoding.
pub mod var_section {
    pub const VAR: u8 = 0;
    pub const VAR_TEMP: u8 = 1;
    pub const VAR_INPUT: u8 = 2;
    pub const VAR_OUTPUT: u8 = 3;
    pub const VAR_IN_OUT: u8 = 4;
    pub const VAR_EXTERNAL: u8 = 5;
    pub const VAR_GLOBAL: u8 = 6;
}

/// A variable name entry (debug section Tag 2).
#[derive(Clone, Debug, PartialEq)]
pub struct VarNameEntry {
    pub var_index: VarIndex,
    pub function_id: FunctionId,
    pub var_section: u8,
    pub iec_type_tag: u8,
    pub name: String,
    pub type_name: String,
}

/// A bytecode-offset → source-location mapping entry (debug section Tag 1).
///
/// Each entry maps a single bytecode offset within a function to a 1-based
/// source line and column. This enables breakpoints, stepping, and stack
/// traces in a debugger. See the bytecode container format spec, Tag 1
/// (LINE_MAP), for the binary layout.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct LineMapEntry {
    /// Function containing this mapping.
    pub function_id: FunctionId,
    /// Offset within the function's bytecode.
    pub bytecode_offset: u16,
    /// Index into the SOURCE_FILE_TABLE (debug section Tag 6). Identifies
    /// which source file `source_line`/`source_column` refer to. Entries
    /// from a container without a source file table all carry the default
    /// `SourceFileId(0)`.
    pub file_id: SourceFileId,
    /// Source line number (1-based).
    pub source_line: SourceLine,
    /// Source column number (1-based; 0 = unknown).
    pub source_column: SourceColumn,
}

/// A source file table entry (debug section Tag 6).
///
/// One entry per distinct source file referenced by the line map. The
/// table is index-addressed: `LineMapEntry.file_id` is an index into
/// `DebugSection.source_files`. The `content_hash` is BLAKE3 over the
/// exact source bytes the parser saw, so a debugger can detect drift
/// against the user's working copy.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct SourceFileEntry {
    /// Path identifying the source file. Format is whatever the compiler
    /// driver records (typically an absolute or workspace-relative path).
    pub path: String,
    /// BLAKE3-256 of the source bytes; all-zero means "no hash available".
    pub content_hash: [u8; SOURCE_FILE_HASH_LEN],
}

/// A function name entry (debug section Tag 3).
#[derive(Clone, Debug, PartialEq)]
pub struct FuncNameEntry {
    pub function_id: FunctionId,
    pub name: String,
}

/// Layout of a STRING variable in the data region (debug section Tag 4).
///
/// STRING values do not live in the variable table — the slot is unused.
/// The actual bytes live at `data_offset` in the data region with the
/// layout `[max_len: u16][cur_len: u16][bytes…]`. Tools that render
/// variable values use this entry to locate and read the string.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct StringLayoutEntry {
    pub var_index: VarIndex,
    pub data_offset: u32,
    pub max_length: u16,
}

/// An enumeration type definition entry (debug section Tag 9).
///
/// Maps a named enumeration type to its value names in ordinal order.
/// See `specs/design/enumeration-codegen.md`; the ENUM_DEF payload format is
/// REQ-EN-container-061 (owned by this crate).
#[derive(Clone, Debug, PartialEq)]
pub struct EnumDefEntry {
    /// The user-defined type name (e.g., "COLOR").
    pub type_name: String,
    /// Value names in ordinal order (e.g., ["RED", "GREEN", "BLUE"]).
    pub values: Vec<String>,
}

/// The debug section of a bytecode container.
#[derive(Clone, Debug, Default)]
pub struct DebugSection {
    pub var_names: Vec<VarNameEntry>,
    pub func_names: Vec<FuncNameEntry>,
    /// Bytecode-offset → source-location mappings (debug section Tag 1).
    /// Empty when no source map is present.
    ///
    /// Invariant: entries are sorted by `(function_id, bytecode_offset)` so
    /// that [`lookup_source_location`](Self::lookup_source_location) can use
    /// a binary search. [`crate::builder::ContainerBuilder::build`] restores
    /// this order; callers constructing a `DebugSection` directly (or
    /// hand-crafting on-disk bytes) must do so themselves.
    pub line_map: Vec<LineMapEntry>,
    /// STRING variable data-region layouts (debug section Tag 4).
    pub string_layouts: Vec<StringLayoutEntry>,
    /// Source file table (debug section Tag 6).
    ///
    /// Index-addressed by `LineMapEntry.file_id`. Each entry pairs a path
    /// with a BLAKE3 hash of the file's source bytes for drift detection.
    pub source_files: Vec<SourceFileEntry>,
    /// Enumeration type definitions (debug section Tag 9).
    /// Maps enum type names to their value names in ordinal order.
    pub enum_defs: Vec<EnumDefEntry>,
}

/// Sorts a line map by `(function_id, bytecode_offset)` to satisfy the
/// invariant required by [`DebugSection::lookup_source_location`].
pub(crate) fn sort_line_map(entries: &mut [LineMapEntry]) {
    entries.sort_by_key(|e| (e.function_id.raw(), e.bytecode_offset));
}

impl DebugSection {
    /// Returns the serialized size of this debug section in bytes.
    pub fn section_size(&self) -> u32 {
        let sub_table_count: u32 = self.num_sub_tables() as u32;
        // Header: sub_table_count(2) + directory entries
        let header_size = 2 + sub_table_count * DIR_ENTRY_SIZE;
        header_size
            + self.line_map_payload_size()
            + self.var_name_payload_size()
            + self.func_name_payload_size()
            + self.string_layout_payload_size()
            + self.source_file_payload_size()
            + self.enum_def_payload_size()
    }

    /// Writes the debug section to the given writer.
    pub fn write_to(&self, w: &mut impl Write) -> Result<(), ContainerError> {
        let sub_table_count = self.num_sub_tables();
        w.write_all(&(sub_table_count as u16).to_le_bytes())?;

        // Write directory entries for present sub-tables.
        if !self.line_map.is_empty() {
            w.write_all(&TAG_LINE_MAP.to_le_bytes())?;
            w.write_all(&0u16.to_le_bytes())?; // reserved
            w.write_all(&self.line_map_payload_size().to_le_bytes())?;
        }
        if !self.var_names.is_empty() {
            w.write_all(&TAG_VAR_NAME.to_le_bytes())?;
            w.write_all(&0u16.to_le_bytes())?; // reserved
            w.write_all(&self.var_name_payload_size().to_le_bytes())?;
        }
        if !self.func_names.is_empty() {
            w.write_all(&TAG_FUNC_NAME.to_le_bytes())?;
            w.write_all(&0u16.to_le_bytes())?; // reserved
            w.write_all(&self.func_name_payload_size().to_le_bytes())?;
        }
        if !self.string_layouts.is_empty() {
            w.write_all(&TAG_STRING_LAYOUT.to_le_bytes())?;
            w.write_all(&0u16.to_le_bytes())?; // reserved
            w.write_all(&self.string_layout_payload_size().to_le_bytes())?;
        }
        if !self.source_files.is_empty() {
            w.write_all(&TAG_SOURCE_FILE.to_le_bytes())?;
            w.write_all(&0u16.to_le_bytes())?; // reserved
            w.write_all(&self.source_file_payload_size().to_le_bytes())?;
        }
        if !self.enum_defs.is_empty() {
            w.write_all(&TAG_ENUM_DEF.to_le_bytes())?;
            w.write_all(&0u16.to_le_bytes())?; // reserved
            w.write_all(&self.enum_def_payload_size().to_le_bytes())?;
        }

        // Write payloads in directory order.
        if !self.line_map.is_empty() {
            self.write_line_map(w)?;
        }
        if !self.var_names.is_empty() {
            self.write_var_names(w)?;
        }
        if !self.func_names.is_empty() {
            self.write_func_names(w)?;
        }
        if !self.string_layouts.is_empty() {
            self.write_string_layouts(w)?;
        }
        if !self.source_files.is_empty() {
            self.write_source_files(w)?;
        }
        if !self.enum_defs.is_empty() {
            self.write_enum_defs(w)?;
        }

        Ok(())
    }

    /// Reads a debug section from the given reader.
    pub fn read_from(r: &mut impl Read) -> Result<Self, ContainerError> {
        let mut buf2 = [0u8; 2];
        r.read_exact(&mut buf2)?;
        let sub_table_count = u16::from_le_bytes(buf2) as usize;

        // Read directory entries.
        let mut directory = Vec::with_capacity(sub_table_count);
        for _ in 0..sub_table_count {
            let mut entry_buf = [0u8; 8];
            r.read_exact(&mut entry_buf)?;
            let tag = u16::from_le_bytes([entry_buf[0], entry_buf[1]]);
            // entry_buf[2..4] is reserved
            let size = u32::from_le_bytes([entry_buf[4], entry_buf[5], entry_buf[6], entry_buf[7]]);
            directory.push((tag, size));
        }

        let mut var_names = Vec::new();
        let mut func_names = Vec::new();
        let mut line_map = Vec::new();
        let mut string_layouts = Vec::new();
        let mut source_files = Vec::new();
        let mut enum_defs = Vec::new();

        // Read payloads in directory order, skipping unknown tags.
        for (tag, size) in &directory {
            match *tag {
                TAG_LINE_MAP => {
                    line_map = Self::read_line_map(r, *size)?;
                }
                TAG_VAR_NAME => {
                    var_names = Self::read_var_names(r, *size)?;
                }
                TAG_FUNC_NAME => {
                    func_names = Self::read_func_names(r, *size)?;
                }
                TAG_STRING_LAYOUT => {
                    string_layouts = Self::read_string_layouts(r, *size)?;
                }
                TAG_SOURCE_FILE => {
                    source_files = Self::read_source_files(r, *size)?;
                }
                TAG_ENUM_DEF => {
                    enum_defs = Self::read_enum_defs(r)?;
                }
                _ => {
                    // Skip unknown tags by reading and discarding their payload.
                    let mut skip_buf = vec![0u8; *size as usize];
                    r.read_exact(&mut skip_buf)?;
                }
            }
        }

        Ok(DebugSection {
            var_names,
            func_names,
            line_map,
            string_layouts,
            source_files,
            enum_defs,
        })
    }

    fn num_sub_tables(&self) -> usize {
        let mut count = 0;
        if !self.line_map.is_empty() {
            count += 1;
        }
        if !self.var_names.is_empty() {
            count += 1;
        }
        if !self.func_names.is_empty() {
            count += 1;
        }
        if !self.string_layouts.is_empty() {
            count += 1;
        }
        if !self.source_files.is_empty() {
            count += 1;
        }
        if !self.enum_defs.is_empty() {
            count += 1;
        }
        count
    }

    fn line_map_payload_size(&self) -> u32 {
        if self.line_map.is_empty() {
            return 0;
        }
        // count(2) + entries
        2 + self.line_map.len() as u32 * LINE_MAP_ENTRY_SIZE
    }

    fn write_line_map(&self, w: &mut impl Write) -> Result<(), ContainerError> {
        w.write_all(&(self.line_map.len() as u16).to_le_bytes())?;
        for entry in &self.line_map {
            w.write_all(&entry.function_id.to_le_bytes())?;
            w.write_all(&entry.bytecode_offset.to_le_bytes())?;
            w.write_all(&entry.file_id.to_le_bytes())?;
            w.write_all(&entry.source_line.to_le_bytes())?;
            w.write_all(&entry.source_column.to_le_bytes())?;
        }
        Ok(())
    }

    fn read_line_map(r: &mut impl Read, _size: u32) -> Result<Vec<LineMapEntry>, ContainerError> {
        let mut buf2 = [0u8; 2];
        r.read_exact(&mut buf2)?;
        let count = u16::from_le_bytes(buf2) as usize;

        let mut entries = Vec::with_capacity(count);
        for _ in 0..count {
            let mut entry_buf = [0u8; 10];
            r.read_exact(&mut entry_buf)?;
            entries.push(LineMapEntry {
                function_id: FunctionId::new(u16::from_le_bytes([entry_buf[0], entry_buf[1]])),
                bytecode_offset: u16::from_le_bytes([entry_buf[2], entry_buf[3]]),
                file_id: SourceFileId::new(u16::from_le_bytes([entry_buf[4], entry_buf[5]])),
                source_line: SourceLine::new(u16::from_le_bytes([entry_buf[6], entry_buf[7]])),
                source_column: SourceColumn::new(u16::from_le_bytes([entry_buf[8], entry_buf[9]])),
            });
        }
        Ok(entries)
    }

    /// Looks up the source location for a given function and bytecode offset.
    ///
    /// Returns the entry whose `bytecode_offset` is the largest value
    /// `<= bytecode_offset` for the given function. This implements the
    /// standard "find enclosing source line" lookup used by debuggers when
    /// the PC does not exactly match an entry. Returns `None` if no entry
    /// for that function precedes the requested offset.
    ///
    /// Runs in O(log N) by binary-searching the slice; relies on the
    /// `(function_id, bytecode_offset)` sort invariant documented on
    /// [`Self::line_map`].
    pub fn lookup_source_location(
        &self,
        function_id: FunctionId,
        bytecode_offset: u16,
    ) -> Option<LineMapEntry> {
        // Find the position one past the last entry that is <= the requested
        // key. The predecessor (if any) within the same function is the hit.
        let key = (function_id.raw(), bytecode_offset);
        let upper = self
            .line_map
            .partition_point(|e| (e.function_id.raw(), e.bytecode_offset) <= key);
        let candidate = self.line_map.get(upper.checked_sub(1)?)?;
        if candidate.function_id == function_id {
            Some(*candidate)
        } else {
            None
        }
    }

    fn var_name_payload_size(&self) -> u32 {
        if self.var_names.is_empty() {
            return 0;
        }
        let mut size: u32 = 2; // count
        for entry in &self.var_names {
            // var_index(2) + function_id(2) + var_section(1) + iec_type_tag(1)
            // + name_len(1) + name + type_name_len(1) + type_name
            size += 8 + entry.name.len() as u32 + entry.type_name.len() as u32;
        }
        size
    }

    fn func_name_payload_size(&self) -> u32 {
        if self.func_names.is_empty() {
            return 0;
        }
        let mut size: u32 = 2; // count
        for entry in &self.func_names {
            // function_id(2) + name_len(1) + name
            size += 3 + entry.name.len() as u32;
        }
        size
    }

    fn write_var_names(&self, w: &mut impl Write) -> Result<(), ContainerError> {
        w.write_all(&(self.var_names.len() as u16).to_le_bytes())?;
        for entry in &self.var_names {
            w.write_all(&entry.var_index.to_le_bytes())?;
            w.write_all(&entry.function_id.to_le_bytes())?;
            w.write_all(&[entry.var_section])?;
            w.write_all(&[entry.iec_type_tag])?;
            w.write_all(&[entry.name.len() as u8])?;
            w.write_all(entry.name.as_bytes())?;
            w.write_all(&[entry.type_name.len() as u8])?;
            w.write_all(entry.type_name.as_bytes())?;
        }
        Ok(())
    }

    fn write_func_names(&self, w: &mut impl Write) -> Result<(), ContainerError> {
        w.write_all(&(self.func_names.len() as u16).to_le_bytes())?;
        for entry in &self.func_names {
            w.write_all(&entry.function_id.to_le_bytes())?;
            w.write_all(&[entry.name.len() as u8])?;
            w.write_all(entry.name.as_bytes())?;
        }
        Ok(())
    }

    fn read_var_names(r: &mut impl Read, _size: u32) -> Result<Vec<VarNameEntry>, ContainerError> {
        let mut buf2 = [0u8; 2];
        r.read_exact(&mut buf2)?;
        let count = u16::from_le_bytes(buf2) as usize;

        let mut entries = Vec::with_capacity(count);
        for _ in 0..count {
            let mut hdr = [0u8; 6];
            r.read_exact(&mut hdr)?;
            let var_index = VarIndex::new(u16::from_le_bytes([hdr[0], hdr[1]]));
            let function_id = FunctionId::new(u16::from_le_bytes([hdr[2], hdr[3]]));
            let var_section_val = hdr[4];
            let iec_type_tag_val = hdr[5];

            let mut len_buf = [0u8; 1];
            r.read_exact(&mut len_buf)?;
            let name_len = len_buf[0] as usize;
            let mut name_buf = vec![0u8; name_len];
            r.read_exact(&mut name_buf)?;
            let name =
                String::from_utf8(name_buf).map_err(|_| ContainerError::InvalidDebugSection)?;

            r.read_exact(&mut len_buf)?;
            let type_name_len = len_buf[0] as usize;
            let mut type_name_buf = vec![0u8; type_name_len];
            r.read_exact(&mut type_name_buf)?;
            let type_name = String::from_utf8(type_name_buf)
                .map_err(|_| ContainerError::InvalidDebugSection)?;

            entries.push(VarNameEntry {
                var_index,
                function_id,
                var_section: var_section_val,
                iec_type_tag: iec_type_tag_val,
                name,
                type_name,
            });
        }
        Ok(entries)
    }

    fn read_func_names(
        r: &mut impl Read,
        _size: u32,
    ) -> Result<Vec<FuncNameEntry>, ContainerError> {
        let mut buf2 = [0u8; 2];
        r.read_exact(&mut buf2)?;
        let count = u16::from_le_bytes(buf2) as usize;

        let mut entries = Vec::with_capacity(count);
        for _ in 0..count {
            let mut id_buf = [0u8; 2];
            r.read_exact(&mut id_buf)?;
            let function_id = FunctionId::new(u16::from_le_bytes(id_buf));

            let mut len_buf = [0u8; 1];
            r.read_exact(&mut len_buf)?;
            let name_len = len_buf[0] as usize;
            let mut name_buf = vec![0u8; name_len];
            r.read_exact(&mut name_buf)?;
            let name =
                String::from_utf8(name_buf).map_err(|_| ContainerError::InvalidDebugSection)?;

            entries.push(FuncNameEntry { function_id, name });
        }
        Ok(entries)
    }

    fn string_layout_payload_size(&self) -> u32 {
        if self.string_layouts.is_empty() {
            return 0;
        }
        // count(2) + entries
        2 + self.string_layouts.len() as u32 * STRING_LAYOUT_ENTRY_SIZE
    }

    fn write_string_layouts(&self, w: &mut impl Write) -> Result<(), ContainerError> {
        w.write_all(&(self.string_layouts.len() as u16).to_le_bytes())?;
        for entry in &self.string_layouts {
            w.write_all(&entry.var_index.to_le_bytes())?;
            w.write_all(&entry.data_offset.to_le_bytes())?;
            w.write_all(&entry.max_length.to_le_bytes())?;
        }
        Ok(())
    }

    fn read_string_layouts(
        r: &mut impl Read,
        _size: u32,
    ) -> Result<Vec<StringLayoutEntry>, ContainerError> {
        let mut buf2 = [0u8; 2];
        r.read_exact(&mut buf2)?;
        let count = u16::from_le_bytes(buf2) as usize;

        let mut entries = Vec::with_capacity(count);
        for _ in 0..count {
            let mut entry_buf = [0u8; 8];
            r.read_exact(&mut entry_buf)?;
            entries.push(StringLayoutEntry {
                var_index: VarIndex::new(u16::from_le_bytes([entry_buf[0], entry_buf[1]])),
                data_offset: u32::from_le_bytes([
                    entry_buf[2],
                    entry_buf[3],
                    entry_buf[4],
                    entry_buf[5],
                ]),
                max_length: u16::from_le_bytes([entry_buf[6], entry_buf[7]]),
            });
        }
        Ok(entries)
    }

    fn source_file_payload_size(&self) -> u32 {
        if self.source_files.is_empty() {
            return 0;
        }
        // count(2) + entries
        let mut size: u32 = 2;
        for entry in &self.source_files {
            // path_len(2) + path bytes + content_hash(32)
            size += 2 + entry.path.len() as u32 + SOURCE_FILE_HASH_LEN as u32;
        }
        size
    }

    fn write_source_files(&self, w: &mut impl Write) -> Result<(), ContainerError> {
        w.write_all(&(self.source_files.len() as u16).to_le_bytes())?;
        for entry in &self.source_files {
            let path_bytes = entry.path.as_bytes();
            w.write_all(&(path_bytes.len() as u16).to_le_bytes())?;
            w.write_all(path_bytes)?;
            w.write_all(&entry.content_hash)?;
        }
        Ok(())
    }

    fn read_source_files(
        r: &mut impl Read,
        _size: u32,
    ) -> Result<Vec<SourceFileEntry>, ContainerError> {
        let mut buf2 = [0u8; 2];
        r.read_exact(&mut buf2)?;
        let count = u16::from_le_bytes(buf2) as usize;

        let mut entries = Vec::with_capacity(count);
        for _ in 0..count {
            r.read_exact(&mut buf2)?;
            let path_len = u16::from_le_bytes(buf2) as usize;
            let mut path_bytes = vec![0u8; path_len];
            r.read_exact(&mut path_bytes)?;
            let path =
                String::from_utf8(path_bytes).map_err(|_| ContainerError::InvalidDebugSection)?;
            let mut content_hash = [0u8; SOURCE_FILE_HASH_LEN];
            r.read_exact(&mut content_hash)?;
            entries.push(SourceFileEntry { path, content_hash });
        }
        Ok(entries)
    }

    fn enum_def_payload_size(&self) -> u32 {
        if self.enum_defs.is_empty() {
            return 0;
        }
        let mut size: u32 = 2; // count
        for entry in &self.enum_defs {
            // type_name_len(1) + type_name + value_count(2)
            size += 1 + entry.type_name.len() as u32 + 2;
            for value in &entry.values {
                // name_len(1) + name
                size += 1 + value.len() as u32;
            }
        }
        size
    }

    fn write_enum_defs(&self, w: &mut impl Write) -> Result<(), ContainerError> {
        w.write_all(&(self.enum_defs.len() as u16).to_le_bytes())?;
        for entry in &self.enum_defs {
            w.write_all(&[entry.type_name.len() as u8])?;
            w.write_all(entry.type_name.as_bytes())?;
            w.write_all(&(entry.values.len() as u16).to_le_bytes())?;
            for value in &entry.values {
                w.write_all(&[value.len() as u8])?;
                w.write_all(value.as_bytes())?;
            }
        }
        Ok(())
    }

    fn read_enum_defs(r: &mut impl Read) -> Result<Vec<EnumDefEntry>, ContainerError> {
        let mut buf2 = [0u8; 2];
        r.read_exact(&mut buf2)?;
        let count = u16::from_le_bytes(buf2) as usize;

        let mut entries = Vec::with_capacity(count);
        for _ in 0..count {
            let mut len_buf = [0u8; 1];
            r.read_exact(&mut len_buf)?;
            let type_name_len = len_buf[0] as usize;
            let mut type_name_buf = vec![0u8; type_name_len];
            r.read_exact(&mut type_name_buf)?;
            let type_name = String::from_utf8(type_name_buf)
                .map_err(|_| ContainerError::InvalidDebugSection)?;

            r.read_exact(&mut buf2)?;
            let value_count = u16::from_le_bytes(buf2) as usize;

            let mut values = Vec::with_capacity(value_count);
            for _ in 0..value_count {
                r.read_exact(&mut len_buf)?;
                let name_len = len_buf[0] as usize;
                let mut name_buf = vec![0u8; name_len];
                r.read_exact(&mut name_buf)?;
                let name =
                    String::from_utf8(name_buf).map_err(|_| ContainerError::InvalidDebugSection)?;
                values.push(name);
            }

            entries.push(EnumDefEntry { type_name, values });
        }
        Ok(entries)
    }
}

#[cfg(test)]
mod tests;
