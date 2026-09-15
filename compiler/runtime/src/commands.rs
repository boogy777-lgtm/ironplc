//! The hot-edit command layer: typed, serializable commands over the P0
//! online change protocol with a line-delimited JSON codec.
//!
//! One line carries one [`Command`]; one line carries one [`Response`].
//! Compilation stays the client's job, so [`Command::AcceptEdits`] carries
//! the compiled container as raw bytes (a JSON array on the wire). The layer
//! adds no protocol state of its own: [`execute`] maps each command onto the
//! matching [`RuntimeHost`] call, and failures become stable user-facing
//! V-codes from the crate's `problem-codes.csv` (kept separate from the VM's
//! `Trap` codes, which the VM's own CSV owns).

use core::fmt;
use std::io::Cursor;

use ironplc_container::Container;
use serde::{Deserialize, Serialize};

use crate::error::OnlineChangeError;
use crate::host::{HostMode, HostStatus, RuntimeHost};

// V-code constants are generated from resources/problem-codes.csv by build.rs.
mod online_change_codes {
    include!(concat!(env!("OUT_DIR"), "/online_change_codes.rs"));
}

/// A command in the hot-edit protocol.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(tag = "command", rename_all = "camelCase")]
pub enum Command {
    /// Returns the host's hot-edit status.
    GetStatus,
    /// Stages the compiled container `program` as the edit candidate.
    AcceptEdits {
        /// The compiled container in its wire format.
        program: Vec<u8>,
    },
    /// Activates the staged candidate at the next scan boundary.
    TestEdits,
    /// Reverts to the original artifact at the next scan boundary.
    UntestEdits,
    /// Promotes the staged candidate to the running application.
    AssembleEdits,
    /// Discards the staged candidate.
    CancelEdits,
}

/// The payload of [`Response::Status`]: a snapshot of the host's hot-edit
/// state, mirroring [`HostStatus`] with generation counters as plain numbers.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct StatusPayload {
    /// Which artifact is executing.
    pub mode: HostMode,
    /// Generation of the artifact currently executing.
    pub active: u32,
    /// Generation of the normal (original) artifact.
    pub normal: u32,
    /// Generation of the staged candidate, if one is staged.
    pub candidate: Option<u32>,
    /// Generation of the active application manifest.
    pub application: u32,
    /// Whether the staged candidate changes the schema, i.e. carries a state
    /// migration plan because its layout hash differs.
    pub migration: bool,
    /// Completed scan rounds since the host was created.
    pub rounds: u64,
}

impl From<HostStatus> for StatusPayload {
    fn from(status: HostStatus) -> Self {
        StatusPayload {
            mode: status.mode,
            active: status.active.raw(),
            normal: status.normal.raw(),
            candidate: status.candidate.map(|generation| generation.raw()),
            application: status.application.raw(),
            migration: status.migration,
            rounds: status.rounds,
        }
    }
}

/// The answer to one [`Command`].
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(tag = "response", rename_all = "camelCase")]
pub enum Response {
    /// The host's status (the answer to [`Command::GetStatus`]).
    Status(StatusPayload),
    /// The command succeeded; there is nothing further to report.
    Ack,
    /// The command failed; carries a stable V-code and the host's message.
    Error(CommandError),
}

/// Why a command failed: a stable V-code plus the host's own description of
/// the problem. V-codes are generated from `resources/problem-codes.csv`.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
pub struct CommandError {
    /// The stable V-code (e.g. `"V4007"`).
    #[serde(rename = "vCode")]
    v_code: &'static str,
    /// What failed, in the host's vocabulary.
    message: String,
}

impl CommandError {
    /// The stable V-code for this error (e.g. `"V4007"`).
    pub fn v_code(&self) -> &'static str {
        self.v_code
    }

    /// What failed, in the host's vocabulary.
    pub fn message(&self) -> &str {
        &self.message
    }

    /// Builds the error for an [`Command::AcceptEdits`] payload that does not
    /// parse as a compiled container.
    fn invalid_container(error: impl fmt::Display) -> Self {
        CommandError {
            v_code: online_change_codes::INVALID_CONTAINER,
            message: format!("accept payload is not a valid compiled container: {error}"),
        }
    }
}

impl fmt::Display for CommandError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{} - {}", self.v_code, self.message)
    }
}

impl From<OnlineChangeError> for CommandError {
    fn from(error: OnlineChangeError) -> Self {
        let v_code = match error {
            OnlineChangeError::LayoutIncompatible => online_change_codes::LAYOUT_INCOMPATIBLE,
            OnlineChangeError::ScheduleIncompatible => online_change_codes::SCHEDULE_INCOMPATIBLE,
            OnlineChangeError::IoIncompatible => online_change_codes::IO_INCOMPATIBLE,
            OnlineChangeError::MigrationUnsupported(_) => {
                online_change_codes::MIGRATION_UNSUPPORTED
            }
            OnlineChangeError::UntestUnsupported => online_change_codes::UNTEST_UNSUPPORTED,
            OnlineChangeError::NoCandidateStaged => online_change_codes::NO_CANDIDATE_STAGED,
            OnlineChangeError::CandidateAlreadyStaged => {
                online_change_codes::CANDIDATE_ALREADY_STAGED
            }
            OnlineChangeError::NoTestInProgress => online_change_codes::NO_TEST_IN_PROGRESS,
            OnlineChangeError::NotAllowedInThisMode => {
                online_change_codes::NOT_ALLOWED_IN_THIS_MODE
            }
        };
        CommandError {
            v_code,
            message: error.to_string(),
        }
    }
}

/// Parses one line-delimited command: a single JSON value without its
/// trailing newline.
pub fn parse_command(line: &str) -> Result<Command, serde_json::Error> {
    serde_json::from_str(line)
}

/// Renders one response as a single line of JSON, without a trailing newline.
pub fn render_response(response: &Response) -> Result<String, serde_json::Error> {
    serde_json::to_string(response)
}

/// Executes `command` against `host` and returns the response to send.
///
/// This is the whole command mapping: each variant delegates to the matching
/// [`RuntimeHost`] method, so the host's controller FSM remains the only
/// protocol state (ADR-0052).
pub fn execute(command: Command, host: &mut RuntimeHost) -> Response {
    match command {
        Command::GetStatus => Response::Status(StatusPayload::from(host.status())),
        Command::AcceptEdits { program } => accept_edits(host, &program),
        Command::TestEdits => ack_or_error(host.test()),
        Command::UntestEdits => ack_or_error(host.untest()),
        Command::AssembleEdits => ack_or_error(host.assemble()),
        Command::CancelEdits => ack_or_error(host.cancel()),
    }
}

/// Parses the candidate container and stages it against the active artifact.
fn accept_edits(host: &mut RuntimeHost, program: &[u8]) -> Response {
    let candidate = match Container::read_from(&mut Cursor::new(program)) {
        Ok(candidate) => candidate,
        Err(error) => return Response::Error(CommandError::invalid_container(error)),
    };
    ack_or_error(host.stage(candidate))
}

/// Turns a host result into an acknowledgment or the coded error.
fn ack_or_error(result: Result<(), OnlineChangeError>) -> Response {
    match result {
        Ok(()) => Response::Ack,
        Err(error) => Response::Error(CommandError::from(error)),
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::migration::MigrationError;
    use rstest::rstest;

    #[test]
    fn parse_command_when_get_status_then_get_status() {
        assert_eq!(
            parse_command(r#"{"command":"getStatus"}"#).unwrap(),
            Command::GetStatus
        );
    }

    #[test]
    fn parse_command_when_accept_edits_then_carries_program_bytes() {
        let command = parse_command(r#"{"command":"acceptEdits","program":[1,2,255]}"#).unwrap();

        assert_eq!(
            command,
            Command::AcceptEdits {
                program: vec![1, 2, 255]
            }
        );
    }

    #[test]
    fn parse_command_when_test_untest_assemble_cancel_then_matching_command() {
        assert_eq!(
            parse_command(r#"{"command":"testEdits"}"#).unwrap(),
            Command::TestEdits
        );
        assert_eq!(
            parse_command(r#"{"command":"untestEdits"}"#).unwrap(),
            Command::UntestEdits
        );
        assert_eq!(
            parse_command(r#"{"command":"assembleEdits"}"#).unwrap(),
            Command::AssembleEdits
        );
        assert_eq!(
            parse_command(r#"{"command":"cancelEdits"}"#).unwrap(),
            Command::CancelEdits
        );
    }

    #[test]
    fn parse_command_when_line_is_not_json_then_error() {
        assert!(parse_command("not json").is_err());
    }

    #[test]
    fn parse_command_when_command_is_unknown_then_error() {
        assert!(parse_command(r#"{"command":"frobinate"}"#).is_err());
    }

    #[test]
    fn render_response_when_status_then_serializes_every_field() {
        let response = Response::Status(StatusPayload {
            mode: HostMode::Testing,
            active: 2,
            normal: 1,
            candidate: Some(2),
            application: 1,
            migration: true,
            rounds: 42,
        });

        let line = render_response(&response).unwrap();
        let value: serde_json::Value = serde_json::from_str(&line).unwrap();

        assert_eq!(value["response"], "status");
        assert_eq!(value["mode"], "testing");
        assert_eq!(value["active"], 2);
        assert_eq!(value["normal"], 1);
        assert_eq!(value["candidate"], 2);
        assert_eq!(value["application"], 1);
        assert_eq!(value["migration"], true);
        assert_eq!(value["rounds"], 42);
    }

    #[test]
    fn render_response_when_ack_then_line_names_the_ack() {
        assert_eq!(
            render_response(&Response::Ack).unwrap(),
            r#"{"response":"ack"}"#
        );
    }

    #[test]
    fn render_response_when_error_then_line_carries_v_code_and_message() {
        let response = Response::Error(CommandError::from(OnlineChangeError::NoCandidateStaged));

        let line = render_response(&response).unwrap();

        assert!(line.starts_with(r#"{"response":"error""#));
        assert!(line.contains(r#""vCode":"V4012""#));
        assert!(line.contains("no candidate is staged"));
    }

    #[test]
    fn command_error_display_then_formats_code_dash_message() {
        let error = CommandError::from(OnlineChangeError::NoCandidateStaged);

        assert_eq!(error.to_string(), "V4012 - no candidate is staged");
        assert_eq!(error.v_code(), "V4012");
        assert_eq!(error.message(), "no candidate is staged");
    }

    #[rstest]
    #[case::layout(OnlineChangeError::LayoutIncompatible, "V4007")]
    #[case::schedule(OnlineChangeError::ScheduleIncompatible, "V4008")]
    #[case::io(OnlineChangeError::IoIncompatible, "V4009")]
    #[case::migration(
        OnlineChangeError::MigrationUnsupported(MigrationError::FbLayoutUnsupported),
        "V4010"
    )]
    #[case::untest(OnlineChangeError::UntestUnsupported, "V4011")]
    #[case::none_staged(OnlineChangeError::NoCandidateStaged, "V4012")]
    #[case::already_staged(OnlineChangeError::CandidateAlreadyStaged, "V4013")]
    #[case::no_test(OnlineChangeError::NoTestInProgress, "V4014")]
    #[case::wrong_mode(OnlineChangeError::NotAllowedInThisMode, "V4015")]
    fn command_error_v_code_when_online_change_error_then_stable_code(
        #[case] error: OnlineChangeError,
        #[case] expected: &'static str,
    ) {
        assert_eq!(CommandError::from(error).v_code(), expected);
    }

    #[test]
    fn command_error_v_code_when_invalid_container_then_v4016() {
        let error = CommandError::invalid_container("invalid magic number");

        assert_eq!(error.v_code(), "V4016");
        assert_eq!(
            error.message(),
            "accept payload is not a valid compiled container: invalid magic number"
        );
    }
}
