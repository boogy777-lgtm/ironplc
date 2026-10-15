//! Tests for the DAP event loop, driven by framed requests through `serve`.

use super::*;
use ironplc_container::debug_section::{
    iec_type_tag, var_section, VarNameEntry, SOURCE_FILE_HASH_LEN,
};
use ironplc_container::{
    ContainerBuilder, FuncNameEntry, FunctionId, InstanceId, LineMapEntry, ProgramInstanceEntry,
    SourceColumn, SourceFileEntry, SourceFileId, SourceLine, TaskEntry, TaskId, TaskType, VarIndex,
};
use serde_json::{json, Value};
use std::io::Cursor;

mod breakpoints;
mod cycle_time;
mod scopes_and_variables;
mod session;
mod stepping;

fn a_var_name() -> VarNameEntry {
    VarNameEntry {
        var_index: VarIndex::new(0),
        function_id: FunctionId::GLOBAL_SCOPE,
        var_section: var_section::VAR,
        iec_type_tag: iec_type_tag::DINT,
        name: "x".into(),
        type_name: "DINT".into(),
    }
}

/// The source file every fixture container claims to be compiled from.
fn demo_source_file() -> SourceFileEntry {
    SourceFileEntry {
        path: "demo.st".into(),
        content_hash: [0u8; SOURCE_FILE_HASH_LEN],
    }
}

/// A line-map entry for `demo.st` mapping `offset` in `function_id` to
/// `line` (column 1).
fn line_entry(function_id: FunctionId, offset: u16, line: u16) -> LineMapEntry {
    LineMapEntry {
        function_id,
        bytecode_offset: offset,
        file_id: SourceFileId::new(0),
        source_line: SourceLine::new(line),
        source_column: SourceColumn::new(1),
    }
}

fn a_task(task_id: TaskId) -> TaskEntry {
    TaskEntry {
        task_id,
        priority: 0,
        task_type: TaskType::Freewheeling,
        flags: 0x01,
        interval_us: 0,
        single_var_index: VarIndex::NO_SINGLE_VAR,
        watchdog_us: 0,
        input_image_offset: 0,
        output_image_offset: 0,
        reserved: [0; 4],
    }
}

fn a_program(instance_id: InstanceId, task_id: TaskId) -> ProgramInstanceEntry {
    ProgramInstanceEntry {
        instance_id,
        task_id,
        entry_function_id: FunctionId::new(0),
        var_table_offset: 0,
        var_table_count: 1,
        fb_instance_offset: 0,
        fb_instance_count: 0,
        init_function_id: FunctionId::new(0),
    }
}

/// Writes a single-instance container with a debug section to a temp file
/// and returns the file (kept alive by the caller) plus its path string.
fn single_instance_debug_container_file() -> (tempfile::NamedTempFile, String) {
    let container = ContainerBuilder::new()
        .num_variables(1)
        .add_function(FunctionId::new(0), &[0x8C], 0, 1, 0)
        .max_call_depth(1)
        .add_var_name(a_var_name())
        .build();
    write_container_to_temp(&container)
}

fn no_debug_container_file() -> (tempfile::NamedTempFile, String) {
    let container = ContainerBuilder::new()
        .num_variables(1)
        .add_function(FunctionId::new(0), &[0x8C], 0, 1, 0)
        .max_call_depth(1)
        .build();
    write_container_to_temp(&container)
}

fn multi_instance_container_file() -> (tempfile::NamedTempFile, String) {
    let container = ContainerBuilder::new()
        .num_variables(1)
        .add_function(FunctionId::new(0), &[0x8C], 0, 1, 0)
        .max_call_depth(1)
        .add_var_name(a_var_name())
        .add_task(a_task(TaskId::new(0)))
        .add_task(a_task(TaskId::new(1)))
        .add_program_instance(a_program(InstanceId::new(0), TaskId::new(0)))
        .add_program_instance(a_program(InstanceId::new(1), TaskId::new(1)))
        .build();
    write_container_to_temp(&container)
}

/// Passes the launch preconditions (debug section, single instance) but the
/// init function divides by zero, so `start()` traps.
fn init_traps_container_file() -> (tempfile::NamedTempFile, String) {
    #[rustfmt::skip]
        let bytecode: Vec<u8> = vec![
            0x00, 0x00, 0x00, // LOAD_CONST_I32 pool[0] (10)
            0x00, 0x01, 0x00, // LOAD_CONST_I32 pool[1] (0)
            0x30,             // DIV_I32 -> DivideByZero
            0x8C,             // RET_VOID
        ];
    let container = ContainerBuilder::new()
        .num_variables(1)
        .add_i32_constant(10)
        .add_i32_constant(0)
        .add_function(FunctionId::new(0), &bytecode, 2, 1, 0)
        .max_call_depth(1)
        .add_var_name(a_var_name())
        .build();
    write_container_to_temp(&container)
}

fn write_container_to_temp(
    container: &ironplc_container::Container,
) -> (tempfile::NamedTempFile, String) {
    use std::io::Write as _;
    let mut buf = Vec::new();
    container.write_to(&mut buf).unwrap();
    let mut file = tempfile::NamedTempFile::new().unwrap();
    file.write_all(&buf).unwrap();
    file.flush().unwrap();
    let path = file.path().to_string_lossy().into_owned();
    (file, path)
}

fn frame(request: &Value) -> Vec<u8> {
    let mut buf = Vec::new();
    let body = serde_json::to_vec(request).unwrap();
    framing::write_message(&mut buf, &body).unwrap();
    buf
}

/// Feeds `requests` (each already a DAP request value) through `serve` and
/// returns the framed responses/events it wrote, decoded as JSON.
fn run_server(requests: &[Value]) -> Vec<Value> {
    let mut input = Vec::new();
    for req in requests {
        input.extend_from_slice(&frame(req));
    }
    let mut reader = Cursor::new(input);
    let mut writer: Vec<u8> = Vec::new();
    serve(&mut reader, &mut writer).unwrap();

    let mut out_reader = Cursor::new(writer);
    let mut messages = Vec::new();
    while let Some(body) = framing::read_message(&mut out_reader).unwrap() {
        messages.push(serde_json::from_slice(&body).unwrap());
    }
    messages
}

// -- run/stop loop ------------------------------------------------------

/// A single-instance container whose **scan** entry function is
/// [`FunctionId::SCAN`] (named `MAIN`), running `scan_bytecode`; `init`
/// is a bare `RET_VOID`. The debug section maps each `(offset, line)`
/// pair in `line_map` to `demo.st`, so tests set breakpoints by source
/// line against it.
fn scan_container_file(
    scan_bytecode: &[u8],
    max_stack: u16,
    line_map: &[(u16, u16)],
) -> (tempfile::NamedTempFile, String) {
    let mut builder = ContainerBuilder::new()
        .num_variables(1)
        .add_function(FunctionId::INIT, &[0x8C], 0, 1, 0)
        .add_function(FunctionId::SCAN, scan_bytecode, max_stack, 1, 0)
        .max_call_depth(1)
        .add_var_name(a_var_name())
        .add_func_name(FuncNameEntry {
            function_id: FunctionId::SCAN,
            name: "MAIN".into(),
        })
        .add_source_file(demo_source_file())
        .add_task(a_task(TaskId::new(0)))
        .add_program_instance(ProgramInstanceEntry {
            instance_id: InstanceId::new(0),
            task_id: TaskId::new(0),
            entry_function_id: FunctionId::SCAN,
            var_table_offset: 0,
            var_table_count: 1,
            fb_instance_offset: 0,
            fb_instance_count: 0,
            init_function_id: FunctionId::INIT,
        });
    for &(offset, line) in line_map {
        builder = builder.add_line_map_entry(line_entry(FunctionId::SCAN, offset, line));
    }
    let container = builder.build();
    write_container_to_temp(&container)
}

/// A single-instance container whose scan increments `x` (`var[0]`, a
/// DINT) by one each cycle, then `RET_VOID`. The increment statement is
/// source line 10 (offset 0) and the `RET_VOID` line 11 (offset 10), so
/// a breakpoint on line 11 pauses *after* the increment — letting a test
/// observe the variable evolve across scans.
fn incrementing_scan_container_file() -> (tempfile::NamedTempFile, String) {
    incrementing_scan_container_with_task(a_task(TaskId::new(0)))
}

/// [`incrementing_scan_container_file`] with a caller-chosen task entry, so
/// a test can pick whether the program declares a cycle time.
fn incrementing_scan_container_with_task(task: TaskEntry) -> (tempfile::NamedTempFile, String) {
    #[rustfmt::skip]
        let scan: Vec<u8> = vec![
            0x0C, 0x00, 0x00, // LOAD_VAR_I32  var[0]   (offset 0, line 10)
            0x00, 0x00, 0x00, // LOAD_CONST_I32 pool[0] (1)
            0x20,             // ADD_I32
            0x10, 0x00, 0x00, // STORE_VAR_I32 var[0]   (offset 7)
            0x8C,             // RET_VOID               (offset 10, line 11)
        ];
    let container = ContainerBuilder::new()
        .num_variables(1)
        .add_i32_constant(1)
        .add_function(FunctionId::INIT, &[0x8C], 0, 1, 0)
        .add_function(FunctionId::SCAN, &scan, 2, 1, 0)
        .max_call_depth(1)
        .add_var_name(a_var_name())
        .add_func_name(FuncNameEntry {
            function_id: FunctionId::SCAN,
            name: "MAIN".into(),
        })
        .add_source_file(demo_source_file())
        .add_line_map_entry(line_entry(FunctionId::SCAN, 0, 10))
        .add_line_map_entry(line_entry(FunctionId::SCAN, 10, 11))
        .add_task(task)
        .add_program_instance(ProgramInstanceEntry {
            instance_id: InstanceId::new(0),
            task_id: TaskId::new(0),
            entry_function_id: FunctionId::SCAN,
            var_table_offset: 0,
            var_table_count: 1,
            fb_instance_offset: 0,
            fb_instance_count: 0,
            init_function_id: FunctionId::INIT,
        })
        .build();
    write_container_to_temp(&container)
}

/// All response messages for `command`, in order.
fn responses<'a>(out: &'a [Value], command: &str) -> Vec<&'a Value> {
    out.iter()
        .filter(|m| m["type"] == "response" && m["command"] == command)
        .collect()
}

/// All event messages named `event`, in order.
fn events<'a>(out: &'a [Value], event: &str) -> Vec<&'a Value> {
    out.iter()
        .filter(|m| m["type"] == "event" && m["event"] == event)
        .collect()
}

/// Index of the first message matching `pred`, for ordering assertions.
fn index_of(out: &[Value], pred: impl Fn(&Value) -> bool) -> usize {
    out.iter().position(pred).unwrap()
}

/// The same container as [`scan_container_file`], but with a task that
/// declares an `INTERVAL` — so the session has a cycle time to work from
/// and assumes nothing.
fn cyclic_scan_container_file() -> (tempfile::NamedTempFile, String) {
    let container = ContainerBuilder::new()
        .num_variables(1)
        .add_function(FunctionId::INIT, &[0x8C], 0, 1, 0)
        .add_function(FunctionId::SCAN, &[0x8C], 0, 1, 0)
        .max_call_depth(1)
        .add_var_name(a_var_name())
        .add_func_name(FuncNameEntry {
            function_id: FunctionId::SCAN,
            name: "MAIN".into(),
        })
        .add_source_file(demo_source_file())
        .add_line_map_entry(line_entry(FunctionId::SCAN, 0, 10))
        .add_task(TaskEntry {
            task_type: TaskType::Cyclic,
            interval_us: 100_000,
            ..a_task(TaskId::new(0))
        })
        .add_program_instance(a_program(InstanceId::new(0), TaskId::new(0)))
        .build();
    write_container_to_temp(&container)
}

/// The `output` event bodies the session wrote to the debug console.
fn console_lines(out: &[Value]) -> Vec<String> {
    events(out, "output")
        .iter()
        .filter(|e| e["body"]["category"] == "console")
        .map(|e| e["body"]["output"].as_str().unwrap_or("").to_string())
        .collect()
}
