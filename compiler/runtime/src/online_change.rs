//! Candidate validation and the buffer swap primitive.
//!
//! # Scheduler state at a swap
//!
//! `Vm::load` re-arms every scheduler field (`tasks`, `programs`) from the
//! container it is given, and the swap rebuilds the buffers from the
//! destination container. Task states are therefore re-armed from the
//! destination task table at every swap, exactly as at a cold load. That is
//! acceptable for P0 -- the validation already rejects a schedule change, so
//! only per-task execution history (scan counts, watchdog maxima, overrun
//! counts) is lost, never schedule identity -- and keeps the scheduler free
//! of a second state-migration path. The IEC variable state and the data
//! region are carried over, which is what the acceptance criteria require.

use ironplc_container::{Container, ProgramInstanceEntry, TaskEntry, TaskTable};
use ironplc_vm::{Vm, VmBuffers};

use crate::error::OnlineChangeError;

/// Validates a candidate against the active container for online change.
///
/// The candidate may change code and grow the data region, but it must keep
/// the active application's state layout: the layout hash (variable table,
/// FB descriptors, array descriptors), the variable count, the header flags,
/// the process-image sizes, and the task table.
pub(crate) fn validate_candidate(
    active: &Container,
    candidate: &Container,
) -> Result<(), OnlineChangeError> {
    if active.header.layout_hash != candidate.header.layout_hash
        || active.header.num_variables != candidate.header.num_variables
        || active.header.flags != candidate.header.flags
    {
        return Err(OnlineChangeError::LayoutIncompatible);
    }

    if active.header.input_image_bytes != candidate.header.input_image_bytes
        || active.header.output_image_bytes != candidate.header.output_image_bytes
        || active.header.memory_image_bytes != candidate.header.memory_image_bytes
    {
        return Err(OnlineChangeError::IoIncompatible);
    }

    if !task_table_matches(&active.task_table, &candidate.task_table) {
        return Err(OnlineChangeError::ScheduleIncompatible);
    }

    Ok(())
}

/// Compares two task tables field by field.
///
/// `TaskTable`, `TaskEntry` and `ProgramInstanceEntry` do not implement
/// `PartialEq`, and the comparison must cover every field so that a
/// schedule change can never slip through as an equal hash: the layout hash
/// deliberately excludes the task table, so this is the only schedule check.
pub(crate) fn task_table_matches(a: &TaskTable, b: &TaskTable) -> bool {
    a.shared_globals_size == b.shared_globals_size
        && a.tasks.len() == b.tasks.len()
        && a.tasks
            .iter()
            .zip(b.tasks.iter())
            .all(|(left, right)| task_entry_matches(left, right))
        && a.programs.len() == b.programs.len()
        && a.programs
            .iter()
            .zip(b.programs.iter())
            .all(|(left, right)| program_entry_matches(left, right))
}

fn task_entry_matches(a: &TaskEntry, b: &TaskEntry) -> bool {
    a.task_id == b.task_id
        && a.priority == b.priority
        && a.task_type == b.task_type
        && a.flags == b.flags
        && a.interval_us == b.interval_us
        && a.single_var_index == b.single_var_index
        && a.watchdog_us == b.watchdog_us
        && a.input_image_offset == b.input_image_offset
        && a.output_image_offset == b.output_image_offset
        && a.reserved == b.reserved
}

fn program_entry_matches(a: &ProgramInstanceEntry, b: &ProgramInstanceEntry) -> bool {
    a.instance_id == b.instance_id
        && a.task_id == b.task_id
        && a.entry_function_id == b.entry_function_id
        && a.var_table_offset == b.var_table_offset
        && a.var_table_count == b.var_table_count
        && a.fb_instance_offset == b.fb_instance_offset
        && a.fb_instance_count == b.fb_instance_count
        && a.init_function_id == b.init_function_id
}

/// Rebuilds `buffers` for `next` and carries the persistent state over.
///
/// Sized buffers (`vars`, `data_region`) come from `next`, so a candidate
/// that grows the data region is accommodated here, outside the scan. The
/// persistent prefix of `vars` and `data_region` is copied byte-for-byte
/// into the rebuilt buffers; transient buffers (stack, temp buffer, frames,
/// ready list) are left at their fresh values because nothing in them
/// outlives a scan boundary. `Vm::load` then re-arms the scheduler state
/// from `next` (see the module comment).
pub(crate) fn swap_buffers(next: &Container, buffers: &mut VmBuffers, rounds: u64) {
    let mut rebuilt = VmBuffers::from_container(next);

    let var_count = rebuilt.vars.len().min(buffers.vars.len());
    rebuilt.vars[..var_count].copy_from_slice(&buffers.vars[..var_count]);

    let data_len = rebuilt.data_region.len().min(buffers.data_region.len());
    rebuilt.data_region[..data_len].copy_from_slice(&buffers.data_region[..data_len]);

    *buffers = rebuilt;

    // Populate task and program state from the destination and establish the
    // continuing scan count. `resume` never runs init functions: the code
    // reverts or advances, the process state does not.
    let ready = Vm::new().load(next, buffers);
    let _running = ready.resume(rounds);
}

#[cfg(test)]
mod tests {
    use super::*;
    use ironplc_container::{
        FunctionId, InstanceId, TaskEntry, TaskId, TaskTable, TaskType, VarIndex,
    };

    fn task_table() -> TaskTable {
        TaskTable {
            shared_globals_size: 2,
            tasks: vec![TaskEntry {
                task_id: TaskId::DEFAULT,
                priority: 1,
                task_type: TaskType::Freewheeling,
                flags: 0x01,
                interval_us: 0,
                single_var_index: VarIndex::NO_SINGLE_VAR,
                watchdog_us: 0,
                input_image_offset: 0,
                output_image_offset: 0,
                reserved: [0; 4],
            }],
            programs: vec![ProgramInstanceEntry {
                instance_id: InstanceId::DEFAULT,
                task_id: TaskId::DEFAULT,
                entry_function_id: FunctionId::INIT,
                var_table_offset: 0,
                var_table_count: 2,
                fb_instance_offset: 0,
                fb_instance_count: 0,
                init_function_id: FunctionId::INIT,
            }],
        }
    }

    #[test]
    fn task_table_matches_when_identical_then_true() {
        assert!(task_table_matches(&task_table(), &task_table()));
    }

    #[test]
    fn task_table_matches_when_task_field_differs_then_false() {
        let mut changed = task_table();
        changed.tasks[0].watchdog_us = 50_000;

        assert!(!task_table_matches(&task_table(), &changed));
    }

    #[test]
    fn task_table_matches_when_program_field_differs_then_false() {
        let mut changed = task_table();
        changed.programs[0].var_table_count = 3;

        assert!(!task_table_matches(&task_table(), &changed));
    }

    #[test]
    fn task_table_matches_when_length_differs_then_false() {
        let mut changed = task_table();
        changed.tasks.clear();

        assert!(!task_table_matches(&task_table(), &changed));
    }
}
