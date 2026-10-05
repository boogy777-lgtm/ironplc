//! The parse corpus must be non-empty and readable, otherwise the parse
//! benchmarks would silently measure nothing.

use ironplc_benchmarks::corpus::{corpus_dir, load_corpus, statement_bodies, CorpusFile};
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

fn unit(source: &str) -> Vec<CorpusFile> {
    vec![CorpusFile {
        name: "unit.st".to_string(),
        path: Path::new("unit.st").to_path_buf(),
        source: source.to_string(),
    }]
}

#[test]
fn statement_bodies_when_unit_has_variable_block_then_the_text_after_it_is_the_body() {
    let bodies = statement_bodies(&unit(
        "PROGRAM main VAR x : INT; END_VAR
  x := 1;
  x := 2;
END_PROGRAM",
    ));

    assert_eq!(bodies.len(), 1);
    assert_eq!(bodies[0].name, "unit.st#1");
    assert_eq!(
        bodies[0].source,
        "x := 1;
  x := 2;"
    );
}

#[test]
fn statement_bodies_when_several_units_then_one_body_each_and_function_block_is_not_function() {
    let bodies = statement_bodies(&unit(
        "FUNCTION_BLOCK a VAR END_VAR x := 1; END_FUNCTION_BLOCK
function f : INT var end_var f := 2; end_function",
    ));

    let texts: Vec<&str> = bodies.iter().map(|b| b.source.as_str()).collect();
    assert_eq!(texts, vec!["x := 1;", "f := 2;"]);
}

#[test]
fn statement_bodies_when_no_variable_block_or_empty_body_then_no_entry() {
    assert!(statement_bodies(&unit("PROGRAM main x := 1; END_PROGRAM")).is_empty());
    assert!(statement_bodies(&unit("PROGRAM main VAR END_VAR END_PROGRAM")).is_empty());
}
