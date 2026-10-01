//! Parse-only Criterion benchmarks over the shared corpus.
//!
//! Per-file warm timings of the two public entry points of `ironplc-parser`:
//! `tokenize_program` (preprocess, lex, token transforms) and `parse_program`
//! (the same path plus the PEG parse), and of the lossless CST entry point
//! `parse_source_file` from `ironplc-syntax` (lex plus CST parse). Cold timings and allocation counts are
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
use ironplc_syntax::{parse_source_file, ParseOptions};
use std::hint::black_box;

/// Registers one benchmark per corpus file in the group `name`, timing `call`
/// on the file's source.
fn bench_corpus<T>(c: &mut Criterion, name: &str, call: impl Fn(&str) -> T) {
    let mut group = c.benchmark_group(name);
    for file in load_corpus(&corpus_dir()).unwrap() {
        group.throughput(Throughput::Bytes(file.source.len() as u64));
        group.bench_with_input(
            BenchmarkId::from_parameter(&file.name),
            &file.source,
            |b, source| b.iter(|| black_box(call(source))),
        );
    }
    group.finish();
}

fn bench_tokenize(c: &mut Criterion) {
    let options = CompilerOptions::default();
    let file_id = FileId::default();
    bench_corpus(c, "parse_tokenize", |source| {
        tokenize_program(source, &file_id, &options, 0, 0)
    });
}

fn bench_parse(c: &mut Criterion) {
    let options = CompilerOptions::default();
    let file_id = FileId::default();
    bench_corpus(c, "parse_full", |source| {
        parse_program(source, &file_id, &options)
    });
}

fn bench_cst_parse(c: &mut Criterion) {
    let options = ParseOptions::default();
    bench_corpus(c, "parse_cst", |source| parse_source_file(source, &options));
}

criterion_group!(benches, bench_tokenize, bench_parse, bench_cst_parse);
criterion_main!(benches);
