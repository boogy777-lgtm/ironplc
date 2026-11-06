//! The retained extent: which variable slots and which bytes of the data
//! region hold a value that is meant to survive a restart of the device.
//!
//! The persistent extent ([`crate::PersistentExtent`]) says what is state. A
//! variable declared `RETAIN` or `PERSISTENT` is state whose value the device
//! keeps across a restart, and the type section declares those variables as
//! data, so the runtime and the device read one declaration and never walk
//! the layout themselves (ADR-0085).
//!
//! The declaration is a list of runs. A run is a kind ([`RetainKind`]), the
//! space it indexes ([`RetainSpace`]: the variable table or the data region)
//! and a start and a length in that space. The two spaces are separate
//! address spaces with no pairing between them, so a variable that owns a slot
//! and bytes (a string, an array, a structure, a function block instance) is
//! one run in each, and a field of a function block instance whose type
//! declares it `RETAIN` is a run of bytes only. The writer sorts the runs and
//! merges adjacent runs of one kind, so a container has one byte stream for
//! one declaration.
//!
//! A container that declares no run retains nothing. The one reader is
//! [`Container::retained_runs`](crate::Container::retained_runs); no other
//! site computes which slots or bytes are retained.

use core::ops::Range;
#[cfg(feature = "std")]
use std::vec::Vec;

#[cfg(feature = "std")]
use crate::Container;
use crate::ContainerError;

/// How a retained value survives.
///
/// Both kinds survive a restart of the device and are saved at one. Only
/// [`RetainKind::Persistent`] is also kept across a download whose declaration
/// is equal (as in CODESYS); that rule belongs to the download, not to this
/// declaration.
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Hash)]
#[repr(u8)]
pub enum RetainKind {
    /// Declared `RETAIN`: survives a restart.
    Retain = 1,
    /// Declared `PERSISTENT`: survives a restart and a download.
    Persistent = 2,
}

impl RetainKind {
    /// Decodes a kind byte; an undefined byte is refused.
    pub fn from_u8(v: u8) -> Result<Self, ContainerError> {
        match v {
            1 => Ok(RetainKind::Retain),
            2 => Ok(RetainKind::Persistent),
            _ => Err(ContainerError::InvalidRetainKind(v)),
        }
    }
}

/// The address space a [`RetainedRun`] indexes.
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Hash)]
#[repr(u8)]
pub enum RetainSpace {
    /// Slots of the variable table.
    Slots = 0,
    /// Bytes of the data region.
    Bytes = 1,
}

impl RetainSpace {
    /// Decodes a space byte; an undefined byte is refused.
    pub fn from_u8(v: u8) -> Result<Self, ContainerError> {
        match v {
            0 => Ok(RetainSpace::Slots),
            1 => Ok(RetainSpace::Bytes),
            _ => Err(ContainerError::InvalidRetainSpace(v)),
        }
    }
}

/// One run of retained slots or retained bytes.
///
/// On disk this is 10 bytes: `kind` (u8), `space` (u8), `start` (u32 LE),
/// `len` (u32 LE).
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct RetainedRun {
    /// How the run survives.
    pub kind: RetainKind,
    /// Whether `start` and `len` count variable table slots or data region
    /// bytes.
    pub space: RetainSpace,
    /// Index of the first slot or byte of the run.
    pub start: u32,
    /// Number of slots or bytes in the run.
    pub len: u32,
}

impl RetainedRun {
    /// Serialized size of one run in bytes.
    pub const SIZE: usize = 10;

    /// A run of variable table slots.
    pub fn slots(kind: RetainKind, start: u32, len: u32) -> Self {
        RetainedRun {
            kind,
            space: RetainSpace::Slots,
            start,
            len,
        }
    }

    /// A run of data region bytes.
    pub fn bytes(kind: RetainKind, start: u32, len: u32) -> Self {
        RetainedRun {
            kind,
            space: RetainSpace::Bytes,
            start,
            len,
        }
    }

    /// Decodes a run from its serialized bytes; an undefined kind or space
    /// byte is refused. Whether the run lies inside the persistent extent is
    /// the load check's question.
    pub fn from_bytes(buf: &[u8; Self::SIZE]) -> Result<Self, ContainerError> {
        Ok(RetainedRun {
            kind: RetainKind::from_u8(buf[0])?,
            space: RetainSpace::from_u8(buf[1])?,
            start: u32::from_le_bytes([buf[2], buf[3], buf[4], buf[5]]),
            len: u32::from_le_bytes([buf[6], buf[7], buf[8], buf[9]]),
        })
    }

    /// Encodes the run into its serialized bytes.
    pub fn to_bytes(&self) -> [u8; Self::SIZE] {
        let mut buf = [0u8; Self::SIZE];
        buf[0] = self.kind as u8;
        buf[1] = self.space as u8;
        buf[2..6].copy_from_slice(&self.start.to_le_bytes());
        buf[6..10].copy_from_slice(&self.len.to_le_bytes());
        buf
    }

    /// The end of the run (exclusive), without overflow.
    pub fn end(&self) -> u64 {
        u64::from(self.start) + u64::from(self.len)
    }

    /// The slots or bytes of the run.
    pub fn range(&self) -> Range<usize> {
        let start = self.start as usize;
        start..start + self.len as usize
    }
}

/// The runs in the canonical form the container stores them in: ascending by
/// space and start, empty runs dropped, and adjacent runs of one kind merged.
/// A run that overlaps its predecessor is kept as it is, so that the load
/// check sees the defect of the producer instead of the merge hiding it.
#[cfg(feature = "std")]
pub(crate) fn canonical_runs(mut runs: Vec<RetainedRun>) -> Vec<RetainedRun> {
    runs.retain(|run| run.len != 0);
    runs.sort_by_key(|run| (run.space, run.start));
    let mut merged: Vec<RetainedRun> = Vec::with_capacity(runs.len());
    for run in runs {
        match merged.last_mut() {
            Some(last)
                if last.kind == run.kind
                    && last.space == run.space
                    && last.end() == u64::from(run.start) =>
            {
                last.len = last.len.saturating_add(run.len);
            }
            _ => merged.push(run),
        }
    }
    merged
}

#[cfg(feature = "std")]
impl Container {
    /// The retained runs: the rows the type section declares, which are the
    /// variable table slots and the data region bytes whose value survives a
    /// restart, with the kind of each. A container whose type section declares
    /// no row, or that has no type section, retains nothing.
    ///
    /// This is the one place that answers which part of the state is
    /// retained. The device file, the apply at boot and the download's keep
    /// rule read it here.
    pub fn retained_runs(&self) -> Vec<RetainedRun> {
        self.type_section
            .as_ref()
            .map(|section| section.retained_runs.clone())
            .unwrap_or_default()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn to_bytes_when_decoded_then_roundtrips() {
        let run = RetainedRun::bytes(RetainKind::Persistent, 0x0102_0304, 0x0A0B_0C0D);

        assert_eq!(RetainedRun::from_bytes(&run.to_bytes()).unwrap(), run);
    }

    #[test]
    fn from_bytes_when_kind_undefined_then_invalid_retain_kind() {
        let mut buf = RetainedRun::slots(RetainKind::Retain, 0, 1).to_bytes();
        buf[0] = 9;

        let result = RetainedRun::from_bytes(&buf);

        assert!(matches!(result, Err(ContainerError::InvalidRetainKind(9))));
    }

    #[test]
    fn from_bytes_when_space_undefined_then_invalid_retain_space() {
        let mut buf = RetainedRun::slots(RetainKind::Retain, 0, 1).to_bytes();
        buf[1] = 7;

        let result = RetainedRun::from_bytes(&buf);

        assert!(matches!(result, Err(ContainerError::InvalidRetainSpace(7))));
    }

    #[test]
    fn range_when_run_has_start_and_length_then_covers_the_run() {
        let run = RetainedRun::bytes(RetainKind::Retain, 16, 8);

        assert_eq!(run.range(), 16..24);
        assert_eq!(run.end(), 24);
    }

    #[test]
    fn canonical_runs_when_adjacent_runs_of_one_kind_then_merged() {
        let runs = std::vec![
            RetainedRun::bytes(RetainKind::Retain, 8, 8),
            RetainedRun::bytes(RetainKind::Retain, 0, 8),
            RetainedRun::slots(RetainKind::Retain, 2, 1),
            RetainedRun::slots(RetainKind::Retain, 3, 1),
        ];

        let canonical = canonical_runs(runs);

        assert_eq!(
            canonical,
            std::vec![
                RetainedRun::slots(RetainKind::Retain, 2, 2),
                RetainedRun::bytes(RetainKind::Retain, 0, 16),
            ]
        );
    }

    #[test]
    fn canonical_runs_when_adjacent_runs_of_two_kinds_then_not_merged() {
        let runs = std::vec![
            RetainedRun::slots(RetainKind::Retain, 0, 1),
            RetainedRun::slots(RetainKind::Persistent, 1, 1),
        ];

        assert_eq!(canonical_runs(runs.clone()), runs);
    }

    #[test]
    fn canonical_runs_when_run_is_empty_then_dropped() {
        let runs = std::vec![RetainedRun::bytes(RetainKind::Retain, 4, 0)];

        assert!(canonical_runs(runs).is_empty());
    }

    #[test]
    fn canonical_runs_when_runs_overlap_then_kept_apart() {
        let runs = std::vec![
            RetainedRun::bytes(RetainKind::Retain, 0, 8),
            RetainedRun::bytes(RetainKind::Retain, 4, 8),
        ];

        assert_eq!(canonical_runs(runs.clone()), runs);
    }
}
