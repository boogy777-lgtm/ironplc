//! What IronLink does: browse, identify and report its version.
//!
//! These functions are the one implementation. The command line and the
//! `serve` shell both call them, and only render the result differently: the
//! clients are thin (ADR-0084 "Decisions are made in one place").
//!
//! The requests carry what the user typed, as text, and are checked here, so
//! that the command line and the shell refuse a bad address in the same way and
//! with the same code (L6004).
//!
//! The commands that keep IronLink's own data (the routes and the address list of
//! the workstation store) are in [`store_commands`]; browse reads the store too.
//! The store is always passed in: nothing here looks for the user's directory.

use std::net::{IpAddr, SocketAddr};

use ironplc_discovery::WIRE_VERSION;
use ironplc_runtime::DEFAULT_SESSION_PORT;
use serde::{Deserialize, Serialize};

use crate::collect::{Stop, Window};
use crate::driver::address_list::AddressList;
use crate::driver::{Driver, DriverKind, Selection};
use crate::error::IronLinkError;
use crate::observation::{group, is_device_id, Device, Heard, Observation, OtherVersion};
use crate::parallel::map_in_parallel;
use crate::store::routes::RouteEntry;
use crate::store::Store;

mod store_commands;
pub(crate) use store_commands::{
    address_add, address_list, address_remove, route_list, route_remove, route_set,
    AddressAddResult, AddressListResult, AddressRemoveResult, AddressRequest, RouteListResult,
    RouteRemoveRequest, RouteRemoveResult, RouteSetRequest, RouteSetResult,
};

/// The name shown for the workstation when the environment gives none.
const UNNAMED_WORKSTATION: &str = "this workstation";

/// Asks the drivers selected by the request to find controllers.
#[derive(Clone, Debug, Default, PartialEq, Eq, Deserialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct BrowseRequest {
    /// Local addresses of this PC to broadcast from.
    #[serde(default)]
    pub(crate) interfaces: Vec<String>,
    /// Controller addresses (`IP` or `IP:PORT`) to ask directly.
    #[serde(default)]
    pub(crate) addresses: Vec<String>,
    /// The discovery port of the broadcast and of an address without a port.
    pub(crate) port: Option<u16>,
    /// How long to wait for replies, in milliseconds.
    pub(crate) timeout_ms: Option<u64>,
    /// How many times to send the request.
    pub(crate) attempts: Option<u32>,
}

/// Asks one route for a fresh observation.
#[derive(Clone, Debug, PartialEq, Eq, Deserialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct IdentifyRequest {
    /// `IP` or `IP:PORT` of the controller.
    pub(crate) address: String,
    pub(crate) timeout_ms: Option<u64>,
    pub(crate) attempts: Option<u32>,
}

/// The workstation IronLink runs on: the root of the browse tree.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
pub(crate) struct Workstation {
    pub(crate) name: String,
}

/// What one driver found.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct DriverReport {
    pub(crate) kind: DriverKind,
    /// What the driver aimed at.
    pub(crate) targets: Vec<String>,
    pub(crate) devices: Vec<Device>,
    /// Addresses that answered with a discovery reply of another wire version.
    pub(crate) other_versions: Vec<OtherVersion>,
    /// How many datagrams were not discovery replies.
    pub(crate) undecodable: u32,
}

/// The tree a browse returns: the workstation and its drivers.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct BrowseResult {
    pub(crate) workstation: Workstation,
    pub(crate) drivers: Vec<DriverReport>,
    /// The stored routes of devices that no driver heard in this browse. They
    /// are not observations (those come only from replies); they say that this
    /// workstation has a route to a device that did not answer now.
    pub(crate) routes_not_seen: Vec<RouteEntry>,
}

/// What the route answered to identify.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(tag = "answer", rename_all = "camelCase")]
pub(crate) enum Answer {
    /// A device answered.
    Device { observation: Observation },
    /// Something answered with a discovery reply of another wire version.
    OtherVersion {
        #[serde(rename = "otherVersion")]
        other_version: OtherVersion,
    },
    /// Nothing answered within the time. This is a normal answer of the shell;
    /// the command line turns it into L6003.
    None,
}

/// The result of identify: the route asked and what it answered.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct IdentifyResult {
    /// The address asked, with its port.
    pub(crate) address: String,
    /// The time waited, in milliseconds.
    pub(crate) timeout_ms: u64,
    #[serde(flatten)]
    pub(crate) answer: Answer,
}

/// The versions of this IronLink.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct VersionInfo {
    /// The version of this program.
    pub(crate) ironlink: &'static str,
    /// The discovery wire version it speaks.
    pub(crate) discovery_wire_version: u8,
}

/// The versions of this IronLink.
pub(crate) fn version() -> VersionInfo {
    VersionInfo {
        ironlink: env!("CARGO_PKG_VERSION"),
        discovery_wire_version: WIRE_VERSION,
    }
}

/// Finds controllers with every driver the request selects, all at the same time.
///
/// The address-list driver asks the addresses of the request and the stored
/// address list; each route of a device that is the stored route of its device
/// id is marked `routed`.
pub(crate) fn browse(
    request: &BrowseRequest,
    store: &Store,
) -> Result<BrowseResult, IronLinkError> {
    let routes = store.routes()?;
    let selection = selection_of(request, store.addresses()?)?;
    let window = Window::from_options(request.timeout_ms, request.attempts, Stop::AtDeadline)?;
    let drivers: Vec<Box<dyn Driver>> = DriverKind::ALL
        .iter()
        .map(|kind| kind.build(&selection))
        .collect::<Result<Vec<_>, _>>()?
        .into_iter()
        .flatten()
        .collect();
    let mut drivers = map_in_parallel(&drivers, |driver| report_of(driver.as_ref(), &window))?;
    for device in drivers
        .iter_mut()
        .flat_map(|driver| driver.devices.iter_mut())
    {
        mark_routed(device, &routes);
    }
    let routes_not_seen = routes
        .into_iter()
        .filter(|route| {
            !drivers
                .iter()
                .flat_map(|driver| &driver.devices)
                .any(|device| device.device_id == route.device_id)
        })
        .collect();
    Ok(BrowseResult {
        workstation: Workstation {
            name: workstation_name(|key| std::env::var(key).ok()),
        },
        drivers,
        routes_not_seen,
    })
}

/// Sets `routed` on each route of `device` that is the stored route of its device
/// id: the same address and the same session port.
fn mark_routed(device: &mut Device, routes: &[RouteEntry]) {
    let Some(stored) = routes
        .iter()
        .find(|route| route.device_id == device.device_id)
    else {
        return;
    };
    for seen in device
        .variants
        .iter_mut()
        .flat_map(|variant| variant.routes.iter_mut())
    {
        seen.routed = stored.reaches(seen.route.address, seen.route.session_port);
    }
}

/// Asks one route for a fresh observation. The answer may be that nothing
/// answered.
pub(crate) fn identify(request: &IdentifyRequest) -> Result<IdentifyResult, IronLinkError> {
    let address = parse_target(&request.address, DEFAULT_SESSION_PORT)?;
    identify_at(address, request.timeout_ms, request.attempts)
}

/// Asks the route `address` for a fresh observation.
fn identify_at(
    address: SocketAddr,
    timeout_ms: Option<u64>,
    attempts: Option<u32>,
) -> Result<IdentifyResult, IronLinkError> {
    let window = Window::from_options(timeout_ms, attempts, Stop::OnFirstOutcome)?;
    let collected = AddressList::new(vec![address]).run(&window)?;
    let answer = match collected.heard.into_iter().next() {
        Some(Heard::Device(observation)) => Answer::Device { observation },
        Some(Heard::OtherVersion(other_version)) => Answer::OtherVersion { other_version },
        None => Answer::None,
    };
    Ok(IdentifyResult {
        address: address.to_string(),
        timeout_ms: window.timeout_ms(),
        answer,
    })
}

/// Runs `driver` and shapes what it heard into its part of the tree.
fn report_of(driver: &dyn Driver, window: &Window) -> Result<DriverReport, IronLinkError> {
    let collected = driver.run(window)?;
    let mut observations = Vec::new();
    let mut other_versions = Vec::new();
    for heard in collected.heard {
        match heard {
            Heard::Device(observation) => observations.push(observation),
            Heard::OtherVersion(other) => other_versions.push(other),
        }
    }
    other_versions.sort_by_key(|other| (other.address, other.wire_version));
    Ok(DriverReport {
        kind: driver.kind(),
        targets: driver.targets(),
        devices: group(observations),
        other_versions,
        undecodable: collected.undecodable,
    })
}

/// What the request names, checked, with the stored address list `remembered`.
fn selection_of(
    request: &BrowseRequest,
    remembered: Vec<SocketAddr>,
) -> Result<Selection, IronLinkError> {
    let port = match request.port {
        Some(0) => return Err(IronLinkError::argument_invalid("the port must not be 0")),
        Some(port) => port,
        None => DEFAULT_SESSION_PORT,
    };
    let interfaces = request
        .interfaces
        .iter()
        .map(|text| parse_interface(text))
        .collect::<Result<Vec<_>, _>>()?;
    let addresses = request
        .addresses
        .iter()
        .map(|text| parse_target(text, port))
        .collect::<Result<Vec<_>, _>>()?;
    Ok(Selection {
        interfaces: without_repeats(interfaces),
        addresses: without_repeats(addresses),
        remembered,
        port,
    })
}

/// The items in their order, each once.
fn without_repeats<T: PartialEq>(items: Vec<T>) -> Vec<T> {
    let mut unique = Vec::with_capacity(items.len());
    for item in items {
        if !unique.contains(&item) {
            unique.push(item);
        }
    }
    unique
}

/// A device id as the user types it: 32 lowercase hexadecimal digits.
fn parse_device_id(text: &str) -> Result<String, IronLinkError> {
    if is_device_id(text) {
        Ok(text.to_string())
    } else {
        Err(IronLinkError::argument_invalid(format!(
            "'{text}' is not a device id; a device id is 32 lowercase hexadecimal digits, \
             for example 00112233445566778899aabbccddeeff"
        )))
    }
}

/// A local interface, named by one of its addresses.
fn parse_interface(text: &str) -> Result<IpAddr, IronLinkError> {
    text.trim().parse().map_err(|_| {
        IronLinkError::argument_invalid(format!(
            "'{text}' is not an IP address of this PC; name an interface by its address, \
             for example 192.168.1.20"
        ))
    })
}

/// A controller address: `IP:PORT`, or an IP alone with `default_port`.
fn parse_target(text: &str, default_port: u16) -> Result<SocketAddr, IronLinkError> {
    let trimmed = text.trim();
    let address = trimmed
        .parse::<SocketAddr>()
        .ok()
        .or_else(|| {
            trimmed
                .parse::<IpAddr>()
                .ok()
                .map(|ip| SocketAddr::new(ip, default_port))
        })
        .ok_or_else(|| {
            IronLinkError::argument_invalid(format!("'{text}' is not an IP address or IP:PORT"))
        })?;
    if address.port() == 0 {
        return Err(IronLinkError::argument_invalid(format!(
            "the port of '{text}' must not be 0"
        )));
    }
    Ok(address)
}

/// The name of this workstation: `COMPUTERNAME`, else `HOSTNAME`, else a fixed
/// text. `lookup` reads an environment variable, so a test need not set one.
fn workstation_name(lookup: impl Fn(&str) -> Option<String>) -> String {
    ["COMPUTERNAME", "HOSTNAME"]
        .into_iter()
        .find_map(|key| lookup(key).filter(|name| !name.trim().is_empty()))
        .unwrap_or_else(|| UNNAMED_WORKSTATION.to_string())
}

#[cfg(test)]
mod tests;
