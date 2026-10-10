//! Workspace-level guard for the module-size rule: a Rust source file has at
//! most 1000 lines.
//!
//! The rule is stated in `CLAUDE.md` and `specs/steering/compiler-standards.md`.
//! This guard reads every `*.rs` file of every member of the workspace (the
//! members come from `compiler/Cargo.toml`, so a new crate or a new file is
//! covered without an edit; `target/`, where generated output lives, is not
//! read) and fails when
//!
//! - a file has more than the limit and is not in the allowlist,
//! - a listed file has more lines than the number recorded for it, or
//! - a listed file is missing or is within the limit (a stale entry).
//!
//! The allowlist is `module_size_allowlist.txt` beside this file: one line per
//! file, `<path from compiler/> <recorded line count>`. It records the files
//! that exceeded the limit when the guard was introduced. It only shrinks: the
//! way out of a failure is to split the module, never to raise a number.

use std::collections::BTreeMap;
use std::fs;
use std::path::{Path, PathBuf};

use toml::Table;

/// The most lines a module may have.
const LIMIT: usize = 1000;

/// Lines per file, keyed by the path from the workspace directory with `/`
/// separators.
type LineCounts = BTreeMap<String, usize>;

/// The `members` of the `[workspace]` table.
fn members(workspace_manifest: &str) -> Vec<String> {
    let manifest = workspace_manifest
        .parse::<Table>()
        .expect("the workspace manifest is valid TOML");
    manifest["workspace"]["members"]
        .as_array()
        .expect("the workspace lists its members")
        .iter()
        .filter_map(|member| member.as_str().map(str::to_string))
        .collect()
}

/// Adds the line count of every `*.rs` file under `dir` to `counts`, skipping
/// `target` directories.
fn count_directory(workspace_dir: &Path, dir: &Path, counts: &mut LineCounts) {
    let entries = fs::read_dir(dir).expect("a workspace member directory can be read");
    for entry in entries {
        let path = entry.expect("a directory entry can be read").path();
        if path.is_dir() {
            if path.file_name().is_some_and(|name| name == "target") {
                continue;
            }
            count_directory(workspace_dir, &path, counts);
        } else if path.extension().is_some_and(|extension| extension == "rs") {
            let text = fs::read_to_string(&path).expect("a Rust source file is UTF-8 text");
            let relative = path
                .strip_prefix(workspace_dir)
                .expect("a member lies inside the workspace");
            let key: Vec<String> = relative
                .components()
                .map(|part| part.as_os_str().to_string_lossy().into_owned())
                .collect();
            counts.insert(key.join("/"), text.lines().count());
        }
    }
}

/// The line count of every Rust source file of the members.
fn line_counts(workspace_dir: &Path, members: &[String]) -> LineCounts {
    let mut counts = LineCounts::new();
    for member in members {
        count_directory(workspace_dir, &workspace_dir.join(member), &mut counts);
    }
    counts
}

/// The allowlist: `<path> <count>` per line; blank lines and `#` comments are
/// ignored.
fn parse_allowlist(text: &str) -> LineCounts {
    let mut listed = LineCounts::new();
    for line in text.lines() {
        let line = line.trim();
        if line.is_empty() || line.starts_with('#') {
            continue;
        }
        let (path, count) = line
            .rsplit_once(char::is_whitespace)
            .unwrap_or_else(|| panic!("allowlist line `{line}` is not `<path> <count>`"));
        let count: usize = count
            .trim()
            .parse()
            .unwrap_or_else(|_| panic!("allowlist line `{line}` has no line count"));
        let previous = listed.insert(path.trim().to_string(), count);
        assert!(previous.is_none(), "allowlist lists `{path}` twice");
    }
    listed
}

/// One message per file that breaks the rule.
fn violations(counts: &LineCounts, allowlist: &LineCounts) -> Vec<String> {
    let mut messages = Vec::new();
    for (path, &count) in counts {
        match allowlist.get(path) {
            None if count > LIMIT => messages.push(format!(
                "{path} has {count} lines, the limit is {LIMIT} and the file is not in the \
                 allowlist: split the module (move code into a module of its own)"
            )),
            Some(&recorded) if count > recorded => messages.push(format!(
                "{path} has {count} lines, more than the {recorded} recorded and over the \
                 limit of {LIMIT}: split the module (move code out); do not raise the number"
            )),
            _ => {}
        }
    }
    for (path, &recorded) in allowlist {
        match counts.get(path) {
            None => messages.push(format!(
                "allowlist entry {path} ({recorded}) names a file that does not exist: \
                 remove the entry"
            )),
            Some(&count) if count <= LIMIT => messages.push(format!(
                "allowlist entry {path} ({recorded}) names a file of {count} lines, within the \
                 limit of {LIMIT}: remove the entry"
            )),
            Some(_) => {}
        }
    }
    messages
}

/// Checks the members of the workspace in `workspace_dir` against `allowlist`.
fn check(workspace_dir: &Path, members: &[String], allowlist: &str) -> Vec<String> {
    violations(
        &line_counts(workspace_dir, members),
        &parse_allowlist(allowlist),
    )
}

fn workspace_dir() -> &'static Path {
    Path::new(env!("CARGO_MANIFEST_DIR"))
        .parent()
        .expect("the test crate sits in the workspace")
}

#[test]
fn workspace_when_checked_then_no_module_over_the_limit_outside_the_allowlist() {
    let manifest = fs::read_to_string(workspace_dir().join("Cargo.toml")).unwrap();
    let allowlist = fs::read_to_string(
        Path::new(env!("CARGO_MANIFEST_DIR"))
            .join("tests")
            .join("module_size_allowlist.txt"),
    )
    .unwrap();

    assert_eq!(
        check(workspace_dir(), &members(&manifest), &allowlist),
        Vec::<String>::new(),
        "module-size rule: at most {LIMIT} lines per module"
    );
}

/// Removes `dir` and what it holds. A directory that is already gone is the
/// state wanted, and a leftover in the temporary directory does not fail a test.
fn remove_directory(dir: &Path) {
    #[expect(
        clippy::let_underscore_must_use,
        reason = "an absent directory is the state wanted, and a leftover temporary directory is harmless"
    )]
    let _ = fs::remove_dir_all(dir);
}

/// A directory that holds one workspace member, removed when dropped.
struct Fixture {
    root: PathBuf,
}

impl Fixture {
    fn new(name: &str) -> Self {
        let root =
            std::env::temp_dir().join(format!("ironplc-module-size-{name}-{}", std::process::id()));
        remove_directory(&root);
        fs::create_dir_all(root.join("m").join("src")).unwrap();
        Fixture { root }
    }

    /// Writes `m/<path>` with `lines` lines.
    fn file(&self, path: &str, lines: usize) -> &Self {
        let full = self.root.join("m").join(path);
        fs::create_dir_all(full.parent().unwrap()).unwrap();
        fs::write(full, "// line\n".repeat(lines)).unwrap();
        self
    }

    fn check(&self, allowlist: &str) -> Vec<String> {
        check(&self.root, &["m".to_string()], allowlist)
    }
}

impl Drop for Fixture {
    fn drop(&mut self) {
        remove_directory(&self.root);
    }
}

#[test]
fn check_when_file_within_limit_then_no_violation() {
    let fixture = Fixture::new("within");
    fixture.file("src/a.rs", LIMIT);

    assert!(fixture.check("").is_empty());
}

#[test]
fn check_when_file_over_limit_and_not_listed_then_names_file_count_and_limit() {
    let fixture = Fixture::new("unlisted");
    fixture.file("src/big.rs", LIMIT + 1);

    let found = fixture.check("");

    assert_eq!(found.len(), 1);
    assert!(found[0].contains("m/src/big.rs"), "{}", found[0]);
    assert!(found[0].contains("1001 lines"), "{}", found[0]);
    assert!(found[0].contains("limit is 1000"), "{}", found[0]);
    assert!(found[0].contains("split the module"), "{}", found[0]);
}

#[test]
fn check_when_listed_file_stays_at_recorded_count_then_no_violation() {
    let fixture = Fixture::new("at-record");
    fixture.file("src/big.rs", 1200);

    assert!(fixture.check("m/src/big.rs 1200\n").is_empty());
}

#[test]
fn check_when_listed_file_shrinks_but_stays_over_limit_then_no_violation() {
    let fixture = Fixture::new("shrunk");
    fixture.file("src/big.rs", 1100);

    assert!(fixture.check("m/src/big.rs 1200\n").is_empty());
}

#[test]
fn check_when_listed_file_grows_past_record_then_names_file_and_says_split() {
    let fixture = Fixture::new("grown");
    fixture.file("src/big.rs", 1201);

    let found = fixture.check("m/src/big.rs 1200\n");

    assert_eq!(found.len(), 1);
    assert!(found[0].contains("m/src/big.rs"), "{}", found[0]);
    assert!(found[0].contains("1201 lines"), "{}", found[0]);
    assert!(found[0].contains("1200 recorded"), "{}", found[0]);
    assert!(found[0].contains("split the module"), "{}", found[0]);
}

#[test]
fn check_when_listed_file_is_missing_then_stale_entry() {
    let fixture = Fixture::new("missing");

    let found = fixture.check("m/src/gone.rs 1200\n");

    assert_eq!(found.len(), 1);
    assert!(found[0].contains("m/src/gone.rs"), "{}", found[0]);
    assert!(found[0].contains("does not exist"), "{}", found[0]);
}

#[test]
fn check_when_listed_file_is_within_limit_then_stale_entry() {
    let fixture = Fixture::new("stale");
    fixture.file("src/small.rs", LIMIT);

    let found = fixture.check("m/src/small.rs 1200\n");

    assert_eq!(found.len(), 1);
    assert!(found[0].contains("m/src/small.rs"), "{}", found[0]);
    assert!(found[0].contains("within the limit"), "{}", found[0]);
}

#[test]
fn check_when_file_under_target_then_not_read() {
    let fixture = Fixture::new("target");
    fixture.file("target/debug/generated.rs", LIMIT + 500);

    assert!(fixture.check("").is_empty());
}

#[test]
fn check_when_file_in_nested_directory_then_read() {
    let fixture = Fixture::new("nested");
    fixture.file("tests/it/deep/big.rs", LIMIT + 1);

    let found = fixture.check("");

    assert_eq!(found.len(), 1);
    assert!(found[0].contains("m/tests/it/deep/big.rs"), "{}", found[0]);
}

#[test]
fn check_when_file_is_not_rust_then_not_counted() {
    let fixture = Fixture::new("not-rust");
    fixture.file("src/notes.txt", LIMIT + 1);

    assert!(fixture.check("").is_empty());
}

#[test]
fn parse_allowlist_when_comments_and_blank_lines_then_ignored() {
    let listed = parse_allowlist("# why\n\nm/src/a.rs 1200\n  m/src/b.rs   1300  \n");

    assert_eq!(listed.len(), 2);
    assert_eq!(listed["m/src/a.rs"], 1200);
    assert_eq!(listed["m/src/b.rs"], 1300);
}

#[test]
fn members_when_manifest_lists_them_then_returned_in_order() {
    let found = members("[workspace]\nmembers = [\"a\", \"b\"]\n");

    assert_eq!(found, ["a", "b"]);
}
