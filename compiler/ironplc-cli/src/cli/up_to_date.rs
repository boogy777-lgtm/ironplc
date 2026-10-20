//! The build record in the `compile` command: whether the build can be skipped
//! because nothing changed, and the writing of the record after a build.
//!
//! CODESYS prints that the application is up to date and does not compile when
//! nothing changed since the last build. The record that makes it possible is
//! `ironplc_project::build_record`; this module only decides what the command
//! does with it and says what it found.

use std::path::Path;

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
    let Some(path) = RecordKind::Build.path(output) else {
        return previous;
    };
    match build_record::read(&path) {
        Ok(BuildRecordRead::Missing) => {}
        Ok(BuildRecordRead::Malformed) => {
            previous
                .diagnostics
                .push(Diagnostic::codeless(codeless::record_malformed(
                    super::display_path(&path.to_string_lossy()),
                )));
        }
        Ok(BuildRecordRead::Present(record)) => {
            if may_skip && record.is_up_to_date(inputs, output) {
                previous.up_to_date = Some(record.application);
            }
        }
        Err(problem) => previous.diagnostics.push(problem),
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
