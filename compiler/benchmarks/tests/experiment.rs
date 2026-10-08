//! Prototype branch only: the prototypes are switches in the analysis, and the
//! result with a switch on must be the result with it off.

#![allow(
    clippy::unwrap_used,
    reason = "test target: panicking helpers are sanctioned in tests"
)]

use ironplc_analyzer::experiment::NO_FALLBACK;
use ironplc_benchmarks::generated::{generate, options, PROBE, SCALES, SHAPES};
use ironplc_benchmarks::project;

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

/// The library and the diagnostics (code and message) of an analysis.
fn analysis(
    shape: &ironplc_benchmarks::generated::Shape,
    scale: &ironplc_benchmarks::generated::Scale,
) -> (String, Vec<String>) {
    let files = generate(shape, scale);
    let (library, context) = project::analyzed(&files, &options()).unwrap();
    let diagnostics = context
        .diagnostics()
        .iter()
        .map(|d| format!("{} {}", d.code, d.primary.message))
        .collect();
    (without_block_counters(&format!("{library:?}")), diagnostics)
}

#[test]
fn switches_when_on_then_analysis_result_is_the_one_with_them_off() {
    let inputs: Vec<_> = SHAPES
        .iter()
        .flat_map(|shape| [&PROBE, &SCALES[0], &SCALES[1]].map(|scale| (*shape, scale.clone())))
        .collect();
    for (shape, scale) in &inputs {
        NO_FALLBACK.set(0);
        let reference = analysis(shape, scale);
        for (name, switch, level) in [("no fallback", &NO_FALLBACK, 1)] {
            switch.set(level);
            let seen = analysis(shape, scale);
            switch.set(0);
            assert_eq!(
                reference.1,
                seen.1,
                "{name}: diagnostics of {}",
                scale.label(shape)
            );
            assert!(
                reference.0 == seen.0,
                "{name}: library of {}",
                scale.label(shape)
            );
        }
    }
}
