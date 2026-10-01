//! S0 integration spike — **test-only, never production**.
//!
//! This crate is the executable evidence for
//! `specs/design/parse-tree-s0-experiment.md`, the S0 stage of the approved
//! [Parse-Tree Architecture](../../../specs/design/parse-tree-architecture.md).
//! It validates two adapters the design selected:
//!
//! - [`cst`] / [`parser`] / [`lexer`]: a small **lossless rowan green/red
//!   CST** over a slice of ST syntax (a POU declaration plus body
//!   statements), built by a scoped recursive-descent/Pratt parser that
//!   consumes every byte of the original text — trivia and error regions
//!   included (design section 3.1).
//! - [`db`]: a **Salsa-tracked declaration-summary query** demonstrating
//!   memoization, recomputation on input change, and backdating of equal
//!   summaries so consumers are not re-executed (design section 3.3).
//!
//! The `parse_baseline` bin measures the **current production** parse path
//! (`ironplc_parser::parse_program`) to record the S0 benchmark baseline.
//!
//! Boundaries (design section 4): no production crate depends on this crate,
//! nothing here is wired into codegen or the CLI, and the crate is deleted at
//! the S1 cutover instead of being promoted into the frontend. The rowan and
//! salsa versions exercised here are recorded in the experiment document;
//! selecting them for production is S1 work.

pub mod baseline;
pub mod cst;
pub mod db;
pub mod lexer;
pub mod parser;
pub mod syntax;
