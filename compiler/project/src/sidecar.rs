//! Stable variable UID sidecar persistence (ADR 0053).
//!
//! The sidecar is a deterministic JSON file named `<stem>.uids.json` stored
//! next to the project, mapping each persistent declaration's `(scope, name)`
//! to its engineering-side entity UID. IEC sources and PLCopen XML stay
//! untouched: the sidecar is the only UID store, so a project that loses it
//! degrades to no-migration (safe rejection) rather than silent guessing.
//!
//! The mapping covers the persistent prefix -- program variables and
//! top-level `VAR_GLOBAL` declarations -- the same population codegen records
//! in the container's `stable_vars` table. Codegen matches declarations by
//! name only, so the sidecar's scope exists for disambiguation and for
//! rename/swap tracking, not for the container lookup.
//!
//! ## Format
//!
//! ```json
//! {
//!   "version": 1,
//!   "variables": [
//!     { "scope": "main", "name": "x", "uid": 42 }
//!   ]
//! }
//! ```
//!
//! Entries are sorted by `(scope, name)` and the field order is fixed, so
//! serializations of equal tables are byte-identical and diffs stay clean.
//! There are no timestamps or environment-specific data.
//!
//! ## Malformed files
//!
//! A missing file loads as an empty sidecar. A malformed file (invalid JSON,
//! an unexpected shape, duplicate keys, or the reserved UID 0) recovers the
//! same way, as an empty table: the next [`Sidecar::sync`] then assigns fresh
//! UIDs, which is the safe no-migration degradation ADR 0053 describes.
//! Genuine I/O failures (a file that exists but cannot be read) are
//! diagnostics, not recovery cases.
//!
//! ## UID allocation
//!
//! [`Sidecar::sync`] keeps UIDs of unchanged keys, drops removed keys, and
//! assigns new keys `max + 1` monotonically (0 is reserved and never
//! assigned), so a removed key's UID is never reused and a later re-add is a
//! new entity with initialization semantics. A one-removed/one-added pair is
//! reported as a rename candidate and a two/two pair as a swap candidate;
//! both are heuristics for the user to resolve with
//! [`Sidecar::map_uid`] -- the sidecar never resolves them on its own.

use std::collections::{BTreeMap, BTreeSet};
use std::fmt;
use std::fs;
use std::path::{Path, PathBuf};

use ironplc_dsl::common::{Library, LibraryElementKind, VarDecl, VariableType};
use ironplc_dsl::core::{FileId, Id};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_problems::Problem;
use serde_json::{json, Value};

/// The format version written into the sidecar's `version` field. A loader
/// accepts only this version; anything else is malformed and recovers empty.
const FORMAT_VERSION: u64 = 1;

/// The scope name recorded for top-level `VAR_GLOBAL` declarations, which
/// have no containing program. Mirrors the debug section's global-scope
/// concept.
const GLOBAL_SCOPE: &str = "global";

/// A sidecar key: the declaration's scope path and name.
///
/// Both parts follow IEC 61131-3 identifier semantics (case-insensitive)
/// through [`Id`], so a key matches a declaration regardless of source
/// casing. Ordering is by lowercase scope, then lowercase name, which is the
/// deterministic serialization order.
#[derive(Clone, Debug)]
pub struct SidecarKey {
    scope: Id,
    name: Id,
}

impl SidecarKey {
    /// Creates a key from scope and name strings.
    pub fn new(scope: &str, name: &str) -> Self {
        SidecarKey {
            scope: Id::from(scope),
            name: Id::from(name),
        }
    }

    /// The scope path (the declaring program's name, or `global`).
    pub fn scope(&self) -> &Id {
        &self.scope
    }

    /// The declaration's name.
    pub fn name(&self) -> &Id {
        &self.name
    }
}

impl PartialEq for SidecarKey {
    fn eq(&self, other: &Self) -> bool {
        self.cmp(other) == std::cmp::Ordering::Equal
    }
}

impl Eq for SidecarKey {}

impl PartialOrd for SidecarKey {
    fn partial_cmp(&self, other: &Self) -> Option<std::cmp::Ordering> {
        Some(self.cmp(other))
    }
}

impl Ord for SidecarKey {
    fn cmp(&self, other: &Self) -> std::cmp::Ordering {
        self.scope
            .lower_case()
            .cmp(other.scope.lower_case())
            .then_with(|| self.name.lower_case().cmp(other.name.lower_case()))
    }
}

impl fmt::Display for SidecarKey {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}.{}", self.scope, self.name)
    }
}

/// A rename heuristic reported by [`Sidecar::sync`]: exactly one key
/// disappeared and exactly one appeared.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct RenameCandidate {
    /// The key that disappeared, with its former UID.
    pub old: SidecarKey,
    /// The key that appeared.
    pub new: SidecarKey,
}

/// A swap heuristic reported by [`Sidecar::sync`]: exactly two keys
/// disappeared and exactly two appeared. The pairing by sort position is
/// arbitrary -- the user decides the real pairing through `map-uid`.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct SwapCandidate {
    /// The two keys that disappeared.
    pub removed: (SidecarKey, SidecarKey),
    /// The two keys that appeared, paired with `removed` by sort position.
    pub added: (SidecarKey, SidecarKey),
}

/// The outcome of [`Sidecar::sync`]: which keys kept their UID, which were
/// assigned one, which were dropped, and the rename/swap heuristics.
#[derive(Clone, Debug, Default)]
pub struct SyncReport {
    /// Declared keys that already had a UID, with that UID.
    pub preserved: Vec<(SidecarKey, u64)>,
    /// Declared keys that had no UID, with the assigned UID.
    pub assigned: Vec<(SidecarKey, u64)>,
    /// Former keys no longer declared, with the UID that was dropped.
    pub removed: Vec<(SidecarKey, u64)>,
    /// One-removed/one-added heuristic, empty unless exactly one of each.
    pub rename_candidates: Vec<RenameCandidate>,
    /// Two-removed/two-added heuristic, empty unless exactly two of each.
    pub swap_candidates: Vec<SwapCandidate>,
}

impl fmt::Display for SyncReport {
    /// Renders the report as stable plain text: five sections, each with a
    /// count and one line per entry. The CLI prints this verbatim so the
    /// format is the machine-readable contract for tooling.
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        writeln!(f, "preserved: {}", self.preserved.len())?;
        for (key, uid) in &self.preserved {
            writeln!(f, "  {key} (uid {uid})")?;
        }
        writeln!(f, "assigned: {}", self.assigned.len())?;
        for (key, uid) in &self.assigned {
            writeln!(f, "  {key} (uid {uid})")?;
        }
        writeln!(f, "removed: {}", self.removed.len())?;
        for (key, uid) in &self.removed {
            writeln!(f, "  {key} (uid {uid})")?;
        }
        writeln!(f, "rename candidates: {}", self.rename_candidates.len())?;
        for candidate in &self.rename_candidates {
            writeln!(f, "  {} -> {}", candidate.old, candidate.new)?;
        }
        writeln!(f, "swap candidates: {}", self.swap_candidates.len())?;
        for candidate in &self.swap_candidates {
            writeln!(
                f,
                "  ({}, {}) -> ({}, {})",
                candidate.removed.0, candidate.removed.1, candidate.added.0, candidate.added.1
            )?;
        }
        Ok(())
    }
}

/// Why [`Sidecar::map_uid`] refused to move a UID.
#[derive(Clone, Debug, PartialEq, Eq)]
pub enum MapUidError {
    /// The old key has no recorded UID.
    UnknownKey,
    /// The new key already has a recorded UID.
    KeyAlreadyMapped,
}

impl fmt::Display for MapUidError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            MapUidError::UnknownKey => {
                write!(f, "the old key has no recorded stable variable ID")
            }
            MapUidError::KeyAlreadyMapped => {
                write!(f, "the new key already has a recorded stable variable ID")
            }
        }
    }
}

impl std::error::Error for MapUidError {}

/// The persistent `(scope, name) -> uid` mapping, kept sorted by key so every
/// operation and serialization is deterministic.
#[derive(Clone, Debug, Default)]
pub struct Sidecar {
    entries: BTreeMap<SidecarKey, u64>,
}

impl Sidecar {
    /// Creates an empty sidecar.
    pub fn new() -> Self {
        Sidecar::default()
    }

    /// Whether the sidecar holds no entries.
    pub fn is_empty(&self) -> bool {
        self.entries.is_empty()
    }

    /// The number of entries.
    pub fn len(&self) -> usize {
        self.entries.len()
    }

    /// The entries in deterministic key order.
    pub fn entries(&self) -> impl Iterator<Item = (&SidecarKey, u64)> + '_ {
        self.entries.iter().map(|(key, uid)| (key, *uid))
    }

    /// Loads the sidecar at `path`.
    ///
    /// A missing file loads as an empty sidecar, as does a malformed file
    /// (see the module documentation); only a genuine I/O failure is an
    /// error.
    pub fn load(path: &Path) -> Result<Sidecar, Diagnostic> {
        let text = match fs::read_to_string(path) {
            Ok(text) => text,
            Err(err) if err.kind() == std::io::ErrorKind::NotFound => return Ok(Sidecar::new()),
            Err(err) => return Err(fs_diagnostic(Problem::CannotReadFile, path, err)),
        };
        Ok(parse(&text).unwrap_or_default())
    }

    /// Saves the sidecar to `path`, overwriting any existing file. The
    /// serialization is deterministic (sorted keys, fixed field order), so
    /// repeated saves of an equal table produce identical bytes.
    pub fn save(&self, path: &Path) -> Result<(), Diagnostic> {
        // Serializing a `Value` built only from strings and integers cannot
        // fail; the error arm guards the type system, not a real case.
        let text = serde_json::to_string_pretty(&self.to_json())
            .map_err(|_| Diagnostic::internal_error())?;
        fs::write(path, text).map_err(|err| fs_diagnostic(Problem::CannotWriteFile, path, err))
    }

    /// Reconciles the sidecar with the keys currently declared.
    ///
    /// Declared keys keep their UID; new keys are assigned `max + 1`
    /// monotonically (0 is reserved); removed keys are dropped. A
    /// one-removed/one-added pair is reported as a rename candidate and a
    /// two/two pair as a swap candidate -- heuristics only, never applied.
    pub fn sync(&mut self, declared: &[SidecarKey]) -> SyncReport {
        let declared: BTreeSet<&SidecarKey> = declared.iter().collect();

        let mut preserved = Vec::new();
        let mut removed = Vec::new();
        let mut retained = BTreeMap::new();
        for (key, uid) in &self.entries {
            if declared.contains(key) {
                preserved.push((key.clone(), *uid));
                retained.insert(key.clone(), *uid);
            } else {
                removed.push((key.clone(), *uid));
            }
        }

        // The next UID is max + 1 over the table as loaded, so a removed
        // key's UID is never reused. saturating_add only matters after
        // u64::MAX assignments, which no real project reaches.
        let mut next = self
            .entries
            .values()
            .max()
            .copied()
            .unwrap_or(0)
            .saturating_add(1);
        let mut assigned = Vec::new();
        for key in declared
            .iter()
            .filter(|key| !self.entries.contains_key(*key))
        {
            let uid = next;
            next = next.saturating_add(1);
            retained.insert((*key).clone(), uid);
            assigned.push(((*key).clone(), uid));
        }
        self.entries = retained;

        let rename_candidates = match (removed.as_slice(), assigned.as_slice()) {
            ([(old, _)], [(new, _)]) => vec![RenameCandidate {
                old: old.clone(),
                new: new.clone(),
            }],
            _ => Vec::new(),
        };
        let swap_candidates = match (removed.as_slice(), assigned.as_slice()) {
            ([(old0, _), (old1, _)], [(new0, _), (new1, _)]) => vec![SwapCandidate {
                removed: (old0.clone(), old1.clone()),
                added: (new0.clone(), new1.clone()),
            }],
            _ => Vec::new(),
        };

        SyncReport {
            preserved,
            assigned,
            removed,
            rename_candidates,
            swap_candidates,
        }
    }

    /// Moves the UID recorded for `old` to `new`, recording an explicit
    /// rename or swap resolution. Fails -- leaving the sidecar unchanged --
    /// when `old` has no UID or `new` already has one.
    pub fn map_uid(&mut self, old: &SidecarKey, new: &SidecarKey) -> Result<(), MapUidError> {
        let Some(uid) = self.entries.remove(old) else {
            return Err(MapUidError::UnknownKey);
        };
        if self.entries.contains_key(new) {
            self.entries.insert(old.clone(), uid);
            return Err(MapUidError::KeyAlreadyMapped);
        }
        self.entries.insert(new.clone(), uid);
        Ok(())
    }

    /// The `(name, uid)` table the compile pipeline forwards to codegen
    /// (ADR 0053). Codegen matches declarations by name only, so the scope
    /// does not participate; when two scopes declare the same name, the
    /// first entry in key order wins, matching codegen's first-match lookup.
    pub fn stable_var_ids(&self) -> Vec<(Id, u64)> {
        self.entries
            .iter()
            .map(|(key, uid)| (key.name.clone(), *uid))
            .collect()
    }

    /// Serializes as the deterministic JSON shape (sorted entries).
    fn to_json(&self) -> Value {
        let variables: Vec<Value> = self
            .entries
            .iter()
            .map(|(key, uid)| {
                json!({
                    "scope": key.scope.original(),
                    "name": key.name.original(),
                    "uid": uid,
                })
            })
            .collect();
        json!({
            "version": FORMAT_VERSION,
            "variables": variables,
        })
    }
}

/// Derives the sidecar path `<stem>.uids.json` for a project path argument:
/// the file stem when `project` names a file, the directory name when it
/// names a directory. Both `FileBackedProject::initialize` and the `refactor`
/// commands resolve the sidecar through this function, so compiling a
/// project and syncing its sidecar address the same file. Returns `None`
/// when the path has no usable name (for example a root directory).
pub fn sidecar_path_for(project: &Path) -> Option<PathBuf> {
    let stem = if project.is_dir() {
        project.file_name()?.to_str()?
    } else {
        project.file_stem()?.to_str()?
    };
    Some(project.with_file_name(format!("{stem}.uids.json")))
}

/// Collects the keys the sidecar tracks for a library: the persistent
/// prefix's declared variables -- program variables (scope = program name)
/// and top-level `VAR_GLOBAL` declarations (scope = `global`), ADR 0053.
/// `VAR_EXTERNAL` aliases are skipped, matching codegen, which allocates the
/// global itself. The result is sorted and deduplicated.
pub fn declared_var_keys(library: &Library) -> Vec<SidecarKey> {
    let mut keys = Vec::new();
    for element in &library.elements {
        match element {
            LibraryElementKind::ProgramDeclaration(program) => {
                push_var_keys(&program.name, &program.variables, &mut keys);
            }
            LibraryElementKind::GlobalVarDeclarations(variables) => {
                push_var_keys(&Id::from(GLOBAL_SCOPE), variables, &mut keys);
            }
            _ => {}
        }
    }
    keys.sort();
    keys.dedup();
    keys
}

fn push_var_keys(scope: &Id, variables: &[VarDecl], keys: &mut Vec<SidecarKey>) {
    for variable in variables {
        if variable.var_type == VariableType::External {
            // VAR_EXTERNAL aliases the global declaration; the global is
            // keyed, so the alias must not become a second key.
            continue;
        }
        if let Some(name) = variable.identifier.symbolic_id() {
            keys.push(SidecarKey {
                scope: scope.clone(),
                name: name.clone(),
            });
        }
    }
}

/// Parses sidecar text into a table. Returns `None` for any deviation from
/// the format -- the caller recovers as an empty sidecar (see module docs).
fn parse(text: &str) -> Option<Sidecar> {
    let value: Value = serde_json::from_str(text).ok()?;
    if value.get("version")?.as_u64()? != FORMAT_VERSION {
        return None;
    }
    let variables = value.get("variables")?.as_array()?;
    let mut entries = BTreeMap::new();
    for variable in variables {
        let scope = variable.get("scope")?.as_str()?;
        let name = variable.get("name")?.as_str()?;
        let uid = variable.get("uid")?.as_u64()?;
        // Empty names are not declarations the compiler could produce, and
        // 0 is reserved; either means the file is not a sidecar we wrote.
        if scope.is_empty() || name.is_empty() || uid == 0 {
            return None;
        }
        if entries.insert(SidecarKey::new(scope, name), uid).is_some() {
            return None;
        }
    }
    Some(Sidecar { entries })
}

/// Builds the diagnostic for a sidecar file I/O failure, following the
/// discovery-time diagnostic shape (`path, error`).
fn fs_diagnostic(problem: Problem, path: &Path, err: std::io::Error) -> Diagnostic {
    Diagnostic::problem(
        problem,
        Label::file(
            FileId::from_path(path),
            format!("{}, {}", path.display(), err),
        ),
    )
}

#[cfg(test)]
mod tests {
    use super::*;
    use rstest::rstest;
    use std::path::Path;
    use tempfile::TempDir;

    /// Writes `text` to a fresh temporary sidecar file and loads it.
    fn load_from_text(text: &str) -> Sidecar {
        let dir = TempDir::new().unwrap();
        let path = dir.path().join("test.uids.json");
        std::fs::write(&path, text).unwrap();
        Sidecar::load(&path).unwrap()
    }

    /// Loads a sidecar from JSON with the given entries already recorded.
    fn loaded(entries: &[(&str, &str, u64)]) -> Sidecar {
        let variables: Vec<String> = entries
            .iter()
            .map(|(scope, name, uid)| {
                format!(r#"{{"scope": "{scope}", "name": "{name}", "uid": {uid}}}"#)
            })
            .collect();
        load_from_text(&format!(
            r#"{{"version": 1, "variables": [{}]}}"#,
            variables.join(", ")
        ))
    }

    fn key(scope: &str, name: &str) -> SidecarKey {
        SidecarKey::new(scope, name)
    }

    #[test]
    fn load_when_file_missing_then_empty() {
        let dir = TempDir::new().unwrap();
        let sidecar = Sidecar::load(&dir.path().join("missing.uids.json")).unwrap();

        assert!(sidecar.is_empty());
    }

    #[test]
    fn save_then_load_round_trips_entries() {
        let dir = TempDir::new().unwrap();
        let path = dir.path().join("test.uids.json");
        let sidecar = loaded(&[("main", "x", 42), ("global", "g", 7)]);
        sidecar.save(&path).unwrap();

        let loaded = Sidecar::load(&path).unwrap();

        let entries: Vec<(String, String, u64)> = loaded
            .entries()
            .map(|(k, uid)| (k.scope().to_string(), k.name().to_string(), uid))
            .collect();
        // Key order is sorted: global.g precedes main.x.
        assert_eq!(
            entries,
            vec![
                ("global".to_string(), "g".to_string(), 7),
                ("main".to_string(), "x".to_string(), 42),
            ]
        );
    }

    #[test]
    fn save_when_repeated_then_identical_bytes() {
        let dir = TempDir::new().unwrap();
        let first = dir.path().join("first.uids.json");
        let second = dir.path().join("second.uids.json");
        let sidecar = loaded(&[("main", "b", 2), ("main", "a", 1)]);

        sidecar.save(&first).unwrap();
        sidecar.save(&second).unwrap();

        assert_eq!(
            std::fs::read(&first).unwrap(),
            std::fs::read(&second).unwrap()
        );
    }

    #[rstest]
    #[case::not_json("this is not json")]
    #[case::missing_version(r#"{"variables": []}"#)]
    #[case::wrong_version(r#"{"version": 2, "variables": []}"#)]
    #[case::variables_not_array(r#"{"version": 1, "variables": {}}"#)]
    #[case::entry_missing_uid(r#"{"version": 1, "variables": [{"scope": "main", "name": "x"}]}"#)]
    #[case::reserved_uid_zero(
        r#"{"version": 1, "variables": [{"scope": "main", "name": "x", "uid": 0}]}"#
    )]
    #[case::empty_name(r#"{"version": 1, "variables": [{"scope": "main", "name": "", "uid": 1}]}"#)]
    #[case::duplicate_keys(r#"{"version": 1, "variables": [{"scope": "main", "name": "x", "uid": 1}, {"scope": "main", "name": "x", "uid": 2}]}"#)]
    fn load_when_malformed_then_recovers_empty(#[case] text: &str) {
        let sidecar = load_from_text(text);

        assert!(sidecar.is_empty());
    }

    #[test]
    fn sync_when_all_keys_declared_then_preserved_only() {
        let mut sidecar = loaded(&[("main", "x", 42)]);
        let report = sidecar.sync(&[key("main", "x")]);

        assert_eq!(report.preserved, vec![(key("main", "x"), 42)]);
        assert!(report.assigned.is_empty());
        assert!(report.removed.is_empty());
        assert!(report.rename_candidates.is_empty());
        assert!(report.swap_candidates.is_empty());
        assert_eq!(sidecar.stable_var_ids(), vec![(Id::from("x"), 42)]);
    }

    #[test]
    fn sync_when_new_key_then_assigned_max_plus_one() {
        let mut sidecar = loaded(&[("main", "x", 42)]);
        let report = sidecar.sync(&[key("main", "x"), key("main", "y")]);

        assert_eq!(report.preserved, vec![(key("main", "x"), 42)]);
        assert_eq!(report.assigned, vec![(key("main", "y"), 43)]);
        assert_eq!(sidecar.stable_var_ids().len(), 2);
    }

    #[test]
    fn sync_when_first_keys_then_assigned_starting_at_one() {
        let mut sidecar = Sidecar::new();
        let report = sidecar.sync(&[key("main", "x"), key("main", "y")]);

        assert_eq!(
            report.assigned,
            vec![(key("main", "x"), 1), (key("main", "y"), 2)]
        );
    }

    #[test]
    fn sync_when_key_removed_then_dropped_and_reported() {
        let mut sidecar = loaded(&[("main", "x", 42), ("main", "old", 7)]);
        let report = sidecar.sync(&[key("main", "x")]);

        assert_eq!(report.removed, vec![(key("main", "old"), 7)]);
        assert_eq!(sidecar.stable_var_ids(), vec![(Id::from("x"), 42)]);
    }

    #[test]
    fn sync_when_key_replaced_in_one_sync_then_removed_uid_not_reused() {
        let mut sidecar = loaded(&[("main", "x", 100)]);
        let report = sidecar.sync(&[key("main", "y")]);

        // Allocation runs over the table as loaded, so the removed key's
        // UID is not handed to the replacement in the same sync: the new
        // key is a new entity with initialization semantics.
        assert_eq!(report.removed, vec![(key("main", "x"), 100)]);
        assert_eq!(report.assigned, vec![(key("main", "y"), 101)]);
        assert_eq!(sidecar.stable_var_ids(), vec![(Id::from("y"), 101)]);
    }

    #[test]
    fn sync_when_key_readded_in_later_sync_then_treated_as_new_entity() {
        let mut sidecar = loaded(&[("main", "x", 42)]);
        sidecar.sync(&[]);
        let report = sidecar.sync(&[key("main", "x")]);

        // The removal was already persisted, so the re-added key gets a
        // fresh UID rather than its old one: drop-on-removal is final.
        assert_eq!(report.assigned, vec![(key("main", "x"), 1)]);
        assert!(report.preserved.is_empty());
    }

    #[test]
    fn sync_when_one_removed_one_added_then_rename_candidate() {
        let mut sidecar = loaded(&[("main", "old", 7)]);
        let report = sidecar.sync(&[key("main", "new")]);

        assert_eq!(
            report.rename_candidates,
            vec![RenameCandidate {
                old: key("main", "old"),
                new: key("main", "new"),
            }]
        );
        assert!(report.swap_candidates.is_empty());
        // The candidate is reported but not applied: the new key still has
        // its own freshly assigned UID.
        assert_eq!(report.assigned, vec![(key("main", "new"), 8)]);
    }

    #[test]
    fn sync_when_two_removed_two_added_then_swap_candidate() {
        let mut sidecar = loaded(&[("main", "a", 1), ("main", "b", 2)]);
        let report = sidecar.sync(&[key("main", "c"), key("main", "d")]);

        assert_eq!(
            report.swap_candidates,
            vec![SwapCandidate {
                removed: (key("main", "a"), key("main", "b")),
                added: (key("main", "c"), key("main", "d")),
            }]
        );
        assert!(report.rename_candidates.is_empty());
    }

    #[test]
    fn sync_when_one_removed_two_added_then_no_candidates() {
        let mut sidecar = loaded(&[("main", "a", 1)]);
        let report = sidecar.sync(&[key("main", "b"), key("main", "c")]);

        assert!(report.rename_candidates.is_empty());
        assert!(report.swap_candidates.is_empty());
    }

    #[test]
    fn map_uid_when_old_known_and_new_free_then_moves_uid() {
        let mut sidecar = loaded(&[("main", "old", 7), ("main", "other", 3)]);
        sidecar
            .map_uid(&key("main", "old"), &key("main", "new"))
            .unwrap();

        assert_eq!(
            sidecar.stable_var_ids(),
            vec![(Id::from("new"), 7), (Id::from("other"), 3)]
        );
    }

    #[test]
    fn map_uid_when_old_unknown_then_error_and_unchanged() {
        let mut sidecar = loaded(&[("main", "x", 7)]);
        let result = sidecar.map_uid(&key("main", "missing"), &key("main", "new"));

        assert_eq!(result.unwrap_err(), MapUidError::UnknownKey);
        assert_eq!(sidecar.stable_var_ids(), vec![(Id::from("x"), 7)]);
    }

    #[test]
    fn map_uid_when_new_already_mapped_then_error_and_unchanged() {
        let mut sidecar = loaded(&[("main", "old", 7), ("main", "new", 9)]);
        let result = sidecar.map_uid(&key("main", "old"), &key("main", "new"));

        assert_eq!(result.unwrap_err(), MapUidError::KeyAlreadyMapped);
        assert_eq!(
            sidecar.stable_var_ids(),
            vec![(Id::from("new"), 9), (Id::from("old"), 7)]
        );
    }

    #[test]
    fn stable_var_ids_when_entries_then_names_only() {
        let sidecar = loaded(&[("main", "x", 42), ("global", "g", 7)]);

        assert_eq!(
            sidecar.stable_var_ids(),
            vec![(Id::from("g"), 7), (Id::from("x"), 42)]
        );
    }

    #[rstest]
    #[case::file_with_extension("proj.plcproj", "proj.uids.json")]
    #[case::source_file("main.st", "main.uids.json")]
    fn sidecar_path_for_when_file_then_sibling(#[case] file: &str, #[case] expected: &str) {
        let path = sidecar_path_for(Path::new("some").join("dir").join(file).as_path()).unwrap();

        assert_eq!(path, Path::new("some").join("dir").join(expected));
    }

    #[test]
    fn sidecar_path_for_when_directory_then_directory_name_stem() {
        let path = sidecar_path_for(&Path::new("some").join("MyProject")).unwrap();

        assert_eq!(path, Path::new("some").join("MyProject.uids.json"));
    }

    #[test]
    fn load_when_read_error_then_diagnostic() {
        // A directory at the sidecar path is not NotFound and not readable
        // as a file, so it exercises the genuine I/O failure arm.
        let dir = TempDir::new().unwrap();
        let result = Sidecar::load(dir.path());

        let err = result.err().unwrap();
        assert_eq!(err.code, Problem::CannotReadFile.code());
        assert!(err
            .primary
            .message
            .contains(dir.path().to_string_lossy().as_ref()));
    }

    #[test]
    fn declared_var_keys_when_program_and_globals_then_persistent_prefix() {
        let library = ironplc_sources::parse_source(
            ironplc_sources::FileType::StructuredText,
            "PROGRAM main VAR x : INT; e : INT; END_VAR \
             VAR_EXTERNAL ext : INT; END_VAR END_PROGRAM \
             VAR_GLOBAL g : INT; END_VAR",
            &FileId::from_string("main.st"),
            &ironplc_parser::options::CompilerOptions {
                allow_top_level_var_global: true,
                ..ironplc_parser::options::CompilerOptions::default()
            },
        )
        .unwrap();

        let keys = declared_var_keys(&library);

        // Sorted by scope then name; VAR_EXTERNAL is skipped.
        assert_eq!(
            keys,
            vec![key("global", "g"), key("main", "e"), key("main", "x")]
        );
    }
}
