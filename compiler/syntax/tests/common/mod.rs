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
/// resources, read in place, and this crate's fixtures.
pub fn corpus() -> Vec<(PathBuf, String)> {
    let mut files = st_files("../resources/test");
    files.extend(st_files("tests/fixtures"));
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

/// Renders `node` as nested text with trivia left out: a node is
/// `Kind(child child ...)`, a token is its source text. Shape tests compare
/// against this so they assert the tree's structure, not only success.
pub fn render(node: &ironplc_syntax::SyntaxNode) -> String {
    let parts: Vec<String> = node
        .children_with_tokens()
        .filter_map(|element| match element {
            ironplc_syntax::SyntaxElement::Node(child) => Some(render(&child)),
            ironplc_syntax::SyntaxElement::Token(token) if token.kind().is_trivia() => None,
            ironplc_syntax::SyntaxElement::Token(token) => Some(token.text().to_string()),
        })
        .collect();
    format!("{:?}({})", node.kind(), parts.join(" "))
}

/// The rendering of the tree of one statement list, without the file root.
pub fn statements(source: &str, options: &ironplc_syntax::ParseOptions) -> String {
    let parsed = ironplc_syntax::parse_statements(source, options);
    parsed
        .root
        .first_child()
        .map(|list| render(&list))
        .unwrap_or_default()
}

/// The rendering of the tree of one expression.
pub fn expression(source: &str, options: &ironplc_syntax::ParseOptions) -> String {
    let parsed = ironplc_syntax::parse_expression(source, options);
    parsed
        .root
        .first_child()
        .map(|node| render(&node))
        .unwrap_or_default()
}

/// The rendering of the top-level nodes of a file, trivia left out, joined by
/// a space.
pub fn file(source: &str, options: &ironplc_syntax::ParseOptions) -> String {
    let parsed = ironplc_syntax::parse_source_file(source, options);
    parsed
        .root
        .children()
        .map(|node| render(&node))
        .collect::<Vec<_>>()
        .join(" ")
}

/// The files compared, named by their path from the crate manifest's
/// directory (`../resources/test/oop.st`, `tests/fixtures/codesys/x.st`),
/// each with its CRLF and tab-indented spellings (`name (CRLF)`,
/// `name (tabs)`).
pub fn file_variants() -> Vec<(String, String)> {
    let manifest = Path::new(env!("CARGO_MANIFEST_DIR"));
    let mut paths: Vec<PathBuf> = st_files("../resources/test");
    paths.extend(st_files("tests/fixtures"));
    let mut files = Vec::new();
    for path in paths {
        let text = std::fs::read_to_string(&path).unwrap_or_default();
        let name = path
            .strip_prefix(manifest)
            .unwrap_or(&path)
            .to_string_lossy()
            .replace('\\', "/");
        files.push((format!("{name} (CRLF)"), text.replace('\n', "\r\n")));
        files.push((format!("{name} (tabs)"), text.replace("    ", "\t")));
        files.push((name, text));
    }
    files
}
