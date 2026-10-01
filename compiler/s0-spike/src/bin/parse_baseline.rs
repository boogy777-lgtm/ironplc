//! S0 parse-baseline harness (test-only; see `specs/design/parse-tree-s0-experiment.md`).
//!
//! Measures the **current production** parse path (`ironplc_parser`) over the
//! shared corpus and the CODESYS fixtures, with wall time and allocation
//! counts. The numbers belong in the S0 experiment document; this binary is
//! never wired into the compiler or the CLI.
//!
//! Usage: `parse_baseline [warm-repeats]` (default 50).

use ironplc_s0_spike::baseline::{report, run};
use stats_alloc::{StatsAlloc, INSTRUMENTED_SYSTEM};
use std::alloc::System;

#[global_allocator]
static GLOBAL: &StatsAlloc<System> = &INSTRUMENTED_SYSTEM;

fn main() -> Result<(), Box<dyn std::error::Error>> {
    let repeats = std::env::args()
        .nth(1)
        .and_then(|argument| argument.parse().ok())
        .unwrap_or(50);
    let run = run(GLOBAL, repeats)?;
    print!("{}", report(&run));
    Ok(())
}
