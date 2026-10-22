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
    assert!(
        parse_command(r#"{"command":"acceptEdits","program":[1],"migration":{"1":"keep"}}"#)
            .is_err()
    );
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
        device_id: None,
        name: "ironplcvm".into(),
        model: "IronPLC SoftPLC".into(),
        modification: "vm-cli".into(),
        firmware_version: "0.13.0".into(),
    }
}

/// The identity response over one status snapshot, the `execute` shape.
fn identity_response(redundancy: Option<RedundancyIdentity>) -> Response {
    identity_response_of(test_device(), redundancy)
}

/// [`identity_response`] for a device block of the test's choosing.
fn identity_response_of(
    device: DeviceIdentity,
    redundancy: Option<RedundancyIdentity>,
) -> Response {
    Response::Identity(Box::new(IdentityPayload {
        protocol: SESSION_PROTOCOL_VERSION,
        device,
        application: StatusPayload {
            mode: HostMode::Normal,
            active: 1,
            normal: 1,
            candidate: None,
            application: 1,
            migration: false,
            rounds: 0,
            pending_edit: None,
            content_hash: None,
            layout_hash: None,
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

/// A device id as the device generates it: 32 lowercase hexadecimal digits.
const DEVICE_ID: &str = "0123456789abcdef0123456789abcdef";

#[test]
fn render_response_when_identity_without_device_id_then_field_is_absent() {
    let line = render_response(&identity_response(None)).unwrap();

    assert!(!line.contains("deviceId"));
}

#[test]
fn render_response_when_identity_with_device_id_then_field_serializes() {
    let mut device = test_device();
    device.device_id = Some(DEVICE_ID.into());
    let line = render_response(&identity_response_of(device, None)).unwrap();
    let value: serde_json::Value = serde_json::from_str(&line).unwrap();

    assert_eq!(value["device"]["deviceId"], DEVICE_ID);
    assert_eq!(value["device"]["name"], "ironplcvm");
}

/// The `device` block of a server that predates `deviceId`.
const DEVICE_WITHOUT_ID: &str = r#"{"name":"ironplcvm","model":"IronPLC SoftPLC","modification":"vm-cli","firmwareVersion":"0.13.0"}"#;

#[test]
fn device_identity_when_device_id_absent_then_parses_with_none() {
    let device: DeviceIdentity = serde_json::from_str(DEVICE_WITHOUT_ID).unwrap();

    assert_eq!(device, test_device());
}

#[test]
fn device_identity_when_device_id_and_unknown_field_present_then_parses() {
    // A reader ignores fields it does not know, which is what keeps the
    // session protocol at version 1 when a server adds one.
    let line = DEVICE_WITHOUT_ID.replace(
        r#""name":"#,
        &format!(r#""deviceId":"{DEVICE_ID}","fieldOfALaterServer":[1],"name":"#),
    );

    let device: DeviceIdentity = serde_json::from_str(&line).unwrap();

    assert_eq!(device.device_id.as_deref(), Some(DEVICE_ID));
    assert_eq!(device.name, "ironplcvm");
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
fn parse_command_when_set_device_settings_then_carries_the_named_settings() {
    let command =
        parse_command(r#"{"command":"setDeviceSettings","name":"cell 1","deviceId":"ab"}"#)
            .unwrap();

    assert_eq!(
        command,
        Command::SetDeviceSettings {
            settings: BTreeMap::from([
                ("deviceId".to_string(), serde_json::json!("ab")),
                ("name".to_string(), serde_json::json!("cell 1")),
            ]),
        }
    );
}

#[test]
fn parse_command_when_set_device_settings_names_nothing_then_empty_settings() {
    assert_eq!(
        parse_command(r#"{"command":"setDeviceSettings"}"#).unwrap(),
        Command::SetDeviceSettings {
            settings: BTreeMap::new(),
        }
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
        content_hash: None,
        layout_hash: None,
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

/// The `getStatus` payload of a device that predates the hashes.
const STATUS_WITHOUT_HASHES: &str = r#"{"mode":"normal","active":1,"normal":1,"candidate":null,"application":1,"migration":false,"rounds":0}"#;

#[test]
fn status_payload_when_hashes_absent_then_parses_with_none() {
    let status: StatusPayload = serde_json::from_str(STATUS_WITHOUT_HASHES).unwrap();

    assert_eq!(status.content_hash, None);
    assert_eq!(status.layout_hash, None);
}

#[test]
fn status_payload_when_hashes_and_unknown_field_present_then_parses() {
    // A reader ignores fields it does not know, which is what keeps the
    // session protocol at version 1 when a server adds one.
    let line = STATUS_WITHOUT_HASHES.replace(
        r#""rounds":0"#,
        r#""rounds":0,"contentHash":"ab","layoutHash":"cd","fieldOfALaterServer":[1]"#,
    );

    let status: StatusPayload = serde_json::from_str(&line).unwrap();

    assert_eq!(status.content_hash.as_deref(), Some("ab"));
    assert_eq!(status.layout_hash.as_deref(), Some("cd"));
}

#[test]
fn render_response_when_status_without_hashes_then_fields_are_absent() {
    let status: StatusPayload = serde_json::from_str(STATUS_WITHOUT_HASHES).unwrap();

    let line = render_response(&Response::Status(status)).unwrap();

    assert!(!line.contains("contentHash"));
    assert!(!line.contains("layoutHash"));
}

#[test]
fn wire_hash_when_no_hash_then_none() {
    assert_eq!(wire_hash(&NO_HASH), None);
}

#[test]
fn wire_hash_when_hash_then_lowercase_hex() {
    let mut hash = [0u8; HASH_LEN];
    hash[0] = 0xAB;

    let text = wire_hash(&hash).unwrap();

    assert_eq!(text.len(), 64);
    assert!(text.starts_with("ab00"));
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
    let response = Response::Error(CommandError::from(OnlineChangeError::MigrationUnsupported(
        MigrationError::TypeChangeUnsupported {
            pairs: vec![TypeChangePair {
                uid: 7,
                name: Some("Counter".into()),
                from: FieldType::I32,
                to: FieldType::U32,
                size_equal: true,
            }],
        },
    )));

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
#[case::unidentified(
    OnlineChangeError::MigrationUnsupported(MigrationError::UnidentifiedVariables {
        variables: Vec::new()
    }),
    "V4020"
)]
#[case::untest(OnlineChangeError::UntestUnsupported, "V4011")]
#[case::none_staged(OnlineChangeError::NoCandidateStaged, "V4012")]
#[case::already_staged(OnlineChangeError::CandidateAlreadyStaged, "V4013")]
#[case::no_test(OnlineChangeError::NoTestInProgress, "V4014")]
#[case::wrong_mode(OnlineChangeError::NotAllowedInThisMode, "V4015")]
#[case::without_test(OnlineChangeError::AssembleWithoutTest, "V4017")]
#[case::snapshot_corrupt(OnlineChangeError::SnapshotCorrupt, "V4019")]
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
