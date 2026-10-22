//! The command protocol of IronLink: JSON lines on stdio, which is how a client
//! (the editor extension, a script) starts IronLink as a child process and uses
//! it. `ironlink serve` runs it.
//!
//! The conventions are those of the session protocol of `ironplcvm serve`: one
//! command per line, an internally tagged `command` enum in camel case, one
//! response line for each command line, and a line that is not a command (not
//! JSON, an unknown command, a missing field) answered by the codeless codec
//! error while the loop goes on. End of input ends the loop. Stdout is the
//! protocol and nothing else; the log goes to stderr.
//!
//! The command line of IronLink calls [`execute`] too and renders the
//! [`Response`] itself, so the shell and the command line cannot disagree.

use std::io::{self, BufRead, Write};

use serde::{Deserialize, Serialize};

use crate::error::IronLinkError;
use crate::service::{
    browse, identify, version, BrowseRequest, BrowseResult, IdentifyRequest, IdentifyResult,
    VersionInfo,
};

/// A command of the shell.
#[derive(Clone, Debug, PartialEq, Eq, Deserialize)]
#[serde(tag = "command", rename_all = "camelCase")]
pub(crate) enum Command {
    /// Finds controllers with the drivers the request selects.
    Browse(BrowseRequest),
    /// Asks one route for a fresh observation.
    Identify(IdentifyRequest),
    /// Reports the version of IronLink and of the discovery wire it speaks.
    Version,
}

/// The answer to one [`Command`].
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(tag = "response", rename_all = "camelCase")]
pub(crate) enum Response {
    Browse(BrowseResult),
    Identify(IdentifyResult),
    Version(VersionInfo),
    /// The command failed, or the line was not a command.
    Error(ErrorResponse),
}

/// A failed command: the L-code, or `null` for a line that is not a command
/// (a codec error has no code, as in the session protocol), and what failed.
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct ErrorResponse {
    pub(crate) l_code: Option<&'static str>,
    pub(crate) message: String,
}

/// Runs a command. This is the whole mapping from command to implementation.
pub(crate) fn execute(command: &Command) -> Result<Response, IronLinkError> {
    match command {
        Command::Browse(request) => browse(request).map(Response::Browse),
        Command::Identify(request) => identify(request).map(Response::Identify),
        Command::Version => Ok(Response::Version(version())),
    }
}

/// The response line (without its newline) for a response.
pub(crate) fn render_response(response: &Response) -> Result<String, IronLinkError> {
    serde_json::to_string(response).map_err(|err| {
        IronLinkError::internal(format!("unable to render the response as JSON: {err}"))
    })
}

/// The response for one input line: the answer to the command, the coded error
/// of a command that failed, or the codeless codec error of a line that is not
/// a command.
fn respond(line: &str) -> Response {
    match serde_json::from_str::<Command>(line) {
        Ok(command) => execute(&command).unwrap_or_else(|err| {
            Response::Error(ErrorResponse {
                l_code: Some(err.l_code()),
                message: err.message().to_string(),
            })
        }),
        Err(err) => {
            log::warn!("ignoring a line that is not a command: {err}");
            Response::Error(ErrorResponse {
                l_code: None,
                message: format!("invalid command line: {err}"),
            })
        }
    }
}

/// Reads one command per line from `reader` and writes one response line for
/// each to `writer`, flushing after every line, until the end of the input.
pub(crate) fn serve(reader: impl BufRead, mut writer: impl Write) -> Result<(), IronLinkError> {
    for line in reader.lines() {
        let line = line.map_err(|err| IronLinkError::session_io(&err))?;
        let rendered = render_response(&respond(&line))?;
        writeln!(writer, "{rendered}")
            .and_then(|()| writer.flush())
            .map_err(|err: io::Error| IronLinkError::session_io(&err))?;
    }
    Ok(())
}

#[cfg(test)]
mod tests;
