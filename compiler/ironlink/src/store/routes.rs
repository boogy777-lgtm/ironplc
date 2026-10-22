//! The route record: how THIS workstation reaches a device id.
//!
//! One route for each device id; setting a route again replaces it (the latest
//! wins). A route is independent of any project. What the record means is
//! decided in `service`: a route is stored only after an identify to its
//! address returned its device id, so every stored route was verified at the time
//! it carries.

use std::collections::BTreeMap;
use std::net::IpAddr;

use serde::{Deserialize, Serialize};

use super::{Store, StoreFile};
use crate::driver::DriverKind;
use crate::error::IronLinkError;
use crate::observation::is_device_id;

/// How the workstation reaches a device, as it was verified.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase", deny_unknown_fields)]
pub(crate) struct RouteRecord {
    /// The address the device answered from.
    pub(crate) address: IpAddr,
    /// The discovery driver by which the device was verified.
    pub(crate) driver: DriverKind,
    /// The TCP port of the session, as the device said.
    pub(crate) session_port: u16,
    /// Milliseconds since the Unix epoch at which the device answered.
    pub(crate) verified_at: u64,
}

/// A route with the device id it is for: a row of the route list.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct RouteEntry {
    pub(crate) device_id: String,
    #[serde(flatten)]
    pub(crate) route: RouteRecord,
}

impl RouteEntry {
    /// Whether the route goes to `address` and `session_port`.
    pub(crate) fn reaches(&self, address: IpAddr, session_port: u16) -> bool {
        self.route.address == address && self.route.session_port == session_port
    }
}

/// `routes.json`.
#[derive(Clone, Debug, Default, PartialEq, Eq, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct RoutesFile {
    routes: BTreeMap<String, RouteRecord>,
}

impl StoreFile for RoutesFile {
    const NAME: &'static str = "routes.json";
    const WHAT: &'static str = "the route record";

    fn check(&self) -> Result<(), String> {
        for (device_id, route) in &self.routes {
            if !is_device_id(device_id) {
                return Err(format!(
                    "names '{device_id}', which is not a device id (32 lowercase hexadecimal \
                     digits)"
                ));
            }
            if route.session_port == 0 {
                return Err(format!(
                    "has the session port 0 in the route of {device_id}"
                ));
            }
        }
        Ok(())
    }
}

impl Store {
    /// Every route, ordered by device id.
    pub(crate) fn routes(&self) -> Result<Vec<RouteEntry>, IronLinkError> {
        Ok(self
            .read::<RoutesFile>()?
            .routes
            .into_iter()
            .map(|(device_id, route)| RouteEntry { device_id, route })
            .collect())
    }

    /// Makes `route` the route of `device_id` and returns the route it replaced.
    pub(crate) fn set_route(
        &self,
        device_id: &str,
        route: RouteRecord,
    ) -> Result<Option<RouteRecord>, IronLinkError> {
        let mut file = self.read::<RoutesFile>()?;
        let replaced = file.routes.insert(device_id.to_string(), route);
        self.write(&file)?;
        Ok(replaced)
    }

    /// Removes the route of `device_id` and returns it; `None`, and no write,
    /// when there was none.
    pub(crate) fn remove_route(
        &self,
        device_id: &str,
    ) -> Result<Option<RouteRecord>, IronLinkError> {
        let mut file = self.read::<RoutesFile>()?;
        let removed = file.routes.remove(device_id);
        if removed.is_some() {
            self.write(&file)?;
        }
        Ok(removed)
    }
}
