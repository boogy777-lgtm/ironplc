//! How a candidate becomes the running application (class K2 of the online
//! model): the mode a client asks for and the kind of candidate the host
//! derives from it.
//!
//! Every way ends in the one boundary swap of the host; what differs is the
//! part of the old state that is kept.
//!
//! | Kind | Kept at the swap | Revert (untest, cancel under Test) |
//! |---|---|---|
//! | [`CandidateKind::Change`] | the whole persistent part, byte for byte (the layout is equal) | yes |
//! | [`CandidateKind::Migrate`] | the planned copy per stable variable ID (ADR-0061) | no |
//! | [`CandidateKind::Replace`] | the `PERSISTENT` variables whose declaration is equal (ADR-0088) | no |
//!
//! A new way is one variant here, its keep rule at the swap and its row in
//! [`CandidateKind::reverts`]; the exhaustive matches name every place.

use serde::{Deserialize, Serialize};

use crate::migration::{KeepPlan, StateMigrationPlan};

/// The mode a client asks for when it stages a candidate (the `mode` field of
/// `acceptEdits`). `Migrate` is not a mode a client chooses: the host selects
/// it from the layout hash when the mode is `Change`.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub enum AcceptMode {
    /// The candidate continues the running application: a body edit when the
    /// layout is equal, a state migration when it differs and both sides carry
    /// stable variable IDs. The default.
    #[default]
    Change,
    /// The candidate replaces the running application (a Download): its state
    /// is built fresh and only the `PERSISTENT` variables whose declaration is
    /// equal keep their value.
    Replace,
}

/// How a staged candidate takes over the state of the running application.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub enum CandidateKind {
    /// Equal layout: the persistent part is carried over unchanged.
    Change,
    /// Different layout: the state is rebuilt and the planned values copied.
    Migrate,
    /// A replacement: the state is rebuilt and the `PERSISTENT` variables with
    /// an equal declaration copied.
    Replace,
}

/// The state a staged candidate takes over from the running application, with
/// what the host needs to carry it out at the swap. One variant for each
/// [`CandidateKind`]; the matches over it are exhaustive.
#[derive(Debug)]
pub(crate) enum Carry {
    /// Equal layout: the persistent part is carried byte for byte.
    Change,
    /// The planned copy per stable variable ID.
    Migrate(StateMigrationPlan),
    /// The `PERSISTENT` variables with an equal declaration.
    Replace(KeepPlan),
}

impl Carry {
    /// The kind of the candidate this carries.
    pub(crate) fn kind(&self) -> CandidateKind {
        match self {
            Carry::Change => CandidateKind::Change,
            Carry::Migrate(_) => CandidateKind::Migrate,
            Carry::Replace(_) => CandidateKind::Replace,
        }
    }
}

impl AcceptMode {
    /// Whether this is the mode an absent field means; it is left off the wire.
    pub(crate) fn is_default(&self) -> bool {
        *self == AcceptMode::default()
    }
}

impl CandidateKind {
    /// Whether the candidate can be reverted once it executes: only a change
    /// leaves the old buffers in place. The other kinds rebuild them at the
    /// swap, so untest and cancel under Test have nothing to go back to.
    pub(crate) const fn reverts(self) -> bool {
        match self {
            CandidateKind::Change => true,
            CandidateKind::Migrate | CandidateKind::Replace => false,
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn reverts_when_each_kind_then_only_a_change_reverts() {
        assert!(CandidateKind::Change.reverts());
        assert!(!CandidateKind::Migrate.reverts());
        assert!(!CandidateKind::Replace.reverts());
    }

    #[test]
    fn accept_mode_when_absent_on_the_wire_then_change() {
        assert_eq!(AcceptMode::default(), AcceptMode::Change);
        assert_eq!(
            serde_json::to_string(&AcceptMode::Replace).unwrap(),
            "\"replace\""
        );
    }
}
