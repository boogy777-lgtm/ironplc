//! Characterization of the hot-edit guards: for every reachable edit state
//! and every edit command, what a real host answers.
//!
//! The expected answers are written out below as data, in the order the
//! host's guards pick between two applicable errors (for example `assemble`
//! while a swap is pending answers "not allowed in this mode", not
//! "assemble without test"). Each cell is checked on a fresh host driven
//! into the state through the public operations only.
//!
//! Not in the table: `permit_execution`, `revoke_execution_permit` and device
//! settings are allowed in every state and are not edit commands.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![expect(
    clippy::unwrap_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

mod common;

use common::compile_with_ids;
use ironplc_container::Container;
use ironplc_runtime::{HostMode, OnlineChangeError, RuntimeHost};

/// An edit state of the host, named by how a test reaches it.
#[derive(Clone, Copy, Debug)]
enum State {
    /// Nothing staged.
    NoCandidate,
    /// Candidate staged, original executing.
    Accepted,
    /// Candidate staged and requires state migration, original executing.
    AcceptedMigration,
    /// Candidate executing.
    Testing,
    /// Migration candidate executing.
    TestingMigration,
    /// Test requested, not yet applied at a boundary.
    TestPending,
    /// Test of a migration candidate requested, not yet applied.
    TestPendingMigration,
    /// Untest requested while the candidate executes, not yet applied.
    UntestPending,
}

/// An edit command.
#[derive(Clone, Copy, Debug)]
enum Command {
    Accept,
    Test,
    Untest,
    Assemble,
    Cancel,
}

const STATES: [State; 8] = [
    State::NoCandidate,
    State::Accepted,
    State::AcceptedMigration,
    State::Testing,
    State::TestingMigration,
    State::TestPending,
    State::TestPendingMigration,
    State::UntestPending,
];

const COMMANDS: [Command; 5] = [
    Command::Accept,
    Command::Test,
    Command::Untest,
    Command::Assemble,
    Command::Cancel,
];

const OK: Result<(), OnlineChangeError> = Ok(());

/// The expected answer of every (state, command), as data. Columns follow
/// `COMMANDS`: accept, test, untest, assemble, cancel.
fn expected(state: State) -> [Result<(), OnlineChangeError>; 5] {
    use OnlineChangeError::{
        AssembleWithoutTest, CandidateAlreadyStaged, NoCandidateStaged, NoTestInProgress,
        NotAllowedInThisMode, UntestUnsupported,
    };
    match state {
        State::NoCandidate => [
            OK,
            Err(NoCandidateStaged),
            Err(NoTestInProgress),
            Err(NoCandidateStaged),
            Err(NoCandidateStaged),
        ],
        State::Accepted | State::AcceptedMigration => [
            Err(CandidateAlreadyStaged),
            OK,
            Err(NoTestInProgress),
            Err(AssembleWithoutTest),
            OK,
        ],
        State::Testing => [
            Err(CandidateAlreadyStaged),
            Err(NotAllowedInThisMode),
            OK,
            OK,
            Err(NotAllowedInThisMode),
        ],
        State::TestingMigration => [
            Err(CandidateAlreadyStaged),
            Err(NotAllowedInThisMode),
            Err(UntestUnsupported),
            OK,
            Err(NotAllowedInThisMode),
        ],
        State::TestPending | State::TestPendingMigration => [
            Err(CandidateAlreadyStaged),
            Err(NotAllowedInThisMode),
            Err(NoTestInProgress),
            Err(NotAllowedInThisMode),
            Err(NotAllowedInThisMode),
        ],
        State::UntestPending => [
            Err(CandidateAlreadyStaged),
            Err(NotAllowedInThisMode),
            Err(NotAllowedInThisMode),
            Err(NotAllowedInThisMode),
            Err(NotAllowedInThisMode),
        ],
    }
}

fn program(extra_variable: bool, step: i32) -> String {
    let extra = if extra_variable { "Extra : DINT;" } else { "" };
    format!(
        "PROGRAM main
  VAR
    Counter : DINT;
    {extra}
  END_VAR
  Counter := Counter + {step};
END_PROGRAM
"
    )
}

const IDS: [(&str, u64); 2] = [("Counter", 1), ("Extra", 2)];

fn base() -> Container {
    compile_with_ids(&program(false, 1), &IDS[..1])
}

/// A candidate with the same layout: only the body differs.
fn body_candidate() -> Container {
    compile_with_ids(&program(false, 2), &IDS[..1])
}

/// A candidate that adds a variable, so its layout hash differs.
fn migration_candidate() -> Container {
    compile_with_ids(&program(true, 2), &IDS)
}

/// A fresh, permitted host driven into `state`.
fn host_in(state: State) -> RuntimeHost {
    let mut host = RuntimeHost::new(base()).unwrap();
    host.permit_execution();
    match state {
        State::NoCandidate => {}
        State::Accepted => host.stage(body_candidate()).unwrap(),
        State::AcceptedMigration => host.stage(migration_candidate()).unwrap(),
        State::Testing => {
            host.stage(body_candidate()).unwrap();
            host.test().unwrap();
            host.run(1, || 0).unwrap();
        }
        State::TestingMigration => {
            host.stage(migration_candidate()).unwrap();
            host.test().unwrap();
            host.run(1, || 0).unwrap();
        }
        State::TestPending => {
            host.stage(body_candidate()).unwrap();
            host.test().unwrap();
        }
        State::TestPendingMigration => {
            host.stage(migration_candidate()).unwrap();
            host.test().unwrap();
        }
        State::UntestPending => {
            host.stage(body_candidate()).unwrap();
            host.test().unwrap();
            host.run(1, || 0).unwrap();
            host.untest().unwrap();
        }
    }
    host
}

fn apply(host: &mut RuntimeHost, command: Command) -> Result<(), OnlineChangeError> {
    match command {
        Command::Accept => host.stage(body_candidate()),
        Command::Test => host.test(),
        Command::Untest => host.untest(),
        Command::Assemble => host.assemble(),
        Command::Cancel => host.cancel(),
    }
}

#[test]
fn host_in_when_each_state_is_built_then_it_is_in_that_state() {
    for (state, candidate, mode, migration) in [
        (State::NoCandidate, false, HostMode::Normal, false),
        (State::Accepted, true, HostMode::Normal, false),
        (State::AcceptedMigration, true, HostMode::Normal, true),
        (State::Testing, true, HostMode::Testing, false),
        (State::TestingMigration, true, HostMode::Testing, true),
        (State::TestPending, true, HostMode::Normal, false),
        (State::TestPendingMigration, true, HostMode::Normal, true),
        (State::UntestPending, true, HostMode::Testing, false),
    ] {
        let status = host_in(state).status();
        assert_eq!(status.candidate.is_some(), candidate, "{state:?}");
        assert_eq!(status.mode, mode, "{state:?}");
        assert_eq!(status.migration, migration, "{state:?}");
    }
}

#[test]
fn edit_command_when_applied_in_each_state_then_the_host_answers_the_table() {
    for state in STATES {
        let table = expected(state);
        for (command, want) in COMMANDS.into_iter().zip(table) {
            let mut host = host_in(state);
            let got = apply(&mut host, command);
            assert_eq!(got, want, "{command:?} in {state:?}");
        }
    }
}
