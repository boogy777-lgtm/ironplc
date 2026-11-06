//! The retained image of a host: the values of the retained variables, as a
//! file can keep them across a restart (ADR-0085).
//!
//! A container declares which slots of its variable table and which bytes of
//! its data region hold a retained value ([`Container::retained_runs`]). This
//! module reads those values out of a host's buffers as a [`RetainedImage`]
//! and writes an image back, walking the runs with the code that moves the
//! persistent part ([`persistent_state`](crate::persistent_state)); there is
//! no second layout walker. A selection of the runs (for instance the kind
//! `Persistent` only, which a download keeps) is a filter in [`runs`], not
//! new walking code.
//!
//! An image carries the [`RetainedTag`] of the container it was read from and
//! is applied only to a container with an equal tag, otherwise nothing is
//! written. The tag is the content hash of the container, which says the code
//! and the layout are the same, and the hash of the retained rows, which says
//! which slots and bytes the image holds. [`encode`](RetainedImage::encode)
//! and [`decode`](RetainedImage::decode) give the bytes that the device file
//! stores as they are.

use core::fmt;

use ironplc_container::integrity::HASH_LEN;
use ironplc_container::{Container, RetainSpace};
use ironplc_vm::VmBuffers;

use crate::persistent_state::{read_runs, write_runs, Run};

/// Which container a retained image belongs to.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct RetainedTag {
    /// The `content_hash` of the container's header, as the host loaded it.
    pub content_hash: [u8; HASH_LEN],
    /// [`Container::retained_runs_hash`]: which slots and bytes are retained.
    pub runs_hash: [u8; HASH_LEN],
}

impl RetainedTag {
    /// The tag of `container`. This is the one place that builds a tag.
    pub fn of(container: &Container) -> Self {
        RetainedTag {
            content_hash: container.header.content_hash,
            runs_hash: container.retained_runs_hash(),
        }
    }
}

/// Why an image was not applied. Nothing was written.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum RetainedMismatch {
    /// The image was read from a container with another content hash.
    ContentHash,
    /// The image was read from a container that retains other slots or bytes,
    /// or another kind of them.
    Runs,
    /// The image does not hold as many slots or bytes as the container
    /// retains: the image is corrupt.
    Length,
}

impl fmt::Display for RetainedMismatch {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            RetainedMismatch::ContentHash => {
                write!(f, "retained image belongs to another container")
            }
            RetainedMismatch::Runs => write!(
                f,
                "retained image holds other variables than the container retains"
            ),
            RetainedMismatch::Length => write!(
                f,
                "retained image does not have the length of the retained runs"
            ),
        }
    }
}

/// Why a host did not apply a retained image.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum RetainedApplyError {
    /// The host has run a scan round: the warm start is over and the image
    /// would overwrite live state.
    AfterFirstRound,
    /// The image does not fit the active container.
    Mismatch(RetainedMismatch),
}

impl fmt::Display for RetainedApplyError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            RetainedApplyError::AfterFirstRound => write!(
                f,
                "retained image can only be applied before the first scan round"
            ),
            RetainedApplyError::Mismatch(reason) => reason.fmt(f),
        }
    }
}

/// Why bytes are not a retained image.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum RetainedDecodeError {
    /// The bytes do not start with the magic of a retained image.
    BadMagic,
    /// The version byte is not one this reader knows.
    UnsupportedVersion(u8),
    /// The bytes end before the image does.
    Truncated,
    /// The bytes continue after the image.
    TrailingBytes,
}

impl fmt::Display for RetainedDecodeError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            RetainedDecodeError::BadMagic => write!(f, "not a retained image"),
            RetainedDecodeError::UnsupportedVersion(version) => {
                write!(f, "unsupported retained image version {version}")
            }
            RetainedDecodeError::Truncated => write!(f, "retained image is truncated"),
            RetainedDecodeError::TrailingBytes => {
                write!(f, "retained image has bytes after its end")
            }
        }
    }
}

/// The retained values of one host.
///
/// `vars` are the retained slots as raw bits and `data` the retained bytes of
/// the data region, each in the order of the container's retained runs.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct RetainedImage {
    /// The container the image was read from.
    pub tag: RetainedTag,
    /// The retained slots, in run order.
    pub vars: Vec<u64>,
    /// The retained bytes of the data region, in run order.
    pub data: Vec<u8>,
}

/// The first bytes of an encoded image.
const MAGIC: [u8; 4] = *b"IPRI";
/// The version of the encoding.
const VERSION: u8 = 1;
/// Bytes of one encoded slot.
const SLOT_LEN: usize = 8;

/// The retained runs of `container` as runs of the buffers.
///
/// A selection of kinds is one `filter` on the rows here.
fn runs(container: &Container) -> Vec<Run> {
    container
        .retained_runs()
        .iter()
        .map(|row| match row.space {
            RetainSpace::Slots => Run {
                vars: row.range(),
                data: 0..0,
            },
            RetainSpace::Bytes => Run {
                vars: 0..0,
                data: row.range(),
            },
        })
        .collect()
}

impl RetainedImage {
    /// The retained values in `buffers`, which `container` laid out.
    pub(crate) fn read(container: &Container, buffers: &VmBuffers) -> Self {
        let (vars, data) = read_runs(&runs(container), buffers);
        RetainedImage {
            tag: RetainedTag::of(container),
            vars,
            data,
        }
    }

    /// Writes the image over the retained values of `buffers`, which
    /// `container` laid out. Nothing is written, and the reason is returned,
    /// when the image belongs to another container or is corrupt.
    pub(crate) fn apply(
        &self,
        container: &Container,
        buffers: &mut VmBuffers,
    ) -> Result<(), RetainedMismatch> {
        let tag = RetainedTag::of(container);
        if self.tag.content_hash != tag.content_hash {
            return Err(RetainedMismatch::ContentHash);
        }
        if self.tag.runs_hash != tag.runs_hash {
            return Err(RetainedMismatch::Runs);
        }
        if write_runs(&runs(container), &self.vars, &self.data, buffers) {
            Ok(())
        } else {
            Err(RetainedMismatch::Length)
        }
    }

    /// The image as the bytes a file keeps, little endian: the magic `IPRI`,
    /// a version byte (1), the content hash and the runs hash (32 bytes each),
    /// the number of slots and the number of bytes (u32 each), the slots (u64
    /// each) and the bytes.
    pub fn encode(&self) -> Vec<u8> {
        let mut out = Vec::with_capacity(
            MAGIC.len() + 1 + 2 * HASH_LEN + 8 + self.vars.len() * SLOT_LEN + self.data.len(),
        );
        out.extend_from_slice(&MAGIC);
        out.push(VERSION);
        out.extend_from_slice(&self.tag.content_hash);
        out.extend_from_slice(&self.tag.runs_hash);
        out.extend_from_slice(&count(self.vars.len()).to_le_bytes());
        out.extend_from_slice(&count(self.data.len()).to_le_bytes());
        for raw in &self.vars {
            out.extend_from_slice(&raw.to_le_bytes());
        }
        out.extend_from_slice(&self.data);
        out
    }

    /// Reads bytes made by [`encode`](Self::encode). Short, long or garbled
    /// input is an error, never a panic. Whether the image fits a container
    /// is the question of
    /// [`RuntimeHost::apply_retained`](crate::RuntimeHost::apply_retained).
    pub fn decode(bytes: &[u8]) -> Result<Self, RetainedDecodeError> {
        let mut input = Input(bytes);
        if input.array::<4>()? != MAGIC {
            return Err(RetainedDecodeError::BadMagic);
        }
        let [version] = input.array::<1>()?;
        if version != VERSION {
            return Err(RetainedDecodeError::UnsupportedVersion(version));
        }
        let tag = RetainedTag {
            content_hash: input.array()?,
            runs_hash: input.array()?,
        };
        let var_count = u32::from_le_bytes(input.array()?) as usize;
        let data_len = u32::from_le_bytes(input.array()?) as usize;
        let slot_bytes = var_count
            .checked_mul(SLOT_LEN)
            .ok_or(RetainedDecodeError::Truncated)?;
        let vars = input
            .take(slot_bytes)?
            .as_chunks::<SLOT_LEN>()
            .0
            .iter()
            .map(|chunk| u64::from_le_bytes(*chunk))
            .collect();
        let data = input.take(data_len)?.to_vec();
        if !input.0.is_empty() {
            return Err(RetainedDecodeError::TrailingBytes);
        }
        Ok(RetainedImage { tag, vars, data })
    }
}

/// A length as the u32 the encoding keeps. A host's buffers are far smaller;
/// the saturation only keeps the encoder total.
fn count(len: usize) -> u32 {
    u32::try_from(len).unwrap_or(u32::MAX)
}

/// The unread rest of the bytes being decoded.
struct Input<'a>(&'a [u8]);

impl<'a> Input<'a> {
    fn take(&mut self, len: usize) -> Result<&'a [u8], RetainedDecodeError> {
        if self.0.len() < len {
            return Err(RetainedDecodeError::Truncated);
        }
        let (head, rest) = self.0.split_at(len);
        self.0 = rest;
        Ok(head)
    }

    fn array<const N: usize>(&mut self) -> Result<[u8; N], RetainedDecodeError> {
        self.take(N).map(array_of)
    }
}

/// `bytes` as an array; the caller has sized it to `N`.
fn array_of<const N: usize>(bytes: &[u8]) -> [u8; N] {
    let mut out = [0u8; N];
    out.copy_from_slice(bytes);
    out
}

#[cfg(test)]
mod tests {
    use ironplc_container::{
        ContainerBuilder, InstanceId, PersistentExtent, RetainKind, RetainedRun,
    };
    use ironplc_vm::Slot;

    use super::*;

    /// Five variables and 32 data bytes, all persistent; variable 2 is
    /// `Retain`, bytes 8..16 are `Persistent`.
    fn container(content_hash: u8, runs: &[RetainedRun]) -> Container {
        let mut builder = ContainerBuilder::new()
            .num_variables(5)
            .data_region_bytes(32)
            .add_persistent_extent(PersistentExtent {
                instance_id: InstanceId::DEFAULT,
                var_start: 0,
                var_count: 5,
                data_start: 0,
                data_len: 32,
            });
        for run in runs {
            builder = builder.add_retained_run(*run);
        }
        let mut container = builder.build();
        container.header.content_hash = [content_hash; HASH_LEN];
        container
    }

    fn declared() -> [RetainedRun; 2] {
        [
            RetainedRun::slots(RetainKind::Retain, 2, 1),
            RetainedRun::bytes(RetainKind::Persistent, 8, 8),
        ]
    }

    fn buffers(container: &Container, fill: u8) -> VmBuffers {
        let mut buffers = VmBuffers::from_container(container);
        for (index, slot) in buffers.vars.iter_mut().enumerate() {
            *slot = Slot::from_u64(u64::from(fill) * 10 + index as u64);
        }
        buffers.data_region.fill(fill);
        buffers
    }

    #[test]
    fn read_when_runs_declared_then_holds_only_the_retained_slots_and_bytes() {
        let container = container(1, &declared());

        let image = RetainedImage::read(&container, &buffers(&container, 3));

        assert_eq!(image.vars, vec![32]);
        assert_eq!(image.data, vec![3u8; 8]);
        assert_eq!(image.tag, RetainedTag::of(&container));
    }

    #[test]
    fn read_when_no_runs_declared_then_image_is_empty() {
        let container = container(1, &[]);

        let image = RetainedImage::read(&container, &buffers(&container, 3));

        assert!(image.vars.is_empty() && image.data.is_empty());
    }

    #[test]
    fn apply_when_tag_matches_then_writes_only_the_retained_slots_and_bytes() {
        let container = container(1, &declared());
        let image = RetainedImage::read(&container, &buffers(&container, 3));
        let mut target = buffers(&container, 5);

        let result = image.apply(&container, &mut target);

        assert_eq!(result, Ok(()));
        let slots: Vec<u64> = target.vars.iter().map(|slot| slot.as_u64()).collect();
        assert_eq!(slots, vec![50, 51, 32, 53, 54]);
        assert_eq!(&target.data_region[..8], &[5u8; 8]);
        assert_eq!(&target.data_region[8..16], &[3u8; 8]);
        assert_eq!(&target.data_region[16..], &[5u8; 16]);
    }

    #[test]
    fn apply_when_content_hash_differs_then_refused_and_nothing_written() {
        let source = container(1, &declared());
        let image = RetainedImage::read(&source, &buffers(&source, 3));
        let other = container(2, &declared());
        let mut target = buffers(&other, 5);

        let result = image.apply(&other, &mut target);

        assert_eq!(result, Err(RetainedMismatch::ContentHash));
        assert_eq!(target.vars[2].as_u64(), 52);
        assert_eq!(target.data_region[8], 5);
    }

    #[test]
    fn apply_when_runs_differ_then_refused_and_nothing_written() {
        let source = container(1, &declared());
        let image = RetainedImage::read(&source, &buffers(&source, 3));
        let other = container(
            1,
            &[
                RetainedRun::slots(RetainKind::Persistent, 2, 1),
                RetainedRun::bytes(RetainKind::Persistent, 8, 8),
            ],
        );
        let mut target = buffers(&other, 5);

        let result = image.apply(&other, &mut target);

        assert_eq!(result, Err(RetainedMismatch::Runs));
        assert_eq!(target.vars[2].as_u64(), 52);
    }

    #[test]
    fn apply_when_image_has_other_lengths_then_refused_and_nothing_written() {
        let container = container(1, &declared());
        let mut image = RetainedImage::read(&container, &buffers(&container, 3));
        image.vars.push(9);
        let mut target = buffers(&container, 5);

        let result = image.apply(&container, &mut target);

        assert_eq!(result, Err(RetainedMismatch::Length));
        assert_eq!(target.vars[2].as_u64(), 52);
        assert_eq!(target.data_region[8], 5);
    }

    fn image() -> RetainedImage {
        RetainedImage {
            tag: RetainedTag {
                content_hash: [7; HASH_LEN],
                runs_hash: [8; HASH_LEN],
            },
            vars: vec![1, u64::MAX],
            data: vec![9, 8, 7],
        }
    }

    #[test]
    fn decode_when_encoded_then_same_image() {
        let encoded = image().encode();

        assert_eq!(RetainedImage::decode(&encoded), Ok(image()));
    }

    #[test]
    fn encode_when_image_then_little_endian_with_magic_and_version() {
        let encoded = image().encode();

        assert_eq!(&encoded[..5], b"IPRI\x01");
        assert_eq!(&encoded[69..73], &2u32.to_le_bytes());
        assert_eq!(&encoded[73..77], &3u32.to_le_bytes());
        assert_eq!(&encoded[77..85], &1u64.to_le_bytes());
        assert_eq!(encoded.len(), 77 + 16 + 3);
    }

    #[test]
    fn decode_when_truncated_at_any_length_then_error() {
        let encoded = image().encode();

        for len in 0..encoded.len() {
            assert!(RetainedImage::decode(&encoded[..len]).is_err(), "{len}");
        }
    }

    #[test]
    fn decode_when_bytes_follow_the_image_then_trailing_bytes() {
        let mut encoded = image().encode();
        encoded.push(0);

        assert_eq!(
            RetainedImage::decode(&encoded),
            Err(RetainedDecodeError::TrailingBytes)
        );
    }

    #[test]
    fn decode_when_magic_or_version_is_wrong_then_error() {
        let mut bad_magic = image().encode();
        bad_magic[0] = b'X';
        let mut bad_version = image().encode();
        bad_version[4] = 2;

        assert_eq!(
            RetainedImage::decode(&bad_magic),
            Err(RetainedDecodeError::BadMagic)
        );
        assert_eq!(
            RetainedImage::decode(&bad_version),
            Err(RetainedDecodeError::UnsupportedVersion(2))
        );
    }

    #[test]
    fn decode_when_counts_are_huge_then_truncated() {
        let mut encoded = image().encode();
        encoded[69..73].copy_from_slice(&u32::MAX.to_le_bytes());
        encoded[73..77].copy_from_slice(&u32::MAX.to_le_bytes());

        assert_eq!(
            RetainedImage::decode(&encoded),
            Err(RetainedDecodeError::Truncated)
        );
    }
}
