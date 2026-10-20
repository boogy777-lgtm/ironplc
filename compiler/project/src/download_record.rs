//! The download record: what a device received at the last successful download
//! over one connection, kept in `<app>.<connection>.download.iplcinfo` beside
//! the application container ([`RecordKind::Download`]).
//!
//! With it the engineering side knows, without asking the device, which
//! application the device was last given, and on connect it compares that with
//! what the device reports it runs (the `contentHash` of the identity), by
//! content and not by generation counters, which restart when the device
//! reboots.
//!
//! ## One writer
//!
//! [`record_download`] is the only function that writes the record. A download
//! path of the editor (an acknowledged assemble, a session started with a
//! container) reaches it through `ironplcc record-download`, so the format has
//! one writer. The record is read by the editor from the file; [`read`] here is
//! the reference reader that the format is tested against.
//!
//! ## Format
//!
//! ```json
//! {
//!   "version": 1,
//!   "connection": "My PLC",
//!   "application": "Main",
//!   "container": { "contentHash": "<hex>", "layoutHash": "<hex>", "debugHash": "<hex>", "size": 1234 },
//!   "device": { "active": 2, "normal": 2, "application": 2 },
//!   "time": "2026-10-20T12:00:00Z"
//! }
//! ```
//!
//! - `connection` is the name of the connection profile, as given. It is part
//!   of the file name in an encoded form ([`RecordKind::path`]); a record whose
//!   `connection` is not the one asked for is another connection's (two names
//!   that differ only in case share a file on a file system that ignores case)
//!   and is read as malformed.
//! - `application` is the name the build record gives the application when the
//!   application has one, else the stem of the container.
//! - `container` is the identity of the container that was sent, read from its
//!   header.
//! - `device` is the generation counters the device reported after the
//!   download; `null` when the caller had none. They describe that session of
//!   the device only: a reboot restarts them.
//! - `time` is RFC 3339, in UTC.

use std::fmt;
use std::path::{Path, PathBuf};

use ironplc_container::integrity::{self, HASH_LEN};
use ironplc_dsl::diagnostic::Diagnostic;
use serde_json::{json, Value};

use crate::build_record::{self, BuildRecordRead, ContainerIdentity};
use crate::records::{self, RecordKind, RecordRead};

/// The generation counters of a device, as it reported them after a download.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct DeviceGenerations {
    /// Generation of the artifact that was executing.
    pub active: u32,
    /// Generation of the normal (committed) artifact.
    pub normal: u32,
    /// Generation of the application manifest.
    pub application: u32,
}

impl DeviceGenerations {
    fn to_json(self) -> Value {
        json!({"active": self.active, "normal": self.normal, "application": self.application})
    }

    fn from_json(value: &Value) -> Option<Self> {
        let counter = |key: &str| u32::try_from(value.get(key)?.as_u64()?).ok();
        Some(DeviceGenerations {
            active: counter("active")?,
            normal: counter("normal")?,
            application: counter("application")?,
        })
    }
}

/// What a device received over one connection.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct DownloadRecord {
    /// The name of the connection profile.
    pub connection: String,
    /// The name of the application.
    pub application: String,
    /// The identity of the container that was sent.
    pub container: ContainerIdentity,
    /// The counters the device reported after the download, when known.
    pub device: Option<DeviceGenerations>,
    /// When the download was recorded, RFC 3339 in UTC.
    pub time: String,
}

impl DownloadRecord {
    fn to_json(&self) -> Value {
        json!({
            "connection": self.connection,
            "application": self.application,
            "container": self.container.to_json(),
            "device": self.device.map(DeviceGenerations::to_json),
            "time": self.time,
        })
    }

    fn from_json(value: &Value) -> Option<Self> {
        let device = match value.get("device")? {
            Value::Null => None,
            device => Some(DeviceGenerations::from_json(device)?),
        };
        Some(DownloadRecord {
            connection: value.get("connection")?.as_str()?.to_owned(),
            application: value.get("application")?.as_str()?.to_owned(),
            container: ContainerIdentity::from_json(value.get("container")?)?,
            device,
            time: value.get("time")?.as_str()?.to_owned(),
        })
    }
}

/// The outcome of reading a download record.
#[derive(Debug, PartialEq)]
pub enum DownloadRecordRead {
    /// There is no file.
    Missing,
    /// The file is not a download record of this version and connection: it is
    /// not JSON, has another version, has a shape that is not the record's, or
    /// names another connection.
    Malformed,
    /// The record.
    Present(Box<DownloadRecord>),
}

/// Reads the download record of `connection` at `path`. Fails only on a
/// genuine I/O error; see [`records::read`].
pub fn read(connection: &str, path: &Path) -> Result<DownloadRecordRead, Diagnostic> {
    Ok(
        match records::read(RecordKind::Download { connection }, path)? {
            RecordRead::Missing => DownloadRecordRead::Missing,
            RecordRead::Malformed => DownloadRecordRead::Malformed,
            RecordRead::Present(value) => match DownloadRecord::from_json(&value) {
                Some(record) if record.connection == connection => {
                    DownloadRecordRead::Present(Box::new(record))
                }
                _ => DownloadRecordRead::Malformed,
            },
        },
    )
}

/// A download to record.
pub struct Download<'a> {
    /// The application container, which the record is named after and sits
    /// beside. It need not exist.
    pub anchor: &'a Path,
    /// The container file that was sent to the device. This is the anchor
    /// itself unless the bytes that were sent are another file (a container
    /// compiled for the occasion).
    pub container: &'a Path,
    /// The name of the connection profile.
    pub connection: &'a str,
    /// The `contentHash` the device reports it runs, when it reports one. When
    /// given, the record is written only if it is the hash of `container`: the
    /// record says what the device received, and a device that runs something
    /// else received something else.
    pub device_content_hash: Option<[u8; HASH_LEN]>,
    /// The counters the device reported after the download, when known.
    pub device: Option<DeviceGenerations>,
}

/// Why a download was not recorded.
#[derive(Debug)]
pub enum DownloadError {
    /// No record can be named: the connection name is empty or the anchor has
    /// no file name.
    NoRecordPath { anchor: PathBuf, connection: String },
    /// The container does not exist or does not start with a valid header.
    NotAContainer(PathBuf),
    /// The device runs another container than the one to record.
    DeviceRunsOther { device: String, container: String },
    /// The record could not be written.
    Record(Diagnostic),
}

impl fmt::Display for DownloadError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            DownloadError::NoRecordPath { anchor, connection } => write!(
                f,
                "Cannot name a download record for connection \"{connection}\" and application {}: the connection name must not be empty and the application needs a file name",
                anchor.display()
            ),
            DownloadError::NotAContainer(path) => write!(
                f,
                "{} does not exist or does not start with a valid container header",
                path.display()
            ),
            DownloadError::DeviceRunsOther { device, container } => write!(
                f,
                "The device reports content hash {device}, but the container has {container}: the device did not receive this container, so no download record is written"
            ),
            DownloadError::Record(problem) => write!(
                f,
                "{}: {}: {}",
                problem.code,
                problem.description(),
                problem.primary.message
            ),
        }
    }
}

/// The path of the download record of `connection` for the application
/// container `anchor`. The one place the editor asks for it, so the rule that
/// names the file is [`RecordKind::path`] and nowhere else.
pub fn record_path(anchor: &Path, connection: &str) -> Result<PathBuf, DownloadError> {
    RecordKind::Download { connection }
        .path(anchor)
        .ok_or_else(|| no_record_path(anchor, connection))
}

fn no_record_path(anchor: &Path, connection: &str) -> DownloadError {
    DownloadError::NoRecordPath {
        anchor: anchor.to_path_buf(),
        connection: connection.to_owned(),
    }
}

/// Writes the download record of `download`, with `unix_seconds` as its time.
///
/// This is the one writer of the record. It overwrites the record of the
/// connection; nothing is written on an error.
pub fn record_download(download: &Download<'_>, unix_seconds: u64) -> Result<(), DownloadError> {
    let path = record_path(download.anchor, download.connection)?;
    // The path has a stem when the anchor has a name; this keeps that fact
    // from being assumed.
    let stem = records::stem(download.anchor)
        .ok_or_else(|| no_record_path(download.anchor, download.connection))?;
    let container = ContainerIdentity::of_file(download.container)
        .ok_or_else(|| DownloadError::NotAContainer(download.container.to_path_buf()))?;
    if let Some(device_hash) = download.device_content_hash {
        if device_hash != container.content_hash {
            return Err(DownloadError::DeviceRunsOther {
                device: integrity::hash_hex(&device_hash).to_string(),
                container: integrity::hash_hex(&container.content_hash).to_string(),
            });
        }
    }
    let record = DownloadRecord {
        connection: download.connection.to_owned(),
        application: application_name(download.anchor, stem),
        container,
        device: download.device,
        time: rfc3339_utc(unix_seconds),
    };
    records::write(
        RecordKind::Download {
            connection: download.connection,
        },
        &path,
        record.to_json(),
    )
    .map_err(DownloadError::Record)
}

/// The name of the application of `anchor`: the one its build record gives, and
/// when there is no usable build record, `stem`. A build record that cannot be
/// used is the build's concern (the next build reports and rewrites it); it
/// does not keep a download from being recorded.
fn application_name(anchor: &Path, stem: &str) -> String {
    RecordKind::Build
        .path(anchor)
        .and_then(|path| match build_record::read(&path) {
            Ok(BuildRecordRead::Present(record)) => Some(record.application),
            _ => None,
        })
        .unwrap_or_else(|| stem.to_owned())
}

/// `unix_seconds` as an RFC 3339 time in UTC, `YYYY-MM-DDTHH:MM:SSZ`.
fn rfc3339_utc(unix_seconds: u64) -> String {
    let days = unix_seconds / 86_400;
    let seconds_of_day = unix_seconds % 86_400;
    // Days since 1970-01-01 to a civil date (H. Hinnant's algorithm), on
    // unsigned numbers: the epoch is shifted to 0000-03-01 so a leap day is the
    // last day of a year.
    let shifted = days + 719_468;
    let era = shifted / 146_097;
    let day_of_era = shifted % 146_097;
    let year_of_era =
        (day_of_era - day_of_era / 1_460 + day_of_era / 36_524 - day_of_era / 146_096) / 365;
    let day_of_year = day_of_era - (365 * year_of_era + year_of_era / 4 - year_of_era / 100);
    let month_index = (5 * day_of_year + 2) / 153;
    let day = day_of_year - (153 * month_index + 2) / 5 + 1;
    let month = if month_index < 10 {
        month_index + 3
    } else {
        month_index - 9
    };
    let year = year_of_era + era * 400 + u64::from(month <= 2);
    format!(
        "{year:04}-{month:02}-{day:02}T{:02}:{:02}:{:02}Z",
        seconds_of_day / 3_600,
        seconds_of_day % 3_600 / 60,
        seconds_of_day % 60
    )
}

#[cfg(test)]
mod tests;
