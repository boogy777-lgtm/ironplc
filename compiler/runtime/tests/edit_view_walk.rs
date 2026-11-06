//! The edit view against a real host: for every reachable edit state and
//! every edit command, the view says what the host does (ADR-0087).
//!
//! A host is driven into each state through the public operations only. The
//! view is read from its status, then each command is applied to a fresh host
//! in the same state, and the two answers must agree: enabled exactly when the
//! host accepts, and the reason is the V-code and the text of the refusal the
//! host gives.
//!
//! The same walk writes the views of all states as the golden file
//! `resources/test/edit_view_golden.json`, which the dialog tests of the VS
//! Code extension read. The test fails when the file is out of date. To
//! regenerate it, run the test with `IRONPLC_UPDATE_GOLDEN=1`.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![expect(
    clippy::unwrap_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

mod common;

use std::path::PathBuf;

use common::edit_states::{body_candidate, host_in, State, STATES};
use ironplc_runtime::{
    edit_view, CommandError, EditCommand, EditStage, EditView, OnlineChangeError, RuntimeHost,
};

/// The environment variable that makes the golden test rewrite its file.
const UPDATE_GOLDEN: &str = "IRONPLC_UPDATE_GOLDEN";

fn apply(host: &mut RuntimeHost, command: EditCommand) -> Result<(), OnlineChangeError> {
    match command {
        EditCommand::Accept => host.stage(body_candidate()),
        EditCommand::Test => host.test(),
        EditCommand::Untest => host.untest(),
        EditCommand::Assemble => host.assemble(),
        EditCommand::Cancel => host.cancel(),
    }
}

fn view_in(state: State) -> EditView {
    edit_view(&host_in(state).status())
}

/// The stage and the next command each state is expected to report, as data.
fn expected_stage_and_next(state: State) -> (EditStage, Option<EditCommand>) {
    match state {
        State::NoCandidate => (EditStage::Idle, Some(EditCommand::Accept)),
        State::Accepted | State::AcceptedMigration => {
            (EditStage::Accepted, Some(EditCommand::Test))
        }
        State::Testing => (EditStage::Testing, Some(EditCommand::Assemble)),
        State::TestingMigration => (EditStage::TestingMigration, Some(EditCommand::Assemble)),
        State::TestPending | State::TestPendingMigration | State::UntestPending => {
            (EditStage::SwapPending, None)
        }
    }
}

#[test]
fn edit_view_when_each_state_and_command_then_enabled_is_what_the_host_accepts() {
    for state in STATES {
        let view = view_in(state);
        for command in EditCommand::ALL {
            let answer = apply(&mut host_in(state), command);
            let shown = &view.commands.edit[&command];

            assert_eq!(
                shown.enabled,
                answer.is_ok(),
                "{command:?} in {state:?}: enabled"
            );
            assert_eq!(shown.label, command.label(), "{command:?} in {state:?}");
        }
    }
}

#[test]
fn edit_view_when_command_is_refused_then_the_reason_is_the_code_and_text_of_the_refusal() {
    for state in STATES {
        let view = view_in(state);
        for command in EditCommand::ALL {
            let answer = apply(&mut host_in(state), command);
            let shown = &view.commands.edit[&command];

            match answer {
                Ok(()) => assert_eq!(shown.reason, None, "{command:?} in {state:?}"),
                Err(error) => {
                    let refusal = CommandError::from(error);
                    let reason = shown.reason.as_ref().unwrap();
                    assert_eq!(reason.code, refusal.v_code(), "{command:?} in {state:?}");
                    assert_eq!(reason.text, refusal.message(), "{command:?} in {state:?}");
                }
            }
        }
    }
}

#[test]
fn edit_view_when_each_state_then_stage_and_next_are_the_expected_ones() {
    for state in STATES {
        let view = view_in(state);
        let (stage, next) = expected_stage_and_next(state);

        assert_eq!(view.stage, stage, "{state:?}");
        assert_eq!(
            view.next.as_ref().map(|next| next.command),
            next,
            "{state:?}"
        );
    }
}

#[test]
fn edit_view_when_next_is_present_then_the_host_accepts_it() {
    for state in STATES {
        if let Some(next) = view_in(state).next {
            let answer = apply(&mut host_in(state), next.command);

            assert_eq!(answer, Ok(()), "next {:?} in {state:?}", next.command);
        }
    }
}

#[test]
fn edit_view_when_finalize_all_then_it_is_enabled_when_accept_is_and_its_steps_run_in_order() {
    for state in STATES {
        let view = view_in(state);
        let finalize = &view.commands.finalize_all;

        assert_eq!(
            finalize.view.enabled,
            apply(&mut host_in(state), EditCommand::Accept).is_ok(),
            "{state:?}"
        );
        if finalize.view.enabled {
            // The client runs the steps in order; a swap applies at the scan
            // boundary between them.
            let mut host = host_in(state);
            for step in &finalize.steps {
                assert_eq!(apply(&mut host, *step), Ok(()), "{step:?} in {state:?}");
                host.run(1, || 0).unwrap();
            }
            let status = host.status();
            assert_eq!(status.candidate, None, "{state:?}");
            assert_eq!(
                edit_view(&status).stage,
                EditStage::Idle,
                "finalized from {state:?}"
            );
        }
    }
}

#[test]
fn edit_view_when_status_payload_is_built_then_it_carries_the_view_of_the_status() {
    for state in STATES {
        let status = host_in(state).status();
        let payload = ironplc_runtime::StatusPayload::from(status.clone());

        assert_eq!(
            payload.edit,
            Some(Box::new(edit_view(&status))),
            "{state:?}"
        );
    }
}

/// The views of all states, in the order of `STATES`, as the pretty JSON of
/// the golden file.
fn golden_text() -> String {
    let entries: Vec<serde_json::Value> = STATES
        .into_iter()
        .map(|state| {
            serde_json::json!({
                "state": format!("{state:?}"),
                "view": view_in(state),
            })
        })
        .collect();
    let mut text = serde_json::to_string_pretty(&entries).unwrap();
    text.push('\n');
    text
}

fn golden_path() -> PathBuf {
    PathBuf::from(env!("CARGO_MANIFEST_DIR")).join("resources/test/edit_view_golden.json")
}

#[test]
fn edit_view_when_compared_with_the_golden_file_then_it_is_up_to_date() {
    let path = golden_path();
    let current = golden_text();

    if std::env::var_os(UPDATE_GOLDEN).is_some() {
        std::fs::create_dir_all(path.parent().unwrap()).unwrap();
        std::fs::write(&path, &current).unwrap();
        return;
    }

    let stored = std::fs::read_to_string(&path)
        .unwrap_or_default()
        .replace("\r\n", "\n");
    assert!(
        stored == current,
        "{} is out of date; run this test with {UPDATE_GOLDEN}=1 to regenerate it",
        path.display()
    );
}
