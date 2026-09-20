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
use std::collections::BTreeMap;
use std::io::Cursor;

use ironplc_container::Container;
use serde::{Deserialize, Serialize};

use crate::conversion::type_name;
use crate::error::OnlineChangeError;
use crate::host::{AcceptedEdit, HostMode, HostStatus, PendingEditRecord, RuntimeHost};
use crate::migration::{MigrationDecision, MigrationError, TypeChangePair};

// V-code constants are generated from resources/problem-codes.csv by build.rs.
mod online_change_codes {
    include!(concat!(env!("OUT_DIR"), "/online_change_codes.rs"));
}

/// A command in the hot-edit protocol.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(tag = "command", rename_all = "camelCase")]
pub enum Command {
    /// Returns the device panel and the application-state snapshot: the
    /// handshake of the engineering connection (ADR-0063), the first command
    /// on every (re)opened transport.
    Identity,
    /// Returns the host's hot-edit status.
    GetStatus,
    /// Stages the compiled container `program` as the edit candidate.
    AcceptEdits {
        /// The compiled container in its wire format.
        program: Vec<u8>,
        /// The optional edit identity (ADR-0064 debt closure): labels that
        /// become the pending-edit record reported in the status payload.
        /// Absent (the default) stages the candidate with an unnamed record.
        #[serde(default, skip_serializing_if = "Option::is_none")]
        edit: Option<EditSpec>,
        /// Per-UID engineer decisions for out-of-policy type changes
        /// (ADR 0061), keyed by stable variable UID. Empty (the default)
        /// fails closed with a V4010 naming every problematic pair.
        #[serde(
            default,
            deserialize_with = "deserialize_migration_map",
            skip_serializing_if = "BTreeMap::is_empty"
        )]
        migration: BTreeMap<u64, MigrationDecisionSpec>,
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

/// The optional `edit` identity block of [`Command::AcceptEdits`]
/// (ADR-0064 debt closure): what the pending-edit record reports while the
/// candidate is staged. Both fields are optional; `origin` is an
/// unauthenticated advisory client label, not engineer identity (ADR-0065
/// decision 2).
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct EditSpec {
    /// The client-supplied edit label.
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub name: Option<String>,
    /// The unauthenticated advisory client label.
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub origin: Option<String>,
}

/// The engineer's decision for one storage-class change as it travels on the
/// wire (ADR 0061): the values of [`Command::AcceptEdits`]'s `migration` map.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "lowercase")]
pub enum MigrationDecisionSpec {
    /// Discard the old value; the candidate's init image stands.
    Init,
    /// Keep the old storage bytes and reinterpret them under the candidate
    /// type (equal-size pairs only); the value may no longer be valid.
    Preserve,
}

impl From<MigrationDecisionSpec> for MigrationDecision {
    fn from(spec: MigrationDecisionSpec) -> Self {
        match spec {
            MigrationDecisionSpec::Init => MigrationDecision::Init,
            MigrationDecisionSpec::Preserve => MigrationDecision::Preserve,
        }
    }
}

/// Deserializes the `migration` map, whose JSON object keys are UID strings
/// and become `u64`s. The explicit string round trip is required because an
/// internally tagged enum (`Command`) buffers its content before the field is
/// deserialized, and buffered map keys cannot be read as integers; a key that
/// is not a UID rejects the command line.
fn deserialize_migration_map<'de, D>(
    deserializer: D,
) -> Result<BTreeMap<u64, MigrationDecisionSpec>, D::Error>
where
    D: serde::Deserializer<'de>,
{
    let wire = BTreeMap::<String, MigrationDecisionSpec>::deserialize(deserializer)?;
    wire.into_iter()
        .map(|(uid, decision)| {
            uid.parse::<u64>()
                .map(|uid| (uid, decision))
                .map_err(serde::de::Error::custom)
        })
        .collect()
}

/// One out-of-policy storage-class change in a [`CommandError`]'s `pairs`
/// details (ADR 0061): what a client renders as a decision-list row.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct TypeChangeDetail {
    /// The entity's stable UID, the key of the decision map.
    pub uid: u64,
    /// The entity's debug name, when the candidate names it.
    pub name: Option<String>,
    /// The active artifact's storage class (e.g. `"I32"`).
    pub from: String,
    /// The candidate's storage class (e.g. `"U32"`).
    pub to: String,
    /// Whether a `preserve` decision is legal for this pair: both sides'
    /// storage sizes match.
    pub size_equal: bool,
}

impl TypeChangeDetail {
    /// Names one planner pair in the wire vocabulary.
    fn from_pair(pair: &TypeChangePair) -> Self {
        TypeChangeDetail {
            uid: pair.uid,
            name: pair.name.clone(),
            from: type_name(pair.from).to_string(),
            to: type_name(pair.to).to_string(),
            size_equal: pair.size_equal,
        }
    }
}

/// The payload of [`Response::Status`]: a snapshot of the host's hot-edit
/// state, mirroring [`HostStatus`] with generation counters as plain numbers.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
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
    /// The pending-edit record written at Accept, if a candidate is staged
    /// (ADR-0064 debt closure). Absent on the wire when there is none — the
    /// additive block shape the engineering connection specifies.
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub pending_edit: Option<PendingEditRecord>,
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
            pending_edit: status.pending_edit,
        }
    }
}

/// The session protocol version this server speaks (ADR-0063): `1` for the
/// version the engineering connection spec defines. A client refuses a
/// higher number; an older server answers every command it does not know —
/// including `identity` — with the codeless codec error, which is the
/// version negotiation.
pub const SESSION_PROTOCOL_VERSION: u32 = 1;

/// The device panel block of an [`IdentityPayload`] (ADR-0063): the static
/// device description the application composes — a soft device reports its
/// binary name and version (`vm-cli` builds it from `env!("CARGO_PKG_VERSION")`),
/// a hardware target reports its catalog values.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct DeviceIdentity {
    /// The device name (e.g. `"ironplcvm"`).
    pub name: String,
    /// The device model (e.g. `"IronPLC SoftPLC"`).
    pub model: String,
    /// The device modification (e.g. `"vm-cli"`).
    pub modification: String,
    /// The firmware version (e.g. `"0.13.0"`).
    pub firmware_version: String,
}

/// The optional redundancy block of an [`IdentityPayload`] (ADR-0063):
/// `pairId`, configured `role`, ownership `epoch`, and the SYNC/CONTROL
/// substates of the HA redundancy FSM. Absent on the wire means standalone.
///
/// Shape only today: no server loads the redundancy layer, so `identity`
/// always answers without this block. It exists so the field set grows
/// structurally — servers add the block, clients ignore fields they do not
/// know — instead of by a negotiated schema.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct RedundancyIdentity {
    /// The redundancy pair's identifier.
    pub pair_id: String,
    /// The configured role (e.g. `"primary"`).
    pub role: String,
    /// The ownership epoch.
    pub epoch: u32,
    /// The SYNC substate (e.g. `"syncReady"`).
    pub sync: String,
    /// The CONTROL substate (e.g. `"active"`).
    pub control: String,
}

/// The payload of [`Response::Identity`]: the device panel fields, the live
/// application-state snapshot, and the optional redundancy block (ADR-0063).
/// The `application` block reuses the [`StatusPayload`] vocabulary verbatim,
/// additive optional fields included, so a client renders one shape for both
/// `identity` and `getStatus`.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct IdentityPayload {
    /// The session protocol version; see [`SESSION_PROTOCOL_VERSION`].
    pub protocol: u32,
    /// The device panel block.
    pub device: DeviceIdentity,
    /// The live hot-edit status snapshot, the same fields `getStatus` returns.
    pub application: StatusPayload,
    /// The redundancy block; absent on the wire when standalone.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub redundancy: Option<RedundancyIdentity>,
}

/// The answer to one [`Command`].
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(tag = "response", rename_all = "camelCase")]
pub enum Response {
    /// The device panel and application snapshot (the answer to
    /// [`Command::Identity`]). Boxed: the payload is four strings plus a
    /// status snapshot, and the response enum travels one at a time.
    Identity(Box<IdentityPayload>),
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
    /// The V4010 type changes, one entry per problematic entity, so the
    /// client can resubmit the same edit with a `migration` decision map
    /// (ADR 0061). Absent on every other refusal.
    #[serde(skip_serializing_if = "Option::is_none")]
    pairs: Option<Vec<TypeChangeDetail>>,
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

    /// The V4010 type changes to decide, or `None` for any other refusal.
    pub fn pairs(&self) -> Option<&[TypeChangeDetail]> {
        self.pairs.as_deref()
    }

    /// Builds the error for an [`Command::AcceptEdits`] payload that does not
    /// parse as a compiled container.
    fn invalid_container(error: impl fmt::Display) -> Self {
        CommandError {
            v_code: online_change_codes::INVALID_CONTAINER,
            message: format!("accept payload is not a valid compiled container: {error}"),
            pairs: None,
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
        let pairs = migration_pairs(&error);
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
            OnlineChangeError::AssembleWithoutTest => online_change_codes::ASSEMBLE_WITHOUT_TEST,
        };
        CommandError {
            v_code,
            message: error.to_string(),
            pairs,
        }
    }
}

/// The structured pair list a V4010 refusal carries, or `None` for every
/// other migration failure and every other error.
fn migration_pairs(error: &OnlineChangeError) -> Option<Vec<TypeChangeDetail>> {
    match error {
        OnlineChangeError::MigrationUnsupported(MigrationError::TypeChangeUnsupported {
            pairs,
        }) => Some(pairs.iter().map(TypeChangeDetail::from_pair).collect()),
        _ => None,
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
/// [`RuntimeHost`] call, so the host's controller FSM remains the only
/// protocol state (ADR-0052). `device` is the application-composed device
/// panel (ADR-0063): the only block the host does not own, so the identity
/// handshake takes it as a parameter — the type system requires a device
/// block wherever commands execute.
pub fn execute(command: Command, host: &mut RuntimeHost, device: &DeviceIdentity) -> Response {
    match command {
        Command::Identity => Response::Identity(Box::new(IdentityPayload {
            protocol: SESSION_PROTOCOL_VERSION,
            device: device.clone(),
            application: StatusPayload::from(host.status()),
            redundancy: None,
        })),
        Command::GetStatus => Response::Status(StatusPayload::from(host.status())),
        Command::AcceptEdits {
            program,
            edit,
            migration,
        } => accept_edits(host, &program, edit, &migration),
        Command::TestEdits => ack_or_error(host.test()),
        Command::UntestEdits => ack_or_error(host.untest()),
        Command::AssembleEdits => ack_or_error(host.assemble()),
        Command::CancelEdits => ack_or_error(host.cancel()),
    }
}

/// Parses the candidate container and stages it against the active artifact,
/// resolving out-of-policy type changes with the engineer's decisions. The
/// program's exact wire bytes and the optional `edit` identity ride along as
/// the accept-supplied extras (ADR-0064): the bytes for the assemble commit,
/// the labels for the pending-edit record.
fn accept_edits(
    host: &mut RuntimeHost,
    program: &[u8],
    edit: Option<EditSpec>,
    migration: &BTreeMap<u64, MigrationDecisionSpec>,
) -> Response {
    let candidate = match Container::read_from(&mut Cursor::new(program)) {
        Ok(candidate) => candidate,
        Err(error) => return Response::Error(CommandError::invalid_container(error)),
    };
    let (name, origin) = match edit {
        Some(edit) => (edit.name, edit.origin),
        None => (None, None),
    };
    let decisions: BTreeMap<u64, MigrationDecision> = migration
        .iter()
        .map(|(&uid, &spec)| (uid, MigrationDecision::from(spec)))
        .collect();
    ack_or_error(host.stage_with_decisions(
        candidate,
        &decisions,
        Some(AcceptedEdit {
            wire: program.to_vec(),
            name,
            origin,
        }),
    ))
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
    use ironplc_container::FieldType;
    use rstest::rstest;

    #[test]
    fn parse_command_when_identity_then_identity() {
        assert_eq!(
            parse_command(r#"{"command":"identity"}"#).unwrap(),
            Command::Identity
        );
    }

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
                program: vec![1, 2, 255],
                edit: None,
                migration: BTreeMap::new(),
            }
        );
    }

    #[test]
    fn parse_command_when_accept_edits_with_migration_then_carries_decisions() {
        let command = parse_command(
            r#"{"command":"acceptEdits","program":[1],"migration":{"1":"init","2":"preserve"}}"#,
        )
        .unwrap();

        assert_eq!(
            command,
            Command::AcceptEdits {
                program: vec![1],
                edit: None,
                migration: BTreeMap::from([
                    (1, MigrationDecisionSpec::Init),
                    (2, MigrationDecisionSpec::Preserve),
                ]),
            }
        );
    }

    #[test]
    fn parse_command_when_accept_edits_with_edit_then_carries_identity() {
        let command = parse_command(
            r#"{"command":"acceptEdits","program":[1],"edit":{"name":"rung-4","origin":"ws-9"}}"#,
        )
        .unwrap();

        assert_eq!(
            command,
            Command::AcceptEdits {
                program: vec![1],
                edit: Some(EditSpec {
                    name: Some("rung-4".into()),
                    origin: Some("ws-9".into()),
                }),
                migration: BTreeMap::new(),
            }
        );
    }

    #[test]
    fn parse_command_when_migration_value_is_unknown_then_error() {
        assert!(parse_command(
            r#"{"command":"acceptEdits","program":[1],"migration":{"1":"keep"}}"#
        )
        .is_err());
    }

    #[test]
    fn parse_command_when_migration_uid_is_not_a_number_then_error() {
        assert!(parse_command(
            r#"{"command":"acceptEdits","program":[1],"migration":{"counter":"init"}}"#
        )
        .is_err());
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

    /// The device block the wire-shape tests compose; the values mirror the
    /// design spec's example (a soft device reports its binary name and
    /// version).
    fn test_device() -> DeviceIdentity {
        DeviceIdentity {
            name: "ironplcvm".into(),
            model: "IronPLC SoftPLC".into(),
            modification: "vm-cli".into(),
            firmware_version: "0.13.0".into(),
        }
    }

    /// The identity response over one status snapshot, the `execute` shape.
    fn identity_response(redundancy: Option<RedundancyIdentity>) -> Response {
        Response::Identity(Box::new(IdentityPayload {
            protocol: SESSION_PROTOCOL_VERSION,
            device: test_device(),
            application: StatusPayload {
                mode: HostMode::Normal,
                active: 1,
                normal: 1,
                candidate: None,
                application: 1,
                migration: false,
                rounds: 0,
                pending_edit: None,
            },
            redundancy,
        }))
    }

    #[test]
    fn render_response_when_identity_then_serializes_every_block() {
        let line = render_response(&identity_response(None)).unwrap();
        let value: serde_json::Value = serde_json::from_str(&line).unwrap();

        assert_eq!(value["response"], "identity");
        assert_eq!(value["protocol"], 1);
        assert_eq!(
            value["device"],
            serde_json::json!({
                "name": "ironplcvm",
                "model": "IronPLC SoftPLC",
                "modification": "vm-cli",
                "firmwareVersion": "0.13.0",
            })
        );
        // The application block is the StatusPayload vocabulary verbatim...
        assert_eq!(value["application"]["mode"], "normal");
        assert_eq!(value["application"]["active"], 1);
        assert_eq!(value["application"]["candidate"], serde_json::Value::Null);
        assert_eq!(value["application"]["rounds"], 0);
        // ...and the standalone server answers without the redundancy block.
        assert!(value.get("redundancy").is_none());
    }

    #[test]
    fn render_response_when_identity_with_redundancy_then_block_serializes() {
        let response = identity_response(Some(RedundancyIdentity {
            pair_id: "7f3a9c".into(),
            role: "primary".into(),
            epoch: 12,
            sync: "syncReady".into(),
            control: "active".into(),
        }));

        let line = render_response(&response).unwrap();
        let value: serde_json::Value = serde_json::from_str(&line).unwrap();

        assert_eq!(
            value["redundancy"],
            serde_json::json!({
                "pairId": "7f3a9c",
                "role": "primary",
                "epoch": 12,
                "sync": "syncReady",
                "control": "active",
            })
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
            pending_edit: None,
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
    fn render_response_when_v4010_type_change_then_line_carries_the_pairs() {
        let response = Response::Error(CommandError::from(
            OnlineChangeError::MigrationUnsupported(MigrationError::TypeChangeUnsupported {
                pairs: vec![TypeChangePair {
                    uid: 7,
                    name: Some("Counter".into()),
                    from: FieldType::I32,
                    to: FieldType::U32,
                    size_equal: true,
                }],
            }),
        ));

        let line = render_response(&response).unwrap();

        assert!(line.contains(r#""vCode":"V4010""#));
        assert!(line.contains(
            r#""pairs":[{"uid":7,"name":"Counter","from":"I32","to":"U32","sizeEqual":true}]"#
        ));
    }

    #[test]
    fn command_error_pairs_when_decision_is_unknown_then_absent() {
        // Only the collected type-change refusal carries pairs; the other
        // V4010 reasons (here a stale decision UID) have nothing to decide.
        let error = CommandError::from(OnlineChangeError::MigrationUnsupported(
            MigrationError::UnknownDecisionUid { uid: 9 },
        ));

        assert_eq!(error.v_code(), "V4010");
        assert!(error.pairs().is_none());
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
    #[case::without_test(OnlineChangeError::AssembleWithoutTest, "V4017")]
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
