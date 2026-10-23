//! The `browse_network` MCP tool.
//!
//! Asks IronLink to find the controllers this workstation can see. The tool
//! starts `ironlink serve`, sends one `browse` command and returns the browse
//! result as JSON, unchanged: the server is a thin client of IronLink and does
//! not mirror its types (`specs/design/ironlink.md`).

use std::time::Duration;

use schemars::JsonSchema;
use serde::{Deserialize, Serialize};

use super::common::ironlink_diagnostics;
use crate::ironlink::{IronLink, IronLinkError};

/// How long IronLink waits for replies when the call names no time: its own
/// default (`specs/design/ironlink.md`, REQ-IL-ironlink-008).
const DEFAULT_WAIT_MS: u64 = 1000;

/// The longest wait IronLink accepts (REQ-IL-ironlink-008).
const MAX_WAIT_MS: u64 = 60000;

/// Input for the `browse_network` tool.
#[derive(Debug, Default, Deserialize, JsonSchema)]
pub struct BrowseNetworkInput {
    /// Controller addresses (`IP` or `IP:PORT`) to ask directly, besides the
    /// broadcast and the address list stored on this workstation.
    #[serde(default)]
    pub addresses: Vec<String>,
    /// How long to wait for replies, in milliseconds. IronLink's default is 1000
    /// and its largest value is 60000.
    #[serde(default)]
    pub timeout_ms: Option<u64>,
    /// How many times to send the request. IronLink's default is 2 and its
    /// largest value is 10.
    #[serde(default)]
    pub attempts: Option<u32>,
}

/// Response for the `browse_network` tool.
#[derive(Debug, Serialize)]
pub struct BrowseNetworkResponse {
    /// Whether IronLink answered the browse.
    pub ok: bool,
    /// The browse result of IronLink: the `workstation` and, for each driver
    /// that ran, its `kind`, `targets`, `devices`, `otherVersions` and
    /// `undecodable`. Null on failure.
    pub browse: Option<serde_json::Value>,
    /// The L-code of IronLink's refusal (such as `"L6004"`). Null when IronLink
    /// refused with no code, or did not refuse.
    pub l_code: Option<String>,
    /// What IronLink said, when it refused.
    pub message: Option<String>,
    /// Diagnostics: IronLink not found, or not answering (P8002).
    pub diagnostics: Vec<serde_json::Value>,
}

impl BrowseNetworkResponse {
    fn failure(
        l_code: Option<String>,
        message: Option<String>,
        diagnostics: Vec<serde_json::Value>,
    ) -> Self {
        BrowseNetworkResponse {
            ok: false,
            browse: None,
            l_code,
            message,
            diagnostics,
        }
    }
}

/// The `browse` command line for the input: only what the caller named is sent,
/// so IronLink's defaults stay IronLink's.
fn browse_command(input: &BrowseNetworkInput) -> serde_json::Value {
    let mut command = serde_json::json!({"command": "browse", "addresses": input.addresses});
    if let Some(timeout_ms) = input.timeout_ms {
        command["timeoutMs"] = timeout_ms.into();
    }
    if let Some(attempts) = input.attempts {
        command["attempts"] = attempts.into();
    }
    command
}

/// Builds the `browse_network` response from the `ironlink` that was found, or
/// the reason it was not.
pub fn build_response(
    input: &BrowseNetworkInput,
    ironlink: Result<IronLink, IronLinkError>,
) -> BrowseNetworkResponse {
    let ironlink = match ironlink {
        Ok(ironlink) => ironlink,
        Err(err) => return failure_of(err),
    };
    let wait = Duration::from_millis(input.timeout_ms.unwrap_or(DEFAULT_WAIT_MS).min(MAX_WAIT_MS));
    match ironlink.call(&browse_command(input), wait) {
        Ok(mut answer) => {
            if let Some(object) = answer.as_object_mut() {
                object.remove("response");
            }
            BrowseNetworkResponse {
                ok: true,
                browse: Some(answer),
                l_code: None,
                message: None,
                diagnostics: vec![],
            }
        }
        Err(err) => failure_of(err),
    }
}

/// The failure response for why IronLink gave no browse result.
pub fn failure_of(err: IronLinkError) -> BrowseNetworkResponse {
    match err {
        IronLinkError::Refused { l_code, message } => {
            BrowseNetworkResponse::failure(l_code, Some(message), vec![])
        }
        other => BrowseNetworkResponse::failure(None, None, ironlink_diagnostics(&other)),
    }
}

#[cfg(test)]
mod tests;
