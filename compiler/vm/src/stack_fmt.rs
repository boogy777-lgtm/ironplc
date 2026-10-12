//! A small stack-allocated buffer for formatting numbers as strings.

use core::fmt::{self, Display, Write};

/// Holds the decimal text of one number without heap allocation.
///
/// Used by CONV_I32_TO_STR, CONV_U32_TO_STR, and CONV_F32_TO_STR. 48 bytes is
/// enough for any i32, u32, or f32 decimal representation (the longest, the
/// smallest positive normal f32 negated, is 48 bytes), so [`StackFmtBuf::of`]
/// never has to cut a number.
pub(crate) struct StackFmtBuf {
    buf: [u8; 48],
    len: usize,
}

impl StackFmtBuf {
    /// The text of `value`.
    pub(crate) fn of(value: impl Display) -> Self {
        let mut text = Self {
            buf: [0u8; 48],
            len: 0,
        };
        // `write_str` below never returns an error: it copies what fits.
        #[expect(
            clippy::let_underscore_must_use,
            reason = "StackFmtBuf::write_str never returns Err, and Display for a number writes nothing else"
        )]
        let _ = write!(text, "{value}");
        text
    }

    pub(crate) fn as_bytes(&self) -> &[u8] {
        &self.buf[..self.len]
    }
}

impl Write for StackFmtBuf {
    fn write_str(&mut self, s: &str) -> fmt::Result {
        let bytes = s.as_bytes();
        let remaining = self.buf.len() - self.len;
        let to_copy = bytes.len().min(remaining);
        self.buf[self.len..self.len + to_copy].copy_from_slice(&bytes[..to_copy]);
        self.len += to_copy;
        Ok(())
    }
}
