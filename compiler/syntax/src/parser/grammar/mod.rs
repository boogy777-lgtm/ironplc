//! The grammar rules, one module per concern.
//!
//! Each rule is a function over [`Parser`](super::state::Parser). Expressions
//! and statements follow the legacy PEG grammar's accepted language; the
//! rules say where the legacy grammar's quirks (a required `;`, bodies that
//! must not be empty) are deliberate parity rather than design.

mod codesys;
mod control;
mod expressions;
mod literals;
mod source_file;
mod statements;
mod temporal;

pub(super) use expressions::expression;
pub(super) use source_file::source_file;
pub(super) use statements::{statement_list, ListEnd};
