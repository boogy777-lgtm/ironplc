//! The build record: what the last successful build produced and from which
//! inputs, kept in `<app>.build.iplcinfo` beside the container
//! ([`RecordKind::Build`]).
//!
//! With it a build that finds nothing changed can say so and skip the compile
//! ([`BuildRecord::is_up_to_date`]).
//!
//! ## The inputs of a build
//!
//! [`BuildInputs`] is everything that can change the bytes of the container: the
//! text and path of each source, the text of each activated library, every
//! setting of the [`CompilerOptions`], the stable variable UID table, the name
//! of the project (it names the application when the program has no
//! `CONFIGURATION`) and the version of the compiler. The output path is not an
//! input: it is the anchor of the record.
//!
//! [`BuildInputs::of`] is the one function that builds them, and it reads them
//! from what the compile receives (the project and the options), so an input
//! that is added to the compile changes the fingerprint where it is gathered:
//!
//! - a new option flag or policy is listed by [`CompilerOptions::entries`], which
//!   the options macro generates, without an edit here;
//! - any other new input is one more field of [`BuildInputs`] and one more line
//!   of [`BuildInputs::of`], and a test below that changes it.
//!
//! The fingerprint is itemized, not a single hash, so a record says which input
//! changed.
//!
//! ## Format
//!
//! ```json
//! {
//!   "version": 1,
//!   "application": "Main",
//!   "compilerVersion": "0.247.0",
//!   "inputs": {
//!     "projectName": "plant",
//!     "sources": [{ "path": "/plant/main.st", "hash": "<blake3 hex>" }],
//!     "libraries": [{ "name": "Tc2_System", "files": [{ "path": "library.toml", "hash": "<blake3 hex>" }] }],
//!     "options": { "allow_adr": "false", "disabled_warnings": "P4083" },
//!     "uids": "<blake3 hex>"
//!   },
//!   "container": { "contentHash": "<hex>", "layoutHash": "<hex>", "debugHash": "<hex>", "size": 1234 }
//! }
//! ```
//!
//! A library whose files cannot be read has `"files": null`; the compile reports
//! why and writes no container, so no record is written with it.

use std::collections::BTreeMap;
use std::fs::{self, File};
use std::path::Path;

use ironplc_container::{integrity, FileHeader, HEADER_SIZE};
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_parser::options::CompilerOptions;
use serde_json::{json, Value};

use crate::project::{FileBackedProject, Project};
use crate::records::{self, RecordKind, RecordRead};
use crate::sidecar;

/// A file that is an input: where it is and the BLAKE3 hash of its text.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct InputFile {
    pub path: String,
    /// BLAKE3 of the text, in lowercase hexadecimal.
    pub hash: String,
}

impl InputFile {
    fn of_text(path: impl Into<String>, text: &str) -> Self {
        InputFile {
            path: path.into(),
            hash: hash_hex(text.as_bytes()),
        }
    }

    fn to_json(&self) -> Value {
        json!({"path": self.path, "hash": self.hash})
    }

    fn from_json(value: &Value) -> Option<Self> {
        Some(InputFile {
            path: value.get("path")?.as_str()?.to_owned(),
            hash: value.get("hash")?.as_str()?.to_owned(),
        })
    }
}

/// An activated compatibility library: its name and its files, or `None` when
/// the library cannot be read.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct InputLibrary {
    pub name: String,
    pub files: Option<Vec<InputFile>>,
}

impl InputLibrary {
    fn to_json(&self) -> Value {
        let files = self
            .files
            .as_ref()
            .map(|files| Value::Array(files.iter().map(InputFile::to_json).collect()));
        json!({"name": self.name, "files": files})
    }

    fn from_json(value: &Value) -> Option<Self> {
        let files = match value.get("files")? {
            Value::Null => None,
            files => Some(files_from_json(files)?),
        };
        Some(InputLibrary {
            name: value.get("name")?.as_str()?.to_owned(),
            files,
        })
    }
}

/// Everything that can change the bytes of the container a build writes.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct BuildInputs {
    /// The version of the compiler.
    pub compiler_version: String,
    /// The name of the project (the file name without its extension of the first
    /// path, or the name of the directory), which names the application when the
    /// program has no `CONFIGURATION`.
    pub project_name: String,
    /// The sources, ascending by path.
    pub sources: Vec<InputFile>,
    /// The activated libraries, in activation order.
    pub libraries: Vec<InputLibrary>,
    /// Every setting of the options, by key ([`CompilerOptions::entries`]).
    pub options: BTreeMap<String, String>,
    /// BLAKE3 of the serialized stable variable UID table.
    pub uids: String,
}

impl BuildInputs {
    /// The inputs of a build of `project` with `options`.
    ///
    /// `project_name` is the name of the project and `compiler_version` the
    /// version of the compiler that builds. The project must be the one the
    /// compile is given, with its files read, its libraries activated and its
    /// UID table loaded.
    pub fn of(
        project: &FileBackedProject,
        options: &CompilerOptions,
        project_name: &str,
        compiler_version: &str,
    ) -> Self {
        let mut sources: Vec<InputFile> = project
            .sources()
            .into_iter()
            .map(|source| InputFile::of_text(source.file_id().to_string(), source.as_string()))
            .collect();
        sources.sort_by(|a, b| a.path.cmp(&b.path));

        BuildInputs {
            compiler_version: compiler_version.to_owned(),
            project_name: project_name.to_owned(),
            sources,
            libraries: library_inputs(project.activated_library_texts()),
            options: options
                .entries()
                .into_iter()
                .map(|(key, value)| (key.to_owned(), value))
                .collect(),
            uids: hash_hex(sidecar::body_text(project.stable_var_ids()).as_bytes()),
        }
    }

    fn to_json(&self) -> Value {
        json!({
            "projectName": self.project_name,
            "sources": self.sources.iter().map(InputFile::to_json).collect::<Vec<_>>(),
            "libraries": self.libraries.iter().map(InputLibrary::to_json).collect::<Vec<_>>(),
            "options": self.options,
            "uids": self.uids,
        })
    }

    fn from_json(compiler_version: &str, value: &Value) -> Option<Self> {
        let options = value
            .get("options")?
            .as_object()?
            .iter()
            .map(|(key, value)| Some((key.clone(), value.as_str()?.to_owned())))
            .collect::<Option<_>>()?;
        Some(BuildInputs {
            compiler_version: compiler_version.to_owned(),
            project_name: value.get("projectName")?.as_str()?.to_owned(),
            sources: files_from_json(value.get("sources")?)?,
            libraries: value
                .get("libraries")?
                .as_array()?
                .iter()
                .map(InputLibrary::from_json)
                .collect::<Option<_>>()?,
            options,
            uids: value.get("uids")?.as_str()?.to_owned(),
        })
    }
}

/// The libraries as inputs. A library that cannot be read has no files: the
/// compile reports the problem itself and writes no container, so the
/// problem is not lost here.
fn library_inputs(texts: Vec<ironplc_sources::project::LibraryTexts>) -> Vec<InputLibrary> {
    texts
        .into_iter()
        .map(|(name, texts)| InputLibrary {
            name: name.to_string(),
            files: texts.ok().map(|texts| {
                texts
                    .iter()
                    .map(|(path, text)| InputFile::of_text(path.as_str(), text))
                    .collect()
            }),
        })
        .collect()
}

fn files_from_json(value: &Value) -> Option<Vec<InputFile>> {
    value.as_array()?.iter().map(InputFile::from_json).collect()
}

/// The identity of a container, as its header states it: the three hashes and the
/// size of the file.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct ContainerIdentity {
    pub content_hash: [u8; 32],
    pub layout_hash: [u8; 32],
    pub debug_hash: [u8; 32],
    /// The size of the file in bytes.
    pub size: u64,
}

impl ContainerIdentity {
    /// The identity of the container whose bytes are `bytes`, or `None` when they
    /// do not start with a valid header.
    pub fn of_bytes(bytes: &[u8]) -> Option<Self> {
        let header: &[u8; HEADER_SIZE] = bytes.get(..HEADER_SIZE)?.try_into().ok()?;
        Some(Self::of_header(
            &FileHeader::from_bytes(header).ok()?,
            bytes.len() as u64,
        ))
    }

    /// The identity of the container file at `path`, or `None` when there is no
    /// such file or it does not start with a valid header.
    pub fn of_file(path: &Path) -> Option<Self> {
        let size = fs::metadata(path).ok()?.len();
        let header = FileHeader::read_from(&mut File::open(path).ok()?).ok()?;
        Some(Self::of_header(&header, size))
    }

    fn of_header(header: &FileHeader, size: u64) -> Self {
        ContainerIdentity {
            content_hash: header.content_hash,
            layout_hash: header.layout_hash,
            debug_hash: header.debug_hash,
            size,
        }
    }

    pub(crate) fn to_json(&self) -> Value {
        json!({
            "contentHash": hex(&self.content_hash),
            "layoutHash": hex(&self.layout_hash),
            "debugHash": hex(&self.debug_hash),
            "size": self.size,
        })
    }

    pub(crate) fn from_json(value: &Value) -> Option<Self> {
        Some(ContainerIdentity {
            content_hash: integrity::hash_from_hex(value.get("contentHash")?.as_str()?)?,
            layout_hash: integrity::hash_from_hex(value.get("layoutHash")?.as_str()?)?,
            debug_hash: integrity::hash_from_hex(value.get("debugHash")?.as_str()?)?,
            size: value.get("size")?.as_u64()?,
        })
    }
}

/// What a successful build produced and from which inputs.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct BuildRecord {
    /// The name of the application the build was for, which the line "Build
    /// started" gives. It is a result of the build (it comes from the
    /// `CONFIGURATION` of the program), not an input.
    pub application: String,
    pub inputs: BuildInputs,
    pub container: ContainerIdentity,
}

impl BuildRecord {
    /// Whether a build with `inputs` that writes the container at `container` would
    /// produce what is already there: the inputs are those of the record and the
    /// file is the container the record describes.
    pub fn is_up_to_date(&self, inputs: &BuildInputs, container: &Path) -> bool {
        self.inputs == *inputs
            && ContainerIdentity::of_file(container) == Some(self.container.clone())
    }

    fn to_json(&self) -> Value {
        json!({
            "application": self.application,
            "compilerVersion": self.inputs.compiler_version,
            "inputs": self.inputs.to_json(),
            "container": self.container.to_json(),
        })
    }

    fn from_json(value: &Value) -> Option<Self> {
        Some(BuildRecord {
            application: value.get("application")?.as_str()?.to_owned(),
            inputs: BuildInputs::from_json(
                value.get("compilerVersion")?.as_str()?,
                value.get("inputs")?,
            )?,
            container: ContainerIdentity::from_json(value.get("container")?)?,
        })
    }
}

/// The outcome of reading the build record.
#[derive(Debug, PartialEq)]
pub enum BuildRecordRead {
    /// There is no file.
    Missing,
    /// The file is not a build record of this version: it is not JSON, has
    /// another version, or has a shape that is not the record's.
    Malformed,
    /// The record. Boxed: it is much larger than the other outcomes.
    Present(Box<BuildRecord>),
}

/// Reads the build record at `path`. Fails only on a genuine I/O error; see
/// [`records::read`].
pub fn read(path: &Path) -> Result<BuildRecordRead, Diagnostic> {
    Ok(match records::read(RecordKind::Build, path)? {
        RecordRead::Missing => BuildRecordRead::Missing,
        RecordRead::Malformed => BuildRecordRead::Malformed,
        RecordRead::Present(value) => match BuildRecord::from_json(&value) {
            Some(record) => BuildRecordRead::Present(Box::new(record)),
            None => BuildRecordRead::Malformed,
        },
    })
}

/// Writes the build record to `path`, overwriting any existing file.
pub fn write(path: &Path, record: &BuildRecord) -> Result<(), Diagnostic> {
    records::write(RecordKind::Build, path, record.to_json())
}

fn hash_hex(bytes: &[u8]) -> String {
    blake3::hash(bytes).to_hex().to_string()
}

fn hex(bytes: &[u8; 32]) -> String {
    integrity::hash_hex(bytes).to_string()
}

#[cfg(test)]
mod tests;
