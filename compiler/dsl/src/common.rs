//! Provides definitions of objects from IEC 61131-3 common elements.
//!
//! See section 2.

pub use crate::attribute::{Attribute, Attributed, Attributes};
pub use crate::oop::*;
pub use crate::type_name::*;

mod data_type;
mod enumerated_default;
mod initializer;
mod library;
mod literal;
mod pou;
mod variable;

pub use data_type::*;
pub use enumerated_default::EnumeratedDefault;
pub use initializer::*;
pub use library::*;
pub use literal::*;
pub use pou::*;
pub use variable::*;

#[cfg(test)]
mod tests;
