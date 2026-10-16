//! CONFIGURATION and RESOURCE round-tripping.

use super::common::*;

#[test]
fn write_to_string_when_resource_has_global_vars_then_they_precede_tasks_and_round_trip() {
    let source = "
CONFIGURATION config
    RESOURCE resource1 ON PLC
        VAR_GLOBAL
            local : BOOL;
        END_VAR
        TASK plc_task(INTERVAL := T#100ms, PRIORITY := 1);
        PROGRAM plc_task_instance WITH plc_task : plc_prg;
    END_RESOURCE
END_CONFIGURATION
";
    let rendered = assert_round_trips(source, &CompilerOptions::default());

    let var = rendered.find("VAR_GLOBAL").expect("the VAR_GLOBAL block");
    let task = rendered.find("TASK").expect("the task");
    assert!(var < task);
}

#[test]
fn write_to_string_when_configuration_has_global_vars_then_they_are_kept_and_round_trip() {
    let source = "
CONFIGURATION config
    VAR_GLOBAL CONSTANT
        ResetCounterValue : INT := 17;
    END_VAR
    RESOURCE resource1 ON PLC
        TASK plc_task(INTERVAL := T#100ms, PRIORITY := 1);
        PROGRAM plc_task_instance WITH plc_task : plc_prg;
    END_RESOURCE
END_CONFIGURATION
";
    let rendered = assert_round_trips(source, &CompilerOptions::default());

    assert!(rendered.contains("ResetCounterValue : INT := 17;"));
}
