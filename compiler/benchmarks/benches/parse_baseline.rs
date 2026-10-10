//! Parse-only baseline: cold and warm timings plus allocation counts.
//!
//! Measures every path of the table in `ironplc_benchmarks::paths` over its set
//! of inputs: the shared corpus (`ironplc_benchmarks::corpus`) for whole
//! programs and the statement bodies of its units for fragments. The table has
//! the public functions of `ironplc-parser` (which run the front end built on
//! the lossless tree) and the entry points of `ironplc-syntax` (the stages they
//! are made of), and every path is measured by the same helpers, so rows are
//! directly comparable. Allocation counts come from `stats_alloc` installed
//! as this binary's global allocator, which is why this is a separate bench
//! target from the Criterion `parse_benchmark`.
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
#![expect(
    clippy::unwrap_used,
    reason = "benchmark target: panicking helpers are sanctioned in benchmarks"
)]

use ironplc_benchmarks::corpus::{corpus_dir, load_corpus};
use ironplc_benchmarks::paths::{Ctx, Input, Over, Path, Probe, PATHS};
use stats_alloc::{Region, StatsAlloc, INSTRUMENTED_SYSTEM};
use std::alloc::System;
use std::time::Instant;

#[global_allocator]
static GLOBAL: &StatsAlloc<System> = &INSTRUMENTED_SYSTEM;

const DEFAULT_REPEATS: usize = 50;

#[derive(Debug, Clone, Copy, Default)]
struct Measurement {
    micros: f64,
    allocations: usize,
    bytes: usize,
}

/// Times the call a path makes and counts the allocations made inside it. The
/// path drops what the call returned after `stop`, so the measurement covers
/// the call, not the caller's retention.
struct Meter {
    region: Option<Region<'static, System>>,
    started: Option<Instant>,
    measurement: Measurement,
}

impl Meter {
    fn new() -> Self {
        Self {
            region: None,
            started: None,
            measurement: Measurement::default(),
        }
    }
}

impl Probe for Meter {
    fn start(&mut self) {
        self.region = Some(Region::new(GLOBAL));
        self.started = Some(Instant::now());
    }

    fn stop(&mut self) {
        let elapsed = self.started.take().map(|start| start.elapsed());
        let stats = self.region.take().map(|region| region.change());
        self.measurement = Measurement {
            micros: elapsed.map_or(0.0, |elapsed| elapsed.as_secs_f64() * 1e6),
            allocations: stats.map_or(0, |stats| stats.allocations),
            bytes: stats.map_or(0, |stats| stats.bytes_allocated),
        };
    }
}

/// One call of `path` on `input`, measured.
fn measure(path: &Path, ctx: &Ctx, input: &Input) -> Measurement {
    let mut meter = Meter::new();
    path.call(ctx, input, &mut meter);
    meter.measurement
}

fn median<T: PartialOrd + Copy + Default>(values: &[T]) -> T {
    let mut sorted = values.to_vec();
    sorted.sort_by(|a, b| a.partial_cmp(b).unwrap_or(std::cmp::Ordering::Equal));
    sorted.get(sorted.len() / 2).copied().unwrap_or_default()
}

/// Cold (first call for this input) plus warm medians for one path.
#[derive(Clone)]
struct Row {
    cold: Measurement,
    warm_micros: f64,
    warm_allocations: usize,
}

fn measure_row(path: &Path, ctx: &Ctx, input: &Input, repeats: usize) -> Row {
    let cold = measure(path, ctx, input);
    let warm: Vec<Measurement> = (0..repeats).map(|_| measure(path, ctx, input)).collect();
    Row {
        cold,
        warm_micros: median(&warm.iter().map(|m| m.micros).collect::<Vec<_>>()),
        warm_allocations: median(&warm.iter().map(|m| m.allocations).collect::<Vec<_>>()),
    }
}

/// One input measured on one path.
#[derive(Clone)]
struct ItemRow<'a> {
    item: &'a Input,
    outcome: String,
    row: Row,
}

/// Everything measured for one path, in the order of the inputs.
struct PathResult<'a> {
    path: &'static Path,
    items: Vec<ItemRow<'a>>,
}

impl<'a> PathResult<'a> {
    /// What was measured on the inputs that `other` was measured on: a baseline
    /// may run over more inputs than the path that is compared with it.
    fn on_inputs_of(&self, other: &PathResult) -> PathResult<'a> {
        PathResult {
            path: self.path,
            items: self
                .items
                .iter()
                .filter(|item| other.item_named(&item.item.name).is_some())
                .cloned()
                .collect(),
        }
    }

    fn item_named(&self, name: &str) -> Option<&ItemRow<'a>> {
        self.items.iter().find(|item| item.item.name == name)
    }

    fn cold_micros(&self) -> f64 {
        self.items.iter().map(|i| i.row.cold.micros).sum()
    }
    fn cold_allocations(&self) -> usize {
        self.items.iter().map(|i| i.row.cold.allocations).sum()
    }
    fn cold_bytes(&self) -> f64 {
        self.items.iter().map(|i| i.row.cold.bytes as f64).sum()
    }
    fn warm_micros(&self) -> f64 {
        self.items.iter().map(|i| i.row.warm_micros).sum()
    }
    fn warm_allocations(&self) -> f64 {
        self.items
            .iter()
            .map(|i| i.row.warm_allocations as f64)
            .sum()
    }
}

fn main() {
    let repeats = std::env::args()
        .filter_map(|argument| argument.parse::<usize>().ok())
        .next_back()
        .unwrap_or(DEFAULT_REPEATS);
    let ctx = Ctx::default();

    // One-time init: each call is made twice; the difference in allocations
    // between the first and second call is the init cost. Plain input first,
    // so the located-variable row isolates what that input kind adds. A path
    // that runs after another has initialized the same lazy state shows none.
    println!("one-time init (tiny inputs, before the corpus loop):");
    println!("| call | first us | first allocs | second us | second allocs | init allocs |");
    println!("|---|---|---|---|---|---|");
    for path in PATHS {
        for (label, input) in path.over.iter().flat_map(|over| over.probes()) {
            let first = measure(path, &ctx, &input);
            let second = measure(path, &ctx, &input);
            println!(
                "| {}, {label} | {:.1} | {} | {:.1} | {} | {} |",
                path.name,
                first.micros,
                first.allocations,
                second.micros,
                second.allocations,
                first.allocations.saturating_sub(second.allocations),
            );
        }
    }
    println!();

    let files = load_corpus(&corpus_dir()).unwrap();
    let sets: Vec<(Over, Vec<Input>)> = Over::all()
        .into_iter()
        .map(|over| (over, over.items(&files)))
        .collect();
    for (over, items) in &sets {
        let bytes: usize = items.iter().map(|item| item.bytes()).sum();
        println!(
            "{}: {}, bytes: {bytes}, warm repeats per input: {repeats}",
            over.label(),
            items.len()
        );
    }

    let mut results: Vec<PathResult> = PATHS
        .iter()
        .map(|path| PathResult {
            path,
            items: Vec::new(),
        })
        .collect();
    for (over, items) in &sets {
        for item in items {
            // What each path makes of the input first (outside the
            // measurement), then the measurements.
            let outcomes: Vec<(usize, String)> = PATHS
                .iter()
                .enumerate()
                .filter(|(_, path)| path.runs_over(*over))
                .map(|(index, path)| (index, (path.describe)(&ctx, item)))
                .collect();
            for (index, outcome) in outcomes {
                let path = &PATHS[index];
                let row = measure_row(path, &ctx, item, repeats);
                results[index].items.push(ItemRow { item, outcome, row });
            }
        }
    }

    for result in &results {
        print_totals(result);
    }
    println!();
    print_ratios(&results);
    println!();
    print_items(&results);
}

fn ratio(numerator: f64, denominator: f64) -> f64 {
    if denominator == 0.0 {
        0.0
    } else {
        numerator / denominator
    }
}

/// Prints one pass over the inputs, summed, for one path.
fn print_totals(result: &PathResult) {
    println!(
        "totals ({}, one pass): cold {:.3} ms, warm-median sum {:.3} ms, {} cold allocations, {:.1} KiB allocated cold",
        result.path.name,
        result.cold_micros() / 1000.0,
        result.warm_micros() / 1000.0,
        result.cold_allocations(),
        result.cold_bytes() / 1024.0,
    );
}

/// For every path with a baseline: the sums against the baseline's, and the
/// input on which the warm ratio and the allocation ratio are worst.
fn print_ratios(results: &[PathResult]) {
    println!("against the baseline path (one pass over the inputs, summed):");
    println!("| path | baseline | cold ratio | warm-median ratio | cold allocations ratio | warm allocations ratio | cold KiB ratio | worst input warm ratio | worst input allocations ratio |");
    println!("|---|---|---|---|---|---|---|---|---|");
    for result in results {
        let Some(baseline) = result
            .path
            .baseline
            .and_then(|name| results.iter().find(|other| other.path.name == name))
            .map(|baseline| baseline.on_inputs_of(result))
        else {
            continue;
        };
        let worst = |select: &dyn Fn(&Row) -> f64| -> String {
            result
                .items
                .iter()
                .filter_map(|item| Some((item, baseline.item_named(&item.item.name)?)))
                .map(|(item, base)| {
                    (
                        ratio(select(&item.row), select(&base.row)),
                        item.item.name.as_str(),
                    )
                })
                .max_by(|a, b| a.0.partial_cmp(&b.0).unwrap_or(std::cmp::Ordering::Equal))
                .map_or("-".to_string(), |(value, name)| {
                    format!("{value:.2} ({name})")
                })
        };
        println!(
            "| {} | {} | {:.2} | {:.2} | {:.2} | {:.2} | {:.2} | {} | {} |",
            result.path.name,
            baseline.path.name,
            ratio(result.cold_micros(), baseline.cold_micros()),
            ratio(result.warm_micros(), baseline.warm_micros()),
            ratio(
                result.cold_allocations() as f64,
                baseline.cold_allocations() as f64
            ),
            ratio(result.warm_allocations(), baseline.warm_allocations()),
            ratio(result.cold_bytes(), baseline.cold_bytes()),
            worst(&|row| row.warm_micros),
            worst(&|row| row.warm_allocations as f64),
        );
    }
}

/// One table of every path on every input.
fn print_items(results: &[PathResult]) {
    println!("| path | input | bytes | outcome | cold us | cold allocs | cold KiB | warm med us | warm allocs | warm ratio | allocs ratio |");
    println!("|---|---|---|---|---|---|---|---|---|---|---|");
    for result in results {
        let baseline = result
            .path
            .baseline
            .and_then(|name| results.iter().find(|other| other.path.name == name));
        for item in &result.items {
            let (warm_ratio, allocations_ratio) =
                match baseline.and_then(|b| b.item_named(&item.item.name)) {
                    Some(base) => (
                        format!("{:.2}", ratio(item.row.warm_micros, base.row.warm_micros)),
                        format!(
                            "{:.2}",
                            ratio(
                                item.row.warm_allocations as f64,
                                base.row.warm_allocations as f64
                            )
                        ),
                    ),
                    None => (String::new(), String::new()),
                };
            println!(
                "| {} | {} | {} | {} | {:.1} | {} | {:.1} | {:.1} | {} | {} | {} |",
                result.path.name,
                item.item.name,
                item.item.bytes(),
                item.outcome,
                item.row.cold.micros,
                item.row.cold.allocations,
                item.row.cold.bytes as f64 / 1024.0,
                item.row.warm_micros,
                item.row.warm_allocations,
                warm_ratio,
                allocations_ratio,
            );
        }
    }
}
