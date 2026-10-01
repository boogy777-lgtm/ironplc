//! The parse corpus must be non-empty and readable, otherwise the parse
//! benchmarks would silently measure nothing.

use ironplc_benchmarks::corpus::{corpus_dir, load_corpus};
use std::path::Path;

#[test]
fn load_corpus_when_resources_dir_then_non_empty_sorted_st_files() {
    let files = load_corpus(&corpus_dir()).unwrap();

    assert!(!files.is_empty());
    assert!(files.iter().all(|file| file.path.is_file()));
    assert!(files.iter().all(|file| file.name.ends_with(".st")));
    assert!(files.windows(2).all(|pair| pair[0].name < pair[1].name));
}

#[test]
fn load_corpus_when_missing_directory_then_error() {
    assert!(load_corpus(Path::new("does/not/exist")).is_err());
}
