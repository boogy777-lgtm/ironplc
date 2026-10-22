//! The path record in the command line: `record-path` writes it and
//! `record-path-path` says where it is.
//!
//! The record is `ironplc_project::path_record`; this module only reads the
//! clock and turns the failures into the command's error string, as the other
//! commands that are not a collection of diagnostics do. The editor calls these
//! commands and reads the file itself, so the format has one writer and the rule
//! that names the file has one place.

use std::path::{Path, PathBuf};
use std::time::{SystemTime, UNIX_EPOCH};

use ironplc_project::path_record::{self, Choice};
use ironplc_project::DeviceId;

/// What `record-path` was given.
pub struct RecordPath<'a> {
    /// The application container, which the record is named after and sits
    /// beside.
    pub application: &'a Path,
    /// The id of the controller that was chosen.
    pub device_id: &'a DeviceId,
    /// The address that reached the controller.
    pub address: &'a str,
    /// The session port at the address.
    pub session_port: u16,
}

/// Writes the path record. Prints nothing; every failure is the error string,
/// and nothing is written then.
pub fn record_path(args: &RecordPath<'_>) -> Result<(), String> {
    let chosen_at_ms = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map_err(|_| {
            "The system clock is before 1970-01-01, so a time cannot be recorded".to_owned()
        })?
        .as_millis();
    path_record::record_choice(
        &Choice {
            anchor: args.application,
            device_id: args.device_id,
            address: args.address,
            session_port: args.session_port,
        },
        u64::try_from(chosen_at_ms).map_err(|_| "The system clock is too far ahead".to_owned())?,
    )
    .map_err(|err| err.to_string())
}

/// The path of the path record for `application`.
pub fn record_path_path(application: &Path) -> Result<PathBuf, String> {
    path_record::record_path(application).map_err(|err| err.to_string())
}

#[cfg(test)]
mod tests;
