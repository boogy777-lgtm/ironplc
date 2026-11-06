//! `ironplcc online-compare`, run as the editor runs it: the identity of the
//! controller in, the comparison as JSON out.

// Test-target boundary: the workspace denies `unwrap` in production code; tests
// assert by panicking, so they are exempt here.
#![expect(
    clippy::unwrap_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use predicates::prelude::*;
use serde_json::{json, Value};
use std::fs;
use std::io::Write;
use std::path::{Path, PathBuf};
use std::process::{Command, Stdio};
use tempfile::TempDir;

const DEVICE: &str = "0123456789abcdef0123456789abcdef";
const OTHER_DEVICE: &str = "fedcba9876543210fedcba9876543210";

const PROGRAM: &str = "PROGRAM main
VAR y : INT; END_VAR
  y := 1;
END_PROGRAM
";

const EDITED_PROGRAM: &str = "PROGRAM main
VAR y : INT; END_VAR
  y := 2;
END_PROGRAM
";

const BROKEN_PROGRAM: &str = "PROGRAM main
VAR y : INT; END_VAR
  y := ;
END_PROGRAM
";

/// A project: a source and the application it builds to.
struct Project {
    dir: TempDir,
}

impl Project {
    fn new() -> Project {
        let project = Project {
            dir: TempDir::new().unwrap(),
        };
        project.write_source(PROGRAM);
        project
    }

    fn source(&self) -> PathBuf {
        self.dir.path().join("main.st")
    }

    fn application(&self) -> PathBuf {
        self.dir.path().join("app.iplc")
    }

    fn write_source(&self, text: &str) {
        fs::write(self.source(), text).unwrap();
    }

    fn compile(&self) {
        Command::new(cargo::cargo_bin!("ironplcc"))
            .arg("compile")
            .arg(self.source())
            .arg("-o")
            .arg(self.application())
            .assert()
            .success();
    }

    /// The identity, hashes and layout hash, of the last build.
    fn built(&self) -> (String, String) {
        let record: Value = serde_json::from_str(
            &fs::read_to_string(self.dir.path().join("app.build.iplcinfo")).unwrap(),
        )
        .unwrap();
        (
            record["container"]["contentHash"]
                .as_str()
                .unwrap()
                .to_owned(),
            record["container"]["layoutHash"]
                .as_str()
                .unwrap()
                .to_owned(),
        )
    }

    fn record_download(&self, device: &str) {
        Command::new(cargo::cargo_bin!("ironplcc"))
            .arg("record-download")
            .arg(self.application())
            .args(["--device-id", device])
            .assert()
            .success();
    }

    fn record_path(&self, device: &str) {
        Command::new(cargo::cargo_bin!("ironplcc"))
            .arg("record-path")
            .arg(self.application())
            .args([
                "--device-id",
                device,
                "--address",
                "10.0.0.5",
                "--port",
                "49152",
            ])
            .assert()
            .success();
    }

    /// `online-compare` for this project.
    fn online_compare(&self) -> Command {
        let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));
        cmd.arg("online-compare")
            .arg(self.source())
            .arg("-o")
            .arg(self.application());
        cmd
    }

    /// Writes the identity file of a controller that runs `content`.
    fn identity_file(&self, device: &str, content: &str, layout: &str) -> PathBuf {
        let path = self.dir.path().join("identity.json");
        fs::write(&path, identity(device, content, layout).to_string()).unwrap();
        path
    }
}

fn identity(device: &str, content: &str, layout: &str) -> Value {
    json!({
        "protocol": 2,
        "device": {"deviceId": device, "name": "ironplcvm", "model": "IronPLC SoftPLC",
                   "modification": "vm-cli", "runtimeVersion": "0.247.0"},
        "application": {"mode": "normal", "active": 4, "normal": 4, "candidate": null,
                        "application": 4, "migration": false, "rounds": 99,
                        "contentHash": content, "layoutHash": layout},
        "runState": "running",
        "containerFormat": 9
    })
}

fn output_of(mut cmd: Command) -> Value {
    let output = cmd.assert().success().get_output().stdout.clone();
    serde_json::from_slice(&output).unwrap()
}

fn kinds(comparison: &Value) -> Vec<String> {
    comparison["information"]
        .as_array()
        .unwrap()
        .iter()
        .map(|row| row["kind"].as_str().unwrap().to_owned())
        .collect()
}

fn action<'a>(comparison: &'a Value, name: &str) -> &'a Value {
    comparison["actions"]
        .as_array()
        .unwrap()
        .iter()
        .find(|action| action["action"] == name)
        .unwrap()
}

fn with_identity(project: &Project, file: &Path) -> Command {
    let mut cmd = project.online_compare();
    cmd.arg("--identity").arg(file);
    cmd
}

/// A hash that is nobody's.
fn other_hash() -> String {
    "ee".repeat(32)
}

#[test]
fn online_compare_when_controller_runs_the_last_build_then_up_to_date_and_online_change_allowed() {
    let project = Project::new();
    project.compile();
    let (content, layout) = project.built();
    let file = project.identity_file(DEVICE, &content, &layout);

    let comparison = output_of(with_identity(&project, &file));

    assert_eq!(comparison["row"], "upToDate");
    assert_eq!(comparison["rowNumber"], 5);
    assert_eq!(comparison["atOnce"], true);
    assert_eq!(comparison["project"]["status"], "upToDate");
    assert_eq!(comparison["project"]["contentHash"], content);
    assert_eq!(comparison["hotEdit"]["allowed"], true);
    assert_eq!(comparison["controller"]["deviceId"], DEVICE);
    assert_eq!(comparison["controller"]["holds"]["kind"], "application");
    assert_eq!(action(&comparison, "monitor")["enabled"], true);
}

#[test]
fn online_compare_when_source_changed_after_the_build_then_sources_changed() {
    let project = Project::new();
    project.compile();
    let (content, layout) = project.built();
    let file = project.identity_file(DEVICE, &content, &layout);
    project.write_source(EDITED_PROGRAM);

    let comparison = output_of(with_identity(&project, &file));

    assert_eq!(comparison["row"], "sourcesChanged");
    assert_eq!(comparison["project"]["status"], "changedSinceBuild");
    assert_eq!(comparison["atOnce"], false);
    assert_eq!(comparison["hotEdit"]["allowed"], true);
}

#[test]
fn online_compare_when_comparing_does_not_build_then_the_container_and_record_are_untouched() {
    let project = Project::new();
    project.compile();
    let (content, layout) = project.built();
    let file = project.identity_file(DEVICE, &content, &layout);
    let container = fs::read(project.application()).unwrap();
    let record = fs::read(project.dir.path().join("app.build.iplcinfo")).unwrap();
    project.write_source(EDITED_PROGRAM);

    output_of(with_identity(&project, &file));

    assert_eq!(fs::read(project.application()).unwrap(), container);
    assert_eq!(
        fs::read(project.dir.path().join("app.build.iplcinfo")).unwrap(),
        record
    );
}

#[test]
fn online_compare_when_controller_holds_the_last_download_then_previous_download() {
    let project = Project::new();
    project.compile();
    let (downloaded, layout) = project.built();
    project.record_download(DEVICE);
    project.write_source(EDITED_PROGRAM);
    project.compile();
    let file = project.identity_file(DEVICE, &downloaded, &layout);

    let comparison = output_of(with_identity(&project, &file));

    assert_eq!(comparison["row"], "previousDownload");
    assert_eq!(comparison["rowNumber"], 7);
    assert_eq!(comparison["lastDownload"]["contentHash"], downloaded);
    assert_eq!(comparison["hotEdit"]["allowed"], true);
}

#[test]
fn online_compare_when_controller_holds_something_else_then_other_application_and_download_only() {
    let project = Project::new();
    project.compile();
    let (_, layout) = project.built();
    let file = project.identity_file(DEVICE, &other_hash(), &layout);

    let comparison = output_of(with_identity(&project, &file));

    assert_eq!(comparison["row"], "otherApplication");
    assert_eq!(comparison["hotEdit"]["allowed"], false);
    assert_eq!(comparison["hotEdit"]["reason"]["code"], "otherApplication");
    assert_eq!(action(&comparison, "download")["enabled"], true);
    assert_eq!(action(&comparison, "monitor")["enabled"], true);
}

#[test]
fn online_compare_when_controller_holds_no_application_then_download_is_the_only_change() {
    let project = Project::new();
    project.compile();
    let (_, layout) = project.built();
    let empty = ironplc_container::integrity::hash_hex(&ironplc_container::EMPTY_APPLICATION_HASH)
        .to_string();
    let file = project.identity_file(DEVICE, &empty, &layout);

    let comparison = output_of(with_identity(&project, &file));

    assert_eq!(comparison["row"], "noApplication");
    assert_eq!(comparison["controller"]["holds"]["kind"], "noApplication");
    assert_eq!(action(&comparison, "onlineChange")["enabled"], false);
    assert_eq!(action(&comparison, "download")["enabled"], true);
}

#[test]
fn online_compare_when_project_was_never_built_then_never_built() {
    let project = Project::new();
    let file = project.identity_file(DEVICE, &other_hash(), &other_hash());

    let comparison = output_of(with_identity(&project, &file));

    assert_eq!(comparison["project"]["status"], "neverBuilt");
    assert_eq!(comparison["project"]["contentHash"], Value::Null);
    assert_eq!(action(&comparison, "download")["enabled"], true);
}

#[test]
fn online_compare_when_sources_do_not_build_then_build_fails_and_nothing_can_be_sent() {
    let project = Project::new();
    project.compile();
    let (content, layout) = project.built();
    let file = project.identity_file(DEVICE, &content, &layout);
    project.write_source(BROKEN_PROGRAM);

    let comparison = output_of(with_identity(&project, &file));

    assert_eq!(comparison["project"]["status"], "buildFails");
    assert_eq!(comparison["row"], "sourcesChanged");
    assert_eq!(kinds(&comparison), vec!["buildFails"]);
    assert_eq!(comparison["hotEdit"]["allowed"], false);
    assert_eq!(action(&comparison, "download")["enabled"], false);
    assert_eq!(action(&comparison, "monitor")["enabled"], true);
}

#[test]
fn online_compare_when_path_record_names_another_controller_then_information_row() {
    let project = Project::new();
    project.compile();
    project.record_path(OTHER_DEVICE);
    let (content, layout) = project.built();
    let file = project.identity_file(DEVICE, &content, &layout);

    let comparison = output_of(with_identity(&project, &file));

    assert_eq!(kinds(&comparison), vec!["otherController"]);
    assert_eq!(comparison["row"], "upToDate");
    assert_eq!(comparison["hotEdit"]["allowed"], true);
}

#[test]
fn online_compare_when_identity_on_standard_input_then_same_answer_as_from_a_file() {
    let project = Project::new();
    project.compile();
    let (content, layout) = project.built();
    let file = project.identity_file(DEVICE, &content, &layout);
    let from_file = output_of(with_identity(&project, &file));

    let mut cmd = project.online_compare();
    let mut child = cmd
        .args(["--identity", "-"])
        .stdin(Stdio::piped())
        .stdout(Stdio::piped())
        .spawn()
        .unwrap();
    child
        .stdin
        .take()
        .unwrap()
        .write_all(identity(DEVICE, &content, &layout).to_string().as_bytes())
        .unwrap();
    let output = child.wait_with_output().unwrap();

    assert!(output.status.success());
    let from_stdin: Value = serde_json::from_slice(&output.stdout).unwrap();
    assert_eq!(from_stdin, from_file);
}

#[test]
fn online_compare_when_attempt_did_not_reach_the_controller_then_row_and_retry() {
    let project = Project::new();
    project.compile();

    let mut cmd = project.online_compare();
    cmd.args(["--attempt", "busy"]);
    let comparison = output_of(cmd);

    assert_eq!(comparison["row"], "busy");
    assert_eq!(comparison["rowNumber"], 2);
    assert_eq!(comparison["connected"], false);
    assert_eq!(comparison["controller"], Value::Null);
    assert_eq!(action(&comparison, "retry")["enabled"], true);
    assert_eq!(action(&comparison, "monitor")["enabled"], false);
}

#[test]
fn online_compare_when_attempt_is_ok_without_identity_then_error() {
    let project = Project::new();

    project
        .online_compare()
        .assert()
        .failure()
        .stderr(predicate::str::contains("needs the identity"));
}

#[test]
fn online_compare_when_identity_given_with_another_attempt_then_error() {
    let project = Project::new();
    let file = project.identity_file(DEVICE, &other_hash(), &other_hash());

    with_identity(&project, &file)
        .args(["--attempt", "unreachable"])
        .assert()
        .failure()
        .stderr(predicate::str::contains("only with --attempt ok"));
}

#[test]
fn online_compare_when_identity_is_not_an_identity_then_error() {
    let project = Project::new();
    let file = project.dir.path().join("identity.json");
    fs::write(&file, r#"{"hello": 1}"#).unwrap();

    with_identity(&project, &file)
        .assert()
        .failure()
        .stderr(predicate::str::contains("cannot be read"));
}

#[test]
fn online_compare_when_identity_is_not_json_then_error() {
    let project = Project::new();
    let file = project.dir.path().join("identity.json");
    fs::write(&file, "not json").unwrap();

    with_identity(&project, &file)
        .assert()
        .failure()
        .stderr(predicate::str::contains("not JSON"));
}
