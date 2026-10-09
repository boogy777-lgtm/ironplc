//! Provides definitions of objects from the IEC 61131-3 language elements
//! and base implementations of common patterns for working with libraries.

// A result that reaches a user must not depend on the order a hash container
// iterates in: see `disallowed-methods` in `clippy.toml`.
#![deny(clippy::iter_over_hash_type, clippy::disallowed_methods)]

pub mod common;
pub mod configuration;
pub mod construct;
pub mod core;
pub mod diagnostic;
pub mod extension;
pub mod fold;
pub mod member_qualifier;
pub mod oop;
pub mod scope;
pub mod sfc;
pub mod stack;
pub mod string_escape;
pub mod textual;
pub mod time;
pub mod type_id;
pub mod visitor;
