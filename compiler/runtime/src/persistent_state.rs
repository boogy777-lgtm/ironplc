//! The persistent state of a host's buffers.
//!
//! A container declares which part of its variable table and of its data
//! region is state that outlives a scan ([`Container::persistent_extents`],
//! ADR-0073). The buffers of a host are laid out by one container, and this
//! module is the one place that moves that part between buffers, snapshots
//! and replicated images: the swap of a body edit carries it, the redundancy
//! snapshot exports and applies it. The working slots of the table and the
//! working bytes of the data region after it are not touched: a function sets
//! its locals at every call and a function block copies its fields in and out
//! of the data region around every call.
//!
//! The walking is over [`Run`]s, not over extent rows, so that any other
//! selection of the buffers (the retained runs, `retained_state`) is moved by
//! the same code. A run that lies outside the buffers is skipped, never indexed. A container
//! that passed the load check cannot declare one: the buffers are sized from
//! the same header the check compares the rows against.

use core::ops::Range;

use ironplc_container::Container;
use ironplc_vm::{Slot, VmBuffers};

/// One run of a host's buffers: slots of the variable table and bytes of the
/// data region. Either range may be empty. The persistent extents give runs
/// with both; the retained runs of a container (ADR-0085) give one range
/// each, so the two selections share the walking below and neither has a
/// walker of its own.
#[derive(Clone, Debug, PartialEq, Eq)]
pub(crate) struct Run {
    pub(crate) vars: Range<usize>,
    pub(crate) data: Range<usize>,
}

impl Run {
    /// The number of slots in the runs, summed.
    pub(crate) fn var_count(runs: &[Run]) -> usize {
        runs.iter().map(|run| run.vars.len()).sum()
    }

    /// The number of bytes in the runs, summed.
    pub(crate) fn data_bytes(runs: &[Run]) -> usize {
        runs.iter().map(|run| run.data.len()).sum()
    }
}

/// The persistent extents of `container` as runs.
pub(crate) fn extent_runs(container: &Container) -> Vec<Run> {
    container
        .persistent_extents()
        .iter()
        .map(|row| Run {
            vars: row.vars(),
            data: row.data(),
        })
        .collect()
}

/// Copies the part of `from` that `runs` select into `to`.
pub(crate) fn carry_runs(runs: &[Run], from: &VmBuffers, to: &mut VmBuffers) {
    for run in runs {
        if let (Some(source), Some(target)) = (
            from.vars.get(run.vars.clone()),
            to.vars.get_mut(run.vars.clone()),
        ) {
            target.copy_from_slice(source);
        }
        if let (Some(source), Some(target)) = (
            from.data_region.get(run.data.clone()),
            to.data_region.get_mut(run.data.clone()),
        ) {
            target.copy_from_slice(source);
        }
    }
}

/// The slots as raw bits and the bytes of the data region of `buffers` that
/// `runs` select, each concatenated in run order.
pub(crate) fn read_runs(runs: &[Run], buffers: &VmBuffers) -> (Vec<u64>, Vec<u8>) {
    let mut vars = Vec::new();
    let mut data = Vec::new();
    for run in runs {
        if let Some(slots) = buffers.vars.get(run.vars.clone()) {
            vars.extend(slots.iter().map(|slot| slot.as_u64()));
        }
        if let Some(bytes) = buffers.data_region.get(run.data.clone()) {
            data.extend_from_slice(bytes);
        }
    }
    (vars, data)
}

/// Writes an image made by [`read_runs`] over the part of `buffers` that
/// `runs` select. Returns `false`, having written nothing, when the image does
/// not have the lengths the runs give.
pub(crate) fn write_runs(runs: &[Run], vars: &[u64], data: &[u8], buffers: &mut VmBuffers) -> bool {
    if vars.len() != Run::var_count(runs) || data.len() != Run::data_bytes(runs) {
        return false;
    }
    let (mut vars, mut data) = (vars, data);
    for run in runs {
        let (var_head, var_rest) = vars.split_at(run.vars.len());
        let (data_head, data_rest) = data.split_at(run.data.len());
        (vars, data) = (var_rest, data_rest);
        if let Some(slots) = buffers.vars.get_mut(run.vars.clone()) {
            for (slot, raw) in slots.iter_mut().zip(var_head) {
                *slot = Slot::from_u64(*raw);
            }
        }
        if let Some(bytes) = buffers.data_region.get_mut(run.data.clone()) {
            bytes.copy_from_slice(data_head);
        }
    }
    true
}

/// Copies the persistent state of `from` into `to`; both are laid out by
/// `container`, or by containers that declare the same extent.
pub(crate) fn carry(container: &Container, from: &VmBuffers, to: &mut VmBuffers) {
    carry_runs(&extent_runs(container), from, to);
}

/// The persistent slots as raw bits and the persistent bytes of the data
/// region of `buffers`, each concatenated in extent order.
pub(crate) fn read(container: &Container, buffers: &VmBuffers) -> (Vec<u64>, Vec<u8>) {
    read_runs(&extent_runs(container), buffers)
}

/// Writes an image made by [`read`] over the persistent state of `buffers`.
/// Returns `false`, having written nothing, when the image does not have the
/// lengths the extent of `container` gives.
pub(crate) fn write(
    container: &Container,
    vars: &[u64],
    data: &[u8],
    buffers: &mut VmBuffers,
) -> bool {
    write_runs(&extent_runs(container), vars, data, buffers)
}

#[cfg(test)]
mod tests {
    use ironplc_container::{ContainerBuilder, InstanceId, PersistentExtent};

    use super::*;

    /// A container of five variables and 32 data bytes whose persistent part
    /// is variables 1..3 and data bytes 8..16.
    fn container() -> Container {
        ContainerBuilder::new()
            .num_variables(5)
            .data_region_bytes(32)
            .add_persistent_extent(PersistentExtent {
                instance_id: InstanceId::DEFAULT,
                var_start: 1,
                var_count: 2,
                data_start: 8,
                data_len: 8,
            })
            .build()
    }

    fn buffers(fill: u8) -> VmBuffers {
        let mut buffers = VmBuffers::from_container(&container());
        for (index, slot) in buffers.vars.iter_mut().enumerate() {
            *slot = Slot::from_u64(u64::from(fill) * 10 + index as u64);
        }
        buffers.data_region.fill(fill);
        buffers
    }

    #[test]
    fn read_when_extent_inside_buffers_then_returns_only_the_persistent_runs() {
        let (vars, data) = read(&container(), &buffers(1));

        assert_eq!(vars, vec![11, 12]);
        assert_eq!(data, vec![1u8; 8]);
    }

    #[test]
    fn carry_when_buffers_differ_then_copies_the_persistent_runs_and_leaves_the_rest() {
        let from = buffers(1);
        let mut to = buffers(2);

        carry(&container(), &from, &mut to);

        let slots: Vec<u64> = to.vars.iter().map(|slot| slot.as_u64()).collect();
        assert_eq!(slots, vec![20, 11, 12, 23, 24]);
        assert_eq!(&to.data_region[..8], &[2u8; 8]);
        assert_eq!(&to.data_region[8..16], &[1u8; 8]);
        assert_eq!(&to.data_region[16..], &[2u8; 16]);
    }

    #[test]
    fn write_when_image_has_the_extent_lengths_then_overwrites_the_persistent_runs() {
        let mut target = buffers(2);

        let written = write(&container(), &[7, 8], &[9u8; 8], &mut target);

        assert!(written);
        let slots: Vec<u64> = target.vars.iter().map(|slot| slot.as_u64()).collect();
        assert_eq!(slots, vec![20, 7, 8, 23, 24]);
        assert_eq!(&target.data_region[8..16], &[9u8; 8]);
        assert_eq!(target.data_region[7], 2);
    }

    #[test]
    fn write_when_image_has_other_lengths_then_writes_nothing() {
        let mut target = buffers(2);

        let written = write(&container(), &[7, 8, 9], &[9u8; 8], &mut target);

        assert!(!written);
        assert_eq!(target.vars[1].as_u64(), 21);
        assert_eq!(target.data_region[8], 2);
    }
}
