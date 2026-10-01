//! Parse-only Criterion benchmarks over the shared corpus.
//!
//! Per-file warm timings of the two public entry points of `ironplc-parser`:
//! `tokenize_program` (preprocess, lex, token transforms) and `parse_program`
//! (the same path plus the PEG parse). Cold timings and allocation counts are
//! reported by `benches/parse_baseline.rs`, which needs its own global
//! allocator and so cannot share this binary.
//!
//! Run with: `cargo bench --package ironplc-benchmarks --bench parse_benchmark`

// Benchmark-target boundary: a corpus that cannot be read is a
// benchmark-authoring bug, not user input.
#![allow(
    clippy::unwrap_used,
    clippy::result_large_err,
    reason = "benchmark target: panicking helpers are sanctioned in benchmarks; the large Err type is parse_program's public signature"
)]

use criterion::{criterion_group, criterion_main, BenchmarkId, Criterion, Throughput};
use ironplc_benchmarks::corpus::{corpus_dir, load_corpus};
use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use ironplc_parser::{parse_program, tokenize_program};
use std::hint::black_box;

fn bench_tokenize(c: &mut Criterion) {
    let options = CompilerOptions::default();
    let file_id = FileId::default();
    let mut group = c.benchmark_group("parse_tokenize");
    for file in load_corpus(&corpus_dir()).unwrap() {
        group.throughput(Throughput::Bytes(file.source.len() as u64));
        group.bench_with_input(
            BenchmarkId::from_parameter(&file.name),
            &file.source,
            |b, source| b.iter(|| black_box(tokenize_program(source, &file_id, &options, 0, 0))),
        );
    }
    group.finish();
}

fn bench_parse(c: &mut Criterion) {
    let options = CompilerOptions::default();
    let file_id = FileId::default();
    let mut group = c.benchmark_group("parse_full");
    for file in load_corpus(&corpus_dir()).unwrap() {
        group.throughput(Throughput::Bytes(file.source.len() as u64));
        group.bench_with_input(
            BenchmarkId::from_parameter(&file.name),
            &file.source,
            |b, source| b.iter(|| black_box(parse_program(source, &file_id, &options))),
        );
    }
    group.finish();
}

criterion_group!(benches, bench_tokenize, bench_parse);
criterion_main!(benches);
