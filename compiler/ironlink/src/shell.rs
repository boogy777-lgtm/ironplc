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
//!
//! The store of the workstation (routes, address list) is passed to the shell;
//! the shell never looks for the user's directory itself.

use std::io::{self, BufRead, Write};

use serde::{Deserialize, Serialize};

use crate::error::IronLinkError;
use crate::service::{
    address_add, address_list, address_remove, browse, identify, route_list, route_remove,
    route_set, version, AddressAddResult, AddressListResult, AddressRemoveResult, AddressRequest,
    BrowseRequest, BrowseResult, IdentifyRequest, IdentifyResult, RouteListResult,
    RouteRemoveRequest, RouteRemoveResult, RouteSetRequest, RouteSetResult, VersionInfo,
};
use crate::store::Store;

/// A command of the shell.
#[derive(Clone, Debug, PartialEq, Eq, Deserialize)]
#[serde(tag = "command", rename_all = "camelCase")]
pub(crate) enum Command {
    /// Finds controllers with the drivers the request selects.
    Browse(BrowseRequest),
    /// Asks one route for a fresh observation.
    Identify(IdentifyRequest),
    /// Verifies the route to a device id with an identify and stores it.
    RouteSet(RouteSetRequest),
    /// Forgets the route to a device id.
    RouteRemove(RouteRemoveRequest),
    /// Lists the stored routes.
    RouteList,
    /// Adds a controller address to the stored address list.
    AddressAdd(AddressRequest),
    /// Removes a controller address from the stored address list.
    AddressRemove(AddressRequest),
    /// Lists the stored address list.
    AddressList,
    /// Reports the version of IronLink and of the discovery wire it speaks.
    Version,
}

/// The answer to one [`Command`].
#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(tag = "response", rename_all = "camelCase")]
pub(crate) enum Response {
    Browse(BrowseResult),
    Identify(IdentifyResult),
    RouteSet(RouteSetResult),
    RouteRemove(RouteRemoveResult),
    RouteList(RouteListResult),
    AddressAdd(AddressAddResult),
    AddressRemove(AddressRemoveResult),
    AddressList(AddressListResult),
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

/// Runs a command, with the workstation store `store`. This is the whole
/// mapping from command to implementation.
pub(crate) fn execute(command: &Command, store: &Store) -> Result<Response, IronLinkError> {
    match command {
        Command::Browse(request) => browse(request, store).map(Response::Browse),
        Command::Identify(request) => identify(request).map(Response::Identify),
        Command::RouteSet(request) => route_set(request, store).map(Response::RouteSet),
        Command::RouteRemove(request) => route_remove(request, store).map(Response::RouteRemove),
        Command::RouteList => route_list(store).map(Response::RouteList),
        Command::AddressAdd(request) => address_add(request, store).map(Response::AddressAdd),
        Command::AddressRemove(request) => {
            address_remove(request, store).map(Response::AddressRemove)
        }
        Command::AddressList => address_list(store).map(Response::AddressList),
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
fn respond(line: &str, store: &Store) -> Response {
    match serde_json::from_str::<Command>(line) {
        Ok(command) => execute(&command, store).unwrap_or_else(|err| {
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
pub(crate) fn serve(
    reader: impl BufRead,
    mut writer: impl Write,
    store: &Store,
) -> Result<(), IronLinkError> {
    for line in reader.lines() {
        let line = line.map_err(|err| IronLinkError::session_io(&err))?;
        let rendered = render_response(&respond(&line, store))?;
        writeln!(writer, "{rendered}")
            .and_then(|()| writer.flush())
            .map_err(|err: io::Error| IronLinkError::session_io(&err))?;
    }
    Ok(())
}

#[cfg(test)]
mod tests;
