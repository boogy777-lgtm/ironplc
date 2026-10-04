//! Parse-only baseline: cold and warm timings plus allocation counts.
//!
//! Drives `tokenize_program` and `parse_program` from `ironplc-parser`, and
//! `lex`, `parse_source_file` and `lower_library` from `ironplc-syntax` (the
//! lossless CST path and its lowering to the objects the legacy parser builds),
//! over the shared corpus (`ironplc_benchmarks::corpus`) and prints a
//! Markdown report. Both paths are measured by the same helpers so their rows
//! are directly comparable. Allocation counts come from `stats_alloc` installed as this
//! binary's global allocator, which is why this is a separate bench target
//! from the Criterion `parse_benchmark`.
//!
//! The first parse of a program with a located variable (`AT %IX0.0`) in a
//! process pays a one-time initialization cost: the direct-variable address
//! regexes in `ironplc-dsl` are compiled lazily (experiment section 4.2). A
//! corpus file that needs it would otherwise carry it in its cold figures, so
//! it is measured first, on tiny inputs, and reported on its own; the per-file
//! cold figures then describe the per-file path, not process start-up.
//!
//! Run with: `cargo bench --package ironplc-benchmarks --bench parse_baseline`
//! (optionally `-- <warm-repeats>`, default 50).

// Benchmark-target boundary: a corpus that cannot be read is a
// benchmark-authoring bug, not user input.
#![allow(
    clippy::unwrap_used,
    clippy::result_large_err,
    reason = "benchmark target: panicking helpers are sanctioned in benchmarks; the large Err type is parse_program's public signature"
)]

use ironplc_benchmarks::corpus::{corpus_dir, load_corpus};
use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use ironplc_parser::{parse_program, tokenize_program};
use ironplc_syntax::{lexer::lex, lower::lower_library, parse_source_file, ParseOptions};
use stats_alloc::{Region, StatsAlloc, INSTRUMENTED_SYSTEM};
use std::alloc::System;
use std::hint::black_box;
use std::time::Instant;

#[global_allocator]
static GLOBAL: &StatsAlloc<System> = &INSTRUMENTED_SYSTEM;

const DEFAULT_REPEATS: usize = 50;

/// A tiny input without any located variable: the baseline for the init probe.
const PLAIN_SOURCE: &str = "PROGRAM main
VAR
  x : INT;
END_VAR
  x := 1;
END_PROGRAM
";

/// A tiny input with located variables: triggers the lazy address regexes.
const LOCATED_SOURCE: &str = "PROGRAM main
VAR
  i AT %IX0.0 : BOOL;
  o AT %QW1 : WORD;
END_VAR
  o := 16#00FF;
END_PROGRAM
";

#[derive(Debug, Clone, Copy)]
struct Measurement {
    micros: f64,
    allocations: usize,
    bytes: usize,
}

/// Times `call` and counts allocations made inside it. The result is dropped
/// after the counters are read so the measurement covers the call, not the
/// caller's retention.
fn measure<T>(call: impl FnOnce() -> T) -> Measurement {
    let region = Region::new(GLOBAL);
    let start = Instant::now();
    let result = call();
    let elapsed = start.elapsed();
    let stats = region.change();
    drop(black_box(result));
    Measurement {
        micros: elapsed.as_secs_f64() * 1e6,
        allocations: stats.allocations,
        bytes: stats.bytes_allocated,
    }
}

fn median<T: PartialOrd + Copy + Default>(values: &[T]) -> T {
    let mut sorted = values.to_vec();
    sorted.sort_by(|a, b| a.partial_cmp(b).unwrap_or(std::cmp::Ordering::Equal));
    sorted.get(sorted.len() / 2).copied().unwrap_or_default()
}

/// Cold (first call for this file) plus warm medians for one entry point.
struct Row {
    cold: Measurement,
    warm_micros: f64,
    warm_allocations: usize,
}

fn measure_row<T>(repeats: usize, mut call: impl FnMut() -> T) -> Row {
    let cold = measure(&mut call);
    let warm: Vec<Measurement> = (0..repeats).map(|_| measure(&mut call)).collect();
    Row {
        cold,
        warm_micros: median(&warm.iter().map(|m| m.micros).collect::<Vec<_>>()),
        warm_allocations: median(&warm.iter().map(|m| m.allocations).collect::<Vec<_>>()),
    }
}

fn main() {
    let repeats = std::env::args()
        .filter_map(|argument| argument.parse::<usize>().ok())
        .next_back()
        .unwrap_or(DEFAULT_REPEATS);
    let options = CompilerOptions::default();
    let file_id = FileId::default();

    // One-time init: each call is made twice; the difference in allocations
    // between the first and second call is the init cost. Plain input first,
    // so the located-variable row isolates what that input kind adds.
    let cst_options = ParseOptions::default();
    type Probe<'a> = (&'a str, &'a str, Box<dyn Fn(&str) + 'a>);
    let probes: Vec<Probe> = vec![
        (
            "tokenize_program, plain",
            PLAIN_SOURCE,
            Box::new(|source| {
                black_box(tokenize_program(source, &file_id, &options, 0, 0).0.len());
            }),
        ),
        (
            "parse_program, plain",
            PLAIN_SOURCE,
            Box::new(|source| {
                black_box(parse_program(source, &file_id, &options).is_ok());
            }),
        ),
        (
            "parse_program, located variables",
            LOCATED_SOURCE,
            Box::new(|source| {
                black_box(parse_program(source, &file_id, &options).is_ok());
            }),
        ),
        (
            "cst lex, plain",
            PLAIN_SOURCE,
            Box::new(|source| {
                black_box(lex(source).0.len());
            }),
        ),
        (
            "cst parse_source_file, plain",
            PLAIN_SOURCE,
            Box::new(|source| {
                black_box(parse_source_file(source, &cst_options).is_ok());
            }),
        ),
        (
            "cst parse_source_file, located variables",
            LOCATED_SOURCE,
            Box::new(|source| {
                black_box(parse_source_file(source, &cst_options).is_ok());
            }),
        ),
    ];
    println!("one-time init (tiny inputs, before the corpus loop):");
    println!("| call | first us | first allocs | second us | second allocs | init allocs |");
    println!("|---|---|---|---|---|---|");
    for (name, source, call) in &probes {
        let first = measure(|| call(source));
        let second = measure(|| call(source));
        println!(
            "| {name} | {:.1} | {} | {:.1} | {} | {} |",
            first.micros,
            first.allocations,
            second.micros,
            second.allocations,
            first.allocations.saturating_sub(second.allocations),
        );
    }
    println!();

    let files = load_corpus(&corpus_dir()).unwrap();
    let total_bytes: usize = files.iter().map(|file| file.source.len()).sum();
    println!(
        "files: {}, bytes: {total_bytes}, warm repeats per file: {repeats}",
        files.len()
    );

    let mut rows = Vec::new();
    for file in &files {
        let tokens = tokenize_program(&file.source, &file_id, &options, 0, 0)
            .0
            .len();
        let status = match parse_program(&file.source, &file_id, &options) {
            Ok(_) => "ok".to_string(),
            Err(diagnostic) => format!("err[{}]", diagnostic.code.as_str()),
        };
        let cst_tokens = lex(&file.source).0.len();
        let cst_errors = parse_source_file(&file.source, &cst_options).errors.len();
        let tokenize = measure_row(repeats, || {
            tokenize_program(&file.source, &file_id, &options, 0, 0)
        });
        let parse = measure_row(repeats, || parse_program(&file.source, &file_id, &options));
        let cst_lex = measure_row(repeats, || lex(&file.source));
        let cst_parse = measure_row(repeats, || parse_source_file(&file.source, &cst_options));
        // The CST path as a consumer runs it: parse, then lower what parsed. A
        // file the parse rejects is not lowered, as the legacy path stops at
        // its first error.
        let cst_lower = measure_row(repeats, || {
            let parse = parse_source_file(&file.source, &cst_options);
            lower_library(&parse, &file_id)
        });
        rows.push(FileRow {
            file,
            tokens,
            status,
            tokenize,
            parse,
            cst_tokens,
            cst_errors,
            cst_lex,
            cst_parse,
            cst_lower,
        });
    }

    print_totals("tokenize", &rows, |row| &row.tokenize);
    print_totals("parse", &rows, |row| &row.parse);
    print_totals("cst lex", &rows, |row| &row.cst_lex);
    print_totals("cst parse", &rows, |row| &row.cst_parse);
    print_totals("cst parse + lower", &rows, |row| &row.cst_lower);
    print_ratios(&rows);
    println!();
    println!("| file | bytes | tokens | tok cold us | tok cold allocs | tok warm med us | parse cold us | parse cold allocs | parse cold KiB | parse warm med us | parse warm allocs | status |");
    println!("|---|---|---|---|---|---|---|---|---|---|---|---|");
    for row in &rows {
        println!(
            "| {} | {} | {} | {:.1} | {} | {:.1} | {:.1} | {} | {:.1} | {:.1} | {} | {} |",
            row.file.name,
            row.file.source.len(),
            row.tokens,
            row.tokenize.cold.micros,
            row.tokenize.cold.allocations,
            row.tokenize.warm_micros,
            row.parse.cold.micros,
            row.parse.cold.allocations,
            row.parse.cold.bytes as f64 / 1024.0,
            row.parse.warm_micros,
            row.parse.warm_allocations,
            row.status,
        );
    }
    println!();
    println!("cst (ironplc-syntax lex and parse_source_file, same corpus and method):");
    println!("| file | bytes | cst tokens | lex cold us | lex cold allocs | lex warm med us | cst parse cold us | cst parse cold allocs | cst parse cold KiB | cst parse warm med us | cst parse warm allocs | cst errors | parse warm ratio | parse allocs ratio | parse + lower warm med us | parse + lower warm allocs | parse + lower warm ratio | parse + lower allocs ratio |");
    println!("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
    for row in &rows {
        println!(
            "| {} | {} | {} | {:.1} | {} | {:.1} | {:.1} | {} | {:.1} | {:.1} | {} | {} | {:.2} | {:.2} | {:.1} | {} | {:.2} | {:.2} |",
            row.file.name,
            row.file.source.len(),
            row.cst_tokens,
            row.cst_lex.cold.micros,
            row.cst_lex.cold.allocations,
            row.cst_lex.warm_micros,
            row.cst_parse.cold.micros,
            row.cst_parse.cold.allocations,
            row.cst_parse.cold.bytes as f64 / 1024.0,
            row.cst_parse.warm_micros,
            row.cst_parse.warm_allocations,
            row.cst_errors,
            ratio(row.cst_parse.warm_micros, row.parse.warm_micros),
            ratio(
                row.cst_parse.cold.allocations as f64,
                row.parse.cold.allocations as f64
            ),
            row.cst_lower.warm_micros,
            row.cst_lower.warm_allocations,
            ratio(row.cst_lower.warm_micros, row.parse.warm_micros),
            ratio(
                row.cst_lower.warm_allocations as f64,
                row.parse.warm_allocations as f64
            ),
        );
    }
}

/// Everything measured for one corpus file: the legacy entry points and the
/// CST entry points, each as a cold-plus-warm [`Row`].
struct FileRow<'a> {
    file: &'a ironplc_benchmarks::corpus::CorpusFile,
    tokens: usize,
    status: String,
    tokenize: Row,
    parse: Row,
    cst_tokens: usize,
    cst_errors: usize,
    cst_lex: Row,
    cst_parse: Row,
    cst_lower: Row,
}

/// Prints the CST path (parse, then lower) against the legacy parse over one
/// corpus pass: the warm-median sum, and the allocations of a warm call, summed.
fn print_ratios<'a>(rows: &[FileRow<'a>]) {
    // The warm-median sum and the warm allocations, over the corpus, of the row
    // `select` picks.
    let sums = |select: &dyn for<'r> Fn(&'r FileRow<'a>) -> &'r Row| -> (f64, f64) {
        (
            rows.iter().map(|row| select(row).warm_micros).sum(),
            rows.iter()
                .map(|row| select(row).warm_allocations as f64)
                .sum(),
        )
    };
    let legacy = sums(&|row| &row.parse);
    let cst = sums(&|row| &row.cst_parse);
    let lowered = sums(&|row| &row.cst_lower);
    println!(
        "cst parse + lower against legacy parse (one pass): warm-median ratio {:.2}, warm allocations ratio {:.2}; cst parse alone: warm-median ratio {:.2}, warm allocations ratio {:.2}",
        ratio(lowered.0, legacy.0),
        ratio(lowered.1, legacy.1),
        ratio(cst.0, legacy.0),
        ratio(cst.1, legacy.1),
    );
}

fn ratio(numerator: f64, denominator: f64) -> f64 {
    if denominator == 0.0 {
        0.0
    } else {
        numerator / denominator
    }
}

/// Prints one corpus pass summed over every file for the row `select` picks.
fn print_totals<'a>(
    label: &str,
    rows: &[FileRow<'a>],
    select: impl for<'r> Fn(&'r FileRow<'a>) -> &'r Row,
) {
    let cold_micros: f64 = rows.iter().map(|row| select(row).cold.micros).sum();
    let cold_allocations: usize = rows.iter().map(|row| select(row).cold.allocations).sum();
    let cold_kib: f64 = rows
        .iter()
        .map(|row| select(row).cold.bytes as f64)
        .sum::<f64>()
        / 1024.0;
    let warm_micros: f64 = rows.iter().map(|row| select(row).warm_micros).sum();
    println!(
        "totals ({label}, one pass): cold {:.3} ms, warm-median sum {:.3} ms, {cold_allocations} cold allocations, {cold_kib:.1} KiB allocated cold",
        cold_micros / 1000.0,
        warm_micros / 1000.0,
    );
}
