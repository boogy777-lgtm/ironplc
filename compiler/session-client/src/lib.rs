//! A client of the IronPLC engineering session protocol.
//!
//! A [`Session`] connects to a device (`ironplcvm serve`, or any device that
//! speaks the protocol), sends the [`Command`](ironplc_runtime::Command)s of
//! `ironplc_runtime` and reads its [`Response`](ironplc_runtime::Response)s,
//! which are the one definition of the protocol. How the lines reach the device
//! is a connection method ([`Endpoint`]): `stdio`, a local `ironplcvm serve`
//! that the client spawns, or `tcp`, a socket to a device on the network. The
//! design is `specs/design/session-client.md`, over the protocol of
//! `specs/design/engineering-connection.md`.
//!
//! The crate has no state machine of the editor's kind (reconnect with backoff,
//! heartbeat): a session is one connection, and a caller that loses it opens
//! another. It is for the programs that connect for a task, such as the MCP
//! server and a command line.

mod channel;
mod endpoint;
mod error;
mod session;

pub use channel::{Channel, ChannelError};
pub use endpoint::{Endpoint, StdioEndpoint, TcpEndpoint};
pub use error::{Phase, SessionError};
pub use session::{Session, Timeouts};

// Spec conformance testing infrastructure (test-only).
#[cfg(test)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}

/// Meta-test: every requirement in `specs/design/session-client.md` has a
/// `#[spec_test(REQ_SC_session_client_NNN)]` in `src/` or `tests/`.
#[cfg(test)]
#[test]
fn all_spec_requirements_have_tests() {
    assert!(
        spec_requirements::UNTESTED.is_empty(),
        "Requirements in spec with no conformance test: {:?}",
        spec_requirements::UNTESTED
    );
}
