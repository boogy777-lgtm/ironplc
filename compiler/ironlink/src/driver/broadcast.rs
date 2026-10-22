//! The broadcast driver: the request goes to the limited broadcast address
//! `255.255.255.255` on the discovery port, from a socket bound to each chosen
//! local interface, so that the broadcast leaves through that interface and
//! the controllers on its network answer (ADR-0084).
//!
//! The user names an interface by a local address of this PC (`192.168.1.20`);
//! `0.0.0.0` lets the operating system choose. Listing the interfaces would
//! need a dependency the standard library lacks. IPv6 has no broadcast, so an
//! IPv6 interface is refused.

use std::io;
use std::net::{IpAddr, Ipv4Addr, SocketAddr, UdpSocket};

use super::{Driver, DriverKind, Selection};
use crate::collect::{collect, Channel, Collected, Window};
use crate::error::IronLinkError;

/// The broadcast from one or more local interfaces.
pub(crate) struct Broadcast {
    interfaces: Vec<Ipv4Addr>,
    destination: SocketAddr,
}

impl Broadcast {
    /// A driver that sends from each of `interfaces` to `destination`. The
    /// destination is a parameter so that it can be aimed at a loopback fake;
    /// [`Broadcast::from_selection`] always uses the limited broadcast address.
    pub(crate) fn aimed_at(
        interfaces: &[IpAddr],
        destination: SocketAddr,
    ) -> Result<Self, IronLinkError> {
        let interfaces = interfaces
            .iter()
            .map(|interface| match interface {
                IpAddr::V4(v4) => Ok(*v4),
                IpAddr::V6(v6) => Err(IronLinkError::argument_invalid(format!(
                    "the interface {v6} is IPv6, and IPv6 has no broadcast. \
                     Name an IPv4 address of this PC, or use an address list"
                ))),
            })
            .collect::<Result<Vec<Ipv4Addr>, IronLinkError>>()?;
        Ok(Broadcast {
            interfaces,
            destination,
        })
    }

    /// The interfaces of the selection. A selection that names no interface and
    /// no address at all means the default: the broadcast through the interface
    /// the operating system picks. One that names only addresses leaves the
    /// broadcast out.
    pub(super) fn from_selection(selection: &Selection) -> Result<Option<Self>, IronLinkError> {
        let limited_broadcast = SocketAddr::new(IpAddr::V4(Ipv4Addr::BROADCAST), selection.port);
        match (
            selection.interfaces.is_empty(),
            selection.addresses.is_empty(),
        ) {
            (true, true) => {
                Self::aimed_at(&[IpAddr::V4(Ipv4Addr::UNSPECIFIED)], limited_broadcast).map(Some)
            }
            (true, false) => Ok(None),
            (false, _) => Self::aimed_at(&selection.interfaces, limited_broadcast).map(Some),
        }
    }

    /// One channel for each interface, from a broadcast-enabled socket bound to it.
    fn open(&self) -> Result<Vec<Channel>, IronLinkError> {
        self.interfaces
            .iter()
            .map(|interface| {
                let bind_err =
                    |err: io::Error| IronLinkError::interface_bind(IpAddr::V4(*interface), &err);
                let socket = UdpSocket::bind((*interface, 0)).map_err(bind_err)?;
                socket.set_broadcast(true).map_err(bind_err)?;
                Ok(Channel {
                    socket,
                    destinations: vec![self.destination],
                })
            })
            .collect()
    }
}

impl Driver for Broadcast {
    fn kind(&self) -> DriverKind {
        DriverKind::Broadcast
    }

    fn targets(&self) -> Vec<String> {
        self.interfaces
            .iter()
            .map(|interface| format!("{interface} -> {}", self.destination))
            .collect()
    }

    fn run(&self, window: &Window) -> Result<Collected, IronLinkError> {
        collect(self.kind(), &self.open()?, window)
    }
}
