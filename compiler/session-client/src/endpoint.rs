//! The connection methods: the ways a client reaches one device.
//!
//! This is a class with two members today, `stdio` (a spawned local
//! `ironplcvm serve`) and `tcp` (a socket to a device), the Rust counterpart of
//! `CONNECTION_METHODS` in `integrations/vscode/src/connectionMethods.ts`. The
//! discovery driver is the near case outside the class: it finds controllers
//! and never carries a session.
//!
//! The table below is the one list of methods. [`Endpoint`], its names and the
//! match that opens a channel are all made from it, so a method without a
//! declaration or without an opener does not exist: the match has no wildcard
//! arm, and a member that does not implement [`Method`] does not compile. A new
//! method (a USB one) is a module of this directory that implements [`Method`],
//! and one row here. Nothing else of the crate is edited: the handshake, the
//! timeouts and the errors are the session's, not the method's.

use std::time::Duration;

use crate::channel::Channel;
use crate::error::SessionError;

mod stdio;
mod tcp;

pub use stdio::StdioEndpoint;
pub use tcp::TcpEndpoint;

/// What a connection method must do: open a channel to the device its endpoint
/// describes, within the connect timeout.
trait Method {
    /// Opens the channel. The handshake is not part of it.
    fn open(&self, connect_timeout: Duration) -> Result<Box<dyn Channel>, SessionError>;
}

/// Declares the connection methods: the name of each as a profile of the editor
/// writes it, the variant that holds its endpoint, and the type of the endpoint.
macro_rules! connection_methods {
    ($($name:literal => $variant:ident($endpoint:ty)),+ $(,)?) => {
        /// A device to connect to, by the connection method that reaches it.
        #[derive(Clone, Debug, PartialEq, Eq)]
        pub enum Endpoint {
            $(
                #[doc = concat!("Reached by the `", $name, "` connection method.")]
                $variant($endpoint),
            )+
        }

        impl Endpoint {
            /// The names of the connection methods, in the order of the table.
            pub const METHODS: &'static [&'static str] = &[$($name),+];

            /// The name of the connection method of this endpoint.
            pub fn method(&self) -> &'static str {
                match self {
                    $(Endpoint::$variant(_) => $name),+
                }
            }

            /// Opens the channel to the device. A [`Session`](crate::Session)
            /// opens it and makes the handshake; a client of another protocol
            /// that is also JSON lines uses the channel directly.
            pub fn open_channel(
                &self,
                connect_timeout: Duration,
            ) -> Result<Box<dyn Channel>, SessionError> {
                match self {
                    $(Endpoint::$variant(endpoint) => Method::open(endpoint, connect_timeout)),+
                }
            }
        }
    };
}

connection_methods! {
    "stdio" => Stdio(StdioEndpoint),
    "tcp" => Tcp(TcpEndpoint),
}

#[cfg(test)]
mod tests;
