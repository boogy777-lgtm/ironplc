//! The persistent extent: the part of the variable table and of the data
//! region that holds state outliving a scan.
//!
//! A container's variable table holds, in this order, the global variables,
//! the variables of the program, and then working slots: the parameters,
//! locals and return value of functions and the fields-in-flight of function
//! block bodies and methods. Only the first two parts are state. The type
//! section declares them as data, one [`PersistentExtent`] row per program
//! instance, and every reader that has to tell state from working memory (the
//! layout hash, the load check, the host's gates, the swap, the migration
//! planner, the redundancy snapshot) reads these rows. None of them computes
//! the answer again.
//!
//! The data region follows the same rule: everything code generation reserves
//! for the persistent variables (strings, arrays, structures, function block
//! instances) comes before everything it reserves afterwards (function locals,
//! function block working slots, string temporaries).

use core::ops::Range;
#[cfg(feature = "std")]
use std::vec::Vec;

use crate::id_types::InstanceId;
#[cfg(feature = "std")]
use crate::Container;

/// One program instance's persistent runs: a run of the variable table and a
/// run of the data region.
///
/// On disk this is 14 bytes: `instance_id` (u16 LE), `var_start` (u16 LE),
/// `var_count` (u16 LE), `data_start` (u32 LE), `data_len` (u32 LE).
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct PersistentExtent {
    /// The program instance that owns the row.
    pub instance_id: InstanceId,
    /// Index of the first persistent variable slot.
    pub var_start: u16,
    /// Number of persistent variable slots.
    pub var_count: u16,
    /// Offset of the first persistent byte of the data region.
    pub data_start: u32,
    /// Number of persistent bytes of the data region.
    pub data_len: u32,
}

impl PersistentExtent {
    /// Serialized size of one row in bytes.
    pub const SIZE: usize = 14;

    /// Decodes a row from its serialized bytes. Every field is a plain
    /// integer, so any 14 bytes decode; a reader that must reject a row that
    /// lies outside the tables calls the load check.
    pub fn from_bytes(buf: &[u8; Self::SIZE]) -> Self {
        PersistentExtent {
            instance_id: InstanceId::new(u16::from_le_bytes([buf[0], buf[1]])),
            var_start: u16::from_le_bytes([buf[2], buf[3]]),
            var_count: u16::from_le_bytes([buf[4], buf[5]]),
            data_start: u32::from_le_bytes([buf[6], buf[7], buf[8], buf[9]]),
            data_len: u32::from_le_bytes([buf[10], buf[11], buf[12], buf[13]]),
        }
    }

    /// Encodes the row into its serialized bytes.
    pub fn to_bytes(&self) -> [u8; Self::SIZE] {
        let mut buf = [0u8; Self::SIZE];
        buf[0..2].copy_from_slice(&self.instance_id.to_le_bytes());
        buf[2..4].copy_from_slice(&self.var_start.to_le_bytes());
        buf[4..6].copy_from_slice(&self.var_count.to_le_bytes());
        buf[6..10].copy_from_slice(&self.data_start.to_le_bytes());
        buf[10..14].copy_from_slice(&self.data_len.to_le_bytes());
        buf
    }

    /// The persistent run of the variable table.
    pub fn vars(&self) -> Range<usize> {
        let start = usize::from(self.var_start);
        start..start + usize::from(self.var_count)
    }

    /// The persistent run of the data region.
    pub fn data(&self) -> Range<usize> {
        let start = self.data_start as usize;
        start..start + self.data_len as usize
    }
}

#[cfg(feature = "std")]
impl Container {
    /// The persistent extent: the rows the type section declares, or, when it
    /// declares none, one row for the whole variable table and the whole data
    /// region of the default program instance. A container that declares no
    /// extent has no working slots.
    ///
    /// This is the one place that answers which part of the container is
    /// state. The layout hash, the load check, the host's gates, the swap, the
    /// migration planner and the redundancy snapshot read it here.
    pub fn persistent_extents(&self) -> Vec<PersistentExtent> {
        match &self.type_section {
            Some(section) if !section.persistent_extents.is_empty() => {
                section.persistent_extents.clone()
            }
            _ => std::vec![PersistentExtent {
                instance_id: InstanceId::DEFAULT,
                var_start: 0,
                var_count: self.header.num_variables,
                data_start: 0,
                data_len: self.header.data_region_bytes,
            }],
        }
    }

    /// The number of persistent variable slots, over all rows.
    pub fn persistent_var_count(&self) -> usize {
        self.persistent_extents()
            .iter()
            .map(|row| usize::from(row.var_count))
            .sum()
    }

    /// The number of persistent bytes of the data region, over all rows.
    pub fn persistent_data_bytes(&self) -> usize {
        self.persistent_extents()
            .iter()
            .map(|row| row.data_len as usize)
            .sum()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn to_bytes_when_decoded_then_roundtrips() {
        let row = PersistentExtent {
            instance_id: InstanceId::new(3),
            var_start: 5,
            var_count: 7,
            data_start: 0x0102_0304,
            data_len: 0x0A0B_0C0D,
        };

        assert_eq!(PersistentExtent::from_bytes(&row.to_bytes()), row);
    }

    #[test]
    fn ranges_when_row_has_start_and_length_then_cover_the_runs() {
        let row = PersistentExtent {
            instance_id: InstanceId::DEFAULT,
            var_start: 2,
            var_count: 3,
            data_start: 16,
            data_len: 8,
        };

        assert_eq!(row.vars(), 2..5);
        assert_eq!(row.data(), 16..24);
    }
}
