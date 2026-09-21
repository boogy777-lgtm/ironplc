//! The HA redundancy shell above the IronPLC runtime host.
//!
//! This crate is the n+1 layer of the Phase 5 redundancy architecture
//! (`specs/design/ha-redundancy-layer-architecture.md`): it depends on
//! [`ironplc_runtime`] and the runtime never depends on it, so a standalone
//! controller ships zero redundancy. It owns the process-topology side of
//! "Shell, Not Runtime +1": startup ordering and the policy of *when* the
//! application may execute.
//!
//! Slice 1 (the foundation) carries only the admission verdict and the
//! execution-permit policy it implies. The permit itself is enforced inside
//! [`RuntimeHost`] — the host boots unpermitted and `run` refuses without
//! the permit (V4018) — so bypass is impossible at the type level; this
//! crate is the policy authority that maps an admission verdict onto the
//! host's grant API. Neighbor discovery, the SYNC/CONTROL statechart,
//! crossload, fencing, and calibration land in their own modules later,
//! per the architecture's module decomposition.
//!
//! The engineering command vocabulary and its crate-local V41xx codes
//! arrive with the engineering surface
//! (`specs/design/ha-engineering-ui.md`); until then this crate registers
//! no codes of its own.

mod admission;

pub use admission::{permit_for, AdmissionVerdict};
