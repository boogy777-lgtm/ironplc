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

/// The kinds of engineering record file.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum RecordKind {
    /// The stable variable UID table (ADR 0053), `<stem>.uids.json`.
    Uids,
}

impl RecordKind {
    /// The path of this kind's record for `anchor`.
    ///
    /// The anchor is the path the record belongs to: for [`RecordKind::Uids`]
    /// it is the project path argument. The record sits beside the anchor and
    /// is named `<stem>.<suffix>`, where the stem is the directory name when
    /// the anchor names a directory and the file stem otherwise. Every reader
    /// and writer of a kind resolves its path through here, so compiling a
    /// project and syncing its record address the same file. Returns `None`
    /// when the anchor has no usable name (for example a root directory).
    pub fn path(self, anchor: &Path) -> Option<PathBuf> {
        let stem = if anchor.is_dir() {
            anchor.file_name()?.to_str()?
        } else {
            anchor.file_stem()?.to_str()?
        };
        Some(anchor.with_file_name(format!("{stem}.{}", self.suffix())))
    }

    /// The format version of this kind, written into every record of the kind.
    /// A reader accepts only this version.
    pub fn version(self) -> u64 {
        match self {
            RecordKind::Uids => 1,
        }
    }

    /// The file name suffix after the stem.
    fn suffix(self) -> &'static str {
        match self {
            RecordKind::Uids => "uids.json",
        }
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
pub fn read(kind: RecordKind, path: &Path) -> Result<RecordRead, Diagnostic> {
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
pub fn write(kind: RecordKind, path: &Path, mut body: Value) -> Result<(), Diagnostic> {
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

    /// Writes `text` to a fresh temporary file and reads it as a `Uids` record.
    fn read_text(text: &str) -> RecordRead {
        let dir = TempDir::new().unwrap();
        let path = dir.path().join("test.uids.json");
        fs::write(&path, text).unwrap();
        read(RecordKind::Uids, &path).unwrap()
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

    #[test]
    fn read_when_file_missing_then_missing() {
        let dir = TempDir::new().unwrap();

        let result = read(RecordKind::Uids, &dir.path().join("missing.uids.json")).unwrap();

        assert_eq!(result, RecordRead::Missing);
    }
    #[rstest]
    #[case::not_json("this is not json")]
    #[case::wrong_version(r#"{"version": 2}"#)]
    #[case::missing_version(r#"{"variables": []}"#)]
    #[case::version_not_integer(r#"{"version": "1"}"#)]
    #[case::not_an_object("[1]")]
    fn read_when_invalid_then_malformed(#[case] text: &str) {
        assert_eq!(read_text(text), RecordRead::Malformed);
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
