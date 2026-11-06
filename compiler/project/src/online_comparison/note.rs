//! The information rows: conditions that stand beside the headline whatever it
//! is. Each is one line of the table below, with the rule that finds it and the
//! text it gives. They inform; none of them refuses to connect.

use ironplc_container::FORMAT_VERSION;
use serde::Serialize;

use super::{Context, EditStage, ProjectState, RunState};

table! {
    /// The kinds of information row. The names in the comments are the rows of
    /// the go-online table of ADR-0089 that they carry.
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize)]
    #[serde(rename_all = "camelCase")]
    pub enum NoteKind -> Option<String>, |ctx| {
        /// Row 12: the controller is not the one last chosen.
        OtherController => other_controller(ctx),
        /// The controller reports no device id, so no download record is kept for it.
        NoDeviceId => no_device_id(ctx),
        /// The controller does not report the content of its application.
        ContentNotReported => content_not_reported(ctx),
        /// Row 10: an online change is unfinished on the controller.
        EditInProgress => edit_in_progress(ctx),
        /// Row 11: the project does not build.
        BuildFails => build_fails(ctx),
        /// Row 15: the controller does not accept the container format this compiler writes.
        ContainerFormatRefused => container_format_refused(ctx),
        /// The controller executes no scan.
        Stopped => stopped(ctx),
        /// The controller is faulted.
        Faulted => faulted(ctx),
        /// Row 14: the running code is not what the controller would boot.
        NotPersisted => not_persisted(ctx),
        /// Row 14: a restart lost an unfinished online change.
        CandidateLost => candidate_lost(ctx),
        /// Row 14: the controller did not use its stored retained values at boot.
        RetainedDiscarded => retained_discarded(ctx),
    }
}

/// One information row of a comparison.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
pub struct InfoRow {
    /// Which row.
    pub kind: NoteKind,
    /// What it says.
    pub text: String,
}

/// The information rows that apply, in the order of the table.
pub(super) fn collect(ctx: &Context<'_>) -> Vec<InfoRow> {
    NoteKind::ALL
        .iter()
        .filter_map(|kind| kind.evaluate(ctx).map(|text| InfoRow { kind: *kind, text }))
        .collect()
}

fn other_controller(ctx: &Context<'_>) -> Option<String> {
    let connected = ctx.device?.device_id?;
    let chosen = ctx.path?.device_id;
    (connected != chosen).then(|| {
        format!(
            "This is not the controller last chosen for this project ({chosen}). \
             Going online does not need it to be; this comparison is for the controller that answered."
        )
    })
}

fn no_device_id(ctx: &Context<'_>) -> Option<String> {
    ctx.device
        .filter(|device| device.device_id.is_none())
        .map(|_| {
            "The controller reports no device id, so no download record is kept for it.".to_owned()
        })
}

fn content_not_reported(ctx: &Context<'_>) -> Option<String> {
    ctx.device
        .filter(|device| device.content_hash.is_none())
        .map(|_| {
            "The controller does not report what its application contains, so it cannot be compared with the project."
                .to_owned()
        })
}

fn edit_in_progress(ctx: &Context<'_>) -> Option<String> {
    let edit = ctx.device?.edit.as_ref()?;
    match edit.stage {
        EditStage::Idle => None,
        EditStage::Accepted
        | EditStage::Testing
        | EditStage::TestingMigration
        | EditStage::TestingReplace
        | EditStage::SwapPending => Some(
            "An online change is unfinished on the controller. Resolve it before another edit; a restart of the controller loses it."
                .to_owned(),
        ),
    }
}

fn build_fails(ctx: &Context<'_>) -> Option<String> {
    match ctx.project {
        ProjectState::BuildFails(_) => Some("The project does not build.".to_owned()),
        ProjectState::NeverBuilt
        | ProjectState::UpToDate(_)
        | ProjectState::ChangedSinceBuild(_) => None,
    }
}

fn container_format_refused(ctx: &Context<'_>) -> Option<String> {
    let accepted = ctx.device?.container_format?;
    (accepted != FORMAT_VERSION).then(|| {
        format!(
            "The controller accepts container format {accepted}; this compiler writes format {FORMAT_VERSION}, so the controller refuses a download or an online change."
        )
    })
}

fn stopped(ctx: &Context<'_>) -> Option<String> {
    match ctx.device?.run_state {
        RunState::Stopped => Some("The controller is stopped: it executes no scan.".to_owned()),
        RunState::Running => None,
    }
}

fn faulted(ctx: &Context<'_>) -> Option<String> {
    let fault = ctx.device?.fault.as_ref()?;
    Some(format!(
        "The controller is faulted ({}: {}). It executes no scan until it is restarted.",
        fault.v_code, fault.message
    ))
}

fn not_persisted(ctx: &Context<'_>) -> Option<String> {
    let device = ctx.device?;
    let running = device.content_hash?;
    let persisted = device.persisted_content_hash?;
    (running != persisted).then(|| {
        "The running code is not persisted: after a restart the controller runs another application."
            .to_owned()
    })
}

fn candidate_lost(ctx: &Context<'_>) -> Option<String> {
    let lost = ctx.device?.candidate_lost.as_ref()?;
    let name = match &lost.name {
        Some(name) => format!(" \"{name}\""),
        None => String::new(),
    };
    Some(format!(
        "A restart lost an unfinished online change{name}. Repeat the edit."
    ))
}

fn retained_discarded(ctx: &Context<'_>) -> Option<String> {
    let note = ctx.device?.retained.as_ref()?;
    (note.result == "discarded").then(|| match &note.reason {
        Some(reason) => {
            format!("At boot the controller did not use its stored retained values ({reason}).")
        }
        None => "At boot the controller did not use its stored retained values.".to_owned(),
    })
}
