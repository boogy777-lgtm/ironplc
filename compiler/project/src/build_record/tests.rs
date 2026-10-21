//! Tests for the build record: that each input changes the fingerprint, and the
//! record's reading, writing and comparison.

use std::fs;
use std::path::{Path, PathBuf};

use ironplc_container::FileHeader;
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::WarningList;
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::WarningSet;
use ironplc_sources::LibraryName;
use serde_json::{json, Value};
use tempfile::TempDir;

use super::*;
use crate::records::{self, RecordKind};
use crate::sidecar::SidecarKey;

const MAIN: &str = "PROGRAM main VAR x : INT; END_VAR x := 1; END_PROGRAM";
const OTHER: &str = "PROGRAM other VAR y : INT; END_VAR y := 2; END_PROGRAM";

/// Writes each `(name, text)` into `dir` and returns the project that holds them,
/// pushed in the order given.
fn project_of(dir: &Path, files: &[(&str, &str)]) -> FileBackedProject {
    let mut project = FileBackedProject::new();
    for (name, text) in files {
        let path = dir.join(name);
        fs::write(&path, text).unwrap();
        project.push(FileId::from_path(&path)).unwrap();
    }
    project
}

fn inputs_of(project: &FileBackedProject) -> BuildInputs {
    BuildInputs::of(project, &CompilerOptions::default(), "plant", "1.0.0")
}

fn baseline(dir: &Path) -> BuildInputs {
    inputs_of(&project_of(dir, &[("main.st", MAIN)]))
}

#[test]
fn of_when_same_inputs_then_equal() {
    let dir = TempDir::new().unwrap();

    let first = baseline(dir.path());
    let second = baseline(dir.path());

    assert_eq!(first, second);
}

#[test]
fn of_when_files_pushed_in_another_order_then_equal() {
    let dir = TempDir::new().unwrap();
    let forward = project_of(dir.path(), &[("a.st", MAIN), ("b.st", OTHER)]);
    let backward = project_of(dir.path(), &[("b.st", OTHER), ("a.st", MAIN)]);

    assert_eq!(inputs_of(&forward), inputs_of(&backward));
}

#[test]
fn of_when_source_text_changes_then_differs() {
    let dir = TempDir::new().unwrap();
    let before = baseline(dir.path());

    let after = inputs_of(&project_of(dir.path(), &[("main.st", OTHER)]));

    assert_ne!(before, after);
}

#[test]
fn of_when_source_added_then_differs() {
    let dir = TempDir::new().unwrap();
    let before = baseline(dir.path());

    let after = inputs_of(&project_of(
        dir.path(),
        &[("main.st", MAIN), ("more.st", OTHER)],
    ));

    assert_ne!(before, after);
}

#[test]
fn of_when_source_moves_then_differs() {
    let dir = TempDir::new().unwrap();
    let before = baseline(dir.path());
    let moved = TempDir::new().unwrap();

    let after = baseline(moved.path());

    assert_ne!(before, after);
}

#[test]
fn of_when_project_name_changes_then_differs() {
    let dir = TempDir::new().unwrap();
    let project = project_of(dir.path(), &[("main.st", MAIN)]);

    let before = BuildInputs::of(&project, &CompilerOptions::default(), "plant", "1.0.0");
    let after = BuildInputs::of(&project, &CompilerOptions::default(), "mill", "1.0.0");

    assert_ne!(before, after);
}

#[test]
fn of_when_compiler_version_changes_then_differs() {
    let dir = TempDir::new().unwrap();
    let project = project_of(dir.path(), &[("main.st", MAIN)]);

    let before = BuildInputs::of(&project, &CompilerOptions::default(), "plant", "1.0.0");
    let after = BuildInputs::of(&project, &CompilerOptions::default(), "plant", "1.0.1");

    assert_ne!(before, after);
}

#[test]
fn of_when_uid_table_changes_then_differs() {
    let dir = TempDir::new().unwrap();
    let mut project = project_of(dir.path(), &[("main.st", MAIN)]);
    let before = inputs_of(&project);

    project.set_stable_var_ids(vec![(SidecarKey::new("main", "x"), 7)]);
    let with_uid = inputs_of(&project);
    project.set_stable_var_ids(vec![(SidecarKey::new("main", "x"), 8)]);
    let with_other_uid = inputs_of(&project);

    assert_ne!(before, with_uid);
    assert_ne!(with_uid, with_other_uid);
}

#[test]
fn of_when_library_activated_then_differs_and_names_it() {
    let dir = TempDir::new().unwrap();
    let mut project = project_of(dir.path(), &[("main.st", MAIN)]);
    let before = inputs_of(&project);

    project.set_activated_libraries(vec![LibraryName::from("Tc2_System")]);
    let after = inputs_of(&project);

    assert_ne!(before, after);
    assert_eq!(after.libraries.len(), 1);
    assert_eq!(after.libraries[0].name, "Tc2_System");
    assert!(after.libraries[0]
        .files
        .as_ref()
        .is_some_and(|files| !files.is_empty()));
}

/// The texts of a library named `Lib` with the one file `path`.
fn lib_texts(path: &str, text: &str) -> Vec<ironplc_sources::project::LibraryTexts> {
    vec![(
        LibraryName::from("Lib"),
        Ok(vec![(path.to_owned(), text.to_owned())]),
    )]
}

#[test]
fn library_inputs_when_text_changes_then_differs() {
    let before = library_inputs(lib_texts("1.0.0/a.st", "FUNCTION F : INT END_FUNCTION"));
    let same = library_inputs(lib_texts("1.0.0/a.st", "FUNCTION F : INT END_FUNCTION"));
    let after = library_inputs(lib_texts("1.0.0/a.st", "FUNCTION G : INT END_FUNCTION"));

    assert_eq!(before, same);
    assert_ne!(before, after);
}

#[test]
fn library_inputs_when_file_renamed_then_differs() {
    assert_ne!(
        library_inputs(lib_texts("1.0.0/a.st", "A")),
        library_inputs(lib_texts("1.0.0/b.st", "A"))
    );
}

#[test]
fn library_inputs_when_library_cannot_be_read_then_no_files() {
    let missing = ironplc_sources::libraries::LibraryRegistry::bundled()
        .texts(&LibraryName::from("DoesNotExist"));

    let inputs = library_inputs(vec![(LibraryName::from("DoesNotExist"), missing)]);

    assert_eq!(inputs[0].files, None);
}

#[test]
fn of_when_warning_list_changes_then_differs() {
    let dir = TempDir::new().unwrap();
    let project = project_of(dir.path(), &[("main.st", MAIN)]);
    let before = BuildInputs::of(&project, &CompilerOptions::default(), "plant", "1.0.0");

    for list in WarningList::ALL {
        let mut options = CompilerOptions::default();
        options
            .messages
            .add(list, WarningSet::from_code("P4083").unwrap());
        let after = BuildInputs::of(&project, &options, "plant", "1.0.0");

        assert_ne!(before, after, "the list {}", list.option_key());
    }
}

#[test]
fn of_when_a_flag_is_set_then_differs_from_the_default() {
    let dir = TempDir::new().unwrap();
    let project = project_of(dir.path(), &[("main.st", MAIN)]);
    let default = BuildInputs::of(&project, &CompilerOptions::default(), "plant", "1.0.0");

    for flag in CompilerOptions::FEATURE_DESCRIPTORS {
        let mut options = CompilerOptions::default();
        assert!(options.set_flag_by_key(flag.option_key, true));
        let changed = BuildInputs::of(&project, &options, "plant", "1.0.0");

        assert_ne!(default, changed, "the flag {}", flag.option_key);
    }
}

#[test]
fn of_when_a_policy_selects_another_alternative_then_differs_from_the_default() {
    let dir = TempDir::new().unwrap();
    let project = project_of(dir.path(), &[("main.st", MAIN)]);
    let default = BuildInputs::of(&project, &CompilerOptions::default(), "plant", "1.0.0");

    for policy in CompilerOptions::POLICY_DESCRIPTORS {
        for alternative in policy
            .alternatives
            .iter()
            .filter(|alternative| **alternative != policy.default)
        {
            let mut options = CompilerOptions::default();
            options
                .set_policy_by_key(policy.option_key, alternative)
                .unwrap();
            let changed = BuildInputs::of(&project, &options, "plant", "1.0.0");

            assert_ne!(
                default, changed,
                "the policy {} set to {alternative}",
                policy.option_key
            );
        }
    }
}

// ---- the container identity -------------------------------------------------

/// The bytes of a container: a valid header with the given content hash, then a
/// body.
fn container_bytes(content_hash: u8, body: &[u8]) -> Vec<u8> {
    let header = FileHeader {
        content_hash: [content_hash; 32],
        layout_hash: [2; 32],
        debug_hash: [3; 32],
        ..FileHeader::default()
    };
    let mut bytes = Vec::new();
    header.write_to(&mut bytes).unwrap();
    bytes.extend_from_slice(body);
    bytes
}

#[test]
fn container_identity_of_bytes_when_valid_header_then_hashes_and_size() {
    let bytes = container_bytes(1, b"body");

    let identity = ContainerIdentity::of_bytes(&bytes).unwrap();

    assert_eq!(identity.content_hash, [1; 32]);
    assert_eq!(identity.layout_hash, [2; 32]);
    assert_eq!(identity.debug_hash, [3; 32]);
    assert_eq!(identity.size, bytes.len() as u64);
}

#[test]
fn container_identity_of_bytes_when_not_a_header_then_none() {
    assert_eq!(ContainerIdentity::of_bytes(b"short"), None);
    assert_eq!(ContainerIdentity::of_bytes(&[0; 300]), None);
}

#[test]
fn container_identity_of_file_when_same_bytes_as_of_bytes_then_equal() {
    let dir = TempDir::new().unwrap();
    let path = dir.path().join("app.iplc");
    let bytes = container_bytes(1, b"body");
    fs::write(&path, &bytes).unwrap();

    assert_eq!(
        ContainerIdentity::of_file(&path),
        ContainerIdentity::of_bytes(&bytes)
    );
    assert_eq!(
        ContainerIdentity::of_file(&dir.path().join("none.iplc")),
        None
    );
}

// ---- the record -------------------------------------------------------------

/// A record for `inputs` that describes the container `bytes`.
fn record_for(inputs: BuildInputs, bytes: &[u8]) -> BuildRecord {
    BuildRecord {
        application: "Main".to_owned(),
        inputs,
        container: ContainerIdentity::of_bytes(bytes).unwrap(),
    }
}

fn record_path(dir: &Path) -> PathBuf {
    RecordKind::Build.path(&dir.join("app.iplc")).unwrap()
}

#[test]
fn write_then_read_when_record_then_round_trips() {
    let dir = TempDir::new().unwrap();
    let options = CompilerOptions {
        allow_adr: true,
        ..CompilerOptions::default()
    };
    let plain = project_of(dir.path(), &[("main.st", MAIN)]);
    let mut with_library = project_of(dir.path(), &[("main.st", MAIN)]);
    with_library.set_activated_libraries(vec![LibraryName::from("Tc2_System")]);

    for project in [&plain, &with_library] {
        let inputs = BuildInputs::of(project, &options, "plant", "1.0.0");
        let record = record_for(inputs, &container_bytes(1, b"body"));

        write(&record_path(dir.path()), &record).unwrap();

        assert_eq!(
            read(&record_path(dir.path())).unwrap(),
            BuildRecordRead::Present(Box::new(record))
        );
    }
}

#[test]
fn write_when_record_then_json_has_the_documented_keys() {
    let dir = TempDir::new().unwrap();
    let record = record_for(baseline(dir.path()), &container_bytes(1, b"body"));

    write(&record_path(dir.path()), &record).unwrap();

    let text = fs::read_to_string(record_path(dir.path())).unwrap();
    let value: Value = serde_json::from_str(&text).unwrap();
    let keys = |v: &Value| -> Vec<String> { v.as_object().unwrap().keys().cloned().collect() };
    assert_eq!(
        keys(&value),
        [
            "application",
            "compilerVersion",
            "container",
            "inputs",
            "version"
        ]
    );
    assert_eq!(
        keys(&value["inputs"]),
        ["libraries", "options", "projectName", "sources", "uids"]
    );
    assert_eq!(
        keys(&value["container"]),
        ["contentHash", "debugHash", "layoutHash", "size"]
    );
    assert_eq!(value["version"], 1);
    assert_eq!(value["compilerVersion"], "1.0.0");
}

#[test]
fn read_when_file_missing_then_missing() {
    let dir = TempDir::new().unwrap();

    assert_eq!(
        read(&record_path(dir.path())).unwrap(),
        BuildRecordRead::Missing
    );
}

#[test]
fn read_when_version_is_good_but_shape_is_not_then_malformed() {
    let dir = TempDir::new().unwrap();
    let record = record_for(baseline(dir.path()), &container_bytes(1, b"body"));
    let good = record.to_json();
    let mut broken: Vec<(&str, Value)> = Vec::new();
    for key in ["application", "compilerVersion", "inputs", "container"] {
        let mut value = good.clone();
        value.as_object_mut().unwrap().remove(key);
        broken.push((key, value));
    }
    let mut bad_hash = good.clone();
    bad_hash["container"]["contentHash"] = json!("not hex");
    broken.push(("bad hash", bad_hash));
    let mut bad_option = good.clone();
    bad_option["inputs"]["options"]["allow_adr"] = json!(true);
    broken.push(("option not text", bad_option));

    for (what, value) in broken {
        records::write(RecordKind::Build, &record_path(dir.path()), value).unwrap();

        assert_eq!(
            read(&record_path(dir.path())).unwrap(),
            BuildRecordRead::Malformed,
            "{what}"
        );
    }
}

#[test]
fn read_when_other_version_then_malformed() {
    let dir = TempDir::new().unwrap();
    fs::write(record_path(dir.path()), r#"{"version": 2}"#).unwrap();

    assert_eq!(
        read(&record_path(dir.path())).unwrap(),
        BuildRecordRead::Malformed
    );
}

// ---- the comparison ---------------------------------------------------------

/// A container file at `dir/app.iplc` and the record that describes it for the
/// baseline inputs.
fn built(dir: &Path) -> (PathBuf, BuildRecord) {
    let path = dir.join("app.iplc");
    let bytes = container_bytes(1, b"body");
    fs::write(&path, &bytes).unwrap();
    (path, record_for(baseline(dir), &bytes))
}

#[test]
fn is_up_to_date_when_inputs_and_container_match_then_true() {
    let dir = TempDir::new().unwrap();
    let (container, record) = built(dir.path());

    assert!(record.is_up_to_date(&baseline(dir.path()), &container));
}

#[test]
fn is_up_to_date_when_an_input_differs_then_false() {
    let dir = TempDir::new().unwrap();
    let (container, record) = built(dir.path());
    let project = project_of(dir.path(), &[("main.st", OTHER)]);

    assert!(!record.is_up_to_date(&inputs_of(&project), &container));
}

#[test]
fn is_up_to_date_when_container_missing_then_false() {
    let dir = TempDir::new().unwrap();
    let (container, record) = built(dir.path());
    fs::remove_file(&container).unwrap();

    assert!(!record.is_up_to_date(&baseline(dir.path()), &container));
}

#[test]
fn is_up_to_date_when_container_has_another_content_hash_then_false() {
    let dir = TempDir::new().unwrap();
    let (container, record) = built(dir.path());
    fs::write(&container, container_bytes(9, b"body")).unwrap();

    assert!(!record.is_up_to_date(&baseline(dir.path()), &container));
}

#[test]
fn is_up_to_date_when_container_has_another_size_then_false() {
    let dir = TempDir::new().unwrap();
    let (container, record) = built(dir.path());
    fs::write(&container, container_bytes(1, b"a longer body")).unwrap();

    assert!(!record.is_up_to_date(&baseline(dir.path()), &container));
}

#[test]
fn is_up_to_date_when_container_is_not_a_container_then_false() {
    let dir = TempDir::new().unwrap();
    let (container, record) = built(dir.path());
    fs::write(&container, b"garbage").unwrap();

    assert!(!record.is_up_to_date(&baseline(dir.path()), &container));
}
