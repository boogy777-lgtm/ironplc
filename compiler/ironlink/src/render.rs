//! The text a person reads: rows of a tree, as in RSLinx's RSWho, without boxes.
//!
//! This only renders. What is found and how it is grouped is decided before
//! (`service`, `observation`); the JSON of the shell holds the same facts.

use std::net::SocketAddr;

use ironplc_discovery::DeviceId;

use crate::observation::{group, Device, SeenRoute, Variant};
use crate::service::{
    Answer, BrowseResult, DriverReport, IdentifyResult, RouteListResult, RouteRemoveResult,
    RouteSetResult,
};
use crate::shell::Response;
use crate::store::routes::RouteEntry;

const INDENT: &str = "  ";

/// The text of a response, at `now_ms` (milliseconds since the Unix epoch), which
/// the ages of the observations are counted to. The text has no final newline.
pub(crate) fn text(response: &Response, now_ms: u64) -> String {
    match response {
        Response::Browse(result) => browse_text(result, now_ms),
        Response::Identify(result) => identify_text(result, now_ms),
        Response::RouteSet(result) => route_set_text(result, now_ms),
        Response::RouteRemove(result) => route_remove_text(result),
        Response::RouteList(result) => route_list_text(result, now_ms),
        Response::AddressAdd(result) if result.added => format!("added {}", result.address),
        Response::AddressAdd(result) => format!("{} was in the list already", result.address),
        Response::AddressRemove(result) if result.removed => {
            format!("removed {}", result.address)
        }
        Response::AddressRemove(result) => format!("{} was not in the list", result.address),
        Response::AddressList(result) if result.addresses.is_empty() => {
            "no stored addresses".to_string()
        }
        Response::AddressList(result) => result.addresses.join("\n"),
        Response::Version(info) => format!(
            "ironlink version {} (discovery wire version {})",
            info.ironlink, info.discovery_wire_version
        ),
        Response::Error(error) => match error.l_code {
            Some(code) => format!("{code} - {}", error.message),
            None => error.message.clone(),
        },
    }
}

fn browse_text(result: &BrowseResult, now_ms: u64) -> String {
    let mut lines = vec![result.workstation.name.clone()];
    for driver in &result.drivers {
        lines.extend(driver_lines(driver, now_ms));
    }
    if !result.routes_not_seen.is_empty() {
        lines.push(format!(
            "{INDENT}stored routes of devices that did not answer"
        ));
        for route in &result.routes_not_seen {
            lines.push(format!(
                "{INDENT}{INDENT}{}",
                route_entry_text(route, now_ms)
            ));
        }
    }
    lines.join("\n")
}

fn driver_lines(driver: &DriverReport, now_ms: u64) -> Vec<String> {
    let mut lines = vec![format!(
        "{INDENT}{}: {}",
        driver.kind.label(),
        driver.targets.join(", ")
    )];
    if driver.devices.is_empty() && driver.other_versions.is_empty() {
        lines.push(format!("{INDENT}{INDENT}no device answered"));
    }
    for device in &driver.devices {
        lines.extend(device_lines(device, 2, now_ms));
    }
    for other in &driver.other_versions {
        lines.push(format!(
            "{INDENT}{INDENT}{}  speaks discovery version {}; update IronLink to see it  \
             seen {} ago",
            other.address,
            other.wire_version,
            age(now_ms, other.seen_at)
        ));
    }
    if driver.undecodable > 0 {
        lines.push(format!(
            "{INDENT}{INDENT}{} datagram(s) were not discovery replies",
            driver.undecodable
        ));
    }
    lines
}

/// The rows of a device at `depth` levels of indentation: one row if it is one
/// device, and under a conflict header one row for each of the different answers.
fn device_lines(device: &Device, depth: usize, now_ms: u64) -> Vec<String> {
    match device.variants.as_slice() {
        [variant] if !device.conflict => {
            variant_lines(variant, Some(device.device_id), depth, now_ms)
        }
        variants => {
            let mut lines = vec![format!(
                "{}id {}  CONFLICT: {} different answers under this id",
                INDENT.repeat(depth),
                device.device_id,
                variants.len()
            )];
            for variant in variants {
                lines.extend(variant_lines(variant, None, depth + 1, now_ms));
            }
            lines
        }
    }
}

/// The row of a variant and, below it, one row for each further route. The id is
/// in the row unless the row is below a conflict header that shows it.
fn variant_lines(
    variant: &Variant,
    id: Option<DeviceId>,
    depth: usize,
    now_ms: u64,
) -> Vec<String> {
    let indent = INDENT.repeat(depth);
    let identity = &variant.identity;
    let mut routes = variant.routes.iter();
    let mut row = format!(
        "{indent}{}  {}  runtime {}",
        identity.name, identity.model, identity.runtime_version
    );
    if let Some(id) = id {
        row.push_str(&format!("  id {id}"));
    }
    if let Some(first) = routes.next() {
        row.push_str(&format!("  {}", route_text(first, now_ms)));
    }
    if identity.protected {
        row.push_str("  protected");
    }
    let mut lines = vec![row];
    for route in routes {
        lines.push(format!(
            "{indent}{INDENT}also {}",
            route_text(route, now_ms)
        ));
    }
    lines
}

/// `address:port  seen <age> ago`, and the marker `route` when the route record of
/// this workstation holds this route.
fn route_text(seen: &SeenRoute, now_ms: u64) -> String {
    let text = format!(
        "{}  seen {} ago",
        SocketAddr::new(seen.route.address, seen.route.session_port),
        age(now_ms, seen.seen_at)
    );
    if seen.routed {
        format!("{text}  route")
    } else {
        text
    }
}

/// `id  address:port  driver  verified <age> ago`: a stored route.
fn route_entry_text(entry: &RouteEntry, now_ms: u64) -> String {
    format!(
        "{}  {}  {}  verified {} ago",
        entry.device_id,
        SocketAddr::new(entry.route.address, entry.route.session_port),
        entry.route.driver.label(),
        age(now_ms, entry.route.verified_at)
    )
}

fn route_set_text(result: &RouteSetResult, now_ms: u64) -> String {
    let entry = RouteEntry {
        device_id: result.device_id,
        route: result.route.clone(),
    };
    let text = format!("stored route {}", route_entry_text(&entry, now_ms));
    if result.replaced {
        format!("{text}  (replaced the earlier route)")
    } else {
        text
    }
}

fn route_remove_text(result: &RouteRemoveResult) -> String {
    if result.removed {
        format!("removed the route to {}", result.device_id)
    } else {
        format!("no route to {}", result.device_id)
    }
}

fn route_list_text(result: &RouteListResult, now_ms: u64) -> String {
    if result.routes.is_empty() {
        return "no routes".to_string();
    }
    result
        .routes
        .iter()
        .map(|entry| route_entry_text(entry, now_ms))
        .collect::<Vec<_>>()
        .join("\n")
}

fn identify_text(result: &IdentifyResult, now_ms: u64) -> String {
    match &result.answer {
        Answer::Device { observation } => group(vec![observation.clone()])
            .iter()
            .flat_map(|device| device_lines(device, 0, now_ms))
            .collect::<Vec<_>>()
            .join("\n"),
        Answer::OtherVersion { other_version } => format!(
            "{}  speaks discovery version {}; update IronLink to see it  seen {} ago",
            other_version.address,
            other_version.wire_version,
            age(now_ms, other_version.seen_at)
        ),
        Answer::None => format!(
            "no answer from {} within {} ms",
            result.address, result.timeout_ms
        ),
    }
}

/// How long ago `seen_at` was at `now_ms`, in the largest whole unit. A time in
/// the future (the clocks differ, or the clock moved back) is `0s`.
fn age(now_ms: u64, seen_at: u64) -> String {
    let seconds = now_ms.saturating_sub(seen_at) / 1000;
    match seconds {
        0..=59 => format!("{seconds}s"),
        60..=3599 => format!("{}m", seconds / 60),
        _ => format!("{}h", seconds / 3600),
    }
}

#[cfg(test)]
mod tests;
