//! The go-online comparison in the command line: `online-compare` gathers what
//! the comparison reads and prints its answer as JSON.
//!
//! The comparison is `ironplc_project::online_comparison::compare`; this module
//! only gathers its inputs. The identity of the controller is read from the
//! JSON the editor received from the session (a file, or standard input), the
//! records are read beside the application container, and the state of the
//! project is the up-to-date check of the build record run without building
//! ([`up_to_date::query`]): the sources are read and compared with the record,
//! and only when they differ are they analyzed to see whether they build.

use std::io::Read;
use std::path::{Path, PathBuf};

use ironplc_dsl::diagnostic::{any_stops_build, Area, Diagnostic};
use ironplc_parser::options::CompilerOptions;
use ironplc_project::build_record::BuildInputs;
use ironplc_project::download_record::{self, DownloadRecordRead};
use ironplc_project::online_comparison::{
    compare, Attempt, BuiltApplication, DeviceFacts, LastDownload, ProjectState, Records,
};
use ironplc_project::path_record::{self, PathRecordRead};
use ironplc_project::Project;
use ironplc_sources::LibraryName;

use super::up_to_date::{self, Query};
use super::{create_project, project_name};

/// How the attempt to connect went, as the editor or the script says it.
#[derive(clap::ValueEnum, Clone, Copy, Debug, PartialEq, Eq)]
pub enum ConnectOutcome {
    /// A session opened and the controller answered `identity` (read from
    /// `--identity`).
    Ok,
    /// The controller did not answer.
    Unreachable,
    /// The controller serves another session (V6014).
    Busy,
    /// The controller is protected.
    Protected,
    /// The session was open and ended.
    ConnectionLost,
}

const NEEDS_IDENTITY: &str = "--attempt ok needs the identity of the controller (--identity)";

/// What `online-compare` was given.
pub struct OnlineCompare<'a> {
    /// The sources of the project.
    pub paths: &'a [PathBuf],
    /// The application container the project builds to: the records sit beside
    /// it.
    pub output: &'a Path,
    /// The options of the build.
    pub compiler_options: CompilerOptions,
    /// The libraries of the build.
    pub libraries: &'a [LibraryName],
    /// How the attempt to connect went.
    pub outcome: ConnectOutcome,
    /// Where the identity is read from when the attempt is ok: a file, or `-`
    /// for standard input.
    pub identity: Option<&'a Path>,
}

/// Compares the controller with the project and returns the comparison as JSON
/// text. Every state of the controller and of the project is an answer; only a
/// question that cannot be put is an error: an identity that cannot be read, or
/// a record that cannot be read from the file system.
pub fn online_compare(args: &OnlineCompare<'_>) -> Result<String, String> {
    let device = match (args.outcome, args.identity) {
        (ConnectOutcome::Ok, Some(source)) => Some(read_device(source)?),
        (ConnectOutcome::Ok, None) => return Err(NEEDS_IDENTITY.to_owned()),
        (_, Some(_)) => return Err("--identity is read only with --attempt ok".to_owned()),
        (_, None) => None,
    };
    let attempt = match (args.outcome, &device) {
        (ConnectOutcome::Ok, Some(device)) => Attempt::Ok(device),
        (ConnectOutcome::Ok, None) => return Err(NEEDS_IDENTITY.to_owned()),
        (ConnectOutcome::Unreachable, _) => Attempt::Unreachable,
        (ConnectOutcome::Busy, _) => Attempt::Busy,
        (ConnectOutcome::Protected, _) => Attempt::Protected,
        (ConnectOutcome::ConnectionLost, _) => Attempt::ConnectionLost,
    };

    let project = project_state(args)?;
    let download = device
        .as_ref()
        .and_then(|device| device.device_id)
        .map(|id| read_download(args.output, &id))
        .transpose()?
        .flatten();
    let path = read_path(args.output)?;
    let records = Records {
        download: download
            .as_ref()
            .map(|(record, kept_container)| LastDownload {
                record,
                kept_container: *kept_container,
            }),
        path: path.as_ref(),
    };

    let comparison = compare(&attempt, &records, &project);
    serde_json::to_string_pretty(&comparison).map_err(|err| err.to_string())
}

/// The identity of the controller, read from `source`: a file, or `-` for
/// standard input.
fn read_device(source: &Path) -> Result<DeviceFacts, String> {
    let text = if source == Path::new("-") {
        let mut text = String::new();
        std::io::stdin()
            .read_to_string(&mut text)
            .map_err(|err| format!("Failed to read the identity from standard input: {err}"))?;
        text
    } else {
        std::fs::read_to_string(source)
            .map_err(|err| format!("Failed to read {}: {err}", source.display()))?
    };
    let value: serde_json::Value =
        serde_json::from_str(&text).map_err(|err| format!("The identity is not JSON: {err}"))?;
    DeviceFacts::from_identity(&value).map_err(|err| err.to_string())
}

/// The state of the project against its last build, found without building.
///
/// A problem of discovery or of reading the sources stops the build, so the
/// project does not build. When the sources and the container are those of the
/// last build, the project is up to date and nothing is analyzed. Otherwise the
/// sources are analyzed as `check` does, and a problem there is a build that
/// fails. The analysis stops short of code generation, so a construct the code
/// generator refuses shows only when the build runs.
fn project_state(args: &OnlineCompare<'_>) -> Result<ProjectState, String> {
    let (mut project, discovery) =
        create_project(args.paths, args.compiler_options, args.libraries);
    let inputs = BuildInputs::of(
        &project,
        &args.compiler_options,
        &project_name(args.paths),
        env!("CARGO_PKG_VERSION"),
    );
    // A record that is not usable is no record; the next build rewrites it.
    let record = match up_to_date::query(args.output, &inputs) {
        Query::Missing | Query::Malformed(_) => None,
        Query::Failed(problem) => return Err(describe(&problem)),
        Query::Present { record, up_to_date } => {
            Some((BuiltApplication::from(&*record), up_to_date))
        }
    };

    if any_stops_build(&discovery) {
        return Ok(ProjectState::BuildFails(record.map(|(built, _)| built)));
    }
    if let Some((built, true)) = record {
        return Ok(ProjectState::UpToDate(built));
    }
    let analysis = project.semantic(Area::Build);
    let builds = !any_stops_build(&analysis);
    Ok(match (builds, record) {
        (false, record) => ProjectState::BuildFails(record.map(|(built, _)| built)),
        (true, Some((built, _))) => ProjectState::ChangedSinceBuild(built),
        (true, None) => ProjectState::NeverBuilt,
    })
}

/// The download record of the device and whether the container that was sent is
/// kept beside it. A missing or damaged record is no record.
fn read_download(
    output: &Path,
    id: &ironplc_project::DeviceId,
) -> Result<Option<(download_record::DownloadRecord, bool)>, String> {
    let path = download_record::record_path(output, id).map_err(|err| err.to_string())?;
    match download_record::read(id, &path).map_err(|problem| describe(&problem))? {
        DownloadRecordRead::Present(record) => {
            let kept = download_record::kept_container_path(output, id)
                .map_err(|err| err.to_string())?
                .is_file();
            Ok(Some((*record, kept)))
        }
        DownloadRecordRead::Missing | DownloadRecordRead::Malformed => Ok(None),
    }
}

/// The path record of the application. A missing or damaged record is no record.
fn read_path(output: &Path) -> Result<Option<path_record::PathRecord>, String> {
    let path = path_record::record_path(output).map_err(|err| err.to_string())?;
    match path_record::read(&path).map_err(|problem| describe(&problem))? {
        PathRecordRead::Present(record) => Ok(Some(record)),
        PathRecordRead::Missing | PathRecordRead::Malformed => Ok(None),
    }
}

fn describe(problem: &Diagnostic) -> String {
    format!(
        "{}: {}: {}",
        problem.code,
        problem.description(),
        problem.primary.message
    )
}
