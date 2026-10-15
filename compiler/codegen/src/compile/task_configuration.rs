//! Applies the task a configuration binds to the compiled program.

use ironplc_container::{Container, TaskType};
use ironplc_dsl::configuration::{ConfigurationDeclaration, TaskConfiguration};
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_problems::text;

/// Applies the `TASK` that the configuration binds to the compiled program.
///
/// `ContainerBuilder` synthesizes a freewheeling task and a single program
/// instance. The VM is single-instance in v1; [`find_program`] and
/// [`check_single_program_instance`] reject a second `PROGRAM` or instance
/// before this runs, so the table keeps that one task and one instance —
/// only the task entry's scheduling fields change. A program with no
/// `CONFIGURATION`, or one that no resource instantiates, or an instance
/// declared without a `WITH` clause, keeps the synthesized freewheeling task.
pub(super) fn apply_task_configuration(
    container: &mut Container,
    config: &ConfigurationDeclaration,
    program_name: &Id,
) -> Result<(), Diagnostic> {
    let Some(task) = find_bound_task(config, program_name) else {
        return Ok(());
    };

    // The VM stubs event-triggered tasks out of `collect_ready_tasks`, so
    // emitting one would produce a program whose task body never runs. Reject
    // it rather than compile something that silently does nothing.
    if task.single.is_some() {
        return Err(Diagnostic::text(
            task.name.span(),
            text::task_single_not_supported(),
        ));
    }

    let priority = u16::try_from(task.priority).map_err(|_| {
        Diagnostic::text(
            task.name.span(),
            text::task_parameter_out_of_range_priority(task.priority, u16::MAX),
        )
    })?;

    let interval_us = task_interval_us(task)?;

    if let Some(entry) = container.task_table.tasks.first_mut() {
        entry.priority = priority;
        entry.interval_us = interval_us;
        // A zero interval means "as fast as possible", which is what a
        // freewheeling task already does. A cyclic task with `interval_us` of
        // zero would instead be permanently overdue, inflating `overrun_count`
        // on every round.
        entry.task_type = if interval_us > 0 {
            TaskType::Cyclic
        } else {
            TaskType::Freewheeling
        };
    }

    Ok(())
}

/// Converts a task's `INTERVAL` to microseconds, or 0 when it declares none.
fn task_interval_us(task: &TaskConfiguration) -> Result<u64, Diagnostic> {
    let Some(interval) = &task.interval else {
        return Ok(0);
    };

    let micros = interval.interval.whole_microseconds();
    u64::try_from(micros).map_err(|_| {
        Diagnostic::text(
            interval.span.clone(),
            text::task_parameter_out_of_range_interval(micros),
        )
    })
}

/// Finds the `TASK` that `config` binds to the program type `program_name`.
///
/// `PROGRAM <instance> WITH <task> : <type>` names the program *type*, so the
/// match is on `type_name`. Returns `None` when no resource instantiates the
/// program or the instance has no `WITH` clause. A `WITH` naming a task that
/// does not exist cannot reach here — the analyzer rejects it first (see
/// `rule_program_task_definition_exists`).
fn find_bound_task<'a>(
    config: &'a ConfigurationDeclaration,
    program_name: &Id,
) -> Option<&'a TaskConfiguration> {
    config.resource_decl.iter().find_map(|resource| {
        let program = resource
            .programs
            .iter()
            .find(|program| &program.type_name == program_name)?;
        let task_name = program.task_name.as_ref()?;
        resource.tasks.iter().find(|task| &task.name == task_name)
    })
}
