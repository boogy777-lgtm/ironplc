//! The OwnerLease: the HA supervisor's authority record, minted at scan
//! commit and expiring on silence.
//!
//! Per ADR-0062 the OwnerLease is generated **exclusively by the HA
//! supervisor at scan commit** — the network task never mints it — and
//! uncontrolled failover starts at `max(T_plc-peer-detection,
//! T_io-owner-lease-expiry)`: both independent proofs must agree before a
//! claim may begin. The lease is the second proof: while the owning unit
//! keeps committing scans, its lease renews and no claimant may take over;
//! when the owner falls silent, the lease expiry is the boundary of its
//! authority, and only then is the death proven.
//!
//! The lease carries the [`Epoch`] it was minted under, so the authority
//! record is anti-stale like everything else the pair exchanges (a lease
//! minted under an old epoch names its own staleness). The supervisor
//! renews the lease at every committed scan boundary (the runtime's
//! scan-commit callback, the Slice-2 seam) and mints it once at the
//! promotion barrier, the other supervisor-owned authority point; both are
//! in the scan loop, never the network task.
//!
//! Timing is abstract: `now` and the TTL are `u64` clock ticks supplied by
//! the composition root (the scan driver), because the served-VM target
//! has no monotonic clock of its own — the same measured-terms philosophy
//! as the liveness exchange's exchange-counted windows (ADR-0062: measured
//! terms, not assumed constants). The TTL is an open parameter placeholder
//! on [`RedundancyConfig`](crate::RedundancyConfig); the calibration
//! pipeline replaces it with the engineer-configured owner-lease expiry
//! time.

use crate::epoch::Epoch;

/// The owning unit's authority record: the epoch it was minted under and
/// the deadline it expires at, both in the composition root's abstract
/// clock ticks.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct OwnerLease {
    epoch: Epoch,
    expires_at: u64,
}

impl OwnerLease {
    /// Mints the lease under `epoch` at `now`, alive for `ttl` ticks
    /// (saturating: an authority that outlives the clock is the least
    /// harmful saturation — expiry is the safety direction).
    pub fn mint(epoch: Epoch, now: u64, ttl: u64) -> Self {
        Self {
            epoch,
            expires_at: now.saturating_add(ttl),
        }
    }

    /// The epoch the lease was minted under — the anti-stale stamp.
    pub const fn epoch(&self) -> Epoch {
        self.epoch
    }

    /// The abstract clock tick the lease expires at.
    pub const fn expires_at(&self) -> u64 {
        self.expires_at
    }

    /// Whether the authority lapsed at `now`. Expiry is the safety
    /// direction: at the deadline the authority is gone (`now >=
    /// expires_at`), so a lease the clock raced past is dead, never
    /// accidentally alive.
    pub const fn is_expired(&self, now: u64) -> bool {
        now >= self.expires_at
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn mint_when_minted_then_carries_epoch_and_expiry() {
        let lease = OwnerLease::mint(Epoch::new(5), 10, 3);

        assert_eq!(lease.epoch(), Epoch::new(5));
        assert_eq!(lease.expires_at(), 13);
        assert!(!lease.is_expired(12));
    }

    #[test]
    fn is_expired_when_deadline_reached_then_true_and_stays_true() {
        let lease = OwnerLease::mint(Epoch::new(5), 10, 3);

        assert!(!lease.is_expired(0));
        assert!(lease.is_expired(13));
        assert!(lease.is_expired(14));
        assert!(lease.is_expired(100));
    }

    #[test]
    fn mint_when_ttl_saturates_then_expiry_is_the_clock_ceiling() {
        let lease = OwnerLease::mint(Epoch::new(0), u64::MAX - 1, 10);

        assert_eq!(lease.expires_at(), u64::MAX);
        assert!(lease.is_expired(u64::MAX));
    }
}
