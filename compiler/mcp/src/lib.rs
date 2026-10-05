//! IronPLC MCP server library.
//!
//! Exposes IEC 61131-3 compiler capabilities as MCP tools over stdio transport.

pub mod cache;
pub mod logging;
pub mod runner;
pub mod server;
pub mod tools;

// Spec conformance testing infrastructure (test-only)
#[cfg(test)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}
#[cfg(test)]
mod spec_conformance;

// Behavioral conformance for compiler feature flags: proves each `--allow-*`
// flag gates real accept/reject behavior on identical source, and (via a
// completeness meta-test) that every flag has such a fixture. Replaces
// count-based coupling in the `list_options` tests.
#[cfg(test)]
mod feature_flag_conformance;

use ironplc_dsl::stack::within_stack_budget;
use rmcp::ServiceExt;
use server::IronPlcMcp;

/// Runs the MCP server over stdin/stdout until the client disconnects.
pub async fn run_server() -> Result<(), String> {
    let service = IronPlcMcp::new();
    let transport = rmcp::transport::io::stdio();
    let server = service
        .serve(transport)
        .await
        .map_err(|e| format!("Failed to start MCP server: {e}"))?;
    server
        .waiting()
        .await
        .map_err(|e| format!("MCP server error: {e}"))?;
    Ok(())
}

/// Runs the MCP server over stdin/stdout until the client disconnects.
///
/// This is the process entry of the server: see [`block_on_budget`].
pub fn serve() -> Result<(), String> {
    block_on_budget(run_server)?
}

/// Runs the future that `make` builds to completion on a runtime of one
/// thread, and returns what it returns.
///
/// The one thread of the runtime runs every task of the server, so the thread
/// that serves a request is this one. It is a process entry: it runs on the
/// stack budget, so a tool that reaches a stage makes no thread of its own.
pub fn block_on_budget<F: std::future::Future>(
    make: impl FnOnce() -> F + Send,
) -> Result<F::Output, String>
where
    F::Output: Send,
{
    within_stack_budget(|| {
        let runtime = tokio::runtime::Builder::new_current_thread()
            .enable_all()
            .build()
            .map_err(|e| format!("Failed to start the runtime: {e}"))?;
        Ok(runtime.block_on(make()))
    })
}
