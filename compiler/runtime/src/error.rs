//! Errors reported by the runtime host.

use core::fmt;

use ironplc_vm::FaultContext;

use crate::migration::MigrationError;

/// Why a candidate was rejected or an online change request was refused.
///
/// These are host-level protocol errors; the VM traps live in
/// [`RuntimeError`]. The command layer maps this enum onto stable user-facing
/// V-codes ([`CommandError`](crate::CommandError)) from the crate's
/// `problem-codes.csv`, so this enum stays the whole host vocabulary.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
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
    /// `untest` was called after a schema-changing test. Writes made under
    /// the candidate's layout have no reverse mapping for added or removed
    /// variables, so the change can only be assembled or cancelled while the
    /// original is active.
    UntestUnsupported,
    /// The requested operation needs a staged candidate and none is staged.
    NoCandidateStaged,
    /// `stage` was called while a candidate is already staged.
    CandidateAlreadyStaged,
    /// `untest` was called while the original (not the candidate) is active.
    NoTestInProgress,
    /// The operation is not allowed in the current hot-edit phase, e.g.
    /// staging another candidate while testing.
    NotAllowedInThisMode,
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
                "untest is not supported after a schema-changing test; assemble or cancel instead"
            ),
            OnlineChangeError::NoCandidateStaged => write!(f, "no candidate is staged"),
            OnlineChangeError::CandidateAlreadyStaged => {
                write!(f, "a candidate is already staged")
            }
            OnlineChangeError::NoTestInProgress => write!(f, "no test is in progress"),
            OnlineChangeError::NotAllowedInThisMode => {
                write!(f, "operation not allowed in the current hot-edit phase")
            }
        }
    }
}

/// Why a scan could not be driven to completion.
#[derive(Debug)]
pub enum RuntimeError {
    /// The VM trapped during init or a scan round.
    Trap(FaultContext),
    /// A host invariant was violated. No input can reach this; it exists so
    /// the host can answer without panicking.
    Internal { reason: &'static str },
}

impl RuntimeError {
    /// Builds an internal-error value with the violated invariant.
    pub(crate) const fn internal(reason: &'static str) -> Self {
        RuntimeError::Internal { reason }
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
            "untest is not supported after a schema-changing test; assemble or cancel instead"
        );
        assert_eq!(
            OnlineChangeError::NoCandidateStaged.to_string(),
            "no candidate is staged"
        );
        assert_eq!(
            OnlineChangeError::CandidateAlreadyStaged.to_string(),
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
    fn runtime_error_display_when_internal_then_names_the_violated_invariant() {
        assert_eq!(
            RuntimeError::internal("candidate missing").to_string(),
            "runtime host invariant violated: candidate missing"
        );
    }
}
