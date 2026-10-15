//! Tests that byte offsets in the combined CDATA text map back to offsets in the XML document.

use super::*;

/// A declaration of 50 bytes at XML offset 100, then a newline, then an
/// implementation at XML offset 200 — the layout `parse_pou` builds for a
/// POU that has an implementation.
fn declaration_and_implementation_offsets() -> CdataOffsets {
    CdataOffsets {
        segments: vec![
            CdataSegment {
                combined_start: 0,
                len: 50,
                xml_start: 100,
            },
            CdataSegment {
                combined_start: 51,
                len: 20,
                xml_start: 200,
            },
        ],
    }
}

#[test]
fn adjust_byte_offset_when_pos_in_implementation_then_adjusts_correctly() {
    let offsets = declaration_and_implementation_offsets();

    // Position 0 is in declaration: 0 + 100 = 100
    assert_eq!(adjust_byte_offset(&offsets, 0), 100);

    // Position 50 (= declaration length) is in declaration: 50 + 100 = 150
    assert_eq!(adjust_byte_offset(&offsets, 50), 150);

    // Position 52 is in implementation: (52 - 51) + 200 = 201
    assert_eq!(adjust_byte_offset(&offsets, 52), 201);
}

#[test]
fn adjust_byte_offset_when_pos_past_last_segment_then_points_to_its_end() {
    let offsets = declaration_and_implementation_offsets();

    // Position 90 is in the synthetic closing keyword, past every
    // segment: it maps to the end of the implementation.
    assert_eq!(adjust_byte_offset(&offsets, 90), 220);
}

#[test]
fn adjust_byte_offset_when_no_implementation_and_pos_past_declaration_then_points_to_end() {
    let offsets = CdataOffsets {
        segments: vec![CdataSegment {
            combined_start: 0,
            len: 50,
            xml_start: 100,
        }],
    };

    // Position beyond the declaration with no implementation: returns the
    // end of the declaration.
    assert_eq!(adjust_byte_offset(&offsets, 60), 150);
}

#[test]
fn adjust_byte_offset_when_no_segments_then_returns_zero() {
    let offsets = CdataOffsets { segments: vec![] };

    assert_eq!(adjust_byte_offset(&offsets, 0), 0);
    assert_eq!(adjust_byte_offset(&offsets, 42), 0);
}
