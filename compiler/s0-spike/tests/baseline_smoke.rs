//! End-to-end smoke test for the parse-baseline harness.
//!
//! Runs the real measurement over the real corpus (one warm repeat) so the
//! path exercised by `bin/parse_baseline.rs` is also covered by the test
//! suite. The global allocator mirrors the binary, so allocation counts are
//! live here too.

use ironplc_s0_spike::baseline::{report, run};
use stats_alloc::{StatsAlloc, INSTRUMENTED_SYSTEM};
use std::alloc::System;

#[global_allocator]
static GLOBAL: &StatsAlloc<System> = &INSTRUMENTED_SYSTEM;

#[test]
fn baseline_when_one_repeat_then_reports_every_corpus_file() {
    let run = run(GLOBAL, 1).expect("baseline run");
    assert_eq!(run.files.len(), 53, "48 standard + 5 codesys fixtures");
    assert_eq!(run.acceptance.len(), 5, "one acceptance count per dialect");

    let text = report(&run);
    assert!(text.contains("std/first_steps.st"));
    assert!(text.contains("codesys/pragmas.st"));
    assert!(text.contains("parse acceptance"));

    assert!(
        run.files
            .iter()
            .any(|file| file.cold_parse.median_allocations() > 0),
        "cold parse should allocate"
    );
    assert!(
        run.files
            .iter()
            .all(|file| file.tokens > 0 && file.bytes > 0),
        "every corpus file should have bytes and tokens"
    );
}
