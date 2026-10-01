//! The grammar rules, one module per concern.
//!
//! Each rule is a function over [`Parser`](super::state::Parser). Expressions
//! and statements follow the legacy PEG grammar's accepted language; the
//! rules say where the legacy grammar's quirks (a required `;`, bodies that
//! must not be empty) are deliberate parity rather than design.
//!
//! The declarations are split by what they declare: `source_file` is the
//! sequence of declarations, `pou` the programs, functions and function
//! blocks and the layout of their bodies, `oop` methods, properties and
//! interfaces, `var_blocks` the variable blocks (one rule table per kind of
//! declaration), `types` type specifications and `TYPE` blocks,
//! `initializers` initial values, `configuration` configurations, and `sfc`
//! sequential function charts.

mod codesys;
mod common;
mod configuration;
mod control;
mod expressions;
mod initializers;
mod literals;
mod oop;
mod pou;
mod sfc;
mod source_file;
mod statements;
mod temporal;
mod types;
mod var_blocks;

pub(super) use expressions::expression;
pub(super) use source_file::source_file;
pub(super) use statements::{statement_list, ListEnd};
