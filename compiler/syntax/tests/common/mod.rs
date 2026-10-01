//! Helpers shared by the integration tests.
#![allow(dead_code, clippy::unwrap_used)]

use ironplc_syntax::lexer::{check_coverage, lex, Token};
use ironplc_syntax::SyntaxKind;
use std::path::{Path, PathBuf};

/// Every `.st` file under `relative` (a path from the crate manifest), sorted.
pub fn st_files(relative: &str) -> Vec<PathBuf> {
    let root = Path::new(env!("CARGO_MANIFEST_DIR")).join(relative);
    let mut files = Vec::new();
    collect(&root, &mut files);
    files.sort();
    files
}

fn collect(dir: &Path, files: &mut Vec<PathBuf>) {
    for entry in std::fs::read_dir(dir).unwrap() {
        let path = entry.unwrap().path();
        if path.is_dir() {
            collect(&path, files);
        } else if path.extension().is_some_and(|ext| ext == "st") {
            files.push(path);
        }
    }
}

/// The corpus the tiling guarantees are checked against: the shared test
/// resources and the S0 spike fixtures, read in place.
pub fn corpus() -> Vec<(PathBuf, String)> {
    let mut files = st_files("../resources/test");
    files.extend(st_files("../s0-spike/fixtures"));
    assert!(
        files.len() > 40,
        "corpus unexpectedly small: {}",
        files.len()
    );
    files
        .into_iter()
        .map(|path| {
            let text = std::fs::read_to_string(&path).unwrap();
            (path, text)
        })
        .collect()
}

/// Asserts the token stream of `source` tiles it exactly and returns it.
pub fn lex_exact(source: &str) -> Vec<Token<'_>> {
    let (tokens, _) = lex(source);
    let coverage = check_coverage(source, &tokens);
    assert!(coverage.is_exact(), "{coverage:?} for {source:?}");
    let joined: String = tokens.iter().map(|token| token.text).collect();
    assert_eq!(joined, source);
    tokens
}

/// Lexes `source` losslessly and returns `(kind, text)` pairs.
pub fn kinds(source: &str) -> Vec<(SyntaxKind, &str)> {
    lex_exact(source)
        .into_iter()
        .map(|token| (token.kind, token.text))
        .collect()
}

/// Like [`kinds`] but without trivia.
pub fn significant(source: &str) -> Vec<(SyntaxKind, &str)> {
    kinds(source)
        .into_iter()
        .filter(|(kind, _)| !kind.is_trivia())
        .collect()
}
