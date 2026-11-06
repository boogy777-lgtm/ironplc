//! The transition table of the hot-edit guards: which edit command is
//! allowed in which edit state, and the refusal when it is not.
//!
//! The host consults [`edit_guard`] first in each of its edit operations, so
//! the table is the single place that says what a command does in a state.
//! Anything that needs to tell a client which commands are allowed, and why
//! not, reads the same table.
//!
//! The commands are the five edit operations of the host: accept (`stage`),
//! test, untest, assemble and cancel. Device settings and the execution
//! permit are allowed in every state and are not edit commands, so they have
//! no rows here.
//!
//! The table is exhaustive: a new state or a new command that has no row does
//! not compile.

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

/// One of the edit operations of the host.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum EditCommand {
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
            // A migration candidate has no revert path.
            Untest if migration => Err(UntestUnsupported),
            Untest => Ok(()),
            Assemble => Ok(()),
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
