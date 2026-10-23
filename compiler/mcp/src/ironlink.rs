//! The server as a client of IronLink.
//!
//! IronLink (`ironlink`) finds controllers and keeps this workstation's route
//! record and address list; IronPLC connects ([ADR-0084]). The server is a thin
//! client of it, as the editor extension is: it starts `ironlink serve` as a
//! child process and speaks its JSON lines over the `stdio` channel of
//! `ironplc_session_client`, and it links no IronLink code. The commands and
//! their answers are those of `specs/design/ironlink.md`; this module passes the
//! answer through as JSON and mirrors none of its types.
//!
//! [ADR-0084]: ../../../specs/adrs/0084-plc-discovery-and-controller-selection.md

use std::env;
use std::ffi::{OsStr, OsString};
use std::path::{Path, PathBuf};
use std::time::Duration;

use ironplc_runtime::frame::CONNECT_TIMEOUT;
use ironplc_session_client::{ChannelError, Endpoint, StdioEndpoint};

/// The time `ironlink` may take beyond the wait it was asked for (the
/// discovery wait of a browse) to start and to write its answer.
pub const ANSWER_GRACE: Duration = Duration::from_secs(5);

/// Why a command to IronLink got no answer.
#[derive(Debug, PartialEq, Eq)]
pub enum IronLinkError {
    /// No `ironlink` executable was found.
    NotFound {
        /// Where it was looked for, for the message.
        searched: String,
    },
    /// `ironlink` did not start, did not answer in time, or ended.
    Unavailable {
        /// What went wrong.
        message: String,
    },
    /// `ironlink` answered with an error.
    Refused {
        /// The L-code (`L6004`), or `None` for the error that has no code, which
        /// answers a line that is not a command.
        l_code: Option<String>,
        /// What `ironlink` said.
        message: String,
    },
}

/// The `ironlink` executable and the store it is told to use.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct IronLink {
    executable: PathBuf,
    store: Option<PathBuf>,
}

/// The file name of the `ironlink` executable on this platform.
fn executable_name() -> String {
    format!("ironlink{}", env::consts::EXE_SUFFIX)
}

/// Finds the `ironlink` executable: first in `server_dir`, the directory of the
/// running server (the installers ship the programs together), then in the
/// directories of `path_var`. This is the order in which the toolchain
/// discovery of the extensions ends.
pub fn locate(
    server_dir: Option<&Path>,
    path_var: Option<&OsStr>,
) -> Result<PathBuf, IronLinkError> {
    let name = executable_name();
    let beside = server_dir.map(|dir| dir.join(&name));
    let on_path = path_var
        .into_iter()
        .flat_map(env::split_paths)
        .map(|dir| dir.join(&name));
    beside
        .into_iter()
        .chain(on_path)
        .find(|candidate| candidate.is_file())
        .ok_or_else(|| IronLinkError::NotFound {
            searched: match server_dir {
                Some(dir) => format!("{} and PATH", dir.display()),
                None => "PATH".to_string(),
            },
        })
}

impl IronLink {
    /// The `ironlink` of this installation, found from the running server.
    pub fn locate_here() -> Result<IronLink, IronLinkError> {
        let server_dir = env::current_exe()
            .ok()
            .and_then(|exe| exe.parent().map(Path::to_path_buf));
        let executable = locate(server_dir.as_deref(), env::var_os("PATH").as_deref())?;
        Ok(IronLink::new(executable, None))
    }

    /// The `ironlink` at `executable`, using the store of the user unless
    /// `store` names a directory (`IRONLINK_STORE`).
    pub fn new(executable: PathBuf, store: Option<PathBuf>) -> IronLink {
        IronLink { executable, store }
    }

    /// Starts `ironlink serve`, sends `command` as one line, and returns the
    /// answer as JSON, waiting at most `wait` (and [`ANSWER_GRACE`]) for it.
    /// `ironlink` is stopped when the call returns.
    pub fn call(
        &self,
        command: &serde_json::Value,
        wait: Duration,
    ) -> Result<serde_json::Value, IronLinkError> {
        let unavailable = |message: String| IronLinkError::Unavailable { message };
        let mut env_vars = Vec::new();
        if let Some(store) = &self.store {
            env_vars.push(("IRONLINK_STORE".to_string(), store.display().to_string()));
        }
        let endpoint = Endpoint::Stdio(StdioEndpoint {
            executable: self.executable.clone(),
            args: vec![OsString::from("serve")],
            env: env_vars,
        });
        let mut channel = endpoint
            .open_channel(CONNECT_TIMEOUT)
            .map_err(|err| unavailable(err.to_string()))?;
        channel
            .send(&command.to_string())
            .map_err(|err| unavailable(describe(&err)))?;
        let line = channel
            .receive(wait + ANSWER_GRACE)
            .map_err(|err| unavailable(describe(&err)))?;
        let answer: serde_json::Value = serde_json::from_str(&line)
            .map_err(|err| unavailable(format!("unreadable answer: {err}")))?;
        if answer["response"] == "error" {
            return Err(IronLinkError::Refused {
                l_code: answer["lCode"].as_str().map(str::to_string),
                message: answer["message"]
                    .as_str()
                    .map_or_else(|| "ironlink gave no message".to_string(), str::to_string),
            });
        }
        Ok(answer)
    }
}

fn describe(err: &ChannelError) -> String {
    match err {
        ChannelError::TimedOut => "no answer in time".to_string(),
        ChannelError::Closed(None) => "ironlink ended".to_string(),
        ChannelError::Closed(Some(detail)) => format!("ironlink ended: {detail}"),
        ChannelError::Broken(message) => message.clone(),
    }
}

#[cfg(test)]
mod tests;
