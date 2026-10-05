//! A byte cursor over the original source text.
//!
//! The cursor advances byte by byte, which is safe because every delimiter the
//! lexer looks for is ASCII and an ASCII byte never occurs inside a multi-byte
//! UTF-8 sequence. A token therefore always ends on a character boundary, with
//! the one exception of [`Cursor::bump_char`], which exists to step over a
//! whole character.

pub(super) struct Cursor<'a> {
    source: &'a str,
    pos: usize,
}

impl<'a> Cursor<'a> {
    pub(super) fn new(source: &'a str) -> Self {
        Cursor { source, pos: 0 }
    }

    pub(super) fn pos(&self) -> usize {
        self.pos
    }

    pub(super) fn set_pos(&mut self, pos: usize) {
        self.pos = pos.min(self.source.len());
    }

    pub(super) fn is_at_end(&self) -> bool {
        self.pos >= self.source.len()
    }

    pub(super) fn peek(&self) -> Option<u8> {
        self.peek_at(0)
    }

    pub(super) fn peek_at(&self, offset: usize) -> Option<u8> {
        self.source.as_bytes().get(self.pos + offset).copied()
    }

    /// Advances one byte; the caller guarantees it is ASCII or that a later
    /// step restores a character boundary.
    pub(super) fn bump(&mut self) {
        self.bump_n(1);
    }

    pub(super) fn bump_n(&mut self, count: usize) {
        self.pos = (self.pos + count).min(self.source.len());
    }

    /// Advances over one whole UTF-8 character.
    pub(super) fn bump_char(&mut self) {
        let width = match self.peek() {
            None => 0,
            Some(0x00..=0x7f) => 1,
            Some(0xc0..=0xdf) => 2,
            Some(0xe0..=0xef) => 3,
            Some(_) => 4,
        };
        self.bump_n(width);
    }

    /// The character starting at the cursor. Only meaningful on a boundary,
    /// which holds whenever a token has just ended.
    pub(super) fn peek_char(&self) -> Option<char> {
        self.source.get(self.pos..)?.chars().next()
    }

    pub(super) fn eat_while(&mut self, predicate: impl Fn(u8) -> bool) {
        while self.peek().is_some_and(&predicate) {
            self.bump();
        }
    }

    pub(super) fn starts_with(&self, text: &str) -> bool {
        self.source
            .as_bytes()
            .get(self.pos..)
            .is_some_and(|rest| rest.starts_with(text.as_bytes()))
    }

    /// The length of the line break at the cursor, if there is one.
    pub(super) fn line_break_len(&self) -> Option<usize> {
        self.source
            .get(self.pos..)
            .and_then(crate::line_index::break_len)
    }

    /// The source text from `start` to the cursor.
    pub(super) fn slice_from(&self, start: usize) -> &'a str {
        self.source.get(start..self.pos).unwrap_or_default()
    }
}
