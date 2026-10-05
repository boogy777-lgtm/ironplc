//! Entry point for the IronPLC MCP server.

fn main() -> Result<(), String> {
    ironplc_mcp::logging::init();
    ironplc_mcp::serve()
}
