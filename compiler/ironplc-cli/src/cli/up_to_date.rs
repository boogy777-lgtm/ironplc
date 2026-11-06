//! The build record in the `compile` command: whether the build can be skipped
//! because nothing changed, and the writing of the record after a build.
//!
//! CODESYS prints that the application is up to date and does not compile when
//! nothing changed since the last build. The record that makes it possible is
//! `ironplc_project::build_record`; this module only decides what the command
//! does with it and says what it found.

use std::path::{Path, PathBuf};

use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_problems::codeless;
use ironplc_project::build_record::{
    self, BuildInputs, BuildRecord, BuildRecordRead, ContainerIdentity,
};
use ironplc_project::RecordKind;

/// What the build record says about the build that is about to run.
pub(super) struct Previous {
    /// The name of the application when the build is up to date and does not
    /// need to run.
    pub up_to_date: Option<String>,
    /// The messages about the record itself: one line when the file cannot be
    /// used, or the problem that kept it from being read.
    pub diagnostics: Vec<Diagnostic>,
}

/// What the build record says about the build that would write `output`, found
/// without building: the one question both the `compile` command (may it skip?)
/// and the go-online comparison (is the project as it was built?) ask.
pub(super) enum Query {
    /// There is no record, or `output` has no name to derive one from.
    Missing,
    /// The file is not a build record of this version.
    Malformed(PathBuf),
    /// The record could not be read: a genuine I/O failure.
    Failed(Box<Diagnostic>),
    /// The record, and whether a build with these inputs would produce the
    /// container that is on disk: the inputs are the record's and the file is the
    /// container the record describes.
    Present {
        record: Box<BuildRecord>,
        up_to_date: bool,
    },
}

/// Reads the record of the build that writes `output` and compares it with the
/// inputs of the build that is about to run.
pub(super) fn query(output: &Path, inputs: &BuildInputs) -> Query {
    let Some(path) = RecordKind::Build.path(output) else {
        return Query::Missing;
    };
    match build_record::read(&path) {
        Ok(BuildRecordRead::Missing) => Query::Missing,
        Ok(BuildRecordRead::Malformed) => Query::Malformed(path),
        Ok(BuildRecordRead::Present(record)) => {
            let up_to_date = record.is_up_to_date(inputs, output);
            Query::Present { record, up_to_date }
        }
        Err(problem) => Query::Failed(Box::new(problem)),
    }
}

/// Reads the record of the build that writes `output` and compares it with the
/// build that is about to run.
///
/// The build is up to date when `may_skip` and the record is the record of the
/// same inputs and of the container that is on disk. A missing record says
/// nothing. A record that cannot be used is reported once (it is rewritten after
/// the build), and only a genuine I/O failure is a problem.
pub(super) fn previous(output: &Path, inputs: &BuildInputs, may_skip: bool) -> Previous {
    let mut previous = Previous {
        up_to_date: None,
        diagnostics: vec![],
    };
    match query(output, inputs) {
        Query::Missing => {}
        Query::Malformed(path) => {
            previous
                .diagnostics
                .push(Diagnostic::codeless(codeless::record_malformed(
                    super::display_path(&path.to_string_lossy()),
                )));
        }
        Query::Present { record, up_to_date } => {
            if may_skip && up_to_date {
                previous.up_to_date = Some(record.application);
            }
        }
        Query::Failed(problem) => previous.diagnostics.push(*problem),
    }
    previous
}

/// The line that says the build did not run because the application is up to
/// date.
pub(super) fn up_to_date_line() -> Diagnostic {
    Diagnostic::codeless(codeless::build_up_to_date())
}

/// Writes the record of the build that wrote the container `container` to
/// `output`, from `inputs`.
///
/// The identity is read from the bytes that were written, because the header of
/// a container is derived while it is serialized.
pub(super) fn write(
    output: &Path,
    inputs: BuildInputs,
    application: &str,
    container: &[u8],
) -> Result<(), Diagnostic> {
    // An output without a name is not a file that could have been written.
    let Some(path) = RecordKind::Build.path(output) else {
        return Ok(());
    };
    let Some(identity) = ContainerIdentity::of_bytes(container) else {
        // A container that was just serialized starts with its header.
        return Err(Diagnostic::internal_error());
    };
    build_record::write(
        &path,
        &BuildRecord {
            application: application.to_owned(),
            inputs,
            container: identity,
        },
    )
}
