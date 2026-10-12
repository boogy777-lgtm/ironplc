//! What each pass and each semantic rule of the analysis costs: time and
//! allocations, cold and warm, on every input of the generated projects.
//!
//! The analysis tells the observer of the benchmarks (`ironplc_benchmarks::profile`)
//! about each of its steps; this binary installs the counting allocator the
//! observer's meter reads, runs `analyze_observed` on each input and prints the
//! cost of each step. The first input is a program of one statement, which is
//! the cost of one call that has nothing to analyze (the fixed cost) and, being
//! the first call of the process, what the process pays once. Then each set of
//! generated projects, smallest scale first: the first call on an input is
//! cold, the calls after it are warm.
//!
//! Run with: `cargo bench --package ironplc-benchmarks --bench analysis_profile`
//! (optionally `-- <warm-runs>`, default 3). `IRONPLC_BENCH_SCALES=<n>` limits
//! the run to the `n` smallest scales.

// Benchmark-target boundary: an input that does not parse is a
// benchmark-authoring bug, not user input.
#![expect(
    clippy::unwrap_used,
    reason = "benchmark target: panicking helpers are sanctioned in benchmarks"
)]

use ironplc_analyzer::observe::{Kind, Step};
use ironplc_analyzer::stages::analyze_observed;
use ironplc_benchmarks::corpus::CorpusFile;
use ironplc_benchmarks::paths::{Ctx, Input, Over, PLAIN_SOURCE};
use ironplc_benchmarks::profile::{Cost, Meter, Profile, Sample};
use ironplc_benchmarks::project;
use ironplc_dsl::common::Library;
use ironplc_dsl::stack::within_stack_budget;
use ironplc_parser::options::CompilerOptions;
use stats_alloc::{StatsAlloc, INSTRUMENTED_SYSTEM};
use std::alloc::System;
use std::time::Instant;

#[global_allocator]
static GLOBAL: &StatsAlloc<System> = &INSTRUMENTED_SYSTEM;

const DEFAULT_WARM_RUNS: usize = 3;

/// Reads the clock and the counting allocator.
struct Process {
    started: Instant,
}

impl Meter for Process {
    fn sample(&self) -> Sample {
        let stats = GLOBAL.stats();
        Sample {
            nanos: self.started.elapsed().as_nanos() as u64,
            allocations: stats.allocations,
            bytes: stats.bytes_allocated,
            live: stats
                .bytes_allocated
                .saturating_sub(stats.bytes_deallocated),
        }
    }
}

/// What one analysis of an input cost, whole and by step.
struct Run {
    total: Cost,
    steps: Vec<(Step, Cost)>,
}

fn analyze_once(meter: &Process, libraries: &[Library], options: &CompilerOptions) -> Run {
    let profile = Profile::new(meter);
    let references: Vec<&Library> = libraries.iter().collect();
    let before = meter.sample();
    let result = within_stack_budget(|| analyze_observed(&references, options, &profile));
    let total = Cost::between(before, meter.sample());
    drop(std::hint::black_box(result));
    Run {
        total,
        steps: profile.into_steps(),
    }
}

fn median(mut values: Vec<u64>) -> u64 {
    values.sort();
    values.get(values.len() / 2).copied().unwrap_or_default()
}

/// The cost of a step in a run, or nothing when the run did not make the step.
fn cost_of(run: &Run, step: &Step) -> Cost {
    run.steps
        .iter()
        .find(|(known, _)| known == step)
        .map(|(_, cost)| *cost)
        .unwrap_or_default()
}

/// The median of one field of a step over the runs.
fn warm_median(warm: &[Run], step: &Step, field: fn(&Cost) -> u64) -> u64 {
    median(
        warm.iter()
            .map(|run| field(&cost_of(run, step)))
            .collect::<Vec<_>>(),
    )
}

fn kind_label(kind: Kind) -> &'static str {
    match kind {
        Kind::Setup => "setup",
        Kind::Pass => "pass",
        Kind::Rule => "rule",
    }
}

fn ms(nanos: u64) -> f64 {
    nanos as f64 / 1e6
}

fn kib(bytes: u64) -> f64 {
    bytes as f64 / 1024.0
}

fn share(part: u64, whole: u64) -> f64 {
    if whole == 0 {
        0.0
    } else {
        part as f64 * 100.0 / whole as f64
    }
}

/// The sums over the steps of one kind.
#[derive(Default)]
struct Sums {
    nanos: u64,
    allocations: u64,
    bytes: u64,
}

/// One input: the cold run, the warm runs and the table of every step.
fn report(label: &str, bytes: usize, cold: &Run, warm: &[Run]) {
    let warm_total = median(warm.iter().map(|run| run.total.nanos).collect());
    let warm_allocations = median(
        warm.iter()
            .map(|run| run.total.allocations as u64)
            .collect(),
    );
    let warm_bytes = median(warm.iter().map(|run| run.total.bytes as u64).collect());
    println!("\n## {label} ({bytes} bytes)\n");
    println!(
        "whole call: cold {:.3} ms, warm median {:.3} ms ({} warm runs); allocations cold {}, warm {}; KiB allocated cold {:.0}, warm {:.0}; live KiB at the end of the call, cold {:.0}",
        ms(cold.total.nanos),
        ms(warm_total),
        warm.len(),
        cold.total.allocations,
        warm_allocations,
        kib(cold.total.bytes as u64),
        kib(warm_bytes),
        kib(cold.total.live_after as u64),
    );
    println!();
    println!("| kind | step | calls | cold ms | warm ms | warm % of call | cold allocs | warm allocs | warm KiB | live KiB after (max) |");
    println!("|---|---|---|---|---|---|---|---|---|---|");
    let mut by_kind: Vec<(Kind, Sums)> = Vec::new();
    for (step, cold_cost) in &cold.steps {
        let nanos = warm_median(warm, step, |cost| cost.nanos);
        let allocations = warm_median(warm, step, |cost| cost.allocations as u64);
        let bytes = warm_median(warm, step, |cost| cost.bytes as u64);
        let live = warm
            .iter()
            .map(|run| cost_of(run, step).live_after)
            .max()
            .unwrap_or(0);
        println!(
            "| {} | {} | {} | {:.3} | {:.3} | {:.1} | {} | {} | {:.1} | {:.0} |",
            kind_label(step.kind),
            step.name,
            cold_cost.calls,
            ms(cold_cost.nanos),
            ms(nanos),
            share(nanos, warm_total),
            cold_cost.allocations,
            allocations,
            kib(bytes),
            kib(live as u64),
        );
        let position = match by_kind.iter().position(|(kind, _)| *kind == step.kind) {
            Some(position) => position,
            None => {
                by_kind.push((step.kind, Sums::default()));
                by_kind.len() - 1
            }
        };
        if let Some((_, sums)) = by_kind.get_mut(position) {
            sums.nanos += nanos;
            sums.allocations += allocations;
            sums.bytes += bytes;
        }
    }
    println!();
    println!("| by kind | warm ms | warm % of call | warm allocs | warm KiB |");
    println!("|---|---|---|---|---|");
    let mut attributed = Sums::default();
    for (kind, sums) in &by_kind {
        println!(
            "| {} | {:.3} | {:.1} | {} | {:.1} |",
            kind_label(*kind),
            ms(sums.nanos),
            share(sums.nanos, warm_total),
            sums.allocations,
            kib(sums.bytes)
        );
        attributed.nanos += sums.nanos;
        attributed.allocations += sums.allocations;
        attributed.bytes += sums.bytes;
    }
    let rest = warm_total.saturating_sub(attributed.nanos);
    println!(
        "| not in a step | {:.3} | {:.1} | {} | {:.1} |",
        ms(rest),
        share(rest, warm_total),
        warm_allocations.saturating_sub(attributed.allocations),
        kib(warm_bytes.saturating_sub(attributed.bytes)),
    );
}

fn libraries_of(files: &[CorpusFile], options: &CompilerOptions) -> Vec<Library> {
    project::parse_files(files, options).unwrap()
}

/// Measures the input: one cold run, then `warm_runs` warm ones.
fn measure(
    meter: &Process,
    label: &str,
    input: &Input,
    options: &CompilerOptions,
    warm_runs: usize,
) {
    let libraries = libraries_of(&input.files, options);
    let cold = analyze_once(meter, &libraries, options);
    let warm: Vec<Run> = (0..warm_runs)
        .map(|_| analyze_once(meter, &libraries, options))
        .collect();
    report(label, input.bytes(), &cold, &warm);
}

fn main() {
    let warm_runs = std::env::args()
        .filter_map(|argument| argument.parse::<usize>().ok())
        .next_back()
        .unwrap_or(DEFAULT_WARM_RUNS);
    let ctx = Ctx::default();
    let meter = Process {
        started: Instant::now(),
    };

    println!("# analysis by step: time and allocations, cold and warm");
    println!("warm runs per input: {warm_runs}");

    // The first call of the process, on a program that has nothing to analyze.
    let tiny = Input::text("one statement", PLAIN_SOURCE);
    measure(
        &meter,
        "one statement (first call of the process)",
        &tiny,
        &ctx.options,
        warm_runs,
    );

    for over in Over::all() {
        let Over::Generated(_) = over else {
            continue;
        };
        for input in over.items(&[]) {
            measure(&meter, &input.name, &input, &ctx.project_options, warm_runs);
        }
    }
}
