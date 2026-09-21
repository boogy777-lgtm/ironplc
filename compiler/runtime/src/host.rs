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
//! fresh and initialized by the candidate's init image, then the migration
//! plan copies the values of the entities both containers share. A migration candidate
//! has no untest path: writes made under the candidate's layout have no
//! reverse mapping for added or removed variables, so it can only be
//! assembled or discarded while the original is active.
//!
//! `run` never holds a [`VmRunning`](ironplc_vm::VmRunning) borrow across a
//! mutation of the host fields: the VM borrows the active container and the
//! buffers for the duration of one session of rounds, and is dropped before
//! a pending swap is applied. That is what makes the two-container design
//! expressible in safe Rust without `unsafe`.

use std::collections::BTreeMap;

use ironplc_container::{Container, VarIndex};
use ironplc_vm::{Vm, VmBuffers};

use crate::error::{OnlineChangeError, RuntimeError};
use crate::generation::{ApplicationGeneration, LogicGeneration};
use crate::migration::{MigrationDecision, StateMigrationPlan};
use crate::online_change::{
    has_stable_vars, swap_buffers, validate_candidate, validate_migration_candidate,
};

/// Which artifact is executing.
#[derive(Clone, Copy, Debug, PartialEq, Eq, serde::Serialize, serde::Deserialize)]
#[serde(rename_all = "camelCase")]
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

/// The accept-supplied extras of one `AcceptEdits` command (ADR-0064): the
/// candidate's exact wire bytes — kept for the assemble commit, never
/// re-serialized — and the optional edit identity for the pending record.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct AcceptedEdit {
    /// The candidate's wire bytes exactly as the client sent them.
    pub wire: Vec<u8>,
    /// The client-supplied edit label, when the accept named one.
    pub name: Option<String>,
    /// An unauthenticated advisory client label; not engineer identity
    /// (ADR-0065 decision 2).
    pub origin: Option<String>,
}

/// The baseline an accepted edit staged against (ADR-0064 debt closure): the
/// normal artifact's identity at Accept.
#[derive(Clone, Debug, PartialEq, Eq, serde::Serialize, serde::Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct EditBaseline {
    /// The normal artifact's generation at Accept.
    pub normal_generation: u32,
    /// The normal artifact's content hash at Accept.
    pub content_hash: [u8; 32],
}

/// The controller-side pending-edit record (ADR-0064 debt closure): RAM-only
/// metadata written at Accept beside the candidate, cleared exactly where the
/// candidate dies (Assemble, Cancel). A reboot discards it with the candidate.
#[derive(Clone, Debug, PartialEq, Eq, serde::Serialize, serde::Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct PendingEditRecord {
    /// The client-supplied edit label, when the accept named one.
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub name: Option<String>,
    /// The unauthenticated advisory client label, when the accept carried one.
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub origin: Option<String>,
    /// Device timestamp (milliseconds since the Unix epoch) at Accept.
    pub accepted_at: u64,
    /// The normal artifact's identity at Accept.
    pub baseline: EditBaseline,
}

/// Identity of one committed scan boundary, handed to the scan-commit
/// callback (the HA redundancy architecture, "Minimal Seams" 2; the
/// OwnerLease minting point of ADR-0062 — the lease is born at scan
/// commit, never in the network task).
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct ScanCommit {
    /// Completed scan rounds since the host was created — the boundary
    /// identity the redundancy layer stamps into what it mints here.
    pub rounds: u64,
    /// Which artifact executed the committed round.
    pub mode: HostMode,
    /// Generation of the artifact that executed the round.
    pub generation: LogicGeneration,
    /// Generation of the active application manifest.
    pub application: ApplicationGeneration,
}

/// Snapshot of the host's hot-edit state.
#[derive(Clone, Debug, PartialEq, Eq)]
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
    /// The pending-edit record written at Accept, if a candidate is staged.
    pub pending_edit: Option<PendingEditRecord>,
}

/// Owns the active artifact, a staged candidate, and the VM's state.
pub struct RuntimeHost {
    normal: Container,
    candidate: Option<Container>,
    candidate_wire: Option<Vec<u8>>,
    committed_wire: Option<Vec<u8>>,
    pending_edit: Option<PendingEditRecord>,
    active_is_candidate: bool,
    buffers: VmBuffers,
    rounds: u64,
    normal_generation: LogicGeneration,
    candidate_generation: Option<LogicGeneration>,
    next_logic_generation: u32,
    application_generation: ApplicationGeneration,
    pending: Option<PendingSwap>,
    migration: Option<StateMigrationPlan>,
    execution_permitted: bool,
}

impl RuntimeHost {
    /// Creates a host for `container` and runs its init functions once.
    ///
    /// The returned host is ready for scans once permitted: the VM is
    /// loaded, initialized, and dropped, so every initialized variable
    /// lives in the host's buffers. Later scans resume without re-running
    /// init. The host boots without the execution permit — the composition
    /// root grants it via [`permit_execution`](Self::permit_execution)
    /// before the first `run`.
    pub fn new(container: Container) -> Result<Self, RuntimeError> {
        let mut buffers = VmBuffers::from_container(&container);
        Vm::new()
            .load(&container, &mut buffers)
            .start()
            .map_err(RuntimeError::Trap)?;

        Ok(RuntimeHost {
            normal: container,
            candidate: None,
            candidate_wire: None,
            committed_wire: None,
            pending_edit: None,
            active_is_candidate: false,
            buffers,
            rounds: 0,
            normal_generation: LogicGeneration::new(1),
            candidate_generation: None,
            next_logic_generation: 2,
            application_generation: ApplicationGeneration::new(1),
            pending: None,
            migration: None,
            execution_permitted: false,
        })
    }

    /// Grants the execution permit: the host may drive scan rounds.
    ///
    /// This is the grant half of the execution permit latch (the HA
    /// redundancy architecture, "Minimal Seams" 1): the host boots
    /// unpermitted and `run` refuses to scan without the permit, so the
    /// composition root owns the policy of *when* execution may begin —
    /// standalone binaries grant immediately at startup, the redundancy
    /// shell grants on an admission verdict. Granting is idempotent.
    pub fn permit_execution(&mut self) {
        self.execution_permitted = true;
    }

    /// Revokes the execution permit: further `run` requests refuse, and a
    /// pending swap requested before the revocation is cancelled at the
    /// boundary instead of being applied.
    pub fn revoke_execution_permit(&mut self) {
        self.execution_permitted = false;
    }

    /// Stages `candidate` after validating it against the normal artifact.
    ///
    /// Equivalent to [`stage_with_decisions`](Self::stage_with_decisions)
    /// with an empty decision map: an out-of-policy type change is refused
    /// with every offender named.
    pub fn stage(&mut self, candidate: Container) -> Result<(), OnlineChangeError> {
        self.stage_with_decisions(candidate, &BTreeMap::new(), None)
    }

    /// Stages `candidate` after validating it against the normal artifact,
    /// resolving out-of-policy type changes with the engineer's `decisions`
    /// (ADR 0061).
    ///
    /// This is the controller-side `Accept`: the candidate is present and
    /// validated, the normal artifact keeps executing. A candidate whose
    /// layout hash matches is validated as a logic-only change. A candidate
    /// whose hash differs is staged as a migration candidate when both
    /// artifacts carry stable variable IDs and the migration planner can
    /// justify every copy — admitted conversions (ADR 0060) or a per-UID
    /// decision; otherwise it is rejected and the running application is
    /// untouched. An unknown decision UID, or `preserve` on a size-mismatched
    /// pair, is rejected by the planner.
    ///
    /// `edit` carries the accept-supplied extras (ADR-0064): the candidate's
    /// exact wire bytes are kept beside the parsed `Container` for the
    /// assemble commit, and the optional identity labels become the
    /// pending-edit record. Pass `None` to stage without an edit provenance
    /// (no wire bytes retained, no record written).
    pub fn stage_with_decisions(
        &mut self,
        candidate: Container,
        decisions: &BTreeMap<u64, MigrationDecision>,
        edit: Option<AcceptedEdit>,
    ) -> Result<(), OnlineChangeError> {
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
                StateMigrationPlan::build_with_decisions(&self.normal, &candidate, decisions)
                    .map_err(OnlineChangeError::MigrationUnsupported)?,
            )
        } else {
            // No stable variable IDs on at least one side means there is
            // nothing to migrate by; a safe rejection, never a guess.
            return Err(OnlineChangeError::LayoutIncompatible);
        };

        let generation = LogicGeneration::new(self.next_logic_generation);
        self.next_logic_generation = self.next_logic_generation.saturating_add(1);
        let (wire, name, origin) = match edit {
            Some(edit) => (Some(edit.wire), edit.name, edit.origin),
            None => (None, None, None),
        };
        self.candidate = Some(candidate);
        self.candidate_wire = wire;
        self.candidate_generation = Some(generation);
        self.migration = migration;
        self.pending_edit = Some(PendingEditRecord {
            name,
            origin,
            accepted_at: now_millis(),
            baseline: EditBaseline {
                normal_generation: self.normal_generation.raw(),
                content_hash: self.normal.header.content_hash,
            },
        });
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
    /// Allowed only from `Testing`: the candidate must have executed under
    /// Test before it can become canonical (ADR-0064). Assembling from
    /// mere `Accepted` is refused with [`OnlineChangeError::AssembleWithoutTest`]
    /// — a commit that never ran is unverified code promoted to canonical.
    /// The buffers are not rebuilt: the layout is compatible by construction.
    ///
    /// The promotion also moves the candidate's retained wire bytes to the
    /// committed latch for the shell to persist (see
    /// [`take_committed_wire`](Self::take_committed_wire)).
    pub fn assemble(&mut self) -> Result<(), OnlineChangeError> {
        if self.candidate.is_none() {
            return Err(OnlineChangeError::NoCandidateStaged);
        }
        if self.pending.is_some() {
            return Err(OnlineChangeError::NotAllowedInThisMode);
        }
        if !self.active_is_candidate {
            return Err(OnlineChangeError::AssembleWithoutTest);
        }

        if let Some(next) = self.candidate.take() {
            self.normal = next;
            self.committed_wire = self.candidate_wire.take();
            self.pending_edit = None;
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
        self.candidate_wire = None;
        self.pending_edit = None;
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
            pending_edit: self.pending_edit.clone(),
        }
    }

    /// Drains the committed-wire latch (ADR-0064 amendment): the exact bytes
    /// the client sent for the candidate, moved here by `assemble`. Returns
    /// `None` when nothing committed since the last drain. The host owns WHEN
    /// commit bytes exist; the shell owns HOW they persist.
    pub fn take_committed_wire(&mut self) -> Option<Vec<u8>> {
        self.committed_wire.take()
    }

    /// Drives up to `rounds` scan rounds, applying a pending swap first.
    ///
    /// Each round is one `run_round(clock())`. A pending `test`/`untest`
    /// request taken at entry is applied at the boundary before the first
    /// round; if a request appears between rounds, the session stops at the
    /// next boundary and the outer loop applies it before continuing.
    ///
    /// Scans execute only while the host holds the execution permit. The
    /// boundary application of a pending swap is the single re-check point
    /// (external FSM review, takeaway 1): a permit revoked between the
    /// request and the boundary cancels the operation terminally — the
    /// pending swap is consumed there and never applied.
    pub fn run(&mut self, rounds: u64, clock: impl FnMut() -> u64) -> Result<(), RuntimeError> {
        self.run_with_commit(rounds, clock, |_| {})
    }

    /// Drives rounds like [`run`](Self::run) and invokes `on_commit` once
    /// per completed scan boundary, with that boundary's identity.
    ///
    /// This is the scan-commit notification seam (the HA redundancy
    /// architecture, "Minimal Seams" 2): the composition root composes what
    /// a committed boundary means — standalone shells pass no-op, the
    /// redundancy shell mints the epoch (and later the OwnerLease) here,
    /// never in a network task (ADR-0062). One notification per committed
    /// round, after the round's state is committed; a round that traps
    /// notifies nothing.
    pub fn run_with_commit(
        &mut self,
        rounds: u64,
        mut clock: impl FnMut() -> u64,
        mut on_commit: impl FnMut(ScanCommit),
    ) -> Result<(), RuntimeError> {
        let mut remaining = rounds;
        while remaining > 0 {
            if self.pending.is_some() {
                self.apply_pending_swap()?;
            }
            if !self.execution_permitted {
                return Err(RuntimeError::NotPermitted);
            }
            remaining = self.run_session(remaining, &mut clock, &mut on_commit)?;
        }
        Ok(())
    }

    /// Runs one session of rounds against the active artifact.
    ///
    /// The [`VmRunning`](ironplc_vm::VmRunning) borrow lives only inside
    /// this method, so it can never overlap a container or buffer mutation
    /// in the host. Returns the rounds not yet executed.
    ///
    /// The boundary identity is captured before the VM borrow and the
    /// rounds counter advances in a local: the callback runs while the VM
    /// holds `buffers`, so no `&self` method may run inside the loop.
    fn run_session(
        &mut self,
        mut remaining: u64,
        clock: &mut impl FnMut() -> u64,
        on_commit: &mut impl FnMut(ScanCommit),
    ) -> Result<u64, RuntimeError> {
        let (container, mode, generation) = if self.active_is_candidate {
            let Some(candidate) = self.candidate.as_ref() else {
                return Err(RuntimeError::internal(
                    "candidate active without a staged candidate",
                ));
            };
            (
                candidate,
                HostMode::Testing,
                self.candidate_generation.unwrap_or(self.normal_generation),
            )
        } else {
            (&self.normal, HostMode::Normal, self.normal_generation)
        };
        let application = self.application_generation;

        let mut completed = self.rounds;
        let mut vm = Vm::new()
            .load(container, &mut self.buffers)
            .resume(completed);
        while remaining > 0 {
            vm.run_round(clock()).map_err(RuntimeError::Trap)?;
            remaining -= 1;
            completed += 1;
            on_commit(ScanCommit {
                rounds: completed,
                mode,
                generation,
                application,
            });
            if self.pending.is_some() {
                break;
            }
        }
        self.rounds = completed;
        Ok(remaining)
    }

    /// Applies a pending swap at a scan boundary.
    ///
    /// Re-validates the execution permit before applying: a permit revoked
    /// between the request and the boundary cancels the operation with a
    /// terminal result — the pending swap is consumed above and never
    /// applied (external FSM review, scenario T07).
    fn apply_pending_swap(&mut self) -> Result<(), RuntimeError> {
        let Some(swap) = self.pending.take() else {
            return Ok(());
        };
        if !self.execution_permitted {
            return Err(RuntimeError::NotPermitted);
        }
        match swap {
            PendingSwap::Test => {
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
            PendingSwap::Untest => {
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
    /// UID carry the candidate's declared initial values. The migration plan
    /// then copies every entity the two layouts share, and the old buffers are
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

/// The device timestamp in milliseconds since the Unix epoch, for the
/// pending-edit record's `accepted_at` (ADR-0064 debt closure). A clock set
/// before the epoch yields 0 rather than failing the accept.
fn now_millis() -> u64 {
    std::time::SystemTime::now()
        .duration_since(std::time::UNIX_EPOCH)
        .map(|duration| duration.as_millis() as u64)
        .unwrap_or(0)
}
