//! The typed state snapshot: the host's persistent regions as a
//! replicable image (HA redundancy architecture, "Minimal Seams" 3 and 4).
//!
//! [`StateSnapshot`] is exactly what [`swap_buffers`] carries over at a
//! scan boundary — the persistent extent of the container's `vars` table and
//! of its `data_region` (ADR-0073) — plus the layout identity of
//! `validate_candidate` (layout hash and the size of the extent). The working
//! slots and working bytes are not state and are not part of it, so two hosts
//! that run different bodies of one layout exchange images of one size. The
//! snapshot is the bulk-read side of the crossload seam; the apply side lives on
//! [`RuntimeHost`](crate::RuntimeHost) beside `apply_migration_swap`,
//! which it mirrors with a replicated image in the migration plan's
//! place. Slots cross as raw `u64`s through the existing
//! `Slot::as_u64` / `Slot::from_u64`; there is no serialization
//! dependency here — the wire codec is the redundancy crate's codec
//! detail, next to the ping/pong frame.

use ironplc_container::Container;
use ironplc_vm::VmBuffers;

/// The persistent state image of one host, exportable and applicable
/// while the host drives no scans.
///
/// `vars` and `data_region` are the persistent runs of the two regions
/// `swap_buffers` copies byte-for-byte at a boundary, each concatenated in
/// extent order; the three layout fields identify the container whose layout
/// produced them, so a receiver can fail closed on a mismatched image instead
/// of guessing (ADR-0064(c): both units of the pair hold the same candidate
/// generation).
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct StateSnapshot {
    /// The producing container's layout hash (the persistent extent, its
    /// variables, the arrays they name, the function block field layouts).
    pub layout_hash: [u8; 32],
    /// The producing container's count of persistent variable slots;
    /// `vars.len()` must equal it.
    pub persistent_vars: u16,
    /// The producing container's count of persistent data-region bytes;
    /// `data_region.len()` must equal it.
    pub persistent_data_bytes: u32,
    /// Every persistent variable slot, in variable-table order, as raw slot
    /// bits.
    pub vars: Vec<u64>,
    /// The persistent bytes of the data region backing STRING/WSTRING and
    /// aggregate values.
    pub data_region: Vec<u8>,
}

impl StateSnapshot {
    /// The snapshot of the persistent state in `buffers`, which `container`
    /// laid out.
    pub(crate) fn of(container: &Container, buffers: &VmBuffers) -> Self {
        let (vars, data_region) = crate::persistent_state::read(container, buffers);
        StateSnapshot {
            layout_hash: container.header.layout_hash,
            persistent_vars: u16::try_from(vars.len()).unwrap_or(u16::MAX),
            persistent_data_bytes: u32::try_from(data_region.len()).unwrap_or(u32::MAX),
            vars,
            data_region,
        }
    }

    /// Whether the snapshot's declared layout is the layout of
    /// `container` — the identity check an apply path fails closed on.
    pub(crate) fn layout_matches(&self, container: &Container) -> bool {
        self.layout_hash == container.header.layout_hash
            && usize::from(self.persistent_vars) == container.persistent_var_count()
            && self.persistent_data_bytes as usize == container.persistent_data_bytes()
    }

    /// Writes the image over the persistent state of `buffers`, which
    /// `container` laid out; the caller has proven [`layout_matches`](Self::layout_matches).
    /// Returns `false` and writes nothing when the payload does not have the
    /// lengths the container extent gives.
    pub(crate) fn write_to(&self, container: &Container, buffers: &mut VmBuffers) -> bool {
        crate::persistent_state::write(container, &self.vars, &self.data_region, buffers)
    }

    /// Whether the payload lengths match the declared layout. A decoded
    /// snapshot that fails this is corrupt, never applicable.
    pub(crate) fn is_consistent(&self) -> bool {
        self.vars.len() == usize::from(self.persistent_vars)
            && self.data_region.len() == self.persistent_data_bytes as usize
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn snapshot(layout_seed: u8, vars: usize, data: usize) -> StateSnapshot {
        StateSnapshot {
            layout_hash: [layout_seed; 32],
            persistent_vars: vars as u16,
            persistent_data_bytes: data as u32,
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
