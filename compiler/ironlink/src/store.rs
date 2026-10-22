//! IronLink's workstation store: the data IronLink keeps for the user of this
//! PC, in one directory per user.
//!
//! - **Class:** IronLink's workstation data, the facts about THIS workstation
//!   that are independent of any project: the route record ([`routes`]: how this
//!   workstation reaches a device id) and the address list ([`addresses`]: the
//!   persistent targets of the address-list driver). A near case outside it: the
//!   project path file, which is IronPLC's record (ADR-0083); IronLink writes
//!   only its own data and knows no project.
//! - **Inv:** each file has one reader and one writer, [`Store::read`] and
//!   [`Store::write`], and nothing outside this module touches the files. A file
//!   is versioned JSON, written atomically (a temporary file in the same
//!   directory, then a rename) and with sorted keys, so a failed or interrupted
//!   write leaves the old file. A file that cannot be read as a file of this
//!   version is an error (L6006), never an empty store: the next write would
//!   destroy what the user kept. A route is stored only after an identify that
//!   returned its device id (`service`). The store always comes from outside
//!   ([`Store::at`], [`Store::resolve`]): no function of IronLink looks for the
//!   directory by itself, so a test cannot reach the real user directory.
//! - **N+1:** a later setting of this workstation (for example the interfaces
//!   to broadcast from) is one more file of the same store: a struct that
//!   implements [`StoreFile`] and its operations, in a module of its own. The
//!   reading, the writing, the version and the location are not edited.
//!
//! Two processes that change the same file at the same time (an `ironlink serve`
//! and a command line) each read, change and write the whole file: the later
//! write wins, and the earlier change is lost. Each change takes a few
//! microseconds, so this is accepted; no lock file is kept.

pub(crate) mod addresses;
pub(crate) mod routes;

use std::fs;
use std::io::{self, Write};
use std::path::{Path, PathBuf};

use serde::de::DeserializeOwned;
use serde::Serialize;
use serde_json::Value;

use crate::error::IronLinkError;

/// The environment variable that names the directory of the store. The option
/// `--store` wins over it.
pub(crate) const STORE_ENV: &str = "IRONLINK_STORE";

/// The version of the format of every file, written as `"version"` in each.
const FORMAT_VERSION: u64 = 1;

/// A file of the store: the content of one JSON file.
pub(crate) trait StoreFile: Serialize + DeserializeOwned + Default {
    /// The name of the file in the directory of the store.
    const NAME: &'static str;

    /// What the file holds, for the message of an unreadable file.
    const WHAT: &'static str;

    /// Checks the content beyond its shape. The text of the error is why the
    /// content cannot be used.
    fn check(&self) -> Result<(), String> {
        Ok(())
    }
}

/// The operating system, as far as the location of the store depends on it.
/// It is a value so that the rule of every system can be tested on one.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum Platform {
    Windows,
    MacOs,
    /// Linux and the other systems that follow the XDG directories.
    Other,
}

impl Platform {
    /// The system IronLink was built for.
    pub(crate) fn current() -> Self {
        if cfg!(windows) {
            Platform::Windows
        } else if cfg!(target_os = "macos") {
            Platform::MacOs
        } else {
            Platform::Other
        }
    }
}

/// The directory of the store of the user on `platform`, from the environment
/// variables read by `lookup`; `None` when the variables it needs are not set.
/// An empty variable is not set.
pub(crate) fn default_dir(
    platform: Platform,
    lookup: impl Fn(&str) -> Option<String>,
) -> Option<PathBuf> {
    let var = |name: &str| lookup(name).filter(|value| !value.is_empty());
    match platform {
        Platform::Windows => {
            var("APPDATA").map(|appdata| PathBuf::from(appdata).join("IronPLC").join("ironlink"))
        }
        Platform::MacOs => var("HOME").map(|home| {
            PathBuf::from(home)
                .join("Library")
                .join("Application Support")
                .join("IronPLC")
                .join("ironlink")
        }),
        Platform::Other => var("XDG_CONFIG_HOME")
            .map(PathBuf::from)
            .or_else(|| var("HOME").map(|home| PathBuf::from(home).join(".config")))
            .map(|config| config.join("ironplc").join("ironlink")),
    }
}

/// The workstation store: a directory with the files of [`StoreFile`] members.
#[derive(Clone, Debug, PartialEq, Eq)]
pub(crate) struct Store {
    /// `None` when no directory could be determined; the commands that need the
    /// store then fail with L6004, and the others (`version`) do not.
    dir: Option<PathBuf>,
}

impl Store {
    /// The store in `dir`; the tests make their stores with it, the program
    /// finds its store with [`Store::resolve`].
    #[cfg(test)]
    pub(crate) fn at(dir: impl Into<PathBuf>) -> Self {
        Store {
            dir: Some(dir.into()),
        }
    }

    /// The store the user asked for: the option `--store`, else the variable
    /// [`STORE_ENV`], else the directory of the user on `platform`.
    pub(crate) fn resolve(
        option: Option<&Path>,
        platform: Platform,
        lookup: impl Fn(&str) -> Option<String>,
    ) -> Self {
        let dir = option
            .map(Path::to_path_buf)
            .or_else(|| {
                lookup(STORE_ENV)
                    .filter(|value| !value.is_empty())
                    .map(PathBuf::from)
            })
            .or_else(|| default_dir(platform, &lookup));
        Store { dir }
    }

    /// The directory of the store (the tests look at the files in it).
    pub(crate) fn dir(&self) -> Result<&Path, IronLinkError> {
        self.dir.as_deref().ok_or_else(|| {
            IronLinkError::argument_invalid(format!(
                "IronLink cannot tell where to keep its data on this PC; name a directory \
                 with --store or the environment variable {STORE_ENV}"
            ))
        })
    }

    /// The content of the file `F`; empty when the file does not exist.
    fn read<F: StoreFile>(&self) -> Result<F, IronLinkError> {
        let path = self.dir()?.join(F::NAME);
        let text = match fs::read_to_string(&path) {
            Ok(text) => text,
            Err(err) if err.kind() == io::ErrorKind::NotFound => return Ok(F::default()),
            Err(err) => return Err(IronLinkError::store_unreadable(&path, err)),
        };
        let unreadable = |reason: String| {
            IronLinkError::store_unreadable(&path, format!("{} {reason}", F::WHAT))
        };
        let mut value: Value = serde_json::from_str(&text)
            .map_err(|err| unreadable(format!("is not JSON ({err})")))?;
        let Some(object) = value.as_object_mut() else {
            return Err(unreadable("is not a JSON object".to_string()));
        };
        match object.remove("version") {
            Some(version) if version == FORMAT_VERSION => {}
            Some(version) => {
                return Err(unreadable(format!(
                    "has the version {version}, and this IronLink reads the version \
                     {FORMAT_VERSION}"
                )))
            }
            None => return Err(unreadable("has no version".to_string())),
        }
        let file: F = serde_json::from_value(value)
            .map_err(|err| unreadable(format!("does not have the expected shape ({err})")))?;
        file.check().map_err(unreadable)?;
        Ok(file)
    }

    /// Writes `file` as the file `F`: pretty-printed with sorted keys, to a
    /// temporary file in the same directory that is then renamed over the file,
    /// so that the file is always whole. The directory is created on the first
    /// write.
    fn write<F: StoreFile>(&self, file: &F) -> Result<(), IronLinkError> {
        let dir = self.dir()?;
        let path = dir.join(F::NAME);
        let fail = |err: &io::Error| IronLinkError::store_write(&path, err);
        let text = render(file)?;
        fs::create_dir_all(dir).map_err(|err| fail(&err))?;
        let temporary = dir.join(format!(".{}.{}.tmp", F::NAME, std::process::id()));
        let written = write_whole(&temporary, &text).and_then(|()| fs::rename(&temporary, &path));
        written.map_err(|err| {
            // The temporary file is of no use after a failure, and its removal
            // can only fail for the reason the write failed, which is reported.
            drop(fs::remove_file(&temporary));
            fail(&err)
        })
    }
}

/// The text of `file`: JSON with the version and sorted keys.
fn render<F: StoreFile>(file: &F) -> Result<String, IronLinkError> {
    let internal = |err: serde_json::Error| {
        IronLinkError::internal(format!("unable to render {} as JSON: {err}", F::NAME))
    };
    let mut value = serde_json::to_value(file).map_err(internal)?;
    let Some(object) = value.as_object_mut() else {
        return Err(IronLinkError::internal(format!(
            "{} is not a JSON object",
            F::NAME
        )));
    };
    object.insert("version".to_string(), Value::from(FORMAT_VERSION));
    let mut text = serde_json::to_string_pretty(&sorted(value)).map_err(internal)?;
    text.push('\n');
    Ok(text)
}

/// `value` with the keys of every object in order, whatever the order the
/// serializer or the map type gives.
fn sorted(value: Value) -> Value {
    match value {
        Value::Object(map) => {
            let mut entries: Vec<(String, Value)> = map.into_iter().collect();
            entries.sort_by(|a, b| a.0.cmp(&b.0));
            Value::Object(
                entries
                    .into_iter()
                    .map(|(key, value)| (key, sorted(value)))
                    .collect(),
            )
        }
        Value::Array(items) => Value::Array(items.into_iter().map(sorted).collect()),
        other => other,
    }
}

/// Writes `text` to a new file at `path` and makes sure it is on the disk.
fn write_whole(path: &Path, text: &str) -> io::Result<()> {
    let mut file = fs::File::create(path)?;
    file.write_all(text.as_bytes())?;
    file.sync_all()
}

#[cfg(test)]
mod tests;
