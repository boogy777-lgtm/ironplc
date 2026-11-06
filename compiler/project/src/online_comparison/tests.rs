//! Tests of the go-online comparison: one case for each row of the table of
//! ADR-0089, the states the table adds, the rule that connect never refuses,
//! and the reading of the identity.

use ironplc_container::{EMPTY_APPLICATION_HASH, FORMAT_VERSION};
use ironplc_discovery::DeviceId;
use rstest::rstest;
use serde_json::json;

use super::*;
use crate::build_record::ContainerIdentity;

const HASH_P: [u8; 32] = [0x11; 32];
const HASH_LP: [u8; 32] = [0x12; 32];
const HASH_R: [u8; 32] = [0x21; 32];
const HASH_LR: [u8; 32] = [0x22; 32];
const HASH_OTHER: [u8; 32] = [0x31; 32];
const HASH_LOTHER: [u8; 32] = [0x32; 32];

fn id(n: u8) -> DeviceId {
    DeviceId::parse(&format!("{n:02x}").repeat(16)).unwrap()
}

fn built(content: [u8; 32], layout: [u8; 32]) -> BuiltApplication {
    BuiltApplication {
        name: "Main".to_owned(),
        content_hash: content,
        layout_hash: layout,
    }
}

fn project_built() -> BuiltApplication {
    built(HASH_P, HASH_LP)
}

/// A controller that runs `content` with `layout` and has nothing to report.
fn holding(content: [u8; 32], layout: [u8; 32]) -> DeviceFacts {
    DeviceFacts {
        device_id: Some(id(1)),
        runtime_version: Some("0.247.0".to_owned()),
        content_hash: Some(content),
        layout_hash: Some(layout),
        persisted_content_hash: Some(content),
        run_state: RunState::Running,
        fault: None,
        container_format: Some(FORMAT_VERSION),
        edit: Some(DeviceEdit {
            stage: EditStage::Idle,
            accept_refusal: None,
        }),
        retained: None,
        candidate_lost: None,
    }
}

fn download_of(device: u8, content: [u8; 32], layout: [u8; 32]) -> DownloadRecord {
    DownloadRecord {
        device_id: id(device),
        application: "Main".to_owned(),
        container: ContainerIdentity {
            content_hash: content,
            layout_hash: layout,
            debug_hash: [0; 32],
            size: 100,
        },
        device: None,
        time: "2026-11-07T10:00:00Z".to_owned(),
    }
}

fn path_of(device: u8) -> PathRecord {
    PathRecord {
        device_id: id(device),
        route_hint: None,
        chosen_at: 1,
    }
}

/// How the attempt went, without the device.
#[derive(Clone, Copy, Debug)]
enum Went {
    Ok,
    Unreachable,
    Busy,
    Protected,
    ConnectionLost,
}

/// One situation and what the comparison must say of it.
#[derive(Debug)]
struct Case {
    went: Went,
    device: DeviceFacts,
    download: Option<DownloadRecord>,
    kept_container: bool,
    path: Option<PathRecord>,
    project: ProjectState,
    row: Row,
    notes: Vec<NoteKind>,
    enabled: Vec<ActionKind>,
}

impl Case {
    fn run(&self) -> Comparison {
        let attempt = match self.went {
            Went::Ok => Attempt::Ok(&self.device),
            Went::Unreachable => Attempt::Unreachable,
            Went::Busy => Attempt::Busy,
            Went::Protected => Attempt::Protected,
            Went::ConnectionLost => Attempt::ConnectionLost,
        };
        let records = Records {
            download: self.download.as_ref().map(|record| LastDownload {
                record,
                kept_container: self.kept_container,
            }),
            path: self.path.as_ref(),
        };
        compare(&attempt, &records, &self.project)
    }
}

/// The actions that are enabled, as the table lists them.
fn enabled(comparison: &Comparison) -> Vec<ActionKind> {
    comparison
        .actions
        .iter()
        .filter(|offer| offer.enabled)
        .map(|offer| offer.action)
        .collect()
}

fn notes(comparison: &Comparison) -> Vec<NoteKind> {
    comparison.information.iter().map(|row| row.kind).collect()
}

use ActionKind::{ChooseAnother, Download, Monitor, OnlineChange, Resolve, Retry, Search};

/// The case of a controller that answered, with the project as the case says.
fn answered(
    device: DeviceFacts,
    project: ProjectState,
    row: Row,
    enabled: Vec<ActionKind>,
) -> Case {
    Case {
        went: Went::Ok,
        device,
        download: None,
        kept_container: false,
        path: None,
        project,
        row,
        notes: vec![],
        enabled,
    }
}

/// The case of an attempt that did not reach the controller.
fn missed(went: Went, row: Row, enabled: Vec<ActionKind>) -> Case {
    Case {
        went,
        device: DeviceFacts::default(),
        download: None,
        kept_container: false,
        path: None,
        project: ProjectState::UpToDate(project_built()),
        row,
        notes: vec![],
        enabled,
    }
}

fn row_01_unreachable() -> Case {
    missed(
        Went::Unreachable,
        Row::Unreachable,
        vec![Search, ChooseAnother, Retry],
    )
}

fn row_02_busy() -> Case {
    missed(Went::Busy, Row::Busy, vec![Search, ChooseAnother, Retry])
}

fn row_03_protected() -> Case {
    missed(Went::Protected, Row::Protected, vec![Search, ChooseAnother])
}

fn row_04_no_application() -> Case {
    answered(
        holding(EMPTY_APPLICATION_HASH, HASH_LP),
        ProjectState::UpToDate(project_built()),
        Row::NoApplication,
        vec![Monitor, Download, Search, ChooseAnother],
    )
}

fn row_05_up_to_date() -> Case {
    answered(
        holding(HASH_P, HASH_LP),
        ProjectState::UpToDate(project_built()),
        Row::UpToDate,
        vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
    )
}

fn row_06_sources_changed() -> Case {
    answered(
        holding(HASH_P, HASH_LP),
        ProjectState::ChangedSinceBuild(project_built()),
        Row::SourcesChanged,
        vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
    )
}

fn row_07_previous_download() -> Case {
    Case {
        download: Some(download_of(1, HASH_R, HASH_LP)),
        ..answered(
            holding(HASH_R, HASH_LP),
            ProjectState::UpToDate(project_built()),
            Row::PreviousDownload,
            vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
        )
    }
}

fn row_08_previous_download_other_layout() -> Case {
    Case {
        download: Some(download_of(1, HASH_R, HASH_LR)),
        ..answered(
            holding(HASH_R, HASH_LR),
            ProjectState::UpToDate(project_built()),
            Row::PreviousDownloadOtherLayout,
            vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
        )
    }
}

fn row_09_other_application() -> Case {
    Case {
        download: Some(download_of(1, HASH_R, HASH_LR)),
        ..answered(
            holding(HASH_OTHER, HASH_LOTHER),
            ProjectState::UpToDate(project_built()),
            Row::OtherApplication,
            vec![Monitor, Download, Search, ChooseAnother],
        )
    }
}

fn row_10_edit_in_progress() -> Case {
    let mut device = holding(HASH_P, HASH_LP);
    device.edit = Some(DeviceEdit {
        stage: EditStage::Testing,
        accept_refusal: Some(Reason {
            code: "V4004".to_owned(),
            text: "a candidate is already staged".to_owned(),
        }),
    });
    Case {
        notes: vec![NoteKind::EditInProgress],
        ..answered(
            device,
            ProjectState::UpToDate(project_built()),
            Row::UpToDate,
            vec![Monitor, Search, ChooseAnother, Resolve],
        )
    }
}

fn row_11_build_fails() -> Case {
    Case {
        notes: vec![NoteKind::BuildFails],
        ..answered(
            holding(HASH_P, HASH_LP),
            ProjectState::BuildFails(Some(project_built())),
            Row::SourcesChanged,
            vec![Monitor, Search, ChooseAnother],
        )
    }
}

fn row_12_other_controller() -> Case {
    Case {
        path: Some(path_of(9)),
        notes: vec![NoteKind::OtherController],
        ..answered(
            holding(HASH_P, HASH_LP),
            ProjectState::UpToDate(project_built()),
            Row::UpToDate,
            vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
        )
    }
}

fn row_13_connection_lost() -> Case {
    missed(
        Went::ConnectionLost,
        Row::ConnectionLost,
        vec![Search, ChooseAnother, Retry],
    )
}

fn row_14_restarted_running_code_not_persisted() -> Case {
    let mut device = holding(HASH_P, HASH_LP);
    device.persisted_content_hash = Some(HASH_OTHER);
    Case {
        notes: vec![NoteKind::NotPersisted],
        ..answered(
            device,
            ProjectState::UpToDate(project_built()),
            Row::UpToDate,
            vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
        )
    }
}

fn row_14_restarted_candidate_lost() -> Case {
    let mut device = holding(HASH_P, HASH_LP);
    device.candidate_lost = Some(CandidateLost {
        name: Some("tune".to_owned()),
        stage: Some(EditStage::Testing),
    });
    Case {
        notes: vec![NoteKind::CandidateLost],
        ..answered(
            device,
            ProjectState::UpToDate(project_built()),
            Row::UpToDate,
            vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
        )
    }
}

fn row_14_restarted_retained_discarded() -> Case {
    let mut device = holding(HASH_P, HASH_LP);
    device.retained = Some(BootNote {
        result: "discarded".to_owned(),
        reason: Some("otherApplication".to_owned()),
    });
    Case {
        notes: vec![NoteKind::RetainedDiscarded],
        ..answered(
            device,
            ProjectState::UpToDate(project_built()),
            Row::UpToDate,
            vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
        )
    }
}

fn row_15_container_format_refused() -> Case {
    let mut device = holding(HASH_P, HASH_LP);
    device.container_format = Some(FORMAT_VERSION + 1);
    Case {
        notes: vec![NoteKind::ContainerFormatRefused],
        ..answered(
            device,
            ProjectState::UpToDate(project_built()),
            Row::UpToDate,
            vec![Monitor, Search, ChooseAnother],
        )
    }
}

fn extra_stopped() -> Case {
    let mut device = holding(HASH_P, HASH_LP);
    device.run_state = RunState::Stopped;
    Case {
        notes: vec![NoteKind::Stopped],
        ..answered(
            device,
            ProjectState::UpToDate(project_built()),
            Row::UpToDate,
            vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
        )
    }
}

fn extra_faulted() -> Case {
    let mut device = holding(HASH_P, HASH_LP);
    device.fault = Some(Fault {
        v_code: "V4001".to_owned(),
        message: "divide by zero".to_owned(),
    });
    Case {
        notes: vec![NoteKind::Faulted],
        ..answered(
            device,
            ProjectState::UpToDate(project_built()),
            Row::UpToDate,
            vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
        )
    }
}

fn extra_content_not_reported() -> Case {
    let mut device = holding(HASH_P, HASH_LP);
    device.content_hash = None;
    device.layout_hash = None;
    device.persisted_content_hash = None;
    Case {
        notes: vec![NoteKind::ContentNotReported],
        ..answered(
            device,
            ProjectState::UpToDate(project_built()),
            Row::OtherApplication,
            vec![Monitor, Download, Search, ChooseAnother],
        )
    }
}

fn extra_no_device_id() -> Case {
    let mut device = holding(HASH_P, HASH_LP);
    device.device_id = None;
    Case {
        // Without an id no record can be named, so the download record of the
        // case is not this device's and is not read.
        download: Some(download_of(1, HASH_R, HASH_LP)),
        notes: vec![NoteKind::NoDeviceId],
        ..answered(
            device,
            ProjectState::UpToDate(project_built()),
            Row::UpToDate,
            vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
        )
    }
}

fn extra_never_built_empty_controller() -> Case {
    answered(
        holding(EMPTY_APPLICATION_HASH, HASH_LP),
        ProjectState::NeverBuilt,
        Row::NoApplication,
        vec![Monitor, Download, Search, ChooseAnother],
    )
}

fn extra_never_built_previous_download() -> Case {
    Case {
        download: Some(download_of(1, HASH_R, HASH_LR)),
        ..answered(
            holding(HASH_R, HASH_LR),
            ProjectState::NeverBuilt,
            Row::PreviousDownload,
            vec![Monitor, OnlineChange, Download, Search, ChooseAnother],
        )
    }
}

fn extra_build_fails_without_a_build() -> Case {
    Case {
        notes: vec![NoteKind::BuildFails],
        ..answered(
            holding(HASH_OTHER, HASH_LOTHER),
            ProjectState::BuildFails(None),
            Row::OtherApplication,
            vec![Monitor, Search, ChooseAnother],
        )
    }
}

fn extra_record_of_another_device_is_ignored() -> Case {
    Case {
        download: Some(download_of(7, HASH_R, HASH_LP)),
        ..answered(
            holding(HASH_R, HASH_LP),
            ProjectState::UpToDate(project_built()),
            Row::OtherApplication,
            vec![Monitor, Download, Search, ChooseAnother],
        )
    }
}

#[rstest]
#[case::row_01_unreachable(row_01_unreachable())]
#[case::row_02_busy(row_02_busy())]
#[case::row_03_protected(row_03_protected())]
#[case::row_04_no_application(row_04_no_application())]
#[case::row_05_up_to_date(row_05_up_to_date())]
#[case::row_06_sources_changed(row_06_sources_changed())]
#[case::row_07_previous_download(row_07_previous_download())]
#[case::row_08_previous_download_other_layout(row_08_previous_download_other_layout())]
#[case::row_09_other_application(row_09_other_application())]
#[case::row_10_edit_in_progress(row_10_edit_in_progress())]
#[case::row_11_build_fails(row_11_build_fails())]
#[case::row_12_other_controller(row_12_other_controller())]
#[case::row_13_connection_lost(row_13_connection_lost())]
#[case::row_14_restarted_running_code_not_persisted(row_14_restarted_running_code_not_persisted())]
#[case::row_14_restarted_candidate_lost(row_14_restarted_candidate_lost())]
#[case::row_14_restarted_retained_discarded(row_14_restarted_retained_discarded())]
#[case::row_15_container_format_refused(row_15_container_format_refused())]
#[case::extra_stopped(extra_stopped())]
#[case::extra_faulted(extra_faulted())]
#[case::extra_content_not_reported(extra_content_not_reported())]
#[case::extra_no_device_id(extra_no_device_id())]
#[case::extra_never_built_empty_controller(extra_never_built_empty_controller())]
#[case::extra_never_built_previous_download(extra_never_built_previous_download())]
#[case::extra_build_fails_without_a_build(extra_build_fails_without_a_build())]
#[case::extra_record_of_another_device_is_ignored(extra_record_of_another_device_is_ignored())]
fn compare_when_situation_then_the_row_the_information_and_the_enabled_actions(#[case] case: Case) {
    let comparison = case.run();

    assert_eq!(comparison.row, case.row);
    assert_eq!(notes(&comparison), case.notes);
    assert_eq!(enabled(&comparison), case.enabled);
    // The Build of an online change is the answer of the action, not a second rule.
    assert_eq!(
        comparison.hot_edit.allowed,
        case.enabled.contains(&OnlineChange)
    );
    assert_eq!(comparison.connected, matches!(case.went, Went::Ok));
}

#[test]
fn compare_when_every_action_then_each_is_listed_once_and_a_refused_one_has_its_reason() {
    let comparison = row_09_other_application().run();

    let listed: Vec<ActionKind> = comparison
        .actions
        .iter()
        .map(|offer| offer.action)
        .collect();
    assert_eq!(listed, ActionKind::ALL);
    let refused_without_reason = comparison
        .actions
        .iter()
        .filter(|offer| !offer.enabled && offer.reason.is_none())
        .count();
    assert_eq!(refused_without_reason, 0);
    let enabled_with_reason = comparison
        .actions
        .iter()
        .filter(|offer| offer.enabled && offer.reason.is_some())
        .count();
    assert_eq!(enabled_with_reason, 0);
}

#[test]
fn compare_when_upload_then_it_is_listed_and_not_available() {
    let comparison = row_05_up_to_date().run();

    let upload = comparison
        .actions
        .iter()
        .find(|offer| offer.action == ActionKind::Upload)
        .unwrap();
    assert!(!upload.enabled);
    assert_eq!(upload.reason.as_ref().unwrap().code, "notAvailable");
}

#[test]
fn compare_when_edit_in_progress_then_the_reason_is_the_controllers_own() {
    let comparison = row_10_edit_in_progress().run();

    let reason = comparison.hot_edit.reason.unwrap();
    assert_eq!(reason.code, "V4004");
    assert_eq!(reason.text, "a candidate is already staged");
    assert!(!comparison.hot_edit.allowed);
}

#[test]
fn compare_when_no_application_then_the_controller_holds_none_and_only_download_changes_it() {
    let comparison = row_04_no_application().run();

    assert_eq!(
        comparison.controller.as_ref().unwrap().holds,
        Holds::NoApplication
    );
    let changing: Vec<ActionKind> = enabled(&comparison)
        .into_iter()
        .filter(|kind| matches!(kind, OnlineChange | Download))
        .collect();
    assert_eq!(changing, vec![Download]);
    assert_eq!(
        comparison.hot_edit.reason.unwrap().code,
        "noApplicationToChange"
    );
}

#[test]
fn compare_when_up_to_date_then_the_editor_goes_online_at_once() {
    assert!(row_05_up_to_date().run().at_once);
    assert!(!row_06_sources_changed().run().at_once);
    assert!(!row_04_no_application().run().at_once);
}

#[test]
fn compare_when_other_controller_then_the_row_names_the_one_last_chosen_and_nothing_is_refused() {
    let comparison = row_12_other_controller().run();

    let row = &comparison.information[0];
    assert!(row.text.contains(&id(9).to_string()), "{}", row.text);
    assert!(comparison.hot_edit.allowed);
    assert_eq!(comparison.row, Row::UpToDate);
}

#[test]
fn compare_when_path_record_is_this_controller_then_no_other_controller_row() {
    let case = Case {
        path: Some(path_of(1)),
        ..row_05_up_to_date()
    };

    assert_eq!(notes(&case.run()), vec![]);
}

#[test]
fn compare_when_previous_download_with_kept_container_then_monitor_is_at_application_level() {
    let kept = Case {
        kept_container: true,
        ..row_07_previous_download()
    };
    let not_kept = row_07_previous_download();

    let detail = |case: &Case| case.run().actions[0].detail.clone().unwrap();
    assert!(detail(&kept).contains("by name"));
    assert!(detail(&not_kept).contains("Device level only"));
}

#[test]
fn compare_when_other_application_then_monitor_is_at_device_level() {
    let comparison = row_09_other_application().run();

    assert!(comparison.actions[0]
        .detail
        .as_ref()
        .unwrap()
        .contains("Device level only"));
}

#[test]
fn compare_when_layout_differs_then_online_change_says_it_migrates() {
    let comparison = row_08_previous_download_other_layout().run();

    let change = &comparison.actions[1];
    assert_eq!(change.action, OnlineChange);
    assert!(change.detail.as_ref().unwrap().contains("migrates"));
}

#[test]
fn compare_when_generation_counters_differ_then_the_comparison_is_the_same() {
    let identity = |counter: u32| {
        json!({
            "protocol": 2,
            "device": {"deviceId": "01".repeat(16), "name": "ironplcvm", "model": "m",
                       "modification": "x", "runtimeVersion": "0.247.0"},
            "application": {
                "mode": "normal", "active": counter, "normal": counter,
                "candidate": null, "application": counter, "migration": false,
                "rounds": counter,
                "contentHash": "11".repeat(32), "layoutHash": "12".repeat(32)
            }
        })
    };

    let before = DeviceFacts::from_identity(&identity(2)).unwrap();
    let after_reboot = DeviceFacts::from_identity(&identity(1)).unwrap();

    assert_eq!(before, after_reboot);
}

// "Connect never refuses because the controller is a different one": whatever
// the controller holds and whatever the records and the project say, an attempt
// that reached the controller connects and can be monitored.
#[rstest]
fn compare_when_the_attempt_reached_the_controller_then_it_connects_and_monitor_is_enabled(
    #[values(
        Some(HASH_P),
        Some(HASH_R),
        Some(HASH_OTHER),
        Some(EMPTY_APPLICATION_HASH),
        None
    )]
    held: Option<[u8; 32]>,
    #[values(
        ProjectState::NeverBuilt,
        ProjectState::UpToDate(project_built()),
        ProjectState::ChangedSinceBuild(project_built()),
        ProjectState::BuildFails(None),
        ProjectState::BuildFails(Some(project_built()))
    )]
    project: ProjectState,
    #[values(true, false)] has_download: bool,
    #[values(EditStage::Idle, EditStage::Testing)] stage: EditStage,
    #[values(true, false)] format_refused: bool,
) {
    let mut device = holding(held.unwrap_or(HASH_OTHER), HASH_LP);
    device.content_hash = held;
    device.container_format = Some(FORMAT_VERSION + u16::from(format_refused));
    device.edit = Some(DeviceEdit {
        stage,
        accept_refusal: (stage != EditStage::Idle).then(|| Reason {
            code: "V4004".to_owned(),
            text: "staged".to_owned(),
        }),
    });
    let download = download_of(1, HASH_R, HASH_LP);
    let path = path_of(9);
    let records = Records {
        download: has_download.then_some(LastDownload {
            record: &download,
            kept_container: false,
        }),
        path: Some(&path),
    };

    let comparison = compare(&Attempt::Ok(&device), &records, &project);

    assert!(comparison.connected);
    assert!(comparison.controller.is_some());
    assert!(comparison.actions[0].enabled, "Monitor is the first action");
    assert_eq!(comparison.actions[0].action, Monitor);
}

#[rstest]
fn compare_when_the_attempt_did_not_reach_the_controller_then_nothing_is_sent(
    #[values(Went::Unreachable, Went::Busy, Went::Protected, Went::ConnectionLost)] went: Went,
) {
    let case = missed(went, Row::Unreachable, vec![]);

    let comparison = case.run();

    assert!(!comparison.connected);
    assert!(comparison.controller.is_none());
    assert!(!comparison.hot_edit.allowed);
    let sending: Vec<ActionKind> = enabled(&comparison)
        .into_iter()
        .filter(|kind| matches!(kind, Monitor | OnlineChange | Download | Resolve))
        .collect();
    assert_eq!(sending, vec![]);
}

#[test]
fn compare_when_serialized_then_the_fields_are_camel_case_and_the_row_is_a_name_and_a_number() {
    let comparison = row_05_up_to_date().run();

    let value = serde_json::to_value(&comparison).unwrap();

    assert_eq!(value["row"], "upToDate");
    assert_eq!(value["rowNumber"], 5);
    assert_eq!(value["atOnce"], true);
    assert_eq!(value["hotEdit"], json!({"allowed": true, "reason": null}));
    assert_eq!(value["controller"]["holds"]["kind"], "application");
    assert_eq!(value["controller"]["holds"]["contentHash"], "11".repeat(32));
    assert_eq!(value["project"]["status"], "upToDate");
    assert_eq!(value["actions"][1]["action"], "onlineChange");
    assert_eq!(value["actions"][1]["enabled"], true);
}

#[test]
fn compare_when_every_row_then_the_numbers_are_distinct_and_in_the_table() {
    let rows = [
        Row::Unreachable,
        Row::Busy,
        Row::Protected,
        Row::NoApplication,
        Row::UpToDate,
        Row::SourcesChanged,
        Row::PreviousDownload,
        Row::PreviousDownloadOtherLayout,
        Row::OtherApplication,
        Row::ConnectionLost,
    ];

    let numbers: Vec<u8> = rows.iter().map(|row| row.number()).collect();

    assert_eq!(numbers, vec![1, 2, 3, 4, 5, 6, 7, 8, 9, 13]);
}

fn identity_json() -> serde_json::Value {
    json!({
        "protocol": 2,
        "device": {"deviceId": "ab".repeat(16), "name": "ironplcvm", "model": "IronPLC SoftPLC",
                   "modification": "vm-cli", "runtimeVersion": "0.247.0"},
        "application": {
            "mode": "normal", "active": 1, "normal": 1, "candidate": null,
            "application": 1, "migration": false, "rounds": 10,
            "contentHash": "11".repeat(32), "layoutHash": "12".repeat(32),
            "edit": {
                "stage": "idle",
                "commands": {
                    "accept": {"enabled": true, "label": "Accept"},
                    "test": {"enabled": false, "label": "Test",
                             "reason": {"code": "V4012", "text": "no candidate"}},
                    "finalizeAll": {"enabled": true, "label": "Finalize all",
                                    "steps": ["accept", "test", "assemble"]}
                }
            }
        },
        "persistedContentHash": "13".repeat(32),
        "retained": {"result": "discarded", "reason": "damaged"},
        "candidateLost": {"name": "tune", "stage": "testing"},
        "fault": {"vCode": "V4001", "message": "trap", "taskId": 1, "instanceId": 2},
        "runState": "stopped",
        "containerFormat": 9,
        "aFieldOfALaterServer": [1, 2]
    })
}

#[test]
fn from_identity_when_full_answer_then_every_field_is_read() {
    let facts = DeviceFacts::from_identity(&identity_json()).unwrap();

    assert_eq!(facts.device_id, Some(id(0xab)));
    assert_eq!(facts.runtime_version.as_deref(), Some("0.247.0"));
    assert_eq!(facts.content_hash, Some([0x11; 32]));
    assert_eq!(facts.layout_hash, Some([0x12; 32]));
    assert_eq!(facts.persisted_content_hash, Some([0x13; 32]));
    assert_eq!(facts.run_state, RunState::Stopped);
    assert_eq!(facts.container_format, Some(9));
    assert_eq!(facts.fault.unwrap().v_code, "V4001");
    assert_eq!(facts.retained.unwrap().reason.as_deref(), Some("damaged"));
    let lost = facts.candidate_lost.unwrap();
    assert_eq!(lost.name.as_deref(), Some("tune"));
    assert_eq!(lost.stage, Some(EditStage::Testing));
    let edit = facts.edit.unwrap();
    assert_eq!(edit.stage, EditStage::Idle);
    assert_eq!(edit.accept_refusal, None);
}

#[test]
fn from_identity_when_the_device_predates_the_fields_then_they_are_absent_and_it_scans() {
    let identity = json!({
        "protocol": 1,
        "device": {"name": "ironplcvm", "model": "m", "modification": "x", "runtimeVersion": "0.1.0"},
        "application": {"mode": "normal", "active": 1, "normal": 1, "candidate": null,
                        "application": 1, "migration": false, "rounds": 0}
    });

    let facts = DeviceFacts::from_identity(&identity).unwrap();

    assert_eq!(facts.device_id, None);
    assert_eq!(facts.content_hash, None);
    assert_eq!(facts.edit, None);
    assert_eq!(facts.container_format, None);
    assert_eq!(facts.run_state, RunState::Running);
}

#[test]
fn from_identity_when_accept_is_refused_then_the_refusal_is_kept() {
    let mut identity = identity_json();
    identity["application"]["edit"]["commands"]["accept"] = json!({
        "enabled": false, "label": "Accept",
        "reason": {"code": "V4004", "text": "a candidate is staged"}
    });

    let facts = DeviceFacts::from_identity(&identity).unwrap();

    assert_eq!(
        facts.edit.unwrap().accept_refusal,
        Some(Reason {
            code: "V4004".to_owned(),
            text: "a candidate is staged".to_owned()
        })
    );
}

#[rstest]
#[case::hash_not_hex("application", "contentHash", json!("not a hash"))]
#[case::persisted_hash_short("top", "persistedContentHash", json!("11"))]
#[case::device_id_invalid("device", "deviceId", json!("XYZ"))]
#[case::run_state_unknown("top", "runState", json!("hovering"))]
#[case::format_not_a_number("top", "containerFormat", json!("nine"))]
fn from_identity_when_a_field_is_present_and_wrong_then_error(
    #[case] block: &str,
    #[case] field: &str,
    #[case] value: serde_json::Value,
) {
    let mut identity = identity_json();
    match block {
        "top" => identity[field] = value,
        other => identity[other][field] = value,
    }

    let error = DeviceFacts::from_identity(&identity).unwrap_err();

    assert!(error.to_string().contains("cannot be read"), "{error}");
}

#[test]
fn from_identity_when_accept_is_disabled_without_a_reason_then_error() {
    let mut identity = identity_json();
    identity["application"]["edit"]["commands"]["accept"] =
        json!({"enabled": false, "label": "Accept"});

    let error = DeviceFacts::from_identity(&identity).unwrap_err();

    assert!(error.to_string().contains("gives no reason"), "{error}");
}

#[test]
fn from_identity_when_not_an_identity_then_error() {
    assert!(DeviceFacts::from_identity(&json!({"hello": 1})).is_err());
    assert!(DeviceFacts::from_identity(&json!([1])).is_err());
}
