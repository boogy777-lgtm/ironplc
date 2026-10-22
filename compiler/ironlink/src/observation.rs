//! What IronLink has heard, and how it is grouped into devices.
//!
//! An [`Observation`] is what one decoded discovery reply said, with the route
//! it came by and the time it was seen (ADR-0084: an observation ages and is
//! never a promise that the device is reachable now). A datagram that decodes
//! as a reply of another wire version is its own outcome, [`OtherVersion`], so
//! that the user is told to update IronLink; [`Heard`] is the enum of the two.
//!
//! [`group`] turns the observations of one driver into devices: one entry for
//! each device id, with one variant for each distinct set of identity fields,
//! and the routes by which each variant answered. More than one variant under
//! one id is an identity conflict.

use std::collections::BTreeMap;
use std::net::IpAddr;
use std::time::{SystemTime, UNIX_EPOCH};

use ironplc_discovery::Reply;
use serde::Serialize;

use crate::driver::DriverKind;

/// How a controller is reached once it is found. The class of connection
/// methods is not built (ADR-0084); TCP is the only way the session is carried
/// today, and a route names it so that a later method is one more value.
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Serialize)]
#[serde(rename_all = "lowercase")]
pub(crate) enum Method {
    /// The session over TCP on the session port.
    Tcp,
}

/// How this workstation reaches a device (ADR-0084 "Route").
#[derive(Clone, Debug, PartialEq, Eq, PartialOrd, Ord, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct Route {
    /// The discovery driver that found the device by this route.
    pub(crate) driver: DriverKind,
    /// The address the reply came from.
    pub(crate) address: IpAddr,
    /// The TCP port of the session, as the device said.
    pub(crate) session_port: u16,
    /// How the session is carried.
    pub(crate) method: Method,
}

/// What one discovery reply said, with the route and the time it was seen.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct Observation {
    /// The 128 bits of the device id as 32 lowercase hexadecimal digits.
    pub(crate) device_id: String,
    pub(crate) name: String,
    pub(crate) model: String,
    pub(crate) runtime_version: String,
    /// The version of the session protocol the device speaks.
    pub(crate) protocol: u32,
    pub(crate) session_port: u16,
    /// Whether the session requires credentials.
    pub(crate) protected: bool,
    pub(crate) route: Route,
    /// Milliseconds since the Unix epoch at which the reply was received.
    pub(crate) seen_at: u64,
}

impl Observation {
    /// The observation of `reply`, which `driver` received from `source`.
    pub(crate) fn from_reply(
        reply: &Reply<'_>,
        driver: DriverKind,
        source: IpAddr,
        seen_at: u64,
    ) -> Self {
        Observation {
            device_id: device_id_hex(&reply.device_id),
            name: reply.name.to_string(),
            model: reply.model.to_string(),
            runtime_version: reply.runtime_version.to_string(),
            protocol: reply.protocol,
            session_port: reply.session_port,
            protected: reply.protected,
            route: Route {
                driver,
                address: source,
                session_port: reply.session_port,
                method: Method::Tcp,
            },
            seen_at,
        }
    }
}

/// A datagram that is a discovery reply of another wire version: the device is
/// there and speaks a discovery this IronLink does not (ADR-0084), so the user
/// is told to update IronLink instead of being shown nothing.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct OtherVersion {
    pub(crate) address: IpAddr,
    /// The wire version the datagram names.
    pub(crate) wire_version: u8,
    pub(crate) seen_at: u64,
}

/// The outcome of one datagram that was accepted as discovery. Every outcome is
/// produced only from a datagram that `decode_reply` examined.
#[derive(Clone, Debug, PartialEq, Eq)]
pub(crate) enum Heard {
    /// A version-1 reply: a device and its identity.
    Device(Observation),
    /// A reply of another wire version.
    OtherVersion(OtherVersion),
}

impl Heard {
    fn seen_at(&self) -> u64 {
        match self {
            Heard::Device(observation) => observation.seen_at,
            Heard::OtherVersion(other) => other.seen_at,
        }
    }

    /// Whether `other` is a repetition of this outcome from the same source:
    /// the same device id on the same route (address and session port), or the
    /// same address speaking the same other version. The session port is part of
    /// the key: two processes on one address that share an id (a copied settings
    /// file) are two routes, and the grouping must see both.
    fn repeated_by(&self, other: &Heard) -> bool {
        match (self, other) {
            (Heard::Device(a), Heard::Device(b)) => {
                a.device_id == b.device_id
                    && a.route.address == b.route.address
                    && a.route.session_port == b.route.session_port
            }
            (Heard::OtherVersion(a), Heard::OtherVersion(b)) => {
                a.address == b.address && a.wire_version == b.wire_version
            }
            (Heard::Device(_), Heard::OtherVersion(_))
            | (Heard::OtherVersion(_), Heard::Device(_)) => false,
        }
    }
}

/// Adds `new` to `heard`. A repetition of an outcome already there (the same
/// device id on the same route) replaces it when it is not older, so one
/// source is one entry, with the latest time and the latest content.
pub(crate) fn merge(heard: &mut Vec<Heard>, new: Heard) {
    match heard.iter_mut().find(|old| old.repeated_by(&new)) {
        Some(old) => {
            if new.seen_at() >= old.seen_at() {
                *old = new;
            }
        }
        None => heard.push(new),
    }
}

/// Milliseconds since the Unix epoch now.
///
/// A clock set before 1970 reports 0, which every reader shows as "seen long
/// ago". That is the contract of the value, not a hidden failure: the time is
/// only the age of an observation.
pub(crate) fn now_ms() -> u64 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map_or(0, |since| {
            u64::try_from(since.as_millis()).unwrap_or(u64::MAX)
        })
}

/// The 32 lowercase hexadecimal digits of a device id.
///
/// `ironplcvm` has the same function, but in a binary crate, which this one
/// cannot depend on; the discovery crate is `no_std` without an allocator and
/// formats nothing.
pub(crate) fn device_id_hex(bytes: &[u8; 16]) -> String {
    bytes.iter().map(|byte| format!("{byte:02x}")).collect()
}

/// The identity fields of a device: the fields that must be equal in the replies
/// of one device id for it to be one device (ADR-0084 "One device id at several
/// addresses"). The session port is not among them: it belongs to the route.
#[derive(Clone, Debug, PartialEq, Eq, PartialOrd, Ord, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct Identity {
    pub(crate) name: String,
    pub(crate) model: String,
    pub(crate) runtime_version: String,
    pub(crate) protocol: u32,
    pub(crate) protected: bool,
}

/// One route by which a variant answered, with the time of the answer.
#[derive(Clone, Debug, PartialEq, Eq, PartialOrd, Ord, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct SeenRoute {
    #[serde(flatten)]
    pub(crate) route: Route,
    pub(crate) seen_at: u64,
}

/// The replies that said the same identity, with the routes they came by.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct Variant {
    #[serde(flatten)]
    pub(crate) identity: Identity,
    pub(crate) routes: Vec<SeenRoute>,
}

/// One device id as heard by a driver: one variant, or several when replies
/// under the id differ, which is an identity conflict.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct Device {
    pub(crate) device_id: String,
    /// More than one variant answers under this id.
    pub(crate) conflict: bool,
    pub(crate) variants: Vec<Variant>,
}

/// Groups observations by device id, and under each id by identity. The result
/// does not depend on the order in which the replies arrived: devices are
/// ordered by the name of their first variant and then by id, variants by
/// identity, routes by address.
pub(crate) fn group(observations: Vec<Observation>) -> Vec<Device> {
    let mut by_id: BTreeMap<String, BTreeMap<Identity, Vec<SeenRoute>>> = BTreeMap::new();
    for observation in observations {
        let identity = Identity {
            name: observation.name,
            model: observation.model,
            runtime_version: observation.runtime_version,
            protocol: observation.protocol,
            protected: observation.protected,
        };
        by_id
            .entry(observation.device_id)
            .or_default()
            .entry(identity)
            .or_default()
            .push(SeenRoute {
                route: observation.route,
                seen_at: observation.seen_at,
            });
    }

    let mut devices: Vec<Device> = by_id
        .into_iter()
        .map(|(device_id, variants)| {
            let variants: Vec<Variant> = variants
                .into_iter()
                .map(|(identity, mut routes)| {
                    routes.sort();
                    Variant { identity, routes }
                })
                .collect();
            Device {
                device_id,
                conflict: variants.len() > 1,
                variants,
            }
        })
        .collect();
    devices.sort_by_cached_key(|device| {
        let first_name = device
            .variants
            .first()
            .map(|variant| variant.identity.name.clone());
        (first_name, device.device_id.clone())
    });
    devices
}

#[cfg(test)]
mod tests;
