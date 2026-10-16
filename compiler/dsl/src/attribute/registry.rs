//! The attributes the compiler knows: one row for each, and the readers and the
//! check that are the same for every row.
//!
//! A row ([`Info`], and [`Row`] with the type of its value) says what an
//! attribute is called, where it may stand, what it takes, and what IronPLC
//! does with it. The declaration list ([`rows`]) holds each row once and makes
//! both its typed constant (`PACK_MODE`) and its place in [`ALL`]; a part of
//! the compiler that needs an attribute asks the constant
//! (`PACK_MODE.of(&declaration.attributes)` gives a `PackMode`) and compares no
//! name. The check ([`faults`]) reads the list and no row by name. Adding an
//! attribute adds an entry to the declaration list and nothing else.
//!
//! An attribute that has no row is user-defined: it is accepted and kept as it
//! is written.

mod faults;
mod row;
mod rows;
mod table;
mod targets;
mod value;

#[cfg(test)]
mod test_support;
#[cfg(test)]
mod tests;

pub use faults::{declared_faults, faults, Fault, FaultKind};
pub use row::{Family, Info, Row, State};
pub use rows::*;
pub use table::table;
pub use targets::{Targets, EVERY, TYPES};
pub use value::*;

/// The row of the attribute `name` (its name or an alias, without regard to
/// case), or nothing when the compiler does not know the attribute.
pub fn find(name: &str) -> Option<&'static Info> {
    ALL.iter().find(|row| row.answers_to(name))
}
