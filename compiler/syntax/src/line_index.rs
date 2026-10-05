//! The one mapping from a byte offset of a text to a line and a column.
//!
//! A line ends at a line break, and the forms of a line break are the data in
//! [`LINE_BREAKS`]: the lexer reads a `Newline` token with the same table, so
//! what the lexer calls a line break and what a position counts as one are one
//! decision. A column counts UTF-16 code units, the unit the language server
//! protocol uses by default.
//!
//! Text that is embedded in another document (the body of a PLCopen XML
//! element) does not start at the beginning of a line of its own: [`LineIndex`]
//! takes the line and the column at which the text starts, and the column
//! offset applies to the first line only, since every later line starts at
//! column 0 of the document.

/// The spellings of a line break, the longest first so that `\r\n` is one
/// break and not two.
///
/// A lone carriage return is a line break (a classic Mac line end), and so is
/// a form feed, which the legacy lexer also reads as one.
pub const LINE_BREAKS: &[&str] = &["\r\n", "\r", "\n", "\u{c}"];

/// The length in bytes of the line break that `text` starts with, or `None`
/// when it starts with none.
pub fn break_len(text: &str) -> Option<usize> {
    LINE_BREAKS
        .iter()
        .find(|spelling| text.starts_with(**spelling))
        .map(|spelling| spelling.len())
}

/// Where the lines of a text start, for turning byte offsets into positions.
#[derive(Debug, Clone)]
pub struct LineIndex<'a> {
    text: &'a str,
    /// The byte offset at which each line starts; the first is 0.
    starts: Vec<usize>,
    /// The line and the column of the first byte of the text.
    origin: (usize, usize),
}

impl<'a> LineIndex<'a> {
    /// The index of `text`, which starts at line `line_offset` and column
    /// `col_offset` (both 0-based) of the document it is part of.
    pub fn new(text: &'a str, line_offset: usize, col_offset: usize) -> Self {
        let mut starts = vec![0];
        let mut at = 0;
        let bytes = text.as_bytes();
        while let Some(byte) = bytes.get(at) {
            // Only a byte that a break starts with is worth matching the table
            // at; the others, which are nearly all of them, are skipped. A break
            // starts with an ASCII byte, so `get` only fails inside a multi-byte
            // character, and then no break starts there.
            let starts_break = LINE_BREAKS
                .iter()
                .any(|spelling| spelling.as_bytes().first() == Some(byte));
            match starts_break
                .then(|| text.get(at..).and_then(break_len))
                .flatten()
            {
                Some(len) => {
                    at += len;
                    starts.push(at);
                }
                None => at += 1,
            }
        }
        LineIndex {
            text,
            starts,
            origin: (line_offset, col_offset),
        }
    }

    /// The line and the column, both 0-based, of the byte at `offset`. An
    /// offset past the end of the text is the end of the text.
    ///
    /// Many positions of one text, in order, are cheaper through
    /// [`LineIndex::walker`]: this one counts the columns of its line from the
    /// start of the line every time.
    pub fn position(&self, offset: usize) -> (usize, usize) {
        self.walker().position(offset)
    }

    /// A reader of positions that remembers where the last one was, so that a
    /// run of positions that only move forward counts each character of the text
    /// once. A position that is before the last one is still right: it is found
    /// from the start of its line.
    pub fn walker(&self) -> Walker<'_, 'a> {
        Walker {
            index: self,
            line: 0,
            at: 0,
            column: 0,
        }
    }
}

/// The positions of one [`LineIndex`], read one after the other.
#[derive(Debug, Clone)]
pub struct Walker<'i, 'a> {
    index: &'i LineIndex<'a>,
    /// The line of `at`, and its column within the line, not counting the
    /// column offset of the first line.
    line: usize,
    at: usize,
    column: usize,
}

impl Walker<'_, '_> {
    /// The line and the column, both 0-based, of the byte at `offset`; see
    /// [`LineIndex::position`].
    pub fn position(&mut self, offset: usize) -> (usize, usize) {
        let index = self.index;
        let mut offset = offset.min(index.text.len());
        while !index.text.is_char_boundary(offset) {
            offset -= 1;
        }
        if offset < self.at {
            // Backwards: start again at the line the offset is on.
            self.line = 0;
            self.at = 0;
            self.column = 0;
        }
        // The last line that starts at or before the offset, looking forward
        // from the line the walker is on. A run of positions that move forward
        // mostly stay on the line or step to the next, which one comparison
        // settles; a longer step is a search.
        let ahead = index.starts.get(self.line..).unwrap_or_default();
        let stays = ahead.get(1).is_none_or(|next| *next > offset);
        let line = if stays {
            self.line
        } else {
            self.line
                + ahead
                    .partition_point(|start| *start <= offset)
                    .saturating_sub(1)
        };
        if line != self.line || self.at < index.starts.get(line).copied().unwrap_or(0) {
            self.line = line;
            self.at = index.starts.get(line).copied().unwrap_or(0);
            self.column = 0;
        }
        let passed = index.text.get(self.at..offset).unwrap_or_default();
        self.column += if passed.is_ascii() {
            // One unit a character: no need to decode.
            passed.len()
        } else {
            passed.chars().map(char::len_utf16).sum::<usize>()
        };
        self.at = offset;

        let (line_offset, col_offset) = index.origin;
        let column = if self.line == 0 {
            col_offset + self.column
        } else {
            self.column
        };
        (line_offset + self.line, column)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn at(text: &str, offset: usize) -> (usize, usize) {
        LineIndex::new(text, 0, 0).position(offset)
    }

    #[test]
    fn position_when_lf_then_the_line_starts_after_it() {
        assert_eq!(at("ab\ncd", 0), (0, 0));
        assert_eq!(at("ab\ncd", 2), (0, 2));
        assert_eq!(at("ab\ncd", 3), (1, 0));
        assert_eq!(at("ab\ncd", 4), (1, 1));
    }

    #[test]
    fn position_when_crlf_then_one_break() {
        assert_eq!(at("ab\r\ncd", 4), (1, 0));
        assert_eq!(at("ab\r\ncd", 5), (1, 1));
        assert_eq!(at("a\r\n\r\nb", 5), (2, 0));
    }

    #[test]
    fn position_when_lone_cr_then_a_break() {
        assert_eq!(at("ab\rcd", 3), (1, 0));
        assert_eq!(at("ab\r\rcd", 4), (2, 0));
    }

    #[test]
    fn position_when_form_feed_then_a_break() {
        assert_eq!(at("ab\u{c}cd", 3), (1, 0));
    }

    #[test]
    fn position_when_non_ascii_then_the_column_counts_utf16_units() {
        // `é` is one unit, the emoji is two.
        let text = "é\u{1F600}x\ny";
        assert_eq!(at(text, 0), (0, 0));
        assert_eq!(at(text, "é".len()), (0, 1));
        assert_eq!(at(text, "é\u{1F600}".len()), (0, 3));
        assert_eq!(at(text, text.len() - 1), (1, 0));
    }

    #[test]
    fn position_when_offset_past_the_end_then_the_end() {
        assert_eq!(at("ab\ncd", 99), (1, 2));
        assert_eq!(at("", 0), (0, 0));
        assert_eq!(at("ab\n", 3), (1, 0));
    }

    #[test]
    fn position_when_text_is_embedded_then_the_column_offset_applies_to_the_first_line_only() {
        let index = LineIndex::new("ab\ncd", 5, 7);
        assert_eq!(index.position(0), (5, 7));
        assert_eq!(index.position(2), (5, 9));
        assert_eq!(index.position(3), (6, 0));
        assert_eq!(index.position(4), (6, 1));
    }

    #[test]
    fn walker_when_offsets_come_in_any_order_then_the_same_positions_as_one_at_a_time() {
        let text = "ab\r\n\u{1F600}cd\ref\n\n\u{e9}x";
        let index = LineIndex::new(text, 3, 4);
        let mut offsets: Vec<usize> = (0..=text.len()).collect();
        let mut walker = index.walker();
        for offset in offsets.clone() {
            assert_eq!(walker.position(offset), index.position(offset), "{offset}");
        }
        offsets.reverse();
        for offset in offsets {
            assert_eq!(walker.position(offset), index.position(offset), "{offset}");
        }
    }

    #[test]
    fn walker_when_text_has_many_lines_then_every_offset_matches_a_count_by_characters() {
        // An oracle that reads the text one character at a time, to hold the
        // forward step, the search and the ASCII shortcut of the walker to it.
        let text = "ab\ncd\r\nef\r\u{e9}\u{1F600}x\n\n\n\u{c}tail\nlast";
        let index = LineIndex::new(text, 0, 0);
        let mut walker = index.walker();
        let mut line = 0;
        let mut column = 0;
        let mut previous = ' ';
        for (offset, ch) in text.char_indices() {
            // The `\n` of a `\r\n` is inside the break: it has no position.
            let inside_crlf = ch == '\n' && previous == '\r';
            if !inside_crlf {
                assert_eq!(walker.position(offset), (line, column), "{offset}");
            }
            let ends_line =
                matches!(ch, '\n' | '\u{c}') || (ch == '\r' && !text[offset..].starts_with("\r\n"));
            if ends_line {
                line += 1;
                column = 0;
            } else if ch != '\r' && !inside_crlf {
                column += ch.len_utf16();
            } else if inside_crlf {
                column = 0;
            }
            previous = ch;
        }
        assert_eq!(walker.position(text.len()), (line, column));
    }

    #[test]
    fn break_len_when_text_starts_with_a_break_then_its_length() {
        assert_eq!(break_len("\r\nx"), Some(2));
        assert_eq!(break_len("\rx"), Some(1));
        assert_eq!(break_len("\nx"), Some(1));
        assert_eq!(break_len("x\n"), None);
        assert_eq!(break_len(""), None);
    }

    #[test]
    fn linebreaks_when_listed_then_longer_spellings_come_first() {
        for (index, spelling) in LINE_BREAKS.iter().enumerate() {
            for later in &LINE_BREAKS[index + 1..] {
                assert!(!later.starts_with(spelling), "{spelling:?} hides {later:?}");
            }
        }
    }
}
