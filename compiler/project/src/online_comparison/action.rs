//! The online actions: what the comparison offers the engineer. One class, one
//! table: every action has an enablement rule that returns the refusal, with its
//! text, when it does not apply. The comparison lists every action, enabled or
//! not, so the dialog shows a disabled action with its reason and reads nothing
//! else.
//!
//! Every action is a client action: it is a sequence (the edit dialog, a build
//! and a download, a search) and not one session command, so [`ActionKind`]'s
//! name is the identifier the client runs.

use ironplc_container::FORMAT_VERSION;
use serde::Serialize;

use super::{Context, EditStage, ProjectState, Reason, Row};

table! {
    /// The online actions.
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize)]
    #[serde(rename_all = "camelCase")]
    pub enum ActionKind -> Availability, |ctx| {
        /// Login without change: watch the controller, change nothing.
        Monitor => monitor(ctx),
        /// Login with online change: the Build runs through the edit dialog.
        OnlineChange => online_change(ctx),
        /// Replace the application on the controller with the project's.
        Download => download(ctx),
        /// Create a project from what the controller holds. Not built.
        Upload => Availability::Refused(Refusal::NotAvailable),
        /// Search the network for controllers.
        Search => Availability::Enabled,
        /// Choose another controller.
        ChooseAnother => Availability::Enabled,
        /// Try the connection again.
        Retry => retry(ctx),
        /// Open the edit dialog at the stage of the unfinished edit.
        Resolve => resolve(ctx),
    }
}

impl ActionKind {
    /// The words of the action.
    pub fn label(self) -> &'static str {
        match self {
            ActionKind::Monitor => "Monitor",
            ActionKind::OnlineChange => "Online change",
            ActionKind::Download => "Download",
            ActionKind::Upload => "Upload",
            ActionKind::Search => "Search the network",
            ActionKind::ChooseAnother => "Choose another controller",
            ActionKind::Retry => "Retry",
            ActionKind::Resolve => "Resolve the unfinished edit",
        }
    }
}

/// Whether an action applies.
#[derive(Clone, Debug, PartialEq, Eq)]
pub(super) enum Availability {
    Enabled,
    Refused(Refusal),
}

/// Why an action does not apply.
#[derive(Clone, Debug, PartialEq, Eq)]
pub(super) enum Refusal {
    /// No session is open.
    NotConnected,
    /// The project does not build, so there is nothing to send.
    ProjectDoesNotBuild,
    /// The controller holds no application, so there is nothing to change.
    NoApplicationToChange,
    /// The controller holds an application this workstation did not send.
    OtherApplication,
    /// The controller does not accept the container format of this compiler.
    ContainerFormat { accepted: u16 },
    /// The controller refuses an edit now; its own code and text.
    Device(Reason),
    /// The controller has no unfinished edit.
    NoPendingEdit,
    /// The action is not built.
    NotAvailable,
    /// A session is open.
    AlreadyConnected,
    /// The controller wants credentials, which are not supported yet.
    Protected,
}

impl Refusal {
    fn reason(self) -> Reason {
        let (code, text) = match self {
            Refusal::Device(reason) => return reason,
            Refusal::NotConnected => ("notConnected", "No session is open.".to_owned()),
            Refusal::ProjectDoesNotBuild => (
                "projectDoesNotBuild",
                "The project does not build, so there is nothing to send.".to_owned(),
            ),
            Refusal::NoApplicationToChange => (
                "noApplicationToChange",
                "The controller holds no application to change; Download sends one.".to_owned(),
            ),
            Refusal::OtherApplication => (
                "otherApplication",
                "The controller holds an application that this workstation did not send, \
                 so an online change cannot be based on it; Download replaces it."
                    .to_owned(),
            ),
            Refusal::ContainerFormat { accepted } => (
                "containerFormat",
                format!(
                    "The controller accepts container format {accepted}; this compiler writes format {FORMAT_VERSION}."
                ),
            ),
            Refusal::NoPendingEdit => (
                "noPendingEdit",
                "The controller has no unfinished edit.".to_owned(),
            ),
            Refusal::NotAvailable => (
                "notAvailable",
                "Upload into a new project is not available yet.".to_owned(),
            ),
            Refusal::AlreadyConnected => {
                ("alreadyConnected", "A session is open.".to_owned())
            }
            Refusal::Protected => (
                "protected",
                "The controller is protected and asks for credentials, which are not supported yet."
                    .to_owned(),
            ),
        };
        Reason {
            code: code.to_owned(),
            text,
        }
    }
}

/// One action of a comparison.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
pub struct ActionOffer {
    /// Which action; also the identifier the client runs.
    pub action: ActionKind,
    /// Its words.
    pub label: &'static str,
    /// Whether it applies now.
    pub enabled: bool,
    /// Why it does not, when it does not.
    pub reason: Option<Reason>,
    /// What to know before running it, when there is something.
    pub detail: Option<String>,
}

/// Whether the Build of an online change may begin: the answer of the
/// `onlineChange` action, so the Build item of the dialog and the action cannot
/// disagree.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
pub struct HotEdit {
    /// Whether it may.
    pub allowed: bool,
    /// Why not, when not.
    pub reason: Option<Reason>,
}

pub(super) fn offer(kind: ActionKind, ctx: &Context<'_>) -> ActionOffer {
    let (enabled, reason) = match kind.evaluate(ctx) {
        Availability::Enabled => (true, None),
        Availability::Refused(refusal) => (false, Some(refusal.reason())),
    };
    ActionOffer {
        action: kind,
        label: kind.label(),
        enabled,
        reason,
        detail: if enabled { detail(kind, ctx) } else { None },
    }
}

pub(super) fn hot_edit(ctx: &Context<'_>) -> HotEdit {
    match ActionKind::OnlineChange.evaluate(ctx) {
        Availability::Enabled => HotEdit {
            allowed: true,
            reason: None,
        },
        Availability::Refused(refusal) => HotEdit {
            allowed: false,
            reason: Some(refusal.reason()),
        },
    }
}

fn monitor(ctx: &Context<'_>) -> Availability {
    // Connect never refuses: the controller is monitored whatever it holds.
    if ctx.connected() {
        Availability::Enabled
    } else {
        Availability::Refused(Refusal::NotConnected)
    }
}

/// What must hold before anything is sent to the controller, for a download and
/// for an online change alike: both begin with an accept.
fn send_gate(ctx: &Context<'_>) -> Option<Refusal> {
    let Some(device) = ctx.device else {
        return Some(Refusal::NotConnected);
    };
    if let Some(accepted) = device
        .container_format
        .filter(|accepted| *accepted != FORMAT_VERSION)
    {
        return Some(Refusal::ContainerFormat { accepted });
    }
    match ctx.project {
        ProjectState::BuildFails(_) => return Some(Refusal::ProjectDoesNotBuild),
        ProjectState::NeverBuilt
        | ProjectState::UpToDate(_)
        | ProjectState::ChangedSinceBuild(_) => {}
    }
    None
}

/// The refusal of the controller itself: its edit view says it does not accept a
/// candidate now.
fn device_refusal(ctx: &Context<'_>) -> Option<Refusal> {
    let edit = ctx.device?.edit.as_ref()?;
    edit.accept_refusal.clone().map(Refusal::Device)
}

fn download(ctx: &Context<'_>) -> Availability {
    match send_gate(ctx).or_else(|| device_refusal(ctx)) {
        Some(refusal) => Availability::Refused(refusal),
        None => Availability::Enabled,
    }
}

fn online_change(ctx: &Context<'_>) -> Availability {
    if let Some(refusal) = send_gate(ctx) {
        return Availability::Refused(refusal);
    }
    // An online change continues the application the controller holds, so the
    // controller must hold one that this workstation knows: the last build or
    // the last download.
    let by_content = match ctx.row {
        Row::NoApplication => Some(Refusal::NoApplicationToChange),
        Row::OtherApplication => Some(Refusal::OtherApplication),
        Row::UpToDate
        | Row::SourcesChanged
        | Row::PreviousDownload
        | Row::PreviousDownloadOtherLayout => None,
        Row::Unreachable | Row::Busy | Row::Protected | Row::ConnectionLost => {
            Some(Refusal::NotConnected)
        }
    };
    match by_content.or_else(|| device_refusal(ctx)) {
        Some(refusal) => Availability::Refused(refusal),
        None => Availability::Enabled,
    }
}

fn retry(ctx: &Context<'_>) -> Availability {
    match ctx.row {
        Row::Unreachable | Row::Busy | Row::ConnectionLost => Availability::Enabled,
        Row::Protected => Availability::Refused(Refusal::Protected),
        Row::NoApplication
        | Row::UpToDate
        | Row::SourcesChanged
        | Row::PreviousDownload
        | Row::PreviousDownloadOtherLayout
        | Row::OtherApplication => Availability::Refused(Refusal::AlreadyConnected),
    }
}

fn resolve(ctx: &Context<'_>) -> Availability {
    let Some(device) = ctx.device else {
        return Availability::Refused(Refusal::NotConnected);
    };
    match device.edit.as_ref().map(|edit| edit.stage) {
        Some(EditStage::Idle) | None => Availability::Refused(Refusal::NoPendingEdit),
        Some(
            EditStage::Accepted
            | EditStage::Testing
            | EditStage::TestingMigration
            | EditStage::TestingReplace
            | EditStage::SwapPending,
        ) => Availability::Enabled,
    }
}

/// What the engineer should know before running an enabled action.
fn detail(kind: ActionKind, ctx: &Context<'_>) -> Option<String> {
    match kind {
        ActionKind::Monitor => Some(monitor_detail(ctx).to_owned()),
        ActionKind::OnlineChange => match ctx.row {
            Row::PreviousDownloadOtherLayout => Some(
                "The layout of the project differs from what the controller holds: the build migrates the state and asks for the decisions."
                    .to_owned(),
            ),
            Row::Unreachable
            | Row::Busy
            | Row::Protected
            | Row::NoApplication
            | Row::UpToDate
            | Row::SourcesChanged
            | Row::PreviousDownload
            | Row::OtherApplication
            | Row::ConnectionLost => None,
        },
        ActionKind::Download => Some(
            "Download replaces the application and starts it from its initial values; only PERSISTENT variables with an equal declaration keep their value."
                .to_owned(),
        ),
        ActionKind::Upload
        | ActionKind::Search
        | ActionKind::ChooseAnother
        | ActionKind::Retry
        | ActionKind::Resolve => None,
    }
}

/// Whether the variables of what the controller holds can be shown by name: the
/// container with its debug information is the last build, or the copy of the
/// last download.
fn monitor_detail(ctx: &Context<'_>) -> &'static str {
    let application_level = match ctx.row {
        Row::UpToDate | Row::SourcesChanged => ctx.project.built().is_some(),
        Row::PreviousDownload | Row::PreviousDownloadOtherLayout => {
            ctx.download.is_some_and(|download| download.kept_container)
        }
        Row::Unreachable
        | Row::Busy
        | Row::Protected
        | Row::NoApplication
        | Row::OtherApplication
        | Row::ConnectionLost => false,
    };
    if application_level {
        "The variables of the application are shown by name."
    } else {
        "Device level only: this workstation has no debug information for what the controller holds."
    }
}
