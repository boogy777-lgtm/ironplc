//! Prototype branch only: the prototypes are switches in the analysis, and the
//! result with a switch on must be the result with it off.

#![allow(
    clippy::unwrap_used,
    reason = "test target: panicking helpers are sanctioned in tests"
)]

use ironplc_analyzer::experiment::{NO_FALLBACK, STANDARD_ENVIRONMENTS};
use ironplc_benchmarks::generated::{generate, options, PROBE, SCALES, SHAPES};
use ironplc_benchmarks::project;
use std::sync::Mutex;

/// The switches are global: one test at a time sets them.
static SWITCHES: Mutex<()> = Mutex::new(());

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
    let _held = SWITCHES
        .lock()
        .unwrap_or_else(|poisoned| poisoned.into_inner());
    let inputs: Vec<_> = SHAPES
        .iter()
        .flat_map(|shape| [&PROBE, &SCALES[0], &SCALES[1]].map(|scale| (*shape, *scale)))
        .collect();
    for (shape, scale) in &inputs {
        NO_FALLBACK.set(0);
        STANDARD_ENVIRONMENTS.set(0);
        let reference = analysis(shape, scale);
        for (name, switch, level) in [
            ("no fallback", &NO_FALLBACK, 1),
            ("standard environments cloned", &STANDARD_ENVIRONMENTS, 1),
            ("standard environments layered", &STANDARD_ENVIRONMENTS, 2),
        ] {
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

/// The diagnostics of an analysis of one text.
fn diagnostics_of(text: &str) -> Vec<String> {
    let files = vec![ironplc_benchmarks::corpus::CorpusFile {
        name: "one.st".to_string(),
        path: Default::default(),
        source: text.to_string(),
    }];
    let analyzed = project::analyzed(&files, &options());
    match analyzed {
        Ok((_, context)) => context
            .diagnostics()
            .iter()
            .map(|d| format!("{} {}", d.code, d.primary.message))
            .collect(),
        Err(diagnostics) => diagnostics
            .iter()
            .map(|d| format!("{} {}", d.code, d.primary.message))
            .collect(),
    }
}

#[test]
fn switches_when_a_program_declares_a_standard_function_then_the_diagnostics_are_the_same() {
    let _held = SWITCHES
        .lock()
        .unwrap_or_else(|poisoned| poisoned.into_inner());
    let text = "FUNCTION SIN : REAL\n  VAR_INPUT x : REAL; END_VAR\n  SIN := x;\nEND_FUNCTION\nPROGRAM main\n  VAR r : REAL; END_VAR\n  r := SIN(1.0);\nEND_PROGRAM\n";
    STANDARD_ENVIRONMENTS.set(0);
    let reference = diagnostics_of(text);
    assert!(!reference.is_empty(), "the text should draw a diagnostic");
    for level in [1, 2] {
        STANDARD_ENVIRONMENTS.set(level);
        let seen = diagnostics_of(text);
        STANDARD_ENVIRONMENTS.set(0);
        assert_eq!(reference, seen, "level {level}");
    }
}

mod library_cache {
    use super::{analysis, without_block_counters, SWITCHES};
    use ironplc_benchmarks::generated::{PROGRAM, SCALES};
    use ironplc_benchmarks::project;
    use ironplc_sources::experiment::LIBRARY_CACHE;
    use ironplc_sources::libraries::{LibraryName, LibraryRegistry};
    use std::fs;

    #[test]
    fn library_cache_when_on_then_bundled_libraries_are_the_ones_parsed_each_time() {
        let _held = SWITCHES
            .lock()
            .unwrap_or_else(|poisoned| poisoned.into_inner());
        LIBRARY_CACHE.set(0);
        let reference =
            without_block_counters(&format!("{:?}", project::bundled_libraries().unwrap()));
        let reference_analysis = analysis(&PROGRAM, &SCALES[0]);

        LIBRARY_CACHE.set(1);
        let first = without_block_counters(&format!("{:?}", project::bundled_libraries().unwrap()));
        let second =
            without_block_counters(&format!("{:?}", project::bundled_libraries().unwrap()));
        let cached_analysis = analysis(&PROGRAM, &SCALES[0]);
        LIBRARY_CACHE.set(0);

        assert!(reference == first);
        assert!(reference == second);
        assert_eq!(reference_analysis.1, cached_analysis.1);
    }

    #[test]
    fn library_cache_when_a_library_file_changes_then_the_new_content_is_parsed() {
        let _held = SWITCHES
            .lock()
            .unwrap_or_else(|poisoned| poisoned.into_inner());
        let root = std::env::temp_dir().join(format!("ironplc-b3-{}", std::process::id()));
        let version = root.join("Lib").join("1.0.0");
        fs::create_dir_all(&version).unwrap();
        fs::write(
            root.join("Lib").join("library.toml"),
            "name = \"Lib\"\nvendor = \"v\"\ndefault_version = \"1.0.0\"\nreferences = [\"r\"]\n",
        )
        .unwrap();
        let file = version.join("a.st");
        let load = || {
            let registry = LibraryRegistry::with_root(&root);
            let loaded = registry.load(&LibraryName::new("Lib")).unwrap();
            without_block_counters(&format!("{:?}", loaded.library))
        };

        LIBRARY_CACHE.set(1);
        fs::write(
            &file,
            "FUNCTION F : DINT\n  VAR_INPUT a : DINT; END_VAR\n  F := a;\nEND_FUNCTION\n",
        )
        .unwrap();
        let before = load();
        let again = load();
        fs::write(
            &file,
            "FUNCTION F : DINT\n  VAR_INPUT a : DINT; END_VAR\n  F := a + 1;\nEND_FUNCTION\n",
        )
        .unwrap();
        let after = load();
        LIBRARY_CACHE.set(0);
        let _ = fs::remove_dir_all(&root);

        assert!(before == again);
        assert!(before != after);
    }
}
