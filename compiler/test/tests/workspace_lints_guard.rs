//! Workspace-level guard that no crate of the workspace is exempt from the
//! rule that a result must not depend on the order a hash container yields
//! its entries.
//!
//! The rule is two clippy lints, configured in `compiler/clippy.toml` and
//! denied once, in `[workspace.lints.clippy]` of `compiler/Cargo.toml`. A
//! crate takes the rule by inheriting that table (`[lints] workspace = true`),
//! so a new crate is covered without an edit of its own.
//!
//! Cargo cannot inherit a table and override one lint of it, so a crate that
//! needs its own `[lints.clippy]` (the build-support and test-support crates,
//! which allow panicking helpers) would silently leave the rule. This guard
//! reads the members from `compiler/Cargo.toml` and fails for a member that
//! neither inherits the workspace table nor denies each of the lints itself.

use std::fs;
use std::path::Path;

use toml::{Table, Value};

/// The lints that carry the rule; `clippy.toml` configures the first.
const HASH_ORDER_LINTS: &[&str] = &["disallowed_methods", "iter_over_hash_type"];

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

/// True when `lints` (a `[lints]` or `[workspace.lints]` table) denies `lint`
/// under `clippy`.
fn denies(lints: Option<&Value>, lint: &str) -> bool {
    lints
        .and_then(|lints| lints.get("clippy"))
        .and_then(|clippy| clippy.get(lint))
        .and_then(Value::as_str)
        == Some("deny")
}

/// The lints of `HASH_ORDER_LINTS` the workspace table does not deny.
fn undenied_by_workspace(workspace: &Table) -> Vec<String> {
    let lints = workspace["workspace"].get("lints");
    HASH_ORDER_LINTS
        .iter()
        .filter(|lint| !denies(lints, lint))
        .map(|lint| lint.to_string())
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

/// The lints of `HASH_ORDER_LINTS` a manifest leaves out: none when it
/// inherits the workspace table, otherwise those it does not deny itself.
fn missing_hash_order_lints(manifest: &Table) -> Vec<String> {
    if inherits_workspace_lints(manifest) {
        return Vec::new();
    }
    let lints = manifest.get("lints");
    HASH_ORDER_LINTS
        .iter()
        .filter(|lint| !denies(lints, lint))
        .map(|lint| lint.to_string())
        .collect()
}

/// `crate: lint, lint` for every member that leaves a lint out.
fn members_missing_lints(workspace_dir: &Path, workspace: &Table) -> Vec<String> {
    members(workspace)
        .into_iter()
        .filter_map(|member| {
            let text = fs::read_to_string(workspace_dir.join(&member).join("Cargo.toml"))
                .expect("a member has a manifest");
            let missing = missing_hash_order_lints(&parse(&text));
            (!missing.is_empty()).then(|| format!("{member}: {}", missing.join(", ")))
        })
        .collect()
}

fn workspace_dir() -> &'static Path {
    Path::new(env!("CARGO_MANIFEST_DIR"))
        .parent()
        .expect("the test crate sits in the workspace")
}

#[test]
fn workspace_lints_when_checked_then_deny_every_hash_order_lint() {
    let text = fs::read_to_string(workspace_dir().join("Cargo.toml")).unwrap();

    assert_eq!(
        undenied_by_workspace(&parse(&text)),
        Vec::<String>::new(),
        "[workspace.lints.clippy] must deny these lints"
    );
}

#[test]
fn members_when_checked_then_each_inherits_or_denies_every_hash_order_lint() {
    let text = fs::read_to_string(workspace_dir().join("Cargo.toml")).unwrap();

    assert_eq!(
        members_missing_lints(workspace_dir(), &parse(&text)),
        Vec::<String>::new(),
        "a member must set `[lints] workspace = true`, or deny these lints in its own [lints.clippy]"
    );
}

#[test]
fn missing_hash_order_lints_when_member_inherits_then_none() {
    let manifest = parse("[lints]\nworkspace = true\n");

    assert!(missing_hash_order_lints(&manifest).is_empty());
}

#[test]
fn missing_hash_order_lints_when_member_has_own_table_without_them_then_both() {
    let manifest = parse("[lints.clippy]\nunwrap_used = \"allow\"\n");

    assert_eq!(
        missing_hash_order_lints(&manifest),
        ["disallowed_methods", "iter_over_hash_type"]
    );
}

#[test]
fn missing_hash_order_lints_when_member_has_no_lints_then_both() {
    let manifest = parse("[package]\nname = \"x\"\n");

    assert_eq!(missing_hash_order_lints(&manifest).len(), 2);
}

#[test]
fn missing_hash_order_lints_when_member_denies_one_then_the_other() {
    let manifest = parse("[lints.clippy]\ndisallowed_methods = \"deny\"\n");

    assert_eq!(missing_hash_order_lints(&manifest), ["iter_over_hash_type"]);
}

#[test]
fn missing_hash_order_lints_when_member_only_warns_then_both() {
    let manifest =
        parse("[lints.clippy]\ndisallowed_methods = \"warn\"\niter_over_hash_type = \"warn\"\n");

    assert_eq!(missing_hash_order_lints(&manifest).len(), 2);
}

#[test]
fn undenied_by_workspace_when_table_allows_one_then_names_it() {
    let workspace = parse(
        "[workspace.lints.clippy]\ndisallowed_methods = \"allow\"\niter_over_hash_type = \"deny\"\n",
    );

    assert_eq!(undenied_by_workspace(&workspace), ["disallowed_methods"]);
}
