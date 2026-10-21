//! The build record in `compile`: an unchanged build does not compile and says
//! the application is up to date, a changed one does, and a record that cannot
//! be used is reported, rebuilt and rewritten.

use std::fs;
use std::path::PathBuf;

use ironplc_parser::options::CompilerOptions;
use ironplc_project::build_record::{self, BuildRecordRead};
use ironplc_project::RecordKind;
use tempfile::TempDir;

use super::{compile, run_compile};

const PROGRAM: &str = "PROGRAM main
VAR y : INT; END_VAR
  y := 1;
END_PROGRAM
";

const OTHER_PROGRAM: &str = "PROGRAM main
VAR y : INT; END_VAR
  y := 2;
END_PROGRAM
";

const UNDECLARED_VARIABLE: &str = "PROGRAM main
VAR y : INT; END_VAR
  y := x + 1;
END_PROGRAM
";

const STARTED: &str = "------ Build started: Application: main -------";
const UP_TO_DATE: &str = "The application is up to date";
const COMPLETE: &str = "Build complete -- 0 errors, 0 warnings : Ready for download";

/// A directory with one source `main.st` and the paths a build of it uses.
struct Workspace {
    _dir: TempDir,
    source: PathBuf,
    container: PathBuf,
    record: PathBuf,
}

impl Workspace {
    fn new(text: &str) -> Self {
        let dir = TempDir::new().unwrap();
        let source = dir.path().join("main.st");
        fs::write(&source, text).unwrap();
        let container = dir.path().join("app.iplc");
        let record = RecordKind::Build.path(&container).unwrap();
        Workspace {
            _dir: dir,
            source,
            container,
            record,
        }
    }

    fn write_source(&self, text: &str) {
        fs::write(&self.source, text).unwrap();
    }

    /// Builds and returns the lines the build shows.
    fn build(&self) -> Vec<String> {
        self.build_with(CompilerOptions::default(), false)
    }

    fn build_with(&self, options: CompilerOptions, force: bool) -> Vec<String> {
        let (_, diagnostics) = run_compile(
            std::slice::from_ref(&self.source),
            &self.container,
            options,
            &[],
            force,
        )
        .unwrap();
        diagnostics.iter().map(|d| d.person_text()).collect()
    }

    fn container_bytes(&self) -> Vec<u8> {
        fs::read(&self.container).unwrap()
    }

    fn record_bytes(&self) -> Vec<u8> {
        fs::read(&self.record).unwrap()
    }

    fn modified(&self) -> std::time::SystemTime {
        fs::metadata(&self.container).unwrap().modified().unwrap()
    }
}

fn built_lines() -> Vec<String> {
    vec![STARTED.to_owned(), COMPLETE.to_owned()]
}

fn up_to_date_lines() -> Vec<String> {
    vec![STARTED.to_owned(), UP_TO_DATE.to_owned()]
}

#[test]
fn compile_when_first_build_then_writes_the_record_beside_the_container_and_no_extra_line() {
    let work = Workspace::new(PROGRAM);

    let lines = work.build();

    assert_eq!(lines, built_lines());
    assert_eq!(work.record.file_name().unwrap(), "app.build.iplcinfo");
    assert!(matches!(
        build_record::read(&work.record).unwrap(),
        BuildRecordRead::Present(_)
    ));
}

#[test]
fn compile_when_nothing_changed_then_up_to_date_and_the_container_is_untouched() {
    let work = Workspace::new(PROGRAM);
    work.build();
    let bytes = work.container_bytes();
    let modified = work.modified();

    let lines = work.build();

    assert_eq!(lines, up_to_date_lines());
    assert_eq!(work.container_bytes(), bytes);
    assert_eq!(work.modified(), modified);
}

#[test]
fn compile_when_up_to_date_then_the_command_succeeds() {
    let work = Workspace::new(PROGRAM);
    let paths = [work.source.clone()];
    compile(
        &paths,
        &work.container,
        CompilerOptions::default(),
        &[],
        false,
        true,
    )
    .unwrap();

    let result = compile(
        &paths,
        &work.container,
        CompilerOptions::default(),
        &[],
        false,
        true,
    );

    assert_eq!(result, Ok(()));
}

#[test]
fn compile_when_up_to_date_and_application_has_a_configuration_then_the_line_names_it() {
    let work = Workspace::new(
        "PROGRAM main
VAR y : INT; END_VAR
  y := 1;
END_PROGRAM

CONFIGURATION Plant
  RESOURCE r ON PLC
    TASK t(INTERVAL := T#10ms, PRIORITY := 1);
    PROGRAM i1 WITH t : main;
  END_RESOURCE
END_CONFIGURATION
",
    );
    work.build();

    let lines = work.build();

    assert_eq!(
        lines,
        vec![
            "------ Build started: Application: Plant -------".to_owned(),
            UP_TO_DATE.to_owned()
        ]
    );
}

#[test]
fn compile_when_force_then_builds_again() {
    let work = Workspace::new(PROGRAM);
    work.build();

    let lines = work.build_with(CompilerOptions::default(), true);

    assert_eq!(lines, built_lines());
    assert!(matches!(
        build_record::read(&work.record).unwrap(),
        BuildRecordRead::Present(_)
    ));
}

#[test]
fn compile_when_force_then_the_record_is_written_and_the_next_build_is_up_to_date() {
    let work = Workspace::new(PROGRAM);
    work.build_with(CompilerOptions::default(), true);

    let lines = work.build();

    assert_eq!(lines, up_to_date_lines());
}

#[test]
fn compile_when_source_changed_then_builds_and_the_next_build_is_up_to_date() {
    let work = Workspace::new(PROGRAM);
    work.build();
    let before = work.container_bytes();

    work.write_source(OTHER_PROGRAM);
    let lines = work.build();

    assert_eq!(lines, built_lines());
    assert_ne!(work.container_bytes(), before);
    assert_eq!(work.build(), up_to_date_lines());
}

#[test]
fn compile_when_option_changed_then_builds() {
    let work = Workspace::new(PROGRAM);
    work.build();
    let options = CompilerOptions {
        allow_c_style_comments: true,
        ..CompilerOptions::default()
    };

    let lines = work.build_with(options, false);

    assert_eq!(lines, built_lines());
}

#[test]
fn compile_when_output_deleted_then_builds_and_writes_it_again() {
    let work = Workspace::new(PROGRAM);
    work.build();
    let bytes = work.container_bytes();
    fs::remove_file(&work.container).unwrap();

    let lines = work.build();

    assert_eq!(lines, built_lines());
    assert_eq!(work.container_bytes(), bytes);
}

#[test]
fn compile_when_output_has_another_content_hash_then_builds_and_restores_it() {
    let work = Workspace::new(PROGRAM);
    work.build();
    let bytes = work.container_bytes();
    // The content hash starts at byte 8 of the header.
    let mut altered = bytes.clone();
    altered[8] ^= 0xff;
    fs::write(&work.container, altered).unwrap();

    let lines = work.build();

    assert_eq!(lines, built_lines());
    assert_eq!(work.container_bytes(), bytes);
}

#[test]
fn compile_when_output_is_not_a_container_then_builds() {
    let work = Workspace::new(PROGRAM);
    work.build();
    let bytes = work.container_bytes();
    fs::write(&work.container, b"not a container").unwrap();

    let lines = work.build();

    assert_eq!(lines, built_lines());
    assert_eq!(work.container_bytes(), bytes);
}

#[test]
fn compile_when_record_missing_then_builds_without_a_message_and_writes_it() {
    let work = Workspace::new(PROGRAM);
    work.build();
    fs::remove_file(&work.record).unwrap();

    let lines = work.build();

    assert_eq!(lines, built_lines());
    assert!(work.record.exists());
}

/// A record that cannot be used: the build runs, says so once, naming the file,
/// and writes the record again.
fn assert_unusable_record_is_rebuilt(text: &str) {
    let work = Workspace::new(PROGRAM);
    work.build();
    let good_record = work.record_bytes();
    fs::write(&work.record, text).unwrap();

    let lines = work.build();

    assert_eq!(lines.len(), 3, "{lines:?}");
    assert_eq!(lines[0], STARTED);
    assert!(
        lines[1].starts_with("The record ") && lines[1].contains("app.build.iplcinfo"),
        "{lines:?}"
    );
    assert_eq!(lines[2], COMPLETE);
    assert_eq!(work.record_bytes(), good_record);
    assert_eq!(work.build(), up_to_date_lines());
}

#[test]
fn compile_when_record_is_not_json_then_reports_it_builds_and_rewrites_it() {
    assert_unusable_record_is_rebuilt("this is not json");
}

#[test]
fn compile_when_record_has_another_version_then_reports_it_builds_and_rewrites_it() {
    assert_unusable_record_is_rebuilt(r#"{"version": 2}"#);
}

#[test]
fn compile_when_record_has_the_version_but_not_the_shape_then_reports_it_builds_and_rewrites_it() {
    assert_unusable_record_is_rebuilt(r#"{"version": 1, "inputs": 7}"#);
}

#[test]
fn compile_when_build_fails_then_the_record_and_the_container_stay() {
    let work = Workspace::new(PROGRAM);
    work.build();
    let record = work.record_bytes();
    let container = work.container_bytes();

    work.write_source(UNDECLARED_VARIABLE);
    let lines = work.build();

    assert!(
        lines.last().unwrap().ends_with("No download possible"),
        "{lines:?}"
    );
    assert_eq!(work.record_bytes(), record);
    assert_eq!(work.container_bytes(), container);
}

#[test]
fn compile_when_first_build_fails_then_no_record_is_written() {
    let work = Workspace::new(UNDECLARED_VARIABLE);

    work.build();

    assert!(!work.record.exists());
    assert!(!work.container.exists());
}

#[test]
fn compile_when_build_failed_and_source_restored_then_up_to_date() {
    let work = Workspace::new(PROGRAM);
    work.build();
    work.write_source(UNDECLARED_VARIABLE);
    work.build();

    work.write_source(PROGRAM);
    let lines = work.build();

    // The record still describes the container on disk, so restoring the source
    // that built it is an unchanged build.
    assert_eq!(lines, up_to_date_lines());
}
