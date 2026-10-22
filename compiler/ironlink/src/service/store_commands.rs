//! The commands that keep IronLink's own data in the workstation store: the route
//! record and the address list (`crate::store`).
//!
//! A route says how THIS workstation reaches a device id. IronLink establishes it
//! itself: `route set` sends a fresh identify to the address and stores the route
//! only when the answer carries the device id, so a stored route is a verified
//! one and carries the time of the answer. Setting a route again overwrites it.

use ironplc_discovery::DeviceId;
use serde::{Deserialize, Serialize};

use super::{identify_at, parse_device_id, parse_target, Answer};
use crate::error::IronLinkError;
use crate::store::routes::{RouteEntry, RouteRecord};
use crate::store::Store;
use ironplc_runtime::DEFAULT_SESSION_PORT;

/// Asks for the route to `device_id` to be verified at `address` and stored.
#[derive(Clone, Debug, PartialEq, Eq, Deserialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct RouteSetRequest {
    /// 32 lowercase hexadecimal digits.
    pub(crate) device_id: String,
    /// `IP` or `IP:PORT` of the controller.
    pub(crate) address: String,
    pub(crate) timeout_ms: Option<u64>,
    pub(crate) attempts: Option<u32>,
}

/// The route that was stored.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct RouteSetResult {
    pub(crate) device_id: DeviceId,
    pub(crate) route: RouteRecord,
    /// Whether the device had a route, which this one replaced.
    pub(crate) replaced: bool,
}

/// Verifies the route to `request.device_id` at `request.address` with a fresh
/// identify and stores it.
///
/// Nothing is stored when nothing answers (L6003), when another device id
/// answers (L6008), or when the answer is of a discovery version this IronLink
/// cannot read (L6003): the store holds only verified routes.
pub(crate) fn route_set(
    request: &RouteSetRequest,
    store: &Store,
) -> Result<RouteSetResult, IronLinkError> {
    let device_id = parse_device_id(&request.device_id)?;
    let address = parse_target(&request.address, DEFAULT_SESSION_PORT)?;
    let identified = identify_at(address, request.timeout_ms, request.attempts)?;
    let observation = match identified.answer {
        Answer::Device { observation } => observation,
        Answer::OtherVersion { other_version } => {
            return Err(IronLinkError::other_version_answer(
                address,
                other_version.wire_version,
            ))
        }
        Answer::None => return Err(IronLinkError::no_answer(address, identified.timeout_ms)),
    };
    if observation.device_id != device_id {
        return Err(IronLinkError::route_mismatch(
            address,
            device_id,
            observation.device_id,
        ));
    }
    let route = RouteRecord {
        address: observation.route.address,
        driver: observation.route.driver,
        session_port: observation.session_port,
        verified_at: observation.seen_at,
    };
    let replaced = store.set_route(&device_id, route.clone())?.is_some();
    Ok(RouteSetResult {
        device_id,
        route,
        replaced,
    })
}

/// Asks for the route to `device_id` to be forgotten.
#[derive(Clone, Debug, PartialEq, Eq, Deserialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct RouteRemoveRequest {
    pub(crate) device_id: String,
}

/// Whether there was a route to remove.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct RouteRemoveResult {
    pub(crate) device_id: DeviceId,
    /// `false` when the device had no route: that is not an error.
    pub(crate) removed: bool,
}

/// Removes the route to a device.
pub(crate) fn route_remove(
    request: &RouteRemoveRequest,
    store: &Store,
) -> Result<RouteRemoveResult, IronLinkError> {
    let device_id = parse_device_id(&request.device_id)?;
    let removed = store.remove_route(&device_id)?.is_some();
    Ok(RouteRemoveResult { device_id, removed })
}

/// The routes of this workstation.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct RouteListResult {
    /// Ordered by device id.
    pub(crate) routes: Vec<RouteEntry>,
}

/// Lists the routes of this workstation.
pub(crate) fn route_list(store: &Store) -> Result<RouteListResult, IronLinkError> {
    Ok(RouteListResult {
        routes: store.routes()?,
    })
}

/// Names one controller address of the stored address list.
#[derive(Clone, Debug, PartialEq, Eq, Deserialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct AddressRequest {
    /// `IP` or `IP:PORT`; an IP alone takes the session port.
    pub(crate) address: String,
}

/// Whether the address was added.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct AddressAddResult {
    /// The address with its port.
    pub(crate) address: String,
    /// `false` when it was in the list already.
    pub(crate) added: bool,
}

/// Whether the address was removed.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct AddressRemoveResult {
    pub(crate) address: String,
    /// `false` when it was not in the list: that is not an error.
    pub(crate) removed: bool,
}

/// The stored address list.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct AddressListResult {
    /// `IP:PORT`, unique and sorted.
    pub(crate) addresses: Vec<String>,
}

/// Adds a controller address to the stored list.
pub(crate) fn address_add(
    request: &AddressRequest,
    store: &Store,
) -> Result<AddressAddResult, IronLinkError> {
    let address = parse_target(&request.address, DEFAULT_SESSION_PORT)?;
    let added = store.add_address(address)?;
    Ok(AddressAddResult {
        address: address.to_string(),
        added,
    })
}

/// Removes a controller address from the stored list.
pub(crate) fn address_remove(
    request: &AddressRequest,
    store: &Store,
) -> Result<AddressRemoveResult, IronLinkError> {
    let address = parse_target(&request.address, DEFAULT_SESSION_PORT)?;
    let removed = store.remove_address(address)?;
    Ok(AddressRemoveResult {
        address: address.to_string(),
        removed,
    })
}

/// Lists the stored controller addresses.
pub(crate) fn address_list(store: &Store) -> Result<AddressListResult, IronLinkError> {
    Ok(AddressListResult {
        addresses: store.addresses()?.iter().map(ToString::to_string).collect(),
    })
}

#[cfg(test)]
mod tests;
