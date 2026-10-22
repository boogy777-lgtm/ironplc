//! The address list driver: the request goes to each configured controller
//! address by unicast. It reaches what a broadcast cannot, the controllers
//! behind a router (ADR-0084; RSLinx's "Ethernet devices" driver does the same).
//!
//! The request leaves from one socket bound to the wildcard address of the
//! family of the targets (`0.0.0.0:0` for IPv4, `[::]:0` for IPv6), and the
//! operating system picks the interface by its routing table.

use std::net::{IpAddr, Ipv4Addr, Ipv6Addr, SocketAddr, UdpSocket};

use super::{Driver, DriverKind, Selection};
use crate::collect::{collect, Channel, Collected, Window};
use crate::error::IronLinkError;

/// The controllers named by address.
pub(crate) struct AddressList {
    targets: Vec<SocketAddr>,
}

impl AddressList {
    /// A driver for `targets`.
    pub(crate) fn new(targets: Vec<SocketAddr>) -> Self {
        AddressList { targets }
    }

    /// The addresses named for this browse, then the stored ones that were not
    /// named, each once; `None` when there are none.
    pub(super) fn from_selection(selection: &Selection) -> Option<Self> {
        let mut targets = selection.addresses.clone();
        for address in &selection.remembered {
            if !targets.contains(address) {
                targets.push(*address);
            }
        }
        (!targets.is_empty()).then(|| Self::new(targets))
    }

    /// One channel for each address family among the targets, each from a
    /// socket bound to the wildcard address of its family.
    fn open(&self) -> Result<Vec<Channel>, IronLinkError> {
        let (v4, v6): (Vec<SocketAddr>, Vec<SocketAddr>) =
            self.targets.iter().copied().partition(SocketAddr::is_ipv4);
        [
            (IpAddr::V4(Ipv4Addr::UNSPECIFIED), v4),
            (IpAddr::V6(Ipv6Addr::UNSPECIFIED), v6),
        ]
        .into_iter()
        .filter(|(_, destinations)| !destinations.is_empty())
        .map(|(wildcard, destinations)| {
            let socket = UdpSocket::bind((wildcard, 0))
                .map_err(|err| IronLinkError::interface_bind(wildcard, &err))?;
            Ok(Channel {
                socket,
                destinations,
            })
        })
        .collect()
    }
}

impl Driver for AddressList {
    fn kind(&self) -> DriverKind {
        DriverKind::AddressList
    }

    fn targets(&self) -> Vec<String> {
        self.targets.iter().map(ToString::to_string).collect()
    }

    fn run(&self, window: &Window) -> Result<Collected, IronLinkError> {
        collect(self.kind(), &self.open()?, window)
    }
}
