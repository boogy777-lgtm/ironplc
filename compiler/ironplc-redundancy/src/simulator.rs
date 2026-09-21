//! The fencing simulator binding: a module registry with target-enforced
//! exclusivity — the first-class test vehicle for every fencing scenario.
//!
//! The architecture doc mandates a simulator binding from the start
//! ("Portability is unproven until a second binding exists"): it gives
//! the CONTROL chart, the barrier, and the partition scenarios CI
//! coverage before any field device is attached, and it turns a protocol
//! swap into a binding addition. This registry *is* the I/O target's
//! admission truth: each module carries owner/ARM state
//! ([`ModuleState`]) and rejects a conflicting claim while a live owner
//! holds it — the fence that decides a partition in favor of the living
//! owner. No real network I/O happens here; the EtherNet/IP binding of
//! the same [`FencingClient`] seam is a later deliverable.
//!
//! The binding is single-threaded: the two units of a test share one
//! registry through `Rc<RefCell<..>>` client handles (the
//! [`crate::loopback`] binding's shape), and a driver steps both units
//! plus the registry in one thread. `tick` advances the registry's
//! abstract clock; the ACTIVE unit's scan-commit callback stamps its
//! armed modules through [`commit_outputs`](ModuleRegistry::commit_outputs),
//! which is the outputs-change truth behind the detection case table's
//! `I` signal ("input data keeps changing ... attributable to the peer's
//! ownership epoch"). `age_out` models the target's old-connection
//! timeout (the failover formula's `T_old-connection-timeout` term): an
//! owner that stopped renewing its presence loses its connections without
//! a release ever arriving — the honest model of a dead owner's modules.

use std::collections::BTreeMap;
use std::sync::{Arc, Mutex};

use crate::epoch::Epoch;
use crate::fencing::{
    FencingCapabilities, FencingClient, FencingError, ModuleId, ModuleOwnership, ModuleState,
    ObserverCapability, OwnerId, OwnershipMode,
};

/// One module of the registry: the ownership truth plus the observation
/// state the `I` signal consumes.
#[derive(Clone, Copy, Debug)]
struct Module {
    state: ModuleState,
    /// The registry clock value when the armed owner last committed
    /// outputs; `None` until the first commit.
    last_commit: Option<u64>,
    /// A faulted module owns nothing and rejects every operation.
    online: bool,
    /// The firmware-reported timing of this device (claim, ARM,
    /// output-apply delays): ADR-0062 — "I/O firmware instruments its
    /// own delays and reports them; the PLC measures peer-detection".
    timing: ModuleTiming,
}

impl Module {
    fn new() -> Self {
        Self {
            state: ModuleState::Unowned,
            last_commit: None,
            online: true,
            timing: ModuleTiming::ZERO,
        }
    }
}

/// A module's firmware-reported delays in abstract clock ticks
/// (ADR-0062: the I/O firmware instruments its own delays and reports
/// them upward; the calibration run samples them into the per-module
/// T-contributions). The simulator's device property standing in for
/// that firmware instrumentation.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub struct ModuleTiming {
    /// The claim processing delay (`T_claim,i`).
    pub claim_ticks: u64,
    /// The ARM-to-output delay (`T_arm,i`).
    pub arm_ticks: u64,
    /// The output-apply delay (`T_output-apply,i`).
    pub output_apply_ticks: u64,
}

impl ModuleTiming {
    /// A device whose operations complete within the abstract step
    /// (no reported delay).
    pub const ZERO: Self = Self {
        claim_ticks: 0,
        arm_ticks: 0,
        output_apply_ticks: 0,
    };
}

/// The shared state behind [`ModuleRegistry`] and its client handles.
#[derive(Debug)]
struct RegistryInner {
    modules: BTreeMap<ModuleId, Module>,
    /// The abstract clock, advanced by the driver's `tick`.
    now: u64,
    /// How many ticks an armed owner may go without committing outputs
    /// before the target ages its connections out (the
    /// old-connection timeout).
    connection_timeout: u64,
}

/// The simulator's I/O network: every module the pair fences against.
/// One registry is shared between the two units of a test; each unit
/// drives its claims through its own [`RegistryClient`] handle.
#[derive(Clone, Debug)]
pub struct ModuleRegistry {
    inner: Arc<Mutex<RegistryInner>>,
}

impl ModuleRegistry {
    /// Creates a registry of `count` modules, identities `0..count`, all
    /// unowned and online. Connections never age until
    /// [`with_connection_timeout`](Self::with_connection_timeout) sets
    /// the target's old-connection timeout.
    pub fn new(count: u16) -> Self {
        let modules = (0..count)
            .map(ModuleId::new)
            .map(|id| (id, Module::new()))
            .collect();
        Self {
            inner: Arc::new(Mutex::new(RegistryInner {
                modules,
                now: 0,
                connection_timeout: u64::MAX,
            })),
        }
    }

    /// Sets the target's old-connection timeout (abstract ticks): an
    /// armed owner that commits nothing for this long loses its
    /// connections at the next tick, without a release ever arriving —
    /// the autonomous target behavior behind the failover formula's
    /// `T_old-connection-timeout` term.
    pub fn with_connection_timeout(self, timeout: u64) -> Self {
        self.inner
            .lock()
            .unwrap_or_else(|err| err.into_inner())
            .connection_timeout = timeout;
        self
    }

    /// The target's old-connection timeout in abstract ticks
    /// (`u64::MAX` when never set): the I/O owner-lease expiry of
    /// ADR-0062's claim-start rule, `T_claim-start =
    /// max(T_plc-peer-detection, T_io-owner-lease-expiry)`.
    pub fn connection_timeout(&self) -> u64 {
        self.inner
            .lock()
            .unwrap_or_else(|err| err.into_inner())
            .connection_timeout
    }

    /// Reports this module's firmware-instrumented delays (ADR-0062):
    /// the device property the calibration run samples into the
    /// per-module T-contributions. A module identity outside the
    /// registry is unknown hardware: `None`.
    pub fn module_timing(&self, module: ModuleId) -> Option<ModuleTiming> {
        self.inner
            .lock()
            .unwrap_or_else(|err| err.into_inner())
            .modules
            .get(&module)
            .map(|entry| entry.timing)
    }

    /// Sets the firmware-reported delays of one module (the simulator's
    /// stand-in for the I/O firmware's own instrumentation). An unknown
    /// module identity is ignored, mirroring [`yank`](Self::yank).
    pub fn with_module_timing(self, module: ModuleId, timing: ModuleTiming) -> Self {
        if let Some(entry) = self
            .inner
            .lock()
            .unwrap_or_else(|err| err.into_inner())
            .modules
            .get_mut(&module)
        {
            entry.timing = timing;
        }
        self
    }

    /// A client handle for one unit of the pair: claims, releases, and
    /// queries flow through it tagged with the unit's [`OwnerId`].
    pub fn client(&self) -> RegistryClient {
        RegistryClient {
            inner: Arc::clone(&self.inner),
        }
    }

    /// Advances the registry's abstract clock to `now` (monotonic by the
    /// driver's construction) and ages out every connection whose armed
    /// owner committed nothing within the connection timeout — the
    /// target's autonomous old-connection timeout.
    pub fn tick(&mut self, now: u64) {
        let mut inner = self.inner.lock().unwrap_or_else(|err| err.into_inner());
        inner.now = now;
        let timeout = inner.connection_timeout;
        for module in inner.modules.values_mut() {
            let silent = module
                .last_commit
                .is_some_and(|last| now.saturating_sub(last) > timeout);
            if silent && module.state.is_armed() {
                module.state = ModuleState::Unowned;
                module.last_commit = None;
            }
        }
    }

    /// Stamps the armed modules of `owner` as freshly committed — the
    /// ACTIVE unit's scan-commit callback calls this each driven round,
    /// keeping its outputs "changing" for its peer's Input Only
    /// observation.
    pub fn commit_outputs(&mut self, owner: OwnerId) {
        let mut inner = self.inner.lock().unwrap_or_else(|err| err.into_inner());
        let now = inner.now;
        for module in inner.modules.values_mut() {
            let armed_by_owner = matches!(
                module.state,
                ModuleState::Armed { owner: held, .. } if held == owner
            );
            if module.online && armed_by_owner {
                module.last_commit = Some(now);
            }
        }
    }

    /// Whether any module armed by `owner` shows outputs that changed
    /// within the last `window` clock ticks — the `I` signal's
    /// "input data keeps changing".
    pub fn outputs_changing(&self, owner: OwnerId, window: u64) -> bool {
        let inner = self.inner.lock().unwrap_or_else(|err| err.into_inner());
        inner.modules.values().any(|module| {
            module.state.owner() == Some(owner)
                && module
                    .last_commit
                    .is_some_and(|last| inner.now.saturating_sub(last) <= window)
        })
    }

    /// The query-owners view of every module, in identity order.
    pub fn owners(&self) -> Vec<ModuleOwnership> {
        self.inner
            .lock()
            .unwrap_or_else(|err| err.into_inner())
            .modules
            .iter()
            .map(|(&module, entry)| ModuleOwnership {
                module,
                state: entry.state,
            })
            .collect()
    }

    /// Whether a module is powered and communicating. A faulted module
    /// is offline: it owns nothing and rejects every operation — the
    /// observation behind the IO_READY "required inputs observable"
    /// input and the driver's fencing-loss detection.
    pub fn is_online(&self, module: ModuleId) -> bool {
        self.inner
            .lock()
            .unwrap_or_else(|err| err.into_inner())
            .modules
            .get(&module)
            .is_some_and(|entry| entry.online)
    }

    /// Models the target's old-connection timeout for a dead owner:
    /// every module `owner` holds becomes unowned without a release
    /// arriving — what the fencing target does while the failover formula
    /// pays `T_old-connection-timeout`.
    pub fn age_out(&mut self, owner: OwnerId) {
        for module in self
            .inner
            .lock()
            .unwrap_or_else(|err| err.into_inner())
            .modules
            .values_mut()
        {
            if module.state.owner() == Some(owner) {
                module.state = ModuleState::Unowned;
                module.last_commit = None;
            }
        }
    }

    /// Faults a module offline: it owns nothing afterwards and rejects
    /// every operation (the barrier verification failure path).
    pub fn yank(&mut self, module: ModuleId) {
        let mut inner = self.inner.lock().unwrap_or_else(|err| err.into_inner());
        let Some(entry) = inner.modules.get_mut(&module) else {
            return;
        };
        entry.state = ModuleState::Unowned;
        entry.last_commit = None;
        entry.online = false;
    }
}

/// One unit's handle to the shared [`ModuleRegistry`]: the
/// [`FencingClient`] the CONTROL chart drives.
#[derive(Clone, Debug)]
pub struct RegistryClient {
    inner: Arc<Mutex<RegistryInner>>,
}

impl FencingClient for RegistryClient {
    fn capabilities(&self) -> FencingCapabilities {
        // The memory binding records exactly the guarantee level it can
        // honestly prove; a protocol swap changes this descriptor, never
        // the FSM.
        FencingCapabilities {
            ownership_mode: OwnershipMode::SingleExclusive,
            observer: ObserverCapability::IndependentInputOnly,
            staged_claim: true,
            explicit_arm: true,
            epoch_support: true,
            cyclic_owner_status: false,
            degradation: None,
        }
    }

    fn claim(
        &mut self,
        module: ModuleId,
        owner: OwnerId,
        epoch: Epoch,
    ) -> Result<(), FencingError> {
        let mut inner = self.inner.lock().unwrap_or_else(|err| err.into_inner());
        let Some(entry) = inner.modules.get_mut(&module) else {
            return Err(FencingError::Unavailable);
        };
        if !entry.online {
            return Err(FencingError::Unavailable);
        }
        match entry.state {
            ModuleState::Unowned => {
                entry.state = ModuleState::ClaimedDisarmed { owner, epoch };
                Ok(())
            }
            // Re-claiming a module this originator holds refreshes the
            // staged claim (a boot re-run after a half-lost barrier).
            ModuleState::ClaimedDisarmed { owner: held, .. } if held == owner => {
                entry.state = ModuleState::ClaimedDisarmed { owner, epoch };
                Ok(())
            }
            ModuleState::ClaimedDisarmed { .. } | ModuleState::Armed { .. } => {
                Err(FencingError::OwnerConflict)
            }
        }
    }

    fn release(&mut self, module: ModuleId, owner: OwnerId) -> Result<(), FencingError> {
        let mut inner = self.inner.lock().unwrap_or_else(|err| err.into_inner());
        let Some(entry) = inner.modules.get_mut(&module) else {
            return Err(FencingError::Unavailable);
        };
        if !entry.online {
            return Err(FencingError::Unavailable);
        }
        match entry.state {
            ModuleState::ClaimedDisarmed { owner: held, .. }
            | ModuleState::Armed { owner: held, .. }
                if held == owner =>
            {
                entry.state = ModuleState::Unowned;
                entry.last_commit = None;
                Ok(())
            }
            ModuleState::Unowned
            | ModuleState::ClaimedDisarmed { .. }
            | ModuleState::Armed { .. } => Err(FencingError::NotOwned),
        }
    }

    fn owners(&self) -> Vec<ModuleOwnership> {
        self.inner
            .lock()
            .unwrap_or_else(|err| err.into_inner())
            .modules
            .iter()
            .map(|(&module, entry)| ModuleOwnership {
                module,
                state: entry.state,
            })
            .collect()
    }

    fn barrier_participate(
        &mut self,
        owner: OwnerId,
        modules: &[ModuleId],
        epoch: Epoch,
    ) -> Result<(), FencingError> {
        let mut inner = self.inner.lock().unwrap_or_else(|err| err.into_inner());
        // Verify first, ARM second — all or nothing: any mismatch leaves
        // every module in its pre-barrier state.
        for &module in modules {
            let Some(entry) = inner.modules.get(&module) else {
                return Err(FencingError::Unavailable);
            };
            let held_disarmed = matches!(
                entry.state,
                ModuleState::ClaimedDisarmed { owner: held, epoch: held_epoch }
                    if held == owner && held_epoch == epoch
            );
            if !held_disarmed {
                return Err(if !entry.online {
                    FencingError::Unavailable
                } else {
                    match entry.state {
                        ModuleState::ClaimedDisarmed { owner: held, .. }
                        | ModuleState::Armed { owner: held, .. }
                            if held != owner =>
                        {
                            FencingError::OwnerConflict
                        }
                        _ => FencingError::NotOwned,
                    }
                });
            }
        }
        let now = inner.now;
        for &module in modules {
            let Some(entry) = inner.modules.get_mut(&module) else {
                return Err(FencingError::Unavailable);
            };
            entry.state = ModuleState::Armed { owner, epoch };
            entry.last_commit = Some(now);
        }
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    const OWNER: OwnerId = OwnerId::new(7);
    const EPOCH: Epoch = Epoch::new(2);

    #[test]
    fn claim_when_module_faulted_then_unavailable() {
        let mut registry = ModuleRegistry::new(2);
        let mut client = registry.client();
        registry.yank(ModuleId::new(1));

        let error = client.claim(ModuleId::new(1), OWNER, EPOCH).unwrap_err();

        assert_eq!(error, FencingError::Unavailable);
        assert_eq!(error.v_code(), "V4107");
    }

    #[test]
    fn operations_when_module_out_of_range_then_unavailable() {
        // The registry knows identities `0..count`; a foreign module
        // identity is unknown hardware, refused fail-closed.
        let mut registry = ModuleRegistry::new(2);
        let mut client = registry.client();
        let foreign = ModuleId::new(99);

        assert_eq!(
            client.claim(foreign, OWNER, EPOCH).unwrap_err(),
            FencingError::Unavailable
        );
        assert_eq!(
            client.release(foreign, OWNER).unwrap_err(),
            FencingError::Unavailable
        );
        assert_eq!(
            client
                .barrier_participate(OWNER, &[foreign], EPOCH)
                .unwrap_err(),
            FencingError::Unavailable
        );
        registry.yank(foreign);
        assert!(registry
            .owners()
            .iter()
            .all(|entry| entry.module != foreign));
    }

    #[test]
    fn release_when_faulted_after_claim_then_unavailable_and_unowned() {
        let mut registry = ModuleRegistry::new(2);
        let mut client = registry.client();
        client.claim(ModuleId::new(0), OWNER, EPOCH).unwrap();
        registry.yank(ModuleId::new(0));

        let error = client.release(ModuleId::new(0), OWNER).unwrap_err();

        assert_eq!(error, FencingError::Unavailable);
        assert_eq!(registry.owners()[0].state, ModuleState::Unowned);
    }

    #[test]
    fn age_out_when_owner_dies_then_modules_unowned_without_release() {
        let mut registry = ModuleRegistry::new(2);
        let mut client = registry.client();
        let modules = [ModuleId::new(0), ModuleId::new(1)];
        crate::fencing::ownership_barrier(&mut client, OWNER, &modules, EPOCH).unwrap();

        registry.age_out(OWNER);

        assert!(registry
            .owners()
            .iter()
            .all(|entry| entry.state == ModuleState::Unowned));
    }

    #[test]
    fn outputs_changing_when_owner_commits_then_true_until_window_passes() {
        let mut registry = ModuleRegistry::new(1);
        let mut client = registry.client();
        let modules = [ModuleId::new(0)];
        crate::fencing::ownership_barrier(&mut client, OWNER, &modules, EPOCH).unwrap();

        registry.tick(10);
        registry.commit_outputs(OWNER);
        assert!(registry.outputs_changing(OWNER, 2));

        registry.tick(12);
        assert!(registry.outputs_changing(OWNER, 2));
        registry.tick(13);
        assert!(!registry.outputs_changing(OWNER, 2));
    }

    #[test]
    fn outputs_changing_when_peer_claims_but_never_arms_then_false() {
        // A staged (disarmed) claim commands no outputs: no I/O evidence
        // flows from it — the `I` signal is the armed owner's alone.
        let mut registry = ModuleRegistry::new(1);
        let mut client = registry.client();
        client.claim(ModuleId::new(0), OWNER, EPOCH).unwrap();

        registry.tick(10);
        registry.commit_outputs(OWNER);

        assert!(!registry.outputs_changing(OWNER, 100));
    }

    #[test]
    fn module_timing_when_configured_then_reports_firmware_delays() {
        let timing = ModuleTiming {
            claim_ticks: 3,
            arm_ticks: 1,
            output_apply_ticks: 2,
        };
        let registry = ModuleRegistry::new(2).with_module_timing(ModuleId::new(1), timing);

        assert_eq!(registry.module_timing(ModuleId::new(1)), Some(timing));
        // Unconfigured modules report zero delay; unknown identities
        // report nothing.
        assert_eq!(
            registry.module_timing(ModuleId::new(0)),
            Some(ModuleTiming::ZERO)
        );
        assert_eq!(registry.module_timing(ModuleId::new(99)), None);
    }

    #[test]
    fn module_timing_when_unknown_module_then_ignored() {
        let registry =
            ModuleRegistry::new(1).with_module_timing(ModuleId::new(99), ModuleTiming::ZERO);

        assert_eq!(registry.module_timing(ModuleId::new(99)), None);
    }

    #[test]
    fn connection_timeout_when_never_set_then_never_ages() {
        let registry = ModuleRegistry::new(1);

        assert_eq!(registry.connection_timeout(), u64::MAX);
    }

    #[test]
    fn is_online_when_faulted_then_false() {
        let mut registry = ModuleRegistry::new(2);
        registry.yank(ModuleId::new(1));

        assert!(registry.is_online(ModuleId::new(0)));
        assert!(!registry.is_online(ModuleId::new(1)));
        // An unknown module identity is unknown hardware: fail-closed
        // offline, exactly like the operation refusals.
        assert!(!registry.is_online(ModuleId::new(99)));
    }
}
