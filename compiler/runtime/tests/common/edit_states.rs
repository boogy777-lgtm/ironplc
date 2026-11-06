//! The edit states of a host and the builders that drive a real host into
//! each one through the public operations only, shared by the tests that walk
//! every reachable state.

use ironplc_container::Container;
use ironplc_runtime::RuntimeHost;

use super::compile_with_ids;

/// An edit state of the host, named by how a test reaches it.
#[derive(Clone, Copy, Debug)]
pub enum State {
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
    /// Candidate staged as a replace (a download), original executing.
    AcceptedReplace,
    /// Replace candidate executing.
    TestingReplace,
    /// Test of a replace candidate requested, not yet applied.
    TestPendingReplace,
    /// Untest requested while the candidate executes, not yet applied.
    UntestPending,
}

pub const STATES: [State; 11] = [
    State::NoCandidate,
    State::Accepted,
    State::AcceptedMigration,
    State::Testing,
    State::TestingMigration,
    State::TestPending,
    State::TestPendingMigration,
    State::UntestPending,
    State::AcceptedReplace,
    State::TestingReplace,
    State::TestPendingReplace,
];

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
pub fn body_candidate() -> Container {
    compile_with_ids(&program(false, 2), &IDS[..1])
}

/// A candidate that adds a variable, so its layout hash differs.
fn migration_candidate() -> Container {
    compile_with_ids(&program(true, 2), &IDS)
}

/// A candidate that replaces the application: another layout and no stable
/// IDs to match it by, which a replace does not need.
fn replace_candidate() -> Container {
    compile_with_ids(&program(true, 3), &[])
}

/// A fresh, permitted host driven into `state`.
pub fn host_in(state: State) -> RuntimeHost {
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
        State::AcceptedReplace => {
            host.stage_replacing(replace_candidate(), None).unwrap();
        }
        State::TestingReplace => {
            host.stage_replacing(replace_candidate(), None).unwrap();
            host.test().unwrap();
            host.run(1, || 0).unwrap();
        }
        State::TestPendingReplace => {
            host.stage_replacing(replace_candidate(), None).unwrap();
            host.test().unwrap();
        }
    }
    host
}
