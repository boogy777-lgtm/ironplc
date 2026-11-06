//! The go-online comparison: what the controller is, what it holds, what the
//! project has, and which actions follow (ADR-0089).
//!
//! [`compare`] is one pure function. It reads the answer to the attempt to
//! connect ([`Attempt`]), the records ([`Records`]) and the state of the project
//! ([`ProjectState`]), and returns a [`Comparison`] that a client renders. The
//! editor, the command line (`ironplcc online-compare`) and the MCP server all
//! call it, so no client decides what an action is worth or whether a hot edit
//! may begin.
//!
//! ## The rules that hold for every comparison
//!
//! - **Connect never refuses.** When the attempt reached the controller, Monitor
//!   is enabled, whatever the controller holds and whether or not it is the
//!   controller last chosen. A different controller is an information row.
//! - **The content hash is the only comparison.** The generation counters of a
//!   controller restart when it boots and are not read.
//! - **One row of the headline, any number of information rows.** The headline
//!   ([`Row`]) names the relation of what the controller holds to the project
//!   and to the last download; the information rows ([`NoteKind`]) are the
//!   conditions that stand beside any headline.
//! - **Every action is a row.** [`ActionKind`] lists the actions; each has an
//!   enablement rule that returns a [`Refusal`] with its text when it does not
//!   apply, and the comparison offers all of them, enabled or not. A new action
//!   is one line of the table in `action.rs`, and a new information row one line
//!   of the table in `note.rs`; the build fails when a variant has no rule.

use ironplc_container::integrity;
use ironplc_discovery::DeviceId;
use serde::Serialize;

use crate::build_record::BuildRecord;
use crate::download_record::DownloadRecord;
use crate::path_record::PathRecord;

/// Declares one class of the comparison from its members: the enum, the list of
/// all its members and the one function that applies a member's rule to the
/// context. A member that is added is added here and nowhere else, and a member
/// without a rule does not compile.
macro_rules! table {
    (
        $(#[$meta:meta])*
        $vis:vis enum $name:ident -> $out:ty, |$ctx:ident| {
            $($(#[$vmeta:meta])* $variant:ident => $rule:expr),+ $(,)?
        }
    ) => {
        $(#[$meta])*
        $vis enum $name {
            $($(#[$vmeta])* $variant),+
        }

        impl $name {
            /// Every member, in the order the comparison lists them.
            pub const ALL: &'static [$name] = &[$($name::$variant),+];

            /// The result of this member's rule for the context.
            fn evaluate(self, $ctx: &Context<'_>) -> $out {
                match self {
                    $($name::$variant => $rule),+
                }
            }
        }
    };
}

mod action;
mod device;
mod note;
mod row;
#[cfg(test)]
mod tests;

pub use action::{ActionKind, ActionOffer, HotEdit};
pub use device::{
    BootNote, CandidateLost, DeviceEdit, DeviceFacts, EditStage, Fault, IdentityError, Reason,
    RunState,
};
pub use note::{InfoRow, NoteKind};
pub use row::Row;

/// The answer to the attempt to connect.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Attempt<'a> {
    /// A session opened and the controller answered `identity`.
    Ok(&'a DeviceFacts),
    /// The controller did not answer.
    Unreachable,
    /// The controller serves another session (V6014).
    Busy,
    /// The controller wants credentials. They are a later class, so a protected
    /// controller cannot be connected yet.
    Protected,
    /// The session was open and ended.
    ConnectionLost,
}

/// The identity of an application container, as the build record or the
/// download record states it.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct BuiltApplication {
    /// The name of the application.
    pub name: String,
    /// The `contentHash` of its container: P.
    pub content_hash: [u8; integrity::HASH_LEN],
    /// The `layoutHash` of its container: Lp.
    pub layout_hash: [u8; integrity::HASH_LEN],
}

impl From<&BuildRecord> for BuiltApplication {
    fn from(record: &BuildRecord) -> Self {
        BuiltApplication {
            name: record.application.clone(),
            content_hash: record.container.content_hash,
            layout_hash: record.container.layout_hash,
        }
    }
}

/// What the project is, given the last build and the sources now. The three
/// states with a build are the answers of the up-to-date check of ADR-0083 run
/// without compiling; the fourth is the project that has no usable build record.
#[derive(Clone, Debug, PartialEq, Eq)]
pub enum ProjectState {
    /// There is no usable build record: the project was never built here.
    NeverBuilt,
    /// The sources and the container on disk are those of the last build.
    UpToDate(BuiltApplication),
    /// The sources are not those of the last build, which the record describes.
    ChangedSinceBuild(BuiltApplication),
    /// The sources do not build; the last successful build, when there was one.
    BuildFails(Option<BuiltApplication>),
}

impl ProjectState {
    /// The last successful build: P and Lp.
    pub fn built(&self) -> Option<&BuiltApplication> {
        match self {
            ProjectState::NeverBuilt => None,
            ProjectState::UpToDate(built) | ProjectState::ChangedSinceBuild(built) => Some(built),
            ProjectState::BuildFails(built) => built.as_ref(),
        }
    }
}

/// The download record of the controller, and whether the container that was
/// sent is kept beside it (`download_record::kept_container_path`).
#[derive(Clone, Copy, Debug)]
pub struct LastDownload<'a> {
    /// The record, read for the id of the connected controller.
    pub record: &'a DownloadRecord,
    /// Whether the copy of the sent container exists, so that what the
    /// controller holds can be monitored with its debug information.
    pub kept_container: bool,
}

/// The records of the project that the comparison reads.
#[derive(Clone, Copy, Debug, Default)]
pub struct Records<'a> {
    /// The download record R for the id of the connected controller. A record
    /// of another device is ignored.
    pub download: Option<LastDownload<'a>>,
    /// The path record: the controller last chosen.
    pub path: Option<&'a PathRecord>,
}

/// What the comparison reads, gathered once for the rules.
struct Context<'a> {
    device: Option<&'a DeviceFacts>,
    download: Option<LastDownload<'a>>,
    path: Option<&'a PathRecord>,
    project: &'a ProjectState,
    row: Row,
}

impl Context<'_> {
    /// Whether the attempt reached the controller.
    fn connected(&self) -> bool {
        self.device.is_some()
    }
}

/// The comparison, as a client renders it.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct Comparison {
    /// The headline.
    pub row: Row,
    /// The number of the headline row in the table of ADR-0089.
    pub row_number: u8,
    /// One line for the status bar.
    pub summary: &'static str,
    /// Whether a session is open.
    pub connected: bool,
    /// Whether the editor goes online with no dialog: the application is up to
    /// date.
    pub at_once: bool,
    /// What the controller is and what it holds; absent when it was not reached.
    pub controller: Option<ControllerRow>,
    /// What the project has.
    pub project: ProjectRow,
    /// The last download of this project to this controller, when there is a
    /// record.
    pub last_download: Option<DownloadRow>,
    /// The conditions that stand beside the headline.
    pub information: Vec<InfoRow>,
    /// Every action, enabled or not, each with its reason when it is not.
    pub actions: Vec<ActionOffer>,
    /// Whether the Build of an online change may begin: the answer of the
    /// `onlineChange` action.
    pub hot_edit: HotEdit,
}

/// What the controller holds.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(tag = "kind", rename_all = "camelCase")]
pub enum Holds {
    /// The built-in empty application (`EMPTY_APPLICATION_HASH`).
    NoApplication,
    /// An application, by its hashes.
    #[serde(rename_all = "camelCase")]
    Application {
        /// The `contentHash`.
        content_hash: String,
        /// The `layoutHash`, when reported.
        layout_hash: Option<String>,
    },
    /// The controller does not report the content of its application.
    Unknown,
}

/// What the controller is.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ControllerRow {
    /// The id of the device, when it reports one.
    pub device_id: Option<String>,
    /// The runtime version, when reported.
    pub runtime_version: Option<String>,
    /// Whether it scans.
    pub run_state: RunState,
    /// What it holds.
    pub holds: Holds,
    /// Where its unfinished edit stands, when it reports an edit view.
    pub edit_stage: Option<EditStage>,
}

/// What the project has.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ProjectRow {
    /// The state of the project against its last build.
    pub status: ProjectStatus,
    /// The name of the application of the last build.
    pub application: Option<String>,
    /// P: the `contentHash` of the last build.
    pub content_hash: Option<String>,
    /// Lp: the `layoutHash` of the last build.
    pub layout_hash: Option<String>,
}

/// The state of the project against its last build.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub enum ProjectStatus {
    /// The sources are those of the last build.
    UpToDate,
    /// The sources changed since the last build.
    ChangedSinceBuild,
    /// There is no build record.
    NeverBuilt,
    /// The sources do not build.
    BuildFails,
}

/// The last download of this project to the controller.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct DownloadRow {
    /// R: the `contentHash` of the container that was sent.
    pub content_hash: String,
    /// The `layoutHash` of that container.
    pub layout_hash: String,
    /// When it was recorded, RFC 3339 in UTC.
    pub time: String,
}

/// Compares the controller with the project.
///
/// `attempt` is the answer to the connect. `records` holds the download record
/// of the connected controller (the caller looks it up by the id the controller
/// reports; a record of another id is ignored here) and the path record.
/// `project` is the state of the project from the up-to-date check.
pub fn compare(attempt: &Attempt<'_>, records: &Records<'_>, project: &ProjectState) -> Comparison {
    let device = match attempt {
        Attempt::Ok(device) => Some(*device),
        Attempt::Unreachable | Attempt::Busy | Attempt::Protected | Attempt::ConnectionLost => None,
    };
    let download = records.download.filter(|download| {
        device.and_then(|device| device.device_id) == Some(download.record.device_id)
    });
    let row = row::classify(attempt, download.as_ref(), project);
    let context = Context {
        device,
        download,
        path: records.path,
        project,
        row,
    };

    let actions: Vec<ActionOffer> = ActionKind::ALL
        .iter()
        .map(|kind| action::offer(*kind, &context))
        .collect();
    Comparison {
        row,
        row_number: row.number(),
        summary: row.summary(),
        connected: context.connected(),
        at_once: row == Row::UpToDate,
        controller: device.map(controller_row),
        project: project_row(project),
        last_download: download.map(|download| download_row(download.record)),
        information: note::collect(&context),
        actions,
        hot_edit: action::hot_edit(&context),
    }
}

fn controller_row(device: &DeviceFacts) -> ControllerRow {
    let holds = match device.content_hash {
        None => Holds::Unknown,
        Some(hash) if hash == ironplc_container::EMPTY_APPLICATION_HASH => Holds::NoApplication,
        Some(hash) => Holds::Application {
            content_hash: hex(&hash),
            layout_hash: device.layout_hash.as_ref().map(hex),
        },
    };
    ControllerRow {
        device_id: device.device_id.as_ref().map(DeviceId::to_string),
        runtime_version: device.runtime_version.clone(),
        run_state: device.run_state,
        holds,
        edit_stage: device.edit.as_ref().map(|edit| edit.stage),
    }
}

fn project_row(project: &ProjectState) -> ProjectRow {
    let status = match project {
        ProjectState::NeverBuilt => ProjectStatus::NeverBuilt,
        ProjectState::UpToDate(_) => ProjectStatus::UpToDate,
        ProjectState::ChangedSinceBuild(_) => ProjectStatus::ChangedSinceBuild,
        ProjectState::BuildFails(_) => ProjectStatus::BuildFails,
    };
    let built = project.built();
    ProjectRow {
        status,
        application: built.map(|built| built.name.clone()),
        content_hash: built.map(|built| hex(&built.content_hash)),
        layout_hash: built.map(|built| hex(&built.layout_hash)),
    }
}

fn download_row(record: &DownloadRecord) -> DownloadRow {
    DownloadRow {
        content_hash: hex(&record.container.content_hash),
        layout_hash: hex(&record.container.layout_hash),
        time: record.time.clone(),
    }
}

fn hex(hash: &[u8; integrity::HASH_LEN]) -> String {
    integrity::hash_hex(hash).to_string()
}
