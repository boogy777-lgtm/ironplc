//! The hot-edit commands carried out on a device, through the session protocol.
//!
//! The command is the same [`Command`] the in-process host executes, and the
//! response has the same shape, so the contract of a tool does not fork: a
//! refusal of the device is its V-code and message, a success is `"ack"` with
//! the status after the command. The device drives the scan round of
//! `testEdits`, `untestEdits` and `assembleEdits` itself before it
//! acknowledges, so there is nothing for the tool to drive.
//!
//! A tool call is one session: connect, `identity`, the command, and a
//! `getStatus` for the snapshot after an acknowledgement, then the connection is
//! dropped. The device serves one engineering session at a time, so a call made
//! while another session is open is refused with V6014, which the tool returns.

use ironplc_problems::text;
use ironplc_runtime::{Command, Response};
use ironplc_session_client::{Session, SessionError};

use super::{internal_failure, validation_failure, HotEditResponse};
use crate::tools::common::ironlink_diagnostics;
use crate::tools::device::{DeviceFault, ResolvedDevice};

/// Carries out `command` on the device and shapes the response.
pub(super) fn exchange(device: &ResolvedDevice, command: Command) -> HotEditResponse {
    let mut session = match Session::connect(&device.endpoint) {
        Ok(session) => session,
        Err(error) => return failure(device, error),
    };
    let identity = session.identity().map(|identity| identity.device.clone());
    let mut response = match carry_out(&mut session, command) {
        Ok(response) => response,
        Err(error) => failure(device, error),
    };
    response.device = identity;
    response
}

/// Sends the command and, after an acknowledgement, reads the status.
fn carry_out(session: &mut Session, command: Command) -> Result<HotEditResponse, SessionError> {
    match session.send(&command)? {
        Response::Status(status) => Ok(HotEditResponse::ok("ack", status)),
        Response::Ack => match session.send(&Command::GetStatus)? {
            Response::Status(status) => Ok(HotEditResponse::ok("ack", status)),
            other => Ok(unexpected(&other)),
        },
        other => Ok(unexpected(&other)),
    }
}

fn unexpected(response: &Response) -> HotEditResponse {
    internal_failure(format!(
        "the device answered with an unexpected {response:?}"
    ))
}

/// The response for a session that failed: a refusal of the device keeps its
/// code, anything else is a connection that failed (P8003).
fn failure(device: &ResolvedDevice, error: SessionError) -> HotEditResponse {
    match error {
        SessionError::Device(refusal) => HotEditResponse::command_error(&refusal),
        other => validation_failure(text::mcp_device_connection_failed(
            device.describe(),
            other.to_string(),
        )),
    }
}

/// The response for a `device` parameter that could not be resolved.
pub fn unresolved(fault: DeviceFault) -> HotEditResponse {
    match fault {
        DeviceFault::Invalid(text) => validation_failure(text),
        DeviceFault::IronLink(error) => {
            HotEditResponse::with_diagnostics(ironlink_diagnostics(&error))
        }
    }
}

#[cfg(test)]
mod tests;
