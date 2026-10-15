//! Unit tests for the VM lifecycle and the bytecode interpreter.

use super::*;
use crate::test_support::{assert_trap, single_function_container, steel_thread_container};
use crate::VmBuffers;
use ironplc_container::ContainerBuilder;

#[test]
fn vm_load_when_valid_container_then_returns_ready() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let ready = Vm::new().load(&c, &mut b).unwrap();

    // If this compiles, the VM is in the Ready state.
    // Verify we can read the initial variable values.
    assert_eq!(ready.read_variable(VarIndex::new(0)).unwrap(), 0);
}

#[test]
fn vm_run_round_when_steel_thread_then_x_is_10_y_is_42() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    vm.run_round(0).unwrap();

    assert_eq!(vm.read_variable(VarIndex::new(0)).unwrap(), 10);
    assert_eq!(vm.read_variable(VarIndex::new(1)).unwrap(), 42);
}

#[test]
fn vm_run_round_when_invalid_opcode_then_trap() {
    let c = single_function_container(&[0xFF], 0, &[]);
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    assert_trap(&mut vm, Trap::InvalidInstruction(0xFF));
}

#[test]
fn vm_request_stop_when_called_then_stop_requested() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    assert!(!vm.stop_requested());
    vm.request_stop();
    assert!(vm.stop_requested());
}

#[test]
fn vm_stop_when_called_then_returns_stopped() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();
    let stopped = vm.stop();
    assert_eq!(stopped.read_variable(VarIndex::new(0)).unwrap(), 0); // not yet executed
}

#[test]
fn vm_fault_when_called_then_returns_faulted_with_context() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();
    let ctx = FaultContext {
        trap: Trap::WatchdogTimeout(ironplc_container::TaskId::new(3)),
        task_id: ironplc_container::TaskId::new(3),
        instance_id: ironplc_container::InstanceId::new(1),
    };
    let faulted = vm.fault(ctx);
    assert_eq!(
        *faulted.trap(),
        Trap::WatchdogTimeout(ironplc_container::TaskId::new(3))
    );
    assert_eq!(faulted.task_id(), ironplc_container::TaskId::new(3));
    assert_eq!(faulted.instance_id(), ironplc_container::InstanceId::new(1));
}

// Phase 1, Step 1.1: Execute error path tests
// These verify that each Trap variant that can fire inside execute()
// is triggered through the full Vm::new().load(c).unwrap().start().run_round() path.

#[test]
fn execute_when_stack_overflow_then_trap() {
    // max_stack_depth: 1, but bytecode pushes two values
    // Cannot use single_function_container because it uses max_stack=16.
    #[rustfmt::skip]
        let bytecode: Vec<u8> = vec![
            0x00, 0x00, 0x00,  // LOAD_CONST_I32 pool[0]
            0x00, 0x01, 0x00,  // LOAD_CONST_I32 pool[1]
        ];
    let c = ContainerBuilder::new()
        .num_variables(0)
        .add_i32_constant(1)
        .add_i32_constant(2)
        .add_function(FunctionId::INIT, &[0x8C], 0, 0, 0) // init: RET_VOID
        .add_function(FunctionId::SCAN, &bytecode, 1, 0, 0) // scan: triggers overflow
        .init_function_id(FunctionId::INIT)
        .entry_function_id(FunctionId::SCAN)
        .max_call_depth(1)
        .build();

    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();
    assert_trap(&mut vm, Trap::StackOverflow);
}

#[test]
fn execute_when_stack_underflow_then_trap() {
    // ADD_I32 tries to pop 2 values from an empty stack
    let c = single_function_container(&[0x20], 0, &[]);
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    assert_trap(&mut vm, Trap::StackUnderflow);
}

#[test]
fn execute_when_invalid_constant_index_then_trap() {
    // 0 constants in pool, but bytecode references pool[0]
    #[rustfmt::skip]
        let bytecode: Vec<u8> = vec![
            0x00, 0x00, 0x00,  // LOAD_CONST_I32 pool[0]
        ];
    let c = single_function_container(&bytecode, 0, &[]);
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    assert_trap(&mut vm, Trap::InvalidConstantIndex(ConstantIndex::new(0)));
}

#[test]
fn execute_when_invalid_variable_index_on_store_then_trap() {
    // 1 variable, but bytecode stores to var[5]
    #[rustfmt::skip]
        let bytecode: Vec<u8> = vec![
            0x00, 0x00, 0x00,  // LOAD_CONST_I32 pool[0]
            0x10, 0x05, 0x00,  // STORE_VAR_I32 var[5]
        ];
    let c = single_function_container(&bytecode, 1, &[42]);
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    assert_trap(&mut vm, Trap::InvalidVariableIndex(VarIndex::new(5)));
}

#[test]
fn execute_when_invalid_variable_index_on_load_then_trap() {
    // 1 variable, but bytecode loads from var[5]
    #[rustfmt::skip]
        let bytecode: Vec<u8> = vec![
            0x0C, 0x05, 0x00,  // LOAD_VAR_I32 var[5]
        ];
    let c = single_function_container(&bytecode, 1, &[]);
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    assert_trap(&mut vm, Trap::InvalidVariableIndex(VarIndex::new(5)));
}

// Phase 1, Step 1.2: Execute edge-case tests

#[test]
fn execute_when_call_user_function_then_returns_value() {
    // Layout: var[0] = result (program), var[1] = A, var[2] = B, var[3] = return slot
    // Function 0 (init): RET_VOID
    // Function 1 (scan): push 3, push 7, CALL func 2 var_offset=1, store result, RET_VOID
    // Function 2 (add):  load A, load B, ADD, store return, load return, RET
    //   num_params=2, num_locals=3 (A, B, return_slot)
    #[rustfmt::skip]
        let scan_bytecode: Vec<u8> = vec![
            0x00, 0x00, 0x00,        // LOAD_CONST_I32 pool[0] (3)
            0x00, 0x01, 0x00,        // LOAD_CONST_I32 pool[1] (7)
            0x84, 0x02, 0x00, 0x01, 0x00,  // CALL function 2, var_offset=1
            0x10, 0x00, 0x00,        // STORE_VAR_I32 var[0] (result)
            0x8C,                    // RET_VOID
        ];
    #[rustfmt::skip]
        let func_bytecode: Vec<u8> = vec![
            0x0C, 0x01, 0x00,  // LOAD_VAR_I32 var[1] (A - absolute index)
            0x0C, 0x02, 0x00,  // LOAD_VAR_I32 var[2] (B - absolute index)
            0x20,              // ADD_I32
            0x10, 0x03, 0x00,  // STORE_VAR_I32 var[3] (return slot - absolute index)
            0x0C, 0x03, 0x00,  // LOAD_VAR_I32 var[3]
            0x88,              // RET
        ];

    let c = ContainerBuilder::new()
        .num_variables(4) // 1 program var + 3 function vars
        .add_i32_constant(3)
        .add_i32_constant(7)
        .add_function(FunctionId::INIT, &[0x8C], 0, 1, 0) // init
        .add_function(FunctionId::SCAN, &scan_bytecode, 2, 1, 0) // scan
        .add_function(FunctionId::new(2), &func_bytecode, 2, 3, 2) // add (num_params=2)
        .init_function_id(FunctionId::INIT)
        .entry_function_id(FunctionId::SCAN)
        .max_call_depth(2) // SCAN -> add (2 frames)
        .build();
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();
    vm.run_round(0).unwrap();

    // result should be 3 + 7 = 10
    assert_eq!(vm.read_variable(VarIndex::new(0)).unwrap(), 10);
}

#[test]
fn execute_when_empty_bytecode_then_ok() {
    let c = single_function_container(&[], 0, &[]);
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    assert!(vm.run_round(0).is_ok());
}

#[test]
#[expect(
    clippy::default_constructed_unit_structs,
    reason = "the test calls `Default::default` on the unit struct to cover its Default impl"
)]
fn vm_default_when_called_then_loads_container() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let ready = Vm::default().load(&c, &mut b).unwrap();

    assert_eq!(ready.read_variable(VarIndex::new(0)).unwrap(), 0);
}

#[test]
fn vm_ready_read_variable_raw_when_before_init_then_returns_zero_slot() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let ready = Vm::new().load(&c, &mut b).unwrap();

    assert_eq!(ready.read_variable_raw(VarIndex::new(0)).unwrap(), 0u64);
    assert_eq!(ready.read_variable_raw(VarIndex::new(1)).unwrap(), 0u64);
}

#[test]
fn write_variable_raw_when_lword_pattern_then_round_trips_without_truncation() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    // A bit pattern that is non-zero in both halves of the slot.
    let value = 0xDEAD_BEEF_0BAD_F00D_u64;
    vm.write_variable_raw(VarIndex::new(0), value).unwrap();

    assert_eq!(vm.read_variable_raw(VarIndex::new(0)).unwrap(), value);
}

#[test]
fn write_variable_raw_when_lreal_pattern_then_reads_back_same_float() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    let value = std::f64::consts::PI;
    vm.write_variable_raw(VarIndex::new(1), value.to_bits())
        .unwrap();

    let raw = vm.read_variable_raw(VarIndex::new(1)).unwrap();
    assert_eq!(f64::from_bits(raw), value);
}

#[test]
fn write_variable_raw_when_lint_value_then_read_variable_i64_agrees() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    let value = -9_007_199_254_740_993_i64;
    vm.write_variable_raw(VarIndex::new(0), value as u64)
        .unwrap();

    assert_eq!(vm.read_variable_i64(VarIndex::new(0)).unwrap(), value);
}

#[test]
fn write_variable_when_64_bit_value_then_truncates_unlike_write_variable_raw() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    let value = 0x0000_0001_0000_002A_u64;
    vm.write_variable(VarIndex::new(0), value as i32).unwrap();
    vm.write_variable_raw(VarIndex::new(1), value).unwrap();

    assert_eq!(vm.read_variable_raw(VarIndex::new(0)).unwrap(), 0x2A);
    assert_eq!(vm.read_variable_raw(VarIndex::new(1)).unwrap(), value);
}

#[test]
fn write_variable_raw_when_index_out_of_range_then_invalid_variable_index_trap() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    let index = VarIndex::new(99);
    assert_eq!(
        vm.write_variable_raw(index, 1).unwrap_err(),
        Trap::InvalidVariableIndex(index)
    );
}

#[test]
fn vm_run_round_when_no_tasks_enabled_then_returns_ok_without_executing() {
    use ironplc_container::{ProgramInstanceEntry, TaskEntry};

    let c = ContainerBuilder::new()
        .num_variables(0)
        .add_function(FunctionId::INIT, &[0x8C], 0, 0, 0)
        .add_function(FunctionId::SCAN, &[0x8C], 0, 0, 0)
        .add_task(TaskEntry {
            task_id: TaskId::DEFAULT,
            priority: 0,
            task_type: TaskType::Freewheeling,
            flags: 0x00, // disabled
            interval_us: 0,
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
        .build();
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    assert!(vm.run_round(0).is_ok());
}

#[test]
fn uptime_when_no_round_run_then_zero() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    assert_eq!(vm.uptime(), Duration::ZERO);
}

#[test]
fn uptime_when_container_lacks_uptime_globals_then_reports_round_clock() {
    // This container has no FLAG_HAS_SYSTEM_UPTIME, so nothing is written
    // to the uptime globals -- the VM still knows the clock it ran with.
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    vm.run_round(2_500_000).unwrap();

    assert_eq!(vm.uptime(), Duration::from_millis(2500));
}

#[test]
fn uptime_when_round_advances_then_follows_the_clock() {
    let c = steel_thread_container();
    let mut b = VmBuffers::from_container(&c);
    let mut vm = Vm::new().load(&c, &mut b).unwrap().start().unwrap();

    vm.run_round(1_000).unwrap();
    assert_eq!(vm.uptime(), Duration::from_millis(1));
    vm.run_round(7_500).unwrap();
    assert_eq!(vm.uptime(), Duration::from_micros(7_500));
}
