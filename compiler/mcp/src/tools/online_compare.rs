//! The `online_compare` MCP tool: what the controller is, what it holds, what
//! the project has, and which actions follow (ADR-0089).
//!
//! The tool connects to the device the way the device tools do, reads its
//! `identity`, and calls `ironplc_project::online_comparison::compare`, the one
//! function every client uses. It decides nothing itself.
//!
//! The server reads no file of the project (REQ-STL-mcp-006), so the records
//! that the editor and the command line read are not available: there is no
//! download record, no path record and no build record. The project is the
//! `sources` of the call, when it has any: they are compiled, and the container
//! that comes out is the build (P), up to date by construction. A call without
//! `sources` compares the controller with no project. The comparison then has no
//! "previous download" and no "other controller" row, and a controller that
//! holds something other than the compiled sources is `otherApplication`.

use std::sync::Mutex;

use ironplc_problems::text;
use ironplc_project::build_record::ContainerIdentity;
use ironplc_project::online_comparison::{
    compare, Attempt, BuiltApplication, Comparison, DeviceFacts, ProjectState, Records,
};
use ironplc_session_client::{Session, SessionError};
use schemars::JsonSchema;
use serde::{Deserialize, Serialize};

use super::common::{
    ironlink_diagnostics, project_name, serialize_diagnostics, validation_diagnostic, SourceInput,
};
use super::device::{DeviceFault, DeviceInput, ResolvedDevice};
use crate::cache::ContainerCache;

/// The code of the refusal of a device that already serves a session.
const BUSY: &str = "V6014";

/// Input of `online_compare`.
#[derive(Debug, Deserialize, JsonSchema)]
pub struct OnlineCompareInput {
    /// The device to compare (see [`DeviceInput`]).
    pub device: DeviceInput,
    /// The sources of the project, compiled to find what the project has. Without
    /// them the controller is compared with no project.
    #[serde(default)]
    pub sources: Option<Vec<SourceInput>>,
    /// Compiler options (dialect + optional feature-flag overrides). Required
    /// with `sources`.
    #[serde(default)]
    #[schemars(schema_with = "super::common::options_schema")]
    pub options: Option<serde_json::Value>,
}

/// Response of `online_compare`.
#[derive(Debug, Serialize)]
pub struct OnlineCompareResponse {
    /// Whether the comparison was made. A controller that cannot be reached or
    /// is busy is a comparison (rows 1 and 2), so `ok` is `true` for it.
    pub ok: bool,
    /// The comparison, as `ironplcc online-compare` prints it.
    pub comparison: Option<Comparison>,
    /// The V-code of a refusal of the device that is not a busy device.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub v_code: Option<String>,
    /// The text of that refusal.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub message: Option<String>,
    /// The messages of compiling `sources`, and the validation failures of the
    /// call.
    pub diagnostics: Vec<serde_json::Value>,
}

/// The project of a call: its state, and the messages of the build.
pub struct Prepared {
    state: ProjectState,
    diagnostics: Vec<serde_json::Value>,
}

/// Compiles `sources` to find what the project has. This runs on the thread that
/// serves the request (the stack budget), apart from the exchange with the
/// device.
pub fn prepare_project(
    sources: Option<&[SourceInput]>,
    options: Option<&serde_json::Value>,
    cache: &Mutex<ContainerCache>,
) -> Prepared {
    let Some(sources) = sources else {
        return Prepared {
            state: ProjectState::NeverBuilt,
            diagnostics: vec![],
        };
    };
    let null = serde_json::Value::Null;
    let compiled = super::compile::build_response(sources, options.unwrap_or(&null), false, cache);
    let built = compiled.container_id.as_ref().and_then(|id| {
        let mut guard = cache.lock().unwrap_or_else(|e| e.into_inner());
        let cached = guard.get(id)?;
        ContainerIdentity::of_bytes(&cached.iplc_bytes)
    });
    let state = match (compiled.ok, built) {
        (true, Some(identity)) => ProjectState::UpToDate(BuiltApplication {
            name: project_name(sources),
            content_hash: identity.content_hash,
            layout_hash: identity.layout_hash,
        }),
        // A build that failed, or whose container the cache no longer has.
        (true, None) | (false, _) => ProjectState::BuildFails(None),
    };
    Prepared {
        state,
        diagnostics: compiled.diagnostics,
    }
}

/// How the attempt to connect went.
enum Gathered {
    /// The controller answered `identity` (or, if it predates the handshake,
    /// nothing).
    Reached(Box<DeviceFacts>),
    Unreachable,
    Busy,
    /// The device refused the session with a code that is not "busy".
    Refused(ironplc_runtime::CommandError),
    /// The session failed in a way that is not an answer of the comparison.
    Failed(String),
}

/// Connects to the device, reads its identity and ends the session.
fn gather(device: &ResolvedDevice) -> Gathered {
    let session = match Session::connect(&device.endpoint) {
        Ok(session) => session,
        Err(error) => return gathered_of(error),
    };
    let facts = match session.identity() {
        // A device that predates `identity` answers nothing about itself.
        None => Ok(DeviceFacts::default()),
        Some(identity) => facts_of(identity),
    };
    // The session ends here, before the comparison, so the next call finds the
    // device free.
    drop(session);
    match facts {
        Ok(facts) => Gathered::Reached(Box::new(facts)),
        Err(message) => Gathered::Failed(message),
    }
}

fn gathered_of(error: SessionError) -> Gathered {
    match error {
        SessionError::Open { .. } | SessionError::TimedOut { .. } | SessionError::Closed { .. } => {
            Gathered::Unreachable
        }
        SessionError::Device(refusal) if refusal.v_code() == BUSY => Gathered::Busy,
        SessionError::Device(refusal) => Gathered::Refused(refusal),
        other @ (SessionError::Codec { .. }
        | SessionError::Protocol { .. }
        | SessionError::ProtocolTooNew { .. }) => Gathered::Failed(other.to_string()),
    }
}

/// The comparison's reading of an identity: the wire form of the payload, which
/// is the one form every client reads.
pub fn facts_of(identity: &ironplc_runtime::IdentityPayload) -> Result<DeviceFacts, String> {
    let value = serde_json::to_value(identity).map_err(|err| err.to_string())?;
    DeviceFacts::from_identity(&value).map_err(|err| err.to_string())
}

/// Builds the `online_compare` response for a resolved device.
pub fn build_response(prepared: &Prepared, device: &ResolvedDevice) -> OnlineCompareResponse {
    let gathered = gather(device);
    let attempt = match &gathered {
        Gathered::Reached(facts) => Attempt::Ok(facts),
        Gathered::Unreachable => Attempt::Unreachable,
        Gathered::Busy => Attempt::Busy,
        Gathered::Refused(refusal) => {
            return OnlineCompareResponse {
                ok: false,
                comparison: None,
                v_code: Some(refusal.v_code().to_string()),
                message: Some(refusal.message().to_string()),
                diagnostics: prepared.diagnostics.clone(),
            }
        }
        Gathered::Failed(error) => {
            return failure(
                prepared,
                text::mcp_device_connection_failed(device.describe(), error.clone()),
            )
        }
    };
    OnlineCompareResponse {
        ok: true,
        comparison: Some(compare(&attempt, &Records::default(), &prepared.state)),
        v_code: None,
        message: None,
        diagnostics: prepared.diagnostics.clone(),
    }
}

/// The response for a `device` parameter that could not be resolved.
pub fn unresolved(prepared: &Prepared, fault: DeviceFault) -> OnlineCompareResponse {
    let mut diagnostics = prepared.diagnostics.clone();
    match fault {
        DeviceFault::Invalid(text) => diagnostics.extend(diagnostics_of(text)),
        DeviceFault::IronLink(error) => diagnostics.extend(ironlink_diagnostics(&error)),
    }
    OnlineCompareResponse {
        ok: false,
        comparison: None,
        v_code: None,
        message: None,
        diagnostics,
    }
}

fn failure(prepared: &Prepared, text: ironplc_problems::Text) -> OnlineCompareResponse {
    let mut diagnostics = prepared.diagnostics.clone();
    diagnostics.extend(diagnostics_of(text));
    OnlineCompareResponse {
        ok: false,
        comparison: None,
        v_code: None,
        message: None,
        diagnostics,
    }
}

fn diagnostics_of(text: ironplc_problems::Text) -> Vec<serde_json::Value> {
    serialize_diagnostics(&[validation_diagnostic(text)], &[])
}

#[cfg(test)]
mod tests;
