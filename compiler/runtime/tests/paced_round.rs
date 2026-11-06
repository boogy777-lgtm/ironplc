//! The host under a paced driver: one round per call at the uptime the driver
//! reads, the cyclic deadlines kept between the calls, and the deadline
//! reported for the driver to wait for.

// Test-target boundary: the workspace denies panicking constructs in
// production code; the helper below asserts by panicking, like the tests.
#![expect(
    clippy::unwrap_used,
    reason = "test target: the fixture helper panics on a failed setup"
)]

use ironplc_container::{
    Container, ContainerBuilder, FunctionId, InstanceId, ProgramInstanceEntry, TaskEntry, TaskId,
    TaskType, VarIndex,
};
use ironplc_runtime::RuntimeHost;

/// A container with one enabled task of `task_type` and `interval_us` whose
/// program returns at once.
fn container(task_type: TaskType, interval_us: u64) -> Container {
    ContainerBuilder::new()
        .num_variables(0)
        .add_function(FunctionId::INIT, &[0x8C], 0, 0, 0)
        .add_function(FunctionId::SCAN, &[0x8C], 0, 0, 0)
        .add_task(TaskEntry {
            task_id: TaskId::DEFAULT,
            priority: 0,
            task_type,
            flags: 0x01,
            interval_us,
            single_var_index: VarIndex::NO_SINGLE_VAR,
            watchdog_us: 0,
            input_image_offset: 0,
            output_image_offset: 0,
            reserved: [0; 4],
        })
        .add_program_instance(ProgramInstanceEntry {
            instance_id: InstanceId::DEFAULT,
            task_id: TaskId::DEFAULT,
            entry_function_id: FunctionId::SCAN,
            var_table_offset: 0,
            var_table_count: 0,
            fb_instance_offset: 0,
            fb_instance_count: 0,
            init_function_id: FunctionId::INIT,
        })
        .init_function_id(FunctionId::INIT)
        .entry_function_id(FunctionId::SCAN)
        .max_call_depth(1)
        .build()
}

fn permitted_host(container: Container) -> RuntimeHost {
    let mut host = RuntimeHost::new(container).unwrap();
    host.permit_execution();
    host
}

#[test]
fn next_due_us_when_cyclic_task_then_its_deadline_after_each_paced_round() {
    let mut host = permitted_host(container(TaskType::Cyclic, 1_000));
    assert_eq!(host.next_due_us(), Some(0));

    host.run_paced_round(0, |_| {}).unwrap();
    assert_eq!(host.next_due_us(), Some(1_000));

    host.run_paced_round(1_000, |_| {}).unwrap();
    assert_eq!(host.next_due_us(), Some(2_000));
}

#[test]
fn next_due_us_when_freewheeling_task_then_none() {
    let host = permitted_host(container(TaskType::Freewheeling, 0));

    assert_eq!(host.next_due_us(), None);
}

#[test]
fn run_paced_round_when_task_not_due_then_it_does_not_run_and_keeps_its_deadline() {
    let mut host = permitted_host(container(TaskType::Cyclic, 1_000));
    host.run_paced_round(0, |_| {}).unwrap();

    host.run_paced_round(400, |_| {}).unwrap();

    assert_eq!(host.next_due_us(), Some(1_000));
    assert_eq!(host.status().rounds, 2);
}

#[test]
fn run_when_called_again_then_every_task_starts_ready_at_the_given_clock() {
    // The driven round of the session at a constant zero clock: the deadline
    // the previous call set does not stop the task from running.
    let mut host = permitted_host(container(TaskType::Cyclic, 1_000));
    host.run(1, || 0).unwrap();
    assert_eq!(host.next_due_us(), Some(1_000));

    host.run(1, || 0).unwrap();

    assert_eq!(host.next_due_us(), Some(1_000));
    assert_eq!(host.status().rounds, 2);
}

#[test]
fn run_paced_round_when_rounds_are_a_little_late_then_deadlines_stay_on_the_grid() {
    let mut host = permitted_host(container(TaskType::Cyclic, 1_000));
    let mut commits = Vec::new();

    for uptime in [0, 1_003, 2_001, 3_000] {
        host.run_paced_round(uptime, |commit| commits.push(commit.rounds))
            .unwrap();
    }

    // Late by a few microseconds each time, the deadline stays on the grid.
    assert_eq!(host.next_due_us(), Some(4_000));
    assert_eq!(commits, vec![1, 2, 3, 4]);
}

#[test]
fn run_paced_round_when_no_permit_then_not_permitted() {
    let mut host = RuntimeHost::new(container(TaskType::Cyclic, 1_000)).unwrap();

    let error = host.run_paced_round(0, |_| {}).unwrap_err();

    assert_eq!(error.v_code(), Some("V4018"));
    assert!(!host.execution_permitted());
}

#[test]
fn execution_permitted_when_granted_then_revoked_then_follows_the_latch() {
    let mut host = RuntimeHost::new(container(TaskType::Cyclic, 1_000)).unwrap();
    assert!(!host.execution_permitted());

    host.permit_execution();
    assert!(host.execution_permitted());

    host.revoke_execution_permit();
    assert!(!host.execution_permitted());
}
