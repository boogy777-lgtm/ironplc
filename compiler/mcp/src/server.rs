//! MCP server handler for IronPLC.

use std::sync::{Arc, Mutex};

use rmcp::handler::server::router::tool::ToolRouter;
use rmcp::handler::server::wrapper::Parameters;
use rmcp::model::{ContentBlock, ServerCapabilities, ServerConfig};
use rmcp::{tool, tool_handler, tool_router, ServerHandler};

use crate::cache::ContainerCache;
use crate::tools;
use crate::tools::browse_network::BrowseNetworkInput;
use crate::tools::common::ParseCheckInput;
use crate::tools::compile::CompileInput;
use crate::tools::container_drop::ContainerDropInput;
use crate::tools::device::DeviceInput;
use crate::tools::explain_diagnostic::ExplainDiagnosticInput;
use crate::tools::hot_edit::{
    HotEditAcceptInput, HotEditDeviceInput, HotEditResponse, HotEditSession, Target,
};
use crate::tools::messages::{MessageObjectsInput, MessagesInput};
use crate::tools::pou_lineage::PouLineageInput;
use crate::tools::pou_scope::PouScopeInput;
use crate::tools::run::RunInput;
use crate::tools::symbols::SymbolsInput;

#[derive(Clone)]
pub struct IronPlcMcp {
    tool_router: ToolRouter<Self>,
    cache: Arc<Mutex<ContainerCache>>,
    hot_edit: Arc<Mutex<HotEditSession>>,
}

impl Default for IronPlcMcp {
    fn default() -> Self {
        Self {
            tool_router: Self::tool_router(),
            cache: Arc::new(Mutex::new(ContainerCache::new(
                crate::cache::DEFAULT_MAX_ENTRIES,
                crate::cache::DEFAULT_MAX_BYTES,
            ))),
            hot_edit: Arc::new(Mutex::new(HotEditSession::default())),
        }
    }
}

impl IronPlcMcp {
    pub fn new() -> Self {
        Self::default()
    }

    /// The names of the registered tools that analyze sources: those whose input
    /// has both `sources` and `options`. A test over these tools
    /// (`spec_conformance::run_diagnostics`) fails for one it has not classified,
    /// so a new tool that analyzes cannot return the diagnostics of a run
    /// unframed without a decision.
    #[cfg(test)]
    pub(crate) fn tools_taking_sources_and_options() -> Vec<String> {
        let mut names: Vec<String> = Self::tool_router()
            .list_all()
            .into_iter()
            .filter(|tool| {
                let properties = tool
                    .input_schema
                    .get("properties")
                    .and_then(|properties| properties.as_object());
                properties.is_some_and(|properties| {
                    properties.contains_key("sources") && properties.contains_key("options")
                })
            })
            .map(|tool| tool.name.to_string())
            .collect();
        names.sort();
        names
    }
}

#[tool_router]
impl IronPlcMcp {
    /// Enumerates dialects and feature flags accepted in the `options` object.
    #[tool(
        name = "list_options",
        description = "Enumerates dialects and feature flags accepted in the options object of analysis and execution tools."
    )]
    fn list_options(&self) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::list_options::build_response();
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Look up the explanation for a problem code.
    #[tool(
        name = "explain_diagnostic",
        description = "Look up the human-readable explanation for a problem code (e.g. `P0042`). Call this before editing code in response to a diagnostic you do not fully understand."
    )]
    fn explain_diagnostic(
        &self,
        Parameters(input): Parameters<ExplainDiagnosticInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::explain_diagnostic::build_response(&input.code);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Syntax check only.
    #[tool(
        name = "parse",
        description = "Syntax check only. Use while drafting to confirm the source tokenizes and parses. Do NOT use this to validate a change -- it does not catch type errors, undeclared symbols, or any other semantic rule. Call `check` for that."
    )]
    fn parse(&self, params: Parameters<ParseCheckInput>) -> Result<ContentBlock, rmcp::ErrorData> {
        let input = params.0;
        let response = tools::parse::build_response(&input.sources, &input.options);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Primary validator.
    #[tool(
        name = "check",
        description = "Primary validator. Runs parse and full semantic analysis and returns structured diagnostics. ALWAYS run this before reporting success to the user and before calling `compile` or `run`. Self-heal by reading the returned diagnostics, fixing the code, and calling `check` again. Call `explain_diagnostic` to understand any unfamiliar problem code BEFORE editing the source."
    )]
    fn check(&self, params: Parameters<ParseCheckInput>) -> Result<ContentBlock, rmcp::ErrorData> {
        let input = params.0;
        let response = tools::check::build_response(&input.sources, &input.options);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// The message objects of a check.
    #[tool(
        name = "get_message_objects",
        description = "The messages of a check as message objects, as `system.get_message_objects` of the CODESYS scripting API: `severity` (FatalError, Error, Warning, Information, Text, SuppressedWarning or SuppressedInformation), `severity_value`, `prefix`, `number`, `text`, `position`, `length`, `position_text`, `object` and `codesys`. `severities` is a mask of `severity_value`s (default: every category, suppressed ones included). `area` is `Build` (default) or `Precompile`, the language server's list while editing; see `get_message_areas`. The last message of the Build area is the line that ends the check, a Text with a null `prefix` and `number`; the Precompile area has none."
    )]
    fn get_message_objects(
        &self,
        Parameters(input): Parameters<MessageObjectsInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::messages::build_objects_response(&input);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// The texts of the messages of a check.
    #[tool(
        name = "get_messages",
        description = "The messages of a check as texts alone, as `system.get_messages` of the CODESYS scripting API. Use `get_message_objects` to see the category, the code and the position of each. `area` is `Build` (default) or `Precompile`; see `get_message_areas`. The last text of the Build area is the line that ends the check."
    )]
    fn get_messages(
        &self,
        Parameters(input): Parameters<MessagesInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::messages::build_texts_response(&input);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// The areas of messages.
    #[tool(
        name = "get_message_areas",
        description = "The areas of messages a query can read, each with its `name`, the `source` an editor shows for it and a one-line `description`, as `system.get_message_categories` and `system.get_message_category_description` of the CODESYS scripting API. Give a `name` as the `area` of `get_message_objects` or `get_messages`."
    )]
    fn get_message_areas(&self) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::message_areas::build_response();
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Full symbol table extraction.
    #[tool(
        name = "symbols",
        description = "Full symbol table for a set of sources. Large responses are capped \u{2014} prefer the `pou` filter or one of the context tools (`pou_scope`, `project_io`, `types_all`) when you only need part of the answer."
    )]
    fn symbols(
        &self,
        Parameters(input): Parameters<SymbolsInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response =
            tools::symbols::build_response(&input.sources, &input.options, input.pou.as_deref());
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Project manifest: files, POU names, and UDTs grouped by kind.
    #[tool(
        name = "project_manifest",
        description = "Flat summary of what is declared across the supplied sources: file names, Program/Function/Function-Block names, and user-defined types grouped by kind (enumerations, structures, arrays, subranges, aliases, strings, references). Use this to orient yourself in an unfamiliar project before calling `symbols` or `pou_scope`."
    )]
    fn project_manifest(
        &self,
        Parameters(input): Parameters<ParseCheckInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::project_manifest::build_response(&input.sources, &input.options);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Project I/O: inputs the caller can drive, outputs it can observe.
    #[tool(
        name = "project_io",
        description = "Inputs the caller can drive and outputs the caller can observe, for planning a `run` call. This is the right tool to call before constructing `stimuli` or deciding which variables to `trace`."
    )]
    fn project_io(
        &self,
        Parameters(input): Parameters<ParseCheckInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::project_io::build_response(&input.sources, &input.options);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Variables visible to a single POU.
    #[tool(
        name = "pou_scope",
        description = "Every variable visible to a single POU. Prefer this over `symbols` when editing one POU."
    )]
    fn pou_scope(
        &self,
        Parameters(input): Parameters<PouScopeInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::pou_scope::build_response(&input.sources, &input.options, &input.pou);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Upstream and downstream POU dependencies.
    #[tool(
        name = "pou_lineage",
        description = "Upstream and downstream POU dependencies, including standard library POUs, each tagged with whether it comes from the supplied sources or the standard library. Use this to decide which other POUs to pull into context before editing one."
    )]
    fn pou_lineage(
        &self,
        Parameters(input): Parameters<PouLineageInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response =
            tools::pou_lineage::build_response(&input.sources, &input.options, &input.pou);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Every user-defined type with kind-specific detail fields.
    #[tool(
        name = "types_all",
        description = "Every user-defined type with enough detail to reference a field or enum value without re-reading the source."
    )]
    fn types_all(
        &self,
        Parameters(input): Parameters<ParseCheckInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::types_all::build_response(&input.sources, &input.options);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Full pipeline: parse, semantic analysis, and codegen.
    #[tool(
        name = "compile",
        description = "Only call this when you need a compiled artifact to `run`. For validation, call `check` instead \u{2014} `check` is faster, produces the same diagnostics, and does not incur codegen cost. A failing `compile` does not give you any information that a failing `check` would not."
    )]
    async fn compile(
        &self,
        Parameters(input): Parameters<CompileInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::compile::build_response(
            &input.sources,
            &input.options,
            input.include_bytes,
            &self.cache,
        );
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Explicitly releases a compiled container from the cache.
    #[tool(
        name = "container_drop",
        description = "Explicitly releases a compiled container from the cache. Not usually necessary \u{2014} the cache evicts on LRU pressure \u{2014} but available for long-running connections."
    )]
    async fn container_drop(
        &self,
        Parameters(input): Parameters<ContainerDropInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::container_drop::build_response(&input.container_id, &self.cache);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Executes a compiled container in the IronPLC VM.
    #[tool(
        name = "run",
        description = "Executes a compiled container in the IronPLC VM for a bounded simulated duration and returns a trace of observed variables. Call `check` until it returns `ok:true`, then `compile` to obtain a `container_id`, then `run`. In this milestone the tool supports `every_cycle` trace mode without stimuli; supply `variables` (fully-qualified names) or `trace_outputs:true` to capture outputs."
    )]
    async fn run(
        &self,
        Parameters(input): Parameters<RunInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = tools::run::build_response(&input, &self.cache);
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Finds the controllers this workstation can see, through IronLink.
    #[tool(
        name = "browse_network",
        description = "Finds the IronPLC controllers this workstation can see, by asking IronLink (`ironlink`, which ships with IronPLC) to browse: a broadcast, the address list stored on this workstation and any `addresses` you give (`IP` or `IP:PORT`). Returns the browse result as IronLink writes it: the workstation and, for each driver, its devices by device id with the routes by which each answered. An observation is what a device said when it was seen and does not promise that it is reachable now. `timeout_ms` is how long to wait for replies (default 1000, at most 60000) and `attempts` how many times to send the request (default 2, at most 10). Fails with a P8002 diagnostic when `ironlink` is not found, and with the `l_code` of IronLink when it refuses the request."
    )]
    async fn browse_network(
        &self,
        Parameters(input): Parameters<BrowseNetworkInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        // The browse waits for replies for as long as the caller asked, so it
        // does not hold the thread that serves every request.
        let response = tokio::task::spawn_blocking(move || {
            tools::browse_network::build_response(&input, crate::ironlink::IronLink::locate_here())
        })
        .await
        .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        let json = serde_json::to_string(&response)
            .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
        Ok(ContentBlock::text(json))
    }

    /// Reports the hot-edit state of the running application.
    #[tool(
        name = "hot_edit_status",
        description = "Reports the hot-edit state of the running application: the active mode (normal or testing), the logic and application generation counters, whether a candidate edit is staged, and completed scan rounds. Fails with a diagnostic until `hot_edit_accept` has started a session. Pass `device` (`{address, port}` for a TCP connection, or `{device_id}` for a device that IronLink has a route to, see `browse_network`) to act on a device over the session protocol instead of the server's own session; the response then names the device that answered in `device`."
    )]
    async fn hot_edit_status(
        &self,
        Parameters(input): Parameters<HotEditDeviceInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = self
            .run_hot_edit(input.device, |target| {
                tools::hot_edit::build_status_response(target)
            })
            .await?;
        hot_edit_json(&response)
    }

    /// Compiles sources and feeds them to the hot-edit protocol.
    #[tool(
        name = "hot_edit_accept",
        description = "Compiles the supplied IEC sources and hands them to the hot-edit protocol. The first call establishes the running application session and reports `established`; each later call stages the compiled program as an edit candidate to activate with `hot_edit_test` and promote with `hot_edit_assemble`, or discard with `hot_edit_cancel`. With `device` the candidate is staged on that device, which already runs a program, and the result is `ack`. Compilation failures return the usual diagnostics; staging refusals return a stable V-code such as V4007 (layout change) or V4013 (candidate already staged). A V4010 refusal carries a `pairs` list of out-of-policy type changes; resubmit the identical sources with the `migration` map (`uid -> \"init\" | \"preserve\"`) to resolve them. Pass `device` (`{address, port}` for a TCP connection, or `{device_id}` for a device that IronLink has a route to, see `browse_network`) to act on a device over the session protocol instead of the server's own session; the response then names the device that answered in `device`."
    )]
    async fn hot_edit_accept(
        &self,
        Parameters(input): Parameters<HotEditAcceptInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        // Compiling stays on the thread that serves the request (the stack
        // budget); only the exchange with a device leaves it.
        let response =
            match tools::hot_edit::prepare_candidate(&input.sources, &input.options, &self.cache) {
                Err(response) => response,
                Ok(candidate) => {
                    let migration = input.migration;
                    self.run_hot_edit(input.device, move |target| {
                        tools::hot_edit::send_candidate(
                            candidate.program,
                            candidate.ids_out_of_sync,
                            &migration,
                            target,
                        )
                    })
                    .await?
                }
            };
        hot_edit_json(&response)
    }

    /// Activates the staged candidate.
    #[tool(
        name = "hot_edit_test",
        description = "Activates the staged candidate at the next scan boundary and drives one scan round, so the candidate is executing when the call returns. Refusals carry a stable V-code, e.g. V4012 when no candidate is staged. Pass `device` (`{address, port}` for a TCP connection, or `{device_id}` for a device that IronLink has a route to, see `browse_network`) to act on a device over the session protocol instead of the server's own session; the response then names the device that answered in `device`."
    )]
    async fn hot_edit_test(
        &self,
        Parameters(input): Parameters<HotEditDeviceInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = self
            .run_hot_edit(input.device, |target| {
                tools::hot_edit::build_test_response(target)
            })
            .await?;
        hot_edit_json(&response)
    }

    /// Reverts to the original artifact.
    #[tool(
        name = "hot_edit_untest",
        description = "Switches back to the original artifact at the next scan boundary and drives one scan round, so the revert is executing when the call returns; process state is preserved. Refused with V4011 after a schema-changing test \u{2014} promote with `hot_edit_assemble` or discard with `hot_edit_cancel` instead. Pass `device` (`{address, port}` for a TCP connection, or `{device_id}` for a device that IronLink has a route to, see `browse_network`) to act on a device over the session protocol instead of the server's own session; the response then names the device that answered in `device`."
    )]
    async fn hot_edit_untest(
        &self,
        Parameters(input): Parameters<HotEditDeviceInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = self
            .run_hot_edit(input.device, |target| {
                tools::hot_edit::build_untest_response(target)
            })
            .await?;
        hot_edit_json(&response)
    }

    /// Promotes the staged candidate.
    #[tool(
        name = "hot_edit_assemble",
        description = "Promotes the staged candidate to the running application and drives one scan round, so the promoted program is executing when the call returns. Works from both the staged and the testing phases. Refusals carry a stable V-code, e.g. V4012 when no candidate is staged. Pass `device` (`{address, port}` for a TCP connection, or `{device_id}` for a device that IronLink has a route to, see `browse_network`) to act on a device over the session protocol instead of the server's own session; the response then names the device that answered in `device`."
    )]
    async fn hot_edit_assemble(
        &self,
        Parameters(input): Parameters<HotEditDeviceInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = self
            .run_hot_edit(input.device, |target| {
                tools::hot_edit::build_assemble_response(target)
            })
            .await?;
        hot_edit_json(&response)
    }

    /// Discards the staged candidate.
    #[tool(
        name = "hot_edit_cancel",
        description = "Discards the staged candidate while the original artifact keeps running. Takes effect immediately. Refused with V4015 while a candidate test is in progress. Pass `device` (`{address, port}` for a TCP connection, or `{device_id}` for a device that IronLink has a route to, see `browse_network`) to act on a device over the session protocol instead of the server's own session; the response then names the device that answered in `device`."
    )]
    async fn hot_edit_cancel(
        &self,
        Parameters(input): Parameters<HotEditDeviceInput>,
    ) -> Result<ContentBlock, rmcp::ErrorData> {
        let response = self
            .run_hot_edit(input.device, |target| {
                tools::hot_edit::build_cancel_response(target)
            })
            .await?;
        hot_edit_json(&response)
    }
}

impl IronPlcMcp {
    /// Carries out a hot-edit command on the server's own session, or on the
    /// device the call names. A device is resolved (IronLink is asked for the
    /// route of an id) and talked to on a thread that may block, so the thread
    /// that serves every request does not wait on the network.
    async fn run_hot_edit<F>(
        &self,
        device: Option<DeviceInput>,
        run: F,
    ) -> Result<HotEditResponse, rmcp::ErrorData>
    where
        F: for<'a> FnOnce(Target<'a>) -> HotEditResponse + Send + 'static,
    {
        let Some(device) = device else {
            return Ok(run(Target::from(&*self.hot_edit)));
        };
        tokio::task::spawn_blocking(move || {
            match tools::device::resolve(&device, crate::ironlink::IronLink::locate_here) {
                Ok(resolved) => run(Target::from(&resolved)),
                Err(fault) => tools::hot_edit::unresolved(fault),
            }
        })
        .await
        .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))
    }
}

/// The text content of a `hot_edit_*` response.
fn hot_edit_json(response: &HotEditResponse) -> Result<ContentBlock, rmcp::ErrorData> {
    let json = serde_json::to_string(response)
        .map_err(|e| rmcp::ErrorData::internal_error(e.to_string(), None))?;
    Ok(ContentBlock::text(json))
}

#[tool_handler(router = self.tool_router)]
impl ServerHandler for IronPlcMcp {
    fn get_info(&self) -> ServerConfig {
        ServerConfig::new(ServerCapabilities::builder().enable_tools().build())
            .with_instructions("IronPLC MCP server \u{2014} IEC 61131-3 compiler tools.")
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn server_when_get_info_then_returns_server_name() {
        let server = IronPlcMcp::new();
        let info = server.get_info();
        assert!(info.instructions.is_some());
    }

    /// Some MCP clients (e.g. OpenCode) reject a boolean JSON schema used as a
    /// `properties` or `items` value and fail to load the entire tool list. This
    /// guards against schemars regressing a `serde_json::Value` field to its
    /// default rendering — the boolean schema `true` — which previously broke
    /// OpenCode connectivity (see integrations/opencode).
    #[test]
    fn server_when_listing_tools_then_no_schema_is_boolean() {
        let router = IronPlcMcp::tool_router();
        for tool in router.list_all() {
            let schema = serde_json::Value::Object((*tool.input_schema).clone());
            let offenders = find_boolean_schemas(&schema, &tool.name);
            assert!(
                offenders.is_empty(),
                "tool `{}` advertises boolean schema(s) that MCP clients reject: {:?}",
                tool.name,
                offenders
            );
        }
    }

    /// Returns the paths of any boolean schemas found in `properties`, `items`,
    /// or `$defs` positions, recursively.
    fn find_boolean_schemas(node: &serde_json::Value, path: &str) -> Vec<String> {
        let mut out = Vec::new();
        let Some(obj) = node.as_object() else {
            return out;
        };
        if let Some(props) = obj.get("properties").and_then(|v| v.as_object()) {
            for (key, value) in props {
                let child = format!("{path}.properties.{key}");
                if value.is_boolean() {
                    out.push(child);
                } else {
                    out.extend(find_boolean_schemas(value, &child));
                }
            }
        }
        if let Some(items) = obj.get("items") {
            let child = format!("{path}.items");
            if items.is_boolean() {
                out.push(child);
            } else {
                out.extend(find_boolean_schemas(items, &child));
            }
        }
        if let Some(defs) = obj.get("$defs").and_then(|v| v.as_object()) {
            for (key, value) in defs {
                out.extend(find_boolean_schemas(value, &format!("{path}.$defs.{key}")));
            }
        }
        out
    }
}
