//! Errors reported by the runtime host.

use core::fmt;

use ironplc_vm::FaultContext;

use crate::host::PendingEditRecord;
use crate::migration::MigrationError;
use crate::problem_codes;

/// The edit that holds the staged candidate, as its accept named it: the
/// client-supplied label and the advisory client label of the
/// [`PendingEditRecord`]. At least one is present; an accept that named
/// neither leaves nothing to say.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct StagedEdit {
    /// The client-supplied edit label.
    pub name: Option<String>,
    /// The unauthenticated advisory client label.
    pub origin: Option<String>,
}

impl StagedEdit {
    /// The edit a record describes, or `None` when the accept named neither a
    /// label nor an origin.
    fn of(record: &PendingEditRecord) -> Option<StagedEdit> {
        (record.name.is_some() || record.origin.is_some()).then(|| StagedEdit {
            name: record.name.clone(),
            origin: record.origin.clone(),
        })
    }
}

impl fmt::Display for StagedEdit {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match (&self.name, &self.origin) {
            (Some(name), Some(origin)) => write!(f, "edit \"{name}\" from \"{origin}\""),
            (Some(name), None) => write!(f, "edit \"{name}\""),
            (None, Some(origin)) => write!(f, "an edit from \"{origin}\""),
            (None, None) => write!(f, "an edit"),
        }
    }
}

/// Why a candidate was rejected or an online change request was refused.
///
/// These are host-level protocol errors; the VM traps live in
/// [`RuntimeError`]. The command layer maps this enum onto stable user-facing
/// V-codes ([`CommandError`](crate::CommandError)) from the crate's
/// `problem-codes.csv`, so this enum stays the whole host vocabulary.
#[derive(Clone, Debug, PartialEq, Eq)]
pub enum OnlineChangeError {
    /// The candidate changes the variable layout (layout hash, variable
    /// count, or header flags) and cannot share the running application's
    /// state.
    LayoutIncompatible,
    /// The candidate's task table differs from the active application's.
    /// A schedule change is a cold start in P0.
    ScheduleIncompatible,
    /// The candidate's process-image sizes (input, output, or memory image)
    /// differ from the active application's.
    IoIncompatible,
    /// The candidate changes the state layout and its stable variable IDs do
    /// not justify a per-variable migration. Carries the planner's reason.
    MigrationUnsupported(MigrationError),
    /// `untest` or `cancel` was called while a schema-changing candidate is
    /// under test. Writes made under the candidate's layout have no reverse
    /// mapping for added or removed variables, and the old buffers are not
    /// kept, so the change can only be assembled.
    UntestUnsupported,
    /// The requested operation needs a staged candidate and none is staged.
    NoCandidateStaged,
    /// `stage` was called while a candidate is already staged. Carries the
    /// edit that holds it when its accept named one, so the engineer knows
    /// whose candidate is in the way; see [`OnlineChangeError::naming`].
    CandidateAlreadyStaged(Option<StagedEdit>),
    /// `untest` was called while the original (not the candidate) is active.
    NoTestInProgress,
    /// The operation is not allowed in the current hot-edit phase, e.g.
    /// staging another candidate while testing.
    NotAllowedInThisMode,
    /// `assemble` was requested while the original (not the candidate) is
    /// active: the candidate never ran under Test, and promoting it would
    /// install code that never executed (ADR-0064).
    AssembleWithoutTest,
    /// A replicated state snapshot's byte lengths do not match its
    /// declared layout: the payload is internally inconsistent, so the
    /// apply refused without touching a byte (the crossload seam).
    SnapshotCorrupt,
}

impl OnlineChangeError {
    /// Adds what the host knows about the staged edit to a refusal that is
    /// about it. The transition table answers with the bare refusal, and the
    /// host and the edit view both call this with the pending-edit record, so
    /// the text a refused command gets and the reason the view gives are built
    /// here and nowhere else.
    pub(crate) fn naming(self, pending: Option<&PendingEditRecord>) -> OnlineChangeError {
        match self {
            OnlineChangeError::CandidateAlreadyStaged(_) => {
                OnlineChangeError::CandidateAlreadyStaged(pending.and_then(StagedEdit::of))
            }
            other => other,
        }
    }
}

impl fmt::Display for OnlineChangeError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            OnlineChangeError::LayoutIncompatible => write!(
                f,
                "candidate layout is incompatible with the active application"
            ),
            OnlineChangeError::ScheduleIncompatible => write!(
                f,
                "candidate task schedule is incompatible with the active application"
            ),
            OnlineChangeError::IoIncompatible => write!(
                f,
                "candidate process-image sizes are incompatible with the active application"
            ),
            OnlineChangeError::MigrationUnsupported(error) => {
                write!(f, "candidate state cannot be migrated: {error}")
            }
            OnlineChangeError::UntestUnsupported => write!(
                f,
                "untest and cancel are not supported after a schema-changing test; assemble instead"
            ),
            OnlineChangeError::NoCandidateStaged => write!(f, "no candidate is staged"),
            OnlineChangeError::CandidateAlreadyStaged(None) => {
                write!(f, "a candidate is already staged")
            }
            OnlineChangeError::CandidateAlreadyStaged(Some(edit)) => {
                write!(f, "a candidate is already staged: {edit}")
            }
            OnlineChangeError::NoTestInProgress => write!(f, "no test is in progress"),
            OnlineChangeError::NotAllowedInThisMode => {
                write!(f, "operation not allowed in the current hot-edit phase")
            }
            OnlineChangeError::AssembleWithoutTest => write!(
                f,
                "assemble requires the candidate to have executed under Test"
            ),
            OnlineChangeError::SnapshotCorrupt => write!(
                f,
                "replicated state snapshot is internally inconsistent: lengths do not match the declared layout"
            ),
        }
    }
}

/// Why a scan could not be driven to completion.
#[derive(Debug)]
pub enum RuntimeError {
    /// The VM trapped during init or a scan round.
    Trap(FaultContext),
    /// `run` was requested while the host holds no execution permit. The
    /// host boots unpermitted and executes only after its composition root
    /// grants the permit — standalone shells at startup, a redundant unit
    /// on its admission verdict (V4018, the permit latch of the HA
    /// redundancy architecture).
    NotPermitted,
    /// A host invariant was violated. No input can reach this; it exists so
    /// the host can answer without panicking.
    Internal { reason: &'static str },
}

impl RuntimeError {
    /// Builds an internal-error value with the violated invariant.
    pub(crate) const fn internal(reason: &'static str) -> Self {
        RuntimeError::Internal { reason }
    }

    /// The stable V-code surfacing this error, when it has one.
    pub fn v_code(&self) -> Option<&'static str> {
        match self {
            RuntimeError::Trap(context) => Some(context.trap.v_code()),
            RuntimeError::NotPermitted => Some(problem_codes::EXECUTION_NOT_PERMITTED),
            RuntimeError::Internal { .. } => None,
        }
    }
}

impl fmt::Display for RuntimeError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            RuntimeError::Trap(context) => write!(
                f,
                "trap in task {} instance {}: {}",
                context.task_id, context.instance_id, context.trap
            ),
            RuntimeError::NotPermitted => write!(
                f,
                "scan execution refused: the host holds no execution permit"
            ),
            RuntimeError::Internal { reason } => {
                write!(f, "runtime host invariant violated: {reason}")
            }
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use ironplc_container::{InstanceId, TaskId};
    use ironplc_vm::error::Trap;

    #[test]
    fn online_change_error_display_when_variant_then_names_the_problem() {
        assert_eq!(
            OnlineChangeError::LayoutIncompatible.to_string(),
            "candidate layout is incompatible with the active application"
        );
        assert_eq!(
            OnlineChangeError::ScheduleIncompatible.to_string(),
            "candidate task schedule is incompatible with the active application"
        );
        assert_eq!(
            OnlineChangeError::IoIncompatible.to_string(),
            "candidate process-image sizes are incompatible with the active application"
        );
        assert_eq!(
            OnlineChangeError::MigrationUnsupported(MigrationError::FbLayoutUnsupported)
                .to_string(),
            "candidate state cannot be migrated: function-block instance layout changed and field UIDs cannot justify the change"
        );
        assert_eq!(
            OnlineChangeError::UntestUnsupported.to_string(),
            "untest and cancel are not supported after a schema-changing test; assemble instead"
        );
        assert_eq!(
            OnlineChangeError::NoCandidateStaged.to_string(),
            "no candidate is staged"
        );
        assert_eq!(
            OnlineChangeError::CandidateAlreadyStaged(None).to_string(),
            "a candidate is already staged"
        );
        assert_eq!(
            OnlineChangeError::NoTestInProgress.to_string(),
            "no test is in progress"
        );
        assert_eq!(
            OnlineChangeError::NotAllowedInThisMode.to_string(),
            "operation not allowed in the current hot-edit phase"
        );
        assert_eq!(
            OnlineChangeError::AssembleWithoutTest.to_string(),
            "assemble requires the candidate to have executed under Test"
        );
        assert_eq!(
            OnlineChangeError::SnapshotCorrupt.to_string(),
            "replicated state snapshot is internally inconsistent: lengths do not match the declared layout"
        );
    }

    #[test]
    fn runtime_error_display_when_trap_then_names_task_instance_and_trap() {
        let error = RuntimeError::Trap(FaultContext {
            trap: Trap::DivideByZero,
            task_id: TaskId::DEFAULT,
            instance_id: InstanceId::DEFAULT,
        });

        assert_eq!(
            error.to_string(),
            "trap in task 0 instance 0: divide by zero"
        );
    }

    #[test]
    fn runtime_error_display_when_not_permitted_then_names_the_missing_permit() {
        assert_eq!(
            RuntimeError::NotPermitted.to_string(),
            "scan execution refused: the host holds no execution permit"
        );
    }

    #[test]
    fn runtime_error_v_code_when_not_permitted_then_v4018() {
        assert_eq!(RuntimeError::NotPermitted.v_code(), Some("V4018"));
    }

    #[test]
    fn runtime_error_v_code_when_internal_then_none() {
        assert_eq!(RuntimeError::internal("candidate missing").v_code(), None);
    }

    #[test]
    fn runtime_error_display_when_internal_then_names_the_violated_invariant() {
        assert_eq!(
            RuntimeError::internal("candidate missing").to_string(),
            "runtime host invariant violated: candidate missing"
        );
    }
}
