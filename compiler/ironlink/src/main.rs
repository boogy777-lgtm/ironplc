//! IronLink: the workstation tool that finds IronPLC controllers (ADR-0084).
//!
//! IronLink FINDS controllers and the routes to them. It opens no listening
//! port, holds no session and never writes to a controller: the only datagram it
//! sends is the discovery request, and IronPLC is what connects. The commands
//! are `browse`, `identify`, `serve` (the same two operations as JSON lines on
//! stdio, for a client that starts IronLink as a child process) and `version`.
//! The design is in `specs/design/ironlink.md`.
//!
//! - **Class:** the discovery driver, in [`driver`]: how IronLink finds
//!   controllers (today broadcast and an address list; later USB enumeration, a
//!   gateway). A near case outside it: the connection method, how IronPLC talks
//!   to a controller, which is not built here.
//! - **Inv:** every driver is a member of [`driver::DriverKind`], which a
//!   command can name only through the list the declaration derives, so a driver
//!   missing from the registry does not compile; every observation is made from a
//!   datagram that `decode_reply` accepted and keeps the time it was seen; nothing
//!   is sent but `ironplc_discovery::request()`, and only [`collect`] sends.
//! - **N+1:** a new driver is its own module, one line of the declaration and
//!   one arm of `DriverKind::build`; browse, identify, the shell and the command
//!   line are not edited.
//!
//! The command line and the shell call the same functions ([`shell::execute`])
//! and differ only in how they render the result.

use std::io::{self, Write};
use std::process::ExitCode;

use clap::Parser;

mod collect;
mod driver;
mod error;
mod observation;
mod parallel;
mod render;
mod service;
mod shell;
#[cfg(test)]
mod test_support;

use error::IronLinkError;
use service::{Answer, BrowseRequest, IdentifyRequest, IdentifyResult};
use shell::{Command, Response};

#[cfg(test)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}

#[derive(Parser, Debug)]
#[command(
    name = "ironlink",
    about = "IronLink finds IronPLC controllers on the network"
)]
struct Args {
    /// Turn on verbose logging to stderr. Repeat to increase verbosity.
    #[arg(short, long, action = clap::ArgAction::Count)]
    verbose: u8,

    /// Selects the subcommand.
    #[command(subcommand)]
    action: Action,
}

#[derive(clap::Subcommand, Debug)]
enum Action {
    /// Finds controllers by broadcast and by address list, and prints the tree
    /// of what answered.
    Browse {
        /// A local address of this PC to broadcast from (repeat for several).
        /// The broadcast leaves through the interface that has this address.
        /// Without --interface and --address, the broadcast uses the interface
        /// the operating system picks.
        #[arg(long = "interface", value_name = "IP")]
        interfaces: Vec<String>,

        /// A controller to ask directly: IP, or IP:PORT (repeat for several).
        #[arg(long = "address", value_name = "ADDR")]
        addresses: Vec<String>,

        /// The discovery port of the broadcast and of an address without one
        /// (default: the IronPLC session port).
        #[arg(long, value_name = "PORT")]
        port: Option<u16>,

        /// How long to wait for replies, in milliseconds (default 1000).
        #[arg(long, value_name = "MS")]
        timeout_ms: Option<u64>,

        /// How many times to send the request, at least 300 ms apart
        /// (default 2).
        #[arg(long, value_name = "N")]
        attempts: Option<u32>,

        /// Print the JSON response of the shell instead of text.
        #[arg(long)]
        json: bool,
    },
    /// Asks one controller address for a fresh observation. Exits with L6003
    /// when nothing answers.
    Identify {
        /// IP, or IP:PORT, of the controller.
        address: String,

        /// How long to wait for the answer, in milliseconds (default 1000).
        #[arg(long, value_name = "MS")]
        timeout_ms: Option<u64>,

        /// How many times to send the request, at least 300 ms apart
        /// (default 2).
        #[arg(long, value_name = "N")]
        attempts: Option<u32>,

        /// Print the JSON response of the shell instead of text.
        #[arg(long)]
        json: bool,
    },
    /// Serves the commands browse, identify and version as JSON lines on
    /// stdin and stdout.
    Serve,
    /// Prints the version of IronLink and of the discovery wire it speaks.
    Version,
}

pub fn main() -> ExitCode {
    let args = Args::parse();

    let result = ironplc_cli_support::logger::configure(args.verbose, None)
        .map_err(|err| IronLinkError::argument_invalid(err.to_string()))
        .and_then(|()| run(args.action));

    match result {
        Ok(()) => ExitCode::SUCCESS,
        Err(err) => {
            eprintln!("Error: {err}");
            ExitCode::from(err.exit_code())
        }
    }
}

fn run(action: Action) -> Result<(), IronLinkError> {
    match action {
        Action::Browse {
            interfaces,
            addresses,
            port,
            timeout_ms,
            attempts,
            json,
        } => {
            let command = Command::Browse(BrowseRequest {
                interfaces,
                addresses,
                port,
                timeout_ms,
                attempts,
            });
            show(&shell::execute(&command)?, json)
        }
        Action::Identify {
            address,
            timeout_ms,
            attempts,
            json,
        } => {
            let command = Command::Identify(IdentifyRequest {
                address,
                timeout_ms,
                attempts,
            });
            let response = shell::execute(&command)?;
            let unanswered = no_answer_of(&response);
            // The shell answers "nothing answered" as a normal response. Here it
            // is an error with a code, so text mode says it once, on stderr.
            if json || unanswered.is_none() {
                show(&response, json)?;
            }
            unanswered.map_or(Ok(()), Err)
        }
        Action::Serve => {
            let stdin = io::stdin();
            shell::serve(stdin.lock(), io::stdout().lock())
        }
        Action::Version => show(&shell::execute(&Command::Version)?, false),
    }
}

/// The error of an identify that nothing answered.
fn no_answer_of(response: &Response) -> Option<IronLinkError> {
    match response {
        Response::Identify(IdentifyResult {
            address,
            timeout_ms,
            answer: Answer::None,
        }) => Some(IronLinkError::no_answer(address, *timeout_ms)),
        _ => None,
    }
}

/// Prints a response: the JSON line of the shell, or the text of a person.
fn show(response: &Response, json: bool) -> Result<(), IronLinkError> {
    let text = if json {
        shell::render_response(response)?
    } else {
        render::text(response, observation::now_ms())
    };
    writeln!(io::stdout().lock(), "{text}").map_err(|err| IronLinkError::session_io(&err))
}
