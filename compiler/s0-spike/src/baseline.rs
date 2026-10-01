//! Parse baseline for S0 (design section 5: "benchmark baseline").
//!
//! Drives the **current production** parse path — `tokenize_program` and
//! `parse_program` from `ironplc-parser` — over the shared corpus in
//! `compiler/resources/test/` and the CODESYS-dialect fixtures in this
//! crate's `fixtures/codesys/`. For each file it records wall time and
//! allocation counts for a cold call (the first call for that file in the
//! run) and medians over warm repeats.
//!
//! Spike-only: no production crate depends on this module, and the numbers
//! live in `specs/design/parse-tree-s0-experiment.md`.

use ironplc_dsl::core::FileId;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_parser::{parse_program, tokenize_program};
use stats_alloc::{Region, Stats, StatsAlloc};
use std::alloc::GlobalAlloc;
use std::fmt::Write as _;
use std::fs;
use std::path::{Path, PathBuf};
use std::time::Instant;

/// One corpus entry: a file plus the dialect its options come from.
#[derive(Debug, Clone)]
pub struct CorpusFile {
    /// Report label, e.g. `std/if.st` or `codesys/pragmas.st`.
    pub label: String,
    pub path: PathBuf,
    pub dialect: Dialect,
}

/// Timed + allocation-counted samples for one measurement point.
#[derive(Debug, Default, Clone)]
pub struct Sample {
    pub micros: Vec<f64>,
    pub allocations: Vec<usize>,
    pub bytes_allocated: Vec<usize>,
}

impl Sample {
    fn push(&mut self, measurement: Measurement) {
        self.micros.push(measurement.micros);
        self.allocations.push(measurement.allocations);
        self.bytes_allocated.push(measurement.bytes_allocated);
    }

    pub fn median_micros(&self) -> f64 {
        median_f64(&self.micros)
    }

    pub fn median_allocations(&self) -> usize {
        median_usize(&self.allocations)
    }

    pub fn median_bytes(&self) -> usize {
        median_usize(&self.bytes_allocated)
    }
}

/// Per-file baseline row.
#[derive(Debug)]
pub struct FileBaseline {
    pub label: String,
    pub dialect: Dialect,
    pub bytes: usize,
    pub tokens: usize,
    pub cold_parse: Sample,
    pub warm_parse: Sample,
    pub cold_tokenize: Sample,
    pub warm_tokenize: Sample,
    pub cold_status: String,
}

/// A complete baseline run.
#[derive(Debug)]
pub struct BaselineRun {
    pub repeats: usize,
    pub files: Vec<FileBaseline>,
    /// Parse acceptance over the standard corpus per preset: (dialect, ok, total).
    pub acceptance: Vec<(Dialect, usize, usize)>,
}

/// Counts `parse_program` acceptances for the standard corpus under every
/// dialect preset. Parse-level only (no analyzer), so the numbers describe
/// grammar/flag coverage rather than semantic validity.
pub fn dialect_acceptance(manifest_dir: &Path) -> Result<Vec<(Dialect, usize, usize)>, String> {
    let mut standard = Vec::new();
    collect(
        &manifest_dir.join("../resources/test"),
        "std",
        Dialect::Iec61131_3Ed2,
        &mut standard,
    )?;
    let mut sources = Vec::new();
    for file in &standard {
        let source = fs::read_to_string(&file.path)
            .map_err(|error| format!("cannot read {}: {error}", file.path.display()))?;
        sources.push(source);
    }

    let mut acceptance = Vec::new();
    for dialect in Dialect::ALL {
        let options = CompilerOptions::from_dialect(*dialect);
        let accepted = sources
            .iter()
            .filter(|source| parse_program(source, &FileId::default(), &options).is_ok())
            .count();
        acceptance.push((*dialect, accepted, sources.len()));
    }
    Ok(acceptance)
}

/// Enumerates the corpus: standard resources plus CODESYS fixtures.
///
/// `manifest_dir` is `compiler/s0-spike` (the value of `CARGO_MANIFEST_DIR`).
pub fn corpus_files(manifest_dir: &Path) -> Result<Vec<CorpusFile>, String> {
    let mut files = Vec::new();
    collect(
        &manifest_dir.join("../resources/test"),
        "std",
        Dialect::Iec61131_3Ed2,
        &mut files,
    )?;
    collect(
        &manifest_dir.join("fixtures/codesys"),
        "codesys",
        Dialect::Codesys,
        &mut files,
    )?;
    files.sort_by(|a, b| a.label.cmp(&b.label));
    Ok(files)
}

fn collect(
    dir: &Path,
    prefix: &str,
    dialect: Dialect,
    files: &mut Vec<CorpusFile>,
) -> Result<(), String> {
    let entries = fs::read_dir(dir)
        .map_err(|error| format!("cannot read corpus directory {}: {error}", dir.display()))?;
    for entry in entries {
        let entry = entry.map_err(|error| format!("cannot read directory entry: {error}"))?;
        let path = entry.path();
        if path.extension().is_some_and(|extension| extension == "st") {
            let name = path
                .file_name()
                .and_then(|name| name.to_str())
                .ok_or_else(|| format!("non-UTF-8 file name in {}", dir.display()))?;
            files.push(CorpusFile {
                label: format!("{prefix}/{name}"),
                path,
                dialect,
            });
        }
    }
    Ok(())
}

/// Runs the baseline: for every corpus file, one cold call plus `repeats`
/// warm calls of the parse and tokenize paths.
pub fn run<A: GlobalAlloc>(alloc: &StatsAlloc<A>, repeats: usize) -> Result<BaselineRun, String> {
    let manifest_dir = Path::new(env!("CARGO_MANIFEST_DIR"));
    let files = corpus_files(manifest_dir)?;
    let mut results = Vec::new();

    for file in files {
        let source = fs::read_to_string(&file.path)
            .map_err(|error| format!("cannot read {}: {error}", file.path.display()))?;
        let options = CompilerOptions::from_dialect(file.dialect);

        let cold_parse = sample(1, || measure_parse(alloc, &source, &options));
        let (cold_status, tokens) = {
            let (status, tokens) = parse_status(&source, &options);
            (status, tokens)
        };
        let cold_tokenize = sample(1, || measure_tokenize(alloc, &source, &options));
        let warm_parse = sample(repeats, || measure_parse(alloc, &source, &options));
        let warm_tokenize = sample(repeats, || measure_tokenize(alloc, &source, &options));

        results.push(FileBaseline {
            label: file.label,
            dialect: file.dialect,
            bytes: source.len(),
            tokens,
            cold_parse,
            warm_parse,
            cold_tokenize,
            warm_tokenize,
            cold_status,
        });
    }

    let acceptance = dialect_acceptance(manifest_dir)?;

    Ok(BaselineRun {
        repeats,
        files: results,
        acceptance,
    })
}

fn sample<F: FnMut() -> Measurement>(count: usize, mut measure: F) -> Sample {
    let mut sample = Sample::default();
    for _ in 0..count {
        sample.push(measure());
    }
    sample
}

#[derive(Debug, Clone, Copy)]
struct Measurement {
    micros: f64,
    allocations: usize,
    bytes_allocated: usize,
}

fn measure_parse<A: GlobalAlloc>(
    alloc: &StatsAlloc<A>,
    source: &str,
    options: &CompilerOptions,
) -> Measurement {
    let region = Region::new(alloc);
    let start = Instant::now();
    let result = parse_program(source, &FileId::default(), options);
    let elapsed = start.elapsed();
    let stats = region.change();
    // The AST is released outside the timed region on purpose: the baseline
    // measures the parse path, not the caller's retention.
    drop(result);
    from_stats(&stats, elapsed.as_secs_f64() * 1e6)
}

fn measure_tokenize<A: GlobalAlloc>(
    alloc: &StatsAlloc<A>,
    source: &str,
    options: &CompilerOptions,
) -> Measurement {
    let region = Region::new(alloc);
    let start = Instant::now();
    let result = tokenize_program(source, &FileId::default(), options, 0, 0);
    let elapsed = start.elapsed();
    let stats = region.change();
    drop(result);
    from_stats(&stats, elapsed.as_secs_f64() * 1e6)
}

fn from_stats(stats: &Stats, micros: f64) -> Measurement {
    Measurement {
        micros,
        allocations: stats.allocations,
        bytes_allocated: stats.bytes_allocated,
    }
}

fn parse_status(source: &str, options: &CompilerOptions) -> (String, usize) {
    let (tokens, diagnostics) = tokenize_program(source, &FileId::default(), options, 0, 0);
    let status = match parse_program(source, &FileId::default(), options) {
        Ok(_) => "ok".to_string(),
        Err(diagnostic) => format!("err[{}]", diagnostic.code.as_str()),
    };
    if status == "ok" && !diagnostics.is_empty() {
        return (format!("tokerr[{}]", diagnostics.len()), tokens.len());
    }
    (status, tokens.len())
}

fn median_f64(values: &[f64]) -> f64 {
    if values.is_empty() {
        return 0.0;
    }
    let mut sorted = values.to_vec();
    sorted.sort_by(|a, b| a.partial_cmp(b).unwrap_or(std::cmp::Ordering::Equal));
    sorted[sorted.len() / 2]
}

fn median_usize(values: &[usize]) -> usize {
    if values.is_empty() {
        return 0;
    }
    let mut sorted = values.to_vec();
    sorted.sort_unstable();
    sorted[sorted.len() / 2]
}

/// Renders the run as Markdown rows plus totals.
pub fn report(run: &BaselineRun) -> String {
    let mut out = String::new();
    let total_bytes: usize = run.files.iter().map(|file| file.bytes).sum();
    let total_cold_micros: f64 = run
        .files
        .iter()
        .map(|file| file.cold_parse.median_micros())
        .sum();
    let total_cold_allocs: usize = run
        .files
        .iter()
        .map(|file| file.cold_parse.median_allocations())
        .sum();
    let total_cold_kib: f64 = run
        .files
        .iter()
        .map(|file| file.cold_parse.median_bytes() as f64)
        .sum::<f64>()
        / 1024.0;

    let _ = writeln!(
        out,
        "files: {}, bytes: {}, warm repeats per file: {}",
        run.files.len(),
        total_bytes,
        run.repeats
    );
    let _ = writeln!(
        out,
        "totals (cold, one pass): {:.3} ms, {} allocations, {:.1} KiB allocated",
        total_cold_micros / 1000.0,
        total_cold_allocs,
        total_cold_kib
    );
    let acceptance: Vec<String> = run
        .acceptance
        .iter()
        .map(|(dialect, accepted, total)| format!("{} {accepted}/{total}", dialect.cli_name()))
        .collect();
    let _ = writeln!(
        out,
        "standard corpus parse acceptance (parse_program): {}",
        acceptance.join(", ")
    );
    let _ = writeln!(out);
    let _ = writeln!(
        out,
        "| file | dialect | bytes | tokens | cold us | cold allocs | cold KiB | warm med us | warm allocs | tok cold us | tok cold allocs | tok warm med us | status |"
    );
    let _ = writeln!(out, "|---|---|---|---|---|---|---|---|---|---|---|---|---|");

    for file in &run.files {
        let _ = writeln!(
            out,
            "| {} | {} | {} | {} | {:.1} | {} | {:.1} | {:.1} | {} | {:.1} | {} | {:.1} | {} |",
            file.label,
            file.dialect.cli_name(),
            file.bytes,
            file.tokens,
            file.cold_parse.median_micros(),
            file.cold_parse.median_allocations(),
            file.cold_parse.median_bytes() as f64 / 1024.0,
            file.warm_parse.median_micros(),
            file.warm_parse.median_allocations(),
            file.cold_tokenize.median_micros(),
            file.cold_tokenize.median_allocations(),
            file.warm_tokenize.median_micros(),
            file.cold_status,
        );
    }

    out
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn median_f64_when_values_then_middle() {
        assert_eq!(median_f64(&[3.0, 1.0, 2.0]), 2.0);
        assert_eq!(median_f64(&[]), 0.0);
    }

    #[test]
    fn median_usize_when_values_then_middle() {
        assert_eq!(median_usize(&[5, 1, 3]), 3);
        assert_eq!(median_usize(&[]), 0);
    }

    #[test]
    fn corpus_files_when_run_then_includes_both_sources() {
        let manifest_dir = Path::new(env!("CARGO_MANIFEST_DIR"));
        let files = corpus_files(manifest_dir).unwrap();
        assert!(files.iter().any(|file| file.label.starts_with("std/")));
        assert!(files.iter().any(|file| file.label.starts_with("codesys/")));
        assert!(files.iter().all(|file| file.path.is_file()));
    }

    #[test]
    fn corpus_files_when_missing_directory_then_error() {
        let missing = Path::new("does/not/exist");
        assert!(corpus_files(missing).is_err());
    }

    #[test]
    fn dialect_acceptance_when_run_then_counts_every_preset() {
        let manifest_dir = Path::new(env!("CARGO_MANIFEST_DIR"));
        let acceptance = dialect_acceptance(manifest_dir).unwrap();
        assert_eq!(acceptance.len(), Dialect::ALL.len());
        assert!(acceptance.iter().all(|(_, _, total)| *total == 48));
        assert!(acceptance.iter().any(|(_, accepted, _)| *accepted > 0));
    }
}
