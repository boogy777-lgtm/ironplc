//! The edit view: which edit commands the host allows right now, with the
//! reason for each one it does not (ADR-0087).
//!
//! The view is built from the transition table of [`crate::edit_guard`] and
//! from the edit state the host derives once (`HostStatus::edit_state`), and
//! from nothing else. It has no enablement rule of its own: a command is
//! enabled when [`edit_guard`] answers `Ok`, and the reason of a disabled one
//! is the refusal the table gives, turned into a V-code and its text by the
//! one mapping the command layer already uses ([`CommandError`]). A client
//! that shows the view therefore shows exactly what the host would accept.

use std::collections::BTreeMap;

use serde::{Deserialize, Serialize};

use crate::commands::CommandError;
use crate::edit_guard::{
    edit_answer, EditCommand, EditState, Steps, FINALIZE_ALL_LABEL, FINALIZE_ALL_STEPS,
};
use crate::error::OnlineChangeError;
use crate::host::{HostStatus, PendingEditRecord};

/// Where the unfinished edit stands, in the words a client shows.
///
/// A coarser view of the edit state: whether the candidate is a migration
/// candidate matters only once it runs (a migration candidate has no revert),
/// and a requested swap that has not yet reached a scan boundary is one
/// transient stage whichever way it goes.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub enum EditStage {
    /// No candidate is staged.
    Idle,
    /// A candidate is staged and the normal artifact executes.
    Accepted,
    /// The candidate executes and can be reverted.
    Testing,
    /// The candidate of a state migration executes and cannot be reverted.
    TestingMigration,
    /// A test or an untest is requested and waits for the next scan boundary.
    SwapPending,
}

/// Why a command is not allowed: the V-code of the refusal and its plain
/// text, as the command layer would answer the command.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Reason {
    /// The stable V-code (for example `"V4013"`).
    pub code: String,
    /// The text of the refusal.
    pub text: String,
}

impl From<OnlineChangeError> for Reason {
    fn from(error: OnlineChangeError) -> Self {
        let error = CommandError::from(error);
        Reason {
            code: error.v_code().to_string(),
            text: error.message().to_string(),
        }
    }
}

/// One command of the view.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct CommandView {
    /// Whether the host accepts the command now.
    pub enabled: bool,
    /// The plain English name of the command.
    pub label: String,
    /// Why the command is not allowed. Absent when it is.
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub reason: Option<Reason>,
    /// The commands that make up this one, in order; absent for a command that
    /// is a single operation. For an edit command it is what the device runs
    /// for the one command the client sends (Cancel while the candidate
    /// executes is untest, then cancel). For `finalizeAll` it is what the
    /// client runs, one command after the other.
    #[serde(default, skip_serializing_if = "Vec::is_empty")]
    pub steps: Vec<EditCommand>,
}

impl CommandView {
    /// The view of a command that the table answers with `answer`.
    fn new(label: &str, answer: Result<Steps, OnlineChangeError>) -> Self {
        CommandView {
            enabled: answer.is_ok(),
            label: label.to_string(),
            steps: answer
                .as_ref()
                .map(|steps| steps.to_vec())
                .unwrap_or_default(),
            reason: answer.err().map(Reason::from),
        }
    }
}

/// Every command of the view: the five edit commands, keyed by their wire
/// names, and `finalizeAll`.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Commands {
    /// The edit commands. A new [`EditCommand`] is a new key.
    #[serde(flatten)]
    pub edit: BTreeMap<EditCommand, CommandView>,
    /// The composite command; its `steps` are always present.
    pub finalize_all: CommandView,
}

/// The command that the Next item of a client runs.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct NextCommand {
    /// The command to run.
    pub command: EditCommand,
    /// Its label: the label of the command in [`Commands`].
    pub label: String,
}

/// What a client may offer for the edit of a device, decided by the device.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct EditView {
    /// Where the edit stands.
    pub stage: EditStage,
    /// Each command with its enablement.
    pub commands: Commands,
    /// The command that moves the edit one stage on. Absent while a swap is
    /// pending (`stage` says so): there is nothing to run until the boundary.
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub next: Option<NextCommand>,
}

/// The stage of an edit state. Exhaustive: a new state does not compile until
/// it is named here.
fn stage(state: EditState) -> EditStage {
    match state {
        EditState::NoCandidate => EditStage::Idle,
        EditState::Accepted { migration: _ } => EditStage::Accepted,
        EditState::Testing { migration: false } => EditStage::Testing,
        EditState::Testing { migration: true } => EditStage::TestingMigration,
        EditState::TestPending { migration: _ } | EditState::UntestPending => {
            EditStage::SwapPending
        }
    }
}

/// The command that moves the edit from `state` towards its end: accept a
/// candidate, test it, assemble it. A pending swap has none. Exhaustive: a new
/// state does not compile until it is named here.
fn next_command(state: EditState) -> Option<EditCommand> {
    match state {
        EditState::NoCandidate => Some(EditCommand::Accept),
        EditState::Accepted { migration: _ } => Some(EditCommand::Test),
        EditState::Testing { migration: _ } => Some(EditCommand::Assemble),
        EditState::TestPending { migration: _ } | EditState::UntestPending => None,
    }
}

/// The view of an edit state. `pending` is the record of the staged edit, which
/// the refusal about a staged candidate names (see [`edit_answer`]).
pub(crate) fn view_of(state: EditState, pending: Option<&PendingEditRecord>) -> EditView {
    let edit = EditCommand::ALL
        .into_iter()
        .map(|command| {
            (
                command,
                CommandView::new(command.label(), edit_answer(state, command, pending)),
            )
        })
        .collect();
    // The composite row is enabled when its first step is. The pattern is
    // irrefutable: the row has at least one step by its type.
    let [first, ..] = FINALIZE_ALL_STEPS;
    let mut finalize_all = CommandView::new(FINALIZE_ALL_LABEL, edit_answer(state, first, pending));
    finalize_all.steps = FINALIZE_ALL_STEPS.to_vec();
    EditView {
        stage: stage(state),
        commands: Commands { edit, finalize_all },
        next: next_command(state).map(|command| NextCommand {
            command,
            label: command.label().to_string(),
        }),
    }
}

/// The edit view of a host status: which edit commands the host allows and,
/// for each one it does not, why.
pub fn edit_view(status: &HostStatus) -> EditView {
    view_of(status.edit_state, status.pending_edit.as_ref())
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::edit_guard::edit_state;
    use crate::host::PendingSwap;

    fn view(
        candidate_staged: bool,
        active_is_candidate: bool,
        pending: Option<PendingSwap>,
        migration: bool,
    ) -> EditView {
        view_of(
            edit_state(candidate_staged, active_is_candidate, pending, migration),
            None,
        )
    }

    fn enabled(view: &EditView) -> Vec<EditCommand> {
        EditCommand::ALL
            .into_iter()
            .filter(|command| view.commands.edit[command].enabled)
            .collect()
    }

    fn code(view: &EditView, command: EditCommand) -> Option<&str> {
        view.commands.edit[&command]
            .reason
            .as_ref()
            .map(|reason| reason.code.as_str())
    }

    fn next(view: &EditView) -> Option<EditCommand> {
        view.next.as_ref().map(|next| next.command)
    }

    #[test]
    fn edit_view_when_no_candidate_then_idle_and_accept_is_next() {
        let view = view(false, false, None, false);

        assert_eq!(view.stage, EditStage::Idle);
        assert_eq!(enabled(&view), [EditCommand::Accept]);
        assert_eq!(next(&view), Some(EditCommand::Accept));
        assert!(view.commands.finalize_all.enabled);
        assert_eq!(code(&view, EditCommand::Test), Some("V4012"));
        assert_eq!(code(&view, EditCommand::Untest), Some("V4014"));
    }

    #[test]
    fn edit_view_when_accepted_then_test_and_cancel_are_enabled_and_test_is_next() {
        for migration in [false, true] {
            let view = view(true, false, None, migration);

            assert_eq!(view.stage, EditStage::Accepted);
            assert_eq!(enabled(&view), [EditCommand::Test, EditCommand::Cancel]);
            assert_eq!(next(&view), Some(EditCommand::Test));
            assert_eq!(code(&view, EditCommand::Assemble), Some("V4017"));
            assert_eq!(code(&view, EditCommand::Accept), Some("V4013"));
        }
    }

    #[test]
    fn edit_view_when_testing_then_untest_and_assemble_are_enabled_and_assemble_is_next() {
        let view = view(true, true, None, false);

        assert_eq!(view.stage, EditStage::Testing);
        assert_eq!(
            enabled(&view),
            [
                EditCommand::Untest,
                EditCommand::Assemble,
                EditCommand::Cancel
            ]
        );
        assert_eq!(next(&view), Some(EditCommand::Assemble));
        assert_eq!(code(&view, EditCommand::Cancel), None);
    }

    #[test]
    fn edit_view_when_testing_then_cancel_shows_its_two_steps_and_no_other_command_has_any() {
        let view = view(true, true, None, false);

        assert_eq!(
            view.commands.edit[&EditCommand::Cancel].steps,
            [EditCommand::Untest, EditCommand::Cancel]
        );
        assert_eq!(
            EditCommand::ALL
                .into_iter()
                .filter(|command| *command != EditCommand::Cancel)
                .filter(|command| !view.commands.edit[command].steps.is_empty())
                .count(),
            0
        );
    }

    #[test]
    fn edit_view_when_testing_a_migration_then_untest_is_refused_with_v4011() {
        let view = view(true, true, None, true);

        assert_eq!(view.stage, EditStage::TestingMigration);
        assert_eq!(enabled(&view), [EditCommand::Assemble]);
        assert_eq!(next(&view), Some(EditCommand::Assemble));
        assert_eq!(code(&view, EditCommand::Untest), Some("V4011"));
        assert_eq!(code(&view, EditCommand::Cancel), Some("V4011"));
    }

    #[test]
    fn edit_view_when_a_swap_is_pending_then_nothing_is_enabled_and_there_is_no_next() {
        for view in [
            view(true, false, Some(PendingSwap::Test), false),
            view(true, false, Some(PendingSwap::Test), true),
            view(true, true, Some(PendingSwap::Untest), false),
        ] {
            assert_eq!(view.stage, EditStage::SwapPending);
            assert_eq!(enabled(&view), []);
            assert_eq!(next(&view), None);
            assert!(!view.commands.finalize_all.enabled);
        }
    }

    #[test]
    fn edit_view_when_disabled_then_the_reason_is_the_text_of_the_command_layer() {
        let view = view(true, false, None, false);

        let reason = view.commands.edit[&EditCommand::Accept]
            .reason
            .clone()
            .unwrap();

        assert_eq!(reason.code, "V4013");
        assert_eq!(reason.text, "a candidate is already staged");
    }

    #[test]
    fn edit_view_when_enabled_then_there_is_no_reason() {
        let view = view(false, false, None, false);

        assert_eq!(view.commands.edit[&EditCommand::Accept].reason, None);
    }

    #[test]
    fn edit_view_when_next_is_present_then_it_is_enabled_and_carries_the_label_of_the_command() {
        for (candidate, active, pending, migration) in [
            (false, false, None, false),
            (true, false, None, false),
            (true, false, None, true),
            (true, true, None, false),
            (true, true, None, true),
        ] {
            let view = view(candidate, active, pending, migration);
            let next = view.next.clone().unwrap();

            assert!(view.commands.edit[&next.command].enabled);
            assert_eq!(next.label, view.commands.edit[&next.command].label);
        }
    }

    #[test]
    fn edit_view_when_finalize_all_then_it_is_enabled_exactly_when_accept_is() {
        for (candidate, active, pending, migration) in [
            (false, false, None, false),
            (true, false, None, false),
            (true, true, None, false),
            (true, false, Some(PendingSwap::Test), false),
        ] {
            let view = view(candidate, active, pending, migration);
            let finalize = &view.commands.finalize_all;

            assert_eq!(
                finalize.enabled,
                view.commands.edit[&EditCommand::Accept].enabled
            );
            assert_eq!(
                finalize.reason,
                view.commands.edit[&EditCommand::Accept].reason
            );
            assert_eq!(
                finalize.steps,
                [
                    EditCommand::Accept,
                    EditCommand::Test,
                    EditCommand::Assemble
                ]
            );
            assert_eq!(finalize.label, "Finalize all");
        }
    }

    #[test]
    fn edit_view_when_serialized_then_the_wire_shape_is_camel_case_with_commands_by_name() {
        let json = serde_json::to_value(view(true, true, None, true)).unwrap();

        assert_eq!(json["stage"], "testingMigration");
        assert_eq!(json["next"]["command"], "assemble");
        assert_eq!(json["next"]["label"], "Assemble");
        assert_eq!(json["commands"]["untest"]["enabled"], false);
        assert_eq!(json["commands"]["untest"]["label"], "Untest");
        assert_eq!(json["commands"]["untest"]["reason"]["code"], "V4011");
        assert_eq!(json["commands"]["assemble"]["enabled"], true);
        assert!(json["commands"]["assemble"].get("reason").is_none());
        assert_eq!(
            json["commands"]["finalizeAll"]["steps"],
            serde_json::json!(["accept", "test", "assemble"])
        );
        assert_eq!(json["commands"]["finalizeAll"]["label"], "Finalize all");
    }

    #[test]
    fn edit_view_when_swap_is_pending_then_the_wire_has_no_next() {
        let json = serde_json::to_value(view(true, false, Some(PendingSwap::Test), false)).unwrap();

        assert_eq!(json["stage"], "swapPending");
        assert!(json.get("next").is_none());
    }

    #[test]
    fn edit_view_when_round_tripped_through_json_then_it_is_unchanged() {
        for (candidate, active, pending, migration) in [
            (false, false, None, false),
            (true, false, None, true),
            (true, true, None, true),
            (true, true, Some(PendingSwap::Untest), false),
        ] {
            let original = view(candidate, active, pending, migration);

            let line = serde_json::to_string(&original).unwrap();
            let back: EditView = serde_json::from_str(&line).unwrap();

            assert_eq!(back, original);
        }
    }
}
