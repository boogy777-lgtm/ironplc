//! Parse-only Criterion benchmarks over the shared corpus.
//!
//! Per-input warm timings of every path of the table in
//! `ironplc_benchmarks::paths`: the public functions of `ironplc-parser`
//! (`tokenize_program`, `parse_program`, `parse_st_statements`, which run the
//! front end built on the lossless tree) and the entry points of
//! `ironplc-syntax` (lex, tokenize, the CST parse and its lowering). Each path is one Criterion
//! group over the inputs it runs over. Cold timings and allocation counts are
//! reported by `benches/parse_baseline.rs`, which needs its own global
//! allocator and so cannot share this binary.
//!
//! Run with: `cargo bench --package ironplc-benchmarks --bench parse_benchmark`.

// Benchmark-target boundary: a corpus that cannot be read is a
// benchmark-authoring bug, not user input.
#![allow(
    clippy::unwrap_used,
    reason = "benchmark target: panicking helpers are sanctioned in benchmarks"
)]

use criterion::{criterion_group, criterion_main, BenchmarkId, Criterion, Throughput};
use ironplc_benchmarks::corpus::{corpus_dir, load_corpus};
use ironplc_benchmarks::paths::{Ctx, Input, Over, Probe, PATHS};
use std::time::{Duration, Instant};

/// Adds up the time between `start` and `stop`, so that Criterion times the
/// call a path makes and not what the path does around it.
struct Timer {
    started: Option<Instant>,
    elapsed: Duration,
}

impl Probe for Timer {
    fn start(&mut self) {
        self.started = Some(Instant::now());
    }

    fn stop(&mut self) {
        if let Some(start) = self.started.take() {
            self.elapsed += start.elapsed();
        }
    }
}

/// Registers one group per path, with one benchmark per input the path runs
/// over.
fn bench_paths(c: &mut Criterion) {
    let ctx = Ctx::default();
    let files = load_corpus(&corpus_dir()).unwrap();
    for over in Over::all() {
        let items = over.items(&files);
        for path in PATHS.iter().filter(|path| path.runs_over(over)) {
            let mut group = c.benchmark_group(path.group);
            group.sample_size(over.samples());
            for item in &items {
                group.throughput(Throughput::Bytes(item.bytes() as u64));
                group.bench_with_input(
                    BenchmarkId::from_parameter(&item.name),
                    item,
                    |b, input: &Input| {
                        b.iter_custom(|iterations| {
                            let mut timer = Timer {
                                started: None,
                                elapsed: Duration::ZERO,
                            };
                            for _ in 0..iterations {
                                path.call(&ctx, input, &mut timer);
                            }
                            timer.elapsed
                        })
                    },
                );
            }
            group.finish();
        }
    }
}

criterion_group!(benches, bench_paths);
criterion_main!(benches);
