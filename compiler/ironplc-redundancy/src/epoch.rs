//! The ownership/fencing epoch: a newtype counter with a strictly
//! anti-stale adoption rule.
//!
//! Epoch follows the opaque-newtype shape of the runtime's generation
//! counters (the architecture doc's pattern-reuse map). Per ADR-0062 the
//! epoch is **anti-stale protection only**: it rejects stale packets,
//! images, and claims, and it is never the arbiter of who may take over —
//! that prevents partition bidding wars. Minting is owned by the HA
//! supervisor at scan commit (the runtime's scan-commit callback), never
//! by the network task; this module carries the type and the adoption
//! rule, not the minting policy. Epoch persistence across power loss is a
//! roadmap open parameter (the `EpochStore` port decides it later); the
//! value is volatile in this slice.

use core::fmt;

/// The ownership/fencing epoch of a redundant pair.
///
/// `Epoch(0)` is the un-set value of a unit that has not committed an
/// epoch (a fresh boot, or a Secondary that has not adopted its peer's):
/// any live peer epoch is newer, so adoption always succeeds from boot.
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct Epoch(u32);

impl Epoch {
    /// Creates an epoch from its raw value.
    pub const fn new(raw: u32) -> Self {
        Self(raw)
    }

    /// Returns the raw value.
    pub const fn raw(self) -> u32 {
        self.0
    }

    /// Mints the next epoch at a scan commit (ADR-0062: only the HA
    /// supervisor mints, and only at scan commit). Saturating: an epoch
    /// that ever stopped advancing would be a defect the pair cannot
    /// paper over, and saturating keeps the mint panic-free.
    pub const fn next(self) -> Self {
        Self(self.0.saturating_add(1))
    }

    /// Adopts the peer's epoch under the anti-stale rule: newer replaces
    /// local, equal is already aligned, older is stale and leaves the
    /// local value untouched. Never arbitration, and never a reason to
    /// move the local value backward. Adoption protects the *local*
    /// counter; the SYNC chart's epoch-discontinuity input is the
    /// liveness exchange's [`PeerRestarted`](crate::LivenessEvent)
    /// report — a peer that rebooted regressed its per-channel sequence
    /// and lost its epoch memory, which a monitoring peer merely trailing
    /// the owner's mint never does.
    pub fn adopt(&mut self, peer: Epoch) -> EpochAdoption {
        if peer.0 > self.0 {
            *self = peer;
            EpochAdoption::Adopted
        } else if peer.0 == self.0 {
            EpochAdoption::Aligned
        } else {
            EpochAdoption::Stale
        }
    }
}

impl fmt::Display for Epoch {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.0)
    }
}

/// The outcome of adopting a peer's epoch.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum EpochAdoption {
    /// The peer's epoch was newer and replaced the local value.
    Adopted,
    /// The peer's epoch matches the local value; the pair is aligned.
    Aligned,
    /// The peer's epoch is older than the local value: stale, rejected,
    /// the local value stands (an epoch discontinuity).
    Stale,
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn next_when_minted_then_advances_by_one() {
        assert_eq!(Epoch::new(41).next(), Epoch::new(42));
        assert_eq!(Epoch::new(0).next(), Epoch::new(1));
    }

    #[test]
    fn adopt_when_peer_newer_then_adopts() {
        let mut epoch = Epoch::new(3);

        assert_eq!(epoch.adopt(Epoch::new(7)), EpochAdoption::Adopted);
        assert_eq!(epoch, Epoch::new(7));
    }

    #[test]
    fn adopt_when_peer_equal_then_aligned() {
        let mut epoch = Epoch::new(7);

        assert_eq!(epoch.adopt(Epoch::new(7)), EpochAdoption::Aligned);
        assert_eq!(epoch, Epoch::new(7));
    }

    #[test]
    fn adopt_when_peer_older_then_stale_and_local_stands() {
        let mut epoch = Epoch::new(7);

        assert_eq!(epoch.adopt(Epoch::new(5)), EpochAdoption::Stale);
        assert_eq!(epoch, Epoch::new(7));
    }

    #[test]
    fn adopt_when_fresh_boot_then_any_live_peer_epoch_adopts() {
        let mut epoch = Epoch::new(0);

        assert_eq!(epoch.adopt(Epoch::new(1)), EpochAdoption::Adopted);
        assert_eq!(epoch, Epoch::new(1));
    }

    #[test]
    fn epoch_display_when_formatted_then_writes_the_raw_value() {
        assert_eq!(Epoch::new(9).to_string(), "9");
    }
}
