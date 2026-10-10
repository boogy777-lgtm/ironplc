//! The default an enumeration declaration states.

use dsl_macro_derive::Recurse;

use super::{EnumeratedValue, SignedInteger};
use crate::fold::Fold;
use crate::visitor::Visitor;

/// The default an enumeration declaration states: a value of the enumeration,
/// or a number, which is a default when it is the number of one of its values
/// (`TYPE Level : (Low, High) := 1;` starts at `High`).
///
/// Whether a number is the number of a value is for the analysis to say; the
/// declaration records what was written.
///
/// See section 2.3.3.1.
#[derive(Clone, PartialEq, Recurse)]
pub enum EnumeratedDefault {
    Value(EnumeratedValue),
    Number(SignedInteger),
}

/// A value default reads as the value does, as it did before a default could
/// be a number, so that what a library of such declarations reads as -- the
/// text a fingerprint of it is made of -- does not depend on there being two
/// kinds of default.
impl std::fmt::Debug for EnumeratedDefault {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            EnumeratedDefault::Value(value) => std::fmt::Debug::fmt(value, f),
            EnumeratedDefault::Number(number) => f.debug_tuple("Number").field(number).finish(),
        }
    }
}

impl EnumeratedDefault {
    /// The value of the enumeration the default names, or `None` for a number.
    pub fn as_value(&self) -> Option<&EnumeratedValue> {
        match self {
            EnumeratedDefault::Value(value) => Some(value),
            EnumeratedDefault::Number(_) => None,
        }
    }
}
