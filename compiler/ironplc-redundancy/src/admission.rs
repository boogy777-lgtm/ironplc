//! Admission: the verdict that decides whether this unit may execute,
//! decided before the application starts.
//!
//! A controller does not know a priori whether it runs standalone or as one
//! unit of a redundant pair; the redundancy layer decides **before** the
//! application starts and grants the application permission to start
//! (`specs/design/ha-redundancy-fsm.md`, "Admission"). This module carries
//! the discovery classification, the verdict, and the permit policy it
//! implies; the neighbor discovery that *produces* the classification runs
//! over the pair link of [`crate::liveness`].
//!
//! The verdict table is the spec's boot flow, and the zombie fence lives
//! here: a live Primary neighbor makes any unit — including a
//! Primary-configured one — the Secondary. Configuration does not override
//! a living owner; a revived ex-Primary repeats this lifecycle, discovers
//! the live Primary (its successor), and joins as the Secondary — it never
//! re-enters as Primary, and there is no automatic failback.

use ironplc_runtime::RuntimeHost;

use crate::config::{ConfiguredRole, PairId};
use crate::liveness::{Packet, PairRole};
use crate::problem_codes;

/// The admission verdict for one unit, decided before the application
/// starts (HA Redundancy FSM, "Admission").
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum AdmissionVerdict {
    /// No pair is configured: the unit runs as a single controller.
    Standalone,
    /// The unit owns (or has been promoted to) output control: it executes
    /// the application and commands outputs.
    Primary,
    /// The unit synchronizes from its peer: it receives state replication
    /// and observes I/O, but it does not execute until a promotion grants
    /// the permit through the promotion path.
    Secondary,
}

impl AdmissionVerdict {
    /// Whether the verdict admits scan execution: the permit policy the
    /// shell applies to the host. A Secondary is admitted without the
    /// permit, so its host keeps refusing scans at the permit latch.
    pub fn permits_execution(&self) -> bool {
        matches!(
            self,
            AdmissionVerdict::Standalone | AdmissionVerdict::Primary
        )
    }
}

/// Applies the verdict's permit policy to `host`: grants the execution
/// permit when — and only when — the verdict admits execution. Refusing is
/// the absence of a grant: the host keeps refusing scans at its permit
/// latch (V4018), so an unadmitted unit cannot execute through a forgotten
/// call, because bypass would require a grant this function never issues.
///
/// This is the single verdict→permit mapping; promotion (a Secondary that
/// becomes Primary) repeats the same call with the promoted verdict.
pub fn permit_for(host: &mut RuntimeHost, verdict: AdmissionVerdict) {
    if verdict.permits_execution() {
        host.permit_execution();
    }
}

/// One validated, live frame observed during the discovery window,
/// classified for admission.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Neighbor {
    /// A live peer of this pair presenting the Primary pair role (the
    /// current owner — a booting unit defers to it).
    Primary,
    /// A live peer of this pair presenting the Secondary pair role.
    Secondary,
    /// A live controller on the link that belongs to a different pair
    /// identity. Never a neighbor: the refusal surfaces as V4101 and the
    /// unit must not admit on top of foreign traffic.
    ForeignPair,
}

/// Classifies one validated inbound packet for admission: pair identity
/// first, then the advertised pair role. The caller has already matched
/// liveness (the frame is from a live exchange); this function answers
/// "is it my peer, and what role does it present".
pub fn classify(local: PairId, packet: &Packet) -> Neighbor {
    if packet.pair_id != local {
        return Neighbor::ForeignPair;
    }
    match packet.role {
        PairRole::Primary => Neighbor::Primary,
        PairRole::Secondary => Neighbor::Secondary,
    }
}

/// The discovery outcome of one admission window: what the unit observed
/// on the pair link before it may decide.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub enum Discovery {
    /// No live peer answered within the window.
    #[default]
    NoPeer,
    /// A live peer of this pair presented the Primary pair role.
    LivePrimary,
    /// A live peer of this pair presented the Secondary pair role.
    LiveSecondary,
    /// A live controller answered that belongs to a different pair.
    ForeignPair,
}

/// Why admission refused: the verdict could not be decided safely.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum AdmissionRefusal {
    /// The controller discovered on the pair link belongs to a different
    /// redundant pair: the link is misconfigured or cross-wired (V4101).
    /// Refusing is fail-safe: the unit keeps discovering rather than
    /// admitting on top of another pair's traffic.
    ForeignPairOnLink,
}

impl AdmissionRefusal {
    /// The stable V-code surfacing this refusal on the HA command surface.
    pub fn v_code(&self) -> &'static str {
        match self {
            AdmissionRefusal::ForeignPairOnLink => problem_codes::PAIR_IDENTITY_MISMATCH,
        }
    }
}

/// Decides the admission verdict from the configured role and the
/// discovery outcome — the spec's boot flow, before the application
/// starts.
///
/// A live Primary neighbor always makes this unit the Secondary: that is
/// the zombie fence, and it holds for a Primary-configured unit exactly as
/// for a Secondary-configured one (configuration does not override a
/// living owner). With no live Primary, the configured role pins the
/// boot-time rights: a Primary-configured unit is the initial owner; a
/// Secondary-configured unit stays admitted without output control (its
/// promotion paths are the two cases of the FSM spec, not admission).
pub fn admit(
    configured: ConfiguredRole,
    discovery: Discovery,
) -> Result<AdmissionVerdict, AdmissionRefusal> {
    match discovery {
        Discovery::ForeignPair => Err(AdmissionRefusal::ForeignPairOnLink),
        Discovery::LivePrimary => Ok(AdmissionVerdict::Secondary),
        Discovery::NoPeer | Discovery::LiveSecondary => Ok(match configured {
            ConfiguredRole::Primary => AdmissionVerdict::Primary,
            ConfiguredRole::Secondary => AdmissionVerdict::Secondary,
        }),
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::epoch::Epoch;
    use ironplc_container::{ContainerBuilder, FunctionId};
    use ironplc_runtime::RuntimeError;

    /// A host over an empty program: the admission tests exercise the
    /// permit seam, not the application.
    fn shell_host() -> RuntimeHost {
        let container = ContainerBuilder::new()
            .add_function(FunctionId::INIT, &[], 0, 0, 0)
            .max_call_depth(1)
            .build();
        RuntimeHost::new(container).unwrap()
    }

    fn frame(pair_id: u64, role: PairRole) -> Packet {
        Packet {
            pair_id: PairId::new(pair_id),
            role,
            epoch: Epoch::new(1),
            generation: 1,
            ping_seq: 1,
            pong_seq: 1,
        }
    }

    #[test]
    fn permits_execution_when_verdict_then_admission_policy() {
        assert!(AdmissionVerdict::Standalone.permits_execution());
        assert!(AdmissionVerdict::Primary.permits_execution());
        assert!(!AdmissionVerdict::Secondary.permits_execution());
    }

    #[test]
    fn permit_for_when_verdict_admits_then_host_executes() {
        let mut standalone = shell_host();
        permit_for(&mut standalone, AdmissionVerdict::Standalone);
        standalone.run(1, || 0).unwrap();
        assert_eq!(standalone.status().rounds, 1);

        let mut primary = shell_host();
        permit_for(&mut primary, AdmissionVerdict::Primary);
        primary.run(1, || 0).unwrap();
        assert_eq!(primary.status().rounds, 1);
    }

    #[test]
    fn permit_for_when_secondary_then_run_refused_with_v4018() {
        let mut host = shell_host();
        permit_for(&mut host, AdmissionVerdict::Secondary);

        let error = host.run(1, || 0).unwrap_err();

        assert!(matches!(error, RuntimeError::NotPermitted));
        assert_eq!(error.v_code(), Some("V4018"));
        assert_eq!(host.status().rounds, 0);
    }

    #[test]
    fn permit_for_when_standalone_after_secondary_refusal_then_grant_is_idempotent_policy() {
        // A unit admitted as Secondary that is later promoted repeats the
        // same policy call with the promoted verdict; granting over an
        // earlier refusal is the whole promotion seam.
        let mut host = shell_host();
        permit_for(&mut host, AdmissionVerdict::Secondary);
        assert!(matches!(host.run(1, || 0), Err(RuntimeError::NotPermitted)));

        permit_for(&mut host, AdmissionVerdict::Primary);
        host.run(1, || 0).unwrap();

        assert_eq!(host.status().rounds, 1);
    }

    #[test]
    fn classify_when_pair_id_differs_then_foreign_pair_regardless_of_role() {
        let local = PairId::new(9);

        assert_eq!(
            classify(local, &frame(99, PairRole::Primary)),
            Neighbor::ForeignPair
        );
        assert_eq!(
            classify(local, &frame(99, PairRole::Secondary)),
            Neighbor::ForeignPair
        );
    }

    #[test]
    fn classify_when_pair_matches_then_advertised_role() {
        let local = PairId::new(9);

        assert_eq!(
            classify(local, &frame(9, PairRole::Primary)),
            Neighbor::Primary
        );
        assert_eq!(
            classify(local, &frame(9, PairRole::Secondary)),
            Neighbor::Secondary
        );
    }

    #[test]
    fn admit_when_live_primary_neighbor_then_secondary_regardless_of_configuration() {
        // The zombie fence: a live Primary neighbor demotes even a
        // Primary-configured unit to the Secondary path.
        assert_eq!(
            admit(ConfiguredRole::Primary, Discovery::LivePrimary),
            Ok(AdmissionVerdict::Secondary)
        );
        assert_eq!(
            admit(ConfiguredRole::Secondary, Discovery::LivePrimary),
            Ok(AdmissionVerdict::Secondary)
        );
    }

    #[test]
    fn admit_when_no_peer_then_configured_role_pins_boot_rights() {
        assert_eq!(
            admit(ConfiguredRole::Primary, Discovery::NoPeer),
            Ok(AdmissionVerdict::Primary)
        );
        assert_eq!(
            admit(ConfiguredRole::Secondary, Discovery::NoPeer),
            Ok(AdmissionVerdict::Secondary)
        );
    }

    #[test]
    fn admit_when_live_secondary_only_then_no_initial_owner_forfeited() {
        // A live Secondary without a Primary does not block a
        // Primary-configured unit's initial-owner admission.
        assert_eq!(
            admit(ConfiguredRole::Primary, Discovery::LiveSecondary),
            Ok(AdmissionVerdict::Primary)
        );
        assert_eq!(
            admit(ConfiguredRole::Secondary, Discovery::LiveSecondary),
            Ok(AdmissionVerdict::Secondary)
        );
    }

    #[test]
    fn admit_when_foreign_pair_then_refused_with_v4101() {
        let refusal = admit(ConfiguredRole::Primary, Discovery::ForeignPair);

        assert!(matches!(refusal, Err(AdmissionRefusal::ForeignPairOnLink)));
        assert_eq!(refusal.unwrap_err().v_code(), "V4101");
    }
}
