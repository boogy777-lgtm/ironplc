//! The `device` parameter of the tools that act on a device.
//!
//! A tool that takes it connects to the device through the session protocol,
//! the way the editor does, instead of acting on the in-process host. The
//! parameter names the device in one of two ways:
//!
//! - `address` and optionally `port`: a TCP connection to that address, the
//!   session port when no port is given.
//! - `device_id`: the 32-digit id of the device, resolved to a route by IronLink,
//!   which keeps the routes of this workstation (`routeList`). A project's path
//!   file would also name a route, but the server reads no file of the project
//!   (REQ-STL-mcp-006), so an id without a route is an error that says to browse.
//!
//! The server never refuses to connect because the device is another one than
//! the id asked for: the answer names the device that answered.

use ironplc_problems::{text, Text};
use ironplc_runtime::DEFAULT_SESSION_PORT;
use ironplc_session_client::{Endpoint, TcpEndpoint};
use schemars::JsonSchema;
use serde::Deserialize;

use crate::ironlink::{IronLink, IronLinkError};

/// The device a tool acts on. Name it by `address` (with an optional `port`) or
/// by `device_id`, not both.
#[derive(Clone, Debug, Default, Deserialize, JsonSchema)]
pub struct DeviceInput {
    /// IPv4 address, IPv6 literal or host name of the device. Connects over TCP.
    #[serde(default)]
    pub address: Option<String>,
    /// The session port of the device. Default 49152. Only with `address`.
    #[serde(default)]
    pub port: Option<u16>,
    /// The device id (32 lowercase hexadecimal digits), as `browse_network`
    /// reports it. The route to it is the one IronLink stores for this
    /// workstation.
    #[serde(default)]
    pub device_id: Option<String>,
}

/// A device ready to connect to.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct ResolvedDevice {
    /// How to reach it.
    pub endpoint: Endpoint,
    /// The device id that was asked for, when the device was named by id.
    pub requested_id: Option<String>,
}

impl ResolvedDevice {
    /// The device as the messages name it.
    pub fn describe(&self) -> String {
        match (&self.endpoint, &self.requested_id) {
            (Endpoint::Tcp(tcp), _) => format!("{}:{}", tcp.address, tcp.port),
            (_, Some(id)) => id.clone(),
            (endpoint, None) => endpoint.method().to_string(),
        }
    }
}

/// Why a device could not be resolved.
#[derive(Debug, PartialEq, Eq)]
pub enum DeviceFault {
    /// The parameter is not valid: the problem text for P8001.
    Invalid(Text),
    /// IronLink could not give the route.
    IronLink(IronLinkError),
}

/// Resolves the parameter to a device. `ironlink` finds IronLink, and is called
/// only when the device is named by id.
pub fn resolve(
    device: &DeviceInput,
    ironlink: impl FnOnce() -> Result<IronLink, IronLinkError>,
) -> Result<ResolvedDevice, DeviceFault> {
    match (&device.address, &device.device_id, device.port) {
        (Some(address), None, port) => Ok(ResolvedDevice {
            endpoint: Endpoint::Tcp(TcpEndpoint {
                address: address.clone(),
                port: port.unwrap_or(DEFAULT_SESSION_PORT),
            }),
            requested_id: None,
        }),
        (None, Some(device_id), None) => {
            let device_id = device_id.to_ascii_lowercase();
            let ironlink = ironlink().map_err(DeviceFault::IronLink)?;
            route_by_id(&ironlink, device_id)
        }
        _ => Err(DeviceFault::Invalid(
            text::mcp_input_validation_device_selector_invalid(),
        )),
    }
}

/// The route IronLink stores for `device_id`.
fn route_by_id(ironlink: &IronLink, device_id: String) -> Result<ResolvedDevice, DeviceFault> {
    let answer = ironlink
        .call(
            &serde_json::json!({"command": "routeList"}),
            std::time::Duration::ZERO,
        )
        .map_err(DeviceFault::IronLink)?;
    let route = answer["routes"]
        .as_array()
        .into_iter()
        .flatten()
        .find(|route| route["deviceId"] == device_id.as_str());
    let Some(route) = route else {
        return Err(DeviceFault::Invalid(
            text::mcp_input_validation_device_id_no_route(device_id),
        ));
    };
    let address = route["address"].as_str();
    let port = route["sessionPort"]
        .as_u64()
        .and_then(|port| u16::try_from(port).ok());
    match (address, port) {
        (Some(address), Some(port)) => Ok(ResolvedDevice {
            endpoint: Endpoint::Tcp(TcpEndpoint {
                address: address.to_string(),
                port,
            }),
            requested_id: Some(device_id),
        }),
        _ => Err(DeviceFault::IronLink(IronLinkError::Unavailable {
            message: "the route list of IronLink has an entry without an address or port"
                .to_string(),
        })),
    }
}

#[cfg(test)]
mod tests;
