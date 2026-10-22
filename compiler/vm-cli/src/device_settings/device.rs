//! The device settings file and the [`Device`] that holds the settings of one
//! served process.
//!
//! The file is `<file>.device.json` beside the served container, next to the
//! slot store's files. It is versioned JSON, and the keys are the keys of the
//! rows:
//!
//! ```json
//! {
//!   "version": 1,
//!   "deviceId": "0123456789abcdef0123456789abcdef",
//!   "name": "ironplcvm"
//! }
//! ```
//!
//! **Start.** A missing file is a new device: its settings take their first
//! values (the id from the operating system's random source) and the file is
//! written. A file that lacks a row takes that row's first value and is
//! written complete. A file that is not JSON, has another version, names a
//! setting that does not exist or holds a value its row refuses stops the
//! start (V6015): the device does not guess, because a new id generated over a
//! damaged file would change the device's identity, and make every record
//! keyed by the old id lose the device, without a word. The operator fixes the file, or deletes it to
//! make the device a new one.
//!
//! **Change.** A change is checked as a whole, written as a temporary file and
//! renamed over the settings file, and only then kept in memory: a write that
//! fails (V6016) leaves the file and the running device as they were.

use std::fs;
use std::io::{self, Write};
use std::path::{Path, PathBuf};

use ironplc_runtime::DeviceIdentity;

use super::{os_random, DeviceSetting, DeviceSettings, Entries, Random, SettingsError};
use crate::error::{self, VmError};
use crate::slot_store::suffix;

/// The only version of the file this device reads and writes.
const FILE_VERSION: u64 = 1;

/// The id of the device that [`Device::in_memory`] builds.
#[cfg(test)]
pub(crate) const TEST_DEVICE_ID: &str = "0102030405060708090a0b0c0d0e0f10";

/// The UTF-8 byte order mark that some editors put in front of a file.
const BYTE_ORDER_MARK: &[u8] = b"\xEF\xBB\xBF";

/// The settings of one served process, and the file that keeps them.
pub(crate) struct Device {
    settings: DeviceSettings,
    /// The settings file; `None` is a device whose settings live in memory
    /// only (the tests of the session).
    file: Option<PathBuf>,
}

impl Device {
    /// Opens the settings file beside the served container `served`, creating
    /// it for a new device.
    pub(crate) fn open(served: &Path) -> Result<Device, VmError> {
        Device::open_with(served, &mut os_random)
    }

    /// [`Device::open`] with the random source of the caller.
    pub(super) fn open_with(served: &Path, random: Random) -> Result<Device, VmError> {
        let path = settings_path(served);
        let existing = match fs::read(&path) {
            Ok(bytes) => Some(parse_file(&bytes).map_err(|err| invalid_file(&path, &err))?),
            Err(err) if err.kind() == io::ErrorKind::NotFound => None,
            Err(err) => {
                return Err(VmError::io(
                    error::FILE_OPEN,
                    format!(
                        "Unable to read the device settings file {}: {err}",
                        path.display()
                    ),
                ))
            }
        };
        let entries = existing.clone().unwrap_or_default();
        let settings =
            DeviceSettings::resolve(&entries, random).map_err(|err| invalid_file(&path, &err))?;
        if existing.as_ref() != Some(&settings.entries()) {
            write_file(&path, &settings)?;
            match existing {
                None => log::info!(
                    "created the device settings file {} (device id {})",
                    path.display(),
                    settings.get(DeviceSetting::DeviceId)
                ),
                Some(_) => log::info!(
                    "completed the device settings file {} with the missing settings",
                    path.display()
                ),
            }
        }
        Ok(Device {
            settings,
            file: Some(path),
        })
    }

    /// A device whose settings are not persisted and whose id is
    /// [`TEST_DEVICE_ID`]: the device of a session test.
    #[cfg(test)]
    pub(crate) fn in_memory() -> Device {
        let mut fixed = |buffer: &mut [u8]| {
            for (byte, value) in buffer.iter_mut().zip(1u8..) {
                *byte = value;
            }
            Ok(())
        };
        Device {
            settings: DeviceSettings::resolve(&Entries::new(), &mut fixed).unwrap(),
            file: None,
        }
    }

    /// The device block of the `identity` answer; see
    /// [`DeviceSettings::identity`].
    pub(crate) fn identity(&self) -> DeviceIdentity {
        self.settings.identity()
    }

    /// Changes the settings named in `entries` (the `setDeviceSettings`
    /// command): V6015 when an entry is not acceptable, or names a setting that
    /// is changed in the settings file only, V6016 when the file cannot be
    /// written. Nothing changes on an error.
    pub(crate) fn change(&mut self, entries: &Entries) -> Result<(), VmError> {
        let mut next = self.settings.clone();
        next.apply_online(entries).map_err(|err| {
            VmError::io(
                error::DEVICE_SETTINGS_INVALID,
                format!("the device settings are not changed: {err}"),
            )
        })?;
        if next == self.settings {
            return Ok(());
        }
        if let Some(path) = &self.file {
            write_file(path, &next)?;
        }
        self.settings = next;
        Ok(())
    }
}

/// The settings file beside the served container.
pub(super) fn settings_path(served: &Path) -> PathBuf {
    suffix(served, "device.json")
}

/// Reads the entries of a settings file, after checking its version.
fn parse_file(bytes: &[u8]) -> Result<Entries, SettingsError> {
    let json = bytes.strip_prefix(BYTE_ORDER_MARK).unwrap_or(bytes);
    let mut entries: Entries = serde_json::from_slice(json).map_err(|err| {
        SettingsError::new(
            None,
            format!("the content is not a JSON object with a version and the settings ({err})"),
        )
    })?;
    match entries.remove("version") {
        Some(version) if version.as_u64() == Some(FILE_VERSION) => Ok(entries),
        Some(_) => Err(SettingsError::new(
            Some("version"),
            format!("must be {FILE_VERSION}, the only version of the file this device reads"),
        )),
        None => Err(SettingsError::new(Some("version"), "is missing")),
    }
}

/// The V6015 error for a settings file that cannot be used.
fn invalid_file(path: &Path, err: &SettingsError) -> VmError {
    VmError::io(
        error::DEVICE_SETTINGS_INVALID,
        format!(
            "the device settings file {} is not valid: {err}. Correct the file, or delete it to make this a new device with a new device id",
            path.display()
        ),
    )
}

/// The text of the settings file: the version, then one line for each row.
fn render(settings: &DeviceSettings) -> String {
    let mut text = format!("{{\n  \"version\": {FILE_VERSION}");
    for setting in DeviceSetting::ALL {
        let key = serde_json::Value::String(setting.key().to_owned());
        let value = serde_json::Value::String(settings.get(*setting).to_owned());
        text.push_str(&format!(",\n  {key}: {value}"));
    }
    text.push_str("\n}\n");
    text
}

/// Writes the settings file as a temporary file renamed over it, so that the
/// file is always the old content or the new, never a part of either. (Rust's
/// `rename` replaces an existing file on Windows too.)
fn write_file(path: &Path, settings: &DeviceSettings) -> Result<(), VmError> {
    let tmp = suffix(path, "tmp");
    let fail = |action: &str, err: io::Error| {
        VmError::io(
            error::DEVICE_SETTINGS_WRITE,
            format!(
                "Unable to {action} the device settings file {}: {err}",
                path.display()
            ),
        )
    };
    let mut file = fs::File::create(&tmp).map_err(|err| fail("create", err))?;
    file.write_all(render(settings).as_bytes())
        .map_err(|err| fail("write", err))?;
    file.sync_all().map_err(|err| fail("flush", err))?;
    drop(file);
    fs::rename(&tmp, path).map_err(|err| {
        if let Err(remove) = fs::remove_file(&tmp) {
            log::warn!("unable to remove {}: {remove}", tmp.display());
        }
        fail("replace", err)
    })
}
