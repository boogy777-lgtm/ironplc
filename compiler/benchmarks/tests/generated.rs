//! The generated project is the input of the analysis benchmarks: it must be
//! the same text on every run, a project the analysis accepts, and a project
//! whose state grows in proportion to its scale.

use ironplc_benchmarks::corpus::CorpusFile;
use ironplc_benchmarks::generated::{generate, options, SCALES};
use ironplc_dsl::core::FileId;
use ironplc_parser::parse_program;
use std::collections::HashMap;

/// What one structure, function block or program may hold at any scale, in
/// scalar variables, with the structures and the instances it holds counted in
/// full. The generator bounds how deep a declaration holds others, and this is
/// above what that depth allows; a nesting that grows with the number of
/// declarations passes it at the second scale.
const MOST_SCALARS: u64 = 10_000;

/// The type of every variable that each structure, function block, function and
/// program of the text declares, by the name of the declaration.
fn declared_types(files: &[CorpusFile]) -> HashMap<String, Vec<String>> {
    let mut declared: HashMap<String, Vec<String>> = HashMap::new();
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
    declared: &'a HashMap<String, Vec<String>>,
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
    for scale in SCALES {
        let first: Vec<String> = generate(scale).into_iter().map(|f| f.source).collect();
        let second: Vec<String> = generate(scale).into_iter().map(|f| f.source).collect();

        assert_eq!(first, second, "{}", scale.label());
    }
}

#[test]
fn generate_when_any_scale_then_no_declaration_holds_more_than_the_bound() {
    for scale in SCALES {
        let declared = declared_types(&generate(scale));

        let mut known = HashMap::new();

        let most = declared
            .keys()
            .map(|name| scalars(name, &declared, &mut known))
            .max()
            .unwrap();

        assert!(most > 1, "{}: no declaration was read", scale.label());
        assert!(
            most <= MOST_SCALARS,
            "{}: a declaration holds {most} scalars",
            scale.label()
        );
    }
}

#[test]
fn generate_when_smallest_scale_then_analysis_has_no_diagnostic() {
    let scale = SCALES.first().unwrap();
    let options = options();
    let libraries: Vec<_> = generate(scale)
        .iter()
        .map(|file| {
            parse_program(&file.source, &FileId::from_string(&file.name), &options).unwrap()
        })
        .collect();
    let references: Vec<_> = libraries.iter().collect();

    let (_, context) = ironplc_analyzer::stages::analyze(&references, &options).unwrap();

    let codes: Vec<&str> = context
        .diagnostics()
        .iter()
        .map(|diagnostic| diagnostic.code.as_str())
        .collect();
    assert!(codes.is_empty(), "{codes:?}");
}
