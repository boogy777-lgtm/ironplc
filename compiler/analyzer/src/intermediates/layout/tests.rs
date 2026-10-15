//! Tests for the layout of the members of a declaration.

use ironplc_dsl::attribute::registry::PackMode;
use rstest::rstest;

use super::*;
use crate::intermediate_type::{ByteSized, IntermediateStructField};
use ironplc_dsl::core::Id;

fn bool_type() -> IntermediateType {
    IntermediateType::Bool
}

fn byte() -> IntermediateType {
    IntermediateType::Bytes {
        size: ByteSized::B8,
    }
}

fn int() -> IntermediateType {
    IntermediateType::Int {
        size: ByteSized::B16,
    }
}

fn word() -> IntermediateType {
    IntermediateType::Bytes {
        size: ByteSized::B16,
    }
}

fn dword() -> IntermediateType {
    IntermediateType::Bytes {
        size: ByteSized::B32,
    }
}

fn lword() -> IntermediateType {
    IntermediateType::Bytes {
        size: ByteSized::B64,
    }
}

fn field(name: &str, field_type: IntermediateType, offset: u32) -> IntermediateStructField {
    IntermediateStructField {
        name: Id::from(name),
        field_type,
        offset,
        var_type: None,
        initial_value: None,
    }
}

/// The first example of the CODESYS help for `pack_mode`.
fn example_1() -> Vec<IntermediateType> {
    vec![bool_type(), int(), bool_type(), bool_type()]
}

/// The second example of the CODESYS help for `pack_mode`.
fn example_2() -> Vec<IntermediateType> {
    vec![
        bool_type(),
        byte(),
        word(),
        byte(),
        dword(),
        byte(),
        byte(),
        dword(),
    ]
}

/// The third example of the CODESYS help for `pack_mode`.
fn example_3() -> Vec<IntermediateType> {
    vec![
        byte(),
        lword(),
        byte(),
        byte(),
        dword(),
        byte(),
        byte(),
        word(),
    ]
}

// The offsets and sizes of the examples of the help: the help gives the offset
// of one member and the size for each mode; the other offsets follow from the
// rule and are checked for consistency with those.
#[rstest]
#[case::example_1_mode_0(example_1(), PackMode::Mode0, vec![0, 1, 3, 4], 5)]
#[case::example_1_mode_1(example_1(), PackMode::Mode1, vec![0, 1, 3, 4], 5)]
#[case::example_1_mode_2(example_1(), PackMode::Mode2, vec![0, 2, 4, 5], 6)]
#[case::example_1_mode_4(example_1(), PackMode::Mode4, vec![0, 2, 4, 5], 6)]
#[case::example_1_mode_8(example_1(), PackMode::Mode8, vec![0, 2, 4, 5], 6)]
#[case::example_2_mode_0(example_2(), PackMode::Mode0, vec![0, 1, 2, 4, 5, 9, 10, 11], 15)]
#[case::example_2_mode_1(example_2(), PackMode::Mode1, vec![0, 1, 2, 4, 5, 9, 10, 11], 15)]
#[case::example_2_mode_2(example_2(), PackMode::Mode2, vec![0, 1, 2, 4, 6, 10, 11, 12], 16)]
#[case::example_2_mode_4(example_2(), PackMode::Mode4, vec![0, 1, 2, 4, 8, 12, 13, 16], 20)]
#[case::example_2_mode_8(example_2(), PackMode::Mode8, vec![0, 1, 2, 4, 8, 12, 13, 16], 20)]
#[case::example_3_mode_0(example_3(), PackMode::Mode0, vec![0, 1, 9, 10, 11, 15, 16, 17], 19)]
#[case::example_3_mode_1(example_3(), PackMode::Mode1, vec![0, 1, 9, 10, 11, 15, 16, 17], 19)]
#[case::example_3_mode_2(example_3(), PackMode::Mode2, vec![0, 2, 10, 11, 12, 16, 17, 18], 20)]
#[case::example_3_mode_4(example_3(), PackMode::Mode4, vec![0, 4, 12, 13, 16, 20, 21, 22], 24)]
#[case::example_3_mode_8(example_3(), PackMode::Mode8, vec![0, 8, 16, 17, 20, 24, 25, 26], 28)]
fn layout_when_examples_of_the_help_then_offsets_and_size_of_the_mode(
    #[case] members: Vec<IntermediateType>,
    #[case] mode: PackMode,
    #[case] offsets: Vec<u32>,
    #[case] size: u32,
) {
    let laid_out = layout(&members, AlignmentBound::from(mode));

    assert_eq!(laid_out.offsets, offsets);
    assert_eq!(laid_out.byte_image_size(), Some(size));
}

#[test]
fn layout_when_no_bound_then_each_member_stands_at_its_own_alignment() {
    let members = example_3();

    let laid_out = layout(&members, AlignmentBound::Natural);

    assert_eq!(laid_out.offsets, vec![0, 8, 16, 17, 20, 24, 25, 26]);
    assert_eq!(laid_out.alignment, 8);
    assert_eq!(laid_out.byte_image_size(), Some(28));
}

#[rstest]
#[case::mode_0(PackMode::Mode0, 1)]
#[case::mode_1(PackMode::Mode1, 1)]
#[case::mode_2(PackMode::Mode2, 2)]
#[case::mode_4(PackMode::Mode4, 4)]
#[case::mode_8(PackMode::Mode8, 8)]
fn layout_when_mode_then_alignment_of_whole_is_the_smaller_of_largest_member_and_mode(
    #[case] mode: PackMode,
    #[case] alignment: u8,
) {
    let members = example_3();

    let laid_out = layout(&members, AlignmentBound::from(mode));

    assert_eq!(laid_out.alignment, alignment);
}

#[test]
fn layout_when_mode_is_above_the_largest_member_then_alignment_of_whole_is_the_largest_member() {
    let members = vec![byte(), int()];

    let laid_out = layout(&members, AlignmentBound::from(PackMode::Mode8));

    assert_eq!(laid_out.alignment, 2);
}

#[test]
fn layout_when_no_member_then_no_size_and_alignment_one() {
    let members: Vec<IntermediateType> = vec![];

    let laid_out = layout(&members, AlignmentBound::Natural);

    assert!(laid_out.offsets.is_empty());
    assert_eq!(laid_out.alignment, 1);
    assert_eq!(laid_out.byte_image_size(), None);
}

#[test]
fn layout_when_member_has_no_known_size_then_no_size() {
    let members = vec![
        byte(),
        IntermediateType::String {
            max_len: None,
            char_width: ironplc_container::CharWidth::Narrow,
        },
        byte(),
    ];

    let laid_out = layout(&members, AlignmentBound::Natural);

    assert_eq!(laid_out.byte_image_size(), None);
}

#[test]
fn layout_when_member_is_a_structure_then_it_occupies_its_stride_not_its_byte_image() {
    // The inner structure is a LWORD and a BYTE: its byte image is 9 bytes, it
    // is 8-aligned, so it occupies 16 where it nests.
    let inner = IntermediateType::Structure {
        fields: vec![field("a", lword(), 0), field("b", byte(), 8)],
        bound: AlignmentBound::Natural,
    };
    assert_eq!(inner.size_in_bytes(), Some(9));
    let members = vec![inner, byte()];

    let laid_out = layout(&members, AlignmentBound::Natural);

    assert_eq!(laid_out.offsets, vec![0, 16]);
    assert_eq!(laid_out.byte_image_size(), Some(17));
}

#[test]
fn layout_when_member_is_a_structure_under_a_bound_then_the_member_alignment_is_bounded() {
    let inner = IntermediateType::Structure {
        fields: vec![field("a", lword(), 0)],
        bound: AlignmentBound::Natural,
    };
    let members = vec![byte(), inner];

    let laid_out = layout(&members, AlignmentBound::from(PackMode::Mode2));

    assert_eq!(laid_out.offsets, vec![0, 2]);
}

#[rstest]
#[case::mode_0(PackMode::Mode0, AlignmentBound::AtMost(1))]
#[case::mode_1(PackMode::Mode1, AlignmentBound::AtMost(1))]
#[case::mode_2(PackMode::Mode2, AlignmentBound::AtMost(2))]
#[case::mode_4(PackMode::Mode4, AlignmentBound::AtMost(4))]
#[case::mode_8(PackMode::Mode8, AlignmentBound::AtMost(8))]
fn alignment_bound_from_pack_mode_then_the_bound_of_the_mode(
    #[case] mode: PackMode,
    #[case] bound: AlignmentBound,
) {
    assert_eq!(AlignmentBound::from(mode), bound);
}

#[rstest]
#[case::already_aligned(8, 4, 8)]
#[case::rounds_up(9, 4, 12)]
#[case::alignment_one(5, 1, 5)]
#[case::alignment_zero(5, 0, 5)]
#[case::zero(0, 8, 0)]
fn align_up_with_offset_and_alignment_then_next_multiple(
    #[case] offset: u32,
    #[case] alignment: u8,
    #[case] expected: u32,
) {
    assert_eq!(align_up(offset, alignment), expected);
}
