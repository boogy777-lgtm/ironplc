//! The path record: the controller last chosen for an application and the route
//! last used to reach it, kept in `<app>.path.iplcinfo` beside the application
//! container ([`RecordKind::Path`]).
//!
//! The record travels with the project, so a project moved to another PC keeps
//! its last path (ADR 0084). It is a remembered convenience and neither an
//! intent nor a binding: the engineer can connect to any controller, and the
//! record changes only when the engineer chooses a controller.
//!
//! ## One writer
//!
//! [`record_choice`] is the only function that writes the record. The editor
//! reaches it through `ironplcc record-path`, so the format has one writer; the
//! editor reads the file itself and asks `ironplcc record-path-path` where it
//! is, as for the download record. [`read`] here is the reference reader that
//! the format is tested against.
//!
//! ## Format
//!
//! ```json
//! {
//!   "version": 1,
//!   "deviceId": "0123456789abcdef0123456789abcdef",
//!   "routeHint": { "address": "192.168.1.20", "sessionPort": 49152 },
//!   "chosenAt": 1761148800000
//! }
//! ```
//!
//! - `deviceId` is the id of the controller last chosen, 32 lowercase
//!   hexadecimal digits ([`DeviceId`]).
//! - `routeHint` is the last address that is not loopback, with the session
//!   port, or `null`. A loopback address means "this PC" and points at another
//!   machine on every other PC, so it is never stored here ([`is_loopback`]); it
//!   belongs to the workstation route record. Choosing a controller by a
//!   loopback address leaves the hint of the same controller as it was, and a
//!   different controller has no hint until a route that is not loopback is
//!   chosen.
//! - `chosenAt` is the time of the choice, in milliseconds since the Unix epoch.

use std::fmt;
use std::net::{IpAddr, Ipv6Addr};
use std::path::{Path, PathBuf};

use ironplc_dsl::diagnostic::Diagnostic;
use serde_json::{json, Value};

use crate::device_id::DeviceId;
use crate::records::{self, RecordKind, RecordRead};

/// The longest host name (RFC 1035), and the longest label in it.
const MAX_HOST_NAME: usize = 253;
const MAX_HOST_LABEL: usize = 63;

/// How to reach a controller: the last route that is not loopback.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct RouteHint {
    /// An IP address or a host name; never loopback.
    pub address: String,
    /// The session port of the controller at `address`.
    pub session_port: u16,
}

impl RouteHint {
    fn to_json(&self) -> Value {
        json!({"address": self.address, "sessionPort": self.session_port})
    }

    /// The hint of a record, or `None` when the value is not one that
    /// [`record_choice`] writes.
    fn from_json(value: &Value) -> Option<Self> {
        let address = value.get("address")?.as_str()?;
        let session_port = u16::try_from(value.get("sessionPort")?.as_u64()?).ok()?;
        (check_address(address).is_ok() && !is_loopback(address) && session_port != 0).then(|| {
            RouteHint {
                address: address.to_owned(),
                session_port,
            }
        })
    }
}

/// The controller last chosen for an application.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct PathRecord {
    /// The id of the controller, 32 lowercase hexadecimal digits.
    pub device_id: String,
    /// The last route that is not loopback, when there was one.
    pub route_hint: Option<RouteHint>,
    /// When the controller was chosen, in milliseconds since the Unix epoch.
    pub chosen_at: u64,
}

impl PathRecord {
    fn to_json(&self) -> Value {
        json!({
            "deviceId": self.device_id,
            "routeHint": self.route_hint.as_ref().map(RouteHint::to_json),
            "chosenAt": self.chosen_at,
        })
    }

    fn from_json(value: &Value) -> Option<Self> {
        let device_id = value.get("deviceId")?.as_str()?;
        DeviceId::parse(device_id).ok()?;
        let route_hint = match value.get("routeHint")? {
            Value::Null => None,
            hint => Some(RouteHint::from_json(hint)?),
        };
        Some(PathRecord {
            device_id: device_id.to_owned(),
            route_hint,
            chosen_at: value.get("chosenAt")?.as_u64()?,
        })
    }
}

/// The outcome of reading a path record.
#[derive(Debug, PartialEq)]
pub enum PathRecordRead {
    /// There is no file: no controller was chosen yet.
    Missing,
    /// The file is not a path record of this version: it is not JSON, has
    /// another version, or has a shape that [`record_choice`] does not write.
    Malformed,
    /// The record.
    Present(PathRecord),
}

/// Reads the path record at `path`. Fails only on a genuine I/O error; see
/// [`records::read`].
pub fn read(path: &Path) -> Result<PathRecordRead, Diagnostic> {
    Ok(match records::read(RecordKind::Path, path)? {
        RecordRead::Missing => PathRecordRead::Missing,
        RecordRead::Malformed => PathRecordRead::Malformed,
        RecordRead::Present(value) => match PathRecord::from_json(&value) {
            Some(record) => PathRecordRead::Present(record),
            None => PathRecordRead::Malformed,
        },
    })
}

/// Checks that `text` is an address of a route: an IP address (IPv6 without
/// brackets) or a host name.
pub fn check_address(text: &str) -> Result<(), String> {
    if text.parse::<IpAddr>().is_ok() || is_host_name(text) {
        Ok(())
    } else {
        Err(format!(
            "'{text}' is not an address: expected an IP address or a host name"
        ))
    }
}

/// A host name: dot-separated labels of letters, digits and `-`, none empty,
/// none starting or ending with `-`. One trailing dot (the root) is allowed.
fn is_host_name(text: &str) -> bool {
    let name = text.strip_suffix('.').unwrap_or(text);
    !name.is_empty()
        && name.len() <= MAX_HOST_NAME
        && name.split('.').all(|label| {
            !label.is_empty()
                && label.len() <= MAX_HOST_LABEL
                && !label.starts_with('-')
                && !label.ends_with('-')
                && label
                    .bytes()
                    .all(|byte| byte.is_ascii_alphanumeric() || byte == b'-')
        })
}

/// Whether `address` means "this PC": an IPv4 address in 127.0.0.0/8, `::1`
/// (also as an IPv4-mapped IPv6 address of 127.0.0.0/8), or the host name
/// `localhost` in any letter case. This is the one rule of what the project path
/// file never holds as a route hint; a text that is not an address is not
/// loopback.
pub fn is_loopback(address: &str) -> bool {
    match address.parse::<IpAddr>() {
        Ok(IpAddr::V4(ip)) => ip.is_loopback(),
        Ok(IpAddr::V6(ip)) => {
            ip == Ipv6Addr::LOCALHOST || ip.to_ipv4_mapped().is_some_and(|v4| v4.is_loopback())
        }
        Err(_) => {
            let name = address.strip_suffix('.').unwrap_or(address);
            name.eq_ignore_ascii_case("localhost")
        }
    }
}

/// Why a path was not recorded.
#[derive(Debug)]
pub enum PathError {
    /// The device id is not 32 lowercase hexadecimal digits.
    BadDeviceId(String),
    /// The address is neither an IP address nor a host name.
    BadAddress(String),
    /// The session port is 0.
    BadPort,
    /// No record can be named: the anchor has no file name.
    NoRecordPath(PathBuf),
    /// The record could not be read or written. Boxed: a diagnostic is large.
    Record(Box<Diagnostic>),
}

impl fmt::Display for PathError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            PathError::BadDeviceId(reason) | PathError::BadAddress(reason) => f.write_str(reason),
            PathError::BadPort => f.write_str("The session port must not be 0"),
            PathError::NoRecordPath(anchor) => write!(
                f,
                "Cannot name a path record for application {}: it needs a file name",
                anchor.display()
            ),
            PathError::Record(problem) => write!(
                f,
                "{}: {}: {}",
                problem.code,
                problem.description(),
                problem.primary.message
            ),
        }
    }
}

/// The path of the path record for the application container `anchor`. The one
/// place the editor asks for it, so the rule that names the file is
/// [`RecordKind::path`] and nowhere else.
pub fn record_path(anchor: &Path) -> Result<PathBuf, PathError> {
    RecordKind::Path
        .path(anchor)
        .ok_or_else(|| PathError::NoRecordPath(anchor.to_path_buf()))
}

fn record_error(problem: Diagnostic) -> PathError {
    PathError::Record(Box::new(problem))
}

/// A controller chosen for an application, with the route that reached it.
pub struct Choice<'a> {
    /// The application container, which the record is named after and sits
    /// beside. It need not exist.
    pub anchor: &'a Path,
    /// The id of the controller, 32 lowercase hexadecimal digits.
    pub device_id: &'a str,
    /// The address that reached the controller.
    pub address: &'a str,
    /// The session port at `address`.
    pub session_port: u16,
}

/// Writes the path record of `choice`, with `chosen_at_ms` as its time.
///
/// This is the one writer of the record. It overwrites the record; nothing is
/// written on an error. When the address is loopback it is not the hint (see
/// the module documentation): the hint stays what the record held for the same
/// controller, else it is `null`. A record that cannot be used (malformed) holds
/// no hint to keep.
pub fn record_choice(choice: &Choice<'_>, chosen_at_ms: u64) -> Result<(), PathError> {
    DeviceId::parse(choice.device_id).map_err(PathError::BadDeviceId)?;
    check_address(choice.address).map_err(PathError::BadAddress)?;
    if choice.session_port == 0 {
        return Err(PathError::BadPort);
    }
    let path = record_path(choice.anchor)?;
    let route_hint = if is_loopback(choice.address) {
        match read(&path).map_err(record_error)? {
            PathRecordRead::Present(previous) if previous.device_id == choice.device_id => {
                previous.route_hint
            }
            _ => None,
        }
    } else {
        Some(RouteHint {
            address: choice.address.to_owned(),
            session_port: choice.session_port,
        })
    };
    let record = PathRecord {
        device_id: choice.device_id.to_owned(),
        route_hint,
        chosen_at: chosen_at_ms,
    };
    records::write(RecordKind::Path, &path, record.to_json()).map_err(record_error)
}

#[cfg(test)]
mod tests;
