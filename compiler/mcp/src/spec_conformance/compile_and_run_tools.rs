//! Conformance tests for the `compile` and `run` tools and the container cache they share.

use spec_test_macro::spec_test;

use crate::tools;

use crate::tools::test_support::{
    ed2_options, source, COUNTER_PROGRAM, COUNTER_PROGRAM_WITH_TASK, SYNTAX_ERROR_PROGRAM,
};

// ===========================================================================
// `compile` tool (REQ-TOL-mcp-030..036)
// ===========================================================================

/// REQ-TOL-mcp-030: The `compile` tool returns a `container_id` on success.
#[spec_test(REQ_TOL_mcp_030)]
fn mcp_spec_req_tol_030_compile_returns_container_id() {
    use std::sync::Mutex;

    use crate::cache::ContainerCache;

    let cache = Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024));
    let sources = source(COUNTER_PROGRAM);
    let options = ed2_options();

    let resp = tools::compile::build_response(&sources, &options, false, &cache);
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    assert!(resp.container_id.is_some());
    assert!(resp.container_id.unwrap().starts_with("c_"));
}

/// REQ-TOL-mcp-031: The `compile` tool returns diagnostics on failure.
#[spec_test(REQ_TOL_mcp_031)]
fn mcp_spec_req_tol_031_compile_returns_diagnostics_on_failure() {
    use std::sync::Mutex;

    use crate::cache::ContainerCache;

    let cache = Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024));
    let sources = source(SYNTAX_ERROR_PROGRAM);
    let options = ed2_options();

    let resp = tools::compile::build_response(&sources, &options, false, &cache);
    assert!(!resp.ok);
    assert!(!resp.diagnostics.is_empty());
    assert!(resp.container_id.is_none());
}

/// REQ-TOL-mcp-032: The `compile` tool returns a `tasks` array with metadata
/// for each task in the configuration.
#[spec_test(REQ_TOL_mcp_032)]
fn mcp_spec_req_tol_032_compile_returns_tasks_array() {
    use std::sync::Mutex;

    use crate::cache::{ContainerCache, TaskKind};

    let cache = Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024));
    let sources = source(COUNTER_PROGRAM_WITH_TASK);
    let options = ed2_options();

    let resp = tools::compile::build_response(&sources, &options, false, &cache);
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    assert!(!resp.tasks.is_empty());
    assert_eq!(resp.tasks[0].name, "plc_task");
    assert_eq!(resp.tasks[0].kind, TaskKind::Cyclic);
    assert_eq!(resp.tasks[0].interval_ms, Some(100.0));

    // `kind` reports the task the container carries: a program with no
    // CONFIGURATION compiles to the freewheeling task the builder synthesizes.
    let sources = source(COUNTER_PROGRAM);
    let resp = tools::compile::build_response(&sources, &options, false, &cache);
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    assert_eq!(resp.tasks[0].kind, TaskKind::Freewheeling);
    assert_eq!(resp.tasks[0].interval_ms, None);
}

/// REQ-TOL-mcp-033: The `compile` tool returns a `programs` array with metadata
/// for each program assignment in the configuration.
#[spec_test(REQ_TOL_mcp_033)]
fn mcp_spec_req_tol_033_compile_returns_programs_array() {
    use std::sync::Mutex;

    use crate::cache::ContainerCache;

    let cache = Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024));
    let sources = source(COUNTER_PROGRAM_WITH_TASK);
    let options = ed2_options();

    let resp = tools::compile::build_response(&sources, &options, false, &cache);
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    assert!(!resp.programs.is_empty());
    assert_eq!(resp.programs[0].name, "program1");
    assert_eq!(resp.programs[0].task.as_deref(), Some("plc_task"));
}

/// REQ-TOL-mcp-034: When `include_bytes` is true, the response includes
/// `container_base64` with the compiled bytecode.
#[spec_test(REQ_TOL_mcp_034)]
fn mcp_spec_req_tol_034_compile_returns_base64_when_requested() {
    use std::sync::Mutex;

    use crate::cache::ContainerCache;

    let cache = Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024));
    let sources = source(COUNTER_PROGRAM);
    let options = ed2_options();

    let resp = tools::compile::build_response(&sources, &options, true, &cache);
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    assert!(resp.container_base64.is_some());
}

/// REQ-TOL-mcp-035: The `compile` tool stores the compiled container in the
/// process-level cache, retrievable by `container_id`.
#[spec_test(REQ_TOL_mcp_035)]
fn mcp_spec_req_tol_035_compile_stores_container_in_cache() {
    use std::sync::Mutex;

    use crate::cache::ContainerCache;

    let cache = Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024));
    let sources = source(COUNTER_PROGRAM);
    let options = ed2_options();

    let resp = tools::compile::build_response(&sources, &options, false, &cache);
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    let container_id = resp.container_id.unwrap();

    let mut guard = cache.lock().unwrap();
    assert!(guard.get(&container_id).is_some());
}

/// REQ-TOL-mcp-036: A cached container is an immutable snapshot — compiling
/// a different program does not mutate previously cached containers.
#[spec_test(REQ_TOL_mcp_036)]
fn mcp_spec_req_tol_036_cached_container_is_immutable_snapshot() {
    use std::sync::Mutex;

    use crate::cache::ContainerCache;
    use crate::tools::common::SourceInput;

    let cache = Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024));
    let sources = source(COUNTER_PROGRAM);
    let options = ed2_options();

    // Compile once and snapshot the bytes
    let resp1 = tools::compile::build_response(&sources, &options, false, &cache);
    assert!(resp1.ok, "diagnostics: {:?}", resp1.diagnostics);
    let id1 = resp1.container_id.unwrap();
    let bytes_before = {
        let mut guard = cache.lock().unwrap();
        guard.get(&id1).unwrap().iplc_bytes.clone()
    };

    // Compile a different program
    let other_sources = vec![SourceInput {
        name: "other.st".into(),
        content: "PROGRAM Other\nVAR\n  y : INT;\nEND_VAR\n  y := 2;\nEND_PROGRAM".into(),
    }];
    let _resp2 = tools::compile::build_response(&other_sources, &options, false, &cache);

    // Original container bytes must be unchanged
    let bytes_after = {
        let mut guard = cache.lock().unwrap();
        guard.get(&id1).unwrap().iplc_bytes.clone()
    };
    assert_eq!(bytes_before, bytes_after);
}

// ===========================================================================
// `run` tool (REQ-TOL-mcp-040..048) — Milestone 2
// ===========================================================================

#[spec_test(REQ_TOL_mcp_040)]
#[ignore]
fn mcp_spec_req_tol_040_run_executes_container_in_vm() {}

#[spec_test(REQ_TOL_mcp_041)]
#[ignore]
fn mcp_spec_req_tol_041_run_returns_trace_array() {}

#[spec_test(REQ_TOL_mcp_042)]
#[ignore]
fn mcp_spec_req_tol_042_run_applies_stimuli() {}

#[spec_test(REQ_TOL_mcp_043)]
#[ignore]
fn mcp_spec_req_tol_043_json_value_encoding() {}

#[spec_test(REQ_TOL_mcp_044)]
#[ignore]
fn mcp_spec_req_tol_044_trace_modes() {}

#[spec_test(REQ_TOL_mcp_045)]
#[ignore]
fn mcp_spec_req_tol_045_tasks_filter() {}

#[spec_test(REQ_TOL_mcp_046)]
#[ignore]
fn mcp_spec_req_tol_046_trace_cap() {}

#[spec_test(REQ_TOL_mcp_047)]
#[ignore]
fn mcp_spec_req_tol_047_resource_limits() {}

#[spec_test(REQ_TOL_mcp_048)]
#[ignore]
fn mcp_spec_req_tol_048_run_returns_summary() {}

/// REQ-TOL-mcp-049: a freewheeling task needs a caller-supplied cycle time;
/// without one the run is rejected rather than run at an invented rate.
#[spec_test(REQ_TOL_mcp_049)]
fn mcp_spec_req_tol_049_freewheeling_task_needs_a_supplied_cycle_time() {
    use std::sync::Mutex;

    use crate::cache::{ContainerCache, TaskKind};
    use crate::tools::run::RunInput;

    let cache = Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024));
    // No CONFIGURATION, so the compiled container carries a freewheeling task.
    let sources = source(COUNTER_PROGRAM);
    let options = ed2_options();

    let compiled = tools::compile::build_response(&sources, &options, false, &cache);
    assert!(compiled.ok, "diagnostics: {:?}", compiled.diagnostics);
    assert_eq!(compiled.tasks[0].kind, TaskKind::Freewheeling);

    let run = |freewheeling_interval_ms| {
        let input = RunInput {
            container_id: compiled.container_id.clone(),
            container_base64: None,
            duration_ms: 1_000,
            freewheeling_interval_ms,
            variables: vec!["Main.Counter".into()],
            trace_outputs: false,
            stimuli: vec![],
            trace: None,
            limits: None,
            tasks: None,
        };
        tools::run::build_response(&input, &cache)
    };

    // Omitted: rejected, naming the task and what to set.
    let omitted = run(None);
    assert!(!omitted.ok);
    assert!(omitted.diagnostics.iter().any(|d| d["message"]
        .as_str()
        .unwrap_or("")
        .contains("freewheeling_interval_ms")));

    // Supplied: the caller's cycle time is what the run uses, and what it
    // reports back.
    let supplied = run(Some(200.0));
    assert!(supplied.ok, "diagnostics: {:?}", supplied.diagnostics);
    assert_eq!(
        supplied.summary.completed_cycles["Main"],
        serde_json::Value::from(5u64)
    );
    assert_eq!(supplied.summary.interval_ms, 200.0);

    // Out of range: rejected before the VM starts.
    let rejected = run(Some(0.0));
    assert!(!rejected.ok);
    assert!(rejected.diagnostics.iter().any(|d| d["message"]
        .as_str()
        .unwrap_or("")
        .contains("freewheeling_interval_ms")));
}
