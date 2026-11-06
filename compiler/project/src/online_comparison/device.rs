//! What the comparison needs to know of a controller: the fields of its
//! `identity` answer, read from the wire form.
//!
//! The runtime crate defines the answer (`IdentityPayload`), and this crate does
//! not depend on the runtime, so [`DeviceFacts`] is read from the JSON of the
//! answer by the private wire types below. They name only the fields the
//! comparison reads and ignore the rest, as every reader of the payload does
//! (ADR-0083: a client ignores the fields it does not know). A field that is
//! present and not in its form is an error, never a default: a hash that is not
//! 64 hexadecimal digits means the answer is not an identity.
//!
//! The editor passes the JSON it received to `ironplcc online-compare`, and the
//! MCP server serializes the payload it received, so the wire form is the one
//! input of both.

use std::fmt;

use ironplc_container::integrity::{self, HASH_LEN};
use ironplc_discovery::DeviceId;
use serde::{Deserialize, Serialize};
use serde_json::Value;

/// Whether the controller executes scan rounds (`runState` of the identity).
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub enum RunState {
    /// The controller scans. A controller that predates the field scans.
    #[default]
    Running,
    /// The controller executes no round until it is started.
    Stopped,
}

/// Where the unfinished edit of the controller stands (`edit.stage`).
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub enum EditStage {
    /// No candidate is staged.
    Idle,
    /// A candidate is staged and the normal artifact executes.
    Accepted,
    /// The candidate executes and can be reverted.
    Testing,
    /// The candidate of a state migration executes and cannot be reverted.
    TestingMigration,
    /// The candidate of a replace (a download) executes and cannot be reverted.
    TestingReplace,
    /// A test or an untest waits for the next scan boundary.
    SwapPending,
}

/// A refusal or a reason in the words of the controller or of the comparison: a
/// stable code and its plain text.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
pub struct Reason {
    /// The V-code of the controller's refusal, or the identifier of the
    /// comparison's own reason.
    pub code: String,
    /// The text.
    pub text: String,
}

/// The unfinished edit of the controller and whether it accepts another.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct DeviceEdit {
    /// Where the edit stands.
    pub stage: EditStage,
    /// Why the controller does not accept a candidate now, from the `accept`
    /// entry of its edit view; `None` when it does. Download and online change
    /// both begin with an accept, so one answer serves both.
    pub accept_refusal: Option<Reason>,
}

/// The trap that faulted the controller.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct Fault {
    /// The V-code of the trap.
    pub v_code: String,
    /// What happened.
    pub message: String,
}

/// An unfinished edit that a restart lost.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct CandidateLost {
    /// The edit label of the accept that staged the candidate, when it named one.
    pub name: Option<String>,
    /// How far the edit had come, as the controller wrote it.
    pub stage: Option<EditStage>,
}

/// What the controller did with its stored retained image at boot.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct BootNote {
    /// `applied`, `absent` or `discarded`.
    pub result: String,
    /// Why it was discarded, when it was.
    pub reason: Option<String>,
}

/// What the comparison reads of a controller. A field is `None` when the
/// controller does not report it (it predates the field, or the host has none).
#[derive(Clone, Debug, Default, PartialEq, Eq)]
pub struct DeviceFacts {
    /// The id of the device, which keys the download record.
    pub device_id: Option<DeviceId>,
    /// The runtime version.
    pub runtime_version: Option<String>,
    /// The `contentHash` of the application it runs: the only thing compared.
    pub content_hash: Option<[u8; HASH_LEN]>,
    /// The `layoutHash` of the application it runs.
    pub layout_hash: Option<[u8; HASH_LEN]>,
    /// The `contentHash` of the application it would boot.
    pub persisted_content_hash: Option<[u8; HASH_LEN]>,
    /// Whether it scans.
    pub run_state: RunState,
    /// The fault, present exactly while it is faulted.
    pub fault: Option<Fault>,
    /// The container format version it accepts.
    pub container_format: Option<u16>,
    /// The edit view, when it reports one.
    pub edit: Option<DeviceEdit>,
    /// What it did with its retained image at boot.
    pub retained: Option<BootNote>,
    /// The unfinished edit that its last run left.
    pub candidate_lost: Option<CandidateLost>,
}

/// Why an identity answer cannot be read.
#[derive(Debug, PartialEq, Eq)]
pub struct IdentityError(String);

impl fmt::Display for IdentityError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "the identity of the controller cannot be read: {}",
            self.0
        )
    }
}

impl std::error::Error for IdentityError {}

impl DeviceFacts {
    /// Reads the answer to `identity` (an `IdentityPayload` as JSON).
    pub fn from_identity(identity: &Value) -> Result<DeviceFacts, IdentityError> {
        let wire: WireIdentity = serde_json::from_value(identity.clone())
            .map_err(|err| IdentityError(err.to_string()))?;
        wire.try_into()
    }
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct WireIdentity {
    device: WireDevice,
    application: WireApplication,
    #[serde(default)]
    persisted_content_hash: Option<String>,
    #[serde(default)]
    retained: Option<WireBootNote>,
    #[serde(default)]
    candidate_lost: Option<WireCandidateLost>,
    #[serde(default)]
    fault: Option<WireFault>,
    #[serde(default)]
    run_state: RunState,
    #[serde(default)]
    container_format: Option<u16>,
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct WireDevice {
    #[serde(default)]
    device_id: Option<String>,
    #[serde(default)]
    runtime_version: Option<String>,
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct WireApplication {
    #[serde(default)]
    content_hash: Option<String>,
    #[serde(default)]
    layout_hash: Option<String>,
    #[serde(default)]
    edit: Option<WireEdit>,
}

#[derive(Deserialize)]
struct WireEdit {
    stage: EditStage,
    commands: WireCommands,
}

#[derive(Deserialize)]
struct WireCommands {
    accept: WireCommand,
}

#[derive(Deserialize)]
struct WireCommand {
    enabled: bool,
    #[serde(default)]
    reason: Option<Reason>,
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct WireFault {
    v_code: String,
    message: String,
}

#[derive(Deserialize)]
struct WireBootNote {
    result: String,
    #[serde(default)]
    reason: Option<String>,
}

#[derive(Deserialize)]
struct WireCandidateLost {
    #[serde(default)]
    name: Option<String>,
    #[serde(default)]
    stage: Option<EditStage>,
}

impl TryFrom<WireIdentity> for DeviceFacts {
    type Error = IdentityError;

    fn try_from(wire: WireIdentity) -> Result<Self, IdentityError> {
        let edit = wire.application.edit.map(edit_of).transpose()?;
        let device_id = wire
            .device
            .device_id
            .map(|text| {
                DeviceId::parse(&text).map_err(|err| {
                    IdentityError(format!("device.deviceId is not a device id: {err}"))
                })
            })
            .transpose()?;
        Ok(DeviceFacts {
            device_id,
            runtime_version: wire.device.runtime_version,
            content_hash: hash("application.contentHash", wire.application.content_hash)?,
            layout_hash: hash("application.layoutHash", wire.application.layout_hash)?,
            persisted_content_hash: hash("persistedContentHash", wire.persisted_content_hash)?,
            run_state: wire.run_state,
            fault: wire.fault.map(|fault| Fault {
                v_code: fault.v_code,
                message: fault.message,
            }),
            container_format: wire.container_format,
            edit,
            retained: wire.retained.map(|note| BootNote {
                result: note.result,
                reason: note.reason,
            }),
            candidate_lost: wire.candidate_lost.map(|lost| CandidateLost {
                name: lost.name,
                stage: lost.stage,
            }),
        })
    }
}

/// The edit of the wire. A command that is not enabled carries its reason (the
/// view always writes one); an answer without it is not an edit view.
fn edit_of(edit: WireEdit) -> Result<DeviceEdit, IdentityError> {
    let accept = edit.commands.accept;
    let accept_refusal = match (accept.enabled, accept.reason) {
        (true, _) => None,
        (false, Some(reason)) => Some(reason),
        (false, None) => {
            return Err(IdentityError(
                "application.edit.commands.accept is not enabled and gives no reason".to_owned(),
            ))
        }
    };
    Ok(DeviceEdit {
        stage: edit.stage,
        accept_refusal,
    })
}

fn hash(field: &str, text: Option<String>) -> Result<Option<[u8; HASH_LEN]>, IdentityError> {
    text.map(|text| {
        integrity::hash_from_hex(&text)
            .ok_or_else(|| IdentityError(format!("{field} is not 64 hexadecimal digits")))
    })
    .transpose()
}
