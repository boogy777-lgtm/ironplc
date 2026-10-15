//! The layout of the members of a declaration whose byte image the analyzer
//! computes: a structure, a union and a function block.
//!
//! One function ([`layout`]) answers where each member stands and how large
//! the whole is, so a rule about offsets (the alignment of a member, the bound
//! a `pack_mode` puts on it, the space a member occupies) cannot hold in one
//! place and not in another. The byte image is what `SIZEOF` and the byte
//! offsets describe; it is not how the virtual machine stores a value, which
//! is whole 8-byte slots (see ADR-0078).

use ironplc_dsl::attribute::registry::PackMode;

use crate::intermediate_type::IntermediateType;

/// The largest alignment a member can have: the alignment of a 64-bit type.
const NATURAL_ALIGNMENT: u8 = 8;

/// The upper bound on the alignment of the members of a declaration.
///
/// `Natural` is no bound: each member stands at a multiple of its own
/// alignment. `AtMost(n)` is the bound `pack_mode := 'n'` puts on the
/// members: each member stands at a multiple of the smaller of its own
/// alignment and `n`.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum AlignmentBound {
    /// Each member is aligned to its own alignment. This is what a declaration
    /// without `pack_mode` gets.
    ///
    /// Open question for the owner: the default of CODESYS without the
    /// attribute may be 4 or 8; until that is checked on a live CODESYS the
    /// natural alignment is used, which is what the compiler did before the
    /// attribute was honoured.
    Natural,
    /// Each member is aligned to the smaller of its own alignment and the
    /// number of bytes.
    AtMost(u8),
}

impl AlignmentBound {
    /// The alignment a member of alignment `alignment` gets under the bound.
    pub fn apply(self, alignment: u8) -> u8 {
        match self {
            AlignmentBound::Natural => alignment,
            AlignmentBound::AtMost(bound) => alignment.min(bound),
        }
    }
}

impl From<PackMode> for AlignmentBound {
    /// The bound of a `pack_mode`. Mode 0 is mode 1 in CODESYS.
    fn from(mode: PackMode) -> Self {
        match mode {
            PackMode::Mode0 | PackMode::Mode1 => AlignmentBound::AtMost(1),
            PackMode::Mode2 => AlignmentBound::AtMost(2),
            PackMode::Mode4 => AlignmentBound::AtMost(4),
            PackMode::Mode8 => AlignmentBound::AtMost(NATURAL_ALIGNMENT),
        }
    }
}

/// Where the members of a declaration stand, and the extent of the whole.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct Layout {
    /// The byte offset of each member, in the order of the members.
    pub offsets: Vec<u32>,
    /// The alignment of the whole: the largest alignment of a member under the
    /// bound, and 1 when there is no member.
    pub alignment: u8,
    /// The end of the last member, that is the offset where one more member
    /// without alignment would stand. `None` when there is no member or the
    /// space of a member is not known.
    end: Option<u32>,
}

impl Layout {
    /// The number of bytes of the byte image of the whole, which is what
    /// `SIZEOF` answers.
    ///
    /// PROVISIONAL, to be checked on a live CODESYS: the byte image ends with
    /// the last member, without padding up to the alignment of the whole
    /// (mode 8 of the third example of the CODESYS help gives 28, not 32). The
    /// padding applies where the whole repeats or nests, and that is
    /// `IntermediateType::stride_bytes`. To make the byte image end with the
    /// padding, return `self.end.map(|end| align_up(end, self.alignment))`
    /// here; nothing else changes.
    pub fn byte_image_size(&self) -> Option<u32> {
        self.end
    }
}

/// Rounds `offset` up to a multiple of `alignment`; an alignment of 0 leaves
/// it as it is.
pub fn align_up(offset: u32, alignment: u8) -> u32 {
    if alignment == 0 {
        return offset;
    }
    offset.div_ceil(alignment as u32) * alignment as u32
}

/// Lays out members of the given types, in order, under the bound.
///
/// A member stands at the next multiple of the smaller of its alignment and
/// the bound, and occupies its stride (its size padded up to its alignment, so
/// a structure that nests in another occupies the same as one that is an
/// element of an array). A member of unknown space occupies nothing, and the
/// layout then has no end.
pub fn layout<'a>(
    members: impl IntoIterator<Item = &'a IntermediateType>,
    bound: AlignmentBound,
) -> Layout {
    let mut offsets = Vec::new();
    let mut alignment = 1u8;
    let mut next = 0u32;
    let mut known = true;
    let mut any = false;

    for member in members {
        any = true;
        let member_alignment = bound.apply(member.alignment_bytes());
        let offset = align_up(next, member_alignment);
        offsets.push(offset);
        alignment = alignment.max(member_alignment);
        match member.stride_bytes() {
            Some(stride) => next = offset.saturating_add(stride),
            None => {
                known = false;
                next = offset;
            }
        }
    }

    Layout {
        offsets,
        alignment,
        end: (any && known).then_some(next),
    }
}

#[cfg(test)]
mod tests;
