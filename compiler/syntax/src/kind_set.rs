//! A set of [`SyntaxKind`]s held as a bit set.
//!
//! The parser and the lowering ask one question of many fixed collections of
//! kinds: is this kind one of them. A [`KindSet`] answers it with one bit
//! test, and is built at compile time, so a collection is a `const` that names
//! its members and, where it is "this and that", the sets it is made of
//! ([`KindSet::union`]) instead of repeating their members.
//!
//! The capacity is not declared: the number of words is derived from
//! [`SyntaxKind::ALL`], which the declaration of the kinds generates, so a new
//! kind needs no edit here. A list whose order or repetition matters, or that
//! is iterated to produce output, is not a set and stays a slice.

use crate::syntax_kind::SyntaxKind;
use core::fmt;

/// Bits in one word of a [`KindSet`].
const WORD_BITS: usize = u64::BITS as usize;

/// Words a [`KindSet`] holds: one bit for every kind there is.
const WORDS: usize = SyntaxKind::ALL.len().div_ceil(WORD_BITS);

/// A set of syntax kinds: [`KindSet::contains`] is true for a kind exactly when
/// the set was built with it.
#[derive(Clone, Copy, PartialEq, Eq)]
pub struct KindSet {
    words: [u64; WORDS],
}

impl KindSet {
    /// The set that holds no kind.
    pub const EMPTY: KindSet = KindSet { words: [0; WORDS] };

    /// The set of exactly the kinds of `kinds`; a kind listed twice is held
    /// once. Meant for a `const`, where it is evaluated when the crate is
    /// compiled.
    pub const fn new(kinds: &[SyntaxKind]) -> KindSet {
        let mut words = [0u64; WORDS];
        let mut index = 0;
        while index < kinds.len() {
            let (word, bit) = position(kinds[index]);
            words[word] |= bit;
            index += 1;
        }
        KindSet { words }
    }

    /// The set of the kinds that are in `self` or in `other`.
    pub const fn union(self, other: KindSet) -> KindSet {
        let mut words = self.words;
        let mut index = 0;
        while index < WORDS {
            words[index] |= other.words[index];
            index += 1;
        }
        KindSet { words }
    }

    /// True when `kind` is in the set.
    pub const fn contains(self, kind: SyntaxKind) -> bool {
        let (word, bit) = position(kind);
        self.words[word] & bit != 0
    }

    /// The kinds of the set, in the order they are declared.
    pub fn iter(self) -> impl Iterator<Item = SyntaxKind> {
        SyntaxKind::ALL
            .iter()
            .copied()
            .filter(move |kind| self.contains(*kind))
    }
}

/// The word of a set that holds `kind`, and the bit of that word that stands
/// for it. Every discriminant is an index of [`SyntaxKind::ALL`], so the word is
/// always inside the array.
const fn position(kind: SyntaxKind) -> (usize, u64) {
    let index = kind as usize;
    (index / WORD_BITS, 1 << (index % WORD_BITS))
}

impl fmt::Debug for KindSet {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.debug_set().entries(self.iter()).finish()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::syntax_kind::SyntaxKind as K;

    #[test]
    fn contains_when_kind_listed_then_true_and_when_not_listed_then_false() {
        let set = KindSet::new(&[K::If, K::Else]);
        assert!(set.contains(K::If));
        assert!(set.contains(K::Else));
        assert!(!set.contains(K::EndIf));
    }

    #[test]
    fn contains_when_set_is_empty_then_false_for_every_kind() {
        for kind in SyntaxKind::ALL {
            assert!(!KindSet::EMPTY.contains(*kind), "{kind:?}");
            assert!(!KindSet::new(&[]).contains(*kind), "{kind:?}");
        }
    }

    #[test]
    fn new_when_any_single_kind_then_holds_that_kind_and_no_other() {
        for kind in SyntaxKind::ALL {
            let set = KindSet::new(&[*kind]);
            for other in SyntaxKind::ALL {
                assert_eq!(set.contains(*other), other == kind, "{kind:?} / {other:?}");
            }
        }
    }

    #[test]
    fn new_when_kind_listed_twice_then_equal_to_listed_once() {
        assert_eq!(
            KindSet::new(&[K::Retain, K::Retain]),
            KindSet::new(&[K::Retain])
        );
    }

    #[test]
    fn new_when_order_differs_then_equal() {
        assert_eq!(
            KindSet::new(&[K::If, K::Else]),
            KindSet::new(&[K::Else, K::If])
        );
    }

    #[test]
    fn union_when_two_sets_then_holds_the_kinds_of_either() {
        let left = KindSet::new(&[K::Constant]);
        let right = KindSet::new(&[K::Retain, K::NonRetain]);
        let both = left.union(right);
        for kind in SyntaxKind::ALL {
            assert_eq!(
                both.contains(*kind),
                left.contains(*kind) || right.contains(*kind),
                "{kind:?}"
            );
        }
    }

    #[test]
    fn union_when_one_side_is_empty_then_other_side_unchanged() {
        let set = KindSet::new(&[K::Plus, K::Minus]);
        assert_eq!(set.union(KindSet::EMPTY), set);
        assert_eq!(KindSet::EMPTY.union(set), set);
    }

    #[test]
    fn new_when_kinds_are_the_first_and_last_of_all_then_both_held() {
        let (first, last) = (SyntaxKind::ALL.first(), SyntaxKind::ALL.last());
        let (Some(first), Some(last)) = (first, last) else {
            panic!("SyntaxKind::ALL is empty");
        };
        let set = KindSet::new(&[*first, *last]);
        assert!(set.contains(*first));
        assert!(set.contains(*last));
    }

    #[test]
    fn iter_when_set_has_kinds_then_yields_each_once_in_declaration_order() {
        let set = KindSet::new(&[K::If, K::Else, K::If]);
        assert_eq!(set.iter().collect::<Vec<_>>(), vec![K::Else, K::If]);
        assert_eq!(KindSet::EMPTY.iter().count(), 0);
    }

    #[test]
    fn debug_when_set_has_kinds_then_lists_them_in_declaration_order() {
        // `Else` is declared before `If`, whatever order the set was built in.
        assert_eq!(
            format!("{:?}", KindSet::new(&[K::If, K::Else])),
            "{Else, If}"
        );
        assert_eq!(format!("{:?}", KindSet::EMPTY), "{}");
    }
}
