//! What the analysis gives for the programs of the corpus and for the
//! generated project is pinned: the library it produces (as a digest of its
//! text) and the diagnostics it reports, in order.
//!
//! The pins are data (`tests/data/analysis_pins.txt`), recorded by the
//! analysis as it was before the passes took one contract (a pass keeps what
//! it transformed and reports what it could not; no pass keeps a copy of the
//! library). A change of the analysis that changes what a correct program
//! produces changes a pin, and the test names the program.
//!
//! The data is rewritten, not edited, with
//! `cargo test -p ironplc-benchmarks --test analysis_pins -- --ignored write_pins`.

#![allow(
    clippy::unwrap_used,
    reason = "test target: panicking helpers are sanctioned in tests"
)]

use ironplc_benchmarks::corpus::{corpus_dir, load_corpus, CorpusFile};
use ironplc_benchmarks::generated::{generate, options, SCALES, SHAPES};
use ironplc_benchmarks::project;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_parser::options::{CompilerOptions, Dialect};
use std::fmt::Write;

const PINS: &str = include_str!("data/analysis_pins.txt");

/// The text with the numbers of declaration blocks replaced: a counter of the
/// process numbers them, so two analyses in one process number them apart.
fn without_block_counters(text: &str) -> String {
    let mut out = String::with_capacity(text.len());
    let mut rest = text;
    while let Some(at) = rest.find("block: ") {
        let (head, tail) = rest.split_at(at + "block: ".len());
        out.push_str(head);
        out.push('_');
        rest = tail.trim_start_matches(|c: char| c.is_ascii_digit());
    }
    out.push_str(rest);
    out
}

/// FNV-1a, 64 bits: a digest that does not change with the toolchain.
fn digest(text: &str) -> u64 {
    text.bytes().fold(0xcbf2_9ce4_8422_2325, |hash, byte| {
        (hash ^ u64::from(byte)).wrapping_mul(0x0000_0100_0000_01b3)
    })
}

fn diagnostic_line(diagnostic: &Diagnostic) -> String {
    format!("{} {}", diagnostic.code, diagnostic.primary.message)
}

/// The block of the pins file for one analysis of `files`.
fn render(title: &str, files: &[CorpusFile], options: &CompilerOptions) -> String {
    let mut block = format!("## {title}\n");
    match project::parse_files(files, options) {
        Err(error) => {
            writeln!(block, "parse {}", diagnostic_line(&error)).unwrap();
        }
        Ok(libraries) => match project::analyze_files(&libraries, options) {
            Err(diagnostics) => {
                for diagnostic in &diagnostics {
                    writeln!(block, "error {}", diagnostic_line(diagnostic)).unwrap();
                }
            }
            Ok((library, context)) => {
                let text = without_block_counters(&format!("{library:?}"));
                writeln!(
                    block,
                    "library {:016x} {} elements",
                    digest(&text),
                    library.elements.len()
                )
                .unwrap();
                for diagnostic in context.diagnostics() {
                    writeln!(block, "diagnostic {}", diagnostic_line(diagnostic)).unwrap();
                }
            }
        },
    }
    block
}

fn rusty() -> CompilerOptions {
    CompilerOptions {
        allow_top_level_var_global: true,
        ..CompilerOptions::from_dialect(Dialect::Rusty)
    }
}

/// Every program of the corpus alone, under the default options and under the
/// options that allow every extension; the generated project of the smallest
/// scale, both shapes.
fn current() -> String {
    let mut text = String::new();
    for file in load_corpus(&corpus_dir()).unwrap() {
        let files = [file];
        text += &render(
            &format!("{} [default]", files[0].name),
            &files,
            &CompilerOptions::default(),
        );
        text += &render(&format!("{} [rusty]", files[0].name), &files, &rusty());
    }
    for shape in SHAPES {
        let scale = &SCALES[0];
        text += &render(&scale.label(shape), &generate(shape, scale), &options());
    }
    text
}

/// The blocks of a pins text by title.
fn blocks(text: &str) -> Vec<(&str, &str)> {
    text.split("## ")
        .skip(1)
        .map(|block| block.split_once('\n').unwrap_or((block, "")))
        .collect()
}

#[test]
fn analysis_when_corpus_and_generated_project_then_library_and_diagnostics_are_the_pinned_ones() {
    let now = current();
    let now_blocks = blocks(&now);
    let pinned = blocks(PINS);

    let titles =
        |blocks: &[(&str, &str)]| blocks.iter().map(|b| b.0.to_string()).collect::<Vec<_>>();
    assert_eq!(titles(&pinned), titles(&now_blocks));
    let differing: Vec<String> = pinned
        .iter()
        .zip(&now_blocks)
        .filter(|(pin, seen)| pin.1 != seen.1)
        .map(|(pin, seen)| format!("{}\n  pinned:\n{}\n  now:\n{}", pin.0, pin.1, seen.1))
        .collect();
    assert!(
        differing.is_empty(),
        "programs whose analysis differs from the pins:\n{}",
        differing.join("\n")
    );
}

#[test]
#[ignore = "rewrites tests/data/analysis_pins.txt from the analysis as it is"]
fn write_pins() {
    let path =
        std::path::Path::new(env!("CARGO_MANIFEST_DIR")).join("tests/data/analysis_pins.txt");
    std::fs::write(path, current()).unwrap();
}
