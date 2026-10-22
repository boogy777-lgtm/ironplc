//! The class of discovery drivers: how IronLink finds controllers (ADR-0084).
//!
//! - **Class:** a way of reaching candidate controllers with the discovery
//!   request and collecting what comes back. Members today: `broadcast`
//!   ([`broadcast`]) and `address list` ([`address_list`]), which are the two
//!   real cases that make this a class. Later: USB enumeration, a gateway. A
//!   near case outside it: the connection method (how IronPLC talks to a
//!   controller once it is found), which a driver never carries and which is
//!   not built.
//! - **Inv:** every member is one value of [`DriverKind`], and the set of
//!   drivers a command can name is [`DriverKind::ALL`], which the declaration
//!   below derives from the same list as the enum, so a member cannot be left
//!   out. [`DriverKind::build`] matches the enum without a wildcard, so a
//!   member without a builder does not compile. A driver produces its findings
//!   only through [`collect`], which turns a datagram into an outcome only after
//!   `decode_reply` examined it and sends nothing but
//!   `ironplc_discovery::request()`.
//! - **N+1:** a new driver is one module with its implementation of [`Driver`]
//!   and a `from_selection` that says what it takes from a selection, one line in
//!   the declaration of [`DriverKind`] and one arm of [`DriverKind::build`].
//!   `browse`, `identify` and the command dispatch are not edited.
//!
//! A driver takes what it needs from a [`Selection`] (the interfaces and
//! addresses the user named) and answers `None` when the selection gives it
//! nothing to do. The default when the selection names nothing at all (the
//! broadcast on the interface the operating system picks) is the broadcast
//! member's own rule, not the dispatcher's.

pub(crate) mod address_list;
pub(crate) mod broadcast;

use std::net::{IpAddr, SocketAddr};

use serde::Serialize;

use crate::collect::{Collected, Window};
use crate::error::IronLinkError;

/// Declares the members of the class once: the enum, the list of all members and
/// the label shown to a person. The JSON name of a member is its variant name in
/// camel case.
macro_rules! declare_drivers {
    ($($variant:ident => $label:literal),+ $(,)?) => {
        /// A member of the class of discovery drivers.
        #[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Serialize)]
        #[serde(rename_all = "camelCase")]
        pub(crate) enum DriverKind {
            $($variant),+
        }

        impl DriverKind {
            /// Every member of the class, in the order of the declaration.
            pub(crate) const ALL: &'static [DriverKind] = &[$(DriverKind::$variant),+];

            /// The name of the member as a person reads it.
            pub(crate) fn label(self) -> &'static str {
                match self {
                    $(DriverKind::$variant => $label),+
                }
            }
        }
    };
}

declare_drivers! {
    Broadcast => "broadcast",
    AddressList => "address list",
}

/// What the user named: the local interfaces to broadcast from, the addresses to
/// ask, and the port both use unless an address carries its own.
#[derive(Clone, Debug, PartialEq, Eq)]
pub(crate) struct Selection {
    /// Local addresses of this PC, by which the interfaces are named.
    pub(crate) interfaces: Vec<IpAddr>,
    /// Controller addresses, each with its port.
    pub(crate) addresses: Vec<SocketAddr>,
    /// The discovery port: the session port unless the user gave another.
    pub(crate) port: u16,
}

/// A way of finding controllers. A driver sends the request to its targets and
/// returns what came back within the window.
pub(crate) trait Driver: Sync {
    /// The member of the class this is.
    fn kind(&self) -> DriverKind;

    /// What the driver aims at, as text for a person and for the browse tree.
    fn targets(&self) -> Vec<String>;

    /// Sends the request, waits for the replies and returns what was heard.
    fn run(&self, window: &Window) -> Result<Collected, IronLinkError>;
}

impl DriverKind {
    /// The driver of this kind for `selection`, or `None` when the selection
    /// gives the kind nothing to do. The match has no wildcard: a member without
    /// an arm does not compile.
    pub(crate) fn build(
        self,
        selection: &Selection,
    ) -> Result<Option<Box<dyn Driver>>, IronLinkError> {
        match self {
            DriverKind::Broadcast => {
                Ok(broadcast::Broadcast::from_selection(selection)?.map(boxed))
            }
            DriverKind::AddressList => {
                Ok(address_list::AddressList::from_selection(selection).map(boxed))
            }
        }
    }
}

fn boxed<D: Driver + 'static>(driver: D) -> Box<dyn Driver> {
    Box::new(driver)
}

#[cfg(test)]
mod tests;
