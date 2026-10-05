//! The shared parse corpus: every `*.st` file under `compiler/resources/test/`.
//!
//! The parse benchmarks (`benches/parse_benchmark.rs` and
//! `benches/parse_baseline.rs`) and the corpus test iterate this one
//! enumeration so they always measure the same files. The statement bodies of
//! the files' program organization units are the second input set
//! ([`statement_bodies`]): what a PLCopen XML document hands to
//! `parse_st_statements`. Files are parsed under
//! the default dialect options (`CompilerOptions::default()`), the same
//! options the parser's own corpus tests use; files that need `--allow-*`
//! flags or are intentionally malformed therefore measure the fail-fast path.

use std::fs;
use std::path::{Path, PathBuf};

/// One corpus file, read into memory.
#[derive(Debug, Clone)]
pub struct CorpusFile {
    /// File name relative to the corpus directory, e.g. `if.st`.
    pub name: String,
    pub path: PathBuf,
    pub source: String,
}

/// The corpus directory, resolved from this crate's manifest location.
pub fn corpus_dir() -> PathBuf {
    Path::new(env!("CARGO_MANIFEST_DIR")).join("../resources/test")
}

/// Reads every `*.st` file in `dir`, sorted by name.
pub fn load_corpus(dir: &Path) -> Result<Vec<CorpusFile>, String> {
    let entries = fs::read_dir(dir)
        .map_err(|error| format!("cannot read corpus directory {}: {error}", dir.display()))?;
    let mut files = Vec::new();
    for entry in entries {
        let path = entry
            .map_err(|error| format!("cannot read directory entry: {error}"))?
            .path();
        if path.extension().is_none_or(|extension| extension != "st") {
            continue;
        }
        let name = path
            .file_name()
            .and_then(|name| name.to_str())
            .ok_or_else(|| format!("non-UTF-8 file name in {}", dir.display()))?
            .to_string();
        let source = fs::read_to_string(&path)
            .map_err(|error| format!("cannot read {}: {error}", path.display()))?;
        files.push(CorpusFile { name, path, source });
    }
    files.sort_by(|a, b| a.name.cmp(&b.name));
    Ok(files)
}

/// The words that end a program organization unit. The statement body of a unit
/// is the text between its last `END_VAR` and one of these.
const UNIT_ENDS: [&str; 5] = [
    "END_PROGRAM",
    "END_FUNCTION_BLOCK",
    "END_FUNCTION",
    "END_METHOD",
    "END_ACTION",
];

/// The statement body of every unit of every file: the text between the last
/// `END_VAR` of a unit and its `END_...` word, trimmed, as one entry named
/// `<file>#<n>`. This is the input `parse_st_statements` gets from a PLCopen XML
/// document, whose ST body is exactly that text. A unit with no variable block
/// has no entry. The scan reads words, not the grammar, so a keyword inside a
/// comment can mislead it: the set is a measurement input, and a body that does
/// not parse is measured on the path that rejects it.
pub fn statement_bodies(files: &[CorpusFile]) -> Vec<CorpusFile> {
    let mut bodies = Vec::new();
    for file in files {
        for (index, body) in bodies_of(&file.source).into_iter().enumerate() {
            bodies.push(CorpusFile {
                name: format!("{}#{}", file.name, index + 1),
                path: file.path.clone(),
                source: body.to_string(),
            });
        }
    }
    bodies
}

fn bodies_of(source: &str) -> Vec<&str> {
    let mut bodies = Vec::new();
    let mut body_start: Option<usize> = None;
    let mut word_start: Option<usize> = None;
    // A sentinel after the last character ends a word that touches the end.
    for (offset, ch) in source
        .char_indices()
        .chain(std::iter::once((source.len(), ' ')))
    {
        if ch.is_ascii_alphanumeric() || ch == '_' {
            word_start.get_or_insert(offset);
            continue;
        }
        let Some(start) = word_start.take() else {
            continue;
        };
        let word = &source[start..offset];
        if word.eq_ignore_ascii_case("END_VAR") {
            body_start = Some(offset);
        } else if UNIT_ENDS.iter().any(|end| word.eq_ignore_ascii_case(end)) {
            if let Some(from) = body_start.take() {
                let body = source[from..start].trim();
                if !body.is_empty() {
                    bodies.push(body);
                }
            }
        }
    }
    bodies
}
