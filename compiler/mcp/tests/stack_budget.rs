//! The stack budget of the MCP server: the thread that runs the server is given
//! the budget once, when the server starts, and a tool call costs no thread
//! after that.
//!
//! The server runs its tasks on the one thread of its runtime, so the future
//! that [`block_on_budget`] runs is what the server's tools run on.

use std::future::Future;
use std::sync::Mutex;

use ironplc_dsl::stack::{is_on_budget, spawns_by_current_thread};
use ironplc_mcp::block_on_budget;
use ironplc_mcp::cache::ContainerCache;
use ironplc_mcp::tools::common::SourceInput;
use ironplc_mcp::tools::{check, compile};
use ironplc_test::fixtures::VALID_PROGRAM;

fn ed2_options() -> serde_json::Value {
    serde_json::json!({"dialect": "iec61131-3-ed2"})
}

fn sources() -> Vec<SourceInput> {
    vec![SourceInput {
        name: "main.st".into(),
        content: VALID_PROGRAM.into(),
    }]
}

/// What the call of a tool is, on the runtime of the server: whether the thread
/// has the budget, and how many threads the call made.
fn inside_the_server<R>(call: impl FnOnce() -> R + Send) -> (bool, usize)
where
    R: 'static,
{
    let observed = block_on_budget(|| async move {
        let before = spawns_by_current_thread();
        let _ = call();
        (is_on_budget(), spawns_by_current_thread() - before)
    });
    observed.expect("the runtime starts")
}

fn spawned_by_the_server<F: Future + 'static>(work: impl FnOnce() -> F + Send) -> usize
where
    F::Output: Send,
{
    let before = spawns_by_current_thread();
    let _ = block_on_budget(work).expect("the runtime starts");
    spawns_by_current_thread() - before
}

#[test]
fn block_on_budget_when_check_tool_runs_then_the_thread_has_the_budget_and_makes_none() {
    let (on_budget, spawned) =
        inside_the_server(|| check::build_response(&sources(), &ed2_options()));

    assert_eq!(spawned, 0);
    assert!(on_budget);
}

#[test]
fn block_on_budget_when_compile_tool_runs_then_the_thread_has_the_budget_and_makes_none() {
    let (on_budget, spawned) = inside_the_server(|| {
        let cache = Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024));
        compile::build_response(&sources(), &ed2_options(), false, &cache)
    });

    assert_eq!(spawned, 0);
    assert!(on_budget);
}

#[test]
fn block_on_budget_when_server_serves_calls_then_the_whole_run_makes_one_thread() {
    let spawned = spawned_by_the_server(|| async {
        for _ in 0..3 {
            let _ = check::build_response(&sources(), &ed2_options());
        }
    });

    assert_eq!(spawned, 1);
}
