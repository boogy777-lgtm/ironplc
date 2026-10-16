//! The range of a diagnostic as an editor draws it.
//!
//! A diagnostic that says something is missing carries an empty span: the
//! exact place where it belongs. An editor draws nothing for an empty range,
//! and at the end of a line or of the file there is no character to hold it, so
//! every conversion that hands a range to an editor goes through
//! [`LineColumn::editor_range`]. The span of the diagnostic is not changed.

use super::LineColumn;

/// True for the characters that end a line. They have no glyph, so a range
/// that covers only one is not drawn either.
fn is_line_break(c: char) -> bool {
    c == '\n' || c == '\r'
}

impl LineColumn {
    /// The start and the end of the bytes `start..end` of `source`, as an
    /// editor can draw them.
    ///
    /// A range that holds bytes is the range as it is. An empty range is
    /// widened to the character after it, or, when a line ends or the text ends
    /// there, to the character before it (skipping over the line break, so that
    /// the end of a text that ends in a line break is drawn at the last
    /// character of the last line). A range that no character can widen to
    /// (an empty text, or one that holds only line breaks) stays empty.
    pub fn editor_range(source: &str, start: usize, end: usize) -> (LineColumn, LineColumn) {
        let (start, end) = widened(source, start, end);
        (
            LineColumn::from_offset(source, start),
            LineColumn::from_offset(source, end),
        )
    }
}

/// The byte range `start..end` of `source` with an empty range widened by one
/// character (see [`LineColumn::editor_range`]).
fn widened(source: &str, start: usize, end: usize) -> (usize, usize) {
    if start != end {
        return (start, end);
    }
    let mut at = start.min(source.len());
    while !source.is_char_boundary(at) {
        at -= 1;
    }
    let (before, after) = source.split_at(at);
    if let Some(next) = after.chars().next().filter(|c| !is_line_break(*c)) {
        return (at, at + next.len_utf8());
    }
    let visible = before.trim_end_matches(is_line_break);
    match visible.chars().next_back() {
        Some(previous) => (visible.len() - previous.len_utf8(), visible.len()),
        None => (at, at),
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn lc(line: u32, column: u32) -> LineColumn {
        LineColumn { line, column }
    }

    #[test]
    fn editor_range_when_range_has_bytes_then_unchanged() {
        assert_eq!(
            LineColumn::editor_range("abc\ndef", 1, 6),
            (lc(0, 1), lc(1, 2))
        );
    }

    #[test]
    fn editor_range_when_empty_mid_line_then_the_next_character() {
        assert_eq!(
            LineColumn::editor_range("abc\ndef", 1, 1),
            (lc(0, 1), lc(0, 2))
        );
    }

    #[test]
    fn editor_range_when_empty_at_start_of_text_then_the_first_character() {
        assert_eq!(LineColumn::editor_range("abc", 0, 0), (lc(0, 0), lc(0, 1)));
    }

    #[test]
    fn editor_range_when_empty_at_end_of_line_then_the_previous_character() {
        assert_eq!(
            LineColumn::editor_range("abc\ndef", 3, 3),
            (lc(0, 2), lc(0, 3))
        );
    }

    #[test]
    fn editor_range_when_empty_at_end_of_line_of_crlf_text_then_the_previous_character() {
        assert_eq!(
            LineColumn::editor_range("abc\r\ndef", 3, 3),
            (lc(0, 2), lc(0, 3))
        );
    }

    #[test]
    fn editor_range_when_empty_at_end_of_input_then_the_previous_character() {
        assert_eq!(
            LineColumn::editor_range("abc\ndef", 7, 7),
            (lc(1, 2), lc(1, 3))
        );
    }

    #[test]
    fn editor_range_when_empty_past_end_of_input_then_the_previous_character() {
        assert_eq!(
            LineColumn::editor_range("abc", 99, 99),
            (lc(0, 2), lc(0, 3))
        );
    }

    #[test]
    fn editor_range_when_empty_at_end_of_input_after_a_line_break_then_the_last_character_of_the_last_line(
    ) {
        assert_eq!(
            LineColumn::editor_range("abc\ndef\r\n", 9, 9),
            (lc(1, 2), lc(1, 3))
        );
    }

    #[test]
    fn editor_range_when_empty_text_then_empty() {
        assert_eq!(LineColumn::editor_range("", 0, 0), (lc(0, 0), lc(0, 0)));
    }

    #[test]
    fn editor_range_when_text_is_only_line_breaks_then_empty() {
        assert_eq!(LineColumn::editor_range("\n\n", 1, 1), (lc(1, 0), lc(1, 0)));
    }

    #[test]
    fn editor_range_when_empty_on_a_blank_line_between_lines_then_the_previous_visible_character() {
        assert_eq!(
            LineColumn::editor_range("ab\n\ncd", 3, 3),
            (lc(0, 1), lc(0, 2))
        );
    }

    #[test]
    fn editor_range_when_character_is_more_than_one_byte_then_one_column_wide() {
        // "é" is two bytes and one column.
        assert_eq!(LineColumn::editor_range("é=1", 0, 0), (lc(0, 0), lc(0, 1)));
        assert_eq!(LineColumn::editor_range("1=é", 4, 4), (lc(0, 2), lc(0, 3)));
    }

    #[test]
    fn editor_range_when_offset_inside_a_character_then_that_character() {
        assert_eq!(LineColumn::editor_range("é=1", 1, 1), (lc(0, 0), lc(0, 1)));
    }
}
