//! The redundancy shell's static configuration for one unit.
//!
//! This is project-side data per the architecture doc: `REDUNDANCY_ENABLED`
//! plus the pair's identity and this unit's configured role, set from the
//! engineering UI ([`ConfiguredRole`] is static configuration, never a
//! runtime state — `specs/design/ha-redundancy-fsm.md`, "Configured Role
//! and Identity"). The link timing values are open parameters of the FSM
//! spec (periods and confirmation time are deliberately undecided); the
//! defaults are placeholders the calibration pipeline (ADR-0062) will
//! replace with measured, engineer-facing times.

/// The configured role of one unit, set per unit from the engineering UI.
///
/// The configured role pins boot-time rights (only a Primary-configured
/// unit may claim output control at boot); it is static configuration,
/// never a runtime state, and a commanded role swap exchanges the pair's
/// configured roles — it is not decided here.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum ConfiguredRole {
    /// The unit the engineer flashed as the pair's initial owner.
    Primary,
    /// The unit that synchronizes from its peer and gains control only
    /// through the two promotion cases.
    Secondary,
}

/// Identity of a redundant pair (the domain identity carried in every
/// ping/pong packet). Permanent: it never changes for the lifetime of the
/// pair.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct PairId(u64);

impl PairId {
    /// Creates a pair identity from its raw value.
    pub const fn new(raw: u64) -> Self {
        Self(raw)
    }

    /// Returns the raw value.
    pub const fn raw(self) -> u64 {
        self.0
    }
}

impl core::fmt::Display for PairId {
    fn fmt(&self, f: &mut core::fmt::Formatter<'_>) -> core::fmt::Result {
        write!(f, "{}", self.0)
    }
}

/// Default placeholder for how many unanswered ping exchanges a pong may
/// lag before one exchange counts as missing. An open parameter of the
/// FSM spec; ADR-0062 replaces the count with the engineer-configured
/// peer-failure confirmation time once calibration exists.
pub const DEFAULT_CONFIRMATION_EXCHANGES: u32 = 2;

/// Default placeholder for how many consecutive missing exchanges confirm
/// peer death. An open parameter of the FSM spec (see
/// [`DEFAULT_CONFIRMATION_EXCHANGES`]).
pub const DEFAULT_MISSED_EXCHANGES: u32 = 3;

/// Static redundancy configuration for one unit, decided before the
/// application starts.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct RedundancyConfig {
    /// The pair this unit belongs to, or `None` when no pair is
    /// configured — standalone admission, the statechart does not run.
    pub pair_id: Option<PairId>,
    /// This unit's configured role (meaningful only when a pair is
    /// configured).
    pub role: ConfiguredRole,
    /// How many unanswered ping exchanges a pong may lag before one
    /// exchange counts as missing (the confirmation window, in exchanges).
    pub confirmation_exchanges: u32,
    /// How many consecutive missing exchanges confirm peer death.
    pub missed_exchanges: u32,
}

impl RedundancyConfig {
    /// Standalone configuration: no pair, the statechart does not run and
    /// the composition root grants the execution permit immediately.
    pub fn standalone() -> Self {
        Self {
            pair_id: None,
            role: ConfiguredRole::Secondary,
            confirmation_exchanges: DEFAULT_CONFIRMATION_EXCHANGES,
            missed_exchanges: DEFAULT_MISSED_EXCHANGES,
        }
    }

    /// Pair membership with this unit's configured role and the default
    /// link-timing placeholders.
    pub fn pair(pair_id: PairId, role: ConfiguredRole) -> Self {
        Self {
            pair_id: Some(pair_id),
            role,
            confirmation_exchanges: DEFAULT_CONFIRMATION_EXCHANGES,
            missed_exchanges: DEFAULT_MISSED_EXCHANGES,
        }
    }

    /// Overrides the confirmation window (in exchanges). Test and
    /// calibration hook for an open parameter.
    pub fn with_confirmation_exchanges(mut self, exchanges: u32) -> Self {
        self.confirmation_exchanges = exchanges;
        self
    }

    /// Overrides the missed-exchange death threshold. Test and
    /// calibration hook for an open parameter.
    pub fn with_missed_exchanges(mut self, exchanges: u32) -> Self {
        self.missed_exchanges = exchanges;
        self
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn standalone_when_constructed_then_no_pair_and_default_timings() {
        let config = RedundancyConfig::standalone();

        assert_eq!(config.pair_id, None);
        assert_eq!(
            config.confirmation_exchanges,
            DEFAULT_CONFIRMATION_EXCHANGES
        );
        assert_eq!(config.missed_exchanges, DEFAULT_MISSED_EXCHANGES);
    }

    #[test]
    fn pair_when_constructed_then_pair_identity_and_role_with_default_timings() {
        let config = RedundancyConfig::pair(PairId::new(7), ConfiguredRole::Primary);

        assert_eq!(config.pair_id, Some(PairId::new(7)));
        assert_eq!(config.role, ConfiguredRole::Primary);
        assert_eq!(
            config.confirmation_exchanges,
            DEFAULT_CONFIRMATION_EXCHANGES
        );
        assert_eq!(config.missed_exchanges, DEFAULT_MISSED_EXCHANGES);
    }

    #[test]
    fn with_link_timing_when_overridden_then_placeholders_replaced() {
        let config = RedundancyConfig::pair(PairId::new(7), ConfiguredRole::Secondary)
            .with_confirmation_exchanges(1)
            .with_missed_exchanges(2);

        assert_eq!(config.confirmation_exchanges, 1);
        assert_eq!(config.missed_exchanges, 2);
    }

    #[test]
    fn pair_id_display_when_formatted_then_writes_the_raw_value() {
        assert_eq!(PairId::new(42).to_string(), "42");
    }
}
