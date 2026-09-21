//! Admission: the verdict that decides whether this unit may execute,
//! decided before the application starts.
//!
//! A controller does not know a priori whether it runs standalone or as one
//! unit of a redundant pair; the redundancy layer decides **before** the
//! application starts and grants the application permission to start
//! (`specs/design/ha-redundancy-fsm.md`, "Admission"). This module carries
//! the verdict shape and the permit policy it implies; the neighbor
//! discovery that *produces* a verdict lands with the pair link, per the
//! module decomposition of
//! `specs/design/ha-redundancy-layer-architecture.md`.

use ironplc_runtime::RuntimeHost;

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
pub fn permit_for(host: &mut RuntimeHost, verdict: AdmissionVerdict) {
    if verdict.permits_execution() {
        host.permit_execution();
    }
}

#[cfg(test)]
mod tests {
    use super::*;
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
}
