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

// This crate surfaces VM errors (`FaultContext`, whose trap variants are
// large) directly; boxing every one to satisfy clippy would obscure the
// error vocabulary for no runtime benefit.
#![allow(clippy::result_large_err)]

mod commands;
mod conversion;
mod error;
mod generation;
mod host;
mod migration;
mod online_change;
mod snapshot;

// V-code constants are generated from resources/problem-codes.csv by build.rs.
mod problem_codes {
    include!(concat!(env!("OUT_DIR"), "/problem_codes.rs"));
}

pub use commands::{
    execute, parse_command, render_response, Command, CommandError, DeviceIdentity, EditSpec,
    IdentityPayload, MigrationDecisionSpec, RedundancyIdentity, Response, StatusPayload,
    TypeChangeDetail, SESSION_PROTOCOL_VERSION,
};
pub use error::{OnlineChangeError, RuntimeError};
pub use generation::{ApplicationGeneration, LogicGeneration};
pub use host::{
    AcceptedEdit, EditBaseline, HostMode, HostStatus, PendingEditRecord, RuntimeHost, ScanCommit,
};
pub use migration::{MigrationDecision, MigrationError, StateMigrationPlan, TypeChangePair};
pub use snapshot::StateSnapshot;
