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
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum EnumeratedDefault {
    Value(EnumeratedValue),
    Number(SignedInteger),
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
