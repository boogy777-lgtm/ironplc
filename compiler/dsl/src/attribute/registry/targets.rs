//! The set of declaration kinds an attribute may stand before.

use crate::attribute::census::TargetKind;

/// A set of [`TargetKind`], one bit for each kind. A `const` value, so a row
/// states its set in the declaration list.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct Targets(u32);

/// The bit of a kind. A kind added beyond the thirty-second does not compile
/// into a row: the shift overflows when the constant is evaluated.
const fn bit(kind: TargetKind) -> u32 {
    1u32 << (kind as u32)
}

/// The ten kinds of type declaration.
pub const TYPES: Targets = Targets::of(&[
    TargetKind::Enumeration,
    TargetKind::Subrange,
    TargetKind::Simple,
    TargetKind::Array,
    TargetKind::Structure,
    TargetKind::Union,
    TargetKind::String,
    TargetKind::Reference,
    TargetKind::Params,
    TargetKind::LateBound,
]);

/// Every kind of declaration.
pub const EVERY: Targets = Targets::of(TargetKind::ALL);

impl Targets {
    /// The set of `kinds`.
    pub const fn of(kinds: &[TargetKind]) -> Self {
        let mut bits = 0;
        let mut index = 0;
        while index < kinds.len() {
            bits |= bit(kinds[index]);
            index += 1;
        }
        Targets(bits)
    }

    /// This set and `kinds`.
    pub const fn and(self, kinds: &[TargetKind]) -> Self {
        Targets(self.0 | Targets::of(kinds).0)
    }

    /// Whether `kind` is in the set.
    pub const fn contains(self, kind: TargetKind) -> bool {
        self.0 & bit(kind) != 0
    }

    /// The kinds of the set, in the order of [`TargetKind::ALL`].
    pub fn kinds(self) -> impl Iterator<Item = TargetKind> {
        TargetKind::ALL
            .iter()
            .copied()
            .filter(move |kind| self.contains(*kind))
    }

    /// How a message and the reference page name the set: `every declaration`,
    /// or the kinds, with the ten kinds of type declaration said as one when
    /// all of them are in the set.
    pub fn describe(self) -> String {
        if self == EVERY {
            return "every declaration".to_string();
        }
        let all_types = TYPES.kinds().all(|kind| self.contains(kind));
        let types = all_types.then(|| "any type declaration".to_string());
        let others = self
            .kinds()
            .filter(|kind| !(all_types && TYPES.contains(*kind)))
            .map(|kind| kind.label().to_string());
        types
            .into_iter()
            .chain(others)
            .collect::<Vec<String>>()
            .join(", ")
    }
}
