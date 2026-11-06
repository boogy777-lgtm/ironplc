//! Loading a container into caller-provided buffers: [`Vm::load`] and
//! [`Vm::load_continuing`], and the helpers they share.

use ironplc_container::Container;

use super::{Vm, VmReady};
use crate::buffers::VmBuffers;
use crate::error::Trap;
#[cfg(feature = "profiling")]
use crate::profile::InstructionProfile;
use crate::scheduler::{ProgramInstanceState, TaskState};
use crate::stack::OperandStack;
use crate::variable_table::VariableTable;

impl Vm {
    /// Loads a container, using caller-provided buffers for execution state.
    ///
    /// Populates task states and program instances from `container.task_table`.
    /// Consumes the empty VM and returns a ready VM.
    ///
    /// Returns `Err(Trap)` if the container's declared call depth is invalid
    /// or exceeds the frame buffer. The check runs here, before either entry
    /// path ([`start`](VmReady::start) or [`resume`](VmReady::resume)), and
    /// before any buffer is written. The error is a bare [`Trap`] rather than
    /// a [`FaultContext`] because it belongs to the container, not to any
    /// task or program instance.
    pub fn load<'a>(
        self,
        container: &'a Container,
        bufs: &'a mut VmBuffers,
    ) -> Result<VmReady<'a>, Trap> {
        validate_call_depth(container, bufs.frames.len())?;
        populate_task_states(container, bufs);
        populate_program_instances(container, bufs);
        Ok(attach(container, bufs))
    }

    /// Loads a container into buffers that an earlier load of the same
    /// container already populated, and leaves the task states as they are.
    ///
    /// [`load`](Vm::load) starts every task ready (`next_due_us` 0, counters 0).
    /// A driver that runs one round per load and paces the rounds itself needs
    /// the deadlines and counters the previous round left, so that a cyclic task
    /// keeps its period; this is the entry for it. The buffers must come from
    /// [`VmBuffers::from_container`] for this container and have been loaded
    /// once; the program instances are repopulated, which changes nothing for
    /// the same container. The call depth is checked as in [`load`](Vm::load).
    pub fn load_continuing<'a>(
        self,
        container: &'a Container,
        bufs: &'a mut VmBuffers,
    ) -> Result<VmReady<'a>, Trap> {
        validate_call_depth(container, bufs.frames.len())?;
        populate_program_instances(container, bufs);
        Ok(attach(container, bufs))
    }
}

/// Fills the task states of `bufs` from the task table: every task starts ready.
fn populate_task_states(container: &Container, bufs: &mut VmBuffers) {
    for (i, t) in container.task_table.tasks.iter().enumerate() {
        if i < bufs.tasks.len() {
            bufs.tasks[i] = TaskState {
                task_id: t.task_id,
                priority: t.priority,
                task_type: t.task_type,
                interval_us: t.interval_us,
                watchdog_us: t.watchdog_us,
                enabled: (t.flags & 0x01) != 0,
                next_due_us: 0,
                scan_count: 0,
                last_execute_us: 0,
                max_execute_us: 0,
                overrun_count: 0,
            };
        }
    }
}

/// Fills the program instance states of `bufs` from the task table.
fn populate_program_instances(container: &Container, bufs: &mut VmBuffers) {
    for (i, p) in container.task_table.programs.iter().enumerate() {
        if i < bufs.programs.len() {
            bufs.programs[i] = ProgramInstanceState {
                instance_id: p.instance_id,
                task_id: p.task_id,
                entry_function_id: p.entry_function_id,
                var_table_offset: p.var_table_offset,
                var_table_count: p.var_table_count,
                init_function_id: p.init_function_id,
            };
        }
    }
}

/// Borrows the execution state of `bufs` for `container`.
fn attach<'a>(container: &'a Container, bufs: &'a mut VmBuffers) -> VmReady<'a> {
    let stack = OperandStack::new(&mut bufs.stack);
    let variables = VariableTable::new(&mut bufs.vars);
    let max_temp_buf_bytes = container.header.max_temp_buf_bytes as usize;
    VmReady {
        container,
        stack,
        variables,
        data_region: &mut bufs.data_region,
        temp_buf: &mut bufs.temp_buf,
        max_temp_buf_bytes,
        task_states: &mut bufs.tasks,
        program_instances: &mut bufs.programs,
        ready_buf: &mut bufs.ready,
        frames: &mut bufs.frames,
        #[cfg(feature = "profiling")]
        profile: InstructionProfile::new(),
    }
}

/// Validates a container's declared call depth against the embedder's
/// frame-buffer capacity.
///
/// Codegen populates `max_call_depth` from the static call graph and always
/// declares at least one frame (the entry function), so a value of 0 is
/// invalid: it means the field was never computed (a legacy or hand-built
/// container).
fn validate_call_depth(container: &Container, capacity: usize) -> Result<(), Trap> {
    let declared = container.header.max_call_depth;
    if declared == 0 {
        return Err(Trap::ZeroCallDepth);
    }
    if declared as usize > capacity {
        return Err(Trap::ProgramExceedsCallDepth {
            required: declared,
            capacity: capacity.min(u16::MAX as usize) as u16,
        });
    }
    Ok(())
}
