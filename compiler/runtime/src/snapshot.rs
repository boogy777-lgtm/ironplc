//! The typed state snapshot: the host's persistent regions as a
//! replicable image (HA redundancy architecture, "Minimal Seams" 3 and 4).
//!
//! [`StateSnapshot`] is exactly what [`swap_buffers`] carries over at a
//! scan boundary — the whole `vars` table and the whole `data_region` —
//! plus the layout identity tuple of `validate_candidate` (layout hash,
//! variable count, data-region size). The snapshot is the bulk-read side
//! of the crossload seam; the apply side lives on
//! [`RuntimeHost`](crate::RuntimeHost) beside `apply_migration_swap`,
//! which it mirrors with a replicated image in the migration plan's
//! place. Slots cross as raw `u64`s through the existing
//! `Slot::as_u64` / `Slot::from_u64`; there is no serialization
//! dependency here — the wire codec is the redundancy crate's codec
//! detail, next to the ping/pong frame.

use ironplc_container::Container;

/// The persistent state image of one host, exportable and applicable
/// while the host drives no scans.
///
/// `vars` and `data_region` are the two regions `swap_buffers` copies
/// byte-for-byte at a boundary; the three layout fields identify the
/// container whose layout produced them, so a receiver can fail closed
/// on a mismatched image instead of guessing (ADR-0064(c): both units
/// of the pair hold the same candidate generation).
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct StateSnapshot {
    /// The producing container's layout hash (variable table, FB
    /// descriptors, array descriptors).
    pub layout_hash: [u8; 32],
    /// The producing container's variable count; `vars.len()` must equal
    /// it, byte for byte.
    pub num_variables: u16,
    /// The producing container's data-region size; `data_region.len()`
    /// must equal it, byte for byte.
    pub data_region_bytes: u32,
    /// Every variable slot, in variable-table order, as raw slot bits.
    pub vars: Vec<u64>,
    /// The data region backing STRING/WSTRING and aggregate values.
    pub data_region: Vec<u8>,
}

impl StateSnapshot {
    /// Whether the snapshot's declared layout is the layout of
    /// `container` — the identity check an apply path fails closed on.
    pub(crate) fn layout_matches(&self, container: &Container) -> bool {
        self.layout_hash == container.header.layout_hash
            && self.num_variables == container.header.num_variables
            && self.data_region_bytes == container.header.data_region_bytes
    }

    /// Whether the payload lengths match the declared layout. A decoded
    /// snapshot that fails this is corrupt, never applicable.
    pub(crate) fn is_consistent(&self) -> bool {
        self.vars.len() == usize::from(self.num_variables)
            && self.data_region.len() == self.data_region_bytes as usize
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn snapshot(layout_seed: u8, vars: usize, data: usize) -> StateSnapshot {
        StateSnapshot {
            layout_hash: [layout_seed; 32],
            num_variables: vars as u16,
            data_region_bytes: data as u32,
            vars: vec![0; vars],
            data_region: vec![0; data],
        }
    }

    #[test]
    fn consistency_when_lengths_match_declared_layout_then_true() {
        assert!(snapshot(1, 3, 8).is_consistent());
        assert!(snapshot(1, 0, 0).is_consistent());
    }

    #[test]
    fn consistency_when_lengths_differ_then_false() {
        let mut corrupt_vars = snapshot(1, 3, 8);
        corrupt_vars.vars.push(0);
        assert!(!corrupt_vars.is_consistent());

        let mut corrupt_data = snapshot(1, 3, 8);
        corrupt_data.data_region.pop();
        assert!(!corrupt_data.is_consistent());
    }
}
