//! The device settings: the values a device owns, persists and lets its
//! operator change: in the settings file, and, for the rows that allow it, by
//! the `setDeviceSettings` session command.
//!
//! A setting is one row: its key, where its first value comes from (a fixed
//! text or 128 random bits), where it may be changed ([`Changeable`]) and the
//! check every new value passes. The rows are
//! listed once, in the `settings!` table below, and the enum, the list of all
//! rows and the exhaustive match from a setting to its row are derived from
//! that list, so a row cannot be forgotten by any user of the table. The file
//! reader ([`DeviceSettings::resolve`]), the online command
//! ([`DeviceSettings::apply_online`]) and the identity
//! ([`DeviceSettings::identity`]) all go through the rows, which is why a new
//! setting is one row, its check and its line in the reference documentation,
//! and nothing else.
//!
//! The values are text. A setting that is not text (a port, a switch) adds a
//! type to the row when the first one arrives; nothing here knows where the
//! device runs or what stores the values: [`device::Device`] persists them in
//! the file beside the served container, and a hardware target would persist
//! them in its own way and reuse the rows.
//!
//! A class apart from the application's slots ([`crate::slot_store`]): a slot
//! holds what the engineer sent, a setting holds what the device is. A device
//! reset removes the settings file with the rest of the device's files, and the
//! next start generates a new id; a restart keeps it.

use std::collections::BTreeMap;
use std::fmt;

use ironplc_runtime::DeviceIdentity;
use serde_json::Value;

pub(crate) mod device;
#[cfg(test)]
mod device_tests;
#[cfg(test)]
mod tests;

/// The settings as the file and the `setDeviceSettings` command carry them:
/// the key of a row, and the JSON value written for it.
pub(crate) type Entries = BTreeMap<String, Value>;

/// Where the first value of a setting comes from.
enum Initial {
    /// The same text on every device.
    Fixed(&'static str),
    /// 128 bits from the operating system's random source, as 32 lowercase
    /// hexadecimal digits: the device's own, and different on every device.
    Random128Hex,
}

/// Where a setting may be changed.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum Changeable {
    /// In the settings file, and over the engineering session with the
    /// `setDeviceSettings` command.
    OnlineAndFile,
    /// In the settings file only, on the device itself. A setting that the
    /// engineering session relies on, such as the recognition of the device,
    /// is `FileOnly`: whoever can change it over the session could give the
    /// device the identity of another one.
    FileOnly,
}

/// One row of the table of settings.
struct Row {
    /// The key in the settings file and in the `setDeviceSettings` command.
    key: &'static str,
    /// The value a device without the setting starts with.
    initial: Initial,
    /// Where a new value may be given.
    changeable: Changeable,
    /// Whether a new value is acceptable; the reason when it is not.
    check: fn(&str) -> Result<(), &'static str>,
}

/// The row of the `deviceId` setting: how a workstation recognises this device
/// again. Random, so that two devices never share one, and changeable on the
/// device only (its settings file), so that an operator can give a replacement
/// device the id of the one it replaces, and nothing can change the id over the
/// session.
const DEVICE_ID: Row = Row {
    key: "deviceId",
    initial: Initial::Random128Hex,
    changeable: Changeable::FileOnly,
    check: check_device_id,
};

/// The row of the `name` setting: the label the device presents.
const NAME: Row = Row {
    key: "name",
    initial: Initial::Fixed("ironplcvm"),
    changeable: Changeable::OnlineAndFile,
    check: check_name,
};

/// The longest device name, in characters.
const NAME_MAX_CHARS: usize = 64;

/// The number of random bytes behind a device id.
const DEVICE_ID_BYTES: usize = 16;

fn check_device_id(value: &str) -> Result<(), &'static str> {
    device_id_bytes(value)
        .map(|_| ())
        .ok_or("must be 32 lowercase hexadecimal digits")
}

/// The 16 bytes of a device id; `None` unless `id` is exactly 32 lowercase
/// hexadecimal digits. The one definition of a well-formed id: the row's check
/// is this function, and the discovery reply carries these bytes, so an id that
/// is accepted is an id that can be sent.
pub(crate) fn device_id_bytes(id: &str) -> Option<[u8; DEVICE_ID_BYTES]> {
    if id.len() != DEVICE_ID_BYTES * 2 {
        return None;
    }
    let mut bytes = [0u8; DEVICE_ID_BYTES];
    // The length is checked, so there are 16 pairs and no remainder.
    let (pairs, _) = id.as_bytes().as_chunks::<2>();
    for (byte, [high, low]) in bytes.iter_mut().zip(pairs.iter().copied()) {
        *byte = (hex_digit(high)? << 4) | hex_digit(low)?;
    }
    Some(bytes)
}

/// The value of one lowercase hexadecimal digit.
fn hex_digit(digit: u8) -> Option<u8> {
    match digit {
        b'0'..=b'9' => Some(digit - b'0'),
        b'a'..=b'f' => Some(digit - b'a' + 10),
        _ => None,
    }
}

/// The 32 lowercase hexadecimal digits of a device id: the inverse of
/// [`device_id_bytes`].
fn device_id_hex(bytes: &[u8; DEVICE_ID_BYTES]) -> String {
    bytes.iter().map(|byte| format!("{byte:02x}")).collect()
}

fn check_name(value: &str) -> Result<(), &'static str> {
    if value.trim().is_empty() {
        Err("must not be empty")
    } else if value.chars().count() > NAME_MAX_CHARS {
        Err("must have at most 64 characters")
    } else if value.chars().any(char::is_control) {
        Err("must not contain control characters")
    } else {
        Ok(())
    }
}

/// Declares the table of settings: the enum, the list of every setting and the
/// match from a setting to its row. Adding a setting is one line here.
macro_rules! settings {
    ($($variant:ident => $row:expr,)+) => {
        /// One setting of the device.
        #[derive(Clone, Copy, Debug, PartialEq, Eq)]
        pub(crate) enum DeviceSetting {
            $($variant,)+
        }

        impl DeviceSetting {
            /// Every setting, in the order the file lists them.
            pub(crate) const ALL: &'static [DeviceSetting] = &[$(DeviceSetting::$variant,)+];

            fn row(self) -> &'static Row {
                match self {
                    $(DeviceSetting::$variant => &$row,)+
                }
            }
        }

        /// The number of settings.
        const COUNT: usize = DeviceSetting::ALL.len();
    };
}

settings! {
    DeviceId => DEVICE_ID,
    Name => NAME,
}

impl DeviceSetting {
    /// The key of the setting in the file and in the command.
    pub(crate) fn key(self) -> &'static str {
        self.row().key
    }

    /// Where the setting may be changed.
    pub(crate) fn changeable(self) -> Changeable {
        self.row().changeable
    }

    /// The setting with `key`, if there is one.
    fn from_key(key: &str) -> Option<DeviceSetting> {
        DeviceSetting::ALL
            .iter()
            .copied()
            .find(|setting| setting.key() == key)
    }
}

/// Why a set of entries is not acceptable as settings. The offending key is
/// named, so that the message says which line of the file, or which field of
/// the command, to fix.
#[derive(Debug, PartialEq, Eq)]
pub(crate) struct SettingsError {
    /// The key as written, when the fault is in one entry.
    key: Option<String>,
    reason: String,
}

impl SettingsError {
    pub(crate) fn new(key: Option<&str>, reason: impl Into<String>) -> Self {
        SettingsError {
            key: key.map(str::to_owned),
            reason: reason.into(),
        }
    }
}

impl fmt::Display for SettingsError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match &self.key {
            Some(key) => write!(f, "the field \"{key}\" {}", self.reason),
            None => f.write_str(&self.reason),
        }
    }
}

/// The source of the random bytes of a generated setting. The device passes
/// the operating system's ([`os_random`]); a test passes a fixed sequence.
pub(crate) type Random<'a> = &'a mut dyn FnMut(&mut [u8]) -> Result<(), String>;

/// Fills `buffer` from the operating system's random source.
pub(crate) fn os_random(buffer: &mut [u8]) -> Result<(), String> {
    getrandom::fill(buffer).map_err(|err| err.to_string())
}

/// The values of every setting of one device. A value exists for every row,
/// and has passed the row's check.
#[derive(Clone, Debug, PartialEq, Eq)]
pub(crate) struct DeviceSettings {
    values: [String; COUNT],
}

impl DeviceSettings {
    /// The value of `setting`.
    pub(crate) fn get(&self, setting: DeviceSetting) -> &str {
        &self.values[setting as usize]
    }

    /// Builds the settings from `entries`, the content of a settings file.
    /// A row without an entry takes its first value; an entry of no row, a
    /// value that is not text and a value its row refuses are errors. Nothing
    /// is repaired: an invalid entry is the operator's to fix.
    pub(crate) fn resolve(entries: &Entries, random: Random) -> Result<Self, SettingsError> {
        let checked = check_entries(entries)?;
        let mut values = Vec::with_capacity(COUNT);
        for setting in DeviceSetting::ALL {
            let value = match checked.get(setting.key()) {
                Some(value) => value.clone(),
                None => first_value(*setting, random)?,
            };
            values.push(value);
        }
        // The loop pushes one value for each of the COUNT rows.
        let values = <[String; COUNT]>::try_from(values).map_err(|_| {
            SettingsError::new(
                None,
                "the table of settings produced a wrong number of values",
            )
        })?;
        Ok(DeviceSettings { values })
    }

    /// Changes the settings named in `entries` and leaves the others: the
    /// online door, the `setDeviceSettings` command. All or nothing: when any
    /// entry names a setting that is [`Changeable::FileOnly`], or is not
    /// acceptable, the settings stay as they were.
    pub(crate) fn apply_online(&mut self, entries: &Entries) -> Result<(), SettingsError> {
        refuse_file_only(entries)?;
        let checked = check_entries(entries)?;
        for (key, value) in checked {
            if let Some(setting) = DeviceSetting::from_key(key) {
                self.values[setting as usize] = value;
            }
        }
        Ok(())
    }

    /// The settings in the form the file holds, one entry for each row.
    pub(crate) fn entries(&self) -> Entries {
        DeviceSetting::ALL
            .iter()
            .map(|setting| {
                (
                    setting.key().to_owned(),
                    Value::String(self.get(*setting).to_owned()),
                )
            })
            .collect()
    }

    /// The device block of the `identity` answer, and of the answer to a
    /// discovery request: the one place a [`DeviceIdentity`] is built, from the
    /// settings and the constants of this build. A caller never fills a field
    /// itself, so the two answers cannot disagree.
    pub(crate) fn identity(&self) -> DeviceIdentity {
        DeviceIdentity {
            device_id: Some(self.get(DeviceSetting::DeviceId).to_owned()),
            name: self.get(DeviceSetting::Name).to_owned(),
            model: "IronPLC SoftPLC".into(),
            modification: "vm-cli".into(),
            runtime_version: env!("CARGO_PKG_VERSION").to_string(),
        }
    }
}

/// Refuses the first entry whose row cannot be changed online. Read from the
/// row, so a new file-only setting is refused without an edit here.
fn refuse_file_only(entries: &Entries) -> Result<(), SettingsError> {
    for key in entries.keys() {
        let file_only = DeviceSetting::from_key(key)
            .is_some_and(|setting| setting.changeable() == Changeable::FileOnly);
        if file_only {
            return Err(SettingsError::new(
                Some(key),
                "can only be changed on the device itself (its settings file)",
            ));
        }
    }
    Ok(())
}

/// Checks every entry against its row and returns the accepted values by key.
fn check_entries(entries: &Entries) -> Result<BTreeMap<&str, String>, SettingsError> {
    let mut checked = BTreeMap::new();
    for (key, value) in entries {
        let setting = DeviceSetting::from_key(key)
            .ok_or_else(|| SettingsError::new(Some(key), "is not a device setting"))?;
        let text = value
            .as_str()
            .ok_or_else(|| SettingsError::new(Some(key), "must be text"))?;
        (setting.row().check)(text).map_err(|reason| SettingsError::new(Some(key), reason))?;
        checked.insert(key.as_str(), text.to_owned());
    }
    Ok(checked)
}

/// The first value of `setting`: its fixed text, or fresh random bits.
fn first_value(setting: DeviceSetting, random: Random) -> Result<String, SettingsError> {
    match setting.row().initial {
        Initial::Fixed(text) => Ok(text.to_owned()),
        Initial::Random128Hex => {
            let mut bytes = [0u8; DEVICE_ID_BYTES];
            random(&mut bytes).map_err(|err| {
                SettingsError::new(
                    Some(setting.key()),
                    format!("could not be generated, the random source failed: {err}"),
                )
            })?;
            Ok(device_id_hex(&bytes))
        }
    }
}
