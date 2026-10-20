//! The download record in the command line: `record-download` writes it and
//! `record-download-path` says where it is.
//!
//! The record is `ironplc_project::download_record`; this module only reads the
//! clock and turns the failures into the command's error string, as the other
//! commands that are not a collection of diagnostics do. The editor calls these
//! commands, so the format has one writer and the rule that names the file has
//! one place.

use std::path::{Path, PathBuf};
use std::time::{SystemTime, UNIX_EPOCH};

use ironplc_container::integrity::HASH_LEN;
use ironplc_project::download_record::{self, DeviceGenerations, Download};

/// What `record-download` was given.
pub struct RecordDownload<'a> {
    /// The application container, which the record is named after and sits
    /// beside.
    pub application: &'a Path,
    /// The container file that was sent, when it is not the application.
    pub container: Option<&'a Path>,
    /// The name of the connection profile.
    pub connection: &'a str,
    /// The content hash the device reports, when it reports one.
    pub device_content_hash: Option<[u8; HASH_LEN]>,
    /// The counters the device reported after the download, when known.
    pub device: Option<DeviceGenerations>,
}

/// Writes the download record. Prints nothing; every failure is the error
/// string, and nothing is written then.
pub fn record_download(args: &RecordDownload<'_>) -> Result<(), String> {
    let unix_seconds = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map_err(|_| {
            "The system clock is before 1970-01-01, so a time cannot be recorded".to_owned()
        })?
        .as_secs();
    download_record::record_download(
        &Download {
            anchor: args.application,
            container: args.container.unwrap_or(args.application),
            connection: args.connection,
            device_content_hash: args.device_content_hash,
            device: args.device,
        },
        unix_seconds,
    )
    .map_err(|err| err.to_string())
}

/// The path of the download record of `connection` for `application`.
pub fn record_download_path(application: &Path, connection: &str) -> Result<PathBuf, String> {
    download_record::record_path(application, connection).map_err(|err| err.to_string())
}

#[cfg(test)]
mod tests;
