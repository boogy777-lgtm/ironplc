//! The transition table of the hot-edit guards: which edit command is
//! allowed in which edit state, and the refusal when it is not.
//!
//! The host consults [`edit_guard`] first in each of its edit operations, so
//! the table is the single place that says what a command does in a state.
//! Anything that needs to tell a client which commands are allowed, and why
//! not, reads the same table: the edit view ([`crate::edit_view`]) is built
//! from [`edit_guard`], [`EditCommand::ALL`] and [`EditCommand::label`] and
//! has no enablement rule of its own.
//!
//! The commands are the five edit operations of the host: accept (`stage`),
//! test, untest, assemble and cancel. Device settings and the execution
//! permit are allowed in every state and are not edit commands, so they have
//! no rows here.
//!
//! The table is exhaustive: a new state or a new command that has no row does
//! not compile.

use serde::{Deserialize, Serialize};

use crate::error::OnlineChangeError;
use crate::host::PendingSwap;

/// Where the unfinished edit of a host stands, derived from its fields by
/// [`edit_state`].
///
/// `migration` says whether the staged candidate is a migration candidate,
/// i.e. whether the host holds a state migration plan.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum EditState {
    /// No candidate is staged.
    NoCandidate,
    /// A candidate is staged and the normal artifact executes.
    Accepted { migration: bool },
    /// The candidate executes and the normal artifact is kept for untest.
    Testing { migration: bool },
    /// A test is requested: the candidate is staged and becomes active at the
    /// next scan boundary.
    TestPending { migration: bool },
    /// An untest is requested: the candidate is active and the normal
    /// artifact becomes active at the next scan boundary. An untest of a
    /// migration candidate is refused, so this state never has a migration.
    UntestPending,
}

/// One of the edit operations of the host. On the wire it is the camelCase
/// name of the variant (`accept`, `test`, ...).
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub enum EditCommand {
    /// Stage a candidate (the accept operation).
    Accept,
    /// Request that the candidate become active.
    Test,
    /// Request that the normal artifact become active again.
    Untest,
    /// Promote the candidate to the normal artifact.
    Assemble,
    /// Discard the staged candidate.
    Cancel,
}

impl EditCommand {
    /// Every edit command, in the order a client lists them. A new variant
    /// does not compile until [`label`](Self::label) and the rows of
    /// [`edit_guard`] name it, and a unit test fails until it is listed here.
    pub const ALL: [EditCommand; 5] = [
        EditCommand::Accept,
        EditCommand::Test,
        EditCommand::Untest,
        EditCommand::Assemble,
        EditCommand::Cancel,
    ];

    /// The plain English name a client shows for the command, defined once
    /// here beside the table.
    pub const fn label(self) -> &'static str {
        match self {
            EditCommand::Accept => "Accept",
            EditCommand::Test => "Test",
            EditCommand::Untest => "Untest",
            EditCommand::Assemble => "Assemble",
            EditCommand::Cancel => "Cancel",
        }
    }
}

/// The composite row "finalize all": the client runs these commands in
/// order. It is enabled when the first step is, which is the whole device of
/// the row: the later steps are allowed by the states the earlier ones lead
/// to, with the scan rounds between them that apply a swap.
pub(crate) const FINALIZE_ALL_STEPS: [EditCommand; 3] = [
    EditCommand::Accept,
    EditCommand::Test,
    EditCommand::Assemble,
];

/// The label of the composite row [`FINALIZE_ALL_STEPS`].
pub(crate) const FINALIZE_ALL_LABEL: &str = "Finalize all";

/// Derives the edit state from the host fields.
///
/// Every state but [`EditState::NoCandidate`] has a staged candidate. The
/// other fields only mean something with one: the host sets
/// `active_is_candidate` and `pending` only on a staged candidate and clears
/// them in the same step that drops it, so "active candidate" or "pending
/// swap" without a candidate cannot occur. A pending swap also fixes which
/// artifact is active: a test is requested only while the normal artifact
/// executes and an untest only while the candidate does, so a pending swap
/// decides the state and `active_is_candidate` is read only without one.
pub(crate) fn edit_state(
    candidate_staged: bool,
    active_is_candidate: bool,
    pending: Option<PendingSwap>,
    migration: bool,
) -> EditState {
    if !candidate_staged {
        return EditState::NoCandidate;
    }
    match pending {
        Some(PendingSwap::Test) => EditState::TestPending { migration },
        Some(PendingSwap::Untest) => EditState::UntestPending,
        None if active_is_candidate => EditState::Testing { migration },
        None => EditState::Accepted { migration },
    }
}

/// The answer of the hot-edit state machine: whether `command` is allowed in
/// `state`, and the refusal when it is not.
///
/// Where two refusals apply, the table gives the one the host has always
/// given: a missing candidate first, then a pending swap, then the artifact
/// that is active.
pub(crate) fn edit_guard(state: EditState, command: EditCommand) -> Result<(), OnlineChangeError> {
    use EditCommand::{Accept, Assemble, Cancel, Test, Untest};
    use OnlineChangeError::{
        AssembleWithoutTest, CandidateAlreadyStaged, NoCandidateStaged, NoTestInProgress,
        NotAllowedInThisMode, UntestUnsupported,
    };

    match state {
        EditState::NoCandidate => match command {
            Accept => Ok(()),
            Test => Err(NoCandidateStaged),
            Untest => Err(NoTestInProgress),
            Assemble => Err(NoCandidateStaged),
            Cancel => Err(NoCandidateStaged),
        },
        EditState::Accepted { migration: _ } => match command {
            Accept => Err(CandidateAlreadyStaged),
            Test => Ok(()),
            Untest => Err(NoTestInProgress),
            // The candidate never ran under Test (ADR-0064).
            Assemble => Err(AssembleWithoutTest),
            Cancel => Ok(()),
        },
        EditState::Testing { migration } => match command {
            Accept => Err(CandidateAlreadyStaged),
            Test => Err(NotAllowedInThisMode),
            // A migration candidate has no revert path, and discarding it
            // would be one: the old buffers are not kept.
            Untest if migration => Err(UntestUnsupported),
            Untest => Ok(()),
            Assemble => Ok(()),
            Cancel if migration => Err(UntestUnsupported),
            Cancel => Err(NotAllowedInThisMode),
        },
        EditState::TestPending { migration: _ } => match command {
            Accept => Err(CandidateAlreadyStaged),
            Test => Err(NotAllowedInThisMode),
            // The candidate is not active, so there is no test to revert.
            Untest => Err(NoTestInProgress),
            Assemble => Err(NotAllowedInThisMode),
            Cancel => Err(NotAllowedInThisMode),
        },
        EditState::UntestPending => match command {
            Accept => Err(CandidateAlreadyStaged),
            Test => Err(NotAllowedInThisMode),
            Untest => Err(NotAllowedInThisMode),
            Assemble => Err(NotAllowedInThisMode),
            Cancel => Err(NotAllowedInThisMode),
        },
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// The position of a command in [`EditCommand::ALL`]. The match is
    /// exhaustive, so a new variant fails to compile here until it is
    /// counted below.
    fn position(command: EditCommand) -> usize {
        match command {
            EditCommand::Accept => 0,
            EditCommand::Test => 1,
            EditCommand::Untest => 2,
            EditCommand::Assemble => 3,
            EditCommand::Cancel => 4,
        }
    }

    #[test]
    fn edit_command_all_when_listed_then_names_every_variant_once_in_order() {
        for (index, command) in EditCommand::ALL.into_iter().enumerate() {
            assert_eq!(position(command), index);
        }
    }

    #[test]
    fn edit_command_label_when_every_command_then_is_a_distinct_plain_word() {
        let labels: Vec<&str> = EditCommand::ALL
            .into_iter()
            .map(EditCommand::label)
            .collect();
        assert_eq!(labels, ["Accept", "Test", "Untest", "Assemble", "Cancel"]);
    }

    #[test]
    fn finalize_all_steps_when_run_in_order_then_are_accept_test_assemble() {
        assert_eq!(
            FINALIZE_ALL_STEPS,
            [
                EditCommand::Accept,
                EditCommand::Test,
                EditCommand::Assemble
            ]
        );
    }
}
