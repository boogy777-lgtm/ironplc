//! The address list: the controller addresses that the address-list driver asks
//! on every browse, in addition to the ones the user names for that browse.
//!
//! Each address is `IP:PORT`; the list is kept unique and sorted (by address,
//! then port, as numbers), so the file is the same whatever the order of the
//! commands that made it.

use std::net::SocketAddr;

use serde::{Deserialize, Serialize};

use super::{Store, StoreFile};
use crate::error::IronLinkError;

/// `addresses.json`.
#[derive(Clone, Debug, Default, PartialEq, Eq, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub(super) struct AddressesFile {
    pub(super) addresses: Vec<SocketAddr>,
}

impl StoreFile for AddressesFile {
    const NAME: &'static str = "addresses.json";
    const WHAT: &'static str = "the address list";

    fn check(&self) -> Result<(), String> {
        match self.addresses.iter().find(|address| address.port() == 0) {
            Some(address) => Err(format!("has the address {address}, whose port is 0")),
            None => Ok(()),
        }
    }
}

impl AddressesFile {
    /// Puts the list in order and without repetition.
    fn normalize(&mut self) {
        self.addresses.sort();
        self.addresses.dedup();
    }
}

impl Store {
    /// The addresses, sorted.
    pub(crate) fn addresses(&self) -> Result<Vec<SocketAddr>, IronLinkError> {
        let mut file = self.read::<AddressesFile>()?;
        file.normalize();
        Ok(file.addresses)
    }

    /// Adds `address`; `false`, and no write, when it was in the list.
    pub(crate) fn add_address(&self, address: SocketAddr) -> Result<bool, IronLinkError> {
        let mut file = self.read::<AddressesFile>()?;
        if file.addresses.contains(&address) {
            return Ok(false);
        }
        file.addresses.push(address);
        file.normalize();
        self.write(&file)?;
        Ok(true)
    }

    /// Removes `address`; `false`, and no write, when it was not in the list.
    pub(crate) fn remove_address(&self, address: SocketAddr) -> Result<bool, IronLinkError> {
        let mut file = self.read::<AddressesFile>()?;
        if !file.addresses.contains(&address) {
            return Ok(false);
        }
        file.addresses.retain(|stored| *stored != address);
        file.normalize();
        self.write(&file)?;
        Ok(true)
    }
}
