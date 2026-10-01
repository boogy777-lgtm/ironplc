//! The shared parse corpus: every `*.st` file under `compiler/resources/test/`.
//!
//! The parse benchmarks (`benches/parse_benchmark.rs` and
//! `benches/parse_baseline.rs`) and the corpus test iterate this one
//! enumeration so they always measure the same files. Files are parsed under
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
