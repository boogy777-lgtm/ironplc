//! Engineering record files: the files the engineering side writes beside an
//! application and reads back. The device never reads one; its own slot files
//! are a different family and live with the device.
//!
//! Every record is versioned JSON. A kind added later is one [`RecordKind`]
//! variant with its suffix and version; both matches are exhaustive, so a kind
//! without them does not compile.
//!
//! [`read`] is fail-soft: a missing file is [`RecordRead::Missing`], a file
//! that is not JSON or has another `version` is [`RecordRead::Malformed`], and
//! only a genuine I/O failure is a diagnostic. [`write`] stamps the kind's
//! version into the body. Keys serialize in `serde_json::Map` order (sorted;
//! `preserve_order` is not enabled), so equal bodies give identical bytes.

use std::fs;
use std::path::{Path, PathBuf};

use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_problems::text;
use serde_json::Value;

use ironplc_discovery::DeviceId;

/// The kinds of engineering record file. A kind may borrow what names its
/// records (the device of a download record), hence the lifetime; it stays
/// `Copy`.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum RecordKind<'a> {
    /// The stable variable UID table (ADR 0053), `<stem>.uids.json`.
    Uids,
    /// What the last successful build produced and from which inputs,
    /// `<stem>.build.iplcinfo`. The anchor is the output path of the build, so
    /// the record sits beside the container it describes.
    Build,
    /// What this project last put on one device,
    /// `<stem>.<deviceId>.download.iplcinfo`. The anchor is the application
    /// container; the record is keyed by the id of the device, which every
    /// device generates once and keeps.
    Download {
        /// The id of the device. Being 32 lowercase hexadecimal digits, it is
        /// a valid part of a file name on every system and cannot write the
        /// suffix of another kind, so it is used as it is.
        device_id: &'a DeviceId,
    },
    /// The controller last chosen for an application and the route last used
    /// to reach it, `<stem>.path.iplcinfo`. The anchor is the application
    /// container. There is one per application, because a project with
    /// several controllers has several applications; it is a remembered
    /// convenience, not a binding of the application to a device (ADR 0084).
    Path,
}

impl RecordKind<'_> {
    /// The path of this kind's record for `anchor`.
    ///
    /// The anchor is the path the record belongs to: for [`RecordKind::Uids`]
    /// it is the project path argument, for [`RecordKind::Build`] the output
    /// path of the build. The record sits beside the anchor and
    /// is named `<stem>.<suffix>`, where the stem is the directory name when
    /// the anchor names a directory and the file stem otherwise. Every reader
    /// and writer of a kind resolves its path through here, so compiling a
    /// project and syncing its record address the same file. Returns `None`
    /// when the anchor has no usable name (for example a root directory).
    pub fn path(self, anchor: &Path) -> Option<PathBuf> {
        let stem = stem(anchor)?;
        Some(anchor.with_file_name(format!("{stem}.{}", self.suffix())))
    }

    /// The format version of this kind, written into every record of the kind.
    /// A reader accepts only this version.
    pub fn version(self) -> u64 {
        match self {
            RecordKind::Uids => 1,
            RecordKind::Build => 1,
            RecordKind::Download { .. } => 2,
            RecordKind::Path => 1,
        }
    }

    /// The file name suffix after the stem.
    fn suffix(self) -> String {
        match self {
            RecordKind::Uids => "uids.json".to_owned(),
            RecordKind::Build => "build.iplcinfo".to_owned(),
            RecordKind::Path => "path.iplcinfo".to_owned(),
            RecordKind::Download { device_id } => format!("{device_id}.download.iplcinfo"),
        }
    }
}

/// The stem a record of `anchor` is named after: the directory name when the
/// anchor names a directory, the file stem otherwise. `None` when the anchor
/// has no usable name (for example a root directory).
pub(crate) fn stem(anchor: &Path) -> Option<&str> {
    if anchor.is_dir() {
        anchor.file_name()?.to_str()
    } else {
        anchor.file_stem()?.to_str()
    }
}

/// The outcome of reading a record that could be read from the file system.
#[derive(Debug, PartialEq)]
pub enum RecordRead {
    /// There is no file.
    Missing,
    /// The file is not JSON, has no integer `version`, or has another version.
    Malformed,
    /// The whole JSON value of a file with the kind's version, `version`
    /// field included.
    Present(Value),
}

/// Reads the record of `kind` at `path`.
///
/// Fails only on a genuine I/O error; see the module documentation for the
/// other outcomes.
pub fn read(kind: RecordKind<'_>, path: &Path) -> Result<RecordRead, Diagnostic> {
    let text = match fs::read_to_string(path) {
        Ok(text) => text,
        Err(err) if err.kind() == std::io::ErrorKind::NotFound => return Ok(RecordRead::Missing),
        Err(err) => {
            return Err(Diagnostic::text_in_file(
                FileId::from_path(path),
                text::cannot_read_file_with_path(path.display(), err),
            ))
        }
    };
    let Ok(value) = serde_json::from_str::<Value>(&text) else {
        return Ok(RecordRead::Malformed);
    };
    match value.get("version").and_then(Value::as_u64) {
        Some(version) if version == kind.version() => Ok(RecordRead::Present(value)),
        _ => Ok(RecordRead::Malformed),
    }
}

/// Writes the record of `kind` to `path`, overwriting any existing file.
///
/// `body` must be a JSON object; its `version` field is set to
/// [`RecordKind::version`]. A body that is not an object is a defect of the
/// caller and is reported as an internal error.
pub fn write(kind: RecordKind<'_>, path: &Path, mut body: Value) -> Result<(), Diagnostic> {
    let Some(object) = body.as_object_mut() else {
        return Err(Diagnostic::internal_error());
    };
    object.insert("version".to_owned(), Value::from(kind.version()));
    // Serializing a `Value` cannot fail; the error arm guards the type system.
    let text = serde_json::to_string_pretty(&body).map_err(|_| Diagnostic::internal_error())?;
    fs::write(path, text).map_err(|err| {
        Diagnostic::text_in_file(
            FileId::from_path(path),
            text::cannot_write_file(path.display(), err),
        )
    })
}

#[cfg(test)]
mod tests {
    use super::*;
    use ironplc_problems::Problem;
    use rstest::rstest;
    use serde_json::json;
    use tempfile::TempDir;

    const DEVICE: &str = "0123456789abcdef0123456789abcdef";

    static DEVICE_ID: std::sync::LazyLock<DeviceId> =
        std::sync::LazyLock::new(|| DeviceId::parse(DEVICE).unwrap());

    fn device_id() -> DeviceId {
        *DEVICE_ID
    }

    /// Writes `text` to a fresh temporary file and reads it as a `Uids` record.
    fn read_text(text: &str) -> RecordRead {
        read_kind_text(RecordKind::Uids, text)
    }

    /// Writes `text` to a fresh temporary file and reads it as a record of `kind`.
    fn read_kind_text(kind: RecordKind<'_>, text: &str) -> RecordRead {
        let dir = TempDir::new().unwrap();
        let path = dir.path().join(format!("test.{}", kind.suffix()));
        fs::write(&path, text).unwrap();
        read(kind, &path).unwrap()
    }

    #[rstest]
    #[case::file_with_extension("proj.plcproj", "proj.uids.json")]
    #[case::source_file("main.st", "main.uids.json")]
    fn path_when_file_then_sibling(#[case] file: &str, #[case] expected: &str) {
        let path = RecordKind::Uids
            .path(Path::new("some").join("dir").join(file).as_path())
            .unwrap();

        assert_eq!(path, Path::new("some").join("dir").join(expected));
    }

    #[rstest]
    #[case::output_file("app.iplc", "app.build.iplcinfo")]
    #[case::dotted_name("My.App.iplc", "My.App.build.iplcinfo")]
    fn path_when_build_record_of_output_then_sibling_with_stem(
        #[case] output: &str,
        #[case] expected: &str,
    ) {
        let path = RecordKind::Build
            .path(Path::new("some").join("dir").join(output).as_path())
            .unwrap();

        assert_eq!(path, Path::new("some").join("dir").join(expected));
    }

    #[rstest]
    #[case::file("app.iplc", "app.0123456789abcdef0123456789abcdef.download.iplcinfo")]
    #[case::dotted_name(
        "My.App.iplc",
        "My.App.0123456789abcdef0123456789abcdef.download.iplcinfo"
    )]
    fn path_when_download_of_device_then_sibling_named_by_the_device_id(
        #[case] application: &str,
        #[case] expected: &str,
    ) {
        let id = device_id();

        let path = RecordKind::Download { device_id: &id }
            .path(Path::new("some").join("dir").join(application).as_path())
            .unwrap();

        assert_eq!(path, Path::new("some").join("dir").join(expected));
    }

    #[test]
    fn path_when_download_of_different_devices_then_different_files() {
        let anchor = Path::new("app.iplc");
        let ids: Vec<DeviceId> = [
            "0123456789abcdef0123456789abcdef",
            "fedcba9876543210fedcba9876543210",
            "00000000000000000000000000000000",
        ]
        .iter()
        .map(|id| DeviceId::parse(id).unwrap())
        .collect();

        let paths: std::collections::BTreeSet<_> = ids
            .iter()
            .map(|device_id| RecordKind::Download { device_id }.path(anchor).unwrap())
            .collect();

        assert_eq!(paths.len(), ids.len());
    }

    #[rstest]
    #[case::file("app.iplc", "app.path.iplcinfo")]
    #[case::dotted_name("My.App.iplc", "My.App.path.iplcinfo")]
    fn path_when_path_record_of_application_then_sibling_with_stem(
        #[case] application: &str,
        #[case] expected: &str,
    ) {
        let path = RecordKind::Path
            .path(Path::new("some").join("dir").join(application).as_path())
            .unwrap();

        assert_eq!(path, Path::new("some").join("dir").join(expected));
    }

    #[test]
    fn path_when_path_record_then_never_the_path_of_another_kind() {
        let anchor = Path::new("app.iplc");
        let path_record = RecordKind::Path.path(anchor).unwrap();

        assert_ne!(path_record, RecordKind::Uids.path(anchor).unwrap());
        assert_ne!(path_record, RecordKind::Build.path(anchor).unwrap());
        // The id is hexadecimal, so no device makes `app.path.download.iplcinfo`
        // or any other name that is another kind's.
        let id = device_id();
        assert_ne!(
            path_record,
            RecordKind::Download { device_id: &id }
                .path(anchor)
                .unwrap()
        );
    }

    #[test]
    fn path_when_download_then_never_the_path_of_another_kind() {
        let anchor = Path::new("app.iplc");
        let build = RecordKind::Build.path(anchor).unwrap();
        let uids = RecordKind::Uids.path(anchor).unwrap();
        let path_record = RecordKind::Path.path(anchor).unwrap();

        let id = device_id();

        let download = RecordKind::Download { device_id: &id }
            .path(anchor)
            .unwrap();

        assert_ne!(download, build);
        assert_ne!(download, uids);
        assert_ne!(download, path_record);
    }

    #[test]
    fn path_when_directory_then_directory_name_stem() {
        let dir = TempDir::new().unwrap();
        let project = dir.path().join("My.Project");
        fs::create_dir(&project).unwrap();

        let path = RecordKind::Uids.path(&project).unwrap();

        assert_eq!(path, dir.path().join("My.Project.uids.json"));
    }

    #[test]
    fn path_when_root_then_none() {
        let root = Path::new(std::path::MAIN_SEPARATOR_STR);

        assert_eq!(RecordKind::Uids.path(root), None);
    }

    #[rstest]
    fn read_when_file_missing_then_missing(
        #[values(
            RecordKind::Uids,
            RecordKind::Build,
            RecordKind::Path,
            RecordKind::Download { device_id: &DEVICE_ID }
        )]
        kind: RecordKind<'_>,
    ) {
        let dir = TempDir::new().unwrap();

        let result = read(kind, &dir.path().join("missing.record")).unwrap();

        assert_eq!(result, RecordRead::Missing);
    }

    #[rstest]
    fn read_when_invalid_then_malformed(
        #[values(
            RecordKind::Uids,
            RecordKind::Build,
            RecordKind::Path,
            RecordKind::Download { device_id: &DEVICE_ID }
        )]
        kind: RecordKind<'_>,
        #[values(
            "this is not json",
            r#"{"version": 3}"#,
            r#"{"variables": []}"#,
            r#"{"version": "1"}"#,
            "[1]"
        )]
        text: &str,
    ) {
        assert_eq!(read_kind_text(kind, text), RecordRead::Malformed);
    }

    #[test]
    fn read_when_version_matches_then_present_with_whole_value() {
        let result = read_text(r#"{"version": 1, "extra": [true]}"#);

        assert_eq!(
            result,
            RecordRead::Present(json!({"version": 1, "extra": [true]}))
        );
    }

    #[test]
    fn read_when_path_is_directory_then_diagnostic() {
        let dir = TempDir::new().unwrap();

        let err = read(RecordKind::Uids, dir.path()).unwrap_err();

        assert_eq!(err.code, Problem::CannotReadFile.code());
    }

    #[test]
    fn write_then_read_round_trips_body_with_version() {
        let dir = TempDir::new().unwrap();
        let path = dir.path().join("test.uids.json");

        write(RecordKind::Uids, &path, json!({"a": [1, 2]})).unwrap();

        assert_eq!(
            read(RecordKind::Uids, &path).unwrap(),
            RecordRead::Present(json!({"a": [1, 2], "version": 1}))
        );
    }

    #[test]
    fn write_when_uids_then_bytes_are_sorted_pretty_json() {
        let dir = TempDir::new().unwrap();
        let path = dir.path().join("test.uids.json");

        write(
            RecordKind::Uids,
            &path,
            json!({"variables": [{"scope": "main", "name": "x", "uid": 1}]}),
        )
        .unwrap();

        let expected = "{\n  \"variables\": [\n    {\n      \"name\": \"x\",\n      \
                        \"scope\": \"main\",\n      \"uid\": 1\n    }\n  ],\n  \
                        \"version\": 1\n}";
        assert_eq!(fs::read_to_string(&path).unwrap(), expected);
    }

    #[test]
    fn write_when_body_not_object_then_internal_error() {
        let dir = TempDir::new().unwrap();

        let result = write(
            RecordKind::Uids,
            &dir.path().join("x.uids.json"),
            json!([1]),
        );

        assert!(result.is_err());
    }

    #[test]
    fn write_when_path_unwritable_then_diagnostic() {
        let dir = TempDir::new().unwrap();

        let err = write(RecordKind::Uids, dir.path(), json!({})).unwrap_err();

        assert_eq!(err.code, Problem::CannotWriteFile.code());
    }
}
