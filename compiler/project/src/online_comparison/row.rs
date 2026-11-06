//! The headline of the comparison: one row of the table of ADR-0089, chosen by
//! the content hash of the controller against the project and the last download.

use ironplc_container::EMPTY_APPLICATION_HASH;
use serde::Serialize;

use super::{Attempt, LastDownload, ProjectState};

/// The headline. The number is the row of the go-online table; the rows that
/// are conditions beside any headline (an edit in progress, a build that
/// fails, another controller, a restart, a refused container format) are
/// [`NoteKind`](super::NoteKind)s.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub enum Row {
    /// Row 1: the controller did not answer.
    Unreachable,
    /// Row 2: the controller serves another session (V6014).
    Busy,
    /// Row 3: the controller is protected.
    Protected,
    /// Row 4: C is the empty application.
    NoApplication,
    /// Row 5: C = P and the sources are those of the build.
    UpToDate,
    /// Row 6: C = P and the sources changed since the build.
    SourcesChanged,
    /// Row 7: C = R, C is not P, and the layout is the project's.
    PreviousDownload,
    /// Row 8: C = R, C is not P, and the layout is not the project's.
    PreviousDownloadOtherLayout,
    /// Row 9: C is neither P nor R, or is not reported.
    OtherApplication,
    /// Row 13: the session ended.
    ConnectionLost,
}

impl Row {
    /// The number of the row in the table of ADR-0089.
    pub fn number(self) -> u8 {
        match self {
            Row::Unreachable => 1,
            Row::Busy => 2,
            Row::Protected => 3,
            Row::NoApplication => 4,
            Row::UpToDate => 5,
            Row::SourcesChanged => 6,
            Row::PreviousDownload => 7,
            Row::PreviousDownloadOtherLayout => 8,
            Row::OtherApplication => 9,
            Row::ConnectionLost => 13,
        }
    }

    /// The line the status bar shows.
    pub fn summary(self) -> &'static str {
        match self {
            Row::Unreachable => "The controller cannot be reached.",
            Row::Busy => "The controller serves another session (V6014).",
            Row::Protected => "The controller is protected; credentials are not supported yet.",
            Row::NoApplication => "The controller holds no application.",
            Row::UpToDate => "Up to date: the controller runs the last build of this project.",
            Row::SourcesChanged => {
                "The controller runs the last build; the sources changed since the build."
            }
            Row::PreviousDownload => {
                "The controller holds the last download, which is not the last build."
            }
            Row::PreviousDownloadOtherLayout => {
                "The controller holds the last download; the last build has another layout."
            }
            Row::OtherApplication => {
                "The controller holds an application that this workstation did not send."
            }
            Row::ConnectionLost => "The connection to the controller was lost.",
        }
    }
}

/// The headline for the attempt, the download record of the connected
/// controller and the project.
pub(super) fn classify(
    attempt: &Attempt<'_>,
    download: Option<&LastDownload<'_>>,
    project: &ProjectState,
) -> Row {
    let device = match attempt {
        Attempt::Unreachable => return Row::Unreachable,
        Attempt::Busy => return Row::Busy,
        Attempt::Protected => return Row::Protected,
        Attempt::ConnectionLost => return Row::ConnectionLost,
        Attempt::Ok(device) => device,
    };
    // The only comparison is the content hash. A controller that reports none
    // cannot be compared with anything.
    let Some(held) = device.content_hash else {
        return Row::OtherApplication;
    };
    let built = project.built();
    if held == EMPTY_APPLICATION_HASH {
        Row::NoApplication
    } else if built.is_some_and(|built| built.content_hash == held) {
        match project {
            ProjectState::UpToDate(_) => Row::UpToDate,
            ProjectState::NeverBuilt
            | ProjectState::ChangedSinceBuild(_)
            | ProjectState::BuildFails(_) => Row::SourcesChanged,
        }
    } else if let Some(record) = download
        .map(|download| &download.record.container)
        .filter(|sent| sent.content_hash == held)
    {
        // Lc against Lp. When the controller reports no layout hash, the record
        // has it: C = R, so the layout is that of the container that was sent.
        let layout = device.layout_hash.unwrap_or(record.layout_hash);
        match built {
            Some(built) if built.layout_hash != layout => Row::PreviousDownloadOtherLayout,
            Some(_) | None => Row::PreviousDownload,
        }
    } else {
        Row::OtherApplication
    }
}
