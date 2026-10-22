//! The errors of IronLink, each with its L-code and the exit code of its
//! category (`specs/steering/problem-code-management.md`).
//!
//! The constants of the codes are generated from `resources/problem-codes.csv`
//! by `ironplc_msg_gen::write_code_constants`, so a code is written once.
//! L6xxx are file system and network errors (exit code 2), L9xxx are internal
//! errors (exit code 3), as with the V-codes of `ironplcvm`.

use std::fmt;
use std::io;
use std::net::{IpAddr, SocketAddr};
use std::path::Path;

mod codes {
    include!(concat!(env!("OUT_DIR"), "/problem_codes.rs"));
}
use codes::{
    ARGUMENT_INVALID, INTERFACE_BIND, INTERNAL, NO_ANSWER, ROUTE_MISMATCH, SEND_FAILED, SESSION_IO,
    STORE_UNREADABLE, STORE_WRITE,
};

/// Exit code for network and IO errors.
const IO_EXIT_CODE: u8 = 2;
/// Exit code for internal errors (a bug of IronLink).
const INTERNAL_EXIT_CODE: u8 = 3;

/// A user-facing error: an L-code, what failed, and the exit code of the
/// category.
pub(crate) struct IronLinkError {
    l_code: &'static str,
    exit_code: u8,
    message: String,
}

impl IronLinkError {
    fn io(l_code: &'static str, message: String) -> Self {
        IronLinkError {
            l_code,
            exit_code: IO_EXIT_CODE,
            message,
        }
    }

    /// A socket cannot be bound on the local address `interface`.
    pub(crate) fn interface_bind(interface: IpAddr, err: &io::Error) -> Self {
        Self::io(
            INTERFACE_BIND,
            format!("unable to bind a socket on the local address {interface}: {err}"),
        )
    }

    /// The request cannot be sent to `destination`.
    pub(crate) fn send_failed(destination: SocketAddr, err: &io::Error) -> Self {
        Self::io(
            SEND_FAILED,
            format!("unable to send the discovery request to {destination}: {err}"),
        )
    }

    /// Nothing answered the identify request to `address` within `timeout_ms`.
    pub(crate) fn no_answer(address: impl fmt::Display, timeout_ms: u64) -> Self {
        Self::io(
            NO_ANSWER,
            format!("no answer from {address} within {timeout_ms} ms"),
        )
    }

    /// `address` answered, but with a discovery reply of another wire version,
    /// so this IronLink cannot read the device id it carries.
    pub(crate) fn other_version_answer(address: impl fmt::Display, wire_version: u8) -> Self {
        Self::io(
            NO_ANSWER,
            format!(
                "no discovery reply this IronLink can read from {address}: it answered with \
                 discovery version {wire_version}; update IronLink"
            ),
        )
    }

    /// An address, an interface or an option value that cannot be used.
    pub(crate) fn argument_invalid(message: impl Into<String>) -> Self {
        Self::io(ARGUMENT_INVALID, message.into())
    }

    /// The command session cannot be read or written.
    pub(crate) fn session_io(err: &io::Error) -> Self {
        Self::io(
            SESSION_IO,
            format!("unable to read or write the command session: {err}"),
        )
    }

    /// A file of the workstation store cannot be read as a file of this version.
    /// The file is left as it is: writing over it would destroy what the user
    /// kept in it.
    pub(crate) fn store_unreadable(file: &Path, reason: impl fmt::Display) -> Self {
        Self::io(
            STORE_UNREADABLE,
            format!(
                "unable to read {}: {reason}; IronLink did not change the file, \
                 fix it or delete it",
                file.display()
            ),
        )
    }

    /// A file of the workstation store cannot be written.
    pub(crate) fn store_write(file: &Path, err: &io::Error) -> Self {
        Self::io(
            STORE_WRITE,
            format!("unable to write {}: {err}", file.display()),
        )
    }

    /// The device at `address` answered with the device id `found`, and the
    /// route is for `expected`.
    pub(crate) fn route_mismatch(address: SocketAddr, expected: &str, found: &str) -> Self {
        Self::io(
            ROUTE_MISMATCH,
            format!(
                "the device at {address} has the device id {found}, not {expected}; \
                 no route was stored"
            ),
        )
    }

    /// A violated invariant of IronLink itself.
    pub(crate) fn internal(message: impl Into<String>) -> Self {
        IronLinkError {
            l_code: INTERNAL,
            exit_code: INTERNAL_EXIT_CODE,
            message: message.into(),
        }
    }

    /// The stable L-code of this error.
    pub(crate) fn l_code(&self) -> &'static str {
        self.l_code
    }

    /// What failed, without the code.
    pub(crate) fn message(&self) -> &str {
        &self.message
    }

    /// The process exit code of this error's category.
    pub(crate) fn exit_code(&self) -> u8 {
        self.exit_code
    }
}

impl fmt::Display for IronLinkError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{} - {}", self.l_code, self.message)
    }
}

// The user-facing form (`L#### - message`) is also the diagnostic form, so
// tests and loggers see exactly what a user would report.
impl fmt::Debug for IronLinkError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        fmt::Display::fmt(self, f)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn display_when_io_error_then_code_dash_message_and_exit_2() {
        let address: SocketAddr = "192.0.2.1:49152".parse().unwrap();

        let err = IronLinkError::no_answer(address, 1000);

        assert_eq!(
            err.to_string(),
            "L6003 - no answer from 192.0.2.1:49152 within 1000 ms"
        );
        assert_eq!(err.exit_code(), 2);
    }

    #[test]
    fn internal_when_created_then_l9001_and_exit_3() {
        let err = IronLinkError::internal("a worker panicked");

        assert_eq!(err.l_code(), "L9001");
        assert_eq!(err.exit_code(), 3);
        assert_eq!(err.message(), "a worker panicked");
    }

    #[test]
    fn store_unreadable_when_created_then_it_names_the_file_and_says_it_was_left_alone() {
        let err = IronLinkError::store_unreadable(Path::new("dir/routes.json"), "not valid JSON");

        assert!(err.message().contains("routes.json"), "{err}");
        assert!(err.message().contains("did not change the file"), "{err}");
        assert_eq!(err.exit_code(), 2);
    }

    #[test]
    fn route_mismatch_when_created_then_it_names_both_ids_and_the_address() {
        let address: SocketAddr = "192.0.2.1:49152".parse().unwrap();

        let err = IronLinkError::route_mismatch(address, "aa", "bb");

        assert_eq!(
            err.to_string(),
            "L6008 - the device at 192.0.2.1:49152 has the device id bb, not aa; \
             no route was stored"
        );
    }

    #[test]
    fn constructors_when_called_then_each_has_its_own_code() {
        let ip: IpAddr = "192.0.2.1".parse().unwrap();
        let socket = SocketAddr::new(ip, 1);
        let io = io::Error::other("boom");

        let codes = [
            IronLinkError::interface_bind(ip, &io).l_code(),
            IronLinkError::send_failed(socket, &io).l_code(),
            IronLinkError::argument_invalid("x").l_code(),
            IronLinkError::session_io(&io).l_code(),
            IronLinkError::store_unreadable(Path::new("routes.json"), "x").l_code(),
            IronLinkError::store_write(Path::new("routes.json"), &io).l_code(),
            IronLinkError::route_mismatch(socket, "a", "b").l_code(),
        ];

        assert_eq!(
            codes,
            ["L6001", "L6002", "L6004", "L6005", "L6006", "L6007", "L6008"]
        );
    }
}
