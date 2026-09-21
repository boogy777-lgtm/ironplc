//! The HA redundancy shell above the IronPLC runtime host.
//!
//! This crate is the n+1 layer of the Phase 5 redundancy architecture
//! (`specs/design/ha-redundancy-layer-architecture.md`): it depends on
//! [`ironplc_runtime`] and the runtime never depends on it, so a standalone
//! controller ships zero redundancy. It owns the process-topology side of
//! "Shell, Not Runtime +1": startup ordering and the policy of *when* the
//! application may execute.
//!
//! Slice 1 (the foundation) carried the admission verdict and the
//! execution-permit policy. Slice 2 (the pair link) adds what a redundant
//! unit needs to find its peer and converge: the configured role and the
//! shell's configuration ([`config`]), neighbor discovery and the zombie
//! fence ([`admission`]), the `NicPort` transport seam and the loopback
//! simulator binding ([`hal`], [`loopback`]), the ping/pong liveness
//! exchange ([`liveness`]), the anti-stale epoch ([`epoch`]), and the
//! SYNC subchart ([`statechart`]). Slice 3 (crossload) carries the
//! ADR-0064 pair pipeline: the runtime's typed state snapshot and the
//! fail-closed idle apply (seams 3 and 4, reusing the runtime's staging),
//! the offer/response codec, and the readiness/alarm latch that keeps a
//! failing unit from pretending redundancy-ready ([`crossload`]). Slice 4
//! (fencing) carries the CONTROL subchart and the OWNERSHIP_BARRIER: the
//! fencing-client seam with its capability descriptor ([`fencing`]), the
//! simulator binding as a module registry with target-enforced
//! exclusivity ([`simulator`]), the OwnerLease minted at the scan-commit
//! seam ([`lease`]), and the CONTROL chart with the guard table, the
//! detection case table, and the coded alarms (V4106–V4109). The permit
//! itself is enforced inside
//! [`RuntimeHost`] — the host boots unpermitted and `run` refuses without
//! the permit (V4018) — and epochs and leases are minted at the host's
//! scan-commit callback; this crate is the policy authority that composes
//! both.
//!
//! Not here (later slices, per the architecture's module decomposition):
//! calibration and the engineering command vocabulary — the V-codes
//! registered so far are V4101–V4109.

mod admission;
mod config;
mod crossload;
mod epoch;
mod fencing;
mod hal;
mod lease;
mod liveness;
mod loopback;
mod simulator;
mod statechart;

// V-code constants are generated from resources/problem-codes.csv by build.rs.
mod problem_codes {
    include!(concat!(env!("OUT_DIR"), "/problem_codes.rs"));
}

pub use admission::{
    admit, classify, permit_for, AdmissionRefusal, AdmissionVerdict, Discovery, Neighbor,
};
pub use config::{ConfiguredRole, PairId, RedundancyConfig};
pub use crossload::{
    accept_offer, decode, encode, package_offer, CrossloadMessage, CrossloadOffer,
    CrossloadReceiver, CrossloadRefusal, FRAME_MAGIC,
};
pub use epoch::{Epoch, EpochAdoption};
pub use fencing::{
    claim_in_order, ownership_barrier, release_all, FencingCapabilities, FencingClient,
    FencingError, ModuleId, ModuleOwnership, ModuleState, ObserverCapability, OwnerId,
    OwnershipMode,
};
pub use hal::{IngressTimestamp, NicPort, PhyCounters, PortCapabilities, PortError};
pub use lease::OwnerLease;
pub use liveness::{Liveness, LivenessEvent, Packet, PairRole, FRAME_LEN};
pub use loopback::{loopback_pair, LoopbackPort};
pub use simulator::{ModuleRegistry, RegistryClient};
pub use statechart::{
    claim_barrier, detect, ClaimBarrier, ControlAlarm, ControlChart, ControlEvent, ControlState,
    CrossloadReadiness, DeSyncReason, DetectionAction, SyncChart, SyncEvent, SyncState,
};
