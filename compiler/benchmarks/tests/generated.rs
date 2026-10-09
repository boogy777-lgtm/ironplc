//! The generated project is the input of the analysis benchmarks: it must be
//! the same text on every run, a project the analysis accepts, and a project
//! whose state grows in proportion to its scale.

// Test-target boundary: a project that does not parse is a defect of the
// generator, and the test that reads it should stop there.
#![allow(
    clippy::unwrap_used,
    reason = "test target: panicking helpers are sanctioned in tests"
)]

use ironplc_benchmarks::corpus::CorpusFile;
use ironplc_benchmarks::generated::{generate, options, PROGRAM, SCALES, SHAPES};
use ironplc_dsl::common::Library;
use ironplc_dsl::core::FileId;
use ironplc_parser::parse_program;
use std::collections::{BTreeMap, HashMap};

/// What one structure, function block or program may hold at any scale, in
/// scalar variables, with the structures and the instances it holds counted in
/// full. The generator bounds how deep a declaration holds others, and this is
/// above what that depth allows; a nesting that grows with the number of
/// declarations passes it at the second scale.
const MOST_SCALARS: u64 = 10_000;

/// The type of every variable that each structure, function block, function and
/// program of the text declares, by the name of the declaration.
fn declared_types(files: &[CorpusFile]) -> BTreeMap<String, Vec<String>> {
    let mut declared: BTreeMap<String, Vec<String>> = BTreeMap::new();
    for file in files {
        let mut current = None;
        for line in file.source.lines() {
            let words: Vec<&str> = line.split_whitespace().collect();
            let unit = ["FUNCTION_BLOCK", "FUNCTION", "PROGRAM"]
                .iter()
                .any(|keyword| line.starts_with(keyword));
            match words.as_slice() {
                [_, name, ..] if unit => current = Some(name.to_string()),
                [name, ":"] => current = Some(name.to_string()),
                [_, ":", of] => {
                    if let (Some(name), Some(of)) = (&current, of.strip_suffix(';')) {
                        declared
                            .entry(name.clone())
                            .or_default()
                            .push(of.to_string());
                    }
                }
                _ => {}
            }
        }
    }
    declared
}

/// The scalar variables a variable of `of` is made of. `known` keeps what is
/// already counted, so a nesting of any size is counted in one pass over the
/// declarations.
fn scalars<'a>(
    of: &'a str,
    declared: &'a BTreeMap<String, Vec<String>>,
    known: &mut HashMap<&'a str, u64>,
) -> u64 {
    if let Some(count) = known.get(of) {
        return *count;
    }
    let count = match declared.get(of) {
        Some(types) => types.iter().fold(0u64, |sum, of| {
            sum.saturating_add(scalars(of, declared, known))
        }),
        None => 1,
    };
    known.insert(of, count);
    count
}

#[test]
fn generate_when_same_scale_then_same_text() {
    for shape in SHAPES {
        for scale in SCALES {
            let first: Vec<String> = generate(shape, scale)
                .into_iter()
                .map(|f| f.source)
                .collect();
            let second: Vec<String> = generate(shape, scale)
                .into_iter()
                .map(|f| f.source)
                .collect();

            assert_eq!(first, second, "{}", scale.label(shape));
        }
    }
}

#[test]
fn generate_when_any_scale_then_no_declaration_holds_more_than_the_bound() {
    for shape in SHAPES {
        for scale in SCALES {
            let declared = declared_types(&generate(shape, scale));

            let mut known = HashMap::new();

            let most = declared
                .keys()
                .filter(|name| !name.starts_with("Prog"))
                .map(|name| scalars(name, &declared, &mut known))
                .max()
                .unwrap();

            assert!(most > 1, "{}: no declaration was read", scale.label(shape));
            assert!(
                most <= MOST_SCALARS,
                "{}: a declaration holds {most} scalars",
                scale.label(shape)
            );
        }
    }
}

/// The libraries of the files of a project, parsed.
fn parsed(files: &[CorpusFile]) -> Vec<Library> {
    let options = options();
    files
        .iter()
        .map(|file| {
            parse_program(&file.source, &FileId::from_string(&file.name), &options).unwrap()
        })
        .collect()
}

#[test]
fn generate_when_smallest_scale_then_analysis_has_no_diagnostic() {
    let scale = SCALES.first().unwrap();
    for shape in SHAPES {
        let libraries = parsed(&generate(shape, scale));
        let references: Vec<_> = libraries.iter().collect();

        let (_, context) = ironplc_analyzer::stages::analyze(&references, &options()).unwrap();

        let codes: Vec<&str> = context
            .diagnostics()
            .iter()
            .map(|diagnostic| diagnostic.code.as_str())
            .collect();
        assert!(codes.is_empty(), "{}: {codes:?}", scale.label(shape));
    }
}

#[test]
fn generate_when_program_shape_then_one_program_holds_an_instance_of_every_function_block() {
    let files = generate(&PROGRAM, SCALES.first().unwrap());

    let programs = count_lines_starting(&files, "PROGRAM ");
    let blocks = count_lines_starting(&files, "FUNCTION_BLOCK ");
    let instances = count_lines_containing(&files, "    inst_");

    assert_eq!(1, programs);
    assert!(blocks > 1);
    assert_eq!(blocks, instances);
}

#[test]
fn generate_when_program_shape_at_smallest_scale_then_code_generation_succeeds() {
    let options = options();
    let libraries = parsed(&generate(&PROGRAM, SCALES.first().unwrap()));
    let references: Vec<_> = libraries.iter().collect();
    let (library, context) = ironplc_analyzer::stages::analyze(&references, &options).unwrap();

    let compiled = ironplc_codegen::compile(
        &library,
        &context,
        &ironplc_codegen::CodegenOptions::default(),
        &ironplc_codegen::EmptyLookup,
    );

    assert!(
        compiled.is_ok(),
        "{:?}",
        compiled.err().map(|d| (d.code, d.primary.message))
    );
}

fn count_lines_starting(files: &[CorpusFile], prefix: &str) -> usize {
    files
        .iter()
        .flat_map(|file| file.source.lines())
        .filter(|line| line.starts_with(prefix))
        .count()
}

fn count_lines_containing(files: &[CorpusFile], text: &str) -> usize {
    files
        .iter()
        .flat_map(|file| file.source.lines())
        .filter(|line| line.contains(text))
        .count()
}
