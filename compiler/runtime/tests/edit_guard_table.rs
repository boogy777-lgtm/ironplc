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

mod common;

use common::edit_states::{body_candidate, host_in, State, STATES};
use ironplc_runtime::{HostMode, OnlineChangeError, RuntimeHost};

/// An edit command.
#[derive(Clone, Copy, Debug)]
enum Command {
    Accept,
    Test,
    Untest,
    Assemble,
    Cancel,
}

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
        // Cancel is allowed here since stage 4b: the host reverts at the next
        // boundary and discards the candidate there (two steps, one command).
        State::Testing => [
            Err(CandidateAlreadyStaged),
            Err(NotAllowedInThisMode),
            OK,
            OK,
            OK,
        ],
        State::TestingMigration => [
            Err(CandidateAlreadyStaged),
            Err(NotAllowedInThisMode),
            Err(UntestUnsupported),
            OK,
            Err(UntestUnsupported),
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
