//! The fencing-client seam: the target-enforced ownership authority
//! behind the OWNERSHIP_BARRIER.
//!
//! Fencing is the one genuinely protocol-bound concern of the redundancy
//! layer (`specs/design/ha-redundancy-layer-architecture.md`, "Protocol
//! Portability"): everything else survives a protocol swap unchanged, and
//! the fencing authority binds behind this declared seam — one interface
//! plus a capability descriptor that records the guarantee level the
//! binding can honestly prove. EtherNet/IP (Exclusive Owner + Input Only)
//! is one later binding; the [`crate::simulator`] registry is the
//! first-class simulator binding this slice proves the seam with. A swap
//! changes the binding and the descriptor, never the FSM.
//!
//! The interface is deliberately four operations — claim, release,
//! query-owners, barrier-participate — matching the FSM spec's connection
//! vocabulary (v1: Exclusive Owner for outputs, an independent Input Only
//! observer; a non-ACTIVE unit never uses Listen Only, because Listen Only
//! depends on an existing owner and would mask peer loss, defeating the
//! `I` signal). Ownership is exclusive at the target: a claim on a module
//! a conflicting owner holds is rejected with [`FencingError::OwnerConflict`]
//! — the partition evidence that keeps a live Primary's outputs fenced.
//!
//! The barrier helpers are the OWNERSHIP_BARRIER procedure of the FSM
//! spec and the architecture doc's fencing module decomposition ("ordered
//! claim, CLAIMED_DISARMED verification, ARM, barrier rollback"), composed
//! over the four operations: claim every required module in the fixed
//! configured order, then the all-or-nothing
//! [`FencingClient::barrier_participate`] (verify each module is
//! claimed-disarmed by this owner in this epoch, then ARM all). Any
//! acquisition or verification failure releases everything acquired
//! before the error returns — partial ownership never survives a failed
//! barrier, and partial ownership never means `ACTIVE`.

use crate::epoch::Epoch;
use crate::problem_codes;

/// Identifies one I/O module of the fenced set, in the fixed configured
/// claim order (the barrier claims modules in slice order of the list the
/// caller passes, which is the configured order).
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct ModuleId(u16);

impl ModuleId {
    /// Creates a module identity from its raw value.
    pub const fn new(raw: u16) -> Self {
        Self(raw)
    }

    /// Returns the raw value.
    pub const fn raw(self) -> u16 {
        self.0
    }
}

impl core::fmt::Display for ModuleId {
    fn fmt(&self, f: &mut core::fmt::Formatter<'_>) -> core::fmt::Result {
        write!(f, "{}", self.0)
    }
}

/// The permanent identity of one controller as a fencing originator. A
/// unit presents the same owner identity for every claim of its lifetime;
/// everything dynamic (which modules, which epoch, armed or not) lives in
/// the per-module [`ModuleState`].
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct OwnerId(u64);

impl OwnerId {
    /// Creates an owner identity from its raw value.
    pub const fn new(raw: u64) -> Self {
        Self(raw)
    }

    /// Returns the raw value.
    pub const fn raw(self) -> u64 {
        self.0
    }
}

impl core::fmt::Display for OwnerId {
    fn fmt(&self, f: &mut core::fmt::Formatter<'_>) -> core::fmt::Result {
        write!(f, "{}", self.0)
    }
}

/// The per-module ownership truth of the fencing target: unowned,
/// claimed-disarmed (exclusive owner acquired, outputs not yet
/// commanded), or armed (output-controlling). Each held state is stamped
/// with the owning originator and the epoch the ownership was taken in —
/// the epoch the cyclic owner status reports (ADR-0062's ownership view).
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum ModuleState {
    /// No originator holds the module.
    Unowned,
    /// Exclusive owner acquired and verified, outputs disarmed — the
    /// CLAIMED_DISARMED staging state between claim and ARM.
    ClaimedDisarmed {
        /// The originator holding the exclusive claim.
        owner: OwnerId,
        /// The ownership epoch the claim was taken in.
        epoch: Epoch,
    },
    /// Exclusive owner armed: the holder commands the module's outputs
    /// under the stamped epoch.
    Armed {
        /// The originator commanding the outputs.
        owner: OwnerId,
        /// The ownership epoch the outputs are commanded under.
        epoch: Epoch,
    },
}

impl ModuleState {
    /// The owning originator, if any originator holds the module.
    pub const fn owner(self) -> Option<OwnerId> {
        match self {
            ModuleState::Unowned => None,
            ModuleState::ClaimedDisarmed { owner, .. } | ModuleState::Armed { owner, .. } => {
                Some(owner)
            }
        }
    }

    /// Whether the module is armed (output-controlling).
    pub const fn is_armed(self) -> bool {
        matches!(self, ModuleState::Armed { .. })
    }
}

/// One entry of the query-owners view: a module and its ownership truth.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct ModuleOwnership {
    /// The module this entry reports.
    pub module: ModuleId,
    /// The module's ownership state.
    pub state: ModuleState,
}

/// Why a fencing operation failed — the refusal vocabulary the CONTROL
/// chart surfaces as coded alarms (see [`crate::ControlAlarm`]).
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum FencingError {
    /// The claim was rejected: a conflicting originator still holds the
    /// module's exclusive ownership (V4106). The partition evidence — a
    /// live Primary behind a network partition still owns its outputs,
    /// so the barrier must retreat, never share.
    OwnerConflict,
    /// The operation named a module this originator does not hold (a
    /// driver sequencing error, not a fencing refusal: the driver owns
    /// its call order).
    NotOwned,
    /// The module is offline or faulted (a barrier verification failure
    /// path; surfaces as V4107 through the CONTROL chart).
    Unavailable,
}

impl FencingError {
    /// The stable V-code surfacing this failure on the HA command
    /// surface (the crate-local CSV), when the failure is a fencing
    /// refusal the engineering surface raises.
    pub const fn v_code(self) -> &'static str {
        match self {
            FencingError::OwnerConflict => problem_codes::OWNER_CONFLICT,
            FencingError::NotOwned | FencingError::Unavailable => {
                problem_codes::OWNERSHIP_BARRIER_FAILED
            }
        }
    }
}

/// The guarantee level a binding records about its fencing authority
/// (the architecture doc's capability descriptor: "a capability
/// descriptor travels with the binding and records the guarantee
/// level"). A protocol swap changes these fields — and the engineering
/// surface must show the difference — never the FSM.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct FencingCapabilities {
    /// How output ownership is arbitrated at the target.
    pub ownership_mode: OwnershipMode,
    /// What a non-ACTIVE unit can observe without owning.
    pub observer: ObserverCapability,
    /// The binding supports the staged CLAIMED_DISARMED claim (acquire
    /// now, ARM later at the barrier).
    pub staged_claim: bool,
    /// The binding supports an explicit ARM step distinct from claim.
    pub explicit_arm: bool,
    /// Ownership is stamped with and checked against the pair epoch.
    pub epoch_support: bool,
    /// The cyclic input carries explicit owner status (owner, epoch,
    /// armed) the observer can read.
    pub cyclic_owner_status: bool,
    /// Protocol-specific degradation this binding records (for example
    /// EtherCAT's missing per-slave ownership), when the guarantee level
    /// degrades below full target-enforced exclusivity.
    pub degradation: Option<&'static str>,
}

/// How output ownership is arbitrated at the target.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum OwnershipMode {
    /// One exclusive owner per output module at a time; a conflicting
    /// claim is rejected while the owner lives (v1 EtherNet/IP
    /// Exclusive Owner; the simulator binding).
    SingleExclusive,
    /// Both pair units hold preconnected owner-class connections and the
    /// target arbitrates between them (dual-owner vendor modules; out of
    /// v1 scope, recorded so a binding can declare it).
    RedundantPair,
}

/// What a non-ACTIVE unit can observe without owning.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum ObserverCapability {
    /// An independent Input Only connection stands alone — the observer
    /// sees the peer's outputs without depending on its connection (the
    /// v1 requirement: the `I` signal must survive peer loss).
    IndependentInputOnly,
    /// Listen Only, which depends on an existing owner — masks peer loss,
    /// so a binding declaring this records the degraded `I` signal.
    ListenOnly,
    /// The binding offers no observer connections.
    None,
}

/// The fencing authority as the redundancy layer sees it: the client of
/// the target-enforced exclusivity that makes the OWNERSHIP_BARRIER real
/// instead of aspirational. Implementations are per-protocol bindings
/// ([`crate::simulator`] is the simulator binding); the redundancy layer
/// never touches a network stack through this seam — the v1 realization
/// is a binding detail, and no real network I/O happens in this slice.
pub trait FencingClient {
    /// The guarantee level this binding records.
    fn capabilities(&self) -> FencingCapabilities;

    /// Claims one module exclusively for `owner` in `epoch`: on success
    /// the module is claimed-disarmed; a conflicting live owner rejects
    /// the claim with [`FencingError::OwnerConflict`]. Re-claiming a
    /// module this originator already holds refreshes the claim.
    fn claim(&mut self, module: ModuleId, owner: OwnerId, epoch: Epoch)
        -> Result<(), FencingError>;

    /// Releases one module held by `owner` back to unowned.
    fn release(&mut self, module: ModuleId, owner: OwnerId) -> Result<(), FencingError>;

    /// The query-owners view: every module and its ownership truth — the
    /// ownership status surface of ADR-0062 (OwnerState, OwnerEpoch,
    /// OwnerArmed), and the fail-closed check behind "partial ownership
    /// never means ACTIVE".
    fn owners(&self) -> Vec<ModuleOwnership>;

    /// The all-or-nothing barrier: verifies every listed module is
    /// claimed-disarmed by `owner` in exactly `epoch`, then arms them
    /// all. Any mismatch arms nothing and returns the failure — the
    /// caller retreats (release-all), per the FSM spec's "one failure
    /// releases everything".
    fn barrier_participate(
        &mut self,
        owner: OwnerId,
        modules: &[ModuleId],
        epoch: Epoch,
    ) -> Result<(), FencingError>;
}

/// Claims every module of `modules` in order for `owner` in `epoch`,
/// pushing each acquired module onto `acquired`. Any failure releases
/// everything acquired and returns the error — the ordered-claim half of
/// the OWNERSHIP_BARRIER.
pub fn claim_in_order(
    client: &mut impl FencingClient,
    owner: OwnerId,
    modules: &[ModuleId],
    epoch: Epoch,
    acquired: &mut Vec<ModuleId>,
) -> Result<(), FencingError> {
    for &module in modules {
        match client.claim(module, owner, epoch) {
            Ok(()) => acquired.push(module),
            Err(error) => {
                release_all(client, owner, acquired);
                return Err(error);
            }
        }
    }
    Ok(())
}

/// Releases every module of `modules` held by `owner`, best effort: a
/// rollback cannot fail — a release that finds the module already gone
/// (faulted, aged out, never acquired) is the desired end state.
pub fn release_all(client: &mut impl FencingClient, owner: OwnerId, modules: &[ModuleId]) {
    for &module in modules {
        let _ = client.release(module, owner);
    }
}

/// The OWNERSHIP_BARRIER of the FSM spec: the ordered exclusive claim of
/// every required module, then the all-or-nothing barrier participation
/// (verify + ARM). Any acquisition or verification failure releases
/// everything acquired before the error returns — on success every module
/// is armed by `owner` in `epoch`, on failure nothing is.
pub fn ownership_barrier(
    client: &mut impl FencingClient,
    owner: OwnerId,
    modules: &[ModuleId],
    epoch: Epoch,
) -> Result<(), FencingError> {
    let mut acquired = Vec::new();
    claim_in_order(client, owner, modules, epoch, &mut acquired)?;
    match client.barrier_participate(owner, modules, epoch) {
        Ok(()) => Ok(()),
        Err(error) => {
            release_all(client, owner, &acquired);
            Err(error)
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::simulator::{ModuleRegistry, RegistryClient};

    const OWNER_A: OwnerId = OwnerId::new(1);
    const OWNER_B: OwnerId = OwnerId::new(2);
    const EPOCH: Epoch = Epoch::new(4);

    /// A registry of three modules split into the two units' handles.
    fn registry() -> (RegistryClient, RegistryClient, ModuleRegistry) {
        let registry = ModuleRegistry::new(3);
        (registry.client(), registry.client(), registry)
    }

    #[test]
    fn claim_when_free_then_claimed_disarmed_with_owner_and_epoch() {
        let (mut a, _, _) = registry();

        a.claim(ModuleId::new(0), OWNER_A, EPOCH).unwrap();

        let owners = a.owners();
        assert_eq!(
            owners[0].state,
            ModuleState::ClaimedDisarmed {
                owner: OWNER_A,
                epoch: EPOCH
            }
        );
        assert!(!owners[0].state.is_armed());
        assert_eq!(owners[0].state.owner(), Some(OWNER_A));
    }

    #[test]
    fn claim_when_conflicting_owner_then_rejected_with_v4106() {
        let (mut a, mut b, _) = registry();
        a.claim(ModuleId::new(0), OWNER_A, EPOCH).unwrap();

        let error = b.claim(ModuleId::new(0), OWNER_B, EPOCH).unwrap_err();

        assert_eq!(error, FencingError::OwnerConflict);
        assert_eq!(error.v_code(), "V4106");
        // The rejected claimant holds nothing.
        assert!(b
            .owners()
            .iter()
            .all(|entry| entry.state.owner() != Some(OWNER_B)));
    }

    #[test]
    fn release_when_owned_then_unowned() {
        let (mut a, _, _) = registry();
        a.claim(ModuleId::new(1), OWNER_A, EPOCH).unwrap();

        a.release(ModuleId::new(1), OWNER_A).unwrap();

        assert_eq!(a.owners()[1].state, ModuleState::Unowned);
        assert_eq!(a.owners()[1].state.owner(), None);
    }

    #[test]
    fn release_when_not_owned_then_not_owned_error() {
        let (mut a, _, _) = registry();

        let error = a.release(ModuleId::new(1), OWNER_A).unwrap_err();

        assert_eq!(error, FencingError::NotOwned);
        assert_eq!(error.v_code(), "V4107");
    }

    #[test]
    fn barrier_when_all_claimed_then_arms_every_module() {
        let (mut a, _, _) = registry();
        let modules = [ModuleId::new(0), ModuleId::new(1), ModuleId::new(2)];
        claim_in_order(&mut a, OWNER_A, &modules, EPOCH, &mut Vec::new()).unwrap();

        a.barrier_participate(OWNER_A, &modules, EPOCH).unwrap();

        for entry in a.owners() {
            assert_eq!(
                entry.state,
                ModuleState::Armed {
                    owner: OWNER_A,
                    epoch: EPOCH
                }
            );
        }
    }

    #[test]
    fn barrier_when_module_not_claimed_then_arms_nothing() {
        let (mut a, _, _) = registry();
        let modules = [ModuleId::new(0), ModuleId::new(1)];
        a.claim(modules[0], OWNER_A, EPOCH).unwrap();

        let error = a.barrier_participate(OWNER_A, &modules, EPOCH).unwrap_err();

        assert_eq!(error, FencingError::NotOwned);
        // All-or-nothing: the claimed module stays disarmed, not armed.
        assert_eq!(
            a.owners()[0].state,
            ModuleState::ClaimedDisarmed {
                owner: OWNER_A,
                epoch: EPOCH
            }
        );
        assert_eq!(a.owners()[1].state, ModuleState::Unowned);
    }

    #[test]
    fn barrier_when_epoch_mismatches_then_arms_nothing() {
        let (mut a, _, _) = registry();
        let modules = [ModuleId::new(0)];
        a.claim(modules[0], OWNER_A, EPOCH).unwrap();

        let error = a
            .barrier_participate(OWNER_A, &modules, EPOCH.next())
            .unwrap_err();

        assert_eq!(error, FencingError::NotOwned);
        assert!(!a.owners()[0].state.is_armed());
    }

    #[test]
    fn ownership_barrier_when_mid_claim_conflicts_then_releases_acquired() {
        let (mut a, mut b, _) = registry();
        let modules = [ModuleId::new(0), ModuleId::new(1), ModuleId::new(2)];
        b.claim(modules[1], OWNER_B, EPOCH).unwrap();

        let error = ownership_barrier(&mut a, OWNER_A, &modules, EPOCH).unwrap_err();

        assert_eq!(error, FencingError::OwnerConflict);
        // Safe retreat: the claimant holds nothing, the conflicting
        // owner is untouched — zero partial ownership.
        assert!(a
            .owners()
            .iter()
            .all(|entry| entry.state.owner() != Some(OWNER_A)));
        assert_eq!(
            a.owners()[1].state,
            ModuleState::ClaimedDisarmed {
                owner: OWNER_B,
                epoch: EPOCH
            }
        );
        assert_eq!(a.owners()[0].state, ModuleState::Unowned);
        assert_eq!(a.owners()[2].state, ModuleState::Unowned);
    }

    #[test]
    fn ownership_barrier_when_verification_fails_then_releases_everything() {
        let (mut a, _, mut registry) = registry();
        let modules = [ModuleId::new(0), ModuleId::new(1), ModuleId::new(2)];
        let mut acquired = Vec::new();
        claim_in_order(&mut a, OWNER_A, &modules, EPOCH, &mut acquired).unwrap();
        // A module faults between the claim and the barrier.
        registry.yank(modules[1]);

        let error = a.barrier_participate(OWNER_A, &modules, EPOCH).unwrap_err();

        assert_eq!(error, FencingError::Unavailable);
        release_all(&mut a, OWNER_A, &acquired);
        assert!(a
            .owners()
            .iter()
            .all(|entry| entry.state.owner() != Some(OWNER_A)));
    }

    #[test]
    fn capabilities_when_simulator_binding_then_records_guarantee_level() {
        let (a, _, _) = registry();

        let capabilities = a.capabilities();

        assert_eq!(capabilities.ownership_mode, OwnershipMode::SingleExclusive);
        assert_eq!(
            capabilities.observer,
            ObserverCapability::IndependentInputOnly
        );
        assert!(capabilities.staged_claim);
        assert!(capabilities.explicit_arm);
        assert!(capabilities.epoch_support);
        assert!(!capabilities.cyclic_owner_status);
        assert_eq!(capabilities.degradation, None);
    }
}
