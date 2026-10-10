//! Workspace-level guard that no crate of the workspace is exempt from a
//! lint the workspace denies.
//!
//! The lints are denied once, in `[workspace.lints.rust]` and
//! `[workspace.lints.clippy]` of `compiler/Cargo.toml`. A crate takes them by
//! inheriting that table (`[lints] workspace = true`), so a new crate is
//! covered without an edit of its own.
//!
//! Cargo cannot inherit a table and override one lint of it, so a crate that
//! needs its own `[lints]` (the build-support and test-support crates, which
//! allow panicking helpers) replaces the workspace table and would silently
//! leave every lint it does not repeat. This guard reads the lints from the
//! workspace tables and the members from `compiler/Cargo.toml`, and fails for
//! a member with its own table that does not name each denied lint, either
//! as a level that keeps it (`deny`, `forbid`) or as an explicit `allow`.
//! A lint added to the workspace table needs no edit of this file.

use std::fs;
use std::path::Path;

use toml::{Table, Value};

/// The tool tables of a `lints` table that carry lint names: `rust` for
/// rustc lints, `clippy` for clippy lints.
const LINT_TOOLS: &[&str] = &["rust", "clippy"];

/// A lint named by its tool and its name.
#[derive(Debug, Clone, PartialEq, Eq)]
struct Lint {
    tool: String,
    name: String,
}

impl Lint {
    fn new(tool: &str, name: &str) -> Self {
        Lint {
            tool: tool.to_string(),
            name: name.to_string(),
        }
    }
}

impl std::fmt::Display for Lint {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}::{}", self.tool, self.name)
    }
}

fn parse(text: &str) -> Table {
    text.parse::<Table>().expect("a manifest is valid TOML")
}

/// The `members` of the `[workspace]` table.
fn members(workspace: &Table) -> Vec<String> {
    workspace["workspace"]["members"]
        .as_array()
        .expect("the workspace lists its members")
        .iter()
        .filter_map(|member| member.as_str().map(str::to_string))
        .collect()
}

/// The level a `lints` table (a `[lints]` or `[workspace.lints]` table) sets
/// for `lint`. A level is either a string or a table with a `level` key.
fn level<'a>(lints: Option<&'a Value>, lint: &Lint) -> Option<&'a str> {
    let setting = lints?.get(&lint.tool)?.get(&lint.name)?;
    setting
        .as_str()
        .or_else(|| setting.get("level").and_then(Value::as_str))
}

/// The lints the `[workspace.lints]` table sets to `deny`, in table order.
fn denied_by_workspace(workspace: &Table) -> Vec<Lint> {
    let lints = workspace["workspace"].get("lints");
    LINT_TOOLS
        .iter()
        .flat_map(|tool| {
            let names = lints
                .and_then(|lints| lints.get(tool))
                .and_then(Value::as_table);
            names
                .into_iter()
                .flat_map(|names| names.keys())
                .map(move |name| Lint::new(tool, name))
        })
        .filter(|lint| level(lints, lint) == Some("deny"))
        .collect()
}

/// True when the manifest takes the workspace lint table.
fn inherits_workspace_lints(manifest: &Table) -> bool {
    manifest
        .get("lints")
        .and_then(|lints| lints.get("workspace"))
        .and_then(Value::as_bool)
        == Some(true)
}

/// True when a member's own table names `lint` with a level that is a
/// decision: it keeps the lint (`deny`, `forbid`) or lifts it on purpose
/// (`allow`). `warn` is neither.
fn names(lints: Option<&Value>, lint: &Lint) -> bool {
    matches!(
        level(lints, lint),
        Some("deny") | Some("forbid") | Some("allow")
    )
}

/// The lints of `required` a manifest leaves out: none when it inherits the
/// workspace table, otherwise those its own table does not name.
fn missing_lints(manifest: &Table, required: &[Lint]) -> Vec<Lint> {
    if inherits_workspace_lints(manifest) {
        return Vec::new();
    }
    let lints = manifest.get("lints");
    required
        .iter()
        .filter(|lint| !names(lints, lint))
        .cloned()
        .collect()
}

/// `crate: lint, lint` for every member that leaves a lint out.
fn members_missing_lints(workspace_dir: &Path, workspace: &Table) -> Vec<String> {
    let required = denied_by_workspace(workspace);
    members(workspace)
        .into_iter()
        .filter_map(|member| {
            let text = fs::read_to_string(workspace_dir.join(&member).join("Cargo.toml"))
                .expect("a member has a manifest");
            let missing = missing_lints(&parse(&text), &required);
            (!missing.is_empty()).then(|| {
                let names: Vec<String> = missing.iter().map(Lint::to_string).collect();
                format!("{member}: {}", names.join(", "))
            })
        })
        .collect()
}

fn workspace_dir() -> &'static Path {
    Path::new(env!("CARGO_MANIFEST_DIR"))
        .parent()
        .expect("the test crate sits in the workspace")
}

#[test]
fn workspace_lints_when_checked_then_deny_at_least_one_lint() {
    let text = fs::read_to_string(workspace_dir().join("Cargo.toml")).unwrap();

    // A guard that reads an empty list would pass for every member.
    assert!(
        !denied_by_workspace(&parse(&text)).is_empty(),
        "[workspace.lints] denies no lint, so the guard would check nothing"
    );
}

#[test]
fn members_when_checked_then_each_inherits_or_names_every_denied_lint() {
    let text = fs::read_to_string(workspace_dir().join("Cargo.toml")).unwrap();

    assert_eq!(
        members_missing_lints(workspace_dir(), &parse(&text)),
        Vec::<String>::new(),
        "a member must set `[lints] workspace = true`, or name each lint that \
         [workspace.lints] denies in its own [lints] (as deny, or as an explicit allow)"
    );
}

#[test]
fn denied_by_workspace_when_tables_set_levels_then_lists_only_deny_in_both_tools() {
    let workspace = parse(
        "[workspace.lints.rust]\nunsafe_code = \"deny\"\nlinker_messages = \"allow\"\n\
         [workspace.lints.clippy]\npanic = \"deny\"\nunwrap_used = { level = \"deny\", priority = 1 }\n\
         todo = \"warn\"\n",
    );

    assert_eq!(
        denied_by_workspace(&workspace),
        [
            Lint::new("rust", "unsafe_code"),
            Lint::new("clippy", "panic"),
            Lint::new("clippy", "unwrap_used"),
        ]
    );
}

#[test]
fn missing_lints_when_member_inherits_then_none() {
    let manifest = parse("[lints]\nworkspace = true\n");

    assert!(missing_lints(&manifest, &[Lint::new("clippy", "panic")]).is_empty());
}

#[test]
fn missing_lints_when_member_has_own_table_without_them_then_all() {
    let manifest = parse("[lints.clippy]\nunwrap_used = \"allow\"\n");
    let required = [Lint::new("clippy", "panic"), Lint::new("rust", "warnings")];

    assert_eq!(missing_lints(&manifest, &required), required);
}

#[test]
fn missing_lints_when_member_has_no_lints_then_all() {
    let manifest = parse("[package]\nname = \"x\"\n");

    assert_eq!(
        missing_lints(&manifest, &[Lint::new("clippy", "panic")]).len(),
        1
    );
}

#[test]
fn missing_lints_when_member_names_one_then_the_other() {
    let manifest = parse("[lints.clippy]\npanic = \"allow\"\n");
    let required = [Lint::new("clippy", "panic"), Lint::new("clippy", "todo")];

    assert_eq!(
        missing_lints(&manifest, &required),
        [Lint::new("clippy", "todo")]
    );
}

#[test]
fn missing_lints_when_member_only_warns_then_missing() {
    let manifest = parse("[lints.clippy]\npanic = \"warn\"\n");

    assert_eq!(
        missing_lints(&manifest, &[Lint::new("clippy", "panic")]).len(),
        1
    );
}

#[test]
fn missing_lints_when_same_name_under_other_tool_then_missing() {
    let manifest = parse("[lints.rust]\npanic = \"deny\"\n");

    assert_eq!(
        missing_lints(&manifest, &[Lint::new("clippy", "panic")]).len(),
        1
    );
}

#[test]
fn missing_lints_when_member_names_with_level_table_then_none() {
    let manifest = parse("[lints.clippy]\npanic = { level = \"deny\", priority = 1 }\n");

    assert!(missing_lints(&manifest, &[Lint::new("clippy", "panic")]).is_empty());
}
