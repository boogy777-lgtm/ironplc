//! The runtime host: owns the running application and performs online change.
//!
//! The host is the seam between the VM's typestate API and a scan loop. It
//! owns the active [`Container`], the staged candidate, the VM's
//! [`VmBuffers`], and the generation counters.
//!
//! Hot edit follows the controller FSM of the baseline for P0:
//!
//! ```text
//! NORMAL(A) --stage B--> ACCEPTED(A,B) --test--> TESTING(A,B)
//!                          |    ^                     |
//!                   cancel |    | untest              | assemble
//!                          v    |                     v
//!                      NORMAL(A)                  NORMAL(B)
//! ```
//!
//! A `test` or `untest` request does not touch the buffers when it is made:
//! it records a pending swap that [`RuntimeHost::run`] applies at the next
//! scan boundary. Code changes; state does not. Untest switches executable
//! logic back to the original artifact while the process state stays
//! current (the hard baseline invariant).
//!
//! A candidate whose layout hash differs is a *migration candidate*: it is
//! staged with a [`StateMigrationPlan`] that rebuilds the persistent state
//! per stable variable ID (ADR 0053) at the boundary. Its buffers are built
//! fresh and initialized by the candidate's init image, then the plan copies
//! the values of the entities both containers share. A migration candidate
//! has no untest path: writes made under the candidate's layout have no
//! reverse mapping for added or removed variables, so it can only be
//! assembled or discarded while the original is active.
//!
//! `run` never holds a [`VmRunning`](ironplc_vm::VmRunning) borrow across a
//! mutation of the host fields: the VM borrows the active container and the
//! buffers for the duration of one session of rounds, and is dropped before
//! a pending swap is applied. That is what makes the two-container design
//! expressible in safe Rust without `unsafe`.

use ironplc_container::{Container, VarIndex};
use ironplc_vm::{Vm, VmBuffers};

use crate::error::{OnlineChangeError, RuntimeError};
use crate::generation::{ApplicationGeneration, LogicGeneration};
use crate::migration::StateMigrationPlan;
use crate::online_change::{
    has_stable_vars, swap_buffers, validate_candidate, validate_migration_candidate,
};

/// Which artifact is executing.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum HostMode {
    /// The normal (original) artifact is active.
    Normal,
    /// The staged candidate is active; the original is the untest fallback.
    Testing,
}

/// A swap requested by `test` or `untest`, applied at the next boundary.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum PendingSwap {
    Test,
    Untest,
}

/// Snapshot of the host's hot-edit state.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct HostStatus {
    /// Generation of the artifact currently executing.
    pub active: LogicGeneration,
    /// Generation of the normal (original) artifact.
    pub normal: LogicGeneration,
    /// Generation of the staged candidate, if one is staged.
    pub candidate: Option<LogicGeneration>,
    /// Generation of the active application manifest.
    pub application: ApplicationGeneration,
    /// Whether the normal artifact or the candidate is executing.
    pub mode: HostMode,
    /// Whether the staged candidate is a migration candidate, i.e. one whose
    /// layout hash differs and which carries a state migration plan.
    pub migration: bool,
    /// Completed scan rounds since the host was created.
    pub rounds: u64,
}

/// Owns the active artifact, a staged candidate, and the VM's state.
pub struct RuntimeHost {
    normal: Container,
    candidate: Option<Container>,
    active_is_candidate: bool,
    buffers: VmBuffers,
    rounds: u64,
    normal_generation: LogicGeneration,
    candidate_generation: Option<LogicGeneration>,
    next_logic_generation: u32,
    application_generation: ApplicationGeneration,
    pending: Option<PendingSwap>,
    migration: Option<StateMigrationPlan>,
}

impl RuntimeHost {
    /// Creates a host for `container` and runs its init functions once.
    ///
    /// The returned host is ready for scans: the VM is loaded, initialized,
    /// and dropped, so every initialized variable lives in the host's
    /// buffers. Later scans resume without re-running init.
    pub fn new(container: Container) -> Result<Self, RuntimeError> {
        let mut buffers = VmBuffers::from_container(&container);
        Vm::new()
            .load(&container, &mut buffers)
            .start()
            .map_err(RuntimeError::Trap)?;

        Ok(RuntimeHost {
            normal: container,
            candidate: None,
            active_is_candidate: false,
            buffers,
            rounds: 0,
            normal_generation: LogicGeneration::new(1),
            candidate_generation: None,
            next_logic_generation: 2,
            application_generation: ApplicationGeneration::new(1),
            pending: None,
            migration: None,
        })
    }

    /// Stages `candidate` after validating it against the normal artifact.
    ///
    /// This is the controller-side `Accept`: the candidate is present and
    /// validated, the normal artifact keeps executing. A candidate whose
    /// layout hash matches is validated as a logic-only change. A candidate
    /// whose hash differs is staged as a migration candidate when both
    /// artifacts carry stable variable IDs and the migration planner can
    /// justify every copy; otherwise it is rejected and the running
    /// application is untouched.
    pub fn stage(&mut self, candidate: Container) -> Result<(), OnlineChangeError> {
        if self.candidate.is_some() {
            return Err(OnlineChangeError::CandidateAlreadyStaged);
        }
        if self.active_is_candidate || self.pending.is_some() {
            return Err(OnlineChangeError::NotAllowedInThisMode);
        }

        let migration = if self.normal.header.layout_hash == candidate.header.layout_hash {
            validate_candidate(&self.normal, &candidate)?;
            None
        } else if has_stable_vars(&self.normal) && has_stable_vars(&candidate) {
            validate_migration_candidate(&self.normal, &candidate)?;
            Some(
                StateMigrationPlan::build(&self.normal, &candidate)
                    .map_err(OnlineChangeError::MigrationUnsupported)?,
            )
        } else {
            // No stable variable IDs on at least one side means there is
            // nothing to migrate by; a safe rejection, never a guess.
            return Err(OnlineChangeError::LayoutIncompatible);
        };

        let generation = LogicGeneration::new(self.next_logic_generation);
        self.next_logic_generation = self.next_logic_generation.saturating_add(1);
        self.candidate = Some(candidate);
        self.candidate_generation = Some(generation);
        self.migration = migration;
        Ok(())
    }

    /// Requests that the candidate become active at the next scan boundary.
    pub fn test(&mut self) -> Result<(), OnlineChangeError> {
        if self.candidate.is_none() {
            return Err(OnlineChangeError::NoCandidateStaged);
        }
        if self.active_is_candidate || self.pending.is_some() {
            return Err(OnlineChangeError::NotAllowedInThisMode);
        }
        self.pending = Some(PendingSwap::Test);
        Ok(())
    }

    /// Requests that the normal artifact become active at the next boundary.
    ///
    /// Reverting switches executable logic only: the buffers keep the
    /// current process state.
    ///
    /// A migration candidate has no revert path: its buffers were rebuilt
    /// under a new layout, so `untest` is refused with
    /// [`OnlineChangeError::UntestUnsupported`] and the state is left
    /// unchanged. Assemble or cancel while the original is active instead.
    pub fn untest(&mut self) -> Result<(), OnlineChangeError> {
        if self.candidate.is_none() || !self.active_is_candidate {
            return Err(OnlineChangeError::NoTestInProgress);
        }
        if self.pending.is_some() {
            return Err(OnlineChangeError::NotAllowedInThisMode);
        }
        if self.migration.is_some() {
            return Err(OnlineChangeError::UntestUnsupported);
        }
        self.pending = Some(PendingSwap::Untest);
        Ok(())
    }

    /// Promotes the candidate to the normal artifact and drops the old one.
    ///
    /// Allowed from `Accepted` (the candidate never ran) and from `Testing`
    /// (the candidate is running and simply stops being optional). The
    /// buffers are not rebuilt: the layout is compatible by construction.
    pub fn assemble(&mut self) -> Result<(), OnlineChangeError> {
        if self.candidate.is_none() {
            return Err(OnlineChangeError::NoCandidateStaged);
        }
        if self.pending.is_some() {
            return Err(OnlineChangeError::NotAllowedInThisMode);
        }

        if let Some(next) = self.candidate.take() {
            self.normal = next;
            if let Some(generation) = self.candidate_generation.take() {
                self.normal_generation = generation;
            }
            self.active_is_candidate = false;
            self.migration = None;
            self.application_generation =
                ApplicationGeneration::new(self.application_generation.raw().saturating_add(1));
        }
        Ok(())
    }

    /// Discards the staged candidate while the normal artifact is active.
    pub fn cancel(&mut self) -> Result<(), OnlineChangeError> {
        if self.candidate.is_none() {
            return Err(OnlineChangeError::NoCandidateStaged);
        }
        if self.active_is_candidate || self.pending.is_some() {
            return Err(OnlineChangeError::NotAllowedInThisMode);
        }
        self.candidate = None;
        self.candidate_generation = None;
        self.migration = None;
        Ok(())
    }

    /// Returns the host's current hot-edit state.
    pub fn status(&self) -> HostStatus {
        HostStatus {
            active: if self.active_is_candidate {
                self.candidate_generation.unwrap_or(self.normal_generation)
            } else {
                self.normal_generation
            },
            normal: self.normal_generation,
            candidate: self.candidate_generation,
            application: self.application_generation,
            mode: if self.active_is_candidate {
                HostMode::Testing
            } else {
                HostMode::Normal
            },
            migration: self.migration.is_some(),
            rounds: self.rounds,
        }
    }

    /// Drives up to `rounds` scan rounds, applying a pending swap first.
    ///
    /// Each round is one `run_round(clock())`. A pending `test`/`untest`
    /// request taken at entry is applied at the boundary before the first
    /// round; if a request appears between rounds, the session stops at the
    /// next boundary and the outer loop applies it before continuing.
    pub fn run(&mut self, rounds: u64, mut clock: impl FnMut() -> u64) -> Result<(), RuntimeError> {
        let mut remaining = rounds;
        while remaining > 0 {
            if self.pending.is_some() {
                self.apply_pending_swap()?;
            }
            remaining = self.run_session(remaining, &mut clock)?;
        }
        Ok(())
    }

    /// Runs one session of rounds against the active artifact.
    ///
    /// The [`VmRunning`](ironplc_vm::VmRunning) borrow lives only inside
    /// this method, so it can never overlap a container or buffer mutation
    /// in the host. Returns the rounds not yet executed.
    fn run_session(
        &mut self,
        mut remaining: u64,
        clock: &mut impl FnMut() -> u64,
    ) -> Result<u64, RuntimeError> {
        let container: &Container = if self.active_is_candidate {
            let Some(candidate) = self.candidate.as_ref() else {
                return Err(RuntimeError::internal(
                    "candidate active without a staged candidate",
                ));
            };
            candidate
        } else {
            &self.normal
        };

        let mut vm = Vm::new()
            .load(container, &mut self.buffers)
            .resume(self.rounds);
        while remaining > 0 {
            vm.run_round(clock()).map_err(RuntimeError::Trap)?;
            remaining -= 1;
            self.rounds += 1;
            if self.pending.is_some() {
                break;
            }
        }
        Ok(remaining)
    }

    /// Applies a pending swap at a scan boundary.
    fn apply_pending_swap(&mut self) -> Result<(), RuntimeError> {
        match self.pending.take() {
            None => Ok(()),
            Some(PendingSwap::Test) => {
                if self.migration.is_some() {
                    self.apply_migration_swap()?;
                } else {
                    let Some(candidate) = self.candidate.as_ref() else {
                        return Err(RuntimeError::internal(
                            "swap to candidate without a staged candidate",
                        ));
                    };
                    swap_buffers(candidate, &mut self.buffers, self.rounds);
                }
                self.active_is_candidate = true;
                Ok(())
            }
            Some(PendingSwap::Untest) => {
                swap_buffers(&self.normal, &mut self.buffers, self.rounds);
                self.active_is_candidate = false;
                Ok(())
            }
        }
    }

    /// Adopts the migration candidate's freshly initialized buffers with the
    /// plan applied.
    ///
    /// The candidate's init image runs once here, so entities with no source
    /// UID carry the candidate's declared initial values. The plan then
    /// copies every entity the two layouts share, and the old buffers are
    /// dropped: a schema change has no untest path.
    fn apply_migration_swap(&mut self) -> Result<(), RuntimeError> {
        let (Some(candidate), Some(plan)) = (self.candidate.as_ref(), self.migration.as_ref())
        else {
            return Err(RuntimeError::internal(
                "migration swap without a staged candidate or plan",
            ));
        };

        let mut migrated = VmBuffers::from_container(candidate);
        Vm::new()
            .load(candidate, &mut migrated)
            .start()
            .map_err(RuntimeError::Trap)?;

        plan.apply(&self.buffers, &mut migrated)
            .map_err(|_| RuntimeError::internal("state migration failed at the scan boundary"))?;

        self.buffers = migrated;
        Ok(())
    }

    /// Reads a variable value as an i32 from the host's buffers.
    pub fn read_variable(&self, index: VarIndex) -> Result<i32, RuntimeError> {
        let slot = self
            .buffers
            .vars
            .get(index.raw() as usize)
            .ok_or_else(|| RuntimeError::internal("variable index out of range"))?;
        Ok(slot.as_i32())
    }

    /// Returns the data region that backs STRING and WSTRING values.
    pub fn data_region(&self) -> &[u8] {
        &self.buffers.data_region
    }
}
