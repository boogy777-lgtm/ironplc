//! Runtime host for the IronPLC VM.
//!
//! The host drives scan cycles and implements the P0 online change protocol:
//! a candidate container is staged and validated against the active one, and
//! the code is swapped atomically at a scan boundary while every byte of IEC
//! state survives in the host's [`VmBuffers`](ironplc_vm::VmBuffers). Code
//! reverts on `untest`; process state never does.
//!
//! See [`RuntimeHost`] for the protocol and `specs/design/bytecode-container-format.md`
//! ("Layout Hash and Online Change") for the container-side contract.

// This crate surfaces VM errors (`FaultContext`, whose trap variants are
// large) directly; boxing every one to satisfy clippy would obscure the
// error vocabulary for no runtime benefit.
#![allow(clippy::result_large_err)]

mod error;
mod generation;
mod host;
mod online_change;

pub use error::{OnlineChangeError, RuntimeError};
pub use generation::{ApplicationGeneration, LogicGeneration};
pub use host::{HostMode, HostStatus, RuntimeHost};
