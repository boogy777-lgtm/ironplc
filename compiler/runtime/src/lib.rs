//! Runtime host for the IronPLC VM.
//!
//! The host drives scan cycles and implements the P0 online change protocol:
//! a candidate container is staged and validated against the active one, and
//! the code is swapped atomically at a scan boundary while every byte of IEC
//! state survives in the host's [`VmBuffers`](ironplc_vm::VmBuffers). Code
//! reverts on `untest`; process state never does.
//!
//! See [`RuntimeHost`] for the protocol and `specs/design/bytecode-container-format.md`
//! ("Layout Hash and Online Change") for the container-side contract. The
//! [`commands`] module exposes the same protocol to external clients as typed,
//! line-delimited JSON commands with stable V-codes.

mod commands;
mod conversion;
mod edit_guard;
mod edit_view;
mod error;
pub mod frame;
mod generation;
mod host;
mod migration;
mod online_change;
mod persistent_state;
mod retained_state;
mod snapshot;

// V-code constants are generated from resources/problem-codes.csv by build.rs.
mod problem_codes {
    include!(concat!(env!("OUT_DIR"), "/problem_codes.rs"));
}

pub use commands::{
    execute, parse_command, parse_response, render_command, render_response, BootNote, Command,
    CommandError, DeviceIdentity, EditSpec, FaultNote, IdentityPayload, MigrationDecisionSpec,
    RedundancyIdentity, Response, RunState, StatusPayload, TypeChangeDetail, DEFAULT_SESSION_PORT,
    SESSION_PROTOCOL_VERSION,
};
pub use edit_guard::EditCommand;
pub use edit_view::{
    edit_view, CommandView, Commands, EditStage, EditView, NextCommand, Reason,
};
pub use error::{OnlineChangeError, RuntimeError};
pub use generation::{ApplicationGeneration, LogicGeneration};
pub use host::{
    AcceptedEdit, EditBaseline, HostMode, HostStatus, PendingEditRecord, RuntimeHost, ScanCommit,
};
pub use migration::{
    IdentitySide, IdentitySubject, MigrationDecision, MigrationError, StateMigrationPlan,
    TypeChangePair, UnidentifiedVariable,
};
pub use retained_state::{
    RetainedApplyError, RetainedDecodeError, RetainedImage, RetainedMismatch, RetainedTag,
};
pub use snapshot::StateSnapshot;
