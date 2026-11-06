//! The empty application: the container of a device that holds no program.
//!
//! "No application" is not a state of the device, it is an application: a
//! container with no program instances and no tasks. A device that has not been
//! given a program boots this container, and the host runs it like any other
//! (a round that finds no task to execute does nothing). That a controller
//! "holds no application" is therefore derived, never stored: its `contentHash`
//! equals [`EMPTY_APPLICATION_HASH`].
//!
//! The container is built here and nowhere else. Its content hash is a constant
//! so that a client can compare it without building the container, and a test
//! checks the constant against the hash of the built container: when the format
//! changes (a new [`FORMAT_VERSION`](crate::FORMAT_VERSION), a new header
//! field), the test fails and the constant is updated in the same change.

use crate::integrity::HASH_LEN;

/// The content hash of the empty application. A controller whose application has
/// this content hash holds no application.
pub const EMPTY_APPLICATION_HASH: [u8; HASH_LEN] = [
    0xf3, 0x63, 0xb8, 0xd3, 0x3c, 0x67, 0xe1, 0x63, 0xac, 0xb0, 0xb0, 0xa6, 0x69, 0xa6, 0xd9, 0xee,
    0x5b, 0x3f, 0x81, 0xe1, 0x3b, 0x59, 0x85, 0x8a, 0x92, 0x34, 0xa8, 0x50, 0x08, 0xdc, 0xb1, 0xc4,
];

#[cfg(feature = "std")]
mod build {
    use std::io::Cursor;
    use std::vec::Vec;

    use crate::code_section::CodeSection;
    use crate::constant_pool::ConstantPool;
    use crate::container::Container;
    use crate::header::FileHeader;
    use crate::task_table::TaskTable;
    use crate::ContainerError;

    /// The call depth the empty application declares. The loader refuses a
    /// container that declares none (V9017: the field was never computed), and
    /// the empty application's longest call path is the entry frame the host
    /// reserves, so it declares that one frame.
    const EMPTY_CALL_DEPTH: u16 = 1;

    /// The wire bytes of the empty application: no task, no program instance,
    /// no variable, no function, no constant, no type section.
    pub fn empty_application_bytes() -> Result<Vec<u8>, ContainerError> {
        let container = Container {
            header: FileHeader {
                max_call_depth: EMPTY_CALL_DEPTH,
                ..FileHeader::default()
            },
            task_table: TaskTable {
                shared_globals_size: 0,
                tasks: Vec::new(),
                programs: Vec::new(),
            },
            type_section: None,
            constant_pool: ConstantPool::default(),
            code: CodeSection::default(),
            debug_section: None,
        };
        let mut bytes = Vec::new();
        container.write_to(&mut bytes)?;
        Ok(bytes)
    }

    /// The empty application as a loaded container: read from its wire bytes
    /// through the same call every container is read with, so it has passed the
    /// content-hash check and the load-time verification, and its header carries
    /// the hash a host reports.
    pub fn empty_application() -> Result<Container, ContainerError> {
        Container::read_from(&mut Cursor::new(empty_application_bytes()?))
    }
}

#[cfg(feature = "std")]
pub use build::{empty_application, empty_application_bytes};
