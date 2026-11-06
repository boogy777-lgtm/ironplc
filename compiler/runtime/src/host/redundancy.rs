//! The redundancy seams of the host: the apply of a replicated state snapshot,
//! the takeover of a candidate under test and the boundary of a monitor-mode
//! host that drives no scans.
//!
//! They are a child of [`super`] because they read and write the private
//! fields of the host that the edit operations own.

use ironplc_container::Container;
use ironplc_vm::{Vm, VmBuffers};

use super::{select_active, RuntimeHost};
use crate::error::{OnlineChangeError, RuntimeError};
use crate::snapshot::StateSnapshot;

impl RuntimeHost {
    /// Applies a replicated snapshot to this host while it drives no
    /// scans (the HA redundancy architecture, "Minimal Seams" 4): the
    /// mirror of `apply_migration_swap` with a replicated image in the
    /// migration plan's place. The host is the single authority; every
    /// refusal is coded and fail-closed — a mismatched image is refused,
    /// never guessed:
    ///
    /// - a snapshot whose byte lengths do not match its declared layout
    ///   is corrupt ([`OnlineChangeError::SnapshotCorrupt`]);
    /// - a snapshot of the layout currently executing copies both
    ///   persistent regions wholesale — the steady-state replication a
    ///   monitoring peer applies;
    /// - a snapshot of the staged candidate's layout while the original
    ///   is active is the pair's Test of a migration candidate arriving
    ///   over replication: the buffers are rebuilt from the candidate's
    ///   init image (exactly `apply_migration_swap`'s first half), every
    ///   persistent byte is overwritten with the replicated image, and
    ///   the selector flips to the candidate. The migration marker stays
    ///   set — untest stays refused (V4011) and the replicated image is
    ///   authoritative over a local plan application;
    /// - a snapshot of the normal layout while the candidate is active
    ///   is the pair's Untest: refused on a migration candidate
    ///   ([`OnlineChangeError::UntestUnsupported`]), otherwise the
    ///   selector flips back with the carried state (ADR-0064(f): the
    ///   candidate is kept, process state is not rolled back);
    /// - anything else names a layout this host does not hold:
    ///   [`OnlineChangeError::LayoutIncompatible`].
    ///
    /// "Idle" is a borrow-checker property: this method takes
    /// `&mut self`, so no scan session can be live across it.
    pub fn apply_state_snapshot(
        &mut self,
        snapshot: &StateSnapshot,
    ) -> Result<(), OnlineChangeError> {
        if !snapshot.is_consistent() {
            return Err(OnlineChangeError::SnapshotCorrupt);
        }
        if self.pending.is_some() {
            // A boundary operation is in flight; no replica may land
            // until it completes.
            return Err(OnlineChangeError::NotAllowedInThisMode);
        }

        let active = select_active(&self.normal, &self.candidate, self.active_is_candidate);
        if snapshot.layout_matches(active) {
            return Self::copy_persistent(snapshot, active, &mut self.buffers);
        }

        if !self.active_is_candidate
            && self
                .candidate
                .as_ref()
                .is_some_and(|candidate| snapshot.layout_matches(candidate))
        {
            return self.apply_advance(snapshot);
        }

        if self.active_is_candidate && snapshot.layout_matches(&self.normal) {
            if self.migration.is_some() {
                // Untest after a schema-changing test is forbidden
                // (V4011); a retreat image names exactly that.
                return Err(OnlineChangeError::UntestUnsupported);
            }
            Self::copy_persistent(snapshot, &self.normal, &mut self.buffers)?;
            self.active_is_candidate = false;
            return Ok(());
        }

        Err(OnlineChangeError::LayoutIncompatible)
    }

    /// Flips the execution selector to the staged candidate without a
    /// boundary state swap — the takeover-during-Testing path of
    /// ADR-0064(e): the standby takes over executing the CANDIDATE,
    /// because the state has already moved under the candidate and
    /// reverting to the original would be an untest, which migration
    /// candidates forbid. The caller certifies the replicated state
    /// moved: a migration candidate whose image has not advanced on this
    /// host is refused, never executed over unmigrated state
    /// ([`OnlineChangeError::NotAllowedInThisMode`]). Idempotent once
    /// the candidate is active.
    pub fn takeover_testing(&mut self) -> Result<(), OnlineChangeError> {
        if self.candidate.is_none() {
            return Err(OnlineChangeError::NoCandidateStaged);
        }
        if self.active_is_candidate {
            return Ok(());
        }
        if self.pending.is_some() {
            return Err(OnlineChangeError::NotAllowedInThisMode);
        }
        if self.migration.is_some() {
            return Err(OnlineChangeError::NotAllowedInThisMode);
        }
        self.active_is_candidate = true;
        Ok(())
    }

    /// Applies a pending `test`/`untest` at a boundary without driving
    /// scan rounds. The monitor-mode host of a redundant pair drives no
    /// scans — its composition root keeps it idle — so its boundary is
    /// wherever the root chooses; this is how the Secondary mirrors the
    /// pair's Untest for a layout-preserving candidate (both layouts hash
    /// identically, so the retreat never appears in the replication
    /// stream). The execution permit is not re-validated here: this path
    /// drives no scans, and refusing the mirror would leave the pair's
    /// edit lifecycle diverged. Shells that drive rounds never need this:
    /// `run` applies pending swaps at round boundaries with the permit
    /// re-check (scenario T07).
    pub fn apply_pending_at_boundary(&mut self) -> Result<(), RuntimeError> {
        self.apply_pending_swap(false)
    }

    /// Overwrites the persistent extent of `buffers`, which `container`
    /// laid out, with the bytes of the snapshot. The caller has already proven
    /// the layout identity; the buffers may be larger than the image says (a
    /// body that needs more working slots or working bytes than the one that
    /// produced it), and the working part is left as it is.
    fn copy_persistent(
        snapshot: &StateSnapshot,
        container: &Container,
        buffers: &mut VmBuffers,
    ) -> Result<(), OnlineChangeError> {
        if snapshot.write_to(container, buffers) {
            Ok(())
        } else {
            Err(OnlineChangeError::SnapshotCorrupt)
        }
    }

    /// The replicated Test advance of a migration candidate: rebuild the
    /// buffers from the candidate's init image (its declared initial
    /// values), then let the replicated image overwrite every persistent
    /// byte — the candidate-only entities keep the init values the
    /// primary's post-migration image carries, the shared entities carry
    /// the replicated values. An init image that traps here — when the
    /// identical container initialized on the peer — means the replica
    /// is divergent, so it is refused as corrupt.
    fn apply_advance(&mut self, snapshot: &StateSnapshot) -> Result<(), OnlineChangeError> {
        let Some(candidate) = self.candidate.as_ref() else {
            return Err(OnlineChangeError::NoCandidateStaged);
        };
        let mut advanced = {
            let mut advanced = VmBuffers::from_container(candidate);
            Vm::new()
                .load(candidate, &mut advanced)
                .map_err(|_| OnlineChangeError::SnapshotCorrupt)?
                .start()
                .map_err(|_| OnlineChangeError::SnapshotCorrupt)?;
            advanced
        };
        Self::copy_persistent(snapshot, candidate, &mut advanced)?;
        self.buffers = advanced;
        self.active_is_candidate = true;
        Ok(())
    }
}
