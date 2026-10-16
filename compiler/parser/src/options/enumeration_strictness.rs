//! The behavior policy that decides which enumerations are no number.
//!
//! An enumeration is *strict* when a program may not compute with its values
//! or store a number into it (`x + 1`, `ABS(x)`, `FOR x := A TO C`, `x := n`);
//! a value of an enumeration that is not strict is its base type wherever the
//! language asks for a number. IEC 61131-3 gives the values of an enumeration
//! no arithmetic. CODESYS and TwinCAT keep the arithmetic and mark the
//! enumerations that refuse it with `{attribute 'strict'}`.
//!
//! The alternatives select what the analysis accepts, not which instructions
//! are emitted, so the enumeration lives with the options and not with the
//! encoded policies of `ironplc_container::policy` (ADR-0079).

use ironplc_container::policy::BehaviorPolicy;
use ironplc_dsl::attribute::registry::STRICT;
use ironplc_dsl::attribute::Attributes;

/// Which enumerations are strict.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
#[repr(u16)]
pub enum EnumerationStrictness {
    /// Every enumeration is strict, whether or not its declaration says so.
    /// The standard's behavior, and the default.
    #[default]
    EveryEnumeration = 0,
    /// An enumeration is strict when its declaration carries
    /// `{attribute 'strict'}`. CODESYS and TwinCAT.
    WithAttribute = 1,
}

impl BehaviorPolicy for EnumerationStrictness {
    const ALL: &'static [Self] = &[Self::EveryEnumeration, Self::WithAttribute];
    const NAMES: &'static [&'static str] = &["every-enumeration", "with-attribute"];

    fn index(self) -> u16 {
        self as u16
    }

    fn description(self) -> &'static str {
        match self {
            Self::EveryEnumeration => "every enumeration is strict",
            Self::WithAttribute => {
                "an enumeration is strict when its declaration has the attribute 'strict'"
            }
        }
    }
}

impl EnumerationStrictness {
    /// Whether an enumeration declared with `attributes` is strict under this
    /// selection.
    ///
    /// This is the one place the attribute `strict` is read: the answer
    /// travels in the type of the enumeration, and every check of the class
    /// asks the type. An enumeration with no declaration of its own to carry
    /// an attribute (one written inline in a variable declaration) is given
    /// `Attributes::default()`. The match has no wildcard, so an alternative
    /// added later does not compile until it answers.
    pub fn is_strict(self, attributes: &Attributes) -> bool {
        match self {
            Self::EveryEnumeration => true,
            Self::WithAttribute => STRICT.of(attributes).is_some(),
        }
    }
}
